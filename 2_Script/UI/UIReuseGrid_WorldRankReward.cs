using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_WorldRankReward : UIReuseGridBase
{
    protected override void Awake()
    {
        onInitializeItem += OnInitializeItem;
        
    }

    private void InitReuseGrid()
    {
        minIndex = (UIPopUpWorldRankReward._instance.GetWorldRankRewardDataCount() - 1) * -1;
        StartCoroutine(SetScroll());
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    public void ScrollReset()
    {
        InitReuseGrid();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        base.OnInitializeItem(go, wrapIndex, realIndex);

        if (go.activeSelf)
        {
            UIItemWorldRank_Reward rankingCell = go.gameObject.GetComponent<UIItemWorldRank_Reward>();
            rankingCell.UpdateData(UIPopUpWorldRankReward._instance.GetWorldRankRewardData(realIndex * -1));
        }
    }
}
