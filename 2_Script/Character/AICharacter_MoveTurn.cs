using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{
    public virtual void OnEnter_MoveTurn(AIChangeCommand in_command)
    {
        _character._speed = 0f;

        float fFadeTime = 0.1f;
        PlayAnimation(false, "move_turn", WrapMode.Once, fFadeTime);
        _changeTimer = _animationState.length;
        Vector3 modelScale = _character._model.transform.localScale;
        _character._model.transform.localScale = new Vector3(-modelScale.x, modelScale.y, modelScale.z);
        _bRight = !_bRight;
    }

    public virtual void OnUpdate_MoveTurn()
    {
        _stateTimer += Time.deltaTime;
        //if (_changeTimer < _stateTimer)
        if (!_animation.isPlaying)
        {
            AIChangeCommand command = new AIChangeCommand();
            command._state = AIStateID.eMove;
            ChangeState(command);
        }
    }

    public virtual void OnLeave_MoveTurn()
    {
     //   Debug.Log("OnLeave_Idle ");
    }

}
