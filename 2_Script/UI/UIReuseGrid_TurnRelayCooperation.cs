using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_TurnRelayCooperation : UIReuseGridBase
{
    protected override void Awake()
    {
        UIPopupTurnRelay_Cooperation._instance.callbackDataComplete += InitReuseGrid;
        base.Awake();
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopupTurnRelay_Cooperation._instance._listRankingDatas.Count - 1) * -1;
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
        UIItemTurnRelay_Cooperation rankingCell = go.gameObject.GetComponent<UIItemTurnRelay_Cooperation>();
        rankingCell.UpdateData(UIPopupTurnRelay_Cooperation._instance.GetTurnRelayRankData(realIndex * -1)); ;
    }
}
