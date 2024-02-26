using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialKey : TutorialBase
{
    public List<CustomBlindData> GetCustomBlindData_KeyHole()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        //열쇠 홀 찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {   
            if (tempBlock != null && tempBlock.DecoOnBoard != null)
            {
                for(int i=0; i<tempBlock.DecoOnBoard.Count; i++)
                {
                    KeyExit exit = tempBlock.DecoOnBoard[i] as KeyExit;
                    if (exit == null)
                        continue;

                    CustomBlindData data = new CustomBlindData(exit.transform.localPosition + new Vector3(0f, -50f, 0f), 80, 50);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
}
