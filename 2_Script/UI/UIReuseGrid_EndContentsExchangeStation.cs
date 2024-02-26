using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_EndContentsExchangeStation : UIReuseGridBase
{
    protected override void Awake()
    {
        onInitializeItem += OnInitializeItem;
    }

    protected override void Start()
    {
        base.Start();
        InitReuseGrid();
    }


    public void InitReuseGrid()
    {
        minIndex = (UIPopupEndContentsEventExchangeStation._instance.GetPriseItemCount() - 1) * -1;
        SetContent();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemEndContents_PriseItemArray rankingCell = go.gameObject.GetComponent<UIItemEndContents_PriseItemArray>();
        rankingCell.UpdateData(UIPopupEndContentsEventExchangeStation._instance.GetEndContentsPriseItemData(realIndex * -1));
    }

    public void ScrollReset()
    {
        InitReuseGrid();
    }
}
