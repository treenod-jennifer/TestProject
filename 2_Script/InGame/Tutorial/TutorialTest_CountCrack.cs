using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTest_CountCrack : TutorialBase
{
    public List<CustomBlindData> GetTargetBlindData()
    {
        List<CustomBlindData> listBlindData = new List<CustomBlindData>();
        listBlindData.Add(new CustomBlindData(GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(2f, 25f, 0f), 425, 170));
        return listBlindData;
    }

    public List<CustomBlindData> GetTargetBlindData_2()
    {
        List<CustomBlindData> listBlindData = new List<CustomBlindData>();
        listBlindData.Add(new CustomBlindData(GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(-90f, 25f, 0f), 200, 170));
        return listBlindData;
    }

    public List<GameObject> GetTargetObjects()
    {
        List<GameObject> listOjbect = new List<GameObject>();
        for(int i=0; i< GameUIManager.instance.listGameTarget.Count; i++)
        {
            listOjbect.Add(GameUIManager.instance.listGameTarget[i].gameObject);
        }
        return listOjbect;
    }
}
