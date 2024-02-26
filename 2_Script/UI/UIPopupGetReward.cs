using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupGetReward : UIPopupBase
{
    [Header("Etc")]
    [SerializeField] private UILabel message;
    [SerializeField] private GenericReward reward;
    [SerializeField] private GameObject effect;
    
    [Header("Reduce Time Icon")]
    [SerializeField] private GameObject timeIcon;
    [SerializeField] private UILabel timeText;

    [Header("DepthObject")]
    [SerializeField] private UIPanel rewardPanel;
    [SerializeField] private GameObject particleSystem;

    private void Start()
    {
        _callbackClose += () => { effect.SetActive(false); };
    }

    public void InitReward(Reward reward)
    {
        message.text = Global._instance.GetString("n_s_46");

        timeIcon.SetActive(false);

        this.reward.SetReward(reward);
        this.reward.gameObject.SetActive(true);
    }

    public void InitReduceTime(long time)
    {
        message.text = Global._instance.GetString("n_s_47");

        reward.gameObject.SetActive(false);

        timeText.text = Global.GetTimeText_HHMMSS(time, false);
        timeIcon.SetActive(true);
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        rewardPanel.depth = depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        int startLayer = 0;
        if (layer >= 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            startLayer = layer;
        }
        else
        {
            startLayer = 10;
        }

        SetEffectSortingOrder(startLayer);
        rewardPanel.sortingOrder = startLayer + 1;
    }

    private void SetEffectSortingOrder(int layer)
    {
        ParticleSystem[] arrayEffect = particleSystem.GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < arrayEffect.Length; i++)
        {
            if (i == 4)
                arrayEffect[i].GetComponent<Renderer>().sortingOrder = layer + 3;
            else if (i == 3)
                arrayEffect[i].GetComponent<Renderer>().sortingOrder = layer + 2;
            else
                arrayEffect[i].GetComponent<Renderer>().sortingOrder = layer;
        }
    }
}
