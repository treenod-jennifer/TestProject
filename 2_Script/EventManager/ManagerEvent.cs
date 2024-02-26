using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerEvent : MonoBehaviour
{
    static public ManagerEvent instance = null;
    public bool isTutorialPlay = false;
    public bool isCutScenePlay = false;

    SortedDictionary<GameEventType, IEventBase> events = new SortedDictionary<GameEventType, IEventBase>();
    
    List<IEventBase> introSceneEvents = new List<IEventBase>();
    List<IEventBase> tutorialEvents = new List<IEventBase>();

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if(instance == this)
            instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterEvent( IEventBase ev)
    {
        events.Add( ev.GetEventType(), ev);
    }

    public static void Init()
    {
        if (instance == null)
        {
            Global._instance.gameObject.AddMissingComponent<ManagerEvent>();
            if (instance == null)
                return;

            instance.InitEvents();
        }
    }

    public void InitEvents()
    {
        ManagerLoginEvent.Init();
        ManagerLoginBonusRenewal.Init();
        ManagerEventStage.Init();
        ManagerMoleCatch.Init();
        ManagerStageChallenge.Init();
        ManagerCoinBonusStage.Init();
        ManagerWorldRanking.Init();
        ManagerPokoFlowerEvent.Init();
        ManagerSpecialEvent.Init();
        ManagerAlphabetEvent.Init();
        ManagerStickerEvent.Init();
        ManagerAdventureEvent.Init();
        ManagerTurnRelay.Init();
        ManagerCapsuleGachaEvent.Init();
        ManagerWelcomeEvent.Init();
        ManagerCoinStashEvent.Init();
        ManagerDiaStash.Init();
        ManagerStageAssistMissionEvent.Init();
        ManagerWelcomeBackEvent.Init();
        ManagerLoginADBonus.Init();
        ManagerDecoCollectionEvent.Init();
        ManagerEventQuest.Init();
        ManagerPremiumPass.Init();
        ManagerEndContentsEvent.Init();
        ManagerBingoEvent.Init();
        ManagerTreasureHunt.Init();
        ManagerAntiqueStore.Init();
        ManagerSingleRoundEvent.Init();
        ManagerAdventurePass.Init();
        ManagerCriminalEvent.Init();
        ManagerIntegratedEvent.Init();
        ManagerOfferwall.Init();
        ManagerOfferwallTapjoy.Init();
        ManagerLuckyRoulette.Init();
        ManagerAtelier.Init();
        ManagerNoyBoostEvent.Init();
        ManagerGroupRanking.Init();
        ManagerForceDisplayEvent.Init();
        ManagerSpaceTravel.Init();
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        foreach (var e in events)
        {
            e.Value.OnBundleLoadPhase(loadList, failCallback);
        }
    }

    public void OnLobbyStart()
    {
        InitEvents();

        foreach (var e in events)
        {
            e.Value.OnLobbyStart();
        }

    }

    public IEnumerator OnPostLobbyEnter()
    {
        foreach (var e in events)
        {
            yield return e.Value.OnPostLobbyEnter();
        }

    }

    public IEnumerator OnRewardPhase()
    {
        if (ManagerLobby.landIndex != 0)
            yield break;
        foreach (var e in this.events)
        {
            yield return e.Value.OnRewardPhase();
        }

    }

    public void OnLobbySceneCheck()
    {
        //로비 이동 시 랜드와 무관하게 컷 씬 플레이 여부를 초기화 시킵니다.
        isCutScenePlay = false;
        
        if (ManagerLobby.landIndex != 0)
            return;

        //이전에 재생되지 못한채로 등록된 이벤트가 있다면 초기화시킴.
        introSceneEvents.Clear();
        foreach (var e in this.events)
        {
            if (e.Value.OnLobbySceneCheck())
            {
                this.introSceneEvents.Add(e.Value);
            }
        }
        if (introSceneEvents.Count > 0)
            isCutScenePlay = true;
    }

    public bool OnTutorialCheck()
    {
        tutorialEvents.Clear();

        bool tutorialExist = false;
        if (ManagerLobby.landIndex != 0)
            return false;

        foreach (var e in this.events)
        {
            if (e.Value.OnTutorialCheck())
            {
                this.tutorialEvents.Add(e.Value);
                tutorialExist = true;
            }
        }
        return tutorialExist;
    }

    public IEnumerator OnLobbyMakeFinished()
    {
        if (ManagerLobby.landIndex != 0)
        {
            yield break;
        }

        // 만약 이벤트가 꺼져있는 경우에도 어떤 처리를 해줘야하는 경우, 여기다가 스태틱함수를 실행시켜주면 됨
        // 경우가 잘 없으므로 일단 이렇게 처리
        // C# 8 스펙에서는 static interface 가 추가되므로 그때 처리하는 것도 방법
        yield return ManagerMoleCatch.OnLobbyLoadDisabled();
    }

    public IEnumerator OnLobbyScenePhase()
    {
        if (ManagerLobby.landIndex != 0)
        {
            yield break;
        }
            
        foreach (var e in this.introSceneEvents)
        {
            yield return e.OnLobbyScenePhase();
        }
        introSceneEvents.Clear();
    }
    public IEnumerator OnLobbyEventTutorialPhase()
    {
        if (ManagerLobby.landIndex != 0)
            yield break;

        isTutorialPlay = false;

        foreach (var e in tutorialEvents)
        {
            yield return e.OnTutorialPhase();
        }
        isTutorialPlay = false;

        tutorialEvents.Clear();
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        if (ManagerLobby.landIndex != 0)
        {
            foreach (var e in this.events)
            {
                yield return e.Value.OnLobbyObjectLoadPhase_Outland();
            }
            yield break;
        }
        else
        {
            foreach (var e in this.events)
            {
                yield return e.Value.OnLobbyObjectLoadPhase();
            }
        }
    }

    public void OnIconPhase()
    {
        foreach (var e in this.events)
        {
            e.Value.OnIconPhase();
        }
    }

    public static void OnReboot()
    {
        if( instance == null )
            return;

        foreach( var e in instance.events)
        {
            e.Value.OnReboot();
        }

        if(instance != null )
            Destroy(instance);

    }

}
