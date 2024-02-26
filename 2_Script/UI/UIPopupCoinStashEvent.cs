using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;

public class UIPopupCoinStashEvent : UIPopupBase
{
    [SerializeField] private UILabel labelEndTs;

    [SerializeField] private UILabel labelInfoMessage;

    [SerializeField] private UIUrlTexture texEventComplte;

    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private List<GameObject> objRewardPoints;
    [SerializeField] private GameObject purchaseLabelRoot;

    [SerializeField] private UILabel labelPurchase;
    [SerializeField] private UILabel labelPurchaseMax;
    [SerializeField] private List<UILabel> labelCurrentCoinValue;
    [SerializeField] private List<UILabel> labelMinCoinValue;
    [SerializeField] private List<UILabel> labelMaxCoinValue;
    [SerializeField] private List<UILabel> labelCoinStashPrice;

    [SerializeField] private float testFullCoin;
    [SerializeField] private float testPointCoin;
    
    public bool bAutoOpen = false;

    private void Start()
    {
        InitData();

        SetRewardPoint();

        labelEndTs.text = Global.GetTimeText_MMDDHHMM_Plus1(ServerContents.CoinStashEvent.end_ts);
    }

    private void InitData()
    {
        //다이아 이미지 앵커 관련 세팅을 위해 잠깐 비활성화
        labelCoinStashPrice[0].gameObject.SetActive(false);


        if( ServerContents.CoinStashEvent.maxBuyCount > 0 )
        {
            purchaseLabelRoot.SetActive(true);
            //구매 가능 횟수 세팅
            labelPurchase.text = (ServerContents.CoinStashEvent.maxBuyCount - ServerRepos.UserCoinStash.buyCount).ToString();
            labelPurchaseMax.text = "/" + ServerContents.CoinStashEvent.maxBuyCount.ToString() + ")";
        }
        else
        {
            purchaseLabelRoot.SetActive(false);
            labelPurchase.text = "";
            labelPurchaseMax.text = "";
        }
        

        //가격 표시
        for (int i = 0; i < labelCoinStashPrice.Count; i++)
        {
            labelCoinStashPrice[i].text = ServerContents.CoinStashEvent.price.ToString();
        }

        //가격 받아와 텍스트 적용 후 다시 활성화
        labelCoinStashPrice[0].gameObject.SetActive(true);




        for (int i = 0; i < labelCurrentCoinValue.Count; i++)
        {
            if(i == 0)
            {
                if(ServerRepos.UserCoinStash.storedCoin > ServerContents.CoinStashEvent.price * 100)
                    labelCurrentCoinValue[0].text = (ServerContents.CoinStashEvent.price * 100).ToString();
                else
                    labelCurrentCoinValue[0].text = ServerRepos.UserCoinStash.storedCoin.ToString();
            }
            else
            {
                if(ServerRepos.UserCoinStash.storedCoin - ServerContents.CoinStashEvent.price * 100 > 0)
                    labelCurrentCoinValue[1].text = $"+ Bonus {ManagerCoinStashEvent.GetBonusCoin(ServerRepos.UserCoinStash.storedCoin)}";
                else
                    labelCurrentCoinValue[1].text = $"+ Bonus {ManagerCoinStashEvent.GetBonusCoin(ServerRepos.UserCoinStash.storedCoin)}";
            }
        }

        for (int i = 0; i < labelMinCoinValue.Count; i++)
        {
            if (i == 0)
            {
                labelMinCoinValue[0].text = (ServerContents.CoinStashEvent.price * 100).ToString();
                labelMaxCoinValue[0].text = (ServerContents.CoinStashEvent.price * 100).ToString();
            }
            else
            {
                labelMinCoinValue[1].text = $"+ {ManagerCoinStashEvent.GetBonusCoin(ServerContents.CoinStashEvent.coinMin)}";
                labelMaxCoinValue[1].text = $"+ {ManagerCoinStashEvent.GetBonusCoin(ServerContents.CoinStashEvent.coinMax)}";
            }
        }

        labelInfoMessage.text = GetInfoMessage(ManagerCoinStashEvent.GetCoinStashState());

        string path = "";
        if (ManagerCoinStashEvent.GetCoinStashState() == ManagerCoinStashEvent.CoinStashState.NOT_BUY_COINLACK 
            || ManagerCoinStashEvent.GetCoinStashState() == ManagerCoinStashEvent.CoinStashState.NOT_BUY_COUNTLACK)
            path = "local_ui/coinstash_bg1";
        else
            path = "local_ui/coinstash_bg2";

        gameObject.AddressableAssetLoad<Texture2D>(path, (texture) =>
        {
            texEventComplte.mainTexture = texture;
            texEventComplte.height = 289;
        });
    }

    private string GetInfoMessage(ManagerCoinStashEvent.CoinStashState state)
    {
        string message = null;

        switch (state)
        {
            case ManagerCoinStashEvent.CoinStashState.NOT_BUY_COINLACK:
                message = Global._instance.GetString("p_gp_2");
                break;
            case ManagerCoinStashEvent.CoinStashState.BUY_COINSTASH:
                message = Global._instance.GetString("p_gp_3");
                break;
            case ManagerCoinStashEvent.CoinStashState.BUY_FULLCOIN:
                message = Global._instance.GetString("p_gp_4");
                break;
            default:
                message = "";
                break;
        }

        return message;
    }

    private void SetRewardPoint()
    {
        float fullCoin = ServerContents.CoinStashEvent.coinMax;
        float pointCoin = ServerContents.CoinStashEvent.coinMin;
        float currentCoin = ServerRepos.UserCoinStash.storedCoin;

        float RewardPoint = pointCoin / fullCoin;
        float CurrentPoint = currentCoin / fullCoin;

        progressBar.value = CurrentPoint;

        Vector3 posZeroCoin = objRewardPoints[0].transform.localPosition;
        Vector3 posFullcoin = objRewardPoints[2].transform.localPosition;

        objRewardPoints[1].transform.localPosition = ((posFullcoin - posZeroCoin) * RewardPoint) + posZeroCoin;

    }

    public void OnClickButton()
    {
        if (ManagerCoinStashEvent.IsBuyCoinStash())
        {
            string text = Global._instance.GetString("n_b_18");
            string coin = ServerRepos.UserCoinStash.storedCoin > ServerContents.CoinStashEvent.price * 100 ? "3000" : $"{ServerRepos.UserCoinStash.storedCoin}";
            string bonus = $"{ManagerCoinStashEvent.GetBonusCoin(ServerRepos.UserCoinStash.storedCoin)}";

            text = text.Replace("[n]", coin);
            text = text.Replace("[m]", bonus);

            UIPopupSystemDescription systemPopup = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
            systemPopup.SetCallbackSetting(1, BuyCoinStash, true);
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: PackagePriceType.Jewel, price_value: ServerContents.CoinStashEvent.price.ToString());
            systemPopup.SetResourceImage("Message/coin");
            systemPopup.ShowBuyInfo("buyinfo_gp_1");
        }
        else
        {
            if (ManagerCoinStashEvent.GetCoinStashState() == ManagerCoinStashEvent.CoinStashState.NOT_BUY_COINLACK)
            {
                UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_20"), false);
                systemPopup.SortOrderSetting();
            }
        }
    }

    public void OnClickInfoButton()
    {
        ManagerUI._instance.OpenPopup<UIPopupCoinStashEventInfo>();
    }

    void BuyCoinStash()
    {
        if(Global.jewel < ServerContents.CoinStashEvent.price)
        {
            StartCoroutine(LackDiamondsPopUp());
        }
        else
        {
            var useJewel = Global._instance.UseJewel(ServerContents.CoinStashEvent.price);

            ServerAPI.BuyCoinStash
            (
                (resp) =>
                {
                    var useDIAMOND = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_COIN_STASH,
                        -useJewel.usePMoney,
                        -useJewel.useFMoney,
                        (int)(ServerRepos.User.jewel),
                        (int)(ServerRepos.User.fjewel),
                        resp.itemId
                        );
                    var docDIAMOND = JsonConvert.SerializeObject(useDIAMOND);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDIAMOND);

                    var getCoin = new ServiceSDK.GrowthyCustomLog_Money (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_COIN_STASH,
                        ServerContents.CoinStashEvent.coinMin,
                        (resp.buyCoin - ServerContents.CoinStashEvent.coinMin),
                        (int)(ServerRepos.User.coin),
                        (int)(ServerRepos.User.fcoin),
                        resp.itemId
                        );
                    var docCOIN = JsonConvert.SerializeObject(getCoin);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCOIN);

                    if (resp.IsSuccess)
                    {
                        UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_19"), false, 
                            () =>
                            {
                                OnClickBtnClose();
                            });

                        systemPopup.SortOrderSetting();
                        systemPopup.SetResourceImage("Message/ok");

                        InitData();

                        SetRewardPoint();

                        Global.coin = (int)(GameData.User.AllCoin);
                        Global.jewel = (int)(GameData.User.AllJewel);

                        ManagerCoinStashEvent.currentUserCoin = 0;

                        ManagerUI._instance.UpdateUI();

                        if(ManagerLobby._instance != null) // 로비에서 구매 했을 때 패키지 배너 업데이트 하도록 수정
                            ManagerUI._instance.PackageUpdate();
                    }
                    
                    if(bAutoOpen)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                            resp.itemId,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
                        var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }
                }
            );
        }
    }

    private IEnumerator LackDiamondsPopUp()
    {
        yield return new WaitUntil(() => UIPopupSystem._instance == null);
        ManagerUI._instance.LackDiamondsPopUp();
    }
}