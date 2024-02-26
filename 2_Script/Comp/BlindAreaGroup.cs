using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindAreaGroup : MonoBehaviour
{
    private Transform _transform;
    public Dictionary<int, BlindArea> _dicBlindAreas = new Dictionary<int, BlindArea>();

    private void Awake()
    {
        _transform = transform;
    }

    // Use this for initialization
	void Start ()
	{
        if (_transform == null)
            _transform = transform;
        _transform.DestroyChildren();
	}
	
    public void AddBlindArea(int nIndex, BlindArea blindArea)
    {
        if (_dicBlindAreas.ContainsKey(nIndex) != false)
            _dicBlindAreas.Add(nIndex, blindArea);
        else
            _dicBlindAreas[nIndex] = blindArea;
        if (_transform == null)
            _transform = transform;
        blindArea.transform.parent = _transform;
        blindArea.CreateTileMaps(nIndex);
    }

    public void RemoveBlindArea(int nIndex)
    {

    }

    public void HideTileMap()
    {
        foreach (var blindArea in _dicBlindAreas)
        {
            
        }
    }

    public TileMap GetTileMap(int nAreaIndex, int nTileMapIndex)
    {
        TileMap tileMap = null;
        if (_dicBlindAreas.ContainsKey(nAreaIndex))
            tileMap = _dicBlindAreas[nAreaIndex]._tileMap[nTileMapIndex];
        return tileMap;
    }

}
