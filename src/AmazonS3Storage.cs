using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using GroupDocs.Viewer.Storage;

namespace GroupDocs.Viewer.AmazonS3
{
    /// <summary>
    /// Amazon S3 file storage
    /// </summary>
    public class AmazonS3Storage : IFileStorage, IDisposable
    {
        private readonly IAmazonS3 _client;

        private readonly string _bucketName;

        /// <summary>
        /// Initializes new instance of <see cref="AmazonS3Storage"/> class.
        /// </summary>
        /// <param name="client">Amazon S3 client</param>
        /// <param name="bucketName">Amazon S3 bucket name</param>
        public AmazonS3Storage(IAmazonS3 client, string bucketName)
        {
            _client = client;
            _bucketName = bucketName;
        }

        /// <summary>
        /// Checks if file exists
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns><c>true</c> when file exists, otherwise <c>false</c></returns>
        public bool FileExists(string path)
        {
            try
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                _client.GetObjectMetadata(request);

                return true;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves file content
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>Stream</returns>
        public Stream GetFile(string path)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                Key = path,
                BucketName = _bucketName,
            };

            using (GetObjectResponse response = _client.GetObject(request))
            {
                MemoryStream result = new MemoryStream();
                CopyStream(response.ResponseStream, result);
                result.Position = 0;

                return result;
            }
        }

        /// <summary>
        /// Saves file
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="content">File content.</param>
        public void SaveFile(string path, Stream content)
        {
            PutObjectRequest request = new PutObjectRequest
            {
                Key = path,
                BucketName = _bucketName,
                InputStream = content
            };

            _client.PutObject(request);
        }

        /// <summary>
        /// Removes directory
        /// </summary>
        /// <param name="path">Directory path.</param>
        public void DeleteDirectory(string path)
        {
            S3DirectoryInfo directory = new S3DirectoryInfo(_client, _bucketName, path);
            directory.Delete(true);
        }

        /// <summary>
        /// Retrieves file information
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>File information.</returns>
        public IFileInfo GetFileInfo(string path)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = path
            };

            GetObjectMetadataResponse response = _client.GetObjectMetadata(request);

            IFileInfo file = new Storage.FileInfo();
            file.Path = path;
            file.Size = response.ContentLength;
            file.LastModified = response.LastModified;

            return file;
        }

        /// <summary>
        /// Retrieves list of files and folders
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <returns>Files and folders.</returns>
        public IEnumerable<IFileInfo> GetFilesInfo(string path)
        {
            var prefix = path.Length > 1 ? path : string.Empty;

            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                Prefix = prefix,
                Delimiter = "/"
            };

            ListObjectsResponse response = _client.ListObjects(request);

            List<IFileInfo> files = new List<IFileInfo>();

            // add directories 
            foreach (string directory in response.CommonPrefixes)
            {
                IFileInfo folder = new Storage.FileInfo();
                folder.Path = directory;
                folder.IsDirectory = true;

                files.Add(folder);
            }

            // add files
            foreach (S3Object entry in response.S3Objects)
            {
                IFileInfo file = new Storage.FileInfo
                {
                    Path = entry.Key,
                    IsDirectory = false,
                    LastModified = entry.LastModified,
                    Size = entry.Size
                };

                files.Add(file);
            }

            return files;
        }

        private void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}