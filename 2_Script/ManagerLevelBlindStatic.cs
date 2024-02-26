using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum eLevelBlindType
{
    High,
    Medium,
    None,
}

public class MeshDataInfo
{
    public int      _nPosIndex_X       = 0;
    public int      _nPosIndex_Z       = 0;
    public float    _fPosX             = 0f;
    public float    _fPosZ             = 0f;
    public int      _nRows             = 50;
    public int      _nColumns          = 50;
    public float    _fTileWidthSize    = 1f;
    public float    _fTileHeightSize   = 1f;
}

public class ManagerLevelBlindStatic : MonoBehaviour
{
    public static ManagerLevelBlindStatic instance;
//    private const int       _MAX_PLANE_COUNT = 2;
//    public TileMap[]        _tileMap = new TileMap[_MAX_PLANE_COUNT];
    public BlindAreaGroup   _BlindAreaGroup;
    public Material         _materialBlind = null;
    public BlockRoot _BlockRoot = null;
    private int _nXSize = 0;
    private int _nYSize = 0;
    private Byte[][] _data;
    private int _nCurrentPlaneIndex = 0;
    private int _nBlendingPlaneIndex = 1;

    private string _strPrevFileName = "";
    private TextAsset _prevAsset = null;

    public bool _bEditMode = false;

    private Transform _transform = null;
    // wakeup 과정에서 불리워질때 이외에 각 씬의 상태들의 젤 최신 파일로 한번만 생성하기 위해..
    Dictionary<int, TextAsset> _areaBank = new Dictionary<int, TextAsset>();

//    Dictionary<int, BlindArea> _dicBlindArea = new Dictionary<int, BlindArea>();
    private int _nCurrentBlindArea = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _transform = transform;
        }
    }

    public TileMap GetTileMap(int nAreaIndex, int nTileMapIndex)
    {
        return _BlindAreaGroup.GetTileMap(nAreaIndex, nTileMapIndex);
    }

    public void Init(MeshDataInfo meshDataInfo, int nAreaIndex, int nTextureID)
    {
        AddBlindArea(nAreaIndex);
        TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
        if (tileMap != null)
            tileMap.InitData(meshDataInfo, nTextureID, _materialBlind);
    }

    public void DeleteColliderMesh(int nAreaIndex)
    {
        for (int i = 0; i < BlindArea._MAX_PLANE_COUNT; i++)
        {
            TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
            if (tileMap == null)
                continue;
            tileMap.DeleteColliderMesh();    
        }
    }

    public void MakeColliderMesh(int nAreaIndex)
    {
        for (int i = 0; i < BlindArea._MAX_PLANE_COUNT; i++)
        {
            TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
            if (tileMap == null)
                continue;
            tileMap.MakeColliderMesh(true);
        }
    }

    public void ChangeTextureID_All(int nTextureID)
    {
        foreach (BlindArea blindArea in _BlindAreaGroup._dicBlindAreas.Values)
        {
            blindArea._tileMap[0].ChangeTextureID(nTextureID);
        }
    }

    public void ChangeTextureID(int nAreaIndex, int nTextureID)
    {
        TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
        if (tileMap)
            tileMap.ChangeTextureID(nTextureID);
    }

    public bool SaveTileChunkDataInfo(string strFileName, MeshDataInfo meshDataInfo, int nAreaIndex)
    {
        string strFullPath = strFileName;
        TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
        if (tileMap == null)
            return false;
        return tileMap.SaveTileChunkDataInfoBinary(strFullPath, meshDataInfo);
    }
    // 젤 최근 파일 기반으로 영역별 블라인드 파일 기록
    public void SetBank(int in_area, TextAsset in_asset)
    {
        if (_areaBank.ContainsKey(in_area))
            _areaBank[in_area] = in_asset;
        else
            _areaBank.Add(in_area, in_asset);
    }
    // 로비 진입시 한번 로딩,
    public void LoadTileChunkDataInfoByBank()
    {
        foreach (KeyValuePair<int, TextAsset> item in _areaBank)
            LoadTileChunkDataInfo(item.Value, item.Key);
        
    }
    public bool LoadTileChunkDataInfo(TextAsset asset, int nAreaIndex = 0, float fDurationTime = 0.0f, bool bFade = false)
    {
        //Debug.Log("LoadTileChunkDataInfo " + asset.name);

        // blind Areas
        BlindArea blindArea = AddBlindArea(nAreaIndex);
        
        bool bRet = blindArea._tileMap[0].LoadTileChunkDataInfoTextAsset(asset, _materialBlind);
        if (bFade == true)
        {
            if (_prevAsset == null)
                blindArea._tileMap[0]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 1f));
            else
            {
                blindArea._tileMap[1].LoadTileChunkDataInfoTextAsset(_prevAsset, _materialBlind);
                StartCoroutine(CoFadeInOut(blindArea, fDurationTime, () =>
                {
                    blindArea._tileMap[0]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 1f));
                    blindArea._tileMap[1]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 0f));
                    blindArea._tileMap[1].gameObject.SetActive(false);
                }));
            }
        }
        _prevAsset = asset;
        return bRet;
    }

    public void AllClearArea()
    {
        _BlindAreaGroup._dicBlindAreas.Clear();
        _BlindAreaGroup.transform.DestroyChildren();
    }

    public BlindArea AddBlindArea(int nAreaIndex)
    {
        BlindArea retBlindArea = null;
        if (_BlindAreaGroup._dicBlindAreas.ContainsKey(nAreaIndex))
        {
            retBlindArea = _BlindAreaGroup._dicBlindAreas[nAreaIndex];
            if (retBlindArea == null || retBlindArea.gameObject == null)
            {
                GameObject go = new GameObject("BlindArea_" + nAreaIndex.ToString("00"));
                
                retBlindArea = go.AddComponent<BlindArea>();
                retBlindArea.transform.parent = transform;
                retBlindArea.CreateTileMaps(nAreaIndex);
                go.transform.localScale = Vector3.one;

            }
        }
        else
        {
            GameObject go = new GameObject("BlindArea_" + nAreaIndex.ToString("00"));
            retBlindArea = go.AddComponent<BlindArea>();
            _BlindAreaGroup.AddBlindArea(nAreaIndex, retBlindArea);
        }
        return retBlindArea;
    }

    public bool LoadTileChunkDataInfo(string strFileName, int nAreaIndex = 0, float fDurationTime = 0.0f, bool bFade = false)
    {
        string strFullPath = strFileName;
        // blind Areas
        BlindArea blindArea = AddBlindArea(nAreaIndex);
        if (blindArea == null)
            return false;

        bool bRet = blindArea._tileMap[0].LoadTileChunkDataInfoBinary(strFullPath, _materialBlind);
        if (bFade == true)
        {
            if (_strPrevFileName == "")
                blindArea._tileMap[1].LoadTileChunkDataInfoBinary(strFullPath, _materialBlind);
            else
            {
                blindArea._tileMap[1].LoadTileChunkDataInfoBinary(_strPrevFileName, _materialBlind);
                StartCoroutine(CoFadeInOut(blindArea, fDurationTime, () =>
                {
                    blindArea._tileMap[0]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 1f));
                    blindArea._tileMap[1]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 0f));
                    blindArea._tileMap[1].gameObject.SetActive(false);
                }));
            }
        }
        _strPrevFileName = strFullPath;
        return bRet;
    }

    private IEnumerator CoFadeInOut(BlindArea blindArea, float _fDelayTime, UnityAction _action = null)
    {
        float _fCurTime = _fDelayTime;
        blindArea._tileMap[0].gameObject.SetActive(true);
        if (blindArea._tileMap[0]._material)
            blindArea._tileMap[0]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 0f));
        blindArea._tileMap[1].gameObject.SetActive(true);
        if (blindArea._tileMap[1]._material)
            blindArea._tileMap[1]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 1f));
        float fValue = 0f;
        while (_fCurTime > 0)
        {
            fValue = (_fCurTime / _fDelayTime);
            if (blindArea._tileMap[0]._material)
                blindArea._tileMap[0]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, 1f - fValue));
            if (blindArea._tileMap[1]._material)
                blindArea._tileMap[1]._material.SetColor("_MainColor", new Color(1f, 1f, 1f, fValue));
            _fCurTime -= Time.deltaTime;
            yield return null;
        }
        _action();
    }
    /*
    public bool CheckMoveEnableByData(float fX, float fY, eLevelBlindType eType = eLevelBlindType.High)
    {
        int nCompareValue = (int) eType;
        bool bRet = true;
        int nX = (int) fX - _rect.left;
        int nY = (int) fY - _rect.top;
        if (nX >= _nXSize || nY >= _nYSize)
            return bRet;
        if (_data[nX][nY] <= nCompareValue && _data[nX + 1][nY] <= nCompareValue
            && _data[nX][nY + 1] <= nCompareValue && _data[nX + 1][nY + 1] <= nCompareValue)
            bRet = false;
//        Debug.Log("CheckMoveEnableByData : " + fX.ToString("0.0") + " " + fY.ToString("0.0") + " ---- " + bRet.ToString());
        return bRet;
    }
    */
    public int GetVerticesCount(int nAreaIndex)
    {
        int nRet = 0;
        TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
        if (tileMap != null)
            nRet = tileMap.GetVerticesCount();
        return nRet;
    }

    public int GetTrianglesCount(int nAreaIndex)
    {
        int nRet = 0;
        TileMap tileMap = _BlindAreaGroup.GetTileMap(nAreaIndex, 0);
        if (tileMap != null)
            nRet = tileMap.GetTrianglesCount();
        return nRet;
    }

}
