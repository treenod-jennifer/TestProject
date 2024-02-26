using Newtonsoft.Json;
using Protocol;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameType_EndContents : GameType_Base
{
    private GameObject readyPrefab;
    
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
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }
    
    override public bool IsChangeSkin_IngameScoreUI()
    {
        return true;
    }

    override public string GetStageKey()
    {
        int stageIndex = ManagerEndContentsEvent.instance.MapIndex;
        return "pp" + stageIndex + ".xml";
    }
    override public string GetStageFilename()
    {
        return Global.GetHashfromText(GetStageKey()) + ".xml";
    }
    
    override public void SetIngameUI()
    {
        GameUIManager.instance.SetEndContents();
    }
    
    override public IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callbackCancel = null)
    {
        if (ManagerEndContentsEvent.instance.endContentsPack_Ingame == null)
            yield return ManagerEndContentsEvent.instance.LoadEndContentsResource("Ingame");
        ServerAPI.EndContentsRefreshAp((resp) =>
        {
            if (resp.IsSuccess)
                ManagerEndContentsEvent.instance.SyncFromServerUserData_AP();
            ManagerUI._instance.OpenPopupReady_EndContents(tempData, callBackStart, callBackClose, callbackCancel);
        });
    }


    override public string GetText_StageFail(ProceedPlayType proceedPlayType)
    {
        return Global._instance.GetString("p_sf_6");
    }
    
    override public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        popup.stage.text = Global._instance.GetString("p_ec_1");
        popup.SettingBoniDialogBubbleEndContents();
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
    
    override public bool IsFirstPlay()
    {
        return ManagerEndContentsEvent.instance.Play == 1;
    }

    override public string GetStageText_IngamePopup()
    {
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX ||
            NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.STAGING)
        {
            if (Global._instance.ShowECStage)
                return base.GetStageText_IngamePopup();
        }

        return Global._instance.GetString("p_ec_1");
    }
    
    override public string GetStageText_ReadyPopup()
    {
        return Global._instance.GetString("p_ec_1");
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);
    }
    
    public override int GetGrowthy_PLAYEND_L_NUM3()
    {
        return ServerRepos.UserEndContentsEvent.stageIndex;
    }
    
    //게임 시작시, 그로씨에 대한 처리
    public override string GetGrowthy_PLAYSTART_DETAILINFO()
    {
        var integratedEventData = new Dictionary<string, object>();

        if (ManagerGroupRanking.IsGroupRankingStage())
        {
            integratedEventData.Add("L_GROUP_RANKING_EVENT_ACTIVE", true);
        }

        return GetJsonData(integratedEventData);
    }

    public override string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventData = new Dictionary<string, object>();

        var    endContentsMaterial   = 0;
        var    endContentsTotalCount = 0;
        var    endContentsBuff       = 0;
        double endContentsRatio      = 0;

        // 엔드 컨텐츠 (비밀의 섬)
        if (ManagerEndContentsEvent.instance != null)
        {
            endContentsMaterial   = ManagerBlock.instance.endContentsItemCount;
            endContentsRatio      = ManagerEndContentsEvent.instance.GetScoreRatio()[ManagerBlock.instance.flowrCount - 1];
            endContentsTotalCount = ManagerEndContentsEvent.GetPokoCoin() + (endContentsMaterial * (int)endContentsRatio);
            endContentsBuff       = ManagerEndContentsEvent.instance.Buff;
        }

        integratedEventData.Add("L_END_CONTENTS_REWARD_PURE", endContentsMaterial);
        integratedEventData.Add("L_END_CONTENTS_REWARD_MULTIPLIER", endContentsRatio);
        integratedEventData.Add("L_END_CONTENTS_REWARD_TOTAL", endContentsTotalCount);
        integratedEventData.Add("L_END_CONTENTS_SCOREUP", endContentsBuff);

        // 그룹랭킹
        if (ManagerGroupRanking.IsGroupRankingStagePlaying())
        {
            integratedEventData.Add("L_GROUP_RANKING_EVENT", true);
        }

        return GetJsonData(integratedEventData);
    }

    public override void SendClearGrowthyLog()
    {
        var clearResp = GameManager.instance.clearResp as GameClearResp;
        //그로씨 넣기
        //"L_ITEM": [{"L_IID":"1001", "L_CNT":1},{"L_IID":"1011", "L_CNT":2},{"L_IID":"2002", "L_CNT":10}]
        List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM> itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        // 사용한 레디 아이템
        for (int i = 0; i < 8; i++)
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

        ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD growthyStageType = Global.GameInstance.GetGrowthyGameMode();
        ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL growthyStar = GetGrowthyStar();

        int tempScore = ManagerBlock.instance.score;
        float eventRatio = Global.GameInstance.GetBonusRatio();   // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
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

        var getCoinB = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        var rankMode = "N";
        rankMode = Global.GameInstance.GetProfileAgreementString();

        int boostingLevel = 0;
        bool checkEcopiEvent = false;
        
        //권리형 패스 데이터 세팅
        int premiumPassMissionCount = 0;
        bool premiumPassActive = false;
        if (Global.GameInstance.GetProp(GameTypeProp.NEED_PREMIUM_PASS) && ManagerPremiumPass.CheckStartable() && clearResp.userPremiumPass != null)
        {
            premiumPassMissionCount = clearResp.userPremiumPass.targetCount - ManagerPremiumPass.userMissionCount;
            premiumPassActive = clearResp.userPremiumPass != null;
        }

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
            tempEventIndex: Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            remainGoals: null,
            rankMode: rankMode,
            ecopi: checkEcopiEvent == true ? "Y" : "N",
            boostLevel: boostingLevel.ToString(),
            firstFlowerLevel: "Y",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: "[0]"
        );
        
        var doc = JsonConvert.SerializeObject(playEnd);
        doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(clearResp, false));
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        //권리형 패스 갱신
        if (Global.GameInstance.GetProp(GameTypeProp.NEED_PREMIUM_PASS) && ManagerPremiumPass.CheckStartable() && clearResp.userPremiumPass != null)
        {
            ManagerPremiumPass.userMissionCount = clearResp.userPremiumPass.targetCount;
        }

        SendStageFirstClearGrowthyLog();
        
        //앤틱스토어 이벤트
        SendClearRewardGrowthy_AntiqueStore(clearResp);
    }

    public override void InitStage()
    {
        ManagerBlock.instance.endContentsItemCount = 0;
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        if (gm != null)
            gm.firstClear = r.isFirst > 0;
        
        ServerUserTokenAsset asset;
        var ecResp = r as GameClearResp;
        
        int endContentsMaterial = ManagerBlock.instance.endContentsItemCount;
        int endContentsRatio = ManagerEndContentsEvent.instance.GetScoreRatio()[ManagerBlock.instance.flowrCount - 1];
        
        if( ecResp != null && ServerRepos.UserTokenAssets.TryGetValue(3, out asset) )
        {
            var GetHeart = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.EC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                0,
                endContentsMaterial * endContentsRatio,
                0,
                asset.amount + endContentsMaterial * endContentsRatio,
                mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
            );
            var doc = JsonConvert.SerializeObject(GetHeart);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }
        
        QuestGameData.SetUserData();
    }
    
    public override void OnRecvGameRestart()
    {
        ManagerEndContentsEvent.instance.SyncFromServerUserData_Ingame();
    }

    override public string GetGrowthyStageIndex()
    {
        return ManagerEndContentsEvent.instance.MapIndex.ToString();
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.END_CONTENTS;
    }

    public override int GetGrowthyPlayCount()
    {
        return ManagerEndContentsEvent.instance.Play;
    }

    public override void CheckKeepPlay()
    {
        if (ServerRepos.UserEndContentsEvent != null && ManagerEndContentsEvent.instance.Play > 0)
        {
            this.keepPlayFlag = true;
        }
    }
    
    override public GameObject GetSpine_IngameScoreUI()
    {
        return ManagerEndContentsEvent.instance.endContentsPack_Ingame?.ScoreSpine ?? null;
    }
    
    override public GameObject GetSpine_GameClearPopup()
    {
        return ManagerEndContentsEvent.instance.endContentsPack_Ingame.ResultSpineObj ?? null;
    }
    
    override protected IEnumerator CheckKeepPlay_Internal()
    {
        if(ServerRepos.UserEndContentsEvent != null && ManagerEndContentsEvent.instance.Play > 0)
        {
            ManagerUI._instance.OpenPopupReadyEndContents();
            yield return new WaitUntil(() => NetworkLoading._tempInstance == null);
            yield return new WaitUntil(() => UIPopupReady._instance == null);
        }
        
        yield return null;
    }
    
    public override float GetBonusRatio()
    {
        return ManagerEndContentsEvent.instance.GetBuffRatio()[ManagerEndContentsEvent.instance.Buff];
    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartEndContentsResp>(req, complete, failCallback);
    }
    
    override public int GetIngameBGIndex()
    {
        return ManagerEndContentsEvent.instance.ResourceIndex;
    }
    
    override public IEnumerator LoadIngameBGAtlas()
    {
        yield return ManagerUIAtlas._instance.LoadIngameBGAtlas(GetIngameBGIndex());
    }
    
    override public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        return ("EC_clear_start", "EC_clear_loop");
    }
    
    override public void OpenPopupClear()
    {
        if (ManagerUI._instance.CheckExitUI()) ManagerUI._instance.ClosePopUpUI();
        ManagerUI._instance.OpenPopup<UIPopupClear_EndContents>((popup) => popup.InitPopup());
        ManagerUI._instance.FocusCheck();
    }
}
