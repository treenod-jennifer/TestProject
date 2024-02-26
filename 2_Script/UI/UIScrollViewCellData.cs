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
}

public enum BtnStageArrowDir
{
    none,
    left,
    right,
    down,
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

    public int questProg1 = 0;
    public int questProg2 = 0;

    public QuestGameData questData = null;

    public StageUIData(int index, int chapterNumber, int stageNumber, QuestGameData qData)
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
    public UserData UserProfile { get; set; }

    public RankingUIData(int index, int rank, UserData profile)
    {
        Index = index;
        nRank = rank;
        UserProfile = profile;
    }
}