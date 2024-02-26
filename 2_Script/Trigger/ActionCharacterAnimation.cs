using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;



[System.Serializable]
public class AnimationList
{
    public string _animation = "";
    public float _speed = 1f;
    public bool _Loop = true;
    public float _dulation = 0f;
} 

public class ActionCharacterAnimation : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public TypeCharacterDir _direction = TypeCharacterDir.None;
    public List<AnimationList> _animation = new List<AnimationList>();
    public int[] _enableMission = null;
    Character character;
    bool haveLock = false;

    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }

    public override void DoAction()
    {
        if (_enableMission != null)
        {
            if (_enableMission.Length > 0)
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
                {
                    bActionFinish = true;
                    ActionBase.TryUnlockCharacter(this._type);
                    return;
                }
            }
        }


        //base.DoAction();

        //Debug.Log("ActionCharacterAnimation _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);
        if (_direction != TypeCharacterDir.None)
            character._ai.BodyTurn(_direction);

        StartCoroutine(CoDoAction());
    }
    IEnumerator CoDoAction()
    {

        for (int i = 0; i < _animation.Count; i++)
        {
            float tiemr = 0f;
            if (_animation[i]._Loop)
            {
                float speed = 1f;
                if (_animation[i]._speed > 0f)
                    speed = _animation[i]._speed;

                character.ChangeState(AIStateID.eIdle);

                character._ai.PlayAnimation(false, _animation[i]._animation, WrapMode.Loop, 0f, speed);


                //Debug.Log("ActionChAni.CoDoAction_Loop_Start " + _type.ToString() + " " + _animation[i]._animation + " Time: " + _animation[i]._dulation.ToString());
                while (tiemr < _animation[i]._dulation)
                {
                    tiemr += Global.deltaTimeLobby;
                    yield return null;
                }
            }
            else
            {
                float speed = 1f;
                if (_animation[i]._speed > 0f)
                    speed = _animation[i]._speed;

                character._ai.PlayAnimation(false, _animation[i]._animation, WrapMode.Once, 0f, speed);
                //Debug.Log("ActionChAni.CoDoAction_NonLoop_Start " + _type.ToString() + " " + _animation[i]._animation);
                while (true)
                {
                    // 중간에 껴든 다른 애니메이션 때문에 무한루프에 빠질 수 있으니 혹시 여기서 루프상태가 되면 탈출시켜준다
                    if (!character._animation.isPlaying || character._animation.wrapMode == WrapMode.Loop)
                    {
                        //Debug.Log("ActionChAni.CoDoAction_NonLoop_Break " + _type.ToString() + " " + _animation[i]._animation);

                        break;
                    }
                        
                    yield return null;
                }
            }
        }
        bActionFinish = true;
        ActionBase.TryUnlockCharacter(this._type);
        yield return null;
    }

    public void OnDisable()
    {
        if( haveLock )
            ActionBase.TryUnlockCharacter(this._type);
    }

    public override bool ActionStartPrecheck()
    {
        haveLock = ActionBase.TryLockCharacter(this._type);
        return haveLock;
    }

}
