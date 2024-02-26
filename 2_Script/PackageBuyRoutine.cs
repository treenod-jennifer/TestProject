using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

//서버에서 받는 패키지의 결제 타입
public enum PackagePriceType
{
    Cash,
    Jewel,
    Coin,
    None
}

//패키지 별 타입 인덱스
public enum PackageType
{
    AD = -1,
    SpeedClearPack,
    NRUPack,
    SpotDiaPack,
    
    NPUPack = 4,
    CompletePack,
    NRUPass,
    CBUPass_PremiumPass,
    SelectPack,
    CBUPack,
    NPUSpotPack,
    DiaStash,
    
    LuckyRoulette = 13,
    SpotCloverPack,
    
    none = 100
}

public class PackageBuyRoutine : MonoBehaviour 
{
    public UIPopupBase hostPopup;
    public CdnShopPackage packageData;
    public int packageId;
    public string productCode;
    public string packMessage;
    public bool autoOpen;

    public Method.FunctionVoid OnAutoGachaCompleted;

    private string messagePackageIconPath = "Message/icon_buyPackage";
    private string messageFailIconPath = "Message/tired";
    
    static public bool packageSuggestedAtLogin = false;

    /// <summary>
    /// 패키지 구입 완료 후 기존과 다른 구매완료창을 생성하고 싶은경우 Event등록.
    /// </summary>
    public System.Action PostPurchaseCompleteEvent = null;

    public static IEnumerator CanGachaCheck(List<int> gachas, UIPopupBase hostPopup, Method.FunctionVoid onCanGacha, Method.FunctionVoid onCannot)
    {
        bool success = true;
        hostPopup.bCanTouch = false;
        for (int i = 0; i < gachas.Count; ++i)
        {
            bool responsed = false;
            NetworkLoading.MakeNetworkLoading(1f);
            ServerAPI.AdventureCanGacha(gachas[i], (Protocol.AdventureCanGachaResp resp)
                => {
                    
                    NetworkLoading.EndNetworkLoading();
                    if (resp.IsSuccess)
                    {
                        if (!resp.canGacha)
                        {
                            success = false;
                        }
                    }
                    responsed = true;
                });

            yield return new WaitUntil(() => {return responsed == true; });

            if (success == false )
                break;
        }

        hostPopup.bCanTouch = true;
        if( success )
        {
            onCanGacha();
        }
        else onCannot();
    }

    public void OnClickButton()
    {
        if (hostPopup.bCanTouch == false)
            return;

        if (this.packageData.payType == 0 && string.IsNullOrEmpty(this.productCode))
        {
            return;
        }

        if (ManagerAdventure.CheckStartable() == false && packageData.HaveAdventureItem())
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_1"), false);
            popup.SetResourceImage(messageFailIconPath);
            popup.SortOrderSetting();
            return;
        }
        
        List<int> gachaIds = packageData.GetGachaId();

        if (gachaIds.Count > 0)
        {
            StartCoroutine(CanGachaCheck( gachaIds, hostPopup,
                () => {
                    OpenPopupConfirmPurchase();
                }, 
                () => {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_2"), false);
                    popup.SetResourceImage(messageFailIconPath);
                    popup.SortOrderSetting();
                } ));
        }
        else
        {
            OpenPopupConfirmPurchase();
        }
    }

    public void OnClickButtonImmediately()
    {
        if (hostPopup.bCanTouch == false)
            return;

        if (this.packageData.payType == 0 && string.IsNullOrEmpty(this.productCode))
        {
            return;
        }

        if (ManagerAdventure.CheckStartable() == false && packageData.HaveAdventureItem())
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_1"), false);
            popup.SetResourceImage(messageFailIconPath);
            popup.SortOrderSetting();
            return;
        }
        
        List<int> gachaIds = packageData.GetGachaId();

        if (gachaIds.Count > 0)
        {
            StartCoroutine(CanGachaCheck( gachaIds, hostPopup,
                () => {
                    ConfirmPurchaseDia();
                }, 
                () => {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_2"), false);
                    popup.SetResourceImage(messageFailIconPath);
                    popup.SortOrderSetting();
                } ));
        }
        else
        {
            ConfirmPurchaseDia();
        }
    }

    internal void init(UIPopupBase instance, CdnShopPackage data, string message = "", bool autoOpen = false)
    {
        hostPopup = instance;
        this.packageId = data.idx;
        this.packageData = data;
        this.packMessage = message;
        this.autoOpen = autoOpen;

#if UNITY_IPHONE
        this.productCode = data.sku_i;
#elif UNITY_ANDROID
        this.productCode = data.sku_a;
#endif
    }



    void OpenPopupConfirmPurchase()
    {
        hostPopup.bCanTouch = true;

        string message = "";
        
        if (string.IsNullOrEmpty(packMessage))
        {
            string name = string.Format("pack_{0}", packageData.title_msg == 0 ? packageId : packageData.title_msg);
            message = Global._instance.GetString("n_b_9").Replace("[1]", Global._instance.GetString(name));
        }
        else
        {
            message = packMessage;
        }
        
        string priceText = "";
        double price = -1;
        string currency = string.Empty;
        if (packageData.payType == 0)
        {
            List<string> priceList = new List<string>();
            foreach (var prices in packageData.prices)
                priceList.Add(prices.ToString());
            priceText = LanguageUtility.GetPrices(priceList);
            price = LanguageUtility.GetPrice(priceList);
            currency = string.Empty;
#if !UNITY_EDITOR
#if UNITY_IPHONE
            string _productCode = packageData.sku_i;
#elif UNITY_ANDROID
            string _productCode = packageData.sku_a;
#endif
            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(_productCode))
            {
                priceText = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].displayPrice;
                price = double.Parse(ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].price, System.Globalization.CultureInfo.InvariantCulture);
                currency = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].currency;
            }
#endif
            // CBU 패키지, NPU 스팟 패키지의 경우 시스템 팝업을 오픈하지 않고 바로 구매 플로우 진입
            if (packageData.type == (int)PackageType.CBUPack || packageData.type == (int)PackageType.NPUSpotPack || packageData.type == (int)PackageType.DiaStash)
            {
                ManagerUI._instance.OpenMinorCheckPopup(price, currency, ConfirmPurchaseDia);
            }
            else
            {
                ManagerUI._instance.OpenMinorCheckPopup(price, currency, () => { OpenPurchaseConfirmPopup(message, priceText); });
            }
        }
        else
        {
            priceText = packageData.prices[0];
            
            if (packageData.type == (int)PackageType.CBUPack || packageData.type == (int)PackageType.NPUSpotPack || packageData.type == (int)PackageType.DiaStash)
            {
                ConfirmPurchaseDia();
            }
            else
            {
                OpenPurchaseConfirmPopup(message, priceText);
            }
        }
    }

    private void OpenPurchaseConfirmPopup(string message, string priceText)
    {
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, ConfirmPurchaseDia, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, price_type: (PackagePriceType)packageData.payType, price_value: priceText);
        popupSystem.SetResourceImage(messagePackageIconPath);
        popupSystem.ShowBuyInfo(packageData.payType != 0 ? "buyinfo_dpk_1" : "buyinfo_pk_1");
    }

    int usePay = 0;
    int useFree = 0;

    private void ConfirmPurchaseDia()
    {
        if (hostPopup.bCanTouch == false)
            return;
        hostPopup.bCanTouch = false;

        if (this.packageData.payType == 1)
        {
            int purchasePrice = (int)(System.Convert.ToDouble(packageData.prices[0]));

            if (Global.jewel < purchasePrice)
            {
                hostPopup.bCanTouch = true;
                ManagerUI._instance.LackDiamondsPopUp();
                return;
            }

            if ((int)ServerRepos.User.jewel >= purchasePrice)
            {
                usePay = purchasePrice;
            }
            else if ((int)ServerRepos.User.jewel > 0)
            {
                usePay = (int)ServerRepos.User.jewel;
                useFree = purchasePrice - (int)ServerRepos.User.jewel;
            }
            else
            {
                useFree = purchasePrice;
            }

            ServerAPI.BuyPackageByJewel(packageData.idx, this.productCode, OnPostPurchase);
            return;
        }

#if UNITY_EDITOR
        ServerUserShopPackage pack = new ServerUserShopPackage();
        pack.idx = 1;
        pack.vsn = 1;
        pack.sku = "a_pkv_0_1";
        ServerRepos.UserShopPackages.Add(pack);
        ManagerUI._instance.UpdateUI();
#endif

        CheckBillingState();
    }

    private void CheckBillingState()
    {
        ManagerNotice.instance.ShowMajorNotice(RequestPurchase, OnCheckBillingStateFail);
    }

    private void OnCheckBillingStateFail(Trident.Error error)
    {
        //터치 가능.
        hostPopup.bCanTouch = true;
        Extension.PokoLog.Log("============GetNotice error");
        this.ShowFailedPopup();
    }

    HashSet<long> prevInbox = new HashSet<long>();

    private void RequestPurchase()
    {
        prevInbox.Clear();
        if (packageData.HaveGacha() )
        {
            ServerAPI.Inbox(
                (resp) => {

                    if (resp.IsSuccess)
                    {
                        for (int i = 0; i < resp.inbox.Count; ++i)
                        {
                            prevInbox.Add(resp.inbox[i].index);
                        }

                        ServiceSDK.ServiceSDKManager.instance.PurchasePackage(this.packageId, this.productCode, OnPurchase);
                    }
                    else
                    {
                        hostPopup.bCanTouch = true;
                        ShowFailedPopup();
                    }
                });

        }
        else
        {
            ServiceSDK.ServiceSDKManager.instance.PurchasePackage(this.packageId, this.productCode, OnPurchase);
        }
    }

    /// <summary>
    /// 구입 콜백
    /// </summary>
    /// <param name="isSuccess"></param>
    private void OnPurchase(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
            Extension.PokoLog.Log("============Billing error");
            this.ShowFailedPopup();
            hostPopup.bCanTouch = true;
            return;
        }

        ServerAPI.BuyShopPackage(this.packageId, this.productCode, orderId, OnPostPurchase);


#if !UNITY_EDITOR
        //MAT로그 전송
        ServiceSDK.BillingProductInfo billingProductInfo = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this.productCode];

        if (billingProductInfo == null)
        {
            return;
        }
        
        ServiceSDK.AdjustManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency);
#endif
    }

    /// <summary>
    /// 게임서버에서 결과 받기
    /// </summary>
    /// <param name="resp"></param>
    private void OnPostPurchase(Protocol.BaseResp resp)
    {
        hostPopup.bCanTouch = true;

        if (resp.IsSuccess == false)
        {
            Extension.PokoLog.Log("============PostPurchase error");
            ShowFailedPopup();
            return;
        }

        if (this.packageData.payType == 1)
        {
            var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                   (
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_PACKAGE,
                   -usePay,
                   -useFree,
                   (int)(ServerRepos.User.jewel),
                   (int)(ServerRepos.User.fjewel),
                   this.productCode
                   );
            var docDia = JsonConvert.SerializeObject(GetDia);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
        }
        
        if (this.packageData.type == 10)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.NPU_SPOT_PACKAGE,
                "NPU_SPOT_PACKAGE_PURCHASE",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                ServerContents.NpuSpotPackage.ver,
                Global.stageIndex.ToString()
            );
            var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        if (this.packageData.paidJewel > 0 || this.packageData.freeJewel > 0)
        {
            var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                   (
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                   this.packageData.paidJewel,
                   this.packageData.freeJewel,
                   (int)(ServerRepos.User.jewel),
                   (int)(ServerRepos.User.fjewel),
                   this.productCode
                   );
            var docDia = JsonConvert.SerializeObject(GetDia);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
        }

        foreach (var item in this.packageData.items)
        {
            if( item.type == (int)RewardType.coin)
            {
                var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                  (
                  ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                  ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                  item.value,
                  0,
                  (int)(ServerRepos.User.coin),
                  (int)(ServerRepos.User.fcoin),
                  this.productCode
                  );
                var docDia = JsonConvert.SerializeObject(GetDia);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);

            }
            else if(item.type == (int)RewardType.FreeCoin)
            {
                var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                  (
                  ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                  ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                  0,
                  item.value,
                  (int)(ServerRepos.User.coin),
                  (int)(ServerRepos.User.fcoin),
                  this.productCode
                  );
                var docDia = JsonConvert.SerializeObject(GetDia);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
            }
            else
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                (int)item.type,
                item.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                this.productCode
                );
            }
        }

        if(autoOpen)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                this.productCode,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        //획득한 상품 ui에 갱신
        Global.jewel = GameData.User.AllJewel;
        Global.clover = GameData.User.AllClover;
        Global.coin = GameData.User.AllCoin;
        Global.star = GameData.User.Star;
        ManagerUI._instance.UpdateUI();


        // 2019.05.24 : 패키지 구매 후 오토가챠 삭제
        //string completeCallbackString = packageData.GetGachaId() != 0 ? "PostProcess_AutoGacha" : "OnClickBtnClose_Base";
        string completeCallbackString = "OnClickBtnClose_Base";

        if (PostPurchaseCompleteEvent != null)
        {
            PostPurchaseCompleteEvent.Invoke();
            return;
        }

        string name = Global._instance.GetString("p_t_4");
        string message = Global._instance.GetString("n_b_14");
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.FunctionSetting(1, completeCallbackString, gameObject, true);
        popup.FunctionSetting(3, completeCallbackString, gameObject, true);
        popup.FunctionSetting(4, completeCallbackString, gameObject, true);
        popup.SortOrderSetting();
        popup.InitSystemPopUp(name, message, false);
        popup.SetResourceImage(messagePackageIconPath);
    }

    /// <summary>
    /// 구입 실패시 알림 팝업 열기
    /// </summary>
    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        popup.SetResourceImage(messageFailIconPath);
    }

    protected void PostProcess_AutoGacha()
    {
        ServerAPI.Inbox(
            (resp) => {
                if (resp.IsSuccess)
                {
                    for (int i = 0; i < resp.inbox.Count; ++i)
                    {
                        // 시작할 때 확인했던 우편함에서 못봤던 가챠티켓이 보이면, 냅다 까고보기
                        // 일단 이 코드대로라면 1개 이상의 가챠티켓은 못까는게 정상
                        if (!prevInbox.Contains(resp.inbox[i].index))
                        {
                            if (resp.inbox[i].type == (int)RewardType.gachaTicket)
                            {
                                ServerAPI.ReceiveMail((int)resp.inbox[i].index,
                                    (recvResp) => {
                                        if (recvResp.IsSuccess)
                                        {
                                            ManagerAdventure.OnInit((bool b) =>
                                            {
                                                ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                                                {
                                                    animalIdx = recvResp.userAdvAnimal.animalId,
                                                    exp = recvResp.userAdvAnimal.exp,
                                                    gettime = 0,
                                                    grade = recvResp.userAdvAnimal.grade,
                                                    level = recvResp.userAdvAnimal.level,
                                                    overlap = recvResp.userAdvAnimal.Overlap
                                                };

                                                var newAnimalInstance = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                                                ManagerAdventure.User.SyncFromServer_Animal();
                                                ManagerAIAnimal.Sync();
                                                ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimalInstance, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                                            });
                                        }

                                        if( OnAutoGachaCompleted != null )
                                            OnAutoGachaCompleted();
                                    });
                                break;
                            }
                        }
                    }
                }
            });
    }


}
