using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPeaBoss : TutorialBase
{
    public List<BlockBase> GetBlockBase_PeaBoss_Open()
    {
        List<BlockBase> listBlockBase = new List<BlockBase>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.PEA_BOSS)
            {
                BlockPeaBoss boss = tempBlock.Block as BlockPeaBoss;

                if (boss.IsCanPang() == true)
                {
                    listBlockBase.Add(boss);
                }
            }
        }
        return listBlockBase;
    }
    public List<CustomBlindData> GetCustomBlindData_PeaBoss_Open()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.PEA_BOSS)
            {
                BlockPeaBoss boss = tempBlock.Block as BlockPeaBoss;

                if(boss.IsCanPang() == true)
                {
                    CustomBlindData data = new CustomBlindData(tempBlock.Block.transform.localPosition, 85, 85);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
    public List<CustomBlindData> GetCustomBlindData_PeaBoss_Close()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.PEA_BOSS)
            {
                BlockPeaBoss boss = tempBlock.Block as BlockPeaBoss;

                if (boss.IsCanPang() == false)
                {
                    CustomBlindData data = new CustomBlindData(tempBlock.Block.transform.localPosition, 85, 85);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
}
