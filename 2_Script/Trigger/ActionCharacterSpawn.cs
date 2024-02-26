using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterSpawn : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public int costumeIdx = -1;
    public TypeCharacterDir _direction = TypeCharacterDir.None;
    public string _animationName = "";
    public float _animationSpeed = 1f;
    public Transform _position;


    public int[] _enableMission = null;

    Character character;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterSpawn _ DoAction");
        if (_enableMission != null)
        {
            if(_enableMission.Length>0)
            {
                bool playAction = false;
                for (int i = 0; i < _enableMission.Length; i++)
                {
                    TypeMissionState state = ManagerData._instance._missionData[_enableMission[i]].state;

                    if (ManagerData._instance._missionData[_enableMission[i]].state == TypeMissionState.Active)
                    {
                        playAction = true;
                        break;
                    }
                }

                if (!playAction)
                    return;
            }
        }

        character = ManagerLobby._instance.MakeOrGetCharacter(_type, costumeIdx, Vector3.zero, true);

        character.StopPath();


        if (_position != null)
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
        }
    }
    void OnDrawGizmosSelected()
    {
        if (_position != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_position.position, new Vector3(0.8f, 1.4f, 0.8f));
        }
    }
}
