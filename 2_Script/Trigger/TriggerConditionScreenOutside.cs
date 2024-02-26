using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerConditionScreenOutside : TriggerCondition
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    Character character;
    Rect pachScreenRect = new Rect(-50f, -50f, Screen.width + 100f, Screen.height + 100f);

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
            Vector3 screenPos = CameraController._instance.WorldToScreen(character._transform.position);
            if (!pachScreenRect.Contains(screenPos))
                _ready = true;
        }

        base.Update();
		
	}
}
