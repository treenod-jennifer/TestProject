using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Protocol;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;
using UnityEngine.Networking;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokoAddressable;

public enum UIState
{
    SHOW,       //UI가 보이는 상태
    SHOWING,    //UI가 보여지고 있는 상태
    HIDE,       //UI가 숨겨진 상태
    HIDING,     //UI가 숨겨지고 있는 상태
}

public enum TypePopupDiary
{
    eNone,
    eMission,
    eStamp,
    eStorage,
    eCostume,
    eGuest,
}

public enum TypeShowUI
{
    eTopUI,
    eBackUI,
    eAll,
}

public enum BtnColorType
{
    green,
    yellow,
    gray,
}

public class ManagerUI : MonoBehaviour
{
    public static ManagerUI _instance = null;
    [System.NonSerialized] public Transform _transform = null;

    public Camera _camera;
    public GameObject _objTouchFlower;
    public GameObject _objEventIconEffect;
    
    public GameObject anchorCenter;
    public UILobbyButtonListManager anchorTopLeft;
    public UILobbyButtonListManager anchorTopRight;
    public UILobbyButtonListManager anchorBottomLeft;
    public GameObject anchorBottomRight;
    public GameObject anchorBottomCenter;
    public GameObject anchorTopCenter;
    public GameObject topCenterPanel;
    public GameObject stageNewIcon;
    public GameObject buttonDiary;
    public GameObject diaryNewIcon;
    public GameObject diaryEventIcon;
    public UISprite diaryIcon;

    public GameObject saleIcon_clover;
    public GameObject saleIcon_wing;
    public GameObject saleIcon_coin;
    public GameObject saleIcon_jewel;
    public GameObject newIcon_clover;
    public GameObject newIcon_wing;
    public GameObject newIcon_coin;
    public GameObject newIcon_jewel;
    public GameObject cloverSendTimeEventIcon;
    public List<UIWidget> iphoneXWidget = new List<UIWidget>();
    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    //오른쪽에 있는 ui들.
    [SerializeField] private UIGrid rightDownGrid;
    [SerializeField] private GameObject optionButton;
    [SerializeField] private GameObject socialButton;
    [SerializeField] private GameObject rankingButton;
    [SerializeField] private GameObject messageButton;
    
    //우측 메뉴 관련 UI
    [SerializeField] private UISprite menuBg;
    [SerializeField] private GameObject rootMenuAlarm;
    [SerializeField] private GameObject menuNewIcon;
    [SerializeField] private GameObject menuEventIcon;
    [SerializeField] private GameObject menuInviteIcon;
    [SerializeField] private GameObject menuWakeUpBubble;
    [SerializeField] private List<GameObject> menuGridChild;
    private bool bCanMenuClick = true;

    public GameObject MessageButton
    {
        get => messageButton;
    }

    public GameObject socialEventIcon;

    public AnimationCurve popupScaleAnimation;
    public AnimationCurve popupAlphaAnimation;

    //플레이 버튼 UI.
    public UILabel[] _labelPlay;
    public UILabel[] _labelStage;
    public UISprite _rootStage;
    public GameObject _btnPlay;
    public GameObject _btnComingSoon;
    public GameObject _btnSingleRoundEvent;
    public GenericReward _genericReward_SindleRountEvent;

    public GameObject _btnBlossom;
    public UILabel[] _btnBlossomText;
    public UISprite _btnBlossomFlower;


    public GameObject _coinEventRoot;
    public UILabel _coinEventLabel;

    public PlayButtonDecos _playButtonDeco;

    public GameObject _cloverSupplyEventRoot;
    public UILabel[] _cloverSupplyLabel;
    public UILabel[] _cloverSupplyTimeLabel;

    public UILabel[] _labelStageFlower;
    public UISprite _rootStageFlower;

    public GameObject _messageNewIcon;
    public UILabel _messageNewCount;

    public GameObject objClover;
    public GameObject objWing;

    public GameObject objExp;
    public GameObject objStar;

    // 상단 재화 관련
    public UISprite _CloverSprite;
    public UISprite _CloverInfinitySprite;
    public UISprite _WingInfinitySprite;
    public UISprite _StarSprite;
    public UISprite _DiaSprite;
    public UILabel _labelClover;
    public UILabel _labelCloverS;

    public UILabel _labelWing;
    public UILabel _labelExp;

    public UILabel _labelCherry;
    public UILabel _labelJewer;
    public UILabel _labelStar;
    public UILabel _labelCloverTime;
    int tempClover = -1;
    int tempCherry = -1;
    int tempJewer = -1;
    int tempStar = -1;
    int tempWing = -1;
    int tempExp = -1;

    // 랭킹 등급
    public UIUrlTexture _textureClass;

    // prefab들 등록
    public GameObject _objPopupPokoYuraInfo;
    public GameObject _objPopupNotice;
    public GameObject _objPopupRankUp;
    public GameObject _objPopupTimeMission;
    public GameObject _objPopupInviteSmall;
    public GameObject _objPopupRequestSmall;
    public GameObject _objPopupNoMoreMove;
    public GameObject _objLobbyChat;
    public GameObject _objLobbyRewardChat;
    public GameObject _objLobbyMission;
    public GameObject _objPopupConfirm;
    public GameObject _objPopupInputText;
    public GameObject _objPopupSendItemToSocial;
    public GameObject _ObjHeartEffect;
    public GameObject _objPopupInputColorPad;
    public GameObject _objHeartGetEffect;
    public GameObject _objPopupPanelEmotionIcon;


    #region 라이브 2D 모델 프리팹.

    public GameObject _objBoni;
    public GameObject _objCoco;
    public GameObject _objBlueBird;
    public GameObject _objPeng;
    public GameObject _objJeff;
    public GameObject _objAlphonse;
    public GameObject _objNoi;

    #endregion

    //상단, 하단 UI.
    public UIPanel topUiPanel;
    public UIPanel backUiPanel;

    //화면 사이즈 영역 구할때 사용.
    public UIRoot uiRoot;

    [HideInInspector] public Vector2 uiScreenSize = Vector2.zero;

    // 월드에 있는 재료를 획득할때 날아갈 타켓
    public Transform _transformDiary;

    /////////////////////////////////////////////////////////////////////////////////////////
    // 생선됨 모든 팝업이 생성되어 등록되고 지워짐
    public List<UIPopupBase> _popupList = new List<UIPopupBase>();

    private UIButtonAdventure btnAdventure = null;

    [HideInInspector] public bool bTouchTopUI = true;
    [HideInInspector] public bool activeClickBlocker = false;

    [SerializeField] private UIItemScrollbar scrollbarRight;
    [SerializeField] private UIItemSidebar sidebarLeft;

    public UIItemScrollbar ScrollbarRight
    {
        get { return scrollbarRight; }
    }

    public UIItemSidebar SidebarLeft
    {
        get { return sidebarLeft; }
    }

    public Dictionary<CurrencyType, int> dicActionCoin = new Dictionary<CurrencyType, int>();
    public Dictionary<CurrencyType, int> dicActionJewel = new Dictionary<CurrencyType, int>();

    //ServerTime Prefab
    [SerializeField] private GameObject serverTime;

    private const int Default_Depth_High = 3;
    
    public const float SpawnEffectTime = 0.65f;

    public enum CurrencyType
    {
        READY_ITEM,
    }

    //로비 패널
    [SerializeField] private UIPanel lobbyUIPanel;

    #region 로비 UI 숨김 처기 관련

    private UIState uiState = UIState.SHOW;

    public UIState UIState
    {
        set
        {
            if (uiState == value) return; //상태가 동일한지
            if (uiState == UIState.SHOWING || uiState == UIState.HIDING) return; //UI 사라지는 연출 중인지

            switch (value)
            {
                case UIState.SHOW:
                    if (hideUIRoutine != null)
                    {
                        StopCoroutine(hideUIRoutine);
                        hideUIRoutine = null;
                    }

                    StartCoroutine(CoShowLobbyUI());
                    break;
                case UIState.HIDE:
                    hideUIRoutine = StartCoroutine(CoHideLobbyUI());
                    break;
            }
        }
    }

    private Coroutine hideUIRoutine = null;

    #endregion

    private const float UI_SHOW_TIME = 0.4f;
    private const float UI_HIDE_TIME = 0.2f;

    public void ActiveLobbyUI()
    {
        lobbyUIPanel.alpha = 1;
        btnAdventure?.SetEnable(true);
    }

    public void InactiveLobbyUI()
    {
        lobbyUIPanel.alpha = 0;
        btnAdventure?.SetEnable(false);
    }

    private void ShowLobbyUI()
    {
        lobbyUIPanel.alpha = 1;
        uiState = UIState.SHOW;
    }
    
    private IEnumerator CoShowLobbyUI()
    {
        uiState = UIState.SHOWING;
        btnAdventure?.SetEnable(true);
        DOTween.To(() => lobbyUIPanel.alpha, x => lobbyUIPanel.alpha = x, 1, UI_SHOW_TIME);
        yield return new WaitForSeconds(UI_SHOW_TIME);
        ShowLobbyUI();
    }

    private IEnumerator CoHideLobbyUI()
    {
        uiState = UIState.HIDING;
        btnAdventure?.SetEnable(false);
        DOTween.To(() => lobbyUIPanel.alpha, x => lobbyUIPanel.alpha = x, 0, UI_HIDE_TIME);
        yield return new WaitForSeconds(UI_HIDE_TIME);
        uiState = UIState.HIDE;

        while (this.gameObject.activeInHierarchy == true)
        {
            //UI 감춰진 도중 팝업이 출력되면 강제로 UI가 출력되도록 설정
            if (IsImmediatelyShowUI() == true)
            {
                btnAdventure?.SetEnable(true);
                ShowLobbyUI();
                hideUIRoutine = null;
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
        ShowLobbyUI();
        hideUIRoutine = null;
    }

    /// <summary>
    /// 강제로 UI를 출력해야 하는 상황인지 검사하기 위한 함수
    /// </summary>
    private bool IsImmediatelyShowUI()
    {
        //UI 감춰진 도중 팝업이 출력되면 강제로 UI 출력
        if (ManagerUI._instance._popupList.Count > 0) return true;

        //UI 가 선택된 상황에서 강제로 UI 출력
        if (UICamera.selectedObject != null && UICamera.selectedObject != ManagerUI._instance.gameObject) return true;

        //로비가 대기 상태가 아니라면 강제로 UI 출력
        if(ManagerLobby._instance._state != TypeLobbyState.Wait) return true;
        
        //튜토리얼이 출력되면 강제로 UI 출력
        if(ManagerTutorial._instance?._current != null) return true;
        
        return false;
    }

    public int GetActionCoin()
    {
        int value = 0;

        foreach (var item in dicActionCoin)
        {
            value += item.Value;
        }

        return value;
    }

    public void SetActionCoin(CurrencyType currencyType, int value)
    {
        if (dicActionCoin.ContainsKey(currencyType))
            dicActionCoin[currencyType] += value;
        else
            dicActionCoin.Add(currencyType, value);
    }
    
    public void InitActionCoin(CurrencyType currencyType)
    {
        if (dicActionCoin.ContainsKey(currencyType))
            dicActionCoin[currencyType] = 0;
    }

    public int GetActionJewel()
    {
        int value = 0;

        foreach (var item in dicActionJewel)
        {
            value += item.Value;
        }

        return value;
    }

    public void SetActionJewel(CurrencyType currencyType, int value)
    {   
        if (dicActionJewel.ContainsKey(currencyType))
            dicActionJewel[currencyType] += value;
        else
            dicActionJewel.Add(currencyType, value);
    }
    
    public void InitActionJewel(CurrencyType currencyType)
    {
        if (dicActionJewel.ContainsKey(currencyType))
            dicActionJewel[currencyType] = 0;
    }

    public static bool IsLobbyButtonActive
    {
        get
        {
            var currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == "Lobby")
            {
                if (ManagerLobby._instance != null && ManagerLobby._instance.IsLobbyComplete)
                {
                    //로비의 연출이 완전 끝나면 로비 버튼을 누를 수 있습니다.
                    return true;
                }
                else if (ManagerTutorial._instance != null && ManagerTutorial._instance._playing)
                {
                    //로비 연출이 끝나지 않았더라도 튜토리얼 도중이라면 로비 버튼을 누를 수 있습니다.
                    return true;
                }
                else if (UIPopupIntegratedEvent._instance != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //로비 씬이 아닌 경우 그냥 모두 허용 합니다.
                return true;
            }
        }
    }

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;
        _transform = transform;

        uiScreenSize = CalculateUISize(uiRoot.activeHeight * 0.5f);


        if (Application.loadedLevelName.Equals("Title"))
        {
            topUiPanel.gameObject.SetActive(false);
            backUiPanel.gameObject.SetActive(false);
        }

        {
            
         //   anchorTopLeft.transform.parent.transform.localPosition
        }
    }

	// Use this for initialization
	void Start ()
	{
        if ( Application.platform != RuntimePlatform.Android )
        {
            //// Debug.Log(" 아이폰X 대응 " + (float)Screen.height / (float)Screen.width);
            if ( ( float ) Screen.height / ( float ) Screen.width > 2f || ( float ) Screen.width / ( float ) Screen.height > 2f )
            {
                for ( int i = 0; i < iphoneXWidget.Count; i++ )
                {
                    iphoneXWidget[i].topAnchor.absolute = -180;
                    iphoneXWidget[i].UpdateAnchors();
                }

                //  Debug.Log(" 아이폰X 대응 " + iphoneXWidget.Count);
                for ( int i = 0; i < iphoneXWidgetY.Count; i++ )
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 60;
                    iphoneXWidgetY[i].UpdateAnchors();
                }
            }
        }

        if(LobbyMenuUtility.eventContentInfo == null)
            LobbyMenuUtility.MakeInitializedData();

        //Invalidate;
	    UpdateUI();

        //ActionInit
        InitActionCurrency();

        //ServerTime Active
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX || NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.STAGING)
        {
            UIItemServerTime itemRoot = NGUITools.AddChild(anchorTopCenter.transform, serverTime).GetComponent<UIItemServerTime>();
        }
    }

    void LateUpdate()
    {
        if ( reservedUpdateUI)
        {
            if( DoUpdateUI() )
                reservedUpdateUI = false;
        }
    }

    bool recursiveLock = false;
    bool reservedUpdateUI = false;
    public void UpdateUI(bool updateImmidiate = false)
    {
        if( updateImmidiate )
        {
            if (!recursiveLock && !reservedUpdateUI)
            {
                recursiveLock = true;
                if (!DoUpdateUI())
                    reservedUpdateUI = true;
                recursiveLock = false;
            }
            else
            {
                reservedUpdateUI = true;
            }
        }
        else
        {
            reservedUpdateUI = true;
        }
    }

    public void InitActionCurrency()
    {
        if(dicActionCoin.ContainsKey(CurrencyType.READY_ITEM))
            dicActionCoin[CurrencyType.READY_ITEM] = 0;
        if(dicActionJewel.ContainsKey(CurrencyType.READY_ITEM))
            dicActionJewel[CurrencyType.READY_ITEM] = 0;
    }
    
    private bool DoUpdateUI() //나중에 재화별로 빼기
    {
        if (Global._pendingReboot) {
            return false;
        }
        
       
        if (EditManager.instance != null) return false;
        //if (ManagerData._instance == null) return;

        if (tempClover != Global.clover)
        {
            tempClover = Global.clover;
            _labelClover.text = tempClover.ToString();
            _labelCloverS.text = _labelClover.text;
        }
        if (tempCherry != Global.coin)
        {
            tempCherry = Global.coin;
            _labelCherry.text = tempCherry.ToString();
        }
        if (tempJewer != Global.jewel)
        {
            tempJewer = Global.jewel;
            _labelJewer.text = tempJewer.ToString();
        }
        if (tempStar != Global.star)
        {
            tempStar = Global.star;
            _labelStar.text = tempStar.ToString();
        }
        if (tempWing != Global.wing)
        {
            tempWing = Global.wing;
            _labelWing.text = tempWing.ToString();
        }

        if (tempExp != Global.exp)
        {
            tempExp = Global.exp;
            _labelExp.text = tempExp.ToString();
        }
        
        int messeageCount = ServerRepos.MailCnt;
        LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.mail] = messeageCount;

        if (ServerRepos.LoginCdn != null)
        {
#if !UNITY_EDITOR
            if(ServerRepos.LoginCdn.EnableInvite == 0)
                SetAnchorRightUI();
#endif
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            if (myProfile.stage < ManagerData._instance.maxStageCount)
            {
                SetActiveStartButton();
            }
            else if (myProfile.stage >= ManagerData._instance.maxStageCount)
            {
                int step = 0; // 0이면 꽃, 1이면 파란꽃  2이면 전체 파란꽃
                int flowerCount = 0;
                step = 2;
                for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                {
                    if (ManagerData._instance._stageData[i]._flowerLevel < 3)
                    {
                        step = 0;
                        break;
                    }
                }
                if (step == 2)
                {
                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                    {
                        if (ManagerData._instance._stageData[i]._flowerLevel < 4)
                        {
                            step = 1;
                            break;
                        }
                    }
                }

                if (step < 2)
                {
                    SetActiveStartButton();

                    if (step == 0)
                        _rootStageFlower.spriteName = "icon_flower_stroke_yellow";
                    else if (step == 1)
                        _rootStageFlower.spriteName = "icon_blueflower_stroke_yellow";
                    _rootStageFlower.MakePixelPerfect();

                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                    {
                        if (step == 0)
                        {
                            if (ManagerData._instance._stageData[i]._flowerLevel >= 3)
                                flowerCount++;
                        }
                        else if (step == 1)
                        {
                            if (ManagerData._instance._stageData[i]._flowerLevel >= 4)
                                flowerCount++;
                        }
                    }

                    for (int i = 0; i < _labelStageFlower.Length; i++)
                        _labelStageFlower[i].text = flowerCount + "/" + ManagerData._instance.maxStageCount;
                }
                else
                {
                    int blossomOverrided = ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent()? 2 : 1;
                    SetActiveStartButton(blossomOverrided);
                }   
            }
            else
            {

            }

            this.RefreshLimitedMaterialRegenForecast();

            int playDecoFlag = 0;
            
            // 1, 5스테이지 클리어시 시간제 클로버 지원 이벤트가 우선순위가 더 높아서 코인x배 이벤트를 가리게 되어있음
            if ( ServerRepos.User.stage < 11 )
            {
                playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CloverSupply;
                if ( ServerRepos.User.stage == 1 )
                {
                    string s = Global._instance.GetString("nru_cl_1");
                    this._cloverSupplyLabel.SetText(s);
                    this._cloverSupplyTimeLabel.SetText($"30{Global._instance.GetString("time_3")}");
                }
                else
                {
                    string s = Global._instance.GetString("nru_cl_2");
                    var leftStage = (11 - ServerRepos.User.stage);
                    s = s.Replace("[n]", leftStage.ToString());
                    this._cloverSupplyTimeLabel.SetText($"60{Global._instance.GetString("time_3")}");

                    this._cloverSupplyLabel.SetText(s);
                }
            }

            if (Global.coinEvent > 0)
            {
                playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CoinEvent;                
                _coinEventLabel.text = "x" + ServerRepos.LoginCdn.CoinEventRatio;
            }
            _playButtonDeco.SetDecoState(playDecoFlag);
        }

        WakeupEventSetting();
        SaleInfoSetting();
        NewInfoSetting();
        CloverSendTimeEventSetting();
        UpdateMenuIcon();
        
        return true;
    }

    // default : 일반(일반 스타트 버튼, 수문장 크루그 스타트 버튼) / 1 : ComingSoon / 2 : Blossom
    private void SetActiveStartButton(int buttonIdx = 0)
    {
        _btnPlay.SetActive(false);
        _btnComingSoon.SetActive(false);
        _btnBlossom.SetActive(false);
        _btnSingleRoundEvent.SetActive(false);
        
        switch (buttonIdx)
        {
            case 1:
                _btnComingSoon.SetActive(true);
                break;
            case 2:
                _btnBlossom.SetActive(true);
                break;
            default:
                if (ManagerSingleRoundEvent.IsSingleRoundStage == false)
                {
                    _btnPlay.SetActive(true);
                    _btnSingleRoundEvent.SetActive(false);
                }
                else
                {
                    _btnPlay.SetActive(false);
                    _btnSingleRoundEvent.SetActive(true);
                    Reward reward = ManagerSingleRoundEvent.instance.GetReward();
                    if(reward != null)
                        _genericReward_SindleRountEvent.SetReward(reward);
                }
                break;
        }
    }

    public void SyncTopUIAssets()
    {
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.star = (int)GameData.Asset.Star;

        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        UpdateUI();
    }

    private bool bResetPokoYura = false;
    //랭킹 아래 포코유라 설정.
    /// <summary>
    /// 랭킹 아래 포코유라 설정. 랭킹 아이콘이 활성화 되어있을 경우에만 텍스쳐 갱신.
    /// </summary>
    /// <param name="isNewData">랭킹 아이콘 포코유라 데이터가 변경되었는지 여부</param>
    public void SettingRankingPokoYura(bool isNewData = true)
    {
        if (IsMenuOpened() == false)
        {
            bResetPokoYura = true;
            return;
        }

        if (isNewData)
        {
            bResetPokoYura = true;
        }

        if (bResetPokoYura == false)
            return;
        bResetPokoYura = false;
        
        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if (myProfile == null || myProfile.GetTridentProfile() == null)
            return;

        Profile_PION profileData = myProfile.GetPionProfile();
        if (profileData != null)
        {
            //대표 포코유라 이미지가 있다면 그 이미지 로드.
            if (profileData.profile.toy > 0)
            {
                _textureClass.LoadCDN(Global.gameImageDirectory, "Pokoyura/", "y_i_" + profileData.profile.toy);
                return;
            }
        }

        //대표 포코 유라가 없다면 랭킹 계산해서 로드.
        gameObject.AddressableAssetLoadClass<Texture>(ServerLogics.UserLevelWithFlower(), texture =>
        {
            APNGPlayer player = _textureClass.gameObject.GetComponent<APNGPlayer>();

            if (player == null)
            {
                _textureClass.mainTexture = texture;
            }
            else
            {
                FrameInfo[] frameInfo = new[] { new FrameInfo(){tex = texture, interval = 0}};
                player.Init(_textureClass, new APNGInfo(frameInfo));
            }
        });
    }
    
    public int GetPopupCount()
    {
        return _popupList.Count;
    }

    public void SettingDiaryButton()
    {
        diaryIcon.color = new Color(1f, 1f, 1f, 1f);
        diaryIcon.transform.localScale = Vector3.one;
    }

    void recvChargeClover(BaseResp code)
    {
        if (code.IsSuccess)
        {
            Global.clover = (int)GameData.Asset.AllClover;
            Global.wing = (int)GameData.Asset.AllWing;

            _labelCloverTime.text = Global.GetCloverTimeText(GameData.RemainChargeTime());
            _labelClover.text = ((int)GameData.Asset.AllClover).ToString();
            _labelCloverS.text = _labelClover.text;
            callChargeClover = false;

            _labelWing.text = Global.wing.ToString();

            //클로버추가
            //그로씨
            {
                var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AUTO_REGENERATION,
                    0,
                    1,
                    0, 
                    (int)(GameData.User.AllClover)
                    );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
            }
        }
    }

    // 메뉴와 하위 항목들의 LobbyMenuUtility의 데이터에 저장된 값에 따라 아이콘을 온 오프합니다.
    public void UpdateMenuIcon()
    {
        //메뉴 하단의 알림 UI를 설정하고 하나라도 0보다 크면 메뉴 UI의 알림을 켭니다.
        if (LobbyMenuUtility.newContentInfo != null)
        {
            bool bNewIcon = false;

            //심사환경이 아닐 경우에만 친구초대 카운트
            if (iosScreeningEnvironment == false)
            {
                bool bInvite = LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.friend] > 0;
                menuInviteIcon.SetActive(bInvite);
                if (bInvite)
                    bNewIcon = true;
            }
            
            int messeageCount = LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.mail];
            if (messeageCount > 0)
            {
                _messageNewIcon.SetActive(true);
                bNewIcon = true;
            
                if (messeageCount > 9)
                    _messageNewCount.text = "N";
                else
                    _messageNewCount.text = messeageCount.ToString();
            }
            else
                _messageNewIcon.SetActive(false);
        
            menuNewIcon.SetActive(bNewIcon);
        }

        //메뉴 상단의 이벤트 UI를 설정하고 하나라도 true인 경우 메뉴 UI의 eventIcon을 켭니다.
        if (LobbyMenuUtility.eventContentInfo != null)
        {
            cloverSendTimeEventIcon.SetActive(LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.ranking]);

            //심사 환경이 아닐 경우에만 친구초대 카운트
            if (iosScreeningEnvironment == false)
            {
                socialEventIcon.SetActive(LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.invite]);
                bool bWakeUp = LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.wakeup];
                menuWakeUpBubble.SetActive(bWakeUp);
            }

            bool bEventIcon = false;
            foreach (var info in LobbyMenuUtility.eventContentInfo)
            {
                //심사환경일 때 친구 초대 아이콘 관련 카운트는 무시
                if (iosScreeningEnvironment)
                {
                    if (info.Key == LobbyMenuUtility.MenuEventIconType.invite ||
                        info.Key == LobbyMenuUtility.MenuEventIconType.wakeup)
                        continue;
                }

                if (info.Value == true)
                    bEventIcon = true;
            }
            
            menuEventIcon.SetActive(bEventIcon);
        }
    }


    public class PackageShowData
    {
        public CdnShopPackage packageData;
        public long expireTs;
    }


    /// <summary>
    /// 패키지, 컴플리트 패키지 데이터 및 로비 패키치 배너 새로고침
    /// </summary>
    public void PackageUpdate()
    {
        SidebarLeft.DestroyAllBanner();
        EventPackagePhase();
        PackagePhase();
    }

    /// <summary>
    /// 이벤트 관련 상품 데이터 세팅
    /// </summary>
    public void EventPackagePhase()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        //PackagePhase()에서 WelcomeEvent의 배너는 생성하지 않아서 예외로 생성
        if (ManagerWelcomeEvent.CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.NRUPass, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerWelcomeEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.WelcomeMission));
            });
        }

        //PackagePhase()에서 WelcomeBackEvent의 배너는 생성하지 않아서 예외로 생성
        if (ManagerWelcomeBackEvent.IsActiveEvent())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.CBUPass_PremiumPass, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerWelcomeBackEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ManagerWelcomeBackEvent.GetUserWelcomeBackMission()));
            });
        }

        //PackagePhase()에서 CoinStashEvent 배너는 생성하지 않아서 예외로 생성
        if (ManagerCoinStashEvent.CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.none, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerCoinStashEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.CoinStashEvent));
            });
        }

        //PackagePhase()에서 DiaStashEvent 배너는 생성하지 않아서 예외로 생성
        if (ManagerDiaStash.CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.DiaStash, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerDiaStashEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.DiaStashEvent));
            });
        }
        
        //PackagePhase()에서 LuckyRoulette 배너는 생성하지 않아서 예외로 생성
        if (ManagerLuckyRoulette.CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.LuckyRoulette, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerLuckyRoulette>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.LuckyRoulette));
            });
        }
    }


    /// <summary>
    /// 패키지, 컴플리트 패키지 데이터 세팅
    /// </summary>
    public void PackagePhase()
    {
        GameCurrencyPackageCheck();
        PackageCheck();
        CompletePackageCheck();
        SelectPackageCheck();
        CBUPackageCheck();

        sidebarLeft.AddBannerToSidebar();
    }

    /// <summary>
    /// 인게임 재화 관련 상품 데이터 세팅
    /// </summary>
    public void GameCurrencyPackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        if (ServerContents.AdInfos.ContainsKey((int)AdManager.AdType.AD_13))
        {
            int index =-1;
            if (ServerRepos.UserAdInfos != null)
            {
                for (int i = 0; i < ServerRepos.UserAdInfos.Count; i++)
                {
                    if (ServerRepos.UserAdInfos[i].adType == (int)AdManager.AdType.AD_13)
                        index = i;
                }
            }
            BannerTypeDatas.Package_RandomBox_ShowAD _package = new BannerTypeDatas.Package_RandomBox_ShowAD();
            _package.adType = AdManager.AdType.AD_13;
            _package.resourceIdx = ServerRepos.LoginCdn.adPackageResourceVer;
            _package.rewards = ServerContents.AdInfos[(int)AdManager.AdType.AD_13].rewards;
            if (index >= 0)
                _package.expiredTime = ServerRepos.UserAdInfos[index].lastUsedTime + ServerContents.AdInfos[(int)_package.adType].useInterval;
            else    // 타입 13 광고 열람 데이터 없으면 (첫 열람이면)
                _package.expiredTime = Global.GetTime();

            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.AD, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerRandomBoxADPackage>(
                    sidebar: SidebarLeft,
                    init: (banner) => banner.Init(_package));
            });

            if (Global.LeftTime(_package.expiredTime) > 0 && ServerRepos.UserAdInfos[index].usedCount < ServerContents.AdInfos[(int)_package.adType].dailyLimit)
            {
                if (SidebarLeft.countCoroutine == null)
                    SidebarLeft.countCoroutine = StartCoroutine(SidebarLeft.CheckBannerTimer(_package.expiredTime, true));
            }
        }
    }

    void PackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        List<PackageShowData> packageShowCandidates = new List<PackageShowData>();

        #region packageShowCandidates 새로운 패키지 데이터값 세팅
        //전체 패키지 수 만큼 검사.
        var enumerator = ServerContents.Packages.GetEnumerator();
        while (enumerator.MoveNext())
        {
            bool checkPassed = true;
            bool buyBefore = false;
            var packageData = enumerator.Current.Value;
            long expireTs = packageData.expireTs;
            switch (packageData.type)
            {
                case 0: //0 : (Speed Clear) 모든사용자에게 expire_ts에 설정된 기간까지 판매되는 상품 (스피드 클리어)
                    break;
                case 1: //1 : (New User) User_infos의 created_at +display_day까지만 판매되는 패키지 / expire_ts 값 무시
                    {
                        System.DateTime date = ServerRepos.User.createdAt;
                        date = date.AddDays(packageData.display_day);
                        expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;
                    }
                    break;
                case 2: //2 : (Stage Fail) 누적 스테이지 실패가 5회 이상인 사용자에게 1회만 노출되는 패키지 / display_day, expire_ts 값 무시
                    // 기본 UI상에는 표시될 일이 없음
                    {
                        checkPassed = false;
                    }
                    break;
                case 3: //3 : (Deluxe) 초심자 패키지를 구입한 사용자에게만 패키지 적용일 + display_day 기간까지 판매되는 패키지 / expire_ts 값 무시
                    {
                        var buyRecord = ServerRepos.UserShopPackages.Find(x => 
                        {
                            CdnShopPackage packageInfo = null;
                            if( ServerContents.Packages.TryGetValue( x.idx, out packageInfo) )
                            {
                                if( packageInfo.type == 1)
                                {
                                    System.DateTime date = x.createdAt;
                                    date = date.AddDays(packageData.display_day);
                                    expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;

                                    if (Global._instance != null && Global.LeftTime(expireTs) > 0)
                                        return true;
                                }
                            }
                            return false;
                        });

                        if (buyRecord == null)
                        {
                            checkPassed = false;
                        }
                    }
                    break;
                case 4: //4 : (Non Pay User) 결제 이력이 없거나 결제일로부터 180일 경과 유저로 서버에서 예외처리 됨
                    break;
                case 5: //5 : 묶음패키지용. 이것도 기본적으로 표시안됨 (묶음패키지 아이콘을 눌러서 들어갔을 때만 나옴)
                    checkPassed = false;
                    break;
                case 6: // 6: welcome pass
                    {
                        if( ServerRepos.UserWelcomeMission != null)
                        {
                            expireTs = ServerRepos.UserWelcomeMission.endTs;
                        }
                        else
                        {
                            System.DateTime date = ServerRepos.User.createdAt;
                            date = date.AddDays(packageData.display_day);
                            expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;
                        }

                        checkPassed = false;
                    }
                    break;

                case 7: // 7: welcome back pass
                    {
                        if (ManagerWelcomeBackEvent.GetUserWelcomeBackMission() != null)
                        {
                            expireTs = ManagerWelcomeBackEvent.GetUserWelcomeBackMission().endTs;
                        }
                        else
                        {
                            System.DateTime date = ServerRepos.User.createdAt;
                            date = date.AddDays(packageData.display_day);
                            expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;
                        }

                        checkPassed = false;
                    }
                    break;
                case 8: // 8: 선택형 패키지.
                    {
                        checkPassed = false;
                    }
                    break;
                case 9: // 9: CBU 패키지.
                {
                    checkPassed = false;
                }
                    break;
                case 10: // 10: NPU 스팟성 패키지.
                {
                    checkPassed = false;
                }
                    break;
                case 11: // 11: 마유지의 특별한 제안 (다이아 주머니)
                {
                    checkPassed = false;
                }
                    break;
                case 12: // 12: 탐험 패스
                {
                    checkPassed = false;
                }
                    break;
                
                case (int)PackageType.SpotCloverPack:
                {
                    checkPassed = false;
                }
                    break;
            }

            if (!checkPassed)
                continue;

            //시간이 지났다면 버튼 생성 안함.
            if (Global._instance != null && Global.LeftTime(expireTs) <= 0)
                continue;

            //현재 유저가 구매한 패키지 수 만큼 검사.
            for (int i = 0; i < ServerRepos.UserShopPackages.Count; i++)
            {
                
                if (ServerRepos.UserShopPackages[i].idx == enumerator.Current.Value.idx)
                {
                    CdnShopPackage packageInfo = null;
                    if (ServerContents.Packages.TryGetValue(ServerRepos.UserShopPackages[i].idx, out packageInfo))
                    {
                        if( ServerRepos.UserShopPackages[i].buyCount >= packageInfo.buyLimit)
                        {
                            buyBefore = true;
                        }
                    }
                }
            }
            //현재 패키지를 유저가 이미 구매한 상태라면 버튼 생성 안함.
            if (buyBefore == true)
                continue;

            packageShowCandidates.Add( new PackageShowData() { packageData = enumerator.Current.Value, expireTs = expireTs } );
        }
        #endregion

        if (PlayerPrefs.HasKey("SpotDia_Btn"))
        {
            bool btnExpired = true;
            string spotDiaBtnInfo = PlayerPrefs.GetString("SpotDia_Btn");
            string[] spotDiaBtnInfos = spotDiaBtnInfo.Split('_');
            if (spotDiaBtnInfos.Length == 2)
            {
                int packageIdx;
                long showPeriod;
                if (Int32.TryParse(spotDiaBtnInfos[0], out packageIdx) && long.TryParse(spotDiaBtnInfos[1], out showPeriod))
                {
                    bool alreadyBuy = false;
                    for (int i = 0; i < ServerRepos.UserShopPackages.Count; i++)
                    {
                        if (ServerRepos.UserShopPackages[i].idx == packageIdx)
                        {
                            alreadyBuy = true;
                        }
                    }

                    if (alreadyBuy == false && Global.LeftTime(showPeriod) > 0 && ServerContents.Packages.ContainsKey(packageIdx) )
                    {
                        var package = ServerContents.Packages[packageIdx];
                        packageShowCandidates.Add(new PackageShowData() { packageData = package, expireTs = showPeriod });
                        btnExpired = false;
                    }
                }
            }
            if(btnExpired)
                PlayerPrefs.DeleteKey("SpotDia_Btn");
        }

        PackageFilterUtility.PackageFiltering(packageShowCandidates);

        for(int i = 0; i < packageShowCandidates.Count; ++i)
        {
            int idx = i;
            ManagerUI._instance.SidebarLeft.AddBannerMaker(packageShowCandidates[idx].packageData.type, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerNormalPackage>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(packageShowCandidates[idx]));
            });
        }
    }

    void CompletePackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        List<CdnCompletePackage> newCdnCompletePackages = new List<CdnCompletePackage>();

        if (ServerContents.CompletePackages.idx == ServerRepos.LoginCdn.completePackageIdx && ServerContents.CompletePackages.idx != 0)
        {
            var compPkg = ServerContents.CompletePackages;

            if(Global.LeftTime(compPkg.expired_at) > 0)
            {
                newCdnCompletePackages.Add(compPkg);
            }
        }        

        foreach (var package in newCdnCompletePackages)
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.CompletePack, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerCompletePackage>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(package));
            });
        }
    }

    private void SelectPackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        if (ServerContents.SelectPackage == null || ServerRepos.UserSelectPackage == null) return;
        
        if (ServerContents.SelectPackage.idx != 0)
        {
            var selectPackage = ServerContents.SelectPackage;

            if (ServerRepos.UserSelectPackage.isExpired || Global.LeftTime(ServerRepos.UserSelectPackage.expiredAt) <= 0 ||
                selectPackage.options == null || selectPackage.options.Count <= 0)
            {
                return;
            }

            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.SelectPack, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerSelectPackage>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(selectPackage));
            });
        }
    }
    
    private void CBUPackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        CdnShopPackage cbuPackage = ServerContents.Packages.Values.ToList().Find(x => x.type == 9);
        
        if (cbuPackage != null && ServerRepos.UserCbuPackage != null)
        {
            cbuPackage.expireTs = ServerRepos.UserCbuPackage.expiredAt;
            
            // 시간 체크
            if (Global._instance != null && Global.LeftTime(cbuPackage.expireTs) <= 0)
                return;

            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.CBUPack, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerCBUPackage>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(new PackageShowData() { packageData = cbuPackage, expireTs = cbuPackage.expireTs }));
            });
        }
    }

    public CdnShopPackage SpotCloverPackageCheck()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver) 
            return null;

        if (ServerContents.SpotCloverPack == null || ServerContents.SpotCloverPack.vsn == 0)
            return null;

        CdnShopPackage package = ServerContents.Packages.Values.ToList().Find(x => x.type == (int)PackageType.SpotCloverPack && x.idx == ServerContents.SpotCloverPack.idx);
      
        if (package == null || Global.LeftTime(package.expireTs) <= 0)
            return null;

        if (ServerRepos.UserShopPackages != null)
        {
            var userPackage = ServerRepos.UserShopPackages.Find(x=>x.vsn == ServerContents.SpotCloverPack.vsn && x.idx == package.idx);
            if(userPackage != null && userPackage.buyCount >= package.buyLimit)
                return null;
        }

        string spotCloverKey = "spot_clover_vsn";
        
        if (PlayerPrefs.HasKey(spotCloverKey) && PlayerPrefs.GetInt(spotCloverKey) == ServerContents.SpotCloverPack.vsn)
            return null;

        PlayerPrefs.SetInt(spotCloverKey, ServerContents.SpotCloverPack.vsn);

        return package;
    }

    private void WakeupEventSetting()
    {
        if (ServerContents.WakeupEvent == null || ServerContents.WakeupEvent.event_index == 0)
        {
            LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.wakeup] = false;
            return;
        }
        LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.wakeup] = true;
    }

    long wingRefreshCheckedTime = 0;
    bool callChargeClover = false;
	void Update () {

        // 클로버 타이머
        if (ManagerData._instance != null)
        {
            // UI가 활성화 되어있을때,, 클로버가 5개 이하일때
            if (ManagerData._instance._state == DataLoadState.eComplete && _labelCloverTime.gameObject.active)
            {
                if (GameData.RemainFreePlayTime() > 0)
                {
                    _labelCloverTime.text = Global.GetCloverFreeTimeText(GameData.RemainFreePlayTime());

                    if (!_CloverInfinitySprite.gameObject.active)
                    {
                        _CloverInfinitySprite.gameObject.SetActive(true);
                        _labelClover.gameObject.SetActive(false);
                        _labelCloverS.gameObject.SetActive(false);
                        _labelCloverTime.color = new Color(156f / 255f, 230f / 255f, 1f);
                    }

                    _CloverInfinitySprite.cachedTransform.localPosition = Vector3.up * Mathf.Cos(Time.time * 15f) * 1.5f;
                }
                else if (GameData.Asset.AllClover < GameData.Asset.MaxClover)
                {
                    _labelCloverTime.text = Global.GetCloverTimeText(GameData.RemainChargeTime());
                    _labelClover.text = ((int)GameData.Asset.AllClover).ToString();
                    _labelCloverS.text = _labelClover.text;
                    //Debug.Log("GameData.RemainChargeTime " + GameData.RemainChargeTime() + "    ");


                    if (GameData.RemainChargeTime() <= 0 && !callChargeClover)
                    {
                        callChargeClover = true;
                        ServerAPI.ChargeAll(recvChargeClover);
                    }
                }
                else
                {
                    _labelCloverTime.text = "Full";
                }

                if (GameData.RemainFreePlayTime() > 0)
                {

                }
                else
                {
                    if (_CloverInfinitySprite.gameObject.active)
                    {
                        _CloverInfinitySprite.gameObject.SetActive(false);
                        _labelClover.gameObject.SetActive(true);
                        _labelCloverS.gameObject.SetActive(true);

                        _labelCloverTime.color = Color.white;
                    }
                }

                if (ManagerAdventure.CheckStartable())
                {
                    if (GameData.RemainFreeWingPlayTime() > 0)
                    {
                        _labelWing.text = Global.GetCloverFreeTimeText(GameData.RemainFreeWingPlayTime());

                        if (!_WingInfinitySprite.gameObject.activeInHierarchy)
                        {
                            _WingInfinitySprite.gameObject.SetActive(true);
                            _labelWing.color = new Color(156f / 255f, 230f / 255f, 1f);
                        }

                        _WingInfinitySprite.cachedTransform.localPosition = Vector3.up * Mathf.Cos(Time.time * 15f) * 1.5f;
                    }
                    else
                    {
                        _labelWing.text = ((int)GameData.Asset.AllWing).ToString();
                        _WingInfinitySprite.gameObject.SetActive(false);
                        _labelWing.color = Color.white;
                    }

                    if (wingRefreshCheckedTime == 0 && GameData.User.loginTs != 0)
                    {
                        wingRefreshCheckedTime = GameData.User.loginTs;
                        Debug.Log("wingRefreshCheckedTime Init " + wingRefreshCheckedTime.ToString());

                        System.DateTime logOrigin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                        System.DateTime logCheckTime = logOrigin;
                        logCheckTime = logCheckTime.AddSeconds(wingRefreshCheckedTime);
                        logCheckTime = logCheckTime.Date;

                        Debug.Log("logCheckTime Init " + logCheckTime.ToString());

                        System.DateTime logRebootTime = logCheckTime.AddHours(ServerRepos.LoginCdn.LoginOffset);
                        logRebootTime = logRebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);
                        logRebootTime = logRebootTime.AddSeconds(10);
                        Debug.Log("logRebootTime Init " + logRebootTime.ToString());
                        Debug.Log("wingRefreshCheckedTime Init " + wingRefreshCheckedTime.ToString());

                    }
                        

                    System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                    System.DateTime checkTime = origin;
                    checkTime = checkTime.AddSeconds(wingRefreshCheckedTime);
                    checkTime = checkTime.Date;

                    System.DateTime rebootTime = checkTime.AddHours(ServerRepos.LoginCdn.LoginOffset);
                    rebootTime = rebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);
                    rebootTime = rebootTime.AddSeconds(10);

                    System.TimeSpan diff = rebootTime - origin;
                    double time = System.Math.Floor(diff.TotalSeconds);
                    
                    if (Global.GetTime() >= time && (wingRefreshCheckedTime < time && wingRefreshCheckedTime != 0))
                    {
                        if( callChargeClover == false)
                        {
                            callChargeClover = true;
                            wingRefreshCheckedTime = Global.GetTime();

                            ServerAPI.ChargeAll(recvChargeClover);
                        }
                    }
                }

            }
        }


        // 터치 이펙트,실지 동작과 별도로 터치 할때마다 생성
        if (Global._touchBegin)
        {
            //인게임이고 팝업이 없을때 안나옴
            if (SceneManager.GetActiveScene().name == "InGame" && _popupList.Count == 0)
            {

            }
            else if (SceneManager.GetActiveScene().name == "InGameTool" && _popupList.Count == 0)
            {

            }
            else
            {
                GameObject obj = ManagerObjectPool.Spawn(_objTouchFlower, _transform);
                obj.transform.position = _camera.ScreenToWorldPoint(Global._touchPos);
                obj.transform.localScale = Vector3.one;
            }
        }
/*
        if (Input.GetKeyDown(KeyCode.M))
        {
            OpenPopupDiaryMission();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ClosePopUpUI();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            UILobbyChat lobbyChat = NGUITools.AddChild(gameObject, _objLobbyChat).GetComponent<UILobbyChat>();
            StartCoroutine(lobbyChat.ShowLobbyChat(Character._boni._transform, "おはよう!!", true));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UIChat uiChat = NGUITools.AddChild(anchorCenter, _objChat).GetComponent<UIChat>();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            OpenPopupStageTarget();
        }*/
      
        //if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ManagerTutorial._instance != null || UIPopupStageTarget.instance != null)
                    return;
                if (activeClickBlocker)
                    return;

                int nCount = ManagerUI._instance._popupList.Count;
                if (nCount > 0)
                {
                    if (Global._instance == null) return;
                    if (UIButtonCanClickCheck() == false) return;
                    ManagerUI._instance._popupList[nCount-1].OnClickBtnBack();
                }
                else
                {
#if UNITY_IOS
                return;
#endif
                    if (Application.platform == RuntimePlatform.IPhonePlayer) return;
                    if (SceneLoading.IsSceneLoading) return;

                    if (SceneManager.GetActiveScene().name == "InGame")
                    {
                        if (SceneManager.GetActiveScene().name == "InGameTool") return;
                        if (GameItemManager.instance != null)
                        {
                            if (GameItemManager.instance.used == false)
                            {
                                GameItemManager.instance.BtnCancel();
                            }
                            return;
                        }
                        if (GameManager.instance.state == GameState.GAMECLEAR || GameManager.instance.state == GameState.GAMEOVER)
                        {
                            if (GameUIManager.instance != null)
                                GameUIManager.instance.SkipStageClear();
                            return;
                        }

                        if (Input.touchCount > 0) return;

                        if (GameManager.instance != null && GameManager.instance.LoadScene) return;
                        
                        if (Global._instance == null) return;
                        if (UIButtonCanClickCheck() == false) return;
                        if (GameUIManager.instance != null)
                            GameUIManager.instance.OnClickBtnPause();
                    }
                    else
                    {
                        if (Global._instance == null) return;
                        if (UIButtonCanClickCheck() == false) return;
                        Global.InitClickTime();

                        // 오프닝 영상 진행 중 백버튼 입력 시 오프닝 영상 스킵
                        if (SceneManager.GetActiveScene().name == "Intro")
                        {
                            if (SceneIntro._instance != null && SceneIntro._instance._skipButton.activeSelf)
                                SceneIntro._instance.OnSkip();
                            return;
                        }
                        // 미션 진행 중 백버튼 입력 시 미션 스킵
                        if (ManagerLobby._instance != null && ManagerLobby._instance._state == TypeLobbyState.TriggerEvent)
                        {
                            if (ManagerCinemaBox._instance != null && ManagerCinemaBox._instance._skipButton.activeSelf)
                                ManagerCinemaBox._instance.OnClickSkip();
                            return;
                        }
                        // 캡슐토이 가챠 중 백버튼 입력 시 연출 스킵
                        
                        // 사이드 바 켜져있을 때 백버튼 입력 시 사이드 바 비활성화 (4.11.0 빌드 기준 좌측 사이드 바만 동작)
                        if (SidebarLeft != null && SidebarLeft.isActiveAndEnabled && SidebarLeft.root.gameObject.activeSelf)
                        {
                            SidebarLeft.Close();
                            return;
                        }
                        else
                        {
                            UIPopupSystem popupSystem = OpenPopupSystem().GetComponent<UIPopupSystem>();
                            popupSystem.FunctionSetting(1, "OnClickExit", gameObject);
                            popupSystem.SetButtonText(1, Global._instance.GetString("btn_18"));
                            popupSystem.SetButtonText(2, Global._instance.GetString("btn_2"));
                            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_7"), Global._instance.GetString("n_s_5"), true);
                            popupSystem.SetResourceImage("Message/boniExit");
                            popupSystem.TypeSetting(PopupType.exit);
                            return;
                        }
                    }
                }
            }
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {   
            if (Input.GetKeyDown(KeyCode.F1))
            {
                //Notice data = new Notice();
                //data.noticeIndex = 30;
                //data.video = new List<NoticeVideo>() { new NoticeVideo() { position = new List<float>() { 0f, 0f }, size = new List<int>() { 500, 600 } } };
                //ManagerUI._instance.OpenPopupGenericPage("HelpPage/BattleEvent");
                //ManagerUI._instance.MakeGenericPageIcon(1, "HelpPage/BattleEvent");
                // Global.ReBoot();
                //OpenPopupRequestReview();

                //OpenPopupBlossomEvent(1, false);
                //SwitchTopUIAdventureMode(tempAdvMode);
                //tempAdvMode = tempAdvMode == false;
                //    ManagerAdventure.UserDataAnimal newAnimal = new ManagerAdventure.UserDataAnimal { animalIdx = 1001, overlap = 1, grade = 3, level = 1 };

                //OpenPopupStageAdventureSummonAction(null, ManagerAdventure.User.GetAnimalInstance(newAnimal), UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>());

                //ManagerLobby._instance.ForceOpenNotices();
                //Global.SetGameType_WorldRanking(ServerContents.WorldRank.eventIndex, ServerRepos.UserWorldRank?.stage ?? 1);
                //ManagerUI._instance.OpenPopupGenericPage("Notice/Test");
                //ServerAPI.GetQuestState(List<int>{ 1,2,3}, (AdminStageMissionFixResp)
                //ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyItem);
                {
                    var sortedAll = Resources.FindObjectsOfTypeAll(typeof(Texture2D)).OrderBy(go =>
                        UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(go)).ToList();

                    StringBuilder sb = new StringBuilder("");
                    long memTexture = 0;
                    for (int i = sortedAll.Count - 1; i >= 0; i--)
                    {
                        if (!sortedAll[i].name.StartsWith("d_"))
                        {
                            memTexture += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(sortedAll[i]);
                            float inKB = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(sortedAll[i]) / 1024f;                            
                            sb.Append("#");
                            sb.Append($"{(sortedAll.Count - i):D4}");
                            sb.Append(":");                            
                            sb.Append("\t/Mem:\t");
                            sb.Append($"{inKB:F2}\t");
                            sb.Append(" KB/\tTotal:\t");
                            sb.Append($"{memTexture/1024:F2}");
                            sb.Append(" KB / ");
                            sb.Append($" Name: {sortedAll[i].name} /");
                            sb.Append($" IID: {sortedAll[i].GetInstanceID()}");

                            sb.Append("\n");
                        }
                    }
                    Debug.Log($"Texture2DInspect:{sortedAll.Count} \n" + sb.ToString());
                }

            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                //ManagerUI._instance.OpenPopupRankUp(2,1);
                //ManagerUI._instance.OpenPopupShare();
                //ManagerUI._instance.OpenPopupRequestSmall();
                //ManagerUI._instance.OpenPopupNoticeHelp(ServerRepos.Notices[0]);
                //ManagerUI._instance.OpenPopupMissionStampEvent();
                //ManagerUI._instance.OpenPopupBoxShop();
                //StartCoroutine(ManagerUI._instance.OpenPopupLoginBonus(1));
                //ManagerUI._instance.OpenPopupAgreementProfileImage();
                //ManagerLobby._instance.SetCostume(2);

                //ManagerUI._instance.OpenPopup<UIPopUpWorldRank>( (popup) => popup.InitPopup() );
                //ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyItem_Ingame);

                //ManagerUI._instance.OpenPopup<UIPopUpLandSelect>();
                //ManagerUI._instance.OpenPopupAlphabetEvent();
                //ManagerTurnRelay.OpenTurnRelay();

                ManagerUI._instance.OpenPopup<UIPopupDecoInformation>();

            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                //ManagerUI._instance.OpenPopupComingSoon();
                //ManagerUI._instance.OpenPopupEventRanking();
                //ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyScoreUp);
                ManagerUI._instance.OpenPopup<UIPopUpLandSelect>();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                //ManagerUI._instance.OpenPopupPackage();
                //    ManagerUI._instance.OpenPopupReadyEventStage();
                /*
                UIPopupSystem popupSystem = OpenPopupSystem().GetComponent<UIPopupSystem>();
                popupSystem.SetButtonText(1, Global._instance.GetString("btn_18"));
                popupSystem.HideCloseButton();
                Texture2D texture = Resources.Load("Message/boniExit") as Texture2D;
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_7"), Global._instance.GetString("n_s_5"), true, texture);*/
                ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Global.ReBoot();
                //ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyMission);
                //ManagerTutorial.PlayTutorial(TutorialType.TutorialInGameItem);
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialCrossHammer);
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialColorBrush);
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGetPlusHousing);
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGetMaterial);
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialCountCrack);
            }
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialPea);
            }
            else if (Input.GetKeyDown(KeyCode.F12))
            {
                Global.SetGameType_CoinBonusStage(1, 1);
                StartCoroutine(CoCheckStageDataBeforeOpenPopUpCoinStageReady());
                //OpenPopupReadyCoinStage();
            }
        }
    }

    public void InitTopUIDepth()
    {
        topUiPanel.depth = 0;
        topUiPanel.useSortingOrder = false;
    }

    public void CoShowUI(float time, bool bShow, TypeShowUI uiType)
    {
        if (uiType == TypeShowUI.eTopUI)
        {
            if (bShow == true)
            {
                topUiPanel.gameObject.SetActive(true);
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 1, time);
            }
            else
            {
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 0, time)
                    .OnComplete(()=>topUiPanel.gameObject.SetActive(false));
            }
        }

        else if (uiType == TypeShowUI.eBackUI)
        {
            if (bShow == true)
            {
                backUiPanel.gameObject.SetActive(true);

                int playDecoFlag = 0;
                if (ServerRepos.User.stage < 11)
                {
                    playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CloverSupply;
                }

                if (Global.coinEvent > 0)
                {
                    playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CoinEvent;
                    _coinEventLabel.text = "x" + ServerRepos.LoginCdn.CoinEventRatio;
                }
                _playButtonDeco.SetDecoState(playDecoFlag);

                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 1, time);
            }
            else
            {
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 0, time)
                    .OnComplete(() => backUiPanel.gameObject.SetActive(false));
            }
        }

        else
        {
            if (bShow == true)
            {
                topUiPanel.gameObject.SetActive(true);
                backUiPanel.gameObject.SetActive(true);
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 1, time);
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 1, time);

                int playDecoFlag = 0;
                if (ServerRepos.User.stage < 11)
                {
                    playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CloverSupply;
                }

                if (Global.coinEvent > 0)
                {
                    playDecoFlag |= (int)PlayButtonDecos.DecoFlag.CoinEvent;
                    _coinEventLabel.text = "x" + ServerRepos.LoginCdn.CoinEventRatio;
                }
                _playButtonDeco.SetDecoState(playDecoFlag);
            }
            else
            {
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 0, time)
                   .OnComplete(() => topUiPanel.gameObject.SetActive(false));
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 0, time)
                    .OnComplete(() => backUiPanel.gameObject.SetActive(false));
            }
        }
    }

    public void AnchorTopDestroy()
    {
        anchorTopRight.DestroyAllButtons();
        anchorTopLeft.DestroyAllButtons();
        ScrollbarRight.DestroyAllIcon();
        SidebarLeft.DestroyAllBanner();
        SidebarLeft.ResetClose();
    }

    public void PanelAlphaTween(UIPanel panel, float alpha, float time, TweenCallback action)
    {
        DOTween.To(() => panel.alpha, x => panel.alpha = x, alpha, time).OnComplete(action);
    }

    public void OpenPopupChat(ChatGameData in_data, Method.FunctionVoid in_callback)
    {
        UIChat uiChat = InstantiateUIObject("UIPrefab/UIChat", anchorCenter).GetComponent<UIChat>();
        uiChat._chatGameData = in_data;
        uiChat._callbackEnd = in_callback;
    }

    public void OpenPopupTimeMission(MissionData mData)
    {
        UIPopupTimeMission popupTimeMission = NGUITools.AddChild(anchorCenter, _objPopupTimeMission).GetComponent<UIPopupTimeMission>();
        popupTimeMission.OpenPopUp(GetPanelDepth(popupTimeMission.bShowTopUI, popupTimeMission.panelCount));
        popupTimeMission.SettingSortOrder(GetSortLayer(popupTimeMission.sortOrderCount));
        popupTimeMission.InitPopUp(mData);
        _popupList.Add(popupTimeMission);
    }

    public void OpenPopupTimeGiftBox(ServerUserGiftBox qData)
    {
        if (UIButtonCanClickCheck() == false)
            return;
        UIPopupTimeGiftBox popupTimeGiftBox = InstantiateUIObject("UIPrefab/UIPopupTimeGiftBox", anchorCenter).GetComponent<UIPopupTimeGiftBox>();
        popupTimeGiftBox.OpenPopUp(GetPanelDepth(popupTimeGiftBox.bShowTopUI, popupTimeGiftBox.panelCount));
        popupTimeGiftBox.SettingSortOrder(GetSortLayer(popupTimeGiftBox.sortOrderCount));
        popupTimeGiftBox.InitPopUp(qData);
        _popupList.Add(popupTimeGiftBox);
    }

    public void OpenPopupUserNameBox(bool bChat)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupUserNameBox popupUserNameBox = InstantiateUIObject("UIPrefab/UIPopupUserNameBox", anchorCenter).GetComponent<UIPopupUserNameBox>();
        popupUserNameBox.OpenPopUp(GetPanelDepth(popupUserNameBox.bShowTopUI, popupUserNameBox.panelCount));
        popupUserNameBox.SettingSortOrder(GetSortLayer(popupUserNameBox.sortOrderCount));
        popupUserNameBox.InitPopUp(bChat);
        _popupList.Add(popupUserNameBox);
    }

    public void OpenPopupInvite()
    {
        if (IsLobbyButtonActive)
        {
            if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
            {
                ManagerUI._instance.GuestLoginSignInCheck();
            }
            else
            {
                //ServerAPI.InvitedFriends(recvInvitedFriends);
                StartCoroutine(CoInitInvitePopup());
            }
        }
    }

    IEnumerator CoInitInvitePopup()
    {
        // 임시로 만들어놓은 친구 목록 갱신 코드 호출용 코루틴
        // 이게 늘 호출되어도 될지, serverapi 호출과 순서문제는 괜찮은지 등등의 문제가 있으니 확인하고 수정필요

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
        }
        else
        {
            yield return UIPopupInvite.CoInitFriendProfile();
        }

        ServerAPI.InvitedFriends(recvInvitedFriends);
    }

    void recvInvitedFriends(InvitedFriendsResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Invited Friends Count : " + resp.invitedFriends.Count);

            //ResetInviteFriendCanInviteFlag()
            foreach (var i in SDKGameProfileManager._instance.GetNonPlayLineFriendList())
            {
                i.canInvite = true;
            }

            for (int i = 0; i < resp.invitedFriends.Count; i++)
            {
                if(SDKGameProfileManager._instance.TryGetNonPlayLineFriend(resp.invitedFriends[i].fUserKey, out UserFriend data))
                {
                    data.canInvite = false;
                }
            }

            UIPopupInvite popupInvite = InstantiateUIObject("UIPrefab/UIPopupInvite", anchorCenter).GetComponent<UIPopupInvite>();
            popupInvite.OpenPopUp(GetPanelDepth(popupInvite.bShowTopUI, popupInvite.panelCount));
            popupInvite.SettingSortOrder(GetSortLayer(popupInvite.sortOrderCount));
            popupInvite.InitPopUpInvite();
            _popupList.Add(popupInvite);
        }
        else
        {
            ManagerUI._instance.bTouchTopUI = true;
        }
    }

    public void OpenPopupInviteSmall()
    {
#if UNITY_IOS
        // ios 에서는 무조건 작은초대창을 띄우지 않게 수정되었음
        return;
#endif
        int friendCount = SDKGameProfileManager._instance.GetNonPlayLineFriendsCount();
        if (friendCount < 6)
            return;
        ServerAPI.InvitedFriends(recvInvitedFriendsSmall);
    }

    void recvInvitedFriendsSmall(InvitedFriendsResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Invited Friends Count : " + resp.invitedFriends.Count);

            //최대 초대 수나 하루 최대 초대 수를 넘었으면 return.
            if (resp.invitedFriends.Count > 994 || ServerRepos.InviteDayCnt > 44)
            {
                return;
            }

            for (int i = 0; i < resp.invitedFriends.Count; i++)
            {
                if(SDKGameProfileManager._instance.TryGetNonPlayLineFriend(resp.invitedFriends[i].fUserKey, out UserFriend data))
                {
                    data.canInvite = false;
                }    
            }

            //친구 리스트 검사.
            List<UserFriend> invitePossible = new List<UserFriend>();

            foreach(var user in SDKGameProfileManager._instance.GetNonPlayLineFriendList())
            {
                //초대 가능한 친구들만 넣음.
                if (user.canInvite == false)
                    continue;
                invitePossible.Add(user);
            }

            //초대가능한 친구 수가 안되면 반환.
            int inviteCount = invitePossible.Count;
            if (inviteCount < 6)
                return;
            
            UIPopupInviteSmall popupInviteSmall = NGUITools.AddChild(anchorCenter, _objPopupInviteSmall).GetComponent<UIPopupInviteSmall>();
            popupInviteSmall.OpenPopUp(GetPanelDepth(popupInviteSmall.bShowTopUI, popupInviteSmall.panelCount));
            popupInviteSmall.SettingSortOrder(GetSortLayer(popupInviteSmall.sortOrderCount));
            popupInviteSmall.InitPopUp(invitePossible);
            _popupList.Add(popupInviteSmall);
        }
    }

    public void OpenPopupRequestClover()
    {
        ServerAPI.RequestedCloverList(recvRequestedCloverList);
    }

    void recvRequestedCloverList(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Requested Clover List Count :" + resp.cloverCoolTime.Count);

            foreach (var item in resp.cloverCoolTime)
            {
                if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.fUserKey, out UserFriend data))
                    data.cloverRequestCoolTime = item.reqCoolTime;
            }
            UIPopupRequestClover popupRequestClover = InstantiateUIObject("UIPrefab/UIPopupRequestClover", anchorCenter).GetComponent<UIPopupRequestClover>();
            popupRequestClover.OpenPopUp(GetPanelDepth(popupRequestClover.bShowTopUI, popupRequestClover.panelCount));
            popupRequestClover.SettingSortOrder(GetSortLayer(popupRequestClover.sortOrderCount));
            popupRequestClover.InitPopUpRequestClover(resp.cloverCoolTime.Count);
            _popupList.Add(popupRequestClover);
        }
        else
        {
            ManagerUI._instance.bTouchTopUI = true;
        }
    }

    public void OpenPopupRequestSmall()
    {
        int friendCount = SDKGameProfileManager._instance.GetPlayingLineFriendsCount();
        if (friendCount < 6)
            return;

        ServerAPI.RequestedCloverList(recvRequestedCloverListSmall);
    }

    public IEnumerator CoOpenPopupRequestSmall()
    {
        int friendCount = SDKGameProfileManager._instance.GetPlayingLineFriendsCount();
        if (friendCount < 6)
            yield break;

        bool iscomplete = false;
        ServerAPI.RequestedCloverList((resp) =>
        {
            recvRequestedCloverListSmall(resp);
            iscomplete = true;
        });

        yield return new WaitUntil(() => iscomplete);
    }

    void recvRequestedCloverListSmall(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            int nCount = resp.cloverCoolTime.Count;
            //Debug.Log("** Requested Clover List Count : " + nCount + " 명");

            //쿨타임 데이터 들고와서 세팅.
            for (int i = 0; i < nCount; i++)
            {
                if (SDKGameProfileManager._instance.TryGetPlayingFriend(resp.cloverCoolTime[i].fUserKey, out UserFriend user))
                {
                    user.cloverRequestCoolTime = resp.cloverCoolTime[i].reqCoolTime;
                }
            }

            //친구 리스트 검사.
            List<UserFriend> requestPossible = new List<UserFriend>();

            foreach(var key in SDKGameProfileManager._instance.GetPlayingLineFriendKeys())
            {
                if(SDKGameProfileManager._instance.TryGetPlayingFriend(key, out UserFriend user))
                {
                    //초대 가능한 친구들만 넣음.
                    if (user.cloverRequestCoolTime != 0) continue;

                    requestPossible.Add(user);
                }
            }

            //초대가능한 친구 수가 안되면 반환.
            int friendCount = requestPossible.Count;
            if (friendCount < 6)
            {
                return;
            }
            UIPopupRequestCloverSmall popupRequestSmall = NGUITools.AddChild(anchorCenter, _objPopupRequestSmall).GetComponent<UIPopupRequestCloverSmall>();
            popupRequestSmall.OpenPopUp(GetPanelDepth(popupRequestSmall.bShowTopUI, popupRequestSmall.panelCount));
            popupRequestSmall.SettingSortOrder(GetSortLayer(popupRequestSmall.sortOrderCount));
            popupRequestSmall.InitPopUp(requestPossible);
            _popupList.Add(popupRequestSmall);
        }
    }

    public void OpenPopupSpecialEvent(int index, bool notifyGetReward, AppliedRewardSet rewardSet = null)
    {
        UIPopupSpecialEvent popupSpecialEvent = InstantiateUIObject("UIPrefab/UIPopupSpecialEvent", anchorCenter).GetComponent<UIPopupSpecialEvent>();
        popupSpecialEvent.OpenPopUp(GetPanelDepth(popupSpecialEvent.bShowTopUI, popupSpecialEvent.panelCount));
        popupSpecialEvent.SettingSortOrder(GetSortLayer(popupSpecialEvent.sortOrderCount));
        popupSpecialEvent.InitPopUp(index, notifyGetReward, rewardSet);
        _popupList.Add(popupSpecialEvent);
    }

    public void OpenPopupAlphabetEvent(bool displayStartButton = true, AppliedRewardSet rewardSet = null)
    {
        if (ManagerAlphabetEvent.instance == null)
            return;
        UIPopupAlphabetEvent popupAlphabetEvent = InstantiateUIObject("UIPrefab/UIPopupAlphabetEvent", anchorCenter).GetComponent<UIPopupAlphabetEvent>();
        popupAlphabetEvent.OpenPopUp(GetPanelDepth(popupAlphabetEvent.bShowTopUI, popupAlphabetEvent.panelCount));
        popupAlphabetEvent.SettingSortOrder(GetSortLayer(popupAlphabetEvent.sortOrderCount));
        popupAlphabetEvent.InitPopup(displayStartButton, rewardSet);
        _popupList.Add(popupAlphabetEvent);
    }

    #region 턴 릴레이 모드 팝업 관련
    public void OpenPopupTurnRelay_SelectItem(Method.FunctionVoid closeCallBack = null, System.Action<ManagerTurnRelay.BONUSITEM_TYPE> SelectItemAction = null)
    {
        UIPopupTurnRelay_IngameItemSelect popupTurnRelay_SelectItem = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_IngameItemSelect", anchorCenter).GetComponent<UIPopupTurnRelay_IngameItemSelect>();
        popupTurnRelay_SelectItem.OpenPopUp(GetPanelDepth(popupTurnRelay_SelectItem.bShowTopUI, popupTurnRelay_SelectItem.panelCount));
        popupTurnRelay_SelectItem.SettingSortOrder(GetSortLayer(popupTurnRelay_SelectItem.sortOrderCount));
        popupTurnRelay_SelectItem.InitPopup(SelectItemAction);
        if (closeCallBack != null)
            popupTurnRelay_SelectItem._callbackClose += closeCallBack;
        _popupList.Add(popupTurnRelay_SelectItem);
    }

    public UIPopupTurnRelay_IngamePause OpenPopupTurnRelay_IngamePause()
    {
        UIPopupTurnRelay_IngamePause popupPause = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_IngamePause", gameObject).GetComponent<UIPopupTurnRelay_IngamePause>();
        popupPause.OpenPopUp(GetPanelDepth(popupPause.bShowTopUI, popupPause.panelCount));
        popupPause.SettingSortOrder(GetSortLayer(popupPause.sortOrderCount));
        popupPause._callbackEnd += () =>
        {
            if (GameUIManager.instance != null && GameUIManager.instance.listPopupPause.Count > 0)
                GameUIManager.instance.listPopupPause.RemoveAt(0);
        };
        _popupList.Add(popupPause);
        return popupPause;
    }

    public void OpenPopupTurnRelay_IngamePause_Warning(System.Action stopCallback, System.Action closeCallback)
    {
        UIPopupTurnRelay_IngamePause_Warning popupWarning = 
            InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_IngamePause_Warning", gameObject).GetComponent<UIPopupTurnRelay_IngamePause_Warning>();
        popupWarning.OpenPopUp(GetPanelDepth(popupWarning.bShowTopUI, popupWarning.panelCount));
        popupWarning.SettingSortOrder(GetSortLayer(popupWarning.sortOrderCount));
        popupWarning.InitPopup(stopCallback, closeCallback);
        _popupList.Add(popupWarning);
    }

    public void OpenPopupTurnRelay_WaveClear()
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();

        UIPopupTurnRelay_WaveClear popupClear = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_WaveClear", anchorCenter).GetComponent<UIPopupTurnRelay_WaveClear>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopup();
        _popupList.Add(popupClear);
        ManagerUI._instance.FocusCheck();
    }

    public void OpenPopupTurnRelay_IngameClear()
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();

        UIPopupTurnRelay_IngameClear popupClear = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_IngameClear", anchorCenter).GetComponent<UIPopupTurnRelay_IngameClear>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopup();
        _popupList.Add(popupClear);

        ManagerUI._instance.FocusCheck();
    }

    public void OpenPopupTurnRelay_Cooperation(Action getRewardAction, Action endAction)
    {
        UIPopupTurnRelay_Cooperation popupTurnRelay_Cooperation = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_Cooperation", anchorCenter).GetComponent<UIPopupTurnRelay_Cooperation>();
        popupTurnRelay_Cooperation.OpenPopUp(GetPanelDepth(popupTurnRelay_Cooperation.bShowTopUI, popupTurnRelay_Cooperation.panelCount));
        popupTurnRelay_Cooperation.SettingSortOrder(GetSortLayer(popupTurnRelay_Cooperation.sortOrderCount));
        popupTurnRelay_Cooperation.InitCallBack(getRewardAction, endAction);
        _popupList.Add(popupTurnRelay_Cooperation);
    }

    public void OpenPopupTurnRelay_SubMission(Action getRewardAction, Action endAction)
    {
        UIPopupTurnRelay_SubMission popupTurnRelay_SubMission = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_SubMission", anchorCenter).GetComponent<UIPopupTurnRelay_SubMission>();
        popupTurnRelay_SubMission.OpenPopUp(GetPanelDepth(popupTurnRelay_SubMission.bShowTopUI, popupTurnRelay_SubMission.panelCount));
        popupTurnRelay_SubMission.SettingSortOrder(GetSortLayer(popupTurnRelay_SubMission.sortOrderCount));
        popupTurnRelay_SubMission.InitPopup(getRewardAction, endAction);
        _popupList.Add(popupTurnRelay_SubMission);
    }

    public void OpenPopupTurnRelay_APTime(Action rechargeAction, Action endAction)
    {
        UIPopupTurnRelay_APTime popupTurnRelay_APTime = InstantiateUIObject("UIPrefab/UIPopUpTurnRelay_APTime", anchorCenter).GetComponent<UIPopupTurnRelay_APTime>();
        popupTurnRelay_APTime.OpenPopUp(GetPanelDepth(popupTurnRelay_APTime.bShowTopUI, popupTurnRelay_APTime.panelCount));
        popupTurnRelay_APTime.SettingSortOrder(GetSortLayer(popupTurnRelay_APTime.sortOrderCount));
        popupTurnRelay_APTime.InitPopUp(rechargeAction, endAction);
        _popupList.Add(popupTurnRelay_APTime);
    }
    #endregion

    public void OpenPopupDiary(TypePopupDiary in_type, int itemIndex = -1, int tapIndex = 0)
    {
        UIDiaryController._instance.isDiaryActionComplete = false;
        UIPopupDiary popupDiary = InstantiateUIObject("UIPrefab/UIPopupDiary", anchorCenter).GetComponent<UIPopupDiary>();
        popupDiary.OpenPopUp(GetPanelDepth(popupDiary.bShowTopUI, popupDiary.panelCount));
        popupDiary.SettingSortOrder(GetSortLayer(popupDiary.sortOrderCount));
        popupDiary.Init(in_type, itemIndex, tapIndex);
        _popupList.Add(popupDiary);
    }

    public void OpenPopupDiaryMission()
    {
        if (IsLobbyButtonActive)
        {
            OpenPopupDiary(TypePopupDiary.eMission);
        }
    }

    public void ClosePopupAndOpenPopupMission()
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoOpenPopupDiaryCostume(TypePopupDiary.eMission));
    }

    public void ClosePopupAndOpenPopupCostume()
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoOpenPopupDiaryCostume(TypePopupDiary.eCostume));
    }

    public void ClosePoupAndOpenLobbyHousing(int housingIdx, int modelIdx)
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoClosePoupAndOpenLobbyHousing(housingIdx, modelIdx));
    }

    public IEnumerator CoClosePoupAndOpenLobbyHousing(int housingIdx, int modelIdx)
    {
        yield return new WaitForSeconds(0.3f);
        if (ManagerLobby.landIndex != ManagerHousing.GetHousingLandIndex(housingIdx))
        {
            ManagerLobby._instance.MoveLand(ManagerHousing.GetHousingLandIndex(housingIdx),
                () => OpenLobbyHousing(housingIdx, modelIdx));
        }
        else
        {
            OpenLobbyHousing(housingIdx, modelIdx);
        }
    }

    private void OpenLobbyHousing(int housingIdx, int modelIdx)
    {
        //현재 랜드에서 housingIdx를 가진 오브젝트에 포커싱
        var focusPos = ManagerHousing.GetHousingFocusPosition(housingIdx);
        LobbyEntryFocus.ResetFocusCandidates();

        PlusHousingModelData item = ManagerHousing.GetHousingModel(housingIdx, modelIdx);
        if (item.active == 0)
        {
            ManagerUI._instance.OpenPopupHousing(housingIdx, -1, false, focusPos);
            UIPopupHousing._instance._callbackOpen += () => UIPopupHousing._instance.PurchaseHousingItem(modelIdx, modelIdx);
        }
        else
        {
            ManagerUI._instance.OpenPopupHousing(housingIdx, modelIdx, false, focusPos);
        }
    }

    public void OpenQuestBanner(Dictionary<int, QuestAlarmData> dicQuestAlarmData)
    {
        UIQuestBanner questBanner = InstantiateUIObject("UIPrefab/UIQuestBanner", anchorBottomCenter).GetComponent<UIQuestBanner>();
        questBanner.OpenBanner(dicQuestAlarmData);
    }

    public void ClosePopupAndOpenPopupPokoyuraSelect()
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoOpenPokoyuraSelect());
    }

    private IEnumerator CoOpenPokoyuraSelect()
    {
        yield return new WaitForSeconds(0.3f);
        ManagerUI._instance.OpenPopupPokoyuraSelector(true);
    }

    public void ClosePopupAndStartNewPokoyuraScene()
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoStartNewPokoyuraScene());
    }

    private IEnumerator CoStartNewPokoyuraScene()
    {
        yield return new WaitForSeconds(0.3f);
        ManagerLobby._instance.ResetTriggerWakeUp("Extend_pokoura");
        ManagerLobby._instance.PlayTriggerWakeUp("Extend_pokoura");
    }

    public void ClosePopupAndOpenPopupHousing(int itemIndex = -1, int tapIndex = 0)
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoOpenPopupDiaryCostume(TypePopupDiary.eStorage, itemIndex, tapIndex));
    }

    public void ClosePopupAndOpenPopupHousing()
    {
        ManagerUI._instance.ClosePopUpUI();
        Global.InitClickTime();
        StartCoroutine(CoOpenPopupDiaryCostume(TypePopupDiary.eStorage));
    }

    private IEnumerator CoOpenPopupDiaryCostume(TypePopupDiary type, int itemIndex = -1, int tapIndex = 0)
    {
        yield return new WaitForSeconds(0.3f);
        ManagerUI._instance.OpenPopupDiary(type, itemIndex, tapIndex);
    }

    public void OpenPopupLobbyMission(MissionData in_data,Vector3 in_positoin)
    {
        UILobbyMission popup = NGUITools.AddChild(anchorCenter, _objLobbyMission).GetComponent<UILobbyMission>();
        popup.InitLobbyMission(in_data, in_positoin);
    }
    
    public void OnClickOpenPopupRaking()
    {
        if (IsLobbyButtonActive)
        {
            OpenPopupRaking();
        }
    }

    public void OpenPopupRaking()
    {
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            ManagerUI._instance.GuestLoginSignInCheck();
        }
        else
        {
            if (PlayerPrefs.HasKey("rankpopup_info"))
            {
                OpenPopupRaking_BySystem();
            }
            else
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                popupSystem.FunctionSetting(1, "OpenPopupRaking_BySystem", gameObject, true);
                popupSystem.FunctionSetting(3, "OpenPopupRaking_BySystem", gameObject, true);
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_10"), false);
                popupSystem.SetResourceImage("Message/ok");
                PlayerPrefs.SetInt("rankpopup_info", 1);
            }
        }
    }

    public void OpenPopupRaking_BySystem()
    {
        UIPopupRanking popupRanking = InstantiateUIObject("UIPrefab/UIPopUpRanking", anchorCenter).GetComponent<UIPopupRanking>();
        popupRanking.OpenPopUp(GetPanelDepth(popupRanking.bShowTopUI, popupRanking.panelCount));
        popupRanking.SettingSortOrder(GetSortLayer(popupRanking.sortOrderCount));
        _popupList.Add(popupRanking);
    }

    public void OpenPopupUserGradeInfo()
    {
        UIPopupUserGradeInfo popupUserGradeInfo= InstantiateUIObject("UIPrefab/UIPopupUserGradeInfo", gameObject).GetComponent<UIPopupUserGradeInfo>();
        popupUserGradeInfo.OpenPopUp(GetPanelDepth(popupUserGradeInfo.bShowTopUI, popupUserGradeInfo.panelCount));
        popupUserGradeInfo.SettingSortOrder(GetSortLayer(popupUserGradeInfo.sortOrderCount));
        popupUserGradeInfo.Init();
        _popupList.Add(popupUserGradeInfo);
    }

    public void OpenPopupUserProfile(ProfileLookupResp userProfile, UserBase userLineProfile, Method.FunctionVoid callBackClose = null)
    {
        UIPopupUserProfile popupUserProfile = InstantiateUIObject("UIPrefab/UIPopUpUserProfile", anchorCenter).GetComponent<UIPopupUserProfile>();
        popupUserProfile.OpenPopUp(GetPanelDepth(popupUserProfile.bShowTopUI, popupUserProfile.panelCount));
        popupUserProfile.SettingSortOrder(GetSortLayer(popupUserProfile.sortOrderCount));
        popupUserProfile.Init(userProfile, userLineProfile);
        if (callBackClose != null)
        {
            popupUserProfile._callbackClose += callBackClose;
        }
        _popupList.Add(popupUserProfile);
    }

    public UIPopupPokoyuraSelector OpenPopupPokoyuraSelector(bool simpleMode, Method.FunctionVoid callBackClose = null)
    {
        if (_popupList.Count > 0)
            return null;

        if (UIPopupPokoyuraSelector._instance == null)
        {
            if(simpleMode && ServerRepos.User.missionCnt >= 9)
            {
                Character._boni.StartPath(new Vector3(-64f, 0f, -7f), null, true);
            }
            CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(32, -90));
            CameraController._instance.SetFieldOfView(15.5f);

            UIPopupPokoyuraSelector popupPokoyuraSelector = InstantiateUIObject("UIPrefab/UIPopupPokoyuraSelector", anchorCenter).GetComponent<UIPopupPokoyuraSelector>();
            popupPokoyuraSelector.OpenPopUp(GetPanelDepth(popupPokoyuraSelector.bShowTopUI, popupPokoyuraSelector.panelCount));
            popupPokoyuraSelector.SettingSortOrder(GetSortLayer(popupPokoyuraSelector.sortOrderCount));
            popupPokoyuraSelector.Init(simpleMode);
            if (callBackClose != null)
            {
                popupPokoyuraSelector._callbackClose += callBackClose;
            }
            _popupList.Add(popupPokoyuraSelector);
            return popupPokoyuraSelector;
        }
        else
        {
            return UIPopupPokoyuraSelector._instance;
        }
    }

    public void OpenPopupPokoYuraInfo()
    {
        UIPopupPokoYuraInfo popupPokoYuraInfo = NGUITools.AddChild(anchorCenter, _objPopupPokoYuraInfo).GetComponent<UIPopupPokoYuraInfo>();
        popupPokoYuraInfo.OpenPopUp(GetPanelDepth(popupPokoYuraInfo.bShowTopUI, popupPokoYuraInfo.panelCount));
        popupPokoYuraInfo.SettingSortOrder(GetSortLayer(popupPokoYuraInfo.sortOrderCount));
        _popupList.Add(popupPokoYuraInfo);
    }
    
    public void OnClickGallery()
    {
        if (IsLobbyButtonActive)
        {
            OpenPopupGallery();
        }
    }

    public void OpenPopupGallery(Method.FunctionVoid callbackOpen = null, Method.FunctionVoid callbackClose = null)
    {
        var popup = InstantiateUIObject("UIPrefab/UIPopupGallery", gameObject).GetComponent<UIPopupGallery>();
        popup.OpenPopUp(GetPanelDepth(popup.bShowTopUI, popup.panelCount));
        popup.SettingSortOrder(GetSortLayer(popup.sortOrderCount));
        popup.Init();
        popup._callbackOpen = callbackOpen;
        popup._callbackEnd  = callbackClose;
        _popupList.Add(popup);
    }

    public void OpenPopupMailBox()
    {
        if (IsLobbyButtonActive)
        {
            ServerAPI.Inbox(recvMessageBox, 0, 1);
        }
    }

    void recvMessageBox(UserInboxResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupMailBox popupMailBox = InstantiateUIObject("UIPrefab/UIPopupMailBox", gameObject).GetComponent<UIPopupMailBox>();
            popupMailBox.OpenPopUp(GetPanelDepth(popupMailBox.bShowTopUI, popupMailBox.panelCount));
            popupMailBox.SettingSortOrder(GetSortLayer(popupMailBox.sortOrderCount));
            popupMailBox.SetTabMailCount(resp.tabMailCnt);
            popupMailBox.SetMailBox();
            _popupList.Add(popupMailBox);
        }
    }

    public bool IsMenuOpened()
    {
        return menuBg.gameObject.activeSelf;
    }

    public void OnClickMenuTab()
    {
        if (IsLobbyButtonActive)
        {
            if (bCanMenuClick == false)
                return;

            StartCoroutine(CoMenuOpenCloseEffect(rootMenuAlarm.activeSelf));
        }
        else //로비가 아니라면 닫기
        {
            StartCoroutine(CoMenuOpenCloseEffect(false));
        }
    }

    //메뉴 버튼 클릭 시 여닫는 이펙트 입니다. 백그라운드, 하위 그리드의 포지션을 이동시키며 그리드 이동에따라 꺼져있거나 켜져있던 하위 UI들을 켜고 끕니다.
    private IEnumerator CoMenuOpenCloseEffect(bool isOpen)
    {
        float effectTime = 0.5f;
        float gridOpenLPos = menuGridChild.Count * -100;
        float toBgPosX = isOpen? gridOpenLPos * 0.5f -10 : -10;
        int toWidth = isOpen? -(int)gridOpenLPos + 80 : 80;
        float toPosX = isOpen? gridOpenLPos +10: 10;

        //메뉴를 열 때 하위 항목 그리드와 백그라운드를 켭니다.
        if (isOpen)
        {
            rightDownGrid.gameObject.SetActive(isOpen);
            menuBg.gameObject.SetActive(isOpen);
            SettingRankingPokoYura(false);
        }
        
        //백그라운드의 위치를 옮깁니다.
        DOTween.To(() => menuBg.transform.localPosition.x, x =>
        {
            Vector2 toPos = new Vector2(x, menuBg.transform.localPosition.y);
            menuBg.transform.localPosition = toPos;
        }, toBgPosX, effectTime);
        
        DOTween.To(() => menuBg.width, x => menuBg.width = x, toWidth, effectTime);

        //그리드의 위치를 옮깁니다.
        DOTween.To(() => rightDownGrid.transform.localPosition.x, x =>
        {
            Vector2 toPos = new Vector2(x, rightDownGrid.transform.localPosition.y);
            rightDownGrid.transform.localPosition = toPos;
        }, toPosX, effectTime);
        
        //위 연출이 진행 중일 때 변화되는 위치에따라 타이밍에 맞춰서 하위 항목들을 켜고 끕니다.
        if (isOpen)
        {
            for (int i = 0; i < menuGridChild.Count; i++)
            {
                while (menuGridChild[i].activeSelf == false)
                {
                    if (rightDownGrid.transform.localPosition.x <= -100 * i + 10)
                    {
                        menuGridChild[i]?.SetActive(true);
                        //친구 깨우기 버블의 온오프 타이밍을 맞추기 위해 내부에서 알람을 온오프합니다.
                        if(i == 0)
                            rootMenuAlarm.SetActive(false);
                    }

                    yield return null;
                }
            }
        }
        else
        {
            for (int i = menuGridChild.Count - 1; i >= 0; i--)
            {
                while (menuGridChild[i].activeSelf == true)
                {
                    if (rightDownGrid.transform.localPosition.x >= gridOpenLPos + (100 * (menuGridChild.Count - i)))
                    {
                        menuGridChild[i].SetActive(false);
                        //친구 깨우기 버블의 온오프 타이밍을 맞추기 위해 내부에서 알람을 온오프합니다.
                        if(i == 0)
                            rootMenuAlarm.SetActive(true);
                    }
                    
                    yield return null;
                }
            }
        }
        
        //닫을 때 백그라운드와 그리드를 끕니다.
        if (!isOpen)
        {
            rightDownGrid.gameObject.SetActive(isOpen);
            menuBg.gameObject.SetActive(isOpen);
        }

        bCanMenuClick = true;
    }
    
    public void OpenPopupComingSoon()
    {
        if (ManagerData._instance._stageData[0]._flowerLevel == 0)
        {
            recvGameList_popType = 1;
            ServerAPI.GetUserStageList(recvGameList);
        }
        else
        {
            UIPopupComingSoon popupComingSoon = InstantiateUIObject("UIPrefab/UIPopUpComingSoon", anchorCenter).GetComponent<UIPopupComingSoon>();
            popupComingSoon.OpenPopUp(GetPanelDepth(popupComingSoon.bShowTopUI, popupComingSoon.panelCount));
            popupComingSoon.SettingSortOrder(GetSortLayer(popupComingSoon.sortOrderCount));
            popupComingSoon.InitPopUp();
            _popupList.Add(popupComingSoon);
        }
    }

    public void OpenPopupPackage(CdnShopPackage packageData, Method.FunctionVoid func = null)
    {
        UIPopupPackage popupPackage = InstantiateUIObject("UIPrefab/UIPopupPackage", anchorCenter).GetComponent<UIPopupPackage>();
        popupPackage.OpenPopUp(GetPanelDepth(popupPackage.bShowTopUI, popupPackage.panelCount));
        popupPackage.SettingSortOrder(GetSortLayer(popupPackage.sortOrderCount));
        popupPackage.InitPopUp(packageData, func);
        _popupList.Add(popupPackage);
    }
    
    public void OpenPopupPackage_Reconfirm(CdnShopPackage packageData, Method.FunctionVoid func = null)
    {
        UIPopupPackage popupPackage = InstantiateUIObject("UIPrefab/UIPopupPackage_Reconfirm", anchorCenter).GetComponent<UIPopupPackage>();
        popupPackage.OpenPopUp(GetPanelDepth(popupPackage.bShowTopUI, popupPackage.panelCount));
        popupPackage.SettingSortOrder(GetSortLayer(popupPackage.sortOrderCount));
        popupPackage.InitPopUp(packageData, func);
        _popupList.Add(popupPackage);
    }
    
    public async UniTask AsyncOpenPopupSpotDiaPackage_Reconfirm(CdnShopPackage packageData, CdnSpotDiaPackage spotDiaData, Method.FunctionVoid func = null)
    {
        await ManagerResourceLoader.instance.AsyncLoadResource(ResourceType.SPOT_DIA_SPINE);
        var popupPackage = InstantiateUIObject("UIPrefab/UIPopUpPackage_SpotDia", anchorCenter).GetComponent<UIPopupPackage>();
        popupPackage.OpenPopUp(GetPanelDepth(popupPackage.bShowTopUI, popupPackage.panelCount));
        popupPackage.SettingSortOrder(GetSortLayer(popupPackage.sortOrderCount));
        popupPackage.InitPopUp(packageData, func);
        popupPackage.InitPopup_SpotDia(GetSortLayer(popupPackage.sortOrderCount), spotDiaData);
        _popupList.Add(popupPackage);
    }

    public void OpenPopupMissionStampEvent(int eventIndex)
    {
        UIPopupMissionStampEvent stampEvent = InstantiateUIObject("UIPrefab/UIPopupMissionStampEvent", anchorCenter).GetComponent<UIPopupMissionStampEvent>();
        stampEvent.eventIndex = eventIndex;
        stampEvent.OpenPopUp(GetPanelDepth(stampEvent.bShowTopUI, stampEvent.panelCount));
        stampEvent.SettingSortOrder(GetSortLayer(stampEvent.sortOrderCount));
        _popupList.Add(stampEvent);
    }

    public void OpenPopupNoticeHelp(Notice noticeIndex)
    {
        UIPopupNoticeHelp popupNoticeHelp = InstantiateUIObject("UIPrefab/UIPopupNoticeHelp", anchorCenter).GetComponent<UIPopupNoticeHelp>();
        popupNoticeHelp.OpenPopUp(GetPanelDepth(popupNoticeHelp.bShowTopUI, popupNoticeHelp.panelCount));
        popupNoticeHelp.SettingSortOrder(GetSortLayer(popupNoticeHelp.sortOrderCount));
        popupNoticeHelp.InitPopUp(noticeIndex);
        _popupList.Add(popupNoticeHelp);
    }

    public void OpenPopupOption()
    {
        if (IsLobbyButtonActive)
        {
            UIPopupOption popupOption = InstantiateUIObject("UIPrefab/UIPopupOption", anchorCenter).GetComponent<UIPopupOption>();
            popupOption.OpenPopUp(GetPanelDepth(popupOption.bShowTopUI, popupOption.panelCount));
            popupOption.SettingSortOrder(GetSortLayer(popupOption.sortOrderCount));
            _popupList.Add(popupOption);
        }
    }

    public void OpenPopupVideo(int movieIndex)
    {
        UIPopupVideo popupVideo = InstantiateUIObject("UIPrefab/UIPopupVideo", this.gameObject).GetComponent<UIPopupVideo>();
        popupVideo.OpenPopUp(GetPanelDepth(popupVideo.bShowTopUI, popupVideo.panelCount));
        popupVideo.SettingSortOrder(GetSortLayer(popupVideo.sortOrderCount));
        popupVideo.InitPopUp(movieIndex);
        _popupList.Add(popupVideo);
    }

    public void OpenPopupGenericPage(string pageName)
    {
        UIPopupGenericPage popup = InstantiateUIObject("UIPrefab/UIPopupGenericPage", this.gameObject).GetComponent<UIPopupGenericPage>();
        popup.OpenPopUp(GetPanelDepth(popup.bShowTopUI, popup.panelCount));
        popup.SettingSortOrder(GetSortLayer(popup.sortOrderCount));
        popup.InitPopUp(pageName);
        _popupList.Add(popup);
    }

    public void OpenPopupUserInfo()
    {
        UIPopUpUserInfo popupUserInfo = InstantiateUIObject("UIPrefab/UIPopUpUserInfo", anchorCenter).GetComponent<UIPopUpUserInfo>();
        popupUserInfo.OpenPopUp(GetPanelDepth(popupUserInfo.bShowTopUI, popupUserInfo.panelCount));
        popupUserInfo.SettingSortOrder(GetSortLayer(popupUserInfo.sortOrderCount));
        _popupList.Add(popupUserInfo);
    }

    public void OpenPopupPushSetting()
    {
        UIPopUpPushSetting popupPushSetting = InstantiateUIObject("UIPrefab/UIPopUpPushSetting", anchorCenter).GetComponent<UIPopUpPushSetting>();
        popupPushSetting.OpenPopUp(GetPanelDepth(popupPushSetting.bShowTopUI, popupPushSetting.panelCount));
        popupPushSetting.SettingSortOrder(GetSortLayer(popupPushSetting.sortOrderCount));
        _popupList.Add(popupPushSetting);
    }

    public void OpenPopupPlusInfo(List<ServerImageLink> links)
    {
        UIPopupPlusInfo popupPlusInfo = InstantiateUIObject("UIPrefab/UIPopupPlusInfo", anchorCenter).GetComponent<UIPopupPlusInfo>();
        popupPlusInfo.OpenPopUp(GetPanelDepth(popupPlusInfo.bShowTopUI, popupPlusInfo.panelCount));
        popupPlusInfo.SettingSortOrder(GetSortLayer(popupPlusInfo.sortOrderCount));
        popupPlusInfo.InitPopUp(links);
        _popupList.Add(popupPlusInfo);
    }

    public GameObject OpenPopupSystem(bool bShowTopUI = false)
    {
        UIPopupSystem popupSystem = InstantiateUIObject("UIPrefab/UIPopupSystem", anchorCenter).GetComponent<UIPopupSystem>();
        popupSystem.bShowTopUI = bShowTopUI;
        popupSystem.OpenPopUp(GetPanelDepth(popupSystem.bShowTopUI, popupSystem.panelCount));
        popupSystem.SettingSortOrder(GetSortLayer(popupSystem.sortOrderCount));
        _popupList.Add(popupSystem);
        return popupSystem.gameObject;
    }

    public void OpenPopupHousing(int housingIdx, int selectIndex, bool bSelectAction, Vector3 focusPos, Method.FunctionVoid in_callback = null)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();

        UIPopupHousing popupHousing = InstantiateUIObject("UIPrefab/UIPopupHousing", anchorCenter).GetComponent<UIPopupHousing>();
        popupHousing.OpenPopUp(GetPanelDepth(popupHousing.bShowTopUI, popupHousing.panelCount));
        popupHousing.SettingSortOrder(GetSortLayer(popupHousing.sortOrderCount));
        popupHousing.InitHousing(housingIdx, selectIndex, bSelectAction, focusPos, in_callback);
        _popupList.Add(popupHousing);
    }

    public IEnumerator OpenPopupLoginBonus(int conceptIdx)
    { 
        GameObject bundleObject = null;
        IEnumerator i = LoadLoginBonusObject(conceptIdx);
        while(i.MoveNext())
            yield return i.Current;

        if( i.Current != null )
        {
            bundleObject = i.Current as GameObject;
            if (bundleObject == null)
                yield break;
        }

        //UIPopupLoginBonus popupLoginBonus = InstantiateUIObject("UIPrefab/UIPopUpLoginBonus", this.gameObject).GetComponent<UIPopupLoginBonus>();
        //popupLoginBonus.OpenPopUp(GetPanelDepth(popupLoginBonus.bShowTopUI, popupLoginBonus.panelCount));
        //popupLoginBonus.SettingSortOrder(GetSortLayer(popupLoginBonus.sortOrderCount));
        //popupLoginBonus.InitPopup(Instantiate(bundleObject));
        //_popupList.Add(popupLoginBonus);

        ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupLoginBonus>((popup) => popup.InitPopup(Instantiate(bundleObject)));

        yield break;
    }

    private IEnumerator LoadLoginBonusObject(int conceptIdx)
    {
        GameObject BundleObject = null;
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path = string.Format("Assets/5_OutResource/loginbonuses/loginbonus_v2_{0}/UILoginBonus_{0}.prefab", conceptIdx);
            BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
        }
        else
        {
            string assetName = string.Format("login_b_v2_{0}", conceptIdx);
            string prefabName = string.Format("UILoginBonus_{0}", conceptIdx);
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    BundleObject = assetBundle.LoadAsset<GameObject>(prefabName);
                }
            }
        }

        var loginBonus = BundleObject.GetComponent<UILoginBonus>();
        bool isComplete = false;

        if (!string.IsNullOrEmpty(loginBonus.textFileName))
        {
            StringHelper.LoadStringFromCDN(loginBonus.textFileName, Global._instance._stringData, complete: (c) => isComplete = true);
        }
        else
        {
            isComplete = true;
        }

        yield return new WaitUntil(() => isComplete);

        yield return BundleObject;
    }

    public void OpenPopupLuckyRoulette()
    {
        CoOpenPopupLuckyRoulette().Forget();
    }
    
    private async UniTaskVoid CoOpenPopupLuckyRoulette()
    {
        var e = ManagerLuckyRoulette.instance.CoLoadAssetBundle();
        
        while (e.MoveNext())
        {
            await UniTask.Yield();
        }
        
        UIPopupLuckyRoulette popupLuckyRoulette = InstantiateUIObject("UIPrefab/UIPopupLuckyRoulette", gameObject).GetComponent<UIPopupLuckyRoulette>();
        popupLuckyRoulette.OpenPopUp(GetPanelDepth(popupLuckyRoulette.bShowTopUI, popupLuckyRoulette.panelCount));
        popupLuckyRoulette.SettingSortOrder(GetSortLayer(popupLuckyRoulette.sortOrderCount));
        _popupList.Add(popupLuckyRoulette);
        popupLuckyRoulette.InitData();
    }
    
    public void OpenPopupAtelier() => StartCoroutine(CoOpenPopupAtelier());

    public IEnumerator CoOpenPopupAtelier(bool stageClear = false)
    {
        ManagerAtelier.instance.CoLoadAssetBundle<AtelierPack>().Forget();
        yield return new WaitWhile(() => ManagerAtelier.instance._atelierPack == null);

        var popup = InstantiateUIObject("UIPrefab/UIPopUpAtelier", gameObject).GetComponent<UIPopupAtelier>();
        popup.OpenPopUp(GetPanelDepth(popup.bShowTopUI, popup.panelCount));
        popup.SettingSortOrder(GetSortLayer(popup.sortOrderCount));
        _popupList.Add(popup);
        popup.InitContentUI(stageClear);
    }


    //public void OpenPopupDiamondShop(bool afterLack)
    //{
    //    UIPopupShop popupShop = InstantiateUIObject("UIPrefab/UIPopupShop", gameObject).GetComponent<UIPopupShop>();
    //    popupShop.Init(UIPopupShop.ShopType.Diamond);
    //    popupShop.OpenPopUp(GetPanelDepth(popupShop.bShowTopUI, popupShop.panelCount));
    //    popupShop.SettingSortOrder(GetSortLayer(popupShop.sortOrderCount));
    //    if (afterLack)
    //        popupShop._callbackOpen = () =>
    //        {
    //            FailCountManager.CheckSpotDiamondPackage();
    //            ManagerUI._instance.UpdateUI();
    //        };
    //    _popupList.Add(popupShop);
    //}

    public void OpenPopupRankUp(int classNum,int pokoyura)
    {
        UIPopupRankUp popupRankUp = NGUITools.AddChild(gameObject, _objPopupRankUp).GetComponent<UIPopupRankUp>();
        popupRankUp.OpenPopUp(GetPanelDepth(popupRankUp.bShowTopUI, popupRankUp.panelCount));
        popupRankUp.SettingSortOrder(GetSortLayer(popupRankUp.sortOrderCount));
        popupRankUp.InitRankUpPopup(classNum, pokoyura);
        _popupList.Add(popupRankUp);
    }

    public UIPopUpOpenGiftBox OpenPopupGiftBox()
    {
        //   ServerAPI.OpenGiftBox(1,recvOpenGiftBox);

        UIPopUpOpenGiftBox popupOpenGiftBox = InstantiateUIObject("UIPrefab/UIPopUpOpenGiftBox", gameObject).GetComponent<UIPopUpOpenGiftBox>();
        popupOpenGiftBox.OpenPopUp(GetPanelDepth(popupOpenGiftBox.bShowTopUI, popupOpenGiftBox.panelCount));
        popupOpenGiftBox.SettingSortOrder(GetSortLayer(popupOpenGiftBox.sortOrderCount));
        _popupList.Add(popupOpenGiftBox);

        ManagerUI._instance.FocusCheck();

        return popupOpenGiftBox;
    }
    void recvOpenGiftBox(OpenGiftBoxResp resp)
    {
        if (resp.IsSuccess)
        {

        }
    }
    public void OpenPopupMaterial(long openTime = 0)
    {
        UIPopupMaterial popupMaterial = InstantiateUIObject("UIPrefab/UIPopupMaterial", gameObject).GetComponent<UIPopupMaterial>();
        popupMaterial.OpenPopUp(GetPanelDepth(popupMaterial.bShowTopUI, popupMaterial.panelCount));
        popupMaterial.SettingSortOrder(GetSortLayer(popupMaterial.sortOrderCount));
        popupMaterial.InitMaterialPopup(openTime);
        _popupList.Add(popupMaterial);
    }

    public void OpenPopupMaterialInfo(string name, MaterialGetInfo info)
    {
        UIPopupMaterialInfo popupMaterialInfo = InstantiateUIObject("UIPrefab/UIPopUpMaterialInfo", gameObject).GetComponent<UIPopupMaterialInfo>();
        popupMaterialInfo.OpenPopUp(GetPanelDepth(popupMaterialInfo.bShowTopUI, popupMaterialInfo.panelCount));
        popupMaterialInfo.SettingSortOrder(GetSortLayer(popupMaterialInfo.sortOrderCount));
        popupMaterialInfo.InitMaterialInfo(name, info);
        _popupList.Add(popupMaterialInfo);
    }
    
    public void OpenPopupStage()
    {
        if (IsLobbyButtonActive)
        {
            UIReuseGrid_Stage.StageScrollMode = UIReuseGrid_Stage.ScrollMode.None;
            OpenPopupStageAction();
        }
    }

    public void OpenPopupStage(UIReuseGrid_Stage.ScrollMode scrollMode)
    {
        if (IsLobbyButtonActive)
        {
            UIReuseGrid_Stage.StageScrollMode = scrollMode;
            OpenPopupStageAction();
        }
    }

    public void OpenPopupStage(int stageIndex)
    {
        if (IsLobbyButtonActive)
        {
            UIReuseGrid_Stage.CustomModeIndex = stageIndex;
            UIReuseGrid_Stage.StageScrollMode = UIReuseGrid_Stage.ScrollMode.CustomFind;
            OpenPopupStageAction();
        }
    }

    public void OpenPopupStageAction(bool bChangeAction = false)
    {
        if (CheckExitUI() == true)
            return;

        // 스테이지 정보가 비어있으면 받아서 창열기
        if (ManagerData._instance._stageData[0]._flowerLevel == 0)
            ServerAPI.GetUserStageList(recvGameList);
        else
        {
            UIPopupStage popupStage = InstantiateUIObject("UIPrefab/UIPopUpStage", anchorCenter).GetComponent<UIPopupStage>();
            popupStage.transform.localScale = Vector3.one;
            popupStage.OpenPopUp(GetPanelDepth(popupStage.bShowTopUI, popupStage.panelCount));
            popupStage.InitStagePopUp(bChangeAction);
            popupStage.SettingSortOrder(GetSortLayer(popupStage.sortOrderCount));
            _popupList.Add(popupStage);

            FocusCheck();
        }
    }

    int recvGameList_popType = 0;
    void recvGameList(BaseResp resp)
    {
        if (resp.IsSuccess)
        {

            foreach (var item in ServerRepos.UserStages)
            {
                //Debug.Log(item.stage + "___play" + item.play + "  score" + item.score + "  score" + item.score);

                StageData stage = ManagerData._instance._stageData[item.stage - 1];
                stage._continue = item.continue_;
                stage._play = item.play;
                stage._fail = item.fail;
                stage._score = item.score;
                stage._missionProg1 = item.mprog1;
                stage._missionProg2 = item.mprog2;
                stage._flowerLevel = item.flowerLevel;
                stage._missionClear = item.missionClear;
            }

            if (recvGameList_popType == 1)
            {
                UIPopupComingSoon popupComingSoon = InstantiateUIObject("UIPrefab/UIPopUpComingSoon", anchorCenter).GetComponent<UIPopupComingSoon>();
                popupComingSoon.OpenPopUp(GetPanelDepth(popupComingSoon.bShowTopUI, popupComingSoon.panelCount));
                popupComingSoon.SettingSortOrder(GetSortLayer(popupComingSoon.sortOrderCount));
                popupComingSoon.InitPopUp();
                _popupList.Add(popupComingSoon);
            }
            else
            {
                UIPopupStage popupStage = InstantiateUIObject("UIPrefab/UIPopUpStage", anchorCenter).GetComponent<UIPopupStage>();
                popupStage.transform.localScale = Vector3.one;
                popupStage.OpenPopUp(GetPanelDepth(popupStage.bShowTopUI, popupStage.panelCount));
                popupStage.InitStagePopUp(false);
                popupStage.SettingSortOrder(GetSortLayer(popupStage.sortOrderCount));
                _popupList.Add(popupStage);
            }
        }
        recvGameList_popType = 0;
    }

    public void OpenPopupFlowerInfo()
    {
        UIPopupFlowerInfo popupFlowerInfo = InstantiateUIObject("UIPrefab/UIPopUpFlowerInfo", anchorCenter).GetComponent<UIPopupFlowerInfo>();
        popupFlowerInfo.OpenPopUp(GetPanelDepth(popupFlowerInfo.bShowTopUI, popupFlowerInfo.panelCount));
        popupFlowerInfo.SettingSortOrder(GetSortLayer(popupFlowerInfo.sortOrderCount));
        _popupList.Add(popupFlowerInfo);
    }

    public void OpenPopupFlowerReward(int topIndex)
    {
        UIPopupFlowerReward popupFlowerReward = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIPopupFlowerReward", gameObject).GetComponent<UIPopupFlowerReward>();
        popupFlowerReward.OpenPopUp(GetPanelDepth(popupFlowerReward.bShowTopUI, popupFlowerReward.panelCount));
        popupFlowerReward.SettingSortOrder(GetSortLayer(popupFlowerReward.sortOrderCount));
        popupFlowerReward.InitFlowerCount(topIndex);
        _popupList.Add(popupFlowerReward);
    }

    public void OpenPopupSingleRoundEvent(StageMapData tempData, Method.FunctionVoid callBackStart, Method.FunctionVoid callBackClose, System.Action callBackCancel)
    {
        UIPopupSingleRoundEvent popupSingleRoundEvent = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIPopupSingleRoundEvent", gameObject).GetComponent<UIPopupSingleRoundEvent>();
        
        if (callBackStart != null)
        {
            popupSingleRoundEvent._callbackOpen += callBackStart;
        }
        if (callBackClose != null)
        {
            popupSingleRoundEvent._callbackClose += callBackClose;
        }
        if (popupSingleRoundEvent != null)
        {
            popupSingleRoundEvent.callBackButtonCancel += callBackCancel;
        }
        popupSingleRoundEvent.OpenPopUp(GetPanelDepth(popupSingleRoundEvent.bShowTopUI, popupSingleRoundEvent.panelCount));
        popupSingleRoundEvent.SettingSortOrder(GetSortLayer(popupSingleRoundEvent.sortOrderCount));
        popupSingleRoundEvent.Init(tempData);

        _popupList.Add(popupSingleRoundEvent);
        
        IsOpenReady = false;
        NetworkLoading.EndNetworkLoading();
    }

    public void OpenPopupReadyLastStage()
    {
        if (IsCanPlayLastState() == false)
        {
            OpenCantPlaySystemPopup();
        }
        else
        {
            if (IsLobbyButtonActive)
            {
                OpenPopupReadyLastStage(true);
            }
        }
    }

    public bool IsCanPlayLastState()
    {
        if ((GameData.User.stage >= 10 && GameData.User.missionCnt < 9)
               || (GameData.User.stage >= 36 && GameData.User.missionCnt < 26))
            return false;
        else
            return true;
    }

    public void OpenCantPlaySystemPopup()
    {
        string msgText = "";
        if (GameData.User.stage >= 10 && GameData.User.missionCnt < 9)
            msgText = Global._instance.GetString("n_s_62");
        else if (GameData.User.stage >= 36 && GameData.User.missionCnt < 26)
            msgText = Global._instance.GetString("n_s_63");

        ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
        {
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), msgText, false);
            popup._callbackEnd += () => { StartCoroutine(CoOpenDiaryMissionAfterClosePopup()); };
        });
    }

    private IEnumerator CoOpenDiaryMissionAfterClosePopup()
    {
        int popupCnt = _popupList.Count;
        for (int i = 0; i < popupCnt; i++)
        {
            ClosePopUpUI();
        }

        if (popupCnt > 0)
            yield return new WaitForSeconds(0.3f);

        OpenPopupDiaryMission();
    }

    bool IsOpenReady = false;
    public void OpenPopupReadyLastStage(bool checkMainComingSoon)
    {
        if (checkMainComingSoon && ManagerUI._instance._rootStageFlower.gameObject.activeInHierarchy == true)
        {
            ManagerUI._instance.OpenPopupComingSoon();
        }
        else
        {
            int pCount = _popupList.Count;
            if (pCount > 0 && _popupList[pCount - 1].name == "UIPopUpReady")
                return;

            if (IsOpenReady)
                return;
            IsOpenReady = true;
            NetworkLoading.MakeNetworkLoading(0.5f);

            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            int stageIndex = myProfile.stage > ManagerData._instance.maxStageCount ? (int)ManagerData._instance.maxStageCount : (int)myProfile.stage;

            Global.SetGameType_NormalGame(stageIndex);

            StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
        }
    }
    
    
    /// <summary>
    /// 특정 인덱스 스테이지 호출 시 사용
    /// </summary>
    public void OpenPopupReadyIndexStage(int stageIndex)
    {
        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].name == "UIPopUpReady")
            return;
        if (IsOpenReady)
            return;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);
        Global.SetGameType_NormalGame(stageIndex);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
    }

    public void OpenPopupReadyStageCallBack(Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart, callBackClose, callBackCancel));
    }

    /// <summary>
    /// 이전에 플레이한 맵의 타겟 데이터를 포함해 레디창 호출할때 사용
    /// </summary>
    public void OpenPopupReadyUsePrevTargetData(Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null)    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart, callBackClose, callBackCancel, true));
    }


    public void OpenPopupReadyStageEvent(Method.FunctionVoid callBackStart = null)
    {
        if (IsOpenReady)
            return;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);

        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public IEnumerator CoOpenPopupReadyStageEvent(Method.FunctionVoid callBackStart = null)
    {
        if (IsOpenReady)
            yield break;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);
        yield return StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public void OpenPopupReadyMoleCatch(Method.FunctionVoid callBackStart = null)
    {
        if (IsOpenReady)
            return;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);

        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }
    
    public void OpenPopupReadyEndContents(Method.FunctionVoid callBackStart = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        Global.SetGameType_EndContents(ManagerEndContentsEvent.instance.EventIndex, ManagerEndContentsEvent.instance.MapIndex);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }
    
    public IEnumerator CoOpenPopupTreasure()
    {
        if (ManagerTreasureHunt.instance.treasureHuntPack == null)
            yield return ManagerTreasureHunt.instance.LoadTreasureHuntResource();
        var popup = _instance.OpenPopup<UIPopupTreasureHunt>();
        popup.InitPopup();
    }
    
    public void OpenPopupReadyTreasure(int stageIndex, Method.FunctionVoid callBackStart = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        Global.SetGameType_TreasureHunt(ManagerTreasureHunt.instance.EventIndex, stageIndex);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public void OpenPopupReadyBingoEvent(int stageIndex, Method.FunctionVoid callBackStart = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        Global.SetGameType_BingoEvent(stageIndex, ManagerBingoEvent.instance.EventIndex);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public void OpenPopupReadyCoinStage(int stagePlayTIme = -1)
    {
        UIPopupReadyCoinStage popupReadyCoinStage = InstantiateUIObject("UIPrefab/UIPopUpReadyCoinStage", gameObject).GetComponent<UIPopupReadyCoinStage>();
        popupReadyCoinStage.OpenPopUp(GetPanelDepth(popupReadyCoinStage.bShowTopUI, popupReadyCoinStage.panelCount));
        popupReadyCoinStage.SettingSortOrder(GetSortLayer(popupReadyCoinStage.sortOrderCount));
        popupReadyCoinStage.InitPopup(stagePlayTIme);
        _popupList.Add(popupReadyCoinStage);
    }

    public void OpenPopupReadyAtelier(int stageIndex, Method.FunctionVoid callBackStart = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        Global.SetGameType_Atelier(ManagerAtelier.instance._vsn, stageIndex);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public void MakeMaterialIcon(int mCount)
    {
        if (UIButtonMaterial._instance == null)
        {
            InstantiateUIObject("UIPrefab/UIButtonMaterial", anchorBottomLeft.gameObject);

            anchorBottomLeft.AddLobbyButton(UIButtonMaterial._instance.gameObject);
            UIButtonMaterial._instance.FirstReset(mCount);
        }
        else
            UIButtonMaterial._instance.Reset();
        //StartCoroutine(UIButtonMaterial._instance.CoSetMaterialIcon(mCount));
    }

    public void MakeLobbyGuestIcon()
    {
        if (UIButtonLobbyGuest._instance == null)
        {
            InstantiateUIObject("UIPrefab/UIButtonLobbyGuest", anchorBottomLeft.gameObject);
            anchorBottomLeft.AddLobbyButton(UIButtonLobbyGuest._instance.gameObject);
        }
        UIButtonLobbyGuest._instance.Reset();
    }

    public void MakeAdventureModeIcon()
    {
        if (anchorBottomLeft.CheckButtonAdd<UIButtonAdventure>() == false)
            return;
        
        btnAdventure = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIButtonAdventure", anchorBottomLeft.gameObject).GetComponent<UIButtonAdventure>();
        btnAdventure.SetButtonEvent(2);
        anchorBottomLeft.AddLobbyButton(btnAdventure.gameObject);
    }

    public void MakeLandMoveIcon()
    {
        if (anchorBottomLeft.CheckButtonAdd<UIButtonLandMove>() == false)
            return;

        var btnLandMove = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIButtonLandMove", anchorBottomLeft.gameObject).GetComponent<UIButtonLandMove>();
        btnLandMove.SetButtonEvent(1);
        anchorBottomLeft.AddLobbyButton(btnLandMove.gameObject);
    }

    public void MakePlayVideoIcon(int videoIndex)
    {
        if (anchorTopLeft.CheckButtonAdd<UIButtonPlayVideo>() == false)
            return;

        var btnBoxShop = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIButtonPlayVideo", anchorTopLeft.gameObject).GetComponent<UIButtonPlayVideo>();
        btnBoxShop.SetButtonEvent(videoIndex);
        anchorTopLeft.AddLobbyButton(btnBoxShop.gameObject);
    }

    public void MakeGenericPageIcon(int eventIdx, string pageName, long endTs, bool needLogin)
    {
        if (anchorTopLeft.CheckEventButtonAdd<UIButtonGenericPage>(eventIdx) == false)
            return;

        var btnGenericPage = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIButtonGenericPage", anchorTopLeft.gameObject).GetComponent<UIButtonGenericPage>();
        btnGenericPage.SetButtonEvent(eventIdx);
        btnGenericPage.page = pageName;
        btnGenericPage.endTs = endTs;
        btnGenericPage.needLogin = needLogin;

        anchorTopLeft.AddLobbyButton(btnGenericPage.gameObject);
    }

    public void MakePromotionEventIcon(int resourceIdx)
    {
        if (anchorTopLeft.CheckButtonAdd<UIButtonPromotion>() == false)
            return;
        UIButtonPromotion btnEvent = InstantiateUIObject("UIPrefab/UIButtonPromotion", anchorTopLeft.gameObject).GetComponent<UIButtonPromotion>();
        anchorTopLeft.AddLobbyButton(btnEvent.gameObject);
        btnEvent.SetButtonEvent(resourceIdx);
    }

    public UIHousingGauge MakeHousingGaugeUI()
    {
        return InstantiateUIObject("UIPrefab/UIHousingGauge", anchorCenter).GetComponent<UIHousingGauge>();
    }

    public void RefreshLimitedMaterialRegenForecast(bool isNormalMaterial = false)
    {
        if (isNormalMaterial)
            return;
        
        UIButtonLimitedMaterialForecast.RemoveAll();

        foreach (var matMeta in ServerContents.MaterialMeta)
        {
            if (matMeta.Value.expireTs != 0)
            {
                string k = "MaterialRegenAt_" + matMeta.Value.mat_id;
                if (PlayerPrefs.HasKey(k))
                {
                    string tsString = PlayerPrefs.GetString(k, "0");
                    long ts = Convert.ToInt64(tsString);
                    if (Global.LeftTime(ts) > 0 && Global.LeftTime(matMeta.Value.expireTs) > 0)
                    {
                        var btn = InstantiateUIObject("UIPrefab/UIButtonLimitedMaterialForecast", scrollbarRight.grid.gameObject).GetComponent<UIButtonLimitedMaterialForecast>();
                        btn.SetData(matMeta.Value.mat_id, ts);
                        scrollbarRight.AddTempIcon(btn.gameObject);
                    }
                }
            }
        }
    }

    public IEnumerator CoCheckStageDataBeforeOpenPopUpCoinStageReady()
    {
        Global.isSingleRoundEvent = false;
        
        if (ManagerCoinBonusStage.instance.GetCurrentQuestState().state != 2)
        {
            string stageName = Global.GameInstance.GetStageFilename();
            yield return CoCheckStageData(stageName, true);
            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && this.isLoadDefaultMapFile == true)
            {
                stageName = Global.GameInstance.GetDefaultMapName();
            }

            using( var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName) )
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();

                if (!www.IsError() && www.downloadHandler != null)
                {
                    StringReader reader = new StringReader(www.downloadHandler.text);
                    var serializer = new XmlSerializer(typeof(StageMapData));
                    StageMapData tempData = serializer.Deserialize(reader) as StageMapData;
                    OpenPopupReadyCoinStage(tempData.moveCount);
                    NetworkLoading.EndNetworkLoading();
                }
                else
                {
                    IsOpenReady = false;
                    NetworkLoading.EndNetworkLoading();
                    ErrorController.ShowNetworkErrorDialogAndRetry("", () => IsOpenReady = false);
                }
            }
        }
        else
        {
            OpenPopupReadyCoinStage();
        }
        yield return null;
    }

    public IEnumerator CoLoadStageData_TurnRelayEvent(System.Action completeAction = null)
    {
        Global.isSingleRoundEvent = false;
        string stageName = Global.GameInstance.GetStageFilename();
        
        yield return CoCheckStageData(stageName, true);
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && this.isLoadDefaultMapFile == true)
        {
            Debug.Log("디폴트 맵 로드해옴");
            stageName = Global.GameInstance.GetDefaultMapName();
        }

        using (var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;
                NetworkLoading.EndNetworkLoading();

                //레디 창 목표 아틀라스 재 조합.
                yield return ManagerUI._instance.CoMakeTargetAtlas(tempData.collectCount, tempData.collectColorCount, tempData.listTargetInfo);

                //로드 성공했을 때, 콜백 호출
                completeAction?.Invoke();
            }
            else
            {
                IsOpenReady = false;
                NetworkLoading.EndNetworkLoading();
                ErrorController.ShowNetworkErrorDialogAndRetry("", () =>
                {
                    IsOpenReady = false;
                    Global.ReBoot();
                });
            }
        }
    }

    private bool isLoadDefaultMapFile = false;
    private IEnumerator CoCheckStageDataBeforeOpenPopUpReady(Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callbackCancel = null, bool isUsePrevTargetData = false)
    {
        string stageName = Global.GameInstance.GetStageFilename();
        yield return CoCheckStageData(stageName, true);
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && this.isLoadDefaultMapFile == true)
        {
            stageName = Global.GameInstance.GetDefaultMapName();
        }

        using (var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");

            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

                int overrideBlockIndex = 0;
                bool bLoadCharacter = false;
                // 이벤트 ^테이지이지만 받은 에셋이 없을때
                if (Global.GameType == GameType.EVENT)
                {
                    CdnEventChapter eventChapterData = ServerContents.EventChapters;
                    string assetName = eventChapterData.assetName;

                    //번들 로드
                    if (!ManagerLobby._assetBankEvent.ContainsKey(assetName))
                        yield return AreaEvent.LoadEventStageBundle(eventChapterData);

                    //현재 레디창에서 사용하는 라이브 2D 캐릭터가 로드되어 있지 않을 경우, 번들에서 해당 캐릭터 받아오기
                    if (eventChapterData.active == 0)
                    {
                        //읽어올 레디창 번들 파일명 설정.
                        string readyBundleName = assetName + "_ready";
                        bLoadCharacter = IsLoadLive2DCharacter(eventChapterData, readyBundleName);
                    }
                    overrideBlockIndex = eventChapterData.blockIndex;
                }

                
                //이벤트 스테이지에서 레디창의 라이브 2D를 다른 캐릭터로 사용 할 대 추가 
                bLoadCharacter = Global.GameInstance.IsLoadLive2DCharacters();

                //목표 데이터를 이용해 타겟 아틀라스 설정
                if (isUsePrevTargetData == false)
                {
                    yield return CoMakeTargetAtlas(tempData.collectCount, tempData.collectColorCount, tempData.listTargetInfo);
                }
                else
                {
                    yield return CoMakeTargetAtlas(tempData.collectCount, tempData.collectColorCount,
                        GetListTargetInfoWithPrevTargetData(tempData.listTargetInfo));
                }

                if (bLoadCharacter == true)
                {
                    yield return ManagerCharacter._instance.LoadCharacters();
                }

                //인게임에서 사용할 커스텀 배경 아틀라스 다운로드.
                yield return Global.GameInstance.LoadIngameBGAtlas();

                var myProfile = SDKGameProfileManager._instance.GetMyProfile();
                if (ManagerSingleRoundEvent.instance != null && 
                    ManagerSingleRoundEvent.IsPlaying && 
                    Global.GameType == GameType.NORMAL && 
                    Global.stageIndex == myProfile.stage && 
                    ManagerSingleRoundEvent.CanPlaySingleRoundEvent() && 
                    tempData != null && tempData.isHardStage == 1)
                {
                    Global.isSingleRoundEvent = true;
                    OpenPopupSingleRoundEvent(tempData, callBackStart, callBackClose, callbackCancel);
                }
                else
                {
                    Global.isSingleRoundEvent = false;
                    StartCoroutine(Global.GameInstance.CoOnPopupOpen_Ready(tempData, callBackStart, callBackClose, callbackCancel));
                }
            }
            else
            {
                //Debug.Log("파일없음");
                IsOpenReady = false;
                NetworkLoading.EndNetworkLoading();
                ErrorController.ShowNetworkErrorDialogAndRetry("", () => IsOpenReady = false);
            }
        }
        yield return null;
    }

    #region 인게임 목표 데이터 가져오기
    private List<CollectTargetInfo> GetListTargetInfoWithPrevTargetData(List<CollectTargetInfo> listCurInfo)
    {
        if (ManagerBlock.instance.stageInfo == null || ManagerBlock.instance.stageInfo.listTargetInfo == null)
            return listCurInfo;

        List<CollectTargetInfo> listTargetInfo = new List<CollectTargetInfo>();
        //현재 맵에서 사용하는 목표 데이터 추가
        for (int i = 0; i < listCurInfo.Count; i++)
        {
            listTargetInfo.Add(listCurInfo[i]);
        }

        //이전 맵에서 사용하는 목표 데이터 추가(현재 데이터에 없는 목표만 추가)
        List<CollectTargetInfo> listPrevInfo = new List<CollectTargetInfo>(ManagerBlock.instance.stageInfo.listTargetInfo);
        for (int i = 0; i < listPrevInfo.Count; i++)
        {   
            if (listTargetInfo.FindIndex(x => x.targetType == (listPrevInfo[i].targetType)) == -1)
                listTargetInfo.Add(listPrevInfo[i]);
        }
        return listTargetInfo;
    }
    #endregion

    public IEnumerator CoCheckStageData(string stageName, bool isUseDefaultMap = false)
    {
        this.isLoadDefaultMapFile = false;
        bool loadStage = false;
        {
            string lastname = PlayerPrefs.GetString("LastStageName", "");
            if (lastname.Length > 0)
            {
                if (File.Exists(lastname))
                    File.Delete(lastname);
            }
            string stagePath = Global.StageDirectory + stageName;
            PlayerPrefs.SetString("LastStageName", stagePath);
            PlayerPrefs.Save();
        }

        Debug.Log("stageName : " + stageName);
        FileInfo downloadedFile = new FileInfo(Global.StageDirectory + stageName);
        if (downloadedFile.Exists == false || HashChecker.IsHashChanged("stage", stageName, downloadedFile))
        {
            loadStage = true;
        }

        if (loadStage)
        {
            string stageURLBase = Global._cdnAddress;
            using( var www = UnityWebRequest.Get(stageURLBase + "stage/" + stageName))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");

                yield return www.SendWebRequest();

                if (www.IsError())
                {
                    if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && isUseDefaultMap == true)
                    {
                        stageName = Global.GameInstance.GetDefaultMapName();
                        yield return CoCheckStageData(stageName);
                        isLoadDefaultMapFile = true;
                    }
                    yield return null;
                }
                else if(www.downloadHandler != null)
                {
                    try
                    {
                        int mapSize = www.downloadHandler.data.Length;
                        if (mapSize > 256)
                        {
                            MemoryStream memoryStream = new MemoryStream(www.downloadHandler.data);
                            memoryStream.Position = 0;
                            byte[] bytes = memoryStream.ToArray();
                            File.WriteAllBytes(Global.StageDirectory + stageName, bytes);
                        }
                        else
                        {
                            throw new System.Exception();
                        }
                    }
                    catch
                    {
                        string stagePath = Global.StageDirectory + stageName;
                        File.Delete(stagePath);
                    }
                }

            }
            
        }
    }

    //현재 레디창에서 사용하는 라이브 2D 캐릭터가 로드되어 있지 않을 경우, 번들에서 해당 캐릭터 받아오기
    private bool IsLoadLive2DCharacter(CdnEventChapter eventChapterData, string fileName)
    {
        List<TypeCharacterType> listLive2DCharacter = new List<TypeCharacterType>();
        for (int x = 0; x < eventChapterData.counts.Count; x++)
        {
            GameObject objReady = null;
            ManagerLobby._assetBankEvent.TryGetValue(fileName + (x + 1), out objReady);

            if (objReady != null)
            {
                ReadyEvent readyEvent = objReady.GetComponent<ReadyEvent>();
                if (readyEvent != null)
                {
                    if (ManagerCharacter._instance._live2dObjects.ContainsKey((int)(readyEvent.live2dCharacter)) == false)
                    {
                        listLive2DCharacter.Add(readyEvent.live2dCharacter);
                    }
                }
            }
        }
        if (listLive2DCharacter.Count <= 0)
            return false;

        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);
        return true;
    }

    public void OpenPopupReady(StageMapData stageData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null)
    {
        UIPopupReady popupReady = InstantiateUIObject("UIPrefab/UIPopupReady", anchorCenter).GetComponent<UIPopupReady>();;
        
        if (callBackStart != null)
        {
            popupReady._callbackOpen += callBackStart;
        }
        if (callBackClose != null)
        {
            popupReady._callbackClose += callBackClose;
        }
        if (callBackCancel != null)
        {
            popupReady.readyCancelAction += callBackCancel;
        }
        popupReady.OpenPopUp(GetPanelDepth(popupReady.bShowTopUI, popupReady.panelCount));

        // 이벤트 스테이지 경우
        if (Global.GameType == GameType.EVENT)
        {
            CdnEventChapter eventUser = ServerContents.EventChapters;

            int groupState = (eventUser.type == (int)EVENT_CHAPTER_TYPE.SCORE) ? 1 : ServerRepos.EventChapters.groupState;

            if (UIPopupReady._instance != null && eventUser.type != (int)EVENT_CHAPTER_TYPE.SCORE)
            {
                if (UIPopupReady.eventGroupClear == true )
                {
                    groupState -= 1;
                }
            }
            
            if (groupState > eventUser.counts.Count)
                groupState = eventUser.counts.Count;

            // 이벤트 스테이지에 필요한 에셋 읽어서 넣어주기
            string fileName = "_ready" + groupState;
            string assetName = eventUser.assetName + fileName;
            popupReady.EventStageSetting(ManagerLobby._assetBankEvent[assetName]);
        }

        popupReady.SettingSortOrder(GetSortLayer(popupReady.sortOrderCount));

        //마지막으로 연 스테이지.
        popupReady.InitPopUp(stageData);
        _popupList.Add(popupReady);

        NetworkLoading.EndNetworkLoading();
        IsOpenReady = false;
    }
    
    public void OpenPopupReady_EndContents(StageMapData stageData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null)
    {
        UIPopupReady_EndContents popupReady = InstantiateUIObject("UIPrefab/UIPopupReady_EndContents", anchorCenter).GetComponent<UIPopupReady_EndContents>();
        
        if (callBackStart != null)
        {
            popupReady._callbackOpen += callBackStart;
        }
        if (callBackClose != null)
        {
            popupReady._callbackClose += callBackClose;
        }
        if (callBackCancel != null)
        {
            popupReady.readyCancelAction += callBackCancel;
        }
        popupReady.OpenPopUp(GetPanelDepth(popupReady.bShowTopUI, popupReady.panelCount));
        popupReady.SettingSortOrder(GetSortLayer(popupReady.sortOrderCount));

        //마지막으로 연 스테이지.
        popupReady.InitPopUp(stageData);
        _popupList.Add(popupReady);

        NetworkLoading.EndNetworkLoading();
        IsOpenReady = false;
    }

    public void OpenPopupReady_BingoEvent(StageMapData stageData, Method.FunctionVoid callBackStart = null,
        Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null)
    {
        UIPopUpReady_BingoEvent popupReady = InstantiateUIObject("UIPrefab/UIPopUpReady_BingoEvent", anchorCenter)
            .GetComponent<UIPopUpReady_BingoEvent>();
        
        if (callBackStart != null)
        {
            popupReady._callbackOpen += callBackStart;
        }
        if (callBackClose != null)
        {
            popupReady._callbackClose += callBackClose;
        }
        if (callBackCancel != null)
        {
            popupReady.readyCancelAction += callBackCancel;
        }
        popupReady.OpenPopUp(GetPanelDepth(popupReady.bShowTopUI, popupReady.panelCount));
        popupReady.SettingSortOrder(GetSortLayer(popupReady.sortOrderCount));

        //마지막으로 연 스테이지.
        popupReady.InitPopUp(stageData);
        _popupList.Add(popupReady);

        NetworkLoading.EndNetworkLoading();
        IsOpenReady = false;
    }
    
    public T OpenPopupReady_Event <T> (StageMapData stageData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callBackCancel = null) where T : UIPopupReadyBase
    {
        T popupReady = InstantiateUIObject("UIPrefab/" + typeof(T).Name, anchorCenter).GetComponent<T>();
        
        if (callBackStart != null)
            popupReady._callbackOpen += callBackStart;
        if (callBackClose != null)
            popupReady._callbackClose += callBackClose;
        if (callBackCancel != null)
            popupReady.readyCancelAction += callBackCancel;
        popupReady.OpenPopUp(GetPanelDepth(popupReady.bShowTopUI, popupReady.panelCount));
        popupReady.SettingSortOrder(GetSortLayer(popupReady.sortOrderCount));

        //마지막으로 연 스테이지.
        popupReady.InitPopUp(stageData);
        _popupList.Add(popupReady);

        NetworkLoading.EndNetworkLoading();
        IsOpenReady = false;

        return popupReady;
    }

    public void OpenPopupTimeOver()
    {
        UIPopupTimeOver popupTimeOver = InstantiateUIObject("UIPrefab/UIPopupTimeOver", anchorCenter).GetComponent<UIPopupTimeOver>();
        popupTimeOver.OpenPopUp(GetPanelDepth(popupTimeOver.bShowTopUI, popupTimeOver.panelCount));
        popupTimeOver.SettingSortOrder(GetSortLayer(popupTimeOver.sortOrderCount));
        popupTimeOver.InitPopup();
        _popupList.Add(popupTimeOver);
    }

    public void OpenPopupClear(int score)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupClear popupClear = InstantiateUIObject("UIPrefab/UIPopupClear", anchorCenter).GetComponent<UIPopupClear>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopUp(score);
        _popupList.Add(popupClear);

        ManagerUI._instance.FocusCheck();
    }

    public void OpenPopupClear_CoinStage()
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupClear_CoinStage popupClear = InstantiateUIObject("UIPrefab/UIPopupClear_CoinStage", anchorCenter).GetComponent<UIPopupClear_CoinStage>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopup();
        _popupList.Add(popupClear);

        ManagerUI._instance.FocusCheck();
    }

    public void OpenPopupClear_WorldRanking()
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupClear_WorldRanking popupClear = InstantiateUIObject("UIPrefab/UIPopupClear_WorldRanking", anchorCenter).GetComponent<UIPopupClear_WorldRanking>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopup();
        _popupList.Add(popupClear);

        ManagerUI._instance.FocusCheck();
    }

    public void OpenPopupFail(GameFailResp resp = null)
    {
        UIPopupFail popupFail = InstantiateUIObject("UIPrefab/UIPopupFail", anchorCenter).GetComponent<UIPopupFail>();
        popupFail.OpenPopUp(GetPanelDepth(popupFail.bShowTopUI, popupFail.panelCount));
        popupFail.SettingSortOrder(GetSortLayer(popupFail.sortOrderCount));
        popupFail.InitPopUp(resp);
        _popupList.Add(popupFail);
    }

    public void OpenPopupContinue()
    {
        UIPopupContinue popupContinue = InstantiateUIObject("UIPrefab/UIPopupContinue", gameObject).GetComponent<UIPopupContinue>();
        popupContinue.OpenPopUp(GetPanelDepth(popupContinue.bShowTopUI, popupContinue.panelCount));
        popupContinue.SettingSortOrder(GetSortLayer(popupContinue.sortOrderCount));
        popupContinue.InitPopUp();
        _popupList.Add(popupContinue);
    }

    public UIPopupPause OpenPopupPause()
    {
        UIPopupPause popupPause = InstantiateUIObject("UIPrefab/UIPopupPause", gameObject).GetComponent<UIPopupPause>();
        popupPause.OpenPopUp(GetPanelDepth(popupPause.bShowTopUI, popupPause.panelCount));
        popupPause.SettingSortOrder(GetSortLayer(popupPause.sortOrderCount));
        popupPause._callbackEnd += () =>
        {
            if (GameUIManager.instance != null && GameUIManager.instance.listPopupPause.Count > 0)
                GameUIManager.instance.listPopupPause.RemoveAt(0);
        };
        _popupList.Add(popupPause);
        return popupPause;
    }

    public void OpenPopupNoMoreMove()
    {
        UIPopupNoMoreMove popupNoMoreMove = NGUITools.AddChild(gameObject, _objPopupNoMoreMove).GetComponent<UIPopupNoMoreMove>();
        popupNoMoreMove.OpenPopUp(GetPanelDepth(popupNoMoreMove.bShowTopUI, popupNoMoreMove.panelCount));
        popupNoMoreMove.SettingSortOrder(GetSortLayer(popupNoMoreMove.sortOrderCount));
        _popupList.Add(popupNoMoreMove);
    }

    public void OpenPopupStageTarget(string targetText = "")
    {
        UIPopupStageTarget popupStageTarget = InstantiateUIObject("UIPrefab/UIPopupStageTarget", gameObject).GetComponent<UIPopupStageTarget>();
        popupStageTarget.OpenPopUp(GetPanelDepth(popupStageTarget.bShowTopUI, popupStageTarget.panelCount));
        popupStageTarget.InitPopup(targetText);
        popupStageTarget.SettingSortOrder(GetSortLayer(popupStageTarget.sortOrderCount));
        _popupList.Add(popupStageTarget);
    }

    public void OpenPopupStageReview(string reviewArticle, Method.FunctionVoid reviewChatFunc, bool bWrite)
    {
        UIPopupStageReview popupStageReview = InstantiateUIObject("UIPrefab/UIPopupStageReview", gameObject).GetComponent<UIPopupStageReview>();
        popupStageReview.OpenPopUp(GetPanelDepth(popupStageReview.bShowTopUI, popupStageReview.panelCount));
        popupStageReview.SettingSortOrder(GetSortLayer(popupStageReview.sortOrderCount));
        popupStageReview.SettingStageArticle(reviewArticle, reviewChatFunc);
        if(bWrite == false)
        {
            popupStageReview.SettingDoNotUseWrite();
        }
        _popupList.Add(popupStageReview);
    }

    public UIPopupConfirm OpenPopupConfirm (string _description, System.Action _callBack = null)
    {
        UIPopupConfirm popupConfirm = NGUITools.AddChild( gameObject, _objPopupConfirm ).GetComponent<UIPopupConfirm>();
        popupConfirm.OpenPopUp( GetPanelDepth( popupConfirm.bShowTopUI, popupConfirm.panelCount ) );
        popupConfirm.SettingSortOrder(GetSortLayer(popupConfirm.sortOrderCount));
        popupConfirm.InitPopUp( _description, _callBack );
        _popupList.Add( popupConfirm );

        return popupConfirm;
    }

    public UIPopupServerSelect OpenPopupServerSelect()
    {
        var popupServerSelect = InstantiateUIObject("UIPrefab/UIPopupServerSelect", gameObject).GetComponent<UIPopupServerSelect>();
        popupServerSelect.OpenPopUp(GetPanelDepth(popupServerSelect.bShowTopUI, popupServerSelect.panelCount));
        popupServerSelect.SettingSortOrder(GetSortLayer(popupServerSelect.sortOrderCount));
        _popupList.Add(popupServerSelect);
        return popupServerSelect;
    }

    public void OpenPopupGetRewardAlarm(string textMsg, Method.FunctionVoid endCallback, AppliedRewardSet rewardSet, float delay = 0f)
    {
        var popup = OpenPopup<UIPopupGetRewardAlarm>((p) => { p.InitPopup(textMsg, rewardSet, delay); }, endCallback);
    }

    public void OpenPopupStageAdventure()
    {
        if (!ManagerAdventure.CheckStartable())
        {
            return;
        }

        if (UIPopupStageAdventure._instance != null)
            return;

        ManagerAdventure.OnInit(OpenPopupStageAdventure_Internal);
    }

    private void OpenPopupStageAdventure_Internal(bool firstLoad)
    {
        var popupStageAdventure = InstantiateUIObject("UIPrefab/UIPopupStageAdventure", anchorCenter).GetComponent<UIPopupStageAdventure>();
        popupStageAdventure.firstLoad = firstLoad;
        popupStageAdventure.OpenPopUp(GetPanelDepth(popupStageAdventure.bShowTopUI, popupStageAdventure.panelCount));
        popupStageAdventure.SettingSortOrder(GetSortLayer(popupStageAdventure.sortOrderCount));
        _popupList.Add(popupStageAdventure);
    }

    public UIPopupStageAdventureReady OpenPopupStageAdventureReady(StageMapData stageData, Method.FunctionVoid callBack = null)
    {
        var popup = OpenPopup<UIPopupStageAdventureReady>((p) => { p.InitData(stageData); });
        popup._callbackOpen = callBack;

        return popup;
    }

    public UIPopupStageAdventureAnimalInfo OpenPopupStageAdventureAnimalInfo(ManagerAdventure.AnimalInstance aData)
    {
        var popupStageAdventureAnimalInfo = InstantiateUIObject("UIPrefab/UIPopupStageAdventureAnimalInfo", anchorCenter).GetComponent<UIPopupStageAdventureAnimalInfo>();
        popupStageAdventureAnimalInfo.InitAnimalInfo(aData);
        popupStageAdventureAnimalInfo.OpenPopUp(GetPanelDepth(popupStageAdventureAnimalInfo.bShowTopUI, popupStageAdventureAnimalInfo.panelCount));
        popupStageAdventureAnimalInfo.SettingSortOrder(GetSortLayer(popupStageAdventureAnimalInfo.sortOrderCount));
        _popupList.Add(popupStageAdventureAnimalInfo);

        return popupStageAdventureAnimalInfo;
    }

    public UIPopupStageAdventureAnimalMaxInfo OpenPopupStageAdventureAnimalMaxInfo(ManagerAdventure.AnimalInstance aData)
    {
        var popupStageAdventureAnimalMaxInfo = InstantiateUIObject("UIPrefab/UIPopupStageAdventureAnimalMaxInfo", anchorCenter).GetComponent<UIPopupStageAdventureAnimalMaxInfo>();
        popupStageAdventureAnimalMaxInfo.InitAnimalMaxInfo(aData);
        popupStageAdventureAnimalMaxInfo.OpenPopUp(GetPanelDepth(popupStageAdventureAnimalMaxInfo.bShowTopUI, popupStageAdventureAnimalMaxInfo.panelCount));
        popupStageAdventureAnimalMaxInfo.SettingSortOrder(GetSortLayer(popupStageAdventureAnimalMaxInfo.sortOrderCount));
        _popupList.Add(popupStageAdventureAnimalMaxInfo);

        return popupStageAdventureAnimalMaxInfo;
    }

    public UIPopUpAdventureAnimalChange OpenPopupAdventureAnimalChange(ManagerAdventure.AnimalInstance targetData, ManagerAdventure.AnimalInstance changeData)
    {
        var popupAdventureAnimalChange = InstantiateUIObject("UIPrefab/UIPopUpAdventureAnimalChange", anchorCenter).GetComponent<UIPopUpAdventureAnimalChange>();
        popupAdventureAnimalChange.InitPopup(targetData, changeData);
        popupAdventureAnimalChange.OpenPopUp(GetPanelDepth(popupAdventureAnimalChange.bShowTopUI, popupAdventureAnimalChange.panelCount));
        popupAdventureAnimalChange.SettingSortOrder(GetSortLayer(popupAdventureAnimalChange.sortOrderCount));
        _popupList.Add(popupAdventureAnimalChange);

        return popupAdventureAnimalChange;
    }

    public void Summon(CdnAdventureGachaProduct gachaData, Action<int> failCallback)
    {
        ManagerAdventure.User.Summon(gachaData,
            (ManagerAdventure.AnimalInstance orgAnimal, ManagerAdventure.AnimalInstance summoned, List<Reward> bonusRewards) =>
            {
                UpdateUI();
                //소환연출 호출.
                OpenPopupStageAdventureSummonAction(orgAnimal, summoned, UIPopupAdventureSummonAction.SummonType.NORMAL, bonusRewards, gachaData);
            }, failCallback);
    }

    public UIPopupAdventureSummonAction OpenPopupStageAdventureSummonAction(ManagerAdventure.AnimalInstance orgAnimal, ManagerAdventure.AnimalInstance summoned, UIPopupAdventureSummonAction.SummonType summonType, List<Reward> bonusRewards, CdnAdventureGachaProduct gachaData, Method.FunctionVoid callBack = null)
    {
        var popupAdventureSummonAction = InstantiateUIObject("UIPrefab/UIPopupAdventureSummonAction", anchorCenter).GetComponent<UIPopupAdventureSummonAction>();
        popupAdventureSummonAction.OpenPopUp(GetPanelDepth(popupAdventureSummonAction.bShowTopUI, popupAdventureSummonAction.panelCount));
        popupAdventureSummonAction.SettingSortOrder(GetSortLayer(popupAdventureSummonAction.sortOrderCount));
        popupAdventureSummonAction.InitPopup(orgAnimal, summoned, summonType, bonusRewards, gachaData, callBack);
        _popupList.Add(popupAdventureSummonAction);

        return popupAdventureSummonAction;
    }
    
    public UIPopupAdventureClear OpenPopupAdventureClear()
    {
        var popupAdventureClear = InstantiateUIObject("UIPrefab/UIPopupAdventureClear", anchorCenter).GetComponent<UIPopupAdventureClear>();
        popupAdventureClear.OpenPopUp(GetPanelDepth(popupAdventureClear.bShowTopUI, popupAdventureClear.panelCount));
        popupAdventureClear.SettingSortOrder(GetSortLayer(popupAdventureClear.sortOrderCount));
        _popupList.Add(popupAdventureClear);

        return popupAdventureClear;
    }

    public UIPopupAdventureFail OpenPopupAdventureFail()
    {
        var popupAdventureFail = InstantiateUIObject("UIPrefab/UIPopupAdventureFail", anchorCenter).GetComponent<UIPopupAdventureFail>();
        popupAdventureFail.OpenPopUp(GetPanelDepth(popupAdventureFail.bShowTopUI, popupAdventureFail.panelCount));
        popupAdventureFail.SettingSortOrder(GetSortLayer(popupAdventureFail.sortOrderCount));
        _popupList.Add(popupAdventureFail);

        return popupAdventureFail;
    }

    public UIPopupAdventurePause OpenPopupAdventurePause(string stageData, Reward[] rewardsData)
    {
        var popupAdventurePause = InstantiateUIObject("UIPrefab/UIPopupAdventurePause", anchorCenter).GetComponent<UIPopupAdventurePause>();
        popupAdventurePause.OpenPopUp(GetPanelDepth(popupAdventurePause.bShowTopUI, popupAdventurePause.panelCount));
        popupAdventurePause.InitPopup(stageData, rewardsData);
        popupAdventurePause.SettingSortOrder(GetSortLayer(popupAdventurePause.sortOrderCount));
        popupAdventurePause._callbackEnd += () => 
        {
            if (GameUIManager.instance != null && GameUIManager.instance.listPopupPause.Count > 0)
                GameUIManager.instance.listPopupPause.RemoveAt(0);
        };
        _popupList.Add(popupAdventurePause);

        return popupAdventurePause;
    }

    public UIPopupAdventureContinue OpenPopupAdventureContinue()
    {
        var popupAdventureContinue = InstantiateUIObject("UIPrefab/UIPopupAdventureContinue", anchorCenter).GetComponent<UIPopupAdventureContinue>();
        popupAdventureContinue.OpenPopUp(GetPanelDepth(popupAdventureContinue.bShowTopUI, popupAdventureContinue.panelCount));
        popupAdventureContinue.SettingSortOrder(GetSortLayer(popupAdventureContinue.sortOrderCount));
        _popupList.Add(popupAdventureContinue);

        return popupAdventureContinue;
    }

    public void OpenPopupClearAdventureChapter(int chapterIdx)
    {
        UIPopupClearAdventureChapter popupChapClear = InstantiateUIObject("UIPrefab/UIPopupClearAdventureChapter", anchorCenter).GetComponent<UIPopupClearAdventureChapter>();
        popupChapClear.OpenPopUp(GetPanelDepth(popupChapClear.bShowTopUI, popupChapClear.panelCount));
        popupChapClear.SettingSortOrder(GetSortLayer(popupChapClear.sortOrderCount));
        popupChapClear.Init(chapterIdx);
        _popupList.Add(popupChapClear);
    }


    public void OpenPopupInputText ( System.Action<string> _callback )
    {
        UIPopupInputText popupInputText = NGUITools.AddChild( gameObject, _objPopupInputText ).GetComponent<UIPopupInputText>();
        popupInputText.OpenPopUp( GetPanelDepth( popupInputText.bShowTopUI, popupInputText.panelCount ) );
        popupInputText.SettingSortOrder(GetSortLayer(popupInputText.sortOrderCount));
        popupInputText.InitPopUp( _callback );
        _popupList.Add( popupInputText );
    }

    public void OpenPopopSendItemToSocial (UIItemStamp _sendItem, Stamp originData, System.Action<UIItemStamp> _callbackBtnHandler, System.Action<Stamp> _callbackResetHandler, int stampIndex = 0)
    {
        UIPopupSendItemToSocial popupSendItemToSocial = NGUITools.AddChild( gameObject, _objPopupSendItemToSocial ).GetComponent<UIPopupSendItemToSocial>();
        popupSendItemToSocial.OpenPopUp( GetPanelDepth( popupSendItemToSocial.bShowTopUI, popupSendItemToSocial.panelCount ) );
        popupSendItemToSocial.SettingSortOrder(GetSortLayer(popupSendItemToSocial.sortOrderCount));
        popupSendItemToSocial.InitPopup( _sendItem, originData, _callbackBtnHandler, _callbackResetHandler, false, stampIndex);
        _popupList.Add( popupSendItemToSocial );
    }

    public void OpenPopupPanelEmotionIcon ( System.Action callbackHandler, GameObject parent )
    {
        UIPopupPanelEmotionIcon popupEmotionIcon = NGUITools.AddChild(parent, this._objPopupPanelEmotionIcon).GetComponent<UIPopupPanelEmotionIcon>();
        popupEmotionIcon.OpenPopUp(GetPanelDepth(popupEmotionIcon.bShowTopUI, popupEmotionIcon.panelCount));
        popupEmotionIcon.SettingSortOrder(GetSortLayer(popupEmotionIcon.sortOrderCount));
        popupEmotionIcon.InitPopup(callbackHandler);
        popupEmotionIcon.transform.localPosition = new Vector3(-18f, 0f, 0f);
        _popupList.Add(popupEmotionIcon);
    }

    public PopupInputColorPad OpenPopupInputColorPad (System.Action<Color> eventDelegate, System.Action<List<UIItemColor>> eventDelegateHandler )
    {
        PopupInputColorPad popupInputColorPad = NGUITools.AddChild(gameObject, _objPopupInputColorPad).GetComponent<PopupInputColorPad>();
        popupInputColorPad.OpenPopUp(GetPanelDepth(popupInputColorPad.bShowTopUI, popupInputColorPad.panelCount));
        popupInputColorPad.SettingSortOrder(GetSortLayer(popupInputColorPad.sortOrderCount));
        popupInputColorPad.InitData(eventDelegate, eventDelegateHandler);
        _popupList.Add(popupInputColorPad);

        return popupInputColorPad;
    }

    public void OpenPopupLobbyGuestChange(int currentIndex, int selectIndex, System.Action<int, int> changeAction)
    {
        UIPopupLobbyGuestChange popupLobbyGuestChange = InstantiateUIObject("UIPrefab/UIPopUpLobbyGuestChange", anchorCenter).GetComponent<UIPopupLobbyGuestChange>();
        popupLobbyGuestChange.OpenPopUp(GetPanelDepth(popupLobbyGuestChange.bShowTopUI, popupLobbyGuestChange.panelCount));
        popupLobbyGuestChange.SettingSortOrder(GetSortLayer(popupLobbyGuestChange.sortOrderCount));
        popupLobbyGuestChange.InitPopup(currentIndex, selectIndex, changeAction);
        _popupList.Add(popupLobbyGuestChange);
    }

    public UIPopupShowAPNG OpenPopupShowAPNG(string filePath, float? textureSizeOffset = null, Vector2? mainSpritePos = null, Vector2Int? popupSize = null)
    {
        UIPopupShowAPNG popupLobbyGuestChange = InstantiateUIObject("UIPrefab/UIPopUpShowAPNG", anchorCenter).GetComponent<UIPopupShowAPNG>();
        popupLobbyGuestChange.OpenPopUp(GetPanelDepth(popupLobbyGuestChange.bShowTopUI, popupLobbyGuestChange.panelCount));
        popupLobbyGuestChange.SettingSortOrder(GetSortLayer(popupLobbyGuestChange.sortOrderCount));
        popupLobbyGuestChange.InitPopup(filePath, textureSizeOffset, mainSpritePos, popupSize);
        _popupList.Add(popupLobbyGuestChange);

        return popupLobbyGuestChange;
    }

    public UIPopupGimmickTutorial OpenPopupGimmickTutorial()
    {
        UIPopupGimmickTutorial popupGimmickTutorial = InstantiateUIObject("UIPrefab/UIPopupGimmickTutorial", anchorCenter).GetComponent<UIPopupGimmickTutorial>();
        popupGimmickTutorial.OpenPopUp(GetPanelDepth(popupGimmickTutorial.bShowTopUI, popupGimmickTutorial.panelCount));
        popupGimmickTutorial.SettingSortOrder(GetSortLayer(popupGimmickTutorial.sortOrderCount));
        popupGimmickTutorial.InitPopup();
        _popupList.Add(popupGimmickTutorial);

        return popupGimmickTutorial;
    }

    public int isOpeningEventPopupCount = 0;
    
    public void OpenPopupMoleCatch()
    {
        isOpeningEventPopupCount++;
        StartCoroutine(CoOpenMoleCatch());
    }

    IEnumerator CoOpenMoleCatch()
    {
        int resIndex = ManagerMoleCatch.GetResourceIndex();
        string prefabName = string.Format("Mole_{0}_MoleCatch", resIndex);
        string bundleName = string.Format("mole_v2_{0}", resIndex);

        GameObject bundleObj = null;
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path = string.Format("Assets/5_OutResource/MoleCatch/Mole_v2_{0}/{1}.prefab", resIndex, prefabName);
            bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
        }
        else
        {
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    bundleObj = assetBundle.LoadAsset<GameObject>(prefabName);
                }
            }
        }
        var moleCatchObj = bundleObj.GetComponent<MoleCatch>();

        if (moleCatchObj != null)
        {
            ManagerMoleCatch.instance.readyResource = new ManagerMoleCatch.ReadyResource()
            {
                readyEmoticonPosition_1 = moleCatchObj.readyEmoticonPosition_1,
                readyEmoticonPosition_2 = moleCatchObj.readyEmoticonPosition_2,
                readyEmoticonTexture = moleCatchObj.readyEmoticonTexture,
                readyMole_hard_1 = moleCatchObj.readyMole_hard_1,
                readyMole_hard_2 = moleCatchObj.readyMole_hard_2,
                readyMole_normal_1 = moleCatchObj.readyMole_normal_1,
                readyMole_normal_2 = moleCatchObj.readyMole_normal_2
            };
        }

        OpenPopupMoleCatch_Internal(bundleObj);
    }

    void OpenPopupMoleCatch_Internal(GameObject bundleObj, AppliedRewardSet rewardSet = null)
    {
        int resIndex = ManagerMoleCatch.GetResourceIndex();
        string assetName = string.Format("Mole_{0}_MoleCatch", resIndex);

        if (bundleObj == null)
        {
            isOpeningEventPopupCount--;
            return;
        }

        UIPopupMoleCatch popupMoleCatch = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIPopUpMoleCatch", gameObject).GetComponent<UIPopupMoleCatch>();
        popupMoleCatch.OpenPopUp(GetPanelDepth(popupMoleCatch.bShowTopUI, popupMoleCatch.panelCount));
        popupMoleCatch.InitPopup(bundleObj);
        popupMoleCatch.SettingSortOrder(GetSortLayer(popupMoleCatch.sortOrderCount));
        //popupMoleCatch.InitPopup();
        _popupList.Add(popupMoleCatch);
        
        isOpeningEventPopupCount--;
    }
    
    public async UniTask AsyncOpenPopupNoyBoostEvent()
    {
        if (ManagerNoyBoostEvent.instance.NoyBoostPackUI == null)
            await ManagerNoyBoostEvent.instance.AsyncLoadNoyBoostResource(ManagerNoyBoostEvent.PrefabType.UI);
        var popup = _instance.OpenPopup<UIPopUpNoyBoostEvent>();
        popup.InitPopup();
    }
    
    public IEnumerator CoOpenPopupSpaceTravelEvent()
    {
        if (ManagerSpaceTravel.instance._spaceTravelPackUI == null)
        {
            ManagerSpaceTravel.instance.AsyncLoadSpaceTravelResource(ManagerSpaceTravel.PrefabType.UI).Forget();
            yield return new WaitUntil(() => ManagerSpaceTravel.instance._spaceTravelPackUI != null);
        }

        ManagerSound._instance.StopBGM();
        Global.SetGameType_SpaceTravel(ManagerSpaceTravel.instance.EventIndex, ManagerSpaceTravel.instance.CurrentStage);
        var popup = _instance.OpenPopup<UIPopUpSpaceTravel>();
        popup.InitPopup();
    }
    
    public void OpenPopupSpaceTravelSelectItem(Method.FunctionVoid closeCallBack = null, System.Action<ManagerSpaceTravel.BonusItemType> selectItemAction = null)
    {
        var popup = InstantiateUIObject("UIPrefab/UIPopupSpaceTravelIngameItemSelect", anchorCenter).GetComponent<UIPopupSpaceTravelIngameItemSelect>();
        popup.OpenPopUp(GetPanelDepth(popup.bShowTopUI, popup.panelCount));
        popup.SettingSortOrder(GetSortLayer(popup.sortOrderCount));
        popup.InitPopup(selectItemAction);
        if (closeCallBack != null)
        {
            popup._callbackClose += closeCallBack;
        }
        _popupList.Add(popup);
    }


    public void OpenPopupStageChallenge(AppliedRewardSet rewardSet = null)
    {
        UIPopupStageChallenge popupStageChallenge = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIPopupStageChallenge", gameObject).GetComponent<UIPopupStageChallenge>();
        popupStageChallenge.OpenPopUp(GetPanelDepth(popupStageChallenge.bShowTopUI, popupStageChallenge.panelCount));
        popupStageChallenge.SettingSortOrder(GetSortLayer(popupStageChallenge.sortOrderCount));
        popupStageChallenge.InitPopup(rewardSet);
        _popupList.Add(popupStageChallenge);
    }

    public void OpenPopupBingoEvent()
    {
        StartCoroutine(CoOpenPopupBingoEvent());
    }

    public IEnumerator CoOpenPopupBingoEvent()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
                
         yield return ManagerBingoEvent.bingoEventResource.CoLoadAssetBundle();
                
        NetworkLoading.EndNetworkLoading();
        
        ManagerUI._instance.OpenPopup<UIPopupBingoEvent_Board>((popup) =>
        {
            popup.InitData();
        });
    }

    public void OpenPopupCriminalEvent()
    {
        StartCoroutine(CoOpenPopupCriminalEvent());
    }
    
    private IEnumerator CoOpenPopupCriminalEvent()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        
        yield return StartCoroutine(ManagerCriminalEvent.instance.LoadCriminalEventResource());
        
        NetworkLoading.EndNetworkLoading();
        
        ManagerUI._instance.OpenPopup<UIPopUpCriminalEvent>((popup) =>
        {
            popup.InitPopup();
        });
    }
    
    public T OpenPopup<T>(System.Action<T> precall = null, Method.FunctionVoid endCallback = null)  where T : UIPopupBase
    {
        T popupInstance = InstantiateUIObject("UIPrefab/" + typeof(T).Name, anchorCenter).GetComponent<T>();
        if (precall != null)
            precall(popupInstance);
        popupInstance.OpenPopUp(GetPanelDepth(popupInstance.bShowTopUI, popupInstance.panelCount));
        popupInstance.SettingSortOrder(GetSortLayer(popupInstance.sortOrderCount));
        popupInstance._callbackEnd += endCallback;
        _popupList.Add(popupInstance);
        return popupInstance;
    }

    /// <summary>
    /// Lobby에서 팝업을 열 때 사용
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="precall"></param>
    /// <param name="endCallback"></param>
    /// <returns></returns>
    public T OpenPopup_LobbyPhase<T>(System.Action<T> precall = null, Method.FunctionVoid endCallback = null) where T : UIPopupBase
    {
        T popupInstance = InstantiateUIObject("UIPrefab/" + typeof(T).Name, anchorCenter).GetComponent<T>();
        if (precall != null)
            precall(popupInstance);
        popupInstance.OpenPopUp(GetPanelDepth(popupInstance.bShowTopUI, popupInstance.panelCount));
        popupInstance.SettingSortOrder(GetSortLayer(popupInstance.sortOrderCount));
        popupInstance._callbackEnd += endCallback;
        SetLobbyActionState(popupInstance);
        _popupList.Add(popupInstance);
        return popupInstance;
    }
    
    /// <summary>
    /// 상품판매 법류 개정으로 인하여 프리팹의 변경사항이 많을 시에 기존 프리팹 이름에 "_Description"를 추가하여 수정된 팝업을 OpenPopup 해주는 함수.
    /// </summary>
    /// <param name="precall"></param>
    /// <param name="endCallback"></param>
    /// <param name="IsLobbyPhase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T OpenPopup_Description<T>(System.Action<T> precall = null, Method.FunctionVoid endCallback = null, bool IsLobbyPhase = false)  where T : UIPopupBase
    {
        string Description = LanguageUtility.IsShowBuyInfo ? "_Description" : "";
        
        T popupInstance = InstantiateUIObject("UIPrefab/" + typeof(T).Name + Description, anchorCenter).GetComponent<T>();
        if (precall != null)
            precall(popupInstance);
        popupInstance.OpenPopUp(GetPanelDepth(popupInstance.bShowTopUI, popupInstance.panelCount));
        popupInstance.SettingSortOrder(GetSortLayer(popupInstance.sortOrderCount));
        popupInstance._callbackEnd += endCallback;
        
        if(IsLobbyPhase)
            SetLobbyActionState(popupInstance);
        
        _popupList.Add(popupInstance);
        return popupInstance;
    }

    /// <summary>
    /// 미성년자 보호 가이드라인 작업으로 결제 직전 나이 확인 팝업을 띄우는 함수.
    /// </summary>
    public void OpenMinorCheckPopup(double price, string currency, Action closeCallback)
    {
        if (LanguageUtility.IsShowBuyInfo && ServerRepos.LoginCdn.minorProtection != null && ServerRepos.LoginCdn.minorProtection.isEventOn)
        {
            ServerAPI.CheckMinorProtection((resp) =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.isEventOn)
                    {
                        if (resp.isShown)
                            OpenPopup<UIPopupMinorCheck>().InitPopup(price, currency, closeCallback);
                        else if (resp.ageType == 3)
                            closeCallback();
                        else
                            OpenPopup<UIPopupMinorApprove>().InitPopup(price, resp.ageType, currency, closeCallback);
                    }
                    else
                    {
                        closeCallback();
                    }
                }
            });
        }
        else
            closeCallback();
    }

    void SetLobbyActionState(UIPopupBase popup)
    {
        if (popup.bShowTopUI == false) return;

        ManagerLobby._instance.IsLoabbyActionState = true;

        popup._callbackEnd += () =>
        {
            //씬이 이동이 되면 IsLoabbyActionState의 값은 true를 유지. 
            if(SceneLoading.IsSceneLoading == false)
                ManagerLobby._instance.IsLoabbyActionState = false;
        };
    }

    public GameObject InstantiateUIObject(string path, GameObject parent)
    {
        GameObject prefab = Resources.Load(path) as GameObject;
        var go = Instantiate(prefab);
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    public bool CheckExitUI()
    {
        int nCount = _popupList.Count;
        if (nCount == 0)
            return false;
        if (_popupList[nCount - 1].GetPopupType() != PopupType.exit)
            return false;
        return true;
    }

    public void ClosePopUpUI(float _mainTime = 0.3f)
    {   
        int _nCount = _popupList.Count;
        if (_nCount == 0)
        {
            return;
        }
        _popupList[_nCount - 1].ClosePopUp(_mainTime, TopUIDepthAndSortLayer);
        _popupList.Remove(_popupList[_nCount - 1]);

        ManagerUI._instance.FocusCheck();
    }

    public void ClosePopUpUI(UIPopupBase closeTarget, float _mainTime = 0.3f )
    {
        int _nCount = _popupList.Count;
        if (_nCount == 0)
        {
            return;
        }
        if (_popupList.Exists(x => x == closeTarget) == false)
            return;

        closeTarget.ClosePopUp(_mainTime, TopUIDepthAndSortLayer);
        _popupList.RemoveAll(x => x == closeTarget);

        ManagerUI._instance.FocusCheck();
    }

    public void ImmediatelyCloseAllPopUp()
    {
        int _nCount = _popupList.Count;
        for (int i = 0; i < _nCount; i++)
        {
            if(_popupList[i].gameObject != null)
            { 
                Destroy(_popupList[i].gameObject);
            }
        }
        _popupList.Clear();
    }

    public void DestroyPopupList ( System.Action eventHandler )
    {
        for ( int i = 0; i < _popupList.Count; i++ )
        {
            GameObject popupObj = _popupList[i]?.gameObject;
            _popupList.RemoveAt(i);
            i--;

            if (popupObj != null) DestroyImmediate(popupObj);
        }

        this.StartCoroutine (this.DestroySystemPopup(eventHandler));
    }

    IEnumerator DestroySystemPopup ( System.Action eventHandler )
    {
        while( UIPopupSystem._instance != null )
        {
            yield return null;
        }
        eventHandler();
    }

    private int GetPanelDepth(bool bShowTopUI, int panelCount)
    {
        int count = _popupList.Count;
        int nextDepth = Default_Depth_High;

        if (count > 0)
        {
            //패널의 다음 뎁스값 = 현재패널의 뎁스값 + 패널 수 + (topUI가 들어갈 공간 1).
            nextDepth = (_popupList[count - 1].uiPanel.depth) + (_popupList[count - 1].panelCount) + 1;
        }
        if (bShowTopUI == true)
            topUiPanel.depth = nextDepth + panelCount;
        return nextDepth;
    }

    public void TopUIDepthAndSortLayer()
    {
        SettingTopUIDepth();
        SettingSortLayer();
    }

    private void SettingTopUIDepth()
    {
        int _nCount = _popupList.Count;

        if (_nCount != 0)
        {
            for(int i = _nCount-1; i >= 0; i--)
            {
                if (_popupList[i].bShowTopUI == true)
                {
                    topUiPanel.depth = _popupList[i].uiPanel.depth + _popupList[i].panelCount;
                    return;
                }
                else
                    topUiPanel.depth = _popupList[i].uiPanel.depth - 1;
            }
        }
        else
            topUiPanel.depth = 1;
    }

    private void SettingSortLayer()
    {
        int pCount = _popupList.Count;
        if (pCount - 1 >= 0)
        {
            UIPopupBase popup = _popupList[pCount - 1];
            //이전 팝업이 재화 UI를 띄워야 하고, sortOrder를 사용 중이라면 topUI가 해당 팝업 위로 가도록.
            if (popup.bShowTopUI == true && 
                (popup.uiPanel.useSortingOrder == true || (popup.uiPanel.useSortingOrder == false && popup.sortOrderCount > 0)))
            {
                topUiPanel.useSortingOrder = true;
                if (popup.uiPanel.sortingOrder < 10)
                    topUiPanel.sortingOrder = 10 + (popup.sortOrderCount) + (popup.panelCount) + 1;
                else
                    topUiPanel.sortingOrder = (popup.uiPanel.sortingOrder) + (popup.sortOrderCount) + (popup.panelCount) + 1;
            }
            else
            {
                topUiPanel.useSortingOrder = false;
                return;
            }
        }
        else
        {
            topUiPanel.useSortingOrder = false;
        }
    }

    private int GetSortLayer(int orderCount)
    {
        int count = _popupList.Count;
        int nextLayer = -1;

        //이전 팝업이 sortLayer 사용 안하고, 현재 팝업은 사용 한다면 초기 값(10) 부터 시작.
        if (count == 0 || (_popupList[count - 1].sortOrderCount == 0 && _popupList[count - 1].uiPanel.useSortingOrder == false))
        {
            if (orderCount > 0)
            {
                //10부터 시작.
                nextLayer = 10;
            }
        }
        //이전 팝업이 sortLayer 사용하고 있다면(패널에는 적용안돼있는 경우).
        else if (_popupList[count - 1].sortOrderCount > 0 && _popupList[count - 1].uiPanel.useSortingOrder == false)
        {
            nextLayer = 10 + (_popupList[count - 1].sortOrderCount) + (_popupList[count - 1].panelCount) + 1;
        }
        //이전 팝업이 sortLayer 사용하고 있다면.
        else
        {
            //패널의 다음 layer값 = 현재패널의 layer값 + layer 수 + 패널 카운트+ (topUI가 들어갈 공간 1).
            nextLayer = (_popupList[count - 1].uiPanel.sortingOrder) + (_popupList[count - 1].sortOrderCount) + (_popupList[count - 1].panelCount) + 1;
        }
        return nextLayer;
    }

    public void TopUIPanelDepth(UIPopupBase popup)
    {
        if (popup.bShowTopUI == false)
            return;
        //topUI depth 값 = 현재패널의 depth값 + 패널 카운트.
        topUiPanel.depth = popup.uiPanel.depth + popup.panelCount;
    }

    public void TopUIPanelSortOrder(UIPopupBase popup)
    {
        if (popup.bShowTopUI == false || (popup.uiPanel.useSortingOrder == false && popup.sortOrderCount == 0))
            return;
        topUiPanel.useSortingOrder = true;
        //topUI layer값 = 현재패널의 layer값 + layer 수 + 패널 카운트.
        if(popup.uiPanel.sortingOrder < 10)
            topUiPanel.sortingOrder = 10 + popup.sortOrderCount + popup.panelCount + 1;
        else
            topUiPanel.sortingOrder = popup.uiPanel.sortingOrder + popup.sortOrderCount + popup.panelCount + 1;
    }

    private Vector2 CalculateUISize(float _fHeight)
    {
        Vector2 _size = Vector2.zero;
        float _fAspect = _camera.aspect;
        _size = new Vector2((_fAspect * _fHeight), _fHeight);
        return _size;
    }

    //하트부족시 하트흔들기
    public void ShakeHeart()
    {
        StartCoroutine(shakingHeart());
    }

    IEnumerator shakingHeart()
    {
        float timer = 0f;

        while (true)
        {

            timer += Global.deltaTimeLobby*10f;
            if (timer > ManagerBlock.PI90*2)
            {
                _StarSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            }
            float ratio = Mathf.Sin(timer*8)/Mathf.Exp(timer);
            _StarSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, ratio*30f));
            yield return null;
        }
        bTouchTopUI = true;
    }

    public void GuestLoginSignInCheck(bool bSortOrder = false, int order = 0, Method.FunctionVoid callbackClose = null, Method.FunctionVoid callbackEnd = null)
    {
        UIPopupSystem popupConfirm = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popupConfirm.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_1" ), false, null );
        popupConfirm.FunctionSetting( 1, "LineLoginFromGuest", gameObject, true );
        if (bSortOrder == true)
        {
            popupConfirm.SortOrderSetting(true, order);
        }
        if (callbackClose != null)
        {
            popupConfirm._callbackClose += callbackClose;
        }
        if (callbackEnd != null)
        {
            popupConfirm._callbackEnd += callbackEnd;
        }
    }

    public void RebootSignOut(bool bSortOrder = false, int order = 0, Method.FunctionVoid callbackClose = null)
    {
        UIPopupSystem popupConfirm = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        /*"System" ,"Sign in?" */
        popupConfirm.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_55"), true, null);
        popupConfirm.FunctionSetting(1, "RebootSignOut_Internal", gameObject, true);
        popupConfirm.SetButtonText(1, Global._instance.GetString("btn_54"));
        popupConfirm.SetButtonText(2, Global._instance.GetString("btn_55"));
        if (bSortOrder == true)
        {
            popupConfirm.SortOrderSetting(true, order);
        }
        if (callbackClose != null)
        {
            popupConfirm._callbackClose += callbackClose;
        }
    }

    public void RebootSignOut_Internal()
    {
        ServiceSDK.ServiceSDKManager.instance.SignOut();

        ServiceSDK.ServiceSDKManager.isRebootSignIn = true;
        Global.isChangeUserData = true;
        Global.ReBoot();
    }

    /// <summary>
    /// 게스트 유저가 라인 로그인 유도 팝업을 통해 Reboot
    /// </summary>
    private void LineLoginFromGuest()
    {
        ServiceSDK.ServiceSDKManager.isRebootSignIn = true;
        Global.ReBoot();
    }

    public void LackCoinsPopUp(bool isIngame = false)
    {
        if (UIPopupSystem._instance != null) return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        if (!isIngame)
            popupSystem.FunctionSetting(1, "OnClickCoinShop", gameObject, true);
        else
            popupSystem.FunctionSetting(1, "OnClickCoinShop_Ingame", gameObject, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_6"), false);
        popupSystem.SetResourceImage("Message/coin");
    }

    private IEnumerator OnClickCoinShop_Ingame()
    {
        OnClickCoinShop();
        CoShowUI(0.1f, true, TypeShowUI.eTopUI);
        yield return new WaitUntil(() => UIPopupShop._instance == null);
        CoShowUI(0.1f, false, TypeShowUI.eTopUI);
    }

    public void OpenPopupEventOver()
    {
        ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
        {
            popup.SortOrderSetting();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
        });
    }
    
    public void OpenReadyAndEndContentsEventPopup()
    {
        if (ManagerData.IsStageAllClear() && ManagerEndContentsEvent.CheckStartable())
        {
            _instance.OpenPopupReadyEndContents();
        }
        else
        {
            _instance.OpenPopupReadyLastStage(false);
        }
    }

    public void LackDiamondsPopUp()
    {
        if (UIPopupSystem._instance != null) return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        popupSystem.FunctionSetting(1, "OnClickDiaShopAfterLack", gameObject, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_5"), false);
        popupSystem.SetResourceImage("Message/jewel");
    }

    public void LackCloverProcess()
    {
        var cloverPack = ManagerUI._instance.SpotCloverPackageCheck();

        if (cloverPack == null)
        {
            ManagerUI._instance.OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Clover));
        }
        else
        {
            ManagerUI._instance.OpenPopupPackage_Reconfirm(cloverPack);

            //재확인 팝업 출력 그로시
            {
                var    mode  = Global.GameInstance.GetGrowthyGameMode();
                string stage = mode == ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL ? Global.stageIndex.ToString() : Global.GameInstance.GetGrowthyStageIndex();

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPOT_CLOVER_PACKAGE,
                    "SPOT_CLOVER_PACKAGE_SUGGEST",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    str1:$"{mode}_{Global.eventIndex}_{stage}"
                );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }
        }
    }

    void OnClickDiaShopAfterLack()
    {
        OnClickDiamondShop(true);
    }
   
    public string UserNameLengthCheck(string name)
    {
        string name_trim = name.Trim();
        if (name_trim.Length < 1)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_11"), false);
            return "";
        }
        return name_trim;
    }

    public string UserNameCharacterCheck(string name)
    {
        string nameChange = name;
        nameChange = nameChange.Replace("[", "");
        nameChange = nameChange.Replace("]", "");

        if (nameChange.Length != name.Length)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_4"), false);
        }
        return nameChange;
    }

    public void SettingDiaryNewIcon(bool bActive)
    {
        diaryNewIcon.SetActive(bActive);
    }

    public void SettingEventIcon(bool bActive)
    {
        diaryEventIcon.SetActive(bActive);
    }

    public void SettingInviteEvent(bool bActive)
    {
        LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.invite] = bActive;
    }

    public void SettingStageNewIcon(bool bActive)
    {
        stageNewIcon.SetActive(bActive);
    }

    void OnClickBtnComingSoon()
    {
        if (UIPopupSystem._instance != null) return;

        //커밍순 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_15"), false);
        popupSystem.SetResourceImage("Message/soon");
    }

    void OnClickBlossom()
    {
        if(ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
        {
            this.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ManagerPokoFlowerEvent.PokoFlowerEventIndex));
        }
    }

    void SaleInfoSetting()
    {
        if (ServerRepos.LoginCdn == null)
            return;

        //세일 정보 받아와서 세팅.
       
        if (ServerContents.CloverShop.Normal != null && ServerContents.CloverShop.Normal.Count(x => x.saleLevel > 0 ) > 0)
        {
            saleIcon_clover.SetActive(true);
        }
        else
        {
            saleIcon_clover.SetActive(false);
        }

        if (ServerContents.CoinShop.Normal != null && ServerContents.CoinShop.Normal.Count(x => x.saleLevel > 0 ) > 0)
        {
            saleIcon_coin.SetActive(true);
        }
        else
        {
            saleIcon_coin.SetActive(false);
        }

        if (ServerContents.JewelShop.Normal != null && ServerContents.JewelShop.Normal.Count(x => x.saleLevel > 0 ) > 0)
        {
            saleIcon_jewel.SetActive(true);
        }
        else
        {
            saleIcon_jewel.SetActive(false);
        }

        if(ServerContents.WingShop.Normal != null && ServerContents.WingShop.Normal.Count(x => x.saleLevel > 0 ) > 0)
        {
            saleIcon_wing.SetActive(true);
        }
        else
        {
            saleIcon_wing.SetActive(false);
        }
    }

    public void NewInfoSetting(bool isFinishAnim = false)
    {
        if (ManagerLobby._instance == null)
            return;
        
        if (ManagerLobby._instance._state == TypeLobbyState.None || ManagerLobby._instance._state == TypeLobbyState.Preparing || ManagerLobby._instance._state == TypeLobbyState.PreparingEnd)
            return;
        
        int clearMission = ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen;

        // 느낌표 비활성화 조건 1 : 미션 카운트가 6 이하일 경우
        bool check1 = GameData.User != null && GameData.User.missionCnt <= clearMission;
        // 느낌표 비활성화 조건 2 : 미션 카운트가 7로 넘어가는 시점에서 연출이 끝나지 않은 경우 (예외처리)
        bool check2 = GameData.User != null && ManagerLobby._instance != null && GameData.User.missionCnt == clearMission + 1 && !isFinishAnim;
        
        if (check1 || check2)
            return;

        bool isNewClover = false;
        List<string> cloverPrevSpecialList = PlayerPrefs.GetString("ShopCloverSpecialItemList").Split(',').ToList();
        if (ServerContents.CloverShop.SpecialByDia != null && ServerContents.CloverShop.SpecialByDia.Count > 0)
        {
            foreach (var item in ServerContents.CloverShop.SpecialByDia)
            {
                string itemKey = item.g_code + "_" + item.idx;
                if (!cloverPrevSpecialList.Contains(itemKey))
                    isNewClover = true;
            }

            newIcon_clover.SetActive(isNewClover);
        }
        if (isNewClover == false && ServerContents.CloverShop.SpecialByInApp != null && ServerContents.CloverShop.SpecialByInApp.Count > 0)
        {
            string       itemKey         = "";
            foreach (var item in ServerContents.CloverShop.SpecialByInApp)
            {
                #if UNITY_IPHONE
                itemKey = item.sku_i + "_" + item.idx;
                #elif UNITY_ANDROID
                itemKey = item.sku_a + "_" + item.idx;
                #else
                itemKey = item.sku_a + "_" + item.idx;
                #endif
                if (!cloverPrevSpecialList.Contains(itemKey))
                    isNewClover = true;
            }

            newIcon_clover.SetActive(isNewClover);
        }
        
        if (ServerContents.WingShop.Special != null && ServerContents.WingShop.Special.Count > 0)
        {
            List<string> prevSpecialList = PlayerPrefs.GetString("ShopWingSpecialItemList").Split(',').ToList();
            bool isNew = false;
            foreach (var item in ServerContents.WingShop.Special)
            {
                string itemKey = item.g_code + "_" + item.idx;
                if (!prevSpecialList.Contains(itemKey))
                    isNew = true;
            }
            newIcon_wing.SetActive(isNew);
        }
        
        if (ServerContents.CoinShop.Special != null && ServerContents.CoinShop.Special.Count > 0)
        {
            List<string> prevSpecialList = PlayerPrefs.GetString("ShopCoinSpecialItemList").Split(',').ToList();
            bool isNew = false;
            foreach (var item in ServerContents.CoinShop.Special)
            {
                string itemKey = item.g_code + "_" + item.idx;
                if (!prevSpecialList.Contains(itemKey))
                    isNew = true;
            }
            newIcon_coin.SetActive(isNew);
        }
        
        if (ServerContents.JewelShop.Special != null && ServerContents.JewelShop.Special.Count > 0)
        {
            List<string> prevSpecialList = PlayerPrefs.GetString("ShopJewelSpecialItemList").Split(',').ToList();
            bool isNew = false;
            string itemKey = "";
            foreach (var item in ServerContents.JewelShop.Special)
            {
                #if UNITY_IPHONE
                itemKey = item.sku_i + "_" + item.idx;
                #elif UNITY_ANDROID
                itemKey = item.sku_a + "_" + item.idx;
                #else
                itemKey = item.sku_a + "_" + item.idx;
                #endif
                if (!prevSpecialList.Contains(itemKey))
                    isNew = true;
            }

            newIcon_jewel.SetActive(isNew);
        }
    }

    private void SwitchTopUIAdventureMode(bool adv)
    {
        if( adv)
        {
            this.objClover.SetActive(true);
            this.objWing.SetActive(true);
            this.objStar.SetActive(true);
            this.objExp.SetActive(true);
            this.objClover.transform.DOLocalMoveY(140f, 0.5f, true);
            this.objWing.transform.DOLocalMoveY(-45f, 0.5f, true);
            this.objStar.transform.DOLocalMoveY(140f, 0.5f, true);
            this.objExp.transform.DOLocalMoveY(-45f, 0.5f, true);
        }
        else
        {
            this.objClover.SetActive(true);
            this.objWing.SetActive(true);
            this.objStar.SetActive(true);
            this.objExp.SetActive(true);
            this.objClover.transform.DOLocalMoveY(-45f, 0.5f, true);
            this.objWing.transform.DOLocalMoveY(140f, 0.5f, true);
            this.objStar.transform.DOLocalMoveY(-45f, 0.5f, true);
            this.objExp.transform.DOLocalMoveY(140f, 0.5f, true);

        }
    }

    private bool isTopUIAdventureMode = false;

    public bool IsTopUIAdventureMode
    {
        get
        {
            return isTopUIAdventureMode;
        }
        set
        {
            if(isTopUIAdventureMode != value)
            {
                isTopUIAdventureMode = value;
                SwitchTopUIAdventureMode(isTopUIAdventureMode);
            }
        }
    }



    private void CloverSendTimeEventSetting()
    {
        if (ServerRepos.LoginCdn == null)
            return;

        if (ServerRepos.LoginCdn.sendCloverEventVer != 0)
            LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.ranking] = true;
        else
            LobbyMenuUtility.eventContentInfo[LobbyMenuUtility.MenuEventIconType.ranking] = false;
    }

    public void OnClickWingShop()
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false) return;

        if (bTouchTopUI == false) return;

        if (!UIPopupShop.IsWingActive) return;

        if (UIPopupShop._instance != null)
        {
            UIPopupShop._instance.SelectTap(UIPopupShop.ShopType.Wing);
        }
        else
        {
            bTouchTopUI = false;
            OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Wing));
        }
    }

    public void OnClickCloverShop()
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false) return;

        if (bTouchTopUI == false) return;

        if (!UIPopupShop.IsCloverActive) return;

        if (UIPopupShop._instance != null)
        {
            UIPopupShop._instance.SelectTap(UIPopupShop.ShopType.Clover);
        }
        else
        {
            bTouchTopUI = false;
            OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Clover));
        }
    }

    public void OnClickCoinShop()
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false) return;

        if (bTouchTopUI == false) return;

        if (!UIPopupShop.IsCoinActive) return;

        if (UIPopupShop._instance != null)
        {
            UIPopupShop._instance.SelectTap(UIPopupShop.ShopType.Coin);
        }
        else
        {
            bTouchTopUI = false;
            OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Coin));
        }
    }

    public void OnClickDiamondShop() => OnClickDiamondShop(false);

    public void OnClickDiamondShop(bool afterLack)
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false)
        {
            return;
        }

        if (bTouchTopUI == false)
        {
            return;
        }

        if (!UIPopupShop.IsDiamondActive)
        {
            return;
        }

        if (UIPopupShop._instance != null)
        {
            if (afterLack)
            {
                ManagerUI._instance.UpdateUI();
            };

            UIPopupShop._instance.SelectTap(UIPopupShop.ShopType.Diamond);
        }
        else
        {
            bTouchTopUI = false;
            OpenPopup<UIPopupShop>((popup) =>
            {
                popup.Init(UIPopupShop.ShopType.Diamond);
                if (afterLack)
                {
                    popup._callbackOpen = () =>
                    {
                        ManagerUI._instance.UpdateUI();
                    };
                };
            });
        }
    }

    public void OnClickMethod()
    {
        OnClickMethod_OpenPopUp();
    }

    public void OnClickMethod_OpenPopUp(bool isMissionFlow = false)
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false) return;

        if (bTouchTopUI == false)
            return;

        //인게임일경우 안열림
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method)
            return;

        bTouchTopUI = false;

        if (UIPopupShop._instance != null)
        {
            UIPopupShop._instance._callbackClose += () => OpenPopup<UIPopUpMethod>().InitData(isMissionFlow);
            UIPopupShop._instance.ClosePopUp();
        }
        else
        {
            OpenPopup<UIPopUpMethod>().InitData(isMissionFlow);
        }
    }

    public void OnClickAdvExp()
    {
        if (ManagerLobby._instance.IsLoabbyActionState == false) return;

        if (bTouchTopUI == false)
            return;

        //인게임일경우 안열림
        if (SceneManager.GetActiveScene().name == "InGame")
            return;


        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method)
            return;

        bTouchTopUI = false;

        if (UIPopupShop._instance != null)
        {
            UIPopupShop._instance._callbackClose += () => OpenPopup<UIPopUpExpBallInfo>();
            UIPopupShop._instance.ClosePopUp();
        }
        else
        {
            OpenPopup<UIPopUpExpBallInfo>();
        }
    }

    public void OpenPopupRequestReview()
    {
        // 이번 버전에서 이전에 리뷰를 쓰거나 팝업을 다시 보지 않도록 설정한 값이 있으면 팝업을 오픈하지 않는다.
        int appReviewVer = ServerRepos.LoginCdn.appReviewVer;
        if (PlayerPrefs.HasKey(Review.key) && PlayerPrefs.GetInt(Review.key) == appReviewVer)
            return;

        ManagerUI._instance.OpenPopup<UIPopupRequestReview>().InitData(appReviewVer);
    }
    
    public void OpenPopupWorldRankExchangeShop()
    {
        var popup = OpenPopup<UIPopUpWorldRankExchangeStation>();
        popup.InitScrollDepth();
    }
    public void OnRequestReview()
    {
        Review.RequestReview();
    }

    public void SortingOrderFalse()
    {
        topUiPanel.useSortingOrder = false;
    }

    void OnClickExit()
    {
#if UNITY_ANDROID
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call<bool>("moveTaskToBack", true);
#else

        Application.Quit();
#endif


    }

    //ios 심사환경
    private bool bScreeningEnvironment = false;

    public bool iosScreeningEnvironment
    {
        get
        {
            return bScreeningEnvironment;
        }
        private set
        {
            bScreeningEnvironment = value;
        }
    }

    //심사환경일 때 socialButton이 뜨지 않도록 메뉴 child에서 제거 후 grid를 정리합니다.
    void SetAnchorRightUI()
    {
        iosScreeningEnvironment = true;
        menuWakeUpBubble.SetActive(false);
        
        int idx = menuGridChild.FindIndex(x => x == socialButton);
        if (idx == -1)
            return;
        
        for (int i = 0; i < menuGridChild.Count; i++)
        {
            menuGridChild[i].SetActive(true);
        }
        menuGridChild[idx].SetActive(false);
        rightDownGrid.Reposition();
        
        if(idx != -1)
            menuGridChild.RemoveAt(idx);

        for (int i = 0; i < menuGridChild.Count; i++)
        {
            menuGridChild[i].SetActive(false);
        }
    }

    public bool UIButtonCanClickCheck()
    {
        if (Global.CanClickButton() == false)
            return false;
        Global.InitClickTime();
        return true;
    }   


    public static string GetColorTypeString(BlockColorType colorType)
    {
        switch (colorType)
        {
            case BlockColorType.A:
                return "A";

            case BlockColorType.B:
                return "B";

            case BlockColorType.C:
                return "C";

            case BlockColorType.D:
                return "D";

            case BlockColorType.E:
                return "E";

            case BlockColorType.F:
                return "F";

            default:
                return "";
        }
    }

    public event Action FirstOpenCallback;
    public event Action AllCloseCallback;

    public void FocusCheck()
    {
        if(_popupList.Count == 0)
        {
            AllCloseCallback?.Invoke();
        }
        else if(_popupList.Count == 1)
        {
            FirstOpenCallback?.Invoke();
        }

        for(int i = 0; i < _popupList.Count; ++i)
        {
            _popupList[i].FocusCheck();
        }
    }

    #region 버튼 컬러 설정
    public (string, Color) GetButtonColorData_BigButton(BtnColorType btnColorType)
    {
        string spriteName = "";
        switch (btnColorType)
        {
            case BtnColorType.green:
                spriteName = "button_play";
                break;
            case BtnColorType.yellow:
                spriteName = "button_002";
                break;
            case BtnColorType.gray:
                spriteName = "button_play03";
                break;
        }

        Color effectColor = new Color(0.13f, 0.4f, 0.05f);
        switch (btnColorType)
        {
            case BtnColorType.green:
                effectColor = new Color(0.13f, 0.4f, 0.05f);
                break;
            case BtnColorType.yellow:
                effectColor = new Color(0.67f, 0.2f, 0f);
                break;
            case BtnColorType.gray:
                effectColor = new Color(0.35f, 0.35f, 0.35f);
                break;
        }

        return (spriteName, effectColor);
    }
    #endregion

    #region 인게임 목표 아틀라스 설정
    public IEnumerator CoMakeTargetAtlas(int[] collectCount,int[] collectColorCount, List<CollectTargetInfo> listTargetInfo)
    {
        int overrideBlockIndex = 0;
        if (Global.GameType == GameType.EVENT)
        {
            CdnEventChapter eventChapterData = ServerContents.EventChapters;
            overrideBlockIndex = eventChapterData.blockIndex;
        }

        List<string> listUseTargetType = new List<string>();

        //구버전 데이터로 목표 검사
        for (int i = 0; i < collectCount.Length; i++)
        {
            if (collectCount[i] == 0)
                continue;

            listUseTargetType.Add(i.ToString());
        }

        for (int i = 0; i < collectColorCount.Length; i++)
        {
            if (collectColorCount[i] == 0)
                continue;

            listUseTargetType.Add(((int)TARGET_TYPE.COLORBLOCK).ToString());
            break;
        }

        //신버전 데이터로 목표 검사
        for (int i = 0; i < listTargetInfo.Count; i++)
        {
            listUseTargetType.Add(listTargetInfo[i].targetType.ToString());
        }

        //목표에서 기본적으로 사용되는 이미지 아틀라스 추가
        listUseTargetType.Add("100");

        yield return ManagerUIAtlas._instance.BuildAtlas(ManagerUIAtlas.AtlasType.TARGET, overrideBlockIndex, listUseTargetType);
    }
    #endregion

    public void SpawnEffect(Transform iconPosition, UIUrlTexture texture)
    {
        StartCoroutine(CoSpawnEffect(iconPosition, texture));
    }

    private IEnumerator CoSpawnEffect(Transform iconPosition, UIUrlTexture texture)
    {
        GameObject obj = ManagerObjectPool.Spawn(_objEventIconEffect, iconPosition);
        obj.GetComponent<UIUrlTexture>().mainTexture = texture.mainTexture;
        obj.transform.parent = iconPosition;
        obj.transform.localScale = Vector3.one;
        obj.transform.parent = _transform; 
        var widget = obj.GetComponent<UIWidget>();
        widget.pivot = UIWidget.Pivot.Center;
        float offesetY = obj.transform.position.y - iconPosition.position.y;

        yield return new WaitForSeconds(0.01f);
        texture.enabled = false;
        
        while (obj.activeInHierarchy)
        {
            Vector3 pos = iconPosition.position;
            pos.y += offesetY;
            obj.transform.position = pos;
            yield return null;
        }
        
        texture.enabled = true;
        widget.pivot = UIWidget.Pivot.Top;
        
    }
}
