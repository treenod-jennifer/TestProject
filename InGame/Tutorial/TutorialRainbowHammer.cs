using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialRainbowHammer : TutorialBase
{
    public GameObject GeObj_RainbowHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.RAINBOW_BOMB_HAMMER)
                return GameUIManager.instance.listGameItem[i].gameObject;
        }
        return null;
    }

    public List<GameObject> GeObjList_IngameItem_RainbowHammer()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.RAINBOW_BOMB_HAMMER)
                listObj.Add(GameUIManager.instance.listGameItem[i].gameObject);
        }
        return listObj;
    }
    public void Touch_RainbowHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.RAINBOW_BOMB_HAMMER)
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
