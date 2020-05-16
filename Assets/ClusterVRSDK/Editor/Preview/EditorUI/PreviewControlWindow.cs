using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVRSDK.Editor;
using ClusterVRSDK.Editor.Preview;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class PreviewControlWindow : EditorWindow
{
    const string messageWhenNotPlayMode = "プレビューオプションは実行中のみ使用可能です";

    [MenuItem("Preview/ControlWindow")]
    public static void Show()
    {
        PreviewControlWindow wnd = GetWindow<PreviewControlWindow>();
        wnd.titleContent = new GUIContent("PreviewControlWindow");
    }


    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        Bootstrap.OnUpdateTriggerIdList += triggerIdList => root.Add(GenerateTriggerSection(triggerIdList));
        root.Add(GenerateCommentSection());
        root.Add(UiUtils.Separator());
        root.Add(GenerateMainScreenSection());
        root.Add(UiUtils.Separator());
        root.Add(GenerateUserDataSection());
        root.Add(UiUtils.Separator());
    }

    void SendComment(string displayName, string userName, string content)
    {
        if (!Bootstrap.IsInGameMode)
        {
            Debug.LogWarning(messageWhenNotPlayMode);
            return;
        }

        if (String.IsNullOrEmpty(displayName))
            displayName = "DisplayName";
        if (String.IsNullOrEmpty(userName))
            userName = "UserName";
        Bootstrap.CommentScreenPresenter.SendCommentFromEditorUI(displayName, userName, content);
    }

    void ShowMainScreenPicture()
    {
        if (!Bootstrap.IsInGameMode)
        {
            Debug.LogWarning(messageWhenNotPlayMode);
            return;
        }

        Bootstrap.MainScreenPresenter.SetImage(
            AssetDatabase.LoadAssetAtPath<Texture>(
                "Assets/ClusterVRSDK/Editor/Preview/SampleTexture/cluster_logo.png"));
    }

    void SendTrigger(string id, float diff)
    {
        Bootstrap.VenueGimmickManager.RunFromEditor(id, diff);
    }


    VisualElement GenerateCommentSection()
    {
        var commentSection = EditorUIGenerator.GenerateSection();
        commentSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h1, "コメント"));
        commentSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "表示名"));
        TextField displayNameField = new TextField();
        commentSection.Add(displayNameField);

        commentSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "ユーザー名"));
        TextField userNameField = new TextField();
        commentSection.Add(userNameField);

        commentSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "コメント内容"));

        TextField commentContentField = new TextField();
        commentContentField.style.unityTextAlign = TextAnchor.UpperLeft;
        commentContentField.multiline = true;
        commentContentField.style.height = 50;
        foreach (var child in commentContentField.Children())
        {
            child.style.unityTextAlign = TextAnchor.UpperLeft;
        }

        commentSection.Add(commentContentField);

        Button commentSendButton = new Button(() =>
        {
            SendComment(displayNameField.value, userNameField.value, commentContentField.value);
            displayNameField.value = "";
            userNameField.value = "";
            commentContentField.value = "";
        });
        commentSendButton.text = "コメントを送信";
        commentSection.Add(commentSendButton);
        return commentSection;
    }

    VisualElement GenerateMainScreenSection()
    {
        var mainScreenSection = EditorUIGenerator.GenerateSection();
        mainScreenSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h1, "メインスクリーン"));
        Button sampleImageSendButton = new Button(ShowMainScreenPicture);
        sampleImageSendButton.text = "サンプル画像を投影";
        mainScreenSection.Add(sampleImageSendButton);
        return mainScreenSection;
    }

    VisualElement GenerateTriggerSection(List<string> triggerIdList)
    {
        var triggerSection = EditorUIGenerator.GenerateSection();
        triggerSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h1, "トリガー"));
        //一般用トリガーとスタッフ向けトリガーにリストを分離
        var audienceTriggerList = triggerIdList.Where(x => x.StartsWith("@")).ToList();
        var staffTriggerList = triggerIdList.Where(x => !x.StartsWith("@")).ToList();

        if (audienceTriggerList.Count != 0)
        {
            triggerSection.Add(GenerateTriggerSender(audienceTriggerList, "一般参加者向けトリガー"));
        }

        if (staffTriggerList.Count != 0)
        {
            triggerSection.Add(GenerateTriggerSender(staffTriggerList, "スタッフ向けトリガー"));
        }


        triggerSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "Jsonからトリガーを発火"));
        Button openTriggerFromJsonWindowButton = new Button(PreviewTriggerFromJsonWindow.Show);
        openTriggerFromJsonWindowButton.text = "開く";

        triggerSection.Add(openTriggerFromJsonWindowButton);

        return triggerSection;
    }

    VisualElement GenerateTriggerSender(List<string> triggerList, string triggerName)
    {
        VisualElement root = new VisualElement();
        root.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, triggerName));

        var triggerPopupField = new PopupField<string>(triggerList, 0);
        root.Add(triggerPopupField);

        Button triggerSendButton = new Button(() => { SendTrigger(triggerPopupField.value, 0f); });
        triggerSendButton.text = "トリガーを送信";
        root.Add(triggerSendButton);
        return root;
    }

    VisualElement GenerateUserDataSection()
    {
        var userDataSection = EditorUIGenerator.GenerateSection();
        userDataSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h1, "プレイヤー情報"));
        userDataSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "権限"));
        var currentPermission = EditorUIGenerator.GenerateLabel(LabelType.h2, "現在の権限:参加者");
        Button permissionChangeButton = new Button(() =>
        {
            if (!Bootstrap.IsInGameMode)
            {
                Debug.LogWarning(messageWhenNotPlayMode);
                return;
            }

            if (Bootstrap.PlayerPresenter.PermissionType == PermissionType.Audience)
            {
                Bootstrap.PlayerPresenter.ChangePermissionType(PermissionType.Performer);
                currentPermission.text = "現在の権限:パフォーマー";
            }
            else
            {
                Bootstrap.PlayerPresenter.ChangePermissionType(PermissionType.Audience);
                currentPermission.text = "現在の権限:参加者";
            }
        });
        permissionChangeButton.text = "権限変更";
        userDataSection.Add(currentPermission);
        userDataSection.Add(permissionChangeButton);

        userDataSection.Add(EditorUIGenerator.GenerateLabel(LabelType.h2, "リスポーン"));
        Button respawnButton = new Button(() =>
        {
            Bootstrap.SpawnPointManager.Respawn(Bootstrap.PlayerPresenter.PermissionType,
                Bootstrap.PlayerPresenter.PlayerTransform, Bootstrap.PlayerPresenter.CameraTransform);
        });
        respawnButton.text = "リスポーンする";
        userDataSection.Add(respawnButton);
        return userDataSection;
    }
}
