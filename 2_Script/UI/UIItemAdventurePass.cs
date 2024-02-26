using UnityEngine;

public class UIItemAdventurePass : MonoBehaviour
{
    public enum RewardState
    {
        LOCK,
        OPEN_REWARD,
        GET_REWARD,
    }

    [Header("Reward")]
    [SerializeField] private UIItemAdventurePassReward nReward;
    [SerializeField] private UIItemAdventurePassReward pReward;
    [SerializeField] private GameObject pRewardBlock;
    
    [Header("Mission")]
    [SerializeField] private UIUrlTexture texMissionIcon;
    [SerializeField] private UILabel[] labelMissionCount;
    [SerializeField] private GameObject objActiveMissionBg;
    [SerializeField] private GameObject objMissionBg;
    
    public void InitData(int index)
    {
        string active = ManagerAdventurePass.IsMissionActive(index) ? "on" : "off";
        
        texMissionIcon.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"icon_{active}_adventure_{ManagerAdventurePass.GetAdventurePassData().resourceIndex}");
        texMissionIcon.MakePixelPerfect();

        labelMissionCount.SetText($"{ManagerAdventurePass.GetAdventurePassData().targetCount[index]}");

        objActiveMissionBg.SetActive(ManagerAdventurePass.IsMissionActive(index));
        objMissionBg.SetActive(ManagerAdventurePass.IsMissionActive(index) == false);

        nReward.SetReward(index, false);
        pReward.SetReward(index, true);
    }

    public void SetRewardState(int index)
    {
        nReward.SetRewardState(ManagerAdventurePass.GetRewardState(index, false));
        pReward.SetRewardState(ManagerAdventurePass.GetRewardState(index, true));
    }

    public void SetPremiumRewardBlock(bool isActive)
    {
        pRewardBlock.SetActive(isActive);
    }
}