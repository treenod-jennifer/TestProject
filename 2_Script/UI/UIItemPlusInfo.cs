using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemPlusInfo : MonoBehaviour
{
    public UITexture BannerTexture;

    private string url;
    private string imgName;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void SettingPluseInfoItem(string textureName, string imageUrl)
    {
        imgName = textureName;

        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Banner", textureName, OnLoadComplete, true);
        url = imageUrl;
    }

    public void OnLoadComplete(Texture2D r)
    {
        BannerTexture.mainTexture = r;
        BannerTexture.MakePixelPerfect();
    }

    void OnClick()
    {
        if (url != null)
        {
            if (url.Length > 0)
            {
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.LINK,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.OPEN_LINK,
                    imgName,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

                Application.OpenURL(url);
            }
                
        }
    }
}
