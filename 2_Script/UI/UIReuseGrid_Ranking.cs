using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Ranking : UIReuseGridBase
{
    protected override void Awake()
    {
        UIPopupRanking._instance.callbackDataComplete += InitReuseGrid;
        base.Awake();
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopupRanking._instance.GetUserRanksCount()) * -1;
        StartCoroutine(SetScroll());
        return;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        //랭킹 셀 위치 세팅.
        float yPos = 0f;

        //친구가 8명 이상일 경우(스크롤 사이즈를 넘는 경우), 위치 설정.
        if (minIndex < -6)
        {
            int index = UIPopupRanking._instance.GetMyIndx();
            if (index - 1 < 0)      // 내 랭킹 위가 없을 경우 내 랭킹을 제일 위로.
                yPos = index * itemSize;
            else if (index - 2 < 0) // 내 랭킹 위 한칸만 있을 경우 내 랭킹 위 한칸을 제일 위로.
                yPos = (index - 1) * itemSize;
            else
            {
                //아래 5개 없을때와 있을 때.
                int nNum = (minIndex + index) * -1;
                if (nNum < 5)
                {
                    //반 짤린 만큼 위로 올려줌.
                    yPos = ((index - (6 - nNum)) * itemSize ) + 100;
                }
                else
                {
                    yPos = (index - 2) * itemSize;
                }
            }
        }

        Vector3 _pos = new Vector3(0f, -yPos, 0f);
        SpringPanel.Begin(mPanel.gameObject, -_pos, 8f);

        //팝업켜지는 시간동안 대기.
        yield return new WaitForSeconds(0.3f);

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
        UIItemRanking rankingCell = go.gameObject.GetComponent<UIItemRanking>();
        rankingCell.UpdateData(UIPopupRanking._instance.GetRankData(realIndex * -1));
    }
}
