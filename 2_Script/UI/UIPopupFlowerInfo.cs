using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupFlowerInfo : UIPopupBase
{
    public UILabel[] buttonText;
    public UILabel[] titleName_1;
    public UILabel[] titleName_2;
    public UILabel[] flowerText_1;
    public UILabel[] flowerText_2;
    public UILabel flowerInfoText;

    public UITexture lightImage;
    public UITexture arrowImage;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        SettingTitle();
        SettingFlowerText();
        SettingPopupText();
        SettingButton();
        SettingTexture();
    }

    private void SettingTitle()
    {
        string titleText_1 = Global._instance.GetString("p_epc_1");
        string titleText_2 = Global._instance.GetString("p_epc_2");
        for (int i = 0; i < titleName_1.Length; i++)
        {
            titleName_1[i].text = titleText_1;
        }
        for (int i = 0; i < titleName_2.Length; i++)
        {
            titleName_2[i].text = titleText_2;
        }
    }

    private void SettingFlowerText()
    {
        string epText_1 = Global._instance.GetString("p_epc_3");
        string epText_2 = Global._instance.GetString("p_epc_4");
        for (int i = 0; i < flowerText_1.Length; i++)
        {
            flowerText_1[i].text = epText_1;
        }
        for (int i = 0; i < flowerText_2.Length; i++)
        {
            flowerText_2[i].text = epText_2;
        }
    }

    private void SettingPopupText()
    {
        flowerInfoText.text = Global._instance.GetString("p_epc_5");
    }

    private void SettingButton()
    {
        string btnText = Global._instance.GetString("btn_4");
        for (int i = 0; i < buttonText.Length; i++)
        {
            buttonText[i].text = btnText;
        }
    }

    private void SettingTexture()
    {
        lightImage.mainTexture = Resources.Load("UI/light") as Texture2D;
        arrowImage.mainTexture = Resources.Load("UI/icon_arrow") as Texture2D;
    }
}
