using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionHousingActive : ActionBase
{
    [SerializeField] int _housingIndex = 0;
    [SerializeField] bool openPopupAtWakeup;

    public override void DoAction()
    {
        if( _stateType == TypeTriggerState.WakeUp )
        {
            var housing = ManagerHousing.FindHousing(_housingIndex);
            housing.SelectModel(1);

            if (this.openPopupAtWakeup)
            {
                if (ManagerCinemaBox._instance != null)
                    ManagerCinemaBox._instance.SkipEmergencyStop();

                ManagerUI._instance.OpenPopupHousing(this._housingIndex, -1, false, housing.GetHousingFocusPosition(), OnChatComplete);
                bWaitActionFinishOnOff = true;
            }
        }
    }
    
    public void OnChatComplete()
    {
        bActionFinish = true;
    }
}

