using System;
using System.Linq;
using ServiceSDK;
using UnityEngine;

public class UIItemShopDiamondNru : UIItemShopDiamondBase
{
    [SerializeField] private UILabel _endTime; // 판매 종료 기간

    public override void InitItemDia(int index)
    {
        itemIndex   = index;
        productCode = GetProductCode(index);

        SetDiaIconTexture(index);
        SetDisplayPriceLabel();
        SetDiaOriginCountLabel();
        SetDiaBonusCountLabel();
        
        _allCount.SetText((GetOriginDiaCount() + GetBonusDiaCount()).ToString());
        
        EndTsTimer.Run(_endTime, GetEndTs(index), Global.GetLeftTimeText_DDHHMMSS);
        
        onCompletePurchase = () =>
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            
            UIItemShopDiamond._instance.ActiveNormalItem(index);
        };
    }

    protected override string GetAllCount() => _allCount[0].text;

    protected override CdnShopJewel.JewelItemBase GetJewelData(int index) => ServerContents.JewelShop.Nru.ToList().Find((item) => item.idx == index);

    protected override void RequestPurchase(Action<bool, string> onComplete) => ServiceSDKManager.instance.PurchaseSendUrl(productCode, onComplete);

    protected override void RequestBuyDiaShopItem() => ServerAPI.BuyDiaShopNru(productCode, PurchaseComplete);

    protected override GrowthyCustomLog_Money.Code_L_MRSN GetGrowthy_MSRN() => GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_NRU_DIA_SHOP;

    private int GetEndTs(int index)
    {
        var jewelData = ServerContents.JewelShop.Nru.ToList().Find((item) => item.idx == index);
        return jewelData.end_ts;
    }
}