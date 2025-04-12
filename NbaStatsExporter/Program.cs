using NbaStatsExporter.Data;
using NbaStatsExporter.Data.ApiExtractors;
using NbaStatsExporter.Data.ApiModels;
using NbaStatsExporter.Data.ExporterUtil;
using NbaStatsExporter.Http;

using static NbaStatsExporter.Data.Enums.Enums;
using static NbaStatsExporter.Http.NbaApiService;

class Program
{
    static void Main(string[] args)
    {
        DateTime _lastSeasonDate = new DateTime(2024, 01, 01); // Looks like API has not been updated with games/matches for current season

        var configDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString();
        var config = FileExporter.Read<LocalConfig>(configDir, "config");
        config.NbaApiKey = LocalConfig.GetFromEnvironmentVariables(config.NbaApiKey);
        config.AwsAccessKey = LocalConfig.GetFromEnvironmentVariables(config.AwsAccessKey);
        config.AwsSecretKey = LocalConfig.GetFromEnvironmentVariables(config.AwsSecretKey);
        var outputDir = config.ApiOutputDirectory;

        var apiExtractor = new ApiDataExtractor(new NbaApiService(), config.NbaApiKey);

        #region Collect data from NBA API

        var seasonData = ReadOrExtractSeasonData(apiExtractor, outputDir);
        var teams = ReadOrExtractTeams(apiExtractor, _lastSeasonDate.Year, outputDir);

        var rosters = ReadOrExtractRosters(apiExtractor, outputDir, teams);
        var players = ReadOrExtractPlayers(apiExtractor, outputDir, rosters);

        var minimalDraftSeason = 2019; // Looks like the API has drafts starting from 2019 onward, despite players having rookie seasons back to 2003
        var years = Enumerable.Range(minimalDraftSeason, _lastSeasonDate.Year - minimalDraftSeason + 1).ToList();

        var draftData = ReadOrExtractDraftData(apiExtractor, outputDir, years, teams);
        var transferData = ReadOrExtractTransferData(apiExtractor, outputDir, years, teams);

        var schedulesData = ReadOrExtractSeasonSchedulesData(apiExtractor, years, outputDir);

        // Extract data for playoff (and play-in ?) games before getting it for regular seasons
        Console.WriteLine("Reading or Extracting data for Games ...");
        foreach (var seasonYear in schedulesData.Keys)
        {
            var seasonPSTMatches = schedulesData.FirstOrDefault(s => s.Key == seasonYear).Value
                .Where(sd => sd.Key == GameType.PST.ToString()).SelectMany(gs => gs.Value).ToList();

            var seasonIdentifier = String.Format("{0}-{1}-{2}", "schedules", seasonYear, GameType.PST.ToString());
            ReadOrExtractGameplayDataForSeason(apiExtractor, seasonIdentifier, seasonPSTMatches, outputDir);
        }

        foreach (var seasonYear in schedulesData.Keys)
        {
            var seasonREGPSTMatches = schedulesData.FirstOrDefault(s => s.Key == seasonYear).Value
                .Where(sd => sd.Key == GameType.REG.ToString()).SelectMany(gs => gs.Value).ToList();

            var seasonIdentifier = String.Format("{0}-{1}-{2}", "schedules", seasonYear, GameType.REG.ToString());
            ReadOrExtractGameplayDataForSeason(apiExtractor, seasonIdentifier, seasonREGPSTMatches, outputDir);
        }

        #endregion

        Console.WriteLine("Building historical Roster data from drafts and transfers ...");
        var historicalRosters = apiExtractor.ProcessHistoricalRosterData(years, rosters, teams, draftData, transferData);
        foreach (var yearlyRoster in historicalRosters)
            foreach (var team in yearlyRoster.Value)
                FileExporter.WriteToJsonFile("roster", team.Value, Path.Combine(yearlyRoster.Key.ToString(), team.Key).ToString());
        FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "RosterData", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });

        Console.WriteLine("Copying extracted API data to outer folder ...");
        FileExporter.CopyFolder(config.ApiOutputDirectory, config.CopyOutputDirectory);

        #region Upload data to S3

        Console.WriteLine("Uploading accumulated data to S3 ...");

        var canAccessS3 = S3FileExporter.CanAccessS3Async(config.AwsAccessKey, config.AwsSecretKey, config.S3BucketName).GetAwaiter().GetResult();
        var awsCredentials = new NbaS3Config().GetAWSCredentials(config.AwsAccessKey, config.AwsSecretKey);
        var s3Client = new NbaS3Config().GetS3Client(awsCredentials);
        var s3exporter = new S3FileExporter(awsCredentials, config.S3BucketName);

        var s3Accessible = s3exporter.EnsureAwsCredentialsAndBucketAccessible().GetAwaiter().GetResult();
        var bucketExists = s3exporter.EnsureBucketExistsAsync().GetAwaiter().GetResult();

        if (s3Accessible && bucketExists)
        {
            var (successCount, failCount) = s3exporter.UploadDirectoryToS3Async(config.CopyOutputDirectory).GetAwaiter().GetResult();
        }

        #endregion

        Console.ReadLine();
    }

    #region Seasons, Teams, Rosters

    private static SeasonSummaryData ReadOrExtractSeasonData(ApiDataExtractor apiExtractor, string outputDir)
    {
        Console.WriteLine("Reading or Extracting Season data ...");
        var seasonData = FileExporter.Read<SeasonSummaryData>(outputDir, "seasonData");
        if (seasonData == null)
        {
            seasonData = apiExtractor.ExtractSeasonData();
            FileExporter.WriteToJsonFile("seasonData", seasonData);
            FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Seasons", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });
        }

        return seasonData;
    }

    private static List<Team> ReadOrExtractTeams(ApiDataExtractor apiExtractor, int seasonYear, string outputDir)
    {
        Console.WriteLine("Reading or Extracting Teams ...");
        var teams = FileExporter.Read<List<Team>>(outputDir, "teamData"); ;
        if (teams == null)
        {
            var teamData = apiExtractor.ExtractTeams(seasonYear);
            teams = teamData.Teams.ToList();
            FileExporter.WriteToJsonFile("teamData", teams);
            FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Teams", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });
        }

        return teams;
    }

    private static List<RosterData> ReadOrExtractRosters(ApiDataExtractor apiExtractor, string outputDir, List<Team> teams)
    {
        Console.WriteLine("Reading or Extracting Rosters ...");
        var rosters = FileExporter.Read<List<RosterData>>(outputDir, "rosters");
        if (rosters == null)
        {
            rosters = apiExtractor.ExtractRosters(teams);
            FileExporter.WriteToJsonFile("rosters", rosters);
            FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Rosters", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });
        }

        return rosters;
    }

    private static List<Player> ReadOrExtractPlayers(ApiDataExtractor apiExtractor, string outputDir, List<RosterData> rosters)
    {
        var players = rosters != null ? rosters.Where(roster => roster != null).SelectMany(roster => roster.Players).ToList() : new List<Player>();
        FileExporter.WriteToJsonFile("players", players);
        FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Players", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });

        return players;
    }

    private static List<DraftData> ReadOrExtractDraftData(ApiDataExtractor apiExtractor, string outputDir, List<int> years, List<Team> teams)
    {
        Console.WriteLine("Reading or Extracting Drafts ...");
        var draftData = FileExporter.Read<List<DraftData>>(outputDir, "draftData");
        if (draftData == null)
        {
            draftData = apiExtractor.ExtractDraftData(years, teams);
            FileExporter.WriteToJsonFile("draftData", draftData);
            FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Drafts", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });
        }

        return draftData;
    }

    private static List<TransferData> ReadOrExtractTransferData(ApiDataExtractor apiExtractor, string outputDir, List<int> years, List<Team> teams)
    {
        Console.WriteLine("Reading or Extracting Transfers ...");
        var transferData = FileExporter.Read<List<TransferData>>(outputDir, "transferData");
        if (transferData == null)
        {
            transferData = apiExtractor.ExtractTransferData(years, teams);
            FileExporter.WriteToJsonFile("transferData", transferData);
            FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Transfers", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });
        }

        return transferData;
    }

    private static Dictionary<int, Dictionary<string, List<Match>>> ReadOrExtractSeasonSchedulesData(ApiDataExtractor apiExtractor, List<int> years, string outputDir)
    {
        Console.WriteLine("Reading or Extracting Schedules ... ");
        var scheduleData = new Dictionary<int, Dictionary<string, List<Match>>>();
        foreach (var scheduleYear in years)
        {
            foreach (var gameType in Enum.GetValues<GameType>())
            {
                var seasonIdentifier = String.Format("{0}-{1}-{2}", "schedules", scheduleYear, gameType);
                Console.WriteLine(seasonIdentifier);
                var scheduleExistingData = FileExporter.Read<List<Match>>(outputDir, seasonIdentifier);

                if (scheduleExistingData == null)
                {
                    var seasonSchedule = apiExtractor.ExtractSeasonSchedulesData(scheduleYear, gameType);
                    FileExporter.WriteToJsonFile(seasonIdentifier, seasonSchedule);

                    scheduleData[scheduleYear] = new Dictionary<string, List<Match>> { { gameType.ToString(), seasonSchedule } };
                }
                else scheduleData[scheduleYear] = new Dictionary<string, List<Match>> { { gameType.ToString(), scheduleExistingData } };
            }
        }
        FileExporter.WriteToJsonFile("logStatistics", new ApiRequestStatistics { Phase = "Schedules", Statistics = apiExtractor.GetNbaApiRequesterStatistics() });

        return scheduleData;
    }

    public class ApiRequestStatistics
    {
        public string Phase { get; set; }
        public RequestStatistics Statistics { get; set; }
    }

    #endregion

    #region Game Data

    private static void ReadOrExtractGameplayDataForSeason(ApiDataExtractor apiExtractor, string seasonIdentifier, List<Match> matches, string outputDir)
    {
        foreach (var game in matches.OrderBy(match => match.Scheduled))
        {
            var gameIdentifier = $"{game.Home.Alias} - {game.Away.Alias} on {game.Scheduled.ToString("yyyy-MM-dd")}";
            Console.WriteLine(gameIdentifier);

            ReadOrExtractGamePlayByPlayData(apiExtractor, outputDir, seasonIdentifier, gameIdentifier, game.Id);

            // Let's not care about Box-Scores now (these are not complete, will prefer to parse in-game data to create the box-scores, if possible)
            //var boxScoresGameIdentifier = "box-scores " + gameIdentifier;
            //ReadOrExtractBoxScores(outputDir, seasonIdentifier, boxScoresGameIdentifier, game.Id);
        }
    }

    private static void ReadOrExtractGamePlayByPlayData(ApiDataExtractor dataExtractor, string outputDir, string gameGroupIdentifier, string gameIdentifier, string gameId)
    {
        var gameData = FileExporter.ReadString(Path.Combine(outputDir, gameGroupIdentifier), gameIdentifier);

        if (gameData == null)
        {
            gameData = dataExtractor.ExtractGamePlayByPlayData(gameId);
            FileExporter.WriteToFile(gameIdentifier, gameData, Path.Combine(outputDir, gameGroupIdentifier));
        }
        else Console.WriteLine("Data for game: " + gameIdentifier + " already exists");
    }

    #endregion

}
