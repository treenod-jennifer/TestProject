using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDig : TutorialBase
{
    public List<CustomBlindData> GetCustomBlindData_IngameUI_Dig()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        Vector3 targetPos = GameUIManager.instance.DigGauge_Root.transform.localPosition;
        listCustomBlindData.Add(new CustomBlindData(targetPos, 100, 200));
        return listCustomBlindData;
    }
}
