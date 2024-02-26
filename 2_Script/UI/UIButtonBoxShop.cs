using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonBoxShop : UIButtonEventBase
{
    public override void SetButtonEvent(int version)
    {
        base.SetButtonEvent(version);
        string fileName = string.Format("box_npc_icon_{0}", version);
        if (PlayerPrefs.HasKey("BoxShopIcon") == false
            && PlayerPrefs.GetInt("BoxShopIcon") == version)
        {
            PlayerPrefs.SetInt("BoxShopIcon", version);
            //버전이 바꼈다면 강제로 로드.
            UIImageLoader.Instance.ForceLoad(Global.gameImageDirectory, "Shop/", fileName, this);
        }
        else
        {
            UIImageLoader.Instance.Load(Global.gameImageDirectory, "Shop/", fileName, this);
        }
    }

    protected override void OnClickBtnEvent()
    {
        if (UIPopupReady._instance != null)
            return;
        ManagerUI._instance.OpenPopupBoxShop();
    }
}
