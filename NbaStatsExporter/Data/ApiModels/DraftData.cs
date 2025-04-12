namespace NbaStatsExporter.Data.ApiModels
{
    using System;
    using System.Collections.Generic;

    public class DraftData
    {
        public Draft Draft { get; set; }
        public Team Team { get; set; }
        public List<Round> Rounds { get; set; }
    }

    public class Draft
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public League League { get; set; }
        public Broadcast Broadcast { get; set; }
        public Venue Venue { get; set; }
    }

    public class Broadcast
    {
        public string Channel { get; set; }
        public string Network { get; set; }
        public string Internet { get; set; }
    }

    public class Venue
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class Round
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public List<Pick> Picks { get; set; }
    }

    public class Pick
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public int Overall { get; set; }
        public bool Traded { get; set; }
        public bool Supplemental { get; set; }
        public bool Compensatory { get; set; }
        public Team Team { get; set; }
        public Prospect Prospect { get; set; }
        public List<Trade> Trades { get; set; }
    }

    public class Prospect
    {
        public Guid Id { get; set; }
        public Guid SourceId { get; set; }
        public Guid LeagueId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string TeamName { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Experience { get; set; }
        public string BirthPlace { get; set; }
        public bool TopProspect { get; set; }
    }
}
