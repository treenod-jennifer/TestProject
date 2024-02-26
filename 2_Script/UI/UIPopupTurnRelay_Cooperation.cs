using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;

public class UIPopupTurnRelay_Cooperation : UIPopupBase
{
    public static UIPopupTurnRelay_Cooperation _instance = null;

    //남은 시간
    [SerializeField] private UILabel labelRemainTime;

    [Header("CoopMissionReward")]
    #region 상단 보상창 UI
    [SerializeField] private UITexture textureRewardBG;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel labelProgress;
    [SerializeField] private UILabel[] labelCoopMissionStep;
    [SerializeField] private List<UIItemTurnRelay_CooperationReward> listItemCooperationReward;
    #endregion

    [Header("CoopRanking")]
    #region 랭킹창 UI
    [SerializeField] private UILabel loadingText;
    [SerializeField] private UIPanel scrollPanel;
    #endregion

    //LAN 페이지 설정
    [SerializeField] private UIItemLanpageButton lanpageButton;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    //보상받을 때 콜백
    public System.Action getRewardAction = null;

    [HideInInspector] public List<ManagerTurnRelay.TurnRelayRankData> _listRankingDatas = new List<ManagerTurnRelay.TurnRelayRankData>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer <= 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public ManagerTurnRelay.TurnRelayRankData GetTurnRelayRankData(int index)
    {
        if (_listRankingDatas.Count <= index || _listRankingDatas[index] == null)
            return null;

        return _listRankingDatas[index];
    }

    private IEnumerator Start()
    {
        InitPopupBG();
        InitBtnLanPage();
        InitTime();
        InitCoopProgress_Default();
        InitCoopReward_Default();

        //랭킹 데이터 받아올 더미 리스트
        List<ManagerTurnRelay.TurnRelayRankData> listTempDataTurnRelay = new List<ManagerTurnRelay.TurnRelayRankData>();

        //그룹 랭킹 정보 받아오기
        yield return ManagerTurnRelay.turnRelayCoop.QueryGroupRanking(listTempDataTurnRelay);

        #if UNITY_EDITOR
        yield return MakeFriend();
        #endif

        for (int i = 0; i < listTempDataTurnRelay.Count; ++i)
        {
            _listRankingDatas.Add(listTempDataTurnRelay[i]);
        }

        //데이터 모두 다운 받은 뒤, 설정
        OnDataComplete();

        //서버 데이터와 싱크 맞추기
        ManagerTurnRelay.turnRelayCoop.SyncFromServerUserData();

        //프로그레스 및 보상 설정 초기화
        InitCoopProgress();
        InitCoopReward();
    }

    #region 초기화 관련
    public void InitCallBack(System.Action getRewardAction, System.Action endAction)
    {
        this.getRewardAction = getRewardAction;
        this._callbackClose += () => { endAction(); };
    }

    private void InitPopupBG()
    {
        gameObject.AddressableAssetLoad<Texture>("local_turn_relay/turnRelay_Cooperation_BG", texture =>
        {
            textureRewardBG.mainTexture = texture;
            textureRewardBG.width = 720;
            textureRewardBG.height = 298;
        });
    }

    private void InitCoopProgress_Default()
    {
        //프로그레스 카운트 설정.
        labelProgress.text = "0/0";

        //단계 텍스트 설정
        for (int i = 0; i < labelCoopMissionStep.Length; i++)
            labelCoopMissionStep[i].gameObject.SetActive(false);
    }

    private void InitCoopProgress()
    {
        int currentIdx = ManagerTurnRelay.turnRelayCoop.currentMissionIdx;

        ManagerTurnRelay.CoopMissionData missionData = ManagerTurnRelay.turnRelayCoop.GetCoopMission(currentIdx);
        if (missionData == null)
            return;

        //프로그레스 바 설정.
        float progressOffset = 100f / missionData.targetCount;
        progressBar.value = (missionData.progress * progressOffset) * 0.01f;

        //프로그레스 카운트 설정.
        labelProgress.text = string.Format("{0}/{1}", missionData.progress, missionData.targetCount);

        //단계 텍스트 설정
        string textStep = Global._instance.GetString("p_tr_m_2")
         .Replace("[n]", (currentIdx + 1).ToString());

        for (int i = 0; i < labelCoopMissionStep.Length; i++)
        {
            labelCoopMissionStep[i].gameObject.SetActive(true);
            labelCoopMissionStep[i].text = textStep;
        }
    }

    public void InitCoopReward_Default()
    {
        for (int i = 0; i < listItemCooperationReward.Count; i++)
        {
            listItemCooperationReward[i].InitCoopMissionReward_Default();
        }
    }

    public void InitCoopReward()
    {
        for (int i = 0; i < listItemCooperationReward.Count; i++)
        {
            listItemCooperationReward[i].InitCoopMissionReward(i);
        }
    }
    #endregion

    #region MakeFriend_EditorTest
    IEnumerator MakeFriend()
    {
        _listRankingDatas.Clear();
        {
            //친구정보 생성
            for (int i = 1; i < 31; i++)
            {
                ManagerTurnRelay.TurnRelayRankData fp = new ManagerTurnRelay.TurnRelayRankData();

                fp.rank = i;
                fp.ingameName = "TestFriend" + i;
                fp.scoreValue = (40 - i) * 1000;
                fp.userKey = "TF" + i;
                fp.photoUseAgreed = i % 5 == 0 ? false : true;

                if (i == 1)
                {
                    UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
                    fp.userKey = myProfile._userKey;
                }

                //게임을 하는 친구
                _listRankingDatas.Add(fp);

                yield return null;
            }
        }
    }

    #endregion

    private void OnDataComplete()
    {
        if (_listRankingDatas.Count == 0) 
            return;

        callbackDataComplete?.Invoke();
        loadingText.gameObject.SetActive(false);
    }

    #region LAN 페이지 설정
    private void InitBtnLanPage()
    {
        //LAN 페이지 설정
        lanpageButton.On("LGPKV_goldfish_event", Global._instance.GetString("p_tr_m_10"));
    }
    #endregion

    #region 시간 표시 관련
    private void InitTime()
    {
        var eventState = ManagerTurnRelay.GetEventState();
        switch (eventState)
        {
            case ManagerTurnRelay.EventState.BEFORE_REWARD:
                SetTime_BeforeReward();
                break;
            case ManagerTurnRelay.EventState.REWARD:
                SetTime_Reward();
                break;
            default:
                SetTime_Play();
                break;
        }
    }

    //플레이 중 타이머
    private void SetTime_Play()
    {
        EndTsTimer.Run(target: labelRemainTime, endTs: ManagerTurnRelay.instance.PlayEndTs, timeOutAction: SetTime_BeforeReward);
    }

    //보상 집계 기간 중 타이머
    private void SetTime_BeforeReward()
    {
        EndTsTimer.Run(
            target: labelRemainTime,
            endTs: ManagerTurnRelay.instance.BeforeRewardEndTs,
            overrideTextFunc: (ts) => $"{Global._instance.GetString("p_tr_m_3")}({Global.GetLeftTimeText(ManagerTurnRelay.instance.BeforeRewardEndTs)})",
            timeOutAction: SetTime_Reward);
    }

    //보상 수령 기간 중 타이머
    private void SetTime_Reward()
    {
        EndTsTimer.Run(
            target: labelRemainTime,
            endTs: ManagerTurnRelay.instance.RewardEndTs,
            overrideTextFunc: (ts) => $"{Global._instance.GetString("p_tr_m_4")}({Global.GetLeftTimeText(ManagerTurnRelay.instance.RewardEndTs)})");
    }
    #endregion
}
