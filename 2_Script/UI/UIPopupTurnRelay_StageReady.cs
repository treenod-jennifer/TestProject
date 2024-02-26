using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Spine.Unity;
using DG.Tweening;
using Newtonsoft.Json;

public class UIPopupTurnRelay_StageReady : UIPopupBase
{
    public static UIPopupTurnRelay_StageReady _instance = null;

    [SerializeField] UIPanel panel_upperSpine;

    [Header("**BackGround")]
    //배경 스파인
    [SerializeField] private SkeletonAnimation spineReadyBG;
    [SerializeField] private UITexture textureTitle;
    [SerializeField] private UITexture textureBG;

    //상단 UI
    [SerializeField] private UILabel labelRemainTime;
    [SerializeField] private UILabel labelTokenCount;

    [Header("**Button")]
    [SerializeField] private UISprite spriteBtnPlay;    
    [SerializeField] private BoxCollider collederBtnPlay;
    [SerializeField] private GameObject objBtnCapsuleToy;
    [SerializeField] private GameObject objBtnTextEventEndRoot;
    [SerializeField] private UILabel[] labelBtnTextEventEnd;
    [SerializeField] private GameObject objBtnCooperation;
    [SerializeField] private GameObject objBtnMission;

    [Header("**AP")]
    //AP표시
    [SerializeField] private GameObject objBtnTextAPRoot;
    [SerializeField] private UILabel[] labelBtnCurrntAp;
    [SerializeField] private UILabel[] labelBtnAllAp;

    //AP시간표시
    [SerializeField] private UILabel labelAPRechargeTime;

    //광고 말풍선
    [SerializeField] private GameObject objADBubble;
    [SerializeField] private GameObject objIcon_AD;
    [SerializeField] private GameObject objIcon_Coin;

    [Header("**Alarm")]
    //알림
    [SerializeField] private GameObject objAlarm_subMission;
    [SerializeField] private UILabel labelAlarm_subMission;
    [SerializeField] private UILabel labelProgress_subMission;
    [SerializeField] private GameObject objAlarm_coop;
    [SerializeField] private UILabel labelAlarm_coop;

    //LAN 페이지 설정
    [SerializeField] private UIItemLanpageButton lanpageButton;

    [Header("**ReadyItem")]
    //레디 아이템 리스트
    [SerializeField] private GameObject objReadyItemBlind;
    [SerializeField] private List<UIItemTurnRelay_ReadyItem> listReadyItem = new List<UIItemTurnRelay_ReadyItem>();

    //선택한 아이템 저장
    private Dictionary<READY_ITEM_TYPE, READY_ITEM_STATE> dicItemSelect = new Dictionary<READY_ITEM_TYPE, READY_ITEM_STATE>();

    //코루틴
    private Coroutine apTimeRoutine = null;
    private Coroutine adBubbleRoutine = null;
    private Coroutine btnCapsuleRoutine = null;

    #region 시작 버튼 상태
    private enum ButtonState
    {
        CAN_PLAY,
        WAIT,
        EVENT_END
    }
    private ButtonState btnStartState = ButtonState.CAN_PLAY;
    #endregion

    #region 텍스쳐 다운로드
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
    #endregion

    private int clearLayer = 1;

    //보상 검사 끝났는지 검사해서 터치 활성화 해줌
    private bool isEndOpenPopupAction = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    private IEnumerator Start()
    {
        if(ManagerTurnRelay.IsRankingEnabled() == false)
        {
            this.objBtnMission.transform.localPosition = new Vector3(-280, 15, 0);
            this.objBtnCooperation.gameObject.SetActive(false);
        }

        //팝업 오픈 연출 대기
        yield return new WaitUntil(() => isEndOpenPopupAction == true);

        //터치 상태 활성화
        SetIsCanTouchPopup(true);

        // 이벤트 시간에 따른 처리
        StartCoroutine(CoCheckEventTime());
    }

    #region 이벤트 시간에 따른 처리
    //이벤트 시간 검사해주는 함수
    private IEnumerator CoCheckEventTime()
    {
        var eventState = ManagerTurnRelay.GetEventState();
        switch (eventState)
        {
            case ManagerTurnRelay.EventState.RUNNING:
                {
                    //현재의 상태가 끝나면 실행되는 다음 상태.
                    yield return ChangePopupSetting(ManagerTurnRelay.instance.PlayEndTs, "n_ev_12", false, () => allPopupClose());
                }
                break;
            case ManagerTurnRelay.EventState.REWARD:
                {
                    //협동랭킹 보상 처리
                    yield return RewardProc();

                    //현재의 상태가 끝나면 실행되는 다음 상태.
                    yield return ChangePopupSetting(ManagerTurnRelay.instance.RewardEndTs, "n_ev_13", true, () => allPopupClose());
                }
                break;
        }
    }

    private IEnumerator ChangePopupSetting(long turnRelayTs, string systemText, bool finalLineState = false, Method.FunctionVoid endCallBack = null)
    {
        long RemainingTime = Global.LeftTime(turnRelayTs);

        yield return new WaitForSeconds(RemainingTime);

        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(systemText), false, endCallBack);
    }

    private void allPopupClose()
    {
        foreach (var uiPopup in ManagerUI._instance._popupList)
            uiPopup.ClosePopUp();
    }

    /// <summary>
    /// 협동 미션 보상 처리
    /// </summary>
    private IEnumerator RewardProc()
    {
        if (!NetworkSettings.Instance.IsRealDevice())
        {
            Debug.Log(" === GetReward");
            yield break;
        }

        if (ServerRepos.UserTurnRelayEvent == null)
            yield break;

        var userTurnRelay = ServerRepos.UserTurnRelayEvent;

        //유저가 보상 받을 수 없는 상태이거나 이미 받았는지 검사
        if (userTurnRelay.clear == 0 || userTurnRelay.coopRankRewardFlag == true)
            yield break;

        //터치 막음
        SetIsCanTouchPopup(false);

        //보상 받는 함수가 종료되었는지 검사하기 위한 변수
        bool isGetReward = false;

        ServerAPI.TurnRelayReqCoopRankReward((Protocol.TurnRelayRankRewardResp resp) =>
        {
            if (resp.IsSuccess == true)
            {
                //보상 팝업 띄우기
                if (resp.clearReward != null)
                {
                    ManagerUI._instance.OpenPopupGetRewardAlarm
                        (Global._instance.GetString("n_ev_11"),
                        () => { isGetReward = true; },
                        resp.clearReward);
                }
                else
                {
                    isGetReward = true;
                }

                //보상 UI 갱신
                RefreshRewardUI();

                //재화 UI 갱신
                ManagerUI._instance.SyncTopUIAssets();
                ManagerUI._instance.UpdateUI();

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                      (int)resp.reward.type,
                      resp.reward.value,
                      ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TURN_RELAY_COOP_RANK_REWARD,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TURN_RELAY_COOP_RANK_REWARD,
                      $"TURNRELAY_{ServerContents.TurnRelayEvent.eventIndex}_COR_RANK_{resp.rank}"
                      );
            }
        });

        //보상 받을 때 까지 대기
        yield return new WaitUntil(() => isGetReward == true);

        //터치 가능하도록 해제
        SetIsCanTouchPopup(true);
    }
    #endregion

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        panel_upperSpine.depth = depth + 1;
    }

    protected override IEnumerator CoPostOpenPopup()
    {
        yield return new WaitForSeconds(openTime);

        if (_callbackOpen != null)
        {
            _callbackOpen();
            yield return new WaitForSeconds(openTime);
        }
        ManagerUI._instance.FocusCheck();
        isEndOpenPopupAction = true;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        clearLayer = layer;

        //클리어창 전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d, spine만 레이어 올려줌.
        if (layer == 10)
        {
            spineReadyBG.GetComponent<MeshRenderer>().sortingOrder = clearLayer;
            clearLayer += 1;
        }
        else
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            spineReadyBG.GetComponent<MeshRenderer>().sortingOrder = clearLayer + 1;
            clearLayer += 2;
        }
        panel_upperSpine.useSortingOrder = true;
        panel_upperSpine.sortingOrder = clearLayer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup()
    {
        //팝업 배경 텍스쳐 설정
        InitPopupBG();

        //시간 설정
        InitTime();

        //알람 설정
        InitAlarm_SubMission();
        StartCoroutine(CoInitAlarm_Cooperation());

        //버튼 설정
        RefreshButton();
        InitBtnLanPage();
        InitBtnCapsuleToyAction();

        //토큰 카운트 설정
        RefreshTokenCount();

        //레디 아이템 UI및 선택 상태 초기화
        InitReadyItem();
    }

    private void InitPopupBG()
    {
        textureBG.mainTexture = ManagerTurnRelay.turnRelayResource.turnRelayPack?.StageReadyBGTexture;
        textureBG.MakePixelPerfect();
        textureBG.width = 701;
        textureBG.height = 557;

        textureTitle.mainTexture = ManagerTurnRelay.turnRelayResource.turnRelayPack?.StageReadyTitleTexture;
        textureTitle.MakePixelPerfect();
        textureTitle.width = 782;
        textureTitle.height = 213;
    }

    #region 버튼 초기화
    public void RefreshButton()
    {
        if (apTimeRoutine != null)
        {
            StopCoroutine(apTimeRoutine);
            apTimeRoutine = null;
        }

        var eventState = ManagerTurnRelay.GetEventState();
        switch (eventState)
        {
            case ManagerTurnRelay.EventState.NEED_CREATE_PROFILE:
            case ManagerTurnRelay.EventState.RUNNING:
                SetButton_Active();
                break;
            default:
                SetButton_InActive();
                break;
        }
    }

    public static event System.Action playCallBack;

    private void SetButton_Active()
     {
        spriteBtnPlay.spriteName = "button_play";

        bool isRemainAP = (ManagerTurnRelay.instance.EventAP > 0);
        string allAP = ManagerTurnRelay.instance.MaxEventAP.ToString();
        string currentAp = "0/";

        //남은 AP 상태에 따른 처리
        if (isRemainAP == true)
        {
            btnStartState = ButtonState.CAN_PLAY;
            currentAp = string.Format("{0}/", ManagerTurnRelay.instance.EventAP);
            objADBubble.SetActive(false);
            playCallBack?.Invoke();
            if(ManagerTurnRelay.instance != null)
                ManagerTurnRelay.instance.SetNewIcon();
        }
        else
        {
            btnStartState = ButtonState.WAIT;
            objADBubble.SetActive(true);
            StartActionADBubble();
        }

        //버튼 텍스트 설정
        for (int i = 0; i < labelBtnCurrntAp.Length; i++)
            labelBtnCurrntAp[i].text = currentAp;

        for (int i = 0; i < labelBtnAllAp.Length; i++)
            labelBtnAllAp[i].text = allAP;

        objBtnTextAPRoot.SetActive(true);
        objBtnTextEventEndRoot.SetActive(false);

        //시간 타이머 재생
        if (ManagerTurnRelay.instance.EventAP < ManagerTurnRelay.instance.MaxEventAP)
            apTimeRoutine = StartCoroutine(CoApChargeTimer());
        else
            labelAPRechargeTime.text = Global._instance.GetString("p_t_tr_3");
    }

    private void SetButton_InActive()
    {
        btnStartState = ButtonState.EVENT_END;
        spriteBtnPlay.spriteName = "button_play03";
        labelAPRechargeTime.text = "00:00:00";

        //AP관련 UI 비활성화
        objADBubble.SetActive(false);
        objBtnTextAPRoot.SetActive(false);
        collederBtnPlay.enabled = false;

        //이벤트 종료 UI 활성화
        objBtnTextEventEndRoot.SetActive(true);
    }

    private void StartActionADBubble()
    {
        if(adBubbleRoutine != null)
        {
            StopCoroutine(adBubbleRoutine);
            adBubbleRoutine = null;
        }

        objIcon_AD.SetActive(true);
        adBubbleRoutine = StartCoroutine(CoActionADBubble());
    }

    private IEnumerator CoActionADBubble()
    {
        while (true)
        {
            if (objADBubble.activeInHierarchy == false)
                break;

            yield return new WaitForSeconds(1.0f);
            bool isActiveIcon = !objIcon_AD.activeInHierarchy;
            objIcon_AD.SetActive(isActiveIcon);
        }
        adBubbleRoutine = null;
    }

    private IEnumerator CoApChargeTimer()
    {
        while (true)
        {
            long leftTime = Global.LeftTime(ManagerTurnRelay.instance.APRechargeTime);
            if (leftTime <= 0)
                break;

            string timeText = Global.GetTimeText_HHMMSS(leftTime, false);
            labelAPRechargeTime.text = timeText;
            yield return null;
        }
        apTimeRoutine = null;
        labelAPRechargeTime.text = "00:00:00";
        ServerAPI.TurnRelayRefreshAP(RecvRechargeAP_ByTime);
    }

    private void RecvRechargeAP_ByTime(TurnRelayResp resp)
    {
        if (resp.IsSuccess == true && ManagerTurnRelay.instance != null)
        {
            //유저 데이터 갱신
            ManagerTurnRelay.instance.SyncFromServerUserData();

            //버튼 상태 변경
            SetButton_Active();
        }
    }
    #endregion

    //보상으로 받을 수 있는 UI 갱신할 때 사용하는 함수
    private void RefreshRewardUI()
    {
        RefreshTokenCount();
        RefreshReadyItem();
    }

    #region 레디 아이템 관련
    private void InitReadyItem()
    {
        for (int i = 0; i < listReadyItem.Count; i++)
        {   //레디 아이템 선택되지 않은 상태로 UI 초기화
            listReadyItem[i].InitItem(SelectItem);
        }

        //선택 상태에 따라 초기화
        InitReadyItemSelectState_ByPlayerPrefs();
    }

    private void RefreshReadyItem()
    {
        for (int i = 0; i < listReadyItem.Count; i++)
        {
            listReadyItem[i].UnSelectAndRefreshItem();
        }

        //선택 상태에 따라 초기화
        InitReadyItemSelectState_ByPlayerPrefs();
    }
    #endregion

    #region 이벤트 남은 시간 표시 관련
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
            case ManagerTurnRelay.EventState.EVENT_END:
                SetTime_EventEnd();
                break;
            default:
                SetTime_Play();
                break;
        }
    }

    //플레이 중 타이머
    private void SetTime_Play()
    {
        objReadyItemBlind.SetActive(false);
        EndTsTimer.Run(target: labelRemainTime, endTs: ManagerTurnRelay.instance.PlayEndTs, 
            timeOutAction: () => {
                RefreshButton();
                SetTime_BeforeReward();
            });
    }

    //보상 집계 기간 중 타이머
    private void SetTime_BeforeReward()
    {
        objReadyItemBlind.SetActive(true);
        EndTsTimer.Run(
            target: labelRemainTime,
            endTs: ManagerTurnRelay.instance.BeforeRewardEndTs,
            overrideTextFunc: (ts) => $"{Global._instance.GetString("p_tr_m_3")}({Global.GetLeftTimeText(ManagerTurnRelay.instance.BeforeRewardEndTs)})",
            timeOutAction: SetTime_Reward);
    }

    //보상 수령 기간 중 타이머
    private void SetTime_Reward()
    {
        objReadyItemBlind.SetActive(true);
        EndTsTimer.Run(
            target: labelRemainTime,
            endTs: ManagerTurnRelay.instance.RewardEndTs,
            overrideTextFunc: (ts) => $"{Global._instance.GetString("p_tr_m_4")}({Global.GetLeftTimeText(ManagerTurnRelay.instance.RewardEndTs)})",
            timeOutAction: SetTime_EventEnd);
    }

    //이벤트 종료시 타이머
    private void SetTime_EventEnd()
    {
        objReadyItemBlind.SetActive(true);
        labelRemainTime.text = "00:00:00";
    }
    #endregion

    #region 알림 초기화
    private void InitAlarm_SubMission()
    {
        //서브미션 데이터 서버와 싱크 맞추기
        ManagerTurnRelay.turnRelaySubMission.SyncFromServerUserData();

        int canReciveCount = ManagerTurnRelay.turnRelaySubMission.GetCanReciveRewardCount();
        if (canReciveCount == 0)
        {
            objAlarm_subMission.SetActive(false);
        }
        else
        {
            objAlarm_subMission.SetActive(true);
            labelAlarm_subMission.text = canReciveCount.ToString();
        }

        labelProgress_subMission.text = string.Format("{0}/{1}",
            canReciveCount + ManagerTurnRelay.turnRelaySubMission.GetRecivedRewardCount(), 
            ManagerTurnRelay.turnRelaySubMission.listSubMission.Count);
    }

    private IEnumerator CoInitAlarm_Cooperation()
    {
        objAlarm_coop.SetActive(false);

        //게임에 한번도 참여하지 않은 상태라면, 알림 표시하지 않음.
        if (ServerRepos.UserTurnRelayEvent == null || ServerRepos.UserTurnRelayEvent.clear == 0)
            yield break;

        if (ManagerTurnRelay.IsRankingEnabled() == false)
            yield break;

        if (ManagerTurnRelay.turnRelayCoop.groupScore == -1)
        {
            //그룹 랭킹 정보 받아오기
            yield return ManagerTurnRelay.turnRelayCoop.QueryGroupRanking_ForAlarm();

            //협동 미션 데이터 초기화
            ManagerTurnRelay.turnRelayCoop.SyncFromServerUserData();
        }

        //보상을 받을 수 있는 협동 미션을 검사하고, 카운트에 따라 알람 설정
        int alarmCount = ManagerTurnRelay.turnRelayCoop.GetCanReciveRewardCount();
        if (alarmCount > 0)
        {
            objAlarm_coop.SetActive(true);
            labelAlarm_coop.text = alarmCount.ToString();
        }
    }
    #endregion

    #region LAN 페이지 설정
    private void InitBtnLanPage()
    {
        //LAN 페이지 설정
        lanpageButton.On("LGPKV_goldfish_mission", Global._instance.GetString("p_tr_2"));
    }
    #endregion

    #region 캡슐토이 아이콘 연출
    private void InitBtnCapsuleToyAction()
    {
        if (btnCapsuleRoutine != null)
        {
            StopCoroutine(btnCapsuleRoutine);
            btnCapsuleRoutine = null;
        }

        btnCapsuleRoutine = StartCoroutine(CoActionBtnCapsuleToy());
    }

    private IEnumerator CoActionBtnCapsuleToy()
    {
        while (true)
        {
            if (this.gameObject.activeInHierarchy == false)
                break;

            yield return new WaitForSeconds(3.0f);
            objBtnCapsuleToy.transform.DOPunchRotation(new Vector3(0f, 0f, 10f), 0.5f);
        }
    }
    #endregion

    private void RefreshTokenCount()
    {
        int tokenCount = 0;
        if (ServerRepos.UserTokenAssets != null && ServerRepos.UserTokenAssets.TryGetValue(2, out var value))
            tokenCount = ServerRepos.UserTokenAssets[2].amount;

        labelTokenCount.text = tokenCount.ToString();
    }

    //PlayerPrefs에 저장된 값으로 레디 아이템 선택 상태를 초기화시켜줌.
    private void InitReadyItemSelectState_ByPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(Global.GameInstance.GetReadyItemSelectKey() + "0") == false)
            return;

        //더블 사과가 선택되어 있는데 현재 사용 불가하다면, 일반 레디 아이템으로 선택상태 변환
        int doubleAddTurnIdx = (int)READY_ITEM_TYPE.DOUBLE_ADD_TURN;
        if (Global.GameInstance.CanUseReadyItem(doubleAddTurnIdx) == false &&
            PlayerPrefs.GetInt(Global.GameInstance.GetReadyItemSelectKey() + doubleAddTurnIdx) == 1)
        {
            int addTurnIdx = (int)READY_ITEM_TYPE.ADD_TURN;
            if (Global.GameInstance.CanUseReadyItem(addTurnIdx) == true)
            {
                //더블체크 제거
                PlayerPrefs.SetInt(Global.GameInstance.GetReadyItemSelectKey() + doubleAddTurnIdx, 0);
                
                //단일체크 추가
                PlayerPrefs.SetInt(Global.GameInstance.GetReadyItemSelectKey() + addTurnIdx, 1);
            }
        }

        //현재 선택된 아이템들의 가격의 합을 구함
        int totalCost_Coin = 0;
        int totlaCost_Jewel = 0;
        
        for (int i = 0; i < listReadyItem.Count; i++)
        {
            //유저가 선택하지 못하는 아이템인 경우에는 검사하지 않음
            if (listReadyItem[i].ReadyItemState != READY_ITEM_STATE.NONE)
                continue;
            
            //현재 아이템이 더블 레디 아이템으로 변환될 수 있는 상태라면, 더블 레디 아이템의 선택 여부 검사
            bool isSelectDoubleReady = false;
            READY_ITEM_TYPE doubleType = Global._instance.GetDoubleReadyItemType(listReadyItem[i].originItemType);
            if (Global._instance.CheckReadyItemType_DoubleItem(doubleType) == true
                && Global.GameInstance.CanUseReadyItem((int)doubleType) == true)
            {   //더블 레디 아이템이 선택된 상황에서 코인 설정
                if (PlayerPrefs.GetInt(Global.GameInstance.GetReadyItemSelectKey() + (int)doubleType) > 0)
                {
                    isSelectDoubleReady = true;
                    totalCost_Coin += ServerRepos.LoginCdn.DoubleReadyItems[UIPopupReady.SERVER_DOUBLEREADY_INDEX];
                }
            }
            
            if (isSelectDoubleReady == false)
            {   //더블 레디 아이템이 선택되지 않은 상태에서 일반 아이템 가격 검사
                //유저가 해당 아이템을 1개 이상 소지하고 있을 땐, 검사하지 않음
                if (ServerRepos.UserItem.ReadyItem(listReadyItem[i].index) > 0)
                    continue;

                //선택된 아이템의 가격의 합을 종류별로 저장
                int index = listReadyItem[i].index;
                if (PlayerPrefs.GetInt(Global.GameInstance.GetReadyItemSelectKey() + index) > 0)
                {
                    if (listReadyItem[i].costType == UIItemTurnRelay_ReadyItem.READYITEM_COSTTYPE.COIN)
                        totalCost_Coin += ServerRepos.LoginCdn.ReadyItems[index];
                    else
                        totlaCost_Jewel += ServerRepos.LoginCdn.ReadyItems[index];
                }
            }
        }

        //구매 가능한 아이템만 선택 상태 변경시켜줌
        bool isCanSelectItem_CoinType = (Global.coin >= totalCost_Coin);
        bool isCanSelectItem_JewelType = (Global.jewel >= totlaCost_Jewel);
        for (int i = 0; i < 8; i++)
        {
            string prefsKey = Global.GameInstance.GetReadyItemSelectKey() + i;

            //아이템 선택되지 않은 상태라면 무시
            if (PlayerPrefs.GetInt(prefsKey) == 0)
                continue;

            if (Global.GameInstance.CanUseReadyItem(i) == false)
            {   //사용할 수 없는 레디 아이템 타입이라면 선택 해제
                PlayerPrefs.SetInt(prefsKey, 0);
                continue;
            }

            READY_ITEM_TYPE itemType = (READY_ITEM_TYPE)i;
            
            //검사하는 타입이 더블 레디 아이템이라면, 해당 아이템의 일반 타입을 가져와 검사
            READY_ITEM_TYPE originItemType = Global._instance.CheckReadyItemType_DoubleItem(itemType)
                ? Global._instance.GetSingleReadyItemType(itemType) : itemType;
                
            int findIndex = listReadyItem.FindIndex(x => x.originItemType == originItemType);
            if (findIndex == -1)
            {   //아이템 타입과 일치하는 레디 아이템이 없다면 선택 해제
                PlayerPrefs.SetInt(prefsKey, 0);
                continue;
            }

            //구매 재화가 부족한 지 검사
            UIItemTurnRelay_ReadyItem readyItem = listReadyItem[findIndex];
            bool isCanSelectItem = true;
            switch (readyItem.costType)
            {
                case UIItemTurnRelay_ReadyItem.READYITEM_COSTTYPE.COIN:
                    isCanSelectItem = isCanSelectItem_CoinType;
                    break;
                case UIItemTurnRelay_ReadyItem.READYITEM_COSTTYPE.JEWEL:
                    isCanSelectItem = isCanSelectItem_JewelType;
                    break;
            }

            //아이템 선택 불가능한 상황이면, 키 값을 변경 시켜줌
            if (readyItem.ReadyItemState != READY_ITEM_STATE.NONE || isCanSelectItem == false)
            {
                PlayerPrefs.SetInt(prefsKey, 0);
            }
            else
            {   //아이템 선택 가능하다면 상태 변화
                readyItem.IsSelectItem = true;
                readyItem.SetSelectItem(itemType);
                readyItem.SetItemUI_Select();
            }
        }
    }

    private void SelectItem(READY_ITEM_TYPE type, READY_ITEM_STATE state, int index, bool isSelect)
    {
        string prefsKey = Global.GameInstance.GetReadyItemSelectKey() + index;

        //일반 상태의 아이템이 아닐 경우, 항상 선택상태이며 PlayerPrefs에 저장되지 않음.
        if (state != READY_ITEM_STATE.NONE)
        {
            if (dicItemSelect.ContainsKey(type) == false)
                dicItemSelect.Add(type, state);
            else
                dicItemSelect[type] = state;
        }
        else
        {
            if (isSelect == false)
            {
                if (dicItemSelect.ContainsKey(type) == true)
                    dicItemSelect.Remove(type);
                PlayerPrefs.SetInt(prefsKey, 0);
            }
            else
            {
                if (dicItemSelect.ContainsKey(type) == false)
                    dicItemSelect.Add(type, state);
                else
                    dicItemSelect[type] = state;
                PlayerPrefs.SetInt(prefsKey, 1);
            }
        }
    }

    /// <summary>
    /// 레디 아이템 선택 여부를 검사 - 유저가 선택해서 선택된 아이템인지
    /// (선택된 아이템의 상태가 None 이 아니라면 유저가 선택한 아이템이 아님)
    /// </summary>
    private bool IsSelectReadyItem_ByUserSelect(READY_ITEM_TYPE type)
    {
        if (dicItemSelect.ContainsKey(type) == false)
            return false;
        else
            return dicItemSelect[type] == READY_ITEM_STATE.NONE;
    }

    /// <summary>
    /// 레디 아이템 선택 여부를 검사 - 자동으로 선택된 아이템인지    
    /// </summary>
    private bool IsSelectReadyItem_ByAutoSelect(READY_ITEM_TYPE type)
    {
        if (dicItemSelect.ContainsKey(type) == false)
            return false;
        else
            return dicItemSelect[type] != READY_ITEM_STATE.NONE;
    }

    public override void OnClickBtnBack()
    {
        OnClickBtnReadyClose();
    }

    private void OnClickBtnReadyClose()
    {
        if (bCanTouch == false)
            return;

        ManagerUI._instance.ClosePopUpUI();

        //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
        ManagerUI._instance.InitActionCurrency();

        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.UpdateUI();

        base.ClosePopUp(_mainTime, callback);
    }

    private void OnClickBtnPlay()
    {
        if (bCanTouch == false)
            return;
        SetIsCanTouchPopup(false);

        switch (btnStartState)
        {
            case ButtonState.CAN_PLAY:
                //스테이지 정보 로드 후, 게임 시작 처리
                CheckStartGame_TurnRelay();
                break;
            case ButtonState.WAIT:
                //스피드 업 광고
                ManagerUI._instance.OpenPopupTurnRelay_APTime(RefreshButton, () => SetIsCanTouchPopup(true));
                break;
            default:
                SetIsCanTouchPopup(true);
                break;
        }
    }

    #region 게임 시작처리 관련
    private void CheckStartGame_TurnRelay()
    {
        #region 이벤트 종료시간 체크하는 영역
        //플레이 가능한 기간이 아닐 경우, 안내 팝업 출력
        if (ManagerTurnRelay.GetEventState() != ManagerTurnRelay.EventState.NEED_CREATE_PROFILE &&
            ManagerTurnRelay.GetEventState() != ManagerTurnRelay.EventState.RUNNING)
        {
            //팝업 종료 후, 재 오픈되도록 설정
            _callbackClose = () => { ManagerTurnRelay.OpenTurnRelay(); };

            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_54"), false, OnClickBtnClose);
            popup.SortOrderSetting();
            SetIsCanTouchPopup(true);
            return;
        }
        #endregion

        long startTime = Global.GetTime();

        //스테이지 로드 후 인게임 진입
        StartCoroutine(ManagerUI._instance.CoLoadStageData_TurnRelayEvent(() => StartGame_TurnRelay()));
    }

    private void StartGame_TurnRelay()
    {
        #region 레디 아이템 사용 여부 저장
        //선택한 레디 아이템 상태 저장
        for (int i = 0; i < 8; i++)
        {
            UIPopupReady.readyItemUseCount[i] = new EncValue();
            UIPopupReady.readyItemUseCount[i].Value = IsSelectReadyItem_ByUserSelect((READY_ITEM_TYPE)i) ? 1 : 0;
        }

        var useItems = new int[] {
            UIPopupReady.readyItemUseCount[0].Value,
            UIPopupReady.readyItemUseCount[1].Value,
            UIPopupReady.readyItemUseCount[2].Value,
            UIPopupReady.readyItemUseCount[3].Value,
            UIPopupReady.readyItemUseCount[4].Value,
            UIPopupReady.readyItemUseCount[5].Value,
            0, //6
            0, //7
            0, //8
            0, //9
            0, //10
            0, //11
            0, //12
            0, //13
            0, //14
            UIPopupReady.readyItemUseCount[6].Value,
            UIPopupReady.readyItemUseCount[7].Value
        };

        //유저의 레디 아이템 소모 없이 사용 가능한 아이템 리스트 초기화
        for (int i = 0; i < 6; i++)
        {
            UIPopupReady.readyItemAutoUse[i] = new EncValue();
            UIPopupReady.readyItemAutoUse[i].Value = IsSelectReadyItem_ByAutoSelect((READY_ITEM_TYPE)i) ? 1 : 0;
        }
        
        getReadyItemCount = new int[]
        {
            ServerRepos.UserItem.ReadyItem(0),
            ServerRepos.UserItem.ReadyItem(1),
            ServerRepos.UserItem.ReadyItem(2),
            ServerRepos.UserItem.ReadyItem(3),
            ServerRepos.UserItem.ReadyItem(4),
            ServerRepos.UserItem.ReadyItem(5)
        };
        
        for (int i = 0; i < 8; i++)
        {
            if (UIPopupReady.readyItemUseCount[i].Value > 0)
            {
                if (i < 3)
                {
                    //사과
                    if (getReadyItemCount[i] < UIPopupReady.readyItemUseCount[i].Value)
                    {
                        if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.coin))
                        {
                            payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            freeCoin[i] = 0;
                        }
                        else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                        {
                            payCoin[i] = (int)(GameData.User.coin);
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.coin);
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                        }
                    }
                }
                else if (Global.GameInstance.CanUseDoubleReadyItem() == true && (i == 6))
                {
                    int doubleIndex = 0; //사과 더블 레디 아이템

                    if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] < (int)(GameData.User.coin))
                    {
                        payCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                        freeCoin[i] = 0;
                    }
                    else if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                    {
                        payCoin[i] = (int)(GameData.User.coin);
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] - (int)(GameData.User.coin);
                    }
                    else
                    {
                        payCoin[i] = 0;
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                    }
                }
                else
                {
                    if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i) == false && getReadyItemCount[i] < UIPopupReady.readyItemUseCount[i].Value)
                    {
                        if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.jewel))
                        {
                            payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            freeCoin[i] = 0;
                        }
                        else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.jewel) && (int)(GameData.User.jewel) > 0)
                        {
                            payCoin[i] = (int)(GameData.User.jewel);
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.jewel);
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                        }
                    }
                }
            }
        }

        beforeFCoin = (int)(GameData.User.fcoin);
        beforeCoin = (int)(GameData.User.coin);
        beforeFDia = (int)(GameData.User.fjewel);
        beforeDia = (int)(GameData.User.jewel);
        
        #endregion

        long startTime = Global.GetTime();

        //퀘스트 정보 갱신
        QuestGameData.SetUserData();

        //게임 시작 시간 갱신
        ServerRepos.GameStartTs = startTime;

        //게임 시작 처리
        var req = new GameStartReq()
        {
            ts = startTime,
            type = (int)Global.GameType,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            items = useItems,
        };
        Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
    }

    //가지고 있는 레디아이템 갯수
    protected int[] getReadyItemCount;
    
    protected int[] payCoin = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
    protected int[] freeCoin = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
    
    protected int beforeFCoin = 0;
    protected int beforeCoin = 0;
    protected int beforeFDia = 0;
    protected int beforeDia = 0;
    
    private void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //게임 시작 설정
            ManagerLobby._stageStart = true;

            #region 유저 UI 갱신

            //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
            ManagerUI._instance.InitActionCurrency();

            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();
            #endregion

            //퀘스트 갱신
            QuestGameData.SetUserData();

            //턴 릴레이 인게임 데이터 초기화
            ManagerTurnRelay.turnRelayIngame.InitIngameData_GameStart(dicItemSelect);

            //인게임 진입 처리
            ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
            ManagerSound._instance.StopBGM();
            SceneLoading.MakeSceneLoading("InGame");
            ManagerUI._instance.bTouchTopUI = true;
            
            for (int i = 0; i < 8; i++)
            {
                //RewardType 의 더블 아이템 인덱스 계산
                string rewardTypestring_ReadyItem = ((i <= 5)? ((RewardType)((int)RewardType.readyItem1 + i)).ToString() : ((RewardType)((int)RewardType.readyItem7 + (i - 6))).ToString());

                int itemIdx = listReadyItem.FindIndex(x => (int)x.itemType == i);
                
                if (UIPopupReady.readyItemUseCount[i].Value > 0)
                {
                    if (listReadyItem[itemIdx].itemType == READY_ITEM_TYPE.LOCK) continue;

                    if (i < 3 || i == 6 || i == 7)
                    {
                        // if (getReadyItemCount[i] < readyItemUseCount[i])
                        if (payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeCoin -= payCoin[i];
                            beforeFCoin -= freeCoin[i];

                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                                (
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                                -payCoin[i],
                                -freeCoin[i],
                                beforeCoin,     //(int)(GameData.User.coin),
                                beforeFCoin,    // (int)(GameData.User.fcoin),
                                "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                                );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            
                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                                (
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                    rewardTypestring_ReadyItem,
                                    "ReadyItem" + listReadyItem[itemIdx].itemType.ToString(),
                                    UIPopupReady.readyItemUseCount[i].Value,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                                    Global.GameInstance.GetGrowthyGameMode().ToString()
                                );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }

                    }
                    else
                    {
                        if (payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeDia -= payCoin[i];
                            beforeFDia -= freeCoin[i];


                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                            (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -payCoin[i],
                            -freeCoin[i],
                           beforeDia,
                            beforeFDia,
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                            );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                            (
                               ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                rewardTypestring_ReadyItem,
                                "ReadyItem" + listReadyItem[itemIdx].itemType.ToString(),
                                UIPopupReady.readyItemUseCount[i].Value,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                                Global.GameInstance.GetGrowthyGameMode().ToString()
                            );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }
                    }

                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[itemIdx].itemType.ToString(),
                            -UIPopupReady.readyItemUseCount[i].Value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
                else if (UIPopupReady.readyItemUseCount[i].Value == 0)
                {   // 무료로 사용된 케이스 체크
                    bool usedFree = false;
                    if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i))
                    {
                        usedFree = true;
                    }

                    if (usedFree)
                    {
                        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[itemIdx].itemType.ToString(),
                            0,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_FREE_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                        var doc = JsonConvert.SerializeObject(useReadyItem);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                    }
                }
            }
        }
    }

    private void onFailStart(GameStartReq req)
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        popupSystem.FunctionSetting(1, "RetryGameStart", gameObject);
        popupSystem.FunctionSetting(3, "RetryCancel", gameObject);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_17"), false, null);
        popupSystem.SetResourceImage("Message/error");
        popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
        retryReq = req;
    }

    GameStartReq retryReq = null;
    private void RetryGameStart()
    {
        if (retryReq == null)
        {
            RetryCancel();
            return;
        }

        Global.GameInstance.GameStart(retryReq, recvGameStart, onFailStart);
    }

    private void RetryCancel()
    {
        Global.ReBoot();
    }
    #endregion

    #region 기타 버튼 처리
    private void OnClickBtnCooperation()
    {
        if (bCanTouch == false)
            return;
        SetIsCanTouchPopup(false);

        //이벤트 진행 상태에 따라 안내팝업/협동미션 팝업 출력
        if (ServerRepos.UserTurnRelayEvent == null || ServerRepos.UserTurnRelayEvent.rankEntryTime == 0)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>
            (
                (popup) =>
                {
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_10"), false);
                },
                endCallback: () => { SetIsCanTouchPopup(true); }
            );
            return;
        }
        else
        {
            ManagerUI._instance.OpenPopupTurnRelay_Cooperation(GetRewardAction_Cooperation, () => SetIsCanTouchPopup(true));
        }
    }

    private void OnClickBtnMission()
    {
        if (bCanTouch == false)
            return;
        SetIsCanTouchPopup(false);

        ManagerUI._instance.OpenPopupTurnRelay_SubMission(GetRewardAction_SubMission, () => SetIsCanTouchPopup(true));
    }

    private void OnClickBtnCapsuleToy()
    {
        if (bCanTouch == false)
            return;
        SetIsCanTouchPopup(false);

        ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>(endCallback : () =>
        {
            //토큰 카운트 갱신
            RefreshTokenCount();

            //터치 활성화
            SetIsCanTouchPopup(true);
        });
    }
    #endregion

    #region 콜백
    private void GetRewardAction_Cooperation()
    {
        StartCoroutine(CoInitAlarm_Cooperation());
        RefreshRewardUI();
    }

    private void GetRewardAction_SubMission()
    {
        InitAlarm_SubMission();
        RefreshRewardUI();
    }
    #endregion

    #region 팝업 터치 관련
    private void SetIsCanTouchPopup(bool isCanTouch)
    {
        ManagerUI._instance.bTouchTopUI = isCanTouch;
        this.bCanTouch = isCanTouch;
    }
    #endregion
}
