using System.Collections.Generic;
using UnityEngine;

public class TutorialSkillItem_Adventure : TutorialBase
{
    public List<CustomBlindData> GetSkillItemPosition()
    {
        var skillItemList       = AdventureManager.instance.skillItemList;
        var skillItemObjectList = new List<CustomBlindData>();

        foreach (var skillItem in skillItemList)
        {
            var targetPos = skillItem.transform.localPosition + new Vector3(0, 5, 0);
            skillItemObjectList.Add(new CustomBlindData(targetPos, 100, 100));
        }

        return skillItemObjectList;
    }

    public List<CustomBlindData> GetItemSlotPosition()
    {
        var skillItemObjectList = new List<CustomBlindData>();
        
        foreach (var ingameItem in GameUIManager.instance.listGameItem)
        {
            if (ingameItem.type == GameItemType.SKILL_HAMMER)
            {
                var targetPos = GameUIManager.instance.transform.InverseTransformPoint(ingameItem.transform.position) + new Vector3(9, -4, 0);
                skillItemObjectList.Add(new CustomBlindData(targetPos, 100, 100));
            }
        }

        return skillItemObjectList;
    }

    public List<GameObject> GetItemSlotAnchor()
    {
        var listObject = new List<GameObject>();

        foreach (var ingameItem in GameUIManager.instance.listGameItem)
        {
            if (ingameItem.type == GameItemType.SKILL_HAMMER)
            {
                listObject.Add(ingameItem.gameObject);
            }
        }

        return listObject;
    }
}