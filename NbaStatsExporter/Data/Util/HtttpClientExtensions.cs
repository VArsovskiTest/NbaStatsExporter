using System.Web;

public static class HttpClientExtensions
{
    // Extension method to add api_key from headers to URL
    public static async Task<HttpResponseMessage> GetWithApiKeyAsync(this HttpClient client, string url)
    {
        if (client.BaseAddress == null)
        {
            throw new InvalidOperationException("BaseAddress is not set in HttpClient.");
        }

        Uri fullUri = new Uri(client.BaseAddress, url);

        if (client.DefaultRequestHeaders.Contains("api_key"))
        {
            var apiKey = client.DefaultRequestHeaders.GetValues("api_key").FirstOrDefault();

            if (!string.IsNullOrEmpty(apiKey))
            {
                var uriBuilder = new UriBuilder(fullUri)
                {
                    Query = AppendApiKeyToQuery(fullUri.Query, apiKey)
                };

                await Task.Delay(1000);  // Non-Blocking delay to respect 1 QPS
                return await client.GetAsync(uriBuilder.Uri);
            }
            else
            {
                throw new ArgumentException("API Key is missing from request headers.");
            }
        }
        else
        {
            throw new ArgumentException("API Key header is not present in the request.");
        }
    }

    private static string AppendApiKeyToQuery(string existingQuery, string apiKey)
    {
        var queryParams = HttpUtility.ParseQueryString(existingQuery);
        queryParams["api_key"] = apiKey;
        return queryParams.ToString();
    }
}
