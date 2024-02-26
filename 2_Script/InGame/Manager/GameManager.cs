using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum GameState
{
    NONE,
    PLAY,
    GAMECLEAR,
    GAMEOVER,
    EDIT
}

public enum GameMode
{
    NORMAL,
    DIG,
    LAVA,
    ADVENTURE,
    COIN,
}

public enum InGameMode
{
    NORMAL,
    EVENT,
    RANK,
    ADVANTURE,
}

public class GameManager : MonoSingletonOnlyScene<GameManager>
{
#region 스테이지 사이즈 관련

    public const int mMAX_X = 11;
    public const int mMAX_Y = 14;
    public static int mExtend_X = 0;
    public static int mExtend_Y = 0;

    //스테이지최대최소크기
    public static int MAX_X
    {
        get { return mMAX_X + mExtend_X; }
    }
    public static int MAX_Y
    {
        get { return mMAX_Y + mExtend_Y; }
    }

    public static int MIN_X = 0;
    public static int MIN_Y = 0;

    //스크린최대최소크기
    public const int X_OFFSET = 1;
    public const int Y_OFFSET = 2;

    public const int STAGE_SIZE_X = 9;
    public const int STAGE_SIZE_Y = 9;

    public static int MOVE_Y = 0;
    public static int MOVE_X = 0;

    public static int MinScreenX
    {
        get {
            if (gameMode == GameMode.NORMAL)// || gameMode == GameMode.ADVENTURE)
                return MIN_X;
            else
                return X_OFFSET + MOVE_X; }
    }

    public static int MaxScreenX
    {
        get {
            if (gameMode == GameMode.NORMAL)// || gameMode == GameMode.ADVENTURE)
                return MAX_X;
            else
                return X_OFFSET + STAGE_SIZE_X + MOVE_X;}
    }

    public static int MinScreenY
    {
        get {
            if (gameMode == GameMode.NORMAL)// || gameMode == GameMode.ADVENTURE)
                return MIN_Y;
            else
                return Y_OFFSET + MOVE_Y; }
    }

    public static int MaxScreenY
    {
        get {
            if (gameMode == GameMode.NORMAL)// || gameMode == GameMode.ADVENTURE)
                return MAX_Y;
            else if (gameMode == GameMode.LAVA)
               return MAX_Y;
            else
                return Y_OFFSET + STAGE_SIZE_Y + MOVE_Y; }
    }

#endregion

    public GameState state = GameState.NONE;
    public static GameMode gameMode = GameMode.NORMAL;
    public InGameMode inGameMode = InGameMode.NORMAL;

    /*
    public Vector3 mMousePosition = Vector3.zero;
    public Vector3 mMouseDeltaPos = Vector3.zero;
    public bool _touching = false;
    public int _touchCount = 0;
    int fingerId = -1;
    */

    public bool IsCanTouch = true;

    public GameObject inGameHelpButtonObj;

    private EncValue _moveCount = new EncValue();
    public int moveCount
    {
        get { return _moveCount.Value; }
        set { _moveCount.Value = value; }
    }

    public int touchCount = 999;

    //클리어연출
    public GameObject StageClearSprite;
    public GameObject boniBirdObj;
    SkeletonAnimation spineboniBird;

    //5턴남음
    public GameObject BirdObj;
    SkeletonAnimation spineBird;

    CameraEffect staticCamera;

    //최초클리어인지 체크 x
    public bool firstClear = false;
    public bool gainClover = false;
    public int useContinueCount = 0;


    public int GrowthyAfterStage = 0;
    public int allStageClearReward = 0;

    public bool firstPlay = false;

    public bool gameOverByDynamite = false;

    //챕터미션 
    //캔디획득
    public int clearMission = 0;
    public int getCandy = 0;
    public int getDuck = 0;

    //플레이시간
    public float playTime = 0;
    public bool LoadScene = false;

    //이벤트
    public int eventIndex = 0;

    //스페셜이벤트_ 임시
    public int specialEventMaxCount = 0;
    public int specialEventAppearMaxCount = 0;
    public float specialEventPorb = 0;
    public int specialEventSect = 0;
    
    //폭탄 방향 전환 
    [System.NonSerialized]
    public bool lineBombRotate = true;

    //랭킹모드
    static public int RankBestScore = 0;
    static public bool IsStageRank = false;

    #region 랜덤 seed 관리
    public System.Random ingameRandom;
    public int ingameSeed = 0;
    #endregion

    void Start()
    {
        Global.timeScalePuzzle = 1f;
        if (EditManager.instance == null)
        {
            GameStart();

            staticCamera = CameraEffect.MakeScreenEffect(1);
            staticCamera.ApplyScreenEffect(Color.black, Color.black, 0f, false); //어둡게
            LoadScene = true;
        }
        else
        {
            ManagerBlock.instance.InitGroundPos();
            ManagerBlock.instance.RefreshBlockManager();
        }        
    }


    public void GameStart()
    {
        mExtend_X = 0;
        mExtend_Y = 0;

        MIN_X = 0;
        MIN_Y = 0;

        MOVE_Y = 0;
        MOVE_X = 0;

        allStageClearReward = 0;

        if (EditManager.instance == null)
        {
            ingameSeed = 0;
            ingameRandom = new System.Random(ingameSeed);

            StartCoroutine(CoInGameStart());

            if (Global._instance.showInGameClearBTN)
             Instantiate(inGameHelpButtonObj);
        }
        else
        {
            ingameSeed = EditManager.instance.inGameSeed <= 0 ? 0 : EditManager.instance.inGameSeed;
            ingameRandom = new System.Random(ingameSeed);

            EditManager.instance.GameStart();
            ManagerBlock.instance.RefreshBlockManagerByStageData();
            Pick.instance.InitPick();
            GameUIManager.instance.GameStart();
            ManagerSound._instance.PlayBGM();
            state = GameState.PLAY;
            ManagerBlock.instance.blockMove = true;
            GameManager.instance.IsCanTouch = true;
            ManagerBlock.instance.BlockTime = 0;
            ManagerBlock.instance.worldRankingItemCount = 0;

            if (gameMode == GameMode.ADVENTURE)
                AdventureManager.instance.InitStage();
        }
    }

    IEnumerator CoInGameStart()
    {        
        if (EditManager.instance != null)
        {
            EditManager.instance.GameStart();
        }
        else
        {
            string stageKey = "pp" + Global.stageIndex + ".xml";
            string stageName = Global.GetHashfromText(stageKey) + ".xml";

            if (Global.eventIndex > 0)
            {
                stageKey = "E" + Global.eventIndex + "_" + Global.stageIndex + ".xml";
                stageName = "E" + Global.eventIndex + "_" + Global.stageIndex + ".xml";
            }

            WWW www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
            yield return www;

            if (www.error == null)
            {
                StringReader reader = new StringReader(www.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

                string key = StageHelper.filekey;
                key = key.Insert(4, stageKey);
                key = key.Substring(0, StageHelper.filekey.Length);
                string sdeData = StageHelper.Decrypt256(tempData.data, key);

                StringReader readerStage = new StringReader(sdeData);
                var serializerStage = new XmlSerializer(typeof(StageInfo));
                ManagerBlock.instance.stageInfo = serializerStage.Deserialize(readerStage) as StageInfo;
            }
            yield return null;
        }

        PlayerPrefs.SetString("LastStageName", "");
/*
        //최초플레이확인
        firstPlay = Global.GameInstance.IsFirstPlay();

        //게임 타입별로 스테이지 초기화
        Global.GameInstance.InitStage();

        //스페셜이벤트 관련 작업
        Global.GameInstance.SpecialEventProcess_OnIngameStart(this);

        //재료모으기 이벤트
        Global.GameInstance.CollectEventProcess_OnIngameStart(this);

        //마지막스테이지 넣기
        GrowthyAfterStage = ManagerData._instance.userData.stage-1;
*/
        //블럭움직임 관련 정보입력
        //스테이지 크기 정렬, 정보
        ManagerBlock.instance.blockMove = false; //블럭정지
        GameManager.instance.IsCanTouch = true;
        yield return null;

        //기타 설정초기화
        GameItemManager.useCount = new int[7]{ 0, 0, 0, 0, 0, 0, 0 };
        useContinueCount = 0;
        gameOverByDynamite = false;

        ManagerBlock.instance.RefreshBlockManagerByStageData();
        yield return null;
        Pick.instance.InitPick();
        yield return null;

        GameUIManager.instance.GameStart();
        yield return null;

        if (gameMode == GameMode.ADVENTURE)
            AdventureManager.instance.InitStage();

        //스파인한번불러오기
        spineboniBird = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, boniBirdObj).GetComponent<SkeletonAnimation>();
        spineboniBird.gameObject.transform.localScale = Vector3.zero;
        Destroy(spineboniBird.gameObject);

        spineboniBird = null;
        yield return null;

        //용암 땅파기일때 화면 끝으로 이동
//        if (Global.RankIndex > 0)
        {

        }
//        else 
        if (mExtend_Y > 0)
        {
            if (gameMode == GameMode.DIG)
            {
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, mExtend_Y * 78, 0);
                ManagerBlock.instance.SetPanel(mExtend_Y * 78);
            }
            else if (gameMode == GameMode.LAVA)
            {
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, 0, 0);
                ManagerBlock.instance.SetLavaPanel(0);
            }
            yield return null;
        }

        //검댕이 연결하기 위해서
        for (int i = GameManager.MAX_Y - 1; i >= GameManager.MIN_X; i--)
        {
            for (int j = GameManager.MAX_X - 1; j >= GameManager.MIN_X; j--)
            {
                Board getBoard = PosHelper.GetBoard(j, i);
                if (getBoard != null)
                {
                    if (getBoard.Block != null && getBoard.Block.IsSpecialMatchable()) getBoard.Block.UpdateBlock();
                }
            }
        }
        yield return null;

        staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.2f, true);  //밝게

        float waitTimer = 0;
        while (waitTimer < 0.2f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

//        yield return Global.GameInstance.SetBGM_OnIngameStart(this);
     
        //배경음악 시작
        ManagerSound._instance.PlayBGM();

        //용암땅파기 화면이동하기
//        if (Global.RankIndex > 0)
        {

        }
//        else 
        if (gameMode == GameMode.DIG && mExtend_Y > 0)
        {
            waitTimer = 0;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            float targetY = mExtend_Y * 78;
            while (true)
            {
                targetY -= Global.deltaTimePuzzle * 120;
                if(targetY <= 0)
                {
                    ManagerBlock.instance.SetPanel(0);
                    GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, 0, 0);
                    break;
                }
                ManagerBlock.instance.SetPanel(targetY);
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);
                yield return null;
            }
        }
        else if (gameMode == GameMode.LAVA && mExtend_Y > 0)
        {
            waitTimer = 0;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            float targetY = 0;
            while (true)
            {
                targetY += Global.deltaTimePuzzle * 120;
                if (targetY >= MOVE_Y * 78)
                {
                    ManagerBlock.instance.SetLavaPanel(MOVE_Y * 78);
                    GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, MOVE_Y * 78, 0);
                    break;
                }
                ManagerBlock.instance.SetLavaPanel(targetY);
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, targetY, 0);
                yield return null;
            }
        }

        if (gameMode == GameMode.ADVENTURE)
        {
            AdventureManager.instance.StartAdventure();
            yield return null;

            while (AdventureManager.instance.IsFinishedInitAction() == false)
                yield return null;
        }


        //목표팝업보여주고 숨기기
        waitTimer = 0;
        while (waitTimer < 0.3f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        {
            ManagerUI._instance.OpenPopupStageTarget();

            waitTimer = 0;
            while (waitTimer < 1.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

/*
        //레디아이템 적용
       if (Global.GameInstance.CanUseReadyItem(0) &&
            (UIPopupReady.readyItemUseCount[0].Value == 1 || UIPopupReady.readyItemUseCount[0].Value == 2))
        {
            int appleCount = 1;

            if(UIPopupReady.readyItemUseCount[0].Value == 1)
            {
                appleCount = 3;
            }
            else if (UIPopupReady.readyItemUseCount[0].Value == 2)
            {
                appleCount = 5;
            }

            waitTimer = 0;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            StartCoroutine(GameUIManager.instance.CoAddAppleBubbleBefore(appleCount));

            yield return GameUIManager.instance.CoAddAppleBefore();

            StartCoroutine(CoStartWorldRank());

            GameUIManager.instance.turnSpirte.transform.localScale = Vector3.one;
            GameUIManager.instance.moveCountLabel.color = new Color32(0xff, 0xe9, 0x32, 0xff);

            for (int i = 0; i < appleCount; i++)
            {
                ManagerSound.AudioPlay(AudioInGame.APPLE);
                moveCount++;
                GameUIManager.instance.RefreshMoveColor(0.5f);

                waitTimer = 0;
                while (waitTimer < 0.2f)
                {
                    waitTimer += Global.deltaTimePuzzle * 0.8f;
                    yield return null;
                }
            }
            GameUIManager.instance.moveCountLabel.color = new Color32(0xff, 0xff, 0xff, 0xff);

            yield return GameUIManager.instance.CoAddAppleBubbleAfter();

        }
        else
        {
            // 월드 랭킹 연출
            yield return CoStartWorldRank();
        }

        //스코어업 표시하기
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            waitTimer = 0;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            //점수업 아이콘표시
        }

        
        if (Global.GameInstance.CanUseReadyItem(3))   //3개모두동시오픈
        {
            // 다이아 레디 아이템 3개 검사
            for (int i = 3; i < 6; i++)
            {
                if (UIPopupReady.readyItemUseCount[i].Value == 0)
                    continue;

                waitTimer = 0;
                while (waitTimer < 0.3f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                BlockBase lineBlock = PosHelper.GetRandomBlockAtGameStart();
                if (Global.GameType == GameType.NORMAL)
                {
                    //스테이지 8의 경우, 인게임 아이템(망치) 튜토리얼이 들어기 때문에 망치가 때릴 위치에 폭탄이 생성되지 않도록 함.
                    //이 경우는 이미 제한된 맵이기 때문에, 컬러 블럭 검사하지 않고 특정영역 내에서만 컬러블럭 생성
                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && firstPlay)
                    {
                        if (Global.stageIndex == 8 && ManagerData._instance.userData.stage <= 8 && ServerRepos.UserItem.InGameItem(0) > 0)
                        {
                            while (lineBlock.indexX == 5 && lineBlock.indexY == 7)
                            {
                                lineBlock = PosHelper.GetRandomBlock();
                            }
                        }
                    }
                }

                if (lineBlock != null)
                {
                    switch (i)
                    {
                        case 3:
                            int randomLine = Random.Range(0, 2);
                            if (randomLine == 0) lineBlock.bombType = BlockBombType.LINE_V;
                            else lineBlock.bombType = BlockBombType.LINE_H;
                            break;
                        case 4:
                            lineBlock.bombType = BlockBombType.BOMB;
                            break;
                        case 5:
                            lineBlock.bombType = BlockBombType.RAINBOW;
                            break;
                    }
                    ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                    lineBlock.JumpBlock();
                    InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                    lineBlock.Destroylinker();
                }
            }
        }

        //부스팅 아이템 체크&생성.
        SetBoostingItem();

        ManagerBlock.instance.useSameColor = false;
        ManagerBlock.instance.SameColorDiffcult = 0;   
        ManagerBlock.instance.SameColorProb = 0;

        if (Global.GameType == GameType.NORMAL &&
            ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0)
        {
            //스테이지 레벨 조정은 CBU 확률을 우선으로 적용.
            if (IsCanAdjustCBUStageLevel() == true)
            {
                //첫 플레이 시 확률 100%, 이후부터 70%확률 적용.
                int levelProg = (ServerRepos.UserComeback.stage == Global.stageIndex) && (ServerRepos.UserComeback.lastFailCount == ManagerData._instance._stageData[Global.stageIndex - 1]._fail) ? 100 : 70;

                SetLevelAdjust(levelProg);
                if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                {
                    GameUIManager.instance.sandboxLabel.gameObject.SetActive(true);
                    GameUIManager.instance.sandboxLabel.text = "CBU 레벨 조정\n 스테이지 [ " + ServerRepos.UserComeback.stage  + " ~ " + (ServerRepos.UserComeback.stage + 9) + " ] , 확률 : " + levelProg;
                }
            }
            else if (ServerRepos.LoginCdn.StageLevelDownList != null && ServerRepos.LoginCdn.StageLevelDownList.Count > 0)
            {
                for (int i = 0; i < ServerRepos.LoginCdn.StageLevelDownList.Count; i++)
                {
                    if (ServerRepos.LoginCdn.StageLevelDownList[i][0] >= Global.stageIndex)
                    {
                        if (ServerRepos.User.purchaseCnt == 0 && ServerRepos.UserShopPackages.Count == 0 &&
                        ManagerData._instance._stageData[Global.stageIndex - 1]._play >= ServerRepos.LoginCdn.StageLevelDownList[i][1])
                        {
                            SetLevelAdjust(ServerRepos.LoginCdn.StageLevelDownList[i][3]);
                            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                            {
                                GameUIManager.instance.sandboxLabel.gameObject.SetActive(true);
                                GameUIManager.instance.sandboxLabel.text = ServerRepos.User.purchaseCnt + "비과금레벨조정적용, 스테이지" + ServerRepos.LoginCdn.StageLevelDownList[i][0] +
                                    " 이하, " + ServerRepos.LoginCdn.StageLevelDownList[i][1] + " 회플레이";
                            }
                            break;
                        }
                        else if ((ServerRepos.User.purchaseCnt > 0 || ServerRepos.UserShopPackages.Count > 0) &&
                        ManagerData._instance._stageData[Global.stageIndex - 1]._play >= ServerRepos.LoginCdn.StageLevelDownList[i][2])
                        {
                            SetLevelAdjust(ServerRepos.LoginCdn.StageLevelDownList[i][3]);
                            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                            {
                                GameUIManager.instance.sandboxLabel.gameObject.SetActive(true);
                                GameUIManager.instance.sandboxLabel.text = ServerRepos.User.purchaseCnt + "과금레벨조정적용, 스테이지" + ServerRepos.LoginCdn.StageLevelDownList[i][0] +
                                    " 이하, " + ServerRepos.LoginCdn.StageLevelDownList[i][2] + " 회플레이";
                            }
                            break;
                        }
                    }
                }
            }
        }
        //튜터리얼불러오기
        //스테이지번호와 최초 플레이 체크
        if (Global.GameType == GameType.NORMAL)
        {
            if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && firstPlay)// && ManagerBlock.instance.flowrCount > 0)
            {
                if (Global.stageIndex == 1 && ManagerData._instance.userData.stage <= 1)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGame2Match);
                }
                else if (Global.stageIndex == 3 && ManagerData._instance.userData.stage <= 3)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGameLineBomb);
                }
                else if (Global.stageIndex == 4 && ManagerData._instance.userData.stage <= 4)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGameCircleBomb);
                }
                else if (Global.stageIndex == 5 && ManagerData._instance.userData.stage <= 5)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGameRainbow);
                }
                else if (Global.stageIndex == 6 && ManagerData._instance.userData.stage <= 6)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialBombXBomb);
                }
                else if (Global.stageIndex == 8 && ManagerData._instance.userData.stage <= 8 && ServerRepos.UserItem.InGameItem(0) > 0)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialInGameItem);

                    //그로씨
                    ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);

                    if (dataServer.play == 1)
                    {
                        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                           "InGameItem1",
                            "InGameItemHAMMER",
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "TutorialInGameItem"
                        );
                        var doc = JsonConvert.SerializeObject(useReadyItem);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                    }
                }
                else if (Global.stageIndex == 9 && ManagerData._instance.userData.stage <= 9)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialLava);
                    //스테이지 13 튜토리얼시 전송하기로 라인측과 협의됨
                    //이시점을 기준으로 이벤트 판단하고 싶다고함
                    if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                    { }
                    else { ServiceSDK.FacebookSDKManager.instance.OnCompleteTutorial(); }
                }
                else if (Global.stageIndex == GameItemManager.CROSS_LINE_OPEN_STAGE && ManagerData._instance.userData.stage <= GameItemManager.CROSS_LINE_OPEN_STAGE && ServerRepos.UserItem.InGameItem(1) > 0)
                {
                    ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);
                    if (dataServer.play == 1)
                    {
                        var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                            (
                               ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                               "InGameItem2",
                                "InGameItemCROSS_LINE",
                                1,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER
                            );
                        var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                    }
                }
                else if (ManagerBlock.instance.stageInfo.tutorialIndex == 1 && ManagerData._instance.userData.stage <= Global.stageIndex)   //검댕이일때
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialBlockBlack);
                }
                else if (Global.stageIndex == 14 && ManagerData._instance.userData.stage <= 14)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialNet);
                }
                else if (Global.stageIndex == GameItemManager.HAMMER3X3_OPEN_STAGE && ManagerData._instance.userData.stage <= GameItemManager.HAMMER3X3_OPEN_STAGE && ServerRepos.UserItem.InGameItem(2) > 0)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialPowerHammer);
                    PlayerPrefs.SetInt("ITEM_POWER_HAMMER_TUTORIAL", 0);

                    ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);
                    if (dataServer.play == 1)
                    {
                        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                           "InGameItem3",
                            "InGameItemTHREE_HAMMER",
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "TutorialInGameItem"
                        );
                        var doc = JsonConvert.SerializeObject(useReadyItem);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                    }
                }
                else if (PlayerPrefs.HasKey("ITEM_POWER_HAMMER_TUTORIAL") == false && ManagerData._instance.userData.stage > 36 && Global.stageIndex > 36 && Global.stageIndex < 232 && ServerRepos.UserItem.InGameItem(2) > 0)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialPowerHammer);
                    PlayerPrefs.SetInt("ITEM_POWER_HAMMER_TUTORIAL", 0);

                    ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);
                    if (dataServer.play == 1)
                    {
                        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                           "InGameItem3",
                            "InGameItemTHREE_HAMMER",
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "TutorialInGameItem"
                        );
                        var doc = JsonConvert.SerializeObject(useReadyItem);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                    }
                }
                else if (Global.stageIndex == 45 && ManagerData._instance.userData.stage <= 45)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TurorialIceApple);
                }
                else if (Global.stageIndex == 66 && ManagerData._instance.userData.stage <= 66)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialStone);
                }
                else if (Global.stageIndex == 171 && ManagerData._instance.userData.stage <= 171)   //다이나마이트
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialDynamite);
                }
                else if (Global.stageIndex == 232 && ManagerData._instance.userData.stage <= 232)   //얼음
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialDecoIce);
                }
                else if (Global.stageIndex == 291 && ManagerData._instance.userData.stage <= 291)   //고정 얼음
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialFixDecoIce);
                }
                else if (Global.stageIndex == 321 && ManagerData._instance.userData.stage <= 321)   //식물 방해블럭
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGrassFenceBlock);
                }
                else if (Global.stageIndex == 381 && ManagerData._instance.userData.stage <= 381)   //카펫
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialCarpet);
                }
                else if (ManagerBlock.instance.stageInfo.tutorialIndex == 4 && ManagerData._instance.userData.stage <= Global.stageIndex)   //화단
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialBigJewel);
                }
                else if (Global.stageIndex == 561 && ManagerData._instance.userData.stage <= 561)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialFireWork);
                }
                else if (ManagerData._instance.userData.stage > 561 && ManagerData._instance.userData.stage <= Global.stageIndex)
                {
                    if (ManagerBlock.instance.stageInfo.tutorialIndex > 0)
                        ManagerTutorial.PlayTutorial((TutorialType)ManagerBlock.instance.stageInfo.tutorialIndex);
                }
            }
        
        */
        playTime = Time.time;
        LoadScene = false;

        state = GameState.PLAY;
        ManagerBlock.instance.blockMove = true; //블럭정지
        GameUIManager.instance.ShowPauseButton(true);

        yield return null;
    }

    /*
   
    public IEnumerator CoStartWorldRank()
    {
        if (ManagerBlock.instance.listWorldRankItem.Count > 0)
        {
            float waitTime = ManagerBlock.instance.GetIngameTime(0.08f);

            for (int i = 0; i < ManagerBlock.instance.listWorldRankItem.Count; i++)
            {
                StartCoroutine(ManagerBlock.instance.listWorldRankItem[i].CoStartEffect());
                yield return new WaitForSeconds(waitTime);
            }

            waitTime = ManagerBlock.instance.GetIngameTime(1f);
            yield return new WaitForSeconds(waitTime);
        }
    }
   
    public IEnumerator LoadBGM()
    {
            if (Global.LoadFromInternal)
                LoadFromInternal();
            else
                yield return LoadFromBundle();
    }

    IEnumerator LoadFromBundle()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);

        bool loadSuccess = false;
        string name = "ingamedata";
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject obj = assetBundle.LoadAsset<GameObject>("ingamedata");
                if (obj)
                {
                    InGameData_Bundle importInfo = Instantiate(obj).GetComponent<InGameData_Bundle>();
                    if (importInfo != null)
                    {
                        var data = importInfo._bgm;
                        if (data != null)
                        {
                            ManagerSound._instance._bgmInGame.clip = data;
                            loadSuccess = true;
                        }
                    }
                }
            }
        }

        if ( !loadSuccess)
        {
            ManagerSound._instance.ResetInGameBGMClip();
        }

        NetworkLoading.EndNetworkLoading();
    }

    public void LoadFromInternal()
    {
        string path = "Assets/5_OutResource/InGame/InGameData.prefab";
#if UNITY_EDITOR
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
            return;

        InGameData_Bundle importInfo = Instantiate(obj).GetComponent<InGameData_Bundle>();
        if (importInfo != null)
        {
            var data = importInfo._bgm;
            if (data != null)
                ManagerSound._instance._bgmInGame.clip = data;
        }
        else
            Debug.LogErrorFormat("LoadFromInternal Failed: ImportData not found in {0}", name);
#endif
    }
    */

    bool CheckReadyItem(int itemType)
    {
        if(UIPopupReady.readyItemUseCount[itemType].Value != 0)
        {
            return true;
        }
        return false;
    }


    void Update()
    {
        //게임클리어 실패 체크
        switch (state)
        {
            case GameState.PLAY:
                if (ManagerBlock.instance.stageInfo.waitState == 0)
                {
                    int tempTurn = ManagerBlock.instance.stageInfo.turnCount - (int)ManagerBlock.instance.BlockTime;
                    if (tempTurn < 0)
                    {
                        tempTurn = 0;
                        StageClear();
                    }
                    if (tempTurn != moveCount)
                    {
                        moveCount = tempTurn;
                        GameUIManager.instance.RefreshMove();
                        if (GameManager.gameMode == GameMode.COIN)
                        {
                            BlockMaker.instance.UseProb2();
                        }
                    }
                }

                if (ManagerBlock.instance.state == BlockManagrState.WAIT && ManagerBlock.instance.checkBlockWait())
                {
                    if (checkAllRemoveTarget())
                    {
                        StageClear();
                    }
                    else if (checkGameOver())
                    {
                        CheckStageFail();
                    }
                }
                break;
        }
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.instance.state = GameState.GAMECLEAR;
            ManagerUI._instance.OpenPopupClear(ManagerBlock.instance.score);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StageFail();
        }


        if (Input.GetKeyDown(KeyCode.Y))
        {
            ManagerTutorial.PlayTutorial(TutorialType.TutorialGame2Match);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            ManagerTutorial.PlayTutorial(TutorialType.TutorialInGameItem);
        }
        */
    }

    bool checkGameOver()
    {
        if (gameMode == GameMode.COIN) return false;
        if (gameMode == GameMode.ADVENTURE) return false;

        if (gameMode == GameMode.NORMAL && moveCount <= 0) return true;
        if (gameMode == GameMode.DIG && moveCount <= 0) return true; //땅파기는 목표가 감지되면 게임오버

        return false;
    }

    public bool checkAllRemoveTarget()
    {
        if (gameMode == GameMode.COIN) 
        {
            if (moveCount > 0)
                return false;
            else
                return true;
        }
        if (gameMode == GameMode.ADVENTURE) return false;

        //목표를 모두 모았는지 검사
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value != null)
            {
                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Value.collectCount > e.Current.Value.pangCount)
                        return false;
                }
            }
        }
        return true;
    }

    public void StageClear()
    {
        if (ManagerBlock.instance.isFeverTime() == true)
            return;

        if (EditManager.instance != null)
        {
            state = GameState.GAMECLEAR;
            return;
        }
        //StartCoroutine(CoRankStageClear());
           StartCoroutine(CoStageClear());
    }


    public void RankNoContinue()
    {
        StartCoroutine(CoRankStageClear());
    }

    public void RankStageClear()
    {
           StartCoroutine(CoRankStageClear());
    }


    bool networkEnd = false;
    bool needClassUp = false;


    /*
    void recvAdventureGameClear(AdventureGameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupAdventureClear._clearResp = resp;

            Global.GameInstance.OnRecvAdvantureGameClear(this, resp);
            networkEnd = true;  
        }
    }


    void recvGameClear(GameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupClear._clearResp = resp;
            UIPopupClear.materialEventGetReward = false;

            Global.GameInstance.OnRecvGameClear(this, resp);

            networkEnd = true;
            //Debug.Log("** GAME CLEAR ok");        

        }
    }
*/

    public int leftMoveCount = 0;   //남은 턴 카운트
    public int useMoveCount = 0;    //사용한 턴 카운트
    public string stageReview = null;

    IEnumerator CoStageClear()
    {
        state = GameState.GAMECLEAR;
        GameUIManager.instance.ShowPauseButton(false);
        BlockMatchManager.instance.DistroyAllLinker();
        yield return null;
/*
        //스테이지클리어테이타처리
        firstClear = false;

        if (EditManager.instance == null)
        {
            if( Global.GameType == GameType.NORMAL )
            {
                if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && ManagerBlock.instance.flowrCount > 0)
                {
                    firstClear = true;
                }
            }
        }

        if (firstClear == true)
        {
            if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsPlayer)
            {
                ServiceSDK.FacebookSDKManager.instance.OnClearStage(Global.stageIndex);
            }
        }

        //ManagerNetwork._instance.SendStageClear(Global.stageIndex, ManagerBlock.instance.flowrCount, ManagerBlock.instance.score, ManagerBlock.instance.coins); //스테이지넘버, 꽃, 점수, 코인 

        ManagerSound._instance.StopBGM();
        ManagerSound.AudioPlay(AudioInGame.STAGE_CLEAR);

        //클리어 글자등장
        GameObject clearSprite = NGUITools.AddChild(GameUIManager.instance.TopUIRoot, StageClearSprite);
        clearSprite.transform.localPosition = new Vector3(0, 70, 0);

        if (GameItemManager.instance != null)        
            GameItemManager.instance.Close();
        
        if (UIPopupPause._instance != null)        
            UIPopupPause._instance.ClosePopUp();
        

        float waitTimer = 0;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //보니 파랑새등장
        if (Global.GameType == GameType.ADVENTURE)
        {

        }
        else
        {
            spineboniBird = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, boniBirdObj).GetComponent<SkeletonAnimation>();
            spineboniBird.gameObject.transform.localScale = Vector3.one * 60;

            spineboniBird.state.SetAnimation(0, "start", false);
            spineboniBird.state.AddAnimation(0, "loop", true, 0f);
        }

        //블럭이 멈출때까지 기다림
        while (!ManagerBlock.instance.checkBlockWait())
        {
            yield return null;
        }


        waitTimer = 0;
        while (waitTimer < 2f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        if (UIPopupPause._instance != null)        
            UIPopupPause._instance.ClosePopUp();
        

        if (GameItemManager.instance != null)        
            GameItemManager.instance.Close();        

        List<BlockBase> listBlock = new List<BlockBase>();

		//클리어 직후에 제거되는 타입의 블럭들 제거
        yield return CoDestroyBlockAtStageClear();

     
        //남은폭탄 처리    
        List<BlockBase> listBlock = new List<BlockBase>();
        if (Global.GameType != GameType.ADVENTURE)
        {
            listBlock.Clear();

            for (int y = MinScreenY; y < MaxScreenY; y++)
            {
                for (int x = MinScreenX; x < MaxScreenX; x++)
                {
                    BlockBase block = PosHelper.GetBlockScreen(x, y);
                    if (block != null && block.IsBombBlock() && block.state == BlockState.WAIT)
                    {
                        block.IsSkipPang = true;
                        listBlock.Add(block);
                        block.JumpBlock();
                        block.RemoveLinkerNoReset();
                        block.AddCoin();
                    }
                }
            }

            while (true)
            {
                if (listBlock.Count == 0 && ManagerBlock.instance.checkBlockWait())
                {
                    break;
                }
                else if (listBlock.Count > 0)
                {
                    int rand = Random.Range(0, listBlock.Count);
                    BlockBase block = listBlock[rand];
                    listBlock.RemoveAt(rand);

                    if (block != null)
                    {
                        block.IsSkipPang = false;
                        InGameEffectMaker.instance.MakeFlyCoin(block._transform.position, 1);
                        block.AddCoin(true);
                        block.BlockPang();
                    }
                    yield return null;

                    waitTimer = 0;
                    while (waitTimer < 0.1f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
                }

                waitTimer = 0;
                while (waitTimer < 0.4f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }

        while (!ManagerBlock.instance.checkBlockWait())
        {
            waitTimer = 0;
            while (waitTimer < 0.1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        waitTimer = 0;
        while (waitTimer < 0.3f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //남은턴 처리
        //남은턴수계산하기

        if (gameMode == GameMode.NORMAL)
        {
            leftMoveCount = moveCount;
        }
        else if (gameMode == GameMode.DIG)
        {
            leftMoveCount = moveCount;
        }
        else if (gameMode == GameMode.LAVA)
        {
            int addLine = 1;
            for (int i = MIN_X; i < MAX_X; i++)
            {
                Board tempBoard = PosHelper.GetBoard(i, Y_OFFSET + STAGE_SIZE_Y -2 +  MOVE_Y - ManagerBlock.instance.LavaLevelCount);
                if (tempBoard != null && tempBoard.IsActiveBoard)
                {
                    if (tempBoard.lava != null)
                    {
                        addLine = 0;
                        break;
                    }
                }
            }

            moveCount = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount-2+ addLine);//ManagerBlock.instance.stageInfo.digCount * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount);
            leftMoveCount = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount -2 + addLine );

            if(leftMoveCount < 0)
            {
                leftMoveCount = 0;
                moveCount = 0;
            }
        }
        else if (gameMode == GameMode.ADVENTURE)
        {
            leftMoveCount = 0;
            moveCount = 0;
        }

        if (moveCount > 0)
        {
            listBlock.Clear();

            for (int y = MinScreenY; y < MaxScreenY; y++)
            {
                for (int x = MinScreenX; x < MaxScreenX; x++)
                {
                    BlockBase block = PosHelper.GetBlockScreen(x, y);
                    Board tempBoard = PosHelper.GetBoard(x,y);
                    if (block != null && tempBoard != null && tempBoard.HasDecoCoverBlock() == false)
                    {
                        if (block.IsNormalBlock() && block.state == BlockState.WAIT && block is NormalBlock)
                        {
                            listBlock.Add(block);
                        }
                    }
                }
            }

            List<BlockBase> pangBlockList = new List<BlockBase>();
            yield return null;

            while (listBlock.Count > 0)
            {
                if (moveCount > 0)
                {
                    moveCount--;
                    GameUIManager.instance.RefreshMove();
                }
                else
                {
                    break;
                }

                int index = Random.Range(0, listBlock.Count);

                if(listBlock[index] != null)
                {
                    listBlock[index].IsSkipPang = true;
                    listBlock[index].bombType = BlockBombType.CLEAR_BOMB;

                    InGameEffectMaker.instance.MakeLastBomb(listBlock[index]._transform.position);

                    listBlock[index].JumpBlock();
                    listBlock[index].AddCoin();
                    listBlock[index].RemoveLinkerNoReset();
                    pangBlockList.Add(listBlock[index]);

                    //사운드재생
                    ManagerSound.AudioPlay(AudioInGame.LAST_PANG);

                    waitTimer = 0;
                    while (waitTimer < 0.2f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
                }

                listBlock.RemoveAt(index);
                yield return null;
            }

            while (pangBlockList.Count > 0)
            {
                BlockBase block = pangBlockList[0];
                pangBlockList.RemoveAt(0);
                if (block != null)
                {
                    block.IsSkipPang = false;
                    block.BlockPang(BlockBomb._bombUniqueIndex);

                    ManagerBlock.instance.AddScore(1000); //남은턴점수추가 나중에 수정
                    InGameEffectMaker.instance.MakeScore(block._transform.position, 1000);
                    InGameEffectMaker.instance.MakeFlyCoin(block._transform.position, 1);

                    waitTimer = 0;
                    while (waitTimer < 0.3f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
                }
                yield return null;
            }

            waitTimer = 0;
            while (waitTimer < 0.6f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        //남은폭탄 한번더처리
        //while (true)
        if (Global.GameType != GameType.ADVENTURE)
        {
            listBlock.Clear();

            for (int y = MinScreenY; y < MaxScreenY; y++)
            {
                for (int x = MinScreenX; x < MaxScreenX; x++)
                {
                    BlockBase block = PosHelper.GetBlockScreen(x, y);
                    if (block != null && block.IsBombBlock() && block.state == BlockState.WAIT)
                    {
                        block.IsSkipPang = true;
                        listBlock.Add(block);
                        block.JumpBlock();
                        block.RemoveLinkerNoReset();
                        block.AddCoin();
                    }
                }
            }

            while (true)
            {
                if (listBlock.Count == 0 && ManagerBlock.instance.checkBlockWait())
                {
                    break;
                }
                else if (listBlock.Count > 0)
                {
                    int rand = Random.Range(0, listBlock.Count);
                    BlockBase block = listBlock[rand];
                    listBlock.RemoveAt(rand);

                    if (block != null)
                    {
                        block.IsSkipPang = false;
                        InGameEffectMaker.instance.MakeFlyCoin(block._transform.position, 1);
                        block.AddCoin(true);
                        block.BlockPang();
                    }
                    yield return null;

                    waitTimer = 0;
                    while (waitTimer < 0.1f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
                }

                waitTimer = 0;
                while (waitTimer < 0.4f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }

        //블럭이 멈출때까지 기다림
        yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());

        //이벤트 상태에서 터지지 못한 블럭들 제거
        yield return CoPangBlockThatCannotRemoveFromEventState();

        //점수정산
        //점수업
        //랭킹모드

        float scoreRatio = 1;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)        
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;        
        else        
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;        

        //GameUIManager.instance.SetScore((int)(tempScoreA * scoreRatio));   //변경된 점수 추가

        int tempScore = (int)(ManagerBlock.instance.score * scoreRatio);

        //꽃점수체크  
        if (GameUIManager.instance.IsRedFlowerGetEvent && GameManager.instance.currentChapter() <= ServerRepos.UserChapters.Count - 1 && ServerRepos.UserChapters[GameManager.instance.currentChapter()].clearState >= 2 && tempScore >= (int)(ManagerBlock.instance.stageInfo.score4*1.1f))
        {
            ManagerBlock.instance.flowrCount = 5;
        }
        else if (GameManager.instance.currentChapter() <= ServerRepos.UserChapters.Count - 1 && ServerRepos.UserChapters[GameManager.instance.currentChapter()].clearState >= 1 && tempScore >= ManagerBlock.instance.stageInfo.score4)
        {
            ManagerBlock.instance.flowrCount = 4;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score3)
        {
            ManagerBlock.instance.flowrCount = 3;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score2)
        {
            ManagerBlock.instance.flowrCount = 2;
        }
        else if (tempScore >= ManagerBlock.instance.stageInfo.score1)
        {
            ManagerBlock.instance.flowrCount = 1;
        }
        yield return null;
        
        //기존랭킹 점수 갱신했는지 체크하기
        // 서버와 통신
        if (Global.GameType == GameType.ADVENTURE)
        {
            networkEnd = false;

            var arg = new AdventureGameClearReq()
            {
                gameKey = ServerRepos.lastGameKey,
                stage = Global.stageIndex,
                chapter = Global.chapterIndex,
                missionClear = 0,
                coin = ManagerBlock.instance.coins,
                deckId = 1,
                dropBoxes = 0,
            };

            ServerAPI.AdventureGameClear(arg, recvAdventureGameClear);
        }
        else
        {
            allStageClearReward = 0;
            
            // 전체 오픈된 스테이지 꽃으로 또는 파란꽃으로 다 클리어 했을떄 보상
            if (ManagerData._instance.userData.stage >= ManagerData._instance.maxStageCount && ManagerBlock.instance.flowrCount >= 3)
            {
                int flowerCount = 0;

                if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel < 3 && ManagerBlock.instance.flowrCount == 3)
                {
                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                        if (ManagerData._instance._stageData[i]._flowerLevel >= 3)
                            flowerCount++;

                    if (ManagerData._instance._stageData.Count == flowerCount + 1)
                        allStageClearReward = 1;

                 //   Debug.Log("흰꽃" + ManagerData._instance._stageData.Count + " " + flowerCount + "  " + allStageClearReward);
                }
                else if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 3 && ManagerBlock.instance.flowrCount == 4)
                {
                    
                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                        if (ManagerData._instance._stageData[i]._flowerLevel >= 4)
                            flowerCount++;

                    if (ManagerData._instance._stageData.Count == flowerCount + 1)
                        allStageClearReward = 2;
                }
            }

            // 0~5 레디아이템, 6~9 인게임아이템, 10 컨티뉴횟수 
            var usedItems = new List<int>();
            usedItems.Add(ManagerBlock.instance.creatBombCount[0].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[1].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[2].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[3].Value);

            var gainMaterial = new List<List<int>>();
            if (Global.GameInstance.CanPlay_CollectEvent() && ServerContents.GetEventChapter(Global.eventIndex).active == 1 && ServerContents.GetEventChapter(Global.eventIndex).type == (int)EVENT_CHAPTER_TYPE.COLLECT)
            {
                gainMaterial.Add(new List<int>() { ManagerBlock.instance.stageInfo.collectEventType, ManagerBlock.instance.materialCollectEvent });
            }

            var getCoinA = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;            
            int tempGainEventItems = Global.GameInstance.CanPlay_SpecialEvent() ? Global.specialEventIndex : 0;

            networkEnd = false;

            int tempStageIndex = Global.stageIndex;
            int tempEventIndex = Global.eventIndex;
            int tempGameType = (int)Global.GameType;

            var arg = new GameClearReq()
            {
                stage = tempStageIndex,
                eventIdx = tempEventIndex,
                isEvent = Global.GameInstance.IsEvent() ? 1 : 0,    // 1은 스템프 이벤트
                type = tempGameType,    //게임타입
                gameFlower = ManagerBlock.instance.flowrCount,
                gameCoin = getCoinA,//ManagerBlock.instance.coins,
                gameScore = tempScore,//ManagerBlock.instance.score,
                gameTurn = leftMoveCount,
            	usedTurn = useMoveCount,
                items = usedItems,
                missionClear = clearMission,
                allStageClear = allStageClearReward,
                missions = new List<int> { getCandy, 0, getDuck, 0},
                playSec = (int)(Time.time - playTime),
                gainMaterials = gainMaterial,
                gainEventItems = new List<int> { tempGainEventItems, ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio },
                eventMaterial = ManagerBlock.instance.materialCollectPos
            };
            ServerAPI.GameClear( arg, recvGameClear );
        }

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        if (UIPopupPause._instance != null)
        {
            UIPopupPause._instance.ClosePopUp();
        }


        //while (UIPopupPause._instance != null)
        //    yield return null;
        if (Global.GameType != GameType.ADVENTURE)
        {
            spineboniBird.state.ClearTracks();
            spineboniBird.state.SetAnimation(0, "end", false);
            spineboniBird.state.Complete += delegate
            {
                Destroy(spineboniBird.gameObject);
            };

            waitTimer = 0;
            while (waitTimer < 1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }      


        // 임시 랭킹 업데이트
        if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer )
        {
        }
        else
        {
            ServiceSDK.ScoreExtraData data = new ServiceSDK.ScoreExtraData();
            ServiceSDK.Score scoreData = ManagerData._instance.userData._profile.scoreData;
            data.isReceiveAlarm = scoreData.isReceiveAlarm;
            data.message = scoreData.message;
            data.pokogoro = scoreData.pokogoro;
            data.comment = scoreData.comment;

            RankingManager.instance.InitRankingServer();
            ManagerData._instance.userData._profile.scoreData.stage = ( int ) ServerRepos.User.stage;
            //Debug.Log("====================== currendtStage: " + ( int ) ServerRepos.User.stage);
           
            //Debug.Log( "ServerReposFlowerCount : " + ServerRepos.User.flower );
            //Debug.Log( "ClientFlowerCount : " + ManagerData._instance.userData._profile.flower );

            //Debug.Log( "============================= Count : " + ManagerData._instance.userData._profile.flower );
        }

       
        if(firstClear)
        {
            var GetClover = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                0,
                1,
                0,
                (int)(ServerRepos.User.star)
                );
            var doc = JsonConvert.SerializeObject(GetClover);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);

            var GetHeart = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                0,
                1,
                0,//(int)(ServerRepos.User.clover),
                (int)(ServerRepos.User.AllClover)//(int)(ServerRepos.User.fclover)
                );
            doc = JsonConvert.SerializeObject(GetHeart);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }

        var getCoin = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        var GetCoin = new ServiceSDK.GrowthyCustomLog_Money
            (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
            0,
            getCoin,//ManagerBlock.instance.coins,
            (int)(ServerRepos.User.coin),
            (int)(ServerRepos.User.fcoin)
            );
        var docCoin = JsonConvert.SerializeObject(GetCoin);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCoin);

        
        if(Global.GameType == GameType.ADVENTURE){
            ManagerUI._instance.OpenPopupAdventureClear();
        }
        else 
        ManagerUI._instance.OpenPopupClear(ManagerBlock.instance.score);
*/
        yield return null;
    }

    /*
    //스테이지 클리어 시, 남아있으면 제거되어야 하는 블럭들 제거
    private IEnumerator CoDestroyBlockAtStageClear()
    {
        List<BlockBase> listBlock = new List<BlockBase>();
        for (int y = MinScreenY; y < MaxScreenY; y++)
        {
            for (int x = MinScreenX; x < MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && block.IsDestroyBlockAtStageClear() && block.state == BlockState.WAIT)
                {
                    block.IsSkipPang = true;
                    listBlock.Add(block);
                }
            }
        }

        for (int i = 0; i < listBlock.Count; i++)
        {
            if (listBlock[i] != null)
            {
                if (SkipIngameClear == false)
                    InGameEffectMaker.instance.MakeBlockPangEffect(listBlock[i]._transform.position);
                listBlock[i].PangDestroyBoardData();
            }
        }
        yield return null;

        float waitTimer = 0;
        while (waitTimer < 0.4f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
    }*/

    IEnumerator CoRankStageClear()
    {
        state = GameState.GAMECLEAR;
        GameUIManager.instance.ShowPauseButton(false);

        //목표로 날아가는 이펙트가 모두 사라질 때 까지 대기.
        yield return new WaitUntil(() => FlyTarget.flyTargetCount <= 0);
        BlockMatchManager.instance.DistroyAllLinker();
        yield return null;


        ManagerSound._instance.StopBGM();

        //ManagerSound.AudioPlay(AudioInGame.STAGE_CLEAR);

        /*
        //클리어 글자등장
        GameObject clearSprite = NGUITools.AddChild(GameUIManager.instance.UI_Root, StageClearSprite);
        clearSprite.transform.localPosition = new Vector3(0, 70, 0);
        */

        if (GameItemManager.instance != null)
        {
            GameItemManager.instance.Close();
        }

        if (UIPopupPause._instance != null)
        {
            UIPopupPause._instance.ClosePopUp();
        }

        
        float waitTimer = 0;
        /*
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        */
        /*
        //보니 파랑새등장
        spineboniBird = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, boniBirdObj).GetComponent<SkeletonAnimation>();
        spineboniBird.gameObject.transform.localScale = Vector3.one * 60;

        spineboniBird.state.SetAnimation(0, "start", false);
        spineboniBird.state.AddAnimation(0, "loop", true, 0f);
        */

        //블럭이 멈출때까지 기다림
        while (!ManagerBlock.instance.checkBlockWait())
        {
            yield return null;
        }

        /*
        waitTimer = 0;
        while (waitTimer < 2f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        */
        if (UIPopupPause._instance != null)
        {
            UIPopupPause._instance.ClosePopUp();
        }

        if (GameItemManager.instance != null)
        {
            GameItemManager.instance.Close();
        }


        while (!ManagerBlock.instance.checkBlockWait())
        {
            waitTimer = 0;
            while (waitTimer < 0.1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        /*
        // 서버와 통신
        {
            // 0~5 레디아이템, 6~9 인게임아이템, 10 컨티뉴횟수 
            var usedItems = new List<int>();
            usedItems.Add(ManagerBlock.instance.creatBombCount[0].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[1].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[2].Value);
            usedItems.Add(ManagerBlock.instance.creatBombCount[3].Value);


            var gainMaterial = new List<List<int>>();
            var getCoinA = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;
            //int tempScore = ManagerBlock.instance.score;


            float scoreRatio = 1;
            if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
                scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;
            else
                scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;

            //GameUIManager.instance.SetScore((int)(tempScoreA * scoreRatio));   //변경된 점수 추가

            int tempScore = (int)(ManagerBlock.instance.score * scoreRatio);



            int tempGainEventItems = Global.GameInstance.CanPlay_SpecialEvent() ? Global.specialEventIndex : 0;

            networkEnd = false;
            var arg = new GameClearReq()
            {
                stage = 0,
                eventIdx = Global.eventIndex,
                isEvent = 0,    // 1은 스템프 이벤트
                type = (int)GameType.RANK,
                gameFlower = ManagerBlock.instance.flowrCount,
                gameCoin = getCoinA,//ManagerBlock.instance.coins,
                gameScore = tempScore,//ManagerBlock.instance.score,
                gameTurn = leftMoveCount,
                items = usedItems,
                missionClear = clearMission,
                allStageClear = 0,
                missions = new List<int> { 0, 0, 0, 0 },
                playSec = (int)(Time.time - playTime),
                gainMaterials = gainMaterial,
                gainEventItems = new List<int> { 0, 0 },
                eventMaterial = 0
            };
            ServerAPI.GameClear(arg, recvGameClear);
        }

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        if (UIPopupPause._instance != null)
        {
            UIPopupPause._instance.ClosePopUp();
        }


        var getCoin = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        var GetCoin = new ServiceSDK.GrowthyCustomLog_Money
            (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
            0,
            getCoin,//ManagerBlock.instance.coins,
            (int)(ServerRepos.User.coin),
            (int)(ServerRepos.User.fcoin)
            );
        var docCoin = JsonConvert.SerializeObject(GetCoin);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCoin);
        */

        ManagerUI._instance.OpenPopupClear(ManagerBlock.instance.score);
        yield return null;
    }


    public void StageOver()
    {
        if (EditManager.instance != null)
        {
            state = GameState.GAMEOVER;
            return;
        }

        if (state == GameState.GAMEOVER) return;

        state = GameState.GAMEOVER;


        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);

        {
            // 0~5 레디아이템, 6~9 인게임아이템, 10 컨티뉴횟수 
            var usedItems = new List<int>();
            usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
            usedItems.Add(GameItemManager.useCount[0]);
            usedItems.Add(GameItemManager.useCount[1]);
            usedItems.Add(GameItemManager.useCount[2]);
            usedItems.Add(GameItemManager.useCount[3]);
            usedItems.Add(GameManager.instance.useContinueCount);

            var remainTarget = ManagerBlock.instance.GetListRemainTarget();

            /*
            if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
            {
                var arg = new AdventureGameFailReq()
                {
                    type = (int)Global.GameType,
                    eventIdx = Global.eventIndex,
                    stage = Global.stageIndex,
                    chapter = Global.chapterIndex,
                    coin = ManagerBlock.instance.coins,
                    seed = ServerRepos.IngameSeed,
                    deckId =  1,
                    
                };

                ServerAPI.AdventureGameFail(arg, recvAdventureGameFail);
            }
            else
            {
                var arg = new GameFailReq()
                {
                    type = (int)Global.GameType,
                    stage = Global.stageIndex,
                    eventIdx = Global.eventIndex,
                    isEvent = Global.GameInstance.IsEvent() ? 1 : 0,
                    chapter = Global.GameInstance.GetChapterIdx(),
                    gameCoin = 0,
                    gameScore = 0,
                    gameRemains = remainTarget,
                    stageKey = Global.GameInstance.GetStageKey(),
                    seed = ServerRepos.IngameSeed,
                    easyMode = LevelAdjusted == true ? 1 : 0

                    //items = usedItems
                };

                ServerAPI.GameFail(arg, recvGameFail);

                if (Global.GameType == GameType.NORMAL)
                    FailCountManager._instance.SetFail(Global.stageIndex.ToString());
            }
*/

        }
    }

    bool checkStage = false;
    public void CheckStageFail()
    {
        if (checkStage) return;
        checkStage = true;

        StartCoroutine(CoCheckStageFail());
    }

    IEnumerator CoCheckStageFail()
    {
        IsCanTouch = false;
        float timer = 0;
        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }   

        if (ManagerBlock.instance.state == BlockManagrState.WAIT && ManagerBlock.instance.checkBlockWait() && FlyTarget.flyTargetCount <= 0 && checkGameOver())
        {
            StageFail();
            yield return null;
        }

        IsCanTouch = true;
        checkStage = false;
        yield return null;
    }

    public void StageFail(bool isDynamiteFail = false)
    {
        if (GameItemManager.instance != null)
        {
            GameItemManager.instance.Close();
        }

        if (UIPopupPause._instance != null)
        {
            UIPopupPause._instance.ClosePopUp();
        }

        if (EditManager.instance != null)
        {
            state = GameState.GAMEOVER;
            return;
        }

        if (state == GameState.GAMEOVER) return;

        state = GameState.GAMEOVER;

        if (isDynamiteFail)
        {
            StartCoroutine(CoEvent(0.2f, () =>
            {
                ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_FAIL);
                GameUIManager.instance.ShakingCamera();
            }));
            GameManager.instance.gameOverByDynamite = true;
        }
        /*
        int leftMoveCount = 0;

        if (gameMode == GameMode.NORMAL || gameMode == GameMode.DIG)
        {
            leftMoveCount = moveCount;
        }
        else if (gameMode == GameMode.LAVA)
        {
            int addLine = 1;
            for (int i = MIN_X; i < MAX_X; i++)
            {
                Board tempBoard = PosHelper.GetBoard(i, Y_OFFSET + STAGE_SIZE_Y - 2 + MOVE_Y - ManagerBlock.instance.LavaLevelCount);
                if (tempBoard != null && tempBoard.IsActiveBoard)
                {
                    if (tempBoard.lava != null)
                    {
                        addLine = 0;
                        break;
                    }
                }
            }
            moveCount = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount - 2 + addLine);
            leftMoveCount = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount - 2 + addLine);

            if (leftMoveCount < 0)
            {
                leftMoveCount = 0;
                moveCount = 0;
            }
        }
        
        bool ShowContinue = Global.GameInstance.CanContinue(this);

        if (ShowContinue && noMoreMoveFail == false)
        {
            if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
            {
                ManagerUI._instance.OpenPopupAdventureContinue();
            }
            else
            {
                if (isDynamiteFail)
                {
                    StartCoroutine(CoEvent(1.3f, () =>
                    {
                        ManagerUI._instance.OpenPopupContinue();
                        ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);
                    }));
                }
                else
                {
                    ManagerUI._instance.OpenPopupContinue();
                    ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);
                }
            }
            return;
        }

        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        
        if( Global.GameInstance.IsCompetitionGame() )   // 승패가 없고, 점수경쟁만 있는 컨텐츠류는 여기서 끊는다
        {
            StartCoroutine(CoRankStageClear());
            return;
        }

        // 이 아래는 게임 타입 특성상 실패가 있는 경우만 처리
        // 근데 콜렉션 처리나 사용한 아이템같은거 처리가 이 밑에 있기 때문에, 점수경쟁이면서 아이템 사용 가능한 컨텐츠가 발생하는 경우 수정이 필요하다
        


        //실패팝업
        if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
        {
            var arg = new AdventureGameFailReq()
            {
                type = (int)Global.GameType,
                eventIdx = Global.eventIndex,
                stage = Global.stageIndex,
                chapter = Global.chapterIndex,
                coin = ManagerBlock.instance.coins,
                seed = ServerRepos.IngameSeed,
                deckId =  1,
            };

            ServerAPI.AdventureGameFail(arg, recvAdventureGameFail);
        }
        else
        {
            // 0~5 레디아이템, 6~9 인게임아이템, 10 컨티뉴횟수 
            var usedItems = new List<int>();
            usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
            usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
            usedItems.Add(GameItemManager.useCount[0]);
            usedItems.Add(GameItemManager.useCount[1]);
            usedItems.Add(GameItemManager.useCount[2]);
            usedItems.Add(GameItemManager.useCount[3]);
            usedItems.Add(GameManager.instance.useContinueCount);

            var remainTarget = new List<int>();
            for (int j = 0; j < ManagerBlock.instance.collectCount.Length; j++)
            {
                if (j == 0)
                {
                    if (ManagerBlock.instance.crackTotalCount == 0)
                    {
                        remainTarget.Add(0);
                    }
                    else
                    {
                        int count = ManagerBlock.instance.crackTotalCount - ManagerBlock.instance.pangBlockCount[0];
                        remainTarget.Add(count);
                    }
                }
                else
                {
                    if (ManagerBlock.instance.collectCount[j] == 0)
                    {
                        remainTarget.Add(0);
                    }
                    else
                    {
                        int count = ManagerBlock.instance.collectCount[j] - ManagerBlock.instance.pangBlockCount[j];
                        remainTarget.Add(count);
                    }
                }
            }

            for (int j = 0; j < ManagerBlock.instance.collectColorCount.Length; j++)
            {
                if (ManagerBlock.instance.collectColorCount[j] == 0)
                {
                    remainTarget.Add(0);
                }
                else
                {
                    int count = ManagerBlock.instance.collectColorCount[j] - ManagerBlock.instance.pangBlockColorCount[j];
                    remainTarget.Add(count);
                }
            }


            var arg = new GameFailReq()
            {
                type = (int)Global.GameType,
                stage = Global.stageIndex,
                eventIdx = Global.eventIndex,
                isEvent = Global.GameInstance.IsEvent() ? 1 : 0,
                chapter = Global.GameInstance.GetChapterIdx(),
                gameCoin = 0,
                gameScore = 0,
                gameRemains = remainTarget,
                stageKey = Global.GameInstance.GetStageKey(),
                seed = ServerRepos.IngameSeed,
                easyMode = LevelAdjusted == true ? 1 : 0
                // items = usedItems
            };

            ServerAPI.GameFail(arg, recvGameFail);

            if (Global.GameType == GameType.NORMAL)
                FailCountManager._instance.SetFail(Global.stageIndex.ToString());
        }

         */

    }

    /*
    void recvGameFail(GameActionResp resp)
    {
        //Debug.Log("GameFail: " + resp);
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            ManagerUI._instance.OpenPopupFail(resp.dcChanged == true);
        }
    }

    void recvAdventureGameFail(AdventureGameFailResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupAdventureFail._failResp = resp;
            ManagerUI._instance.OpenPopupAdventureFail();
            ManagerAdventure.User.SyncFromServer_Animal();
        }
    }

    */

    public void RemoveTurn()
    {
        if (ManagerBlock.instance.stageInfo.waitState == 0)
            return;

        useMoveCount++;
        if (gameMode == GameMode.COIN)        
            return;        

        if (gameMode == GameMode.NORMAL)
        {
            moveCount--;
            if (moveCount == 5)
            {
                GameUIManager.instance.ShowLeft5Turn();
            }
            GameUIManager.instance.RefreshMove();
        }
        else if (gameMode == GameMode.DIG)
        {
            moveCount--;
            if (moveCount == 5)
            {
                GameUIManager.instance.ShowLeft5Turn();
            }
            GameUIManager.instance.RefreshMove();
        }
        else if (gameMode == GameMode.LAVA)
        {

        }


//        GameUIManager.instance.RemoveSpecialEventCount(); 
        GameUIManager.instance.GetStageRankRoutte();
    }

    public void GetTurnCount(int continueCount)
    {
        if (continueCount > 4) continueCount = 4;

        for (int i = 0; i < 4 + continueCount; i++)
        {
            StartCoroutine(CoEvent(i * 0.2f, () =>
            {
                ManagerSound.AudioPlay(AudioInGame.APPLE);
                moveCount++;
                GameUIManager.instance.RefreshMove();

                if (continueCount <= 1)
                {
                    StartCoroutine(CoEvent(0.6f, () => { GameManager.instance.state = GameState.PLAY; }));
                }

            }));
        }

        if (continueCount <= 1)
        {
            BlockMatchManager.instance.SetBlockLink(true);
            if (GameManager.instance.lineBombRotate) ManagerBlock.instance.LineType = (ManagerBlock.instance.LineType == BlockBombType.LINE_V) ? BlockBombType.LINE_H : BlockBombType.LINE_V;
            return;
        }
        

        StartCoroutine(CoEvent(1f, () =>
        {
            BlockBase lineBlock = PosHelper.GetRandomBlock();
            if(lineBlock != null)
            {
                if (continueCount == 2 || continueCount >= 4)
                {
                    int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                    if (randomLine == 0) lineBlock.bombType = BlockBombType.LINE_V;
                    else lineBlock.bombType = BlockBombType.LINE_H;
                }
                else if (continueCount == 3) lineBlock.bombType = BlockBombType.BOMB;

                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                lineBlock.JumpBlock();
                InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                lineBlock.Destroylinker();

                if (continueCount <= 3)
                {
                    StartCoroutine(CoEvent(0.6f, () => { GameManager.instance.state = GameState.PLAY; }));
                    BlockMatchManager.instance.SetBlockLink(true);
                    if (GameManager.instance.lineBombRotate) ManagerBlock.instance.LineType = (ManagerBlock.instance.LineType == BlockBombType.LINE_V) ? BlockBombType.LINE_H : BlockBombType.LINE_V;
                    return;
                }

                lineBlock = PosHelper.GetRandomBlock();
                lineBlock.bombType = BlockBombType.BOMB;

                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                lineBlock.JumpBlock();
                InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                lineBlock.Destroylinker();
            }

            StartCoroutine(CoEvent(0.6f, () => { GameManager.instance.state = GameState.PLAY; }));
            BlockMatchManager.instance.SetBlockLink(true);
            if (GameManager.instance.lineBombRotate) ManagerBlock.instance.LineType = (ManagerBlock.instance.LineType == BlockBombType.LINE_V) ? BlockBombType.LINE_H : BlockBombType.LINE_V;
            return; 

        }));

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


    public int currentChapter(int stageNum = 0)
    {
        if (stageNum == 0) stageNum = Global.stageIndex;

        if (stageNum < 10)
        {
            return 0;
        }
        else if (stageNum < 21)
        {
            return 1;
        }
        else
        {
            return (stageNum - 21) / 15 + 2;
        }
    }

    //인게임에서 랜덤 값 구할 때 사용.
    public int GetIngameRandom(int minValue, int maxValue)
    {
        if (ingameSeed <= 0)
            return Random.Range(minValue, maxValue);
        else
            return ingameRandom.Next(minValue, maxValue);
    }
}
