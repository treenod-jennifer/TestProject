using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceSDK;
using UnityEngine;

public class UIItemEndContents_PriseItem : MonoBehaviour
{
    [SerializeField] private UIUrlTexture texPriseItem;
    [SerializeField] private UISprite sprPriseItem;
    [SerializeField] private UISprite sprItemPrice;
    [SerializeField] private UILabel[] labelItemPrise;
    [SerializeField] private GameObject oneLimitOption;
    [SerializeField] private GameObject countObj;
    [SerializeField] private UIUrlTexture costumeObj;
    [SerializeField] private UILabel[] labelTimeCount;
    [SerializeField] private GameObject[] aniGrade;
    [SerializeField] private GameObject limitedItem;
    [SerializeField] private GameObject InfoBtn;
    [SerializeField] private UILabel labelLimitedItem;
    [SerializeField] private UILabel labelEvent;

    CdnEndContentsShop priceItem = new CdnEndContentsShop();

    public void UpdateData(CdnEndContentsShop priseItemData)
    {
        priceItem = priseItemData;
        gameObject.SetActive(priceItem != null);
        if (priceItem == null)
            return;
        
        if(priceItem.endTs == 0)
        {
            limitedItem.SetActive(false);
        }
        else
        {
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
            int buyCount = 0;
            if (ManagerEndContentsEvent.instance.GoodsInfo.ContainsKey(priceItem.goodsIndex.ToString()))
                buyCount = ManagerEndContentsEvent.instance.GoodsInfo[priceItem.goodsIndex.ToString()];
            oneLimitOption.GetComponentsInChildren<UILabel>().SetText($"{buyCount}/{priceItem.buyLimit}");
        }
        //텍스쳐 처리
        SetRewardTexture(priceItem.reward[0]);

        //아이템 가격
        for (int i = 0; i < 2; i++)
            labelItemPrise[i].text = priceItem.price.ToString();

        sprItemPrice.atlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.UIAtlas;
        sprItemPrice.spriteName = "pokoCoin";
        InfoBtn.SetActive(priseItemData.isShowInfo);
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
            costumeObj.SettingTextureScale(costumeObj.width, costumeObj.height);
            costumeObj.LoadResource("Message/costume_stroke");
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
                $"at_{priceItem.reward[0].value:D4}.png",
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

        var BuyType = ManagerEndContentsEvent.instance.CanBuy(priceItem.goodsIndex);

        switch(BuyType)
        {
            case ManagerEndContentsEvent.BuyError.BUY_LIMIT:
                break;
            case ManagerEndContentsEvent.BuyError.INVALID_GOODS:
                break;
            case ManagerEndContentsEvent.BuyError.NOT_ENOUGH_TOKEN:
                {
                    stringMessage = Global._instance.GetString("n_ec_2");
                    ShowSystemPopup(stringMessage);
                }
                break;
            case ManagerEndContentsEvent.BuyError.NO_ERROR:
                {
                    stringMessage = Global._instance.GetString("n_ec_1").Replace("[n]", priceItem.price.ToString());
                    ShowSystemPopup(stringMessage, "EndContentsBuy");
                }
                break;
        }
    }
    
    void OnBuyEndContentsPriceItem(Protocol.EndContentsBuyShopResp resp)
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

            var growthyMoney = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.EC,
                GrowthyCustomLog_Money.Code_L_MRSN.U_END_CONTENTS_SHOP_PURCHASE,
                0,
                -priceItem.price,
                0,
                ManagerEndContentsEvent.GetPokoCoin()
            );
            var docMoney = JsonConvert.SerializeObject(growthyMoney);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);

            // 구매한 아이템 기록
            foreach (var item in priceItem.reward)
            {
                GrowthyCusmtomLogHelper.SendGrowthyLog(
                    item.type,
                    item.value,
                    GrowthyCustomLog_Money.Code_L_MRSN.G_END_CONTENTS_SHOP_PURCHASE,
                    GrowthyCustomLog_ITEM.Code_L_RSN.G_END_CONTENTS_SHOP_PURCHASE,
                    ""
                );
            }
            
            ManagerSound.AudioPlay(AudioInGame.PRAISE1);

            ManagerEndContentsEvent.instance.SyncFromServerUserData_PostBuyShop();
            UIPopupEndContentsEventExchangeStation._instance.labelPokoCoin.text = ManagerEndContentsEvent.GetPokoCoin().ToString();
            UIPopupEndContentsEventExchangeStation._instance.SetEndContentsColor(ManagerEndContentsEvent.GetPokoCoin());
            UIPopupEndContentsEventExchangeStation._instance.ResetPriceItemList();
            ManagerUI._instance.UpdateUI();
        }
        else
        {
            ShowSystemPopup("n_b_4");
        }
    }

    void EndContentsBuy()
    {
        ServerAPI.EndContentsBuyShop(priceItem.goodsIndex, OnBuyEndContentsPriceItem);
    }

    private void ShowSystemPopup(string stringKey, string succesFunctionName = null)
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

        string path;
        if ((RewardType)priceItem.reward[0].type == RewardType.animalOverlapTicket)
            path = RewardHelper.GetOverlapTextureResourcePath(priceItem.reward[0].value);
        else
            path = RewardHelper.GetRewardTextureResourcePath((RewardType)priceItem.reward[0].type);
        
        popup.FunctionSetting(1, succesFunctionName, gameObject, true);
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), stringKey, false);

        if ((RewardType)priceItem.reward[0].type == RewardType.animal)
        {
            ResourceManager.LoadCDN
            (
                "Animal",
                $"ap_{priceItem.reward[0].value:D4}.png",
                (Texture2D texture) =>
                {
                    popup.SetResourceImage(path, texture);
                }
            );
        }
        else
            popup.SetResourceImage(path, (Texture2D)texPriseItem.mainTexture);
    }

    private void OnClickInfoButton()
    {
        int housingIdx = (int)(priceItem.reward[0].value / 10000);
        int modelIdx = (int)(priceItem.reward[0].value % 10000);
        string fileName = $"{housingIdx}_{modelIdx}";
        
        UIPopUpDecoInfo popup = ManagerUI._instance.OpenPopup<UIPopUpDecoInfo>();
        popup.InitPopup("DecoInfo/", $"e_deco_{fileName}.png", $"ec_" + fileName + "_info");
    }
}
