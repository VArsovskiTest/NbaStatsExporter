namespace NbaStatsExporter.Data
{
    public class LocalConfig
    {
        public string NbaApiKey { get; set; }
        public string S3BucketName { get; set; }
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public string ApiOutputDirectory { get; set; }
        public string CopyOutputDirectory { get; set; }

        public static string GetFromEnvironmentVariables(string envVarName)
        {
            return Environment.GetEnvironmentVariable(envVarName);
        }
    }
}
