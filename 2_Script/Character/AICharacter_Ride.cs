using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{

    [System.NonSerialized]
    public Vector3 _rideStartPosition = Vector3.zero;
    [System.NonSerialized]
    public Vector3 _rideEndPosition = Vector3.zero;

    int rideState = 0;

    [System.NonSerialized]
    public float _rideExitTime = 0f;
    public virtual void OnEnter_Ride(AIChangeCommand in_command)
    {
        _rideExitTime = in_command._moveSpeed;
        _character._speed = 0f;
        rideState = 0;
        PlayAnimation(false, "sitChair_start", WrapMode.Once);
        //PlayQueued("sitChair_loop", QueueMode.CompleteOthers);

   /*     if (in_command._animationName != null)
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
            _animationState.speed = in_command._animationSpeed;*/
    }

    public virtual void OnUpdate_Ride()
    {
        _stateTimer += Time.deltaTime;
        if(rideState == 0)
        {
            if (!_animation.isPlaying)
            {
                rideState = 1;
                PlayAnimation(false, "sitChair_loop", WrapMode.Loop);
            }
            else
            {
                float ratio = _animationState.time / _animationState.length;

                float jump = ManagerLobby._instance.objectRideCurveJump.Evaluate(ratio);
                float move = ManagerLobby._instance.objectRideCurveMove.Evaluate(ratio);
                _character._transform.position = Vector3.Lerp(_rideStartPosition, _rideEndPosition, move) + Vector3.up * jump;
            }
        }
        else if (rideState == 1)
        {
           // if (_character.GetTargetLen() > _character.nextWaypointDistance)

            if (_rideExitTime > 0f)
            {
                if (_stateTimer >= _rideExitTime)
                {
                    rideState = 2;
                    PlayAnimation(false, "sitChair_end", WrapMode.Once);
                }
            }
            else
            {
                if (Global._touchBegin && ManagerLobby._instance._state == TypeLobbyState.Wait)
                {
                    rideState = 2;
                    PlayAnimation(false, "sitChair_end", WrapMode.Once);
                }
            }
         //   if (Input.GetKeyDown(KeyCode.B))
           /* {
                rideState = 2;
                PlayAnimation(false, "sitChair_end", WrapMode.Once);
            }*/
        }
        else if (rideState == 2)
        {

            if (!_animation.isPlaying)
            {
                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eIdle;
                ChangeState(command);
            }
            else
            {
                float ratio = _animationState.time / _animationState.length;

                float jump = ManagerLobby._instance.objectRideCurveJump.Evaluate(1f - ratio);
                float move = ManagerLobby._instance.objectRideCurveMove.Evaluate(1f - ratio);
                _character._transform.position = Vector3.Lerp(_rideStartPosition, _rideEndPosition, move) + Vector3.up * jump;
            }
        }



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

    public virtual void OnLeave_Ride()
    {
        //_addAnimationTag = "";
    }
}
