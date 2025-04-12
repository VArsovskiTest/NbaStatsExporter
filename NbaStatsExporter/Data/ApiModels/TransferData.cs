namespace NbaStatsExporter.Data.ApiModels
{
    public class TransferData
    {
        public Draft Draft { get; set; }
        public List<Trade> Trades { get; set; } = new List<Trade>();
        public string Comment { get; set; }
    }

    public class Trade
    {
        public string Id { get; set; }
        public long Sequence { get; set; }
        public bool Complete { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class Transaction
    {
        public string Id { get; set; }
        public Team ToTeam { get; set; }
        public Team FromTeam { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }

    public class Item
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public Pick Pick { get; set; }
        public FuturePick FuturePick { get; set; }
    }

    public class FuturePick
    {
        public string Year { get; set; }
        public string Round { get; set; }
    }
}
