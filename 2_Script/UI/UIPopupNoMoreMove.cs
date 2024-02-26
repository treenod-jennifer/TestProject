using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokoAddressable;

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
        gameObject.AddressableAssetLoad<Texture2D>("local_message/icon_nomove", (texture) => nomoreMoveIcon.mainTexture = texture);
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
