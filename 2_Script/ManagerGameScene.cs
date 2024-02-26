using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSceneState
{
    NONE,
    Title,
    Intro,
    Lobby,
    InGame,
}

public class ManagerGameScene : MonoBehaviour
{

    public static ManagerGameScene _instance = null;

    [System.NonSerialized]
    public GameSceneState sceneState = GameSceneState.NONE;

    void Awake()
    {
        _instance = this;
    }

	// Use this for initialization
	void Start () {


	}
	
    public void ChangeSnene(GameSceneState in_state)
    {
        sceneState = in_state;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
