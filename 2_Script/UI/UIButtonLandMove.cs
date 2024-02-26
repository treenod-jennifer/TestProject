using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonLandMove : UIButtonEventBase
{
    [SerializeField] private GameObject alarmNewLand;
    
    public void SetAlarmNewLand()
    {
        var outlandData = ServerContents.Day.outlands;

        foreach (var outlandItem in outlandData)
        {
            if (outlandItem.Value.Count > 0)
            {
                if (PlayerPrefs.HasKey($"VisitedLand_{outlandItem.Key}") == false)
                {
                    if (outlandItem.Key > 0)
                    {
                        alarmNewLand.SetActive(true);
                        return;
                    }
                }
            }
        }
        alarmNewLand.SetActive(false);
    }

    protected override void OnClickBtnEvent()
    {
        if(ManagerUI.IsLobbyButtonActive)
        {
            if (IsDefaultLand() > 2)
                ManagerUI._instance.OpenPopup<UIPopupLandMove_Scroll>();
            else
            {
                ManagerUI._instance.OpenPopup<UIPopUpLandMove_Small>((popup) => popup.SetLandItemPosition(IsDefaultLand()));
            }
        }
    }

    protected int IsDefaultLand()
    {
        var outlandData = ServerContents.Day.outlands;

        int LandEpisodeCount = 0;
        int LandHousingCount = 0;

        foreach (var outlandItme in outlandData)
        {
            if(outlandItme.Key > 99)
            {
                LandHousingCount++;
            }
            else
            {
                LandEpisodeCount++;
            }
        }

        int maxLandCount = LandEpisodeCount > LandHousingCount ? LandEpisodeCount : LandHousingCount;

        return maxLandCount;
    }
}
