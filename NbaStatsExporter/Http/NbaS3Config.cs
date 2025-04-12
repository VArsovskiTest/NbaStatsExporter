using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace NbaStatsExporter.Http
{
    class NbaS3Config
    {
        private RegionEndpoint region = RegionEndpoint.USEast1;
        public AmazonS3Client GetS3Client(AWSCredentials credentials)
        {
            return new AmazonS3Client(credentials, region);
        }

        public AWSCredentials GetAWSCredentials(string accessKey, string secretKey)
        {
            return new BasicAWSCredentials(accessKey, secretKey);
        }

    }
}
