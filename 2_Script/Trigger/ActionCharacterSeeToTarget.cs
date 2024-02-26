using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterSeeToTarget : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public Transform _target = null;

    Character character;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterSeeToTarget _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);

        Vector3 v = character._transform.position - _target.position;
        if (Vector3.Dot(CameraController.cameraRight, v.normalized) > 0f)
        {
            character._ai.BodyTurn(TypeCharacterDir.Left);
        }
        else
        {
            character._ai.BodyTurn(TypeCharacterDir.Right);
        }
    }
}
