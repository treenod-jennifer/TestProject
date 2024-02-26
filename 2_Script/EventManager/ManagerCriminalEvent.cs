using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManagerCriminalEvent : MonoBehaviour, IEventBase
{
    public const string CRIMINAL_CLEAR_KEY = "CriminalEventClearShow";

    public static ManagerCriminalEvent instance = null;
    public CriminalEventPack criminalEventPack = null;


    // Contents Data
    public int eventIndex { get; set; }
    public int resourceIndex { get; set; }

    public int stageCount { get; set; }

    public int stageGap { get; set; }
    public List<Reward> rewardList { get; set; } = new List<Reward>();
    public List<Reward> finalRewardList { get; set; } = new List<Reward>();
    public List<int> animalList { get; set; } = new List<int>();
    public long endTs { get; set; }

    // User Data
    public List<int> stages { get; set; } = new List<int>();
    public List<int> stageClear { get; set; } = new List<int>();
    public List<int> stageReward { get; set; } = new List<int>();
    public bool isAllClearReward { get; set; }
    
    [NonSerialized] public bool isPlayCriminalEventStage = false;
    
    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerCriminalEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
            return false;

        if (ServerContents.CriminalEvent == null || ServerRepos.UserCriminalEvent == null)
            return false;

        return Global.LeftTime(ServerContents.CriminalEvent.endTs) >= 0 &&
               Global.LeftTime(ServerContents.CriminalEvent.startTs) <= 0;
    }

    public GameEventType GetEventType()
    {
        return GameEventType.CRIMINAL_EVENT;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList,
        Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public void OnLobbyStart()
    {
        if (instance != null && ServerContents.CriminalEvent != null && ServerRepos.UserCriminalEvent != null)
        {
            instance.SyncFromServerContentsData();
            instance.SyncFromServerUserData();
        }
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
        yield return null;
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
        yield return null;
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public void OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconCriminalEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.CriminalEvent));
        }
    }

    public void OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
        if (CheckStartable())
        {
            ManagerUI._instance.OpenPopupCriminalEvent();
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
        }
    }

    private void SyncFromServerContentsData()
    {
        eventIndex = ServerContents.CriminalEvent.vsn;
        resourceIndex = ServerContents.CriminalEvent.resourceIndex;
        endTs = ServerContents.CriminalEvent.endTs;
        stageCount = ServerContents.CriminalEvent.stageCount;
        stageGap = ServerContents.CriminalEvent.stageGap;

        rewardList.Clear();
        foreach (var item in ServerContents.CriminalEvent.stageReward)
            rewardList.Add(item);
        finalRewardList.Clear();
        foreach (var item in ServerContents.CriminalEvent.stageAllClearReward)
            finalRewardList.Add(item);
        animalList.Clear();
        foreach (var item in ServerContents.CriminalEvent.animals)
            animalList.Add(item);
    }

    public void SyncFromServerUserData()
    {
        stages.Clear();
        foreach (var item in ServerRepos.UserCriminalEvent.stages)
            stages.Add(item);
        stageClear.Clear();
        foreach (var item in ServerRepos.UserCriminalEvent.stageClear)
            stageClear.Add(item);
        stageReward.Clear();
        foreach (var item in ServerRepos.UserCriminalEvent.stageReward)
            stageReward.Add(item);
        isAllClearReward = ServerRepos.UserCriminalEvent.isAllClearReward == 1;
    }

    /// <summary>
    /// 스테이지가 클리어가 되었는지 확인하는 함수
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool IsStageClear(int index)
    {
        return stageClear[index] == 1;
    }

    /// <summary>
    /// 현재 목표로 하는 스테이지 인덱스 /Sync 되지 않은 데이터를 사용해서 확인
    /// </summary>
    public int GetEventStep()
    {
        return stageClear.Count(t => t == 1);
    }

    /// <summary>
    /// 현재 목표로 하는 스테이지 인덱스 / 서버 데이터를 사용해서 확인
    /// </summary>
    /// <returns></returns>
    public int GetEventStep_ServerData()
    {
        return ServerRepos.UserCriminalEvent.stageClear.Count(t => t == 1);
    }

    /// <summary>
    /// 현재 유저가 클리어해야 될 스테이지
    /// </summary>
    /// <returns></returns>
    public int GetTargetStage()
    {
        var count = stageClear.Count(t => t == 1);

        return stages.Count == count ? 0 : stages[count];
    }

    /// <summary>
    /// 현재 스테이지를 클리어 하고 클리어 한 스테이지가 현재 타겟 스테이지인지 구분하는 함수.
    /// 인게임 내에서만 동작
    /// </summary>
    /// <returns></returns>
    public bool IsClearStage()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGame" && GameManager.instance.CurrentStage == GetTargetStage();
    }

    public bool IsAllStageClear()
    {
        return ServerRepos.UserCriminalEvent.stageClear != null &&
               ServerRepos.UserCriminalEvent.stageClear.All(t => t != 0);
    }

    public bool IsGetReward()
    {
        if (ServerRepos.UserCriminalEvent.stageClear.All(clear => clear == 1))
        {
            if (ServerRepos.UserCriminalEvent.isAllClearReward == 0)
                return true;
        }

        var count = ServerRepos.UserCriminalEvent.stageClear.FindIndex(index => index == 0);

        if (count < 0) count = ServerRepos.UserCriminalEvent.stageClear.Count;

        for (int i = 0; i < count; i++)
        {
            if (ServerRepos.UserCriminalEvent.stageReward[i] == 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 유저가 모든 보상을 받았을 때
    /// </summary>
    /// <returns></returns>
    public bool IsGetAllReward()
    {
        return ServerRepos.UserCriminalEvent.isAllClearReward == 1 && ServerRepos.UserCriminalEvent.stageReward.All(t => t == 1);
    }

    public IEnumerator LoadCriminalEventResource()
    {
        if (Global.LoadFromInternal)
            LoadFromInternal_CriminalEventPack();
        else
            yield return LoadFromBundle_CriminalEventPack();
    }

    private void LoadFromInternal_CriminalEventPack()
    {
#if UNITY_EDITOR
        string bundleName = string.Format($"criminal_event_{resourceIndex}");
        string path = "Assets/5_OutResource/criminalEvent/" + bundleName + $"/CriminalEventPack.prefab";
        GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (bundleObj.GetComponent<CriminalEventPack>() != null)
            criminalEventPack = bundleObj.GetComponent<CriminalEventPack>();
#endif
    }

    private IEnumerator LoadFromBundle_CriminalEventPack()
    {
        // 이미 번들을 받았으면 밑에 과정 생략
        if (criminalEventPack != null) yield break;

        NetworkLoading.MakeNetworkLoading(0.5f);
        string bundleName = string.Format($"criminal_event_{resourceIndex}");
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"CriminalEventPack");
                if (bundleObj.GetComponent<CriminalEventPack>() != null)
                    criminalEventPack = bundleObj.GetComponent<CriminalEventPack>();
            }
        }

        NetworkLoading.EndNetworkLoading();
    }
    
    public static bool IsCriminalStagePlaying()
    {
        if (instance == null)
        {
            return false;
        }
        
        return instance.isPlayCriminalEventStage;
    }
}