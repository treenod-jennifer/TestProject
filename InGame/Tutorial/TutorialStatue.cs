using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStatue : TutorialBase
{
    public List<DecoBase> GetListGrass()
    {
        List<DecoBase> listDeco = new List<DecoBase>();

        //잔디 찾기
        for (int i = 0; i < 2; i++)
        {
            Board tempBoard = ManagerBlock.boards[7, 2 + i];
            if (tempBoard != null && tempBoard.DecoOnBoard != null)
            {
                for (int deco = 0; deco < tempBoard.DecoOnBoard.Count; deco++)
                {
                    Grass grass = tempBoard.DecoOnBoard[deco] as Grass;
                    if (grass != null)
                        listDeco.Add(grass);
                }
            }
        }
        return listDeco;
    }
}
