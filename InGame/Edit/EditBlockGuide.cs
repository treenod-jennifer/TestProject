using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditBlockGuide : MonoBehaviour
{
    public int inX;
    public int inY;

    [System.NonSerialized] public static bool reflesh = false;
    [System.NonSerialized] public int _index;

    public UISprite _spriteBody;

    //데코 정보와 관련된 블럭 정보까지 받기위한 클래스
    private class EditBlockAndDecoInfo
    {
        //해당 데코를 가지고 있는 블럭 정보
        public BlockInfo blockInfo = null;

        //데코 정보
        public DecoInfo decoInfo = null;
    }

    void Start()
    {
        _spriteBody.alpha = 0.0f;
        _spriteBody.depth = (int)GimmickDepth.UI_LABEL;
    }

    void OnEdit()
    {
        EditMode mode = EditManager.instance.eMode;
        BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(inX, inY);

        BlockOverwrite(mode, blockInfo);
        
        switch (mode)
        {
            case EditMode.PLANT:
                {
                    int setPlnat = EditManager.instance.paintCount[(int)mode] % 4;
                    int plnatCount = (blockInfo.type == (int)BlockType.PLANT) ? blockInfo.count : 0;

                    if ( plnatCount != setPlnat || blockInfo.subType != EditManager.instance.plantType || EditManager.instance.candyType || EditManager.instance.duckType )
                    {
                        if ((plnatCount == setPlnat && EditManager.instance.candyType && blockInfo.subType == 6) ||
                            (plnatCount == setPlnat && EditManager.instance.duckType && blockInfo.subType == 7))
                            break;

                        blockInfo.InitBlock();
                        if (setPlnat == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type =  (int)BlockType.PLANT;
                            blockInfo.count = setPlnat;
                            blockInfo.colorType = EditManager.instance.boxColorType;
                            if (EditManager.instance.candyType) blockInfo.subType = (int)PLANT_TYPE.CANDY;
                            else if (EditManager.instance.duckType) blockInfo.subType = (int)PLANT_TYPE.DUCK;
                            else if (EditManager.instance.plantType == (int)PLANT_TYPE.WATER) blockInfo.subType = (int)PLANT_TYPE.WATER;
                            else if (EditManager.instance.plantType == (int)PLANT_TYPE.COIN) blockInfo.subType = (int)PLANT_TYPE.COIN;
                            else if (EditManager.instance.plantType == (int)PLANT_TYPE.HealPotion) blockInfo.subType = (int)PLANT_TYPE.HealPotion;
                            else if (EditManager.instance.plantType == (int)PLANT_TYPE.SkillPotion) blockInfo.subType = (int)PLANT_TYPE.SkillPotion;
                            else if (EditManager.instance.plantType == (int)PLANT_TYPE.SPACESHIP) blockInfo.subType = (int)PLANT_TYPE.SPACESHIP;
                            else blockInfo.subType = 0;

                            if(blockInfo.subType != 0)
                                blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.ICE_PLANT:
                {
                    int setPlnat = EditManager.instance.paintCount[(int)mode] % 4;
                    int plnatCount = (blockInfo.type == (int)BlockType.PLANT && blockInfo.subType == (int)PLANT_TYPE.ICE_APPLE) ? blockInfo.count : 0;

                    if (plnatCount != setPlnat)
                    {
                        blockInfo.InitBlock();
                        if (setPlnat == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.subType = (int)PLANT_TYPE.ICE_APPLE;
                            blockInfo.count = setPlnat;
                            blockInfo.index = 0;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;


            case EditMode.BOMB_PLANT:
                {
                    int setPlant = EditManager.instance.paintCount[(int)mode] % 4;
                    // subType 비교
                    bool differentSubType = blockInfo.type == (int)BlockType.PLANT && 
                       !((blockInfo.subType == (int)PLANT_TYPE.LINE && EditManager.instance.blockType == 0) ||
                         (blockInfo.subType == (int)PLANT_TYPE.LINE_V && EditManager.instance.blockType == 2) ||
                         (blockInfo.subType == (int)PLANT_TYPE.LINE_H && EditManager.instance.blockType == 3) ||
                         (blockInfo.subType == (int)PLANT_TYPE.CIRCLE && EditManager.instance.blockType == 1));
                    // 단계 비교
                    int plantCount = (blockInfo.type == (int)BlockType.PLANT && 
                        ( blockInfo.subType == (int)PLANT_TYPE.LINE|| 
                        blockInfo.subType == (int)PLANT_TYPE.LINE_V || 
                        blockInfo.subType == (int)PLANT_TYPE.LINE_H || 
                        blockInfo.subType == (int)PLANT_TYPE.CIRCLE )) ? blockInfo.count : 0;

                    if (plantCount != setPlant || differentSubType)
                    {
                        blockInfo.InitBlock();
                        if (setPlant == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            PLANT_TYPE type = PLANT_TYPE.LINE;
                            if (EditManager.instance.blockType == 1) type = PLANT_TYPE.CIRCLE;
                            else if (EditManager.instance.blockType == 2) type = PLANT_TYPE.LINE_V;
                            else if (EditManager.instance.blockType == 3) type = PLANT_TYPE.LINE_H;

                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.count = setPlant;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.subType = (int)type;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.PLANT_APPLE:
               // if (BlockManager.boards[inX, inY].IsActiveBoard)
                {
                    int setPlnat   = EditManager.instance.paintCount[(int)mode] % 4;
                    int blockCount;
                    //제거일 경우 타입만 같으면 제거할 수 있고 설치일 경우 타입, 카운트까지 같으면 설치 불가능하도록 예외 추가
                    if (setPlnat == 0)
                    {
                        blockCount = blockInfo.type == (int)BlockType.PLANT && blockInfo.subType == (int)PLANT_TYPE.APPLE ? blockInfo.count : 0;
                    }
                    else
                    {
                        blockCount = blockInfo.type       == (int)BlockType.PLANT 
                                     && blockInfo.subType == (int)PLANT_TYPE.APPLE
                                     && blockInfo.count   == setPlnat 
                                     && blockInfo.index   == (int)EditManager.instance.appleCount 
                            ? blockInfo.count : 0;
                    }

                    if (blockCount != setPlnat)
                    {
                        blockInfo.InitBlock();
                        if (setPlnat == 0)
                        {
                            if(ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.subType = (int)PLANT_TYPE.APPLE;
                            blockInfo.count = setPlnat;
                            blockInfo.index = (int)EditManager.instance.appleCount;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PLANT_KEY:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setPlnat = EditManager.instance.paintCount[(int)mode] % 4;
                    int plnatCount = (blockInfo.type == (int)BlockType.PLANT && blockInfo.subType == (int)PLANT_TYPE.KEY) ? blockInfo.count : 0;

                    if (plnatCount != setPlnat)
                    {
                        blockInfo.InitBlock();
                        if (setPlnat == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.subType = (int)PLANT_TYPE.KEY;
                            blockInfo.count = setPlnat;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PLANT_COIN:
                {
                    int setPlnat = EditManager.instance.paintCount[(int)mode] % 4;
                    int plnatCount = ((blockInfo.type == (int)BlockType.PLANT && blockInfo.subType == (int)PLANT_TYPE.COIN_BAG)) ? blockInfo.count : 0;

                    if (plnatCount != setPlnat)
                    {
                        blockInfo.InitBlock();
                        if (setPlnat == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.subType = (int)PLANT_TYPE.COIN_BAG;
                            blockInfo.count = setPlnat;
                            blockInfo.index = (int)EditManager.instance.CoinCount;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.CRACK1:
                if (ManagerBlock.instance.dicCountCrack.Count > 0)
                    break;
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCrack = EditManager.instance.paintCount[(int)mode] % 4;
                    int crackCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.CARCK1)
                        {
                            crackCount = deco.count;
                            break;
                        }
                    }

                    if (setCrack != crackCount)
                    {
                        if (setCrack != 0)
                        {
                            bool hasCrack1 = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CARCK1)
                                {
                                    deco.count = setCrack;
                                    hasCrack1 = true;
                                }
                            }

                            if (!hasCrack1)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.CARCK1;
                                deco.count = setCrack;

                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CARCK1)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.COUNT_CRACK:
                if (ManagerBlock.instance.IsStageTarget(TARGET_TYPE.CRACK))
                    break;

                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCrack = EditManager.instance.paintCount[(int)mode] % 2;
                    int crackIndex = EditManager.instance.blockIndex;

                    if (setCrack != 0)
                    {
                        bool hasCrack1 = false;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.COUNT_CRACK && deco.index == crackIndex)
                            {
                                deco.index = crackIndex;
                                hasCrack1 = true;
                            }
                        }

                        if (!hasCrack1)
                        {
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.COUNT_CRACK;
                            deco.index = crackIndex;
                            blockInfo.ListDeco.Add(deco);
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.COUNT_CRACK && deco.index == crackIndex)
                            {
                                blockInfo.ListDeco.Remove(deco);
                                break;
                            }
                        }
                    }
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }
                break;

            case EditMode.GRASSFENCEBLOCK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setFence = EditManager.instance.paintCount[(int)mode] % 2;

                    if (setFence != 0)
                    {
                        EditBlockAndDecoInfo sideInfo = GetSideDeco(blockInfo, (BlockDirection)EditManager.instance.blockIndex);
                        bool hasFence = HasSideDeco(mode, sideInfo);

                        if (hasFence == false)
                        {
                            if (sideInfo != null) OverWrite_SideDeco(mode, sideInfo);

                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.GRASSFENCEBLOCK;
                            deco.index     = EditManager.instance.blockIndex;
                            deco.count     = (int)EditManager.instance.fenceCount;

                            blockInfo.ListDeco.Add(deco);
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK)
                            {
                                blockInfo.ListDeco.Remove(deco);
                                break;
                            }
                        }
                    }
                    _spriteBody.alpha = 1;
                    reflesh = true;
                }
                break;

            case EditMode.FENCEBLOCK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setFence = EditManager.instance.paintCount[(int)mode] % 2;
                    
                    if (setFence != 0)
                    {
                        EditBlockAndDecoInfo sideInfo = GetSideDeco(blockInfo, (BlockDirection)EditManager.instance.blockIndex);

                        bool hasFence = HasSideDeco(mode, sideInfo);
                        if (hasFence == false)
                        {
                            if (sideInfo != null) OverWrite_SideDeco(mode, sideInfo);
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.FENCEBLOCK;
                            deco.index     = EditManager.instance.blockIndex;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.FENCEBLOCK)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }
                }
                break;
            case EditMode.GRASS:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCrack = EditManager.instance.paintCount[(int)mode] % 3;
                    int crackCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.GRASS)
                        {
                            crackCount = deco.count;
                            break;
                        }
                    }
                    if (setCrack != crackCount)
                    {
                        if (setCrack != 0)
                        {
                            bool hasCrack1 = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.GRASS)
                                {
                                    deco.count = setCrack;
                                    hasCrack1 = true;
                                }
                            }
                            if (!hasCrack1)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.GRASS;
                                deco.count = setCrack;

                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.GRASS)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.STATUE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCrack = EditManager.instance.paintCount[(int)mode] % 3;
                    int crackCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.STATUE && deco.type == EditManager.instance.statueIndex)
                        {
                            crackCount = deco.count + 1;
                            break;
                        }
                    }
                    if (setCrack != crackCount)
                    {
                        if (setCrack != 0)
                        {
                            bool hasCrack1 = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.STATUE && deco.type == EditManager.instance.statueIndex)
                                {
                                    deco.count = setCrack - 1;
                                    hasCrack1 = true;
                                }
                            }
                            if (!hasCrack1)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.STATUE;
                                deco.index = EditManager.instance.blockIndex;
                                deco.count = setCrack - 1;
                                deco.type = EditManager.instance.blockType;

                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.STATUE && deco.type == EditManager.instance.statueIndex)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }

                break;
            case EditMode.NET:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNet = EditManager.instance.paintCount[(int)mode] % 3;
                    int netCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.NET)
                        {
                            netCount = deco.count;
                            break;
                        }
                    }

                    if (setNet != netCount)
                    {
                        if (setNet != 0)
                        {
                            bool hasNet = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.NET)
                                {
                                    deco.count = setNet;
                                    hasNet = true;
                                }
                            }

                            if (!hasNet)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.NET;
                                deco.count = setNet;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.NET)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.CARPET:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCarpet = EditManager.instance.paintCount[(int)mode] % 2;
                    int carpetCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.CARPET)
                        {
                            carpetCount = 1;
                            break;
                        }
                    }

                    if (setCarpet != carpetCount)
                    {
                        if (setCarpet != 0)
                        {
                            bool hasCarpet = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CARPET)
                                {
                                    hasCarpet = true;
                                }
                            }

                            if (!hasCarpet)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.CARPET;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CARPET)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.KEY_HOLE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setKeyHole = EditManager.instance.paintCount[(int)mode] % 2;
                    int holeCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.KEY_EXIT)
                        {
                            holeCount = 1;
                            break;
                        }
                    }

                    if (setKeyHole != holeCount)
                    {
                        if (setKeyHole != 0)
                        {
                            bool hasNet = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.KEY_EXIT)
                                {
                                    hasNet = true;
                                }
                            }

                            if (!hasNet)
                            {
                                foreach (var decoSpaceShipExit in blockInfo.ListDeco)
                                {
                                    if (decoSpaceShipExit.BoardType == (int)BoardDecoType.SPACESHIP_EXIT)
                                    {
                                        blockInfo.ListDeco.Remove(decoSpaceShipExit);
                                        break;
                                    }
                                }
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.KEY_EXIT;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.KEY_EXIT)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.BOARD:
                int ground = ManagerBlock.boards[inX, inY].IsActiveBoard ? 1 : 0;
                int setGround = EditManager.instance.paintCount[(int)mode] % 2;

                if(ground != setGround)
                {
                    blockInfo.InitBlock();
                    if (setGround == 1)
                    {
                        ManagerBlock.boards[inX, inY].IsActiveBoard = true;
                        ManagerBlock.boards[inX, inY].IsHasScarp = false;

                        blockInfo.isActiveBoard = 1;
                        blockInfo.isScarp = 0;
                        blockInfo.type = (int)BlockType.NORMAL;
                        blockInfo.colorType = (int)BlockColorType.RANDOM;

                        ManagerBlock.instance.SortListBlock();
                    }
                    else
                    {
                        ManagerBlock.boards[inX, inY].IsActiveBoard = false;
                        blockInfo.isActiveBoard = 0;
                        blockInfo.type = (int)BlockType.NONE;
                        blockInfo.ListDeco.Clear();
                    }
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }

                break;

            case EditMode.SCARP:

                int scarp = ManagerBlock.boards[inX, inY].IsHasScarp ? 1 : 0;
                int setScarp = EditManager.instance.paintCount[(int)mode] % 2;

                if (scarp != setScarp && !ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    ManagerBlock.boards[inX, inY].IsHasScarp = setScarp == 1;
                    blockInfo.isScarp = setScarp;
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }

                break;

            case EditMode.START:

                int blockStart = ManagerBlock.boards[inX, inY].IsStartBoard ? 1 : 0;
                int setStart = EditManager.instance.paintCount[(int)mode] % 2;

                //현재 선택한 블럭의 출발 정보 저장할 변수.
                DecoInfo selectDecoInfo = null;
                if (EditManager.instance.bShowStartInfo == true)
                {
                    if (blockStart == 1)
                    {
                        EditManager.instance.decoInfo = null;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.START ||
                                deco.BoardType == (int)BoardDecoType.BLOCK_GENERATOR)
                            {
                                selectDecoInfo = deco;
                                break;
                            }
                        }
                        _spriteBody.alpha = 1;
                    }
                    //정보가 다를 경우에만 화면 갱신해줌.
                    if (selectDecoInfo != EditManager.instance.decoInfo)
                    {
                        EditManager.instance.decoInfo = selectDecoInfo;
                        reflesh = true;
                    }
                }
                else
                {
                    if (setStart == 1)
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.BLOCK_GENERATOR || deco.BoardType == (int)BoardDecoType.POTAL_OUT)
                            {
                                selectDecoInfo = deco;
                                blockInfo.ListDeco.Remove(deco);
                                blockStart = 0;
                                break;
                            }
                            else if (deco.BoardType == (int)BoardDecoType.START)
                            {
                                selectDecoInfo = deco;
                                if (deco.index != EditManager._startPositionBlockType ||
                                    deco.type != EditManager._startPositionBlockType2 ||
                                    deco.count != EditManager._startPositionBlockColor)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    blockStart = 0;
                                }
                                else
                                {
                                    blockStart = 1;
                                }

                                break;
                            }
                        }
                    }

                    if (blockStart != setStart)// && BlockManager.boards[inX, inY].IsActiveBoard)
                    {
                        if(setStart == 1)
                        {
                            ManagerBlock.boards[inX, inY].IsStartBoard = true;
                            DecoInfo boardInfo = new DecoInfo();
                            boardInfo.BoardType = (int)BoardDecoType.START;
                            boardInfo.index = EditManager._startPositionBlockType; //TODO 설정에서 값을 가져와야함
                            boardInfo.count = EditManager._startPositionBlockColor;
                            
                            //출발2값
                            boardInfo.type = EditManager._startPositionBlockType2;

                            blockInfo.ListDeco.Add(boardInfo);
                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                        else
                        {
                            foreach(var deco in blockInfo.ListDeco)
                            {
                                if(deco.BoardType == (int)BoardDecoType.START)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    reflesh = true;
                                    _spriteBody.alpha = 1;
                                    break;
                                }
                            }
                            ManagerBlock.boards[inX, inY].IsStartBoard = false;
                            ManagerBlock.boards[inX, inY].startBlockType = 0;
                            ManagerBlock.boards[inX, inY].startBlockType2 = 0;
                        }
                    }
                }
                break;

            case EditMode.BLOCKGENERATOR:
                {
                    int blockGenerator = ManagerBlock.boards[inX, inY].IsStartBoard ? 1 : 0;
                    int setGenerator = EditManager.instance.paintCount[(int)mode] % 2;

                    if (setGenerator == 1)
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.START || deco.BoardType == (int)BoardDecoType.POTAL_OUT)
                            {
                                blockInfo.ListDeco.Remove(deco);
                                blockGenerator = 0;
                                break;
                            }

                            if (deco.BoardType == (int)BoardDecoType.BLOCK_GENERATOR)
                            {
                                if (deco.tempData_1 != EditManager.instance.SelectGeneratorIdx)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    blockGenerator = 0;
                                }
                                else
                                {
                                    blockGenerator = 1;
                                }
                                break;
                            }
                        }
                    }

                    if (blockGenerator != setGenerator)
                    {
                        if (setGenerator == 1)
                        {
                            int gIdx = EditManager.instance.SelectGeneratorIdx;
                            ManagerBlock.boards[inX, inY].IsStartBoard = true;
                            DecoInfo boardInfo = new DecoInfo();
                            boardInfo.BoardType = (int)BoardDecoType.BLOCK_GENERATOR;
                            boardInfo.tempData_1 = gIdx;

                            //출발 설정
                            if (EditManager.instance.dicToggleData_BlockGenerator != null)
                            {
                                int[] arrStartBlockType = EditManager.instance.GetArrStartBlockType_AtDicToggleData_BlockGenerator();
                                boardInfo.index = arrStartBlockType[0];
                                boardInfo.type = arrStartBlockType[1];

                                //생성기에서 생성될 수 있는 블럭의 컬러를 가져옴
                                var item = EditManager.instance.GetListColorData_AtGeneratorBlockColor(gIdx);
                                boardInfo.count = item.Item1;
                                boardInfo.listBlockColorData = item.Item2;
                            }

                            blockInfo.ListDeco.Add(boardInfo);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.BLOCK_GENERATOR)
                                {
                                    blockInfo.ListDeco.Remove(deco);

                                    reflesh = true;
                                    _spriteBody.alpha = 1;
                                    break;
                                }
                            }
                            ManagerBlock.boards[inX, inY].IsStartBoard = false;
                            ManagerBlock.boards[inX, inY].startBlockType = 0;
                            ManagerBlock.boards[inX, inY].startBlockType2 = 0;
                        }
                    }
                }
                break;
            case EditMode.PotalIn:

                if(ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setPotal   = EditManager.instance.paintCount[(int)mode] % 2 == 0 ? 0 : EditManager.instance.blockIndex;
                    int potalindex = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.POTAL_IN)
                        {
                            potalindex =  deco.index;
                            break;
                        }
                    }

                    if (setPotal != potalindex)
                    {
                        if(setPotal != 0)
                        {
                            bool hasPotal = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.POTAL_IN)
                                {
                                    deco.index = EditManager.instance.blockIndex;
                                    hasPotal   = true;
                                }
                            }

                            if (!hasPotal)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.POTAL_IN;
                                deco.index     = EditManager.instance.blockIndex;

                                blockInfo.ListDeco.Add(deco);                              
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.POTAL_IN)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }

                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PotalOut:

                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setPotal   = EditManager.instance.paintCount[(int)mode] % 2 == 0 ? 0 : EditManager.instance.blockIndex;
                    int potalindex = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.POTAL_OUT)
                        {
                            potalindex = deco.index;
                            break;
                        }
                    }

                    if (setPotal != potalindex)
                    {
                        if (setPotal != 0)
                        {
                            bool hasPotal = false;
                            DecoInfo removeDeco = new DecoInfo();

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.POTAL_OUT)
                                {
                                    deco.index = EditManager.instance.blockIndex;
                                    hasPotal   = true;
                                }
                                if (deco.BoardType == (int)BoardDecoType.START || deco.BoardType == (int)BoardDecoType.BLOCK_GENERATOR)
                                {
                                    removeDeco = deco;
                                }
                            }

                            if (removeDeco.BoardType != (int)BoardDecoType.NONE)
                                blockInfo.ListDeco.Remove(removeDeco);

                            if (!hasPotal)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.POTAL_OUT;
                                deco.index     = EditManager.instance.blockIndex;

                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.POTAL_OUT)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.EMPTY2:

                break;
            case EditMode.EMPTY3:

                break;
            case EditMode.BLOCK:
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.NORMAL) ? (int)blockInfo.colorType : 0;

                    blockInfo.InitBlock();
                    if (setBlock == 0)
                    {
                        blockInfo.type = (int)BlockType.NONE;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.bombType = (int)BlockBombType.NONE;
                        blockInfo.subType = 0;
                        blockInfo.index = 0;
                        blockInfo.count = 0;

                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.ICE)
                            {
                                blockInfo.ListDeco.Remove(deco);
                                break;
                            }
                        }
                    }
                    else if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                    {
                        blockInfo.type = (int)BlockType.NORMAL;
                        blockInfo.colorType = (int)BlockColorType.RANDOM;
                        blockInfo.bombType = (int)BlockBombType.NONE;
                        blockInfo.subType = 0;
                        blockInfo.index = 0;
                        blockInfo.count = 0;
                    }
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }
                break;

            case EditMode.BLOCK_BLACK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.BLOCK_BLACK || blockInfo.type == (int)BlockType.GROUND_BLOCKBLACK) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            if (EditManager.instance.groundCount == 0)
                            {
                                blockInfo.type = (int)BlockType.BLOCK_BLACK;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.GROUND_BLOCKBLACK;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = EditManager.instance.groundCount;
                                blockInfo.subType = (int)GROUND_TYPE.BlOCKBLACK;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.BLOCKTYPE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 7;
                    int blockCount = (blockInfo.type == (int)BlockType.NORMAL) ? (int)blockInfo.colorType : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.bombType = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = setBlock;
                            blockInfo.bombType = (int)BlockBombType.NONE;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.KEY:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.KEY) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.KEY;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.CANDY:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.CANDY ||
                        (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.CANDY)) ? 1 : 0;

                    if (blockCount != setBlock
                        || ((setBlock != 0) && blockInfo.count != EditManager.instance.groundCount))
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.colorType = (int)BlockColorType.NONE;

                            if (EditManager.instance.groundCount == 0)
                            {
                                blockInfo.type = (int)BlockType.CANDY;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)GROUND_TYPE.CANDY;
                                blockInfo.count = EditManager.instance.groundCount;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.DUCK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.DUCK ||
                        (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.DUCK)) ? 1 : 0;

                    if (blockCount != setBlock
                        || ((setBlock != 0) && blockInfo.count != EditManager.instance.groundCount))
                    { 
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.subType = 0;
                            blockInfo.count = 0;
                        }
                        else
                        {
                            blockInfo.colorType = (int)BlockColorType.NONE;

                            if (EditManager.instance.groundCount == 0)
                            {
                                blockInfo.type = (int)BlockType.DUCK;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)GROUND_TYPE.DUCK;
                                blockInfo.count = EditManager.instance.groundCount;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.APPLE:
                //if (BlockManager.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount;
                    //제거일 경우 타입만 같으면 제거할 수 있고 설치일 경우 타입, 카운트까지 같으면 설치 불가능하도록 예외 추가
                    if (setBlock == 0)
                    {
                        blockCount = blockInfo.type == (int)BlockType.APPLE ? 1 : 0;
                    }
                    else
                    {
                        blockCount = blockInfo.type == (int)BlockType.APPLE && blockInfo.count == (int)EditManager.instance.appleCount? 1 : 0;
                    }

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.APPLE;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = (int)EditManager.instance.appleCount;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.COIN:
                //if (BlockManager.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.BLOCK_COIN) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.BLOCK_COIN;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count =  0;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.COIN_BAG:                
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.COIN_BAG) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.COIN_BAG;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = EditManager.instance.CoinCount;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.ICE_APPLE:
                //if (BlockManager.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.ICE_APPLE) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.ICE_APPLE;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = 0;// EditManager.instance.blockCount;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.GROUND:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 4;
                    int blockCount = (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.NORMAL) ? blockInfo.count : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.GROUND;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = setBlock;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.GROUND_APPLE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCount   = EditManager.instance.paintCount[(int)mode] % 4;
                    //제거일 경우 타입만 같으면 제거할 수 있고 설치일 경우 타입, 서브타입, 카운트, 인덱스까지 같으면 설치 불가능하도록 예외 추가
                    int blockCount;
                    if (setCount == 0)
                    {
                        blockCount = blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.APPLE ? blockInfo.count : 0;
                    }
                    else
                    {
                        blockCount = blockInfo.type       == (int)BlockType.GROUND 
                                     && blockInfo.subType == (int)GROUND_TYPE.APPLE
                                     && blockInfo.count == setCount 
                                     && blockInfo.index == (int)EditManager.instance.appleCount 
                            ? blockInfo.count : 0;
                    }

                    if (blockCount != setCount)
                    {
                        blockInfo.InitBlock();
                        if (setCount == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.count = setCount;
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.index = (int)EditManager.instance.appleCount;
                                blockInfo.subType = (int)GROUND_TYPE.APPLE;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.GROUND_ICE_APPLE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCount = EditManager.instance.paintCount[(int)mode] % 4;
                    int plantCount = (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.ICE_APPLE) ? blockInfo.count : 0;

                    if (plantCount != setCount)
                    {
                        blockInfo.InitBlock();
                        if (setCount == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.count = setCount;
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)GROUND_TYPE.ICE_APPLE;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.GROUND_BOMB:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setCount = EditManager.instance.paintCount[(int)mode] % 4;
                    
                    //제거일 경우 타입만 같으면 제거할 수 있고 설치일 경우 타입, 서브타입까지 같으면 설치 불가능하도록 예외 추가
                    int blockCount;
                    if (setCount == 0)
                    {
                        blockCount = (blockInfo.type == (int)BlockType.GROUND && (
                        blockInfo.subType == (int)GROUND_TYPE.LINE ||
                        blockInfo.subType == (int)GROUND_TYPE.LINE_V ||
                        blockInfo.subType == (int)GROUND_TYPE.LINE_H ||
                        blockInfo.subType == (int)GROUND_TYPE.CIRCLE
                        )) ? blockInfo.count : 0;
                    }
                    else
                    {
                        GROUND_TYPE type = GROUND_TYPE.LINE;
                        if(EditManager.instance.blockType       == 0) type = GROUND_TYPE.LINE;
                        else if (EditManager.instance.blockType == 1) type = GROUND_TYPE.CIRCLE;
                        else if (EditManager.instance.blockType == 2) type = GROUND_TYPE.LINE_V;
                        else if (EditManager.instance.blockType == 3) type = GROUND_TYPE.LINE_H;
                        
                        blockCount = blockInfo.type       == (int)BlockType.GROUND 
                                     && blockInfo.subType == (int)type
                                     && blockInfo.count   == setCount
                            ? blockInfo.count : 0;
                    }

                    if (blockCount != setCount)
                    {
                        blockInfo.InitBlock();
                        if (setCount == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                GROUND_TYPE type = GROUND_TYPE.LINE;
                                if(EditManager.instance.blockType == 0) type = GROUND_TYPE.LINE;
                                else if (EditManager.instance.blockType == 1) type = GROUND_TYPE.CIRCLE;
                                else if (EditManager.instance.blockType == 2) type = GROUND_TYPE.LINE_V;
                                else if (EditManager.instance.blockType == 3) type = GROUND_TYPE.LINE_H;


                                blockInfo.count = setCount;
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)type;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.JEWEL:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 5;
                    int plantCount = (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.JEWEL) ? blockInfo.count : 0;

                    if (plantCount != setNoColor)
                    {
                        blockInfo.InitBlock();
                        if (setNoColor == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.count = setNoColor;
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)GROUND_TYPE.JEWEL;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.Event_Block:
                //if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 5;
                    int plantCount = (blockInfo.type == (int)BlockType.BLOCK_EVENT || blockInfo.type == (int)BlockType.BLOCK_EVENT_GROUND) ? (blockInfo.count + 1) : 0;

                    if (((EditManager.instance.groundCount == 0 || blockInfo.count <= 0) && blockInfo.type == (int)BlockType.BLOCK_EVENT_GROUND)
                        || ((EditManager.instance.groundCount > 0 && blockInfo.count > 0) && blockInfo.type == (int)BlockType.BLOCK_EVENT)
                        || plantCount != setNoColor)
                    {
                        blockInfo.InitBlock();
                        if (setNoColor == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.colorType = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                        }
                        else
                        {
                            //if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.count = setNoColor - 1;
                                blockInfo.colorType = 0;
                                blockInfo.subType = 0;
                                //blockInfo.index = EditManager.instance.blockIndex;
                            }

                            if (EditManager.instance.groundCount == 0 || blockInfo.count <= 0)
                                blockInfo.type = (int)BlockType.BLOCK_EVENT;
                            else
                                blockInfo.type = (int)BlockType.BLOCK_EVENT_GROUND;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.Event_Stone:
                //if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 3;
                    int plantCount = (blockInfo.type == (int)BlockType.BLOCK_EVENT_STONE) ? blockInfo.count : 0;

                    if (plantCount != setNoColor)
                    {
                        blockInfo.InitBlock();
                        if (setNoColor == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                        }
                        else
                        {                            
                                blockInfo.count = setNoColor;
                                blockInfo.type = (int)BlockType.BLOCK_EVENT_STONE;
                                blockInfo.subType = 0;                            
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.ADVENTURE_POTION_HEAL:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setPotion = EditManager.instance.paintCount[(int)mode] % 2;
                    int potionType = (blockInfo.type == (int)BlockType.ADVENTURE_POTION_HEAL) ? blockInfo.index : 0;

                    if (setPotion != potionType)
                    {
                        blockInfo.InitBlock();
                        if (setPotion == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.ADVENTURE_POTION_HEAL;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                            blockInfo.index = setPotion;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.ADVENTURE_POTION_SKILL:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setPotion = EditManager.instance.paintCount[(int)mode] % 2;
                    int potionType = (blockInfo.type == (int)BlockType.ADVENTURE_POTION_HEAL) ? blockInfo.index : 0;

                    if (setPotion != potionType)
                    {
                        blockInfo.InitBlock();
                        if (setPotion == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                                blockInfo.index = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.ADVENTURE_POTION_SKILL;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                            blockInfo.index = setPotion;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.DYNAMITE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 8;
                    int plantCount = (blockInfo.type == (int)BlockType.BLOCK_DYNAMITE) ? blockInfo.colorType : 0;

                    if ((plantCount != setNoColor) ||
                        ((plantCount != 0) && ((blockInfo.count != EditManager.instance.blockCount) || (blockInfo.index != EditManager.instance.blockIndex))))
                    {
                        // 같은 째각타입이어도 라이프나 시간이 다른 경우

                        blockInfo.InitBlock();
                        if (setNoColor == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {                            
                            blockInfo.colorType = setNoColor;
                            blockInfo.type = (int)BlockType.BLOCK_DYNAMITE;
                            blockInfo.subType = 0;
                            blockInfo.count = EditManager.instance.blockCount;
                            blockInfo.index = EditManager.instance.blockIndex;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.WATER:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setWater = EditManager.instance.paintCount[(int)mode] % 2;
                    int waterCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.WATER)
                        {
                            waterCount = deco.count;
                            break;
                        }
                    }

                    if (setWater != waterCount)
                    {
                        if (setWater != 0)
                        {
                            bool hasWater = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.WATER)
                                {
                                    deco.count = setWater;
                                    hasWater = true;
                                }
                            }

                            if (!hasWater)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.WATER;
                                deco.count = setWater;

                                bool hasNetDeco = false;
                                foreach (var item in blockInfo.ListDeco)
                                {
                                    if (item.BoardType == (int) BoardDecoType.ICE ||
                                        item.BoardType == (int) BoardDecoType.NET ||
                                        item.BoardType == (int) BoardDecoType.RANDOM_BOX)
                                    {
                                        hasNetDeco = true;
                                        break;
                                    }
                                }
                                
                                if (blockInfo.type == (int) BlockType.NORMAL && blockInfo.bombType == (int)BlockBombType.NONE && !hasNetDeco)
                                {
                                    blockInfo.type = (int)BlockType.NONE;
                                    blockInfo.colorType = 0;
                                    blockInfo.count = 0;
                                    blockInfo.index = 0;
                                    blockInfo.subType = 0;
                                }
                                
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.WATER)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.GROUND_KEY:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 4;
                    int plantCount = (blockInfo.type == (int)BlockType.GROUND && blockInfo.subType == (int)GROUND_TYPE.KEY) ? blockInfo.count : 0;

                    if (plantCount != setNoColor)
                    {
                        blockInfo.InitBlock();
                        if (setNoColor == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.count = setNoColor;
                                blockInfo.type = (int)BlockType.GROUND;
                                blockInfo.subType = (int)GROUND_TYPE.KEY;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PLANT2X2:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.PLANT2X2) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.index = 0;
                            blockInfo.subType = 0;
                            blockInfo.count = 0;
                            blockInfo.subTarget = new int[] { 0, 0, 0, 0, 0 };
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PLANT2X2;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.index = EditManager.instance.blockIndex;
                            blockInfo.subType = EditManager.instance.plantType;
                            blockInfo.count = EditManager.instance.blockCount > 0 ? EditManager.instance.blockCount : 1;                            
                            for (int i = 0; i < 5; i++)
                            {
                                blockInfo.subTarget[i] = EditManager.instance.blockSubTarget[i];
                                if (EditManager.instance.blockSubTarget[i] != 0 && EditManager.instance.blockSubTargetBool[i])
                                    blockInfo.subTarget[i] += 10;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.ColorFlowerPot_Little:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 8;
                    int blockCount = (blockInfo.type == (int)BlockType.LITTLE_FLOWER_POT) ? (int)blockInfo.colorType : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.LITTLE_FLOWER_POT;
                            blockInfo.colorType = setBlock;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.COLOR_BIG_JEWEL:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.type == (int)BlockType.ColorBigJewel) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.index = 0;
                            blockInfo.subType = 0;
                            blockInfo.count = 0;
                            blockInfo.subTarget = new int[] { 0, 0, 0, 0, 0 };
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.ColorBigJewel;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.index = EditManager.instance.blockIndex;
                            blockInfo.subType = 0;
                            blockInfo.count = 5;

                            for (int i = 0; i < 5; i++)
                            {
                                blockInfo.subTarget[i] = EditManager.instance.blockSubTarget[i];
                                if (EditManager.instance.blockSubTargetBool[i])
                                    blockInfo.subTarget[i] += 10;
                            }
                            
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.SODAJELLY:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    const int pivotCount = 3;
                    int blockType = (int)BlockType.SODAJELLY;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 3;
                    int sodaJellyCount = (blockInfo.type == blockType) ? blockInfo.count : pivotCount;

                    if (sodaJellyCount != (pivotCount - setBlock)
                        || (blockInfo.type == blockType && blockInfo.index != EditManager.instance.blockIndex))
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {
                            blockInfo.type = blockType;
                            blockInfo.count = (pivotCount - setBlock);
                            blockInfo.index = EditManager.instance.blockIndex;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PEA:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int blockCount = (blockInfo.type == (int)BlockType.PEA) ? (int)blockInfo.count : 0;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 3;

                    if (blockCount != setBlock)
                    {
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PEA;
                            blockInfo.count = setBlock;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.PEA_BOSS:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int blockCount = (blockInfo.type == (int)BlockType.PEA_BOSS) ? (int)blockInfo.count : 0;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 3;

                    if (blockCount != setBlock)
                    {
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PEA_BOSS;
                            blockInfo.count = setBlock;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.WORLD_RANK_ITEM:
                {
                    int blockType = (int)BlockType.WORLD_RANK_ITEM;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;

                    if (blockType != setBlock)
                    {
                        if (setBlock == 0)
                        {
                            //월드랭킹 기믹의 제거 동작은 해당 기믹만 제거할 수 있음.
                            if (blockInfo.type == blockType)
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            //월드랭킹 기믹은 땅이 없고, 다른 블럭이 배치되지 않은 곳에만 배치 가능함.
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard == false
                                && (blockInfo.type == (int)BlockType.NONE || blockInfo.type == blockType))
                            {
                                blockInfo.type = blockType;
                                blockInfo.count = (int)EditManager.instance.blockIndex;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.FLOWER_INK:
                {
                    if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                    {
                        int setDeco = EditManager.instance.paintCount[(int)mode] % 6;

                        if (setDeco != 0)
                        {
                            bool hasCrack1 = false;
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.FLOWER_INK)
                                {
                                    deco.index = setDeco;
                                    hasCrack1 = true;
                                }
                            }

                            if (!hasCrack1)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.FLOWER_INK;
                                deco.index = setDeco;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.FLOWER_INK)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.SPACESHIP:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int blockType = (int)BlockType.SPACESHIP;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;

                    if (blockType != setBlock)
                    {
                        if (setBlock == 0)
                        {
                            if (blockInfo.type == blockType)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = blockType;
                            blockInfo.index = (int)EditManager.instance.blockIndex;

                            if ((int)EditManager.instance.blockCount <= 1)
                            {
                                blockInfo.count = 1;
                            }
                            else
                            {
                                blockInfo.count = (int)EditManager.instance.blockCount;
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.SPACESHIP_EXIT:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setDeco = EditManager.instance.paintCount[(int)mode] % 2;

                    if (setDeco != 0)
                    {
                        bool hasSpaceExit = false;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.SPACESHIP_EXIT)
                            {
                                deco.index = setDeco;
                                hasSpaceExit = true;
                            }
                        }

                        if (!hasSpaceExit)
                        {
                            foreach (var decoKey in blockInfo.ListDeco)
                            {
                                if (decoKey.BoardType == (int)BoardDecoType.KEY_EXIT)
                                {
                                    blockInfo.ListDeco.Remove(decoKey);
                                    break;
                                }
                            }

                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.SPACESHIP_EXIT;
                            deco.index = setDeco;
                            blockInfo.ListDeco.Add(deco);
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.SPACESHIP_EXIT)
                            {
                                blockInfo.ListDeco.Remove(deco);
                                break;
                            }
                        }
                    }
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }
                break;
            case EditMode.BOX:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 4;
                    int blockCount = (blockInfo.type == (int)BlockType.BOX) ? blockInfo.count : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.BOX;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.colorType = EditManager.instance.boxColorType;
                            blockInfo.count = setBlock;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            //
            case EditMode.FIRE_WORK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 8;
                    int blockCount = (blockInfo.type == (int)BlockType.FIRE_WORK) ? blockInfo.count : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.FIRE_WORK;
                            blockInfo.colorType = setBlock;
                            blockInfo.count = EditManager.instance.blockCount;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.STONE:
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 3;
                    int blockCount = (blockInfo.type == (int)BlockType.STONE ) ? blockInfo.count : 0;

                    //열쇠, 폭탄 stoneType, subType이 달라서 예외처리
                    int subType = EditManager.instance.stoneType;
                    if (EditManager.instance.stoneType == 3)
                    {
                        subType = (int)STONE_TYPE.KEY;
                    }
                    else if (EditManager.instance.stoneType == 2)
                    {
                        switch (EditManager.instance.blockType)
                        {
                            case 0:
                                subType = (int)STONE_TYPE.LINE;
                                break;
                            case 1:
                                subType = (int)STONE_TYPE.CIRCLE;
                                break;
                            case 2:
                                subType = (int)STONE_TYPE.LINE_V;
                                break;
                            case 3:
                                subType = (int)STONE_TYPE.LINE_H;
                                break;
                        }
                    }

                    if (blockCount != setBlock || subType != blockInfo.subType || (EditManager.instance.candyType && EditManager.instance.stoneType == (int)STONE_TYPE.NORMAL))
                    {
                        if (blockCount == setBlock)
                        { 
                            // 사탕 예외처리 (따로 토글로 떨어져 있어서)
                            if (EditManager.instance.candyType && EditManager.instance.stoneType == (int)STONE_TYPE.NORMAL && blockInfo.subType == 5)
                                break;
                            // 열쇠 예외처리 (stoneType이랑 subType 값이 다름)
                            else if (EditManager.instance.stoneType == 3 && blockInfo.subType == (int)STONE_TYPE.KEY)
                                break;
                            // 폭탄 예외처리
                            else if (EditManager.instance.stoneType == 2)
                            {
                                bool lineBombCantMake = EditManager.instance.blockType == 0 && blockInfo.subType == (int)STONE_TYPE.LINE;
                                bool circleBombCantMake = EditManager.instance.blockType == 1 && blockInfo.subType == (int)STONE_TYPE.CIRCLE;
                                bool lineVBombCantMake = EditManager.instance.blockType == 2 && blockInfo.subType == (int)STONE_TYPE.LINE_V;
                                bool lineHBombCantMake = EditManager.instance.blockType == 3 && blockInfo.subType == (int)STONE_TYPE.LINE_H;

                                if (lineBombCantMake || circleBombCantMake || lineVBombCantMake || lineHBombCantMake)
                                    break;
                            }
                        }

                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if(ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type      = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                            }
                            else
                            {
                                blockInfo.type      = (int)BlockType.NONE;
                                blockInfo.colorType = (int)BlockColorType.NONE;
                            }
                            
                            blockInfo.count   = 0;
                            blockInfo.index   = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.STONE;

                            if (EditManager.instance.stoneType == 0 && EditManager.instance.candyType) blockInfo.subType = (int)STONE_TYPE.CANDY;
                            else if (EditManager.instance.stoneType == 1) blockInfo.subType = (int)STONE_TYPE.APPLE;
                            else if (EditManager.instance.stoneType == 2 && EditManager.instance.blockType == 0) blockInfo.subType = (int)STONE_TYPE.LINE;
                            else if (EditManager.instance.stoneType == 2 && EditManager.instance.blockType == 1) blockInfo.subType = (int)STONE_TYPE.CIRCLE;
                            else if (EditManager.instance.stoneType == 2 && EditManager.instance.blockType == 2) blockInfo.subType = (int)STONE_TYPE.LINE_V;
                            else if (EditManager.instance.stoneType == 2 && EditManager.instance.blockType == 3) blockInfo.subType = (int)STONE_TYPE.LINE_H;
                            else if (EditManager.instance.stoneType == 3) blockInfo.subType = (int)STONE_TYPE.KEY;
                            else if (EditManager.instance.stoneType == 8) blockInfo.subType = (int)STONE_TYPE.WATER;
                            else if (EditManager.instance.stoneType == 9) blockInfo.subType = (int)STONE_TYPE.HealPotion;
                            else if (EditManager.instance.stoneType == 10) blockInfo.subType = (int)STONE_TYPE.SkillPotion;
                            else if (EditManager.instance.stoneType == 11) blockInfo.subType = (int)STONE_TYPE.SPACESHIP;
                            else blockInfo.subType = (int)STONE_TYPE.NORMAL;

                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = setBlock;
                            blockInfo.index = EditManager.instance.blockType;

                            if (blockInfo.subType == (int)STONE_TYPE.APPLE && blockInfo.index == 0)
                                blockInfo.index = 1;

                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.RAINBOW:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 7;
                    int blockCount = (blockInfo.bombType == (int)BlockBombType.RAINBOW) ? (int)blockInfo.colorType : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.bombType = (int)BlockBombType.NONE;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = setBlock;
                            blockInfo.bombType = (int)BlockBombType.RAINBOW;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.BOMB:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int blockCount = (blockInfo.bombType == (int)BlockBombType.BOMB) ? 1 : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.bombType = (int)BlockBombType.NONE;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.bombType = (int)BlockBombType.BOMB;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.LINE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 3;
                    int blockCount = (blockInfo.bombType == (int)BlockBombType.LINE_H || blockInfo.bombType == (int)BlockBombType.LINE_V) ? blockInfo.index : 0;

                    if (blockCount != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.bombType = (int)BlockBombType.NONE;
                            blockInfo.index = 0;
                            blockInfo.subType = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.bombType = (setBlock == 1) ? (int)BlockBombType.LINE_H : (int)BlockBombType.LINE_V;
                            blockInfo.index = setBlock;
                            blockInfo.subType = 0;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.EMPTY1:
                
                break;
            case EditMode.LINKSIDE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 5;
                    
                    if (setSide != 0)
                    {
                        EditBlockAndDecoInfo sideInfo = GetSideDeco(blockInfo, (BlockDirection)setSide);
                        bool hasLinkSide = HasSideDeco(mode, sideInfo);
                        if (hasLinkSide == false)
                        {
                            if (sideInfo != null) OverWrite_SideDeco(mode, sideInfo);
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.LINK_SIDE_BLOCK;
                            deco.index = setSide;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.LINK_SIDE_BLOCK)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }


                }
                break;
            case EditMode.SIDEBLOCK:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 5;

                    if (setSide != 0)
                    {
                        bool hasCrack1 = false;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.SIDE_BLOCK && deco.index == setSide)
                            {
                                hasCrack1 = true;
                            }
                        }

                        if (!hasCrack1)
                        {
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.SIDE_BLOCK;
                            deco.index = setSide;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.SIDE_BLOCK)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }


                }
                break;
                /*
            case EditMode.ARROW:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 5;

                    if (setSide != 0)
                    {
                        bool hasCrack1 = false;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.ARROW && deco.index == setSide)
                            {
                                hasCrack1 = true;
                            }
                        }

                        if (!hasCrack1)
                        {
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.ARROW;
                            deco.index = setSide;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.ARROW)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }
                }
                break;
                */
            case EditMode.ICE:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard && ManagerBlock.boards[inX, inY].Block != null)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 4;

                    if (setSide != 0)
                    {
                        bool hasCrack1 = false;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.ICE )
                            {
                                hasCrack1 = true;

                                if(deco.count != setSide)
                                {
                                    deco.count = setSide;

                                    reflesh = true;
                                    _spriteBody.alpha = 1;
                                }
                            }
                        }

                        if (!hasCrack1)
                        {
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.ICE;
                            deco.count = setSide;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.ICE)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }
                }
                break;
            case EditMode.MAP_DECO:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard == false)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 2 == 0 ? 0 : EditManager.instance.blockIndex;

                    if (setSide != 0)
                    {
                        int hasCrack1 = 0;
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.MAP_DECO)
                            {
                                hasCrack1 = deco.count;
                            }
                        }

                        if (hasCrack1 == 0)
                        {
                            DecoInfo deco = new DecoInfo();
                            deco.BoardType = (int)BoardDecoType.MAP_DECO;
                            deco.count = EditManager.instance.blockIndex;

                            blockInfo.ListDeco.Add(deco);

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                        else if (hasCrack1 != setSide)
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.MAP_DECO)
                                {
                                    deco.count = EditManager.instance.blockIndex;
                                    reflesh = true;
                                    _spriteBody.alpha = 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.MAP_DECO)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }
                }
                break;
            case EditMode.SAND_BELT:
                //if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setSide = EditManager.instance.paintCount[(int)mode] % 2;

                    if (setSide != 0)
                    {
                        if(EditManager.instance.listSand.Count == 0)
                        {
                            EditManager.instance.listSand.Add(ManagerBlock.boards[inX, inY]);
                        }
                        else if(EditManager.instance.listSand[EditManager.instance.listSand.Count-1] != ManagerBlock.boards[inX, inY])
                        {
                            EditManager.instance.listSand.Add(ManagerBlock.boards[inX, inY]);
                        }

                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                    else
                    {
                        foreach (var deco in blockInfo.ListDeco)
                        {
                            if (deco.BoardType == (int)BoardDecoType.SAND_BELT)
                            {
                                blockInfo.ListDeco.Remove(deco);

                                reflesh = true;
                                _spriteBody.alpha = 1;

                                break;
                            }
                        }
                    }
                }
                break;
            case EditMode.RANDOM_BOX:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setRandomBox = EditManager.instance.paintCount[(int)mode] % 3;
                    int randomBoxCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.RANDOM_BOX)
                        {
                            randomBoxCount = deco.count;
                            break;
                        }
                    }

                    if (setRandomBox != randomBoxCount)
                    {
                        if (setRandomBox != 0)
                        {
                            bool hasRandomBox = false;

                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.RANDOM_BOX)
                                {
                                    deco.count = setRandomBox;
                                    hasRandomBox = true;
                                }
                            }

                            if (!hasRandomBox)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.RANDOM_BOX;
                                deco.count = setRandomBox;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.RANDOM_BOX)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.HEART:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                    int heartIndex = EditManager.instance.blockIndex;
                    int wayCount = EditManager.instance.wayCount;

                    if (setBlock !=  0)
                    {
                        if (EditManager.instance.isHeartWayTap == false)
                        {
                            if((ManagerBlock.instance.GetHeartByHeartIndex(heartIndex) == null && blockInfo.type != (int)BlockType.HEART)
                                || (blockInfo.type == (int)BlockType.HEART && blockInfo.index == heartIndex
                                    && (blockInfo.colorType != (int)EditManager.instance.colorType)))
                            {
                                blockInfo.type = (int)BlockType.HEART;
                                blockInfo.index = heartIndex;
                                blockInfo.colorType = (int)EditManager.instance.colorType;

                                reflesh = true;
                                _spriteBody.alpha = 1;
                            }
                        }
                        else
                        {
                            BlockHeart tempHeart = ManagerBlock.instance.GetHeartByHeartIndex(heartIndex);
                            if (tempHeart != null
                                && ManagerBlock.instance.FindHeartWay_GimmickInfo(heartIndex, inX, inY) == false)
                            {
                                ManagerBlock.instance.AddHeartWay_GimmickInfo(heartIndex, wayCount, inX, inY);
                                EditManager.instance.wayCount++;

                                reflesh = true;
                                _spriteBody.alpha = 1;
                            }
                        }
                    }
                    else
                    {
                        if (blockInfo.type == (int)BlockType.HEART)
                        {
                            RemoveHeart(blockInfo.index);

                            //하트 기믹 제거
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.index = 0;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;

                            reflesh = true;
                            _spriteBody.alpha = 1;
                        }
                        else
                        {
                            //하트끝기믹 제거
                            ManagerBlock.instance.RemoveHeartHomeInfo(inX, inY);

                            //heartindex를  -1로 넘기면 인덱스와 상관없이 inX,inY 좌표값에 맞으면 전부 삭제 
                            if (ManagerBlock.instance.RemoveHeartWay_GimmickInfo(-1, inX, inY))
                            {
                                reflesh = true;
                                _spriteBody.alpha = 1;
                            }
                        }
                    }
                }
                break;

            case EditMode.PAINT:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int setNoColor = EditManager.instance.paintCount[(int)mode] % 8;
                    //현재 없는 컬러는 찍히지 않도록 수정
                    if (setNoColor == (int)BlockColorType.F) return; 

                    int bColor = (blockInfo.type == (int)BlockType.PAINT) ? blockInfo.colorType : 0;
                    if (bColor != setNoColor || blockInfo.count != (int)EditManager.instance.blockCount)
                    {
                        if (setNoColor == 0)
                        {
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {
                            blockInfo.type = (int)BlockType.PAINT;
                            blockInfo.colorType = setNoColor;
                            blockInfo.count = (int)EditManager.instance.blockCount;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;

            case EditMode.BREAD:
                {
                    int blockType = (int)BlockType.BREAD;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;

                    if (blockType != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = blockType;
                            blockInfo.index = EditManager.instance.blockIndex;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.WATERBOMB:
                if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                {
                    int blockType = (int)BlockType.WATERBOMB;
                    int setBlock = EditManager.instance.paintCount[(int)mode] % 2;

                    if (blockType != setBlock)
                    {
                        blockInfo.InitBlock();
                        if (setBlock == 0)
                        {
                            if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                            {
                                blockInfo.type = (int)BlockType.NORMAL;
                                blockInfo.colorType = (int)BlockColorType.RANDOM;
                                blockInfo.count = 0;
                                blockInfo.index = 0;
                            }
                            else
                            {
                                blockInfo.type = (int)BlockType.NONE;
                                blockInfo.colorType = 0;
                                blockInfo.count = 0;
                                blockInfo.subType = 0;
                            }
                        }
                        else
                        {
                            blockInfo.type = blockType;
                            blockInfo.index = EditManager.instance.waterBombTimeCount;
                            blockInfo.count = EditManager.instance.blockCount;
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.CLOVER:
                {
                    int setClover = EditManager.instance.paintCount[(int)mode] % 2;
                    int cloverCount = 0;

                    foreach (var deco in blockInfo.ListDeco)
                    {
                        if (deco.BoardType == (int)BoardDecoType.CLOVER)
                        {
                            cloverCount = deco.count;
                            break;
                        }
                    }

                    if (setClover != cloverCount)
                    {
                        if (setClover != 0)
                        {
                            ManagerBlock.boards[inX, inY].IsActiveBoard = true;
                            
                            bool hasClover = false;
        
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CLOVER)
                                {
                                    deco.count = setClover;
                                    hasClover = true;
                                }
                            }

                            if (!hasClover)
                            {
                                DecoInfo deco = new DecoInfo();
                                deco.BoardType = (int)BoardDecoType.CLOVER;
                                deco.count = setClover;
                                blockInfo.ListDeco.Add(deco);
                            }
                        }
                        else
                        {
                            foreach (var deco in blockInfo.ListDeco)
                            {
                                if (deco.BoardType == (int)BoardDecoType.CLOVER)
                                {
                                    blockInfo.ListDeco.Remove(deco);
                                    break;
                                }
                            }
                        }
                        reflesh = true;
                        _spriteBody.alpha = 1;
                    }
                }
                break;
            case EditMode.CANNON:
            {
                int blockType = (int)BlockType.CANNON;
                int setBlock = EditManager.instance.paintCount[(int)mode] % 2;
                
                if (blockType != setBlock)
                {
                    if (setBlock == 0)
                    {
                        if (ManagerBlock.boards[inX, inY] == null || ManagerBlock.boards[inX, inY].Block == null || ManagerBlock.boards[inX, inY].Block.type != BlockType.CANNON)
                            break;
                        blockInfo.InitBlock();
                        if (ManagerBlock.boards[inX, inY].IsActiveBoard)
                        {
                            blockInfo.type = (int) BlockType.NORMAL;
                            blockInfo.colorType = (int) BlockColorType.RANDOM;
                            blockInfo.count = 0;
                            blockInfo.index = 0;
                        }
                        else
                        {
                            blockInfo.type = (int) BlockType.NONE;
                            blockInfo.colorType = 0;
                            blockInfo.count = 0;
                            blockInfo.subType = 0;
                        }
                    }
                    else
                    {
                        blockInfo.InitBlock();
                        blockInfo.type = blockType;
                        blockInfo.colorType = (int)EditManager.instance.colorType;
                        blockInfo.subType = EditManager.instance.blockIndex;
                        blockInfo.count = EditManager.instance.blockCount;
                    }
                    reflesh = true;
                    _spriteBody.alpha = 1;
                }
            }
            break;
        }

        if (IsBombTypeBlock((BlockType)blockInfo.type, (BlockBombType)blockInfo.bombType) == false)
        {   //폭탄 타입을 사용하지 않는 블럭에 폭탄 데이터가 들어가 있다면 초기화 시켜줌
            blockInfo.bombType = (int)BlockBombType.NONE;
        }
    }
    
    private bool IsBombTypeBlock(BlockType blockType, BlockBombType bombType = BlockBombType.NONE)
    {
        if (blockType == BlockType.NORMAL && bombType != BlockBombType.NONE)
            return true;
        else if (blockType == BlockType.START_Bomb
                 || blockType == BlockType.START_Line
                 || blockType == BlockType.START_Rainbow)
            return true;
        else
            return false;
    }

    public bool IsBlockTypeByEditMode(EditMode mode)
    {
        switch (mode)
        {
        case EditMode.APPLE:
        case EditMode.BLOCK:
        case EditMode.BLOCKTYPE:
        case EditMode.CANDY:
        case EditMode.KEY:
        case EditMode.PLANT:
        case EditMode.PLANT_APPLE:
        case EditMode.PLANT_KEY:
        case EditMode.GROUND:
        case EditMode.LINE:
        case EditMode.BOMB:
        case EditMode.RAINBOW:
        case EditMode.JEWEL:
        case EditMode.PLANT2X2:
        case EditMode.STONE:
        case EditMode.BOX:
        case EditMode.ICE_APPLE:
        case EditMode.BLOCK_BLACK:
        case EditMode.ICE_PLANT:
        case EditMode.BOMB_PLANT:
        case EditMode.GROUND_APPLE:
        case EditMode.GROUND_BOMB:
        case EditMode.DUCK:
        case EditMode.GROUND_KEY:
        case EditMode.GROUND_ICE_APPLE:
        case EditMode.Event_Block:
        case EditMode.DYNAMITE:
        case EditMode.COIN:
        case EditMode.ADVENTURE_POTION_HEAL:
        case EditMode.ADVENTURE_POTION_SKILL:
        case EditMode.Event_Stone:
        case EditMode.ColorFlowerPot_Little:
        case EditMode.COLOR_BIG_JEWEL:
        case EditMode.FIRE_WORK:
        case EditMode.COIN_BAG:
        case EditMode.PLANT_COIN:
        case EditMode.SODAJELLY:
        case EditMode.PEA:
        case EditMode.SPACESHIP:
        case EditMode.PEA_BOSS:
        case EditMode.HEART:
        case EditMode.BREAD:
        case EditMode.WATERBOMB:
        case EditMode.CANNON:
            return true;
        }

        return false;
    }

    private BlockType ConvertEditModeToBlockType(EditMode mode)
    {
        //사용할 때 추가
        switch (mode)
        {
            case EditMode.HEART: return BlockType.HEART;
        }

        return BlockType.NONE;
    }

    private void BlockOverwrite(EditMode mode, BlockInfo info)
    {
        //덮어쓰기 설치로 인한 기존 기믹 제거

        if(IsBlockTypeByEditMode(mode)) //데코인 경우 제외
        {
            if (ConvertEditModeToBlockType(mode) != (BlockType)info.type)
            {
                switch((BlockType)info.type)
                {
                    case BlockType.HEART:
                        RemoveHeart(info.index);
                        break;
                }
            }
        }
    }

    private void RemoveHeart(int heartIndex)
    {
        //하트 끝 기믹 제거
        BlockHeart tempHeart = ManagerBlock.instance.GetHeartByHeartIndex(heartIndex);
        if (tempHeart != null
            && ManagerBlock.instance.FindHeartWay_GimmickInfo(heartIndex, inX, inY) == false)
        {
            tempHeart.RemoveHeartHome(true);
        }

        //하트 길 데코 제거
        ManagerBlock.instance.RemoveHeartWay_GimmickInfo(heartIndex);
    }

    private EditBlockAndDecoInfo GetSideDeco(BlockInfo editInfo, BlockDirection dir)
    {
        EditBlockAndDecoInfo returnInfo = new EditBlockAndDecoInfo();

        //같은 방향으로 설치된 경우 체크
        foreach (var deco in editInfo.ListDeco)
        {
            if (deco.BoardType == (int)BoardDecoType.LINK_SIDE_BLOCK
                || deco.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK
                || deco.BoardType == (int)BoardDecoType.FENCEBLOCK)
            {
                if (dir == (BlockDirection)deco.index)
                {
                    returnInfo.blockInfo = editInfo;
                    returnInfo.decoInfo = deco;
                    return returnInfo;
                }
            }
        }

        //반대 방향이지만 같은 위치인 경우 체크
        int xOffset = 0;
        int yOffset = 0;
        BlockDirection checkDir = BlockDirection.NONE;

        switch (dir)
        {
            case BlockDirection.UP:
                checkDir = BlockDirection.DOWN;
                yOffset = -1;
                break;

            case BlockDirection.DOWN:
                checkDir = BlockDirection.UP;
                yOffset = 1;
                break;

            case BlockDirection.LEFT:
                checkDir = BlockDirection.RIGHT;
                xOffset = -1;
                break;

            case BlockDirection.RIGHT:
                checkDir = BlockDirection.LEFT;
                xOffset = 1;
                break;
        }

        int tempX = inX + xOffset;
        int tempY = inY + yOffset;

        if (tempX >= 0 && tempX < GameManager.MAX_X && tempY >= 0 && tempY < GameManager.MAX_X
            && ManagerBlock.boards[tempX, tempY].IsActiveBoard)
        {
            BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(tempX, tempY);
            if (blockInfo == null)
                return null;

            foreach (var deco in blockInfo.ListDeco)
            {
                if (deco.BoardType == (int)BoardDecoType.LINK_SIDE_BLOCK
                    || deco.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK
                    || deco.BoardType == (int)BoardDecoType.FENCEBLOCK)
                {
                    if (checkDir == (BlockDirection)deco.index)
                    {
                        returnInfo.blockInfo = blockInfo;
                        returnInfo.decoInfo = deco;
                        return returnInfo;
                    }
                }
            }
        }

        return null;
    }

    private bool HasSideDeco(EditMode editType, EditBlockAndDecoInfo sideInfo)
    {
        if (sideInfo == null)
            return false;

        //같은 데코일 때만 true 리턴
        if ((editType == EditMode.LINKSIDE && sideInfo.decoInfo.BoardType == (int)BoardDecoType.LINK_SIDE_BLOCK)
            || (editType == EditMode.GRASSFENCEBLOCK && sideInfo.decoInfo.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK && sideInfo.decoInfo.count == (int)EditManager.instance.fenceCount)
            || (editType == EditMode.FENCEBLOCK && sideInfo.decoInfo.BoardType == (int)BoardDecoType.FENCEBLOCK))
        {
            return true;
        }

        return false;
    }

    //방해블럭, 울타리류 등 덮어쓰기
    private void OverWrite_SideDeco(EditMode editType, EditBlockAndDecoInfo sideInfo)
    {
        //덮어쓰기 전 제거
        sideInfo.blockInfo.ListDeco.Remove(sideInfo.decoInfo);
    }
}
