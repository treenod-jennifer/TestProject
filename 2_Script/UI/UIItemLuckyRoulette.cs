using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum LuckyRouletteState
{
    NORMAL_BOX,
    SPECIAL_BOX,
    RECEIVED_BOX
}
public class UIItemLuckyRoulette : MonoBehaviour
{
    [SerializeField] private UISprite spriteBox;
    [SerializeField] private UISprite[] spriteHighlight;
    [SerializeField] private GenericReward genericReward;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprLuckyRouletteList;
    
    public void Init()
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprLuckyRouletteList.Count; i++)
        {
            sprLuckyRouletteList[i].atlas =
                ManagerLuckyRoulette.instance.luckyRoulettePack.AtlasUI;
        }
    }
    
    private void OnDestroy()
    {
        foreach (var spr in sprLuckyRouletteList)
            spr.atlas = null;
    }
    
    public void SetIcon(LuckyRouletteState state)
    {
        if (state == LuckyRouletteState.NORMAL_BOX)
            spriteBox.spriteName = "normal_box";
        else if (state == LuckyRouletteState.SPECIAL_BOX)
            spriteBox.spriteName = "special_box";
        else if (state == LuckyRouletteState.RECEIVED_BOX)
            spriteBox.spriteName = "received_box";

        if (state == LuckyRouletteState.RECEIVED_BOX)
        {
            genericReward.SetColor(Color.gray);
        }
        else
            genericReward.SetColor(Color.white);
    }

    public IEnumerator SetHighLight(float time)
    {
        foreach (var sprite in spriteHighlight)
            DOTween.ToAlpha(() => sprite.color, x => sprite.color = x, 1, time * 0.2f);
        yield return new WaitForSeconds(time);
        
        foreach (var sprite in spriteHighlight)
            DOTween.ToAlpha(() => sprite.color, x => sprite.color = x, 0, time * 0.8f);
    }

    public void SetReward(Reward reward)
    {
        genericReward.SetReward(reward);
    }
}
