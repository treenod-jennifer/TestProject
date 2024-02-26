using System.Collections.Generic;
using UnityEngine;

public class UIPopupLuckyRouletteRatio : UIPopupBase
{
    public static UIPopupLuckyRouletteRatio _instance = null;
    [SerializeField] private UIPanel panelScrollView;
    
    [SerializeField] private List<UIItemLuckyRouletteRatio> listUIItemLuckyRouletteRatios;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprLuckyRouletteList;
    private void Awake()
    {
        _instance = this;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        
        base.SettingSortOrder(layer);
        panelScrollView.useSortingOrder = true;
        panelScrollView.sortingOrder = layer + 1;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            
            foreach (var spr in sprLuckyRouletteList)
                spr.atlas = null;
        }
        
        base.OnDestroy();
    }

    public void SetReward(List<RouletteProbability> rouletteProbabilities)
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprLuckyRouletteList.Count; i++)
        {
            sprLuckyRouletteList[i].atlas =
                ManagerLuckyRoulette.instance.luckyRoulettePack.AtlasUI;
        }

        if (rouletteProbabilities != null)
        {
            for (int i = 0; i < rouletteProbabilities.Count; i++)
            {
                //리워드 정보 가져옴
                int sortId = rouletteProbabilities[i].id;
                int rewardIdx = ManagerLuckyRoulette.instance.reward.FindIndex(x => x.id == sortId);
                Reward reward = new Reward
                {
                    value = ManagerLuckyRoulette.instance.reward[rewardIdx].value,
                    type = ManagerLuckyRoulette.instance.reward[rewardIdx].type
                };

                //상태 정보 가져옴
                LuckyRouletteState state;
                if (ManagerLuckyRoulette.instance.rewardState.Contains(ManagerLuckyRoulette.instance.reward[rewardIdx]
                        .id))
                {
                    state = LuckyRouletteState.RECEIVED_BOX;
                }
                else
                {
                    if (ManagerLuckyRoulette.instance.reward[rewardIdx].hot_label == 0) //Hot 리워드 x
                    {
                        state = LuckyRouletteState.NORMAL_BOX;
                    }
                    else //Hot 리워드 o
                    {
                        state = LuckyRouletteState.SPECIAL_BOX;
                    }
                }

                listUIItemLuckyRouletteRatios[i].Init(reward, state, rouletteProbabilities[i].probability);
            }
        }
    }
}
