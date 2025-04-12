using System.Text.Json.Serialization;

namespace NbaStatsExporter.Data.ApiModels
{
    public class SeasonSummaryData
    {
        public League? League { get; set; }
        public List<Season> Seasons { get; set; }
    }

    public class SeasonData
    {
        public League? League { get; set; }
        public Season Season { get; set; }
        public List<Match> Games { get; set; }
    }

    public class League {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Alias { get; set; }
    }

    public class Season
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }  // Nullable to handle missing end_date

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("type")]
        public SeasonType Type { get; set; }
    }

    public class SeasonType
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
