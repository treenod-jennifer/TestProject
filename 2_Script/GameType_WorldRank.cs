using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Protocol;
using ServiceSDK;
using UnityEngine;

public class GameType_WorldRank : GameType_Base
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
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }
    public override void InitStage()
    {
        ManagerBlock.instance.worldRankingItemCount = 0;
    }

    override public string GetStageKey()
    {
        int stg = ManagerWorldRanking.userData?.CurrentStage ?? 1;
        int stgFile = ManagerWorldRanking.contentsData.GetStageFileIndex(stg);
        bool easyFlag = ManagerWorldRanking.contentsData.IsEasyStage(stg);
        
        return $"WR_{(easyFlag ? "S_" : "")}{stgFile}.xml";
    }

    override public string GetStageFilename()
    {   
        return GetStageKey();
    }

    override public string GetDefaultMapName()
    {
        return "WR_1.xml";
    }

    override public int GetIngameBGIndex()
    {
        return ServerContents.WorldRank.themeId;
    }

    override public IEnumerator LoadIngameBGAtlas()
    {
        yield return ManagerUIAtlas._instance.LoadIngameBGAtlas(GetIngameBGIndex());
    }
    
    override public IEnumerator GameModeProcess_OnIngameStart()
    { 
        yield return base.GameModeProcess_OnIngameStart();
        ManagerWorldRanking.userData.SyncForGameClearLog();
        yield break;
    }

    //인게임 UI 설정
    override public void SetIngameUI()
    {
        GameUIManager.instance.SetWorldRanking();
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        gm.firstClear = true;
        QuestGameData.SetUserData();
    }

    override public void SetMoveCount_Ingame(int gameMode, int dicCount, int turnCount)
    {
        int reduceCount = ManagerWorldRanking.contentsData.GetRound(Global.stageIndex);
        if (reduceCount > 5)
            reduceCount = 5;

        if (gameMode == (int)GameMode.LAVA)
            GameManager.instance.moveCount = dicCount;
        else
            GameManager.instance.moveCount = turnCount - reduceCount;
    }

    override public void OnPopupInit_Ready(UIPopupReady popup)
    {
        popup.changeBGTexture.mainTexture = ManagerWorldRanking.resourceData?.worldRankingPack?.ReadyTitle ?? null;
        if (popup.changeBGTexture.mainTexture == null)
        {
            popup.labelBGTexture.gameObject.SetActive(false);
            return;
        }
        else
        {
            popup.labelBGTexture.gameObject.SetActive(true);
        }
        
        if (ManagerWorldRanking.GetEventState() == ManagerWorldRanking.EventState.NEED_PARTICIPATE)
            popup.worldRankShopObj.SetActive(true);
        popup.flowerBGObj.SetActive(false);
    }
    
    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return true;
            case 1: return true;
            case 2: return true;
            case 3: return true;
            case 4: return true;
            case 5: return true;
            case 6: return CanUseDoubleReadyItem();
            case 7: return CanUseDoubleReadyItem();
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

    override public GameObject GetSpine_IngameScoreUI()
    {
        return ManagerWorldRanking.resourceData?.worldRankingPack?.ScoreSpine ?? null;
    }

    override public bool IsChangeSkin_IngameScoreUI()
    {
        return true;
    }

    override public GameObject GetSpine_GameClearPopup()
    {
        return ManagerWorldRanking.resourceData?.worldRankingPack?.ResultSpineObj ?? null;
    }

    override public bool IsHideDefaultResultUI()
    {
        return true;
    }

    override public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        return ("clear_W_start", "clear_W_loop");
    }

    override public void OpenPopupClear()
    {
        ManagerUI._instance.OpenPopupClear_WorldRanking();
    }

    override public IEnumerator PlaySound_ResultStar(bool isSkip, int tempScore)
    {
        if (isSkip == true)
        {
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
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

            timer = 0f;
            while (timer < 0.7f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);

            timer = 0f;
            while (timer < 0.6f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        yield break;
    }

    override public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        base.OnPopupInit_Continue(popup);
        
        int userContinueCount = ManagerWorldRanking.userData.SaleContinueCount;
        int allContinueCount = ServerContents.WorldRank.saleContinuePrices.Length;
        if (userContinueCount < allContinueCount)
        {
            popup.powerSaleObj.SetActive(true);
            popup.powerSaleLabel.text = Global._instance.GetString("p_ctn_12").
                Replace("[n]", (allContinueCount - userContinueCount).ToString()).Replace("[m]", allContinueCount.ToString());
        }
        else
        {
            popup.powerSaleObj.SetActive(false);
        }

        popup.SettingBoniDialogBubbleWorldRank();
    }

    override public int GetContinueCost()
    {
        //사용할 수 있는 파워세일 카운트
        int leftPowerSaleCount = (ManagerWorldRanking.userData.SaleContinueCount < ServerContents.WorldRank.saleContinuePrices.Length) ?
            ServerContents.WorldRank.saleContinuePrices.Length - ManagerWorldRanking.userData.SaleContinueCount : 0;

        //현재 판에서 사용한 컨티뉴 카운트
        int useContinueCount = GameManager.instance.useContinueCount;

        if (leftPowerSaleCount > 0 && useContinueCount < ServerContents.WorldRank.saleContinuePrices.Length)
            return ServerContents.WorldRank.saleContinuePrices[useContinueCount];
        else
            return base.GetContinueCost();
    }

    override public bool isContinueSale()
    {
        //월드랭킹에서의 파워세일이 진행중이면, 파워세일 처리부터 우선적으로 해줌.
        if (ManagerWorldRanking.userData.SaleContinueCount < ServerContents.WorldRank.saleContinuePrices.Length)
            return false;
        else
            return base.isContinueSale();
    }
    public override GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return GrowthyCustomLog_PLAYEND.Code_L_GMOD.WORLD_RANKING;
    }

    public override int GetGrowthyPlayCount()
    {
        return ManagerWorldRanking.userData?.PlayCount ?? 0;
    }

    override public bool IsFirstPlay()
    {
        return ServerRepos.UserWorldRank.play == 1;
    }

    public override string GetGrowthyStageIndex()
    {
        return ManagerWorldRanking.userData.CurrentStage.ToString();
    }

    //게임 클리어 시, 보상받는 그로씨에 대한 처리
    public override IEnumerator CoOpenClearRewardPopup()
    {
        if (GameManager.instance.clearResp != null)
            yield return null;

        Protocol.GameClearResp clearResp = GameManager.instance.clearResp as GameClearResp;
        if (clearResp != null && clearResp.worldRank != null && clearResp.worldRankEnded == true)
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_wrk_6"), false, null);
            popup.SortOrderSetting();

            //팝업 닫힐때까지 대기
            yield return new WaitUntil(() => { return UIPopupSystem._instance == null; });
        }
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);
    }

    public override void SendClearGrowthyLog()
    {
        // Growthy 그로씨
        // 사용한 레디 아이템
        var itemList = new List<GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 0; i < 8; i++)
        {
            if (UIPopupReady.readyItemUseCount[i].Value > 0)
            {
                var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
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
            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = GameManager.instance.useContinueCount
            };
            itemList.Add(readyItem);
        }
        var docItem = JsonConvert.SerializeObject(itemList);

        // 획득한 스코어
        var tempScore  = ManagerBlock.instance.score;
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
        
        // 획득한 코인
        var getCoinB = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
        var rankMode = Global.GameInstance.GetProfileAgreementString();

        // 노이 부스팅 단계
        var boostingLevel   = 0;
        
        // 에코피 이벤트
        var checkEcopiEvent = false;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var playEnd = new GrowthyCustomLog_PLAYEND
        (
            myProfile.userID,
            (myProfile.stage - 1).ToString(),
            GameManager.instance.GrowthyAfterStage.ToString(),
            Global.GameInstance.GetGrowthyStageIndex(),
            growthyStageType,
            GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
            growthyStar,
            tempScore,
            getCoinB, //ManagerBlock.instance.coins,
            (long)(Time.time - GameManager.instance.playTime),
            GameManager.instance.firstPlay, //최초플레이
            GameManager.instance.useContinueCount > 0,
            GameManager.instance.leftMoveCount, //남은턴 다시계산
            docItem,
            GameManager.instance.firstClear ? 1 : 0,
            GameManager.instance.clearMission        > 0,
            GameManager.instance.allStageClearReward > 0,
            GameManager.instance.stageReview,
            Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            null,
            rankMode,
            checkEcopiEvent == true ? "Y" : "N",
            boostLevel: boostingLevel.ToString(),
            firstFlowerLevel: "Y",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: null
        );
        
        var doc = JsonConvert.SerializeObject(playEnd);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        SendStageFirstClearGrowthyLog();
        ManagerWorldRanking.userData.SyncForGameClearLog();
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        if (GameManager.instance.firstClear)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WORLD_RANKING,
                    $"WR_{ServerContents.WorldRank?.eventIndex}_{ManagerWorldRanking.userData?.CurrentStage}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            
            achieve.L_NUM1 = ManagerWorldRanking.userData.PlayCount;
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    public override void CheckKeepPlay()
    {
        if (ServerRepos.UserWorldRank != null && ServerRepos.UserWorldRank.stage > 1)
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        if(ServerRepos.UserWorldRank != null && ServerRepos.UserWorldRank.stage > 1)
        {
            ManagerUI._instance.OpenPopup<UIPopUpWorldRank>((popup) => popup.InitPopup(true));
            yield return new WaitUntil(() => UIPopUpWorldRank._instance == null);
        }
        else
        {
            yield break;
        }
    }
    
    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartWorldRankResp>(req, complete, failCallback);
    }
}
