using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// (DB)game data > 서버에서 유저 상관없이 고정적으로 받아오는 게임 데이타(로컬캐싱 하고 사용할수있다, 예 전체 스테이지 리스트)
// (cdn)game data> DB game data를 기반으로 클라서 필요에 따라 받아서 사용
// (DB)user data> 서버에서 유저에 따라 받아오는 데이타(캐싱없이 로그인 할때마다 받아오기, 예 유저가 클리어한 스테이지 정보)


//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
[System.Serializable]
public class CdnLogin
{
    public int cdnV = 1;

    public int textV = 1;                       // systemtext 버전
    public int chapterV = 1;                    // 챕터 버전
    public int missionV = 1;                    // 지금 플레이 중인 날의 모든 미션 리스트 버전

    
    public int emergencyResetCacheAssetV = 0;         // 저장된 케시가 해당 버전 보다 낮으면 캐시된 데이타를 모두 날린다(서비스중 잘못된 버전을 올려 파일이 꼬였을때 긴급하게 활용)
    public int emergencyResetCacheImageV = 0;        

    public int timeMissionCost = 10;             // 시간 미션 즉시 완료, (코인)

    public bool readyItemSale = false;
    public int readyItem1 = 1;  // 0이면 비활성화,   item1,2,3은 코인   item4,5,6은 다이아
    public int readyItem2 = 2;
    public int readyItem3 = 0;
    public int readyItem4 = 4;
    public int readyItem5 = 5;
    public int readyItem6 = 0;

    public bool inGameItemSale = false;
    public int inGameItem1 = 1; // 인게임 아이템은 모두 다이아
    public int inGameItem2 = 2;
    public int inGameItem3 = 3;
    public int inGameItem4 = 0;
    public int inGameItem5 = 0;
    public int inGameItem6 = 0;

    public bool continueSale = false; // 세일유무, 세일이면 다이아 가격을 낮추기 보다 추가 아이템 표시 방향으로,또는 갯수 추가,턴은 고정, 사과로 풀던지
    public int continueCost = 8;      // 컨티뉴 가격,다이아
    public int continueMax = 3;       // 한번에 이어서 컨티뉴 최대 가능 횟수, 0이면 무한 계속 가능

    public int cItemType1_1 = 0; // 0이면 없고 1면 라인폭, 2면 더블, 3이면 레인보우  4면 사과?
    public int cItemCount1_1 = 0;// 아이템 갯수,  
    public int cItemType1_2 = 0;
    public int cItemCount1_2 = 0;
    public int cItemType1_3 = 0;
    public int cItemCount1_3 = 0;

    public int cItemType2_1 = 0;
    public int cItemCount2_1 = 0;
    public int cItemType2_2 = 0;
    public int cItemCount2_2 = 0;
    public int cItemType2_3 = 0;
    public int cItemCount2_3 = 0;

    public int cItemType3_1 = 0;
    public int cItemCount3_1 = 0;
    public int cItemType3_2 = 0;
    public int cItemCount3_2 = 0;
    public int cItemType3_3 = 0;
    public int cItemCount3_3 = 0;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // jewer shop
    // 항목당 240엔  720엔  이런가격은 고정
    public int jewerSale = 0;  // 1이면 세일 라벨 표시 
    public int jewerCount1 = 0; // 1번 제품을 구입 했을때 획득하는 총 다이아 갯수
    public int jewerBonus1 = 0; // 1번 제품을 구입 했을때 획득하는 총 다이아 갯수에서 보너스로 획득한 수,, jShopCount1 이 110이면  jShopBonus1가 10이면 구입시 110개를 얻고 10는 그냥 보여주는 용
    public int jewerCount2 = 0;
    public int jewerBonus2 = 0;
    public int jewerCount3 = 0;
    public int jewerBonus3 = 0;
    public int jewerCount4 = 0;
    public int jewerBonus4 = 0;
    public int jewerCount5 = 0;
    public int jewerBonus5 = 0;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // coin shop
    // 항목당 다이아 갯수는 고정
    public int coinSale = 0;  // 1이면 세일 라벨 표시 
    public int coinCount1 = 0;
    public int coinBonus1 = 0;
    public int coinCount2 = 0;
    public int coinBonus2 = 0;
    public int coinCount3 = 0;
    public int coinBonus3 = 0;
    public int coinCount4 = 0;
    public int coinBonus4 = 0;
    public int coinCount5 = 0;
    public int coinBonus5 = 0;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // clover shop 클로버만 갯수는 그대로인데 가격이 바뀜  각각 5개,60분 각각 세일할수있게.
    public int clover5Sale = 0;         // 1이면 세일 라벨 표시 
    public int cloverPrice = 0;
    public int cloverFreeTimeSale = 0;  // 1이면 세일 라벨 표시 
    public int cloverFreeTimePrice = 0; 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // giftbox 관련
    public int gbDurationS = 360;      // 작은 박스 여는데 걸리는 시간 60*60 =   1시간
    public int gbDurationM = 2880;      // 작은 박스 여는데 걸리는 시간 60*60*8 =   8시간
    public int gbDurationB = 17280;      // 작은 박스 여는데 걸리는 시간 60*60*48 =   48시간
    
    // 각 박스의 코인 최소 최대 범위
    public int gbMixCoinS = 5;          // 작은 기프트박스 코인 최소 갯수
    public int gbMaxCoinS = 10;         // 작은 기프트박스 코인 최대 갯수
    public int gbMixCoinM = 20;
    public int gbMaxCoinM = 30;
    public int gbMixCoinB = 50;
    public int gbMaxCoinB = 100;

    // 각 박스의 클로버 최대 최소 범위 //스몰 상자는 클로버 없음
    public int gbMixCloverM = 1;       // 중간 박스 최소 클로버
    public int gbMaxCloverM = 3;
    public int gbMixCloverB = 3;
    public int gbMaxCloverB = 5;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // 


    public int reboot = 30;     // 메뉴씬에서 백그라운드 갔다 포그라운드올때 30분 지났으면 재시작
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
// 스테이지, 챕터
[System.Serializable]
public class StageData
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 게임 데이타 없음,
    


    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 유저 데이타
    public int _flowerLevel = 0;       // 획득 꽃 단계,  0이상이면 클리어,3이면 꽃
    public int _score = -1;     // 최고 스코어
    public int _continue = 0;     // 컨티뉴 카운트//지표용 아님, 한판내에서 연속으로 컨티뉴를 할때마다 보상이나 가격을 다르게 하기위해// 게임 시작하면 0으로 리셋, 
    public int _play = 0;       // 스테이지 play카운트 성공이든 실패든 도전한 카운트
    public int _fail = 0;       // 스테이지fail카우트//스테이지를 클리어 하지 못하면 계속 올라감, 나중에 오랜만에 접속한 유저이면서 일정 획수 이상 실패하면 난이도를 낮추기 위해서,,, 


    public string _comment = "";    // 코멘트
    public int _like = 0;           // 좋아요 아이콘 종류
    public int _missionProg1 = 0;   // 챕터 미션 달성도1
    public int _missionProg2 = 0;   // 챕터 미션 달성도2
    public int _missionClear = 0;   
}

[System.Serializable]
public class ChapterData
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 게임 데이타
    public int _cIndex = 0;
    public int _stageIndex = -1;    // 해당 챕터의 스테이지 시작 인텍스
    public int _stageCount = -1;    // 해당 챕터의 스테이지 갯수


    // 퀘스트에 포함 되므로 삭제
    //public int _missionType = 0;    // 챕터미션 종류
    //public int _rewardType = 0;     // 챕터미션 보상 종류
    //public int _rewardCount = 0;    // 보상 갯수


    public int materialType1 = 0;   // 랜덤하게 지정된 스테이지에서 나올 재료에 대한 타입
    public int materialCount1 = 0;  // 갯수
    public int materialRatio1 = 0;  // 3가지 종류중에서 비율,,, materialRatio1,2,3 각각   1,5,5라면   리스트에 각각 타입1을 1나
    public int materialType2 = 0;
    public int materialCount2 = 0;
    public int materialRatio2 = 0;
    public int materialType3 = 0;
    public int materialCount3 = 0;
    public int materialRatio3 = 0;

    public List<int> stageVersion = new List<int>();
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 유저 데이타
    //public int _state = 0;          // 챕터 상태, 미션 진행전 = 0, 진행중 = 1, 완료 = 2,, 흰꽃으로 완료 = 3, 별꽃진행중  = 4 뭐 등등
    //public int _missionState = 0;   // 챕터 미션 상태.. 진행전 = 0 진행중 = 1 , 완료 = 2 , 보상 받아감 = 3
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
// 이벤트 스테이지

[System.Serializable]
public class EventStageData
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 게임 데이타 
    public int index = 0;
    public int group = 0;           // 이벤트 내 스테이지 그룹,
    public bool freeClover = false; // 무료 클로버 스테이지
    public bool failReset = false;

    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 유저 데이타
    public int _flowerLevel = 0;    // 획득 꽃 단계,  0이상이면 클리어,3이면 꽃
    public int _score = -1;         // 최고 스코어
    public int _play = 0;       // 스테이지 play카운트 성공이든 실패든 도전한 카운트

    public string _comment = "";    // 코멘트
    public int _like = 0;           // 좋아요 아이콘 종류
}
[System.Serializable]
public class EventChapterRewardData
{
    public RewardType rewardType = RewardType.none;                     // 보상 종류
    public int rewardCount = 0;
}
[System.Serializable]
public class EventChapterData
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 게임 데이타 
    public int index = 0;   // 이벤트 스테이지 그룹
  //  public long _eventSrartTime = 0;
 //   public long _eventErartTime = 0;
    // 그룹은 최대 3개.
 //   public List<EventChapterRewardData> _rewardGroup1 = new List<EventChapterRewardData>();
//    public List<EventChapterRewardData> _rewardGroup2 = new List<EventChapterRewardData>();
 //   public List<EventChapterRewardData> _rewardGroup3 = new List<EventChapterRewardData>();
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // 유저 데이타
    public int _active = 0;
    public int _groupState = 0; //만악 2그룹까지 있을때 _groupState가 1이면 초반,  2이면 1그룹 완료 3이면 2그룹 완료
    public int _state = 0;

    public static void SetUserData()
    {
        ManagerData._instance._eventChapterData.Clear();
     /*   foreach (var item in ServerContents.EventChapters)
        {
            EventChapterData data = new EventChapterData();
            data.index = item.Value.index;
            ManagerData._instance._eventChapterData.Add(item.Key, data);
        }
        foreach (var item in ServerRepos.EventChapters)
        {
            EventChapterData data;
            if (ManagerData._instance._eventChapterData.TryGetValue(item.Key, out data))
            {
                data._groupState = item.Value.groupState;
                data._state = item.Value.stage;
                data._active = 0;
            }
        }*/
    }
    public static void SetUserData(int in_index)
    {
     //   ManagerData._instance._eventChapterData[in_index]._groupState = ServerRepos.EventChapters[in_index].groupState;
     //   ManagerData._instance._eventChapterData[in_index]._state = ServerRepos.EventChapters[in_index].stage;
    }
   // public List<EventStageData> _stageList = new List<EventStageData>();


}


[System.Serializable]
public class CdnEvent
{
    public int index = 0;
    public List<string> assetNameList = new List<string>();

}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
// 씬월드

public enum TypeSceneState
{   
    Wait,
    Active,
    Finish,
    None,
}
[System.Serializable]
public class SceneData
{
    public int sceneArea = 0;
    public int sceneIndex = 0;
    public TypeSceneState state = TypeSceneState.Wait;  // 미션 상태

}


public enum TypeMissionState
{
    Inactive,
    Active,
    Clear,
}
[System.Serializable]
public class MissionData
{
    // game data
    public int index = 0;                           // 미션의 글로발 index,  1번부터 시작, 1번이 팬스 열기 2번이 우체통,  2Day라도 계속 증가,, 다시 1부터 시작아님

    public int day = 0;                             // 이 미션의 day
    public int sceneArea = 0;                       // 해당 영역 인댁스
    public int sceneIndex = 0;                      // 해당 씬 익댁스 // 단계 미션의 경우는 해당 인덱스에서 단계만큼 다음  // 시간 미션의 경우 해당 인덱스 다음이 완료
    public int sceneIndexWakeup = 0;                // 완료 액션 해당 씬 익댁스
    

    public int needStar = 1;                        // 미션 클리어 하는데 필요한 별갯수   // 0이면 트리구조를 만들기 위한 빈노드
    public int stepClear = 0;                       // 단계가 있는 미션, 3이면 3단계로 needStar만큼 각각 클리어 해야함
    public int waitTime = 0;                        // 미션 클리어 후 기다리는 시간(초)
    public int waitCoin = 0; //지울 예정, 통일
    public int housingIndx = 0;                     // 해당 미션 완료시 해당 하우징타입 추가//0이면 추가 하우징 없고.. 1이면 우체국 하우징 컬럼 추가
    public Texture2D _icon = null;                  // 미션 인텍스를 통해 www 로딩 캐싱,  미션 완료 누를때 뒤에서 아이콘 파일도 받아두기
    

    // user data
    public TypeMissionState state = TypeMissionState.Inactive;  // 미션 상태
    public int clearCount = 0;                                  // 단계 미션의 진행 카운트
    public long clearTime = 0;




    
}
[System.Serializable]
public class MissionTreeNode
{
    public string comment = "";

    //child가 있으면 자신은 빈 노드
    //next보다 child를 먼져 돌아야함
    //하나의 노드 내  child는 순서대로 받기  단, child만 가지고 있는 빈 노드면 다음 노드도 진행... 그래야 복수의 미션을 얻을수있음

    public int missionIndex = -1;                           // 미션 인덱스,, -1이면 빈노드
    // user data
    public bool clear = false;  // 미션 상태
    public List<MissionTreeNode> childMissionList = null;   // child가 있으면 해당 노드는 빈노드로 해야됨.
    public List<MissionTreeNode> nextMissionList = null;    // child를 모두 돌고 next가 있다면 next로
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
//
// 하루 단위 미션 순서
//
[System.Serializable]
public class DayData
{
    public string comment = "";
    public List<string> areaDataUpdate;             // 각각의 영역 데이타 갱신할 에셋번들 이름 리스트
    //public List<SceneFinishData> sceneFinish = new List<SceneFinishData>();
    public List<MissionTreeNode> sceneTree = new List<MissionTreeNode>();
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
//
// 퀘스트
public enum RewardType
{
    none,
    clover,
    coin,
    jewel,
    star,
    flower,

    readyItem1,
    readyItem2,
    readyItem3,
    readyItem4,
    readyItem5,
    readyItem6,

    ingameItem1,
    ingameItem2,
    ingameItem3,
    ingameItem4,
    ingameItem5,

    ingameContinue,


    toy,        // pokogoro  value가 토이 번호
    stamp,      // 다이어리창에 있는 이미지   value가 스템프 번호
    housing,    // value가 하우징 번호
    cloverFreeTime,
    costume = 22,   // value 가 코스튬 번호


    boxSmall = 100,    // 선물박스,
    boxMiddle,
    boxBig,

    material = 1000,    // 1001 번이 1번 메트리얼   value가 갯수
}
public enum QuestType
{
    none,
    star,                               // 별모으기
    whiteFlower,                        // 흰꽃모으기
    inviteBase,                         // 초대하기 미션


        
    chapter_Candy    = 1000,            // 챕터미션 사탕모으기(챕터 미션은 중복으로 받는 경우 없음, 주어진 미션을 해결해야 해결한 챕터 다음 챕터 미션을 받는 구조, 다음챕터 미션이 없다면 그다음 챕터 미션을 받음)
    chapter_AllFlower,
    chapter_Duck,

    collabo_OnePiece    = 2000          // 원피스 콜라보(cdn 설정에 따른 기간 한정 이벤트들,유저 상태와 상관없이 추가 되었다가 사라짐)
}
[System.Serializable]
public class QuestUserData
{
    public int index = 0;           // 미션 인텍스
    public int state = 0;           // 0.진행중(받은상태)> 1.퀘스트는 완료> 2.보상 확인해서 퀘스트 사라짐
    public long timer = 0;          // 해당시간까지 완료 못하면 미션 fail
    public long exTimer = 0;        // 확장 타이머
    public int prog1 = 0;               // 달성도
    public int prog2 = 0;
    public int prog3 = 0;

}
[System.Serializable]
public class QuestGameData
{
    // game data
    public int index = 0;
    public QuestType type = QuestType.none;
    public string title = null; // null이면 type에 따라 string파일에서 읽어서 만듬 (이벤트 같은 경우 서버에서 미션을 등록할때 기입)
    public string info = null;

    public int level = 0;                           // 미션 레벨
    public int duration = 0;                        // 주어진 시간이 있다면 주어진 시간내에 완료해야함(초.. 미션을 받을때 시간 더하기 duration)
    public int targetCount = 0;                     // 목표 카운트
    public int value1 = 0;
    public int value2 = 0;
    public int value3 = 0;
    public int value4 = 0;
    public int value5 = 0;
    public long valueTime1 = 0;
    public long valueTime2 = 0;

    public RewardType rewardType = RewardType.none;                     // 보상 종류
    public int rewardCount = 0;                                            // 보상 갯수

    // user data
    public int state = 0;           // 0.진행중(받은상태)> 1.퀘스트는 완료> 2.보상 확인해서 퀘스트 사라짐
    public long timer = 0;          // 해당시간까지 완료 못하면 미션 fail
    public long exTimer = 0;        // 확장 타이머
    public int prog1 = 0;               // 달성도
    public int prog2 = 0;
    public int prog3 = 0;


    public static void RefleshData()
    {/*
        foreach (var item in ServerRepos.OpenQuest)
        {
            QuestGameData quest;
            if (ManagerData._instance._questGameData.TryGetValue(item.index, out quest))
            {
                quest.state = item.state;

                quest.prog1 = item.prog1;
                quest.prog2 = item.prog2;
                quest.prog3 = item.prog3;
            }
        }
        */
    }
    public static void SetUserData()
    {/*
        ManagerData._instance._questGameData.Clear();
        foreach (var item in ServerRepos.OpenQuest)
        {
            QuestGameData quest = new QuestGameData();
            quest.state = item.state;
            quest.index = item.index;
            quest.type = (QuestType)item.type;
            //quest.title = item.title;
            quest.info = item.info;
            quest.level = item.level;
            //quest.duration = item.duration;
            quest.targetCount = item.targetCount;
            quest.prog1 = item.prog1;
            quest.prog2 = item.prog2;
            quest.prog3 = item.prog3;
            quest.valueTime1 = item.valueTime1;

            if (item.rewardList.Count > 0)
            {
                quest.rewardType = (RewardType)item.rewardList[0].type;
                quest.rewardCount = item.rewardList[0].value;
            }

            //quest.value1 = item.values[0];
            //quest.value2 = item.values[1];
            //quest.value3 = item.values[2];
            //quest.value4 = item.values[3];
            //quest.value5 = item.values[4];
            //quest.valueTime1 = item.valueTime1;
            //quest.valueTime2 = item.valueTime2;
            //quest.rewardType1 = (RewardType)item.rewards[0].type;
            //quest.rewardCount1 = item.rewards[0].value;
            //quest.rewardType2 = (RewardType)item.rewards[1].type;
            //quest.rewardCount2 = item.rewards[1].value;
            //quest.rewardType3 = (RewardType)item.rewards[2].type;
            //quest.rewardCount3 = item.rewards[2].value;
            quest.timer = item.timer;

            ManagerData._instance._questGameData.Add(item.index, quest);
        }*/
    }
}


// 순서형 미션
// 미션의 갯수가 0이거나 1일때 최대 3개까지 받기(미션 완료 할때마다 미션 주는 형태는 유저를 지치게 만드므로 적어졌다 확 쌓이는 흐름이 되도록,,  이벤트 미션은 별도)
[System.Serializable]
public class QuestProgress
{
    public int index = 0;

    // 생성될 조건(모두 만족할때 받을수 있음)
    public int day = 0;         // 0이면 무시 // 만약 3이면 3일이상은 되어야 받을수 있음
    public int chapter = 0;     // 0이면 무시 // 만약 5면 5챕터 이상 위치에  있어야 받을수 있음
    public int stage = 0;       // 0이면 무시 // 만약 2면 2스테이지 이상이여야 받을수있음
    public int mission = 0;     // 0이면 무시 

    // 생성 해야할 미션(최대 동시에 2개)
    public int spawnIndex1 = 0;     // 생성한 퀘스트 인텍스
    public int spawnLevel1 = 0;     // 퀘스트 레벨   
    public int spawnEx1 = 0;        // 예비

    public int spawnIndex2 = 0;
    public int spawnLevel2 = 0;
    public int spawnEx2 = 0;
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
//
// Housing (꾸미기 아이템) ,usetdata, 
[System.Serializable]
public class HousingUserData
{
    public string tempComment = "";
    // user data
    public int index = 0;   // 1우체통,2벤치,3펌프,보니집문 리스
    public int selectModel = 0; // 해당 하우징에서 선택 가능한모델중에 선택된 모델


    public static void SetUserData()
    {/*
        foreach (var item in ServerRepos.UserHousingSelected)
        {
            int findIndex = -1;
            for (int i = 0; i < ManagerData._instance._housingSelectData.Count; i++)
            {
                if (ManagerData._instance._housingSelectData[i].index == item.index)
                    findIndex = i;
            }


            if (findIndex == -1)
            {
                HousingUserData data = new HousingUserData();
                data.index = item.index;
                data.selectModel = item.selectModel;
                ManagerData._instance._housingSelectData.Add(data);
            }
            else
            {
                ManagerData._instance._housingSelectData[findIndex].index = item.index;
                ManagerData._instance._housingSelectData[findIndex].selectModel = item.selectModel;
            }
        }*/
    }
}

public class HousingProgressData
{
    public int day;
    public int chapter;
    public int stage;
    public int mission;

    public bool CheckProgress(int _mission, int _day, int _chapter, int _stage)
    {
        return this.mission == _mission && this.day <= _day && this.chapter <= _chapter && this.stage <= _stage;
    }
}


[System.Serializable]
public class HousingMaterialData // 하우징 재료
{
    public int _index = 0; // 재료 인텍스    // 재료들은 단순히 번호임,   재료들 게임 데이타 리스트는 불필요
    public int _count = 0; // 재료 갯수
}
public enum PlusHousingModelDataType
{
    none,
    byMission,  // 일반 day내 미션에 의한(기존으로 진행하면 재공받는)
    byProgress, // 재료를 모아 획득
    byEvent,    // 초대나 다른 외부 이벤트를 통해 획득
}
[System.Serializable]
public class PlusHousingModelData // 일반 진행상의 Day미션 이외에 추가로 얻을수있는 모델
{
    // game data
    //
    public string housingName = ""; // 지울예정. housingIndex_modelIndex  를 이용해서 생성한 키로 스트링리스트에서 빼씀,
    public int housingIndex = 0;    // 1번이면 우체통 하우징
    public int modelIndex = 0;      // 1,2,3번은 기본 씬에 있으니 4번부터.
    public long expire_ts = 0;      // 하우징 끝나는 시간.

    public int costCoin = 1;    // 코인이나 보석 둘중하나... 즉시 획득 비용
    public int costJewel = 2;
    public PlusHousingModelDataType type = PlusHousingModelDataType.byMission;  // 하우징 open타입
    public List<HousingMaterialData> material = null;   // 필요한 재료// 

    // user data
    public int open = 0;       // true면 오픈되어 유저에게 목표로 보이는 상태, false면 이유저에게는 아직 미오픈 진행상태
    public int active = 0;     // open되었을때 또는 미션을 통해 획득한 하우징모델

    public HousingProgressData housingProgressData;

    public static void SetUserData()
    {
        //Debug.Log("OpenHousing 하우징 유저" + ServerRepos.UserHousingItems.Count);
        /*
        foreach (var item in ServerRepos.UserHousingItems)
        {
            PlusHousingModelData data = null;
            if (ManagerData._instance._housingGameData.TryGetValue(item.index + "_" + item.modelIndex, out data))
            {
                data.open = item.isOpen;
                data.active = item.active;
            }
            //PlusHousingModelData data = ManagerData._instance._housingGameData[item.index + "_" + item.modelIndex];
            
        }*/
    }
}
// 순서형 미션
// 하우징 요소들은 퀘스트와 다르게 해당되는 상태면 open되어 쌓이는 형태로 한두가지 다음 오픈될 요소도 락되고 언제 열릴지 보여짐.,,,  이벤트 미션은 별도로 서버에서 이벤트 기간 바로바로 오픈,
[System.Serializable]
public class HousingProgress
{
    public int index = 0;

    // 오픈될 조건(모두 만족할때 받을수 있음)
    public int day = 0;         // 0이면 무시 // 만약 3이면 3일이상은 되어야 받을수 있음
    public int chapter = 0;     // 0이면 무시 // 만약 5면 5챕터 이상 위치에  있어야 받을수 있음
    public int stage = 0;       // 0이면 무시 // 만약 2면 2스테이지 이상이여야 받을수있음
    public int mission = 0;     // 0이면 무시 


    public int spawnIndex = 0;     // 오픈될 하우징인덱스
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 
//
// 재료
[System.Serializable]
public class MaterialData
{
    // game data
    public string name = "";

    // user data
    public int index = 0;
    public int count = 0;


    public static void SetUserData()
    {
        //Debug.Log("UserMaterials 재료 유저" + ServerRepos.UserMaterials.Count);

        ManagerData._instance._meterialData.Clear();
            /*
        foreach (var item in ServerRepos.UserMaterials)
        {
            MaterialData data = new MaterialData();
            data.index = item.index;
            data.count = item.count;
            ManagerData._instance._meterialData.Add(data);
        }*/
    }
}


[System.Serializable]
public class MaterialSpawnProgress
{
    public int index = 0;
    public int missionStart = 0;    // 조건 , 해당 미션을 완료해야만 생성조건에 해당됨, 0이면 무시
    public int missionEnd = 0;      // 조건 , 해당 미션을 이후는 생성하지않음, 0이면 무시

    public int position = 0;        // 생성 시킬 위치
    public int duration = 0;        // 생성 시킬 시간 간격(초)(60*60이면 한시간에 position위치에 materialIndex를 materialCount씩 생성 최대 maxCount만큼)
    public int maxCount = 0;        // 생성할수 있는 최대 갯수, 
    public int materialIndex = 0;        // 생성할  재료 인덱스
    public int materialCount = 0;        // 한번에 생성될 재료 객수
}


[System.Serializable]
public class MaterialSpawnUserData
{
    public int index = 0;

    public long timer = 0;               //           
    public int position = 0;             // 생성 시킬 위치
    public int materialIndex = 0;        // 생성된  재료 인덱스
    public int materialCount = 0;        // 재료 객수


    public static void SetUserData()
    {

        ManagerData._instance._materialSpawnData.Clear();/*
        foreach (var item in ServerRepos.SpawnMaterial)
        {
            MaterialSpawnUserData material = new MaterialSpawnUserData();

            material.index = item.spawnIndex;
            material.materialIndex = item.materialIndex;
            material.materialCount = item.materialCount;
            material.position = item.position;

            ManagerData._instance._materialSpawnData.Add(material);
        }*/
    }
}
//////////////////////////////////////////////////////////////////////////////////////////////////
[System.Serializable]
public class PokoyuraData
{
    public int index = 0;


    public static void SetUserData()
    {/*
        ManagerData._instance._pokoyuraData.Clear();
        foreach (var item in ServerRepos.UserToys)
        {
            PokoyuraData poko = new PokoyuraData();
            poko.index = item.index;
            ManagerData._instance._pokoyuraData.Add(poko);
        }*/
    }
}

//////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////// 

////////////////////////////////////////////////////////////////////////////////////////////////// 
// 유저
[System.Serializable]
public class UserData
{
    public int coin = 0;
    public int jewel = 0;
    public int clover = 0;
    public int star = 0;

    // Profile
    public ulong userID = 0;       // 게임 아이디
    public string token = "";
    public string name = "";

    public string message = "";
    public bool isMessageBlocked;
    
    public int stage = 0;
    public int chapter = 0;
    public int day = 0;
    public int mission = 0; // 미션 카운트,완료 할때마다 계속 증가,
    public int questProgress = 0;   // 퀘스트 뭉치 받는 진행도
    public int housingProgress = 0; // 하우징 뭉치 받는 진행도

    public int materialStage1 = 0;  // 재료가 나오는 스테이지로 지정된 스테이지들(해당 아이템 나오는 스테이지를 클리어 하거나 신규 스테이지를 클리어 할때 마다 1,2,3번 모두 다시 리셋  내가 있는 스테이지 위치를 기준으로 최근에 클리어한 스테이지 8개중에 3개가 선택됨)
    public int materialStage2 = 0;
    public int materialStage3 = 0;


    // 필요한 경우만 별도
    public long cloverCoolTime = 0; // 클로버 쿨타임
    public long cloverRequestCoolTime = 0; // 클로버 요청하기 쿨타임
    public long inviteTime = 0; // 초대메세지 타임

    public void SetUserData()
    {

    }
}


[System.Serializable]
public class MessageData
{
    public long index;           // 메세지 index
    public long fuid;          // 상대방 uid
    public string userKey;      // 상태 라인 유저키, 프로필표시나 메세지를 보내기 위해

    public long ts;         // 유효한 시간  ,0 이면 무한 저장
    public RewardType type;
    public int mtype = 0;
    public int value = 0;

    public int textKey = 0;
    public string text = null;
    
    public static void SetUserData()
    {
        ManagerData._instance._messageData.Clear();
        /*
        foreach (var item in ServerRepos.Inbox)
        {
            MessageData data = new MessageData();

            data.index = item.index;
            data.fuid = item.fuid;
            data.userKey = item.fUserKey;
            data.ts = item.ts;
            data.type = (RewardType)item.type;
            data.mtype = item.mtype;
            data.value = item.value;
            data.textKey = item.textKey;
            data.text = item.text;


            ManagerData._instance._messageData.Add(data);
        }*/
    }

}
////////////////////////////////////////////////////////////////////////////////////////////////// 
// NoticeData
[System.Serializable]
public class NoticeData
{
    public int id = 0;
    public int noticeIndex = 0;
    public int csh;         // 국가별 노출유무 (eThailand,eTaiwan,eJapan,eEng) 시프트 비트방식  (0010->일본만 노출)
    public int os;          // 기기별 노출유무 (eIOS,eAOS) 시프트 비트방식  (10->아이폰만 노출)
    public long startTs = 0;
    public long endTs = 0;
    public string url = "";
    public int depth = 0;


    public static void SetData()
    {
    /*    ManagerData._instance._noticeData.Clear();
        foreach (var item in ServerRepos.Notices)
        {
            NoticeData data = new NoticeData();
            data.id = item.id;
            data.noticeIndex = item.noticeIndex;
            data.startTs = item.startTs;
            data.endTs = item.endTs;
            data.url = item.url;
            data.depth = item.depth;

            ManagerData._instance._noticeData.Add(data);
        }
        */
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////// 
// gift
// 
public enum GiftBoxType
{
    boxSmall,        // 선물박스 타입
    boxMiddle,
    boxBig,
    boxNomal,
    boxSpecial,
    boxPremium,
}

// 서버에서 유저가 선물상자를 획득했을때  day 진행도에 따라 내용물을 결정하는데 참조할 변수리스트
// 디비 Day쪽 컨텐츠
public class GiftBoxMaterialGameData
{
    public int index = 0;

    public int day = 0;

    // 각 상자의 메트리얼 시작과 끝
    public int sStartIndex = 1;
    public int sEndIndex = 2;
    public int sCount = 5;

    public int mStartIndex = 1;
    public int mEndIndex = 4;
    public int mCount = 5;

    public int bStartIndex = 3;
    public int bEndIndex = 6;
    public int bCount = 5;
}


public class GiftBoxUserReward
{
    public RewardType type = RewardType.none;
    public int value = 0;
}
// 선물 상자를 받는 순간 내용물이 정해짐
[System.Serializable]
public class GiftBoxUserData
{
    public long index = 0;
    public GiftBoxType type = GiftBoxType.boxSmall; // 이미지 종류
    public long openTimer = 0;  // 기다려야하는 시간
  //  public ServerUserGiftBox _data = null;

    public List<GiftBoxUserReward> rewardList = new List<GiftBoxUserReward>();
}



public enum LOCAL_NOTIFICATION_TYPE
{
    CLOVER,
    TIME_MISSION,
    GIFT_BOX,
    EVENT_INFO,
    END
}