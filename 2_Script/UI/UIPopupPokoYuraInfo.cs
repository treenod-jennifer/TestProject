using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupPokoYuraInfo : UIPopupBase
{
    public UILabel[] title;
    public UILabel message;

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        string titleText = Global._instance.GetString("p_u_p_7");
        title[0].text = titleText;
        title[1].text = titleText;
        message.text = Global._instance.GetString("p_u_p_8");
    }
}
