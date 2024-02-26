using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public enum EVENT_CHAPTER_TYPE
{
    FAIL_RESET,
    COLLECT,
    SCORE,
}

[System.Serializable]
[XmlRoot("StageMapData")]
public class StageMapData
{
    [XmlAttribute("A")]
    public int version = 1;

    [XmlAttribute("B")]
    public int moveCount = 0;

    #region 구버전 목표 데이터
    [XmlAttribute("C")]
    public int[] collectCount = { 0 };

    [XmlAttribute("D")]
    public int[] collectType = { 0 };

    [XmlAttribute("E")]
    public int[] collectColorCount = { 0 };
    #endregion

    [XmlAttribute("F")]
    public int gameMode = (int)GameMode.NORMAL;

    [XmlAttribute("G")]
    public int isHardStage = 0;

    #region 필요없는 데이터
    //사용하지는 않지만 이전에 저장할때 사용했기 때문에, "H","I" 에 다른 데이터 덮어씌우면 오류남.
    //["H","I" 사용금지]
    //[XmlAttribute("H")]
    //public int isStartLineH = 0;

    //[XmlAttribute("I")]
    //public int isBlockBlackAdjust = 0;
    #endregion

    //모험모드 정보
    [XmlAttribute("j")]
    public int bossIdx = 0;

    [XmlAttribute("k")]
    public int bossAttr = 0;

    //목표를 저장할 데이터
    [XmlElement("L")]
    public List<CollectTargetInfo> listTargetInfo = new List<CollectTargetInfo>();

    public string data;
}


[System.Serializable]
[XmlRoot("A")]
public class StageInfo
{
    [XmlAttribute("A1")]
    public int Extend_Y = 0;

    #region 구버전 목표 데이터
    [XmlAttribute("A2")]
    public int[] collectCount = { 0 };
    [XmlAttribute("A3")]
    public int[] collectType = { 0 };
    [XmlAttribute("A90")]
    public int[] collectColorCount = { 0 };
    #endregion

    [XmlAttribute("A4")]
    public int widthCentering = 0;
    [XmlAttribute("A5")]
    public int heightCentering = 0;

    [XmlAttribute("A6")]
    public int gameMode = (int)GameMode.NORMAL;

    [XmlAttribute("A7")]
    public int turnCount = 30;

    [XmlAttribute("A9")]
    public int[] probability = { 10, 10, 10, 10, 0, 0 };

    [XmlAttribute("A22")]
    public int score1 = 5000;

    [XmlAttribute("A23")]
    public int score2 = 10000;

    [XmlAttribute("A24")]
    public int score3 = 15000;

    [XmlAttribute("A31")]
    public int score4 = 20000;

    [XmlElement("AA")]
    public List<BlockInfo> ListBlock = new List<BlockInfo>();  //블럭리스트

    [XmlElement("AB")]
    public List<StartBlockInfo> ListStartInfo = new List<StartBlockInfo>();  //출발정보리스트

    [XmlAttribute("A11")]
    public int isCanUse2X2 = 0;

    [XmlAttribute("A12")]
    public int RainbowXBombType = 0;    //현재 사용하지 않는 데이터.

    [XmlAttribute("A13")]
    public int BombType = 0;

    [XmlAttribute("A14")]
    public int waitState = 1;   //wait 모든상태가 한번돌고 wait로 넘어감 

    [XmlAttribute("A17")]
    public int digCount = 0;

    [XmlAttribute("A26")]
    public int[] probability2 = { 10, 10, 10, 10, 0, 0 };

    [XmlAttribute("A27")]
    public int useProbability2 = 0;

    [XmlAttribute("A28")]
    public int statueCount = 1;

    [XmlAttribute("A32")]
    public int collectEventType = 0;

    [XmlAttribute("A33")]
    public int tutorialIndex = 0;

    [XmlAttribute("A34")]
    public int reverseMove = 0;
    [XmlAttribute("A36")]
    public int isHardStage = 0;

    [XmlAttribute("A37")]
    public int isStartLineH = 0;

    [XmlAttribute("A38")]
    public int isBlockBlackAdjust = 0;

    [XmlElement("A39")]
    public List<BattleWaveInfo> battleWaveList = new List<BattleWaveInfo>();

    [XmlElement("A40")]
    public BossInfo bossInfo = new BossInfo();

    [XmlElement("A41")]
    public int sameBlockRatio = 4;

    [XmlAttribute("A42")]
    public int[] probabilityMapMake = { 10, 10, 10, 10, 0, 0 };    //맵 처음 생성 시, 블럭이 생성되어있을 확률.

    [XmlAttribute("A43")]
    public bool isUseProbabilityMakMake = false;

    [XmlAttribute("A44")]
    public int isFixedIceAdjust = 0;

    [XmlAttribute("A45")]
    public int coinProp = 200;    //1000분율
    [XmlAttribute("A46")]
    public int feverCount = 50;     //피버발동 갯수
    [XmlAttribute("A47")]
    public int feverTime = 5;    //피버발동시간

    [XmlAttribute("A48")]
    public bool isCanMakeBonusBombAtColor = true;    //맵에 지정돼있는 컬러블럭위에 보너스 폭탄이 생길 수 있는지(레디아이템, 부스팅 아이템).

    //여러 기믹들에 대한 확장정보들 저장할 리스트
    [XmlElement("A49")]
    public List<GimmickInfo> listBlockInfo = new List<GimmickInfo>();

    [XmlElement("A50")]
    public List<GimmickInfo> listDecoInfo = new List<GimmickInfo>();

    [XmlElement("A51")] //목표를 저장할 데이터
    public List<CollectTargetInfo> listTargetInfo = new List<CollectTargetInfo>();

    [XmlElement("A52")]
    public List<int> listBlockAdjust = new List<int>(); //출발 설정시, 화면수 미포함인 기믹 타입
    
    [XmlAttribute("A53")]
    public int heightDownOneSpace = 0;
    
    // 사과가 메인 기믹으로 쓰인 사과 맵 SMS 통계 구분으로 사용하기 위한 데이터.
    [XmlAttribute("A54")]
    public int isAppleStage = 0;
}

[System.Serializable]
[XmlRoot("B")]
public class BlockInfo
{
    [XmlAttribute("B1")]
    public int inX = 0;
    [XmlAttribute("B2")]
    public int inY = 0;

    [XmlAttribute("B3")]
    public int isActiveBoard = 0;
    [XmlAttribute("B4")]
    public int isScarp = 0;

    [XmlAttribute("B5")]
    public int index = 0;
    [XmlAttribute("B6")]
    public int count = 0;

    [XmlAttribute("B7")]
    public int type = (int)BlockType.NONE;
    [XmlAttribute("B8")]
    public int colorType = (int)BlockColorType.NONE;
    [XmlAttribute("B9")]
    public int bombType = (int)BlockBombType.NONE;

    [XmlAttribute("B10")]
    public int subType = 0;

    [XmlAttribute("B11")]
    public int[] subTarget = new int[5]{0,0,0,0,0};

    [XmlElement("BB")]
    public List<DecoInfo> ListDeco = new List<DecoInfo>();    //데코리스트

    public void Init()
    {
        inX = 0;
        inY = 0;

        isActiveBoard = 1;
        isScarp = 0;

        index = 0;
        count = 0;

        type = (int)BlockType.NONE;
        colorType = (int)BlockColorType.NONE;
        bombType = (int)BlockBombType.NONE;

        ListDeco = new List<DecoInfo>();
        subTarget = new int[5] { 0, 0, 0, 0, 0 };
        subType = 0;
    }

    public void InitBlock()
    {
        index = 0;
        count = 0;

        type = (int)BlockType.NONE;
        colorType = (int)BlockColorType.NONE;
        bombType = (int)BlockBombType.NONE;

        subTarget = new int[5] { 0, 0, 0, 0, 0 };
        subType = 0;
    }
}

[System.Serializable]
[XmlRoot("C")]
public class DecoInfo
{
    [XmlAttribute("C1")]
    public int BoardType = (int)BoardDecoType.NONE;
    [XmlAttribute("C2")]
    public int index = 0; //포탈에 사용  //사이드블럭은 방향으로 사용    //출발사용시 블럭중료 //석상 인덱스   //모래 인덱스
    [XmlAttribute("C3")]
    public int count = 0; //석상 회전   //모래방향
    [XmlAttribute("C4")]
    public int type = 0; //석상단계     //모래 인아웃 인덱스    //출발블럭2로 사용
    [XmlAttribute("C5")]
    public int tempData_1 = 0;
    [XmlElement("C6")]
    public List<BlockAndColorData> listBlockColorData = new List<BlockAndColorData>();
}

//블럭생성정보

[System.Serializable]
[XmlRoot("D")]
public class StartBlockInfo
{
    //생성타입
    [XmlAttribute("D1")]
    public int type = (int)BlockType.NONE;

    [XmlAttribute("D2")]
    public int probability = 0;   //확률

    [XmlAttribute("D3")]
    public int max_display_Count = 0;   //화면최대갯수

    [XmlAttribute("D4")]
    public int max_stage_Count = 0;   //게임최대갯수  0이면 무한대

    [XmlAttribute("D6")]
    public int minTurn = 0;   //생성최소턴

    [XmlAttribute("D7")]
    public int[] countProb = new int[3]{10,10,10};   //확률

    [XmlElement("D8")]
    public List<int> Probs;// = new List<int>();

    [XmlElement("D9")]
    public int[] timeCount = new int[3]{10,10,10};  //

    [XmlElement("D10")]
    public List<int> ProbsSub;// = new List<int>();

    [XmlAttribute("D11")]
    public int[] iceProb = new int[3] { 0, 0, 0 };   //확률

    [XmlElement("D12")]
    public List<int> ProbsICE;// = new List<int>();
}

//모험모드 정보
//적정보, 보상정보
[System.Serializable]
[XmlRoot("E")]
public class BattleWaveInfo
{
    [XmlElement("E1")]
    public List<EnemyInfo> enemyIndexList = new List<EnemyInfo>();

    [XmlElement("E2")]
    public List<int> rewardList;

    [XmlAttribute("E3")]
    public int bossWave = 0;
}


[System.Serializable]
[XmlRoot("F")]
public class EnemyInfo
{
    [XmlElement("F1")]
    public int idx = 0;

    [XmlElement("F2")]
    public int life = 100;

    [XmlAttribute("F3")]
    public int TurnCount = 2;

    [XmlAttribute("F4")]
    public int attPoint = 10;
    
    [XmlAttribute("F5")]
    public int attribute = 0;

    [XmlAttribute("F6")]
    public int enemyHeight = 0;

    [XmlAttribute("F7")]
    public bool isBoss = false;

    [XmlAttribute("F8")]
    public int skillRotation = 0;

    [XmlElement("F9")]
    public List<EnemySkillInfo> skillList = new List<EnemySkillInfo>();
}

[System.Serializable]
[XmlRoot("G")]
public class BossInfo
{
    [XmlElement("G1")]
    public int idx = 0;

    [XmlElement("G2")]
    public int attribute = 0;

    [XmlElement("G3")]
    public int attrSize = 10;
}

[System.Serializable]
[XmlRoot("H")]
public class EnemySkillInfo
{
    [XmlElement("H1")]
    public int skill = 3;

    [XmlElement("H2")]
    public int skillGrade = 3;

    [XmlElement("H3")]
    public int skillMaxCount = 2;
}

[System.Serializable]
[XmlRoot("I")]
[XmlInclude(typeof(GimmickInfo_CountCrack))]
[XmlInclude(typeof(GimmickInfo_BlockGenerator))]
[XmlInclude(typeof(GimmickInfo_BlockHeart))]
public class GimmickInfo
{
    [XmlAttribute("I_0")]
    public int gimmickType = 0;
}

#region 단계석판 정보
[System.Serializable]
[XmlRoot("I1")]
public class GimmickInfo_CountCrack : GimmickInfo
{
    [XmlElement("I1_1")]
    public List<CountCrack_AppleInfo> listAppleInfo = new List<CountCrack_AppleInfo>();
}

[System.Serializable]
[XmlRoot("I1_A")]
public class CountCrack_AppleInfo
{
    [XmlAttribute("I1_A_1")]
    public int crackIndex = 0;

    [XmlAttribute("I1_A_2")]
    public int count = 0;

    [XmlAttribute("I1_A_3")]
    public int index = 0;

    public CountCrack_AppleInfo()
    {
        crackIndex = 1;
        count = 1;
        index = 1;
    }

    public CountCrack_AppleInfo(int crackIdx, int idx, int cnt)
    {
        crackIndex = crackIdx;
        index = idx;
        count = cnt;
    }
}
#endregion

#region 블럭 생성기 정보
[System.Serializable]
[XmlRoot("I2")]
public class GimmickInfo_BlockGenerator : GimmickInfo
{
    [XmlElement("I2_1")]
    public List<BlockGenerator_ImageInfo> listImageInfo = new List<BlockGenerator_ImageInfo>();
}

[System.Serializable]
[XmlRoot("I2_A")]
public class BlockGenerator_ImageInfo
{
    [XmlAttribute("I2_A_1")]
    public int generatorIndex = 0;

    [XmlElement("I2_A_2")]
    public List<BlockAndColorData> listBlockAndColorData = new List<BlockAndColorData>();

    public BlockGenerator_ImageInfo()
    {
        generatorIndex = 0;
        listBlockAndColorData = new List<BlockAndColorData>();
    }

    public BlockGenerator_ImageInfo(int generatorIdx, List<BlockAndColorData> listDatas)
    {
        generatorIndex = generatorIdx;
        listBlockAndColorData = new List<BlockAndColorData>(listDatas);
    }
}
#endregion

#region 하트 길 정보
[System.Serializable]
[XmlRoot("I3")]
public class GimmickInfo_BlockHeart : GimmickInfo
{
    [XmlElement("I3_1")]
    public List<HeartWayInfo> listHeartWayInfo = new List<HeartWayInfo>();
}

[System.Serializable]
[XmlRoot("I3_A")]
public class HeartWayInfo
{
    [XmlAttribute("I3_A_1")]
    public int heartIndex = 0;

    [XmlElement("I3_A_2")]
    public List<HeartWayData> listHeartWayData = new List<HeartWayData>();

    public HeartWayInfo()
    {
        heartIndex = 0;
        listHeartWayData = new List<HeartWayData>();
    }

    public HeartWayInfo(int _heartIndex, List<HeartWayData> listDatas)
    {
        heartIndex = _heartIndex;
        listHeartWayData = new List<HeartWayData>(listDatas);
    }
}

[System.Serializable]
[XmlRoot("I3_B")]
public class HeartWayData
{
    [XmlAttribute("I3_B_1")]
    public int indexX = 0;

    [XmlAttribute("I3_B_2")]
    public int indexY = 0;

    [XmlAttribute("I3_B_3")]
    public int wayCount = -1;

    public HeartWayData()
    {
        indexX = 0;
        indexY = 0;
        wayCount = -1;
    }

    public HeartWayData(int _indexX, int _indexY, int _wayCount)
    {
        indexX = _indexX;
        indexY = _indexY;
        wayCount = _wayCount;
    }

    public HeartWayData(HeartWayData typeData)
    {
        indexX = typeData.indexX;
        indexY = typeData.indexY;
        wayCount = typeData.wayCount;
    }
}
#endregion

#region 인게임 목표 정보
[System.Serializable]
[XmlRoot("J")]
public class CollectTargetInfo
{
    [XmlAttribute("J_0")]  //목표타입
    public int targetType = 0;

    [XmlElement("J_1")]  //목표 정보(컬러당 몇개의 목표를 모아야 하는지)
    public List<TargetColorInfo> listTargetColorInfo = new List<TargetColorInfo>();
}

[System.Serializable]
[XmlRoot("J1")]
public class TargetColorInfo
{
    [XmlAttribute("J1_0")]  //모아야 하는 컬러 타입
    public int colorType = 0;
    [XmlAttribute("J1_1")]  //모아야 하는 목표 수
    public int collectCount = 0;
}
#endregion

[System.Serializable]
[XmlRoot("K")]
public class BlockAndColorData
{
    [XmlAttribute("K_1")]
    public int blockType = (int)BlockType.NONE;

    [XmlAttribute("K_2")]
    public int blockColorType = (int)BlockColorType.NONE;

    [XmlAttribute("K_3")]
    public int subType = -1;

    public BlockAndColorData()
    {
        blockType = (int)BlockType.NONE;
        blockColorType = (int)BlockColorType.NONE;
        subType = -1;
    }

    public BlockAndColorData(BlockAndColorData typeData)
    {
        blockType = typeData.blockType;
        blockColorType = typeData.blockColorType;
        subType = typeData.subType;
    }
}