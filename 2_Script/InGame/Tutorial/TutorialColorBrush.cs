using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialColorBrush : TutorialBase
{
    public GameObject GetObj_ColorBrush()
    {
        for (var i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.COLOR_BRUSH)
            {
                return GameUIManager.instance.listGameItem[i].gameObject;
            }
        }
        return null;
    }

    public List<GameObject> GetObjList_ColorBrush()
    {
        var listObj = new List<GameObject>();
        for (var i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.COLOR_BRUSH)
            {
                listObj.Add(GameUIManager.instance.listGameItem[i].gameObject);
            }
        }
        return listObj;
    }

    public List<BlockBase> GetBlockList_NormaBlock()
    {
        var listBlock = new List<BlockBase>();

        //일반 블럭 찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock == null || tempBlock.HasDecoCoverBlock() == true)
            {
                continue;
            }

            var block = tempBlock.Block;
            if (block != null && block.IsNormalBlock() && block.type == BlockType.NORMAL)
            {
                listBlock.Add(block);
            }
        }
        return listBlock;
    }

    public void Touch_ColorBrush()
    {
        for (var i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.COLOR_BRUSH)
            {
                Touch_UIPokoButton(GameUIManager.instance.listGameItem[i].gameObject);
            }
        }
    }

    public bool IsOpenItemUI()
    {
        if (GameItemManager.instance == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsUsedItem()
    {
        if (GameItemManager.instance == null || GameItemManager.instance.used == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
