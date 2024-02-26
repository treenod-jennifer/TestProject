using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Protocol;
using ServiceSDK;
using UnityEngine;

public class GameType_TurnRelay: GameType_Base
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
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }

    override public string GetStageKey()
    {
        var stg = ServerRepos.UserTurnRelayEvent.stage;
        var stg_selected = ServerRepos.UserTurnRelayEvent.stageList[ServerRepos.UserTurnRelayEvent.stage - 1];
        return $"TR_{stg}_{stg_selected}.xml";
    }

    override public string GetStageFilename()
    {
        return GetStageKey();
    }

    override public string GetDefaultMapName()
    {
        return "TR_1_1.xml";
    }

    override public void SetMoveCount_Ingame(int gameMode, int dicCount, int turnCount)
    {
        if (gameMode == (int)GameMode.LAVA)
            GameManager.instance.moveCount = dicCount;
        else
            GameManager.instance.moveCount = turnCount + ManagerTurnRelay.turnRelayIngame.GetBonuseTurn_AtWave();
    }

    override public void SetIngameUI()
    {
        GameUIManager.instance.SetEventUI_TurnRelay();
        GameUIManager.instance.coinRoot.SetActive(false);
        GameUIManager.instance.scoreRoot.SetActive(false);
    }

    #region 게임 시작시, 연출 및 설정 해주는 함수
    //목표 팝업이 등장하기 전, 게임 타입별로 실행될 액션
    override public IEnumerator CoActionIngameStart_BeforeOpenTargetPopup()
    {
        //보너스 아이템 선택 팝업
        bool isClosePopup = false;
        ManagerTurnRelay.BONUSITEM_TYPE selectItemType = ManagerTurnRelay.BONUSITEM_TYPE.EVENTPOINT;
        ManagerUI._instance.OpenPopupTurnRelay_SelectItem(
            () => isClosePopup = true,
            (type) => selectItemType = type);

        //팝업 닫힐 때 까지 대기
        yield return new WaitUntil(() => isClosePopup == true);

        //아이템 증가 연출
        if (selectItemType == ManagerTurnRelay.BONUSITEM_TYPE.LINE_BOMB 
            || selectItemType == ManagerTurnRelay.BONUSITEM_TYPE.CIRCLE_BOMB 
            || selectItemType == ManagerTurnRelay.BONUSITEM_TYPE.RAINBOW_BOMB)
            yield return GameUIManager.instance.CoActionItemAddCount_TurnRelay(selectItemType);

        //포인트 증가 연출
        if (ManagerTurnRelay.turnRelayIngame.BonusEventPoint> 0)
            yield return GameUIManager.instance.CoActionAddEventPoint_TurnRelay();

        //현재까지 모은 인게임 포인트 말풍선 표시
        yield return GameUIManager.instance.CoActionShowAllPointBubble_TurnRelay();

        //인게임 턴 증가 연출(사과 선택 + 이월된 턴)
        if (ManagerTurnRelay.turnRelayIngame.RemainTurn > 0)
            yield return GameUIManager.instance.CoActionAddTurn_WithMakeBubble(ManagerTurnRelay.turnRelayIngame.RemainTurn);

        //이월된 턴 초기화
        ManagerTurnRelay.turnRelayIngame.RemainTurn = 0;
    }
    #endregion

    #region 일시정지 팝업 관련
    override public GameObject OpenPopupPause()
    {
        return ManagerUI._instance.OpenPopupTurnRelay_IngamePause().gameObject;
    }

    override public string GetStageText_IngamePopup()
    {
        return string.Format("{0} {1}/{2}", Global._instance.GetString("p_sc_5"),
            ManagerTurnRelay.turnRelayIngame.CurrentWave, ManagerTurnRelay.instance.MaxWaveCount);
    }
    #endregion

    override public IEnumerator CoGameClear()
    {
        yield return GameManager.instance.CoWaveClear_TurnRelay();
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase resp)
    {
        if (gm != null)
            gm.firstClear = resp.isFirst > 0;

        ServerUserTokenAsset asset;
        var trResp = resp as TurnRelayGameClearResp;

        //현재 스코어 및 유예기간 갱신
        if (trResp != null)
        {
            ManagerTurnRelay.turnRelayCoop.myScore = trResp.finalScore;
            ManagerTurnRelay.turnRelayIngame.IsPlayEnd = trResp.eventEnd;
        }
       
        if( trResp != null && ServerRepos.UserTokenAssets.TryGetValue(2, out asset) )
        {
            var GetHeart = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AT_CAPSULEMEDAL,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TURN_RELAY_PLAY,
                    0,
                    trResp.getTokenCount,
                    0,
                    asset.amount,
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
            var doc = JsonConvert.SerializeObject(GetHeart);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }
    }

    override public void OpenPopupClear()
    {
        if (ManagerTurnRelay.turnRelayIngame.CurrentWave < 5)
            ManagerUI._instance.OpenPopupTurnRelay_WaveClear();
        else
            ManagerUI._instance.OpenPopupTurnRelay_IngameClear();
    }

    override public void OnPopupInit_Ready(UIPopupReady popup)
    {
    }

    #region 레디 아이템 관련
    // 레디아이템 종류별로 사용가능여부 체크
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

    override public int GetReadyItem_AddTurnCount()
    {   //턴 릴레이 모드에서는 턴 추가 아이템 사용하면 목표창 전에 떠야하므로, 바로 RemainTurn으로 들어감
        return 0;
    }
    #endregion

    override public bool IsChangeSkin_IngameScoreUI()
    {
        return true;
    }
    
    override public bool IsHideDefaultResultUI()
    {
        return true;
    }

    public override GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return GrowthyCustomLog_PLAYEND.Code_L_GMOD.TURN_RELAY;
    }

    public override int GetGrowthyPlayCount()
    {
        return ServerRepos.UserTurnRelayEvent.play;
    }

    override public bool IsFirstPlay()
    {
        return ServerRepos.UserTurnRelayEvent.play == 1;
    }

    public override string GetGrowthyStageIndex()
    {
        return ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString();
    }

    #region 인게임 클리어 팝업 관련
    override public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        if (IsStageAllClear() == true)
            return ("turn_clear_2_start", "turn_clear_2_loop"); 
        else
            return ("turn_clear_1_start", "turn_clear_1_loop");
    }

    override public IEnumerator PlaySound_ResultStar(bool isSkip, int tempScore)
    {
        if (isSkip == true)
        {
            if (IsStageAllClear() == false)
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);
            else
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR1);

            yield return new WaitForSeconds(0.5f);
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);

            //마지막 스테이지까지 클리어 했는지 여부에 따라 소리 다르게 출력
            if (IsStageAllClear() == true)
            {
                yield return new WaitForSeconds(0.7f);
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
            }
            yield return new WaitForSeconds(0.6f);
        }
        yield break;
    }

    private bool IsStageAllClear()
    {
        return (ManagerTurnRelay.turnRelayIngame.CurrentWave >= ManagerTurnRelay.instance.MaxWaveCount);
    }
    #endregion

    //게임 클리어 후, 보상 관련해서 출력되는 팝업
    public override IEnumerator CoOpenClearRewardPopup()
    {
        yield break;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }

    public override void SendClearGrowthyLog()
    {
        // Growthy 그로씨
        // 사용한 아이템
        var itemList = new List<GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 0; i < 8; i++)
        {
            var usedReadyItemCount = ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(i);
            if (usedReadyItemCount > 0)
            {
                var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
                    L_IID = ((READY_ITEM_TYPE)i).ToString(),
                    L_CNT = usedReadyItemCount
                };
                itemList.Add(readyItem);
            }
        }

        for (var i = 0; i < 4; i++)
        {
            if (ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount[i] > 0)
            {
                var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = ((GameItemType)i + 1).ToString(),
                    L_CNT = ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount[i]
                };
                itemList.Add(readyItem);
            }
        }

        if (ManagerTurnRelay.turnRelayIngame.TotalContinueCount > 0)
        {
            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = ManagerTurnRelay.turnRelayIngame.TotalContinueCount
            };
            itemList.Add(readyItem);
        }

        var docItem = JsonConvert.SerializeObject(itemList);

        // 스테이지 모드
        var growthyStageType = Global.GameInstance.GetGrowthyGameMode();

        // 획득한 꽃 갯수
        var growthyStar = GetGrowthyStar();

        // 획득한 이벤트 포인트
        var tempScore  = ManagerTurnRelay.turnRelayIngame.IngameEventPoint;
        
        // 이월된 턴 수
        var remainTurn = ManagerTurnRelay.turnRelayIngame.RemainTurn;

        // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
        var rankMode = Global.GameInstance.GetProfileAgreementString();

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var clearResp = GameManager.instance.clearResp as GameClearResp;

        var playEnd = new GrowthyCustomLog_PLAYEND
        (
            pid: myProfile.userID,
            lastStage: (myProfile.stage       - 1).ToString(),
            beforeLastStage: (myProfile.stage - 1).ToString(),
            currentStage: Global.GameInstance.GetGrowthyStageIndex(),
            stageType: growthyStageType,
            win: GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
            star: growthyStar,
            score: tempScore,
            getCoin: 0,
            playTime: ManagerTurnRelay.turnRelayIngame.TotalPlayTime,
            firstPlay: Global.GameInstance.IsFirstPlay(), //최초플레이
            continuePlay: ManagerTurnRelay.turnRelayIngame.TotalContinueCount > 0,
            leftTurn: remainTurn, //남은턴 다시계산
            useItemList: docItem,
            getHeart: 0,
            clearStageMission: false,
            clearChapterMission: false,
            reviewCount: "0",
            tempEventIndex: Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            remainGoals: null,
            rankMode: rankMode,
            ecopi: "N",
            boostLevel: "0",
            firstFlowerLevel: "Y",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: "[0]"
        );

        var doc = JsonConvert.SerializeObject(playEnd);
        doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(clearResp, false));
        ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGame") // 중도포기를 한 경우에는 무조건 first clear가 있을 수 없다
        {
            SendStageFirstClearGrowthyLog();
        }
    }
    
    //게임 시작시, 그로씨에 대한 처리
    public override string GetGrowthy_PLAYSTART_DETAILINFO()
    {
        var integratedEventData = new Dictionary<string, object>();
        
        if (ManagerTurnRelay.turnRelayIngame != null && ManagerTurnRelay.turnRelayIngame.IsSaveWave)
        {            
            integratedEventData.Add("L_IS_SAVEWAVE", true);
        }
        
        return GetJsonData(integratedEventData);
    }
    
    public override string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventData = new Dictionary<string, object>();
        
        if (Global.GameType == GameType.TURN_RELAY && ManagerTurnRelay.turnRelayIngame != null && ManagerTurnRelay.turnRelayIngame.SaveWaveCount > 0)
        {            
            integratedEventData.Add("L_IS_SAVEWAVE", true);
            integratedEventData.Add("L_SAVEWAVECOUNT", ManagerTurnRelay.turnRelayIngame.SaveWaveCount);
        }
        
        return GetJsonData(integratedEventData);
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        // 올클리어 처음으로 한 경우        
        if (GameManager.instance.firstClear)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.TURN_RELAY_EVENT,
                    $"TR_{ServerContents.TurnRelayEvent?.eventIndex}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );

            achieve.L_NUM1 = ServerRepos.UserTurnRelayEvent.play;
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    public override void CheckKeepPlay()
    {
        if (ServerRepos.UserTurnRelayEvent != null && ServerRepos.UserTurnRelayEvent.play > 0)
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        ManagerTurnRelay.OpenTurnRelay();
        yield return new WaitUntil(() => UIPopupTurnRelay_StageReady._instance == null);
    }
    
    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.TurnRelayGameStart(req, complete, failCallback);
    }

    #region 인게임 컨티뉴 팝업 관련
    override public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        popup.stage.text = Global.GameInstance.GetStageText_IngamePopup();
        popup.rankRoot.SetActive(false);

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        popup.SettingBoniDialogBubbleTurnRelay();
    }
    #endregion

    #region 인게임 실패 팝업 관련
    //스테이지 실패 팝업에서 사용할 텍스트 가져오기
    public override string GetText_StageFail(ProceedPlayType proceedPlayType)
    {
        var textKey = "p_sf_5";

        if (ManagerTurnRelay.turnRelayIngame != null && ManagerTurnRelay.turnRelayIngame.IsTurnRelayRestart())
        {
            textKey = "p_ads_12"; // 광고 보기 후 웨이브 유지 텍스트
        }

        return Global._instance.GetString(textKey);
    }
    #endregion

    /// <summary>
    /// 레디, 인게임 아이템 사용 여부
    /// </summary>
    /// <returns></returns>
    public override List<bool> GetUseItemData()
    {
        List<bool> tempUseItemDatas = new List<bool>();

        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[0] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[1] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[2] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[3] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[4] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[5] > 0);

        var index = 0;
        foreach (var item in ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount)
        {
            if (index >= 4)
            {
                break;
            }
            
            tempUseItemDatas.Add(item > 0 || GameItemManager.useCount[index] > 0);
            index++;
        }
        
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[15] > 0);
        tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listUseReadyItem[16] > 0);

        if (ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount.Count >= 5)
        {
            tempUseItemDatas.Add(ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount[4] > 0 || GameItemManager.useCount[7] > 0);
        }
        
        return tempUseItemDatas;
    }
}