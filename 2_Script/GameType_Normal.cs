using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;

public class GameType_Normal : GameType_Base
{
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT]                      = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT]        = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT]       = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE]            = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED]          = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.FLOWER_ON_READY]               = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.FLOWER_ON_INGAME]              = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_USE_READYITEM]             = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE]                  = this.CanContinue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE]            = () => { return this.CanRetry(false); };
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL]    = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS]             = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM]    = GameType_Base.ReturnTrue;
    }

    public override void InitStage()
    {
        // HACK: 옛날에 이상하게 코인보너스스테이지 아닌데 WAITSTATE == 0 으로 저장되어있던 맵들이 있어서, 다 찾기도 어렵고 해서 여기서 일단 정리해줌
        // 근데 언젠가는 시간제 스테이지를 다른 모드에서도 쓸 수 있으니 이거에 의존할 게 아니라 언젠가는 다 정리해줘야함
        if (ManagerBlock.instance.stageInfo.waitState == 0)
            ManagerBlock.instance.stageInfo.waitState = 1;

        SpecialEventProcess_OnIngameStart(GameManager.instance);
        AlphabetEventProcess_OnIngameStart();
    }

    void SpecialEventProcess_OnIngameStart(GameManager gm)
    {
        if (Global.specialEventIndex == 0)
            return;

        foreach (var item in ServerContents.SpecialEvent)
        {
            if (item.Value.index == Global.specialEventIndex)
            {
                int getCount = 0;

                foreach (var itemUser in ServerRepos.UserSpecilEvents)
                {
                    if (itemUser.eventIndex == Global.specialEventIndex)
                    {
                        getCount = itemUser.progress;
                        gm.specialEventSect = itemUser.rewardSection;
                    }
                }

                int maxGetCount = item.Value.sections[item.Value.sections.Count - 1];

                if (getCount < maxGetCount)
                {
                    maxGetCount -= getCount;

                    var myProfile = SDKGameProfileManager._instance.GetMyProfile();

                    //유저의 최조위치를 기준으로 
                    float ratioAppear = ((float)Global.stageIndex / (float)myProfile.stage) * 0.8f;
                    if (((float)Global.stageIndex / (float)myProfile.stage) > 0.9f) ratioAppear = 0.8f;

                    if (maxGetCount < item.Value.maxCount * (0.2f + ratioAppear))
                        gm.specialEventMaxCount = maxGetCount;
                    else
                        gm.specialEventMaxCount = (int)(item.Value.maxCount * (0.2f + ratioAppear));

                    gm.specialEventAppearMaxCount = item.Value.appearMaxCount;
                    gm.specialEventPorb = System.Convert.ToSingle(item.Value.probability, CultureInfo.InvariantCulture);

                    //관련이미지 받기
                    ResourceManager.LoadCDN
                    (
                        Global.gameImageDirectory,
                        "IconEvent/",
                        $"sEventBlock_{Global.specialEventIndex}.png",
                        (Texture2D texture) => ResourceManager.UnLoad(texture)
                    );
                }
            }
        }
    }

    void AlphabetEventProcess_OnIngameStart()
    {
        if (GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == false)
            return;

        if (ManagerAlphabetEvent.instance == null)
            return;

        if (ManagerAlphabetEvent.instance.isUser_eventComplete == true)
        {
            ManagerAlphabetEvent.alphabetIngame.InitIngameData_Default();
            return;
        }

        //알파벳 이벤트 적용 가능하다면, 서버에서 데이터 받아와서 초기화 시켜줌.
        if (ManagerAlphabetEvent.instance.IsAlphabetEventStage_GameStartTime() == true)
            ManagerAlphabetEvent.alphabetIngame.InitIngameData();
    }
    
    bool flowerLevelUpdatedOnClear = false;

    override public void SetSelectReadyItem() 
    {
        if ((IsCanPlayTutorialStage(UIPopupReady.OPEN_ADD_TURN_STAGE, (int)READY_ITEM_TYPE.ADD_TURN) == true)
            || (IsCanPlayTutorialStage(UIPopupReady.OPEN_SCORE_UP_STAGE, (int)READY_ITEM_TYPE.SCORE_UP) == true))
        {
            UIPopupReady._instance.ReleaseAllReadyItem();
        }
    }

    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return Global.stageIndex >= UIPopupReady.OPEN_ADD_TURN_STAGE;
            case 1: return Global.stageIndex >= UIPopupReady.OPEN_SCORE_UP_STAGE;
            case 2: return Global.stageIndex >= UIPopupReady.OPEN_RANDOM_BOMB;
            case 3: return Global.stageIndex >= UIPopupReady.OPEN_LINE_BOMB_STAGE;
            case 4: return Global.stageIndex >= UIPopupReady.OPEN_CIRCLE_BOMB_STAGE;
            case 5: return Global.stageIndex >= UIPopupReady.OPEN_RAINBOW_BOMB_STAGE;
            case 6: return (CanUseDoubleReadyItem() == false ? false : Global.stageIndex >= UIPopupReady.OPEN_DOUBLE_ADD_TURN_STAGE);
            case 7: return (CanUseDoubleReadyItem() == false ? false : Global.stageIndex >= UIPopupReady.OPEN_DOUBLE_SCORE_UP_STAGE);
            default: return false;
        }
    }

    override public bool CanUseDoubleReadyItem()
    {
        if (ServerRepos.LoginCdn == null
            || (ServerRepos.LoginCdn.DoubleReadyItems == null || ServerRepos.LoginCdn.DoubleReadyItems.Length == 0))
        {
            return false;
        }

        return true;
    }

    public override bool CanUseIngameItem(GameItemType itemIndex)
    {
        switch (itemIndex)
        {
            case GameItemType.NONE:                return true;
            case GameItemType.HAMMER:              return Global.stageIndex >= GameItemManager.HAMMER_OPEN_STAGE;
            case GameItemType.CROSS_LINE:          return Global.stageIndex >= GameItemManager.CROSS_LINE_OPEN_STAGE;
            case GameItemType.THREE_HAMMER:        return Global.stageIndex >= GameItemManager.HAMMER3X3_OPEN_STAGE;
            case GameItemType.RAINBOW_BOMB_HAMMER: return Global.stageIndex >= GameItemManager.RAINBOW_HAMMER_OPEN_STAGE;
            case GameItemType.COLOR_BRUSH:         return Global.stageIndex >= GameItemManager.COLOR_BRUSH_OPEN_STAGE;
            default:                               return false;
        }
    }

    override public ScoreFlowerType GetMaxType_FlowerScore()
    {
        //현재 챕터 클리어 상태 받아옴(0: 아직 챕터 덜깨짐, 1: 모두 흰꽃 이상, 2.모두 파란꽃 이상)
        int flowerClearState = 0;
        if (GameManager.instance.currentChapter() <= ServerRepos.UserChapters.Count - 1)
            flowerClearState = ServerRepos.UserChapters[GameManager.instance.currentChapter()].clearState;

        #region 에코피(꽃 피우기) 이벤트 검사
        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent_Ingame() == true) //신버전 에코피 이벤트
        {
            //현재 스테이지의 꽃 상태
            ScoreFlowerType prevStageFlower = (ScoreFlowerType)ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;

            //해당 스테이지 파란 꽃 이상 피운 상태이고 현재 에피소드의 파란꽃을 전부 피웠는지
            if (prevStageFlower >= ScoreFlowerType.FLOWER_BLUE && flowerClearState > 1)
                return ScoreFlowerType.FLOWER_RED;

            //해당 스테이지 흰 꽃 이상 피운 상태일 때
            if (prevStageFlower >= ScoreFlowerType.FLOWER_WHITE)
                return ScoreFlowerType.FLOWER_BLUE;
        }
        #endregion

        //현재 챕터에서 흰꽃을 모두 달성했다면, 파란꽃이 최대
        if (flowerClearState > 0)
            return ScoreFlowerType.FLOWER_BLUE;

        //그 외의 경우, 모두 흰꽃이 최대
        return ScoreFlowerType.FLOWER_WHITE;
    }

    //override public ServerStageBase GetStageBase()
    //{
    //    return ServerRepos.FindUserStage(Global.stageIndex, Global.eventIndex);
    //}

    override public int GetStageVer()
    {
        return ManagerData._instance.StageVersionList[Global.stageIndex - 1];
    }

    bool CanContinue()
    {
        // 일반 모드에서는, 부스팅 이벤트 일 경우에서 무한 컨티뉴 가능.
        if (ServerRepos.LoginCdn.ContinueMax == 0 || ServerRepos.LoginCdn.ContinueMax > GameManager.instance.useContinueCount)
        {
            return true;
        }
        return false;
    }

    bool CanRetry(bool isClear)
    {
        if (isClear == true)
        {
            //알파벳 이벤트 진행중인 경우 검사.
            if (GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == true && ManagerAlphabetEvent.instance != null
                && ManagerAlphabetEvent.instance.IsAlphabetEventStage_GameStartTime() == true)
            {
                //보상을 받았다면 재플레이 불가(로비에서 연출을 봐야함)
                if (PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_N) 
                    || PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_S))
                    return false;
            }
        }

        return true;
    }

    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.STAR | (int)FirstClearRewardType.CLOVER;
    }

    public override void OnPopupInit_Continue(UIPopupContinue popup)
    {
        base.OnPopupInit_Continue(popup);
        StartCoroutine(CoOnPopupInit_Continue(popup));
    }

    //인게임의 스페셜이벤트 배율이 정해질때까지(애니메이션이 끝날때 까지) 기다린 후 팝업을 세팅한다.
    private IEnumerator CoOnPopupInit_Continue(UIPopupContinue popup)
    {
        if (ManagerBlock.instance.getSpecialEventBlock > 0)
        {
            float waitingTime = 0.0f;
            while (!GameUIManager.instance.isCompleteSpecialEventSettings)
            {
                waitingTime += Time.unscaledDeltaTime;
                if (waitingTime > 5.0f)
                    break;

                yield return null;
            }
        }
            
        popup.SettingBoniDialogBubbleNormal();
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        this.flowerLevelUpdatedOnClear = false;

        StageData dataLocal = ManagerData._instance._stageData[Global.stageIndex - 1];
        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);

        //첫 스테이지 클리어
        if (dataLocal._flowerLevel == 0)
        {
            this.flowerLevelUpdatedOnClear = true;
            Global.UpdateNormalGameProgress((int)ServerRepos.User.stage);

            ManagerLobby._stageClear = true;
            //MAT로그 전송
            ServiceSDK.AdjustManager.instance.OnStageClear(Global.stageIndex - 1);
        }
        //재시도 후 스테이지 클리어.
        else
        {
            // 이전단계보다 높아졌다면 그로시 플래그 남김
            if (dataLocal._flowerLevel < dataServer.flowerLevel)
            {
                this.flowerLevelUpdatedOnClear = true;
                
                //꽃을 피웠고, 이전 시도보다 더 높은 단계로 클리어 한 경우 연출 나오도록 키 추가.
                if (dataServer.flowerLevel >= 3)
                {
                    if (PlayerPrefs.HasKey("ActionFlowerState") == false)
                        PlayerPrefs.SetInt("ActionFlowerState", dataLocal._flowerLevel);
                }
            }
        }
        Global.flower = (int)GameData.User.flower;

        dataLocal._flowerLevel = dataServer.flowerLevel;
        dataLocal._score = dataServer.score;
        dataLocal._missionProg1 = dataServer.mprog1;
        dataLocal._missionProg2 = dataServer.mprog2;
        dataLocal._missionClear = dataServer.missionClear;
        dataLocal._play = dataServer.play;

        //파란꽃 피웠으면 보상 데이터 갱신시켜줌.
        //(2020.04.23) 3.4.0 패치로 흰꽃, 포코꽃 피우기 보상 추가로 조건 변경.
        if (dataLocal._flowerLevel > 2)
        {
            ManagerData._instance.RefreshFlowerRewardData();
            //UI 갱신.
            if (ManagerData._instance.listGetFlowersReward.Count > 0)
            {
                ManagerUI._instance.SettingStageNewIcon(true);
            }
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if (myProfile.stage < 10)
        {
            QuestGameData quest = null;
            if (ManagerData._instance._questGameData.TryGetValue(1, out quest))
            {
                if (quest.state == QuestState.Completed)
                    ManagerLobby._tutorialQuestComplete_Play = true;
            }
        }

        if (Global.specialEventIndex > 0)
        {
            for (int i = 0; i < ServerRepos.UserSpecilEvents.Count; i++)
            {
                if (ServerRepos.UserSpecilEvents[i].eventIndex == Global.specialEventIndex)
                {
                    if (gm.specialEventSect < ServerRepos.UserSpecilEvents[i].rewardSection)
                    {
                        Global.flower = (int)GameData.User.flower;
                        PlayerPrefs.SetInt("ShowSpeicalEventPopup", 1);
                        UIPopupClear.materialEventGetReward = true;
                        gm.specialEventSect = ServerRepos.UserSpecilEvents[i].rewardSection;
                    }
                    break;
                }
            }
        }

        //부스팅 이벤트
        GameManager.instance.RefreshBoostingUserData();
    }

    public override void OnRecvGameFail()
    {
        StageData dataLocal = ManagerData._instance._stageData[Global.stageIndex - 1];
        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
        dataLocal._play = dataServer.play;
        dataLocal._fail = dataServer.fail;
        //부스팅 이벤트
        GameManager.instance.RefreshBoostingUserData();
    }

    public override void OnRecvGameRestart()
    {
        StageData dataLocal = ManagerData._instance._stageData[Global.stageIndex - 1];
        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
        dataLocal._play = dataServer.play;
        dataLocal._fail = dataServer.fail;
        //부스팅 이벤트
        GameManager.instance.RefreshBoostingUserData();
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL;
    }

    public override int GetGrowthyPlayCount()
    {
        // Global.stageIndex 는 첫클리어 상황인 경우, 그로시에 넣는 시점에 실제 플레이한 스테이지 +1 상태가 되어있으므로 사용불가
        // 그래서 GameManager.instance.CurrentStage 를 사용한다
        ServerUserStage dataServer = ServerRepos.FindUserStage(GameManager.instance.CurrentStage);
        return dataServer?.play ?? 0;
    }

    public override string GetGrowthyStageIndex()
    {
        return GameManager.instance.CurrentStage.ToString();
    }

    override public bool IsFirstPlay()
    {
        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
        if (dataServer == null)
            return true;
        else if (dataServer.play == 1)
            return true;
        return false;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);
    }

    public override void SendClearGrowthyLog()
    {
        base.SendClearGrowthyLog_Regular(this.flowerLevelUpdatedOnClear);
    }
    
    public override int GetGrowthy_PLAYEND_L_NUM3()
    {
        return Global.eventIndex;
    }
    
    //게임 시작시, 그로씨에 대한 처리
    public override string GetGrowthy_PLAYSTART_DETAILINFO()
    {
        var integratedEventData = new Dictionary<string, object>();

        // 코코의 수사일지
        if (ManagerCriminalEvent.instance != null && Global.GameType == GameType.NORMAL &&
            ServerRepos.UserCriminalEvent.stages.Count > ManagerCriminalEvent.instance.GetEventStep_ServerData() && //ServerRepos.UserCriminalEvent.stages Argument에러 방지 조건 
            ServerRepos.UserCriminalEvent.stages[ManagerCriminalEvent.instance.GetEventStep_ServerData()] == Global.stageIndex)
        {
            integratedEventData.Add("L_CRIMINAL_EVENT", ManagerCriminalEvent.instance.GetEventStep());
        }
        
        // 수문장 크루그
        if (Global.isSingleRoundEvent)
        {
            integratedEventData.Add("L_SINGLE_ROUND_EVENT", true);
        }
        
        // 그룹랭킹
        if (ManagerGroupRanking.IsGroupRankingStage())
        {
            integratedEventData.Add("L_GROUP_RANKING_EVENT_ACTIVE", true);
        }

        return GetJsonData(integratedEventData);
    }

    //게임 클리어 후, 보상받는 그로씨에 대한 처리
    public override void SendClearRewardGrowthyLog(GameClearRespBase resp = null)
    {
        Protocol.GameClearResp clearResp =
            (GameManager.instance.clearResp == null) ? null : GameManager.instance.clearResp as GameClearResp;

        //챕터 미션 클리어 그로씨
        SendClearRewardGrowthy_ChapterMissionClear(clearResp);

        // 스페셜이벤트 그로씨
        SendClearRewardGrowthy_SpecialEvent(clearResp);

        //알파벳 이벤트 그로씨
        SendClearRewardGrowthy_AlphabetEvent(clearResp);

        //에코피(꽃 피우기) 그로씨
        SendClearRewardGrowthy_PokoFlowerEvent(clearResp);

        //부스팅 그로씨
        SendClearRewardGrowthy_BoostEvent(clearResp);

        // 통상 스테이지 독려 이벤트 그로시
        SendClearRewardGrowthy_StageAssistMission(clearResp);

        // 마유지의 도전장 이벤트 그로시
        SendClearRewardGrowthy_StageChallenge(clearResp);

        //모든 스테이지 클리어 그로씨
        SendClearRewardGrowthy_AllStageClear();

        //앤틱스토어 이벤트
        SendClearRewardGrowthy_AntiqueStore(clearResp);
        
        //수문장 크루그 이벤트 그로시
        SendClearRewardGrowthy_SingleRound(clearResp);
        
        //코코의 수사일지 이벤트
        SendClearRewardGrowthy_CriminalEvent(clearResp);

        //GameType_Base 그로씨 추가
        base.SendClearRewardGrowthyLog();
    }

    #region 인게임 클리어 보상 그로씨
    private void SendClearRewardGrowthy_ChapterMissionClear(GameClearResp clearResp)
    {
        if (clearResp == null || clearResp.chapterMissionCleared == null)
            return;

        foreach (var r in clearResp.chapterMissionCleared.rewardList)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                (int)r.type,
                (int)r.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_QUEST_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                "q_" + clearResp.chapterMissionCleared.index.ToString()
                );
        }

        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.QUEST,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.HOBBY_TIME,
            "q_" + clearResp.chapterMissionCleared.index.ToString(),
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }

    private void SendClearRewardGrowthy_SpecialEvent(GameClearResp clearResp)
    {
        if (ServerContents.SpecialEvent == null || clearResp == null || clearResp.specialEvent == null)
            return;

        if (Global.specialEventIndex == 0 || UIPopupClear.materialEventGetReward == false)
            return;

        var userSpecialEvent = ServerRepos.UserSpecilEvents.Find(x => x.eventIndex == Global.specialEventIndex);
        if (userSpecialEvent == null)
            return;

        //리워드받기 그로씨
        foreach (var temp in ServerContents.SpecialEvent)
        {
            if (temp.Value.index == Global.specialEventIndex)
            {
                int rwSection = userSpecialEvent.rewardSection - 1;

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPECIAL_EVENT,
                    $"SPECIAL_EVENT_{Global.specialEventIndex}_{userSpecialEvent.rewardSection}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                achieve.L_NUM1 = 0;
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

                foreach (var rewards in temp.Value.rewards[rwSection])
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        rewards.type,
                        rewards.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "SpeicalEvent" + Global.specialEventIndex + "Reward" + userSpecialEvent.rewardSection
                        );
                }
            }
        }

        // 보상을 이번에 받았는데, 섹션이 전체보다 같거나 크면 이번에 최종보상을 받은 상황이라 볼 수 있음
        if( ServerContents.SpecialEvent[Global.specialEventIndex].sections.Count <= userSpecialEvent.rewardSection)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPECIAL_EVENT,
                $"SPECIAL_EVENT_{Global.specialEventIndex}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            achieve.L_NUM1 = 0;
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

        }
    }

    private void SendClearRewardGrowthy_AlphabetEvent(GameClearResp clearResp)
    {
        if (ServerContents.AlphabetEvent == null || clearResp == null || clearResp.alphabetEvent == null
            || clearResp.alphabetEvent.rewardReceived == null || clearResp.alphabetEvent.rewardReceived.Count == 0)        
            return;

        for (int i = 0; i < clearResp.alphabetEvent.rewardReceived.Count; ++i)
        {
            int rwIndex = clearResp.alphabetEvent.rewardReceived[i];
            if (rwIndex > 0 && rwIndex - 1 < ServerContents.AlphabetEvent.reward.Count)
            {
                var rwList = ServerContents.AlphabetEvent.reward[rwIndex - 1];
                for (int j = 0; j < rwList.Count; ++j)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        rwList[j].type,
                        rwList[j].value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "ALPHABET_EVENT");
                }

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ALPHABET_EVENT,
                $"ALPHABET_EVENT_{ManagerAlphabetEvent.instance.eventIndex}_{rwIndex}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
                achieve.L_NUM1 = ServerRepos.UserAlphabetEvents.playCount;
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }
        }

        bool allClear = true;
        for (int i = 0; i < clearResp.alphabetEvent.reward.Length; ++i)
        {
            if (clearResp.alphabetEvent.reward[i] == 0)
                allClear = false;
        }
        if (clearResp.alphabetEvent.specialStatus != null && clearResp.alphabetEvent.specialReward == 0)
            allClear = false;

        if (allClear)
        {

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ALPHABET_EVENT,
                $"ALPHABET_EVENT_{ManagerAlphabetEvent.instance.eventIndex}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            achieve.L_NUM1 = ServerRepos.UserAlphabetEvents.playCount;
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
    }

    private void SendClearRewardGrowthy_PokoFlowerEvent(GameClearResp clearResp)
    {
        if (ServerContents.PokoFlowerEvent == null || clearResp == null ||
            clearResp.pokoFlower == null || clearResp.pokoFlower.get_reward != 1)
            return;

        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
               ServerContents.PokoFlowerEvent.reward.type,
                ServerContents.PokoFlowerEvent.reward.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                "POKOFLOWER_EVENT"
                );

        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.POKOFLOWER_EVENT,
               "POKOFLOWER_EVENT",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }

    private void SendClearRewardGrowthy_BoostEvent(GameClearResp clearResp)
    {
        if (ServerContents.BoostEvent == null || clearResp == null || clearResp.boostEvent == null )
            return;
        
        var userBoostEvent = ServerRepos.UserBoostEvents.Find(x => { return x.event_index == ServerContents.BoostEvent.event_index; });
        if (userBoostEvent != null && clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.BOOST_EVENT) == true)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    ServerContents.BoostEvent.rewards[userBoostEvent.rewardCount - 1].type,
                    ServerContents.BoostEvent.rewards[userBoostEvent.rewardCount - 1].value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BOOST_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BOOST_REWARD,
                    $"BOOSTING_EVENT_{userBoostEvent.rewardCount}"
                );

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.BOOSTING,
                $"BOOSTING_EVENT_{userBoostEvent.rewardCount}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
        
    }

    private void SendClearRewardGrowthy_StageAssistMission(GameClearResp clearResp)
    {
        if (ServerContents.StageAssistMissionEvent == null || clearResp == null || clearResp.stageAssistMission == null
            || clearResp.clearRewards == null || clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.STAGE_ASSIST_MISSION_EVENT) == false)
            return;

        // 보상을 받은 때만 그로시 남기면 된다

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        //그로씨추가
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.STAGE_ASSIST_MISSION_CLEAR,
            $"STAGE_ASSIST_MISSION_{ServerContents.StageAssistMissionEvent.eventIndex}_{clearResp.stageAssistMission.achieveCount}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            str1: $"STG_{myProfile.stage}"
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

        if (clearResp.stageAssistMission.missionIndex == 0) // 할 수 있는데까지 전부 클리어한 경우
        {
            var firstClearAchieve = new ServiceSDK.GrowthyCustomLog_Achievement (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.STAGE_ASSIST_MISSION_CLEAR,
                $"STAGE_ASSIST_MISSION_{ServerContents.StageAssistMissionEvent.eventIndex}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR,
                str1: $"STG_{myProfile.stage}"
);
            var achievementDoc = JsonConvert.SerializeObject(firstClearAchieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", achievementDoc);

        }

        if( ServerContents.StageAssistMissionEventDetails.ContainsKey(clearResp.stageAssistMission.achieveCount))
        {
            var reward = ServerContents.StageAssistMissionEventDetails[clearResp.stageAssistMission.achieveCount].reward;

            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                reward.type,
                reward.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_STAGE_ASSIST_MISSION_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_STAGE_ASSIST_MISSION_REWARD,
                $"STAGE_ASSIST_MISSION_{ServerContents.StageAssistMissionEvent.eventIndex}_{clearResp.stageAssistMission.achieveCount}"
                );
        }
    }

    private void SendClearRewardGrowthy_StageChallenge(GameClearResp clearResp)
    {
        if (ServerContents.StageChallenge == null || clearResp == null || clearResp.stageChallenge == null
            || clearResp.clearRewards == null || clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.STAGE_CHALLENGE) == false)
            return;

        // 보상을 받은 때만 그로시 남기면 된다

        //그로씨추가
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.STAGE_CHALLENGE_CLEAR,
            $"STAGE_CHALLENGE_{ServerContents.StageChallenge.eventIndex}_{clearResp.stage}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            num1: clearResp.stageChallenge.playCount
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

        for(int i = 0;i < ServerContents.StageChallenge.condition.Length; ++i)
        {
            if( ServerContents.StageChallenge.condition[i].stage == clearResp.stage)
            {
                var reward = ServerContents.StageChallenge.rewards[i];

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type,
                    reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_STAGE_CHALLENGE_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_STAGE_CHALLENGE_REWARD,
                    $"STAGE_CHALLENGE_{ServerContents.StageChallenge.eventIndex}_{clearResp.stage}"
                    );

                break;
            }
        }
    }

    private void SendClearRewardGrowthy_AllStageClear()
    {
        if (GameManager.instance.allStageClearReward <= 0)
            return;

        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    ServerRepos.LoginCdn.AllClearRewards[GameManager.instance.allStageClearReward - 1],
                    1,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_QUEST_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                    $"ComingSoonGetFlower"
                    );        
    }

    private void SendClearRewardGrowthy_SingleRound(GameClearResp clearResp)
    {
        if (Global.isSingleRoundEvent == false || clearResp == null || clearResp.clearRewards == null)
            return;

        if (clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.SINGLEROUND_EVENT) == false)
            return;

        AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.SINGLEROUND_EVENT];

        if (rewardSet.directApplied != null)
        {
            foreach (var reward in rewardSet.directApplied)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.Value.type,
                    reward.Value.valueDelta,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SINGLE_ROUND_EVENT_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SINGLE_ROUND_EVENT_REWARD,
                    $"SingleRoundEventGetReward_{ServerContents.SingleRound.eventIndex}_{clearResp.stage}"
                );
            }
        }

        if (rewardSet.mailReceived != null)
        {
            foreach (var reward in rewardSet.mailReceived)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type,
                    reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SINGLE_ROUND_EVENT_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SINGLE_ROUND_EVENT_REWARD,
                    $"SingleRoundEventGetReward_{ServerContents.SingleRound.eventIndex}_{clearResp.stage}"
                );
            }
        }
    }

    private void SendClearRewardGrowthy_CriminalEvent(GameClearResp clearResp)
    {
        //클리어 한 스테이지가 현재 유저의 목표 스테이지를 클리어 했는지
        if (ManagerCriminalEvent.instance == null || ManagerCriminalEvent.instance.IsClearStage() == false) return;
        
        //그로시추가
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.CRIMINAL_EVENT,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CRIMINAL_EVENT_STAGE_CLEAR,
            $"{ServerContents.CriminalEvent.vsn}_{clearResp.userCriminalEvent.stageClear.Count(t => t == 1)}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }

    #endregion

    //게임 클리어 후, 보상 관련해서 출력되는 팝업
    public override IEnumerator CoOpenClearRewardPopup()
    {
        //상단 재화 UI 업데이트
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.flower = (int)GameData.User.flower;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        Protocol.GameClearResp clearResp = 
            (GameManager.instance.clearResp == null) ? null : GameManager.instance.clearResp as GameClearResp;

        //2, 11 스테이지 첫 클리어 시, 시간제 클로버 보상 안내팝업.
        yield return CoRewardPopup_CloverFreeTime(clearResp);

        //챕터 미션 보상 안내 팝업
        yield return CoRewardPopup_ChapterMissionClear(clearResp);

        //통상 스테이지 독려 보상
        yield return CoRewardPopup_StageAssistMission(clearResp);

        //모으기 이벤트 보상 안내 팝업
        yield return CoRewardPopup_SpecialEvent(clearResp);

        //알파벳 이벤트 보상 안내 팝업
        yield return CoRewardPopup_AlphabetEvent(clearResp);

        //에코피(꽃 피우기) 보상 안내 팝업
        yield return CoRewardPopup_PokoFlowerEvent(clearResp);

        //도전장 보상 안내 팝업
        yield return CoRewardPopup_StageChallenge(clearResp);

        //모든 스테이지 클리어 보상 안내 팝업
        yield return CoRewardPopup_AllStageClear(clearResp);
        
        //수문장 크루그 클리어 보상 안내 팝업
        yield return CoRewardPopup_SingleRoundEvent(clearResp);
    }

    #region 인게임 클리어 보상 팝업
    private IEnumerator CoRewardPopup_CloverFreeTime(GameClearResp clearResp)
    {
        //초반 보상 받을 수 있는 스테이지가 아니거나, 첫 클리어가 아니라면 검사하지 않음
        if (GameManager.instance.firstClear == false || (Global.stageIndex != 2 && Global.stageIndex != 11))
            yield break;

        //결과 반환값이 없다면 검사하지 않음
        if (clearResp == null)
            yield break;

        //획득할 수 있는 보상값이 있다면, 보상 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.TUTORIAL_11STAGE) == true)
        {
            string rewardMessage = Global.stageIndex == 2 ? Global._instance.GetString("n_s_27") : Global._instance.GetString("n_s_28");
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.TUTORIAL_11STAGE];

            //보상 팝업 띄우기
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (rewardMessage,
                () => { isGetReward = true; },
                rewardSet);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }
    }
   
    private IEnumerator CoRewardPopup_ChapterMissionClear(GameClearResp clearResp)
    {
        if (clearResp == null || clearResp.chapterMissionCleared == null)
            yield break;

        //획득할 수 있는 보상값이 있다면, 보상 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.CHAPTERMISSION_CLEAR) == true)
        {
            string rewardMessage = Global._instance.GetString("n_s_45");
            string cmKey = clearResp.chapterMissionCleared.type == (int)QuestType.chapter_Candy ? "item_ep_2" : "item_ep_1";
            rewardMessage = rewardMessage.Replace("[1]", Global._instance.GetString(cmKey));
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.CHAPTERMISSION_CLEAR];

            //보상 팝업 띄우기
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (rewardMessage,
                () => { isGetReward = true; },
                rewardSet);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }
    }

    private IEnumerator CoRewardPopup_StageAssistMission(GameClearResp clearResp)
    {
        if (ServerContents.StageAssistMissionEvent == null)
            yield break;

        if (ManagerStageAssistMissionEvent.Instance == null)
            yield break;

        if (UIPopupClear._instance.stageAssistMission.gameObject.activeInHierarchy == false)
            yield break;

        yield return StartCoroutine(UIPopupClear._instance.stageAssistMission.StartAction_v2());

        //if (ManagerStageAssistMissionEvent.IsStageAssistMissionClear())
        if(clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.STAGE_ASSIST_MISSION_EVENT))
        {
            //보상 팝업 띄우기
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_ev_1"),
                () => { isGetReward = true; },
                clearResp.clearRewards[GameClearResp.RewardReason.STAGE_ASSIST_MISSION_EVENT]);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }
    }

    private IEnumerator CoRewardPopup_SpecialEvent(GameClearResp clearResp)
    {
        if (UIPopupClear.materialEventGetReward == false || clearResp == null)
            yield break;

        //획득할 수 있는 보상값이 있다면, 모으기 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.SPECIAL_EVENT) == true)
        {
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.SPECIAL_EVENT];
            ManagerUI._instance.OpenPopupSpecialEvent(Global.specialEventIndex, false, rewardSet);
            yield return new WaitUntil(() => UIPopupSpecialEvent._instance == null);;
        }
    }

    private IEnumerator CoRewardPopup_AlphabetEvent(GameClearResp clearResp)
    {
        if (ServerContents.AlphabetEvent == null)
            yield break;

        if (ManagerAlphabetEvent.instance == null)
            yield break;

        ManagerAlphabetEvent.instance.InitUserData();

        if (clearResp == null || clearResp.alphabetEvent == null ||
            clearResp.alphabetEvent.rewardReceived == null || clearResp.alphabetEvent.rewardReceived.Count == 0)
            yield break;

        //일반 블럭의 보상과 스페셜 블럭 보상 정보를 각각 저장
        int specialIndex = ManagerAlphabetEvent.instance.dicAlphabetIndex_Normal.Count + 1;
        bool isGetSpecialReward = (clearResp.alphabetEvent.rewardReceived.FindIndex(x => x == specialIndex) != -1);
        bool isGetNormalReward = (isGetSpecialReward == false || (isGetSpecialReward == true && clearResp.alphabetEvent.rewardReceived.Count > 1));

        if (isGetSpecialReward)
            PlayerPrefs.SetInt(ManagerAlphabetEvent.prefsKey_S, ManagerAlphabetEvent.instance.eventIndex);
        if (isGetNormalReward)
            PlayerPrefs.SetInt(ManagerAlphabetEvent.prefsKey_N, ManagerAlphabetEvent.instance.eventIndex);

        //획득할 수 있는 보상값이 있다면, 알파벳 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.ALPHABET_EVENT) == true)
        {
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.ALPHABET_EVENT];
            ManagerUI._instance.OpenPopupAlphabetEvent(false, rewardSet);
            yield return new WaitUntil(() => UIPopupAlphabetEvent._instance == null);
        }
    }

    private IEnumerator CoRewardPopup_PokoFlowerEvent(GameClearResp clearResp)
    {
        if (ServerContents.PokoFlowerEvent == null || clearResp == null ||
            clearResp.pokoFlower == null || clearResp.pokoFlower.get_reward != 1)
            yield break;

        //획득할 수 있는 보상값이 있다면, 꽃피우기 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.POKOFLOWER_EVENT) == true)
        {
            //아이템 친구에게 보내기 팝업
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.POKOFLOWER_EVENT];
            ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ServerContents.PokoFlowerEvent.event_index, true, rewardSet));

            //팝업 닫힐때까지 대기
            yield return new WaitUntil(() => UIPopupPokoFlowerEvent._instance == false);
        }
    }

    private IEnumerator CoRewardPopup_StageChallenge(GameClearResp clearResp)
    {
        if (ServerContents.StageChallenge == null || clearResp == null ||
            clearResp.stageChallenge == null)
            yield break;

        //서버와 싱크 맞추기
        if (ManagerStageChallenge.instance != null)
            ManagerStageChallenge.instance.SyncFromServerUserData();

        //획득할 수 있는 보상값이 있다면, 부스팅 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.STAGE_CHALLENGE) == true)
        {
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.STAGE_CHALLENGE];
            ManagerUI._instance.OpenPopupStageChallenge(rewardSet);

            //팝업 닫힐때까지 대기
            yield return new WaitUntil(() => UIPopupStageChallenge._instance == null);
        }
    }

    private IEnumerator CoRewardPopup_AllStageClear(GameClearResp clearResp)
    {
        if (GameManager.instance.allStageClearReward <= 0 || clearResp == null)
            yield break;

        //획득할 수 있는 보상값이 있다면, 보상 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.ALL_STAGE_CLEAR) == true)
        {
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.ALL_STAGE_CLEAR];

            //보상 팝업 띄우기
            if (rewardSet != null)
            {
                bool isGetReward = false;
                string rewardMessage = (GameManager.instance.allStageClearReward > 1) ?
                Global._instance.GetString("n_s_9") : Global._instance.GetString("n_s_8");

                ManagerUI._instance.OpenPopupGetRewardAlarm
                    (rewardMessage,
                    () => { isGetReward = true; },
                    rewardSet);

                //사운드 출력
                ManagerSound.AudioPlay(AudioInGame.PRAISE0);

                //보상 팝업 종료될 때까지 대기.
                yield return new WaitUntil(() => isGetReward == true);
            }

            StartCoroutine(CoAllStageClearReward());
            ManagerSound.AudioPlayMany(AudioInGame.GET_HEART);
        }
    }

    private IEnumerator CoAllStageClearReward()
    {
        int coinCount = ServerRepos.LoginCdn.AllClearRewards[GameManager.instance.allStageClearReward - 1];
        int totalCoinCount = Global.coin + coinCount;
        Global.clover = (int)(GameData.Asset.AllClover);
        ManagerUI._instance.UpdateUI();

        int addCount = coinCount / 10;
        if (addCount <= 0)
            addCount = 1;

        while (true)
        {
            Global.coin += addCount;

            if (Global.coin >= totalCoinCount)
            {
                Global.coin = totalCoinCount;
                ManagerUI._instance.UpdateUI();
                break;
            }
            ManagerUI._instance.UpdateUI();

            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.03f));
        }
    }
    
    private IEnumerator CoRewardPopup_SingleRoundEvent(GameClearResp clearResp)
    {
        if (ServerContents.SingleRound == null || clearResp == null)
            yield break;

        //획득할 수 있는 보상값이 있다면, 보상 팝업 출력 
        if (clearResp.clearRewards != null && clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.SINGLEROUND_EVENT) == true)
        {
            string rewardMessage = Global._instance.GetString("n_s_46");
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.SINGLEROUND_EVENT];

            //보상 팝업 띄우기
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm
            (rewardMessage,
                () => { isGetReward = true; },
                rewardSet);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }
    }
    
    #endregion

    public override void OnReadyOpenTutorial()
    {
        if (IsCanPlayTutorialStage(UIPopupReady.OPEN_ADD_TURN_STAGE, (int)READY_ITEM_TYPE.ADD_TURN) == true)
        {
            ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyItem, LanguageUtility.IsShowBuyInfo ? "_Description" : "");
        }
        else if (IsCanPlayTutorialStage(UIPopupReady.OPEN_SCORE_UP_STAGE, (int)READY_ITEM_TYPE.SCORE_UP) == true)
        {
            ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyScoreUp);
        }
        else if (IsCanPlayTutorialStage(UIPopupReady.OPEN_RANDOM_BOMB, (int)READY_ITEM_TYPE.RANDOM_BOMB) == true)
        {
            ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyRandomBomb);
        }
    }

    override public void ResetData_AfterGameStartProgress()
    {   //게임 시작 시, 데이터 초기화를 위해 임시로 사용했던 변수들을 초기화 시켜줌
        UIPopupReady.addTurnCount_ByAD.Value = 0;
    }

    override public int GetTurnCount_UseAD_AddTurn()
    {
        return UIPopupReady.addTurnCount_ByAD.Value;
    }

    override public bool IsCanWatch_AD_AddTurn(bool isNormalStage, int gameMode, int failCountOffset = 0)
    {
        #region 기본 검사
        //최소 미션 조건 확인
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;
        #endregion

        #region 게임 모드 검사
        //턴을 사용하지 않는 용암 스테이지는 광고 출력하지 않음
        if (gameMode == (int)GameMode.LAVA) return false;
        #endregion

        #region 신규 스테이지 조건 검사
        //신규 스테이지인지 확인
        int curStageIdx = Global.stageIndex;
        if (GameData.User.stage > curStageIdx) return false;

        //스테이지 데이터 있는지 확인
        if (ManagerData._instance._stageData.Count < Global.stageIndex) return false;

        //꽃 레벨이 0 이상인지 확인
        StageData curStageData = ManagerData._instance._stageData[Global.stageIndex - 1];
        if (curStageData._flowerLevel > 0) return false;
        #endregion

        #region 광고 데이터 확인

        CdnAdReadyItemInfo readyInfo = Global.GetAdReadyItemInfo();

        //스테이지 준비 팝업에서 사용하는 광고 데이터 없는지 확인
        if (readyInfo == null) return false;

        //AdReadyItemInfo 설정에 따라 광고 타입 변경
        AdManager.AdType adType = (AdManager.AdType)readyInfo.adType;

        //광고 출력 기본 조건 검사
        if (AdManager.ADCheck(adType) == false) return false;
        #endregion

        #region 실패 카운트 검사

        //실패 카운트 검사
        int checkFailCount = (isNormalStage == true) ?
            readyInfo.failCountNormal : readyInfo.failCountHard;
        if (checkFailCount == -1 || (curStageData._fail + failCountOffset) < checkFailCount) return false;
        #endregion

        return true;
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        if (GameManager.instance.firstClear)
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            int clearedStageIdx = (myProfile.stage - 1);
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.NORMAL,
                   "STAGE_" + clearedStageIdx.ToString(),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            var userStage = ServerRepos.FindUserStage(clearedStageIdx);
            achieve.L_NUM1 = userStage.play;
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    private bool IsCanPlayTutorialStage(int stageIndex, int itemIndex)
    {
        if (Global.stageIndex != stageIndex || ServerRepos.UserItem.ReadyItem(itemIndex) <= 0)
            return false;

        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && Global._instance.showTutorialBTN == true)
            return true;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 
            && myProfile.stage <= Global.stageIndex)
        {
            ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
            if (dataServer == null || dataServer.play == 0)
                return true;
            else
                return false;
        }
        return false;
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ManagerLobby._stageTryCount++;
        ServerAPI.GameStart<GameStartNormalResp>(req, complete, failCallback);
        
        if (ManagerNoyBoostEvent.instance != null && ManagerNoyBoostEvent.instance.isBoostOn)
        {
            if (ManagerNoyBoostEvent.instance.NoyBoostPackIngame == null)
                ManagerNoyBoostEvent.instance.AsyncLoadNoyBoostResource(ManagerNoyBoostEvent.PrefabType.Ingame).Forget();
        }
    }
}

