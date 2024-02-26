using System.Collections.Generic;

public class TutorialWaterBomb : TutorialBase
{
    public List<BlockBase> GetBlockBase_WaterBomb_1()
    {
        List<BlockBase> listBlockBase = new List<BlockBase>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexX == 5 && tempBoard.Block.indexY >= 5 && tempBoard.Block.indexY <= 6)
                {
                    listBlockBase.Add(tempBoard.Block);
                }
            }
        }
        return listBlockBase;
    }

    public List<CustomBlindData> GetCustomBlindData_WaterBomb_1()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block != null)
            {
                if (tempBoard.Block.indexX == 5 && tempBoard.Block.indexY >= 5 && tempBoard.Block.indexY <= 7)
                {
                    listCustomBlindData.Add(new CustomBlindData(tempBoard.Block.transform.localPosition, 85, 85));
                }
            }
        }

        return listCustomBlindData;
    }

    public List<DecoBase> GetDecoBase_WaterBomb_2()
    {
        List<DecoBase> listDecoBaseData = new List<DecoBase>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.DecoOnBoard != null && tempBoard.DecoOnBoard.Count > 0)
            {
                foreach (DecoBase tempWater in tempBoard.DecoOnBoard)
                {
                    if (tempWater is Water)
                        listDecoBaseData.Add(tempWater);
                }
            }
        }

        return listDecoBaseData;
    }

    public List<CustomBlindData> GetCustomBlindData_WaterBomb_2()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.DecoOnBoard != null && tempBoard.DecoOnBoard.Count > 0)
            {
                foreach (DecoBase tempWater in tempBoard.DecoOnBoard)
                {
                    if (tempWater is Water)
                        listCustomBlindData.Add(new CustomBlindData(tempWater.transform.localPosition, 85, 85));
                }
            }
        }

        return listCustomBlindData;
    }
}
