using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSelectPackage : UIPopupBase
{
    public static UIPopupSelectPackage instance = null;

    private readonly int OptionMaxCount = 2; //옵션 개수 -1.
    
    [SerializeField] private UILabel labelTimer;
    
    [SerializeField] private UISprite[] sprites;
    
    [SerializeField] private UIPanel scrollViewOverPanel;
    [SerializeField] private UIScrollView scrollView;
    [SerializeField] UISprite[] scrollIndicators;

    //결제 버튼 텍스트.
    public UILabel[] buttonText;
    //특정 상거래법 버튼.
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    //자금결제법 버튼.
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    //LAN 페이지 버튼.
    [SerializeField] private UIItemLanpageButton lanpageButton;
    
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    
    [SerializeField] private GameObject labelLoading;
    [SerializeField] private GameObject labelDescription;
    [SerializeField] private GameObject objPackageBack;
    [SerializeField] private GameObject objDia;
    [SerializeField] private GameObject root;
    
    private bool isScrolling = false;
    private int centerIndex = 0;
    
    private List<CdnShopPackage> _packageDataList;
    private CdnSelectPackage _selectPackageData;
    
    private PackageBuyRoutine buyRoutine = null;
    public  bool              bAutoOpen  = false;

    private string TermsOfUse
    {
        get
        {
            string value = Global._instance.GetString("p_dia_4");

            if (string.IsNullOrEmpty(value))
                objTermsOfUse.SetActive(false);
            else
                objTermsOfUse.SetActive(true);

            return value;
        }
    }
    private string Precautions
    {
        get
        {
            string value = Global._instance.GetString("p_dia_3");

            if (string.IsNullOrEmpty(value))
                objPrecautions.SetActive(false);
            else
                objPrecautions.SetActive(true);

            return value;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        foreach (var sprite in sprites)
        {
            sprite.atlas = null;
        }

        if (buyRoutine != null)
        {
            buyRoutine.PostPurchaseCompleteEvent -= OnPurchaseComplete;
        }
        
        base.OnDestroy();
    }

    public override void OpenPopUp(int _depth)
    {
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;
        
        if (string.IsNullOrEmpty(labelTermsOfUse.text) && string.IsNullOrEmpty(labelPrecautions.text))
        {
            objPackageBack.SetActive(false);
            mainSprite.pivot = UIWidget.Pivot.Top;
            mainSprite.height = 1070;
            mainSprite.pivot = UIWidget.Pivot.Center;
            labelDescription.transform.localPosition = new Vector3(-346f, -510f);
            root.transform.localPosition= new Vector3(0f, -25f, 0f);
        }
        else
        {
            objPackageBack.SetActive(true);
            mainSprite.height = 1120;
            labelDescription.transform.localPosition = new Vector3(-346f, -560f);
        }
        
        base.OpenPopUp(_depth);
        scrollView.panel.depth = uiPanel.depth + 1;
        scrollViewOverPanel.depth = uiPanel.depth + 2;
    }
    
    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        
        scrollView.panel.useSortingOrder = true;
        scrollView.panel.sortingOrder = layer;
        
        scrollViewOverPanel.useSortingOrder = true;
        scrollViewOverPanel.sortingOrder = layer;
        
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }
   
    public void InitPopUp(CdnSelectPackage data)
    {
        //LAN 페이지 설정
        lanpageButton.On("LGPKV_SelectPackage", Global._instance.GetString("p_s_pack_2"));
        
        _selectPackageData = data;
        _packageDataList = new List<CdnShopPackage>();
        for (int i = 0; i <= OptionMaxCount; i++)
        { 
            if ( _selectPackageData.options != null && 
                 _selectPackageData.options.Count > i && 
                 ServerContents.Packages.ContainsKey(_selectPackageData.options[i]))
            {
                _packageDataList.Add(ServerContents.Packages[_selectPackageData.options[i]]);
            }
            else
            {
                _packageDataList.Add(new CdnShopPackage());
            }
        }

        SetScrollViewEvent();
        
        int index = Mathf.Clamp(ServerRepos.UserSelectPackage.userType, 0, OptionMaxCount);
        UpdateUI(index);
        UpdateScrollPosition(-514f * index);
        
        objDia.SetActive(_packageDataList[index].payType != 0);
        
        StartCoroutine(CoLoadAssetBundle());
    }

    private IEnumerator Start()
    {
        labelTimer.gameObject.SetActive(true);
        labelTimer.text = Global.GetTimeText_MMDDHHMM_Plus1(ServerRepos.UserSelectPackage.expiredAt);
        yield break;
    }
    
    IEnumerator CoLoadAssetBundle()
    {
        string assetName = $"pack_select_{_selectPackageData.assetVersion}_" + LanguageUtility.SystemCountryCodeForAssetBundle;
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path =
                $"Assets/5_OutResource/PackageSelect/{assetName}/PackageSelectAtlas.asset";
            var atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<NGUIAtlas>(path);
            
            if (atlas == null)
            {
                Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
                yield break;
            }

            foreach (var sprite in sprites)
            {
                sprite.atlas = atlas;
            }
#endif
        }
        else
        {
            NetworkLoading.MakeNetworkLoading(0.5f);

            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    NGUIAtlas atlas = assetBundle.LoadAsset<NGUIAtlas>("PackageSelectAtlas");
                    yield return null;

                    foreach (var sprite in sprites)
                    {
                        sprite.atlas = atlas;
                    }
                }
            }

            NetworkLoading.EndNetworkLoading();
        }

        labelLoading.SetActive(false);
    }
    
    private void SetScrollViewEvent()
    {
        scrollView.centerOnChild.onCenter = delegate(GameObject obj)
        {
            var index = int.Parse(obj.name);
            if (centerIndex != index)
            {
                UpdateUI(index);
            }
        };

        scrollView.onDragStarted = () => { isScrolling = true; };
        scrollView.onStoppedMoving = () => { isScrolling = false; };
    }

    private void UpdateScrollPosition(float x)
    {
        scrollView.MoveRelative(new Vector3(x, 0f));
    }
    
    private void UpdateUI(int index)
    {
        centerIndex = Mathf.Clamp(index, 0, OptionMaxCount);
        
        //Indicators
        for (int i = 0; i < scrollIndicators.Length; i++)
        {
            scrollIndicators[i].SetDimensions(i == centerIndex ? 40 : 22, 22);
        }
        
        //Arrow
        switch (centerIndex)
        {
            case 0:
                leftArrow.SetActive(false);
                rightArrow.SetActive(true);
                break;
            case 2:
                leftArrow.SetActive(true);
                rightArrow.SetActive(false);
                break;
            default:
                leftArrow.SetActive(true);
                rightArrow.SetActive(true);
                break;
        }

        UpdateButtonPriceLabel();
    }

    private void UpdateButtonPriceLabel()
    {
        if (_packageDataList[centerIndex].payType == 0)
        {
            string baseDisplayPrice = LanguageUtility.GetPrices(_packageDataList[centerIndex].prices);
            buttonText[0].transform.localPosition = new Vector3(-6, 3, 0);
            buttonText[0].text = baseDisplayPrice;
            buttonText[1].text = baseDisplayPrice;

#if !UNITY_EDITOR
#if UNITY_IPHONE
            string productCode = _packageDataList[centerIndex].sku_i;
#elif UNITY_ANDROID
            string productCode = _packageDataList[centerIndex].sku_a;
#endif
            string displayPrice = string.Empty;

            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
            {
                displayPrice = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[productCode].displayPrice;
                buttonText[0].text = displayPrice;
                buttonText[1].text = displayPrice;
            }
#endif
        }
        else
        {
            string baseDisplayPrice = _packageDataList[centerIndex].prices[0];
            buttonText[0].transform.localPosition = new Vector3(18, 3, 0);
            buttonText[0].text = baseDisplayPrice;
            buttonText[1].text = baseDisplayPrice;
        }
    }
    
    #region Button
    /// <summary>
    /// 구매 버튼 클릭.
    /// </summary>
    private void OnClickPurchaseBtnClick()
    {
        if (!bCanTouch || isScrolling) 
            return;
        if (_packageDataList == null || _packageDataList.Count <= centerIndex)
            return;

        var packageData = _packageDataList[centerIndex];
        if (buyRoutine == null)
        {
            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.PostPurchaseCompleteEvent += OnPurchaseComplete;
        }
        
        string name = string.Format("pack_{0}", packageData.title_msg == 0 ? packageData.idx : packageData.title_msg);
        name = Global._instance.GetString(name).Replace("[n]", (centerIndex + 1).ToString());
        string message = Global._instance.GetString("n_b_9").Replace("[1]", name);
        
        buyRoutine.init(this, packageData, message, autoOpen: bAutoOpen);
        
        //패키지 구매.
        buyRoutine.OnClickButton();
    }

    private void OnPurchaseComplete()
    {
        ServerRepos.UserSelectPackage.isExpired = true;
        ManagerUI._instance.PackageUpdate();
        
        string name = Global._instance.GetString("p_t_4");
        string message = Global._instance.GetString("n_b_14");
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.FunctionSetting(1, "OnClickBtnClose_Base", gameObject, true);
        popup.FunctionSetting(3, "OnClickBtnClose_Base", gameObject, true);
        popup.FunctionSetting(4, "OnClickBtnClose_Base", gameObject, true);
        popup.SortOrderSetting();
        popup.InitSystemPopUp(name, message, false);
        popup.SetResourceImage("Message/icon_buyPackage");
    }
    
    private void OnClickBtnClose_Base()
    {
        base.OnClickBtnClose();
    }
    
    protected override void OnClickBtnClose()
    {
        if (!bCanTouch) return;
        
        base.OnClickBtnClose();
    }

    private void OnClickLeftArrow()
    {
        if (!bCanTouch) return;
        if (isScrolling) return;
        
        int index = Mathf.Clamp(centerIndex - 1, 0, OptionMaxCount);
        if (index != centerIndex)
        {
            UpdateUI(index);
            UpdateScrollPosition(514f);
        }
    }
    
    private void OnClickRightArrow()
    {
        if (!bCanTouch) return;
        if (isScrolling) return;
        
        int index = Mathf.Clamp(centerIndex + 1, 0, OptionMaxCount);
        if (index != centerIndex)
        {
            UpdateUI(index);
            UpdateScrollPosition(-514f);
        }
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출.
    /// </summary>
    private void OnClickTermsOfUse()
    {
        if (!bCanTouch) return;
        
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }
    
    /// <summary>
    /// 자금결제법 링크 클릭시 호출.
    /// </summary>
    private void OnClickPrecautions()
    {
        if (!bCanTouch) return;
        
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
    #endregion
}
