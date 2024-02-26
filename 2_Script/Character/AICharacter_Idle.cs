using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AICharacter : AI
{
    bool haveTag = false;
    float sleepTimer = 5f;

    public virtual void OnEnter_Idle(AIChangeCommand in_command)
    {
        _character._speed = 0f;
        _stateTimer = 0f;

        if (_addAnimationTag != null)
            if (_addAnimationTag.Length > 0)
                haveTag = true;

        sleepTimer = Random.Range(10f, 25f);

        if (haveTag)
            PlayAnimation(false, "idle" + _addAnimationTag, WrapMode.Loop, 0.1f);
        else
            PlayAnimation(false, "idle", WrapMode.Loop, 0.1f);

        if(_character._startMotion != null)
            if (_character._startMotion.Length > 0)
            {
                PlayAnimation(false, _character._startMotion, WrapMode.Once, 0f);
                _character._startMotion = null;
            }
    }

    public virtual void OnUpdate_Idle()
    {

        _stateTimer += Global.deltaTimeLobby;

        if (_character._type == TypeCharacterType.Boni)
        {
            bool skip = false;

            if (ManagerTutorial._instance != null)
                if (ManagerTutorial._instance._playing)
                {
                    _stateTimer = 0f;
                    skip = true;
                }

            if (LobbyBehavior._instance != null)
                if (LobbyBehavior._instance._playing)
                {
                    _stateTimer = 0f;
                    skip = true;
                }

            if (ManagerUI._instance != null)
                if (ManagerUI._instance._popupList.Count>0)
                {
                    _stateTimer = 0f;
                    skip = true;
                }


            if (ManagerLobby._instance._state != TypeLobbyState.Wait )
            {
                skip = true;
                _stateTimer = 0f;
            }

            if (_animation["idle"].enabled && !skip && _stateTimer > sleepTimer)
            {
                if(Random.value>0.5f)
                    PlayAnimation(false, "indifference_01", WrapMode.Loop, 0.1f);
                else
                    PlayAnimation(false, "sleepIn", WrapMode.Loop, 0.1f);
                //Debug.Log("아아아아");
            }
        }
        

        if (_character.GetTargetLen() > _character.nextWaypointDistance)
        {
            if(_character._path != null)
            {
                if (_character._path.vectorPath.Count > 0)
                {

                    AIChangeCommand command = new AIChangeCommand();
                    //    if (_addAnimationTag.Length > 0)
                    //        command._animationTag = _addAnimationTag;
                    command._state = AIStateID.eMove;

                    float fDot = Vector3.Dot(CameraController._instance._transform.right, _character.GetPathDir());
                    bool bDot = (fDot > 0) ? true : false;
                    if (_character._ai._bRight != bDot)
                        command._state = AIStateID.eIdleTurn;

                    ChangeState(command);    
                
                }
            }            
        }
       // Debug.Log("OnUpdate_Idle ");
    }

    public virtual void OnLeave_Idle()
    {
        if (haveTag)
            _addAnimationTag = "";
     //   Debug.Log("OnLeave_Idle ");
    }

}
