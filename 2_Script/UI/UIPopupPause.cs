using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using Protocol;
using UnityEngine.Networking;

public class UIPopupPause : UIPopupBase
{
    static public UIPopupPause _instance = null;

    public UILabel[] title;
    public UILabel stageText;

    public GameObject btnClose;
    public GameObject btnContinue;
    public GameObject btnMap;
    public GameObject btnRetry;
    
    public GameObject objEndContentsInfo;
    public UIItemEndContentsBoost itemEndContentsBuff;
    
    //노이 부스팅 이벤트 관련
    [SerializeField]
    UIIngameBoostingGauge noyBoostingGauge;
    [SerializeField]
    List<UISprite> noySpriteList;

    public UISprite spriteContinue;
    public UISprite spriteMap;

    [System.NonSerialized]
    public PrevGameReservedRestartData preservedRestart = null;

    public class PrevGameReservedRestartData
    {
        public int stageIdx;
        public GameFailReq failReq;
        public string growthyLog;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    protected override void OnDestroy()
    {   
        _instance = null;
        base.OnDestroy();

        if (ManagerBlock.instance != null && Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            ManagerBlock.instance.stageInfo.waitState = 0;
        }

        if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage())
        {
            foreach (var spr in noySpriteList)
                spr.atlas = null;
        }
    }

    private void Start()
    {
        ManagerSound._instance.PauseBGM();

        if (ManagerBlock.instance != null && Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            ManagerBlock.instance.stageInfo.waitState = 1;
        }

        stageText.text = Global.GameInstance.GetStageText_IngamePopup();
        stageText.MakePixelPerfect();

        if (Global.GameInstance.GetProp(GameTypeProp.CAN_RETRY_AT_PAUSE) == false)
        {
            SettingPauseButtons_DisableRetry();
        }

        if (Global.stageIndex == 0)
            stageText.gameObject.SetActive(false);
        
        objEndContentsInfo.SetActive(Global.GameType == GameType.END_CONTENTS);
        if (Global.GameType == GameType.END_CONTENTS && ManagerEndContentsEvent.instance != null)
            itemEndContentsBuff.Init(ManagerEndContentsEvent.instance.Buff);

        InitNoyBoostingEvent();
    }
    
    private void InitNoyBoostingEvent()
    {
        if (ManagerNoyBoostEvent.instance != null && ManagerNoyBoostEvent.instance.NoyBoostPackIngame != null && GameManager.instance.IsNoyBoostStage())
        {
            foreach (var spr in noySpriteList)
                spr.atlas = ManagerNoyBoostEvent.instance.NoyBoostPackIngame.IngameAtlas;
            noyBoostingGauge.gameObject.SetActive(true);
            noyBoostingGauge.InitBoostingGauge();
        }
        else
        {
            noyBoostingGauge.gameObject.SetActive(false);
        }
    }

    void SettingPauseButtons_DisableRetry()
    {
        mainSprite.height = 480;
        title[0].transform.localPosition = new Vector3(-2f, 177f, 0f);
        btnClose.transform.localPosition = new Vector3(330f, 183f, 0f);
        btnContinue.transform.localPosition = new Vector3(0f, 32f, 0f);
        btnMap.transform.localPosition = new Vector3(0f, -128f, 0f);
        stageText.transform.localPosition = new Vector3(0f, 275f, 0f);

        spriteContinue.height = 130;
        spriteMap.height = 130;

        btnRetry.gameObject.SetActive(false);
    }


    void OnClickBtnMenu()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        if (Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_cs_ip_1"), true);
            popup.FunctionSetting(1, "FailProcess", this.gameObject);
            popup.SetButtonText(1, Global._instance.GetString("btn_1"));
            popup.SetButtonText(2, Global._instance.GetString("btn_2"));
            bCanTouch = true;
        }
        else
        {
            FailProcess();
        }
    }

    private void FailProcess()
    {
        ManagerSound._instance.StopBGM();
        
        // Growthy 그로씨
        {
            // 사용한 레디 아이템
            var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
            if (Global.GameInstance.GetProp(GameTypeProp.CAN_USE_READYITEM))
            {
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
            // 턴 추가 광고
            if (GameManager.instance.addTurnCount_ByAD > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.AD_ITEM_LOBBY.ToString(),
                    L_IID = "AD_ADD_TURN",
                    L_CNT = 1
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
            
            // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
            var rankMode = "N";
            if (Global.GameInstance.GetProp(GameTypeProp.NEED_PROFILE_AGREEMENT))
            {
                rankMode = Global.GameInstance.GetProfileAgreementString();
            }

            // 노이 부스팅 단계
            var boostingLevel = 0;
            if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage())
            {
                boostingLevel = GameManager.instance.boostingStep;
            }

            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
            (
                myProfile.userID,
                (myProfile.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                Global.GameInstance.GetGrowthyStageIndex(),
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.DRAW,
                0,
                tempScore,
                0,
                (long)(Time.time - GameManager.instance.playTime),
                GameManager.instance.firstPlay,
                GameManager.instance.useContinueCount > 0,
                GameManager.instance.leftMoveCount, //남은턴 다시계산
                docItem,
                0,
                false,
                false,
                null,
                Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
                null,
                rankMode,
                boostLevel: boostingLevel.ToString(),
                firstFlowerLevel: "N",
                usedTurn: GameManager.instance.useMoveCount,
                continueReconfirm: GameManager.instance.continueReconfirmCount,
                detailInfo: "[0]"
            );

            var doc = JsonConvert.SerializeObject(playEnd);
            doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(null, true));
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);
        }

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템 
        var usedItems = new List<int>();
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_USE_READYITEM))
        {
            usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        }
        else
        {
            usedItems.Add(0);
            usedItems.Add(0);
            usedItems.Add(0);
            usedItems.Add(0);
            usedItems.Add(0);
            usedItems.Add(0);
        }

        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameItemManager.useCount[7]);
        
        usedItems.Add(0); //11
        usedItems.Add(0); //12
        usedItems.Add(0); //13
        usedItems.Add(0); //14
        usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
        usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

        #region 광고 데이터 추가
        var listADItems = new List<int>();
        var itemCount   = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (var i = 0; i < itemCount; i++)
            listADItems.Add(0);

        if (GameManager.instance.addTurnCount_ByAD > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        #endregion

        var remainTarget = ManagerBlock.instance.GetListRemainTarget();
        var tempGameMode = (int)Global.GameType;

        var arg = new GameFailReq()
        {
            type = tempGameMode,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),

            gameCoin = 0,
            gameScore = 0,
            Remains = remainTarget,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = GameManager.instance.LevelAdjusted > 0 ? 1 : 0,
            items = usedItems,
            adItems = listADItems,
        };

        ServerAPI.GameFail(arg, recvGameFail);
        Global.SetIsClear(false);

        FailCountManager._instance.SetFail(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex);
    }

    void recvGameFail(GameFailResp resp)
    {
        //Debug.Log("GameFail: " + resp);
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            SceneLoading.MakeSceneLoading("Lobby");
        }
    }
    
    void recvGameFail_EndContents(EndContentsResp resp)
    {
        //Debug.Log("GameFail: " + resp);
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            SceneLoading.MakeSceneLoading("Lobby");
        }
    }


    private void OnClickBtnRestart()
    {
        if (bCanTouch == false)
        {
            return;
        }

        //블럭 움직임이 멈추지 않았거나 째깍폭탄이 터지려고 한다면 재시작 버튼 눌러지지 않도록 처리.
        if (ManagerBlock.instance.stageInfo.waitState == 1 &&
            (ManagerBlock.instance.state != BlockManagrState.WAIT || GameManager.instance.IsCanTouch == false))
        {
            return;
        }

        bCanTouch = false;

        ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);

        this.preservedRestart = new PrevGameReservedRestartData();

        // Growthy 그로씨        
        {
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
            // 턴 추가 광고
            if (GameManager.instance.addTurnCount_ByAD > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.AD_ITEM_LOBBY.ToString(),
                    L_IID = "AD_ADD_TURN",
                    L_CNT = 1
                };
                itemList.Add(readyItem);
            }
            var docItem = JsonConvert.SerializeObject(itemList);

            // 획득한 스코어
            var tempScore  = ManagerBlock.instance.score;
            var eventRatio = Global.GameInstance.GetBonusRatio(); // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
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

            // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
            var rankMode = "N";
            if (Global.GameInstance.GetProp(GameTypeProp.NEED_PROFILE_AGREEMENT))
            {
                rankMode = Global.GameInstance.GetProfileAgreementString();
            }

            // 노이 부스팅 단계
            var boostingLevel = 0;
            if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage())
            {
                boostingLevel = GameManager.instance.boostingStep;
            }

            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
            (
                myProfile.userID,
                (myProfile.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                Global.GameInstance.GetGrowthyStageIndex(),
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.DRAW,
                0,
                tempScore,
                0,
                (long)(Time.time - GameManager.instance.playTime),
                GameManager.instance.firstPlay,
                GameManager.instance.useContinueCount > 0,
                GameManager.instance.leftMoveCount, //남은턴 다시계산
                docItem,
                0,
                false,
                false,
                null,
                Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
                null,
                rankMode,
                boostLevel: boostingLevel.ToString(),
                firstFlowerLevel: "N",
                usedTurn: GameManager.instance.useMoveCount,
                continueReconfirm: GameManager.instance.continueReconfirmCount,
                detailInfo: "[0]"
            );

            var doc = JsonConvert.SerializeObject(playEnd);
            doc                         = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(null, true));
            preservedRestart.growthyLog = doc;
        }

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템 
        var usedItems = new List<int>();
        usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameItemManager.useCount[7]);
        usedItems.Add(0); //11
        usedItems.Add(0); //12
        usedItems.Add(0); //13
        usedItems.Add(0); //14
        usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
        usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

        #region 광고 데이터 추가
        var listADItems = new List<int>();
        int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (int i = 0; i < itemCount; i++)
            listADItems.Add(0);

        if (GameManager.instance.addTurnCount_ByAD > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        #endregion

        var remainTarget = ManagerBlock.instance.GetListRemainTarget();
        var arg = new GameFailReq()
        {
            type = (int)Global.GameType,
            stage = GameManager.instance.CurrentStage,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            gameCoin = 0,
            gameScore = 0,
            Remains = remainTarget,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = GameManager.instance.LevelAdjusted > 0 ? 1 : 0,
            items = usedItems,
            adItems = listADItems,
        };

        this.preservedRestart.stageIdx = GameManager.instance.CurrentStage;
        this.preservedRestart.failReq = arg;
        bool isSingleRoundEvent = Global.isSingleRoundEvent;

        //이벤트 정보 갱신
        if (ManagerStageChallenge.instance != null)
            ManagerStageChallenge.instance.SyncFromServerContentsData();

        //다음 스테이지 진입 취소할 경우, isSingleRoundEvent를 이전 상태로 변경해줌.
        Global.SetStageIndex(GameManager.instance.CurrentStage);
        ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch, CoShowTopUIFalse,
            () => { Global.isSingleRoundEvent = isSingleRoundEvent; });
    }
    
    void OnTouch()
    {
        if (_instance != null && this.gameObject != null)
            this.bCanTouch = true;
    }

    void CoShowTopUIFalse()
    {
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eTopUI);
    }

    IEnumerator CoRestartGame()
    {
        string stageKey = Global.GameInstance.GetStageKey();
        string stageName = Global.GameInstance.GetStageFilename();

        using(var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

                ManagerUI._instance.OpenPopupReady(tempData);

                yield return null;
            }
        }
    }

    public void ClosePopUp()
    {
        OnClickBtnClose();
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.ClosePopUpUI();
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        if (GameManager.instance.state == GameState.PLAY)
            ManagerSound._instance.PlayBGM();
        base.ClosePopUp(_mainTime, callback);
    }
}
