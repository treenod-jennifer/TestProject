using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using ServiceSDK;

public class UIItemShopSpecialItem : MonoBehaviour
{
    private enum ItemType
    {
        None,
        clover,
        wing,
        coin,
        diamond
    }

    private class ItemInfo
    {
        public ItemType itemType;
        public int idx;
        public string g_code;
        public int endTs;
        public int price_type;  // 0 : 인앱, 2 : 코인, 3 : 다이아
        public List<string> price_value;
        public int buy_count;
        public int limit_count;
        public List<Reward> rewardList;
    }
    
    private ItemInfo itemInfo = new ItemInfo();
    [SerializeField] private UIUrlTexture texture_Item;
    [SerializeField] private UISprite sprite_PriceItem;
    [SerializeField] private UILabel text_Timer;
    [SerializeField] private UILabel text_Price;
    [SerializeField] private UILabel text_Price_S;
    [SerializeField] private UILabel text_BuyCount;
    [SerializeField] private GameObject obj_LimitObject;
    [SerializeField] private GameObject obj_ButtonDisable;
    [SerializeField] private UILabel text_LimitCount;
    [SerializeField] private NGUIFont font_InAppText;
    [SerializeField] private NGUIFont font_CurrnecyText;
    
    private Global.UseMoneyData currentUesJewelData;
    private Global.UseMoneyData currentUesCoinData;
    
    public void Init_Clover(CdnShopClover.SpecialItem data)
    {
        itemInfo.itemType = ItemType.clover;
        itemInfo.idx = data.idx;
        itemInfo.g_code = data.g_code;
        itemInfo.endTs = data.end_ts;
        itemInfo.price_type = 3;
        itemInfo.limit_count = data.purchase_limit;
        itemInfo.price_value = new List<string> () {data.price.ToString()};
        itemInfo.rewardList = data.gifts_json.ToList();
        UpdateBuyCount();
        
        Init_UI();
    }
    
    public void Init_InAppClover(CdnShopClover.SpecialItemInApp data)
    {
        itemInfo.itemType    = ItemType.clover;
        itemInfo.idx         = data.idx;
        #if UNITY_IOS
        itemInfo.g_code = data.sku_i;
        #elif UNITY_ANDROID
        itemInfo.g_code = data.sku_a;
        #else
        itemInfo.g_code = data.sku_a;
        #endif        
        itemInfo.endTs       = data.end_ts;
        itemInfo.price_type  = 0;
        itemInfo.limit_count = data.purchase_limit;
        itemInfo.price_value = data.prices.ToList();
        itemInfo.rewardList  = data.gifts_json.ToList();
        UpdateBuyCount();
        
        Init_UI();
    }

    public void Init_Wing(CdnShopWing.SpecialItem data)
    {
        itemInfo.itemType = ItemType.wing;
        itemInfo.idx = data.idx;
        itemInfo.g_code = data.g_code;
        itemInfo.endTs = data.end_ts;
        itemInfo.price_type = data.price_type;
        itemInfo.limit_count = data.purchase_limit;
        itemInfo.price_value = new List<string> () {data.price_value.ToString()};
        itemInfo.rewardList = data.gifts_json.ToList();
        UpdateBuyCount();
        
        Init_UI();
    }
    
    public void Init_Coin(CdnShopCoin.SpecialItem data)
    {
        itemInfo.itemType = ItemType.coin;
        itemInfo.idx = data.idx;
        itemInfo.g_code = data.g_code;
        itemInfo.endTs = data.end_ts;
        itemInfo.price_type = 3;
        itemInfo.limit_count = data.purchase_limit;
        itemInfo.price_value = new List<string> () {data.price.ToString()};
        itemInfo.rewardList = data.gifts_json.ToList();
        UpdateBuyCount();
        
        Init_UI();
    }
    
    public void Init_Jewel(CdnShopJewel.SpecialItem data)
    {
        itemInfo.itemType = ItemType.diamond;
        itemInfo.idx = data.idx;
        #if UNITY_IOS
        itemInfo.g_code = data.sku_i;
        #elif UNITY_ANDROID
        itemInfo.g_code = data.sku_a;
        #else
        itemInfo.g_code = data.sku_a;
        #endif        
        itemInfo.endTs = data.end_ts;
        itemInfo.price_type = 0;
        itemInfo.limit_count = data.purchase_limit;
        itemInfo.price_value = data.prices.ToList();
        itemInfo.rewardList = data.gifts_json.ToList();
        UpdateBuyCount();
        Init_UI();
    }
    
    private void Init_UI()
    {
        texture_Item.LoadCDN(Global.gameImageDirectory, $"IconShopItem/special", $"shop_special_{itemInfo.itemType.ToString()}_{itemInfo.idx}");
        obj_ButtonDisable.SetActive(itemInfo.limit_count > 0 && itemInfo.limit_count <= itemInfo.buy_count);
        obj_LimitObject.SetActive(itemInfo.limit_count > 0);
        float posY = itemInfo.limit_count > 0 ? 15.5f : 3f;
        if (itemInfo.price_type > 0)
        {
            sprite_PriceItem.gameObject.SetActive(true);
            text_Price.transform.localPosition = new Vector3(18f, posY, 0);
            text_Price.text = itemInfo.price_value[0];
            text_Price_S.text = itemInfo.price_value[0];
            text_Price.bitmapFont = font_CurrnecyText;
            text_Price_S.bitmapFont = font_CurrnecyText;
            text_Price.fontSize = 40;
            text_Price_S.fontSize = 40;
            string priceType = itemInfo.price_type == 2 ? "coin" : "gem";
            sprite_PriceItem.spriteName = $"icon_{priceType}_y";
            sprite_PriceItem.MakePixelPerfect();
            sprite_PriceItem.transform.localPosition = new Vector2(text_Price.transform.localPosition.x - 38f - (text_Price.text.Length * 10), text_Price.transform.localPosition.y);
        }
        else // 인앱 상품일 때
        {
            sprite_PriceItem.gameObject.SetActive(false);
            text_Price.transform.localPosition = new Vector3(-5f, posY, 0);
            text_Price.bitmapFont = font_InAppText;
            text_Price_S.bitmapFont = font_InAppText;
            text_Price.fontSize = 35;
            text_Price_S.fontSize = 35;
            List<string> priceList = new List<string>();
            foreach (var prices in itemInfo.price_value)
                priceList.Add(prices.ToString());
            string priceText = LanguageUtility.GetPrices(priceList);
#if !UNITY_EDITOR
            if (ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(itemInfo.g_code))
                priceText = ServiceSDKManager.instance.billingProductInfoDic[itemInfo.g_code].displayPrice;
#endif
            text_Price.text = priceText;
            text_Price_S.text = priceText;
        }

        text_BuyCount.text = itemInfo.buy_count.ToString();
        text_LimitCount.text = "/ " + itemInfo.limit_count;
        EndTsTimer.Run(text_Timer, itemInfo.endTs);
    }

    private void OnClickPurchaseButton()
    {
        if (!UIPopupShop._instance.bCanTouch)
            return;
        if (itemInfo.limit_count > 0 && itemInfo.buy_count >= itemInfo.limit_count)
            return;
        
        string priceText = "";
        if (itemInfo.price_type == 0)
        {
            List<string> priceList = new List<string>();
            foreach (var prices in itemInfo.price_value)
                priceList.Add(prices.ToString());
            priceText = LanguageUtility.GetPrices(priceList);
            double price = LanguageUtility.GetPrice(priceList);
            string currency = string.Empty;
#if !UNITY_EDITOR
            if (ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(itemInfo.g_code))
            {
                priceText = ServiceSDKManager.instance.billingProductInfoDic[itemInfo.g_code].displayPrice;
                price = double.Parse(ServiceSDKManager.instance.billingProductInfoDic[itemInfo.g_code].price, System.Globalization.CultureInfo.InvariantCulture);
                currency = ServiceSDKManager.instance.billingProductInfoDic[itemInfo.g_code].currency;
            }
#endif
            ManagerUI._instance.OpenMinorCheckPopup(price, currency, () => { OpenPurchaseConfirmPopup(priceText); });
        }
        else
        {
            priceText = itemInfo.price_value[0];
            OpenPurchaseConfirmPopup(priceText);
        }
    }

    private void OpenPurchaseConfirmPopup(string priceText)
    {
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, OnClickConfirmButton, true);

        string packageName = "";
        if (itemInfo.itemType.Equals(ItemType.clover))
            packageName = Global._instance.GetString("item_1");
        else if (itemInfo.itemType.Equals(ItemType.wing))
            packageName = Global._instance.GetString("item_11");
        else if (itemInfo.itemType.Equals(ItemType.coin))
            packageName = Global._instance.GetString("item_4");
        else if (itemInfo.itemType.Equals(ItemType.diamond))
            packageName = Global._instance.GetString("item_2");
        
        PackagePriceType priceType = Global._instance.ChangeTypeFromRewardType((RewardType)itemInfo.price_type);
        
        //itemInfo.idx(1,2,3...)를 알파벳(A,B,C)으로 변환
        byte[] b = new byte[1]{byte.Parse((64 + itemInfo.idx).ToString())};
        string option = Encoding.ASCII.GetString(b);
        string text = Global._instance.GetString("n_b_9").Replace("[1]", packageName + Global._instance.GetString("pack_115") + option);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: priceType, price_value: priceText);
        string imgStr = itemInfo.itemType.ToString();
        if (itemInfo.itemType == ItemType.diamond) imgStr = "jewel";
        popupSystem.SetResourceImage($"Message/{imgStr}");
        popupSystem.ShowBuyInfo(GetBuyInfoKey());
    }

    private string GetBuyInfoKey()
    {
        switch (itemInfo.itemType)
        {
            case ItemType.clover:
                return "buyinfo_spc_3";
            case ItemType.coin:
                return "buyinfo_spc_2";
            case ItemType.diamond:
                return "buyinfo_spc_1";
            case ItemType.wing:
                return "buyinfo_spc_4";
        }

        return "";
    }

    private void OnClickConfirmButton()
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;
        UIPopupShop._instance.bCanTouch = false;

        if (itemInfo.price_type == 0) //인앱 결제
        {
            ManagerNotice.instance.ShowMajorNotice(OnCheckBillingStateComplete, OnCheckBillingStateFail);
        }
        else
        {
            // 인앱 결제 스페셜 아이템이 아닌 경우, price_value에는 무조건 하나의 값만 세팅되므로 0인덱스의 값을 꺼내 구매 가능한지 체크
            int purchasePrice = Int32.Parse(itemInfo.price_value[0]);
            if (CheckCanPurchase(itemInfo.price_type, purchasePrice))
            {
                if (itemInfo.price_type == 2)   // 코인으로 구매
                    currentUesCoinData = Global._instance.UseCoin(purchasePrice);
                else  // 다이아로 구매
                    currentUesJewelData = Global._instance.UseJewel(purchasePrice);
                
                
                if (itemInfo.itemType == ItemType.clover)
                    ServerAPI.BuySpecialClover(itemInfo.idx, PurchaseComplete);
                else if (itemInfo.itemType == ItemType.wing)
                    ServerAPI.BuySpecialWing(itemInfo.idx, PurchaseComplete);
                else if (itemInfo.itemType == ItemType.coin)
                    ServerAPI.BuySpecialCoin(itemInfo.idx, PurchaseComplete);
            }
            else
            {
                UIPopupShop._instance.bCanTouch = true;
                if (itemInfo.price_type == 2)
                    ManagerUI._instance.LackCoinsPopUp();
                else if (itemInfo.price_type == 3)
                    ManagerUI._instance.LackDiamondsPopUp();
            }
        }
    }

    private void OnCheckBillingStateComplete()
    {
        if (itemInfo.itemType == ItemType.clover)
        {
            ServiceSDKManager.instance.PurchaseSpecialInAppClover(itemInfo.idx, itemInfo.g_code, CompletePurchaseSpecialInApp);
        }
        else if (itemInfo.itemType == ItemType.diamond)
        {
            ServiceSDKManager.instance.PurchaseSpecialJewel(itemInfo.idx, itemInfo.g_code, CompletePurchaseSpecialInApp);
        }
    }
    
    private void OnCheckBillingStateFail(Trident.Error error)
    {
        //터치 가능.
        UIPopupShop._instance.bCanTouch = true;
        Extension.PokoLog.Log("============GetNotice error");
        ShowFailedPopup();
    }

    private void CompletePurchaseSpecialInApp(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
            //터치 가능.
            UIPopupShop._instance.bCanTouch = true;
            Extension.PokoLog.Log("============Billing error");
            ShowFailedPopup();
            return;
        }

        if (itemInfo.itemType == ItemType.clover)
        {
            ServerAPI.CompleteBuyInAppSpecialClover(itemInfo.idx, PurchaseComplete);
        }
        else if (itemInfo.itemType == ItemType.diamond)
        {
            ServerAPI.CompleteBuySpecialJewel(itemInfo.idx, PurchaseComplete);
        }
        
#if !UNITY_EDITOR
        //MAT로그 전송
        ServiceSDK.BillingProductInfo billingProductInfo = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[itemInfo.g_code];

        if (billingProductInfo == null)
        {
            return;
        }
        
        ServiceSDK.AdjustManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency);
#endif
    }

    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        
        if(itemInfo.itemType == ItemType.diamond)
            popup.SetResourceImage("Message/jewel");
        else if(itemInfo.itemType == ItemType.clover)
            popup.SetResourceImage("Message/clover");
    }

    private bool CheckCanPurchase(int type, int price)
    {
        if (type == 0)
            return true;
        else if (type == 2)
            return Global.coin >= price;
        else if (type == 3)
            return Global.jewel >= price;
        else
            return false;
    }

    private void PurchaseComplete(Protocol.BaseResp resp)
    {
        if (UIPopupShop._instance.bCanTouch == false)
            UIPopupShop._instance.bCanTouch = true;
        
        UpdateBuyCount();
        Init_UI();

        if (resp.IsSuccess)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            string text = Global._instance.GetString("n_b_3").Replace("[1]", Global._instance.GetString("pack_115"));
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
            popupSystem.SetResourceImage("Message/ok");

            // 메일함 업데이트
            MessageData.SetUserData();
            
            // UI업데이트.
            Global.clover = (int)(ServerRepos.User.AllClover);
            Global.coin = (int)(ServerRepos.User.AllCoin);
            Global.wing = (int)(ServerRepos.User.AllWing);
            Global.jewel = (int)(ServerRepos.User.AllJewel);
            ManagerUI._instance.UpdateUI();

            SendGrowthyLog(resp);
        }
        else
        {
            ShowFailedPopup();
        }
    }

    private void UpdateBuyCount()
    {
        if (itemInfo.itemType == ItemType.clover)
            itemInfo.buy_count = ServerRepos.UserSpecialShop.Clover.Find(x => x.Idx == itemInfo.idx).purchase_count;
        else if (itemInfo.itemType == ItemType.wing)
            itemInfo.buy_count = ServerRepos.UserSpecialShop.Wing.Find(x => x.Idx == itemInfo.idx).purchase_count;
        else if (itemInfo.itemType == ItemType.coin)
            itemInfo.buy_count = ServerRepos.UserSpecialShop.Coin.Find(x => x.Idx == itemInfo.idx).purchase_count;
        else if (itemInfo.itemType == ItemType.diamond)
            itemInfo.buy_count = ServerRepos.UserSpecialShop.Jewel.Find(x => x.Idx == itemInfo.idx).purchase_count;
    }

    private void SendGrowthyLog(Protocol.BaseResp resp)
    {
        string itemCode = itemInfo.g_code;
        
        // 재화 소비 그로시 로그
        if (itemInfo.price_type == 2) // 코인으로 구매
        {
            var growthyMoney = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.SC,
                GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_SPECIALSHOPITEM,
                currentUesCoinData.usePMoney * -1,
                currentUesCoinData.useFMoney * -1,
                (int)(ServerRepos.User.coin),
                (int)(ServerRepos.User.fcoin),
                itemCode
            );
            var docMoney = JsonConvert.SerializeObject(growthyMoney);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
        }
        else if (itemInfo.price_type == 3) // 다이아로 구매
        {
            var growthyMoney = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.FC,
                GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_SPECIALSHOPITEM,
                currentUesJewelData.usePMoney * -1,
                currentUesJewelData.useFMoney * -1,
                (int) (ServerRepos.User.jewel),
                (int) (ServerRepos.User.fjewel),
                itemCode
            );
            var docMoney = JsonConvert.SerializeObject(growthyMoney);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
        }
        else
        {
            // 인앱 구매 : 여기서 그로시 로그 기재하지 않음
        }

        // 상품 수령 그로시 로그 : 재화 4종
        if (itemInfo.itemType == ItemType.clover)
        {
            if (itemInfo.price_type == 3)
            {
                SendGrowthyLog_CloverSpecialItem();
            }
            else if (itemInfo.price_type == 0)
            {
                SendGrowthyLog_CloverSpecialInAppItem();
            }
        }
        else if (itemInfo.itemType == ItemType.coin)
        {
            SendGrowthyLog_CoinSpecialItem();
        }
        else if (itemInfo.itemType == ItemType.diamond)
        {
            SendGrowthyLog_JewelSpecialItem();
        }
        else if (itemInfo.itemType == ItemType.wing)
        {
            SendGrowthyLog_WingSpecialItem();
        }

        // 상품 수령 그로시 로그 : 기타 아이템 (메일함 전송)
        foreach (var reward in itemInfo.rewardList)
        {
            // 보상 수령 관련 그로시 로그 전송
            GrowthyCusmtomLogHelper.SendGrowthyLog(
                reward.type, reward.value,
                GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
                GrowthyCustomLog_ITEM.Code_L_RSN.G_SPECIALSHOPITEM_REWARD,
                $"SPECIALSHOPITEM_GET_{itemInfo.g_code}_{itemInfo.idx}"
            );
        }
    }

    private void SendGrowthyLog_CloverSpecialItem()
    {
        // 0 : 개수
        var itemData = ServerContents.CloverShop.SpecialByDia.Find(x => x.idx == itemInfo.idx);
        if (itemData.clover_type == 0)
        {
            var playEnd = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
                0,
                itemData.pClover,
                0,
                (int)(ServerRepos.User.AllClover),
                itemInfo.g_code
            );
            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }
        // 1: 시간
        else if (itemData.clover_type == 1)
        {
            var useReadyItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                itemInfo.g_code,
                $"CLOVER_{itemData.pClover / 60}m",
                1,
                GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
            );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

            var useCloverItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                itemInfo.g_code,
                $"CLOVER_{itemData.pClover / 60}m",
                -1,
                GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
            );
            var cloverDoc = JsonConvert.SerializeObject(useCloverItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", cloverDoc);
        }
    }
    
    private void SendGrowthyLog_CloverSpecialInAppItem()
    {
        // 0 : 개수
        var itemData = ServerContents.CloverShop.SpecialByInApp.Find(x => x.idx == itemInfo.idx);
        if (itemData.clover_type == 0)
        {
            var playEnd = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
                itemData.pClover,
                itemData.fClover,
                (int)(ServerRepos.User.clover),
                (int)(ServerRepos.User.fclover),
                itemInfo.g_code
            );
            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }
        // 1: 시간
        else if (itemData.clover_type == 1)
        {
            var useReadyItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                itemInfo.g_code,
                $"CLOVER_{itemData.pClover / 60}m",
                1,
                GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
            );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

            var useCloverItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                itemInfo.g_code,
                $"CLOVER_{itemData.pClover / 60}m",
                -1,
                GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
            );
            var cloverDoc = JsonConvert.SerializeObject(useCloverItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", cloverDoc);
        }
    }
    
    private void SendGrowthyLog_WingSpecialItem()
    {
        var itemData = ServerContents.WingShop.Special.Find(x => x.idx == itemInfo.idx);
        // 0 : 개수
        if (itemData.wing_type == 0)
        {        
            var getWA = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.WA,
                GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
                0,
                itemData.pWing,
                0,
                (int)(GameData.User.AllWing)
            );
            var docGetWA = JsonConvert.SerializeObject(getWA);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docGetWA);
        }
        // 1: 시간
        else if (itemData.wing_type == 1)
        {
            var useReadyItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                $"WING_{itemData.pWing / 60}m",
                itemData.g_code,
                1,
                GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
            );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

            var useWingItem = new GrowthyCustomLog_ITEM
            (
                GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                $"WING_{itemData.pWing / 60}m",
                itemData.g_code,
                -1,
                GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
            );
            var wingDoc = JsonConvert.SerializeObject(useWingItem);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", wingDoc);
        }
    }

    private void SendGrowthyLog_CoinSpecialItem()
    {
        var itemData = ServerContents.CoinShop.Special.Find(x => x.idx == itemInfo.idx);
        
        var playEnd = new GrowthyCustomLog_Money
        (
            GrowthyCustomLog_Money.Code_L_TAG.SC,
            GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
            GetPJewelFJewel(itemData).Item1,
            GetPJewelFJewel(itemData).Item2,
            (int)(ServerRepos.User.coin),
            (int)(ServerRepos.User.fcoin),
            itemData.g_code
        );
        var doc = JsonConvert.SerializeObject(playEnd);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
    }

    private (int, int) GetPJewelFJewel(CdnShopCoin.SpecialItem itemData)
    {
        int payJewel = itemData.pCoin;
        int freeJewel = itemData.fCoin;
        int usePay = 0;
        int useFree = 0;
        
        if ((int)ServerRepos.User.jewel >= itemData.price)
        {
            usePay = itemData.price;
        }
        else if ((int)ServerRepos.User.jewel > 0)
        {
            usePay = (int)ServerRepos.User.jewel;
            useFree = itemData.price - (int)ServerRepos.User.jewel;
        }
        else
        {
            useFree = itemData.price;
        }

        if (usePay > 0 && useFree == 0)
        {
            payJewel = itemData.pCoin;
            freeJewel = itemData.fCoin;
        }
        else if (usePay == 0 && useFree > 0)
        {
            payJewel = 0;
            freeJewel = itemData.pCoin + itemData.fCoin;
        }
        else if (usePay > 0 && useFree > 0)
        {
            payJewel = usePay * 100;
            freeJewel = useFree * 100 + itemData.fCoin;
        }
        
        return (payJewel, freeJewel);
    }

    private void SendGrowthyLog_JewelSpecialItem()
    {
        var itemData = ServerContents.JewelShop.Special.Find(x => x.idx == itemInfo.idx);     

        var playEnd = new GrowthyCustomLog_Money
        (
            GrowthyCustomLog_Money.Code_L_TAG.FC,
            GrowthyCustomLog_Money.Code_L_MRSN.G_SPECIALSHOPITEM_REWARD,
            itemData.pJewel,
            itemData.fJewel,
            (int)(ServerRepos.User.jewel),
            (int)(ServerRepos.User.fjewel),
            itemInfo.g_code
        );
        var doc = JsonConvert.SerializeObject(playEnd);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
    }
}
