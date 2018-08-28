using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using NUnit.Framework;

namespace GroupDocs.Viewer.AmazonS3.Tests
{
    [TestFixture]
    public class AmazonS3StorageTests
    {
        private AmazonS3Client _amazonS3Client;
        private AmazonS3Storage _amazonS3Storage;
        private readonly string _bucketName = ConfigurationManager.AppSettings["AWSBucket"];
        private readonly string _testFilePath = "folder/sample.txt";

        [SetUp]
        public void SetupFixture()
        {
            _amazonS3Client = new AmazonS3Client();
            _amazonS3Storage = new AmazonS3Storage(_amazonS3Client, _bucketName);

            UploadTestFile(_testFilePath);
        }

        [Test]
        public void TestFileExists()
        {
            Assert.IsTrue(_amazonS3Storage.FileExists(_testFilePath));
        }

        [Test]
        public void TestGetFile()
        {
            var stream = _amazonS3Storage.GetFile(_testFilePath);

            var contents = Encoding.UTF8.GetString(ReadBytes(stream));

            Assert.AreEqual("Hello, World!", contents);
        }

        [Test]
        public void TestSaveFile()
        {
            var filePath = "folder/saved.txt";

            var fileContent = "Hello, World!";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var fileStream = new MemoryStream(fileBytes);

            _amazonS3Storage.SaveFile(filePath, fileStream);

            Assert.AreEqual("Hello, World!", GetFileContent(filePath));

            DeleteFile(filePath);
        }

        [Test]
        public void TestGetFileInfo()
        {
            var fileInfo = _amazonS3Storage.GetFileInfo(_testFilePath);

            Assert.AreEqual(_testFilePath, fileInfo.Path);
            Assert.AreNotEqual(DateTime.MinValue, fileInfo.LastModified);
            Assert.IsFalse(fileInfo.IsDirectory);
            Assert.IsTrue(fileInfo.Size > 0);
        }

        [Test]
        public void TestGetFilesInfo()
        {
            var path = "folder/";

            var filesInfo = _amazonS3Storage.GetFilesInfo(path).ToList();

            Assert.AreEqual(1, filesInfo.Count);
            Assert.AreEqual(_testFilePath, filesInfo[0].Path);
        }

        [Test]
        public void TestDeleteDirectory()
        {
            var filePath = "directory_to_delete/file.txt";

            UploadTestFile(filePath);

            _amazonS3Storage.DeleteDirectory("directory_to_delete");

            Assert.IsFalse(FileExist(filePath));
        }

        private bool FileExist(string filePath)
        {
            try
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                _amazonS3Client.GetObjectMetadata(request);

                return true;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
        }

        private void UploadTestFile(string filePath)
        {
            var fileContent = "Hello, World!";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);

            var request = new PutObjectRequest
            {
                Key = filePath,
                BucketName = _bucketName,
                InputStream = new MemoryStream(fileBytes)
            };

            _amazonS3Client.PutObject(request);
        }

        private string GetFileContent(string filePath)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                Key = filePath,
                BucketName = _bucketName,
            };

            using (GetObjectResponse response = _amazonS3Client.GetObject(request))
                return Encoding.UTF8.GetString(ReadBytes(response.ResponseStream));
        }

        private void DeleteFile(string filePath)
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                Key = filePath,
                BucketName = _bucketName,
            };

            _amazonS3Client.DeleteObject(request);
        }

        private byte[] ReadBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}