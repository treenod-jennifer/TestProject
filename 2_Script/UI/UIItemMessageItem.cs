using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemMessageItem : MonoBehaviour
{
    [SerializeField] private GenericReward reward;

    public void cellData(MessageData messageData)
    {
        Reward reward = new Reward();
        reward.type = (int)messageData.type;
        reward.value = messageData.value;

        this.reward.SetReward(reward);
    }
}
