using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupDiamondShop : UIPopupBase
{
    public static UIPopupDiamondShop _instance = null;

    public UILabel diashopText;
    //public UIItemDiamondShop[] itemDiashop;

    public UITexture boniImage;
    public UITexture[] diaIcon;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int _depth)
    {/*
        base.OpenPopUp(_depth);
        popupType = PopupType.jewelShop;
        boniImage.mainTexture = Resources.Load("UI/shop_boni_diamond") as Texture2D;
        for (int i = 0; i < 5; i++)
        {
            itemDiashop[i].InitItemDia(i);
            string path = string.Format("UI/shop_icon_dia_0{0}", (i + 1));
            diaIcon[i].mainTexture = Resources.Load(path) as Texture2D;
        }*/
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
       // ServiceSDK.ServiceSDKManager.instance.ShowLANBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", "特定商取引法の表示");
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        //ServiceSDK.ServiceSDKManager.instance.ShowLANBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", "資金決済法に基づく表示");
    }
}
