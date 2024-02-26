using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupNoMoreMove : UIPopupBase
{
    static public UIPopupNoMoreMove instance = null;
    public UITexture nomoreMoveIcon;
    
    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        nomoreMoveIcon.mainTexture = Resources.Load("Message/icon_nomove") as Texture2D;
        yield return new WaitForSeconds(1.0f);
        ManagerUI._instance.ClosePopUpUI();
    }

    public override void OnClickBtnBack()
    {
        return;
    }

    new void OnDestroy()
    {
        instance = null;
        base.OnDestroy();
    }
}
