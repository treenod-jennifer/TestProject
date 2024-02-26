using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarpManager : MonoBehaviour {

    public static ScarpManager instance = null;
    public ScarpFloor _scarp;                 //scarp
    public Transform _scarpFloor;       //scarp

    //public UIPanel _uip;
    public PokoTile[] _arrayTile;

    //public UIAnchor anchor;

    public int _mapWidth;// = Global.blockMaxWCount;
    public int _mapHeight;// = Global.blockMaxHCount;

    public int _mapSize;

    public int mapSize
    {
        get { return GameManager.MAX_X * GameManager.MAX_Y; }
    }

    int ground = 16;
    //int tileSizeX = 70;
    //int tileSizeY = 70;

    int tileSizeX = 78;
    int tileSizeY = 78;

    //int tileSizeX = (int)Global._blockInterval;
    //int tileSizeY = (int)Global._blockInterval;


    void Awake()
    {
        instance = this;
        _mapWidth = GameManager.MAX_X;
        _mapHeight = GameManager.MAX_Y;
        CreateMapFirst();
    }

    //맵 생성.
    public void CreateMapFirst()
    {
        _mapWidth = GameManager.MAX_X;
        _mapHeight = GameManager.MAX_Y;

        _arrayTile = new PokoTile[_mapWidth* _mapHeight];

        for (int i = 0; i < _mapWidth * _mapHeight; i++)
        {
            _arrayTile[i] = new PokoTile();
            SetGround(i);
        }
    }


    float _offsetX = 5f;
    float _offsetY = 0f;

    public void CreateFloor(float in_offsetX = 0f, float in_offsetY = 0f)
    {
        if(_scarpFloor != null)Destroy(_scarpFloor.gameObject);
        _offsetX = in_offsetX;
        _offsetY = in_offsetY;

        _scarpFloor = NGUITools.AddChild(GameUIManager.instance.groundAnchor, _scarp.gameObject).GetComponent<Transform>();

        RefleshPos();
    }

    void RefleshPos()
    {
        float offsetY = -78 * GameManager.mExtend_Y;        
        _scarpFloor.localPosition = new Vector3(ManagerBlock.instance._initGroundPos.x + (_offsetX - 78f * 5.5f) + (ManagerBlock.instance.moveWay * 1f), -624f+ offsetY + _offsetY, 0f);
    }
    int moveWay = -1;
    void Update()
    {
        if (_scarpFloor != null && moveWay != ManagerBlock.instance.moveWay)
        {
            moveWay = ManagerBlock.instance.moveWay;
            RefleshPos();
        }

        //if()
    }
    //타일의 타입을 판별하기 위한 마스크.


    //맵 생성.
    public void CreateMap()
    {
        _mapWidth = GameManager.MAX_X;
        _mapHeight = GameManager.MAX_Y;

        _arrayTile = new PokoTile[mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            _arrayTile[i] = new PokoTile();
            SetGround(i);
        }
    }

    void SetGround(int _index)
    {
        _arrayTile[_index]._sprite_name = "NonScarp";
        _arrayTile[_index]._tileType = ground;
    }

    public void InitMap()
    {
        for (int i = 0; i < mapSize; i++)
        {
            _arrayTile[i].Init();
        }

        DrawMap();
    }

    //맵 초기화.
    public void ClearMap()
    {
        if (_scarpFloor != null)
        {
            Destroy(_scarpFloor.gameObject);
            _scarpFloor = null;
        }
    }


    void SetTileType(int _index, int _type)
    {
        _arrayTile[_index]._tileType = _type;
    }


    void SetSubType(int _index, int _sub_type)
    {
        _arrayTile[_index]._tileSubType = _sub_type;
    }

    bool EqualXAxis(int _index01, int _index02) { return (_index01 / _mapWidth) == (_index02 / _mapWidth); }
    bool EqualYAxis(int _index01, int _index02) { return (_index01 % _mapWidth) == (_index02 % _mapWidth); }


    //잔디인지 검사.
    bool IsTile(int _index)
    {
        if (CheckTileSize(_index) &&
            _arrayTile[_index]._tileType != ground)
            return true;

        return false;
    }

    //타일 범위 검사.
    bool CheckTileSize(int _index)
    {
        if (_index >= mapSize ||
            _index < 0)
            return false;

        return true;
    }


    //타일의 타입을 반환.
    int GetTileType(int _index)
    {
        int result = 0;

        if (IsTile(_index - _mapWidth)
            && GetUpTileIndex(_index) != -1)
        {
            result += (int)TILE_MASK.UP;
        }

        if (IsTile(_index + _mapWidth)
            && GetDownTileIndex(_index) != -1)
        {
            result += (int)TILE_MASK.DOWN;
        }

        if (IsTile(_index - 1)
            && GetLeftTileIndex(_index) != -1)
        {
            result += (int)TILE_MASK.LEFT;
        }

        if (IsTile(_index + 1)
            && GetRightTileIndex(_index) != -1)
        {
            result += (int)TILE_MASK.RIGHT;
        }

        return result;
    }

    //왼쪽 타일의 인덱스 값을 반환.
    int GetLeftTileIndex(int _index)
    {
        if (_index / _mapWidth == (_index - 1) / _mapWidth)
            return _index - 1;

        return -1;
    }

    //오른쪽 타일의 인덱스 값을 반환.
    int GetRightTileIndex(int _index)
    {
        if (_index / _mapWidth == (_index + 1) / _mapWidth)
            return _index + 1;

        return -1;
    }


    //위쪽 타일의 인덱스 값을 반환.
    int GetUpTileIndex(int _index)
    {
        if (_index - _mapWidth >= 0)
            return _index - _mapWidth;

        return -1;
    }

    //아래쪽 타일의 인덱스 값을 반환.
    int GetDownTileIndex(int _index)
    {
        if (_index + _mapWidth < mapSize)
            return _index + _mapWidth;
        return -1;
    }


    string CreateSpriteName(int _type01, int _type02)
    {
        if (_type01 == 16)
            return "NonScarp";

        string name = "scarp_" + _type01.ToString() + "-" + _type02.ToString();
        return name;
    }

    void SetSpriteName(int _index, int _type, int _sub_type)
    {
        _arrayTile[_index]._sprite_name = CreateSpriteName(_type, _sub_type);
    }


    void ChangeSideTileType(int _index)
    {
        int index = _index;

        //위.
        if (IsTile(_index - _mapWidth))
        {
            int up_type = GetTileType(_index - _mapWidth);
            int up_sub_type = CheckSideTile(_index - _mapWidth, up_type);
            _arrayTile[index - _mapWidth]._tileType = up_type;
            _arrayTile[index - _mapWidth]._tileSubType = up_sub_type;
            SetSpriteName(index - _mapWidth, up_type, up_sub_type);
        }

        //아래.
        if (IsTile(_index + _mapWidth))
        {
            int down_type = GetTileType(_index + _mapWidth);
            int down_sub_type = CheckSideTile(_index + _mapWidth, down_type);
            _arrayTile[index + _mapWidth]._tileType = down_type;
            _arrayTile[index + _mapWidth]._tileSubType = down_type;
            SetSpriteName(index + _mapWidth, down_type, down_sub_type);
        }

        //좌.
        if (IsTile(_index - 1))
        {
            int left_type = GetTileType(_index - 1);
            int left_sub_type = CheckSideTile(_index - 1, left_type);
            _arrayTile[index - 1]._tileType = left_type;
            _arrayTile[index - 1]._tileSubType = left_sub_type;
            SetSpriteName(index - 1, left_type, left_sub_type);
        }

        //우.
        if (IsTile(_index + 1))
        {
            int right_type = GetTileType(_index + 1);
            int right_sub_type = CheckSideTile(_index + 1, right_type);
            _arrayTile[index + 1]._tileType = right_type;
            _arrayTile[index + 1]._tileSubType = right_type;
            SetSpriteName(index + 1, right_type, right_sub_type);
        }


        //오른쪽 위.
        if (IsTile(_index - _mapWidth + 1))
        {
            int ur_type = GetTileType(_index - _mapWidth + 1);
            int ur_sub_type = CheckSideTile(_index - _mapWidth + 1, ur_type);
            _arrayTile[index - _mapWidth + 1]._tileType = ur_type;
            _arrayTile[index - _mapWidth + 1]._tileSubType = ur_sub_type;
            SetSpriteName(index - _mapWidth + 1, ur_type, ur_sub_type);

        }

        //오른쪽 아래.
        if (IsTile(_index + _mapWidth + 1))
        {
            int dr_type = GetTileType(_index + _mapWidth + 1);
            int dr_sub_type = CheckSideTile(_index + _mapWidth + 1, dr_type);
            _arrayTile[index + _mapWidth + 1]._tileType = dr_type;
            _arrayTile[index + _mapWidth + 1]._tileSubType = dr_sub_type;
            SetSpriteName(index + _mapWidth + 1, dr_type, dr_sub_type);
        }

        //왼쪽 아래.
        if (IsTile(_index + _mapWidth - 1))
        {
            int dl_type = GetTileType(_index + _mapWidth - 1);
            int dl_sub_type = CheckSideTile(_index + _mapWidth - 1, dl_type);
            _arrayTile[index + _mapWidth - 1]._tileType = dl_type;
            _arrayTile[index + _mapWidth - 1]._tileSubType = dl_sub_type;
            SetSpriteName(index + _mapWidth - 1, dl_type, dl_sub_type);
        }

        //왼쪽 위.
        if (IsTile(_index - _mapWidth - 1))
        {
            int ul_type = GetTileType(_index - _mapWidth - 1);
            int ul_sub_type = CheckSideTile(_index - _mapWidth - 1, ul_type);
            _arrayTile[index - _mapWidth - 1]._tileType = ul_type;
            _arrayTile[index - _mapWidth - 1]._tileSubType = ul_sub_type;
            SetSpriteName(index - _mapWidth - 1, ul_type, ul_sub_type);
        }
    }


    int CheckSideTile(int _index, int _type)
    {
        int result = 0;
        int type = _type;

        switch (type)
        {
            case 3:
            case 6:
            case 9:
            case 12:
                result = CheckTwoWayTile(_index, type);
                break;


            case 7:
            case 11:
            case 13:
            case 14:
                result = CheckThreeWayTile(_index, type);
                break;


            case 15:
                result = CheckFourWayTile(_index, type);
                break;

            default:
                break;
        }

        return result;
    }

    int CheckTwoWayTile(int _index, int _type)
    {
        int result = 0;

        switch (_type)
        {
            case 3:
                if (CheckFirstSide(_index))
                    result = 1;
                break;

            case 6:
                if (CheckSecondSide(_index))
                    result = 2;
                break;

            case 9:
                if (CheckFourthSide(_index))
                    result = 8;
                break;

            case 12:
                if (CheckThirdSide(_index))
                    result = 4;
                break;
        }
        return result;
    }

    int CheckThreeWayTile(int _index, int _type)
    {
        int result = 0;

        switch (_type)
        {
            case 7: // ㅏ. 모양.
                if (CheckFirstSide(_index) &&
                    CheckSecondSide(_index))
                    result = 3;
                else if (!CheckFirstSide(_index) &&
                    !CheckSecondSide(_index))
                    result = 0;
                else if (CheckFirstSide(_index))
                    result = 1;
                else
                    result = 2;
                break;

            case 11:    //  ㅗ. 모양
                if (CheckFirstSide(_index) &&
                    CheckFourthSide(_index))
                    result = 9;
                else if (!CheckFirstSide(_index) &&
                    !CheckFourthSide(_index))
                    result = 0;
                else if (CheckFirstSide(_index))
                    result = 1;
                else
                    result = 8;
                break;

            case 13:        // ㅓ. 모양.
                if (CheckThirdSide(_index) &&
                    CheckFourthSide(_index))
                    result = 12;
                else if (!CheckThirdSide(_index) &&
                    !CheckFourthSide(_index))
                    result = 0;
                else if (CheckThirdSide(_index))
                    result = 4;
                else
                    result = 8;
                break;

            case 14:        // ㅜ. 모양.
                if (CheckSecondSide(_index) &&
                    CheckThirdSide(_index))
                    result = 6;
                else if (!CheckSecondSide(_index) &&
                    !CheckThirdSide(_index))
                    result = 0;
                else if (CheckSecondSide(_index))
                    result = 2;
                else
                    result = 4;
                break;
        }
        return result;
    }

    //전방향 타일 검사.

    int CheckFourWayTile(int _index, int _type)
    {
        int result = 0;

        if (IsTile(_index - _mapWidth + 1))
            result += (int)SIDE_MASK.UR;

        if (IsTile(_index + _mapWidth + 1))
            result += (int)SIDE_MASK.DR;

        if (IsTile(_index + _mapWidth - 1))
            result += (int)SIDE_MASK.DL;

        if (IsTile(_index - _mapWidth - 1))
            result += (int)SIDE_MASK.UL;

        return result;
    }

    //오른쪽 위 검사.
    bool CheckFirstSide(int _index)
    {
        if (IsTile(_index - _mapWidth + 1))
            return true;

        return false;
    }

    //오른쪽 아래 검사.
    bool CheckSecondSide(int _index)
    {
        if (IsTile(_index + _mapWidth + 1))
            return true;

        return false;
    }

    //왼쪽 아래 검사.
    bool CheckThirdSide(int _index)
    {
        if (IsTile(_index + _mapWidth - 1))
            return true;

        return false;
    }

    //왼쪽 위 검사.
    bool CheckFourthSide(int _index)
    {
        if (IsTile(_index - _mapWidth - 1))
            return true;
        return false;
    }

    public void SetTile(int indexX, int indexY, Board in_data)
    {
        int index = (indexY * _mapWidth) + indexX;

        if (in_data.IsActiveBoard)
        {
            _arrayTile[index]._sprite_name = "NonScarp";
            _arrayTile[index]._tileType = 16;
        }
        else if(in_data.IsHasScarp == true)
        {
            _arrayTile[index]._tileType = GetTileType(index);
            _arrayTile[index]._tileSubType = CheckSideTile(index, _arrayTile[index]._tileType);
            SetSpriteName(index, _arrayTile[index]._tileType, _arrayTile[index]._tileSubType);
            ChangeSideTileType(index);
        }

        DrawMap();
    }

    public int GetTileType(int indexX, int indexY)
    {
        return _arrayTile[(indexY * _mapWidth) + indexX]._tileType;
    }

    void DrawMap()
    {
        //수정해야 될 부분.
        _scarpFloor.GetComponent<ScarpFloor>().Change();
    }
}
