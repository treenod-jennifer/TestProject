using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFenceBlock : TutorialBase
{
    //돌울타리 튜토리얼 맵에서 돌울타리에 둘러싸인 기믹이 양털 기믹이라 양털 있는 보드 반환
    public List<CustomBlindData> GetCustomeBlindData_Carpet()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.DecoOnBoard != null)
            {
                for (int i = 0; i < tempBlock.DecoOnBoard.Count; i++)
                {
                    Carpet carpet = tempBlock.DecoOnBoard[i] as Carpet;
                    if (carpet == null)
                        continue;

                    CustomBlindData data = new CustomBlindData(carpet.transform.localPosition + new Vector3(0f, 2f, 0f), 115, 115);
                    listCustomBlindData.Add(data);
                }
            }
        }
        return listCustomBlindData;
    }

    public List<DecoBase> GetDecoBase_Fence()
    {
        List<DecoBase> listDecoBase = new List<DecoBase>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.DecoOnBoard != null)
            {
                for (int i = 0; i < tempBlock.DecoOnBoard.Count; i++)
                {
                    FenceBlock fence = tempBlock.DecoOnBoard[i] as FenceBlock;
                    if (fence == null)
                        continue;

                    listDecoBase.Add(fence);
                }
            }
        }
        return listDecoBase;
    }
}
