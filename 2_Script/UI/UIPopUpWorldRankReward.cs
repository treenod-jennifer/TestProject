using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpWorldRankReward : UIPopupBase
{
    public static UIPopUpWorldRankReward _instance = null;

    [SerializeField] private UIPanel scroll;
    [SerializeField] private UIReuseGrid_WorldRankReward uIReuseGrid;

    public List<WorldRankReward> listReward;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scroll.depth = depth + 1;
    }

    private void Start()
    {
        listReward = ManagerWorldRanking.contentsData.rewards;
        uIReuseGrid.ScrollReset();
    }

    public WorldRankReward GetWorldRankRewardData(int index)
    {
        if (listReward == null) return null;

        if (listReward.Count <= index || index < 0) return null;

        return listReward[index];
    }

    public int GetWorldRankRewardDataCount()
    {
        return listReward.Count;
    }
}
