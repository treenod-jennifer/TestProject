using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Protocol;
using ServiceSDK;
using UnityEngine;

public class GameType_AdventureEvent : GameType_Base
{
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = this.CanContinue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }
    override public string GetStageKey() { return "ae_" + Global.eventIndex + "_" + Global.stageIndex + ".xml"; ; }
    override public string GetStageFilename() { return "ae_" + Global.eventIndex + "_" + Global.stageIndex + ".xml"; }
    
    override public IEnumerator GameModeProcess_OnBeforeWait()
    {
        yield return AdventureManager.instance.CoWait_AllAnimalStateWait();
        ManagerBlock.instance.isPlayBeforeWaitAction = false;
        ManagerBlock.instance.state = BlockManagrState.WAIT;
    }

    bool CanContinue()
    {
        if (ServerRepos.LoginCdn.ContinueMax == 0 || ServerRepos.LoginCdn.ContinueMax > GameManager.instance.useContinueCount)
        {
            return true;
        }
        return false;
    }

    public override bool IsStageTargetHidden()
    {
        return true;
    }

    public override bool CanUseIngameItem(GameItemType itemIndex)
    {
        switch (itemIndex)
        {
            case GameItemType.NONE: return true;
            case GameItemType.HEAL_ONE_ANIMAL: return true;
            case GameItemType.SKILL_HAMMER: return true;
            case GameItemType.ADVENTURE_RAINBOW_BOMB: return true;
            default: return false;
        }
    }

    public override void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        return;
    }

    public override GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE_EVENT;
    }

    public override int GetGrowthyPlayCount()
    {
        var stageInfo = ServerRepos.UserEventAdventureStages?.Find(x => { return x.event_index == Global.eventIndex && x.stage == Global.stageIndex; }) ?? null;

        return stageInfo?.play ?? 0;
    }

    override public bool IsFirstPlay()
    {
        foreach (var temp in ServerRepos.UserEventAdventureStages)
        {
            if (temp.event_index == Global.eventIndex && temp.stage == Global.stageIndex)
            {
                if (temp.play == 1)
                    return true;
                else
                    return false;
            }
        }
        return true;
    }
    override public IEnumerator GameModeProcess_OnIngameStart()
    {
        yield return base.GameModeProcess_OnIngameStart();
        AdventureManager.instance.InitStage();
    }

    public override string GetGrowthyStageIndex()
    {
        return Global.stageIndex.ToString();
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }
    
    public override int GetGrowthy_PLAYEND_L_NUM3()
    {
        return Global.eventIndex;
    }

    public override void SendClearGrowthyLog()
    {
        //그로씨
        var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        // 사용한 인게임 아이템
        for (var i = 4; i < 7; i++)
        {
            if (GameItemManager.useCount[i] > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = ((GameItemType)i + 1).ToString(),
                    L_CNT = GameItemManager.useCount[i]
                };
                itemList.Add(readyItem);
            }
        }
        // 컨티뉴 횟수
        if (GameManager.instance.useContinueCount > 0)
        {
            var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = GameManager.instance.useContinueCount
            };
            itemList.Add(readyItem);
        }
        var docItem = Newtonsoft.Json.JsonConvert.SerializeObject(itemList);

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var clearResp = GameManager.instance.clearResp as GameClearResp;
        
        var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
        (
            myProfile.userID,
            (myProfile.stage - 1).ToString(),
            GameManager.instance.GrowthyAfterStage.ToString(),
            Global.GameInstance.GetGrowthyStageIndex(),
            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE_EVENT,
            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL,
            0,
            ManagerBlock.instance.coins,
            (long)(Time.time - GameManager.instance.playTime),
            GameManager.instance.firstPlay, //최초플레이
            GameManager.instance.useContinueCount > 0,
            0, //남은턴 다시계산
            docItem,
            GameManager.instance.firstClear ? 1 : 0,
            GameManager.instance.clearMission        > 0,
            GameManager.instance.allStageClearReward > 0,
            GameManager.instance.stageReview,
            Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            null,
            "N",
            boostLevel: "0",
            firstFlowerLevel: "N",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: "[0]"
        );
        
        //사용동물
        var animalList = new List<GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL>();
        for (var i = 0; i < 3; i++)
        {
            var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);

            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL
            {
                L_CID = animalData.idx,
                L_LEV = animalData.level,
                L_CNT = animalData.overlap
            };
            animalList.Add(readyItem);
        }

        var docAnimal = Newtonsoft.Json.JsonConvert.SerializeObject(animalList);
        playEnd.L_CHAR = docAnimal;

        var doc = Newtonsoft.Json.JsonConvert.SerializeObject(playEnd);
        doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(clearResp, false));
        ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);
        
        SendStageFirstClearGrowthyLog();
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        if (!ManagerAdventure.EventData.IsAdvEventStageFirstCleard()) return;

        var achieve = new GrowthyCustomLog_Achievement
        (
            tag:  GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
            cat:  GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE_EVENT,
            anm:  $"EVENT_{Global.eventIndex}_STAGE_{Global.stageIndex}",
            arlt: GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
        );

        ServerUserEventAdventureStage adventureEventStage = null;
        foreach (var stage in ServerRepos.UserEventAdventureStages)
        {
            if (stage.stage == Global.stageIndex)
            {
                adventureEventStage = stage;
                break;
            }
        }

        achieve.L_NUM1 = adventureEventStage.play;

        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

        SendFirstAllClearGrowthyLog();
    }

    private void SendFirstAllClearGrowthyLog()
    {
        if (Global.stageIndex != ManagerAdventure.EventData.GetAdvEventStageCount()) return;

        var achieve = new GrowthyCustomLog_Achievement
        (
            tag: GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            cat: GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE_EVENT,
            anm: $"EVENT_STAGE_ALLCLEAR",
            arlt: GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
        );

        ServerUserEventAdventureStage adventureEventStage = null;
        foreach (var stage in ServerRepos.UserEventAdventureStages)
        {
            if (stage.stage == Global.stageIndex)
            {
                adventureEventStage = stage;
                break;
            }
        }

        achieve.L_NUM1 = adventureEventStage.play;

        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

        SendSuccessGrowthyLog();
    }

    private void SendSuccessGrowthyLog()
    {
        var achieve = new GrowthyCustomLog_Achievement
        (
            tag: GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            cat: GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE_EVENT,
            anm: $"EVENT_STAGE_SUCCESS",
            arlt: GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );

        ServerUserEventAdventureStage adventureEventStage = null;
        foreach (var stage in ServerRepos.UserEventAdventureStages)
        {
            if (stage.stage == Global.stageIndex)
            {
                adventureEventStage = stage;
                break;
            }
        }

        achieve.L_NUM1 = adventureEventStage.play;

        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }
    public override void CheckKeepPlay()
    {
        if (ManagerAdventure.EventData.GetActiveEvent_AdventureEvent())
        {
            this.keepPlayFlag = true;
        }
        else
        {
            ManagerUI._instance.IsTopUIAdventureMode = false;
        }
       
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        ManagerUI._instance.OpenPopup<UIPopUpStageAdventureEventReady>();
        yield return new WaitUntil(() => UIPopUpStageAdventureEventReady._instance == null && UIPopupStageAdventure._instance == null);
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.AdventureGameStart<GameStartAdventureEventResp>(req, complete, failCallback);
    }
    
    public override string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventDatas = new Dictionary<string, object>();
        
        var adventureClearResp = GameManager.instance.clearResp as AdventureGameClearResp;
        if (ManagerAdventurePass.CheckStartable() && adventureClearResp != null && adventureClearResp.userAdventurePass != null)
        {
            var adventurePassMissionCount = adventureClearResp.userAdventurePass.targetCount - ManagerAdventurePass.userMissionCount;
            integratedEventDatas.Add("L_ADVENTURE_PASS_MISSION_COUNT", adventurePassMissionCount);
            integratedEventDatas.Add("L_ADVENTURE_PASS_ACTIVE", true);
        }
        
        return GetJsonData(integratedEventDatas);
    }
}
