using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 로그인 이벤트 개선 매니저
/// </summary>
public class ManagerLoginBonusRenewal : MonoBehaviour, IEventBase
{
    public static ManagerLoginBonusRenewal Instance { get; private set; }

    public NGUIAtlas atlas = null;
    
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        if (ServerContents.LoginBonusRenewal == null)
            return false;

        return Global.LeftTime(ServerContents.LoginBonusRenewal.endTs) > 0;
    }

    public static void Init()
    {
        if (Instance != null)
            return;

        if (!CheckStartable())
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerLoginBonusRenewal>();

        if (Instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(Instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.RENEWAL_LOGIN_EVENT;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList,
        Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public void OnLobbyStart()
    {
    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    public IEnumerator OnRewardPhase()
    {
        if (ManagerLobby._firstLogin && ServerContents.LoginBonusRenewal != null)
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
            
            bool loginWaitFlag = true;
            bool loginDataExist = false;
            
            ServerAPI.LoginBonusRenewal((x) => {
                loginWaitFlag = false;
                loginDataExist = x.userLoginEventRenewal != null;
            });
            
            while (loginWaitFlag)
            {
                yield return null;
            }
            
            SendGrowthyLog();
            
            if (loginDataExist && CanGetReward())
            {
                if (atlas == null)
                {
                    yield return CoLoadAssetBundle();
                }
                NetworkLoading.EndNetworkLoading();
                
                yield return ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupLoginBonusRenewal>((popup) => popup.InitPopup());
                yield return new WaitUntil(() => UIPopupLoginBonusRenewal.instance == null);
            }
            else
            {
                NetworkLoading.EndNetworkLoading();
            }
        }
    }

    // 보상 획득 가능한 상태인지 여부.
    private bool CanGetReward()
    {
        if (ServerRepos.UserLoginBonusRenewal == null ||
            ServerRepos.UserLoginBonusRenewal.getCumulativeLoginReward == null ||
            ServerRepos.UserLoginBonusRenewal.getContinuousLoginReward == null ||
            ServerContents.LoginBonusRenewal.cumulativeLoginRewardData == null)
        {
            return false;
        }

        bool getAllRewards = true;
        // 누적 보상 획득 가능한지(누적 보상의 타겟 누적 접속일 수 조건을 충족하고, 보상을 획득 하지 않았을 경우 true)
        for (var index = 0; index < ServerRepos.UserLoginBonusRenewal.getCumulativeLoginReward.Count; index++)
        {
            if (ServerContents.LoginBonusRenewal.cumulativeLoginRewardData.Count <= index ||
                ServerContents.LoginBonusRenewal.cumulativeLoginRewardData[index].targetCount > ServerRepos.UserLoginBonusRenewal.cumulativeLoginCount)
            {
                getAllRewards = false;
                break;
            }
            if (ServerRepos.UserLoginBonusRenewal.getCumulativeLoginReward[index] == false)
            {
                return true;
            }
        }

        // 연속 보상 획득 가능한지(연속 보상의 연속 접속일을 충족하고, 보상을 획득하지 않았을 경우 true)
        for (var index = 0; index < ServerRepos.UserLoginBonusRenewal.getContinuousLoginReward.Count; index++)
        {
            if (ServerRepos.UserLoginBonusRenewal.getContinuousLoginReward[index] == false)
            {
                getAllRewards = false;
                if ((index + 1) <= ServerRepos.UserLoginBonusRenewal.continuousLoginMostCount)
                {
                    return true;
                }
            }
        }

        // 처음 접속 할 경우 무조건 출력.
        return ServerRepos.UserLoginBonusRenewal.firstAccess && getAllRewards == false;
    }

    public void OnIconPhase()
    {
    }

    public void OnReboot()
    {
        if (Instance != null)
            Destroy(Instance);
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    private static void SendGrowthyLog()
    {
        if (ServerRepos.UserLoginBonusRenewal == null)
        {
            return;
        }

        if (ServerRepos.UserLoginBonusRenewal.firstAccess)
        {
            SendAchievementGrowthy(UIPopupLoginBonusRenewal.LoginBonusRewardType.CUMULATIVE, ServerRepos.UserLoginBonusRenewal.cumulativeLoginCount);
            
            // 운영 일자별 연속 로그인 조건에 달성한 유저의 정확한 현황 파악 목적, 단계별 최초 카운트만 집계.
            if (ServerRepos.UserLoginBonusRenewal.firstContinuousLoginFlag)
            {
                SendAchievementGrowthy(UIPopupLoginBonusRenewal.LoginBonusRewardType.CONTINUOUS, ServerRepos.UserLoginBonusRenewal.continuousLoginCount);
            }
        }
    }
    
    private static void SendAchievementGrowthy(UIPopupLoginBonusRenewal.LoginBonusRewardType type, int count)
    {
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.LOGIN_EVENT,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.LOGIN_EVENT_POPUP_OPEN,
            type == UIPopupLoginBonusRenewal.LoginBonusRewardType.CUMULATIVE?
                $"LOGIN_ACCUMULATE_EVENT_{count}":
                $"LOGIN_CONTINUOUS_EVENT_{count}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }

    private IEnumerator CoLoadAssetBundle()
    {
        string assetName = $"login_bonus_renewal_{ServerContents.LoginBonusRenewal.resourceIndex}";

        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path =
                $"Assets/5_OutResource/LoginBonusRenewal/{assetName}/LoginBonusRenewalAtlas.asset";
            atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<NGUIAtlas>(path);

            if (atlas == null)
            {
                Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
                yield break;
            }
#endif
        }
        else
        {
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
            while (e.MoveNext())
            {
                yield return e.Current;
            }

            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    atlas = assetBundle.LoadAsset<NGUIAtlas>("LoginBonusRenewalAtlas");
                    yield return null;
                }
            }
        }

        yield return null;
    }
}