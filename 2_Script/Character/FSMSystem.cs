using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum AIStateID
{
    eNone,
    eAppear,
    eIdle,
    eTouch,
    eMove,
    eIdleTurn,

    eEvent,
    eRide,
    eRide_Start,
    eRide_Loop,
    eRide_End,
    eIntaracionEnvObject,
    eChase,
    ePeriodWarp,
}

public struct AIChangeCommand
{
    public AIStateID _state;
    public AIStateID _nextState;
    public AICharacter _other;       // 상태변화를 시킨사람(공격자,NPC,도우미)

    public float _moveSpeed;
    public float _animationSpeed;


    public string _animationName;

}

public class FSMFunction<T>
{
    private T _owner;

    private Method.FunctionTemplate<AIChangeCommand> _enter = null;
    private Method.FunctionVoid _update = null;
    private Method.FunctionVoid _leave = null;

    public Method.FunctionTemplate<AIChangeCommand> Enter
    {
        get { return _enter; }
    }

    public Method.FunctionVoid Update
    {
        get { return _update; }
    }

    public Method.FunctionVoid Leave
    {
        get { return _leave; }
    }

    public FSMFunction(T owner, Method.FunctionTemplate<AIChangeCommand> funcEnter, Method.FunctionVoid funcUpdate, Method.FunctionVoid funcLeave)
    {
        _owner = owner;
        _enter = funcEnter;
        _update = funcUpdate;
        _leave = funcLeave;
    }
}


//상태 정보 데이터
public class FSMState<T>
{
    public T _owner { get; protected set; }
    
    public FSMFunction<T> _fsmFunction = null;
    public float _enterTime = 0f;
    public float _leaveTime = 0f;


    protected AIStateID _id = AIStateID.eNone;
    public AIStateID ID { get { return _id; } }

    public FSMState(T owner)
    {
        this._owner = owner;
    }

    public FSMState(T owner, AIStateID id, Method.FunctionTemplate<AIChangeCommand> funcEnter, Method.FunctionVoid funcUpdate, Method.FunctionVoid funcLeave)
    {
        this._owner = owner;
        this._id = id;
        this._fsmFunction = new FSMFunction<T>(owner, funcEnter, funcUpdate, funcLeave);
    }
}
public class FSMSystem<T>
{
    public List<FSMState<T>> _states = new List<FSMState<T>>();


    public AIStateID _prevStateID = AIStateID.eNone;
    public AIStateID _currentStateID = AIStateID.eNone;
    public FSMState<T> _currentState = null;
    AIStateID _nextStateID = AIStateID.eNone;
    AIChangeCommand _command;

    public float _nextStateParam_float { get; private set; }
    public void SetNextStateParam_Float(float fParam)
    { _nextStateParam_float = fParam; }

    public float _nextStateParam_float_2 { get; private set; }
    public void SetNextStateParam_Float_2(float fParam)
    { _nextStateParam_float_2 = fParam; }

    public Vector3 _nextStateParam_Vector3 { get; private set; }
    public void SetNextStateParam_Vector3(Vector3 vec3Param)
    { _nextStateParam_Vector3 = vec3Param; }

    public string _nextStateParam_string { get; private set; }
    public void SetNextStateParam_String(string strParam)
    { _nextStateParam_string = strParam; }

    public string _nextStateParam_string_2 { get; private set; }
    public void SetNextStateParam_String_2(string strParam)
    { _nextStateParam_string_2 = strParam; }

    public bool _nextStateParam_bool { get; private set; }
    public void SetNextStateParam_Bool(bool bParam)
    { _nextStateParam_bool = bParam; }

    public bool _nextStateParam_bool_2 { get; private set; }
    public void SetNextStateParam_Bool_2(bool bParam)
    { _nextStateParam_bool_2 = bParam; }

    public void SetNextStateParam_Clear()
    {
        _nextStateParam_float       = 0f;
        _nextStateParam_float_2     = 0f;
        _nextStateParam_Vector3     = Vector3.zero;
        _nextStateParam_bool        = false;
        _nextStateParam_bool_2      = false;
    }

    public void Clear()
    {
        _currentStateID = AIStateID.eNone;
        _currentState = null;
        _nextStateID = AIStateID.eNone;
        //_nextTarget = null;

        _states.Clear();
    }
    public void AddState(FSMState<T> s)
    {
        _currentState = null;
        _states.Add(s);
    }
    public void ChangeState(AIChangeCommand in_command)
    {
        if (_currentState != null)
        {
            _currentState._leaveTime = Time.time;
            _currentState._fsmFunction.Leave();
        }

        FSMState<T> state = FindState(in_command._state);
        _prevStateID = _currentStateID;
        
        _currentStateID = in_command._state;
        _currentState = state;
        _currentState._enterTime = Time.time;
        _command = in_command;

        _currentState._fsmFunction.Enter(in_command);
    }

    public void Update()
    {
        if (_currentState != null)
            _currentState._fsmFunction.Update();
    }
    public FSMState<T> FindState(AIStateID in_state)
    {
        foreach (FSMState<T> state in _states)
        {
            if (state.ID == in_state)
                return state;
        }
        return null;
    }

}
