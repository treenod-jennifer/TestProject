using UnityEngine;

public class UIIconBubble : MonoBehaviour
{
    public struct BubbleData
    {
        public string state;
        public Reward reward;
    }

    public GameObject rewardObj;
    public UISprite   rewardSprite;
    public UILabel[]  rewardValue;

    public UILabel stateLabel;

    private BubbleData _data;

    public BubbleData CopyBubble
    {
        get => _data;
        set
        {
            _data = value;

            SetBubble(_data.state);
            SetBubble(_data.reward);
        }
    }

    public void SetBubble(string state)
    {
        if (string.IsNullOrEmpty(state))
        {
            return;
        }

        _data.reward = null;
        rewardObj.SetActive(false);
        stateLabel.gameObject.SetActive(true);

        _data.state      = state;
        stateLabel.text = state;
    }

    public void SetBubble(Reward reward)
    {
        if (reward == null)
        {
            return;
        }

        _data.state = "";
        rewardObj.SetActive(true);
        stateLabel.gameObject.SetActive(false);

        _data.reward = reward;
        RewardHelper.SetRewardImage(reward, rewardSprite, null, rewardValue, 1);
    }
}