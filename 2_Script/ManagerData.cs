using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Protocol;
using ServiceSDK;

public enum DataLoadState
{
    eNone,

    eStart,
    eLineInitialize,
    eLineLogin,         // LineSDK
    eLineUserData,      
    eLineFriendsData,
    eLineInviteFriendsData,
    eLineProfileData,
    eLineRankingData,



    eUserLogin,
    eGameData,      // DB
    eResourceData,

    eComplete,
}

public class ManagerData : MonoBehaviour {

    public static ManagerData _instance = null;

    public enum PromotionInitializeState
    {
        NONE,
        INITIALIZING,
        COMPLETE,
    }

    private class LoadingRebootRAII : System.IDisposable
    {
        private bool disposed = false;
        public LoadingRebootRAII()
        {

            
        }

        public void Done() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this); 
        }
        protected virtual void Dispose(bool disposing)
        {
            //Debug.Log("Dispose LoadingRebootRAII " + disposing); 
            if (disposed)
                return; 

            disposed = true;
            if (disposing) {
                Global.ReBoot();
            }
        }
    }
    
#region Public
    
    [System.NonSerialized]
    public DataLoadState _state = DataLoadState.eNone;
    
    // 스테이지, 챕터
    public int               maxStageCount    = 0;
    public List<ChapterData> chapterData      = new List<ChapterData>();
    public List<StageData>   _stageData       = new List<StageData>();
    public List<int>         StageVersionList = new List<int>();

    public SortedDictionary<ScoreFlowerType, List<int>> listGetFlowersReward = new SortedDictionary<ScoreFlowerType, List<int>>();

    // 이벤트 챕터
    public EventChapterData _eventChapterData = new EventChapterData();
    
    // 씬(미션)의 게임데이타 유저데이타
    public Dictionary<int, MissionData> _missionData = new Dictionary<int, MissionData>();
    
    // 퀘스트
    public Dictionary<int, QuestGameData> _questGameData = new Dictionary<int,QuestGameData>();
    
    // 챕터 미션 퀘스트
    public Dictionary<int, ChapterMission> _chapterMissionData = new Dictionary<int, ChapterMission>();
    
    // 재료
    public List<MaterialData>          _meterialData          = new List<MaterialData>();
    public List<MaterialSpawnUserData> _materialSpawnData     = new List<MaterialSpawnUserData>();
    public Dictionary<int, long>       _materialSpawnProgress = new Dictionary<int, long>();

    // 포코코로
    public List<PokoyuraData> _pokoyuraData = new List<PokoyuraData>();
    
    // message
    public List<MessageData> _messageData = new List<MessageData>();
    
    //코스튬.
    public Dictionary<string, CdnCostume> _costumeData = new Dictionary<string, CdnCostume>();
    
    //토큰 갱신이 완료되었는지 확인할 때 사용하는 변수.
    //(백그라운드에서 돌아왔을 때 토큰을 갱신하는데 이 때, 서버 통신이 진행된다면 토큰 갱신될 때까지 대기해야 함)
    public bool isRefreshedToken = true;
    
    public AndroidPermissionManager androidPermissionManager;
    
    //프로모션 sdk 초기화가 완료되었는지 확인할 때 사용.
    public static PromotionInitializeState promotionState = PromotionInitializeState.NONE;
    
#endregion
    
#region Private

    //로그인시때만 사용
    private ProcessManager process = null;
    
    //인증중인데 다시 인증하지 않게 플래그 설정
    //ServiceSDK.ServiceSDKManager.instance.IsAuthorizing() 으로 확인해도 될것같은데 11월19일 서비스전 아직 라인측에서 해당 api 확인이 안되어 대신 사용
    //라인측에서 해당 api사용해도 된다고 하면 바꾸면 될것같음 테스트는 해봤음
    private bool _isAuthorizing = false;
    
    //GetUserStageList 통신 성공 여부
    private bool recvGameList_complete = false;
    
    //프로모션 sdk, 앱 실행 시 최초 한번만 초기화 진행하면 되기 때문에 static으로 설정
    private static InitializeCallbackImpl completeCallback;

#endregion
  
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        //HashSet<int> 변환이 가능하도록 역직렬화(Deserialize) 유형 추가
        Newtonsoft.Json.Utilities.AotHelper.EnsureList<int>();
    }

    // 미성년자 보호 가이드라인 스플래시와 권한 및 알림 허용 팝업이 겹치지 않도록 사용
    public static bool IsMinorPrefabLoading;
    
	// Use this for initialization
    private IEnumerator Start() {

        Global._pendingReboot = false;
        IsMinorPrefabLoading = false;

        // 누가 시작 상태로 바꿔야 진행 (타이틀 로딩 씬에서 또는 로비 씬에서 바로 시작할때)
        if(ManagerUI._instance != null)
        { 
            ManagerUI._instance.topUiPanel.gameObject.SetActive( false );
        }

        while (true)
        {
            if (_state == DataLoadState.eStart)
                break;
            yield return null;
        }

        ManagerObjectBank._instance.MakeStart();
        while (true)
        {
            if (ManagerObjectBank._instance._complete)
                break;
            yield return null;
        }
        Global.join = false;
        ServerAPI.Reset();

        yield return new WaitUntil(() => IsMinorPrefabLoading);
        LocalNotification.Reset();
        
#if UNITY_ANDROID && !UNITY_EDITOR
        // 안드로이드 권한 요청 팝업 출력(푸시 알림 권한)
        if (androidPermissionManager == null)
        {
            androidPermissionManager = new AndroidPermissionManager();
        }
        yield return androidPermissionManager.CoCheckAndroidPermission();
#endif

        ///////////////////////////////////////////////////////////////////////////////////
        SetupLoginProcess();
            
        process.Process( );
        yield return new WaitUntil(() => _state == DataLoadState.eGameData);

        ReleaseLoginProcess();
        ///////////////////////////////////////////////////////////////////////////////////

        // CDN 및 데이터 셋팅
        {
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.CDN_DOWNLOAD_S);

            // fileList2.json 로드 및 설정
            yield return HashChecker.Init();

            // 저장된 캐시가 해당 버전 보다 낮으면 캐시된 데이타를 모두 날린다(서비스중 잘못된 버전을 올려 파일이 꼬였을때 긴급하게 활용)
            EmergencyResetCache();

            // 서버에서 받은 유저 스테이지 정보를 프로필 정보에 셋팅
            Global.UpdateNormalGameProgress((int)GameData.User.stage);

            // 클로버, 탐험 날개 자동생성 그로시 전송
            SendAutoRegenerationLog();

            // Global 데이터 셋팅
            Global._cdnAddress = NetworkSettings.Instance.GetCDN_URL() + "/"; // MUST
            Global.star        = (int)GameData.Asset.Star;
            Global.day         = (int)GameData.User.day;
            Global.clover      = (int)GameData.Asset.AllClover;
            Global.coin        = (int)GameData.Asset.AllCoin;
            Global.jewel       = (int)GameData.Asset.AllJewel;
            Global.flower      = (int)GameData.User.flower;
            Global.wing        = (int)GameData.Asset.AllWing;
            Global.exp         = (int)GameData.User.expBall;

            // 포코유라 업데이트 (랭킹 버튼).
            ManagerUI._instance.SettingRankingPokoYura();

            ManagerAssetLoader._cdnURL = NetworkSettings.Instance.GetCDN_URL() + "/" + ManagerAssetLoader.GetBundlePath() + "/";

            Global.LoadRankingPointGrade();

            // 챕터, 스테이지 데이터 셋팅
            {
                maxStageCount = 0;   // 오픈된 최종 스테이지
                _stageData.Clear();  // 스테이지 데이터 리스트
                chapterData.Clear(); // 챕터 데이터 리스트
                StageVersionList.Clear();

                Dictionary<int, List<int>> tempStageVer = new Dictionary<int, List<int>>();
                foreach (var item in ServerContents.Chapters)
                {
                    ChapterData chapter = new ChapterData();
                    chapterData.Add(chapter);
                    tempStageVer.Add(item.Value.index, item.Value.stageVersions);
                }

                foreach (var item in ServerContents.Chapters)
                {
                    ChapterData chapter = chapterData[item.Key - 1];
                    chapter._cIndex     = item.Value.index;
                    chapter._stageIndex = item.Value.stageIndex;
                    chapter._stageCount = item.Value.stageCount;

                    chapter.allWhiteFlowerReward = item.Value.allWhiteFlowerReward;
                    chapter.allBlueFlowerReward  = item.Value.allBlueFlowerReward;
                    chapter.allPokoFlowerReward  = item.Value.allPokoFlowerReward;

                    maxStageCount += chapter._stageCount;
                    for (int i = 0; i < item.Value.stageCount; i++)
                    {
                        StageData stage = new StageData();
                        _stageData.Add(stage);
                    }
                }

                for (int i = 1; i < tempStageVer.Count + 1; i++)
                {
                    List<int> tempVer = new List<int>();
                    tempStageVer.TryGetValue(i, out tempVer);

                    foreach (var temp in tempVer)
                    {
                        StageVersionList.Add(temp);
                    }
                }

                // 이벤트 챕터 데이터 설정
                EventChapterData.SetUserData();

                // 서버에서 스테이지 정보를 받아온 뒤 셋팅 //모든 스테이지를 클리어 했으면 꽃피우게 하기 위해서 스테이지 정보 읽기
                ServerAPI.GetUserStageList(recvGameList);
                yield return new WaitUntil(() => recvGameList_complete == true);

                // 스테이지 파란꽃 보상 정보 / UI 알림 갱신.
                RefreshFlowerRewardData();
                bool bActiveStageAlarm = _instance.listGetFlowersReward.Count > 0;
                ManagerUI._instance.SettingStageNewIcon(bActiveStageAlarm);
            }

            // 미션, 퀘스트
            {
                _missionData.Clear();
                foreach (var item in ServerContents.Missions)
                {
                    MissionData mission = new MissionData
                    {
                        index            = item.Key,
                        day              = item.Value.day,
                        sceneArea        = item.Value.sceneArea,
                        sceneIndex       = item.Value.sceneIndex,
                        sceneIndexWakeup = item.Value.sceneIndexWakeup,
                        needStar         = item.Value.needStar,
                        stepClear        = item.Value.stepClear,
                        waitTime         = item.Value.waitTime
                    };

                    if (mission.day < Global.day)
                    {
                        mission.state = TypeMissionState.Clear;
                    }
                    else if (mission.day == Global.day)
                    {
                        mission.state = TypeMissionState.Inactive;
                    }
                    else
                    {
                        mission.state = TypeMissionState.Inactive;
                    }

                    _missionData.Add(item.Key, mission);
                    //Debug.Log(item.Key + "___" + item.Value.day + "  " + item.Value.stepClear);
                }

                foreach (var item in ServerRepos.OpenMission)
                {
                    if (_missionData.ContainsKey(item.idx))
                    {
                        _missionData[item.idx].state      = (TypeMissionState)item.state;
                        _missionData[item.idx].clearCount = item.clearCount;
                        _missionData[item.idx].clearTime  = item.clearTime;
                        if (item.clearTime > 0)
                        {
                            LocalNotification.TimeMissionNotification(item.idx, (int)item.clearTime - (int)GameData.GetTime());
                        }
                    }
                    //Debug.Log(item.idx + "_미션__" + _missionData[item.idx].state);
                }

                if (Global.join)
                {
                    ManagerData._instance._missionData[1].state = TypeMissionState.Inactive;
                }

                //챕터 미션 세팅
                ChapterMission.SetUserData();

                // 퀘스트
                QuestGameData.SetUserData();
            }

            // 하우징(데코)
            {
                //  Debug.Log("Housings 컨텐츠" + ServerContents.Housings.Count);
                HousingSetInfoUtility.LoadData();
                yield return new WaitWhile(() => HousingSetInfoUtility.IsLoading);

                ManagerHousing.BuildInfo();
                ManagerHousing.SyncUserHousing();
            
                // 이벤트임을 검사하는 변수(하우징에서 사용).
                bool bEvent = ManagerHousing.GetUnfinishedEventItemCount() > 0;
                if (bEvent == true)
                {
                    ManagerUI._instance.SettingEventIcon(bEvent);
                }
            }
            
            // 친구 초대, 친구 깨우기 이벤트 설정
            bool socialEventIcon = ServerRepos.LoginCdn.PerInviteRewardEvent == 1 || (ServerContents.WakeupEvent != null && ServerContents.WakeupEvent.event_index != 0);
            ManagerUI._instance.SettingInviteEvent(socialEventIcon);

            // UI 갱신 (재화, 메시지함, 플레이 버튼, 이벤트 재료 아이콘 등...)
            if (ManagerUI._instance != null)
            {
                ManagerUI._instance.UpdateUI();
            }

            PlusHousingModelData.SetUserData();
            HousingUserData.SetUserData();
            MaterialData.SetUserData();
            PokoyuraData.SetUserData();

            // 코스튬 데이터 초기화.
            InitCostumeData();

            // 재료 데이터 초기화.
            InitMaterialSpawnData();

            // 클로버 전송 쿨타임 설정,로그인시 랭킹창을 바로 열어줘야 하므로 로그인 할때 읽어야함, 랭킹창에 보낸 친구들 또 못보내게 시간 표시하기 위해
            foreach (var item in ServerRepos.UserCloverCoolTimes)
            {
                if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.fUserKey, out UserFriend data))
                {
                    data.CloverCoolTime = item.sendCoolTime;
                }
            }
            
            // 로비에 놓인 재료 리스트
            //Debug.Log("SpawnMaterial 로비재료" + ServerRepos.SpawnMaterial.Count);
            MaterialSpawnUserData.SetUserData();

            // 로비에 놓인 선물 상자
            //Debug.Log("GiftBoxes 로비선물상자" + ServerRepos.GiftBoxes.Count);
            for (var i = 0; i < ServerRepos.GiftBoxes.Count; i++)
            {
                LocalNotification.GiftBoxNotification(ServerRepos.GiftBoxes[i].index, (int)ServerRepos.GiftBoxes[i].openTimer - (int)GameData.GetTime());
            }

            yield return CheckUserDataIntegrity();

            // 탐험모드 매니저
            if (ManagerAdventure.CheckStartable())
            {
                bool advInitComplete = false;
                ManagerAdventure.OnInit((b) => { advInitComplete = true; });
                while (advInitComplete == false)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            // RewardURL 보상 즉시 지급 로그 전송.
            if (ServerRepos.ItemDelivery != null)
            {
                foreach (var reward in ServerRepos.ItemDelivery)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int) reward.type,
                        reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_REWARD_URL,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_REWARD_URL,
                        "RewardURL"
                    );
                }
            }
            
            // 리소스 로드 매니저 초기화
            ManagerResourceLoader.Init();
            
            // 이벤트 매니저 초기화
            ManagerEvent.Init();
            
            _state = DataLoadState.eResourceData;

            // [AssetBundles]
            {
                SceneTitle.resourceProgress   = 0;
                SceneTitle.resourceDownloaded = 0;
                SceneTitle.resourceTotal      = 0;

                List<string> areaNameList = ManagerLobby.landIndex == 0 ? ServerContents.Day.GetString() : ServerContents.Day.outlands[ManagerLobby.landIndex];

                List<ManagerAssetLoader.BundleRequest> bundleLoadList = new List<ManagerAssetLoader.BundleRequest>();
                ManagerAssetLoader.EstimatedLoadResult loadEstimate   = new ManagerAssetLoader.EstimatedLoadResult();

                yield return TitleBundle.CheckBundleAndDownload(ManagerAssetLoader._cdnURL, GameData.LoginCdn.titleImageVer);

                string loadFailedBundleString = "";

                // [AssetBundle] 이벤트 로비 오브젝트
                ManagerEvent.instance.OnBundleLoadPhase(bundleLoadList, (ManagerAssetLoader.ResultCode r, string errStr) => { loadFailedBundleString += r.ToString() + errStr; });

                // [AssetBundle] Area
                for (int i = 0; i < areaNameList.Count; i++)
                {
                    string areaName = areaNameList[i];

                    ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
                    {
                        uri             = areaNameList[i],
                        successCallback = null,
                        failCallback = (r) =>
                        {
                            Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                            loadFailedBundleString += areaName + "\n";
                        }
                    };
                    bundleLoadList.Add(bundleReq);
                }

                // [AssetBundle] 선택된 하우징
                ManagerHousing.RequestLoadSelectedModels(bundleLoadList, loadFailedBundleString);

                // [AssetBundle] 탐험 모드
                if (ManagerAdventure.CheckStartable())
                {
                    string assetName  = "adv_ent";
                    ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
                    {
                        uri             = assetName,
                        successCallback = null,
                        failCallback = (r) =>
                        {
                            Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                            loadFailedBundleString += assetName + "\n";
                        }
                    };
                    bundleLoadList.Add(bundleReq);
                }

                yield return ManagerAssetLoader._instance.EstimateLoad(bundleLoadList, loadEstimate);
                Debug.Log("Estimated: " + loadEstimate.totalDownloadLength);
                SceneTitle.resourceTotal = loadEstimate.totalDownloadLength;

                if (Global.LoadFromInternal == false)
                {
                    using (var raii = new LoadingRebootRAII())
                    {
                        yield return ManagerAssetLoader._instance.ExecuteLoad(loadEstimate, bundleLoadList,
                            (f) =>
                            {
                                SceneTitle.resourceDownloaded = (long)(f * SceneTitle.resourceTotal);
                                SceneTitle.resourceProgress   = (int)(f  * 100f);
                            }
                        );

                        if (loadFailedBundleString.Length > 0)
                        {
                            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                            {
                                var go = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                                go.InitServerSystemPopUp(
                                    Global._instance.GetString("p_t_4"),
                                    $"{Global._instance.GetString("n_er_8")}\n{loadFailedBundleString}",
                                    true,
                                    isError: true
                                );
                                go.SetResourceImage("Message/error");
                                go.SetButtonText(1, "Reboot");
                                go.SetButtonText(2, "GoAhead");
                                go.actionYesCallback = () => { Global.ReBoot(); };
                                go.actionNoCallback  = () => { };
                            }
                            else
                            {
                                ErrorController.ShowNetworkErrorDialog("Error", "[Download Resource Failed]\n" + loadFailedBundleString, true);
                            }
                        }

                        raii.Done();
                    }
                }
            }
            
            yield return null;
        }
        
        ServiceSDK.ServiceSDKManager.instance.LoadGrowthyUserInfo();
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.CDN_DOWNLOAD_E);

        _state = DataLoadState.eComplete;
        
        yield return null;

        //캔디, 오리 퀘스트 데이터가 꼬인 유저를 위한 데이터 수복 기능 (1회성 동작)
        QuestDataCheckAndFix(_questGameData);
    }

    private void Update () {
        // 반드시 호출 (Scheduler가동)
        if (_state == DataLoadState.eComplete)
        {

        }
    }
    
    private long deactiveTime = 0;
    private void OnApplicationPause(bool pause)
    {
        if (Global._onlineMode && _state == DataLoadState.eComplete)
        {
            // 활성화
            if (!pause)
            {
                //Debug.Log("==== " + (Global.GetTime() - deactiveTime) + "    " + GameData.LoginCdn.ReBoot);
                if (deactiveTime > 0 && (Global.GetTime() - deactiveTime) > (60 * GameData.LoginCdn.ReBoot) && Application.loadedLevelName == "Lobby")
                {
                    var clientCdnVer = ServerRepos.LoginCdn.ID;
                    ServerAPI.RebootCheck(clientCdnVer, ServerCdnId);
                }
                else
                {
                    if (NetworkLoading._tempInstance == null)
                    {
                        if (Application.platform != RuntimePlatform.WindowsEditor &&
                            Application.platform != RuntimePlatform.OSXEditor     &&
                            Application.platform != RuntimePlatform.WindowsPlayer &&
                            NetworkSettings.Instance.enableLiAppSecureTool)
                        {
                            ServiceSDK.ServiceSDKManager.instance.ReStartLiapp();
                        }

                        CheckSystemState();
                    }
                }

#if UNITY_IOS && !UNITY_EDITOR
                if (completeCallback != null && ManagerData.promotionState == PromotionInitializeState.COMPLETE)
                {
                    completeCallback.ResetUrl();
                    completeCallback.SendDeeplink();
                }
#endif

                LocalNotification.CancelAllNotification();
            }
            else // 비활성화
            {
                LocalNotification.RegisterNotifications();
                deactiveTime = Global.GetTime();

                //그로씨 로그 전송
                ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();
            }
        }
    }
    
    public static Dictionary<int, MissionData> BackupMissionData()
    {
        var ret = new Dictionary<int, MissionData>();
        foreach ( var d in ManagerData._instance._missionData ) 
        {
            ret.Add(d.Key, d.Value.Clone());
        }

        return ret;
    }
    
    /// <summary>
    /// PromotionSDK 초기화.
    /// </summary>
    public void InitializePromotionSDK()
    {
        if (completeCallback == null)
        {
            completeCallback = new InitializeCallbackImpl();
            completeCallback.InitializePromotionSDK();
        }
        else if (promotionState == PromotionInitializeState.NONE)
        {
            completeCallback.InitializePromotionSDK();
        }
        else
        {
            completeCallback.ResetUrl();
        }
    }
    
    public void StartNetwork() {
          
        if (_state == DataLoadState.eComplete)
            return;
        _state = DataLoadState.eStart;
    }
    
    public bool IsGetRewardState(int rewardState, int flowerType)
    {
        bool state = false;
        switch (flowerType)
        {
            case 3:
                state = (rewardState % 100 / 10) == 1;
                break;
            case 4:
                state = (rewardState % 10) == 1;
                break;
            case 5:
                state = (rewardState % 1000 / 100) == 1;
                break;
        }

        return state;
    }

    public void RefreshFlowerRewardData()
    {
        ManagerData._instance.listGetFlowersReward.Clear();
        int flowerCount = 0;

        for (int flowerIndex = 3; flowerIndex <= 5; flowerIndex++)
        {
            int chapterIdx       = 0;
            int chapterLastIndex = chapterData[chapterIdx]._stageIndex + chapterData[chapterIdx]._stageCount - 2;
            for (int i = 0; i < _stageData.Count; i++)
            {
                if ((chapterIdx <= ServerRepos.UserChapters.Count - 1) && IsGetRewardState(ServerRepos.UserChapters[chapterIdx].isGetBlueFlowerReward, flowerIndex)
                   )
                {
                    i = chapterLastIndex;
                }
                else
                {
                    if (_stageData[i]._flowerLevel > flowerIndex - 1)
                        flowerCount++;
                }

                if (i == chapterLastIndex)
                {
                    if (flowerCount == chapterData[chapterIdx]._stageCount)
                    {
                        if (listGetFlowersReward.ContainsKey((ScoreFlowerType)flowerIndex))
                            listGetFlowersReward[(ScoreFlowerType)flowerIndex].Add(chapterIdx);
                        else
                            listGetFlowersReward.Add((ScoreFlowerType)flowerIndex, new List<int>() { chapterIdx });
                    }
                    chapterIdx++;
                    flowerCount = 0;

                    if (chapterIdx >= chapterData.Count)
                        break;

                    chapterLastIndex = chapterData[chapterIdx]._stageIndex + chapterData[chapterIdx]._stageCount - 2;
                }
            }
        }
    }
    
    private static IEnumerator CheckUserDataIntegrity()
    {
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.SANDBOX)
        {
            yield break;
        }

        string errorStrings = "";
        int errorCount = 0;

        foreach( var hs in ServerRepos.UserHousingSelected)
        {
            if( ServerRepos.UserHousingItems.Exists(x => { return x.index == hs.index && x.modelIndex == hs.selectModel; }) == false )
            {
                errorStrings += string.Format($"SelectedHousing Not Exist: {hs.index} / {hs.selectModel}\n");
                errorCount++;
            }
        }

        foreach (var housing in ServerRepos.UserHousingItems)
        {
            if (ServerContents.Housings.ContainsKey(housing.index) == false)
            {
                errorStrings += string.Format($"Not Opened Housing: {housing.index}\n");
                errorCount++;
            }
            else if( ServerContents.Housings[housing.index].ContainsKey(housing.modelIndex) == false)
            {
                errorStrings += string.Format($"Not Opened Housing: {housing.index} / {housing.modelIndex}\n");
                errorCount++;

            }
        }

        if( errorCount > 0 )
        {
            bool ack = false;
            var go = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            go.InitServerSystemPopUp("Invalid UserData", errorStrings, true, isError: true);
            go.SetResourceImage("Message/error");
            go.SetButtonText(1, "Reboot");
            go.SetButtonText(2, "GoAhead");
            go.actionYesCallback = () => { Global.ReBoot(); ack = true; };
            go.actionNoCallback = () => { ack = true; };

            yield return new WaitUntil(() => { return ack == true; });
        }
    }

    public  static IEnumerator CheckUserAdvDataIntegrity()
    {
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.SANDBOX)
        {
            yield break;
        }

        string errorStrings = "";
        int errorCount = 0;

        if (ServerRepos.LoginCdn.adventureVer != 0 && ServerRepos.UserAdventureLobbyAnimals != null && ServerRepos.UserAdventureLobbyAnimals != null)
        {
            foreach (var animal in ServerRepos.UserAdventureAnimals)
            {
                if (ServerContents.AdvAnimals.ContainsKey(animal.animalId) == false)
                {
                    errorCount++;
                    errorStrings += string.Format($"Invalid animal : {animal.animalId}\n");
                }
            }

            foreach (var ia in ServerRepos.UserAdventureLobbyAnimals)
            {
                if (ServerContents.AdvAnimals.ContainsKey(ia.animalId) == false)
                {
                    errorCount++;
                    errorStrings += string.Format($"Invalid Lobbyanimal : {ia.animalId}\n");

                }

                if (ServerRepos.UserAdventureAnimals.Exists(x => x.animalId == ia.animalId) == false)
                {
                    errorStrings += string.Format($"Lobbyanimal Not Exist: {ia.animalId}\n");
                    errorCount++;
                }
            }
        }

        if (errorCount > 0)
        {
            bool ack = false;
            var go = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            go.InitServerSystemPopUp("Invalid UserData", errorStrings, true, isError: true);
            go.SetResourceImage("Message/error");
            go.SetButtonText(1, "Reboot");
            go.SetButtonText(2, "GoAhead");
            go.actionYesCallback = () => { Global.ReBoot(); ack = true; };
            go.actionNoCallback = () => { ack = true; };

            yield return new WaitUntil(() => { return ack == true; });
        }
    }
    
    /// <summary>
    /// 저장된 캐시가 해당 버전 보다 낮으면 캐시된 데이타를 모두 날린다(서비스중 잘못된 버전을 올려 파일이 꼬였을때 긴급하게 활용)
    /// </summary>
    private void EmergencyResetCache()
    {
        if (ServerRepos.LoginCdn.EmergencyResetCacheAssetV > 0)
        {
            if (PlayerPrefs.GetInt("EmergencyResetCacheAssetV", 0) != ServerRepos.LoginCdn.EmergencyResetCacheAssetV)
            {
                PlayerPrefs.SetInt("EmergencyResetCacheAssetV", ServerRepos.LoginCdn.EmergencyResetCacheAssetV);

                if (System.IO.Directory.Exists(Global.gameDataDirectory))
                {
                    System.IO.Directory.Delete(Global.gameDataDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.gameDataDirectory);
                }
            }
        }

        if (ServerRepos.LoginCdn.EmergencyResetCacheImageV > 0)
        {
            if (PlayerPrefs.GetInt("EmergencyResetCacheImageV", 0) != ServerRepos.LoginCdn.EmergencyResetCacheImageV)
            {
                PlayerPrefs.SetInt("EmergencyResetCacheImageV", ServerRepos.LoginCdn.EmergencyResetCacheImageV);

                if (System.IO.Directory.Exists(Global.gameImageDirectory))
                {
                    System.IO.Directory.Delete(Global.gameImageDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.gameImageDirectory);
                }

                if (System.IO.Directory.Exists(Global.adventureDirectory))
                {
                    System.IO.Directory.Delete(Global.adventureDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.adventureDirectory);
                }

                if (System.IO.Directory.Exists(Global.effectDirectory))
                {
                    System.IO.Directory.Delete(Global.effectDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.effectDirectory);
                }

                if (System.IO.Directory.Exists(Global.soundDirectory))
                {
                    System.IO.Directory.Delete(Global.soundDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.soundDirectory);
                }

                if (System.IO.Directory.Exists(Global.cachedScriptDirectory))
                {
                    System.IO.Directory.Delete(Global.cachedScriptDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.cachedScriptDirectory);
                }

                if (System.IO.Directory.Exists(Global.cachedResourceDirectory))
                {
                    System.IO.Directory.Delete(Global.cachedResourceDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.cachedResourceDirectory);
                }
            }
        }

        if (ServerRepos.LoginCdn.EmergencyResetCacheStageV > 0)
        {
            if (PlayerPrefs.GetInt("EmergencyResetCacheStageV", 0) != ServerRepos.LoginCdn.EmergencyResetCacheStageV)
            {
                PlayerPrefs.SetInt("EmergencyResetCacheStageV", ServerRepos.LoginCdn.EmergencyResetCacheStageV);

                if (System.IO.Directory.Exists(Global.StageDirectory))
                {
                    System.IO.Directory.Delete(Global.StageDirectory, true);
                    System.IO.Directory.CreateDirectory(Global.StageDirectory);
                }
            }
        }
    }

    /// <summary>
    /// 클로버, 탐험 날개 자동생성 그로시 전송
    /// </summary>
    private void SendAutoRegenerationLog()
    {
        //그로씨 클로버 자동생성 관련 
        if ((int)GameData.Asset.AllClover <= 5 && PlayerPrefs.HasKey("checkClover") && PlayerPrefs.GetInt("checkClover") < 5)
        {
            if (PlayerPrefs.GetInt("checkClover") < (int)GameData.Asset.AllClover)
            {
                //그로씨
                {
                    var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AUTO_REGENERATION,
                        0,
                        (int)GameData.Asset.AllClover - PlayerPrefs.GetInt("checkClover"),
                        0,
                        (int)(GameData.User.AllClover)
                    );
                    var doc = JsonConvert.SerializeObject(playEnd);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
                }
            }
        }

        //탐험날개 자동생산          PlayerPrefs.SetInt("checkWing", _wing.Value);
        if ((int)GameData.Asset.AllWing <= 3 && PlayerPrefs.HasKey("checkWing") && PlayerPrefs.GetInt("checkWing") < 3)
        {
            if (PlayerPrefs.GetInt("checkWing") < (int)GameData.Asset.AllWing)
            {
                //그로씨
                {
                    var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AUTO_REGENERATION,
                        0,
                        (int)GameData.Asset.AllWing - PlayerPrefs.GetInt("checkWing"),
                        0,
                        (int)(GameData.User.AllWing)
                    );
                    var doc = JsonConvert.SerializeObject(playEnd);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
                }
            }
        }
    }

    /// <summary>
    /// 코스튬 데이터 초기화
    /// </summary>
    private void InitCostumeData()
    {
        _costumeData.Clear();

        CostumeComparer costumeComparer = new CostumeComparer();
        List<CdnCostume> costumeList = new List<CdnCostume>();
        //코스튬 데이터 추가.
        var enumerator = ServerContents.Costumes.GetEnumerator();
        while (enumerator.MoveNext())
        {
            costumeList.Add(enumerator.Current.Value);
        }

        //코스튬 데이터 캐릭터ID & 코스튬 ID 별로 정렬.
        costumeList.Sort(costumeComparer);

        int charIndex = -1;
        string key = "";
        for (int i = 0; i < costumeList.Count; i++)
        {
            //캐릭터 id 가 달라질 때 해당 캐릭터의 디폴트 데이터를 생성해서 추가시켜줌.
            if (charIndex != costumeList[i].char_id)
            {
                charIndex = costumeList[i].char_id;
                CdnCostume defaultCostume = new CdnCostume();
                defaultCostume.char_id = charIndex;
                defaultCostume.costume_id = 0;
                defaultCostume.get_type = 1;
                defaultCostume.price_coin = 0;
                key = string.Format("{0}_{1}", charIndex, 0);
                //_costumeData 에 추가.
                _costumeData.Add(key, defaultCostume);
            }
            key = string.Format("{0}_{1}", charIndex, costumeList[i].costume_id);
            _costumeData.Add(key, costumeList[i]);
        }
    }

    /// <summary>
    /// 재료 데이터 초기화
    /// </summary>
    private void InitMaterialSpawnData()
    {
        foreach (var matMeta in ServerContents.MaterialMeta)
        {
            _materialSpawnProgress.Add(matMeta.Value.mat_id, matMeta.Value.expireTs);
        }
    }

    /// <summary>
    /// ServerAPI.RebootCheck 통신 후 호출되는 콜백, Reboot 여부 확인
    /// </summary>
    private void ServerCdnId(RebootCheckResp resp)
    {
        if (resp.IsSuccess)
        {
            if (resp.rebootCheck == true)
            {
                Global.ReBoot();
            }
            else
            {
                if (NetworkLoading._tempInstance == null)
                {
                    if (Application.platform != RuntimePlatform.WindowsEditor &&
                        Application.platform != RuntimePlatform.OSXEditor &&
                        Application.platform != RuntimePlatform.WindowsPlayer &&
                        NetworkSettings.Instance.enableLiAppSecureTool)
                    {
                        ServiceSDK.ServiceSDKManager.instance.ReStartLiapp();
                    }

                    CheckSystemState(); // 라인통신
                }
            }
        }
    }

    /// <summary>
    /// GetUserStageList 통신 후 호출되는 콜백, 스테이지 정보 로드
    /// </summary>
    private void recvGameList(BaseResp resp)
    {
        if (resp.IsSuccess)
        {
            foreach (var item in ServerRepos.UserStages)
            {
                //Debug.Log(item.stage + "___play" + item.play + "  score" + item.score + "  score" + item.score);

                if(ManagerData._instance._stageData.Count <= item.stage - 1)
                {
                    continue;
                }

                StageData stage = ManagerData._instance._stageData[item.stage - 1];
                stage._continue = item.continue_;
                stage._play = item.play;
                stage._fail = item.fail;
                stage._score = item.score;
                stage._missionProg1 = item.mprog1;
                stage._missionProg2 = item.mprog2;
                stage._missionClear = item.missionClear;
                stage._flowerLevel = item.flowerLevel;
            }

            foreach (var item in ServerRepos.EventStages)
            {

            }
            
            recvGameList_complete = true;
        }
    }
    
    public static int GetFinalChapter()
    {
        if (ServerContents.Chapters == null) return 0;

        int chapterIdx = 1;
        foreach (var item in ServerContents.Chapters)
        {
            if (chapterIdx < item.Key)
            {
                chapterIdx = item.Key;
            }
        }

        return chapterIdx;
    }

    public static bool IsStageAllClear()
    {
        var isStageAllClear = false;

        var stageCount = 1;
        foreach (var data in ManagerData._instance.chapterData)
        {
            stageCount += data._stageCount;
            if (stageCount > ServerRepos.User.stage)
            {
                break;
            }
        }

        if (GameData.User.stage >= stageCount)
        {
            isStageAllClear = true;
        }

        return isStageAllClear;
    }

#region 점검 및 강제 없데이트가 있는지 확인 + 토큰 값 갱신

    /// <summary>
    /// 점검중인지 강제 업데이트가 있는지 확인
    /// </summary>
    private void CheckSystemState()
    {
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            return;
        }

        ManagerNotice.instance.ShowNoticeOnAppleCationPause(OnCheckSystemStateComplete, OnCheckSystemStateFail);
    }

    private void OnCheckSystemStateComplete()
    {
        if (ServerRepos.User.loginTs != 0)
        {
            ServerAPI.RefershUserGame(ServerRepos.User.loginTs, OnRespCheckNeedReboot);
        }
        else
        {
            VerifyToken();
        }
    }
    
    private void OnCheckSystemStateFail(Trident.Error error)
    {
        string title        = Global._instance.GetString("p_t_4");
        string buttonTitle  = Global._instance.GetString("btn_1");
        string errorMessage = Global._instance.GetSystemNetworkErrorAndRebootString(error.getCode(), error.getMessage());
        
        UIPopupSystem popupNotice  = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupNotice.InitSystemPopUp(title, errorMessage, false, RebootApplication);
        popupNotice.SetButtonText(1, buttonTitle);
        popupNotice.HideCloseButton();
    }

    private void VerifyToken()
    {
        if (_isAuthorizing == true)
        {
            return;
        }
        isRefreshedToken = false;
        _isAuthorizing   = true;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        myProfile.token = ServiceSDKManager.instance.GetAccessToken();
        
        ServerAPI.VerifyToken(myProfile.token, (resp) =>
        {
            isRefreshedToken = true;            
            _isAuthorizing   = false;
        });
    }

    private void OnRespCheckNeedReboot(UserRefreshResp resp)
    {
        if (resp.isRefresh != 0)
        {
            Global.ReBoot();
        }
        else
        {
            VerifyToken();
        }
    }
    
    private void OnApplicationPauseReboot ()
    {
        if( UIPopupSystem._instance != null )
        { 
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }
    
    private void RebootApplication()
    {
        Global.ReBoot();
    }

#endregion

#region Trident SDK Initialize And Login Process

    private void SetupLoginProcess()
    {
        if (process != null)
        {
            ReleaseLoginProcess();
        }

        GameObject loginObj = new GameObject();
        process = loginObj.AddComponent<ProcessManager>();
    }

    private void ReleaseLoginProcess()
    {
        DestroyImmediate(process.gameObject);
        process = null;
    }

#endregion

#region Send Line Message

    public static void SendLineMessage(string in_userKey, string in_templateId)
    {
        if (string.IsNullOrEmpty(in_userKey))
        {
            return;
        }

        SendLineMessage(new List<string>() { in_userKey }, in_templateId);
    }

    public static void SendLineMessage(List<string> in_userKey, string in_templateId)
    {
        if (in_userKey == null || in_userKey.Count == 0)
        {
            return;
        }
        
        List<string> userKeyList = new List<string>();
        foreach (var userKey in in_userKey)
        {
            if (SDKGameProfileManager._instance.TryGetPlayingLineFriend(userKey, out UserFriend user))
            {
                var pionProfile    = user.GetPionProfile();
                var tridentProfile = user.GetTridentProfile();

                if (pionProfile == null || tridentProfile == null || pionProfile.profile.isLineMessageBlocked())
                {
                    continue;
                }

                if (string.IsNullOrEmpty(tridentProfile.userKey) == false)
                {
                    userKeyList.Add(tridentProfile.userKey);
                }
            }
        }
        
        if (userKeyList.Count == 0)
        {
            return;
        }
        
        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
        ServerAPI.SendLineFlexMessage(userKeyList, in_templateId, myProfile.DefaultName, SendLineFlexMessageComplete);
    }

    private static void SendLineFlexMessageComplete(TridentAPIResp obj)
    {
        if (obj.IsSuccess == false)
        {
            Debug.Log($"SendLineFlexMessage: error {obj.error}/{obj.message}");
        }
        else
        {
            
            Debug.Log($"SendLineFlexMessage: success!");
        }
    }

#endregion

#region 퀘스트 데이터 수복 코드 (임시)

    private void QuestDataCheckAndFix(Dictionary<int, QuestGameData> questData)
    {
        const string CANDY_EVNET_KEY = "CandyQuestDataIntegrityCheck";
        const string DUCK_EVNET_KEY  = "DuckQuestDataIntegrityCheck";

        QuestGameData candyEvent = null;
        QuestGameData duckEvent  = null;

        foreach (var data in questData)
        {
            if (data.Value.type == QuestType.chapter_Candy)
                candyEvent = data.Value;

            if (data.Value.type == QuestType.chapter_Duck)
                duckEvent = data.Value;
        }

        if (candyEvent != null && !PlayerPrefs.HasKey(CANDY_EVNET_KEY))
        {
            PlayerPrefs.SetInt(CANDY_EVNET_KEY, 0);

            if (!QuestDataIntegrityCheck(candyEvent))
            {
                ServerAPI.AdminStageMissionFix((AdminStageMissionFixResp fixResp) =>
                {
                    if (fixResp.IsSuccess)
                    {
                        Debug.Log("Successful Repair Quest Data");
                        QuestGameData.RefleshData();
                    }
                    else
                    {
                        Debug.Log("Repair Failed Quest Data");
                        PlayerPrefs.DeleteKey(CANDY_EVNET_KEY);
                    }
                });
            }
        }

        if (duckEvent != null && !PlayerPrefs.HasKey(DUCK_EVNET_KEY))
        {
            PlayerPrefs.SetInt(DUCK_EVNET_KEY, 0);

            if (!QuestDataIntegrityCheck(duckEvent))
            {
                ServerAPI.AdminStageMissionFix((AdminStageMissionFixResp fixResp) =>
                {
                    if (fixResp.IsSuccess)
                    {
                        Debug.Log("Successful Repair Quest Data");
                        QuestGameData.RefleshData();
                    }
                    else
                    {
                        Debug.Log("Repair Failed Quest Data");
                        PlayerPrefs.DeleteKey(DUCK_EVNET_KEY);
                    }
                });
            }
        }
    }

    /// <summary>
    /// 퀘스트 데이터의 무결성 체크
    /// </summary>
    /// <param name="questData"></param>
    /// <returns>정상데이터 true / 비정상데이터 false</returns>
    private bool QuestDataIntegrityCheck(QuestGameData questData)
    {
        if ((int)questData.type < 1000 || (int)questData.type >= 2000)
            return true;

        int chapterCount = ManagerData._instance?.chapterData?.Count ?? 0;
        if (questData.level > chapterCount || questData.level < 1) return true;

        var chapter       = ManagerData._instance.chapterData[questData.level - 1];
        int stageStartIdx = chapter._stageIndex                 - 1;
        int stageEndIdx   = stageStartIdx + chapter._stageCount - 1;

        int progCount       = Mathf.Clamp(questData.prog1, 0, questData.targetCount);
        int questClearCount = 0;

        for (int i = stageStartIdx; i <= stageEndIdx; i++)
        {
            if (ManagerData._instance._stageData[i]._missionClear != 0)
                questClearCount++;
        }

        return questClearCount == progCount;
    }

#endregion
    
}
