using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{
    readonly float sWalkSpeed = 3f;
    readonly float sRunSpeed = 7f;

    private void runAniCheck()
    {

        if (_character.GetTargetLen() < 3f || _addAnimationTag.Length > 0)
        {
            if (_character._runDust != null)
                if (_character._runDust.isPlaying == true)
                    _character._runDust.Stop();

            PlayAnimation(false, "run" + _addAnimationTag, WrapMode.Loop, _fFadeTime);

            if (_character._speed == sRunSpeed)
                LobbyBehavior._instance.CompleteRun();

            _character._speed = sWalkSpeed;
        }
        else
        {

            if (_character._runDust != null)
                if (_character._runDust.isPlaying == false)
                    _character._runDust.Play();

            PlayAnimation(false, "run_fast", WrapMode.Loop, _fFadeTime);
            _character._speed = sRunSpeed;
        }
        
        _changeTimer = _animationState.length;
    }

    public virtual void OnEnter_Move(AIChangeCommand in_command)
    {
        _fFadeTime = 0.3f;
        

        if (in_command._animationName != null)
            if (in_command._animationName.Length > 0)
                _addAnimationTag = in_command._animationName;
        
        runAniCheck();

        if (in_command._animationSpeed > 0f)
            _animationState.speed = in_command._animationSpeed;
        if (in_command._moveSpeed > 0f)
        {
            _character._speed = in_command._moveSpeed;
            if (in_command._moveSpeed >= 4f)
            {
                if (_character._runDust != null)
                    if (_character._runDust.isPlaying == false)
                        _character._runDust.Play();
            }
        }
    }

    float stepSound = 0f;
    public virtual void OnUpdate_Move()
    {
        _stateTimer += Global.deltaTimeLobby;

        stepSound += _character._speed * Global.deltaTimeLobby;
        if (stepSound > 1f)
        {
            stepSound = 0f;
         //   ManagerSound.AudioPlay(AudioLobby.BoniStep);
        }

        if (_character.GetTargetLen() < _character.nextWaypointDistance)
        {
            _character.StopPath();
 //           _pc.moveType = MoveStyle.NORMAL;
            AIChangeCommand command = new AIChangeCommand();
            command._state = AIStateID.eIdle;
            ChangeState(command);
        }
        else
        {
            if (_stateTimer > _changeTimer && _addAnimationTag.Length == 0)
            {
                runAniCheck();
                _stateTimer = 0f;
            }

            if (_character.GetPathLen() > 0.1f)
            {
                float fDot = Vector3.Dot(CameraController._instance._transform.right, _character.GetPathDir());
                bool bDot = (fDot > 0) ? true : false;
                if (_bRight != bDot)
                {
                   // Debug.Log(fDot);
                    /*if (Mathf.Abs(fDot) > 0.5f)
                    {
                        AIChangeCommand command = new AIChangeCommand();
                        command._state = AIStateID.eMoveTurn;
                        ChangeState(command);
                    }
                    else*/
                    {
                        Vector3 modelScale = _character._model.transform.localScale;
                        _character._model.transform.localScale = new Vector3(-modelScale.x, modelScale.y, modelScale.z);
                        _bRight = !_bRight;
                    }
                }
            }
            
        }
    }

    public virtual void OnLeave_Move()
    {
        if (_character._runDust != null)
            if (_character._runDust.isPlaying == true)
                _character._runDust.Stop();
    }

}
