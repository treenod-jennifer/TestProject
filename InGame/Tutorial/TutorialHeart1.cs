using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHeart1 : TutorialBase
{
    public void StartEffect_HeartMoveCountLabel()
    {
        List<BlockHeart> tempHeartList = new List<BlockHeart>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.HEART)
            {
                tempHeartList.Add(tempBlock.Block as BlockHeart);
            }
        }

        if(tempHeartList.Count > 0)
        {
            tempHeartList.Sort((a, b) => a.heartIndex.CompareTo(b.heartIndex));
            BlockHeart tempHeart = tempHeartList[0];

            if(tempHeart != null) StartCoroutine(tempHeart.TutorialStart_ShowMoveCountAction());
        }
    }

    public void StopEffect_HeartMoveCountLabel()
    {
        List<BlockHeart> tempHeartList = new List<BlockHeart>();

        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.HEART)
            {
                tempHeartList.Add(tempBlock.Block as BlockHeart);
            }
        }

        if (tempHeartList.Count > 0)
        {
            tempHeartList.Sort((a, b) => a.heartIndex.CompareTo(b.heartIndex));
            BlockHeart tempHeart = tempHeartList[0];

            if (tempHeart != null) tempHeart.TutorialStop_ShowMoveCountAction();
        }
    }

}
