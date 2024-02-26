using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json;

public class UIPopupBoxShop : UIPopupBase
{
    public UILabel[] title;
    public UILabel merchantText;
    public UITexture merchantBubble;
    public GameObject merchantRoot;

    public UIPanel shopPanel;

    //박스들 관련.
    public UITexture[] boxTexture;
    public UILabel[] boxPrice_1;
    public UILabel[] boxPrice_2;
    public UILabel[] boxPrice_3;
    public UILabel[] purchaseText;
    public UILabel[] boxName_1;
    public UILabel[] boxName_2;
    public UILabel[] boxName_3;
    public UILabel[] infoText;

    //이벤트 관련 UI.
    public UIUrlTexture eventIcon;

    //상인 live2D.
    private LAppModelProxy merchantLive2D;

    public bool inSale = false;

    public override void OpenPopUp(int _depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = _depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        shopPanel.depth = uiPanel.depth + 1;
        SettingPopUp();
        SettingBoxTexture();
        SettingBoxName();
        SettingBoxPurchaseButton();
        SettingMerchantChar();
        SettingMerchantText();
        SettingPriceText();
        SettingEvent();

        StartCoroutine(CoAction(openTime, () =>
        {
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
            ManagerSound.AudioPlay(AudioLobby.m_alphonse_good);
        }));
    }

    public override void ClosePopUp(float _startTime = 0.3f, Method.FunctionVoid callback = null)
    {
        merchantLive2D.setAnimation(false, "Out");
        base.ClosePopUp();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //샵 창 전에 팝업들이 sortOrder을 사용하지 않는다면 live2d 쪽만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            merchantLive2D._CubismRender.SortingOrder = layer + 1;
            shopPanel.sortingOrder = layer + 2;
        }
        else
        {
            merchantLive2D._CubismRender.SortingOrder = layer;
            shopPanel.sortingOrder = layer + 1;
        }
        shopPanel.useSortingOrder = true;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private void SettingPopUp()
    {
        string titleText = Global._instance.GetString("p_bs_1");
        title[0].text = titleText;
        title[1].text = titleText;

        string btnInfoText = Global._instance.GetString("btn_26");
        infoText[0].text = btnInfoText;
        infoText[1].text = btnInfoText;
    }

    private void SettingBoxTexture()
    {
        for (int i = 0; i < boxTexture.Length; i++)
        {
            string path = string.Format("Message/shop_giftBox{0}", (i + 1));
            boxTexture[i].mainTexture = Resources.Load(path) as Texture2D;
        }
    }

    private void SettingBoxName()
    {
        string name_1 = Global._instance.GetString("p_bs_3");
        string name_2 = Global._instance.GetString("p_bs_4");
        string name_3 = Global._instance.GetString("p_bs_5");

        boxName_1[0].text = name_1;
        boxName_1[1].text = name_1;
        boxName_2[0].text = name_2;
        boxName_2[1].text = name_2;
        boxName_3[0].text = name_3;
        boxName_3[1].text = name_3;
    }

    private void SettingBoxPurchaseButton()
    {
        for (int i = 0; i < purchaseText.Length; i++)
        {
            purchaseText[i].text = Global._instance.GetString("btn_27");
        }
    }

    private void SettingMerchantChar()
    {
        merchantLive2D = NGUITools.AddChild(merchantRoot, ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Alphonse].obj).GetComponent<LAppModelProxy>();
        merchantLive2D.SetVectorScale(new Vector3(600f, 600f, 600f));
        merchantLive2D._CubismRender.SortingOrder = uiPanel.sortingOrder + 1;
        merchantRoot.SetActive(true);
        merchantLive2D.setAnimation(false, "Appear");
    }

    private void SettingMerchantText()
    {
        merchantBubble.mainTexture = Resources.Load("Tutorial/tutorial_bubble01") as Texture2D;
        merchantText.text = Global._instance.GetString("p_bs_2");

        int _nLineCharCount = (int)(merchantText.printedSize.x / merchantText.fontSize);

        //26f : 폰트 사이즈.
        float boxLength = 80f + (_nLineCharCount * 26f);
        //최대 크기 설정.
        if (boxLength > 320)
            boxLength = 320f;
        merchantBubble.width = (int)boxLength;

        //폰트 위치 설정.
        float xPos = (boxLength / 2) + 10;
        merchantText.transform.localPosition = new Vector3(xPos, merchantText.transform.localPosition.y, 0f);
    }

    void SettingPriceText()
    {
     //   SetText(this.boxPrice_1, (ServerRepos.LoginCdn.normalGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]).ToString());
     //   SetText(this.boxPrice_2, (ServerRepos.LoginCdn.specialGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]).ToString());
      //  SetText(this.boxPrice_3, (ServerRepos.LoginCdn.premiumGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]).ToString());
    }

    private void SettingEvent()
    {
      /*  int shopResourceId = ServerRepos.LoginCdn.shopResourceId;
        if (shopResourceId == 0)
        {
            eventIcon.gameObject.SetActive(false);
            return;
        }
        eventIcon.gameObject.SetActive(true);
        string fileName = string.Format("box_npc_event_{0}", shopResourceId);
        eventIcon.Load(Global.gameImageDirectory, "Shop/", fileName);*/
    }

    private void OnClickBtnInfo()
    {
        ManagerUI._instance.OpenPopupBoxShopInfo();
    }

    private void OnClickPurchaseBox0()
    {
        OnClickedPurchase(GiftBoxType.boxNomal);
    }
    private void OnClickPurchaseBox1()
    {
        OnClickedPurchase(GiftBoxType.boxSpecial);
    }
    private void OnClickPurchaseBox2()
    {
        OnClickedPurchase(GiftBoxType.boxPremium);
    }

    void OnClickedPurchase(GiftBoxType boxType)
    {
        if (!bCanTouch)
            return;
        bCanTouch = false;

        string boxImageName = "giftbox1";

        int price = 0;
        switch ( boxType )
        {
           // case GiftBoxType.boxNomal: price = ServerRepos.LoginCdn.normalGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]; boxImageName = "shop_giftBox1";  break;
          //  case GiftBoxType.boxSpecial: price = ServerRepos.LoginCdn.specialGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]; boxImageName = "shop_giftBox2"; break;
          //  case GiftBoxType.boxPremium: price = ServerRepos.LoginCdn.premiumGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]; boxImageName = "shop_giftBox3"; break;
            default: return;
        }

        if (Global.jewel < price)
        {
            ManagerUI._instance.LackDiamondsPopUp();
            bCanTouch = true;
            return;
        }

        purchaseBoxType = boxType;

        string message = Global._instance.GetString("n_b_10");
        message = message.Replace("[n]", price.ToString());
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        
        popupSystem.FunctionSetting(1, "OnConfirmPurchaseBox", gameObject);
        popupSystem.FunctionSetting(3, "OnPurchaseCancelled", gameObject);
        Texture2D texture = Resources.Load("Message/" + boxImageName) as Texture2D;
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, texture);
    }

    GiftBoxType? purchaseBoxType = null;

    public void OnConfirmPurchaseBox()
    {
        if( purchaseBoxType.HasValue )
        {
          //  ServerAPI.BuyGiftBoxWithAsset(purchaseBoxType.Value, OnPurchaseBoxResponse);
        }
            
    }

    public void OnPurchaseCancelled()
    {
        bCanTouch = true;
        purchaseBoxType = null;
    }
    /*
    private void OnPurchaseBoxResponse(Protocol.BuyGiftBoxResp resp)
    {
        if( resp.IsSuccess )
        {
            // 구매성공 확정
            UIPopUpOpenGiftBox popup = ManagerUI._instance.OpenPopupGiftBox();
            var boxData = new ServerUserGiftBox();
            if (purchaseBoxType.HasValue)
                boxData.type = (int)purchaseBoxType.Value;
            boxData.rewardList = resp.giftBox;
            popup._data = boxData;
            
            //그로씨            
            {
                int price = 0;
                switch (purchaseBoxType)
                {
                    case GiftBoxType.boxNomal: price = ServerRepos.LoginCdn.normalGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]; break;
                    case GiftBoxType.boxSpecial: price = ServerRepos.LoginCdn.specialGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale];  break;
                    case GiftBoxType.boxPremium: price = ServerRepos.LoginCdn.premiumGiftBoxPrice[ServerRepos.LoginCdn.giftBoxSale]; break;
                    default: return;
                }

                int useFJewel = 0;
                int usePJewel = 0;

                if ((int)ServerRepos.User.jewel > price) usePJewel = price;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = price - (int)ServerRepos.User.jewel;
                }
                else useFJewel = price;
                
                var useJewelPurchaseClover = new ServiceSDK.GrowthyCustomLog_Money
                   (
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_PRESENT_BOX,
                   -usePJewel,
                   -useFJewel,
                  (int)(ServerRepos.User.jewel),
                  (int)(ServerRepos.User.fjewel),
                   purchaseBoxType.ToString()
                   );
                var docJewel = JsonConvert.SerializeObject(useJewelPurchaseClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docJewel);
            }
        }
        else
        {

        }

        purchaseBoxType = null;
        bCanTouch = true;
    }
    */
    public void SetText(UILabel[] shadowText, string text)
    {
        for (int i = 0; i < shadowText.Length; ++i)
        {
            shadowText[i].text = text;
        }
    }
}
