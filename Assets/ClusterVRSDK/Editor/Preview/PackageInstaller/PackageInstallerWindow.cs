using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class PackageInstallerWindow : EditorWindow
{
    static PackageStates packageStates;
    public static void ShowWithState(PackageStates packageStates)
    {
        PackageInstallerWindow.packageStates = packageStates;
        PackageInstallerWindow wnd = GetWindow<PackageInstallerWindow>();
        wnd.titleContent = new GUIContent("PackageInstaller");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("必要なパッケージがインポートされていません");
        Label notExistingPackage = new Label();
        if (!packageStates.TimeLine)
            notExistingPackage.text += "TimeLine\n";
        if (!packageStates.TMPro)
            notExistingPackage.text += "TextMeshPro\n";
        if (!packageStates.PostProcessingStack)
            notExistingPackage.text += "PostProcessingStack\n";
        if (!packageStates.OpenVR)
            notExistingPackage.text += "OpenVR";

        VisualElement certificationLabel = new Label("これらのパッケージをインポートしますか？");

        Button acceptButton = new Button(() => ImportPackages(packageStates));
        Button declineButton = new Button(Close);

        acceptButton.text = "はい";
        declineButton.text = "いいえ";

        Box buttonsBox = new Box();
        buttonsBox.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.RowReverse);
        buttonsBox.Add(declineButton);
        buttonsBox.Add(acceptButton);

        root.Add(label);
        root.Add(notExistingPackage);
        root.Add(certificationLabel);
        root.Add(buttonsBox);
    }

    void ImportPackages(PackageStates packageStates)
    {
        if(!packageStates.TimeLine)
            Client.Add("com.unity.timeline");
        if(!packageStates.TMPro)
            Client.Add("com.unity.textmeshpro");
        if(!packageStates.PostProcessingStack)
            Client.Add("com.unity.postprocessing");
        if(!packageStates.OpenVR)
            Client.Add("com.unity.xr.openvr.standalone");
       Close();
    }
}
