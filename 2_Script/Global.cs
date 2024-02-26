using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using ServiceSDK;

public class Global : MonoBehaviour
{
    public static Global _instance = null;

    static public bool _debugEdit = true;
    static public bool _debugLog = true;
    static public bool _pendingReboot = false;
    static public float _debugNetworkDelay = 0.0f;
    
    static public string _appVersion = "1.0.3";
    static public string _countryCode = "US";
    static public string _marketUrl = "https://play.google.com/store/apps/details?id=com.linecorp.LGPKPK";
    static public bool _optionBGM = true;
    static public bool _optionSoundEffect = true;     //BGM
    static public bool _optionGameNotice = true;
    static public bool _optionLocalPush = true;
    static public string _gameServerAddress;
    static public string _cdnAddress = "http://ec2-52-79-164-197.ap-northeast-2.compute.amazonaws.com/";
    //static public string _cdnAddress = "http://110.46.209.133:5885/www/pokopoko/cdn/";
    static public string _marketAppLinkAddr = "market://details?id=com.linecorp.LGPKV";
    //static public string _timeLineAddress = "http://down.hangame.co.jp/jp-smp/dist/SJLGPP/timeline/";

    // 메모리 해킹을 대비,EncValue,,하나의 함수내에서 읽고 쓸때 유의, 함수에서 읽어서 사용할때 for나 while을 돌때는 별도의 함수내 변수에 읽어서 사용하기...int coin = Global.coin;  while(){ coin++; };
    static public bool _touch = true;

    static private bool _enableOnlineMode = true;
    static public bool _onlineMode = true;             // Online, Offline (정상 플리에와 캐시된 데이터로 오프라인 모드) 
    static public bool _rebootWithAuth = false;        // Line, Server등 인증 관련 오류시 true => 로그인 버튼 유도
    
    static public bool isChangeUserData = false;

    //이전 로그인 했을 때, pid 정보를 담고있음.
    static public string lastLogin_pid = "";
    
    //인게임에서만 사용 (레디 팝업 오픈 시 초기화)
    private static bool _isSingleRoundEvent = false;

    public static bool _optionTutorialOn //튜토리얼 온오프
    {
        get
        {
            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                return PlayerPrefs.GetInt("_optionTutorialOn", 1) == 1;
            }
            else
            {
                return true;
            }

        }
        set
        {
            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                PlayerPrefs.SetInt("_optionTutorialOn", (value) ? 1 : 0);
            }
        }
    }

    public static bool isSingleRoundEvent
    {
        get
        {
            return _isSingleRoundEvent;
        }
        set
        {
            _isSingleRoundEvent = value;
        }
    }

    //[SerializeField]
    //private bool _forceLoadBundle = false;

    static public bool LoadFromInternal
    {
        get { return !UIPopupServerSelect.GetForceLoadBundleActive() && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer); }
        private set { }
    }
    static public string FileUri
    {
        // windows 의 경우, "file:///C:/Windows/..." 형태가 되어야 하고, persistantDataPath 가 "C:/" 부터 시작하므로 file:/// 으로 시작하는 게 맞고
        // 기타 안드로이드, ios의 경우 persistantDataPath가 "/mnt/" 같은 형태로 시작하고 "file:///mnt/..." 형태가 되어야하므로 file://으로 시작하는 게 맞음
        get
        {
            return NetworkSettings.Instance.IsRealDevice() ? "file://" : "file:///";
        }
    } 
    // user data
    static private EncValue _coin = new EncValue();
    static public int coin
    {
        get { return _coin.Value + ManagerUI._instance.GetActionCoin(); }
        set { _coin.Value = value; }
    }
    static private EncValue _jewel = new EncValue();
    static public int jewel
    {
        get { return _jewel.Value + ManagerUI._instance.GetActionJewel(); }
        set {
            if( _jewel.Value != 0 && value == 0)
            {
                Debug.Log(System.Environment.StackTrace.ToString());
            }
            _jewel.Value = value;

        }
    }
    static private EncValue _clover = new EncValue();
    static public int clover
    {
        get { return _clover.Value; }
        set
        {
            _clover.Value = value;
            PlayerPrefs.SetInt("checkClover", _clover.Value);
        }
    }
    static private EncValue _star = new EncValue();
    static public int star
    {
        get { return _star.Value; }
        set { _star.Value = value; }
    }

    static private EncValue _exp = new EncValue();
    static public int exp
    {
        get { return _exp.Value; }
        set { _exp.Value = value; }
    }

    static private EncValue _wing = new EncValue();
    static public int wing
    {
        get { return _wing.Value; }
        set { _wing.Value = value;
        PlayerPrefs.SetInt("checkWing", _wing.Value);
        }
    }
    static private EncValue _chapterIndex = new EncValue();
    static public int chapterIndex
    {
        get { return _chapterIndex.Value; }
        private set { _chapterIndex.Value = value; }
    }
    static private EncValue _stageIndex = new EncValue();
    static public int stageIndex                    //로비에서 시작이나 스테이지 선택창에서 등에서 선택되어 플레이 할 스테이지 인텍스 , 1이면 첫 스테이지
    {
        get { return _stageIndex.Value; }
        private set { _stageIndex.Value = value; }
    }
    static private EncValue _eventIndex = new EncValue();
    static public int eventIndex                    // 현재 진행중이면서 선택한 이벤트 인텍스 (인게임 레디창,인게임 결과에 쓰임)
    {
        get { return _eventIndex.Value; }
        private set { _eventIndex.Value = value; }
    }

    static private EncValue _specialEventIndex = new EncValue();    //모으기이벤트
    static public int specialEventIndex                  
    {
        get { return _specialEventIndex.Value; }
        set { _specialEventIndex.Value = value; }
    }

    static private EncValue _coinEvent = new EncValue();
    static public int coinEvent
    {
        get { return _coinEvent.Value; }
        set { _coinEvent.Value = value; }
    }

    static private EncValue _day = new EncValue();
    static public int day
    {
        get { return _day.Value; }
        set { _day.Value = value; }
    }
    static private EncValue _flower = new EncValue();
    static public int flower
    {
        get { return _flower.Value; }
        set { _flower.Value = value; }
    }
    
    //콜라보 정보
    static private EncValue _CollaboIndex = new EncValue();    //모으기이벤트
    static public int CollaboIndex
    {
        get { return _CollaboIndex.Value; }
        set { _CollaboIndex.Value = value; }
    }

    // 직전 스테이지 클리어 했는지?
    static private bool _isClear = false;
    static public bool IsClear
    {
        get { return _isClear; }
        private set { _isClear = value; }
    }

    static private EncValue _gameType = new EncValue((int)GameType.NORMAL);
    static public GameType GameType
    {
        get { return (GameType)_gameType.Value; }
        private set { _gameType.Value = (int)value; }
    }

    static private EncValue _gameSoundType = new EncValue((int)GameType.NORMAL);
    static public GameType GameSoundType
    {
        get { return (GameType)_gameSoundType.Value; }
        private set { _gameSoundType.Value = (int)value; }
    }

    static GameType_Base _GameInstance = null;
    static public GameType_Base GameInstance
    {
        get { return _GameInstance; }
        private set { _GameInstance = value; }
    }

    // 각종 로컬 상태
    static public bool join = false;    // 첫가입 유저

    [System.NonSerialized]
    public Dictionary<string, string> _stringData = new Dictionary<string, string>();
    public bool invalidateString = true;

    //랭킹 포인트 데이터
    [System.NonSerialized]
    public List<RankingPointGradeData> _strRankingPointGradeData = new List<RankingPointGradeData>();
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    //  
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    // 시간을 나눈 이유는 예를 들어 팝업이 뜨면 진행하던 요소들이 일시정지 시키거나 할때
    static public float timeScale = 1f; 
    static public float deltaTime = 0f;     // 각종 시스템과 UI등에 사용
    static public float deltaTimeNoScale = 0f;

    static public float timeScaleLobbySpine = 1f;   //빨리감기 할 때 스파인은 속도 너무 빠르면 뒷프레임 재생안하는 문제가 있어서...
    static public float timeScaleLobby = 1f;// 로비내 요소 모든 업데이트 코루틴에 영향을 줌(보니의 움직임은 정지 시키고 ui는 열린다던지 하는 동작을 위해)
    static public float deltaTimeLobby = 0f;// 로비내 요소의 모든 업데이트 코루틴을 이걸 사용,  별도의 Time.delta를 사용하면 안됨
    
    static public float timeScalePuzzle = 1f;// 인게임내 요소 모든 업데이트 코루틴에 
    static public float deltaTimePuzzle = 0f;// 인게임내 요소의 모든 업데이트 코루틴을 이걸 사용,  별도의 Time.delta를 사용하면 안됨


    static public Vector3 _touchPos = Vector3.zero;
    static public Vector3 _touchBeginPos = Vector3.zero;
    static public Vector3 _touchDeltaPos = Vector3.zero;
    static public bool _touching = false;
    static public bool _touchBegin = false;
    static public bool _touchEnd = false;
    static public bool _touchTap = false;
    static public float _touchTapTimer = 0f;
    static public int _touchCount = 0;
    int fingerId = -1;
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    //  prefab data
    public GameObject _objScreenEffect;
    public GameObject _objSceneLoading;
    public GameObject _objNetworkLoading;
    public GameObject _objClickBlocker;
    public GameObject _objBlindMaker;
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    //  const data
    //
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    static public readonly Quaternion billboardRotation = Quaternion.Euler(50f, -45f, 0f);
    static public int eventObjectMask = 0;
    static public int cameraBounceMask = 0;
    // path
    static public string assetBundleDirectory = "AssetBundles";
    static public string adventureDirectory = "/Adventure/";             // 모험모드 이미지 (동물, 몬스터, 동물아이콘)
    static public string gameDataDirectory = "/GameData/";        // 번들 게임 데이타
    static public string gameImageDirectory = "/GameImage/";             // 미션,퀘스트,재료 등등 게임 이미지
    static public string noticeDirectory = "/Notice/";                   // 공지 팝업 이미지
    static public string movieDirectory = "/movie/";                   // 공지 팝업 이미지
    //static public string thumbnailDirectory = "/Thumbnail/";             // 프로필 썸네일 이미지     
    static public string StageDirectory = "/Stage/";             // 프로필 썸네일 이미지    
    static public string effectDirectory = "/Effect/";
    static public string soundDirectory = "/Sound/";
    static public string cachedScriptDirectory = "/CachedScript/";
    static public string cachedResourceDirectory = "/CachedResource/";

#region CheatTool
    
    //인게임 클리어버튼
    public bool showInGameClearBTN = false;

    //스팟패키지 관련 버튼
    public bool showFailDataBTN = false;

    //인게임 튜토리얼 보이게 하는 버튼
    public bool showTutorialBTN = false;
    //미션/퀘스트 인덱스 보이게 할지 버튼
    public bool isShowIndex = false;
    
    public bool ShowIngameInfo = false;
    
    // 일시정지 팝업에서 엔드 컨텐츠 맵 인덱스 보이게 할지 버튼
    public bool ShowECStage = false;

    public bool showAdNetworkInfo = false;
    // 미성년자 보호 가이드라인 관련 금액 표기 여부 버튼
    public bool showMinorPriceInfo = false;
    
#endregion
    
    #region UI버튼 터치관련.
    public static float clickTime = 0f;
    public static void InitClickTime()
    {
        clickTime = Time.time;
    }

    public static bool CanClickButton()
    {
        return Time.time - clickTime >= 0.5f;
    }
    #endregion

    void Awake()
    {
#if UNIUM_ENABLE
        treenod.qa.UniumManager.Instance.Initialize();
#endif
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;

        eventObjectMask = 1 << LayerMask.NameToLayer("EventObject");
        cameraBounceMask = 1 << LayerMask.NameToLayer("CameraCollider");

        gameDataDirectory = Application.persistentDataPath + gameDataDirectory;
        gameImageDirectory = Application.persistentDataPath + gameImageDirectory;
        noticeDirectory = Application.persistentDataPath + noticeDirectory;
        movieDirectory = Application.persistentDataPath + movieDirectory;
        //thumbnailDirectory = Application.persistentDataPath + thumbnailDirectory;
        StageDirectory = Application.persistentDataPath + StageDirectory;
        adventureDirectory = Application.persistentDataPath + adventureDirectory;

        effectDirectory = Application.persistentDataPath + effectDirectory;
        soundDirectory = Application.persistentDataPath + soundDirectory;
        cachedScriptDirectory = Application.persistentDataPath + cachedScriptDirectory;
        cachedResourceDirectory = Application.persistentDataPath + cachedResourceDirectory;

        LocalText.Instance.Init();
    }
	// Use this for initialization
	void Start () 
    {
        Application.targetFrameRate = 60;

        //그로씨 플로우 로그
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SPLASH);

        // 로비에서 시스템.  로딩할땐 안꺼지게.. 인트로도...
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Screen.sleepTimeout = SleepTimeout.SystemSetting;

        _optionBGM = PlayerPrefs.GetInt("_optionBGM", 1) == 1;
        _optionSoundEffect = PlayerPrefs.GetInt("_optionSoundEffect", 1) == 1;
        _optionLocalPush = PlayerPrefs.GetInt("_optionLocalPush", 1) == 1;

        // 필요한 디렉토리 검사해서 생성
        if (!System.IO.Directory.Exists(Global.gameDataDirectory))
            System.IO.Directory.CreateDirectory(Global.gameDataDirectory);
        if (!System.IO.Directory.Exists(Global.gameImageDirectory))
            System.IO.Directory.CreateDirectory(Global.gameImageDirectory);
        if (!System.IO.Directory.Exists(Global.noticeDirectory))
            System.IO.Directory.CreateDirectory(Global.noticeDirectory);
        if (!System.IO.Directory.Exists(Global.movieDirectory))
            System.IO.Directory.CreateDirectory(Global.movieDirectory);
        /*if (!System.IO.Directory.Exists(Global.thumbnailDirectory))
            System.IO.Directory.CreateDirectory(Global.thumbnailDirectory);*/
        if (!System.IO.Directory.Exists(Global.StageDirectory))
            System.IO.Directory.CreateDirectory(Global.StageDirectory);
        if (!System.IO.Directory.Exists(Global.adventureDirectory))
            System.IO.Directory.CreateDirectory(Global.adventureDirectory);
        if (!System.IO.Directory.Exists(Global.effectDirectory))
            System.IO.Directory.CreateDirectory(Global.effectDirectory);
        if (!System.IO.Directory.Exists(Global.soundDirectory))
            System.IO.Directory.CreateDirectory(Global.soundDirectory);
        if (!System.IO.Directory.Exists(Global.cachedScriptDirectory))
            System.IO.Directory.CreateDirectory(Global.cachedScriptDirectory);
        if (!System.IO.Directory.Exists(Global.cachedResourceDirectory))
            System.IO.Directory.CreateDirectory(Global.cachedResourceDirectory);

        if ( _GameInstance == null )
        {
            SetGameType_NormalGame(1);
        }

    }
	
	// Update is called once per frame
	void Update () {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // time
        deltaTimeNoScale = Mathf.Min(Time.unscaledDeltaTime, 1f / 10f);// = Mathf.Min(Time.deltaTime * timeScale, 1f / 20f);
        deltaTime = deltaTimeNoScale * timeScale;
        deltaTimeLobby = deltaTimeNoScale * timeScaleLobby;
        deltaTimePuzzle = deltaTimeNoScale * timeScalePuzzle;
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // touch
        //  
        _touchCount = 0;
        _touchBegin = false;
        _touching = false;
        _touchEnd = false;
        _touchTap = false;
        

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
            {
                _touchBegin = true;
                _touchTapTimer = Time.unscaledTime;

                _touchDeltaPos = Vector3.zero;
                _touchPos = Input.mousePosition;
                _touchBeginPos = Input.mousePosition;
            }

            if (Input.GetButton("Fire1") || Input.GetButton("Fire2"))
            {
                _touchCount = 1;
                _touching = true;
                _touchDeltaPos = Input.mousePosition - _touchPos;
                _touchPos = Input.mousePosition;
            }

            if (Input.GetButton("Fire2"))
            {
                _touchCount = 2;
            }

            if (Input.GetButtonUp("Fire1") || Input.GetButtonUp("Fire2"))
            {
                _touchEnd = true;

                //Debug.Log((_touchBeginPos - _touchPos).magnitude);
                if (Time.unscaledTime - _touchTapTimer < 0.4f && (_touchBeginPos - _touchPos).magnitude<30f)
                    _touchTap = true;
            }
        }
        else
        {
            _touchCount = Input.touchCount;
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch getTouch = Input.GetTouch(i);
                switch (getTouch.phase)
                {
                    case TouchPhase.Began:
                        {
                            if (fingerId < 0)
                            {
                                fingerId = getTouch.fingerId;
                                _touchPos = getTouch.position;
                                _touchBeginPos = Input.mousePosition;
                                _touchBegin = true;
                                _touchTapTimer = Time.unscaledTime;
                            }
                        }
                        break;
                    case TouchPhase.Moved:
                        {
                            if (fingerId == getTouch.fingerId)
                            {
                                _touching = true;
                                _touchDeltaPos = new Vector3(getTouch.position.x, getTouch.position.y, 0f) - _touchPos;
                                _touchPos = getTouch.position;
                            }
                        }
                        break;
                    case TouchPhase.Stationary:
                        {
                            if (fingerId == getTouch.fingerId)
                            {
                                _touching = true;
                            }
                        }
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        {
                            if (fingerId == getTouch.fingerId)
                            {
                                _touching = true;
                                _touchEnd = true;
                                _touchDeltaPos = new Vector3(getTouch.position.x, getTouch.position.y, 0f) - _touchPos;
                                _touchPos = getTouch.position;
                                fingerId = -1;

                                if (Time.unscaledTime - _touchTapTimer < 0.4f && (_touchBeginPos - _touchPos).magnitude < 25f)
                                    _touchTap = true;
                            }
                        }
                        break;
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	}

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static void SetGameTypeStateToDefault()
    {
        Global.chapterIndex = 0;
        Global.eventIndex = 0;
        Global.stageIndex = 1;
        Global.GameType = GameType.NORMAL;
        SetGameSoundType(GameType.NORMAL);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Normal>();
    }

    static public void SetGameType_Event(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.EVENT;
        SetGameSoundType(GameType.EVENT);
        Global.stageIndex = stageIndex;
        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Event>();
    }

    static public void SetGameType_NormalGame(int stageIndex = 0)
    {
        Global.chapterIndex = 0;
        Global.GameType = GameType.NORMAL;
        SetGameSoundType(GameType.NORMAL);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Normal>();
        if ( stageIndex == 0 )
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            stageIndex = myProfile.stage > ManagerData._instance.maxStageCount ? (int)ManagerData._instance.maxStageCount : (int)myProfile.stage;

            if (stageIndex == 0)
                stageIndex = 1;
        }

        Global.stageIndex = stageIndex;
        Global.eventIndex = 0;
    }

    static public void SetGameType_Adventure(int chapterIndex, int stageIndex)
    {
        Global.GameType = GameType.ADVENTURE;
        SetGameSoundType(GameType.ADVENTURE);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Adventure>();

        Global.chapterIndex = chapterIndex;
        Global.stageIndex = stageIndex;
        Global.eventIndex = 0;
    }

    static public void SetGameType_AdventureEvent(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.ADVENTURE_EVENT;
        SetGameSoundType(GameType.ADVENTURE_EVENT);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_AdventureEvent>();

        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
        Global.eventIndex = eventIndex;
    }

    static public void SetGameType_MoleCatch(int eventIndex, int chapterIndex, int stageIndex)
    {
        Global.GameType = GameType.MOLE_CATCH;
        SetGameSoundType(GameType.MOLE_CATCH);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_MoleCatch>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = chapterIndex;
        Global.stageIndex = stageIndex;
    }

    static public void SetGameType_CoinBonusStage(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.COIN_BONUS_STAGE;
        SetGameSoundType(GameType.COIN_BONUS_STAGE);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_CoinBonusStage>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }

    static public void SetGameType_WorldRanking(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.WORLD_RANK;
        SetGameSoundType(GameType.WORLD_RANK);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_WorldRank>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }

    static public void SetGameType_TurnRelay(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.TURN_RELAY;
        SetGameSoundType(GameType.TURN_RELAY);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_TurnRelay>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }
    
    static public void SetGameType_EndContents(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.END_CONTENTS;
        SetGameSoundType(GameType.END_CONTENTS);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_EndContents>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }
    
    static public void SetGameType_TreasureHunt(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.TREASURE_HUNT;
        SetGameSoundType(GameType.TREASURE_HUNT);
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_TreasureHunt>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }

    public static void SetGameType_BingoEvent(int stageIndex, int eventIndex)
    {
        Global.GameType = GameType.BINGO_EVENT;
        SetGameSoundType(GameType.BINGO_EVENT);
        if(Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_BingoEvent>();

        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex = stageIndex;
    }

    public static void SetGameType_SpaceTravel(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.SPACE_TRAVEL;
        SetGameSoundType(GameType.SPACE_TRAVEL);
        if (Global.GameInstance != null)
        {
            Destroy(Global.GameInstance);
        }

        Global.GameInstance = _instance.gameObject.AddComponent<GameType_SpaceTravel>();

        Global.eventIndex   = eventIndex;
        Global.chapterIndex = 0;
        Global.stageIndex   = stageIndex;
    }

    public static void SetGameType_Atelier(int eventIdx, int stageIdx)
    {
        GameType = GameType.ATELIER;
        SetGameSoundType(GameType.ATELIER);
        if (GameInstance != null)
        {
            Destroy(GameInstance);
        }

        GameInstance = _instance.gameObject.AddComponent<GameType_Atelier>();

        eventIndex   = eventIdx;
        chapterIndex = 0;
        stageIndex   = stageIdx;
    }

    static public void SetGameSoundType(GameType type)
    {
        Global.GameSoundType = type;
    }

    static public void UpdateNormalGameProgress(int stageIndex)
    {
        Global.stageIndex = stageIndex;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        myProfile.SetStage(stageIndex);
    }

    static public void SetStageIndex(int stageIndex)
    {
        Global.stageIndex = stageIndex;
    }

    static public void SetIsClear(bool isClear)
    {
        Global._isClear = isClear;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static public bool SkipStageComment()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.STAGE_REVIEW_ENABLED) == true && GameData.LoginCdn.skipStageComment == 0)
            return false;
        return true;
    }

    //유니티에서 이모티콘 표시할수없으므로 닉네임에 이모티콘 다른 기호로 치환 
    static public string ClipString(string in_name, int in_len = 0)
    {
        if (string.IsNullOrEmpty(in_name))
        {
            return in_name;
        }

        // '\' 문자 사용 금지
        in_name = in_name.Replace(@"\", string.Empty);

        // '"' 문자 사용 금지
        in_name = in_name.Replace("\"", "＂");
        in_name = in_name.Replace(@"&#x22", "＂");
        in_name = in_name.Replace(@"&#34", "＂");
        in_name = in_name.Replace(@"\34", "＂");

        string    filename = "emojiPattern";
        TextAsset textFile = Resources.Load(filename) as TextAsset;

        if (textFile != null)
        {
            string pattern = textFile.ToString();

            while (true)
            {
                var matches = Regex.Matches(in_name, pattern);

                if (matches.Count == 0) break;

                foreach (Match match in matches)
                {
                    in_name = in_name.Replace(match.Value, "□");
                }

                if (string.IsNullOrEmpty(in_name)) break;
            }
        }

        //utf32에서 포함되면 안되는 범위 : D800 ~ DFFF
        //highSurrogates : D800 ~ uDBFF
        //lowSurrogates : DC00 ~ DFFF
        Regex  surrogateCodepoint = new Regex("[\uD800-\uDFFF]");
        string output = surrogateCodepoint.Replace(in_name, "□");
        
        if (in_len > 0)
        {
            if (output.Length > in_len)
            {
                output =  output.Remove(in_len);
                output += "...";
            }
        }

        return output;
    }

    static public bool IsFilterClipString ( System.Action checkAction )
    {
        try
        {
            DelegateHelper.SafeCall( checkAction );
        }
        catch ( System.InvalidCastException e )
        {
            //Debug.LogError( " System.InvalidCastException e 들어옴 " );
            //Debug.LogError( e.Message );
 
            return true;
        }
        catch ( System.ArgumentException e )
        {
            //Debug.LogError( " System.ArgumentException e  들어옴 " );
            //Debug.LogError( e.Message );
            return true;
        }

        return false;
    }

    public bool HasString(string in_key) 
    {
        if (_stringData != null && _stringData.ContainsKey(in_key))
        {
            return true;
        }
        else if (LocalText.Instance.HasString(in_key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public string GetString(string in_key)
    {
        if (_stringData.TryGetValue(in_key, out string text))
        {
            return text;
        }
        else if (LocalText.Instance.TryGetString(in_key, out text))
        {
            return text;
        }
        else
        {
            return in_key + ": string empty";
        }
    }

    public void GetSystemErrorString ( int errorCode, string text, ref string retString, ref string okButtonString, ref bool closeBtn)
    {
        string errorCode16 = System.Convert.ToString( errorCode * -1, 16 );
        retString = text;
        
        switch ( errorCode16 )
        {
            case "f280" :
                //로그인에 실패했습니다. ID와 패스워드를 다시 확인해주세요.
                retString = $"{Global._instance.GetString("n_er_1")} : {errorCode16}\n{Global._instance.GetString("n_er_2")}";
                okButtonString = Global._instance.GetString("btn_72");
                break;
            case "f202" :
                //이 게임을 즐기기 위해서는 이용약관 동의가 필요합니다.
                retString = $"{Global._instance.GetString("n_er_1")} : {errorCode16}\n{Global._instance.GetString("n_er_3")}";
                okButtonString = Global._instance.GetString("btn_1");
                closeBtn = false;
                break;
            case "210" :
                //LINE 인증에 실패했습니다.
                retString = $"{Global._instance.GetString("n_er_1")} : {errorCode16}\n{Global._instance.GetString("n_er_4")}";
                okButtonString = Global._instance.GetString("btn_1");
                break;
            case "f281" :
                //인증 프로세스가 취소 됐습니다. 재 설치 후 다시 로그인 해주세요.
                retString = $"{Global._instance.GetString("n_er_1")} : {errorCode16}\n{Global._instance.GetString("n_er_5")}";
                okButtonString = Global._instance.GetString("btn_1");
                break;
            default :
                //어플리케이션을 재설치 후 실행해도 표시될 경우, 옵션 > 헬프에 에러 코드를 적어 문의해 주세요.
                retString = $"{Global._instance.GetString("n_er_1")} : {errorCode16}\n{Global._instance.GetString("n_er_6")}";
                okButtonString = Global._instance.GetString("btn_1");
                break;
        }
    }

    public string GetSystemNetworkErrorAndRebootString (int errorCode, string text)
    {
        //네트워크가 불안정하거나, 다른 이유로 문제가 발생하여 메인 화면으로 돌아갑니다.
        return $"{Global._instance.GetString("n_er_1")} : {errorCode}\n{Global._instance.GetString("n_er_7")}";
    }


    public string GetSystemErrorString ( string errorCode, string text )
    {
        //어플리케이션을 재설치 후 실행해도 표시될 경우, 옵션 > 헬프에 에러 코드를 적어 문의해 주세요.
        return $"{Global._instance.GetString("n_er_1")} : {errorCode}\n{Global._instance.GetString("n_er_6")}";
    }

    public static void LoadRankingPointGrade()
    {
        Global._instance._strRankingPointGradeData.Clear();
        TextAsset rankingPointGrade = Resources.Load("TextAsset/userGrade") as TextAsset;
        System.IO.StringReader stringReader = new System.IO.StringReader(rankingPointGrade.text);
        //stringReader.Read(); // skip BOM

        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        doc.LoadXml(stringReader.ReadToEnd());

        //최상위 노드 선택.
        System.Xml.XmlNode rootNode = doc.ChildNodes[1];
        for (int i = 0; i < rootNode.ChildNodes.Count; ++i)
        {
            System.Xml.XmlNode node = rootNode.ChildNodes[i];
            RankingPointGradeData data = new RankingPointGradeData();
            data.pointMin = xmlhelper.GetInt(node, "pointMin", 0);
            data.pointMax = xmlhelper.GetInt(node, "pointMax", 0);
            data.strKey = xmlhelper.GetString(node, "strKey", "");
            data.topColor = xmlhelper.GetHexColor(node, "topColor");
            data.bottomColor = xmlhelper.GetHexColor(node, "bottomColor");
            data.effectColor = xmlhelper.GetHexColor(node, "effectColor");
            data.colorTint = xmlhelper.GetHexColor(node, "colorTint");

            if (data != null)
            {
                Global._instance._strRankingPointGradeData.Add(data);
            }
        }
    }

    public void UpdateCloverCoolTime(string friendUserKey)
    {
        ServerAPI.GetCloverCoolTime(friendUserKey, (resp) =>
        {
            if (resp.IsSuccess)
            {
                if (ServerRepos.UserCloverCoolTimes.Find(fU => fU.fUserKey == friendUserKey) != null)
                {
                    ServerRepos.UserCloverCoolTimes.Find(fU => fU.fUserKey == friendUserKey).sendCoolTime = resp.cloverCoolTime;
                }
                else
                {
                    ServerRepos.SaveSendCloverCoolTime(new ServerUserSendCloverCoolTime()
                    {
                        fUserKey = friendUserKey, sendCoolTime = resp.cloverCoolTime, sendCount = 1
                    });
                }

                if (SDKGameProfileManager._instance.TryGetPlayingFriend(friendUserKey, out UserFriend data))
                {
                    data.CloverCoolTime = resp.cloverCoolTime;
                }
            }
        });
    }

    // 네트워크 에라등등에 따라 처음부터 시작
    public static void ReBoot () {
        if (_pendingReboot)
        {
            return;
        }

        _pendingReboot = true;

        UILobbyChat_Base.RemoveAll();

        ManagerEvent.OnReboot();
        ManagerAIAnimal.OnReboot();
        ManagerAreaAnimal.OnReboot();
        ManagerAdventure.OnReboot();
        AdManager.OnReboot();
        ManagerNotice.OnReboot();

        if ( ManagerData._instance != null )
        {
            
            Destroy( ManagerData._instance.gameObject );
            ManagerData._instance = null;
        }

        ManagerHousing.OnReboot();
        ManagerUIAtlas.OnReboot();
        ManagerResourceLoader.OnReboot();

        if (SceneTitle.instance != null)
        {
            SceneTitle.instance.DestroySceneTitle();
        }
        if ( ManagerGlobal._instance != null )
        {
            Destroy( ManagerGlobal._instance.gameObject );
            ManagerGlobal._instance = null;
        }
        if ( ManagerUI._instance != null )
        {
            ManagerUI._instance.DestroyPopupList(() =>
            {
               // Debug.Log( "============================ Count: " + ManagerUI._instance._popupList.Count + " ============================ " );
                
                if(ManagerUI._instance != null)
                    Destroy( ManagerUI._instance.gameObject );
                ManagerUI._instance = null;
                ManagerCinemaBox._instance = null;
                SceneTitle._showStartButton = true;
                //Debug.Log( "============================  Application.LoadLevel( \"Title\" ) ============================ " );
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Title");
            });   
        }
        ManagerLobby._firstNotice = true;
        ManagerLobby._firstLobby = true;
        ManagerLobby._firstLogin = true;

        ManagerLobby._stageClear    = false;
        ManagerLobby._stageTryCount = 0;
        ManagerLobby._newDay        = false;
        ManagerLobby.landIndex      = 0;
        ManagerLobby.transLand      = false;

        ManagerLobby._tutorialLobbyMission_Play  = false;
        ManagerLobby._tutorialDiaryMission_Play  = false;
        ManagerLobby._tutorialQuestComplete_Play = false;
        ManagerLobby.oldStage                    = -1;
        
        ManagerLobby.InvalidateBundles();
        ManagerAssetLoader.UnloadBundles();
        HashChecker.OnReboot();

        //IronSource -> Applovin Max 교체 작업
        //IronSourceManager.OnReboot();
        
        Global._instance.invalidateString = true;
        
        Global.SetGameTypeStateToDefault();
        
        UIPopupReady.dicEventStageFlower.Clear();

        //timeScale(스킵 시 속도 조절할 때 사용) 원래대로 돌려줌.
        Global.timeScaleLobby = 1f;
        Global.timeScaleLobbySpine = 1f;
        Global.timeScalePuzzle = 1f;

        LocalText.Instance.Init();
        Global._instance._stringData.Clear();

        UIPopupReady.eventStageFail = false; //네트워크 에러로 Reboot 발생 시 연속모드 광고 데이터 초기화를 위해 추가.

        // 포코유라 데이터 초기화.
        if (Pokoyura.pokoyuraDeployCustom != null)
        {
            Pokoyura.pokoyuraDeployCustom.Clear();
            Pokoyura.pokoyuraDeployCustom = null;
        }
        UIPopupPokoyuraSelector.prevPokoyuraList.Clear();
    }

  
#if UNITY_EDITOR || UNITY_ANDROID
    ///////////////////////////////////////////
    // Log
    static List<string> logLines = new List<string>();
    static public void Log(string text)
    {
        if (logLines.Count > 20)
            logLines.RemoveAt(0);

        logLines.Add(text);
    }
    void OnGUI()
    {
        if (logLines.Count > 0)
        {
            GUI.color = Color.red;
            for (int i = 0, imax = logLines.Count; i < imax; ++i)
            {
                GUILayout.Label(logLines[i]);
            }
        }
    }
#else
    static public void Log(string text) { }
#endif
    static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        StringBuilder sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length - 1; i++)
        {
            sOutput.Append(arrInput[i].ToString("X2"));
        }
        return sOutput.ToString();
    }
    static MD5CryptoServiceProvider _hash = new MD5CryptoServiceProvider();
    static public string GetHashfromText(string in_text)
    {
        byte[] tmpSource = ASCIIEncoding.ASCII.GetBytes(in_text);
        byte[] tmpHash = _hash.ComputeHash(tmpSource);

        return ByteArrayToString(tmpHash);
    }

    public static CdnAdReadyItemInfo GetAdReadyItemInfo()
    {
        if( ServerContents.AdReadyItemInfos == null || ServerContents.AdReadyItemInfos.Count == 0 )
            return null;

        var nowTs = ServerRepos.GetServerTime();
        //var nowDate = ConvertFromUnixTimestamp(nowTs);
        int pu = ServerRepos.IsPurchasedUser() ? 1 : 0;

        int flowerCount = 0;
        int clearCount = 0;
        for (int i = 0; i < ManagerData._instance._stageData.Count; ++i)
        {
            if (ManagerData._instance._stageData[i]._flowerLevel < 3)
            {
                flowerCount += ManagerData._instance._stageData[i]._flowerLevel < 3 ? 0 : ManagerData._instance._stageData[i]._flowerLevel - 3;
            }
            if (ManagerData._instance._stageData[i]._flowerLevel != 0)
                clearCount++;
        }

        float clearRate = (float)clearCount / (float)ManagerData._instance._stageData.Count;
        int stageSegmentIdx = -1;
        if (clearCount >= ManagerData._instance._stageData.Count)
            stageSegmentIdx = 7;
        else if (GameManager.NowFinalChapter())
            stageSegmentIdx = 6;
        else if (clearCount <= 50)
            stageSegmentIdx = 0;
        else if (clearRate < 0.25f)
            stageSegmentIdx = 1;
        else if (clearRate < 0.50f)
            stageSegmentIdx = 2;
        else if (clearRate < 0.75f)
            stageSegmentIdx = 3;
        else if (clearRate < 0.90f)
            stageSegmentIdx = 4;
        else
            stageSegmentIdx = 5;

        var ret = ServerContents.AdReadyItemInfos.Find(x => x.segmentId == stageSegmentIdx && x.puFlag == pu  );
        return ret;
    }

    public static int? GetIntByPlayerPrefs(string key)
    {
        if (PlayerPrefs.HasKey(key) == true)
            return PlayerPrefs.GetInt(key);
        else
            return null;
    }
    
    public static string GetStringByPlayerPrefs(string key)
    {
        if (PlayerPrefs.HasKey(key) == true)
            return PlayerPrefs.GetString(key);
        else
            return null;
    }
    
    public static string GetPIONUrlHead()
    {
        switch (NetworkSettings.Instance.GetBuildPhase())
        {
            case NetworkSettings.eBuildPhases.SANDBOX:
                return "https://game-api-sandbox.line.me/";
            case NetworkSettings.eBuildPhases.STAGING:
                return "https://game-api-staging.line.me/";
            case NetworkSettings.eBuildPhases.RELEASE:
                return "https://game-api.line.me/";
            default:
                return string.Empty;
        }
    }

    #region 시간관련
    static public long serverTime = 0;
    static long serverTime_updataTime = 0;

    static public long GetTime()
    {
        return ServerRepos.GetServerTime();
        //long delta = (System.DateTime.Now.Ticks - serverTime_updataTime) / 10000000;
        //return delta + serverTime;
    }

    static public long LeftTime(long in_time)
    {
        return in_time - GetTime();
    }

    static public bool IsDayPassed(long checkTime)
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        System.DateTime rebootTime = System.DateTime.Today.AddHours(ServerRepos.LoginCdn.LoginOffset);
        rebootTime = rebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);

        System.TimeSpan diff = rebootTime - origin;
        double time = System.Math.Floor(diff.TotalSeconds);

        //현재 시간이 갱신되는 시점 이후이고, 로그인은 갱신시간 이전에 한 경우 reboot 시켜줌.
        if (Global.GetTime() >= time && (checkTime < time ))
        {
            return true;
        }
        return false;
    }

    static public long GetSecondsToNextDay()
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        System.DateTime rebootTime = System.DateTime.Today.AddHours(ServerRepos.LoginCdn.LoginOffset);
        rebootTime = rebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);
        rebootTime = rebootTime.AddDays(1);

        System.TimeSpan diff = rebootTime - origin;
        double time = System.Math.Floor(diff.TotalSeconds);

        return (long)time - GetTime();
    }

    // 서버와 시간 동기화가 필요.
   /* static public void ResetTime(long in_time)
    {
        serverTime = in_time;
        serverTime_updataTime = System.DateTime.Now.Ticks;
    }*/

    static public string GetTimeText_DDHHMM(long in_time, bool bLeftTime = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = LeftTime(t);
        }
        if (t < 0)
            t = 0;

        string text = "";

        int day = (int)(t / (3600 * 24));
        if (day > 0)
        {
            text += $"{day}{Global._instance.GetString("time_1")} ";
        }

        text += string.Format("{0:D2}", (int)((t / 3600) % 24)) + ": ";//"時間 ";
        text += string.Format("{0:D2}", (int)((t / 60) % 60)) + ": ";//"分 ";
        text += string.Format("{0:D2}", (int)(t % 60));//+ "秒";
        return text;
    }
    
    public static string GetTimeText_YYMMDDHHMM(long in_time, string key = "p_lb_2")
    {
        const string DATE_FORMAT = "yy.MM.dd HH:mm";
        var date = ConvertFromUnixTimestamp(in_time);
        return Global._instance.GetString(key)
            .Replace("[0]", date.ToString(DATE_FORMAT));; 
    }
    
    public static string GetTimeText_MMDDHHMM(long in_time)
    {
        const string DATE_FORMAT = "MM.dd HH:mm";
        var          date        = ConvertFromUnixTimestamp(in_time);
        return Global._instance.GetString("time_5")
                     .Replace("[0]", date.ToString(DATE_FORMAT));; 
    }
    
    //남은 시간에 +1초를 하여 59분으로 끝나지 않고 00분으로 끝나게 함
    public static string GetTimeText_MMDDHHMM_Plus1(long in_time)
    {
        in_time += 1;
        const string DATE_FORMAT = "MM.dd HH:mm";
        var          date        = ConvertFromUnixTimestamp(in_time);
        return Global._instance.GetString("time_5")
            .Replace("[0]", date.ToString(DATE_FORMAT));; 
    }
    
    public static string GetCloverFreeTimeText(int leftTime)
    {
        if (leftTime < 0)
            leftTime = 0;

        string text = "";
        if (leftTime / 60 > 0)
            text = string.Format("{0:D2}", leftTime / 60) + ":";
        else
            text = "00:";
        text += string.Format("{0:D2}", leftTime % 60);

        return text;
    }
    public static string GetCloverTimeText(int leftTime)
     {
         if (!Global._onlineMode)
             return "-- : --";
         if (Global.clover >= 5)
             return "FULL";

     //    int leftTime = Global.LeftTime(Global._cloverTimer);
         if (leftTime < 0)
             leftTime = 0;

         string text = "";
         if (leftTime / 60 > 0)
             text = string.Format("{0:D2}", leftTime / 60) + ":";
         else
             text = "00:";
         text += string.Format("{0:D2}", leftTime % 60);

         return text;
     }
    /*  public static string GetCloverTimeText(int in_time)
      {
          if (!Global._onlineMode)
              return "00:00";

          int t = in_time;///60;
          ///
          if (t < 0)
              t = 0;
  //        string text = string.Format("{0:D2}", (int)(t / 3600));
          string text = ((int)(t / 3600)).ToString();
          text += ":" + string.Format("{0:D2}", (int)((t / 60) % 60));
          text += ":" + string.Format("{0:D2}", (int)(t % 60));
          return text;
      }*/

    static public string GetLeftTimeText(long in_time)
    {
        long t = LeftTime(in_time);

        if (t < 0)
            t = 0;

        if( t > 3600*24)
        {
            int day = ((int)(t / (3600 * 24)));
            string text = null;
            text += day.ToString() + (day > 1 ? "Days " : "Day");
            return text;
        }
        else
        {
            string text = null;
            text = string.Format("{0:D2}", (int)(t / 3600));
            text += ":" + string.Format("{0:D2}", (int)((t / 60) % 60));
            text += ":" + string.Format("{0:D2}", (int)(t % 60));
            return text;
        }
    }
    
    public static string GetLeftTimeText_DDHHMMSS(long inTime)
    {
        var t = LeftTime(inTime);

        if (t < 0)
        {
            t = 0;
        }
    
        if (t >= 3600 * 24) // 1일 이상 : n일 n시간
        {
            var text = (int) (t / (3600 * 24)) + _instance.GetString("p_lob_c_4");
            text += string.Format("{0:D2}",(int) ((t / 3600) % 24)) + _instance.GetString("time_6");
            return text;
        }
        if (t >= 3600) // 1일 미만 1시간 이상 : n시간 n분
        {
            var text =  string.Format("{0:D2}", (int) (t / 3600)) + _instance.GetString("time_6");
            text += string.Format("{0:D2}", (int) ((t / 60) % 60)) + _instance.GetString("p_lob_c_6");
            return text;
        }
        else // 1시간 미만 : n분:n초
        {
            var text = string.Format("{0:D2}", (int) ((t / 60) % 60));
            text += ":" + string.Format("{0:D2}", (int) (t % 60));
            return text;
        }
    }

    static public string GetTimeText_HHMMSS(long in_time, bool bLeftTime = true, bool in_MiddleText = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = LeftTime(t);
        }

        if (t < 0)
            t = 0;
        string text = null;
        text = string.Format("{0:D2}", (int)(t / 3600));
        if (in_MiddleText)
            text += ":" + string.Format("{0:D2}", (int)((t / 60) % 60));
        else
            text += " " + string.Format("{0:D2}", (int)((t / 60) % 60));
        text += ":" + string.Format("{0:D2}", (int)(t % 60));
        return text;
    }

    static public string GetTimeText_HHMM(long in_time, bool bLeftTime = true,bool in_MiddleText = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = LeftTime(t);
        }

        if (t < 0)
            t = 0;
        string text = null;
        text = string.Format("{0:D2}", (int)(t / 3600));
        if (in_MiddleText)
            text += ":" + string.Format("{0:D2}", (int)((t / 60) % 60));
        else
            text += " " + string.Format("{0:D2}", (int)((t / 60) % 60));

        return text;
    }

    static public string GetTimeText_MMSS(long in_time, bool bLeftTime = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = LeftTime(t);
        }

        if (t < 0)
            t = 0;

        string text = " " + string.Format("{0:D2}", (int)(t / 60));
        text += ":" + string.Format("{0:D2}", (int)(t % 60));
        return text;
    }

    static public string GetTimeText_SS(long in_time, bool bLeftTime = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = LeftTime(t);
        }

        if (t < 0)
            t = 0;            
        string text = null;

        if (t < 10)
            text = string.Format("{0:D2}", "0" + (int)t + Global._instance.GetString("time_4"));
        else
            text = string.Format("{0:D2}", (int)t + Global._instance.GetString("time_4"));
        
        return text;
    }

    //뷴 -> 시 -> 일 표시
    //현재 시간에서 얼마나 지났는지 표시
    static public string GetTimeText_UpdateTime(long in_time)
    {
        long time = GetTime() - in_time;
        string text;

        if (time > 24 * 60 * 60) //##일 
        {
            text = _instance.GetString("p_gf_10");

            int tempDay = ((int)(time / (24 * 60 * 60)));

            text = text.Replace("[n]", tempDay.ToString());
        }
        else if(time > 60 * 60) //##시
        {
            text = _instance.GetString("p_gf_9");

            int tempHour = ((int)(time / (60 * 60)));

            text = text.Replace("[n]", tempHour.ToString());
        }
        else //분
        {
            text = _instance.GetString("p_gf_8");

            int tempMinute = Mathf.Max((int)(time / (60 * 60)), 1);

            text = text.Replace("[n]", tempMinute.ToString());
        }

        return text;
    }

    static private System.DateTime ConvertFromUnixTimestamp(long timestamp)
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        origin = origin.AddHours(9);
        return origin.AddSeconds(timestamp);
    }

    static public string GetTimeText_Abs(long timestamp, bool includeTime = false)
    {
        return GetTimeText_Abs(ConvertFromUnixTimestamp(timestamp), includeTime);

    }

    static public string GetTimeText_Abs(System.DateTime date, bool includeTime = false)
    {
        string text = string.Format("{0}.{1:00}.{2:00}", date.Year, date.Month, date.Day);
        if( includeTime )
            text += string.Format(" {0:00}:{1:00}", date.Hour, date.Minute);

        return text;
    }
    #endregion

    #region 재화 소비 관련(페이 머니, 프리 머니 구분)
    public struct UseMoneyData
    {
        public int usePMoney;
        public int useFMoney;
    }

    private UseMoneyData UseMoney(int price, int money)
    {
        var useJewelData = new UseMoneyData();

        if (money >= price)
        {
            useJewelData.usePMoney = price;
            useJewelData.useFMoney = 0;
        }
        else if (money > 0)
        {
            useJewelData.usePMoney = money;
            useJewelData.useFMoney = price - money;
        }
        else
        {
            useJewelData.usePMoney = 0;
            useJewelData.useFMoney = price;
        }

        return useJewelData;
    }

    public UseMoneyData UseJewel(int price)
    {
        return UseMoney(price, (int)ServerRepos.User.jewel);
    }

    public UseMoneyData UseCoin(int price)
    {
        return UseMoney(price, (int)ServerRepos.User.coin);
    }

    //price type을 가져오는 방식이 달라서 RewardType을 사용하는 구매 타입만 PackagePriceType으로 변경
    public PackagePriceType ChangeTypeFromRewardType(RewardType rewardType)
    {
        switch (rewardType)
        {
            case RewardType.none:
                return PackagePriceType.Cash;
            case RewardType.coin:
                return PackagePriceType.Coin;
            case RewardType.jewel:
                return PackagePriceType.Jewel;
        }

        return PackagePriceType.None;
    }
    
    #endregion

    #region 레디 아이템 관련
    /// <summary>
    /// 레디 아이템의 총 카운트를 반환하는 함수
    /// </summary>
    public int GetReadyItemCount()
    {
        return System.Enum.GetNames(typeof(READY_ITEM_TYPE)).Length -1;
    }

    /// <summary>
    /// 해당 아이템을 더블 레디 아이템으로 변환했을 때 타입 반환
    /// </summary>
    public READY_ITEM_TYPE GetDoubleReadyItemType(READY_ITEM_TYPE readyItemType)
    {
        switch (readyItemType)
        {
            case READY_ITEM_TYPE.ADD_TURN:
                return READY_ITEM_TYPE.DOUBLE_ADD_TURN;
            case READY_ITEM_TYPE.SCORE_UP:
                return READY_ITEM_TYPE.DOUBLE_SCORE_UP;
        }
        return READY_ITEM_TYPE.LOCK;
    }
    
    /// <summary>
    /// 더블 레디 아이템일 일반 아이템으로 변환했을 때 타입 반환
    /// </summary>
    public READY_ITEM_TYPE GetSingleReadyItemType(READY_ITEM_TYPE readyItemType)
    {
        switch (readyItemType)
        {
            case READY_ITEM_TYPE.DOUBLE_ADD_TURN:
                return READY_ITEM_TYPE.ADD_TURN;
            case READY_ITEM_TYPE.DOUBLE_SCORE_UP:
                return READY_ITEM_TYPE.SCORE_UP;
        }
        return READY_ITEM_TYPE.LOCK;
    }
    
    /// <summary>
    /// 더블 레디 아이템으로 변환 가능한지 검사
    /// </summary>
    public bool CheckReadyItemType_CanChangeDouble(READY_ITEM_TYPE readyItemType)
    {
        return (GetDoubleReadyItemType(readyItemType) != READY_ITEM_TYPE.LOCK);
    }
    
    /// <summary>
    /// 해당 아이템이 더블 레디 아이템인지 검사
    /// </summary>
    public bool CheckReadyItemType_DoubleItem(READY_ITEM_TYPE readyItemType)
    {
        if (readyItemType == READY_ITEM_TYPE.DOUBLE_ADD_TURN
            || readyItemType == READY_ITEM_TYPE.DOUBLE_SCORE_UP)
            return true;

        return false;
    }
    #endregion
}
