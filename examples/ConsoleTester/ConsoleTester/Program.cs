using System;
using System.Configuration;
using System.IO;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using GroupDocs.Viewer.AmazonS3;
using GroupDocs.Viewer.Config;
using GroupDocs.Viewer.Handler;

namespace ConsoleTester
{
    static class Program
    {
        static void Main(string[] args)
        {
            //TODO: Set your credentials and bucket name app.config

            var client = new AmazonS3Client();
            var bucket = ConfigurationManager.AppSettings["AWSBucket"];
            var fileName = "sample.txt";

            UploadTestFile(fileName, bucket, client);
            RenderTestFile(fileName, bucket, client);

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static void RenderTestFile(string fileName, string bucket, AmazonS3Client client)
        {
            var storage = new AmazonS3Storage(client, bucket);

            var config = new ViewerConfig
            {
                EnableCaching = true
            };
            var viewer = new ViewerHtmlHandler(config, storage);

            var pages = viewer.GetPages(fileName);

            viewer.ClearCache(fileName);

            foreach (var page in pages)
            {
                Console.WriteLine(page.HtmlContent);
            }
        }

        static void UploadTestFile(string fileName, string bucket, AmazonS3Client client)
        {
            var fileContent = "Hello, World!";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);

            var request = new PutObjectRequest
            {
                Key = fileName,
                BucketName = bucket,
                InputStream = new MemoryStream(fileBytes)
            };

            client.PutObject(request);
        }
    }
}
