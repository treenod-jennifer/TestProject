using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameType_BingoEvent : GameType_Base
{
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = this.CanContinue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.USE_READY_CHARACTER] = GameType_Base.ReturnTrue;
    }

    override public string GetStageKey()
    {
        return $"B_{Global.eventIndex}_{Global.stageIndex}.xml";
    }

    override public string GetStageFilename()
    {
        return GetStageKey();
    }
    
    override public IEnumerator GameModeProcess_OnIngameStart()
    {
        yield return base.GameModeProcess_OnIngameStart();
    }
    
    override public IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callbackCancel = null)
    {
        ManagerUI._instance.OpenPopupReady_BingoEvent(tempData, callBackStart, callBackClose, callbackCancel);
        
        yield return null;
    }

    override public string GetStageText_IngamePopup()
    {
        return Global._instance.GetString("p_bge_1");
    }
    
    override public string GetStageText_ClearPopup()
    {
        return Global._instance.GetString("p_bge_1");
    }
    
    override public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        popup.stage.text = Global._instance.GetString("p_bge_1");
        popup.stage.MakePixelPerfect();
        popup.rankRoot.SetActive(false);
        popup.SettingBoniDialogBubbleBingo();

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
    }

    override public string GetText_StageFail(ProceedPlayType proceedPlayType)
    {
        return Global._instance.GetString("p_sf_7");
    }

    override public string GetGrowthyStageIndex()
    {
        return Global.stageIndex.ToString();
    }
    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return true;
            case 1: return false;
            case 2: return true;
            case 3: return true;
            case 4: return true;
            case 5: return true;
            case 6: return CanUseDoubleReadyItem();
            case 7: return false;
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
        foreach (var temp in ServerRepos.UserMoleCatchStages)
        {
            if (temp.event_index == Global.eventIndex && temp.stage == Global.stageIndex && temp.chapter == Global.chapterIndex)
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
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }

    public override void SendClearGrowthyLog()
    {
        var clearResp = GameManager.instance.clearResp as GameClearResp;
        
        // Growthy 그로씨
        // 사용한 레디 아이템
        var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 0; i < 8; i++)
        {
            if (UIPopupReady.readyItemUseCount[i].Value > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
                    L_IID = ((READY_ITEM_TYPE)i).ToString(),
                    L_CNT = UIPopupReady.readyItemUseCount[i].Value
                };
                itemList.Add(readyItem);
            }
        }
        // 사용한 인게임 아이템
        itemList.AddRange(GameItemManager.GetPlayEndInGameItemLogData());
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
        var docItem = JsonConvert.SerializeObject(itemList);
        
        // 획득한 스코어
        var tempScore = ManagerBlock.instance.score;
        // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
        var eventRatio = Global.GameInstance.GetBonusRatio(); 
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            tempScore = (int)(tempScore * (1.1f + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
        {
            tempScore = (int)(tempScore * (1.2f + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }
        else
        {
            tempScore = (int)(tempScore * (1 + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }

        // 스테이지 모드
        var growthyStageType = Global.GameInstance.GetGrowthyGameMode();
        var growthyStar      = GetGrowthyStar();
        
        var getCoinB         = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
        var rankMode = "N";
        rankMode = Global.GameInstance.GetProfileAgreementString();

        // 노이 부스팅 단계
        var boostingLevel   = 0;
        
        // 에코피 이벤트
        var checkEcopiEvent = false;
        
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
        (
            pid: myProfile.userID,
            lastStage: (myProfile.stage - 1).ToString(),
            beforeLastStage: GameManager.instance.GrowthyAfterStage.ToString(),
            currentStage: Global.GameInstance.GetGrowthyStageIndex(),
            stageType: growthyStageType,
            win: ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
            star: growthyStar,
            score: tempScore,
            getCoin: getCoinB, //ManagerBlock.instance.coins,
            playTime: (long) (Time.time - GameManager.instance.playTime),
            firstPlay: GameManager.instance.firstPlay, //최초플레이
            continuePlay: GameManager.instance.useContinueCount > 0,
            leftTurn: GameManager.instance.leftMoveCount, //남은턴 다시계산
            useItemList: docItem,
            getHeart: GameManager.instance.firstClear ? 1 : 0,
            clearStageMission: GameManager.instance.clearMission > 0,
            clearChapterMission: GameManager.instance.allStageClearReward > 0,
            reviewCount: GameManager.instance.stageReview,
            tempEventIndex: ManagerBingoEvent.instance.EventIndex,
            remainGoals: null,
            rankMode: rankMode,
            ecopi: checkEcopiEvent == true ? "Y" : "N",
            boostLevel: boostingLevel.ToString(),
            firstFlowerLevel: "Y",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: null
        );
        
        var doc = JsonConvert.SerializeObject(playEnd);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        SendStageFirstClearGrowthyLog();
    }
    
    override public void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        GameClearResp resp = r as GameClearResp;

        if (resp.isFirst > 0)
        {
            gm.firstClear = true;
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        gm.gainClover = resp.gainClover > 0;
    }

    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.CLOVER;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.BINGO;
    }

    public override int GetGrowthyPlayCount()
    {
        return ServerRepos.UserBingoEventStage.play;
    }

    public override void CheckKeepPlay()
    {
        if (ServerRepos.UserBingoEvent != null && ServerRepos.UserBingoEventStage != null && ServerRepos.UserBingoEventStage.play > 0)
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        ManagerUI._instance.OpenPopup<UIPopupBingoEvent_Board>((popup) =>
        {
            ManagerBingoEvent.instance.isSlotOpen = true;
            popup.InitData();
        });
        yield return new WaitUntil(() => UIPopupBingoEvent_Board._instance == null);

    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartBingoEventResp>(req, complete, failCallback);
    }
    
    override public GameObject GetSpine_GameClearPopup()
    {
        return ManagerBingoEvent.bingoEventResource.bingoEventPack.ObjResultSpine ?? null;
    }

    public override bool IsLoadLive2DCharacters()
    {
        List<TypeCharacterType> listLive2DCharacter = new List<TypeCharacterType>();

        ManagerBingoEvent.instance.live2dCharacter = ManagerBingoEvent.instance.IsUseReadyCollaboCharacter()
            ? (TypeCharacterType)ManagerBingoEvent.instance.ReadyCharacterType
            : ManagerBingoEvent.instance.live2dCharacter;

        if (ManagerCharacter._instance._live2dObjects.ContainsKey((int)(ManagerBingoEvent.instance.live2dCharacter)) == false)
        {
            listLive2DCharacter.Add(ManagerBingoEvent.instance.live2dCharacter);
        }
        
        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);

        return listLive2DCharacter.Count > 0;
    }
}
