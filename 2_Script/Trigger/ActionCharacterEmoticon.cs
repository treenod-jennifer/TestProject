using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterEmoticon : ActionBase
{
    public TypeCharacterType _type = TypeCharacterType.Boni;


    Character character;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterEmoticon _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_type);

    }

}
