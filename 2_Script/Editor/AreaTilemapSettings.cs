using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class AreaTilemapSettings : ScriptableObject
{
    [SerializeField] private ManagerTilemap manager;

    [SerializeField] private Tilemap defaultBlindTile;
    [SerializeField] private Tilemap defaultCloudTile;
    
    public ManagerTilemap Manager { get { return manager; } }

    public Tilemap BlindTile { get { return defaultBlindTile; } }

    public Tilemap CloudTile { get { return defaultCloudTile; } }
}
