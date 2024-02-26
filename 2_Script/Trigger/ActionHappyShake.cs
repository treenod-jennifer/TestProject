using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionHappyShake : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public ObjectBase _target = null;
    public float _scale = 1f;
    public float _scaleSpeed = 1f;

    public override void DoAction()
    {
        StartCoroutine(CoDoAction());
    }

    IEnumerator CoDoAction()
    {
        Character character = ManagerLobby._instance.GetCharacter(_type);
        Vector3 scale = _target._transformModel.localScale;

        while (true)
        {
            float ratio = Mathf.Sin(Time.time * 20f * _scaleSpeed) * 0.06f * _scale;

            _target._transformModel.localScale = new Vector3(1f - ratio, 1f + ratio,0f);

            if (character._ai.GetStateID() != AIStateID.eIdle)
            {
              //  bActionFinish = true;
                break;
            }
            yield return null;
        }
    }
}
