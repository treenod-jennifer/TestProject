using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_Install : UIItemDeco
{
    [Header("InstallLink")]
    [SerializeField] private UITexture texDeco;
    [SerializeField] private UISprite sprDecoEmpty;
    [SerializeField] private UILabel labelDecoCount;

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
        texDeco.mainTexture = r;
        texDeco.MakePixelPerfect();

        if (texDeco.mainTexture == null)
        {
            sprDecoEmpty.gameObject.SetActive(true);
            texDeco.gameObject.SetActive(false);
        }
        else
        {
            sprDecoEmpty.gameObject.SetActive(false);
            texDeco.gameObject.SetActive(true);
        }
    }

    public override void UpdataData(DecoItemData cellData)
    {
        decoItem = cellData;

        base.UpdataData(decoItem);

        if (cellData == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        labelDecoCount.text = $"{decoItem.listDecoItem.Count}";
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconHousing", $"{decoItem.housingIndex}_{decoItem.modelIndex}", OnLoadComplete, true);
    }

}
