using NbaStatsExporter.Data.ApiModels;
using NbaStatsExporter.Data.Util;
using NbaStatsExporter.Http;
using static NbaStatsExporter.Data.Enums.Enums;
using static NbaStatsExporter.Http.NbaApiService;

namespace NbaStatsExporter.Data.ApiExtractors
{
    public class ApiDataExtractor
    {
        private NbaApiService _nbaClientRequester;
        private string _apiKey;

        public ApiDataExtractor(NbaApiService nbaClientRequester, string apiKey)
        {
            _nbaClientRequester = nbaClientRequester;
            _apiKey = apiKey;
        }

        public SeasonSummaryData ExtractSeasonData()
        {
            var seasonData = new SeasonSummaryData();

            using (var client = new NbaClientConfig(_apiKey).GetV8Client())
            {
                seasonData = _nbaClientRequester.SeasonData(client).GetAwaiter().GetResult();
            }

            return seasonData;
        }

        public TeamData ExtractTeams(int lastSeasonYear)
        {
            var teamData = new TeamData();
            using (var client = new NbaClientConfig(_apiKey).GetV8Client())
            {
                teamData = _nbaClientRequester.TeamData(client).GetAwaiter().GetResult();
            }

            // Remove excess/ former teams from teamData and save it
            using (var client = new NbaClientConfig(_apiKey).GetV8Client())
            {
                var scheduleLastYear = _nbaClientRequester.ScheduleData(client, lastSeasonYear, "REG").GetAwaiter().GetResult();

                var activeTeams = teamData.Teams.Where(team => scheduleLastYear
                    .Games.Select(game => game.Home.Alias).Distinct().ToList().Contains(team.Alias)).ToList();
                teamData.Teams = activeTeams;
            }

            return teamData;
        }

        public List<RosterData> ExtractRosters(List<Team> teamData)
        {
            var rosters = new List<RosterData>();
            foreach (var team in teamData)
            {
                var rosterData = new RosterData();
                using (var client = new NbaClientConfig(_apiKey).GetV8Client())
                {
                    rosterData = _nbaClientRequester.RosterData(client, team.Id.ToString()).GetAwaiter().GetResult();
                    rosters.Add(rosterData);
                }
            }

            return rosters;
        }

        public List<Match> ExtractSeasonSchedulesData(int year, GameType gameType)
        {
            var scheduleData = new ScheduleData();
            var seasonIdentifier = year+ " - " + gameType;
            using (var client = new NbaClientConfig(_apiKey).GetV8Client())
            {
                Console.WriteLine("Schedules for season " + seasonIdentifier + " ...");
                scheduleData = _nbaClientRequester.ScheduleData(client, year, gameType.ToString()).GetAwaiter().GetResult();
            }

            return scheduleData.Games;
        }

        public List<DraftData> ExtractDraftData(List<int> years, List<Team> teams)
        {
            var draftData = new List<DraftData>();
            using (var client = new NbaClientConfig(_apiKey).GetV1DraftClient())
            {
                Console.WriteLine("Extracting data for Drafts ...");
                draftData = _nbaClientRequester.DraftData(client, years, teams).GetAwaiter().GetResult();
            }

            return draftData;
        }

        public List<TransferData> ExtractTransferData(List<int> years, List<Team> teams)
        {
            var transferData = new List<TransferData>();

            using (var client = new NbaClientConfig(_apiKey).GetV1DraftClient())
            {
                Console.WriteLine("Extracting data for Transfers ...");
                transferData = _nbaClientRequester.TransferData(client, years).GetAwaiter().GetResult();
            }

            return transferData;
        }

        public string ExtractGamePlayByPlayData(string gameId)
        {
            var gamePlayByPlayData = string.Empty;

            using (var client = new NbaClientConfig(_apiKey).GetV8Client())
            {
                gamePlayByPlayData = _nbaClientRequester.GamePlayByPlay(client, gameId).GetAwaiter().GetResult();
            }

            return gamePlayByPlayData;
        }

        public string ExtractBoxScores(string gameId)
        {
            var boxScores = string.Empty;

            using (var client = new NbaClientConfig(_apiKey).GetV1DraftClient())
            {
                boxScores = _nbaClientRequester.BoxScore(client, gameId).GetAwaiter().GetResult();
            }

            return boxScores;
        }

        public Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>> ProcessHistoricalRosterData(List<int> years, List<RosterData> rosters, List<Team> teams, List<DraftData> draftData, List<TransferData> transferData)
        {
            var currentRosters = rosters.Where(roster => roster != null).ToDictionary(roster => roster.Name, roster => roster.Players);
            var rostersByYear = new Dictionary<int, Dictionary<string, List<Player>>> { { 2025, currentRosters } };
            return NbaDataProcessingUtil.BuildHistoricalRosters(draftData, transferData, rostersByYear);
        }

        public RequestStatistics GetNbaApiRequesterStatistics()
        {
            return _nbaClientRequester.GetRequestStatistics();
        }
    }
}
