using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriseItemComparer : IComparer<CdnWorldRankShop>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(CdnWorldRankShop a, CdnWorldRankShop b)
    {
        if (a.endTs > 0 && b.endTs == 0)
            return -1;
        else if (a.endTs == 0 && b.endTs > 0)
            return 1;
        else
        {
            if (a.buyLimit > b.buyLimit)        //a만 1회 구매상품일 떄
                return -1;
            else if (a.buyLimit < b.buyLimit)   //b만 1회 구매상품일 떄
                return 1;
            else                                //a와b가 둘다 1회 구매상품이거나 1회 구매상품이 아닐 때는 그대로
                return 0;
        }
    }
}

public class UIPopUpWorldRankExchangeStation : UIPopupBase
{
    public static UIPopUpWorldRankExchangeStation _instance = null;

    [SerializeField] private UIPanel scrollView;
    [SerializeField] public UILabel labelRankToken;
    [SerializeField] private UIReuseGrid_WorldRankingExchangeStation scroll;
    [SerializeField] private UIItemLanpageButton lanpageButton;
    [SerializeField] private GameObject item_AD;
    
    [HideInInspector] public List<CdnWorldRankShop> _listWorldRankPriseItemData = new List<CdnWorldRankShop>();

    private const int priseItemArraySize = 3;
    private int myRankToken = 0;
    public const int maxRankToken = 3000;

    PriseItemComparer priseItemComparer = new PriseItemComparer();
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        //LAN 페이지 설정
        lanpageButton.On("LGPKV_world_shop", Global._instance.GetString("p_leaf_4"));
    }

    private int panelDepth;
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        panelDepth = depth;

        SetPriseItemData();
        myRankToken = ManagerWorldRanking.userData.GetRankToken();
        labelRankToken.text = myRankToken.ToString();
        SetRankTokenColor(myRankToken);
        AdBubbleSetting();
    }

    public void InitScrollDepth()
    {
        int layer = uiPanel.sortingOrder;

        scrollView.depth = panelDepth + 1;

        if (layer < 10) return;

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
    }

    void SetPriseItemData()
    {
        List<CdnWorldRankShop> listLimitItem = new List<CdnWorldRankShop>();
        List<CdnWorldRankShop> listUnLimitItem = new List<CdnWorldRankShop>();

        List<CdnWorldRankShop> listPriseItem = new List<CdnWorldRankShop>();

        for (int i = 0; i < ServerContents.WorldRankShop.Count; i++)
        {
            if ( (Global.LeftTime(ServerContents.WorldRankShop[i].endTs) < 0 && ServerContents.WorldRankShop[i].endTs != 0) ||
                ManagerWorldRanking.userData.CanBuy(ServerContents.WorldRankShop[i].goodsId) == ManagerWorldRanking.UserData.BuyError.BUY_LIMIT) continue;

            listPriseItem.Add(ServerContents.WorldRankShop[i]);
        }

        _listWorldRankPriseItemData.AddRange(listPriseItem);
        _listWorldRankPriseItemData.Sort(priseItemComparer);
    }

    // rankToken이 3000이상개 라면 색이 바뀜
    public void SetRankTokenColor(int currentToken)
    {
        if (currentToken > maxRankToken)
            labelRankToken.color = new Color32(255, 109, 38, 255);
        else
            labelRankToken.color = new Color32(31, 107, 154, 255);
    }

    public CdnWorldRankShop[] GetWorldRankPriseItemData(int index)
    {
        int priseItemArrayCount = Mathf.CeilToInt((float)_listWorldRankPriseItemData.Count / priseItemArraySize);
        int firstPriseItemIndex = index * priseItemArraySize;

        CdnWorldRankShop[] _arrayPriseItemData = new CdnWorldRankShop[3];

        for(int i = 0; i < priseItemArraySize; i++)
        {
            if (firstPriseItemIndex + i >= _listWorldRankPriseItemData.Count) break;

            _arrayPriseItemData[i] = _listWorldRankPriseItemData[firstPriseItemIndex + i];

        }

        return _arrayPriseItemData;
    }

    //스크롤 최대 수
    public int GetPriseItemCount()
    {
        int _nCount = Mathf.CeilToInt((float)_listWorldRankPriseItemData.Count / priseItemArraySize);

        return _nCount;
    }

    public void ResetPriceItemList()
    {
        if (_listWorldRankPriseItemData.Count > 0)
            _listWorldRankPriseItemData.Clear();

        SetPriseItemData();
        scroll.ScrollReset();
    }
    
    public void OnClickBtnAD()
    {
        ManagerUI._instance.OpenPopup<UIPopupADView>
        (
            (popup) =>
            {
                popup.SetWorldRankExchangeStation(ResAdRewardComplete, AdManager.AdType.AD_19, ServerContents.AdInfos[(int)AdManager.AdType.AD_19].rewards);
            }
        );
    }

    private void AdBubbleSetting()
    {
        bool active = AdManager.ADCheck(AdManager.AdType.AD_19) &&
        ManagerWorldRanking.GetEventOpenCondition() == ManagerWorldRanking.EventOpenCondition.MEET;
        
        item_AD.SetActive(active);
        if (UIPopUpWorldRank._instance != null)
            UIPopUpWorldRank._instance.SetAdButton(active);
        if (UIPopUpWorldRankHallOfFame._instance != null)
            UIPopUpWorldRankHallOfFame._instance.SetAdButton(active);
    }
    
    private void ResAdRewardComplete()
    {
        myRankToken = ManagerWorldRanking.userData.GetRankToken();
        labelRankToken.text = myRankToken.ToString();
        SetRankTokenColor(myRankToken);
        AdBubbleSetting();
    }
}
