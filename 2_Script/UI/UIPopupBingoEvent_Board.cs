using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServiceSDK;
using Newtonsoft.Json;

public class UIPopupBingoEvent_Board : UIPopupBase
{
    public static UIPopupBingoEvent_Board _instance = null;

    [SerializeField] private GenericReward ressetReward;
    [SerializeField] private GameObject objRessetBlock;
    
    [SerializeField] private GameObject objSaleIcon;
    [SerializeField] private List<GenericReward> lineRewards;
    [SerializeField] private UIGrid grieLineReward;
    [SerializeField] public GameObject sprBlockRewardGetButton;
    [SerializeField] private GameObject objNotReward;
    [SerializeField] private UILabel labelCurrentBingoCount;
    
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private List<GameObject> progressBarLine;

    [SerializeField] public List<UIItemBingoEventBoard> listBingoBoardItems;
    [SerializeField] public GameObject grid;

    [SerializeField] private UIItemLanpageButton Lanpage;
    
    //빙고 연출 이펙트
    [SerializeField] private GameObject complete_effect;
    [SerializeField] private GameObject complete_confetti_effect;
    [SerializeField] private List<UISprite> bingoPopupSpriteList;
    
    [HideInInspector]
    //빙고판 연출이 끝났는지 확인하기 위한 변수
    public bool bCanBingoTouch = true;
    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }

    public void InitData()
    {   
        Global.SetGameType_BingoEvent(1, ManagerBingoEvent.instance.GetBingoEvent_EventIndex());
        
        SetAtlas();
        
        //리셋 재화 세팅
        SetResetButton();

        //보상 세팅
        SetLineReward();
        
        //프로그래스바 세팅
        SetProgressBar();

        //랜 페이지
        Lanpage.gameObject.SetActive(true);
        Lanpage.On("LGPKV_bingo_event", Global._instance.GetString("p_bge_2"));

        //연출
        StartCoroutine((CoInitData()));
    }

    private IEnumerator CoInitData()
    {
        ManagerUI._instance.bTouchTopUI = false;
        _instance.bCanBingoTouch = false;
        
        //빙고 클리어 연출
        bool isActionEnd_BingoClear = true;
        if (ManagerBingoEvent.instance.isBingoClear)
        {
            isActionEnd_BingoClear = false;
            StartCoroutine(CoPlayBingoComplete(() => isActionEnd_BingoClear = true));
        }

        //빙고판 튜토리얼 및 빙고판 초기화
        bool isActionEnd_BingoBoard = false;
        if (ManagerBingoEvent.instance.CheckTutorial(ManagerBingoEvent.tutorialOpenKey))
        {
            StartCoroutine(CoInitBingoBoard(()=> isActionEnd_BingoBoard = true));
            ManagerTutorial.PlayTutorial(TutorialType.TutorialBingoEvent_EventOpen);
        }
        else
        {
            StartCoroutine(CoInitBingoBoard(()=> isActionEnd_BingoBoard = true));
            LineTutorial();
        }

        //연출 끝날때까지 대기
        yield return new WaitUntil(() => isActionEnd_BingoClear == true && isActionEnd_BingoBoard == true);
        ManagerUI._instance.bTouchTopUI = true;
        _instance.bCanBingoTouch = true;
    }

    void LineTutorial()
    {
        if(ManagerBingoEvent.instance.IsLineComplete() == false) return;

        UIItemBingoEventBoard.LineClearEvent +=
            () =>
            {
                if (ManagerBingoEvent.instance.CheckTutorial(ManagerBingoEvent.tutorialLineCompleteKey))
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialBingoEvent_FirstLineComplete);
                }

                UIItemBingoEventBoard.LineClearEvent = null;
            };
    }

    private IEnumerator CoInitBingoBoard(System.Action endAction)
    {
        int actionEndCount = 0;
        for (int i = 0; i < listBingoBoardItems.Count; i++)
        {
            listBingoBoardItems[i].InitDate(i);
            StartCoroutine(listBingoBoardItems[i].CoSetSlot(() => actionEndCount++));
        }

        //연출 종료까지 대기
        yield return new WaitUntil(() => actionEndCount >= listBingoBoardItems.Count);
        endAction?.Invoke();
    }

    void SetResetButton()
    {
        ressetReward.SetReward(ManagerBingoEvent.instance.GetBingoBoardResetCostFromManager());
        objRessetBlock.SetActive(ManagerBingoEvent.instance.IsRessetButton());
        objSaleIcon.SetActive(ManagerBingoEvent.instance.SaleState != 0);
    }
     void SetProgressBar()
    {
        progressBar.value = (float)ManagerBingoEvent.instance.LineCompleteProgress / 12;
        labelCurrentBingoCount.text = $"{ManagerBingoEvent.instance.LineCompleteProgress}";

        for (int i = 0; i < progressBarLine.Count; i++)
        {
            progressBarLine[i].transform.localPosition =
                new Vector3(500 / 12 * (ManagerBingoEvent.instance.GetLineDecoRewards()[i] + 1), 0f, 0f);
        }
    }

     public void SetBonusStageClearUpdate()
     {
         SetProgressBar();
         SetLineReward();
         SetResetButton();
     }

    void SetLineReward()
    {
        for (int i = 0; i < lineRewards.Count; i++)
        {
            lineRewards[i].gameObject.SetActive(false);   
        }

        objNotReward.SetActive(false);
        
        if (ManagerBingoEvent.instance.LineReward.Count == ManagerBingoEvent.instance.GetCurrentLineRewardIndex())
        {
            objNotReward.SetActive(true);
            sprBlockRewardGetButton.SetActive(true);
            return;
        }

        var currentLineRewards = ManagerBingoEvent.instance.LineReward[ManagerBingoEvent.instance.GetCurrentLineRewardIndex()];

        for (int i = 0; i < currentLineRewards.Count; i++)
        {
            this.lineRewards[i].gameObject.SetActive(true);
            this.lineRewards[i].SetReward(currentLineRewards[i]);
        }
        
        if(ManagerBingoEvent.instance.CheckGetLineReward())
            sprBlockRewardGetButton.SetActive(false);
        else
            sprBlockRewardGetButton.SetActive(true);
        
        grieLineReward.Reposition();
    }

    //빙고 클리어 시 나오는 폭죽 이펙트 위치 저장용 리스트
    [SerializeField] private List<Vector3> listBingoComplete = new List<Vector3>();
    
    //빙고 클리어 연출
    IEnumerator CoPlayBingoComplete(System.Action completeAction)
    {
        yield return new WaitForSeconds(0.5f);
        
        var obj = NGUITools.AddChild(mainSprite.gameObject, complete_confetti_effect);
        obj.transform.localPosition = new Vector3(0f, 300f, 0f);
        obj.transform.localScale = new Vector3(0.17f, 0.17f, 0.17f);

        for (int i = 0; i < 4; i++)
        {
            PlayBingoComplete(i);
            yield return new WaitForSeconds(0.3f);
        }

        ManagerBingoEvent.instance.isBingoClear = false;
        completeAction?.Invoke();
    }

    void PlayBingoComplete(int index)
    {
        var obj = NGUITools.AddChild(mainSprite.gameObject, complete_effect);
        obj.transform.localPosition = listBingoComplete[index];
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    /// <summary>
    /// 보드가 갱신되는 타이밍에 호출되는 함수
    /// </summary>
    public void SetBoard(bool isBonus = true)
    {
        if (isBonus)
            LineTutorial();
        
        //빙고 완료 보상 연출 및 빙고 슬롯 오픈 연출
        StartCoroutine((CoActionSetBoard()));
    }

    /// <summary>
    /// 보드 설정 연출
    /// </summary>
    private IEnumerator CoActionSetBoard()
    {
        ManagerUI._instance.bTouchTopUI = false;
        _instance.bCanBingoTouch = false;

        bool isActionEnd_CompleteBingo = true;
        if (ManagerBingoEvent.instance.isBingoClear)
        {
            isActionEnd_CompleteBingo = false;
            StartCoroutine(CoPlayBingoComplete(() => isActionEnd_CompleteBingo = true));
        }

        int endActionSlotCount = 0;
        for (int i = 0; i < listBingoBoardItems.Count; i++)
        {
            StartCoroutine(listBingoBoardItems[i].CoSetSlot(() => endActionSlotCount++));
        }

        // 연출이 완료될 떄까지 대기
        yield return new WaitUntil(() => isActionEnd_CompleteBingo == true && endActionSlotCount >= listBingoBoardItems.Count);
        ManagerUI._instance.bTouchTopUI = true;
        _instance.bCanBingoTouch = true;
    }

    private void SetAtlas()
    {
        for (int i = 0; i < bingoPopupSpriteList.Count; i++)
        {
            bingoPopupSpriteList[i].atlas = ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasOutgame;
        }

        for (int i = 0; i < listBingoBoardItems.Count; i++)
        {
            listBingoBoardItems[i].SetAtlas();
        }
    }

    public bool IsCanPopupTouch()
    {
        return (_instance.bCanTouch == true &&
                _instance.bCanBingoTouch == true);
    }

    public override void OnClickBtnBack()
    {
        if (IsCanPopupTouch() == false) return;
        
        base.OnClickBtnBack();
    }

    protected override void OnClickBtnClose()
    {
        if (IsCanPopupTouch() == false) return;
        
        base.OnClickBtnClose();
    }

    void OnClickResetButton()
    {
        if (IsCanPopupTouch() == false) return;
        if (ManagerBingoEvent.instance.IsRessetButton())
            return;
            
        int price = ManagerBingoEvent.instance.GetBingoBoardResetCost().value;

        if (Global.coin < price)
        {
            ManagerUI._instance.LackCoinsPopUp();
            return;
        }
        
        ServerAPI.BingoEventSlotReset((resp) =>
        {
            if (resp.IsSuccess)
            {
                //그로시
                {
                    int coin = ManagerBingoEvent.instance.GetBingoBoardResetCostFromManager().value;
                        
                    int usePCoin = 0;
                    int useFCoin = 0;
                    
                    if ((int)GameData.User.coin > coin) usePCoin = coin;
                    else if ((int)GameData.User.coin > 0)
                    {
                        usePCoin = (int)GameData.User.coin;
                        useFCoin = coin - (int)GameData.User.coin;
                    }
                    else useFCoin = coin;
                    
                    var growthyMoney = new GrowthyCustomLog_Money
                    (
                        GrowthyCustomLog_Money.Code_L_TAG.SC,
                        GrowthyCustomLog_Money.Code_L_MRSN.U_BINGO_RESET,
                        -usePCoin,
                        -useFCoin,
                        (int)(ServerRepos.User.coin),
                        (int)(ServerRepos.User.fcoin),
                        "BINGO"
                    );
                    var docMoney = JsonConvert.SerializeObject(growthyMoney);
                    ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                }
                
                ManagerBingoEvent.instance.SyncFromServerUserData();

                ManagerBingoEvent.instance.isSlotOpen = true;
                
                SetBoard(false);
                
                if (ManagerUI._instance != null)
                    ManagerUI._instance.SyncTopUIAssets();
            }
        });
    }

    void OnClickRewardList()
    {
        if (IsCanPopupTouch() == false) return;
        
        ManagerUI._instance.OpenPopup<UIPopupBingoEvent_Reward>((popup) => popup.InitData()); 
    }

    void OnClickGetReward()
    {
        if (IsCanPopupTouch() == false) return;
        
        if (ManagerBingoEvent.instance.CheckGetLineReward())
        {
            int currentLineRewardIndex = ManagerBingoEvent.instance.GetCurrentLineRewardIndex();
            ServerAPI.BingoEventGetReward(currentLineRewardIndex, (resp) =>
            {
                if (resp.IsSuccess)
                {
                    //그로시
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.BINGO_REWARD,
                            $"BINGO_LINE_REWARD_{(currentLineRewardIndex + 1)}",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
                    
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);


                        if(resp.lineReward.directApplied != null)
                        {
                            foreach (var reward in resp.lineReward.directApplied)
                            {
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                                (
                                    rewardType: reward.Value.type,
                                    rewardCount: reward.Value.valueDelta,
                                    moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BINGO_LINE_REWARD,
                                    itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BINGO_LINE_REWARD,
                                    QuestName: null
                                );
                            }
                        }

                        if (resp.lineReward.mailReceived != null)
                        {
                            foreach (var reward in resp.lineReward.mailReceived)
                            {
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                                (
                                    rewardType: reward.type,
                                    rewardCount: reward.value,
                                    moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BINGO_LINE_REWARD,
                                    itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BINGO_LINE_REWARD,
                                    QuestName: null
                                );
                            }
                        }
                        
                    }
                    
                    ManagerBingoEvent.instance.SyncFromServerUserData();
                    
                    ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.lineReward);
                    
                    if (ManagerUI._instance != null)
                        ManagerUI._instance.SyncTopUIAssets();

                    //다음 보상 세팅
                    SetLineReward();

                    //강제 노출 이벤트 리워드 갱신
                    if (ManagerForceDisplayEvent.instance != null)
                    {
                        var rewardState = resp.userBingoEvent.lineRewardState.FindIndex(x => x == 0);
                        if (rewardState == -1)
                        {
                            ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.BINGO_EVENT);
                        }
                    }
                }
            });   
        }
    }
}
