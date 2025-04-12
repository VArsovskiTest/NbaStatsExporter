namespace NbaStatsExporter.Data.ApiModels
{
    public class PlayerOrTeamStatsData
    {
        public decimal Minutes { get; set; }
        public int FieldGoalsMade { get; set; }
        public int FieldGoalsAttempted { get; set; }
        public decimal FieldGoalsPercentage { get; set; }
        public int ThreePointsMade { get; set; }
        public int ThreePointsAttempted { get; set; }
        public decimal ThreePointsPercentage { get; set; }
        public int TwoPointsMade { get; set; }
        public int TwoPointsAttempted { get; set; }
        public decimal TwoPointsPercentage { get; set; }
        public int BlockedAttempts { get; set; }
        public int FreeThrowsMade { get; set; }
        public int FreeThrowsAttempted { get; set; }
        public decimal FreeThrowsPercentage { get; set; }
        public int OffensiveRebounds { get; set; }
        public int DefensiveRebounds { get; set; }
        public int Rebounds { get; set; }
        public int Assists { get; set; }
        public int Turnovers { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public decimal AssistsTurnoverRatio { get; set; }
        public int PersonalFouls { get; set; }
        public int TechnicalFouls { get; set; }
        public int FlagrantFouls { get; set; }
        public int PlusMinus { get; set; }
        public int Points { get; set; }
        public bool DoubleDouble { get; set; }
        public bool TripleDouble { get; set; }
        public decimal EffectiveFieldGoalPercentage { get; set; }
        public int Efficiency { get; set; }
        public decimal EfficiencyGameScore { get; set; }
        public int FoulsDrawn { get; set; }
        public int OffensiveFouls { get; set; }
        public int PointsInPaint { get; set; }
        public int PointsInPaintAttempted { get; set; }
        public int PointsInPaintMade { get; set; }
        public decimal PointsInPaintPercentage { get; set; }
        public int PointsOffTurnovers { get; set; }
        public decimal TrueShootingAttempts { get; set; }
        public decimal TrueShootingPercentage { get; set; }
        public int CoachEjections { get; set; }
        public int CoachTechnicalFouls { get; set; }
        public int SecondChancePoints { get; set; }
        public decimal SecondChancePercentage { get; set; }
    }
}
