using System.Collections;
using System.Collections.Generic;
﻿using PokoAddressable;
using UnityEngine;

public class UIPopupMaterialInfo : UIPopupBase
{
    public UILabel[] title;
    public UILabel[] btnOk;
    public UILabel message;
    public UILabel[] getPath;
    public GameObject field;
    public GameObject box1_X;
    public GameObject box2_X;
    public GameObject box3_X;
    public UITexture field_icon;
    public UITexture box1_icon;
    public UITexture box2_icon;
    public UITexture box3_icon;

    private float color = 0f;

    public void InitMaterialInfo(string name, MaterialGetInfo info)
    {
        color = 130f / 255f;
        InitTexture();
        InitPopupText();
        InitPopupMessage(name);
        InitPathText();

        field.SetActive(!info.bFiled);
        box1_X.SetActive(!info.bBox1);
        box2_X.SetActive(!info.bBox2);
        box3_X.SetActive(!info.bBox3);
        
        InitGetPathIcon(info.bFiled, field_icon, field);
        InitGetPathIcon(info.bBox1, box1_icon, box1_X);
        InitGetPathIcon(info.bBox2, box2_icon, box2_X);
        InitGetPathIcon(info.bBox3, box3_icon, box3_X);
    }

    private void InitTexture()
    {
        gameObject.AddressableAssetLoad<Texture2D>("local_message/materialBox", (texture) => field_icon.mainTexture = texture);
        gameObject.AddressableAssetLoad<Texture2D>("local_message/giftbox1", (texture) => box1_icon.mainTexture = texture);
        gameObject.AddressableAssetLoad<Texture2D>("local_message/giftbox2", (texture) => box2_icon.mainTexture = texture);
        gameObject.AddressableAssetLoad<Texture2D>("local_message/giftbox3", (texture) => box3_icon.mainTexture = texture);
    }
    
    private void InitPopupText()
    {
        string titleText = Global._instance.GetString("p_t_4");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }

        string btnText = Global._instance.GetString("btn_1");
        for (int i = 0; i < btnOk.Length; i++)
        {
            btnOk[i].text = btnText;
        }
    }

    private void InitPopupMessage(string name)
    {
        string text = Global._instance.GetString("p_mt_3");
        text.Replace("[1]", name);
        message.text = text;
    }

    private void InitPathText()
    {
        for (int i = 0; i < getPath.Length; i++)
        {
            string key = string.Format("mt_info_{0}", i);
            getPath[i].text = Global._instance.GetString(key);
        }
    }

    private void InitGetPathIcon(bool bActive, UITexture texture, GameObject obj)
    {
        obj.SetActive(!bActive);
        if (bActive == true)
            return;
        texture.color = new Color(color, color, color, color);
    }
}
