using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BlockColorType
{
    NONE,
    A,
    B,
    C,
    D,
    E,
    F,    //무색블럭으로 사용예정 //
    RANDOM, //??필요할까
}

public enum BlockType
{
    NONE,                   //0
    NORMAL,         //1
    PLANT,          //2
    KEY,            //3
    APPLE,          //4
    PLANT_APPLE,            //5 //제거

    PLANT_KEY,      //6         //제거
    GROUND,         //7
    BLOCK_BLACK,        //8 검댕이
    BOX,            //9//항아리
    BLOCK2X2,               //10

    STONE,          //11
    GROUND_JEWEL,     //12        //출발용
    GROUND_KEY,         //13        //출발용
    GROUND_BOMB,   //14        //출발용
    PLANT2X2,               //15

    GROUND_APPLE,   //16        //출발용 작업전
    PLANT_ICE_APPLE,    //17    //제거
    ICE_APPLE,          //18
    CANDY,         //19
    DUCK,                   //20

    GROUND_ICE_APPLE,       //21
    BLOCK_FIRE,             //22

    BLOCK_EVENT,            //23    이벤트블럭
    BLOCK_DYNAMITE,         //24    다이나마이트
    BLOCK_EVENT_STONE,      //25    이벤트바위
    BLOCK_COIN,             //26    블럭코인
    BUBBLE,                 //27    풍선
    ICE,                    //28    얼음,, 출발데코생성용
    ADVENTURE_POTION_HEAL,        //29    모험모드 포션_힐
    ADVENTURE_POTION_SKILL,        //30    모험모드 포션_스킬

    BigJewel,               //31
    ColorBigJewel,          //32

    LITTLE_FLOWER_POT,      //33 //Littel_FlowerPot 이름변경 //출발정보 2에서 처리  Board.startBlockType2  //startBlockType 다참
    EMTPY_TYPE_1,           //34    미사용
    EMTPY_TYPE_2,           //35    미사용

    START_Bomb,             //36    폭탄블럭 출발용
    START_Rainbow,          //37    폭탄블럭 출발용
    FIRE_WORK,              //38    폭죽
    
    COIN_BAG,               //38    돈자루
    SODAJELLY,              //40    소다젤리
    PEA,                    //41    완두콩
    WORLD_RANK_ITEM,        //42    월드랭킹 아이템
    SPACESHIP,              //43    우주선
    PEA_BOSS,               //44    대장완두콩
    START_Line,             //45    폭탄블럭 출발용

    HEART,                  //46    하트
    GROUND_BLOCKBLACK,      //47    흙연쇄폭탄
    HEART_HOME,             //48    하트 끝
    BLOCK_EVENT_GROUND,     //49    이벤트흙

    PAINT,                  //50    페인트
    BREAD,                  //51   빵
    WATERBOMB,              //52    물풍선
    ENDCONTENTS_ITEM,       //53    엔드컨텐츠 이벤트 아이템
    CANNON,                 //54    대포

    All = int.MaxValue,
}

public enum BlockBombType
{
    NONE,
    DUMMY,

    RAINBOW,
    LINE,
    LINE_H,
    LINE_V,
    BOMB,

    RAINBOW_X_RAINBOW,
    RAINBOW_X_BOMB,
    RAINBOW_X_LINE,
    BOMB_X_BOMB,
    BOMB_X_LINE,
    LINE_X_LINE,

    HALF_BOMB,
    CLEAR_BOMB,

    CROSS_LINE, //인게임아이템2

    BLACK_BOMB,

    R_LINE_H,
    R_LINE_V,

    ADVENTURE_BOMB,

    POWER_BOMB, //인게임아이템3

    RAINBOW_X_BOMB_WITHSELF,//인게임아이템4 //해당자리에도 둥근효과
}

public enum BlockState
{
    WAIT,
    PANG,
    MOVE,
    MOVE_OTHER, //흐름형이 아닌 움직임(흐름과 관계없이 움직여야 하는 경우 사용)
    HOLD,
}

public enum BlockDirection
{
    NONE,

    UP ,
    RIGHT ,
    DOWN ,
    LEFT,

    UP_RIGHT,
    DOWN_RIGHT,

    DOWN_LEFT,
    UP_LEFT,

}

public enum FireWorkRank
{
    RANK_1 = 1,
    RANK_2 = 2,
    RANK_3 = 3,
    RANK_4 = 4,
    RANK_5 = 5,

    RANK_NONE = 100,
}

public enum BlockSizeType
{
    NOMAL,  // 1x1 사이즈
    BIG,    // 1x1 보다 큰 사이즈
}

public class BlockBase : MonoBehaviour 
{
    public const int NORMAL_BLOCK_COUNT = 6;
    public const float MAX_VELOCITY = Mathf.PI*0.5f;
    public const float BLOCK_MOVE_SPEED_RATIO = 1f;

    public static List<BlockBase> _listBlock;
    public static List<BlockBase> _listBlockTemp;
    public static int _waitCount = 0;
    public static int StopMovingBlockCount = 0;
    public static int ExplodeBombCount = 0;

    public int indexX;
    public int indexY;

    //움직임 관련 
    public Transform _transform;
    public Vector3 targetPos = Vector3.zero;
    [System.NonSerialized] public float _velocity = 0f;
    private bool sideMove = false;
    public BlockDirection direction = BlockDirection.NONE;
    public bool isMoveDownThisTurn = false;

    //MOVEOTHER 상태일 때 강제로 이동 속도를 정해줄 때 사용.
    private float moveOtherSpeedRatio = 900;
    public float MoveOtherSpeedRatio
    {
        get  { return moveOtherSpeedRatio; }
        set { moveOtherSpeedRatio = value; }
    }

    public float _stateTimer = -1f;
    public float _spriteScale = 1f;
    float _scale = 1f;

    public BlockColorType rainbowColorType = BlockColorType.NONE;

    private List<BlockType> _listAdjustBlock = new List<BlockType>();
    private List<BlockType> listAdjustBlock
    {
        get
        {
            if (_listAdjustBlock.FindIndex(x => x == type) == -1)
            {
                _listAdjustBlock.Add(type);
            }

            return _listAdjustBlock;
        }
        set
        {
            _listAdjustBlock = value;
        }
    }

    [SerializeField]
    private BlockColorType mColorType = BlockColorType.NONE;
    public BlockColorType colorType
    {
        get { return mColorType; }
        set
        {
            if (mColorType == value)return;

            mColorType = value;
            UpdateSpriteByBlockType();
        }   
    }
    [SerializeField]
    private BlockType mType = BlockType.NONE;
    public BlockType type
    {
        get { return mType; }
        set
        {
            if (mType == value)return;
            
                mType = value;
                UpdateSpriteByBlockType();
        }
    }

    [SerializeField]
    private BlockBombType mExpectType = BlockBombType.NONE;
    public BlockBombType expectType
    {
        get { return mExpectType; }
        set
        {
            if (mExpectType != value)
            {
                ChangeTempBombSprite(value);
                mExpectType = value;            
            }
        }
    }

    public BlockBombType TempBombType = BlockBombType.NONE; //폭탄으로 바뀌는 효과를 위해서 사용

    [SerializeField]
    private BlockBombType mBombType = BlockBombType.NONE;
    public BlockBombType bombType
    {
        get { return mBombType; }
        set
        {
            if (mBombType == value) return;
            
            if (value == BlockBombType.BOMB) mColorType = BlockColorType.NONE;
            if (value == BlockBombType.LINE_H) mColorType = BlockColorType.NONE;
            if (value == BlockBombType.LINE_V) mColorType = BlockColorType.NONE;
            
            mBombType = value;
            if (value != BlockBombType.DUMMY)
            {
                if (value != BlockBombType.RAINBOW_X_BOMB &&
                    value != BlockBombType.RAINBOW_X_LINE &&
                    value != BlockBombType.RAINBOW_X_RAINBOW &&
                    value != BlockBombType.BOMB_X_BOMB &&
                    value != BlockBombType.BOMB_X_LINE &&
                    value != BlockBombType.LINE_X_LINE)
                {
                    UpdateSpriteByBlockType();
                }                
            }   

            if(value != BlockBombType.NONE)
                RemoveTempBombType();
        }
    }
    [SerializeField]
    private BlockState mState = BlockState.WAIT;
    public BlockState state
    {
        get { return mState; }
        set
        {
            if (mState == value)return;

            _stateTimer = 0f;
            _stageBeforePos = _transform.localPosition;

            mState = value;

            switch (value)
            {
                case BlockState.PANG:
                    SetStatePang();
                    SetSpriteAlpha(1);
                    break;

                case BlockState.WAIT:
                    bombEffect = false;
                    SetStateWait();

                    if (IsCanLink())
                    {
                        BlockMatchManager.instance.CheckBlockLinkToItem(this);
                    }
                    break;

                case BlockState.MOVE:
                    SetStateMove();
                    SetSpriteAlpha(1);
                    _velocity = 0f;
                    break;
            }
        }
    }
    
    //블럭 사이즈(1x1 or 1x1보다 큰 블럭[ex)컬러화단]) 관련
    [SerializeField]
    private BlockSizeType mSize = BlockSizeType.NOMAL;
    public BlockSizeType size
    {
        get { return mSize; }
        set
        {
            if (mSize == value) return;
            
            mSize = value;
        }
    }

    public virtual void SetStateWait() { }
    public virtual void SetStateMove() { }
    public virtual void SetStatePang() { }


    [SerializeField]
    int mLifeCount = 1;
    public int lifeCount
    {
        get { return mLifeCount; }
        set
        {
            if (mLifeCount == value)return;

            mLifeCount = value;
        }
    }
    
    private bool isDisabled = false;
    public bool IsDisabled
    {
        get
        {
            Board board = PosHelper.GetBoard(indexX, indexY);

            if (size == BlockSizeType.NOMAL)
                return board.HasDecoHideBlock();
            else
                return isDisabled;
        }
        set
        {
            isDisabled = value;
        }
    }

    //만들어질 블럭미리 보기
    public UISprite toyBombSprite = null;

    public UIBlockSprite mainSprite;
    public UISprite dummySprite;
    public List<CustomFill> dummySpriteList = new List<CustomFill>();

    public BlockCarpetSprite carpetSprite;

    // sprite을 얼마나 그릴지에 관련된 변수들.
    [System.NonSerialized] public Vector3 rTargetPos;

    //폭발관련
    public static int uniqueIndexCount = 1;
    public int pangIndex = -1;
    public bool bombEffect = false;
    public bool pang = false;
    public Vector3 _pangGatherEffect = Vector3.zero;    //폭탄생성시 모이는 이펙트
    public Vector3 _stageBeforePos = Vector3.zero;

    public bool isUseMakeBomb = false;
    public bool secendBomb = false;
    public bool bombHasCarpet = false;

    public bool hasCarpet = false; // 카펫이 묻어 있는지

    public bool StopMovingBlock = false;

    //매치관련
    public bool isCheckedMatchable = false; //같이 매치될지 체크
    public bool isCheckBombMakable = false; //링커생성이 가능한지 

    //팡관련
    public bool IsSkipPang = false;     //더블폭탄이 한번터졌을때?  //블럭제거를 막는것 //

    public bool isMakeBomb = false;     //터치에 의해서 폭탄이 생성되었을때
    public bool isRainbowBomb = false;  //레인보우에 의해서 폭탄이 생긴경우에 
    public bool isSkipDistroy = false;  //팡상태에서 블럭이 제거되지않고 기다림 //폭탄블럭에서 해재해줌

    public bool IsExplodeByBomb = false;
    public bool makeLineBomb = false;
    public bool isMatchedBomb = false;

    //레인보우조합에 의한 폭탄일떄 //화단에만 영향
    public bool byRainbowBomb = false;

    //일반팡딜레이
    public bool normalPangDelay = false;
    public float normaPangDelayTimer = 0f;

    //블럭제거 관련
    //[System.NonSerialized]
    public float _pangRemoveDelay = 0.0f;
    public float _pangAlphaDelay = 0.1f;

    //효과관련
    public bool isShowMakeBombEffect = false;

    //블럭데코
    public BlockDeco blockDeco = null;

    //링커관련
    public BlockLinker upLinker = null;
    public BlockLinker rightLinker = null;
    public BlockLinker leftLinker = null;
    public BlockLinker DownLinker = null;

    //이벤트 
    public bool IsSpecialEventBlock = false;

    //아이템사용
    public bool inGameItemUse = false;

    //인게임아이템 레인보우폭탄해머 사용
    public bool rainbowBombHammerUse = false;

    //출발에서 생성됨
    public bool isMakeByStart = false;

    //블럭 이벤트를 멈춤
    public bool isStopEvent = false;

    //링크를 억지로 끊은 경우.
    public bool isDestroyLink = false;

    //코인스테이지
    public bool coinFeverBomb = false;

    //블럭 이동 후 BlockStop() 이펙트, 위아래로 진동하는 이펙트 상태
    public bool isBlockStop_state = false;

    #region 폭탄 영역 이펙트 관련
    /// <summary>
    /// 해당 블럭에서 폭탄 영역 이펙트를 출력하고 싶을 때 사용하는 함수
    /// </summary>
    public virtual void MakeBombFieldEffect(float? dTime = null)
    {
        return;
    }
    #endregion

    public virtual void PostBlockMake()
    {
        ManagerUIAtlas.CheckAndApplyEventAtlas(mainSprite);

        if (toyBombSprite != null)
            ManagerUIAtlas.CheckAndApplyEventAtlas(toyBombSprite);
    }

    public void Destroylinker()
    {
        if (upLinker != null)
        {
            upLinker.DestroyLinker();
        }
        if (rightLinker != null)
        {
            rightLinker.DestroyLinker();
        }
        if (leftLinker != null)
        {
            leftLinker.DestroyLinker();
        }
        if (DownLinker != null)
        {
            DownLinker.DestroyLinker();
        }

        upLinker = null;
        rightLinker = null;
        leftLinker = null;
        DownLinker = null;
    }

    public void RemoveLinkerNoReset()
    {
        if (upLinker != null)
        {
            upLinker.DestroyLinkerNoReset();
        }
        if (rightLinker != null)
        {
            rightLinker.DestroyLinkerNoReset();
        }
        if (leftLinker != null)
        {
            leftLinker.DestroyLinkerNoReset();
        }
        if (DownLinker != null)
        {
            DownLinker.DestroyLinkerNoReset();
        }

        upLinker = null;
        rightLinker = null;
        leftLinker = null;
        DownLinker = null;
    }

    public void Removelinker(BlockLinker linker)
    {
        if (upLinker == linker)
        {
            upLinker = null;
        }
        if (rightLinker == linker)
        {
            rightLinker = null;
        }
        if (leftLinker == linker)
        {
            leftLinker = null;
        }
        if (DownLinker == linker)
        {
            DownLinker = null;
        }
    }


    public bool isLastMove(int addIndexY) //블럭이 더이상 이동할곳이 없는지 체크
    {
        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, addIndexY) == null || !PosHelper.GetBoardSreeen(indexX, indexY, 0, addIndexY).IsActiveBoard)
        {
            return true;
        }
        else if (PosHelper.GetBlockScreen(indexX, indexY, 0, addIndexY) == null)
        {
            return false;
        }

        else if (PosHelper.GetBoardSreeen(indexX, indexY, 1, addIndexY) != null &&
            PosHelper.GetBlockScreen(indexX, indexY, 1, addIndexY) == null &&
            (PosHelper.GetBoardSreeen(indexX, indexY, 1, addIndexY - 1) == null || PosHelper.GetBlockScreen(indexX, indexY, 1, addIndexY - 1) == null)
            )
        {
            return false;
        }
        else if (PosHelper.GetBoardSreeen(indexX, indexY, -1, addIndexY) != null &&
            PosHelper.GetBlockScreen(indexX, indexY, -1, addIndexY) == null &&
            (PosHelper.GetBoardSreeen(indexX, indexY, -1, addIndexY) == null || PosHelper.GetBlockScreen(indexX, indexY, -1, addIndexY) == null))
        {
            return false;
        }
        else
        {
            return isLastMove(addIndexY + 1);
        }
    }

    const float LINK_MAKE_DISTANCE = 1.1f;

    public void MakeLinkerByManager()
    {
        if (!IsCanLink()) return;
        if (PosHelper.GetBoardSreeen(indexX, indexY) == null) return;


        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            if (rightLinker != null) Destroy(rightLinker.gameObject);
            if (DownLinker != null) Destroy(DownLinker.gameObject);
            return;
        }

        //장막 기믹류가 있을 때 링크 제거
        if (PosHelper.GetBoardSreeen(indexX, indexY).HasDecoHideBlock()) return;

        if (type == BlockType.BLOCK_BLACK &&
        (state != BlockState.WAIT || state != BlockState.PANG) &&
        GameManager.gameMode == GameMode.ADVENTURE &&
        GameManager.adventureMode == AdventureMode.CURRENT) return;
        
        if (type == BlockType.BLOCK_BLACK && state != BlockState.WAIT) return;

        BlockBase rightBlock = PosHelper.GetBlockScreen(indexX, indexY, 1, 0);

        if (rightBlock != null
            //&& state == BlockState.WAIT
           // && rightBlock.state == BlockState.WAIT
            && rightBlock.IsCanLink()
            && checkSameType(rightBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 1, 0)
            && rightBlock.secendBomb == false
            && rightBlock.isRainbowBomb == false
            && secendBomb == false
            && isRainbowBomb == false
            && Vector3.Distance(_transform.localPosition, rightBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * ManagerBlock.linkerDistanceRatio
        )
        {
            if(IsBombBlock() && PosHelper.GetBoard(indexX,indexY).HasDecoCoverBlock())
            {

            }
            else if (rightBlock.IsBombBlock() && PosHelper.GetBoard(indexX, indexY,1,0).HasDecoCoverBlock())
            {

            }
            else if(blockDeco != null && blockDeco.IsInterruptBlockSelect())
            {

            }
            else if (rightBlock.blockDeco != null && rightBlock.blockDeco.IsInterruptBlockSelect())
            {

            }   
            else if ((type == BlockType.BLOCK_BLACK && rightBlock.type != BlockType.BLOCK_BLACK) ||(type != BlockType.BLOCK_BLACK && rightBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else if (
                (state == BlockState.WAIT && rightBlock.state == BlockState.WAIT) //||
                || ((state == BlockState.WAIT || state == BlockState.PANG) && (rightBlock.state == BlockState.PANG || rightBlock.state == BlockState.WAIT))
             //   (state == BlockState.WAIT && rightBlock.isLastMove(1)) ||
             //   (isLastMove(1) && rightBlock.state == BlockState.WAIT) ||
             //   (rightBlock.isLastMove(1) && isLastMove(1))
            )
            {
                if (rightLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    rightLinker =
                        NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                rightLinker.blockA = this;
                rightLinker.blockB = rightBlock;
                rightLinker.linkerDirectionA = 2;
                rightLinker.linkerDirectionB = 4;

                if (type == BlockType.BLOCK_BLACK)
                {
                    rightLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else 
                    rightLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);
                

                rightLinker.SetSpriteAlpha(mainSprite.color.a);

                rightBlock.leftLinker = rightLinker;
            }
        }

        BlockBase downBlock = PosHelper.GetBlockScreen(indexX, indexY, 0, 1);

        if (downBlock != null
           // && state == BlockState.WAIT
          //  && downBlock.state == BlockState.WAIT
            && downBlock.IsCanLink()
            && checkSameType(downBlock)
            && downBlock.secendBomb == false
            && downBlock.isRainbowBomb == false
            && secendBomb == false
            && isRainbowBomb == false
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, 1)
            && Vector3.Distance(_transform.localPosition, downBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * ManagerBlock.linkerDistanceRatio
            )
        {

            if (IsBombBlock() && PosHelper.GetBoard(indexX, indexY).HasDecoCoverBlock())
            {

            }
            else if (downBlock.IsBombBlock() && PosHelper.GetBoard(indexX, indexY, 0, 1).HasDecoCoverBlock())
            {

            }
            else if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            {

            }
            else if (downBlock.blockDeco != null && downBlock.blockDeco.IsInterruptBlockSelect())
            {

            }
            else if ((type == BlockType.BLOCK_BLACK && downBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && downBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else if 
                (
                (state == BlockState.WAIT && downBlock.state == BlockState.WAIT)// ||
                || ((state == BlockState.WAIT || state == BlockState.PANG) && (downBlock.state == BlockState.PANG || downBlock.state == BlockState.WAIT))
             //   (state == BlockState.WAIT && downBlock.isLastMove(1)) ||
             //   (isLastMove(1) && downBlock.state == BlockState.WAIT) ||
             //   (downBlock.isLastMove(1) && isLastMove(1))
            )
            {
                if (DownLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    DownLinker = NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                DownLinker.blockA = this;
                DownLinker.blockB = downBlock;

                DownLinker.linkerDirectionA = 3;
                DownLinker.linkerDirectionB = 1;

                if (type == BlockType.BLOCK_BLACK)
                {
                    DownLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    DownLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);


                DownLinker.SetSpriteAlpha(mainSprite.color.a);

                downBlock.upLinker = DownLinker;  
            }
        }
    }

    public void MakeLinkerByWaitState()
    {
        if (!IsCanLink()) return;
        if (PosHelper.GetBoardSreeen(indexX, indexY) == null) return;


        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            if (rightLinker != null) Destroy(rightLinker.gameObject);
            if (DownLinker != null) Destroy(DownLinker.gameObject);
            return;
        }

        //장막 기믹류가 있을 때 링크 제거
        if (PosHelper.GetBoardSreeen(indexX, indexY).HasDecoHideBlock()) return;

        if (type == BlockType.BLOCK_BLACK &&
        (state != BlockState.WAIT || state != BlockState.PANG) &&
        GameManager.gameMode == GameMode.ADVENTURE &&
        GameManager.adventureMode == AdventureMode.CURRENT) return;
 
        if (type == BlockType.BLOCK_BLACK && state != BlockState.WAIT) return;

        BlockBase rightBlock = PosHelper.GetBlockScreen(indexX, indexY, 1, 0);

        if (rightBlock != null
            //&& state == BlockState.WAIT
            // && rightBlock.state == BlockState.WAIT
            && rightBlock.IsCanLink()
            && checkSameType(rightBlock)
            && rightBlock.secendBomb == false
            && rightBlock.isRainbowBomb == false
            && secendBomb == false
            && isRainbowBomb == false
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 1, 0)
            &&
            Vector3.Distance(_transform.localPosition, rightBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * ManagerBlock.linkerDistanceRatio
        )
        {
            if (IsBombBlock() && PosHelper.GetBoard(indexX, indexY).HasDecoCoverBlock())
            {

            }
            else if (rightBlock.IsBombBlock() && PosHelper.GetBoard(indexX, indexY, 1, 0).HasDecoCoverBlock())
            {

            }
            else if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            {

            }
            else if (rightBlock.blockDeco != null && rightBlock.blockDeco.IsInterruptBlockSelect())
            {

            }
            else if ((type == BlockType.BLOCK_BLACK && rightBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && rightBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            if (
                (state == BlockState.WAIT && rightBlock.state == BlockState.WAIT)
                 || ((state == BlockState.WAIT || state == BlockState.PANG) && (rightBlock.state == BlockState.PANG || rightBlock.state == BlockState.WAIT))
            //    || (state == BlockState.WAIT && rightBlock.isLastMove(1)) 
            //    || (isLastMove(1) && rightBlock.state == BlockState.WAIT) 
            //    || (rightBlock.isLastMove(1) && isLastMove(1))
            )
            {


                if (rightLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    rightLinker =
                        NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                rightLinker.blockA = this;
                rightLinker.blockB = rightBlock;
                rightLinker.linkerDirectionA = 2;
                rightLinker.linkerDirectionB = 4;

                if (type == BlockType.BLOCK_BLACK)
                {
                    rightLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    rightLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);

                rightBlock.leftLinker = rightLinker;
            }
        }
        else if (rightLinker != null && rightLinker.blockA != null && rightLinker.blockB != null)
        {
            if (rightLinker.blockA.indexX == rightLinker.blockB.indexX)
                rightLinker.DestroyLinkerNoReset();
        }

        BlockBase downBlock = PosHelper.GetBlockScreen(indexX, indexY, 0, 1);

        if (downBlock != null
            // && state == BlockState.WAIT
            //  && downBlock.state == BlockState.WAIT
            && downBlock.IsCanLink()
            && checkSameType(downBlock)
            && downBlock.secendBomb == false
            && downBlock.isRainbowBomb == false
            && secendBomb == false
            && isRainbowBomb == false
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, 1)
            && Vector3.Distance(_transform.localPosition, downBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * ManagerBlock.linkerDistanceRatio
            )
        {
            if (IsBombBlock() && PosHelper.GetBoard(indexX, indexY).HasDecoCoverBlock())
            {

            }
            else if (downBlock.IsBombBlock() && PosHelper.GetBoard(indexX, indexY, 0, 1).HasDecoCoverBlock())
            {

            }
            else if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            {

            }
            else if (downBlock.blockDeco != null && downBlock.blockDeco.IsInterruptBlockSelect())
            {

            }
            else if ((type == BlockType.BLOCK_BLACK && downBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && downBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            if (
                (state == BlockState.WAIT && downBlock.state == BlockState.WAIT)
                 || ((state == BlockState.WAIT || state == BlockState.PANG) && (downBlock.state == BlockState.PANG || downBlock.state == BlockState.WAIT))
             //   || (state == BlockState.WAIT && downBlock.isLastMove(1)) 
             //   || (isLastMove(1) && downBlock.state == BlockState.WAIT) 
             //   || (downBlock.isLastMove(1) && isLastMove(1))
            )
            {
                if (DownLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    DownLinker = NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                DownLinker.blockA = this;
                DownLinker.blockB = downBlock;

                DownLinker.linkerDirectionA = 3;
                DownLinker.linkerDirectionB = 1;
                if (type == BlockType.BLOCK_BLACK)
                {
                    DownLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    DownLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);

                downBlock.upLinker = DownLinker;
            }
        }
        else if (DownLinker != null && DownLinker.blockA != null && DownLinker.blockB != null)
        {
            if (DownLinker.blockA.indexY == DownLinker.blockB.indexY)
                DownLinker.DestroyLinkerNoReset();
        }
    }


    public void MakeLinkerAll()
    {
        if (!IsCanLink()) return;
        if (PosHelper.GetBoardSreeen(indexX, indexY) == null) return;


        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            if (rightLinker != null) Destroy(rightLinker.gameObject);
            if (DownLinker != null) Destroy(DownLinker.gameObject);
            return;
        }

        if (type == BlockType.BLOCK_BLACK &&
        (state != BlockState.WAIT || state != BlockState.PANG) &&
        GameManager.gameMode == GameMode.ADVENTURE &&
        GameManager.adventureMode == AdventureMode.CURRENT) return;
        
        if (type == BlockType.BLOCK_BLACK && state != BlockState.WAIT) return;

        BlockBase rightBlock = PosHelper.GetBlockScreen(indexX, indexY, 1, 0);

        if (rightBlock != null
            && state == BlockState.WAIT
            && rightBlock.state == BlockState.WAIT
            && rightBlock.IsCanLink()
            && checkSameType(rightBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 1, 0)
            &&
            Vector3.Distance(_transform.localPosition, rightBlock._transform.localPosition) <
            ManagerBlock.BLOCK_SIZE * LINK_MAKE_DISTANCE
        )
        {
            if ((type == BlockType.BLOCK_BLACK && rightBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && rightBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            {
                if (rightLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    rightLinker =
                        NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                rightLinker.blockA = this;
                rightLinker.blockB = rightBlock;
                rightLinker.linkerDirectionA = 2;
                rightLinker.linkerDirectionB = 4;
                if (type == BlockType.BLOCK_BLACK)
                {
                    rightLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    rightLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);
                rightLinker.SetSpriteAlpha(mainSprite.color.a);

                rightBlock.leftLinker = rightLinker;
            }
        }


        BlockBase leftBlock = PosHelper.GetBlockScreen(indexX, indexY, -1, 0);

        if (leftBlock != null
            && state == BlockState.WAIT
            && leftBlock.state == BlockState.WAIT
            && leftBlock.IsCanLink()
            && checkSameType(leftBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, -1, 0)
            && Vector3.Distance(_transform.localPosition, leftBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * LINK_MAKE_DISTANCE
            )
        {
            if ((type == BlockType.BLOCK_BLACK && leftBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && leftBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            {
                if (leftLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    leftLinker = NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                leftLinker.blockA = leftBlock;
                leftLinker.blockB = this;
                leftLinker.linkerDirectionA = 4;
                leftLinker.linkerDirectionB = 2;
                if (type == BlockType.BLOCK_BLACK)
                {
                    leftLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    leftLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);
                leftLinker.SetSpriteAlpha(mainSprite.color.a);

                leftBlock.rightLinker = leftLinker;
            }
        }


        BlockBase downBlock = PosHelper.GetBlockScreen(indexX, indexY, 0, 1);

        if (downBlock != null
            && state == BlockState.WAIT
            && downBlock.state == BlockState.WAIT
            && downBlock.IsCanLink()
            && checkSameType(downBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, 1)
            && Vector3.Distance(_transform.localPosition, downBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * LINK_MAKE_DISTANCE
            )
        {
            if ((type == BlockType.BLOCK_BLACK && downBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && downBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            {
                if (DownLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    DownLinker = NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                DownLinker.blockA = this;
                DownLinker.blockB = downBlock;

                DownLinker.linkerDirectionA = 3;
                DownLinker.linkerDirectionB = 1;
                if (type == BlockType.BLOCK_BLACK)
                {
                    DownLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    DownLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);
                DownLinker.SetSpriteAlpha(mainSprite.color.a);

                downBlock.upLinker = DownLinker;
            }
        }

        BlockBase upBlock = PosHelper.GetBlockScreen(indexX, indexY, 0, -1);

        if (upBlock != null
            && state == BlockState.WAIT
            && upBlock.state == BlockState.WAIT
            && upBlock.IsCanLink()
            && checkSameType(upBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, -1)
            && Vector3.Distance(_transform.localPosition, upBlock._transform.localPosition) < ManagerBlock.BLOCK_SIZE * LINK_MAKE_DISTANCE
            )
        {
            if ((type == BlockType.BLOCK_BLACK && upBlock.type != BlockType.BLOCK_BLACK) || (type != BlockType.BLOCK_BLACK && upBlock.type == BlockType.BLOCK_BLACK))
            {

            }
            else
            {
                if (upLinker == null)
                {
                    GameObject linkerGameObject = BlockMaker.instance.blockLinkerObject;
                    upLinker = NGUITools.AddChild(GameUIManager.instance.groundAnchor, linkerGameObject).GetComponent<BlockLinker>();
                }

                upLinker.blockA = upBlock;
                upLinker.blockB = this;

                upLinker.linkerDirectionA = 1;
                upLinker.linkerDirectionB = 3;
                if (type == BlockType.BLOCK_BLACK)
                {
                    upLinker.Setlinker(LINKER_TYPE.BLACK);
                }
                else
                    upLinker.Setlinker(IsBombBlock() ? LINKER_TYPE.BOMB : LINKER_TYPE.NORMAL);
                upLinker.SetSpriteAlpha(mainSprite.color.a);

                upBlock.DownLinker = upLinker;
            }
        }
    }

    public virtual bool IsNormalBlock()
    {
        return false;
    }

    public virtual bool IsBombBlock()
    {
        return false;
    }

    // 흐르는 블럭
    // 물에 뜨는 블럭들은 isBlockType == true
    public virtual bool IsBlockType()
    {
        return true;
    }

    public virtual bool IsSelectable()
    {
        return false;
    }

    public virtual bool IsCanPang()
    {
        return false;
    }

    //파워 폭탄으로 터질 수 있는지.
    public virtual bool IsCanPangByPowerBomb()
    {
        return IsCanPang();
    }

    public virtual bool isCanPangByRainbow()
    {
        return true;
    }

    public virtual bool IsCanLink()
    {
        return false;
    }

    public virtual bool IsPangAffectBoard() //블럭의 폭발이 보드에 영향을 주는지?
    {
        return false;
    }

    public virtual bool IsCanMove()
    {
        return false;
    }

    public virtual bool IsPangExtendable()
    {
        return true;
    }

    public virtual bool IsTarget_LavaMode()
    {
        return false;
    }

    public virtual bool IsDigTarget()
    {
        return false;
    }

    public  virtual bool IsDepomerBlock()
    {
        return true;
    }

    public virtual bool IsSpecialMatchable()
    {
        return false;
    }

    public virtual bool IsCanRemoveWater()
    {
        return false;
    }

    public virtual bool IsPangStopMove() //이것이 터질때는 블럭의 움직임이 멈춤
    {
        return false;
    }

    public virtual bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return false;
    }
    
    public virtual bool IsCoverCarpet() //카펫위에 덮이는 블럭인지(카펫을 감추는 블럭인지)
    {
        return false;
    }

    public virtual bool IsCanCoverIce() // 얼음이 생성될 수 있는 블럭인지.
    {
        return true;
    }

    public virtual bool IsCanMakeCarpet() //현재 블럭 아래 카펫을 만들 수 있는지.
    {
        return false;
    }

    public virtual bool IsCanMakeCarpetByBomb() // 현재 블럭 위로 카펫묻은 폭탄이 지나가면 밑에 카펫을 만들 수 있는지.
    {
        return false;
    }

    public virtual bool IsCanCoveredCarpet() // 카펫이 묻을 수 있는 지
    {
        return false;
    }

    public virtual bool IsCanAddCoin() //코인이 붙을 수 있는 블럭인지
    {
        return false;
    }

    // public virtual void GetMainSpriteDepth()
    // {
    //     mainSprite.depth = ManagerBlock.BLOCK_SRPRITE_MIN;// indexY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT + ManagerBlock.BLOCK_SRPRITE_MIN;
    // }

    public virtual void SetMainSpriteDepth()
    {
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE;
    }

    public virtual bool IsThisBlockHasPlace()   //바닥을 가지는 블럭인지
    {
        return false;
    }

    public virtual bool IsStopEvnetAtDestroyIce()   //얼음을 제거했을 때, 블럭의 이벤트를 멈추는지.
    {
        return false;
    }

    public virtual bool IsStopEvnetAtDestroyWater()   //물을 제거했을 때, 블럭의 이벤트를 멈추는지.
    {
        return false;
    }

    public virtual bool IsStopEvnetAtDestroyNet()   //잡기돌을 제거했을 때, 블럭의 이벤트를 멈추는지.
    {
        return false;
    }

    public virtual bool IsStopEventAtDestroyRandomBox() //  랜덤박스를 제거했을 때, 블럭의 이벤트를 멈추는지.
    {
        return false;
    }

    /// <summary>
    /// 클로버를 제거하거나 클로버 밖으로 나왔을 때 블럭의 이벤트를 멈추는지.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsStopEventAtOutClover()
    {
        return false;
    }

    /// <summary>
    /// 서로 링크 된 블럭기믹이 제거가 된 이후에 제거가 되야 할 블럭 위에 클로버 기믹이 있으면 해당 클로버 기믹을 제거 하는지.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsRemoveHideDeco_AtBlockPang()
    {
        return false;
    }

    public virtual bool IsStopEventAtMoveGround() // 땅파기 모드 웨이브가 전환되었을때, 블럭의 이벤트를 멈추는지.
    {
        return false;
    }

    public virtual bool IsDestroyBlockAtStageClear()    //스테이지 클리어 직후, 획득하지 못하면 사라지는 블럭인지
    {
        return false;
    }

    public virtual bool IsCanMakeBombFieldEffect() //폭탄 영역 표시가 출력될 수 있는 블럭인지
    {
        return true;
    }

    public virtual bool GetTargetBoard(BlockType blockType)
    {
        return false;
    }

    public virtual bool IsBlockRemoving() //블럭이 제거되고 있는 상황인지
    {
        return (lifeCount <= 0);
    }

    public virtual void SetRandomBoxHide(bool isHide)
    {
        if(IsCanMove() == true)
        {
            SetSpriteEnabled(isHide);
        }
    }

    public virtual void SetHideBeforeCloverPang() { }  // 클로버 기믹 팡 직전에 기믹 활성화가 필요할 때 사용
    public virtual void SetCloverHide(bool isHide) { } // 클로버 기믹 아래에 있을 때 이펙트 꺼짐

    private void RemoveCloverDeco()
    {
        Board board = PosHelper.GetBoard(indexX, indexY);

        if (board == null) return;
        
        for (int i = 0; i < board.BoardOnHide.Count; i++)
        {
            board.BoardOnHide[i].SetHideDecoPang();
        }
    }
    
    public virtual void ChangeTempBombSprite(BlockBombType xpectBombType)
    {
    }

    public virtual void RemoveTempBombType() { }

    public virtual void UpdateBlock()
    {
        if (IsCanMove())
        {
            MoveBlock();
        }
        else
        {
            StatePang();
        }

    }

    //해당 블럭이 폭탄 효과를 막는지 검사 // 터짐X
    public virtual bool IsDisturbBlock_ByBomb(int tempPangIndex = 0, bool isPangByPowerBomb = false)
    {
        if (IsSkipPang)
            return IsPangExtendable();

        if (isRainbowBomb)
            return IsPangExtendable();

        if (state == BlockState.PANG)
            return IsPangExtendable();

        bool isCanPang = (isPangByPowerBomb == true) ? IsCanPangByPowerBomb() : IsCanPang();
        if (isCanPang == false)
        {
            isUseMakeBomb = false;
            normalPangDelay = false;

            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard == null) return IsPangExtendable();

            if (getBoard.IsDecoDisturbBlock())
                return IsPangExtendable();

            return IsPangExtendable();
        }

        if (pangIndex == tempPangIndex)
            return IsPangExtendable();

        Board getBoardB = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoardB == null)
        {
            normalPangDelay = false;
            return IsPangExtendable();
        }

        if (getBoardB.BoardOnHide.Count > 0)
            return getBoardB.HasDecoHideBlock();

        return IsPangExtendable();
    }

    //외부에서 블럭을 터트릴때 //폭발을 막는지 안막는지 체크
    public virtual bool BlockPang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false, bool isPangByPowerBomb = false)
    {
        if (IsSkipPang)        
            return IsPangExtendable();        

        if (isRainbowBomb)
            return IsPangExtendable();

        if (state == BlockState.PANG)
            return IsPangExtendable();

        bool isCanPang = (isPangByPowerBomb == true) ? IsCanPangByPowerBomb() : IsCanPang();
        if (isCanPang == false)
        {
            isUseMakeBomb = false;
            normalPangDelay = false;

            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard == null)return IsPangExtendable();

            if (getBoard.HasDecoHideBlock(true, tempPangIndex, pangColorType))
                return IsPangExtendable();
            
            if (getBoard.HasDecoCoverBlock(true, tempPangIndex, pangColorType))
                return IsPangExtendable();

            if (blockDeco != null)
                blockDeco.DecoPang(tempPangIndex, pangColorType);
            
            return IsPangExtendable();
        }

        if (pangIndex == tempPangIndex)
            return IsPangExtendable();
        
        pangIndex = tempPangIndex;

        Board getBoardB = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoardB == null)
        {
            normalPangDelay = false;
            return IsPangExtendable();
        }

        bool interruptExtendPangB = IsPangExtendable();

        if (getBoardB.BoardOnHide.Count > 0)
            return getBoardB.HasDecoHideBlock(true, tempPangIndex, pangColorType);
        
        if (IsPangStopMove() && IsBlockType() && getBoardB.HasDecoCoverBlock(false, tempPangIndex, pangColorType))
        {
            state = BlockState.PANG;
        }
        else if (getBoardB.HasDecoCoverBlock(true, tempPangIndex, pangColorType))
        {
            if (IsBlockType())
            {
                isUseMakeBomb = false;
                state = BlockState.WAIT;
                isCheckedMatchable = false;
                BlockMatchManager.instance.CheckBlockLinkToItem(this);
            }
            normalPangDelay = false;
            return interruptExtendPangB;
        }

        if (blockDeco == null)
        {
            if (IsBlockType())
                state = BlockState.PANG;

            if (normalPangDelay == false)
            {
                if (isPangByPowerBomb == true)
                    PangByPowerBomb(pangColorType, PangByBomb);
                else
                    Pang(pangColorType, PangByBomb);
            }

            return interruptExtendPangB;
        }

        if (blockDeco.DecoPang(tempPangIndex, pangColorType))
        {
            if (IsBlockType())
            {
                isUseMakeBomb = false;
                state = BlockState.WAIT;
                isCheckedMatchable = false;
                BlockMatchManager.instance.CheckBlockLinkToItem(this);
            }
            normalPangDelay = false;
        }
        else
        {
            if (IsBlockType())
                state = BlockState.PANG;

            if (normalPangDelay == false)
            {
                if (isPangByPowerBomb == true)
                    PangByPowerBomb(pangColorType, PangByBomb);
                else
                    Pang(pangColorType, PangByBomb);
            }
        }
        return interruptExtendPangB;
    }

    //제거 가능한 기믹의 경우, 얼음, 잡기돌 및 단계 상관없이 즉시제거
    public virtual IEnumerator CoPangImmediately()
    {
        lifeCount = 0;

        DestroyBlockData();

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);

        yield return null;
    }

    //스테이지 클리어 시, 이벤트 상태에서 터지지 못한 블럭들 제거
    public virtual IEnumerator CoPangThatCannotRemoveFromEventState()
    {
        yield break;
    }

    //실제 폭발
    public virtual void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoard != null)
        {
            getBoard.BoardPang(pangIndex, pangColorType, PangByBomb);
        }
    }

    //전체 블럭의 단계를 감소시키는 폭탄이 터졌을 때 처리
    public virtual void PangByPowerBomb(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        Pang(pangColorType, PangByBomb);
    }

    public void SetSplashEffect(BlockColorType colorType = BlockColorType.NONE, bool takeEffectbomb = false)
    {
        if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null)
        {
			if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).IsNotDisturbBlock(BlockDirection.LEFT) == true)
            {
            	if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).SetSplashEffect(pangIndex, colorType, takeEffectbomb) == false)
            	{
                	if (!PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoHideBlock() && 
                        !PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoCoverBlock() &&
                         PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null)
                        PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb, 1, byRainbowBomb);
            	}                
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null)
        {
			if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).IsNotDisturbBlock(BlockDirection.RIGHT) == true)
            {
            	if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).SetSplashEffect(pangIndex, colorType, takeEffectbomb) == false)
            	{
                	if (!PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoHideBlock() &&
                        !PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoCoverBlock() &&
                         PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null)
                        PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb, 3, byRainbowBomb);
				}
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null)
        {
			if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).IsNotDisturbBlock(BlockDirection.UP) == true)
            {
            	if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).SetSplashEffect(pangIndex, colorType, takeEffectbomb) == false)
            	{
                	if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoHideBlock()&&
                        !PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoCoverBlock() &&
                         PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null)
                        PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb, 2, byRainbowBomb);
				}
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null)
        {
			if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).IsNotDisturbBlock(BlockDirection.DOWN) == true)
            {
            	if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).SetSplashEffect(pangIndex, colorType, takeEffectbomb) == false)
            	{
                	if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoHideBlock() && 
                        !PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoCoverBlock() &&
                         PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null)
                        PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb, 0, byRainbowBomb);
				}
            }
        }
    }

    public virtual bool EventAction()//턴소비후 물이늘어나거나 하는 행동 턴이 줄어들거나
    {
        return false;
    }

    public virtual void MoveAction()
    {
        
    }

    public virtual void UpdateSpriteByBlockType() //스프라이트 이름정해주기
    {
    }

    public virtual void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool bombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if(blockDeco != null)
        {
            blockDeco.SidePang(uniqueIndex, splashColorType, bombEffect);
        }
    }

    public virtual void DestroySelf()
    {

    }

    public virtual string GetSpriteName()
    {
        return "";
    }

    public virtual void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
    }

    public string GetColorTypeString()
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

    /// <summary>
    /// 레인보우 폭탄 사용했을 때, 이펙트 이름 가져오는 함수
    /// </summary>
    public virtual string GetRainbowEffectSpriteName()
    {
        return "";
    }


    public void SetSpriteAlpha(float alphaColor)
    {
        mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, alphaColor);

        if (upLinker != null)upLinker.SetSpriteAlpha(alphaColor);
        if (rightLinker != null)rightLinker.SetSpriteAlpha(alphaColor);
        if (leftLinker != null)leftLinker.SetSpriteAlpha(alphaColor);
        if (DownLinker != null)DownLinker.SetSpriteAlpha(alphaColor);
    }

    public void SetSpriteColor(Color alphaColor)
    {
        mainSprite.color = alphaColor;

        if (upLinker != null) upLinker.SetSpriteColor(alphaColor);
        if (rightLinker != null) rightLinker.SetSpriteColor(alphaColor);
        if (leftLinker != null) leftLinker.SetSpriteColor(alphaColor);
        if (DownLinker != null) DownLinker.SetSpriteColor(alphaColor);
    }


    #region 이동관련

    public virtual void MakeDummyImage(Vector3 tartgetPos)
    {
        //기존더미지우기
        if (mainSprite.customFill.blockRatio != 0 || dummySpriteList.Count > 0)
        {
                mainSprite.customFill.blockRatio = 0;
                SpriteRatio(0);
                if (blockDeco != null) blockDeco.uiSprite.customFill.blockRatio = 0;

                foreach (var dummyImage in dummySpriteList)
                    Destroy(dummyImage.gameObject);

                dummySpriteList.Clear();
        }

        UIBlockSprite dummyMainSprite = NGUITools.AddChild(gameObject, BlockMaker.instance.blockDummySpriteObj).GetComponent<UIBlockSprite>();
        ManagerUIAtlas.CheckAndApplyEventAtlas(dummyMainSprite);
        dummyMainSprite.spriteName = mainSprite.spriteName;
        dummyMainSprite.depth = mainSprite.depth;
        MakePixelPerfect(dummyMainSprite);
        dummyMainSprite.cachedTransform.localPosition = tartgetPos;
        dummySpriteList.Add(dummyMainSprite.customFill);
    }

    public virtual void SpriteRatio(float ratio, float vertRatio = 0)
    {

    }

    public void MoveBlock()
    {
        var board = PosHelper.GetBoard(indexX, indexY);
        
        _stateTimer += Global.deltaTimePuzzle;
        switch (state)
        {
            case BlockState.WAIT:
                _waitCount++;

                if (IsCanMove())
                {
                    if (RestTargetToMove())
                    {
                      //  Destroylinker();
                    }
                    else
                    {
                        if (mainSprite.customFill.blockRatio != 0)
                        {
                            mainSprite.customFill.blockRatio = 0;
                            SpriteRatio(0);

                            foreach (var dummyImage in dummySpriteList)
                                Destroy(dummyImage.gameObject);

                            dummySpriteList.Clear();

                            if (blockDeco != null)
                                blockDeco.uiSprite.customFill.blockRatio = 0;
                        }
                    }

                    //타겟목표에 도착하지않았다면 이동
                    if (targetPos != Vector3.zero && Vector3.Distance(_transform.localPosition, targetPos) > 20f)                    
                        state = BlockState.MOVE;                    
                }
                BlockStop();
                MakeLinkerByWaitState();
                break;
            case BlockState.MOVE:
                if (!IsCanMove())
                {
                    state = BlockState.WAIT;
                    break;
                }
                //MakeLinkerByManager();
                    
                if(board.HasDecoHideBlock())
                {
                    RemoveLinkerNoReset();
                }

                if (Vector3.Distance(_transform.localPosition, targetPos) < 10f)
                {
                    if (RestTargetToMove())
                    {
                        if ((ManagerBlock.instance.stageInfo.reverseMove == 0 && _transform.localPosition.y - rTargetPos.y <= 1f) || (ManagerBlock.instance.stageInfo.reverseMove == 1 && rTargetPos.y - _transform.localPosition.y <= 1f)) // if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                        {
                            mainSprite.customFill.blockRatio = 0;
                            SpriteRatio(0);

                            foreach (var dummyImage in dummySpriteList)
                                Destroy(dummyImage.gameObject);

                            dummySpriteList.Clear();

                            if (blockDeco != null)
                                blockDeco.uiSprite.customFill.blockRatio = 0;
                        }
                    }
                }

                if ((ManagerBlock.instance.stageInfo.reverseMove == 0 && _transform.localPosition.y > targetPos.y) || (ManagerBlock.instance.stageInfo.reverseMove == 1 && targetPos.y > _transform.localPosition.y)) // if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                {
                    //아래로 흘렀을 때
                    isMoveDownThisTurn = true;
                }

                if (Vector3.Distance(_transform.localPosition, targetPos) < 1f)
                {
                    _transform.localPosition = targetPos;
                    state = BlockState.WAIT;                    
                    ManagerBlock.instance.PlayLandAudio();

                    if ((ManagerBlock.instance.stageInfo.reverseMove == 0 && _transform.localPosition.y - rTargetPos.y <= 1f) || (ManagerBlock.instance.stageInfo.reverseMove == 1 && rTargetPos.y - _transform.localPosition.y <= 1f)) // if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                    {
                        mainSprite.customFill.blockRatio = 0;
                        SpriteRatio(0);
                        if (blockDeco != null)
                            blockDeco.uiSprite.customFill.blockRatio = 0;

                    foreach (var dummyImage in dummySpriteList)
                        Destroy(dummyImage.gameObject);

                    dummySpriteList.Clear();
                    }

                    if (onLavaWarring)
                    {
                        if (indexY > GameManager.MinScreenY + 1)
                            OnLavaWarring(false);
                    }
                }
                else
                {
                    int offsetY = 1;
                    if (ManagerBlock.instance.stageInfo.reverseMove == 1) offsetY = -1;

                    _velocity += Global.deltaTimePuzzle * ManagerBlock.NORMAL_SPEED;
                    _velocity = (_velocity > MAX_VELOCITY) ? MAX_VELOCITY : _velocity;

                    float gameTypeVelocity = 1.0f * Global.GameInstance.BlockMoveSpeedRatio();
                    if (GameManager.gameMode == GameMode.ADVENTURE)
                        gameTypeVelocity = 2f;// 1.75f;
                    if (GameManager.gameMode == GameMode.COIN) 
                        gameTypeVelocity = 2f;

                    if (sideMove)
                        _transform.localPosition = Vector3.MoveTowards(_transform.localPosition, targetPos, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * 900f * BLOCK_MOVE_SPEED_RATIO * gameTypeVelocity * 1.414f);
                    else
                        _transform.localPosition = Vector3.MoveTowards(_transform.localPosition, targetPos, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * 900f * BLOCK_MOVE_SPEED_RATIO * gameTypeVelocity);
                    
                    if(mainSprite.customFill.blockRatio != 0)
                    {
                        float distance = _transform.localPosition.y - rTargetPos.y;
                        float tempBlockRatio = distance / ManagerBlock.BLOCK_SIZE;
                        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                        {
                            tempBlockRatio = -tempBlockRatio -1;
                        }

                        mainSprite.customFill.blockRatio = tempBlockRatio;
                        SpriteRatio(tempBlockRatio);
                        if (blockDeco != null)blockDeco.uiSprite.customFill.blockRatio = tempBlockRatio;

                        foreach (var dummyImage in dummySpriteList)
                            dummyImage.blockRatio = -tempBlockRatio;                                                
                        
                        if ((ManagerBlock.instance.stageInfo.reverseMove == 0 && _transform.localPosition.y - rTargetPos.y <= 1f) ||(ManagerBlock.instance.stageInfo.reverseMove == 1 && rTargetPos.y - _transform.localPosition.y  <= 1f)) //Mathf.Abs(distance) <= 1f)
                        {
                            mainSprite.customFill.blockRatio = 0;
                            SpriteRatio(0);
                            if (blockDeco != null) blockDeco.uiSprite.customFill.blockRatio = 0;

                            foreach (var dummyImage in dummySpriteList)
                                Destroy(dummyImage.gameObject);   
                            
                            dummySpriteList.Clear();
                        }
                    }

                }
                SetBlockScaleMove();
                break;
            case BlockState.MOVE_OTHER:
                _velocity = 0.1f;
                float speed = Global.deltaTimePuzzle * 900f * MoveOtherSpeedRatio;
                _transform.localPosition = Vector3.MoveTowards(_transform.localPosition, targetPos, speed);
                if (Vector3.Distance(_transform.localPosition, targetPos) < 1f)
                {
                    _transform.localPosition = targetPos;
                    state = BlockState.WAIT;
                    ManagerBlock.instance.PlayLandAudio();
                }
                SetBlockScaleMove();
                break;
            case BlockState.PANG:

                if (GameManager.gameMode == GameMode.ADVENTURE)
                {
                    if(GameManager.adventureMode == AdventureMode.CURRENT)
                    {
                        BlockStop();
                        MakeLinkerByWaitState();
                    }
                }


                if (!IsSkipPang && !isSkipDistroy)
                {
                    if (normalPangDelay)
                    {
                        if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode != AdventureMode.ORIGIN)
                        {
                            float ratio = (Mathf.Sin(Mathf.PI * _stateTimer * 10)+1)*0.5f;
                            mainSprite.color = Color.Lerp(Color.white, new Color(0.5f, 0.5f, 0.5f, 1), ratio);
                            SetSpriteColor(Color.Lerp(Color.white, new Color(0.5f, 0.5f, 0.5f, 1), ratio));
                        }
                        break;
                    }
                    else
                    {
                        if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode != AdventureMode.ORIGIN)
                        {
                            mainSprite.color = Color.white;
                            SetSpriteColor(Color.white);
                        }
                    }

                    if (_stateTimer >= _pangRemoveDelay)
                    {
                        PangDestroyBoardData();
                    }

                    if (_stateTimer >= _pangAlphaDelay)
                    {
                        if (_pangGatherEffect != Vector3.zero)
                        {
                            float ratio = Mathf.Cos(ManagerBlock.PI90*((_stateTimer - _pangAlphaDelay)*1.5f)/0.4f);

                            _transform.localPosition = Vector3.Lerp(_stageBeforePos, _pangGatherEffect,((_stateTimer - _pangAlphaDelay)*1.5f)/0.4f);

                            if ((_stateTimer - _pangAlphaDelay) * 1.5f / 0.4f > 0.5f)
                            {
                                mainSprite.color = new Color(1f, 1f, 1f,1.5f - (_stateTimer - _pangAlphaDelay)*1.5f/0.4f*1.5f);
                            }
                        }
                        else 
                        {
                            float ratio = Mathf.Cos(Mathf.PI * 0.5f * (_stateTimer - _pangAlphaDelay) / 0.4f);
                            mainSprite.color = new Color(1f, 1f, 1f, ratio);
                        }
                    }
                }
                break;
        }

        BlockJumping();
    }

    void StatePang()
    {
        _stateTimer += Global.deltaTimePuzzle;

        switch (state)
        {
            case BlockState.WAIT:
                _waitCount++;
                break;
            case BlockState.PANG:

                if (!IsSkipPang && !isSkipDistroy)
                {
                    if (_stateTimer >= _pangRemoveDelay)
                    {
                        PangDestroyBoardData();
                    }

                    if (_stateTimer >= _pangAlphaDelay)
                    {
                        if (_pangGatherEffect != Vector3.zero)
                        {
                            float ratio = Mathf.Cos(ManagerBlock.PI90 * ((_stateTimer - _pangAlphaDelay) * 1.5f) / 0.4f);
                            _transform.localPosition = Vector3.Lerp(_stageBeforePos, _pangGatherEffect, ((_stateTimer - _pangAlphaDelay) * 1.5f) / 0.4f);

                            if ((_stateTimer - _pangAlphaDelay) * 1.5f / 0.4f > 0.5f)
                            {
                                mainSprite.color = new Color(1f, 1f, 1f,
                                    1.5f - (_stateTimer - _pangAlphaDelay) * 1.5f / 0.4f * 1.5f);
                            }

                            MakePixelPerfect(mainSprite);
                        }
                        else
                        {
                            float ratio = Mathf.Cos(Mathf.PI * 0.5f * (_stateTimer - _pangAlphaDelay) / 0.4f);
                            mainSprite.color = new Color(1f, 1f, 1f, ratio);
                        }
                    }
                }
                break;
        }
    }

    bool RestTargetToMove()     //이동가능한지 체크 아래가 비었거나 포탈이거나
    {
        if (!ManagerBlock.instance.blockMove)
            return false;

        if (isRainbowBomb)
            return false;

        if (IsExplodeByBomb)
            return false;

        if (normalPangDelay)
            return false;

        if (normalPangDelay)
            return false;

        Board[,] board = ManagerBlock.boards;

        if (board[indexX, indexY].HasDecoCoverBlock()) return false;

        if (!board[indexX, indexY].IsCanMove()) return false;

        if (!board[indexX, indexY].IsActiveBoard) return false;

        if (blockDeco != null && blockDeco.IsInterruptBlockMove() == true) return false;

        if (board[indexX, indexY].direction != BlockDirection.NONE)
        {
             Board targetBoard = PosHelper.GetBoardByDir(indexX, indexY, board[indexX, indexY].direction);
            int addX = targetBoard.indexX - indexX;
            int addY = targetBoard.indexY - indexY;

            if (targetBoard != null && targetBoard.IsCanFill() && ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, addX, addY))
            {
                MoveToDown(addX, addY);
                direction = board[indexX, indexY].direction;
                sideMove = false;
                return true;
            }
            return false;
        }

        if(ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            if (PosHelper.GetBoard(indexX, indexY - 1) != null && board[indexX, indexY - 1].IsCanFill() && ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, 0, -1))
            {
                MoveToDown(0, -1);
                sideMove = false;
                return true;
            }
        }else if (PosHelper.GetBoard(indexX, indexY + 1) != null && board[indexX, indexY + 1].IsCanFill() && ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, 0, 1))
        {
            MoveToDown(0, 1);
            sideMove = false;
            return true;
        }
        
        //포탈, 점프 추가, 포탈찾기 //포탈이동을 블럭에서 처리하는 이유 포탈으로 들어가는 속도를 일정하게 
        if(board[indexX, indexY].IsHasPotal() != null && MoveToPotal(board[indexX, indexY]))
        {
            sideMove = false;
            return true;
        }        

        if (MoveToSide(1) || MoveToSide(-1))    //사이드
        {
            sideMove = true;
            return true;
        }

        return false;           
    }

    public bool IsCanChangePosition(bool checkDisturb, BlockDirection directionDisturb = BlockDirection.UP) //이동 가능한지 체크
    {
        if (IsCanMove() == false)
            return false;

        Board board = PosHelper.GetBoard(indexX, indexY);

        if (board != null)
        {
            if (board.HasDecoCoverBlock()) return false;
            if (!board.IsCanMove()) return false;
            if (!board.IsActiveBoard) return false;

            if(checkDisturb)
            {
                for (int i = 0; i < board.BoardOnDisturbs.Count; i++)
                {
                    if(board.BoardOnDisturbs[i].IsDisturbBoardDirection(directionDisturb, indexX, indexY))
                    {
                        return false;
                    }
                }
            }
        }

        if (blockDeco != null && blockDeco.IsInterruptBlockMove() == true) return false;

        return true;
    }

    //흐름형이 아닌 경우
    public bool IsCanChangePositionWithoutMove(bool checkDisturb, BlockDirection directionDisturb = BlockDirection.UP, bool checkSideLink = true) //이동 가능한지 체크
    {
        Board board = PosHelper.GetBoard(indexX, indexY);

        if (board != null)
        {
            if (board.HasDecoHideBlock()) return false;
            if (board.HasDecoCoverBlock()) return false;
            if (!board.IsCanMove()) return false;
            if (!board.IsActiveBoard) return false;

            if (checkDisturb)
            {
                for (int i = 0; i < board.BoardOnDisturbs.Count; i++)
                {
                    if((checkSideLink == true || !(board.BoardOnDisturbs[i] is SideBlock || board.BoardOnDisturbs[i] is LinkSideBlock))
                        && board.BoardOnDisturbs[i].IsDisturbBoardDirection(directionDisturb, indexX, indexY))
                    {
                        return false;
                    }
                }
            }
        }

        if (blockDeco != null && blockDeco.IsInterruptBlockMove() == true) return false;

        return true;
    }


    bool MoveToSide(int offsetX)
    {
        int offsetY = 1;
        if (ManagerBlock.instance.stageInfo.reverseMove == 1) offsetY = -1;

        Board board = PosHelper.GetBoard(indexX + offsetX, indexY + offsetY);

        if (board == null)
        {
            return false;
        }

        if (!ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, offsetX, 0) &&
            !ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, 0, offsetY))
        {
            return false;
        }

        if (!ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, 0, offsetY) &&
            !ManagerBlock.instance.IsCanMoveSideBlock(indexX + offsetX, indexY, 0, offsetY))
        {
            return false;
        }

        if (!ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY, offsetX, 0) &&
            !ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY + offsetY, offsetX, 0))
        {
            return false;
        }

        if (!ManagerBlock.instance.IsCanMoveSideBlock(indexX + offsetX, indexY, 0, offsetY) &&
            !ManagerBlock.instance.IsCanMoveSideBlock(indexX, indexY + offsetY, offsetX, 0))
        {
            return false;
        }
        

        if (!board.IsCanFill())
        {
            return false;
        }

        if (board.IsStartBoard)
        {
            BlockDirection checkDirection = BlockDirection.UP;
            if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                checkDirection = BlockDirection.DOWN;

            if (board.IsNotDisturbBlock(checkDirection))
                return false;
        }

        if (board.HasDecoCoverBlock())
        {
            return false;
        }

        else if (board.IsHasPotalOut() != null && board.IsHasPotalOut().IsNotExistDisturbDeco()
            && board.IsHasPotalOut().exitPotal != null && board.IsHasPotalOut().exitPotal.IsHasMovableBlock())
        {
            return false;
        }



        int count = -1;
        if (ManagerBlock.instance.stageInfo.reverseMove == 1) count = 1;

        while (true)
        {
            int inX = indexX + offsetX;
            int inY = indexY + offsetY;

            Board tempUpBoard = PosHelper.GetBoard(inX, inY, 0, count);

            if (tempUpBoard == null)
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (!tempUpBoard.IsActiveBoard)
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (tempUpBoard.HasDecoCoverBlock())
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (tempUpBoard.Block != null && tempUpBoard.HasBlockDecoInterruptBlockMove())
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (!ManagerBlock.instance.IsCanMoveSideBlock(tempUpBoard.indexX, tempUpBoard.indexY, 0, offsetY))
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (tempUpBoard.Block != null && !tempUpBoard.Block.IsCanMove() && tempUpBoard.Block.state != BlockState.PANG && tempUpBoard.Block.IsBlockRemoving() == false)
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
            else if (tempUpBoard.Block != null && tempUpBoard.Block.IsCanMove())
            {
                return false;
            }
            else if (tempUpBoard.IsMakerBlock())
            {
                if ((ManagerBlock.instance.stageInfo.reverseMove == 0 && tempUpBoard.IsNotDisturbBlock(BlockDirection.UP)) ||
                   (ManagerBlock.instance.stageInfo.reverseMove == 1 && tempUpBoard.IsNotDisturbBlock(BlockDirection.DOWN)))
                {
                    return false;
                }
            }

            else if (tempUpBoard.IsHasPotalOut() != null && tempUpBoard.IsHasPotalOut().IsNotExistDisturbDeco() 
                && tempUpBoard.IsHasPotalOut().exitPotal != null && tempUpBoard.IsHasPotalOut().exitPotal.IsHasMovableBlock())
            {
                return false;
            }

            if (ManagerBlock.instance.stageInfo.reverseMove == 1) count++;
            else count--;

            if ((count < -9 && ManagerBlock.instance.stageInfo.reverseMove == 0) || (count > 9 && ManagerBlock.instance.stageInfo.reverseMove == 1))
            {
                MoveToDown(offsetX, offsetY);
                return true;
            }
        }

        return false;
    }

    public void MoveToDown(int offsetX, int offsetY)
    {
        Board[,] board = ManagerBlock.boards;
        board[indexX + offsetX, indexY + offsetY].Block = this;
        board[indexX, indexY].Block = null;
        targetPos = PosHelper.GetPosByIndex(indexX + offsetX, indexY + offsetY);
        expectType = BlockBombType.NONE;
        indexX += offsetX;
        indexY += offsetY;

        state = BlockState.MOVE;
    }

    public void MoveToDirWithoutNull(int offsetX, int offsetY)
    {
        Board[,] board = ManagerBlock.boards;
        board[indexX + offsetX, indexY + offsetY].Block = this;
        targetPos = PosHelper.GetPosByIndex(indexX + offsetX, indexY + offsetY);
        expectType = BlockBombType.NONE;
        indexX += offsetX;
        indexY += offsetY;

        state = BlockState.MOVE;
    }

    #region state 변환없이 이동시키는 함수 관련(모드 관계없이 원하는 위치로 이동시킬 때 사용)
    public void MoveBlockToTargetPosition(int offsetX, int offsetY, float speedRatio, bool isExistTargetBlock = true)
    {
        Board[,] board = ManagerBlock.boards;
        board[indexX + offsetX, indexY + offsetY].Block = this;
        //목표가 되는 블럭이 없다면, 이동 전 보드의 Block은 null로 만들어줌.
        if (isExistTargetBlock == false)
            board[indexX, indexY].Block = null;
        targetPos = PosHelper.GetPosByIndex(indexX + offsetX, indexY + offsetY);
        expectType = BlockBombType.NONE;
        indexX += offsetX;
        indexY += offsetY;
        MoveOtherSpeedRatio = speedRatio;
        state = BlockState.MOVE_OTHER;
    }

    //위치 변환 없이 targetPos로 움직이는 모션만 나오도록 하는 함수
    //이동 즉시 삭제할것
    public void MoveToDirWithoutBlockSwitch(int offsetX, int offsetY, float speedRatio)
    {
        targetPos = PosHelper.GetPosByIndex(indexX + offsetX, indexY + offsetY);
        expectType = BlockBombType.NONE;
        MoveOtherSpeedRatio = speedRatio;
        state = BlockState.MOVE_OTHER;
    }
    #endregion

    bool MoveToPotal(Board board)
    {
        int offsetY = 1;

        BlockDirection checkBoardDir = BlockDirection.DOWN;
        BlockDirection checkExitDir = BlockDirection.UP;
        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            offsetY = -1;
            checkBoardDir = BlockDirection.UP;
            checkExitDir = BlockDirection.DOWN;
        }

        if (!board.IsNotDisturbBlock(checkBoardDir)) return false;

        Potal exitPotal = board.IsHasPotal().exitPotal;

        if (exitPotal == null)return false;

        Board exitBoard = PosHelper.GetBoard(exitPotal.inX, exitPotal.inY);
        Board beforeBoard = PosHelper.GetBoard(indexX, indexY);
        
        if (exitBoard == null)return false;
        if (exitBoard.Block != null) return false;
        if (!exitBoard.IsCanFill()) return false;
        if (exitBoard.HasDecoCoverBlock()) return false;
        if (!exitBoard.IsNotDisturbBlock(checkExitDir)) return false;
        if (ManagerBlock.instance._portalPause)return false;

        //TODO 더미블럭만들기   
        UpdateSpriteByBlockType();

        MakeDummyImage(PosHelper.GetPosByIndex(indexX, indexY) - PosHelper.GetPosByIndex(exitPotal.inX, exitPotal.inY - 1* offsetY));
        ManagerBlock.boards[indexX, indexY].Block = null;
        exitBoard.Block = this;        
        _transform.localPosition = PosHelper.GetPosByIndex(exitPotal.inX, exitPotal.inY - 1* offsetY);
        targetPos = PosHelper.GetPosByIndex(exitPotal.inX, exitPotal.inY);
        state = BlockState.MOVE;
        indexX = exitPotal.inX;
        indexY = exitPotal.inY;

        mainSprite.customFill.blockRatio = 1f* offsetY;
        SpriteRatio(1f* offsetY);
        if (blockDeco != null) blockDeco.uiSprite.customFill.blockRatio = 1f * offsetY;

        expectType = BlockBombType.NONE;
        rTargetPos = PosHelper.GetPosByIndex(exitPotal.inX, exitPotal.inY);

        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            mainSprite.customFill.blockRatio = -0.01f;
            SpriteRatio(-0.01f);
            if (blockDeco != null) blockDeco.uiSprite.customFill.blockRatio = -0.01f;
        }
        Destroylinker();

        SetMainSpriteDepth();



        //포탈테스트
        if (_potal == null)
        {
            _potal = beforeBoard.IsHasPotal();
        }
        else if (_potal2 == null && _potal != beforeBoard.IsHasPotal())
        {
            _potal2 = beforeBoard.IsHasPotal();
        }
        else if (_potal == beforeBoard.IsHasPotal() || _potal2 == beforeBoard.IsHasPotal())
        {
            _potalCount++;
            if (_potalCount > 1)
            {
                ManagerBlock.instance._portalPause = true;
                _potal = null;
                _potal2 = null;
                _potalCount = 0;
            }
        }
        else if (_potal != beforeBoard.IsHasPotal() && _potal2 != beforeBoard.IsHasPotal())
        {
            _potal = beforeBoard.IsHasPotal();
            _potal2 = null;
            _potalCount = 0;
        }
        return true;
    }


    //포탈테스트
    private Potal _potal = null;
    private Potal _potal2 = null;
    public int _potalCount = 0;

    public void MoveToTargetPos(Vector3 reTargetPos)
    {
        targetPos = reTargetPos;

        if (GameManager.instance.SkipIngameClear == false)
        {
            state = BlockState.MOVE;
        }
        else
        {
            _transform.localPosition = targetPos;       
            state = BlockState.WAIT;
        }
    }

#endregion

    //블럭을 한번에 제거할 때 사용.
    public virtual void PangDestroyBoardData()
    {
        if(IsRemoveHideDeco_AtBlockPang()) RemoveCloverDeco();
        
        ManagerBlock.boards[indexX, indexY].Block = null;
        ManagerBlock.boards[indexX, indexY].TempBlock = null;
        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

    //블럭을 제거후 null로 만들면 즉시 위에서 흐르기 때문에 제작
    public virtual void PangDestroyBoardDataWithoutNull()
    {
        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

    public virtual void DestroyBlockData(int adType = -1)
    {
        int adjustBlockType = (adType == -1) ? (int)type : adType;
        bool isAdjust = ManagerBlock.instance.stageInfo.listBlockAdjust.FindIndex(x => x == adjustBlockType) != -1 ? true : false;

        if (isAdjust == false || (isAdjust == true && isMakeByStart == true))
        {
            for (int i = 0; i < listAdjustBlock.Count; i++)
            {
                ManagerBlock.instance.liveBlockTypeCount[(int)listAdjustBlock[i]]--;
            }
        }
    }

    #region 효과
    void SetBlockScaleMove()
    {
        if (!IsDepomerBlock()) return;

        float ratio = 0.2f * _velocity / MAX_VELOCITY;
        mainSprite.cachedTransform.localScale = new Vector3(1 - ratio, 1 + ratio, 1);
    }

    void BlockStop()
    {
        if (!IsDepomerBlock()) return;

        if (_stateTimer < 1f && _velocity > 0f)
        {
            isBlockStop_state = true;

            float ratio = Mathf.Sin(_stateTimer * 20f) / Mathf.Exp(_stateTimer * 8f);
            float scale = 1f + 0.2f * ratio;

            if (direction == BlockDirection.NONE || direction == BlockDirection.DOWN)
            {
                _transform.localPosition = targetPos + Vector3.up * 20f * -ratio;
                mainSprite.cachedTransform.localScale = new Vector3(scale * scale, (2f - scale) * scale, 1f);
            }
            else if (direction == BlockDirection.UP)
            {
                _transform.localPosition = targetPos + Vector3.down * 20f * -ratio;
                mainSprite.cachedTransform.localScale = new Vector3(scale * scale, (2f - scale) * scale, 1f);
            }
            else if (direction == BlockDirection.RIGHT)
            {
                _transform.localPosition = targetPos + Vector3.left * 20f * -ratio;
                mainSprite.cachedTransform.localScale = new Vector3((2f - scale) * scale, scale * scale,  1f);
            }
            else if (direction == BlockDirection.LEFT)
            {
                _transform.localPosition = targetPos + Vector3.right * 20f * -ratio;
                mainSprite.cachedTransform.localScale = new Vector3((2f - scale) * scale, scale * scale,  1f);
            }
        }
        else if(_velocity > 0f)
        {
            isBlockStop_state = false;

            _velocity = 0;
            mainSprite.cachedTransform.localScale = Vector3.one;
        }
    }

    //선택한 블럭이 pang되지못할때 효과
    public void DontMatchBlockSelect()
    {
        Board selectBoard = PosHelper.GetBoard(indexX, indexY);
        
        //해당 블럭이 장막 안에 있다면 애니메이션을 재생하지 않도록 예외 처리.
        if (selectBoard.HasDecoHideBlock()) return;
        
        StartCoroutine(DoJigglingScale());
    }

    IEnumerator DoJigglingScale()
    {
        float scaleTimer = 0;
        float timeDecay = 0.2f;
        float scaleAmp = 0.25f;
        float timeScale = 10f;

        while (scaleTimer < 4f)
        {
            float ratio = Mathf.Sin(scaleTimer * ManagerBlock.PI90 * 2) / Mathf.Exp(scaleTimer * timeDecay);
            _transform.localScale = Vector3.one * (1 + ratio * scaleAmp);
            scaleTimer += Global.deltaTimePuzzle * timeScale;
            yield return null;
        }

        _transform.localScale = Vector3.one;
        mainSprite.transform.localPosition = new Vector3(0, 0, 0);
        yield return null;
    }

    //랜덤 상자에서 나올때 효과
    public void ExitByRandomBox(System.Action callback = null)
    {
        StartCoroutine(DoJigglingScale2(callback));
    }

    public IEnumerator DoJigglingScale2(System.Action callback = null)
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.08f);
        transform.DOScale(0.3f, actionTime).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(actionTime);

        //중간호출
        callback?.Invoke();

        actionTime = ManagerBlock.instance.GetIngameTime(0.08f);
        transform.DOScale(1f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);

        transform.localScale = Vector3.one;
        yield return null;
    }

    public void Hide(float tempAlpha = 0f)
    {
        mainSprite.color = new Color(1, 1, 1, tempAlpha);

      //  UISprite[] uisprites = gameObject.GetComponentsInChildren<UISprite>();
       // foreach(var sprite in uisprites)sprite.color = new Color(1, 1, 1, 0.0f);


        if (rightLinker != null)
        {
            rightLinker.SetSpriteAlpha(tempAlpha);
        }
        if (DownLinker != null)
        {
            DownLinker.SetSpriteAlpha(tempAlpha);
        }
    }

    public void Show()
    {
        mainSprite.color = Color.white;

        UISprite[] uisprites = gameObject.GetComponentsInChildren<UISprite>();
        foreach (var sprite in uisprites) sprite.color = Color.white;

        if (rightLinker != null)
        {
            rightLinker.SetSpriteAlpha(1f);
        }
        if (DownLinker != null)
        {
            DownLinker.SetSpriteAlpha(1f);
        }
    }


    public float jumpTimer = 0f;
    UISprite _shadowBody = null;

    public void JumpBlock()
    {
        if (jumpTimer > 0f)
            return;

        if (_shadowBody == null)
        {
            _shadowBody = NGUITools.AddChild(gameObject, mainSprite.gameObject).GetComponent<UISprite>();
        }

        _shadowBody.depth = (int)GimmickDepth.DECO_GROUND;
        mainSprite.depth = (int)GimmickDepth.FX_EFFECTBASE;
        _shadowBody.color = new Color(0f, 0f, 0f, 0.4f);

        jumpTimer = 1f;

        RemoveLinkerNoReset();
    }

    void BlockJumping()
    {
        if (jumpTimer > 0f)
        {
            jumpTimer -= Global.deltaTimePuzzle * 1.2f;

            float ratio = ManagerBlock.instance._curveBlockJump.Evaluate(1f - jumpTimer);      //1.
            mainSprite.cachedTransform.localPosition = Vector3.up * 30f * ratio;               //2.

            float s = Mathf.Sin(Time.time * 20f) / Mathf.Exp((1f - jumpTimer) * 3f);
            mainSprite.cachedTransform.localScale = new Vector3(1f + 0.4f * s, 1f - 0.4f * s, 1f);

            if (_shadowBody != null)
            {
                _shadowBody.cachedTransform.localScale = Vector3.one * (1f - ratio * 0.1f);
                _shadowBody.color = new Color(0f, 0f, 0f, 0.4f);

                if (jumpTimer <= 0f)
                {
                    jumpTimer = 0f;
                    if (inGameItemUse)
                        mainSprite.depth = (int)GimmickDepth.FX_EFFECT;
                    else
                        SetMainSpriteDepth();   //mainSprite.depth =

                    mainSprite.cachedTransform.localPosition = Vector3.zero;
                    MakePixelPerfect(mainSprite);

                    Destroy(_shadowBody.gameObject);
                    _shadowBody = null;
                }
            }
        }
    }

    #endregion

    bool checkSameType(BlockBase checkBlock)
    {
        if(bombType != BlockBombType.RAINBOW && checkBlock.bombType != BlockBombType.RAINBOW)
        {
            if(colorType == checkBlock.colorType)
            {
                return true;
            }
        }

        if(IsBombBlock() && checkBlock.IsBombBlock())
        {
            return true;
        }

        if (type == BlockType.BLOCK_BLACK && checkBlock.type == BlockType.BLOCK_BLACK)
            return true;
        
        return false;
    }

    bool onLavaWarring = false;

    IEnumerator CoWarring()
    {
        while (onLavaWarring)
        {
            mainSprite.color = new Color(0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 1); //mainSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);
            yield return null;
        }
        mainSprite.color = Color.white;
        yield return null;
    }
    public void OnLavaWarring(bool onWarring = true)
    {
        onLavaWarring = onWarring;

        if (upLinker != null) upLinker.OnLavaWarring(onWarring);
        if (rightLinker != null) rightLinker.OnLavaWarring(onWarring);
        if (leftLinker != null) leftLinker.OnLavaWarring(onWarring);
        if (DownLinker != null) DownLinker.OnLavaWarring(onWarring);

        if (onWarring) StartCoroutine(CoWarring());
    }

    bool showMatch = false;
    public void ShowMatchBlock()
    {
        if (showMatch) return;
        showMatch = true;

        StartCoroutine(CoShowMatchBlock());
    }

    IEnumerator CoShowMatchBlock()
    {
        float showTimer = 0f;
        Color _color = Color.white;

        while(showTimer < 2f)
        {
            if (ManagerBlock.instance.state != BlockManagrState.WAIT) break;
            if (GameItemManager.instance != null) break;

            showTimer += Global.deltaTimePuzzle*1.5f;
            float ratioColor = Mathf.Cos(Mathf.PI * showTimer * 2);
            _color = new Color(0.8f + 0.2f * ratioColor, 0.8f + 0.2f*ratioColor, 0.8f + 0.2f * ratioColor,1);

            mainSprite.color = _color;
            if(toyBombSprite != null) toyBombSprite.color = _color;
            if (rightLinker != null) rightLinker.SetSpriteColor(_color);
            if (DownLinker != null) DownLinker.SetSpriteColor(_color);
            yield return null;
        }

        mainSprite.color = Color.white;
        if (toyBombSprite != null) toyBombSprite.color = Color.white;
        if (rightLinker != null) rightLinker.SetSpriteColor(Color.white);
        if (DownLinker != null) DownLinker.SetSpriteColor(Color.white);
        showMatch = false;
        yield return null;
    }

    public virtual IEnumerator CoFlashBlock_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => mainSprite.color, x => mainSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            if (toyBombSprite != null)
                DOTween.To(() => toyBombSprite.color, x => toyBombSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            if (specialEventSprite != null)
                DOTween.To(() => specialEventSprite.color, x => specialEventSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            if (collectBlock_Alphabet != null)
                collectBlock_Alphabet.Action_Flash(targetColor, aTime, 2, LoopType.Yoyo);
            if (rightLinker != null)
                DOTween.To((a) => rightLinker.SetSpriteColor_Float(a), 1f, targetColorValue, aTime).SetLoops(2, LoopType.Yoyo);
            if (DownLinker != null)
                DOTween.To((a) => DownLinker.SetSpriteColor_Float(a), 1f, targetColorValue, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        mainSprite.color = Color.white;
        if (toyBombSprite != null) toyBombSprite.color = Color.white;
        if (specialEventSprite != null) specialEventSprite.color = Color.white;
        if (collectBlock_Alphabet != null) collectBlock_Alphabet.SetCollectBlockImageColor(Color.white);
        if (rightLinker != null) rightLinker.SetSpriteColor(Color.white);
        if (DownLinker != null) DownLinker.SetSpriteColor(Color.white);
        yield return null;
    }

    public virtual IEnumerator CoFlashBlock_Alpha(int actionCount, float actionTIme, float waitTime)
    {
        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To((a) => SetSpriteAlpha(a), 1f, 0f, aTime).SetLoops(2, LoopType.Yoyo);
            if (toyBombSprite != null)
                DOTween.ToAlpha(() => toyBombSprite.color, x => toyBombSprite.color = x, 0, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        SetSpriteAlpha(1);
        if (toyBombSprite != null) toyBombSprite.color = new Color(toyBombSprite.color.r, toyBombSprite.color.g, toyBombSprite.color.b, 1f);
        yield return null;
    }

    public virtual IEnumerator CoSetBlockAlpha(float fromAlpha, float toAlpha, float actionTime)
    {
        float aTime = actionTime * 0.5f;

        DOTween.To((a) => SetSpriteAlpha(a), fromAlpha, toAlpha, aTime);

        yield return new WaitForSeconds(aTime);
    }

    public int BlockScore()
    {
        return ManagerBlock.instance.ScoreNormalBlock;
    }

    //코인추가
    public bool hasCoin = false;
    UISprite coinSprtie = null;
    public void AddCoin(bool hide = false)
    {
        if (hide)
        {
            if (hasCoin)
                return;

            if (coinSprtie != null) Destroy(coinSprtie.gameObject);
            coinSprtie = null;
            return;
        }

        if(coinSprtie == null)
        {
            GameObject coinObj = BlockMaker.instance.MakeBlockCoin(mainSprite.gameObject);
            coinSprtie = coinObj.GetComponent<UISprite>();
        }
        coinSprtie.depth = mainSprite.depth + 4;
        coinSprtie.cachedTransform.localScale = Vector3.one * 0.8f;
        coinSprtie.cachedTransform.localPosition = new Vector3(22, -21, 0);
    }

    //블럭에 붙어있는 오브젝트 모으기 종류의 획득 처리.
    public virtual void GetCollectEventBlock()
    {
        //스페셜 모으기 이벤트.
        GetSpecialEventBlock();

        //알파벳 모으기 이벤트.
        GetAlphabetEventBlock();
    }

    //블럭에 붙어있는 오브젝트 지우기(물이나 용암 등, 블럭이 강제로 삭제되는 타이밍에서 블럭 수 감소 처리)
    public virtual void RemoveCollectEventBlock()
    {
        //알파벳 모으기 이벤트.
        RemoveAlphabetEventBlock();
    }

    #region 스페셜 이벤트 블럭
    public UIBlockUrlTexture specialEventSprite = null;
    public void AddSpecialEventSprite(bool hide = false, bool isMakeStartBoard = true)
    {
        int offsetY = 1;
        if (ManagerBlock.instance.stageInfo.reverseMove == 1) offsetY = -1;

        if (hide)
        {
            if (specialEventSprite != null) Destroy(specialEventSprite.gameObject);
            specialEventSprite = null;
            IsSpecialEventBlock = false;
            return;
        }

        IsSpecialEventBlock = true;

        if (specialEventSprite == null)
        {
            specialEventSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.urlImageObj).GetComponent<UIBlockUrlTexture>();
        }

        specialEventSprite.blockSpriteType = CustomFill.BlockSpriteType.Block;
        specialEventSprite.customFill.blockRatio = (isMakeStartBoard == true) ? (1 * offsetY) : 0;
        specialEventSprite.depth = mainSprite.depth + 2;
        specialEventSprite.SettingTextureScale(57,57);
        specialEventSprite.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
        specialEventSprite.cachedTransform.localPosition = new Vector3(22, -21, 0);
    }

    public virtual void GetSpecialEventBlock()
    {
        if (IsSpecialEventBlock == false)
            return;

        AddSpecialEventSprite();
        InGameEffectMaker.instance.MakeUrlEffect(transform.position, GameUIManager.instance.specialEventObj.transform.position, 
            "sEventBlock_" + Global.specialEventIndex, GetSpecialEffect);
    }

    public virtual void GetSpecialEffect()
    {
        ManagerBlock.instance.getSpecialEventBlock++;
        ManagerBlock.instance.specialEventAppearCount--;
        GameUIManager.instance.RefreshSpecialEvent();
    }

    public virtual void CoverBlockWithCarpet()
    {
        Board tempBoard = PosHelper.GetBoard(indexX, indexY);
        if (tempBoard != null && tempBoard.HasDecoCoverBlock() == false 
            && (blockDeco == null || blockDeco.IsInterruptBlockSelect() == false) 
            && carpetSprite == null && mainSprite != null)
        {
            carpetSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockCarpetSpriteObj).GetComponent<BlockCarpetSprite>();
            carpetSprite.InitCarpetSprite((int)GimmickDepth.BLOCK_BASE + 1);
            hasCarpet = true;
        }
    }
    #endregion

    #region 알파벳 이벤트 블럭
    public CollectBlock_Alphabet collectBlock_Alphabet = null;
    public void AddAlphabetEventBlock(int alphabetIdx, bool isMakeStartBoard, bool isNormalAlphabet = true)
    {
        if (collectBlock_Alphabet == null)
        {
            collectBlock_Alphabet = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.alphabetEventObj).GetComponent<CollectBlock_Alphabet>();
        }

        collectBlock_Alphabet.InitCollectBlock(alphabetIdx, mainSprite.depth + 2, isMakeStartBoard, isNormalAlphabet);
    }

    public void AddAlphabetEventBlock(CollectBlock_Alphabet originData)
    {
        if (collectBlock_Alphabet == null)
        {
            collectBlock_Alphabet = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.alphabetEventObj).GetComponent<CollectBlock_Alphabet>();
        }

        collectBlock_Alphabet.InitCollectBlock(originData);
    }

    public void RemoveAlphabetEventBlock()
    {
        if (collectBlock_Alphabet == null)
            return;

        ManagerAlphabetEvent.alphabetIngame.screenCount_N--;
        Destroy(collectBlock_Alphabet.gameObject);
        collectBlock_Alphabet = null;
    }

    public virtual void GetAlphabetEventBlock(bool isDestroyBlock = false)
    {
        if (collectBlock_Alphabet == null)
            return;

        collectBlock_Alphabet.GainCollectBlock();
        if (isDestroyBlock == true)
        {
            Destroy(collectBlock_Alphabet.gameObject);
            collectBlock_Alphabet = null;
            return;
        }
    }
    #endregion
    
    public void ShakingBlock()
    {
        StartCoroutine(CoShakeBlock());
    }

    IEnumerator CoShakeBlock()
    {
        float factor = 1f;
        Vector3 value = Vector3.zero;

        while (factor > 0)
        {
            factor -= Global.deltaTimePuzzle;
            if (factor <= 1f)
            {
                float result = Mathf.Sin(2.0f * 3.14159f * factor * 7f) * 6f;
                value.x = result * (factor) / 1f;
                float result1 = Mathf.Sin(2.0f * 3.14159f * factor * 6f + 0.4f) * 5f;
                value.y = result1 * (factor) / 1f;
            }
            mainSprite.cachedTransform.localPosition = value;
            yield return null;
        }

        mainSprite.cachedTransform.localPosition = Vector3.zero;
        yield return null;
    }

    #region 벌집기믹 우선순위 관련된 함수
    //벌집기믹에서 사용하는 우선순위 계산하는 함수
    public virtual FireWorkRank GetBlockRankAtFireWork(Board board, bool fireworkHasCarpet = false)
    {
        if (IsBlockRankExclusionTypeAtFireWork() == true)
            return FireWorkRank.RANK_NONE;

        FireWorkRank blockRank = GetDefaultBlockRankAtFireWork();

        #region 1순위 검사
        //우선순위가 1순위인 경우, 다른 검사 안하고 우선순위 반환.
        if (blockRank == FireWorkRank.RANK_1)
            return blockRank;

        //석판, 카펫관련 우선순위 검사.
        if (board.BoardOnCrack.Count > 0)
            return FireWorkRank.RANK_1;
        if (ManagerBlock.instance.isCarpetStage && fireworkHasCarpet && board.IsExistCarpet() == false)
            return FireWorkRank.RANK_1;

        //석상 목표를 달성하지 않은 상태일 때, 잔디의 우선순위 1순위.
        if (board.BoardOnGrass.Count > 0)
        {
            if(ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.STATUE))
                return FireWorkRank.RANK_1;
        }
        #endregion

        #region 얼음검사
        if (IsCanCoverIce() == true && blockDeco != null)
        {
            //얼음이 목표인 경우 검사
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.ICE))
                return FireWorkRank.RANK_1;

            //현재 블록의 우선순위의 값이 얼음의 우선순위보다 큰 경우 얼음 우선순위를 반환해줌.
            if (blockRank > FireWorkRank.RANK_3)
                blockRank = FireWorkRank.RANK_3;
        }
        #endregion

        #region 열쇠, 우주선 검사
        //열쇠, 우주선 검사, 이미 기본 목표 순위가 2순위보다 높거나 같을 경우에는 검사하지 않음.
        if (IsBlockCheckKeyTypeAtFireWork() == true && blockRank > FireWorkRank.RANK_2)
        {
            if (IsCheckTypeExistUpBoardsAtFireWork(TARGET_TYPE.KEY, BlockType.KEY) == true)
                return FireWorkRank.RANK_2;

            if (IsCheckTypeExistUpBoardsAtFireWork(TARGET_TYPE.SPACESHIP, BlockType.SPACESHIP) == true)
                return FireWorkRank.RANK_2;
        }
        #endregion

        //모든 조건 해당하지 않으면, 블럭의 기본 우선순위 반환.
        return blockRank;
    }

    public virtual FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (IsBlockRankExclusionTypeAtFireWork() == true)
            return FireWorkRank.RANK_NONE;
        return FireWorkRank.RANK_1;
    }

    //벌집 우선순위 대상 제외 기믹인지.
    public virtual bool IsBlockRankExclusionTypeAtFireWork()
    {
        return false;
    }

    //열쇠,우주선 아래 체크하는 블럭인지.
    public virtual bool IsBlockCheckKeyTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        return false;
    }

    //벌집 우선순위 검사할 때, 해당타입이 현재 블록 위쪽에 존재하는지 검사
    protected bool IsCheckTypeExistUpBoardsAtFireWork(TARGET_TYPE targetType, BlockType blockType)
    {
        if (ManagerBlock.instance.HasAchievedCollectTarget(targetType) == false)
            return false;

        if ((indexY - 1) > GameManager.MIN_Y)
        {
            for (int i = (indexY - 1); i > GameManager.MIN_Y; i--)
            {
                Board checkBoard = PosHelper.GetBoardSreeen(indexX, i);
                BlockBase checkBlock = PosHelper.GetBlockScreen(indexX, i);

                if (checkBoard == null || checkBoard.IsActiveBoard == false)
                    break;

                if (checkBlock == null)
                    break;

                //위쪽에 해당타입 있으면 바로 반환.
                if (checkBlock.type == blockType)
                    return true;

                //위쪽에 벌집으로 칠 수 있는 블럭이 있으면 더 이상 검사하지 않음.
                if (checkBlock.IsBlockRankExclusionTypeAtFireWork() == false)
                    break;
            }
        }
        return false;
    }
    #endregion

    #region 자동매치관련

    public bool showShakeBombEffect = false;
    public virtual void BombShakeEffect(int dir) //대각선 위오른쪽부터 4,5,6,7,
    {
        if (state != BlockState.WAIT) return;
        if (showShakeBombEffect) return;

        showShakeBombEffect = true;

        StartCoroutine(CoBombShake(dir));
    }

    IEnumerator CoBombShake(int dir)
    {
        int dirX = 0;
        int dirY = 0;

        if(dir == 0 || dir == 4 || dir == 7)
        {
            dirY = 1;
        }
        if (dir == 1 || dir == 4 || dir == 5)
        {
            dirX = 1;
        }
        if (dir == 2 || dir == 5 || dir == 6)
        {
            dirY = -1;
        }
        if (dir == 3 || dir == 6 || dir == 7)
        {
            dirX = -1;
        }

        float Shaketimer = 0;

        while (Shaketimer < 1)
        {
            if(Shaketimer > 0.5f)
            Shaketimer += Global.deltaTimePuzzle*2;
            else
                Shaketimer += Global.deltaTimePuzzle * 8;

            float ratio = 8 * (1 - Mathf.Cos(2*Shaketimer * Mathf.PI)) * 0.5f;//          Mathf.Sin(Shaketimer * ManagerBlock.PI90);// / Mathf.Exp(Shaketimer);

            mainSprite.cachedTransform.localPosition = new Vector3(ratio*dirX, ratio* dirY, 0);
            
            yield return null;
        }

        mainSprite.cachedTransform.localPosition = Vector3.zero;
        showShakeBombEffect = false;
        yield return null;
    }

    #endregion

    /// <summary>
    /// 다른 블럭으로 변하기 전, 연출
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator CoActionChangeBlock_Destroy()
    {
        this.expectType = BlockBombType.NONE;
        isDestroyLink = true;
        Destroylinker();

        MakeBlockShadowImage();

        float actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        float targetY = mainSprite.transform.localPosition.y + 30f;
        mainSprite.transform.DOLocalMoveY(targetY, actionTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(actionTime);
    }

    /// <summary>
    /// 장막기믹이 사라지고 밑에 있는 블럭들의 기능을 변경이 필요하면 사용.
    /// </summary>
    public virtual void HideDecoRemoveAction()
    {
        
    }

    public virtual IEnumerator CoActionChangeBlock_Appear()
    {
        MakeBlockShadowImage();
        float targetY = mainSprite.transform.localPosition.y;
        mainSprite.transform.localPosition += Vector3.up * 30f;

        InGameEffectMaker.instance.MakeLastBomb(this._transform.position);
        ManagerSound.AudioPlayMany(AudioInGame.GET_STATUE);

        float actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        mainSprite.transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        _transform.localScale = Vector3.one;
        Destroy(_shadowBody.gameObject);
        _shadowBody = null;

        mainSprite.transform.DOLocalMoveY(targetY, actionTime).SetEase(ManagerBlock.instance._curveTreeScaleIn);

        actionTime = ManagerBlock.instance.GetIngameTime(0.5f);
        mainSprite.transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        SetMainSpriteDepth();
    }

    private void MakeBlockShadowImage()
    {
        if (_shadowBody == null)
        {
            _shadowBody = NGUITools.AddChild(gameObject, mainSprite.gameObject).GetComponent<UISprite>();
            _shadowBody.color = new Color(0f, 0f, 0f, 0.4f);
            _shadowBody.depth = (int)GimmickDepth.DECO_GROUND; 
            mainSprite.depth += 25;
            if (specialEventSprite != null)
                specialEventSprite.depth = mainSprite.depth + 2;
            if (collectBlock_Alphabet != null)
                collectBlock_Alphabet.SetDepth(mainSprite.depth + 2);
        }
    }

    protected void MakePixelPerfect(UISprite sprite, float offset = 1.25f)
    {
        sprite.MakePixelPerfect();
        sprite.width = Mathf.RoundToInt(sprite.width * offset);
        sprite.height = Mathf.RoundToInt(sprite.height * offset);
    }

    public void AddAdjutBlockType(BlockType addAdjustType)
    {
        if (addAdjustType != BlockType.NONE && listAdjustBlock.FindIndex(x => x == addAdjustType) == -1)
        {
            listAdjustBlock.Add(addAdjustType);
        }
    }
    
    public void UseColorBrush(BlockDirection dir)
    {
        var blockBase = GetListCanChangeBlock(dir);
        if (blockBase != null)
        {
            StartCoroutine(CoChangeBlockColor(blockBase));
        }

        BlockBase GetListCanChangeBlock(BlockDirection dir)
        {
            //블럭의 컬러 변화를 막는 데코가 있다면 검사하지 않음.
            var tempBoard = PosHelper.GetBoardByDir(indexX, indexY, dir);
            if (tempBoard != null && tempBoard.DecoOnBoard.FindIndex(x => x.IsInterruptBlockColorChange() == true) == -1)
            {
                var tempBlock = PosHelper.GetBlockScreenByDir(indexX, indexY, dir);
                if (tempBlock != null && tempBlock.type == BlockType.NORMAL && tempBlock.IsNormalBlock() && tempBlock.blockDeco == null && tempBlock.state != BlockState.PANG)
                {
                    return tempBlock;
                }
            }

            return null;
        }

        IEnumerator CoChangeBlockColor(BlockBase tempBlock)
        {
            InGameEffectMaker.instance.MakeEffectFlySimpleColorChangeEffect(tempBlock._transform.localPosition, colorType);
            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.15f)); //이펙트 생성 후 자연스럽게 컬러값이 변경될 수 있도록 딜레이 추가.
            tempBlock.colorType = colorType;
            tempBlock.RemoveLinkerNoReset();
            tempBlock.MakeLinkerByManager();
        }
    }
}
