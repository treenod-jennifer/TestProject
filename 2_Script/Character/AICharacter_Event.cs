using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{

    public virtual void OnEnter_Event(AIChangeCommand in_command)
    {
        _character._speed = 0f;

        if (in_command._animationName != null)
        {
            if (in_command._animationName.Length > 0)
            {
                PlayAnimation(false, in_command._animationName, WrapMode.Loop, 0.1f);
            }
            else
            {
                PlayAnimation(false, "idle", WrapMode.Loop, 0.1f);
            }
        }
        else
        {
            PlayAnimation(false, "idle", WrapMode.Loop, 0.1f);
        }

        if (in_command._animationSpeed > 0f)
            _animationState.speed = in_command._animationSpeed;
    }

    public virtual void OnUpdate_Event()
    {




     /*   if (ManagerLobby._instance._state == TypeLobbyState.Wait)
        {
            if (_character.GetTargetLen() > _character.nextWaypointDistance)
            {
                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eMove;

                float fDot = Vector3.Dot(CameraController._instance._transform.right, _character.GetPathDir());
                bool bDot = (fDot > 0) ? true : false;
                if (_character._ai._bRight != bDot)
                    command._state = AIStateID.eIdleTurn;

                ChangeState(command);
            }
        }
        */




       // Debug.Log("OnUpdate_Idle ");
    }

    public virtual void OnLeave_Event()
    {
        //_addAnimationTag = "";
    }

}
