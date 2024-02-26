using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureAnimal : UIPopupBase, ManagerAdventure.IUserAnimalListener, ISummonHost {
    public enum PopupMode
    {
        ViewMode,
        ChangeMode,
        OverLapMode
    }

    public static UIPopupStageAdventureAnimal _instance = null;
    private PopupMode popupMode;
    public PopupMode GetMode
    {
        get
        {
            return popupMode;
        }
    }
    public List<ManagerAdventure.AnimalInstance> characterList = new List<ManagerAdventure.AnimalInstance>();

    [Header("Linked Object")]
    [SerializeField] private GameObject changeMode;
    [SerializeField] private UIReuseGrid_StageAdventure_Animal scroll;

    [SerializeField] List<UIItemSummon> summonButtons;
    [SerializeField] protected UIItemLanpageButton[] lanpageButtons;

    [SerializeField] UIGrid summonButtonArrangeGrid;
    [SerializeField] private GameObject object_buyInfo;
    [SerializeField] private UILabel label_LanPage;
    [SerializeField] private UILabel label_lawAmendment;
    
    public bool disableAnimalUpdateUntilFocus = false;
    bool needAnimalUpdate = false;
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            if( ManagerAdventure.User != null )
                ManagerAdventure.User.animalListeners.Add(this);
        }
        
        bShowTopUI = true;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        if(ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Remove(this);

        base.OnDestroy();
    }

    public UIButtonDropDown sortButton;
    private void Update()
    {
        if (Global._touchEnd)
            sortButton.Reduction();
    }

    public virtual void InitTarget(PopupMode popupMode)
    {
        changeMode.SetActive(popupMode == PopupMode.ChangeMode);
        this.popupMode = popupMode;
    }

    protected virtual void InitData()
    {
        //데이터 초기화
        characterList.Clear();

        var tempList = ManagerAdventure.User.GetAnimalList(
            includeNull:    GetMode == PopupMode.ViewMode,
            filter:         currentFilter,
            sortOpt:        currentSort
        );

        characterList.AddRange(tempList);

        RefreshSummonButtons();

        if (ServerRepos.UserInfo.coinGachaCount == 0)
        {
            _callbackOpen += () => { ManagerTutorial.PlayTutorial(TutorialType.TutorialGacha_Adventure); };
        }
            
        object_buyInfo.SetActive(LanguageUtility.IsShowBuyInfo);
        label_LanPage.fontSize = LanguageUtility.IsShowBuyInfo ? 18 : 25;
        label_lawAmendment.fontSize = LanguageUtility.IsShowBuyInfo ? 18 : 25;
    }

    public override void OpenPopUp(int depth)
    {
        InitData();
        NewMarkUtility.isView = true;
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        scroll.ScrollReset();

        var scrollView = scroll.GetComponentInParent<UIScrollView>();
        if (scrollView != null)
        {
            scrollView.ResetPosition();
        }

        tapList.OnTab(currentFilter);
    }

    public void CloseSelf()
    {
        OnClickBtnClose();
    }

    public void RePaint(bool repositioning = false)
    {
        InitData();
        scroll.ScrollReset(repositioning);
    }

    public void OnAnimalChanged(int animalIdx)
    {
        if( disableAnimalUpdateUntilFocus )
        {
            needAnimalUpdate = true;
            return;
        }
        RePaint();
    }

    protected override void OnFocus(bool focus)
    {
        base.OnFocus(focus);

        if (focus)
        {
            bCanTouch = true;
            if( disableAnimalUpdateUntilFocus )
            {
                disableAnimalUpdateUntilFocus = false;
                if( needAnimalUpdate )
                {
                    needAnimalUpdate = false;
                    Tab_All();
                    RePaint();
                }
            }

            RefreshSummonButtons();
        }
            
    }

    #region ISummonHost implements
    public void RefreshSummonButtons()
    {
        CdnAdventureGachaProduct coinGacha = null;
        CdnAdventureGachaProduct diaGacha = null;

        bool tutorialGacha = ServerRepos.UserInfo.coinGachaCount == 0;
        if(tutorialGacha) // coin gacha count == 0
        {
            foreach( var g in ServerContents.AdvGachaProducts )
            {
                var gachaProduct = g.Value;
                if(  gachaProduct.asset_type == 2 && gachaProduct.price == 0 && ServerRepos.LoginCdn.GachaProducts.Exists(x => x == g.Key) )
                {
                    coinGacha = gachaProduct;
                    break;
                }
            }

            if (coinGacha == null)
                tutorialGacha = false;
        }

        if (tutorialGacha == false )
        {
            for (int i = 0; i < ServerRepos.LoginCdn.GachaProducts.Count; ++i)
            {
                int gachaPId = ServerRepos.LoginCdn.GachaProducts[i];
                var gachaProduct = ServerContents.AdvGachaProducts[gachaPId];

                // 타임오버된 가챠는 미리 거르고본다
                if (gachaProduct.expired_at != 0)
                {   // 시간제한 가챠
                    if (Global.LeftTime(gachaProduct.expired_at) < 0)
                        continue;
                }

                if (gachaProduct.asset_type == 2 && gachaProduct.price == 0)    // 튜토리얼 가챠는 거르고본다
                    continue;

                if (gachaProduct.asset_type == 2)
                {
                    if ( gachaProduct.expired_at != 0 )
                    {
                        coinGacha = gachaProduct;   // 시간제한 가챠는 통상가챠에 우선한다
                    }
                    else
                    {   // 통상가챠
                        if (coinGacha == null)
                            coinGacha = gachaProduct;
                        else if (coinGacha.expired_at == 0) // 통상가챠는 통상가챠만 덮어쓸 수 있다 (사실 두개이상의 통상가챠가 열려있으면 안되긴한다)
                            coinGacha = gachaProduct;
                    }

                }
                else if (gachaProduct.asset_type == 3)
                {
                    if (gachaProduct.expired_at != 0)
                    {
                        diaGacha = gachaProduct;   // 시간제한 가챠는 통상가챠에 우선한다
                    }
                    else
                    {   // 통상가챠
                        if (diaGacha == null)
                            diaGacha = gachaProduct;
                        else if (diaGacha.expired_at == 0) // 통상가챠는 통상가챠만 덮어쓸 수 있다 (사실 두개이상의 통상가챠가 열려있으면 안되긴한다)
                            diaGacha = gachaProduct;
                    }
                }
            }
        }

        List<CdnAdventureGachaProduct> usingGachas = new List<CdnAdventureGachaProduct>();
        if(coinGacha != null)
            usingGachas.Add(coinGacha);
        if (diaGacha != null)
            usingGachas.Add(diaGacha);

        int activeBtnCount = 0;
        for(int i = 0; i < usingGachas.Count; ++i)
        {
            summonButtons[i].InitData(this, usingGachas[i]);
            activeBtnCount++;

            if (summonButtons[i].gameObject.activeInHierarchy == false)
                summonButtons[i].gameObject.SetActive(true);

            if (summonButtons[i].premiumTrigger && summonButtons[i].premiumClose)
            {
                activeBtnCount--;
                summonButtons[i].gameObject.SetActive(false);

                //탐험모드 팝업의 동료 리스트 버튼 업데이트
                if (UIPopupStageAdventure._instance != null)
                    UIPopupStageAdventure._instance.AnimalListButtonUpdate();

                //로비 탐험모드 버튼 업데이트
                if (UIButtonAdventure._instance != null)
                    UIButtonAdventure._instance.SetButtonEvent(1);
            }
        }

        

        for (int i = activeBtnCount; i < summonButtons.Count; ++i)
        {
            summonButtons[i].gameObject.SetActive(false);
        }

        summonButtonArrangeGrid.enabled = true;


        //마일리지 또는 프리미엄 가챠가 있는경우 창 사이즈를 조절
        bool isMileageOrPremium = false;
        foreach(var button in summonButtons)
        {
            if(button.mileageTrigger || button.premiumTrigger)
            {
                isMileageOrPremium = true;
                break;
            }
            isMileageOrPremium = false;
        }
        SetPopupSize(isMileageOrPremium);


        for(int i = 0; i < lanpageButtons.Length; i++)
        {
            if(i < usingGachas.Count)
            {
                lanpageButtons[i].On(ManagerAdventure.GetGachaLanpageKey(usingGachas[i]), usingGachas[i].asset_type == 3);
            }
            else
            {
                lanpageButtons[i].Off();
            }
        }
    }

    public void Summon(CdnAdventureGachaProduct data)
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        disableAnimalUpdateUntilFocus = true;

        ManagerUI._instance.Summon(data, OnSummonFailed);
    }

    public void OnSummonFailed(int errCode)
    {
        bCanTouch = true;

        switch (errCode)
        {
            case 200:
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_29"), false, RefreshSummonButtons);
                    popup.SortOrderSetting();
                }
                break;
            case 201:
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_33"), false, Global.ReBoot);
                    popup.SortOrderSetting();
                }
                break;
        }
    }

    #endregion

    public GameObject GetSummonsButton()
    {
        for(int i=0; i<summonButtons.Count; i++)
        {
            if (summonButtons[i].gameObject.activeSelf)
                return summonButtons[i].gameObject;
        }

        return null;
    }

    [SerializeField] Transform probabilityRoot;
    private const int PREMIUM_POPUP_SIZE = 1110;
    private const int NORMAL_POPUP_SIZE = 1080;
    private const int PREMIUM_BUTTON_HEIGHT = -1125;
    private const int NORMAL_BUTTON_HEIGHT = -1095;
    private void SetPopupSize(bool isPremium)
    {
        mainSprite.height = isPremium ? PREMIUM_POPUP_SIZE : NORMAL_POPUP_SIZE;

        Vector3 buttonPos = probabilityRoot.localPosition;
        buttonPos.y = isPremium ? PREMIUM_BUTTON_HEIGHT : NORMAL_BUTTON_HEIGHT;
        probabilityRoot.localPosition = buttonPos;
    }

    #region Tab 기능
    public ManagerAdventure.AnimalFilter currentFilter { get; protected set; } = ManagerAdventure.AnimalFilter.AF_ALL;
    public ManagerAdventure.AnimalSortOption currentSort { protected get; set; } = ManagerAdventure.AnimalSortOption.byGrade;

    [SerializeField]
    private AnimalTabList tapList;

    public void OnTab(ManagerAdventure.AnimalFilter filter)
    {
        if (currentFilter != filter)
        {
            currentFilter = filter;

            tapList.OnTab(currentFilter);

            RePaint(true);
        }
    }

    public void Tab_All()
    {
        OnTab(ManagerAdventure.AnimalFilter.AF_ALL);
    }

    public void Tab_Monster()
    {
        OnTab(ManagerAdventure.AnimalFilter.AF_MONSTER);
    }

    #endregion
}
