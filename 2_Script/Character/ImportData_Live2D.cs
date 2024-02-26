using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ImportData_Live2D: MonoBehaviour {

    [System.Serializable]
    public class ImportData
    {
        public GameObject obj;
        public float defaultScale = 20f;
        public Vector3 emoticonOffset = new Vector3(-115f, 200f, 0f);
        public List<AudioClip> chatSound;
        
        //레디 팝업창에서 사용하는 데이터
        public Vector3 readyScale = new Vector3(370f, 370f, 370f);
        public Vector3 readyPostion = new Vector3();
        public AudioLobby readySound = AudioLobby.NO_SOUND;
    }
    
    public ImportData data;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}


