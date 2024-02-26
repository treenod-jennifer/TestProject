using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterRide : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public ObjectEvent _targetObject = null;
    public Vector3 _startOffset = Vector3.one;
    public Vector3 _endOffset = Vector3.zero;
    public TypeCharacterDir _direction = TypeCharacterDir.None;
    public float _exitTime = 0f;
    public bool _screenInStart = false;
    bool aborted = false;
    Character character;
    public void Awake()
    {
        //bWaitActionFinishOnOff = true;
    }
    public override void DoAction()
    {
        //base.DoAction();
        _startOffset.y = 0f;

        character = ManagerLobby._instance.GetCharacter(_type);
        character.StartPath(_targetObject.transform.position + _startOffset, OnPathComplete, _screenInStart);

        character._ai._rideStartPosition = _targetObject.transform.position + _startOffset;
        character._ai._rideEndPosition = _targetObject.transform.position + _endOffset;
    }

    public override void AbortAction()
    {
        aborted = true;
        
    }
    IEnumerator CoDoAction()
    {
        aborted = false;
        while (true)
        {
            if( aborted )
            {
                yield break;
            }

            if (character._ai.GetStateID() == AIStateID.eIdle)
            {
                bActionFinish = true;
                break;
            }
            yield return null;
        }
        if (_direction == TypeCharacterDir.Left)
            character._ai.BodyTurn(TypeCharacterDir.Left);
        else if (_direction == TypeCharacterDir.Right)
            character._ai.BodyTurn(TypeCharacterDir.Right);


        AIChangeCommand command = new AIChangeCommand();
        command._state = AIStateID.eRide;
        command._moveSpeed = _exitTime;
        character._ai.ChangeState(command);
    }
    public virtual void OnPathComplete(Path _p)
    {
        AIChangeCommand command = new AIChangeCommand();
        command._state = AIStateID.eMove;
        command._moveSpeed = 0f;
        command._animationSpeed = 0f;
        character._ai.ChangeState(command);


        StartCoroutine(CoDoAction());
    }
    void OnDrawGizmosSelected()
    {
        if (_targetObject != null)
        {
            _startOffset.y = 0f;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_targetObject.transform.position + _startOffset, new Vector3(0.4f, 0.6f, 0.4f));
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_targetObject.transform.position + _endOffset, new Vector3(0.4f, 0.6f, 0.4f));
        }
    }
}
