using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupWorldRankStageReward : UIPopupBase
{
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel progressValue;

    public void InitData()
    {
        progressBar.value = (float)ManagerWorldRanking.userData.StageClearCount % 10.0f * 0.1f;
        progressValue.text = (ManagerWorldRanking.userData.StageClearCount % 10.0f).ToString();
    }
}
