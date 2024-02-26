using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonPlayVideo : UIButtonEventBase
{
    int videoIndex = 1;

    public override void SetButtonEvent(int idx)
    {
        this.videoIndex = idx;
        base.SetButtonEvent(videoIndex);
        string fileName = string.Format("ev_pv_{0}", videoIndex);

        buttonTexture.SuccessEvent += OnLoadComplete;
        buttonTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", fileName);
    }

    protected override void OnClickBtnEvent()
    {
        ManagerUI._instance.OpenPopupVideo(videoIndex);
    }
}
