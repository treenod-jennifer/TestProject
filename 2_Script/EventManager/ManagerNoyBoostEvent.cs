using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ManagerNoyBoostEvent : MonoBehaviour, IEventBase
{
    public static ManagerNoyBoostEvent instance = null;
    public NoyBoostPack_UI NoyBoostPackUI = null;
    public NoyBoostPack_Ingame NoyBoostPackIngame = null;

    // Contents Data
    public int EventIndex { get; private set; }
    public int StartStage { get; private set; }
    public int EndStage { get; private set; }
    public List<int> ComboList { get; private set; }
    public long EndTs { get; private set; }

    // User Data
    public int ComboStep { get; set; }
    
    // 레디 팝업 > 인게임으로 넘어갈 때 해당 스테이지의 부스팅 적용 여부 (다른 곳에서는 사용 절대 X)
    // 턴 추가 광고 시청, 로딩 등으로 스타트 버튼 클릭 시점에는 부스팅이 적용되어 있었으나 인게임 진입 직후에는 부스팅이 적용되지 않는 이슈 방지
    public bool isBoostOn = false;
    

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        UnLoadNoyBoostResource(PrefabType.Ingame);
        UnLoadNoyBoostResource(PrefabType.UI);

        cts.Cancel();
        cts.Dispose();
        NetworkLoading.EndNetworkLoading();
        
        if (instance == this)
            instance = null;
    }

    public void OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }
    
    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerNoyBoostEvent>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    /// <summary>
    /// 이벤트 시작 가능한지
    /// </summary>
    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
            return false;
        
        if (ServerContents.NoyBoostEvent == null || ServerRepos.UserNoyBoostEvent == null)
            return false;
        
        if (Global.LeftTime(ServerContents.NoyBoostEvent.endTs) < 0)
            return false;

        return true;
    }

    /// <summary>
    /// 이벤트 참여 가능한 유저인지 : 시간 체크 관련해서는 해당 함수 사용 X (최신 스테이지가 아닐 경우 플레이 불가 처리가 진행되기 때문)
    /// </summary>
    public bool IsActiveUser()
    {
        if (!CheckStartable())
            return false;

        if (GameData.User.stage < StartStage || GameData.User.stage > EndStage)
            return false;

        if (Global.stageIndex != SDKGameProfileManager._instance.GetMyProfile().stage)
            return false;

        return true;
    }

    /// <summary>
    /// 현재 부스트 단계 획득
    /// </summary>
    public int GetBoostStep()
    {
        int boostStep = 1;
        for (int i = 0; i < ComboList.Count; i++)
        {
            if (ComboStep < ComboList[i])
                return boostStep;
            boostStep += 1;
        }
        return boostStep;
    }

    private void InitData()
    {
        SyncFromServerContentsData();
        SyncFromServerUserData();
    }

    private void SyncFromServerContentsData()
    {
        EventIndex = ServerContents.NoyBoostEvent.eventIndex;
        StartStage = ServerContents.NoyBoostEvent.startStage;
        EndStage = ServerContents.NoyBoostEvent.endStage;
        ComboList = ServerContents.NoyBoostEvent.stageClearCombos;
        EndTs = ServerContents.NoyBoostEvent.endTs;
    }
    
    public void SyncFromServerUserData()
    {
        ComboStep = ServerRepos.UserNoyBoostEvent.comboStep;
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.NOY_BOOST;
    }

    void IEventBase.OnLobbyStart()
    {
        InitData();
        UnLoadNoyBoostResource(PrefabType.Ingame);
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield break;
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        //부스팅이벤트.
        if (CheckStartable() && GameData.User.stage >= StartStage && GameData.User.stage <= EndStage)
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconNoyBoostEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.NoyBoostEvent));
        }
    }
    
    public void OnEventIconClick(object obj = null)
    {
        if (CheckStartable())
            ManagerUI._instance.AsyncOpenPopupNoyBoostEvent().Forget();
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false,
                    null);
            });
        }
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        if (IsActiveUser())
        {
            // 리소스 할당하지 않고 로드만 진행
            if (instance.NoyBoostPackIngame == null)
                AsyncLoadNoyBoostResource(PrefabType.Ingame, false).Forget();
        }
    }

    public enum PrefabType
    {
        Ingame,
        UI
    }

    public async UniTask AsyncLoadNoyBoostResource(PrefabType prefabType, bool isAllocate = true)
    {
        if (Global.LoadFromInternal)
            LoadFromInternal_NoyBoostPack(prefabType, isAllocate);
        else
            await AsyncLoadFromBundle_NoyBoostPack(prefabType, isAllocate);
    }
    
    public void UnLoadNoyBoostResource(PrefabType prefabType)
    {
        if (prefabType == PrefabType.Ingame)
            NoyBoostPackIngame = null;
        else
            NoyBoostPackUI = null;
    }

    private void LoadFromInternal_NoyBoostPack(PrefabType prefabType, bool isAllocate)
    {
#if UNITY_EDITOR
        string path = $"Assets/5_OutResource/noyBoost/NoyBoost_{prefabType}/NoyBoostPack.prefab";
        GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (isAllocate)
        {
            if (bundleObj.GetComponent<NoyBoostPack_UI>() != null && NoyBoostPackUI == null)
                NoyBoostPackUI = bundleObj.GetComponent<NoyBoostPack_UI>();
            if (bundleObj.GetComponent<NoyBoostPack_Ingame>() != null && NoyBoostPackIngame == null)
                NoyBoostPackIngame = bundleObj.GetComponent<NoyBoostPack_Ingame>();
        }
#endif
    }

    private CancellationTokenSource cts = new CancellationTokenSource();
    private async UniTask AsyncLoadFromBundle_NoyBoostPack(PrefabType prefabType, bool isAllocate)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string bundleName = string.Format($"n_boost_{prefabType.ToString().ToLower()}");
        AssetBundle assetBundle = await ManagerAssetLoader._instance.AsyncAssetBundleLoader(bundleName, cts.Token);
        if (isAllocate && assetBundle != null)
        {
            GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"NoyBoostPack");
            if (bundleObj.GetComponent<NoyBoostPack_UI>() != null && NoyBoostPackUI == null)
                NoyBoostPackUI = bundleObj.GetComponent<NoyBoostPack_UI>();
            if (bundleObj.GetComponent<NoyBoostPack_Ingame>() != null && NoyBoostPackIngame == null)
                NoyBoostPackIngame = bundleObj.GetComponent<NoyBoostPack_Ingame>();
        }
        NetworkLoading.EndNetworkLoading();
    }
}
