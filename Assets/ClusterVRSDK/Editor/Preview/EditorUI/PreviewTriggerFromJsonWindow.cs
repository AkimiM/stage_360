using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ClusterVRSDK.Editor.Preview
{
    public class PreviewTriggerFromJsonWindow : EditorWindow
    {
        Label pathLabel;
        VisualElement jsonSection;
        VisualElement triggerSection;

        [MenuItem("Preview/PreviewTriggerFromJsonWindow")]
        public static void Show()
        {
            PreviewTriggerFromJsonWindow wnd = GetWindow<PreviewTriggerFromJsonWindow>();
            wnd.titleContent = new GUIContent("PreviewTriggerFromJsonWindow");
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            root.Add(EditorUIGenerator.GenerateLabel(LabelType.h1, "Jsonからトリガーを発火"));
            jsonSection = GenerateJsonSection();
            triggerSection = EditorUIGenerator.GenerateSection();
            root.Add(jsonSection);
            root.Add(triggerSection);
        }

        VisualElement GenerateJsonSection()
        {
            var jsonSection = EditorUIGenerator.GenerateSection();
            jsonSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "参照するJsonを選択"));
            var pathText = new Label();
            pathText.text = "Jsonファイル未選択";
            var openFileButton = new Button(() =>
            {
                var path = EditorUtility.OpenFilePanelWithFilters("Jsonを選択", "", new[] {"Json", "json"});
                pathText.text = path;
                UpdatePath(path);
            });
            openFileButton.text = "参照";
            jsonSection.Add(pathText);
            jsonSection.Add(openFileButton);
            return jsonSection;
        }

        void UpdatePath(string currentPath)
        {
            triggerSection.Clear();
            var pathLabel = new Label();
            pathLabel.text = currentPath;
            var json = System.IO.File.ReadAllText(currentPath);
            Debug.Log(json);

            var triggers = JsonUtility.FromJson<TriggerList>(json).Triggers;
            Debug.Log(triggers.Select(x => x.Id).ToList()[0]);
            var triggerPopupField = new PopupField<string>(triggers.Select(x => x.Id).ToList(), 0);

            var triggerButton = new Button(() =>
            {
                if (Bootstrap.IsInGameMode)
                {
                    SendTrigger(triggerPopupField.value, 0);
                }
                else
                {
                    Debug.LogError("プレビュー実行中のみトリガーを送信できます");
                }
            });
            triggerButton.text = "トリガー送信";

            triggerSection.Add(pathLabel);
            triggerSection.Add(triggerPopupField);
            triggerSection.Add(triggerButton);
        }

        void SendTrigger(string id, float diff)
        {
            Bootstrap.VenueGimmickManager.RunFromEditor(id, diff);
        }
    }

    [Serializable]
    public struct TriggerList
    {
        [SerializeField] List<Trigger> triggers;
        public List<Trigger> Triggers => triggers;
    }

    [Serializable]
    public struct Trigger
    {
        [SerializeField] string id;
        public string Id => id;
        [SerializeField] string category;
        [SerializeField] bool instant;
        [SerializeField] bool showConfirmDialog;
        [SerializeField] string displayName;
        public string DisplayName => displayName;
        Color color;
    }
}
