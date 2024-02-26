using Protocol;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class GameType_Atelier : GameType_Base
{
    private GameObject _readyPrefab;

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
        Properties[GameTypeProp.FLOWER_ON_INGAME]              = this.FlowerOn_InGame;
        Properties[GameTypeProp.CAN_USE_READYITEM]             = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT]        = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE]                  = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE]            = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL]    = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS]             = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.USE_READY_CHARACTER]           = GameType_Base.ReturnTrue;
    }

    public override bool       IsChangeSkin_IngameScoreUI() => true;
    public override string     GetStageKey()                => $"AT{Global.eventIndex}_{Global.stageIndex}.xml";
    public override string     GetStageFilename()           => GetStageKey();
    public override void       SetIngameUI()                => GameUIManager.instance.SetAtelier();
    public override GameObject GetSpine_IngameScoreUI()     => ManagerAtelier.instance._atelierInGamePack?.ScoreSpineObj  ?? null;
    public override GameObject GetSpine_GameClearPopup()    => ManagerAtelier.instance._atelierInGamePack?.ResultSpineObj ?? null;

    private bool FlowerOn_InGame() => ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.SCORE;

    public override ScoreFlowerType GetMaxType_FlowerScore()
    {
        if (ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.SCORE)
        {
            return ScoreFlowerType.FLOWER_WHITE;
        }
        else
        {
            return base.GetMaxType_FlowerScore();
        }
    }

    public override IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null,
        System.Action                                            callbackCancel = null)
    {
        ManagerUI._instance.OpenPopupReady_Event<UIPopupReady_Atelier>(tempData, callBackStart, callBackClose, callbackCancel);
        yield return null;
    }

    public override void OnPopupInit_Continue(UIPopupContinue popup)
    {
        popup.stage.text = GetStageText_ReadyPopup();
        popup.stage.MakePixelPerfect();
        popup.rankRoot.SetActive(false);

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
    }

    public override bool CanUseReadyItem(int itemIndex) =>
        itemIndex switch
        {
            0 => true,
            1 => ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.SCORE,
            2 => true,
            3 => true,
            4 => true,
            5 => true,
            6 => CanUseDoubleReadyItem(),
            7 => CanUseDoubleReadyItem() && ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.SCORE,
            _ => false
        };

    public override bool CanUseDoubleReadyItem()
    {
        if (ServerRepos.LoginCdn == null
            || (ServerRepos.LoginCdn.DoubleReadyItems == null || ServerRepos.LoginCdn.DoubleReadyItems.Length == 0))
        {
            return false;
        }

        return true;
    }

    public override bool IsFirstPlay() => ServerRepos.UserAtelierStage == null || ServerRepos.UserAtelierStage.Count == 0;

    public override string GetStageText_ReadyPopup() =>
        $"{Global._instance.GetString("atelier_sr_1").Replace("[n]", ManagerAtelier.instance.GetStageIndexFromPuzzle(ManagerAtelier.instance._currentPuzzleIndex).ToString())}";

    public override string GetStageText_IngamePopup() =>
        $"{Global._instance.GetString("atelier_t_1")} {ManagerAtelier.instance.GetStageIndexFromPuzzle(ManagerAtelier.instance._currentPuzzleIndex)}";

    public override string GetStageText_ClearPopup() =>
        $"{Global._instance.GetString("atelier_t_1")} {ManagerAtelier.instance.GetStageIndexFromPuzzle(ManagerAtelier.instance._currentPuzzleIndex)}";

    public override void CheckKeepPlay()
    {
        if (ManagerAtelier.instance != null)
        {
            this.keepPlayFlag = true;
        }
    }

    protected override IEnumerator CheckKeepPlay_Internal()
    {
        var isClear = ManagerAtelier.instance.IsPuzzleClear(ServerRepos.UserAtelier.lastSelectIndex, ServerRepos.UserAtelier.puzzleState);

        yield return ManagerUI._instance.CoOpenPopupAtelier(isClear);

        yield return new WaitWhile(() => UIPopupAtelier.instance != null);
    }

    public override void SendClearGrowthyLog()
    {
        var isClear = ManagerAtelier.instance.IsPuzzleClear(ServerRepos.UserAtelier.lastSelectIndex, ServerRepos.UserAtelier.puzzleState);
        if (isClear)
        {
            UIPopupAtelier.RewardGrowthy(null, 0, true);
        }

        SendClearGrowthyLog_Regular(false);
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar() => (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);

    public override string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventData = new Dictionary<string, object>();

        //퍼즐 명화 게임 타입 디테일 값 추가
        integratedEventData.Add("L_ATELIER_EVENT_GAME_TYPE", (ManagerAtelier.instance._mode).ToString());

        return GetJsonData(integratedEventData);
    }

    public override void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        var resp = r as GameClearResp;

        if (resp != null && resp.isFirst > 0)
        {
            gm.firstClear = true;
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        if (resp != null)
        {
            gm.gainClover = resp.gainClover > 0;
        }
    }

    //순차 : 3 고정, 스코어 : 1,2,3(기존 스코어 적용 string과 동일)
    public override (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        if (ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.COLLECT)
        {
            return ("clear_3_start", "clear_3_loop");
        }
        else
        {
            return ("", "");
        }
    }

    public override int GetFirstClearRewardType() => (int)FirstClearRewardType.CLOVER;

    public override string GetGrowthyStageIndex() => Global.stageIndex.ToString();

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        switch (ManagerAtelier.instance._mode)
        {
            case ManagerAtelier.PuzzleGameType.SCORE:
                return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ATELIER_SCORE;
            case ManagerAtelier.PuzzleGameType.COLLECT:
            default:
                return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ATELIER_COLLECT;
        }
    }

    public override int GetGrowthyPlayCount()
    {
        var userStageData = ServerRepos.UserAtelierStage?.Find(x => x.eventIndex == Global.eventIndex && x.stage == Global.stageIndex) ?? null;

        return userStageData?.play ?? 0;
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartAtelierResp>(req, complete, failCallback);

        ManagerAtelier.instance.CoLoadAssetBundle<AtelierInGamePack>().Forget();
    }

    public override bool IsLoadLive2DCharacters()
    {
        if (ManagerAtelier.instance._live2dIndex == 0)
        {
            return false;
        }

        var listLive2DCharacter = new List<TypeCharacterType>();

        if (ManagerCharacter._instance._live2dObjects.ContainsKey((int)(ManagerAtelier.instance._live2dIndex)) == false)
        {
            listLive2DCharacter.Add((TypeCharacterType)ManagerAtelier.instance._live2dIndex);
        }

        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);

        return listLive2DCharacter.Count > 0;
    }
}