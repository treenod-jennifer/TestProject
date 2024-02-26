using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAlphabetEvent : MonoBehaviour, IEventBase
{
    static public ManagerAlphabetEvent instance = null;

    public const string prefsKey_N = "AlphabetReward_N";
    public const string prefsKey_S = "AlphabetReward_S";

    private const string specialKey = "special";

    //인게임에서 사용하는 데이터
    static public AlphabetIngame alphabetIngame;

    #region 게임 데이터
    public int eventIndex { get; set; }
    public int resourceIndex { get; set; }
    public long endTs { get; set; }

    //알파벳 데이터
    public Dictionary<int, List<int>> dicAlphabetIndex_Normal = null;
    public List<int> listAlphabetIndex_Special = null;
    #endregion

    #region 유저 데이터
    public int currentGroup { get; set; }

    //알파벳 이벤트 플레이 가능한 스테이지(0이면 모든 스테이지에서 플레이 가능)
    public int canPlayStageIndex { get; set; }

    //이벤트 완료한 유저인지.
    public bool isUser_eventComplete { get; set; }
    public bool isUser_normalComplete { get; set; }
    public bool isUser_specialComplete { get; set; }

    //유저가 알파벳 모은 상태
    public Dictionary<int, List<int>> dicCollectCount_Normal = null;
    public List<int> listCollectCount_Special = null;
    #endregion

    #region 알파벳 모으기 종료시간 검사
    //알파벳 모으기 진행할 수 있는 스테이지인지 (인게임 진입 시 시간으로 검사)
    public bool IsAlphabetEventStage_GameStartTime()
    {
        //유저가 이벤트 진행할 수 있는 상태인지 검사.
        if (IsUser_CanPlayAlphabetEvent() == false)
            return false;

        //게임 시작을 누른 타이밍에 이벤트가 종료되었는지 검사.
        if (ServerRepos.GameStartTs >= instance.endTs)
            return false;

        //검사하는 스테이지가 이벤트 가능 스테이지인지 검사.
        if (ManagerAlphabetEvent.instance.canPlayStageIndex != 0
            && ManagerAlphabetEvent.instance.canPlayStageIndex != Global.stageIndex)
            return false;

        return true;
    }

    /// <summary>
    /// 알파벳 모으기 진행할 수 있는 스테이지인지 (실제 이벤트 시간으로 검사)
    /// </summary>
    public bool IsCanPlayEvent_AtStage_RealTime(int stage)
    {
        //유저가 이벤트 진행할 수 있는 상태인지 검사.
        if (IsUser_CanPlayAlphabetEvent() == false)
            return false;

        //이벤트 시간 종료되었으면 진행 불가.
        if (Global.LeftTime(ManagerAlphabetEvent.instance.endTs) <= 0)
            return false;

        //검사하는 스테이지가 이벤트 가능 스테이지인지 검사.
        if (ManagerAlphabetEvent.instance.canPlayStageIndex != 0
            && ManagerAlphabetEvent.instance.canPlayStageIndex != stage)
            return false;

        return true;
    }
    #endregion

    #region 알파벳 모으기 진행 가능 유저 검사
    /// <summary>
    /// 알파벳 이벤트 진행 가능한 유저인지.
    /// </summary>
    public bool IsUser_CanPlayAlphabetEvent()
    {
        //유저의 미션 진행도 검사
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        //유저가 이벤트 완료한 상태라면 진행 불가.
        if (ManagerAlphabetEvent.instance.isUser_eventComplete == true)
            return false;

        return true;
    }
    #endregion

    private void Awake()
    {
        instance = this;

        alphabetIngame = new AlphabetIngame();
    }

    public GameEventType GetEventType()
    {
        return GameEventType.ALPHABET;
    }

    public void OnLobbyStart()
    {
        ManagerAlphabetEvent.instance.InitGameData();
        ManagerAlphabetEvent.instance.InitUserData();

        if (ManagerAlphabetEvent.alphabetIngame != null)
            ManagerAlphabetEvent.alphabetIngame.InitIngameData_Default();
    }

    public void OnIconPhase()
    {
        //알파벳 이벤트가 진행중인 상태라면 아이콘 띄워줌.
        if (ManagerAlphabetEvent.IsActiveEvent())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconAlphabetEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(currentGroup));
        }
    }

    public IEnumerator OnRewardPhase()
    {
        yield return null;
    }

    public static bool IsActiveEvent()
    {
        if (instance == null)
            return false;

        return CheckStartable();
    }

    static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        return ServerContents.AlphabetEvent != null && ServerContents.AlphabetEvent.eventIndex != 0
            && Global.LeftTime(ServerContents.AlphabetEvent.endTs) > 0;
    }

    public static void Init()
    {
        if (CheckStartable() == false)
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerAlphabetEvent>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);

        //보상 키 제거.
        if (PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_N) == true)
            PlayerPrefs.DeleteKey(ManagerAlphabetEvent.prefsKey_N);
        if (PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_S) == true)
            PlayerPrefs.DeleteKey(ManagerAlphabetEvent.prefsKey_S);
    }

    //이벤트 시간 종료되었는지 확인
    public bool CheckEventEnd()
    {
        return (Global.LeftTime(ManagerAlphabetEvent.instance.endTs) <= 0);
    }

    #region 게임 데이터 초기화
    public void InitGameData()
    {
        eventIndex = ServerContents.AlphabetEvent.eventIndex;
        resourceIndex = ServerContents.AlphabetEvent.resourceIndex;
        endTs = ServerContents.AlphabetEvent.endTs;

        //일반 알파벳 인덱스 저장
        dicAlphabetIndex_Normal = new Dictionary<int, List<int>>();
        foreach (var temp in ServerContents.AlphabetEvent.alphabetRatio)
        {
            if (temp.Key == specialKey)
                continue;

            List<int> listTemp = new List<int>();
            for (int i = 0; i < temp.Value.Length; i++)
            {
                listTemp.Add(temp.Value[i][0]);
            }
            dicAlphabetIndex_Normal.Add(int.Parse(temp.Key), listTemp);
        }

        //스페셜 알파벳 인덱스 저장
        listAlphabetIndex_Special = new List<int>();
        if (ServerContents.AlphabetEvent.alphabetRatio.ContainsKey(specialKey) == true)
        {
            int[][] arrTemp = ServerContents.AlphabetEvent.alphabetRatio[specialKey];
            for (int i = 0; i < arrTemp.Length; i++)
            {
                listAlphabetIndex_Special.Add(arrTemp[i][0]);
            }
        }
    }
    #endregion

    #region 유저 데이터 초기화
    public void InitUserData()
    {
        InitUserData_Default();

        //알파벳 모으기 플레이 가능한 스테이지 인덱스 저장
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        if (myProfile.stage > ManagerData._instance.maxStageCount)
            canPlayStageIndex = 0;
        else
            canPlayStageIndex = myProfile.stage;

        if (ServerRepos.UserAlphabetEvents == null)
            return;

        //현재 그룹 초기화.
        currentGroup = ServerRepos.UserAlphabetEvents.alphabetProgress;

        //유저가 모은 알파벳 데이터 초기화.
        if (ServerRepos.UserAlphabetEvents.alphabetStatus != null)
        {
            dicCollectCount_Normal = new Dictionary<int, List<int>>();
            for (int i = 0; i < ServerRepos.UserAlphabetEvents.alphabetStatus.Count; i++)
            {
                List<int> listCount = new List<int>(ServerRepos.UserAlphabetEvents.alphabetStatus[i]);
                dicCollectCount_Normal.Add(i + 1, listCount);
            }
        }

        if (ServerRepos.UserAlphabetEvents.specialStatus != null)
        {
            listCollectCount_Special = new List<int>(ServerRepos.UserAlphabetEvents.specialStatus);
        }

        //유저 그룹 정보와 보상 정보로 이벤트 달성도 검사
        int groupCount = dicAlphabetIndex_Normal.Count;
        if (currentGroup > groupCount)
            isUser_normalComplete = true;

        if (IsExistSpecialBlock() == false || ServerRepos.UserAlphabetEvents.specialReward == 1)
            isUser_specialComplete = true;

        if (isUser_normalComplete && isUser_specialComplete)
            isUser_eventComplete = true;
    }

    //유저 데이터 디폴트 상태로 초기화.
    public void InitUserData_Default()
    {
        currentGroup = 0;
        canPlayStageIndex = 0;
        isUser_eventComplete = false;
        isUser_normalComplete = false;
        isUser_specialComplete = false;
        dicCollectCount_Normal = null;
        listCollectCount_Special = null;
    }
    #endregion

    #region 알파벳 획득 여부 검사
    //리스트의 인덱스를 이용해, 해당 위치의 알파벳이 획득됐는지 검사하는 함수 - 일반 알파벳
    public bool CheckCollectAlphabet_Normal_ByListIndex(int groupIdx, int listIdx)
    {
        if (dicCollectCount_Normal == null)
            return false;

        if (dicCollectCount_Normal.ContainsKey(groupIdx) == false)
            return false;

        List<int> listCount = dicCollectCount_Normal[groupIdx];
        if (listCount.Count <= listIdx)
            return false;

        return dicCollectCount_Normal[groupIdx][listIdx] > 0;
    }

    //리스트의 인덱스를 이용해, 해당 위치의 알파벳이 획득됐는지 검사하는 함수 - 스페셜 알파벳
    public bool CheckCollectAlphabet_Special_ByListIndex(int listIdx)
    {
        if (listCollectCount_Special == null)
            return false;

        if (listCollectCount_Special.Count <= listIdx)
            return false;

        return listCollectCount_Special[listIdx] > 0;
    }
    #endregion

    //현재 이벤트에 스페셜 블럭이 목표로 있는 상태인지 저장하는 변수.
    public bool IsExistSpecialBlock()
    {
        return (listAlphabetIndex_Special != null && listAlphabetIndex_Special.Count > 0);
    }

    //특정 그룹의 보상을 받은 상태인지 반환하는 함수 (checkIndex 값이 -1 일 경우, 스페셜 블럭 보상 검사)
    public bool isAchieveReward(int checkIndex)
    {
        if (ServerRepos.UserAlphabetEvents == null)
            return false;

        if (checkIndex > -1)
        {
            return ServerRepos.UserAlphabetEvents.reward[checkIndex] == 1;
        }
        else
        {
            return ServerRepos.UserAlphabetEvents.specialReward == 1;
        }
    }

    #region 알파벳 이름 가져오기
    public string GetAlphabetSpriteName_N(int alphabetIdx, bool isOn = true)
    {
        return (isOn == true) ? string.Format("aBlock_{0}_on", alphabetIdx) : string.Format("aBlock_{0}_off", alphabetIdx);
    }

    public string GetAlphabetSpriteName_S(int alphabetIdx)
    {
        return name = string.Format("e_special_block_{0}_{1}", instance.resourceIndex, alphabetIdx);
    }
    #endregion

    #region 인터페이스에서 사용하지 않는 함수
    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback) { }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public void OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        ManagerUI._instance.OpenPopupAlphabetEvent();
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
    #endregion

}
