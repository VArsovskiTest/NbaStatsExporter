using Newtonsoft.Json;

namespace NbaStatsExporter.Data.ApiModels
{
    public class RosterData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Founded { get; set; }

        [JsonProperty("sr_id")]
        public string SrId { get; set; }

        [JsonProperty("playoff_appearances")]
        public int PlayoffAppearances { get; set; }

        public string Reference { get; set; }

        public League League { get; set; }

        public List<Player> Players { get; set; }
    }

    public class Player
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        //[JsonProperty("status")]
        //public string Status { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("primary_position")]
        public string PrimaryPosition { get; set; }

        [JsonProperty("jersey_number")]
        public string JerseyNumber { get; set; }

        [JsonProperty("experience")]
        public string Experience { get; set; }

        [JsonProperty("sr_id")]
        public string SrId { get; set; }

        [JsonProperty("rookie_year")]
        public int RookieYear { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class PlayerOnTeamAndYear : Player {
        public int Year { get; set; }
        public string Team { get; set; }
    }
}
