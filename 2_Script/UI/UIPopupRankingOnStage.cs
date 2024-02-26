using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupRankingOnStage : UIPopupRanking
{
    private int filterStage = -1;

    public void Init(int stageIndex)
    {
        filterStage = stageIndex;
    }

    protected override void InitText()
    {
        string titleText = Global._instance.GetString("p_sm_4").Replace("[n]", filterStage.ToString());
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }
        btnGradeInfoLabel.text = Global._instance.GetString("p_e_rk_8");
    }

    protected override void OnDataComplete()
    {
        StageFiltering(filterStage);
        base.OnDataComplete();
    }

    private void StageFiltering(int stageIndex)
    {
        if (stageIndex < 0) return;

        for (int i = 0; i < _listRankingDatas.Count; i++)
        {
            if (_listRankingDatas[i].UserProfile.stage != stageIndex)
            {
                _listRankingDatas.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < _listUserRanks.Count; i++)
        {
            if (_listUserRanks[i].stage != stageIndex)
            {
                _listUserRanks.RemoveAt(i);
                i--;
            }
        }
    }
}
