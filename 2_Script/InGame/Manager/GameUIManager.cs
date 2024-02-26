using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity;
using DG.Tweening;

public enum TARGET_TYPE
{
    CRACK,
    KEY,
    STATUE,
    SHOVEL,
    JEWEL,
    BOMB_COLLECT,//SCORE,
    COLORBLOCK,
    BLACK,  //사용하지 않음
    ICE,
    CARPET,
    FLOWER_POT,
    COUNT_CRACK1,
    COUNT_CRACK2,
    COUNT_CRACK3,
    COUNT_CRACK4,
    PEA,
    FLOWER_INK,
    SPACESHIP,
    BOMB_ALL,
    BOMB_LINE,
    BOMB_CIRCLE,
    BOMB_RAINBOW,
    HEART,
    BREAD_1,
    BREAD_2,
    BREAD_3,
}

public class GameUIManager : MonoSingletonOnlyScene<GameUIManager>
{
    private readonly Vector3 GAME_ITEM_POS = new Vector3(-64, 75, 0); //60
    private const    float   GAUGE_WIDTH   = 182f;
    
    //최상위 패널
    public GameObject TopUIRoot;

    //게임타겟
    public GameObject targetObj;
    public GameObject Target_Root;
    public List<GameMissionTarget> listGameTarget = new List<GameMissionTarget>();
    public List<GameObject> listUiObj = new List<GameObject>();

    public UISprite targetBG;
    public UISprite targetIcon;

    //턴
    public GameObject MoveCount_Root;
    public GameObject MoveCountBGRoot;
    public UILabel moveCountLabel;
    public UILabel moveCountLabelShadow;
    public GameObject moveCountAddTurnBubble;
    public UILabel moveCountAddTurnLabel;
    public AnimationCurve _curveAddApple = new AnimationCurve();
    public GameObject turnUi;
    public UISprite turnSpirte;
    public UISprite turnShadowSpirte;
    public UISprite turnIceSpirte;
    public UISprite turnLightSprite;

    public UILabel[] turnLabelList;

    //점수
    public GameObject scoreRoot;
    public UISlider scoreSlider;
    public UILabel scoreLabel;
    public GameObject gaugeRoot;

    //코인
    public GameObject coinRoot;
    public UILabel coinLabel;
    public ParticleSystem coinParticle;
    public UISprite coinSprite;


    //인게임아이템
    public GameObject gameItemObj;
    public List<GameItem> listGameItem = new List<GameItem>();
    public GameObject gameItemManagerObj;
    public GameObject gameItemRoot;

    //인게임루트
    public GameObject AnchorBottom;
    public GameObject groundAnchor;
    public GameObject UI_Root;
    public Transform groundMoveTransform;
    public GameObject Effect_Root;
    public GameObject gyroRoot;
    public GameObject gyroSpawn_Root;
    public GameObject grassRoot;

    //땅파기모드
    public UIPanel block_Panel;
    public GameObject DigModeBG;
    public GameObject DigModeBG1;
    public GameObject DigModeBlockBG;

    //땅파기 게이지
    public GameObject DigGauge_Root;
    public GameObject digGradation;
    const float digGradStart = 50;
    const float digGradEnd = -56;

    //일시정지버튼
    public GameObject pauseButton;
    //일시정지 팝업을 리스트형태로 가지고 있음(여러개 출력되는 상황을 대비)
    public List<GameObject> listPopupPause = new List<GameObject>();

    //챕터미션관련
    public UISprite CandySprite;

    //별표시
    int tempFlowerCount = 0;
    int flowerClearState = 0;
    public GameObject flowerGaugeRoot;
    public SkeletonAnimation[] flowerSpineObj;
    public GameObject[] flowerGauge;
    public List<UIWidget> iphoneXWidget = new List<UIWidget>();
    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    //스페셜이벤트
    public GameObject specialEventRoot;
    public UIUrlTexture specialEventObj;
    public UILabel specialEventLabel;

    //재료모으기이벤트
    public GameObject materialEventRoot;
    public UIUrlTexture materialEventObj;
    public UILabel materialEventLabel;
    
    //보물찾기 이벤트
    public UISprite treasureHuntScoreSprite;
    public UISprite treasureHuntLightSprite;
    public GameObject treasureHuntScoreLight;
    public UISprite treasureHuntCharSprite;
    
    //퍼즐 명화 이벤트
    [SerializeField] private GameObject objAtelierRoot;

    int materialTurnCount = -1;
    bool isGetSpecialEvent = false;

    //재료정해지기전
    public GameObject materialRoutteRoot;
    public UILabel materialRoutteCountLabel;
    public UILabel[] materialRoutteRatioLabel;

    public UISprite[] materialBubbleBefore;
    public UISprite[] materialBubbleAfter;

    //점수업
    public GameObject scoreUpRoot;
    public UISprite scoreUpSprite;
    
    public GameObject endScoreUpRoot;
    public GameObject scoreUpTextRoot;
    public UILabel scoreUpTextLabel;
    public GameObject scoreUpTextBg;
    
    public GameObject scoreUpBubbleRoot;
    public UILabel scoreUpBubbleLabel;

    public GameObject InGameCamera;

    public UILabel[] targetLabel;

    //피울 수 있는 최대 꽃 단계
    public ScoreFlowerType maxType_flowerState = ScoreFlowerType.FLOWER_WHITE;

    //빨간꽃모으기
    public UIEcopiIngame uiEcopiIngame;

    public bool IsRedFlowerGetEvent = false;
    public bool IsBlueFlowerGetEvent = false;
    public bool IsWhiteFlowerGetEvent = false;

    public int RedFlowerGetCount = 0;
    public int RedFlowerTargetCount = 0;

    //모으고자 하는 아이템 스프라이트
    public UISprite collectItemSprite;

    //샌드박스메세지
    public UILabel sandboxLabel;

    //모험모드
    public GameObject Advance_Root;
    public GameObject Advance_Gaige_Root;
    public GameObject Advanture_Block_BG;
    public GameObject Advanture_Grass;
    public GameObject Advanture_Effect_Root;

    //bool isChargeAdvantureGaige = false;
    public UISlider advantureGaigeSlider;
    public UISprite advantureGaigeBomb;
    public UILabel advantureWaveLabel;
    public UIItemAdventureBGSelecter advantureBGSkeleton;
    public UIItemAdventureBGSelecter adventureBGSky;
    public UISprite adventureEffectBG;
    public GameObject adventureDarkBGBlock;

    //모험모드 보상
    public GameObject adventure_RewardRoot;
    public UILabel adventureCoinLabel;
    public ParticleSystem coinAdventureParticle;

    public UILabel treasureLabel;
    public UISprite treasureSprite;

    public UILabel adventureComboLabel;

    //모험모드 아이템 가이드
    public GameObject adventureItemGuideRoot;

    //배경
    public GameObject ingameBGSprites;

    //에디터에서 사용
    public GameObject objScreenLine;
    public BoxCollider editPopupCollider;

    //스킵버튼
    public GameObject SkipBtn;

    //카운트다운
    public GameObject startCountDownRoot;
    public UITexture textureCountDown_1;
    public UITexture textureCountDown_2;

    #region 월드랭킹
    public GameObject scoreBubbleObj_worldRank;
    public UILabel[] scoreRatioLabel_worldRank;
    #endregion

    #region 턴 릴레이
    [SerializeField] private GameObject eventUIRoot_turnRelay;
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPointCount_turnRelay;

    //추가 포인트 말풍선
    [SerializeField] private GameObject objEventPointBubble_turnRelay;
    [SerializeField] private UILabel labelEventPointBubble_turnRelay;

    //현재까지 모은 포인트 말풍선
    [SerializeField] private GameObject objAllPointBubble_turnRelay;
    [SerializeField] private UILabel[] labelAllPointBubble_turnRelay;

    //턴 릴레이 보너스 아이템
    [SerializeField] private GameObject gameItemRoot_turnRelay;
    [SerializeField] private List<UITurnRelayEvent_GameItem> listGameItem_turnRelay;
    #endregion

    #region 알파벳 모으기 UI
    public GameObject alphabetEventUIRoot;

    [System.Serializable]
    public class AlphabetInagameUI_Normal
    {
        public GameObject alphabetObj;
        public UISprite spriteAlphabet;
    }

    //일반 알파벳 UI
    public GameObject alphabetUIRoot_N;
    public GameObject alphabetIconRoot_N;
    public UISprite alphabetBG_N;
    public List<AlphabetInagameUI_Normal> listAlphabetUI_N;

    //스페셜 알파벳 UI
    public GameObject alphabetUIRoot_S;
    public GameObject unknownText_S;
    public UIUrlTexture textureAlphabet_S;

    //알파벳 모으기 연출 관련
    private bool isCompleteAlphabet_Normal = false;
    private bool isCompleteAlphabet_Special = false;
    private Coroutine alphabetActionRoutine = null;
    #endregion

    #region 턴 컬러
    private Color defaultEffectColor_turn = new Color(118f / 255f, 72f / 255f, 36f / 255f);
    private Color warningEffectColor_turn = new Color(88f / 255f, 4f / 255f, 0f);
    private Color warningColor_turn = new Color(239f / 255f, 54f / 255f, 86f / 255f);
    #endregion

    #region 통상 배경
    public List<GameObject> listNormalBGRoot = new List<GameObject>();
    #endregion

    #region 커스텀 배경
    public UISprite IngameBG_Base;
    public List<UISprite> listIngameBG_Grass = new List<UISprite>();

    public List<GameObject> listCustomBGRoot = new List<GameObject>();
    public UISprite IngameBG_Gradation;
    public UISprite IngameBG_Hill;
    public List<UISprite> listIngameBG_Deco = new List<UISprite>();
    #endregion

    #region 기믹 튜토리얼

    //기믹 튜토리얼 버튼(팁버튼)
    public GameObject tipButton;
    public UIPopupGimmickTutorial gimmickTutorialPopup;

    #endregion

    #region 빙고 이벤트

    [SerializeField] private GameObject objBingoEventImage;
    
    #endregion

    #region 우주여행 이벤트
    
    //우주여행 보너스 아이템
    [SerializeField] private GameObject gameItemRoot_spaceTravel;
    [SerializeField] private List<UISpaceTravelEventGameItem> listGameItem_spaceTravel;
    
    #endregion

    //목표 UI 카운트 갱신해주기 위한 리스트.
    private List<int> listTargetUICount = new List<int>();

    //자이로 오브젝트 리스트.
    private List<IngameGyroObj> listGyroObj = new List<IngameGyroObj>();

    //수집형 아이템 액션 코루틴 (월드랭킹, 엔드컨텐츠)
    private Coroutine collectItemCountAction = null;

    void Awake()
    {
        foreach (var obj in Liststatue) Destroy(obj);
        Liststatue.Clear();
    }

    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            if ((float)Screen.height / (float)Screen.width > 2f || (float)Screen.width / (float)Screen.height > 2f)
            {
                for (int i = 0; i < iphoneXWidget.Count; i++)
                { iphoneXWidget[i].topAnchor.absolute = -100;
                    iphoneXWidget[i].UpdateAnchors();
                }

                for (int i = 0; i < iphoneXWidgetY.Count; i++)
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 85;
                    iphoneXWidgetY[i].UpdateAnchors();
                }
            }
        }
    }

    /// <summary>
    /// 꽃 랭킹을 사용하는 게임타입의 경우, 점수 설정
    /// flowerChangeAction : 꽃 단계가 변했을 때 실행될 액션
    /// </summary>
    private void SetScore_GameTypeUseFlowerScore(int tempScore, System.Action flowerChangeAction = null)
    {
        //점수 게이지 설정
        switch (maxType_flowerState)
        {
            case ScoreFlowerType.FLOWER_RED:
                scoreSlider.value = (float)tempScore / ((((float)ManagerBlock.instance.stageInfo.score4)) * 1.1f);
                break;
            case ScoreFlowerType.FLOWER_BLUE:
                scoreSlider.value = (float)tempScore / ((float)ManagerBlock.instance.stageInfo.score4);
                break;
            default:
                scoreSlider.value = (float)tempScore / (float)ManagerBlock.instance.stageInfo.score3;
                break;
        }

        #region 꽃 단계 변할 때 설정
        //점수를 한번에 많이 획득한다면 꽃 단계가 한번에 변하므로, else문을 사용하지 않음.
        
        //0단계 -> 1단계(x -> 씨앗)
        if (tempFlowerCount < 1 && tempScore >= ManagerBlock.instance.stageInfo.score1)
        {
            ChangeFlowerState(1, flowerChangeAction);
        }

        //1단계 -> 2단계(씨앗 -> 새싹)
        if (tempFlowerCount < 2 && tempScore >= ManagerBlock.instance.stageInfo.score2)
        {
            ChangeFlowerState(2, flowerChangeAction);
        }

        //2단계 -> 3단계(새싹 -> 흰꽃)
        if (tempFlowerCount < 3 && tempScore >= ManagerBlock.instance.stageInfo.score3)
        {
            ChangeFlowerState(3, flowerChangeAction);
        }

        //최대로 피울 수 있는 꽃 단계가 파란 꽃 이상일 때.
        if (maxType_flowerState >= ScoreFlowerType.FLOWER_BLUE)
        {
            //3단계 -> 4단계(흰꽃 -> 파란꽃)
            if (tempFlowerCount < 4 && tempScore >= ManagerBlock.instance.stageInfo.score4)
            {
                ChangeFlowerState(4, flowerChangeAction);
            }
        }

        //최대로 피울 수 있는 꽃 단계가 빨간 꽃 이상일 때.
        if (maxType_flowerState >= ScoreFlowerType.FLOWER_RED)
        {
            //5단계 -> 5단계(파란꽃 -> 빨간꽃)
            if (tempFlowerCount < 5 && tempScore >= (ManagerBlock.instance.stageInfo.score4 * 1.1f))
            {
                ChangeFlowerState(5, flowerChangeAction);
            }
        }
        #endregion
    }

    //꽃 단계 변경될 때의 설정.
    private void ChangeFlowerState(int changeState, System.Action flowerChangeAction)
    {
        tempFlowerCount = changeState;
        SetFlowerSpineAtFlowerStateChange();
        if (flowerChangeAction != null)
            flowerChangeAction.Invoke();
    }

    //꽃 단계에 따라 꽃 스파인 설정 변경해주는 함수.
    private void SetFlowerSpineAtFlowerStateChange()
    {
        int spineIdx = tempFlowerCount - 1;
        flowerSpineObj[spineIdx].state.SetAnimation(0, "appear_1", false);
        flowerSpineObj[spineIdx].state.AddAnimation(0, "idle2", true, 0);

        if (spineIdx > 0)
        {
            flowerSpineObj[spineIdx].state.Complete += delegate
            {
                for (int i = 0; i < spineIdx; i++)
                {
                    flowerSpineObj[i].state.SetAnimation(0, "idle2", true);
                }
            };
        }
    }

    #region 꽃 단계 변할때 실행될 액션들
    //꽃 피우기 이벤트 액션
    private void ChangeFlowerAction_BlossomEvent()
    {
        switch (tempFlowerCount)
        {
            case 3:
                if (IsWhiteFlowerGetEvent)
                    uiEcopiIngame.BlossomAction();
                break;
            case 4:
                if (IsBlueFlowerGetEvent)
                    uiEcopiIngame.BlossomAction();
                break;
            case 5:
                if (IsRedFlowerGetEvent)
                    uiEcopiIngame.BlossomAction();
                break;
        }
    }

    //월드랭킹 이벤트 액션
    private void ChangeFlowerAction_WorldRanking()
    {
        if (tempFlowerCount <= 1)
            return;

        //말풍선 활성화
        if (scoreBubbleObj_worldRank.activeInHierarchy == false)
            scoreBubbleObj_worldRank.gameObject.SetActive(true);

        //말풍선 위치 설정
        int spineIdx = tempFlowerCount - 1;
        float targetPos_X = flowerSpineObj[spineIdx].transform.position.x;
        Vector3 originPos = scoreBubbleObj_worldRank.transform.position;
        scoreBubbleObj_worldRank.transform.position = new Vector3(targetPos_X, originPos.y, originPos.z);

        //말풍선 텍스트 설정
        string ratioText = string.Format("x{0}", tempFlowerCount);
        for (int i = 0; i < scoreRatioLabel_worldRank.Length; i++)
        {
            scoreRatioLabel_worldRank[i].text = ratioText;
        }
    }
    
    //엔드컨텐츠 이벤트 액션
    private void ChangeFlowerAction_EndContents()
    {
        if (ManagerEndContentsEvent.instance.GetScoreRatio()[tempFlowerCount - 1] < 2)
            return;

        //말풍선 활성화
        if (scoreBubbleObj_worldRank.activeInHierarchy == false)
            scoreBubbleObj_worldRank.gameObject.SetActive(true);

        //말풍선 위치 설정
        int spineIdx = tempFlowerCount - 1;
        float targetPos_X = flowerSpineObj[spineIdx].transform.position.x;
        Vector3 originPos = scoreBubbleObj_worldRank.transform.position;
        scoreBubbleObj_worldRank.transform.position = new Vector3(targetPos_X, originPos.y, originPos.z);

        //말풍선 텍스트 설정
        string ratioText = string.Format("x{0}", ManagerEndContentsEvent.instance.GetScoreRatio()[tempFlowerCount - 1]);
        for (int i = 0; i < scoreRatioLabel_worldRank.Length; i++)
        {
            scoreRatioLabel_worldRank[i].text = ratioText;
        }
    }

    #endregion

    /// <summary>
    /// 코인소모처 이벤트용 점수 세팅 함수
    /// </summary>
    private void SetScore_GameTypeTreasureHunt(int tempScore)
    {
        scoreSlider.value = (float)tempScore / (float)ManagerBlock.instance.stageInfo.score2;
        if (!treasureHuntScoreLight.activeSelf)
        {
            if ((float) tempScore >= (float) ManagerBlock.instance.stageInfo.score2)
            {
                treasureHuntScoreSprite.color = Color.white;
                treasureHuntScoreLight.SetActive(true);
                treasureHuntScoreLight.transform.localScale = Vector3.one * 0.1f;
                treasureHuntScoreLight.transform.DOScale(Vector3.one * 0.5f, 0.2f);
            }
        }
    }

    public void SetScore(int tempScore)
    {
        if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_INGAME) == false)
            return;

        scoreLabel.text = tempScore.ToString();

        if (Global.GameType == GameType.NORMAL)
        {
            SetScore_GameTypeUseFlowerScore(tempScore, ChangeFlowerAction_BlossomEvent);
        }
        else if (Global.GameType == GameType.WORLD_RANK)
        {
            SetScore_GameTypeUseFlowerScore(tempScore, ChangeFlowerAction_WorldRanking);
        }
        else if (Global.GameType == GameType.END_CONTENTS)
        {
            SetScore_GameTypeUseFlowerScore(tempScore, ChangeFlowerAction_EndContents);
        }
        else if (Global.GameType == GameType.EVENT)
        {
            if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_INGAME))
                SetScore_GameTypeUseFlowerScore(tempScore);
            else
                return;
        }
        else if (Global.GameType == GameType.TREASURE_HUNT)
        {
            SetScore_GameTypeTreasureHunt(tempScore);
        }
        else if(Global.GameType == GameType.ATELIER)
        {
            if(Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_INGAME))
            {
                SetScore_GameTypeUseFlowerScore(tempScore);
            }
            else
            {
                return;
            }
        }

        endScoreUpRoot.SetActive(false);
        scoreUpBubbleRoot.SetActive(false);
        scoreUpTextRoot.SetActive(false);
        
        //점수업
        if (Global.GameType == GameType.END_CONTENTS)
        {
            bool useScoreUpItem1 = Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1;
            bool useScoreUpItem2 = Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1;
            int scoreItem = 0;
             
            if (useScoreUpItem1 || useScoreUpItem2)
            {
                scoreItem = useScoreUpItem2 ? 20 : 10;
            }
            
            //스코어업 이미지의 텍스트 표시
            SetScoreUpImage_Text(scoreItem + (int)Global.GameInstance.GetBonusRatio(), ManagerEndContentsEvent.instance.Buff == 7);

            //스코어업 옆의 말풍선에 텍스트 표시
            if (ManagerEndContentsEvent.instance.Buff > 0)
                SetScoreUpImage_Bubble((int)Global.GameInstance.GetBonusRatio());
        }
        else
        {
            if ((Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
                || (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1))
            {
                SetScoreUpImage();
            }
        }
    }

    void SetScoreUpImage()
    {
        scoreUpRoot.SetActive(true);
        if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
            scoreUpSprite.spriteName = "item_scoreUp20";
        else
            scoreUpSprite.spriteName = "item_scoreUp";
    }

    void SetScoreUpImage_Text(int percent, bool full)
    {
        if (percent == 0) return;
        endScoreUpRoot.SetActive(true);
        scoreUpTextRoot.SetActive(true);
        scoreUpTextLabel.text = $"+{percent}%";
        scoreUpTextBg.SetActive(full);
        // 버프 단계가 가장 높은 상태라면 텍스트 컬러값 변경
        Color colorTop = Color.white;
        Color colorBottom = Color.white;
        ColorUtility.TryParseHtmlString(full ? "#ffef83" : "#ffffff", out colorTop);
        ColorUtility.TryParseHtmlString(full ? "#ffe431" : "#ffbb57", out colorBottom);
        scoreUpTextLabel.gradientTop = colorTop;
        scoreUpTextLabel.gradientBottom = colorBottom;
    }
    
    void SetScoreUpImage_Bubble(int percent)
    {
        endScoreUpRoot.SetActive(true);
        scoreUpBubbleRoot.SetActive(true);
        scoreUpBubbleLabel.text = Global._instance.GetString("icon_17").Replace("[n]", percent.ToString());
    }

    public void SetCoin(int tempCoin)
    {
        coinLabel.text = tempCoin.ToString();

        coinParticle.gameObject.SetActive(true);
        coinParticle.Play();

        //코인획득효과
        StartCoroutine(CoAddCoin());
    }

    float waitTimerTreasure = 0f;
    IEnumerator CoAddCoin()
    {
        waitTimerTreasure = 0;
        while (waitTimerTreasure < 1f)
        {
            waitTimerTreasure += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimerTreasure);
            coinSprite.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f) * 1f);

            yield return null;
        }
    }

    public void SetTreasure(int tempTreasure)
    {
        treasureLabel.text = tempTreasure.ToString();
        //보물상자 획득효과
        StartCoroutine(CoAddTreasure());
    }

    IEnumerator CoAddTreasure()
    {
        treasureSprite.spriteName = string.Format("icon_treasureBox_{0}_01", ((TreasureType)AdventureManager.instance.TreasureType).ToString());
        waitTimerTreasure = 0;
        while (waitTimerTreasure < 1f)
        {
            waitTimerTreasure += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimerTreasure);
            treasureSprite.transform.localScale = (Vector3.one * 0.7f) * ((1 + (ratio - 1) * 0.5f) * 1f);

            yield return null;
        }
    }

    //게임 시작 전 카운트 다운 연출
    public IEnumerator CoStartCountDownAction()
    {
        textureCountDown_1.alpha = 0;
        textureCountDown_2.alpha = 0;
        textureCountDown_2.transform.localScale = Vector3.zero;
        startCountDownRoot.SetActive(true);

        ManagerSound.AudioPlay(AudioInGame.YOI);

        textureCountDown_1.transform.DOLocalMoveX(0f, 0.3f).SetEase(Ease.OutBack);
        DOTween.ToAlpha(() => textureCountDown_1.color, x => textureCountDown_1.color = x, 1, 0.2f);
        yield return new WaitForSeconds(0.5f);

        textureCountDown_1.transform.DOScale(Vector3.zero, 0.3f);
        DOTween.ToAlpha(() => textureCountDown_1.color, x => textureCountDown_1.color = x, 1, 0.3f);
        yield return new WaitForSeconds(0.3f);

        ManagerSound.AudioPlay(AudioInGame.START);

        textureCountDown_2.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        DOTween.ToAlpha(() => textureCountDown_2.color, x => textureCountDown_2.color = x, 1, 0.2f);
        yield return new WaitForSeconds(0.5f);

        textureCountDown_2.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InCubic);
        DOTween.ToAlpha(() => textureCountDown_2.color, x => textureCountDown_2.color = x, 0, 0.2f);
        yield return new WaitForSeconds(0.1f);
    }

    //게임시작할때 UI정렬
    public void GameStart()
    {
        SetIngameBG();
        SetDigMode();
        SetFlower();
        SetUI();
        SetGameItem();
        SetChapterMission();
        SetCoin(ManagerBlock.instance.coins);
        SetSpecialEvent();
        SetAlphabetEvent();
        SetMaterialEvent();
        SetScore(0);
        SetAdvanceMode();
        SetListGyroObject();
        SetBingoEvent();

        //게임 타입에 따라 UI 설정.
        Global.GameInstance.SetIngameUI();
    }

    private void SetIngameBG()
    {
        if (ManagerUIAtlas.GetCustomIngameBGAtlasIndex() == -1)
            return;

        //커스텀 배경 오브젝트 활성화, 통상 배경 오브젝트 비활성화
        for (int i = 0; i < listNormalBGRoot.Count; i++)
            listNormalBGRoot[i].SetActive(false);

        for (int i = 0; i < listCustomBGRoot.Count; i++)
            listCustomBGRoot[i].SetActive(true);

        //커스텀 배경 사용하는 이미지들에 커스텀 배경 설정
        ManagerUIAtlas.CheckAndApplyIngameBGAtlas(IngameBG_Base);
        IngameBG_Base.type = UIBasicSprite.Type.Simple;

        ManagerUIAtlas.CheckAndApplyIngameBGAtlas(IngameBG_Gradation);
        ManagerUIAtlas.CheckAndApplyIngameBGAtlas(IngameBG_Hill);

        for (int i = 0; i < listIngameBG_Grass.Count; i++)
            ManagerUIAtlas.CheckAndApplyIngameBGAtlas(listIngameBG_Grass[i]);

        for (int i = 0; i < listIngameBG_Deco.Count; i++)
            ManagerUIAtlas.CheckAndApplyIngameBGAtlas(listIngameBG_Deco[i]);
    }

    public void SetAdventureCoin(int tempCoin)
    {
        adventureCoinLabel.text = tempCoin.ToString();

        coinAdventureParticle.gameObject.SetActive(true);
        coinAdventureParticle.Play();
    }

    void SetAdvanceMode()
    {
        if (GameManager.gameMode != GameMode.ADVENTURE)
            return;

        Target_Root.SetActive(false);
        turnUi.SetActive(false);
        scoreRoot.SetActive(false);
        coinRoot.SetActive(false);

        Advance_Root.SetActive(true);
        adventureBGSky.GetAdventureObject().SetActive(true);

        //if(GameManager.adventureMode != AdventureMode.ORIGIN)
        //    Advance_Gaige_Root.SetActive(true);

        Advanture_Block_BG.SetActive(true);
        Advanture_Grass.SetActive(false);

        //advantureGaigeSlider.value = 0;
        ingameBGSprites.SetActive(false);

        advantureWaveLabel.color = Global.GameType == GameType.ADVENTURE_EVENT ?
            new Color(64.0f / 255.0f, 107.0f / 255.0f, 134.0f / 255.0f) :
            new Color(27.0f / 255.0f, 71.0f / 255.0f, 90.0f / 255.0f);
        advantureWaveLabel.gameObject.SetActive(true);

        adventure_RewardRoot.SetActive(true);
        adventureCoinLabel.text = "0";

        adventureItemGuideRoot.SetActive(true);

        SetAdventureCoin(ManagerBlock.instance.coins);
    }

    public void SetAdvantureWave(int waveCount, int totalWaveCount)
    {
        advantureWaveLabel.text = "Wave " + waveCount + "/" + totalWaveCount;
    }

    public void SetAdvantureBG(string aniNum, bool loop)
    {
        var spineObj = advantureBGSkeleton.GetAdventureObject<SkeletonAnimation>();

        spineObj.state.ClearTracks();
        spineObj.state.SetAnimation(0, aniNum, loop);

        if (aniNum == "Boss_wave")
        {
            adventureBGSky.GetAdventureObject<UIItemBGController>().ChangeBG(1);
        }
    }

    void SetSpecialEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_SPECIAL_EVENT) == false)
            return;

        if (Global.specialEventIndex > 0)
        {
            foreach (var item in ServerContents.SpecialEvent)
            {
                if (item.Value.index == Global.specialEventIndex)
                {
                    int getCount = 0;

                    foreach (var itemUser in ServerRepos.UserSpecilEvents)
                    {
                        if (itemUser.eventIndex == Global.specialEventIndex)
                        {
                            getCount = itemUser.progress;
                        }
                    }

                    int maxGetCount = item.Value.sections[item.Value.sections.Count - 1];

                    if (getCount < maxGetCount)
                    {
                        isGetSpecialEvent = true;
                        specialEventRoot.SetActive(true);
                        specialEventLabel.text = "0";
                        specialEventObj.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);

                        materialRoutteRoot.SetActive(true);
                        materialTurnCount = 5;
                        materialRoutteCountLabel.text = materialTurnCount + Global._instance.GetString("ig_1");
                    }
                }
            }
        }
    }

    public void RemoveSpecialEventCount()
    {
        if (Global.specialEventIndex == 0)
            return;

        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_SPECIAL_EVENT) == false)
            return;

        if (materialTurnCount <= 0)
            return;

        if (isGetSpecialEvent == false)
            return;

        materialTurnCount--;
        materialRoutteCountLabel.text = materialTurnCount + Global._instance.GetString("ig_1");
        StartCoroutine(DoBubbleAnim(materialRoutteRoot));

        if (materialTurnCount == 0)
        {
            StartCoroutine(DoSetRoutte());
        }
    }

    IEnumerator DoBubbleAnim(GameObject tempObj)
    {
        float waitTimer = 0f;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 2;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 4);

            tempObj.transform.localScale = Vector3.one * (1 + (0.3f * scaleRatio) * (1 - waitTimer));
            yield return null;
        }
        tempObj.transform.localScale = Vector3.one;
        yield return null;
    }

    [System.NonSerialized]
    public bool isCompleteSpecialEventSettings = false;
    IEnumerator DoSetRoutte()
    {
        float waitTimer = 0f;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        while (ManagerBlock.instance.state == BlockManagrState.MOVE)
        {
            yield return null;
        }

        //확률설정
        int RatioRandom = Random.Range(0, 1000);
        int materialRatio = 1;
        if (RatioRandom > 990)
            materialRatio = 10;
        else if (RatioRandom > 950)
            materialRatio = 5;
        else if (RatioRandom > 800)
            materialRatio = 3;
        else if (RatioRandom > 500)
            materialRatio = 2;

        //룰렛돌기
        int[] ratioList = new int[] { 1, 2, 3, 5, 10 };
        waitTimer = 0f;

        materialRoutteCountLabel.gameObject.SetActive(false);
        materialRoutteRatioLabel[0].gameObject.transform.localPosition = new Vector3(-2.8f, 4.3f, 0);

        //사운드재생 SPECIAL_EVENT_ROUTTE
        ManagerSound.AudioPlay(AudioInGame.SPECIAL_EVENT_ROUTTE);

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle;

            int count = (int)(waitTimer * 5f);
            count = count % ratioList.Length;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 10);

            foreach (var temp in materialRoutteRatioLabel)
            {
                temp.text = "x" + ratioList[count].ToString();
                temp.gameObject.transform.localScale = Vector3.one * (0.8f + 0.3f * scaleRatio);
            }

            materialRoutteRoot.transform.localScale = Vector3.one * (1f + 0.05f * scaleRatio);

            foreach (var temp in materialBubbleBefore)
                temp.alpha = 1 - waitTimer * 4;

            foreach (var temp in materialBubbleAfter)
            {
                if (waitTimer * 4 > 1f)
                    temp.color = Color.white;
                else
                    temp.color = new Color(1, 1, 1, waitTimer * 4);
            }

            yield return null;
        }

        waitTimer = 0f;
        while (waitTimer < 0.1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //이펙트 사운드
        if (materialRatio > 1)
        {
            InGameEffectMaker.instance.MakeDuckEffect(materialRoutteRatioLabel[0].transform.position);
        }
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

        isCompleteSpecialEventSettings = true;
        ManagerBlock.instance.getSpecialEventRatio = materialRatio;
        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
        specialEventLabel.color = Color.yellow;

        waitTimer = 0f;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 2f;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 5) * (1 - waitTimer);//ManagerBlock.instance._curveBlockJump.Evaluate(waitTimer);

            foreach (var temp in materialRoutteRatioLabel)
            {
                temp.text = "x" + materialRatio.ToString();
                temp.gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 1f);
                temp.alpha = waitTimer * 2f;
            }

            materialBubbleAfter[0].gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 0.5f);

            yield return null;
        }

        foreach (var temp in materialRoutteRatioLabel)
            temp.text = "x" + materialRatio.ToString();

        waitTimer = 0f;
        while (true)
        {
            if (materialRoutteRoot == null)
                break;

            waitTimer += Global.deltaTimePuzzle;
            float ratio = Mathf.Sin(Mathf.PI * waitTimer);
            materialRoutteRoot.transform.localScale = new Vector3(1 + 0.08f * ratio, 1 - 0.08f * ratio, 0);
            yield return null;
        }


        yield return null;
    }

    public void SetMaterialEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_COLLECT_EVENT) && ServerContents.EventChapters.active == 1 && ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT)    //일반 이벤트 스테이지는 아니고
        {
            string blockName = "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType;
            materialEventRoot.SetActive(true);
            materialEventLabel.text = "0";
            materialEventObj.LoadCDN(Global.gameImageDirectory, "IconEvent/", blockName);
        }
    }

    //인게임 블럭 모으기 카운트 갱신
    public void RefreshMaterial()
    {
        StartCoroutine(CoChangeCount(materialEventObj.transform));
        materialEventLabel.text = ManagerBlock.instance.materialCollectEvent.ToString();
    }

    //껌딱지 모으기 카운트 갱신
    public void RefreshSpecialEvent()
    {
        StartCoroutine(CoChangeCount(specialEventObj.transform));
        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
    }


    #region 알파벳 모으기 이벤트

    void SetAlphabetEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == false
           || ManagerAlphabetEvent.instance == null
           || ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == false)
            return;

        //일반 알파벳 UI초기화
        SetAlphabetEvent_Normal();

        //스페셜 알파벳 초기화
        SetAlphabetEvent_Special();

        alphabetEventUIRoot.SetActive(true);
    }

    private void SetAlphabetEvent_Normal()
    {
        //일반 알파벳 전부 모았을 경우, ui표시하지 않음
        if (ManagerAlphabetEvent.instance.isUser_normalComplete == true)
        {
            alphabetUIRoot_N.SetActive(false);
            return;
        }

        int alphabetCount = ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N.Count;
        alphabetBG_N.width = (alphabetCount * 38) + 5;
        alphabetIconRoot_N.transform.localPosition = new Vector3(380f - (alphabetCount * 38), 0f, 0f);

        for (int i = 0; i < listAlphabetUI_N.Count; i++)
        {
            if (i < alphabetCount)
            {
                listAlphabetUI_N[i].alphabetObj.SetActive(true);
                listAlphabetUI_N[i].spriteAlphabet.spriteName 
                    = ManagerAlphabetEvent.alphabetIngame.GetAlphabetSpriteName_AtListData_N(i);
            }
            else
            {
                listAlphabetUI_N[i].alphabetObj.SetActive(false);
            }
        }
    }

    public void RefreshAlphabetEvent_Normal(int index, string spriteName)
    {
        listAlphabetUI_N[index].spriteAlphabet.spriteName = spriteName;
    }

    public void ActionAlphabetEvent_AllCollect_Normal()
    {
        isCompleteAlphabet_Normal = true;
        if (alphabetActionRoutine == null)
            alphabetActionRoutine = StartCoroutine(CoActionAlphabetEvent_AllCollect());
    }

    private void SetAlphabetEvent_Special()
    {
        if (ManagerAlphabetEvent.instance.isUser_specialComplete == true)
        {
            alphabetUIRoot_S.SetActive(false);
            return;
        }

        alphabetUIRoot_S.SetActive(true);
        if (ManagerAlphabetEvent.alphabetIngame.Alphabet_S == 0)
            return;
        
        textureAlphabet_S.gameObject.SetActive(false);
        textureAlphabet_S.SuccessEvent += SetAlphabetTexture_Special;
        string textureName = ManagerAlphabetEvent.alphabetIngame.GetAppearAlphabetSpriteName_S();
        textureAlphabet_S.LoadCDN(Global.gameImageDirectory, "IconEvent/", textureName);
    }

    private void SetAlphabetTexture_Special()
    {
        if (textureAlphabet_S.mainTexture == null)
            return;

        textureAlphabet_S.transform.localScale = Vector3.one * 0.65f;

        if (ManagerAlphabetEvent.alphabetIngame.alphabetData_S != null &&
            ManagerAlphabetEvent.alphabetIngame.alphabetData_S.getCount_All == 0)
            textureAlphabet_S.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void AppearAlphabetEvent_Special()
    {
        unknownText_S.SetActive(false);
        Vector3 originScale = textureAlphabet_S.transform.localScale;
        textureAlphabet_S.transform.localScale = Vector3.zero;
        textureAlphabet_S.gameObject.SetActive(true);
        textureAlphabet_S.transform.DOScale(originScale, 0.2f).SetEase(Ease.InOutBack);
    }

    public void RefreshAlphabetEvent_Special(string spriteName)
    {
        textureAlphabet_S.color = new Color(1f, 1f, 1f, 1f);
    }

    public void ActionAlphabetEvent_AllCollect_Special()
    {
        isCompleteAlphabet_Special = true;
        if (alphabetActionRoutine == null)
            alphabetActionRoutine = StartCoroutine(CoActionAlphabetEvent_AllCollect());
    }

    public IEnumerator CoActionAlphabetEvent_AllCollect()
    {
        while (GameManager.instance.state == GameState.PLAY)
        {
            if (isCompleteAlphabet_Normal == true)
            {
                for (int i = 0; i < listAlphabetUI_N.Count; i++)
                {
                    AlphabetInagameUI_Normal uiNormal = listAlphabetUI_N[i];
                    if (uiNormal.alphabetObj.activeInHierarchy == false)
                        break;

                    uiNormal.spriteAlphabet.transform.DOLocalJump(uiNormal.spriteAlphabet.transform.localPosition, 7.0f, 1, 0.15f);
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (isCompleteAlphabet_Special == true)
            {
                textureAlphabet_S.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.25f);
                textureAlphabet_S.transform.DOLocalJump(textureAlphabet_S.transform.localPosition, 7.0f, 1, 0.25f);
            }
            yield return new WaitForSeconds(3.0f);
        }

        alphabetActionRoutine = null;
    }
    #endregion

    //생성된 자이로 오브젝트들 삭제, 리스트 초기화.
    public void SetListGyroObject()
    {
        for (int i = 0; i < listGyroObj.Count; i++)
        {
            Destroy(listGyroObj[i].gameObject);
        }
        listGyroObj.Clear();
    }

    #region 월드랭킹 UI 설정
    public void SetWorldRanking()
    {
        if (Global.GameType == GameType.WORLD_RANK)
        {
            gyroRoot.SetActive(true);

            //월드랭킹 수집품 UI 설정
            if (collectItemCountAction != null)
            {
                StopCoroutine(collectItemCountAction);
                collectItemCountAction = null;
            }
            materialEventRoot.SetActive(true);
            if (ManagerWorldRanking.instance != null)
            {
                if (ManagerWorldRanking.resourceData?.worldRankingPack?.IngameAtlas != null)
                    collectItemSprite.atlas = ManagerWorldRanking.resourceData.worldRankingPack.IngameAtlas;
            }
            collectItemSprite.gameObject.SetActive(true);
            materialEventLabel.text = "0";
        }
    }

    public void CollectAction_WorldRankingItem(int itemCount)
    {
        //이전에 진행되던 연출 있으면 멈추기.
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemSprite.MakePixelPerfect();
        }

        //재료 카운트 표시 마지막으로 획득한 카운트까지 갱신해줌.
        materialEventLabel.text = ManagerBlock.instance.worldRankingItemCount.ToString();

        //연출에 필요한 카운트는 따로 저장.
        int actionCount = ManagerBlock.instance.worldRankingItemCount;
        if (itemCount > 8)
        {
            actionCount += (itemCount - 8);
            itemCount = 8;
        }
        
        //실제 카운트는 따로 올려주고 연출 진행.
        ManagerBlock.instance.worldRankingItemCount += itemCount;
        collectItemCountAction = StartCoroutine(CoCollectAction_WorldRanking(actionCount, itemCount));
    }

    private IEnumerator CoCollectAction_WorldRanking(int actionCount, int itemCnt)
    {
        for (int i = 0; i < itemCnt; i++)
        {
            actionCount++;
            materialEventLabel.text = actionCount.ToString();
            StartCoroutine(CoChangeCount(collectItemSprite.transform));

            //자이로 오브젝트 생성(최대 카운트 제한), 일정 수 이상되면 사라지도록 해줌.
            IngameGyroObj gyroObj = InGameEffectMaker.instance.MakeFlyWorldRankGyroItem();
            if (ManagerBlock.instance.worldRankingItemCount <= 30)
            {
                if (gyroObj != null)
                    listGyroObj.Add(gyroObj);
            }
            else
            {
                if (gyroObj != null)
                    StartCoroutine(gyroObj.CoAutoDestroy(0.5f));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    //에디터에서 사용
    public void SetWorldRanking_Editor(bool isWorldRank)
    {
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemCountAction = null;
        }
        materialEventRoot.SetActive(isWorldRank);
        collectItemSprite.gameObject.SetActive(isWorldRank);
        materialEventLabel.text = "0";
        gyroRoot.SetActive(isWorldRank);
    }
    #endregion

    #region 턴 릴레이 이벤트 UI 설정
    public void SetEventUI_TurnRelay()
    {
        eventUIRoot_turnRelay.SetActive(true);
        gameItemRoot_turnRelay.SetActive(true);

        //이벤트 포인트 값 초기화
        RefreshEventPoint_TurnRelay();

        //게임 아이템 값 초기화
        for (int i = 0; i < listGameItem_turnRelay.Count; i++)
            listGameItem_turnRelay[i].InitGameItem();
    }

    public void RefreshEventPoint_TurnRelay()
    {
        eventPointCount_turnRelay.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1, 
            ManagerTurnRelay.turnRelayIngame.IsLuckyWave());
    }

    public IEnumerator CoActionItemAddCount_TurnRelay(ManagerTurnRelay.BONUSITEM_TYPE itemType)
    {
        int findIndex = listGameItem_turnRelay.FindIndex(x => x.itemType == itemType);
        if (findIndex == -1)
            yield break;

        //게임 아이템 카운트 증가 연출
        yield return listGameItem_turnRelay[findIndex].CoAction_AppearGameItem();
    }

    public IEnumerator CoActionAddEventPoint_TurnRelay()
    {
        int currentPoint = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1;
        int prevPoint = currentPoint - ManagerTurnRelay.turnRelayIngame.BonusEventPoint;

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        //말풍선 띄우기
        labelEventPointBubble_turnRelay.text = string.Format("+{0}", ManagerTurnRelay.turnRelayIngame.BonusEventPoint);
        objEventPointBubble_turnRelay.SetActive(true);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        //텍스트 컬러 변경
        Color originColor = eventPointCount_turnRelay.labelEventPoint[0].color;
        eventPointCount_turnRelay.labelEventPoint[0].color = new Color32(0xff, 0xe9, 0x32, 0xff);

        //포인트 증가 연출
        yield return CpActionAddTurn(eventPointCount_turnRelay.labelEventPoint, prevPoint, currentPoint);

        //이펙트 출력 및 텍스트 컬러 변경
        InGameEffectMaker.instance.MakeDuckEffect(eventPointCount_turnRelay.labelEventPoint[0].transform.position);
        eventPointCount_turnRelay.labelEventPoint[0].color = originColor;
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.4f));

        //말풍선 제거
        objEventPointBubble_turnRelay.transform.DOScale(0f, 0.1f);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));
        objEventPointBubble_turnRelay.SetActive(false);
    }

    public IEnumerator CoActionShowAllPointBubble_TurnRelay()
    {
        //현재까지 모은 포인트가 없다면 말풍선 생성하지 않음.
        if (ManagerTurnRelay.turnRelayIngame.IngameEventPoint == 0)
            yield break;

        //말풍선 띄우기
        string allPointText = ManagerTurnRelay.turnRelayIngame.IngameEventPoint.ToString();
        for (int i = 0; i < labelAllPointBubble_turnRelay.Length; i++)
            labelAllPointBubble_turnRelay[i].text = allPointText;
        objAllPointBubble_turnRelay.SetActive(true);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
    }
    #endregion

    IEnumerator CoChangeCount(Transform transObj)
    {
        float tmpeWaitTimer = 0;

        while (tmpeWaitTimer < 1f)
        {
            tmpeWaitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = ManagerBlock.instance._curveBlockPopUp.Evaluate(tmpeWaitTimer);
            transObj.localScale = Vector3.one * ratio;

            yield return null;
        }
        transObj.localScale = Vector3.one;

        yield return null;
    }


    void SetChapterMission()
    {
        if (Global.GameType != GameType.NORMAL) return;

        if (EditManager.instance != null) return;

        foreach (var item in ManagerData._instance._questGameData)
        {
            if(item.Value.level == GameManager.instance.currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                    {
                        CandySprite.gameObject.SetActive(true);
                        CandySprite.spriteName = "Mission_DUCK_1";
                        CandySprite.MakePixelPerfect();
                    }
                    return;
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                    {
                        CandySprite.gameObject.SetActive(true);
                        CandySprite.spriteName = "Mission_CANDY_1";
                        CandySprite.MakePixelPerfect();
                    }
                    return;
                }
            }
        }
        CandySprite.gameObject.SetActive(false);
    }

    public void GetChapterMissionSprite()
    {
        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == GameManager.instance.currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    CandySprite.gameObject.SetActive(true);
                    CandySprite.spriteName = "Mission_DUCK_2";
                    CandySprite.MakePixelPerfect();
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    CandySprite.gameObject.SetActive(true);
                    CandySprite.spriteName = "Mission_CANDY_2";
                    CandySprite.MakePixelPerfect();
                }
            }
        }

        //획득효과 추가
        TweenPosition tweenPos = CandySprite.gameObject.GetComponent<TweenPosition>();
        tweenPos.enabled = true;
        //StartCoroutine(CoShowGetDuck());
    }

    /*
    IEnumerator CoShowGetDuck()
    {
        waitTimer = 0;

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            CandySprite.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f));

            yield return null;
        }
    }
    */


    void SetDigMode()
    {
        if(ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.NORMAL)
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_apple";
            turnShadowSpirte.spriteName = "ingame_item_apple";

            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach(var label in turnLabelList)
            {
                label.text = Global._instance.GetString("ig_2");
                label.fontSize = 25;
                label.MakePixelPerfect();
            }
        }
        else if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.DIG)
        {
            //if (ManagerBlock.instance.stageInfo.digType ==  2)            
                turnUi.SetActive(true);
            DigGauge_Root.SetActive(true);

            turnSpirte.spriteName = "ingame_item_apple";
            turnShadowSpirte.spriteName = "ingame_item_apple";
            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
            {
                label.text = Global._instance.GetString("ig_2");
                label.fontSize = 25;
                label.MakePixelPerfect();
            }
        }
        else if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.LAVA)
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_lava";
            turnShadowSpirte.spriteName = "ingame_item_lava";
            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
            {
                label.text = Global._instance.GetString("ig_3");
                label.fontSize = 22;
                label.MakePixelPerfect();
            }   
        }
    }

    void SetGameItem()
    {
        foreach(var tempItem in listGameItem)
        {
            Destroy(tempItem.gameObject);
        }
        listGameItem.Clear();

        if (Global.GameInstance.IsHideIngameItemUI() == true)
        {
            return;
        }

        if (EditManager.instance == null)
        {
            if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
            {
                bool bSale = ServerRepos.LoginCdn.RenewalAdvInGameItemSale == 1;
                MakeGameItem(GameItemType.ADVENTURE_RAINBOW_BOMB, ServerRepos.UserItem.AdventureItem(2), ServerRepos.LoginCdn.RenewalAdvIngameItems[2], bSale);
                MakeGameItem(GameItemType.SKILL_HAMMER, ServerRepos.UserItem.AdventureItem(1), ServerRepos.LoginCdn.RenewalAdvIngameItems[1], bSale);
                MakeGameItem(GameItemType.HEAL_ONE_ANIMAL, ServerRepos.UserItem.AdventureItem(0), ServerRepos.LoginCdn.RenewalAdvIngameItems[0], bSale);
            }
            else
            {
                List<int> itemPriceList = Global.GameInstance.GetItemCostList(ItemType.INGAME_ITEM);
                bool isCoinItem = Global.GameInstance.GetItemCostType(ItemType.INGAME_ITEM) == RewardType.coin;

                bool bSale = ServerRepos.LoginCdn.RenewalInGameItemSale == 1;
                MakeGameItem(GameItemType.RAINBOW_BOMB_HAMMER, ServerRepos.UserItem.InGameItem(3), itemPriceList[3], bSale, isCoinItem);
                MakeGameItem(GameItemType.THREE_HAMMER, ServerRepos.UserItem.InGameItem(2), itemPriceList[2], bSale, isCoinItem);
                MakeGameItem(GameItemType.CROSS_LINE, ServerRepos.UserItem.InGameItem(1), itemPriceList[1], bSale, isCoinItem);
                MakeGameItem(GameItemType.COLOR_BRUSH, ServerRepos.UserItem.InGameItem(4), itemPriceList[4], bSale, isCoinItem);
                MakeGameItem(GameItemType.HAMMER, ServerRepos.UserItem.InGameItem(0), itemPriceList[0], bSale, isCoinItem);
            } 
        }
        else
        {
            if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
            {
                MakeGameItem(GameItemType.ADVENTURE_RAINBOW_BOMB, 3, 3);
                MakeGameItem(GameItemType.SKILL_HAMMER, 3, 3);
                MakeGameItem(GameItemType.HEAL_ONE_ANIMAL, 3, 3);
            }
            else
            {
                MakeGameItem(GameItemType.RAINBOW_BOMB_HAMMER, 3, 3);
                MakeGameItem(GameItemType.THREE_HAMMER, 3, 3);
                MakeGameItem(GameItemType.CROSS_LINE, 3, 3);
                MakeGameItem(GameItemType.COLOR_BRUSH, 3, 3);
                MakeGameItem(GameItemType.HAMMER, 3, 3);
            }
        }
    }

    //무료아이템카운트 갱신
    public void RefreshInGameItem()
    {
        if (EditManager.instance == null)
        {
            foreach (var tempItem in listGameItem)
            {
                if (tempItem.type == GameItemType.HAMMER) tempItem.RefreshCount(ServerRepos.UserItem.InGameItem(0));
                if (tempItem.type == GameItemType.CROSS_LINE) tempItem.RefreshCount(ServerRepos.UserItem.InGameItem(1));
                if (tempItem.type == GameItemType.THREE_HAMMER) tempItem.RefreshCount(ServerRepos.UserItem.InGameItem(2));
                if (tempItem.type == GameItemType.RAINBOW_BOMB_HAMMER) tempItem.RefreshCount(ServerRepos.UserItem.InGameItem(3));
                if (tempItem.type == GameItemType.COLOR_BRUSH) tempItem.RefreshCount(ServerRepos.UserItem.InGameItem(4));

                if (tempItem.type == GameItemType.ADVENTURE_RAINBOW_BOMB) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(2));
                if (tempItem.type == GameItemType.SKILL_HAMMER) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(1));
                if (tempItem.type == GameItemType.HEAL_ONE_ANIMAL) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(0));
            }
        }
        else
        {
            foreach (var tempItem in listGameItem)
            {
                if (tempItem.type == GameItemType.HAMMER) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.CROSS_LINE) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.THREE_HAMMER) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.RAINBOW_BOMB_HAMMER) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.COLOR_BRUSH) tempItem.RefreshCount(3);

                if (tempItem.type == GameItemType.ADVENTURE_RAINBOW_BOMB) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.SKILL_HAMMER) tempItem.RefreshCount(3);
                if (tempItem.type == GameItemType.HEAL_ONE_ANIMAL) tempItem.RefreshCount(3);
            }
        }
    }

    public void ReOpenIngameItem()
    {
        for (int i = 0; i < listGameItem.Count; i++)
        {
            listGameItem[i].RefreshItemUI(true);
        }
    }

    private void MakeGameItem(GameItemType tempType, int count, int currencyCount, bool bSale = false, bool isCoin = false)
    {
        var gameItem = gameItemRoot.AddChild(gameItemObj).GetComponent<GameItem>();
        gameItem.Init(tempType, count, currencyCount, bSale, isCoin);

        gameItem._transform.localScale    = Vector3.one * 0.9f;
        gameItem._transform.localPosition = GAME_ITEM_POS + Vector3.left * 95 * listGameItem.Count;

        listGameItem.Add(gameItem);
    }
    
    void SetFlower()
    {
        //꽃 점수 사용하지 않는 타입은 점수 비활성화 시켜줌.
        if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_INGAME) == false)
        {
            scoreRoot.SetActive(false);
            return;
        }

        //현재 게임 타입에서 최대로 피울 수 있는 꽃 단계 가져오기.
        if (EditManager.instance == null)
            maxType_flowerState = Global.GameInstance.GetMaxType_FlowerScore();

        Debug.Log("GameType : " + Global.GameType + ", 꽃의 최대 단계 : " + maxType_flowerState);

        //초기화
        gaugeRoot.SetActive(true);
        tempFlowerCount = 0;
        flowerClearState = 0;

        //현재 챕터 클리어 상태 받아옴(0: 아직 챕터 덜깨짐, 1: 모두 흰꽃 이상, 2.모두 파란꽃 이상)
        if (EditManager.instance == null && (GameManager.instance.currentChapter() <= ServerRepos.UserChapters.Count - 1))
        {
            flowerClearState = ServerRepos.UserChapters[GameManager.instance.currentChapter()].clearState;
        }

        //꽃 스파인 상태 초기화.
        SetSpineFlower();

        // 에코피(꽃 피우기) 이벤트 검사
        if (Global.GameType == GameType.NORMAL)
        {
            if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent_Ingame() == true) //신버전 에코피 이벤트
            {
                //현재 스테이지에서 피운 꽃이 최대 피울 수 있는 꽃 레벨보다 작은 경우 에코피 이벤트 참여 가능
                ScoreFlowerType prevStageFlower = (ScoreFlowerType)ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;
                if (prevStageFlower < Global.GameInstance.GetMaxType_FlowerScore())
                    GameManager.instance.isRunningEcopiEvent = true;
            }

            //꽃 피우기 UI 떠야하는 상황이라면 UI표시
            if (GameManager.instance.isRunningEcopiEvent == true)
            {
                uiEcopiIngame.gameObject.SetActive(true);
                uiEcopiIngame.InitEcopiUI(maxType_flowerState);

                switch (maxType_flowerState)
                {
                    case ScoreFlowerType.FLOWER_RED:
                        IsRedFlowerGetEvent = true;
                        break;
                    case ScoreFlowerType.FLOWER_BLUE:
                        IsBlueFlowerGetEvent = true;
                        break;
                    case ScoreFlowerType.FLOWER_WHITE:
                        IsWhiteFlowerGetEvent = true;
                        break;
                }
            }
        }

        //최대 피울 수 있는 꽃에 따른 스코어UI 설정.
        switch (maxType_flowerState)
        {
            case ScoreFlowerType.FLOWER_RED:
                {
                    float score5 = ((float)ManagerBlock.instance.stageInfo.score4) * 1.1f;
                    float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / score5;
                    float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / score5;
                    float ratio3 = (float)ManagerBlock.instance.stageInfo.score3 / score5;
                    float ratio4 = (float)ManagerBlock.instance.stageInfo.score4 / score5;
                    SetPositionFlowerScore(ratio1, ratio2, ratio3, ratio4);
                }
                break;
            case ScoreFlowerType.FLOWER_BLUE:
                {
                    SetActiveFlowerScore(4, false);
                    float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / (float)ManagerBlock.instance.stageInfo.score4;
                    float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / (float)ManagerBlock.instance.stageInfo.score4;
                    float ratio3 = (float)ManagerBlock.instance.stageInfo.score3 / (float)ManagerBlock.instance.stageInfo.score4;
                    SetPositionFlowerScore(ratio1, ratio2, ratio3);
                }
                break;
            default:
                {
                    SetActiveFlowerScore(4, false);
                    SetActiveFlowerScore(3, false);
                    float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / (float)ManagerBlock.instance.stageInfo.score3;
                    float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / (float)ManagerBlock.instance.stageInfo.score3;
                    SetPositionFlowerScore(ratio1, ratio2);
                }
                break;
        }
    }

    private void SetSpineFlower()
    {
        //꽃 스코어 스파인 오브젝트 교체
        GameObject spineObj = Global.GameInstance.GetSpine_IngameScoreUI();
        if (spineObj != null)
        {
            int maxCount = (int)maxType_flowerState;
            for (int i = 0; i < maxCount; i++)
            {
                var changeObj = Instantiate(spineObj, flowerSpineObj[i].transform.parent);
                changeObj.transform.localPosition = flowerSpineObj[i].transform.localPosition;
                changeObj.transform.localScale = flowerSpineObj[i].transform.localScale;

                SkeletonAnimation changeSpine = changeObj.GetComponent<SkeletonAnimation>();
                if (changeSpine != null)
                {
                    flowerSpineObj[i].gameObject.SetActive(false);
                    flowerSpineObj[i] = changeSpine;
                }
                else
                {
                    changeObj.SetActive(false);
                }
                
                // UIRendererWidget으로 Depth 설정을 하기 때문에 SortingOrder를 0으로 초기화.
                flowerSpineObj[i].GetComponent<MeshRenderer>().sortingOrder = 0;
                
                // 스파인 Depth 설정을 위해 UIRendererWidget 추가.
                var widget = changeObj.AddComponent<UIRendererWidget>();
                widget.depth = 5 + i;
            }
        }

        if (Global.GameInstance.IsChangeSkin_IngameScoreUI() == true)
        {
            //꽃 스코어 스파인 스킨 설정
            for (int i = 0; i < flowerSpineObj.Length; i++)
            {
                flowerSpineObj[i].skeleton.SetSkin(Global.GameInstance.GetSkinName_IngameScoreUI(i));
                flowerSpineObj[i].skeleton.SetSlotsToSetupPose();
            }
        }

        //꽃 스코어 스파인 오브젝트 애니메이션 설정
        for (int i = 0; i < flowerSpineObj.Length; i++)
        {
            flowerSpineObj[i].state.SetAnimation(0, "idle", true);
        }
    }

    private void SetActiveFlowerScore(int index, bool isActive)
    {
        flowerSpineObj[index].gameObject.SetActive(isActive);
        if (index > 0)
            flowerGauge[index -1].gameObject.SetActive(isActive);
    }

    private void SetPositionFlowerScore(params float[] ratios)
    {
        for (int i = 0; i < ratios.Length; i++)
        {
            flowerSpineObj[i].transform.localPosition = new Vector3(GAUGE_WIDTH * ratios[i], 10, 0);
            flowerGauge[i].transform.localPosition    = new Vector3(GAUGE_WIDTH * ratios[i], -2, 0);
        }
        flowerSpineObj[ratios.Length].transform.localPosition = new Vector3(GAUGE_WIDTH, 10, 0);
    }

    void SetUI()
    {
        targetLabel[0].text = Global._instance.GetString("p_tag_1");
        targetLabel[1].text = Global._instance.GetString("p_tag_1");

        targetIcon.spriteName = "ingame_icon_target";
        targetIcon.MakePixelPerfect();

        if (listGameTarget.Count > 0)
        {
            foreach (GameMissionTarget item in listGameTarget)
            {
                Destroy(item.gameObject);
            }
            listGameTarget.Clear();
        }

        if (listUiObj.Count > 0)
        {
            foreach (GameObject item in listUiObj)
            {
                Destroy(item.gameObject);
            }
            listUiObj.Clear();
        }

        //목표 UI 설정
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

                    GameMissionTarget target = NGUITools.AddChild(Target_Root, targetObj).GetComponent<GameMissionTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    string collectCount = e.Current.Value.collectCount.ToString();
                    target.targetCount.text = collectCount;
                    target.targetCountShadow.text = collectCount;

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;

                    //이미지 크기 및 체크 설정
                    target.targetSprite.MakePixelPerfect();
                    target.targetSprite.cachedTransform.localScale = Vector3.one * 0.75f;

                    listGameTarget.Add(target);
                    listUiObj.Add(target.gameObject);
                }
            }
        }

        moveCountLabel.text = GameManager.instance.moveCount.ToString();
        moveCountLabelShadow.text = GameManager.instance.moveCount.ToString();

        float startPos = (1 - listGameTarget.Count) * 48;
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            listGameTarget[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }

        targetBG.width = 302;
        GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(152, 431, 0);

        if (listGameTarget.Count == 3) targetBG.width = 340;
        else if (listGameTarget.Count == 4) targetBG.width = 440;
        else if (listGameTarget.Count == 5)
        {
            targetBG.width = 530;
            GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(130, 431, 0);
        }
        else if (listGameTarget.Count > 5)
        {
            startPos = (1 - listGameTarget.Count) * 40;
            for (int i = 0; i < listGameTarget.Count; i++)
            {
                listGameTarget[i].transform.localPosition = new Vector3(startPos + 80 * i, 0, 0);
            }
            targetBG.width = 540;
            GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(130, 431, 0);
        }

        InitTargetUICount();
    }

    //목표 UI 카운트 초기화.
    private void InitTargetUICount()
    {
        listTargetUICount.Clear();
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            listTargetUICount.Add(0);
        }
    }

    //목표 UI 카운트 변경시킬 때 사용하는 함수.
    public void RefreshTarget(TARGET_TYPE targetType, BlockColorType targetColor = BlockColorType.NONE, int addCount = 1)
    {
        if (Global.GameInstance.IsStageTargetHidden())
            return;

        if (GameManager.gameMode == GameMode.ADVENTURE || GameManager.gameMode == GameMode.COIN)
            return;

        ManagerBlock.CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(targetType, targetColor);
        if (collectTargetCount != null)
        {
            int findIndex = listGameTarget.FindIndex(x => x.targetType == targetType && x.targetColor == targetColor);
            if (findIndex == -1)
                return;

            listTargetUICount[findIndex] += addCount;
            int count = collectTargetCount.collectCount - listTargetUICount[findIndex];
            listGameTarget[findIndex].ShowChangeCount(count);
            showCount(findIndex, count);
        }
    }

    void showCount(int tempIndex, int tempCount)
    {
        listGameTarget[tempIndex].targetCount.text = tempCount.ToString();
        listGameTarget[tempIndex].targetCountShadow.text = tempCount.ToString();

        if (tempCount <= 0)
        {
            listGameTarget[tempIndex].targetCount.gameObject.SetActive(false);
            listGameTarget[tempIndex].targetCountShadow.gameObject.SetActive(false);
            listGameTarget[tempIndex].checkSprtie.gameObject.SetActive(true);
        }
    }

    public void RefreshTargetAll()
    {
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            TARGET_TYPE targetType = listGameTarget[i].targetType;
            BlockColorType targetColor = listGameTarget[i].targetColor;

            ManagerBlock.CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(targetType, targetColor);
            if (collectTargetCount != null)
            {
                int count = (collectTargetCount.collectCount > collectTargetCount.pangCount) ?
                    (collectTargetCount.collectCount - collectTargetCount.pangCount) : 0;
                showCount(i, count);
            }
        }
    }

    public void RefreshMove()
    {
        ShowEffectAddApple();

        moveCountLabel.text = GameManager.instance.moveCount.ToString();
        moveCountLabelShadow.text = GameManager.instance.moveCount.ToString();

        if (IsWarningsRemainingTurns() == true)
            StartCoroutine(CoWarningsRemainingTurnsAction());
    }

    public void RefreshMoveColor(float addTime = 0f)
    {
        moveCountLabel.text = GameManager.instance.moveCount.ToString();
        moveCountLabelShadow.text = GameManager.instance.moveCount.ToString();

        StartCoroutine(CoAddAppleColor(addTime));
    }

    public void SetIceCount()
    {
        //turnIceSpirte.gameObject.SetActive(true);
        //turnIceSpirte.spriteName = "DecoIce2";
        InGameEffectMaker.instance.MakeIceShineEffect2(turnSpirte.gameObject.transform.position);
        InGameEffectMaker.instance.MakeIceShineEffect1(turnSpirte.gameObject.transform.position);

        GameManager.instance.moveCount = 0;
        RefreshMove();

        turnSpirte.spriteName = "ingame_item_lava2";
        turnSpirte.MakePixelPerfect();
        //turnShadowSpirte.spriteName = "ingame_item_lava";
    }

    public void DisCountIce(int iceCount)
    {
        if(iceCount > 1)
        {
            turnSpirte.spriteName = "ingame_item_lava2";
            turnSpirte.MakePixelPerfect();                
        }
        else if(iceCount == 0)
        {
            turnSpirte.spriteName = "ingame_item_lava";
            turnSpirte.MakePixelPerfect();
        }
        else
        {
            turnSpirte.spriteName = "ingame_item_lava1";
            turnSpirte.MakePixelPerfect();
        }

        //이펙트
        if (iceCount < 2) 
            InGameEffectMaker.instance.MakeICeEffect(turnSpirte.gameObject.transform.position);
    }

    #region 턴 증가 연출 관련
    //말풍선과 함께 턴 증가 연출이 뜨는 연출
    public IEnumerator CoActionAddTurn_WithMakeBubble(int addTurnCount)
    {
        //말풍선 설정
        moveCountAddTurnBubble.SetActive(true);
        moveCountAddTurnLabel.text = string.Format("+{0}", addTurnCount);

        //빛 설정
        turnLightSprite.gameObject.SetActive(true);        
        turnLightSprite.transform.localScale = Vector3.one * 0.1f;

        //턴 이미지 설정
        turnSpirte.transform.localScale = Vector3.one * 0.5f;

        float actionTime = ManagerBlock.instance.GetIngameTime(0.4f);

        //말풍선 커지는 연출
        moveCountAddTurnBubble.transform.localScale = Vector3.one * 0.5f;
        moveCountAddTurnBubble.transform.DOScale(1f, actionTime).SetEase(Ease.OutBack);

        //빛 회전 연출
        turnLightSprite.transform.DORotate(new Vector3(0f, 0f, 200f), actionTime * 2f);
        turnLightSprite.transform.DOScale(1f, actionTime);

        //턴 이미지 커지는 연출
        turnSpirte.transform.DOScale(1.2f, actionTime).SetEase(Ease.OutBack);

        //연출 종료까지 대기
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.2f);

        //빛 이미지 사라지는 연출
        turnLightSprite.transform.DOScale(0.1f, actionTime);

        //턴 이미지 원래 사이즈로 돌아가는 연출
        turnSpirte.transform.DOScale(1.0f, actionTime).SetEase(Ease.OutBack);

        //연출 종료까지 대기
        yield return new WaitForSeconds(actionTime);

        turnLightSprite.gameObject.SetActive(false);
        turnSpirte.transform.localScale = Vector3.one;

        //증가할 턴 카운트 컬러 변경
        Color originColor = moveCountLabel.color;
        moveCountLabel.color = new Color32(0xff, 0xe9, 0x32, 0xff);

        //턴 카운트 증가 연출
        yield return CpActionAddTurn(addTurnCount);

        //턴 카운트 설정
        GameManager.instance.moveCount += addTurnCount;

        string turnText = GameManager.instance.moveCount.ToString();
        moveCountLabel.text = turnText;
        moveCountLabelShadow.text = turnText;
        moveCountLabel.color = originColor;

        //말풍선 사라지는 연출
        actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        moveCountAddTurnBubble.transform.DOScale(0.1f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
        moveCountAddTurnBubble.SetActive(false);
    }

    //턴 숫자 증가 연출
    private IEnumerator CpActionAddTurn(int addTurn)
    {
        UILabel[] labels = new UILabel[] { moveCountLabel, moveCountLabelShadow };
        int startCount = GameManager.instance.moveCount;
        yield return CpActionAddTurn(labels, startCount, startCount + addTurn);
    }

    //숫자 증가 연출
    private IEnumerator CpActionAddTurn(UILabel[] labetText, int startCount, int totalCount, int maxActionCount = 5)
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.1f);

        //더할 량
        int addTurn = totalCount - startCount;

        //연출이 등장할 횟수
        int actionCnt = (addTurn > maxActionCount) ? maxActionCount : addTurn;

        //연출 한 번당 증가할 카운트
        int count = (addTurn > maxActionCount) ? (addTurn / maxActionCount) : 1;

        //턴 카운트 증가 연출
        for (int i = 0; i < actionCnt; i++)
        {
            int addCount = count;

            //맨 마지막 카운트는 나머지까지 전부 합산한 카운트.
            if (addTurn > maxActionCount && (i + 1) >= actionCnt)
                addCount += addTurn % maxActionCount;

            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
            startCount += addCount;
            string turnText = startCount.ToString();
            foreach (var item in labetText)
                item.text = turnText;

            labetText[0].transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);
            yield return new WaitForSeconds(actionTime);
        }
    }

    public void ShowEffectAddApple()
    {
        StartCoroutine(CoAddApple());
    }

    float waitTimer = 0f;
    const int CURVE_SPPED = 3;

    IEnumerator CoAddApple()
    {
        waitTimer = 0;

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            moveCountLabel.transform.localScale = Vector3.one * ((1 + (ratio-1)*0.5f) * 0.8f);
            yield return null;
        }
    }

    IEnumerator CoAddAppleColor(float addTime = 0f)
    {
        moveCountLabel.color = new Color32(0xff, 0xe9, 0x32, 0xff);

        waitTimer = 0;

        while (waitTimer < (1f + addTime))
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            moveCountLabel.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f) * 0.8f);

            yield return null;
        }

        moveCountLabel.color = new Color32(0xff, 0xff, 0xff, 0xff);
    }
    #endregion

    public void OnClickBtnPause()
    {        
        if (SceneManager.GetActiveScene().name == "InGameTool") return;    
        if (GameItemManager.instance != null) return;
        if (GameManager.instance.state != GameState.PLAY) return;
        if (ManagerTutorial._instance != null) return;
        if (IsExistPopupPauseUI() == true) return;
        if (Input.touchCount > 0) return;

        if (Global.GameType == GameType.ADVENTURE)
        {
            string pauseStr = Global._instance.GetString("p_sm_1") + " " + Global.chapterIndex.ToString() + "-" + Global.stageIndex.ToString();
            if (Global.stageIndex > 100)
            {
                pauseStr = Global._instance.GetString("p_sm_1") + " C" + Global.chapterIndex.ToString() + "-" + (Global.stageIndex % 100).ToString();
            }

            listPopupPause.Add(ManagerUI._instance.OpenPopupAdventurePause(pauseStr, null).gameObject);
        }
        else if (Global.GameType == GameType.ADVENTURE_EVENT)
        {
            string pauseStr = Global._instance.GetString("p_sm_1") + " " + Global.stageIndex.ToString();
            listPopupPause.Add(ManagerUI._instance.OpenPopupAdventurePause(pauseStr, null).gameObject);
        }
        else
        {
            listPopupPause.Add(Global.GameInstance.OpenPopupPause());
        }
    }

    public void ShowFlower(bool enable)
    {
        //꽃 점수 사용하지 않는 모드에서는 제외.
        if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_INGAME))
            flowerGaugeRoot.SetActive(enable);
        if (Global.GameType == GameType.TREASURE_HUNT)
            treasureHuntScoreSprite.gameObject.SetActive(enable);
    }

    void OnClickBtnTip()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL) == false) return;
        if (ManagerGimmickTutorial.instance == null || ManagerGimmickTutorial.instance.IsExistGimmickTutorialData() == false) return;
        if (GameManager.instance.state != GameState.PLAY) return;
        if (GameItemManager.instance != null) return;

        gimmickTutorialPopup = ManagerUI._instance.OpenPopupGimmickTutorial();
    }

    public void ShowTipButton(bool isShow)
    {   //UIButtonAdventureSpeedUp 위치가 팁 버튼과 같으므로 주의
        if (isShow == true)
        {
            //튜토리얼이 출력될 수 없는 게임 타입인 경우 버튼 출력하지 않음.
            if (Global.GameInstance.GetProp(GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL) == false)
                return;

            //튜토리얼 버튼이 출력되어야할 때 데이터가 없다면 버튼 출력하지 않음.
            if (ManagerGimmickTutorial.instance == null || 
                ManagerGimmickTutorial.instance.IsExistGimmickTutorialData() == false) 
                return;
        }
        tipButton.SetActive(isShow);
    }

    public void ShowPauseButton(bool isShow)
    {
        pauseButton.SetActive(isShow);
    }

    bool showing5Left = false;

    public void ShowLeft5Turn()
    {
        if (EditManager.instance != null) return;

        if (showing5Left) return;

        showing5Left = true;

        bool isLive2DMake = birdLive2D == null;
        if (isLive2DMake)
            birdLive2D = LAppModelProxy.MakeLive2D(ManagerUI._instance.anchorCenter, TypeCharacterType.BlueBird, "count");

        birdLive2D.gameObject.SetActive(true);

        ManagerSound.AudioPlay(AudioInGame.TURN5_LEFT);
        birdLive2D.SetVectorScale(new Vector3(-1f, 1f, 1f) * 200f);
        birdLive2D.transform.localPosition = new Vector3(-358f, 415f, 0f);
        birdLive2D.SetSortOrder(10);

        if (!isLive2DMake)
            birdLive2D.SetAnimation("count", false);

        StartCoroutine(CoShowLeft5Turn());
    }


    public static LAppModelProxy birdLive2D = null;
    public List<GameObject> Liststatue = new List<GameObject>();//static



    IEnumerator CoShowLeft5Turn()
    {       
        float waitTimer = 0;
        while (waitTimer < 2.3f)
        {
            //if (birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("count") && birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) break;
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
         
        birdLive2D.gameObject.SetActive(false);
        showing5Left = false;
        yield return null;
    }

    public void SetDigGauge(float ratio)
    {
        float ratioY = Mathf.Lerp(digGradStart, digGradEnd, ratio);
        digGradation.transform.localPosition = new Vector3(13f, ratioY, 0);
    }

    public void ShakingCamera()
    {
        StartCoroutine(CoShakeCamera());
    }

    IEnumerator CoShakeCamera()
    {
        float factor = 1f;
        Vector3 value = Vector3.zero;

        while (factor > 0)
        {
            factor -= Global.deltaTimePuzzle;
            if (factor <= 1f)
            {
                float result = Mathf.Sin(2.0f * 3.14159f * factor * 7f) * 6f;
                value.x = result * (factor) / 1f;
                float result1 = Mathf.Sin(2.0f * 3.14159f * factor * 6f + 0.4f) * 5f;
                value.y = result1 * (factor) / 1f;
            }
            InGameCamera.transform.localPosition = value;
            yield return null;
        }        

        InGameCamera.transform.localPosition = Vector3.zero;
        yield return null;
    }

    /*
    public void SetAdventureGaige(float tempCount)
    {
        advantureGaigeSlider.value = tempCount;
        if (tempCount >= 1f && isChargeAdvantureGaige == false)
        {
            isChargeAdvantureGaige = true;
            StartCoroutine(DoAdvantureGaige());
        }
        else if (tempCount < 1f)
        {
            isChargeAdvantureGaige = false;
        }
    }
    
    IEnumerator DoAdvantureGaige()
    {
        //반짝반짝거리기
        while (isChargeAdvantureGaige)
        {
            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            advantureGaigeBomb.color = new Color(0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 1); //mainSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);            
            advantureGaigeBomb.cachedTransform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f) * 1.5f;
            yield return null;
        }
        advantureGaigeBomb.color = Color.white;
        advantureGaigeBomb.cachedTransform.localScale = Vector3.one *1.5f;
        yield return null;
    }
    */

    private List<InGameAnimal> animals;
    private List<InGameEnemy> enemys;
    public void AdventureEffectBG_On(List<InGameAnimal> animals, List<InGameEnemy> enemys)
    {
        if (adventureEffectBG.gameObject.activeSelf)
            return;

        this.animals = animals;
        this.enemys = enemys;

        if (animals != null)
        {
            foreach (var animal in animals)
                animal.ActiveAtBlind(true);
        }

        if (enemys != null)
        {
            foreach (var enemy in enemys)
                enemy.ActiveAtBlind(true);
        }

        adventureEffectBG.gameObject.SetActive(true);
    }

    public void AdventureEffectBG_Off()
    {
        if (!adventureEffectBG.gameObject.activeSelf)
            return;

        if (animals != null)
        {
            foreach (var animal in animals)
                animal.ActiveAtBlind(false);
        }

        if (enemys != null)
        {
            foreach (var enemy in enemys)
                enemy.ActiveAtBlind(false);
        }

        adventureEffectBG.gameObject.SetActive(false);

        animals = null;
        enemys = null;
    }

    bool showAdventureBG = false;
    public void ShowAdventureDarkBGBlock(bool tempShow)
    {
        if (showAdventureBG == tempShow)
            return;

        showAdventureBG = tempShow;
        StartCoroutine(DoShowAdventureDarkBlock(tempShow));
    }

    IEnumerator DoShowAdventureDarkBlock(bool show)
    {
        float showTimer = 0f;
        UISprite BGSprite = adventureDarkBGBlock.GetComponent<UISprite>();

            while (showTimer < 1f)
            {
                showTimer += Global.deltaTimePuzzle * 5;
                if (showTimer > 1f)
                    showTimer = 1f;

                if(show)
                BGSprite.alpha = 0.8f * showTimer;
                else
                BGSprite.alpha = 0.8f * (1 - showTimer);
                yield return null;
            }
        yield return null;
    }

    public void ShowSkipBtn(bool showBtn)
    {
        SkipBtn.SetActive(showBtn);
    }

    public void SkipStageClear()
    {
        if (GameManager.instance == null || GameManager.instance.SkipIngameClear || !SkipBtn.activeSelf)
            return;

        Global.timeScalePuzzle = 1000f;
        GameManager.instance.SkipIngameClear = true;

        GameManager.instance.SetClearBlind(false);
        ShowSkipBtn(false);
    }

    public void ShowInfo()
    {
        if(GameManager.gameMode == GameMode.ADVENTURE)
        {
            foreach(var animal in AdventureManager.instance.AnimalLIst)
            {
                animal.SelectHealAnimalByItem();
            }

            foreach (var enemy in AdventureManager.instance.EnemyLIst)
            {
                enemy.ShowEnemyInfo();
            }
        }
    }

    #region 코인스테이지
    public GameObject coinStageNpcRoot;
    public GameObject coinStageFriendsRoot;
    public GameObject _objCoinStageNpc;
    public GameObject _objCoinStageFriends;
    private SkeletonAnimation coinStageNpc = null;
    private SkeletonAnimation coinStageFriends = null;

    [SerializeField]
    List<GameObject> CoinStageUI = new List<GameObject>();  //코인스테이지용 UI

    [SerializeField]
    List<GameObject> NonCoinStageUI = new List<GameObject>();  //일반스테이지용 UI
  
    [SerializeField]
    List<GameObject> CoinStageFeverUI = new List<GameObject>();     //피버시 나타나는UI

    [SerializeField]
    List<GameObject> CoinStageFeverHideUI = new List<GameObject>(); //피버일때 숨기는 UI

    [SerializeField]
    List<UISprite> FeverColorChangeUI = new List<UISprite>(); //피버일때 숨기는 UI
    [SerializeField]
    UITexture CoinBG;

    [SerializeField]
    List<GameObject> FeverMovingObjList = new List<GameObject>();   //움직이는 동전오브젝트
    [SerializeField]
    AnimationCurve feverCoinCurve;
    float coinMoveHeight = 20f;
    float kuniDrawScale = 1260f;
    float kuniDistance = 120f;
    
    public UISprite feverBlockColor;
    
    [SerializeField]
    UISprite feverBlockGaige;

    [SerializeField]
    GameObject KuinObj;
    [SerializeField]
    GameObject KuinRoot;
    List<GameObject> KuniList = new List<GameObject>();

    //미러볼
    [SerializeField]
    List<GameObject> mirrorBallRootList = new List<GameObject>();  //미러볼
    [SerializeField]
    List<UISprite> mirrorBallList = new List<UISprite>();  //미러볼
    [SerializeField]
    List<UISprite> mirrorBallEffectList = new List<UISprite>();  //미러볼이펙트

    public void SetCoinStage()
    {
        if (GameManager.gameMode != GameMode.COIN)
            return;

        //턴라벨수정
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_clock";
            turnShadowSpirte.spriteName = "ingame_item_clock";

            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
                label.enabled = false;

            moveCountLabel.transform.localPosition = new Vector3(-9.6f, 7f, 0);
            turnSpirte.transform.localPosition = new Vector3(-50f, 122f, 0);
            
            //턴, 목표 위치 옮기기
            Target_Root.transform.localPosition = new Vector3(228, 431, 0); //0.8
            Target_Root.transform.localScale = Vector3.one * 0.8f;
            turnUi.transform.localPosition = new Vector3(-246, 360, 0); //0.83
            //turnUi.transform.localScale = Vector3.one * 0.83f;
        }        

        //점수, 아이템
        foreach (var nonObj in NonCoinStageUI)
            nonObj.SetActive(false);

        //피버게이지, 코인배경
        foreach (var coinObj in CoinStageUI)
            coinObj.SetActive(true);

        //피버타임 배경
        foreach (var feverObj in CoinStageFeverUI)
            feverObj.SetActive(false);

        feverBlockGaige.fillAmount = 0f;


        int kuniRatio = (int)(kuniDrawScale / kuniDistance);
        kuniDrawScale = (float)(kuniRatio * kuniDistance);

        foreach(var tmp in KuniList)
            Destroy(tmp);
        KuniList = new List<GameObject>();
        for (int i = 0; i < kuniRatio; i++)
            for (int j = 0; j < kuniRatio; j++)
            {
                UISprite kuni = NGUITools.AddChild(KuinRoot, KuinObj).GetComponent<UISprite>();
                kuni.depth = -3;

                if(j % 2 == 0)
                    kuni.gameObject.transform.localPosition = new Vector3(-kuniDistance * i, -kuniDistance * j, 0);
                else
                    kuni.gameObject.transform.localPosition = new Vector3(-kuniDistance * i + kuniDistance * 0.5f, -kuniDistance * j, 0);

                if ((i + j) % 2 == 0)
                {
                    kuni.spriteName = "bg_coin";
                    kuni.MakePixelPerfect();

                    TweenScale tween = kuni.gameObject.GetComponent<TweenScale>();
                    tween.from = Vector3.one * 1.2f;
                    tween.to = Vector3.one * 0.8f;
                }
                KuniList.Add(kuni.gameObject);
            }   
    }

    public void InitCoinStage()
    {
        if (coinStageNpc == null)
        {
            coinStageNpc = NGUITools.AddChild(GameUIManager.instance.coinStageNpcRoot, _objCoinStageNpc).GetComponent<SkeletonAnimation>();
        }
        coinStageNpc.transform.localScale = Vector3.one * 130f;
        if (EditManager.instance == null)
            coinStageNpc.gameObject.SetActive(false);

        if (coinStageFriends == null)
        {
            coinStageFriends = NGUITools.AddChild(GameUIManager.instance.coinStageFriendsRoot, _objCoinStageFriends).GetComponent<SkeletonAnimation>();
        }
        coinStageFriends.transform.localScale = Vector3.one * 100f;
        coinStageFriends.gameObject.SetActive(false);
    }

    public void SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState state)
    {
        switch (state)
        {
            case ManagerBlock.CoinStageNpcState.APPEAR:
                coinStageNpc.gameObject.SetActive(true);
                coinStageNpc.state.SetAnimation(0, "0_appear", false);
                coinStageNpc.state.AddAnimation(0, "1_idle", true, 0f);
                coinStageNpc.Update(0f);
                break;
            case ManagerBlock.CoinStageNpcState.IDLE:
                coinStageNpc.state.SetAnimation(0, "2_base_dance", true);
                break;
            case ManagerBlock.CoinStageNpcState.Fever:
                coinStageNpc.state.SetAnimation(0, "3_fever_dance", true);
                break;
            case ManagerBlock.CoinStageNpcState.Clear:
                coinStageNpc.state.SetAnimation(0, "4_clear_appear", false);
                coinStageNpc.state.AddAnimation(0, "4_clear_idle", true, 0f);
                break;
        }
    }

    public void SetCoinStageNpcFriend(bool isFeverMode)
    {
        if (isFeverMode == true)
        {
            coinStageFriends.gameObject.SetActive(true);
            coinStageFriends.state.SetAnimation(0, "4_fever_npc_appear", false);
            coinStageFriends.state.AddAnimation(0, "4_fever_npc_dance2", true, 0f);
        }
        else
        {
            coinStageFriends.gameObject.SetActive(false);
        }
    }

    public void SetFeverMode(bool isOn)
    {
        if (!isOn)
        {
            SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.IDLE);
            foreach (var feverObj in CoinStageFeverUI)
                feverObj.SetActive(isOn);

            foreach (var feverObj in CoinStageFeverHideUI)
                feverObj.SetActive(!isOn);

            foreach (var tmp in FeverMovingObjList)
                tmp.transform.localPosition = Vector3.zero;
            grassRoot.transform.DOLocalMoveY(0f, 0.2f);
        }
        else
        {
            SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.Fever);
            if (mirrorBallOn == false)
            {
                mirrorBallOn = true;
                StartCoroutine(ShowFeverColorChagne());
            }
            grassRoot.transform.DOLocalMoveY(-200f, 0.2f);
        }
        SetCoinStageNpcFriend(isOn);
    }

    bool mirrorBallOn = false;
    IEnumerator ShowFeverColorChagne()
    {
        float timer = 0f;
        foreach (var tmp in mirrorBallRootList)
            tmp.SetActive(true);

        StartCoroutine(CoMirrorballDown());
        yield return new WaitForSeconds(0.1f);

        foreach (var feverObj in CoinStageFeverUI)
            feverObj.SetActive(true);

        foreach (var feverObj in CoinStageFeverHideUI)
            feverObj.SetActive(true);

        while (ManagerBlock.instance.isFeverTime())
        {
            timer += Global.deltaTimePuzzle;
            float color = ((Mathf.Sin(timer * Mathf.PI * 6) + 1) * 0.5f) *0.25f + 0.75f;

            foreach (var tmp in FeverColorChangeUI)
                tmp.color = new Color(color, color, color, 1f);

            CoinBG.color = new Color(color, color, color, 1f);
            yield return null;
        }

        timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 4;
            float posY = Mathf.Lerp(740f, 1100f, timer);
            foreach (var tmp in mirrorBallRootList)
                tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x, posY);
            yield return null;
        }

        foreach (var tmp in FeverColorChangeUI)
            tmp.color = Color.white;
        CoinBG.color = Color.white;

        foreach (var tmp in mirrorBallRootList)
            tmp.SetActive(false);

        mirrorBallOn = false;
        yield return null;
    }

    private IEnumerator CoMirrorballDown()
    {
        yield return new WaitForSeconds(0.3f);
        float targetPosY = 670f;
        for (int i = 0; i < mirrorBallRootList.Count; i++)
        {
            mirrorBallRootList[i].transform.DOLocalMoveY(targetPosY, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void SetFeverGaige(float ratio, float color = 1f)
    {
        feverBlockGaige.fillAmount = ratio;
        feverBlockGaige.color = new Color(color, color, color, color);

        if (ManagerBlock.instance.isFeverTime())
        {
            foreach (var tmp in FeverMovingObjList)
            {
                float timeRatio = (ManagerBlock.instance.BlockTime * 2) % 1;
                float MoveRatio = feverCoinCurve.Evaluate(timeRatio);
                tmp.transform.localPosition = Vector3.up * coinMoveHeight * MoveRatio;
            }

            foreach(var tmp in KuniList)
            {
                tmp.transform.localPosition += (Vector3.down + Vector3.left)*2;

                if (tmp.transform.localPosition.x <= -kuniDrawScale)
                    tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x + kuniDrawScale, tmp.transform.localPosition.y);
                if (tmp.transform.localPosition.y <= -kuniDrawScale)
                    tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x, tmp.transform.localPosition.y + kuniDrawScale);               
            }

            int MirrorTimeRatio = (int)((ManagerBlock.instance.BlockTime * 6) % 4 + 1);
            foreach(var tmp in mirrorBallList)            
                tmp.spriteName = "Mirror_ball0" + MirrorTimeRatio;

            int MirrorTimeRatioE = (int)((ManagerBlock.instance.BlockTime * 3) % 3 + 1);
            foreach(var tmp in mirrorBallEffectList)
                tmp.spriteName = "Mirror_ball_light0" + MirrorTimeRatioE;            
        }
    }

    private bool IsWarningsRemainingTurns()
    {
        if (GameManager.gameMode != GameMode.COIN)
            return false;
        if (GameManager.instance.moveCount > 10)
            return false;
        else
            return true;
    }

    private IEnumerator CoWarningsRemainingTurnsAction()
    {
        int flipCount = (int)(2f + (10 - GameManager.instance.moveCount) * 0.5f);
        float interval = 1.0f / (float) (flipCount * 2);

        for( int i = 0; i < flipCount; ++i)
        {
            moveCountLabel.color = warningColor_turn;
            moveCountLabel.effectColor = warningEffectColor_turn;
            yield return new WaitForSeconds(interval);
            moveCountLabel.color = Color.white;
            moveCountLabel.effectColor = defaultEffectColor_turn;
            yield return new WaitForSeconds(interval);
        }
        
    }
    #endregion

    #region 엔드 컨텐츠

    public void SetEndContents()
    {
        if (Global.GameType == GameType.END_CONTENTS)
        {
            if (collectItemCountAction != null)
            {
                StopCoroutine(collectItemCountAction);
                collectItemCountAction = null;
            }
            materialEventRoot.SetActive(true);
            if (ManagerEndContentsEvent.instance != null)
            {
                if (ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas != null)
                {
                    collectItemSprite.atlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas;
                    collectItemSprite.spriteName = "endContents2";
                }
            }
            collectItemSprite.gameObject.SetActive(true);
            materialEventLabel.text = "0";
        }
    }
    
    public void CollectAction_EndContentsItem(int itemCount)
    {
        //이전에 진행되던 연출 있으면 멈추기.
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemSprite.MakePixelPerfect();
        }

        //재료 카운트 표시 마지막으로 획득한 카운트까지 갱신해줌.
        materialEventLabel.text = ManagerBlock.instance.endContentsItemCount.ToString();

        //연출에 필요한 카운트는 따로 저장.
        int actionCount = ManagerBlock.instance.endContentsItemCount;
        if (itemCount > 8)
        {
            actionCount += (itemCount - 8);
            itemCount = 8;
        } 
        
        //실제 카운트는 따로 올려주고 연출 진행.
        ManagerBlock.instance.endContentsItemCount += itemCount;
        collectItemCountAction = StartCoroutine(CoCollectAction_EndContents(actionCount, itemCount));
    }

    private IEnumerator CoCollectAction_EndContents(int actionCount, int itemCnt)
    {
        for (int i = 0; i < itemCnt; i++)
        {
            actionCount++;
            materialEventLabel.text = actionCount.ToString();
            StartCoroutine(CoChangeCount(collectItemSprite.transform));
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    #region 빙고 이벤트

    void SetBingoEvent()
    {
        if (Global.GameType == GameType.BINGO_EVENT)
        {
            objBingoEventImage.SetActive(true);
            objBingoEventImage.GetComponentInChildren<UISprite>().atlas =
                ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasIngame;
        }
        else
        {
            objBingoEventImage.SetActive(false);
        }
    }
    #endregion

    #region 보물찾기 이벤트

    public void SetTreasure()
    {
        if (Global.GameType == GameType.TREASURE_HUNT)
        {
            treasureHuntScoreSprite.gameObject.SetActive(true);
            treasureHuntScoreSprite.atlas = ManagerTreasureHunt.instance.treasureHuntPack.IngameAtlas;
            treasureHuntScoreSprite.color = Color.gray;
            treasureHuntLightSprite.atlas = ManagerTreasureHunt.instance.treasureHuntPack.IngameAtlas;
            treasureHuntCharSprite.gameObject.SetActive(true);
            treasureHuntCharSprite.atlas = ManagerTreasureHunt.instance.treasureHuntPack.IngameAtlas;
            foreach (var item in flowerGauge)
                item.SetActive(false);
            foreach (var item in flowerSpineObj)
                item.gameObject.SetActive(false);
        }
    }
    
    #endregion

    #region 우주여행 이벤트

    public void SetSpaceTravel()
    {
        if (Global.GameType == GameType.SPACE_TRAVEL)
        {
            gameItemRoot_spaceTravel.SetActive(true);
            coinRoot.SetActive(false);

            //게임 아이템 값 초기화
            for (var i = 0; i < listGameItem_spaceTravel.Count; i++)
                listGameItem_spaceTravel[i].InitGameItem();
        }
    }
    
    public IEnumerator CoActionItemAddCount_SpaceTravel(ManagerSpaceTravel.BonusItemType itemType)
    {
        var findIndex = listGameItem_spaceTravel.FindIndex(x => x._itemType == itemType);
        if (findIndex == -1)
        {
            yield break;
        }

        //게임 아이템 카운트 증가 연출
        yield return listGameItem_spaceTravel[findIndex].CoAction_AppearGameItem();
    }

    #endregion

    #region 퍼즐 명화 이벤트

    public void SetAtelier()
    {
        if (Global.GameType == GameType.ATELIER)
        {
            objAtelierRoot.SetActive(true);
            var sprites = objAtelierRoot.GetComponentsInChildren<UISprite>();

            foreach (var spr in sprites)
            {
                spr.atlas = ManagerAtelier.instance._atelierInGamePack.InGameAtlas;
            }
        }
    }

    #endregion
    
    private void OnApplicationPause(bool pause)
    {
        if (pause == false) //백그라운드에서 돌아왔을 때 호출
        {
            if (GameManager.instance.state != GameState.PLAY || ManagerUI._instance._popupList.Count > 0)
            {
                return;
            }
            else
            {
                OnClickBtnPause();
            }
        }
    }

    private bool IsExistPopupPauseUI()
    {
        int pCount = listPopupPause.Count;
        if (pCount == 0)
        {
            return false;
        }
        else
        {
            for (int i = pCount - 1; i >= 0; i--)
            {   //혹시라도 오브젝트는 삭제되었지만 리스트에 남아있는 팝업이 있다면 제거
                if (listPopupPause[i] == null)
                    listPopupPause.RemoveAt(i);
            }
            return (listPopupPause.Count > 0);
        }
    }
}
