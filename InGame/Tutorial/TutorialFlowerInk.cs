using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlowerInk : TutorialBase
{
    public List<CustomBlindData> GetListCustomBlindData_FlowerInkArea()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block == null && tempBoard.DecoOnBoard != null)
            {
                for (int i = 0; i < tempBoard.DecoOnBoard.Count; i++)
                {
                    FlowerInk flowerInk = tempBoard.DecoOnBoard[i] as FlowerInk;
                    if (flowerInk == null)
                        continue;

                    CustomBlindData data = new CustomBlindData(flowerInk.transform.localPosition + new Vector3(2f, 2f, 0f), 90, 85);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }

    public List<DecoBase> GetListFlowerInk()
    {
        List<DecoBase> listFlowerInk = new List<DecoBase>();

        //잉크 찾기
        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.Block == null && tempBoard.DecoOnBoard != null)
            {
                for (int i = 0; i < tempBoard.DecoOnBoard.Count; i++)
                {
                    FlowerInk flowerInk = tempBoard.DecoOnBoard[i] as FlowerInk;
                    if (flowerInk != null)
                    {
                        listFlowerInk.Add(flowerInk);
                    }
                }
            }
        }
        return listFlowerInk;
    }

    public List<CustomBlindData> GetCustomBlindData_IngameUI_Target_1()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        Vector3 targetPos = GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(-100f, 25f, 0f);
        listCustomBlindData.Add(new CustomBlindData(targetPos, 150, 150));
        return listCustomBlindData;
    }
}
