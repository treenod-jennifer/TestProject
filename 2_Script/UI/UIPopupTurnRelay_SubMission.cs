using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupTurnRelay_SubMission : UIPopupBase
{
    [SerializeField] private UIPanel scrollPanel;
    [SerializeField] private GameObject loadTextObj;

    //최상단에 표시될 미션
    [SerializeField] private UIItemTurnRelay_SubMission specialMission;

    //재사용 스크롤
    [SerializeField] private UIReuseGrid_TurnRelaySubMission reuseGrid;

    //서브미션 데이터
    List<ManagerTurnRelay.SubmissionData> listSubMissionData = new List<ManagerTurnRelay.SubmissionData>();

    //보상 콜백
    private System.Action getRewardAction = null;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer <= 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup(System.Action getRewardAction, System.Action endAction)
    {
        //콜백 액션 등록
        this.getRewardAction = getRewardAction;
        this._callbackClose += () => { endAction(); };

        //최상단에 고정될 미션 설정
        InitSpecialMission();

        //서브미션 데이터 및 스크롤 설정
        InitSubMissionData();
        InitSubMissionReuseGrid();
    }

    /// <summary>
    /// 최상단에 고정되어 있을 미션 설정(제일 처음 설정된 미션)
    /// </summary>
    private void InitSpecialMission()
    {
        ManagerTurnRelay.SubmissionData missionData = ManagerTurnRelay.turnRelaySubMission.GetSubmission(0);
        if (missionData == null)
            return;

        specialMission.InitItem(getRewardAction);
        specialMission.UpdateData(missionData);
    }

    private void InitSubMissionData()
    {
        listSubMissionData.Clear();

        int subMissionCount = ManagerTurnRelay.turnRelaySubMission.GetSubmissionCount();
        if (subMissionCount <= 1)
            return;

        for (int i = 1; i < subMissionCount; i++)
        {
            ManagerTurnRelay.SubmissionData tempData = ManagerTurnRelay.turnRelaySubMission.GetSubmission(i);
            if (tempData != null)
                listSubMissionData.Add(tempData);
        }
        loadTextObj.SetActive(false);

        //서브미션 정렬
        SortSubMission();
    }

    private void SortSubMission()
    {
        //미션 상태 순으로 정렬
        listSubMissionData.Sort(delegate (ManagerTurnRelay.SubmissionData a, ManagerTurnRelay.SubmissionData b)
        {
            int aRank = GetSortRank_ByState(a.state);
            int bRank = GetSortRank_ByState(b.state);

            if (aRank < bRank)
                return -1;
            else if (aRank > bRank)
                return 1;
            else //상태가 같다면 진행도 순으로 정렬
            {
                float aRatio = a.progress == 0 ? 0 : ((float)a.progress / (float)a.targetCount);
                float bRatio = b.progress == 0 ? 0 : ((float)b.progress / (float)b.targetCount);

                if (aRatio > bRatio)
                    return -1;
                else if (aRatio < bRatio)
                    return 1;
                else //상태가 같다면 서버에 입력된 데이터 순으로 정렬
                    return a.idx.CompareTo(b.idx);
            }
        });
    }

    //서브미션 상태로 우선순위 가져오는 함수
    private int GetSortRank_ByState(ManagerTurnRelay.SubmissionState state)
    {
        switch (state)
        {
            case ManagerTurnRelay.SubmissionState.COMPLETED:
                return 1;
            case ManagerTurnRelay.SubmissionState.INCOMPLETED:
            case ManagerTurnRelay.SubmissionState.INVALID_STATE:
                return 2;
            case ManagerTurnRelay.SubmissionState.REWARD_FINISHED:
            default:
                return 3;
        }
    }

    private void InitSubMissionReuseGrid()
    {
        reuseGrid.InitReuseGrid((listSubMissionData.Count - 1), 0, listSubMissionData, getRewardAction);
    }
}
