using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindArea : MonoBehaviour {

    public const int    _MAX_PLANE_COUNT = 2;
    public int          _nIndex = 0;
    public TileMap[]    _tileMap = new TileMap[_MAX_PLANE_COUNT];

    public void CreateTileMaps(int nIndex)
    {
        _nIndex = nIndex;
        for (int i = 0; i < _MAX_PLANE_COUNT; i++)
        {
            GameObject go = new GameObject("TileMap_" + i.ToString());
            _tileMap[i] = go.AddComponent<TileMap>();
            _tileMap[i].transform.parent = transform;
        }
    }


    public void HideTileMap()
    {
        for (int i = 0; i < _MAX_PLANE_COUNT; i++)
            _tileMap[i].gameObject.SetActive(false);	
    }

}
