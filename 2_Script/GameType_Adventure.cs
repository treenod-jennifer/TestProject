using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ServiceSDK;

public class GameType_Adventure : GameType_Base 
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
    override public string GetStageKey()
    {
        return "a_" + Global.chapterIndex + "_" + Global.stageIndex + ".xml";
    }

    override public string GetStageFilename()
    {
        return GetStageKey();// Global.GetHashfromText(GetStageKey()) + ".xml";
    }

    override public bool IsFirstPlay()
    {
        if (ManagerAdventure.User.GetChapterProgress(Global.chapterIndex) == null ||
            ManagerAdventure.User.GetChapterProgress(Global.chapterIndex).GetStageProgress(Global.stageIndex) == null ||
            ManagerAdventure.User.GetChapterProgress(Global.chapterIndex).GetStageProgress(Global.stageIndex).playCount < 1)
            return true;
        return false;
    }

    override public IEnumerator GameModeProcess_OnIngameStart()
    {
        yield return base.GameModeProcess_OnIngameStart();
        AdventureManager.instance.InitStage();
    }

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

    override public bool CanUseReadyItem(int itemIndex)
    {
        return false;
    }

    override public bool IsStageTargetHidden()
    {
        return true;
    }

    public override void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        return;
    }

    override public void OnRecvAdvantureGameClear(GameManager gm, AdventureGameClearResp resp)
    {
        gm.firstClear = resp.isFirst == 1;

        foreach (var reward in resp.rewards)
        {
            SetTopUIValue(reward);
        }

        foreach (var box in resp.boxes)
        {
            if(box.Value != null)
            {
                foreach(var item in box.Value)
                {
                    SetTopUIValue(item);
                }
            }
        }
        //SendClearGrowthyLog();
    }

    private void SetTopUIValue(Reward reward)
    {
        switch (reward.type)
        {
            case (int)RewardType.coin:
                Global.coin = Global.coin + reward.value;
                break;
            case (int)RewardType.wing:
                Global.wing = Global.wing + reward.value;
                break;
            case (int)RewardType.jewel:
                Global.jewel = Global.jewel + reward.value;
                break;
            case (int)RewardType.expBall:
                Global.exp = Global.exp + reward.value;
                break;

            default:
                break;
        }
    }

    override public bool CanUseIngameItem(GameItemType itemIndex)
    {
        switch (itemIndex)
        {
            case GameItemType.NONE: return true;
            case GameItemType.HEAL_ONE_ANIMAL: return true;
            case GameItemType.SKILL_HAMMER:
                return !(Global.chapterIndex == GameItemManager.SKILL_HAMMER_OPEN_CHAPTER &&
                         Global.stageIndex < GameItemManager.SKILL_HAMMER_OPEN_STAGE); //1-4 스테이지 이상이라면 사용가능
                
            case GameItemType.ADVENTURE_RAINBOW_BOMB: return true;
            default: return false;
        }
    }

    override public string GetGrowthyStageIndex()
    {
        return Global.stageIndex.ToString();
    }


    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE;
    }

    public override int GetGrowthyPlayCount()
    {
        var advStage = ServerRepos.UserAdventureStages?.Find(x => x.chapter == Global.chapterIndex && x.stage == Global.stageIndex) ?? null;
        return advStage?.play ?? 0;
        //return ManagerAdventure.User.GetChapterProgress(Global.chapterIndex)?.GetStageProgress(Global.stageIndex)?.playCount ?? 0;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }
    
    public override int GetGrowthy_PLAYEND_L_NUM3()
    {
        return Global.chapterIndex;
    }

    public override void SendClearGrowthyLog()
    {
        // Growthy 그로씨
        // 사용한 인게임 아이템
        var itemList = new List<GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 4; i < 7; i++)
        {
            if (GameItemManager.useCount[i] > 0)
            {
                var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = ((GameItemType)i + 1).ToString(),
                    L_CNT = GameItemManager.useCount[i]
                };
                itemList.Add(readyItem);
            }
        }
        // 컨티뉴 횟수
        if (GameManager.instance.useContinueCount > 0)
        {
            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = GameManager.instance.useContinueCount
            };
            itemList.Add(readyItem);
        }
        var docItem = JsonConvert.SerializeObject(itemList);

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var clearResp = GameManager.instance.clearResp as GameClearResp;

        var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
        (
            myProfile.userID,
            (myProfile.stage - 1).ToString(),
            GameManager.instance.GrowthyAfterStage.ToString(),
            Global.GameInstance.GetGrowthyStageIndex(),
            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE,
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
        
        // 사용동물
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
        var docAnimal = JsonConvert.SerializeObject(animalList);
        playEnd.L_CHAR = docAnimal;

        var doc = JsonConvert.SerializeObject(playEnd);
        doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(clearResp, false));
        ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        if (GameManager.instance.firstClear)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE,
                    string.Format("CHAP_{0}_STAGE_{1}", Global.chapterIndex.ToString(), Global.stageIndex.ToString()),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );

            var chapProg = ManagerAdventure.User.GetChapterProgress(Global.chapterIndex);
            if (chapProg == null)
            {
                achieve.L_NUM1 = 1;
            }
            else
            {
                var stageProg = chapProg.GetStageProgress(Global.stageIndex);
                achieve.L_NUM1 = stageProg == null ? 1 : stageProg.playCount + 1;
            }

            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    public override void CheckKeepPlay()
    {
        if (UIPopupStageAdventure.startChapter != 0 && !(ManagerTutorial._instance != null && ManagerTutorial._instance._playing))
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        ManagerUI._instance.OpenPopupStageAdventure();
        yield return new WaitUntil(() => UIPopUpStageAdventureEventReady._instance == null && UIPopupStageAdventure._instance == null);
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.AdventureGameStart<GameStartAdventureResp>(req, complete, failCallback);
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

