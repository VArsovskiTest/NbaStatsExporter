using System.Text.Json.Serialization;

namespace NbaStatsExporter.Data.ApiModels
{
    public class TeamData
    {
        [JsonPropertyName("league")]
        public League League { get; set; }

        [JsonPropertyName("teams")]
        public List<Team> Teams { get; set; }
    }

    public class Team
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        [JsonPropertyName("market")]
        public string Market { get; set; }
    }
}
