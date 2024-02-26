using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIcon : MonoBehaviour
{

    [System.NonSerialized]
    public Transform _transform;

    public void Awake()
    {
        _transform = transform;
    }
    virtual public void OnTap()
    {
        Debug.Log("ObjectIcon");
    }
/*	// Use this for initialization
	void Start () {
		
	}
    public void OnTap()
    {
        Debug.Log("ObjectMissionIcon");
    }
	// Update is called once per frame
	void Update () {
		
	}*/
}
