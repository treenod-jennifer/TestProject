using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_TurnRelaySubMission : UIReuseGridBase
{
    //최상단에 배치될 미션 인덱스
    private int topMissionIndex = 0;

    //서브미션 데이터
    private List<ManagerTurnRelay.SubmissionData> listSubMissionData = new List<ManagerTurnRelay.SubmissionData>();

    //콜백
    private System.Action getRewardAction = null;

    public void InitReuseGrid(int missionCnt, int topIndex, List<ManagerTurnRelay.SubmissionData> datas, System.Action getRewardAction)
    {
        this.getRewardAction = getRewardAction;
        minIndex = missionCnt * -1;
        topMissionIndex = topIndex;
        listSubMissionData.Clear();
        listSubMissionData = datas;
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
            yPos = topMissionIndex * itemSize;

            //아래 7개 없을 때와 있을 때.
            int nNum = (minIndex + topMissionIndex) * -1;
            if (nNum < 6)
            {
                yPos = (topMissionIndex - (7 - nNum)) * itemSize;
            }
        }

        Vector3 _pos = new Vector3(0f, -yPos, 0f);
        SpringPanel.Begin(mPanel.gameObject, -_pos, 8f);

        //팝업 켜지는 시간동안 대기.
        yield return new WaitForSeconds(0.3f);

        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            InitItem(mChildren[i].gameObject);
            UpdateItem(mChildren[i], i);
        }
    }

    private void InitItem(GameObject go)
    {
        go.gameObject.GetComponent<UIItemTurnRelay_SubMission>().InitItem(getRewardAction);
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemTurnRelay_SubMission subMissionCell = go.gameObject.GetComponent<UIItemTurnRelay_SubMission>();
        if (listSubMissionData.Count > realIndex)
        {
            subMissionCell.UpdateData(listSubMissionData[realIndex * -1]);
        }
    }
}
