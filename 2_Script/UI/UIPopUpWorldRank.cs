using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpWorldRank : UIPopupBase
{
    public static UIPopUpWorldRank _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    //LAN 페이지 설정
    [SerializeField] private UIItemLanpageButton lanpageButton;

    private int selectPage = 0;
    private List<ManagerWorldRanking.WorldRankData> rankingDatas = new List<ManagerWorldRanking.WorldRankData>();

    [SerializeField] private UILabel loadingText;
    [SerializeField] private UILabel[] startButtonLabels;

    [SerializeField] private UITexture titleTexture;
    [SerializeField] private GameObject objFinalLine;
    [SerializeField] private GameObject myRankButton;
    [SerializeField] private GameObject objHallofFame;
    [SerializeField] private GameObject objCooperation;
    [SerializeField] private GameObject objCooperationAlarm;

    [SerializeField] private UIPanel                  scrollView;
    [SerializeField] private UIReuseGrid_WorldRanking uIReuseGrid;

    [SerializeField] private AnimationCurve rankupAnimationCurve;

    [SerializeField] private GameObject rewardButton;
    [SerializeField] private UIProgressBar stageRewardProgress;
    [SerializeField] private UILabel labelProgress;
    [SerializeField] private TweenRotation rewardTweenRotation;
    [SerializeField] private TweenScale rewardTweenScale;
    [SerializeField] private GameObject objStageRewardAlarm;
    [SerializeField] private GameObject objAdBubble;

    private bool isRankupCheck = false;
    private static int lastMyScore = 0;
    private static int lastMyStage = 0;
    private bool IsRewardActive
    {
        get
        {
            return (ServerContents.WorldRankBonus != null && ServerContents.WorldRank != null &&
                ServerContents.WorldRankBonus.eventIndex == ServerContents.WorldRank.eventIndex );
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (ManagerWorldRanking.contentsData.PreviousEventRankingFactor == null)
            objHallofFame.SetActive(false);

        rewardButton.SetActive(IsRewardActive);
    }

    private IEnumerator Start()
    {
        SettingDepth();
        
        var eventState = ManagerWorldRanking.GetEventState();

        if( eventState != ManagerWorldRanking.EventState.RUNNING)
        {
            string buttonText = Global._instance.GetString("btn_1");
            startButtonLabels.SetText(buttonText);
        }

        if( !ManagerWorldRanking.userData.IsParticipated())
        {
            objCooperation.SetActive(false);
        }

        if (IsRewardActive)
        {
            SetStageRewardProgressBar();
            
            yield return CoSetStageRewardCount();
        }

        yield return LoadMyRankingPage(isRankupCheck);
        
        switch (eventState)
        {
            case ManagerWorldRanking.EventState.RUNNING:
                {
                    objFinalLine.SetActive(false);

                    if (isRankupCheck)
                    {
                        yield return Rankup();
                    }

                    //현재의 상태가 끝나면 실행되는 다음 상태.
                    yield return ChangePopupSetting(ServerContents.WorldRank.deadlineTs, "n_wrk_5", false, () => allPopupClose());
                }
                break;
            case ManagerWorldRanking.EventState.REWARD:
                {
                    objFinalLine.SetActive(true);
                    //보상
                    yield return RewardProc();

                    //현재의 상태가 끝나면 실행되는 다음 상태.
                    yield return ChangePopupSetting(ServerContents.WorldRank.endTs, "n_ev_13", true, () => allPopupClose());
                }
                break;
        }
    }

    private IEnumerator ChangePopupSetting(long worldRankTs, string systemText, bool finalLineState = false, Method.FunctionVoid endCallBack = null)
    {
        long RemainingTime = Global.LeftTime(worldRankTs);

        yield return new WaitForSeconds(RemainingTime);

        objFinalLine.SetActive(finalLineState);

        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(systemText), false, endCallBack);
    }

    private IEnumerator Rankup()
    {
        if (lastMyScore >= ManagerWorldRanking.userData.Score)
        {
            uIReuseGrid.HideScroll(false);
            yield break;
        }

        yield return new WaitUntil(() => uIReuseGrid.IsReady);
        yield return new WaitForSeconds(0.5f);

        var myRankItem = uIReuseGrid.GetItem(ManagerWorldRanking.userData.myUserKey);

        float endTime = rankupAnimationCurve.keys[rankupAnimationCurve.keys.Length - 1].time;
        float totalTime = 0.0f;
        Vector3 endPos = myRankItem.localPosition;
        Vector3 startPos = endPos;
        startPos.y -= 750;

        var itemWidgets = myRankItem.GetComponentsInChildren<UIWidget>();

        uIReuseGrid.ActiveScroll(false);
        uIReuseGrid.HideScroll(false);

        foreach (var widget in itemWidgets) widget.depth += 100;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTime;

            myRankItem.localPosition = Vector3.LerpUnclamped(startPos, endPos, rankupAnimationCurve.Evaluate(totalTime));

            yield return null;
        }

        foreach (var widget in itemWidgets) widget.depth -= 100;

        uIReuseGrid.ActiveScroll(true);
    }
    
    private void SettingDepth()
    {
        int depth = uiPanel.depth;
        int layer = uiPanel.sortingOrder;

        scrollView.depth = depth + 1;

        if (layer < 10) return;

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder    = layer + 1;
    }
    
    private void allPopupClose()
    {
        int popupCnt = ManagerUI._instance._popupList.Count;
        for (int i = 0; i < popupCnt; i++)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    private bool isLoading = false;

    private void LoadingOn()
    {
        isLoading = true;
        loadingText.gameObject.SetActive(true);
        uIReuseGrid.gameObject.SetActive(false);
    }

    private void LoadingOff()
    {
        loadingText.gameObject.SetActive(false);
        uIReuseGrid.gameObject.SetActive(true);
        callbackDataComplete?.Invoke();

        isLoading = false;
    }

    public void InitPopup(bool rankupCheck = false)
    {
        isRankupCheck = rankupCheck;
        titleTexture.mainTexture = ManagerWorldRanking.resourceData.worldRankingPack.RankingTitle;

        if (ManagerWorldRanking.GetEventState() == ManagerWorldRanking.EventState.RUNNING &&/*경쟁중일 때 만 알림 작동*/
             ((isRankupCheck && lastMyScore < ManagerWorldRanking.userData.Score/*당근 수량 변경*/) ||
             IsDayPassWorldRankCheack()/*하루 중 월드랭킹 처음 입장*/))
        {
            objCooperationAlarm.SetActive(true);
        }
        
        SetAdButton(AdManager.ADCheck(AdManager.AdType.AD_19) && ManagerWorldRanking.GetEventOpenCondition() == ManagerWorldRanking.EventOpenCondition.MEET);
        InitData();
    }

    public void SetAdButton(bool isShowADButton)
    {
        objAdBubble.SetActive(isShowADButton);
    }

    IEnumerator CoSetStageRewardCount()
    {
        if (ManagerWorldRanking.userData.StageRewardGain() < 1)
        {
            objStageRewardAlarm.SetActive(false);
            rewardTweenRotation.enabled = false;
            rewardTweenRotation.transform.localRotation = Quaternion.Euler(Vector3.zero);

            yield return CoClearAction();
        }
        else
        {
            objStageRewardAlarm.SetActive(true);
            objStageRewardAlarm.GetComponentInChildren<UILabel>().text = ManagerWorldRanking.userData.StageRewardGain().ToString();

            yield return CoClearAction();

            rewardTweenRotation.enabled = true;
        }
    }

    IEnumerator CoClearAction()
    {
        if (isRankupCheck && lastMyStage < ManagerWorldRanking.userData.CurrentStage)
        {
            rewardTweenScale.enabled = true;

            //yield return new WaitWhile(() => rewardTweenScale.enabled == true);

            float time = 0 - rewardTweenScale.delay;
            System.Action soundAction = () => ManagerSound.AudioPlay(ResourceBox.Make(gameObject).LoadResource<AudioClip>("Sound/giftbox-item"));
            while (time < rewardTweenScale.duration)
            {
                time += Time.deltaTime;

                if(time >= rewardTweenScale.animationCurve.keys[4].time)
                {
                    soundAction?.Invoke();
                    soundAction = null;
                }

                yield return null;
            }
        }
    }

    bool IsDayPassWorldRankCheack()
    {
        if (PlayerPrefs.GetString("IsDayPassWorldRank") == "")
        {
            PlayerPrefs.SetString("IsDayPassWorldRank", Global.GetTime().ToString());
            return true;
        }

        if (Global.IsDayPassed(long.Parse(PlayerPrefs.GetString("IsDayPassWorldRank"))))
        {
            PlayerPrefs.SetString("IsDayPassWorldRank", Global.GetTime().ToString());
            return true;
        }
        else
        {
            return false;
        }
    }

    void InitData()
    {
        //LAN 페이지 설정
        lanpageButton.On("LGPKV_world_ranking", Global._instance.GetString("p_opt_11"));
    }

    public ManagerWorldRanking.WorldRankData GetWorldRankingData(int index)
    {
        if (rankingDatas == null) return null;

        if (rankingDatas.Count <= index || index < 0) return null;

        return rankingDatas[index];
    }

    public int GetWorldRankingDataCount()
    {
        return rankingDatas.Count;
    }

    IEnumerator CoLoadingText()
    {
        while (true)
        {
            if (loadingText.gameObject.activeInHierarchy == false)
                break;
            loadingText.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// 내가 속한 랭킹 페이지를 가져온다. 내 랭킹이 없는 경우 1위가 속한 페이지를 가져온다.
    /// </summary>
    /// <param name="isRankupCheck">랭크업 연출이 있는 경우 로딩이 완료되더라도 화면에 보이지 않도록 처리할 필요가 있다.</param>
    /// <returns></returns>
    private IEnumerator LoadMyRankingPage(bool isRankupCheck = false)
    {
        int page = 1;
        int rank = 1;
        bool isFindMyRank = false;

        if (isRankupCheck) uIReuseGrid.HideScroll(true);

#if UNITY_EDITOR
        rank = testMyRank;
        page = Mathf.CeilToInt((float)rank / ManagerWorldRanking.RANKING_PAGE_SIZE);
        isFindMyRank = true;
#else
        var myRank = new ManagerWorldRanking.MyRankQueryResult();
        yield return ManagerWorldRanking.QueryMyRanking(myRank);

        if (myRank.found)
        {
            rank = (int)myRank.rank;
            page = Mathf.CeilToInt((float)rank / ManagerWorldRanking.RANKING_PAGE_SIZE);
            isFindMyRank = true;
        }
#endif

        myRankButton.SetActive(isFindMyRank);

        System.Action setScroll = delegate 
        {
            int itemIndex = ((rank - 1) % ManagerWorldRanking.RANKING_PAGE_SIZE) + 1;
            uIReuseGrid.ScrollReset(itemIndex, 3);
        };

        yield return LoadRankingPage(page, setScroll);
    }

    private void SetStageRewardProgressBar()
    {
        stageRewardProgress.value = (float)ManagerWorldRanking.userData.StageClearCount % 10.0f * 0.1f;
        labelProgress.text = $"{(float)ManagerWorldRanking.userData.StageClearCount % 10.0f} / 10";
    }

    private IEnumerator LoadRankingPage(int page, System.Action changedCallback = null)
    {
#if UNITY_EDITOR
        if (page < 1) yield break;
        LoadingOn();
        yield return MakeTestRankingData(page);
        LoadingOff();

        changedCallback?.Invoke();
#else
        if (page < 1) yield break;
        LoadingOn();
        List<ManagerWorldRanking.WorldRankData> rankList = new List<ManagerWorldRanking.WorldRankData>();
        yield return ManagerWorldRanking.QueryRankingPage(page, rankList);
        LoadingOff();

        if (rankList != null && rankList.Count > 0)
        {
            selectPage = page;
            rankingDatas = rankList;
            changedCallback?.Invoke();
        }
#endif
    }

    void GetStageReward(Protocol.WorldRankBonusResp resp)
    {
        if (resp.IsSuccess)
        {
            ManagerUI._instance.OpenPopup<UIPopUpOpenGiftBox>((popup) =>
            {
                var data = new ServerUserGiftBox();
                List<Reward> rewardList = new List<Reward>();

                for (int i = 0; i < resp.worldRankBonus.Count; i++)
                {
                    Reward reward = new Reward();
                    reward.type = resp.worldRankBonus[i].type;
                    reward.value = resp.worldRankBonus[i].value;
                    rewardList.Add(reward);
                    popup.gradeRewardList.Add(resp.worldRankBonus[i].grade);
                }

                data.rewardList = rewardList;

                popup._data = data;
                popup._data.type = 5;
                popup.SetRankToken(new Reward() { type = (int)RewardType.rankToken, value = 1 });

                //정보 초기화
                ManagerWorldRanking.instance.InitData();
                StartCoroutine(CoSetStageRewardCount());
                SetStageRewardProgressBar();

                //UI업데이트.
                ManagerUI._instance.UpdateUI();

            });
        }
    }

    #region MakeFriend_EditorTest
    private const int testMyRank = 2;

    private const int maxRank = 5;

    private IEnumerator MakeTestRankingData(int page)
    {
        int startIndex = (page - 1) * ManagerWorldRanking.RANKING_PAGE_SIZE + 1;
        if (startIndex > maxRank) yield break;

        selectPage = page;
        rankingDatas.Clear();

        //친구정보 생성
        for (int i = 1; i <= ManagerWorldRanking.RANKING_PAGE_SIZE; i++)
        {
            int index = (page - 1) * ManagerWorldRanking.RANKING_PAGE_SIZE + i;

            if (index > maxRank) yield break;

            ManagerWorldRanking.WorldRankData fp = new ManagerWorldRanking.WorldRankData();
            fp.rank = index;
            fp.ingameName = "TestFriend" + index;
            fp.rankEventPoint = index * 10;
            fp.scoreValue = index * 100;
            fp.photoUseAgreed = true;
            fp.userKey = "TF" + index;

            if (i == 1)
            {
                UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
                fp.userKey = myProfile._userKey;
            }
                
            //게임을 하는 친구
            rankingDatas.Add(fp);

            yield return null;
        }
    }
#endregion

    //버튼 함수들
#region ButtonFunction

    void OnClickOpenPopUpStageReward()
    {
        if (ManagerWorldRanking.userData.StageRewardGain() > 0)/* 보상을 받을 수 있는지에 대한 bool값 */
        {
            ServerAPI.WorldRankBonus(GetStageReward);
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupWorldRankStageReward>((popup) => popup.InitData());
        }
    }


    void OnClickOpenPopUpAchievementBtn()
    {
        ManagerUI._instance.OpenPopup<UIPopUpWorldRankHallOfFame>().InitPopup(false);
    }

    void OnClickOpenPoopUpRewardBtn()
    {
        ManagerUI._instance.OpenPopup<UIPopUpWorldRankReward>();
    }
    void OnClickOpenPopUpCooperationBtn()
    {
        ManagerUI._instance.OpenPopup<UIPopUpWorldRankCooperation>();
        objCooperationAlarm.SetActive(false);
    }
    void OnClickOpenPopUpStartWorldRankingBtn()
    {
        if (ManagerWorldRanking.GetEventState() != ManagerWorldRanking.EventState.RUNNING)
        {
            ManagerUI._instance.ClosePopUpUI();
            return;
        }

        Debug.Log("시작");
        lastMyScore = ManagerWorldRanking.userData.Score;
        lastMyStage = ManagerWorldRanking.userData.CurrentStage;
        Global.SetGameType_WorldRanking(ServerContents.WorldRank.eventIndex, ServerRepos.UserWorldRank.stage);

        ManagerUI._instance.OpenPopupReadyStageCallBack();
    }
    void OnClickBackRankingBtn()
    {
        if (isLoading) return;

        StartCoroutine(LoadRankingPage(selectPage - 1, () => { uIReuseGrid.ScrollReset(); }));
    }
    void OnClickNextRankingBtn()
    {
        if (isLoading) return;

        StartCoroutine(LoadRankingPage(selectPage + 1, () => { uIReuseGrid.ScrollReset(); }));
    }
    void OnClickMyRankingBtn()
    {
        if (isLoading) return;

        StartCoroutine(LoadMyRankingPage());
    }

    void OnClickTopRankingBtn()
    {
        if (isLoading) return;
        
        StartCoroutine(LoadRankingPage(1, () => { uIReuseGrid.ScrollReset(); }));

    }
    private void OnClickOpenPopUpExchangeStationBtn()
    {
        ManagerUI._instance.OpenPopupWorldRankExchangeShop();
    }

#endregion

    private IEnumerator RewardProc()
    {
        if( !NetworkSettings.Instance.IsRealDevice() )
        {
            Debug.Log("RewardGet");
            yield break;
        }

        var eventState = ManagerWorldRanking.GetEventState();

        if( ServerRepos.UserWorldRank == null )
            yield break;

        var userWorldRank = ServerRepos.UserWorldRank;

        if (userWorldRank != null && 
            userWorldRank.stage > 1 && 
            userWorldRank.state < 2 && 
            eventState == ManagerWorldRanking.EventState.REWARD)
        {
            //보상 받는 함수가 종료되었는지 검사하기 위한 변수
            bool isGetReward = false;
            ServerAPI.WorldRankReward(ServerContents.WorldRank.eventIndex,
                (Protocol.WorldRankRewardResp resp) =>
                {
                    if (resp.IsSuccess)
                    {
                        //보상 팝업 띄우기
                        if (resp.clearReward != null)
                        {
                            ManagerUI._instance.OpenPopupGetRewardAlarm
                                (Global._instance.GetString("n_wrk_7"),
                                () => { isGetReward = true; },
                                resp.clearReward);
                        }
                        else
                        {
                            isGetReward = true;
                        }

                        if(resp.rankReward > 0) // 일반랭킹보상
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)RewardType.rankToken,
                                resp.rankReward,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_WORLDRANK_REWARD,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                                $"WORLDRANK_{ServerContents.WorldRank.eventIndex}_RANK_REWARD"
                            );
                        }

                        if(resp.coopReward > 0) // 협동과제보상
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)RewardType.rankToken,
                                resp.coopReward,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_WORLDRANK_REWARD,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                                $"WORLDRANK_{ServerContents.WorldRank.eventIndex}_COOP_REWARD"
                            );
                        }
                    }
                });

            //보상 받을 때 까지 대기
            yield return new WaitUntil(() => isGetReward == true);
        }
    }
}