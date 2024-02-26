using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPopupShop : UIPopupBase
{
    public enum ShopType
    {
        Clover,
        Wing,
        Coin,
        Diamond
    }

    [Header("Tab")]
    [SerializeField] private UIItemTab tabClover;
    [SerializeField] private UIItemTab tabWing;
    [SerializeField] private UIItemTab tabCoin;
    [SerializeField] private UIItemTab tabDiamond;

    [Header("Sale Tag")]
    [SerializeField] private GameObject tagSaleClover;
    [SerializeField] private GameObject tagSaleWing;
    [SerializeField] private GameObject tagSaleCoin;
    [SerializeField] private GameObject tagSaleDiamond;

    [SerializeField] private UILabel label_Title;
    [SerializeField] private UILabel label_Title_S;
    
    public static UIPopupShop _instance;

    public static bool IsCloverSale
    {
        get
        {
            return ServerContents.CloverShop.Normal != null && ServerContents.CloverShop.Normal.Count(x => x.saleLevel > 0 ) > 0;
        }
    }
    public static bool IsWingSale
    {
        get
        {
            return ServerContents.WingShop.Normal != null && ServerContents.WingShop.Normal.Count(x => x.saleLevel > 0 ) > 0;
        }
    }
    public static bool IsCoinSale
    {
        get
        {
            return ServerContents.CoinShop.Normal != null && ServerContents.CoinShop.Normal.Count(x => x.saleLevel > 0 ) > 0;
        }
    }
    public static bool IsDiamondSale
    {
        get
        {
            return ServerContents.JewelShop.Normal != null && ServerContents.JewelShop.Normal.Count(x => x.saleLevel > 0 ) > 0;
        }
    }

    public static bool IsCloverActive
    {
        get
        {
            //인게임씬 이면서 탐험모드이면 안열림
            if (SceneManager.GetActiveScene().name == "InGame")
            {
                if(GameManager.gameMode == GameMode.ADVENTURE)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public static bool IsWingActive
    {
        get
        {
            //탐험모드 비활성화 시 상점 안열림
            if (!ManagerAdventure.CheckStartable())
            {
                return false;
            }

            //인게임씬 이면서 탐험모드가 아니라면 안열림
            if (SceneManager.GetActiveScene().name == "InGame")
            {
                if(GameManager.gameMode != GameMode.ADVENTURE)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public static bool IsCoinActive
    {
        get
        {
            if (SceneManager.GetActiveScene().name == "InGame")
            {
                if (UIPopupContinue._instance != null && UIPopupContinue._instance.pushContinue == true)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public static bool IsDiamondActive
    {
        get
        {
            if (SceneManager.GetActiveScene().name == "InGame")
            {
                if (UIPopupContinue._instance != null && UIPopupContinue._instance.pushContinue == true) 
                { 
                    return false; 
                }
            }

            return true;
        }
    }

    private bool isTopUIAdventureMode;

    private void Awake()
    {
        _instance = this;

        label_Title.text = Global._instance.GetString("p_shop_1");
        label_Title_S.text = Global._instance.GetString("p_shop_1");

        isTopUIAdventureMode = ManagerUI._instance.IsTopUIAdventureMode;

        tagSaleClover.SetActive(IsCloverSale);
        tagSaleWing.SetActive(IsWingSale);
        tagSaleCoin.SetActive(IsCoinSale);
        tagSaleDiamond.SetActive(IsDiamondSale);

        tabClover.SetActive(IsCloverActive);
        tabWing.SetActive(IsWingActive);
        tabCoin.SetActive(IsCoinActive);
        tabDiamond.SetActive(IsDiamondActive);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_instance == this)
        {
            _instance = null;
        }

        ManagerUI._instance.IsTopUIAdventureMode = isTopUIAdventureMode;
    }

    public void Init(ShopType shopType)
    {
        SelectTap(shopType);
    }

    public void SelectTap(ShopType shopType)
    {
        switch (shopType)
        {
            case ShopType.Clover:
                tabClover.On();
                break;
            case ShopType.Wing:
                tabWing.On();
                break;
            case ShopType.Coin:
                tabCoin.On();
                break;
            case ShopType.Diamond:
                tabDiamond.On();
                break;
            default:
                break;
        }
    }
}