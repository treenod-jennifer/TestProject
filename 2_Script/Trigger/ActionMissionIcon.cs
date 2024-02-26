using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public enum TypeMissionIconState
{
    Normal,
    Time,
}

public class ActionMissionIcon : ActionBase
{
    public sChatMission addMission;
    public TypeMissionIconState mode = TypeMissionIconState.Normal;
    public override void DoAction()
    {
        //base.DoAction();

        ManagerLobby._instance.OpenPopupLobbyMission(addMission._missionIndex, addMission._targetTranform.position + addMission._offset);
    }

}
