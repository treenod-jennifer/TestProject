using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;

public enum EditMode
{
    START,  BOARD,  SCARP, BLOCK,  PotalIn,  PotalOut,
    APPLE,  NET,  BLOCKTYPE, CANDY, EMPTY2, EMPTY3,
    CRACK1, COUNT_CRACK, KEY, KEY_HOLE, GRASSFENCEBLOCK, FENCEBLOCK,
    MaxCount1,

    PLANT, PLANT_APPLE, PLANT_KEY, GROUND, LINKSIDE, SIDEBLOCK,
    LINE, EMPTY1, BOMB, RAINBOW, GRASS, STATUE,
    JEWEL, PLANT2X2, STONE, BOX, ICE_APPLE, BLOCK_BLACK,
    MaxCount2,

    AUTO_TRIGGER,  MAP_DECO, ICE, SAND_BELT, ICE_PLANT, BOMB_PLANT, 
    GROUND_APPLE, GROUND_BOMB, DUCK, GROUND_KEY, GROUND_ICE_APPLE, Event_Block, 
    WATER, DYNAMITE, COIN, ADVENTURE_POTION_HEAL, ADVENTURE_POTION_SKILL, Event_Stone,
    MaxCount3,

    CARPET, ColorFlowerPot_Little, COLOR_BIG_JEWEL, FIRE_WORK, COIN_STAGE, COIN_BAG,
    PLANT_COIN, SODAJELLY, PEA, WORLD_RANK_ITEM, FLOWER_INK, SPACESHIP,
    SPACESHIP_EXIT, BLOCKGENERATOR, PEA_BOSS, RANDOM_BOX, HEART, PAINT,
    MaxCount4,

    BREAD, WATERBOMB, CLOVER, CANNON, MaxCount5,

    MaxCount,

    None = 100000
}

//맵 파일 다운로드 시, 어디서 다운받을 지
public enum DownLoadAdressType
{
    Download_Default,   //툴 설정대로 받기
    Download_Ext        //외부 주소에서 받기
}

public enum EditWindowMode
{
    GAME,
    HOME,
    BLOCK,
    PLANT,
    GIMMICK,
    GIMMICK2,
    GIMMICK3,
    START,
    MAP_MAKER,
    MAP_INFO,
    ADVANCE_MODE,
}

public enum EditAdvanceTap
{
    Animal_SETTING,
    ENEMY_SETTING
}

public enum EditGimmickTap_2
{
    NONE,
    BLOCKGENERATOR,
}

public class EditManager : MonoBehaviour
{
    public static EditManager instance = null;

    //태그 관련
    [SerializeField] private EditUIMapTag editUIMapTag;

    //편집툴
    public GameObject Edit_collider;
    public GameObject blockGuidePrefab;
    public GameObject mEditAnchor;
    public Camera mCamera;

    private EditBlockGuide[,] mBlockGiudeList;
    public List<GameObject> listGuideObj = new List<GameObject>();

    public bool isPopupOpen = false;
    public const string mapMakerKey = "MapMaker";

    //편집툴
    public List<Board> listSand = new List<Board>();

    //에디터에서만 보이는 기믹별 표시되는 정보 오브젝트
    public Dictionary<BlockType, List<GameObject>> dicGimmickGuide = new Dictionary<BlockType, List<GameObject>>();

    //[System.NonSerialized]
    public int[] paintCount;
    string _stringFileName = "";
    string _toolState = "";
    Rect windowRectSetting = new Rect(0, 0, 480, 200);

    string[] paintModeStrings1 = {
                                 "출발",  "배경",   "절벽",   "블럭", "포탈-인","포탈아웃",
                                 "사과","잡기돌","블럭타입","사탕", "00","00",
                                 "석판","단계석판","열쇠","열쇠홀","풀울타리","돌울타리",
                             };
    string[] paintModeStrings2 = {
                                 "식물",  "사과식물",   "열쇠식물",   "흙", "방해블럭A","방해블럭B",
                                 "라인폭탄","00","둥근","레인보우","잔디","석상",
                                 "보석","식물2","바위","포츈쿠키","얼음사과","연쇄폭탄",
                             };

    string[] paintModeStrings3 = {"스위치1","데코","얼음","모래","얼음식물","폭탄나무",
                                 "흙사과","흙폭탄","오리","흙열쇠","흙얼음사과","이벤트블럭",
                                 "물","째깍폭탄","코인","포션-회복","포션-스킬","이벤트바위",
                             };

    string[] paintModeStrings4 = {"양털","작은화단","컬러화단","벌집","코인설정","코인주머니",
                                 "코인식물","소다젤리","완두콩","월드랭킹","잉크","거미",
                                 "거미홀","생성기","폭죽콩","랜덤박스","배터리","페인트",
                             };

    string[] paintModeStrings5 = {"빵","물폭탄","낙엽","대포","000","000",
                                 "000","000","000","000","000","000",
                                 "000","000","000","000","000","000",
                             };

    public EditMode eMode = EditMode.BLOCK;
    public EditWindowMode windowMode = EditWindowMode.HOME;

    //흙 확장형 기믹 탭 전환 버튼
    private bool isGroundTap = false;
    //하트와 하트길 탭 전환 버튼
    public bool isHeartWayTap = false;

    //출발 정보 보기 체크되어 있는지 확인하는 변수.
    public bool bShowStartInfo = false;
    public DecoInfo decoInfo = null;

    //이전에 선택한 모드 저장하는 변수.
    private EditMode eModePreSelect = EditMode.BLOCK;

    //맵 파일 다운로드 받는 타입
    public DownLoadAdressType downLoadAdressType = DownLoadAdressType.Download_Default;

    //목표 종류 수
    private int collectTypeCount = 0;
    //private int adventureSameBlockCount = 5;

    //현재 맵에 있는 블럭/데코들의 수를 자동으로 카운트 할 때 사용되는 딕셔너리(툴에서 카운트 확인용으로만 사용)
    public Dictionary<BlockType, int> dicBlockCount = new Dictionary<BlockType, int>();
    public Dictionary<BoardDecoType, int> dicDecoCount = new Dictionary<BoardDecoType, int>();

    //출발 가능한 기믹 종류와 기믹 이름
    List<BlockTypeAndText_AtBlockMake> listBlockTypeAndText = null;

    #region 생성기 에디터 전용 데이터
    //블럭 생성기 토글 데이터(블럭 타입에 따른 설정).
    public Dictionary<BlockType, GeneratorToggleData> dicToggleData_BlockGenerator = null;

    //블럭 생성기의 컬러 설정 토글 데이터.
    bool[] generatorBlockColor = new bool[] { true, true, true, true, true };

    //인덱스 별 블럭 생성기 데이터
    Dictionary<int, GeneratorBlockInfo> dicGeneratorBlockInfo = null;
    #endregion

    //출발에서 생성될 수 있는 컬러 타입을 지정한 토글 데이터
    public Dictionary<BlockType, int> dicToggleData_StartColorType = null;

    //컬러 박스
    Dictionary<BlockColorType, Texture2D> dicTexColorBox = new Dictionary<BlockColorType, Texture2D>();

    //레디아이템 테스트용
    public bool[] readyItemTest = { false, false, false, false, false, false };
    
    //종류별로 정렬하여 담은 출발 리스트
    private List<(int startBlockTypeIdx, string blockName, int kind)> _blockListByStart;
    public List<(int startBlockTypeIdx, string blockName, int kind)> blockListByStart
    {
        get
        {
            if (_blockListByStart != null && _blockListByStart.Count > 0)
            {
                return _blockListByStart;
            }
            else
            {
                _blockListByStart = GetBlockIdxAndNameByStart();
                return _blockListByStart;
            }
        }
    }
    
    //스크롤에 담긴 기믹 라인 수
    private int scrollLine = 0;

    public class GeneratorToggleData
    {
        //출발 설정 여부
        public bool isCanMakeBlock = false;
        //대표 이미지 설정 여부
        public bool isShow = false;
        //대표 이미지 컬러 설정
        public List<BlockColorType> listShowColorType = null;
        //서브 타입 설정
        public int subType = -1;

        public GeneratorToggleData()
        {
            isCanMakeBlock = false;
            isShow = false;
            listShowColorType = new List<BlockColorType>();
            subType = -1;
        }

        public GeneratorToggleData(GeneratorToggleData copyData)
        {
            isCanMakeBlock = copyData.isCanMakeBlock;
            isShow = copyData.isShow;
            listShowColorType = new List<BlockColorType>(copyData.listShowColorType);
            subType = copyData.subType;
        }
    }

    private class GeneratorBlockInfo
    {
        //생성될 수 있는 블럭 정보
        public List<BlockType> listCanMakeBlock = new List<BlockType>();

        //생성될 수 있는 컬러 정보
        //key : 블럭 타입, value : 생성가능한 컬러값
        public Dictionary<BlockType, int> dicCanMakeColor
            = new Dictionary<BlockType, int>() { { BlockType.NORMAL, int.MaxValue } };

        //대표 이미지로 블럭 및 컬러 정보
        public List<BlockAndColorData> listTitleImageData = new List<BlockAndColorData>();
    }

    #region 저장/읽기 관련
    enum SaveAndLoadState
    {
        None,
        Loading,
        Complete,
        Failed
    }
    private SaveAndLoadState mapState = SaveAndLoadState.None;

    private string stateText = "";
    private string message = "";
    private bool bShowText = false;
    private Coroutine stateCoroutine = null;
    #endregion

    #region 인게임 에디터 팝업 창 관련
    private class IngameEditPopupData
    {
        public string name = "";
        public Rect popupRect = new Rect(15, 50, 450, 300);
        public GUI.WindowFunction windowFunc;
    }

    private IngameEditPopupData popupData = null;

    #endregion

    #region 블럭 생성 구조체

    struct BlockTypeAndText_AtBlockMake
    {
        //블럭 타입
        public BlockType blockType;
        //출력할 블럭 이름
        public string blockName;
    }

    //출발 가능한 기믹들 종류와 이름 설정
    private List<BlockTypeAndText_AtBlockMake> GetBlockTypeAndTextData_AtBlockMake()
    {
        List<BlockTypeAndText_AtBlockMake> listData = new List<BlockTypeAndText_AtBlockMake>();
        for (int i = 0; i < 28; i++)
        {
            BlockTypeAndText_AtBlockMake data = new BlockTypeAndText_AtBlockMake();

            switch (i)
            {
                case 0:
                    data.blockType = BlockType.NORMAL;
                    data.blockName = "일반";
                    break;
                case 1:
                    data.blockType = BlockType.GROUND;
                    data.blockName = "흙";
                    break;
                case 2:
                    data.blockType = BlockType.BOX;
                    data.blockName = "포츈쿠키";
                    break;
                case 3:
                    data.blockType = BlockType.GROUND_JEWEL;
                    data.blockName = "흙보석";
                    break;
                case 4:
                    data.blockType = BlockType.GROUND_BOMB;
                    data.blockName = "흙폭탄";
                    break;
                case 5:
                    data.blockType = BlockType.GROUND_KEY;
                    data.blockName = "흙열쇠";
                    break;
                case 6:
                    data.blockType = BlockType.KEY;
                    data.blockName = "열쇠";
                    break;
                case 7:
                    data.blockType = BlockType.GROUND_ICE_APPLE;
                    data.blockName = "흙얼음사과";
                    break;
                case 8:
                    data.blockType = BlockType.BLOCK_BLACK;
                    data.blockName = "연쇄폭탄";
                    break;
                case 9:
                    data.blockType = BlockType.BLOCK_DYNAMITE;
                    data.blockName = "째깍폭탄";
                    break;
                case 10:
                    data.blockType = BlockType.ICE;
                    data.blockName = "얼음";
                    break;
                case 11:
                    data.blockType = BlockType.ADVENTURE_POTION_HEAL;
                    data.blockName = "힐포션";
                    break;
                case 12:
                    data.blockType = BlockType.ADVENTURE_POTION_SKILL;
                    data.blockName = "스킬포션";
                    break;
                case 13:
                    data.blockType = BlockType.START_Line;
                    data.blockName = "라인폭탄";
                    break;
                case 14:
                    continue;
                case 15:
                    data.blockType = BlockType.START_Bomb;
                    data.blockName = "둥근폭탄";
                    break;
                case 16:
                    data.blockType = BlockType.START_Rainbow;
                    data.blockName = "레인보우";
                    break;
                case 17:
                    data.blockType = BlockType.APPLE;
                    data.blockName = "사과";
                    break;
                case 18:
                    data.blockType = BlockType.ICE_APPLE;
                    data.blockName = "얼음사과";
                    break;
                case 19:
                    data.blockType = BlockType.FIRE_WORK;
                    data.blockName = "폭죽";
                    break;
                case 20:
                    data.blockType = BlockType.COIN_BAG;
                    data.blockName = "코인주머니";
                    break;
                case 21:
                    data.blockType = BlockType.PEA;
                    data.blockName = "완두콩";
                    break;
                case 22:
                    data.blockType = BlockType.GROUND_APPLE;
                    data.blockName = "흙사과";
                    break;
                case 23:
                    data.blockType = BlockType.SPACESHIP;
                    data.blockName = "거미";
                    break;
                case 24:
                    data.blockType = BlockType.PEA_BOSS;
                    data.blockName = "폭죽콩";
                    break;
                case 25:
                    data.blockType = BlockType.GROUND_BLOCKBLACK;
                    data.blockName = "흙연쇄";
                    break;
                case 26:
                    data.blockType = BlockType.PAINT;
                    data.blockName = "페인트";
                    break;
                case 27:
                    data.blockType = BlockType.WATERBOMB;
                    data.blockName = "물풍선";
                    break;
                default:
                    data.blockType = BlockType.NONE;
                    data.blockName = "";
                    break;
            }

            listData.Add(data);
        }

        return listData;
    }

    //출발 가능한 기믹의 이름 가져오기
    private string GetBlockName_ByBlockType(BlockType blockType)
    {
        string name = "";
        if (listBlockTypeAndText != null)
        {
            int fIndex = listBlockTypeAndText.FindIndex(x => x.blockType == blockType);
            if (fIndex > -1)
                return listBlockTypeAndText[fIndex].blockName;
        }
        return name;
    }
    
    // startBlockTypes 인덱스 순서대로가 아니라 흙종류별, 폭탄별 등등으로 묶어서 표시 
    private List<(int startBlockTypeIdx, string blockName, int kind)> GetBlockIdxAndNameByStart()
    {
        List<(int startBlockTypeIdx, string blockName, int kind)> listData = new List<(int startBlockTypeIdx, string blockName, int kind)>();
        for (int i = 0; i < 27; i++)
        {
            (int startBlockTypeIdx, string blockName, int kind) data;

            switch (i)
            {
                //일반
                case 0:
                    data.startBlockTypeIdx = 0;
                    data.blockName         = "일반";
                    data.kind              = 0;
                    break;
                case 1:
                    data.startBlockTypeIdx = 17;
                    data.blockName         = "사과";
                    data.kind              = 0;
                    break;
                case 2:
                    data.startBlockTypeIdx = 2;
                    data.blockName         = "포춘쿠키";
                    data.kind              = 0;
                    break;
                case 3:
                    data.startBlockTypeIdx = 10;
                    data.blockName         = "얼음";
                    data.kind              = 0;
                    break;
                case 4:
                    data.startBlockTypeIdx = 6;
                    data.blockName         = "열쇠";
                    data.kind              = 0;
                    break;
                case 5:
                    data.startBlockTypeIdx = 23;
                    data.blockName         = "거미";
                    data.kind              = 0;
                    break;
                case 6:
                    data.startBlockTypeIdx = 21;
                    data.blockName         = "완두콩";
                    data.kind              = 0;
                    break;
                case 7:
                    data.startBlockTypeIdx = 24;
                    data.blockName         = "폭죽콩";
                    data.kind              = 0;
                    break;
                //흙류
                case 8:
                    data.startBlockTypeIdx = 1;
                    data.blockName         = "흙";
                    data.kind              = 1;
                    break;
                case 9:
                    data.startBlockTypeIdx = 3;
                    data.blockName         = "흙보석";
                    data.kind              = 1;
                    break;
                case 10:
                    data.startBlockTypeIdx = 4;
                    data.blockName         = "흙폭탄";
                    data.kind              = 1;
                    break;
                case 11:
                    data.startBlockTypeIdx = 5;
                    data.blockName         = "흙열쇠";
                    data.kind              = 1;
                    break;
                case 12:
                    data.startBlockTypeIdx = 7;
                    data.blockName         = "흙얼음사과";
                    data.kind              = 1;
                    break;
                case 13:
                    data.startBlockTypeIdx = 22;
                    data.blockName         = "흙사과";
                    data.kind              = 1;
                    break;
                case 14:
                    data.startBlockTypeIdx = 26;
                    data.blockName         = "흙연쇄";
                    data.kind              = 1;
                    break;
                //폭탄류
                case 15:
                    data.startBlockTypeIdx = 25;
                    data.blockName         = "라인폭탄";
                    data.kind              = 2;
                    break;
                case 16:
                    data.startBlockTypeIdx = 15;
                    data.blockName         = "둥근폭탄";
                    data.kind              = 2;
                    break;
                case 17:
                    data.startBlockTypeIdx = 16;
                    data.blockName         = "레인보우";
                    data.kind              = 2;
                    break;
                case 18:
                    data.startBlockTypeIdx = 8;
                    data.blockName         = "연쇄폭탄";
                    data.kind              = 2;
                    break;
                case 19:
                    data.startBlockTypeIdx = 9;
                    data.blockName         = "째깍폭탄";
                    data.kind              = 2;
                    break;
                case 20:
                    data.startBlockTypeIdx = 19;
                    data.blockName         = "벌집";
                    data.kind              = 2;
                    break;
                //기타
                case 21:
                    data.startBlockTypeIdx = 18;
                    data.blockName         = "얼음사과";
                    data.kind              = 3;
                    break;
                case 22:
                    data.startBlockTypeIdx = 27;
                    data.blockName         = "페인트";
                    data.kind              = 3;
                    break;
                case 23:
                    data.startBlockTypeIdx = 28;
                    data.blockName         = "물폭탄";
                    data.kind              = 3;
                    break;
                //특정 모드 기믹
                case 24:
                    data.startBlockTypeIdx = 11;
                    data.blockName         = "힐포션";
                    data.kind              = 4;
                    break;
                case 25:
                    data.startBlockTypeIdx = 12;
                    data.blockName         = "스킬포션";
                    data.kind              = 4;
                    break;
                case 26:
                    data.startBlockTypeIdx = 20;
                    data.blockName         = "코인주머니";
                    data.kind              = 4;
                    break;
                default:
                    data.startBlockTypeIdx = -1;
                    data.blockName         = "";
                    data.kind              = -1;
                    break;
            }

            listData.Add(data);
        }

        return listData;
    }
    
    #endregion

    #region 컬러 타입별 이름 가져오는 함수

    private string GetColorTypeName(BlockColorType blockColorType)
    {
        switch (blockColorType)
        {
            case BlockColorType.A:
                return "핑크";
            case BlockColorType.B:
                return "갈색";
            case BlockColorType.C:
                return "노랑";
            case BlockColorType.D:
                return "주황";
            case BlockColorType.E:
                return "파랑";
            case BlockColorType.RANDOM:
                return "랜덤";
            default:
                return "없음";
        }
    }
    #endregion

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(CoLoadUrl());
        
        Edit_collider.SetActive(true);

        paintCount = new int[(int)EditMode.MaxCount];
        for (int i = 0; i < (int)EditMode.MaxCount; i++)
        {
            paintCount[i] = 1;
        }

        GameManager.instance.state = GameState.EDIT;

        if (PlayerPrefs.HasKey("fileName"))
        {
            _stringFileName = PlayerPrefs.GetString("fileName");
        }

        if (PlayerPrefs.HasKey(mapMakerKey) == false)
        {
            isPopupOpen = true;
        }

        //태그 에디터 초기화
        editUIMapTag.InitTaggingData_Clear();

        InitBlockGuide();
        CreateBlockGuide();
        InitColorBox();

        //출발 가능한 기믹 종류 및 이름 설정
        listBlockTypeAndText = GetBlockTypeAndTextData_AtBlockMake();
        
        touchPos.y = 10f;
        MoveDown();
    }

    public IEnumerator CoSetReadyItem()
    {
        yield return new WaitUntil(() => ManagerBlock.instance.state == BlockManagrState.READY);

        if(readyItemTest[0] == true)
        {
            //사과 3개
            StartCoroutine(GameUIManager.instance.CoActionAddTurn_WithMakeBubble(3));
        }

        if (readyItemTest[1] == true)
        {
            //스코어 업은 점수 계산에 사용되는 것으로 에디터에서 테스트 X
        }

        if (readyItemTest[2] == true)
        {
        }

        if (readyItemTest[3] == true)
        {
            BlockBase lineBlock = PosHelper.GetRandomBlockAtGameStart();

            int randomLine = GameManager.instance.GetIngameRandom(0, 2);
            if (randomLine == 0) lineBlock.bombType = BlockBombType.LINE_V;
            else lineBlock.bombType = BlockBombType.LINE_H;

            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            lineBlock.JumpBlock();
            InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
            lineBlock.Destroylinker();
        }
        if (readyItemTest[4] == true)
        {
            BlockBase lineBlock = PosHelper.GetRandomBlockAtGameStart();

            lineBlock.bombType = BlockBombType.BOMB;
            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            lineBlock.JumpBlock();
            InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
            lineBlock.Destroylinker();
        }
        if (readyItemTest[5] == true)
        {
            BlockBase lineBlock = PosHelper.GetRandomBlockAtGameStart();

            lineBlock.bombType = BlockBombType.RAINBOW;
            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            lineBlock.JumpBlock();
            InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
            lineBlock.Destroylinker();
        }
    }

    private void InitColorBox()
    {
        dicTexColorBox.Clear();

        int colorLength = (System.Enum.GetValues(typeof(BlockColorType)).Length - 1);
        for (int i = 1; i < colorLength; i++)
        {
            BlockColorType cType = (BlockColorType)i;
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, GetBlockColorTypeColor(cType));
            texture.Apply();
            dicTexColorBox.Add(cType, texture);
        }
    }

    public void CreateBlockGuide()
    {
        mBlockGiudeList = new EditBlockGuide[GameManager.MAX_X, GameManager.MAX_Y];

        for (int i = GameManager.MIN_X; i < GameManager.MAX_X; i++)
        {
            for (int j = GameManager.MIN_Y; j < GameManager.MAX_Y; j++)
            {
                GameObject obj = NGUITools.AddChild(mEditAnchor, blockGuidePrefab);

                mBlockGiudeList[i, j] = obj.GetComponent<EditBlockGuide>();
                mBlockGiudeList[i, j].inX = i;
                mBlockGiudeList[i, j].inY = j;
                mBlockGiudeList[i, j].transform.localPosition = PosHelper.GetPosByIndex(i, j) - Vector3.forward * 20;
                listGuideObj.Add(obj);
            }
        }
    }

    //초기화나 맵 로딩 시, 에디터에서 저장된 데이터 날려주는 함수
    private void ResetEditorManagerData()
    {
        dicEditData_StartProb.Clear();
        ResetBlockGeneratorData();
        InitEditManagerData();

        wayCount = 0;
        eModePreSelect = EditMode.BLOCK;
    }

    public void ResetGuidePosition()
    {
        foreach (var guide in mBlockGiudeList)
        {
            guide.transform.localPosition = PosHelper.GetPosByIndex(guide.inX, guide.inY) - Vector3.forward * 20;
        }
    }

    /// <summary>
    /// 맵 데이터를 이용해 에디터 데이터를 초기화 할 때 사용
    /// </summary>
    private void InitEditDataUseMapData()
    {
        //인덱스 별 생성기 데이터 초기화
        InitDicGeneratorBlockInfo_UseMapData();
    }

    public void InitBlockGuide()
    {
        foreach (GameObject obj in listGuideObj) Destroy(obj.gameObject);

        listGuideObj.Clear();
    }

    public void GameStart()
    {
        //툴버튼쪽 콜라이더 제거하기
        windowMode = EditWindowMode.GAME;
        foreach (GameObject obj in listGuideObj) obj.SetActive(true);

        //레디 아이템 사용
        StartCoroutine(CoSetReadyItem());
    }

    Vector3 touchPos = Vector3.zero;

    public int editTouchCount = 0;
    bool isLongClick = false;

    void Update()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            //모래정렬            
            if (eMode == EditMode.SAND_BELT && listSand.Count > 2)
                CheckSand();

            if (EditBlockGuide.reflesh == true)
            {
                for (int i = 0; i < 6; i++)
                {
                    UIPopupReady.readyItemUseCount[i] = new EncValue();
                    UIPopupReady.readyItemUseCount[i].Value = 0;
                }
                ManagerBlock.instance.RefreshBlockManagerByStageData();
                EditBlockGuide.reflesh = false;

                foreach (var obj in mBlockGiudeList)
                {
                    obj.GetComponent<EditBlockGuide>()._spriteBody.alpha = 0.0f;
                }
                ResetGuidePosition();
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            listSand.Clear();
        }

        if (GameManager.instance.state == GameState.EDIT)
        {
            if (Global._touching && Global._touchCount == 1) //(GameManager._touching && GameManager._touchCount == 1)
            {
                Vector3 pos = mCamera.ScreenToViewportPoint(Global._touchPos);//  GameManager.mMousePosition);

                if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
                {

                }
                else
                {
                    Ray ray = mCamera.ScreenPointToRay(Global._touchPos);//GameManager.mMousePosition);
                    RaycastHit hit;
                    float dist = mCamera.farClipPlane - mCamera.nearClipPlane;
                    if (Physics.Raycast(ray, out hit, dist))
                    {
                        hit.collider.SendMessage("OnEdit", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            if (Input.GetButtonUp("Fire1") && Global._touchTap == false)
            {
                if ((Global._touchPos.y / (float)Screen.height) > 0.8f)
                    isLongClick = true;
            }
            else
                isLongClick = false;

            if (Global._touching && Global._touchCount == 2) //if (GameManager._touching && GameManager._touchCount == 2)
            {
                touchPos += Global._touchDeltaPos / ((float)Screen.width / 95f);
                if (touchPos.x > 10f)
                {
                    MoveRight();
                    touchPos.x -= 10f;
                }
                else if (touchPos.x < -10)
                {
                    MoveLeft();
                    touchPos.x += 10f;
                }
                else if (touchPos.y > 10f)
                {
                    MoveUP();
                    touchPos.y -= 10f;
                }
                else if (touchPos.y < -10f)
                {
                    MoveDown();
                    touchPos.y += 10f;
                }
            }
        }
    }


    void CheckSand()
    {
        if (listSand.Count > 4 && listSand[0] == listSand[listSand.Count - 1])   //순환형
        {
            for (int i = 0; i < listSand.Count - 1; i++)
            {
                BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(listSand[i].indexX, listSand[i].indexY);

                DecoInfo deco = new DecoInfo();
                deco.BoardType = (int)BoardDecoType.SAND_BELT;

                deco.index = blockCount;

                if (i != listSand.Count - 2)
                {
                    deco.count = (int)PosHelper.GetDir(listSand[i].indexX, listSand[i].indexY, listSand[i + 1].indexX, listSand[i + 1].indexY);
                }
                else
                {
                    deco.count = (int)PosHelper.GetDir(listSand[i].indexX, listSand[i].indexY, listSand[0].indexX, listSand[0].indexY);
                }
                blockInfo.ListDeco.Add(deco);
            }
        }
        else
        {
            for (int i = 0; i < listSand.Count - 1; i++)
            {
                BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(listSand[i].indexX, listSand[i].indexY);

                DecoInfo deco = new DecoInfo();
                deco.BoardType = (int)BoardDecoType.SAND_BELT;
                deco.index = blockCount;
                deco.count = (int)PosHelper.GetDir(listSand[i].indexX, listSand[i].indexY, listSand[i + 1].indexX, listSand[i + 1].indexY); //방향

                if (i == listSand.Count - 2)
                {
                    deco.type = listSand[0].indexX + listSand[0].indexY * GameManager.mMAX_X;
                }

                blockInfo.ListDeco.Add(deco);
            }
        }
    }

    void MoveUP()
    {
        Vector3 vec = GameUIManager.instance.groundAnchor.transform.localPosition;
        vec.y += 78f;
        GameUIManager.instance.groundAnchor.transform.localPosition = vec;
        mEditAnchor.transform.localPosition = vec;
    }

    void MoveDown()
    {
        Vector3 vec = GameUIManager.instance.groundAnchor.transform.localPosition;
        vec.y -= 78f;
        GameUIManager.instance.groundAnchor.transform.localPosition = vec;
        mEditAnchor.transform.localPosition = vec;
    }

    void MoveLeft()
    {
        Vector3 vec = GameUIManager.instance.groundAnchor.transform.localPosition;
        vec.x -= 78f;
        GameUIManager.instance.groundAnchor.transform.localPosition = vec;
        mEditAnchor.transform.localPosition = vec;
    }

    void MoveRight()
    {
        Vector3 vec = GameUIManager.instance.groundAnchor.transform.localPosition;
        vec.x += 78f;
        GameUIManager.instance.groundAnchor.transform.localPosition = vec;
        mEditAnchor.transform.localPosition = vec;
    }

    //맵 작성자 명
    string mapMakerName = "";

    void OpenPopupMapMaker()
    {
        if (popupData == null)
        {
            SetPopupData_MapMaker();
        }

        GUI.backgroundColor = Color.black;
        GUI.Window(5, popupData.popupRect, SetPopupMapMakerSetting, new GUIContent());
    }
    private void SetPopupData_MapMaker()
    {
        popupData = new IngameEditPopupData();
        popupData.popupRect = new Rect(90, 300, 300, 150);
        popupData.name = "작성자 정보";
        popupData.windowFunc = SetPopupMapMakerSetting;
    }

    //맵 작성자 팝업 내부 설정.
    private void SetPopupMapMakerSetting(int windowID)
    {
        //콜라이더 설정
        GameUIManager.instance.editPopupCollider.gameObject.SetActive(true);

        GUI.Box(new Rect(0f, 0f, popupData.popupRect.width, popupData.popupRect.height), popupData.name);
        GUI.Box(new Rect(0f, 0f, popupData.popupRect.width, popupData.popupRect.height), popupData.name);
        GUI.Box(new Rect(0f, 0f, popupData.popupRect.width, popupData.popupRect.height), popupData.name);
        GUI.Box(new Rect(0f, 0f, popupData.popupRect.width, popupData.popupRect.height), popupData.name);

        GUI.Label(new Rect(30f, 60f, 100f, 25f), "작성자 명");
        mapMakerName = GUI.TextField(new Rect(130f, 60f, 100f, 25f), mapMakerName);

        //창 닫기 버튼
        if (GUI.Button(new Rect(30f, popupData.popupRect.height - 50f, 60f, 40f), "OK"))
        {
            GameUIManager.instance.editPopupCollider.gameObject.SetActive(false);
            PlayerPrefs.SetString(mapMakerKey, mapMakerName);
            isPopupOpen = false;
            popupData = null;
        }
    }

    void OnGUI()
    {
        //NGUi에서 lockState 바꿔서 OnGUI 터치를 막는 부분 때문에 추가.
        Cursor.lockState = CursorLockMode.None;

        Matrix4x4 mat = Matrix4x4.identity;
        float scale = Screen.width / 480f;
        mat.SetTRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.matrix = mat;


        float scale2 = (float)Screen.height / (float)Screen.width;
        Edit_collider.transform.localScale = new Vector3(1, 2.57f - scale2, 1);

        if (isPopupOpen == true)
        {
            OpenPopupMapMaker();
            return;
        }


        if (windowMode == EditWindowMode.HOME)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, WindowEditHome, guiContent);
        }
        else if (windowMode == EditWindowMode.BLOCK)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowBlock, guiContent);
        }
        else if (windowMode == EditWindowMode.PLANT)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowPlant, guiContent);
        }
        else if (windowMode == EditWindowMode.GIMMICK)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowGimmick, guiContent);
        }
        else if (windowMode == EditWindowMode.GIMMICK2)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowGimmick2, guiContent);

            if (popupData != null)
                OpenPopupGimmickSetting_AtBlockGenerator();
        }
        else if (windowMode == EditWindowMode.GIMMICK3)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowGimmick3, guiContent);
        }
        else if (windowMode == EditWindowMode.START)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowStart, guiContent);
        }
        /*
        else if (windowMode == EditWindowMode.MAP_MAKER)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowMapMaker, guiContent);
        }
        */
        else if (windowMode == EditWindowMode.MAP_INFO)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowMapInfo, guiContent);
        }
        else if (windowMode == EditWindowMode.ADVANCE_MODE)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = _toolState;
            windowRectSetting = GUI.Window(0, windowRectSetting, windowAdvance, guiContent);
        }
        else if (windowMode == EditWindowMode.GAME)
        {
            if (GUI.Button(new Rect(10, 40, 80, 30), "Tool"))
            {
                ResetTool();
            }

            if (GUI.Button(new Rect(360, 40, 80, 30), "Reset"))
            {
                Application.LoadLevel("InGameTool");
            }
        }
    }

    public int GetDicBlockCount(BlockType blockType)
    {
        if (dicBlockCount.ContainsKey(blockType) == false)
        {
            dicBlockCount.Add(blockType, 1);
        }
        else
        {
            dicBlockCount[blockType]++;
        }
        return dicBlockCount[blockType];
    }

    public int GetDicDecoCount(BoardDecoType decoType)
    {
        if (dicDecoCount.ContainsKey(decoType) == false)
        {
            dicDecoCount.Add(decoType, 1);
        }
        else
        {
            dicDecoCount[decoType]++;
        }
        return dicDecoCount[decoType];
    }
    public void ResetTool()
    {
        windowMode = EditWindowMode.HOME;
        GameManager.instance.state = GameState.EDIT;
        mEditAnchor.SetActive(true);

        ManagerBlock.instance.targetY = 0;
        mEditAnchor.transform.localPosition = Vector3.zero;
        ManagerBlock.instance.InitGroundPos();
        ManagerBlock.instance._initGroundPos = Vector3.zero;
        GameUIManager.instance.groundAnchor.transform.localPosition = Vector3.zero;
        GameUIManager.instance.groundMoveTransform.localPosition = Vector3.zero;
        GameUIManager.instance.SetListGyroObject();

        GameManager.mExtend_X = 0;
        GameManager.mExtend_Y = 0;

        GameManager.MIN_X = 0;
        GameManager.MIN_Y = 0;

        GameManager.MOVE_Y = 0;
        GameManager.MOVE_X = 0;

        ManagerBlock.instance.RefreshBlockManagerByStageData();

        Edit_collider.SetActive(true);
        
        touchPos.y = 10f;
        MoveDown();
    }

    bool bSelectProbTap = false;
    bool bSelectReadyItemTap = false;
    void WindowEditHome(int windowID)
    {
        if (bSelectProbTap == false && bSelectReadyItemTap == false)
        {
            SetEditTap();
            
            if (GUI.Button(new Rect(140, 100, 43, 23), "레디"))
            {
                bSelectReadyItemTap = true;
            }
            
            if (GUI.Button(new Rect(185, 100, 50, 23), "맵구성"))
            {
                bSelectProbTap = true;
            }
        }
        else if (bSelectReadyItemTap == true)
        {
            SetReadyItemTap();
            if (GUI.Button(new Rect(390, 160, 70, 30), "홈 화면"))
            {
                bSelectReadyItemTap = false;
            }
        }
        else
        {
            SetProbTap();
            if (GUI.Button(new Rect(390, 160, 70, 30), "홈 화면"))
            {
                bSelectProbTap = false;
            }
        }
        
        EditBlockGuide.reflesh = true;
        GuiTab();
    }


    string[]            gameModeKind = { "일반", "땅파기", "용암", "모험", "코인" };
    private GUIDropDown gameModeDropDown = new GUIDropDown();
    
    void SetEditTap()
    {
        gameModeDropDown.CheckBlockingShowMenuGUI();
        
        float y = 45f;
        float x = 10f;
        int parseInt;

        GUI.Label(new Rect(x, y, 100, 20), "파일");
        _stringFileName = GUI.TextField(new Rect(x + 30, y, 100, 25), _stringFileName);
        x += 140f;

        if (GUI.Button(new Rect(x, y, 70, 30), "저장"))
        {
            message = "";
            mapState = SaveAndLoadState.Loading;
            stateText = string.Format("[{0}] 저장", _stringFileName);
            if (_stringFileName.Length > 0)
            {
                for (int i = 0; i < ManagerBlock.instance.stageInfo.ListBlock.Count; i++)
                {
                    BlockInfo blockA = ManagerBlock.instance.stageInfo.ListBlock[i];
                    if (blockA.isActiveBoard == 0 &&
                        blockA.isScarp == 0 &&
                        blockA.type == (int)BlockType.NONE &&
                        blockA.ListDeco.Count == 0
                        )
                    {
                        ManagerBlock.instance.stageInfo.ListBlock.Remove(blockA);
                    }
                }

                foreach (var temp in ManagerBlock.instance.stageInfo.ListStartInfo)
                {
                    temp.Probs = new List<int>();
                }

                SaveLevel();
                PlayerPrefs.SetString("fileName", _stringFileName);
            }
        }
        if (GUI.Button(new Rect(x + 75, y, 70, 30), "읽기"))
        {
            message = "";
            mapState = SaveAndLoadState.Loading;
            stateText = string.Format("[{0}] 읽기", _stringFileName);
            if (_stringFileName.Length > 0)
            {
                isOldVer = false;

                LoadLevel();
                PlayerPrefs.SetString("fileName", _stringFileName);
                InitBlockGuide();
                CreateBlockGuide();
                touchPos = Vector3.zero;
                mEditAnchor.transform.localPosition = GameUIManager.instance.groundAnchor.transform.localPosition;

                //태그 에디터 초기화
                editUIMapTag.InitTaggingData_ByLoadData();

                //에디터에서 사용하는 정보 초기화.
                ResetEditorManagerData();
            }
        }
        if (GUI.Button(new Rect(x + 150, y, 70, 30), "초기화"))
        {
            if (_stringFileName.Length > 0)
            {
                ManagerBlock.instance.InitGroundPos();
                ManagerBlock.instance.RefreshBlockManager();
                PlayerPrefs.SetString("fileName", _stringFileName);
                touchPos = Vector3.zero;
                mEditAnchor.transform.localPosition = GameUIManager.instance.groundAnchor.transform.localPosition;
                GameManager.MOVE_Y = 0;

                //에디터에서 사용하는 정보 초기화.
                ResetEditorManagerData();
            }
        }
        if (GUI.Button(new Rect(x + 240f, y, 70, 30), "게임시작"))
        {
            Global._cdnAddress = NetworkSettings.Instance.GetCDN_URL() + "/";    // MUST
            PlayerPrefs.SetString("fileName", _stringFileName);
            GameManager.instance.state = GameState.NONE;

            touchPos = Vector3.zero;
            ManagerBlock.instance.InitGroundPos();
            GameManager.MOVE_Y = 0;
            mEditAnchor.transform.localPosition = Vector3.zero;

            GameManager.instance.GameStart();
            Edit_collider.SetActive(false);
            mEditAnchor.SetActive(false);
            RemoveGimmickGuide_All();
        }

        if (isOldVer)
        {
            if (GUI.Button(new Rect(x + 140, y - 30, 70, 30), "구버전"))
            {
                //식물,보석 고치기
                foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
                {
                    if (blockInfo.type == (int)BlockType.PLANT_APPLE)
                    {
                        blockInfo.type = (int)BlockType.PLANT;
                        blockInfo.subType = (int)PLANT_TYPE.APPLE;
                    }
                    else if (blockInfo.type == (int)BlockType.PLANT_KEY)
                    {
                        blockInfo.type = (int)BlockType.PLANT;
                        blockInfo.subType = (int)PLANT_TYPE.KEY;
                    }
                    else if (blockInfo.type == (int)BlockType.PLANT_ICE_APPLE)
                    {
                        blockInfo.type = (int)BlockType.PLANT;
                        blockInfo.subType = (int)PLANT_TYPE.ICE_APPLE;
                    }
                    /*
                    else if (blockInfo.type == 14)//(int)BlockType.GROUND_JEWEL)
                    {
                        blockInfo.type = (int)BlockType.GROUND;
                        blockInfo.subType = (int)GROUND_TYPE.JEWEL;
                    }
                    */
                }
            }
        }


        y += 30f;
        x = 10f;

        GUI.Label(new Rect(x, y, 30, 25), "모드");
        ManagerBlock.instance.stageInfo.gameMode = gameModeDropDown.OnGUI(new Rect(x + 30, y, 50, 300), ManagerBlock.instance.stageInfo.gameMode, gameModeKind);

        if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.ADVENTURE)
        {
            ManagerBlock.instance.ClearStageInfo_TargetCount();
        }
        else
        {
            ManagerBlock.instance.stageInfo.battleWaveList.Clear();
        }

        if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.COIN)
        {
            ManagerBlock.instance.stageInfo.waitState = 0;
        }
        else
        {
            ManagerBlock.instance.stageInfo.waitState = 1;
        }

        
        if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.DIG || ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.LAVA)
        {
            GUI.Label(new Rect(x + 90, y, 30, 25), "길이");
            if (int.TryParse(GUI.TextField(new Rect(x + 115, y, 30, 25), ManagerBlock.instance.stageInfo.Extend_Y.ToString()), out parseInt))
            {
                if (ManagerBlock.instance.stageInfo.Extend_Y != parseInt && TouchScreenKeyboard.visible == false)
                {
                    ManagerBlock.instance.stageInfo.Extend_Y = parseInt;
                    ManagerBlock.instance.RefreshDigBlockManager(parseInt, ManagerBlock.instance.stageInfo.gameMode);

                    InitBlockGuide();
                    CreateBlockGuide();
                }
            }

            if (int.TryParse(GUI.TextField(new Rect(x + 145, y, 30, 25), ManagerBlock.instance.stageInfo.digCount.ToString()), out parseInt))
            {
                if (ManagerBlock.instance.stageInfo.digCount != parseInt) ManagerBlock.instance.stageInfo.digCount = parseInt;
            }
        }
        
        if (GUI.Toggle(new Rect(278, 130, 40, 20), ManagerBlock.instance.stageInfo.reverseMove == 1, "만우"))
        {
            ManagerBlock.instance.stageInfo.reverseMove = 1;
        }
        else
        {
            ManagerBlock.instance.stageInfo.reverseMove = 0;
        }
        
        if (GUI.Toggle(new Rect(318, 130, 40, 20), ManagerBlock.instance.stageInfo.isAppleStage == 1, "사과"))
        {
            ManagerBlock.instance.stageInfo.isAppleStage = 1;
        }
        else
        {
            ManagerBlock.instance.stageInfo.isAppleStage = 0;
        }
        
        x = 235;
        if (GUI.Toggle(new Rect(x, y + 25, 50, 20), ManagerBlock.instance.stageInfo.useProbability2 == 1, "확률2"))
            ManagerBlock.instance.stageInfo.useProbability2 = 1;
        else
            ManagerBlock.instance.stageInfo.useProbability2 = 0;
        

        x = 250f;

        if (int.TryParse(GUI.TextField(new Rect(x + 40, y, 30, 25), ManagerBlock.instance.stageInfo.probability[0].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[0] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 70, y, 30, 25), ManagerBlock.instance.stageInfo.probability[1].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[1] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 100, y, 30, 25), ManagerBlock.instance.stageInfo.probability[2].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[2] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 130, y, 30, 25), ManagerBlock.instance.stageInfo.probability[3].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[3] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 160, y, 30, 25), ManagerBlock.instance.stageInfo.probability[4].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[4] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 190, y, 30, 25), ManagerBlock.instance.stageInfo.probability[5].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[5] = parseInt;
        }


        if (int.TryParse(GUI.TextField(new Rect(x + 40, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[0].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[0] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 70, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[1].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[1] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 100, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[2].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[2] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 130, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[3].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[3] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 160, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[4].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[4] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 190, y + 25, 30, 25), ManagerBlock.instance.stageInfo.probability2[5].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability2[5] = parseInt;
        }
        
        y += 25f;
        x =  10f;
        
        if (GUI.Toggle(new Rect(x, y, 40, 30), ManagerBlock.instance.stageInfo.widthCentering == 1, "←"))
        {
            ManagerBlock.instance.stageInfo.widthCentering = 1;
            EditBlockGuide.reflesh                         = true;
        }
        else
        {
            ManagerBlock.instance.stageInfo.widthCentering = 0;
            EditBlockGuide.reflesh                         = true;
        }

        x += 42;
        
        if (GUI.Toggle(new Rect(x, y, 40, 30), ManagerBlock.instance.stageInfo.heightCentering == 1, "↓"))
        {
            ManagerBlock.instance.stageInfo.heightCentering    = 1;
            ManagerBlock.instance.stageInfo.heightDownOneSpace = 0;
            EditBlockGuide.reflesh                             = true;
        }
        else
        {
            ManagerBlock.instance.stageInfo.heightCentering = 0;
            EditBlockGuide.reflesh                          = true;
        }
        
        x += 42;
        
        if (GUI.Toggle(new Rect(x, y, 40, 30), ManagerBlock.instance.stageInfo.heightDownOneSpace == 1, "↓↓"))
        {
            ManagerBlock.instance.stageInfo.heightDownOneSpace = 1;
            ManagerBlock.instance.stageInfo.heightCentering    = 0;
            EditBlockGuide.reflesh                             = true;
        }
        else
        {
            ManagerBlock.instance.stageInfo.heightDownOneSpace = 0;
            EditBlockGuide.reflesh                             = true;
        }

        x = 10f;
        y += 25f;
        GUI.Label(new Rect(x, y, 60, 20), "시작");
        int.TryParse(GUI.TextField(new Rect(x + 30, y, 25, 25), ManagerBlock.instance.stageInfo.turnCount.ToString()), out ManagerBlock.instance.stageInfo.turnCount);

        x += 58;
        if (ManagerBlock.instance.IsStageTarget(TARGET_TYPE.CARPET))
        {
            GUI.Label(new Rect(x, y, 100, 20), "카펫:" + ManagerBlock.instance.GetCollectTargetCount(TARGET_TYPE.CARPET));
        }
        else if (IsTargetCountCrack() == true)
        {
            string text = "";
            for (int i = 1; i <= ManagerBlock.MAXCOUNT_COUNTCRACK; i++)
            {
                text += string.Format("{0},", ManagerBlock.instance.GetCollectTargetCount(ManagerBlock.instance.GetCollectTypeCountCrackByInteger(i)));
            }
            GUI.Label(new Rect(x, y, 100, 20), text);
        }
        else
        {
            GUI.Label(new Rect(x, y, 100, 20), "석판:" + ManagerBlock.instance.GetCollectTargetCount(TARGET_TYPE.CRACK));
        }

        x += 43;

        // 맵에 지정돼있는 컬러블럭위에 보너스타입의 폭탄이 생길 수 있는지(레디아이템, 부스팅 아이템... 등)
        // * 해당 기능을 사용하면 컬러 지정된 블럭을 제외한 맵에 있는 블럭이 보너스로 지급되는 폭탄의 최대수량보다 많아야 합니다.
        if (GUI.Toggle(new Rect(x, y + 2, 100, 20), ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor == false, "폭탄생성제한"))
            ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor = false;
        else
            ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor = true;
        x += 30;

        //시작 시 화살표 방향 설정.
        if (ManagerBlock.instance.stageInfo.isStartLineH == 0)
        {
            if (GUI.Button(new Rect(x + 65, y + 5, 25, 20), "V"))
            {
                ManagerBlock.instance.stageInfo.isStartLineH = 1;
            }
        }
        else
        {
            if (GUI.Button(new Rect(x + 65, y + 5, 25, 20), "H"))
            {
                ManagerBlock.instance.stageInfo.isStartLineH = 0;
            }
        }

        #region 파일 다운로드 경로 설정버튼
        if (ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT)
            GUI.enabled = false;

        if (this.downLoadAdressType == DownLoadAdressType.Download_Default)
        {
            string text = (ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT) ? "외부" : "사내";

            if (GUI.Button(new Rect(x + 95, y + 5, 40, 20), text))
                this.downLoadAdressType = DownLoadAdressType.Download_Ext;
        }
        else
        {
            if (GUI.Button(new Rect(x + 95, y + 5, 40, 20), "외부"))
                this.downLoadAdressType = DownLoadAdressType.Download_Default;
        }
        if (ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT)
            GUI.enabled = true;
        #endregion

        x = 10f;
        y += 30f;

        GUI.Label(new Rect(x, y, 40, 20), "목표");
        x += 30f;

        //목표 입력란 설정
        SetCountInputBox_NormalTarget(x, y);
        x = 35f;

        //목표 타입 텍스트 출력.
        for (int i = 0; i < System.Enum.GetNames(typeof(TARGET_TYPE)).Length; i++)
        {
            if (ManagerBlock.instance.IsStageTarget((TARGET_TYPE)i) == false)
            {
                if ((TARGET_TYPE)i != TARGET_TYPE.ICE || (ManagerBlock.instance.hasDeco_CheckAllBlock(BoardDecoType.ICE) == false
                     && ManagerBlock.instance.hasDeco_CheckStartInfo(BoardDecoType.ICE) == false))
                    continue;
            }

            if (i == (int)TARGET_TYPE.COLORBLOCK || i == (int)TARGET_TYPE.CARPET || i == (int)TARGET_TYPE.COUNT_CRACK1 || i == (int)TARGET_TYPE.COUNT_CRACK2 || i == (int)TARGET_TYPE.COUNT_CRACK3 || i == (int)TARGET_TYPE.COUNT_CRACK4
                 || i == (int)TARGET_TYPE.CRACK || i == (int)TARGET_TYPE.FLOWER_INK || i == (int)TARGET_TYPE.BOMB_COLLECT )
                continue;

            if (i == (int)TARGET_TYPE.BOMB_ALL    ||
                i == (int)TARGET_TYPE.BOMB_LINE   ||
                i == (int)TARGET_TYPE.BOMB_CIRCLE ||
                i == (int)TARGET_TYPE.BOMB_RAINBOW)
                continue;

            x = SetLabelAndGetXPos(x, y, GetTargetTypeText((TARGET_TYPE)i));
        }

        //폭탄 목표 입력란
        y -= 30f;

        SetCountInputBox_NormalTarget2(360, y);

        GUI.Label(new Rect(360 + 25 * 0, y + 20, 30, 25), "폭탄");
        GUI.Label(new Rect(360 + 25 * 1, y + 20, 50, 25), "라인");
        GUI.Label(new Rect(360 + 25 * 2, y + 20, 30, 25), "둥근");
        GUI.Label(new Rect(360 + 25 * 3, y + 20, 50, 25), "레인");

        x += 5f;
        y += 25f;

        //목표 입력란 설정
        SetCountInputBox_ColorTarget(355, y + 10);

        //컬러를 사용하는 목표 설정
        GUI.Label(new Rect(355 + 25 * 0, y + 30, 30, 25), "핑크");
        GUI.Label(new Rect(355 + 25 * 1, y + 30, 50, 25), "브라");
        GUI.Label(new Rect(355 + 25 * 2, y + 30, 30, 25), "노랑");
        GUI.Label(new Rect(355 + 25 * 3, y + 30, 50, 25), "주황");
        GUI.Label(new Rect(355 + 25 * 4, y + 30, 50, 25), "파랑");

        x += 10f;

        //잉크 목표 입력란 설정
        float flowerInkPosX = SetCountInputBox_FlowerInkTarget(x, y);
        x = flowerInkPosX;

        //목표 갱신 버튼
        if (GUI.Button(new Rect(x, y, 35, 40), "목표\n갱신"))
        {
            ManagerBlock.instance.SetTargetAllCount();
        }

        if (GUI.Toggle(new Rect(100, -3, 52, 20), ManagerBlock.instance.stageInfo.isHardStage == 1, "(H)"))
        {
            ManagerBlock.instance.stageInfo.isHardStage = 1;
            editUIMapTag.SetHardTag(true);
        }
        else
        {
            ManagerBlock.instance.stageInfo.isHardStage = 0;
            editUIMapTag.SetHardTag(false);
        }

        //태그 UI 생성
        editUIMapTag.MapTagUI();
    }

    bool IsTargetCountCrack()
    {
        for (int i = 1; i <= ManagerBlock.MAXCOUNT_COUNTCRACK; i++)
        {
            if (ManagerBlock.instance.IsStageTarget(ManagerBlock.instance.GetCollectTypeCountCrackByInteger(i)) == true)
                return true;
        }
        return false;
    }

    #region 스테이지 목표 설정

    //컬러가 아닌 형태의 목표를 입력하는 칸 생성해주는 함수
    private void SetCountInputBox_NormalTarget(float posX, float posY)
    {
        //컬러 형을 사용하지 않는 목표 설정.
        for (int i = 0; i < System.Enum.GetNames(typeof(TARGET_TYPE)).Length; i++)
        {
            if (ManagerBlock.instance.IsStageTarget((TARGET_TYPE)i) == false)
            {
                if ((TARGET_TYPE)i != TARGET_TYPE.ICE || (ManagerBlock.instance.hasDeco_CheckAllBlock(BoardDecoType.ICE) == false
                     && ManagerBlock.instance.hasDeco_CheckStartInfo(BoardDecoType.ICE) == false))
                    continue;
            }

            if (i == (int)TARGET_TYPE.COLORBLOCK || i == (int)TARGET_TYPE.FLOWER_INK || i == (int)TARGET_TYPE.BOMB_COLLECT || i == (int)TARGET_TYPE.CARPET ||
                i == (int)TARGET_TYPE.CRACK || i == (int)TARGET_TYPE.COUNT_CRACK1 || i == (int)TARGET_TYPE.COUNT_CRACK2 || i == (int)TARGET_TYPE.COUNT_CRACK3 || i == (int)TARGET_TYPE.COUNT_CRACK4)
                continue;
            
            if (i == (int)TARGET_TYPE.BOMB_ALL    ||
                i == (int)TARGET_TYPE.BOMB_LINE   ||
                i == (int)TARGET_TYPE.BOMB_CIRCLE ||
                i == (int)TARGET_TYPE.BOMB_RAINBOW)
                continue;
            
            //타겟 값 가져오기
            int targetCount = 0;
            int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == i);
            if (findIndex > -1 && ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.Count > 0)
            {
                targetCount = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo[0].collectCount;
            }

            int inputCount = 0;
            int.TryParse(GUI.TextField(new Rect(posX, posY, 25, 25), targetCount.ToString()), out inputCount);
            posX += 25;
            //입력한 값과 원래의 값이 다를 경우에만 처리해줌.
            if (targetCount == inputCount)
                continue;

            targetCount = inputCount;
            ManagerBlock.instance.SetStageInfo_TargetCount(i, targetCount);
        }
    }

    //컬러가 아닌 형태의 목표를 입력하는 칸(윗칸) 생성해주는 함수
    private void SetCountInputBox_NormalTarget2(float posX, float posY)
    {
        //컬러 형을 사용하지 않는 목표 설정.
        for (int i = 0; i < System.Enum.GetNames(typeof(TARGET_TYPE)).Length; i++)
        {
            if (i != (int)TARGET_TYPE.BOMB_ALL &&
                i != (int)TARGET_TYPE.BOMB_LINE &&
                i != (int)TARGET_TYPE.BOMB_CIRCLE &&
                i != (int)TARGET_TYPE.BOMB_RAINBOW)
                continue;

            //타겟 값 가져오기
            int targetCount = 0;
            int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == i);
            if (findIndex > -1 && ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.Count > 0)
            {
                targetCount = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo[0].collectCount;
            }

            int inputCount = 0;
            int.TryParse(GUI.TextField(new Rect(posX, posY, 25, 25), targetCount.ToString()), out inputCount);
            posX += 25;
            //입력한 값과 원래의 값이 다를 경우에만 처리해줌.
            if (targetCount == inputCount)
                continue;

            targetCount = inputCount;
            ManagerBlock.instance.SetStageInfo_TargetCount(i, targetCount);
        }
    }

    private float SetCountInputBox_FlowerInkTarget(float posX, float posY)
    {
        TARGET_TYPE flowerInkTarget = TARGET_TYPE.FLOWER_INK;

        for (int j = 1; j < System.Enum.GetNames(typeof(BlockColorType)).Length; j++)
        {
            if (ManagerBlock.instance.hasDeco_CheckAllBlock(BoardDecoType.FLOWER_INK, (BlockColorType)j) == false)
                continue;

            int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == (int)flowerInkTarget);
            int targetCount = 0;
            int inputCount = 0;
            int colorIndex = -1;

            //타겟 값 가져오기
            if (findIndex > -1 && ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.Count > 0)
            {
                CollectTargetInfo targetInfo = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex];
                colorIndex = targetInfo.listTargetColorInfo.FindIndex(x => x.colorType == j);

                if (colorIndex > -1)
                {
                    targetCount = targetInfo.listTargetColorInfo[colorIndex].collectCount;

                }
            }

            int.TryParse(GUI.TextField(new Rect(posX, posY, 25, 25), targetCount.ToString()), out inputCount);
            DrawColorBox(posX, posY + 25, (BlockColorType)j);
            posX += 25f;

            //입력한 값과 원래의 값이 다를 경우에만 처리해줌.
            if (targetCount == inputCount)
                continue;

            targetCount = inputCount;
            ManagerBlock.instance.SetStageInfo_TargetCount((int)flowerInkTarget, targetCount, j);
        }
        posX += 5f;

        return posX;
    }

    //컬러 형태의 목표를 입력하는 칸 생성해주는 함수
    private void SetCountInputBox_ColorTarget(float posX, float posY)
    {
        for (int i = System.Enum.GetNames(typeof(TARGET_TYPE)).Length - 1; i >= 0; i--)
        {
            if (i != (int)TARGET_TYPE.COLORBLOCK)
                continue;

            for (int j = 1; j < 6; j++)
            {
                int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == i);
                int targetCount = 0;
                int inputCount = 0;
                int colorIndex = -1;

                //타겟 값 가져오기
                if (findIndex > -1 && ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.Count > 0)
                {
                    CollectTargetInfo targetInfo = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex];
                    colorIndex = targetInfo.listTargetColorInfo.FindIndex(x => x.colorType == j);
                    if (colorIndex > -1)
                        targetCount = targetInfo.listTargetColorInfo[colorIndex].collectCount;
                }

                int.TryParse(GUI.TextField(new Rect(posX, posY, 25, 25), targetCount.ToString()), out inputCount);
                posX += 25f;

                //입력한 값과 원래의 값이 다를 경우에만 처리해줌.
                if (targetCount == inputCount)
                    continue;

                targetCount = inputCount;
                ManagerBlock.instance.SetStageInfo_TargetCount(i, targetCount, j);
            }
            posX += 5f;
        }
    }

    /// <summary>
    /// 신버전 컬러 타입이 아닌 목표 데이터에 값을 넣어주기 위해 사용하는 함수.
    /// </summary>
    private void InputTargetCount_Normal(int targetType, int targetCount)
    {
        int colorType = (int)BlockColorType.NONE;
        int findIndex = ManagerBlock.instance.stageInfo.listTargetInfo.FindIndex(x => x.targetType == targetType);
        if (findIndex > -1)
        {   //기존에 데이터가 있으면, 해당 데이터에 값을 덮어씌워줌.
            int colorIndex = ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.FindIndex(x => x.colorType == colorType);
            if (colorIndex > -1)
            {
                ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo[colorIndex].collectCount = targetCount;
            }
            else
            {
                TargetColorInfo info = new TargetColorInfo()
                {
                    colorType = colorType,
                    collectCount = targetCount,
                };
                ManagerBlock.instance.stageInfo.listTargetInfo[findIndex].listTargetColorInfo.Add(info);
            }
        }
        else
        {   //기존에 데이터가 없는 경우, 데이터를 추가해줌
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


    #endregion

    float SetLabelAndGetXPos(float x, float y, string text)
    {
        GUI.Label(new Rect(x, y + 20, 35, 25), text);
        return x + 25f;
    }

    void SetProbTap()
    {
        float x = 20f;
        float y = 50f;
        int parseInt;

        //처음 맵에 생성될 블럭 확률.
        if (GUI.Toggle(new Rect(350, y, 150, 20), ManagerBlock.instance.stageInfo.isUseProbabilityMakMake == true, "맵구성 확률사용"))
            ManagerBlock.instance.stageInfo.isUseProbabilityMakMake = true;
        else
            ManagerBlock.instance.stageInfo.isUseProbabilityMakMake = false;

        //처음 맵에 생성될 블럭 확률.
        GUI.Label(new Rect(x, y, 150, 25), "맵 구성 블럭확률 입력");
        y += 23f;
        for (int i = 0; i < 6; i++)
        {
            if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), ManagerBlock.instance.stageInfo.probabilityMapMake[i].ToString()), out parseInt))
            {
                ManagerBlock.instance.stageInfo.probabilityMapMake[i] = parseInt;
                x += 30f;
            }
        }
    }
    void SetReadyItemTap()
    {
        float x = 20f;
        float y = 50f;
        int parseInt;

        //readyItemTest[0] = GUI.Toggle(new Rect(x, y, 70, 20), readyItemTest[0], "사과3");
        //x += 70;
        readyItemTest[1] = GUI.Toggle(new Rect(x, y, 70, 20), readyItemTest[1], "스코어업");
        x += 70;
        //readyItemTest[2] = GUI.Toggle(new Rect(x + 25, y, 50, 20), readyItemTest[2], "사과3");
        //x += 25;

        y += 25;

        //x = 20f;
        //readyItemTest[3] = GUI.Toggle(new Rect(x, y, 70, 20), readyItemTest[3], "라인폭탄");
        //x += 70;
        //readyItemTest[4] = GUI.Toggle(new Rect(x, y, 70, 20), readyItemTest[4], "둥근");
        //x += 70;
        //readyItemTest[5] = GUI.Toggle(new Rect(x, y, 70, 20), readyItemTest[5], "레인");
        //x += 70;
    }

    void LoadLevel()
    {
        StartCoroutine(CoLoad(_stringFileName + ".xml"));
    }

    bool isOldVer = false;
    void CheckVersion()
    {
        foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
        {
            if (blockInfo.type == (int)BlockType.PLANT_APPLE ||
                blockInfo.type == (int)BlockType.PLANT_KEY ||
                blockInfo.type == (int)BlockType.PLANT_ICE_APPLE ||
                blockInfo.type == 14//(int)BlockType.GROUND_JEWEL
                )
            {
                isOldVer = true;
            }
        }

        /*
        foreach (var blockstart in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            if (blockstart.type == 13)
            {
                isOldVer = true;
            }
        }
        */
    }

    IEnumerator CoLoad(string name)
    {
        IEnumerator e = InGameFileHelper2.WWW_DownLoad<StageInfo>(name);

        while (e.MoveNext())
        {
            yield return e.Current;
        }

        if (e.Current != null)
        {
            StageInfo tempData = e.Current as StageInfo;

            ManagerBlock.instance.stageInfo = new StageInfo();
            ManagerBlock.instance.stageInfo = tempData;
            ManagerBlock.instance.RefreshBlockManagerByStageData();

            //맵 로드 후, 에디터에서 사용하는 데이터 초기화
            InitEditDataUseMapData();

            //블럭 가이드 생성
            InitBlockGuide();
            CreateBlockGuide();

            //이전 버전인지 체크
            CheckVersion();

            yield return InGameFileHelper2.WWW_GetInfo(name);
            editUIMapTag.InitTaggingData_ByLoadData();

            mapState = SaveAndLoadState.Complete;
        }
        else
        {
            Debug.Log("다운로드실패");
            mapState = SaveAndLoadState.Failed;
        }
        bShowText = true;
        yield return null;
    }

    void SaveLevel()
    {
        ManagerBlock.instance.SortListTargetInfo();
        string filePath = Application.persistentDataPath + "/" + _stringFileName + ".xml";
        InGameFileHelper2.SaveLocal(filePath, ManagerBlock.instance.stageInfo);
        StartCoroutine(CoSave(filePath));
    }

    IEnumerator CoSave(string path)
    {
        mapState = SaveAndLoadState.Loading;

        #region 스크린 샷 로컬에 저장
        //탭을 맵 정보로 옮김.
        SetWindowMode(EditWindowMode.MAP_INFO);
        //블럭이 새로 그려지는 시간 때문에 기다림.
        yield return new WaitForSeconds(0.3f);

        var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        var rect = new Rect(0, 0, Screen.width, Screen.height);
        yield return new WaitForEndOfFrame();

        texture.ReadPixels(rect, 0, 0, false);
        texture.Apply();

        byte[] bytes = texture.EncodeToJPG();
        File.WriteAllBytes(Application.persistentDataPath + "/" + _stringFileName + ".png", bytes);

        //탭을 맵 화면으로 옮김.
        SetWindowMode(EditWindowMode.HOME);
        #endregion

        #region 신규 저장소에 저장
        IEnumerator e = InGameFileHelper2.WWW_Upload(_stringFileName + ".xml", bytes, GetGimmickString(), GetTargetString(), GetModeString(), GetDescriptionString(), GetAuthorString());
        while (e.MoveNext())
            yield return e.Current;

        yield return InGameFileHelper2.WWW_Tagging(_stringFileName + ".xml", editUIMapTag.GetArrayStringTagData(), GetAuthorString());

        if (e.Current != null && mapState != SaveAndLoadState.Failed)
        {
            Debug.Log("신규 저장소 - 업로드성공");
            mapState = SaveAndLoadState.Complete;
        }
        else
        {
            Debug.Log("신규 저장소 - 업로드실패");
            mapState = SaveAndLoadState.Failed;
        }
        bShowText = true;
        yield return null;
        #endregion
        bShowText = true;
    }

    string GetAuthorString()
    {
        return PlayerPrefs.GetString(mapMakerKey);
    }

    string GetDescriptionString()
    {
#if UNITY_EDITOR
        return "에디터";
#else
        ToolSettings.ToolType toolType = ToolSettings.Instance.toolType;

        switch(toolType)
        {
            case ToolSettings.ToolType.TOOL_REAL:
                return "리얼툴";
            case ToolSettings.ToolType.TOOL_DEV:
                return "개발툴";
            case ToolSettings.ToolType.TOOL_TEST:
                return "기믹툴";
            case ToolSettings.ToolType.TOOL_EXT:
                return "외부툴";
        }

        return toolType.ToString();
#endif
    }

    string GetModeString()
    {
        string modeString = ((GameMode)ManagerBlock.instance.stageInfo.gameMode).ToString();

        if (ManagerBlock.instance.listWorldRankItem != null && ManagerBlock.instance.listWorldRankItem.Count > 0)
        {
            modeString = string.Format("{0},{1}", modeString, "WORLD_RANK");
        }

        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            modeString = string.Format("{0},{1}", modeString, "REVERSE");
        }

        if(ManagerBlock.instance.stageInfo.isAppleStage == 1)
        {
            modeString = string.Format("{0},{1}", modeString, "APPLE");
        }
        
        return modeString;
    }

    string GetTargetString()
    {
        string targetString = "";

        for (int i = 0; i < ManagerBlock.instance.stageInfo.listTargetInfo.Count; i++)
        {
            TARGET_TYPE targetType = (TARGET_TYPE)ManagerBlock.instance.stageInfo.listTargetInfo[i].targetType;

            targetString = (targetString == "") ? targetType.ToString() : string.Format("{0},{1}", targetString, targetType);
        }

        return targetString;
    }

    string GetGimmickString()
    {
        UseGimmickData gimmickData = ManagerBlock.instance.GetUseGimmickData();

        string gimmickString = "";
        for (int i = 0; i < gimmickData.listUseBlockType.Count; i++)
        {
            BlockType blockType = (BlockType)gimmickData.listUseBlockType[i];
            if (blockType == BlockType.NONE || blockType == BlockType.NORMAL)
                continue;

            gimmickString = (gimmickString == "") ? blockType.ToString() : string.Format("{0},{1}", gimmickString, blockType);
        }

        for (int i = 0; i < gimmickData.listUseDecoType.Count; i++)
        {
            BoardDecoType decoType = (BoardDecoType)gimmickData.listUseDecoType[i];
            if (decoType == BoardDecoType.NONE || decoType == BoardDecoType.START || decoType == BoardDecoType.POTAL_OUT
                || decoType == BoardDecoType.KEY_EXIT || decoType == BoardDecoType.MAP_DECO)
                continue;

            if (gimmickData.listUseDecoType[i] == 100)
                continue;

            gimmickString = (gimmickString == "") ? decoType.ToString() : string.Format("{0},{1}", gimmickString, decoType);
        }

        for (int i = 0; i < gimmickData.listUseTopDecoType.Count; i++)
        {
            BoardDecoType decoType = (BoardDecoType)gimmickData.listUseTopDecoType[i];
            gimmickString = (gimmickString == "") ? decoType.ToString() : string.Format("{0},{1}", gimmickString, decoType);
        }
        return gimmickString;
    }

    void ShowSaveAndLoadState()
    {
        if (mapState == SaveAndLoadState.None)
            return;

        string fileState = "";
        if (mapState == SaveAndLoadState.Loading)
        {
            fileState = string.Format(" {0}중...", stateText);
        }
        else if (mapState == SaveAndLoadState.Complete)
        {
            fileState = string.Format("<color=#00ff00ff> {0} 성공 {1} </color>", stateText, message);
            SetStateCoroutine();
        }
        else
        {
            fileState = string.Format("<color=#ff0000ff> {0} 실패 {1} </color> ", stateText, message);
            SetStateCoroutine();
        }

        GUI.Label(new Rect(10f, 0, 150, 50), fileState);
    }

    void SetStateCoroutine()
    {
        if (bShowText == false)
            return;
        bShowText = false;

        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
            stateCoroutine = null;
        }
        stateCoroutine = StartCoroutine(CoShowSaveAndLoadText());
    }

    IEnumerator CoShowSaveAndLoadText()
    {
        yield return new WaitForSeconds(1.0f);
        mapState = SaveAndLoadState.None;
        message = "";
        stateCoroutine = null;
    }

    public void SetSaveLoadFailed(string error = "")
    {
        message = error;
        mapState = SaveAndLoadState.Failed;
    }

    //선택한 기믹 변경될 때, 변수들 초기화 해주는 함수.
    private void InitEditManagerData()
    {
        blockIndex = 0;
        blockType  = 0;
        blockCount = 0;
        appleCount = AppleCount.One;
        isBool     = false;
    }

    void windowBlock(int windowID)
    {
        float y = 45f;
        float x = 10f;

        int tempPaintMode = GUI.SelectionGrid(new Rect(x, y, 460, 100f), (int)EditMode.MaxCount, paintModeStrings1, 6);
        y += 105f;

        if (tempPaintMode != (int)EditMode.MaxCount)
        {
            if (eMode == (EditMode)tempPaintMode)
                paintCount[tempPaintMode]++;
            eMode = (EditMode)tempPaintMode;

            if (eModePreSelect != eMode)
            {
                InitEditManagerData();
                eModePreSelect = eMode;
            }
        }

        if (eMode == EditMode.None)
        {
            GuiTab();
            return;
        }

        _toolState = "Paint Mode : " + paintModeStrings1[(int)eMode % 19];

        if (eMode == EditMode.START)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            //출발 정보.
            ShowStartInfo();
        }
        else if (eMode == EditMode.PotalIn)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            if (blockIndex == 0) blockIndex = 1;
            SetIndexByTwoButtonAndInputField(ref blockIndex, 16, 50, 150);
        }
        else if (eMode == EditMode.PotalOut)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            if (blockIndex == 0) blockIndex = 1;
            SetIndexByTwoButtonAndInputField(ref blockIndex, 16, 50, 150);
        }
        else if (eMode == EditMode.EMPTY2)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }
        else if (eMode == EditMode.EMPTY3)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }
        else if (eMode == EditMode.CRACK1)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            if (ManagerBlock.instance.dicCountCrack.Count > 0)
            {
                SetText(x2: 500, text: "<color=#ff0000ff>단계석판이 설치되어 있어 석판을 설치할 수 없습니다.</color>");
            }
        }
        else if (eMode == EditMode.COUNT_CRACK)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            if (ManagerBlock.instance.IsStageTarget(TARGET_TYPE.CRACK) == true)
            {
                SetText(x2: 500, text: "<color=#ff0000ff>석판이  설치되어 있어 단계석판을 설치할 수 없습니다.</color>");
            }
            else
            {
                SetIndexByButton(1, ManagerBlock.MAXCOUNT_COUNTCRACK);
                SetText(x2: 300, y1: 145, text: GetCountCrackInfo());
                SetApple(x1: 300, x2: 30, x3: 0, y1: 145, offsetX: 40, toggleSize: 30, text: "사과");
                SetCount(x1: 300);
                SetButton(x1: 400, buttonEvent: SetCountCrackInfo);
            }
        }
        else if (eMode == EditMode.GRASSFENCEBLOCK)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetFence();
            x = 200;
            y = 160;
            GUIStyle selectStyle = new GUIStyle(GUI.skin.button);
            selectStyle.normal.textColor = Color.magenta;
            
            if(blockIndex == 0 || blockIndex > 4) blockIndex = 1;
            if(GUI.Button(new Rect(x, y, 40, 25), "상", blockIndex == 1 ? selectStyle : GUI.skin.button))
                blockIndex = 1;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "하", blockIndex == 3 ? selectStyle : GUI.skin.button))
                blockIndex = 3;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "좌", blockIndex == 4 ? selectStyle : GUI.skin.button))
                blockIndex = 4;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "우", blockIndex == 2 ? selectStyle : GUI.skin.button))
                blockIndex = 2;
        }
        else if (eMode == EditMode.FENCEBLOCK)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            
            x = 200;
            y = 160;
            GUIStyle selectStyle = new GUIStyle(GUI.skin.button);
            selectStyle.normal.textColor = Color.magenta;
            
            if(blockIndex == 0 || blockIndex > 4) blockIndex = 1;
            if(GUI.Button(new Rect(x, y, 40, 25), "상", blockIndex == 1 ? selectStyle : GUI.skin.button))
                blockIndex = 1;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "하", blockIndex == 3 ? selectStyle : GUI.skin.button))
                blockIndex = 3;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "좌", blockIndex == 4 ? selectStyle : GUI.skin.button))
                blockIndex = 4;
            x += 50;
            if(GUI.Button(new Rect(x, y, 40, 25), "우", blockIndex == 2 ? selectStyle : GUI.skin.button))
                blockIndex = 2;
        }
        else if (eMode == EditMode.NET)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";
        }
        else if (eMode == EditMode.BLOCKTYPE)
        {
            if (isLongClick) paintCount[(int)eMode] = 7;
            int ii = paintCount[(int)eMode] % 7;
            if (paintCount[(int)eMode] % 7 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/6 설치</color>";
        }
        else if (eMode == EditMode.APPLE)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetApple();
            SetStart(BlockType.APPLE);
            SetBlockAdjust(BlockType.APPLE, 250, 170);
        }
        else if (eMode == EditMode.KEY)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetStart(BlockType.KEY);
            SetBlockAdjust(BlockType.KEY);
            //SetGenerator(BlockType.KEY);
        }
        else if (eMode == EditMode.CANDY)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            isGroundTap = true;
            SetGroundTap_Change(showToggle: true, showButton: false, togglePosX: 45, togglePosY: 155, toggleSizeX: 40, toggleMinValue: 0);
        }
        else
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }

        GuiTab();
    }

    void windowPlant(int windowID)
    {
        float y = 45f;
        float x = 10f;
        int parseInt;

        int tempPaintMode = GUI.SelectionGrid(new Rect(x, y, 460, 100f), (int)EditMode.MaxCount, paintModeStrings2, 6); y += 105f;

        if (tempPaintMode != (int)EditMode.MaxCount)
        {
            tempPaintMode += 19;

            if (eMode == (EditMode)tempPaintMode)
                paintCount[tempPaintMode]++;
            eMode = (EditMode)tempPaintMode;

            if (eModePreSelect != eMode)
            {
                InitEditManagerData();
                eModePreSelect = eMode;
            }
        }

        else if (eMode == EditMode.None)
        {
            GuiTab();
            return;
        }

        _toolState = "Paint Mode : " + paintModeStrings2[(int)eMode % 19];


        if (eMode == EditMode.PLANT)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetPlantType();
            SetBoxColor();
            SetCandy(55);
            SetDuck(100);
        }
        else if (eMode == EditMode.PLANT_APPLE)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetApple();
        }
        else if (eMode == EditMode.GROUND)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetStart(BlockType.GROUND);
            SetBlockAdjust(BlockType.GROUND);

            //SetJewel();
        }
        else if (eMode == EditMode.PLANT_KEY)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";
        }
        else if (eMode == EditMode.GRASS)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";
        }
        else if (eMode == EditMode.BOMB)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetStart(BlockType.START_Bomb);
            SetBlockAdjust(BlockType.START_Bomb);
        }
        else if (eMode == EditMode.LINE)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (ii == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else if (ii == 1)
                _toolState += "<color=#00ff00ff> 가로 설치</color>";
            else if (ii == 2)
                _toolState += "<color=#00ff00ff> 세로 설치</color>";

            SetStart(BlockType.START_Line, 2);
            GUI.Label(new Rect(270, 170, 60, 20), "가로 세로");
            SetBlockAdjust(BlockType.START_Line);
        }
        else if (eMode == EditMode.RAINBOW)
        {
            if (isLongClick) paintCount[(int)eMode] = 7;
            int ii = paintCount[(int)eMode] % 7;
            if (paintCount[(int)eMode] % 7 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/6 설치</color>";

            SetStart(BlockType.START_Rainbow);
            SetBlockAdjust(BlockType.START_Rainbow);
        }
        else if (eMode == EditMode.LINKSIDE)
        {
            if (isLongClick) paintCount[(int)eMode] = 5;
            int ii = paintCount[(int)eMode] % 5;
            if (ii == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else if (ii == 1)
                _toolState += "<color=#00ff00ff>  UP </color>";
            else if (ii == 2)
                _toolState += "<color=#00ff00ff>  RIGHT </color>";
            else if (ii == 3)
                _toolState += "<color=#00ff00ff>  DOWN </color>";
            else if (ii == 4)
                _toolState += "<color=#00ff00ff>  LEFT </color>";
        }
        else if (eMode == EditMode.SIDEBLOCK)
        {
            if (isLongClick) paintCount[(int)eMode] = 5;
            int ii = paintCount[(int)eMode] % 5;
            if (ii == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else if (ii == 1)
                _toolState += "<color=#00ff00ff>  UP </color>";
            else if (ii == 2)
                _toolState += "<color=#00ff00ff>  RIGHT </color>";
            else if (ii == 3)
                _toolState += "<color=#00ff00ff>  DOWN </color>";
            else if (ii == 4)
                _toolState += "<color=#00ff00ff>  LEFT </color>";
        }
        else if (eMode == EditMode.STATUE)
        {
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else if (paintCount[(int)eMode] % 3 == 1)
                _toolState += "  <color=#00ff00ff>1 설치</color>";
            else
                _toolState += "  <color=#00ff00ff>2 설치</color>";

            int posX  = 10;
            SetIndexByTwoButton(ref blockIndex, x : posX, textWidth: 70, horizontal: false, maxValue: ManagerBlock.instance.listStatues.Count > 0 ? ManagerBlock.instance.listStatues.Max(x => x.index + 1) : 0);
            posX += 100;
            SetIndexByTwoButton(ref blockType, x : posX, text: "단계", textWidth: 70, horizontal: false);
            posX += 100;
            SetIndexByTwoButton(ref ManagerBlock.instance.stageInfo.statueCount, x : posX, text: "석상맵개수", textWidth: 70, horizontal: false);
            posX += 100;
            int preStatueIndex = statueIndex;
            SetIndexByTwoButton(ref statueIndex, x : posX, text: "현재단계", textWidth: 70, horizontal: false);
            
            if (preStatueIndex != statueIndex)
            {
                ManagerBlock.instance.RefreshBlockManagerByStageData();
            }
        }
        else if (eMode == EditMode.JEWEL)
        {
            if (isLongClick) paintCount[(int)eMode] = 5;
            int ii = paintCount[(int)eMode] % 5;

            if (paintCount[(int)eMode] % 5 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/4 설치</color>";

            SetStart(BlockType.GROUND_JEWEL);
            SetBlockAdjust(BlockType.GROUND_JEWEL);
        }
        else if (eMode == EditMode.PLANT2X2)
        {
            /*
            int ii = paintCount[(int)eMode] % 3;

            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            SetStart(BlockType.BOX);
            SetBoxColor();
            */
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetSubTarget();
            SetPlant2X2Type();
            SetIndex();
            SetCount(x1: 90);
            GUI.Label(new Rect(150, 170, 50, 20), "최대7");

        }
        else if (eMode == EditMode.STONE)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;

            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            SetStoneType();
            SetType(70);
            SetCandy(92);
        }
        else if (eMode == EditMode.BOX)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;

            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";
            //SetBoxColor();
            SetStart(BlockType.BOX);
            SetBlockAdjust(BlockType.BOX);
        }
        else if (eMode == EditMode.ICE_APPLE)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            SetStart(BlockType.ICE_APPLE);
            SetBlockAdjust(BlockType.ICE_APPLE);
        }
        else if (eMode == EditMode.BLOCK_BLACK)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetGroundTap_Change(SetBlockBlackTap, SetGroundBlockBlackTap);
        }
        else
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }

        GuiTab();
    }

    void windowGimmick(int windowID)
    {
        float y = 45f;
        float x = 10f;
        int parseInt;

        int tempPaintMode = GUI.SelectionGrid(new Rect(x, y, 460, 100f), (int)EditMode.MaxCount, paintModeStrings3, 6); y += 105f;

        if (tempPaintMode != (int)EditMode.MaxCount)
        {
            tempPaintMode += 38;

            if (eMode == (EditMode)tempPaintMode)
                paintCount[tempPaintMode]++;
            eMode = (EditMode)tempPaintMode;
        }

        else if (eMode == EditMode.None)
        {
            GuiTab();
            return;
        }

        _toolState = "Paint Mode : " + paintModeStrings3[(int)eMode % 19];

        if (eMode == EditMode.AUTO_TRIGGER)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;

            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";
        }
        else if (eMode == EditMode.MAP_DECO)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            
            if (blockIndex == 0) blockIndex = 1;
            SetIndexByTwoButtonAndInputField(ref blockIndex, 10, 50, 150);
        }
        else if (eMode == EditMode.ICE)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;

            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetStart(BlockType.ICE);
            SetIceAdjust();
        }
        else if (eMode == EditMode.SAND_BELT)
        {
            /*
            int ii = paintCount[(int)eMode] % 5;
            if (ii == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else if (ii == 1)
                _toolState += "<color=#00ff00ff>  UP </color>";
            else if (ii == 2)
                _toolState += "<color=#00ff00ff>  RIGHT </color>";
            else if (ii == 3)
                _toolState += "<color=#00ff00ff>  DOWN </color>";
            else if (ii == 4)
                _toolState += "<color=#00ff00ff>  LEFT </color>";
                */
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetIndex();
        }
        else if (eMode == EditMode.ICE_PLANT)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";
        }
        else if (eMode == EditMode.BOMB_PLANT)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            Set_BOMBPLANT_Type();
            //SetBoxColor();
        }
        else if (eMode == EditMode.GROUND_APPLE)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetApple();
            SetStart(BlockType.GROUND_APPLE);
            SetBlockAdjust(BlockType.GROUND_APPLE, 250, 170);
        }
        else if (eMode == EditMode.GROUND_ICE_APPLE)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetStart(BlockType.GROUND_ICE_APPLE);
            SetBlockAdjust(BlockType.GROUND_ICE_APPLE);
        }
        else if (eMode == EditMode.GROUND_BOMB)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            Set_BOMBPLANT_Type(0);
            SetStart(BlockType.GROUND_BOMB, 3, 3);
            SetBlockAdjust(BlockType.GROUND_BOMB, 230, 170);
            GUI.Label(new Rect(350, 170, 100, 20), "둥근 가로 세로");
        }
        else if (eMode == EditMode.GROUND_KEY)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetStart(BlockType.GROUND_KEY);
            SetBlockAdjust(BlockType.GROUND_KEY);
        }
        else if (eMode == EditMode.Event_Block)
        {
            if (isLongClick) paintCount[(int)eMode] = 5;
            int ii = paintCount[(int)eMode] % 5;
            if (paintCount[(int)eMode] % 5 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/4 설치</color>";

            SetGroundTap_Change(SetEventBlockTap, SetEventBlockTap, showToggle: false, normalTapName: "[식물]");
        }
        else if (eMode == EditMode.Event_Stone)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            Set_EventBlock();
        }
        else if (eMode == EditMode.ADVENTURE_POTION_HEAL)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + " 설치</color>";
            SetStart(BlockType.ADVENTURE_POTION_HEAL);
            SetBlockAdjust(BlockType.ADVENTURE_POTION_HEAL);
        }
        else if (eMode == EditMode.ADVENTURE_POTION_SKILL)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + " 설치</color>";
            SetStart(BlockType.ADVENTURE_POTION_SKILL);
            SetBlockAdjust(BlockType.ADVENTURE_POTION_SKILL);
        }
        else if (eMode == EditMode.DYNAMITE)
        {
            if (isLongClick) paintCount[(int)eMode] = 8;
            int ii = paintCount[(int)eMode] % 8;
            if (paintCount[(int)eMode] % 8 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/7 설치</color>";

            SetCountByButton(1, 3, "라이프");
            SetDynamiteLife();
            SetStart(BlockType.BLOCK_DYNAMITE);
            SetBlockAdjust(BlockType.BLOCK_DYNAMITE, 250, 170);
        }
        else if (eMode == EditMode.DUCK)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            isGroundTap = true;
            SetGroundTap_Change(showToggle: true, showButton: false, togglePosX: 45, togglePosY: 155, toggleSizeX: 40, toggleMinValue: 0);
        }
        else
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }

        GuiTab();
    }

    EditGimmickTap_2 gimmickTap_2 = EditGimmickTap_2.NONE;
    void windowGimmick2(int windowID)
    {
        switch (gimmickTap_2)
        {
            case EditGimmickTap_2.NONE:
                SetGimmickTap_2();
                break;

            case EditGimmickTap_2.BLOCKGENERATOR:
                SetBlockGeneratorTap();
                if (GUI.Button(new Rect(390, 160, 50, 25), "나가기"))
                {
                    EditBlockGuide.reflesh = true;
                    gimmickTap_2 = EditGimmickTap_2.NONE;
                }
                break;
        }
    }
    private void SetGimmickTap_2()
    {
        float y = 45f;
        float x = 10f;
        int tempPaintMode = GUI.SelectionGrid(new Rect(x, y, 460, 100f), (int)EditMode.MaxCount, paintModeStrings4, 6); y += 105f;

        if (tempPaintMode != (int)EditMode.MaxCount)
        {
            tempPaintMode += 57;

            if (eMode == (EditMode)tempPaintMode)
                paintCount[tempPaintMode]++;
            eMode = (EditMode)tempPaintMode;

            if (eModePreSelect != eMode)
            {
                InitGimmickModeButton_GimmickTap_2(eMode);
                eModePreSelect = eMode;
            }
        }

        else if (eMode == EditMode.None)
        {
            GuiTab();
            return;
        }

        _toolState = "Paint Mode : " + paintModeStrings4[(int)eMode % 19];

        if (eMode == EditMode.CARPET)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }
        else if (eMode == EditMode.ColorFlowerPot_Little)
        {
            if (isLongClick) paintCount[(int)eMode] = 8;
            int ii = paintCount[(int)eMode] % 8;
            if (paintCount[(int)eMode] % 8 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + (BlockColorType)(ii) + " 설치</color>";
        }
        else if (eMode == EditMode.COLOR_BIG_JEWEL)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetSubTargetByButton(0, 5);
            SetIndexByTwoButton(ref blockIndex, y: 165, maxValue: ManagerBlock.instance.listBigJewel.Count > 0 ? ManagerBlock.instance.listBigJewel.Max(x => x.index + 1) : 0);
        }
        else if (eMode == EditMode.FIRE_WORK)
        {
            if (isLongClick) paintCount[(int)eMode] = 8;
            int ii = paintCount[(int)eMode] % 8;
            if (paintCount[(int)eMode] % 8 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + (BlockColorType)(ii) + " 설치</color>";
            SetStart(BlockType.FIRE_WORK);
            SetBlockAdjust(BlockType.FIRE_WORK, 250, 170);
            SetCountByButton(1, 2);
        }
        else if (eMode == EditMode.COIN_STAGE)
        {
            SetCoinStage();
        }
        else if (eMode == EditMode.COIN_BAG)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            SetStart(BlockType.COIN_BAG);
            SetCoinBag();
        }
        else if (eMode == EditMode.PLANT_COIN)
        {
            if (isLongClick) paintCount[(int)eMode] = 4;
            int ii = paintCount[(int)eMode] % 4;
            if (paintCount[(int)eMode] % 4 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/3 설치</color>";

            SetCoinBag();
        }
        else if (eMode == EditMode.SODAJELLY)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            SetIndexByButton(1, 5);
        }
        else if (eMode == EditMode.PEA)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            SetStart(BlockType.PEA, 2);
            SetBlockAdjust(BlockType.PEA);
        }
        else if (eMode == EditMode.WORLD_RANK_ITEM)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetIndex(x2: 80, y1: 145, text: "최대 획득 수");

            SetCount(x2: 70, text: "설치 수");
            SetButton(x1: 150, x2: 80, text: "랜덤 생성", buttonEvent:
                () => { MakeBlockRandomPos(EditManager.instance.blockCount, BlockType.WORLD_RANK_ITEM, EditManager.instance.blockIndex, false); });
        }
        else if (eMode == EditMode.FLOWER_INK)
        {
            if (isLongClick) paintCount[(int)eMode] = 6;
            int ii = paintCount[(int)eMode] % 6;
            if (paintCount[(int)eMode] % 6 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/5 설치</color>";
        }
        else if (eMode == EditMode.SPACESHIP)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetStart(BlockType.SPACESHIP);
            SetBlockAdjust(BlockType.SPACESHIP);
        }
        else if (eMode == EditMode.SPACESHIP_EXIT)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }
        else if (eMode == EditMode.BLOCKGENERATOR)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            //생성기 설정 버튼.
            SetButton(x1: 150, text: "설정", buttonEvent: ClickEvent_SetBlockGeneratorTap);
        }
        else if (eMode == EditMode.PEA_BOSS)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";

            SetStart(BlockType.PEA_BOSS, 2);
            SetBlockAdjust(BlockType.PEA_BOSS);
        }
        else if (eMode == EditMode.RANDOM_BOX)
        {
            if (isLongClick) paintCount[(int)eMode] = 3;
            int ii = paintCount[(int)eMode] % 3;
            if (paintCount[(int)eMode] % 3 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + ii + "/2 설치</color>";
        }
        else if (eMode == EditMode.HEART)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            if (GUI.Button(new Rect(x, y, 35, 50), isHeartWayTap == false ? "배터\n리" : "<color=#ff0000ff>길</color>"))
            {
                isHeartWayTap = !isHeartWayTap;
            }

            if (isHeartWayTap == false)
            {
                SetButton_HeartIndex();
                SetButton_ColorType();
            }
            else
            {
                SetButton_HeartIndex();
                SetButton_HeartWay();
            }
        }
        else if (eMode == EditMode.PAINT)
        {
            if (isLongClick) paintCount[(int)eMode] = 8;
            int ii = paintCount[(int)eMode] % 8;
            if (paintCount[(int)eMode] % 8 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "<color=#00ff00ff> " + (BlockColorType)(ii) + " 설치</color>";

            BlockType bType = BlockType.PAINT;
            SetStart(bType, 2, 5);
            SetCountByButton(1, 2, "단계", x: 140);
            GUI.Label(new Rect(325, 145, 26, 20), "컬러");
            SetBlockAdjust(BlockType.PAINT);
        }

        GuiTab();
        ShowScreenArea(eMode);
    }

    void windowGimmick3(int windowID)
    {
        float y = 45f;
        float x = 10f;
        int tempPaintMode = GUI.SelectionGrid(new Rect(x, y, 460, 100f), (int)EditMode.MaxCount, paintModeStrings5, 6); y += 105f;

        if (tempPaintMode != (int)EditMode.MaxCount)
        {
            tempPaintMode += 76;

            if (eMode == (EditMode)tempPaintMode)
                paintCount[tempPaintMode]++;
            eMode = (EditMode)tempPaintMode;

            if (eModePreSelect != eMode)
            {
                InitEditManagerData();
                eModePreSelect = eMode;
            }
        }

        else if (eMode == EditMode.None)
        {
            GuiTab();
            return;
        }

        _toolState = "Paint Mode : " + paintModeStrings5[(int)eMode % 19];
        if (eMode == EditMode.BREAD)
        {
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetIndexByButton(1, 3);
        }
        else if (eMode == EditMode.WATERBOMB)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";

            SetStart(BlockType.WATERBOMB, 3);
            SetCountByButton(1, 2, "단계", 0);
            SetBlockAdjust(BlockType.WATERBOMB, 80);
            SetButton_WaterBomb();
        }
        else if (eMode == EditMode.CLOVER)
        {
            int ii = paintCount[(int)eMode] % 2;
            if (paintCount[(int)eMode] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }
        
        else if (eMode == EditMode.CANNON)
        {
            if (isLongClick) paintCount[(int)eMode] = 2;
            int ii = paintCount[(int)eMode] % 2;
            if (ii == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
            SetButton_Cannon();
        }

        GuiTab();
        ShowScreenArea(eMode);
    }

    public int waterBombTimeCount = 10;

    private void SetButton_WaterBomb()
    {
        int _waterBomb = 10;
        GUI.Label(new Rect(220, 170, 80, 20), "타임카운트");
        if (int.TryParse(GUI.TextField(new Rect(300, 170, 30, 25), waterBombTimeCount.ToString()), out _waterBomb))
        {
            waterBombTimeCount = _waterBomb;
        }
    }
    
    private void SetButton_Cannon()
    {
        // 카운트 세팅
        int _cannonCount = 10;
        GUI.Label(new Rect(30, 145, 80, 20), "카운트");
        if (int.TryParse(GUI.TextField(new Rect(33, 165, 30, 20), blockCount.ToString()), out _cannonCount))
            blockCount = _cannonCount;

        // 컬러 타입 세팅
        int x = -80;
        int y = -10;
        SetButton_ColorType(-80, -10);
        GUI.Label(new Rect(330 + x, 160 + y, 40, 30), "<color=#A5E5F1>전체</color>");
        if (GUI.Toggle(new Rect(330 + x, 175 + y, 20, 30), colorType == BlockColorType.NONE, ""))
        {
            colorType = BlockColorType.NONE;
            EditBlockGuide.reflesh = true;
        }
        GUI.Label(new Rect(360 + x, 160 + y, 40, 30), "랜덤");
        if (GUI.Toggle(new Rect(360 + x, 175 + y, 20, 30), colorType == BlockColorType.RANDOM, ""))
        {
            colorType = BlockColorType.RANDOM;
            EditBlockGuide.reflesh = true;
        }
        // 방향 타입 세팅
        SetButton_DirectionType(330, -20);
    }

    //게이지형 하트 전환 버튼
    public BlockColorType colorType = BlockColorType.A;
    private int tempBlockIndex = 0;

    private void SetButton_DirectionType(int x = 0, int y = 0)
    {
        GUIStyle selectStyle = new GUIStyle(GUI.skin.button);
        if (colorType == BlockColorType.NONE)
            selectStyle.normal.textColor = new Color(165f / 255f, 229f / 255f, 241f / 255f);
        else if (colorType == BlockColorType.RANDOM)
            selectStyle.normal.textColor = new Color(100f / 255f, 100f / 255f, 100f / 255f);
        else
            selectStyle.normal.textColor = GetBlockColorTypeColor(colorType);
        blockIndex = tempBlockIndex;
        if(blockIndex == 0 || blockIndex > 4) blockIndex = 1;
        if(GUI.Button(new Rect(x, 175 + y, 30, 25), "상", blockIndex == 1 ? selectStyle : GUI.skin.button))
            blockIndex = 1;
        x += 35;
        if(GUI.Button(new Rect(x, 175 + y, 30, 25), "하", blockIndex == 3 ? selectStyle : GUI.skin.button))
            blockIndex = 3;
        x += 35;
        if(GUI.Button(new Rect(x, 175 + y, 30, 25), "좌", blockIndex == 4 ? selectStyle : GUI.skin.button))
            blockIndex = 4;
        x += 35;
        if(GUI.Button(new Rect(x, 175 + y, 30, 25), "우", blockIndex == 2 ? selectStyle : GUI.skin.button))
            blockIndex = 2;
        tempBlockIndex = blockIndex;
    }

    private void SetButton_ColorType(int x = 0, int y = 0)
    {
        GUI.Label(new Rect(180 + x, 160 + y, 40, 30), GetBlockColorName_WithColorType(BlockColorType.A));
        if (GUI.Toggle(new Rect(180 + x, 175 + y, 20, 30), colorType == BlockColorType.A, ""))
        {
            colorType = BlockColorType.A;
            EditBlockGuide.reflesh = true;
        }
        GUI.Label(new Rect(210 + x, 160 + y, 40, 30), GetBlockColorName_WithColorType(BlockColorType.B));
        if (GUI.Toggle(new Rect(210 + x, 175 + y, 20, 30), colorType == BlockColorType.B, ""))
        {
            colorType = BlockColorType.B;
            EditBlockGuide.reflesh = true;
        }
        GUI.Label(new Rect(240 + x, 160 + y, 40, 30), GetBlockColorName_WithColorType(BlockColorType.C));
        if (GUI.Toggle(new Rect(240 + x, 175 + y, 20, 30), colorType == BlockColorType.C, ""))
        {
            colorType = BlockColorType.C;
            EditBlockGuide.reflesh = true;
        }
        GUI.Label(new Rect(270 + x, 160 + y, 40, 30), GetBlockColorName_WithColorType(BlockColorType.D));
        if (GUI.Toggle(new Rect(270 + x, 175 + y, 20, 30), colorType == BlockColorType.D, ""))
        {
            colorType = BlockColorType.D;
            EditBlockGuide.reflesh = true;
        }
        GUI.Label(new Rect(300 + x, 160 + y, 40, 30), GetBlockColorName_WithColorType(BlockColorType.E));
        if (GUI.Toggle(new Rect(300 + x, 175 + y, 20, 30), colorType == BlockColorType.E, ""))
        {
            colorType = BlockColorType.E;
            EditBlockGuide.reflesh = true;
        }
    }

    private void InitGimmickModeButton_GimmickTap_2(EditMode eMode)
    {
        //기믹 버튼 처음 눌렀을 때
        switch (eMode)
        {
            case EditMode.HEART:
                SetWayCountByBlockIndex();
                break;
            case EditMode.BLOCKGENERATOR:
                InitEditDataUseMapData();
                break;
        }
    }

    private void SetButton_HeartIndex()
    {
        List<BlockHeart> tempList = ManagerBlock.instance.listHeart;

        GUI.Label(new Rect(45, 175, 40, 25), "인덱스");

        if (tempList.Count == 0) blockIndex = 0;

        //인덱스 변경
        if (GUI.Button(new Rect(85, 175, 50, 25), blockIndex.ToString()))
        {
            if (tempList.Count != 0)
            {
                tempList.Sort((BlockHeart a, BlockHeart b) => a.heartIndex.CompareTo(b.heartIndex));

                blockIndex++;
                int lastIndex = tempList[tempList.Count - 1].heartIndex;
                if (lastIndex < blockIndex)
                {
                    blockIndex = 0;
                }
                SetWayCountByBlockIndex();
            }
        }

        if (tempList.Count == 0) return;

        //인덱스 추가
        if (GUI.Button(new Rect(135, 175, 30, 25), "+"))
        {
            //현재 하트 리스트 인덱스 중에서 가장 큰 값 가져오기
            int maxHeartIndex = 0;

            while (tempList.Find(x => maxHeartIndex < x.heartIndex) != null)
            {
                maxHeartIndex++;
            }

            blockIndex = maxHeartIndex + 1;
        }
    }

    public void SetWayCountByBlockIndex()
    {
        BlockHeart temp = ManagerBlock.instance.GetHeartByHeartIndex(blockIndex);

        if (temp != null) wayCount = temp.GetHeartWayCount();
        else wayCount = 0;
    }

    private void SetButton_HeartWay()
    {
        GUI.Label(new Rect(80, 150, 120, 25), "길 갯수 : " + wayCount);
    }

    public delegate void tapDelegate();

    private void SetGroundTap_Change(tapDelegate originTap = null, tapDelegate groundTap = null, int x = 10, int y = 145, bool showToggle = true, bool showButton = true, int togglePosX = 177, int togglePosY = 170, int toggleSizeX = 25, int toggleMinValue = 1, string normalTapName = "[일반]")
    {
        if (showButton == true)
        {
            if (GUI.Button(new Rect(x, y, 35, 50), isGroundTap == true ? "<color=#ff0000ff>[흙]</color>" : normalTapName))
            {
                isGroundTap = !isGroundTap;

                if (isGroundTap == false) groundCount = 0;
                else groundCount = 1;
            }
        }

        if (isGroundTap == false)
            originTap?.Invoke();
        else
            groundTap?.Invoke();

        if (showToggle == true && isGroundTap == true)
        {
            GUI.Label(new Rect(togglePosX, togglePosY, 40, 20), "흙단계");

            if (GUI.Button(new Rect(togglePosX + 40, togglePosY, toggleSizeX, 20), groundCount.ToString()))
            {
                if (groundCount == 3)
                    groundCount = toggleMinValue;
                else
                    groundCount++;
            }
        }
    }

    private void SetEventBlockTap()
    {
        Set_EventBlock();
        SetBoxColor();
    }

    private void SetBlockBlackTap()
    {
        SetStart(BlockType.BLOCK_BLACK, xX: 45);
        SetBlockAdjust(BlockType.BLOCK_BLACK, 260, 170);
    }
    private void SetGroundBlockBlackTap()
    {
        SetStart(BlockType.GROUND_BLOCKBLACK, xX: 45);
        SetBlockAdjust(BlockType.GROUND_BLOCKBLACK, 260, 170);
    }
    #region 블럭 생성기 탭
    private void ResetBlockGeneratorData()
    {
        dicGeneratorBlockInfo = null;
    }

    private void ClickEvent_SetBlockGeneratorTap()
    {
        gimmickTap_2 = EditGimmickTap_2.BLOCKGENERATOR;

        //생성기 선택 인덱스 초기화.
        SelectGeneratorIdx = 0;

        //생성기 카운트와 데이터 카운트 맞추기
        SyncDicGeneratorBlockInfoCount();

        //토글 데이터 초기화.
        InitDicToggleData_BlockGenerator();

        //토글 초기화
        InitToggle_MakeData_BlockGenerator(SelectGeneratorIdx);

        EditBlockGuide.reflesh = true;
    }

    //블럭 생성기 토글 데이터 초기화 해주는 함수.
    private void InitDicToggleData_BlockGenerator()
    {
        if (dicToggleData_BlockGenerator != null)
            return;

        //출발에서 생성될 수 있는 블럭 수 만큼 딕셔너리 생성
        dicToggleData_BlockGenerator = new Dictionary<BlockType, GeneratorToggleData>();
        for (int i = 0; i < listBlockTypeAndText.Count; i++)
        {
            dicToggleData_BlockGenerator.Add(listBlockTypeAndText[i].blockType, new GeneratorToggleData());
        }
    }

    private void InitDicGeneratorBlockInfo_UseMapData()
    {
        if (dicGeneratorBlockInfo != null)
            return;

        dicGeneratorBlockInfo = new Dictionary<int, GeneratorBlockInfo>();

        #region 맵에 찍힌 생성기로 사용할 정보 초기화
        foreach (var generator in ManagerBlock.instance.dicBlockGenerator)
        {
            int generatorIdx = generator.Key;
            if (dicGeneratorBlockInfo.ContainsKey(generatorIdx) == true)
                continue;

            GeneratorBlockInfo generatorBlockInfo = new GeneratorBlockInfo();
            BlockGenerator blockGenerator = generator.Value.First();
            BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(blockGenerator.inX, blockGenerator.inY);
            int decoIndex = blockInfo.ListDeco.FindIndex(x => x.BoardType == (int)BoardDecoType.BLOCK_GENERATOR);

            //현재 검사하고 있는 인덱스의 생성기를 사용하는 데코의 정보를 받아와, 해당 데코에서 생성 가능한 블럭과 컬러타입을 가져옴.
            DecoInfo decoInfo = blockInfo.ListDeco[decoIndex];
            generatorBlockInfo.listCanMakeBlock = GetListCanMakeType_Block(decoInfo);
            //일반블럭이 아닌 기믹들의 생성 가능 컬러를 받아옴
            generatorBlockInfo.dicCanMakeColor = GetDicCanMakeType_Color(decoInfo);
            //일반블럭의 생성 가능 컬러를 받아옴
            generatorBlockInfo.dicCanMakeColor.Add(BlockType.NORMAL, decoInfo.count);

            //스테이지 정보에서 현재 검사하고 있는 인덱스의 생성기의 대표 이미지를 가져옴.
            GimmickInfo_BlockGenerator gimmickInfo = ManagerBlock.instance.GeBlockGeneratorInfo_AtDecoInfo();
            if (gimmickInfo != null && gimmickInfo.listImageInfo.Count > 0)
            {
                int gimmickInfoIndex = gimmickInfo.listImageInfo.FindIndex(x => x.generatorIndex == generatorIdx);
                if (gimmickInfoIndex != -1)
                {
                    generatorBlockInfo.listTitleImageData = gimmickInfo.listImageInfo[gimmickInfoIndex].listBlockAndColorData;
                }
            }
            //딕셔너리에 추가.
            dicGeneratorBlockInfo.Add(generatorIdx, generatorBlockInfo);
        }
        #endregion
    }

    //블럭 생성기 카운트 맞춰주는 함수
    private void SyncDicGeneratorBlockInfoCount()
    {
        if (dicGeneratorBlockInfo.Count == 0)
        {
            dicGeneratorBlockInfo.Add(SelectGeneratorIdx, new GeneratorBlockInfo());
        }
        else if (dicGeneratorBlockInfo.Count - 1 != dicGeneratorBlockInfo.Last().Key)
        {
            for (int i = 0; i < dicGeneratorBlockInfo.Count; i++)
            {
                if (dicGeneratorBlockInfo.ContainsKey(i) == false)
                {
                    dicGeneratorBlockInfo.Add(i, new GeneratorBlockInfo());
                }
            }
        }
    }

    public Vector2 scrollPos_BlockGeneratorTap = Vector2.zero;
    public bool allType_Toggle = false;

    private void SetBlockGeneratorTap()
    {
        //스크롤바 두께 수정
        GUI.skin.verticalScrollbar.fixedWidth = 30f;
        GUI.skin.verticalScrollbarThumb.fixedWidth = 30f;

        scrollPos_BlockGeneratorTap = GUI.BeginScrollView(new Rect(10, 25, 460, 100), scrollPos_BlockGeneratorTap, new Rect(0, 0, 0, scrollLine * 35));

        GUI.Box(new Rect(0, 0, 440, scrollLine * 35), "");
        scrollLine = 0;
        
        //토글 선택 창.
        SetToggle_AppearBlockType();

        GUI.EndScrollView();

        //출발 블럭 컬러 설정.
        SetToggle_AppearColorType();

        if (GUI.Button(new Rect(350, 130, 80, 25), allType_Toggle ? "전체해제" : "전체선택"))
        {
            allType_Toggle = !allType_Toggle;
            SetGeneratorToggle_BlockAndColorType_All(allType_Toggle);

            //일반 블럭은 기본 true
            dicToggleData_BlockGenerator[BlockType.NORMAL].isCanMakeBlock = true;

            RefreshDicGeneratorBlockInfo(SelectGeneratorIdx);
            RefreshStartBlockInfo_BlockGenerator(SelectGeneratorIdx);
        }

        if (GUI.Button(new Rect(10, 160, 50, 25), "생성기"))
        {
            paintCount[(int)EditMode.BLOCKGENERATOR]++;

            _toolState = "Paint Mode : " + paintModeStrings4[(int)EditMode.BLOCKGENERATOR % 19];
            if (paintCount[(int)EditMode.BLOCKGENERATOR] % 2 == 0)
                _toolState += "  <color=#ff0000ff>제거</color>";
            else
                _toolState += "  <color=#00ff00ff>설치</color>";
        }

        //인덱스 설정
        SetButton_GeneratorIndex();

        if (GUI.Button(new Rect(300, 160, 80, 25), "상세 정보"))
        {
            SetPopupData();
        }
    }

    #region 블럭 타입 토글 설정
    //등장할 블럭 타입을 설정할 토글 UI 생성
    private void SetToggle_AppearBlockType()
    {
        int x = 10; int y = 10;
        bool isChangeData = false;
        
        GUIStyle style = new GUIStyle(GUI.skin.toggle);
        style.normal.textColor = Color.white;
        style.fontSize         = 11;

        for (int i = 0; i < listBlockTypeAndText.Count; i++)
        {
            BlockTypeAndText_AtBlockMake data = listBlockTypeAndText[i];
            bool isMakeBlock_Temp = GUI.Toggle(new Rect(x, y, 70, 20), dicToggleData_BlockGenerator[data.blockType].isCanMakeBlock, data.blockName, style);

            //토글 값이 달라졌는지 검사.
            if (dicToggleData_BlockGenerator[data.blockType].isCanMakeBlock != isMakeBlock_Temp)
            {
                dicToggleData_BlockGenerator[data.blockType].isCanMakeBlock = isMakeBlock_Temp;
                isChangeData = true;

                //일반 블럭 설정한 경우 색 토글 값도 함께 갱신
                if (data.blockType == BlockType.NORMAL)
                {
                    RefreshDicGeneratorColorInfo(SelectGeneratorIdx, BlockType.NORMAL);   //딕셔너리 정보 갱신
                    RefreshGeneratorStartColorInfo_StageInfo(SelectGeneratorIdx);   //맵 데이터 정보 갱신
                }

                //출발에서 제거된 경우, 대표 이미지에서도 제거
                if (dicToggleData_BlockGenerator[data.blockType].isCanMakeBlock == false)
                {
                    dicToggleData_BlockGenerator[data.blockType].isShow = false;
                    RefreshDicGeneratorTitleInfo(SelectGeneratorIdx);
                    RefreshDecoInfo_BlockGeneratorTitleImage();
                }
            }

            x += 70;
            if (i % 6 == 5)
            {
                x = 10;
                y += 25;
                scrollLine++;
            }
        }

        //토글 값이 달라졌을 경우, 현재 인덱스를 가지고 있는 블럭 생성기의 데이터 갱신
        if (isChangeData == true)
        {
            RefreshDicGeneratorBlockInfo(SelectGeneratorIdx);
            RefreshStartBlockInfo_BlockGenerator(SelectGeneratorIdx);
        }
    }

    //생성될 수 있는 블럭 정보에 대한 에디터 데이터를 갱신해주는 함수.
    private void RefreshDicGeneratorBlockInfo(int generatorIdx)
    {
        if (dicGeneratorBlockInfo.ContainsKey(generatorIdx) == false)
            return;

        GeneratorBlockInfo generatorBlockInfo = dicGeneratorBlockInfo[generatorIdx];
        generatorBlockInfo.listCanMakeBlock = new List<BlockType>();

        //현재 에디터에서의 토글 설정을 읽어와, 생성될 수 있는 블럭에 대한 정보를 갱신해줌.
        foreach (var temp in dicToggleData_BlockGenerator)
        {
            if (temp.Value.isCanMakeBlock == true)
            {
                generatorBlockInfo.listCanMakeBlock.Add(temp.Key);
            }
        }
    }

    //인덱스에 해당하는 생성기들의 stageinfo 정보를 갱신해줌
    private void RefreshStartBlockInfo_BlockGenerator(int generatorIdx)
    {
        //출발 설정 가져옴.
        int[] arrStartBlockType = GetArrStartBlockType_AtDicToggleData_BlockGenerator();

        //인덱스에 해당하는 생성기들의 DecoInfo에 출발 설정 값을 넣어줌.
        if (ManagerBlock.instance.dicBlockGenerator.ContainsKey(generatorIdx) == true)
        {
            for (int i = 0; i < ManagerBlock.instance.dicBlockGenerator[generatorIdx].Count; i++)
            {
                BlockGenerator blockGenerator = ManagerBlock.instance.dicBlockGenerator[generatorIdx][i];

                BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(blockGenerator.inX, blockGenerator.inY);
                int infoIndex = blockInfo.ListDeco.FindIndex(x => x.BoardType == (int)BoardDecoType.BLOCK_GENERATOR);
                if (infoIndex > -1)
                {
                    blockInfo.ListDeco[infoIndex].index = arrStartBlockType[0];
                    blockInfo.ListDeco[infoIndex].type = arrStartBlockType[1];
                }
            }
        }
    }
    #endregion


    #region 컬러 타입 토글 설정
    //등장할 컬러 타입을 설정할 토글 UI 생성
    private void SetToggle_AppearColorType()
    {
        int x = 10; int y = 130;
        bool isChangeData = false;

        for (int i = 0; i < 5; i++)
        {
            BlockColorType cType = (BlockColorType)(i + 1);
            bool isMakeColor_Temp = GUI.Toggle(new Rect(x + (50 * i), y, 50, 20), generatorBlockColor[i], GetColorTypeName(cType));

            //토글 값이 달라졌는지 검사.
            if (generatorBlockColor[i] != isMakeColor_Temp)
            {
                generatorBlockColor[i] = isMakeColor_Temp;
                isChangeData = true;
            }
        }

        //토글 값이 달라졌을 경우, 현재 인덱스를 가지고 있는 블럭 생성기의 데이터 갱신
        if (isChangeData == true)
        {
            RefreshDicGeneratorColorInfo(SelectGeneratorIdx, BlockType.NORMAL);
            RefreshGeneratorStartColorInfo_StageInfo(SelectGeneratorIdx);
        }
    }

    //생성될 수 있는 블럭 정보에 대한 딕셔너리 데이터 갱신해주는 함수.
    private void RefreshDicGeneratorColorInfo(int generatorIdx, BlockType bType)
    {
        if (dicGeneratorBlockInfo.ContainsKey(generatorIdx) == false)
            return;

        //이전에 저장되어 있던 컬러 데이터 초기화
        GeneratorBlockInfo generatorBlockInfo = dicGeneratorBlockInfo[generatorIdx];
        if (generatorBlockInfo.dicCanMakeColor.ContainsKey(bType))
            generatorBlockInfo.dicCanMakeColor[bType] = 0;
        else
            generatorBlockInfo.dicCanMakeColor.Add(bType, 0);

        //현재 에디터에서의 토글 설정을 읽어와, 생성될 수 있는 블럭에 대한 정보를 갱신해줌.
        int canMakeColor = 0;
        if (bType == BlockType.NORMAL)
        {
            for (int i = 0; i < generatorBlockColor.Length; i++)
            {
                int colorIdx = (i + 1);
                if (generatorBlockColor[i] == true)
                {
                    canMakeColor += (1 << colorIdx);
                }
            }
        }
        else
        {
            bool isExistData = (dicToggleData_StartColorType.ContainsKey(bType));
            canMakeColor = (isExistData == true) ? dicToggleData_StartColorType[bType] : -1;
        }
        generatorBlockInfo.dicCanMakeColor[bType] = canMakeColor;
    }

    //인덱스에 해당하는 생성기들의 stageinfo 정보를 갱신해줌
    private void RefreshGeneratorStartColorInfo_StageInfo(int gIdx)
    {
        //인덱스에 해당하는 생성기들의 DecoInfo에 출발 설정 값을 넣어줌.
        if (ManagerBlock.instance.dicBlockGenerator.ContainsKey(gIdx) == true)
        {
            for (int i = 0; i < ManagerBlock.instance.dicBlockGenerator[gIdx].Count; i++)
            {
                BlockGenerator blockGenerator = ManagerBlock.instance.dicBlockGenerator[gIdx][i];
                BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(blockGenerator.inX, blockGenerator.inY);
                int infoIndex = blockInfo.ListDeco.FindIndex(x => x.BoardType == (int)BoardDecoType.BLOCK_GENERATOR);
                if (infoIndex > -1)
                {
                    //생성기에서 생성될 수 있는 블럭의 컬러를 가져옴
                    var item = EditManager.instance.GetListColorData_AtGeneratorBlockColor(gIdx);
                    blockInfo.ListDeco[infoIndex].count = item.Item1;
                    blockInfo.ListDeco[infoIndex].listBlockColorData = item.Item2;
                }
            }
        }
    }

    #endregion

    private void SetButton_GeneratorIndex()
    {
        GUI.Label(new Rect(140, 160, 40, 25), "인덱스");

        //인덱스 변경
        if (GUI.Button(new Rect(180, 160, 50, 25), SelectGeneratorIdx.ToString()))
        {
            //마지막 인덱스인 경우
            if (SelectGeneratorIdx == (dicGeneratorBlockInfo.Count - 1))
            {
                SelectGeneratorIdx = 0;
            }
            else
            {
                SelectGeneratorIdx++;
            }
        }

        //인덱스 추가
        if (GUI.Button(new Rect(230, 160, 30, 25), "+"))
        {
            int addIndex = dicGeneratorBlockInfo.Count;

            if (dicGeneratorBlockInfo.ContainsKey(addIndex))
            {
                //설치되지 않은 인덱스에 빈 정보 저장
                for (int i = 0; i < dicGeneratorBlockInfo.Count; i++)
                {
                    if (dicGeneratorBlockInfo.ContainsKey(i) == false)
                    {
                        dicGeneratorBlockInfo[i] = new GeneratorBlockInfo();
                    }
                }

                //기존 데이터 덮어쓰기
                dicGeneratorBlockInfo[addIndex] = new GeneratorBlockInfo();
            }
            else
            {
                //새로운 데이터 추가.
                dicGeneratorBlockInfo.Add(addIndex, new GeneratorBlockInfo());
            }

            //현재 인덱스 선택 상태를 변경해줌.
            SelectGeneratorIdx = addIndex;
        }
    }

    #region 토글 값 초기화 - 블럭 생성기
    //토글 값과 해당 인덱스에 해당하는 생성기의 값을 초기화.
    void InitToggle_MakeData_BlockGenerator(int generatorIndex)
    {
        //토글 전부 false 상태로 초기화
        SetGeneratorToggle_BlockAndColorType_All(false);
        SetGeneratorToggle_TitleImage_All(false);

        //현재 인덱스의 데이터를 찾아, 해당 데이터를 토글에 표시.
        if (dicGeneratorBlockInfo.ContainsKey(generatorIndex) == true)
        {
            //생성 가능한 블럭 타입의 토글을 활성화.
            List<BlockType> listCanMakeBlock = dicGeneratorBlockInfo[generatorIndex].listCanMakeBlock;
            for (int i = 0; i < listCanMakeBlock.Count; i++)
            {
                dicToggleData_BlockGenerator[listCanMakeBlock[i]].isCanMakeBlock = true;
            }

            //생성 가능한 컬러 타입의 토글을 활성화.
            int canMakeColor = dicGeneratorBlockInfo[generatorIndex].dicCanMakeColor.ContainsKey(BlockType.NORMAL) ?
                dicGeneratorBlockInfo[generatorIndex].dicCanMakeColor[BlockType.NORMAL] : int.MaxValue;

            for (int i = 0; i < generatorBlockColor.Length; i++)
            {
                generatorBlockColor[i] = ((canMakeColor & (1 << (i + 1))) != 0);
            }

            //대표 이미지 토글을 활성화
            List<BlockAndColorData> listTitleData = dicGeneratorBlockInfo[generatorIndex].listTitleImageData;
            for (int i = 0; i < listTitleData.Count; i++)
            {
                BlockType blockType = (BlockType)listTitleData[i].blockType;
                dicToggleData_BlockGenerator[blockType].isShow = true;

                BlockColorType colorType = (BlockColorType)listTitleData[i].blockColorType;
                if (colorType != BlockColorType.NONE && colorType != BlockColorType.RANDOM)
                    dicToggleData_BlockGenerator[blockType].listShowColorType.Add(colorType);

                dicToggleData_BlockGenerator[blockType].subType = listTitleData[i].subType;
            }
        }
    }

    //블럭 및 컬러타입에 대한 토글 값 전체 갱신
    void SetGeneratorToggle_BlockAndColorType_All(bool in_set)
    {
        for (int i = 0; i < listBlockTypeAndText.Count; i++)
        {
            dicToggleData_BlockGenerator[listBlockTypeAndText[i].blockType].isCanMakeBlock = in_set;
        }
    }

    //블럭 및 컬러타입에 대한 토글 값 전체 갱신
    void SetGeneratorToggle_TitleImage_All(bool in_set)
    {
        for (int i = 0; i < listBlockTypeAndText.Count; i++)
        {
            GeneratorToggleData generatorToggleData = dicToggleData_BlockGenerator[listBlockTypeAndText[i].blockType];
            generatorToggleData.isShow = in_set;
            generatorToggleData.listShowColorType = new List<BlockColorType>();
            generatorToggleData.subType = -1;
        }
    }
    #endregion

    //블럭 생성기 팝업 생성
    private void OpenPopupGimmickSetting_AtBlockGenerator()
    {
        if (popupData == null)
            return;

        //팝업 뷰
        GUI.backgroundColor = Color.black;
        GUI.Window(5, popupData.popupRect, popupData.windowFunc, new GUIContent());
    }

    private void SetPopupData()
    {
        popupData = new IngameEditPopupData();
        popupData.name = "상세 정보";
        popupData.windowFunc = SetPopupGimmickSetting_AtBlockGenerator;
    }

    //블럭 생성기 팝업 내부 설정.
    private void SetPopupGimmickSetting_AtBlockGenerator(int windowID)
    {
        //콜라이더 설정
        GameUIManager.instance.editPopupCollider.gameObject.SetActive(true);

        //팝업창 더 선명하게 보이게 박스 생성
        GUI.Box(new Rect(0, 0, popupData.popupRect.width, popupData.popupRect.height), "");
        GUI.Box(new Rect(0, 0, popupData.popupRect.width, popupData.popupRect.height), popupData.name);
        GUI.Box(new Rect(0, 0, popupData.popupRect.width, popupData.popupRect.height), popupData.name);
        GUI.Box(new Rect(0, 0, popupData.popupRect.width, popupData.popupRect.height), popupData.name);

        //기믹별 설정 탭 생성.
        SetBlockProbView_BlockGenerator();

        //창 닫기 버튼
        if (GUI.Button(new Rect(popupData.popupRect.width - 35, 5, 30, 30), "X"))
        {
            GameUIManager.instance.editPopupCollider.gameObject.SetActive(false);
            popupData = null;
        }
    }

    public Vector2 scrollPos_BlockProb_BlockGenerator = Vector2.zero;
    public int[] tempColorProb = new int[5] { 0, 0, 0, 0, 0 };

    private int selectGeneratorIdx = 0;
    public int SelectGeneratorIdx
    {
        get { return selectGeneratorIdx; }
        set
        {
            if (selectGeneratorIdx != value)
            {
                selectGeneratorIdx = value;
                InitToggle_MakeData_BlockGenerator(selectGeneratorIdx);
            }
        }
    }

    #region 블럭 생성기 상세 정보란
    private void SetBlockProbView_BlockGenerator()
    {
        //인덱스 변경 버튼 처리.
        MakeBtnChangeIndex_BlockProbView_BlockGenerator();

        if (dicGeneratorBlockInfo.ContainsKey(SelectGeneratorIdx) == false)
            return;

        //생성할 수 있는 블럭의 리스트를 가져옴.
        List<BlockType> listCanMakeBlock = dicGeneratorBlockInfo[SelectGeneratorIdx].listCanMakeBlock;

        //스크롤 가능한 영역은 현재 설정한 블럭 수에 따라 변경되도록 설정.
        Rect posRect = new Rect(10, 45, popupData.popupRect.width - 10, popupData.popupRect.height - 45);
        Rect scrollRect = new Rect(0, 0, 0, (listCanMakeBlock.Count * (35 + 90)) + 50);

        //스크롤 시작
        scrollPos_BlockProb_BlockGenerator = GUI.BeginScrollView(posRect, scrollPos_BlockProb_BlockGenerator, scrollRect);

        Vector2Int tempPos = new Vector2Int(5, 0);

        //대표이미지 출력
        GUI.Label(new Rect(tempPos.x - 3, tempPos.y, posRect.width - 15, 40), "대표 이미지 :" + GetName_TitleBlockImage());
        tempPos.y += 20;

        //블럭 별 데이터 설정창
        bool isChangeToggleData = false;
        for (int i = 0; i < listCanMakeBlock.Count; i++)
        {
            BlockType bType = listCanMakeBlock[i];
            string gimmickName = GetBlockName_ByBlockType(bType);

            //이름 띄우기
            bool isShowBlock = GUI.Toggle(new Rect(tempPos.x, tempPos.y, 100, 25), dicToggleData_BlockGenerator[bType].isShow, "<b>" + gimmickName + "</b>");

            //토글 값이 달라졌는지 검사.
            if (dicToggleData_BlockGenerator[bType].isShow != isShowBlock)
            {
                dicToggleData_BlockGenerator[bType].isShow = isShowBlock;
                isChangeToggleData = true;
            }

            tempPos.y += 35;

            //설정창 띄우기
            if (bType != BlockType.NORMAL)
            {
                SetStart_BlockGenerator(bType, tempPos);
                tempPos.y += 65;
            }

            if (isShowBlock == true)
            {
                if (bType == BlockType.NORMAL || bType == BlockType.PAINT)
                {
                    //대표이미지 색 설정창 띄우기
                    if (isChangeToggleData == false)
                    {
                        isChangeToggleData = SetStartColorType_BlockGenerator(bType, tempPos);
                    }
                    else
                    {
                        SetStartColorType_BlockGenerator(bType, tempPos);
                    }
                    tempPos.y += 35;
                }
                else if (bType == BlockType.GROUND_BOMB)
                {
                    //서브타입 설정창 띄우기
                    if (isChangeToggleData == false)
                    {
                        isChangeToggleData = SetStartSubType_BlockGenerator(bType, tempPos);
                    }
                    else
                    {
                        SetStartSubType_BlockGenerator(bType, tempPos);
                    }
                    tempPos.y += 35;
                }
                else if (bType == BlockType.START_Line)
                {
                    //서브타입 설정하기
                    int findIndex = ManagerBlock.instance.stageInfo.ListStartInfo.FindIndex(x => x.type == (int)BlockType.START_Line);
                    if (findIndex > -1)
                    {
                        StartBlockInfo startInfo = ManagerBlock.instance.stageInfo.ListStartInfo[findIndex];

                        //가로/세로 폭탄 확인
                        if (startInfo.countProb[0] > 0 && startInfo.countProb[1] > 0)
                        {
                            //둘다 설정된 경우
                            dicToggleData_BlockGenerator[bType].subType = 2;
                        }
                        else if (startInfo.countProb[0] > 0)
                        {
                            //가로만 설정된 경우
                            dicToggleData_BlockGenerator[bType].subType = 0;
                        }
                        else
                        {
                            //세로만 설정된 경우
                            dicToggleData_BlockGenerator[bType].subType = 1;
                        }

                        isChangeToggleData = true;
                    }
                }

            }

            GUI.Label(new Rect(tempPos.x - 3, tempPos.y, posRect.width - 15, 40), "=============================================");
            tempPos.y += 25;
        }

        //달라진 토글 데이터가 있다면 실제 데이터를 갱신시켜줌.
        if (isChangeToggleData == true)
        {
            RefreshDicGeneratorTitleInfo(SelectGeneratorIdx);
            RefreshDecoInfo_BlockGeneratorTitleImage();
        }

        //스크롤 끝
        GUI.EndScrollView();
    }

    //인덱스 변환 버튼
    private void MakeBtnChangeIndex_BlockProbView_BlockGenerator()
    {
        //인덱스 변경
        GUI.Label(new Rect(2, 20, 40, 40), "인덱스");

        if (GUI.Button(new Rect(42, 20, 50, 25), SelectGeneratorIdx.ToString()))
        {
            if (SelectGeneratorIdx == (dicGeneratorBlockInfo.Count - 1))
                SelectGeneratorIdx = 0;
            else
                SelectGeneratorIdx++;
        }
    }

    //대표 이미지에 출력할 블럭 이름 가져옴.
    private string GetName_TitleBlockImage()
    {
        string titleImageName = "";

        //현재 생성기에서 사용하는 블럭에 대해서만 타이틀 이미지로 사용되는지 검사.
        if (dicGeneratorBlockInfo.ContainsKey(SelectGeneratorIdx) == false)
            return titleImageName;

        List<BlockType> listCanMakeBlock = dicGeneratorBlockInfo[SelectGeneratorIdx].listCanMakeBlock;
        for (int i = 0; i < listCanMakeBlock.Count; i++)
        {
            BlockType bType = listCanMakeBlock[i];
            if (dicToggleData_BlockGenerator.ContainsKey(bType) == false)
                continue;

            GeneratorToggleData data = dicToggleData_BlockGenerator[bType];
            if (data.isShow == false)
                continue;

            string blockName = GetBlockName_ByBlockType(bType);
            if (data.listShowColorType.Count == 0)
            {
                if (data.subType != -1)
                {
                    if (bType == BlockType.START_Line && data.subType == 2)
                    {
                        titleImageName += string.Format("{0}_{1}",
                            blockName, 0);
                        titleImageName += string.Format("{0}_{1}",
                            blockName, 1);
                    }
                    else
                    {
                        titleImageName += string.Format("{0}_{1}",
                            blockName, data.subType.ToString());
                    }
                }
                else
                {
                    titleImageName += blockName;
                }
            }
            else
            {
                if (data.subType != -1)
                {
                    for (int j = 0; j < data.listShowColorType.Count; j++)
                    {
                        titleImageName += string.Format("{0}_{1}_{2}",
                            blockName, data.subType.ToString(), data.listShowColorType[j].ToString());
                    }
                }
                else
                {
                    for (int j = 0; j < data.listShowColorType.Count; j++)
                    {
                        titleImageName += string.Format("{0}_{1}",
                            blockName, data.listShowColorType[j].ToString());
                    }
                }
            }
        }
        return titleImageName;
    }
    #endregion

    private void RefreshDicGeneratorTitleInfo(int generatorIdx)
    {
        if (dicGeneratorBlockInfo.ContainsKey(generatorIdx) == false)
            return;

        GeneratorBlockInfo generatorBlockInfo = dicGeneratorBlockInfo[generatorIdx];
        generatorBlockInfo.listTitleImageData = new List<BlockAndColorData>();

        //현재 에디터에서의 토글 설정을 읽어와, 타이틀 이미지를 갱신해줌.
        foreach (var temp in dicToggleData_BlockGenerator)
        {
            if (temp.Value.isShow == true)
            {
                if (temp.Value.listShowColorType.Count == 0)
                {
                    BlockAndColorData addData = new BlockAndColorData();
                    addData.blockType = (int)temp.Key;
                    addData.subType = temp.Value.subType;
                    generatorBlockInfo.listTitleImageData.Add(addData);
                }
                else
                {
                    for (int i = 0; i < temp.Value.listShowColorType.Count; i++)
                    {
                        BlockAndColorData addData = new BlockAndColorData();
                        addData.blockType = (int)temp.Key;
                        addData.blockColorType = (int)temp.Value.listShowColorType[i];
                        addData.subType = temp.Value.subType;
                        generatorBlockInfo.listTitleImageData.Add(addData);
                    }
                }
            }
        }
    }

    //실제 맵의 타이틀 정보 설정해주는 함수.
    private void RefreshDecoInfo_BlockGeneratorTitleImage()
    {
        //스테이지 정보에서 현재 맵의 블럭 생성기들의 데이터 가져오기.
        int gimmickIndex = ManagerBlock.instance.stageInfo.listDecoInfo.FindIndex(x => x.gimmickType == (int)BoardDecoType.BLOCK_GENERATOR);
        if (gimmickIndex == -1)
        {
            BlockGenerator_ImageInfo imageInfo = new BlockGenerator_ImageInfo()
            {
                generatorIndex = SelectGeneratorIdx,
                listBlockAndColorData = new List<BlockAndColorData>(),
            };

            GimmickInfo_BlockGenerator gimmickInfo_BlockGenerator = new GimmickInfo_BlockGenerator();
            gimmickInfo_BlockGenerator.gimmickType = (int)BoardDecoType.BLOCK_GENERATOR;
            gimmickInfo_BlockGenerator.listImageInfo.Add(imageInfo);

            ManagerBlock.instance.stageInfo.listDecoInfo.Add(gimmickInfo_BlockGenerator);
            gimmickIndex = (ManagerBlock.instance.stageInfo.listDecoInfo.Count - 1);
        }

        if (ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] is GimmickInfo_BlockGenerator == false)
            return;

        //가져온 생성기 정보에서, 현재 선택되어 있는 인덱스의 생성기 정보만 가져오기.
        GimmickInfo_BlockGenerator generatorInfo = ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] as GimmickInfo_BlockGenerator;
        int infoIndex = generatorInfo.listImageInfo.FindIndex(x => x.generatorIndex == SelectGeneratorIdx);
        if (infoIndex == -1)
        {
            BlockGenerator_ImageInfo imageInfo = new BlockGenerator_ImageInfo()
            {
                generatorIndex = SelectGeneratorIdx,
                listBlockAndColorData = new List<BlockAndColorData>(),
            };

            generatorInfo.listImageInfo.Add(imageInfo);
            infoIndex = generatorInfo.listImageInfo.Count - 1;
        }

        generatorInfo.listImageInfo[infoIndex].listBlockAndColorData = new List<BlockAndColorData>();
        List<BlockAndColorData> listBlockAndColorData = generatorInfo.listImageInfo[infoIndex].listBlockAndColorData;

        //현재 생성기 인덱스에서 사용할 수 있는 블럭들만 대표 이미지 설정 검사 실행.
        List<BlockType> listCanMakeBlock = dicGeneratorBlockInfo[SelectGeneratorIdx].listCanMakeBlock;
        for (int i = 0; i < listCanMakeBlock.Count; i++)
        {
            BlockType bType = listCanMakeBlock[i];
            if (dicToggleData_BlockGenerator.ContainsKey(bType) == false)
                continue;

            GeneratorToggleData data = dicToggleData_BlockGenerator[bType];
            if (data.isShow == false)
                continue;

            if (data.listShowColorType.Count == 0)
            {
                BlockAndColorData addData = new BlockAndColorData();
                addData.blockType = (int)bType;
                addData.subType = data.subType;
                listBlockAndColorData.Add(addData);
            }
            else
            {
                for (int j = 0; j < data.listShowColorType.Count; j++)
                {
                    BlockAndColorData addData = new BlockAndColorData();
                    addData.blockType = (int)bType;
                    addData.blockColorType = (int)data.listShowColorType[j];
                    addData.subType = data.subType;
                    listBlockAndColorData.Add(addData);
                }
            }
        }
    }

    #endregion

    //디바이스에서 보이는 영역의 최대를 표시
    private void ShowScreenArea(EditMode mode)
    {
        if (mode == EditMode.WORLD_RANK_ITEM)
            GameUIManager.instance.objScreenLine.SetActive(true);
        else
            GameUIManager.instance.objScreenLine.SetActive(false);
    }

    public int stoneType = 0;
    void SetStoneType()
    {
        if (GUI.Toggle(new Rect(5, 170, 45, 20), stoneType == 0, "일반"))
        {
            stoneType = 0;
        }
        if (GUI.Toggle(new Rect(50, 170, 45, 20), stoneType == 1, "사과"))
        {
            stoneType = 1;
        }
        if (GUI.Toggle(new Rect(95, 170, 45, 20), stoneType == 2, "폭탄"))
        {
            stoneType = 2;
        }
        if (GUI.Toggle(new Rect(140, 170, 45, 20), stoneType == 3, "열쇠"))
        {
            stoneType = 3;
        }
        if (GUI.Toggle(new Rect(185, 170, 30, 20), stoneType == 8, "물"))
        {
            stoneType = 8;
        }
        if (GUI.Toggle(new Rect(5, 150, 30, 20), stoneType == 9, "힐"))
        {
            stoneType = 9;
        }
        if (GUI.Toggle(new Rect(50, 150, 45, 20), stoneType == 10, "스킬"))
        {
            stoneType = 10;
        }
        if (GUI.Toggle(new Rect(95, 150, 45, 20), stoneType == 11, "거미"))
        {
            stoneType = 11;
        }
        GUI.Label(new Rect(275, 170, 170, 20), "0랜덤L1둥근2세로3가로");
    }

    //출발에서 생성 가능한 블럭 및 컬러 정보
    public static int _startPositionBlockType = int.MaxValue;
    public static int _startPositionBlockType2 = int.MaxValue;
    public static int _startPositionBlockColor = int.MaxValue;
    public int statueIndex = 0;

    int tempStartBlockType = 0;
    bool startNormalBlock = true;
    bool startGround = true;
    bool startBox = true;
    bool startNoColor = true;

    bool[] startBlockTypes = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
    bool[] startBlockColor = new bool[] { true, true, true, true, true };

    /*
    public bool MM_CRACK = true;
    public bool MM_POTAL = true;
    public bool MM_PLANT = true;
    public bool MM_STONE = true;

    public bool MM_WATER = true;
    public bool MM_NET = true;

    public bool MM_CANDY = true;
    public bool MM_SAND = false;
    public bool MM_BLACK = false;

    public bool MM_KEY = true;

    void windowMapMaker(int windowID)
    {
        int parseInt;

        float y = 45f;
        float x = 10f;

        if (GUI.Button(new Rect(x, y, 60, 30), "맵만들기"))
        {
            MapMakeManager.instance.GetMapFromWeb();
        }

        if (GUI.Button(new Rect(x + 120f, y, 70, 30), "게임시작"))
        {
            GameManager.instance.state = GameState.NONE;

            touchPos = Vector3.zero;
            ManagerBlock.instance.InitGroundPos();
            GameManager.MOVE_Y = 0;
            mEditAnchor.transform.localPosition = Vector3.zero;

            GameManager.instance.GameStart();
            Edit_collider.SetActive(false);
            mEditAnchor.SetActive(false);
            RemoveGimmickGuide_All();
        }

        x += 190;

        if (int.TryParse(GUI.TextField(new Rect(x + 40, y, 30, 25), ManagerBlock.instance.stageInfo.probability[0].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[0] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 70, y, 30, 25), ManagerBlock.instance.stageInfo.probability[1].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[1] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 100, y, 30, 25), ManagerBlock.instance.stageInfo.probability[2].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[2] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 130, y, 30, 25), ManagerBlock.instance.stageInfo.probability[3].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[3] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 160, y, 30, 25), ManagerBlock.instance.stageInfo.probability[4].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[4] = parseInt;
        }
        if (int.TryParse(GUI.TextField(new Rect(x + 190, y, 30, 25), ManagerBlock.instance.stageInfo.probability[5].ToString()), out parseInt))
        {
            ManagerBlock.instance.stageInfo.probability[5] = parseInt;
        }


        x = 10f;
        y += 30;

        MM_CRACK = GUI.Toggle(new Rect(x, y, 60, 20), MM_CRACK, "석판");
        x += 60;
        MM_POTAL = GUI.Toggle(new Rect(x, y, 60, 20), MM_POTAL, "포탈");
        x += 60;
        MM_KEY = GUI.Toggle(new Rect(x, y, 60, 20), MM_KEY, "열쇠");

        x = 10f;
        y += 25;

        MM_PLANT = GUI.Toggle(new Rect(x, y, 60, 20), MM_PLANT, "식물");
        x += 60;
        MM_STONE = GUI.Toggle(new Rect(x, y, 60, 20), MM_STONE, "바위");
        x += 60;
        MM_WATER = GUI.Toggle(new Rect(x, y, 60, 20), MM_WATER, "물");
        x += 60;
        MM_NET = GUI.Toggle(new Rect(x, y, 60, 20), MM_NET, "잡기돌");
        x += 60;

        x = 10f;
        y += 25;

        MM_CANDY = GUI.Toggle(new Rect(x, y, 60, 20), MM_CANDY, "쿠키");
        x += 60;
        MM_SAND = GUI.Toggle(new Rect(x, y, 60, 20), MM_SAND, "흙");
        x += 60;
        MM_BLACK = GUI.Toggle(new Rect(x, y, 60, 20), MM_BLACK, "연쇄폭탄");
        x += 60;

        x = 10f;
        y += 25;

        //턴
        GUI.Label(new Rect(x, y, 15, 20), "턴");
        int.TryParse(GUI.TextField(new Rect(x + 15, y, 25, 25), ManagerBlock.instance.stageInfo.turnCount.ToString()), out ManagerBlock.instance.stageInfo.turnCount);
        x += 55;

        GUI.Label(new Rect(x, y, 55, 20), "목표");
        x += 25;

        //목표 입력란 설정
        SetCountInputBox_NormalTarget(x, y);
        //목표 타입 텍스트 출력.
        for (int i = 0; i < System.Enum.GetNames(typeof(TARGET_TYPE)).Length; i++)
        {
            if (i == (int)TARGET_TYPE.COLORBLOCK || i == (int)TARGET_TYPE.CARPET || i == (int)TARGET_TYPE.COUNT_CRACK1 || i == (int)TARGET_TYPE.COUNT_CRACK2 || i == (int)TARGET_TYPE.COUNT_CRACK3 || i == (int)TARGET_TYPE.COUNT_CRACK4
                 || i == (int)TARGET_TYPE.CRACK || i == (int)TARGET_TYPE.FLOWER_INK || i == (int)TARGET_TYPE.BOMB_COLLECT)
                continue;
            x = SetLabelAndGetXPos(x, y, GetTargetTypeText((TARGET_TYPE)i));
        }

        //목표 입력란 설정
        SetCountInputBox_ColorTarget(x, y);
        //컬러를 사용하는 목표 설정
        GUI.Label(new Rect(x + 25 * 0, y + 20, 30, 25), "핑크");
        GUI.Label(new Rect(x + 25 * 1, y + 20, 50, 25), "브라");
        GUI.Label(new Rect(x + 25 * 2, y + 20, 30, 25), "노랑");
        GUI.Label(new Rect(x + 25 * 3, y + 20, 50, 25), "주황");
        GUI.Label(new Rect(x + 25 * 4, y + 20, 50, 25), "파랑");
        x += 5f;

        GUI.Label(new Rect(x + 25 * 5, y + 20, 30, 25), "핑크");
        GUI.Label(new Rect(x + 25 * 6, y + 20, 50, 25), "브라");
        GUI.Label(new Rect(x + 25 * 7, y + 20, 30, 25), "노랑");
        GUI.Label(new Rect(x + 25 * 8, y + 20, 50, 25), "주황");
        GUI.Label(new Rect(x + 25 * 9, y + 20, 50, 25), "파랑");
        GuiTab();
    }
    */

    void windowMapInfo(int windowID)
    {
        float y = 45f;
        float x = 10f;

        //파일명 표시.
        GUI.Label(new Rect(x, y, 40, 20), "파일 : ");
        GUI.TextField(new Rect(x + 40, y, 100, 22), _stringFileName);
        x += 150f;

        //난이도 표시.
        y = 47f;
        GUI.Label(new Rect(x, y, 50, 20), "난이도 : ");
        GUI.Label(new Rect(x + 50, y, 40, 20), ManagerBlock.instance.stageInfo.isHardStage == 1 ? "<color=#ff0000ff>어려움</color>" : "보통");
        x = 10f;
        y += 22f;

        //현재 게임 모드
        bool isShowLengthTap = false;
        if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.DIG || ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.LAVA)
            isShowLengthTap = true;
        else
            y += 10f;

        string modeText = "모드 : ";
        modeText += GetModeText();
        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            modeText += ", <color=#00ff00ff>만우절</color>";
        }
        if(ManagerBlock.instance.stageInfo.isAppleStage == 1)
        {
            modeText += ", <color=#00ff00ff>사과</color>";
        }
        GUI.Label(new Rect(x, y, 150, 20), modeText);

        //시작 시 미리 찍혀진 컬러블럭에 보너스 폭탄 생성 허용할지.
        x = 160f;
        string bombText = "폭탄생성 : ";
        bombText += ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor == true ? "랜덤" : "<color=#ff0000ff>제한</color>";
        GUI.Label(new Rect(x, y, 120, 20), bombText);

        //lava나 땅파기일 때 길이 텍스트.
        if (isShowLengthTap == true)
        {
            x = 10f;
            y += 22f;
            GUI.Box(new Rect(x, y, 200, 30), "");

            x += 2f;
            y += 3f;
            GUI.Label(new Rect(x, y, 30, 25), "길이 ");
            GUI.TextField(new Rect(x + 30, y, 30, 22), ManagerBlock.instance.stageInfo.Extend_Y.ToString());

            x += 70f;
            GUI.Label(new Rect(x, y, 30, 25), "이동 ");
            GUI.TextField(new Rect(x + 30, y, 30, 22), ManagerBlock.instance.stageInfo.digCount.ToString());
        }

        #region 시작 정보 표시(턴, 라인방향)
        //시작 정보 표시
        x = 10f;
        if (isShowLengthTap == true)
            y = 122f;
        else
            y = 110f;

        GUI.Label(new Rect(x, y, 60, 20), "시작 턴 :");
        GUI.TextField(new Rect(x + 55f, y, 25, 22), ManagerBlock.instance.stageInfo.turnCount.ToString());

        //시작 시 화살표 방향 설정.
        x += 80f;
        string lineText = "라인 방향 : ";
        lineText += ManagerBlock.instance.stageInfo.isStartLineH == 0 ? "세로(V)" : "가로(H)";
        GUI.Label(new Rect(x, y, 120, 20), lineText);
        #endregion

        #region 목표 표시
        x = 8f;
        if (isShowLengthTap == true)
            y = 147f;
        else
            y = 143f;
        GUI.Box(new Rect(x, y, 200, 43), GUIContent.none);

        x += 5f;
        y += 5f;
        List<TARGET_TYPE> listCollectType = new List<TARGET_TYPE>();
        List<BlockColorType> listCollectColorType = new List<BlockColorType>();

        bool isExistTarget = false;
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value != null)
            {
                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Value.collectCount == 0)
                        continue;

                    isExistTarget = true;
                    BlockColorType colorType = e.Current.Key;

                    GUI.TextField(new Rect(x, y, 25, 25), e.Current.Value.collectCount.ToString());

                    //일반타입은 텍스트 표시, 컬러타입은 컬러 박스 표시
                    if (colorType == BlockColorType.NONE)
                    {
                        x = SetLabelAndGetXPos(x, y, GetTargetTypeText(enumerator.Current.Key));
                        x += 2f;
                    }
                    else
                    {
                        DrawColorBox(x + 7, y + 25, colorType);
                        x += 27f;
                    }
                }
            }
        }

        if (isExistTarget == false)
        {
            GUI.Label(new Rect(x + 55, y + 5, 180, 25), "목표 없음");
        }
        #endregion

        #region 확률 표시
        x = 260f;
        y = 45f;
        GUI.Box(new Rect(x, y, 212, 97), GUIContent.none);

        float probStartX = 267f;

        //컬러표시
        x = probStartX + 20f;
        y = 50f;
        for (int i = 1; i < 6; i++)
        {
            DrawColorBox(x, y, (BlockColorType)i);
            x += 29f;
        }

        //확률 1 표시.
        x = probStartX + 12;
        y += 10f;
        GUI.Label(new Rect(probStartX, y, 10, 25), "1");
        for (int i = 0; i < 6; i++)
        {
            GUI.TextField(new Rect(x, y, 30, 23), ManagerBlock.instance.stageInfo.probability[i].ToString());
            x += 29f;
        }

        //확률 2 표시.
        x = probStartX + 12f;
        y += 25f;
        if (ManagerBlock.instance.stageInfo.useProbability2 == 1)
        {
            GUI.Label(new Rect(probStartX, y, 10, 25), "2");
            for (int i = 0; i < 6; i++)
            {
                GUI.TextField(new Rect(x, y, 30, 23), ManagerBlock.instance.stageInfo.probability2[i].ToString());
                x += 29f;
            }
        }
        else
        {
            GUI.Label(new Rect(x, y + 2, 180, 23), "확률 2 사용 안 함");
        }

        //맵 구성확률 표시.
        x = probStartX + 12f;
        y += 25f;
        if (ManagerBlock.instance.stageInfo.isUseProbabilityMakMake == true)
        {
            GUI.Label(new Rect(probStartX, y, 15, 25), "맵");
            for (int i = 0; i < 6; i++)
            {
                GUI.TextField(new Rect(x, y, 30, 23), ManagerBlock.instance.stageInfo.probabilityMapMake[i].ToString());
                x += 29f;
            }
        }
        else
        {
            GUI.Label(new Rect(x, y, 180, 25), "맵 구성 확률 사용 안 함");
        }
        #endregion

        #region 점수 표시
        x = 215f;
        y = 147f;
        GUI.Box(new Rect(x, y, 257, 40), GUIContent.none);

        //점수
        x += 2f;
        y += 8f;
        GUI.Label(new Rect(x, y, 10, 20), "1");
        x += 10f;
        GUI.TextField(new Rect(x, y, 50, 23), ManagerBlock.instance.stageInfo.score1.ToString());

        x += 53f;
        GUI.Label(new Rect(x, y, 10, 20), "2");
        x += 10f;
        GUI.TextField(new Rect(x, y, 50, 23), ManagerBlock.instance.stageInfo.score2.ToString());

        x += 53f;
        GUI.Label(new Rect(x, y, 10, 20), "3");
        x += 10f;
        GUI.TextField(new Rect(x, y, 50, 23), ManagerBlock.instance.stageInfo.score3.ToString());

        x += 53f;
        GUI.Label(new Rect(x, y, 10, 20), "4");
        x += 10f;
        GUI.TextField(new Rect(x, y, 50, 23), ManagerBlock.instance.stageInfo.score4.ToString());
        #endregion

        GuiTab();
    }

    int waveCount = 0;
    int bossWave = -1;
    EditAdvanceTap advanceTap = EditAdvanceTap.Animal_SETTING;

    // 목표 타입의 텍스트 가져오는 함수.
    private string GetTargetTypeText(TARGET_TYPE targetType)
    {
        switch (targetType)
        {
            case TARGET_TYPE.CRACK:
                return "석판";
            case TARGET_TYPE.KEY:
                return "열쇠";
            case TARGET_TYPE.STATUE:
                return "석상";
            case TARGET_TYPE.SHOVEL:
                return "쿠키";
            case TARGET_TYPE.JEWEL:
                return "보석";
            case TARGET_TYPE.BOMB_COLLECT:
                return "폭탄";
            case TARGET_TYPE.ICE:
                return "얼음";
            case TARGET_TYPE.CARPET:
                return "카펫";
            case TARGET_TYPE.FLOWER_POT:
                return "화단";
            case TARGET_TYPE.COUNT_CRACK1:
                return "석1";
            case TARGET_TYPE.COUNT_CRACK2:
                return "석2";
            case TARGET_TYPE.COUNT_CRACK3:
                return "석3";
            case TARGET_TYPE.COUNT_CRACK4:
                return "석4";
            case TARGET_TYPE.PEA:
                return "콩";
            case TARGET_TYPE.SPACESHIP:
                return "거미";
            case TARGET_TYPE.HEART:
                return "하트";
            case TARGET_TYPE.BREAD_1:
                return "빵1";
            case TARGET_TYPE.BREAD_2:
                return "빵2";
            case TARGET_TYPE.BREAD_3:
                return "빵3";
            case TARGET_TYPE.BOMB_ALL:
                return "폭탄";
            case TARGET_TYPE.BOMB_LINE:
                return "라인";
            case TARGET_TYPE.BOMB_CIRCLE:
                return "둥근";
            case TARGET_TYPE.BOMB_RAINBOW:
                return "레인";
        }
        return "";
    }

    //게임 모드 텍스트 가져오는 함수.
    private string GetModeText()
    {
        switch (ManagerBlock.instance.stageInfo.gameMode)
        {
            case (int)GameMode.ADVENTURE:
                return "모험";
            case (int)GameMode.DIG:
                return "땅파기";
            case (int)GameMode.LAVA:
                return "용암";
            case (int)GameMode.NORMAL:
                return "일반";
        }
        return "";
    }

    //블럭 컬러의 컬러값 가져오는 함수.
    private Color GetBlockColorTypeColor(BlockColorType colorType)
    {
        switch (colorType)
        {
            case BlockColorType.A:  //분홍
                return new Color(251f / 255f, 157f / 255f, 158f / 255f);
            case BlockColorType.B:  //갈색
                return new Color(131f / 255f, 71f / 255f, 39f / 255f);
            case BlockColorType.C:  //노랑
                return new Color(250f / 255f, 233f / 255f, 79f / 255f);
            case BlockColorType.D:  //주황
                return new Color(250f / 255f, 160f / 255f, 65f / 255f);
            case BlockColorType.E:  //파랑
                return new Color(81f / 255f, 115f / 255f, 200f / 255f);
        }
        return new Color(1f, 1f, 1f);
    }

    //블럭 컬러의 컬러값이 적용된 컬러이름 가져오는 함수.
    private string GetBlockColorName_WithColorType(BlockColorType colorType)
    {
        Color tempColor = GetBlockColorTypeColor(colorType);
        string hexColorString = ColorUtility.ToHtmlStringRGBA(tempColor);

        switch (colorType)
        {
            case BlockColorType.A:  //분홍
                return "<color=#" + hexColorString + ">분홍</color>";
            case BlockColorType.B:  //갈색
                return "<color=#" + hexColorString + ">갈색</color>";
            case BlockColorType.C:  //노랑
                return "<color=#" + hexColorString + ">노랑</color>";
            case BlockColorType.D:  //주황
                return "<color=#" + hexColorString + ">주황</color>";
            case BlockColorType.E:  //파랑
                return "<color=#" + hexColorString + ">파랑</color>";
        }
        return "<color=#ff0000ff>N/A</color>";
    }
    void windowAdvance(int windowID)
    {
        switch (advanceTap)
        {
            case EditAdvanceTap.Animal_SETTING:
                SetAnimalTap();
                if (GUI.Button(new Rect(390, 160, 70, 30), "내 캐릭터"))
                {
                    advanceTap = EditAdvanceTap.ENEMY_SETTING;
                }
                break;
            case EditAdvanceTap.ENEMY_SETTING:
                SetEnemyTap();
                if (GUI.Button(new Rect(390, 160, 70, 30), "적 캐릭터"))
                {
                    advanceTap = EditAdvanceTap.Animal_SETTING;
                }
                break;
        }
        GuiTab();
    }

    public List<ManagerAdventure.AnimalInstance> animalList = new List<ManagerAdventure.AnimalInstance>();
    void SetAnimalTap()
    {
        int parseInt;
        float y = 50f;
        float x = 35f;

        #region 내 동물 데이터 표시
        SetAnimalDataGuide(x, y);
        y += 20;

        if (animalList.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                ManagerAdventure.AnimalInstance tempAnimal = new ManagerAdventure.AnimalInstance();
                tempAnimal.idx = 1001 + i;
                tempAnimal.level = 1;
                tempAnimal.overlap = 1;
                animalList.Add(tempAnimal);
            }
        }

        for (int i = 0; i < animalList.Count; i++)
        {
            x = 10;
            GUI.Label(new Rect(x, y, 30, 25), (i + 1).ToString());
            x += 20;

            if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), animalList[i].idx.ToString()), out parseInt))
                animalList[i].idx = parseInt;
            x += 30;

            if (int.TryParse(GUI.TextField(new Rect(x, y, 35, 25), animalList[i].level.ToString()), out parseInt))
                animalList[i].level = parseInt;
            x += 35;

            if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), animalList[i].overlap.ToString()), out parseInt))
                animalList[i].overlap = parseInt;
            x += 30;
            y += 30;
        }
        #endregion
    }

    void SetAnimalDataGuide(float x, float y)
    {
        GUI.Label(new Rect(x, y, 30, 20), "idx");
        x += 25;
        GUI.Label(new Rect(x, y, 30, 20), "레벨");
        x += 35;
        GUI.Label(new Rect(x, y, 30, 20), "중첩");
    }

    void SetEnemyTap()
    {
        int parseInt;
        float y = 40f;
        float x = 20f;

        #region 웨이브 설정 탭
        GUI.Label(new Rect(x, y, 50, 20), "WAVE");
        x += 50;

        if (GUI.Button(new Rect(x, y, 30, 20), "+"))
        {
            BattleWaveInfo waveA = new BattleWaveInfo();
            waveA.enemyIndexList = new List<EnemyInfo>();
            EnemyInfo tempEnemy = new EnemyInfo();
            waveA.enemyIndexList.Add(tempEnemy);

            ManagerBlock.instance.stageInfo.battleWaveList.Add(waveA);
        }
        x += 50f;

        for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList.Count; i++)
        {
            if (GUI.Toggle(new Rect(x, y, 40, 20), waveCount == i, (" " + (i + 1).ToString())))
            {
                waveCount = i;
            }
            x += 40;
        }

        x += 10;
        if (GUI.Button(new Rect(x, y, 30, 20), "-"))
        {
            ManagerBlock.instance.stageInfo.battleWaveList.RemoveAt(ManagerBlock.instance.stageInfo.battleWaveList.Count - 1);
            waveCount = 0;
        }
        #endregion

        #region 적 데이터 표시
        y += 25;
        x = 35f;
        SetEnemyDataGuide(x, y);

        y += 20;
        if (ManagerBlock.instance.stageInfo.battleWaveList.Count > 0)
        {
            int xOffset = 230;
            int lineCount = 0;
            float startYPos = y;
            for (int j = 0; j < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count; j++)
            {
                // 캐릭터 칸이 3줄씩 찰때마다 다음 x축 라인으로 이동.
                if (j >= 3 && (j % 3 == 0))
                {
                    lineCount++;

                    y = startYPos - 20;
                    x = 35f + (xOffset * lineCount);
                    SetEnemyDataGuide(x, y);
                    y += 20;
                }

                x = 10 + (xOffset * lineCount);
                GUI.Label(new Rect(x, y, 30, 25), (j + 1).ToString());
                x += 20;

                if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].idx.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].idx = parseInt;
                x += 30;

                if (int.TryParse(GUI.TextField(new Rect(x, y, 35, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].life.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].life = parseInt;
                x += 35;

                if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].attPoint.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].attPoint = parseInt;
                x += 30;

                if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].TurnCount.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].TurnCount = parseInt;
                x += 30;
                if (int.TryParse(GUI.TextField(new Rect(x, y, 25, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].attribute.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].attribute = parseInt;
                x += 25;
                if (int.TryParse(GUI.TextField(new Rect(x, y, 30, 25), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].enemyHeight.ToString()), out parseInt))
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].enemyHeight = parseInt;
                x += 35;

                //현재 웨이브가 보스 단계라면, 보스 설정 탭 보이도록.
                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave == 1)
                {
                    if (GUI.Toggle(new Rect(x, y, 45, 20), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].isBoss == true, ""))
                    {
                        foreach (var temp in ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList)
                        {
                            temp.isBoss = false;
                        }
                        EnemyInfo tempEnemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j];

                        tempEnemyInfo.isBoss = true;
                        ManagerBlock.instance.stageInfo.bossInfo.idx = tempEnemyInfo.idx;
                        ManagerBlock.instance.stageInfo.bossInfo.attribute = tempEnemyInfo.attribute;
                    }
                    else
                    {
                        EnemyInfo tempEnemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j];

                        tempEnemyInfo.isBoss = false;
                        ManagerBlock.instance.stageInfo.bossInfo.idx = 0;
                        ManagerBlock.instance.stageInfo.bossInfo.attribute = 0;
                    }
                    //ShowBossSelectButton(x, y + 2, j, ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].idx, ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[j].attribute);
                }
                y += 25;
            }

            #region 적 데이터 추가/제거  버튼
            x = 10;
            y = 170;
            if (GUI.Button(new Rect(x, y, 30, 20), "+"))
            {
                EnemyInfo tempEnemy = new EnemyInfo();
                ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Add(tempEnemy);
            }
            x += 40;

            GUI.Label(new Rect(x, y, 50, 20), " 캐릭터 ");
            x += 50;

            if (GUI.Button(new Rect(x, y, 30, 20), "-"))
            {
                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count > 0)
                    ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.RemoveAt(ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count - 1);
            }
            #endregion

            x += 50;
            //보스 웨이브 설정.
            if (GUI.Toggle(new Rect(x, y, 80, 20), ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave == 1, " 보스연출 "))
            {
                if (bossWave > -1 && ManagerBlock.instance.stageInfo.battleWaveList.Count > bossWave)
                {
                    ManagerBlock.instance.stageInfo.battleWaveList[bossWave].bossWave = 0;
                }
                bossWave = waveCount;
                ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave = 1;
            }
            else
            {
                if (bossWave == ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave)
                    bossWave = -1;
                ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave = 0;

                foreach (var temp in ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList)
                    temp.isBoss = false;
            }
        }
        #endregion
    }

    void SetEnemyDataGuide(float x, float y)
    {
        GUI.Label(new Rect(x, y, 30, 20), "idx");
        x += 30;
        GUI.Label(new Rect(x, y, 30, 20), "Life");
        x += 30;
        GUI.Label(new Rect(x, y, 30, 20), "ATT");
        x += 35;
        GUI.Label(new Rect(x, y, 30, 20), "턴");
        x += 20;
        GUI.Label(new Rect(x, y, 30, 20), "속성");
        x += 35;
        GUI.Label(new Rect(x, y, 25, 20), "키");
        x += 25;
        if (ManagerBlock.instance.stageInfo.battleWaveList.Count > 1 &&
            ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave == 1)
        {
            GUI.Label(new Rect(x, y, 30, 20), "보스");
        }
    }

    void ShowBossSelectButton(float x, float y, int selectIndex, int idx, int attribute)
    {
        bool bSelect = false;
        if (ManagerBlock.instance.stageInfo.bossInfo.idx == idx && ManagerBlock.instance.stageInfo.bossInfo.attribute == attribute)
        {
            bSelect = true;
        }

        if (GUI.Toggle(new Rect(x, y, 45, 20), bSelect == true, ""))
        {
            ManagerBlock.instance.stageInfo.bossInfo.idx = idx;
            ManagerBlock.instance.stageInfo.bossInfo.attribute = attribute;
        }
    }

    private Vector2 scrollPos_WindowStart = Vector2.zero;

    void windowStart(int windowID)
    {
        eMode = EditMode.START;
        float y = 45f;
        float x = 10f;

        //스크롤바 두께 수정
        GUI.skin.verticalScrollbar.fixedWidth      = 30f;
        GUI.skin.verticalScrollbarThumb.fixedWidth = 30f;
        
        int   xPos       = 0; //현재 라인의 x 위치
        int   kind       = 0; //현재 종류
        float textWidth  = 70;
        float textHeight = 25;
        var   blockList  = blockListByStart; //종류에 따라 담은 리스트

        scrollPos_WindowStart = GUI.BeginScrollView(new Rect(x, y, 460, 85), scrollPos_WindowStart, new Rect(0, 0, 0, scrollLine * textHeight));
        
        GUI.Box(new Rect(0, 0, 440, scrollLine * textHeight), "");
        scrollLine = 0;

        x = 10;
        y = 10;
        
        GUIStyle style = new GUIStyle(GUI.skin.toggle);
        style.normal.textColor = Color.white;
        style.fontSize         = 11;

        for (int i = 0; i < blockList.Count; i++)
        {
            //종류가 다르거나 한 라인에 7개 이상이 되었을 경우 다음 라인으로 변경
            if (kind != blockList[i].kind || xPos >= 6)
            {
                y    += textHeight;
                xPos =  0;
                scrollLine++;

                if (kind != blockList[i].kind)
                {
                    kind =  blockList[i].kind;
                    y    += 5;
                    scrollLine++;
                }
            }
            
            startBlockTypes[blockList[i].startBlockTypeIdx] =  GUI.Toggle(new Rect(x + xPos * textWidth, y, textWidth, textHeight), startBlockTypes[blockList[i].startBlockTypeIdx], blockList[i].blockName,style);
            
            xPos += 1;
        }

        GUI.EndScrollView();

        x = 10;
        y = 135;
        
        //출발 블럭 컬러 설정.
        startBlockColor[0] = GUI.Toggle(new Rect(x, y, 62, 20), startBlockColor[0], " 분홍");
        x += 65;
        startBlockColor[1] = GUI.Toggle(new Rect(x, y, 62, 20), startBlockColor[1], " 갈색");
        x += 65;
        startBlockColor[2] = GUI.Toggle(new Rect(x, y, 62, 20), startBlockColor[2], " 노랑");
        x += 65;
        startBlockColor[3] = GUI.Toggle(new Rect(x, y, 62, 20), startBlockColor[3], " 주황");
        x += 65;
        startBlockColor[4] = GUI.Toggle(new Rect(x, y, 62, 20), startBlockColor[4], " 파랑");
        x += 65;

        x += 20;
        //튜터리얼 표시
        GUI.Label(new Rect(x, y, 70, 20), "튜토리얼");
        x += 55;
        int.TryParse(GUI.TextField(new Rect(x, y, 30, 20), ManagerBlock.instance.stageInfo.tutorialIndex.ToString()), out ManagerBlock.instance.stageInfo.tutorialIndex);
        
        //점수
        x = 10;
        y = 160;
        GUI.Label(new Rect(10, y, 50, 20), "점수1");
        x += 35;
        int.TryParse(GUI.TextField(new Rect(x, y, 50, 25), ManagerBlock.instance.stageInfo.score1.ToString()), out ManagerBlock.instance.stageInfo.score1);

        x += 50;
        GUI.Label(new Rect(x, y, 50, 20), "점수2");
        x += 35;
        int.TryParse(GUI.TextField(new Rect(x, y, 50, 25), ManagerBlock.instance.stageInfo.score2.ToString()), out ManagerBlock.instance.stageInfo.score2);

        x += 50;
        GUI.Label(new Rect(x, y, 50, 20), "점수3");
        x += 35;
        int.TryParse(GUI.TextField(new Rect(x, y, 50, 25), ManagerBlock.instance.stageInfo.score3.ToString()), out ManagerBlock.instance.stageInfo.score3);

        x += 50;
        GUI.Label(new Rect(x, y, 50, 20), "점수4");
        x += 35;
        int.TryParse(GUI.TextField(new Rect(x, y, 50, 25), ManagerBlock.instance.stageInfo.score4.ToString()), out ManagerBlock.instance.stageInfo.score4);

        x += 100;
        if (GUI.Button(new Rect(x, y, 50, 30), "적용"))
        {
            SetStartBlockType();
            SetStartBlockColor();
        }
        //점수
        GuiTab();

        GUI.Label(new Rect(10, 180, 50, 20), (_startPositionBlockType).ToString());
        GUI.Label(new Rect(70, 180, 50, 20), (_startPositionBlockType2).ToString());
    }

    void SetStartBlockType()
    {
        SetStartPositionBlockType(1 << (int)BlockType.NORMAL, startBlockTypes[0]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND, startBlockTypes[1]);
        SetStartPositionBlockType(1 << (int)BlockType.BOX, startBlockTypes[2]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND_JEWEL, startBlockTypes[3]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND_BOMB, startBlockTypes[4]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND_KEY, startBlockTypes[5]);
        SetStartPositionBlockType(1 << (int)BlockType.KEY, startBlockTypes[6]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND_ICE_APPLE, startBlockTypes[7]);
        SetStartPositionBlockType(1 << (int)BlockType.BLOCK_BLACK, startBlockTypes[8]);
        SetStartPositionBlockType(1 << (int)BlockType.BLOCK_DYNAMITE, startBlockTypes[9]);
        SetStartPositionBlockType(1 << (int)BlockType.ICE, startBlockTypes[10]);
        SetStartPositionBlockType(1 << (int)BlockType.ADVENTURE_POTION_HEAL, startBlockTypes[11]);
        SetStartPositionBlockType(1 << (int)BlockType.ADVENTURE_POTION_SKILL, startBlockTypes[12]);

        SetStartPositionBlockType(1 << (int)BlockType.APPLE, startBlockTypes[17]);
        SetStartPositionBlockType(1 << (int)BlockType.ICE_APPLE, startBlockTypes[18]);
        SetStartPositionBlockType(1 << (int)BlockType.GROUND_APPLE, startBlockTypes[22]);

        //출발정보2에서 사용 블럭 인덱스번호로 위치부여함
        //SetStartPositionBlockType2(1 << ((int)BlockType.START_Line_V - 32), startBlockTypes[13]); // 미사용
        //SetStartPositionBlockType2(1 << ((int)BlockType.START_Line_H - 32), startBlockTypes[14]); // 미사용
        SetStartPositionBlockType2(1 << ((int)BlockType.START_Bomb - 32), startBlockTypes[15]);
        SetStartPositionBlockType2(1 << ((int)BlockType.START_Rainbow - 32), startBlockTypes[16]);
        SetStartPositionBlockType2(1 << ((int)BlockType.FIRE_WORK - 32), startBlockTypes[19]);
        SetStartPositionBlockType2(1 << ((int)BlockType.COIN_BAG - 32), startBlockTypes[20]);
        SetStartPositionBlockType2(1 << ((int)BlockType.PEA - 32), startBlockTypes[21]);
        SetStartPositionBlockType2(1 << ((int)BlockType.SPACESHIP - 32), startBlockTypes[23]);
        SetStartPositionBlockType2(1 << ((int)BlockType.PEA_BOSS - 32), startBlockTypes[24]);
        SetStartPositionBlockType2(1 << ((int)BlockType.START_Line - 32), startBlockTypes[25]);
        SetStartPositionBlockType2(1 << ((int)BlockType.GROUND_BLOCKBLACK - 32), startBlockTypes[26]);
        SetStartPositionBlockType2(1 << ((int)BlockType.PAINT - 32), startBlockTypes[27]);
        SetStartPositionBlockType2(1 << ((int)BlockType.WATERBOMB - 32), startBlockTypes[28]);
    }

    void SetStartPositionBlockType(int type, bool in_set)
    {
        if (in_set)
            _startPositionBlockType |= type;
        else
            _startPositionBlockType &= ~type;
    }

    void SetStartPositionBlockType2(int type, bool in_set)
    {
        if (in_set)
            _startPositionBlockType2 |= type;
        else
            _startPositionBlockType2 &= ~type;
    }

    public int[] GetArrStartBlockType_AtDicToggleData_BlockGenerator()
    {
        int[] arrStartBlockType = new int[2] { int.MaxValue, int.MaxValue };

        foreach (var temp in dicToggleData_BlockGenerator)
        {
            int checkType = (int)temp.Key;

            if (checkType < 32)
            {
                if (temp.Value.isCanMakeBlock == true)
                    arrStartBlockType[0] |= (1 << checkType);
                else
                    arrStartBlockType[0] &= ~(1 << checkType);
            }
            else
            {
                if (temp.Value.isCanMakeBlock == true)
                    arrStartBlockType[1] |= (1 << checkType);
                else
                    arrStartBlockType[1] &= ~(1 << checkType);
            }
        }
        return arrStartBlockType;
    }

    /// <summary>
    /// 해당 인덱스의 생성기에서 나올 수 있는 블럭 컬러를 반환해주는 함수
    /// </summary>
    public (int, List<BlockAndColorData>) GetListColorData_AtGeneratorBlockColor(int idx)
    {
        int normalColorIdx = int.MaxValue;
        List<BlockAndColorData> listTemp = new List<BlockAndColorData>();
        if (dicGeneratorBlockInfo != null && dicGeneratorBlockInfo.ContainsKey(idx) == true)
        {
            foreach (var item in dicGeneratorBlockInfo[idx].dicCanMakeColor)
            {
                if (item.Key == BlockType.NORMAL)
                {
                    normalColorIdx = item.Value;
                }
                else
                {
                    BlockAndColorData tempData = new BlockAndColorData()
                    {
                        blockType = (int)item.Key,
                        subType = (item.Value == 0) ? int.MaxValue : item.Value
                    };
                    listTemp.Add(tempData);
                }
            }
        }
        return (normalColorIdx, listTemp);
    }

    void SetStartBlockColor()
    {
        _startPositionBlockColor = 0;
        SetStartPositionBlockColor(1 << (int)BlockColorType.A, startBlockColor[0]);
        SetStartPositionBlockColor(1 << (int)BlockColorType.B, startBlockColor[1]);
        SetStartPositionBlockColor(1 << (int)BlockColorType.C, startBlockColor[2]);
        SetStartPositionBlockColor(1 << (int)BlockColorType.D, startBlockColor[3]);
        SetStartPositionBlockColor(1 << (int)BlockColorType.E, startBlockColor[4]);

        if (_startPositionBlockColor == 0)
            _startPositionBlockColor = int.MaxValue;
    }

    void SetStartPositionBlockColor(int type, bool in_set)
    {
        if (in_set)
            _startPositionBlockColor += type;
    }

    public int blockCount = 1;
    public int blockIndex = 0;
    public int blockType = 0;
    public bool isBool = false;

    //흙 확장형 단계
    public int groundCount = 0;

    //하트 길 갯수
    public int wayCount = 0;

    void SetCount(int x1 = 10, int x2 = 50, int y1 = 170, int y2 = 20, string text = "카운트")
    {
        GUI.Label(new Rect(x1, y1, x2, y2), text);
        int.TryParse(GUI.TextField(new Rect(x1 + x2, y1, 25, 25), blockCount.ToString()), out blockCount);
    }

    void SetText(int x1 = 10, int x2 = 50, int y1 = 170, int y2 = 20, string text = "")
    {
        GUI.Label(new Rect(x1, y1, x2, y2), text);
    }

    void SetIndex(int x1 = 10, int x2 = 50, int y1 = 170, int y2 = 20, string text = "인덱스")
    {
        GUI.Label(new Rect(x1, y1, x2, y2), text);
        int.TryParse(GUI.TextField(new Rect(x1 + x2, y1, 25, 25), blockIndex.ToString()), out blockIndex);
    }

    void SetButton(int x1 = 10, int x2 = 50, int y1 = 170, int y2 = 25, string text = "설정", System.Action buttonEvent = null)
    {
        if (GUI.Button(new Rect(x1, y1, x2, y2), text))
            buttonEvent.Invoke();
    }
    
    //버튼으로 인덱스 조정
    void SetIndexByButton(int minIndex, int maxIndex)
    {
        GUI.Label(new Rect(10, 170, 50, 20), "인덱스");
        if (GUI.Button(new Rect(60, 170, 25, 25), blockIndex.ToString()))
        {
            blockIndex++;
        }

        if (blockIndex < minIndex || blockIndex > maxIndex)
            blockIndex = minIndex;
    }

    //버튼으로 카운트 조정
    void SetCountByButton(int minCount, int maxCount, string text = "카운트", int x = 10, int y = 170)
    {
        GUI.Label(new Rect(x, y, 50, 20), text);
        if (GUI.Button(new Rect(x + 50, y, 25, 25), blockCount.ToString()))
        {
            blockCount++;
        }

        if (blockCount < minCount || blockCount > maxCount)
            blockCount = minCount;
    }
    
    /// <param name="indexType"> 접근할 인덱스 변수 참조를 설정합니다. ex) ref blockIndex, ref blockType </param>
    /// <param name="maxValue"> 참조할 리스트의 max 인덱스 + 1 값을 넣어야합니다. +버튼 클릭 시 마지막 인덱스 + 1로 설정합니다. 미 설정 시에는 기본으로 작동됩니다. </param>
    private void SetIndexByTwoButton(ref int indexType, int x = 10, int y = 145, int textWidth = 50, string text = "인덱스", bool horizontal = true, int maxValue = -1)
    {
        GUI.Label(new Rect(x, y, textWidth, 20), text);

        if (horizontal)
        {
            x += textWidth;
        }
        else
        {
            y += 20;
        }

        if (GUI.Button(new Rect(x, y, 50, 25), indexType.ToString()))
        {
            //maxValue가 없다면 0만 설정합니다. 
            if (maxValue == -1)
            {
                indexType = 0;
            }
            //maxValue가 있다면 maxValue-1까지 순차로 +됩니다.
            else
            {
                if (indexType < maxValue - 1)
                {
                    indexType++;
                }
                else
                {
                    indexType = 0;
                }
            }
        }

        x += 50;

        //maxValue가 없으면 계속 +됩니다.
        if (maxValue == -1)
        {
            if (GUI.Button(new Rect(x, y, 30, 25), "+"))
            {
                indexType++;
            }
        }
        //maxVlue가 있으면 maxValue로 고정됩니다.
        else if(maxValue > 0)
        {
            if (GUI.Button(new Rect(x, y, 30, 25), "+"))
            {
                indexType = maxValue;
            }
        }
    }

    public void SetIndexByTwoButtonAndInputField(ref int index, int maxIndex, int x, int y)
    {
        if(GUI.Button(new Rect(x, y, 40, 25), " - "))
        {
            index = (index-1) % maxIndex;
            if (index == 0)
                index = maxIndex;
        }
        x += 50;

        int.TryParse(GUI.TextField(new Rect(x, y, 40, 25), index.ToString()), out index);
        x += 50;

        if (GUI.Button(new Rect(x, y, 40, 25), " + "))
        {
            index = (index + 1) % maxIndex;
            if (index == 0)
                index = maxIndex;
        }
    }

    public int dynamiteBombType = 0;
    void SetDynamiteLife()
    {
        GUI.Label(new Rect(115, 170, 26, 20), "시간");
        int.TryParse(GUI.TextField(new Rect(145, 170, 25, 25), blockIndex.ToString()), out blockIndex);
    }

    /*
    void SetStatue()
    {
        GUI.Label(new Rect(90, 170, 30, 20), "방향");
        int.TryParse(GUI.TextField(new Rect(120, 170, 25, 25), blockIndex.ToString()), out blockIndex);
    }
    */
    void SetType(int addX = 0)
    {
        GUI.Label(new Rect(150 + addX, 170, 30, 20), "단계");
        int.TryParse(GUI.TextField(new Rect(180 + addX, 170, 25, 25), blockType.ToString()), out blockType);
    }

    [System.NonSerialized]
    public int CoinCount = 1;
    [System.NonSerialized]
    public int[] coinBagCount = new int[8] { 3, 5, 7, 10, 15, 20, 25, 0 };
    private int coinBagIndex = 0;
    void SetCoinBag(int addX = 0)
    {
        GUI.Label(new Rect(10, 170, 50, 20), "코인");
        SetCoinBagIndex();
        CoinCount = coinBagCount[coinBagIndex];
    }

    void SetCoinBagIndex()
    {
        int indexCount = 8;
        for (int i = 0; i < indexCount; i++)
        {
            //마지막 칸은 직접 코인을 입력할 수 있는 칸
            if (i == indexCount - 1)
            {
                int.TryParse(GUI.TextField(new Rect(60 + (40 * i), 170, 30, 25), coinBagCount[i].ToString()), out coinBagCount[i]);
            }
            else
            {
                GUI.Label(new Rect(60 + (40 * i), 170, 30, 25), coinBagCount[i].ToString());
            }

            if (GUI.Toggle(new Rect(40 + (40 * i), 170, 30, 50), coinBagIndex == i, ""))
            {
                coinBagIndex = i;
            }
        }
    }

    void Set_BOMBPLANT_Type(int addX = 0)
    {
        if (addX == 0)
        {
            GUI.Label(new Rect(10, 170, 170, 20), "0랜덤라인1둥근2세로3가로");
            int.TryParse(GUI.TextField(new Rect(180, 170, 25, 25), blockType.ToString()), out blockType);
        }
        else
        {
            GUI.Label(new Rect(10 + addX, 170, 30, 20), "폭탄");
            int.TryParse(GUI.TextField(new Rect(40 + addX, 170, 25, 25), blockType.ToString()), out blockType);
        }
    }

    void Set_EventBlock()
    {
        GUI.Label(new Rect(130, 170, 50, 20), "인덱스");
        int.TryParse(GUI.TextField(new Rect(180, 170, 25, 25), ManagerBlock.instance.stageInfo.collectEventType.ToString()), out ManagerBlock.instance.stageInfo.collectEventType);

        /*
          ManagerBlock.instance.stageInfo.collectEventType 
         
        GUI.Label(new Rect(10, 170, 170, 20), "0기본1식물");
        int.TryParse(GUI.TextField(new Rect(180, 170, 25, 25), blockType.ToString()), out blockType);
        */
        //라이프카운트, 이벤트인덱스, 타입
    }


    public bool candyType = false;

    void SetCandy(int addX = 0)
    {
        candyType = GUI.Toggle(new Rect(330 + addX, 170, 50, 20), candyType, "사탕");
        if (candyType)  
            duckType = !candyType;    }

    public bool duckType = false;
    void SetDuck(int addX = 0)
    {
        duckType = GUI.Toggle(new Rect(330 + addX, 170, 50, 20), duckType, "오리");
        if (duckType)
            candyType = !duckType;    }

    public int boxColorType = 0;
    void SetBoxColor()
    {
        GUI.Label(new Rect(330, 170, 30, 20), "컬러");
        int.TryParse(GUI.TextField(new Rect(360, 170, 25, 25), boxColorType.ToString()), out boxColorType);
    }

    public int plantType = 0;
    void SetPlantType()
    {
        /*
        if (GUI.Toggle(new Rect(5, 170, 45, 20), plantType == 0, "물"))
        {
            stoneType = 0;
        }
        if (GUI.Toggle(new Rect(50, 170, 45, 20), stoneType == 1, "사과"))
        {
            stoneType = 1;
        }
        if (GUI.Toggle(new Rect(95, 170, 45, 20), stoneType == 2, "폭탄"))
        {
            stoneType = 2;
        }
        if (GUI.Toggle(new Rect(140, 170, 45, 20), stoneType == 3, "열쇠"))
        {
            stoneType = 3;
        }
        */

        if (GUI.Toggle(new Rect(240, 170, 45, 20), plantType == (int)PLANT_TYPE.SPACESHIP, "거미"))
        {
            plantType = (int)PLANT_TYPE.SPACESHIP;
        }

        if (GUI.Toggle(new Rect(195, 170, 45, 20), plantType == (int)PLANT_TYPE.SkillPotion, "스킬포션"))
        {
            plantType = (int)PLANT_TYPE.SkillPotion;
        }

        if (GUI.Toggle(new Rect(140, 170, 45, 20), plantType == (int)PLANT_TYPE.HealPotion, "힐포션"))
        {
            plantType = (int)PLANT_TYPE.HealPotion;
        }

        if (GUI.Toggle(new Rect(95, 170, 45, 20), plantType == (int)PLANT_TYPE.COIN, "코인"))
        {
            plantType = (int)PLANT_TYPE.COIN;
        }

        if (GUI.Toggle(new Rect(50, 170, 30, 20), plantType == (int)PLANT_TYPE.WATER, "물"))
        {
            plantType = (int)PLANT_TYPE.WATER;
        }

        if (GUI.Toggle(new Rect(5, 170, 45, 20), plantType == (int)PLANT_TYPE.NORMAL, "일반"))
        {
            plantType = (int)PLANT_TYPE.NORMAL;
        }
    }

    void SetPlant2X2Type()
    {
        if (GUI.Toggle(new Rect(5, 145, 45, 20), plantType == (int)PLANT_TYPE.NORMAL, "일반"))
        {
            plantType = (int)PLANT_TYPE.NORMAL;
        }

        if (GUI.Toggle(new Rect(50, 145, 30, 20), plantType == (int)PLANT_TYPE.WATER, "물"))
        {
            plantType = (int)PLANT_TYPE.WATER;
        }
    }

    //public bool CheckPlantSplash = false;

    public int[] blockSubTarget = new int[] { 0, 0, 0, 0, 0 };
    public bool[] blockSubTargetBool = new bool[] { false, false, false, false, false };

    void SetSubTarget()
    {
        GUI.Label(new Rect(220, 170, 70, 20), "컬러목표");
        for (int i = 0; i < 5; i++)
        {
            int.TryParse(GUI.TextField(new Rect(250 + 25 * (i + 1), 170, 25, 25), blockSubTarget[i].ToString()), out blockSubTarget[i]);
            blockSubTargetBool[i] = GUI.Toggle(new Rect(250 + 27 * (i + 1), 145, 25, 25), blockSubTargetBool[i], "");
        }

        //CheckPlantSplash = (GUI.Toggle(new Rect(250, 150, 100, 20), CheckPlantSplash , "중복스플래쉬"));
    }
    private void SetSubTargetByButton(int minColor, int maxColor, int x = 220, int y = 165)
    {
        GUI.Label(new Rect(x, y, 70, 20), "컬러목표");
        for (int i = 0; i < 5; i++)
        {
            if (GUI.Button(new Rect(x + 30 + 32 * (i + 1), y, 30, 25), blockSubTarget[i].ToString()))
            {
                int index = blockSubTarget[i]++ <= maxColor ? blockSubTarget[i]++ : minColor;
                blockSubTarget[i] = index;
            }
            blockSubTargetBool[i] = GUI.Toggle(new Rect(x + 38 + 32 * (i + 1), y - 20, 30, 25), blockSubTargetBool[i], "");
        }
    }

    public enum AppleCount
    {
        One = 1,
        Three = 3,
        Five = 5
    }
    [System.NonSerialized]
    public AppleCount appleCount = AppleCount.One;

    private void SetApple(int x1 = 10, int x2 = 70, int y1 = 170, int y2 = 20, int x3 = 20, int offsetX = 50, int toggleSize = 50, string text = "카운트")
    {
        GUI.Label(new Rect(x1, y1, x2, y2), text);

        x3 += x1 + x2;
        if (GUI.Toggle(new Rect(x3, y1, toggleSize, toggleSize), appleCount == AppleCount.One, "1"))
            appleCount = AppleCount.One;
        if (GUI.Toggle(new Rect(x3 + offsetX, y1, toggleSize, toggleSize), appleCount == AppleCount.Three, "3"))
            appleCount = AppleCount.Three;
        if (GUI.Toggle(new Rect(x3 + (offsetX * 2), y1, toggleSize, toggleSize), appleCount == AppleCount.Five, "5"))
            appleCount = AppleCount.Five;
    }

    private void SetCountCrackInfo()
    {
        int gimmickIndex = ManagerBlock.instance.stageInfo.listDecoInfo.FindIndex(x => x.gimmickType == (int)BoardDecoType.COUNT_CRACK);
        int crackIndex = EditManager.instance.blockIndex;
        if (gimmickIndex == -1)
        {
            CountCrack_AppleInfo appleInfo = new CountCrack_AppleInfo(crackIndex, (int)EditManager.instance.appleCount, EditManager.instance.blockCount);
            GimmickInfo_CountCrack countCrackInfo = new GimmickInfo_CountCrack();
            countCrackInfo.gimmickType = (int)BoardDecoType.COUNT_CRACK;
            countCrackInfo.listAppleInfo.Add(appleInfo);
            ManagerBlock.instance.stageInfo.listDecoInfo.Add(countCrackInfo);
        }
        else
        {
            if (ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] is GimmickInfo_CountCrack == false)
                return;
            GimmickInfo_CountCrack countCrackInfo = ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] as GimmickInfo_CountCrack;
            int infoIndex = countCrackInfo.listAppleInfo.FindIndex(x => x.crackIndex == crackIndex);
            if (infoIndex == -1)
            {
                CountCrack_AppleInfo appleInfo = new CountCrack_AppleInfo(crackIndex, (int)EditManager.instance.appleCount, EditManager.instance.blockCount);
                countCrackInfo.listAppleInfo.Add(appleInfo);
            }
            else
            {
                countCrackInfo.listAppleInfo[infoIndex].index = (int)EditManager.instance.appleCount;
                countCrackInfo.listAppleInfo[infoIndex].count = EditManager.instance.blockCount;
            }
        }
    }

    private string GetCountCrackInfo()
    {
        int crackIndex = EditManager.instance.blockIndex;
        int gimmickIndex = ManagerBlock.instance.stageInfo.listDecoInfo.FindIndex(x => x.gimmickType == (int)BoardDecoType.COUNT_CRACK);

        if (gimmickIndex > -1 && ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] is GimmickInfo_CountCrack == true)
        {
            GimmickInfo_CountCrack countCrackInfo = ManagerBlock.instance.stageInfo.listDecoInfo[gimmickIndex] as GimmickInfo_CountCrack;
            int infoIndex = countCrackInfo.listAppleInfo.FindIndex(x => x.crackIndex == crackIndex);
            if (infoIndex > -1)
            {
                return string.Format("사과 : {0}, 카운트: {1}", countCrackInfo.listAppleInfo[infoIndex].index, countCrackInfo.listAppleInfo[infoIndex].count);
            }
        }
        return string.Format("사과 : {0}, 카운트: {1}", 0, 0);
    }

    public enum FenceCount
    {
        One = 1,
        Two = 2
    }
    [System.NonSerialized]
    public FenceCount fenceCount = FenceCount.One;

    private void SetFence()
    {
        GUI.Label(new Rect(10, 170, 70, 20), "카운트");

        if (GUI.Toggle(new Rect(100, 170, 50, 50), fenceCount == FenceCount.One, "1"))
            fenceCount = FenceCount.One;
        if (GUI.Toggle(new Rect(150, 170, 50, 50), fenceCount == FenceCount.Two, "2"))
            fenceCount = FenceCount.Two;
    }

    /*
    public bool jewel = false;
    void SetJewel()
    {
        jewel = GUI.Toggle(new Rect(360, 170, 50, 25), jewel, "보석");
    }
    */
    public int startProbablilty = 0;
    public int max_displayCount = 0;
    public int max_stageCount = 0;
    public int default_count = 0;
    public int min_turn_count = 0;
    public int generator_index = 0;

    private class EditData_StartProbData
    {
        public int[] count_Prob = new int[3] { 10, 0, 0 };
        public int[] time_Prob = new int[3] { 10, 10, 10 };
        public int[] ice_Prob = new int[3] { 0, 0, 0 };
    }
    private Dictionary<BlockType, EditData_StartProbData> dicEditData_StartProb
        = new Dictionary<BlockType, EditData_StartProbData>();

    void ShowStartInfo()
    {
        if (GUI.Toggle(new Rect(10, 150, 180, 20), this.bShowStartInfo, "출발 정보"))
        {
            this.bShowStartInfo = true;
        }
        else
        {
            this.bShowStartInfo = false;
            this.decoInfo = null;
        }

        if (this.bShowStartInfo == true && decoInfo != null)
            SetStartInfo();
    }

    void SetStart(BlockType tempType, int countCnt = -1, int timeCnt = -1, int xX = 0, int yY = 145)
    {
        bool hasStartInfo = false;

        //출발에 있나 찾기 
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            if (startInfo.type == (int)tempType)
            {
                GUI.Label(new Rect(xX + 1, yY, 26, 20), "확률");
                int.TryParse(GUI.TextField(new Rect(xX + 27, yY, 33, 25), startInfo.probability.ToString()), out startInfo.probability);

                GUI.Label(new Rect(xX + 60, yY, 40, 20), "화면수");
                int.TryParse(GUI.TextField(new Rect(xX + 100, yY, 25, 25), startInfo.max_display_Count.ToString()), out startInfo.max_display_Count);

                GUI.Label(new Rect(xX + 125, yY, 26, 20), "전체");
                int.TryParse(GUI.TextField(new Rect(xX + 151, yY, 28, 25), startInfo.max_stage_Count.ToString()), out startInfo.max_stage_Count);

                if (GUI.Button(new Rect(xX + 179, yY, 35, 25), "제거"))
                {
                    ManagerBlock.instance.stageInfo.ListStartInfo.Remove(startInfo);
                    EditBlockGuide.reflesh = true;
                    return;
                }
                hasStartInfo = true;

                GUI.Label(new Rect(xX + 214, yY, 26, 20), "턴간");
                int.TryParse(GUI.TextField(new Rect(xX + 240, yY, 23, 25), startInfo.minTurn.ToString()), out startInfo.minTurn);

                if (tempType != BlockType.START_Bomb && tempType != BlockType.START_Rainbow)
                {
                    if (countCnt == -1)
                        countCnt = startInfo.countProb.Length;
                    for (int i = 0; i < 3; i++)
                    {
                        if (countCnt > i)
                            int.TryParse(GUI.TextField(new Rect(xX + 270 + 25 * i, yY, 25, 25), startInfo.countProb[i].ToString()), out startInfo.countProb[i]);
                        else
                            startInfo.countProb[i] = 0;
                    }
                }
                
                if (tempType == BlockType.PAINT || tempType == BlockType.BLOCK_DYNAMITE || tempType == BlockType.GROUND_BOMB || tempType == BlockType.COIN_BAG || tempType == BlockType.GROUND_APPLE || tempType == BlockType.WATERBOMB)
                {
                    if (timeCnt == -1)
                        timeCnt = startInfo.timeCount.Length;
                    for (int i = 0; i < startInfo.timeCount.Length; i++)
                    {
                        if (timeCnt > i)
                            int.TryParse(GUI.TextField(new Rect(xX + 355 + 25 * i, yY, 25, 25), startInfo.timeCount[i].ToString()), out startInfo.timeCount[i]);
                        else
                            startInfo.timeCount[i] = 0;
                    }
                }
                
                if (tempType != BlockType.GROUND_BLOCKBLACK)
                {
                    if (startInfo.type == (int)BlockType.ICE)
                    {
                        GUI.Label(new Rect(xX + 5, yY + 26, 26, 20), "컬러");
                        for (int i = 0; i < startInfo.iceProb.Length; i++)
                            int.TryParse(GUI.TextField(new Rect(xX + 25 + 25 * i, yY + 26, 25, 25), startInfo.iceProb[i].ToString()), out startInfo.iceProb[i]);
                    }
                    else
                    {
                        //얼음 확률
                        for (int i = 0; i < startInfo.iceProb.Length; i++)
                            int.TryParse(GUI.TextField(new Rect(xX + 355 + 25 * i, yY + 26, 25, 25), startInfo.iceProb[i].ToString()), out startInfo.iceProb[i]);
                    }
                }
            }
        }

        //없으면 생성
        if (!hasStartInfo)
        {
            GUI.Label(new Rect(xX + 1, yY, 26, 20), "확률");
            int.TryParse(GUI.TextField(new Rect(xX + 27, yY, 33, 25), startProbablilty.ToString()), out startProbablilty);

            GUI.Label(new Rect(xX + 60, yY, 40, 20), "화면수");
            int.TryParse(GUI.TextField(new Rect(xX + 100, yY, 25, 25), max_displayCount.ToString()), out max_displayCount);

            GUI.Label(new Rect(xX + 125, yY, 26, 20), "전체");
            int.TryParse(GUI.TextField(new Rect(xX + 151, yY, 28, 25), max_stageCount.ToString()), out max_stageCount);

            if (GUI.Button(new Rect(xX + 179, yY, 35, 25), "설정"))
            {
                StartBlockInfo tempInfo = new StartBlockInfo();
                InitStartProbData(tempType);

                tempInfo.type = (int)tempType;
                tempInfo.probability = startProbablilty;
                tempInfo.max_display_Count = max_displayCount;
                tempInfo.max_stage_Count = max_stageCount;
                tempInfo.minTurn = min_turn_count;
                tempInfo.countProb = (int[])GetStartProbData(tempType, 0).Clone();
                tempInfo.timeCount = (int[])GetStartProbData(tempType, 1).Clone();
                tempInfo.iceProb = (int[])GetStartProbData(tempType, 2).Clone();
                ManagerBlock.instance.stageInfo.ListStartInfo.Add(tempInfo);
                EditBlockGuide.reflesh = true;
            }

            GUI.Label(new Rect(xX + 214, yY, 26, 20), "턴간격");
            int.TryParse(GUI.TextField(new Rect(xX + 240, yY, 23, 25), min_turn_count.ToString()), out min_turn_count);

            if (tempType != BlockType.START_Bomb && tempType != BlockType.START_Rainbow)
            {
                int[] countProb = GetStartProbData(tempType, 0);
                if (countCnt == -1)
                    countCnt = countProb.Length;
                for (int i = 0; i < countProb.Length; i++)
                {   
                    if (countCnt > i)
                        int.TryParse(GUI.TextField(new Rect(xX + 270 + 25 * i, yY, 25, 25), countProb[i].ToString()), out countProb[i]);
                    else
                        countProb[i] = 0;
                }
                SetStartProbData(tempType, 0, countProb);
            }

            if (tempType == BlockType.BLOCK_DYNAMITE || tempType == BlockType.GROUND_BOMB || tempType == BlockType.COIN_BAG || tempType == BlockType.GROUND_APPLE || tempType == BlockType.PAINT || tempType == BlockType.WATERBOMB)
            {
                int[] timeProb = GetStartProbData(tempType, 1);
                if (timeCnt == -1)
                    timeCnt = timeProb.Length;
                for (int i = 0; i < timeProb.Length; i++)
                {
                    if (timeCnt > i)
                        int.TryParse(GUI.TextField(new Rect(xX + 355 + 25 * i, yY, 25, 25), timeProb[i].ToString()), out timeProb[i]);
                    else
                        timeProb[i] = 0;
                }
                SetStartProbData(tempType, 1, timeProb);
            }

            if (tempType == BlockType.ICE)
            {
                GUI.Label(new Rect(xX + 5, yY + 26, 26, 20), "컬러");
                int[] iceProb = GetStartProbData(tempType, 2);
                for (int i = 0; i < iceProb.Length; i++)
                    int.TryParse(GUI.TextField(new Rect(xX + 25 + 25 * i, yY + 26, 25, 25), iceProb[i].ToString()), out iceProb[i]);
                SetStartProbData(tempType, 2, iceProb);
            }
            else if (tempType != BlockType.GROUND && tempType != BlockType.GROUND_APPLE && tempType != BlockType.GROUND_BLOCKBLACK && tempType != BlockType.GROUND_BOMB && tempType != BlockType.GROUND_ICE_APPLE && tempType != BlockType.GROUND_KEY && tempType != BlockType.BLOCK_EVENT_GROUND)
            {
                int[] iceProb = GetStartProbData(tempType, 2);
                for (int i = 0; i < iceProb.Length; i++) 
                    int.TryParse(GUI.TextField(new Rect(xX + 355 + 25 * i, yY + 26, 25, 25), iceProb[i].ToString()), out iceProb[i]);
                SetStartProbData(tempType, 2, iceProb);
            }
        }
    }

    private int GetCountCnt(BlockType tempType)
    {
        if (tempType == BlockType.START_Bomb || tempType == BlockType.START_Rainbow)
            return 0;

        else if (tempType == BlockType.PEA || tempType == BlockType.START_Line)
            return 2;
        else
            return 3;
    }

    bool SetStartSubType_BlockGenerator(BlockType tempType, Vector2Int originPos)
    {
        bool isChangeToggleData = false;
        Vector2Int tempPos = originPos;

        if(tempType == BlockType.GROUND_BOMB)
        {
            GUI.Label(new Rect(tempPos.x, tempPos.y, 110, 20), "0라인1둥근2혼합");
            if (GUI.Button(new Rect(tempPos.x + 110, tempPos.y, 33, 25), dicToggleData_BlockGenerator[tempType].subType.ToString()))
            {
                if(dicToggleData_BlockGenerator[tempType].subType >= 2)
                    dicToggleData_BlockGenerator[tempType].subType = 0;
                else
                    dicToggleData_BlockGenerator[tempType].subType++;

                isChangeToggleData = true;
            }
        }



        return isChangeToggleData;
    }

    bool SetStartColorType_BlockGenerator(BlockType tempType, Vector2Int originPos)
    {
        bool isChangeToggleData = false;
        Vector2Int tempPos = originPos;

        List<bool> showColorList = new List<bool>();
        showColorList.Add(dicToggleData_BlockGenerator[tempType].listShowColorType.FindIndex((x => x == BlockColorType.A)) > -1);
        showColorList.Add(dicToggleData_BlockGenerator[tempType].listShowColorType.FindIndex((x => x == BlockColorType.B)) > -1);
        showColorList.Add(dicToggleData_BlockGenerator[tempType].listShowColorType.FindIndex((x => x == BlockColorType.C)) > -1);
        showColorList.Add(dicToggleData_BlockGenerator[tempType].listShowColorType.FindIndex((x => x == BlockColorType.D)) > -1);
        showColorList.Add(dicToggleData_BlockGenerator[tempType].listShowColorType.FindIndex((x => x == BlockColorType.E)) > -1);

        for (int i = 0; i < showColorList.Count; i++)
        {
            BlockColorType tempColorType = (BlockColorType)i + 1;
            bool isShowColor = GUI.Toggle(new Rect(tempPos.x, tempPos.y, 40, 25), showColorList[i], "<b>" + GetColorTypeName(tempColorType) + "</b>");

            if (isShowColor != showColorList[i])
            {
                if (isShowColor == true)
                    dicToggleData_BlockGenerator[tempType].listShowColorType.Add(tempColorType);
                else
                    dicToggleData_BlockGenerator[tempType].listShowColorType.Remove(tempColorType);
                isChangeToggleData = true;
            }
            tempPos.x += 40;
        }

        return isChangeToggleData;
    }

    void SetStart_BlockGenerator(BlockType tempType, Vector2Int originPos)
    {
        Vector2Int tempPos = originPos;

        int findIndex = ManagerBlock.instance.stageInfo.ListStartInfo.FindIndex(x => x.type == (int)tempType);
        if (findIndex == -1)
        {
            StartBlockInfo tempInfo = new StartBlockInfo();
            tempInfo.type = (int)tempType;
            tempInfo.probability = startProbablilty;
            tempInfo.max_display_Count = max_displayCount;
            tempInfo.max_stage_Count = max_stageCount;
            tempInfo.minTurn = min_turn_count;
            tempInfo.countProb = (int[])GetStartProbData(tempType, 0).Clone();
            tempInfo.timeCount = (int[])GetStartProbData(tempType, 1).Clone();
            tempInfo.iceProb = (int[])GetStartProbData(tempType, 2).Clone();
            ManagerBlock.instance.stageInfo.ListStartInfo.Add(tempInfo);
            findIndex = 0;
        }

        StartBlockInfo startInfo = ManagerBlock.instance.stageInfo.ListStartInfo[findIndex];
        if (startInfo.type == (int)tempType)
        {
            GUI.Label(new Rect(tempPos.x, tempPos.y, 26, 20), "확률");
            int.TryParse(GUI.TextField(new Rect(tempPos.x + 26, tempPos.y, 33, 25), startInfo.probability.ToString()), out startInfo.probability);
            tempPos.x += 70;

            GUI.Label(new Rect(tempPos.x, tempPos.y, 40, 20), "화면수");
            int.TryParse(GUI.TextField(new Rect(tempPos.x + 40, tempPos.y, 25, 25), startInfo.max_display_Count.ToString()), out startInfo.max_display_Count);
            tempPos.x += 70;

            GUI.Label(new Rect(tempPos.x, tempPos.y, 26, 20), "전체");
            int.TryParse(GUI.TextField(new Rect(tempPos.x + 26, tempPos.y, 28, 25), startInfo.max_stage_Count.ToString()), out startInfo.max_stage_Count);
            tempPos.x += 70;

            GUI.Label(new Rect(tempPos.x, tempPos.y, 26, 20), "턴간");
            int.TryParse(GUI.TextField(new Rect(tempPos.x + 26, tempPos.y, 23, 25), startInfo.minTurn.ToString()), out startInfo.minTurn);
            tempPos.x += 70;

            for (int i = 0; i < 3; i++)
            {
                if (GetCountCnt(tempType) > i)
                {
                    int.TryParse(GUI.TextField(new Rect(tempPos.x, tempPos.y, 25, 25), startInfo.countProb[i].ToString()), out startInfo.countProb[i]);
                    tempPos.x += 25;
                }
                else
                {
                    startInfo.countProb[i] = 0;
                }
            }
            tempPos.x += 10;

            //다음 줄.
            tempPos.x = originPos.x;
            tempPos.y += 35;

            if (tempType == BlockType.BLOCK_DYNAMITE || tempType == BlockType.GROUND_BOMB || tempType == BlockType.COIN_BAG || tempType == BlockType.GROUND_APPLE || tempType == BlockType.PAINT || tempType == BlockType.WATERBOMB)
            {
                for (int i = 0; i < startInfo.timeCount.Length; i++)
                {
                    int.TryParse(GUI.TextField(new Rect(tempPos.x, tempPos.y, 25, 25), startInfo.timeCount[i].ToString()), out startInfo.timeCount[i]);
                    tempPos.x += 25;
                }
                tempPos.x += 10;
            }
            
            if (tempType != BlockType.GROUND_BLOCKBLACK)
            {
                if (startInfo.type == (int)BlockType.ICE)
                {
                    GUI.Label(new Rect(tempPos.x, tempPos.y, 26, 20), "컬러");
                    tempPos.x += 30;

                    for (int i = 0; i < startInfo.iceProb.Length; i++)
                    {
                        int.TryParse(GUI.TextField(new Rect(tempPos.x, tempPos.y, 25, 25), startInfo.iceProb[i].ToString()), out startInfo.iceProb[i]);
                        tempPos.x += 25;
                    }
                }
                else
                {
                    GUI.Label(new Rect(tempPos.x, tempPos.y, 26, 20), "얼음");
                    tempPos.x += 25;

                    for (int i = 0; i < 3; i++)
                    {
                        //얼음 확률
                        int.TryParse(GUI.TextField(new Rect(tempPos.x, tempPos.y, 25, 25), startInfo.iceProb[i].ToString()), out startInfo.iceProb[i]);
                        tempPos.x += 25;
                    }
                }
            }
        }

        //블럭 타입별 설정
        switch (tempType)
        {
            case BlockType.BLOCK_BLACK:
                {
                    tempPos.x += 10;
                    SetBlockAdjust(tempType, tempPos.x, tempPos.y);
                }
                break;
            case BlockType.ICE:
                {
                    tempPos.x += 10;
                    SetIceAdjust(tempPos.x, tempPos.y);
                }
                break;
            case BlockType.PAINT:
                {
                    tempPos.x += 10;
                    SetStart_StartColor(SelectGeneratorIdx, BlockType.PAINT, tempPos);
                }
                break;
        }
    }

    // 기존에 맵에 깔려있는 기믹을 화면에 출력될 수 있는 기믹수에 합산할지 말지 설정하는 함수.
    void SetBlockAdjust(BlockType tempBlockType, int posX = 10, int posY = 170)
    {
        if(tempBlockType == BlockType.BLOCK_BLACK)
        {
            if (GUI.Toggle(new Rect(posX, posY, 180, 20), ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 1, "화면수에 미포함"))
                ManagerBlock.instance.stageInfo.isBlockBlackAdjust = 1;
            else
                ManagerBlock.instance.stageInfo.isBlockBlackAdjust = 0;
        }
        else
        {
            int listIndex = ManagerBlock.instance.stageInfo.listBlockAdjust.FindIndex(x => x == (int)tempBlockType);

            if(GUI.Toggle(new Rect(posX, posY, 180, 20), listIndex != -1, "화면수에 미포함"))
            {
                if(listIndex == -1)
                    ManagerBlock.instance.stageInfo.listBlockAdjust.Add((int)tempBlockType);
            }
            else
            {
                if (listIndex != -1)
                    ManagerBlock.instance.stageInfo.listBlockAdjust.Remove((int)tempBlockType);
            }
        }
    }

    // 고정되는 얼음(2, 3단계)는 화면에 출력될 수 있는 얼음수에 합산할지 말지 설정하는 함수.
    void SetIceAdjust(int posX = 170, int posY = 172)
    {
        if (GUI.Toggle(new Rect(posX, posY, 180, 20), ManagerBlock.instance.stageInfo.isFixedIceAdjust == 1, "고정얼음 화면수에 미포함"))
            ManagerBlock.instance.stageInfo.isFixedIceAdjust = 1;
        else
            ManagerBlock.instance.stageInfo.isFixedIceAdjust = 0;
    }

    void SetStartInfo()
    {
        //컬러정보.
        GetStartColorTexture();

        List<int> listCanMakeBlock = new List<int>();
        foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
        {
            if (startInfo.probability > 0)
            {
                listCanMakeBlock.Add(startInfo.type);
            }
        }

        //블럭정보.
        string blockInfo = GetStartBlockString1(listCanMakeBlock);
        GUI.Label(new Rect(80, 140, 500, 20), blockInfo);

        string blockInfo2 = GetStartBlockString2(listCanMakeBlock);
        GUI.Label(new Rect(80, 160, 500, 20), blockInfo2);

        string blockInfo3 = GetStartBlockString3(listCanMakeBlock);
        GUI.Label(new Rect(80, 180, 500, 20), blockInfo3);
    }

    void GetStartColorTexture(float xPosition = 10f, float yPosition = 175f, float xOffset = 15)
    {
        if (decoInfo.count == 0 || ((decoInfo.count & ((int)(1 << (int)BlockColorType.A))) != 0))
        {
            DrawColorBox(xPosition, yPosition, BlockColorType.A);
            xPosition += xOffset;
        }
        if (decoInfo.count == 0 || ((decoInfo.count & ((int)(1 << (int)BlockColorType.B))) != 0))
        {
            DrawColorBox(xPosition, yPosition, BlockColorType.B);
            xPosition += xOffset;
        }
        if (decoInfo.count == 0 || ((decoInfo.count & ((int)(1 << (int)BlockColorType.C))) != 0))
        {
            DrawColorBox(xPosition, yPosition, BlockColorType.C);
            xPosition += xOffset;
        }
        if (decoInfo.count == 0 || ((decoInfo.count & ((int)(1 << (int)BlockColorType.D))) != 0))
        {
            DrawColorBox(xPosition, yPosition, BlockColorType.D);
            xPosition += xOffset;
        }
        if (decoInfo.count == 0 || ((decoInfo.count & ((int)(1 << (int)BlockColorType.E))) != 0))
        {
            DrawColorBox(xPosition, yPosition, BlockColorType.E);
        }
    }

    void DrawColorBox(float xPosition, float yPosition, BlockColorType colorType)
    {
        if (dicTexColorBox.ContainsKey(colorType) == true)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = dicTexColorBox[colorType];
            GUI.Box(new Rect(xPosition, yPosition, 10, 10), GUIContent.none, style);
        }
    }

    /// <summary>
    /// 출발에서 생성될 수 있는 블럭의 타입을 반환하는 함수
    /// typeOffset : int 최대치를 넘은 블럭들의 블럭타입에 - 해줄 오프셋 값
    /// </summary>
    string GetStartBlockName(List<int> listCanMakeBlock, string blockName, int blockType, int typeOffset = 0)
    {
        int bType = blockType - (32 * typeOffset);
        int startIdx = 0;
        switch (typeOffset)
        {
            case 0:
                startIdx = decoInfo.index;
                break;
            default:
                startIdx = decoInfo.type;
                break;
        }

        if ((startIdx & ((int)(1 << bType))) != 0)
        {
            if (listCanMakeBlock.Contains(blockType) == true)
                return blockName;
            else
                return string.Format("<color=#505050ff>{0}</color>", blockName);
        }
        return "";
    }

    string GetStartBlockString1(List<int> listCanMakeBlock)
    {
        string block = "";
        if ((decoInfo.index & ((int)(1 << (int)BlockType.NORMAL))) != 0)
            block += " 일반";
        block += GetStartBlockName(listCanMakeBlock, " 흙", (int)BlockType.GROUND);
        block += GetStartBlockName(listCanMakeBlock, " 쿠키", (int)BlockType.BOX);
        block += GetStartBlockName(listCanMakeBlock, " 흙보", (int)BlockType.GROUND_JEWEL);
        block += GetStartBlockName(listCanMakeBlock, " 흙폭", (int)BlockType.GROUND_BOMB);
        block += GetStartBlockName(listCanMakeBlock, " 흙열", (int)BlockType.GROUND_KEY);
        block += GetStartBlockName(listCanMakeBlock, " 흙사", (int)BlockType.GROUND_APPLE);
        block += GetStartBlockName(listCanMakeBlock, " 열쇠", (int)BlockType.KEY);
        block += GetStartBlockName(listCanMakeBlock, " 흙얼사", (int)BlockType.GROUND_ICE_APPLE);
        block += GetStartBlockName(listCanMakeBlock, " 사과", (int)BlockType.APPLE);
        block += GetStartBlockName(listCanMakeBlock, " 얼사", (int)BlockType.ICE_APPLE);
        block += GetStartBlockName(listCanMakeBlock, " 연쇄폭", (int)BlockType.BLOCK_BLACK);
        return block;
    }

    string GetStartBlockString2(List<int> listCanMakeBlock)
    {
        string block = "";
        block += GetStartBlockName(listCanMakeBlock, " 째깍", (int)BlockType.BLOCK_DYNAMITE);
        block += GetStartBlockName(listCanMakeBlock, " 얼음", (int)BlockType.ICE);
        block += GetStartBlockName(listCanMakeBlock, " 힐포", (int)BlockType.ADVENTURE_POTION_HEAL);
        block += GetStartBlockName(listCanMakeBlock, " 스포", (int)BlockType.ADVENTURE_POTION_SKILL);

        block += GetStartBlockName(listCanMakeBlock, " 둥근", (int)BlockType.START_Bomb, 1);
        block += GetStartBlockName(listCanMakeBlock, " 레인", (int)BlockType.START_Rainbow, 1);
        block += GetStartBlockName(listCanMakeBlock, " 폭죽", (int)BlockType.FIRE_WORK, 1);
        block += GetStartBlockName(listCanMakeBlock, " 코인주", (int)BlockType.COIN_BAG, 1);
        block += GetStartBlockName(listCanMakeBlock, " 콩", (int)BlockType.PEA, 1);
        block += GetStartBlockName(listCanMakeBlock, " 거미", (int)BlockType.SPACESHIP, 1);
        block += GetStartBlockName(listCanMakeBlock, " 폭콩", (int)BlockType.PEA_BOSS, 1);
        block += GetStartBlockName(listCanMakeBlock, " 라인폭", (int)BlockType.START_Line, 1);
        return block;
    }

    string GetStartBlockString3(List<int> listCanMakeBlock)
    {
        string block = "";
        block += GetStartBlockName(listCanMakeBlock, " 흙연쇄", (int)BlockType.GROUND_BLOCKBLACK, 1);
        block += GetStartBlockName(listCanMakeBlock, " 페인트", (int)BlockType.PAINT, 1);
        block += GetStartBlockName(listCanMakeBlock, " 물폭탄", (int)BlockType.WATERBOMB, 1);
        return block;
    }

    void GuiTab()
    {
        if (GUI.Toggle(new Rect(10f, 20, 40, 20), windowMode == EditWindowMode.HOME, "홈"))
            SetWindowMode(EditWindowMode.HOME);
        if (GUI.Toggle(new Rect(50f, 20, 45, 20), windowMode == EditWindowMode.BLOCK, "편1"))
            SetWindowMode(EditWindowMode.BLOCK);
        if (GUI.Toggle(new Rect(100f, 20, 45, 20), windowMode == EditWindowMode.PLANT, "편2"))
            SetWindowMode(EditWindowMode.PLANT);
        if (GUI.Toggle(new Rect(150f, 20, 45, 20), windowMode == EditWindowMode.GIMMICK, "편3"))
            SetWindowMode(EditWindowMode.GIMMICK);
        if (GUI.Toggle(new Rect(200f, 20, 45, 20), windowMode == EditWindowMode.GIMMICK2, "편4"))
            SetWindowMode(EditWindowMode.GIMMICK2);
        if (GUI.Toggle(new Rect(250f, 20, 45, 20), windowMode == EditWindowMode.GIMMICK3, "편5"))
            SetWindowMode(EditWindowMode.GIMMICK3); 
        if (GUI.Toggle(new Rect(300f, 20, 45, 20), windowMode == EditWindowMode.START, "출발"))
            SetWindowMode(EditWindowMode.START);
        if (GUI.Toggle(new Rect(350f, 20, 45, 20), windowMode == EditWindowMode.MAP_MAKER, "맵"))
            SetWindowMode(EditWindowMode.MAP_MAKER);
        if (GUI.Toggle(new Rect(400f, 20, 60, 20), windowMode == EditWindowMode.MAP_INFO, "맵 정보"))
            SetWindowMode(EditWindowMode.MAP_INFO);
        if (GUI.Toggle(new Rect(410f, 0, 60, 20), windowMode == EditWindowMode.ADVANCE_MODE, "모험"))
            SetWindowMode(EditWindowMode.ADVANCE_MODE);

        //저장/읽기 상태 표시
        ShowSaveAndLoadState();
    }

    /*
    public void SaveStageInfo()
    {
        ManagerBlock.instance.tempStageInfo = new StageInfo();

        ManagerBlock.instance.tempStageInfo.Extend_Y = ManagerBlock.instance.stageInfo.Extend_Y;

        ManagerBlock.instance.tempStageInfo.collectCount = ManagerBlock.instance.stageInfo.collectCount;
        ManagerBlock.instance.tempStageInfo.collectType = ManagerBlock.instance.stageInfo.collectType;

        ManagerBlock.instance.tempStageInfo.widthCentering = ManagerBlock.instance.stageInfo.widthCentering;
        ManagerBlock.instance.tempStageInfo.heightCentering = ManagerBlock.instance.stageInfo.heightCentering;

        ManagerBlock.instance.tempStageInfo.gameMode = ManagerBlock.instance.stageInfo.gameMode;

        ManagerBlock.instance.tempStageInfo.turnCount = ManagerBlock.instance.stageInfo.turnCount;
        ManagerBlock.instance.tempStageInfo.gameOverType = ManagerBlock.instance.stageInfo.gameOverType;

        ManagerBlock.instance.tempStageInfo.probability = ManagerBlock.instance.stageInfo.probability;

        ManagerBlock.instance.tempStageInfo.makeBombCount = ManagerBlock.instance.stageInfo.makeBombCount;
        ManagerBlock.instance.tempStageInfo.isCanUse2X2 = ManagerBlock.instance.stageInfo.isCanUse2X2;
        ManagerBlock.instance.tempStageInfo.RainbowXBombType = ManagerBlock.instance.stageInfo.RainbowXBombType;
        ManagerBlock.instance.tempStageInfo.BombType = ManagerBlock.instance.stageInfo.BombType;
        ManagerBlock.instance.tempStageInfo.waitState = ManagerBlock.instance.stageInfo.waitState;
        ManagerBlock.instance.tempStageInfo.noColorBlockType = ManagerBlock.instance.stageInfo.noColorBlockType;

        ManagerBlock.instance.tempStageInfo.StoneBlockType = ManagerBlock.instance.stageInfo.StoneBlockType;
        ManagerBlock.instance.tempStageInfo.digCount = ManagerBlock.instance.stageInfo.digCount;
        ManagerBlock.instance.tempStageInfo.digType = ManagerBlock.instance.stageInfo.digType;
        ManagerBlock.instance.tempStageInfo.startAutoBlock = ManagerBlock.instance.stageInfo.startAutoBlock;
        ManagerBlock.instance.tempStageInfo.AutoBlockLifeCount = ManagerBlock.instance.stageInfo.AutoBlockLifeCount;
        ManagerBlock.instance.tempStageInfo.auto_Probability = ManagerBlock.instance.stageInfo.auto_Probability;

        
        ManagerBlock.instance.tempStageInfo.ListBlock = new List<BlockInfo>();
        foreach(var block in ManagerBlock.instance.stageInfo.ListBlock)        
            ManagerBlock.instance.tempStageInfo.ListBlock.Add(block);


        ManagerBlock.instance.tempStageInfo.ListStartInfo = new List<StartBlockInfo>();
        foreach (var block in ManagerBlock.instance.stageInfo.ListStartInfo)
            ManagerBlock.instance.tempStageInfo.ListStartInfo.Add(block);
    }

    public void LoadStageInfo()
    {
        ManagerBlock.instance.stageInfo = new StageInfo();

        ManagerBlock.instance.stageInfo.Extend_Y = ManagerBlock.instance.tempStageInfo.Extend_Y;

        ManagerBlock.instance.stageInfo.collectCount = ManagerBlock.instance.tempStageInfo.collectCount;
        ManagerBlock.instance.stageInfo.collectType = ManagerBlock.instance.tempStageInfo.collectType;

        ManagerBlock.instance.stageInfo.widthCentering = ManagerBlock.instance.tempStageInfo.widthCentering;
        ManagerBlock.instance.stageInfo.heightCentering = ManagerBlock.instance.tempStageInfo.heightCentering;

        ManagerBlock.instance.stageInfo.gameMode = ManagerBlock.instance.tempStageInfo.gameMode;

        ManagerBlock.instance.stageInfo.turnCount = ManagerBlock.instance.tempStageInfo.turnCount;
        ManagerBlock.instance.stageInfo.gameOverType = ManagerBlock.instance.tempStageInfo.gameOverType;

        ManagerBlock.instance.stageInfo.probability = ManagerBlock.instance.tempStageInfo.probability;

        ManagerBlock.instance.stageInfo.makeBombCount = ManagerBlock.instance.tempStageInfo.makeBombCount;
        ManagerBlock.instance.stageInfo.isCanUse2X2 = ManagerBlock.instance.tempStageInfo.isCanUse2X2;
        ManagerBlock.instance.stageInfo.RainbowXBombType = ManagerBlock.instance.tempStageInfo.RainbowXBombType;
        ManagerBlock.instance.stageInfo.BombType = ManagerBlock.instance.tempStageInfo.BombType;
        ManagerBlock.instance.stageInfo.waitState = ManagerBlock.instance.tempStageInfo.waitState;
        ManagerBlock.instance.stageInfo.noColorBlockType = ManagerBlock.instance.tempStageInfo.noColorBlockType;

        ManagerBlock.instance.stageInfo.StoneBlockType = ManagerBlock.instance.tempStageInfo.StoneBlockType;
        ManagerBlock.instance.stageInfo.digCount = ManagerBlock.instance.tempStageInfo.digCount;
        ManagerBlock.instance.stageInfo.digType = ManagerBlock.instance.tempStageInfo.digType;
        ManagerBlock.instance.stageInfo.startAutoBlock = ManagerBlock.instance.tempStageInfo.startAutoBlock;
        ManagerBlock.instance.stageInfo.AutoBlockLifeCount = ManagerBlock.instance.tempStageInfo.AutoBlockLifeCount;
        ManagerBlock.instance.stageInfo.auto_Probability = ManagerBlock.instance.tempStageInfo.auto_Probability;


        ManagerBlock.instance.stageInfo.ListBlock = new List<BlockInfo>();
        foreach (var block in ManagerBlock.instance.tempStageInfo.ListBlock)
            ManagerBlock.instance.stageInfo.ListBlock.Add(block);


        ManagerBlock.instance.stageInfo.ListStartInfo = new List<StartBlockInfo>();
        foreach (var block in ManagerBlock.instance.tempStageInfo.ListStartInfo)
            ManagerBlock.instance.stageInfo.ListStartInfo.Add(block);
    }
    */

    private void SetWindowMode(EditWindowMode mode)
    {
        if (windowMode != mode)
        {
            windowMode = mode;
            eMode = EditMode.None;
            _toolState = "";
        }
    }

    private void MakeBlockRandomPos(int maxCount, BlockType type, int blockCount, bool isMakeActiveBoard = true)
    {
        List<Board> listCanMakeBoard = new List<Board>();

        int bombCheckCount = 10;
        int minX = GameManager.MIN_X + 1;
        int minY = GameManager.MIN_Y + 2;
        int maxX = minX + bombCheckCount - 1;
        int maxY = minY + bombCheckCount;

        for (int i = minX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
            {
                if (ManagerBlock.boards[i, j].IsHasScarp == true)
                    continue;

                if (ManagerBlock.boards[i, j].Block != null)
                {
                    if (ManagerBlock.boards[i, j].Block.type == type)
                    {
                        BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(i, j);
                        blockInfo.type = (int)BlockType.NONE;
                        blockInfo.colorType = 0;
                        blockInfo.count = 0;
                        blockInfo.index = 0;
                        blockInfo.subType = 0;
                    }
                    else
                        continue;
                }

                if (ManagerBlock.boards[i, j].IsActiveBoard != isMakeActiveBoard)
                    continue;

                listCanMakeBoard.Add(ManagerBlock.boards[i, j]);
            }
        }

        int makeCount = maxCount;
        if (maxCount < listCanMakeBoard.Count)
            GenericHelper.Shuffle(listCanMakeBoard);
        else if (maxCount > listCanMakeBoard.Count)
            makeCount = listCanMakeBoard.Count;

        for (int i = 0; i < makeCount; i++)
        {
            Board board = listCanMakeBoard[i];
            BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(board.indexX, board.indexY);
            blockInfo.type = (int)type;
            blockInfo.count = blockCount;
        }

        EditBlockGuide.reflesh = true;
    }

    #region 툴 가이드 표시 관련
    public void RemoveGimmickGuide_All()
    {
        foreach (var item in EditManager.instance.dicGimmickGuide)
        {
            if (item.Value.Count > 0)
            {
                List<GameObject> tempList = new List<GameObject>(item.Value);
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    Destroy(tempList[i].gameObject);
                }
                item.Value.Clear();
            }
        }
    }

    public void RemoveGimmickGuide(BlockType bType)
    {
        if (EditManager.instance.dicGimmickGuide.ContainsKey(bType) == true
            && EditManager.instance.dicGimmickGuide[bType].Count > 0)
        {
            List<GameObject> tempList = new List<GameObject>(EditManager.instance.dicGimmickGuide[bType]);
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                Destroy(tempList[i].gameObject);
            }
            EditManager.instance.dicGimmickGuide[bType].Clear();
        }
    }
    #endregion

    #region 코인스테이지

    void SetCoinStage()
    {
        GUI.Label(new Rect(5, 150, 30, 20), "확률");
        int.TryParse(GUI.TextField(new Rect(35, 150, 30, 20), ManagerBlock.instance.stageInfo.coinProp.ToString()), out ManagerBlock.instance.stageInfo.coinProp);

        GUI.Label(new Rect(80, 150, 100, 20), "피버 카운트");
        int.TryParse(GUI.TextField(new Rect(155, 150, 30, 20), ManagerBlock.instance.stageInfo.feverCount.ToString()), out ManagerBlock.instance.stageInfo.feverCount);

        GUI.Label(new Rect(5, 170, 80, 20), "피버시간");
        int.TryParse(GUI.TextField(new Rect(65, 170, 30, 20), ManagerBlock.instance.stageInfo.feverTime.ToString()), out ManagerBlock.instance.stageInfo.feverTime);
    }
    #endregion

    #region 출발 설정 관련
    //해당 타입이 검사하고자 하는 곳에서 생성될 수 있는지.
    private bool IsCanMakeBlockTypeAtDeco(int startBlockType, int blockType)
    {
        int checkBlockType = blockType;
        if (checkBlockType >= 32)
            checkBlockType -= 32;

        return ((startBlockType & ((int)(1 << checkBlockType))) != 0);
    }

    //데코 정보를 읽어, 생성될 수 있는 블럭 타입을 리스트의 형태로 반환해주는 함수
    private List<BlockType> GetListCanMakeType_Block(DecoInfo decoInfo)
    {
        List<BlockType> listBlockType = new List<BlockType>();

        for (int i = 0; i < listBlockTypeAndText.Count; i++)
        {
            BlockType bType = listBlockTypeAndText[i].blockType;
            if ((int)bType < 32)
            {
                if (IsCanMakeBlockTypeAtDeco(decoInfo.index, (int)bType) == true)
                    listBlockType.Add(bType);
            }
            else
            {
                if (IsCanMakeBlockTypeAtDeco(decoInfo.type, (int)bType) == true)
                    listBlockType.Add(bType);
            }
        }
        return listBlockType;
    }

    /// <summary>
    /// 데코 정보를 읽어, 블럭당 생성될 수 있는 컬러 타입을 딕셔너리 형태로 반환해주는 함수
    /// </summary>
    private Dictionary<BlockType, int> GetDicCanMakeType_Color(DecoInfo decoInfo)
    {
        Dictionary<BlockType, int> dicColorType = new Dictionary<BlockType, int>();
        foreach (var item in decoInfo.listBlockColorData)
        {
            BlockType bType = (BlockType)item.blockType;
            if (dicColorType.ContainsKey(bType) == false)
                dicColorType.Add(bType, item.subType);
        }
        return dicColorType;
    }

    private void InitStartProbData(BlockType bType)
    {
        if (dicEditData_StartProb.ContainsKey(bType) == false)
        {
            EditData_StartProbData tempData = new EditData_StartProbData();
            switch (bType)
            {
                case BlockType.PAINT:
                    tempData.time_Prob = new int[5] { 0, 0, 0, 0, 0 };
                    break;
                case BlockType.ICE:
                    tempData.count_Prob = new int[3] { 10, 0, 0 };
                    tempData.ice_Prob = new int[5] { 0, 0, 0, 0, 0 };
                    break;
            }
            dicEditData_StartProb.Add(bType, tempData);
        }
    }

    /// <summary>
    /// 원하는 출발정보 데이터 설정하기
    /// (dataType : 0 - count, 1 - time, 2 - ice)
    /// </summary>
    private void SetStartProbData(BlockType bType, int dataType, int[] probData)
    {
        if (dicEditData_StartProb.ContainsKey(bType) == false)
            InitStartProbData(bType);

        switch (dataType)
        {
            case 0:
                dicEditData_StartProb[bType].count_Prob = probData;
                break;
            case 1:
                dicEditData_StartProb[bType].time_Prob = probData;
                break;
            case 2:
                dicEditData_StartProb[bType].ice_Prob = probData;
                break;
        }
    }

    /// <summary>
    /// 원하는 출발정보 데이터 들고오기
    /// (dataType : 0 - count, 1 - time, 2 - ice)
    /// </summary>
    private int[] GetStartProbData(BlockType bType, int dataType)
    {
        if (dicEditData_StartProb.ContainsKey(bType) == false)
            InitStartProbData(bType);

        switch (dataType)
        {
            case 0:
                return dicEditData_StartProb[bType].count_Prob;
            case 1:
                return dicEditData_StartProb[bType].time_Prob;
            case 2:
                return dicEditData_StartProb[bType].ice_Prob;
        }
        return null;
    }
    #endregion

    #region 출발에서 생성 가능한 컬러 설정 관련
    private void SetStart_StartColor(int generaorIdx, BlockType bType, Vector2Int pos)
    {
        if (dicGeneratorBlockInfo.ContainsKey(generaorIdx) == false)
            return;

        if (dicToggleData_StartColorType == null)
            dicToggleData_StartColorType = new Dictionary<BlockType, int>();

        //저장되어 있던 생성가능 컬러 데이터 가져옴
        int colorData_cur = dicGeneratorBlockInfo[generaorIdx].dicCanMakeColor.ContainsKey(bType) ?
            dicGeneratorBlockInfo[generaorIdx].dicCanMakeColor[bType] : int.MaxValue;

        int colorData_next = 0;
        for (int i = 0; i < 5; i++)
        {
            int colorIdx = (i + 1);
            bool isUseColor_cur = ((colorData_cur & (1 << colorIdx)) != 0);
            GUI.Label(new Rect(pos.x - 3 + (30 * i), pos.y - 7, 30, 30), GetBlockColorName_WithColorType((BlockColorType)colorIdx));
            if (GUI.Toggle(new Rect(pos.x + (30 * i), pos.y + 7, 30, 20), isUseColor_cur, "") == true)
            {
                colorData_next += (1 << colorIdx);
            }
        }

        //데이터가 변경되었다면 새로 갱신
        if (colorData_cur != colorData_next)
        {
            if (dicToggleData_StartColorType.ContainsKey(bType) == true)
                dicToggleData_StartColorType[bType] = colorData_next;
            else
                dicToggleData_StartColorType.Add(bType, colorData_next);

            //토글 값이 달라졌을 경우, 현재 인덱스를 가지고 있는 블럭 생성기의 데이터 갱신
            RefreshDicGeneratorColorInfo(SelectGeneratorIdx, bType);
            RefreshGeneratorStartColorInfo_StageInfo(SelectGeneratorIdx);
        }
    }
    #endregion
    
    
    IEnumerator CoLoadUrl()
    {
        string url = "https://lgpkv-launcher.lt.treenod.com/1";
        using( var www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            www.timeout = 2;
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                Debug.Log(www.downloadHandler.text);
                if (www.downloadHandler.text.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<UIPopupServerSelect.ServerUrl>(www.downloadHandler.text);
                    if (data != null)
                    {
                        Global._gameServerAddress = data.gameUrl;
                        Global._cdnAddress = data.cdnUrl;
                        yield break;
                    }
                }
            }
            
            yield break;
        }
    }
}
