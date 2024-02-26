using System;
using System.Linq;
using UnityEngine;
using ServiceSDK;

public class UIItemShopDiamondNormal : UIItemShopDiamondBase
{
    public UILabel[] _bonus;

    public GameObject _saleIcon;
    public UILabel    _originCountSale;
    public UILabel[]  _bonusSale;
    public UILabel    _bonusCountSale;
    public UILabel[]  _allCountSale;

    public UISprite _bestIcon;
    public UILabel  _bestLabel;

    public GameObject _superExpensiveBgObj;

    public override void InitItemDia(int index)
    {
        itemIndex   = index;
        productCode = GetProductCode(index);

        SetDiaIconTexture(index);
        SetDisplayPriceLabel();
        SetDiaOriginCountLabel();

        SettingBestBubbleUI(GetBestType(index));

        if (IsNotSailType(index))
        {
            SettingDefault(GetOriginDiaCount(), GetBonusDiaCount());
        }
        else
        {
            SettingSale(GetOriginDiaCount(), GetBonusDiaCount());
        }
    }

    protected override CdnShopJewel.JewelItemBase GetJewelData(int index) => ServerContents.JewelShop.Normal.ToList().Find((item) => item.idx == index);

    protected override void RequestPurchase(Action<bool, string> onComplete) => ServiceSDKManager.instance.Purchase(productCode, onComplete);

    protected override void RequestBuyDiaShopItem() => ServerAPI.PostPurchase(productCode, PurchaseComplete);

    protected override GrowthyCustomLog_Money.Code_L_MRSN GetGrowthy_MSRN() => GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP;

    protected override string GetAllCount()
    {
        if (_allCount[0].gameObject.activeInHierarchy == true)
        {
            return _allCount[0].text;
        }
        else
        {
            return _allCountSale[0].text;
        }
    }

    /// <summary>
    /// 추천 상품 타입 리턴 (1: 가장 인기, 2: 최고 혜택, 3: 추천!)
    /// </summary>
    private int GetBestType(int index)
    {
        var jewelData = ServerContents.JewelShop.Normal.ToList().Find((item) => item.idx == index);
        return jewelData.best;
    }

    /// <summary>
    /// 세일이 아닌지 여부 리턴
    /// </summary>
    private bool IsNotSailType(int index)
    {
        var jewelData = ServerContents.JewelShop.Normal.ToList().Find((item) => item.idx == index);
        return jewelData.saleLevel == 0;
    }

    /// <summary>
    /// 초고가 상품 UI 설정
    /// </summary>
    public void SettingSuperExpensiveItem(bool isActive)
    {
        if (isActive)
        {
            _superExpensiveBgObj.SetActive(true);
            _originCount.enabled  = false;
            _originCountSale.text = _originCount.text;
            _originCountSale.gameObject.SetActive(true);

            for (var i = 0; i < _bonus.Length; i++)
            {
                _bonus[i].gameObject.SetActive(false);
                _bonusSale[i].gameObject.SetActive(true);
            }
        }
        else
        {
            _superExpensiveBgObj.SetActive(false);
            _originCount.enabled = true;
            _originCountSale.gameObject.SetActive(false);

            for (var i = 0; i < _bonus.Length; i++)
            {
                _bonus[i].gameObject.SetActive(true);
                _bonusSale[i].gameObject.SetActive(false);
            }
        }
    }

    private void SettingDefault(int origin, int bonus)
    {
        _saleIcon.SetActive(false);

        _allCount.SetText((origin + bonus).ToString());
        _bonusCount.text = bonus.ToString();
    }

    private void SettingSale(int origin, int bonus)
    {
        _allCount[0].gameObject.SetActive(false);
        _bonusCount.gameObject.SetActive(false);

        _saleIcon.SetActive(true);
        _allCountSale[0].gameObject.SetActive(true);
        _bonusCountSale.gameObject.SetActive(true);

        _allCountSale.SetText((origin + bonus).ToString());
        _bonusCountSale.text = bonus.ToString();
    }

    private void SettingBestBubbleUI(int type)
    {
        if (type == 1)
        {
            SettingBubbleUI("icon_sale_05", "p_dia_5", -3.5f, new Color(192f / 255f, 21f / 255f, 27f / 255f));
        }
        else if (type == 2)
        {
            SettingBubbleUI("icon_sale_05", "p_dia_6", -3.5f, new Color(192f / 255f, 21f / 255f, 27f / 255f));
        }
        else if (type == 3)
        {
            SettingBubbleUI("icon_sale_06", "p_dia_7", LanguageUtility.SystemLanguage == SystemLanguage.Japanese ? -0.5f : -3.5f, new Color(207f / 255f, 163f / 255f, 46f / 255f));
        }
        else
        {
            _bestIcon.gameObject.SetActive(false);
        }
    }

    private void SettingBubbleUI(string spriteName, string text, float xPos, Color outLineColor)
    {
        _bestIcon.spriteName               = spriteName;
        _bestLabel.text                    = Global._instance.GetString(text);
        _bestLabel.transform.localPosition = new Vector2(xPos, _bestLabel.transform.localPosition.y);
        _bestLabel.effectColor             = outLineColor;
    }
}