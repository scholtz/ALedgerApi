using ALedgerBFFApi.Model.Options;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using RestSharp;

namespace ALedgerBFFApi.Extension
{
    public static class ObjectStorageExtension
    {

        public static async Task<bool> Upload(this ObjectStorage config, string objectKey, byte[] fileBytes)
        {
            if(config.Type == "AWS")
            {

                RegionEndpoint linodeRegionEndpoint = RegionEndpoint.EUCentral1;
                //.GetBySystemName("eu-central-1.linodeobjects.com");
                AmazonS3Config awsConfig = new AmazonS3Config()
                {
                    RegionEndpoint = linodeRegionEndpoint,
                    ServiceURL = "https://eu-central-1.linodeobjects.com"
                };
                var awsCredentials = new BasicAWSCredentials(config.Key, config.Secret);
                var awsClient = new AmazonS3Client(awsCredentials, awsConfig);
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = config.Bucket,
                    Key = objectKey,
                    ContentType = "application/pdf",
                    InputStream = new MemoryStream(fileBytes),
                    CannedACL = "public-read"
                };

                var response = await awsClient.PutObjectAsync(putRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            else
            {
                if (!Directory.Exists(config.Bucket))
                {
                    Directory.CreateDirectory(config.Bucket);
                }
                if (objectKey.Contains("/"))
                {
                    var pos = objectKey.LastIndexOf("/");
                    var path = objectKey.Substring(0, pos);

                    if (!Directory.Exists($"{config.Bucket}/{path}"))
                    {
                        Directory.CreateDirectory($"{config.Bucket}/{path}");
                    }
                }
                File.WriteAllBytes($"{config.Bucket}/{objectKey}", fileBytes);
                return true;
            }
            //IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.Pri);


            //AmazonUploader myUploader = new AmazonUploader();


            //// Create a unique identifier for the request
            //string requestId = Guid.NewGuid().ToString();

            //// Get the current UTC time in the required format
            //string date = DateTime.UtcNow.ToString("r");

            //// Generate the authorization signature
            //string signature = GenerateSignature(requestId, date, objectKey, "PUT", config);

            //// Create the request
            //var client = new RestClient($"https://{config.Host}");
            //var request = new RestRequest($"/{config.Bucket}/{objectKey}", Method.PUT);
            //request.AddParameter("Signature", signature);
            //request.AddParameter("AWSAccessKeyId", config.Key);

            //// Set the necessary headers
            //request.AddHeader("Host", $"{config.Host}");
            //request.AddHeader("Content-Length", fileBytes.Length.ToString());
            //request.AddHeader("Content-Type", "application/octet-stream");
            //request.AddHeader("Date", date);
            //request.AddHeader("x-amz-date", date);
            ////request.AddHeader("Authorization", $"Linode {config.Key}:{signature}");
            //request.AddParameter("application/octet-stream", fileBytes, ParameterType.RequestBody);

            //// Execute the request
            //var response = client.Execute(request);

            //// Check the response status
            //if (response.IsSuccessful)
            //{
            //    Console.WriteLine("File uploaded successfully!");
            //}
            //else
            //{
            //    Console.WriteLine("File upload failed. Error: " + response.ErrorMessage);
            //}
            //return response.IsSuccessful;
        }
        private static string GenerateSignature(string requestId, string date, string objectKey, string httpMethod, ObjectStorage config)
        {
            string signatureContent = $"{httpMethod}\n{config.Host}\n/{config.Bucket}/{objectKey}\n{date}";

            using (var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(config.Secret)))
            {
                byte[] signatureBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signatureContent));
                return Convert.ToBase64String(signatureBytes);
            }
        }
    }
}
