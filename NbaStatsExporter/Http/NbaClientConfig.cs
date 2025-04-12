namespace NbaStatsExporter.Data
{
    public class NbaClientConfig
    {
        private string _apiKey;

        public NbaClientConfig(string apiKey)
        {
            _apiKey = apiKey;
        }

        public HttpClient GetV8Client()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.sportradar.com/nba/trial/v8/")
            };

            client.DefaultRequestHeaders.Add("api_key", _apiKey);
            return client;
        }

        public HttpClient GetV1DraftClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.sportradar.com/draft/nba/trial/v1/")
            };

            client.DefaultRequestHeaders.Add("api_key", _apiKey);
            return client;
        }
    }
}
