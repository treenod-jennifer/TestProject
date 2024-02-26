using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum TypeLobbyState
{
    None,
    Preparing,              //  로비 준비중                  
    PreparingEnd,           //  로비 준비 완료

    StartEvent,             //  앱 기동 이벤트(세일,이벤트 스테이지등등 초반 앱 기동시 강제 이벤트)

    Wait,                   //  대기 상태, 보니를 조정하거나 보니가 스스로 움직이는 상태(최소 가능)
    TriggerEvent,    //  미션을 받거나 미션 완료 수행중 (취소 불가)
    NewDayEvent,            //  새로운 날 이벤트
    
    LoadingNextScene,
}

[System.Serializable]
public class SpawnPosition
{
    public Vector3 position = Vector3.zero;
    public Color color = Color.white;
    //public float radius = 2f;

    [System.NonSerialized]
    public bool used = false;
}

public class ReservedScene
{
    public Dictionary<int, MissionData> missionBackup;

    public int scnStartDay;
    public int scnDestDay;
    public CdnDay orgDay;
    public CdnDay newDay;
    public int areaIndex;
    public int sceneIndex;
    public int missionIndex;
}

public class ManagerLobby : MonoBehaviour
{
    public const string LOBBY_TUTORIAL_KEY = "LobbyRenewalTutorial";
    
    public static ManagerLobby _instance = null;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Character
    public List<GameObject> _objCharacter;

    public GameObject _objCharacterNPC;

    //[System.NonSerialized]
    public TypeLobbyState _state = TypeLobbyState.None;

    public bool IsLobbyComplete { get; private set; } = false;
    public bool IsLoabbyActionState = false;

    static public int _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen = 6;    // 공지와 팩키지 상품, 스페셜 상품, 이벤트 스테이지는 미션이 6스테이지 이후 일때 노출
    static public int _missionThreshold_eventHousingOpen = 8;    // 이벤트 타입의 하우징은 미션진행수가 8 이상 일때 노출.
    static public int _missionThreshold_firstDayOver = 9;    // 첫날 지나고나서의 기준

    static public bool _loadComplete = false;
    static public bool _firstLobby = true;
    static public bool _firstNotice = true;
    static public bool _firstLogin = true;
    static public bool _stageClear = false;
    static public int _stageTryCount = 0;
    static public bool _stageStart = false;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Object
    public AnimationCurve objectTouchAnimationCurve;
    public GameObject _objMissionIcon;
    public GameObject _objHousingIcon;
    public GameObject _objRewardIcon;
    public GameObject _objEventObjectUI;    

    public List<GameObject> _objCharacterMotionblur;
    public AnimationCurve objectRideCurveJump;
    public AnimationCurve objectRideCurveMove;
    public AnimationCurve motionblurCurveMove;
    public List<AnimationCurve> motionblurCurveFrameList = new List<AnimationCurve>();

    public ObjectMaterial _objMaterial;
    public ObjectGiftbox _objGiftBox;
    public ObjectMaterialbox _objMaterialBox;
    public Pokoyura _objPokoyura;
    public Pokogoro _objPokogoro = null;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public SpawnPosition[] _spawnMaterialPosition = null;
    public SpawnPosition[] _spawnGiftBoxPosition = null;
    public SpawnPosition[] _spawnPokogoroPosition = null;
    public SpawnPosition[] _spawnMaterialBoxPosition = null;
    public ObjectHousingIcon _housingIcon;
 //   public SpawnMaterialPosition[] _spawnMaterialPosition = null;

    [System.NonSerialized]
    private Dictionary<int, Character> _characterList = new Dictionary<int, Character>();
    public static bool          _newDay   = false;
    public static bool          transLand = false; // INCIDENT-1221 : 옵션 팝업에서 인트로를 재생 하고 로비 씬이 다시 로드될 때 리뷰 유도 팝업이 출력되는 이슈 수정.
    public static bool          showIntro              = false;
    public static ReservedScene _reservedScene         = null;
    public static System.Action _reservedHoisingChange = null;
    public static System.Action _reservedPostAction    = null;

    public static Dictionary<string, GameObject> _assetBankHousing = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> _assetBankEvent = new Dictionary<string, GameObject>();

    [System.NonSerialized]
    public int lateClearMission = -1;

    // 네비게이트 관련
    [System.NonSerialized]
    public Vector3 _homeCameraPosition = new Vector3(-10.0f, 0f, -7.7f);
    [System.NonSerialized]
    public Vector3 _workCameraPosition = new Vector3(-10.0f, 0f, -7.7f);

    public List<ParticleSystem> _staticEffectObj = new List<ParticleSystem>();
    public List<AnimationClip> _staticAnimationObj = new List<AnimationClip>();

    // 튜토리얼 관련
    static public bool _tutorialLobbyMission_Play = false;
    static public bool _tutorialDiaryMission_Play = false;
    static public bool _tutorialQuestComplete_Play = false;
   
    public static int _activeGetPokoyura = 0; // 포코유라 획득 연출.
    public static int oldStage           = -1;// 미션 1 완료 튜토리얼 체크 시 사용하는 데이터.

    //현재 로비 npc가 접근할 수 있는 에리어 인덱스
    [System.NonSerialized]
    public List<int> listOpenAreaIndex = new List<int>();

    //트리거에서 사용하고 있는 동물들 데이터
    public List<UseCharacterData_AtScene> listUseCharRegister = new List<UseCharacterData_AtScene>();
    
    //액션
    public event System.Action OnEventHighlight;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    void Awake()
    {
        _instance = this;
        _loadComplete = false;
        _state = TypeLobbyState.None;
        lastMaterialRegenChecked = System.DateTime.Now;
        ActionCameraCollider.ResetCameraCollider();

        if( ManagerAIAnimal.instance == null)
            gameObject.AddComponent<ManagerAIAnimal>();
    }
    void OnDestroy()
    {
        ManagerHousing.OnLobbyDestroyed();
        _tutorialLobbyMission_Play = false;
        _tutorialDiaryMission_Play = false;
        _tutorialQuestComplete_Play = false;

        _activeGetPokoyura = 0;
    }
    IEnumerator Start()
    {
        IsLobbyComplete = false;
        IsLoabbyActionState = false;

        _stageStart = false;
        int mainCamCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = 0;

        if( !transLand )
            landIndex = 0;

        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        LobbyEntryFocus.ResetFocusCandidates();

        CameraEffect staticCamera = null;
        if (Global.join && _firstLobby)
        {
            staticCamera = CameraEffect.MakeScreenEffect(1);
            staticCamera.ApplyScreenEffect(Color.black, Color.black, 0f, false);

            //그로씨
            {
                var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_FIRST_USE_REWARD,
                    0,
                    5,                   
                    0, //(int)(GameData.User.clover),
                    (int)(GameData.User.AllClover)//(int)(GameData.User.fclover)
                    );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);


                var useReadyItem20 = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                      "MATERIAL_2",
                      "material",
                      3,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_ADMIN
                  );
                var docStamp20 = JsonConvert.SerializeObject(useReadyItem20);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp20);
            }


        }
        //    CameraEffect staticCamera = CameraEffect.MakeScreenEffect(1);
        //  staticCamera.ApplyScreenEffect(Color.black, Color.black, 0f, false);
        
        //로비 진입할 때, 사운드 타입 일반으로 변경시켜줌.
        Global.SetGameSoundType(GameType.NORMAL);

        // 로비 씬에서 바로 씬 시작할때를 위해
        {
            ManagerData._instance.StartNetwork();
            #if UNITY_EDITOR
            ManagerData.IsMinorPrefabLoading = true;
            #endif
            // 데이타 완전이 받을때까지 대기
            while (true)
            {
                if (Global._pendingReboot) {
                    break;
                }
                
                if (ManagerData._instance._state == DataLoadState.eComplete)
                    break;
                yield return null;
            }
        }

        #region 탐험모드 매니저
        if (ManagerAdventure.CheckStartable())
        {
            bool advInited = false;
            ManagerAdventure.OnInit((b) => { advInited = true; });
            while (advInited == false)
            {
                yield return new WaitForSeconds(0.1f);
            }

            yield return ManagerData.CheckUserAdvDataIntegrity();

            ManagerAdventure.OnLobbyEnter();
        }
        #endregion

        ManagerEvent.instance.OnLobbyStart();
        
        // 크루그 이벤트 진행 중일 때만 체크
        if (ManagerSingleRoundEvent.instance != null)
            yield return ManagerSingleRoundEvent.instance.SetIsPlaying();

        #region 에리어 동물 매니져
        if (ManagerAreaAnimal.CheckStartable())
        {
            ManagerAreaAnimal.Init();
        }
        #endregion

        yield return null;
        yield return null;

        yield return ManagerSound._instance.InitSoundPack();
        
        // 보니 생성
        // 로비 area들 데이타 로딩하고 상태에따라 생성(보니 위치, 카메라 위치 설정)
        _state = TypeLobbyState.Preparing;
        StartCoroutine(CoLobbyMake());
        while (_state == TypeLobbyState.Preparing)
            yield return null;

        LobbyEntryFocus.DoFocus(_firstLobby);
        LobbyEntryFocus.ResetFocusCandidates();

        bool bgmOverrided = false;

        if (landIndex != 0)
        {
            bool bgmLoaded = false;
            var outlandBgmFilename = $"bgm_land_{landIndex}.data";
            if (HashChecker.IsExist("Sound/", outlandBgmFilename))
            {
                Box.LoadCDN("Sound", outlandBgmFilename, (AudioClip audio) =>
                {
                    bgmLoaded = true;
                    if (audio != null)
                    {
                        bgmOverrided = true;
                        ManagerSound._instance.PlayBGM(audio);
                    }
                });
                yield return new WaitUntil(() => { return bgmLoaded; });
            }
            
        }

        if ( !bgmOverrided )
        {
            //BGM 재생.
            ManagerSound._instance.PlayBGM();
        }

        yield return null;
        _state = TypeLobbyState.StartEvent;
        yield return null;

        ManagerUI._instance.InitTopUIDepth();
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eAll);
        ManagerUI._instance.ScrollbarRight.Open();

        //상단 UI 터치 막음.
        ManagerUI._instance.bTouchTopUI = false;

        // 읽고 설치하고 등 모든 상태가 완료되고 밝아짐
        if (Global.join && _firstLobby)
            if (staticCamera != null)
                staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.4f, true);
        ManagerUI._instance.UpdateUI(true);
        ManagerUI._instance.OnClickMenuTab();

        Camera.main.cullingMask = mainCamCullingMask;   // 카메라 컬링마스크 롤백 (월드 렌더링 개시)
        UIPopupDiary.openAtLobbyOptionEnabled = GameData.User.missionCnt > _missionThreshold_eventHousingOpen;

        yield return null;

        yield return ManagerEvent.instance.OnLobbyMakeFinished();

        SceneLoading.Release();
        SceneTitle.instance?.LobbyMakeWaitRelease();

        if ( ServerRepos.User.loginTs != 0 )
        {
            bool refreshCheckCompleted = false;
            bool needRefresh = false;
            ServerAPI.RefershUserGame(ServerRepos.User.loginTs,
                    (x) =>
                    {
                        if (x.isRefresh != 0)
                            needRefresh = true;
                        refreshCheckCompleted = true;
                    });

            while(refreshCheckCompleted == false)
            {
                yield return null;
            }

            if( needRefresh )
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_19"), false, null);
                popupSystem.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
                popupSystem.FunctionSetting(3, "OnApplicationPauseReboot", this.gameObject);

                yield break;
            }
        }
        
        var reservedActions = new List<KeyValuePair<StartPostProcTypes, int>>();
        //var reservedEventActions = new List<KeyValuePair<StartEventPostProcTypes, int>>();
        var reservedTutorialActions = new List<KeyValuePair<StartTutorialPostProcTypes, int>>();
        var reservedPopupEndActions = new List<KeyValuePair<StartPopupPostProcTypes, int>>();

        if (GameData.User.missionCnt > _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            //시간 갱신 체크.
            if (!_newDay && _reservedScene == null)
            {
                System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                System.DateTime rebootTime = System.DateTime.Today.AddHours(ServerRepos.LoginCdn.LoginOffset);
                rebootTime = rebootTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);
                rebootTime = rebootTime.AddSeconds(10);

                System.TimeSpan diff = rebootTime - origin;
                double time = System.Math.Floor(diff.TotalSeconds);

                //현재 시간이 갱신되는 시점 이후이고, 로그인은 갱신시간 이전에 한 경우 reboot 시켜줌.
                if (Global.GetTime() >= time && (GameData.User.loginTs < time && GameData.User.loginTs != 0))
                {
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_18"), false, null);
                    popupSystem.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
                    popupSystem.FunctionSetting(3, "OnApplicationPauseReboot", this.gameObject);
                    yield break;
                }
            }
            
            //공지(Notice)
            if (_firstNotice && Global._onlineMode)
            {
                yield return NoticeHelper.CoShowNotice();
            }

            //앱 실행 후 최초 로비 진입시에만 공지를 보여주도록 처리했습니다.
            //리붓시에는 공지를 다시 보여주도록 처리했습니다.
            _firstNotice = false;
        }

        //이벤트 정보 팝업(Notice와 별개)
        if (ServerRepos.Notices.Count > 0)
        {
            bool timeOverNoticeExists = false;
            for (int i = 0; i < ServerRepos.Notices.Count; i++)
            {
                var n = ServerRepos.Notices[i];

                if (n.type == 2 || n.type == 3 || n.type == 4)
                {
                    if (Global.LeftTime(ServerRepos.Notices[i].endTs) < 0)
                    {
                        timeOverNoticeExists = true;
                        break;
                    }
                }
            }
            if (timeOverNoticeExists)
                ManagerUI._instance.anchorTopLeft.DestroyLobbyButton<UIButtonGenericPage>();

            for (int i = 0; i < ServerRepos.Notices.Count; i++)
            {
                var n = ServerRepos.Notices[i];

                bool makeGenericPageButton = false;
                long endTs = 0;
                bool needLogin = false;

                if (Global.LeftTime(n.endTs) < 0)
                    continue;

                switch (n.type)
                {
                    case 2:
                        {
                            makeGenericPageButton = GameData.User.missionCnt > _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen;
                        }
                        break;
                    case 3:
                        {
                            makeGenericPageButton = GameData.User.stage > 1;
                        }
                        break;
                    case 4:
                        {
                            makeGenericPageButton = GameData.User.stage > 1;
                            needLogin = true;
                            endTs = n.endTs;
                        }
                        break;
                }

                if(makeGenericPageButton)
                {
                    ManagerUI._instance.MakeGenericPageIcon(n.noticeIndex, n.url, endTs, needLogin);
                }
            }
        }

        //스팟성 안내 팝업
        if (UIPopupSpotInfo.spotMessageData.Count > 0)
        {
            ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupSpotInfo>((popup) => popup.InitData());

            yield return new WaitWhile(() => UIPopupSpotInfo._instance != null);
        }

        //OA 친구 추가
        if ( ServerRepos.OAStatus != null && ServerRepos.OAStatus.status == "UNSPECIFIED")
        {
            bool showOAPopup = false;
            if(PlayerPrefs.HasKey("Last_OA_Recommended"))
            {
                int lastOAShowTime = PlayerPrefs.GetInt("Last_OA_Recommended");
                if(Global.LeftTime( (long)(lastOAShowTime + ServerRepos.OAStatus.interval)) < 0)
                {
                    showOAPopup = true;
                }
            }
            else 
                showOAPopup = true;

            if (showOAPopup)
            {
                PlayerPrefs.SetInt("Last_OA_Recommended", (int)Global.GetTime());


                ManagerUI._instance.OpenPopup_LobbyPhase<UIPopUpAddLineOAFriend>();

                yield return null;
                while (UIPopUpAddLineOAFriend.instance != null)
                    yield return null;
            }
        }

        if (GameData.User.missionCnt > _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        { 
            yield return ManagerAdventure.EventData.RequestUnclaimedRewards();
            yield return ManagerEvent.instance.OnRewardPhase();
        }

        // 대기
        _state = TypeLobbyState.Wait;
        // 조인(첫진입)
        if (Global.join && ManagerData._instance._missionData[1].state == TypeMissionState.Inactive)//Global.join && ManagerData._instance.missionData[0].state == TypeMissionState.Inactive)
        {
            ManagerData._instance._missionData[1].state = TypeMissionState.Active;
            _tutorialLobbyMission_Play = true;
            // 미션 아이콘 미리 읽어두기
            ResourceManager.LoadCDN(Global.gameImageDirectory, "IconMission", "m_1.png", (Texture2D texture) => ResourceManager.UnLoad(texture));

            if (ManagerArea._instance._areaStep.Count > 0)
                if (ManagerArea._instance._areaStep[1].area != null)
                {
                    PlayerPrefs.SetInt("missionCheck" + 1, 1);   // 다이어리 미션 아이콘 연출관련
                    yield return WaitForSceneEnd(ManagerArea._instance._areaStep[1].area, 1, -1, false);
                }

            yield return null;
        }
        else if (_newDay)
        {
            if (Global.day > 1)
            {
                int minMission = int.MaxValue;
                foreach (var item in ManagerData._instance._missionData)
                {
                    if (item.Value.day == Global.day)
                    {
                        if (minMission > item.Key)
                            minMission = item.Key;
                    }
                }
                //ManagerData._instance._missionData[minMission].state = TypeMissionState.Active;
                //PlaySceneWakeUp(ManagerArea._instance._areaStep[0].area, 16,-1,false);

                MissionData mission = ManagerData._instance._missionData[minMission];
                mission.state = TypeMissionState.Active;

                //미션 아이콘 미리 읽어두기
                ResourceManager.LoadCDN(Global.gameImageDirectory, "IconMission", $"m_{minMission}.png", (Texture2D texture) => ResourceManager.UnLoad(texture));
                PlayerPrefs.SetInt("missionCheck" + minMission, 1);   // 다이어리 미션 아이콘 연출관련

                yield return WaitForSceneEnd(ManagerArea._instance._areaStep[mission.sceneArea].area, mission.sceneIndex, -1, false);
                //Debug.Log("하루 시작  " + minMission + "  " + mission.sceneIndex);

                bool isTutorialPlay = false;

                if (!PlayerPrefs.HasKey(LOBBY_TUTORIAL_KEY) && ManagerUI._instance.ScrollbarRight.icons.Count > 0 && GameData.User.missionCnt > _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
                {
                    reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.LobbyRenewalTutorial, 0));
                }
                if (ManagerArea._instance._adventureEntry != null && PlayerPrefs.HasKey(ManagerAdventure.OpenSceneKey) == false)
                {
                    isTutorialPlay = true;
                    reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.AdventureStage, 0));
                }
                if (ManagerEvent.instance.OnTutorialCheck())
                {
                    reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.EventTutorial, 0));
                }
            }

            yield return null;
        }
        else if(transLand)
        {
            if (_reservedScene != null)
            {
                if (_reservedScene.scnStartDay != _reservedScene.scnDestDay)
                {
                    Global.day = _reservedScene.scnDestDay;
                    ServerContents.UpdateCdnDay(new List<CdnDay> { _reservedScene.newDay });
                }

                var rScene = _reservedScene;
                _reservedScene = null;
                yield return WaitForSceneEnd(ManagerArea._instance._areaStep[rScene.areaIndex].area, rScene.sceneIndex, rScene.missionIndex, false);

                _reservedScene = null;
            }

            if(_reservedHoisingChange != null)
            {
                _reservedHoisingChange();

                _reservedHoisingChange = null;
            }

            if(_reservedPostAction != null)
            {
                _reservedPostAction();

                _reservedPostAction = null;
            }
        }
        else if (_tutorialDiaryMission_Play)
        {
            _tutorialDiaryMission_Play = false;
            ManagerTutorial.PlayTutorial(TutorialType.TutorialDiaryMission);
        }
        else if (_tutorialQuestComplete_Play)
        {
            _tutorialQuestComplete_Play = false;
            ManagerTutorial.PlayTutorial(TutorialType.TutorialQuestComplete);
        }
        else
        {
            if (!PlayerPrefs.HasKey(LOBBY_TUTORIAL_KEY) && ManagerUI._instance.ScrollbarRight.icons.Count > 0 && GameData.User.missionCnt > _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            {
                reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.LobbyRenewalTutorial, 0));
            }
            
            if (ManagerEvent.instance.OnTutorialCheck())
            {
                reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.EventTutorial, 0));
            }
            
            yield return PlayNewPokoyuraScene();

            if (ManagerArea._instance._adventureEntry != null && PlayerPrefs.HasKey(ManagerAdventure.OpenSceneKey) == false)
            { //모험모드 이벤트 연출 + 튜토리얼
                reservedTutorialActions.Add(new KeyValuePair<StartTutorialPostProcTypes, int>(StartTutorialPostProcTypes.AdventureStage, 0));
            }            
            
            else
            {
                // 설정된 적 없으면 기본값 false
                bool diaryOpenAtEnterLobby = PlayerPrefs.HasKey("DoNotOpenDiaryAtLobbyIn") == false ? false : PlayerPrefs.GetInt("DoNotOpenDiaryAtLobbyIn") != 1;
                if (GameData.User.missionCnt <= _missionThreshold_eventHousingOpen)
                {
                    diaryOpenAtEnterLobby = true;
                }
                
                if (diaryOpenAtEnterLobby && _stageClear && !_firstLobby)
                {
                    // -미션다이어리창(인게임 클리어해서 별 획득하고 로비로 왔을때 그리고 미션을 클리어 할수 있는 조건만큼 별을 획득했을때 노출)(이벤트 같은 씬 연출이 예정되어 있을때는 연출이 겹칠수있으므로 제외)
                    bool haveClearMission = false;
                    var enumerator_M = ServerRepos.OpenMission.GetEnumerator();
                    while (enumerator_M.MoveNext())
                    {
                        ServerUserMission mData = enumerator_M.Current;
                        if (mData.state == (int)TypeMissionState.Active)
                        {
                            if (ManagerData._instance._missionData.ContainsKey(mData.idx))
                            {
                                MissionData mission = ManagerData._instance._missionData[mData.idx];
                                if (mission.waitTime > 0 && mData.clearTime > 0)// && Global.GetTime() > mData.clearTime)
                                {
                                    if (Global.GetTime() > mData.clearTime)
                                    {
                                        haveClearMission = true;
                                        break;
                                    }
                                }
                                else if (Global.star >= mission.needStar)
                                {
                                    haveClearMission = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (haveClearMission)
                    {
                        reservedPopupEndActions.Add(new KeyValuePair<StartPopupPostProcTypes, int>(StartPopupPostProcTypes.MissionDiary, (int)TypePopupDiary.eMission));
                    }
                }
                //재도전 해서 꽃 피웠을 경우, 스테이지 선택창에서 꽃 연출 띄워줌.
                else if (PlayerPrefs.HasKey("ActionFlowerState") == true)
                {
                    //첫 진입 시 남아있는 키는 제거.
                    if (_firstLobby)
                    {
                        PlayerPrefs.DeleteKey("ActionFlowerState");
                    }
                    else
                    {
                        reservedActions.Add(new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.StageAction, 1));
                    }
                }

                // -클로버 요청창(로그인시 하루에 한번, 랭킹창과 번갈아가면서 노출, 하루에 최대 한번만 노출)
                if (_firstLobby && !ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
                {
                    int day = System.DateTime.Now.DayOfYear;

                    if (day % 2 == 1 && PlayerPrefs.GetInt("OpenPopupRequestSmall", 0) != day)
                    {
                        PlayerPrefs.SetInt("OpenPopupRequestSmall", day);
                        reservedActions.Add(new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.RequestCloverSmall, day));

                    }
                    else if (PlayerPrefs.GetInt("OpenPopupRaking", 0) != day)
                    {
                        PlayerPrefs.SetInt("OpenPopupRaking", day);
                        reservedActions.Add(new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.Ranking, 0));

                    }
                }

                // 처음 행동은 무조껀 글로발 (첫 가입시는 예외) (((앱 업데이트관련+스테이지 업데이트관련)<너무 오랜만이면 패스)+이벤트 스테이지 관련>인사>)(너무 오랜만이면 인사만)
                if (!Global.join && _firstLobby)
                {
                    reservedActions.Add(new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.HelloBehavior, 0));
                }
                else if (_stageClear)
                {
                    reservedActions.Add(new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.UpdateStageBehavior, _stageTryCount));
                }
            }
        }

        //초반 스테이지 미션 (첫째날일 경우만 추가)
        if (ServerRepos.User.missionCnt < ManagerData._instance.chapterData[0]._stageCount)
            reservedPopupEndActions.Add(new KeyValuePair<StartPopupPostProcTypes, int>(StartPopupPostProcTypes.OneDayMission, 0));

        // 포코유라 업데이트가 돼야할 상황에 뭐 이런저런 우선순위 문제로 밀려서 업데이트에 실패했을 때
        if (_activeGetPokoyura > 0)
        {
            PokoyuraData.SetUserData();
            ReMakePokoyura();
            _activeGetPokoyura = 0;
        }
        
        //스테이지형 이벤트 강제 노출 미출력 조건
        var isForceDisplay = true;
        //1. 로비 튜토리얼(기본 튜토리얼)
        if (ManagerTutorial._instance != null)
        {
            isForceDisplay = ManagerTutorial._instance._playing == false;
        }
        //2. 이벤트 튜토리얼(캡슐가챠, 코인스테이지)
        if (ManagerEvent.instance != null)
        {
            isForceDisplay = isForceDisplay && ManagerEvent.instance.isTutorialPlay == false;
        }
        //3. 튜토리얼 스킵이 아닌 경우 특정 로비 튜토리얼(모험) / 컷씬만 출력은 제외
        if (Global._optionTutorialOn)
        {
            isForceDisplay = isForceDisplay && reservedTutorialActions.Count <= 0;
        }
        //4. 노멀 스테이지 외 종료 후, 이벤트 팝업 미출력
        isForceDisplay = isForceDisplay &&
                         (Global.GameType == GameType.NORMAL || transLand);


        //리뷰 팝업 등장 조건 : 로비 내에서 실행되는 연출(튜토리얼, 컷씬 등)이 없는 경우에만 재생, 하나라도 있을 경우에는 팝업을 등장시키지 않는다.
        // 1. 로비 튜토리얼(기본 튜토리얼)
        bool isScenePlay_AppReview = false;
        if (ManagerTutorial._instance != null)
            isScenePlay_AppReview = ManagerTutorial._instance._playing;

        // 2. 이벤트 컷씬(오브젝트 있는 이벤트), 3. 이벤트 튜토리얼(캡슐가챠, 코인스테이지)
        if (ManagerEvent.instance != null)
            isScenePlay_AppReview = isScenePlay_AppReview || ManagerEvent.instance.isTutorialPlay || ManagerEvent.instance.isCutScenePlay;

        // 4. 특정 로비 튜토리얼(모험)
        isScenePlay_AppReview = isScenePlay_AppReview || reservedTutorialActions.Count > 0;

        // 5. 포코유라 연출
        isScenePlay_AppReview = isScenePlay_AppReview || _activeGetPokoyura > 0;

        if (!_firstLobby && !isScenePlay_AppReview)
        {
            bool isAppear = false;
            // 1. 모든 에피소드의 마지막 스테이지 클리어 직후 (예외처리 : 타 팝업이 존재하는 경우 (이벤트 스테이지 / 모험), (예외처리2 : 맵 이동 직후에는 등장하지 않도록))
            if (reservedPopupEndActions.Count == 0 && ManagerUI._instance.GetPopupCount() == 0 && Global.GameType == GameType.NORMAL && Global.IsClear && !transLand && !showIntro)
            {
                int stageCount = 1;
                foreach (var chapterData in ManagerData._instance.chapterData)
                {
                    stageCount += chapterData._stageCount;
                    if (stageCount > ServerRepos.User.stage)
                        break;
                    if (ServerRepos.User.stage == stageCount && Global.stageIndex == ServerRepos.User.stage)
                    {
                        reservedPopupEndActions.Add(new KeyValuePair<StartPopupPostProcTypes, int>(StartPopupPostProcTypes.ReviewRequest, 0));
                        isAppear = true;
                        break;
                    }
                }
            }
            // 2. 다음 날 연출이 끝난 직후
            if (!isAppear)
            {
                if (_newDay)
                {
                    reservedPopupEndActions.Add(new KeyValuePair<StartPopupPostProcTypes, int>(StartPopupPostProcTypes.ReviewRequest, 0));
                    isAppear = true;
                }
            }
        }

        if (ManagerEvent.instance != null)
            ManagerEvent.instance.isTutorialPlay = false;

        if (landIndex == 0)
        {
            Global.GameInstance.CheckKeepPlay();
        }
        else
        {
            //집 내부 진입 시, 게임 플레이 상태 초기화 시켜줌
            Global.GameInstance.SkipKeepPlay();
            Global.SetGameType_NormalGame();
        }
        yield return null;

        bool wasFirstLobby = _firstLobby;

        // newDay로 한번 씬을 시작하고 나면 기존으로 돌리기
        _newDay        = false;
        transLand      = false;
        showIntro   = false;
        _reservedScene = null;
        _loadComplete  = true;
        _firstLobby    = false;
        _firstLogin    = false;
        if (_stageClear)
            _stageTryCount = 0;
        _stageClear = false;
        _activeGetPokoyura = 0;

        yield return ManagerEvent.instance.OnPostLobbyEnter();

        if (ManagerAreaAnimal.IsActiveEvent())
        {
            yield return ManagerAreaAnimal.OnLobbyEnter();
        }

        yield return ManagerAIAnimal.StartSync(wasFirstLobby);

        yield return coStartPostProcess = StartCoroutine(CoStartPostProcess(reservedActions));

        if (Global.day > 1 && ServerRepos.User.missionCnt <= ManagerData._instance.chapterData[0]._stageCount && UIItemOneDayMission._instance != null)
        {
            UIItemOneDayMission._instance.IsRunCoroutine();

            yield return new WaitWhile(() => UIItemOneDayMission._instance.IsRun);

            UIItemOneDayMission._instance.OneDayMissionClear();

            yield return new WaitWhile(() => UIItemOneDayMission._instance != null);
        }

        DownLoadAndCheckAreaIllustration();

        //yield return coStartEventPostProcess = StartCoroutine(CoStartEvnetPostProcess(reservedEventActions));

        yield return ManagerEvent.instance.OnLobbyScenePhase();

        IsLoabbyActionState = true;

        yield return coStartTutorialPostProcess = StartCoroutine(CoStartTutorialPostProcess(reservedTutorialActions));
        yield return coStartPopupPostProcess = StartCoroutine(CoStartPopupPostProcess(reservedPopupEndActions));

        if ( Global.GameInstance != null)
        {
            yield return Global.GameInstance.KeepPlayOn();
        }

        //인게임 진입하지 않은 상태에서만 동작하는 코드들.
        if (_stageStart == false)
        {
            UIDiaryController._instance.OpenQuestBanner();

            if (OnEventHighlight != null)
            {
                yield return new WaitForSeconds(0.5f);
                
                OnEventHighlight.Invoke();
                OnEventHighlight = null;
                yield return new WaitForSeconds(ManagerUI.SpawnEffectTime);
            }
            
            yield return CoShowIntegratedEventPopup();

            if (ManagerForceDisplayEvent.CheckStartable() && isForceDisplay)
            {
                yield return ManagerForceDisplayEvent.instance.OnForceDisplayEvent();
            }
        }

        IsLobbyComplete = true;

        //상단 UI 터치 막음 해제.
        ManagerUI._instance.bTouchTopUI = true;
    }
    Coroutine coStartPostProcess = null;
    Coroutine coStartShopPostProcess = null;
    Coroutine coStartEventPostProcess = null;
    Coroutine coStartTutorialPostProcess = null;
    Coroutine coStartPopupPostProcess = null;

    public void StopRemainStartPostProc()
    {
        if (coStartPostProcess != null)
            StopCoroutine(coStartPostProcess);
        if (coStartShopPostProcess != null)
            StopCoroutine(coStartShopPostProcess);
        if (coStartEventPostProcess != null)
            StopCoroutine(coStartEventPostProcess);
        if (coStartTutorialPostProcess != null)
            StopCoroutine(coStartTutorialPostProcess);
        if (coStartPopupPostProcess != null)
            StopCoroutine(coStartPopupPostProcess);
    }

    enum StartPostProcTypes
    {
        StageAction,
        RequestCloverSmall,
        Ranking,
        HelloBehavior,
        UpdateStageBehavior,
        KeepPlayOn,
    }

    //enum StartEventPostProcTypes
    //{   
    //    EventAction,
    //    BoxAction,
    //    MoleCatchAction,
    //    WorldRankAction,
    //    OneDayMissionReward,
    //    PokoFlowerEventAction,
    //}

    enum StartTutorialPostProcTypes
    {
        AdventureStage,
        EventTutorial,
        LobbyRenewalTutorial,
    }

    enum StartPopupPostProcTypes
    {
        MissionDiary,
        OneDayMission,
        SpotInfoPopup,
        ReviewRequest,
    }

    private IEnumerator CoStartPostProcess(List< KeyValuePair<StartPostProcTypes, int> > reservedList)
    {
        for(int i = 0; i < reservedList.Count; ++i)
        {
            if (SceneManager.GetActiveScene().name != "Lobby")
                yield break;
            var popupType = reservedList[i].Key;
            switch (popupType)
            {
                case StartPostProcTypes.StageAction:
                    {
                        //ManagerUI._instance.OpenPopupStageAction(reservedList[i].Value == 1);

                        ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupStage>((popup) =>
                        {
                            popup.InitStagePopUp(reservedList[i].Value == 1);
                        });

                        yield return null;
                        while (UIPopupStage._instance != null)
                            yield return null;
                    }
                    break;
                case StartPostProcTypes.KeepPlayOn:
                    {
                    }
                    break;
                case StartPostProcTypes.RequestCloverSmall:
                    {
                        ManagerLobby._instance.IsLoabbyActionState = true;
#if UNITY_IOS
                        // ios 정책상 여러명 초대나 요청같은거 못보내게 됐음
                        break;
#endif

                        yield return ManagerUI._instance.CoOpenPopupRequestSmall();

                        if (UIPopupRequestCloverSmall._instance != null)
                        {
                            yield return new WaitUntil(() => UIPopupRequestCloverSmall._instance == null);
                        }
                        else
                        {
                            if (PlayerPrefs.GetInt("OpenPopupRaking", 0) != reservedList[i].Value)
                            {
                                PlayerPrefs.SetInt("OpenPopupRaking", reservedList[i].Value);
                                ManagerUI._instance.OpenPopupRaking();
                                yield return new WaitNoPopup();
                            }
                        }

                        ManagerLobby._instance.IsLoabbyActionState = false;
                    }
                    break;
                case StartPostProcTypes.Ranking:
                    {
                        ManagerLobby._instance.IsLoabbyActionState = true;

                        ManagerUI._instance.OpenPopupRaking();
                        yield return new WaitNoPopup();

                        ManagerLobby._instance.IsLoabbyActionState = false;
                    }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          break;
                case StartPostProcTypes.HelloBehavior:
                    {
                        LobbyBehavior._instance.PlayHelloBehavior();
                    }
                    break;
                case StartPostProcTypes.UpdateStageBehavior:
                    {
                        LobbyBehavior._instance.PlayUpdateStageBehavior(reservedList[i].Value);
                    }
                    break;
            }
        }
        coStartPostProcess = null;
    }

    private IEnumerator CoStartTutorialPostProcess(List<KeyValuePair<StartTutorialPostProcTypes, int>> reservedList)
    {
        for (int i = 0; i < reservedList.Count; ++i)
        {
            if (SceneManager.GetActiveScene().name != "Lobby")
                yield break;
            var tutorialType = reservedList[i].Key;
            switch (tutorialType)
            {
                case StartTutorialPostProcTypes.AdventureStage:
                    {
                        IsLoabbyActionState = false;

                        yield return StartAdventureCutScene();

                        IsLoabbyActionState = true;
                    }
                    break;

                case StartTutorialPostProcTypes.EventTutorial:
                    {
                        yield return ManagerEvent.instance.OnLobbyEventTutorialPhase();
                    }
                    break;
                case StartTutorialPostProcTypes.LobbyRenewalTutorial:
                {
                    yield return StartLobbyRenewalTutorial();
                }
                    break;
            }
        }
        coStartTutorialPostProcess = null;
    }

    private IEnumerator CoStartPopupPostProcess(List<KeyValuePair<StartPopupPostProcTypes, int>> reservedList)
    {
        for (int i = 0; i < reservedList.Count; ++i)
        {
            if (SceneManager.GetActiveScene().name != "Lobby")
                yield break;
            var popupType = reservedList[i].Key;
            switch (popupType)
            {
                case StartPopupPostProcTypes.MissionDiary:
                    {
                        Debug.Log("ManagerLobby:1001: OpenPopupDiary");
                        ManagerUI._instance.OpenPopupDiary((TypePopupDiary)(reservedList[i].Value));
                        yield return null;
                        while (UIPopupDiary._instance != null)
                            yield return null;
                        while (UIDiaryController._instance.isDiaryActionComplete == false)
                            yield return null;
                        Debug.Log("ManagerLobby:1006: PopupDiary Closed");
                        break;
                    }
                case StartPopupPostProcTypes.OneDayMission:
                    {
                        UIItemOneDayMission itemOneDayMission = UIItemOneDayMission._instance;

                        if (itemOneDayMission == null)
                        {
                            itemOneDayMission = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemOneDayMission", ManagerUI._instance.anchorTopCenter).GetComponent<UIItemOneDayMission>();
                        }

                        itemOneDayMission.SetOneDayMissionPosition();
                        
                        yield return new WaitWhile(() => itemOneDayMission.IsRun);
                    }
                    break;
                case StartPopupPostProcTypes.ReviewRequest:
                    {
                        ManagerUI._instance.OpenPopupRequestReview();
                    }
                    break;
            }
        }
        coStartPopupPostProcess = null;
    }

    private IEnumerator StartLobbyRenewalTutorial()
    {
        PlayerPrefs.SetInt(LOBBY_TUTORIAL_KEY, 1);
        
        ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyRenewal);
        if (ManagerTutorial._instance != null)
            yield return new WaitUntil(() => ManagerTutorial._instance._playing == false);
    }

    private IEnumerator StartAdventureCutScene()
    {
        PlayerPrefs.SetInt(ManagerAdventure.OpenSceneKey, 1);

        if (ManagerAdventure.User.GetChapterProgress(1) == null)
        {
            yield return WaitForSceneEnd(ManagerArea._instance._adventureEntry, 1);
            ManagerTutorial.PlayTutorial(TutorialType.TutorialStart_Adventure);

            //탐험모드 처음시작 날개 지급
            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
            (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_FIRST_USE_REWARD,
            0,
            3,
            0,
            (int)(GameData.User.AllWing)
            );
            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);

            //동물지급
            var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                      "Animal_1001",
                      "Animal",
                     1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_FIRST_USE_REWARD,
                   "Animal_1001"
                  );
            var DocTicket = JsonConvert.SerializeObject(rewardTicket);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);

            var rewardTicket1 = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                      "Animal_1002",
                      "Animal",
                     1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_FIRST_USE_REWARD,
                   "Animal_1002"
                  );
            var DocTicket1 = JsonConvert.SerializeObject(rewardTicket1);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket1);

            var rewardTicket2 = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                      "Animal_3001",
                      "Animal",
                     1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_FIRST_USE_REWARD,
                   "Animal_3001"
                  );
            var DocTicket2 = JsonConvert.SerializeObject(rewardTicket2);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket2);

            if (ManagerTutorial._instance != null)
                while (ManagerTutorial._instance._playing)
                    yield return new WaitForSeconds(0.1f);
        }
        else
        {
            ManagerArea._instance._adventureEntry._listSceneDatas[0].state = TypeSceneState.Active;
            ManagerArea._instance._adventureEntry.TriggerStart();
        }
    }

    private IEnumerator CoShowIntegratedEventPopup()
    {
        if (ManagerIntegratedEvent.Instance == null || ManagerIntegratedEvent.CanShowIntegratedEventPopup() == false)
        {
            yield break;
        }

        yield return StartCoroutine(ManagerIntegratedEvent.Instance.CoOpenIntegratedEventPopup());
    }

    // Update is called once per frame
    void Update () {

        // 대기 상태이거나 팝업창이 없을때
        if (_state == TypeLobbyState.Wait)
        {
            bool touchUI = false;
            if (ManagerUI._instance != null)
                if (UICamera.selectedObject != null)
                    if (UICamera.selectedObject != ManagerUI._instance.gameObject)
                        touchUI = true;

            if (ManagerUI._instance != null && ManagerUI._instance._popupList.Count != 0)
                touchUI = true;

            if (SceneLoading.IsSceneLoading)
                touchUI = true;

            if (touchUI == false)
            {
                if (Global._touchTap)
                {
                    #region 터치했을 때 동작
                    Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(Global._touchPos);
                    RaycastHit hitInfo;
                    bool skipWalk = false;

                    if (Physics.Raycast(ray, out hitInfo, 400, Global.eventObjectMask))
                    {
                        ObjectIcon objIcon = hitInfo.collider.transform.GetComponent<ObjectIcon>();
                        if (objIcon != null)
                        {
                            skipWalk = true;
                            objIcon.OnTap();
                        }
                        else
                        {
                            Character objCharacter = hitInfo.collider.GetComponent<Character>();
                            if (objCharacter != null)
                            {
                                skipWalk = true;
                                objCharacter.OnTap();
                            }
                            else
                            {
                                ObjectEvent objEvent = hitInfo.collider.transform.parent.GetComponent<ObjectEvent>();
                                if (objEvent != null)
                                {
                                    if (objEvent._touch)
                                    {
                                        skipWalk = true;
                                        objEvent.OnTap();
                                    }
                                }
                                ObjectMaterial objMaterial = hitInfo.collider.transform.parent.GetComponent<ObjectMaterial>();
                                if (objMaterial != null)
                                {
                                    skipWalk = true;
                                    objMaterial.OnTap();
                                }
                            }
                        }
                    }
                    if (!skipWalk)
                    {

                        ManagerSound.AudioPlay(AudioLobby.Button_01);

                        if (Random.value >= 0.5f)
                            ManagerSound.AudioPlay(AudioLobby.m_boni_haa);
                        else
                            ManagerSound.AudioPlay(AudioLobby.m_boni_hoa);

                        LobbyBehavior._instance.CommandWalk();
                    }
                    #endregion
                }
                else if (Global._touchBegin == true)
                {
                    //UI 위치를 클릭중이라면 반환
                    if (UICamera.Raycast(Global._touchPos) == true)
                        return;

                    Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(Global._touchPos);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 400, Global.eventObjectMask))
                    {
                        ObjectEvent objEvent = hitInfo.collider.transform.parent?.GetComponent<ObjectEvent>();
                        if (objEvent != null)
                        {
                            objEvent.OnTouching();
                        }
                    }
                }
            }
            else
            {
                CheckAndRegenerateMaterial();
            }
            //    Debug.Log(UICamera.selectedObject);
        }
	}

    System.DateTime lastMaterialRegenChecked;
    public void CheckAndRegenerateMaterial()
    {
        if( ManagerLobby.landIndex != 0)
            return;

        if (lastMaterialRegenChecked.AddSeconds(1.0f) < System.DateTime.Now)
        {
            lastMaterialRegenChecked = System.DateTime.Now;
        }
        else return;

        //CheckAndRegenerateMaterial_Internal_Server();
        CheckAndRegenerateMaterial_Internal_Client();
    }

    private void CheckAndRegenerateMaterial_Internal_Server()
    {
        bool regen = false;
        foreach (var matProg in ServerContents.MaterialSpawnProgresses)
        {
            string k = "MaterialRegenAt_" + matProg.Value.materialIndex;
            if (PlayerPrefs.HasKey(k))
            {
                string tsString = PlayerPrefs.GetString(k, "0");
                long ts = System.Convert.ToInt64(tsString);
                if (Global.LeftTime(ts) < 0)
                {
                    regen = true;
                    PlayerPrefs.DeleteKey(k);
                }
            }
        }

        if (regen)
        {
            ServerAPI.MaterialRespawn((Protocol.MaterialRespawnResp resp)
                =>
            {
                if (resp.IsSuccess)
                {
                    MaterialSpawnUserData.SetUserData();
                    if (ManagerLobby._instance != null)
                        ReMakeMaterial();
                }
            }
                );
        }
    }

    private void CheckAndRegenerateMaterial_Internal_Client()
    {
        bool regen = false;

        foreach(var matMeta in ServerContents.MaterialMeta)
        {
            var serverProg = ServerRepos.SpawnMaterial.Find(x => { return x.materialIndex == matMeta.Value.mat_id; });

            string k = "MaterialRegenAt_" + matMeta.Value.mat_id;

            if (serverProg != null && PlayerPrefs.HasKey(k))
            {
                bool timeOver = false;
                if (matMeta.Value.expireTs != 0 && Global.LeftTime(matMeta.Value.expireTs) <= 0)
                    timeOver = true;

                if (serverProg.materialCount > 0 || timeOver)
                {
                    PlayerPrefs.DeleteKey(k);
                }
                else
                {
                    string tsString = PlayerPrefs.GetString(k, "0");
                    long ts = System.Convert.ToInt64(tsString);
                    if (Global.LeftTime(ts) < 0)
                    {
                        regen = true;
                        serverProg.materialCount++;
                        PlayerPrefs.DeleteKey(k);
                    }
                }
            }
        }

        if (regen)
        {
            MaterialSpawnUserData.SetUserData();
            if (ManagerLobby._instance != null)
                ReMakeMaterial();
        }
    }
   
    static public GameObject NewObject(Object obj)
    {
        try {
            GameObject tmp = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
            //tmp = tmp.Instantiate_ShaderFix();
            tmp.name = obj.name;

            tmp.transform.parent = ManagerArea._instance.transform;
            return tmp;
        }
        catch (System.Exception e) {
            //Debug.LogError("Lobby NewObject Exception: " + e);
        }

        return null;
#if UNITY_EDITOR
       // ResetShader(tmp.transform);
#endif
    }

    public static int landIndex = 0;

    IEnumerator CoLobbyMake()
    {
        // 원래 로비만들때마다 스트링파일 한번씩 체크했던거같음
        if (Global._instance._stringData.Count == 0 || Global._instance.invalidateString == true)
        {
            LocalText.Instance.Init();

            Global._instance._stringData.Clear();
            Global._instance.invalidateString = false;

            string fileName = "g_1.json";

            if (!string.IsNullOrEmpty(fileName))
            {
                StringHelper.LoadStringFromCDN(fileName, Global._instance._stringData);
            }
        }

        List<string> areaNameList = landIndex == 0 ? ServerContents.Day.GetString() : ServerContents.Day.outlands[landIndex];
        Area.globalAreaOrder = 1;

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Reset();
        watch.Start();
        // 개발 설정이 아니고 씬에 추가되어 있는 area파일이 없다면 받거나 읽어오기
        {
            // 읽어오는 방법은 총 2가지   1.프로젝트 폴더내에서 바로바로 읽어오기(에디터상에서 개발할때, 에셋만들 필요없음)   2.cdn서버 또는 캐시된 파일에서 읽어오기,각 플렛폼에 맞는 에셋을 만들어야 함(폰이나 에디터 상테서 실행 테스트 일때)
            for (int i = 0; i < areaNameList.Count; i++)
            {
                if (areaNameList[i].Contains("_p") == false)
                {
                    continue;
                }

                GameObject loaded = null;

                if (Global.LoadFromInternal)
                {
                    MatchCollection matches = Regex.Matches(areaNameList[i], "[0-9]+");

                    bool haveObject = false;

                    for (int x = 0; x < ManagerArea._instance.transform.childCount; x++)
                    {
                        var childTm = ManagerArea._instance.transform.GetChild(x);
                        if (childTm.gameObject.active && areaNameList[i].Contains(childTm.name))
                        {
                            haveObject = true;
                            loaded = childTm.gameObject;
                            break;
                        }
                    }

                    if (!haveObject)
                    {
                        string path = "";

                        if (areaNameList[i].Contains("a_"))
                        {
                            int areaIndex = int.Parse(matches[0].Value);
                            int levelIndex = int.Parse(matches[1].Value);

                            path = "Assets/5_OutResource/area_" + areaIndex + "/area_" + areaIndex + "_" + levelIndex + "/prefab/a_" + areaIndex + "_" + levelIndex + ".prefab";
                        }
                        else if (areaNameList[i].Contains("ac_"))
                        {
                            int areaIndex = int.Parse(matches[0].Value);
                            int levelIndex = int.Parse(matches[1].Value);

                            path = "Assets/5_OutResource/area_" + areaIndex + "/areacommon_" + areaIndex + "_" + levelIndex + "/prefab/ac_" + areaIndex + "_" + levelIndex + ".prefab";
                        }
                        else if (areaNameList[i].Contains("g_"))
                        {
                            if( matches.Count > 1 )
                            {
                                int landIndex = int.Parse(matches[0].Value);
                                int levelIndex = int.Parse(matches[1].Value);
                                path = $"Assets/5_OutResource/global/global_{landIndex}_{levelIndex}/g_{landIndex}_{levelIndex}.prefab";
                            }
                            else
                            {
                                int levelIndex = int.Parse(matches[0].Value);
                                path = "Assets/5_OutResource/global/global_" + levelIndex + "/g_" + levelIndex + ".prefab";
                            }
                            
                        }

#if UNITY_EDITOR
                        ApplyAreaRedirect(ref path);
                        GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        loaded = NewObject(BundleObject);
#endif
                    }
                }
                else
                {
                    if (areaNameList[i].Contains("_p"))
                    {
                        yield return null;

                        // 앱기동시,  뉴데이씬에서 혹시 못 읽은 에셋은 지금이라도 읽는다.

                        GameObject objN = null;
                        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(areaNameList[i]);
                        while (e.MoveNext())
                            yield return e.Current;
                        if (e.Current != null)
                        {
                            AssetBundle assetBundle = e.Current as AssetBundle;
                            if (assetBundle != null)
                            {
                                var assetReq = assetBundle.LoadAssetAsync<GameObject>(areaNameList[i].Replace("_p", ""));
                                yield return assetReq;
                                if (assetReq.isDone)
                                {
                                    objN = assetReq.asset as GameObject;
                                }

                            }
                        }
                        yield return null;


                        if( objN != null )
                            loaded = NewObject(objN);
                    }
                }

                if( loaded != null)
                {
                    AreaBase areaBase = loaded.GetComponent<AreaBase>();
                    if (areaBase)
                    {
                        ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                    }
                }
            }
        }

        yield return ManagerCharacter._instance.LoadCharacters();

        //코인 배수 이벤트
        if (ServerRepos.LoginCdn.CoinEventRatio > 0)
        {           
            long leftTime = Global.LeftTime(ServerRepos.LoginCdn.CoinEventTs);

            if (leftTime > 0)
            {
                Global.coinEvent = ServerRepos.LoginCdn.CoinEventRatio;
            }
            else
            {
                Global.coinEvent = 0;
            }
        }
        else
        {
            Global.coinEvent = 0;
        }

        yield return ManagerEvent.instance.OnLobbyObjectLoadPhase();

        ManagerEvent.instance.OnLobbySceneCheck();

        ManagerEvent.instance.OnIconPhase();
       
        if (ServerRepos.User.missionCnt >= 7)
        {
            ManagerUI._instance.MakeLandMoveIcon();
            ManagerUI._instance.anchorBottomLeft.GetComponentInChildren<UIButtonLandMove>().SetAlarmNewLand();
        }
        
        if (ManagerAdventure.CheckStartable())
        {
            ManagerUI._instance.MakeAdventureModeIcon(); 
        }

        ManagerUI._instance.PackagePhase();

        yield return AdventureEntry.OnLoadLobbyObject();

        //라인7주년 이벤트
        if (ServerContents.PromotionEvent != null && ServerContents.PromotionEvent.event_index > 0
            && Global.LeftTime(ServerContents.PromotionEvent.end_ts) > 0)
        {
            ManagerUI._instance.MakePromotionEventIcon(ServerContents.PromotionEvent.event_index);
        }
        else
        {
            ManagerUI._instance.anchorTopLeft.DestroyLobbyButton<UIButtonPromotion>();
        }

        watch.Stop();
        //Global.Log("asset " + (float)watch.ElapsedMilliseconds / 1000f);

        var missionData = _reservedScene != null && transLand ? _reservedScene.missionBackup : ManagerData._instance._missionData;

        ManagerArea._instance.TriggerSetting(missionData);
        if( ManagerLobby.landIndex == 0)
            ManagerArea._instance.TriggerSettingEvent();

        Vector3 boniPosition = Vector3.zero;
        Vector3 cameraPosition = Vector3.zero;
        float cameraZoom = 0f;

        //일단 젤 첫 area 위치에서 시작
        foreach( var a in ManagerArea._instance._areaStep)
        {
            Area area = a.Value.area;
            if (area != null)
            {
                // 카메라
                if (area._defaultCameraPosition != null)
                {
                    cameraPosition = new Vector3(area._defaultCameraPosition.position.x + area._defaultCameraOffset.x, 0f, area._defaultCameraPosition.position.z + area._defaultCameraOffset.y);
                    if (area._defaultZoom > 0f)
                        cameraZoom = area._defaultZoom;
                }
                // 케릭터
                if (area._defaultCharacterPosition != null)
                {
                    boniPosition = area._defaultCharacterPosition.position + new Vector3(area._defaultCharacterOffset.x, 0f, area._defaultCharacterOffset.y);
                }
            }
        }

        // 받은 미션이 있고 카메라, 케릭터 정보가 있다면 그곳에서 시작
        foreach( var a in ManagerArea._instance._areaStep)
        {
            Area area = a.Value.area;
            if (area != null)
            {
                for (int x = 0; x < area._listSceneDatas.Count; x++)
                {
                    if (area._listSceneDatas[x].state == TypeSceneState.Active)
                    {
                        if (area._listSceneDatas[x].sceneData !=null)
                            if (area._listSceneDatas[x].sceneData._triggerActive != null)
                            {
                                TriggerStateActive tState = area._listSceneDatas[x].sceneData._triggerActive;
                                // 카메라
                                if (tState._defaultCameraPosition != null)
                                {
                                    cameraPosition = new Vector3(tState._defaultCameraPosition.position.x + tState._defaultCameraOffset.x, 0f, tState._defaultCameraPosition.position.z + tState._defaultCameraOffset.y);
                                    if (tState._defaultZoom > 0f)
                                        cameraZoom = tState._defaultZoom;
                                    else
                                        cameraZoom = 0f;
                                }
                                // 케릭터
                                if (tState._defaultCharacterPosition != null)
                                {
                                    boniPosition = tState._defaultCharacterPosition.position + new Vector3(tState._defaultCharacterOffset.x, 0f, tState._defaultCharacterOffset.y);
                                }
                                else
                                {
                                    //Debug.Log( " ------------------------------ Boni _defaultCharacterPosition이 null임 ------------------------------ " );
                                }
                            }
                    }
                }
            }
        }

        CameraController._instance.SetCameraPosition(cameraPosition);
        if (cameraZoom > 0f)
            CameraController._instance.SetFieldOfView(cameraZoom);
            //CameraController._instance.moveCamera.fieldOfView = cameraZoom;
        _workCameraPosition = cameraPosition;
        LobbyAreaIcon._instance.SetWorkPosition(CameraController._instance._transform.position);
        boniPosition.y = 0f;

        ServerUserCostume costumeInfo = ServerRepos.UserCostumes.Find(x => x.is_equip == 1);
        //int costumeID = 1;    // 테스트시에는 대충 이렇게
        int costumeID = (costumeInfo == null) ? 0 : costumeInfo.costume_id;

        // NOTE: 2019 만우절
        if (ServerRepos.LoginCdn.aprilFool != 0)
            costumeID = ServerRepos.LoginCdn.aprilFool;

        yield return CoLoadCostume(costumeID);

        MakeCharacter(TypeCharacterType.Boni, costumeID, boniPosition);

        yield return null;

        yield return PreDownloader.Download();

        ManagerArea._instance.TriggerStart();
        ManagerArea._instance.TriggerStartEvent();
        yield return null;

        //if (ManagerLevelBlindStatic.instance != null)
        //    ManagerLevelBlindStatic.instance.LoadTileChunkDataInfoByBank();

        ActionNaviMesh.LoadLastNaviMesh();
        ObjectBase.EditNaviCollider();

        ActionCameraCollider.LastColliderActive();

        ReMakeGiftbox();
        ReMakeMaterial();
        Pokoyura.LoadPokoyuraDeploy();
        ReMakePokoyura();
        UIDiaryController._instance.InitDiaryData();

        //광고 지면 재료상자 생성.
        ReMakeMaterialbox();

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        // 클리어하고 로비 진입시 스타트 버튼에 스테이지 표시 증가 연출
        {
            if (oldStage != myProfile.stage && oldStage != -1)
            {
                SetStageLabel(oldStage.ToString());
                StartCoroutine(CoClearStage());
            }
            else
            {
                SetStageLabel(myProfile.stage.ToString());
            }
            if (oldStage == 1 && myProfile.stage == 2)
                _tutorialDiaryMission_Play = true;

            oldStage = myProfile.stage;
        }
        // 일반 스테이지 모두 플레이 했을때 (꽃이 목표가 되게)
        {
            if (myProfile.stage > ManagerData._instance.maxStageCount)
            {
                ManagerUI._instance._rootStage.gameObject.SetActive(false);
                ManagerUI._instance._rootStageFlower.gameObject.SetActive(true);
            }
            else
            {
                ManagerUI._instance._rootStage.gameObject.SetActive(true);
                ManagerUI._instance._rootStageFlower.gameObject.SetActive(false);
            }

        }
        _state = TypeLobbyState.PreparingEnd;        
        yield return null;

        System.GC.Collect();
    }

    IEnumerator CoClearStage()
    {
        yield return new WaitForSeconds(0.8f);

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        SetStageLabel(myProfile.stage.ToString());

        ManagerUI._instance._labelStage[0].cachedTransform.localScale = Vector3.one * 1.5f;
        ManagerUI._instance._labelStage[0].cachedTransform.DOScale(1f, 0.5f).SetEase(Ease.OutSine);
        ManagerUI._instance._labelStage[2].cachedTransform.localScale = Vector3.one * 1.5f;
        ManagerUI._instance._labelStage[2].cachedTransform.DOScale(1f, 0.5f).SetEase(Ease.OutSine);
        yield return null;
    }

    private void SetStageLabel(string stageIdx)
    {
        foreach (var label in ManagerUI._instance._labelStage)
        {
            label.text = stageIdx;
        }
    }
    
    public void NewMakePokoyura()
    {
        if (ManagerLobby.landIndex != 0)
            return;

        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);

        if(ManagerData._instance._pokoyuraData.Count < _spawnPokogoroPosition.Length && _activeGetPokoyura != 0)
        {
            Pokoyura poko = Instantiate<Pokoyura>(_objPokoyura);
            poko.transform.position = _spawnPokogoroPosition[ManagerData._instance._pokoyuraData.Count].position;
            poko._index = _activeGetPokoyura;
            poko._newMakeShow = true;

            if (_objPokogoro != null)
            {
                _objPokogoro.AttachPokoyura(poko);
            }
        }

        PokoyuraData.SetUserData();
    }
    public void ReMakePokoyura()
    {
        if( ManagerLobby.landIndex != 0)
            return;

        while (Pokoyura._pokoyuraList.Count > 0)
            DestroyImmediate(Pokoyura._pokoyuraList[0].gameObject);

        if( Pokoyura.pokoyuraDeployCustom != null)
        {
            for(int i = 0; i < Pokoyura.pokoyuraDeployCustom.Count; ++i)
            {
                if (i >= _spawnPokogoroPosition.Length)
                    break;

                if (Pokoyura.pokoyuraDeployCustom[i] == 0)
                    continue;

                Pokoyura poko = Instantiate<Pokoyura>(_objPokoyura);
                poko.transform.position = _spawnPokogoroPosition[i].position;
                poko._index = Pokoyura.pokoyuraDeployCustom[i];

                if (_objPokogoro != null)
                {
                    _objPokogoro.AttachPokoyura(poko);
                }
            }
        }
        else
        {
            for(int i = 0; i < ManagerData._instance._pokoyuraData.Count; ++i)
            {
                if (i >= _spawnPokogoroPosition.Length)
                    break;

                Pokoyura poko = Instantiate<Pokoyura>(_objPokoyura);
                poko.transform.position = _spawnPokogoroPosition[i].position;
                poko._index = ManagerData._instance._pokoyuraData[i].index;

                if (_objPokogoro != null)
                {
                    _objPokogoro.AttachPokoyura(poko);
                }
            }
        }
    }

    protected IEnumerator PlayNewPokoyuraScene()
    {
        if (_activeGetPokoyura > 0)
        {
            if (ServerRepos.User.missionCnt >= 9)
            {
                bool autoHangPokoyura = false;

                if (PlayerPrefs.HasKey("PokoyuraDeploy") == false)
                {
                    if (ManagerData._instance._pokoyuraData.Count < ManagerLobby._instance._spawnPokogoroPosition.Length)
                    {
                        autoHangPokoyura = true;
                    }
                }

                if (autoHangPokoyura)
                {
                    PlayTriggerWakeUp("Extend_pokoura");
                    yield return new WaitUntil(() => _state != TypeLobbyState.TriggerEvent);
                    
                }
                else
                {
                    _activeGetPokoyura = 0;
                    PokoyuraData.SetUserData();

                    var selectorPopup = ManagerUI._instance.OpenPopupPokoyuraSelector(false);
                    yield return new WaitUntil(() => selectorPopup == null);
                    yield return new WaitUntil(() => _state != TypeLobbyState.TriggerEvent);
                }
            }
            else
            {
                PokoyuraData.SetUserData();
                ReMakePokoyura();
            }
            _activeGetPokoyura = 0;
        }
    }

    public void ReMakeGiftbox()
    {
        if (ManagerLobby.landIndex != 0)
            return;

        while (ObjectGiftbox._giftboxList.Count > 0)
            DestroyImmediate(ObjectGiftbox._giftboxList[0].gameObject);


        //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@ServerRepos.GiftBoxes.Count  " + ServerRepos.GiftBoxes.Count);

        // 위치를 가지고 있는 아이들은 저장된 위치에 생성
        for (int i = 0; i < ServerRepos.GiftBoxes.Count; i++)
        {
            ServerUserGiftBox data = ServerRepos.GiftBoxes[i];

            if (PlayerPrefs.HasKey("Giftbox" + data.index))
            {
                ObjectGiftbox giftbox = Instantiate<ObjectGiftbox>(_objGiftBox);
                int index = PlayerPrefs.GetInt("Giftbox" + data.index);
                _spawnGiftBoxPosition[index].used = true;
                giftbox.transform.position = _spawnGiftBoxPosition[index].position;
                giftbox.InitGiftBox(data);
            }
        }
        // 위치가 없던 아이들은 빈 위치에 랜덤하게 들어가고 위치 저장
        for (int i = 0; i < ServerRepos.GiftBoxes.Count; i++)
        {
            ServerUserGiftBox data = ServerRepos.GiftBoxes[i];
            //GiftBoxUserData data = ManagerData._instance._giftBox[i];
            if (!PlayerPrefs.HasKey("Giftbox" + data.index))
            {
                ObjectGiftbox giftbox = Instantiate<ObjectGiftbox>(_objGiftBox);
                int count = 0;
                int index = Random.RandomRange(0, _spawnGiftBoxPosition.Length - 1);
                while (true)
                {

                    if (_spawnGiftBoxPosition[index].used == false)
                    {
                        PlayerPrefs.SetInt("Giftbox" + data.index, index);
                        _spawnGiftBoxPosition[index].used = true;
                        giftbox.transform.position = _spawnGiftBoxPosition[index].position;
                        giftbox.InitGiftBox(data);
                        break;
                    }

                    index++;
                    if (index >= _spawnGiftBoxPosition.Length)
                        index = 0;

                    count++;
                    if (count > _spawnGiftBoxPosition.Length)
                        break;
                }
            }
        }
    }
    public void ReMakeMaterial()
    {
        if (ManagerLobby.landIndex != 0)
            return;

        while (ObjectMaterial._materialList.Count > 0)
            DestroyImmediate(ObjectMaterial._materialList[0].gameObject);



        foreach (var item in ManagerData._instance._materialSpawnData)
        {
            if (item.materialCount > 0)
            {
                if (PlayerPrefs.HasKey("Material" + item.index))
                {
                    ObjectMaterial material = Instantiate<ObjectMaterial>(_objMaterial);
                    material._data = item;
                    int index = PlayerPrefs.GetInt("Material" + item.index);
                    _spawnMaterialPosition[index].used = true;
                    material.transform.position = _spawnMaterialPosition[index].position;
                }
            }
        }
        foreach (var item in ManagerData._instance._materialSpawnData)
        {
            if (item.materialCount > 0)
            {
                if (!PlayerPrefs.HasKey("Material" + item.index))
                {
                    ObjectMaterial material = Instantiate<ObjectMaterial>(_objMaterial);
                    material._data = item;
                    int count = 0;
                    int index = Random.Range(0, _spawnMaterialPosition.Length - 1);
                    while (true)
                    {

                        if (_spawnMaterialPosition[index].used == false)
                        {
                            PlayerPrefs.SetInt("Material" + item.index, index);
                            _spawnMaterialPosition[index].used = true;
                            material.transform.position = _spawnMaterialPosition[index].position;
                            break;
                        }

                        index++;
                        if (index >= _spawnMaterialPosition.Length)
                            index = 0;

                        count++;
                        if (count > _spawnMaterialPosition.Length)
                        {
                            material.transform.position = _spawnMaterialPosition[0].position;
                            break;

                        }
                    }
                }
            }
            
        }

        ManagerUI._instance.RefreshLimitedMaterialRegenForecast();
    }

    public void ReMakeMaterialbox()
    {
        if (ManagerLobby.landIndex != 0)
            return;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen) return;

        while (ObjectMaterialbox._materialboxList.Count > 0)
            DestroyImmediate(ObjectMaterialbox._materialboxList[0].gameObject);

        if(AdManager.ADCheck(AdManager.AdType.AD_8))
        {
            ObjectMaterialbox giftbox = Instantiate<ObjectMaterialbox>(_objMaterialBox);
            giftbox.transform.position = _spawnMaterialBoxPosition[0].position;
        }
    }

    public void ResetTriggerWakeUp(string in_triggerName)
    {
        TriggerScene trigger = null;
        if (ManagerArea._instance._extendTrigger.TryGetValue(in_triggerName, out trigger))
        {
            trigger._triggerWait.gameObject.SetActive(true);
            trigger._triggerWakeUp.Reset();
            trigger._triggerWakeUp.gameObject.SetActive(false);
        }
    }

    public void PlayTriggerWakeUp(string in_triggerName)
    {
        TriggerScene trigger = null;
        if (ManagerArea._instance._extendTrigger.TryGetValue(in_triggerName, out trigger))
        {
            LobbyBehavior._instance.ResetSelectBehavior();
            LobbyBehavior._instance.CancleBehavior();
            
            //앞에서 실행되는 이벤트 끝날때까지 기다렸다 재생되도록 수정해야함.
            ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
            ManagerCinemaBox._instance.OnBox($"TRG_{in_triggerName}", 1f, true, skippableScene : trigger.isSkippableScene);

            trigger._triggerWait.gameObject.SetActive(false);
            trigger._triggerWakeUp.gameObject.SetActive(true);
            trigger._triggerWakeUp.StartCondition();

            ManagerSound._instance.SetTimeBGM(96f);

            _state = TypeLobbyState.TriggerEvent;
        }
    }

    public void PlayExtendTriggerFinish(string in_triggerName)
    {
        TriggerScene trigger = null;
        if (ManagerArea._instance._extendTrigger.TryGetValue(in_triggerName, out trigger))
        {
            LobbyBehavior._instance.ResetSelectBehavior();
            LobbyBehavior._instance.CancleBehavior();

            trigger._triggerWait.gameObject.SetActive(false);
            trigger._triggerWakeUp.gameObject.SetActive(false);
            trigger._triggerActive.gameObject.SetActive(false);
            trigger._triggerFinish.gameObject.SetActive(true);
            trigger._triggerFinish.StartCondition();
        }
    }

    public void PlayScene_FromEditor<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T : AreaBase
    {
        StartCoroutine(CoPlayScene_FromEditor(in_area, in_scene, in_missionIndex, in_startSound));
    }

    public IEnumerator CoPlayScene_FromEditor<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T: AreaBase
    {
        ManagerCharacter._instance.AddLoadList(in_area._characters, in_area._costumeCharacters, in_area._live2dChars);

        yield return ManagerCharacter._instance.LoadCharacters();

        PlaySceneWakeUp(in_area, in_scene, in_missionIndex, in_startSound);
    }

    
   
    public void PlaySceneWakeUp<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T : AreaBase
    {
        LobbyEntryFocus.ResetFocusCandidates();

        ManagerAIAnimal.BeginTriggerControl();
        LobbyBehavior._instance.ResetSelectBehavior();
        LobbyBehavior._instance.CancleBehavior();

        if (in_area._listSceneDatas.Count >= in_scene && in_scene > 0)
        {
            var sceneData = in_area._listSceneDatas[in_scene - 1].sceneData;

            if (sceneData._triggerWakeUp.gameObject.active == false)
            {
                //앞에서 실행되는 이벤트 끝날 때 까지 기다렸다 재생되도록 수정해야함.
                ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
                string sceneId = $"TRG_{in_area.name}_SCN-{in_scene}";
                ManagerCinemaBox._instance.OnBox(sceneId, 1f, in_startSound, skippableScene: sceneData.isSkippableScene);
                sceneData._triggerWait.gameObject.SetActive(false);
                sceneData._triggerWakeUp.gameObject.SetActive(true);
                sceneData._triggerWakeUp.StartCondition();

                if( !in_area.IsEventArea() )
                {
                    if (in_scene != 1)
                    {
                        ManagerSound._instance.SetTimeBGM(in_area.sceneStartBgmOffset);

                        if (in_area.sceneStartBgmOff)
                            ManagerSound._instance?.PauseBGM();
                    }
                }
            }
        }
        lateClearMission = in_missionIndex;
        _state = TypeLobbyState.TriggerEvent;
    }

    public void PlaySceneWakeUp<T>(T in_area, List<int> listScene) where T : AreaBase
    {
        StartCoroutine(CoPlaySceneWakeUp(in_area, listScene));
    }

    private IEnumerator CoPlaySceneWakeUp<T>(T in_area, List<int> listScene) where T : AreaBase
    {
        for (int i = 0; i < listScene.Count; i++)
        {
            PlaySceneWakeUp(in_area, listScene[i]);
            yield return WaitForSceneEnd(in_area, listScene[i]);
        }
    }

    public IEnumerator WaitForSceneEnd<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T : AreaBase
    {
        PlaySceneWakeUp<T>(in_area, in_scene, in_missionIndex, in_startSound);
        while(_state == TypeLobbyState.TriggerEvent)
        {
            yield return null;
        }
        
        yield break;
    }

    public void ChageState(TypeLobbyState in_state)
    {
        if (_state == TypeLobbyState.TriggerEvent && in_state == TypeLobbyState.Wait)
        {
            StartCoroutine(CoChangeStateBeforeOffBox(in_state));

            CharacterBoni._boni.StopPath();
            AIChangeCommand command = new AIChangeCommand();
            command._state = AIStateID.eIdle;
            CharacterBoni._boni._ai.ChangeState(command);

            ManagerUI._instance.CoShowUI(0.5f, true, TypeShowUI.eAll);

            ManagerSound._instance.UnPauseBGM();

            CameraController._instance._cameraTarget.transform.position = CameraController._instance.GetCenterWorldPos();
            CameraController._instance._cameraTarget.velocity = Vector3.zero;
            
            // 투토리얼
            {
                if (Global.join && _tutorialLobbyMission_Play)
                {
                    _tutorialLobbyMission_Play = false;
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyMission);
                }
                
                // lateClearMission에 등록되는 미션의 경우 사용자가 어떤 미션을 먼저 클리어하냐에 따라 달라지므로 (4, 6, 7) lateClearMission 대신 bool 매개변수 전달
                if (lateClearMission <= 7)
                    ManagerUI._instance.NewInfoSetting(true);

                if (lateClearMission == 7)
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGetPlusHousing);


                if (ServerRepos.User.missionCnt == 5 && ObjectMaterial._materialList.Count>0)
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGetMaterial);

                if (lateClearMission == 9)
                {
                    Notice data = new Notice();
                    data.noticeIndex = 1;
                    ManagerUI._instance.OpenPopupNoticeHelp(data);
                }
            }
        }
    }

    private IEnumerator CoChangeStateBeforeOffBox(TypeLobbyState in_state)
    {
        yield return ManagerCinemaBox._instance.OffBox(2f);
        yield return ManagerAIAnimal.EndTriggerControl();
        _state = in_state;
    }

    public void EndDay()
    {
        _newDay = true;
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeNewDayLoading();
    }

    public void MoveLand(int landID, ReservedScene scn = null)
    {
        ManagerSound._instance.StopBGM();
        ManagerLobby.landIndex = landID;
        ManagerLobby.transLand = true;
        ManagerLobby._reservedScene = scn;
        if ( scn != null )
        {
            scn.newDay = ServerContents.Day;
            Global.day = scn.scnStartDay;
            ServerContents.UpdateCdnDay(new List<CdnDay> { _reservedScene.orgDay });
        }
        
        SceneLoading.MoveLand(landID);
    }

    public void MoveLand(int landID, System.Action serverAPI)
    {
        ManagerSound._instance.StopBGM();
        ManagerLobby.landIndex = landID;
        ManagerLobby.transLand = true;
        ManagerLobby._reservedHoisingChange = serverAPI;
        SceneLoading.MoveLand(landID);
    }

    /// <summary>
    /// 메인 랜드로 이동 후 추가 동작을 할 경우
    /// </summary>
    /// <param name="postAction"></param>
    public void MoveMainLand(System.Action postAction)
    {
        //메인 랜드 ID
        int landID = 0;

        ManagerSound._instance.StopBGM();
        ManagerLobby.landIndex = landID;
        ManagerLobby.transLand = true;
        ManagerLobby._reservedPostAction = postAction;

        SceneLoading.MoveLand(landID);
    }

    public Character MakeOrGetCharacter(TypeCharacterType in_Type, int costumeId, Vector3 pos, bool isMakeByTrigger = false)
    {
        // costumeId 가 -1 이면, Get 해보고 코스튬이 달라도 그냥 넘어가는거고
        // -1 이 아니면, 코스튬 다른거 입고있으면 갈아입힌다

        if (in_Type == TypeCharacterType.None)
            return null;
        // null 이면 만든다
        if (!_characterList.ContainsKey((int)in_Type))
        {
            if (costumeId == -1)
                costumeId = ManagerCharacter._instance.GetCostumeIdx(in_Type);

            return MakeCharacter(in_Type, costumeId, pos, isMakeByTrigger);
        }
        else
        {
            if( costumeId == -1)
                return _characterList[(int)in_Type];
            else
            {
                if( _characterList[(int)in_Type].costumeId == costumeId )
                {
                    return _characterList[(int)in_Type];
                }

                var charImportInfo = ManagerCharacter._instance.GetCharacter( (int)in_Type, costumeId);
                var orgChar = _characterList[(int)in_Type];
                var orgPos = orgChar.transform.position;
                orgPos.y = 0;

                DestroyImmediate(orgChar.gameObject);
                _characterList.Remove((int)in_Type);

                return MakeCharacter(in_Type, costumeId, orgPos, isMakeByTrigger);
            }
        }
    }

    //트리거에 의해 생성된 캐릭터인지 확인하는 함수.
    //(실제 로비에서 사용되고 있는 캐릭터 중 트리거에서 만들어진 캐릭터인지 판단할 때 사용)
    public bool CheckCharacter_MakeByTrigger(int characterType)
    {
        if (_characterList.ContainsKey(characterType) == false)
            return false;

        CharacterNPC npc = _characterList[characterType] as CharacterNPC;
        if (npc == null)
            return false;

        return npc.isMakeByTrigger;
    }

    #region 미션에서 사용할 캐릭터 등록
    //에리어에서 특정 미션에서 사용한다고 등록해놓은 캐릭터의 인덱스를 들고오는데 함수.
    public int GetCharacterIndex_UseCharacterRegisterList(TypeCharacterType characterType)
    {
        return listUseCharRegister.FindIndex(x => x._type == characterType);
    }

    //트리거에 의해 사용한다고 설정되어 있는 캐릭터인지 확인하는 함수.
    //(로비에 캐릭터가 있는지의 여부와 상관없이, 데이터에 캐릭터가 설정되어 있는지 판단할 때 사용)
    public bool IsUseCharacter_UseCharacterRegisterList(TypeCharacterType characterType)
    {
        int findIndex = GetCharacterIndex_UseCharacterRegisterList(characterType);
        if (findIndex == -1)
            return false;

        List<int> mission = listUseCharRegister[findIndex].listUseMission;
        for (int i = 0; i < mission.Count; i++)
        {
            if (ManagerData._instance._missionData[mission[i]].state == TypeMissionState.Active)
            {
                return true;
            }
        }
        return false;
    }

    public void AddCharacter_UseCharacterRegisterList(TypeCharacterType characterType, List<int> addMissionIdx)
    {
        int findIndex = GetCharacterIndex_UseCharacterRegisterList(characterType);
        if (findIndex == -1)
        {
            UseCharacterData_AtScene data = new UseCharacterData_AtScene()
            {
                _type = characterType,
                listUseMission = new List<int>(addMissionIdx)
            };
            listUseCharRegister.Add(data);
        }
        else
        {
            for (int i = 0; i < addMissionIdx.Count; i++)
            {
                if (listUseCharRegister[findIndex].listUseMission.FindIndex(x => x == addMissionIdx[i]) == -1)
                {
                    listUseCharRegister[findIndex].listUseMission.Add(addMissionIdx[i]);
                }
            }
        }
    }

    public void RemoveCharacter_UseCharacterRegisterList(TypeCharacterType characterType, List<int> removeMissionIdx)
    {
        int fIdx = GetCharacterIndex_UseCharacterRegisterList(characterType);
        if (fIdx != -1)
        {
            for (int i = 0; i < removeMissionIdx.Count; i++)
            {
                if (listUseCharRegister[fIdx].listUseMission.FindIndex(x => x == removeMissionIdx[i]) != -1)
                {
                    listUseCharRegister[fIdx].listUseMission.Remove(removeMissionIdx[i]);
                }
            }

            if (listUseCharRegister[fIdx].listUseMission.Count == 0)
            {
                listUseCharRegister.Remove(listUseCharRegister[fIdx]);
            }
        }
    }

    //트리거가 끝난 뒤, 미션의 상태 검사해서 클리어 된 미션 데이터를 제거해주는 함수
    public void SyncUseCharacterRegisterList()
    {
        List<UseCharacterData_AtScene> listRemoveData = new List<UseCharacterData_AtScene>();
        for (int i = 0; i < listUseCharRegister.Count; i++)
        {
            UseCharacterData_AtScene data = listUseCharRegister[i];

            //완료된 미션들은 리스트에서 제거해줌.
            List<int> listRemoveMission = new List<int>();
            for (int j = 0; j < data.listUseMission.Count; j++)
            {
                if (ManagerData._instance._missionData[data.listUseMission[j]].state == TypeMissionState.Clear)
                {
                    listRemoveMission.Add(data.listUseMission[j]);
                }
            }

            //제거될 미션이 있으면 지워질 데이터 항목에 추가
            if (listRemoveMission.Count > 0)
            {
                UseCharacterData_AtScene removeData = new UseCharacterData_AtScene()
                {
                    _type = data._type,
                    listUseMission = new List<int>(listRemoveMission)
                };
                listRemoveData.Add(removeData);
            }
        }

        //지워질 데이터들 지워줌.
        for (int i = (listRemoveData.Count - 1); i >= 0; i--)
        {
            ManagerLobby._instance.RemoveCharacter_UseCharacterRegisterList(listRemoveData[i]._type, listRemoveData[i].listUseMission);
        }
    }
    #endregion

    public Character GetCharacter(TypeCharacterType in_Type)
    {
        return MakeOrGetCharacter(in_Type, -1, Vector3.zero);
    }

    public bool IsCharacterExist(TypeCharacterType in_Type)
    {
        if (in_Type == TypeCharacterType.None)
            return false;
        // null 이면 만든다
        if (!_characterList.ContainsKey((int)in_Type))
        {
            return false;
        }
        return true;
    }

    public void RemoveCharacter(TypeCharacterType in_Type)
    {
        if (_characterList.ContainsKey((int)in_Type))
        {
            Destroy(_characterList[(int)in_Type].gameObject);
            _characterList.Remove((int)in_Type);
        }
    }

    List<GameObject> removeReservedCharacterList = new List<GameObject>();
    public void RemoveCharacter_Reserve(TypeCharacterType in_Type)
    {
        if (_characterList.ContainsKey((int)in_Type))
        {
            _characterList[(int)in_Type].gameObject.SetActive(false);
            removeReservedCharacterList.Add(_characterList[(int)in_Type].gameObject);
            _characterList.Remove((int)in_Type);
        }
    }

    public void FlushRemoveReservedCharacters()
    {
        for(int i = 0; i < removeReservedCharacterList.Count; ++i)
        {
            Destroy(removeReservedCharacterList[i]);
        }
    }

    public Character MakeCharacter(TypeCharacterType in_Type, Vector3 in_Pos)//, Vector3 vecPos, bool bForcePos = false, int nModelIndex = 1000, CHAT_SIDE Char_Direction = CHAT_SIDE.LEFT, float fForcePosDist = 5.0f, float fTransparentTime = 0.5f)
    {
        if (_characterList.ContainsKey((int)in_Type))
        {
            return _characterList[(int)in_Type];
        }
        Character character = null;
        
        var charData = ManagerCharacter._instance.GetCharacter((int)in_Type, 0);
        character = Instantiate(charData.obj).GetComponent<Character>();
        character.tapSound = charData.tapSound;
        character._heightOffset = charData.characterHeightOffset;

        character.SetFallbackSound();

        character.costumeId = 0;

        if (character != null)
            character._transform.position = in_Pos;

        _characterList.Add((int)in_Type, character);

        return character;
    }

    public Character MakeCharacter(TypeCharacterType in_Type, int costumeId, Vector3 in_Pos, bool isMakeByTrigger = false)
    {
        if (_characterList.ContainsKey((int)in_Type))
        {
            return _characterList[(int)in_Type];
        }

        Character character = null;
        var charData = ManagerCharacter._instance.GetCharacter((int)in_Type, costumeId);

        if( charData == null )
        {   // 테스트중에 리소스 없는 코스튬 실수로 선택한 경우 로그인 안되는 사태를 방지하기 위해
            charData = ManagerCharacter._instance.GetCharacter((int)in_Type, 0);
            costumeId = 0;
        }
        character = Instantiate(charData.obj).GetComponent<Character>();
        character.tapSound = charData.tapSound;
        character._heightOffset = charData.characterHeightOffset;

        character.SetFallbackSound();
        character.costumeId = costumeId;

        if (character != null)
            character._transform.position = in_Pos;

        CharacterNPC npc = character as CharacterNPC;
        if (npc != null)
            npc.isMakeByTrigger = isMakeByTrigger;

        _characterList.Add((int)in_Type, character);

        return character;
    }

    public void SetCostume(int costumeId)
    {
        // NOTE: 2019 만우절
        if (ServerRepos.LoginCdn.aprilFool != 0)
        {
            costumeId = ServerRepos.LoginCdn.aprilFool;
        }

        StartCoroutine(CoChangeCostume(costumeId));
    }

    IEnumerator CoChangeCostume(int costumeId)
    {
        if(ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId) == null)
        {
            yield return CoLoadCostume(costumeId);
        }

        var charImportInfo = ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId);
        var c = Instantiate(charImportInfo.obj).GetComponent<Character>();
        c.costumeId = costumeId;
        var orgChar = GetCharacter(TypeCharacterType.Boni);
        RemoveCharacter(TypeCharacterType.Boni);
        c.transform.position = orgChar.transform.position;
        _characterList[(int)TypeCharacterType.Boni] = c;

        yield return new WaitForSeconds(0.1f);
        c._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);
    }

    IEnumerator CoLoadCostume(int costumeId)
    {
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            ManagerCharacter._instance.LoadFromInternal(TypeCharacterType.Boni, costumeId);
#endif
        }
        else
        {
            if(ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId) == null)
            {
                yield return ManagerCharacter._instance.LoadFromBundle(TypeCharacterType.Boni, costumeId);
            }
        }
    }

    public void ExposePokoyuraTree()
    {
        StartCoroutine(CoExposePokoyuraTree());

    }
    

    public IEnumerator CoExposePokoyuraTree()
    {
        if (ServerRepos.User.missionCnt < 9)
            yield break;

            Character._boni.StartPath(new Vector3(-64f, 0f, -7f), null, true);

        while (Character._boni._ai.GetStateID() != AIStateID.eIdle) {
            yield return new WaitForSeconds(0.1f);
        }

        if (Vector3.Distance(new Vector3(-64f, 0f, -7f), Character._boni.gameObject.transform.position) > 0.1f)
            yield break;

        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);

        Character._boni._ai.BodyTurn(TypeCharacterDir.Right);
        yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(0.1f);
        Character._boni._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);
        yield break;
    }
    
    public void OpenPopupLobbyMission(int in_missionIndex, Vector3 in_positoin)
    {
        bool missionStopped = ServerRepos.LoginCdn.LimitMission != 0 && (ServerRepos.User.missionCnt >= ServerRepos.LoginCdn.LimitMission);
        if(missionStopped)
        {
            return;
        }

        ObjectMissionIcon icon = Instantiate(_objMissionIcon).GetComponent<ObjectMissionIcon>();
        icon.InitLobbyMission(in_missionIndex, in_positoin);
    }
    public void OpenHousingIcon(ObjectBase in_obj, int housingIdx, Vector3 in_position)
    {
        _housingIcon.transform.position = in_position + new Vector3(2f,0f,-2f);
        _housingIcon.gameObject.SetActive(true);
        _housingIcon.InitHousingIcon(in_obj, housingIdx, in_position + new Vector3(2f, 0f, -2f));
    }

    void OnApplicationPauseReboot()
    {
        if (UIPopupSystem._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }

    public static void InvalidateBundles()
    {
        _assetBankHousing.Clear();
        _assetBankEvent.Clear();
    }

    private void DownLoadAndCheckAreaIllustration()
    {
        if (Global.day > 1)
        {
            ResourceManager.LoadCDN
            (
                "CachedResource",
                $"AreaIllustration_{Global.day - 1}.png",
                (Texture2D texture) => ResourceManager.UnLoad(texture)
            );
        }

        ResourceManager.LoadCDN
        (
            "CachedResource",
            $"AreaIllustration_{Global.day}.png",
            (Texture2D texture) => ResourceManager.UnLoad(texture)
        );
    }

    //오픈된 에리어 목록에서 원하는 에리어 인덱스 가져오기.
    public int GetOpenAreaListIndex(int areaIdx)
    {
        return listOpenAreaIndex.FindIndex(x => x == areaIdx);
    }

    void OnDrawGizmos()
    {
        if (_spawnMaterialPosition != null)
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < _spawnMaterialPosition.Length; i++)
            {
                Gizmos.DrawWireSphere(_spawnMaterialPosition[i].position, 0.3f);
                PokoUtil.drawString("<" + i + ">", _spawnMaterialPosition[i].position, 0f, 0f, Color.white);
            }
            
        }
        if (_spawnGiftBoxPosition != null)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < _spawnGiftBoxPosition.Length; i++)
            {
                PokoUtil.drawString("<" + i + ">", _spawnGiftBoxPosition[i].position, 0f, 0f, Color.green);
                Gizmos.DrawWireSphere(_spawnGiftBoxPosition[i].position, 0.3f);
            }

        }
        if (_spawnPokogoroPosition != null)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < _spawnPokogoroPosition.Length; i++)
            {
                PokoUtil.drawString("<" + i + ">", _spawnPokogoroPosition[i].position, 0f, 0f, Color.yellow);
                Gizmos.DrawWireSphere(_spawnPokogoroPosition[i].position, 0.3f);
            }

        }
        if (_spawnMaterialBoxPosition != null)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < _spawnMaterialBoxPosition.Length; i++)
            {
                PokoUtil.drawString("<" + i + ">", _spawnMaterialBoxPosition[i].position, 0f, 0f, Color.blue);
                Gizmos.DrawWireSphere(_spawnMaterialBoxPosition[i].position, 0.3f);
            }
        }
        if (Global._touchBegin)
        {
            //   Debug.Log("ddddd");
        }
    }

    public void MoveCameraMaterial()
    {
        var material = FindObjectOfType<ObjectMaterial>();

        if (material == null)
        {
            string title = Global._instance.GetString("p_t_4");
            string message = Global._instance.GetString("n_s_25");
            string buttonTitle = Global._instance.GetString("btn_1");
            UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupNotice.InitSystemPopUp(title, message, false, null);
            popupNotice.SetButtonText(1, buttonTitle);
            popupNotice.HideCloseButton();
        }
        else
        {
            ManagerUI._instance.ClosePopUpUI();
            CameraController._instance.MoveToPosition(material.transform.position, 0.5f);
        }
    }

#if UNITY_EDITOR
    public static Dictionary<string, string> areaRedirect = new Dictionary<string, string>();

    void ApplyAreaRedirect(ref string prefabPath)
    {
        if (areaRedirect.Count > 0)
        {
            foreach(var r in areaRedirect)
            {
                prefabPath = prefabPath.Replace(r.Key, r.Value);
            }
        }
    }
#endif

    private class WaitNoPopup : CustomYieldInstruction
    {
        /// <summary>
        /// 모든 팝업이 닫히고 경과된 시간
        /// </summary>
        private float closeTime = 0.0f;

        /// <summary>
        /// 모든 팝업이 닫히고 대기할 시간(해당 시간 만큼 대기한 뒤에도 팝업이 모두 닫혀 있다면, 모든 팝업이 닫힌 것으로 판단 합니다)
        /// </summary>
        private readonly float waitTime;

        public WaitNoPopup(float waitTime = 0.5f)
        {
            this.waitTime = waitTime;
        }

        public override bool keepWaiting
        {
            get
            {
                if(ManagerUI._instance == null)
                {
                    return false;
                }
                else
                {
                    if(ManagerUI._instance.GetPopupCount() == 0)
                    {
                        closeTime += Global.deltaTimeLobby;
                    }
                    else
                    {
                        closeTime = 0.0f;
                    }

                    return closeTime < waitTime;
                }
            }
        }
    }
}
