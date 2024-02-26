using System.Collections.Generic;
using UnityEngine;

public class UIItemLuckyRouletteRatio : MonoBehaviour
{
    [SerializeField] private UISprite spriteBox;
    [SerializeField] private UILabel labelRatio;
    [SerializeField] private GenericReward genericReward;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprLuckyRouletteList;
    
    //아이템, 상태, 비율
    public void Init(Reward reward, LuckyRouletteState state, string ratio)
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprLuckyRouletteList.Count; i++)
        {
            sprLuckyRouletteList[i].atlas =
                ManagerLuckyRoulette.instance.luckyRoulettePack.AtlasUI;
        }
        
        genericReward.SetReward(reward);
        if(state == LuckyRouletteState.RECEIVED_BOX)
        {
            genericReward.EnableCheck(true);
            genericReward.SetColor(Color.gray);
        }
        else
            genericReward.SetColor(Color.white);
        
        if (state == LuckyRouletteState.NORMAL_BOX)
            spriteBox.spriteName = "normal_box";
        else if (state == LuckyRouletteState.SPECIAL_BOX)
            spriteBox.spriteName = "special_box";
        else if (state == LuckyRouletteState.RECEIVED_BOX)
            spriteBox.spriteName = "received_box";
        
        labelRatio.text = ratio;
    }
    
    private void OnDestroy()
    {
        foreach (var spr in sprLuckyRouletteList)
            spr.atlas = null;
    }
}
