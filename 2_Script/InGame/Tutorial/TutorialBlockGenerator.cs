using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBlockGenerator : TutorialBase
{
    public List<CustomBlindData> GetCustomBlindData_BlockGenerator()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.DecoOnBoard != null)
            {
                for (int i = 0; i < tempBlock.DecoOnBoard.Count; i++)
                {
                    BlockGenerator generator = tempBlock.DecoOnBoard[i] as BlockGenerator;
                    if (generator == null)
                        continue;

                    CustomBlindData data = new CustomBlindData(generator.transform.localPosition + new Vector3(0f, 65f, 0f), 80, 60);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }
}
