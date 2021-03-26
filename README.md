# **Addressable S3 Uploader**

This script implements the ability to switch platform(s) and build addressables automatically within Unity. An additional option then allows for the uploading of addressable data directly to an AWS S3 Bucket.

  

# Example scenario

If you support iOS, Android and Windows, this script will:
-   Clean addressables data
-   Build addressables for current platform
-   Switch to next supported platform
-   repeat process

Once complete, you can optionally choose to upload this data asynchronously to an AWS S3 bucket.


## Requirements

 - A project using [addressables](https://docs.unity3d.com/Packages/com.unity.addressables@0.3/manual/index.html)
 - Amazon S3 package ([AWS SDK for .NET](https://github.com/aws/aws-sdk-net))
 - An AWS account to generate an [IAM access
   key](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_access-keys.html)

  

## Configuration

**Set up**

You will need to supply details for the following constants

    private const string region = ""; // AWS region, E.G: eu-west-1
    
    private const string bucketName = ""; // AWS S3 Bucket Name. E.G: path/to/bucket
    
    private const string iamAccessKeyId = ""; // AWS Access key
    
    private const string iamSecretKey = ""; // AWS Secret key
    
    private const string addressablesBuildPath = ""; // Path to built addressable data


**Specify platforms**

Specify your supported build targets within the `buildTargets` array. A complete list of build targets is within the `ConvertBuildTarget` method.

    static BuildTarget[] buildTargets =
    {
      BuildTarget.iOS,
      BuildTarget.Android,
      BuildTarget.StandaloneWindows
    };

  

Regardless of order, your active platform will take priority to reduce the time switching platform.

  

## How to build and upload

**Build all platforms**<br>
Within the top menu bar, select Tools > Addressables Uploader > Build all platforms

**Upload to S3**<br>
Within the top menu bar, select Tools > Addressables Upploader > Upload to S3


Progress status will appear within the console.
