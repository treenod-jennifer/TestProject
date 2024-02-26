using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class UIItemShopClover : MonoBehaviour
{
    public static UIItemShopClover _instance;
    
    public                   UIPanel    scrollView;
    [SerializeField] private GameObject obj_ItemAD;
    [SerializeField] private GameObject obj_ItemShop;
    [SerializeField] private GameObject obj_ItemSpecialShop;
    [SerializeField] private GameObject obj_CloverEvent;
    [SerializeField] private UISprite   sprite_ScrollBarBG;
    [SerializeField] private UISprite   sprite_ScrollBar;
    [SerializeField] private UISprite   panelBG;
    [SerializeField] private GameObject btn_InviteFriend;
    [SerializeField] private Transform  btn_RequestClover;
    [SerializeField] private GameObject objScrollEnd;
    [SerializeField] private GameObject objButtonRoot;

    //법률 버튼 관련
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel    labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel    labelPrecautions;

    private UIItemShopADItem                       item_AD;
    private Dictionary<int, UIItemShopSpecialItem> specialList_Shop      = new Dictionary<int, UIItemShopSpecialItem>();
    private List<CdnShopClover.SpecialItem>        specialList_Data      = new List<CdnShopClover.SpecialItem>();
    private List<CdnShopClover.SpecialItemInApp>   specialList_InAppData = new List<CdnShopClover.SpecialItemInApp>();
    private List<UIItemShopItem>                   itemList_Shop         = new List<UIItemShopItem>();

    private bool alreadyMakeItem = false;
    private int itemIndex = -1;
    private bool bCanTouch = true;
    
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
    
    private void Start()
    {
        _instance = this;
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text  = TermsOfUse;
        labelPrecautions.text = Precautions;

        if (string.IsNullOrEmpty(labelTermsOfUse.text) && string.IsNullOrEmpty(labelPrecautions.text))
        {
            objButtonRoot.transform.localPosition = new Vector3(4f, -844f, 0f);
            panelBG.height                        = 775;
            objScrollEnd.transform.localPosition  = new Vector3(0f, -742f, 0f);
        }
        else
        {
            objButtonRoot.transform.localPosition = new Vector3(4f, -804f, 0f);
            panelBG.height                        = 740;
            objScrollEnd.transform.localPosition  = new Vector3(0f, -710f, 0f);
        }
        
        sprite_ScrollBarBG.height = (int) panelBG.height - 15;
        sprite_ScrollBar.height   = (int) panelBG.height - 10;
        sprite_ScrollBar.GetComponent<UIScrollBar>().value = 0;
        scrollView.GetComponent<UIScrollView>().ResetPosition();

        if (!alreadyMakeItem)
        {
            MakeItem();
            alreadyMakeItem = true;
        }
        
        obj_CloverEvent.SetActive(ServerRepos.LoginCdn.sendCloverEventVer != 0);
        SettingDepth();

#if UNITY_IOS || UNITY_ANDROID
        if (ServerRepos.LoginCdn.EnableInvite == 0)
        {
            btn_InviteFriend.SetActive(false);
            btn_RequestClover.localPosition = new Vector3 (0, btn_RequestClover.localPosition.y, 0);
        }
#endif
    }
    
    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }

    private void OnEnable()
    {
        NewCheck();

        if (!alreadyMakeItem)
        {
            MakeItem();
            alreadyMakeItem = true;
        }

        foreach (var specialShopItem in specialList_Shop)
        {
            {
                var data = specialList_Data.Find(x => x.idx == specialShopItem.Key);
                if (data != null)
                {
                    specialShopItem.Value.Init_Clover(data);
                    continue;
                }
            }
            {
                var data = specialList_InAppData.Find(x => x.idx == specialShopItem.Key);
                if (data != null)
                {
                    specialShopItem.Value.Init_InAppClover(data);
                }
            }
        }

        for (int i = 0; i < itemList_Shop.Count; i++)
            itemList_Shop[i].Init_Clover( ServerContents.CloverShop.Normal[i] );
        
        if (ManagerUI._instance != null)
            ManagerUI._instance.IsTopUIAdventureMode = false;
    }

    private void MakeItem()
    {
        // 정렬 1 : 인앱, 다이아 스페셜 상품 (미션 6스테이지 이후 노출)
        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            foreach (var shopItem in ServerRepos.UserSpecialShop.Clover)
            {
                {
                    var data = ServerContents.CloverShop.SpecialByInApp.Find(x=>x.idx == shopItem.Idx);
                    if (data != null)
                    {
                        if (shopItem.purchase_count < data.purchase_limit || data.purchase_limit == 0) // 구매 카운트가 구매 제한보다 낮거나, 구매 제한값이 0(무제한)일 경우 아이템 생성
                        {
                            if (Global.LeftTime(data.end_ts) > 0)
                            {
                                var cloverItem = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform,
                                                              obj_ItemSpecialShop)
                                                          .GetComponent<UIItemShopSpecialItem>();
                                specialList_Shop.Add(data.idx, cloverItem);
                                specialList_InAppData.Add(data);
                            }
                        }
                    }
                }
                {
                    var data = ServerContents.CloverShop.SpecialByDia.Find(x=>x.idx == shopItem.Idx);
                    if (data != null)
                    {
                        if (shopItem.purchase_count < data.purchase_limit || data.purchase_limit == 0) // 구매 카운트가 구매 제한보다 낮거나, 구매 제한값이 0(무제한)일 경우 아이템 생성
                        {
                            if (Global.LeftTime(data.end_ts) > 0)
                            {
                                var cloverItem = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform,
                                                              obj_ItemSpecialShop)
                                                          .GetComponent<UIItemShopSpecialItem>();
                                specialList_Shop.Add(data.idx, cloverItem);
                                specialList_Data.Add(data);
                            }
                        }
                    }
                }
            }
        }
        
        // 정렬 2 : 광고 상품
        ADCheck();
        // 정렬 3 : 일반 상품
        foreach (var data in ServerContents.CloverShop.Normal)
        {
            var cloverItem = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemShop)
                .GetComponent<UIItemShopItem>();
            itemList_Shop.Add(cloverItem);
        }
    }

    public void ADCheck()
    {
        if (AdManager.ADCheck(AdManager.AdType.AD_6))
        {
            if (item_AD == null)
            {
                item_AD = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemAD)
                    .GetComponent<UIItemShopADItem>();
            }
            item_AD.Init(UIItemShopADItem.ADItemType.Clover);
        }
        else
        {
            if (item_AD != null)
            {
                StartCoroutine(DestroyObject());
            }
        }
    }

    private IEnumerator DestroyObject()
    {
        Destroy(item_AD.gameObject);
        yield return new WaitUntil(() => !item_AD.isActiveAndEnabled);
        item_AD = null;
        yield return new WaitUntil(() => item_AD == null);
        scrollView.GetComponentInChildren<UIGrid>().enabled = true;
    }

    private void NewCheck()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return;
        {
            string str = "";
            foreach (var item in ServerContents.CloverShop.SpecialByDia)
            {
                string itemKey = item.g_code + "_" + item.idx;
                str += (itemKey + ",");
            }
            
            foreach (var item in ServerContents.CloverShop.SpecialByInApp)
            {
                string itemKey;
                
                #if UNITY_IPHONE
                itemKey = item.sku_i + "_" + item.idx;
                #elif UNITY_ANDROID
                itemKey = item.sku_a + "_" + item.idx;
                #else
                itemKey = item.sku_a + "_" + item.idx;
                #endif
                str += (itemKey + ",");
            }

            if (PlayerPrefs.GetString("ShopCloverSpecialItemList") != str)
                PlayerPrefs.SetString("ShopCloverSpecialItemList", str);
        }
        
        ManagerUI._instance.newIcon_clover.SetActive(false);
    }

    private void SettingDepth()
    {
        int depth = UIPopupShop._instance.uiPanel.depth;
        int layer = UIPopupShop._instance.uiPanel.sortingOrder;

        scrollView.depth = depth + 1;

        if (layer < 10) return;

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
    }

    #region 버튼 이벤트

    void OnClickBtnIviteFriend()
    {
        if (bCanTouch == false)
            return;

        bCanTouch = false;

        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            //터치 가능.
            OnTouch();
            ManagerUI._instance.GuestLoginSignInCheck(true, UIPopupShop._instance.uiPanel.sortingOrder + 1);
        }
        else
        {
            UIPopupShop._instance._callbackClose = ManagerUI._instance.OpenPopupInvite;
            //터치 가능.
            UIPopupShop._instance._callbackEnd += OnTouch;
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnRequestClover()
    {
        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            //터치 가능.
            OnTouch();
            ManagerUI._instance.GuestLoginSignInCheck(true, UIPopupShop._instance.uiPanel.sortingOrder + 1);
        }
        else
        {
            UIPopupShop._instance._callbackClose = ManagerUI._instance.OpenPopupRequestClover;
            //터치 가능.
            UIPopupShop._instance._callbackEnd += OnTouch;
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    public void OnClickBtnBuyClover(int clickItemIndex)
    {
        var itemList = ServerContents.CloverShop.Normal;
        if (bCanTouch == false)
            return;
        for (int i = 0; i < itemList.Count; i++)
            if (ServerContents.CloverShop.Normal[i].idx == clickItemIndex)
                itemIndex = i;


        if (ServerContents.CloverShop.Normal[itemIndex].clover_type == 0)
        {
            string text = Global._instance.GetString("n_b_1").Replace("[1]", Global._instance.GetString("item_1"));
            text = text.Replace("[n]", ServerContents.CloverShop.Normal[itemIndex].pClover.ToString());
            MakePurchasePopUP(text);
        }
        else if (ServerContents.CloverShop.Normal[itemIndex].clover_type == 1)
        {
            string text = Global._instance.GetString("n_b_2").Replace("[n]", (ServerContents.CloverShop.Normal[itemIndex].pClover / 60).ToString());
            MakePurchasePopUP(text);
        }
    }
    
    void OnClickBtnConfirm()
    {
        if (itemIndex < 0)
        { 
            //터치 가능.
            OnTouch();
            return;
        }

        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

        //살 수 있는 돈이 되는지 검사.
        if (CheckCanPurchase(ServerContents.CloverShop.Normal[itemIndex].price) == true)
        {
            useIndex = itemIndex;
            var purchasePrice = ServerContents.CloverShop.Normal[useIndex].price;
            if (useIndex >= 0)
            {
                if ((int)ServerRepos.User.jewel >= purchasePrice) usePJewel = purchasePrice;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = purchasePrice - (int)ServerRepos.User.jewel;
                }
                else useFJewel = purchasePrice;
            }
            ServerAPI.BuyClover(ServerContents.CloverShop.Normal[itemIndex].idx, PurchaseClover);
        }
        else
        {
            ManagerUI._instance.LackDiamondsPopUp();
            //터치 가능.
            OnTouch();
            itemIndex = -1;
        }
    }
    
    #endregion
    
    #region 구매 플로우
    
    void MakePurchasePopUP(string text)
    {
        if (this.bCanTouch == false)
            return;
        
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        popupSystem.SetCallbackSetting(1, OnClickBtnConfirm, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: PackagePriceType.Jewel, price_value: ServerContents.CloverShop.Normal[itemIndex].price.ToString());
        popupSystem.SetResourceImage("Message/clover");
        popupSystem.ShowBuyInfo("buyinfo_cls_1");
    }
    
    int useFJewel = 0;
    int usePJewel = 0;


    bool CheckCanPurchase(int price)
    {
        if (Global.jewel < price)
        {   
            return false;
        }
        return true;
    }

    int useIndex = -1;
    void PurchaseClover(Protocol.BaseResp resp)
    {
        if (resp.IsSuccess)
        {
            PurchasePopUP();

            //UI업데이트.
            Global.clover = (int)(ServerRepos.User.AllClover);
            Global.jewel = (int)(ServerRepos.User.AllJewel);
            ManagerUI._instance.UpdateUI();


            //그로씨            
            {
                var data = ServerContents.CloverShop.Normal[useIndex];
                string productCode = data.g_code;
                if (data.saleLevel > 0) productCode += "_s";
                
                // 0 : 개수
                if (data.clover_type == 0)
                {
                    var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_BY_1ST,
                        0,
                        data.pClover,
                        0,
                        (int)(ServerRepos.User.AllClover),
                        productCode
                    );
                    var doc = JsonConvert.SerializeObject(playEnd);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
                }
                // 1: 시간
                else if (data.clover_type == 1)
                {
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                        productCode,
                        $"CLOVER_{data.pClover / 60}m",
                        1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
                    );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

                    var useCloverItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                        productCode,
                        $"CLOVER_{data.pClover / 60}m",
                        -1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
                    );
                    var cloverDoc = JsonConvert.SerializeObject(useCloverItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", cloverDoc);
                }
                
                if (useIndex >= 0)
                {
                    var useJewelPurchaseClover = new ServiceSDK.GrowthyCustomLog_Money
                           (
                           ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                           ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_AP_CLOVAR,
                           -usePJewel,
                           -useFJewel,
                          (int)(ServerRepos.User.jewel),
                          (int)(ServerRepos.User.fjewel),
                           productCode
                           );
                    var docJewel = JsonConvert.SerializeObject(useJewelPurchaseClover);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docJewel);
                }
                
            }
        }
        else
        {
            ShowFailedPopup();
        }
        itemIndex = -1;
    }

    void PurchasePopUP()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        string text = "";
        //클로버 5개 구매 시. 
        if (itemIndex == 0)
        {
            text = Global._instance.GetString("n_b_3").Replace("[1]", Global._instance.GetString("item_1"));
            text = text.Replace("[n]", ServerContents.CloverShop.Normal[useIndex].pClover.ToString());
        }
        //FreeTime Clover 구매 시.
        else
        {
            text = Global._instance.GetString("n_b_8").Replace("[n]", (ServerContents.CloverShop.Normal[itemIndex].pClover / 60).ToString());
        }

        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, OnTouch);
        popupSystem.SetResourceImage("Message/ok");
    }

    void OnTouch()
    {
        //터치 가능.
        this.bCanTouch = true;
    }

    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false, OnTouch);
        popup.SetResourceImage("Message/clover");
    }
    
    #endregion
    
    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
}
