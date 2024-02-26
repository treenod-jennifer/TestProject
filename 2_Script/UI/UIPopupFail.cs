using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopupFail : UIPopupBase
{
    public static UIPopupFail _instance = null;

    public UILabel _labelStage;
    public UILabel failText;
    public GameObject _trTarget;
    public GameObject[] failButton;

    [SerializeField]
    UIIngameBoostingGauge noyBoostingGauge;
    [SerializeField]
    List<UISprite> noySpriteList;

    //리뷰 관련.
    private UIIngameResultChat resultChat;

    //표시할 버튼 상태
    private ProceedPlayType proceedPlayType = ProceedPlayType.NONE;

    List <StageTarget> stageTargets = new List<StageTarget>();

    #region 광고 관련
    //턴 추가 광고 관련
    [SerializeField] private GameObject objADRoot_AddTurn;
    [SerializeField] private GameObject[] arrADIconRoot_AddTurn;
    [SerializeField] private UILabel labelADTurnCount_AddTurn;
    #endregion

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

        if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage())
        {
            foreach (var spr in noySpriteList)
                spr.atlas = null;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        if (UIPopupContinue._instance != null && UIPopupContinue._instance.boniLive2D != null)
        {
            UIPopupContinue._instance.boniLive2D.SetSortOrder(layer + 1);
            UIPopupContinue._instance.SettingReviewChatSortOrder(layer + 1);
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(Protocol.GameFailResp resp)
    {
        proceedPlayType = (resp == null) ? ProceedPlayType.NONE : (ProceedPlayType)resp.ProceedPlayType;

        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);

        _labelStage.text = Global.GameInstance.GetStageText_IngamePopup();
        _labelStage.MakePixelPerfect();

        //실패 텍스트 띄우기
        failText.text = Global.GameInstance.GetText_StageFail(proceedPlayType);

        //재시작 버튼 설정
        if (ManagerTurnRelay.turnRelayIngame != null && ManagerTurnRelay.turnRelayIngame.IsTurnRelayRestart())
        {
            InitBtnTurnRelayRestart();
        }
        else
        {
            if (proceedPlayType != ProceedPlayType.RETRY)
            {
                SettingFailButton();
            }
            else
            {
                InitBtnRetry_AD();
            }
        }
        
        SetTarget();  //목표 설정.
        InitNoyBoostingEvent();

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

            // 남은 목표
            var remainTarget   = ManagerBlock.instance.GetListRemainTarget();
            var docRemainGoals = JsonConvert.SerializeObject(remainTarget);
            
            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
            (
                myProfile.userID,
                (myProfile.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                Global.GameInstance.GetGrowthyStageIndex(),
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.LOSE,
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
                docRemainGoals,
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

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL2);
        ManagerSound._instance.PauseBGM();
        CheckAndMakeReviewChat();
    }
    
    private void InitNoyBoostingEvent()
    {
        if (ManagerNoyBoostEvent.instance != null && ManagerNoyBoostEvent.instance.NoyBoostPackIngame != null && GameManager.instance.IsNoyBoostStage())
        {
            foreach (var spr in noySpriteList)
                spr.atlas = ManagerNoyBoostEvent.instance.NoyBoostPackIngame.IngameAtlas;
            noyBoostingGauge.gameObject.SetActive(true);
            noyBoostingGauge.InitBoostingGauge();
            noyBoostingGauge.DecreaseBoostingAction();
        }
        else
        {
            noyBoostingGauge.gameObject.SetActive(false);
        }
    }

    private void InitBtnTurnRelayRestart()
    {
        failButton[1].SetActive(false);
        failButton[2].SetActive(true);
        
        // 광고 버튼 노출 그로시
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
            tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
            cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_SHOW,
            anm: $"TYPE_{(int)AdManager.AdType.AD_22}_SHOW",
            arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            str1: ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString()
        );
        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }

    private void InitBtnRetry_AD()
    {
        //턴추가 관련 광고 설정
        StageInfo tempInfo = ManagerBlock.instance.stageInfo;
        bool isCanWatchAD_AddTurn = Global.GameInstance.IsCanWatch_AD_AddTurn(tempInfo.isHardStage == 0, tempInfo.gameMode);
        objADRoot_AddTurn.SetActive(isCanWatchAD_AddTurn);
        if (isCanWatchAD_AddTurn == true)
        {
            StartCoroutine(CoBubbleAction_ADAddTurn());   
            //턴 카운트 텍스트 설정
            labelADTurnCount_AddTurn.text = Global.GameInstance.TurnCountByReadyAD().ToString();
        }
    }

    private IEnumerator CoBubbleAction_ADAddTurn()
    {
        int index = 1;
        while (true)
        {
            arrADIconRoot_AddTurn[index].SetActive(false);
            index = (index == 0) ? 1 : 0;
            arrADIconRoot_AddTurn[index].SetActive(true);
            yield return new WaitForSeconds(1f);
        }
    }

    public void SetTarget()
    {
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;
            if (enumerator.Current.Value != null)
            {
                string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                    string.Format("StageTarget_{0}", targetType) : "StageTarget";

                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;

                    StageTarget target = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIstageMission", _trTarget).GetComponent<StageTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    int remainCount = e.Current.Value.collectCount - e.Current.Value.pangCount;
                    if (remainCount <= 0)
                    {
                        target.targetCount.gameObject.SetActive(false);
                        target.targetCountShadow.gameObject.SetActive(false);
                        target.checkSprite.gameObject.SetActive(true);
                        target.checkSprite.depth = 5;
                    }
                    else
                    {
                        string remainCountText = remainCount.ToString();
                        target.targetCount.text = remainCountText;
                        target.targetCountShadow.text = remainCountText;
                    }

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;
                    ManagerUIAtlas.CheckAndApplyEventAtlas(target.targetSprite);
                    target.targetSprite.MakePixelPerfect();

                    stageTargets.Add(target);
                }
            }
        }

        float startPos = (1 - stageTargets.Count) * 48;
        for (int i = 0; i < stageTargets.Count; i++)
        {
            stageTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, -45, 0);
        }    
    }

    #region 확인 버튼 눌렀을 때 동작
    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        switch (proceedPlayType)
        {
            case ProceedPlayType.REBOOT:
                Reboot();
                break;
            default:
                GoToLobby();
                return;
        }
    }

    private void Reboot()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_65"), false);
        popupSystem._callbackEnd += () => Global.ReBoot();
    }

    private void GoToLobby()
    {
        resultChatColliderSetting(false);
        ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("Lobby");
        UIPopupStageAdventure.startChapter = Global.chapterIndex;
    }
    #endregion

    public void SettingFailButton()
    {
        failButton[0].transform.localPosition = new Vector3(-10f, failButton[0].transform.localPosition.y, 0f);
        failButton[1].SetActive(false);
    }
    
    private void CheckAndMakeReviewChat()
    {
        if (UIPopupContinue._instance != null)
            return;
        if (resultChat == null)
        {
            resultChat = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIIngameResultChat", gameObject).GetComponent<UIIngameResultChat>();
        }
        resultChat.transform.localPosition = new Vector3(0f, 80f, 0f);

        //리뷰 데이터 5개 가져 온 뒤, 미리보기 호출.
        InitResultChat();
    }

    private void InitResultChat()
    {
        resultChatColliderSetting(false);
        if (Global.GameType == GameType.NORMAL && GameManager.instance.CurrentStage >= 2 && Global.SkipStageComment() == false)
        {
            resultChat.SettingChatData(uiPanel.depth + 1, false);
        }
    }

    private void resultChatColliderSetting(bool bColl)
    {
        if (UIPopupContinue._instance != null)
        {
            UIPopupContinue._instance.resultChatColliderSetting(bColl);
        }
        else
        {
            if (resultChat != null)
                resultChat.SetCollider(bColl);
        }
    }

    void OnClickBtnRetry()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        resultChatColliderSetting(false);
        
        bool isSingleRoundEvent = Global.isSingleRoundEvent;
        //레디창.
        //다음 스테이지 진입 취소할 경우, isSingleRoundEvent를 이전 상태로 변경해줌.
        ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch, callBackCancel: ()=>
        {
            Global.isSingleRoundEvent = isSingleRoundEvent;
        });
    }
    
    private void OnClickBtnSaveWave()
    {
        if (bCanTouch == false)
        {
            return;
        }

        bCanTouch = false;
        resultChatColliderSetting(false);
        
        AdManager.ShowAD_ReqTurnRelaySaveWave(AdManager.AdType.AD_22, (isSuccess) =>
        {
            if (isSuccess)
            {
                ServerAPI.TurnRelaySaveWave((resp) =>
                {
                    bCanTouch = true;
                    if (resp.IsSuccess)
                    {
                        //퀘스트 데이터 갱신
                        QuestGameData.SetUserData();
                        
                        //스테이지 정보 로드 후, 게임 시작 처리
                        ServerRepos.GameStartTs = Global.GetTime();
                        ManagerTurnRelay.turnRelayIngame.SaveWaveCount += 1;
                        StartCoroutine(ManagerUI._instance.CoLoadStageData_TurnRelayEvent(() => RecvGameRestart(resp)));
                    }
                    else
                    {
                        StartCoroutine(OpenSystemPopup());
                    }
                });
            }
            else
            {
                bCanTouch = true;
            }
        });

        IEnumerator OpenSystemPopup()
        {
            var popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_64"), false);
            yield return new WaitUntil(() => UIPopupSystem._instance == null);
            OnClickBtnClose();
        }
    }
    
    private void RecvGameRestart(Protocol.BaseResp code)
    {
        if (code.IsSuccess)
        {
            //레디 아이템 모두 해제
            var readyItemCount = Global._instance.GetReadyItemCount();
            for (var i = 0; i < readyItemCount; i++)
            {
                UIPopupReady.readyItemUseCount[i] = new EncValue();
                UIPopupReady.readyItemUseCount[i].Value = 0;
            }
            
            for (var i = 0; i < 6; i++)
            {
                UIPopupReady.readyItemAutoUse[i] = new EncValue();
                UIPopupReady.readyItemAutoUse[i].Value = 0;
            }

            //턴 릴레이 웨이브 데이터 초기화
            ManagerTurnRelay.turnRelayIngame.InitIngameUserData(true);

            //인게임 진입 처리
            ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
            ManagerSound._instance.StopBGM();
            SceneLoading.MakeSceneLoading("InGame");
            ManagerUI._instance.bTouchTopUI = true;
        }
    }

    void OnTouch()
    {
        this.bCanTouch = true;
        if (Global.GameType == GameType.NORMAL && GameManager.instance.CurrentStage >= 2 && Global.SkipStageComment() == false)
            resultChatColliderSetting(true);
    }
}
