using Protocol;
using System.Collections;
using UnityEngine;
using System;
using ServiceSDK;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class GameType_SpaceTravel : GameType_Base
{
    public override string GetStageKey() => $"ST{Global.eventIndex}_{Global.stageIndex}.xml";
    public override string GetStageFilename() => GetStageKey();
    public override string GetStageText_ReadyPopup()  => $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
    public override string GetStageText_IngamePopup() => $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
    public override string GetStageText_ClearPopup()  => $"{Global._instance.GetString("p_sr_1")} {GameManager.instance.CurrentStage}";
    public override string GetText_StageFail(ProceedPlayType proceedPlayType) => Global._instance.GetString("p_sf_9");
    public override int GetChapterIdx() => 0;
    public override int GetFirstClearRewardType() => (int) FirstClearRewardType.CLOVER;
    public override void SetIngameUI() => GameUIManager.instance.SetSpaceTravel();
    public override bool IsFirstPlay() => ServerRepos.UserSpaceTravelEventStage == null || ServerRepos.UserSpaceTravelEventStage.Count == 0;
    public override bool IsChangeSkin_IngameScoreUI() => true;
    public override (string, string) GetSpineAniNames_GameClearPopup(int flowerCount) => Global.stageIndex % 3 == 0 ? ("clear_start_2", "clear_loop_2") : ("clear_start_1", "clear_loop_1");
    public override GameObject GetSpine_GameClearPopup() => ManagerSpaceTravel.instance._spaceTravelPackIngame.ResultSpineObj;
    public override GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar() => GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    public override GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode() => GrowthyCustomLog_PLAYEND.Code_L_GMOD.SPACE_TRAVEL;
    public override void SendClearGrowthyLog() => base.SendClearGrowthyLog_Regular(false);
    public override string GetGrowthyStageIndex() => Global.stageIndex.ToString();
    
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT]                      = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT]       = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE]            = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED]          = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY]               = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME]              = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_USE_READYITEM]             = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE]                  = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE]            = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL]    = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS]             = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.USE_READY_CHARACTER]           = GameType_Base.ReturnTrue;
    }

    /// <summary>
    /// 목표 팝업이 등장하기 전, 게임 타입별로 실행될 액션
    /// </summary>
    public override IEnumerator CoActionIngameStart_BeforeOpenTargetPopup()
    {
        //보너스 아이템 선택 팝업
        var isClosePopup   = false;
        var selectItemType = ManagerSpaceTravel.BonusItemType.ADD_TURN;
        ManagerUI._instance.OpenPopupSpaceTravelSelectItem(() => isClosePopup = true, (type) => selectItemType = type);

        //팝업 닫힐 때 까지 대기
        yield return new WaitUntil(() => isClosePopup == true);

        //인게임 아이템 증가 연출(폭탄류)
        if (selectItemType    == ManagerSpaceTravel.BonusItemType.LINE_BOMB
            || selectItemType == ManagerSpaceTravel.BonusItemType.CIRCLE_BOMB
            || selectItemType == ManagerSpaceTravel.BonusItemType.RAINBOW_BOMB)
        {
            yield return GameUIManager.instance.CoActionItemAddCount_SpaceTravel(selectItemType);
        }

        //인게임 턴 증가 연출(사과)
        var addTurnCount = GetReadyItem_AddTurnCount();
        if (selectItemType == ManagerSpaceTravel.BonusItemType.ADD_TURN)
        {
            addTurnCount += ManagerSpaceTravel.instance.addTurnCount;
        }

        if (addTurnCount > 0)
        {
            yield return GameUIManager.instance.CoActionAddTurn_WithMakeBubble(addTurnCount);
        }
    }

    public override IEnumerator CoApplyReadyItem_AddTurn(int appleCount = 0)
    {
        //우주여행 모드에서는 턴 추가 아이템 사용하면 목표창 전에 떠야하므로, 해당 코루틴은 아무런 동작도 하지 않음
        yield break;
    }


    public override IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callbackCancel = null)
    {
        ManagerUI._instance.OpenPopupReady_Event<UIPopupReadySpaceTravel>(tempData, callBackStart, callBackClose, callbackCancel);
        yield return null;
    }

    public override void OnPopupInit_Continue(UIPopupContinue popup)
    {
        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        popup.stage.text = $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
        popup.stage.MakePixelPerfect();
        popup.rankRoot.SetActive(false);
        popup.SettingBoniDialogBubbleSpaceTravel();
    }

    public override bool CanUseReadyItem(int itemIndex) =>
        itemIndex switch
        {
            0 => true,
            1 => false,
            2 => true,
            3 => true,
            4 => true,
            5 => true,
            6 => CanUseDoubleReadyItem(),
            7 => false,
            _ => false
        };

    public override bool CanUseDoubleReadyItem()
    {
        if (ServerRepos.LoginCdn == null || (ServerRepos.LoginCdn.DoubleReadyItems == null || ServerRepos.LoginCdn.DoubleReadyItems.Length == 0))
        {
            return false;
        }

        return true;
    }
    
    public override bool IsLoadLive2DCharacters()
    {
        var live2Didx           = ManagerSpaceTravel.instance.ReadyCharIdx == 0 ? (int) ManagerSpaceTravel.instance.baseCharacterType : ManagerSpaceTravel.instance.ReadyCharIdx;
        var listLive2DCharacter = new List<TypeCharacterType>();

        if (ManagerCharacter._instance._live2dObjects.ContainsKey(live2Didx) == false)
        {
            listLive2DCharacter.Add((TypeCharacterType) live2Didx);
        }

        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);

        return listLive2DCharacter.Count > 0;
    }

    public override int GetGrowthyPlayCount()
    {
        var userStageData = ServerRepos.UserSpaceTravelEventStage?.Find(x => x.vsn == Global.eventIndex && x.stage == Global.stageIndex) ?? null;

        return userStageData?.play ?? 0;
    }
    
    public override string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventData = new Dictionary<string, object>();

        var itemLineBomb    = 0;
        var itemCircleBomb  = 0;
        var itemRainbowBomb = 0;

        if (ManagerSpaceTravel.instance != null)
        {
            itemLineBomb    = ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.LINE_BOMB];
            itemCircleBomb  = ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.CIRCLE_BOMB];
            itemRainbowBomb = ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.RAINBOW_BOMB];
        }

        integratedEventData.Add("USE_ITEM", ManagerSpaceTravel.instance._useBonusItem);
        integratedEventData.Add("LINE_BOMB", itemLineBomb);
        integratedEventData.Add("CIRCLE_BOMB", itemCircleBomb);
        integratedEventData.Add("RAINBOW_BOMB", itemRainbowBomb);

        return GetJsonData(integratedEventData);
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        if (ManagerSpaceTravel.instance._spaceTravelPackIngame == null)
        {
            ManagerSpaceTravel.instance.AsyncLoadSpaceTravelResource(ManagerSpaceTravel.PrefabType.INGAME).Forget();
        }

        ServerAPI.GameStart<GameStartSpaceTravelResp>(req, complete, failCallback);
    }
    
    public override IEnumerator CoGameClear()
    {
        yield return GameManager.instance.CoClear_SpaceTravel();
    }
    
    public override void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        var resp = r as GameClearResp;

        if (resp.isFirst > 0)
        {
            gm.firstClear = true;
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        gm.gainClover = resp.gainClover > 0;
    }

    public override void CheckKeepPlay()
    {
        if (ManagerSpaceTravel.instance != null && ServerRepos.UserSpaceTravelEventStage[0].play > 0)
        {
            this.keepPlayFlag = true;
        }
    }

    protected override IEnumerator CheckKeepPlay_Internal()
    {
        yield return StartCoroutine(ManagerUI._instance.CoOpenPopupSpaceTravelEvent());
        yield return new WaitUntil(() => UIPopUpSpaceTravel.instance == null);
    }
}