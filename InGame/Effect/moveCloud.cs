using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCloud : MonoBehaviour {

    Transform _trasform;
	// Use this for initialization
	void Awake () {
        _trasform = transform;

    }
	
	// Update is called once per frame
	void Update ()
    {
        _trasform.localPosition = new Vector3(_trasform.localPosition.x - Global.deltaTimePuzzle* 6f, _trasform.localPosition.y, _trasform.localPosition.z);

        if (_trasform.localPosition.x < -900)
        {
            _trasform.localPosition = new Vector3( 900f, _trasform.localPosition.y, _trasform.localPosition.z);
        }
    }
}
