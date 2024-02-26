using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAreaInfo
{
    public int indexX = 0;
    public int indexY = 0;

    //경계 방향들을 가지고 있는 리스트.
    public List<BlockDirection> areaDirList = new List<BlockDirection>();

    public BombAreaInfo(int indexX, int indexY, List<BlockDirection> areaDir)
    {
        this.indexX = indexX;
        this.indexY = indexY;
        for (int i = 0; i < areaDir.Count; i++)
        {
            this.areaDirList.Add(areaDir[i]);
        }
    }
}

public class BlockBomb : MonoBehaviour
{
    [System.NonSerialized]
    public BlockBombType type = BlockBombType.NONE;

    [System.NonSerialized] public int indexY = 0;
    [System.NonSerialized] public int indexX = 0;

    [System.NonSerialized]
    public List<BlockBase> rainbowPangList = new List<BlockBase>();

    [System.NonSerialized]
    public int _uniqueIndex = 0;

    [System.NonSerialized]
    public BlockBase _block = null;

    [System.NonSerialized]
    public BlockColorType colorType = BlockColorType.NONE;

    [System.NonSerialized]
    static public int _liveCount = 0;

    [System.NonSerialized]
    static public int _bombUniqueIndex = 100000;

    float bombTimer = -1;

    readonly float BOMB_DESTROY_TIME = 0.5f;
    readonly float RAINBOW_DESTROY_TIME = 1f;
    readonly float CLEAR_BOMB_DESTROY_TIME = 0.15f;

    //구분
    public bool isSecendBomb = false;

    [System.NonSerialized]
    public bool pangByNow = false;

    //카운트 고정
    public bool isFixBombUniIndex = false;

    //고정팡   //다이나마트에서 사용, 다이나마이트일때 구분함 //화분에서 다이나마이트일때 중복 적용에 사용되고있음 주의!!!!!!
    public int isFixPangIndex = -1;
    public int DynamiteBombToFlowerPotPangIndex = -1;   //화분일때 한번만 적용하기 사용

    private bool hasCarpet = false;

    public enum BombShape
    {
        Rect,   // 사각형 모양 폭발 : 둥근, 라인, 둥근x라인, 째깍 ... 등
        Cross,  // 십자모양 폭발 : 검댕이, 클리어폭탄 ... 등
        RectCross   // 사각형모양 + 테두리 중간이 십자 모양으로 튀어나온 폭발 : 둥근x 둥근.
    }

    void Start()
    {
        if (isFixPangIndex == -1)
        {
            if (!isFixBombUniIndex) _bombUniqueIndex++;
            _uniqueIndex = _bombUniqueIndex;
        }
        else
        {
            _uniqueIndex = isFixPangIndex;
        }

        bombTimer = 0;

        if (_block != null && _block.bombHasCarpet == true)
        {
            MakeCarpet(PosHelper.GetBoardSreeen(indexX, indexY));
        }

        hasCarpet = CheckExistCarpetAtTargetBoard(PosHelper.GetBoardSreeen(indexX, indexY));
        if(hasCarpet == true)
            _block.CoverBlockWithCarpet();

        switch (type)
        {
            case BlockBombType.LINE_H:
            case BlockBombType.R_LINE_H:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangLine(1, 0));
                break;
            case BlockBombType.R_LINE_V:
            case BlockBombType.LINE_V:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangLine(0, 1));
                break;
            case BlockBombType.BOMB:
                StartCoroutine(PangCircleBomb());
                break;
            case BlockBombType.RAINBOW:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangRainbowBomb());
                break;
            case BlockBombType.RAINBOW_X_RAINBOW:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangRainbowXRainbow2());
                break;
            case BlockBombType.RAINBOW_X_BOMB:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangRainbowXBomb2());                
                break;
            case BlockBombType.RAINBOW_X_LINE:
                BoardDisturbPang(indexX, indexY);
                StartCoroutine(PangRainBowXLine2());
                break;
            case BlockBombType.BOMB_X_BOMB:
                if(isSecendBomb)StartCoroutine(PangBombXBomb());
                else StartCoroutine(PangBombXBombFirst());
                break;
            case BlockBombType.BOMB_X_LINE:
                StartCoroutine(PangBombXLine());
                break;
            case BlockBombType.LINE_X_LINE:
                StartCoroutine(PangLineXLine());
                break;
            case BlockBombType.HALF_BOMB:
                StartCoroutine(PangBomb());
                break;
            case BlockBombType.CLEAR_BOMB:
                StartCoroutine(PangClearBomb());
                break;
            case BlockBombType.BLACK_BOMB:
                StartCoroutine(PangBlackBomb());
                break;
            case BlockBombType.CROSS_LINE:
                StartCoroutine(PangLineXLine());
                break;
            case BlockBombType.ADVENTURE_BOMB:
                StartCoroutine(PangAdventure());
                break;
            case BlockBombType.POWER_BOMB:
                StartCoroutine(PangPowerBomb());
                break;
            case BlockBombType.RAINBOW_X_BOMB_WITHSELF:
                StartCoroutine(PangRainbowXBomb2(true));
                break;
            case BlockBombType.DUMMY:
                break;
        }
    }

    void OnDestroy()
    {
        _liveCount--;
    }

    IEnumerator PangAdventure()
    {
        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_BOMB);

        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        Board checkBoard = PosHelper.GetBoardSreeen(indexX, indexY, 0, 0);
        bool[] bDirExtension = new bool[4] { true, true, true, true };
        BlockDirection[] directions
            = new BlockDirection[4] { BlockDirection.RIGHT, BlockDirection.LEFT, BlockDirection.DOWN, BlockDirection.UP };

        //해당 방향으로 이동 가능한지 여부를 배열 형태로 가져옴.
        bDirExtension = GetExtensionCrossdirection(checkBoard, indexX, indexY, directions, true);

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.RectCross, indexX, indexY, 5, 5);

        //십자 방향으로 확장되면서 터지도록.
        if (bDirExtension[0] == true)
            PangToDirection(indexX, indexY, 1, 0, 3, 0.3f, infoList, true);
        if (bDirExtension[1] == true)
            PangToDirection(indexX, indexY, -1, 0, 3, 0.3f, infoList, true);
        if (bDirExtension[2] == true)
            PangToDirection(indexX, indexY, 0, 1, 3, 0.3f, infoList, true);
        if (bDirExtension[3] == true)
            PangToDirection(indexX, indexY, 0, -1, 3, 0.3f, infoList, true);

        //대각 방향으로 확장되면서 터지도록.
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, 1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, -1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, -1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, 1, 2, 0.3f, infoList, true);

        blockShakeEffect4X4();

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangLineXLine()
    {
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.LINE_X_LINE, _uniqueIndex);

        ManagerSound.AudioPlay(AudioInGame.PANG_LINE);

        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHLeft, false, true, 0); //PosHelper.GetPosByIndex(indexX,indexY)
        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHRight, false, true, 1);

        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVDown, false, true, 2);
        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVUp, false, true, 3);

        float timer = 0f;
        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 10, 3));
        StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1,10, 3));

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangBlackBomb()
    {
        ManagerSound.AudioPlay(AudioInGame.PANG_BOMB);

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Cross, indexX, indexY, 3, 3);
        StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0, infoList, true));
        StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0, infoList, true));

        //블럭포지는효과
        blockShakeEffect1X1();

        while (bombTimer < CLEAR_BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangClearBomb()
    {
        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Cross, indexX, indexY, 3, 3);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.CLEAR_BOMB, _infoList: infoList, unique:_uniqueIndex);

        StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0, infoList, true));
        StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0, infoList, true));

        //블럭포지는효과
        //if (GameManager.instance.SkipIngameClear == false)
        {
            ManagerSound.AudioPlay(AudioInGame.PANG_BOMB);

            blockShakeEffect1X1();

            InGameEffectMaker.instance.MakelastBombPang(_block._transform, 0.0f);

            while (bombTimer < CLEAR_BOMB_DESTROY_TIME)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        Destroy(gameObject);
        yield return null;
    }
    
    void blockShakeEffect1X1()
    {
        if (PosHelper.GetBlockScreen(indexX, indexY, 0, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, -2).BombShakeEffect(0);
        if (PosHelper.GetBlockScreen(indexX, indexY, 1, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, -1).BombShakeEffect(4);

        if (PosHelper.GetBlockScreen(indexX, indexY, 2, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 0).BombShakeEffect(1);
        if (PosHelper.GetBlockScreen(indexX, indexY, 1, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, 1).BombShakeEffect(5);

        if (PosHelper.GetBlockScreen(indexX, indexY, 0, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, 2).BombShakeEffect(2);
        if (PosHelper.GetBlockScreen(indexX, indexY, -1, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, 1).BombShakeEffect(6);

        if (PosHelper.GetBlockScreen(indexX, indexY, -2, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 0).BombShakeEffect(3);
        if (PosHelper.GetBlockScreen(indexX, indexY, -1, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, -1).BombShakeEffect(7);
    }

    void blockShakeEffect3X3()
    {
        if (PosHelper.GetBlockScreen(indexX, indexY, -1, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, -2).BombShakeEffect(0);
        if (PosHelper.GetBlockScreen(indexX, indexY, 0, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, -2).BombShakeEffect(0);
        if (PosHelper.GetBlockScreen(indexX, indexY, 1, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, -2).BombShakeEffect(0);
        
        if (PosHelper.GetBlockScreen(indexX, indexY, 2, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, -2).BombShakeEffect(4);

        if (PosHelper.GetBlockScreen(indexX, indexY, 2, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, -1).BombShakeEffect(1);
        if (PosHelper.GetBlockScreen(indexX, indexY, 2, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 0).BombShakeEffect(1);
        if (PosHelper.GetBlockScreen(indexX, indexY, 2, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 1).BombShakeEffect(1);

        if (PosHelper.GetBlockScreen(indexX, indexY, 2, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 2).BombShakeEffect(5);

        if (PosHelper.GetBlockScreen(indexX, indexY, 1, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, 2).BombShakeEffect(2);
        if (PosHelper.GetBlockScreen(indexX, indexY, 0, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, 2).BombShakeEffect(2);
        if (PosHelper.GetBlockScreen(indexX, indexY, -1, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, 2).BombShakeEffect(2);

        if (PosHelper.GetBlockScreen(indexX, indexY, -2, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 2).BombShakeEffect(6);

        if (PosHelper.GetBlockScreen(indexX, indexY, -2, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 1).BombShakeEffect(3);
        if (PosHelper.GetBlockScreen(indexX, indexY, -2, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 0).BombShakeEffect(3);
        if (PosHelper.GetBlockScreen(indexX, indexY, -2, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, -1).BombShakeEffect(3);

        if (PosHelper.GetBlockScreen(indexX, indexY, -2, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, -2).BombShakeEffect(7);
    }

    void blockShakeEffect4X4()
    {
        if (PosHelper.GetBlockScreen(indexX, indexY, -2, -3) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, -3).BombShakeEffect(0);
        if (PosHelper.GetBlockScreen(indexX, indexY, -1, -3) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, -3).BombShakeEffect(0);

        if (PosHelper.GetBlockScreen(indexX, indexY, 0, -4) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, -4).BombShakeEffect(0);

        if (PosHelper.GetBlockScreen(indexX, indexY, 1, -3) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, -3).BombShakeEffect(0);
        if (PosHelper.GetBlockScreen(indexX, indexY, 2, -3) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, -3).BombShakeEffect(0);

        //if (PosHelper.GetBlockScreen(indexX, indexY, 2, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, -2).BombShakeEffect(4);

        if (PosHelper.GetBlockScreen(indexX, indexY, 3, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, 3, -2).BombShakeEffect(1);
        if (PosHelper.GetBlockScreen(indexX, indexY, 3, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, 3, -1).BombShakeEffect(1);

        if (PosHelper.GetBlockScreen(indexX, indexY, 4, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, 4, 0).BombShakeEffect(1);

        if (PosHelper.GetBlockScreen(indexX, indexY, 3, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, 3, 1).BombShakeEffect(1);
        if (PosHelper.GetBlockScreen(indexX, indexY, 3, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 3, 2).BombShakeEffect(1);

        // if (PosHelper.GetBlockScreen(indexX, indexY, 2, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 2).BombShakeEffect(5);

        if (PosHelper.GetBlockScreen(indexX, indexY, 2, 3) != null) PosHelper.GetBlockScreen(indexX, indexY, 2, 3).BombShakeEffect(2);
        if (PosHelper.GetBlockScreen(indexX, indexY, 1, 3) != null) PosHelper.GetBlockScreen(indexX, indexY, 1, 3).BombShakeEffect(2);

        if (PosHelper.GetBlockScreen(indexX, indexY, 0, 4) != null) PosHelper.GetBlockScreen(indexX, indexY, 0, 4).BombShakeEffect(2);

        if (PosHelper.GetBlockScreen(indexX, indexY, -1, 3) != null) PosHelper.GetBlockScreen(indexX, indexY, -1, 3).BombShakeEffect(2);
        if (PosHelper.GetBlockScreen(indexX, indexY, -2, 3) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 3).BombShakeEffect(2);

        //if (PosHelper.GetBlockScreen(indexX, indexY, -2, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, 2).BombShakeEffect(6);

        if (PosHelper.GetBlockScreen(indexX, indexY, -3, 2) != null) PosHelper.GetBlockScreen(indexX, indexY, -3, 2).BombShakeEffect(3);
        if (PosHelper.GetBlockScreen(indexX, indexY, -3, 1) != null) PosHelper.GetBlockScreen(indexX, indexY, -3, 1).BombShakeEffect(3);

        if (PosHelper.GetBlockScreen(indexX, indexY, -4, 0) != null) PosHelper.GetBlockScreen(indexX, indexY, -4, 0).BombShakeEffect(3);

        if (PosHelper.GetBlockScreen(indexX, indexY, -3, -1) != null) PosHelper.GetBlockScreen(indexX, indexY, -3, -1).BombShakeEffect(3);
        if (PosHelper.GetBlockScreen(indexX, indexY, -3, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, -3, -2).BombShakeEffect(3);

        //if (PosHelper.GetBlockScreen(indexX, indexY, -2, -2) != null) PosHelper.GetBlockScreen(indexX, indexY, -2, -2).BombShakeEffect(7);
    }

    IEnumerator PangBombXLine()
    {
        ManagerBlock.instance.blockMove = false;
        ManagerBlock.instance.creatBlock = false;

        float waitTimer = 0;
        while (waitTimer < 0.1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_LINE_BOMB);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: _uniqueIndex);
        InGameEffectMaker.instance.MakeEffectBombL(transform.position, 1.2f);

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 3);

        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 1, 0, 1, infoList, true, _uniqueIndex);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, -1, 0, 1, infoList, true, _uniqueIndex);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 1, 1, infoList, true, _uniqueIndex);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, -1, 1, infoList, true, _uniqueIndex);

        PangToDirection(indexX, indexY, 1, 0, 1, 0.03f, infoList, true);
        PangToDirection(indexX, indexY, -1, 0, 1, 0.03f, infoList, true);
        PangToDirection(indexX, indexY, 0, 1, 1, 0.03f, infoList, true);
        PangToDirection(indexX, indexY, 0, -1, 1, 0.03f, infoList, true);

        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, 1, 1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, -1, -1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, 1, -1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, -1, 1, 1, infoList, true, _uniqueIndex);

        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, 1, 1, 0.03f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, -1, 1, 0.03f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, -1, 1, 0.03f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, 1, 1, 0.03f, infoList, true);

        waitTimer = 0;
        while (waitTimer < 0.1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_LINE);

        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHLeft, true, opacity: 0.5f); //
        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHRight, true, opacity: 0.5f);

        float timer = 0f;
        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        List<BombAreaInfo> infoListH = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 20, 3);

        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.DOWN) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX, indexY + 1, 1, 0, 10, infoListH, true, _uniqueIndex, indexX, indexY);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, 0, 10, null, true, _uniqueIndex);
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.UP) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX, indexY - 1, 1, 0, 10, infoListH, true, _uniqueIndex, indexX, indexY);

        //현재 블럭이 터진 위치에서 위, 아래 방해하는 블럭이 없으면 해당 x 라인으로 블럭터짐.
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.DOWN) == false)
            StartCoroutine(CoPangToDirection(indexX, indexY + 1, 1, 0, 10, 3, 0.03f, infoListH, true));
        StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 10, 0, 0.03f, null, true));
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.UP) == false)
            StartCoroutine(CoPangToDirection(indexX, indexY - 1, 1, 0, 10, 1, 0.03f, infoListH, true));

        waitTimer = 0;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_LINE);

        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVDown, true, opacity: 0.5f);
        InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVUp, true, opacity: 0.5f);

        List<BombAreaInfo> infoListV_Effect = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 20);

        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.RIGHT) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX + 1, indexY, 0, 1, 10, infoListV_Effect, true, _uniqueIndex, indexX, indexY, dTime: 0.15f);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX, indexY, 0, 1, 10, null, true, _uniqueIndex, dTime: 0.15f);
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.LEFT) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_BothLine(indexX - 1, indexY, 0, 1, 10, infoListV_Effect, true, _uniqueIndex, indexX, indexY, dTime: 0.15f);
     
        timer = 0f;
        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        List<BombAreaInfo> infoListV = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 20);
        //현재 블럭이 터진 위치에서 좌, 우 방해하는 블럭이 없으면 해당 y 라인으로 블럭터짐.
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.RIGHT) == false)
            StartCoroutine(CoPangToDirection(indexX + 1, indexY, 0, 1, 10, 1, 0.03f, infoListV, true));
        StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 10, 0, 0.03f, null, true));
        if (ManagerBlock.instance.bCheckDisturbPang(indexX, indexY, 0, 0, BlockDirection.LEFT) == false)
            StartCoroutine(CoPangToDirection(indexX - 1, indexY, 0, 1, 10, 3, 0.03f, infoListV, true));

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        StartCoroutine(CoDestroyBomb());
    }

    IEnumerator PangBombXBomb()
    {
        Board checkBoard = PosHelper.GetBoardSreeen(indexX, indexY, 0, 0);

        //폭탄 영역 이펙트 출력
        ShowEffectPangFieldBombXBomb(checkBoard);

        _block._pangAlphaDelay = 0.2f;
        _block._pangRemoveDelay = 0.5f;

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_BOMB);

        InGameEffectMaker.instance.MakeBombXBombEffect(_block._transform.position, true);

        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        bool[] bDirExtension = new bool[4] { true, true, true, true };
        BlockDirection[] directions
            = new BlockDirection[4] { BlockDirection.RIGHT, BlockDirection.LEFT, BlockDirection.DOWN, BlockDirection.UP };

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.RectCross, indexX, indexY, 5, 5);

        //해당 방향으로 이동 가능한지 여부를 배열 형태로 가져옴.
        bDirExtension = GetExtensionCrossdirection(checkBoard, indexX, indexY, directions, true);

        //십자 방향으로 확장되면서 터지도록.
        if (bDirExtension[0] == true)
            PangToDirection(indexX, indexY, 1, 0, 3, 0.3f, infoList, true);
        if (bDirExtension[1] == true)
            PangToDirection(indexX, indexY, -1, 0, 3, 0.3f, infoList, true);
        if (bDirExtension[2] == true)
            PangToDirection(indexX, indexY, 0, 1, 3, 0.3f, infoList, true);
        if (bDirExtension[3] == true)
            PangToDirection(indexX, indexY, 0, -1, 3, 0.3f, infoList, true);

        //대각 방향으로 확장되면서 터지도록.
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, 1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, -1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, -1, 2, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, 1, 2, 0.3f, infoList, true);
        blockShakeEffect4X4();

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangBombXBombFirst()
    {
        Board checkBoard = PosHelper.GetBoardSreeen(indexX, indexY, 0, 0);

        //폭탄 영역 이펙트 출력
        ShowEffectPangFieldBombXBombFirst(checkBoard);

        _block._pangAlphaDelay = 0.2f;
        _block._pangRemoveDelay = 0.5f;

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.PANG_BOMB);
        InGameEffectMaker.instance.MakeBombXBombEffect(_block._transform.position, false);

        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        bool[] bDirExtension = new bool[4] { true, true, true, true };
        BlockDirection[] directions
            = new BlockDirection[4] { BlockDirection.RIGHT, BlockDirection.LEFT, BlockDirection.DOWN, BlockDirection.UP };

        //해당 방향으로 이동 가능한지 여부를 배열 형태로 가져옴.
        bDirExtension = GetExtensionCrossdirection(checkBoard, indexX, indexY, directions, true);

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.RectCross, indexX, indexY, 3, 3);

        //십자 방향으로 확장되면서 터지도록.
        if (bDirExtension[0] == true)
            PangToDirection(indexX, indexY, 1, 0, 2, 0.3f, infoList, true);
        if (bDirExtension[1] == true)
            PangToDirection(indexX, indexY, -1, 0, 2, 0.3f, infoList, true);
        if (bDirExtension[2] == true)
            PangToDirection(indexX, indexY, 0, 1, 2, 0.3f, infoList, true);
        if (bDirExtension[3] == true)
            PangToDirection(indexX, indexY, 0, -1, 2, 0.3f, infoList, true);

        //대각 방향으로 확장되면서 터지도록.
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, 1, 1, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, -1, 1, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            SearchDiagonalBlock(indexX, indexY, 1, -1, 1, 0.3f, infoList, true);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            SearchDiagonalBlock(indexX, indexY, -1, 1, 1, 0.3f, infoList, true);

        blockShakeEffect4X4();

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    #region 둥근x둥근 영역 연출 관련 함수
    private void ShowEffectPangFieldBombXBombFirst(Board checkBoard)
    {
        bool[] bDirExtension = new bool[4] { true, true, true, true };
        BlockDirection[] directions
            = new BlockDirection[4] { BlockDirection.RIGHT, BlockDirection.LEFT, BlockDirection.DOWN, BlockDirection.UP };

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.RectCross, indexX, indexY, 3, 3);

        //해당 방향으로 이동 가능한지 여부를 배열 형태로 가져옴.
        bDirExtension = GetExtensionCrossdirection(checkBoard, indexX, indexY, directions, true, false);
        
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: _uniqueIndex);

        //십자 방향으로 확장되면서 터지도록.
        if (bDirExtension[0] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 1, 0, 2, infoList, true, _uniqueIndex);
        if (bDirExtension[1] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, -1, 0, 2, infoList, true, _uniqueIndex);
        if (bDirExtension[2] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 1, 2, infoList, true, _uniqueIndex);
        if (bDirExtension[3] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, -1, 2, infoList, true, _uniqueIndex);

        //대각 방향으로 확장되면서 터지도록.
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 1, 1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, -1, -1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 1, -1, 1, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, -1, 1, 1, infoList, true, _uniqueIndex);
    }

    private void ShowEffectPangFieldBombXBomb(Board checkBoard)
    {
        bool[] bDirExtension = new bool[4] { true, true, true, true };
        BlockDirection[] directions
            = new BlockDirection[4] { BlockDirection.RIGHT, BlockDirection.LEFT, BlockDirection.DOWN, BlockDirection.UP };

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.RectCross, indexX, indexY, 5, 5);

        //해당 방향으로 이동 가능한지 여부를 배열 형태로 가져옴.
        bDirExtension = GetExtensionCrossdirection(checkBoard, indexX, indexY, directions, true, false);

        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: _uniqueIndex);

        //십자 방향으로 확장
        if (bDirExtension[0] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 1, 0, 3, infoList, true, _uniqueIndex);
        if (bDirExtension[1] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, -1, 0, 3, infoList, true, _uniqueIndex);
        if (bDirExtension[2] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 1, 3, infoList, true, _uniqueIndex);
        if (bDirExtension[3] == true)
            InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, -1, 3, infoList, true, _uniqueIndex);

        //대각 방향으로 확장
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, 1, 1, 2, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, -1, -1, 2, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, 1, -1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, 1, -1, 2, infoList, true, _uniqueIndex);
        if (ManagerBlock.instance.IsDiagonalDisturbPang(indexX, indexY, -1, 1) == false)
            InGameEffectMaker.instance.MakePangFieldEffectDiagonalBlock(indexX, indexY, -1, 1, 2, infoList, true, _uniqueIndex);
    }
    #endregion

    private bool[] GetExtensionCrossdirection(Board checkBoard, int indexX, int indexY, BlockDirection[] directions, bool bNoneDirection, bool isDecoPang = true)
    {
        //현재 검사하고자 하는 방향의 수.
        int checkCnt = directions.Length;

        //확장되는 지 여부를 가지고 있는 bool 배열 생성.
        bool[] bExtension = new bool[checkCnt];
        for (int i = 0; i < checkCnt; i++)
        {
            bExtension[i] = true;
        }

        if (checkBoard != null)
        {
            //검사하는 방향으로 터질 수 있는 지 검사.
            for (int i = 0; i < checkBoard.BoardOnDisturbs.Count; i++)
            {
                IDisturb disturb = checkBoard.BoardOnDisturbs[i];
                for (int j = 0; j < checkCnt; j++)
                {
                    if (bExtension[j] == true && ManagerBlock.instance.IsCanExtensionBombDirection(checkBoard, disturb, indexX, indexY, directions[j]) == false)
                    {
                        bExtension[j] = false;
                    }
                }
            }

            //실제 데코를 제거할건지 여부에 따라 데코 제거 처리.
            if (isDecoPang == true)
            {
                //해당 위치에 있는 방향을 확인해서 터지는 데코들에 대한 처리.
                for (int k = 0; k < checkCnt; k++)
                {
                    SetDirectionDecoPang(checkBoard, directions[k], null, bNoneDirection);
                }
            }
        }
        return bExtension;
    }

    IEnumerator RainbowPangSameLine(int lineY)
    {
        rainbowPangCount++;

        float waitTimer = 0;
        List<BlockBase> listBlock = new List<BlockBase>();
        List<BlockBase> listBlock2 = new List<BlockBase>();

        int sameTypeCount = 0;
        for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
        {
            BlockBase block = PosHelper.GetBlockScreen(j, lineY);
            Board board = PosHelper.GetBoardSreeen(j, lineY);
            if (block != null && (block.type == BlockType.START_Rainbow || block.type == BlockType.START_Line || block.type == BlockType.START_Bomb ||
                block.type == BlockType.NORMAL || block.type == BlockType.BLOCK_DYNAMITE || block.type == BlockType.FIRE_WORK))// block.IsNormalBlock())//
            {
                bool isCanPangByRainbow = true;
                foreach (var deco in board.DecoOnBoard)
                {
                    if (deco.IsDisturbPangByRainbowBomb() == false)
                    {
                        isCanPangByRainbow = false;
                        break;
                    }
                }

                if (isCanPangByRainbow == true)
                {
                    listBlock.Add(block);
                    sameTypeCount++;
                }
            }
        }
        yield return null;

        if (sameTypeCount > 1)
        {
            int beforeCount = 0;
            listBlock2.Add(listBlock[0]);
            while(true)
            {
                int randomNext = GameManager.instance.GetIngameRandom(0, 5);
                beforeCount += randomNext;

                if (beforeCount >= listBlock.Count)
                {
                    break;
                }
                listBlock2.Add(listBlock[beforeCount]);
            }

            foreach(var tempBlock in listBlock2)
            {
                listBlock.Remove(tempBlock);
            }

            for (int j = 0; j < 10; j++)
            {
                if (hasCarpet)
                {
                    if (j < listBlock.Count)
                        listBlock[j].CoverBlockWithCarpet();
                    if (j < listBlock2.Count)
                        listBlock2[j].CoverBlockWithCarpet();
                }

                if (j < listBlock.Count - 1)
                {
                    if (listBlock[j] != null && listBlock[j + 1] != null)
                    {   
                        InGameEffectMaker.instance.MakeRainbowLine2(listBlock[j]._transform.position, listBlock[j + 1]._transform.position, listBlock[j + 1], _uniqueIndex, BlockBombType.NONE, true, hasCarpet); 
                    }
                }

                if (j < listBlock2.Count - 1)
                {
                    if (listBlock2[j] != null && listBlock2[j + 1] != null)
                    {
                        InGameEffectMaker.instance.MakeRainbowLine2(listBlock2[j]._transform.position, listBlock2[j + 1]._transform.position, listBlock2[j + 1], _uniqueIndex, BlockBombType.NONE, true, hasCarpet); 
                    }
                }

                waitTimer = 0;
                while (waitTimer < 0.08f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }
        else if (sameTypeCount == 1)
        {
            if (listBlock[0])
            {
                if (hasCarpet)
                {
                    listBlock[0].CoverBlockWithCarpet();
                }
                InGameEffectMaker.instance.MakeRainbowLine3(listBlock[0]._transform.position, listBlock[0], _uniqueIndex, BlockBombType.NONE, true, hasCarpet);
                listBlock.RemoveAt(0);
            }
        }

        foreach (var tempBlock in listBlock2)
        {
            listBlock.Add(tempBlock);
        }


        while (listBlock.Count > 0)
        {
            waitTimer = 0f;
            while (waitTimer < 0.01f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            int index = 0;

            BlockBase block = listBlock[index];
            listBlock.Remove(block);
            Board board = PosHelper.GetBoardSreeen(block.indexX, block.indexY);
            if (hasCarpet && block.hasCarpet == true)
            {
                MakeCarpet(board);
            }

            block._pangRemoveDelay = 0.3f;
            block.IsSkipPang = false;
            block.isRainbowBomb = false;
            yield return null;
            block.BlockPang(_uniqueIndex, BlockColorType.NONE, false);

            if (listBlock.Count <= 0)
            {
                break;
            }
            yield return null;
        }

        rainbowPangCount--;
        yield return null;
    }

    IEnumerator PangRainbowXRainbow2()
    {
        ManagerBlock.instance.waitBlockToFinishMoving = true;

        ManagerSound.AudioPlay(AudioInGame.PANG_RAINBOW);
        ManagerBlock.instance.blockMove = false;
        ManagerBlock.instance.creatBlock = false;
        GetAllBlockInScreen();

        GameObject circleObj = null;
        if(_block != null)
         circleObj =InGameEffectMaker.instance.MakeRainbowCircleEffect(_block._transform.position);

        yield return null;
        
        bombTimer = 0;
        while (bombTimer < 0.2f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        GameObject circleObj2 = InGameEffectMaker.instance.MakeRainbowCircleEffect2(_block._transform.position);

        bombTimer = 0;
        while (bombTimer < 0.3f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            StartCoroutine(RainbowPangSameLine(i));
            bombTimer = 0;
            while (bombTimer < 0.1f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        bombTimer = 0;
        while (bombTimer < 0.4f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        _block.isSkipDistroy = false;
        Destroy(circleObj);
        Destroy(circleObj2);

        while (rainbowPangCount > 0)
        {
            bombTimer = 0;
            while (bombTimer < 0.1f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        ManagerBlock.instance.waitBlockToFinishMoving = false;
        StartCoroutine(CoDestroyBomb());
    }

    IEnumerator PangRainbowXBomb2(bool ingameRainbowStick = false)
    {
        ManagerBlock.instance.blockMove = false;
        ManagerBlock.instance.creatBlock = false;

        //ManagerBlock.instance.blockMove = false;
        ManagerSound.AudioPlay(AudioInGame.PANG_RAINBOW);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique:_uniqueIndex);
        yield return null;

        GameObject circleObj = InGameEffectMaker.instance.MakeRainbowCircleEffect(_block._transform.position);

        rainbowPangList.Clear();
        yield return null;
        GetColorBlockInScreenFromCenter();
        yield return null;


        bombTimer = 0;
        while (bombTimer < 0.2f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        GameObject circleObj2 = InGameEffectMaker.instance.MakeRainbowCircleEffect2(_block._transform.position);

        bombTimer = 0;
        while (bombTimer < 0.3f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        if(ingameRainbowStick == true)
        {
            //4번째 인게임아이템인 경우 자기 자리도 레인보우 폭탄 효과 추가
            _block.state = BlockState.WAIT;
            InGameEffectMaker.instance.MakeRainbowLine3(_block._transform.position, _block._transform.position, _block, _block, _uniqueIndex, BlockBombType.HALF_BOMB, 0f, hasCarpet);

            bombTimer = 0;
            while (bombTimer < 0.02f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        float waitBombTimer = 0f;
        for (int i = 0; i < rainbowPangList.Count; i++)
        {
            if (rainbowPangList[i] != null)
            {
                waitBombTimer++;
                InGameEffectMaker.instance.MakeRainbowLine3(_block._transform.position, rainbowPangList[i]._transform.position, rainbowPangList[i], _block, _uniqueIndex, BlockBombType.HALF_BOMB, i * 0.02f, hasCarpet, ingameRainbowStick);
                 
                bombTimer = 0;
                while (bombTimer < 0.02f)
                {
                    bombTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }
        waitBombTimer = waitBombTimer * 0.04f;

        bombTimer = 0;
        _block.isSkipDistroy = false;
        while (bombTimer < BOMB_DESTROY_TIME)//BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        Destroy(circleObj);
        Destroy(circleObj2);

        bombTimer = 0;
        while (bombTimer < 1 + 0.1f + waitBombTimer- BOMB_DESTROY_TIME)//BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        StartCoroutine(CoDestroyBomb());
    }

    IEnumerator PangRainBowXLine2()
    {
        ManagerBlock.instance.blockMove = false;
        ManagerBlock.instance.creatBlock = false;
        
        ManagerSound.AudioPlay(AudioInGame.PANG_RAINBOW);
        yield return null;

        GameObject circleObj = InGameEffectMaker.instance.MakeRainbowCircleEffect(_block._transform.position);

        rainbowPangList.Clear();
        yield return null;
        GetColorBlockInScreenFromCenter();
        yield return null;


        bombTimer = 0;
        while (bombTimer < 0.2f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        GameObject circleObj2 = InGameEffectMaker.instance.MakeRainbowCircleEffect2(_block._transform.position);

        bombTimer = 0;
        while (bombTimer < 0.3f)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        float waitBombTimer = 0f;
        for (int i = 0; i < rainbowPangList.Count; i++)
        {
            if (rainbowPangList[i] != null)
            {
                waitBombTimer++;

                int randLine = GameManager.instance.GetIngameRandom(0, 2);
                BlockBombType tempLine = randLine == 0 ? BlockBombType.R_LINE_H : BlockBombType.R_LINE_V;

                InGameEffectMaker.instance.MakeRainbowLine3(_block._transform.position, rainbowPangList[i]._transform.position, rainbowPangList[i], _block, _uniqueIndex, tempLine, i * 0.03f, hasCarpet); //BlockBombType.LINE

                bombTimer = 0;
                while (bombTimer < 0.02f)
                {
                    bombTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }

        }
        waitBombTimer = waitBombTimer * 0.03f;

        bombTimer = 0;
        _block.isSkipDistroy = false;

        while (bombTimer < BOMB_DESTROY_TIME)//BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(circleObj);
        Destroy(circleObj2);

        bombTimer = 0;
        while (bombTimer < 1 +  waitBombTimer - BOMB_DESTROY_TIME)//BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        StartCoroutine(CoDestroyBomb());
    }

    IEnumerator PangLine(int offX, int offY)
    {
        GameObject lineA = null;
        GameObject lineB = null;

        _block._pangAlphaDelay = 0.0f;
        _block._pangRemoveDelay = 0.5f;

        float timer = 0f;

        //if (GameManager.instance.SkipIngameClear == false)
        {
            InGameEffectMaker.instance.MakeLineTargetEffect(_block._transform.position);
            InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.LINE, offX, offY, unique:_uniqueIndex);

            if (offX == 1 && offY == 0)
            {
               lineA = InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHLeft, opacity: 0.5f).gameObject;
               lineB = InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eHRight, opacity: 0.5f).gameObject;
            }
            else if (offX == 0 && offY == 1)
            {
               lineA = InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVDown, opacity: 0.5f).gameObject;
               lineB = InGameEffectMaker.instance.MakeEffectLine(transform.position, EEffectBombLine.eVUp, opacity: 0.5f).gameObject;
            }

            while (timer < 0.2f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            ManagerSound.AudioPlayMany(AudioInGame.PANG_LINE);
        }

        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        //if (GameManager.instance.SkipIngameClear == false)
        {
            StartCoroutine(CoPangToDirection(indexX, indexY, offX, offY, 10, 3));

            if (offY == 0)
            {
                Board UpbackA = PosHelper.GetBoardSreeen(indexX, indexY, 0, -1);
                if (UpbackA != null && UpbackA.Block != null) UpbackA.Block.BombShakeEffect(0);

                Board DownbackA = PosHelper.GetBoardSreeen(indexX, indexY, 0, 1);
                if (DownbackA != null && DownbackA.Block != null) DownbackA.Block.BombShakeEffect(2);


                Board UpbackB = PosHelper.GetBoardSreeen(indexX, indexY, 0, -1);
                Board DownbackB = PosHelper.GetBoardSreeen(indexX, indexY, 0, 1);
                if (UpbackB != null && UpbackB.Block != null) UpbackB.Block.BombShakeEffect(0);
                if (DownbackB != null && DownbackB.Block != null) DownbackB.Block.BombShakeEffect(2);
            }

            if (offX == 0)
            {
                Board UpbackA = PosHelper.GetBoardSreeen(indexX, indexY, 1, 0);
                Board DownbackA = PosHelper.GetBoardSreeen(indexX, indexY, -1, 0);
                if (UpbackA != null && UpbackA.Block != null) UpbackA.Block.BombShakeEffect(1);
                if (DownbackA != null && DownbackA.Block != null) DownbackA.Block.BombShakeEffect(3);

                Board UpbackB = PosHelper.GetBoardSreeen(indexX, indexY, 1, -0);
                Board DownbackB = PosHelper.GetBoardSreeen(indexX, indexY, -1, -0);
                if (UpbackB != null && UpbackB.Block != null) UpbackB.Block.BombShakeEffect(1);
                if (DownbackB != null && DownbackB.Block != null) DownbackB.Block.BombShakeEffect(3);
            }

        }
        /*else
        {
            StartCoroutine(CoPangToDirection(indexX, indexY, offX, offY, 10, 3, 0f));
            Destroy(lineA);
            Destroy(lineB);
        }*/


        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangCircleBomb(float waitTime = 0.3f)
    {
        //폭탄 영역 이펙트 출력
        ShowEffectPangFieldCircle();

        _block._pangAlphaDelay = 0.2f;
        _block._pangRemoveDelay = 0.5f;

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 3);
        //if (GameManager.instance.SkipIngameClear == false)
        {
            ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);

            if (isSecendBomb)
                InGameEffectMaker.instance.MakeEffectCircleBomb2(_block._transform, 0.0f);
            else
                InGameEffectMaker.instance.MakeEffectCircleBomb(_block._transform, 0.0f);

            timer = 0f;
            while (timer < 0.1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 1, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, -1, 1, 0, 0.03f, infoList, true));

            blockShakeEffect3X3();
        }
        /*else
        {
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 1, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, -1, 1, 0, 0f, infoList, true));
        }*/

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator PangBomb(float waitTime = 0.3f)
    {
        //폭탄 영역 이펙트 출력
        ShowEffectPangFieldHalfCircle();
        
        //if (GameManager.instance.SkipIngameClear == false)
        {
            ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);
            InGameEffectMaker.instance.MakeEffectCircleBomb2(_block._transform, 0.0f);
        }

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 3);

        //if (GameManager.instance.SkipIngameClear == false)
        {
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 1, 1, 0, 0.03f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, -1, 1, 0, 0.03f, infoList, true));

            blockShakeEffect3X3();
        }
        /*else
        {
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 0, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 0, 1, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, 1, 1, 0, 0f, infoList, true));
            StartCoroutine(CoPangToDirection(indexX, indexY, 1, -1, 1, 0, 0f, infoList, true));
        }*/

        while (bombTimer < BOMB_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }

    #region 둥근 폭탄영역 연출 관련
    private void ShowEffectPangFieldCircle()
    {
        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, 3, 3);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.BOMB, _infoList: infoList, bNoneDirection: true, unique: _uniqueIndex);
    }

    private void ShowEffectPangFieldHalfCircle()
    {
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.BOMB, _uniqueIndex);
    }
    #endregion

    IEnumerator PangPowerBomb()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        if (board == null)
            yield break;

        //폭탄 영역
        int offset = 3;
        int area = offset / 2;
        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BombShape.Rect, indexX, indexY, offset, offset);

        //폭탄 영역 표시
        InGameEffectMaker.instance.MakePangFieldEffect_PowerBomb(indexX - area, indexY - area, indexX + area, indexY + area, infoList, _uniqueIndex, colorType, dTime : 0.08f);

        //폭탄 효과
        StartCoroutine(CoPangPowerBomb(indexX - area, indexY - area, indexX + area, indexY + area, board.IsExistCarpet(), infoList));

        //사운드 및 이펙트
        //if (GameManager.instance.SkipIngameClear == false)
        {
            ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);
            InGameEffectMaker.instance.MakeEffectCircleBomb2(_block._transform, 0.0f);
            blockShakeEffect3X3();
            yield return new WaitForSeconds(BOMB_DESTROY_TIME);
        }
        Destroy(gameObject);
        yield return null;
    }

    IEnumerator CoPangPowerBomb(int minX, int minY, int maxX, int maxY, bool isExistCarpet, List<BombAreaInfo> infoList)
    {
        //범위 내에 있는 블럭들 제거
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                Board checkBoard = PosHelper.GetBoardSreeen(i, j);
                if (checkBoard == null)
                    continue;

                //방해블럭 제거
                SetDirectionDecoPang(checkBoard, BlockDirection.UP, infoList, true);
                SetDirectionDecoPang(checkBoard, BlockDirection.DOWN, infoList, true);
                SetDirectionDecoPang(checkBoard, BlockDirection.RIGHT, infoList, true);
                SetDirectionDecoPang(checkBoard, BlockDirection.LEFT, infoList, true);

                //카펫 설치
                if (isExistCarpet == true)
                    MakeCarpet(checkBoard);

                //잡기돌/물, 얼음 및 블럭 제거
                SetRemoveAllBlock(checkBoard);
            }
        }
        yield return null;
    }

    int rainbowPangCount = 0;

    IEnumerator PangRainbowBomb()
    {
        GameObject circleObj = null;
        GameObject circleObj2 = null;

        //if (GameManager.instance.SkipIngameClear == false)
        {
            ManagerSound.AudioPlay(AudioInGame.PANG_RAINBOW);
            circleObj = InGameEffectMaker.instance.MakeRainbowCircleEffect(_block._transform.position);
        }

        rainbowPangList.Clear();
        yield return null;

        GetColorBlockInScreenFromCenter();
        yield return null;

        //if (GameManager.instance.SkipIngameClear == false)
        {
            bombTimer = 0;
            while (bombTimer < 0.2f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
            circleObj2 = InGameEffectMaker.instance.MakeRainbowCircleEffect2(_block._transform.position);

            bombTimer = 0;
            while (bombTimer < 0.3f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        for (int i = 0; i < rainbowPangList.Count; i++)
        {
            if (rainbowPangList[i] == null) continue;

            InGameEffectMaker.instance.MakeRainbowLine3(_block._transform.position, rainbowPangList[i]._transform.position, rainbowPangList[i], _block, _uniqueIndex, BlockBombType.NONE, 0f, hasCarpet);
                                    
            bombTimer = 0;
            while (bombTimer < 0.02f)
            {
                bombTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            yield return null;
        }

        bombTimer = 0;
        _block.isSkipDistroy = false;

        while (bombTimer < RAINBOW_DESTROY_TIME)
        {
            bombTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(circleObj);
        Destroy(circleObj2);

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator CoPangToDirection(int inX, int inY, int offX, int offY, int count = 10, int effect = 0, float waitTime = 0.030f, List<BombAreaInfo> infoList = null, bool bNoneDirection = false)
    {
        int offsetX = offX;
        int offsetY = offY;

        bool upBombExtend = true;
        bool downBombExtend = true;

        bool upHasCarpet = hasCarpet;
        bool downHasCarpet = hasCarpet;

        BlockDirection upDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);
        BlockDirection downDirection = BlockDirection.NONE;

        if (upDirection == BlockDirection.RIGHT)
            downDirection = BlockDirection.LEFT;
        else if (upDirection == BlockDirection.LEFT)
            downDirection = BlockDirection.RIGHT;
        else if (upDirection == BlockDirection.DOWN)
            downDirection = BlockDirection.UP;
        else if (upDirection == BlockDirection.UP)
            downDirection = BlockDirection.DOWN;

        //대각선 방향의 블럭을 검사하는 경우, 주위에 블럭 팡을 방해하는 데코가 없는지 검사함.
        if (offsetX != 0 && offsetY != 0)
        {
            if (ManagerBlock.instance.IsDiagonalDisturbPang(inX, inY, offsetX, offsetY) == true)
            {
                upBombExtend = false;
            }
            if (ManagerBlock.instance.IsDiagonalDisturbPang(inX, inY, -offsetX, -offsetY) == true)
            {
                downBombExtend = false;
            }
        }
        else
        {   // 현재 폭탄이 향하는 방향과 반대 방향에 방해블럭이 있으면, 해당 방향으로는 폭탄 영역 확장안되도록 함.
            Board checkBoard = PosHelper.GetBoardSreeen(inX, inY, 0, 0);

            if (checkBoard != null)
            {
                for (int i = 0; i < checkBoard.BoardOnDisturbs.Count; i++)
                {
                    IDisturb disturb = checkBoard.BoardOnDisturbs[i];
                    if (upBombExtend == true && 
                        ManagerBlock.instance.IsCanExtensionBombDirection(checkBoard, disturb, inX, inY, upDirection) == false)
                    {
                        upBombExtend = false;   
                    }

                    if (downBombExtend == true &&
                        ManagerBlock.instance.IsCanExtensionBombDirection(checkBoard, disturb, inX, inY, downDirection) == false)
                    {
                        downBombExtend = false;   
                    }
                }
                SetDirectionDecoPang(checkBoard, upDirection, infoList, bNoneDirection);
                SetDirectionDecoPang(checkBoard, downDirection, infoList, bNoneDirection);
            }
        }

        for (int i = 0; i < count; i++)
        {
            float tempTimer = 0;

            Board Upback = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);
            Board Downback = PosHelper.GetBoardSreeen(inX, inY, -offsetX, -offsetY);

            if (Upback != null && upBombExtend)
            {
                if (upHasCarpet)
                {
                    MakeCarpet(Upback); 
                }
                else
                    upHasCarpet = CheckExistCarpetAtTargetBoard(Upback);

                //현재 폭발 방향 따라서 블럭과, 보드 주변을 막고있는 방해블럭 있으면 삭제해줌.
                upBombExtend = SetRemoveBlock(Upback, upDirection);
                if (upBombExtend == true)
                {
                    for (int j = 0; j < Upback.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = Upback.BoardOnDisturbs[j];
                        if (upBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(Upback, disturb, Upback.indexX, Upback.indexY, upDirection) == false)
                            upBombExtend = false;
                    }
                }
                SetDirectionDecoPang(Upback, upDirection, infoList, bNoneDirection);
            }

            if (Downback != null && downBombExtend)
            {
                if (downHasCarpet)
                {
                    MakeCarpet(Downback); 
                }
                else
                    downHasCarpet = CheckExistCarpetAtTargetBoard(Downback);

                downBombExtend = SetRemoveBlock(Downback, downDirection);
                if (downBombExtend == true)
                {
                    for (int j = 0; j < Downback.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = Downback.BoardOnDisturbs[j];
                        if (downBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(Downback, disturb, Downback.indexX, Downback.indexY, downDirection) == false)
                            downBombExtend = false;
                    }
                }
                SetDirectionDecoPang(Downback, downDirection, infoList, bNoneDirection);
            }

            if (offsetY == 0)
            {
                if (upBombExtend)
                {
                    Board UpbackA = PosHelper.GetBoardSreeen(inX, inY, offsetX, -1);
                    if (UpbackA != null && UpbackA.Block != null && (effect == 1 || effect == 3)) UpbackA.Block.BombShakeEffect(0);

                    Board DownbackA = PosHelper.GetBoardSreeen(inX, inY, offsetX, 1);
                    if (DownbackA != null && DownbackA.Block != null && (effect == 2 || effect == 3)) DownbackA.Block.BombShakeEffect(2);
                }
                if (downBombExtend)
                {
                    Board UpbackB = PosHelper.GetBoardSreeen(inX, inY, -offsetX, -1);
                    Board DownbackB = PosHelper.GetBoardSreeen(inX, inY, -offsetX, 1);
                    if (UpbackB != null && UpbackB.Block != null && (effect == 1 || effect == 3)) UpbackB.Block.BombShakeEffect(0);
                    if (DownbackB != null && DownbackB.Block != null && (effect == 2 || effect == 3)) DownbackB.Block.BombShakeEffect(2);
                }
            }

            if (offsetX == 0)
            {
                if (upBombExtend )
                {
                    Board UpbackA = PosHelper.GetBoardSreeen(inX, inY, 1, offsetY);
                    Board DownbackA = PosHelper.GetBoardSreeen(inX, inY, -1, offsetY);
                    if (UpbackA != null && UpbackA.Block != null && (effect == 1 || effect == 3)) UpbackA.Block.BombShakeEffect(1);
                    if (DownbackA != null && DownbackA.Block != null && (effect == 2 || effect == 3)) DownbackA.Block.BombShakeEffect(3);
                }
                if (downBombExtend)
                {
                    Board UpbackB = PosHelper.GetBoardSreeen(inX, inY, 1, -offsetY);
                    Board DownbackB = PosHelper.GetBoardSreeen(inX, inY, -1, -offsetY);
                    if (UpbackB != null && UpbackB.Block != null && (effect == 1 || effect == 3)) UpbackB.Block.BombShakeEffect(1);
                    if (DownbackB != null && DownbackB.Block != null && (effect == 2 || effect == 3)) DownbackB.Block.BombShakeEffect(3);
                }
            }

            offsetX += offX;
            offsetY += offY;


            while (tempTimer < waitTime)
            {
                tempTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        yield return null;
    }

    void SetRemoveAllBlock(Board board)
    {
        
        //장막류 기믹이 있을 경우 해당 데코 삭제
        if (board.HasDecoHideBlock(true, _uniqueIndex, colorType))
            return;
        
        //잡기돌, 물 등이 있을 경우 해당 데코 삭제
        if (board.HasDecoCoverBlock(true, _uniqueIndex, colorType) == true)
            return;

        //검사하고자 하는 블럭이 없으면 검사 안 함.
        BlockBase block = board.Block;
        if (block == null)
            return;

        //얼음같은 데코 제거
        if (board.Block.blockDeco != null && board.Block.blockDeco.IsInterruptBlockSelect())
        {
            board.Block.blockDeco.DecoPang(_uniqueIndex, colorType);
            return;
        }

        //블럭 터지기 전 처리
        if (block.IsBombBlock() && block.IsSkipPang == false && block.state != BlockState.PANG
            && block.IsExplodeByBomb == false && pangByNow == false && block.pangIndex != _uniqueIndex)
        {
            board.Block.IsExplodeByBomb = true; //폭탄일때 한템포 늦게 터는지는 설정
            board.Block.pangIndex = _uniqueIndex;
            BlockBase.ExplodeBombCount++;

            //콤보 설정
            /*
            if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode == AdventureMode.ORIGIN
                && block.bombType != BlockBombType.DUMMY)
            {
                if (board.Block.isRainbowBomb == false && board.Block.secendBomb == false && board.Block.bombType != BlockBombType.HALF_BOMB)
                {
                    ManagerBlock.instance.comboCount++;
                    InGameEffectMaker.instance.MakeAdventureCombo(board.Block._transform.position, "Bomb\nCombo " + ManagerBlock.instance.comboCount.ToString());// + new Vector3(0, -0.06f, 0)
                }
                else if (board.Block.isRainbowBomb == true && board.Block.secendBomb == false && type == BlockBombType.RAINBOW_X_RAINBOW)
                {
                    ManagerBlock.instance.comboCount++;
                    InGameEffectMaker.instance.MakeAdventureCombo(board.Block._transform.position, "Bomb\nCombo " + ManagerBlock.instance.comboCount.ToString());// + new Vector3(0, -0.06f, 0)
                }
            }
            else
            */
            {
                ManagerBlock.instance.comboCount++;
                InGameEffectMaker.instance.MakeCombo(board.Block._transform.position, "Combo " + ManagerBlock.instance.comboCount.ToString());
            }
        }
        else
        {
            InGameEffectMaker.instance.MakeScore(board.Block._transform.position, 80);
        }
        
        bool isHideDeco = false;
        
        //큰 화단
        if (block.type == BlockType.ColorBigJewel)
        {
            if(isFixPangIndex != -1)
            {
                if (DynamiteBombToFlowerPotPangIndex == -1)
                {
                    _bombUniqueIndex++;
                    DynamiteBombToFlowerPotPangIndex = _bombUniqueIndex;
                }

                block.BlockPang(DynamiteBombToFlowerPotPangIndex, colorType, true, true);
                return;
            }
            
            var ColorBigJewel = board.Block as BlockColorBigJewel;

            for (int i = 0; i < ColorBigJewel.blockBoardList.Count; i++)
            {
                if (ColorBigJewel.blockBoardList[i].DecoOnBoard
                    .Find((DecoBoard) => DecoBoard.boardDecoOrder == BoardDecoOrder.CLOVER))
                {
                    isHideDeco = true;
                    board.HasDecoHideBlock(true, _uniqueIndex, colorType);
                }
            }
        }

        ManagerBlock.instance.AddScore(80);
        
        if(!isHideDeco)
            block.BlockPang(_uniqueIndex, colorType, true, true);
    }

    bool SetRemoveBlock(Board board, BlockDirection bombDirection)
    {
        bool bCanPangDirection = true;
        
        //블럭이 터질수 있는지 검사.
        foreach (DecoBase boardDeco in board.DecoOnBoard)
        {
            if (bCanPangDirection == true)
            {
                bCanPangDirection = boardDeco.IsCanPangBlockBoardIndex(board.indexX, board.indexY, bombDirection);
            }
        }

        if (bCanPangDirection == true && board.Block != null)
        {
            if ((board.Block.IsBombBlock()) &&
                board.Block.IsSkipPang == false &&
                board.Block.state != BlockState.PANG &&
                board.Block.IsExplodeByBomb == false &&
                pangByNow == false)//|| board.Block.type == BlockType.BLOCK_BLACK
            {
                if (board.HasDecoHideBlock() == false && board.HasDecoCoverBlock() == false && board.Block.pangIndex != _uniqueIndex)
                {
                    if (board.Block.blockDeco != null && board.Block.blockDeco.IsInterruptBlockSelect())
                    {
                        if (board.Block.blockDeco.DecoPang(_uniqueIndex, colorType))
                        {
                            return board.Block.IsPangExtendable();
                        }
                    }

                    board.Block.IsExplodeByBomb = true;     //폭탄일때 한템포 늦게 터는지는 설정
                    board.Block.pangIndex = _uniqueIndex;
                    BlockBase.ExplodeBombCount++;

                    //if (GameManager.gameMode == GameMode.ADVENTURE &&
                    //     GameManager.adventureMode == AdventureMode.ORIGIN &&
                    //     board.Block.state != BlockState.PANG &&
                    //     board.Block.bombType != BlockBombType.DUMMY
                    //     )
                    //{
                    //    if (board.Block.isRainbowBomb == false && board.Block.secendBomb == false && board.Block.bombType != BlockBombType.HALF_BOMB)
                    //    {
                    //        ManagerBlock.instance.comboCount++;
                    //        InGameEffectMaker.instance.MakeAdventureCombo(board.Block._transform.position, "Bomb\nCombo " + ManagerBlock.instance.comboCount.ToString());// + new Vector3(0, -0.06f, 0)
                    //    }
                    //    else if (board.Block.isRainbowBomb == true && board.Block.secendBomb == false && type == BlockBombType.RAINBOW_X_RAINBOW)
                    //    {
                    //        ManagerBlock.instance.comboCount++;
                    //        InGameEffectMaker.instance.MakeAdventureCombo(board.Block._transform.position, "Bomb\nCombo " + ManagerBlock.instance.comboCount.ToString());// + new Vector3(0, -0.06f, 0)
                    //    }
                    //}
                    //else
                    {
                        ManagerBlock.instance.comboCount++;
                        InGameEffectMaker.instance.MakeCombo(board.Block._transform.position, "Combo " + ManagerBlock.instance.comboCount.ToString());
                    }
                }
            }
            else
            {
                if (board.Block.type != BlockType.ColorBigJewel)
                    InGameEffectMaker.instance.MakeScore(board.Block._transform.position, 80);
            }
            
            //화단위에 클로버가 있을 경우 처리를 위한 변수.
            bool isHideDeco = false;
            
            if (board.Block.type == BlockType.ColorBigJewel)
            {
                var ColorBigJewel = board.Block as BlockColorBigJewel;
                
                for (int i = 0; i < ColorBigJewel.blockBoardList.Count; i++)
                {
                    if (ColorBigJewel.blockBoardList[i].DecoOnBoard
                        .Find((DecoBoard) => DecoBoard.boardDecoOrder == BoardDecoOrder.CLOVER))
                    {
                        board.HasDecoHideBlock(true, _uniqueIndex, colorType);
                        isHideDeco = true;
                    }
                }

                if (isHideDeco)
                    return true;
                    
                if (isFixPangIndex != -1)
                {
                    if (DynamiteBombToFlowerPotPangIndex == -1)
                    {
                        _bombUniqueIndex++;
                        DynamiteBombToFlowerPotPangIndex = _bombUniqueIndex;
                    }

                    return board.Block.BlockPang(DynamiteBombToFlowerPotPangIndex, colorType, true);
                }
            }
            
            ManagerBlock.instance.AddScore(80);

            if (!isHideDeco)
                return board.Block.BlockPang(_uniqueIndex, colorType, true);
            
        }

        if(board.HasDecoHideBlock(true, _uniqueIndex, colorType) == false)
            board.HasDecoCoverBlock(true, _uniqueIndex, colorType);
        
        return true;
    }

    void SearchDiagonalBlock(int x, int y, int addX, int addY, int count, float removeDelay = 0.3f, List<BombAreaInfo> infoList = null, bool bNoneDirection = false)
    {
        Board back = PosHelper.GetBoardSreeen(x, y, addX, addY);
        BlockDirection bombDirection = ManagerBlock.instance.GetBombDirection(addX, addY);

        if (back != null && count > 0)
        {
            if (hasCarpet == true || CheckExistCarpetAtTargetBoard(PosHelper.GetBoardSreeen(x, y)) == true)
            {
                MakeCarpet(back);
            }

            if (SetRemoveBlock(back, bombDirection))
            {
                bool bombExtend = true;
                for (int j = 0; j < back.BoardOnDisturbs.Count; j++)
                {
                    IDisturb disturb = back.BoardOnDisturbs[j];
                    bombExtend = ManagerBlock.instance.IsCanExtensionBombDirection(back, disturb, back.indexX, back.indexY, bombDirection);
                    if (bombExtend == false)
                        break;
                }

                if (bombExtend == true)
                {
                    PangToDirection(x + addX, y + addY, addX, 0, count - 1, removeDelay, infoList, bNoneDirection);
                    PangToDirection(x + addX, y + addY, 0, addY, count - 1, removeDelay, infoList, bNoneDirection);
                }
                if (ManagerBlock.instance.IsDiagonalDisturbPang(x + addX, y + addY, addX, addY) == false)
                    SearchDiagonalBlock(x + addX, y + addY, addX, addY, count - 1, removeDelay, infoList, bNoneDirection);
            }
            //현재 보드에 있는 데코 중, 방향 확인해서 터지는 데코들에 대한 처리.
            SetDirectionDecoPang(back, BlockDirection.NONE, infoList, bNoneDirection);
        }
    }

    void PangToDirection(int inX, int inY, int offX, int offY, int count = 10, float waitTime = 0.3f, List<BombAreaInfo> infoList = null, bool bNoneDirection = false)
    {
        int offsetX = offX;
        int offsetY = offY;

        bool bombExtend = true;
        BlockDirection bombDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);
        bool hasPangCarpet = hasCarpet;
        if (hasPangCarpet == false)
            hasPangCarpet = CheckExistCarpetAtTargetBoard(PosHelper.GetBoardSreeen(inX, inY));

        for (int i = 0; i < count; i++)
        {
            Board back = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);

            if (back != null && bombExtend)
            {
                if (hasPangCarpet)
                    MakeCarpet(back);
                else
                    hasPangCarpet = CheckExistCarpetAtTargetBoard(back);

                bombExtend = SetRemoveBlock(back, bombDirection);
                if (bombExtend == true)
                {
                    for (int j = 0; j < back.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = back.BoardOnDisturbs[j];
                        bombExtend = ManagerBlock.instance.IsCanExtensionBombDirection(back, disturb, back.indexX, back.indexY, bombDirection);
                        if (bombExtend == false)
                            break;
                    }
                }
                SetDirectionDecoPang(back, bombDirection, infoList, bNoneDirection);
            }

            offsetX += offX;
            offsetY += offY;
        }
    }

    void GetAllBlockInScreen()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                Board board = PosHelper.GetBoardSreeen(j, i);
                if (block != null && block.IsNormalBlock())// && block.type == BlockType.NORMAL)
                {
                    bool isCanPangByRainbow = true;
                    foreach (var deco in board.DecoOnBoard)
                    {
                        if (deco.IsDisturbPangByRainbowBomb() == false)
                        {
                            isCanPangByRainbow = false;
                            break;
                        }
                    }

                    if (isCanPangByRainbow == true)
                    {
                        block.isRainbowBomb = true;
                        rainbowPangList.Add(block);
                    }
                }
            }
        }
    }

    void GetColorBlockInScreenFromCenter()
    {
        List<BlockBase> tempBlockList = new List<BlockBase>();

        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                Board board = PosHelper.GetBoardSreeen(j, i);
                if (block != null &&
                    block.IsBombBlock() == false &&
                    block.colorType != BlockColorType.RANDOM &&
                    block.colorType != BlockColorType.NONE &&
                    block.state != BlockState.PANG &&
                    block.isRainbowBomb == false &&
                    block.colorType == colorType && 
                    block.isCanPangByRainbow() &&
                    board.HasDecoHideBlock() == false &&
                    i >= indexY)
                {
                    block.isRainbowBomb = true;
                    tempBlockList.Add(block);
                }
            }
        }
        BlockBase maxBlock;

        while (tempBlockList.Count > 0)
        {
            maxBlock = tempBlockList[0];

            for (int j = 0; j < tempBlockList.Count; j++)
            {
                float ratioA = (float)(tempBlockList[j].indexX - indexX)/(float)(tempBlockList[j].indexY - indexY);  //(float)(indexY - tempBlockList[j].indexY) / (float)(tempBlockList[j].indexX - indexX);
                float ratioB = (float)(maxBlock.indexX - indexX)/(float)(maxBlock.indexY - indexY);   // (float)(indexY - maxBlock.indexY) / (float)(maxBlock.indexX - indexX);

                if (ratioA < ratioB && rainbowPangList.Contains(tempBlockList[j]) == false)
                {
                    maxBlock = tempBlockList[j];
                }
            }

            if (rainbowPangList.Contains(maxBlock) == false)
            {
                rainbowPangList.Add(maxBlock);
                tempBlockList.Remove(maxBlock);
            }
        }

        tempBlockList.Clear();

        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                BlockBase block = PosHelper.GetBlockScreen(j, i);
                Board board = PosHelper.GetBoardSreeen(j, i);
                if (block != null &&
                    block.IsBombBlock() == false &&
                    block.colorType != BlockColorType.RANDOM &&
                    block.colorType != BlockColorType.NONE &&
                    block.state != BlockState.PANG &&
                    block.isRainbowBomb == false &&
                    block.colorType == colorType &&
                    block.isCanPangByRainbow() &&
                    board.HasDecoHideBlock() == false &&
                    i < indexY)
                {
                    block.isRainbowBomb = true;
                    tempBlockList.Add(block);
                }
            }
        }

        while (tempBlockList.Count > 0)
        {
            maxBlock = tempBlockList[0];

            for (int j = 0; j < tempBlockList.Count; j++)
            {              
                float ratioA = (float)(tempBlockList[j].indexX - indexX) / (float)(tempBlockList[j].indexY - indexY);  //(float ratioA = (float)(indexY - tempBlockList[j].indexY) / (float)(tempBlockList[j].indexX - indexX);
                float ratioB = (float)(maxBlock.indexX - indexX) / (float)(maxBlock.indexY - indexY);   //   float ratioB = (float)(indexY - maxBlock.indexY) / (float)(maxBlock.indexX - indexX);

                if (ratioA < ratioB && rainbowPangList.Contains(tempBlockList[j]) == false)
                {
                    maxBlock = tempBlockList[j];
                }
            }

            if (rainbowPangList.Contains(maxBlock) == false)
            {
                rainbowPangList.Add(maxBlock);
                tempBlockList.Remove(maxBlock);
            }
        }
    }

    //방향으로 터질수 있는 데코 검사.
    private void SetDirectionDecoPang(Board checkBoard, BlockDirection direction, List<BombAreaInfo> infoList, bool bNoneDirection)
    {
        //현재 보드에 있는 데코 중, 방향 확인해서 터지는 데코들에 대한 처리.
        foreach (DecoBase boardDeco in checkBoard.DecoOnBoard)
        {
            boardDeco.SetDirectionPang(checkBoard, indexX, indexY, _uniqueIndex, direction, infoList, bNoneDirection);
        }
    }

    //현재 블럭 한 칸 주위의 방해블럭 제거(십자, 레인보우 폭발 시)
    private void BoardDisturbPang(int indexX, int indexY)
    {
        Board checkBoard = PosHelper.GetBoardSreeen(indexX, indexY, 0, 0);
        for (int i = 0; i < checkBoard.BoardOnDisturbs.Count; i++)
        {
            checkBoard.BoardOnDisturbs[i].SetDisturbPang(_uniqueIndex, BlockBombType.NONE, false);
        }
    }

    //폭탄 효과 제거 코드
    private IEnumerator CoDestroyBomb()
    {
        ManagerBlock.instance.blockMove = true;
        ManagerBlock.instance.creatBlock = true;
        yield return null;

        Destroy(gameObject);
        yield return null;
    }

    #region 카펫    
    private bool CheckExistCarpetAtTargetBoard(Board board)
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        if (board == null)
            return false;

        return board.IsExistCarpetAndCanExpand();
    }

    private void MakeCarpet(Board makeBoard, float delay = 0)
    {
        if (makeBoard == null 
            || makeBoard.Block == null 
            || (makeBoard.Block.IsCanMakeCarpetByBomb() == false)
            || makeBoard.Block.pangIndex == _uniqueIndex)
            return;

        makeBoard.MakeCarpet(delay);
    }
    #endregion
}
