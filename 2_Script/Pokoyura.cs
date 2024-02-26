using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pokoyura : MonoBehaviour
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

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    void Awake()
    {
        _transform = transform;
        _pokoyuraList.Add(this);
        _pokoTransform = _pokoyura.transform;
        _pokoyura.gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Pokoyura/", $"y_{_index}.png", OnLoadComplete);
        
        if (_newMakeShow)
            StartCoroutine(CoNewMake());
	}
    void OnDestroy()
    {
        _pokoyuraList.Remove(this);
    }
    public void OnLoadComplete(Texture2D r)
    {
        _pokoyura.texture = r;
        _pokoyura.gameObject.SetActive(true);
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

    public class PokoyuraDeployJsonData
    {
        public List<int> PokoyuraList = new List<int>();
    }


    static public List<int> pokoyuraDeployCustom = null;
    static public void LoadPokoyuraDeploy()
    {
        if (PlayerPrefs.HasKey("PokoyuraDeploy"))
        {
            string saved = PlayerPrefs.GetString("PokoyuraDeploy");

            try
            {
                PokoyuraDeployJsonData loaded = JsonUtility.FromJson<PokoyuraDeployJsonData>(saved);
                pokoyuraDeployCustom = loaded.PokoyuraList;
            }
            catch (System.Exception e)
            {

            }
        }
    }

    static public void SavePokoyuraDeploy()
    {
        if (pokoyuraDeployCustom == null)
        {
            PlayerPrefs.DeleteKey("PokoyuraDeploy");

            return;
        }
            

        try
        {
            PokoyuraDeployJsonData saveData = new PokoyuraDeployJsonData() { PokoyuraList = pokoyuraDeployCustom };
            string data = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("PokoyuraDeploy", data);
        }
        catch (System.Exception e)
        {

        }

    }
}
