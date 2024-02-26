using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{
    AIStateID idleTurnNextState = AIStateID.eNone;

    public virtual void OnEnter_IdleTurn(AIChangeCommand in_command)
    {
        _character._speed = 0f;
        idleTurnNextState = AIStateID.eMove;
        float fFadeTime = 0.3f;
        
        if (_doubleSideObject == false)
        {
            PlayAnimation(false, "idle_turn", WrapMode.Once, fFadeTime);
            _changeTimer = _animationState.length;
            Vector3 modelScale = _character._model.transform.localScale;
            _character._model.transform.localScale = new Vector3(-modelScale.x, modelScale.y, modelScale.z);
        }
        else
        {
            PlayAnimation(false, "idle_turn" + (!_bRight ? "_r" : "_l"), WrapMode.Once, fFadeTime);
            _changeTimer = _animationState.length;

        }
        _bRight = !_bRight;

        if (in_command._nextState == AIStateID.eIdle)
            idleTurnNextState = in_command._nextState;
    }

    public virtual void OnUpdate_IdleTurn()
    {
        _stateTimer += Time.deltaTime;
        if (_changeTimer < _stateTimer)
        {
            AIChangeCommand command = new AIChangeCommand();
            command._state = idleTurnNextState;
            ChangeState(command);
        }
    }

    public virtual void OnLeave_IdleTurn()
    {
     //   Debug.Log("OnLeave_Idle ");
    }

}
