using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBread : TutorialBase
{
    public List<BlockBase> GetBlockBase_Bread_GIndex_1()
    {
        List<BlockBase> listBlockBase = new List<BlockBase>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null && tempBoard.Block.type == BlockType.BREAD)
            {
                BlockBread bread = tempBoard.Block as BlockBread;

                if (bread.GetGroupIndex() == 1 && bread.IsCanPang() == true)
                {
                    listBlockBase.Add(bread);
                }
            }
        }
        return listBlockBase;
    }

    public List<CustomBlindData> GetCustomBlindData_Bread_GIndex_1()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null && tempBoard.Block.type == BlockType.BREAD)
            {
                BlockBread bread = tempBoard.Block as BlockBread;

                if (bread.GetGroupIndex() == 1 && bread.IsCanPang() == true)
                {
                    CustomBlindData data = new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
}
