using System.Collections.Generic;
using UnityEngine;

public class UIPopUpAdventurePassMission : UIPopupBase
{
    [SerializeField] private List<UIUrlTexture> texMissionIcon;
    [SerializeField] private List<UILabel> labelMissionCount;
    [SerializeField] private List<UILabel> labelMissionShadowCount;
    
    private void OnDestroy()
    {
        base.OnDestroy();
        
        foreach (var icon in texMissionIcon)
            icon.mainTexture = null;
    }
    
    private void Start()
    {
        for (int i = 0; i < texMissionIcon.Count; i++)
        {
            texMissionIcon[i].LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"icon_on_adventure_{ManagerAdventurePass.GetAdventurePassData().resourceIndex}");
            texMissionIcon[i].MakePixelPerfect();
            labelMissionCount[i].text = $"{ManagerAdventurePass.GetAdventurePassData().targetPayCondition[i].value}";
            labelMissionShadowCount[i].text = $"{ManagerAdventurePass.GetAdventurePassData().targetPayCondition[i].value}";
        }
    }
}