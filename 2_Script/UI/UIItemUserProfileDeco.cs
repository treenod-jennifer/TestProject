using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfileDeco : MonoBehaviour
{
    public UITexture DecoImage;
    public UITexture DecoImageShadow;

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

    public void OnLoadComplete(Texture2D r)
    {
        DecoImage.mainTexture = r;
        DecoImage.color = new Color(1f, 1f, 1f);
        DecoImageShadow.mainTexture = r;
        DecoImage.MakePixelPerfect();
        DecoImageShadow.MakePixelPerfect();

        DecoImage.width += 30;
        DecoImage.height += 30;
        DecoImageShadow.width += 30;
        DecoImageShadow.height += 30;
    }

    public void SetDecoItemImage(string fileName)
    {
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconHousing", fileName, OnLoadComplete, true);
    }
}
