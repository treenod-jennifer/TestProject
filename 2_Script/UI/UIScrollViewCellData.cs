using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewCellData
{
    public int Index { get; set; }
}

public enum BtnStageState
{
    none,
    BtnLock,
    BtnCheerUp,
    BtnComingSoon,
    BtnSeed,
    BtnLeaf,
    BtnFlower,
    BtnBlueFlower,
    BtnRedFlower,
}

public enum ChapterState
{
    Chapter_Lock,
    Chapter_Flower_White,
    Chapter_Flower_Blue,
    Chapter_Flower_Red,
    Chapter_Flower_Complete
}

public enum BtnStageArrowDir
{
    none,
    left,
    right,
    down,
    up
}

//스테이지 선택 화면 아이템 데이터.
public class StageUIData : ScrollViewCellData
{
    public int nChapterNumber { get; set; }
    public int nStageNumber { get; set; }
    public bool bChapterUI { get; set; }  
    public bool bRight { get; set; }
    public bool bAction { get; set; }
    public BtnStageState eBtnState { get; set; }
    public BtnStageArrowDir eArrowDir { get; set; }

    //에피소드 미션 클리어 여부
    public int questProg1 = 0;
    //에피소드 미션 달성도
    public int questProg2 = 0;

    public ChapterMission questData = null;

    public StageUIData(int index, int chapterNumber, int stageNumber, ChapterMission qData)
    {
        Index           = index;
        nChapterNumber  = chapterNumber;
        nStageNumber    = stageNumber;
        questData       = qData;
    }
}

//랭킹 창 아이템 데이터.
public class RankingUIData : ScrollViewCellData
{
    public int nRank { get; set; }
    public UserBase UserProfile { get; set; }

    public RankingUIData(int index, int rank, UserBase profile)
    {
        Index = index;
        nRank = rank;
        UserProfile = profile;
    }
}

//꽃 보상 아이템 데이터.
public class FlowerRewardUIData : ScrollViewCellData
{
    public int stageCnt { get; set; }
    public int blueFlowerCnt { get; set; }
    public RewardType rewardType { get; set; }
    public int rewardValue { get; set; }
    public int rewardState { get; set; }
    public int flowerType { get; set; }

    public FlowerRewardUIData(int index, int stCnt, int blueCnt, RewardType type, int value, int state, int flowertype)
    {
        Index = index;
        stageCnt = stCnt;
        blueFlowerCnt = blueCnt;
        rewardType = type;
        rewardValue = value;
        rewardState = state;
        flowerType = flowertype;
    }
}
