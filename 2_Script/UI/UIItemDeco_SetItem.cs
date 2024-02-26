using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_SetItem : UIItemDeco
{
    [Header("SetItemLink")]
    [SerializeField] private UITexture texSetItem;
    [SerializeField] private UILabel labelSetItemCount;
    [SerializeField] private UILabel labelSetItemMaxCount;

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
        texSetItem.mainTexture = r;
        texSetItem.MakePixelPerfect();
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

        if(HousingSetInfoUtility.TryGetSetImage(decoItem.setItemIndex, out string imageName))
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconSetHousing", imageName, OnLoadComplete, true);
        }

        if(decoItem.listDecoItem.Count > 0)
        {
            labelSetItemCount.text = GetSetItemHaveCount().ToString();
            labelSetItemMaxCount.text = $"/{decoItem.listDecoItem.Count.ToString()}";
        }
    }

    private int GetSetItemHaveCount()
    {
        int count = 0;

        for(int i = 0; i < decoItem.listDecoItem.Count; i++)
        {
            if(decoItem.listDecoItem[i].active == 1)
            {
                count++;
            }
        }

        return count;
    }
}
