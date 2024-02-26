using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_MainScroll_ItemRoot : MonoBehaviour
{
    [SerializeField] private GameObject tDecoItemRoot;

    [Header("LandObjectLink")]
    [SerializeField] private UILabel[] labelLandName;

    [Header("UIItemDecoGridLink")]
    [SerializeField] private GameObject decoItemGrid;

    [Header("UIItemDecoLink")]
    [SerializeField] private GameObject prefabProduction;
    [SerializeField] private GameObject prefabInstall;
    [SerializeField] private GameObject prefabSetItem;


    public void UpdateData(int landIndex, List<DecoItemData> decoItems, int index)
    {

        labelLandName[0].text = Global._instance.GetString($"ld_{landIndex}");
        labelLandName[1].text = Global._instance.GetString($"ld_{landIndex}");

        for (int i = 0; i < decoItems.Count; i++)
        {
            UIItemDeco decoItem = SetDecoItem();

            if(decoItem == null) return;

            decoItem.UpdataData(decoItems[i]);
            decoItem.landColorIndex = index;
            decoItem.SetDecoItemSpriteBg();
        }
    }

    public void SetPosition(float yPos)
    {
        this.transform.localPosition = new Vector3(0, -yPos, 0);
    }

    UIItemDeco SetDecoItem()
    {
        switch (UIDiaryDeco.tapType)
        {
            case UIDiaryDeco.DecoTapType.Production:
                return NGUITools.AddChild(decoItemGrid, prefabProduction).GetComponent<UIItemDeco_Production>();
            case UIDiaryDeco.DecoTapType.Install:
                return NGUITools.AddChild(decoItemGrid, prefabInstall).GetComponent<UIItemDeco_Install>();
            case UIDiaryDeco.DecoTapType.SetItem:
                return NGUITools.AddChild(decoItemGrid, prefabSetItem).GetComponent<UIItemDeco_SetItem>();
        }

        return null;
    }
}