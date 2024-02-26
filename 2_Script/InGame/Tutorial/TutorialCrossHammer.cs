using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCrossHammer : TutorialBase
{
    public GameObject GeObj_CrossHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.CROSS_LINE)
                return GameUIManager.instance.listGameItem[i].gameObject;
        }
        return null;
    }

    public List<GameObject> GeObjList_IngameItem_CrossHammer()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.CROSS_LINE)
                listObj.Add(GameUIManager.instance.listGameItem[i].gameObject);
        }
        return listObj;
    }

    public void Touch_CrossHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.CROSS_LINE)
                Touch_UIPokoButton(GameUIManager.instance.listGameItem[i].gameObject);
        }
    }

    public bool IsOpenItemUI()
    {
        if (GameItemManager.instance == null)
            return false;
        else
            return true;
    }

    public bool IsUsedItem()
    {
        if (GameItemManager.instance == null || GameItemManager.instance.used == true)
            return true;
        else
            return false;
    }
}
