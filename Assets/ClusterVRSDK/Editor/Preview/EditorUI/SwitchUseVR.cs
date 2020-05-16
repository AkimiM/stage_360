using UnityEditor;
using UnityEngine;

namespace ClusterVRSDK.Editor.Preview
{
    public class SwitchUseVR
    {
        public const string useVRKey = "useVR";
        [MenuItem("Tools/UseVR/EnableVR", false, 1)]
        public static void EnableVRMenu()
        {
            PlayerPrefs.SetInt(useVRKey, 1);
        }

        [MenuItem("Tools/UseVR/EnableVR", true)]
        public static bool EnableVRMenuValidate()
        {
            if (!PackageListRepository.Contain("openvr"))
            {
                return false;
            }
            return PlayerPrefs.GetInt(useVRKey, 1) != 1;
        }

        [MenuItem("Tools/UseVR/DisableVR", false, 1)]
        public static void DisableVRMenu()
        {
            PlayerPrefs.SetInt(useVRKey, 0);
        }

        [MenuItem("Tools/UseVR/DisableVR", true)]
        public static bool DisableVRMenuValidate()
        {
            if (!PackageListRepository.Contain("openvr"))
            {
                return false;
            }

            return PlayerPrefs.GetInt(useVRKey, 1) != 0;
        }
    }
}
