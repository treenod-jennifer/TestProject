using System.Collections.Generic;
using UnityEngine;

public class UIItemShopDiamond : MonoBehaviour
{
    public static UIItemShopDiamond _instance;
    
    [SerializeField] private GameObject obj_ItemAD;
    [SerializeField] private GameObject _objItemDiashop;
    [SerializeField] private GameObject _objItemSpecialShop;
    [SerializeField] private GameObject _objItemNRU;

    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;

    [SerializeField] private UISprite sprBgImage;

    [SerializeField] private UISprite sprite_ScrollBarBG;
    [SerializeField] private UISprite sprite_ScrollBar;

    [SerializeField] private GameObject objScrollEnd;
    public UIPanel scrollView;
    
    private UIItemShopADItem            item_AD;
    private Dictionary<int, GameObject> _normalItemDic = new Dictionary<int, GameObject>();
        
    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    private string TermsOfUse
    {
        get
        {
            string value = Global._instance.GetString("p_dia_4");

            if (string.IsNullOrEmpty(value))
                objTermsOfUse.SetActive(false);
            else
                objTermsOfUse.SetActive(true);

            return value;
        }
    }

    private string Precautions
    {
        get
        {
            string value = Global._instance.GetString("p_dia_3");

            if (string.IsNullOrEmpty(value))
                objPrecautions.SetActive(false);
            else
                objPrecautions.SetActive(true);

            return value;
        }
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }
    
    private void Start()
    {
        _instance = this;
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        if (string.IsNullOrEmpty(labelTermsOfUse.text) && string.IsNullOrEmpty(labelPrecautions.text))
        {
            sprBgImage.height = 900;
            objScrollEnd.transform.localPosition = new Vector3(0f, -862.5f, 0f);
        }
        else
        {
            sprBgImage.height = 853;
            objScrollEnd.transform.localPosition = new Vector3(0f, -815f, 0f);
        }
            
        //스크롤 세팅
        SetScrollView();

        //뎁스 설정
        SettingDepth();
    }

    private void SetScrollView()
    {
        // 정렬 1 : 스페셜 상품 (미션 6스테이지 이후 노출)
        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            foreach (var data in ServerContents.JewelShop.Special)
            {
                int purchaseCount = ServerRepos.UserSpecialShop.Jewel.Find(x => x.Idx == data.idx).purchase_count;
                if (purchaseCount < data.purchase_limit ||
                    data.purchase_limit == 0) // 구매 카운트가 구매 제한보다 낮거나, 구매 제한값이 0(무제한)일 경우 아이템 생성
                {
                    if (Global.LeftTime(data.end_ts) > 0)
                    {
                        var item_Shop = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, _objItemSpecialShop).GetComponent<UIItemShopSpecialItem>();
                        item_Shop.Init_Jewel(data);
                    }
                }
            }
        }

        // 정렬 2 : 일반 상품 (초고가 상품)
        var superExpensiveProducts = false;
        if (ServerContents.JewelShop.Normal.Count > 0)
        {
            var item = ServerContents.JewelShop.Normal[0];
            superExpensiveProducts = (item.saleLevel == 1);

            if (superExpensiveProducts)
            {
                var diamondItem = scrollView.GetComponentInChildren<UIGrid>().transform.AddChild(_objItemDiashop).GetComponent<UIItemShopDiamondNormal>();

                diamondItem.InitItemDia(item.idx);
                diamondItem.SettingSuperExpensiveItem(item.saleLevel == 1);
            }
        }

        // 정렬 3 : 광고 상품
        ADCheck();

        // 정렬 4 : 일반 상품 (일반 상품)
        for (var index = superExpensiveProducts ? 1 : 0; index < ServerContents.JewelShop.Normal.Count; index++)
        {
            var item        = ServerContents.JewelShop.Normal[index];
            var diamondItem = scrollView.GetComponentInChildren<UIGrid>().transform.AddChild(_objItemDiashop).GetComponent<UIItemShopDiamondNormal>();

            diamondItem.InitItemDia(item.idx);
            diamondItem.SettingSuperExpensiveItem(false);

            if (HasNruItem(item.idx) == false)
            {
                continue;
            }

            var nruItem = scrollView.GetComponentInChildren<UIGrid>().transform.AddChild(_objItemNRU).GetComponent<UIItemShopDiamondNru>();
            nruItem.InitItemDia(item.idx);

            diamondItem.gameObject.SetActive(false); // 일반 상품은 노출하지 않음
            _normalItemDic.Add(item.idx, diamondItem.gameObject);
        }
        
        scrollView.GetComponentInChildren<UIGrid>().Reposition();

        scrollView.SetAnchor(sprBgImage.gameObject, 0, 10, 20, -10);

        sprite_ScrollBarBG.height = (int) sprBgImage.height - 20;
        sprite_ScrollBar.height = (int) sprBgImage.height - 15;
        sprite_ScrollBar.GetComponent<UIScrollBar>().value = 0;
        scrollView.GetComponent<UIScrollView>().ResetPosition();
    }

    private bool HasNruItem(int index)
    {
        if (ServerContents.JewelShop.Nru == null)
        {
            return false;
        }

        var nruItemData = ServerContents.JewelShop.Nru.Find(x => x.idx == index);
        if (nruItemData == null)
        {
            return false;
        }

        if (Global.LeftTime(nruItemData.end_ts) <= 0)
        {
            return false;
        }
        
        if (ServerRepos.UserShopNru == null || ServerRepos.UserShopNru.jewel == null)
        {
            return false;
        }

        foreach (var item in ServerRepos.UserShopNru.jewel)
        {
            if (item.idx == index && item.purchaseCount == 0)
            {
                return true;
            }
        }

        return false;
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

    public void ActiveNormalItem(int index)
    {
        if (_normalItemDic.ContainsKey(index))
        {
            _normalItemDic[index].SetActive(true);
            _normalItemDic.Remove(index);
            
            scrollView.GetComponentInChildren<UIGrid>().Reposition();
        }
    }

    public void ADCheck()
    {
        if (AdManager.ADCheck(AdManager.AdType.AD_15))
        {
            if (item_AD == null)
            {
                item_AD = NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().transform, obj_ItemAD)
                    .GetComponent<UIItemShopADItem>();
            }
            item_AD.Init(UIItemShopADItem.ADItemType.Diamond);
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
        foreach (var item in ServerContents.JewelShop.Special)
        {
            #if UNITY_IPHONE
            string itemKey = item.sku_i + "_" + item.idx;
            #elif UNITY_ANDROID
            string itemKey = item.sku_a + "_" + item.idx;
            #else
            string itemKey = item.sku_a + "_" + item.idx;
            #endif
            str += (itemKey + ",");
        }


        if (PlayerPrefs.GetString("ShopJewelSpecialItemList") != str)
            PlayerPrefs.SetString("ShopJewelSpecialItemList", str);
        
        ManagerUI._instance.newIcon_jewel.SetActive(false);
    }
    
    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
}
