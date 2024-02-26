using System;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public class ManagerTreasureHunt : MonoBehaviour, IEventBase
{
    public static ManagerTreasureHunt instance = null;
    public TreasureHuntPack treasureHuntPack = null;
    public const string TREASURE_POINT_POS_KEY = "TreasureHuntPointPos";
    public const string TREASURE_HUNT_TUTORIAL_KEY = "TreasureHuntTutorial";
    
    // Contents Data
    public int EventIndex { get; set; }
    public int ReadyCharIdx { get; set; }
    public List<TreasureHuntDecoReward> DecoRewardList { get; set; }    // index : 보상 획득을 위해 필요한 지도 개수, reward : 보상 정보
    public List<Reward> StageRewardList { get; set; }
    public List<int> PriceReadyItem { get; set; }
    public List<int> PriceIngameItem { get; set; }
    public List<int> PriceContinue { get; set; }
    public int DuplicateFlag { get; set; }      // 0 : 통상, 1 : 콜라보, 2 : 콜라보 (두 개의 콜라보 패치 일정이 겹칠 경우)
    public float EndTs { get; set; }

    // User Data
    public List<DecoStatusType> decoStatusList { get; set; } = new List<DecoStatusType>();
    public List<StageStatusType> stageStatusList { get; set; } = new List<StageStatusType>();
    
    // Temp Data
    // 지도 획득 여부를 체크하고 스테이지 번호를 받아와 메인 팝업에서 지도가 날아가는 연출을 재생하기 위한 데이터
    public bool isGetMap = false;

    private IconTreasureHunt icon;
    
    public enum DecoStatusType
    {
        None = 0,       // 획득 불가능
        Get = 1,        // 획득 가능
        Complete = 2,   // 획득 완료
    }
    public enum StageStatusType
    {
        NotClear = 0,   // 클리어 X
        Clear = 1,      // 클리어 O, 지도 획득 X
        Complete = 2,   // 클리어 O, 지도 획득 O
    }

    void Awake()
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

        Global._instance.gameObject.AddMissingComponent<ManagerTreasureHunt>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable() 
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        if (ServerContents.TreasureHunt == null)
            return false;
        
        return Global.LeftTime(ServerContents.TreasureHunt.endTs) > 0;
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.TREASURE_HUNT;
    }
    
    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    void IEventBase.OnLobbyStart()
    {
        if (instance != null && ServerContents.TreasureHunt != null && ServerRepos.UserTreasureHunt != null)
        {
            if (Global.GameType == GameType.TREASURE_HUNT)
                isGetMap = IsGetMap();

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
        if( !CheckStartable() )
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

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield return null;
    }
    
    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconTreasureHunt>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    icon.Init(ServerContents.TreasureHunt);
                    this.icon = icon;
                });
        }
    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserTreasureHunt != null && ServerRepos.UserTreasureHuntStage.FindIndex(x => x.eventIndex == ServerContents.TreasureHunt.eventIndex) == -1)
            return false;
        else
            return true;
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public bool IsShowTutorial()
    {
        return !PlayerPrefs.HasKey(ManagerTreasureHunt.TREASURE_HUNT_TUTORIAL_KEY) &&
               (ServerRepos.UserTreasureHuntStage == null || ServerRepos.UserTreasureHuntStage.Count == 0);
    }
    
    public void OnEventIconClick(object obj = null)
    {
        if (CheckStartable())
        {
            if (IsShowTutorial())
            {
                instance.TutorialCheckAndPlay();
            }
            else
            {
                StartCoroutine(ManagerUI._instance.CoOpenPopupTreasure());
            }   
        }
        else
        {
            icon?.SetNewIcon(false);
                
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
        }
    }

    public void SyncFromServerContentsData()
    {
        EventIndex = ServerContents.TreasureHunt.eventIndex;
        DuplicateFlag = ServerContents.TreasureHunt.resourceIndex;
        ReadyCharIdx = ServerContents.TreasureHunt.readyCharIndex;
        DecoRewardList = ServerContents.TreasureHunt.decoRewards;
        StageRewardList = ServerContents.TreasureHunt.stageClearRewards;
        PriceReadyItem = ServerContents.TreasureHunt.readyItemPrice;
        PriceIngameItem = ServerContents.TreasureHunt.inGameItemPrice;
        PriceContinue = ServerContents.TreasureHunt.continueItemPrice;
        EndTs = ServerContents.TreasureHunt.endTs;
    }
    
    public void SyncFromServerUserData()
    {
        decoStatusList.Clear();
        foreach (var item in ServerRepos.UserTreasureHunt.decoRewardState)
            decoStatusList.Add((DecoStatusType)item);
        
        stageStatusList.Clear();
        foreach (var item in ServerRepos.UserTreasureHunt.stageClearState)
            stageStatusList.Add((StageStatusType)item);
    }

    public int GetStageProgress()
    {
        for (int i = 0; i < stageStatusList.Count; i++)
        {
            if (stageStatusList[i] == StageStatusType.NotClear)
            {
                return i;
            }
        }

        return stageStatusList.Count;
    }
    
    public int GetMapCount()
    {
        int count = 0;
        for (int i = 0; i < stageStatusList.Count; i++)
        {
            if (stageStatusList[i] == StageStatusType.Complete)
            {
                count++;
            }
        }

        return count;
    }

    public bool IsGetMap()
    {
        // 조건 1 : 클리어 전 스테이지 상태가 2 미만 (미클리어, 지도 미획득)
        // 조건 2 : 클리어 후 스테이지 상태가 2 이상 (지도 획득)
        return stageStatusList[Global.stageIndex - 1] < StageStatusType.Complete &&
               (StageStatusType) ServerRepos.UserTreasureHunt.stageClearState[Global.stageIndex - 1] == StageStatusType.Complete;
    }
    
    public IEnumerator LoadTreasureHuntResource()
    {
        if (Global.LoadFromInternal)
            LoadFromInternal_TreasureHuntPack();
        else
            yield return LoadFromBundle_TreasureHuntPack();
    }

    private void LoadFromInternal_TreasureHuntPack()
    {
#if UNITY_EDITOR
        string bundleName = string.Format($"treasureHunt_{DuplicateFlag}");
        string path = "Assets/5_OutResource/treasureHunt/" + bundleName + $"/treasureHuntPack.prefab";
        GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (bundleObj.GetComponent<TreasureHuntPack>() != null)
            treasureHuntPack = bundleObj.GetComponent<TreasureHuntPack>();
#endif
    }

    private IEnumerator LoadFromBundle_TreasureHuntPack()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string bundleName = string.Format($"th_event_{DuplicateFlag}");
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"treasureHuntPack");
                if (bundleObj.GetComponent<TreasureHuntPack>() != null)
                    treasureHuntPack = bundleObj.GetComponent<TreasureHuntPack>();
            }
        }
        NetworkLoading.EndNetworkLoading();
    }

    public void TutorialCheckAndPlay()
    {
        if (ManagerLobby.landIndex != 0)
            ManagerLobby._instance.MoveMainLand(() => { StartCoroutine(CoTutorialPlay()); });
        else
            StartCoroutine(CoTutorialPlay());
    }

    private IEnumerator CoTutorialPlay()
    {
        ManagerTutorial.PlayTutorial(TutorialType.TutorialTreasureHunt);
        yield return new WaitUntil(() => ManagerTutorial._instance == null);
        StartCoroutine(ManagerUI._instance.CoOpenPopupTreasure());
        
        PlayerPrefs.SetInt(ManagerTreasureHunt.TREASURE_HUNT_TUTORIAL_KEY, 1);
    }
    
    /// <summary>
    /// 스테이지형 이벤트 팝업 강제 노출
    /// </summary>
    public IEnumerator CoOpenForceDisplayEventPopup()
    {
        if (CheckStartable() == false)
        {
            yield break;
        }
        
        yield return ManagerUI._instance.CoOpenPopupTreasure();
    }
}
