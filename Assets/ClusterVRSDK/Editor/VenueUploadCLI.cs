using System;
using ClusterVRSDK.Core.Editor.Venue;
using ClusterVRSDK.Core.Editor.Venue.Json;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClusterVRSDK.Editor
{
    public class VenueUploadCLI
    {
        static void ValidateScene()
        {
            OpenTargetScene();

            if (!VenueValidator.ValidateVenue(out var errorMessage))
            {
                Debug.LogError($"ValidationError:{errorMessage}");
            }

            Debug.Log("Passed Validation");
        }

        static void BuildScene()
        {
            OpenTargetScene();

            var venueId = GetEnv("VENUE_ID");
            AssetExporter.ExportCurrentSceneResource(venueId, false);

            Debug.Log("Finished Build");
        }

        static void OpenTargetScene()
        {
            var sceneAssetPath = GetEnv("SCENE_PATH");
            EditorSceneManager.OpenScene(sceneAssetPath);
        }

        static void UploadScene()
        {
            Debug.Log("Upload Start");

            var accessToken = GetEnv("ACCESS_TOKEN");
            var venueId = new VenueID(GetEnv("VENUE_ID"));

            AssetUploader.Upload(accessToken, venueId);

            Debug.Log("Upload End");
        }

        static string GetEnv(string key)
        {
            var v = Environment.GetEnvironmentVariable(key);
            if (v == null)
            {
                throw new Exception(key + " environment value is not found");
            }

            Debug.Log("use environment value for " + key);
            Debug.Log(v);

            return v;
        }
    }
}
