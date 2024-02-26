using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIPopupConfirm : UIPopupBase
{
    public static UIPopupConfirm _instance = null;

    private Action onCallbackEvent; 

    public UILabel labelDescription;

    void Awake ()
    {
        _instance = this;
    }

    new void OnDestroy ()
    {
        // 팝업 꺼질때 callbackEvent
        if( onCallbackEvent != null )
        { 
            this.onCallbackEvent();
        }
        base.OnDestroy();
    }

    public void InitPopUp ( string labelDescription, Action onCallbackEvent )
    {
        this.labelDescription.text = labelDescription;
        this.onCallbackEvent = onCallbackEvent;
    }

    public void OnClickBtnOk ()
    {
        ManagerUI._instance.ClosePopUpUI();
    }
  
}
