using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSpaceShip : TutorialBase
{
    public List<CustomBlindData> GetCustomBlindData_SpaceShipHole()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach(var tempBlock in ManagerBlock.boards)
        {
            if(tempBlock != null && tempBlock.DecoOnBoard != null)
            {
                for(int i = 0; i< tempBlock.DecoOnBoard.Count; i++)
                {
                    SpaceShipExit exit = tempBlock.DecoOnBoard[i] as SpaceShipExit;
                    if (exit == null)
                        continue;

                    CustomBlindData data = new CustomBlindData(exit.transform.localPosition + new Vector3(0f, -60f, 0f), 78, 60);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
}
