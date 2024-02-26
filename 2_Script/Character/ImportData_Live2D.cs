using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportData_Live2D: MonoBehaviour {

    [System.Serializable]
    public class ImportData
    {
        public GameObject obj;
        public float defaultScale = 20f;
        public Vector3 emoticonOffset = new Vector3(-115f, 200f, 0f);
        public List<AudioClip> chatSound;
    }

    public ImportData data;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}


