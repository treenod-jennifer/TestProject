using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

#region 기믹데이터
public class UseGimmickData
{
    public List<int> listUseBlockType = new List<int>();
    public List<int> listUseDecoType = new List<int>();
    public List<int> listUseTopDecoType = new List<int>();
}
#endregion

public enum BlockManagrState
{
    STOP,
    READY,
    WAIT,
    MOVE,
    EVENT,
    BEFORE_WAIT,    //통상 : 인게임 아이템 사용시 , 모험 : 스킬 썼을 때 //이상태에서는 블럭의 움직임이 멈출때까지 기다려서 wait로 감
}

public enum GimmickDepth
{ 
    DECO_BASE       = 5,                        // (기존 0) 데코 뎁스 디폴트 (5)
    DECO_AREA       = DECO_BASE + 10,           // (기존 1) 데코 뎁스 1
    DECO_LAND       = DECO_AREA + 10,           // (기존 2) 데코 뎁스 2 / 잔디 (고정 기믹 있을 때)

    DECO_GROUND     = DECO_LAND + 10,           // (기존 3) 데코 뎁스 3 / plantPlace
    DECO_FIELD      = DECO_GROUND + 10,         // (기존 4) 데코 뎁스 4 / 잔디 (고정 기믹 없을 때)
    DECO_FLOOR      = DECO_FIELD + 10,          // (기존 5) 데코 뎁스 5 / 방해블럭1
    DECO_SURFACE    = DECO_FLOOR + 10,          // (기존 6) 데코 뎁스 6 / 하트길, 폭탄, 방해블럭2
    DECO_PLACE      = DECO_SURFACE + 10,        // (신규)   데코 뎁스 7 / plantPlace : 빵틀
    DECO_OBJECT     = DECO_PLACE + 10,          // (기존 7) 데코 뎁스 8 / 출발, 생성기
    DECO_UNDER      = DECO_OBJECT + 10,         // (기존 8) 데코 뎁스 9 / 물

    BLOCK_BASE      = DECO_UNDER + 10,          // (기존 10) 블럭 뎁스 디폴트 
    BLOCK_AREA      = BLOCK_BASE + 10,          // (기존 12) 블럭 뎁스 1 / 벌집
    BLOCK_LAND      = BLOCK_AREA + 10,          // 블럭 뎁스 2 / 물, 물 위에 떠야 하는 기믹 (ex.째깍폭탄 카운트)
    BLOCK_GROUND    = BLOCK_LAND + 10,          // (기존 15) 블럭 뎁스 3 / 땅

    DECO_CATCH = BLOCK_GROUND + 10,             // 데코 뎁스 (블럭 상단 데코 디폴트) / 얼음, 잡기돌
    DECO_COVER = DECO_CATCH + 10,               // 데코 뎁스 11 (블럭 상단) / 랜덤박스
    DECO_DISTURB = DECO_COVER + 10,             // 데코 뎁스 12 (블럭 상단) / 울타리
    DECO_HIDE = DECO_DISTURB + 10,              // 데코 뎁스 13 (블럭 상단) / 장막
    DECO_DISTROY = DECO_HIDE + 10,              // 데코 뎁스 14 (블럭 상단) / 용암

    FX_EFFECTBASE = DECO_DISTROY + 10,          // 이펙트 뎁스 디폴트
    INGAMEITEM = FX_EFFECTBASE + 10,            // 인게임 아이템 사용한 블럭 뎁스
    FX_EFFECT = INGAMEITEM + 10,                // INGAMEITEM 뎁스보다 위 이펙트 뎁스
    FX_FLYEFFECT = FX_EFFECT + 10,              // 날아가는 이펙트 뎁스
    UI_LABEL = FX_FLYEFFECT + 10,               // 에디터에서 사용되는 텍스트
}

public class ManagerBlock : MonoSingletonOnlyScene<ManagerBlock>
{
    public static readonly float BLOCK_SIZE = 78f;
    public int moveWay = 0;

    public const float NORMAL_SPEED = 3f;
    public const float HIGH_SPEED = 2f;

    static public float PI90 = Mathf.PI * 0.5f;

    //스프라이트 뎁스관련
    // public const int BLOCK_SRPRITE_MIN = 10; //블럭기본뎁스

    public const int BLOCK_SRPRITE_DEPTH_COUNT = 5;

    public const int NET_RPRITE_MIN = 3;
    public const int EFFECT_RAINBOW_LIGHT = 65;

    //폭탄생성갯수
    public static int MAKE_LINE_BLOCK_COUNT = 5;
    public static int MAKE_BOMB_BLOCK_COUNT = 7;
    public static int MAKE_RAINBOW_BLOCK_COUNT = 10;

    //모험모드
    public static int ADVENTURE_PANG_SAME_COLOR_COUNT = 4;

    //링커관련
    public const float linkerDistanceRatio = 1.13f;

    //폭탄생성조건
    public const int LINE_BOMB_BLOCK_COUNT = 5;
    public const int CIRCLE_BOMB_BLOCK_COUNT = 7;
    public const int RAINBOW_BOMB_BLOCK_COUNT = 10;

    public bool _portalPause = false;

    //레디 함수 작동중인지 검사
    private bool isPlayReadyAction = false;
    private Coroutine readyAction = null;

    //컨티뉴 함수 작동중인지 검사.
    public bool isPlayContinueAction = false;

    [SerializeField]
    private BlockManagrState mState = BlockManagrState.STOP;
    public BlockManagrState state
    {
        get { return mState; }
        set
        {
            if (mState == value)
                return;

            mState = value;
            waitTimer = 0;

            stateTiemr = 0f;

            switch (mState)
            {
                case BlockManagrState.EVENT:
                    if (checkSameColor)
                    {
                        ManagerBlock.instance.SameColorCount = 0;
                        checkSameColor = false;
                    }
                    _portalPause = false;
                    EventAction();
                    BlockMatchManager.instance.Check2X2Block();
                    break;
                case BlockManagrState.MOVE:
                    MoveAction();
                    break;
                case BlockManagrState.READY:
                    BlockMatchManager.instance.SetBlockLink();
                    break;
                case BlockManagrState.WAIT:
                    comboCount = 0;
                    BlockMatchManager.instance.SetBlockLink();
                    if (GameManager.gameMode != GameMode.COIN)
                    {
                        BlockMaker.instance.UseProb2();
                    }
                    ResetMoveTurn(); //우주선 턴 갱신
                    ResetListPangIndexBlockBread();
                    break;
            }
            //checkGameClear();
        }
    }

    public static Board[,] boards;
    public StageInfo stageInfo;
    public StageInfo tempStageInfo = null;

    //카펫 생성가능한지 검사하는 데서만 쓰임.
    public bool isCarpetStage = false;

    //인게임시계
    public float BlockTime = 0;

    #region 게임목표 //카운트를 내리지 않음
    public class CollectTargetCount
    {
        public int collectCount = 0; //모아야 하는 목표 수
        public int pangCount = 0; //모은 목표 수
    }
    public Dictionary<TARGET_TYPE, Dictionary<BlockColorType, CollectTargetCount>> dicCollectCount = new Dictionary<TARGET_TYPE, Dictionary<BlockColorType, CollectTargetCount>>();

    public int[] liveBlockTypeCount = null;
    public int[] totalCreatBlockTypeCount = null;
    public int[] creatBlockTypeTurnCount = null;
    public Dictionary<BlockType, int[]> dicTotalCreateBlockColor = new Dictionary<BlockType, int[]>();

    public int LiveCrackCount = 0;
    public int LiveKeyCount = 0;
    public int liveAutoBlockCount = 0;
    public int statueIndex = 0;

    public EncValue[] creatBombCount = new EncValue[4];
    public EncValue[] removeBlockCount = new EncValue[5];

    //public int score = 0;
    //public int flowrCount = 1;

    private EncValue _score = new EncValue();
    public int score
    {
        get { return _score.Value; }
        set { _score.Value = value; }
    }


    private EncValue _flowrCount = new EncValue(1);
    public int flowrCount
    {
        get { return _flowrCount.Value; }
        set { _flowrCount.Value = value; }
    }

    private EncValue _coins = new EncValue();
    public int coins
    {
        get { return _coins.Value; }
        set { _coins.Value = value; }
    }

    // 코인스테이지 피버 코인, ui에 표시할 때만 사용.
    private int _feverCoins = new int();
    public int feverCoins
    {
        get { return _feverCoins; }
        set { _feverCoins = value; }
    }

    //[System.NonSerialized] public int coins = 0;

    public int iceCount = 0;
    public int comboCount = 0;

    //스페셜이벤트
    public int specialEventCount = 0;
    public int specialEventAppearCount = 0;

    private EncValue _getSpecialEventBlock = new EncValue();
    public int getSpecialEventBlock
    {
        get { return _getSpecialEventBlock.Value; }
        set { _getSpecialEventBlock.Value = value; }
    }

    private EncValue _getSpecialEventRatio = new EncValue(1);
    public int getSpecialEventRatio
    {
        get { return _getSpecialEventRatio.Value; }
        set { _getSpecialEventRatio.Value = value; }
    }

    private EncValue _StageRankBonusRatio = new EncValue(0);
    public int StageRankBonusRatio
    {
        get { return _StageRankBonusRatio.Value; }
        set { _StageRankBonusRatio.Value = value; }
    }

    //재료모으기이벤트
    private EncValue _materialCollectEvent = new EncValue();
    public int materialCollectEvent
    {
        get { return _materialCollectEvent.Value; }
        set { _materialCollectEvent.Value = value; }
    }

    private EncValue _materialCollectPos = new EncValue();
    public int materialCollectPos
    {
        get { return _materialCollectPos.Value; }
        set { _materialCollectPos.Value = value; }
    }

    //월드랭킹
    private EncValue _worldRankingItemCount = new EncValue();
    public int worldRankingItemCount
    {
        get { return _worldRankingItemCount.Value; }
        set { _worldRankingItemCount.Value = value; }
    }

    //엔드컨텐츠
    private EncValue _endContentsItemCount = new EncValue();
    public int endContentsItemCount
    {
        get { return _endContentsItemCount.Value; }
        set { _endContentsItemCount.Value = value; }
    }
    #endregion
    
    public List<Board> startBoardList = new List<Board>();
    public List<GameObject> listObject = new List<GameObject>();
    public List<Potal> listPotal = new List<Potal>();
    public List<Crack> listCrack = new List<Crack>();
    public List<StoneStatue> listStatues = new List<StoneStatue>();
    public List<Plant2X2> listPlant = new List<Plant2X2>();
    public List<FlowerInk> listFlowerInk = new List<FlowerInk>();
    public List<Clover> listClover = new List<Clover>();

    public List<BlockColorBigJewel> listBigJewel = new List<BlockColorBigJewel>();
    public List<BlockWorldRankItem> listWorldRankItem = new List<BlockWorldRankItem>();
    public List<BlockSpaceShip> listSpaceShip = new List<BlockSpaceShip>();
    public List<SpaceShipExit> listSpaceShipExit = new List<SpaceShipExit>();
    public List<BlockBase> listPeas = new List<BlockBase>();
    public List<BlockHeart> listHeart = new List<BlockHeart>();
    public List<BlockHeartHome> listHeartHome = new List<BlockHeartHome>();
    public List<BlockPaint> listPaint = new List<BlockPaint>();
    public List<BlockWaterBomb> listWaterBomb = new List<BlockWaterBomb>();
    public List<BlockEndContentsItem> listEndContentsItem = new List<BlockEndContentsItem>();
    public List<BlockCannon> listCannonItem = new List<BlockCannon>();

    public List<MapDeco> listMapDeco = new List<MapDeco>();
    public List<SandBelt> listSandBelt = new List<SandBelt>();
    public List<Lava> listLava = new List<Lava>();
    public List<Water> listWater = new List<Water>();

    public Dictionary<int, List<BlockSodaJelly>> dicBlockSodaJelly = new Dictionary<int, List<BlockSodaJelly>>();
    public Dictionary<int, List<BlockGenerator>> dicBlockGenerator = new Dictionary<int, List<BlockGenerator>>();
    public Dictionary<int, List<BlockBread>> dicBlockBread = new Dictionary<int, List<BlockBread>>();
    public Dictionary<BlockColorType, List<BlockHeart>> dicHeart = new Dictionary<BlockColorType, List<BlockHeart>>();

    #region 단계석판 관련
    public Dictionary<int, List<CountCrack>> dicCountCrack = new Dictionary<int, List<CountCrack>>();
    public int activeCountCrackIdx = 0; //현재 활성화중인 단계석판 인덱스
    #endregion

    [System.NonSerialized] public Vector3 _initGroundPos;
    [System.NonSerialized] public Vector3 _targetPos;                //맵이 이동해야 될 위치.

    #region 스테이지옵션
    //[System.NonSerialized] 
    public bool waitBlockToFinishMoving = false;    //블럭이 멈추고 이벤트가 발생해야 선택가능 //
    [System.NonSerialized] public const float WAIT_TIME_FOR_NEXT_TOUCH = 0.5f;
    #endregion

    #region 게임 기타옵션
    public bool isTouchDownBlock = false;
    public bool isWaitForBombToBurst = true;
    //public bool setAllBombBurst = true;         //모든폭탄이 모두 같이 터짐 근처에 있는 폭탄중에서 가장 강한 조합으로 
    //public bool changeBombToFish = false;         //모든폭탄이 모두 같이 터질지
    //public bool toyLineEntryEvent = true;       //라인폭탄이 바뀌는 타이밍 조절
    #endregion

    #region 블럭움직임
    public AnimationCurve _curveTreeScaleOut;
    public AnimationCurve _curveTreeScaleIn;
    public AnimationCurve _curveBlockJump;
    public AnimationCurve _curveBlockPopUp;
    public AnimationCurve _curveRainbowPang;

    public AnimationCurve _curveSandMove;
    public AnimationCurve _curveSandMove2;

    public int blockDropSpeed = 2;
    int _lastFrame = 0;         //프레임을 체크하기 위한 변수.
    #endregion

    //public BlockBombType tempLineType = BlockBombType.LINE_V;                      //바뀌는 타이밍을 위해서 필요
    public BlockBombType LineType = BlockBombType.LINE_V;

    #region 점수
    public int ScoreNormalBlock = 80;
    public int ScoreLastPang = 1000;
    public int ScoreLineBomb = 100;
    #endregion

    //water
    public bool GetWater = false;
    // 노이 부스트 이벤트 실행되었는지?
    private bool isNoyBoostBomb = false;

    #region 난이도조절
    public bool useSameColor = false;
    public bool checkSameColor = false;
    public BlockColorType sameColor = BlockColorType.NONE;
    public int SameColorCount = 0;
    public int SameColorDiffcult = 0;
    #endregion

    public const int MAXCOUNT_COUNTCRACK = 4;

    void Start()
    {

    }

    //디폴트상태로 스테이지리셋
    public void RefreshBlockManager()
    {
        stageInfo = new StageInfo();
        stageInfo.ListBlock = new List<BlockInfo>();

        GameManager.gameMode = (GameMode)stageInfo.gameMode;
        GameManager.mExtend_Y = stageInfo.Extend_Y;

        ResetManager();
        SetDeafault();
    }

    public void RefreshDigBlockManager(int extendY, int gameMode)
    {
        stageInfo.gameMode = gameMode;
        stageInfo.Extend_Y = extendY;

        GameManager.gameMode = (GameMode)stageInfo.gameMode;

        //이전 맵 기억.
        int preMaxY = GameManager.MAX_Y;
        GameManager.mExtend_Y = stageInfo.Extend_Y;

        if (preMaxY < GameManager.MAX_Y)
        {
            ExtendMap(preMaxY);
        }
        else
        {
            ReduceMap();
        }
    }

    //맵확장할 때 사용하는 함수.
    void ExtendMap(int preMaxY)
    {
        int lengthX = boards.GetLength(0);
        int lengthY = boards.GetLength(1);

        Board[,] tempBoards = new Board[lengthX, lengthY];
        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                tempBoards[i, j] = boards[i, j];
            }
        }

        boards = new Board[GameManager.MAX_X, GameManager.MAX_Y];
        for (int i = 0; i < GameManager.MAX_X; i++)
        {
            for (int j = 0; j < GameManager.MAX_Y; j++)
            {
                //새로 생설될 데이터들.
                if (lengthY <= j)
                {
                    boards[i, j] = new Board();
                    boards[i, j].indexX = i;
                    boards[i, j].indexY = j;
                    boards[i, j].Block = null;
                    boards[i, j].DecoOnBoard = new List<DecoBase>();

                    BlockInfo blockInfo = new BlockInfo();
                    blockInfo.inX = i;
                    blockInfo.inY = j;
                    blockInfo.ListDeco = new List<DecoInfo>();

                    if (i == GameManager.MIN_X ||
                        i == GameManager.MAX_X - 1 ||
                        j <= GameManager.MIN_Y + 1 ||
                        j >= GameManager.MAX_Y - 3)
                    {
                        boards[i, j].IsActiveBoard = false;
                        boards[i, j].IsHasScarp = false;
                        blockInfo.isActiveBoard = 0;
                        blockInfo.isScarp = 0;

                        blockInfo.type = (int)BlockType.NONE;
                    }
                    else
                    {
                        boards[i, j].IsActiveBoard = true;
                        boards[i, j].IsHasScarp = false;

                        blockInfo.isScarp = 0;
                        blockInfo.isActiveBoard = 1;
                        blockInfo.type = (int)BlockType.NORMAL;
                        blockInfo.colorType = (int)BlockColorType.RANDOM;

                        if (j == GameManager.MIN_Y + 2)
                        {
                            boards[i, j].IsStartBoard = true;
                            DecoInfo boardInfo = new DecoInfo();
                            boardInfo.BoardType = (int)BoardDecoType.START;
                            boardInfo.index = int.MaxValue;
                            boardInfo.type = int.MaxValue;
                            blockInfo.ListDeco.Add(boardInfo);
                        }
                    }
                    stageInfo.ListBlock.Add(blockInfo);
                }
                else
                {
                    //기존 데이터는 그대로 붙여넣기.
                    boards[i, j] = tempBoards[i, j];

                    if (((preMaxY - 3) <= j) && (j < (GameManager.MAX_Y - 3)))
                    {
                        if (i == GameManager.MIN_X || i == GameManager.MAX_X - 1)
                            continue;
                        int index = stageInfo.ListBlock.FindIndex(x => (x.inX == i && x.inY == j));
                        if (index > -1)
                        {
                            BlockInfo blockInfo = stageInfo.ListBlock[index];

                            boards[i, j].IsActiveBoard = true;
                            boards[i, j].IsHasScarp = false;

                            blockInfo.isScarp = 0;
                            blockInfo.isActiveBoard = 1;
                            blockInfo.type = (int)BlockType.NORMAL;
                            blockInfo.colorType = (int)BlockColorType.RANDOM;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < stageInfo.ListBlock.Count; i++)
        {
            if (lengthY > stageInfo.ListBlock[i].inY)
                continue;

            BlockInfo blockInfo = stageInfo.ListBlock[i];
            Board board = PosHelper.GetBoard(blockInfo.inX, blockInfo.inY);

            board.IsActiveBoard = (blockInfo.isActiveBoard == 1) ? true : false;
            board.IsHasScarp = (blockInfo.isScarp == 1);
        }

        // 기존 데이터에 추가된 데이터 정렬
        SortListBlock();
    }

    void ReduceMap()
    {
        int lengthX = boards.GetLength(0);
        int lengthY = boards.GetLength(1);

        Board[,] tempBoards = new Board[lengthX, lengthY];
        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                tempBoards[i, j] = boards[i, j];
            }
        }

        boards = new Board[GameManager.MAX_X, GameManager.MAX_Y];
        for (int i = 0; i < GameManager.MAX_X; i++)
        {
            for (int j = 0; j < GameManager.MAX_Y; j++)
            {
                if ((GameManager.MAX_Y - 3) <= j)
                {
                    boards[i, j] = new Board();
                    boards[i, j].indexX = i;
                    boards[i, j].indexY = j;
                    boards[i, j].Block = null;
                    boards[i, j].DecoOnBoard = new List<DecoBase>();
                    boards[i, j].IsActiveBoard = false;
                    boards[i, j].IsHasScarp = false;

                    int index = stageInfo.ListBlock.FindIndex(x => (x.inX == i && x.inY == j));
                    if (index > -1)
                    {
                        stageInfo.ListBlock[index] = new BlockInfo();
                        stageInfo.ListBlock[index].inX = i;
                        stageInfo.ListBlock[index].inY = j;
                        stageInfo.ListBlock[index].ListDeco = new List<DecoInfo>();
                        stageInfo.ListBlock[index].isActiveBoard = 0;
                        stageInfo.ListBlock[index].isScarp = 0;
                        stageInfo.ListBlock[index].type = (int)BlockType.NONE;
                    }
                }
                else
                {
                    //기존 데이터 그대로 붙여넣기.
                    boards[i, j] = tempBoards[i, j];
                }
            }
        }

        //범위 외의 블럭정보 제거.
        List<BlockInfo> listRemove = new List<BlockInfo>();
        foreach (BlockInfo blockInfo in stageInfo.ListBlock)
        {
            if (GameManager.MAX_Y > blockInfo.inY)
                continue;
            listRemove.Add(blockInfo);
        }

        for (int i = 0; i < listRemove.Count; i++)
        {
            stageInfo.ListBlock.Remove(listRemove[i]);
        }
    }

    void ResetManager()
    {
        state = BlockManagrState.STOP;
        _initGroundPos = Vector3.zero;

        foreach (GameObject obj in listObject) Destroy(obj);
        startBoardList.Clear();
        listObject.Clear();
        listStatues.Clear();
        listPlant.Clear();
        listBigJewel.Clear();
        listMapDeco.Clear();
        listSandBelt.Clear();
        listLava.Clear();
        listWater.Clear();
        listClover.Clear();
        listFlowerInk.Clear();
        listWorldRankItem.Clear();
        listSpaceShip.Clear();
        listSpaceShipExit.Clear();
        listPeas.Clear();
        listHeart.Clear();
        listHeartHome.Clear();
        listPaint.Clear();
        listCannonItem.Clear();
        dicBlockSodaJelly.Clear();
        dicCountCrack.Clear();
        dicBlockGenerator.Clear();
        dicBlockBread.Clear();
        dicHeart.Clear();

        foreach (var obj in FlyStatue.flyStatueList) Destroy(obj.gameObject);
        FlyStatue.flyStatueList.Clear();

        BlockBase._listBlock = new List<BlockBase>();
        BlockBase._listBlockTemp = new List<BlockBase>();
        BlockBase._waitCount = 0;
        BlockBase.StopMovingBlockCount = 0;
        BlockBase.ExplodeBombCount = 0;
        DecoBase._listBoardDeco = new List<DecoBase>();
        DecoBase._listBoardDecoTemp = new List<DecoBase>();
        BlockDeco._listBlockDeco = new List<BlockDeco>();

        Net.NetCount = 0;
        BlockBomb._liveCount = 0;

        boards = new Board[GameManager.MAX_X, GameManager.MAX_Y];

        FloorManager.instance.ClearMap();
        FloorManager.instance.CreateMap();
        ScarpManager.instance.ClearMap();
        ScarpManager.instance.CreateMap();

        InGameObjectPoolManager.instance.InitEffectDataList();

        BlockMaker.instance.gimmickRemoveRatio = 0;
        BlockMaker.instance.SetBlockProbability();
        BlockMaker.instance.SetBlockProbability2();

        BlockMaker.instance.SetBlockProbabilityMapMake();

        iceCount = 0;
        comboCount = 0;
        coins = 10;
        //BlockBombType LineType = BlockBombType.LINE_V;
        specialEventCount = 0;
        specialEventAppearCount = 0;
        getSpecialEventBlock = 0;
        getSpecialEventRatio = 1;
        GetWater = false;
        _portalPause = false;
    }

    //게임매니져 데이타로 스테이지다시구성
    public void RefreshBlockManagerByStageData()
    {
        //용암관련
        _portalPause = false;
        movePanelPause = false;
        targetY = 0;
        lavaModeGameOver = false;
        LavaMoveCount = 0;
        LavaLevelCount = 0;
        makeLaveCount = 0;
        //땅파기
        digCount = 0;
        GameManager.instance.touchCount = 0;
        isPlayBeforeWaitAction = false;
        GetWater = false;

        //레디 상태에서의 액션
        isPlayReadyAction = false;

        //컨티뉴 상태에서의 액션
        isPlayContinueAction = false;

        //게임매니져에 게임상태 넣기
        GameManager.gameMode = (GameMode)stageInfo.gameMode;
        GameManager.mExtend_Y = stageInfo.Extend_Y;
        GameManager.MOVE_Y = 0;

        if (EditManager.instance != null && GameManager.gameMode == GameMode.ADVENTURE)
        {
            //Global.SetGameType_Adventure(0, 0);
        }


        if (stageInfo.widthCentering == 1)
            PosHelper.Block_OffsetX = 70f * -5.58f - 35f;
        else
            PosHelper.Block_OffsetX = 70f * -5.58f;

        if (stageInfo.heightCentering == 1)
        {
            PosHelper.Block_OffsetY = -10f + 70f * 6.3f - 35f;
        }
        else if (stageInfo.heightDownOneSpace == 1)
        {
            PosHelper.Block_OffsetY = -10f + 70f * 6.3f - 70f;
        }
        else
        {
            PosHelper.Block_OffsetY = -10f + 70f * 6.3f;
        }
        
        if (EditManager.instance != null)
        {
            statueIndex = EditManager.instance.statueIndex;
            EditManager.instance.dicBlockCount.Clear();
            EditManager.instance.dicDecoCount.Clear();
        }
        else
        {
            statueIndex = GameManager.instance.GetIngameRandom(0, stageInfo.statueCount);
        }

        SetGameMode((GameMode)stageInfo.gameMode);

        ResetManager();
        InitGroundPos();

        FloorManager.instance.ClearMap();
        FloorManager.instance.CreateMap();

        ScarpManager.instance.ClearMap();
        ScarpManager.instance.CreateMap();

        //전체/화면수/턴 수 카운트 초기화
        int bTypeCount = System.Enum.GetNames(typeof(BlockType)).Length;
        totalCreatBlockTypeCount = new int[bTypeCount];
        liveBlockTypeCount = new int[bTypeCount];
        creatBlockTypeTurnCount = new int[bTypeCount];
        for (int i = 0; i < bTypeCount; i++)
        {
            totalCreatBlockTypeCount[i] = 0;
            liveBlockTypeCount[i] = 0;
            creatBlockTypeTurnCount[i] = 0;
        }

        //생성된 컬러 기믹 수 카운트 초기화
        dicTotalCreateBlockColor = new Dictionary<BlockType, int[]>();

        //모은 타겟 카운트 초기화
        dicCollectCount.Clear();

        for (int i = 0; i < 4; i++)
        {
            creatBombCount[i] = new EncValue();
            creatBombCount[i].Value = 0;
        }

        for (int i = 0; i < 5; i++)
        {
            removeBlockCount[i] = new EncValue();
            removeBlockCount[i].Value = 0;
        }

        for (int i = 0; i < creatBlockTypeTurnCount.Length; i++)
            creatBlockTypeTurnCount[i] = stageInfo.turnCount;

        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                boards[i, j] = new Board();
                boards[i, j].indexX = i;
                boards[i, j].indexY = j;
                boards[i, j].IsActiveBoard = false;
                boards[i, j].IsHasScarp = false;
                boards[i, j].Block = null;
                boards[i, j].DecoOnBoard = new List<DecoBase>();
            }
        }

        int eventBlockCount = 0;
        int showEventBlockCount = 0;

        foreach (BlockInfo blockInfo in stageInfo.ListBlock)
        {
            Board board = PosHelper.GetBoard(blockInfo.inX, blockInfo.inY);
            board.IsActiveBoard = (blockInfo.isActiveBoard == 1) ? true : false;
            board.IsHasScarp = (blockInfo.isScarp == 1);

            if (blockInfo.type == (int)BlockType.PLANT2X2)
            {
                bool isFindPlant2X2 = false;
                foreach (var plant2 in listPlant)
                {
                    if (plant2.index == blockInfo.index)
                    {
                        plant2.blockBoardList.Add(board);
                        isFindPlant2X2 = true;
                        board.Block = plant2;
                        Plant2X2 plant2X2 = board.Block as Plant2X2;
                        plant2X2.initBlock(blockInfo.subType, blockInfo.subTarget);
                        break;
                    }
                }

                if (!isFindPlant2X2)
                {
                    board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                    Plant2X2 plant2X2 = board.Block as Plant2X2;
                    listPlant.Add(plant2X2);
                    plant2X2.blockBoardList.Add(board);
                }
            }
            else if (blockInfo.type == (int)BlockType.ColorBigJewel)
            {
                bool isFindBigJewel = false;
                foreach (var tempColorBigJewel in listBigJewel)
                {
                    if (tempColorBigJewel.index == blockInfo.index)
                    {
                        tempColorBigJewel.blockBoardList.Add(board);
                        isFindBigJewel = true;
                        board.Block = tempColorBigJewel;
                        BlockColorBigJewel bigColorJewel = board.Block as BlockColorBigJewel;
                        bigColorJewel.initBlock(blockInfo.subType, blockInfo.subTarget);
                        break;
                    }
                }

                if (!isFindBigJewel)
                {
                    board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                    BlockColorBigJewel bigColorJewel = board.Block as BlockColorBigJewel;
                    listBigJewel.Add(bigColorJewel);
                    bigColorJewel.blockBoardList.Add(board);
                    bigColorJewel.initBlock(blockInfo.subType, blockInfo.subTarget);
                }
            }
            else if (blockInfo.type == (int)BlockType.BLOCK_EVENT)
            {
                eventBlockCount++;

                /*if (Global.GameInstance.CanPlay_CollectEvent() && ServerContents.GetEventChapter(Global.eventIndex).active == 1 && ServerContents.GetEventChapter(Global.eventIndex).type == (int)EVENT_CHAPTER_TYPE.COLLECT) //추가 이벤트 타입확인 //이벤트 시간확인 //매니져데이타에서
                {
                    if ((materialCollectPos & (1 << eventBlockCount)) == 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);

                        BlockEvent blockEvent = board.Block as BlockEvent;
                        blockEvent.eventBlockIndex = eventBlockCount;
                        showEventBlockCount++;
                    }
                    else
                    {
                        if(blockInfo.isActiveBoard == 1)
                        {
                            if (hasDeco(blockInfo.ListDeco, BoardDecoType.ICE, BoardDecoType.NET))
                            {
                                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.NORMAL, BlockColorType.RANDOM, blockInfo.count, blockInfo.index, blockInfo.subType);
                            }
                            else if (blockInfo.count > 0)
                            {
                                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.PLANT, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                            }
                        }
                    }                    
                }
                else*/
                if (EditManager.instance != null)
                {
                    board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);

                    BlockEvent blockEvent = board.Block as BlockEvent;
                    blockEvent.eventBlockIndex = eventBlockCount;
                    showEventBlockCount++;
                }
                else
                {
                    if (hasDeco(blockInfo.ListDeco,decoType : new[] { BoardDecoType.ICE, BoardDecoType.NET, BoardDecoType.RANDOM_BOX }))
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.NORMAL, BlockColorType.RANDOM, blockInfo.count, blockInfo.index, blockInfo.subType);
                    }
                    else if (blockInfo.count > 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.PLANT, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                    }
                }
            }
            else if (blockInfo.type == (int)BlockType.BLOCK_EVENT_STONE)
            {
                eventBlockCount++;
                /*
                if (Global.GameInstance.CanPlay_CollectEvent() && ServerContents.GetEventChapter(Global.eventIndex).active == 1 && ServerContents.GetEventChapter(Global.eventIndex).type == (int)EVENT_CHAPTER_TYPE.COLLECT) //추가 이벤트 타입확인 //이벤트 시간확인 //매니져데이타에서
                {
                    if ((materialCollectPos & (1 << eventBlockCount)) == 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);

                        BlockEventStone blockEvent = board.Block as BlockEventStone;
                        blockEvent.eventBlockIndex = eventBlockCount;
                        showEventBlockCount++;
                    }
                    else
                    {
                        if (blockInfo.isActiveBoard == 1)
                        {
                            if (blockInfo.count > 0)
                                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.STONE, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                        }
                    }
                }
                else*/
                if (EditManager.instance != null)
                {
                    board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);

                    BlockEventStone blockEvent = board.Block as BlockEventStone;
                    blockEvent.eventBlockIndex = eventBlockCount;
                    showEventBlockCount++;
                }
                else
                {
                    if (blockInfo.count > 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.STONE, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                    }
                }
            }
            else if (blockInfo.type == (int)BlockType.BLOCK_EVENT_GROUND)
            {
                eventBlockCount++;
                /*
                if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_COLLECT_EVENT) && ServerContents.EventChapters.active == 1 && ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT)
                {
                    if ((materialCollectPos & (1 << eventBlockCount)) == 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                        BlockEventGround blockEvent = board.Block as BlockEventGround;
                        blockEvent.eventBlockIndex = eventBlockCount;
                        showEventBlockCount++;
                    }
                    else
                    {
                        if (blockInfo.isActiveBoard == 1)
                        {
                            if (blockInfo.count > 0)
                                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.GROUND, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                        }
                    }
                }
                else */
                if (EditManager.instance != null)
                {
                    board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);

                    BlockEventGround blockEvent = board.Block as BlockEventGround;
                    blockEvent.eventBlockIndex = eventBlockCount;
                    showEventBlockCount++;
                }
                else
                {
                    if (blockInfo.count > 0)
                    {
                        board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, BlockType.GROUND, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                    }
                }
            }
            else if (blockInfo.type == (int)BlockType.CANNON)
            {
                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
                BlockCannon blockCannon = board.Block as BlockCannon;
                AddCannon(blockCannon);
            }
            else if (blockInfo.type != (int)BlockType.NONE)
            {
                board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType, blockInfo.ListDeco);

                if (blockInfo.bombType != (int)BlockBombType.NONE && blockInfo.type != (int)BlockType.GROUND_BOMB)
                {
                    //타 기믹들과 liveBlockTypeCount, totalCreatBlockTypeCount 카운트 동기화
                    switch ((BlockBombType)blockInfo.bombType)
                    {
                        case BlockBombType.LINE:
                        case BlockBombType.LINE_H:
                        case BlockBombType.LINE_V:
                            board.Block.bombType = (BlockBombType)blockInfo.bombType;
                            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Line]++;
                            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Line]++;
                            break;
                        case BlockBombType.BOMB:
                            board.Block.bombType = (BlockBombType)blockInfo.bombType;
                            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Bomb]++;
                            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Bomb]++;
                            break;
                        case BlockBombType.RAINBOW:
                            board.Block.bombType = (BlockBombType)blockInfo.bombType;
                            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Rainbow]++;
                            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Rainbow]++;
                            break;
                    }
                    if(board.Block.type == BlockType.NORMAL)
                    {
                        NormalBlock tempBlock = board.Block as NormalBlock;
                        tempBlock.InitBlock();
                    }
                }

                //화면 수 포함인 기믹수 화면 전체수에서 미리 감소
                SetAdjustBlockLiveCount(blockInfo, blockBase : board.Block);
            }

            foreach (DecoInfo boardInfo in blockInfo.ListDeco)
            {
                if (boardInfo.BoardType == (int)BoardDecoType.STATUE)
                {
                    if (boardInfo.type == statueIndex)
                    {
                        bool isFindStatue = false;
                        foreach (var statue in listStatues)
                        {
                            if (statue.index == boardInfo.index)
                            {
                                statue.listBoards.Add(board);
                                board.AddDeco(statue);
                                isFindStatue = true;
                                break;
                            }
                        }
                        if (!isFindStatue)
                        {
                            DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(board, board.indexX, board.indexY, boardInfo);
                            boardDeco.SetSprite();
                        }
                    }
                }
                else if (boardInfo.BoardType == (int)BoardDecoType.ARROW)
                {
                    DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(board, board.indexX, board.indexY, boardInfo);
                    board.direction = (BlockDirection)boardInfo.index;
                    boardDeco.SetSprite();
                }
                else if (boardInfo.BoardType == (int)BoardDecoType.ICE) //BlockDeco 
                {
                    BlockDeco boardDeco = BoardDecoMaker.instance.MakeBlockDeco(board.Block, board.indexX, board.indexY, boardInfo);
                    boardDeco.SetSprite();
                }
                else if (boardInfo.BoardType != (int)BoardDecoType.NONE)
                {
                    DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(board, board.indexX, board.indexY, boardInfo);
                    boardDeco.SetSprite();
                }
            }

            //랜덤상자 최상단 데코로 설정
            for (int i = 0; i < board.DecoOnBoard.Count; i++)
            {
                RandomBox randomBox = board.DecoOnBoard[i] as RandomBox;
                Clover hide = board.DecoOnBoard[i] as Clover;

                if (randomBox != null)
                {
                    randomBox.SetBlockSpriteEnable();
                }

                if (hide != null)
                {
                    hide.SetBlockSpriteEnable();
                }
            }

        }

        //빈 보드데이타 넣어주기 //나중에바꾸기 만들어주고        
        if (EditManager.instance != null)
        {
            for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
            {
                for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
                {
                    bool isFindBoard = true;

                    for (int k = 0; k < stageInfo.ListBlock.Count; k++)
                    {
                        if (stageInfo.ListBlock[k].inX == i && stageInfo.ListBlock[k].inY == j)
                        {
                            isFindBoard = false;
                            break;
                        }
                    }

                    if (isFindBoard)
                    {
                        BlockInfo blockInfo = new BlockInfo();
                        blockInfo.inX = i;
                        blockInfo.inY = j;
                        blockInfo.ListDeco = new List<DecoInfo>();
                        blockInfo.isActiveBoard = 0;
                        blockInfo.isScarp = 0;
                        blockInfo.type = (int)BlockType.NONE;
                        stageInfo.ListBlock.Add(blockInfo);
                    }
                }
            }
        }

        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                if (!boards[i, j].IsActiveBoard && boards[i, j].IsHasScarp)
                {
                    float offsetX = 0f;
                    float offsetY = 0f;

                    if (stageInfo.widthCentering == 1)
                        offsetX = -35f;

                    if (stageInfo.heightCentering == 1)
                    {
                        offsetY = -35f;
                    }
                    else if (stageInfo.heightDownOneSpace == 1)
                    {
                        offsetY = -70f;
                    }
                    
                    ScarpManager.instance.CreateFloor(offsetX, offsetY);
                    ScarpManager.instance.SetTile(i, j, boards[i, j]);
                }
                if (!boards[i, j].IsActiveBoard)
                {
                    float offsetX = 0f;
                    float offsetY = 0f;

                    if (stageInfo.widthCentering == 1)
                        offsetX = -35f;

                    if (stageInfo.heightCentering == 1)
                    {
                        offsetY = -35f;
                    }
                    else if (stageInfo.heightDownOneSpace == 1)
                    {
                        offsetY = -70f;
                    }

                    FloorManager.instance.CreateFloor(offsetX, offsetY);
                    FloorManager.instance.SetTile(i, j, boards[i, j]);
                }
            }
        }

        if (listStatues.Count > 0)
        {
            foreach (var statue in listStatues)
            {
                statue.Init();
            }
        }

        if (listSandBelt.Count > 0)
        {
            List<SandBelt> tempSandBeltList = new List<SandBelt>();

            foreach (var sandBelt in listSandBelt)
            {
                sandBelt.Init();
            }
        }


        //용암모드일때 마지막줄에 용암 설정
        if (GameManager.gameMode == GameMode.LAVA)
        {
            DecoInfo decoLava = new DecoInfo();
            decoLava.BoardType = (int)BoardDecoType.LAVA;

            for (int j = GameManager.MAX_Y - 4; j < GameManager.MAX_Y; j++)
            {
                for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
                {
                    Board tempBoard = PosHelper.GetBoard(i, j); //Board tempBoard = PosHelper.GetBoard(i, GameManager.MAX_Y - 4);
                    if (tempBoard != null && ((j > GameManager.MAX_Y - 4 || tempBoard.IsActiveBoard)))// && tempBoard.IsActiveBoard)
                    {
                        if (tempBoard.Block != null)
                        {
                            tempBoard.Block.PangDestroyBoardData();
                        }

                        for (var index = tempBoard.DecoOnBoard.Count - 1; index >= 0; index--)
                        {
                            var tempDeco = tempBoard.DecoOnBoard[index];

                            // 울타리의 경우 인접한 보드도 데이터를 가지고 있기 때문에 제거할 수 있도록 처리 추가
                            if (tempDeco is GrassFenceBlock)
                            {
                                var fence = tempDeco as GrassFenceBlock;
                                fence.boardB?.RemoveDeco(fence);

                                // 인접한 보드에서 울타리를 설치 할 경우 boardB가 설정되지 않기 때문에 fence가 설치된 보드 데이터를 가져와 데코 제거
                                var fenceBoard = PosHelper.GetBoard(fence.inX, fence.inY);
                                fenceBoard?.RemoveDeco(fence);
                            }
                            else if (tempDeco is FenceBlock)
                            {
                                var fence = tempDeco as FenceBlock;
                                fence.boardB?.RemoveDeco(fence);

                                var fenceBoard = PosHelper.GetBoard(fence.inX, fence.inY);
                                fenceBoard?.RemoveDeco(fence);
                            }

                            tempDeco.DestroySelf();
                        }
                        tempBoard.DecoOnBoard.Clear();

                        BoardDecoMaker.instance.MakeBoardDeco(tempBoard, i, j, decoLava);
                    }
                }
            }
        }

        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                Board board = PosHelper.GetBoard(i, j);

                if (board != null && board.Block != null && board.BoardOnGrass.Count > 0)
                {
                    if (board.Block.IsThisBlockHasPlace() == false)
                    {
                        foreach (var tempGrass in board.BoardOnGrass)
                        {
                            Grass grassA = tempGrass as Grass;
                            grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                        }
                    }
                }
            }
        }

        //출발확률생성
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            startInfo.Probs = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                if (startInfo.countProb == null) break;

                for (int j = 0; j < startInfo.countProb[i]; j++)
                {
                    startInfo.Probs.Add(i + 1);
                }
            }

            startInfo.ProbsSub = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                if (startInfo.timeCount == null) break;

                for (int j = 0; j < startInfo.timeCount[i]; j++)
                {
                    startInfo.ProbsSub.Add(i + 1);
                }
            }

            startInfo.ProbsICE = new List<int>();

            if (startInfo.iceProb != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < startInfo.iceProb[i]; j++)
                    {
                        startInfo.ProbsICE.Add(i + 1);
                    }
                }
            }
        }

        //생성기 설정하기
        InitBlockGenerator();

        PreventingBombsOnStartup();

        if (stageInfo.gameMode == (int)GameMode.LAVA)
            GameManager.instance.moveCount = stageInfo.digCount;
        else
            GameManager.instance.moveCount = stageInfo.turnCount;

        //데코정렬수정
        for (int i = 0; i < listMapDeco.Count; i++)
        {
            if (PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 1, 0) != null && PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 1, 0).IsActiveBoard)
            {
                listMapDeco[i].gameObject.transform.localPosition -= new Vector3(1, 0, 0) * ManagerBlock.BLOCK_SIZE * 0.2f;
            }
            if (PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, -1, 0) != null && PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, -1, 0).IsActiveBoard)
            {
                listMapDeco[i].gameObject.transform.localPosition -= new Vector3(-1, 0, 0) * ManagerBlock.BLOCK_SIZE * 0.2f;
            }
            if (PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 0, 1) != null && PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 0, 1).IsActiveBoard)
            {
                listMapDeco[i].gameObject.transform.localPosition += new Vector3(0, 1, 0) * ManagerBlock.BLOCK_SIZE * 0.2f;
            }
            if (PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 0, -1) != null && PosHelper.GetBoard(listMapDeco[i].inX, listMapDeco[i].inY, 0, -1).IsActiveBoard)
            {
                listMapDeco[i].gameObject.transform.localPosition += new Vector3(0, -1, 0) * ManagerBlock.BLOCK_SIZE * 0.2f;
            }
        }

        //단계석판 설정
        InitCountCrack();
        
        //석판 갯수 설정(옛날 툴에서는 석판수가 0으로 들어왔기 때문에 해당 부분 코드를 남겨둠)
        SetTargetCrack();

        //에디터 모드에서만 검사하는 항목들.
        if (GameManager.instance.state == GameState.EDIT)
        {
            SetTargetCarpetCount();
            SetTargetCountCrack();
            SetUI_WorldRank();
        }

        //구버전 목표 데이터를 신버전 목표데이로 변환.
        CopyStageInfo_TargetCount_PrevVersionToNewVersion();

        //목표 초기화
        InitCollectTargetCount();

        //현재 목표중에 카펫이 있는 스테이지인지 검사.
        isCarpetStage = IsStageTarget(TARGET_TYPE.CARPET);
        FlyTarget.flyTargetCount = 0;
        Net.NetCount = 0;
        score = 0;
        
        // 엔드 컨텐츠 스테이지 설정
        SetEndContentsStage();

        //하트 설정
        InitHeart();

        if (showEventBlockCount > 0)
        {
            GameUIManager.instance.SetMaterialEvent();
        }

        //포탈끼리 연결하기
        SetPotal();

        LineType = stageInfo.isStartLineH == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;

        //코인스테이지설정
        SetCoinStage();
        
        // 부스팅 이벤트 변수 설정
        isNoyBoostBomb = false;

        //마무리
        state = BlockManagrState.READY;
    }

    #region 생성한 블럭 카운트 관리 관련
    public void AddCreateBlockColorCount(BlockType bType, int colorIdx)
    {
        if (ManagerBlock.instance.dicTotalCreateBlockColor.ContainsKey(bType) == false)
            ManagerBlock.instance.dicTotalCreateBlockColor.Add(bType, new int[5] { 0, 0, 0, 0, 0 });
        ManagerBlock.instance.dicTotalCreateBlockColor[bType][colorIdx]++;
    }

    /// <summary>
    /// 특정 블럭의 컬러가 검사하는 카운트 이상 생성이 되었는지 검사
    /// </summary>
    public bool IsLimitOver_BlockColorCreate(BlockType bType, int colorIdx, int limitCnt)
    {
        if (limitCnt == 0)
            return true;
        if (ManagerBlock.instance.dicTotalCreateBlockColor.ContainsKey(bType) == false)
            return false;
        return (ManagerBlock.instance.dicTotalCreateBlockColor[bType][colorIdx] >= limitCnt);
    }
    #endregion

    public void SetAdjustBlockLiveCount(BlockInfo blockInfo = null, BlockType tempBlockType = BlockType.NONE, BlockBase blockBase = null)
    {
        if (blockInfo == null) return;

        //화면에 설치된 타입 가져오기
        int adjustBlockType = (tempBlockType == BlockType.NONE) ? GetAdjustBlockType_ForBombType(blockInfo) : (int)tempBlockType;

        //전체수/화면수에 영향 주지 않는 경우 확인
        if (CheckExcept_AdjustBlock((BlockType)adjustBlockType, originBlock:blockBase) == false)
        {
            return;
        }

        //화면수 미포함 확인
        bool isAdjust = ManagerBlock.instance.stageInfo.listBlockAdjust.FindIndex(x => x == adjustBlockType) != -1 ? true : false;

        //화면수 미포함인 경우
        if (isAdjust == true)
        {
            ManagerBlock.instance.liveBlockTypeCount[adjustBlockType]--;
        }

        // 화면에 보일수 있는 검댕이 수에 미리 생성돼있는 검댕이가 포함돼있는 경우면, 미리 생성된 검댕이 수는 화면 전체 수에서 감소시켜줌.
        if ((ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 1 && adjustBlockType == (int)BlockType.BLOCK_BLACK))
        {
            ManagerBlock.instance.liveBlockTypeCount[adjustBlockType]--;
        }
    }

    //화면수/전체수 리스트에서 제외되는 경우 확인
    public bool CheckExcept_AdjustBlock(BlockType blockType, int blockSubType = -1, BlockBase originBlock = null)
    {
        int subType = blockSubType;

        if(originBlock != null && subType == -1)
        {
            //subType 찾아보기
            if(originBlock.type == BlockType.GROUND)
            {
                BlockGround ground = originBlock as BlockGround;
                subType = (int)ground.groundType;
            }
        }

        if ((blockType == BlockType.GROUND && subType == (int)GROUND_TYPE.DUCK)
            || (blockType == BlockType.GROUND && subType == (int)GROUND_TYPE.CANDY))
        {
            return false;
        }
        
        return true;
    }

    private int GetAdjustBlockType_ForBombType(BlockInfo blockInfo)
    {
        if(blockInfo.type == (int)BlockType.GROUND)
        {
            switch((GROUND_TYPE)blockInfo.subType)
            {
                case GROUND_TYPE.APPLE:
                    return (int)BlockType.GROUND_APPLE;
                case GROUND_TYPE.JEWEL:
                    return (int)BlockType.GROUND_JEWEL;
                case GROUND_TYPE.KEY:
                    return (int)BlockType.GROUND_KEY;
                case GROUND_TYPE.CIRCLE:
                    return (int)BlockType.GROUND_BOMB;
                case GROUND_TYPE.ICE_APPLE:
                    return (int)BlockType.GROUND_ICE_APPLE;
                case GROUND_TYPE.LINE:
                case GROUND_TYPE.LINE_H:
                case GROUND_TYPE.LINE_V:
                    return (int)BlockType.GROUND_BOMB;
            }
        }

        if(blockInfo.type != (int)BlockType.GROUND_BOMB)
        {
            // 폭탄 타입인 경우
            switch ((BlockBombType)blockInfo.bombType)
            {
                case BlockBombType.LINE:
                case BlockBombType.LINE_H:
                case BlockBombType.LINE_V:
                    return (int)BlockType.START_Line;
                case BlockBombType.BOMB:
                    return (int)BlockType.START_Bomb;
                case BlockBombType.RAINBOW:
                    return (int)BlockType.START_Rainbow;
            }
        }
        
        if (blockInfo.type == (int)BlockType.PLANT)
        {
            switch ((PLANT_TYPE)blockInfo.subType)
            {
                case PLANT_TYPE.LINE:
                case PLANT_TYPE.LINE_H:
                case PLANT_TYPE.LINE_V:
                    return (int)BlockType.START_Line;
                case PLANT_TYPE.CIRCLE:
                    return (int)BlockType.START_Bomb;
                case PLANT_TYPE.APPLE:
                    return (int)BlockType.APPLE;
                case PLANT_TYPE.KEY:
                    return (int)BlockType.KEY;
                case PLANT_TYPE.ICE_APPLE:
                    return (int)BlockType.ICE_APPLE;
                case PLANT_TYPE.SPACESHIP:
                    return (int)BlockType.SPACESHIP;
            }
        }
        else if (blockInfo.type == (int)BlockType.STONE)
        {
            switch ((STONE_TYPE)blockInfo.subType)
            {
                case STONE_TYPE.LINE:
                case STONE_TYPE.LINE_H:
                case STONE_TYPE.LINE_V:
                    return (int)BlockType.START_Line;
                case STONE_TYPE.CIRCLE:
                    return (int)BlockType.START_Bomb;
                case STONE_TYPE.APPLE:
                    return (int)BlockType.APPLE;
                case STONE_TYPE.KEY:
                    return (int)BlockType.KEY;
                case STONE_TYPE.SPACESHIP:
                    return (int)BlockType.SPACESHIP;
            }
        }

        return blockInfo.type;
    }

    private void InitCollectTargetCount()
    {
        //목표 순서대로 정렬
        SortListTargetInfo();

        //리스트를 딕셔너리로 옮김.
        for (int i = 0; i < stageInfo.listTargetInfo.Count; i++)
        {
            Dictionary<BlockColorType, CollectTargetCount> dicColorInfo = new Dictionary<BlockColorType, CollectTargetCount>();
            for (int j = 0; j < stageInfo.listTargetInfo[i].listTargetColorInfo.Count; j++)
            {
                BlockColorType colorType = (BlockColorType)stageInfo.listTargetInfo[i].listTargetColorInfo[j].colorType;
                CollectTargetCount collectTargetCount = new CollectTargetCount()
                {
                    collectCount = stageInfo.listTargetInfo[i].listTargetColorInfo[j].collectCount,
                    pangCount = 0,
                };
                dicColorInfo.Add(colorType, collectTargetCount);
            }
            dicCollectCount.Add((TARGET_TYPE)stageInfo.listTargetInfo[i].targetType, dicColorInfo);
        }
    }

    public void ResetTargetCount(TARGET_TYPE resetTargetType)
    {
        if (GameManager.instance.state != GameState.EDIT
            || EditManager.instance.windowMode == EditWindowMode.MAP_INFO
            || EditManager.instance.windowMode == EditWindowMode.HOME
            || stageInfo.gameMode == (int)GameMode.ADVENTURE)
        {
            return;
        }

        //컬러타입 있는 경우
        if(resetTargetType == TARGET_TYPE.FLOWER_INK)
        {
            int resetTargetTypeInt = (int)resetTargetType;
            Dictionary<BlockColorType, int> dicColorTargetCount = new Dictionary<BlockColorType, int>();

            //설치된 타겟 확인
            foreach (BlockInfo blockInfo in stageInfo.ListBlock)
            {
                foreach (DecoInfo boardInfo in blockInfo.ListDeco)
                {
                   // targetTypeInt = GetTargetTypeByBoardType((BoardDecoType)boardInfo.BoardType);
                    if (resetTargetType == TARGET_TYPE.FLOWER_INK &&
                        (BoardDecoType)boardInfo.BoardType == BoardDecoType.FLOWER_INK)
                    {
                        //잉크 기믹은 컬러 타입이 있으므로 예외 처리
                        BlockColorType targetColorTypeTemp = (BlockColorType)boardInfo.index;// 잉크 기믹 stargeInfo.index에 컬러 값 저장

                        if (dicColorTargetCount.ContainsKey(targetColorTypeTemp))
                        {
                            dicColorTargetCount[targetColorTypeTemp]++;
                        }
                        else
                        {
                            dicColorTargetCount.Add(targetColorTypeTemp, 1);
                        }
                    }
                }
            }

            foreach (var colorInfo in dicColorTargetCount)
            {
                SetStageInfo_TargetCount(resetTargetTypeInt, colorInfo.Value, (int)colorInfo.Key);
            }
        }
        else
        {
            int resetTargetTypeInt = (int)resetTargetType;
            int targetCount = 0;

            //설치된 타겟 확인
            foreach (BlockInfo blockInfo in stageInfo.ListBlock)
            {
                int targetTypeInt = GetTargetTypeByBlockType((BlockType)blockInfo.type, blockInfo, true);
                if (targetTypeInt != -1 && resetTargetTypeInt == targetTypeInt)
                {
                    // 블럭 타입이 목표 타입인 경우
                    targetCount++;
                }
                else if (resetTargetType == TARGET_TYPE.JEWEL &&
                    (BlockType)blockInfo.type == BlockType.GROUND && (GROUND_TYPE)blockInfo.subType == GROUND_TYPE.JEWEL)
                {
                    //보석은 예외 처리
                    targetCount++;
                }

                foreach (DecoInfo boardInfo in blockInfo.ListDeco)
                {
                    targetTypeInt = GetTargetTypeByBoardType((BoardDecoType)boardInfo.BoardType);

                    if (targetTypeInt != -1 && resetTargetTypeInt == targetTypeInt)
                    {
                        // 데코 타입이 목표 타입인 경우
                        targetCount++;
                    }
                }
            }

            //출발 설정된 타겟 확인
            foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
            {
                int targetTypeInt = GetTargetTypeByBlockType((BlockType)startInfo.type);

                if (targetTypeInt != -1 && resetTargetTypeInt == targetTypeInt)
                {
                    // 블럭 타입이 목표 타입인 경우
                    targetCount++;
                }
                else if (resetTargetType == TARGET_TYPE.JEWEL &&
                    (BlockType)startInfo.type == BlockType.GROUND_JEWEL)
                {
                    //보석은 예외 처리
                    targetCount++;
                }
            }

            //석상 갯수 예외 처리
            if (resetTargetType == TARGET_TYPE.STATUE && ManagerBlock.instance.listStatues.Count > 0)
                targetCount = ManagerBlock.instance.listStatues.Count;

            if (targetCount > 0)
            {
                SetStageInfo_TargetCount(resetTargetTypeInt, targetCount);
            }
        }
    }

    public void SetTargetAllCount()
    {
        if (GameManager.instance.state != GameState.EDIT
            || stageInfo.gameMode == (int)GameMode.ADVENTURE)
        {
            return;
        }

        Dictionary<TARGET_TYPE, int> dicTargetCount = new Dictionary<TARGET_TYPE, int>();
        Dictionary<TARGET_TYPE, Dictionary<BlockColorType, int>> dicColorTargetCount = new Dictionary<TARGET_TYPE, Dictionary<BlockColorType, int>>();

        //설치된 타겟 확인
        foreach (BlockInfo blockInfo in stageInfo.ListBlock)
        {
            int targetTypeInt = GetTargetTypeByBlockType((BlockType)blockInfo.type, blockInfo, true);
            if (targetTypeInt != -1 && (BlockType)blockInfo.type != BlockType.ColorBigJewel)
            {
                // 블럭 타입이 목표 타입인 경우
                TARGET_TYPE targetTypeTemp = (TARGET_TYPE)targetTypeInt;

                if (dicTargetCount.ContainsKey(targetTypeTemp)) dicTargetCount[targetTypeTemp]++;
                else dicTargetCount.Add(targetTypeTemp, 1);
            }
            else if ((BlockType)blockInfo.type == BlockType.GROUND && (GROUND_TYPE)blockInfo.subType == GROUND_TYPE.JEWEL)
            {
                //보석은 예외 처리
                TARGET_TYPE targetTypeTemp = TARGET_TYPE.JEWEL;
                if (dicTargetCount.ContainsKey(targetTypeTemp)) dicTargetCount[targetTypeTemp]++;
                else dicTargetCount.Add(targetTypeTemp, 1);
            }

            foreach (DecoInfo boardInfo in blockInfo.ListDeco)
            {
                targetTypeInt = GetTargetTypeByBoardType((BoardDecoType)boardInfo.BoardType);

                if ((BoardDecoType)boardInfo.BoardType == BoardDecoType.FLOWER_INK ||
                    targetTypeInt == (int)TARGET_TYPE.ICE)
                {
                    continue;
                }
                else if (targetTypeInt != -1)
                {
                    // 데코 타입이 목표 타입인 경우
                    TARGET_TYPE targetTypeTemp = (TARGET_TYPE)targetTypeInt;
                    if (dicTargetCount.ContainsKey(targetTypeTemp)) dicTargetCount[targetTypeTemp]++;
                    else dicTargetCount.Add(targetTypeTemp, 1);
                }
            }
        }

        //출발 설정된 타겟 확인
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            int targetTypeInt = GetTargetTypeByBlockType((BlockType)startInfo.type);
            if (targetTypeInt != -1)
            {
                // 블럭 타입이 목표 타입인 경우
                TARGET_TYPE targetTypeTemp = (TARGET_TYPE)targetTypeInt;

                if (dicTargetCount.ContainsKey(targetTypeTemp)) dicTargetCount[targetTypeTemp]++;
                else dicTargetCount.Add(targetTypeTemp, 1);
            }
            else if ((BlockType)startInfo.type == BlockType.GROUND_JEWEL)
            {
                //보석은 예외 처리
                TARGET_TYPE targetTypeTemp = TARGET_TYPE.JEWEL;
                if (dicTargetCount.ContainsKey(targetTypeTemp)) dicTargetCount[targetTypeTemp]++;
                else dicTargetCount.Add(targetTypeTemp, 1);
            }
        }

        //석상 갯수 예외 처리
        if (ManagerBlock.instance.listStatues.Count > 0)
            dicTargetCount.Add(TARGET_TYPE.STATUE, ManagerBlock.instance.listStatues.Count);

        //큰 화단 갯수 예외 처리
        if (ManagerBlock.instance.listBigJewel.Count > 0)
        {
            if (dicTargetCount.ContainsKey(TARGET_TYPE.FLOWER_POT)) dicTargetCount[TARGET_TYPE.FLOWER_POT] += ManagerBlock.instance.listBigJewel.Count;
            else dicTargetCount.Add(TARGET_TYPE.FLOWER_POT, ManagerBlock.instance.listBigJewel.Count);
        }

        //카펫 갯수 예외 처리
        if (stageInfo.listTargetInfo.Count > 0)
        {
            foreach (var target in stageInfo.listTargetInfo)
            {
                if (target.targetType == (int)TARGET_TYPE.CARPET)
                {
                    TargetColorInfo tempInfo = target.listTargetColorInfo.Find(x => x.colorType == 0);
                    if (tempInfo.collectCount > 0)
                    {
                        dicTargetCount.Add(TARGET_TYPE.CARPET, tempInfo.collectCount);
                    }
                }
            }
        }

        ClearStageInfo_TargetCount();

        foreach (var targetCountInfo in dicTargetCount)
        {
            SetStageInfo_TargetCount((int)targetCountInfo.Key, targetCountInfo.Value);
        }

        foreach (var targetCountInfo in dicColorTargetCount)
        {
            foreach (var colorInfo in targetCountInfo.Value)
            {
                SetStageInfo_TargetCount((int)targetCountInfo.Key, colorInfo.Value, (int)colorInfo.Key);
            }
        }
    }

    public int GetTargetTypeByBoardType(BoardDecoType decoType)
    {
        switch (decoType)
        {
            case BoardDecoType.ICE:
                return (int)TARGET_TYPE.ICE;
            case BoardDecoType.FLOWER_INK:
                return (int)TARGET_TYPE.FLOWER_INK;
        }

        return -1;
    }

    public int GetTargetTypeByBlockType(BlockType blockType, BlockInfo blockInfo = null, bool countCoveredBlock = false)
    {
        int subType = blockInfo != null? blockInfo.subType : -1;

        //맨 블럭
        switch (blockType)
        {
            case BlockType.KEY:
            case BlockType.GROUND_KEY:
            case BlockType.PLANT_KEY:
                return (int)TARGET_TYPE.KEY;
            case BlockType.BOX:
                return (int)TARGET_TYPE.SHOVEL;
            case BlockType.PEA:
            case BlockType.PEA_BOSS:
                return (int)TARGET_TYPE.PEA;
            case BlockType.SPACESHIP:
                return (int)TARGET_TYPE.SPACESHIP;
            case BlockType.LITTLE_FLOWER_POT:
            case BlockType.ColorBigJewel:
                return (int)TARGET_TYPE.FLOWER_POT;
            case BlockType.HEART:
                return (int)TARGET_TYPE.HEART;
            case BlockType.GROUND_JEWEL:
                return (int)TARGET_TYPE.JEWEL;
            case BlockType.BREAD:
                {
                    if (blockInfo.index == 1)
                        return (int)TARGET_TYPE.BREAD_1;
                    else if(blockInfo.index == 2)
                        return (int)TARGET_TYPE.BREAD_2;
                    else
                        return (int)TARGET_TYPE.BREAD_3;
                }
        }

        //출발 설정은 싸여진 블럭 제외
        if (countCoveredBlock == false) return -1;

        //싸여진 블럭
        switch (blockType)
        {
            case BlockType.GROUND:
                GROUND_TYPE groundSubType = (GROUND_TYPE)subType;
                switch (groundSubType)
                {
                    case GROUND_TYPE.JEWEL:
                        return (int)TARGET_TYPE.JEWEL;
                    case GROUND_TYPE.KEY:
                        return (int)TARGET_TYPE.KEY;
                }
                break;
            case BlockType.PLANT:
                PLANT_TYPE plantSubType = (PLANT_TYPE)subType;
                switch (plantSubType)
                {
                    case PLANT_TYPE.KEY:
                        return (int)TARGET_TYPE.KEY;
                    case PLANT_TYPE.SPACESHIP:
                        return (int)TARGET_TYPE.SPACESHIP;
                }
                break;
            case BlockType.STONE:
                STONE_TYPE stoneSubType = (STONE_TYPE)subType;
                switch (stoneSubType)
                {
                    case STONE_TYPE.KEY:
                        return (int)TARGET_TYPE.KEY;
                    case STONE_TYPE.SPACESHIP:
                        return (int)TARGET_TYPE.SPACESHIP;
                }
                break;
        }
        return -1;
    }

    void SetDeafault()
    {
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                boards[i, j] = new Board();
                boards[i, j].indexX = i;
                boards[i, j].indexY = j;
                boards[i, j].Block = null;
                boards[i, j].DecoOnBoard = new List<DecoBase>();

                BlockInfo blockInfo = new BlockInfo();
                blockInfo.inX = i;
                blockInfo.inY = j;
                blockInfo.ListDeco = new List<DecoInfo>();

                if (i == GameManager.MIN_X ||
                    i == GameManager.MAX_X - 1 ||
                    j <= GameManager.MIN_Y + 1 ||
                    j >= GameManager.MAX_Y - 3)
                {
                    boards[i, j].IsActiveBoard = false;
                    boards[i, j].IsHasScarp = false;
                    blockInfo.isActiveBoard = 0;
                    blockInfo.isScarp = 0;

                    blockInfo.type = (int)BlockType.NONE;
                }
                else
                {
                    boards[i, j].IsActiveBoard = true;
                    boards[i, j].IsHasScarp = false;
                    boards[i, j].Block = BlockMaker.instance.MakeBlockBase(i, j, BlockType.NORMAL, BlockColorType.RANDOM);

                    blockInfo.isScarp = 0;
                    blockInfo.isActiveBoard = 1;
                    blockInfo.type = (int)BlockType.NORMAL;
                    blockInfo.colorType = (int)BlockColorType.RANDOM;

                    if (j == GameManager.MIN_Y + 2)
                    {
                        boards[i, j].IsStartBoard = true;
                        DecoInfo boardInfo = new DecoInfo();
                        boardInfo.BoardType = (int)BoardDecoType.START;
                        boardInfo.index = int.MaxValue;
                        boardInfo.type = int.MaxValue;

                        blockInfo.ListDeco.Add(boardInfo);
                    }
                }

                stageInfo.ListBlock.Add(blockInfo);
            }
        }
    }

    public void InitGroundPos()
    {
        if (GameManager.instance.state != GameState.EDIT)
        {
            GameUIManager.instance.groundAnchor.transform.localPosition = _initGroundPos;
            GameUIManager.instance.groundMoveTransform.localPosition = Vector3.zero;

            if (GameManager.gameMode == GameMode.LAVA)
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);
        }
    }

    void SetPotal()
    {
        for (int i = 0; i < listPotal.Count; i++)
        {
            for (int j = 0; j < listPotal.Count; j++)
            {
                if (listPotal[i].type == POTAL_TYPE.IN &&
                    listPotal[j].type == POTAL_TYPE.OUT &&
                    listPotal[i].index == listPotal[j].index)
                {
                    listPotal[i].exitPotal = listPotal[j];
                    listPotal[j].exitPotal = listPotal[i];

                    listPotal[i].SetPotal();
                    listPotal[j].SetPotal();
                }
            }
        }
    }

    float stateTiemr = 0;

    void Update()
    {
        if (GameManager.instance.state == GameState.EDIT ||
            GameManager.instance.state == GameState.NONE) return;

        BlockTime += Global.deltaTimePuzzle;
        BlockBase._waitCount = 0;

        stateTiemr += Global.deltaTimePuzzle;

        for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
        {
            for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
            {
                Board getBoard = PosHelper.GetBoard(j, i);
                if (getBoard != null)
                {
                    if (getBoard.Block != null) getBoard.Block.UpdateBlock();
                    if (getBoard.TempBlock != null) getBoard.TempBlock.UpdateBlock();
                }
            }
        }

        UpdateCoinStage();

        if (GameManager.instance.state != GameState.GAMECLEAR &&
           GameManager.instance.state != GameState.GAMEOVER)
        {
            /*
            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX && 
                AdventureManager.instance != null &&
                stateTiemr > 10)
            {
                Debug.Log(
                    "게임스테이츠 = " + state.ToString() + 
                    ",Net Count = " + Net.NetCount + 
                    ", Blockbomb LiveCount = " + BlockBomb._liveCount + 
                    ", blockCount = " + BlockBase._waitCount + " : " + BlockBase._listBlock.Count +
                    "어드밴쳐매니져 =" + AdventureManager.instance.AdventureState
                    );
            }
            */
            switch (state)
            {
                case BlockManagrState.MOVE:
                    if (checkBlockWait())
                    {
                        state = BlockManagrState.EVENT;
                    }
                    else
                    {
                        BlockMatchManager.instance.SetBlockLink();
                    }
                    break;
                case BlockManagrState.READY:
                    if (isPlayReadyAction == false)
                    {
                        if (readyAction != null)
                            StopCoroutine(readyAction);
                        readyAction = StartCoroutine(CoReadyAction());
                    }
                    break;

                case BlockManagrState.WAIT:
                    waitTimer += Global.deltaTimePuzzle;
                    if (SearchMatchBlock() == false && checkBlockWait())
                    {
                        if (NoMatchableSpace() == false && HasAutoMatch_FlowerInk() == false)
                        {   //매치 가능한 공간이 존재하지 않는 경우.
                            state = BlockManagrState.STOP;
                            if (EditManager.instance != null)
                            {
                                EditManager.instance.ResetTool();
                            }
                            else
                            {
                                GameManager.instance.StageOver();
                            }
                        }
                        else
                        {
                            state = BlockManagrState.STOP;
                            StartCoroutine(CoResetNoMoreMove());
                        }
                    }
                    break;
                case BlockManagrState.EVENT:
                    if (!checkBlockWait())
                    {
                        BlockMatchManager.instance.SetBlockLink();
                    }
                    break;
                case BlockManagrState.BEFORE_WAIT:
                    if (checkBlockWait())
                    {
                        if (GameManager.gameMode != GameMode.ADVENTURE && isPlayBeforeWaitAction == false)
                        {
                            isPlayBeforeWaitAction = true;
                            StartCoroutine(CoWaitEnd_BeforeWaitAction());
                        }
                    }
                    else
                    {
                        BlockMatchManager.instance.SetBlockLink();
                    }
                    break;
            }
        }

        //모래바람
        if (listSandBelt.Count > 0)
        {
            List<SandBelt> tempSandBeltList = new List<SandBelt>();

            foreach (var sandBelt in listSandBelt)
            {
                sandBelt.MoveSandBelt(_curveSandMove.Evaluate((BlockTime * 0.35f) % 1f), _curveSandMove2.Evaluate((BlockTime * 0.35f) % 1f));
            }
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {

            if (Input.GetKeyDown(KeyCode.C))
            {
                //GameManager.instance.moveCount = 0;
                score = 1000000;
                GameManager.instance.StageClear();
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                GameManager.instance.moveCount = 0;
                score = 10;
                GameManager.instance.StageClear();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameManager.instance.StageFail();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                BlockBase lineBlock = PosHelper.GetRandomBlock();
                if (lineBlock != null)
                {
                    int randomLine = Random.Range(0, 4);
                    if (randomLine == 0) lineBlock.bombType = BlockBombType.BOMB;
                    else if (randomLine == 2)
                    {
                        lineBlock.bombType = BlockBombType.RAINBOW;
                        lineBlock.colorType = BlockColorType.A;
                    }
                    else if (randomLine == 1) lineBlock.bombType = BlockBombType.LINE_V;
                    else lineBlock.bombType = BlockBombType.LINE_H;
                    ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                    lineBlock.JumpBlock();
                    lineBlock.Destroylinker();
                }
            }

            /*
            if((Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT))
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    for (int i = 0; i < AdventureManager.instance.EnemyLIst.Count; i++)
                    {
                        if (AdventureManager.instance.EnemyLIst[i].GetState() != ANIMAL_STATE.DEAD)
                        {
                            var animal = AdventureManager.instance.AnimalLIst[0];
                            AdventureManager.instance.EnemyLIst[i].Pang(animal, 99999);
                        }
                    }
                    AdventureManager.instance.EventAction();
                }

                if (Input.GetKeyDown(KeyCode.D))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (AdventureManager.instance.AnimalLIst[i].GetState() != ANIMAL_STATE.DEAD)
                        {
                            AdventureManager.instance.AnimalLIst[i].Pang(99999);
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (AdventureManager.instance.AnimalLIst[i].GetState() == ANIMAL_STATE.DEAD)
                            continue;

                        if (AdventureManager.instance.AnimalLIst[i].skillItem == null)
                            continue;

                        AdventureManager.instance.AnimalLIst[i].skillItem.AddSkillPointUsingPercent(1.0f);
                    }
                }
            }*/
        }
    }

    //스테이지 시작한 직후 발생하는 액션들
    public IEnumerator CoReadyAction()
    {
        isPlayReadyAction = true;

        //잠시 대기(시작 시, 모든 블럭의 상태가 잠시 대기 상태이기 때문에 한 프레임 기다렸다가 검사)
        yield return new WaitForSeconds(0.1f);

        //블럭들 움직임 멈출 때 까지 대기
        yield return new WaitUntil(() => checkBlockWait());

        //자동 매치 처리
        yield return CoAutoMatchEvent();

        //장막 처리
        CloverEvent();
        
        //유저 입력 없이는 땅파기 맵 이동 연출 동작하지 않음
        state = BlockManagrState.WAIT;

        isPlayReadyAction = false;
    }

    //BEFORE_WAIT상태에서 재생되는 액션이 끝나길 기다린 뒤, 상태를 WAIT로 전환해주는 함수
    private IEnumerator CoWaitEnd_BeforeWaitAction()
    {
        // 대포 발사
        yield return CoCheckCannon(null);
        
        //페인트 터뜨리기
        yield return CoPangPaint();

        //단계석판이 모두 제거 되었을 때, 다음 단계석판이 등장하는 연출
        yield return CoCountCrackAppearEvent();

        //자동 매치 및 땅파기 모드
        yield return CoMatchEvent();

        //목표 모두 모았다면 게임 종료.
        if (state == BlockManagrState.WAIT)
        {
            yield break;
        }

        //목표 모두 모았는지 검사
        if (GameManager.instance.checkAllRemoveTarget())
        {
            state = BlockManagrState.WAIT;
            yield break;
        }

        //우주선 이벤트 상태 갱신
        ResetStopEvent();

        //생성기 이미지 갱신
        BlockGeneratorImageCheck();
        
        //장막 처리
        CloverEvent();

        isPlayBeforeWaitAction = false;
        state = BlockManagrState.WAIT;
    }

    public void MakeBombRandom()
    {
        BlockBase lineBlock = PosHelper.GetRandomBlock();
        if (lineBlock != null)
        {
            int randomLine = GameManager.instance.GetIngameRandom(0, 3);

            if (randomLine == 0) lineBlock.bombType = BlockBombType.BOMB;
            else if (randomLine == 1) lineBlock.bombType = BlockBombType.LINE_V;
            else lineBlock.bombType = BlockBombType.LINE_H;

            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            lineBlock.JumpBlock();
            lineBlock.Destroylinker();
        }
    }

    void LateUpdate()
    {
        if (state == BlockManagrState.STOP)
            return;

        if (creatBlock)
        {
            if (GameManager.gameMode == GameMode.ADVENTURE)
            {
                return;
            }

            int offsetY = 1;
            float blockRatioY = 1;
            BlockDirection checkDirection = BlockDirection.UP;
            if (ManagerBlock.instance.stageInfo.reverseMove == 1)
            {
                offsetY = -1;
                blockRatioY = 0.01f;
                checkDirection = BlockDirection.DOWN;
            }

            for (int i = 0; i < startBoardList.Count; i++)
            {
                if (startBoardList[i].IsStartBoard && startBoardList[i].Block == null && startBoardList[i].IsCanFill()
                    && startBoardList[i].IsNotDisturbBlock(checkDirection))
                {
                    startBoardList[i].Block = BlockMaker.instance.MakeBlockByStartBoard(startBoardList[i].indexX, startBoardList[i].indexY);
                    startBoardList[i].Block.transform.localPosition = PosHelper.GetPosByIndex(startBoardList[i].Block.indexX, startBoardList[i].Block.indexY - offsetY);
                    startBoardList[i].Block.MoveToTargetPos(PosHelper.GetPosByIndex(startBoardList[i].Block.indexX, startBoardList[i].Block.indexY));
                    startBoardList[i].Block.mainSprite.customFill.blockRatio = 1f * blockRatioY;
                    startBoardList[i].Block.SpriteRatio(1f * blockRatioY);
                    startBoardList[i].Block.rTargetPos = PosHelper.GetPosByIndex(startBoardList[i].Block.indexX, startBoardList[i].Block.indexY);
                    startBoardList[i].Block.isMakeByStart = true;

                    //블럭이 생성될 때, 작동할 연출.
                    startBoardList[i].MakeBlockAction();

                    BlockBase downBlock = PosHelper.GetBlock(startBoardList[i].Block.indexX, startBoardList[i].Block.indexY + offsetY);
                    if (downBlock != null) startBoardList[i].Block._velocity = downBlock._velocity;

                    //코인스테이지일때 코인나오기
                    if (GameManager.gameMode == GameMode.COIN
                        && GameManager.instance.state != GameState.GAMECLEAR
                        && GameManager.instance.state != GameState.GAMEOVER
                        && startBoardList[i].Block.IsNormalBlock())
                    {
                        int coinProp = Random.Range(0, 1000);
                        if (coinProp <= ManagerBlock.instance.stageInfo.coinProp)
                        {
                            startBoardList[i].Block.hasCoin = true;
                            startBoardList[i].Block.AddCoin();
                        }

                        if (coinState == COINSTATE.FEVER)
                            startBoardList[i].Block.AddCoin();
                    }
                    /*
                    //스페셜이벤트
                    if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_SPECIAL_EVENT) && Global.specialEventIndex > 0 )
                    {
                        if(startBoardList[i].Block.IsNormalBlock() && specialEventCount < GameManager.instance.specialEventMaxCount && specialEventAppearCount < GameManager.instance.specialEventAppearMaxCount)// GameManager.instance.specialEvent.maxCount
                        {
                            int randProb = (startBoardList[i].Block.type == BlockType.FIRE_WORK) ? 0 :  Random.Range(0, 100);
                            if ((float)randProb < (GameManager.instance.specialEventPorb*100))
                            {
                                specialEventCount++;
                                specialEventAppearCount++;

                                startBoardList[i].Block.AddSpecialEventSprite();
                            }
                        }
                    }

                    //알파벳 이벤트
                    if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) && ManagerAlphabetEvent.instance != null
                        && ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == true)
                    {
                        if (startBoardList[i].Block.IsNormalBlock() && ManagerAlphabetEvent.alphabetIngame.IsCanAppearAlphabetBlock_N())
                        {
                            int alphabetIndex = ManagerAlphabetEvent.alphabetIngame.GetNextAlphabetIdx();
                            startBoardList[i].Block.AddAlphabetEventBlock(alphabetIndex, true, true);
                            ManagerAlphabetEvent.alphabetIngame.screenCount_N++;
                        }
                    }

                    */
                    //얼음생성
                    Board tempBoard = startBoardList[i];
                    foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
                    {
                        //게임클리어시 노말블럭만 나오기
                        if (GameManager.instance.state != GameState.GAMECLEAR &&
                            GameManager.instance.state != GameState.GAMEOVER)
                        {
                            if (startInfo.type == (int)BlockType.ICE
                                && startBoardList[i].Block.blockDeco == null
                                && startBoardList[i].Block.IsCanCoverIce() == true
                                && startInfo.probability > 0
                                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
                            )
                            {
                                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                                if (rand <= startInfo.probability)
                                {
                                    int randCount = 0;
                                    //컬러가 지정돼 있는 얼음인데 출발에서 나오는 블럭이 해당 컬러가 아니면 얼음생성 하지않음.
                                    if (isColorIce(startInfo.iceProb) == true &&
                                        (startBoardList[i].Block.colorType == BlockColorType.NONE ||
                                        startInfo.iceProb[(int)startBoardList[i].Block.colorType - 1] <= 0))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                                    }

                                    DecoInfo boardInfo = new DecoInfo();
                                    boardInfo.BoardType = (int)BoardDecoType.ICE;
                                    boardInfo.count = startInfo.Probs[randCount];

                                    BlockDeco boardDeco = BoardDecoMaker.instance.MakeBlockDeco(startBoardList[i].Block, startBoardList[i].indexX, startBoardList[i].indexY, boardInfo);
                                    boardDeco.SetSprite();
                                    boardDeco.uiSprite.customFill.blockRatio = 1f * blockRatioY;
                                }

                            }
                        }
                    }
                }
            }
        }
    }

    private bool isColorIce(int[] iceProb)
    {
        bool isColor = false;
        for (int i = 0; i < iceProb.Length; i++)
        {
            if (iceProb[i] > 0)
            {
                isColor = true;
                break;
            }
        }
        return isColor;
    }

    public bool checkBlockWait()
    {
        if (BlockBase._listBlock.Count == 0 && BlockBomb._liveCount == 0) return true;

        if (BlockBase._waitCount != BlockBase._listBlock.Count)
        {
            _lastFrame = Time.frameCount;
        }

        if (Net.NetCount > 0)
            return false;

        if (BlockBase._waitCount == BlockBase._listBlock.Count && Time.frameCount - _lastFrame > 8 && BlockBomb._liveCount == 0)
        {
            return true;
        }

        return false;
    }

    void MoveAction()
    {
        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            return;
        }

        if (GameManager.instance.lineBombRotate)
            LineType = (LineType == BlockBombType.LINE_V) ? BlockBombType.LINE_H : BlockBombType.LINE_V;

        StartCoroutine(CoMoveAction());
    }

    IEnumerator CoMoveAction()
    {
        if (GameManager.instance.lineBombRotate)
        {
            for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
            {
                for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
                {
                    BlockBase block = PosHelper.GetBlockScreen(j, i);
                    if (block != null)
                    {
                        block.MoveAction();
                    }
                }
            }
        }
        yield return null;
    }

    /// <summary>
    /// 자동 매치 처리 및 땅파기 연출
    /// </summary>
    public IEnumerator CoMatchEvent(bool isFirstLoop = true)
    {
        //하트 길따라 이동
        if (listHeart.Count > 0)
        {
            yield return new WaitUntil(() => checkBlockWait());
            yield return CoMoveHeart();
        }

        //자동 매치 처리
        yield return new WaitUntil(() => checkBlockWait());
        yield return CoAutoMatchEvent();

        //목표 모두 모았는지 검사
        if (GameManager.instance.checkAllRemoveTarget())
        {
            state = BlockManagrState.WAIT;
            yield break;
        }

        //블럭들 움직임 멈출 때 까지 대기
        yield return new WaitUntil(() => checkBlockWait());

        //땅파기 모드에서, 땅 움직이는 연출
        if (GameManager.gameMode == GameMode.DIG && CheckDigCount() == true)
        {
            yield return CoMoveGround();
            yield return CoMatchEvent(false);

            if (isFirstLoop == true)
            {
                MakeBlockStopEvent();
            }
        }
        yield return null;
    }

    //beforeWait 함수 작동중인지 검사
    bool isPlayBeforeWaitAction = false;

    /// <summary>
    /// 땅파기 연출
    /// </summary>
    IEnumerator CoMoveGround()
    {
        ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);
        noizeGroundTimer = 0;
        while (true)
        {
            if (MoveGround())
            {
                break;
            }
            yield return null;
        }

        movePanelPause = false;
        yield return null;
    }

    //땅파기 웨이브 전환될 때, 이벤트 멈추는 경우 확인
    void MakeBlockStopEvent()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                if (block != null)
                {
                    if (block.IsStopEventAtMoveGround())
                    {
                        block.isStopEvent = true;
                    }
                }
            }
        }
    }

    void EventAction()
    {
        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            return;
        }

        if (GameManager.instance.checkAllRemoveTarget())
        {
            state = BlockManagrState.WAIT;
            return;
        }

        StartCoroutine(CoEventAction());
    }

    IEnumerator CoEventAction()
    {
        float waitTime = 0f;

        if (GameManager.gameMode == GameMode.ADVENTURE)
        {
            AdventureManager.instance.EventAction();
            yield return null;

            while (AdventureManager.instance.IsFinishedEventAction() == false)
            {
                yield return null;
            }
            AdventureManager.instance.LockBlock(false);
        }
        
        yield return NoyBoostingEvent();

        // 대포 발사
        yield return CoCheckCannon(null);
        
        //페인트 터뜨리기
        yield return CoPangPaint();
        
        //단계석판 검사
        yield return CoCountCrackAppearEvent();

        //자동 매치 및 땅파기 모드.
        yield return CoMatchEvent();

        //자동 매치 후 목표 모았음
        if (state == BlockManagrState.WAIT)
        {
            yield break;
        }

        #region 블럭별 개별 이벤트
        waitTime = 0;
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                if (block != null)
                {
                    if (block.EventAction())
                        waitTime = 0.5f;
                }
            }
        }

        while (waitTime > 0)
        {
            waitTime -= Global.deltaTimePuzzle;
            yield return null;
        }

        while (!checkBlockWait())
        {
            yield return null;
        }
        #endregion

        //우주선 위로 올라감
        yield return CoMoveSpaceShip();

        yield return CoBombAllWaterBomb();

        yield return new WaitUntil(() => checkBlockWait());

        #region 데코별 개별 이벤트
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board board = PosHelper.GetBoardSreeen(j, i);
                if (board != null)
                {
                    foreach (var deco in board.DecoOnBoard)
                        if (deco.EventAction()) waitTime = 0.2f;
                }
            }
        }

        while (waitTime > 0)
        {
            waitTime -= Global.deltaTimePuzzle;
            yield return null;
        }

        while (DecoBase._waitCount > 0)
        {
            yield return null;
        }
        #endregion

        #region 물 증식
        if (listWater.Count > 0)
        {
            if (GetWater == false)
            {
                List<Board> tempBoard = new List<Board>();
                GetNormalBoardForWater(tempBoard);

                if (tempBoard.Count > 0)
                {
                    //물늘어나기
                    int randomBoard = GameManager.instance.GetIngameRandom(0, tempBoard.Count);

                    if (tempBoard[randomBoard] != null)
                    {
                        //주변물찾기 물방향넣어서 만들기
                        int waterDir = 0;
                        Board tempWaterBoard = PosHelper.GetBoardSreeen(tempBoard[randomBoard].indexX, tempBoard[randomBoard].indexY, 0, 1);
                        if (tempWaterBoard.IsNotDisturbMoveWater(BlockDirection.UP) == false)
                        {   //물 생성 위치에 방해블럭이 있는지 검사
                            waterDir = 0;
                        }
                        else
                        {
                            if (tempWaterBoard != null && tempWaterBoard.DecoOnBoard.Count != 0)
                            {
                                foreach (DecoBase tempWater in tempWaterBoard.DecoOnBoard)
                                {
                                    if (tempWater is RandomBox)
                                    {
                                        waterDir = 0;
                                        break;
                                    }

                                    if (tempWater is Water)
                                    {
                                        waterDir = 3;
                                    }
                                }
                            }
                        }

                        tempWaterBoard = PosHelper.GetBoardSreeen(tempBoard[randomBoard].indexX, tempBoard[randomBoard].indexY, 1, 0);
                        if (waterDir == 0 && tempWaterBoard != null && tempWaterBoard.DecoOnBoard.Count != 0)
                        { 
                            if (tempWaterBoard.IsNotDisturbMoveWater(BlockDirection.LEFT) == false)
                            {   //물 생성 위치에 방해블럭이 있는지 검사
                                waterDir = 0;
                            }
                            else
                            {
                                foreach (DecoBase tempWater in tempWaterBoard.DecoOnBoard)
                                {
                                    if (tempWater is RandomBox)
                                    {
                                        waterDir = 0;
                                        break;
                                    }

                                    if (tempWater is Water)
                                    {
                                        waterDir = 4;
                                    }
                                }
                            }
                        }
                        
                        tempWaterBoard = PosHelper.GetBoardSreeen(tempBoard[randomBoard].indexX, tempBoard[randomBoard].indexY, -1, 0);
                        if (waterDir == 0 && tempWaterBoard != null && tempWaterBoard.DecoOnBoard.Count != 0)
                        {
                            if (tempWaterBoard.IsNotDisturbMoveWater(BlockDirection.RIGHT) == false)
                            {   //물 생성 위치에 방해블럭이 있는지 검사
                                waterDir = 0;
                            }
                            else
                            {
                                foreach (DecoBase tempWater in tempWaterBoard.DecoOnBoard)
                                {
                                    if (tempWater is RandomBox)
                                    {
                                        waterDir = 0;
                                        break;
                                    }

                                    if (tempWater is Water)
                                    {
                                        waterDir = 2;
                                    }
                                }
                            }
                        }

                        tempWaterBoard = PosHelper.GetBoardSreeen(tempBoard[randomBoard].indexX, tempBoard[randomBoard].indexY, 0, -1);
                        if (waterDir == 0 && tempWaterBoard != null && tempWaterBoard.DecoOnBoard.Count != 0)
                        {
                            if (tempWaterBoard.IsNotDisturbMoveWater(BlockDirection.DOWN) == false)
                            {   //물 생성 위치에 방해블럭이 있는지 검사
                                waterDir = 0;
                            }
                            else
                            {
                                foreach (DecoBase tempWater in tempWaterBoard.DecoOnBoard)
                                {
                                    if (tempWater is RandomBox)
                                    {
                                        waterDir = 0;
                                        break;
                                    }

                                    if (tempWater is Water)
                                    {
                                        waterDir = 1;
                                    }
                                }
                            }
                        }

                        DecoInfo decoWater = new DecoInfo();
                        decoWater.BoardType = (int)BoardDecoType.WATER;
                        decoWater.index = waterDir;

                        DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(tempBoard[randomBoard], tempBoard[randomBoard].indexX, tempBoard[randomBoard].indexY, decoWater);

                        if (tempBoard[randomBoard].Block != null && tempBoard[randomBoard].Block.IsCanRemoveWater())
                        {
                            tempBoard[randomBoard].Block.RemoveCollectEventBlock();
                            tempBoard[randomBoard].Block.PangDestroyBoardData();
                        }
                        else
                        {
                            tempBoard[randomBoard].Block.RemoveLinkerNoReset();
                        }

                        ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);
                    }
                }
            }
            GetWater = false;
        }
        #endregion

        #region 용암 증식 및 맵 이동
        bool isMoveMap = false;
        //용암모드 보드올리기, 용암증식추가
        //용암 카운트가 다되었는지확인
        if (GameManager.gameMode == GameMode.LAVA)
        {
            //화면에 남아있는 목표가 없을 경우, 빠른 진행을 위해 턴이 되지않았어도 용암을 위로 올려줌
            if (IsExistLavaModeTargetAtScreen() == false)
            {
                //LavaLevelCount
                if (GameManager.MOVE_Y > 0)
                {
                    LavaLevelCount--;
                    movePanelPause = true;
                    LavaMoveCount = 1;
                    targetY = (GameManager.MOVE_Y - LavaMoveCount) * 78;
                    yield return null;

                    while (LavaMoveCount > 0)
                    {
                        isMoveMap = true;
                        MoveLavaGround();
                        yield return null;
                    }
                }
            }

            if (iceCount > 0)
            {
                iceCount--;
                GameUIManager.instance.DisCountIce(iceCount);

                if (iceCount <= 0)
                {
                    ResetIceApple();
                }
            }
            else
            {
                List<Board> tempBoard = new List<Board>();
                int makeCount = stageInfo.digCount;
                bool digTargetGameOver = false;

                while (makeCount > 0)
                {
                    GetNormalBoardForLava(tempBoard);
                    yield return null;

                    int randLava = GameManager.instance.GetIngameRandom(0, tempBoard.Count);

                    //여기서 게임오버처리
                    if (tempBoard.Count > 0)
                    {
                        Board tempLavaBoard = PosHelper.GetBoardSreeen(tempBoard[randLava].indexX, tempBoard[randLava].indexY);
                        if (tempLavaBoard != null && tempLavaBoard.Block != null && tempLavaBoard.Block.IsDigTarget())
                        {
                            digTargetGameOver = true;
                        }
                        if (tempLavaBoard != null && tempLavaBoard.BoardOnCrack.Count > 0)
                        {
                            digTargetGameOver = true;
                        }
                        if (IsGameOverRavaMode_ByFlowerInk(tempLavaBoard))
                        {
                            digTargetGameOver = true;
                        }
                        if (IsGameOverRavaMode_ByBattery(tempLavaBoard))
                        {
                            digTargetGameOver = true;
                        }

                        // 다이나마이트 게임오버
                        if (tempLavaBoard.Block != null && tempLavaBoard.Block is BlockDynamite)
                        {
                            BlockDynamite tempDynamite = tempLavaBoard.Block as BlockDynamite;
                            if (tempDynamite != null && tempDynamite.DynamiteCount <= 0)
                            {
                                digTargetGameOver = true;
                                makeCount = 0;
                                break;
                            }
                        }

                        Lava makeLava = BoardDecoMaker.instance.MakeLava(tempBoard[randLava], tempBoard[randLava].indexX, tempBoard[randLava].indexY);
                        if (digTargetGameOver) makeLava.gameOverLava = true;
                        makeLava.MakeLava();
                        yield return null;

                        makeCount--;
                        GameManager.instance.moveCount--;
                        GameUIManager.instance.RefreshMove();
                    }
                    else
                    {
                        digTargetGameOver = true;
                    }


                    if (digTargetGameOver)
                    {
                        makeCount = 0;
                        break;
                    }
                    else
                    {
                        if (tempBoard[randLava].Block != null)
                        {
                            tempBoard[randLava].Block.DestroyBlockData();
                            tempBoard[randLava].Block.RemoveCollectEventBlock();
                            tempBoard[randLava].Block.PangDestroyBoardData();
                        }
                    }


                    waitTime = 0f;
                    while (waitTime < 0.05f)
                    {
                        waitTime += Global.deltaTimePuzzle;
                        yield return null;
                    }

                    LavaMove();
                    yield return null;

                    if (LavaMoveCount > 0) ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);


                    while (LavaMoveCount > 0)
                    {
                        isMoveMap = true;
                        MoveLavaGround();
                        yield return null;
                    }

                    if (GameManager.instance.state == GameState.GAMEOVER)
                    {
                        makeCount = 0;
                        break;
                    }
                }

                if (digTargetGameOver)
                {
                    waitTime = 0f;
                    while (waitTime < 0.5f)
                    {
                        waitTime += Global.deltaTimePuzzle;
                        yield return null;
                    }
                    GameManager.instance.StageFail();
                    yield return null;
                }

                if (BlockBase._listBlock.Count <= 1)
                {
                    if (EditManager.instance != null)
                        EditManager.instance.ResetTool();
                    else
                        GameManager.instance.StageFail();

                }
            }
        }
        movePanelPause = false;
        yield return null;

        while (DecoBase._waitCount > 0)
        {
            yield return null;
        }
        #endregion

        //맵 이동 후 하트 기믹 움직이기
        if (isMoveMap == true && listHeart.Count > 0)
        {
            yield return CoMoveHeart();
            yield return new WaitUntil(() => checkBlockWait());
        }

        //자동매치 검사
        yield return CoAutoMatchEvent();
        yield return new WaitUntil(() => checkBlockWait());

        //목표 모두 모았는지 검사
        if (GameManager.instance.checkAllRemoveTarget())
        {
            state = BlockManagrState.WAIT;
            yield break;
        }

        ManagerBlock.instance.blockMove = true;
        ManagerBlock.instance.creatBlock = true;

        while (!checkBlockWait())
        {
            yield return null;
        }

        //생성기 이미지 갱신
        BlockGeneratorImageCheck();

        //이벤트끝
        state = BlockManagrState.WAIT;
        GameManager.instance.touchCount--;

        yield return null;

        if (GameManager.gameMode == GameMode.LAVA)
        {
            waitTime = 0f;
            while (waitTime < 0.6f)
            {
                waitTime += Global.deltaTimePuzzle;
                yield return null;
            }

            if (iceCount == 0)
            {
                GameManager.instance.moveCount = stageInfo.digCount;
                GameUIManager.instance.RefreshMove();
            }
        }
    }

    private bool IsGameOverRavaMode_ByFlowerInk(Board tempLavaBoard)
    {
        if (listFlowerInk.Count == 0)
            return false;

        //용암에 먹히지 않은 자동 데코가 있는지 검사.
        Dictionary<BlockColorType, bool> dicRemainAutoDeco = new Dictionary<BlockColorType, bool>();
        for (int i = 0; i < listFlowerInk.Count; i++)
        {
            BlockColorType colorType = listFlowerInk[i].colorType;
            if (dicRemainAutoDeco.ContainsKey(colorType) == false)
            {
                dicRemainAutoDeco.Add(colorType, false);
            }

            if (dicRemainAutoDeco[colorType] == true)
                continue;

            Board checkBoard = listFlowerInk[i].board;
            if ((checkBoard != tempLavaBoard) && checkBoard.lava == null)
            {
                dicRemainAutoDeco[colorType] = true;
            }
        }

        //남아있는 데코가 없을 경우 게임오버
        var enumerator = dicRemainAutoDeco.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value == false)
                return true;
        }
        return false;
    }

    private bool IsGameOverRavaMode_ByBattery(Board tempLavaBoard)
    {
        return IsExitHeartWayInBoard(tempLavaBoard);
    }

    private bool IsExitHeartWayInBoard(Board tempBoard)
    {
        if (listHeart.Count == 0)
            return false;

        foreach (var item in listHeart)
        {
            //보드에 배터리 길이 있는지 확인
            List<Vector2Int> listHeartWayPos = item.GetListHeartWayPos();
            int fIndex = listHeartWayPos.FindIndex(x => x.x == tempBoard.indexX && x.y == tempBoard.indexY);
            if (fIndex != -1)
                return true;
        }
        return false;
    }

    public List<BlockBase> MakeAdventureBlock()
    {
        List<BlockBase> makeBlockList = new List<BlockBase>();
        List<Board> emptyBoardList = new List<Board>();

        for (int j = GameManager.MIN_X; j < GameManager.MAX_X; j++)
        {
            for (int i = GameManager.MIN_Y; i < GameManager.MAX_Y; i++)
            {
                Board board = PosHelper.GetBoard(j, i);
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                if (board != null && board.IsActiveBoard && block == null)
                {
                    block = BlockMaker.instance.MakeBlockByStartBoard(j, i);
                    board.Block = block;
                    block.Hide(0f);
                    makeBlockList.Add(block);

                    block.transform.localPosition = PosHelper.GetPosByIndex(j, i);
                }
            }
        }

        while (true)
        {
            for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
                for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
                {
                    BlockBase block = PosHelper.GetBlock(x, y);
                    if (block != null)
                        block.isCheckBombMakable = false;
                }
            for (int i = 0; i < makeBlockList.Count; i++)
            {
                if (BlockMatchManager.instance.CheckAdventureSameBlockCount(makeBlockList[i]) >= ADVENTURE_PANG_SAME_COLOR_COUNT)
                {
                    makeBlockList[i].colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap(makeBlockList[i].colorType);
                }
            }

            //생성되는지 확인
            bool checkBomb = true;
            for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
                for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
                {
                    BlockBase block = PosHelper.GetBlock(x, y);
                    if (block != null)
                        block.isCheckBombMakable = false;
                }

            for (int i = 0; i < makeBlockList.Count; i++)
            {
                if (BlockMatchManager.instance.CheckAdventureSameBlockCount(makeBlockList[i]) >= ADVENTURE_PANG_SAME_COLOR_COUNT)
                    checkBomb = false;
            }

            if (checkBomb)
            {
                //블럭좌표만들기
                //
                //Board[,] tempBoard = new Board[GameManager.MAX_X, GameManager.MAX_Y];
                for (int i = 0; i < makeBlockList.Count; i++)
                {
                    makeBlockList[i].transform.localPosition = PosHelper.GetPosByIndex(makeBlockList[i].indexX, makeBlockList[i].indexY - 1);
                    makeBlockList[i].MoveToTargetPos(PosHelper.GetPosByIndex(makeBlockList[i].indexX, makeBlockList[i].indexY));
                    makeBlockList[i].rTargetPos = PosHelper.GetPosByIndex(makeBlockList[i].indexX, makeBlockList[i].indexY);
                    makeBlockList[i].isMakeByStart = true;
                    makeBlockList[i].Hide(0.5f);

                    Board board = PosHelper.GetBoard(makeBlockList[i].indexX, makeBlockList[i].indexY);
                    if (board.IsStartBoard)
                        makeBlockList[i].mainSprite.customFill.blockRatio = 1f;

                    //startBoardList[i].Block.mainSprite.blockRatio = 1f

                    // block.MoveToTargetPos(PosHelper.GetPosByIndex(j, i));
                    //  block.rTargetPos = PosHelper.GetPosByIndex(j, i);
                    // block.isMakeByStart = true;
                }


                break;
            }

        }

        ManagerBlock.instance.creatBlock = true;
        //블럭 숨기고 리스트 만들기

        return makeBlockList;
    }

    public bool IsCanMoveSideBlock(int aInX, int aInY, int addX, int addY)
    {
        Board boardA = PosHelper.GetBoard(aInX, aInY);
        Board boardB = PosHelper.GetBoard(aInX + addX, aInY + addY);

        if (boardA == null || boardB == null)
            return true;

        if (boardA.BoardOnDisturbs.Count == 0 || boardB.BoardOnDisturbs.Count == 0)
            return true;

        for (int i = 0; i < boardA.BoardOnDisturbs.Count; i++)
        {
            for (int j = 0; j < boardB.BoardOnDisturbs.Count; j++)
            {
                if (boardA.BoardOnDisturbs[i] == boardB.BoardOnDisturbs[j] && boardA.BoardOnDisturbs[i].IsDisturbMove())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsCanLinkSideBLock(int aInX, int aInY, int addX, int addY)
    {
        Board boardA = PosHelper.GetBoard(aInX, aInY);
        Board boardB = PosHelper.GetBoard(aInX + addX, aInY + addY);

        for (int i = 0; i < boardA.BoardOnDisturbs.Count; i++)
        {
            for (int j = 0; j < boardB.BoardOnDisturbs.Count; j++)
            {
                if (boardA.BoardOnDisturbs[i] == boardB.BoardOnDisturbs[j] && !boardA.BoardOnDisturbs[i].IsLinkable())
                {
                    return false;
                }
            }
        }

        foreach (var deco in boardA.DecoOnBoard)
        {
            if (deco.IsWarpBlock()) return false;
        }

        foreach (var deco in boardB.DecoOnBoard)
        {
            if (deco.IsWarpBlock()) return false;
        }

        if (boardA == null || boardB == null)
            return true;

        if (boardA.BoardOnDisturbs.Count == 0 || boardB.BoardOnDisturbs.Count == 0)
            return true;

        return true;
    }

    public float waitTimer = 0;
    bool showAlarm = false;

    bool SearchMatchBlock()
    {
        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            return true;
        }

        if (BlockBase._listBlock.Count <= 1)
            return true;

        if (BlockBomb._liveCount != 0)
            return true;

        bool showMatchBlock = false;

        if (state == BlockManagrState.WAIT)
        {
            if (waitTimer > 4f)
            {
                waitTimer = 0f;
                showMatchBlock = true;
                showAlarm = false;
            }

            if (GameItemManager.instance != null)
                showMatchBlock = false;

            if (ManagerTutorial._instance != null)
                showMatchBlock = false;
        }

        CheckAlarm_ShakingBlock();

        List<BlockBase> listMatchableBlock = new List<BlockBase>();
        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenY; x++)
            {
                Board tempBoard = PosHelper.GetBoardSreeen(x, y);
                BlockBase block = PosHelper.GetBlockScreen(x, y);

                if (block != null && block.IsSelectable() && tempBoard != null && tempBoard.HasDecoHideBlock() == false && tempBoard.HasDecoCoverBlock() == false && (block.blockDeco == null || block.blockDeco.IsInterruptBlockSelect() == false) && block.IsCanLink() == true)
                {
                    if (block.IsBombBlock())
                    {
                        if (showMatchBlock == false)
                            return true;

                        listMatchableBlock.Add(block);
                        continue;
                    }
                    if (CheckMatchableBlock(x, y, 1, 0, block))
                    {
                        if (showMatchBlock == false) return true;
                        listMatchableBlock.Add(block);
                        continue;
                    }
                    if (CheckMatchableBlock(x, y, -1, 0, block))
                    {
                        if (showMatchBlock == false) return true;
                        listMatchableBlock.Add(block);
                        continue;
                    }
                    if (CheckMatchableBlock(x, y, 0, 1, block))
                    {
                        if (showMatchBlock == false) return true;
                        listMatchableBlock.Add(block);
                        continue;
                    }
                    if (CheckMatchableBlock(x, y, 0, -1, block))
                    {
                        if (showMatchBlock == false) return true;
                        listMatchableBlock.Add(block);
                        continue;
                    }
                }
            }
        }

        if (showMatchBlock == false) return false;

        if (listMatchableBlock.Count > 0)
        {
            int selectShowBlock = GameManager.instance.GetIngameRandom(0, listMatchableBlock.Count);
            if (listMatchableBlock[selectShowBlock] != null && listMatchableBlock[selectShowBlock].IsNormalBlock())
            {
                listShowBlock.Clear();
                listShowBlock.Add(listMatchableBlock[selectShowBlock]);
                GetNearTempColor(listMatchableBlock[selectShowBlock]);
                foreach (var temp in listShowBlock) temp.ShowMatchBlock();
            }
            else if (listMatchableBlock[selectShowBlock] != null && listMatchableBlock[selectShowBlock].IsBombBlock())
            {
                listShowBlock.Clear();
                listShowBlock.Add(listMatchableBlock[selectShowBlock]);
                GetNearTempBomb(listMatchableBlock[selectShowBlock]);
                foreach (var temp in listShowBlock) temp.ShowMatchBlock();
            }
            return true;
        }

        return false;
    }

    //남은 목표 알람 //블럭 흔들기
    void CheckAlarm_ShakingBlock()
    {
        if (state == BlockManagrState.WAIT
            && waitTimer > 2.5f
            && waitTimer < 3f
            && showAlarm == false)
        {
            bool isCarpetAlarm = IsCanAlarm_Carpet();
            bool isCrackAlarm = IsCanAlarm_Crack();

            if (isCarpetAlarm || isCrackAlarm)
            {
                showAlarm = true;

                for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
                {
                    for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
                    {
                        Board checkBoard = PosHelper.GetBoardSreeen(x, y);

                        if (checkBoard != null
                            && PosHelper.GetBlockScreen(x, y) != null
                            && checkBoard.IsActiveBoard == true
                            && ((isCarpetAlarm && checkBoard.IsExistCarpet() == false)
                            || (isCrackAlarm && checkBoard.IsExistCrack() == true))
                            )
                        {
                            PosHelper.GetBlockScreen(x, y).ShakingBlock();
                        }
                    }
                }
            }
        }

    }

    bool IsCanAlarm_Carpet()
    {
        if (isCarpetStage == false)
            return false;

        //남은 카펫의 목표수가 10개 이하일 때 알람 표시.
        CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(TARGET_TYPE.CARPET);
        if (collectTargetCount != null && collectTargetCount.collectCount > 0
            && (collectTargetCount.collectCount > collectTargetCount.pangCount)
            && (collectTargetCount.collectCount - collectTargetCount.pangCount) <= 10)
        {
            return true;
        }
        return false;
    }

    bool IsCanAlarm_Crack()
    {
        //단계 석판 제외
        if (IsStageTarget(TARGET_TYPE.CRACK) == false)
            return false;

        //남은 석판의 목표수가 10개 이하일 때 알람 표시.
        CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(TARGET_TYPE.CRACK);
        if (collectTargetCount != null && collectTargetCount.collectCount > 0
            && (collectTargetCount.collectCount > collectTargetCount.pangCount)
            && (collectTargetCount.collectCount - collectTargetCount.pangCount) <= 10)
        {
            return true;
        }

        return false;
    }

    bool CheckMatchableBlock(int x, int y, int a, int b, BlockBase block)
    {
        BlockBase blockA = PosHelper.GetBlockScreen(x, y, a, b);
        Board tempBoardA = PosHelper.GetBoardSreeen(x, y, a, b);
        if (blockA != null && blockA.colorType == block.colorType && IsCanLinkSideBLock(x, y, a, b) && (blockA.blockDeco == null || blockA.blockDeco.IsInterruptBlockSelect() == false) && blockA.IsCanLink() == true)
        {
            if (block.IsNormalBlock() && blockA.bombType == BlockBombType.RAINBOW)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    List<BlockBase> listShowBlock = new List<BlockBase>();

    void GetNearTempBomb(BlockBase block)//, List<BlockBase> tempCheckBlocks)
    {
        block.isCheckBombMakable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT//
            && !listShowBlock.Contains(getNearBlock)
            && getNearBlock.IsBombBlock())
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && !listShowBlock.Contains(getNearBlock)
            && getNearBlock.IsBombBlock())
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && !listShowBlock.Contains(getNearBlock)
            && getNearBlock.IsBombBlock())
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && !listShowBlock.Contains(getNearBlock)
            && getNearBlock.IsBombBlock())
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        foreach (var block1 in tempCheckBlockList)
        {
            //if (!block1.isCheckBombMakable)
            {
                GetNearTempBomb(block1);//, tempCheckBlocks);
            }
        }
    }



    public void GetNearTempColor(BlockBase block)//, List<BlockBase> tempCheckBlocks)
    {
        //block.isCheckBombMakable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT 
            && listShowBlock.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && listShowBlock.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && listShowBlock.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && getNearBlock.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT
            && listShowBlock.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                listShowBlock.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        foreach (var block1 in tempCheckBlockList)
        {
            //if (!block1.isCheckBombMakable)
            {
                GetNearTempColor(block1);//, listShowBlock);
            }
        }
    }

    public void SortListBlock()
    {
        stageInfo.ListBlock.Sort(delegate (BlockInfo a, BlockInfo b)
        {
            if (a.inX < b.inX)
                return -1;
            else if (a.inX > b.inX)
                return 1;
            else
            {
                if (a.inY < b.inY)
                    return -1;
                else if (a.inY > b.inY)
                    return 1;
                else
                    return 0;
            }
        });
    }

    public BlockInfo GetBlockInfo(int inX, int inY)
    {
        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            foreach (BlockInfo blockInfo in stageInfo.ListBlock)
            {
                if (blockInfo.inX == inX && blockInfo.inY == inY)
                {
                    return blockInfo;
                }
            }
        }
        return null;
    }

    public BlockInfo GetBlockInfoScreen(int inX, int inY)
    {
        if (inX >= GameManager.MinScreenX && inX < GameManager.MaxScreenX && inY >= GameManager.MinScreenY && inY < GameManager.MaxScreenY)
        {
            foreach (BlockInfo blockInfo in stageInfo.ListBlock)
            {
                if (blockInfo.inX == inX && blockInfo.inY == inY)
                {
                    return blockInfo;
                }
            }
        }
        return null;
    }

    //블럭움직임 멈추기
    bool mBlockMove = true;
    public bool blockMove
    {
        get { return mBlockMove; }
        set { mBlockMove = value; }
    }

    //출발에서 블럭생성
    bool mCreatBlock = true;
    public bool creatBlock
    {
        get { return mCreatBlock; }
        set { mCreatBlock = value; }
    }


    public bool movePanelPause = false;
    const int PANEL_MOVE_SPEED = 120;
    const int PANEL_RANK_MOVE_SPEED = 180;
    public float targetY = 0;

    #region 용암모드
    public bool lavaModeGameOver = false;
    public int LavaMoveCount = 0;
    public int LavaLevelCount = 0;
    public int makeLaveCount = 0;   //게임 중 생성된 용암 카운트

    //화면에 목표 체크
    bool IsExistLavaModeTargetAtScreen()
    {
        for (int i = GameManager.MIN_Y; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board tempBoard = PosHelper.GetBoard(j, i);
                if (tempBoard == null)
                    continue;

                for (int k = 0; k < tempBoard.DecoOnBoard.Count; k++)
                {
                    if (tempBoard.DecoOnBoard[k].IsTarget_LavaMode())
                    {
                        return true;
                    }
                }

                if (tempBoard.Block != null)
                {
                    if (tempBoard.Block.IsTarget_LavaMode())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //물이 증식 가능한지
    bool CheckNormalBoard(Board tempBoard)
    {
        if (tempBoard != null &&
            tempBoard.IsActiveBoard &&
            tempBoard.lava == null &&
            tempBoard.HasDecoHideBlock() == false && 
            tempBoard.HasDecoCoverBlock() == false)
        {
            if (tempBoard.Block == null) return false;
            if (tempBoard.Block.IsBlockType() == false) return false;
            if (tempBoard.Block.blockDeco != null && tempBoard.Block.blockDeco.IsInterruptBlockSelect()) return false;

            //흙이고 보석타입이고 라이프카운트가 1이하일때
            if (tempBoard.Block is BlockGround)
            {
                BlockGround tempBlockGround = tempBoard.Block as BlockGround;
                if (tempBlockGround.groundType == GROUND_TYPE.JEWEL && tempBlockGround.lifeCount <= 1) return true;
                else return false;
            }

            if (tempBoard.Block is BlockEventGround && tempBoard.Block.lifeCount > 0)
                return false;

            if (tempBoard.Block is BlockHeart || tempBoard.Block is BlockHeartHome)
                return false;

            return true;
        }
        return false;
    }

    void GetNormalBoardForWater(List<Board> tempBoard)
    {
        tempBoard.Clear();

        foreach (var temp in listWater)      //석판X //목표X // 폭탄X // 풀,돌 방해블럭 X
        {
            bool hasInterruptDeco = temp.board.DecoOnBoard.FindIndex(x => x.IsInterruptBlockEvent() == true) > -1;
            if (temp != null && hasInterruptDeco == false)
            {
                Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, 1);
                if (CheckNormalBoard(nearBoard) && tempBoard.Contains(nearBoard) == false && nearBoard.IsNotDisturbMoveWater(BlockDirection.UP))
                    tempBoard.Add(nearBoard);

                nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, -1);
                if (CheckNormalBoard(nearBoard) && tempBoard.Contains(nearBoard) == false && nearBoard.IsNotDisturbMoveWater(BlockDirection.DOWN))
                    tempBoard.Add(nearBoard);

                nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 1, 0);
                if (CheckNormalBoard(nearBoard) && tempBoard.Contains(nearBoard) == false && nearBoard.IsNotDisturbMoveWater(BlockDirection.LEFT))
                    tempBoard.Add(nearBoard);

                nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, -1, 0);
                if (CheckNormalBoard(nearBoard) && tempBoard.Contains(nearBoard) == false && nearBoard.IsNotDisturbMoveWater(BlockDirection.RIGHT))
                    tempBoard.Add(nearBoard);
            }
        }
    }

    //주변에 빈보드 찾기 //노말 -> 폭탄 -> 목표 -> 석판(용암이 증식하는 우선순위 검사 함수)
    void GetNormalBoardForLava(List<Board> tempBoard)
    {
        tempBoard.Clear();

        foreach (var temp in listLava)      //석판X //목표X // 폭탄X
        {
            if (temp != null)
            {
                Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, -1);
                if (nearBoard != null &&
                    nearBoard.IsActiveBoard &&
                    nearBoard.lava == null &&
                    nearBoard.IsExistLavaModeTargetDeco() == false &&
                    IsExitHeartWayInBoard(nearBoard) == false &&
                    (nearBoard.Block == null || (nearBoard.Block.IsDigTarget() == false && nearBoard.Block.IsNormalBlock())) &&
                    tempBoard.Contains(nearBoard) == false &&
                    temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 1 - LavaLevelCount)
                {
                    tempBoard.Add(nearBoard);
                }

                nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 1, 0);
                if (nearBoard != null &&
                    nearBoard.IsActiveBoard &&
                    nearBoard.lava == null &&
                    nearBoard.IsExistLavaModeTargetDeco() == false &&
                    IsExitHeartWayInBoard(nearBoard) == false &&
                    (nearBoard.Block == null || (nearBoard.Block.IsDigTarget() == false && nearBoard.Block.IsNormalBlock())) &&
                    tempBoard.Contains(nearBoard) == false &&
                    temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                    PosHelper.GetBoard(temp.inX, temp.inY, 1, 1).IsActiveBoard == false)
                {
                    tempBoard.Add(nearBoard);
                }

                nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, -1, 0);
                if (nearBoard != null &&
                    nearBoard.IsActiveBoard &&
                    nearBoard.lava == null &&
                    nearBoard.IsExistLavaModeTargetDeco() == false &&
                    IsExitHeartWayInBoard(nearBoard) == false &&
                    (nearBoard.Block == null || (nearBoard.Block.IsDigTarget() == false && nearBoard.Block.IsNormalBlock())) &&
                    tempBoard.Contains(nearBoard) == false &&
                    temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                    PosHelper.GetBoard(temp.inX, temp.inY, -1, 1).IsActiveBoard == false)
                {
                    tempBoard.Add(nearBoard);
                }
            }
        }

        //라인빈곳에 있는지 확인  //같은라인인데 저기 있지않을때 //
        if (tempBoard.Count == 0)
        {
            foreach (var temp in listLava)      //석판X //목표X // 폭탄O
            {
                if (temp != null)
                {
                    Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, -1);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard &&
                        nearBoard.lava == null &&
                        nearBoard.IsExistLavaModeTargetDeco() == false &&
                        IsExitHeartWayInBoard(nearBoard) == false &&
                        (nearBoard.Block == null || nearBoard.Block.IsDigTarget() == false) &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 1 - LavaLevelCount)
                    {
                        tempBoard.Add(nearBoard);
                    }

                    nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 1, 0);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard &&
                        nearBoard.lava == null &&
                        nearBoard.IsExistLavaModeTargetDeco() == false &&
                        IsExitHeartWayInBoard(nearBoard) == false &&
                       (nearBoard.Block == null || nearBoard.Block.IsDigTarget() == false) &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                        PosHelper.GetBoard(temp.inX, temp.inY, 1, 1).IsActiveBoard == false)
                    {
                        tempBoard.Add(nearBoard);
                    }

                    nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, -1, 0);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard &&
                        nearBoard.lava == null &&
                        nearBoard.IsExistLavaModeTargetDeco() == false &&
                        IsExitHeartWayInBoard(nearBoard) == false &&
                       (nearBoard.Block == null || nearBoard.Block.IsDigTarget() == false) &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                        PosHelper.GetBoard(temp.inX, temp.inY, -1, 1).IsActiveBoard == false)
                    {
                        tempBoard.Add(nearBoard);
                    }
                }
            }
        }


        if (tempBoard.Count == 0)
        {
            foreach (var temp in listLava)
            {

                if (temp != null)
                {
                    Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, -1);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard && nearBoard.lava == null &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 1 - LavaLevelCount)
                    {
                        tempBoard.Add(nearBoard);
                    }

                    nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 1, 0);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard && nearBoard.lava == null &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                        PosHelper.GetBoard(temp.inX, temp.inY, 1, 1).IsActiveBoard == false)
                    {
                        tempBoard.Add(nearBoard);
                    }

                    nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, -1, 0);
                    if (nearBoard != null &&
                        nearBoard.IsActiveBoard &&
                        nearBoard.lava == null &&
                        tempBoard.Contains(nearBoard) == false &&
                        temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount &&
                        PosHelper.GetBoard(temp.inX, temp.inY, -1, 1).IsActiveBoard == false)
                    {
                        tempBoard.Add(nearBoard);
                    }
                }


            }
        }
    }

    public bool LavaMove()
    {
        if (GameManager.instance.state != GameState.PLAY) return false;

        //지금라인에 용암이 하나도 없는지 체크
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            Board tempBoard = PosHelper.GetBoard(i, GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount);
            if (tempBoard != null && tempBoard.IsActiveBoard)
            {
                if (tempBoard.lava == null)
                {
                    return false;
                }
            }
        }

        if (GameManager.MOVE_Y == 0)
        {
            LavaLevelCount++;
            return false;
        }

        movePanelPause = true;
        LavaMoveCount = 1;
        targetY = (GameManager.MOVE_Y - LavaMoveCount) * 78;
        return true;
    }

    bool MoveLavaGround()//float moveTime)
    {
        if (LavaMoveCount == 0) return true;

        if (targetY > GameUIManager.instance.groundAnchor.transform.localPosition.y)//Mathf.Abs(targetY - GameUIManager.instance.groundAnchor.transform.localPosition.y) < 2f)
        {
            GameManager.MOVE_Y -= LavaMoveCount;
            LavaMoveCount = 0;

            GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);

            BlockMatchManager.instance.DistroyAllLinker();
            BlockMatchManager.instance.SetBlockLink();
            return true;
        }
        else
        {
            noizeGroundTimer += Global.deltaTimePuzzle;
            float result = Mathf.Sin(2.0f * 3.14159f * noizeGroundTimer * 7f) * 2f;
            float valueX = result;

            GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(valueX, GameUIManager.instance.groundAnchor.transform.localPosition.y - Global.deltaTimePuzzle * PANEL_MOVE_SPEED, 0);
            SetLavaPanel(GameUIManager.instance.groundAnchor.transform.localPosition.y);

            float digGauget = GameUIManager.instance.groundAnchor.transform.localPosition.y / ((float)stageInfo.Extend_Y * 78);

            return false;
        }
    }

    public void ResetDynamite()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                if (block != null)
                {
                    if (block is BlockDynamite && (block.blockDeco == null || block.blockDeco.IsInterruptBlockSelect() == false))
                    {
                        BlockDynamite block_Dynamite = block as BlockDynamite;
                        block_Dynamite.DynamiteCount += 5;
                        block_Dynamite.RecoverDynamite();
                        block.UpdateSpriteByBlockType();
                    }
                }
            }
        }
    }

    public void RecoverLavaMode(int continueCount)    //컨티뉴
    {
        isPlayContinueAction = true;
        int RemoveCount = stageInfo.digCount * 3 - 1;

        //현재 생성된 용암 수보다 제거될 수가 더 많다면, 제거될 수를 생성된 수랑 동일하게 설정
        if (RemoveCount > makeLaveCount)
            RemoveCount = makeLaveCount;

        List<Lava> tempLava = new List<Lava>();

        foreach (var lava in listLava)
        {
            if (lava != null && lava.gameOverLava) tempLava.Add(lava);
        }

        foreach (var lava in tempLava)
        {
            lava.RemoveLava();
            listLava.Remove(lava);
            makeLaveCount--;
        }

        List<Lava> tempBoard = new List<Lava>();
        tempBoard.Clear();

        foreach (var temp in listLava)
        {
            if (temp != null)
            {
                Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, 0);
                if (nearBoard != null && nearBoard.IsActiveBoard && nearBoard.lava != null && temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount)  //&& nearBoard.lava != null
                {
                    tempBoard.Add(temp);
                }
            }

        }

        while (RemoveCount > 0 && tempBoard.Count > 0)
        {
            int lastIndex = tempBoard.Count - 1;

            if (tempBoard[lastIndex] != null)
            {
                listLava.Remove(tempBoard[lastIndex]);
                tempBoard[lastIndex].RemoveLava();
                tempBoard.RemoveAt(lastIndex);
                makeLaveCount--;
                RemoveCount--;
            }

        }

        while (RemoveCount > 0)
        {
            LavaLevelCount--;
            foreach (var temp in listLava)
            {
                if (temp != null)
                {
                    Board nearBoard = PosHelper.GetBoardSreeen(temp.inX, temp.inY, 0, 0);
                    if (nearBoard != null && nearBoard.IsActiveBoard && nearBoard.lava != null && temp.inY == GameManager.Y_OFFSET + GameManager.STAGE_SIZE_Y + GameManager.MOVE_Y - 2 - LavaLevelCount)   // nearBoard.lava != null && 
                    {
                        tempBoard.Add(temp);
                    }
                }

            }

            while (RemoveCount > 0 && tempBoard.Count > 0)
            {
                int lastIndex = tempBoard.Count - 1;
                tempBoard[lastIndex].RemoveLava();
                tempBoard.RemoveAt(lastIndex);
                makeLaveCount--;
                RemoveCount--;
            }
        }

        foreach (var tempCrack in listCrack)
        {
            tempCrack.OnLavaWarring(false);
        }

        foreach (var checkLava in listCrack)
        {
            Board tempUpBoard = PosHelper.GetBoard(checkLava.inX, checkLava.inY, 0, 1);
            if (tempUpBoard != null && tempUpBoard.lava != null)
            {
                checkLava.OnLavaWarring();
            }
        }

        StartCoroutine(CoContinue(continueCount));
    }

    IEnumerator CoContinue(int continueCount)
    {
        if (continueCount > 1)
        {
            yield return new WaitForSeconds(0.6f);

            //컨티뉴 횟수에 따른 폭탄 생성
            bool isMakeBomb = false;
            if (continueCount == 2 || continueCount >= 4)
            {   //세로, 가로 폭탄 생성
                BlockBase lineBlock = PosHelper.GetRandomBlock();
                if (lineBlock != null)
                {
                    int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                    lineBlock.bombType = (randomLine == 0) ? BlockBombType.LINE_V : BlockBombType.LINE_H;
                    lineBlock.Destroylinker();
                    lineBlock.JumpBlock();
                    InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                    isMakeBomb = true;
                }
            }

            if (continueCount >= 3)
            {   //둥근 폭탄 생성
                BlockBase lineBlock = PosHelper.GetRandomBlock();
                if (lineBlock != null)
                {
                    lineBlock.bombType = BlockBombType.BOMB;
                    lineBlock.Destroylinker();
                    lineBlock.JumpBlock();
                    InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                    isMakeBomb = true;
                }
            }

            //사운드 재생
            if (isMakeBomb == true)
                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
        }

        yield return new WaitForSeconds(0.6f);
        GameManager.instance.state = GameState.PLAY;
        BlockMatchManager.instance.SetBlockLink(true);

        //자동 매치 연출 재생
        if (IsCanAutoMatch_FlowerInk())
        {
            yield return CoMatchEvent();
        }

        //컨티뉴 연출 종료
        isPlayContinueAction = false;
    }

    IEnumerator DownLava(Vector3 targetY)
    {
        float timer = 0f;
        while (true)
        {
            timer += Global.deltaTimePuzzle * 4f;
            GameUIManager.instance.groundAnchor.transform.localPosition = Vector3.Lerp(GameUIManager.instance.groundAnchor.transform.localPosition, targetY, timer);
            SetLavaPanel(GameUIManager.instance.groundAnchor.transform.localPosition.y);
            if (timer >= 1f)
                break;

            yield return null;
        }
    }


    #endregion   


    //컨티뉴에서 사용
    public bool CheckDynamiteInScreen()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                if (block != null)
                {
                    if (block is BlockDynamite && (block.blockDeco == null || block.blockDeco.IsInterruptBlockSelect() == false))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator CoEvent(float waitTime, UnityAction action)
    {
        float timer = 0;
        while (timer < waitTime)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        action();
        yield return null;
    }


    #region 땅파기모드    
    int digCount = 0;
    public void SetDigCountMove(int tempCount)
    {
        if (GameManager.instance.state != GameState.PLAY) return;

        movePanelPause = true;
        digCount = tempCount;
        targetY = (GameManager.MOVE_Y + digCount) * 78;
    }


    void SetGameMode(GameMode gameMode)
    {
        if (GameManager.instance.state == GameState.EDIT)
        {
            GameUIManager.instance.block_Panel.clipping = UIDrawCall.Clipping.None;
            GameUIManager.instance.DigModeBG.SetActive(false);
            GameUIManager.instance.DigModeBG1.SetActive(false);
            GameUIManager.instance.DigModeBlockBG.SetActive(false);
        }
        else if (gameMode == GameMode.DIG)
        {
            GameUIManager.instance.block_Panel.clipping = UIDrawCall.Clipping.SoftClip;
            GameUIManager.instance.block_Panel.clipOffset = new Vector2(0, -33);

            movePanelPause = false;
            lavaModeGameOver = false;
            digCount = 0;
            targetY = 0;

            GameManager.MOVE_Y = 0;

            SetPanel(0);
            GameUIManager.instance.DigModeBG.SetActive(true);
            GameUIManager.instance.DigModeBG1.SetActive(true);
            GameUIManager.instance.DigModeBlockBG.SetActive(true);
        }
        else if (gameMode == GameMode.LAVA)
        {
            GameUIManager.instance.block_Panel.clipping = UIDrawCall.Clipping.SoftClip;
            GameUIManager.instance.block_Panel.clipOffset = new Vector2(0, -33);

            movePanelPause = false;
            lavaModeGameOver = false;
            LavaMoveCount = 0;
            LavaLevelCount = 0;
            makeLaveCount = 0;

            GameManager.MOVE_Y = stageInfo.Extend_Y;
            targetY = GameManager.MOVE_Y * 78;

            GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);

            SetLavaPanel(targetY);
            GameUIManager.instance.DigModeBG.SetActive(true);
            GameUIManager.instance.DigModeBG1.SetActive(false);
            GameUIManager.instance.DigModeBlockBG.SetActive(false);
        }
        else if (gameMode == GameMode.ADVENTURE)
        {
            GameUIManager.instance.block_Panel.clipping = UIDrawCall.Clipping.SoftClip;
            GameUIManager.instance.block_Panel.clipOffset = new Vector2(0, -33);

            movePanelPause = false;
            lavaModeGameOver = false;
            digCount = 0;
            targetY = 0;

            GameManager.MOVE_Y = 0;

            SetPanel(0);
            //GameUIManager.instance.DigModeBG.SetActive(true);
            //GameUIManager.instance.DigModeBG1.SetActive(true);
            GameUIManager.instance.DigModeBlockBG.SetActive(true);
        }
        else
        {
            GameUIManager.instance.block_Panel.clipping = UIDrawCall.Clipping.None;

            GameUIManager.instance.DigModeBG.SetActive(false);
            GameUIManager.instance.DigModeBG1.SetActive(false);
            GameUIManager.instance.DigModeBlockBG.SetActive(false);
        }
    }

    bool checkDigMoving()
    {
        if (GameManager.gameMode != GameMode.DIG)
            return false;

        if (digCount == 0) return false;
        return true;
    }

    float noizeGroundTimer = 0;


    public bool MoveGround(float MoveSpeed = 1f)
    {
        if (digCount == 0) return true;

        if (targetY < GameUIManager.instance.groundAnchor.transform.localPosition.y)//Mathf.Abs(targetY - GameUIManager.instance.groundAnchor.transform.localPosition.y) < 2f)
        {
            GameManager.MOVE_Y += digCount;

            digCount = 0;
            GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);
            BlockMatchManager.instance.DistroyAllLinker();
            BlockMatchManager.instance.SetBlockLink();

            return true;
        }
        else
        {
            noizeGroundTimer += Global.deltaTimePuzzle;
            float result = Mathf.Sin(2.0f * 3.14159f * noizeGroundTimer * 7f) * 2f;
            float valueX = result / MoveSpeed;

            GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(valueX, GameUIManager.instance.groundAnchor.transform.localPosition.y + Global.deltaTimePuzzle * PANEL_MOVE_SPEED * MoveSpeed, 0);
            SetPanel(GameUIManager.instance.groundAnchor.transform.localPosition.y);

            float digGauget = GameUIManager.instance.groundAnchor.transform.localPosition.y / ((float)stageInfo.Extend_Y * 78);
            GameUIManager.instance.SetDigGauge(digGauget);

            return false;
        }
    }

    public bool CheckDigCount()
    {
        if (GameManager.mExtend_Y <= GameManager.MOVE_Y) return false;

        bool existGround = false;
        //라인위치 체크
        //화면전체 체크로 변경
        for (int j = GameManager.MinScreenY; j < GameManager.MaxScreenY; j++)
        {
            if (ExistGRound(j))
            {
                existGround = true;
                break;
            }
        }

        if (existGround == false)
        {
            SetDigCountMove(stageInfo.digCount);
        }

        return !existGround;
    }

    bool ExistGRound(int indexY)
    {
        for (int i = GameManager.MinScreenX; i < GameManager.MaxScreenX; i++)
        {
            #region 블럭 검사
            BlockBase block = PosHelper.GetBlockScreen(i, indexY);
            BlockGround groundBlock = block as BlockGround;
            if (block != null && groundBlock != null)
            {
                if (groundBlock != null)
                {
                    if (groundBlock.groundType == GROUND_TYPE.JEWEL)
                        return true;

                    if (groundBlock.groundType == GROUND_TYPE.KEY && indexY != GameManager.MaxScreenY - 1)
                        return true;
                }
            }
            if (block != null && block.type == BlockType.KEY && indexY != GameManager.MaxScreenY - 1)
            {
                return true;
            }
            if (block != null && block.type == BlockType.SPACESHIP && indexY != GameManager.MaxScreenY - 1)
            {
                return true;
            }
            if (block != null && block.type == BlockType.PLANT && indexY != GameManager.MaxScreenY - 1)
            {
                BlockPlant plantBlock = block as BlockPlant;
                if (plantBlock != null && plantBlock.plantType == PLANT_TYPE.KEY)
                    return true;
            }
            if (block != null && block.type == BlockType.STONE && indexY != GameManager.MaxScreenY - 1)
            {
                BlockStone stoneBlock = block as BlockStone;
                if (stoneBlock != null && stoneBlock.stoneType == STONE_TYPE.KEY)
                    return true;
            }
            if (block != null && block.type == BlockType.ColorBigJewel && indexY != GameManager.MaxScreenY - 1)
            {
                return true;
            }
            if (block != null && block.type == BlockType.LITTLE_FLOWER_POT)
            {
                return true;
            }
            if (block != null && block.type == BlockType.PEA)
            {
                return true;
            }
            if (block != null && block.type == BlockType.PEA_BOSS)
            {
                return true;
            }
            if (block != null && block.type == BlockType.HEART)
            {
                BlockHeart heart = block as BlockHeart;
                if (heart.HasHeartWayOnScreen())
                    return true;
            }
            if (block != null && block.type == BlockType.BREAD)
            {
                return true;
            }
            #endregion

            #region 보드 검사
            Board tempBoard = PosHelper.GetBoardSreeen(i, indexY);
            if (tempBoard != null && tempBoard.IsActiveBoard == true)
            {
                //카펫 검사
                if (isCarpetStage == true && tempBoard.IsExistCarpet() == false)
                    return true;

                //목표타입의 데코 검사
                for (int k = 0; k < tempBoard.DecoOnBoard.Count; k++)
                {
                    if (tempBoard.DecoOnBoard[k].IsTarget_DigMode() == true)
                        return true;
                }
            }
            #endregion
        }
        return false;
    }

    public void SetPanel(float posY)
    {
        Vector4 range = new Vector4();
        range.x = 0;// 센터
        range.y = -posY;
        range.z = 1024; //사이즈
        range.w = 730;

        if (GameManager.gameMode == GameMode.ADVENTURE)
            range.w = 1120;

        GameUIManager.instance.block_Panel.baseClipRegion = range;
    }

    public void SetLavaPanel(float posY)
    {
        Vector4 range = new Vector4();
        range.x = 0;// 센터
        range.y = -posY - 310;
        range.z = 1024; //사이즈
        range.w = 730 + 620; //620

        GameUIManager.instance.block_Panel.baseClipRegion = range;
    }


    #endregion

    #region NoMoreMove
    //매치불가능 먼저체크
    bool NoMatchableSpace()
    {
        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenY; x++)
            {
                Board tempBoard = PosHelper.GetBoardSreeen(x, y);
                BlockBase block = PosHelper.GetBlockScreen(x, y);

                if (block != null && tempBoard != null && tempBoard.HasDecoHideBlock() == false && tempBoard.HasDecoCoverBlock() == false && (block.blockDeco == null || block.blockDeco.IsInterruptBlockSelect() == false))
                {
                    if (block.IsBombBlock())
                    {
                        return true;
                    }

                    BlockBase blockA = PosHelper.GetBlockScreen(x, y, 1, 0);
                    Board tempBoardA = PosHelper.GetBoardSreeen(x, y, 1, 0);

                    if (block.IsSelectable() && blockA != null && (blockA.IsCanLink() && IsCanLinkSideBLock(x, y, 1, 0)) && (blockA.blockDeco == null || blockA.blockDeco.IsInterruptBlockSelect() == false))// && tempBoardA.HasDecoCoverBlock() == false )
                    {
                        return true;
                    }
                    blockA = PosHelper.GetBlockScreen(x, y, -1, 0);
                    tempBoardA = PosHelper.GetBoardSreeen(x, y, -1, 0);
                    if (block.IsSelectable() && blockA != null && (blockA.IsCanLink() && IsCanLinkSideBLock(x, y, -1, 0)) && (blockA.blockDeco == null || blockA.blockDeco.IsInterruptBlockSelect() == false))//&& tempBoardA.HasDecoCoverBlock() == false)
                    {
                        return true;
                    }
                    blockA = PosHelper.GetBlockScreen(x, y, 0, 1);
                    tempBoardA = PosHelper.GetBoardSreeen(x, y, 0, 1);
                    if (block.IsSelectable() && blockA != null && (blockA.IsCanLink() && IsCanLinkSideBLock(x, y, 0, 1)) && (blockA.blockDeco == null || blockA.blockDeco.IsInterruptBlockSelect() == false))//&& tempBoardA.HasDecoCoverBlock() == false)
                    {
                        return true;
                    }
                    blockA = PosHelper.GetBlockScreen(x, y, 0, -1);
                    tempBoardA = PosHelper.GetBoardSreeen(x, y, 0, -1);
                    if (block.IsSelectable() && blockA != null && (blockA.IsCanLink() && IsCanLinkSideBLock(x, y, 0, -1)) && (blockA.blockDeco == null || blockA.blockDeco.IsInterruptBlockSelect() == false))//&& tempBoardA.HasDecoCoverBlock() == false)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator CoResetNoMoreMove()
    {
        float waitTimer = 0;
        //툴 ManagerUI._instance.OpenPopupNoMoreMove();

        while (true)
        {
            if (checkBlockWait())
            {
                break;
            }

            waitTimer = 0;
            while (waitTimer < 0.1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        if (BlockBase._listBlock.Count <= 1)
        {
            ShowScreenNormalBlock(false);
        }
        yield return null;

        waitTimer = 0;
        while (waitTimer < 0.8f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        int count = 0;
        bool gameFail = false;

        List<BlockColorType> colorTypeList = new List<BlockColorType>();
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                {
                    colorTypeList.Add(back.Block.colorType);
                }
            }
        }
        yield return null;

        //매치할 수 있는 블럭이 생길 떄 까지 검사(최대 200회)
        while (IsCanMatchBlock() == false)
        {
            List<BlockColorType> tempColorTypeList = new List<BlockColorType>();

            if (count > 200)
            {
                gameFail = true;
                break;
            }
            else if (count > 100)
            {
                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                        {
                            back.Block.colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap();
                        }
                    }
                }
            }
            else
            {
                tempColorTypeList.Clear();
                foreach (var colorType in colorTypeList)
                {
                    tempColorTypeList.Add(colorType);
                }

                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                        {
                            int rand = GameManager.instance.GetIngameRandom(0, tempColorTypeList.Count);
                            back.Block.colorType = tempColorTypeList[rand];
                            tempColorTypeList.RemoveAt(rand);
                        }
                    }
                }
            }
            count++;
        }
        yield return null;

        if (gameFail)
        {
            if (EditManager.instance != null)
            {
                EditManager.instance.ResetTool();
            }
            else
            {
                ManagerUI._instance.ImmediatelyCloseAllPopUp();
                waitTimer = 0;
                while (waitTimer < 0.2f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
                GameManager.instance.StageFail();
            }
        }
        else
        {
            ShowScreenNormalBlock(true);
            waitTimer = 0;
            while (waitTimer < 0.8f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            //툴ManagerUI._instance.ImmediatelyCloseAllPopUp();
            state = BlockManagrState.READY;
        }
        yield return null;
    }

    /// <summary>
    /// 매치할 블럭이 있는지 검사하는 코드
    /// </summary>
    private bool IsCanMatchBlock()
    {
        //매치할 수 있는 블럭이 있는지 검사.
        if (SearchMatchBlock())
            return true;

        //자동 매치 검사
        if (IsCanAutoMatch_FlowerInk())
            return true;
        return false;
    }

    //블럭섞기 치트에서 사용
    public IEnumerator CoMixBlockAtTool()
    {
        float waitTimer = 0;
        ManagerUI._instance.OpenPopupNoMoreMove();

        while (true)
        {
            if (checkBlockWait())
            {
                break;
            }

            waitTimer = 0;
            while (waitTimer < 0.1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        if (BlockBase._listBlock.Count <= 1)
        {
            ShowScreenNormalBlock(false);
        }
        yield return null;

        waitTimer = 0;
        while (waitTimer < 0.8f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        int count = 0;
        bool gameFail = false;

        List<BlockColorType> colorTypeList = new List<BlockColorType>();
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                {
                    colorTypeList.Add(back.Block.colorType);
                }
            }
        }
        yield return null;

        //치트에서 동작되기 때문에 매치되는 블럭 있더라도 한번은 무조건 섞기.
        do
        {
            List<BlockColorType> tempColorTypeList = new List<BlockColorType>();
            if (count > 200)
            {
                gameFail = true;
                break;
            }
            else if (count > 100)
            {
                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                        {
                            back.Block.colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap();
                        }
                    }
                }
            }
            else
            {
                tempColorTypeList.Clear();
                foreach (var colorType in colorTypeList)
                {
                    tempColorTypeList.Add(colorType);
                }

                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                        {
                            int rand = GameManager.instance.GetIngameRandom(0, tempColorTypeList.Count);
                            back.Block.colorType = tempColorTypeList[rand];
                            tempColorTypeList.RemoveAt(rand);
                        }
                    }
                }
                count++;
            }
        }
        while (IsCanMatchBlock() == false);
        yield return null;

        if (gameFail)
        {
            if (EditManager.instance != null)
            {
                EditManager.instance.ResetTool();
            }
            else
            {
                ManagerUI._instance.ImmediatelyCloseAllPopUp();
                waitTimer = 0;
                while (waitTimer < 0.2f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
                GameManager.instance.StageFail();
            }
        }
        else
        {
            ShowScreenNormalBlock(true);
            waitTimer = 0;
            while (waitTimer < 0.8f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            ManagerUI._instance.ImmediatelyCloseAllPopUp();
            state = BlockManagrState.READY;
        }
        yield return null;
    }

    public void ShowScreenNormalBlock(bool enable)
    {
        if (enable)
        {
            BlockMatchManager.instance.DestroyLinkerNoReset();
            StartCoroutine(CoShowAllBlock());
        }
        else
        {
            StartCoroutine(HideAllBlock());
        }
    }

    public IEnumerator HideAllBlock()
    {
        float waitTimer = 0;

        for (int y = GameManager.MIN_Y; y < GameManager.MAX_Y; y++)
        {
            int tempY = y;
            int tempX = GameManager.MIN_X;

            while (tempY > GameManager.MIN_Y)
            {
                Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                {
                    back.Block.Hide();
                }

                tempY--;
                tempX++;
            }
            waitTimer = 0;
            while (waitTimer < 0.03f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        for (int x = GameManager.MIN_X; x < GameManager.MAX_X; x++)
        {
            int tempX = x;
            int tempY = GameManager.MAX_Y;

            while (tempX < GameManager.MAX_X)
            {
                Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                {
                    back.Block.Hide();
                }

                tempY--;
                tempX++;
            }
            waitTimer = 0;
            while (waitTimer < 0.03f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        BlockMatchManager.instance.DestroyLinkerNoReset();
        yield return null;
    }

    public IEnumerator CoShowAllBlock(int opt = 1)
    {
        float waitTimer = 0;

        if (opt == 1)
        {
            BlockMatchManager.instance.SetBlockLink();
            yield return null;

            for (int y = GameManager.MIN_Y; y < GameManager.MAX_Y; y++)
            {
                int tempY = y;
                int tempX = GameManager.MIN_X;

                while (tempY > GameManager.MIN_Y)
                {
                    Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                    if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                    {
                        //back._objBlock.Type = back._objBlock.TempType;
                        back.Block.Show();
                        back.Block.MakeLinkerByManager();
                    }
                    else if (back != null && back.Block != null)
                    {
                        back.Block.MakeLinkerByManager();
                    }

                    tempY--;
                    tempX++;
                }

                waitTimer = 0;
                while (waitTimer < 0.015f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }


            for (int x = GameManager.MIN_X; x < GameManager.MAX_X; x++)
            {
                int tempX = x;
                int tempY = GameManager.MAX_Y;

                while (tempX < GameManager.MAX_X)
                {
                    Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                    if (back != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock() && back.Block != null && back.Block.IsNormalBlock() && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                    {
                        //back._objBlock.Type = back._objBlock.TempType;
                        back.Block.Show();
                        back.Block.MakeLinkerByManager();
                    }
                    else if (back != null && back.Block != null)
                    {
                        back.Block.MakeLinkerByManager();
                    }

                    tempY--;
                    tempX++;
                }
                waitTimer = 0;
                while (waitTimer < 0.015f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }

            BlockMatchManager.instance.SetBlockLink();
        }
        else if (opt == 0)
        {
            for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
            {
                for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                {
                    Board back = PosHelper.GetBoardSreeen(j, i);
                    if (back != null && back.Block != null)
                    {
                        back.Block.Show();
                        back.Block.Destroylinker();
                    }
                }
            }
        }

        yield return null;

    }

    #endregion

    public void AddScore(int tempScore)
    {
        score += tempScore; //원점수추가

        int tempScoreA = score;


        float scoreRatio = 1;
        if (EditManager.instance != null && EditManager.instance.readyItemTest[1] == true)
        {
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;
        }
        /*        
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            //tempScoreA = (int)((float)score * (1.1f + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;
        }
        else
        {
            //tempScoreA = (int)((float)score * (1 + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;
        }

        */
        GameUIManager.instance.SetScore((int)(tempScoreA * scoreRatio));   //변경된 점수 추가

        //Debug.Log("실재점수=" + score + ", 배율 =" + scoreRatio + ", 표시점수=" + (int)(tempScoreA * scoreRatio));

    }
    public void AddCoin(int tempCoin)
    {
        coins += tempCoin;

        if (GameManager.gameMode != GameMode.ADVENTURE)
            GameUIManager.instance.SetCoin(coins);
        else
            GameUIManager.instance.SetAdventureCoin(coins);
    }

    //블럭도착사운드 재생

    float _lastAudioPlayTime = 0;
    public void PlayLandAudio()
    {
        Invoke("PlayLandSound", 0.1f);
    }

    void PlayLandSound()
    {
        if (Time.time - _lastAudioPlayTime >= 0.2f)
        {
            ManagerSound.AudioPlayMany(AudioInGame.LAND_BLOCK);
            _lastAudioPlayTime = Time.time;
        }
    }


    void GetNearCoverColor(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            //&& getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            //&& getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            //&& getNearBlock.TempBombType == BlockBombType.NONE
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            GetNearCoverColor(block1, tempCheckBlocks);
        }
    }


    void GetNearSameColor(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (getNearBlock != null
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType
            && getNearBlock.IsCanLink()
            && getNearBlock.IsBombBlock() == false)
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            GetNearCoverColor(block1, tempCheckBlocks);
        }
    }


    public void GetIceApple()
    {
        iceCount = 3;
        /*
        GameUIManager.instance.SetIceCount();

        foreach (var lava in listLava)
        {
            lava.SetIceLave(true);

            //lava.uiSprite.color = new Color(0,1,0,1);
            //lava.BGSprite.color = new Color(0, 1, 0, 1);
        }
        */


    }

    public void SetImageGetIceApple()
    {
        if (GameManager.gameMode != GameMode.LAVA) return;

        GameUIManager.instance.SetIceCount();

        foreach (var lava in listLava)
        {
            if (lava != null)
            {
                lava.SetIceLave(true);

                int randEffect = Random.Range(0, 3);
                if (randEffect == 0) InGameEffectMaker.instance.MakeIceShineEffect1(lava.transform.position);
            }


            //lava.uiSprite.color = new Color(0,1,0,1);
            //lava.BGSprite.color = new Color(0, 1, 0, 1);
        }
    }


    public void ResetIceApple()
    {
        foreach (var lava in listLava)
        {
            if (lava != null)
            {
                lava.SetIceLave(false);
                //lava.uiSprite.color = Color.white;
                //lava.BGSprite.color = Color.white;
                // int randEffect = Random.Range(0, 2);
                //  if (randEffect == 0) InGameEffectMaker.instance.MakeIceShineEffect1(lava.transform.position);

                int randEffect = Random.Range(0, 4);
                if (randEffect == 0) InGameEffectMaker.instance.MakeICeEffect(lava.transform.position);

            }
        }
    }

    private void SetTargetCarpetCount()
    {
        bool hasCarpet = false;
        int targetCount = 0;
        int boardCount = 0;

        //보드에 카펫이 있는 지 검사.
        foreach (var board in ManagerBlock.boards)
        {
            if (board.IsActiveBoard == false)
                continue;
            boardCount++;

            hasCarpet = false;
            for (int i = 0; i < board.DecoOnBoard.Count; i++)
            {
                if (board.DecoOnBoard[i] as Carpet != null)
                {
                    hasCarpet = true;
                    break;
                }
            }
            if (hasCarpet == false)
            {
                targetCount++;
            }
        }

        //현재 카펫이 깔려있는 맵이라면 카펫일 깔리지 않은 보드들을 타겟으로 설정.
        int targetType = (int)TARGET_TYPE.CARPET;
        int count = (boardCount > targetCount) ? targetCount : 0;
        SetStageInfo_TargetCount(targetType, count);
    }

    public string GetColorTypeString(BlockColorType colorType)
    {
        switch (colorType)
        {
            case BlockColorType.A:
                return "Rabbit";

            case BlockColorType.B:
                return "Bear";

            case BlockColorType.C:
                return "Kangaroo";

            case BlockColorType.D:
                return "Tiger";

            case BlockColorType.E:
                return "Wolf";

            case BlockColorType.F:
                return "Wildpig";

            default:
                return "";
        }
    }

    public bool hasDeco_CheckStartInfo(BoardDecoType decoType)
    {
        //출발에서 생성될 수 있는 블럭 정보
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            int blockType = startInfo.type;
            if (startInfo.probability == 0)
                continue;

            switch(decoType)
            {
                case BoardDecoType.ICE:
                    if (blockType == (int)BlockType.ICE || isColorIce(startInfo.iceProb))
                        return true;
                    break;
            }
        }
        return false;
    }

    public bool hasDeco_CheckAllBlock(BoardDecoType decoType, BlockColorType colorType = BlockColorType.NONE)
    {
        foreach (BlockInfo blockInfo in stageInfo.ListBlock)
        {
            if (hasDeco(blockInfo.ListDeco, colorType, decoType) == true)
            {
                return true;

            }
        }

        return false;
    }

    public static bool hasDeco(List<DecoInfo> decoList, BlockColorType colorType = BlockColorType.NONE, params BoardDecoType[] decoType)
    {
        if (decoList == null)
            return false;

        foreach (DecoInfo boardInfo in decoList)
        {
            foreach (BoardDecoType type in decoType)
            {
                if (boardInfo.BoardType == (int)type)
                {
                    if(colorType == BlockColorType.NONE || boardInfo.index == (int)colorType)
                    return true;
                }
            }
        }

        return false;
    }

    //시작할때 폭탄이 생성되지않도록 수정
    private void PreventingBombsOnStartup()
    {
        if (GameManager.instance.state == GameState.EDIT)
            return;

        int MAX_LINK_BLOCK_COUNT = 5;
        if (GameManager.gameMode == GameMode.ADVENTURE)
            MAX_LINK_BLOCK_COUNT = ADVENTURE_PANG_SAME_COLOR_COUNT;// -1;

        List<BlockBase> tempCheckBlockList = new List<BlockBase>();
        tempCheckBlockList.Clear();
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                Board board = PosHelper.GetBoard(i, j);

                if (board != null && board.Block != null && board.Block is NormalBlock && board.Block.colorType != BlockColorType.RANDOM && board.Block.colorType != BlockColorType.NONE)
                {
                    bool isNotRandomBlock = false;

                    foreach (BlockInfo blockInfo in stageInfo.ListBlock)
                    {
                        if (blockInfo.inX == i && blockInfo.inY == j && blockInfo.type == (int)BlockType.NORMAL && blockInfo.colorType != (int)BlockColorType.RANDOM)
                        {
                            isNotRandomBlock = true;
                            break;
                        }
                    }

                    if (isNotRandomBlock == false)
                    {
                        tempCheckBlockList.Clear();
                        GetNearCoverColor(board.Block, tempCheckBlockList);

                        if (tempCheckBlockList.Count >= MAX_LINK_BLOCK_COUNT)
                        {
                            board.Block.colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap(board.Block.colorType);
                        }
                    }
                }
            }
        }

        tempCheckBlockList.Clear();
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                Board board = PosHelper.GetBoard(i, j);

                if (board != null && board.Block != null && board.Block is NormalBlock && board.Block.colorType != BlockColorType.RANDOM && board.Block.colorType != BlockColorType.NONE)
                {
                    bool isNotRandomBlock = false;

                    foreach (BlockInfo blockInfo in stageInfo.ListBlock)
                    {
                        if (blockInfo.inX == i && blockInfo.inY == j && blockInfo.type == (int)BlockType.NORMAL && blockInfo.colorType != (int)BlockColorType.RANDOM)
                        {
                            isNotRandomBlock = true;
                            break;
                        }
                    }

                    if (isNotRandomBlock == false)
                    {
                        tempCheckBlockList.Clear();
                        GetNearCoverColor(board.Block, tempCheckBlockList);

                        if (tempCheckBlockList.Count >= MAX_LINK_BLOCK_COUNT)
                        {
                            board.Block.colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap(board.Block.colorType);
                        }
                    }
                }
            }
        }

        tempCheckBlockList.Clear();
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                Board board = PosHelper.GetBoard(i, j);

                if (board != null && board.Block != null && board.Block is NormalBlock && board.Block.colorType != BlockColorType.RANDOM && board.Block.colorType != BlockColorType.NONE)
                {
                    bool isNotRandomBlock = false;

                    foreach (BlockInfo blockInfo in stageInfo.ListBlock)
                    {
                        if (blockInfo.inX == i && blockInfo.inY == j && blockInfo.type == (int)BlockType.NORMAL && blockInfo.colorType != (int)BlockColorType.RANDOM)
                        {
                            isNotRandomBlock = true;
                            break;
                        }
                    }

                    if (isNotRandomBlock == false)
                    {
                        tempCheckBlockList.Clear();
                        GetNearCoverColor(board.Block, tempCheckBlockList);

                        if (tempCheckBlockList.Count >= MAX_LINK_BLOCK_COUNT)
                        {
                            board.Block.colorType = BlockMaker.instance.GetBlockRandomTypeAtMakeMap(board.Block.colorType);
                        }
                    }
                }
            }
        }
    }

    //벌집기믹에서 블럭들의 우선순위 검사하는 함수.
    public Board GetTargetBoardAtFireWork(bool hasCarpet = false)
    {
        int rank = 100;
        List<Board> listTargetBoard = new List<Board>();

        //전체 보드 검사
        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                Board board = PosHelper.GetBoardSreeen(i, j);
                BlockBase checkBlock = PosHelper.GetBlockScreen(i, j);

                if (board == null || board.IsActiveBoard == false || board.isFireWorkBoard == true || board.lava != null)
                    continue;

                bool isContinue = false;
                
                for (int k = 0; k < board.BoardOnHide.Count; k++)
                {
                    if(board.BoardOnHide[k].IsRainbowBomb)
                    {
                        isContinue = true;
                        break;
                    }
                }
                
                if(isContinue)
                    continue;

                int getBlockRank = 100;

                //잡기돌, 물 같이 블럭이 없을 수도 있는 보드들 1순위로 체크.
                if ((board.BoardOnNet.Count > 0 || board.BoardOnHide.Count > 0) &&
                    (checkBlock == null || (checkBlock.isRainbowBomb == false && checkBlock.state != BlockState.PANG)))
                {
                    getBlockRank = 1;
                }
                else if (checkBlock != null && checkBlock.IsDisabled)
                {
                    continue;
                }
                else if (checkBlock == null || checkBlock.state == BlockState.PANG
                    || checkBlock.isRainbowBomb == true || checkBlock.secendBomb == true)
                {
                    continue;
                }

                if (getBlockRank != 1)
                {
                    getBlockRank = (int)checkBlock.GetBlockRankAtFireWork(board, hasCarpet);
                }

                //검사하는 랭크와 현재 블럭의 랭크를 비교.
                if (getBlockRank == 100 || rank < getBlockRank)
                {
                    continue;
                }
                else if (rank > getBlockRank)
                {
                    rank = getBlockRank;
                    listTargetBoard.Clear();
                }

                if (listTargetBoard.Contains(board) == false)
                {
                    listTargetBoard.Add(board);
                }
            }
        }

        //우선순위가 높은 블럭 중 랜덤으로 반환.
        if (listTargetBoard.Count > 0)
        {
            int random = GameManager.instance.GetIngameRandom(0, listTargetBoard.Count);
            return listTargetBoard[random];
        }
        return null;
    }

    //컬러블럭이 제거될때 목표 및 확인하는 장치
    public void PangColorBlock(BlockColorType tempColor, Vector3 pos)
    {
        //제거한 컬러 블럭 카운트
        int colorIndex = (int)tempColor - 1;
        if (colorIndex < ManagerBlock.instance.removeBlockCount.Length)
        {
            ManagerBlock.instance.removeBlockCount[colorIndex].Value++;
        }

        //컬러 목표확인
        if (HasAchievedCollectTarget(TARGET_TYPE.COLORBLOCK, tempColor))
        {
            InGameEffectMaker.instance.MakeFlyTargetColor(pos, tempColor);
        }

        //하트 기믹 게이지 확인
        if (dicHeart != null && dicHeart.Count > 0)
        {
            AddGaugeHeartColor(tempColor);
        }

        if (listCannonItem != null && listCannonItem.Count > 0)
        {
            AddCannonCount(tempColor);
        }
    }

    //폭탄을 만들 때 사용됐던 블럭들을 모으는 연출.
    public void CollectBlocksUsedToMakeBomb(BlockColorType tempColor, Vector3 pos)
    {
        if (GameManager.gameMode == GameMode.COIN)
        {
            InGameEffectMaker.instance.MakeFlyFeverBlock(pos, tempColor);
        }
    }

    #region 소다젤리
    public void AddBlockSodaJellyInDictionary(int groupIndex, BlockSodaJelly blockSodaJelly)
    {
        if (dicBlockSodaJelly.ContainsKey(groupIndex) == false)
        {
            List<BlockSodaJelly> listBlockSodaJelly = new List<BlockSodaJelly>();
            listBlockSodaJelly.Add(blockSodaJelly);
            ManagerBlock.instance.dicBlockSodaJelly.Add(groupIndex, listBlockSodaJelly);
        }
        else
        {
            List<BlockSodaJelly> listBlockSodaJelly = ManagerBlock.instance.dicBlockSodaJelly[groupIndex];
            if (listBlockSodaJelly == null)
            {
                listBlockSodaJelly = new List<BlockSodaJelly>();
                listBlockSodaJelly.Add(blockSodaJelly);
                ManagerBlock.instance.dicBlockSodaJelly[groupIndex] = listBlockSodaJelly;
            }
            else
            {
                if (listBlockSodaJelly.Contains(blockSodaJelly) == false)
                    ManagerBlock.instance.dicBlockSodaJelly[groupIndex].Add(blockSodaJelly);
            }
        }
    }

    public bool IsCanPangBlockSodaJelly(int groupIndex)
    {
        if (dicBlockSodaJelly.ContainsKey(groupIndex) == false)
        {
            return false;
        }
        else
        {
            List<BlockSodaJelly> listBlockSodaJelly = ManagerBlock.instance.dicBlockSodaJelly[groupIndex];
            for (int i = 0; i < listBlockSodaJelly.Count; i++)
            {
                if (listBlockSodaJelly[i].IsCanDestroy() == false)
                    return false;
            }
        }
        return true;
    }

    public void PangBlockSodaJelly(int groupIndex)
    {
        if (dicBlockSodaJelly.ContainsKey(groupIndex) == false)
            return;

        List<BlockSodaJelly> listBlockSodaJelly = ManagerBlock.instance.dicBlockSodaJelly[groupIndex];
        for (int i = 0; i < listBlockSodaJelly.Count; i++)
        {
            listBlockSodaJelly[i].PangBlockSodaJelly();
        }
    }
    #endregion

    #region 단계석판
    public void AddCountCrackDictionary(int groupIndex, CountCrack countCrack)
    {
        if (dicCountCrack.ContainsKey(groupIndex) == false)
        {
            List<CountCrack> listCountCrack = new List<CountCrack>();
            listCountCrack.Add(countCrack);
            ManagerBlock.instance.dicCountCrack.Add(groupIndex, listCountCrack);
        }
        else
        {
            List<CountCrack> listCountCrack = ManagerBlock.instance.dicCountCrack[groupIndex];
            if (listCountCrack == null)
            {
                listCountCrack = new List<CountCrack>();
                listCountCrack.Add(countCrack);
                ManagerBlock.instance.dicCountCrack[groupIndex] = listCountCrack;
            }
            else
            {
                if (listCountCrack.Contains(countCrack) == false)
                    ManagerBlock.instance.dicCountCrack[groupIndex].Add(countCrack);
            }
        }
    }

    /// <summary>
    /// 게임 시작 시, 최소 인덱스의 단계석판만 활성화해주는 코드
    /// </summary>
    public void InitCountCrack()
    {
        //단계석판 사용하는 맵만 해당 함수 실행.
        if (dicCountCrack.Count == 0 || GameManager.instance.state == GameState.EDIT)
            return;

        for (int i = 1; i <= MAXCOUNT_COUNTCRACK; i++)
        {
            if (ManagerBlock.instance.dicCountCrack.ContainsKey(i) == true)
            {
                activeCountCrackIdx = i;
                break;
            }
        }

        //현재 액티브된 단계석판만 활성화.
        var enumerator = ManagerBlock.instance.dicCountCrack.GetEnumerator();
        while (enumerator.MoveNext())
        {
            List<CountCrack> listCountCrack = new List<CountCrack>(enumerator.Current.Value);

            if (enumerator.Current.Key == activeCountCrackIdx)
            {
                for (int i = 0; i < listCountCrack.Count; i++)
                {
                    listCountCrack[i].ActiveCountCrack();
                }
            }
            else
            {
                for (int i = 0; i < listCountCrack.Count; i++)
                {
                    listCountCrack[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public TARGET_TYPE GetCollectTypeCountCrackByInteger(int count)
    {
        switch (count)
        {
            case 1:
                return TARGET_TYPE.COUNT_CRACK1;
            case 2:
                return TARGET_TYPE.COUNT_CRACK2;
            case 3:
                return TARGET_TYPE.COUNT_CRACK3;
            case 4:
                return TARGET_TYPE.COUNT_CRACK4;
        }
        return 0;
    }

    /// <summary>
    /// 석판 전체 카운트 세는 함수.
    /// </summary>
    private void SetTargetCrack()
    {
        int crackTotalCount = 0;
        foreach (BlockInfo blockInfo in stageInfo.ListBlock)
        {
            foreach (DecoInfo boardInfo in blockInfo.ListDeco)
            {
                if (boardInfo.BoardType == (int)BoardDecoType.CARCK1)
                {
                    crackTotalCount++;
                }
            }
        }
        SetStageInfo_TargetCount((int)TARGET_TYPE.CRACK, crackTotalCount);
    }

    /// <summary>
    /// 단계석판 전체 카운트 세는 함수.
    /// </summary>
    private void SetTargetCountCrack()
    {
        for (int i = 1; i <= ManagerBlock.MAXCOUNT_COUNTCRACK; i++)
        {
            int targetType = (int)GetCollectTypeCountCrackByInteger(i);
            int crackCount = (ManagerBlock.instance.dicCountCrack.ContainsKey(i) == false) ? 0 : ManagerBlock.instance.dicCountCrack[i].Count;
            SetStageInfo_TargetCount(targetType, crackCount);
        }
    }

    /// <summary>
    /// 단계석판이 제거되었는지 확인하고 생성해주는 이벤트 
    /// </summary>
    private IEnumerator CoCountCrackAppearEvent()
    {
        //단계석판 없는 경우는 이벤트 검사하지 않음.
        if (ManagerBlock.instance.activeCountCrackIdx == 0)
            yield break;

        TARGET_TYPE targetType = GetCollectTypeCountCrackByInteger(ManagerBlock.instance.activeCountCrackIdx);

        //현재 단계석판 덜 제거한 상태면 검사 나감.
        CollectTargetCount collectTargetCount = GetCollectTargetCountData(targetType);
        if (collectTargetCount != null && (collectTargetCount.collectCount > collectTargetCount.pangCount))
            yield break;

        //현재 활성화된 단계석판 목표 수까지 석판이 모두 제거됐으면 다음 단계석판 활성화
        bool isChangeCrackIdx = false;
        for (int i = (activeCountCrackIdx + 1); i <= MAXCOUNT_COUNTCRACK; i++)
        {
            if (ManagerBlock.instance.dicCountCrack.ContainsKey(i) == false)
                continue;
            activeCountCrackIdx = i;
            isChangeCrackIdx = true;
            break;
        }

        if (isChangeCrackIdx == false)
        {   //더이상 단계석판 없으면 인덱스 0으로 바꾸고 종료.
            activeCountCrackIdx = 0;
            yield break;
        }
        else
        {   //단계석판 있으면 등장 연출.
            int endCount = 0;
            int waitCount = 0;
            List<CountCrack> listCountCrack = new List<CountCrack>(ManagerBlock.instance.dicCountCrack[activeCountCrackIdx]);

            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            for (int i = 0; i < listCountCrack.Count; i++)
            {
                waitCount++;
                StartCoroutine(listCountCrack[i].CoAppearAction(() => endCount++));
            }
            waitCount++;
            StartCoroutine(CoMakeBlockByCountCrack(() => endCount++));

            //연출 모두 끝날떄까지 대기.
            yield return new WaitUntil(() => endCount >= waitCount);
        }
    }

    /// <summary>
    /// 단계 석판이 생성될 때, 블럭 생성해주는 함수.
    /// </summary>
    public IEnumerator CoMakeBlockByCountCrack(System.Action action = null)
    {
        //다음으로 활성화 될 단계석판 인덱스 가져오기.
        int gimmickIndex = ManagerBlock.instance.stageInfo.listDecoInfo.FindIndex(x => x.gimmickType == (int)BoardDecoType.COUNT_CRACK);
        if (gimmickIndex > -1 && ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] is GimmickInfo_CountCrack == true)
        {
            //해당 단계석판이 활성화될 떄 생성될 사과 정보 받아오기.
            GimmickInfo_CountCrack info = ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] as GimmickInfo_CountCrack;
            int infoIndex = info.listAppleInfo.FindIndex(x => x.crackIndex == activeCountCrackIdx);
            if (infoIndex > -1)
            {
                int makeCount = info.listAppleInfo[infoIndex].count;

                //블럭 리스트 가져오기.
                List<BlockBase> listBlock = PosHelper.GetRandomBlockListOnTheCountCrack();
                if (listBlock.Count > makeCount)
                {
                    if (GameManager.instance.ingameSeed <= 0)
                        GenericHelper.Shuffle(listBlock);
                    else
                        GenericHelper.Shuffle(listBlock, GameManager.instance.ingameRandom);
                }
                else
                    makeCount = listBlock.Count;

                int endCount = 0;
                for (int i = 0; i < makeCount; i++)
                {
                    StartCoroutine(CoChangeBlockAnotherType(
                        isAction: true,
                        baseBlock: listBlock[i],
                        type: BlockType.APPLE,
                        blockColorType: BlockColorType.NONE,
                        count: info.listAppleInfo[infoIndex].index,
                        endAction: () => { endCount++; }
                        ));
                }

                //연출 모두 끝날떄까지 대기.
                yield return new WaitUntil(() => endCount >= makeCount);
            }
        }
        action.Invoke();
    }
    #endregion

    #region 페인트
    private IEnumerator CoPangPaint()
    {
        bool isPang = false;
        for (int i = 0; i < listPaint.Count; i++)
        {
            if (listPaint[i].isStateWaitPang == true)
            {
                StartCoroutine(listPaint[i].CoPangFinal());
                isPang = true;
            }
        }
        if (isPang == true)
        {
            //페인트가 터진 뒤, 블럭이 흐를 수 있는 시간까지 대기
            yield return new WaitForSeconds(GetIngameTime(0.8f));
            yield return new WaitUntil(() => checkBlockWait());
        }
    }
    #endregion


    #region 물폭탄

    private IEnumerator CoBombAllWaterBomb()
    {
        foreach (var waterBomb in listWaterBomb)
        {
            if (waterBomb.timeCount <= 0)
            {
                waterBomb.IsSkipPang = true;
                waterBomb.state = BlockState.PANG;
                StartCoroutine(waterBomb.CoPangFinal());
            }
        }
        yield return null;
    }

    #endregion
    
    #region 엔드 컨텐츠
    public void AddEndContents(BlockEndContentsItem blockEndContents)
    {
        listEndContentsItem.Add(blockEndContents);
    }

    #endregion

    #region 자동매치

    //모든 자동 매치 확인
    private bool isCanAutoMatch()
    {
        return IsCanAutoMatch_FlowerInk();
    }

    //모든 자동 매치에 대한 처리
    private IEnumerator CoAutoMatchEvent()
    {
        bool isMatching = isCanAutoMatch();

        //자동매치 가능한지 검사
        if (isMatching)
        {
            BlockBase.uniqueIndexCount++;

            if (IsCanAutoMatch_FlowerInk())
            {
                //자동매치 블럭/데코 처리
                AutoMatchDecoEvent();

                //모든 블럭의 움직임이 멈출 때 까지 대기
                yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());
            }

            //생성기 이미지 갱신
            BlockGeneratorImageCheck();

            //재귀
            yield return CoAutoMatchEvent();
        }
        yield break;
    }

    //자동으로 매치 할 수 있는 블럭이 자동매치 바닥 위에 있는지.
    private bool IsCanAutoMatch_FlowerInk()
    {
        if (ManagerBlock.instance.listFlowerInk.Count <= 0)
            return false;

        for (int i = 0; i < listFlowerInk.Count; i++)
        {
            if (listFlowerInk[i].IsCanAutoMatch() == true)
                return true;
        }
        return false;
    }

    //자동매치 바닥과 같은 색의 블럭이 보드 위에 존재 하는지.
    private bool HasAutoMatch_FlowerInk()
    {
        if (ManagerBlock.instance.listFlowerInk.Count <= 0)
            return false;

        HashSet<BlockColorType> setFlowerInkColorType = new HashSet<BlockColorType>();
        for (int i = 0; i < listFlowerInk.Count; i++)
        {
            setFlowerInkColorType.Add(listFlowerInk[i].colorType);
        }

        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenY; x++)
            {
                //보드 검사
                Board tempBoard = PosHelper.GetBoardSreeen(x, y);
                if (tempBoard == null || tempBoard.BoardOnNet.Count > 0 || tempBoard.Block == null)
                    continue;

                //블럭 검사
                BlockBase block = tempBoard.Block;
                if (block.type != BlockType.NORMAL || block.IsNormalBlock() == false || block.blockDeco != null)
                    continue;

                //잉크 틀과 같은 색의 블럭인지 검사.
                if (setFlowerInkColorType.Contains(block.colorType))
                    return true;
            }
        }
        return false;
    }

    private void AutoMatchDecoEvent()
    {
        //목표가 남아있는 상태면 블럭 매칭 동작
        for (int i = 0; i < listFlowerInk.Count; i++)
        {
            if (HasAchievedCollectTarget(TARGET_TYPE.FLOWER_INK, listFlowerInk[i].colorType))
            {
                listFlowerInk[i].PangMatchBlock();
            }
        }

        //목표를 달성한 상태의 데코 제거
        for (int i = listFlowerInk.Count - 1; i >= 0; i--)
        {
            if (HasAchievedCollectTarget(TARGET_TYPE.FLOWER_INK, listFlowerInk[i].colorType) == false)
                listFlowerInk[i].RemoveDeco();
        }
    }
    #endregion

    #region 우주선
    private IEnumerator CoMoveSpaceShip()
    {
        int moveSpaceShipCount = 0;

        List<BlockSpaceShip> listTemp = new List<BlockSpaceShip>();

        //보드 위쪽에 존재하는 기믹 -> 아래에 존재하는 기믹 순서로 정렬
        listSpaceShip.Sort((BlockSpaceShip a, BlockSpaceShip b) => a.indexY.CompareTo(b.indexY));

        for (int i = 0; i < listSpaceShip.Count; i++)
        {
            BlockSpaceShip temp = listSpaceShip[i] as BlockSpaceShip;

            temp.isMoveToUp = temp.isCanMoveToUpSpaceShip();

            if (temp.isMoveToUp)
            {
                temp.setMoveToUpBlock();
                listTemp.Add(temp);
            }
        }
        if (listTemp.Count > 0)
        {
            for (int i = 0; i < listTemp.Count; i++)
            {
                StartCoroutine(listTemp[i].MoveToUpSpaceShip(() => { moveSpaceShipCount++; }));

                listSpaceShip[i].isMoveToUp = false;
            }

            yield return new WaitUntil(() => moveSpaceShipCount >= listTemp.Count);
        }
    }

    public void ResetMoveTurn()
    {
        for (int i = 0; i < listSpaceShip.Count; i++)
        {
            listSpaceShip[i].isMoveDownThisTurn = false;
        }
    }

    public void ResetStopEvent()
    {
        for (int i = 0; i < listSpaceShip.Count; i++)
        {
            listSpaceShip[i].isStopEvent = false;
        }
    }

    public void CloverEvent()
    {
        //장막 처리
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board board = PosHelper.GetBoard(j, i);
                if (board != null)
                {
                    if(board.HasDecoHideBlock())
                    {
                        foreach (var deco in board.DecoOnBoard)
                        {
                            deco.EventAction();
                            deco.Init();
                        }
                    }
                }
            }
        }
    }
    
    public void AddSpaceShip(BlockSpaceShip blockSpaceShip)
    {
        listSpaceShip.Add(blockSpaceShip);
    }

    public void RemoveSpaceShip(BlockSpaceShip blockSpaceShip)
    {
        listSpaceShip.Remove(blockSpaceShip);
    }

    public void MakeBomb(int bombCount, int[] bombProb)
    {
        StartCoroutine(CoMakeBomb(bombCount, bombProb));
    }

    public IEnumerator CoMakeBomb(int bombCount, int[] bombProb)
    {
        BlockBomb._liveCount++;

        int pangCheck = 0;
        List<BlockBase> listBlock = new List<BlockBase>();

        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                Board tempBoard = PosHelper.GetBoard(x, y);
                if (block != null && tempBoard != null && tempBoard.HasDecoHideBlock() == false && tempBoard.HasDecoCoverBlock() == false && block.blockDeco == null)
                    if (block.IsNormalBlock() && block.state == BlockState.WAIT && block is NormalBlock)
                        listBlock.Add(block);
            }
        }

        List<BlockBase> listPangBlock = new List<BlockBase>();
        int index = 0;

        if (listBlock.Count < bombCount)
        {
            bombCount = listBlock.Count;
        }

        // 확률 계산
        int bombProbCount = 0;
        List<BlockBombType> listBombProb = new List<BlockBombType>();
        for (int i = 0; i < bombProb.Length; i++)
        {
            bombProbCount += bombProb[i];

            switch (i)
            {
                case 0: // BOMB
                    for (int j = 0; j < bombProb[i]; j++)
                    {
                        listBombProb.Add(BlockBombType.BOMB);
                    }
                    break;
                case 1: //LINE_V
                    for (int j = 0; j < bombProb[i]; j++)
                    {
                        listBombProb.Add(BlockBombType.LINE_V);
                    }
                    break;
                case 2: //LINE_H
                    for (int j = 0; j < bombProb[i]; j++)
                    {
                        listBombProb.Add(BlockBombType.LINE_H);
                    }
                    break;
            }
        }

        if (bombProbCount == 0)
        {
            bombProbCount = 3;
            listBombProb.Add(BlockBombType.BOMB);
            listBombProb.Add(BlockBombType.LINE_V);
            listBombProb.Add(BlockBombType.LINE_H);
        }

        //폭탄 생성
        for (int i = 0; i < bombCount; i++)
        {
            index = Random.Range(0, listBlock.Count);

            if (listBlock[index] != null)
            {
                listPangBlock.Add(listBlock[index]);
                listBlock.RemoveAt(index);

                listPangBlock[i].colorType = BlockColorType.NONE;
                listPangBlock[i].IsSkipPang = true;

                int randomLine = Random.Range(0, bombProbCount);

                listPangBlock[i].bombType = listBombProb[randomLine];

                StartCoroutine(CoBombPang(listPangBlock[i], () => { pangCheck++; }));
                yield return null;
            }
        }

        yield return new WaitUntil(() => pangCheck >= bombCount);

        BlockBomb._liveCount--;
    }

    public IEnumerator CoBombPang(BlockBase pangBlock, UnityAction callback)
    {
        pangBlock.RemoveLinkerNoReset();
        pangBlock.JumpBlock();
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));

        pangBlock.IsSkipPang = false;
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));

        callback();
        yield return null;
    }

    public void AddSpaceShipExit(SpaceShipExit spaceShipExit)
    {
        listSpaceShipExit.Add(spaceShipExit);
    }

    #endregion

    #region 하트
    public IEnumerator CoMoveHeart()
    {
        if(listHeart.Count == 0)
            yield break;

        //하트 기믹 상태 초기화
        for (int i = 0; i < listHeart.Count; i++)
        {
            listHeart[i].InitHeartMoveState();
        }

        //하트 이동 재귀 함수 실행
        yield return CoMoveHeartLoop();
    }

    private IEnumerator CoMoveHeartLoop()
    {
        //하트 이동 중, 하트가 골에 도달해 리스트에서 제거될 수도 있으므로 tempList로 검사
        List<BlockHeart> tempList = new List<BlockHeart>(listHeart);

        //하트 기믹 상태 설정 & 하트 이동
        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i] != null && tempList[i].MoveState != BlockHeart.HeartMoveState.END)
            {
                tempList[i].SetHeartMoveState();
                yield return tempList[i].CoMoveAction();
            }
        }

        //이동한 하트가 하나라도 있다면, 재귀
        if (listHeart.FindIndex(x => x.MoveState == BlockHeart.HeartMoveState.MOVE) != -1)
        {   
            yield return CoMoveHeartLoop();
        }
    }

    public void AddHeart(BlockHeart blockHeart)
    {
        listHeart.Add(blockHeart);
    }

    public void AddHeartHome(BlockHeartHome blockHeartHome)
    {
        listHeartHome.Add(blockHeartHome);
    }

    public void RemoveHeart(BlockHeart blockHeart)
    {
        listHeart.Remove(blockHeart);
        BlockColorType colorType = blockHeart.colorType;
        if (dicHeart.ContainsKey(colorType) == true)
        {
            dicHeart[colorType].Remove(blockHeart);
        }
    }

    public void RemoveHeartHome(BlockHeartHome blockHeartHome)
    {
        listHeartHome.Remove(blockHeartHome);
    }

    public BlockHeart GetHeartByHeartIndex(int _heartIndex)
    {
         return listHeart.Find(x => x.heartIndex == _heartIndex);
    }

    public bool FindHeartWay_GimmickInfo(int heartIndex, int inX, int inY)
    {
        int gimmickIndex = stageInfo.listBlockInfo.FindIndex(x => x.gimmickType == (int)BlockType.HEART);

        if (gimmickIndex != -1)
        {
            if (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] is GimmickInfo_BlockHeart == false)
                return false;

            List<HeartWayInfo> tempInfolist = (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] as GimmickInfo_BlockHeart).listHeartWayInfo;
            if (tempInfolist != null && tempInfolist.Count > 0)
            {
                int tempInfoindex = tempInfolist.FindIndex(x => x.heartIndex == heartIndex);
                if (tempInfoindex > -1 && tempInfolist[tempInfoindex].listHeartWayData != null && tempInfolist[tempInfoindex].listHeartWayData.Count > 0)
                {
                    return tempInfolist[tempInfoindex].listHeartWayData.Exists(x => x.indexX == inX && x.indexY == inY);
                }
            }
            return false;
        }
        return false;
    }

    public void AddHeartWay_GimmickInfo(int heartIndex, int wayCount, int inX, int inY)
    {
        int gimmickIndex = stageInfo.listBlockInfo.FindIndex(x => x.gimmickType == (int)BlockType.HEART);

        if(gimmickIndex == -1)
        {
            GimmickInfo_BlockHeart tempHeartGimmickInfo = new GimmickInfo_BlockHeart();
            tempHeartGimmickInfo.gimmickType = (int)BlockType.HEART;

            List<HeartWayData> tempDatalist = new List<HeartWayData>();
            tempDatalist.Add(new HeartWayData(inX, inY, wayCount));

            tempHeartGimmickInfo.listHeartWayInfo.Add(new HeartWayInfo(heartIndex, tempDatalist));

            stageInfo.listBlockInfo.Add(tempHeartGimmickInfo);
        }
        else
        {
            if (stageInfo.listBlockInfo[gimmickIndex] is GimmickInfo_BlockHeart == false)
                return;

            List<HeartWayInfo> tempInfolist = (stageInfo.listBlockInfo[gimmickIndex] as GimmickInfo_BlockHeart).listHeartWayInfo;
            int tempInfoindex = tempInfolist.FindIndex(x => x.heartIndex == heartIndex);
            if(tempInfoindex == -1)
            {
                List<HeartWayData> tempDatalist = new List<HeartWayData>();
                tempDatalist.Add(new HeartWayData(inX, inY, wayCount));

                tempInfolist.Add(new HeartWayInfo(heartIndex, tempDatalist));
            }
            else
            {
                List<HeartWayData> tempDatalist = tempInfolist[tempInfoindex].listHeartWayData;

                //해당 길 번호가 이미 있는 경우 제거
                int removeIndex = tempDatalist.FindIndex(x => x.wayCount == wayCount);
                if (removeIndex > -1) tempDatalist.RemoveAt(removeIndex);

                tempDatalist.Add(new HeartWayData(inX, inY, wayCount));
            }
        }
    }

    public bool RemoveHeartWay_GimmickInfo(int heartIndex, int inX = -1, int inY = -1)
    {
        int gimmickIndex = stageInfo.listBlockInfo.FindIndex(x => x.gimmickType == (int)BlockType.HEART);

        if (gimmickIndex != -1)
        {
            if (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] is GimmickInfo_BlockHeart == false)
                return true;

            List<HeartWayInfo> tempInfolist = (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] as GimmickInfo_BlockHeart).listHeartWayInfo;
            if (tempInfolist != null && tempInfolist.Count > 0)
            {
                if(heartIndex == -1)
                {
                    for(int i = 0; i < tempInfolist.Count; i++)
                    {
                        int removeIndex = tempInfolist[i].listHeartWayData.FindIndex(x => x.indexX == inX && x.indexY == inY);
                        if (removeIndex > -1)
                        {
                            tempInfolist[i].listHeartWayData.RemoveAt(removeIndex);
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    int tempInfoindex = tempInfolist.FindIndex(x => x.heartIndex == heartIndex);
                    if (tempInfoindex > -1)
                    {
                        if (inX == -1 && inY == -1)
                        {
                            //해당 하트 인덱스 관련 데이터 전부 삭제
                            tempInfolist.RemoveAt(tempInfoindex);
                        }
                        else
                        {
                            //해당 위치에 있는 하트 길 하나만 삭제
                            List<HeartWayData> tempDatalist = tempInfolist[tempInfoindex].listHeartWayData;
                            if(tempDatalist != null && tempDatalist.Count > 0)
                            {
                                int removeIndex = tempDatalist.FindIndex(x => x.indexX == inX && x.indexY == inY);
                                if (removeIndex > -1)
                                {
                                    tempDatalist.RemoveAt(removeIndex);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    private void InitHeart()
    {
        //하트 리스트를 인덱스 순으로 정렬
        listHeart.Sort((BlockHeart a, BlockHeart b) => a.heartIndex.CompareTo(b.heartIndex));

        //하트 길 설정하기
        InitHeartWay();

        dicHeart.Clear();
        for (int i = 0; i < listHeart.Count; i++)
        {
            //컬러별로 딕셔너리 추가
            BlockColorType colorType = listHeart[i].colorType;
            if (dicHeart.ContainsKey(colorType) == false)
                dicHeart.Add(colorType, new List<BlockHeart>());
            dicHeart[colorType].Add(listHeart[i]);

            //양털 확인
            listHeart[i].CheckCarpet();
        }

        //같은 컬러 타입 하트 확인하기
        foreach (var item in dicHeart)
        {
            List<BlockHeart> listHeart = item.Value;
            for (int i = 0; i < listHeart.Count; i++)
            {
                bool isInActiveHeart = (i == 0);
                StartCoroutine(listHeart[i].CoSetActiveHeartImage_ByColorType(isInActiveHeart, false));
            }
        }
    }

    public void InitHeartWay()
    {
        //하트별 길 데이터 생성
        for (int i = 0; i < listHeart.Count; i++)
        {
            listHeart[i].MakeHeartWay();
        }

        RefreshHeartHome();
        if (GameManager.instance.state == GameState.EDIT)
        {
            MakeGuide_Edit();
        }
    }

    private void MakeGuide_Edit()
    {
        EditManager.instance.SetWayCountByBlockIndex();
        BlockType bType = BlockType.HEART;
        EditManager.instance.RemoveGimmickGuide(bType);
        for (int i = 0; i < listHeart.Count; i++)
        {
            listHeart[i].MakeGuide_Edit();
        }
    }

    //다음 하트 활성화
    public void SetActiveNextHeart(BlockColorType colorType)
    {
        if (dicHeart.ContainsKey(colorType) && dicHeart[colorType].Count > 0)
            StartCoroutine(dicHeart[colorType][0].CoSetActiveHeartImage_ByColorType(true, true));
    }

    public void AddGaugeHeartColor(BlockColorType colorType)
    {
        //해당 컬러타입 하트 기믹 리스트 찾아서 카운트 더해주기
        if (dicHeart.ContainsKey(colorType) == true)
        {
            if (dicHeart[colorType].Count > 0)
                dicHeart[colorType][0].AddMoveGaugeCount(1);
        }
    }

    public void RefreshHeartHome()
    {
        InstallHeartHome();
        SetHeartHomeIndex();
    }

    public void InstallHeartHome()
    {
        //하트 기믹 정보 전체 삭제
        for (int i = 0; i < listHeart.Count; i++)
        {
            listHeart[i].RemoveHeartHome(false);
        }

        //하트 기믹 전체 순환 하트 끝 기믹 설치
        for (int i = 0; i < listHeart.Count; i++)
        {
            listHeart[i].MakeHeartHome_LastHeartWay();
        }
    }

    public void SetHeartHomeIndex()
    {
        if(listHeartHome.Count > 0)
        {
            //하트 끝 기믹 하트 인덱스 리스트 전체 삭제
            for (int i = 0; i < listHeartHome.Count; i++)
            {
                listHeartHome[i].ClearListHeart();
            }

            //하트 끝 기믹 하트 인덱스 리스트 넣기
            for (int i = 0; i < listHeart.Count; i++)
            {
                HeartWayData way = listHeart[i].GetLastHeartWay();
                if(way != null)
                {
                    BlockHeartHome homeTemp = listHeartHome.Find(x => x.indexX == way.indexX && x.indexY == way.indexY);
                    if(homeTemp != null)
                        homeTemp.AddHeartIndex(listHeart[i].heartIndex);
                }
            }
        }
    }

    public void RemoveHeartHomeInfo(int indexX, int indexY)
    {
        BlockInfo removeInfo = ManagerBlock.instance.GetBlockInfo(indexX, indexY);

        if (removeInfo != null && removeInfo.type == (int)BlockType.HEART_HOME)
        {
            removeInfo.type = (int)BlockType.NORMAL;
            removeInfo.colorType = (int)BlockColorType.RANDOM;
            removeInfo.index = 0;
            removeInfo.count = 0;
            removeInfo.subType = 0;
        }
    }

    #endregion

    #region 코인스테이지
    public enum COINSTATE
    {
        NORMAL,
        FEVER,
    }

    public enum FEVERTYPE
    {
        COLOR,
        BOMBCOUNT,
    }

    public enum CoinStageNpcState
    {
        APPEAR, //시작
        IDLE,
        Fever,
        Clear,
    }

    COINSTATE coinState = COINSTATE.NORMAL;
    BlockColorType FeverBlockColor = BlockColorType.A;
    float FeverTime = 0f;
    float feverBlockCount = 0f;

    float FEVER_TIME_LENGTH = 5f;  //피버타임시간
    float FEVER_BLOCK_COUNT = 50f;    //피버발동 블럭 갯수
    int colorTimePeriod = 5;        //효과속도

    void SetCoinStage()
    {
        if (GameManager.gameMode != GameMode.COIN)
            return;

        feverBlockCount = 0f;
        FeverTime = 0f;
        coinState = COINSTATE.NORMAL;

        FEVER_TIME_LENGTH = (float)ManagerBlock.instance.stageInfo.feverTime;
        FEVER_BLOCK_COUNT = (float)ManagerBlock.instance.stageInfo.feverCount;

        //이미배치된 블럭에 코인넣기
        for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
            for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
                if (PosHelper.GetBoard(j, i) != null
                    && PosHelper.GetBoard(j, i).Block != null
                    && PosHelper.GetBoard(j, i).Block.IsNormalBlock())
                {
                    int coinProp = Random.Range(0, 1000);
                    if (coinProp <= ManagerBlock.instance.stageInfo.coinProp)
                    {
                        PosHelper.GetBoard(j, i).Block.hasCoin = true;
                        PosHelper.GetBoard(j, i).Block.AddCoin();
                    }
                }
    }

    /// <summary>
    /// 엔드 컨텐츠 스테이지 세팅 (스테이지에 엔드 컨텐츠 블럭이 없다면 3개 생성)
    /// </summary>
    /// <returns></returns>
    private void SetEndContentsStage()
    {
        if (Global.GameType != GameType.END_CONTENTS)
            return;
        MakeBlockRandomPos(3, BlockType.ENDCONTENTS_ITEM, 5, false);
        GameUIManager.instance.SetEndContents_Editor();
    }
    
    public void MakeBlockRandomPos(int maxCount, BlockType type, int blockCount, bool isMakeActiveBoard = true)
    {
        List<Board> listCanMakeBoard = new List<Board>();
        
        int bombCheckCount = 9;
        int minX = GameManager.MIN_X + 1;
        int minY = GameManager.MIN_Y + 2;
        int maxX = minX + bombCheckCount - 1;
        int maxY = minY + bombCheckCount;

        foreach (var item in boards)
        {
            if (item.Block != null)
                continue;
            if (item.IsActiveBoard != isMakeActiveBoard)
                continue;
            if (item.indexX < minX || item.indexY < minY || item.indexX > maxX || item.indexY > maxY)
                continue;
            if (!boards[item.indexX + 1, item.indexY].IsActiveBoard && !boards[item.indexX - 1, item.indexY].IsActiveBoard &&
                !boards[item.indexX, item.indexY + 1].IsActiveBoard && !boards[item.indexX, item.indexY - 1].IsActiveBoard)
                continue;


            listCanMakeBoard.Add(item);
        }

        int makeCount = maxCount;
        if (maxCount < listCanMakeBoard.Count)
            GenericHelper.Shuffle(listCanMakeBoard);
        else if (maxCount > listCanMakeBoard.Count)
            makeCount = listCanMakeBoard.Count;

        for (int i = 0; i < makeCount; i++)
        {
            Board board = listCanMakeBoard[i];
            board.Block = BlockMaker.instance.MakeBlockBase(board.indexX, board.indexY, type, BlockColorType.NONE, blockCount);
        }
    }

    public void CheckFeverBlock(BlockColorType tempColor)
    {
        if (GameManager.gameMode != GameMode.COIN)
            return;

        if (coinState == COINSTATE.FEVER)
            return;

        if (GameManager.instance.state == GameState.GAMECLEAR || GameManager.instance.state == GameState.GAMEOVER)
            return;

        feverBlockCount++;
        GameUIManager.instance.SetFeverGaige(feverBlockCount / FEVER_BLOCK_COUNT);

        if (feverBlockCount >= FEVER_BLOCK_COUNT)
        {
            coinState = COINSTATE.FEVER;
            GameUIManager.instance.SetFeverMode(true);
            FeverTime = FEVER_TIME_LENGTH;

            for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
                for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
                    if (PosHelper.GetBoard(j, i) != null
                        && PosHelper.GetBoard(j, i).Block != null && PosHelper.GetBoard(j, i).Block.IsCanAddCoin() == true)
                    {
                        PosHelper.GetBoard(j, i).Block.AddCoin();
                    }
        }
    }

    void UpdateCoinStage()
    {
        if (GameManager.gameMode != GameMode.COIN)
            return;

        //게임오버일때체크
        if (coinState == COINSTATE.FEVER)
        {
            FeverTime -= Global.deltaTimePuzzle;
            float color = Mathf.Abs(Mathf.Sin(Mathf.PI * (FEVER_TIME_LENGTH - FeverTime) * colorTimePeriod));
            GameUIManager.instance.SetFeverGaige(FeverTime / FEVER_TIME_LENGTH, color);

            if (GameManager.instance.state == GameState.GAMECLEAR || GameManager.instance.state == GameState.GAMEOVER)
                FeverTime = 0f;

            if (FeverTime <= 0f)
            {
                coinState = COINSTATE.NORMAL;
                GameUIManager.instance.SetFeverGaige(0f);
                GameUIManager.instance.SetFeverMode(false);
                FeverTime = 0f;
                feverBlockCount = 0;

                for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
                {
                    for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
                    {
                        if (PosHelper.GetBoard(j, i) != null && PosHelper.GetBoard(j, i).Block != null)
                        {
                            PosHelper.GetBoard(j, i).Block.AddCoin(true);
                        }
                    }
                }
                BlockMatchManager.instance.SetBlockLink();
            }
        }
    }

    public bool isFeverTime()
    {
        if (GameManager.gameMode != GameMode.COIN || coinState == COINSTATE.NORMAL)
            return false;

        return true;
    }

    #endregion

    #region 월드랭킹
    private void SetUI_WorldRank()
    {
        //월드랭킹 기믹이 설치되어 있는지 검사.
        foreach (var board in ManagerBlock.boards)
        {
            if (board.Block != null && board.Block.type == BlockType.WORLD_RANK_ITEM)
            {
                GameUIManager.instance.SetWorldRanking_Editor(true);
                return;
            }
        }
        GameUIManager.instance.SetWorldRanking_Editor(false);
    }

    public void AddWorldRank(BlockWorldRankItem blockWorldRank)
    {
        listWorldRankItem.Add(blockWorldRank);
    }

    #endregion

    #region 생성기

    //생성기 값 초기화
    public void InitBlockGenerator()
    {
        GimmickInfo_BlockGenerator generatorInfo = GeBlockGeneratorInfo_AtDecoInfo();

        if (generatorInfo == null)
        {
            BlockGeneratorNoInitSprite();
            return;
        }

        //stageinfo에서 등록된 데이터로 데이터 설정해줌
        foreach (var tempList in dicBlockGenerator)
        {
            int infoIndex = generatorInfo.listImageInfo.FindIndex(x => x.generatorIndex == tempList.Key);

            List<BlockGenerator> listBlockGenerator = tempList.Value;
            List<BlockAndColorData> listImageInfoData = (infoIndex == -1) ?
                null : generatorInfo.listImageInfo[infoIndex].listBlockAndColorData;

            for (int i = 0; i < listBlockGenerator.Count; i++)
            {
                listBlockGenerator[i].InitStartBlockSprite(listImageInfoData);
            }
        }
    }

    //생성기 기믹 물음표 확인
    public void BlockGeneratorNoInitSprite()
    {
        foreach (var tempList in dicBlockGenerator)
        {
            List<BlockGenerator> listBlockGenerator = tempList.Value;

            for (int i = 0; i < listBlockGenerator.Count; i++)
            {
                listBlockGenerator[i].InitStartBlockSprite(null);
            }
        }
    }

    //생성기 기믹 비활성화 확인
    public void BlockGeneratorImageCheck()
    {
        foreach(var tempList in dicBlockGenerator)
        {
            List<BlockGenerator> listBlockGenerator = tempList.Value;

            for (int i = 0; i < listBlockGenerator.Count; i++)
            {
                listBlockGenerator[i].RefreshImageList();
            }
        }
    }

    //스테이지 정보의 listDecoInfo 에서 생성기 정보 가져오기
    public GimmickInfo_BlockGenerator GeBlockGeneratorInfo_AtDecoInfo()
    {
        int gimmickIndex = ManagerBlock.instance.stageInfo.listDecoInfo.FindIndex(x => x.gimmickType == (int)BoardDecoType.BLOCK_GENERATOR);
        if (gimmickIndex == -1)
            return null;

        if (ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] is GimmickInfo_BlockGenerator == false)
            return null;

        //가져온 생성기 정보에서, 현재 선택되어 있는 인덱스의 생성기 정보만 가져오기.
        GimmickInfo_BlockGenerator generatorInfo = ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] as GimmickInfo_BlockGenerator;
        return generatorInfo;
    }

    #endregion

    #region 페인트
    public void AddInkPaint(BlockPaint paint)
    {
        listPaint.Add(paint);
    }

    public void RemovePaint(BlockPaint paint)
    {
        listPaint.Remove(paint);
    }
    #endregion

    #region 물폭탄
    public void AddWaterBomb(BlockWaterBomb waterBomb)
    {
        listWaterBomb.Add(waterBomb);
    }

    public void RemoveWaterBomb(BlockWaterBomb waterBomb)
    {
        listWaterBomb.Remove(waterBomb);
    }
    #endregion

    #region 빵 기믹

    public void AddBlockBreadInDictionary(int groupIndex, BlockBread blockBread)
    {
        if (dicBlockBread.ContainsKey(groupIndex) == false)
        {
            List<BlockBread> listBlockBread = new List<BlockBread>();
            listBlockBread.Add(blockBread);
            ManagerBlock.instance.dicBlockBread.Add(groupIndex, listBlockBread);
        }
        else
        {
            List<BlockBread> listBlockBread = ManagerBlock.instance.dicBlockBread[groupIndex];
            if (listBlockBread == null)
            {
                listBlockBread = new List<BlockBread>();
                listBlockBread.Add(blockBread);
                ManagerBlock.instance.dicBlockBread[groupIndex] = listBlockBread;
            }
            else
            {
                if (listBlockBread.Contains(blockBread) == false)
                    ManagerBlock.instance.dicBlockBread[groupIndex].Add(blockBread);
            }
        }
    }

    public void PangBlockBread(int groupIndex)
    {
        if (dicBlockBread.ContainsKey(groupIndex) == false)
            return;

        List<BlockBread> listBread = ManagerBlock.instance.dicBlockBread[groupIndex];
        for (int i = 0; i < listBread.Count; i++)
        {
            listBread[i].PangBlockBread();
        }
        ManagerBlock.instance.dicBlockBread.Remove(groupIndex);
    }

    //빵기믹이 가지고 있던 팡인덱스 리스트를 초기화
    public void ResetListPangIndexBlockBread()
    {
        foreach (var item in dicBlockBread)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].ResetPangIndexList();
            }
        }
    }
    #endregion

    #region 대포

    private void AddCannonCount(BlockColorType colorType)
    {
        if (listCannonItem.Count == 0)
            return;
        
        if (GameManager.instance.state == GameState.GAMECLEAR)
            return;

        List<BlockCannon> tempList = new List<BlockCannon>();
        
        foreach (var item in listCannonItem)
        {
            // 대포 기본 체크 : 1. null체크, 2. 낙엽 체크
            if (item == null || boards[item.indexX, item.indexY] == null || CannonBoardHasClover(item))
                continue;
            
            // 대포 색상 체크 : 색상 타입이 있는 대포 기믹의 경우 해당 색상 제거 시에만 카운트 진행
            if (item.targetBlockType != BlockColorType.NONE && item.targetBlockType != colorType)
                continue;
            
            tempList.Add(item);
        }

        foreach (var item in tempList)
            item.SetCount(1);
    }

    private bool CannonBoardHasClover(BlockCannon item)
    {
        foreach (var deco in boards[item.indexX, item.indexY].DecoOnBoard)
            if (deco is Clover) return true;
        return false;
    }

    private IEnumerator CoCheckCannon(BlockCannon cannon)
    {
        if(listCannonItem.Count == 0)
            yield break;

        if (cannon != null)
        {
            yield return new WaitUntil(() => cannon == null || cannon.state != BlockState.PANG);
            yield return new WaitUntil(() => checkBlockWait());
        }

        if (GameManager.instance.checkAllRemoveTarget())
            yield break;

        BlockCannon tempCannon = null;
        bool isLoop = false;
        for (int i = 0; i < listCannonItem.Count; i++)
        {
            if (listCannonItem[i].CheckPang())
            {
                isLoop = true;
                tempCannon = listCannonItem[i];
            }
        }

        if (isLoop)
            yield return CoCheckCannon(tempCannon);
    }
    
    public void AddCannon(BlockCannon cannon)
    {
        listCannonItem.Add(cannon);
    }

    public void RemoveCannon(BlockCannon cannon)
    {
        listCannonItem.Remove(cannon);
    }

    #endregion
    
    #region 노이 부스팅 이벤트

    private IEnumerator NoyBoostingEvent ()
    {
        if (Global.GameType != GameType.NORMAL)
            yield break;
        
        if (EditManager.instance.boostStep == 0)
            yield break;
        
        if (ManagerTutorial._instance != null)
            yield break;

        if (isNoyBoostBomb)
            yield break;

        isNoyBoostBomb = true;
        // 랜덤 보드 지정
        List<Board> listAllBoard = PosHelper.GetAllPangBoardList();
        List<Board> listBaseBoard = new List<Board>();
        if (listAllBoard.Count > 0)
        {
            int cnt = 0;
            while (cnt < EditManager.instance.boostStep && state == BlockManagrState.EVENT)
            {
                int index = GameManager.instance.GetIngameRandom(0, listAllBoard.Count);
                var baseBlock = listAllBoard[index];
                // 폭발 범위가 완전히 겹치는 일이 없도록 중복처리 (단, 블럭 수가 카운트보다 작으면 겹칠 수 있도록 처리 (ex. 블럭은 3개 남아있는데, 부스팅 카운트가 4라면 어쩔 수 없이 3개에 터질 수 밖에 없음))
                if (listBaseBoard.Contains(PosHelper.GetBoard(baseBlock.indexX, baseBlock.indexY)) && listAllBoard.Count > listBaseBoard.Count) continue;
                listBaseBoard.Add(PosHelper.GetBoard(baseBlock.indexX, baseBlock.indexY));
                cnt++;
            }
        }

        // [연출] 별똥별 이펙트 생성
        List<SkeletonAnimation> fxList = new List<SkeletonAnimation>();
        for (int i = 0; i < listBaseBoard.Count; i++)
        {
            //위치 설정
            Vector3 basePos = PosHelper.GetPosByIndex(listBaseBoard[i].indexX, listBaseBoard[i].indexY) + GameUIManager.instance.groundAnchor.transform.localPosition;
            GameObject fx_Stardust = InGameEffectMaker.instance.MakeBoostStardustEffect(new Vector3(basePos.x + 39f, basePos.y - 39f, 0));
            SkeletonAnimation skleton_Fx = fx_Stardust.GetComponentInChildren<SkeletonAnimation>();
            if (skleton_Fx != null) fxList.Add(skleton_Fx);
        }
        yield return new WaitForSeconds(0.1f);

        // [연출] 0.1초 후 마법사 노이 등장
        var efNoy = InGameEffectMaker.instance.MakeBoostNoyEffect();
        
        // [연출] 이벤트 설정 별 사운드 재생 및 애니메이션 변경
        efNoy.GetComponentInChildren<SkeletonAnimation>().AnimationState.Event += (t, e) =>
        {
            if (e.Data.Name == "sound")
                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            if (e.Data.Name == "boom")
            {
                foreach (var item in fxList)
                    item.AnimationName = "ef_boom";
            }
        };

        yield return new WaitForSeconds(0.733f);
        // [연출] 별똥별 생성 0.733초 (ef_appear 재생 시간) 후 별똥별 idle로 변경
        foreach (var item in fxList)
            item.AnimationName = "ef_idle";
        yield return new WaitForSeconds(1.0f);
        
        // [연출] 별똥별 생성 1.733초 후 이펙트 제거
        foreach (var item in fxList)
            Destroy(item);
        Destroy(efNoy);

        // 블럭 폭발
        BlockBomb._bombUniqueIndex++;
        bool isComplete = false;
        Remove2X2Block(listBaseBoard, () => { isComplete = true; });

        yield return new WaitUntil(() => isComplete);
        yield return new WaitUntil(() => checkBlockWait());
    }

    // 부스팅 이벤트 블럭 폭발 및 제거
    private void Remove2X2Block(List<Board> listBaseBoard, System.Action isComplete)
    {
        for (int i = 0; i < listBaseBoard.Count; i++)
        {
            Board board0 = listBaseBoard[i];
            // 해당 영역 (baseBlock 기준으로 2X2) 리스트 지정
            List<Board> boardList = new List<Board>();
            boardList.Add(board0);
            boardList.Add(PosHelper.GetBoard(board0.indexX + 1, board0.indexY));
            boardList.Add(PosHelper.GetBoard(board0.indexX, board0.indexY + 1));
            boardList.Add(PosHelper.GetBoard(board0.indexX + 1, board0.indexY + 1));

            // 폭탄 영역 표시
            List<BombAreaInfo> infoList = GetBombAreaInfoList(BlockBomb.BombShape.Rect, board0.indexX, board0.indexY, 2, 2);
            InGameEffectMaker.instance.MakePangFieldEffect_PowerBomb(board0.indexX, board0.indexY, board0.indexX + 1, board0.indexY + 1, infoList, BlockBomb._bombUniqueIndex, BlockColorType.NONE, dTime : 0.08f);

            bool isCompleteMakeCarpet = false;
            foreach (var board in boardList)
            {
                if (board != null)
                {
                    //블럭 가리는 데코 기믹 먼저 체크 > 그 다음에 블럭 체크
                    if (board.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex)) continue;
                    if (board.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex)) continue;
                    if (board.Block != null)
                        board.Block.BlockPang(BlockBomb._bombUniqueIndex, BlockColorType.NONE, true);
                    
                    // 데코 관련 처리
                    foreach (DecoBase boardDeco in board.DecoOnBoard)
                    {
                        // 양털 증식
                        if (!isCompleteMakeCarpet && boardDeco is Carpet)
                        {
                            foreach (var _board in boardList)
                                if (_board != null && _board.Block != null && _board.Block.IsCanMakeCarpetByBomb())
                                    _board.MakeCarpet();

                            isCompleteMakeCarpet = true;
                        }

                        // 블럭은 없으나 빈 보드에 낙엽 등이 있을 경우 데코 제거
                        if (boardDeco is IHide && board.Block == null)
                            board.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex);
                        // 블럭은 없으나 빈 보드에 물, 랜덤박스 등이 있을 경우 데코 제거
                        else if (boardDeco is INet && board.Block == null)
                            board.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex);
                        // 울타리 데코 제거
                        if (boardDeco is GrassFenceBlock)
                        {
                            GrassFenceBlock fence = boardDeco as GrassFenceBlock;
                            if (!board.HasDecoHideBlock())
                            {
                                if (fence.inX == board0.indexX && fence.direction == BlockDirection.RIGHT)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inX == board0.indexX + 1 && fence.direction == BlockDirection.LEFT)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inY == board0.indexY && fence.direction == BlockDirection.DOWN)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inY == board0.indexY + 1 && fence.direction == BlockDirection.UP)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                            }
                        }

                        if (boardDeco is FenceBlock)
                        {
                            FenceBlock fence = boardDeco as FenceBlock;
                            if (!board.HasDecoHideBlock())
                            {
                                if (fence.inX == board0.indexX && fence.direction == BlockDirection.RIGHT)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inX == board0.indexX + 1 && fence.direction == BlockDirection.LEFT)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inY == board0.indexY && fence.direction == BlockDirection.DOWN)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                                if (fence.inY == board0.indexY + 1 && fence.direction == BlockDirection.UP)
                                    fence.SetDirectionPang(board, 0, 0, 0, fence.direction);
                            }
                        }
                    }
                }
            }

            // 음향 및 이펙트 재생
            ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);
            Vector3 basePos = PosHelper.GetPosByIndex(listBaseBoard[i].indexX, listBaseBoard[i].indexY) + GameUIManager.instance.groundAnchor.transform.localPosition;
            InGameEffectMaker.instance.MakeEffectCircleBombByPosition(new Vector3(basePos.x + 39f, basePos.y - 39f, 0), 0.0f);
            List<(Vector2, BlockDirection)> shakePosList = new List<(Vector2, BlockDirection)>()
            {
                (new Vector2(-1, -1), BlockDirection.UP_LEFT), (new Vector2(0, -1), BlockDirection.UP), (new Vector2(1, -1), BlockDirection.UP), (new Vector2(2, -1), BlockDirection.UP_RIGHT),     // 상단 블럭 4종
                (new Vector2(-1, 0), BlockDirection.LEFT), (new Vector2(-1, 1), BlockDirection.LEFT), (new Vector2(2, 0), BlockDirection.RIGHT), (new Vector2(2, 1), BlockDirection.RIGHT),         // 좌측 블럭 2종, 우측 블럭 2종
                (new Vector2(-1, 2), BlockDirection.DOWN_LEFT), (new Vector2(0, 2), BlockDirection.DOWN), (new Vector2(1, 2), BlockDirection.DOWN), (new Vector2(2, 2), BlockDirection.DOWN_RIGHT), // 하단 블럭 4종
            };
            foreach (var item in shakePosList)
            {
                if (PosHelper.GetBlockScreen(board0.indexX, board0.indexY, (int)item.Item1.x, (int)item.Item1.y) != null)
                    PosHelper.GetBlockScreen(board0.indexX, board0.indexY, (int)item.Item1.x, (int)item.Item1.y).BombShakeEffect((int)item.Item2 - 1);
            }
        }
        isComplete?.Invoke();
    }

    #endregion

    /// <summary>
    /// 설치된 블럭을 다른 타입의 블럭으로 교체
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoChangeBlockAnotherType(bool isAction, BlockBase baseBlock, BlockType type, BlockColorType blockColorType, int count = 0, int index = 0, int subType = 0, List<DecoInfo> decoList = null, System.Action endAction = null)
    {
        bool isSpecialEventBlock = baseBlock.IsSpecialEventBlock;
        if (isAction == true)
        {
            yield return baseBlock.CoActionChangeBlock_Destroy();
        }
        baseBlock.DestroyBlockData();
        baseBlock.PangDestroyBoardData();

        BlockBase changeBlock = BlockMaker.instance.MakeBlockBase(baseBlock.indexX, baseBlock.indexY, type, blockColorType, count, index, subType, decoList);
        ManagerBlock.boards[baseBlock.indexX, baseBlock.indexY].Block = changeBlock;
        if (isSpecialEventBlock == true)
            changeBlock.AddSpecialEventSprite(isMakeStartBoard: false);

        if (isAction == true)
        {
            yield return changeBlock.CoActionChangeBlock_Appear();
        }
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));
        endAction.Invoke();
    }

    #region 맵에서 사용중인 기믹 정보 저장
    public UseGimmickData GetUseGimmickData()
    {
        UseGimmickData gimmickData = new UseGimmickData();

        //출발에서 생성될 수 있는 블럭 정보
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            int blockType = startInfo.type;
            if (startInfo.probability == 0)
                continue;
            if (isColorIce(startInfo.iceProb))
                AddGimmickDataByDecoType(gimmickData, (int)BoardDecoType.ICE);
            AddGimmickDataByBlockType(gimmickData, blockType);
        }

        //사용하고 있는 블럭/데코 정보
        foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
        {
            //블럭 정보 추가
            AddGimmickDataByBlockType(gimmickData, blockInfo.type, blockInfo.subType, blockInfo.bombType);

            //데코 정보 추가
            foreach (DecoInfo decoInfo in blockInfo.ListDeco)
            {
                AddGimmickDataByDecoType(gimmickData, decoInfo.BoardType);
            }
        }

        //모드에 따른 데코 정보 추가
        if ((GameMode)stageInfo.gameMode == GameMode.LAVA)
        {
            AddGimmickDataByDecoType(gimmickData, (int)BoardDecoType.LAVA);
        }
        return gimmickData;
    }

    public void AddGimmickDataByBlockType(UseGimmickData gimmickData, int blockType, int subType = 0, int bombType = 0)
    {
        //다른 타입의 블럭을 포함할 수 있는 블럭일 수도 있기 때문에 리스트로 관리
        //(ex.식물, 바위, 흙..)
        List<BlockType> listBlockType = new List<BlockType>();

        //데코 정보
        BoardDecoType subDecoType = BoardDecoType.NONE;

        switch ((BlockType)blockType)
        {
            #region 식물
            case BlockType.PLANT:
                listBlockType.Add(BlockType.PLANT);
                listBlockType.Add(ConvertSubtypeToBlockType_Plant((PLANT_TYPE)subType));
                subDecoType = ConvertSubtypeToBoardDecoType_Plant((PLANT_TYPE)subType);
                break;

            case BlockType.PLANT_APPLE:
                listBlockType.Add(BlockType.PLANT);
                listBlockType.Add(BlockType.APPLE);
                break;

            case BlockType.PLANT_ICE_APPLE:
                listBlockType.Add(BlockType.PLANT);
                listBlockType.Add(BlockType.ICE_APPLE);
                break;

            case BlockType.PLANT_KEY:
                listBlockType.Add(BlockType.PLANT);
                listBlockType.Add(BlockType.KEY);
                break;
            #endregion

            #region 바위
            case BlockType.STONE:
                listBlockType.Add(BlockType.STONE);
                listBlockType.Add(ConvertSubtypeToBlockType_Stone((STONE_TYPE)subType));
                subDecoType = ConvertSubtypeToBoardDecoType_Stone((STONE_TYPE)subType);
                break;
            #endregion

            #region 흙
            case BlockType.GROUND:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(ConvertSubtypeToBlockType_Ground((GROUND_TYPE)subType));
                break;

            case BlockType.GROUND_APPLE:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(BlockType.APPLE);
                break;

            case BlockType.GROUND_BOMB:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(BlockType.START_Bomb);
                listBlockType.Add(BlockType.START_Line);
                break;

            case BlockType.GROUND_ICE_APPLE:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(BlockType.ICE_APPLE);
                break;

            case BlockType.GROUND_KEY:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(BlockType.KEY);
                break;

            case BlockType.GROUND_BLOCKBLACK:
                listBlockType.Add(BlockType.GROUND);
                listBlockType.Add(BlockType.BLOCK_BLACK);
                break;
            #endregion

            #region 일반블럭(폭탄)
            case BlockType.NORMAL:
                listBlockType.Add((BlockType)blockType);
                if (bombType > 0)
                {
                    BlockBombType bType = (BlockBombType)bombType;
                    if (bType == BlockBombType.RAINBOW)
                        listBlockType.Add(BlockType.START_Rainbow);
                    else if (bType == BlockBombType.LINE_H || bType == BlockBombType.LINE_V || bType == BlockBombType.LINE)
                        listBlockType.Add(BlockType.START_Line);
                    else if (bType == BlockBombType.BOMB)
                        listBlockType.Add(BlockType.START_Bomb);
                }
                break;
            #endregion

            #region 이벤트블럭, 이벤트바위
            case BlockType.BLOCK_EVENT:
                listBlockType.Add((BlockType)blockType);
                listBlockType.Add(BlockType.PLANT);
                break;

            case BlockType.BLOCK_EVENT_STONE:
                listBlockType.Add((BlockType)blockType);
                listBlockType.Add(BlockType.STONE);
                break;
            #endregion

            default:
                listBlockType.Add((BlockType)blockType);
                break;
        }

        //블럭 타입 추가
        for (int i = 0; i < listBlockType.Count; i++)
        {
            BlockType checkBlockType = listBlockType[i];
            if (checkBlockType == BlockType.ICE && gimmickData.listUseTopDecoType.FindIndex(x => x == (int)BoardDecoType.ICE) == -1)
            {
                gimmickData.listUseTopDecoType.Add((int)BoardDecoType.ICE);
            }
            else if (checkBlockType != BlockType.NONE && gimmickData.listUseBlockType.FindIndex(x => x == (int)checkBlockType) == -1)
            {
                gimmickData.listUseBlockType.Add((int)checkBlockType);
            }
        }

        //데코 타입 추가
        if (subDecoType != BoardDecoType.NONE)
        {
            AddGimmickDataByDecoType(gimmickData, (int)subDecoType);
        }
    }

    public void AddGimmickDataByDecoType(UseGimmickData gimmickData, int decoType)
    {
        if (IsDecoType_UpperBoard(decoType) == false)
        {
            if (gimmickData.listUseDecoType.FindIndex(x => x == decoType) == -1)
                gimmickData.listUseDecoType.Add(decoType);
        }
        else
        {
            if (gimmickData.listUseTopDecoType.FindIndex(x => x == decoType) == -1)
                gimmickData.listUseTopDecoType.Add(decoType);
        }
    }

    public BlockType ConvertSubtypeToBlockType_Plant(PLANT_TYPE subType)
    {
        switch (subType)
        {
            case PLANT_TYPE.APPLE:
                return BlockType.APPLE;
            case PLANT_TYPE.KEY:
                return BlockType.KEY;
            case PLANT_TYPE.ICE_APPLE:
                return BlockType.ICE_APPLE;
            case PLANT_TYPE.CANDY:
                return BlockType.CANDY;
            case PLANT_TYPE.DUCK:
                return BlockType.DUCK;
            case PLANT_TYPE.EVENT:
                return BlockType.BLOCK_EVENT;
            case PLANT_TYPE.COIN:
                return BlockType.BLOCK_COIN;
            case PLANT_TYPE.HealPotion:
                return BlockType.ADVENTURE_POTION_HEAL;
            case PLANT_TYPE.SkillPotion:
                return BlockType.ADVENTURE_POTION_SKILL;
            case PLANT_TYPE.COIN_BAG:
                return BlockType.COIN_BAG;
            case PLANT_TYPE.SPACESHIP:
                return BlockType.SPACESHIP;
            case PLANT_TYPE.LINE:
            case PLANT_TYPE.LINE_H:
            case PLANT_TYPE.LINE_V:
                return BlockType.START_Line;
            case PLANT_TYPE.CIRCLE:
                return BlockType.START_Bomb;
            default:
                return (int)BlockType.NONE;
        }
    }

    public BoardDecoType ConvertSubtypeToBoardDecoType_Plant(PLANT_TYPE subType)
    {
        switch (subType)
        {
            case PLANT_TYPE.WATER:
                return BoardDecoType.WATER;
            default:
                return (int)BlockType.NONE;
        }
    }

    public BlockType ConvertSubtypeToBlockType_Stone(STONE_TYPE subType)
    {
        switch (subType)
        {
            case STONE_TYPE.APPLE:
                return BlockType.APPLE;
            case STONE_TYPE.KEY:
                return BlockType.KEY;
            case STONE_TYPE.CANDY:
                return BlockType.CANDY;
            case STONE_TYPE.HealPotion:
                return BlockType.ADVENTURE_POTION_HEAL;
            case STONE_TYPE.SkillPotion:
                return BlockType.ADVENTURE_POTION_SKILL;
            case STONE_TYPE.SPACESHIP:
                return BlockType.SPACESHIP;
            case STONE_TYPE.LINE:
            case STONE_TYPE.LINE_H:
            case STONE_TYPE.LINE_V:
                return BlockType.START_Line;
            case STONE_TYPE.CIRCLE:
                return BlockType.START_Bomb;
            default:
                return (int)BlockType.NONE;
        }
    }

    public BoardDecoType ConvertSubtypeToBoardDecoType_Stone(STONE_TYPE subType)
    {
        switch (subType)
        {
            case STONE_TYPE.WATER:
                return BoardDecoType.WATER;
            default:
                return (int)BlockType.NONE;
        }
    }

    public BlockType ConvertSubtypeToBlockType_Ground(GROUND_TYPE subType)
    {
        switch (subType)
        {
            case GROUND_TYPE.JEWEL:
                return BlockType.GROUND_JEWEL;
            case GROUND_TYPE.APPLE:
                return BlockType.APPLE;
            case GROUND_TYPE.KEY:
                return BlockType.KEY;
            case GROUND_TYPE.ICE_APPLE:
                return BlockType.ICE_APPLE;
            case GROUND_TYPE.LINE:
            case GROUND_TYPE.LINE_H:
            case GROUND_TYPE.LINE_V:
                return BlockType.START_Line;
            case GROUND_TYPE.CIRCLE:
                return BlockType.START_Bomb;
            case GROUND_TYPE.DUCK:
                return BlockType.DUCK;
            case GROUND_TYPE.CANDY:
                return BlockType.CANDY;
            case GROUND_TYPE.BlOCKBLACK:
                return BlockType.BLOCK_BLACK;
            default:
                return (int)BlockType.NONE;
        }
    }

    //블럭 위에 올라가는 데코 타입인지.
    private bool IsDecoType_UpperBoard(int decoType)
    {
        switch ((BoardDecoType)decoType)
        {
            case BoardDecoType.NET:
            case BoardDecoType.LAVA:
            case BoardDecoType.ICE:
            case BoardDecoType.GRASSFENCEBLOCK:
            case BoardDecoType.FENCEBLOCK:
            case BoardDecoType.RANDOM_BOX:
            case BoardDecoType.CLOVER:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region 목표관련
    /// <summary>
    /// 현재 스테이지에서 목표로 사용하는 타입인지 검사.
    /// </summary>
    public bool IsStageTarget(TARGET_TYPE targetType, BlockColorType colorType = BlockColorType.NONE)
    {
        CollectTargetCount collectTargetCount = GetCollectTargetCountData(targetType, colorType);
        if (collectTargetCount != null && collectTargetCount.collectCount > 0)
            return true;
        return false;
    }

    /// <summary>
    /// 달성해야 하는 목표가 남아있는지 검사.
    /// </summary>
    public bool HasAchievedCollectTarget(TARGET_TYPE targetType, BlockColorType colorType = BlockColorType.NONE)
    {
        CollectTargetCount collectTargetCount = GetCollectTargetCountData(targetType, colorType);
        if (collectTargetCount != null && collectTargetCount.collectCount > 0
            && (collectTargetCount.collectCount > collectTargetCount.pangCount))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 목표 카운트 반환
    /// </summary>
    public int GetCollectTargetCount(TARGET_TYPE targetType, BlockColorType colorType = BlockColorType.NONE)
    {
        CollectTargetCount collectTargetCount = GetCollectTargetCountData(targetType, colorType);
        if (collectTargetCount == null)
            return 0;
        else
            return collectTargetCount.collectCount;
    }

    /// <summary>
    /// 획득한 목표 카운트 갱신
    /// </summary>
    public void UpdateCollectTarget_PangCount(TARGET_TYPE targetType, BlockColorType colorType = BlockColorType.NONE)
    {
        CollectTargetCount collectTargetCount = GetCollectTargetCountData(targetType, colorType);
        if (collectTargetCount != null)
            collectTargetCount.pangCount++;
    }

    /// <summary>
    /// 특정 목표의 카운트 정보를 가져옴.
    /// </summary>
    public CollectTargetCount GetCollectTargetCountData(TARGET_TYPE targetType, BlockColorType colorType = BlockColorType.NONE)
    {
        if (dicCollectCount.ContainsKey(targetType) == false)
            return null;
        if (dicCollectCount[targetType].ContainsKey(colorType) == false)
            return null;

        return dicCollectCount[targetType][colorType];
    }

    public class RemainTargetInfo
    {
        public string targetName = "";
        public int remainCount = 0;
    }

    public List<RemainTargetInfo> GetListRemainTarget()
    {
        List<RemainTargetInfo> listRemainTarget = new List<RemainTargetInfo>();
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;

            if (enumerator.Current.Value != null)
            {
                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;
                    int remainCount = (e.Current.Value.collectCount > e.Current.Value.pangCount) ? e.Current.Value.collectCount - e.Current.Value.pangCount : 0;
                    if (remainCount == 0)
                        continue;
                    RemainTargetInfo info = new RemainTargetInfo()
                    {
                        targetName = (colorType != BlockColorType.NONE) ? string.Format("{0}_{1}", targetType, colorType) : targetType.ToString(),
                        remainCount = remainCount,
                    };
                    listRemainTarget.Add(info);
                }
            }
        }
        return listRemainTarget;
    }

    public void ClearStageInfo_TargetCount()
    {
        ManagerBlock.instance.stageInfo.listTargetInfo.Clear();
    }

    /// <summary>
    /// 신버전 목표 데이터에 값을 넣어주기 위해 사용하는 함수
    /// </summary>
    public void SetStageInfo_TargetCount(int targetType, int targetCount, int colorType = 0)
    {
        int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == targetType);
        if (findIndex > -1)
        {   //기존에 데이터가 있으면, 해당 데이터에 값을 덮어씌워줌.
            CollectTargetInfo info = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex];
            int colorIndex = info.listTargetColorInfo.FindIndex(x => x.colorType == colorType);
            if (colorIndex > -1)
            {
                if (targetCount == 0)   //목표의 값이 0이면 리스트에서 삭제
                    info.listTargetColorInfo.Remove(info.listTargetColorInfo[colorIndex]);
                else
                    info.listTargetColorInfo[colorIndex].collectCount = targetCount;
            }
            else
            {   //기존에 딕셔너리는 있지만 컬러에 해당하는 데이터가 없는 경우, 해당 컬러의 목표를 추가해줌.
                if (targetCount == 0)
                    return;

                TargetColorInfo newData = new TargetColorInfo()
                {
                    colorType = colorType,
                    collectCount = targetCount,
                };
                info.listTargetColorInfo.Add(newData);
            }

            //해당 타입에 남아있는 목표 카운트가 없으면, 리스트에서 삭제해줌.
            if (info.listTargetColorInfo.Count == 0)
                ManagerBlock.instance.stageInfo.listTargetInfo.Remove(info);
        }
        else
        {   //기존에 데이터가 없는 경우, 데이터를 추가해줌
            if (targetCount == 0)
                return;

            TargetColorInfo info = new TargetColorInfo()
            {
                colorType = colorType,
                collectCount = targetCount,
            };

            CollectTargetInfo targetInfo = new CollectTargetInfo();
            targetInfo.targetType = targetType;
            targetInfo.listTargetColorInfo.Add(info);

            ManagerBlock.instance.stageInfo.listTargetInfo.Add(targetInfo);
        }
    }

    //구버전 목표 데이터를 사용하고 있는 맵일 경우, 해당 목표 데이터를 신버전 데이터로 옮겨줌
    public void CopyStageInfo_TargetCount_PrevVersionToNewVersion()
    {
        //일반 타입 목표의 데이터 옮기기
        for (int i = 0; i < ManagerBlock.instance.stageInfo.collectCount.Length; i++)
        {
            if (ManagerBlock.instance.stageInfo.collectCount[i] == 0)
                continue;

            //신버전 목표 데이터에 구버전 데이터 옮기기.
            SetStageInfo_TargetCount(i, ManagerBlock.instance.stageInfo.collectCount[i]);
        }

        //컬러 타입 목표의 데이터 옮기기.(구버전에서는 컬러블럭만 사용했기 때문에 해당 데이터만 옮겨줌)
        int targetType = (int)TARGET_TYPE.COLORBLOCK;
        for (int i = 0; i < ManagerBlock.instance.stageInfo.collectColorCount.Length; i++)
        {
            if (ManagerBlock.instance.stageInfo.collectColorCount[i] == 0)
                continue;

            SetStageInfo_TargetCount(targetType, ManagerBlock.instance.stageInfo.collectColorCount[i], i);
        }

        //구버전 목표 데이터 지우기.
        ManagerBlock.instance.stageInfo.collectCount = new int[1] { 0 };
        ManagerBlock.instance.stageInfo.collectColorCount = new int[1] { 0 };
    }

    //타겟 리스트를 목표 순으로 정렬해줌.
    public void SortListTargetInfo()
    {
        //목표 순으로 정렬
        stageInfo.listTargetInfo.Sort(delegate (CollectTargetInfo a, CollectTargetInfo b)
        {
            if (a.targetType < b.targetType)
                return -1;
            else if (a.targetType > b.targetType)
                return 1;
            else
                return 0;
        });

        //컬러 순으로 정렬
        for (int i = 0; i < stageInfo.listTargetInfo.Count; i++)
        {
            stageInfo.listTargetInfo[i].listTargetColorInfo.Sort(delegate (TargetColorInfo a, TargetColorInfo b)
            {
                if (a.colorType < b.colorType)
                    return -1;
                else if (a.colorType > b.colorType)
                    return 1;
                else
                    return 0;
            });
        }
    }
    #endregion

    //특정 시간을 인게임 시간으로 나눠서 구함.
    public float GetIngameTime(float time)
    {
        return time / Global.timeScalePuzzle;
    }

    #region 폭탄 영역 관련 함수
    //블럭이 터지는 방향 알아오는 함수.
    public BlockDirection GetBombDirection(int offsetX, int offsetY)
    {
        BlockDirection bombDirection = BlockDirection.NONE;

        if (offsetY == 0)
        {
            if (offsetX > 0)
                bombDirection = BlockDirection.RIGHT;
            else
                bombDirection = BlockDirection.LEFT;
        }
        else if (offsetX == 0)
        {
            if (offsetY > 0)
                bombDirection = BlockDirection.DOWN;
            else
                bombDirection = BlockDirection.UP;
        }
        return bombDirection;
    }

    //해당 방향의 폭탄 효과가 있을 때 폭탄 영역 표시를 할 것 인지 검사
    public bool IsCanShowPangFieldByBomb(Board board, BlockDirection bombDirection, int _unique = 0, bool isAlwaysEffectOn = false)
    {
        //이미 해당 위치에 폭탄 영역 이펙트가 표시되고 있으면 표시하지 않음.
        Vector2Int checkIndex = new Vector2Int(board.indexX, board.indexY);
        if (InGameEffectMaker.instance.GetIndexPangFieldEffect(checkIndex) != -1)
            return false;

        //데코가 해당 방향의 폭발을 막는지 검사
        bool bCanPangDirection = true;
        foreach (DecoBase boardDeco in board.DecoOnBoard)
        {
            bCanPangDirection = boardDeco.IsCanPangBlockBoardIndex(board.indexX, board.indexY, bombDirection);
            if (bCanPangDirection == false)
                return false;
        }
        
        //장막 기믹이 있는 보드 검사
        if(board.BoardOnHide.Count > 0)
            return true;
        
        //잡기돌, 물 같이 블럭이 없을 수도 있는 보드 검사
        if (board.BoardOnNet.Count > 0)
            return true;

        //블럭이 없으면 표시하지 않음
        BlockBase checkBlock = board.Block;
        if (checkBlock == null)
            return false;

        //블럭이 제거될 수 없는 상태이거나, 폭탄 영역을 표시할 수 없는 기믹이라면 표시하지 않음.
        if (checkBlock.pangIndex == _unique ||
            (checkBlock.IsCanMakeBombFieldEffect() == false && isAlwaysEffectOn == false))
                return false;

        return true;
    }
    
    
    //해당 방향의 폭탄 효과가 있을 때 폭탄 영역 표시를 할 것 인지 검사 : 관통 효과 (방해 블럭 무시)
    public bool IsCanShowPangFieldByBomb_NotDisturb(Board board, BlockDirection bombDirection, int _unique = 0, bool isAlwaysEffectOn = false)
    {
        //이미 해당 위치에 폭탄 영역 이펙트가 표시되고 있으면 표시하지 않음.
        Vector2Int checkIndex = new Vector2Int(board.indexX, board.indexY);
        if (InGameEffectMaker.instance.GetIndexPangFieldEffect(checkIndex) != -1)
            return false;
        
        //장막 기믹이 있는 보드 검사
        if(board.BoardOnHide.Count > 0)
            return true;
        
        //잡기돌, 물 같이 블럭이 없을 수도 있는 보드 검사
        if (board.BoardOnNet.Count > 0)
            return true;

        //블럭이 없으면 표시하지 않음
        BlockBase checkBlock = board.Block;
        if (checkBlock == null)
            return false;

        //블럭이 제거될 수 없는 상태이거나, 폭탄 영역을 표시할 수 없는 기믹이라면 표시하지 않음.
        if (checkBlock.pangIndex == _unique ||
            (checkBlock.IsCanMakeBombFieldEffect() == false && isAlwaysEffectOn == false))
            return false;

        return true;
    }

    //해당 방향의 폭발을 막는지 검사
    public bool IsCanPangExtendByBomb(Board board, BlockDirection bombDirection, int unique)
    {
        bool bCanPangDirection = true;

        //데코가 해당 방향의 폭발을 막는지 검사
        foreach (DecoBase boardDeco in board.DecoOnBoard)
        {
            bCanPangDirection = boardDeco.IsCanPangBlockBoardIndex(board.indexX, board.indexY, bombDirection);
            if (bCanPangDirection == false)
                return false;
        }

        if (bCanPangDirection == true && board.Block != null && board.Block.type != BlockType.ColorBigJewel)
            return board.Block.IsDisturbBlock_ByBomb(unique);

        return true;
    }

    //해당 방향으로 확장 가능한지(이동을 막고있는 게 있는지) 검사하는 함수.
    public bool IsCanExtensionBombDirection(Board checkBoard, IDisturb disturb, int indexX, int indexY, BlockDirection direction)
    {
        if (checkBoard == null || disturb == null)
            return true;

        //해당 방향에 블럭 밤을 방해하는 데코가 있는지 검사.
        if (disturb.IsDisturbBomb() == true && disturb.IsDisturbBoard(direction, indexX, indexY) == true)
        {
            return false;
        }
        return true;
    }

    public bool IsDiagonalDisturbPang(int inX, int inY, int offX, int offY)
    {//해당 offset 주변에 블럭팡을 막는 오브젝트가 있는지 검사.
        if (offX > 0)
        {
            if (offY > 0)
            {   //오른쪽 하단 대각일 경우 검사.

                if (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.DOWN) == true
                    && (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.RIGHT) == true || bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.DOWN) == true))
                    return true;
                if ((bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.DOWN) == true || bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.LEFT) == true)
                    && bCheckDisturbPang(inX, inY, 0, 1, BlockDirection.RIGHT) == true)
                    return true;
            }
            else
            {   //오른쪽 상단 대각 검사.
                if (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.UP) == true
                    && (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.RIGHT) == true || bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.UP) == true))
                    return true;
                if ((bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.UP) == true || bCheckDisturbPang(inX, inY, 1, 0, BlockDirection.LEFT) == true)
                    && bCheckDisturbPang(inX, inY, 0, -1, BlockDirection.RIGHT) == true)
                    return true;
            }
        }
        else
        {
            if (offY > 0)
            {   //왼쪽 하단 대각일 경우 검사.
                if (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.DOWN) == true
                    && (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.LEFT) == true || bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.DOWN) == true))
                    return true;
                if ((bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.DOWN) == true || bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.RIGHT) == true)
                    && bCheckDisturbPang(inX, inY, 0, 1, BlockDirection.LEFT) == true)
                    return true;
            }
            else
            {   //왼쪽 상단 대각 검사.
                if (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.UP) == true
                    && (bCheckDisturbPang(inX, inY, 0, 0, BlockDirection.LEFT) == true || bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.UP) == true))
                    return true;
                if ((bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.UP) == true || bCheckDisturbPang(inX, inY, -1, 0, BlockDirection.RIGHT) == true)
                    && bCheckDisturbPang(inX, inY, 0, -1, BlockDirection.LEFT) == true)
                    return true;
            }
        }
        return false;
    }

    public bool bCheckDisturbPang(int inX, int inY, int offX, int offY, BlockDirection dir)
    {
        Board checkBoard = PosHelper.GetBoardSreeen(inX, inY, offX, offY);
        if (checkBoard != null)
        {
            for (int i = 0; i < checkBoard.BoardOnDisturbs.Count; i++)
            {
                if (checkBoard.BoardOnDisturbs[i].IsDisturbBomb() == true &&
                    checkBoard.BoardOnDisturbs[i].IsDisturbBoard(dir, inX + offX, inY + offY) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region 폭발범위설정
    public List<BombAreaInfo> GetBombAreaInfoList(BlockBomb.BombShape bombShape, int bombX, int bombY, int sizeX, int sizeY)
    {
        // 폭발 범위 시작 지점 구하기.
        int startX = bombX;
        int startY = bombY;
        int offsetX = 0;
        int offsetY = 0;

        if (sizeX > 2)
            offsetX = sizeX / 2;
        if (sizeY > 2)
            offsetY = sizeY / 2;

        startX -= offsetX;
        startY -= offsetY;

        //모양 따라 폭발 범위 지정.
        List<BombAreaInfo> infoList = new List<BombAreaInfo>();
        switch (bombShape)
        {
            case BlockBomb.BombShape.Rect:
                MakeBombAreaShapeRect(startX, startY, sizeX, sizeY, out infoList);
                break;
            case BlockBomb.BombShape.Cross:
                MakeBombAreaShapeCross(startX, startY, sizeX, sizeY, bombX, bombY, out infoList);
                break;
            case BlockBomb.BombShape.RectCross:
                MakeBombAreaShapeRectCross(startX, startY, sizeX, sizeY, bombX, bombY, out infoList);
                break;
        }
        return infoList;
    }

    public void MakeBombAreaShapeRect(int startX, int startY, int sizeX, int sizeY, out List<BombAreaInfo> infoList)
    {
        infoList = new List<BombAreaInfo>();

        int infoY = startY;
        BlockDirection dir = BlockDirection.UP;

        // x 축의 경계선들 넣음.
        for (int y = 0; y < 2; y++)
        {
            List<BlockDirection> dirLIstX = new List<BlockDirection>();
            dirLIstX.Add(BlockDirection.LEFT);

            for (int x = 0; x < sizeX; x++)
            {
                //경계 방향 넣어줌.
                dirLIstX.Add(dir);
                if (x + 1 >= sizeX)
                    dirLIstX.Add(BlockDirection.RIGHT);

                BombAreaInfo info = new BombAreaInfo(startX + x, infoY, dirLIstX);
                infoList.Add(info);
                dirLIstX.Clear();
            }
            infoY += (sizeY - 1);
            dir = BlockDirection.DOWN;
        }

        int infoX = startX;
        if ((sizeY - 1) > 0)
        {
            dir = BlockDirection.LEFT;
            // y축의 경계선들 넣음.
            for (int x = 0; x < 2; x++)
            {
                List<BlockDirection> dirLIstY = new List<BlockDirection>();
                for (int y = 1; y < sizeY - 1; y++)
                {
                    //경계 방향 넣어줌.
                    dirLIstY.Add(dir);

                    BombAreaInfo info = new BombAreaInfo(infoX, startY + y, dirLIstY);
                    infoList.Add(info);
                }
                infoX += (sizeX - 1);
                dir = BlockDirection.RIGHT;
            }
        }
    }

    public void MakeBombAreaShapeCross(int startX, int startY, int sizeX, int sizeY, int bombX, int bombY, out List<BombAreaInfo> infoList)
    {
        infoList = new List<BombAreaInfo>();

        // x 축의 경계선들 넣음.
        List<BlockDirection> dirLIstX = new List<BlockDirection>();
        dirLIstX.Add(BlockDirection.LEFT);

        for (int i = 0; i < sizeX; i++)
        {
            if (startX + i == bombX)
                continue;
            if (i + 1 >= sizeX)
                dirLIstX.Add(BlockDirection.RIGHT);

            dirLIstX.Add(BlockDirection.UP);
            dirLIstX.Add(BlockDirection.DOWN);
            BombAreaInfo info = new BombAreaInfo(startX + i, bombY, dirLIstX);
            infoList.Add(info);
            dirLIstX.Clear();
        }

        // y 축의 경계선들 넣음.
        List<BlockDirection> dirLIstY = new List<BlockDirection>();
        dirLIstY.Add(BlockDirection.UP);

        for (int i = 0; i < sizeY; i++)
        {
            if (startY + i == bombY)
                continue;
            if (i + 1 >= sizeY)
                dirLIstY.Add(BlockDirection.DOWN);

            dirLIstY.Add(BlockDirection.LEFT);
            dirLIstY.Add(BlockDirection.RIGHT);
            BombAreaInfo info = new BombAreaInfo(bombX, startY + i, dirLIstY);
            infoList.Add(info);
            dirLIstY.Clear();
        }
    }

    private void MakeBombAreaShapeRectCross(int startX, int startY, int sizeX, int sizeY, int bombX, int bombY, out List<BombAreaInfo> infoList)
    {
        infoList = new List<BombAreaInfo>();

        // x 축의 경계선들 넣음.
        int infoY = startY;
        int offsetY = 0;
        BlockDirection dirX = BlockDirection.UP;
        for (int y = 0; y < 2; y++)
        {
            List<BlockDirection> dirLIstX = new List<BlockDirection>();
            dirLIstX.Add(BlockDirection.LEFT);

            for (int x = 0; x < sizeX; x++)
            {
                // 중간지점은 한칸 위/아래쪽 데이터로 채우기.
                if (bombX == (x + startX))
                {
                    if (y == 0)
                        offsetY -= 1;
                    else
                        offsetY += 1;

                    dirLIstX.Add(BlockDirection.RIGHT);
                    dirLIstX.Add(BlockDirection.LEFT);
                }

                if (x + 1 >= sizeX)
                    dirLIstX.Add(BlockDirection.RIGHT);

                dirLIstX.Add(dirX);

                BombAreaInfo info = new BombAreaInfo(startX + x, (infoY + offsetY), dirLIstX);
                infoList.Add(info);
                dirLIstX.Clear();
                offsetY = 0;
            }
            infoY += (sizeY - 1);
            dirX = BlockDirection.DOWN;
        }

        // y축의 경계선을 넣음
        int infoX = startX;
        int offsetX = 0;
        BlockDirection dirY = BlockDirection.LEFT;

        if ((sizeY - 1) > 0)
        {
            // y축의 경계선들 넣음.
            for (int x = 0; x < 2; x++)
            {
                List<BlockDirection> dirLIstY = new List<BlockDirection>();

                for (int y = 1; y < sizeY - 1; y++)
                {
                    // 중간지점은 한칸 위/아래쪽 데이터로 채우기.
                    if (bombY == (y + startY))
                    {
                        if (x == 0)
                            offsetX -= 1;
                        else
                            offsetX += 1;

                        dirLIstY.Add(BlockDirection.UP);
                        dirLIstY.Add(BlockDirection.DOWN);
                    }
                    dirLIstY.Add(dirY);

                    BombAreaInfo info = new BombAreaInfo((infoX + offsetX), startY + y, dirLIstY);
                    infoList.Add(info);
                    dirLIstY.Clear();
                    offsetX = 0;
                }
                infoX += (sizeX - 1);
                dirY = BlockDirection.RIGHT;
            }
        }
    }
    #endregion
}
