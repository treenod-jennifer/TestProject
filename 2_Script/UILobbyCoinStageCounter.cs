using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyCoinStageCounter : MonoBehaviour {

    public Image _uiCounterBg;
    public Image[] _uiCounter;

    public Sprite spriteOn;
    public Sprite spriteOff;


	// Use this for initialization
	void Start () {

        //_uiCounter[0].sprite.name
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetCounter(int current)
    {

        for(int i = 0; i < 3; ++i)
        {
            _uiCounter[i].sprite = i < current ? spriteOn : spriteOff;
        }

    }
}
