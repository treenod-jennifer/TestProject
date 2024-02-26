using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupBoxShopInfoDetail : UIPopupBase
{
    [SerializeField]
    private GameObject loadText;

    [SerializeField]
    private GameObject btnClose;

    [SerializeField]
    private UIUrlTexture texture;

    [SerializeField]
    UISprite frameTexture;

    static bool onceLoaded = false;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        texture.SettingCallBack(SettingLoadComplete);

        if( onceLoaded )
            texture.Load(Global.gameImageDirectory, "Shop/", "box_material_info");
        else
        {
            texture.ForceLoad(Global.gameImageDirectory, "Shop/", "box_material_info");
            onceLoaded = true;
        }
    }

    private void SettingLoadComplete()
    {
        loadText.SetActive(false);

        frameTexture.width = (int)(texture.localSize.x + 26);
        frameTexture.height = (int)(texture.localSize.y + 34);
        btnClose.transform.localPosition = new Vector3((frameTexture.width / 2) - 65, (frameTexture.height / 2) - 60, btnClose.transform.localPosition.z);
    }
}
