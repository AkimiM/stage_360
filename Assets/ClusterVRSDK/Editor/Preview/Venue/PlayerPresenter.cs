using System;
using ClusterVRSDK.Preview.PlayerController;
using UniGLTF;
using UnityEditor;
using UnityEngine;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace ClusterVRSDK.Editor.Preview
{
    public class PlayerPresenter
    {
        const string NonVRPrefabPath = "Assets/ClusterVRSDK/Editor/Preview/Prefabs/PreviewOnly.prefab";
        const string VRPrefabPath = "Assets/ClusterVRSDK/Editor/Preview/Prefabs/VRPlayerController.prefab";
        GameObject previewOnly;

        readonly IPlayerController playerController;
        readonly EnterDeviceType enterDeviceType;

        Vector3? recordedPosition;
        Quaternion? recordedRotation;

        readonly Transform cameraTransform;
        readonly Transform playerTransform;

        private bool isInPersonalCamera;

        public Transform PlayerTransform => playerTransform;
        public Transform CameraTransform => cameraTransform;


        PermissionType permissionType;
        public PermissionType PermissionType => permissionType;

        public PlayerPresenter(PermissionType permissionType, EnterDeviceType enterDeviceType)
        {
            //TODO 引数のenumをPlayerのGameObjectに渡しつつシーンに生成
            this.permissionType = permissionType;
            this.enterDeviceType = enterDeviceType;

            GameObject previewOnlyPrefab;
            switch (enterDeviceType)
            {
                case EnterDeviceType.Desktop:
                    previewOnlyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NonVRPrefabPath);
                    break;
                case EnterDeviceType.VR:
                    previewOnlyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VRPrefabPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enterDeviceType), enterDeviceType, null);
            }

            previewOnly = PrefabUtility.InstantiatePrefab(previewOnlyPrefab) as GameObject;

            playerController = previewOnly.GetComponentInChildren<IPlayerController>();
            playerTransform = playerController.PlayerTransform;
            cameraTransform = playerController.CameraTransform;


#if UNITY_POST_PROCESSING_STACK_V2
            var postProcessLayer = cameraTransform.gameObject.GetOrAddComponent<PostProcessLayer>();
            postProcessLayer.volumeTrigger = cameraTransform;
            postProcessLayer.volumeLayer = 1 << LayerMask.NameToLayer("PostProcessing");
#endif


            //Permissionに応じた初期位置にスポーンする
            Bootstrap.SpawnPointManager.Respawn(permissionType, playerTransform, cameraTransform);
        }

        public void ChangePermissionType(PermissionType permissionType)
        {
            this.permissionType = permissionType;
            ChangeLayer(permissionType);
        }

        void ChangeLayer(PermissionType permissionType)
        {
            switch (permissionType)
            {
                case PermissionType.Performer:
                    playerTransform.gameObject.layer = LayerMask.NameToLayer("Performer");
                    break;
                case PermissionType.Audience:
                    playerTransform.gameObject.layer = LayerMask.NameToLayer("Audience");
                    break;
            }
        }

        public void SetPointOfView(Transform targetPoint)
        {
            isInPersonalCamera = true;
            //位置を記録した後PersonalCameraの位置に移動、CharacterControllerを無効化する
            if (enterDeviceType == EnterDeviceType.VR)
            {
                recordedPosition = playerTransform.position;
                recordedRotation = playerTransform.rotation;
                // OpenVRのカメラの高さがそのまま視点の高さになるので、 見せたい視点の高さ(y) - OpenVRによるカメラの高さ(local y)をしてやることで、視点の視線の高さにする
                var targetPosition = new Vector3(targetPoint.position.x, targetPoint.position.y - cameraTransform.localPosition.y, targetPoint.position.z);
                playerTransform.SetPositionAndRotation(targetPosition, targetPoint.rotation);
            }
            else
            {
                recordedPosition = cameraTransform.position;
                recordedRotation = cameraTransform.rotation;
                cameraTransform.SetPositionAndRotation(targetPoint.position, targetPoint.rotation);
            }

            playerController.ActivateCharacterController(false);
        }

        public void ResetPointOfView()
        {
            if (!isInPersonalCamera)
            {
                return;
            }

            //SetPointOfViewの時に記録した位置に戻す、CharacterControllerを再度有効化
            if (!recordedPosition.HasValue || !recordedRotation.HasValue)
            {
                return;
            }

            if (enterDeviceType == EnterDeviceType.VR)
            {
                playerTransform.SetPositionAndRotation(recordedPosition.Value, recordedRotation.Value);
            }
            else
            {
                cameraTransform.SetPositionAndRotation(recordedPosition.Value, recordedRotation.Value);
            }

            playerController.ActivateCharacterController(true);
            isInPersonalCamera = false;
        }
    }

    public enum PermissionType
    {
        //TODO 必要な役職を列挙
        Performer,
        Audience
    }

    public enum EnterDeviceType
    {
        Desktop,
        VR
    }
}
