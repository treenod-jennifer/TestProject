using JsonFx.Json;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public partial class ManagerTurnRelay : MonoBehaviour, IEventBase
{
    static public ManagerTurnRelay instance = null;
    
    //로비 아이콘 관련
    private IconTurnRelayEvent icon;
    private Coroutine          coSetNewIcon;

    //인게임에서 사용하는 데이터
    static public TurnRelayIngame turnRelayIngame;

    //리소스 데이터
    static public TurnRelayResource turnRelayResource;

    //서브미션 데이터
    static public TurnRelaySubmission turnRelaySubMission;

    //협동 데이터
    static public TurnRelayCooperation turnRelayCoop;

    #region 게임 데이터
    public int MaxEventAP { get; set; } = 3;
    public int MaxWaveCount { get; set; } = 5;
    public int EventIndex { get; set; }
    public int ResourceIndex { get; set; }
    public long PlayEndTs { get; set; }
    public long BeforeRewardEndTs { get; set; }
    public long RewardEndTs { get; set; }
    public long APTime { get; set; }
    public int APPrice { get; set; } = 200;
    #endregion

    #region 유저 데이터
    public int EventScore { get; set; }
    public int EventAP { get; set; }
    public long APRechargeTime { get; set; }
    #endregion

    public enum EventState
    {
        NOT_STARTED,        // 이벤트 시작하지 않음
        NEED_CREATE_PROFILE,// 프로필 생성 필요
        RUNNING,            // 경쟁중
        BEFORE_REWARD,      // 보상 집계기간
        REWARD,             // 보상기간
        EVENT_END,          // 이벤트 종료
    }
    
    private void Awake()
    {
        instance = this;

        turnRelayIngame = new TurnRelayIngame();
        turnRelayResource = new TurnRelayResource();
        turnRelaySubMission = new TurnRelaySubmission();
        turnRelayCoop = new TurnRelayCooperation();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
        SetNewIcon();
        OpenTurnRelay();
    }

    //이벤트 상태 가져오기
    public static EventState GetEventState()
    {
        //서버에 이벤트 데이터 조차 없는 경우
        if (ServerContents.TurnRelayEvent == null || ServerContents.TurnRelayEvent.eventIndex == 0) 
            return EventState.NOT_STARTED;

        //유저 미션 진행도 체크
        if (ServerRepos.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return EventState.NOT_STARTED;

        //이벤트 시작 시간 체크
        if (Global.GetTime() < ServerContents.TurnRelayEvent.startTs)
            return EventState.NOT_STARTED;

        #region 기간에 따른 이벤트 처리
        if(ManagerTurnRelay.IsRankingEnabled() == false && 
            Global.LeftTime(ServerContents.TurnRelayEvent.startTs + ServerContents.TurnRelayEvent.runningPeriod) <= 0)
        {
            return EventState.EVENT_END;
        }

        if (Global.LeftTime(ServerContents.TurnRelayEvent.startTs + ServerContents.TurnRelayEvent.runningPeriod + ServerContents.TurnRelayEvent.countingPeriod + ServerContents.TurnRelayEvent.rewardPeriod) <= 0)
            return EventState.EVENT_END;
        else if (Global.LeftTime(ServerContents.TurnRelayEvent.startTs + ServerContents.TurnRelayEvent.runningPeriod + ServerContents.TurnRelayEvent.countingPeriod) <= 0)
            return EventState.REWARD;
        else if (Global.LeftTime(ServerContents.TurnRelayEvent.startTs + ServerContents.TurnRelayEvent.runningPeriod) <= 0 )
           return EventState.BEFORE_REWARD;
        #endregion

        if (ServerRepos.UserTurnRelayEvent == null || ServerRepos.UserTurnRelayEvent.stage < 1)
            return EventState.NEED_CREATE_PROFILE;
        
        //위의 경우에 해당하지 않으면 이벤트 진행 가능
        return EventState.RUNNING;
    }

    //현재 이벤트가 열려있는지 검사.
    public static bool IsActiveEvent()
    {
        if (instance == null)
            return false;

        EventState checkState = GetEventState();
        return (checkState != EventState.NOT_STARTED);
    }

    public static bool IsRankingEnabled()
    {
        if (ServerContents.TurnRelayEvent == null)
            return false;
        if (string.IsNullOrEmpty(ServerContents.TurnRelayEvent.tableIdEntry))
            return false;
        if (ServerContents.TurnRelayEvent.tableIdEntry.Length < 2)
            return false;
        return true;
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerTurnRelay>();
        
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public void InitData()
    {
        SyncFromServerContentsData();
        SyncFromServerUserData();
        turnRelayIngame.SyncFromServerContentsData_Ingame();
        turnRelayCoop.SyncFromServerContentsData();
    }

    public void SyncFromServerContentsData()
    {
        CdnTurnRelayEvent cdnTurnRelayEvent = ServerContents.TurnRelayEvent;
        MaxEventAP = cdnTurnRelayEvent.maxAp;
        MaxWaveCount = cdnTurnRelayEvent.score.Count;
        EventIndex = cdnTurnRelayEvent.eventIndex;
        ResourceIndex = cdnTurnRelayEvent.resourceIndex;
        PlayEndTs = cdnTurnRelayEvent.startTs + cdnTurnRelayEvent.runningPeriod;
        BeforeRewardEndTs = PlayEndTs + cdnTurnRelayEvent.countingPeriod;
        RewardEndTs = BeforeRewardEndTs + cdnTurnRelayEvent.rewardPeriod;
        APTime = cdnTurnRelayEvent.apRechargeInterval;
        APPrice = cdnTurnRelayEvent.apPrice;
    }

    public void SyncFromServerUserData()
    {
        if (ServerRepos.UserTurnRelayEvent == null)
            return;

        ServerUserTurnRelayEvent userData = ServerRepos.UserTurnRelayEvent;
        EventScore = userData.scoreStacked;
        EventAP = userData.apCount;
        APRechargeTime = userData.apRechargeAt;
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        CdnTurnRelayEvent cdnTurnRelayEvent = ServerContents.TurnRelayEvent;
        if (cdnTurnRelayEvent == null || cdnTurnRelayEvent.eventIndex == 0)
            return false;

        long eventEndTs = cdnTurnRelayEvent.startTs + cdnTurnRelayEvent.runningPeriod + cdnTurnRelayEvent.countingPeriod + cdnTurnRelayEvent.rewardPeriod;
        return Global.LeftTime(eventEndTs) > 0;
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.TURN_RELAY;
    }

    void IEventBase.OnLobbyStart()
    {
        if (ManagerTurnRelay.IsActiveEvent())
        {
            ManagerTurnRelay.instance.InitData();
        }
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield return ManagerTurnRelay.turnRelayResource.LoadTurnRelayResource();

        var areaObject = ManagerTurnRelay.turnRelayResource.GetAreaObject();
        if (areaObject != null)
            ManagerLobby.NewObject(areaObject);

        AreaBase areaBase = (areaObject == null) ? null : areaObject.GetComponent<AreaBase>();
        if (areaBase != null)
            ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);

        yield return ManagerCharacter._instance.LoadCharacters();
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin() == false
            && ServerContents.TurnRelayEvent != null
            && ServerRepos.UserTurnRelayEvent != null)
        {
            var eventState = ManagerTurnRelay.GetEventState();

            //보상 받을 수 있는 상황에서, 턴 이월 팝업 출력
            if (ManagerTurnRelay.IsRankingEnabled() &&
                ServerRepos.UserTurnRelayEvent.rankEntryTime > 0 &&
                ServerRepos.UserTurnRelayEvent.coopRankRewardFlag == false &&
                eventState == ManagerTurnRelay.EventState.REWARD)
            {
                //보상창이 뜨도록 설정
                //ManagerUI._instance.OpenPopup<UIPopupTurnRelay_StageReady>((popup) => popup.InitPopup());
                ManagerUI._instance.OpenPopup_Description<UIPopupTurnRelay_StageReady>((popup) => popup.InitPopup(), null, true);
                yield return new WaitUntil(() => UIPopupTurnRelay_StageReady._instance == null);
            }
        }
    }

    void IEventBase.OnIconPhase()
    {
        if (IsActiveEvent())
        {
            var eventState = ManagerTurnRelay.GetEventState();

            bool participated = false;
            if (ServerRepos.UserTurnRelayEvent != null && ServerRepos.UserTurnRelayEvent.play >= 1)
                participated = true;

            if (eventState > ManagerTurnRelay.EventState.RUNNING && participated == false)
                return;

            SideIcon.Maker.MakeIcon<SideIcon.IconTurnRelayEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    icon.Init(ServerContents.TurnRelayEvent);
                    this.icon = icon;
                    SetNewIcon();
                });
        }
    }


    public void SetNewIcon()
    {
        if (coSetNewIcon != null)
        {
            StopCoroutine(coSetNewIcon);
            coSetNewIcon = null;
        }

        if (icon == null)
        {
            return;
        }

        if (IsActiveEventNewIcon() == true)
        {
            long remainingTime = Global.LeftTime(APRechargeTime);
            if (EventAP > 0 || remainingTime <= 0)
            {
                icon.SetNewIcon(true);
            }
            else
            {
                icon.SetNewIcon(false);
                coSetNewIcon = StartCoroutine(CoApTimer());
            }
        }
        else
        {
            icon.SetNewIcon(false);
        }
    }

    private bool IsActiveEventNewIcon()
    {
        var state = GetEventState();
        return (state == EventState.RUNNING || state == EventState.NEED_CREATE_PROFILE);
    }
    
    IEnumerator CoApTimer()
    {
        long remainingTime = Global.LeftTime(APRechargeTime);

        while (remainingTime > 0)
        {
            remainingTime--;
            yield return new WaitForSeconds(1.0f);
        }
        
        SetNewIcon();
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        ManagerTurnRelay.SetPlayCutScene(ManagerTurnRelay.instance.EventIndex);
        yield return ManagerLobby._instance.WaitForSceneEnd(ManagerArea._instance.GetEventLobbyObject(GameEventType.TURN_RELAY).GetAreaBase(), 1);
    }

    static public void SetPlayCutScene(int eventIndex)
    {
        PlayerPrefs.SetInt(TurnRelayResource.TURNRELAY_CUTSCENE_PLAYED, eventIndex);
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        if (ManagerArea._instance.GetEventLobbyObject(GameEventType.TURN_RELAY) != null && ManagerTurnRelay.IsPlayCutScene(ManagerTurnRelay.instance.EventIndex))
        {
            return true;
        }
        return false;
    }

    static public bool IsPlayCutScene(int eventIndex)
    {
        //이벤트 러닝중이 아니면 컷씬을 보여주지 않는다.
        var state = GetEventState();
        if (state != EventState.RUNNING && state != EventState.NEED_CREATE_PROFILE) return false;

        //플레이 기록이 있으면 컷씬 재생하지 않음.
        if (ServerRepos.UserTurnRelayEvent?.play > 0)
        {
            SetPlayCutScene(eventIndex);
        }

        if (PlayerPrefs.HasKey(TurnRelayResource.TURNRELAY_CUTSCENE_PLAYED))
        {
            int played = PlayerPrefs.GetInt(TurnRelayResource.TURNRELAY_CUTSCENE_PLAYED);

            return !(played == eventIndex);
        }
        else
        {
            return true;
        }
    }

    public static void OpenTurnRelay()
    {
        if (ManagerLobby._instance._state != TypeLobbyState.Wait || UIPopupTurnRelay_StageReady._instance != null)
        {
            return;
        }

        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            ManagerUI._instance.GuestLoginSignInCheck();
            return;
        }

        var eventState = ManagerTurnRelay.GetEventState();
        switch (eventState)
        {
            case ManagerTurnRelay.EventState.RUNNING:
                {
                    //진행중인 데이터가 있었는지에 따라 팝업 설정
                    if (ServerRepos.UserTurnRelayEvent.state == true || ServerRepos.UserTurnRelayEvent.stage > 1)
                    {
                        ManagerTurnRelay.turnRelayIngame.SyncFromServerUserData_Ingame();
                        ManagerUI._instance.OpenPopup<UIPopupTurnRelay_ExistData>((popup) => popup.InitPopup());
                    }
                    else
                    {
                        Global.SetGameType_TurnRelay(ServerContents.TurnRelayEvent?.eventIndex ?? 1, 1);
                        ManagerUI._instance.OpenPopup_Description<UIPopupTurnRelay_StageReady>((popup) => popup.InitPopup());
                    }
                }
                break;
            case ManagerTurnRelay.EventState.NEED_CREATE_PROFILE:
                {
                    var profilePopup = ManagerUI._instance.OpenPopup<UIPopupPionProfile>();
                    profilePopup.Init(PionProfileContentType.TURN_RELAY, () =>
                    {
                        ServerRepos.UserTurnRelayEvent.stage = 1;
                        OpenTurnRelay();
                    });
                }
                break;
            case ManagerTurnRelay.EventState.REWARD:
                {
                    ManagerUI._instance.OpenPopup_Description<UIPopupTurnRelay_StageReady>((popup) => popup.InitPopup());
                }
                break;
            case ManagerTurnRelay.EventState.BEFORE_REWARD:
                {
                    ManagerUI._instance.OpenPopup<UIPopupSystem>
                    (
                        (popup) =>
                        {
                            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_12"), false);
                        }
                    );
                }
                break;
            case ManagerTurnRelay.EventState.EVENT_END:
                {
                    ManagerUI._instance.OpenPopup<UIPopupSystem>
                    (
                        (popup) =>
                        {
                            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false);
                        }
                    );
                }
                break;
        }
    }

    #region 인터페이스에서 사용하지 않는 함수
    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }
    #endregion
}