using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterTurnToTarger : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public Transform _target = null;
    Character character;

    public override void DoAction()
    {
        //base.DoAction();

        character = ManagerLobby._instance.GetCharacter(_type);
        Vector3 v = character._transform.position - _target.position;
        if (Vector3.Dot(CameraController.cameraRight, v.normalized) > 0f)
        {
            if (character._ai._bRight)
            {
                character._ai._addAnimationTag = "";
                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eIdleTurn;
                command._nextState = AIStateID.eIdle;
                character._ai.ChangeState(command);
            }
        }
        else
        {
            if (!character._ai._bRight)
            {
                character._ai._addAnimationTag = "";
                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eIdleTurn;
                command._nextState = AIStateID.eIdle;
                character._ai.ChangeState(command);
            }
        }
    }
}
