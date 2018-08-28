# GroupDocs.Viewer .NET AmazonS3

[Amazon S3](https://aws.amazon.com/s3/) file storage for [GroupDocs.Viewer for .NET](https://www.nuget.org/packages/groupdocs.viewer)
 which allows you to keep files and cache in the cloud. 

## Installation & Configuration

Install via [nuget.org](http://nuget.org)

```powershell
Install-Package GroupDocs.Viewer.AmazonS3Storage
```

If you're hosting your project inside EC2 instance IAM access keys already exist inside instance via environment variables.
Please check [AWS Access Keys best practices article](http://docs.aws.amazon.com/general/latest/gr/aws-access-keys-best-practices.html) for more 
information about keeping your access keys secure. 

For the test purposes you can add [IAM access keys](http://docs.aws.amazon.com/IAM/latest/UserGuide/ManagingCredentials.html) to your AppSettings in app.config or web.config.
(Do not commit your access keys to the source control).

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="AWSAccessKey" value="***"/>
    <add key="AWSSecretKey" value="***"/>
    <add key="AWSRegion" value="***" />
  </appSettings>
</configuration>
```

## How to use

```csharp

var client = new AmazonS3Client();
var bucket = "my-bucket";

var storage = new AmazonS3Storage(client, bucket);

var viewer = new ViewerHtmlHandler(storage);

var pages = viewer.GetPages("document.docx");
```

## License

GroupDocs.Viewer .NET AmazonS3 is Open Source software released under the [MIT license](https://github.com/harumburum/groupdocs-viewer-net-amazons3/blob/master/LICENSE.md).