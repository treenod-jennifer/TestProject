using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GameType_Event : GameType_Base
{
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME] = this.FlowerOn_Ingame;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = this.CanContinue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = () => { return this.CanRetry(false); };
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.USE_READY_CHARACTER] = GameType_Base.ReturnTrue;
    }

    override public string GetStageKey() { return "E" + Global.eventIndex + "_" + Global.stageIndex + ".xml"; }
    override public string GetStageFilename() { return "E" + Global.eventIndex + "_" + Global.stageIndex + ".xml"; }

    public override void InitStage()
    {
        CollectEventProcess_OnIngameStart(GameManager.instance);
    }

    bool CanRetry(bool isClear)
    {
        CdnEventChapter eventChapter = ServerContents.EventChapters;
        switch (eventChapter.type)
        {
            case (int)EVENT_CHAPTER_TYPE.FAIL_RESET:
                return false;
            case (int)EVENT_CHAPTER_TYPE.COLLECT:
                return true;
            case (int)EVENT_CHAPTER_TYPE.SCORE:
                if (isClear == false)
                    return true;
                else
                    return IsCanGetReward_ScoreMode();
        }
        return true;
    }

    //스코어 모드에서 보상을 받을 수 있는 상태인지 검사.
    private bool IsCanGetReward_ScoreMode()
    {
        CdnEventChapter eventChapter = ServerContents.EventChapters;
        int rewardIndex = ManagerData._instance._eventChapterData._groupState - 1;
        
        if (eventChapter.rewardFlowerCount.Count <= rewardIndex)
            return true;

        //이벤트 스테이지에 대한 정보 받아오기.
        List<ServerEventStage> listEventStage = new List<ServerEventStage>();
        foreach (var item in ServerRepos.EventStages)
        {
            if (item.eventIdx == Global.eventIndex)
            {
                listEventStage.Add(item);
            }
        }

        //유저가 획득한 이벤트 뱃지 카운트 가져오기.
        int getScoreBadgeCount = 0;
        for (int i = 0; i < eventChapter.counts[0]; i++)
        {
            var stage = listEventStage.Find(x => x.stage == (i + 1));
            getScoreBadgeCount += (stage == null) ? 0 : stage.flowerLevel;
        }

        //현재 보상을 획득하기 위한 별 수가 충분하면 재시작 가능
        if (eventChapter.rewardFlowerCount[rewardIndex] <= getScoreBadgeCount)
            return false;
        else
            return true;
    }

    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return true;
            case 1: return ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE;
            case 2: return true;
            case 3: return true;
            case 4: return true;
            case 5: return true;
            case 6: return CanUseDoubleReadyItem();
            case 7: return (CanUseDoubleReadyItem() == false ? false :  ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE);
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

    override public int GetStageVer()
    {
        return ServerContents.EventChapters.stageVersions[Global.stageIndex - 1];
    }

    bool CanContinue()
    {
        if (ServerRepos.LoginCdn.EventContinueMax == 0 || ServerRepos.LoginCdn.EventContinueMax > GameManager.instance.useContinueCount)
        {
            return true;
        }
        return false;
    }

    override public bool IsFirstPlay()
    {
        foreach (var temp in ServerRepos.EventStages)
        {
            if (temp.eventIdx == Global.eventIndex && temp.stage == Global.stageIndex)
            {
                if (temp.play == 1)
                    return true;
                else
                    return false;
            }
        }
        return true;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        if(ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);


        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
        
    }

    public override void SendClearGrowthyLog()
    {
        base.SendClearGrowthyLog_Regular(false);
    }

    void CollectEventProcess_OnIngameStart(GameManager gm)
    {
        if (ServerContents.EventChapterActivated() && ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT)
        {
            //모은것 확인하기
            foreach (var temp in ServerRepos.EventStages)
            {
                if (temp.eventIdx == Global.eventIndex && temp.stage == Global.stageIndex)
                {
                    ManagerBlock.instance.materialCollectPos = temp.materialCnt;
                }
            }
        }
    }

    override public IEnumerator SetBGM_OnIngameStart(GameManager gm)
    {
        if (Global.CollaboIndex == 2)
        {
            yield return gm.LoadBGM();
        }
        else
            yield return base.SetBGM_OnIngameStart(gm);
    }

    bool FlowerOn_Ingame()
    {
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return true;
        else
            return false;
    }

    override public ScoreFlowerType GetMaxType_FlowerScore()
    {
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return ScoreFlowerType.FLOWER_WHITE;
        else
            return base.GetMaxType_FlowerScore();
    }

    override public ScoreFlowerType GetMaxType_UnlimitedFlowerScore() //해당 게임타입에서 피울 수 있는 꽃의 최대 단계 (무제한)
    {
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return ScoreFlowerType.FLOWER_WHITE;
        else
            return base.GetMaxType_UnlimitedFlowerScore();
    }

    override public bool IsChangeSkin_IngameScoreUI()
    {
        return (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE);
    }

    override public string GetSkinName_IngameScoreUI(int index)
    {
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return "6";
        else
            return "";
    }

    override public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.SCORE)
            return (string.Format("clear_S_start{0}", flowerCount), string.Format("clear_S_loop{0}", flowerCount));
        else
            return ("", "");
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        GameClearResp resp = r as GameClearResp;

        ManagerEventStage._eventStageClear = true;

        // 스테이지 클리어
        if (ServerContents.EventChapters.type != (int)EVENT_CHAPTER_TYPE.SCORE)
        {
            if (ManagerData._instance._eventChapterData._state < ServerRepos.EventChapters.stage)
            {
                UIPopupReady.eventStageClear = true;
            }

            // 로컬 정보와 갱신된 정보를 비교(그룹 상태 변화..초기1그룹..  2가되면 1그룹 완료,,, )
            if (ManagerData._instance._eventChapterData._groupState < ServerRepos.EventChapters.groupState)
            {
                UIPopupReady.eventGroupClear = true;
                //포코유라 데이터 갱신.
                PokoyuraData.SetUserData();
                Global.flower = (int)GameData.User.flower;
            }
        }
        else
        {
            //스코어 모드에서는 무조건 클리어 처리.
            UIPopupReady.eventStageClear = true;
        }

        if (resp.isFirst > 0)
        {
            gm.firstClear = true;
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        gm.gainClover = resp.gainClover > 0;

        //강제 노출 이벤트 리워드 갱신
        if (ManagerForceDisplayEvent.instance != null)
        {
            if (ServerRepos.EventChapters.groupState > ServerContents.EventChapters.counts.Count)
            {
                if (ServerContents.EventChapters.type != (int)EVENT_CHAPTER_TYPE.SCORE)
                {
                    ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.EVENT_STAGE);
                }
                else
                {
                    //갱신될 스테이지 제외하고 모든 꽃 더함
                    var stages = ServerRepos.EventStages.FindAll(x => x.eventIdx == ServerContents.EventChapters.index && x.stage != resp.eventStage.stage);
                    var flowers = stages.Sum(x => x.flowerLevel);
                    //갱신될 스테이지 꽃 더함
                    flowers += resp.flowerLevel;

                    if (flowers >= ServerContents.EventChapters.rewardFlowerCount.Last())
                    {
                        ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.EVENT_STAGE);
                    }
                }
                
            }
        }
    }

    override public void OnRecvGameFail()
    {
        ManagerEventStage._eventStageFail = true;
        UIPopupReady.eventStageFail = true;
    }

    override public void OnRecvGameRestart()
    {
        ManagerEventStage._eventStageFail = true;
        UIPopupReady.eventStageFail = true;
    }

    public override void OnPopupInit_Continue(UIPopupContinue popup)
    {
        base.OnPopupInit_Continue(popup);
        popup.SettingBoniDialogBubble();
    }

    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.CLOVER;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        CdnEventChapter cdnData = ServerContents.EventChapters;
        switch((EVENT_CHAPTER_TYPE)cdnData.type)
        {
            case EVENT_CHAPTER_TYPE.FAIL_RESET:
                return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_FAILRESET;
            case EVENT_CHAPTER_TYPE.COLLECT:
                return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_COLLECT;
            case EVENT_CHAPTER_TYPE.SCORE:
                return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_SCORE;
        }
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD._EVENT;
    }

    public override int GetGrowthyPlayCount()
    {
        var userStageData = ServerRepos.EventStages.Find(x => x.eventIdx == Global.eventIndex && Global.stageIndex == x.stage);
        return userStageData?.play ?? 0;
    }
    
    public override string GetGrowthyStageIndex()
    {
        return GameManager.instance.CurrentStage.ToString();
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        if (GameManager.instance.firstClear)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT,
                   "EVENT_" + Global.eventIndex + "_STAGE_" + Global.stageIndex,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            var userStageData = ServerRepos.EventStages.Find(x => x.eventIdx == Global.eventIndex && Global.stageIndex == x.stage);
            if( userStageData == null )
            {   // panic
                return;
            }
            achieve.L_NUM1 = userStageData.play;
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    public override void CheckKeepPlay()
    {
        if (ManagerEventStage._eventStageClear || ManagerEventStage._eventStageFail)  // 이벤트 스테이지를 플레이하고 왔으면 스테이지 창 바로 열기.
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        int stageIndex = ServerRepos.EventChapters.stage;

        CdnEventChapter cdnData = ServerContents.EventChapters;
        if (cdnData != null)
        {
            if (stageIndex > cdnData.counts[cdnData.counts.Count - 1])
                stageIndex = Global.stageIndex;

            if (UIPopupReady.beforeStage)
                stageIndex = Global.stageIndex;

            Global.SetGameType_Event(Global.eventIndex, stageIndex);
            yield return ManagerUI._instance.CoOpenPopupReadyStageEvent();
            yield return new WaitUntil(() => UIPopupReady._instance == null);
        }
        yield break;
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartEventStageResp>(req, complete, failCallback);
    }

    #region 인게임 클리어 보상 팝업
    //게임 클리어 후, 보상 관련해서 출력되는 팝업
    public override IEnumerator CoOpenClearRewardPopup()
    {
        Protocol.GameClearResp clearResp =
            (GameManager.instance.clearResp == null) ? null : GameManager.instance.clearResp as GameClearResp;

        if (clearResp.clearRewards == null ||
            clearResp.clearRewards.ContainsKey(GameClearResp.RewardReason.EVENT_STAGE) == false)
        {
            yield break;
        }

        CdnEventChapter eventChapter = ServerContents.EventChapters;
        int eventGroup = (eventChapter.type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET) ?
           1 : ManagerData._instance._eventChapterData._groupState;

        //보상 업데이트
        UpdateReward(eventChapter, eventGroup);

        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.flower = (int)GameData.User.flower;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        string textMessage = "";
        bool isCanReceiveReward = false;
        EVENT_CHAPTER_TYPE eType = (EVENT_CHAPTER_TYPE)eventChapter.type;
        switch (eType)
        {
            case EVENT_CHAPTER_TYPE.FAIL_RESET:
                isCanReceiveReward = IsCanReceiveClearReward_FAIL_RESET(eventChapter);
                textMessage = Global._instance.GetString("n_ev_1");
                break;
            case EVENT_CHAPTER_TYPE.COLLECT:
                isCanReceiveReward = IsCanReceiveClearReward_COLLECT(eventChapter);
                textMessage = Global._instance.GetString("n_ev_1");
                break;
            case EVENT_CHAPTER_TYPE.SCORE:
                isCanReceiveReward = IsCanReceiveClearReward_SCORE(eventChapter);
                int scoreRewardState = ManagerData._instance._eventChapterData._groupState;
                textMessage = Global._instance.GetString("n_ev_6").Replace("[n]", scoreRewardState.ToString());
                break;
        }

        if (isCanReceiveReward == false)
            yield break;

        AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.EVENT_STAGE];
        yield return CoGetEventReward(rewardSet, textMessage);
    }

    private bool IsCanReceiveClearReward_FAIL_RESET(CdnEventChapter eventChapter)
    {
        int eventGroup = ManagerData._instance._eventChapterData._groupState;

        //현재 그룹의 마지막 스테이지를 클리어했을 때, 보상처리
        if (Global.stageIndex == eventChapter.counts[(eventGroup - 1)])
            return true;
        else
            return false;
    }

    private bool IsCanReceiveClearReward_COLLECT(CdnEventChapter eventChapter)
    {
        int stageCount = eventChapter.counts[0];

        //마지막 스테이지를 클리어했을 때, 보상처리
        if (Global.stageIndex == stageCount)
            return true;
        else
            return false;
    }

    private bool IsCanReceiveClearReward_SCORE(CdnEventChapter eventChapter)
    {
        int stageCount = eventChapter.counts[0];

        List<ServerEventStage> listEventStage = new List<ServerEventStage>();
        foreach (var item in ServerRepos.EventStages)
        {
            if (item.eventIdx == Global.eventIndex)
                listEventStage.Add(item);
        }

        int getScoreBadgeCount_Current = 0;
        for (int i = 0; i < stageCount; i++)
        {
            int checkStage = (i + 1);
            var stage = listEventStage.Find(x => x.stage == checkStage);
            int currentLevel = (stage == null) ? 0 : stage.flowerLevel;

            //전체 꽃 카운트 갱신
            getScoreBadgeCount_Current += currentLevel;
        }

        //보상 상태 설정
        int scoreRewardState = ManagerData._instance._eventChapterData._groupState;
        int scoreRewardIndex = scoreRewardState - 1;
        if (eventChapter.rewardFlowerCount.Count > scoreRewardIndex)
        {
            //보상을 획득한 상태인지 검사
            if (eventChapter.rewardFlowerCount[scoreRewardIndex] <= getScoreBadgeCount_Current)
                return true;
        }
        return false;
    }

    //보상획득 팝업 출력
    private IEnumerator CoGetEventReward(AppliedRewardSet rewardSet, string textMessage)
    {
        bool isGetReward = false;

        ManagerUI._instance.OpenPopupGetRewardAlarm
            (textMessage,
            () =>
            {   //탐험모드 캐릭터 소환 시, 해당 팝업이 종료될 때까지 대기
                if (ManagerAdventure.Active && ManagerAdventure.User != null && ManagerAdventure.User.noticePostponedAnimal != null)
                {
                    UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, ManagerAdventure.User.noticePostponedAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                    ManagerAdventure.User.noticePostponedAnimal = null;
                    summonPopup._callbackEnd = () => { isGetReward = true; };
                }
                else
                    isGetReward = true;
            },
            rewardSet);

        //보상 팝업 종료될 때까지 대기.
        yield return new WaitUntil(() => isGetReward == true);
    }

    //보상 업데이트
    private void UpdateReward(CdnEventChapter eventChapter, int eventGroup)
    {
        // 스코어모드는 그룹번호가 0, 1, 2 순이고, 나머지 다른 타입은 1,2,3 순서기 때문에 조정해줘야함
        int eventGroupIdx = eventChapter.type == (int)EVENT_CHAPTER_TYPE.SCORE ? ManagerData._instance._eventChapterData._groupState - 1 : eventGroup - 1;

        //받는 보상 중 선물상자가 있는 경우, 로비에 생성.
        for (int i = 0; i < eventChapter.rewards[eventGroupIdx].Count; i++)
        {
            int type = eventChapter.rewards[eventGroupIdx][i].type;
            if (type >= 100)
            {
                ManagerLobby._instance.ReMakeGiftbox();
                break;
            }
        }

        //그로씨
        for (int i = 0; i < eventChapter.rewards[eventGroupIdx].Count; i++)
        {
            Reward rewards = eventChapter.rewards[eventGroupIdx][i];

            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                rewards.type,
                rewards.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                );
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                           (
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT,
                               $"EVENT_CHAPTER_{Global.eventIndex}_{eventGroupIdx + 1}_CLEAR",
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                           );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        //이벤트 전체 첫클리어시
        var totalCleared = ServerRepos.EventChapters.stage > eventChapter.counts[eventChapter.counts.Count - 1];
        if (totalCleared)
        {
            int totalPlayCount = 0;
            foreach (var item in ServerRepos.EventStages)
                if (item.eventIdx == Global.eventIndex)
                    totalPlayCount += item.play;

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                            (
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT,
                                Global.eventIndex.ToString(),
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                            );
            achieve.L_NUM1 = totalPlayCount;
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
    }
    #endregion
    
    public override bool IsLoadLive2DCharacters()
    {
        List<TypeCharacterType> listLive2DCharacter = new List<TypeCharacterType>();
        
        CdnEventChapter eventUser = ServerContents.EventChapters;
        int groupState = (eventUser.type == (int)EVENT_CHAPTER_TYPE.SCORE) ? 1 : ServerRepos.EventChapters.groupState;
        if (groupState > eventUser.counts.Count)
            groupState = eventUser.counts.Count;
        
        string fileName = "_ready" + groupState;
        string assetName = eventUser.assetName + fileName;
        listLive2DCharacter.Add(ManagerLobby._assetBankEvent[assetName].GetComponent<ReadyEvent>().live2dCharacter);
        
        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);

        return listLive2DCharacter.Count > 0;
    }
}
