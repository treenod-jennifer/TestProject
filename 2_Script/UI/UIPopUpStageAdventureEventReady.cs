using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpStageAdventureEventReady : UIPopupStageAdventureReady
{
    
    //상품판매 법률 개정 수정 오브젝트
    [SerializeField] private GameObject objAnimalRoot;
    [SerializeField] private UISprite sprAnimalFrame;
    [SerializeField] private UISprite sprPopupBack;
    [SerializeField] private GameObject objRecomandBtn;
    [SerializeField] private GameObject objProbabilityBtn;
    
    
    protected override void Awake()
    {
        base.Awake();
        InitData();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DestroySpineObject();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        ClickBlocker.Make(0.8f);
    }

    //상품판매 법률 개정 세팅
    private void SetDescription()
    {
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
        
        if (LanguageUtility.IsShowBuyInfo)
        {
            mainSprite.height = 1035;
            sprAnimalFrame.height = 505;
            sprPopupBack.height = 1000;
            
            objAnimalRoot.transform.localPosition = new Vector3(0f, 10f, 0f);
            objAnimalRoot.transform.localScale = new Vector3(0.95f, 0.95f, 0f);
            startBtnTr.transform.localPosition = new Vector3(0f, -955f, 0f);
            objRecomandBtn.transform.localPosition = new Vector3(265.8f, -925, 0f);
            objProbabilityBtn.transform.localPosition = new Vector3(347f, -980f, 0f);
            objDescription.transform.localPosition = new Vector3(0f, -1030f, 0f);
        }
    }
    
    private void InitData()
    {
        SetDescription();
        
        if (!PlayerPrefs.HasKey("FirstAdventureEvent"))
        {
            _callbackOpen += () => OnClickHelp();
            PlayerPrefs.SetInt("FirstAdventureEvent", 0);
        }

        if (UIPopupStageAdventure._instance != null)
        {
            UIPopupStageAdventure._instance.ClosePopUp();
            _callbackClose += () => ManagerUI._instance.OpenPopup<UIPopupStageAdventure>();
        }
        else
        {
            _callbackClose += () =>
            {
                ManagerSound._instance.StopBGM();
                Global.SetGameType_NormalGame();
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.IsTopUIAdventureMode = false;
            };
        }

        ManagerUI._instance.IsTopUIAdventureMode = true;

        MakeSpineObject();

        SettingFreeWingButton();

        ManagerSound._instance.StopBGM();

        Global.SetGameType_AdventureEvent
        (
            ManagerAdventure.EventData.GetAdvEventIndex(),
            ManagerAdventure.EventData.GetAdvEventStageCount()
        );

        ManagerSound._instance.PlayBGM();

        string mapName = ManagerAdventure.EventData.GetAdvEventMapFileName(Global.eventIndex, Global.stageIndex);

        NetworkLoading.MakeNetworkLoading(0.5f);

        StageUtility.StageMapDataLoad
        (
            mapName,
            (mapData) =>
            {
                NetworkLoading.EndNetworkLoading();

                if (mapData != null)
                {
                    this.stageData = mapData;

                    for (int i = 0; i < 3; i++)
                    {
                        var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
                        animals[i].SetAnimalSelect(animalData);
                        levelUpButtons[i].ResetButton();

                        topPanel.SetAnimal(i, animalData.idx);
                    }

                    topPanel.SetBoss(mapData.bossIdx);
                    topPanel.SetWeight();

                    var eventPanel = topPanel as UIItemAdventureEventPanel;
                    if (eventPanel == null)
                    {
                        return;
                    }

                    int selectIndex = ManagerAdventure.EventData.GetLastPlayStage();
                    selectIndex = selectIndex == -1 ? 1 : selectIndex;

                    eventPanel.InitEventPanel(ManagerAdventure.EventData.GetAdvEventStageCount(), selectIndex);
                    eventPanel.MoveEndEvent += InitBoxRewards;

                    InitTitle();
                    InitBonusAnimalList();
                    InitRewards();
                    InitBoxRewards(selectIndex);
                }
                else
                {
                    ErrorController.ShowNetworkErrorDialogAndRetry("", null);
                }
            }
        );
    }

    protected override void OnClickGoInGame()
    {
        if (bCanTouch == false)
            return;

        Global.SetGameType_AdventureEvent(Global.eventIndex, UIItemAdventureEventStep.selectStepIndex);
        StageUtility.StageMapDataLoad(
            ManagerAdventure.EventData.GetAdvEventMapFileName(Global.eventIndex, Global.stageIndex),
            (mapData) =>
            {
                if (mapData != null)
                {
                    if (GoIngameProduction())
                    {
                        var req = new GameStartReq()
                        {
                            type = (int)Global.GameType,
                            eventIdx = Global.eventIndex,
                            stage = Global.stageIndex
                        };
                        ServerRepos.GameStartTs = Global.GetTime();
                        ManagerAdventure.EventData.SetLastPlayStage();
                        QuestGameData.SetUserData();
                        Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
                    }
                }
                else
                {
                    ErrorController.ShowNetworkErrorDialogAndRetry("", null);
                }
            }
        );
    }

    private void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //게임 시작 설정
            ManagerLobby._stageStart = true;

            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            QuestGameData.SetUserData();

            if (ManagerCoinStashEvent.CheckStartable())
                ManagerCoinStashEvent.currentCoinMultiplierState = ServerRepos.UserCoinStash.multiplier;

            _recvGameStart_end = true;
            bool nowWingFreeTime = GameData.RemainFreeWingPlayTime() > 0;

            //그로씨
            var useClover = new ServiceSDK.GrowthyCustomLog_Money
                (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
                    nowWingFreeTime ? ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_ADVENTURE_PLAY_FREE : ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_ADVENTURE_PLAY,
                    0,
                    nowWingFreeTime ? 0 : -1,
                    0,
                    (int)(ServerRepos.User.AllWing),
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                );
            var cloverDoc = Newtonsoft.Json.JsonConvert.SerializeObject(useClover);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
        }
    }

    void onFailStart(GameStartReq req)
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

    void RetryCancel()
    {
        Global.ReBoot();
    }

    GameStartReq retryReq = null;

    void RetryGameStart()
    {
        if (retryReq == null)
        {
            RetryCancel();
            return;
        }

        Global.GameInstance.GameStart(retryReq, recvGameStart, onFailStart);
    }

    protected override void ChangeAnimal(int slot)
    {
        if (!bCanTouch)
            return;

        bCanTouch = false;

        selectSlot = slot;

        ManagerUI._instance.OpenPopup<UIPopupStageAdventureAnimalEvent>((popup) => { popup.InitTarget(UIPopupStageAdventureAnimal.PopupMode.ChangeMode); });
    }

    public override void OnClickRecommendDeck()
    {
        if (!bCanTouch)
            return;

        BossInfo bossInfo = StageUtility.StageInfoDecryption(stageData).bossInfo;
        StartCoroutine(ManagerAdventure.User.SetRecommendDeck(bossInfo.attribute, bossInfo.attrSize, true, RePaintAll));
    }

    public void OnClickHelp()
    {
        ManagerUI._instance.OpenPopup<UIPopupAdventureEventInfo>();
    }

    #region TopTitle

    [Header("Title")]
    [SerializeField] private UIUrlTexture title;
    [SerializeField] private TextMeshGlobalString titleText;

    private void InitTitle()
    {
        titleText.SetStringKey($"title_adv_{Global.eventIndex}");

        string fileName = ManagerAdventure.EventData.GetAdvEventTitleName(Global.eventIndex);

        Vector2 size = new Vector2(title.width, title.height);

        if (HashChecker.GetHash("IconEvent/", fileName + ".png") != null)
        {
            title.SuccessEvent += () => { title.width = (int)size.x; title.height = (int)size.y; };
            title.LoadCDN(Global.gameImageDirectory, "IconEvent/", fileName);
            title.gameObject.SetActive(true);
        }
    }

    #endregion

    #region BonusAnimalList Button

    [Header("BonusAnimalList Button")]
    [SerializeField] private UIUrlTexture mainTexture;
    [SerializeField] private UILabel bonusLabel;

    private void InitBonusAnimalList()
    {
        string fileName = ManagerAdventure.EventData.GetAdvEventBonusListIconName(Global.eventIndex);

        Vector2 size = new Vector2(mainTexture.width, mainTexture.height);

        if (HashChecker.GetHash("IconEvent/", fileName + ".png") != null)
        {
            mainTexture.SuccessEvent += () => { mainTexture.width = (int)size.x; mainTexture.height = (int)size.y; };
            mainTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", fileName);
            mainTexture.gameObject.SetActive(true);
        }

        bonusLabel.text = "+" + ManagerAdventure.EventData.GetAdvEventBonus() + "%";
    }

    public void ClickBonusAnimalList()
    {
        ManagerUI._instance.OpenPopup<UIPopupStageAdventureAnimalEvent>((popup) => { popup.InitTarget(UIPopupStageAdventureAnimal.PopupMode.ViewMode); });
    }

    #endregion

    #region Reward List

    [Header("Reward List")]
    [SerializeField] private Transform rewardTitle;
    [SerializeField] private UISprite rewardBox;
    [SerializeField] private GenericReward[] rewards;
    [SerializeField] private UIGrid rewardsGrid;

    private void InitRewards()
    {
        int rewardCount = 0;
        var rewardsData = ManagerAdventure.EventData.GetAdvEventRewards();

        for (int i = 0; i < rewards.Length; i++)
        {
            if(i < rewardsData.Count)
            {
                rewards[i].gameObject.SetActive(true);
                rewards[i].SetReward(rewardsData[i]);
                rewardCount++;
            }
            else
            {
                rewards[i].gameObject.SetActive(false);
            }
        }

        Vector3 pos = rewardTitle.localPosition;
        pos.x = -1.0f * (rewardCount * (rewardsGrid.cellWidth * 0.5f));
        rewardTitle.localPosition = pos;
        rewardBox.width = 10 + Mathf.RoundToInt(rewardCount * rewardsGrid.cellWidth);

        rewardsGrid.enabled = true;
    }

    #endregion

    #region BoxReward List

    [Header("BoxReward List")]
    [SerializeField] private GenericReward[] boxRewards;
    [SerializeField] private UIGrid boxRewardsGrid;

    private void InitBoxRewards(int selectStage)
    {
        var rewardsData = ManagerAdventure.EventData.GetAdvEventBoxRewards(selectStage);

        for (int i = 0; i < boxRewards.Length; i++)
        {
            if (i < rewardsData.Count)
            {
                boxRewards[i].gameObject.SetActive(true);
                boxRewards[i].SetReward(rewardsData[i]);
            }
            else
            {
                boxRewards[i].gameObject.SetActive(false);
            }
        }

        boxRewardsGrid.enabled = true;
    }

    #endregion

    #region SpineObject

    [Header("SpineObject")]
    [SerializeField] private GameObject spineScreenObject;
    private GameObject spineInstance = null;

    private void MakeSpineObject()
    {
        spineInstance = ObjectMaker.Instance.RenderTextureObjectMake("AdventureEvent_Ready_SpineObject");
        spineScreenObject.SetActive(true);
    }

    private void DestroySpineObject()
    {
        Destroy(spineInstance);
    }

    #endregion
}
