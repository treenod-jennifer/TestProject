using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNPC : Character
{
    [System.NonSerialized]
    public LobbyAnimalAI attachedLobbyAI = null;

    [System.NonSerialized]
    public bool isMakeByTrigger = false;

    private System.Action tapCallback = null;

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

    public void SetTapCallBack(System.Action _callback)
    {
        tapCallback = _callback;
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

    public override void OnTap()
    {
        ClickBlocker.Make();

        if (attachedLobbyAI != null)
        {
            attachedLobbyAI.OnTap();
        }
        else
            base.OnTap();

        tapCallback?.Invoke();
    }
}
