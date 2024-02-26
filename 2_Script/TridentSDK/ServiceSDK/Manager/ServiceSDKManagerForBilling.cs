using UnityEngine;
using System;
using System.Collections.Generic;
using Trident;

namespace ServiceSDK
{
    public class BillingProductInfo
    {
        public string productId;
        public string currency;
        public string storeLocation;
        public string displayPrice;
        public string price;
    }

    public partial class ServiceSDKManager
    {
        private static bool isBillingInitialized = false;
        
        private string _purchasedProductId;
        private Action<bool, string> _billingHandler;
        
        public Dictionary<string, BillingProductInfo> billingProductInfoDic = new Dictionary<string, BillingProductInfo>();
        
        /// <summary>
        /// 빌링 서비스 초기화
        /// </summary>
        public void BillingServiceInitialize()
        {
            if (isBillingInitialized)
            {
                return;
            }

#if UNITY_EDITOR || UNUSED_LINE_SDK
            return;
#endif

            BillingService billingService = ServiceManager.getInstance().getService<BillingService>();

            if (billingService == null)
            {
                Extension.PokoLog.Log("============Billing Service Instance is null");
                return;
            }

            billingService.initialize((bool isSuccess, string result, string returnParam, Error error) =>
            {
                if (result != null)
                {
                    //코인 갱신
                    //게임서버 로그인전에 호출되니 따로 갱신할 필요는 없어보임
                }

                if (isSuccess)
                {
                    isBillingInitialized = true;
                    Extension.PokoLog.Log("============Billing Service returnParam: " + (returnParam != null ? returnParam : "null"));
                }
                else
                {
                    if (error != null)
                    {
                        Extension.PokoLog.Log("============Billing Service Error[" + error.getCode() + "]: " + error.getMessage());
                    }
                    else
                    {
                        Extension.PokoLog.Log("============Billing Service Error: null");
                    }
                }
            });
        }

        /// <summary>
        /// 게임 서버로 부터 받은 모든 상품 아이디 가져오기 (다이아샵 일반, 다이아샵 스페셜, Packages)
        /// </summary>
        private StringList GetAllProductId()
        {
            StringList productIdList = new StringList();

            foreach (var p in ServerContents.JewelShop.Normal)
            {
                string sku = "";
#if UNITY_IPHONE
                sku = p.sku_i;
#else
                sku = p.sku_a;
#endif
                productIdList.Add(sku);
            }
            
            foreach (var p in ServerContents.JewelShop.Special)
            {
                string sku = "";
#if UNITY_IPHONE
                sku = p.sku_i;
#else
                sku = p.sku_a;
#endif
                productIdList.Add(sku);
            }
            
            foreach (var p in ServerContents.JewelShop.Nru)
            {
                var sku = "";
#if UNITY_IPHONE
                sku = p.sku_i;
#else
                sku = p.sku_a;
#endif
                productIdList.Add(sku);
            }
            
            foreach (var p in ServerContents.CloverShop.SpecialByInApp)
            {
                string sku = "";
#if UNITY_IPHONE
                sku = p.sku_i;
#else
                sku = p.sku_a;
#endif
                productIdList.Add(sku);
            }

            foreach ( var p in ServerContents.Packages)
            {
                string sku = "";
#if UNITY_IPHONE
                sku = p.Value.sku_i;
#else
                sku = p.Value.sku_a;
#endif
                productIdList.Add(sku);
            }
            
            return productIdList;
        }

        /// <summary>
        /// 상품 정보 취득
        /// </summary>
        public void GetProductInfo(Action<bool> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true);
            return;
#endif

            BillingService billingService = ServiceManager.getInstance().getService<BillingService>();
            
            if (billingService == null)
            {
                Extension.PokoLog.Log("============Billing Service Instance is null");
                return;
            }

            // 게임 서버로 부터 받은 모든 상품 아이디를 SDK에 전달해서 상품 정보를 취득한 뒤 billingProductInfoDic에 셋팅
            billingService.getProductInfo(this.GetAllProductId(), (bool isSuccess, BillingProductInfoList result, Error error) =>
                {
                    Extension.PokoLog.Log("======BillingService GetProductInfo callback: isSuccess: " + isSuccess);

                    if (isSuccess == false)
                    {
                        Extension.PokoLog.Log("\tError[" + error.getCode() + "]: " + error.getMessage());
                        DelegateHelper.SafeCall(onComplete, isSuccess);
                        return;
                    }

                    List<BillingProductInfo> productList = ConvertBillingProductInfo(result.getProductList());

                    int count = productList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (billingProductInfoDic.ContainsKey(productList[i].productId))
                        {
                            Debug.Log($"ProductInfo Replaced ({productList[i].productId}): {billingProductInfoDic[productList[i].productId].price},{billingProductInfoDic[productList[i].productId].displayPrice}-> {productList[i].price},{productList[i].displayPrice} ");
                            billingProductInfoDic[productList[i].productId] = productList[i];
                        }
                        else
                        {
                            Debug.Log($"ProductInfo Added({productList[i].productId}): {productList[i].price},{productList[i].displayPrice} ");
                            billingProductInfoDic.Add(productList[i].productId, productList[i]);
                        }
                    }

                    DelegateHelper.SafeCall(onComplete, isSuccess);
                });
        }
        
        /// <summary>
        /// 상품 구입
        /// </summary>
        public void Purchase(string productID, Action<bool, string> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, string.Empty);
            return;
#endif
            //주문 중이라면 실행하지 않음
            if (this._billingHandler != null)
            {
                DelegateHelper.SafeCall(onComplete, false, string.Empty);
                return;
            }

            this._billingHandler = onComplete;
            this._purchasedProductId = productID;

            //서버에서 주문id를 받아오고 콜백 호출
            ServerAPI.PrePurchase(productID, OnGetShopOrderID);
        }

        public void PurchaseSendUrl(string productID, Action<bool, string> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, string.Empty);
            return;
#endif
            //주문 중이라면 실행하지 않음
            if (_billingHandler != null)
            {
                DelegateHelper.SafeCall(onComplete, false, string.Empty);
                return;
            }

            _billingHandler     = onComplete;
            _purchasedProductId = productID;

            //서버에서 주문id를 받아오고 콜백 호출
            ServerAPI.PrePurchase(productID, OnGetShopOrderIdSendUrl);
        }
        
        /// <summary>
        /// 패키지 상품구입
        /// </summary>
        public void PurchasePackage(int packageID, string productID, Action<bool, string> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, string.Empty);
            return;
#endif
            //주문 중이라면 실행하지 않음
            if (this._billingHandler != null)
            {
                DelegateHelper.SafeCall(onComplete, false, string.Empty);
                return;
            }

            this._billingHandler = onComplete;
            this._purchasedProductId = productID;


            //서버에서 주문id를 받아오고 콜백 호출
            ServerAPI.PreBuyShopPackage(packageID, productID, OnGetShopOrderIDByPackageItem);
        }
        
        /// <summary>
        /// 스페셜 클로버 상품구입
        /// </summary>
        public void PurchaseSpecialInAppClover(int idx, string productCode, Action<bool, string> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, string.Empty);
            return;
#endif
            //주문 중이라면 실행하지 않음
            if (this._billingHandler != null)
            {
                DelegateHelper.SafeCall(onComplete, false, string.Empty);
                return;
            }

            this._billingHandler     = onComplete;
            this._purchasedProductId = productCode;

            //서버에서 주문id를 받아오고 콜백 호출
            ServerAPI.PreBuyInAppSpecialClover(idx, OnGetShopOrderIDByPackageItem);
        }
        
        /// <summary>
        /// 스페셜 쥬얼 상품구입
        /// </summary>
        public void PurchaseSpecialJewel(int idx, string productCode, Action<bool, string> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, string.Empty);
            return;
#endif
            //주문 중이라면 실행하지 않음
            if (this._billingHandler != null)
            {
                DelegateHelper.SafeCall(onComplete, false, string.Empty);
                return;
            }

            this._billingHandler = onComplete;
            this._purchasedProductId = productCode;

            //서버에서 주문id를 받아오고 콜백 호출
            ServerAPI.PreBuySpecialJewel(idx, OnGetShopOrderIDByPackageItem);
        }

        /// <summary>
        /// Trident.BillingProductInfo >> ServiceSDK.BillingProductInfo 타입 변환
        /// </summary>
        private BillingProductInfo ConvertBillingProductInfo(Trident.BillingProductInfo tridentInfo)
        {
            if (tridentInfo == null)
            {
                return null;
            }
            
            var product = new BillingProductInfo
            {
                currency      = tridentInfo.getCurrency(),
                displayPrice  = tridentInfo.getDisplayPrice(),
                price         = tridentInfo.getPrice(),
                productId     = tridentInfo.getProductId(),
                storeLocation = tridentInfo.getStoreLocation()
            };

            return product;
        }

        /// <summary>
        /// SDK에서 전달받은 상품 정보 리스트를 ServiceSDK.BillingProductInfo 리스트로 변환하여 리턴
        /// </summary>
        private List<BillingProductInfo> ConvertBillingProductInfo(BillingProductInfoVector tridentInfoList)
        {
            if (tridentInfoList == null)
            {
                return null;
            }
            
            List<BillingProductInfo> productInfoList = new List<BillingProductInfo>();
            for (int i = 0; i < tridentInfoList.Count; i++)
            {
                BillingProductInfo info = ConvertBillingProductInfo(tridentInfoList[i]);
                if (info != null)
                {
                    productInfoList.Add(info);
                }
            }

            return productInfoList;
        }
        
        /// <summary>
        /// 게임서버에서 상품주문아이디(OrderID) 발급 받아온 뒤 호출되는 함수 (Purchase)
        /// </summary>
        private void OnGetShopOrderID(Protocol.UserPrePurchaseResp resp)
        {
            if (string.IsNullOrEmpty(resp.orderId))
            {
                Extension.PokoLog.Log("============OrderId is null");
                Action<bool, string> handler = this._billingHandler;
                this._billingHandler = null;
                DelegateHelper.SafeCall(handler, false, string.Empty);
                return;
            }
            
            this.DoPurchase(this._purchasedProductId, resp.orderId);
        }
        
        private void OnGetShopOrderIdSendUrl(Protocol.UserPrePurchaseResp resp)
        {
            if (string.IsNullOrEmpty(resp.orderId))
            {
                Extension.PokoLog.Log("============OrderId is null");
                var handler = _billingHandler;
                _billingHandler = null;
                DelegateHelper.SafeCall(handler, false, string.Empty);
                return;
            }
            
            DoPurchase(_purchasedProductId, resp.orderId, NetworkSettings.Instance.GetPurchaseCallback());
        }

        /// <summary>
        /// 게임서버에서 상품주문아이디(OrderID) 발급 받아온 뒤 호출되는 함수 (PurchasePackage, PurchaseSpecialJewel)
        /// </summary>
        private void OnGetShopOrderIDByPackageItem(Protocol.UserPrePurchaseResp resp)
        {
            if (resp.IsSuccess)
            {
                DoPurchase(this._purchasedProductId, resp.orderId, NetworkSettings.Instance.GetPurchaseCallback());
            }
        }

        /// <summary>
        /// 상품 구입 진행
        /// </summary>
        private void DoPurchase(string productID, string shopOrderID, string billingServiceUrl = "")
        {
            //임시 (상품 데이터가 만들어지기 전까지 하드코딩. 데이터 관리 방법이 정해지면 그곳에서 받아와 쓰도록 작업해야함)
#if UNITY_IPHONE
            string billingCpId = "PKV_AS";
#else
            string billingCpId = "PKV_GG";
#endif
            string billingReturnParam = shopOrderID;
            string billingComment = string.Empty;

            BillingService billingService = ServiceManager.getInstance().getService<BillingService>();

            if (billingService == null)
            {
                Extension.PokoLog.Log("============Billing Service Instance is null");
                Action<bool, string> handler = this._billingHandler;
                this._billingHandler = null;
                DelegateHelper.SafeCall(handler, false, string.Empty);
                return;
            }

            billingService.doPurchase(productID, billingCpId, billingServiceUrl,  (bool isSuccess, string result, string returnParam, Error error) =>
                {
                    Extension.PokoLog.Log("======BillingService DoPurchase callback: isSuccess: " + isSuccess + " cb:" + billingServiceUrl);
                    if (result != null)
                    {
                        Extension.PokoLog.Log("\tresult: " + result);
                    }

                    if (isSuccess == false)
                    {
                        Extension.PokoLog.Log("\tError[" + error.getCode() + "]: " + error.getMessage());
                    }

                    Extension.PokoLog.Log("\treturnParam: " + returnParam);
                    Action<bool, string> handler = this._billingHandler;
                    this._billingHandler = null;

                    DelegateHelper.SafeCall(handler, isSuccess, shopOrderID);
                },
                billingReturnParam, shopOrderID, billingComment);
            }
        
        #region 패스 상품 관련 데이터 반환
    
        /// <summary>
        /// 패스 상품 가격 반환 (string)
        /// </summary>
        public string GetPassPrice_String(int passKey)
        {
            if (ServerContents.Packages.ContainsKey(passKey) == false)
                return null;
    
            var data = ServerContents.Packages[passKey];
    
            string _productCode = string.Empty;
            
#if UNITY_IPHONE
            _productCode = data.sku_i;
#elif UNITY_ANDROID
            _productCode = data.sku_a;
#else
            _productCode = data.sku_a;
#endif
            
#if !UNITY_EDITOR
            if (billingProductInfoDic.ContainsKey(_productCode))
                return billingProductInfoDic[_productCode].displayPrice;
#endif
            return LanguageUtility.GetPrices(data.prices);
        }
        
        /// <summary>
        /// 패스 상품 가격 반환 (double)
        /// </summary>
        public double GetPassPrice_Double(int passKey)
        {
            var data = ServerContents.Packages[passKey];
            string _productCode = string.Empty;
            
#if UNITY_IPHONE
            _productCode = data.sku_i;
#elif UNITY_ANDROID
            _productCode = data.sku_a;
#else
            _productCode = data.sku_a;
#endif
            
#if !UNITY_EDITOR
            if (billingProductInfoDic.ContainsKey(_productCode))
                return double.Parse(billingProductInfoDic[_productCode].price, System.Globalization.CultureInfo.InvariantCulture);
#endif
            return LanguageUtility.GetPrice(data.prices);
        }
        
        /// <summary>
        /// 패스 상품 재화 타입 반환 (ex. KRW, JPY, TWD)
        /// </summary>
        public string GetPassCurrency(int passKey)
        {
            if (ServerContents.Packages.ContainsKey(passKey) == false)
                return null;
            
            var data = ServerContents.Packages[passKey];
            string _productCode = string.Empty;
            
#if UNITY_IPHONE
            _productCode = data.sku_i;
#elif UNITY_ANDROID
            _productCode = data.sku_a;
#else
            _productCode = data.sku_a;
#endif
            
#if !UNITY_EDITOR
            if (billingProductInfoDic.ContainsKey(_productCode))
                return billingProductInfoDic[_productCode].currency;
#endif
            return string.Empty;
        }
        #endregion
    }
}