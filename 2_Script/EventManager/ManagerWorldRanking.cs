using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public partial class ManagerWorldRanking : MonoBehaviour, IEventBase
{
    static public ManagerWorldRanking instance = null;

    public enum EventState
    {
        NOT_STARTED,        // 이벤트 시작하지 않음
        NEED_PARTICIPATE,   // 참여대기상태
        RUNNING,            // 경쟁중
        BEFORE_REWARD,      // 보상받기전
        REWARD,             // 보상기간
        EVENT_END,          // 이벤트 종료
    }

    public enum EventOpenCondition
    {
        WAIT_FOR_MISSION,   //미션을 완료하지 못한 경우
        WAIT_FOR_START,     //스테이지를 완료하지 못한 경우
        MEET                //조건 충족
    }

    static public ContentsData contentsData;
    static public UserData userData;
    static public Resource resourceData;
    
    private IconWorldRankingEvent icon;

    private void Awake()
    {
        instance = this;

        contentsData = new ContentsData();
        userData = new UserData();
        resourceData = new Resource();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        icon?.SetNewIcon(IsPlayCurrentVersion() == false);
        StartCoroutine(OpenWorldRanking());
    }

    //이벤트 상태 가져오기
    public static EventState GetEventState()
    {
        // 서버에 월드랭킹 데이터 조차 없는 경우
        // 시작시간은 따로 없음 (켜져있으면 유저가 대충 스테이지 다깨면 바로 시작)
        if (ServerContents.WorldRank == null || ServerContents.WorldRank.eventIndex == 0) 
            return EventState.NOT_STARTED;        

        #region 기간에 따른 이벤트 처리
        if (Global.LeftTime(ServerContents.WorldRank.endTs) < 0)
            return EventState.EVENT_END;
        else if (Global.LeftTime(ServerContents.WorldRank.rewardTs) < 0 )
            return EventState.REWARD;
        else if (Global.LeftTime(ServerContents.WorldRank.deadlineTs) < 0)
            return EventState.BEFORE_REWARD;
        #endregion

        if (ServerRepos.UserWorldRank.stage <= 1)
            return EventState.NEED_PARTICIPATE;

        //위의 경우에 해당하지 않으면 이벤트 진행 가능
        return EventState.RUNNING;
    }

    public static EventOpenCondition GetEventOpenCondition()
    {
        //유저가 미션 달성을 덜 한 경우
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return EventOpenCondition.WAIT_FOR_MISSION;

        //이벤트가 보이는 조건이지만, 모든 스테이지를 다 달성하지 않아 참여할 수 없는 상태
        if (GameData.User.stage <= ServerContents.WorldRank.reqStage)
            return EventOpenCondition.WAIT_FOR_START;

        return EventOpenCondition.MEET;
    }

    public static bool IsFirstExperience()
    {
        if( ServerRepos.UserWorldRank.prevPlayedEventCount > 0 )
            return false;
        return true;
    }

    public static bool GetIconShowState()
    {
        switch(GetEventState())
        {
            case EventState.NOT_STARTED:
                return false;
        }

        switch (GetEventOpenCondition())
        {
            case EventOpenCondition.WAIT_FOR_MISSION:
            case EventOpenCondition.WAIT_FOR_START:
                return false;
        }

        return true;
    }

    public static bool IsShopObjectActive()
    {
        var eventState = GetEventState();

        //월드랭킹이 오픈되지 않은 경우
        if (eventState == EventState.NOT_STARTED) return false;

        //이전 또는 이번 월드랭킹에 참여한 이력이 있다면 상점 오픈
        int presentCount = 0;
        if (userData != null) presentCount = userData.IsParticipated() ? 1 : 0;

        int count = ServerRepos.UserWorldRank.prevPlayedEventCount + presentCount;
        if (count > 0) return true;

        //미션 또는 스테이지를 달성하지 못한 경우
        var eventOpenCondition = GetEventOpenCondition();
        
        if (eventOpenCondition == EventOpenCondition.WAIT_FOR_MISSION || 
            eventOpenCondition == EventOpenCondition.WAIT_FOR_START)
        {
            return false;
        } 

        //월드랭킹이 열려있고, 참여 이력이 없으며, 오픈 조건을 달성한 경우
        return true;
    }

    public static bool IsMainObjectActive()
    {
        switch (GetEventState())
        {
            case EventState.NOT_STARTED:
            case EventState.EVENT_END:
                return false;
        }

        switch (GetEventOpenCondition())
        {
            case EventOpenCondition.WAIT_FOR_MISSION:
            case EventOpenCondition.WAIT_FOR_START:
                return false;
        }

        return true;
    }

    //현재 월드랭킹 이벤트가 열려있는지 검사.
    public static bool IsActiveEvent()
    {
        if (instance == null)
            return false;

        EventState checkState = GetEventState();
        return (checkState != EventState.NOT_STARTED);
    }

    public static void Init()
    {
        if (instance != null) return;

        Global._instance.gameObject.AddMissingComponent<ManagerWorldRanking>();
        
        if (instance == null) return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    //정보 초기화
    public void InitData()
    {
        contentsData.SyncFromServerContentsData();
        userData.SyncFromServerUserData();
    }

    public class WorldRankDesc
    {
        public List<long> scores;
    }

    public class WorldRankData : ProfileTextureManager.IUserProfileData
    {
        public string ingameName = "";
        public string userKey;
        public long scoreValue;
        public int rankEventPoint;

        public int flower = 0;
        public int toy = 0;

        public long rank = 0;
        public string alterPicture;
        public bool photoUseAgreed;

        public long rankDiff = 0;
        public long scoreDiff = 0;

        //프로필 데이터 세팅
        public string _userKey      { get { return userKey; } }
        public string _alterPicture { get { return alterPicture; } }
        public string _pictureUrl   => string.Empty;

        public WorldRankData()
        {
        }

        public WorldRankData(Protocol.RankingData tridentScore)
        {
            if (tridentScore == null)
                return;

            this.scoreValue = (long)tridentScore.score;
            this.userKey = tridentScore.userKey;
            this.rank = tridentScore.rank;
        }

        public override string ToString()
        {
            return string.Format(
                "UserKey: {0}, scoreValue:{1} rank:{2} photoUseAgreed:{3}",
                userKey, scoreValue, rank, photoUseAgreed);
        }

    }
    

    public class MyRankQueryResult
    {
        public bool found = false;
        public long rank;
        public long score;
    }

    public const int RANKING_PAGE_SIZE = 100;

    static public IEnumerator QueryRankingPage(int page, List<WorldRankData> rankResult)
    {
        int startRank = (page - 1) * RANKING_PAGE_SIZE;

        yield return QueryRanking(contentsData.TableIdRank, startRank, RANKING_PAGE_SIZE, rankResult);

        if (rankResult == null || rankResult.Count == 0) yield break;

        yield return QueryRankingProfiles(rankResult);
    }

    static public IEnumerator QueryHallOfFame(string factorId, List<WorldRankData> list)
    {
#if UNITY_EDITOR
        yield return null;
#else
        yield return QueryRanking(factorId, 0, 100, list);
        yield return QueryRankingProfiles(list);
#endif
    }

    static public IEnumerator QueryGroupRanking(List<WorldRankData> list)
    {
        var myRank = new MyRankQueryResult();
        yield return QueryMyEntry_Server(myRank);

        if (myRank.found == false)
        {
            Debug.Log("My Rank Not Found");
            yield break;
        }

        int groupStartRank = (((int)((myRank.rank - 1) / contentsData.coopGroupSize)) * contentsData.coopGroupSize);

        List<string> groupUserList = new List<string>();
        yield return QueryUsers(contentsData.TableIdEntry, groupStartRank, contentsData.coopGroupSize, groupUserList);
        
        yield return QueryRankingSpecificUsers(contentsData.TableIdRank, groupUserList, list);
        
        if (list == null || list.Count == 0) yield break;
        
        yield return QueryRankingProfiles(list);

    }

    static public IEnumerator QueryMyRank_Server(MyRankQueryResult res)  // 내 글로벌 랭킹 가져오기
    {
        res.found = false;

        bool ret = false;
        ServerAPI.WorldRankGetMyRank((resp)
            => {
                if (resp.IsSuccess)
                {
                    res.found = resp.found;
                    res.rank = resp.rank;
                    res.score = resp.score;
                }
                ret = true;
            }
        );

        yield return new WaitUntil(() => { return ret; });
    }

    static public IEnumerator QueryMyEntry_Server(MyRankQueryResult res)  // 내 글로벌 랭킹 가져오기
    {
        res.found = false;

        bool ret = false;
        ServerAPI.WorldRankGetMyEntry((resp) 
            => { 
                if( resp.IsSuccess)
                {
                    res.found = resp.found;
                    res.rank = resp.rank;
                    res.score = resp.score;
                }
                ret = true;
            }
        );

        yield return new WaitUntil(() => {return ret; });
    }

    static public IEnumerator QueryMyRanking(MyRankQueryResult res)
    {
        yield return QueryMyRank_Server(res);
    }

    private static IEnumerator QueryRanking(string factorId, int rankStart, int querySize, List<WorldRankData> rankResult)
    {
        bool isComplete = false;
        ServerAPI.QueryUsers(factorId, rankStart.ToString(), querySize.ToString(), resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                if (resp.data != null && resp.data.data != null)
                {
                    foreach (var item in resp.data.data)
                    {
                        rankResult.Add(new WorldRankData(item));
                    }
                }
            }
            isComplete = true;
        });
        
        yield return new WaitUntil(() => isComplete);
    }

    private static IEnumerator QueryUsers(string factorId, int rankStart, int querySize, List<string> resultUserList)
    {
        bool isComplete = false;
        ServerAPI.QueryUsers(factorId, rankStart.ToString(), querySize.ToString(), resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                if (resp.data != null && resp.data.data != null)
                {
                    foreach (var item in resp.data.data)
                    {
                        resultUserList.Add(item.userKey);
                    }
                }
            }
            isComplete = true;
        });
        
        yield return new WaitUntil(() => isComplete);
    }

    private static IEnumerator QueryRankingSpecificUsers(string factorId, List<string> userKeys, List<WorldRankData> rankResult)
    {
        bool   isComplete        = false;
        string userKeyStringList = string.Join(",", userKeys.ToArray());
        ServerAPI.QueryRankingSpecificUsers(factorId, userKeyStringList, resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                if (resp.data != null && resp.data.data != null)
                {
                    foreach (var item in resp.data.data)
                    {
                        rankResult.Add(new WorldRankData(item));
                    }
                }
            }
            isComplete = true;
        });

        yield return new WaitUntil(() => isComplete);
    }

    public static IEnumerator QueryRankingProfiles(List<WorldRankData> fillTargets)
    {
        List<string> userKeyList = new List<string>();
        for(int i = 0; i < fillTargets.Count; ++i)
        {
            userKeyList.Add(fillTargets[i].userKey);
        }

        bool profileRet1 = false;
        Debug.Log("ManagerWorldRanking.QueryRankingProfiles Start");
        yield return GetProfileList((Profile_PION[] profileList) =>
            {
                Debug.Log("ManagerWorldRanking.GetProfileList.QueryRankingProfiles Callback" + profileList.Length);
                profileRet1 = true;

                if (profileList != null)
                {
                    for (int i = 0; i < profileList.Length; ++i)
                    {
                        var score = fillTargets.Find((x) => x.userKey == profileList[i].userKey);
                        if (score != null)
                        {
                            score.alterPicture   = profileList[i].profile.alterPicture;
                            score.flower         = profileList[i].profile.flower;
                            score.rankEventPoint = profileList[i].profile.rankEventPoint;
                            score.photoUseAgreed = profileList[i].profile.isLineTumbnailUsed();
                            score.ingameName     = profileList[i].profile.name;
                            score.toy            = profileList[i].profile.toy;

                            profileList[i].profile.Log();
                        }
                    }
                }
            },
            userKeyList.ToArray());

        while (profileRet1 == false)
        {
            yield return null;
        }

        yield break;
    }

    private static IEnumerator GetProfileList(System.Action<Profile_PION[]> callbackHandler, string[] userKeys)
    {
        bool isComplete = false;

        string key   = NetworkUtil.CSV_FromArray(userKeys);
        string token = ServiceSDK.ServiceSDKManager.instance.GetAccessToken();
        ServerAPI.GetProfileList(token, key, resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.data);
            }
            else if (resp.IsFailTridentAPI)
            {
                // 라인 API 통신 중 에러 발생 시 경고 팝업만 출력.
                TridentAPIErrorController.OpenErrorPopup(resp.lineStatusCode, resp.error);
                DelegateHelper.SafeCall(callbackHandler, null);
            }
            
            isComplete = true;
        });

        yield return new WaitUntil(() => isComplete);
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.WORLD_RANKING;
    }

    void IEventBase.OnLobbyStart()
    {
        if (ManagerWorldRanking.IsActiveEvent())
        {
            ManagerWorldRanking.instance.InitData();
        }
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield break;
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin() == false && ServerContents.WorldRank != null
                && ServerRepos.UserWorldRank != null && NetworkSettings.Instance.IsRealDevice())
        {
            var worldRankEventState = ManagerWorldRanking.GetEventState();

            if (ServerRepos.UserWorldRank != null &&
                ServerRepos.UserWorldRank.state < 2 &&
                ManagerWorldRanking.userData.IsParticipated() &&
                worldRankEventState == ManagerWorldRanking.EventState.REWARD)
            {
                if (resourceData.worldRankingPack == null)
                    yield return resourceData.LoadWorldRankingResource();
                ManagerUI._instance.OpenPopup_LobbyPhase<UIPopUpWorldRank>((popup) => popup.InitPopup());
                yield return new WaitWhile(() => UIPopUpWorldRank._instance != null);
            }
        }

    }

    void IEventBase.OnIconPhase()
    {
        if (ManagerWorldRanking.GetIconShowState())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconWorldRankingEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    icon.Init(ServerContents.WorldRank);
                    this.icon = icon;
                });
        }
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public static IEnumerator OpenWorldRanking()
    {
        if (ManagerLobby._instance._state != TypeLobbyState.Wait || UIPopUpWorldRank._instance != null)
        {
            yield break;
        }

        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            ManagerUI._instance.GuestLoginSignInCheck();
            yield break;
        }

        var eventState = GetEventState();
        
        if (resourceData.worldRankingPack == null)
            yield return resourceData.LoadWorldRankingResource();

        switch (eventState)
        {
            case EventState.REWARD:
            case EventState.RUNNING:
                {
                    ManagerUI._instance.OpenPopup<UIPopUpWorldRank>((popup) => popup.InitPopup());
                }
                break;
            case EventState.NEED_PARTICIPATE:
                {
                    if (ServerRepos.UserWorldRank != null && ServerRepos.UserWorldRank.state > 0)
                    {
                        Global.SetGameType_WorldRanking(ServerContents.WorldRank.eventIndex, 1);
                        ManagerUI._instance.OpenPopupReadyStageCallBack();
                    }
                    else
                    {
                       var profilePopup = ManagerUI._instance.OpenPopup<UIPopupPionProfile>();
                       profilePopup.Init(PionProfileContentType.WORLD_RANKING, () =>
                       {
                           ServerRepos.UserWorldRank.state = 1;
                           
                           Global.SetGameType_WorldRanking(ServerContents.WorldRank.eventIndex, 1);
                           ManagerUI._instance.OpenPopupReadyStageCallBack();
                       });
                    }
                }
                break;
            case EventState.BEFORE_REWARD:
                //정산중
                UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_wrk_5"), true);
                popup.SetButtonText(1, Global._instance.GetString("btn_2"));
                popup.SetButtonText(2, Global._instance.GetString("btn_95"));
                popup.SetCallbackSetting(2, () => ManagerUI._instance.OpenPopupWorldRankExchangeShop(), true);
                break;
            case EventState.EVENT_END:
                //이벤트 종료
                ManagerUI._instance.OpenPopup<UIPopUpWorldRankHallOfFame>().InitPopup(true);
                break;
        }
    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserWorldRank == null)
            return true;
        
        var  eventState = GetEventState();
        bool isCanPlay  = eventState == EventState.NEED_PARTICIPATE || eventState == EventState.RUNNING;
        if (ServerRepos.UserWorldRank.totalPlay == 0 && isCanPlay)
            return false;
        else 
            return true;
    }
}
