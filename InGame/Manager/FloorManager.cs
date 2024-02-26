using System.Collections;
using UnityEngine;

public enum TILE_MASK
{
    UP = 1,      //위.
    RIGHT = 2,      //오른쪽.
    DOWN = 4,      //아래.
    LEFT = 8,       //왼쪽.
}


//타일의 서브 타입을 판별하기 위한 마스크.
public enum SIDE_MASK
{
    UR = 1,         //오른쪽 위.
    DR = 2,         //오른쪽 아래.
    DL = 4,         //왼쪽 아래.
    UL = 8,         //왼쪽 위.
}

public class FloorManager : MonoBehaviour {

    public static FloorManager instance = null;

    public Floor floor;

    Transform _floor = null; //floor
    Floor _floorClass = null;

    public PokoTile[] array_tile;

    public int _mapWidth;// = Global.blockMaxWCount;
    public int _mapHeight;// = Global.blockMaxHCount;

    public int _mapSize;

    public int mapSize
    {
        get { return (GameManager.MAX_X ) * GameManager.MAX_Y; }
    }

    int ground = 16;
    int tileSizeX = 78;
    int tileSizeY = 78;



    void Awake()
    {
        instance = this;
        _mapWidth = GameManager.MAX_X ;
        _mapHeight = GameManager.MAX_Y;

        CreateMapFirst();
    }

    public void CreateMapFirst()
    {
        _mapWidth = GameManager.MAX_X;
        _mapHeight = GameManager.MAX_Y;

        array_tile = new PokoTile[_mapWidth* _mapHeight];

        for (int i = 0; i < _mapWidth * _mapHeight; i++)
        {
            array_tile[i] = new PokoTile();
            SetGround(i);
        }
    }


    float _offsetX = 5f;
    float _offsetY = 0f;

    public void CreateFloor(float in_offsetX = 0f, float in_offsetY = 0f)
    {
        if (_floor != null)Destroy(_floor.gameObject);

        _offsetX = in_offsetX;
        _offsetY = in_offsetY;

        _floor = NGUITools.AddChild(GameUIManager.instance.groundAnchor, floor.gameObject).transform;
        _floorClass = _floor.gameObject.GetComponent<Floor>();
        _floorClass.InitFloorSprite();
        RefleshPos();
    }

    void RefleshPos()
    {
            float offsetY = -78 * GameManager.mExtend_Y;
            _floor.localPosition = new Vector3(ManagerBlock.instance._initGroundPos.x + (_offsetX - 78f * 5.5f) + (ManagerBlock.instance.moveWay * 1f), -624f + offsetY+ _offsetY, 0f);       
    }

    int moveWay = -1;
    void Update()
    {
        if (_floor != null && moveWay != ManagerBlock.instance.moveWay)
        {
            moveWay = ManagerBlock.instance.moveWay;
            RefleshPos();
        }
    }

    //맵 생성.
    public void CreateMap()
    {
        _mapWidth = GameManager.MAX_X;
        _mapHeight = GameManager.MAX_Y;

        array_tile = new PokoTile[mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            array_tile[i] = new PokoTile();
            SetGround(i);
        }
    }

    void SetGround(int _index)
    {
        int h = _index / _mapWidth;
        int w = _index % _mapWidth;

        if (h % 2 == 0 && w % 2 == 0)
            array_tile[_index]._sprite_name = "land02";
        else if (h % 2 == 1 && w % 2 == 1)
            array_tile[_index]._sprite_name = "land02";
        else
            array_tile[_index]._sprite_name = "land01";

        array_tile[_index]._tileType = ground;
    }

    public void InitMap()
    {
        for (int i = 0; i < mapSize; i++)
        {
            array_tile[i].Init();
        }

        //수정을 해야 될 부분.
        DrawMap();
    }

    //맵 초기화.
    public void ClearMap()
    {
        if (_floor != null)
        {
            Destroy(_floor.gameObject);
            _floor = null;
        }

    }


    void SetTileType(int _index, int _type)
    {
        array_tile[_index]._tileType = _type;
    }


    void SetSubType(int _index, int _sub_type)
    {
        array_tile[_index]._tileSubType = _sub_type;
    }

    bool EqualXAxis(int _index01, int _index02) { return (_index01 / _mapWidth) == (_index02 / _mapWidth); }
    bool EqualYAxis(int _index01, int _index02) { return (_index01 % _mapWidth) == (_index02 % _mapWidth); }


    //잔디인지 검사.
    bool IsTileIndex(int _index, int _originIndex, int offsetX, int offsetY)
    {
        int h1 = _originIndex / _mapWidth;
        int w1 = _originIndex % _mapWidth;

        int h = _index / _mapWidth;
        int w = _index % _mapWidth;

        if (PosHelper.GetBoard(w1, h1, offsetX, offsetY) == null) return false;

        if (CheckTileSize(_index) &&
            array_tile[_index]._tileType != ground)
        {
            //언덕이 존재할 경우.
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                return false;
            else
                return true; ;
        }


        return false;
    }


    bool IsTile(int _index)
    {

        int h = _index / _mapWidth;
        int w = _index % _mapWidth;

        if (CheckTileSize(_index) &&
            array_tile[_index]._tileType != ground)
        {
            //언덕이 존재할 경우.
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                return false;
            else
                return true; ;
        }


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
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;
        if (PosHelper.GetBoard(w, h).IsHasScarp == true)
            return 16;

        int result = 0;


        //위
        if (PosHelper.GetBoard(w, h, 0, -1) == null ||
            (IsTile(_index - _mapWidth) && GetUpTileIndex(_index) != -1))
        {
            result += (int)TILE_MASK.UP;
        }

        //아래
        if (PosHelper.GetBoard(w, h, 0, 1) == null ||
            (IsTile(_index + _mapWidth) && GetDownTileIndex(_index) != -1))
        {
            result += (int)TILE_MASK.DOWN;
        }

        if (PosHelper.GetBoard(w, h, -1, 0) == null ||
            (IsTile(_index - 1) && GetLeftTileIndex(_index) != -1))
        {
            result += (int)TILE_MASK.LEFT;
        }

        if (PosHelper.GetBoard(w, h, 1, 0) == null ||
            (IsTile(_index + 1) && GetRightTileIndex(_index) != -1))
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
        {
            return "land01";
        }

        string name = "grass" + _type01.ToString() + "-" + _type02.ToString();
        return name;
    }

    void SetSpriteName(int _index, int _type, int _sub_type)
    {
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;

        if (PosHelper.GetBoard(w, h).IsHasScarp == true)
        {
            array_tile[_index]._sprite_name = CreateSpriteName(16, _sub_type);
            return;
        }

        array_tile[_index]._sprite_name = CreateSpriteName(_type, _sub_type);
    }


    void ChangeSideTileType(int _index)
    {
        int index = _index;

        //위.
        if (IsTileIndex(_index - _mapWidth, _index, 0,-1))   //(IsTile(_index - _mapWidth))
        {
            int w = (_index - _mapWidth) % _mapWidth;
            int h = (_index - _mapWidth) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(index - _mapWidth, 16, 0);

            int up_type = GetTileType(_index - _mapWidth);
            int up_sub_type = CheckSideTile(_index - _mapWidth, up_type);
            array_tile[index - _mapWidth]._tileType = up_type;
            array_tile[index - _mapWidth]._tileSubType = up_sub_type;
            SetSpriteName(index - _mapWidth, up_type, up_sub_type);
        }

        //아래.
        if (IsTileIndex(_index + _mapWidth, _index, 0, 1)) //(IsTile(_index + _mapWidth))
        {
            int w = (_index + _mapWidth) % _mapWidth;
            int h = (_index + _mapWidth) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(index + _mapWidth, 16, 0);

            int down_type = GetTileType(_index + _mapWidth);
            int down_sub_type = CheckSideTile(_index + _mapWidth, down_type);
            array_tile[index + _mapWidth]._tileType = down_type;
            array_tile[index + _mapWidth]._tileSubType = down_type;
            SetSpriteName(index + _mapWidth, down_type, down_sub_type);
        }

        //좌.
        if (IsTileIndex(_index - 1, _index, -1, 0)) //(IsTile(_index - 1))
        {
            int w = (_index - 1) % _mapWidth;
            int h = (_index - 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(index - 1, 16, 0);

            int left_type = GetTileType(_index - 1);
            int left_sub_type = CheckSideTile(_index - 1, left_type);
            array_tile[index - 1]._tileType = left_type;
            array_tile[index - 1]._tileSubType = left_sub_type;
            SetSpriteName(index - 1, left_type, left_sub_type);
        }

        //우.
        if (IsTileIndex(_index +1, _index, 1,0)) //(IsTile(_index + 1))
        {
            int w = (_index + 1) % _mapWidth;
            int h = (_index + 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(index + 1, 16, 0);

            int right_type = GetTileType(_index + 1);
            int right_sub_type = CheckSideTile(_index + 1, right_type);
            array_tile[index + 1]._tileType = right_type;
            array_tile[index + 1]._tileSubType = right_type;
            SetSpriteName(index + 1, right_type, right_sub_type);
        }


        //오른쪽 위.
        if (IsTileIndex(_index - _mapWidth + 1, _index, 1, -1)) //(IsTile(_index - _mapWidth + 1))
        {
            int w = (_index - _mapWidth + 1) % _mapWidth;
            int h = (_index - _mapWidth + 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(_index - _mapWidth + 1, 16, 0);

            int ur_type = GetTileType(_index - _mapWidth + 1);
            int ur_sub_type = CheckSideTile(_index - _mapWidth + 1, ur_type);
            array_tile[index - _mapWidth + 1]._tileType = ur_type;
            array_tile[index - _mapWidth + 1]._tileSubType = ur_sub_type;
            SetSpriteName(index - _mapWidth + 1, ur_type, ur_sub_type);

        }

        //오른쪽 아래.
        if (IsTileIndex(_index + _mapWidth + 1, _index, 1, 1)) //(IsTile(_index + _mapWidth + 1))
        {
            int w = (_index + _mapWidth + 1) % _mapWidth;
            int h = (_index + _mapWidth + 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(_index + _mapWidth + 1, 16, 0);

            int dr_type = GetTileType(_index + _mapWidth + 1);
            int dr_sub_type = CheckSideTile(_index + _mapWidth + 1, dr_type);
            array_tile[index + _mapWidth + 1]._tileType = dr_type;
            array_tile[index + _mapWidth + 1]._tileSubType = dr_sub_type;
            SetSpriteName(index + _mapWidth + 1, dr_type, dr_sub_type);
        }

        //왼쪽 아래.
        if (IsTileIndex(_index + _mapWidth - 1, _index, -1, 1)) //(IsTile(_index + _mapWidth - 1))
        {
            int w = (_index + _mapWidth - 1) % _mapWidth;
            int h = (_index + _mapWidth - 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(_index + _mapWidth - 1, 16, 0);

            int dl_type = GetTileType(_index + _mapWidth - 1);
            int dl_sub_type = CheckSideTile(_index + _mapWidth - 1, dl_type);
            array_tile[index + _mapWidth - 1]._tileType = dl_type;
            array_tile[index + _mapWidth - 1]._tileSubType = dl_sub_type;
            SetSpriteName(index + _mapWidth - 1, dl_type, dl_sub_type);
        }

        //왼쪽 위.
        if (IsTileIndex(_index - _mapWidth - 1, _index, -1, -1)) //(IsTile(_index - _mapWidth - 1))
        {
            int w = (_index - _mapWidth - 1) % _mapWidth;
            int h = (_index - _mapWidth - 1) / _mapWidth;
            if (PosHelper.GetBoard(w, h).IsHasScarp == true)
                SetSpriteName(_index - _mapWidth - 1, 16, 0);

            int ul_type = GetTileType(_index - _mapWidth - 1);
            int ul_sub_type = CheckSideTile(_index - _mapWidth - 1, ul_type);
            array_tile[index - _mapWidth - 1]._tileType = ul_type;
            array_tile[index - _mapWidth - 1]._tileSubType = ul_sub_type;
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
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;

        int result = 0;

        if (PosHelper.GetBoard(w, h, 1, -1) == null ||
            IsTile(_index - _mapWidth + 1))
            result += (int)SIDE_MASK.UR;

        if (PosHelper.GetBoard(w, h, 1, +1) == null ||
            IsTile(_index + _mapWidth + 1))
            result += (int)SIDE_MASK.DR;

        if (PosHelper.GetBoard(w, h, -1, +1) == null ||
            IsTile(_index + _mapWidth - 1))
            result += (int)SIDE_MASK.DL;

        if (PosHelper.GetBoard(w, h, -1, -1) == null || 
            IsTile(_index - _mapWidth - 1))
            result += (int)SIDE_MASK.UL;

        return result;
    }

    //오른쪽 위 검사.
    bool CheckFirstSide(int _index)
    {
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;


        if (PosHelper.GetBoard(w, h, 1, -1) == null ||
            IsTile(_index - _mapWidth + 1))
            return true;

        return false;
    }

    //오른쪽 아래 검사.
    bool CheckSecondSide(int _index)
    {
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;


        if (PosHelper.GetBoard(w, h, 1, 1) == null)
            return true;

        if (IsTile(_index + _mapWidth + 1))
            return true;

        return false;
    }

    //왼쪽 아래 검사.
    bool CheckThirdSide(int _index)
    {
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;


        if (PosHelper.GetBoard(w, h, -1, 1) == null)
            return true;

        if (IsTile(_index + _mapWidth - 1))
            return true;

        return false;
    }

    //왼쪽 위 검사.
    bool CheckFourthSide(int _index)
    {
        int w = _index % _mapWidth;
        int h = _index / _mapWidth;


        if (PosHelper.GetBoard(w, h, -1, -1) == null)
            return true;


        if (IsTile(_index - _mapWidth - 1))
            return true;
        return false;
    }

    public void SetTile(int indexX, int indexY, Board in_data)
    {
        int index = (indexY * _mapWidth) + indexX;

        if (in_data.IsActiveBoard || in_data.IsHasScarp)
        {
            if (PosHelper.GetBoard(indexX, indexY).IsHasScarp)
                array_tile[index]._sprite_name = "land01";
            else if (indexX % 2 == 0 && indexY % 2 == 0)
                array_tile[index]._sprite_name = "land02";
            else if (indexX % 2 == 1 && indexY % 2 == 1)
                array_tile[index]._sprite_name = "land02";
            else
                array_tile[index]._sprite_name = "land01";

            array_tile[index]._tileType = 16;
        }
        else
        {
            array_tile[index]._tileType = GetTileType(index);
            array_tile[index]._tileSubType = CheckSideTile(index, array_tile[index]._tileType);
            SetSpriteName(index, array_tile[index]._tileType, array_tile[index]._tileSubType);
            ChangeSideTileType(index);
        }

        DrawMap();
    }

    public int GetTileType(int indexX, int indexY)
    {
        return array_tile[(indexY * _mapWidth) + indexX]._tileType;
    }

    void DrawMap()
    {
        //수정해야 될 부분.
        _floorClass.Change();
    }


}
