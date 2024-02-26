using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public enum GameType
{
	NORMAL = 0,
	EVENT,
	RANK,
    ADVENTURE,
    WORLD_RANK,
    END_CONTENTS,
}

public enum CurrLang
{
    eJap,   //일본
    eEng,   // 영어버전
    //eKor,
};

public class Global : MonoBehaviour
{
    public static Global _instance = null;

    static public bool _debugEdit = true;
    static public bool _debugLog = true;
    static public bool _pendingReboot = false;
    static public float _debugNetworkDelay = 0.0f;
    
    static public string _appVersion = "1.0.3";
    static public string _countryCode = "US";
    static public CurrLang _systemLanguage = CurrLang.eJap;
    static public string _marketUrl = "https://play.google.com/store/apps/details?id=com.linecorp.LGPKPK";
    static public bool _optionBGM = true;
    static public bool _optionSoundEffect = true;     //BGM
    static public bool _optionGameNotice = true;
    static public bool _optionLocalPush = true;
    //static public string _gameServerAddress = "http://110.46.209.133:5886/pokopoko/";   // 사내주소 고정... 알파랑 리얼은 라인로그인시 받아서 설정
    //static public string _cdnAddress = "http://110.46.209.133:5885/www/pokopoko/cdn/";
    static public string _cdnAddress        = "https://lgpkv-sandbox-cdn.lt.treenod.com/cdn/";
    static public string _marketAppLinkAddr = "market://details?id=com.linecorp.LGPKV";
    //static public string _timeLineAddress = "http://down.hangame.co.jp/jp-smp/dist/SJLGPP/timeline/";

    // 메모리 해킹을 대비,EncValue,,하나의 함수내에서 읽고 쓸때 유의, 함수에서 읽어서 사용할때 for나 while을 돌때는 별도의 함수내 변수에 읽어서 사용하기...int coin = Global.coin;  while(){ coin++; };
    static public bool _touch = true;

    static private bool _enableOnlineMode = true;
    static public bool _onlineMode = true;             // Online, Offline (정상 플리에와 캐시된 데이터로 오프라인 모드) 
    static public bool _rebootWithAuth = false;        // Line, Server등 인증 관련 오류시 true => 로그인 버튼 유도
    
    static public bool isChangeUserData = false;

    public bool ForceLoadBundle = false;
    static public string FileUri { get {
#if UNITY_2017_1_OR_NEWER
            return "file://";
#else
            return "file:///";
#endif
         }
    } 
    // user data
    static private EncValue _coin = new EncValue();
    static public int coin
    {
        get { return _coin.Value; }
        set { _coin.Value = value; }
    }
    static private EncValue _jewel = new EncValue();
    static public int jewel
    {
        get { return _jewel.Value; }
        set { _jewel.Value = value; }
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
        set { _wing.Value = value; }
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
        get { if (GameType == GameType.RANK) return 0; return _stageIndex.Value; }
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


    static private EncValue _gameType = new EncValue((int)GameType.NORMAL);
    static public GameType GameType
    {
        get { return (GameType)_gameType.Value; }
        private set { _gameType.Value = (int)value; }
    }


    // 각종 로컬 상태
    static public bool join = false;    // 첫가입 유저

    [System.NonSerialized]
    public Dictionary<string, string> _stringData = new Dictionary<string, string>();
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    //  
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    // 시간을 나눈 이유는 예를 들어 팝업이 뜨면 진행하던 요소들이 일시정지 시키거나 할때
    static public float timeScale = 1f; 
    static public float deltaTime = 0f;     // 각종 시스템과 UI등에 사용
    static public float deltaTimeNoScale = 0f;

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
    static public string adventureDirectory = "Adventure/";             // 모험모드 이미지 (동물, 몬스터, 동물아이콘)
    static public string gameDataDirectory = "/GameData/";        // 번들 게임 데이타
    static public string gameImageDirectory = "/GameImage/";             // 미션,퀘스트,재료 등등 게임 이미지
    static public string noticeDirectory = "/Notice/";                   // 공지 팝업 이미지
    static public string movieDirectory = "/movie/";                   // 공지 팝업 이미지
    //static public string thumbnailDirectory = "/Thumbnail/";             // 프로필 썸네일 이미지     
    static public string StageDirectory = "/Stage/";             // 프로필 썸네일 이미지     

    //인게임 클리어버튼
    public bool showInGameClearBTN = false;

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
    }
	// Use this for initialization
	void Start () {


        Application.targetFrameRate = 60;
        
        
        // 로비에서 시스템.  로딩할땐 안꺼지게.. 인트로도...
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Screen.sleepTimeout = SleepTimeout.SystemSetting;

        _optionBGM = PlayerPrefs.GetInt("_optionBGM", 1) == 1;
        _optionSoundEffect = PlayerPrefs.GetInt("_optionSoundEffect", 1) == 1;
        _optionLocalPush = PlayerPrefs.GetInt("_optionLocalPush", 1) == 1;

        if (PlayerPrefs.HasKey("_systemLanguage"))
        {
            _systemLanguage = (CurrLang)(PlayerPrefs.GetInt("_systemLanguage"));
        }
        else
        {
            // 유니티에서 개발할때는 일본어가 초기 설정이 되도록
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                _systemLanguage = CurrLang.eJap;
            }
            /*else if (NetworkSettings.Instance.serverTarget != NetworkSettings.ServerTargets.DevServer)
            {
                _systemLanguage = CurrLang.eJap;
            }*/
            else
            {
                if (Application.systemLanguage == SystemLanguage.Japanese)
                    _systemLanguage = CurrLang.eJap;
                else
                    _systemLanguage = CurrLang.eEng;
            }
            PlayerPrefs.SetInt("_systemLanguage", (int)Global._systemLanguage);
        }

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
/*

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static public void SetGameType_Event(int eventIndex, int stageIndex)
    {
        Global.GameType = GameType.EVENT;
        Global.stageIndex = stageIndex;
        Global.eventIndex = eventIndex;
        Global.chapterIndex = 0;
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Event>();
    }

    static public void SetGameType_Ranking(int rankingIdx)
    {
        Global.GameType = GameType.RANK;
        Global.stageIndex = 0;
        Global.eventIndex = rankingIdx;
        Global.chapterIndex = 0;
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Rank>();
    }

    static public void SetGameType_NormalGame(int stageIndex = 0)
    {
        Global.chapterIndex = 0;
        Global.GameType = GameType.NORMAL;
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Normal>();
        if ( stageIndex == 0 )
        {
            stageIndex = ManagerData._instance.userData.stage > ManagerData._instance.maxStageCount ? (int)ManagerData._instance.maxStageCount : (int)ManagerData._instance.userData.stage;
        }

        Global.stageIndex = stageIndex;
        Global.eventIndex = 0;
    }

    static public void SetGameType_Adventure(int chapterIndex, int stageIndex)
    {
        Global.GameType = GameType.ADVENTURE;
        if (Global.GameInstance != null)
            Destroy(Global.GameInstance);
        Global.GameInstance = _instance.gameObject.AddComponent<GameType_Adventure>();

        Global.chapterIndex = chapterIndex;
        Global.stageIndex = stageIndex;
        Global.eventIndex = 0;
    }

    static public void UpdateNormalGameProgress(int stageIndex)
    {
        Global.stageIndex = stageIndex;
        ManagerData._instance.userData.stage = stageIndex;
    }

    static public void SetStageIndex(int stageIndex)
    {
        Global.stageIndex = stageIndex;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static public bool SkipStageComment()
    {
        if (Global.GameInstance.OpenReview() == true && GameData.LoginCdn.skipStageComment == 0)
            return false;
        return true;
    }

*/
    //유니티에서 이모티콘 표시할수없으므로 닉네임에 이모티콘 다른 기호로 치환 
    static public string ClipString(string in_name, int in_len = 0)
    {
        Regex rg = new Regex("[\ud83c-\udfff]");
        Regex rg2 = new Regex("[\u25a1-\u26F0]");

        string output = rg.Replace(in_name, "□");
        output = rg2.Replace(output, "□");

        if (in_len > 0)
        {
            if (output.Length > in_len)
            {
                output = output.Remove(in_len);
                output += "...";
            }
        }

        return output;
    }

    static public bool IsFilterClipString ( System.Action checkAction )
    {
        try
        {
           // DelegateHelper.SafeCall( checkAction );
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

    public bool HasString(string in_key) {
        return _stringData != null ? _stringData.ContainsKey(in_key) : false;
    }
    public string GetString(string in_key)
    {
        if (_stringData.ContainsKey(in_key))
            return _stringData[in_key];
        else
            return in_key + ": string empty";
    }

    public string GetSystemErrorString ( int errorCode, string text )
    {
        //bool isJpn = (Application.systemLanguage == SystemLanguage.Japanese) ? true : false;
        string errorCode16 = System.Convert.ToString( errorCode * -1, 16 );
        string errorText = text;
        switch ( errorCode16 )
        {
            case "f280" :
                errorText = string.Concat( "ログインに失敗しました。\nID&パスワードを再度ご確認\nください。", "\n", "エラーコード： -", errorCode16 );
                break;
            case "f202" :
                errorText = string.Concat( "利用規約に同意してください。", "\n", "エラーコード： -", errorCode16 );
                break;
            case "210" :
                errorText = string.Concat( "LINE認証に失敗しました。", "\n", "エラーコード： -", errorCode16 );
                break;
            case "f281" :
                errorText = string.Concat( "認証プロセスがキャンセル\nされました。再インストール後\n再度ログインしてください。", "\n", "エラーコード： -", errorCode16 );
                break;
            default :
                errorText = string.Concat( "エラーコード： ", errorCode16, "\n\n",
                "アプリを再インストール、再起動しても表示される場合は、オプション>ヘルプからエラーコードを記載してお問い合わせください" );
                break;
        }

        return errorText;
    }

    public string GetSystemNetworkErrorAndRebootString (int errorCode, string text)
    {
        //bool isJpn = (Application.systemLanguage == SystemLanguage.Japanese) ? true : false;

        string message = "ネットワーク環境が不安定か、その他の理由で問題があったためメイン画面に戻ります。";
        string errorText = string.Concat(message, "\n", "エラーコード： ", errorCode.ToString());

        return errorText;
    }


    public string GetSystemErrorString ( string errorCode, string text )
    {
        //bool isJpn = (Application.systemLanguage == SystemLanguage.Japanese) ? true : false;

        return string.Concat( "エラーコード： -", errorCode, "\n\n",
            "アプリを再インストール、再起動しても表示される場合は、オプション>ヘルプからエラーコードを記載してお問い合わせください" );
    }

   
        
   
// 네트워크 에라등등에 따라 처음부터 시작
    public static void ReBoot () {
        _pendingReboot = true;

        if ( ManagerData._instance != null )
        {
            
            Destroy( ManagerData._instance.gameObject );
            ManagerData._instance = null;
        }
        if (SceneTitle.instance != null)
        {
            SceneTitle.instance.DestroySeneTitle();
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
                
                Destroy( ManagerUI._instance.gameObject );
                ManagerUI._instance = null;
                ManagerCinemaBox._instance = null;
                SceneTitle._showStartButton = true;
                //Debug.Log( "============================  Application.LoadLevel( \"Title\" ) ============================ " );
                Application.LoadLevel( "Title" );
            });   
        }
        ManagerLobby._firstNotice = true;
        ManagerLobby._firstLobby = true;

        ManagerLobby._stageClear = false;
        ManagerLobby._eventStageClear = false;
        ManagerLobby._stageTryCount = 0;
        ManagerLobby._newDay = false;
        ManagerLobby._tutorialLobbyMission_Play = false;
        ManagerLobby._tutorialDiaryMission_Play = false;
        ManagerLobby._tutorialQuestComplete_Play = false;
        ManagerAssetLoader.assetDataList.Clear();

        UIPopupReady.eventStageClear = false;
        UIPopupReady.eventGroupClear = false;
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

    #region 시간관련
    static public long serverTime = 0;
    static long serverTime_updataTime = 0;

    static public long GetTime()
    {
        return 0;// ServerRepos.GetServerTime();
        //long delta = (System.DateTime.Now.Ticks - serverTime_updataTime) / 10000000;
        //return delta + serverTime;
    }

    static public long LeftTime(long in_time)
    {
        return in_time - GetTime();
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
            if (Global._systemLanguage == CurrLang.eJap)
                text += day + "日 ";
            else
                text += day + "DAY";
        }

        text += string.Format("{0:D2}", (int)((t / 3600) % 24)) + ": ";//"時間 ";
        text += string.Format("{0:D2}", (int)((t / 60) % 60)) + ": ";//"分 ";
        text += string.Format("{0:D2}", (int)(t % 60));//+ "秒";
        return text;
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
        if (_systemLanguage == CurrLang.eJap)
        {
            if (t < 10)
                text = string.Format("{0:D2}", "0" + (int)t + "秒");
            else
                text = string.Format("{0:D2}", (int)t + "秒");
        }
        else
        {
            if (t < 10)
                text = string.Format("{0:D2}", "0" + (int)t + " S");
            else
                text = string.Format("{0:D2}", (int)t + " S");
        }
        
        return text;
    }
    #endregion
}
