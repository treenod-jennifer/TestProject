using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLineReward : MonoBehaviour
{
    [SerializeField] private UILabel labelLine;
    [SerializeField] private GenericReward reward;
    [SerializeField] private GameObject objCheack;
    [SerializeField] private List<UISprite> spriteList;
    
    private int rewardIndex;
    
    public void InitData(int rewardIndex)
    {
        this.rewardIndex = rewardIndex;

        labelLine.text = Global._instance.GetString("bge_rw_5").Replace("[n]", $"{rewardIndex + 1}");
        reward.SetReward(ManagerBingoEvent.instance.GetLineDecoReward(rewardIndex));
        objCheack.SetActive(ManagerBingoEvent.instance.IsGetLineReward(rewardIndex));
        
        for (int i = 0; i < spriteList.Count; i++)
        {
            spriteList[i].atlas = ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasOutgame;
        }
    }
}
