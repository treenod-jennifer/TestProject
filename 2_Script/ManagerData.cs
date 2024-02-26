using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Protocol;
using Newtonsoft.Json;

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
}/*
public enum DataLoadState
{
    NONE,
    READY,
    LOGIN,
    FRIENDS_DATA,
    USER_INFO,
    UPDATE_DATA,
    LOADING_STAGE
}
*/

public class ManagerData : MonoBehaviour {

    public static ManagerData _instance = null;


    [System.NonSerialized]
    public DataLoadState _state = DataLoadState.eNone;
    public UserData userData;
    //친구 정보 리스트(게임하는 친구)
    public Dictionary< string, UserData > _friendsData = new Dictionary<string, UserData>();
    //친구 정보 리스트(게임을하지 않는 친구)
    public Dictionary<string, UserData> _inviteFriendsData = new Dictionary<string, UserData>();
    //유저 프로필 데이터
    //public Dictionary<string, SDKGameProfileInfo> _dicUserProfileInfo = new Dictionary<string, SDKGameProfileInfo>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // cdn 지울예정
    public CdnLogin _cdnData;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 스테이지, 챕터
    public List<ChapterData> chapterData = new List<ChapterData>();
    public int maxStageCount = 0;
    public List<StageData> _stageData = new List<StageData>();

    public Dictionary<int, EventChapterData> _eventChapterData = new Dictionary<int, EventChapterData>();

    public List<int> StageVersionList = new List<int>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 씬(미션)의 게임데이타 유저데이타
    //public List<DayData> dayData = new List<DayData>();             // 오늘의 전체 미션관계(진행 중인 미션을 완료로 바꾸면 새로운 미션 유무를 뽑아낼수있다)
    public List<MissionData> missionData = new List<MissionData>(); // 오늘의 총 미션리스트, 받은 미션을 가리키는것이 아님...  받은 미션은 상태를 통해 걸러내서 사용
    public Dictionary<int, MissionData> _missionData = new Dictionary<int, MissionData>();

    //public List<SceneData> sceneData = new List<SceneData>();       // 열려있는 영역의 씬 상태들 저장,,(점점 증가하다가 영역이 바뀌면 0)

    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 퀘스트
    public Dictionary<int, QuestGameData> _questGameData = new Dictionary<int,QuestGameData>();
    //public List<QuestUserData> _questUserData = new List<QuestUserData>();
    //public List<QuestProgress> _questProgress = new List<QuestProgress>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 꾸미기 아이템
    //public List<HousingUserData> _housingData = new List<HousingUserData>();
    public List<HousingUserData> _housingSelectData = new List<HousingUserData>();
    public List<PlusHousingModelData> _housingModelData = new List<PlusHousingModelData>(); // 확장 모델

    public Dictionary<string, PlusHousingModelData> _housingGameData = new Dictionary<string, PlusHousingModelData>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 재료
    public List<MaterialData> _meterialData = new List<MaterialData>();
    public List<MaterialSpawnUserData> _materialSpawnData = new List<MaterialSpawnUserData>();

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 포코코로
    public List<PokoyuraData> _pokoyuraData = new List<PokoyuraData>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 선물상자 
    //public List<GiftBoxUserData> _giftBox = new List<GiftBoxUserData>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 스템프
    //public List<StampData> _stampData = new List<StampData>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // message
    public List<MessageData> _messageData = new List<MessageData>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Notice
    //public List<NoticeData> _noticeData = new List<NoticeData>();
    //코스튬.
    //public Dictionary<string, CdnCostume> _costumeData = new Dictionary<string, CdnCostume>();
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //로그인시때만 사용
    //private SDKLoginProcess _loginProcess = null;

    //인증중인데 다시 인증하지 않게 플래그 설정
    //ServiceSDK.ServiceSDKManager.instance.IsAuthorizing() 으로 확인해도 될것같은데 11월19일 서비스전 아직 라인측에서 해당 api 확인이 안되어 대신 사용
    //라인측에서 해당 api사용해도 된다고 하면 바꾸면 될것같음 테스트는 해봤음
    private bool _isAuthorizing = false;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        
         //HashSet<int> 변환이 가능하도록 역직렬화(Deserialize) 유형 추가
         Newtonsoft.Json.Utilities.AotHelper.EnsureList<int>();
    }
    
    class LoadingRebootRAII : System.IDisposable
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

    [System.NonSerialized]
    public List<int> _newMissionIndex = new List<int>();
	// Use this for initialization
    IEnumerator Start() {
        Global._pendingReboot = false;
            
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();

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
        //ServerAPI.Reset();    
 

	}

    void InitCostumeData()
    {
        /*_costumeData.Clear();

     //   CostumeComparer costumeComparer = new CostumeComparer();
     //   List<CdnCostume> costumeList = new List<CdnCostume>();
        //코스튬 데이터 추가.
      //  var enumerator = ServerContents.Costumes.GetEnumerator();
     //   while (enumerator.MoveNext())
        {
     //       costumeList.Add(enumerator.Current.Value);
        }

        //코스튬 데이터 캐릭터ID & 코스튬 ID 별로 정렬.
      //  costumeList.Sort(costumeComparer);

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
        }*/
    }

    long deactiveTime = 0;

    void OnApplicationPauseReboot ()
    {
        if( UIPopupSystem._instance != null )
        { 
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }/*
    void recvVerify(BaseResp code)
    {
        if (code.IsSuccess) {

        }
        else {
            // REBOOT?
        }

    }*/
    void OnApplicationPause(bool pause)
    {
 
        
    }

    //점검중인지 강제업데이트가 있는지 확인
    private void CheckSystemState()
    {
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            return;
        }

      
        
    }

  

    private void GoToAppMarketAndReboot()
    {
    }

    private void RebootApplication()
    {
        Global.ReBoot();
    }


    private void RefreshToken()
    {

    }

    public void OnNetworkFailDelegate()
    {


        //Debug.LogError("네트워크 에러 발생,,, 시작화면으로 되돌리기");
    }

    

    public void StartNetwork() {
          
        if (_state == DataLoadState.eComplete)
            return;
        _state = DataLoadState.eStart;
    }
    /*
    IEnumerator OfflineModeCoroutine() {
 
        ServerAPI.CheckNetworking(null); // PING CHECK
        bool bDoneOfflineDoc = true;
        bool bDoneUserAsk = true;
        
        GameData.Reset();
        yield return RxPatterns.TimeoutCoroutine(ServerLogics.ThePingWaitTime, () => ServerAPI.EnabledNetworking,
            (ok) => {
                try {
                    if (ok) {
                        //Debug.Log("[cache] Established internet....");

                        // 네트워크 연결 됨 (PING)
                        if (CacheGameData.HasCachedPlayData()) {
                            bDoneOfflineDoc = false;
                            //Debug.Log("[cache] HasCachedPlayData (send doc)");
                            ServerAPI.OfflineGame(GameData.BuildOfflineGame(), (resp) =>
                            {
                               
                                if (!resp.IsNetworkError) {
                                    CacheGameData.DeleteCachedPlayData();
                                    //Debug.Log("[cache] ********* delete cached play data");
                                }
                                //Debug.Log("[cache] ********* sent offline doc: " + resp);
                               
                                bDoneOfflineDoc = true;
                            });
                        }
                    }
                    else {
                        // NOT CONNECT INTERNET
                        // 한번 이라도 로그인 되있었어야 플레이 가능
                        if (CacheGameData.CanPlayOffline()) {
                            bDoneUserAsk = false;
                            // 유저에게 오프라인 모드 질문...
                            ErrorController.QuestionOfflineMode((yes) => {
                                
                                if (yes) {
                                    Global._onlineMode = false;
                                    GameData.SetOnlineMode(false);
                                }
                                else {
                                    Global.ReBoot();
                                }
                                bDoneUserAsk = true;
                                //Debug.Log("[cache] Ask Offline " + bDoneUserAsk);
                            });
                        }
                        else {
                            bDoneUserAsk = false;
                            ErrorController.ShowFailedConnection();    // REBOOT
                        }
                    }
                }
                catch (System.Exception e) {
                    //Debug.LogWarning("[cache] Flow Control Exception: " + e);
                    // CacheGameData.DeleteAll();
                }
            });
        
        var waitTime = new WaitForSeconds(0.1f);
        while (!bDoneUserAsk) {
            yield return waitTime;
        }
              
        yield return RxPatterns.TimeoutCoroutine(10, () => bDoneOfflineDoc, (ok) => { });
        //Debug.Log("[cache] bDoneOfflineDoc " + bDoneOfflineDoc);
#if UNITY_EDITOR
        if (!Global._onlineMode) {
            NGUIDebug.Log("* OFF-LINE MODE *");
        }
#endif
        
    }
    */
    /*   public void GetNewMission(int in_day = 0)
       {
           _newMissionIndex.Clear();
   
           if (in_day > dayData.Count)
               return;
           
   
           for (int i = 0; i < dayData[in_day-1].sceneTree.Count; i++)
           {
               if (GetMission(dayData[in_day-1].sceneTree[i]))
                   break;
           }
       }
       bool GetMission(MissionTreeNode in_node)
       {
           bool result= false;
   
           int count = _newMissionIndex.Count;
           if (in_node.childMissionList != null)
           {
               for (int i = 0; i < in_node.childMissionList.Count; i++)
               {
                   if (GetMission(in_node.childMissionList[i]))
                   {
                       break;
                   }
               }
           }
   
   
           if (count == _newMissionIndex.Count)
           {
               if (in_node.childMissionList.Count == 0)
                   if (in_node.missionIndex > 0)
                    //   if (in_node.clear == false)
                       if (missionData[in_node.missionIndex - 1].state == TypeMissionState.Inactive || missionData[in_node.missionIndex - 1].state == TypeMissionState.Active)
                       {
                           _newMissionIndex.Add(in_node.missionIndex);
                           return true;
                       }
           }
   
           if (count == _newMissionIndex.Count)
           {
               if (in_node.nextMissionList != null)
               {
                   for (int i = 0; i < in_node.nextMissionList.Count; i++)
                   {
                       if (GetMission(in_node.nextMissionList[i]))
                           break;
                   }
               }
           }
           return result;
       }*/
   
    private void OnUpdateScore (bool success)
    {
        //Debug.Log( "Updated Score" );
    }
    /*
    void recvVerifyToken(BaseResp code)
    {
        if ( code.IsSuccess )
        {
            string text = Global._instance.GetSystemErrorString( System.Convert.ToInt32(code.error), code.error );
            UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
            popupWarning.InitSystemPopUp( "メッセージ", text, false, null );
            popupWarning.SetButtonText( 1, "確認" );
            popupWarning.FunctionSetting(1, "Reboot", Global._instance.gameObject );
        }
        else
        {

        }

    }
    
    static public bool newUser = false;
	void recvLogin(BaseResp code)
    {
        if (code.IsSuccess) {
            _state = DataLoadState.eUserLogin;
            Global.join = false;

            if (ServerRepos.User.loginTs == 0) {
                Global.join = true;
            }
            //Debug.Log("** GAME Login ok (recvLogin) : " + ServerRepos.User.loginTs + "  " + Time.time);
            
        }
        else {
            // TODO: FAILED DIALOG
        }
    }
    bool recvGameList_complete = false;
    void recvGameList(BaseResp resp)
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
	 */
	void Update () {
	    // 반드시 호출 (Scheduler가동)
	   // ServerAPI.Tick();


        if (_state == DataLoadState.eComplete)
        {

        }
	}
    
    public void ResetLocalData()
    {
       /* PlayerPrefs.DeleteKey("WindowNoticeStart");
        PlayerPrefs.DeleteKey("optionEff");
        PlayerPrefs.DeleteKey("optionLPush");
        PlayerPrefs.DeleteKey("optSoundBgm");
        PlayerPrefs.DeleteKey("OpenItemGetMsg");
        PlayerPrefs.DeleteKey("eMessageBox_LineNickname");
        for (int i = 0; i < 30; i++)
        {
            if (PlayerPrefs.HasKey("TutorialShow" + i))
                PlayerPrefs.DeleteKey("TutorialShow" + i);

            if (PlayerPrefs.HasKey("MenuTutorialShow" + i))
                PlayerPrefs.DeleteKey("MenuTutorialShow" + i);
        }*/
    }

    private void SetLoginAccessToken ()
    {
   
    }

    // 로그인 버튼이 나와야 할때
    private void SetLoginButton ( SceneTitleBtnContainer.BtnContainerState state )
    {
     
    }

    /// <summary>
    /// 로그인 프로세스 셋팅
    /// </summary>
    private void SetupLoginProcess()
    {
  
    }

    /// <summary>
    /// 로그인 프로세스 해제
    /// </summary>
    private void ReleaseLoginProcess()
    {
 
    }

    /// <summary>
    /// sdk init 완료 핸들러
    /// </summary>
    public void OnCompleteInitializeHandler()
    {
        this._state = DataLoadState.eLineInitialize;
    }

    /// <summary>
    /// sdk 로그인 완료 핸들러
    /// </summary>
    /// <param name="isSuccess"></param>
    public void OnCompleteLoginHandler(bool isSuccess)
    {

    }

   
    public static void SendLineMessageInvite(List<string> in_providerKey, string in_templateId, string in_message)
    {
       

    }
    public static void SendLineMessage(List<string> in_userKey ,string in_templateId, string in_message)
    {

    }

    public static int[] GetFlowerCount_OnlyStage()
    {
        int[] flowerLevelSum = new int[4] { 0, 0, 0, 0 };
        for(int i = 0; i < _instance._stageData.Count; ++i)
        {
            flowerLevelSum[_instance._stageData[i]._flowerLevel]++;
        }
        return flowerLevelSum;
    }
}
