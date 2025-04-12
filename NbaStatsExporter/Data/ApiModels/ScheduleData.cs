using Newtonsoft.Json;

namespace NbaStatsExporter.Data.ApiModels
{
    public class ScheduleData
    {
        public League? League { get; set; }
        public SeasonData? Season { get; set; }
        public List<Match> Games { get; set; } = new List<Match>();
    }

    public class Match
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Coverage { get; set; }
        public DateTime Scheduled { get; set; }
        [JsonProperty("home_points")]
        public int HomePoints { get; set; }
        [JsonProperty("away_points")]
        public int AwayPoints { get; set; }
        [JsonProperty("sr_id")]
        public string SrId { get; set; }
        public string Reference { get; set; }
        public TeamVisit Home { get; set; }
        public TeamVisit Away { get; set; }
    }

    public class TeamVisit
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Id { get; set; }
        [JsonProperty("sr_id")]
        public string SrId { get; set; }
        public string Reference { get; set; }
    }
}
