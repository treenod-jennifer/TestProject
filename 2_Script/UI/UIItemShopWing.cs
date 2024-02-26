using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ServiceSDK;

public class UIItemShopWing : MonoBehaviour
{
    public static UIItemShopWing _instance;
    
    public UIPanel scrollView;
    [SerializeField] private GameObject obj_ItemAD;
    [SerializeField] private GameObject obj_ItemShop;
    [SerializeField] private GameObject obj_ItemSpecialShop;
    [SerializeField] private GameObject obj_WingUpgrade;
    [SerializeField] private UISprite sprite_PriceItem;
    [SerializeField] private UISprite sprite_MainBox;
    [SerializeField] private UISprite sprite_ScrollBarBG;
    [SerializeField] private UISprite sprite_ScrollBar;
    [SerializeField] private UILabel label_Purchase;
    [SerializeField] private UILabel label_Purchase_S;
    [SerializeField] private Transform obj_scrollEnd;
    
    private UIItemShopADItem item_AD;
    private List<UIItemShopSpecialItem> specialList_Shop = new List<UIItemShopSpecialItem>();
    private List<CdnShopWing.SpecialItem> specialList_Data = new List<CdnShopWing.SpecialItem>();
    private List<UIItemShopItem> itemList_Shop = new List<UIItemShopItem>();

    private bool alreadyMakeItem = false;
    private int itemIndex = -1;
    private Global.UseMoneyData currentUesJewelData;
    private Global.UseMoneyData currentUesCoinData;

    private enum BuyMethod
    {
        Coin,
        Diamond,
    }

    private void Start()
    {
        _instance = this;
        SetWingUpgradeObject(GameData.User.maxWing <= 3);
        SettingDepth();
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

        for (int i = 0; i < specialList_Shop.Count; i++)
            specialList_Shop[i].Init_Wing( specialList_Data[i] );
        for (int i = 0; i < itemList_Shop.Count; i++)
            itemList_Shop[i].Init_Wing( ServerContents.WingShop.Normal[i] );
        SetScrollView();
        
        if (ManagerUI._instance != null)
            ManagerUI._instance.IsTopUIAdventureMode = true;
    }

    public void MakeItem()
    {
        // 정렬 1 : 스페셜 상품 (미션 6스테이지 이후 노출)
        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            foreach (var data in ServerContents.WingShop.Special)
            {
                int purchaseCount = ServerRepos.UserSpecialShop.Wing.Find(x => x.Idx == data.idx).purchase_count;
                if (purchaseCount < data.purchase_limit ||
                    data.purchase_limit == 0) // 구매 카운트가 구매 제한보다 낮거나, 구매 제한값이 0(무제한)일 경우 아이템 생성
                {
                    if (Global.LeftTime(data.end_ts) > 0)
                    {
                        var wingItem = NGUITools
                            .AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemSpecialShop)
                            .GetComponent<UIItemShopSpecialItem>();
                        specialList_Data.Add(data);
                        specialList_Shop.Add(wingItem);
                    }
                }
            }
        }
        // 정렬 2 : 광고 상품
        ADCheck();
        // 정렬 3 : 일반 상품
        foreach (var data in ServerContents.WingShop.Normal)
        {
            var wingItem = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemShop)
                .GetComponent<UIItemShopItem>();
            itemList_Shop.Add(wingItem);
        }
    }

    public void ADCheck()
    {
        if (AdManager.ADCheck(AdManager.AdType.AD_7))
        {
            if (item_AD == null)
            {
                item_AD = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemAD)
                    .GetComponent<UIItemShopADItem>();
            }
            item_AD.Init(UIItemShopADItem.ADItemType.Wing);
        }
        else
        {
            if (item_AD != null)
            {
                Destroy(item_AD.gameObject);
                item_AD = null;
                scrollView.GetComponentInChildren<UIGrid>().enabled = true;
            }
        }
    }

    private void NewCheck()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return;
        string str = "";
        foreach (var item in ServerContents.WingShop.Special)
        {
            string itemKey = item.g_code + "_" + item.idx;
            str += (itemKey + ",");
        }

        if (PlayerPrefs.GetString("ShopWingSpecialItemList") != str)
            PlayerPrefs.SetString("ShopWingSpecialItemList", str);
        
        ManagerUI._instance.newIcon_wing.SetActive(false);
    }
    
    public void SetWingUpgradeObject(bool isYetUpgrade)
    {
        obj_WingUpgrade.SetActive(isYetUpgrade);
        if (isYetUpgrade)
        {
            label_Purchase.text = ServerRepos.LoginCdn.wingExtPrice.ToString();
            label_Purchase_S.text = ServerRepos.LoginCdn.wingExtPrice.ToString();
            sprite_PriceItem.spriteName = "icon_gem";
            sprite_PriceItem.MakePixelPerfect();
            sprite_PriceItem.transform.localPosition = new Vector2(label_Purchase.transform.localPosition.x - 38f - (label_Purchase.text.Length * 10), sprite_PriceItem.transform.localPosition.y);
            sprite_MainBox.height = 752;
            sprite_MainBox.transform.localPosition = new Vector3(0, -148, 0);
        }
        else
        {
            sprite_MainBox.height = 900;
            sprite_MainBox.transform.localPosition = new Vector3(0, 0, 0);
        }
        SetScrollView();
    }

    private void SetScrollView()
    {
        if (GameData.User.maxWing <= 3)
            obj_scrollEnd.localPosition = new Vector3(0, -715, 0);
        else
            obj_scrollEnd.localPosition = new Vector3(0, -862.5f, 0);
        sprite_ScrollBarBG.height = (int) sprite_MainBox.height - 15;
        sprite_ScrollBar.height = (int) sprite_MainBox.height - 10;
        scrollView.GetComponent<UIScrollView>().ResetPosition();
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

    #region 버튼이벤트

    public void OnClickBtnBuyQuantityLimitUp()
    {
        ManagerUI._instance.OpenPopup<UIPopupBuyWingUp>();
        Debug.Log("BuyQuantityLimitUp");
    }

    public void OnClickBtnBuyWing(int dataIndex)
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;

        for (int i = 0; i < ServerContents.WingShop.Normal.Count; i++)
        {
            if (ServerContents.WingShop.Normal[i].idx == dataIndex)
                itemIndex = i;
        }

        var data = ServerContents.WingShop.Normal[itemIndex];

        if (data.wing_type == 0)
        {
            string text = Global._instance.GetString("n_b_1");
            text = text.Replace("[1]", Global._instance.GetString("item_11"));
            text = text.Replace("[n]", data.pWing.ToString());
            if (data.price_type == 2)
                MakePurchasePopUP(text, BuyMethod.Coin, data);
            else if (data.price_type == 3)
                MakePurchasePopUP(text, BuyMethod.Diamond, data);
        }
        else if (data.wing_type == 1)
        {
            PackagePriceType priceType = Global._instance.ChangeTypeFromRewardType((RewardType)data.price_type);
            
            UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();   
            popupSystem.SetCallbackSetting(1, OnClickBtnConfirm_BuyWingFreeTime, true);
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), 
                Global._instance.GetString("n_b_2").Replace("[n]", (data.pWing / 60).ToString()), false, price_type: priceType, price_value: data.price_value.ToString());
            popupSystem.SetResourceImage("Message/wingTime");
            popupSystem.ShowBuyInfo("buyinfo_ws_1");
        }
    }

    void OnClickBtnConfirm_BuyWingFreeTime()
    {
        int price = ServerContents.WingShop.Normal[itemIndex].price_value;

        if (CheckCanPurchase(BuyMethod.Diamond, price) == true)
        {
            currentUesJewelData = Global._instance.UseJewel(price);
            ServerAPI.BuyWing(ServerContents.WingShop.Normal[itemIndex].idx, OnWingFreetimePurchased);
        }
        else
        {
            ManagerUI._instance.LackDiamondsPopUp();
            //터치 가능.
            UIPopupShop._instance.bCanTouch = true;
        }
    }

    void OnClickBtnConfirm_Diamond()
    {
        OnClickBtnConfirm(BuyMethod.Diamond);
    }

    void OnClickBtnConfirm_Coin()
    {
        OnClickBtnConfirm(BuyMethod.Coin);
    }

    void OnClickBtnConfirm(BuyMethod method)
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;
        UIPopupShop._instance.bCanTouch = false;

        int price = ServerContents.WingShop.Normal[itemIndex].price_value;

        //살 수 있는 돈이 되는지 검사.
        if (CheckCanPurchase(method, price))
        {            
            switch(method)
            {
                case BuyMethod.Diamond:
                {
                    currentUesJewelData = Global._instance.UseJewel(price);
                        
                    ServerAPI.BuyWing(ServerContents.WingShop.Normal[itemIndex].idx, (Protocol.BaseResp resp) => { OnWingPurchased(resp, BuyMethod.Diamond); });
                }
                    break;
                case BuyMethod.Coin:
                {
                    currentUesCoinData = Global._instance.UseCoin(price);

                    ServerAPI.BuyWing(ServerContents.WingShop.Normal[itemIndex].idx, (Protocol.BaseResp resp) => { OnWingPurchased(resp, BuyMethod.Coin); });
                }
                    break;
            }
        }
        else
        {
            if (method == BuyMethod.Diamond)
                ManagerUI._instance.LackDiamondsPopUp();
            else
                ManagerUI._instance.LackCoinsPopUp();

            //터치 가능.
            UIPopupShop._instance.bCanTouch = true;
        }
    }
    
    #endregion

    #region 구매 플로우

    void MakePurchasePopUP(string text, BuyMethod buyMethod, CdnShopWing.NormalItem data ) 
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;

        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        if (buyMethod == BuyMethod.Diamond)
            popupSystem.SetCallbackSetting(1, OnClickBtnConfirm_Diamond, true);
        else
            popupSystem.SetCallbackSetting(1, OnClickBtnConfirm_Coin, true);
        
        PackagePriceType priceType = Global._instance.ChangeTypeFromRewardType((RewardType)data.price_type);

        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: priceType, price_value: data.price_value.ToString());
        popupSystem.SetResourceImage("Message/wing");
        popupSystem.ShowBuyInfo("buyinfo_ws_1");
    }
    
    void OnWingFreetimePurchased(Protocol.BaseResp resp)
    {
        if( resp.IsSuccess ){

            ManagerSound.AudioPlay(AudioInGame.PRAISE1);
            PopUpPurchaseSucceed();
            
            //UI업데이트.
            Global.clover = (int)(ServerRepos.User.AllClover);
            Global.jewel = (int)(ServerRepos.User.AllJewel);
            Global.coin = (int)(ServerRepos.User.AllCoin);
            Global.wing = (int)(ServerRepos.User.AllWing);
            Global.exp = (int)GameData.User.expBall;
            ManagerUI._instance.UpdateUI();

            SendGrowthyLog_Item_Wing60M();
        }
        else
        {
            ShowFailedPopup();
        }
    }
    
    bool CheckCanPurchase(BuyMethod buyMethod, int price)
    {
        int assetValue = buyMethod == BuyMethod.Diamond ? Global.jewel : Global.coin;
        if (assetValue < price)
        {
            return false;
        }
        return true;
    }

    void OnWingPurchased(Protocol.BaseResp resp, BuyMethod method)
    {
        if (resp.IsSuccess)
        {
            ManagerSound.AudioPlay(AudioInGame.PRAISE1);
            PopUpPurchaseSucceed();

            //UI업데이트.
            Global.clover = (int)(ServerRepos.User.AllClover);
            Global.jewel = (int)(ServerRepos.User.AllJewel);
            Global.coin = (int)(ServerRepos.User.AllCoin);
            Global.wing = (int)(ServerRepos.User.AllWing);
            Global.exp = (int)GameData.User.expBall;
            ManagerUI._instance.UpdateUI();
            
            {
                // 세일 중 상품 구매인 경우 상품 코드 뒤에 _s 추가됨
                var data = ServerContents.WingShop.Normal[itemIndex];
                string itemCode = data.g_code;
                if(data.saleLevel > 0)
                    itemCode += "_s";
                
                if (data.price_type == 3)   // 다이아로 구매
                {
                    var useDIAMOND = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_WING,
                           -currentUesJewelData.usePMoney,
                           -currentUesJewelData.useFMoney,
                          (int)(ServerRepos.User.jewel),
                          (int)(ServerRepos.User.fjewel),
                        itemCode
                        );
                    var docDIAMOND = JsonConvert.SerializeObject(useDIAMOND);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDIAMOND);
                }
                else if (data.price_type == 2)  // 코인으로 구매
                {
                    var useCOIN = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_WING,
                           -currentUesCoinData.usePMoney,
                           -currentUesCoinData.useFMoney,
                          (int)(ServerRepos.User.coin),
                          (int)(ServerRepos.User.fcoin),
                        itemCode
                        );
                    var docCOIN = JsonConvert.SerializeObject(useCOIN);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCOIN);
                }

                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN tempMRSN = data.price_type == 3 ?
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_BY_1ST : ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_BY_2ND;                

                var getWA = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
                    tempMRSN,
                    0,
                    data.pWing,
                    0,
                    (int)(GameData.User.AllWing)
                    );
                var docGetWA = JsonConvert.SerializeObject(getWA);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docGetWA);
            }
        }
        else
        {
            ShowFailedPopup();
        }
    }

    void PopUpPurchaseSucceed()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();

        var data = ServerContents.WingShop.Normal[itemIndex];
        if (data.wing_type == 0)
        {
            string text = Global._instance.GetString("n_b_3");
            text = text.Replace("[1]", Global._instance.GetString("item_11"));
            text = text.Replace("[n]", data.pWing.ToString());
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, () => UIPopupShop._instance.bCanTouch = true);
        }
        else if (ServerContents.WingShop.Normal[itemIndex].wing_type == 1)
        {
            string text = Global._instance.GetString("n_b_8");
            text = text.Replace("[n]", (data.pWing / 60).ToString());
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, () => UIPopupShop._instance.bCanTouch = true);
        }

        popupSystem.SetResourceImage("Message/ok");
    }

    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false, () => UIPopupShop._instance.bCanTouch = true);
        popup.SetResourceImage("Message/wing");
    }

    private void SendGrowthyLog_Item_Wing60M()
    {
        var data = ServerContents.WingShop.Normal[itemIndex];
        string itemCode = data.g_code;
        if (data.saleLevel > 0) itemCode += "_s";
        
        // 다이아 구매면 FC, 코인 구매면 SC 태그
        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG _tag = GrowthyCustomLog_Money.Code_L_TAG.FC;
        if (data.price_type == 2)
            _tag = GrowthyCustomLog_Money.Code_L_TAG.SC;
            

        var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
        (
            _tag,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_WING,
            currentUesJewelData.usePMoney * -1,
            currentUesJewelData.useFMoney * -1,
            (int)(ServerRepos.User.jewel),
            (int)(ServerRepos.User.fjewel),
            itemCode
        );
        var docMoney = JsonConvert.SerializeObject(growthyMoney);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);

        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
        (
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
            $"WING_{data.pWing / 60}m",
            itemCode,
            1,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
        );
        var doc = JsonConvert.SerializeObject(useReadyItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

        var useWingItem = new ServiceSDK.GrowthyCustomLog_ITEM
        (
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
            $"WING_{data.pWing / 60}m",
            itemCode,
            -1,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
        );
        var wingDoc = JsonConvert.SerializeObject(useWingItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", wingDoc);
    }

    #endregion
}