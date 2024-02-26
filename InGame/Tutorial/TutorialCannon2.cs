using System.Collections.Generic;

public class TutorialCannon2 : TutorialBase
{
    public List<BlockBase> GetBlockBase_Cannon()
    {
        List<BlockBase> listBlockBase = new List<BlockBase>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexY == 3 && tempBoard.Block.indexX >= 2 && tempBoard.Block.indexX <= 8)
                {
                    listBlockBase.Add(tempBoard.Block);
                }
            }
        }
        return listBlockBase;
    }

    public List<CustomBlindData> GetCustomBlindData_Cannon_1()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexY == 2 && tempBoard.Block.indexX >= 1 && tempBoard.Block.indexX <= 4)
                {
                    listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
                else if (tempBoard.Block.indexY == 3 && tempBoard.Block.indexX >= 1 && tempBoard.Block.indexX <= 9)
                {
                    listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
            }
        }

        return listCustomBlindData;
    }
    
    public List<CustomBlindData> GetCustomBlindData_Cannon_2()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexY >= 3 && tempBoard.Block.indexX >= 2 && tempBoard.Block.indexX <= 8)
                {
                    listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
            }
        }

        return listCustomBlindData;
    }
}
