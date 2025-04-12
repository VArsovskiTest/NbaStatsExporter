using NbaStatsExporter.Data.ApiModels;
using Newtonsoft.Json;

namespace NbaStatsExporter.Http
{
    public class NbaApiService
    {
        private const string localeStr = "en";
        private RequestStatistics requestStatistics { get; set; }

        public NbaApiService()
        {
            requestStatistics = new RequestStatistics();
        }

        public async Task<TeamData?> TeamData(HttpClient client)
        {
            TeamData? result = null;
            try
            {
                string url = $"{localeStr}/league/teams.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<TeamData>(jsonResponse);
                    return result;
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }
            
            requestStatistics.SuccessfulRequests++;
            return result;
        }

        public async Task<SeasonSummaryData?> SeasonData(HttpClient client)
        {
            SeasonSummaryData? result = null;
            try
            {
                string url = $"{localeStr}/league/seasons.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<SeasonSummaryData>(jsonResponse);
                    return result;
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return result;
        }

        public async Task<ScheduleData> ScheduleData(HttpClient client, int seasonYear, string seasonType)
        {
            ScheduleData? result = new ScheduleData();
            try
            {
                string url = $"{localeStr}/games/{seasonYear}/{seasonType}/schedule.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    result = JsonConvert.DeserializeObject<ScheduleData>(jsonResponse);
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return result;
        }

        public async Task<RosterData?> RosterData(HttpClient client, string teamId)
        {
            RosterData result = null;
            try
            {
                string url = $"{localeStr}/teams/{teamId}/profile.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    result = JsonConvert.DeserializeObject<RosterData>(jsonResponse);
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return result;
        }

        public async Task<List<DraftData>> DraftData(HttpClient client, List<int> years, List<Team> teams)
        {
            List<DraftData> result = new List<DraftData>();
            try
            {
                foreach (var year in years)
                {
                    foreach (var team in teams)
                    {
                        Console.WriteLine($"{year}-{team.Alias}");

                        string url = $"{localeStr}/{year}/teams/{team.Id}/draft.json";
                        HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            result.Add(JsonConvert.DeserializeObject<DraftData>(jsonResponse));
                            requestStatistics.SuccessfulRequests++;
                        }
                        else ParseFailedResults(response);
                    }
                }
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            return result;
        }

        public async Task<List<TransferData>> TransferData(HttpClient client, List<int> years)
        {
            List<TransferData> result = new List<TransferData>();
            try
            {
                foreach (var year in years)
                {
                    string url = $"{localeStr}/{year}/trades.json";
                    HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        result.Add(JsonConvert.DeserializeObject<TransferData>(jsonResponse));
                    }
                    else ParseFailedResults(response);
                }
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return result;
        }

        public async Task<string> GamePlayByPlay(HttpClient client, string gameId)
        {
            var jsonResponse = string.Empty;
            try
            {
                string url = $"{localeStr}/games/{gameId}/pbp.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return jsonResponse;
        }


        public async Task<string> BoxScore(HttpClient client, string gameId)
        {
            var jsonResponse = string.Empty;
            try
            {
                string url = $"${localeStr}/games/{gameId}/boxscore.json";
                HttpResponseMessage response = await client.GetWithApiKeyAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else ParseFailedResults(response);
            }
            catch (Exception ex)
            {
                ParseError(ex);
            }

            requestStatistics.SuccessfulRequests++;
            return jsonResponse;
        }

        private void ParseError(Exception ex)
        {
            requestStatistics.ErroredRequests++;
            Console.Error.WriteLine("Exception: " + ex.Message);
            requestStatistics.Exceptions.Add(new Tuple<string, string>(ex.Message, ex.StackTrace));

            if (requestStatistics.Exceptions.Count > 2) throw new Exception("API data collection stopped");
        }

        private void ParseFailedResults(HttpResponseMessage response)
        {
            requestStatistics.ErroredRequests++; ;
            Console.WriteLine("Error: Unable to fetch data. Status code: " + response.StatusCode);
        }

        public RequestStatistics GetRequestStatistics()
        {
            return requestStatistics;
        }

        public class RequestStatistics
        {
            public int SuccessfulRequests { get; set; }
            public int FailedRequests { get; set; }
            public int ErroredRequests { get; set; }
            public List<Tuple<string, string>> Exceptions { get; set; } = new List<Tuple<string, string>>();
        }
    }
}
