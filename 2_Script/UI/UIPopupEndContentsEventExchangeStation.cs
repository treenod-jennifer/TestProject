using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriseItemComparer_EndContentsEvent : IComparer<CdnEndContentsShop>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(CdnEndContentsShop a, CdnEndContentsShop b)
    {
        if (a.endTs > 0 && b.endTs == 0)
            return -1;
        else if (a.endTs == 0 && b.endTs > 0)
            return 1;
        else if (a.buyLimit > b.buyLimit)        //a만 1회 구매상품일 떄
            return -1;
        else if (a.buyLimit < b.buyLimit)   //b만 1회 구매상품일 떄
            return 1;
        else
        {
            if (a.goodsIndex < b.goodsIndex)
                return -1;
            else if (a.goodsIndex > b.goodsIndex)
                return 1;
            else
                return 0;
        }
    }
}

public class UIPopupEndContentsEventExchangeStation : UIPopupBase
{
    public static UIPopupEndContentsEventExchangeStation _instance = null;

    [SerializeField] private UIPanel scrollView;
    [SerializeField] public UILabel labelPokoCoin;
    [SerializeField] public UILabel labelMaxPokoCoin;
    [SerializeField] private UIReuseGrid_EndContentsExchangeStation scroll;
    [SerializeField] private UISprite sprite_PokoCoin;
    
    [HideInInspector] public List<CdnEndContentsShop> _listEndContentsPriseItemData = new List<CdnEndContentsShop>();

    private const int priseItemArraySize = 3;
    private int myEndContents = 0;

    PriseItemComparer_EndContentsEvent priseItemComparer = new PriseItemComparer_EndContentsEvent();
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    public void InitPopup()
    {
        SetPriseItemData();
        myEndContents = ManagerEndContentsEvent.GetPokoCoin();
        labelPokoCoin.text = myEndContents.ToString();
        labelMaxPokoCoin.text = "/ " + ManagerEndContentsEvent.GetMaxPokoCoin();
        SetEndContentsColor(myEndContents);

        int depth = uiPanel.depth;
        int layer = uiPanel.sortingOrder;

        scrollView.depth = depth + 1;

        if (layer < 10) return;

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
    }

    void SetPriseItemData()
    {
        List<CdnEndContentsShop> listPriseItem = new List<CdnEndContentsShop>();

        foreach (var item in ServerContents.EndContentsShop.Values)
        {
            if ( (Global.LeftTime(item.endTs) < 0 &&item.endTs != 0) || ManagerEndContentsEvent.instance.CanBuy(item.goodsIndex) == ManagerEndContentsEvent.BuyError.BUY_LIMIT) continue;
            listPriseItem.Add(item);
        }

        _listEndContentsPriseItemData.AddRange(listPriseItem);
        _listEndContentsPriseItemData.Sort(priseItemComparer);
    }

    // endContents이 3000이상개 라면 색이 바뀜
    public void SetEndContentsColor(int currentCoin)
    {
        sprite_PokoCoin.atlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.UIAtlas;
        sprite_PokoCoin.spriteName = "pokoCoin";
        if (currentCoin > ManagerEndContentsEvent.GetMaxPokoCoin())
            labelPokoCoin.color = new Color32(255, 109, 38, 255);
        else
            labelPokoCoin.color = new Color32(31, 107, 154, 255);
    }

    public CdnEndContentsShop[] GetEndContentsPriseItemData(int index)
    {
        int priseItemArrayCount = Mathf.CeilToInt((float)_listEndContentsPriseItemData.Count / priseItemArraySize);
        int firstPriseItemIndex = index * priseItemArraySize;

        CdnEndContentsShop[] _arrayPriseItemData = new CdnEndContentsShop[3];

        for(int i = 0; i < priseItemArraySize; i++)
        {
            if (firstPriseItemIndex + i >= _listEndContentsPriseItemData.Count) break;

            _arrayPriseItemData[i] = _listEndContentsPriseItemData[firstPriseItemIndex + i];

        }

        return _arrayPriseItemData;
    }

    //스크롤 최대 수
    public int GetPriseItemCount()
    {
        int _nCount = Mathf.CeilToInt((float)_listEndContentsPriseItemData.Count / priseItemArraySize);

        return _nCount;
    }

    public void ResetPriceItemList()
    {
        if (_listEndContentsPriseItemData.Count > 0)
            _listEndContentsPriseItemData.Clear();

        SetPriseItemData();
        scroll.ScrollReset();
    }

}
