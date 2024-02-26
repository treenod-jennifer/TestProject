using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Pathfinding.Util;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Protocol;
using Newtonsoft.Json;
using PokoAddressable;
using UnityEngine.Networking;

[IsReadyPopup]
public class UIPopupReady : UIPopupBase
{
    public class EventStageFlowerData
    {
        public int prevLevel = 0;
        public int currentLevel = 0;
    }

    public static UIPopupReady _instance = null;

    //취소(닫기)버튼 눌렀을 때 실행될 액션
    public System.Action readyCancelAction = null;

    public const int OPEN_ADD_TURN_STAGE = 6;
    public const int OPEN_SCORE_UP_STAGE = 23;
    public const int OPEN_RANDOM_BOMB = 9;

    public const int OPEN_LINE_BOMB_STAGE = 6;
    public const int OPEN_CIRCLE_BOMB_STAGE = 6;
    public const int OPEN_RAINBOW_BOMB_STAGE = 6;

    public const int OPEN_DOUBLE_ADD_TURN_STAGE = 23;
    public const int OPEN_DOUBLE_SCORE_UP_STAGE = 23;

    public const int SERVER_DOUBLEREADY_INDEX = 0;

    public UIPanel clippingPanel;

    #region 게임 시작 버튼 관련
    public Transform startBtnTr;
    public UILabel[] labelBtnStage;
    public UISprite sprBtnStart;
    public UISprite startClover;
    public GameObject startCloverShadow;
    #endregion

    public UILabel[] stage;

    public Transform targetTr;
    
    public GameObject hardStageObj;

    //꽃 배경 오브젝트
    public GameObject flowerBGObj;

    public UITexture[] flowerBack;

    public UITexture flower;
    public UITexture flowerEmoticon;
    public UISprite flowerShadow;

    public GameObject noyBoostingEventRoot;
    public UILabel noyBoostingStepText;
    public UIUrlTexture noyBoostingEventTexture;

    //보니 대화창.
    public UIItemReadyBubble dialogBubble;

    //챕터미션
    public UISprite CandySprite;

    //private List<UIItemTarget> listTargets = new List<UIItemTarget>();
    private int listCracks = 0;
    private List<int> listAnmimals = new List<int>();
    private int animalCrackCount = 0;
    private int keyCount = 0;

    public GameObject readyBox;
    //public static XMLStage tempData = null;
    public GameObject _objEFfectUseClover;
    public GameObject _objEffectStartButton;
    public GameObject _objEffectCloverSprite;
    public GameObject _objEffectButton;
    public GameObject _objRingGlow;

    //목표표시
    public GameObject targetRoot;
    protected List<StageTarget> listTargets = new List<StageTarget>();
    protected StageMapData tempData;

    //레디아이템
    public GameObject readyItemRoot;
    public List<ReadyItem> listReadyItem = new List<ReadyItem>();

    //게임 실행할 때, 실제로 사용한 갯수 //인게임 적용 개수
    public static EncValue[] readyItemUseCount = new EncValue[8];
    //아이템 선택됐는지 확인
    public static int[] readyItemSelectCount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

    //유저가 가진 레디 아이템을 소모하지 않고 자동으로 사용되는 레디 아이템인지 저장(시간제 아이템 등). //더블 제외
    public static EncValue[] readyItemAutoUse = new EncValue[6];
    //시간제한 걸려있는 레디 아이템인지(레디창이 켜진 시점에 초기화됨) //더블 제외
    protected bool[] readyItemTimeLimit = new bool[] { false, false, false, false, false, false };

    #region 광고 관련
    //턴 추가 광고 관련
    protected bool isCanWatchAD_AddTurn = false;  //현재 스테이지에서 광고볼 수 있는지
    protected bool isSelectAD_AddTurn = false;    //광고 보기 선택했는지

    [SerializeField] protected GameObject objSelectADRoot_AddTurn;
    [SerializeField] protected GameObject[] arrADIconRoot_AddTurn;
    [SerializeField] protected UILabel labelADTurnCount_AddTurn;
    [SerializeField] protected UISprite sprADCheck_AddTurn;
    [SerializeField] protected UISprite sprADBird_AddTurn;
    [SerializeField] protected UISprite sprStartADIcon_AddTurn;
    protected Coroutine ADAddturn_birdRoutine = null; //파랑새 연출 루틴
    protected Coroutine ADAddturn_bubbleRoutine = null;   // 광고 말풍선 연출 루틴

    //광고로 획득한 턴
    public static EncValue addTurnCount_ByAD = new EncValue();
    #endregion

    //이벤트 보상 관련.
    #region 연속/순차모드 보상
    public UISprite rewardBox;
    public GenericReward[] rewards;
    public UILabel[] rewardText;
    public Transform rewardIconRoot;
    #endregion

    #region 스코어 모드 보상
    public GameObject rewardScoreRoot;
    public UIItemRewardBubble[] scoreRewareBubble;
    #endregion

    [HideInInspector]
    public LAppModelProxy boniLive2D;

    //튜토리얼에서 버튼 눌러졌는지/레디 아이템에서 게임시작했는지 확인하는데 사용됨.
    [HideInInspector]
    public bool bStartGame = false;

    //이벤트에서 사용하는 오브젝트.
    public ReadyEvent readyEventObj;

    static public int stageIndex = 0;
    protected bool _recvGameStart_end = false;

    public static bool eventStageClear = false;
    public static bool eventGroupClear = false;
    public static bool eventStageFail = false;
    public static bool beforeStage = false;
    public static Dictionary<int, EventStageFlowerData> dicEventStageFlower = new Dictionary<int, EventStageFlowerData>();

    #region 스코어 모드 데이터
    //연출 데이터
    private bool canPlayAction_CompleteGetReward = false;

    private int getScoreBadgeCount_Prev = 0;
    private int getScoreBadgeCount_Current = 0;

    private int scoreRewardState = 0;
    private int scoreRewardIndex = 0;
    #endregion

    //이벤트 연출이 끝나고 불릴 콜백
    private System.Action actionEventActionEnd = null;

    //이벤트 스테이지.
    CdnEventChapter eventChapter = null;
    int eventGroup = 0;
    int eventStep = 0;
    bool bFreeStage = false;

    public static bool isFreeStage = true;

    [SerializeField]
    GameObject modeObjectRoot = null;
    [SerializeField]
    UILabel modeInfoText = null;

    //코인이벤트
    public GameObject coinEventRoot;
    public UILabel coinEventLabel;

    //스페셜이벤트
    public UIUrlTexture specialEventUrlTexture;

    //알파벳 이벤트
    public UIUrlTexture alphabetEventTexture;
    
    //엔티크 스토어 이벤트
    public UIUrlTexture antiqueStoreEventTexture;

    //꽃피우기 이벤트
    public UIUrlTexture blossomEventUrlTexture;

    //개편된 에코피 이벤트
    public UIUrlTexture pokoFlowerEventUrlTexture;
    
    // 코코의 수사일지
    public UIUrlTexture criminalEventUrlTexture;

    //그룹랭킹
    public UIUrlTexture _groupRankingEventTexture;
    
    //이벤트 아이콘 정렬
    public UIGrid gridEventIcon;

    //두더지잡기 이벤트
    public GameObject moleCatchStageSign;
    public UILabel moleCatchStageText;

    //레디 팝업 상단 이미지 바꿀 때 사용
    public UITexture changeBGTexture;

    //레디 팝업 상단 이미지에 들어갈 텍스트에 사용
    public UILabel labelBGTexture;
    
    //월드랭킹 1스테이지에서 상점 아이콘 표시에 사용
    public GameObject worldRankShopObj;

    //이벤트 스테이지에서 팝업창 이미지크기 변경에 사용
    public UISprite mainReadySprite;

    //상품판매 법률 개정 텍스트
    public GameObject objDescription;
    public Transform objMaskPanel;
    public UISprite sprSelectItemBox;
    public GameObject objItemBoxLabel;
    public GameObject objSelectItemRoot;
    
    //통상 스테이지 독려 이벤트
    public UIItemStageAssistMission stageAssistMission;

    //보니 말풍선 DOTween 관련.
    protected Sequence dialogSequence;

    //어려운 스테이지로 설정되어있는지 관련.
    private bool isSetHardStageUI = false;

    //현재 레디창에서 사용하고 있는 타겟 정보(타겟 아틀라스 조합 시 사용).
    protected List<CollectTargetInfo> listTargetInfo = new List<CollectTargetInfo>();
    
    //연속모드 광고 시청 후 별 위치 리셋을 위함
    private Vector2 starPosition;

    #region 연속모드 광고 지면
    public static int backupStageIndex = 0;
    private FailResetAdRewardResp _failResetAdRewardResp;
    
    private bool failResetAdOnCheckComplete = false;
    private bool blueBirdFailActionEnd = false;
    private bool onFailResetAd = false;
    private AdManager.AdType failResetAdType = AdManager.AdType.None;
    #endregion

    #region LAN 페이지 관련

    [SerializeField] protected UIItemLanpageButton lanpageButton;

    #endregion
    
    void Awake()
    {
        _instance = this;
    }

    public override void OpenPopUp(int _depth)
    {
        listTargetInfo.Clear();
        if (rewardBox != null)
            rewardBox.gameObject.SetActive(false);
        clippingPanel.depth = _depth + 1;

        _callbackOpen += () =>
        {
            Global.GameInstance.OnReadyOpenTutorial();
        };
        base.OpenPopUp(_depth);
    }

    protected override IEnumerator CoPostOpenPopup()
    {
        HasEventChapterAnimation();
        
        yield return new WaitForSeconds(openTime);
        
        if (IsFailResetAdOn())
        {
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = false;
            
            RequestFailResetAdAvailable();
            yield return new WaitUntil(()=> failResetAdOnCheckComplete);
            
            if (_callbackOpen != null)
            {
                _callbackOpen();
                yield return new WaitForSeconds(openTime);
            }
        }
        else
        {
            if (_callbackOpen != null)
            {
                _callbackOpen();
                yield return new WaitForSeconds(openTime);
            }
            
            ManagerUI._instance.bTouchTopUI = true;
            bCanTouch = true;
        }
        
        ManagerUI._instance.FocusCheck();
    }
    
    private void HasEventChapterAnimation()
    {
        if (Global.GameType != GameType.EVENT && eventStageClear)
        {
            return;
        }

        if (eventStageClear == false && eventStageFail == false)
        {
            return;
        }

        // 연속모드 제외 실패 연출이 없음, 연속모드 1 스테이지의 경우 연출이 없음
        if (eventStageFail == true)
        {
            if (ServerContents.EventChapters.type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
            {
                return;
            }

            int step  = ManagerData._instance._eventChapterData._state;
            int group = ManagerData._instance._eventChapterData._groupState;
            if (group > 1)
            {
                step -= ServerContents.EventChapters.counts[group - 2];
            }
            
            if (step == 1)
            {
                return;
            }
        }
        
        ClickBlocker.Make(Mathf.Infinity);
    }

    private IEnumerator CoDelayEventChapterActionCompleteFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClickBlocker.Make(0f);
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //레디창 전에 팝업들이 sortOrder을 사용하지 않는다면 live2d 쪽만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            clippingPanel.sortingOrder = layer + 2;
        }
        else
        {
            clippingPanel.sortingOrder = layer + 1;
        }
        clippingPanel.useSortingOrder = true;

        if (boniLive2D != null)
            boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    protected virtual void SetStageLabel()
    {
        string text = Global.GameInstance.GetStageText_ReadyPopup();

        stage[0].text = text;
        stage[1].text = text;

        stage[0].MakePixelPerfect();
        stage[1].MakePixelPerfect();
    }

    public virtual void InitPopUp(StageMapData stageData)
    {
        SetStageLabel();
        stageIndex = Global.stageIndex;
        
        //상품판매 법률 개정 텍스트
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
        
        //Lan 페이지 설정
        if(lanpageButton != null)
            lanpageButton.On($"LGPKV_bomb_random", Global._instance.GetString("p_sr_27"));
        
        if (LanguageUtility.IsShowBuyInfo)
        {
            mainReadySprite.height -= 50;
            sprSelectItemBox.spriteName = "stage_bg_01";
            sprSelectItemBox.height = 315;
            objItemBoxLabel.SetActive(false);
            objSelectItemRoot.transform.localPosition += new Vector3(0f, 60f, 0f);
            noyBoostingEventRoot.transform.localPosition = new Vector3(-230f, 240f, 0);
            objMaskPanel.transform.localPosition += new Vector3(0f, 60f, 0f);
            lanpageButton.transform.localPosition = new Vector3(350f, -565f, 0f);
        }

        //아이템선택상태 읽어오기
        if (PlayerPrefs.HasKey(Global.GameInstance.GetReadyItemSelectKey() + "0"))
        {
            for (int i = 0; i < 8; i++)
            {
                if (Global.GameInstance.CanUseReadyItem(i) == false)
                {

                    if(PlayerPrefs.GetInt(Global.GameInstance.GetReadyItemSelectKey() + i) == 1
                        && (i == 6 || i == 7))
                    {
                        //더블레디 체크 설정 되었으나, 더블레디를 못쓰는 레디창인 경우, 단일 체크 설정 켜기
                        int singleIndex = (i == 6) ? 0 : 1;

                        if (Global.GameInstance.CanUseReadyItem(singleIndex) == true)
                        {
                            //더블체크 제거
                            string prefsName_Double = Global.GameInstance.GetReadyItemSelectKey() + i;
                            PlayerPrefs.SetInt(prefsName_Double, 0);
                            readyItemSelectCount[i] = PlayerPrefs.GetInt(prefsName_Double);

                            //단일체크 추가
                            string prefsName_Single = Global.GameInstance.GetReadyItemSelectKey() + singleIndex;
                            PlayerPrefs.SetInt(prefsName_Single, 1);
                            readyItemSelectCount[singleIndex] = PlayerPrefs.GetInt(prefsName_Single);
                        }
                    }

                    continue;
                }

                string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                readyItemSelectCount[i] = PlayerPrefs.GetInt(prefsName);
            }
        }

        //게임 타입에 따른 레디 아이템 선택 설정.
        Global.GameInstance.SetSelectReadyItem();

        //클로버 버튼 이미지 설정.
        if (GameData.RemainFreePlayTime() > 0)
        {
            SettingFreeCloverButton();
        }

        tempData = stageData;
        AddListTargetInfo(tempData.listTargetInfo);
        LoadTarget();
        SetReadyItem(tempData.gameMode == (int)GameMode.LAVA);
        SetHardStage();
        SetStageFlower();
        SetSpecialEvent();
        SetAlphabetEvent();
        SetPokoFlowerEvent();
        SetMoleCatchEvent();
        SetCoinEvent();
        SetNoyBoostingEvent();
        SetStageAssistMissionEvent();
        SetAntiqueStoreEvent();
        SetCriminalEvent();
        SetGroupRankingEvent();
        
        //이벤트 아이콘 정렬
        gridEventIcon.Reposition();

        //광고 설정
        InitPopupReadyAD();

        //스타트 버튼 설정
        SetStartButton();

        if (Global.GameType != GameType.EVENT)
            SetBoniModel();

        //보니 대사 창.
        SettingBoniDialog();

        if (Global.GameType == GameType.NORMAL)
        {
            SetStageMission();
            modeObjectRoot.SetActive(false);
        }
        else if (Global.GameType == GameType.EVENT)
        {
            EVENT_CHAPTER_TYPE eventType = (EVENT_CHAPTER_TYPE)ServerContents.EventChapters.type;
            switch (eventType)
            {
                case EVENT_CHAPTER_TYPE.FAIL_RESET:
                    modeInfoText.text = Global._instance.GetString("p_sr_24");
                    break;
                case EVENT_CHAPTER_TYPE.COLLECT:
                    modeInfoText.text = Global._instance.GetString("p_sr_25");
                    break;
                case EVENT_CHAPTER_TYPE.SCORE:
                    modeInfoText.text = Global._instance.GetString("p_sr_26");
                    break;
            }
            modeObjectRoot.SetActive(true);
        }
        else
        {
            modeObjectRoot.SetActive(false);
        }

        Global.GameInstance.OnPopupInit_Ready(this);
    }

    public void SetCoinEvent()
    {
        if (Global.coinEvent > 0)
        {
            coinEventRoot.SetActive(true);
            coinEventLabel.text = "x" + Global.coinEvent;
            StartCoroutine(CoMoveCoinEventObject());
        }
        else
        {
            coinEventRoot.SetActive(false);
        }
    }

    IEnumerator CoMoveCoinEventObject()
    {
        float initPos = coinEventRoot.transform.localPosition.y;
        while (coinEventRoot.activeInHierarchy == true)
        {
            coinEventRoot.transform.localPosition
                = new Vector3(coinEventRoot.transform.localPosition.x, initPos + (Mathf.Cos(Time.time * 5f) * 3f), 0f);
            yield return null;
        }
        yield return null;
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
                        specialEventUrlTexture.gameObject.SetActive(true);
                        specialEventUrlTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventReady_" + Global.specialEventIndex);
                    }
                }
            }
        }
    }

    void SetAlphabetEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == false)
            return;

        if (ManagerAlphabetEvent.instance != null && ManagerAlphabetEvent.instance.IsCanPlayEvent_AtStage_RealTime(Global.stageIndex) == true)
        {
            alphabetEventTexture.gameObject.SetActive(true);
            alphabetEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "aEventReady_" + ManagerAlphabetEvent.instance.resourceIndex);
        }
    }

    void SetPokoFlowerEvent()
    {
        if (Global.GameType != GameType.NORMAL) return;

        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent() == false)
            return;

        //현재 챕터 클리어 상태 받아옴(0: 아직 챕터 덜깨짐, 1: 모두 흰꽃 이상, 2.모두 파란꽃 이상)
        int flowerClearState = 0;
        int curChapter = currentChapter();
        if (curChapter <= ServerRepos.UserChapters.Count - 1)
            flowerClearState = ServerRepos.UserChapters[curChapter].clearState;

        ScoreFlowerType prevStageFlower = (ScoreFlowerType)ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;
        ScoreFlowerType maxFlowerLevel = ScoreFlowerType.FLOWER_WHITE;

        if (prevStageFlower >= ScoreFlowerType.FLOWER_BLUE && flowerClearState > 1)
        { //해당 스테이지 파란 꽃 이상 피운 상태이고 현재 에피소드의 파란꽃을 전부 피웠는지
            maxFlowerLevel = ScoreFlowerType.FLOWER_RED;
        }
        else if (prevStageFlower >= ScoreFlowerType.FLOWER_WHITE)
        { //해당 스테이지 흰 꽃 이상 피운 상태일 때
            maxFlowerLevel = ScoreFlowerType.FLOWER_BLUE;
        }

        //현재 스테이지의 최대 레벨의 꽃을 피우지 않았다면 에코피 이벤트 아이콘 표시.
        if (prevStageFlower < maxFlowerLevel)
        {
            pokoFlowerEventUrlTexture.gameObject.SetActive(true);
            pokoFlowerEventUrlTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "bEventReady_" + ManagerPokoFlowerEvent.PokoFlowerEventResourceIndex);

            if (specialEventUrlTexture.gameObject.activeInHierarchy)
            {
                var orgPos = pokoFlowerEventUrlTexture.gameObject.transform.localPosition;
                orgPos.x -= 100;
                pokoFlowerEventUrlTexture.gameObject.transform.localPosition = orgPos;
            }

            if (alphabetEventTexture.gameObject.activeInHierarchy)
            {
                var orgPos = pokoFlowerEventUrlTexture.gameObject.transform.localPosition;
                orgPos.x -= 100;
                pokoFlowerEventUrlTexture.gameObject.transform.localPosition = orgPos;
            }

            if (antiqueStoreEventTexture.gameObject.activeInHierarchy)
            {
                var orgPos = pokoFlowerEventUrlTexture.gameObject.transform.localPosition;
                orgPos.x -= 100;
                pokoFlowerEventUrlTexture.gameObject.transform.localPosition = orgPos;
            }

            if (criminalEventUrlTexture.gameObject.activeInHierarchy)
            {
                var orgPos = pokoFlowerEventUrlTexture.gameObject.transform.localPosition;
                orgPos.x -= 100;
                pokoFlowerEventUrlTexture.gameObject.transform.localPosition = orgPos;
            }
            
            if (_groupRankingEventTexture.gameObject.activeInHierarchy)
            {
                var orgPos = pokoFlowerEventUrlTexture.gameObject.transform.localPosition;
                orgPos.x -= 100;
                pokoFlowerEventUrlTexture.gameObject.transform.localPosition = orgPos;
            }
        }
    }

    private void SetNoyBoostingEvent()
    {
        if (IsApplyNoyBoostingEvent() == false) return;
        
        noyBoostingEventRoot.SetActive(true);
        noyBoostingEventTexture.SettingTextureScale(noyBoostingEventTexture.width, noyBoostingEventTexture.height);
        noyBoostingEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "e_icon_noy_boost_star.png");
        noyBoostingStepText.text = UIPopupPause._instance == null ? "x" + ManagerNoyBoostEvent.instance.GetBoostStep() : noyBoostingStepText.text = "x1";
    }

    private void SetCriminalEvent()
    {
        if (Global.GameType != GameType.NORMAL)
            return;

        if (ManagerCriminalEvent.instance == null || ManagerCriminalEvent.CheckStartable() == false)
            return;

        if (ManagerCriminalEvent.instance.IsAllStageClear()) 
            return;
        
        if (ServerRepos.UserCriminalEvent.stages.Count > ManagerCriminalEvent.instance.GetEventStep_ServerData() && //ServerRepos.UserCriminalEvent.stages Argument에러 방지 조건 
            ServerRepos.UserCriminalEvent.stages[ManagerCriminalEvent.instance.GetEventStep_ServerData()] == stageIndex)
        {
            criminalEventUrlTexture.gameObject.SetActive(true);
            criminalEventUrlTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "cEventReady_" + ManagerCriminalEvent.instance.resourceIndex);

            List<GameObject> activeEventObjList = new List<GameObject>() { specialEventUrlTexture.gameObject, alphabetEventTexture.gameObject, pokoFlowerEventUrlTexture.gameObject };

            foreach (var item in activeEventObjList)
            {
                if (item.activeInHierarchy)
                {
                    var orgPos = criminalEventUrlTexture.gameObject.transform.localPosition;
                    orgPos.x -= 100;
                    criminalEventUrlTexture.transform.localPosition = orgPos;
                }
            }
        }
    }

    private void SetGroupRankingEvent()
    {
        if (Global.GameType != GameType.NORMAL)
        {
            return;
        }

        if (ManagerGroupRanking.IsGroupRankingStage() == false)
        {
            return;
        }

        if (GameData.User.stage != stageIndex)
        {
            return;
        }

        _groupRankingEventTexture.gameObject.SetActive(true);
        _groupRankingEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"grEventReady");
    }

    void SetStageAssistMissionEvent()
    {
        if (ManagerStageAssistMissionEvent.CheckStartable() && ManagerStageAssistMissionEvent.IsNewStageCheck())
        {
            stageAssistMission.gameObject.SetActive(true);
            stageAssistMission.SetStageAssist();
        }
        else
        {
            stageAssistMission.gameObject.SetActive(false);
        }
    }
    
    private void SetAntiqueStoreEvent()
    {
        if (ManagerAntiqueStore.CheckStartable() && ServerRepos.User.stage == Global.stageIndex && 
            Global.GameInstance.GetProp(GameTypeProp.IS_EVENT) == false)
        {
            antiqueStoreEventTexture.gameObject.SetActive(true);
            antiqueStoreEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", 
                $"asEventReady_{(ManagerAntiqueStore.IsSpecialEventCheck() ? "2" : "1")}_{ServerContents.AntiqueStore.resourceIndex}");
        }
        else
        {
            antiqueStoreEventTexture.gameObject.SetActive(false);
        }
    }

    public void InitList()
    {
        if (listTargets.Count > 0)
        {
            foreach (StageTarget obj in listTargets) Destroy(obj.gameObject);
        }

        listTargets.Clear();
        listCracks = 0;
        listAnmimals.Clear();
        animalCrackCount = 0;
        keyCount = 0;
    }

    public GameObject GetButton()
    {
        return startBtnTr.gameObject;
    }

    public void MakeBoniDialog(int index)
    {
        dialogSequence.Kill();
        if (index == -1)
        {
            if (IsApplyNoyBoostingEvent())
                index = 17;
            else if (isSetHardStageUI == true)
                index = 16;
        }
        string key = string.Format("p_sr_{0}", (index + 5));

        dialogBubble.text = GetLive2DString(key);

        dialogBubble.transform.localScale = Vector3.one * 0.5f;
        dialogBubble.transform.DOScale(Vector3.one, 0.2f);
    }

    public string GetReadyLoopAnimationName()
    {
        if (IsApplyNoyBoostingEvent())
        {
            return "chance_loop";
        }
        else
        {
            if (Global.GameType == GameType.EVENT || isSetHardStageUI == false)
                return "Ready";
            else
                return "flustered";
        }
    }

    public string GetItemAnimationName()
    {
        if (IsApplyNoyBoostingEvent())
        {
            return "InGame_Item_Select_02";
        }
        else
        {
            if (isSetHardStageUI == false)
                return "InGame_Item_Select";
            else
                return "InGame_Item_Select_01";
        }
    }

    public virtual AudioLobby GetReadyCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.readyVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    public AudioLobby GetMoveCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.moveVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    public AudioLobby GetFailCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.failVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    public virtual void SettingBoniDialog()
    {
        dialogSequence = DOTween.Sequence();
        if (IsApplyNoyBoostingEvent())
        {
            dialogBubble.text = Global._instance.GetString("p_sr_22");
        }
        else
        {
            if (isSetHardStageUI == false)
            {
                dialogBubble.text = GetLive2DString("p_sr_4");
            }
            else
            {
                dialogBubble.text = Global._instance.GetString("p_sr_21");
            }
        }
    }

    protected virtual string GetLive2DString(string key)
    {
        // 이벤트 진행중일때, 만약 라이브2D 캐릭터가 보니가 아니라면 에리어이벤트에서 같은 스트링을 찾아보고 없으면 뭐 기본스트링 사용한다
        if (Global.GameType == GameType.EVENT)
        {
            if (readyEventObj != null && readyEventObj.live2dCharacter != TypeCharacterType.Boni)
            {
                //해당 키가 글로벌에 없을 경우, 기본 대사 출력해야함.
                string eventKey = string.Format(key + "_{0}", (int)readyEventObj.live2dCharacter);
                if (Global._instance.HasString(eventKey) == true)
                    key = eventKey;
            }
        }
        return Global._instance.GetString(key);
    }

    void SetStageMission()
    {
        CandySprite.gameObject.SetActive(false);
        
        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    CandySprite.gameObject.SetActive(true);

                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear > 0)
                        CandySprite.spriteName = "Mission_DUCK_2";
                    else
                        CandySprite.spriteName = "Mission_DUCK_1";

                    CandySprite.MakePixelPerfect();
                    return;
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    CandySprite.gameObject.SetActive(true);

                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear > 0)
                        CandySprite.spriteName = "Mission_CANDY_2";
                    else
                        CandySprite.spriteName = "Mission_CANDY_1";

                    CandySprite.MakePixelPerfect();
                    return;
                }
            }
        }


    }

    /// <summary>
    /// 구버전 목표 타입의 데이터가 남아있는 경우 신 버전으로 옮김
    /// </summary>
    /// <param name="stageData"></param>
    private void CopyStageMapData_TargetCount_PrevVersionToNewVersion()
    {
        //일반 타입 목표의 데이터 옮기기
        for (int i = 0; i < tempData.collectCount.Length; i++)
        {
            if (tempData.collectCount[i] == 0)
                continue;

            //신버전 목표 데이터에 구버전 데이터 옮기기.
            SetStageMapData_TargetCount(i, tempData.collectCount[i]);
        }

        //컬러 타입 목표의 데이터 옮기기.(구버전에서는 컬러블럭만 사용했기 때문에 해당 데이터만 옮겨줌)
        int targetType = (int)TARGET_TYPE.COLORBLOCK;
        for (int i = 0; i < tempData.collectColorCount.Length; i++)
        {
            if (tempData.collectColorCount[i] == 0)
                continue;

            //신버전 목표 데이터에 구버전 데이터 옮기기.
            SetStageMapData_TargetCount(targetType, tempData.collectColorCount[i], i);
        }

        //구버전 데이터는 지워줌.
        tempData.collectCount = new int[1] { 0 };
        tempData.collectColorCount = new int[1] { 0 };
    }

    public void SetStageMapData_TargetCount(int targetType, int targetCount, int colorType = 0)
    {
        int findIndex = tempData.listTargetInfo.FindIndex(x => x.targetType == targetType);
        if (findIndex > -1)
        {   //기존에 데이터가 있으면, 해당 데이터에 값을 덮어씌워줌.
            CollectTargetInfo info = tempData.listTargetInfo[findIndex];
            int colorIndex = info.listTargetColorInfo.FindIndex(x => x.colorType == colorType);
            if (colorIndex > -1)
            {
                if (targetCount == 0)   //목표의 값이 0이면 리스트에서 삭제
                    info.listTargetColorInfo.Remove(info.listTargetColorInfo[colorIndex]);
                else
                    info.listTargetColorInfo[colorIndex].collectCount = targetCount;
            }
            else
            {   //기존에 딕셔너리는 있지만 컬러에 해당하는 데이터가 없는 경우, 해당 컬러의 목표를 추가해줌.
                if (targetCount == 0)
                    return;

                TargetColorInfo newData = new TargetColorInfo()
                {
                    colorType = colorType,
                    collectCount = targetCount,
                };
                info.listTargetColorInfo.Add(newData);
            }

            //해당 타입에 남아있는 목표 카운트가 없으면, 리스트에서 삭제해줌.
            if (info.listTargetColorInfo.Count == 0)
                tempData.listTargetInfo.Remove(info);
        }
        else
        {   //기존에 데이터가 없는 경우, 데이터를 추가해줌
            if (targetCount == 0)
                return;

            TargetColorInfo info = new TargetColorInfo()
            {
                colorType = colorType,
                collectCount = targetCount,
            };

            CollectTargetInfo targetInfo = new CollectTargetInfo();
            targetInfo.targetType = targetType;
            targetInfo.listTargetColorInfo.Add(info);

            tempData.listTargetInfo.Add(targetInfo);
        }
    }

    int currentChapter()
    {
        foreach (var item in ServerContents.Chapters)
        {
            if (item.Value.stageIndex <= Global.stageIndex && item.Value.stageIndex + item.Value.stageCount > Global.stageIndex)
            {
                return item.Value.index - 1;
            }
        }

        if (Global.stageIndex < 10)
        {
            return 0;
        }
        else if (Global.stageIndex < 21)
        {
            return 1;
        }
        else
        {
            return (Global.stageIndex - 21) / 15 + 2;
        }
    }
    new void OnDestroy()
    {
        _instance = null;

        listTargetInfo.Clear();
        OnDestroy_EventStage();
        base.OnDestroy();
    }

    protected virtual void SetBoniModel()
    {
        int modelNo = (int)TypeCharacterType.Boni;

        float posY = LanguageUtility.IsShowBuyInfo ? -290f : -320f-40f;
        
        if (Global.GameType == GameType.EVENT)
        {
            modelNo = ManagerCharacter._instance._live2dObjects.ContainsKey((int)(readyEventObj.live2dCharacter)) ? (int)readyEventObj.live2dCharacter : modelNo;

            boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);
            boniLive2D.SetVectorScale(readyEventObj.live2dSize);
            boniLive2D.SetPosition((new Vector3(221f, posY, 0f) + readyEventObj.live2dOffset));
        }
        else
        {
            boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);

            bool flip = ManagerCharacter._instance._live2dObjects[modelNo].defaultScale < 0.0f;
            // 보니를 300사이즈로 만드는 게 기준이므로, 다른캐릭터로 할 때는 보니 크기에 비례해서 스케일 조정해줘야
            float scaleRatio = Mathf.Abs(ManagerCharacter._instance._live2dObjects[modelNo].defaultScale / ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni].defaultScale);
            boniLive2D.SetVectorScale(new Vector3(flip ? -300f : 300f, 300f, 300f) * scaleRatio);
            boniLive2D.SetPosition(new Vector3(221f, posY, 0f));
        }

        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        SettingLive2DAnimation();
    }

    protected void SettingLive2DAnimation()
    {
        boniLive2D.SetAnimation("Ready_in", GetReadyLoopAnimationName());
    }

    #region 타겟 데이터 읽어오는 함수.
    protected void AddListTargetInfo(List<CollectTargetInfo> listAddTargetInfo)
    {
        for (int i = 0; i < listAddTargetInfo.Count; i++)
        {
            //이미 리스트에 들어있는 타겟 데이터면 추가하지 않음.
            if (listTargetInfo.FindIndex(x => x.targetType == listAddTargetInfo[i].targetType) != -1)
                continue;

            listTargetInfo.Add(listAddTargetInfo[i]);
        }
    }


    protected void LoadTarget()
    {
        //스테이지 목표 데이터 초기화(구버전에서 사용하던 데이터가 남아있으면 신버전 데이터로 옮겨줌)
        CopyStageMapData_TargetCount_PrevVersionToNewVersion();
        SortListTargetInfo();

        InitList();

        for (int i = 0; i < tempData.listTargetInfo.Count; i++)
        {
            CollectTargetInfo targetInfo = tempData.listTargetInfo[i];
            TARGET_TYPE targetType = (TARGET_TYPE)targetInfo.targetType;

            string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                string.Format("StageTarget_{0}", targetType) : "StageTarget";

            for (int j = 0; j < targetInfo.listTargetColorInfo.Count; j++)
            {
                BlockColorType colorType = (BlockColorType)targetInfo.listTargetColorInfo[j].colorType;

                StageTarget target = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIstageMission", targetRoot).GetComponent<StageTarget>();
                target.targetType = targetType;
                target.targetColor = colorType;

                //목표 수 표시
                string collectCount = targetInfo.listTargetColorInfo[j].collectCount.ToString();
                target.targetCount.text = collectCount;
                target.targetCountShadow.text = collectCount;

                //목표 이미지 설정
                string targetColorName = (colorType != BlockColorType.NONE) ?
                      string.Format("{0}_{1}", targetName, colorType) : targetName;
                target.targetSprite.spriteName = targetColorName;
                ManagerUIAtlas.CheckAndApplyEventAtlas(target.targetSprite);
                target.targetSprite.MakePixelPerfect();

                listTargets.Add(target);
            }
        }

        float startPos = (1 - listTargets.Count) * 48;
        for (int i = 0; i < listTargets.Count; i++)
        {
            listTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }
    }

    public void SortListTargetInfo()
    {
        //목표 순으로 정렬
        tempData.listTargetInfo.Sort(delegate (CollectTargetInfo a, CollectTargetInfo b)
        {
            if (a.targetType < b.targetType)
                return -1;
            else if (a.targetType > b.targetType)
                return 1;
            else
                return 0;
        });

        //컬러 순으로 정렬
        for (int i = 0; i < tempData.listTargetInfo.Count; i++)
        {
            tempData.listTargetInfo[i].listTargetColorInfo.Sort(delegate (TargetColorInfo a, TargetColorInfo b)
            {
                if (a.colorType < b.colorType)
                    return -1;
                else if (a.colorType > b.colorType)
                    return 1;
                else
                    return 0;
            });
        }
    }
    #endregion

    public virtual void SetHardStage()
    {
        if (tempData.isHardStage == 1)
        {
            if (isSetHardStageUI == true)
                return;
            //팝업 배경 이미지 변경.
            mainReadySprite.spriteName = "popup_bg_02";

            //스테이지 폰트 컬러설정.
            SetTitleTextColor(157f / 255f, 41f / 255f, 0f, 100f / 255f);

            //꽃 배경 이미지설정.
            SetFlowerBackTexture("local_ui/ready_flower_back_hardStage");
            
            //어려운 스테이지 라벨 오브젝트 활성화
            hardStageObj.SetActive(true);
            
            // 사탕/오리 위치 조정
            Vector3 candyPos = CandySprite.transform.localPosition;
            candyPos.y = -82.1f;
            CandySprite.transform.localPosition = candyPos;

            isSetHardStageUI = true;
        }
        else
        {
            //꽃 배경 이미지설정.
            SetFlowerBackTexture("local_ui/ready_flower_back");
            
            if (isSetHardStageUI == false)
                return;

            //팝업 배경 이미지 변경.
            mainReadySprite.spriteName = "popup_bg_01";

            //스테이지 폰트 컬러설정.
            SetTitleTextColor(52f / 255f, 95f / 255f, 130f / 255f, 100f / 255f);

            // 사탕/오리 위치 조정
            Vector3 candyPos = CandySprite.transform.localPosition;
            candyPos.y = -82.1f;
            CandySprite.transform.localPosition = candyPos;

            isSetHardStageUI = false;
        }
    }

    private void SetTitleTextColor(float r, float g, float b, float a)
    {
        Color stageTextColor = new Color(r, g, b);
        Color stageTextColorAlpha = new Color(r, g, b, a);
        if (stage[0] != null)
        {
            stage[0].effectColor = stageTextColor;
        }
        if (stage[1] != null)
        {
            stage[1].color = stageTextColor;
            stage[1].effectColor = stageTextColorAlpha;
        }
    }

    private void SetFlowerBackTexture(string filePath)
    {
        if (flowerBack[0] != null)
        {
            string path1 = string.Format("{0}_01", filePath);
            this.gameObject.AddressableAssetLoad<Texture>(path1,(x) => flowerBack[0].mainTexture = x);
        }
        if (flowerBack[1] != null)
        {
            string path2 = string.Format("{0}_02", filePath);
            this.gameObject.AddressableAssetLoad<Texture>(path2,(x) => flowerBack[1].mainTexture = x);
        }
    }

    protected void SetReadyItem(bool isLavaMode = false, bool checkPay = true)
    {
        //오픈여부 블러오기, 임의데이타
        //다이아, 코인 적용
        if (ServerRepos.UserItem == null) {
            Debug.LogWarning("User Item Class NULL.............");
        }

        //현재 선택된 아이템의 재화 합계와 남은 재화 수를 계산
        InitSelectReadyItem(isLavaMode);

        if (isLavaMode) listReadyItem[0].initItem(READY_ITEM_TYPE.ADD_TURN, 0, ServerRepos.UserItem.ReadyItem(0), GetReadyItemSaleType(READY_ITEM_TYPE.ADD_TURN), checkPay);
        else InitNoramlTypeReadyItem(0, READY_ITEM_TYPE.ADD_TURN, GetReadyItemSaleType(READY_ITEM_TYPE.ADD_TURN), checkPay);

        InitNoramlTypeReadyItem(1, READY_ITEM_TYPE.SCORE_UP, GetReadyItemSaleType(READY_ITEM_TYPE.SCORE_UP), checkPay);
        
        //랜덤 폭탄 레디아이템 처리
        CheckNoramlTypeAndTimeLimitTypeReadyItem(2, READY_ITEM_TYPE.RANDOM_BOMB, GetReadyItemSaleType(READY_ITEM_TYPE.RANDOM_BOMB), checkPay);

        //레디 아이템 타입이 나뉘는 아이템들은 상태 검사해서 설정해줌.
        CheckReadyItemStateAndInitReadyItem(3, READY_ITEM_TYPE.LINE_BOMB, GetReadyItemSaleType(READY_ITEM_TYPE.LINE_BOMB), checkPay);
        CheckReadyItemStateAndInitReadyItem(4, READY_ITEM_TYPE.CIRCLE_BOMB, GetReadyItemSaleType(READY_ITEM_TYPE.CIRCLE_BOMB), checkPay);
        CheckReadyItemStateAndInitReadyItem(5, READY_ITEM_TYPE.RAINBOW_BOMB, GetReadyItemSaleType(READY_ITEM_TYPE.RAINBOW_BOMB), checkPay);
    }

    public int GetReadyItemSaleType(READY_ITEM_TYPE itemType)
    {
        int saleType = (ServerRepos.LoginCdn.ReadyItemSale == 1) ? 1 : 0;

        if(itemType == READY_ITEM_TYPE.DOUBLE_ADD_TURN)
        {
            int doubleIndex = SERVER_DOUBLEREADY_INDEX;
            if (ServerRepos.LoginCdn.DoubleReadyItemSaleLevel != null)
                saleType = ServerRepos.LoginCdn.DoubleReadyItemSaleLevel[doubleIndex];
        }
        else if (itemType == READY_ITEM_TYPE.DOUBLE_SCORE_UP)
        {
            int doubleIndex = SERVER_DOUBLEREADY_INDEX + 1;
            if (ServerRepos.LoginCdn.DoubleReadyItemSaleLevel != null)
                saleType = ServerRepos.LoginCdn.DoubleReadyItemSaleLevel[doubleIndex];
        }
        else
        {
            //신버전 할인 데이터 있으면 해당 세일 타입 들고옴.
            if (ServerRepos.LoginCdn.ReadyItemSaleLevel != null)
                saleType = ServerRepos.LoginCdn.ReadyItemSaleLevel[(int)itemType];
        }

        return saleType;
    }

    public void ReleaseAllReadyItem()
    {
        for (int i = 0; i < UIPopupReady.readyItemSelectCount.Length; i++)
        {
            if (UIPopupReady.readyItemSelectCount[i] == 1)
                UIPopupReady.readyItemSelectCount[i] = 0;
        }
    }

    private void InitSelectReadyItem(bool isLavaMode)
    {
        if (Global.GameInstance.GetItemCostType(ItemType.READY_ITEM) == RewardType.jewel)
        {
            //코인 아이템 계산.
            int totalCoin = 0;
    
            //더블 아이템 먼저 계산
            totalCoin += isLavaMode == true ? 0 : GetSelectItemCost(6, READY_ITEM_TYPE.DOUBLE_ADD_TURN);
            totalCoin += GetSelectItemCost(7, READY_ITEM_TYPE.DOUBLE_SCORE_UP);
    
            //이후 단일 아이템도 계산
            totalCoin += isLavaMode == true ? 0 : GetSelectItemCost(0, READY_ITEM_TYPE.ADD_TURN);
            totalCoin += GetSelectItemCost(1, READY_ITEM_TYPE.SCORE_UP);
            totalCoin += GetSelectItemCost(2, READY_ITEM_TYPE.RANDOM_BOMB);
    
            //돈 모자라면, 선택 취소
            if (Global.coin < totalCoin)
            {
                //더블 아이템 해제
                for (int i = 6; i < 8; i++)
                {
                    readyItemSelectCount[i] = 0;
                    string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                    PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
                }
            }
    
            //이후 단일 아이템도 계산
            totalCoin = 0;
            totalCoin += isLavaMode == true ? 0 : GetSelectItemCost(0, READY_ITEM_TYPE.ADD_TURN);
            totalCoin += GetSelectItemCost(1, READY_ITEM_TYPE.SCORE_UP);
            totalCoin += GetSelectItemCost(2, READY_ITEM_TYPE.RANDOM_BOMB);
    
            //돈 모자라면, 현재 선택된 코인 아이템 모두 해제시켜줌.
            if (Global.coin < totalCoin)
            {
                for (int i = 0; i < 3; i++)
                {
                    // 레디 아이템을 가지고 있으면 아이템 선택 해제 제외.
                    if (ServerRepos.UserItem.GetItem(i) > 0) continue;
    
                    readyItemSelectCount[i] = 0;
                    string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                    PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
                }
            }
    
            //다이아 아이템 계산.
            int totalJewel = 0;
            totalJewel += GetSelectItemCost(3, READY_ITEM_TYPE.LINE_BOMB, true);
            totalJewel += GetSelectItemCost(4, READY_ITEM_TYPE.CIRCLE_BOMB, true);
            totalJewel += GetSelectItemCost(5, READY_ITEM_TYPE.RAINBOW_BOMB, true);
            //돈 모자라면, 현재 선택된 다이아 아이템 모두 해제시켜줌.
            if (Global.jewel < totalJewel)
            {
                for (int i = 3; i < 6; i++)
                {
                    // 레디 아이템을 가지고 있으면 아이템 선택 해제 제외.
                    if (ServerRepos.UserItem.GetItem(i) > 0) continue;
    
                    readyItemSelectCount[i] = 0;
                    string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                    PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
                }
            }
        }
        else
        {            
            int totalCoin = 0;
            totalCoin += isLavaMode == true ? 0 : GetSelectItemCost(6, READY_ITEM_TYPE.DOUBLE_ADD_TURN);
            totalCoin += GetSelectItemCost(7, READY_ITEM_TYPE.DOUBLE_SCORE_UP);
            totalCoin += isLavaMode == true ? 0 : GetSelectItemCost(0, READY_ITEM_TYPE.ADD_TURN);
            totalCoin += GetSelectItemCost(1, READY_ITEM_TYPE.SCORE_UP);
            totalCoin += GetSelectItemCost(2, READY_ITEM_TYPE.RANDOM_BOMB);
            totalCoin += GetSelectItemCost(3, READY_ITEM_TYPE.LINE_BOMB, true);
            totalCoin += GetSelectItemCost(4, READY_ITEM_TYPE.CIRCLE_BOMB, true);
            totalCoin += GetSelectItemCost(5, READY_ITEM_TYPE.RAINBOW_BOMB, true);
    
            if (Global.coin < totalCoin)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i < 6 && ServerRepos.UserItem.GetItem(i) > 0) continue;
                    readyItemSelectCount[i] = 0;
                    string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                    PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
                }
            }
        }
    }

    private int GetSelectItemCost(int index, READY_ITEM_TYPE type, bool boosterType = false)
    {
        if (readyItemSelectCount[index] == 0)
            return 0;
        if (Global.GameInstance.CanUseReadyItem((int)type) == false)
            return 0;
        if (ServerRepos.UserItem.ReadyItem(index) > 0)
            return 0;

        int cost = 0;

        if (type == READY_ITEM_TYPE.DOUBLE_ADD_TURN)
            cost = ServerRepos.LoginCdn.DoubleReadyItems[UIPopupReady.SERVER_DOUBLEREADY_INDEX];
        else if (type == READY_ITEM_TYPE.DOUBLE_SCORE_UP)
            cost = ServerRepos.LoginCdn.DoubleReadyItems[UIPopupReady.SERVER_DOUBLEREADY_INDEX+1];
        else
            cost = Global.GameInstance.GetItemCostList(ItemType.READY_ITEM)[index];

        return cost;
    }
    
    //노이 부스팅 이벤트 적용되어 있는 상태인지.
    private bool IsApplyNoyBoostingEvent()
    {
        if (Global.GameType != GameType.NORMAL)
            return false;
        if (ManagerNoyBoostEvent.instance == null)
            return false;
        //일시정지 창 켜져있는 상태에서 레디창이 켜졌을때는, 매번 부스팅 이벤트 끝났는지 검사.
        if (UIPopupPause._instance != null && Global.LeftTime(ManagerNoyBoostEvent.instance.EndTs) < 0)
            return false;
        return ManagerNoyBoostEvent.instance.IsActiveUser();
    }

    //현재 레디 아이템의 상태를 검사하고 초기화 해주는 함수.
    private void CheckReadyItemStateAndInitReadyItem(int index, READY_ITEM_TYPE type, int saleType, bool checkPay)
    {
        if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(index) == true && Global.GameInstance.CanUseReadyItem(index) == true)
        {   //시간제 폭탄 아이템 적용 상태 확인
            InitTimeLimitTypeReadyItem(index, type, true);
        }
        else if (Global.GameType == GameType.TREASURE_HUNT)
        {
            InitTreasureModeTypeReadyItem(index, type, saleType, checkPay);
        }
        else
        {   //기본 상태 
            InitNoramlTypeReadyItem(index, type, saleType, checkPay);
        }
    }

    //랜덤 폭탄 아이템 처리
    private void CheckNoramlTypeAndTimeLimitTypeReadyItem(int index, READY_ITEM_TYPE type, int saleType, bool checkPay)
    {
        if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(index) == true && Global.GameInstance.CanUseReadyItem(index) == true)
        {   //시간제 폭탄 아이템 적용 상태 확인
            InitTimeLimitTypeReadyItem(index, type, true);
        }
        else
        {   //기본 상태 
            InitNoramlTypeReadyItem(index, type, saleType, checkPay);
        }
    }

    //일반 타입의 레디 아이템들 초기화. // 더블 X
    private void InitNoramlTypeReadyItem(int index, READY_ITEM_TYPE type, int saleType, bool checkPay)
    {
        listReadyItem[index].initItem(type, ServerRepos.LoginCdn.ReadyItems[index], ServerRepos.UserItem.ReadyItem(index), saleType, checkPay);
    }
    
    //코인소모처 타입의 레디 아이템들 초기화. // 더블 X
    private void InitTreasureModeTypeReadyItem(int index, READY_ITEM_TYPE type, int saleType, bool checkPay)
    {
        listReadyItem[index].initItem(type, Global.GameInstance.GetItemCostList(ItemType.READY_ITEM)[index], ServerRepos.UserItem.ReadyItem(index), saleType, checkPay);
    }

    //부스터 타입의 레디 아이템들 초기화.
    private void InitBoosterTypeReadyItem(int index, READY_ITEM_TYPE type)
    {
        listReadyItem[index].InitBoostingItem(type);
    }

    //시간제 타입의 레디 아이템들 초기화.
    public void InitTimeLimitTypeReadyItem(int index, READY_ITEM_TYPE type, bool isRemainTime = true)
    {
        if (isRemainTime == true)
        {
            listReadyItem[index].InitTimeLimitItem(type);
        }
        else
        {
            CheckReadyItemStateAndInitReadyItem(index, type, GetReadyItemSaleType(type), false);
        }
        readyItemTimeLimit[index] = isRemainTime;
    }

    protected virtual void SetStageFlower()
    {
        //꽃 이미지 표시하지 않는 타입인지 검사.
        if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_READY) == false)
        {
            return;
        }

        //꽃 이미지 설정.
        int star = 0;
        if (Global.stageIndex > 0 && ManagerData._instance._stageData[Global.stageIndex - 1] != null)
            star = ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;

        {
            string flowerPath = string.Format("local_ui/ready_icon_flower_0{0}", star);
            this.gameObject.AddressableAssetLoad<Texture>(flowerPath,(x) => flower.mainTexture = x);
            if (star == 1)
            {
                flower.width = 97;
                flower.height = 130;
                flower.transform.localPosition = new Vector3(1f, -15f, 0f);
                flowerShadow.width = 90;
            }
            else if (star == 2)
            {
                flower.width = 177;
                flower.height = 124;
                flower.transform.localPosition = new Vector3(2f, -12f, 0f);
            }
            else if (star == 3)
            {
                flower.width = 160;
                flower.height = 157;
                flower.transform.localPosition = new Vector3(5f, -12f, 0f);
            }
            else if (star == 4)
            {
                flower.width = 162;
                flower.height = 158;
                flower.transform.localPosition = new Vector3(2f, -17f, 0f);
            }
            else if (star == 5)
            {
                flower.width = 162;
                flower.height = 158;
                flower.transform.localPosition = new Vector3(2f, -17f, 0f);
            }
            else
            {
                flower.width = 100;
                flower.height = 141;
                flower.transform.localPosition = new Vector3(3f, -10f, 0f);
                flowerShadow.enabled = false;
            }
        }
    }

    #region 두더지잡기 이벤트
    private void SetMoleCatchEvent()
    {
        if (Global.GameType != GameType.MOLE_CATCH)
            return;

        moleCatchStageSign.SetActive(true);
        moleCatchStageText.text = Global.stageIndex.ToString();
        ChangeMoleCatchImage();
    }

    private void ChangeMoleCatchImage()
    {
        int currentWave = ManagerMoleCatch.instance.GetWaveIndex();
        int allWaveCount = ManagerMoleCatch.instance.GetWaveCount();
        //wave count가 1이하이거나, 마지막 wave가 아닐 경우 일반 웨이브.
        bool isNormalWave = (allWaveCount == 1 || currentWave != allWaveCount) ? true : false;

        var moleCatchObj = ManagerMoleCatch.instance.readyResource;
        //어려운 웨이브의 두더지는 화난 이모티콘 달고 있음.
        if (isNormalWave == false)
        {
            flowerEmoticon.mainTexture = moleCatchObj.readyEmoticonTexture;
            flowerEmoticon.MakePixelPerfect();
        }

        if (tempData.isHardStage == 0)
        {
            StartCoroutine(CoMoleCatchImageAction(moleCatchObj.readyMole_normal_1, moleCatchObj.readyMole_normal_2,
                moleCatchObj.readyEmoticonPosition_1, moleCatchObj.readyEmoticonPosition_2));
        }
        else
        {
            StartCoroutine(CoMoleCatchImageAction(moleCatchObj.readyMole_hard_1, moleCatchObj.readyMole_hard_2,
                moleCatchObj.readyEmoticonPosition_1, moleCatchObj.readyEmoticonPosition_2));
        }
    }

    private IEnumerator CoMoleCatchImageAction(Texture texture_1, Texture texture_2, Vector3 emoticonPos_1, Vector3 emoticonPos_2)
    {
        bool isTextureOn = true;
        flower.transform.localPosition = new Vector3(0f, -70f, 0f);
        while (true)
        {
            if (isTextureOn == true)
            {
                flower.mainTexture = texture_1;
                isTextureOn = false;
                flower.MakePixelPerfect();
                flowerEmoticon.transform.localPosition = emoticonPos_1;
            }
            else
            {
                flower.mainTexture = texture_2;
                isTextureOn = true;
                flower.MakePixelPerfect();
                flowerEmoticon.transform.localPosition = emoticonPos_2;
            }
            yield return new WaitForSeconds(0.7f);
        }
    }
    #endregion

    /*
    private void SetPosition()
    {
        int nCount = listTargets.Count;
        if (nCount % 2 == 0)
            targetTr.localPosition = new Vector3(55, targetTr.localPosition.y, targetTr.localPosition.z);
        else
            targetTr.localPosition = new Vector3(0, targetTr.localPosition.y, targetTr.localPosition.z);

        for (int i = 0; i < nCount; i++)
        {
            float xPos = (-118 * (nCount / 2)) - (-118 * i);
            listTargets[i].SetPos(xPos);
            listTargets[i].ReadyPopUpTarget();
        }
    }*/



    public override void ClosePopUp(float _startTime = 0.3f, Method.FunctionVoid callback = null)
    {
        PlayerPrefs.SetString("LastStageName", "");

        StartCoroutine(CoAction(_startTime, () =>
        {
            int nCount = listTargets.Count;
            for (int i = 0; i < nCount; i++)
            {
                listTargets[i].Recycle();
            }
        }));

        boniLive2D.SetAnimation("Ready_out", false);

        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
        mainSprite.transform.DOScale(Vector3.zero, openTime).SetEase(Ease.InBack);
        StartCoroutine(CoAction(0.1f, () =>
        {
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, openTime - 0.1f);
        }));

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(openTime, () =>
            PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(openTime + 0.15f, () =>
        {
            Destroy(gameObject);
        }));
    }

    public void ShowUseClover()
    {
        StartCoroutine(ShowCloverEffect());
    }

    protected virtual IEnumerator ShowCloverEffect(bool setLayerSort = false)
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;
        float defaultScaleValue = startClover.cachedTransform.localScale.x;
        Vector3 startButtonPos = startBtnTr.localPosition;

        showTimer = 0;

        RingGlowEffect ringGlow = NGUITools.AddChild(startBtnTr.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.9f;

        // 특정 레디 팝업에서 setLayerSort 설정하여 클로버 터지는 이펙트 뎁스 정렬 진행
        if (!setLayerSort)
        {
            NGUITools.AddChild(startBtnTr.gameObject, _objEffectStartButton);
        }
        else
        {
            var effect = NGUITools.AddChild(startBtnTr.gameObject, _objEffectStartButton);
            effect.GetComponent<Renderer>().sortingOrder = objMaskPanel.GetComponent<UIPanel>().sortingOrder - 1;
        }

        _objEffectButton.SetActive(true);

        //이펙트 터질 때 버튼 움직임.
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        while (showTimer < 0.8f)
        {
            showTimer += Global.deltaTimeLobby * 4f;

            if (showTimer < 0.5f)
            {
                scaleRatio = 0.7f + showTimer;
            }
            else
            {
                scaleRatio = 1.8f - showTimer;
            }

            startBtnTr.localPosition = startButtonPos * (1 + (1 - showTimer) * 0.04f);
            startClover.cachedTransform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            _objEffectButton.transform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            yield return null;
        }

        _objEffectButton.SetActive(false);
        showTimer = 0;

        while (showTimer < 0.5f)
        {
            showTimer += Global.deltaTimeLobby;
            yield return null;
        }

        //인게임 씬로드.
        //GlobalGameManager.instance.LoadScene(eSceneNameType.InGame);
        startClover.cachedTransform.localScale = Vector3.one * defaultScaleValue;

        while (true)
        {
            if (_recvGameStart_end)
                break;
            yield return null;
        }



        touchButton = false;
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
        //ManagerNetwork._instance.SendStagePlay(Global.stageIndex, readyItemUseCount);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("InGame");
        ManagerUI._instance.bTouchTopUI = true;

        yield return null;
    }

    protected bool touchButton = false;

    protected virtual void OnClickGoInGame()
    {
        //통상 플레이에서, 미션 진행 제한에 걸렸다면 플레이 할 수 없음
        if (Global.GameType == GameType.NORMAL &&
            (GameData.User.stage == stageIndex) && ManagerUI._instance.IsCanPlayLastState() == false)
        {
            ManagerUI._instance.OpenCantPlaySystemPopup();
            return;
        }
        
        if (bCanTouch == false)
            return;
        bCanTouch = false;


        #region 이벤트 종료시간 체크하는 영역

        bool needRefreshPopup = false;
        string refreshPopupKey = "";

        if (Global.GameType == GameType.NORMAL)
        {
            if(_groupRankingEventTexture.gameObject.activeSelf)
            {
                if (ManagerGroupRanking.IsGroupRankingStage() == false)
                {
                    needRefreshPopup = true;
                    refreshPopupKey  = "n_s_54";
                }
            }
            if (noyBoostingEventRoot.activeSelf)
            {
                if (IsApplyNoyBoostingEvent() == false || Global.LeftTime(ManagerNoyBoostEvent.instance.EndTs) < 0)
                {
                    needRefreshPopup = true;
                    refreshPopupKey = "n_ev_13";
                }
                else
                {
                    ManagerNoyBoostEvent.instance.isBoostOn = true;
                }
            }
            if (ServerContents.PokoFlowerEvent != null && this.pokoFlowerEventUrlTexture.gameObject.activeSelf)
            {
                if (Global.LeftTime(ServerContents.PokoFlowerEvent.end_ts) < 0)
                {
                    needRefreshPopup = true;
                    refreshPopupKey = "n_s_54";
                }
            }
            if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == true && alphabetEventTexture.gameObject.activeSelf)
            {
                if (ManagerAlphabetEvent.instance == null || ManagerAlphabetEvent.instance.CheckEventEnd() == true)
                {
                    needRefreshPopup = true;
                    refreshPopupKey = "n_s_54";
                }
            }
            if (stageAssistMission.gameObject.activeSelf)
            {
                if (ManagerStageAssistMissionEvent.Instance == null || ManagerStageAssistMissionEvent.CheckStartable() == false
                    || ManagerStageAssistMissionEvent.IsNewStageCheck() == false)
                {
                    needRefreshPopup = true;
                    refreshPopupKey = "n_ev_13";
                }
            }

            if (antiqueStoreEventTexture.gameObject.activeSelf)
            {
                if (ManagerAntiqueStore.instance == null || ManagerAntiqueStore.CheckStartable() == false)
                {
                    needRefreshPopup = true;
                    refreshPopupKey = "n_ev_13";
                }
                else
                    ManagerAntiqueStore.instance.SyncAntiqueStoreSpecialEvent();

            }
            
            if (ServerContents.CriminalEvent != null)
            {
                if(criminalEventUrlTexture.gameObject.activeSelf)
                {
                    if (ManagerCriminalEvent.instance == null ||
                        Global.LeftTime(ManagerCriminalEvent.instance.endTs) <= 0)
                    {
                        needRefreshPopup = true;
                        refreshPopupKey = "n_s_54";
                    }
                }
                
                if(ManagerCriminalEvent.instance != null)
                    ManagerCriminalEvent.instance.SyncFromServerUserData();
            }
        }

        if (needRefreshPopup)
        {
            _callbackClose = () => { ManagerUI._instance.OpenPopupReady(tempData); };
            bCanTouch = true;

            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(refreshPopupKey), false, OnClickBtnClose);
            popup.SortOrderSetting();
            return;
        }

        long startTime = Global.GetTime();

        #endregion
        
        if ((Global.clover > 0 || GameData.RemainFreePlayTime() > 0 || bFreeStage == true) && touchButton == false)
        {
            if (UIPopupPause._instance != null && UIPopupPause._instance.preservedRestart != null)
            {
                StartCoroutine(CoRestart(startTime));
            }
            else StartCoroutine(CoStartGame(startTime));
        }
        else if (Global.clover <= 0 && touchButton == false)
        {
            ManagerUI._instance.LackCloverProcess();
            bCanTouch = true;
        }
    }

    protected IEnumerator CoRestart(long startTime)
    {
        if (UIPopupPause._instance == null)
        {
            yield break;
        }

        bStartGame = true;

        //광고 출력
        var isGetADReward_AddTurn = false;
        if (isCanWatchAD_AddTurn == true && isSelectAD_AddTurn == true)
        {
            yield return CoStartAD_BeforeGameStart((isSuccess) => isGetADReward_AddTurn = isSuccess);
            if (isGetADReward_AddTurn == false)
            {   //광고 시청에 실패했다면, 게임 시작하지 않음
                bCanTouch = true;
                bStartGame = false;
                yield break;
            }
        }

        //턴 추가 보상받지 않았다면 광고 턴 카운트 초기화
        if (isGetADReward_AddTurn == false)
        {
            addTurnCount_ByAD.Value = 0;
        }

        //게임 실패 처리
        ServerAPI.GameFail(UIPopupPause._instance.preservedRestart.failReq, (resp) =>
        {
            if (resp.IsSuccess)
            {
                Global.GameInstance.OnRecvGameRestart();

                FailCountManager._instance.SetFail(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex);

                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", UIPopupPause._instance.preservedRestart.growthyLog);

                UIPopupPause._instance.preservedRestart = null;

                StartGame(startTime);
            }
        });
        Global.SetIsClear(false);
    }

    protected IEnumerator CoStartGame(long startTime)
    {
        bStartGame = true;

        //광고 출력
        bool isGetADReward_AddTurn = false;
        if (isCanWatchAD_AddTurn == true && isSelectAD_AddTurn == true)
        {
            yield return CoStartAD_BeforeGameStart((isSuccess) => isGetADReward_AddTurn = isSuccess);
            if (isGetADReward_AddTurn == false)
            {   //광고 시청에 실패했다면, 게임 시작하지 않음
                bCanTouch = true;
                bStartGame = false;
                yield break;
            }
        }

        //턴 추가 보상받지 않았다면 광고 턴 카운트 초기화
        if (isGetADReward_AddTurn == false)
            addTurnCount_ByAD.Value = 0;

        //게임 시작
        StartGame(startTime);
    }

    private IEnumerator CoStartAD_BeforeGameStart(System.Action<bool> endCallback)
    {
        #region 광고 출력
        {
            bool isCompleteAD = false;
            bool isSuccessWatchAD = false;

            CdnAdReadyItemInfo readyInfo = Global.GetAdReadyItemInfo();
            AdManager.AdType adType = readyInfo != null ? (AdManager.AdType)readyInfo.adType : 0;

            AdManager.ShowAD_ReqAdReadyItem(adType, (isSuccess) =>
            {
                if (isSuccess == true)
                {   //광고를 정상적으로 확인한 상태에서는 인게임 리워드 제공
                    addTurnCount_ByAD.Value = Global.GameInstance.TurnCountByReadyAD();
                    isSuccessWatchAD = true;
                }
                isCompleteAD = true;
            });
            yield return new WaitUntil(() => isCompleteAD == true);
            endCallback.Invoke(isSuccessWatchAD);
        }
        #endregion
    }

    protected virtual void StartGame(long startTime)
    {
        FailResetAdBackupData();

        ManagerSound.AudioPlay(AudioLobby.UseClover);
        touchButton = true;
        // UI 정지 시키고 .. 클로버 날리기
        //클로버날리기
        RingGlowEffect ringGlow = NGUITools.AddChild(ManagerUI._instance._CloverSprite.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.45f;

        UIUseCloverEffect cloverEffect = NGUITools.AddChild(clippingPanel.gameObject, _objEFfectUseClover).GetComponent<UIUseCloverEffect>();
        cloverEffect.targetObj = this.gameObject;

        //클로버 사용(이벤트 무료 스테이지 거나, 무료 타임이 남았을 경우 투명 클로버).
        if (GameData.RemainFreePlayTime() > 0 || bFreeStage == true)
        {
            cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position, true);
        }
        else
        {
            cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position);
        }

        //클로버 사용하는 경우 클로버 감소.
        if (GameData.RemainFreePlayTime() <= 0 && bFreeStage == false)
        {
            Global.clover--;
            ManagerUI._instance.UpdateUI();
        }

        getReadyItemCount = new int[]
        {
                ServerRepos.UserItem.ReadyItem(0),
                ServerRepos.UserItem.ReadyItem(1),
                ServerRepos.UserItem.ReadyItem(2),
                ServerRepos.UserItem.ReadyItem(3),
                ServerRepos.UserItem.ReadyItem(4),
                ServerRepos.UserItem.ReadyItem(5)
        };

        for (int i = 0; i < 8; i++)
        {
            string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
            PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
        }

        for (int i = 0; i < 8; i++)
        {
            readyItemUseCount[i] = new EncValue();
            readyItemUseCount[i].Value = readyItemSelectCount[i];

            if (readyItemSelectCount[i] > 0 && Global.GameInstance.CanUseReadyItem(i) == false)
            {
                readyItemUseCount[i].Value = 0;
            }
        }

        if (tempData.gameMode == (int)GameMode.LAVA)
        {
            readyItemUseCount[0].Value = 0;
            readyItemUseCount[6].Value = 0;
        }

        for (int i = 0; i < 8; i++)
        {
            if (readyItemUseCount[i].Value > 0)
            {
                if (i < 3)
                {
                    //사과, 스코어업
                    if (getReadyItemCount[i] < readyItemUseCount[i].Value)
                    {
                        if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.coin))
                        {
                            payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            freeCoin[i] = 0;
                        }
                        else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                        {
                            payCoin[i] = (int)(GameData.User.coin);
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.coin);
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                        }
                    }
                }
                else if (Global.GameInstance.CanUseDoubleReadyItem() == true && (i == 6 || i == 7))
                {
                    int doubleIndex = SERVER_DOUBLEREADY_INDEX;
                    if (i == 7) doubleIndex++;

                    //더블 아이템 사과, 스코어업
                    if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] < (int)(GameData.User.coin))
                    {
                        payCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                        freeCoin[i] = 0;
                    }
                    else if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                    {
                        payCoin[i] = (int)(GameData.User.coin);
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] - (int)(GameData.User.coin);
                    }
                    else
                    {
                        payCoin[i] = 0;
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                    }
                }
                else
                {
                    if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i) == false && getReadyItemCount[i] < readyItemUseCount[i].Value)
                    {
                        if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.jewel))
                        {
                            payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            freeCoin[i] = 0;
                        }
                        else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.jewel) && (int)(GameData.User.jewel) > 0)
                        {
                            payCoin[i] = (int)(GameData.User.jewel);
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.jewel);
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                        }
                    }
                }
            }
        }

        UIItemStageAlarmBubble._instance.LastEpisodeCheck(Global.stageIndex);

        beforeFCoin = (int)(GameData.User.fcoin);
        beforeCoin = (int)(GameData.User.coin);
        beforeFDia = (int)(GameData.User.fjewel);
        beforeDia = (int)(GameData.User.jewel);

        // 0~5 레디아이템, 15~16 더블레디아이템 //6~14 인게임아이템, 컨티뉴횟수 등 : 서버측 동기화요청으로 0으로 설정
        var useItems = new int[] {
                    readyItemUseCount[0].Value,
                    readyItemUseCount[1].Value,
                    readyItemUseCount[2].Value,
                    readyItemUseCount[3].Value,
                    readyItemUseCount[4].Value,
                    readyItemUseCount[5].Value,
                    0, //6
                    0, //7
                    0, //8
                    0, //9
                    0, //10
                    0, //11
                    0, //12
                    0, //13
                    0, //14
                    readyItemUseCount[6].Value,
                    readyItemUseCount[7].Value };

        //유저의 레디 아이템 소모 없이 사용가능한 아이템 리스트 초기화
        for (int i = 0; i < 6; i++)
        {
            readyItemAutoUse[i] = new EncValue();
            readyItemAutoUse[i].Value = 0;
        }

        //시간제 레디 아이템 사용되어 있으면 무료로 사용 가능한 아이템 true 로 설정.
        for (int i = 0; i < readyItemTimeLimit.Length; i++)
        {
            if (readyItemTimeLimit[i] == true)
                readyItemAutoUse[i].Value = 1;
        }

        /*
        if ((int)ServerRepos.User.clover > 1) usePCoin = 1;
        else useFCoin = 1;
        */

        #region 광고 데이터 추가
        var listADItems = new List<int>();
        int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (int i = 0; i < itemCount; i++)
            listADItems.Add(0);

        if (isCanWatchAD_AddTurn == true && isSelectAD_AddTurn == true && addTurnCount_ByAD.Value > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        #endregion

        int tempGameMode = (int)Global.GameType;

        var req = new GameStartReq()
        {
            ts = startTime,
            type = tempGameMode,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            items = useItems,
            adItems = listADItems,
        };

        ServerRepos.GameStartTs = startTime;    // 임시작업

        QuestGameData.SetUserData();

        Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
    }

    protected void onFailStart(GameStartReq req)
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        popupSystem.FunctionSetting(1, "RetryGameStart", gameObject);
        popupSystem.FunctionSetting(3, "RetryCancel", gameObject);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_17"), false, null);
        popupSystem.SetResourceImage("Message/error");

        popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
        retryReq = req;
    }

    void RetryCancel()
    {
        Global.ReBoot();

    }

    GameStartReq retryReq = null;

    void RetryGameStart()
    {
        if (retryReq == null)
        {
            RetryCancel();
            return;
        }

        Global.GameInstance.GameStart(retryReq, recvGameStart, onFailStart);
    }

    //가지고 있는 레디아이템 갯수
    protected int[] getReadyItemCount;

    protected int[] payCoin = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
    protected int[] freeCoin = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

    protected int beforeFCoin = 0;
    protected int beforeCoin = 0;
    protected int beforeFDia = 0;
    protected int beforeDia = 0;

    protected virtual void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //게임 시작 설정
            ManagerLobby._stageStart = true;

            //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
            ManagerUI._instance.InitActionCurrency();

            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            stageIndex = Global.stageIndex;
            QuestGameData.SetUserData();

            if (ManagerCoinStashEvent.CheckStartable())
                ManagerCoinStashEvent.currentCoinMultiplierState = ServerRepos.UserCoinStash.multiplier;

            _recvGameStart_end = true;

            //그로씨
            //클로버사용
            if (GameData.RemainFreePlayTime() <= 0 && bFreeStage == false)
            {
                var useClover = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_GAME_PLAY,
                        0, //-usePCoin,
                        -1, //-useFCoin,
                        0,//(int)(ServerRepos.User.clover),
                        (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                        mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                var cloverDoc = JsonConvert.SerializeObject(useClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
            }
            else
            {
                var useClover = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_GAME_PLAY_FREE,
                        0, //-usePCoin,
                        0, //-useFCoin,
                        0,//(int)(ServerRepos.User.clover),
                        (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                        mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                var cloverDoc = JsonConvert.SerializeObject(useClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
            }

            for (int i = 0; i < 8; i++)
            {
                //RewardType 의 더블 아이템 인덱스 계산
                string rewardTypestring_ReadyItem = ((i <= 5)? ((RewardType)((int)RewardType.readyItem1 + i)).ToString() : ((RewardType)((int)RewardType.readyItem7 + (i - 6))).ToString());

                if (readyItemUseCount[i].Value > 0)
                {
                    if (listReadyItem[i].type == READY_ITEM_TYPE.LOCK) continue;

                    if (i < 3 || i == 6 || i == 7)
                    {
                        // if (getReadyItemCount[i] < readyItemUseCount[i])
                        if (payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeCoin -= payCoin[i];
                            beforeFCoin -= freeCoin[i];

                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                                (
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                                -payCoin[i],
                                -freeCoin[i],
                                beforeCoin,     //(int)(GameData.User.coin),
                                beforeFCoin,    // (int)(GameData.User.fcoin),
                                "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                                );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            
                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                                (
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                    rewardTypestring_ReadyItem,
                                    "ReadyItem" + listReadyItem[i].type.ToString(),
                                    readyItemUseCount[i].Value,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                                    Global.GameInstance.GetGrowthyGameMode().ToString()
                                );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }

                    }
                    else
                    {
                        if (payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeDia -= payCoin[i];
                            beforeFDia -= freeCoin[i];


                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                            (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -payCoin[i],
                            -freeCoin[i],
                           beforeDia,
                            beforeFDia,
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                            );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                            (
                               ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                rewardTypestring_ReadyItem,
                                "ReadyItem" + listReadyItem[i].type.ToString(),
                                readyItemUseCount[i].Value,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                                Global.GameInstance.GetGrowthyGameMode().ToString()
                            );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }
                    }

                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[i].type.ToString(),
                            -readyItemUseCount[i].Value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
                else if (readyItemUseCount[i].Value == 0)
                {   // 무료로 사용된 케이스 체크
                    bool usedFree = false;
                    if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i))
                    {
                        usedFree = true;
                    }

                    if (usedFree)
                    {
                        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[i].type.ToString(),
                            0,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_FREE_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                        var doc = JsonConvert.SerializeObject(useReadyItem);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                    }
                }
            }

            //선택한 스테이지 다시할경우
            if (SceneManager.GetActiveScene().name != "InGame")
                beforeStage = false;

            if (Global.GameType == GameType.EVENT && ServerContents.EventChapters.type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
            {
                if (eventStep > Global.stageIndex)
                    beforeStage = true;
                else
                    beforeStage = false;
            }
        }
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
        ManagerUI._instance.InitActionCurrency();

        //재화 및 UI 업데이트
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        ManagerUI._instance.ClosePopUpUI();
    }

    public override void OnClickBtnBack()
    {
        if (bCanTouch == false)
            return;
        OnClickBtnReadyClose();
    }

    void OnClickBtnReadyClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
        ManagerUI._instance.InitActionCurrency();

        //재화 및 UI 업데이트
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        //팝업 취소(닫기)눌렀을 때 콜백 실행
        readyCancelAction?.Invoke();

        if (ManagerGroupRanking.isEventOn)
        {
            ManagerGroupRanking.instance.isTutorialEnd = true;
        }
        
        //팝업 닫기
        ManagerUI._instance.ClosePopUpUI();
    }

    private void ChangeEventButtonClover()
    {
        if (GameData.RemainFreePlayTime() > 0)
            return;

        int freeStage = eventChapter.freePlays[eventGroup - 1] - 1;
        if (freeStage >= 0 && freeStage <= readyEventObj._free.Count)
        {
            //무료스테이지가 현재 진행중인 스테이지라면 무료 클로버 표시.
            if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
            {
                if ((freeStage + 1) == eventStep)
                {
                    SettingFreeCloverButton(true, true);
                    bFreeStage = true;
                }
                else
                {
                    SettingFreeCloverButton(false, true);
                    bFreeStage = false;
                }
            }
            else
            {
                if ((freeStage + 1) == Global.stageIndex)
                {
                    SettingFreeCloverButton(true, true);
                    bFreeStage = true;
                }
                else
                {
                    SettingFreeCloverButton(false, true);
                    bFreeStage = false;
                }
            }
        }
    }

    protected IEnumerator OnTouch(float time)
    {
        if (onFailResetAd)
            yield break;
        
        yield return new WaitForSeconds(time);
        this.bCanTouch = true;
    }

    //스테이지 정보 다시부르기
    protected virtual IEnumerator CoCheckStageDataBeforeOpenPopUpReady()
    {
        //기존에 열린 스테이지가 용암인지 아닌지
        bool isLavaStage = tempData.gameMode == (int)GameMode.LAVA;

        string stageName = Global.GameInstance.GetStageFilename();
        yield return ManagerUI._instance.CoCheckStageData(stageName);

        using (var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                tempData = serializer.Deserialize(reader) as StageMapData;
            }
            else
            {
                NetworkLoading.EndNetworkLoading();
            }
        }
        yield return null;

        AddListTargetInfo(tempData.listTargetInfo);

        //레디 창 목표 아틀라스 재 조합.
        yield return ManagerUI._instance.CoMakeTargetAtlas(tempData.collectCount, tempData.collectColorCount, listTargetInfo);

        LoadTarget();
        
        if (eventChapter != null && eventChapter.type == (int) EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            SettingEventFailResetEvent();
            ChangeEventButtonClover();
            yield break;
        }
        
        //목표바꾸기, 파랑새이동
        if (Global.GameType == GameType.EVENT)
        {
            Vector3 pos = readyEventObj._step[Global.stageIndex - 1].transform.localPosition;
            pos += readyEventObj._offsetPoint;
            readyEventObj._texturePointShadow.transform.DOLocalMove(pos, 0.5f).SetEase(Ease.OutQuint);
            PlayMoveSound();
            SettingEventFailResetEvent();
            ChangeEventButtonClover();
            StartCoroutine(CoAction(0.5f, () => { ManagerSound.AudioPlay(AudioLobby.BoniStep); }));
        }
        else
        {
            SetStageFlower();
            SetStageLabel();
            SetStageMission();
            SetHardStage();
        }
        //연출 후 터치가능.
        StartCoroutine(OnTouch(0.5f));
        yield return null;
    }
    
    private void OnClickOpenPopUpExchangeStationBtn()
    {
        ManagerUI._instance.OpenPopupWorldRankExchangeShop();
    }

    #region 게임 시작버튼 설정
    /// <summary>
    /// 시작 버튼 이미지 설정
    /// </summary>
    protected virtual void SetStartButton()
    {
        if (isSelectAD_AddTurn == true)
        {   //광고 선택 상태 확인
            InitSelectADTypeBtnStart();
        }
        else
        {   //기본 상태 
            InitNormalTypeBtnStart();
        }
    }

    protected virtual  void InitSelectADTypeBtnStart()
    {
        sprStartADIcon_AddTurn.gameObject.SetActive(true);
        Vector3 originScale = sprStartADIcon_AddTurn.transform.localScale;
        sprStartADIcon_AddTurn.transform.localScale = Vector3.zero;
        sprStartADIcon_AddTurn.transform.DOScale(originScale, 0.1f).SetEase(Ease.InOutBack);

        //버튼 및 스타트 텍스트 컬러 설정
        SetStartButtonColor(BtnColorType.yellow);
    }

    private void InitNormalTypeBtnStart()
    {
        sprStartADIcon_AddTurn.gameObject.SetActive(false);

        //버튼 및 스타트 텍스트 컬러 설정
        SetStartButtonColor(BtnColorType.green);
    }

    public void SetStartButtonColor(BtnColorType btnColorType)
    {
        var colorData = ManagerUI._instance.GetButtonColorData_BigButton(btnColorType);
        sprBtnStart.spriteName = colorData.Item1;
        labelBtnStage[0].effectColor = colorData.Item2;
        labelBtnStage[1].color = colorData.Item2;
        labelBtnStage[1].effectColor = colorData.Item2;
    }

    private void SettingFreeCloverButton(bool bFree = true, bool bAction = false)
    {
        if (bFree == true)
        {
            startClover.spriteName = "icon_clover_infinity";
            startClover.width = 100;
            startClover.height = 100;
            startCloverShadow.SetActive(false);
        }
        else
        {
            startClover.spriteName = "icon_clover";
            startClover.width = 72;
            startClover.height = 74;
            startCloverShadow.SetActive(true);
        }

        if (bAction == true)
        {
            startClover.transform.localScale = Vector3.one * 0.5f;
            startClover.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    #endregion

    #region 광고 관련 함수
    /// <summary>
    /// 광고 관련 설정들 초기화
    /// </summary>
    protected virtual void InitPopupReadyAD()
    {
        //일시정지 팝업에서 스테이지 재시작을 누른 상태라면, 실제 데이터가 반영되기 전이기 떄문에 현재 실패카운트 +1 처리
        int failCountOffset = (UIPopupPause._instance == null) ? 0 : 1;

        //광고 출력 가능 상태인지 검사
        isCanWatchAD_AddTurn = Global.GameInstance.IsCanWatch_AD_AddTurn((tempData.isHardStage == 0), tempData.gameMode, failCountOffset);

        //광고 출력 가능 상태에 따른 UI 처리
        objSelectADRoot_AddTurn.SetActive(isCanWatchAD_AddTurn);
        if (isCanWatchAD_AddTurn == true)
            InitBtnSelectAD_AddTurn();
    }

    /// <summary>
    /// 턴추가 광고 선택 UI 초기화
    /// </summary>
    private void InitBtnSelectAD_AddTurn()
    {
        isSelectAD_AddTurn = false;

        //턴 카운트 텍스트 설정
        labelADTurnCount_AddTurn.text = Global.GameInstance.TurnCountByReadyAD().ToString();

        //광고 UI 변경
        SetBtnSelectAD_AddTurn();
    }

    /// <summary>
    /// 턴추가 광고 선택 UI 설정
    /// </summary>
    private void SetBtnSelectAD_AddTurn()
    {
        if (isSelectAD_AddTurn == true)
        {
            if (ADAddturn_birdRoutine != null)
            {   //파랑새 연출 정지
                StopCoroutine(ADAddturn_birdRoutine);
                ADAddturn_birdRoutine = null;
            }
            sprADBird_AddTurn.spriteName = "stage_icon_bluebird_01";
            sprADBird_AddTurn.transform.DOLocalMove(Vector3.zero, 0.15f);

            //말풍선 리워드 이미지 교체 정지
            if (ADAddturn_bubbleRoutine != null)
            {
                StopCoroutine(ADAddturn_bubbleRoutine);
                ADAddturn_bubbleRoutine = null;
            }
            arrADIconRoot_AddTurn[0].SetActive(true);
            arrADIconRoot_AddTurn[1].SetActive(false);

            //체크 표시 이미지 변경
            sprADCheck_AddTurn.spriteName = "ready_button_01_on";
        }
        else
        {
            if (ADAddturn_birdRoutine == null)  //파랑새 연출
                ADAddturn_birdRoutine = StartCoroutine(CoBirdAction_ADAddTurn());
            sprADBird_AddTurn.transform.DOLocalMove(new Vector3(-67f, 0.7f, 0f), 0.15f);

            if (ADAddturn_bubbleRoutine == null) //말풍선 리워드 이미지 교체 연출
                ADAddturn_bubbleRoutine = StartCoroutine(CoBubbleAction_ADAddTurn());

            //체크 표시 이미지 변경
            sprADCheck_AddTurn.spriteName = "ready_button_01_off";
        }
    }

    private IEnumerator CoBirdAction_ADAddTurn()
    {
        bool isMotion_1 = true;
        while (true)
        {
            sprADBird_AddTurn.spriteName = (isMotion_1 == true) ?
                "stage_icon_bluebird_01" : "stage_icon_bluebird_02";
            isMotion_1 = !isMotion_1;
            yield return new WaitForSeconds(0.4f);
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

    private void OnClickBtnSelectAD_AddTurn()
    {
        if (bCanTouch == false) return;

        isSelectAD_AddTurn = !isSelectAD_AddTurn;

        //광고 UI 변경
        SetBtnSelectAD_AddTurn();

        //스타트 버튼 설정
        SetStartButton();
    }
    #endregion
    
    #region 연속 모드 광고 지면
    
    private void FailResetAdBackupData()
    {
        if (eventChapter != null && eventChapter.type != (int) EVENT_CHAPTER_TYPE.FAIL_RESET) 
            return;

        //연속 모드 광고 지면 출력 가능한지 조건 체크를 위해.
        backupStageIndex = Global.stageIndex;
    }
    
    private bool IsFailResetAdOn() //연속 모드 광고 지면 출력 가능한지 조건 체크.
    {
        if (eventChapter == null || eventChapter.type != (int) EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            return false;
        }

        if (eventStageFail == false)
        {
            return false;
        }

        if (ServerRepos.LoginCdn.adEventChapterContinueOnOff == 0 && ServerRepos.AdTypeEventChapterContinue == 0)
        {
            return false;
        }

        failResetAdType = (AdManager.AdType) ServerRepos.AdTypeEventChapterContinue;
        if(AdManager.ADCheck(failResetAdType) == false)
        {
            return false;
        }
        
        int tempStageIndex = 0;
        if (eventChapter.counts != null)
        {
            for (int i = 0; i < eventChapter.counts.Count; i++)
            {
                if (eventChapter.counts[i] < backupStageIndex)
                {
                    tempStageIndex = eventChapter.counts[i];
                }
            }
        }
        
        if (backupStageIndex - tempStageIndex <= 1)
        {
            return false;
        }

        return true;
    }
    
    private void RequestFailResetAdAvailable() //연속 모드 광고 지면 조건 체크(PU/NPU, 실패 횟수).
    {
        onFailResetAd = true;
        bCanTouch = false;
        
        ServerAPI.ReqFailResetAdAvailable
        (
            (int)failResetAdType,
            (a) =>
            {
                if (a.isAvailable == true)
                {
                    StartCoroutine(CoWaitFailAction());
                }
                else
                {
                    ResetFailResetAd();
                }

                failResetAdOnCheckComplete = true;
            }
        );
    }
    
    IEnumerator CoWaitFailAction()
    {
        //실패 연출이 끝날 때 까지 대기.
        yield return new WaitUntil(() => blueBirdFailActionEnd);
        
        OpenFailResetAdPopup();
    }
    
    private void OpenFailResetAdPopup()
    {
        ClickBlocker.Make(0f);
        ManagerUI._instance.OpenPopup<UIPopupADView>
        (
            (popup) =>
            {
                StartCoroutine(OnTouch(0.3f));
                
                popup.SetEventChapterAd(failResetAdType, FailResetAdShowComplete, ResetFailResetAd);

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                    cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT_FAILRESET,
                    anm: "FAILRESET_AD_OPEN",
                    arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    backupStageIndex,
                    $"FAILRESET_AD_{(failResetAdType == AdManager.AdType.AD_17 ? "PU":"NonPU")}");
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }
        );
    }
    
    private void FailResetAdShowComplete() //광고 시청 완료 후 서버 통신.
    {
        //리워드 획득.
        ServerAPI.ReqFailResetAdReward
        (
            (int)failResetAdType,
            (a) =>
            {
                if (ServerContents.AdInfos[(int)failResetAdType].dailyLimit <= a.userAdInfo.usedCount)
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                        tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                        cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                        anm: $"TYPE_{((int)failResetAdType).ToString()}_DAILY_MAX",
                        arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                        backupStageIndex);
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                }

                //퀘스트 데이터 갱신
                QuestGameData.SetUserData();
                
                _failResetAdRewardResp = a;
                StartCoroutine(CoFailResetAdComplete());
            }
        );
    }
    
    private void ResetFailResetAd()
    {
        bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;
        
        onFailResetAd = false;
        backupStageIndex = 0;
        failResetAdType = AdManager.AdType.None;
    }
    
    //연속 모드 광고 지면 연출.
    IEnumerator CoFailResetAdComplete()
    {
        bCanTouch = false;
        
        //스테이지 값에 -1 적용. 원본 데이터는 연출 후 적용.
        var tempEventChapter = new ServerEventChapter()
        {
            eventIndex = _failResetAdRewardResp.userEventChapter.eventIndex,
            groupState = _failResetAdRewardResp.userEventChapter.groupState,
            stage = _failResetAdRewardResp.userEventChapter.stage - 1,
        };
        ServerRepos.UpsertEventChapter(tempEventChapter);
        EventChapterData.SetUserData();
        
        //이벤트 챕터 정보 저장
        eventChapter = ServerContents.EventChapters;

        //현재 스텝 설정.
        SetEventStep();

        //파랑새 이동 연출.
        yield return StartCoroutine(CoBlueBirdMoveAction());
        
        //데이터 갱신.
        ServerRepos.UpsertEventChapter(_failResetAdRewardResp.userEventChapter);
        EventChapterData.SetUserData();
        
        //현재 스텝 설정.
        SetEventStep();
        
        //스테이지 갱신.
        Global.SetStageIndex(_failResetAdRewardResp.userEventChapter.stage);
        yield return StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
        
        ResetFailResetAd();
    }

    private IEnumerator CoBlueBirdMoveAction()
    {
        for (int i = 1; i <= eventStep; i++)
        {
            //파랑새 이동 연출
            if (IsCanAction_BlueBirdMove() == true)
            {
                readyEventObj._star[i - 1].transform.localPosition = starPosition;
                EventStageAction_BlueBirdMove(i);
                yield return StartCoroutine(StarAction((i - 1)));
                PlayMoveSound();
                StartCoroutine(CoAction(0.5f, () => { ManagerSound.AudioPlay(AudioLobby.BoniStep); }));
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    private void EventStageAction_BlueBirdMove(int tempStep)
    {
        int stageCount = eventChapter.counts[0];

        //이벤트 그룹을 사용하는 이벤트의 경우, 그룹 상태를 이용해 현재 챕터의 스테이지 카운트를 구함.
        if (IsEventType_UsingEventGroup() == true)
        {
            int preStageCount = 0;
            for (int i = 0; i < eventChapter.counts.Count; i++)
            {
                if (i == (eventGroup - 1))
                {
                    stageCount = eventChapter.counts[(eventGroup - 1)] - preStageCount;
                    break;
                }
                else
                {
                    preStageCount += eventChapter.counts[i];
                }
            }
        }

        //파랑새가 이동할 위치 구함
        Vector3 birdPosition = Vector3.zero;
        if (tempStep > stageCount)
        {
            return;
        }
        else if (tempStep == stageCount) //스테이지를 모두 클리어했을 때, 마지막 위치로 이동
        {
            birdPosition = readyEventObj.giftRoot.localPosition;

            //스테이지 클리어를 했을 때 보상을 받는 이벤트 타입이라면, 팝업 종료
            if (IsEventType_GetRewardAtStageAllClear() == true)
                SetEventStageReviewPopup();
        }
        else
        {
            birdPosition = readyEventObj._step[tempStep].transform.localPosition;
            readyEventObj._step[tempStep].mainTexture = readyEventObj._textureStepOff;
            readyEventObj._step[tempStep].MakePixelPerfect();
        }
        birdPosition += readyEventObj._offsetPoint;

        //파랑새 이동 연출
        readyEventObj._texturePointShadow.transform.DOLocalMove(birdPosition, 0.5f).SetEase(Ease.OutQuint);
    }
    
    private void SetEventStageReviewPopup()
    {
        actionEventActionEnd += () => OnClickBtnClose();
        if (eventChapter.type == (int) EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            if (ManagerData._instance._eventChapterData._groupState == eventChapter.counts.Count)
                _callbackClose += () => ManagerUI._instance.OpenPopupRequestReview();
        }
        else
            _callbackClose += () => ManagerUI._instance.OpenPopupRequestReview();
    }

    #endregion

    #region 이벤트 스테이지 관련 함수

    #region 이벤트 스테이지 레디창 UI 초기화
    public void EventStageSetting(GameObject obj)
    {
        //창 다 열린 후 콜백.
        _callbackOpen += EventActionStart;

        //레디창 사이즈 조절.
        mainReadySprite.height = 1015;
        mainReadySprite.transform.localPosition += new Vector3(0f, -110f, 0f);
        readyBox.transform.localPosition = new Vector3(0f, -452.7f, 0f);

        //이벤트 오브젝트 설정.
        readyEventObj = NGUITools.AddChild(mainSprite.gameObject, obj).GetComponent<ReadyEvent>();
        readyEventObj.transform.localPosition = new Vector3(0f, 5f, 0f);
        SetBoniModel();
        boniLive2D.SetPosition(new Vector3(221f, -350f + (LanguageUtility.IsShowBuyInfo ? 60f : 0f), 0f) + readyEventObj.live2dOffset);

        //타이틀없앰.
        stage[0].gameObject.SetActive(false);

        //이벤트 챕터 정보 저장
        eventChapter = ServerContents.EventChapters;

        //유저 데이터에서 현재 몇 번째 그룹에 있는지 받아와야함(순차/스코어 모드는 그룹이 무조건 1).
        eventGroup = (eventChapter.type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET) ?
            1 : ManagerData._instance._eventChapterData._groupState;

        SetEventStep();

        //스코어 모드에서 사용하는 데이터 설정.
        SettingScoreModeData();

        //이벤트 정보에 따라 UI 초기화
        SettingEventReward();
        SettingStep();
        SettingEventFailResetEvent();
        SettingCollectEvent();
        SettingScoreEvent();

        //파랑새 디폴트 움직임.
        StartCoroutine(CoEvnet());
    }

    private void SetEventStep()
    {
        eventStep = ManagerData._instance._eventChapterData._state;
        //그룹 데이터를 사용하는 이벤트일 경우, 1번째 그룹 이상일 때 현재 스텝은 이전 그룹의 수만큼 빼 줘야함.
        if (IsEventType_UsingEventGroup())
        {
            if (eventGroup > 1)
            {
                eventStep -= eventChapter.counts[eventGroup - 2];
            }
        }
    }

    //보상 UI 초기화
    private void SettingEventReward()
    {
        //보상 설정
        if (eventChapter.type == (int)EVENT_CHAPTER_TYPE.SCORE)
        {
            rewardBox.gameObject.SetActive(false);
            rewardScoreRoot.SetActive(true);

            int rCnt = eventChapter.rewards.Count;
            for (int i = 0; i < scoreRewareBubble.Length; i++)
            {   
                //보상에 따라 말풍선 설정, 사용하지 않는 보상 말풍선은 비활성화.
                if (i < rCnt)
                {
                    scoreRewareBubble[i].gameObject.SetActive(true);
                    bool isGetReward = (i < scoreRewardIndex);
                    scoreRewareBubble[i].InitBubble(eventChapter.rewards[i], isGetReward);
                }
                else
                {
                    scoreRewareBubble[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            rewardBox.gameObject.SetActive(true);
            rewardScoreRoot.SetActive(false);

            int rCnt = eventChapter.rewards[eventGroup - 1].Count;
            rewardText[0].transform.localPosition = new Vector3(-5f - (rCnt * 30f), 33f, 0f);
            rewardIconRoot.transform.localPosition = new Vector3(180f - (rCnt * 60f), 0f, 0f);
            rewardBox.width = 10 + (rCnt * 60);

            for (int j = 0; j < rCnt; j++)
            {
                rewards[j].gameObject.SetActive(true);
                rewards[j].SetReward(eventChapter.rewards[eventGroup - 1][j]);
            }
        }
    }

    //발판 UI, 파랑새 위치 초기화
    private void SettingStep()
    {
        foreach (var step in readyEventObj._step)
        {
            step.mainTexture = readyEventObj._textureStepOff;
            step.MakePixelPerfect();
        }

        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            Vector3 birdPosition = readyEventObj._step[(eventStep - 1)].transform.localPosition;
            birdPosition += readyEventObj._offsetPoint;
            readyEventObj._texturePointShadow.transform.localPosition = birdPosition;
        }
        else
        {
            for (int i = 0; i < eventStep; i++)
            {
                if (i < readyEventObj._step.Count)
                {
                    ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                    eventStage.stageCount = i;
                    readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
                    readyEventObj._step[i].MakePixelPerfect();
                }
            }

            //이전스테이지 체크하기
            Vector3 birdPosition;
            if (beforeStage)
            {
                birdPosition = readyEventObj._step[(Global.stageIndex - 1)].transform.localPosition;
            }
            else
            {
                if (readyEventObj._step.Count < eventStep)
                {
                    birdPosition = readyEventObj._step[(Global.stageIndex - 1)].transform.localPosition;
                }
                else
                {
                    birdPosition = readyEventObj._step[(eventStep - 1)].transform.localPosition;
                }
            }


            birdPosition += readyEventObj._offsetPoint;
            readyEventObj._texturePointShadow.transform.localPosition = birdPosition;
        }

        for (int i = 0; i < (eventStep - 1); i++)
        {
            if (IsEventType_UsingStarUpperStep() == true)
                readyEventObj._star[i].gameObject.SetActive(true);
            readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
            readyEventObj._step[i].MakePixelPerfect();
        }

        if (SceneManager.GetActiveScene().name != "InGame")
            beforeStage = false;
    }

    //연속모드 UI 초기화
    private void SettingEventFailResetEvent()
    {
        if (readyEventObj.waveRoot != null)
        {
            readyEventObj.waveRoot.SetActive(true);
            readyEventObj.waveLabel.text = Global._instance.GetString("p_sc_5") + $" {eventGroup}/{ServerContents.EventChapters.counts.Count}";
            readyEventObj.waveLabelShadow.text = Global._instance.GetString("p_sc_5") + $" {eventGroup}/{ServerContents.EventChapters.counts.Count}";
        }
        
        int freeStage = eventChapter.freePlays[eventGroup - 1] - 1;
        if (freeStage >= 0 && freeStage <= readyEventObj._free.Count)
        {
            readyEventObj._free[freeStage].SetActive(true);
            //무료스테이지가 현재 진행중인 스테이지라면 무료 클로버 표시.
            if ((freeStage + 1) == eventStep)
            {
                SettingFreeCloverButton();
                bFreeStage = true;
                isFreeStage = true;
            }
        }
        isFreeStage = bFreeStage;
    }

    //순차모드 UI 초기화
    void SettingCollectEvent()
    {
        if (eventChapter.active <= 0 || eventChapter.type != (int)EVENT_CHAPTER_TYPE.COLLECT)
            return;

        int maxCount = 0;
        for (int i = 1; i < eventChapter.collectMaterials.Count; i += 2)
        {
            maxCount += eventChapter.collectMaterials[i];
        }

        int getCount = 0;
        foreach (var item in ServerRepos.EventStages)
        {
            if (item.eventIdx == Global.eventIndex)
            {
                for (int i = 1; i < eventChapter.collectMaterials[(item.stage - 1) * 2 + 1] + 1; i++)
                {
                    if ((item.materialCnt & (1 << i)) != 0)
                    {
                        getCount++;
                    }
                }
            }
        }

        List<int> materialTypList = new List<int>();
        for (int i = 0; i < readyEventObj._step.Count; i++)
        {
            int tempGetCount = 0;

            foreach (var item in ServerRepos.EventStages)
            {
                if (item.eventIdx == Global.eventIndex && item.stage == (i + 1))
                {
                    for (int j = 1; j < eventChapter.collectMaterials[(item.stage - 1) * 2 + 1] + 1; j++)
                    {
                        if ((item.materialCnt & (1 << j)) != 0)
                        {
                            tempGetCount++;
                        }
                    }
                }
            }

            if (materialTypList.Contains(eventChapter.collectMaterials[i * 2]) == false && eventChapter.collectMaterials[i * 2] > 0) materialTypList.Add(eventChapter.collectMaterials[i * 2]);
            {
                if (eventChapter.collectMaterials[i * 2] > 0)
                {
                    ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                    eventStage.materialRoot.SetActive(true);
                    eventStage.SetGetMaterial(eventChapter.collectMaterials[i * 2 + 1], tempGetCount, eventChapter.collectMaterials[i * 2]);
                }
            }
        }

        if (materialTypList.Count == 1 && maxCount > 0)
        {
            readyEventObj.collectRoot.SetActive(true);
            readyEventObj.maxcountLabel.text = maxCount.ToString();
            readyEventObj.maxcountLabelShadow.text = maxCount.ToString();

            readyEventObj.getCountLabel.text = getCount + "/";
            readyEventObj.getCountLabelShadow.text = getCount + "/";

            string fileName = "mt_" + materialTypList[0];
            readyEventObj.collectObj.SettingTextureScale(80, 80);
            readyEventObj.collectObj.LoadCDN(Global.gameImageDirectory, "IconMaterial/", fileName);
        }
    }

    //스코어모드 UI 초기화
    private void SettingScoreEvent()
    {
        if (eventChapter.active <= 0 || eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE)
            return;

        int rewardFlowerCnt = eventChapter.rewardFlowerCount.Count;
        int allScoreBadgeCount = eventChapter.rewardFlowerCount[rewardFlowerCnt - 1];

        //스텝에 있는 뱃지 설정
        for (int i = 0; i < readyEventObj._step.Count; i++)
        {
            int checkStage = (i + 1);
            int flowerLevel = (dicEventStageFlower.ContainsKey(checkStage) == false) ? 0 : dicEventStageFlower[checkStage].prevLevel;

            ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
            eventStage.scoreRoot.SetActive(true);
            eventStage.SetGetScoreBadge(flowerLevel);
        }

        //전체 목표 뱃지 설정
        readyEventObj.scoreRoot.SetActive(true);
        string maxCountString = string.Format("/{0}", allScoreBadgeCount);
        string getCountString = getScoreBadgeCount_Current.ToString();
        for (int i = 0; i < readyEventObj.maxScoreBadgeLabel.Length; i++)
            readyEventObj.maxScoreBadgeLabel[i].text = maxCountString;
        for (int i = 0; i < readyEventObj.getScoreBadgeLabel.Length; i++)
            readyEventObj.getScoreBadgeLabel[i].text = getCountString;

        //보상 UI들 위치 설정
        int rewardCount = eventChapter.rewardFlowerCount.Count;
        for (int i = 0; i < rewardCount; i++)
        {
            int lineIdx = (eventChapter.rewardFlowerCount[i] - 1);
            Vector3 rewardBubblePos = Vector3.zero;

            if (i < (rewardCount - 1))
            {
                //보상 받을 수 있는 뱃지 카운트 텍스트 설정 및 라인 굵기 설정, 말풍선 표시.
                if (lineIdx < 0 || readyEventObj.listScoreRewardLine.Count > lineIdx)
                {
                    ReadyEvent.ScoreModeRewardLine scoreModeRewardLine = readyEventObj.listScoreRewardLine[lineIdx];

                    //텍스트 설정
                    scoreModeRewardLine.textRewardCount.text = eventChapter.rewardFlowerCount[i].ToString();
                    scoreModeRewardLine.textRewardCount.gameObject.SetActive(true);

                    //라인 설정
                    scoreModeRewardLine.spriteRewardLine.transform.localScale = Vector3.one;
                    scoreModeRewardLine.spriteRewardLine.transform.localPosition -= new Vector3(0f, -0.5f, 0f);
                    scoreModeRewardLine.spriteRewardLine.width = 3;
                    scoreModeRewardLine.spriteRewardLine.alpha = 1f;

                    //보상 말풍선 위치 설정
                    rewardBubblePos = scoreModeRewardLine.textRewardCount.transform.position;
                }
            }
            else
            {
                //마지막 보상은 젤 마지막 줄에 설정
                readyEventObj.maxBadgeText.text = eventChapter.rewardFlowerCount[i].ToString();

                //보상 표시 말풍선 위치 설정
                rewardBubblePos = readyEventObj.maxBadgeText.transform.position;
            }

            //보상 표시 말풍선 설정
            if (scoreRewareBubble.Length > i)
            {
                scoreRewareBubble[i].transform.position = rewardBubblePos;
                scoreRewareBubble[i].transform.localPosition += Vector3.up * 55f;
            }
        }

        //보상 프로그래스 설정
        readyEventObj.scoreProgressBar.value = (getScoreBadgeCount_Prev == 0) ? 0 : (((float)getScoreBadgeCount_Prev / allScoreBadgeCount));

        //연출이 나오지 않는 상태라면, 초기화 단계에서 바로 이벤트 꽃 정보 저장
        if (eventStageClear == false)
            SettingEventStageFlower();
    }

    //파랑새 디폴트 액션
    private IEnumerator CoEvnet()
    {
        float initPos = readyEventObj._texturePoint.transform.localPosition.y;
        while (UIPopupReady._instance != null)
        {
            readyEventObj._texturePoint.transform.localPosition
                = new Vector3(readyEventObj._texturePoint.transform.localPosition.x, initPos + Mathf.Abs(Mathf.Cos(Time.time * 8f) * 6f), 0f);
            yield return null;
        }
        yield return null;
    }
    #endregion

    #region 이벤트 스테이지 관련 데이터 업데이트
    //이벤트 스테이지에서 획득한 꽃 데이터 저장.
    private void SettingEventStageFlower()
    {
        if (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE)
            return;

        dicEventStageFlower.Clear();
        int eventStageCnt = eventChapter.counts[(eventChapter.counts.Count - 1)];
        for (int i = 0; i < eventStageCnt; i++)
        {
            int stage = (i + 1);
            int findIndex = ServerRepos.EventStages.FindIndex((x => x.eventIdx == Global.eventIndex && x.stage == stage));
            int flowerLevel = (findIndex == -1) ? 0 : ServerRepos.EventStages[findIndex].flowerLevel;

            EventStageFlowerData flowerData = new EventStageFlowerData()
            {
                prevLevel = flowerLevel,
                currentLevel = flowerLevel
            };
            dicEventStageFlower.Add(stage, flowerData);
        }
    }

    //스코어 모드에서 사용하는 데이터들 설정
    private void SettingScoreModeData()
    {
        if (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE)
            return;
        
        //현재 이벤트의 스테이지 리스트 가져옴.
        List<ServerEventStage> listEventStage = new List<ServerEventStage>();
        foreach (var item in ServerRepos.EventStages)
        {
            if (item.eventIdx == Global.eventIndex)
            {
                listEventStage.Add(item);
            }
        }

        getScoreBadgeCount_Prev = 0;
        getScoreBadgeCount_Current = 0;

        //꽃 카운트 계산
        int stageCount = eventChapter.counts[0];
        for (int i = 0; i < stageCount; i++)
        {
            int checkStage = (i + 1);
            var stage = listEventStage.Find(x => x.stage == checkStage);

            int currentLevel = (stage == null) ? 0 : stage.flowerLevel;
            int prevLevel = currentLevel;

            //갱신된 꽃 정보를 딕셔너리에 저장
            if (dicEventStageFlower.ContainsKey(checkStage) == true)
            {
                dicEventStageFlower[checkStage].currentLevel = currentLevel;
                prevLevel = dicEventStageFlower[checkStage].prevLevel;
            }
            else
            {
                EventStageFlowerData flowerData = new EventStageFlowerData()
                {
                    prevLevel = currentLevel,
                    currentLevel = currentLevel,
                };
                dicEventStageFlower.Add(checkStage, flowerData);
            }

            //전체 꽃 카운트 갱신
            getScoreBadgeCount_Prev += prevLevel;
            getScoreBadgeCount_Current += currentLevel;
        }

        //보상 상태 설정
        scoreRewardState = ManagerData._instance._eventChapterData._groupState;
        scoreRewardIndex = scoreRewardState - 1;
        
        //연출 설정
        if (eventStageClear == true)
        {
            //모든 보상 목표를 다 달성했는지 검사.
            if (scoreRewardState == eventChapter.rewardFlowerCount.Count
                && eventChapter.rewardFlowerCount[scoreRewardIndex] <= getScoreBadgeCount_Current)
            {
                canPlayAction_CompleteGetReward = true;
            }
            else
            {
                canPlayAction_CompleteGetReward = false;
            }
        }
    }

    //이벤트 연출(스테이지 클리어/실패)가 끝난 후, 데이터 싱크를 맞춰주는 함수.
    IEnumerator ChangeEventAction(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        EventChapterData.SetUserData(Global.eventIndex);
        SetEventStep();
        ChangeEventButtonClover();
        SettingEventStageFlower();

        //현재 위치한 스테이지의 발판도 켜줌
        if (ServerContents.EventChapters.type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET
            && readyEventObj._step.Count >= eventStep)
        {
            ReadyEventStage eventStage = readyEventObj._step[(eventStep - 1)].GetComponent<ReadyEventStage>();
            eventStage.stageCount = (eventStep - 1);
            readyEventObj._step[(eventStep - 1)].mainTexture = readyEventObj._textureStepOn;
            readyEventObj._step[(eventStep - 1)].MakePixelPerfect();
        }
    }

    IEnumerator EventActionEnd(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (actionEventActionEnd != null)
            actionEventActionEnd.Invoke();
    }

    //보상 업데이트
    private void UpdateReward()
    {
        // 스코어모드는 그룹번호가 0, 1, 2 순이고, 나머지 다른 타입은 1,2,3 순서기 때문에 조정해줘야함
        int eventGroupIdx = eventChapter.type == (int)EVENT_CHAPTER_TYPE.SCORE ? ManagerData._instance._eventChapterData._groupState - 1 : eventGroup - 1;

        //받는 보상 중 선물상자가 있는 경우, 로비에 생성.
        for (int i = 0; i < eventChapter.rewards[eventGroupIdx].Count; i++)
        {
            int type = eventChapter.rewards[eventGroupIdx][i].type;
            if (type >= 100)
            {
                ManagerLobby._instance.ReMakeGiftbox();
                break;
            }
        }

        //그로씨
        for (int i = 0; i < eventChapter.rewards[eventGroupIdx].Count; i++)
        {
            Reward rewards = eventChapter.rewards[eventGroupIdx][i];

            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                rewards.type,
                rewards.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                );
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                           (
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT,
                               $"EVENT_CHAPTER_{Global.eventIndex}_{eventGroupIdx + 1}_CLEAR",
                               ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                           );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        //이벤트 전체 첫클리어시
        var totalCleared = ServerRepos.EventChapters.stage > eventChapter.counts[eventChapter.counts.Count - 1];
        if (totalCleared)
        {
            int totalPlayCount = 0;
            foreach (var item in ServerRepos.EventStages)
                if (item.eventIdx == Global.eventIndex)
                    totalPlayCount += item.play;

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                            (
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.EVENT,
                                Global.eventIndex.ToString(),
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                            );
            achieve.L_NUM1 = totalPlayCount;
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
    }
    #endregion

    #region 이벤트 스테이지 연출
    private void PlayMoveSound(bool isFailed = false)
    {
        AudioLobby voice = (isFailed == false) ? GetMoveCharacterVoice() : GetFailCharacterVoice();
        if (voice != AudioLobby.NO_SOUND)
        {
            ManagerSound.AudioPlay(voice);
        }
    }

    void EventActionStart()
    {
        //이벤트 성공 실패에 따라 움직이는 연출.
        if (eventStageClear == true)
        {
            Invoke("BlueBirdMoveAction", 0.3f);
        }
        else if (eventStageFail == true)
        {
            if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
                Invoke("BlueBirdFailAction", 0.3f);
        }
    }

    #region 스테이지 클리어 연출
    private void BlueBirdMoveAction()
    {
        //연출 시 터치불가.
        this.bCanTouch = false;

        //파랑새 이동 연출
        if (IsCanAction_BlueBirdMove() == true)
        {
            EventStageAction_BlueBirdMove();
            StartCoroutine(StarAction((eventStep - 1)));
            PlayMoveSound();
            StartCoroutine(CoAction(0.5f, () => { ManagerSound.AudioPlay(AudioLobby.BoniStep); }));
        }

        //스코어 모드 연출
        StartCoroutine(ScoreModeAction());

        //연출 후 데이터 변경.
        StartCoroutine(ChangeEventAction(0.5f));

        //연출 후 터치가능.
        StartCoroutine(OnTouch(0.7f));

        //모든 연출 처리와 데이터 처리가 끝나고 불릴 액션들 호출.
        StartCoroutine(EventActionEnd(0.7f));
        
        StartCoroutine(CoDelayEventChapterActionCompleteFlag(1f));
    }

    private bool IsCanAction_BlueBirdMove()
    {
        if (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE)
        {
            return true;
        }
        else
        {
            int lastStage = ManagerData._instance._eventChapterData._state;
            int checkStage = (stageIndex <= lastStage) ? stageIndex : (stageIndex - 1);
            int flowerLevel = (dicEventStageFlower.ContainsKey(checkStage) == false) ? 0 : dicEventStageFlower[checkStage].prevLevel;

            //이미 꽃을 피운 상태라면, 스테이지 재 플레이한 상태이므로 이동 액션을 하지 않음.
            if (flowerLevel > 0)
                return false;
            else
                return true;
        }
    }

    private void EventStageAction_BlueBirdMove()
    {
        int stageCount = eventChapter.counts[0];

        //이벤트 그룹을 사용하는 이벤트의 경우, 그룹 상태를 이용해 현재 챕터의 스테이지 카운트를 구함.
        if (IsEventType_UsingEventGroup() == true)
        {
            int preStageCount = 0;
            for (int i = 0; i < eventChapter.counts.Count; i++)
            {
                if (i == (eventGroup - 1))
                {
                    stageCount = eventChapter.counts[(eventGroup - 1)] - preStageCount;
                    break;
                }
                else
                {
                    preStageCount += eventChapter.counts[i];
                }
            }
        }

        //파랑새가 이동할 위치 구함
        Vector3 birdPosition = Vector3.zero;
        if (eventStep > stageCount)
        {
            return;
        }
        else if (eventStep == stageCount) //스테이지를 모두 클리어했을 때, 마지막 위치로 이동
        {
            birdPosition = readyEventObj.giftRoot.localPosition;

            //스테이지 클리어를 했을 때 보상을 받는 이벤트 타입이라면, 팝업 종료
            if (IsEventType_GetRewardAtStageAllClear() == true)
                SetEventStageReviewPopup();
        }
        else
        {
            birdPosition = readyEventObj._step[eventStep].transform.localPosition;
            readyEventObj._step[eventStep].mainTexture = readyEventObj._textureStepOff;
            readyEventObj._step[eventStep].MakePixelPerfect();
        }
        birdPosition += readyEventObj._offsetPoint;

        //파랑새 이동 연출
        readyEventObj._texturePointShadow.transform.DOLocalMove(birdPosition, 0.5f).SetEase(Ease.OutQuint);
    }

    private IEnumerator StarAction(int stepNum)
    {
        yield return new WaitForSeconds(0.1f);

        //별 연출
        if (IsEventType_UsingStarUpperStep() == true)
        {
            readyEventObj._star[stepNum].gameObject.SetActive(true);
            readyEventObj._star[stepNum].color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => readyEventObj._star[stepNum].color, x => readyEventObj._star[stepNum].color = x, 1f, 0.2f).SetEase(Ease.InQuint);
            readyEventObj._star[stepNum].transform.localScale = Vector3.one * 1.3f;
            readyEventObj._star[stepNum].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuint);
            yield return new WaitForSeconds(0.3f);
        }

        //발판 연출
        readyEventObj._step[stepNum].mainTexture = readyEventObj._textureStepOn;
        readyEventObj._step[stepNum].MakePixelPerfect();
        readyEventObj._step[stepNum].transform.DOShakePosition(0.5f, 5f, 20, 90f, false, false);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
    }

    private IEnumerator ScoreModeAction()
    {
        //스코어 모드 이벤트 스테이지를 클리어 해야만 연출 등장함.
        if (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE || eventStageClear == false)
            yield break;

        int rewardFlowerCnt = eventChapter.rewardFlowerCount.Count;
        int allScoreBadgeCount = eventChapter.rewardFlowerCount[rewardFlowerCnt - 1];

        bool isChangeData = false;
        for (int i = 0; i < readyEventObj._step.Count; i++)
        {
            int checkStage = (i + 1);
            int prevFlowerLevel = (dicEventStageFlower.ContainsKey(checkStage) == false) ? 0 : dicEventStageFlower[checkStage].prevLevel;
            int currentFlowerLevel = (dicEventStageFlower.ContainsKey(checkStage) == false) ? 0 : dicEventStageFlower[checkStage].currentLevel;
            
            //스코어 벳지 연출.
            if (prevFlowerLevel != currentFlowerLevel)
            {
                isChangeData = true;
                ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                eventStage.SetGetScoreBadge(prevFlowerLevel, currentFlowerLevel);
            }
        }

        //이전의 상태와 달라졌을때만 연출 시켜줌.
        if (isChangeData == true)
        {
            //보상 프로그래스 연출
            float targetValue = ((float)getScoreBadgeCount_Current / allScoreBadgeCount);
            DOTween.To(() => readyEventObj.scoreProgressBar.value, x => readyEventObj.scoreProgressBar.value = x, targetValue, 0.2f);

            if (eventChapter.rewardFlowerCount.Count > scoreRewardIndex)
            {
                //보상을 획득한 상태인지 검사
                if (eventChapter.rewardFlowerCount[scoreRewardIndex] <= getScoreBadgeCount_Current)
                {
                    //획득한 보상 체크 표시
                    if (scoreRewareBubble.Length > scoreRewardIndex)
                    {
                        scoreRewareBubble[scoreRewardIndex].SetCheckObj(true);
                        
                        //팝업이 닫히는 조건이라면, 연출이후 팝업 닫히도록 설정
                        if (canPlayAction_CompleteGetReward == true)
                            SetEventStageReviewPopup();
                    }
                }
            }
        }
    }
    #endregion

    #region 스테이지 실패 연출
    private void BlueBirdFailAction()
    {
        //현재 플레이어가 첫번째 칸이라면 연출 안 함.
        if (eventStep == 1)
            return;

        //연출 시 터치불가.
        this.bCanTouch = false;

        //별 세팅.
        for (int i = 0; i < (eventStep - 1); i++)
        {
            StarFailAction(i);
        }

        PlayMoveSound(true);

        //파랑새 위치이동 & 회전.
        StartCoroutine(CoBluebirdFailAction());
        eventStep = 1;

        //연출 후 데이터 변경
        StartCoroutine(ChangeEventAction(1.5f));

        //연출 후 터치가능.
        StartCoroutine(OnTouch(1.7f));
        
        StartCoroutine(CoDelayEventChapterActionCompleteFlag(2f));
    }

    private void StarFailAction(int stepNum)
    {
        readyEventObj._step[stepNum].mainTexture = readyEventObj._textureStepOff;
        readyEventObj._step[stepNum].MakePixelPerfect();

        if (IsEventType_UsingStarUpperStep() == true)
        {
            starPosition = readyEventObj._star[stepNum].transform.localPosition;
            
            DOTween.ToAlpha(() => readyEventObj._star[stepNum].color, x => readyEventObj._star[stepNum].color = x, 0f, 0.4f).SetEase(Ease.InQuint);
            readyEventObj._star[stepNum].transform.DOLocalMoveY(50f, 0.5f);
        }
        ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
    }

    IEnumerator CoBluebirdFailAction()
    {
        //첫번째 스텝 위치.
        Vector3 pos = pos = readyEventObj._step[0].transform.localPosition;
        pos += readyEventObj._offsetPoint;

        yield return new WaitForSeconds(0.5f);
        readyEventObj._texturePointShadow.transform.DOLocalMove(pos, 1.5f).SetEase(Ease.OutQuint)
            .onComplete = () => { blueBirdFailActionEnd = true; };
        readyEventObj._texturePoint.flip = UITexture.Flip.Horizontally;
        yield return new WaitForSeconds(0.7f);
        ManagerSound.AudioPlay(AudioLobby.BoniStep);
        yield return new WaitForSeconds(0.1f);
        readyEventObj._texturePoint.flip = UITexture.Flip.Nothing;
    }
    #endregion

    #endregion

    //이벤트 스테이지 변경
    public void ChangeEventStage(int tempStageNumber)
    {
        if (tempStageNumber >= 0 && tempStageNumber < readyEventObj._step.Count)
        {
            if (this.bCanTouch == false)
                return;

            this.bCanTouch = false;

            Global.SetStageIndex(tempStageNumber + 1);
            StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
        }
    }

    //이벤트 스테이지 모드 설명 팝업
    private void OnClickBtnEventModeInfo()
    {
        if (Global.GameType == GameType.EVENT)
        {
            string infoText = "";
            EVENT_CHAPTER_TYPE eventType = (EVENT_CHAPTER_TYPE)ServerContents.EventChapters.type;
            switch (eventType)
            {
                case EVENT_CHAPTER_TYPE.FAIL_RESET:
                    infoText = Global._instance.GetString("n_s_20");
                    break;
                case EVENT_CHAPTER_TYPE.COLLECT:
                    infoText = Global._instance.GetString("n_s_21");
                    break;
                case EVENT_CHAPTER_TYPE.SCORE:
                    infoText = Global._instance.GetString("n_s_50");
                    break;
            }

            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), infoText, false);
            popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
        }
    }

    //팝업 사라질 떄 이벤트 스테이지 처리
    private void OnDestroy_EventStage()
    {
        if (Global.GameType != GameType.EVENT) return;

        CdnEventChapter cdnEvent = ServerContents.EventChapters;
        if (IsEventType_GetRewardAtStageAllClear() == true)
        {
            if (eventGroupClear)
            {
                if (cdnEvent.active > 0)
                {
                    if (eventChapter.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET && cdnEvent.counts.Count >= ManagerData._instance._eventChapterData._groupState)
                        ManagerUI._instance.OpenPopupReadyStageEvent();
                }
                else
                {
                    if (cdnEvent.counts.Count > 1 && cdnEvent.counts.Count >= ManagerData._instance._eventChapterData._groupState)
                    {
                        ManagerUI._instance.OpenPopupStageAction(false);
                    }
                }
            }
        }

        if (eventStageClear || eventGroupClear || eventStageFail)
        {
            EventChapterData.SetUserData(Global.eventIndex);
        }
        eventStageClear = false;
        eventGroupClear = false;
        eventStageFail = false;
    }

    //발판위에 별 아이콘을 표시하는 이벤트 타입인지 검사
    private bool IsEventType_UsingStarUpperStep()
    {
        if (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE)
            return true;
        else
            return false;
    }

    //그룹을 사용하는 이벤트 타입인지 검사
    private bool IsEventType_UsingEventGroup()
    {
        return (eventChapter.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET);
    }

    //보상을 받는 조건이 스테이지 클리어 여부인 이벤트 타입인지 검사
    private bool IsEventType_GetRewardAtStageAllClear()
    {
        return (eventChapter.type != (int)EVENT_CHAPTER_TYPE.SCORE);
    }
    #endregion

    #region 이벤트 연출 미리보기    
    public void EventStageSettingByPreviewEditor(ReadyEvent obj)
    {
        if (readyEventObj != null)
        {
            Destroy(readyEventObj.gameObject);
            readyEventObj = null;
        }

        //레디창 사이즈 조절.
        mainSprite.height = 1015 + (LanguageUtility.IsShowBuyInfo ? -90 : 0);
        mainSprite.transform.localPosition = new Vector3(0f, -100f, 0f);
        readyBox.transform.localPosition = new Vector3(0f, 65.3f, 0f);

        //타이틀없앰.
        stage[0].gameObject.SetActive(false);

        //이벤트 오브젝트 설정.
        readyEventObj = NGUITools.AddChild(mainSprite.gameObject, obj.gameObject).GetComponent<ReadyEvent>();
        readyEventObj.transform.localPosition = new Vector3(0f, 105f + (LanguageUtility.IsShowBuyInfo ? -26 : 0), 0f);

        //더미로 값 설정.
        SettingEventPriview(readyEventObj);
    }

    private void SettingEventPriview(ReadyEvent readyObj)
    {
        //보상창 켜줌.
        if (rewardBox.gameObject.activeInHierarchy == false)
        {
            rewardBox.gameObject.SetActive(true);
            rewardText[0].transform.localPosition = new Vector3(-5f - (3 * 30f), 33f, 0f);
            rewardIconRoot.transform.localPosition = new Vector3(180f - (3 * 60f), 0f, 0f);
            rewardBox.width = 10 + (3 * 60);
            for (int j = 0; j < 3; j++)
            {
                rewards[j].gameObject.SetActive(true);
            }
        }
    }
    
    private void OnClickBtnNoyBoosting()
    {
        if (bCanTouch == false)
            return;
        
        if (!ManagerNoyBoostEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
            return;
        }

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        string message = Global._instance.GetString("noyboost_i_2")
            .Replace("[n]", ManagerNoyBoostEvent.instance.StartStage.ToString())
            .Replace("[m]", ManagerNoyBoostEvent.instance.EndStage.ToString());
        popupSystem.InitSystemPopUp(Global._instance.GetString("noyboost_i_1"), message, false, null);
        popupSystem.textCenter.applyGradient = false;
    }
    #endregion
}
