using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{
    private FSMSystem<Character> _fsm = new FSMSystem<Character>();
    private Character _character;

    [System.NonSerialized]
    public string _addAnimationTag = "";
    private Animation _animation;
    private AnimationState _animationState;
    private string _animationName = "";
    public bool _bRight = false;
    private float _fFadeTime = 0.3f;

    [System.NonSerialized]
    bool _doubleSideObject = false;

    [System.NonSerialized]
    public bool canRun = true;

    [System.NonSerialized]
    float aniSpeedMultiplier = 1f;
    float aniSpeed = 1f;

    public void AIStart(Character in_character, Animation in_animation)
    {
        _doubleSideObject = in_animation.GetClip("idle_r") != null;

        _character = in_character;
        _animation = in_animation;
        _fsm.Clear();

        _fsm.AddState(new FSMState<Character>(in_character, AIStateID.eEvent, OnEnter_Event, OnUpdate_Event, OnLeave_Event));
        _fsm.AddState(new FSMState<Character>(in_character, AIStateID.eIdle, OnEnter_Idle, OnUpdate_Idle, OnLeave_Idle));
        _fsm.AddState(new FSMState<Character>(in_character, AIStateID.eMove, OnEnter_Move, OnUpdate_Move, OnLeave_Move));
        _fsm.AddState(new FSMState<Character>(in_character, AIStateID.eIdleTurn, OnEnter_IdleTurn, OnUpdate_IdleTurn, OnLeave_IdleTurn));
        _fsm.AddState(new FSMState<Character>(in_character, AIStateID.eRide, OnEnter_Ride, OnUpdate_Ride, OnLeave_Ride));

        //_fsm.AddState(new FSMState<Character>(in_character, AIStateID.eMoveTurn, OnEnter_MoveTurn, OnUpdate_MoveTurn, OnLeave_MoveTurn)); // 삭제
        /*_fsm.AddState(new FSMState<PC>(_pc, AIStateID.eEvent, OnEnter_Event, OnUpdate_Event, OnLeave_Event));
        _fsm.AddState(new FSMState<PC>(_pc, AIStateID.eRide_Start, OnEnter_RideStart, OnUpdate_RideStart, OnLeave_RideStart));
        _fsm.AddState(new FSMState<PC>(_pc, AIStateID.eRide_Loop, OnEnter_RideLoop, OnUpdate_RideLoop, OnLeave_RideLoop));
        _fsm.AddState(new FSMState<PC>(_pc, AIStateID.eRide_End, OnEnter_RideEnd, OnUpdate_RideEnd, OnLeave_RideEnd));
        _fsm.AddState(new FSMState<PC>(_pc, AIStateID.ePeriodWarp, OnEnter_PeriodWarp, OnUpdate_PeriodWarp, OnLeave_PeriodWarp));*/
    }

    public void BodyTurn(TypeCharacterDir in_dir)
    {
        if( _doubleSideObject )
        {
            _bRight = !_bRight;
        }
        else
        {
            if (in_dir == TypeCharacterDir.Left && _bRight)
            {
                Vector3 modelScale = _character._model.transform.localScale;
                _character._model.transform.localScale = new Vector3(-modelScale.x, modelScale.y, modelScale.z);
                _bRight = !_bRight;
            }
            else if (in_dir == TypeCharacterDir.Right && !_bRight)
            {
                Vector3 modelScale = _character._model.transform.localScale;
                _character._model.transform.localScale = new Vector3(-modelScale.x, modelScale.y, modelScale.z);
                _bRight = !_bRight;
            }
        }
    }
    //////////////////////////////////////////////////////////////////////////
    /// Animation
    public void PlayAnimation(bool bUseCroseFade, string aniname, WrapMode mode, float fadetime = 0.2f,float in_speed = 1f)
    {
        if (bUseCroseFade)
            _animation.CrossFade(aniname, fadetime);
        else
        {
            _animation.Stop();
            if( _doubleSideObject && _bRight && _animation.GetClip(aniname + "_r") != null)
            {
                aniname = aniname += "_r";
            }

            _animation.Play(aniname);
        }
        _animation.wrapMode = mode;
        _animationName = aniname;
        _animationState = _animation[_animationName];

        aniSpeed = in_speed;

        _animationState.speed = aniSpeed * aniSpeedMultiplier;
    }
    public void SetAnmationSpeed(float fSpeed)
    {
        if (_animationState == null)
            return;
        aniSpeed = fSpeed;
        _animationState.speed = aniSpeed * aniSpeedMultiplier;
        //        _globalAnimSpeedAdjust = fSpeed;
    }

    public void ChangeState(AIChangeCommand in_command)
    {
        _stateTimer = 0f;
        _fsm.ChangeState(in_command);
    }

    public void AIUpdate()
    {
        if( aniSpeedMultiplier != Global.timeScaleLobby )
        {
            aniSpeedMultiplier = Global.timeScaleLobby;

            SetAnmationSpeed(aniSpeed);
        }

        _fsm.Update();
    }

    public AIStateID GetStateID()
    {
        return _fsm._currentStateID;
    }

}
