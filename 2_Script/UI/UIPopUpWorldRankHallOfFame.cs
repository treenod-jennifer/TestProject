using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpWorldRankHallOfFame : UIPopupBase
{
    public static UIPopUpWorldRankHallOfFame _instance = null;
    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    [SerializeField] private UIPanel scrollView;

    [HideInInspector] public List<ManagerWorldRanking.WorldRankData> _listRankingDatas = new List<ManagerWorldRanking.WorldRankData>();

    [SerializeField] private UILabel loadingText;
    [SerializeField] private UIReuseGrid_WorldRankHallOfFame uIReuseGrid;
    [SerializeField] private GameObject[] btnArrow;
    [SerializeField] private UILabel labelShowSeason;
    [SerializeField] private GameObject objRankShop;
    [SerializeField] private GameObject objAdBubble;
    
    private int TestEventIndex = 3;
    private int currentRankEventIndex = 0;
    public int CurrentResourceID { get; private set; }
    private int CurrentRankEventIndex
    {
        get
        {
            return currentRankEventIndex;
        }
        set
        {
            if(currentRankEventIndex != value)
            {
                currentRankEventIndex = value;
                var rank = ManagerWorldRanking.contentsData.AllEventRankingFactors.Find((rankingFactor) => rankingFactor.eventIndex == currentRankEventIndex);
                CurrentResourceID = rank.resourceId;
            }
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private IEnumerator Start()
    {
#if UNITY_EDITOR
        CurrentRankEventIndex = TestEventIndex - 1;
#else
        CurrentRankEventIndex = ManagerWorldRanking.contentsData.PreviousEventRankingFactor.eventIndex;
#endif
        yield return LoadRankingPage(CurrentRankEventIndex, () => uIReuseGrid.ScrollReset() );
    }

    void SetShowSeasonLabel(int rankSeason)
    {
        labelShowSeason.text = Global._instance.GetString("p_wrk_li_2").Replace("[n]", rankSeason.ToString());
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        scrollView.depth = depth + 1;
    }

    public ManagerWorldRanking.WorldRankData GetWorldRankingData(int index)
    {
        if (_listRankingDatas == null) return null;

        if (_listRankingDatas.Count <= index || _listRankingDatas[index] == null)
            return null;

        return _listRankingDatas[index];
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

    void OnClickNextRankingBtn()
    {
        if (isLoading) return;

        StartCoroutine(LoadRankingPage(CurrentRankEventIndex + 1, () => { uIReuseGrid.ScrollReset(); }));
    }
    void OnClickBackRankingBtn()
    {
        if (isLoading) return;

        StartCoroutine(LoadRankingPage(CurrentRankEventIndex - 1, () => { uIReuseGrid.ScrollReset(); }));
    }

    private IEnumerator LoadRankingPage(int page, System.Action changedCallback = null)
    {
        
#if UNITY_EDITOR
        if (page < 1 || page >= TestEventIndex) yield break;

        LoadingOn();
        Debug.Log(page);
        yield return MakeTestRankingData();
        _listRankingDatas = testRankDataList[page - 1];
        LoadingOff();

        CurrentRankEventIndex = page;

        changedCallback?.Invoke();
#else
        if (page < 1 || page > ManagerWorldRanking.contentsData.PreviousEventRankingFactor.eventIndex) yield break;

        LoadingOn();

        List<ManagerWorldRanking.WorldRankData> rankList = new List<ManagerWorldRanking.WorldRankData>();
        CdnWorldRankHeader eventWorldRankHeader = ManagerWorldRanking.contentsData.AllEventRankingFactors.Find(x => x.eventIndex == page);
        yield return ManagerWorldRanking.QueryHallOfFame(eventWorldRankHeader.tableIdRank, rankList);

        LoadingOff();


        if (rankList != null && rankList.Count > 0)
        {
            CurrentRankEventIndex = page;
            _listRankingDatas = rankList;
            changedCallback?.Invoke();
        }
#endif
        //시즌 텍스트 갱신
        SetShowSeasonLabel(page);
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

    public void InitPopup(bool isShopOn)
    {
        objRankShop.SetActive(isShopOn);
        
        //교환소 광고 버튼 처리
        if (isShopOn)
            SetAdButton(AdManager.ADCheck(AdManager.AdType.AD_19) && ManagerWorldRanking.GetEventOpenCondition() == ManagerWorldRanking.EventOpenCondition.MEET);
    }
    
    public void SetAdButton(bool isShowADButton)
    {
        objAdBubble.SetActive(isShowADButton);
    }

    private void OnClickOpenPopUpExchangeStationBtn()
    {
        ManagerUI._instance.OpenPopupWorldRankExchangeShop();
    }

#region MakeFriend_EditorTest

    List<List<ManagerWorldRanking.WorldRankData>> testRankDataList = new List<List<ManagerWorldRanking.WorldRankData>>();

    private IEnumerator MakeTestRankingData()
    {

        //친구정보 생성
        for (int j = 1; j <= TestEventIndex; j++)
        {
            List<ManagerWorldRanking.WorldRankData> testList = new List<ManagerWorldRanking.WorldRankData>();

            for (int i = 1; i <= ManagerWorldRanking.RANKING_PAGE_SIZE; i++)
            {
                ManagerWorldRanking.WorldRankData fp = new ManagerWorldRanking.WorldRankData();

                int index = i;

                fp.rank = index;
                fp.ingameName = $"TestF_{j}_{index} ";
                fp.rankEventPoint = index * 10;
                fp.scoreValue = index * 100;
                fp.photoUseAgreed = true;
                fp.userKey = "TF" + index;

                //게임을 하는 친구
                testList.Add(fp);

            }
            testRankDataList.Add(testList);

            yield return null;
        }
    }
#endregion
}
