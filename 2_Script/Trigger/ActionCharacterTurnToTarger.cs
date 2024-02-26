using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterTurn : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public TypeCharacterDir _direction = TypeCharacterDir.None;

    Character character;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterTurn _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);


        if (_direction == TypeCharacterDir.Right)
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
        else if (_direction == TypeCharacterDir.Left)
            if (character._ai._bRight)
            {
                character._ai._addAnimationTag = "";
                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eIdleTurn;
                command._nextState = AIStateID.eIdle;
                character._ai.ChangeState(command);
            }
    }

    void OnDrawGizmosSelected()
    {

    }
}
