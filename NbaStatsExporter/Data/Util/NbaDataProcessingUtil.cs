using NbaStatsExporter.Data.ApiModels;

namespace NbaStatsExporter.Data.Util
{
    public static class NbaDataProcessingUtil
    {
        // This method builds rosters year by year, using drafts and transfers.
        public static Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>> BuildHistoricalRosters(
            List<DraftData> drafts,
            List<TransferData> transfers,
            Dictionary<int, Dictionary<string, List<Player>>> currentRosters = null)
        {
            var currentRosterPerTeamAndYearData = TransformPlayerRosterDictionary(currentRosters, playersOnRoster => new PlayerOnTeamAndYear
            {
                Id = playersOnRoster.Id,
                Position = playersOnRoster.Position,
                PrimaryPosition = playersOnRoster.PrimaryPosition,
                Reference = playersOnRoster.Reference,
                SrId = playersOnRoster.SrId,
                // Team = playerOnRoster.Team,
                Height = playersOnRoster.Height,
                Weight = playersOnRoster.Weight,
                FullName = playersOnRoster.FullName,
                Experience = playersOnRoster.Experience,
                JerseyNumber = playersOnRoster.JerseyNumber,
                Year = 2025
            });

            var rosters = currentRosterPerTeamAndYearData ?? new Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>>();

            // Iterate over each draft and apply to rosters for that year.
            foreach (var draftData in drafts)
            {
                foreach (var round in draftData.Rounds)
                {
                    foreach (var pick in round.Picks)
                    {
                        AddPlayerToRoster(rosters, pick.Prospect, pick.Team.Name, draftData.Draft.Year);
                    }
                }
            }

            // Apply trades to update rosters for that year.
            foreach (var transfer in transfers)
                foreach (var trade in transfer.Trades)
                    foreach (var transaction in trade.Transactions.Where(transaction => transaction.FromTeam != null && transaction.ToTeam != null))
                        foreach (var item in transaction.Items)
                        {
                            if (item.Pick != null)
                            {
                                AddPlayerToRoster(rosters, item.Pick.Prospect, transaction.ToTeam.Name, transfer.Draft.Year);
                            }
                        }

            return rosters;
        }

        private static void AddPlayerToRoster(Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>> rosters, Prospect player, string teamName, int year)
        {
            if (!rosters.ContainsKey(year))
            {
                rosters[year] = new Dictionary<string, List<PlayerOnTeamAndYear>>();
            }

            if (!rosters[year].ContainsKey(teamName))
            {
                rosters[year][teamName] = new List<PlayerOnTeamAndYear>();
            }

            rosters[year][teamName].Add(new PlayerOnTeamAndYear
            {
                Id = player.Id,
                FullName = player.Name,
                Team = teamName,
                Year = year
            });
        }

        private static Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>> TransformPlayerRosterDictionary(
    Dictionary<int, Dictionary<string, List<Player>>> inputDictionary,
    Func<Player, PlayerOnTeamAndYear> transformFunc)
        {
            var transformedDictionary = new Dictionary<int, Dictionary<string, List<PlayerOnTeamAndYear>>>();

            foreach (var yearEntry in inputDictionary)
            {
                int year = yearEntry.Key;
                var teamData = yearEntry.Value;

                // Prepare a dictionary for the transformed year
                var transformedTeamData = new Dictionary<string, List<PlayerOnTeamAndYear>>();

                foreach (var teamEntry in teamData)
                {
                    string teamName = teamEntry.Key;
                    var playersForTeam = new List<PlayerOnTeamAndYear>();
                    foreach (var playerEntry in teamEntry.Value)
                    {
                        Player item = playerEntry;
                        PlayerOnTeamAndYear transformedItem = transformFunc(item);
                        playersForTeam.Add(transformedItem);
                    }
                    // Add the transformed item to the dictionary
                    transformedTeamData[teamName] = playersForTeam;
                }

                // Add the transformed team data to the transformed dictionary for the current year
                transformedDictionary[year] = transformedTeamData;
            }

            return transformedDictionary;
        }
    }
}
