using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemPremiumPass : MonoBehaviour
{
    public enum RewardState
    {
        LOCK,
        OPEN_REWARD,
        GET_REWARD,
    }

    [Header("Reward")]
    [SerializeField] private UIItemPremiumPassReward nReward;
    [SerializeField] private UIItemPremiumPassReward pReward;
    [SerializeField] private GameObject pRewardBlock;
    
    [Header("Mission")]
    [SerializeField] private UIUrlTexture texMissionIcon;
    [SerializeField] private UILabel[] labelMissionCount;
    [SerializeField] private GameObject objActiveMissionBg;
    [SerializeField] private GameObject objMissionBg;
    

    public void InitData(int index)
    {
        string active = ManagerPremiumPass.IsMissionActive(index) ? "on" : "off";

        texMissionIcon.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"icon_{active}_{ManagerPremiumPass.GetPremiumPassResourceIndex()}");
        texMissionIcon.MakePixelPerfect();

        labelMissionCount.SetText($"{ManagerPremiumPass.GetPremiumPassData().targetCount[index]}");

        objActiveMissionBg.SetActive(ManagerPremiumPass.IsMissionActive(index));
        objMissionBg.SetActive(ManagerPremiumPass.IsMissionActive(index) == false);

        nReward.SetReward(index, false);
        pReward.SetReward(index, true);
    }

    public void SetRewardState(int index)
    {
        nReward.SetRewardState(ManagerPremiumPass.GetRewardState(index, false));
        pReward.SetRewardState(ManagerPremiumPass.GetRewardState(index, true));
    }

    public void SetPremiumRewardBlock(bool isActive)
    {
        pRewardBlock.SetActive(isActive);
    }
}