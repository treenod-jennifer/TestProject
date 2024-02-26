using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CloudInfo
{
    public Transform _transform;
    [System.NonSerialized]
    public Vector3 _velocity = Vector3.zero;
    [System.NonSerialized]
    public float _resetDelay = float.MaxValue;
    [System.NonSerialized]
    public bool _active = true;
}

public class ManagerEnvironment : MonoBehaviour {
    public static ManagerEnvironment _instance = null;
    public List<CloudInfo> _cloudList = new List<CloudInfo>();
    public Transform _leafRoot = null;
    float resetRangeX = 70f;
    float resetRangeY = 80f;

    void Awake()
    {
        _instance = this;
    }
	// Use this for initialization
	void Start () {

        Vector3 center = CameraController._instance.GetCenterWorldPos();
        for (int i = 0; i < _cloudList.Count; i++)
        {
            _cloudList[i]._active = true;
            _cloudList[i]._velocity = Vector3.left * Random.Range(3f, 6f);
            _cloudList[i]._transform.position = center;
            _cloudList[i]._transform.localPosition += new Vector3(Random.Range(-resetRangeX + 1f, resetRangeX - 0.5f), 0f, Random.Range(-resetRangeY, resetRangeY));
        }
        UpdateCloud(center);
       
	}
	// Update is called once per frame
	void Update () {
        Vector3 center = CameraController._instance.GetCenterWorldPos();
        UpdateCloud(center);
        UpdateLeaf(center);
	}

    void UpdateCloud(Vector3 in_center)
    {
        

        for (int i = 0; i < _cloudList.Count; i++)
        {
            if (_cloudList[i]._active)
            {
                _cloudList[i]._transform.localPosition += _cloudList[i]._velocity * Global.deltaTimeLobby;
                if (Mathf.Abs(Mathf.Abs(_cloudList[i]._transform.position.x) - Mathf.Abs(in_center.x)) > resetRangeX || Mathf.Abs(Mathf.Abs(_cloudList[i]._transform.position.z) - Mathf.Abs(in_center.z)) > resetRangeY)
                {
                    _cloudList[i]._resetDelay = Random.Range(0f, 2f);
                    _cloudList[i]._transform.gameObject.SetActive(false);
                    _cloudList[i]._active = false;
                }
            }
            else
            {
                _cloudList[i]._resetDelay -= Global.deltaTimeLobby;
                if (_cloudList[i]._resetDelay < 0f)
                {
                    _cloudList[i]._active = true;
                    _cloudList[i]._transform.gameObject.SetActive(true);
                    _cloudList[i]._velocity = Vector3.left * Random.Range(3f, 6f);
                    _cloudList[i]._transform.position = in_center;
                    _cloudList[i]._transform.localPosition += new Vector3(Random.Range(resetRangeX - 1f, resetRangeX - 0.5f), 0f, Random.Range(-resetRangeY + 0.5f, resetRangeY - 0.5f));
                }
            }
            
        }
    }
    void UpdateLeaf(Vector3 in_center)
    {
        _leafRoot.position = in_center;
    }
}
