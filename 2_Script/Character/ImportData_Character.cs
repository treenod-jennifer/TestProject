﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportData_Character : MonoBehaviour {

    [System.Serializable]
    public class ImportData
    {
        public GameObject obj;
        public List<AudioClip> tapSound = new List<AudioClip>();
        public float characterHeightOffset = 0.0f;  // 보니 키를 기준으로 0.0

        public LobbyAIHints aiHint = null;
    }

    

    public ImportData data;

    


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

