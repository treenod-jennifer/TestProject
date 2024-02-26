using System.Collections.Generic;

public class TutorialCannon1 : TutorialBase
{
    public List<BlockBase> GetBlockBase_Cannon_1()
    {
        List<BlockBase> listBlockBase = new List<BlockBase>();
        listBlockBase.Add(PosHelper.GetBoard(9, 2).Block);
        listBlockBase.Add(PosHelper.GetBoard(9, 10).Block);
        
        return listBlockBase;
    }

    public List<CustomBlindData> GetCustomBlindData_Cannon_1()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 2).Block.transform.localPosition, 85, 85));
        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 10).Block.transform.localPosition, 85, 85));

        return listCustomBlindData;
    }
    
    public List<CustomBlindData> GetCustomBlindData_Cannon_2()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexX == 6 && tempBoard.Block.indexY >= 4 && tempBoard.Block.indexY <= 8)
                {
                    listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
            }
        }

        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 2).Block.transform.localPosition, 85, 85));
        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 10).Block.transform.localPosition, 85, 85));

        return listCustomBlindData;
    }

    public List<CustomBlindData> GetCustomBlindData_Cannon_3()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexX >= 4 && tempBoard.Block.indexX <= 6 && tempBoard.Block.indexY >= 4 && tempBoard.Block.indexY <= 8)
                {
                    if (tempBoard.Block.colorType == BlockColorType.C && (tempBoard.Block.leftLinker != null || tempBoard.Block.rightLinker != null || tempBoard.Block.upLinker != null || tempBoard.Block.DownLinker != null))
                        listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
            }
        }

        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 2).Block.transform.localPosition, 85, 85));
        listCustomBlindData.Add(new CustomBlindData(PosHelper.GetBoard(9, 10).Block.transform.localPosition, 85, 85));

        return listCustomBlindData;
    }
}
