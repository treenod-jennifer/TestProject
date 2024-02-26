using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class ManagerStageChallenge : MonoBehaviour, IEventBase
{
    static public ManagerStageChallenge instance = null;

    #region ContentData
    private long endTs = 0;
    public long EndTs
    { get { return endTs; } }

    private int eventIndex = 0;
    public int EventIndex
    { get { return eventIndex; } }

    //몇 번째 이벤트인지
    private int eventStepIndex = 0;
    public int EventStepIndex
    { get { return eventStepIndex; } }

    //마지막 이벤트인지
    private bool isLastEventStep = false;
    public bool IsLastEventStep
    { get { return isLastEventStep; } }

    //스테이지 번호
    private int stage = 0;
    public int Stage
    { get { return stage; } }

    //목표 스코어
    private int targetScore = 0;
    public int TargetScore
    { get { return targetScore; } }

    //보상
    private Reward rewardInfo = null;
    public Reward RewardInfo
    { get { return rewardInfo; } }

    //영상 URL
    private string urlStr = null;
    public string UrlStr
    {  get { return urlStr; } }

    //리소스 번호
    private int resourceIndex = 0;
    public int ResourceIndex
    { get { return resourceIndex; } }

    #endregion

    #region UserData

    //최고 스코어
    private int bestScore = 0;
    public int BestScore
    { get { return bestScore; } }

    //현재 진행 이벤트 기간 끝났는지
    private bool isEndStageChallenge = false;
    public bool IsEndStageChallenge
    { get { return isEndStageChallenge; } }

    //현재 진행 이벤트 클리어 했는지
    private bool isClearStageChallenge = false;
    public bool IsClearStageChallenge
    { get { return isClearStageChallenge; } }

    #endregion

    private void Awake()
    {
        instance = this;
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
        ManagerUI._instance.OpenPopupStageChallenge();
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerStageChallenge>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        CdnStageChallenge cachingCDNData = ServerContents.StageChallenge;

        if (cachingCDNData == null || cachingCDNData.start_ts == 0)
            return false;

        //이벤트 시작했는지 확인
        if (Global.LeftTime(cachingCDNData.start_ts) > 0)
            return false ;

        //이벤트 끝났는지 확인 (전체기간으로 확인)
        if (Global.LeftTime(cachingCDNData.start_ts + (cachingCDNData.interval * cachingCDNData.condition.Length)) < 0)
            return false;

        //유저의 미션 진행도 검사
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        return true;
    }
    public void InitData()
    {
        SyncFromServerContentsData();
        SyncFromServerUserData();
    }

    public void SyncFromServerContentsData()
    {
        eventStepIndex = (int)((Global.GetTime() - ServerContents.StageChallenge.start_ts) / ServerContents.StageChallenge.interval);

        if(eventStepIndex < ServerContents.StageChallenge.condition.Length)
        {
            endTs = ServerContents.StageChallenge.start_ts + (ServerContents.StageChallenge.interval * (eventStepIndex + 1));

            if (ServerContents.StageChallenge.condition.Length - 1 == eventStepIndex)
                isLastEventStep = true;

            stage = ServerContents.StageChallenge.condition[eventStepIndex].stage;
            targetScore = ServerContents.StageChallenge.condition[eventStepIndex].score;
            rewardInfo = ServerContents.StageChallenge.rewards[eventStepIndex];
            resourceIndex = ServerContents.StageChallenge.resourceIndex[eventStepIndex];

            if (ServerContents.StageChallenge.url.TryGetValue(LanguageUtility.SystemCountryCode, out string[] urls))
            {
                urlStr = urls[eventStepIndex];
            }
            else
            {
                urlStr = string.Empty;
            }
        }
        else
        {
            eventStepIndex = ServerContents.StageChallenge.condition.Length - 1;
        }
    }

    public static void UpdateEventEnd()
    {
        //현재 진행중인 이벤트 끝났는지 확인
        if (Global.LeftTime(instance.endTs) < 0)
        {
            instance.isEndStageChallenge = true;
        }
        else
            instance.isEndStageChallenge = false;
    }

    public void SyncFromServerUserData()
    {
        //남은 시간 검사해서 이벤트 끝났는지 체크.
        UpdateEventEnd();

        bool isExistUserData = false;
        if (ServerRepos.UserStageChallenge != null)
        {
            ServerUserStageChallenge userData = ServerRepos.UserStageChallenge;
            if(userData.stage == stage)
            {
                isExistUserData = true;
                bestScore = userData.maxScore;
            }
        }

        //현재 유저 데이터가 없는 경우
        if(isExistUserData == false)
        {
            bestScore = 0;
            isClearStageChallenge = false;
        }
        else if(bestScore >= targetScore)
        {
            //유저가 보상을 받은 상태인 경우
            isClearStageChallenge = true;
        }
    }

    public GameEventType GetEventType()
    {
        return GameEventType.STAGE_CHALLENGE;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnLobbyStart()
    {
        InitData();
    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public void OnIconPhase()
    {
        if (ManagerStageChallenge.instance.IsEndStageChallenge == false)
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconStageChallenge>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.StageChallenge));
        }
    }
}
