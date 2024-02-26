using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Pathfinding.Util;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UIPopupReady : UIPopupBase
{
    public static UIPopupReady _instance = null;
    
    //오픈 연출 나오는 도중 콜백.
    public Method.FunctionVoid _callbackOpen = null;

    public const int OPEN_ADD_TURN_STAGE = 7;
    public const int OPEN_SCORE_UP_STAGE = 21;
    public const int OPEN_ITEM2_STAGE = int.MaxValue;

    public const int OPEN_LINE_BOMB_STAGE = 7;
    public const int OPEN_CIRCLE_BOMB_STAGE = 7;
    public const int OPEN_RAINBOW_BOMB_STAGE = 7;

    public UIPanel clippingPanel;

    public UILabel[] stage;
    public Transform targetTr;
    public Transform startBtnTr;

    public UITexture flower;
    public UISprite flowerShadow;
    public UISprite startClover;
    
    //보니 대화창.
    public UITexture dialogBubble;
    public UILabel dialogText;

    public GameObject startCloverShadow;

    //챕터미션
    public UISprite CandySprite;

    //private List<UIItemTarget> listTargets = new List<UIItemTarget>();
    private int listCracks = 0;
    private List<int> listAnmimals = new List<int>();
    private int animalCrackCount = 0;
    private int keyCount = 0;

    public GameObject readyBox;
    //public static XMLStage tempData = null;
    public GameObject _objEFfectUseClover;
    public GameObject _objEffectStartButton;
    public GameObject _objEffectCloverSprite;
    public GameObject _objEffectButton;
    public GameObject _objRingGlow;

    //목표표시
    public GameObject targetRoot;
    List<StageTarget> listTargets = new List<StageTarget>();
    StageMapData tempData;

    //레디아이템
    public List<ReadyItem> listReadyItem = new List<ReadyItem>();
    public static EncValue[] readyItemUseCount = new EncValue[6];//{ 0, 0, 0, 0, 0, 0 };
    public static int[] readyItemSelectCount = new int[]{ 0, 0, 0, 0, 0, 0 };

    //보상 관련.
    public UISprite         rewardBox;
    public GameObject[]     reward;
    public UISprite[]       rewardSprite;
    public UIUrlTexture[]   rewardTexture;
    public UILabel[]        rewardText;
    public UILabel[]        rewardCount;
    public UILabel[]        rewardCountShadow;
    public Transform        rewardIconRoot;

    [HideInInspector]
    public LAppModelProxy boniLive2D;

    //튜토리얼에서 버튼 눌러졌는지 확인하는데 사용됨.
    [HideInInspector]
    public bool bStartGame = false;

    //이벤트에서 사용하는 오브젝트.
    public ReadyEvent readyEventObj;

    static public int stageIndex = 0;
    bool _recvGameStart_end = false;

    public static bool eventStageClear = false;
    public static bool eventGroupClear = false;
    public static bool eventStageFail = false;


    //이벤트 스테이지.
    //CdnEventChapter eventChapter = null;
    int eventGroup = 0;
    int eventStep = 0;
    bool bFreeStage = false;

    //코인이벤트
    public GameObject coinEventRoot;
    public UILabel coinEventLabel;

    //스페셜이벤트
    public UIUrlTexture specialEventUrlTexture;

    //보니 말풍선 DOTween 관련.
    private Sequence dialogSequence;

    void Awake()
    {
        _instance = this;
    }

    public override void OpenPopUp(int _depth)
    {
        rewardBox.gameObject.SetActive(false);
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);

        uiPanel.depth = _depth;
        mainSprite.transform.localScale = Vector3.one * 0.2f;
        mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
        mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.1f).SetEase(ManagerUI._instance.popupAlphaAnimation);

        clippingPanel.depth = uiPanel.depth + 1;
        if( Global.eventIndex <= 0)
            SetBoniModel();

        StartCoroutine(CoAction(openTime, () =>
        {
            if (Global.eventIndex<=0)
                if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0)
                {/*
                    if (Global.stageIndex == 7 && ManagerData._instance.userData.stage <= 7 && ServerRepos.UserItem.ReadyItem(0) > 0) //유저최종스테이지
                    {
                        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);
                        if (PlayerPrefs.HasKey("READYITEM_TUTORIAL") == false && (dataServer == null || dataServer.play == 0)) 
                        {
                            PlayerPrefs.SetInt("READYITEM_TUTORIAL", 0);
                            ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyItem);                            

                            int[] readyIndex = new int[4] { 0, 3, 4, 5 };

                            for(int i = 0; i < readyIndex.Length; i++)
                            {
                                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                    (
                                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                       "ReadyItem" + readyIndex[i].ToString(),
                                       "ReadyItem" + ((READY_ITEM_TYPE)readyIndex[i]).ToString(),
                                       1,
                                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                                       "TutorialReadyItem"
                                    );
                                var doc = JsonConvert.SerializeObject(useReadyItem);
                                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                            }
                        }
                    }
                    else if (Global.stageIndex == 21 && ManagerData._instance.userData.stage <= 21 && ServerRepos.UserItem.ReadyItem(0) > 0) //점수업
                    {
                        ServerUserStage dataServer = ServerRepos.FindUserStage(Global.stageIndex, 0);
                        if (PlayerPrefs.HasKey("READYITEM_SCORE_TUTORIAL") == false && (dataServer == null || dataServer.play == 0))
                        {
                            PlayerPrefs.SetInt("READYITEM_SCORE_TUTORIAL", 0);
                            ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyScoreUp);

                            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                                (
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                   "ReadyItem1",
                                   "ReadyItemSCORE_UP",
                                   1,
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                                   "TutorialReadyItem"
                                );
                            var doc = JsonConvert.SerializeObject(useReadyItem);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                        }
                    }*/

                }
            /*
            if(PlayerPrefs.HasKey("READYITEM_SCORE_TUTORIAL") == false && ManagerData._instance.userData.stage >= 21 && Global.stageIndex >= 21 && Global.stageIndex < 97)
            {
                PlayerPrefs.SetInt("READYITEM_SCORE_TUTORIAL", 0);
                ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyScoreUp);

                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                       "ReadyItem1",
                       "ReadyItemSCORE_UP",
                       1,
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                       "TutorialReadyItem"
                    );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            */

            if (_callbackOpen != null)
                _callbackOpen();
            bCanTouch = true;
        }));
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //레디창 전에 팝업들이 sortOrder을 사용하지 않는다면 live2d 쪽만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            boniLive2D._CubismRender.SortingOrder = layer + 1;
            clippingPanel.sortingOrder = layer + 2;

        }
        else
        {
            boniLive2D._CubismRender.SortingOrder = layer;
            clippingPanel.sortingOrder = layer + 1;
        }
        clippingPanel.useSortingOrder = true;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(StageMapData stageData)
    {
        if (Global._systemLanguage == CurrLang.eJap)
        {
            stage[0].text = string.Format("ステージ {0}", Global.stageIndex);
            stage[1].text = string.Format("ステージ {0}", Global.stageIndex);
        }
        else
        {
            stage[0].text = string.Format("STAGE {0}", Global.stageIndex);
            stage[1].text = string.Format("STAGE {0}", Global.stageIndex);
        }
        stage[0].MakePixelPerfect();
        stage[1].MakePixelPerfect();
        stageIndex = Global.stageIndex;

        //아이템선택상태 읽어오기
        if (PlayerPrefs.HasKey("readyItemSelect0"))
        {
            for(int i = 0; i < 6; i++)
            {
                string prefsName = "readyItemSelect" + i;
                readyItemSelectCount[i] = PlayerPrefs.GetInt(prefsName);
            }            
        }

        //클로버 버튼 이미지 설정.
       // if (GameData.RemainFreePlayTime() > 0)
        {
            SettingFreeCloverButton();
        }

        tempData = stageData;
        LoadTarget();
        SetStageFlower();
        SetSpecialEvent();
        SetCoinEvent();

        //보니 대사 창.
        SettingBoniDialog();

        if (Global.eventIndex <= 0)
            SetStageMission();
    }

    void SetCoinEvent()
    {
        if (Global.coinEvent > 0)
        {
            coinEventRoot.SetActive(true);
            coinEventLabel.text = "x" + Global.coinEvent;
            StartCoroutine(CoMoveCoinEventObject());
        }
        else
        {
            coinEventRoot.SetActive(false);
        }
    }

    IEnumerator CoMoveCoinEventObject()
    {
        float initPos = coinEventRoot.transform.localPosition.y;
        while (coinEventRoot.activeInHierarchy == true)
        {
            coinEventRoot.transform.localPosition
                = new Vector3(coinEventRoot.transform.localPosition.x, initPos + (Mathf.Cos(Time.time * 5f) * 3f), 0f);
            yield return null;
        }
        yield return null;
    }

    void SetSpecialEvent()
    {
        if (Global.eventIndex == 0 &&  Global.specialEventIndex > 0)
        {/*
            foreach (var item in ServerContents.SpecialEvent)
            {
                if (item.Value.index == Global.specialEventIndex)
                {
                    int getCount = 0;

                    foreach (var itemUser in ServerRepos.UserSpecilEvents)
                    {
                        if (itemUser.eventIndex == Global.specialEventIndex)
                        {
                            getCount = itemUser.progress;
                        }
                    }

                    int maxGetCount = item.Value.sections[item.Value.sections.Count - 1];

                    if (getCount < maxGetCount)
                    {
                        specialEventUrlTexture.gameObject.SetActive(true);
                        specialEventUrlTexture.Load(Global.gameImageDirectory, "IconEvent/", "sEventReady_" + Global.specialEventIndex);
                    }
                }
            }*/
        }
    }

    public void InitList()
    {
        if(listTargets.Count > 0)
        {
            foreach (StageTarget obj in listTargets) Destroy(obj.gameObject);
        }

        listTargets.Clear();
        listCracks = 0;
        listAnmimals.Clear();
        animalCrackCount = 0;
        keyCount = 0;
    }

    public GameObject GetButton()
    {
        return startBtnTr.gameObject;
    }

    public void MakeBoniDialog(int index)
    {
        dialogSequence.Kill();
        string key = string.Format("p_sr_{0}", (index + 5));

        dialogText.text = GetLive2DString(key);

        dialogBubble.transform.localScale = Vector3.one * 0.5f;
        dialogBubble.transform.DOScale(Vector3.one, 0.2f);
        SetBoxSize();
    }

    public AudioLobby GetReadyCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.readyVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    public AudioLobby GetMoveCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.moveVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    public AudioLobby GetFailCharacterVoice()
    {
        if (readyEventObj != null)
        {
            return readyEventObj.failVoice;
        }
        return AudioLobby.NO_SOUND;
    }

    void SettingBoniDialog()
    {   
        dialogSequence = DOTween.Sequence();
        dialogText.text = GetLive2DString("p_sr_4");
        SetBoxSize();
    }

    string GetLive2DString(string key)
    {
        // 이벤트 진행중일때, 만약 라이브2D 캐릭터가 보니가 아니라면 에리어이벤트에서 같은 스트링을 찾아보고 없으면 뭐 기본스트링 사용한다
        if (Global.eventIndex > 0)
        {
            if (readyEventObj != null && readyEventObj.live2dCharacter != TypeCharacterType.Boni)
            {
                string evKey = "event_" + Global.eventIndex;
                if (ManagerArea._instance._areaEventStep.ContainsKey(evKey))
                {
                    var areaEvent = ManagerArea._instance._areaEventStep[evKey];
                    if (areaEvent.StringExists(key))
                    {
                        return areaEvent.GetString(key);
                    }
                }
            }
        }

        return Global._instance.GetString(key);
    }

    //입력된 라인 수를 읽어 말풍선 크기 세팅(폭은 고정).
    private void SetBoxSize()
    {
        int _nLineCount = (int)(dialogText.printedSize.y / dialogText.fontSize);

        int boxHeight = 100;

        if (_nLineCount == 2)
        {
            boxHeight = 112;
        }
        else if(_nLineCount > 2)
        {
            boxHeight = 125;
        }
        dialogBubble.height = boxHeight;
    }

    void SetStageMission()
    {
        CandySprite.gameObject.SetActive(false);
        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    CandySprite.gameObject.SetActive(true);

                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear > 0)
                        CandySprite.spriteName = "Mission_DUCK_2";
                    else
                        CandySprite.spriteName = "Mission_DUCK_1";

                    CandySprite.MakePixelPerfect();
                    return;
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    CandySprite.gameObject.SetActive(true);

                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear > 0)
                        CandySprite.spriteName = "Mission_CANDY_2";
                    else
                        CandySprite.spriteName = "Mission_CANDY_1";

                    CandySprite.MakePixelPerfect();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 구버전 목표 타입의 데이터가 남아있는 경우 신 버전으로 옮김
    /// </summary>
    /// <param name="stageData"></param>
    private void CopyStageMapData_TargetCount_PrevVersionToNewVersion()
    {
        //일반 타입 목표의 데이터 옮기기
        for (int i = 0; i < tempData.collectCount.Length; i++)
        {
            if (tempData.collectCount[i] == 0)
                continue;

            //신버전 목표 데이터에 구버전 데이터 옮기기.
            SetStageMapData_TargetCount(i, tempData.collectCount[i]);
        }

        //컬러 타입 목표의 데이터 옮기기.(구버전에서는 컬러블럭만 사용했기 때문에 해당 데이터만 옮겨줌)
        int targetType = (int)TARGET_TYPE.COLORBLOCK;
        for (int i = 0; i < tempData.collectColorCount.Length; i++)
        {
            if (tempData.collectColorCount[i] == 0)
                continue;

            //신버전 목표 데이터에 구버전 데이터 옮기기.
            SetStageMapData_TargetCount(targetType, tempData.collectColorCount[i], i);
        }

        //구버전 데이터는 지워줌.
        tempData.collectCount = new int[1] { 0 };
        tempData.collectColorCount = new int[1] { 0 };
    }

    public void SetStageMapData_TargetCount(int targetType, int targetCount, int colorType = 0)
    {
        int findIndex = tempData.listTargetInfo.FindIndex(x => x.targetType == targetType);
        if (findIndex > -1)
        {   //기존에 데이터가 있으면, 해당 데이터에 값을 덮어씌워줌.
            CollectTargetInfo info = tempData.listTargetInfo[findIndex];
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
                tempData.listTargetInfo.Remove(info);
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

            tempData.listTargetInfo.Add(targetInfo);
        }
    }

    int currentChapter()
    {
        if (Global.stageIndex < 10)
        {
            return 0;
        }
        else if (Global.stageIndex < 21)
        {
            return 1;
        }
        else
        {
            return (Global.stageIndex - 21) / 15 + 2;
        }
    }
    new void OnDestroy()
    {
        _instance = null;

        if (eventGroupClear)
        {
          //  if(ServerContents.EventChapters[Global.eventIndex].active > 0)
            {//
               // string assetName = ServerContents.EventChapters[Global.eventIndex].assetName;
              //  ManagerLobby._instance.PlaySceneWakeUp(ManagerArea._instance._areaEventStep[assetName], ServerRepos.EventChapters[Global.eventIndex].groupState);
            }
        }

        if (eventStageClear || eventGroupClear || eventStageFail)
            EventChapterData.SetUserData(Global.eventIndex);
        eventStageClear = false;
        eventGroupClear = false;
        eventStageFail = false; 

        base.OnDestroy();
    }

    public void EventStageSetting(GameObject obj)
    {
        //창 다 열린 후 콜백.
        _callbackOpen += EventActionStart;

        //레디창 사이즈 조절.
        mainSprite.height = 1015;
        mainSprite.transform.localPosition = new Vector3(0f, -100f, 0f);
        readyBox.transform.localPosition = new Vector3(0f, 65.3f, 0f);

        //이벤트 오브젝트 설정.

        readyEventObj = NGUITools.AddChild(mainSprite.gameObject, obj).GetComponent<ReadyEvent>();
        readyEventObj.transform.localPosition = new Vector3(1f, 105f, 0f);
        SetBoniModel();
        boniLive2D.SetPosition(new Vector3(221f, -255f, 0f) + readyEventObj.live2dOffset);


        SettingEventReward();
        SettingStep();
        SettingEventFreeStage();
        SettingCollectEvent();

        //파랑새 디폴트 움직임.
        StartCoroutine(CoEvnet());
    }

    void EventActionStart()
    {
        bool bChange = false;
        //이벤트 성공 실패에 따라 움직이는 연출.
        if (eventStageClear == true)
        {
            Invoke("BlueBirdMoveAction", 0.3f);
        }
        else if (eventStageFail == true)
        {
          //  if (ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
           //     Invoke("BlueBirdFailAction", 0.3f);
        }
    }

    void SetBoniModel()
    {
        int modelNo = (int)TypeCharacterType.Boni;
        if( Global.eventIndex > 0)
        {
            modelNo = ManagerCharacter._instance._live2dObjects.ContainsKey( (int)(readyEventObj.live2dCharacter) ) ? (int)readyEventObj.live2dCharacter : modelNo;

            boniLive2D = NGUITools.AddChild(mainSprite.gameObject, ManagerCharacter._instance._live2dObjects[modelNo].obj).GetComponent<LAppModelProxy>();
            boniLive2D.SetVectorScale(readyEventObj.live2dSize);
            boniLive2D.SetPosition((new Vector3(221f, -320f, 0f) + readyEventObj.live2dOffset));
        }
        else
        {
            boniLive2D = NGUITools.AddChild(mainSprite.gameObject, ManagerCharacter._instance._live2dObjects[modelNo].obj).GetComponent<LAppModelProxy>();

            bool flip = ManagerCharacter._instance._live2dObjects[modelNo].defaultScale < 0.0f;
            // 보니를 300사이즈로 만드는 게 기준이므로, 다른캐릭터로 할 때는 보니 크기에 비례해서 스케일 조정해줘야
            float scaleRatio = Mathf.Abs(ManagerCharacter._instance._live2dObjects[modelNo].defaultScale / ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni].defaultScale);
            boniLive2D.SetVectorScale(new Vector3(flip ? -300f : 300f, 300f, 300f) * scaleRatio);
            boniLive2D.SetPosition(new Vector3(221f, -320f, 0f));
        }
        
        boniLive2D._CubismRender.SortingOrder = uiPanel.sortingOrder + 1;
        boniLive2D.setAnimation(false, "Ready");
        boniLive2D.setAnimation(false, "Ready_in");
    }

    #region 타겟 데이터 읽어오는 함수.

    void LoadTarget()
    {
        //스테이지 목표 데이터 초기화(구버전에서 사용하던 데이터가 남아있으면 신버전 데이터로 옮겨줌)
        CopyStageMapData_TargetCount_PrevVersionToNewVersion();
        SortListTargetInfo();

        InitList();
        SetReadyItem(tempData.gameMode == (int)GameMode.LAVA);

        for (int i = 0; i < tempData.listTargetInfo.Count; i++)
        {
            CollectTargetInfo targetInfo = tempData.listTargetInfo[i];
            TARGET_TYPE targetType = (TARGET_TYPE)targetInfo.targetType;

            string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                string.Format("StageTarget_{0}", targetType) : "StageTarget";

            for (int j = 0; j < targetInfo.listTargetColorInfo.Count; j++)
            {
                BlockColorType colorType = (BlockColorType)targetInfo.listTargetColorInfo[j].colorType;

                StageTarget target = NGUITools.AddChild(targetRoot, ManagerUI._instance._objInGameTarget).GetComponent<StageTarget>();
                target.targetType = targetType;
                target.targetColor = colorType;

                //목표 수 표시
                string collectCount = targetInfo.listTargetColorInfo[j].collectCount.ToString();
                target.targetCount.text = collectCount;
                target.targetCountShadow.text = collectCount;

                //목표 이미지 설정
                string targetColorName = (colorType != BlockColorType.NONE) ?
                      string.Format("{0}_{1}", targetName, colorType) : targetName;
                target.targetSprite.spriteName = targetColorName;
                target.targetSprite.MakePixelPerfect();

                listTargets.Add(target);
            }
        }

        float startPos = (1 - listTargets.Count) * 48;
        for (int i = 0; i < listTargets.Count; i++)
        {
            listTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }
    }

    public void SortListTargetInfo()
    {
        tempData.listTargetInfo.Sort(delegate (CollectTargetInfo a, CollectTargetInfo b)
        {
            if (a.targetType < b.targetType)
                return -1;
            else if (a.targetType > b.targetType)
                return 1;
            else
                return 0;
        });
    }
    #endregion


    void SetReadyItem(bool isLavaMode = false)
    {
        //세일 여부 받아옴.
       /* bool bSale = ServerRepos.LoginCdn.ReadyItemSale == 1;
        //오픈여부 블러오기, 임의데이타
        //다이아, 코인 적용
        if (ServerRepos.UserItem == null) {
            Debug.LogWarning("User Item Class NULL.............");
        }
        if(isLavaMode) listReadyItem[0].initItem(READY_ITEM_TYPE.ADD_TURN, 0, ServerRepos.UserItem.ReadyItem(0), bSale); // ManagerData._instance._cdnData.readyItem1
        else listReadyItem[0].initItem(READY_ITEM_TYPE.ADD_TURN, ServerRepos.LoginCdn.ReadyItems[0], ServerRepos.UserItem.ReadyItem(0), bSale);

        listReadyItem[1].initItem(READY_ITEM_TYPE.SCORE_UP, ServerRepos.LoginCdn.ReadyItems[1], ServerRepos.UserItem.ReadyItem(1), bSale);
        listReadyItem[2].initItem(READY_ITEM_TYPE.ITEM2, ServerRepos.LoginCdn.ReadyItems[2], ServerRepos.UserItem.ReadyItem(2), bSale);
        listReadyItem[3].initItem(READY_ITEM_TYPE.LINE_BOMB, ServerRepos.LoginCdn.ReadyItems[3], ServerRepos.UserItem.ReadyItem(3), bSale);
        listReadyItem[4].initItem(READY_ITEM_TYPE.CIRCLE_BOMB, ServerRepos.LoginCdn.ReadyItems[4], ServerRepos.UserItem.ReadyItem(4), bSale);
        listReadyItem[5].initItem(READY_ITEM_TYPE.RAINBOW_BOMB, ServerRepos.LoginCdn.ReadyItems[5], ServerRepos.UserItem.ReadyItem(5), bSale);*/
    }

    private void SetStageFlower()
    {
        // 이벤트 스테이지 경우
        if (Global.eventIndex > 0)
        {
            return;
        }
        //꽃 이미지 설정.
        int star = 0;
        if(Global.stageIndex > 0 && ManagerData._instance._stageData[Global.stageIndex - 1] != null)
            star = ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;
        
        { 
            string flowerPath = string.Format("UI/ready_icon_flower_0{0}", star);
            flower.mainTexture = Resources.Load(flowerPath) as Texture2D;
            if (star == 1)
            {
                flower.width = 97;
                flower.height = 130;
                flower.transform.localPosition = new Vector3(1f, -15f, 0f);
                flowerShadow.width = 90;
            }
            else if (star == 2)
            {
                flower.width = 177;
                flower.height = 124;
                flower.transform.localPosition = new Vector3(2f, -12f, 0f);
            }
            else if (star == 3)
            {
                flower.width = 160;
                flower.height = 157;
                flower.transform.localPosition = new Vector3(5f, -12f, 0f);
            }
            else if (star == 4)
            {
                flower.width = 162;
                flower.height = 158;
                flower.transform.localPosition = new Vector3(2f, -17f, 0f);
            }
            else
            {
                flower.width = 100;
                flower.height = 141;
                flower.transform.localPosition = new Vector3(3f, -10f, 0f);
                flowerShadow.enabled = false;
            }
        }
    }

    /*
    private void SetPosition()
    {
        int nCount = listTargets.Count;
        if (nCount % 2 == 0)
            targetTr.localPosition = new Vector3(55, targetTr.localPosition.y, targetTr.localPosition.z);
        else
            targetTr.localPosition = new Vector3(0, targetTr.localPosition.y, targetTr.localPosition.z);

        for (int i = 0; i < nCount; i++)
        {
            float xPos = (-118 * (nCount / 2)) - (-118 * i);
            listTargets[i].SetPos(xPos);
            listTargets[i].ReadyPopUpTarget();
        }
    }*/



    public override void ClosePopUp(float _startTime = 0.3f, Method.FunctionVoid callback = null)
    {
        PlayerPrefs.SetString("LastStageName", "");


        StartCoroutine(CoAction(_startTime, () =>
        {
            int nCount = listTargets.Count;
            for (int i = 0; i < nCount; i++)
            {
                listTargets[i].Recycle();
            }
        }));
        
        boniLive2D.setAnimation(false, "Ready_out");

        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
        mainSprite.transform.DOScale(Vector3.zero, openTime).SetEase(Ease.InBack);
        StartCoroutine(CoAction(0.1f, () =>
        {
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, openTime - 0.1f);
        }));

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(openTime, () =>
            PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(openTime + 0.15f, () =>
        {
            Destroy(gameObject);
        }));
    }

    public void ShowUseClover()
    {
        StartCoroutine(ShowCloverEffect());
    }

    IEnumerator ShowCloverEffect()
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;
        float defaultScaleValue = startClover.cachedTransform.localScale.x;
        Vector3 startButtonPos = startBtnTr.localPosition;

        showTimer = 0;

        RingGlowEffect ringGlow = NGUITools.AddChild(startBtnTr.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.9f;

        NGUITools.AddChild(startBtnTr.gameObject, _objEffectStartButton);
        _objEffectButton.SetActive(true);

        //이펙트 터질 때 버튼 움직임.
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        while (showTimer < 0.8f)
        {
            showTimer += Global.deltaTimeLobby * 4f;

            if (showTimer < 0.5f)
            {
                scaleRatio = 0.7f + showTimer;
            }
            else
            {
                scaleRatio = 1.8f - showTimer;
            }

            startBtnTr.localPosition = startButtonPos*(1 + (1 - showTimer)*0.04f);
            startClover.cachedTransform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            _objEffectButton.transform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            yield return null;
        }

        _objEffectButton.SetActive(false);
        showTimer = 0;
             
        while (showTimer < 0.5f)
        {
            showTimer += Global.deltaTimeLobby;
            yield return null;
        }

        //인게임 씬로드.
        //GlobalGameManager.instance.LoadScene(eSceneNameType.InGame);
        startClover.cachedTransform.localScale = Vector3.one * defaultScaleValue;

        while (true)
        {
            if (_recvGameStart_end)
                break;
            yield return null;
        }



        touchButton = false;
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
        //ManagerNetwork._instance.SendStagePlay(Global.stageIndex, readyItemUseCount);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("InGame");
        ManagerUI._instance.bTouchTopUI = true;

        yield return null;
    }

    bool touchButton = false;

    private void OnClickGoInGame()
    {
        /*
        if (GlobalGameManager.instance.GetGameMode() == eGameMode.None)
        {
            if (GlobalGameManager.instance._nCurrentClover <= 0)
            {
                return;
            }
            GlobalGameManager.instance.SetGameMode(eGameMode.InGame);
            GlobalGameManager.instance._bOnPopUpUI = false;
            GlobalGameManager.instance._nCurrentClover--;
            LobbyUI.instance.ShowGlowthyUseEffect(gameObject, GrowthyType.CLOVER, startClover.transform.position, 1.5f, false);
            SoundManager.instance.PlayAudio(LobbyUI.instance.audioInGameStartClip);
            GlobalGameManager.instance._nCurChapterIndex = ChapterDataStaticManager.instance._nCurChapterIndex;
            ChapterDataStaticManager.instance.SaveListChapterDataGroups();
            GlobalGameManager.instance._housingDataInfos = ChapterDataStaticManager.instance._housingDataInfos;
        }*/

        if (bCanTouch == false)
            return;
        bCanTouch = false;
        /*
        if ((Global.clover > 0 || GameData.RemainFreePlayTime() > 0 || bFreeStage == true ) && touchButton == false)
        {
            bStartGame = true;

            ManagerSound.AudioPlay(AudioLobby.UseClover);
            touchButton = true;
            // UI 정지 시키고 .. 클로버 날리기
            //클로버날리기
            RingGlowEffect ringGlow = NGUITools.AddChild(ManagerUI._instance._CloverSprite.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
            ringGlow._effectScale = 0.45f;

            UIUseCloverEffect cloverEffect = NGUITools.AddChild(clippingPanel.gameObject, _objEFfectUseClover).GetComponent<UIUseCloverEffect>();
            //클로버 사용(이벤트 무료 스테이지 거나, 무료 타임이 남았을 경우 투명 클로버).
            if (GameData.RemainFreePlayTime() > 0 || bFreeStage == true)
            {
                cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position, true);
            }
            else
            {
                cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position);
            }

            //클로버 사용하는 경우 클로버 감소.
            if (GameData.RemainFreePlayTime() <= 0 && bFreeStage == false)
            {
                Global.clover--;
                ManagerUI._instance.UpdateUI();
            }

            ManagerLobby._stageTryCount++;


            getReadyItemCount = new int[]
            {
                ServerRepos.UserItem.ReadyItem(0),
                ServerRepos.UserItem.ReadyItem(1),
                ServerRepos.UserItem.ReadyItem(2),
                ServerRepos.UserItem.ReadyItem(3),
                ServerRepos.UserItem.ReadyItem(4),
                ServerRepos.UserItem.ReadyItem(5)
            };

            for (int i = 0; i < 6; i++)
            {
                string prefsName = "readyItemSelect" + i;
                PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
            }

            for (int i = 0; i < 6; i++)
            {
                readyItemUseCount[i] = new EncValue();
                readyItemUseCount[i].Value = readyItemSelectCount[i];
            }


            if(readyItemSelectCount[0]> 0 && Global.stageIndex < OPEN_ADD_TURN_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[0].Value = 0;
            }

            if(tempData.gameMode == (int)GameMode.LAVA)
            {
                readyItemUseCount[0].Value = 0;
            }



            if (readyItemSelectCount[1] > 0 && Global.stageIndex < OPEN_SCORE_UP_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[1].Value = 0;
            }
            
            if (readyItemSelectCount[1] > 0 && Global.eventIndex > 0)
            {
                readyItemUseCount[1].Value = 0;
            }
            

            if (readyItemSelectCount[2] > 0 && Global.stageIndex < OPEN_ITEM2_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[2].Value = 0;
            }
            if (readyItemSelectCount[3] > 0 && Global.stageIndex < OPEN_LINE_BOMB_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[3].Value = 0;
            }
            if (readyItemSelectCount[4] > 0 && Global.stageIndex < OPEN_CIRCLE_BOMB_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[4].Value = 0;
            }
            if (readyItemSelectCount[5] > 0 && Global.stageIndex < OPEN_RAINBOW_BOMB_STAGE && Global.eventIndex == 0)
            {
                readyItemUseCount[5].Value = 0;
            }


            for (int i = 0; i < 6; i++)
            {
                if (readyItemUseCount[i].Value > 0)
                {
                    if (i < 3)
                    {
                        if (getReadyItemCount[i] < readyItemUseCount[i].Value)
                        {
                            if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.coin))
                            {
                                payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                                freeCoin[i] = 0;
                            }
                            else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                            {
                                payCoin[i] = (int)(GameData.User.coin);
                                freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.coin);
                            }
                            else
                            {
                                payCoin[i] = 0;
                                freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            }
                        }
                    }
                    else
                    {
                        if (getReadyItemCount[i] < readyItemUseCount[i].Value)
                        {
                            if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.jewel))
                            {
                                payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                                freeCoin[i] = 0;
                            }
                            else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.jewel) && (int)(GameData.User.jewel) > 0)
                            {
                                payCoin[i] = (int)(GameData.User.jewel);
                                freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.jewel);
                            }
                            else
                            {
                                payCoin[i] = 0;
                                freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            }
                        }
                    }
                }
            }

            beforeFCoin = (int)(GameData.User.fcoin);
            beforeCoin = (int)(GameData.User.coin);
            beforeFDia = (int)(GameData.User.fjewel);
            beforeDia = (int)(GameData.User.jewel);

            var useItems = new int[] {
                    readyItemUseCount[0].Value,
                    readyItemUseCount[1].Value,
                    readyItemUseCount[2].Value,
                    readyItemUseCount[3].Value,
                    readyItemUseCount[4].Value,
                    readyItemUseCount[5].Value };

            
            if ((int)ServerRepos.User.clover > 1) usePCoin = 1;
            else useFCoin = 1;
            

            var req = new GameStartReq()
            {
                stage = (int)Global.stageIndex,
                eventIdx = Global.eventIndex,
                isEvent = Global.eventIndex > 0 ? 1 : 0,
                items = useItems,

            };
            QuestGameData.SetUserData();
            ServerAPI.GameStart(req, recvGameStart);
        }
        else if(Global.clover <= 0 && touchButton == false)
        {
            // 클로버 부족 메세지 노출
            ManagerUI._instance.OpenPopupCloverShop();
            bCanTouch = true;
        }*/
    }

    //int[] useItems;
    int[] getReadyItemCount;

    int[] payCoin = new int[6] { 0,0,0,0,0,0};
    int[] freeCoin = new int[6] { 0, 0, 0, 0, 0, 0 };

    /*
    int usePCoin = 0;
    int useFCoin = 0;
    */

    int beforeFCoin = 0;
    int beforeCoin = 0;
    int beforeFDia = 0;
    int beforeDia = 0;


    /*
    void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();


            QuestGameData.SetUserData();

            _recvGameStart_end = true;

            //그로씨
            //클로버사용
            if (GameData.RemainFreePlayTime() <= 0 && bFreeStage == false)
            {
                var useClover = new ServiceSDK.GrowthyCustomLog_Money
                    (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_GAME_PLAY,
                        0, //-usePCoin,
                        -1, //-useFCoin,
                        0,//(int)(ServerRepos.User.clover),
                        (int)(ServerRepos.User.AllClover)//(int)(ServerRepos.User.fclover)
                    );
                var cloverDoc = JsonConvert.SerializeObject(useClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
            }
         
            for (int i = 0; i < 6; i++)
            {
                if (readyItemUseCount[i].Value > 0)
                {
                    if (listReadyItem[i].type == READY_ITEM_TYPE.LOCK) continue;

                    if (i < 3)
                    {
                       // if (getReadyItemCount[i] < readyItemUseCount[i])
                       if(payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeCoin -= payCoin[i];
                            beforeFCoin -= freeCoin[i];

                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                                (
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                                -payCoin[i],
                                -freeCoin[i],                                    
                             beforeCoin ,//(int)(GameData.User.coin),
                               beforeFCoin,                 // (int)(GameData.User.fcoin),
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                                );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                                (
                                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                    "ReadyItem" + i.ToString(),
                                    "ReadyItem" + listReadyItem[i].type.ToString(),
                                    readyItemUseCount[i].Value,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
                                );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }
                       
                    }
                    else
                    {
                        if (payCoin[i] > 0 || freeCoin[i] > 0)
                        {
                            beforeDia -= payCoin[i];
                            beforeFDia -= freeCoin[i];


                            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                            (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -payCoin[i],
                            -freeCoin[i],
                           beforeDia,
                            beforeFDia,
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                            );
                            var docItem = JsonConvert.SerializeObject(playEnd);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                            (
                               ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                                "ReadyItem" + i.ToString(),
                                "ReadyItem" + listReadyItem[i].type.ToString(),
                                readyItemUseCount[i].Value,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
                            );
                            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                        }
                    }

                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            "ReadyItem" + i.ToString(),
                            "ReadyItem" + listReadyItem[i].type.ToString(),
                            -readyItemUseCount[i].Value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
                        );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
            }

        }
    }*/

    private void SettingFreeCloverButton(bool bFree = true, bool bAction = false)
    {
        if (bFree == true)
        {
            startClover.spriteName = "icon_clover_infinity";
            startClover.width = 100;
            startClover.height = 100;
            startCloverShadow.SetActive(false);
        }
        else
        {
            startClover.spriteName = "icon_clover";
            startClover.width = 72;
            startClover.height = 74;
            startCloverShadow.SetActive(true);
        }

        if (bAction == true)
        {
            startClover.transform.localScale = Vector3.one * 0.5f;
            startClover.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    void OnClickBtnReadyClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;        

        //if (SceneManager.GetActiveScene().name == "InGame")
        //{
        //    ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
        //    ManagerSound._instance.StopBGM();
        //    SceneLoading.MakeSceneLoading("Lobby");
        //}
        //else
        {
            ManagerUI._instance.ClosePopUpUI();
        }

        //Global.clover = (int)GameData.Asset.AllClover;
      //  Global.coin = (int)GameData.Asset.AllCoin;
       // Global.jewel = (int)GameData.Asset.AllJewel;
        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();
    }

    private IEnumerator CoEvnet()
    {
        float initPos = readyEventObj._texturePoint.transform.localPosition.y;
        while (UIPopupReady._instance != null)
        {
            readyEventObj._texturePoint.transform.localPosition 
                = new Vector3(readyEventObj._texturePoint.transform.localPosition.x, initPos + Mathf.Abs(Mathf.Cos(Time.time * 8f) * 6f), 0f);
            yield return null;
        }
        yield return null;
    }

    private void SettingEventReward()
    {
        rewardBox.gameObject.SetActive(true);
        /*
        //유저 데이터에서 현재 몇 번째 그룹에 있는지 받아와야함.
        eventGroup = ManagerData._instance._eventChapterData[Global.eventIndex]._groupState;
        if (ServerContents.EventChapters[Global.eventIndex].type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET) eventGroup = 1;

        eventChapter = ServerContents.EventChapters[Global.eventIndex];
        int rCnt = eventChapter.rewards[eventGroup - 1].Count;
        rewardText[0].transform.localPosition = new Vector3(-5f - (rCnt * 30f), 33f, 0f);
        rewardIconRoot.transform.localPosition = new Vector3(180f - (rCnt * 60f), 0f, 0f);
        rewardBox.width = 10 + (rCnt * 60);

        for (int j = 0; j < rCnt; j++)
        {
            reward[j].SetActive(true);
            GetRewardIcon(eventChapter.rewards[eventGroup - 1][j].type, eventChapter.rewards[eventGroup - 1][j].value, j);
        }*/
    }

    private void SettingStep()
    {
        eventStep = ManagerData._instance._eventChapterData[Global.eventIndex]._state;
        /*
        if(ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            //1번째 그룹 이상일 경우, 현재 스텝은 이전 그룹의 수만큼 빼 줘야함.
            if (eventGroup > 1)
            {
                eventStep -= eventChapter.counts[eventGroup - 2];
                if (eventStep < 0)
                {
                    // Debug.Log("EventStep Error  -  EventStep" + eventStep);
                }
            }

            Vector3 pos = readyEventObj._step[(eventStep - 1)].transform.localPosition;
            pos += readyEventObj._offsetPoint;
            readyEventObj._texturePointShadow.transform.localPosition = pos;
        }
        else //if (ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.COLLECT)
        {
            for (int i = 0; i < (eventStep); i++)
            {
                 if (i < readyEventObj._step.Count)
                {
                    ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                    eventStage.stageCount = i;
                    readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
                }
            }

            Vector3 pos;
            if (readyEventObj._step.Count < eventStep)
            {
                pos = readyEventObj.giftRoot.transform.localPosition;
            }
            else
            {
                pos = readyEventObj._step[(eventStep - 1)].transform.localPosition;
            }            
            pos += readyEventObj._offsetPoint;
            readyEventObj._texturePointShadow.transform.localPosition = pos;
        }
        
        for (int i = 0; i < (eventStep - 1); i++)
        {
            readyEventObj._star[i].gameObject.SetActive(true);
            readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
            readyEventObj._step[i].MakePixelPerfect();
        }*/
    }

    void SettingCollectEvent()
    {/*
        if (ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.COLLECT && ServerContents.GetEventChapter(Global.eventIndex).active == 1)
        {
            int maxCount = 0;

            for (int i = 1; i < eventChapter.collectMaterials.Count; i+=2)
            {
                maxCount += eventChapter.collectMaterials[i];
            }

            int getCount = 0;

            foreach (var item in ServerRepos.EventStages)
            {
                if(item.eventIdx == Global.eventIndex)
                {
                    for(int i = 1; i < eventChapter.collectMaterials[(item.stage -1)*2 +1]+1; i++)
                    {
                        if ((item.materialCnt & (1 << i)) != 0)
                        {
                            getCount++;
                        }
                    }
                }
            }

            List<int> materialTypList = new List<int>();

            for (int i = 0; i < readyEventObj._step.Count; i++)
            {
                int tempGetCount = 0;

                foreach (var item in ServerRepos.EventStages)
                {
                    if (item.eventIdx == Global.eventIndex && item.stage == (i+1))
                    {
                        for (int j = 1; j < eventChapter.collectMaterials[(item.stage - 1) * 2 + 1] + 1; j++)
                        {
                            if ((item.materialCnt & (1 << j)) != 0)
                            {
                                tempGetCount++;
                            }
                        }
                    }
                }

                if (materialTypList.Contains(eventChapter.collectMaterials[i * 2]) == false && eventChapter.collectMaterials[i * 2] > 0) materialTypList.Add(eventChapter.collectMaterials[i * 2]);
                {
                    if(eventChapter.collectMaterials[i * 2] > 0)
                    {
                        ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                        eventStage.materialRoot.SetActive(true);
                        eventStage.SetGetMaterial(eventChapter.collectMaterials[i * 2 + 1], tempGetCount, eventChapter.collectMaterials[i * 2]);
                    }
                }
             
            }

            if (materialTypList.Count ==1 && maxCount > 0)// && ServerContents.GetEventChapter(Global.eventIndex).active == 1)
            {
                readyEventObj.collectRoot.SetActive(true);
                readyEventObj.maxcountLabel.text = maxCount.ToString();
                readyEventObj.maxcountLabelShadow.text = maxCount.ToString();

                readyEventObj.getCountLabel.text = getCount + "/";
                readyEventObj.getCountLabelShadow.text = getCount + "/";

                string fileName = "mt_" + materialTypList[0];
                readyEventObj.collectObj.SettingTextureScale(80, 80);
                readyEventObj.collectObj.Load(Global.gameImageDirectory, "IconMaterial/", fileName);
            }
        }
        */
    }

    private void SettingEventFreeStage()
    {/*
        int freeStage = eventChapter.freePlays[eventGroup - 1] - 1;

        if (freeStage >= 0 && freeStage <= readyEventObj._free.Count)
        {
            readyEventObj._free[freeStage].SetActive(true);
            //무료스테이지가 현재 진행중인 스테이지라면 무료 클로버 표시.
            if ((freeStage + 1) == eventStep)
            {
                SettingFreeCloverButton();
                bFreeStage = true;
            }
        }*/
    }

    private void ChangeEventButtonClover()
    {/*
        if (GameData.RemainFreePlayTime() > 0)
            return;
        int freeStage = eventChapter.freePlays[eventGroup - 1] - 1;
        if (freeStage >= 0 && freeStage <= readyEventObj._free.Count)
        {
            //무료스테이지가 현재 진행중인 스테이지라면 무료 클로버 표시.
            if (ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
            {
                if ((freeStage + 1) == eventStep)
                {
                    SettingFreeCloverButton(true, true);
                    bFreeStage = true;
                }
                else
                {
                    SettingFreeCloverButton(false, true);
                    bFreeStage = false;
                }
            }
            else
            {
                if ((freeStage + 1) == Global.stageIndex)
                {
                    SettingFreeCloverButton(true, true);
                    bFreeStage = true;
                }
                else
                {
                    SettingFreeCloverButton(false, true);
                    bFreeStage = false;
                }
            }
        }*/
    }
    
    private void GetRewardIcon(int type, int value, int index)
    {
        bool bSprite = true;
        bool bCount = true;
        switch (type)
        {
            //일반 재화.
            case (int)RewardType.clover:
                rewardSprite[index].spriteName = "icon_clover_stroke_green";
                break;
            case (int)RewardType.coin:
                rewardSprite[index].spriteName = "icon_coin_stroke_yellow";
                break;
            case (int)RewardType.jewel:
                rewardSprite[index].spriteName = "icon_diamond_stroke_blue";
                break;
            case (int)RewardType.star:
                rewardSprite[index].spriteName = "icon_star_stroke_pink";
                break;
            case (int)RewardType.flower:
                rewardSprite[index].spriteName = "stage_icon_level_03";
                break;

            //레디아이템.
            case (int)RewardType.readyItem1:
                rewardSprite[index].spriteName = "icon_apple_stroke";
                break;
            case (int)RewardType.readyItem2:
                rewardSprite[index].spriteName = "icon_scoreUp_stroke";
                break;
            //case (int)RewardType.readyItem3:
            //    return "icon_flower_stroke_blue";
            case (int)RewardType.readyItem4:
                rewardSprite[index].spriteName = "icon_line_bomb_stroke";
                break;
            case (int)RewardType.readyItem5:
                rewardSprite[index].spriteName = "icon_bomb_stroke";
                break;
            case (int)RewardType.readyItem6:
                rewardSprite[index].spriteName = "icon_rainbow_stroke";
                break;

            //인게임 아이템.
            case (int)RewardType.ingameItem1:
                rewardSprite[index].spriteName = "icon_flower_stroke_blue";
                break;
            case (int)RewardType.ingameItem2:
                rewardSprite[index].spriteName = "icon_flower_stroke_blue";
                break;

            //선물상자.
            case (int)RewardType.boxSmall:
                rewardSprite[index].spriteName = "icon_giftbox_blueStroke_01";
                break;
            case (int)RewardType.boxMiddle:
                rewardSprite[index].spriteName = "icon_giftbox_blueStroke_02";
                break;
            case (int)RewardType.boxBig:
                rewardSprite[index].spriteName = "icon_giftbox_blueStroke_03";
                break;

            //포코유라.
            case (int)RewardType.toy:
                string yuraName = string.Format("y_i_{0}", value);
                rewardTexture[index].SettingTextureScale(65, 65);
                rewardTexture[index].Load(Global.gameImageDirectory, "Pokoyura/", yuraName);
                bSprite = false;
                bCount = false;
                break;

            //스티커.
            case (int)RewardType.stamp:
                rewardTexture[index].mainTexture = Resources.Load("Message/stamps") as Texture2D;
                rewardTexture[index].width = 65;
                rewardTexture[index].height = 65;
                bSprite = false;
                bCount = false;
                break;

            //스티커.
            case (int)RewardType.costume:
                string costumeName = string.Format("{0}", value);
                rewardTexture[index].SettingTextureScale(65, 65);
                rewardTexture[index].Load(Global.gameImageDirectory, "Costume/", costumeName);
                bSprite = false;
                bCount = false;
                break;

            //그 외(재료). 
            default:
                int matNum = (type % 1000);
                string matName = string.Format("mt_{0}", matNum);
                rewardTexture[index].SettingTextureScale(60, 60);
                rewardTexture[index].Load(Global.gameImageDirectory, "IconMaterial/", matName);
                bSprite = false;
                break;
        }

        //스프라이트/텍스쳐 따라 설정.
        if (bSprite == true)
        {
            rewardSprite[index].enabled = true;
            rewardSprite[index].MakePixelPerfect();
            rewardSprite[index].width = (int)(rewardSprite[index].width / 1.5f);
            rewardSprite[index].height = (int)(rewardSprite[index].height / 1.5f);
            rewardTexture[index].enabled = false;
        }
        else
        {
            rewardSprite[index].enabled = false;
            rewardTexture[index].enabled = true;
        }

        //숫자 설정.
        if (bCount == true)
        {
            rewardCount[index].gameObject.SetActive(true);
            rewardCount[index].text = value.ToString();
            rewardCountShadow[index].text = value.ToString();
        }
        else
        {
            rewardTexture[index].transform.localPosition = new Vector3(3.5f, 2f, 0f);
            rewardCount[index].gameObject.SetActive(false);
        }
    }

    private void BlueBirdMoveAction()
    {
        Vector3 pos = Vector3.zero;

        /*
        if (ServerContents.EventChapters[Global.eventIndex].type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            int preStageCount = 0;
            int stageCount = 0;
            //현재 챕터의 스테이지 카운트를 구함.
            for (int i = 0; i < eventChapter.counts.Count; i++)
            {
                if (i == (eventGroup - 1))
                {
                    stageCount = eventChapter.counts[(eventGroup - 1)] - preStageCount;
                    break;
                }
                else
                {
                    preStageCount += eventChapter.counts[i];
                }
            }

            //현재 플레이어 스테이지 번호가 현재 이벤트 그룹의 수보다 적을때에만 다음으로 넘어가는 연출.
            if (eventStep > stageCount)
            {
                return;
            }
            else if (eventStep == stageCount)
            {
                pos = readyEventObj.giftRoot.localPosition;
                StartCoroutine(GetEventReward());
            }
            else
            {
                pos = readyEventObj._step[eventStep].transform.localPosition;
                readyEventObj._step[eventStep].mainTexture = readyEventObj._textureStepOff;
            }

        }
        else
        {
            //현재 플레이어 스테이지 번호가 현재 이벤트 그룹의 수보다 적을때에만 다음으로 넘어가는 연출.
            if (eventStep > eventChapter.counts[0])
            {
                return;
            }
            else if (eventStep == eventChapter.counts[0])
            {
                pos = readyEventObj.giftRoot.localPosition;
                StartCoroutine(GetEventReward());
            }
            else
            {
                pos = readyEventObj._step[eventStep].transform.localPosition;
                readyEventObj._step[eventStep].mainTexture = readyEventObj._textureStepOff;
            }
        }



        //연출 시 터치불가.
        this.bCanTouch = false;

        StartCoroutine(StarAction((eventStep - 1)));
        pos += readyEventObj._offsetPoint;
        readyEventObj._texturePointShadow.transform.DOLocalMove(pos, 0.5f).SetEase(Ease.OutQuint);
        PlayMoveSound();
        eventStep++;

            //연출 후 데이터 변경.
        StartCoroutine(ChangeEventAction(0.5f));

        //어느정도 지난 후 사운드.
        StartCoroutine(CoAction(0.5f, () => { ManagerSound.AudioPlay(AudioLobby.BoniStep); }));

        //연출 후 터치가능.
        StartCoroutine(OnTouch(0.7f));*/
    }

    IEnumerator GetEventReward()
    {
        yield return new WaitForSeconds(0.5f);

        UpdateReward();

        //Global.clover = (int)GameData.Asset.AllClover;
        //Global.coin = (int)GameData.Asset.AllCoin;
       // Global.jewel = (int)GameData.Asset.AllJewel;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        //"이벤트 보상을 획득했어!" 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/happy2") as Texture2D;
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_1"), false, texture, OnClickBtnClose);
        popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
        ManagerSound.AudioPlay(AudioLobby.m_boni_wow);
    }

    IEnumerator ChangeEventAction(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        EventChapterData.SetUserData(Global.eventIndex);
        ChangeEventButtonClover();

        /*
        if (ServerContents.EventChapters[Global.eventIndex].type != (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            //이동한 스테이지 되도록수정
            for (int i = 0; i < (eventStep); i++)
            {
                if (i < readyEventObj._step.Count)
                {
                    ReadyEventStage eventStage = readyEventObj._step[i].GetComponent<ReadyEventStage>();
                    eventStage.stageCount = i;
                    readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
                }
            }

            for (int i = 0; i < (eventStep - 1); i++)
            {
                readyEventObj._star[i].gameObject.SetActive(true);
                readyEventObj._step[i].mainTexture = readyEventObj._textureStepOn;
                readyEventObj._step[i].MakePixelPerfect();
            }
        }*/
    }

    private void UpdateReward()
    {/*
        //받는 보상 중 선물상자가 있는 경우, 로비에 생성.
        for (int i = 0; i < eventChapter.rewards[eventGroup - 1].Count; i++)
        {
            int type = eventChapter.rewards[eventGroup - 1][i].type;
            if (type >= 100)
            {
                ManagerLobby._instance.ReMakeGiftbox();
                break;
            }
        }

        //그로씨
        for (int i = 0; i < eventChapter.rewards[eventGroup - 1].Count; i++)
        {
            TypeValue rewards = eventChapter.rewards[eventGroup - 1][i];

            switch (rewards.type)
            {
                //일반 재화.
                case 1:
                    var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                    0,
                    rewards.value,
                    0,//(int)(ServerRepos.User.clover),
                    (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                    "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );
                    var docMoney = JsonConvert.SerializeObject(growthyMoney);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                    break;
                case 2:
                    var growthyMoney2 = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                    0,
                     rewards.value,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );
                    var docMoney2 = JsonConvert.SerializeObject(growthyMoney2);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney2);
                    break;
                case 3:
                    var growthyMoney3 = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                    0,
                   rewards.value,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel),
                    "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );
                    var docMoney3 = JsonConvert.SerializeObject(growthyMoney3);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney3);
                    break;
                case 4:
                    var growthyMoney4 = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                    0,
                    rewards.value,
                    0,
                    (int)(ServerRepos.User.fcoin),
                    "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );
                    var docMoney4 = JsonConvert.SerializeObject(growthyMoney4);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney4);
                    break;
                case 5:
                    break;

                 //   LINE_BOMB,
//    CIRCLE_BOMB,
//    ,
                //레디아이템.
                case 6:
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                             "ReadyItem0",
                            "ReadyItemADD_TURN",
                            rewards.value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                        );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

                    break;
                case 9:
                    var useReadyItem9 = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                             "ReadyItem3",
                            "ReadyItemLINE_BOMB",
                            rewards.value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                        );
                    var doc9 = JsonConvert.SerializeObject(useReadyItem9);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc9);
                    break;
                case 10:
                    var useReadyItem10 = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                             "ReadyItem4",
                            "ReadyItemCIRCLE_BOMB",
                            rewards.value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                        );
                    var doc10 = JsonConvert.SerializeObject(useReadyItem10);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc10);
                    break;
                case 11:
                    var useReadyItem11 = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                             "ReadyItem5",
                            "ReadyItemRAINBOW_BOMB",
                            rewards.value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                        );
                    var doc11 = JsonConvert.SerializeObject(useReadyItem11);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc11);
                    break;

                //인게임 아이템.
                case 12:
                    var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                       "InGameItem1",
                        "InGameItemHAMMER",
                        rewards.value,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );

                    var doc12 = JsonConvert.SerializeObject(useReadyItem1);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc12);
                    break;
                case 13:
                    var useReadyItem13 = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                       "InGameItem2",
                        "InGameItemCROSS_LINE",
                        rewards.value,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                        "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                    );

                    var doc13 = JsonConvert.SerializeObject(useReadyItem13);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc13);
                    break;

                //선물상자.
                case 100:
                case 101:
                case 102:

                    RewardType boxType = (RewardType)rewards.type;
                    
                    var useReadyItem102 = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.GIFTBOX,
                          "GIFTBOX_"+ boxType.ToString(),
                          "GIFTBOX_" + boxType.ToString(),
                          1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                          "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                      );
                    var doc102 = JsonConvert.SerializeObject(useReadyItem102);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc102);
                    break;

                //포코유라.
                case 18:
                    var useReadyItem18 = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                          "POKOYURA_" + rewards.value,
                          "POKOYURA_" + rewards.value,
                          1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                          "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                      );
                    var doc18 = JsonConvert.SerializeObject(useReadyItem18);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc18);
                    break;

                //스티커.
                case 19:
                    var useReadyItem19 = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.STAMP,
                          "Stamp" + rewards.value.ToString(),
                          "Stamp",
                         1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                          "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                      );
                    var docStamp = JsonConvert.SerializeObject(useReadyItem19);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
                    break;

                //그 외(재료). 
                default:

                    var useReadyItem20 = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                          "MATERIAL_" + (rewards.type % 1000).ToString(),
                          "material",
                          rewards.value,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                          "EVENT_CHAPTER_" + Global.eventIndex + "_CLEAR"
                      );
                    var docStamp20 = JsonConvert.SerializeObject(useReadyItem20);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp20);

                    break;
            }
        }
        */
    }

    private IEnumerator StarAction(int stepNum)
    {
        yield return new WaitForSeconds(0.1f);

        readyEventObj._star[stepNum].gameObject.SetActive(true);
        readyEventObj._star[stepNum].color = new Color(1f, 1f, 1f, 0f);
        DOTween.ToAlpha(() => readyEventObj._star[stepNum].color, x => readyEventObj._star[stepNum].color = x, 1f, 0.2f).SetEase(Ease.InQuint);
        readyEventObj._star[stepNum].transform.localScale = Vector3.one * 1.3f;
        readyEventObj._star[stepNum].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuint);
        yield return new WaitForSeconds(0.3f);
        
        readyEventObj._step[stepNum].mainTexture = readyEventObj._textureStepOn;
        readyEventObj._step[stepNum].MakePixelPerfect();
        readyEventObj._step[stepNum].transform.DOShakePosition(0.5f, 5f, 20, 90f, false, false);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
    }

    private void BlueBirdFailAction()
    {
        //현재 플레이어가 첫번째 칸이라면 연출 안 함.
        if (eventStep == 1)
        {
            return;
        }

        //연출 시 터치불가.
        this.bCanTouch = false;

        //별 세팅.
        for (int i = 0; i < (eventStep -1); i++)
        {
            StarFailAction(i);
        }

        PlayTrainingFailSound();
        //파랑새 위치이동 & 회전.
        StartCoroutine(CoBluebirdFailAction());
        eventStep = 1;

        //연출 후 데이터 변경
        StartCoroutine(ChangeEventAction(1.5f));
        //연출 후 터치가능.
        StartCoroutine(OnTouch(1.7f));
    }

    IEnumerator CoBluebirdFailAction()
    {
        //첫번째 스텝 위치.
        Vector3 pos = pos = readyEventObj._step[0].transform.localPosition;
        pos += readyEventObj._offsetPoint;

        yield return new WaitForSeconds(0.5f);
        readyEventObj._texturePointShadow.transform.DOLocalMove(pos, 1.5f).SetEase(Ease.OutQuint);
        readyEventObj._texturePoint.flip = UITexture.Flip.Horizontally;
        yield return new WaitForSeconds(0.7f);
        ManagerSound.AudioPlay(AudioLobby.BoniStep);
        yield return new WaitForSeconds(0.1f);
        readyEventObj._texturePoint.flip = UITexture.Flip.Nothing;
    }

    private void StarFailAction(int stepNum)
    {
        readyEventObj._step[stepNum].mainTexture = readyEventObj._textureStepOff;
        readyEventObj._step[stepNum].MakePixelPerfect();
        DOTween.ToAlpha(() => readyEventObj._star[stepNum].color, x => readyEventObj._star[stepNum].color = x, 0f, 0.4f).SetEase(Ease.InQuint);
        readyEventObj._star[stepNum].transform.DOLocalMoveY(50f, 0.5f);
        ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
    }

    IEnumerator OnTouch(float time)
    {
        yield return new WaitForSeconds(time);
        this.bCanTouch = true;
    }

    public void ChangeEventStage(int tempStageNumber)
    {
        if(tempStageNumber >= 0 && tempStageNumber < readyEventObj._step.Count)
        {
            if (this.bCanTouch == false)
                return;

//            Global.stageIndex = tempStageNumber+1;
            StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
        }
    }

    //스테이지 정보 다시부르기
    IEnumerator CoCheckStageDataBeforeOpenPopUpReady()
    {
        //Cdn에서 스테이지 버전데이타 받아오기
        //임시로 StageInfo 에 버전추가 더 상위 파일 만들기 -> StageMapData
        string stageKey = "pp" + Global.stageIndex + ".xml";
        string stageName = Global.GetHashfromText(stageKey) + ".xml";
        bool loadStage = false;
        int stageCurrentVer = 0;

        // 이벤트 스테이지 일 경우
        if (Global.eventIndex > 0)
        {
            stageName = "E" + Global.eventIndex + "_" + Global.stageIndex + ".xml";
          //  stageCurrentVer = ServerContents.GetEventChapter(Global.eventIndex).stageVersions[Global.stageIndex - 1];
        }
        else
            stageCurrentVer = ManagerData._instance.StageVersionList[Global.stageIndex - 1];

        WWW www;
        WWWForm form = new WWWForm();
        form.AddField("Cache-Control", "no-cache");

        {
            string lastname = PlayerPrefs.GetString("LastStageName", "");
            if (lastname.Length > 0)
            {
                if (File.Exists(lastname))
                    File.Delete(lastname);
            }
            string stagePath = Global.StageDirectory + stageName;
            PlayerPrefs.SetString("LastStageName", stagePath);
            PlayerPrefs.Save();
        }


        www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
        yield return www;

        if (www.error == null)
        {
            try
            {
                int mapSize = www.bytes.Length;
                if (mapSize > 1024)
                {
                    StringReader reader = new StringReader(www.text);
                    var serializer = new XmlSerializer(typeof(StageMapData));
                    StageMapData stageData = serializer.Deserialize(reader) as StageMapData;

                    if (stageCurrentVer > stageData.version) loadStage = true;
                }
                else
                {
                    throw new System.Exception();
                }
            }
            catch
            {
                string stagePath = Global.StageDirectory + stageName;
                File.Delete(stagePath);
                loadStage = true;
            }
        }
        else
        {
            loadStage = true;
        }

        if (loadStage)
        {
            www = new WWW(Global._cdnAddress + "stage/" + stageName);//, form);
            yield return www;

            if (www.error != null)
            {
                www.Dispose();
                yield return null;
            }
            else
            {
                try
                {
                    int mapSize = www.bytes.Length;
                    if (mapSize > 256)
                    {
                        MemoryStream memoryStream = new MemoryStream(www.bytes);
                        memoryStream.Position = 0;
                        byte[] bytes = memoryStream.ToArray();
                        File.WriteAllBytes(Global.StageDirectory + stageName, bytes);
                        www.Dispose();
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
                catch
                {
                    string stagePath = Global.StageDirectory + stageName;
                    File.Delete(stagePath);
                }
            }
        }

        www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
        yield return www;

        if (www.error == null)
        {
            StringReader reader = new StringReader(www.text);
            var serializer = new XmlSerializer(typeof(StageMapData));
            tempData = serializer.Deserialize(reader) as StageMapData;
        }
        else
        {

            NetworkLoading.EndNetworkLoading();
            //ErrorController.ShowNetworkErrorDialogAndRetry("");
        }
        yield return null;

        LoadTarget();

        //목표바꾸기, 파랑새이동
        Vector3 pos = readyEventObj._step[Global.stageIndex-1].transform.localPosition;
        pos += readyEventObj._offsetPoint;

        this.bCanTouch = false;

        readyEventObj._texturePointShadow.transform.DOLocalMove(pos, 0.5f).SetEase(Ease.OutQuint);
        PlayMoveSound();
        SettingEventFreeStage();
        ChangeEventButtonClover();
        //어느정도 지난 후 사운드.
        StartCoroutine(CoAction(0.5f, () => { ManagerSound.AudioPlay(AudioLobby.BoniStep); }));

        //연출 후 터치가능.
        StartCoroutine(OnTouch(0.5f));

        yield return null;
    }

    private void PlayMoveSound()
    {
        if (Global.eventIndex > 0)
        {
            AudioLobby voice = UIPopupReady._instance.GetMoveCharacterVoice();
            if (voice != AudioLobby.NO_SOUND)
            {
                ManagerSound.AudioPlay(voice);
            }
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
        }
    }

    private void PlayTrainingFailSound()
    {
        if (Global.eventIndex > 0)
        {
            AudioLobby voice = UIPopupReady._instance.GetFailCharacterVoice();
            if (voice != AudioLobby.NO_SOUND)
            {
                ManagerSound.AudioPlay(voice);
            }
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.m_bird_hansum);
        }
    }
}
