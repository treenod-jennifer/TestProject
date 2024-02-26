using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAutoScale : MonoBehaviour {

	// Use this for initialization
	void Start () {

        transform.localPosition /= (720f / 1280f) / ((float)Screen.width / (float)Screen.height);
        transform.localScale /= (720f / 1280f) / ((float)Screen.width / (float)Screen.height);
	}

}
