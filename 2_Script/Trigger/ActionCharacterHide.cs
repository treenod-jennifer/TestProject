using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterHide : ActionBase
{
   /* public TypeCharacterType _type = TypeCharacterType.Boni;
    public float _duration = 0f;
    public bool _hide = true;

    Character character;
    public override void DoAction()
    {
        character = ManagerLobby._instance.GetCharacter(_type);
        StartCoroutine(CoDoAction());
    }
    IEnumerator CoDoAction()
    {
        


        if (_hide)
            character.SetAlpha(1f);
        else
            character.SetAlpha(0f);

        float timer = _duration;
        while (timer>0f)
        {
            

            if (_hide)
                character.SetAlpha(timer / _duration);
            else
                character.SetAlpha(1f - (timer / _duration));
            timer -= Global.deltaTimeLobby;

            yield return null;
        }

        if (_hide)
            character.SetAlpha(0f);
        else
            character.SetAlpha(1f);
    }*/
}
