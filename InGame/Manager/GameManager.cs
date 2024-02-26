using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Spine.Unity;
using UnityEngine;
using Protocol;
using UnityEngine.Events;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.Networking;

public enum ProceedPlayType
{
    NONE = 0,   //서버에서 작업되지 않은 상태
    CLOSE,      //마을가기만 허용
    RETRY,      //재플레이
    NEXT,       //다음스테이지 가기
    REBOOT,     //강제리붓
}

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

public enum AdventureMode
{
    ORIGIN, //일반 스테이지 버전
    NORMAL,   //초기 버전 
    CURRENT,    //현재 버전
}

public enum GameFailType
{
    TURN,
    LAVA,
    DYNAMITE,
    TIME,
}

public enum ScoreFlowerType
{
    BUD = 1,
    SPROUT = 2,
    FLOWER_WHITE = 3,
    FLOWER_BLUE = 4,
    FLOWER_RED = 5,
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
            if (gameMode == GameMode.NORMAL || gameMode == GameMode.ADVENTURE)
                return MIN_Y;
            else
                return Y_OFFSET + MOVE_Y; }
    }

    public static int MaxScreenY
    {
        get {
            if (gameMode == GameMode.NORMAL || gameMode == GameMode.ADVENTURE)
                return MAX_Y;
            else if (gameMode == GameMode.LAVA)
               return MAX_Y;
            else
                return Y_OFFSET + STAGE_SIZE_Y + MOVE_Y; }
    }

#endregion

    public GameState state = GameState.NONE;
    public static GameMode gameMode = GameMode.NORMAL;
    public static AdventureMode adventureMode = AdventureMode.ORIGIN;

    public void ChangeAdventureMode(AdventureMode tempAdventureMode)
    {
        GameManager.adventureMode = tempAdventureMode;

        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
        {
            GameUIManager.instance.sandboxLabel.gameObject.SetActive(true && Global._instance.ShowIngameInfo);
            GameUIManager.instance.sandboxLabel.text = adventureMode.ToString();
        }
    }

    /*
    public Vector3 mMousePosition = Vector3.zero;
    public Vector3 mMouseDeltaPos = Vector3.zero;
    public bool _touching = false;
    public int _touchCount = 0;
    int fingerId = -1;
    */

    public bool IsCanTouch = true;

    public GameObject inGameHelpButtonObj;
    public GameObject rainStickTestObj;

    public InGameHelpButton inGameHelpButton_UI;

    private EncValue _moveCount = new EncValue();
    public int moveCount
    {
        get { return _moveCount.Value; }
        set { _moveCount.Value = value; }
    }

    //현재 플레이중인 스테이지 번호
    private int currentStage = 0;
    public int CurrentStage
    {
        get { return currentStage; }
    }

    public int touchCount = 999;

    //클리어연출
    public GameObject StageClearSprite;
    public GameObject boniBirdObj;
    SkeletonAnimation spineboniBird;

    public bool SkipIngameClear = false; //인게임 클리어 스킵

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

    //컨티뉴 관련.
    public GameFailType failType = GameFailType.TURN;
    public bool gameOverByDynamite = false;
    public bool useNPUContinue = false;

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

    //현재 이벤트에서 에코피 이벤트 진행중인지.
    public bool isRunningEcopiEvent = false;

    //폭탄 방향 전환 
    [System.NonSerialized]
    public bool lineBombRotate = true;

    //스킵관련 
    public int ClearBombCount = 0;

    //부스팅 이벤트 관련.
    public int boostingStep = 0;

    //레벨조정기능?
    public int LevelAdjusted { get; private set; } = 0;

    //인게임 테마
    public int ingameBGIndex = 0;

    #region 랜덤 seed 관리
    public System.Random ingameRandom;
    public int ingameSeed = 0;
    #endregion

    //광고로 획득한 턴 데이터
    public int addTurnCount_ByAD = 0;

    //클리어 처리 이후 서버 반환값 저장
    [System.NonSerialized]
    public Protocol.GameClearRespBase clearResp = null;

    //컨티뉴 의사 재확인 이후 컨티뉴를 한 횟수
    [System.NonSerialized]
    public int continueReconfirmCount = 0;

    void Start()
    {
        Global.timeScalePuzzle = 1f;
        if (EditManager.instance == null)
        {
            GameStart();
            ServiceSDK.GrowthyCusmtomLogHelper.SendStartGrowthyLog();
            
            staticCamera = CameraEffect.MakeScreenEffect(1);
            staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0f), 0f, false);
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

        //컨티뉴 의사 재확인 여부 초기화
        UIPopupContinue.IsContinuReconfirm = false;

        if (EditManager.instance == null)
        {
            currentStage = Global.stageIndex;
            ingameSeed = ServerRepos.IngameSeed <= 0 ? 0 : ServerRepos.IngameSeed;
            ingameRandom = new System.Random(ingameSeed);

            StartCoroutine(CoInGameStart());

            if (Global._instance.showInGameClearBTN)
                inGameHelpButton_UI = Instantiate(inGameHelpButtonObj).GetComponent<InGameHelpButton>();
        }
        else
        {
            ingameSeed = 0;
            ingameRandom = new System.Random(ingameSeed);

            SetIngameBGIndex();
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
            {
                AdventureManager.instance.InitStage();
            }
            else if (GameManager.gameMode == GameMode.COIN)
            {
                GameUIManager.instance.SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.APPEAR);
            }
        }
    }

    private void SetIngameBGIndex()
    {
        ingameBGIndex = Global.GameInstance.GetIngameBGIndex();
    }

    IEnumerator CoInGameStart()
    {
        if (EditManager.instance != null)
        {
            EditManager.instance.GameStart();
        }
        else
        {
            string stageKey = Global.GameInstance.GetStageKey();
            string stageName = Global.GameInstance.GetStageFilename();

            using( UnityWebRequest www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName) )
            {
                yield return www.SendWebRequest();

                if (!www.IsError() && www.downloadHandler != null)
                {
                    StringReader reader = new StringReader(www.downloadHandler.text);
                    StageMapData tempData = null;
                    try
                    {
                        var serializer = new XmlSerializer(typeof(StageMapData));
                        tempData = serializer.Deserialize(reader) as StageMapData;

                        string key = StageHelper.filekey;
                        key = key.Insert(4, stageKey);
                        key = key.Substring(0, StageHelper.filekey.Length);
                        string sdeData = StageHelper.Decrypt256(tempData.data, key);

                        StringReader readerStage = new StringReader(sdeData);
                        var serializerStage = new XmlSerializer(typeof(StageInfo));
                        ManagerBlock.instance.stageInfo = serializerStage.Deserialize(readerStage) as StageInfo;
                    }
                    catch (System.Exception e)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("암호화 되지 않은 맵을 읽는중입니다! - 실제 기기에서는 에러가 날 수 있습니다");
                        string enckey = StageHelper.filekey;
                        enckey = enckey.Insert(4, stageKey);
                        enckey = enckey.Substring(0, StageHelper.filekey.Length);

                        string encrypted = StageHelper.AESEncrypt256(reader.ReadToEnd(), enckey);

                        MemoryStream memoryStream = new MemoryStream(www.downloadHandler.data);
                        var stageInfoSerializer = new XmlSerializer(typeof(StageInfo));
                        www.Dispose();
                        memoryStream.Position = 0;
                        var openStage = stageInfoSerializer.Deserialize(memoryStream) as StageInfo;

                        StageMapData stageData = new StageMapData();
                        stageData.version = 0;
                        stageData.moveCount = openStage.turnCount;
                        stageData.collectCount = openStage.collectCount;
                        stageData.collectType = openStage.collectType;
                        stageData.collectColorCount = openStage.collectColorCount;
                        stageData.gameMode = openStage.gameMode;
                        stageData.isHardStage = openStage.isHardStage;
                        stageData.data = encrypted;

                        if (openStage.gameMode == (int)GameMode.ADVENTURE)
                        {
                            stageData.bossIdx = openStage.bossInfo.idx;
                            stageData.bossAttr = openStage.bossInfo.attribute;
                        }
                        tempData = stageData;
                        ManagerBlock.instance.stageInfo = openStage;
#endif
                    }
                }
                else if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                {
                    stageKey = "pp1.xml";
                    stageName = Global.GetHashfromText("pp1.xml") + ".xml";

                    using (var defaultMapReq = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName) )
                    {
                        yield return defaultMapReq.SendWebRequest();

                        if ( !defaultMapReq.IsError() && defaultMapReq.downloadHandler != null )
                        {
                            StringReader reader = new StringReader(defaultMapReq.downloadHandler.text);
                            StageMapData tempData = null;
                            try
                            {
                                var serializer = new XmlSerializer(typeof(StageMapData));
                                tempData = serializer.Deserialize(reader) as StageMapData;

                                string key = StageHelper.filekey;
                                key = key.Insert(4, stageKey);
                                key = key.Substring(0, StageHelper.filekey.Length);
                                string sdeData = StageHelper.Decrypt256(tempData.data, key);

                                StringReader readerStage = new StringReader(sdeData);
                                var serializerStage = new XmlSerializer(typeof(StageInfo));
                                ManagerBlock.instance.stageInfo = serializerStage.Deserialize(readerStage) as StageInfo;
                            }
                            catch (System.Exception e)
                            {

                            }
                        }
                    }
                }
                yield return null;
            }
        }

        //인게임에서 사용할 아틀라스 생성.
        yield return CoMakeIngameAtlas();

        //기믹 튜토리얼에서 사용할 정보 세팅.
        if (Global.GameInstance.GetProp(GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL) == true)
        {
            if (ManagerGimmickTutorial.instance != null)
                StartCoroutine(ManagerGimmickTutorial.instance.CoInitTutorialData());
        }

        PlayerPrefs.SetString("LastStageName", "");

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        //최초플레이확인
        firstPlay = Global.GameInstance.IsFirstPlay();

        //게임 타입별로 스테이지 초기화
        Global.GameInstance.InitStage();
        Global.GameInstance.SetEventDataOnStageStart();

        //인게임 배경 설정
        SetIngameBGIndex();

        //마지막스테이지 넣기
        GrowthyAfterStage = myProfile.stage-1;

        //블럭움직임 관련 정보입력
        //스테이지 크기 정렬, 정보
        ManagerBlock.instance.blockMove = false; //블럭정지
        GameManager.instance.IsCanTouch = true;
        yield return null;

        //기타 설정초기화
        GameItemManager.useCount = new int[8]{ 0, 0, 0, 0, 0, 0, 0, 0 };
        useContinueCount = 0;
        failType = GameFailType.TURN;
        gameOverByDynamite = false;
        addTurnCount_ByAD = Global.GameInstance.GetTurnCount_UseAD_AddTurn();

        ManagerBlock.instance.RefreshBlockManagerByStageData();
        yield return null;
        Pick.instance.InitPick();
        yield return null;

        GameUIManager.instance.GameStart();
        yield return null;

        //스파인한번불러오기
        spineboniBird = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, boniBirdObj).GetComponent<SkeletonAnimation>();
        spineboniBird.gameObject.transform.localScale = Vector3.zero;
        Destroy(spineboniBird.gameObject);

        spineboniBird = null;
        yield return null;

        yield return Global.GameInstance.GameModeProcess_OnIngameStart();

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

        ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eTopUI);

        float waitTimer = 0;
        while (waitTimer < 0.2f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        yield return Global.GameInstance.SetBGM_OnIngameStart(this);
        
        //배경음악 시작
        ManagerSound._instance.PlayBGM();

        //로딩화면 제거
        SceneLoading.Release();

        // 용암,땅파기 화면이동하기
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

        //게임 타입 별, 인게임 시작할 때 연출 등장(목표 팝업 등장 전)
        yield return Global.GameInstance.CoActionIngameStart_BeforeOpenTargetPopup();

        //목표 팝업 보여주고 숨기기
        waitTimer = 0;
        while (waitTimer < 0.3f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        if (Global.GameInstance.IsStageTargetHidden() == false)
        {
            ManagerUI._instance.OpenPopupStageTarget(Global.GameInstance.GetStageTargetText());

            waitTimer = 0;
            while (waitTimer < 1.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        //월드 랭킹 연출
        yield return CoStartWorldRank();
        //엔드 컨텐츠 연출
        yield return CoStartEndContents();

        bool isCanUseReadyItem = Global.GameInstance.GetProp(GameTypeProp.CAN_USE_READYITEM);

        //턴 카운트 증가 연출
        int appleCount = (isCanUseReadyItem == true) ? Global.GameInstance.GetReadyItem_AddTurnCount() : 0;
        appleCount += GameManager.instance.addTurnCount_ByAD;
        yield return Global.GameInstance.CoApplyReadyItem_AddTurn(appleCount);

        //레디 아이템 사용할 수 있는 모드에서 레디 아이템 적용
        if (isCanUseReadyItem == true)
        {   
            yield return Global.GameInstance.CoApplyReadyItemBomb();
        }

        //부스팅 체크 & 레벨 적용
        SetBoostingItem();

        ManagerBlock.instance.useSameColor = false;
        ManagerBlock.instance.SameColorDiffcult = 0;   
        ManagerBlock.instance.SameColorProb = 0;

        string sandBoxText = "";

        if(ServerRepos.IngameLevelAdjust > 0)
        {
            SetLevelAdjust(ServerRepos.IngameLevelAdjust);

            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                Debug.Log(" === 1. PlayCount 그로씨에서 현재 스테이지 알아옴 ");
                GameUIManager.instance.sandboxLabel.gameObject.SetActive(true && Global._instance.ShowIngameInfo);
                sandBoxText = 
                    $"레벨조정{ServerRepos.IngameLevelAdjust} 적용됨/ 결제횟수:{ServerRepos.User.purchaseCnt}/" +
                    $"패키지구매수:{ServerRepos.UserShopPackages?.Count}/" + 
                    $"플레이횟수:{Global.GameInstance.GetGrowthyPlayCount()}";

                if (IsCanAdjustCBUStageLevel())
                {
                    sandBoxText += $"/CBU Stage:{ServerRepos.UserComeback.stage}";
                }
                    
                GameUIManager.instance.sandboxLabel.text = sandBoxText;
            }
        }

        if ( NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
        {
            if( ingameSeed != 0)
            {
                GameUIManager.instance.sandboxLabel.gameObject.SetActive(true && Global._instance.ShowIngameInfo);
                sandBoxText += "\n [ Seed : " + ingameSeed + " ]";
                GameUIManager.instance.sandboxLabel.text = sandBoxText;
            }

            if( Global.GameType == GameType.WORLD_RANK)
            {
                GameUIManager.instance.sandboxLabel.gameObject.SetActive(true && Global._instance.ShowIngameInfo);
                sandBoxText += $"\nMap: {Global.GameInstance.GetStageFilename()}";
                GameUIManager.instance.sandboxLabel.text = sandBoxText;
            }
            
        }

        //튜터리얼불러오기
        //스테이지번호와 최초 플레이 체크
        if (Global.GameType == GameType.NORMAL)
        {
            if (IsCanPlayTutorialStage() == true)
            {
                TutorialType tutoType = (TutorialType)ManagerBlock.instance.stageInfo.tutorialIndex;
                bool isPlayTutorial = true;

                #region 튜토리얼 플레이 조건 체크, 그로씨 처리
                switch (tutoType)
                {
                    case TutorialType.TutorialInGameItem:
                        {
                            if (ServerRepos.UserItem.InGameItem(0) <= 0)
                                isPlayTutorial = false;
                            else
                            {
                                //그로씨
                                ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);

                                if (dataServer.play == 1)
                                {
                                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                    (
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                                        "InGameItem1",
                                        "InGameItemHAMMER",
                                        ServerRepos.UserItem.InGameItem(0),
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL,
                                        "TutorialInGameItem"
                                    );
                                    var doc = JsonConvert.SerializeObject(useReadyItem);
                                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                                }
                            }
                        }
                        break;

                    case TutorialType.TutorialCrossHammer:
                        {
                            if (ServerRepos.UserItem.InGameItem(1) <= 0)
                                isPlayTutorial = false;
                            else
                            {
                                ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
                                if (dataServer.play == 1)
                                {
                                    var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                                        (
                                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                                            "InGameItem2",
                                            "InGameItemCROSS_LINE",
                                            ServerRepos.UserItem.InGameItem(1),
                                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL
                                        );
                                    var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                                }
                            }
                        }
                        break;

                    case TutorialType.TutorialPowerHammer:
                        {
                            if (ServerRepos.UserItem.InGameItem(2) <= 0)
                                isPlayTutorial = false;
                            else
                            {
                                ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
                                if (dataServer.play == 1)
                                {
                                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                    (
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                                        "InGameItem3",
                                        "InGameItemTHREE_HAMMER",
                                        ServerRepos.UserItem.InGameItem(2),
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL,
                                        "TutorialInGameItem"
                                    );
                                    var doc = JsonConvert.SerializeObject(useReadyItem);
                                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                                }
                            }
                        }
                        break;
                    case TutorialType.TutorialRainbowHammer:
                        {
                            if (ServerRepos.UserItem.InGameItem(3) <= 0)
                                isPlayTutorial = false;
                            else
                            {
                                ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex);
                                if (dataServer.play == 1)
                                {
                                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                    (
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                                        "InGameItem4",
                                        "InGameItemRAINBOW_BOMB_HAMMER",
                                        ServerRepos.UserItem.InGameItem(3),
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL,
                                        "TutorialInGameItem"
                                    );
                                    var doc = JsonConvert.SerializeObject(useReadyItem);
                                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                                }
                            }
                        }
                        break;

                    case TutorialType.TutorialColorBrush:
                    {
                        if (ServerRepos.UserItem.InGameItem(4) <= 0)
                        {
                            isPlayTutorial = false;
                        }
                        else
                        {
                            var dataServer = ServerRepos.FindUserStage(Global.stageIndex);
                            if (dataServer.play == 1)
                            {
                                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                (
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                                    "InGameItem5",
                                    "InGameItemCOLOR_BRUSH",
                                    ServerRepos.UserItem.InGameItem(4),
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL,
                                    "TutorialInGameItem"
                                );
                                var doc = JsonConvert.SerializeObject(useReadyItem);
                                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                            }
                        }
                    }
                    break;
                    
                    case TutorialType.TutorialReadyItem_Ingame:
                        {
                            if (CheckReadyItem((int)READY_ITEM_TYPE.ADD_TURN) == false)
                                isPlayTutorial = false;
                        }
                        break;

                    case TutorialType.TutorialReadyScoreUp_Ingame:
                        {
                            if (CheckReadyItem((int)READY_ITEM_TYPE.SCORE_UP) == false)
                                isPlayTutorial = false;
                        }
                        break;
                }
                #endregion

                if (isPlayTutorial == true)
                    ManagerTutorial.PlayTutorial(tutoType);
            }
        }
        else if (Global.GameType == GameType.ADVENTURE)
        {
            if (firstPlay == true)
            {
                if (Global.chapterIndex == 1 && Global.stageIndex == 1
                    && ManagerAdventure.User.GetChapterCursor() == 1 && ManagerAdventure.User.GetStageCursor() == 1)
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGame2Match_Adventure);    //블럭 매치

                else if (Global.chapterIndex == 1 && Global.stageIndex == 2
                    && ManagerAdventure.User.GetChapterCursor() == 1 && ManagerAdventure.User.GetStageCursor() == 2)
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialPotion_Adventure);    //포션

                else if (Global.chapterIndex == 1 && Global.stageIndex == 3
                    && ManagerAdventure.User.GetChapterCursor() == 1 && ManagerAdventure.User.GetStageCursor() == 3)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialHealItem_Adventure);  //인게임 아이템

                    //그로씨
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                       "InGameItem" + ((int)GameItemType.HEAL_ONE_ANIMAL),
                        "InGameItemHEAL_ONE_ANIMAL",
                        1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "TutorialInGameItem"
                    );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
                else if (Global.chapterIndex == GameItemManager.SKILL_HAMMER_OPEN_CHAPTER &&
                         Global.stageIndex == GameItemManager.SKILL_HAMMER_OPEN_STAGE   &&
                         ManagerAdventure.User.GetChapterCursor() == GameItemManager.SKILL_HAMMER_OPEN_CHAPTER &&
                         ManagerAdventure.User.GetStageCursor() == GameItemManager.SKILL_HAMMER_OPEN_STAGE)
                {
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialSkillItem_Adventure); //탐험 인게임 아이템 (스킬 해머)

                    //그로씨
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                        "InGameItem" + ((int)GameItemType.SKILL_HAMMER),
                        "InGameItemSKILL_HAMMER",
                        1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "TutorialInGameItem"
                    );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
            }
        }
        else if (Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            if (firstPlay == true)
            {
                //코인모드 인게임 튜토리얼
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGameRule_CoinStage);
                PlayerPrefs.SetInt("CoinStageTutorial_Ingame", 1);
                while (ManagerTutorial._instance != null)
                {
                    yield return null;
                }
                yield return null;
            }
        }
        yield return null;

        //튜토리얼 등장 후, 게임 타입별로 등장해야하는 액션
        yield return Global.GameInstance.CoActionIngameStart_AfterTutorial();

        playTime = Time.time;
        LoadScene = false;

        ClearBombCount = 0;
        Global.GameInstance.ResetData_AfterGameStartProgress();
        state = GameState.PLAY;
        ManagerBlock.instance.blockMove = true; //블럭정지
        //ManagerBlock.instance.AdventureMatchBlock = true;
        GameUIManager.instance.ShowPauseButton(true);
        yield return null;
    }

    public bool IsCanPlayTutorialStage()
    {
        if (ManagerBlock.instance.stageInfo.tutorialIndex <= 0)
            return false;

        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX  && Global._instance.showTutorialBTN == true)
            return true;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && firstPlay && myProfile.stage <= Global.stageIndex)
            return true;

        return false;
    }

    //유저 부스팅 정보 다시 받아오기
    //게임 종료 시 호출해주기(성공, 실패, 일시정지 - 맵으로 돌아가기)
    //'일시정지 - 재시작' 은 준비 팝업에서 버튼 누를때 갱신해줌
    public void RefreshBoostingUserData()
    {
        if (ManagerNoyBoostEvent.instance != null && ManagerNoyBoostEvent.instance.IsActiveUser())
            ManagerNoyBoostEvent.instance.SyncFromServerUserData();
    }

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
    
    public IEnumerator CoStartEndContents()
    {
        if (ManagerBlock.instance.listEndContentsItem.Count > 0)
        {
            float waitTime = ManagerBlock.instance.GetIngameTime(0.08f);

            for (int i = 0; i < ManagerBlock.instance.listEndContentsItem.Count; i++)
            {
                StartCoroutine(ManagerBlock.instance.listEndContentsItem[i].CoStartEffect());
                yield return new WaitForSeconds(waitTime);
            }

            waitTime = ManagerBlock.instance.GetIngameTime(1f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    //부스팅 이벤트 대상인지 검사하고 아이템 설정하는 함수.
    private void SetBoostingItem()
    {
        if (ManagerNoyBoostEvent.instance == null)
            return;
        
        boostingStep = 0;
        if (ManagerNoyBoostEvent.instance.isBoostOn)
            boostingStep = ManagerNoyBoostEvent.instance.GetBoostStep();
        ManagerNoyBoostEvent.instance.isBoostOn = false;
    }

    public bool IsNoyBoostStage()
    {
        if (ManagerNoyBoostEvent.instance == null)
        {
            return false;
        }

        if (GameData.User == null || GameData.User.stage < ManagerNoyBoostEvent.instance.StartStage || GameData.User.stage > ManagerNoyBoostEvent.instance.EndStage)
        {
            return false;
        }

        if (SDKGameProfileManager._instance == null || Global.stageIndex != SDKGameProfileManager._instance.GetMyProfile().stage)
        {
            return false;
        }

        return boostingStep > 0;
    }

    // cbu 유저 레벨 조정 적용이 가능한지 검사하는 함수
    private bool IsCanAdjustCBUStageLevel()
    {
        if (ServerRepos.UserComeback != null)
        {
            ServerUserComeback combackData = ServerRepos.UserComeback;
            if (combackData.loginCount > 7)
                return false;

            if (combackData.stage <= Global.stageIndex && Global.stageIndex < (combackData.stage + 10))
            {
                Debug.Log(" [ 컴백유저 스테이지 클리어 지원 ] ");
                return true;
            }
        }
        return false;
    }

    private void SetLevelAdjust(int levelProg)
    {
        ManagerBlock.instance.useSameColor = true;
        ManagerBlock.instance.SameColorDiffcult = 4;    // ServerRepos.LoginCdn.StageLevelDownList[i][3];
        ManagerBlock.instance.SameColorProb = levelProg;
        LevelAdjusted = levelProg;
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
        yield break;
        //string name = "ingamedata";
        //IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        //while (e.MoveNext())
        //    yield return e.Current;
        //if (e.Current != null)
        //{
        //    AssetBundle assetBundle = e.Current as AssetBundle;
        //    if (assetBundle != null)
        //    {
        //        GameObject obj = assetBundle.LoadAsset<GameObject>("ingamedata");
        //        if (obj)
        //        {
        //            InGameData_Bundle importInfo = Instantiate(obj).GetComponent<InGameData_Bundle>();
        //            if (importInfo != null)
        //            {
        //                var data = importInfo._bgm;
        //                if (data != null)
        //                {
        //                    ManagerSound._instance._bgmInGame.clip = data;
        //                    loadSuccess = true;
        //                }
        //            }
        //        }
        //    }
        //}

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

        //InGameData_Bundle importInfo = Instantiate(obj).GetComponent<InGameData_Bundle>();
        //if (importInfo != null)
        //{
        //    var data = importInfo._bgm;
        //    if (data != null)
        //        ManagerSound._instance._bgmInGame.clip = data;
        //}
        //else
        //    Debug.LogErrorFormat("LoadFromInternal Failed: ImportData not found in {0}", name);
#endif
    }


    bool CheckReadyItem(int itemType)
    {
        if(Global.GameInstance.CanUseReadyItem(itemType) == true && UIPopupReady.readyItemUseCount[itemType].Value != 0)
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

                        //코인 스테이지 컨티뉴 사용
                        if (isCoinStageContinue == false)
                        {
                            StageClear();
                        }
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
                    else
                    {
                        GameUIManager.instance.ShowTipButton(true); 
                        GameUIManager.instance.ShowPauseButton(true); 
                    }
                }
                else
                {
                    if (checkGameOver())
                    {
                        GameUIManager.instance.ShowTipButton(false);
                        GameUIManager.instance.ShowPauseButton(false); 
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

        if (state == GameState.GAMECLEAR)
            return;
        /*
        if (EditManager.instance != null)
        {
            state = GameState.GAMECLEAR;
            return;
        }
        */

        // 코인 스테이지 컨티뉴 이벤트 사용.
        if(ManagerCoinBonusStage.CheckStartable_Continue() && Global.GameType == GameType.COIN_BONUS_STAGE)
            StartCoroutine(CoCoinStageContinue());
        else if (Global.GameType != GameType.ADVENTURE && Global.GameType != GameType.ADVENTURE_EVENT)
            StartCoroutine(CoStageClear());
        else
            StartCoroutine(CoAdventureClear());
    }

    bool networkEnd = false;
    bool needClassUp = false;


    void recvAdventureGameClear(AdventureGameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            clearResp = resp;
            UIPopupAdventureClear._clearResp = resp;

            Global.GameInstance.OnRecvAdvantureGameClear(this, resp);
            networkEnd = true;  
        }
    }

    void recvTurnRelayWaveClear(TurnRelayGameSaveResp resp)
    {
        if (resp.IsSuccess)
        {
            networkEnd = true;
        }
    }

    void recvTurnRelayInGameClear(TurnRelayGameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            clearResp = resp;
            Global.GameInstance.OnRecvGameClear(this, resp);
            networkEnd = true;
        }
    }

    void recvGameClear(GameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            clearResp = resp;
            UIPopupClear.materialEventGetReward = false;
            Global.GameInstance.OnRecvGameClear(this, resp);

            networkEnd = true;
            //Debug.Log("** GAME CLEAR ok");        

        }
    }

    public int leftMoveCount = 0;   //남은 턴 카운트
    public int useMoveCount = 0;    //사용한 턴 카운트
    public string stageReview = null;


    IEnumerator CoAdventureClear()
    {
        float waitTimer = 0f;
        state = GameState.GAMECLEAR;
        GameUIManager.instance.ShowPauseButton(false);

        while (FlyTarget.flyTargetCount > 0)
        {
            while (waitTimer < 0.1f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        BlockMatchManager.instance.DistroyAllLinker();
        yield return null;

        ManagerSound._instance.StopBGM();
        float delayTime = 0f;

        CloseIngamePopup();

        waitTimer = 0f;
        while (waitTimer < delayTime)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //블럭이 멈출때까지 기다림
        while (!ManagerBlock.instance.checkBlockWait())
        {
            yield return null;
        }

        CloseIngamePopup();

        List<BlockBase> listBlock = new List<BlockBase>();

        waitTimer = 0;
        while (waitTimer < 0.3f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        // 서버와 통신
        networkEnd = false;

        Dictionary<int, int> dicDropBoxes = new Dictionary<int, int>();
        dicDropBoxes.Add(AdventureManager.instance.TreasureType, AdventureManager.instance.TreasureCnt);

        var arg = new AdventureGameClearReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            gameKey = ServerRepos.lastGameKey,
            stage = Global.stageIndex,
            chapter = Global.chapterIndex,
            missionClear = 0,
            coin = ManagerBlock.instance.coins,
            deckId = 1,
            dropBoxes = dicDropBoxes,
            expEvent = ManagerAdventure.instance.ExpEvent,
            seed = ServerRepos.IngameSeed
        };

        ServerAPI.AdventureGameClear(arg, recvAdventureGameClear);

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        CloseIngamePopup();

        // 임시 랭킹 업데이트
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
        }
        else
        {
            UserSelf myProfile = SDKGameProfileManager._instance.GetMyProfile();
            myProfile.SetStage((int)ServerRepos.User.stage);
        }

        ManagerUI._instance.OpenPopupAdventureClear();

        //그로씨 최종스테이지 갱신
        {
            int tempChapterCnt = 0;
            int tempStageCnt = 0;

            for (int i = 0; i < ServerRepos.UserAdventureStages.Count; ++i)
            {
                if (ServerRepos.UserAdventureStages[i].chapter > tempChapterCnt
                    && ServerRepos.UserAdventureStages[i].flag > 0
                    )
                {
                    tempChapterCnt = ServerRepos.UserAdventureStages[i].chapter;
                    tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                }
                else if (tempChapterCnt > 0
                    && ServerRepos.UserAdventureStages[i].chapter == tempChapterCnt
                    && ServerRepos.UserAdventureStages[i].stage > tempStageCnt
                     && ServerRepos.UserAdventureStages[i].flag > 0
                    )
                {
                    tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                }
            }

            if (tempChapterCnt != 0 && tempStageCnt != 0)
            {
                string tempL_STR2 = tempChapterCnt + "_" + tempStageCnt;
                PlayerPrefs.SetString("ADV_STR_NUM", tempL_STR2);
            }
        }

        yield return null;
    }

    private bool isCoinStageContinue = false;
    [HideInInspector] public bool isCoinStageClear = false;
    private IEnumerator CoCoinStageContinue()
    {
        if (isCoinStageContinue) yield break;

        //컨티뉴를 사용할 수 있는 횟수가 남아있는지 검사
        if (ManagerCoinBonusStage.instance.IsCoinStageContinueCountOver()) isCoinStageClear = true;
        
        //코인 스테이지 컨티뉴 사용
        if (isCoinStageClear == false)
        {
            ManagerUI._instance.OpenPopupContinue();
            ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);

            isCoinStageContinue = true;
        }
        
        yield return new WaitUntil(() => UIPopupContinue._instance == null);
        
        if (state == GameState.GAMECLEAR) yield break;
        
        if(isCoinStageClear)
        {
            StartCoroutine(CoStageClear());
        }
    }

    public void InitCoinStageContinue()
    {
        isCoinStageContinue = false;
        isCoinStageClear = false;
    }
    
    //인게임 클리어 처리 - 일반
    public IEnumerator CoStageClear_Default()
    {
        //남은폭탄 처리        
        List<BlockBase> listBlock = new List<BlockBase>();
        BlockBase.uniqueIndexCount++;

        for (int y = MinScreenY; y < MaxScreenY; y++)
        {
            for (int x = MinScreenX; x < MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && block.IsBombBlock() && block.state == BlockState.WAIT && (block.blockDeco == null || block.blockDeco.IsInterruptBlockPang() == false))
                {
                    block.IsSkipPang = true;
                    listBlock.Add(block);
                    block.JumpBlock();
                    block.RemoveLinkerNoReset();
                    block.AddCoin();
                }
            }
        }

        if (listBlock.Count > 0)
        {
            for (int i = 0; i < listBlock.Count; i++)
            {
                listBlock[i].IsSkipPang = false;
                ManagerBlock.instance.AddCoin(1);
                listBlock[i].BlockPang(BlockBase.uniqueIndexCount);

                if (SkipIngameClear == false)
                {
                    InGameEffectMaker.instance.MakeFlyCoin(listBlock[i]._transform.position, 1, isAddCoin: false);
                    listBlock[i].AddCoin(true);
                    yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));
                }
                yield return null;
            }
        }

        yield return null;
        listBlock.Clear();

        //블럭이 멈출때까지 기다림
        yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());

        //잠시 대기
        float waitTimer = 0f;
        while (waitTimer < 0.2f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //남은턴 처리
        //남은턴수계산하기
        BlockBomb._bombUniqueIndex++;

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

        if (moveCount > 0)
        {
            for (int y = MinScreenY; y < MaxScreenY; y++)
            {
                for (int x = MinScreenX; x < MaxScreenX; x++)
                {
                    BlockBase block = PosHelper.GetBlockScreen(x, y);
                    Board tempBoard = PosHelper.GetBoard(x, y);
                    if (block != null && tempBoard != null && tempBoard.HasDecoHideBlock() == false && tempBoard.HasDecoCoverBlock() == false && block.blockDeco == null)
                        if (block.IsNormalBlock() && block.state == BlockState.WAIT && block is NormalBlock)
                            listBlock.Add(block);
                }
            }

            List<BlockBase> pangBlockList = new List<BlockBase>();
            for (int i = 0; i < moveCount; i++)
            {
                int indexS = GameManager.instance.GetIngameRandom(0, listBlock.Count);

                if (listBlock[indexS] != null)
                {
                    listBlock[indexS].IsSkipPang = true;
                    pangBlockList.Add(listBlock[indexS]);
                }
                listBlock.RemoveAt(indexS);
                if (listBlock.Count <= 0)
                    break;
            }
            yield return null;

            if (SkipIngameClear == false)
            {

                for (int i = 0; i < pangBlockList.Count; i++)
                {
                    if (pangBlockList[i] != null)
                    {
                        pangBlockList[i].IsSkipPang = true;
                        pangBlockList[i].bombType = BlockBombType.CLEAR_BOMB;
                        pangBlockList[i].RemoveLinkerNoReset();
                        pangBlockList[i].JumpBlock();
                        pangBlockList[i].AddCoin();

                        if (SkipIngameClear == false)
                        {
                            InGameEffectMaker.instance.MakeLastBomb(pangBlockList[i]._transform.position);
                            ManagerSound.AudioPlay(AudioInGame.LAST_PANG);
                            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
                        }
                    }
                    moveCount--;
                    GameUIManager.instance.RefreshMove();
                    yield return null;
                }

                moveCount = 0;
                GameUIManager.instance.RefreshMove();
                yield return null;

                for (int i = 0; i < pangBlockList.Count; i++)
                {
                    if (pangBlockList[i] != null)
                    {
                        pangBlockList[i].IsSkipPang = false;
                        pangBlockList[i].BlockPang(BlockBomb._bombUniqueIndex);
                        ManagerBlock.instance.AddScore(1000); //남은턴점수추가 나중에 수정
                        ManagerBlock.instance.AddCoin(1);

                        if (SkipIngameClear == false)
                        {
                            InGameEffectMaker.instance.MakeScore(pangBlockList[i]._transform.position, 1000);
                            InGameEffectMaker.instance.MakeFlyCoin(pangBlockList[i]._transform.position, 1, isAddCoin: false);
                            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
                        }
                    }
                    yield return null;
                }

                waitTimer = 0f;
                while (waitTimer < 0.5f && SkipIngameClear == false)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
                yield return null;
            }
            else
            {
                for (int i = 0; i < pangBlockList.Count; i++)
                {
                    if (pangBlockList[i] != null)
                    {
                        pangBlockList[i].IsSkipPang = false;
                        pangBlockList[i].BlockPang(BlockBomb._bombUniqueIndex);
                        ManagerBlock.instance.AddScore(1000); //남은턴점수추가 나중에 수정
                        ManagerBlock.instance.AddCoin(1);
                    }
                }
                moveCount = 0;
                GameUIManager.instance.RefreshMove();
                yield return null;
            }
        }

        //블럭이 멈출 때까지 기다림
        yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());

        //남은 폭탄 한번 더 처리
        listBlock.Clear();
        BlockBase.uniqueIndexCount++;

        for (int y = MinScreenY; y < MaxScreenY; y++)
        {
            for (int x = MinScreenX; x < MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && block.IsBombBlock() && block.state == BlockState.WAIT
                    && block.secendBomb == false
                    && (block.blockDeco == null || block.blockDeco.IsInterruptBlockPang() == false) && block.bombType != BlockBombType.CLEAR_BOMB)
                {
                    block.IsSkipPang = true;
                    listBlock.Add(block);
                    block.JumpBlock();
                    block.RemoveLinkerNoReset();
                    block.AddCoin();
                }
            }
        }

        if (listBlock.Count > 0)
        {
            for (int i = 0; i < listBlock.Count; i++)
            {
                if (listBlock[i] != null)
                {
                    listBlock[i].IsSkipPang = false;
                    listBlock[i].BlockPang(BlockBase.uniqueIndexCount);

                    if (SkipIngameClear == false)
                    {
                        InGameEffectMaker.instance.MakeFlyCoin(listBlock[i]._transform.position, 1, isAddCoin: false);
                        ManagerBlock.instance.AddCoin(1);
                        listBlock[i].AddCoin(true);
                        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));
                    }
                    else
                    {
                        ManagerBlock.instance.AddCoin(1);
                    }
                }
                yield return null;
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
        float eventRatio = Global.GameInstance.GetBonusRatio();   // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
        
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
            scoreRatio = 1.2f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        else
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        

        //GameUIManager.instance.SetScore((int)(tempScoreA * scoreRatio));   //변경된 점수 추가
        int tempScore = (int)(ManagerBlock.instance.score * scoreRatio);

        //꽃점수체크  
        if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_RED) && (tempScore >= (int)(ManagerBlock.instance.stageInfo.score4 * 1.1f)))
        {
            ManagerBlock.instance.flowrCount = 5;
        }
        else if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_BLUE) && (tempScore >= ManagerBlock.instance.stageInfo.score4))
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
        allStageClearReward = 0;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        // 전체 오픈된 스테이지 꽃으로 또는 파란꽃으로 다 클리어 했을떄 보상
        if (Global.GameType == GameType.NORMAL && (myProfile.stage >= ManagerData._instance.maxStageCount && ManagerBlock.instance.flowrCount >= 3))
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

        // 만든 블럭 횟수 
        var usedItems = new List<int>();
        usedItems.Add(ManagerBlock.instance.creatBombCount[0].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[1].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[2].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[3].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[0].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[1].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[2].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[3].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[4].Value);

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템  
        var paidItems = new List<int>();
        paidItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        paidItems.Add(GameItemManager.useCount[0]);
        paidItems.Add(GameItemManager.useCount[1]);
        paidItems.Add(GameItemManager.useCount[2]);
        paidItems.Add(GameItemManager.useCount[3]);
        paidItems.Add(GameItemManager.useCount[7]);
        paidItems.Add(0); //11
        paidItems.Add(0); //12
        paidItems.Add(0); //13
        paidItems.Add(0); //14
        paidItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
        paidItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

        var gainMaterial = new List<List<int>>();
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_COLLECT_EVENT) && ServerContents.EventChapters.active == 1 && ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT)
        {
            gainMaterial.Add(new List<int>() { ManagerBlock.instance.stageInfo.collectEventType, ManagerBlock.instance.materialCollectEvent });

            //그로씨 생성, 꽃모으기완료시
            {
                CdnEventChapter eventChapter = ServerContents.EventChapters;
                ServerEventStage eventCurrentStage = ServerRepos.EventStages.Find(x => x.eventIdx == Global.eventIndex && x.stage == Global.stageIndex);

                int thisStageMaterialMax = eventChapter.collectMaterials[(Global.stageIndex - 1) * 2 + 1];
                int thisStageCurrentMaterial = 0;
                {
                    for (int i = 1; i < eventChapter.collectMaterials[(Global.stageIndex - 1) * 2 + 1] + 1; i++)
                        if ((eventCurrentStage.materialCnt & (1 << i)) != 0)
                            thisStageCurrentMaterial++;
                }

                if( thisStageCurrentMaterial < thisStageMaterialMax && thisStageMaterialMax <= thisStageCurrentMaterial + ManagerBlock.instance.materialCollectEvent)
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                                    (
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.COLLECT_MATERIAL,
                                        $"{Global.eventIndex}_{Global.stageIndex}",
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                                    );
                    achieve.L_NUM1 = eventCurrentStage.play;
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                }


                int totalPlayCount = 0;
                int maxCount = 0;
                for (int i = 1; i < eventChapter.collectMaterials.Count; i += 2)
                    maxCount += eventChapter.collectMaterials[i];

                int getCount = 0;

                foreach (var item in ServerRepos.EventStages)
                    if (item.eventIdx == Global.eventIndex)
                    {
                        for (int i = 1; i < eventChapter.collectMaterials[(item.stage - 1) * 2 + 1] + 1; i++)
                            if ((item.materialCnt & (1 << i)) != 0)
                                getCount++;

                        totalPlayCount += item.play;
                    }

                if (getCount < maxCount && getCount + ManagerBlock.instance.materialCollectEvent >= maxCount)
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                                    (
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.COLLECT_MATERIAL,
                                        Global.eventIndex.ToString(),
                                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                                    );
                    achieve.L_NUM1 = totalPlayCount;
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                }
            }
        }

        if (Global.GameType == GameType.WORLD_RANK)
        {
            gainMaterial.Add(new List<int>() { ManagerBlock.instance.worldRankingItemCount * ManagerBlock.instance.flowrCount });
        }
        else if (Global.GameType == GameType.END_CONTENTS)
        {
            gainMaterial.Add(new List<int>() { ManagerBlock.instance.endContentsItemCount * ManagerEndContentsEvent.instance.GetScoreRatio()[ManagerBlock.instance.flowrCount - 1]});
        }

        var bonusItem = new List<int>();
        var getCoinA = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;
        int tempGainEventItems = Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_SPECIAL_EVENT) ? Global.specialEventIndex : 0;

        #region 알파벳 이벤트
        List<int> listGainAlphabet_N = new List<int>();
        int gainAlphabet_S = 0;
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == true && ManagerAlphabetEvent.instance != null
            && ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == true)
        {
            //현재 스테이지에서 획득한 일반 알파벳의 카운트를 리스트 형태로 넘겨줌.
            listGainAlphabet_N = ManagerAlphabetEvent.alphabetIngame.GetListCurrentGainCount_All_N();

            //스페셜 알파벳 획득한 경우에만 데이터 넣어줌.
            if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == true
                && ManagerAlphabetEvent.alphabetIngame.IsGainAlphabet_S() == true)
                gainAlphabet_S = ManagerAlphabetEvent.alphabetIngame.Alphabet_S;
        }
        #endregion

        #region 광고 데이터 추가
        var listADItems = new List<int>();
        int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (int i = 0; i < itemCount; i++)
            listADItems.Add(0);

        if (GameManager.instance.addTurnCount_ByAD > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        #endregion

        networkEnd = false;

        int tempStageIndex = Global.stageIndex;
        int tempEventIndex = Global.eventIndex;
        int tempGameType = (int)Global.GameType;

        var arg = new GameClearReq()
        {
            stage = tempStageIndex,
            eventIdx = tempEventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            type = tempGameType,    //게임타입
            gameFlower = ManagerBlock.instance.flowrCount,
            gameCoin = getCoinA,//ManagerBlock.instance.coins,
            gameScore = tempScore,//ManagerBlock.instance.score,
            gameTurn = leftMoveCount,
            usedTurn = useMoveCount,
            items = usedItems,
            adItems = listADItems,
            missionClear = clearMission,
            allStageClear = allStageClearReward,
            missions = new List<int> { getCandy, 0, getDuck, 0 },
            playSec = (int)(Time.time - playTime),
            gainMaterials = gainMaterial,
            gainEventItems = new List<int> { tempGainEventItems, ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio },
            eventMaterial = ManagerBlock.instance.materialCollectPos,
            gainAlphabets = listGainAlphabet_N,
            gainSpecial = gainAlphabet_S,
            paidItems = paidItems,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = LevelAdjusted > 0 ? 1 : 0,
            unlimitedFlower = Global.GameInstance.CalcFlowerLevel(false, tempScore),
            singleRoundEvent = Global.isSingleRoundEvent? 1 : 0,
            inGameItem = bonusItem,
        };

        if (SceneManager.GetActiveScene().name == "InGameTool")
        {
            networkEnd = true;
        }
        else
        {
            Debug.Log("게임 클리어 처리");
            ServerAPI.GameClear(arg, recvGameClear);
            Global.SetIsClear(true);
        }

        GameUIManager.instance.ShowSkipBtn(false);
        Pick.instance.SetBoxCollider(true);

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        CloseIngamePopup();

        if (spineboniBird != null && spineboniBird.AnimationName != "end")
        {
            spineboniBird.state.ClearTracks();
            spineboniBird.state.SetAnimation(0, "end", false);
            spineboniBird.state.Complete += delegate
            {
                Destroy(spineboniBird.gameObject);
            };
        }

        waitTimer = 0f;
        while (waitTimer < 1f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        // 임시 랭킹 업데이트
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
        }
        else
        {
            myProfile.SetStage((int)ServerRepos.User.stage);
        }


        if (firstClear)
        {
            if (Global.eventIndex == 0)
            {
                var GetClover = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                    0,
                    1,
                    0,
                    (int)(ServerRepos.User.star),
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()

                    );
                var docStar = JsonConvert.SerializeObject(GetClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docStar);
            }

            {
                var GetHeart = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                    0,
                    1,
                    0,//(int)(ServerRepos.User.clover),
                    (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                var doc = JsonConvert.SerializeObject(GetHeart);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
            }
        }

        if (SceneManager.GetActiveScene().name != "InGameTool")
        {
            var getCoin = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

            var GetCoin = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
                0,
                getCoin,
                (int)(ServerRepos.User.coin),
                (int)(ServerRepos.User.fcoin),
                mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                );
            var docCoin = JsonConvert.SerializeObject(GetCoin);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCoin);

            if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_COLLECT_EVENT) && 
                ServerContents.EventChapters.active == 1 && 
                ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT &&
                ManagerBlock.instance.materialCollectEvent != 0
                )
            {
                var GetMaterial = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                          "MATERIAL_" + ManagerBlock.instance.stageInfo.collectEventType,
                          "material",
                          ManagerBlock.instance.materialCollectEvent,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                          $"EVENT_{ServerContents.EventChapters.index}_STAGE_{Global.stageIndex}"
                      );
                var docGetMaterial = JsonConvert.SerializeObject(GetMaterial);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docGetMaterial);
            }
        }
    }

    //단계 클리어 했을 때 공통으로 처리되는 부분 - 턴 릴레이
    public IEnumerator CoWaveClear_TurnRelay()
    {
        //남은 턴 저장
        ManagerTurnRelay.turnRelayIngame.RemainTurn = moveCount;

        //총 이벤트 포인트 증가
        int eventPoint_CurrentStage = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1;
        ManagerTurnRelay.turnRelayIngame.IngameEventPoint += eventPoint_CurrentStage;

        //누적 플레이 시간 갱신
        ManagerTurnRelay.turnRelayIngame.TotalPlayTime += (int)(Time.time - playTime);

        //누적 컨티뉴 횟수 갱신
        ManagerTurnRelay.turnRelayIngame.TotalContinueCount += GameManager.instance.useContinueCount;

        //누적 폭탄 제작 & 인게임 아이템 사용 카운트 갱신
        ManagerTurnRelay.turnRelayIngame.AddTotalMakeBombCount();
        ManagerTurnRelay.turnRelayIngame.AddTotalUseIngameItemCount();

        //버튼 및 터치 비활성화
        GameUIManager.instance.ShowSkipBtn(false);
        Pick.instance.SetBoxCollider(true);

        //현재 웨이브 상태에 따라 저장/클리어 처리
        if (ManagerTurnRelay.turnRelayIngame.CurrentWave < ManagerTurnRelay.instance.MaxWaveCount)
            yield return CoStageSave_TurnRelay();
        else
            yield return CoStageClear_TurnRelay();

        //인게임 팝업 닫기
        CloseIngamePopup();

        //결과 포코타&파랑새 제거
        if (spineboniBird != null && spineboniBird.AnimationName != "end")
        {
            spineboniBird.state.ClearTracks();
            spineboniBird.state.SetAnimation(0, "end", false);
            spineboniBird.state.Complete += delegate
            {
                Destroy(spineboniBird.gameObject);
            };
        }

        float waitTimer = 0f;
        while (waitTimer < 1f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
    }
    
    /// <summary>
    /// 인게임 클리어 처리 : 우주여행
    /// </summary>
    public IEnumerator CoClear_SpaceTravel()
    {
        // 남은 턴 수 계산 : 인게임 동작에는 사용되지 않으나, 서버 데이터 및 그로시 전송을 위함
        if (gameMode == GameMode.NORMAL || gameMode == GameMode.DIG)
        {
            leftMoveCount = moveCount;
        }
        if (gameMode == GameMode.LAVA)
        {
            var addLine = 1;
            for (var i = MIN_X; i < MAX_X; i++)
            {
                var tempBoard = PosHelper.GetBoard(i, Y_OFFSET + STAGE_SIZE_Y - 2 + MOVE_Y - ManagerBlock.instance.LavaLevelCount);
                if (tempBoard != null && tempBoard.IsActiveBoard)
                {
                    if (tempBoard.lava != null)
                    {
                        addLine = 0;
                        break;
                    }
                }
            }

            moveCount     = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount - 2 + addLine);
            leftMoveCount = 2 * (MOVE_Y + 9 - ManagerBlock.instance.LavaLevelCount - 2 + addLine);

            if (leftMoveCount < 0)
            {
                leftMoveCount = 0;
                moveCount     = 0;
            }
        }
        
        // 점수 계산 : 인게임 UI에는 사용되지 않으나, 서버 데이터 및 그로시 전송을 위함
        float scoreRatio = 1;
        var eventRatio = Global.GameInstance.GetBonusRatio();
        scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f + eventRatio * 0.01f;
        var tempScore = (int)(ManagerBlock.instance.score * scoreRatio);

        if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_RED) && (tempScore >= (int)(ManagerBlock.instance.stageInfo.score4 * 1.1f)))
        {
            ManagerBlock.instance.flowrCount = 5;
        }
        if ((GameUIManager.instance.maxType_flowerState >= ScoreFlowerType.FLOWER_BLUE) && (tempScore >= ManagerBlock.instance.stageInfo.score4))
        {
            ManagerBlock.instance.flowrCount = 4;
        }
        if (tempScore >= ManagerBlock.instance.stageInfo.score3)
        {
            ManagerBlock.instance.flowrCount = 3;
        }
        if (tempScore >= ManagerBlock.instance.stageInfo.score2)
        {
            ManagerBlock.instance.flowrCount = 2;
        }
        if (tempScore >= ManagerBlock.instance.stageInfo.score1)
        {
            ManagerBlock.instance.flowrCount = 1;
        }
        yield return null;

        allStageClearReward = 0;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        // 만든 블럭 횟수
        var usedItems = new List<int>();
        usedItems.Add(ManagerBlock.instance.creatBombCount[0].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[1].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[2].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[3].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[0].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[1].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[2].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[3].Value);
        usedItems.Add(ManagerBlock.instance.removeBlockCount[4].Value);

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템  
        var paidItems = new List<int>();
        paidItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        paidItems.Add(GameItemManager.useCount[0]);
        paidItems.Add(GameItemManager.useCount[1]);
        paidItems.Add(GameItemManager.useCount[2]);
        paidItems.Add(GameItemManager.useCount[3]);
        paidItems.Add(GameItemManager.useCount[7]);
        paidItems.Add(0);
        paidItems.Add(0);
        paidItems.Add(0);
        paidItems.Add(0);
        paidItems.Add(UIPopupReady.readyItemUseCount[6].Value);
        paidItems.Add(UIPopupReady.readyItemUseCount[7].Value);

        
        // 현재 남은 보너스 아이템
        var bonusItem = new List<int>();
        bonusItem.Add(ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.LINE_BOMB]);
        bonusItem.Add(ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.CIRCLE_BOMB]);
        bonusItem.Add(ManagerSpaceTravel.instance.selectItemDic[ManagerSpaceTravel.BonusItemType.RAINBOW_BOMB]);

        // 우주여행에서 얻을 수 있는 재료, 알파벳은 없기 때문에 빈 리스트 반환
        var getCoinA = 0;
        var tempGainEventItems = 0;
        var gainMaterial = new List<List<int>>();
        var listADItems = new List<int>();
        var itemCount   = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (var i = 0; i < itemCount; i++)
            listADItems.Add(0);
        if (addTurnCount_ByAD > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        var listGainAlphabetN = new List<int>();
        var gainAlphabetS = 0;

        networkEnd = false;

        var tempStageIndex = Global.stageIndex;
        var tempEventIndex = Global.eventIndex;
        var tempGameType = (int)Global.GameType;

        var arg = new GameClearReq()
        {
            stage = tempStageIndex,
            eventIdx = tempEventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            type = tempGameType,
            gameFlower = ManagerBlock.instance.flowrCount,
            gameCoin = getCoinA,
            gameScore = tempScore,
            gameTurn = leftMoveCount,
            usedTurn = useMoveCount,
            items = usedItems,
            adItems = listADItems,
            missionClear = clearMission,
            allStageClear = allStageClearReward,
            missions = new List<int> { getCandy, 0, getDuck, 0 },
            playSec = (int)(Time.time - playTime),
            gainMaterials = gainMaterial,
            gainEventItems = new List<int> { tempGainEventItems, ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio },
            eventMaterial = ManagerBlock.instance.materialCollectPos,
            gainAlphabets = listGainAlphabetN,
            gainSpecial = gainAlphabetS,
            paidItems = paidItems,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = LevelAdjusted > 0 ? 1 : 0,
            unlimitedFlower = Global.GameInstance.CalcFlowerLevel(false, tempScore),
            singleRoundEvent = Global.isSingleRoundEvent? 1 : 0,
            inGameItem = bonusItem,
        };

        if (SceneManager.GetActiveScene().name == "InGameTool")
        {
            networkEnd = true;
        }
        else
        {
            Debug.Log("게임 클리어 처리");
            ServerAPI.GameClear(arg, recvGameClear);
            Global.SetIsClear(true);
        }

        GameUIManager.instance.ShowSkipBtn(false);
        Pick.instance.SetBoxCollider(true);

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        CloseIngamePopup();

        if (spineboniBird != null && spineboniBird.AnimationName != "end")
        {
            spineboniBird.state.ClearTracks();
            spineboniBird.state.SetAnimation(0, "end", false);
            spineboniBird.state.Complete += delegate
            {
                Destroy(spineboniBird.gameObject);
            };
        }
        
        var waitTimer = 0f;
        while (waitTimer < 1f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        
        // 우주여행 모드의 경우 클리어 시 항상 클로버를 얻는 사양이기 때문에 해당 그로시 전송
        var getClover = new ServiceSDK.GrowthyCustomLog_Money
        (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
            0,
            1,
            0,
            (int)(ServerRepos.User.AllClover),
            mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
        );
        var doc = JsonConvert.SerializeObject(getClover);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
    }

    //인게임 클리어 처리 - 턴 릴레이
    public IEnumerator CoStageSave_TurnRelay()
    {
        //만든 블럭 횟수 
        var usedItems = new List<int>();
        usedItems.Add(ManagerBlock.instance.creatBombCount[0].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[1].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[2].Value);
        usedItems.Add(ManagerBlock.instance.creatBombCount[3].Value);
    
        //인게임 아이템 사용 횟수
        var usedIngameItems = new List<int>();
        usedIngameItems.Add(GameItemManager.useCount[0]);
        usedIngameItems.Add(GameItemManager.useCount[1]);
        usedIngameItems.Add(GameItemManager.useCount[2]);
        usedIngameItems.Add(GameItemManager.useCount[3]);
        usedIngameItems.Add(GameItemManager.useCount[7]);

        //현재 보너스 아이템 보유 현황
        Dictionary<int, int> dicBonusItem = new Dictionary<int, int>();
        foreach (var item in ManagerTurnRelay.turnRelayIngame.dicBonusItem_Gain)
        {
            dicBonusItem.Add((int)item.Key, item.Value);
        }

        var arg = new TurnRelaySaveGameReq()
        {
            eventIdx = Global.eventIndex,
            stage = ManagerTurnRelay.turnRelayIngame.CurrentWave,
            gameKey = ServerRepos.lastGameKey,
            turn = ManagerTurnRelay.turnRelayIngame.RemainTurn,
            score = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1,
            bonusItems = dicBonusItem,
            bonusItemSelected = (int)ManagerTurnRelay.turnRelayIngame.SelectBonusItemType,
            usedItems = usedItems,
            usedIngameItems = usedIngameItems,
            usedContinueCount = GameManager.instance.useContinueCount,
            playSec = (int)(Time.time - playTime),
        };

        ServerAPI.TurnRelayGameSave(arg, recvTurnRelayWaveClear);

        // 네트워크 통신이 완료될때까지 기다림
        yield return new WaitUntil(() => networkEnd);
    }

    public IEnumerator CoStageClear_TurnRelay()
    {
        // 만든 블럭 횟수 
        var usedItems = new List<int>();
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(0));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(1));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(2));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(3));

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템
        var paidItems = new List<int>();
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(0));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(1));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(2));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(3));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(4));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(5));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(0));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(1));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(2));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(3));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(4));
        paidItems.Add(0); //11
        paidItems.Add(0); //12
        paidItems.Add(0); //13
        paidItems.Add(0); //14
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(6)); //15
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(7)); //16

        networkEnd = false;

        int remainTurn = ManagerTurnRelay.turnRelayIngame.RemainTurn;

        //총 점수는, 현재까지 얻은 점수 + 남은 턴을 합친 점수
        int totalScore = ManagerTurnRelay.turnRelayIngame.IngameEventPoint + remainTurn;

        var arg = new TurnRelayGameClearReq()
        {
            chapter = Global.GameInstance.GetChapterIdx(),
            stage = 1,
            eventIdx = Global.eventIndex,
            type = (int)Global.GameType,
            playSec = (int)ManagerTurnRelay.turnRelayIngame.TotalPlayTime,
            gameScore = totalScore,
            gameTurn = remainTurn,
            seed = 0,
            easyMode = 0,
            items = usedItems,
            gameKey = ServerRepos.lastGameKey,
            bonusItemsSelected = ManagerTurnRelay.turnRelayIngame.listTotalBonusItem_Select,
            paidItems = paidItems,
            stageKey = $"TR_1_1.xml",
        };

        ServerAPI.TurnRelayGameClear(arg, recvTurnRelayInGameClear);

        // 네트워크 통신이 완료될때까지 기다림
        yield return new WaitUntil(() => networkEnd);
    }

    IEnumerator CoStageClear()
    {
        Global.timeScalePuzzle = 1f;
        SkipIngameClear = false;

        state = GameState.GAMECLEAR;
        GameUIManager.instance.ShowPauseButton(false);
        GameUIManager.instance.ShowTipButton(false);

        //목표로 날아가는 이펙트가 모두 사라질 때 까지 대기.
        yield return new WaitUntil(() => FlyTarget.flyTargetCount <= 0);

        Pick.instance.SetBoxCollider(false);
        BlockMatchManager.instance.DistroyAllLinker();
        yield return null;

        //스테이지클리어테이타처리
        firstClear = false;

        if (EditManager.instance == null)
        {
            if (Global.GameType == GameType.NORMAL)
            {
                if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && ManagerBlock.instance.flowrCount > 0)
                {
                    firstClear = true;
                }
            }
        }

        ManagerSound._instance.StopBGM();

        if (GameManager.gameMode == GameMode.COIN)
        {
            GameUIManager.instance.SetCoinStageNpcAnimation(ManagerBlock.CoinStageNpcState.Clear);
        }

        //타임오버 연출
        if (Global.GameInstance.IsOpenTimeOverUI() == true)
        {
            ManagerSound.AudioPlay(AudioInGame.TIMEOVER);
            ManagerUI._instance.OpenPopupTimeOver();
            yield return new WaitForSeconds(1.0f);
            ManagerUI._instance.ClosePopUpUI();
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(CoPlaySoundBeforeWaitTime(0.7f, AudioInGame.TIME_CLEAR));
        }
        else
        {
            ManagerSound.AudioPlay(AudioInGame.STAGE_CLEAR);
        }

        GameUIManager.instance.ShowSkipBtn(true);
     
        //클리어 글자등장
        GameObject clearSprite = NGUITools.AddChild(GameUIManager.instance.TopUIRoot, StageClearSprite);
        clearSprite.transform.localPosition = new Vector3(0, 70, 0);

        //열려있는 인게임 팝업 닫기
        CloseIngamePopup();

        //0.5초 대기
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));

        //보니 파랑새등장
        spineboniBird = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, boniBirdObj).GetComponent<SkeletonAnimation>();
        spineboniBird.gameObject.transform.localScale = Vector3.one * 60;
        spineboniBird.GetComponent<MeshRenderer>().sortingOrder = 10;

        spineboniBird.state.SetAnimation(0, "start", false);
        spineboniBird.state.AddAnimation(0, "loop", true, 0f);

        //블럭이 멈출때까지 기다림
        yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());

        float waitTimer = 0f;
        while (waitTimer < 2.0f && SkipIngameClear == false)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //열려있는 인게임 팝업 닫기
        CloseIngamePopup();

        //이벤트 상태에서 터지지 못한 블럭들 제거
        yield return CoPangBlockThatCannotRemoveFromEventState();

        //클리어 직후에 제거되는 타입의 블럭들 제거
        yield return CoDestroyBlockAtStageClear();

        //게임 타입에 따른 클리어 처리.
        yield return Global.GameInstance.CoGameClear();

        //인게임 시간 수정
        Global.timeScalePuzzle = 1f;

        //블라인드 제거
        GameManager.instance.SetClearBlind(true);

        //클리어 팝업 출력
        if (SceneManager.GetActiveScene().name == "InGameTool")
        {
            Debug.Log("게임종료 성공");
        }
        else
        {
            Global.GameInstance.OpenPopupClear();
        }
        yield return null;
    }

    //스테이지 클리어 시, 이벤트 상태에서 터지지 못한 블럭들 제거
    private IEnumerator CoPangBlockThatCannotRemoveFromEventState()
    {
        bool isPangBlock = false;
        for (int y = MinScreenY; y < MaxScreenY; y++)
        {
            for (int x = MinScreenX; x < MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && block.state == BlockState.WAIT)
                {
                    isPangBlock = true;
                    StartCoroutine(block.CoPangThatCannotRemoveFromEventState());
                }
            }
        }

        //제거된 블럭이 있다면 블럭 이동이 멈출 때까지 대기
        if (isPangBlock == true)
            yield return new WaitUntil(() => ManagerBlock.instance.checkBlockWait());
    }

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
    }

    IEnumerator CoPlaySoundBeforeWaitTime(float waitTime, AudioInGame sound)
    {
        yield return new WaitForSeconds(waitTime);
        ManagerSound.AudioPlay(sound);
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
            // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템
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
            usedItems.Add(GameItemManager.useCount[7]);
            usedItems.Add(0); //11
            usedItems.Add(0); //12
            usedItems.Add(0); //13
            usedItems.Add(0); //14
            usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
            usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

            var remainTarget = ManagerBlock.instance.GetListRemainTarget();

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
            else if (Global.GameType == GameType.TURN_RELAY)
            {
                var arg = new GameFailReq()
                {
                    type = (int)Global.GameType,
                    stage = Global.stageIndex,
                    eventIdx = Global.eventIndex,
                    chapter = Global.GameInstance.GetChapterIdx(),
                    gameCoin = 0,
                    gameScore = 0,
                    Remains = remainTarget,
                    stageKey = Global.GameInstance.GetStageKey(),
                    seed = ServerRepos.IngameSeed,
                    easyMode = LevelAdjusted > 0 ? 1 : 0,

                    items = usedItems
                };

                ServerAPI.TurnRelayGameFail(arg, recvTurnRelayGameFail);
            }
            else
            {
                #region 광고 데이터 추가
                var listADItems = new List<int>();
                int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
                for (int i = 0; i < itemCount; i++)
                    listADItems.Add(0);

                if (GameManager.instance.addTurnCount_ByAD > 0)
                {
                    listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
                }
                #endregion

                var arg = new GameFailReq()
                {
                    type = (int)Global.GameType,
                    stage = Global.stageIndex,
                    eventIdx = Global.eventIndex,
                    chapter = Global.GameInstance.GetChapterIdx(),
                    gameCoin = 0,
                    gameScore = 0,
                    Remains = remainTarget,
                    stageKey = Global.GameInstance.GetStageKey(),
                    seed = ServerRepos.IngameSeed,
                    easyMode = LevelAdjusted > 0 ? 1 : 0,

                    items = usedItems,
                    adItems = listADItems,
                };

                ServerAPI.GameFail(arg, recvGameFail);
                Global.SetIsClear(false);

                FailCountManager._instance.SetFail(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex);
            }


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

    public void StageFail(bool isDynamiteFail = false, bool noMoreMoveFail = false)
    {
        CloseIngamePopup();

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
            failType = GameFailType.DYNAMITE;
            GameManager.instance.gameOverByDynamite = true;
        }
        else
        {
            if (gameMode == GameMode.LAVA)
            {
                failType = GameFailType.LAVA;
            }
            else
            {
                failType = GameFailType.TURN;
            }
        }

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
        
        bool ShowContinue = Global.GameInstance.GetProp(GameTypeProp.CAN_CONTINUE);

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
                deckId = 1,
            };

            ServerAPI.AdventureGameFail(arg, recvAdventureGameFail);
        }
        else if (Global.GameType == GameType.TURN_RELAY)
        {
            // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템
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
            usedItems.Add(GameItemManager.useCount[7]);
            usedItems.Add(0); //11
            usedItems.Add(0); //12
            usedItems.Add(0); //13
            usedItems.Add(0); //14
            usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
            usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

            var remainTarget = ManagerBlock.instance.GetListRemainTarget();
            var arg = new GameFailReq()
            {
                type = (int)Global.GameType,
                stage = Global.stageIndex,
                eventIdx = Global.eventIndex,
                chapter = Global.GameInstance.GetChapterIdx(),
                gameCoin = 0,
                gameScore = 0,
                Remains = remainTarget,
                stageKey = Global.GameInstance.GetStageKey(),
                seed = ServerRepos.IngameSeed,
                easyMode = LevelAdjusted > 0 ? 1 : 0,
                items = usedItems
            };

            ServerAPI.TurnRelayGameFail(arg, recvTurnRelayGameFail);
        }
        else
        {
            // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템 
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
            usedItems.Add(GameItemManager.useCount[7]);
            usedItems.Add(0); //11
            usedItems.Add(0); //12
            usedItems.Add(0); //13
            usedItems.Add(0); //14
            usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
            usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

            #region 광고 데이터 추가
            var listADItems = new List<int>();
            int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
            for (int i = 0; i < itemCount; i++)
                listADItems.Add(0);

            if (GameManager.instance.addTurnCount_ByAD > 0)
            {
                listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
            }
            #endregion

            var remainTarget = ManagerBlock.instance.GetListRemainTarget();
            var arg = new GameFailReq()
            {
                type = (int)Global.GameType,
                stage = Global.stageIndex,
                eventIdx = Global.eventIndex,
                chapter = Global.GameInstance.GetChapterIdx(),
                gameCoin = 0,
                gameScore = 0,
                Remains = remainTarget,
                stageKey = Global.GameInstance.GetStageKey(),
                seed = ServerRepos.IngameSeed,
                easyMode = LevelAdjusted > 0 ? 1 : 0,
                items = usedItems,
                adItems = listADItems,
            };

            ServerAPI.GameFail(arg, recvGameFail);
            Global.SetIsClear(false);

            FailCountManager._instance.SetFail(Global.GameType, Global.eventIndex, Global.chapterIndex, Global.stageIndex);
        }
    }

    void recvGameFail(GameFailResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            ManagerUI._instance.OpenPopupFail(resp);
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

    void recvTurnRelayGameFail(TurnRelayResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            ManagerUI._instance.OpenPopupFail();
        }
    }

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

        GameUIManager.instance.RemoveSpecialEventCount();
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
            return; 

        }));
    }

    public void GetAdventureCircleBomb()
    {
        StartCoroutine(CoEvent(1f, () =>
        {
            BlockBase lineBlock = PosHelper.GetRandomBlock();
            if (lineBlock != null)
            {
                lineBlock.bombType = BlockBombType.BOMB;
                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                lineBlock.JumpBlock();
                InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                lineBlock.Destroylinker();                
            }

            StartCoroutine(CoEvent(0.6f, () => {                
                GameManager.instance.state = GameState.PLAY;
                BlockMatchManager.instance.SetBlockLink();
            }));

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

        foreach (var item in ServerContents.Chapters)
        {
            if( item.Value.stageIndex <= stageNum && item.Value.stageIndex + item.Value.stageCount > stageNum)
            {
                return item.Value.index - 1;
            }
        }


        // 예외상황 생각해서 옛날공식 내비두긴 함...
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

    static public bool NowFinalChapter(int stageNum = 0)
    {
        if (stageNum == 0) {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();
            stageNum = myProfile.stage; 
        }

        int chapterIdx = 1;
        foreach (var item in ServerContents.Chapters)
        {
            if (item.Value.stageIndex <= stageNum && item.Value.stageIndex + item.Value.stageCount > stageNum)
            {
                chapterIdx = item.Value.index;
                break;
            }
        }

        foreach (var item in ServerContents.Chapters)
        {
            if (chapterIdx < item.Key)
                return false;
        }
        return true;
    }

    public void SetClearBlind(bool show) 
    {
         if(show)
         {
             if(staticCamera != null)
             staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0.6f), new Color(0f, 0f, 0f, 0.0f), 0.35f, true);  //밝게
         }
         else
         {
             staticCamera = CameraEffect.MakeScreenEffect(1);
             staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.6f), 0.2f, false);  //어둡게


             if (spineboniBird != null && spineboniBird.AnimationName == "end")
                 Debug.Log("중복0");

             if (spineboniBird != null && spineboniBird.AnimationName != "end")
             {
                 spineboniBird.state.ClearTracks();
                 spineboniBird.state.SetAnimation(0, "end", false);
                 spineboniBird.state.Complete += delegate
                 {
                     Destroy(spineboniBird.gameObject);
                 };
             }
         }
    }

    //인게임 내에 떠있는 팝업들 제거(일시정지, 게임 아이템 사용 등..)
    public void CloseIngamePopup()
    {
        if (GameItemManager.instance != null)
            GameItemManager.instance.Close();

        if (UIPopupPause._instance != null)
            ManagerUI._instance.ClosePopUpUI(UIPopupPause._instance);

        if (GameUIManager.instance.gimmickTutorialPopup != null)
            ManagerUI._instance.ClosePopUpUI(GameUIManager.instance.gimmickTutorialPopup);
    }

    //인게임에서 랜덤 값 구할 때 사용.
    public int GetIngameRandom(int minValue, int maxValue)
    {
        if (ingameSeed <= 0)
            return Random.Range(minValue, maxValue);
        else
            return ingameRandom.Next(minValue, maxValue);
    }

    //블럭 이미지 출력
    public void MakeGimmickSpriteByBlockType(BlockType type, ref UIBlockSprite[] _outGimmickSprite, int subType = -1)
    {
        NGUIAtlas blockAtlas = ManagerUIAtlas._instance.GetAtlas(ManagerUIAtlas.AtlasType.BLOCK);
        NGUIAtlas decoTopAtlas = ManagerUIAtlas._instance.GetAtlas(ManagerUIAtlas.AtlasType.DECO_TOP);

        //블럭 이미지가 1겹이 아닐 수 있으므로 array로 받아서 출력 (ex.소다젤리, 흙기믹류)
        //subType은 컬러타입이나 이미지인덱스로 사용
        switch (type)
        {
            case BlockType.APPLE:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "Apple_Plus1");
                break;
            case BlockType.BLOCK_BLACK:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "blockLaboratoryAnimal1_0");
                break;
            case BlockType.BOX:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "blockBox1");
                break;
            case BlockType.FIRE_WORK:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "Fire0_Rabbit");
                break;
            case BlockType.PEA:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "pea_1");
                break;
            case BlockType.PEA_BOSS:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "peaBoss_1");
                break;
            case BlockType.PLANT:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "plant1");
                break;
            case BlockType.STONE:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "blockStone1");
                break;
            case BlockType.SPACESHIP:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "SpaceShip1");
                break;
            case BlockType.SODAJELLY:
                //이미지 인덱스
                if (subType <= 0 || subType >= 6)
                    subType = 1;
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "sodaJelly_Base" + subType.ToString(), "sodaJelly" + subType.ToString());
                break;
            case BlockType.KEY:
                GimmickSprite(ref _outGimmickSprite, blockAtlas, "blockKey");
                break;
            case BlockType.HEART:
                //컬러 타입
                if (subType <= 0 || subType >= 6)
                    subType = 1;
                
                GimmickSprite( ref _outGimmickSprite, blockAtlas, "blockHeartGauge", "blockHeart" +subType.ToString());
                if (_outGimmickSprite[0] != null)
                {
                    _outGimmickSprite[0].width = 20;
                    _outGimmickSprite[0].height = 40;
                    _outGimmickSprite[0].color = new Color32(0xAB, 0xAB, 0xAB, 0xFF);
                }

                break;
            case BlockType.ICE:
                GimmickSprite(ref _outGimmickSprite, decoTopAtlas, "DecoIce2");
                break;
        }
        return;
    }

    //데코 이미지 출력
    public void MakeGimmickSpriteByDecoType(BoardDecoType type, ref UIBlockSprite[] _outGimmickSprite, int subType = -1)
    {
        //아틀라스 변경
        NGUIAtlas decoAtlas = ManagerUIAtlas._instance.GetAtlas(ManagerUIAtlas.AtlasType.DECO);
        NGUIAtlas decoTopAtlas = ManagerUIAtlas._instance.GetAtlas(ManagerUIAtlas.AtlasType.DECO_TOP);

        //subType은 컬러타입이나 이미지인덱스로 사용
        switch (type)
        {
            case BoardDecoType.FLOWER_INK:
                //컬러 타입
                if (subType <= 0 || subType >= 6)
                    subType = 1;
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "FlowerInk_" + ManagerUI.GetColorTypeString((BlockColorType)subType));
                break;
            case BoardDecoType.CARCK1:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "crack01");
                break;
            case BoardDecoType.CARPET:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "carpet");
                break;
            case BoardDecoType.COUNT_CRACK:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "countCrack_1");
                break;
            case BoardDecoType.GRASS:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "Grass_1");
                break;
            case BoardDecoType.WATER:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "water_BG");
                break;
            case BoardDecoType.STATUE:
                GimmickSprite(ref _outGimmickSprite, decoAtlas, "StoneStatue2");
                break;
            case BoardDecoType.GRASSFENCEBLOCK:
                GimmickSprite(ref _outGimmickSprite, decoTopAtlas, "grass_fence_H_01");
                break;
            case BoardDecoType.ICE:
                GimmickSprite(ref _outGimmickSprite, decoTopAtlas, "DecoIce2");
                break;
            case BoardDecoType.NET:
                GimmickSprite(ref _outGimmickSprite, decoTopAtlas, "netPlants2");
                break;
            case BoardDecoType.RANDOM_BOX:
                GimmickSprite(ref _outGimmickSprite, decoTopAtlas, "RandomBox1_2");
                break;
        }
        return;
    }

    private void GimmickSprite(ref UIBlockSprite[] _out, NGUIAtlas _atlas, params string[] _spriteName)
    {
        for (int i = 0; i < _out.Length; i++)
        {
            if (_out[i] != null)
            {
                _out[i].atlas = _atlas;

                if (_spriteName.Length > i && _spriteName[i] != null)
                {
                    _out[i].gameObject.SetActive(true);
                    _out[i].spriteName = _spriteName[i];
                    _out[i].depth = _out[0].depth + i;
                }
                else
                    _out[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetActive_HelpButtonUI(bool show)
    {
        if(inGameHelpButton_UI != null)
            inGameHelpButton_UI.gameObject.SetActive(show);
    }

    #region 인게임 아틀라스 설정
    private IEnumerator CoMakeIngameAtlas()
    {
        //맵에서 사용하는 기믹 데이터 들고와서 설정.
        Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames = new Dictionary<ManagerUIAtlas.AtlasType, List<string>>();
        SetUseGimmickAtlasNames(dicAtlasNames);

        int overrideBlockIndex = 0;
        if (Global.GameType == GameType.EVENT)
        {
            CdnEventChapter eventChapterData = ServerContents.EventChapters;
            overrideBlockIndex = eventChapterData.blockIndex;
        }

        yield return ManagerUIAtlas._instance.BuildAtlas(ManagerUIAtlas.AtlasType.BLOCK, overrideBlockIndex, dicAtlasNames[ManagerUIAtlas.AtlasType.BLOCK]);
        if (dicAtlasNames.ContainsKey(ManagerUIAtlas.AtlasType.DECO) == true)
            yield return ManagerUIAtlas._instance.BuildAtlas(ManagerUIAtlas.AtlasType.DECO, overrideBlockIndex, dicAtlasNames[ManagerUIAtlas.AtlasType.DECO]);
        if (dicAtlasNames.ContainsKey(ManagerUIAtlas.AtlasType.DECO_TOP) == true)
            yield return ManagerUIAtlas._instance.BuildAtlas(ManagerUIAtlas.AtlasType.DECO_TOP, overrideBlockIndex, dicAtlasNames[ManagerUIAtlas.AtlasType.DECO_TOP]);
        if (dicAtlasNames.ContainsKey(ManagerUIAtlas.AtlasType.EFFECT) == true)
            yield return ManagerUIAtlas._instance.BuildAtlas(ManagerUIAtlas.AtlasType.EFFECT, overrideBlockIndex, dicAtlasNames[ManagerUIAtlas.AtlasType.EFFECT]);

    }

    /// <summary>
    /// 맵에서 사용하는 기믹들의 아틀라스 이름을 가져오는 함수.
    /// </summary>
    private void SetUseGimmickAtlasNames(Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames)
    {
        //현재 맵에서 사용되는 기믹 정보 가져오기
        UseGimmickData gimmickData = ManagerBlock.instance.GetUseGimmickData();

        //탐험모드 스킬에서 사용하는 기믹 정보 가져오기
        GetUseGimmickData_Adventure(gimmickData);

        //블럭 데이터 읽어서 가져오기
        SetAtlasName_UseBlockType(dicAtlasNames, gimmickData.listUseBlockType);

        //데코 데이터 읽어서 가져오기
        SetAtlasName_UseDecoType(dicAtlasNames, gimmickData.listUseDecoType);

        //데코_Top 데이터 읽어서 가져오기
        SetAtlasName_UseDecoTopType(dicAtlasNames, gimmickData.listUseTopDecoType);

        //게임 모드에 따라 사용하는 아틀라스 인덱스 설정
        SetAtlasName_UseGameMode(dicAtlasNames);
    }

    private void GetUseGimmickData_Adventure(UseGimmickData useGimmickData)
    {
        if (ManagerBlock.instance.stageInfo.gameMode != (int)GameMode.ADVENTURE)
            return;

        if (ManagerAdventure.instance == null)
            return;

        //해당 맵에서 사용하는 스킬 데이터 저장
        List<int> listSkillType = new List<int>();

        //적 스킬 데이터 저장
        for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList.Count; i++)
        {
            for (int j = 0; j < ManagerBlock.instance.stageInfo.battleWaveList[i].enemyIndexList.Count; j++)
            {
                EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[i].enemyIndexList[j];

                for (int k = 0; k < enemyInfo.skillList.Count; k++)
                {
                    listSkillType.Add(enemyInfo.skillList[k].skill);
                }
            }
        }

        //내 캐릭터 스킬 데이터 저장
        for (int i = 0; i < 3; i++)
        {
            var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            listSkillType.Add(animalData.skill);
        }

        //스킬 데이터에서 사용하는 기믹 데이터 분리
        for (int i = 0; i < listSkillType.Count; i++)
        {
            ENEMY_SKILL_TYPE skill = (ENEMY_SKILL_TYPE)listSkillType[i];
            InGameSkillUtility.SKILL_TYPE skillType = InGameSkillUtility.GetSkillType(skill);
            int skillCategory = InGameSkillUtility.GetSkillCategory(skill);
            int skillSubCategory = InGameSkillUtility.GetSkillSubCategory(skill);

            if (skillCategory == 0)
                return;

            switch(skillType)
            {
                case InGameSkillUtility.SKILL_TYPE.INSTALL_BLOCK:
                    ManagerBlock.instance.AddGimmickDataByBlockType(useGimmickData, skillCategory, skillSubCategory);
                    break;
                case InGameSkillUtility.SKILL_TYPE.INSTALL_DECO:
                    ManagerBlock.instance.AddGimmickDataByDecoType(useGimmickData, skillCategory);
                    break;
            }

        }
    }

    #region 블럭 타입 검사
    private void SetAtlasName_UseBlockType(Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames, List<int> listUseBlockType)
    {
        List<string> listBlockAtlasIndex = new List<string>();
        List<string> listEffectAtlasIndex = new List<string>();

        bool[] isExistMainType = new bool[3];
        isExistMainType[0] = (listUseBlockType.Contains((int)BlockType.PLANT));
        isExistMainType[1] = (listUseBlockType.Contains((int)BlockType.STONE));
        isExistMainType[2] = (listUseBlockType.Contains((int)BlockType.GROUND));

        for (int i = 0; i < listUseBlockType.Count; i++)
        {
            int blockType = listUseBlockType[i];
            listBlockAtlasIndex.Add(blockType.ToString());
            listEffectAtlasIndex.Add(blockType.ToString());

            //서브타입이 존재하는 블럭들 이미지 이름 저장
            if (isExistMainType[0] == true)
            {
                string subTypeName = GetSubTypeBlockName(blockType, BlockType.PLANT);
                if (subTypeName != null)
                    listBlockAtlasIndex.Add(subTypeName);
            }
            if (isExistMainType[1] == true)
            {
                string subTypeName = GetSubTypeBlockName(blockType, BlockType.STONE);
                if (subTypeName != null)
                    listBlockAtlasIndex.Add(subTypeName);
            }
            if (isExistMainType[2] == true)
            {
                string subTypeName = GetSubTypeBlockName(blockType, BlockType.GROUND);
                if (subTypeName != null)
                    listBlockAtlasIndex.Add(subTypeName);
            }
        }
        dicAtlasNames.Add(ManagerUIAtlas.AtlasType.BLOCK, listBlockAtlasIndex);
        dicAtlasNames.Add(ManagerUIAtlas.AtlasType.EFFECT, listEffectAtlasIndex);
    }
    
    private string GetSubTypeBlockName(int checkType, BlockType mainType)
    {
        string subTypeBlockName = null;
        int subTypeCount = 0;
        switch (mainType)
        {
            case BlockType.PLANT:
                subTypeCount = Enum.GetValues(typeof(PLANT_TYPE)).Length;
                break;
            case BlockType.STONE:
                subTypeCount = Enum.GetValues(typeof(STONE_TYPE)).Length;
                break;
            case BlockType.GROUND:
                subTypeCount = Enum.GetValues(typeof(GROUND_TYPE)).Length;
                break;
            default:
                return subTypeBlockName;
        }

        BlockType type = BlockType.NONE;
        for (int i = 1; i < subTypeCount; i++)
        {
            switch (mainType)
            {
                case BlockType.PLANT:
                    type = ManagerBlock.instance.ConvertSubtypeToBlockType_Plant((PLANT_TYPE)i);
                    break;
                case BlockType.STONE:
                    type = ManagerBlock.instance.ConvertSubtypeToBlockType_Stone((STONE_TYPE)i);
                    break;
                case BlockType.GROUND:
                    type = ManagerBlock.instance.ConvertSubtypeToBlockType_Ground((GROUND_TYPE)i);
                    break;
            }

            if (type != BlockType.NONE && type == (BlockType)checkType)
            {
                subTypeBlockName = string.Format("{0}_{1}", (int)mainType, i);
                break;
            }
        }
        return subTypeBlockName;
    }
    #endregion

    #region 데코 타입 검사
    private void SetAtlasName_UseDecoType(Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames, List<int> listUseDecoType)
    {
        List<string> listDecoAtlasIndex = new List<string>();
        for (int i = 0; i < listUseDecoType.Count; i++)
        {
            int decoType = listUseDecoType[i];
            listDecoAtlasIndex.Add(decoType.ToString());

            switch ((BoardDecoType)decoType)
            {
                case BoardDecoType.CARPET:  //카펫의 경우, 블럭 아틀라스/이펙트도 사용함.
                    dicAtlasNames[ManagerUIAtlas.AtlasType.BLOCK].Add(string.Format("0_0_{0}", decoType));
                    dicAtlasNames[ManagerUIAtlas.AtlasType.EFFECT].Add(string.Format("0_0_{0}", decoType));
                    break;
                case BoardDecoType.FLOWER_INK:
                    dicAtlasNames[ManagerUIAtlas.AtlasType.EFFECT].Add(string.Format("0_0_{0}", decoType));
                    break;
                case BoardDecoType.COUNT_CRACK:
                    string blockType = ((int)BlockType.APPLE).ToString();
                    if (dicAtlasNames[ManagerUIAtlas.AtlasType.BLOCK].FindIndex(x => x == blockType) == -1)
                        dicAtlasNames[ManagerUIAtlas.AtlasType.BLOCK].Add(blockType);
                    break;

                case BoardDecoType.STATUE:  //석상의 경우, 이펙트에 아틀라스 사용함.
                    dicAtlasNames[ManagerUIAtlas.AtlasType.EFFECT].Add(string.Format("0_0_{0}", decoType));
                    break;
                case BoardDecoType.BLOCK_GENERATOR:
                    //생성기 대표 이미지 추가
                    for (int j = 0; j < ManagerBlock.instance.stageInfo.listDecoInfo.Count; j++)
                    {
                        if (ManagerBlock.instance.stageInfo.listDecoInfo[j] is GimmickInfo_BlockGenerator == true)
                        {
                            GimmickInfo_BlockGenerator generatorInfo = ManagerBlock.instance.stageInfo.listDecoInfo[j] as GimmickInfo_BlockGenerator;
                            foreach (var tempImageInfo in generatorInfo.listImageInfo)
                            {
                                foreach (var blockAndColorData in tempImageInfo.listBlockAndColorData)
                                {
                                    if(blockAndColorData.subType != -1)
                                    {
                                        listDecoAtlasIndex.Add(string.Format("{0}_{1}_{2}", decoType, blockAndColorData.blockType, blockAndColorData.subType));
                                    }
                                    else
                                    {
                                        listDecoAtlasIndex.Add(string.Format("{0}_{1}", decoType, blockAndColorData.blockType));
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        if (listDecoAtlasIndex.Count > 0)
            dicAtlasNames.Add(ManagerUIAtlas.AtlasType.DECO, listDecoAtlasIndex);
    }
    #endregion

    #region 상단 데코 타입 검사
    private void SetAtlasName_UseDecoTopType(Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames, List<int> listUseDecoTopType)
    {
        List<string> listDecoAtlasIndex = new List<string>();
        for (int i = 0; i < listUseDecoTopType.Count; i++)
        {
            int decoType = listUseDecoTopType[i];
            listDecoAtlasIndex.Add(decoType.ToString());
        }

        if (listDecoAtlasIndex.Count > 0)
            dicAtlasNames.Add(ManagerUIAtlas.AtlasType.DECO_TOP, listDecoAtlasIndex);
    }
    #endregion

    #region 게임 타입에 따른 검사
    private void SetAtlasName_UseGameMode(Dictionary<ManagerUIAtlas.AtlasType, List<string>> dicAtlasNames)
    {
        //게임 모드에 따라 기본으로 사용하는 이펙트 추가
        dicAtlasNames[ManagerUIAtlas.AtlasType.EFFECT].Add(string.Format("0_0_0_{0}", ManagerBlock.instance.stageInfo.gameMode));
    }
    #endregion

    #endregion
}
