using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokoAddressable;

public class UIItemWorldRank_PriseItem : MonoBehaviour
{
    [SerializeField] private UIUrlTexture texPriseItem;
    [SerializeField] private UISprite sprPriseItem;
    [SerializeField] private UILabel[] labelItemPrise;
    [SerializeField] private GameObject oneLimitOption;
    [SerializeField] private GameObject countObj;
    [SerializeField] private UIUrlTexture costumeObj;
    [SerializeField] private UILabel[] labelTimeCount;
    [SerializeField] private GameObject[] aniGrade;
    [SerializeField] private GameObject limitedItem;
    [SerializeField] private UILabel labelLimitedItem;
    [SerializeField] private UILabel labelEvent;

    CdnWorldRankShop priceItem = new CdnWorldRankShop();

    public void UpdateData(CdnWorldRankShop priseItemData)
    {
        priceItem = priseItemData;

        if (priceItem == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        if(priceItem.endTs == 0)
        {//기간 한정 상품이 아닌 아이템
            limitedItem.SetActive(false);
        }
        else
        {//기간 한정 상품 아이템
            StartCoroutine(CoPackageTime());
            limitedItem.SetActive(true);
            labelEvent.text = Global._instance.GetString("icon_2");
        }

        //한정 수량 물품
        if(priceItem.buyLimit == 0)
        {
            oneLimitOption.SetActive(false);
        }
        else
        {
            oneLimitOption.SetActive(true);
            oneLimitOption.GetComponentsInChildren<UILabel>().SetText($"{ManagerWorldRanking.userData.UserBuyLimitItemCount(priceItem)}/{priceItem.buyLimit}");
        }
        //텍스쳐 처리
        SetRewardTexture(priceItem.reward);

        //아이템 가격
        for (int i = 0; i < 2; i++)
        {
            labelItemPrise[i].text = priceItem.price.ToString();
        }

    }
    

    IEnumerator CoPackageTime()
    {
        yield return null;
        long leftTime = 0;

        while (limitedItem.activeInHierarchy == true)
        {
            leftTime = Global.LeftTime(priceItem.endTs);
            if (leftTime <= 0)
            {
                leftTime = 0;
                labelLimitedItem.text = "00:00:00";
                ManagerUI._instance.UpdateUI();
                yield break;
            }
            labelLimitedItem.text = Global.GetTimeText_DDHHMM(priceItem.endTs);
            yield return null;
        }
        yield return null;
    }

    void SetRewardTexture(Reward rewardItem)
    {
        RewardType type = (RewardType)rewardItem.type;
        int value = rewardItem.value;
        countObj.SetActive(false);
        costumeObj.gameObject.SetActive(false);

        RewardHelper.SetRewardImage(rewardItem, sprPriseItem, texPriseItem, labelTimeCount, 1.00f, true, true, aniGrade);

        if(type == RewardType.costume)
        {
            gameObject.AddressableAssetLoad<Texture2D>("local_message/costume_stroke", (texture) => costumeObj.mainTexture = texture);
            costumeObj.SettingTextureScale(costumeObj.width, costumeObj.height);
            costumeObj.gameObject.SetActive(true);
        }
        else if(type == RewardType.cloverFreeTime || type == RewardType.wingFreetime || type == RewardType.readyItem3_Time
            || type == RewardType.readyItem4_Time || type == RewardType.readyItem5_Time 
            || type == RewardType.readyItem6_Time || type == RewardType.readyItemBomb_Time)
        {
            countObj.SetActive(true);
        }
        else if(type == RewardType.animal)
        {
            ResourceManager.LoadCDN
            (
                "Animal",
                $"at_{priceItem.reward.value:D4}.png",
                (Texture2D texture) =>
                {
                    texPriseItem.mainTexture = texture;
                    texPriseItem.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
            );
        }
    }

    void OnClickPriceItem()
    {
        string stringMessage;

        var BuyType = ManagerWorldRanking.userData.CanBuy(priceItem.goodsId);

        switch(BuyType)
        {
            case ManagerWorldRanking.UserData.BuyError.BUY_LIMIT:
                break;
            case ManagerWorldRanking.UserData.BuyError.INVALID_GOODS:
                break;
            case ManagerWorldRanking.UserData.BuyError.NOT_ENOUGH_TOKEN:
                {
                    stringMessage = Global._instance.GetString("n_wrk_2");
                    ShowSystemPopup(stringMessage);
                }
                break;
            case ManagerWorldRanking.UserData.BuyError.NO_ERROR:
                {
                    stringMessage = Global._instance.GetString("n_wrk_1").Replace("[n]", priceItem.price.ToString());
                    ShowSystemPopup(stringMessage, "WorldRankBuy");
                }
                break;
        }
    }
    
    void OnBuyWolrdRankPriceItem(Protocol.WorldRankBuyResp resp)
    {
        if( resp.IsSuccess)
        {
            // 구매 완료 팝업
            ManagerUI._instance.OpenPopup<UIPopupSystem>((pop) =>
            {
                pop.SortOrderSetting();
                pop.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_wrk_12"), false);
                pop.SetResourceImage("Message/ok");
            });

            // 사용한 토큰 기록
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)RewardType.rankToken,
                                resp.usedToken,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_WORLDRANK_SHOP,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                                ((RewardType) priceItem.reward.type).ToString()
                            );

            // 구매한 아이템 기록
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                priceItem.reward.type,
                                priceItem.reward.value,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_RANKSHOP_BUY,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_RANKSHOP_BUY,
                                ""
                            );

            ManagerSound.AudioPlay(AudioInGame.PRAISE1);

            UIPopUpWorldRankExchangeStation._instance.labelRankToken.text = ManagerWorldRanking.userData.GetRankToken().ToString();
            UIPopUpWorldRankExchangeStation._instance.SetRankTokenColor(ManagerWorldRanking.userData.GetRankToken());
            UIPopUpWorldRankExchangeStation._instance.ResetPriceItemList();
        }
        else
        {
            ShowSystemPopup("n_b_4");
        }
    }

    void WorldRankBuy()
    {
        ServerAPI.WorldRankBuy(priceItem.goodsId, OnBuyWolrdRankPriceItem);
    }

    private void ShowSystemPopup(string stringKey, string succesFunctionName = null)
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

        string path;
        if ((RewardType)priceItem.reward.type == RewardType.animalOverlapTicket)
            path = RewardHelper.GetOverlapTextureResourcePath(priceItem.reward.value);
        else
            path = RewardHelper.GetRewardTextureResourcePath((RewardType)priceItem.reward.type);
        
        popup.FunctionSetting(1, succesFunctionName, gameObject, true);
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), stringKey, false);

        if ((RewardType)priceItem.reward.type == RewardType.animal)
        {
            ResourceManager.LoadCDN
            (
                "Animal",
                $"ap_{priceItem.reward.value:D4}.png",
                (Texture2D texture) =>
                {
                    popup.SetResourceImage(path, texture);
                }
            );
        }
        else
            popup.SetResourceImage(path, (Texture2D)texPriseItem.mainTexture);
    }
}
