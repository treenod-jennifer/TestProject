using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterExit : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public string _animationTag = "";
    public float _animationSpeed = 0f;
    public float _moveSpeed = 0f;
    public Transform _position;
    public Vector2 _offset;
    public bool _screenInStart = false;

    Character character;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterExit _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);

        if (_position == null)
        {
            ManagerLobby._instance.RemoveCharacter(_type);
            bActionFinish = true;
        }else
            character.StartPath(_position.position + new Vector3(_offset.x, 0f, _offset.y), OnPathComplete, _screenInStart);

   /*     if (_position != null)
            character._transform.position = _position.position;

        if (_direction != TypeCharacterDir.None)
            character._ai.BodyTurn(_direction);

        {
            AIChangeCommand command = new AIChangeCommand();
            command._state = AIStateID.eEvent;
            command._moveSpeed = 0f;
            command._animationSpeed = 0f;
            command._animationName = _animationName;

            if (_animationSpeed > 0f)
                command._animationSpeed = _animationSpeed;

            character._ai.ChangeState(command);
        }*/


    }
    public virtual void OnPathComplete(Path _p)
    {
        AIChangeCommand command = new AIChangeCommand();
        command._state = AIStateID.eMove;
        command._moveSpeed = 0f;
        command._animationSpeed = 0f;
        command._animationName = _animationTag;

        if (_moveSpeed > 0f)
            command._moveSpeed = _moveSpeed;
        if (_animationSpeed > 0f)
            command._animationSpeed = _animationSpeed;


        character._ai.ChangeState(command);


         
        StartCoroutine(CoDoAction());
    }
    IEnumerator CoDoAction()
    {
        while (true)
        {
            if (character._ai.GetStateID() == AIStateID.eIdle)
            {
                ManagerLobby._instance.RemoveCharacter(_type);
                bActionFinish = true;
                break;
            }
            yield return null;
        }
    }
    void OnDrawGizmosSelected()
    {
        if (_position != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_position.position + new Vector3(_offset.x, 0f, _offset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }
    }
}
