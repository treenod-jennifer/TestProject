using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupBoxShopInfo : UIPopupBase
{
    public UILabel[] title;

    public UITexture[] boxTexture;
    public UILabel[] infoText;

    public UIScrollView scrollView;
    public UIUrlTexture infoImage;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        SettingBoxTexture();
        SettingText();
        //SettingBoxInfoTexture();
    }

    private void SettingBoxTexture()
    {
        for (int i = 0; i < boxTexture.Length; i++)
        {
            string path = string.Format("Message/shop_giftBox{0}", (i + 1));
            boxTexture[i].mainTexture = Resources.Load(path) as Texture2D;
        }
    }

    private void SettingText()
    {
        string titleText = Global._instance.GetString("p_bsl_1");
        title[0].text = titleText;
        title[1].text = titleText;

        for (int i = 0; i < infoText.Length; i++)
        {
            string textKey = string.Format("p_bsl_{0}", i + 2);
            infoText[i].text = Global._instance.GetString(textKey);
        }
    }

    public void OnClickBtnDetailInfo()
    {
        ManagerUI._instance.OpenPopupBoxShopInfoDetail();
    }

    public void OnClickShowChanceLanBoard()
    {
        //if (GameData.LoginCdn.probabilityInfo.Length > 0)
        {
           // ServiceSDK.ServiceSDKManager.instance.ShowLANBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, GameData.LoginCdn.probabilityInfo, "福袋の中身");
        }
    }

    private void SettingBoxInfoTexture()
    {
        int version = 0;
        string fileName = string.Format("boxShopInfo");
        if (PlayerPrefs.HasKey("BoxShopInfoVer" + version) == false)
        {
            PlayerPrefs.SetInt("BoxShopInfoVer", version);
            //버전이 바꼈다면 강제로 로드.
            infoImage.ForceLoad(Global.gameImageDirectory, "IconMaterial/", fileName);
        }
        else
        {
            infoImage.Load(Global.gameImageDirectory, "IconMaterial/", fileName);
        }
    }
}
