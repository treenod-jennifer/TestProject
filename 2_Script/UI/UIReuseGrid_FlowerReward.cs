using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_FlowerReward : UIReuseGridBase
{
    private int topRewardIndex = 0;
    private Dictionary<int, FlowerRewardUIData> dicFlowerCount_Blue = new Dictionary<int, FlowerRewardUIData>();
   
    public void InitReuseGrid(int chapCnt, int topIndex, Dictionary<int, FlowerRewardUIData> datas)
    {
        minIndex = chapCnt * -1;
        topRewardIndex = topIndex;
        dicFlowerCount_Blue.Clear();
        dicFlowerCount_Blue = datas;
        StartCoroutine(SetScroll());
        return;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        float yPos = 0f;
        //아이템이 스크롤 사이즈를 넘는 경우, 위치 설정.
        if (minIndex < -7)
        {   //기본적으로 젤 상위에 획득가능한 get 버튼이 위로가도록.
            yPos = topRewardIndex * itemSize;
            
            //아래 6개 없을때와 있을 때.
            int nNum = (minIndex + topRewardIndex) * -1;
            if (nNum < 6)
            {
                yPos = (topRewardIndex - (6 - nNum)) * itemSize;
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
        UIItemFlowerReward rewardCell = go.gameObject.GetComponent<UIItemFlowerReward>();
        if (dicFlowerCount_Blue.ContainsKey(realIndex * -1))
        {
            rewardCell.UpdateData(dicFlowerCount_Blue[realIndex * -1]);
        }
    }
}
