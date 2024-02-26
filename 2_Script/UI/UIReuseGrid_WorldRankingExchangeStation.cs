using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_WorldRankingExchangeStation : UIReuseGridBase
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
        minIndex = (UIPopUpWorldRankExchangeStation._instance.GetPriseItemCount() - 1) * -1;
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
        UIItmeWorldRank_PriseItemArray rankingCell = go.gameObject.GetComponent<UIItmeWorldRank_PriseItemArray>();
        rankingCell.UpdateDate(UIPopUpWorldRankExchangeStation._instance.GetWorldRankPriseItemData(realIndex * -1));
    }

    public void ScrollReset()
    {
        InitReuseGrid();
    }
}
