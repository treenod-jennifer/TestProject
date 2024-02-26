using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protocol;
using Newtonsoft.Json;

public class ObjectMaterial : ObjectIcon
{
    Vector3 _initScale = Vector3.one;
    public RawImage _meshRenderer;
    public Text _textCount;
    public Animation _animation;
    public GameObject _objFly;

    [System.NonSerialized]
    public MaterialSpawnUserData _data = null;
    bool _canClick = true;
    
    static public List<ObjectMaterial> _materialList = new List<ObjectMaterial>();
    
    private const int texW = 128;
    private const int texH = 128;
    private const int atW = 512;
    private const int atH = 512;

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

    void Start()
    {
        string fileName = "mt_" + _data.materialIndex;

        long expireTs = 0;
        if (ManagerData._instance._materialSpawnProgress.ContainsKey(_data.materialIndex))
            expireTs = ManagerData._instance._materialSpawnProgress[_data.materialIndex];

        //현재 검사하는 재료 텍스쳐 정보가 있는지 검사.
        if (ManagerCdnTextureAtlas._instance != null && expireTs == 0)
        {
            bool bLocalLoadTexture = false;
            bLocalLoadTexture = ManagerCdnTextureAtlas._instance.CheckTextureData((int)TextureCategory_T.Material, _data.materialIndex);

            if (bLocalLoadTexture == true)
            {
                string hashFileName = Global.GetHashfromText("IconMaterial/" + fileName) + ".png";
                string filePath = Global.gameImageDirectory + hashFileName;

                //로컬에 해당 재료 이미지 저장되어있는지 검사.
                if (System.IO.File.Exists(filePath))
                {
                    //해시값이 다른지 검사.
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    if ((HashChecker.IsHashChanged("IconMaterial", fileName + ".png", fileInfo)))
                    {
                        //Debug.Log("1. ChangeHash : " + fileName);
                        bLocalLoadTexture = false;
                    }
                    else
                        bLocalLoadTexture = true;
                }
                else
                {
                    //Debug.Log("2. File Not Exists : " + fileName);
                    bLocalLoadTexture = false;
                }
            }
            //else
            //    Debug.Log("3. Matarial Data Exists : " + fileName);

            if (bLocalLoadTexture == false)
            {
                Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconMaterial", fileName, OnLoadComplete);
                ManagerCdnTextureAtlas._instance.InitTextureDataLoadState((int)TextureCategory_T.Material, _data.materialIndex);
            }
            else
            {
                TextureKeyAndData textureData = new TextureKeyAndData();
                textureData = ManagerCdnTextureAtlas._instance.GetTextureKeyAndData((int)TextureCategory_T.Material, _data.materialIndex);
                _meshRenderer.texture
                    = ManagerCdnTextureAtlas._instance.GetTexture((int)TextureCategory_T.Material, textureData, texW, texH, atW, atH);
                _meshRenderer.uvRect = ManagerCdnTextureAtlas._instance.GetUVRect(textureData, texW, texH, atW, atH);
                _meshRenderer.gameObject.SetActive(true);
            }
        }
        else
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconMaterial", fileName, OnLoadComplete);
        }
        _animation["objectMaterial"].time = Random.value;
        SettingEvent();
    }

    public void OnLoadComplete(Texture2D r)
    {
        _meshRenderer.gameObject.SetActive(true);
        _meshRenderer.texture = r;

        long expireTs = 0;
        if (ManagerData._instance._materialSpawnProgress.ContainsKey(_data.materialIndex))
            expireTs = ManagerData._instance._materialSpawnProgress[_data.materialIndex];
        if (ManagerCdnTextureAtlas._instance != null && expireTs == 0)
        {
            if (ManagerCdnTextureAtlas._instance.CheckTextureData((int)TextureCategory_T.Material, _data.materialIndex) == false)
            { 
                ManagerCdnTextureAtlas._instance.MakeAtlasAndTextureData(TextureCategory_T.Material, _data.materialIndex, (Texture2D)_meshRenderer.texture, texW, texH, atW, atH);
            }
        }
    }

    void OnDestroy() 
    {
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
            ServerAPI.MaterialHarvest(harvestIndex, recvMaterialHarvest);
        }
    }
    
    private void SettingEvent()
    {
        if (ServerContents.MaterialMeta.TryGetValue(_data.index, out CdnMaterialMeta materialMeta))
        {
            if(materialMeta.expireTs != 0)
            {
                _animation["objectMaterial"].speed = 3f;
            }
        }
    }

    void recvMaterialHarvest(MaterialHarvestResp resp)
    {
        if (resp.IsSuccess)
        {
            StartCoroutine(DoGetMaterial());
            ManagerUI._instance.MakeMaterialIcon(_data.materialCount);
            ManagerData._instance._materialSpawnData.Remove(_data);
            MaterialData.SetUserData();
            QuestGameData.RefleshData();

            if (ServerContents.MaterialSpawnProgresses.ContainsKey(_data.index))
            {
                long regenTs = Global.GetTime() + ServerContents.MaterialSpawnProgresses[_data.index].duration;

                bool timeOver = false;

                if(ServerContents.MaterialMeta.TryGetValue(_data.index, out CdnMaterialMeta materialMeta))
                {
                    timeOver = (materialMeta.expireTs != 0 && materialMeta.expireTs < regenTs);
                }

                if( timeOver == false)
                {
                    PlayerPrefs.SetString("MaterialRegenAt_" + _data.materialIndex, regenTs.ToString());

                    bool isNormalMaterial = ServerContents.MaterialMeta[_data.materialIndex].expireTs == 0;
                    ManagerUI._instance.RefreshLimitedMaterialRegenForecast(isNormalMaterial);
                }
            }

            //다이어리 쪽 퀘스트 데이터, 하우징 데이터 갱신.
            UIDiaryController._instance.UpdateProgressHousingData();
            UIDiaryController._instance.UpdateQuestData(true);

            if (PlayerPrefs.HasKey("Material" + _data.index))
            {
                int index = PlayerPrefs.GetInt("Material" + _data.index);
                if (index < ManagerLobby._instance._spawnMaterialPosition.Length)
                    ManagerLobby._instance._spawnMaterialPosition[index].used = false;
                PlayerPrefs.DeleteKey("Material" + _data.index);
            }

            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
              (
                 ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                  "MATERIAL_" + _data.materialIndex.ToString(),
                  "material",                  
                  _data.materialCount,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TOWN_GIFT
              );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
        }
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
            com._meshRenderer.uvRect = _meshRenderer.uvRect;
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

