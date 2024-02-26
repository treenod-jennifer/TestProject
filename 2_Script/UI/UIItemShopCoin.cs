using System;
using UnityEngine;

public class UIItemShopCoin : MonoBehaviour
{
    public static UIItemShopCoin _instance;
    
    [SerializeField] private GameObject obj_ItemShop;
    [SerializeField] private GameObject obj_ItemSpecialShop;
    [SerializeField] private UIScrollBar sprite_ScrollBar;
    public UIPanel scrollView;

    private void Start()
    {
        _instance = this;
        // 정렬 1 : 스페셜 상품 (미션 6스테이지 이후 노출)
        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            foreach (var data in ServerContents.CoinShop.Special)
            {
                int purchaseCount = ServerRepos.UserSpecialShop.Coin.Find(x => x.Idx == data.idx).purchase_count;
                if (purchaseCount < data.purchase_limit ||
                    data.purchase_limit == 0) // 구매 카운트가 구매 제한보다 낮거나, 구매 제한값이 0(무제한)일 경우 아이템 생성
                {
                    if (Global.LeftTime(data.end_ts) > 0)
                    {
                        var item_Shop = NGUITools
                            .AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemSpecialShop)
                            .GetComponent<UIItemShopSpecialItem>();
                        item_Shop.Init_Coin(data);
                    }
                }
            }
        }
        // 정렬 2 : 일반 상품
        foreach (var data in ServerContents.CoinShop.Normal)
        {
            var item_Shop = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemShop)
                .GetComponent<UIItemShopCoinItem>();
            item_Shop.InitItemCoin(data);
        }
        
        sprite_ScrollBar.value = 0;
        scrollView.GetComponent<UIScrollView>().ResetPosition();
        
        SettingDepth();
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

    private void OnEnable()
    {
        NewCheck();
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.IsTopUIAdventureMode = false;
        }
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }

    private void NewCheck()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return;
        string str = "";
        foreach (var item in ServerContents.CoinShop.Special)
        {
            string itemKey = item.g_code + "_" + item.idx;
            str += (itemKey + ",");
        }

        if (PlayerPrefs.GetString("ShopCoinSpecialItemList") != str)
            PlayerPrefs.SetString("ShopCoinSpecialItemList", str);
        
        ManagerUI._instance.newIcon_coin.SetActive(false);
    }
}
