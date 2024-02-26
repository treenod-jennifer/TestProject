using System.Collections;
using UnityEngine;

public class Tile
 {
    public int _tileType;
    public int _tileSubType;
    public string _sprite_name;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        _tileType = 0;
        _tileSubType = 0;
        _sprite_name = "land01";
    }
}
