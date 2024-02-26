using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCoinBonusStage : MonoBehaviour, NPCHost, IEventBase {

    public static ManagerCoinBonusStage instance = null;
    public const string OutgameTutorialKey = "CoinStageTutorial_Outgame";

    public class CoinBonusStageQuest
    {
        public int type;
        public int state;
        public int progress;
        public int targetCount;
    }

    int currentStage = 0;

    CoinBonusStageQuest questState = new CoinBonusStageQuest();
    Dictionary<int, int> storedBestScore = new Dictionary<int, int>();
    [System.NonSerialized]
    UILobbyCoinStageCounter _objEventObjectUI = null;
    [System.NonSerialized]
    GameObject progressUIRoot = null;

    public static int EventIndex
    {
        get { return (ServerContents.CoinBonusStage != null && instance != null) ? ServerContents.CoinBonusStage.event_index : 1; }
    }

    static public bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        if( ServerContents.CoinBonusStage != null && Global.LeftTime(ServerContents.CoinBonusStage.end_ts) > 0)
            return true;

        return false;
    }

    #region 코인 스테이지 컨티뉴 관련

    public static bool CheckStartable_Continue()
    {
        if (ManagerCoinBonusStage.instance == null)
            return false;
        
        if(ServerContents.CoinStageDiaContinue == null || ServerContents.CoinStageDiaContinue.Count == 0)
            return false;

        return true;
    }

    public int GetContinueRewardTime()
    {
        if (IsCoinStageContinueCountOver()) return 0;
        
        return ServerContents.CoinStageDiaContinue[ManagerCoinBonusStage.instance.GetStageIndex()]
            .continuePriceMap[GameManager.instance.useContinueCount].extraTime;
    }

    public int GetContinuePrice()
    {
        if (IsCoinStageContinueCountOver()) return 0;
        
        return ServerContents.CoinStageDiaContinue[ManagerCoinBonusStage.instance.GetStageIndex()]
            .continuePriceMap[GameManager.instance.useContinueCount].price;
    }

    public bool IsCoinStageContinueCountOver()
    {
        if (CheckStartable_Continue() == false) return false;
        
        return GameManager.instance.useContinueCount >= ServerContents.CoinStageDiaContinue[ManagerCoinBonusStage.instance.GetStageIndex()].continuePriceMap.Count;
    }
    
    #endregion
    
    public static void Init()
    {
        if( !CheckStartable())
            return;
        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerCoinBonusStage>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public void Awake()
    {
        instance = this;
    }

    public void OnDestroy()
    {
        if( instance == this)
            instance = null;
    }

    public void Update()
    {
        if(progressUIRoot != null && ManagerLobby._instance != null)
        {
            var c = ManagerLobby._instance.GetCharacter((TypeCharacterType)22);
            if( c != null )
                progressUIRoot.transform.position = c.gameObject.transform.position;
        } 
    }

    public static bool IsActiveEvent()
    {
        if( ServerContents.CoinBonusStage == null )
            return false;

        if( Global.LeftTime(ServerContents.CoinBonusStage.end_ts) <= 0 )
            return false;

        if (instance == null)
            return false;

        if ( ServerRepos.UserCoinBonusStage.openQuestState == 2)
        {
            System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            System.DateTime rebootTime = System.DateTime.Today.AddHours(ServerRepos.LoginCdn.LoginOffset);
            rebootTime = rebootTime.AddDays(1);
            rebootTime = rebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);

            System.TimeSpan diff = rebootTime - origin;
            double time = System.Math.Floor(diff.TotalSeconds);

            if( ServerContents.CoinBonusStage.end_ts <= time )
            {
                return false;
            }
        }

        return true;
    }

    public int GetStageIndex()
    {
        return currentStage;
    }

    public int GetMapIndex()
    {
        return ServerRepos.UserCoinBonusStage.maps[currentStage];
    }

    public bool IsDayPassed()
    {
        if( ServerRepos.UserCoinBonusStage == null)
            return false;

        return Global.IsDayPassed( ServerRepos.UserCoinBonusStage.created_time );
    }

    public CoinBonusStageQuest GetCurrentQuestState()
    {
        var ret = new CoinBonusStageQuest();
        questState.CopyAllTo(ret);
        return ret;
    }

    public int GetCurrentBestScore()
    {
        return ServerRepos.UserCoinBonusStage.bestScore[currentStage];
    }

    public int GetStoredBestScore()
    {
        return storedBestScore[currentStage];

    }

    public static IEnumerator OnLobbyMake()
    {
        if( !IsActiveEvent() )
            yield break;
    }


    public IEnumerator OnPostLobbyEnter()
    {
        if( !IsActiveEvent() )
            yield break;

        instance.SyncFromServer();

        if (ManagerLobby.landIndex != 0)
            yield break;

        var costumeIdx = instance.GetCurrentQuestState().state == 1 ? 0 : 1;

        List<CharCostumePair> costumePairList = new List<CharCostumePair>()
        {
            new CharCostumePair() { character = (TypeCharacterType)22,  costumeIdx= 0 },
            new CharCostumePair() { character = (TypeCharacterType)22,  costumeIdx= 1 }
        };

        ManagerCharacter._instance.AddLoadList(null, costumePairList, null);
        yield return ManagerCharacter._instance.LoadCharacters();

        ManagerAIAnimal.instance.RegisterNPC(22, costumeIdx, ManagerCoinBonusStage.instance);
    }

    public void SyncFromServer()
    {
        currentStage = ServerRepos.UserCoinBonusStage.current_stage;

        questState = new CoinBonusStageQuest()
        {
            type = ServerRepos.UserCoinBonusStage.openQuest,
            progress = ServerRepos.UserCoinBonusStage.openQuestProgress,
            targetCount = ServerRepos.UserCoinBonusStage.openQuestCount,
            state = ServerRepos.UserCoinBonusStage.openQuestState
        };
        storedBestScore = new Dictionary<int, int>(ServerRepos.UserCoinBonusStage.bestScore);
    }

    public void OnNPCCreated(LobbyAnimalAI ai)
    {
        var qState = this.GetCurrentQuestState();

        int counterCurrent = ServerRepos.UserCoinBonusStage.maps.Count - (ServerRepos.UserCoinBonusStage.current_stage - 1);
        if (qState.state == 2)
        {
            counterCurrent--;
        }

        GameObject ui = Instantiate<GameObject>(Resources.Load("UIPrefab/UILobbyCoinStageCounter") as GameObject);
        progressUIRoot = new GameObject();
        ui.transform.parent = progressUIRoot.transform;
        ui.transform.localPosition = new Vector3(3f, 4.3f, -3f);
        progressUIRoot.transform.position = ai.gameObject.transform.position;
        _objEventObjectUI = ui.GetComponent<UILobbyCoinStageCounter>();

        _objEventObjectUI.SetCounter(counterCurrent);
    }

    public void OnNPCDestroyed(LobbyAnimalAI ai)
    {
        DestroyNPCLinkedObjects();
    }

    void DestroyNPCLinkedObjects()
    {
        if( progressUIRoot != null )
        {
            this._objEventObjectUI = null;
            Destroy(progressUIRoot);
            progressUIRoot = null;
        }
    }

    public void OnNPCTap(LobbyAnimalAI ai)
    {
        if (ServerRepos.UserCoinBonusStage.openQuestState == 2 && Global.IsDayPassed(ServerRepos.UserCoinBonusStage.created_time))
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_19"), false, null);
            popupSystem.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
            popupSystem.FunctionSetting(3, "OnApplicationPauseReboot", this.gameObject);
        }
        else
        {
            Global.SetGameType_CoinBonusStage(ServerRepos.UserCoinBonusStage.event_index, ServerRepos.UserCoinBonusStage.current_stage);
            instance.StartCoroutine(ManagerUI._instance.CoCheckStageDataBeforeOpenPopUpCoinStageReady());
        }
    }

    void OnApplicationPauseReboot()
    {
        if (UIPopupSystem._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }

    public void OnReboot()
    {
        if( instance != null )
        {
            instance.DestroyNPCLinkedObjects();
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        Global.SetGameType_CoinBonusStage(ServerRepos.UserCoinBonusStage.event_index, ServerRepos.UserCoinBonusStage.current_stage);
        StartCoroutine(ManagerUI._instance.CoCheckStageDataBeforeOpenPopUpCoinStageReady());
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.COIN_BONUS;
    }

    void IEventBase.OnLobbyStart()
    {
        if (ManagerCoinBonusStage.IsActiveEvent())
        {
            ManagerCoinBonusStage.instance.SyncFromServer();
        }
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        ManagerEvent.instance.isTutorialPlay = true;

        PlayerPrefs.SetInt(ManagerCoinBonusStage.OutgameTutorialKey, 2);
        ManagerTutorial.PlayTutorial(TutorialType.TutorialCoinStageOutgame);
        if (ManagerTutorial._instance != null)
            yield return new WaitUntil(() => UIPopupReadyCoinStage.instance == null && ManagerTutorial._instance._playing == false );
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        if (IsActiveEvent())
        {
            int maxPlayCount = ServerRepos.UserCoinBonusStage.maps.Count;
            int playCount = GetCurrentQuestState()?.state == 2 ? maxPlayCount : ServerRepos.UserCoinBonusStage.current_stage - 1;
            
            var eventData = new SideIcon.CoinBonusStageEvent(EventIndex, playCount, maxPlayCount);
            SideIcon.Maker.MakeIcon<SideIcon.IconCoinBonusStageEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(eventData));
        }
    }

    bool IEventBase.OnTutorialCheck()
    {
        if (ManagerCoinBonusStage.IsActiveEvent() && PlayerPrefs.HasKey(ManagerCoinBonusStage.OutgameTutorialKey) == false)
        {
            if (ManagerCoinBonusStage.instance.GetStageIndex() > 1)
            {
                PlayerPrefs.SetInt(ManagerCoinBonusStage.OutgameTutorialKey, 2);
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserCoinBonusStage != null && ServerRepos.UserCoinBonusStage.current_stage == 1)
            return false;
        else
            return true;
    }
}
