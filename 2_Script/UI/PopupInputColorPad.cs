using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PopupInputColorPad : UIPopupBase
{
    public List<UIItemColor> colorDataList = new List<UIItemColor>();
    public System.Action<List<UIItemColor>> colorDataList2;   
    private UIItemColor curColor;

	// Use this for initialization
    public void InitData ( Action<Color> eventDelegate, Action<List<UIItemColor>> eventDelegate2  )
    {
        int length = this.colorDataList.Count;
        Vector3 setupPos = Vector3.zero;

        int startX = 0;//-(50 * (length / 2));
        for(int i = 0; i < length; i++)
        {
            setupPos = new Vector3( startX + (70 * i), 0f, 0f );
            this.colorDataList[i].transform.localPosition = setupPos;
            this.colorDataList[i].InitData( eventDelegate, receiveColorHandler );
        }

        this.colorDataList2 = eventDelegate2;
    }

    private void receiveColorHandler ( UIItemColor curColor )
    {
        if( this.curColor != null )
        { 
            this.curColor.OnDeselectBtnColor();
        }   
        this.curColor = curColor;
    }

    public UIItemColor GetCurrentItemColor ()
    {
        return curColor;
    }

    private void ClosePopup ()
    {
        ManagerUI._instance.ClosePopUpUI ();
    }
}
