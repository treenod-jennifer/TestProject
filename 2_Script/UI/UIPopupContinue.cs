using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Protocol;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopupContinue : UIPopupBase
{
    public static UIPopupContinue _instance = null;
    public UILabel stage;
    public UISprite costSprite;
    public List<UILabel> cost = new List<UILabel>();
    public List<UILabel> apple = new List<UILabel>();
    public UILabel info;
    public UIUrlTexture infoImage;
    public GenericReward infoGenericReward;
    public GameObject saleIcon;
    public GameObject live2DAnchor;
    public UISprite turnImage;

    //추가보너스
    public GameObject bonusRoot;
    public UISprite[] bonusSprite;
    public UILabel bonusSpriteCount;

    public bool inFalseClickDelay = true;
    public bool pushContinue = false;

    //이벤트 용.
    public UITexture eventTextBox;
    public UILabel eventText;

    [HideInInspector]
    public LAppModelProxy boniLive2D;

    //랭킹
    public GameObject rankRoot;
    public UILabel bestScoreLabel;
    public UILabel ScoreLabel;
    public GameObject BestRoot;

    //블럭모으기
    [SerializeField]    GameObject specialRoot;
    [SerializeField]    UIUrlTexture specialEventObj;
    [SerializeField]    UILabel specialEventLabel;
    [SerializeField]    UILabel specialEventLabelShadow;
    [SerializeField]    UILabel specialEventRatioLabel;
    [SerializeField]    UILabel specialEventRatioLabelShadow;

    //월드랭킹
    [SerializeField]    GameObject worldRankRoot;
    [SerializeField]    UISprite worldRankObj;
    [SerializeField]    UILabel worldRankLabel;
    [SerializeField]    UILabel worldRankLabelShadow;
    [SerializeField]    UILabel worldRankRatioLabel;
    [SerializeField]    UILabel worldRankRatioLabelShadow;

    //엔드컨텐츠
    [SerializeField]    GameObject endContentsRoot;
    [SerializeField]    UISprite endContentsObj;
    [SerializeField]    UILabel endContentsLabel;
    [SerializeField]    UILabel endContentsLabelShadow;
    [SerializeField]    UILabel endContentsRatioLabel;
    [SerializeField]    UILabel endContentsRatioLabelShadow;
    [SerializeField]    UIItemEndContentsBoost endContentsBoostItem;

    //알파벳 이벤트 관련
    [SerializeField]    GameObject alphabetRoot;
    [SerializeField]    List<UISprite> listSpriteAlphabet_N;
    [SerializeField]    UIUrlTexture textureAlphabet_S;
    
    //턴 릴레이 이벤트
    [SerializeField] GameObject turnRelayRoot;
    [SerializeField] GameObject turnRelayContinueItemObj;
    [SerializeField] UILabel[] turnRelayGetCountLabel;
    [SerializeField] UILabel[] turnRelayBonusCountLabel;
    [SerializeField] List<UIItemTurnRelay_ContinueItem> listTurnRelayContinueItem;

    //노이 부스팅 이벤트 관련
    [SerializeField]
    UIIngameBoostingGauge noyBoostingGauge;
    [SerializeField]
    List<UISprite> noySpriteList;
    
    // 우주여행 이벤트
    [SerializeField] private GameObject                         _spaceTravelRoot;
    [SerializeField] private List<UIItemSpaceTravelContinueItem> _listSpaceTravelContinueItem;

    //통상스테이지 독려 이벤트 관련
    [SerializeField] private UIItemStageAssistMission stageAssistMission;

    //월드랭킹
    public GameObject powerSaleObj;
    public UILabel powerSaleLabel;

    //리뷰 관련.
    private UIIngameResultChat resultChat;
    
    //상품판매 법률 개정 관련.
    [SerializeField] private GameObject objDescription;

    //컨티뉴 효과 관련.
    private List<GameFailType> listContinueType = new List<GameFailType>();

    //재확인 기능 관련.
    [Header("ReconfirmLink")]
    [SerializeField] private GameObject objActiveItemBubble;
    [SerializeField] private UIPokoButton[] btnClose;
    [SerializeField] private List<GameObject> objActiveItem;
    [SerializeField] private UITable tableActiveItem;
    
    //NPU 광고 컨티뉴 버튼.
    [SerializeField] private GameObject btnADObject;
    [SerializeField] private GameObject btnNoObject;
    private bool btnADOn = false;

    //코인 스테이지 관련
    [SerializeField] private GameObject coinStageRoot;
    [SerializeField] private UILabel labelCoinStageContinueCoin;
    [SerializeField] private List<UILabel> labelContinueRewardTime;
    [SerializeField] private UIPokoButton btnYesCoinStage;
    
    
    public static bool IsContinuReconfirm = false;

    private int clearLayer = 1;

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

    private void Start()
    {
        // 출력 1순위 : 스팟다이아
        if (SpotDiaShowCheck().Item1)
        {
            // 데이터 세팅 및 팝업 오픈
            var spotData = SpotDiaShowCheck().Item2;
            CdnShopPackage pkgData = null;
            ServerContents.Packages.TryGetValue(spotData.packageIdx, out pkgData);
            if (pkgData != null)
            {
                ManagerUI._instance.AsyncOpenPopupSpotDiaPackage_Reconfirm(pkgData, spotData).Forget();
                ServerRepos.UserSpotDiaShow.isShowed = 1;
                FailCountManager._instance.ClearData();
            }
            
            // SPOT_DIA_SUGGEST 그로시 전송
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPOT_DIA_SUGGEST,
                $"SPOT_DIA_{spotData.segMin}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            achieve.L_STR1 = $"{Global.GameInstance.GetGrowthyGameMode()}_{Global.eventIndex}_{Global.chapterIndex}_{Global.stageIndex}";
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            
            // 스팟 다이아 출력 API 전송
            ServerAPI.RenewalSpotDiaShow();
            StartCoroutine(ProceedFalseClickDelay());
            return;
        }

        // 출력 2순위 : NPU 광고
        if (FailCountManager._instance.FailCheck(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex, FailCountManager.FailCountType.NPU_AD_CONTINUE))
        {
            if (FailCountManager.CheckNpuAdContinue())
            {
                btnADOn = true;
                SetContinueADUI();
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.NPU_CONTINUE_AD,
                    "NPU_CONTINUE_AD_OPEN",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    ServerContents.NpuAdContinueInfo.ver,
                    Global.stageIndex.ToString()
                );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                StartCoroutine(ProceedFalseClickDelay());
                return;
            }
        }

        // 출력 3순위 : NPU 스팟 패키지
        if (FailCountManager._instance.FailCheck(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex, FailCountManager.FailCountType.SPOT_NPU_PACKAGE))
        {
            var checkResult = FailCountManager.CheckNpuSpotPackage();
            if ( checkResult.Item1 && checkResult.Item2 != null )
            {
                StartCoroutine(ProceedFalseClickDelay());
                return;
            }
        }

        StartCoroutine(ProceedFalseClickDelay());

        (bool, CdnSpotDiaPackage) SpotDiaShowCheck()
        {
            // 게임 모드 체크
            if (Global.GameType != GameType.NORMAL && Global.GameType != GameType.EVENT && Global.GameType != GameType.MOLE_CATCH &&
                Global.GameType != GameType.BINGO_EVENT  && Global.GameType != GameType.TREASURE_HUNT &&
                Global.GameType != GameType.SPACE_TRAVEL && Global.GameType != GameType.ATELIER)
            {
                return (false, null);
            }
            
            // 게임 시작시 동기화되는 스팟 관련 데이터 기본 체크
            if (ServerRepos.UserSpotDiaShow == null ||
                ServerRepos.UserSpotDiaShow.failTargetCount == 0 ||
                ServerRepos.UserSpotDiaShow.isShowed == 1)
            {
                return (false, null);
            }

            foreach (var p in ServerContents.SpotDiaPackage)
            {
                // 패키지 상태 기본 체크
                CdnShopPackage pkgData;
                ServerContents.Packages.TryGetValue(p.Value.packageIdx, out pkgData);
                if (pkgData == null || pkgData.type != 2 || (pkgData.expireTs != 0 && Global.LeftTime(pkgData.expireTs) <= 0))
                {
                    continue;
                }
                
                // 구매하지 않은 상태라면 카운트 체크 후 패키지 리턴
                if (!ServerRepos.UserShopPackages.Exists(x => x.idx == pkgData.idx))
                {
                    var failCount       = ServerRepos.UserSpotDiaShow.failCount + 1;
                    var failTargetCount = ServerRepos.UserSpotDiaShow.failTargetCount;
                    return (failCount >= failTargetCount, p.Value);
                }
            }
            return (false, null);
        }
    }

    // Start와 OpenPopup 함수 간 딜레이가 있을 수 있어 중복으로 UI 세팅 실행
    private void SetContinueADUI()
    {
        btnADObject.SetActive(true);
        btnNoObject.SetActive(false);
        info.text = Global._instance.GetString("p_ctn_4");
    }

    IEnumerator ProceedFalseClickDelay()
    {
        this.inFalseClickDelay = true;
        //var yesBtn = gameObject.transform.Find("ContinueUI").Find("btnYes");
        //var yesSprite = yesBtn.Find("Sprite_Yes").GetComponent<UISprite>();
        //yesSprite.spriteName = "button_play03";

        yield return new WaitForSeconds(0.5f);

        //yesSprite.spriteName = "button_play";
        this.inFalseClickDelay = false;
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        ManagerSound._instance.PauseBGM();
            
        BlockMatchManager.instance.DistroyAllLinker();
        SetCharacter();
        SetInGameMode();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        clearLayer = layer;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            clearLayer += 1;
        }

        if (LanguageUtility.IsShowBuyInfo)
        {
            objDescription.SetActive(true);
            objDescription.GetComponent<UIPanel>().sortingOrder = clearLayer + 1;
        }
        else
        {
            MakeResultChat();   
            objDescription.SetActive(false);
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public override void OnClickBtnBack()
    {
        if (bCanTouch == false)
        {
            return;
        }

        // 첫 컨티뉴 팝업 시점 광고지면이 출력된 상태에서 광고지면을 확인하지 않고 X버튼을 누르면 NPU 스팟성 패키지 출력
        if (btnADOn)
        {
            if (FailCountManager._instance.FailCheck(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex, FailCountManager.FailCountType.SPOT_NPU_PACKAGE))
            {
                var checkResult = FailCountManager.CheckNpuSpotPackage();
                if ( checkResult.Item1 && checkResult.Item2 != null )
                {
                    StartCoroutine(ProceedFalseClickDelay());
                    return;
                }
            }
        }
        OnClickBtnNo();
    }

    public void InitPopUp()
    {
        //게임 타입에 따라 컨티뉴 팝업 설정
        Global.GameInstance.OnPopupInit_Continue(this);

        //게임 타입에 따라 컨티뉴 가격 알아오기
        string costText = Global.GameInstance.GetContinueCost().ToString();
        foreach (var item in cost)
        {
            item.text = costText;
        }
        
        // 게임 타입에 따라 컨티뉴 재화 이미지 변경
        if (Global.GameInstance.GetItemCostType(ItemType.CONTINUE) == RewardType.coin)
        {
            costSprite.spriteName = "icon_coin_shadow";
            cost[1].transform.localPosition = new Vector2(-1f, -15.5f);
        }
        else
        {
            costSprite.spriteName = "icon_diamond_shadow";
            cost[1].transform.localPosition = new Vector2(17f, -8f);
        }

        //게임 타입에 따라 컨티뉴 세일 아이콘 설정해줌.
        saleIcon.SetActive(Global.GameInstance.isContinueSale());
        
        //노이 부스팅 이벤트
        InitNoyBoostingEvent();

        //알파벳 이벤트
        InitAlphabetEvent();

        //통상 스테이지 독려 이벤트
        InitStageAssistMissionEvent();

        //수문장 크루그에서만 컨티뉴 재확인 팝업 반복해서 띄움
        if(IsContinuReconfirm && Global.isSingleRoundEvent == false)
        {
            for (int i = 0; i < btnClose.Length; i++)
            {
                btnClose[i].functionName = "OnClickBtnBack";
            }
        }
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

    private void InitAlphabetEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == false
            || ManagerAlphabetEvent.instance == null
            || ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == false)
        {
            alphabetRoot.gameObject.SetActive(false);
            return;
        }

        alphabetRoot.gameObject.SetActive(true);
        Vector3 pos = Vector3.zero;

        int groupAlphabetCount = (ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N == null) ?
            0 : ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N.Count;

        if (groupAlphabetCount > 8)
            alphabetRoot.transform.localScale = Vector3.one * 0.85f;
        else
            alphabetRoot.transform.localScale = Vector3.one * 0.95f;

        //일반 알파벳 블럭 출력
        int prevWidth = 0;
        for (int i = 0; i < listSpriteAlphabet_N.Count; i++)
        {
            if (i >= groupAlphabetCount)
            {
                listSpriteAlphabet_N[i].gameObject.SetActive(false);
            }
            else
            {
                ManagerAlphabetEvent.AlphabetIngame.AlphabetData data = ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N[i];

                listSpriteAlphabet_N[i].gameObject.SetActive(true);

                //알파벳 획득 상태에 따라 이미지 설정.
                bool isGainAlphabet = data.getCount_All > 0;
                listSpriteAlphabet_N[i].spriteName = ManagerAlphabetEvent.instance.GetAlphabetSpriteName_N(data.index, isGainAlphabet);
                listSpriteAlphabet_N[i].MakePixelPerfect();

                //현재 스테이지에서 해당 블럭을 획득한 상태라면, 알파벳 깜빡이는 연출
                if (data.getCount_All > 0 && data.getCount_All == data.getCount_Current)
                {
                    UISprite uiSprite = listSpriteAlphabet_N[i];
                    Sequence mySequence = DOTween.Sequence();
                    mySequence.PrependInterval(0.3f);
                    mySequence.Append(DOTween.ToAlpha(() => uiSprite.color, x => uiSprite.color = x, 0.3f, 0.3f));
                    mySequence.Append(DOTween.ToAlpha(() => uiSprite.color, x => uiSprite.color = x, 1f, 0.3f));
                    mySequence.SetLoops(-1);
                }

                //알파벳 이미지 사이즈에 따라 위치 설정
                int currentWidth = listSpriteAlphabet_N[i].GetAtlasSprite().width;
                if (prevWidth > 0)
                {
                    float offset = GetAlphabetPosOffset(prevWidth, currentWidth);
                    if (currentWidth > prevWidth)
                        pos.x += offset;
                    else
                        pos.x -= offset;
                }

                listSpriteAlphabet_N[i].transform.localPosition = pos;
                pos.x += (currentWidth + 5f);
                prevWidth = currentWidth;
            }
        }

        //스페셜 알파벳 블럭 출력
        if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == true 
            && ManagerAlphabetEvent.alphabetIngame.IsGainNewAlphabet_S() == true)
        {
            string textureName = ManagerAlphabetEvent.alphabetIngame.GetAppearAlphabetSpriteName_S();

            //알파벳 깜빡이는 연출
            Sequence mySequence = DOTween.Sequence();
            mySequence.PrependInterval(0.3f);
            mySequence.Append(DOTween.ToAlpha(() => textureAlphabet_S.color, x => textureAlphabet_S.color = x, 0.3f, 0.3f));
            mySequence.Append(DOTween.ToAlpha(() => textureAlphabet_S.color, x => textureAlphabet_S.color = x, 1f, 0.3f));

            textureAlphabet_S.SuccessEvent += () =>
            {
                mySequence.SetLoops(-1);
            };
            textureAlphabet_S.LoadCDN(Global.gameImageDirectory, "IconEvent/", textureName);
            pos.x += 7f;
            textureAlphabet_S.transform.localPosition = pos;
        }
    }

    private void InitStageAssistMissionEvent()
    {
        if (ManagerStageAssistMissionEvent.CheckStartable_InGame())
        {
            stageAssistMission.gameObject.SetActive(true);
            stageAssistMission.SetStageAssist(true);
        }
        else
        {
            stageAssistMission.gameObject.SetActive(false);
        }
    }
    
    

    private float GetAlphabetPosOffset(int prevWidth, int currentWidth)
    {
        return Mathf.Abs((prevWidth * 0.5f) - (currentWidth * 0.5f));
    }

    public void SettingReviewChatSortOrder(int layer)
    {
        if (resultChat == null)
            return;
        resultChat.uiPanel.sortingOrder = layer + 1;
        resultChat.scrollView.panel.sortingOrder = layer + 2;
    }

    public void SettingBoniDialogBubbleWorldRank()
    {
        if (ManagerWorldRanking.instance != null)
        {
            NGUIAtlas worldRankingAtlas = ManagerWorldRanking.resourceData.worldRankingPack.IngameAtlas;
            worldRankObj.atlas = worldRankingAtlas;
        }

        float eventRatio = Global.GameInstance.GetBonusRatio();   // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
        //플라워 카운트 계산
        float scoreRatio = 1;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
            scoreRatio = 1.2f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;

        int tempScore = (int)(ManagerBlock.instance.score * scoreRatio);

        if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_RED) && (tempScore >= (int)(ManagerBlock.instance.stageInfo.score4 * 1.1f)))
        {
            ManagerBlock.instance.flowrCount = 5;
        }
        else if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_BLUE) && (tempScore >= ManagerBlock.instance.stageInfo.score4))
        {
            ManagerBlock.instance.flowrCount = 4;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score3)
        {
            ManagerBlock.instance.flowrCount = 3;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score2)
        {
            ManagerBlock.instance.flowrCount = 2;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score1)
        {
            ManagerBlock.instance.flowrCount = 1;
        }

        int getCount = ManagerBlock.instance.worldRankingItemCount * ManagerBlock.instance.flowrCount;

        if (getCount > 0)
        {
            if (getCount <= 330)
            {
                SetEventTextBox("p_ctn_13");
            }
            else if (getCount <= 660)
            {
                SetEventTextBox("p_ctn_14");
            }
            else if (getCount > 660)
            {
                SetEventTextBox("p_ctn_15");
            }  
        }

        worldRankRoot.SetActive(true);
        worldRankObj.spriteName = "worldCollectItem";
        worldRankObj.MakePixelPerfect();

        string text = ManagerBlock.instance.worldRankingItemCount.ToString();
        worldRankLabel.text = text;
        worldRankLabelShadow.text = text;

        worldRankRatioLabel.text = "x" + ManagerBlock.instance.flowrCount;
        worldRankRatioLabelShadow.text = "x" + ManagerBlock.instance.flowrCount;
    }
    
    public void SettingBoniDialogBubbleEndContents()
    {
        if (ManagerEndContentsEvent.instance != null)
        {
            NGUIAtlas endContentsAtlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas;
            endContentsObj.atlas = endContentsAtlas;
        }

        float eventRatio = Global.GameInstance.GetBonusRatio();   // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
        //플라워 카운트 계산
        float scoreRatio = 1;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
            scoreRatio = 1.2f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;

        int tempScore = (int)(ManagerBlock.instance.score * scoreRatio);

        if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_RED) && (tempScore >= (int)(ManagerBlock.instance.stageInfo.score4 * 1.1f)))
        {
            ManagerBlock.instance.flowrCount = 5;
        }
        else if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_BLUE) && (tempScore >= ManagerBlock.instance.stageInfo.score4))
        {
            ManagerBlock.instance.flowrCount = 4;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score3)
        {
            ManagerBlock.instance.flowrCount = 3;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score2)
        {
            ManagerBlock.instance.flowrCount = 2;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score1)
        {
            ManagerBlock.instance.flowrCount = 1;
        }

        int getCount = ManagerBlock.instance.endContentsItemCount * ManagerBlock.instance.flowrCount;
        if (tempScore < ManagerBlock.instance.stageInfo.score1)
            getCount = ManagerBlock.instance.endContentsItemCount;

        if (getCount > 0)
        {
            if (getCount <= 330)
            {
                SetEventTextBox("p_ctn_13");
            }
            else if (getCount <= 660)
            {
                SetEventTextBox("p_ctn_14");
            }
            else if (getCount > 660)
            {
                SetEventTextBox("p_ctn_15");
            }  
        }

        endContentsRoot.SetActive(true);
        endContentsObj.spriteName = "endContents2";
        endContentsObj.MakePixelPerfect();

        string text = ManagerBlock.instance.endContentsItemCount.ToString();
        endContentsLabel.text = text;
        endContentsLabelShadow.text = text;

        if (tempScore < ManagerBlock.instance.stageInfo.score1)
        {
            endContentsRatioLabel.text = "x1";
            endContentsRatioLabelShadow.text = "x1";
        }
        else
        {
            endContentsRatioLabel.text = "x" + ( ManagerEndContentsEvent.instance.GetScoreRatio()[ManagerBlock.instance.flowrCount - 1]);
            endContentsRatioLabelShadow.text = "x" + ( ManagerEndContentsEvent.instance.GetScoreRatio()[ManagerBlock.instance.flowrCount - 1]);
        }
        
        endContentsBoostItem.Init(ManagerEndContentsEvent.instance.Buff);
        
        // 포코타 말풍선 생성
        if (Global.GameType == GameType.END_CONTENTS)
        {
            SetEventTextBox("p_ctn_19");
        }
        
        //그룹랭킹 이벤트일 경우
        if (ManagerGroupRanking.IsGroupRankingStage())
        {
            SetEventTextBox("p_ctn_24");
        }
    }

    public void SettingBoniDialogBubbleBingo()
    {
        // 포코타 말풍선 생성
        if (Global.GameType == GameType.BINGO_EVENT)
        {
            SetEventTextBox("p_ctn_20");
        }
    }
    
    public void SettingBoniDialogBubbleTreasureHunt()
    {
        // 포코타 말풍선 생성
        if (Global.GameType == GameType.TREASURE_HUNT)
        {
            SetEventTextBox("p_ctn_21");
        }
    }
    
    public void SettingBoniDialogBubbleSpaceTravel()
    {
        // 포코타 말풍선 생성
        if (Global.GameType == GameType.SPACE_TRAVEL)
        {
            //유저가 보유하고 있는 아이템 표시
            var hasRemainItem = false;
            for (var i = (int)ManagerSpaceTravel.BonusItemType.LINE_BOMB; i <= (int)ManagerSpaceTravel.BonusItemType.RAINBOW_BOMB; i++)
            {
                var type = (ManagerSpaceTravel.BonusItemType)i;
                if (ManagerSpaceTravel.instance.selectItemDic[type] > 0)
                {
                    hasRemainItem = true;
                    break;
                }
            }
            _spaceTravelRoot.SetActive(hasRemainItem);
            if (hasRemainItem)
            {
                for (var i = 0; i < _listSpaceTravelContinueItem.Count; i++)
                {
                    _listSpaceTravelContinueItem[i].InitItem();
                }
            }
            
            SetEventTextBox(Global.stageIndex % 3 == 1 ? "p_ctn_26" : "p_ctn_25");
        }
    }

    public void SettingBoniDialogBubbleTurnRelay()
    {
        //말풍선 텍스트 설정
        SetEventTextBox("p_ctn_16");

        //턴 릴레이 UI 설정
        turnRelayRoot.SetActive(true);

        //유저가 보유하고 있는 아이템 표시
        bool hasRemainItem = false;
        for (int i = (int)ManagerTurnRelay.BONUSITEM_TYPE.LINE_BOMB; i <= (int)ManagerTurnRelay.BONUSITEM_TYPE.RAINBOW_BOMB; i++)
        {
            ManagerTurnRelay.BONUSITEM_TYPE type = (ManagerTurnRelay.BONUSITEM_TYPE)i;
            if (ManagerTurnRelay.turnRelayIngame.GetData_DicIngameItemCount(type) > 0)
            {
                hasRemainItem = true;
                break;
            }
        }

        turnRelayContinueItemObj.gameObject.SetActive(hasRemainItem);
        if (hasRemainItem == true)
        {
            for (int i = 0; i < listTurnRelayContinueItem.Count; i++)
            {
                listTurnRelayContinueItem[i].InitItem();
            }
        }

        //이벤트 포인트 텍스트 표시
        var currentWavePoint = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave();
        string totalPoint = (ManagerTurnRelay.turnRelayIngame.IngameEventPoint + currentWavePoint.Item1).ToString();
        string bonusPoint = (currentWavePoint.Item2 == 0) ? "" : string.Format("(+{0})", currentWavePoint.Item2);
        for (int i = 0; i < turnRelayGetCountLabel.Length; i++)
            turnRelayGetCountLabel[i].text = totalPoint.ToString();
        for (int i = 0; i < turnRelayBonusCountLabel.Length; i++)
            turnRelayBonusCountLabel[i].text = bonusPoint;
    }

    public void SettingCoinStage()
    {
        //기본 보상 이미지 비활성화
        turnImage.gameObject.SetActive(false);
        
        //코인 스테이지 UI 활성화
        coinStageRoot.SetActive(true);
        info.text = Global._instance.GetString("cs_ctn_1");
        labelCoinStageContinueCoin.text = $"{ManagerBlock.instance.coins}";

        for (int i = 0; i < labelContinueRewardTime.Count; i++)
        {
            labelContinueRewardTime[i].text = $"{ManagerCoinBonusStage.instance.GetContinueRewardTime()}{Global._instance.GetString("time_4")}";
        }
        
        // 버튼에 대한 함수 세팅을 코인 스테이지 컨티뉴의 형태로 변경
        SetCoinStageContinueBtn();
    }
    
    public void SettingBoniDialogBubbleNormal()
    {
        //모으기이벤트중일때 //모은갯수가 1개이상일때 
        if (GameUIManager.instance.isCompleteSpecialEventSettings && ManagerBlock.instance.getSpecialEventBlock > 0)
        {
            SetEventTextBox("n_ev_5");

            specialRoot.SetActive(true);
            specialEventObj.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
            specialEventObj.transform.localScale = Vector3.one * 1.5f;
            specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
            specialEventLabelShadow.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();

            specialEventRatioLabel.text = "x" + ManagerBlock.instance.getSpecialEventRatio;
            specialEventRatioLabelShadow.text = "x" + ManagerBlock.instance.getSpecialEventRatio;
        }

        //알파벳 이벤트일 경우, 획득한 아이템이 하나라도 있다면 말풍선 출력
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == true
            && ManagerAlphabetEvent.instance != null
            && ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == true)
        {
            if (ManagerAlphabetEvent.alphabetIngame.GetAlphabetKindsCount_NewGain_N() > 0
                || ManagerAlphabetEvent.alphabetIngame.IsGainNewAlphabet_S() == true)
            {
                SetEventTextBox("n_ev_5");
            }
        }
        
        //노이 부스트 이벤트일 경우
        if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage() && GameManager.instance.boostingStep > 0)
        {
            SetEventTextBox("p_ctn_23");
        }

        //통상스테이지 독려 모드일 경우
        if(ManagerStageAssistMissionEvent.CheckStartable_InGame()
            && ManagerStageAssistMissionEvent.GetStageAssistMissionCount(ServerContents.StageAssistMissionEventDetails[ManagerStageAssistMissionEvent.currentMissionIndex].missionType) > 0)
        {
            SetEventTextBox("n_ev_5");
        }
        
        
        //그룹랭킹 이벤트일 경우
        if (ManagerGroupRanking.IsGroupRankingStage())
        {
            SetEventTextBox("p_ctn_24");
        }
    }

    internal void SettingBoniDialogBubble()
    {
        if (ServerContents.EventChapters == null)
            return;
        int eventType = ServerContents.EventChapters.type;

        //유저 데이터에서 현재 몇 번째 그룹에 있는지 받아와야함.
        int eventGroup = (eventType != (int)EVENT_CHAPTER_TYPE.FAIL_RESET) ?
            1 : ManagerData._instance._eventChapterData._groupState;
        int eventStep = ManagerData._instance._eventChapterData._state;
        
        //1번째 그룹 이상일 경우, 현재 스텝은 이전 그룹의 수만큼 빼 줘야함.
        if (eventGroup > 1)
        {
            CdnEventChapter eventChapter = ServerContents.EventChapters;
            eventStep -= eventChapter.counts[eventGroup - 2];
            if (eventStep < 0)
            {
                return;
            }
        }

        //트레이닝 모드에서 현재 그룹의 1번째 위치가 아니라면 "정말 끝낼거야?" 말풍선 생성.
        if (eventType == (int)EVENT_CHAPTER_TYPE.FAIL_RESET && eventStep > 1)
        {
            SetEventTextBox("n_ev_2");
        }
    }

    private void SetEventTextBox(string eventStr)
    {
        eventTextBox.gameObject.SetActive(true);
        eventTextBox.transform.localScale = Vector3.one * 0.3f;
        eventTextBox.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutCirc);
        DOTween.ToAlpha(() => eventTextBox.color, x => eventTextBox.color = x, 1f, 0.2f);
        eventText.text = Global._instance.GetString(eventStr);
    }

    void SetInGameMode()
    {
        listContinueType.Clear();

        //컨티뉴 아이콘.
        SetFailIcon_Main();
        SetFailIcon_Sub();

        //컨티뉴 텍스트.
        if (btnADOn)
            SetContinueADUI();
        else
            info.text = Global._instance.GetString("p_ctn_3");
    }

    void SetFailIcon_Main()
    {
        switch (GameManager.instance.failType)
        {
            case GameFailType.TURN:
                {
                    turnImage.spriteName = "ingame_item_apple";

                    int addCount = 5 + GameManager.instance.useContinueCount;
                    if (addCount > 8) addCount = 8;
                    foreach (var label in apple)
                        label.text = addCount.ToString();
                }
                break;
            case GameFailType.LAVA:
                {
                    turnImage.spriteName = "ingame_item_lava";

                    int count = ManagerBlock.instance.stageInfo.digCount * 3;
                    foreach (var label in apple)
                        label.text = "-" + count;
                }
                break;
            case GameFailType.DYNAMITE:
                {
                    turnImage.spriteName = "ingame_item_dynamite";

                    foreach (var label in apple)
                        label.text = "+5";
                    apple[0].transform.localPosition = new Vector3(-6.2f, -23, 0);
                }
                break;
        }
        //이미지 사이즈 조정
        turnImage.MakePixelPerfect();
        turnImage.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
        turnImage.width = 112;

        //컨티뉴 타입 추가.
        listContinueType.Add(GameManager.instance.failType);
    }

    void SetFailIcon_Sub()
    {
        int count = 0;
        int iconIndex = 0;
        for (int i = 0; i < bonusSprite.Length; i++)
        {
            bonusSprite[i].gameObject.SetActive(false);
        }
        switch (GameManager.instance.failType)
        {
            case GameFailType.TURN:
            case GameFailType.LAVA:
                {   
                    //맵에 다이너 마이트 있는지 검사.
                    if (ManagerBlock.instance.CheckDynamiteInScreen() == true)
                    {
                        listContinueType.Add(GameFailType.DYNAMITE);
                        bonusSprite[iconIndex].gameObject.SetActive(true);
                        bonusSprite[iconIndex].spriteName = "ingame_item_dynamite";
                        bonusSpriteCount.transform.localPosition = new Vector3(-3.4f, -11.6f, 0f);
                        bonusSpriteCount.fontSize = 22;
                        bonusSpriteCount.text = "+5";
                        iconIndex++;
                    }
                }
                break;
            case GameFailType.DYNAMITE:
                {
                    //턴인지, 용암인지 검사.
                    if (GameManager.gameMode == GameMode.LAVA)
                    {
                        count = ManagerBlock.instance.stageInfo.digCount * 3;
                        bonusSprite[iconIndex].spriteName = "ingame_item_lava";
                        listContinueType.Add(GameFailType.LAVA);
                    }
                    else
                    {
                        count = (GameManager.instance.useContinueCount <= 3) ? 5 + GameManager.instance.useContinueCount : 8;
                        bonusSprite[iconIndex].spriteName = "ingame_item_apple";
                        listContinueType.Add(GameFailType.TURN);
                    }
                    bonusRoot.gameObject.SetActive(true);
                    bonusRoot.transform.localPosition = new Vector3(130, 41, 0);
                    bonusSprite[iconIndex].gameObject.SetActive(true);
                    bonusSpriteCount.text = count.ToString();
                    iconIndex++;
                }
                break;
        }

        //아이템.
        if (GameManager.instance.useContinueCount == 1)//라인추가
        {
            bonusSprite[iconIndex].gameObject.SetActive(true);
            bonusSprite[iconIndex].spriteName = "item_line_bomb";
            iconIndex++;
        }
        else if (GameManager.instance.useContinueCount == 2)//더블추가
        {
            bonusSprite[iconIndex].gameObject.SetActive(true);
            bonusSprite[iconIndex].spriteName = "item_bomb";
            iconIndex++;
        }
        else if (GameManager.instance.useContinueCount >= 3)//라인+더블추가
        {
            bonusSprite[iconIndex].gameObject.SetActive(true);
            bonusSprite[iconIndex].spriteName = "item_line_bomb";
            iconIndex++;
            bonusSprite[iconIndex].gameObject.SetActive(true);
            bonusSprite[iconIndex].spriteName = "item_bomb";
            iconIndex++;
        }

        //이미지 크기 설정
        for (int i = 0; i < iconIndex; i++)
        {
            bonusSprite[i].MakePixelPerfect();
            bonusSprite[i].keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
            bonusSprite[i].width = 56;
        }

        if (iconIndex > 0)
            bonusRoot.SetActive(true);
    }

    void SetCharacter()
    {
        boniLive2D = LAppModelProxy.MakeLive2D(live2DAnchor, TypeCharacterType.Boni);
        boniLive2D.SetRenderer(false);
        boniLive2D.SetAnimation("Fail_01_appear", "Fail_01_loop");
        StartCoroutine(boniLive2D.CoSetRenderer(true));
        boniLive2D.SetScale(450f);
        boniLive2D.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        boniLive2D.SetSortOrder(clearLayer);
    }

    void recvCoinStageContinue(CoinStageDiaContinueResp resp)
    {
        if (resp.IsSuccess)
        {
            //게임 타입에 따른 처리.
            Global.GameInstance.OnRecvContinue();

            Global.clover = (int)(GameData.Asset.AllClover);
            Global.coin = (int)(GameData.Asset.AllCoin);
            Global.jewel = (int)(GameData.Asset.AllJewel);
            Global.wing = (int)(GameData.Asset.AllWing);
            Global.exp = (int)GameData.User.expBall;

            ManagerUI._instance.UpdateUI();
            networkEnd = true;

            GameManager.instance.useContinueCount++;

            boniLive2D.SetAnimation("Continue_01", false);
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            if (UIPopupShop._instance != null) UIPopupShop._instance.OnClickBtnBack();
            
            StartCoroutine(CoCloseBoniDialogBubble());

            pushContinue = false;

            StartCoroutine(CoEvent(1.5f, () =>
            {
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.ClosePopUpUI();
                GameManager.instance.IsCanTouch = true;
                ManagerBlock.instance.state = BlockManagrState.WAIT;
                GameUIManager.instance.ShowTipButton(true);
                GameUIManager.instance.ShowPauseButton(true);
                BlockMatchManager.instance.SetBlockLink(true);
                // BlockTime이 0의 값을 가질 때 코인 스테이지 기본 플레이 시간으로 초기화가 된다.
                // 이때, extraTime(컨티뉴 시 추가 시간)을 turnCount(코인 스테이지 기본 플레이 시간)에서 빼준다.
                ManagerBlock.instance.BlockTime = ManagerBlock.instance.stageInfo.turnCount - resp.extraTime;
                GameManager.instance.InitCoinStageContinue();
                Pick.instance.InitPick();
                StartCoroutine(CoEvent(0.6f, () => { GameManager.instance.state = GameState.PLAY; }));
            }));


            //그로시
            {
                // 컨티뉴에 사용 된 쥬얼
                var growthyMoneyrecvGameContinue = new ServiceSDK.GrowthyCustomLog_Money
                (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_CONTINUE_PLAY,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel),
                    mrsn_DTL: $"{Global.GameInstance.GetGrowthyGameMode().ToString()}"
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoneyrecvGameContinue);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);   
            }
        }
        else
        {
            bCanTouch = true;
            GameClear_CoinStage();
        }
    }
    
    void recvGameContinue(GameContinueResp resp)
    {
        if (resp.IsSuccess)
        {
            //게임 타입에 따른 처리.
            Global.GameInstance.OnRecvContinue();

            Global.coin = (int)(GameData.Asset.AllCoin);
            Global.jewel = (int)(GameData.Asset.AllJewel);
            Global.wing = (int)(GameData.Asset.AllWing);
            Global.exp = (int)GameData.User.expBall;

            //컨티뉴 의사 재확인 후 컨티뉴 사용
            if (IsReconfirmPopup)
                GameManager.instance.continueReconfirmCount++;

            ManagerUI._instance.UpdateUI();
            networkEnd = true;

            GameManager.instance.useContinueCount++;

            boniLive2D.SetAnimation("Continue_01", false);
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            if (UIPopupShop._instance != null) UIPopupShop._instance.OnClickBtnBack();
            
            StartCoroutine(CoCloseBoniDialogBubble());

            pushContinue = false;

            StartCoroutine(CoEvent(1.5f, () =>
            {
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.ClosePopUpUI();
                GameManager.instance.IsCanTouch = true;
                ManagerBlock.instance.state = BlockManagrState.WAIT;
                GameUIManager.instance.ShowTipButton(true);
                GameUIManager.instance.ShowPauseButton(true);

                //컨티뉴 시, 인게임 아이템 재 오픈하는 사양이라면 오픈시켜줌.
                if (Global.GameInstance.GetProp(GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE) == true)
                    GameUIManager.instance.ReOpenIngameItem();  

                // 각 실패조건에 맞는 컨티뉴 효과 발동.
                for (int i = 0; i < listContinueType.Count; i++)
                {
                    Debug.Log(" === 컨티뉴 타입 : " + listContinueType[i].ToString() + " ===");
                    switch (listContinueType[i])
                    {
                        case GameFailType.TURN:
                            GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
                            break;
                        case GameFailType.LAVA:
                            ManagerBlock.instance.RecoverLavaMode(GameManager.instance.useContinueCount);
                            break;
                        case GameFailType.DYNAMITE:
                            ManagerBlock.instance.ResetDynamite();
                            break;
                        default:
                            GameManager.instance.state = GameState.PLAY;
                            break;
                    }
                }
            }));


            if (Global.GameInstance.GetItemCostType(ItemType.CONTINUE) == RewardType.coin)
            {
                //그로씨 : 코인
                var growthyMoneyrecvGameContinue = new ServiceSDK.GrowthyCustomLog_Money
                (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_CONTINUE_PLAY,
                    -usePCoin,
                    -useFCoin,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoneyrecvGameContinue);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else
            {
                //그로씨 : 쥬얼
                var growthyMoneyrecvGameContinue = new ServiceSDK.GrowthyCustomLog_Money
                (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_CONTINUE_PLAY,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel),
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoneyrecvGameContinue);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }

    private void RecvGameContinue_Ad(GameContinueNPUAdResp resp)
    {
        if (resp.IsSuccess)
        {
            //퀘스트 데이터 갱신
            QuestGameData.SetUserData();
            
            GameManager.instance.useNPUContinue = true;
            FailCountManager._instance.ClearData();
            
            //게임 타입에 따른 처리.
            Global.GameInstance.OnRecvContinue();
            
            //컨티뉴 의사 재확인 후 컨티뉴 사용
            if (IsReconfirmPopup)
                GameManager.instance.continueReconfirmCount++;

            ManagerUI._instance.UpdateUI();
            networkEnd = true;

            GameManager.instance.useContinueCount++;

            boniLive2D.SetAnimation("Continue_01", false);
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            if (UIPopupShop._instance != null) UIPopupShop._instance.OnClickBtnBack();
            
            StartCoroutine(CoCloseBoniDialogBubble());

            pushContinue = false;

            StartCoroutine(CoEvent(1.5f, () =>
            {
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.ClosePopUpUI();
                GameManager.instance.IsCanTouch = true;
                ManagerBlock.instance.state = BlockManagrState.WAIT;
                GameUIManager.instance.ShowTipButton(true);
                GameUIManager.instance.ShowPauseButton(true);

                //컨티뉴 시, 인게임 아이템 재 오픈하는 사양이라면 오픈시켜줌.
                if (Global.GameInstance.GetProp(GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE) == true)
                    GameUIManager.instance.ReOpenIngameItem();  

                // 각 실패조건에 맞는 컨티뉴 효과 발동.
                for (int i = 0; i < listContinueType.Count; i++)
                {
                    Debug.Log(" === 컨티뉴 타입 : " + listContinueType[i].ToString() + " ===");
                    switch (listContinueType[i])
                    {
                        case GameFailType.TURN:
                            GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
                            break;
                        case GameFailType.LAVA:
                            ManagerBlock.instance.RecoverLavaMode(GameManager.instance.useContinueCount);
                            break;
                        case GameFailType.DYNAMITE:
                            ManagerBlock.instance.ResetDynamite();
                            break;
                        default:
                            GameManager.instance.state = GameState.PLAY;
                            break;
                    }
                }
            }));
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }

    int useFJewel = 0;
    int usePJewel = 0;
    int useFCoin = 0;
    int usePCoin = 0;

    float yesTimer = 0;

    void OnClickBtnYes()
    {
        if (this.inFalseClickDelay)
            return;

        if (bCanTouch == false)
            return;

        if (Global.GameInstance.GetItemCostType(ItemType.CONTINUE) == RewardType.coin)
        {
            if (Global.coin < Global.GameInstance.GetContinueCost())
            {
                ManagerUI._instance.LackCoinsPopUp();
                return;
            }
        }
        else
        {
            if (Global.jewel < Global.GameInstance.GetContinueCost())
            {
                ManagerUI._instance.LackDiamondsPopUp();
                return;
            }
        }

        bCanTouch = false;
        resultChatColliderSetting(false);

        pushContinue = true;

        // 서버와 통신
        {
            int price = Global.GameInstance.GetContinueCost();

            if (Global.GameInstance.GetItemCostType(ItemType.CONTINUE) == RewardType.coin)
            {
                if ((int)ServerRepos.User.coin >= price) usePCoin = price;
                else if ((int)ServerRepos.User.coin > 0)
                {
                    usePCoin = (int)ServerRepos.User.coin;
                    useFCoin = price - (int)ServerRepos.User.coin;
                }
                else useFCoin = price;
            }
            else
            {
                if ((int)ServerRepos.User.jewel >= price) usePJewel = price;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = price - (int)ServerRepos.User.jewel;
                }
                else useFJewel = price;
            }

            int tempGameMode = (int)Global.GameType;

            var req = new GameContinueReq()
            {
                type = tempGameMode,
                unlimitedContinue = Global.GameInstance.GetProp(GameTypeProp.UNLIMITED_CONTINUE) ? 1 : 0,
                stage = Global.stageIndex,
                eventIdx = Global.eventIndex,
                chapter = Global.GameInstance.GetChapterIdx()
            };
            
            ServerAPI.GameContinue(req, recvGameContinue);
        }
    }

    private void OnClickCoinContinueYes()
    {
        if (this.inFalseClickDelay)
            return;

        if (bCanTouch == false)
            return;

        if (Global.jewel < Global.GameInstance.GetContinueCost())
        {
            ManagerUI._instance.LackDiamondsPopUp();
            return;
        }

        bCanTouch = false;
        resultChatColliderSetting(false);

        pushContinue = true;

        // 서버와 통신
        {
            int price = Global.GameInstance.GetContinueCost();

            if ((int)ServerRepos.User.jewel >= price) usePJewel = price;
            else if ((int)ServerRepos.User.jewel > 0)
            {
                usePJewel = (int)ServerRepos.User.jewel;
                useFJewel = price - (int)ServerRepos.User.jewel;
            }
            else useFJewel = price;
            
            ServerAPI.CoinStageDiaContinue(GameManager.instance.useContinueCount, recvCoinStageContinue);
        }
    }
    
    private void OnClickBtnAdContinue()
    {
        if (this.inFalseClickDelay)
            return;

        if (bCanTouch == false)
            return;

        bCanTouch = false;
        resultChatColliderSetting(false);

        pushContinue = true;

        AdManager.ShowAD_ReqNPUContinue(AdManager.AdType.AD_21, (isSuccess) =>
        {
            if (isSuccess)
            {
                var req = new GameContinueReq()
                {
                    type = (int)Global.GameType,
                    unlimitedContinue = Global.GameInstance.GetProp(GameTypeProp.UNLIMITED_CONTINUE) ? 1 : 0,
                    stage = Global.stageIndex,
                    eventIdx = Global.eventIndex,
                    chapter = Global.GameInstance.GetChapterIdx()
                };
                ServerAPI.GameContinue_ShowAd(req, RecvGameContinue_Ad);
            }
            else
            {
                bCanTouch = true;
                resultChatColliderSetting(true);
                pushContinue = false;
            }
        });
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        pushContinue = false;
    }


    void OnClickBtnNo()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        boniLive2D.SetAnimation("Continue_02_start", "Continue_02_loop");
        pushContinue = false;

        if (Global.GameType == GameType.TURN_RELAY)
        {
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

            var remainTarget = ManagerBlock.instance.GetListRemainTarget();
            var arg = new GameFailReq()
            {
                type = (int)Global.GameType,
                stage = Global.stageIndex,
                eventIdx = Global.eventIndex,
                chapter = Global.GameInstance.GetChapterIdx(),
                gameCoin = 0,
                gameScore = 0,
                Remains = remainTarget,
                stageKey = Global.GameInstance.GetStageKey(),
                seed = ServerRepos.IngameSeed,
                easyMode = GameManager.instance.LevelAdjusted > 0 ? 1 : 0,
                items = usedItems
            };
            ServerAPI.TurnRelayGameFail(arg, recvTurnRelayGameFail);
        }
        else if (Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            GameManager.instance.StageClear();
        }
        else
        {
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
    }

    void recvGameFail(GameFailResp resp)
    {
        //Debug.Log("GameFail: " + resp);
        if (resp.IsSuccess)
        {
            if(noyBoostingGauge.gameObject.activeInHierarchy)
                noyBoostingGauge.gameObject.SetActive(false);
            Global.GameInstance.OnRecvGameFail();

            //상품판매 법률 개정으로 Continue -> Fail 팝업창으로 넘어갈 때 리뷰 창 비활성화 및 리뷰창 생성.
            if (LanguageUtility.IsShowBuyInfo)
            {
                objDescription.SetActive(false);
                MakeResultChat();
            }
            
            ManagerUI._instance.OpenPopupFail(resp);

            if (resultChat != null)
            {
                resultChat.panelCount = panelCount;
                resultChat.uiPanel.depth += (panelCount + 1);
                resultChat.scrollView.panel.depth += (panelCount + 1);
            }
            
            StartCoroutine(CoCloseBoniDialogBubble());
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }

    void recvTurnRelayGameFail(TurnRelayResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            
            //상품판매 법률 개정으로 Continue -> Fail 팝업창으로 넘어갈 때 상품판매 정보창 비활성화
            if (LanguageUtility.IsShowBuyInfo)
                objDescription.SetActive(false);
            
            ManagerUI._instance.OpenPopupFail();
            StartCoroutine(CoCloseBoniDialogBubble());
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }
    
    void recvEndContentsGameFail(EndContentsResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            
            //상품판매 법률 개정으로 Continue -> Fail 팝업창으로 넘어갈 때 상품판매 정보창 비활성화
            if (LanguageUtility.IsShowBuyInfo)
                objDescription.SetActive(false);
            
            ManagerUI._instance.OpenPopupFail();
            StartCoroutine(CoCloseBoniDialogBubble());
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }

    IEnumerator CoCloseBoniDialogBubble()
    {
        eventTextBox.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCirc);
        DOTween.ToAlpha(() => eventTextBox.color, x => eventTextBox.color = x, 0f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        eventTextBox.gameObject.SetActive(false);
    }

    bool networkEnd = false;
    IEnumerator CoEvent(float waitTime, UnityAction action)
    {
        float timer = 0;
        while (timer < waitTime)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        action();
        yield return null;
    }

    private void MakeResultChat()
    {
        if (resultChat == null)
        {
            resultChat = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIIngameResultChat", gameObject).GetComponent<UIIngameResultChat>();
        }
        resultChat.transform.localPosition = new Vector3(0f, 80f, 0f);
        resultChat.uiPanel.useSortingOrder = true;
        resultChat.scrollView.panel.useSortingOrder = true;
        resultChat.uiPanel.sortingOrder = clearLayer + 1;
        resultChat.scrollView.panel.sortingOrder = clearLayer + 2;

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

    public void resultChatColliderSetting(bool bColl)
    {
        if (resultChat != null)
            resultChat.SetCollider(bColl);
    }

    bool IsReconfirmPopup = false;
    void OnClickReconfirm()
    {
        if (bCanTouch == false)
            return;
        
        IsContinuReconfirm = true;
        IsReconfirmPopup = true;

        SetActiveItemBubble();

        if (Global.isSingleRoundEvent)
        {
            Vector3 infoPosition = info.transform.localPosition;
            infoPosition.x = 70;
            info.transform.localPosition = infoPosition;
            
            Reward reward = ManagerSingleRoundEvent.instance.GetReward();
            if (reward != null)
            {
                info.text = Global._instance.GetString("p_ctn_22").Replace("[0]", RewardHelper.GetRewardName((RewardType)reward.type, reward.value));
                infoGenericReward.SetReward(reward);
                infoGenericReward.gameObject.SetActive(true);
            }

            infoImage.LoadCDN(Global.gameImageDirectory, "IconEvent/", "continue_kroog");
            infoImage.enabled = true;
        }
        else
        {
            Vector3 infoPosition = info.transform.localPosition;
            infoPosition.x = 5;
            info.transform.localPosition = infoPosition;
            info.text = Global._instance.GetString("p_ctn_17");
            
            infoGenericReward.gameObject.SetActive(false);
            infoImage.enabled = false;
        }

        for (int i = 0; i < btnClose.Length; i++)
        {
            btnClose[i].functionName = "OnClickBtnBack";
        }
    }

    void SetActiveItemBubble()
    {
        int ActiveCount = 0;

        //사용한 아이템 체크
        List<bool> listUseItem = Global.GameInstance.GetUseItemData();

        for (var i = 0; i < objActiveItem.Count; i++)
        {
            if (listUseItem.Count > i)
            {
                objActiveItem[i].SetActive(listUseItem[i]);

                if (listUseItem[i])
                {
                    ActiveCount++;
                }
            }
            else
            {
                objActiveItem[i].SetActive(false);
            }
        }

        objActiveItemBubble.SetActive(ActiveCount > 0);

        tableActiveItem.Reposition();
    }

    public void SetCoinStageContinueBtn()
    {
        btnYesCoinStage.functionName = "OnClickCoinContinueYes";
        
        for (int i = 0; i < btnClose.Length; i++)
        {
            btnClose[i].functionName = "GameClear_CoinStage";
        }
    }

    private void GameClear_CoinStage()
    {
        GameManager.instance.isCoinStageClear = true;
        GameManager.instance.StageClear();
        
        ClosePopUp();
    }
}
