using UnityEngine;

public class UIItemCapsuleLimitedItem : MonoBehaviour
{
    #region Private
    [SerializeField] private GenericReward genericReward;
    [SerializeField] private GenericReward genericRewardShadow;
    #endregion

    public void UpdateData(LimitReward reward)
    {
        genericRewardShadow.gameObject.SetActive(!ManagerCapsuleGachaEvent.CanBuyLimitedCapsuleItem(reward.goodsId));

        var limitedReward = new Reward
        {
            type  = reward.type,
            value = reward.value
        };

        genericReward.SetReward(limitedReward);
        genericRewardShadow.SetReward(limitedReward);
    }
}
