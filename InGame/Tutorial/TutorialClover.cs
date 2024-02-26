using System.Collections.Generic;
using UnityEngine;

public class TutorialClover : TutorialBase
{
    public List<DecoBase> GetListCloverBlock()
    {
        List<DecoBase> listCloverBlock = new List<DecoBase>();

        int boardXMax = ManagerBlock.boards.GetLength(0);
        int boardYMax = ManagerBlock.boards.GetLength(1);
        
        int xMin = Mathf.Min(2,boardXMax);
        int xMax = Mathf.Min(10,boardXMax);
        int yMin = Mathf.Min(2,boardYMax);
        int yMax = Mathf.Min(11,boardYMax);

        //낙엽 블럭 찾기
        for (int i = xMin; i < xMax; i++)
        {
            for (int j = yMin; j < yMax; j++)
            {
                Board tempBoard = ManagerBlock.boards[i, j];
                
                if (tempBoard.BoardOnHide.Count > 0)
                {
                    if (tempBoard.HasDecoHideBlock())
                        listCloverBlock.Add(tempBoard.DecoOnBoard[0]);
                }
            }
        }
        return listCloverBlock;
    }
}
