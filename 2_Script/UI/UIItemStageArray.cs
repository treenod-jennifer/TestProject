using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemStageArray : MonoBehaviour
{
    public UIItemStage[] _arrayBtnStage;

    public void UpdateData(StageUIData[] _arrayStageData)
    {
        //현재 셀에 있는 버튼들 정보 받아와서 세팅.
        for (int i = 0; i < _arrayBtnStage.Length; i++)
        {
            if (gameObject.activeInHierarchy == false)
                return;

            UIPopupStage._instance.SetBoniRun(_arrayStageData[i], _arrayBtnStage[i]._sprBtnBack.transform);
            UIPopupStage._instance.SetComingSoon(_arrayStageData[i], _arrayBtnStage[i]._sprBtnBack.transform);
            UIPopupStage._instance.SetBlueBirdCheer(_arrayStageData[i], _arrayBtnStage[i]._sprBtnBack.transform);

            _arrayBtnStage[i].UpdateData(_arrayStageData[i]);
        }
    }
}
