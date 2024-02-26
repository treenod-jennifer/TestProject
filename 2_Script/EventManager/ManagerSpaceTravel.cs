using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SideIcon;
using UnityEngine;

public class ManagerSpaceTravel : MonoBehaviour, IEventBase
{
    public static ManagerSpaceTravel instance = null;

    public SpaceTravelPackUI     _spaceTravelPackUI     = null;
    public SpaceTravelPackIngame _spaceTravelPackIngame = null;

    // 사과 턴 추가 수
    public readonly int addTurnCount = 3;
    // 레디창 우주복 포코타 스파인 번호
    public readonly TypeCharacterType baseCharacterType = TypeCharacterType.collabo_1064;
    // 연출용 임시 저장 데이터
    public readonly string prevClearIndexKey = "SpaceTravelPrevClearIndex";
    // 이벤트 아이콘
    private IconSpaceTravel _icon;

    public enum BonusItemType
    {
        ADD_TURN,
        LINE_BOMB,
        CIRCLE_BOMB,
        RAINBOW_BOMB,
    }

    public enum StageType
    {
        OPEN,
        CLOSE,
        CLEAR,
        REWARDED,
    }

    public enum PrefabType
    {
        INGAME,
        UI
    }

    // Contents Data
    public int EventIndex { get; private set; }
    private int ResourceIndex { get; set; }
    public long EndTs { get; private set; }
    public int MaxStage { get; private set; }
    public List<List<Reward>> RewardList { get; private set; }
    public int ReadyCharIdx { get; private set; }

    // User Data
    public int CurrentStage { get; private set; }
    public List<int> RewardGetState { get; set; }

    // In game Data
    public Dictionary<BonusItemType, int> selectItemDic = new Dictionary<BonusItemType, int>();
    public bool _useBonusItem = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        UnLoadSpaceTravelResource(PrefabType.INGAME);
        UnLoadSpaceTravelResource(PrefabType.UI);
        
        _cts.Cancel();
        _cts.Dispose();
        NetworkLoading.EndNetworkLoading();
        
        if (instance == this)
        {
            instance = null;
        }
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
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerSpaceTravel>();

        if (instance == null)
        {
            return;
        }

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (ServerContents.SpaceTravelEvent == null || ServerRepos.UserSpaceTravelEvent == null)
        {
            return false;
        }

        if (Global.LeftTime(ServerContents.SpaceTravelEvent.endTs) < 0)
        {
            return false;
        }

        return true;
    }

    private void InitData()
    {
        SyncFromServerContentsData();
        SyncFromServerUserData();
    }

    private void SyncFromServerContentsData()
    {
        EventIndex    = ServerContents.SpaceTravelEvent.eventIndex;
        ResourceIndex = ServerContents.SpaceTravelEvent.resourceIndex;
        EndTs         = ServerContents.SpaceTravelEvent.endTs;
        MaxStage      = ServerContents.SpaceTravelEvent.maxStage;
        RewardList    = ServerContents.SpaceTravelEvent.rewardList;
        ReadyCharIdx  = ServerContents.SpaceTravelEvent.readyCharacterIndex;
    }

    public void SyncFromServerUserData()
    {
        CurrentStage   = ServerRepos.UserSpaceTravelEvent.currentStage;
        RewardGetState = ServerRepos.UserSpaceTravelEvent.rewardGetState;

        _useBonusItem = false;
        selectItemDic.Clear();
        if (ServerRepos.UserSpaceTravelEvent.inGameItem != null)
        {
            for (var i = 0; i < ServerRepos.UserSpaceTravelEvent.inGameItem.Count; i++)
            {
                AddSelectItemDictionary((BonusItemType) i + 1, ServerRepos.UserSpaceTravelEvent.inGameItem[i]);
            }
        }
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconSpaceTravel>(ManagerUI._instance.ScrollbarRight,
                (icon) =>
                {
                    icon.Init(ServerContents.SpaceTravelEvent);
                    _icon = icon;
                });
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        if (!CheckStartable())
        {
            _icon?.SetNewIcon(false);    // 이벤트 시간 오버 후 클릭 시 빨간점 제거
            ManagerUI._instance.OpenPopupEventOver();
            return;
        }

        StartCoroutine(ManagerUI._instance.CoOpenPopupSpaceTravelEvent());
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        // 리소스 할당하지 않고 로드만 진행
        if (instance._spaceTravelPackIngame == null)
        {
            AsyncLoadSpaceTravelResource(PrefabType.INGAME, false).Forget();
        }
    }

    public async UniTask AsyncLoadSpaceTravelResource(PrefabType prefabType, bool isAllocate = true)
    {
        if (Global.LoadFromInternal)
        {
            LoadFromInternal_SpaceTravelPack(prefabType, isAllocate);
        }
        else
        {
            await AsyncLoadFromBundle_SpaceTravelPack(prefabType, isAllocate);
        }
    }

    private void LoadFromInternal_SpaceTravelPack(PrefabType prefabType, bool isAllocate)
    {
#if UNITY_EDITOR
        var directory = prefabType == PrefabType.UI ? $"spaceTravel_UI_{ResourceIndex}" : "spaceTravel_Ingame";
        var bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/5_OutResource/spaceTravel/{directory}/spaceTravelPack.prefab");

        if (isAllocate)
        {
            if (bundleObj.GetComponent<SpaceTravelPackUI>() != null && _spaceTravelPackUI == null)
            {
                _spaceTravelPackUI = bundleObj.GetComponent<SpaceTravelPackUI>();
            }

            if (bundleObj.GetComponent<SpaceTravelPackIngame>() != null && _spaceTravelPackIngame == null)
            {
                _spaceTravelPackIngame = bundleObj.GetComponent<SpaceTravelPackIngame>();
            }
        }
#endif
    }

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private async UniTask AsyncLoadFromBundle_SpaceTravelPack(PrefabType prefabType, bool isAllocate)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        var bundleName  = prefabType == PrefabType.UI ? string.Format($"st_ui_{ResourceIndex}") : string.Format($"st_ingame");
        var assetBundle = await ManagerAssetLoader._instance.AsyncAssetBundleLoader(bundleName, _cts.Token);
        if (isAllocate && assetBundle != null)
        {
            var bundleObj = assetBundle.LoadAsset<GameObject>($"spaceTravelPack");
            if (bundleObj.GetComponent<SpaceTravelPackUI>() != null && _spaceTravelPackUI == null)
            {
                _spaceTravelPackUI = bundleObj.GetComponent<SpaceTravelPackUI>();
            }

            if (bundleObj.GetComponent<SpaceTravelPackIngame>() != null && _spaceTravelPackIngame == null)
            {
                _spaceTravelPackIngame = bundleObj.GetComponent<SpaceTravelPackIngame>();
            }
        }

        NetworkLoading.EndNetworkLoading();
    }
    
    public void UnLoadSpaceTravelResource(PrefabType prefabType)
    {
        if (prefabType == PrefabType.INGAME)
        {
            _spaceTravelPackIngame = null;
        }
        else
        {
            _spaceTravelPackUI = null;
        }
    }
    
    /// <summary>
    /// 스테이지형 이벤트 팝업 강제 노출
    /// </summary>
    public IEnumerator OpenForceDisplayEventPopup()
    {
        if (CheckStartable() == false)
        {
            yield break;
        }

        StartCoroutine(ManagerUI._instance.CoOpenPopupSpaceTravelEvent());
        
        yield return new WaitUntil(() => UIPopUpSpaceTravel.instance != null);
    }

    public StageType GetStageType(int index)
    {
        if (index + 1 < CurrentStage)
        {
            if (RewardGetState[index / 3] == 1)
            {
                return StageType.REWARDED;
            }

            return StageType.CLEAR;
        }

        if (index + 1 == CurrentStage)
        {
            return StageType.OPEN;
        }

        if (index + 1 > CurrentStage)
        {
            return StageType.CLOSE;
        }

        return StageType.CLEAR;
    }

    public void AddSelectItemDictionary(BonusItemType itemType, int itemCount = 1)
    {
        if (selectItemDic.ContainsKey(itemType) == false)
        {
            selectItemDic.Add(itemType, itemCount);
        }
        else
        {
            selectItemDic[itemType] += itemCount;
        }
    }
    
    public bool IsPlayCurrentVersion() => ServerRepos.UserSpaceTravelEventStage != null && ServerRepos.UserSpaceTravelEventStage.FindIndex(x => x.vsn == ServerContents.SpaceTravelEvent.eventIndex) > -1;
    GameEventType IEventBase.GetEventType()      => GameEventType.SPACE_TRAVEL;
    bool IEventBase.         OnLobbySceneCheck() => false;
    bool IEventBase.         OnTutorialCheck()   => false;
    void IEventBase.OnLobbyStart()
    {
        InitData();
        UnLoadSpaceTravelResource(PrefabType.INGAME);
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

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }
}