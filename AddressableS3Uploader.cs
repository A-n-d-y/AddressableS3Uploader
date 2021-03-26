using System;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AddressableAssets;

#pragma warning disable 4014 // Disable async await warning.

public class AddressableS3Uploader : EditorWindow
{
    private const string region = ""; // AWS region, E.G: eu-west-1
    private const string bucketName = ""; // AWS S3 Bucket Name. E.G: path/to/bucket
    private const string iamAccessKeyId = ""; // AWS Access key 
    private const string iamSecretKey = ""; // AWS Secret key
    private const string addressablesBuildPath = ""; // Path to built addressable data
    private RegionEndpoint bucketRegion;
    public static List<BuildTarget> remainingBuildTargets;


    /// <summary>
    /// Array of build targets for addressables creation
    /// Available build targets are within the ConvertBuildTarget method
    /// </summary>
    static BuildTarget[] buildTargets =
    {
        BuildTarget.iOS,
        BuildTarget.Android,
        BuildTarget.StandaloneWindows
    };

    static BuildTargetGroup ConvertBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneOSX:
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                case BuildTarget.Tizen:
                    return BuildTargetGroup.Tizen;
                case BuildTarget.PSP2:
                    return BuildTargetGroup.PSP2;
                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;
                case BuildTarget.PSM:
                    return BuildTargetGroup.PSM;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                case BuildTarget.N3DS:
                    return BuildTargetGroup.N3DS;
                case BuildTarget.WiiU:
                    return BuildTargetGroup.WiiU;
                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
                case BuildTarget.NoTarget:
                default:
                    return BuildTargetGroup.Standalone;
            }
        }


    [MenuItem("Tools/Addressables Uploader/Build Addressables all platforms")]
    public static async void BuildAll()
    {
        string platformSwitchWarning = "This action will switch between platforms and build all addressable bundles. Do you want to continue?";
        if (!EditorUtility.DisplayDialog("Build for all platforms?", platformSwitchWarning, "Yes", "No")) return;

        Debug.Log("Building addressables for " + EditorUserBuildSettings.activeBuildTarget);
        await BuildAddressables();

        remainingBuildTargets = new List<BuildTarget>(buildTargets);

        // We have now built for this platform, so remove it from the remaining targets
        if (remainingBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget))
        {
            remainingBuildTargets.Remove(EditorUserBuildSettings.activeBuildTarget);
        }

        // Builds the remaining platforms
        foreach (BuildTarget buildTarget in remainingBuildTargets)
        {
            bool switched = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(ConvertBuildTarget(buildTarget), buildTarget);
            if (switched == false)
            {
                Debug.Log("Exiting early: error during build of " + buildTarget);
                break;
            }
        }
    }
    

    static async void BuildAddressables()
        {
            UnityEngine.Debug.Log("Cleaning addressables...");
            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            UnityEngine.Debug.Log("Building addressables...");
            AddressableAssetSettings.BuildPlayerContent();
            UnityEngine.Debug.Log("Addressables built for current platform.");
        }


    [MenuItem("Tools/Addressables Uploader/Upload to S3")]
    static async void InitiateUpload()
    {
        string uploadWarning = "This action will upload addressables for all platforms to S3. Do you want to continue?";
        if (!EditorUtility.DisplayDialog("Upload Addressables?", uploadWarning, "Yes", "No")) return;
        await UploadAsync(RegionEndpoint.GetBySystemName(region), bucketName, iamAccessKeyId, iamSecretKey, addressablesBuildPath);
    }
    

    private static async void UploadAsync(RegionEndpoint bucketRegion, string bucketName, string iamAccessKeyId, string iamSecretKey, string path)
    {
        try
        {
            Debug.Log("Starting upload...");
            BasicAWSCredentials credentials = new BasicAWSCredentials(iamAccessKeyId, iamSecretKey);
            AmazonS3Client s3Client = new AmazonS3Client(credentials, bucketRegion);
            TransferUtility transferUtility = new TransferUtility(s3Client);
            TransferUtilityUploadDirectoryRequest transferUtilityRequest = new TransferUtilityUploadDirectoryRequest
            {
                BucketName = bucketName,
                Directory = path,
                StorageClass = S3StorageClass.StandardInfrequentAccess,
                CannedACL = S3CannedACL.PublicRead,
                SearchOption = SearchOption.AllDirectories,
            };

            await transferUtility.UploadDirectoryAsync(transferUtilityRequest);
            Debug.Log("Upload completed");
        }
        catch (AmazonS3Exception e)
        {
            Debug.LogError("Error encountered on server when writing an object: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Unknown encountered on server when writing an object: " + e.Message);
        }
    }
}