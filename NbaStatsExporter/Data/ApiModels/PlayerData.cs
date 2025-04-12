namespace NbaStatsExporter.Data.ApiModels
{
    public class PlayerData
    {
        public Player Player { get; set; }
        public Team Team { get; set; }
        public int Year { get; set; }
        public int RegAttendances { get; set; }
        public int PstAttendances { get; set; }
    }
}
