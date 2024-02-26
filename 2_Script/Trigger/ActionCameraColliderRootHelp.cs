using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraColliderRootHelp : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        //Debug.Log("ActionCameraColliderRootHelp " + gameObject.transform.parent.name);

        ActionCameraCollider.AddCameraCollider(this);
	}
}
