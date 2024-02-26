using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_WorldRankHallOfFame : UIReuseGridBase
{
    protected override void Awake()
    {
        onInitializeItem += OnInitializeItem;
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopUpWorldRankHallOfFame._instance._listRankingDatas.Count - 1) * -1;
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

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        base.OnInitializeItem(go, wrapIndex, realIndex);

        if (go.activeSelf)
        {
            UIItemWorldRank_UserInfo rankingCell = go.gameObject.GetComponent<UIItemWorldRank_UserInfo>();

            int index = realIndex * -1;

            rankingCell.UpdateData
            (
                UIPopUpWorldRankHallOfFame._instance.GetWorldRankingData(index), 
                UIPopUpWorldRankHallOfFame._instance.CurrentResourceID, 
                index + 1
            );
        }
    }

    public void ScrollReset()
    {
        var scroll = GetComponentInParent<UIScrollView>();

        scroll?.ResetPosition();

        for (int i = 0; i < mChildren.Count; i++)
        {
            mChildren[i].localPosition = Vector3.down * i * itemSize;
        }

        InitReuseGrid();
    }
}
