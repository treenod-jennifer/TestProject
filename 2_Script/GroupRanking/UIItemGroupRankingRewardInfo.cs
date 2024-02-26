using System.Collections.Generic;
using UnityEngine;

public class UIItemGroupRankingRewardInfo : MonoBehaviour
{
    [SerializeField] private UILabel    _labelRank;
    [SerializeField] private UIGrid     _grid;
    [SerializeField] private GameObject _rewardItem;

    public void Init(string rank, List<Reward> rewardData)
    {
        _labelRank.text = Global._instance.GetString("p_grk_if_2").Replace("[n]", rank);

        foreach (var data in rewardData)
        {
            var reward = _grid.transform.AddChild(_rewardItem).GetComponent<GenericReward>();
            if (reward != null)
            {
                reward.gameObject.SetActive(true);
                reward.SetReward(data);
            }
        }

        _grid.enabled = true;
    }
}