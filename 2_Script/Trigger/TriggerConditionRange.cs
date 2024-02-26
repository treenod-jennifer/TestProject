using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerConditionRange : TriggerCondition
{
    public Transform _target = null;
    public TypeCharacterType _type = TypeCharacterType.Boni;
    Character character;
	// Use this for initialization
	override public void Start () {
        base.Start();
        character = ManagerLobby._instance.GetCharacter(_type);
        _ready = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (!_ready)
        {
            Vector3 len = _target.position - character._transform.position;
            len.y = 0f;
            if ((len).sqrMagnitude < 0.5f * 0.5f)
                _ready = true;
        }

        base.Update();
		
	}
}
