using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterVR.InternalSDK.Core;
using ClusterVR.InternalSDK.Core.Gimmick;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;


namespace ClusterVRSDK.Editor.Preview
{
    [InitializeOnLoad]
    public static class Bootstrap
    {
        static RankingScreenPresenter rankingScreenPresenter;

        public static RankingScreenPresenter RankingScreenPresenter => rankingScreenPresenter;
        static CommentScreenPresenter commentScreenPresenter;
        public static CommentScreenPresenter CommentScreenPresenter => commentScreenPresenter;

        static VenueGimmickManager venueGimmickManager;

        public static VenueGimmickManager VenueGimmickManager => venueGimmickManager;

        static MainScreenPresenter mainScreenPresenter;

        public static MainScreenPresenter MainScreenPresenter => mainScreenPresenter;

        static SpawnPointManager spawnPointManager;

        public static SpawnPointManager SpawnPointManager => spawnPointManager;

        static PlayerPresenter playerPresenter;

        static AvatarRespawner avatarRespawner;
        static bool isInGameMode;

        public static bool IsInGameMode => isInGameMode;

        public static PlayerPresenter PlayerPresenter => playerPresenter;
        public static Action<List<string>> OnUpdateTriggerIdList;

        static Bootstrap()
        {
            EditorApplication.playModeStateChanged += (async (playMode) =>
                {
                    await OnChangePlayModeAsync(playMode);
                    OnChangePlayMode(playMode);
                });
        }

        static void OnChangePlayMode(PlayModeStateChange playMode)
        {
            switch (playMode)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    SetIsInGameMode(false);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    SetIsInGameMode(true);

                    var commentScreenViews = new List<ICommentScreenView>();
                    var mainScreenViews = new List<IMainScreenView>();
                    var rankingScreenViews = new List<IRankingScreenView>();
                    var spawnPoints = new List<ISpawnPoint>();
                    var despawnHeight = float.MinValue;
                    var triggerSenders = new List<ITriggerSender>();

                    foreach (var binding in Resources.FindObjectsOfTypeAll<SdkBindingBase>()
                        .Where(x => x.gameObject.scene.isLoaded))
                    {
                        switch (binding)
                        {
                            case ICommentScreenView commentScreenView:
                                commentScreenViews.Add(commentScreenView);
                                break;
                            case IRankingScreenView rankingScreenView:
                                rankingScreenViews.Add(rankingScreenView);
                                break;
                            case IMainScreenView mainScreenView:
                                mainScreenViews.Add(mainScreenView);
                                break;
                            case ISpawnPoint spawnPoint:
                                spawnPoints.Add(spawnPoint);
                                break;
                            case IWarpPortal warpPortal:
                                warpPortal.OnEnterWarpPortalEvent += (sender, e) =>
                                {
                                    if (!e.KeepPosition)
                                    {
                                        playerPresenter.PlayerTransform.position = e.ToPosition;
                                    }

                                    if (!e.KeepRotation)
                                    {
                                        playerPresenter.CameraTransform.rotation = e.ToRotation;
                                    }
                                };
                                break;
                            case IDespawnHeight _despawnHeight:
                                despawnHeight = _despawnHeight.Height;
                                break;
                            case ITriggerSender triggerSender:
                                triggerSender.TriggerEvent += (sender, args) =>
                                    venueGimmickManager.RunFromTriggerSender(args.Id, 0,
                                        playerPresenter.PermissionType);
                                break;
                        }
                    }


                    venueGimmickManager = new VenueGimmickManager();

                    foreach (var venueGimmick in Resources.FindObjectsOfTypeAll<VenueGimmickBase>()
                        .Where(x => x.gameObject.scene.isLoaded))
                    {
                        venueGimmick.Initialize(venueGimmickManager);
                    }

                    rankingScreenPresenter = new RankingScreenPresenter(rankingScreenViews);
                    commentScreenPresenter = new CommentScreenPresenter(commentScreenViews);
                    mainScreenPresenter = new MainScreenPresenter(mainScreenViews);
                    spawnPointManager = new SpawnPointManager(spawnPoints);
                    //疑似Playerの設定
                    var enterDeviceType = EnterDeviceType.Desktop;
                    if (XRSettings.enabled)
                    {
                        enterDeviceType = EnterDeviceType.VR;
                    }

                    playerPresenter = new PlayerPresenter(PermissionType.Audience, enterDeviceType);
                    avatarRespawner = new AvatarRespawner(despawnHeight, playerPresenter);

                    rankingScreenPresenter.SetRanking(11);
                    var GimmickIdList = venueGimmickManager.GimmickDataList.Select(x => x.Id).ToList();
                    if (GimmickIdList.Count != 0)
                        OnUpdateTriggerIdList?.Invoke(GimmickIdList);
                    break;
            }
        }

        static async Task OnChangePlayModeAsync(PlayModeStateChange playMode)
        {
            if (playMode != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }
            await PackageListRepository.UpdatePackageList();

            if (!PackageListRepository.Contain("openvr") || PlayerPrefs.GetInt(SwitchUseVR.useVRKey) == 0)
            {
                XRSettings.enabled = false;
            }
        }

        public static void SetIsInGameMode(bool value)
        {
            isInGameMode = value;
        }
    }
}
