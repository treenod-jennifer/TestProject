using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIItemColor : MonoBehaviour
{
    public Color color;
    public UISprite disabled;
    public UISprite pressed;

    private Action<Color> btnEventDelegate;
    private Action<UIItemColor> curSelectDelegate;
 
    public void InitData ( Action<Color> btnEventDelegate , Action<UIItemColor> curSelectDelegate )
    {
        this.disabled.enabled = true;
        this.pressed.enabled = false;
        this.btnEventDelegate  = btnEventDelegate;
        this.curSelectDelegate = curSelectDelegate;
    }

    public void OnClickBtnColor ()
    {
        this.ChangeBtnState();
        this.btnEventDelegate ( color );
        this.curSelectDelegate( this );
    }

    public void OnDeselectBtnColor ()
    {
        this.ChangeBtnState();
    }

    private void ChangeBtnState ()
    {
        this.pressed.enabled = !this.pressed.enabled;
        //this.disabled.enabled = !this.disabled.enabled;
    }
}
