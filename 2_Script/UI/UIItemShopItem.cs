using UnityEngine;

public class UIItemShopItem : MonoBehaviour
{
    private enum CurrencyType
    {
        None,
        clover,
        wing
    }
    
    private class ItemInfo
    {
        public CurrencyType currencyType = CurrencyType.None;
        public int idx;
        public int item_type;   // 0 : 개수, 1 : 시간
        public int pItem;
        public int fItem;
        public int saleLevel;
        public int best;
        public int price_type;  // 2 : 코인, 3 : 다이아
        public int price_value;
    }
    
    private ItemInfo itemInfo = new ItemInfo();

    [SerializeField] private UIUrlTexture texture_Item;
    [SerializeField] private UISprite sprite_PriceItem;
    [SerializeField] private UISprite sprite_Best;
    [SerializeField] private UILabel label_Best;
    [SerializeField] private UILabel label_Main;
    [SerializeField] private UILabel label_Main_S;
    [SerializeField] private UILabel label_Info;
    [SerializeField] private UILabel label_Count;
    [SerializeField] private UILabel label_Count_S;
    [SerializeField] private UILabel label_Price;
    [SerializeField] private UILabel label_Price_S;
    [SerializeField] private GameObject obj_Sale;
    [SerializeField] private Color color_Wing;
    [SerializeField] private Color color_Clover;

    public UIPokoButton purchaseButton;
    
    public void Init_Clover(CdnShopClover.NormalItem data)
    {
        itemInfo.idx = data.idx;
        itemInfo.currencyType = CurrencyType.clover;
        itemInfo.item_type = data.clover_type;
        itemInfo.pItem = data.pClover;
        itemInfo.fItem = data.fClover;
        itemInfo.saleLevel = data.saleLevel;
        itemInfo.best = data.best;
        itemInfo.price_type = 3;
        itemInfo.price_value = data.price;
        Init_ItemUI();
    }
    
    public void Init_Wing(CdnShopWing.NormalItem data)
    {
        itemInfo.idx = data.idx;
        itemInfo.currencyType = CurrencyType.wing;
        itemInfo.item_type = data.wing_type;
        itemInfo.pItem = data.pWing;
        itemInfo.fItem = data.fWing;
        itemInfo.saleLevel = data.saleLevel;
        itemInfo.best = data.best;
        itemInfo.price_type = data.price_type;
        itemInfo.price_value = data.price_value;
        Init_ItemUI();
    }

    private void Init_ItemUI()
    {
        string priceType = itemInfo.price_type == 2 ? "coin" : "gem";
        string timeFree = itemInfo.item_type == 1 ? "time" : "";
        string mainTextKey = Global._instance.GetString(itemInfo.currencyType == CurrencyType.clover ? "p_cl_4" : $"p_wi_{itemInfo.price_type}");
        int count = itemInfo.pItem + itemInfo.fItem;
        
        // 시간 아이템일 시
        if (itemInfo.item_type == 1)
        {
            count /= 60;
            mainTextKey = Global._instance.GetString(itemInfo.currencyType == CurrencyType.clover ? "p_cl_5" : "p_wi_5");
            mainTextKey = mainTextKey.Replace("[n]", count.ToString());
            label_Main.transform.localPosition = new Vector3(label_Main.transform.localPosition.x, 10f, 0);
            label_Info.gameObject.SetActive(true);
            label_Info.text = Global._instance.GetString(itemInfo.currencyType == CurrencyType.clover ? "p_cl_6" : "p_wi_6");
        }
        else
        {
            label_Main.transform.localPosition = new Vector3(label_Main.transform.localPosition.x, -5f, 0);
            label_Info.gameObject.SetActive(false);
            label_Count.effectColor = itemInfo.currencyType == CurrencyType.clover ? color_Clover : color_Wing;
            label_Count_S.color = itemInfo.currencyType == CurrencyType.clover ? color_Clover : color_Wing;
            label_Count_S.effectColor = itemInfo.currencyType == CurrencyType.clover ? color_Clover : color_Wing;
            label_Count.text = count > 0 ? count.ToString() : "";
            label_Count_S.text = count > 0 ? count.ToString() : "";
        }

        // UI 세팅
        label_Main.text = mainTextKey;
        label_Main_S.text = mainTextKey;
        texture_Item.LoadCDN(Global.gameImageDirectory, $"IconShopItem/{itemInfo.currencyType.ToString()}", $"shop_icon_{timeFree}{itemInfo.currencyType.ToString()}");
        sprite_PriceItem.spriteName = $"icon_{priceType}";
        label_Count.gameObject.SetActive(itemInfo.item_type == 0);
        label_Count_S.gameObject.SetActive(itemInfo.item_type == 0);
        label_Price.text = itemInfo.price_value.ToString();
        label_Price_S.text = itemInfo.price_value.ToString();
        obj_Sale.SetActive(itemInfo.saleLevel > 0);
        SettingBubble(itemInfo.best);
        
        sprite_PriceItem.transform.localPosition = new Vector2(label_Price.transform.localPosition.x - 38f - (label_Price.text.Length * 10), sprite_PriceItem.transform.localPosition.y);
        texture_Item.MakePixelPerfect();
        sprite_PriceItem.MakePixelPerfect();
    }
    
    private void SettingBubble(int type)
    {
        if (type == 1)
            SettingBubbleUI("icon_sale_05", "p_dia_5", -3.5f, new Color(192f / 255f, 21f / 255f, 27f / 255f));
        else if (type == 2)
            SettingBubbleUI("icon_sale_05", "p_dia_6", -3.5f, new Color(192f / 255f, 21f / 255f, 27f / 255f));
        else if (type == 3)
            SettingBubbleUI("icon_sale_06", "p_dia_7", LanguageUtility.SystemLanguage == SystemLanguage.Japanese ? -0.5f : -3.5f, new Color(207f / 255f, 163f / 255f, 46f / 255f));
        else
            sprite_Best.gameObject.SetActive(false);
    }

    private void SettingBubbleUI(string spriteName, string text, float xPos, Color outLineColor)
    {
        sprite_Best.gameObject.SetActive(true);
        sprite_Best.spriteName = spriteName;
        label_Best.text = Global._instance.GetString(text);
        label_Best.transform.localPosition = new Vector2(xPos, label_Best.transform.localPosition.y);
        label_Best.effectColor = outLineColor;
    }

    private void OnClickBtnPurchase()
    {
        if (itemInfo.currencyType == CurrencyType.clover && UIItemShopClover._instance != null)
            UIItemShopClover._instance.OnClickBtnBuyClover(itemInfo.idx);
        else if (itemInfo.currencyType == CurrencyType.wing && UIItemShopWing._instance != null)
            UIItemShopWing._instance.OnClickBtnBuyWing(itemInfo.idx);
    }
}
