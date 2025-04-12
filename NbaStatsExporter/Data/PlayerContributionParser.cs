// var results = PlayerContributionParser.ExtractWithTimeAndPossessions(gameData, playerIdToName);

// var sb = new StringBuilder();
// sb.AppendLine("Player,OpposingLineup,SecondsPlayed,Possessions,PointsScored,PointsConceded,PlusMinus");

// foreach (var (playerId, records) in results)
// {
    // string name = playerIdToName.GetValueOrDefault(playerId, playerId);
    // foreach (var r in records)
    // {
        // sb.AppendLine($"{name},\"{string.Join(" | ", r.OpposingLineup)}\"," +
                      // $"{r.SecondsPlayed},{r.Possessions},{r.PointsScored},{r.PointsConceded},{r.PlusMinus}");
    // }
// }

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerContributionParser
{
    public class LineupContribution
    {
        public List<string> OpposingLineup { get; set; } = new();
        public int SecondsPlayed { get; set; } = 0;
        public int Possessions { get; set; } = 0;
        public int PointsScored { get; set; } = 0;
        public int PointsConceded { get; set; } = 0;
        public int PlusMinus => PointsScored - PointsConceded;
    }

    private static int ClockToSeconds(string clock)
    {
        if (string.IsNullOrEmpty(clock)) return -1;
        var parts = clock.Split(':');
        return parts.Length == 2 && int.TryParse(parts[0], out int m) && int.TryParse(parts[1], out int s) ? m * 60 + s : -1;
    }

    private static string GetTeamSide(JObject data, string teamName)
    {
        return data["home"]?["name"]?.ToString() == teamName ? "home" : "away";
    }

    public static Dictionary<string, List<LineupContribution>> ExtractWithTimeAndPossessions(
        JObject data,
        Dictionary<string, string> playerIdToName
    )
    {
        var lineupByTime = new Dictionary<(int, int), Dictionary<string, List<string>>>();
        var possessionTimeline = new List<((int Period, int Clock), string Team, List<string> Home, List<string> Away)>();

        // Build lineup timeline and mark possessions
        foreach (var period in data["periods"])
        {
            int periodNum = period.Value<int>("number");
            foreach (var ev in period["events"])
            {
                int clock = ClockToSeconds(ev.Value<string>("clock"));
                if (clock < 0) continue;

                var home = ev["on_court"]?["home"]?["players"]?
                    .Select(p => p?["id"]?.ToString()).Where(id => !string.IsNullOrEmpty(id)).ToList() ?? new();
                var away = ev["on_court"]?["away"]?["players"]?
                    .Select(p => p?["id"]?.ToString()).Where(id => !string.IsNullOrEmpty(id)).ToList() ?? new();

                lineupByTime[(periodNum, clock)] = new Dictionary<string, List<string>> {
                    ["home"] = home,
                    ["away"] = away
                };

                foreach (var stat in ev["statistics"] ?? new JArray())
                {
                    var type = stat["type"]?.ToString();
                    if (type is "fieldgoal" or "turnover" or "freethrow")
                    {
                        string team = stat["team"]?["name"]?.ToString();
                        if (!string.IsNullOrEmpty(team))
                            possessionTimeline.Add(((periodNum, clock), team, home, away));
                        break;
                    }
                }
            }
        }

        var sortedTimes = lineupByTime.Keys.OrderBy(k => k.Item1).ThenByDescending(k => k.Item2).ToList();
        var playerLineupMap = new Dictionary<string, Dictionary<string, LineupContribution>>();

        // Time tracking
        for (int i = 0; i < sortedTimes.Count - 1; i++)
        {
            var (period, clock) = sortedTimes[i];
            var (nextPeriod, nextClock) = sortedTimes[i + 1];
            if (period != nextPeriod) continue;

            int delta = Math.Abs(clock - nextClock);
            var currentLineups = lineupByTime[(period, clock)];

            foreach (var side in new[] { "home", "away" })
            {
                var players = currentLineups[side];
                var opponentLineup = currentLineups[side == "home" ? "away" : "home"];
                string lineupKey = string.Join(" | ", opponentLineup.OrderBy(id => id));

                foreach (var pid in players)
                {
                    if (!playerLineupMap.ContainsKey(pid))
                        playerLineupMap[pid] = new();
                    if (!playerLineupMap[pid].ContainsKey(lineupKey))
                        playerLineupMap[pid][lineupKey] = new LineupContribution
                        {
                            OpposingLineup = opponentLineup.Select(id => playerIdToName.GetValueOrDefault(id, id)).ToList()
                        };

                    playerLineupMap[pid][lineupKey].SecondsPlayed += delta;
                }
            }
        }

        // Points scored/conceded
        foreach (var period in data["periods"])
        {
            int periodNum = period.Value<int>("number");
            foreach (var ev in period["events"])
            {
                int clock = ClockToSeconds(ev.Value<string>("clock"));
                if (clock < 0) continue;

                var lineups = lineupByTime.GetValueOrDefault((periodNum, clock));
                if (lineups == null) continue;

                foreach (var stat in ev["statistics"] ?? new JArray())
                {
                    if (stat["type"]?.ToString() is not ("fieldgoal" or "freethrow")) continue;

                    string playerId = stat["player"]?["id"]?.ToString();
                    string team = stat["team"]?["name"]?.ToString();
                    int points = stat.Value<int?>("points") ?? 0;
                    if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(team)) continue;

                    string teamSide = GetTeamSide(data, team);
                    string oppSide = teamSide == "home" ? "away" : "home";

                    var scoringLineup = lineups[teamSide];
                    var defendingLineup = lineups[oppSide];
                    string oppKey = string.Join(" | ", defendingLineup.OrderBy(id => id));
                    string scoreKey = string.Join(" | ", scoringLineup.OrderBy(id => id));

                    if (!playerLineupMap.ContainsKey(playerId))
                        playerLineupMap[playerId] = new();
                    if (!playerLineupMap[playerId].ContainsKey(oppKey))
                        playerLineupMap[playerId][oppKey] = new LineupContribution
                        {
                            OpposingLineup = defendingLineup.Select(id => playerIdToName.GetValueOrDefault(id, id)).ToList()
                        };
                    playerLineupMap[playerId][oppKey].PointsScored += points;

                    foreach (var defenderId in defendingLineup)
                    {
                        if (!playerLineupMap.ContainsKey(defenderId))
                            playerLineupMap[defenderId] = new();
                        if (!playerLineupMap[defenderId].ContainsKey(scoreKey))
                            playerLineupMap[defenderId][scoreKey] = new LineupContribution
                            {
                                OpposingLineup = scoringLineup.Select(id => playerIdToName.GetValueOrDefault(id, id)).ToList()
                            };
                        playerLineupMap[defenderId][scoreKey].PointsConceded += points;
                    }
                }
            }
        }

        // Possession tracking
        foreach (var ((period, clock), teamName, home, away) in possessionTimeline)
        {
            string teamSide = GetTeamSide(data, teamName);
            string oppSide = teamSide == "home" ? "away" : "home";
            var offense = teamSide == "home" ? home : away;
            var defense = teamSide == "home" ? away : home;

            string lineupKey = string.Join(" | ", defense.OrderBy(id => id));

            foreach (var playerId in offense)
            {
                if (!playerLineupMap.ContainsKey(playerId)) continue;
                if (!playerLineupMap[playerId].ContainsKey(lineupKey)) continue;
                playerLineupMap[playerId][lineupKey].Possessions++;
            }
        }

        return playerLineupMap.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Values.OrderByDescending(c => c.SecondsPlayed).ToList()
        );
    }
}
