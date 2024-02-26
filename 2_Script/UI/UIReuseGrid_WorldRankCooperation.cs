using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_WorldRankCooperation : UIReuseGridBase
{
    protected override void Awake()
    {
        UIPopUpWorldRankCooperation._instance.callbackDataComplete += InitReuseGrid;
        base.Awake();
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopUpWorldRankCooperation._instance._listRankingDatas.Count - 1) * -1;
        StartCoroutine(SetScroll());
        return;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemWorldRank_Cooperation rankingCell = go.gameObject.GetComponent<UIItemWorldRank_Cooperation>();
        rankingCell.UpdateData(UIPopUpWorldRankCooperation._instance.GetWorldRankingData(realIndex * -1));
    }

}
