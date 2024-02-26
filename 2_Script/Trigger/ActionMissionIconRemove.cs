using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionMissionIconRemove : ActionBase
{
    public override void DoAction()
    {
        ObjectMissionIcon.RemoveMissionIcon(ManagerLobby._instance.lateClearMission);
        //base.DoAction();

        //MissionData missionData = ManagerData._instance.missionData[addMission._missionIndex - 1];
        //ManagerLobby._instance.OpenPopupLobbyMission(missionData, addMission._targetTranform.position + addMission._offset);
    }

}
