using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pokoyura : MonoBehaviour, IImageRequestable
{
    [System.NonSerialized]
    public Transform _transform;
    public Image _leaves;
    public RawImage _pokoyura;

    //[System.NonSerialized]
    public int _index = 1;
    public bool _newMakeShow = false;

    static public List<Pokoyura> _pokoyuraList = new List<Pokoyura>();
    Transform _pokoTransform;

    void Awake()
    {
        _transform = transform;
        _pokoyuraList.Add(this);
        _pokoTransform = _pokoyura.transform;
        _pokoyura.gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "Pokoyura/", "y_" + _index, this);

        if (_newMakeShow)
            StartCoroutine(CoNewMake());
	}
    void OnDestroy()
    {
        _pokoyuraList.Remove(this);
    }
    public void OnLoadComplete(ImageRequestableResult r)
    {
        _pokoyura.texture = r.texture;
        _pokoyura.gameObject.SetActive(true);
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }
	// Update is called once per frame
	void Update () {
        _pokoTransform.rotation = Quaternion.Euler(50f, -45f, Mathf.Sin(Time.time * 5f) * 8f);
	}
    IEnumerator CoNewMake()
    {
        float timer = 0f;
        while (timer <= Mathf.PI)
        {
            timer += Global.deltaTime * 18f;
            _pokoTransform.localPosition = Vector3.up * Mathf.Sin(timer) * -0.7f;
            yield return null;
        }
    }
}
