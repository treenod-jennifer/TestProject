using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureMissionRewardGage : MonoBehaviour {
    [SerializeField] private GameObject root;
    [SerializeField] private UILabel missionCount;
    [SerializeField] private UIProgressBar bar;
    [SerializeField] private GenericReward reward;
    [SerializeField] private GameObject getCheck;

    public void SetRewardBox(int missionCount, int maxMissionCount, List<Reward> rewards)
    {
        if (maxMissionCount <= 0)
        {
            root.SetActive(false);
            return;
        }

        root.SetActive(true);

        float gage = (float)missionCount / maxMissionCount;

        this.bar.value = gage;

        this.missionCount.text = missionCount.ToString() + "/" + maxMissionCount.ToString();

        //this.reward.SetReward(reward);

        this.getCheck.SetActive(Mathf.Approximately(gage, 1.0f));
    }
}
