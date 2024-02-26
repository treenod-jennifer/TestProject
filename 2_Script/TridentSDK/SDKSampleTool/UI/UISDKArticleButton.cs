using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UISDKArticleButton : UIPokoButton 
{
    public string idBtnArticle;
    
    public GameObject callbackTarget;
    public string callbackName;

    public void OnClickBtnOK ()
    {
        if ( string.IsNullOrEmpty( callbackName )) return;
        if ( this.callbackTarget == null ) return;

        this.callbackTarget.SendMessage( callbackName, this.idBtnArticle, SendMessageOptions.DontRequireReceiver );
    }
}
