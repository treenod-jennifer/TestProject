using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupPremiumPassMission : UIPopupBase
{
    [SerializeField] private List<UIUrlTexture> texMissionIcon;
    [SerializeField] private List<UILabel> labelMissionCount;
    [SerializeField] private List<UILabel> labelMissionShadowCount;

    private void Start()
    {
        for (int i = 0; i < texMissionIcon.Count; i++)
        {
            texMissionIcon[i].LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"icon_on_{ManagerPremiumPass.GetPremiumPassResourceIndex()}");
            texMissionIcon[i].MakePixelPerfect();

            labelMissionCount[i].text = $"{ManagerPremiumPass.GetPremiumPassData().targetPayCondition[i].value}";
            labelMissionShadowCount[i].text = $"{ManagerPremiumPass.GetPremiumPassData().targetPayCondition[i].value}";
        }
    }
}
