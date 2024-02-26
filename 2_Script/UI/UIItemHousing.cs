using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using DG.Tweening;
using System;
using Newtonsoft.Json;

public class UIItemHousing : MonoBehaviour
{ 
    public UIUrlTexture housingIcon;
    public UISprite     housingBox;
    public UISprite     housingPriceIcon;
    public UILabel      housingPrice;
    public GameObject   housingSelectBox;
    public GameObject   housingSelectIcon;

    [HideInInspector]
    public PlusHousingModelData housingData;

    //아이템에서의 인덱스.
    private int itemIndex = 0;
    private bool bFree = false;
    private bool bAction = false;
    private bool bCoin = true;
    private Vector3 DEFAULT_POS = new Vector3(-1f, 15f, 0f);
    private Vector3 PURCHASE_POS = new Vector3(-1f, -3.5f, 0f);

    public void SetDecoInfo(int iIndex, PlusHousingModelData hData)
    {
        itemIndex = iIndex;
        housingData = hData;

        transform.localScale = Vector3.one;
        string fileName = string.Format("{0}_{1}", housingData.housingIndex, housingData.modelIndex);

        housingIcon.SettingTextureScale(85, 85);
        housingIcon.LoadCDN(Global.gameImageDirectory, "IconHousing/", fileName);
        //StartCoroutine(housingIcon.SetTextureScale(64, 64));
    }

    //구매 완료 된 상태의 버튼 세팅.
    public void PurchaseSetting()
    {
        housingIcon.transform.localPosition = PURCHASE_POS;
        housingSelectBox.SetActive(true);
        housingPriceIcon.gameObject.SetActive(false);
    }

    //구매 하지 않은 상태의 버튼 세팅.
    public void DefaultSetting()
    {
        housingSelectBox.SetActive(false);
        housingPriceIcon.gameObject.SetActive(true);
        housingIcon.transform.localPosition = DEFAULT_POS;

        if (housingData.costCoin != 0)
        {
            housingPrice.text = housingData.costCoin.ToString();
            housingPriceIcon.spriteName = "icon_coin";
            housingPriceIcon.MakePixelPerfect();
            housingPriceIcon.width = housingPriceIcon.width / 2;
            housingPriceIcon.height = housingPriceIcon.height / 2;
            bCoin = true;
        }
        else
        {
            housingPrice.text = housingData.costJewel.ToString();
            housingPriceIcon.spriteName = "icon_diamond";
            housingPriceIcon.MakePixelPerfect();
            housingPriceIcon.width = housingPriceIcon.width / 2;
            housingPriceIcon.height = housingPriceIcon.height / 2;
            bCoin = false;
        }
    }

    public void FreeSetting()
    { 
        //구매하지않은 상태에서 켜진 경우는 저장.
        bFree = true;
        housingPriceIcon.gameObject.SetActive(false);
        housingIcon.transform.localPosition = PURCHASE_POS;
    }

    public void OnClickDecoBtn()
    {
        // 현재 터치 불가능하거나 이미 선택돼있는 아이템을 다시 클릭한 경우는 함수 작동안함.
        // 하우징 추가되는 연출 나오고 있을 때는 무시.
        if (bAction == false && UIPopupHousing._instance.SelectObj(itemIndex, housingData) == false)
            return;

        // 아직 구매하지 않았거나, 연출에서 켜진게 아니라면 구매 팝업 생성.
        if (housingData.active == 0 && bFree == false)
        {
            UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
            popupSystem.SetCallbackSetting(1, PurchaseItem, true);
            popupSystem.SetButtonText(1, Global._instance.GetString("btn_58"));
            popupSystem.SetButtonText(2, Global._instance.GetString("btn_2"));
            string message = $"{Global._instance.GetString("n_b_15")} {housingPrice.text}";
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_5"), message, true);
            popupSystem.SetImage((Texture2D)housingIcon.mainTexture);
            popupSystem.ShowBuyInfo("buyinfo_dc_1");
        }
        // 구매를 했거나, 연출에서 켜진 경우는 선택한 모델로 변경시키기.
        else
        {
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.Housing_Select);

            // 미션형이면서 추가가 아니면 그냥 선택
            if (housingData.type == PlusHousingModelDataType.byMission)
            {
                SetPurchaseSetting();
            }
            else
            {
                ModelSetting();
            }
        }
    }

    public void ModelSetting()
    {
        ManagerHousing.MakeHousingInstance(housingData.housingIndex, housingData.modelIndex, SetPurchaseSetting);
    }

    private void SetPurchaseSetting()
    {
        housingBox.spriteName = "housing_box_01";
        housingSelectIcon.gameObject.SetActive(true);
        UIPopupHousing._instance.CheckAndChangeModel();
    }

    void PurchaseItem()
    {
        // 구매가능한지 검사한 뒤.
        if (UIPopupHousing._instance.CheckCanPurchase() == true)
        {
            //그로씨            
            if(bCoin == true)
            {
                if ((int)ServerRepos.User.coin >= housingData.costCoin) usePJewel = housingData.costCoin;
                else if ((int)ServerRepos.User.coin > 0)
                {
                    usePJewel = (int)ServerRepos.User.coin;
                    useFJewel = housingData.costCoin - (int)ServerRepos.User.coin;
                }
                else useFJewel = housingData.costCoin;
            }
            else
            {
                if ((int)ServerRepos.User.jewel >= housingData.costJewel) usePJewel = housingData.costJewel;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = housingData.costJewel - (int)ServerRepos.User.jewel;
                }
                else useFJewel = housingData.costJewel;
            }



            UIPopupHousing._instance.bCanTouch = false;
            ServerAPI.HousingBuy(housingData.housingIndex, housingData.modelIndex, recvHousingBuy);
        }
    }

    int useFJewel = 0;
    int usePJewel = 0;

    void recvHousingBuy(HousingBuyResp resp)
    {
        if (resp.IsSuccess)
        {
            PlusHousingModelData.SetUserData();
            UIPopupHousing._instance.PurchaseHousingItem();
            UIPopupHousing._instance.CheckAndChangeModel();
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            housingBox.spriteName = "housing_box_01";

            if (bCoin == true)
            {
                Global.coin = (int)(GameData.User.AllCoin);
            }
            else
            {
                Global.jewel = (int)(GameData.User.AllJewel);
            }
            ManagerUI._instance.UpdateUI();
            StartCoroutine(CoChangeButton());

          
            //그로씨            
            {
                //그로씨
                string fileName = string.Format("h_{0}_{1}", housingData.housingIndex, housingData.modelIndex);
                string ItemName = string.Format("DECO-{0}-{1}", housingData.housingIndex, housingData.modelIndex);


                var getItem = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                              ItemName,//housingData.housingIndex.ToString(),
                              fileName,
                              1,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
                          );
                var docItem = JsonConvert.SerializeObject(getItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docItem);



                var playEndrecvHousingBuy = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        bCoin == true ? ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC : ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_DECO,
                        -usePJewel,
                        -useFJewel,
                        bCoin == true ? (int)(GameData.User.coin) : (int)(GameData.User.jewel),
                        bCoin == true ? (int)(GameData.User.fcoin) : (int)(GameData.User.fjewel),
                        fileName
                        );
                var doc = JsonConvert.SerializeObject(playEndrecvHousingBuy);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
            }
            
        }
    }

    public IEnumerator CoChangeButton(bool bClose = false, System.Action action = null)
    {   
        housingSelectIcon.transform.localScale = Vector3.zero;

        housingPriceIcon.transform.DOScale(Vector3.zero, 0.1f);
        yield return new WaitForSeconds(0.1f);

        housingIcon.transform.localScale = Vector3.one * 0.8f;
        housingIcon.transform.DOScale(Vector3.one, 0.2f).SetEase(UIPopupHousing._instance.selectButtonAnimation);
        housingIcon.transform.DOLocalMoveY(PURCHASE_POS.y, 0.2f).SetEase(UIPopupHousing._instance.selectButtonAnimation);
        yield return new WaitForSeconds(0.2f);

        housingPriceIcon.gameObject.SetActive(false);
        housingIcon.transform.localPosition = PURCHASE_POS;

        //하우징 선택 동그라미 박스 생성.
        housingSelectBox.SetActive(true);
        //하우징 선택 동그라미 생성.
        housingSelectIcon.gameObject.SetActive(true);
        housingSelectIcon.transform.DOScale(Vector3.one, 0.1f).SetEase(UIPopupHousing._instance.selectButtonAnimation);
        yield return new WaitForSeconds(0.1f);

        if (action != null)
        {
            action();
        }

        //구입 후 바로 팝업 닫히지 않는 경우에만 터치 활성화.
        if (bClose == false)
        {
            UIPopupHousing._instance.bCanTouch = true;
        }
    }

    public IEnumerator PlusHousingAction()
    {
        bAction = true;
        housingBox.alpha = 0;
        housingIcon.alpha = 0;
        housingIcon.transform.localScale = Vector3.one * 1.2f;
        DOTween.ToAlpha(() => housingBox.color, x => housingBox.color = x, 1f, 0.4f);
        transform.localScale = Vector3.one * 0.3f;
        transform.DOScale(Vector3.one, 0.4f).SetEase(UIPopupHousing._instance.selectButtonAnimation);

        //모델 읽어오고 세팅하는 부분.
        ModelSetting();

        yield return new WaitForSeconds(0.2f);
        DOTween.ToAlpha(() => housingIcon.color, x => housingIcon.color = x, 1f, 0.2f).SetEase(UIPopupHousing._instance.selectButtonAnimation);
        housingIcon.transform.DOScale(Vector3.one, 0.2f).SetEase(UIPopupHousing._instance.selectButtonAnimation);

        yield return new WaitForSeconds(0.2f);
        UIPopupHousing._instance.bCanTouch = true;
        bAction = false;
    }

    public void MoveBtnPosition(float xOffset)
    {
        transform.DOLocalMoveX(transform.localPosition.x + xOffset, 0.4f);
    }
}
