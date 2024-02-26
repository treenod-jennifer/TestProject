using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Protocol;
using UnityEngine;

public partial class ManagerGroupRanking : MonoBehaviour, IEventBase
{
    public static ManagerGroupRanking instance = null;

    public enum EventState
    {
        NEED_PARTICIPATE, // 참여대기상태
        RUNNING,          // 경쟁중
        REWARD,           // 보상기간
        EVENT_END         // 이벤트 종료
    }

    #region AssetBundleName
    private const string GROUP_RANKING_EVENT_TUTORIAL_KEY  = "GroupRankingEventTutorial";
    private const string GROUP_RANKING_EVENT_CUT_SCENE_KEY = "GroupRankingEventCutScene";

    private const string AREA_ASSET_BUNDLE_NAME  = "group_ranking_area";
    private const string AREA_ASSET_NAME         = "GroupRanking";
    public const  string SPINE_ASSET_BUNDLE_NAME = "group_ranking_spine";
    public const  string SPINE_ASSET_NAME        = "BoxAnimation";
    public const  string ATLAS_ASSET_BUNDLE_NAME = "group_ranking_atlas";
    public const  string ATLAS_ASSET_NAME        = "GroupRankAtlas";

    public static string GetTextureAssetBundleName() => isEventOn == false ? string.Empty : $"group_ranking_texture_{ServerContents.GroupRanking.resourceId}";

    public static string GetHousingAssetBundleName() => isEventOn == false ? string.Empty : $"h_{GetHousingAssetName}";

    public static string GetHousingAssetName
    {
        get
        {
            var rewardValue = ServerContents.GroupRanking.GetSpineHousingReward().value;
            var housingIdx  = (int)(rewardValue / 10000);
            var modelIdx    = (int)(rewardValue % 10000);

            return $"{housingIdx}_{modelIdx}";
        }
    }
    #endregion

    /// <summary>
    /// 로비 오브젝트가 상시 오픈이기 때문에 이벤트 오픈 여부를 확인하기 위해 추가
    /// </summary>
    public static bool isEventOn;

    public static  bool IsDeadlineOver => Global.LeftTime(ServerContents.GroupRanking.deadlineTs) < 0;
    private static bool IsEventOver    => Global.LeftTime(ServerContents.GroupRanking.endTs)      < 0;

    [NonSerialized] public bool isTutorialEnd = false;
    [NonSerialized] public bool isPlayGroupRankingStage = false;
    
    public SideIcon.IconGroupRanking icon;
    public GroupRankingDataResp      serverData;

    private Coroutine _coPopupOpen;

    public static void Init()
    {
        // 로비 구역이 상시 오픈이라 CheckStartable 체크 제외
        if (isEventOn)
        {
            return;
        }

        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerGroupRanking>();
        if (instance == null)
        {
            return;
        }

        ManagerEvent.instance.RegisterEvent(instance);

        isEventOn = CheckStartable();
    }

    private void Awake() => instance = this;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void OnReboot()
    {
        if (instance != null)
        {
            isEventOn = false;
            Destroy(instance);
        }
    }

    public GameEventType GetEventType() => GameEventType.GROUP_RANKING;

    private static bool CheckStartable()
    {
        if (ServerContents.GroupRanking == null || ServerContents.GroupRanking.eventIndex == 0 || ServerRepos.UserGroupRanking == null)
        {
            return false;
        }

        if (IsEventOver)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 그룹랭킹 대상 스테이지인지 여부
    /// </summary>
    public static bool IsGroupRankingStage(bool checkDeadline = true)
    {
        if (instance == null || isEventOn == false)
        {
            return false;
        }

        // 참가 보상을 받지 않을 경우 이벤트 적용 X
        if (ServerRepos.UserGroupRanking.isParticipationRewardReceived == false)
        {
            return false;
        }

        if (checkDeadline && IsDeadlineOver)
        {
            return false;
        }

        if (Global.GameType == GameType.END_CONTENTS)
        {
            return true;
        }

        if (Global.GameType == GameType.NORMAL && ManagerData.IsStageAllClear() == false && GameData.User.stage == Global.stageIndex)
        {
            return true;
        }

        return false;
    }

    public static bool IsGroupRankingStagePlaying(bool checkDeadline = true)
    {
        if (instance == null || isEventOn == false)
        {
            return false;
        }

        if (checkDeadline && IsDeadlineOver)
        {
            return false;
        }
        
        if (Global.GameType != GameType.END_CONTENTS && Global.GameType != GameType.NORMAL)
        {
            return false;
        }

        return instance.isPlayGroupRankingStage;
    }

    /// <summary>
    /// 이벤트 상태 리턴
    /// </summary>
    public static EventState GetEventState()
    {
        if (IsEventOver)
        {
            // 이벤트 기간 종료
            return EventState.EVENT_END;
        }
        
        if (IsDeadlineOver)
        {
            // 참여 기간 종료
            if (ServerRepos.UserGroupRanking.isParticipationRewardReceived)
            {
                return EventState.REWARD;
            }
            else
            {
                // 이벤트 참여를 하지 못한 유저는 이벤트 종료 처리
                return EventState.EVENT_END;
            }
        }

        if (ServerRepos.UserGroupRanking.hasProfile                    == false ||
            ServerRepos.UserGroupRanking.isParticipationRewardReceived == false)
        {
            return EventState.NEED_PARTICIPATE;
        }

        // 위의 경우에 해당하지 않으면 이벤트 진행 가능
        return EventState.RUNNING;
    }

    /// <summary>
    /// 이벤트 상태에 따라 종료 기간 리턴
    /// </summary>
    public long GetEndTs()
    {
        switch (GetEventState())
        {
            case EventState.NEED_PARTICIPATE:
            case EventState.RUNNING:
                return ServerContents.GroupRanking.deadlineTs;
            case EventState.REWARD:
                return ServerContents.GroupRanking.endTs;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 로비 아이콘 생성
    /// </summary>
    public void OnIconPhase()
    {
        if (isEventOn && GetIconShowState())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconGroupRanking>(ManagerUI._instance.ScrollbarRight,
                (icon) =>
                {
                    icon.Init(ServerContents.GroupRanking);
                    this.icon = icon;
                });
        }

        bool GetIconShowState()
        {
            switch (GetEventState())
            {
                case EventState.NEED_PARTICIPATE:
                case EventState.RUNNING:
                case EventState.REWARD:
                    return true;
                case EventState.EVENT_END:
                default:
                    return false;
            }
        }
    }

    public bool IsEventStateEnd()
    {
        var eventState = GetEventState();
        return eventState == EventState.EVENT_END;
    }

    /// <summary>
    /// 로비 아이콘, 이벤트 배너 클릭 시 처리
    /// </summary>
    public void OnEventIconClick(object obj = null)
    {
        if (IsEventStateEnd())
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, () => { isTutorialEnd = true; });
            });
            return;
        }

        if (_coPopupOpen != null)
        {
            return;
        }

        _coPopupOpen = StartCoroutine(CoPreOpenRankingPopup());
    }

    /// <summary>
    /// 번들 로드 + 프로필 설정 팝업 or 랭킹 팝업 오픈
    /// </summary>
    private IEnumerator CoPreOpenRankingPopup()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);

        // 번들 로드 (번들 매니저에 미리 로드해두고 각 사용처에서 바로 사용할 수 있도록)
        string[] bundles = { SPINE_ASSET_BUNDLE_NAME, ATLAS_ASSET_BUNDLE_NAME, GetTextureAssetBundleName(), GetHousingAssetBundleName() };
        yield return ManagerAssetLoader._instance.CoPreBundleLoad(bundles);

        _coPopupOpen = null;
        NetworkLoading.EndNetworkLoading();

        if (ServerRepos.UserGroupRanking.hasProfile == false)
        {
            // 프로필 설정 팝업 오픈
            OpenPionProfilePopup(true);
        }
        else
        {
            OpenRankingPopup();
        }
    }

    /// <summary>
    /// 랭킹 팝업 오픈
    /// </summary>
    private void OpenRankingPopup()
    {
        if (IsEventStateEnd())
        {
            ManagerUI._instance.OpenPopupEventOver();
            return;
        }

        // 랭킹 팝업 오픈 전 데이터 요청
        ServerAPI.RequestGroupRankingData(ResponseGroupRankingData);

        void ResponseGroupRankingData(GroupRankingDataResp resp)
        {
            if (resp.IsSuccess)
            {
                serverData = resp;
                // 랭킹 팝업 오픈
                ManagerUI._instance.OpenPopup<UIPopupGroupRanking>((popup) => popup.InitData());

                // (참가, 랭킹) 보상 획득 체크, 그로시
                GetParticipationReward();
                GetRankingReward();
            }
        }

        void GetParticipationReward()
        {
            if (serverData.CanGetParticipationReward())
            {
                // 참가 보상 획득 완료
                ServerRepos.UserGroupRanking.isParticipationRewardReceived = true;
                icon?.UpdateIconState();

                // growthy
                SendRewardLog(serverData.participationReward, "ParticipationReward");
            }
        }

        void GetRankingReward()
        {
            if (serverData.CanGetRankingReward())
            {
                // 랭킹 보상 획득 완료
                ServerRepos.UserGroupRanking.isGetRankRewardReceived = true;

                // growthy
                SendRewardLog(serverData.rankingReward, "RankingReward");
            }
        }
    }

    /// <summary>
    /// 프로필 생성 팝업 출력.
    /// </summary>
    public void OpenPionProfilePopup(bool isParticipation, Action onUpdateMyProfile = null)
    {
        if (isParticipation)
        {
            // 그룹 랭킹 팝업 출력
            var profilePopup = ManagerUI._instance.OpenPopup<UIPopupPionProfile>();
            profilePopup.Init(PionProfileContentType.GROUP_RANKING, () =>
                {
                    ServerRepos.UserGroupRanking.hasProfile = true;
                    OpenRankingPopup();
                },
                () =>
                {
                    isTutorialEnd = true;
                });
        }
        else
        {
            // 그냥 팝업 닫히기
            var profilePopup = ManagerUI._instance.OpenPopup<UIPopupPionProfile>();
            profilePopup.Init(PionProfileContentType.GROUP_RANKING);
            profilePopup._callbackEnd += () => { onUpdateMyProfile?.Invoke(); };
        }
    }

    /// <summary>
    /// 보상 획득 후 남기는 그로시
    /// </summary>
    /// <param name="appliedRewardSet">보상 데이터</param>
    /// <param name="rewardType">보상 타입(참가, 랭킹, 점수)</param>
    public void SendRewardLog(AppliedRewardSet appliedRewardSet, string rewardType)
    {
        if (appliedRewardSet == null)
        {
            return;
        }

        if (appliedRewardSet.directApplied != null)
        {
            foreach (var reward in appliedRewardSet.directApplied)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.Value.type, reward.Value.valueDelta,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GROUP_RANKING_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GROUP_RANKING_REWARD,
                    rewardType
                );
            }
        }

        if (appliedRewardSet.mailReceived != null)
        {
            foreach (var reward in appliedRewardSet.mailReceived)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type, reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GROUP_RANKING_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GROUP_RANKING_REWARD,
                    rewardType
                );
            }
        }

        var str1 = $"{serverData.groupId}_{serverData.segment}_{serverData.myRankingUserData.rank}";
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.GROUP_RANK,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.GROUP_RANK_GET_REWARD,
            rewardType,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            (int)serverData.myRankingUserData.score,
            str1
        );
        var d = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }

    #region LobbyObject
    /// <summary>
    /// 에리어 오브젝트(섬) 등장 연출 출력 가능한지 여부
    /// </summary>
    public static bool CheckPlayStartCutScene()
    {
        if (ServerRepos.GroupRankingAreaOpen || PlayerPrefs.HasKey(GROUP_RANKING_EVENT_CUT_SCENE_KEY))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 에리어 오브젝트(섬) 등장 연출 출력 여부
    /// </summary>
    public bool OnLobbySceneCheck()
    {
        if (ManagerArea._instance.GetEventLobbyObject(GameEventType.GROUP_RANKING) != null && CheckPlayStartCutScene())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 에리어 오브젝트(섬) 생성 및 등록
    /// </summary>
    public IEnumerator OnLobbyObjectLoadPhase()
    {
        GameObject groupRankingAreaObj = null;

        NetworkLoading.MakeNetworkLoading(0.5f);
        var isLoad = false;

#if UNITY_EDITOR
        if (Global.LoadFromInternal)
        {
            groupRankingAreaObj = LoadEditorAreaAssetBundle();
            isLoad              = true;
        }
        else
        {
            LoadAreaAssetBundle();
        }
#else
         LoadAreaAssetBundle();
#endif

        yield return new WaitUntil(() => isLoad);
        NetworkLoading.EndNetworkLoading();

        GameObject obj = null;
        if (groupRankingAreaObj != null)
        {
            obj = ManagerLobby.NewObject(groupRankingAreaObj);
            var areaBase = obj.GetComponent<AreaBase>();
            if (areaBase != null)
            {
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
            }
        }

        yield return ManagerCharacter._instance.LoadCharacters();
        if (obj != null)
        {
            ManagerArea._instance.RegisterEventLobbyObject(obj.GetComponent<GroupRankingAreaBase>());
        }

        void LoadAreaAssetBundle() => ManagerAssetLoader._instance.BundleLoad<GameObject>(AREA_ASSET_BUNDLE_NAME, AREA_ASSET_NAME, OnComplete);

        void OnComplete(GameObject bundleObj)
        {
            if (bundleObj == null || bundleObj.GetComponent<GroupRankingAreaBase>() == null)
            {
                Debug.LogError("Asset Not found: " + AREA_ASSET_BUNDLE_NAME + "(not in assetDataList)");
            }
            else
            {
                groupRankingAreaObj = bundleObj;
            }

            isLoad = true;
        }
    }

    /// <summary>
    /// 에리어 오브젝트(섬) 등장 연출 출력
    /// </summary>
    /// <returns></returns>
    public IEnumerator OnLobbyScenePhase()
    {
        var lobbyObject = ManagerArea._instance.GetEventLobbyObject(GameEventType.GROUP_RANKING).GetAreaBase();
        if (lobbyObject == null)
        {
            yield break;
        }

        PlayerPrefs.SetInt(GROUP_RANKING_EVENT_CUT_SCENE_KEY, 1);
        ServerRepos.GroupRankingAreaOpen = true;

        yield return ManagerLobby._instance.WaitForSceneEnd(lobbyObject, 1);
    }
    #endregion

    #region Tutorial
    /// <summary>
    /// 튜토리얼 실행 가능 여부 반환
    /// </summary>
    public bool OnTutorialCheck()
    {
        if (isEventOn && PlayerPrefs.HasKey(GROUP_RANKING_EVENT_TUTORIAL_KEY) == false)
        {
            var eventState = GetEventState();
            if (eventState != EventState.EVENT_END && eventState != EventState.REWARD)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 튜토리얼 실행
    /// </summary>
    public IEnumerator OnTutorialPhase()
    {
        ManagerEvent.instance.isTutorialPlay = true;

        PlayerPrefs.SetInt(GROUP_RANKING_EVENT_TUTORIAL_KEY, 1);

        ManagerTutorial.PlayTutorial(TutorialType.TutorialGroupRanking);
        if (ManagerTutorial._instance != null)
        {
            yield return new WaitUntil(IsTutorialEnd);
        }
    }

    /// <summary>
    /// 튜토리얼 종료 가능 여부 반환 (그룹랭킹 관련 팝업이 출력되고 있을 경우 튜토리얼이 종료되지 않도록)
    /// </summary>
    private bool IsTutorialEnd()
    {
        if (ManagerTutorial._instance._playing || isTutorialEnd == false)
        {
            return false;
        }

        return true;
    }
    #endregion

#if UNITY_EDITOR
    public NGUIAtlas LoadEditorAtlasAssetBundle()
    {
        var path  = $"Assets/5_OutResource/GroupRanking/{ATLAS_ASSET_BUNDLE_NAME}/atlas/{ATLAS_ASSET_NAME}.asset";
        var atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<NGUIAtlas>(path);

        if (atlas == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return null;
        }
        else
        {
            return atlas;
        }
    }

    public Texture LoadEditorTextureAssetBundle(string textureName)
    {
        var path    = $"Assets/5_OutResource/GroupRanking/{GetTextureAssetBundleName()}/{textureName}.png";
        var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);

        if (texture == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return null;
        }
        else
        {
            return texture;
        }
    }

    public GameObject LoadEditorSpineAssetBundle()
    {
        var path = $"Assets/5_OutResource/GroupRanking/{SPINE_ASSET_BUNDLE_NAME}/{SPINE_ASSET_NAME}.prefab";
        var obj  = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (obj == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return null;
        }

        return obj;
    }

    private GameObject LoadEditorAreaAssetBundle()
    {
        var path = $"Assets/5_OutResource/GroupRanking/{AREA_ASSET_BUNDLE_NAME}/{AREA_ASSET_NAME}.prefab";
        var obj  = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (obj == null || obj.GetComponent<GroupRankingAreaBase>() == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return null;
        }

        return obj;
    }
#endif

    #region 사용하지 않는 함수
    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList,
        Action<ManagerAssetLoader.ResultCode, string>                    failCallback)
    {
    }

    public void OnLobbyStart()
    {
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }
    #endregion
}