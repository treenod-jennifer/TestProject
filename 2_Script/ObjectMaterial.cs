using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ObjectMaterial : ObjectIcon, IImageRequestable
{
    Vector3 _initScale = Vector3.one;
    public RawImage _meshRenderer;
    public Text _textCount;
    public Animation _animation;
    public GameObject _objFly;

    [System.NonSerialized]
    public MaterialSpawnUserData _data = null;
    bool _canClick = true;
    private LoadingContext _loadingContext;
    
    static public List<ObjectMaterial> _materialList = new List<ObjectMaterial>();


    void Awake()
    {
        
        base.Awake();
        _textCount.gameObject.SetActive(false);
        _meshRenderer.gameObject.SetActive(false);
        _materialList.Add(this);
    }
	// Use this for initialization

    private List<int> intList;
    private List<GameObject> objList;
    private string texUrl;

    public void SetClickable(bool b)
    {
        _canClick = b;
    }

	void Start () {
 
	    texUrl = ("mt_" + _data.materialIndex);
	    _loadingContext = UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconMaterial/", texUrl, this);
	 
        _animation["objectMaterial"].time = Random.value;
        SettingEvent();
    }

    public void OnLoadComplete(ImageRequestableResult r)
    {
        _meshRenderer.gameObject.SetActive(true);
        _meshRenderer.texture = r.texture;
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
    void OnDestroy() {
        if (!UIImageLoader.IsNull()) {
            UIImageLoader.Instance.CancelAndUnload(_loadingContext);
        }
        _materialList.Remove(this);
    }


    override public void OnTap()
    {
        if (ManagerLobby._instance != null)
            if (ManagerLobby._instance._state != TypeLobbyState.Wait)
                return;


        if (_canClick == false)
            return;

        _canClick = false;
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        if (_data != null)
        {
            List<int> harvestIndex = new List<int> { _data.index };
           // ServerAPI.MaterialHarvest(harvestIndex, recvMaterialHarvest);
        }
    }
    
    private void SettingEvent()
    {

    }


    IEnumerator DoGetMaterial()
    {
        for (int i = 0; i < _data.materialCount; i++)
        {
            if(i>0)
                yield return new WaitForSeconds(0.1f);
            ObjectMaterialFly com = (Instantiate(_objFly) as GameObject).GetComponent<ObjectMaterialFly>();
            com._transform.position = _transform.position;
            com._meshRenderer.texture = _meshRenderer.texture;
        }
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
/*
        ManagerSound.AudioPlay(AudioLobby.m_boni_tubi);
        float animationTimer = 0f;
        Vector3 startP = _transform.position;
        Material material = _meshRenderer.material;
        while (animationTimer < 1f)
        {
            _meshRenderer.color = new Color(1f, 1f, 1f, 1f - animationTimer);
            _transform.position = startP + Vector3.up * Mathf.Sin(Mathf.PI * 0.5f * animationTimer)*3f;
            animationTimer += Time.deltaTime * 2f;
            yield return null;
        } */
        GameObject.Destroy(gameObject);
       
    }
}

