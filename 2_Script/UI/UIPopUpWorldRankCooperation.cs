using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpWorldRankCooperation : UIPopupBase
{
    public static UIPopUpWorldRankCooperation _instance = null;
    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;


    [SerializeField] private UIPanel scrollView;

    //LAN 페이지 설정
    [SerializeField] private UIItemLanpageButton lanpageButton;

    [HideInInspector] public List<ManagerWorldRanking.WorldRankData> _listRankingDatas = new List<ManagerWorldRanking.WorldRankData>();

    
    [SerializeField] private UILabel loadingText;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private List<UISprite> stepButton;
    [SerializeField] private UISprite stepBG;
    [SerializeField] private GameObject stepRootObject;
    [SerializeField] private NGUIAtlas atlas;
    [SerializeField] private UILabel[] currentScore;
    [SerializeField] private UILabel[] rewardRankTokenCnt;
    [SerializeField] private UILabel rewardScore;
    [SerializeField] private UILabel scoreItem;
    [SerializeField] private UILabel[] labelWorldRankCooperation;

    private int currentStep = 0;

    [Header("Texture")]
    [SerializeField] private UITexture titleTexture;
    [SerializeField] private UITexture progressBoxTexture;
    [SerializeField] private UITexture progressBarTexture;

    [Header("ScoreIcon")]
    [SerializeField] private GameObject spineIconRoot;
    [SerializeField] private UIPanel scoreRoot;
    private GameObject spineIcon;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        titleTexture.mainTexture = ManagerWorldRanking.resourceData.worldRankingPack.CooperationTitle;
        progressBoxTexture.mainTexture = ManagerWorldRanking.resourceData.worldRankingPack.ProgressBox;
        progressBarTexture.mainTexture = ManagerWorldRanking.resourceData.worldRankingPack.ProgressBar;

        spineIcon = Instantiate(ManagerWorldRanking.resourceData.worldRankingPack.TopGaugesSpineObj, spineIconRoot.transform);
        spineIcon.transform.localScale = Vector3.one * 100;

        //협동미션 진행도 버튼을 가변적으로 처리
        for(int i = 0; i< ManagerWorldRanking.contentsData.coopReward.Length; i++)
        {
            stepButton.Add(NGUITools.AddSprite(stepRootObject, atlas as INGUIAtlas, "button_off", 18));
            stepButton[i].MakePixelPerfect();
            stepBG.width += (int)stepRootObject.GetComponent<UIGrid>().cellWidth;
        }
        stepRootObject.GetComponent<UIGrid>().Reposition();

        //리소스 아이디에 따라 수집품 이름 변경.
        scoreItem.text = Global._instance.GetString("p_sr_23").Replace("[1]", Global._instance.GetString($"wrk_col_{ManagerWorldRanking.contentsData.ResourceIndex}"));
    }
    private IEnumerator Start()
    {
        SetTitleStateLabel();

        List<ManagerWorldRanking.WorldRankData> worldRankList = new List<ManagerWorldRanking.WorldRankData>();

        yield return ManagerWorldRanking.QueryGroupRanking(worldRankList);

#if UNITY_EDITOR
        yield return MakeFriend();
#else
        for (int i = 0; i < worldRankList.Count; ++i)
        {
            _listRankingDatas.Add(worldRankList[i]);
        }
#endif

        OnDataComplete();
        //스탭 
        SetCooperationMissionStep(GetTotalScore());

        var currentCoopReward = GetCoopReward();
        var nextCoopReward = GetCoopReward(1);

        for (int i = 0; i < 2; i++)
        {
            currentScore[i].text = StringHelper.IntToString(GetTotalScore());
            rewardRankTokenCnt[i].text = string.Format("x{0}", nextCoopReward.rewardBase.ToString());
        }

        rewardScore.text = StringHelper.IntToString(nextCoopReward.targetCount);

        if (GetTotalScore() < nextCoopReward.targetCount)
            progressBar.value = (float)(GetTotalScore() - currentCoopReward.targetCount) / (float)(nextCoopReward.targetCount - currentCoopReward.targetCount);
        else
            progressBar.value = 1;

#if UNITY_EDITOR
        //에디터 테스트용
        for (int i = 1; i < worldRankList.Count + 1; ++i)
        {
            _listRankingDatas[i].rank = i;
        }
#endif
    }

    public CoopReward GetCoopReward(int addStep = 0)
    {
        int step = currentStep + addStep;

        if (step < 0)
            return new CoopReward() { rewardBase = 0, rewardDelta = 0, targetCount = 0 };

        return ManagerWorldRanking.contentsData.GetCurrentCoopReward(step);
    }

    public int GetCoopUserReward(int rank)
    {
        CoopReward coopReward = GetCoopReward();

        return (ManagerWorldRanking.contentsData.coopGroupSize + 1 - rank) * coopReward.rewardDelta + coopReward.rewardBase;
    }

    void SetTitleStateLabel()
    {
        var eventState = ManagerWorldRanking.GetEventState();
        switch (eventState)
        {
            case ManagerWorldRanking.EventState.RUNNING:
                {
                    labelWorldRankCooperation[0].text = Global._instance.GetString("p_wrk_m_1");
                    labelWorldRankCooperation[1].text = Global._instance.GetString("p_wrk_m_1");
                }
                break;
            default:
                {
                    labelWorldRankCooperation[0].text = Global._instance.GetString("p_wrk_m_9");
                    labelWorldRankCooperation[1].text = Global._instance.GetString("p_wrk_m_9");
                }
                break;
        }
    }

    private void OnDataComplete()
    {
        Debug.Log($"2. {_listRankingDatas.Count}");
        if (_listRankingDatas.Count == 0) return;

        callbackDataComplete?.Invoke();
        loadingText.gameObject.SetActive(false);
    }
    

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        scrollView.depth = depth + 1;

        //LAN 페이지 설정
        lanpageButton.On("LGPKV_world_mission", Global._instance.GetString("p_wrk_m_8"));
        CoLoadingText();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }

        spineIcon.GetComponent<MeshRenderer>().sortingOrder = layer;
        scoreRoot.useSortingOrder = true;
        scoreRoot.sortingOrder = layer + 1;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private int GetTotalScore()
    {
        long totalscore = 0;

        for(int i = 0; i < _listRankingDatas.Count; i++)
        {
            totalscore += _listRankingDatas[i].scoreValue;
        }

        return (int)totalscore;
    }

    private void SetCooperationMissionStep(int scoreTotal)
    {
        currentStep = ManagerWorldRanking.contentsData.GetCoopRewardLevel(scoreTotal);

        for (int i = 0; i <= currentStep; i++)
        {
            stepButton[i].spriteName = "button_on";
        }
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

    public ManagerWorldRanking.WorldRankData GetWorldRankingData(int index)
    {
        if (_listRankingDatas.Count <= index || _listRankingDatas[index] == null)
            return null;

        return _listRankingDatas[index];
    }

    #region MakeFriend_EditorTest
    IEnumerator MakeFriend()
    {
        _listRankingDatas.Clear();
        {
            //친구정보 생성
            for (int i = 1; i < 31; i++)
            {
                ManagerWorldRanking.WorldRankData fp = new ManagerWorldRanking.WorldRankData();

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
}
