using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterMove : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public string _animationTag = "";
    public float _moveSpeed = 0f;
    public float _animationSpeed = 0f;

    public Transform _startPosition;
    public Transform _endPosition;
    public Vector2 _offset;
    public bool _screenInStart = false;
    Character character;

    public float followCameraSmoothTime = 0f;

    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterMove _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);


        if (_startPosition != null)
            character._transform.position = _startPosition.position;


        character.StartPath(_endPosition.position + new Vector3(_offset.x, 0f, _offset.y), OnPathComplete, _screenInStart);

        
    }
    public virtual void OnPathComplete(Path _p)
    {
        if (_p.vectorPath.Count == 0)
        {
            character.StopPath();
        }
        else
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
        }

        


        if (_conditionWaitOn)
            StartCoroutine(CoDoAction());
    }
    IEnumerator CoDoAction()
    {

        
        Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = CameraController._instance._transform.position - startPos;
        Vector3 currentVelocity = Vector3.zero;
        float zcurrentVelocity = 0f;

        while (true)
        {
            if (followCameraSmoothTime > 0f)
            {
                Vector3 targetPos = character._transform.position + offset;
                targetPos = Vector3.SmoothDamp(CameraController._instance._transform.position, targetPos, ref currentVelocity, followCameraSmoothTime, 10000f, Global.deltaTimeLobby);
                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(targetPos.x, targetPos.z));
            }
            

            if (character._ai.GetStateID() == AIStateID.eIdle)
            {
                bActionFinish = true;
                break;
            }
            yield return null;
        }
    }
    void OnDrawGizmosSelected()
    {
        if (_startPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_startPosition.position, new Vector3(0.8f, 1.4f, 0.8f));
        }
        if (_endPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(_endPosition.position + new Vector3(_offset.x, 0f, _offset.y), new Vector3(0.8f, 1.4f, 0.8f));

            if (_startPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_endPosition.position + new Vector3(_offset.x, 0f, _offset.y), _startPosition.position);
            }
        }
    }
}
