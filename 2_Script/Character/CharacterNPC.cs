using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNPC : Character
{
    void Awake()
    {
        base.Awake();
        _ai.AIStart(this, _animation);
    }
	// Use this for initialization
	void Start () {

        if (_runDust != null)
            _runDust.Stop();
        ManualyStart();
	}
    public override void ManualyStart()
    {
        
        ChangeState(AIStateID.eIdle);
    }
	// Update is called once per frame
	void Update () {
        _ai.AIUpdate();
        MoveUpdate();
	}
}
