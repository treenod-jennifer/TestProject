using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterRideExit : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    Character character;
    public override void DoAction()
    {
        character = ManagerLobby._instance.GetCharacter(_type);

        character._ai._rideExitTime = 0.01f;
    }
}
