using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using PokoAddressable;

public class UIPopupStageAdventure : UIPopupBase
{
    public static UIPopupStageAdventure _instance = null;

    static public int startChapter = 0;

    [Header("Linked Object")]
    [SerializeField] private UILabel whiteFlowerLabel;
    [SerializeField] private UILabel blueFlowerLabel;
    [SerializeField] private GameObject blueFlowerBox;
    [SerializeField] private UIItemAdventureMissionRewardGage rewardBox;

    public UIReuseGrid_StageAdventure_C scroll_ChapterList;
    public UIReuseGrid_StageAdventure_S scroll_StageList;

    [HideInInspector] public List<StageAdventure_C_ItemData> chapterDataList = new List<StageAdventure_C_ItemData>();
    [HideInInspector] public List<StageAdventure_S_ItemData> stageDataList = new List<StageAdventure_S_ItemData>();

    public static int selectedChapter = 1;
    public static int stageCursor = 1;
    public bool firstLoad = false;
    
    public static int prevChapterCursor = 0;
    public static int prevStageCursor = 0;

    public static bool nowStageClear = false;
    public static bool allClearedAndNothingToExpress = false;

    public static void ResetStaticVariables()
    {
        selectedChapter = 1;
        prevChapterCursor = 0;
        prevStageCursor = 0;
        nowStageClear = false;
        allClearedAndNothingToExpress = false;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        this.bShowTopUI = true;
    }

    public override void OpenPopUp(int depth)
    {
        ManagerSound._instance.StopBGM();
        base.OpenPopUp(depth);

        if( firstLoad )
        {
            ResetStaticVariables();
        }

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        //초기 위치 설정
        ManagerAdventure.User.SyncFromServer_Stage();
        bool challengeFirstCleared = ManagerAdventure.User.IsChallengeFirstCleared();
        selectedChapter = ManagerAdventure.User.GetChapterCursor() < 1 ? 1 : ManagerAdventure.User.GetChapterCursor();
        stageCursor = ManagerAdventure.User.GetStageCursor() < 1 ? 1 : ManagerAdventure.User.GetStageCursor();

        int lastUnfinishedChap = ManagerAdventure.User.GetLastUnfinishedChapter();
        int lastUnfinishedChallenge = ManagerAdventure.User.GetLastChallengeClearedChapter();
        if (lastUnfinishedChap != 0)
        {
            selectedChapter = lastUnfinishedChap;
            var chapData = ManagerAdventure.Stage.GetChapter(selectedChapter);
            stageCursor = chapData.GetLastStageIdx();

            prevChapterCursor = selectedChapter;
            prevStageCursor = stageCursor;
        }
        else if(lastUnfinishedChallenge != 0)
        {
            selectedChapter = lastUnfinishedChallenge;
            var chapData = ManagerAdventure.Stage.GetChapter(selectedChapter);
            stageCursor = chapData.GetLastStageIdx(true);

            prevChapterCursor = selectedChapter;
            prevStageCursor = stageCursor;
        }
        else
        {
            if (challengeFirstCleared)
            {
                prevChapterCursor = ManagerAdventure.User.GetLastPlayedChapter();
                prevStageCursor = ManagerAdventure.User.GetLastPlayedStage();
            }
            else if (prevStageCursor != 0)
            {
                stageCursor = prevStageCursor;
            }
            else
            {
                prevChapterCursor = selectedChapter;
                prevStageCursor = stageCursor;
            }
        }

        nowStageClear = (
            prevChapterCursor != ManagerAdventure.User.GetChapterCursor() || 
            prevStageCursor != ManagerAdventure.User.GetStageCursor() || 
            lastUnfinishedChap != 0 || 
            lastUnfinishedChallenge != 0 ||
            challengeFirstCleared
        );

        allClearedAndNothingToExpress = lastUnfinishedChallenge == 0 && lastUnfinishedChap == 0 && !challengeFirstCleared && ManagerAdventure.User.IsAllCleared(false);

        if(lastUnfinishedChallenge == 0 && lastUnfinishedChap == 0 )
        {
            ManagerAdventure.User.RecommendStage(out selectedChapter, out stageCursor);
        }            

        InitData();

        scroll_ChapterList.InitReuseGrid();
        scroll_StageList.InitReuseGrid();

        scroll_ChapterList.ScrollItemMove(selectedChapter - 1);
        scroll_StageList.ScrollItemMove(stageCursor - 1, 1);

        Global.SetGameType_Adventure(selectedChapter, stageCursor);
        ManagerUI._instance.IsTopUIAdventureMode = true;
        ManagerSound._instance.PlayBGM();
    }

    public IEnumerator Start()
    {
        bool nowChapterClear = ManagerAdventure.User.GetLastUnfinishedChapter() != 0;

        ClickBlocker.Make(Mathf.Infinity);

        Debug.Log("UIPopupStageAdventure.Start()");
        yield return new WaitForSeconds(0.5f);

        if( allClearedAndNothingToExpress )
        {
            ClickBlocker.Make(0.1f);

            nowStageClear = false;
            yield break;
        }

        int prevChap = prevChapterCursor;
        int prevStage = prevStageCursor;

        prevChapterCursor = ManagerAdventure.User.GetChapterCursor();
        prevStageCursor = ManagerAdventure.User.GetStageCursor();

        StartCoroutine(BonusStageOpenProcess(prevChap, prevStage));
        yield return StageClearProcess(prevChap, prevStage);
        

        base.bCanTouch = false;

        yield return ChapterClearRewardProcess(prevChap, prevStage);
        yield return ChallengeClearRewardProcess(prevChap, prevStage);

        yield return StageOpenProcess(prevChap, prevStage);

        nowStageClear = false;

        yield return ChapterMissionRewardProcess();

        base.bCanTouch = true;


        int recommendedChapter = prevChap;
        int recommendedStage = prevStage;

        if (ManagerAdventure.User.RecommendStage(out recommendedChapter, out recommendedStage))
        {
            if (selectedChapter != recommendedChapter)
            {
                scroll_ChapterList.SelectChapter(recommendedChapter);
                scroll_StageList.ScrollItemMove(recommendedStage - 1);
            }
        }

        ClickBlocker.Make(0.1f);

        if (!PlayerPrefs.HasKey("TutorialChallenge_Adventure") && selectedChapter > 1 && !ChallengeClearCheck())
        {
            PlayerPrefs.SetInt("TutorialChallenge_Adventure", 0);
            ManagerTutorial.PlayTutorial(TutorialType.TutorialChallenge_Adventure);
        }
    }

    public IEnumerator StageClearProcess(int prevChap, int prevStage)
    {
        if( nowStageClear == false)
        {
            yield break;
        }

        yield return new WaitWhile(() => scroll_ChapterList.isScrolling);

        var chapItem = scroll_ChapterList.GetChapterItem(prevChap);

        if (chapItem == null)
            yield break;

        StartCoroutine(chapItem.CoBossHit_StageClear());

        if (UIPopupStageAdventure.selectedChapter != prevChap)
            yield break;

        var stageItem = scroll_StageList.GetStageItem(prevStage);
        yield return stageItem.PlayClear();

        ManagerAdventure.User.SaveLastSelectedStage(prevChap, prevStage);

        yield break;
    }

    public IEnumerator BonusStageOpenProcess(int prevChap, int prevStage)
    {
        int lastUnfinishedChap = ManagerAdventure.User.GetLastUnfinishedChapter();
        if (lastUnfinishedChap == 0)
        {
            yield break;
        }
        yield return new WaitForSeconds(0.1f);

        var chapData = ManagerAdventure.Stage.GetChapter(lastUnfinishedChap);
        var stageList = chapData.GetStageList();
        foreach( var stage in stageList )
        {
            if( stage.stageType == 1 )
            {
                var stageItem = scroll_StageList.GetStageItem(stage.idx);
                StartCoroutine(stageItem.PlayBonusStageOpen());
            }
            yield return new WaitForSeconds(0.2f);
        }

        yield break;
    }

    public IEnumerator StageOpenProcess(int prevChap, int prevStage)
    {
        if (prevChap == selectedChapter && prevStage == stageCursor)
        {
            yield break;
        }

        if (nowStageClear == false)
        {
            yield break;
        }
        var stageItem = scroll_StageList.GetStageItem(stageCursor);
        yield return stageItem.PlayOpen();

        ManagerAdventure.User.SaveLastSelectedStage(selectedChapter, stageCursor);

        yield break;
    }

    public IEnumerator ChapterClearRewardProcess(int prevChap, int prevStage)
    {
        int lastUnfinishedChap = ManagerAdventure.User.GetLastUnfinishedChapter();
        if (lastUnfinishedChap == 0 )
        {
            yield break;
        }
        var chapItem = this.scroll_ChapterList.GetChapterItem(lastUnfinishedChap);
        if(chapItem != null)
        {
            yield return chapItem.CoBossDown();
        }

        //var chapData = ManagerAdventure.Stage.GetChapter(lastUnfinishedChap);
        //var clearRewards = chapData.chapterClearReward;
        //ManagerUI._instance.OpenPopupClearAdventureChapter(lastUnfinishedChap);
        
        var arg = new AdventureGetChapterClearRewardReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            chapter = lastUnfinishedChap,
        };

        bool retReceived = false;
        ServerAPI.AdventureGetChapterClearReward(arg, (Protocol.AdventureGetChapterClearRewardResp resp) 
            => {
                retReceived = true;
                if ( resp.IsSuccess )
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                   (
                       ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                       ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE,
                       lastUnfinishedChap.ToString(),
                       ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                   );
                    var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);

                    //그로씨추가 
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)RewardType.jewel,
                        2,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PLAY,
                        null
                        );
                }
            });

        yield return new WaitForSeconds(1f);

        while (retReceived == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Global.jewel = GameData.Asset.AllJewel;
        ManagerUI._instance.UpdateUI();

        if( lastUnfinishedChap != ManagerAdventure.User.GetChapterCursor() )
        {
            var nextChapItem = this.scroll_ChapterList.GetChapterItem(ManagerAdventure.User.GetChapterCursor());
            yield return nextChapItem.CoBossAppear();
        }

        if( ManagerAdventure.User.AllCleared() )
        {
            var comingSoonItem = this.scroll_ChapterList.GetChapterItem(ManagerAdventure.User.GetChapterCursor() + 1);
            if(comingSoonItem != null)
            {
                yield return comingSoonItem.OpenComingSoon();
            }
        }

        ManagerAdventure.User.SyncFromServer_Stage();
    }

    public IEnumerator ChallengeClearRewardProcess(int prevChap, int prevStage)
    {
        int lastUnfinishedChap = ManagerAdventure.User.GetLastChallengeClearedChapter();
        if (lastUnfinishedChap == 0)
        {
            yield break;
        }
        var chapItem = this.scroll_ChapterList.GetChapterItem(lastUnfinishedChap);
        if (chapItem != null)
        {
            yield return chapItem.CoReplaceFlower();
            
        }

        bool retReceived = false;
        var arg = new AdventureGetChapterClearRewardReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            chapter = lastUnfinishedChap,
        };
        ServerAPI.AdventureGetChapterClearReward(arg, (Protocol.AdventureGetChapterClearRewardResp resp)
            =>
        {
            retReceived = true;
            if (resp.IsSuccess)
            {
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                    (
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE_CHALLENGE,
                       lastUnfinishedChap.ToString(),
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
            }
        });

        yield return new WaitForSeconds(1f);

        while (retReceived == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Global.jewel = GameData.Asset.AllJewel;
        ManagerUI._instance.UpdateUI();

        ManagerAdventure.User.SyncFromServer_Stage();
    }

    public IEnumerator ChapterMissionRewardProcess()
    {
        yield break;
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        ManagerSound._instance.StopBGM();

        Global.SetGameType_NormalGame();

        ManagerUI._instance.IsTopUIAdventureMode = false;
        ManagerSound._instance.PlayBGM();
        base.ClosePopUp(_mainTime, callback);

        if (_instance == this)
            _instance = null;
    }

    public void InitData()
    {
        whiteFlowerLabel.text = GetFlowerCount(1) + "/" + GetFlowerCountMax();
        blueFlowerLabel.text = GetFlowerCount(2) + "/" + GetFlowerCountMax();

        if (GetFlowerCount(2) > 0)
            blueFlowerBox.SetActive(true);

        {
            chapterDataList.Clear();
            var chapterList = ManagerAdventure.Stage.GetChapterIdxList();

            int chapId = 0;
            foreach (var key in chapterList)
            {
                var chapterData = ManagerAdventure.Stage.GetChapter(key);
                chapterDataList.Add(new StageAdventure_C_ItemData(chapterData.GetBossName(), key, chapterData.chapterBossResId));
                chapId = key > chapId ? key : chapId;
            }

            chapterDataList.Add(new StageAdventure_C_ItemData("", chapId + 1, 0));
        }

        RefreshStageList();

        AnimalListButtonUpdate();
    }

    private int GetFlowerCountMax()
    {
        int count = 0;
        foreach(var chapter in ManagerAdventure.Stage.GetChapterIdxList())
            count++;

        return count;
    }

    private int GetFlowerCount(int clearLevel)
    {
        int clearCount = 0;

        foreach (var chapter in ManagerAdventure.Stage.GetChapterIdxList())
        {
            var progress = ManagerAdventure.User.GetChapterProgress(chapter);

            if (progress == null)
            {
                break;
            }

            if (clearLevel == 1 && progress.chapterCleared)
                clearCount++;
            else if (clearLevel == 2 && progress.bonusStageCleared)
                clearCount++;
                
        }

        return clearCount;
    }

    static public void RefreshStageList()
    {
        int missionCount = 0;
        int maxMissionCount = 0;

        UIPopupStageAdventure._instance.stageDataList.Clear();
        var chapterData = ManagerAdventure.Stage.GetChapter(selectedChapter);
        var chapterProg = ManagerAdventure.User.GetChapterProgress(selectedChapter);

        var stageList = chapterData.GetStageList();
        foreach (var stageData in stageList) 
        {
            StageAdventure_S_ItemData.CheckBoxState clearState = StageAdventure_S_ItemData.CheckBoxState.None;

            if (stageData != null)
            {
                ManagerAdventure.StageReward stageRewards = stageData.rewards;

                maxMissionCount += stageData.mission != 0 ? 1 : 0;

                bool missionClear;
                if (chapterProg == null)
                    missionClear = false;
                else
                    missionClear = chapterProg.stageProgress.ContainsKey(stageData.idx) ? chapterProg.stageProgress[stageData.idx].missionCleared : false;

                missionCount += missionClear ? 1 : 0;

                var stageItem = new StageAdventure_S_ItemData(
                    selectedChapter,
                    stageData.idx, 
                    stageRewards, 
                    stageData.stageType,
                    stageData.mission
                    );

                if (chapterProg != null)
                {
                    var stageProg = chapterProg.GetStageProgress(stageData.idx);
                    if (stageProg != null)
                    {
                        clearState = (StageAdventure_S_ItemData.CheckBoxState)stageProg.clearLevel;
                        stageItem.stageCheck = clearState;
                        stageItem.missionCleared = stageProg.missionCleared;
                    }
                }

                if( stageData.stageType == 0  )
                {
                    UIPopupStageAdventure._instance.stageDataList.Add(stageItem);
                }
                else
                {
                    if(chapterProg != null && chapterProg.state > 0)
                        UIPopupStageAdventure._instance.stageDataList.Add(stageItem);
                }                
            }
        }
        _instance.rewardBox.SetRewardBox(missionCount, maxMissionCount, chapterData.missionClearReward);

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
    }

    public void OnClickAnimalList()
    {
        if (!bCanTouch)
            return;

        bCanTouch = false;

        ManagerUI._instance.OpenPopup<UIPopupStageAdventureAnimal>((popup) => { popup.InitTarget(UIPopupStageAdventureAnimal.PopupMode.ViewMode); });
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        UIPopupStageAdventure.startChapter = 0;
    }

    protected override void OnFocus(bool focus)
    {
        base.OnFocus(focus);

        if(focus)
            bCanTouch = true;
    }

    [SerializeField] UIPokoButton animalListButton;

    [SerializeField] private GameObject objAnimalListBtnEventRibborn;
    [SerializeField] private GameObject objAnimalListBtnSaleBubble;
    [SerializeField] private UILabel labelAnimalListBtnRibborn;
    [SerializeField] private UIUrlTexture texAnimalListButton;
    [SerializeField] private UISprite sprIndexIcon;
    [SerializeField] private UILabel labelAnimalListSummon;

    public void AnimalListButtonUpdate()
    {
        var chapter = ManagerAdventure.User.GetChapterProgress(1);

        if (chapter != null && chapter.stageProgress[1] != null && chapter.stageProgress[1].clearLevel != 0)
        {
            int collaboIndex = ManagerAdventure.GetActiveCollabo();

            bool bEvent =   ManagerAdventure.GetActiveEvent_GachaRateUp() ||
                            ManagerAdventure.GetActiveEvent_MileageGachaEvent();
            

            if (collaboIndex != 0)
            {
                objAnimalListBtnEventRibborn.SetActive(true);
                texAnimalListButton.LoadCDN(Global.adventureDirectory, "Animal/", "bl_c_gacha_" + collaboIndex);
            }
            else if (bEvent)
            {
                objAnimalListBtnEventRibborn.SetActive(true);

                this.gameObject.AddressableAssetLoad<Texture>("local_ui/adventure_gacha_icon", (x) => texAnimalListButton.mainTexture = x);
                animalListButton.GetComponent<TweenScale>().enabled = true;
            }
            else
            {
                objAnimalListBtnEventRibborn.SetActive(false);

                this.gameObject.AddressableAssetLoad<Texture>("local_ui/adventure_gacha_icon", (x) => texAnimalListButton.mainTexture = x);
                animalListButton.GetComponent<TweenScale>().enabled = true;
            }

            if (ManagerAdventure.GetActiveGachaSale())
            {
                objAnimalListBtnEventRibborn.SetActive(true);
                objAnimalListBtnSaleBubble.SetActive(true);
            }
        }
        else
        {
            animalListButton.GetComponent<TweenScale>().enabled = false;

            objAnimalListBtnEventRibborn.SetActive(false);
            this.gameObject.AddressableAssetLoad<Texture>("local_ui/adventure_gacha_icon", (x) => texAnimalListButton.mainTexture = x);

            Color disabledColor = Color.gray;
            sprIndexIcon.color = disabledColor;
            texAnimalListButton.color = disabledColor;
            labelAnimalListSummon.color = disabledColor;

            animalListButton.functionName = "";
        }
    }

    private bool ChallengeClearCheck()
    {
        int chapterCount = ManagerAdventure.Stage.GetChapterIdxList().Count;

        for (int i = 1; i <= chapterCount; i++)
        {
            ManagerAdventure.ChapterInfo chapterInfo = ManagerAdventure.Stage.GetChapter(i);

            foreach (var stage in chapterInfo.stages)
            {
                if(stage.stageType == 1)
                {
                    var chapterProgress = ManagerAdventure.User.GetChapterProgress(i);
                    if (chapterProgress != null)
                    {
                        var stageProg = chapterProgress.GetStageProgress(stage.idx);
                        if (stageProg != null)
                        {
                            if (stageProg.clearLevel > 0)
                                return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void OnDisable()
    {
        if (NewMarkUtility.isView)
            NewMarkUtility.newAnimalDataDelete();
        else
            NewMarkUtility.newAnimalDataSave();
    }
}
