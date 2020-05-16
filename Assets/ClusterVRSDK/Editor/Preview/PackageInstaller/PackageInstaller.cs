using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterVRSDK.Editor.Preview;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

[InitializeOnLoad]
public static class PackageInstaller
{

    static PackageInstaller()
    {
        Initialize();
    }
    [MenuItem("Preview/PackageInstaller")]
    static void Initialize()
    {
        AsyncInitialize();
    }

    async static void AsyncInitialize()
    {

        await PackageListRepository.UpdatePackageList();
        var packageStates = new PackageStates(
            PackageListRepository.Contain("timeline"),
            PackageListRepository.Contain("textmeshpro"),
            PackageListRepository.Contain("postprocessing"),
            PackageListRepository.Contain("openvr")
            );

        //全部そろってたらウィンドウを表示しない
        if (packageStates.TimeLine && packageStates.TMPro && packageStates.PostProcessingStack && packageStates.OpenVR)
        {
            return;
        }

        PackageInstallerWindow.ShowWithState(packageStates);
    }

}

public readonly struct PackageStates
{
    //各packageがインストールされているかどうかをboolで表現する

    public readonly bool TimeLine;
    public readonly bool TMPro;
    public readonly bool PostProcessingStack;
    public readonly bool OpenVR;

    public PackageStates(bool timeLine, bool tMPro, bool postProcessingStack, bool openVR)
    {
        TimeLine = timeLine;
        TMPro = tMPro;
        PostProcessingStack = postProcessingStack;
        OpenVR = openVR;
    }
}
