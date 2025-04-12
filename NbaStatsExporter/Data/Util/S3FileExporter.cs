using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

class S3FileExporter
{
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;

    public S3FileExporter(AWSCredentials credentials, string bucketName)
    {
        _s3Client = new AmazonS3Client(credentials.GetCredentials().AccessKey, credentials.GetCredentials().SecretKey, Amazon.RegionEndpoint.USEast1);
        _bucketName = bucketName;
    }

    public static async Task<bool> CanAccessS3Async(string accessKey, string secretKey, string bucketName)
    {
        try
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.EUWest1); // match your bucket's region

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = "test-object.txt",
                ContentBody = "Testing S3 write access from code"
            };

            await s3Client.PutObjectAsync(request);
            Console.WriteLine("✅ Credentials and bucket access OK");
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"❌ S3 error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnsureAwsCredentialsAndBucketAccessible()
    {
        try
        {
            // Check if the credentials are valid by listing S3 buckets
            var listResponse = await _s3Client.ListBucketsAsync();
            Console.WriteLine("AWS credentials are valid.");

            // Check if the credentials have permission to access the specified bucket
            var listObjectsResponse = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = 1
            });
            Console.WriteLine($"Access to bucket '{_bucketName}' is valid.");
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"S3 Error: {ex.Message}");
            return false;
        }
        catch (AmazonServiceException ex)
        {
            Console.WriteLine($"AWS Service Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UploadFile(string fileLocation, string fileName)
    {
        // Upload the file to S3
        try
        {
            Console.WriteLine($"Uploading {fileName} to S3...");

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(fileLocation, _bucketName);

            Console.WriteLine($"File {fileName} uploaded to S3.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            return false;
        }
    }

    public async Task<(int successCount, int failureCount)> UploadDirectoryToS3Async(string rootPath, string s3Prefix = "NbaApiOutput", CancellationToken cancellationToken = default)
    {
        int success = 0;
        int failure = 0;

        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine($"Directory does not exist: {rootPath}");
            return (0, 0);
        }

        var fileTransferUtility = new TransferUtility(_s3Client);

        foreach (var filePath in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            try
            {
                string relativePath = Path.GetRelativePath(rootPath, filePath);
                string s3Key = $"{s3Prefix}/{relativePath.Replace('\\', '/')}";

                await fileTransferUtility.UploadAsync(filePath, _bucketName, s3Key, cancellationToken);

                Console.WriteLine($"Uploaded: {filePath} => s3://{_bucketName}/{s3Key}");
                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload: {filePath} — {ex.Message}");
                failure++;
            }
        }

        return (success, failure);
    }

    public async Task<bool> EnsureBucketExistsAsync()
    {
        try
        {
            var listResponse = await _s3Client.ListBucketsAsync();

            bool exists = listResponse.Buckets.Any(b => b.BucketName.Equals(_bucketName, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                Console.WriteLine($"Bucket '{_bucketName}' does not exist. Creating...");

                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = _bucketName,
                    UseClientRegion = true // Automatically picks up region from config
                };

                await _s3Client.PutBucketAsync(putBucketRequest);

                Console.WriteLine($"Bucket '{_bucketName}' created.");
                return true;
            }
            else
            {
                Console.WriteLine($"Bucket '{_bucketName}' already exists.");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking/creating bucket: {ex.Message}");
            return false;
        }
    }
}
