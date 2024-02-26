using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ServiceSDK;

public class GameType_CoinBonusStage : GameType_Base
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
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }

    override public string GetStageKey()
    {
        return string.Format("cbs{0}.xml", ManagerCoinBonusStage.instance.GetMapIndex());
    }

    override public string GetStageFilename()
    {
        return Global.GetHashfromText(GetStageKey()) + ".xml";
    }

    override public string GetDefaultMapName()
    {
        return Global.GetHashfromText("cbs1.xml") + ".xml";
    }

    override public bool CanUseReadyItem(int itemIndex)
    {
        return false;
    }

    override public bool CanUseIngameItem(GameItemType itemIndex)
    {
        return false;
    }

    override public int GetStageVer()
    {
        return 0;
    }

    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.NONE;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.COINBONUS;
    }

    public override int GetGrowthyPlayCount()
    {
        return 0;
    }

    public override string GetGrowthyStageIndex()
    {
        return ManagerCoinBonusStage.instance.GetStageIndex().ToString();
    }

    override public bool IsFirstPlay()
    {
        if (PlayerPrefs.HasKey("CoinStageTutorial_Ingame") == false)
        {
            if( Global.stageIndex != 1)
            {
                PlayerPrefs.SetInt("CoinStageTutorial_Ingame", 1);
                return false;
            }

            return true;
        }
        else
            return false;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }

    public override void SendClearGrowthyLog()
    {
        base.SendClearGrowthyLog_Regular(false);
        
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.COINBONUSSTAGE,
            $"{ManagerCoinBonusStage.instance.GetStageIndex()}_{ManagerCoinBonusStage.instance.GetMapIndex()}_{GameManager.instance.useContinueCount}_{ManagerCoinBonusStage.instance.GetCurrentBestScore()}",
            GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );

        achieve.L_NUM1 = ServerRepos.UserTurnRelayEvent.play;
        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }


    #region 게임 시작시, 연출 및 설정 해주는 함수
    //목표 팝업이 등장하기 전, 게임 타입별로 실행될 액션
    override public IEnumerator CoActionIngameStart_BeforeOpenTargetPopup()
    {
        GameUIManager.instance.SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.APPEAR);
        yield return null;
    }

    //튜토리얼이 등장한 뒤, 게임 타입별로 실행될 액션
    override public IEnumerator CoActionIngameStart_AfterTutorial()
    {
        yield return GameUIManager.instance.CoStartCountDownAction();
        GameUIManager.instance.SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.IDLE);
    }
    #endregion

    #region 인게임 컨티뉴 팝업 관련

    public override void OnPopupInit_Continue(UIPopupContinue popup)
    {
        popup.stage.text = Global._instance.GetString("p_cs_sc_1").Replace("[n]", $"{Global.stageIndex}");
        popup.rankRoot.SetActive(false);
        popup.bonusRoot.SetActive(false);
        
        popup.SettingCoinStage();
    }

    public override int GetContinueCost()
    {
        return ManagerCoinBonusStage.instance.GetContinuePrice();
    }
    
    public override bool isContinueSale()
    {
        return false;
    }

    #endregion
    
    override public bool IsHideIngameItemUI()
    {
        return true;
    }

    override public bool IsHideDefaultResultUI()
    {
        return true;
    }

    override public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        return ("clear_Coin_start", "clear_Coin_loop");
    }

    override public string GetStageTargetText()
    {
        return Global._instance.GetString("p_cs_tag_0");
    }

    override public bool IsOpenTimeOverUI()
    {
        return true;
    }

    //인게임 UI 설정
    override public void SetIngameUI()
    {
        GameUIManager.instance.SetCoinStage();
    }

    override public void OpenPopupClear()
    {
        ManagerUI._instance.OpenPopupClear_CoinStage();
    }

    override public IEnumerator PlaySound_ResultStar(bool isSkip, int tempScore)
    {
        if (isSkip == true)
        {
            if (ManagerCoinBonusStage.instance.GetCurrentBestScore() > ManagerCoinBonusStage.instance.GetStoredBestScore())
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
            else
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);
        }
        else
        {
            float timer = 0f;
            while (timer < 0.2f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR1);

            timer = 0f;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);

            //기록갱신
            if (ManagerCoinBonusStage.instance.GetCurrentBestScore() > ManagerCoinBonusStage.instance.GetStoredBestScore())
            {
                timer = 0f;
                while (timer < 0.7f)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
            }

            timer = 0f;
            while (timer < 0.6f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        yield break;
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        Global.flower = (int)GameData.User.flower;

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();
    }

    override public void OnRecvGameFail()
    {
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartCoinStageResp>(req, complete, failCallback);
    }
    
    
}

