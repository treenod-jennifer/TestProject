using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoardDecoMaker : MonoSingletonOnlyScene<BoardDecoMaker>
{
    public GameObject startPrefab;
    public GameObject potalPrefab;
    public GameObject crackPrefab;
    public GameObject NetPrefab;
    public GameObject LinkSideBlockPrefab;
    public GameObject KeyExitPrefab;
    public GameObject SideBlockPrefab;
    public GameObject GrassPrefab;
    public GameObject StatuePrefab;
    public GameObject arrowPrefab;
    public GameObject MapDecoPrefab;
    public GameObject SandBeltPrefab;
    public GameObject SandPotalPrefab;
    public GameObject LavaPrefab;
    public GameObject WaterPrefab;
 	public GameObject FenceBlockPrefab;
    public GameObject GrassFenceBlockPrefab;
    public GameObject CarpetPrefab;
    public GameObject CountCrackPrefab;
    public GameObject FlowerInkPrefab;
    public GameObject SpaceShipExitPrefab;
    public GameObject GeneratorPrefab;
    public GameObject RandomBoxPrefab;
    public GameObject HeartWayPrefab;
    public GameObject CloverPrefab;

    public DecoBase MakeBoardDeco(Board board, int indexX, int indexY, DecoInfo decoinfo)
    {
        if(decoinfo.BoardType == (int)BoardDecoType.START)
        {
            return MakeBlockStart(board, indexX, indexY, decoinfo.index, decoinfo.count, decoinfo.type);
        }
        else if(decoinfo.BoardType == (int)BoardDecoType.POTAL_IN)
        {
            return MakePotal(board, indexX, indexY, decoinfo.index, POTAL_TYPE.IN);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.POTAL_OUT)
        {
            return MakePotal(board, indexX, indexY, decoinfo.index, POTAL_TYPE.OUT);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.CARCK1)
        {
            return MakeCrack(board, indexX, indexY, 1, decoinfo.count);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.NET)
        {
            return MakeNet(board, indexX, indexY, decoinfo.count);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.LINK_SIDE_BLOCK)
        {
            return MakeLinkSide(board, indexX, indexY, decoinfo.index);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.SIDE_BLOCK)
        {
            return MakeSideBlock(board, indexX, indexY, decoinfo.index);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.ARROW)
        {
            return MakeArrow(board, indexX, indexY, decoinfo.index);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.KEY_EXIT)
        {
            return MakeKeyExit(board, indexX, indexY);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.GRASS)
        {
            return MakeGrass(board, indexX, indexY, decoinfo.count);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.STATUE)
        {
            return MakeStatue(board, indexX, indexY, decoinfo.index, decoinfo.count, decoinfo.type);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.MAP_DECO)
        {
            return MakeMapDeco(board, indexX, indexY, decoinfo.count);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.SAND_BELT)
        {
            return MakeSandBelt(board, indexX, indexY, decoinfo.count , decoinfo.index, decoinfo.type);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.LAVA)
        {
            return MakeLava(board, indexX, indexY);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.WATER)
        {
            return MakeWater(board, indexX, indexY, decoinfo.index);
        }
 		else if (decoinfo.BoardType == (int)BoardDecoType.FENCEBLOCK)
        {
            return MakeFenceBlock(board, indexX, indexY, decoinfo.index, decoinfo.count);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK)
        {
            return MakeGrassFenceBlock(board, indexX, indexY, decoinfo.index, decoinfo.count, decoinfo.type);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.CARPET)
        {
            return MakeCarpet(board, indexX, indexY);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.COUNT_CRACK)
        {
            return MakeCountCrack(board, indexX, indexY, decoinfo.index);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.FLOWER_INK)
        {
            return MakeFlowerInk(board, indexX, indexY, decoinfo.index);
        }
        else if(decoinfo.BoardType == (int)BoardDecoType.SPACESHIP_EXIT)
        {
            return MakeSpaceShipExit(board, indexX, indexY);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.BLOCK_GENERATOR)
        {
            return MakeGenerator(board, indexX, indexY, decoinfo);
        }
        else if (decoinfo.BoardType == (int)BoardDecoType.RANDOM_BOX)
        {
            return MakeRandomBox(board, indexX, indexY, decoinfo.count);
        }
        else if(decoinfo.BoardType == (int)BoardDecoType.CLOVER)
        {
            return MakeClover(board, indexX, indexY, decoinfo.count);
        }

        return null;
    }

    public Water MakeWater(Board board, int indexX, int indexY, int direction)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, WaterPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Water _water = obj.GetComponent<Water>();
        _water.inX = indexX;
        _water.inY = indexY;
        _water.board = board;
        _water.SetDir(direction);

        CheckDownBlockOrDeco(board);

        board.AddDeco(_water);

        ManagerBlock.instance.listWater.Add(_water);
        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(_water);

        return _water;
    }

    public Lava MakeLava(Board board, int indexX, int indexY)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, LavaPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Lava _lava = obj.GetComponent<Lava>();
        _lava.inX = indexX;
        _lava.inY = indexY;

        _lava.board = board;
        board.AddDeco(_lava);
        board.lava = _lava;

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.listLava.Add(_lava);
        DecoBase._listBoardDeco.Add(_lava);

        return _lava;
    }


    public SandBelt MakeSandBelt(Board board, int indexX, int indexY, int count, int index, int outBoard)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, SandBeltPrefab);
        SandBelt sandBelt = obj.GetComponent<SandBelt>();
        sandBelt.inX = indexX;
        sandBelt.inY = indexY;
        sandBelt.direction = (BlockDirection)count;
        sandBelt.board = board;
        sandBelt.sandIndex = index;

        if(outBoard != 0)
        {
            int tempX = outBoard % GameManager.mMAX_X;
            int tempY = outBoard / GameManager.mMAX_X;
            sandBelt.exitBoard = PosHelper.GetBoard(tempX, tempY);
        }

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);// + new Vector3(1,-1,0)* ManagerBlock.BLOCK_SIZE *0.5f;
        board.AddDeco(sandBelt);

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.listSandBelt.Add(sandBelt);
        DecoBase._listBoardDeco.Add(sandBelt);
        return sandBelt;
    }

    public MapDeco MakeMapDeco(Board board, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, MapDecoPrefab);
        MapDeco mapDeco = obj.GetComponent<MapDeco>();
        mapDeco.inX = indexX;
        mapDeco.inY = indexY;
        mapDeco.lifeCount = count;
        mapDeco.board = board;
        mapDeco.Init();

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);// + new Vector3(1,-1,0)* ManagerBlock.BLOCK_SIZE *0.5f;
        board.AddDeco(mapDeco);

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.listMapDeco.Add(mapDeco);
        DecoBase._listBoardDeco.Add(mapDeco);
        return mapDeco;
    }

    public Blockstart MakeBlockStart(Board board, int indexX, int indexY, int tempStartBlockType, int tempStartBlockColor, int tempStartBlockType2)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, startPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX,indexY);
        Blockstart blockStart = obj.GetComponent<Blockstart>();
        blockStart.inX = indexX;
        blockStart.inY = indexY;
        blockStart.Init();

        board.AddDeco(blockStart);
        board.IsStartBoard = true;
        board.startBlockType = tempStartBlockType;
        board.startBlockType2 = tempStartBlockType2;

        if (tempStartBlockColor < 2)
            tempStartBlockColor = int.MaxValue;

        // 출발에서 나오지 않을 컬러 정보 계산해서 넣어줌.
        GetStartColorList(tempStartBlockColor, null, out board.dicIgnoreColor);

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.startBoardList.Add(board);

        return blockStart;
    }

    public Potal MakePotal(Board board, int indexX, int indexY, int index, POTAL_TYPE potalType)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, potalPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Potal blockPotal = obj.GetComponent<Potal>();
        blockPotal.inX = indexX;
        blockPotal.inY = indexY;
        blockPotal.index = index;
        blockPotal.type = potalType;
        blockPotal.board = board;

        board.AddDeco(blockPotal);
        ManagerBlock.instance.listPotal.Add(blockPotal);
        ManagerBlock.instance.listObject.Add(obj);
        return blockPotal;
    }

    public Crack MakeCrack(Board board, int indexX, int indexY, int index, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, crackPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Crack crack = obj.GetComponent<Crack>();
        crack.inX = indexX;
        crack.inY = indexY;
        crack.crackIndex = index;
        crack.lifeCount = count;
        crack.board = board;

        if (CheckUpBlockOrDeco(board) == true)
        {
            MakeLabel(obj, "C", Vector3.zero);
        }
		CheckDownBlockOrDeco(board);
        board.AddDeco(crack);

        ManagerBlock.instance.listCrack.Add(crack);
        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(crack);
        return crack;
    }

    public Net MakeNet(Board board, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, NetPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Net net = obj.GetComponent<Net>();
        net.inX = indexX;
        net.inY = indexY;
        net.lifeCount = count;
        net.board = board;

        board.AddDeco(net);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(net);
        return net;
    }

    public LinkSideBlock MakeLinkSide(Board board, int indexX, int indexY, int index)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, LinkSideBlockPrefab);
        LinkSideBlock side = obj.GetComponent<LinkSideBlock>();
        side.inX = indexX;
        side.inY = indexY;
        side.board = board;
        side.direction = (BlockDirection)index;

        side.boardB = PosHelper.GetBoardByDir(indexX,indexY, (BlockDirection)index);
        side.boardB.AddDeco(side);

        obj.transform.localPosition = (PosHelper.GetPosByIndex(indexX, indexY) + PosHelper.GetPosByIndex(side.boardB.indexX, side.boardB.indexY)) *0.5f;

        board.AddDeco(side);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(side);
        return side;
    }

	public FenceBlock MakeFenceBlock(Board board, int indexX, int indexY, int index, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, FenceBlockPrefab);
        FenceBlock fence = obj.GetComponent<FenceBlock>();
        fence.inX = indexX;
        fence.inY = indexY;
        fence.board = board;
        if (index == (int)BlockDirection.NONE)
        {
            index = (int)BlockDirection.DOWN;
            ShowFalseData(obj);
        }
        fence.direction = (BlockDirection)index;
        fence.lifeCount = 1;

        fence.boardB = PosHelper.GetBoardByDir(indexX, indexY, (BlockDirection)index);
        fence.boardB.AddDeco(fence);

        obj.transform.localPosition = (PosHelper.GetPosByIndex(indexX, indexY) + PosHelper.GetPosByIndex(fence.boardB.indexX, fence.boardB.indexY)) * 0.5f;

        board.AddDeco(fence);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(fence);
        return fence;
    }

    public GrassFenceBlock MakeGrassFenceBlock(Board board, int indexX, int indexY, int index, int count, int fIndex)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, GrassFenceBlockPrefab);
        GrassFenceBlock fence = obj.GetComponent<GrassFenceBlock>();
        fence.inX = indexX;
        fence.inY = indexY;
        fence.board = board;
        fence.direction = (BlockDirection)index;
        fence.lifeCount = count;
        fence.fenceIndex = fIndex;

        fence.boardB = PosHelper.GetBoardByDir(indexX, indexY, (BlockDirection)index);
        fence.boardB.AddDeco(fence);

        obj.transform.localPosition = (PosHelper.GetPosByIndex(indexX, indexY) + PosHelper.GetPosByIndex(fence.boardB.indexX, fence.boardB.indexY)) * 0.5f;

        board.AddDeco(fence);
        ManagerBlock.instance.listObject.Add(obj);

        DecoBase._listBoardDeco.Add(fence);
        return fence;
    }

    public SideBlock MakeSideBlock(Board board, int indexX, int indexY, int index)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, SideBlockPrefab);
        SideBlock side = obj.GetComponent<SideBlock>();
        side.inX = indexX;
        side.inY = indexY;
        side.board = board;
        side.direction = (BlockDirection)index;

        side.boardB = PosHelper.GetBoardByDir(indexX, indexY, (BlockDirection)index);
        side.boardB.AddDeco(side);

        obj.transform.localPosition = (PosHelper.GetPosByIndex(indexX, indexY) + PosHelper.GetPosByIndex(side.boardB.indexX, side.boardB.indexY)) * 0.5f;

        board.AddDeco(side);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(side);
        return side;
    }

    public Arrow MakeArrow(Board board, int indexX, int indexY, int index)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, arrowPrefab);
        Arrow arrow = obj.GetComponent<Arrow>();
        arrow.inX = indexX;
        arrow.inY = indexY;
        arrow.board = board;
        arrow.direction = (BlockDirection)index;

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        obj.transform.localEulerAngles = new Vector3(0,0, -90 * (index -1));

        if(PosHelper.GetBoardByDir(indexX,indexY, (BlockDirection)index) == null || PosHelper.GetBoardByDir(indexX, indexY, (BlockDirection)index).IsActiveBoard == false)
        {
          //  arrow.uiSprite.alpha = 0;
        }


        board.AddDeco(arrow);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(arrow);
        return arrow;
    }
    
    
    public KeyExit MakeKeyExit(Board board, int indexX, int indexY)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, KeyExitPrefab);
        KeyExit key = obj.GetComponent<KeyExit>();
        key.inX = indexX;
        key.inY = indexY;
        key.board = board;

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        board.AddDeco(key);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(key);
        return key;
    }

    public Grass MakeGrass(Board board, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, GrassPrefab);
        Grass grass = obj.GetComponent<Grass>();
        grass.inX = indexX;
        grass.inY = indexY;
        grass.lifeCount = count;
        grass.board = board;

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        if (CheckUpBlockOrDeco(board) == true)
        {
            MakeLabel(obj, "G", new Vector3(25f, 0f, 0f));
        }
        CheckDownBlockOrDeco(board);
        board.AddDeco(grass);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(grass);

        return grass;
    }


    public StoneStatue MakeStatue(Board board, int indexX, int indexY, int index, int count, int type)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, StatuePrefab);
        StoneStatue statue = obj.GetComponent<StoneStatue>();
        statue.inX = indexX;
        statue.inY = indexY;
        statue.index = index;
        statue.rotateDirection = count;
        statue.type = type;

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        ManagerBlock.instance.listStatues.Add(statue);
        statue.listBoards.Add(board);
        board.AddDeco(statue);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(statue);

        return statue;
    }

    public Carpet MakeCarpet(Board board, int indexX, int indexY)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CarpetPrefab);
        Carpet carpet = obj.GetComponent<Carpet>();
        carpet.inX = indexX;
        carpet.inY = indexY;
        carpet.board = board;
        carpet.CheckBoard(false);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        if (CheckUpBlockOrDeco(board) == true)
        {
            MakeLabel(obj, "C.p", Vector3.zero);
        }
        CheckDownBlockOrDeco(board);
        board.AddDeco(carpet);

        ManagerBlock.instance.listObject.Add(obj);

        DecoBase._listBoardDeco.Add(carpet);
        return carpet;
    }

    public Crack MakeCountCrack(Board board, int indexX, int indexY, int index)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CountCrackPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        CountCrack crack = obj.GetComponent<CountCrack>();
        crack.InitCountCrack(indexX, indexY, board, index);

        //단계석판 같은 경우, 활성화 될 때만 AddDeco, listCrack.Add가 불림.
        ManagerBlock.instance.listObject.Add(obj);
        if(GameManager.instance.state == GameState.EDIT)
            board.AddDeco(crack);

        CheckDownBlockOrDeco(board);

        return crack;
    }

    public FlowerInk MakeFlowerInk(Board board, int indexX, int indexY, int index)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, FlowerInkPrefab);
        FlowerInk makeDeco = obj.GetComponent<FlowerInk>();
        makeDeco.inX = indexX;
        makeDeco.inY = indexY;
        makeDeco.board = board;
        makeDeco.colorType = (BlockColorType)index;
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        if (CheckUpBlockOrDeco(board) == true)
        {
            MakeLabel(obj, "A.D", Vector3.zero);
        }
        board.AddDeco(makeDeco);

        ManagerBlock.instance.listFlowerInk.Add(makeDeco);
        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(makeDeco);
        return makeDeco;
    }

    public SpaceShipExit MakeSpaceShipExit(Board board, int indexX, int indexY)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, SpaceShipExitPrefab);
        SpaceShipExit key = obj.GetComponent<SpaceShipExit>();
        key.inX = indexX;
        key.inY = indexY;
        key.board = board;
        key.Init();

        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        board.AddDeco(key);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(key);


        return key;
    }

    public BlockGenerator MakeGenerator(Board board, int indexX, int indexY, DecoInfo decoInfo)
    {
        int generatorIndex = decoInfo.tempData_1;

        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, GeneratorPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        BlockGenerator blockGenerator = obj.GetComponent<BlockGenerator>();
        blockGenerator.inX = indexX;
        blockGenerator.inY = indexY;
        blockGenerator.board = board;
        blockGenerator.Init(generatorIndex);

        if (GameManager.instance.state == GameState.EDIT)
        {
            MakeLabel(obj, generatorIndex.ToString(), new Vector3(0f, 58f, 0f));
        }

        board.AddDeco(blockGenerator);
        board.IsStartBoard = true;
        board.startBlockType = decoInfo.index;
        board.startBlockType2 = decoInfo.type;

        if (board.startBlockType2< 2)
            board.startBlockType2 = int.MaxValue;

        // 출발에서 나오지 않을 컬러 정보 계산해서 넣어줌.
        GetStartColorList(decoInfo.count, decoInfo.listBlockColorData, out board.dicIgnoreColor);

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.startBoardList.Add(board);

        if (ManagerBlock.instance.dicBlockGenerator.ContainsKey(generatorIndex) == false)
        {
            ManagerBlock.instance.dicBlockGenerator.Add(generatorIndex, new List<BlockGenerator>());
        }

        ManagerBlock.instance.dicBlockGenerator[generatorIndex].Add(blockGenerator);

        return blockGenerator;
    }

    public RandomBox MakeRandomBox(Board board, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, RandomBoxPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        RandomBox blockRandomBox = obj.GetComponent<RandomBox>();
        blockRandomBox.inX = indexX;
        blockRandomBox.inY = indexY;
        blockRandomBox.board = board;
        blockRandomBox.lifeCount = count;

        blockRandomBox.boardDecoOrder = BoardDecoOrder.RANDOM_BOX;
        board.AddDeco(blockRandomBox);

        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(blockRandomBox);

        return blockRandomBox;
    }

    public Clover MakeClover(Board board, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CloverPrefab);
        obj.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        Clover clover = obj.GetComponent<Clover>();
        clover.inX = indexX;
        clover.inY = indexY;
        clover.lifeCount = count;
        clover.board = board;
        
        clover.boardDecoOrder = BoardDecoOrder.CLOVER;
        board.AddDeco(clover);
        ManagerBlock.instance.listClover.Add(clover);

        if(clover.board.Block != null)
            clover.board.Block.HideDecoRemoveAction();
        
        ManagerBlock.instance.listObject.Add(obj);
        DecoBase._listBoardDeco.Add(clover);

        return clover;
    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////////

    public GameObject icePrefab;

    public BlockDeco MakeBlockDeco(BlockBase baseBlock, int indexX, int indexY, DecoInfo decoinfo)
    {
        if (decoinfo.BoardType == (int)BoardDecoType.ICE)
        {
            return MakeIce(baseBlock, indexX, indexY, decoinfo.count);
        }
        return null;
    }

    public DecoIce MakeIce(BlockBase baseBlock, int indexX, int indexY, int count)
    {
        GameObject obj = NGUITools.AddChild(baseBlock.mainSprite.gameObject, icePrefab);
        DecoIce decoIce = obj.GetComponent<DecoIce>();
        decoIce.inX = indexX;
        decoIce.inY = indexY;
        decoIce.lifeCount = count;
        decoIce.parentBlock = baseBlock;

        obj.transform.localPosition = Vector3.zero;//PosHelper.GetPosByIndex(indexX, indexY);

        Board board = ManagerBlock.boards[indexX, indexY];
        CheckDownBlockOrDeco(board);

        baseBlock.blockDeco = decoIce;

        ManagerBlock.instance.listObject.Add(obj);
        BlockDeco._listBlockDeco.Add(decoIce);

        // 고정 얼음을 화면수에 포함시키지 않는 설정이 켜져있으면, 고정얼음 수는 화면수에 포함하지 않음.
        if (ManagerBlock.instance.stageInfo.isFixedIceAdjust == 0 || (ManagerBlock.instance.stageInfo.isFixedIceAdjust == 1 && decoIce.lifeCount <= 1))
        {
            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ICE]++;
        }
        ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ICE]++;

        return decoIce;
    }

    //현재 보드 위에 식물, 돌, 물, 흙 등 블럭을 덮는 데코나 블럭이 있는지 검사하는 함수.
    private bool CheckUpBlockOrDeco(Board board)
    {
        if (GameManager.instance.state != GameState.EDIT)
            return false;

        if (board == null)
            return false;

        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            if (board.DecoOnBoard[i] as Water != null || board.DecoOnBoard[i] as Grass != null
                || board.DecoOnBoard[i] as Crack != null || board.DecoOnBoard[i] as Carpet != null)
            {
                return true;
            }
        }

		if (board.Block != null &&
            (board.Block.blockDeco != null || board.Block.type == BlockType.STONE || board.Block.type == BlockType.GROUND || board.Block.type == BlockType.LITTLE_FLOWER_POT
            || board.Block.type == BlockType.GROUND_JEWEL || board.Block.type == BlockType.GROUND_KEY || board.Block.type == BlockType.GROUND_BOMB
            || board.Block.type == BlockType.GROUND_APPLE || board.Block.type == BlockType.GROUND_ICE_APPLE || board.Block.type == BlockType.PLANT_ICE_APPLE
            || board.Block.type == BlockType.ColorBigJewel || board.Block.type == BlockType.BREAD
            || board.Block.type == BlockType.PLANT2X2 || board.Block.type == BlockType.ICE || board.Block.type == BlockType.PLANT
            || board.Block.type == BlockType.SODAJELLY || board.Block.type == BlockType.BLOCK_EVENT || board.Block.type == BlockType.BLOCK_EVENT_STONE ))
        {
            return true;
        }
        return false;
    }

    //현재 만들어진 블럭 아래에 석판, 석상, 잔디 등이 있는지 검사하는 함수.
    private void CheckDownBlockOrDeco(Board board)
    {
        if (GameManager.instance.state != GameState.EDIT)
            return;

		if (board == null)
            return;

        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            Crack crack = board.DecoOnBoard[i] as Crack;
            Grass grass = board.DecoOnBoard[i] as Grass;
            Carpet carpet = board.DecoOnBoard[i] as Carpet;
            FlowerInk flowerInk = board.DecoOnBoard[i] as FlowerInk;
            if (crack as Crack || grass != null || carpet != null || flowerInk != null)
            {
                if (board.DecoOnBoard[i].GetComponentInChildren<UILabel>() == null)
                {
                    Vector3 tr = Vector3.zero;
                    string text = "";
                    if (crack != null)
                    {
                        if (crack as CountCrack)
                        {
                            CountCrack countCrack = crack as CountCrack;
                            SetCountCrackText(countCrack, board.DecoOnBoard[i].gameObject);
                            continue;
                        }
                        else
                        {
                            text = "C";
                        }
                    }
                    if (grass != null)
                    {
                        text = "G";
                        tr = new Vector3(25f, 0f, 0f);
                    }
                    if (carpet != null)
                    {
                        text = "C.p";
                    }
                    if(flowerInk != null)
                    {
                        text = "A.D";
                    }
                    MakeLabel(board.DecoOnBoard[i].gameObject, text, tr);
                }
            }
        }
    }

    private void SetCountCrackText(CountCrack countCrack, GameObject obj)
    {
        Vector3 tr = Vector3.zero;
        Color color = Color.white;
        Color effectColor = Color.white;
        switch (countCrack.crackIndex)
        {
            case 1:
                color = new Color(145f / 255f, 1f, 1f);
                effectColor = new Color(31f / 255f, 57f / 255f, 128f / 255f);
                tr = new Vector3(-27.7f, -2f, 0f);
                break;
            case 2:
                color = new Color(213f / 255f, 170f / 255f, 1f);
                effectColor = new Color(123f / 255f, 0f, 97f / 255f);
                tr = new Vector3(-12.2f, -2f, 0f);
                break;
            case 3:
                color = new Color(1f, 117f / 255f, 121f / 255f);
                effectColor = new Color(137f / 255f, 0f, 22f / 255f);
                tr = new Vector3(5.3f, -2f, 0f);
                break;
            case 4:
                color = new Color(1f, 150f / 255f, 90f / 255f);
                effectColor = new Color(113f / 255f, 0f, 7f / 255f);
                tr = new Vector3(23.8f, -2f, 0f);
                break;
        }
        MakeLabel(obj, countCrack.crackIndex.ToString(), tr, color, effectColor, 25);
    }

    private void MakeLabel(GameObject obj, string text, Vector3 tr)
    {
        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = new Color(124f / 255f, 40f / 255f, 40f / 255f);
        uiLabel.color = new Color(1f, 208f / 255f, 56f / 255f);
        uiLabel.text = text;
        uiLabel.transform.localPosition = tr;
    }

    private void MakeLabel(GameObject obj, string text, Vector3 tr, Color color, Color effectColor, int fontSize)
    {
        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = effectColor;
        uiLabel.color = color;
        uiLabel.text = text;
        uiLabel.transform.localPosition = tr;
        uiLabel.fontSize = fontSize;
    }

    private void GetStartColorList(int startBlockColorType, List<BlockAndColorData> listBlockColorData, out Dictionary<BlockType, List<BlockColorType>> dicIgnoreColors)
    {
        dicIgnoreColors = new Dictionary<BlockType, List<BlockColorType>>();
        
        //일반 블럭 등장 컬러 설정
        List<BlockColorType> listNormalColor = new List<BlockColorType>();
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.A))) == 0)
            listNormalColor.Add(BlockColorType.A);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.B))) == 0)
            listNormalColor.Add(BlockColorType.B);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.C))) == 0)
            listNormalColor.Add(BlockColorType.C);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.D))) == 0)
            listNormalColor.Add(BlockColorType.D);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.E))) == 0)
            listNormalColor.Add(BlockColorType.E);
        dicIgnoreColors.Add(BlockType.NORMAL, listNormalColor);

        //기타 블럭 등장 컬러 설정
        if (listBlockColorData != null)
        {
            for (int i = 0; i < listBlockColorData.Count; i++)
            {
                List<BlockColorType> listTempColor = new List<BlockColorType>();
                int color = listBlockColorData[i].subType;
                if ((color & ((int)(1 << (int)BlockColorType.A))) == 0)
                    listTempColor.Add(BlockColorType.A);
                if ((color & ((int)(1 << (int)BlockColorType.B))) == 0)
                    listTempColor.Add(BlockColorType.B);
                if ((color & ((int)(1 << (int)BlockColorType.C))) == 0)
                    listTempColor.Add(BlockColorType.C);
                if ((color & ((int)(1 << (int)BlockColorType.D))) == 0)
                    listTempColor.Add(BlockColorType.D);
                if ((color & ((int)(1 << (int)BlockColorType.E))) == 0)
                    listTempColor.Add(BlockColorType.E);

                dicIgnoreColors.Add((BlockType)listBlockColorData[i].blockType, listTempColor);
            }
        }
    }

    private void GetStartColorDictionary(int startBlockColorType, out List<BlockColorType> listIgnoreColors)
    {
        listIgnoreColors = new List<BlockColorType>();
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.A))) == 0)
            listIgnoreColors.Add(BlockColorType.A);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.B))) == 0)
            listIgnoreColors.Add(BlockColorType.B);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.C))) == 0)
            listIgnoreColors.Add(BlockColorType.C);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.D))) == 0)
            listIgnoreColors.Add(BlockColorType.D);
        if ((startBlockColorType & ((int)(1 << (int)BlockColorType.E))) == 0)
            listIgnoreColors.Add(BlockColorType.E);
    }

    //잘못들어간 데이터 표시.
    private void ShowFalseData(GameObject obj)
    {
        if (GameManager.instance.state != GameState.EDIT)
            return;

        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = new Color(124f / 255f, 40f / 255f, 40f / 255f);
        uiLabel.color = Color.red;
        uiLabel.text = "X";
        uiLabel.transform.localPosition = Vector3.zero;
    }
}
