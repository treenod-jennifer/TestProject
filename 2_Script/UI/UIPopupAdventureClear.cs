using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using DG.Tweening;
using Newtonsoft.Json;

public class UIPopupAdventureClear : UIPopupBase, ManagerAdventure.IUserAnimalListener
{
    public static Protocol.AdventureGameClearResp _clearResp = null;

    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo[] animals = new UIItemAdventureAnimalInfo[3];
    [SerializeField] private Animation[] animalsAni = new Animation[3];
    [SerializeField] private UITexture[] animalBoard = new UITexture[3];
    [SerializeField] private UIButtonAdventureLevelUp[] levelUpButtons = new UIButtonAdventureLevelUp[3];
    [SerializeField] private GenericReward[] rewards;
    [SerializeField] private UIItemAdvtureRewardBox[] boxs;
    [SerializeField] private SkeletonAnimation ClerRainbowSpine;
    [SerializeField] private GameObject clear_Root;
    [SerializeField] private UIPanel overUIPanel;
    [SerializeField] private GameObject particleObj;
    [SerializeField] private GameObject[] skillTitles;

    //CoinStash 이벤트
    [SerializeField] private GameObject CoinStashEventRoot;
    
    //DiaStash 이벤트
    [SerializeField] private GameObject DiaStashEventRoot;

    //이벤트 오브젝트 Table
    [SerializeField] private UITable tableEventObject;

    // 법률 대응 관련 위치/활성화 변경 필요한 오브젝트
    [SerializeField] private Transform AniRoot;
    [SerializeField] private List<Transform> AnimalList;
    [SerializeField] private Transform btnOK;
    [SerializeField] private GameObject labelBuyInfo;
    
    public override void OpenPopUp(int depth)
    {
        ManagerSound._instance.StopBGM();
        ManagerSound.AudioPlay(AudioLobby.adventure_clear);

        InitPopup();


        bCanTouch = false;
        foreach (var button in levelUpButtons)
            button.bCanTouch = false;


        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;

        if (LanguageUtility.IsShowBuyInfo)
        {
            labelBuyInfo.SetActive(true);
            AniRoot.localPosition = new Vector2(AniRoot.localPosition.x, 70f);
            btnOK.localPosition = new Vector2(btnOK.localPosition.x, -460f);
            foreach (var item in AnimalList)
                item.localPosition = new Vector2(item.localPosition.x, -100f);
        }

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        StartCoroutine(CoAction(openTime, () =>
        {
            ManagerUI._instance.bTouchTopUI = true;
            if (_callbackOpen != null)
                _callbackOpen();

            ManagerUI._instance.FocusCheck();
        }));
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 스파인 레이어만 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }        

        ClerRainbowSpine.GetComponent<MeshRenderer>().sortingOrder = layer;
        overUIPanel.useSortingOrder = true;
        overUIPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    void Awake()
    {
        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Add(this);

        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].changeStartEvent += () =>
            {
                foreach (var button in levelUpButtons)
                {
                    ManagerUI._instance.bTouchTopUI = false;
                    button.bCanTouch = false;
                }
            };

            animals[i].changeEndEvent += () =>
            {
                foreach (var button in levelUpButtons)
                {
                    ManagerUI._instance.bTouchTopUI = true;
                    button.ResetButton();
                    button.bCanTouch = true;
                }
            };
            animals[i].levelUpStartEvent += levelUpEffect;
        }
    }

    [SerializeField] private GameObject levelUpEffectObj;
    [SerializeField] private AnimationCurve sizeAniController;
    private IEnumerator levelUpEffect(UIItemAdventureAnimalInfo target)
    {
        while (!target.isFullShotLoaded)
            yield return new WaitForSeconds(0.1f);

        GameObject effect = NGUITools.AddChild(target.gameObject, levelUpEffectObj);
        effect.transform.localPosition = new Vector3(0.0f, 220.0f, 0.0f);


        float totalTime = 0.0f;
        float endTime = sizeAniController.keys[sizeAniController.length - 1].time;

        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            target.FullshotObject.transform.localScale = Vector3.one * sizeAniController.Evaluate(totalTime);

            if (totalTime >= endTime)
                break;

            yield return null;
        }
    }

    void OnDestroy()
    {
        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Remove(this);

        base.OnDestroy();
    }

    private void InitPopup()
    {
        for (int i = 0; i < 3; i++)
        {
            ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            animals[i].SetAnimalSelect(aData);
            levelUpButtons[i].ResetButton();
        }

        ManagerAdventure.User.SyncFromServer_Animal();

        DOTween.ToAlpha(() => blackSprite.color, x => blackSprite.color = x, 1f, 0.5f);
        StartCoroutine(showClearEvent());
    }

    public void OnAnimalChanged(int animalIdx)
    {
        for(int i = 0; i < 3; ++i)
        {
            if( animals[i].AnimalIdx == animalIdx )
            {
                ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
                animals[i].SetAnimalSelect(aData);
                levelUpButtons[i].ResetButton();
            }
        }
    }

    protected override void OnFocus(bool focus)
    {
        if( focus )
        {
            RePaintButtons();
        }
    }

    public void RePaintButtons()
    {
        for (int i = 0; i < levelUpButtons.Length; i++)
            levelUpButtons[i].ResetButton();
    }

    //private IEnumerator CoBlind()
    //{
    //    blackSprite.alpha = 0.0f;

    //    while(blackSprite.alpha <= 1.0f)
    //    {
    //        blackSprite.alpha += Global.deltaTimeNoScale * 1.5f;
    //        yield return null;
    //    }
    //}

    private IEnumerator CoWaitResp()
    {
        if (_clearResp == null)
            NetworkLoading.MakeNetworkLoading(0.0f);

        while (_clearResp == null)
            yield return null;

        NetworkLoading.EndNetworkLoading();
    }

    private IEnumerator CoSetReward(params System.Func<IEnumerator>[] openEvents)
    {
        List<Reward> rData = new List<Reward>();

        if (_clearResp.rewards != null && _clearResp.rewards.Count != 0)
        {
            rData.AddRange(_clearResp.rewards);
        }
        if (_clearResp.boxes != null && _clearResp.boxes.Count != 0)
        {
            List<Reward> reward = null;
            _clearResp.boxes.TryGetValue(AdventureManager.instance.TreasureType, out reward);
            if(reward != null)
                rData.AddRange(reward);
        }


        UIGrid grid = rewards.Length != 0 ? rewards[0].GetComponentInParent<UIGrid>() : null;
        int boxCount = _clearResp.boxes == null ? 0 : _clearResp.boxes.Count;

        for (int i = 0; i < rData.Count; i++)
        {
            if (i < _clearResp.rewards.Count)
            {
                if (rData[i].type == (int)RewardType.flower && _clearResp.IsChallenge() )
                    rewards[i].AddSpriteOverride("stage_icon_level_03", "stage_icon_level_04");

                rewards[i].SetReward(rData[i]);
                rewards[i].gameObject.SetActive(true);
                grid.Reposition();

                //탐험모드 보상
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    rData[i].type,
                    rData[i].value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PLAY,
                    null
                    );

            }
            else if(i < _clearResp.rewards.Count + boxCount)
            {
                List<Reward> reward = null;
                _clearResp.boxes.TryGetValue(AdventureManager.instance.TreasureType, out reward);
                if (reward != null)
                {
                    int index = i - _clearResp.rewards.Count;
                    boxs[index].SetReward(reward[index]);
                    boxs[index].gameObject.SetActive(true);
                    grid.Reposition();

                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward[index].type,
                        reward[index].value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PLAY,
                        null
                        );
                }
            }
        }

        for(int i=0; i<boxCount; i++)
        {
            yield return boxs[i].CoGetReward(openEvents[i]);
        }
    }

    private void CoSetAnimal()
    {
        for(int i=0; i<animalsAni.Length; i++)
        {
            animalsAni[i].transform.localScale = Vector3.one;
            animalsAni[i].Play();
        }
        for (int i = 0; i < animalBoard.Length; i++)
        {
            UITexture tempTexture = animalBoard[i];
            tempTexture.transform.DOScale(0.5f, 0.4f);
            DOTween.ToAlpha(() => tempTexture.color, x => tempTexture.color = x, 1f, 0.3f);
        }
    }

    private void SetExp()
    {
        for (int i=0; i< animals.Length; i++)
        {
            animals[i].ChangeAnimal_Ani(ManagerAdventure.User.GetAnimalFromDeck(1, i));
        }
    }

    private bool isAnimalAniRun()
    {
        for (int i = 0; i < animals.Length; i++)
        {
            if (animals[i].isChangeAniRun)
                return true;
        }

        return false;
    }

    IEnumerator CoRankupCheck()
    {
        if (_clearResp.levelUp == false)
            yield break;

        bool newToyArrived = _clearResp.userToy != null && _clearResp.userToy.Count > 0;

        if (newToyArrived)
            ManagerLobby._activeGetPokoyura = _clearResp.userToy[0].index;

        yield return new WaitForSeconds(0.5f);
        if (newToyArrived)
            ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), _clearResp.userToy[0].index);
        else
            ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), 0);
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (UIPopupRankUp._instance == null)
                break;
            yield return null;
        }
        //포코유라 업데이트.
        ManagerUI._instance.SettingRankingPokoYura();
        yield break;
    }

    IEnumerator showClearEvent()
    {
        for (int i = 0; i < animalsAni.Length; i++)
            animalsAni[i].transform.localScale = Vector3.zero;
        for (int i = 0; i < animalBoard.Length; i++)
        {
            animalBoard[i].transform.localScale = Vector3.zero;
            animalBoard[i].color = new Color(1f, 1f, 1f, 0f);
        }

        ClerRainbowSpine.transform.localPosition = new Vector3(0, 200f, 0);
        ClerRainbowSpine.state.SetAnimation(0, "1_appear", false);
        ClerRainbowSpine.state.AddAnimation(0, "1_idle", true, 0);

        CoSetAnimal();
        yield return new WaitForSeconds(1.3f);

        particleObj.SetActive(true);
        clear_Root.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        ClerRainbowSpine.transform.DOLocalMoveY(372f, 0.3f);

        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        ManagerUI._instance.bTouchTopUI = true;
        ManagerUI._instance.UpdateUI();

        yield return CoWaitResp();

        yield return CoSetReward(CoSummon);

        yield return new WaitForSeconds(0.5f);
        SetExp();

        yield return CoRankupCheck();

        yield return new WaitWhile(() => { return isAnimalAniRun(); });

        //CoinStash 이벤트 연출 스킵 상관없이 시작
        if (ManagerCoinStashEvent.CheckStartable())
        {
            CoinStashEventRoot.SetActive(true);
            UIItemCoinStashEvent itemCoinStashEvent = CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>();
            if (itemCoinStashEvent != null)
            {
                itemCoinStashEvent.InitData();
                itemCoinStashEvent.StartDirecting();
            }
        }

        if (ManagerDiaStash.CheckStartable())
        {
            DiaStashEventRoot.SetActive(true);
            UIItemDiaStashEvent itemDiaStashEvent = DiaStashEventRoot.GetComponent<UIItemDiaStashEvent>();
            if (itemDiaStashEvent != null)
            {
                itemDiaStashEvent.InitData();
                itemDiaStashEvent.StartDirecting();
                if(ManagerDiaStash.instance != null)
                {
                    ManagerDiaStash.instance.SetStageApplyDiaStash(true);
                    
                    //스테이지 클리어 횟수 변환
                    ManagerDiaStash.instance.SyncPrevStageClearCount(GameManager.instance.clearResp.diaStash.stageClearCount);
                    //유저의 보너스 다이아 업데이트
                    ManagerDiaStash.instance.SyncPrevBonusDiaCount();
                }
            }
        }
        else
        {
            DiaStashEventRoot.SetActive(false);
            if(ManagerDiaStash.instance != null)
                ManagerDiaStash.instance.SetStageApplyDiaStash(false);
        }
        
        //table 정렬
        tableEventObject.Reposition();

        Global.GameInstance.SendClearGrowthyLog();
        Global.GameInstance.SendClearRewardGrowthyLog();
        bCanTouch = true;
    }

    private IEnumerator CoSummon()
    {
        if (_clearResp.userAnimals != null && _clearResp.userAnimals.Count > 3)
        {
            var respAnimal = _clearResp.userAnimals[3];
            bool waitFlag = true;
            ManagerAdventure.OnInit((b)
                =>
            {
                ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                {
                    animalIdx = respAnimal.animalId,
                    exp = respAnimal.exp,
                    gettime = 0,
                    grade = respAnimal.grade,
                    level = respAnimal.level,
                    overlap = respAnimal.Overlap
                };

                /*
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)RewardType.animal,
                    respAnimal.animalId,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                    null
                    );
                */

                ManagerAdventure.User.noticePostponedAnimal = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                ManagerAdventure.User.SyncFromServer_Animal();
                ManagerAIAnimal.Sync();
                waitFlag = false;
            });

            while (waitFlag)
                yield return null;

            UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, ManagerAdventure.User.noticePostponedAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
            ManagerAdventure.User.noticePostponedAnimal = null;

            while (UIPopupAdventureSummonAction._instance != null)
                yield return null;
        }
    }

    protected override void OnClickBtnClose()
    {
        if (UIPopupPanelEmotionIcon._instance == null)
        {
            if (this.bCanTouch == false)
                return;
            this.bCanTouch = false;

            ManagerSound._instance.StopBGM();

            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            StartCoroutine(CoEvent(0.5f, () =>
            {
                ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
                SceneLoading.MakeSceneLoading("Lobby");

                Global.day = (int)GameData.User.day;
                Global.clover = (int)(GameData.Asset.AllClover);
                Global.coin = (int)(GameData.Asset.AllCoin);
                Global.jewel = (int)(GameData.Asset.AllJewel);
                Global.wing = (int)(GameData.Asset.AllWing);
                Global.exp = (int)GameData.User.expBall;
                myProfile.SetStage((int)ServerRepos.User.stage);

                //모험모드 창열기, 스테이지 챕터 저장
                UIPopupStageAdventure.startChapter = Global.chapterIndex;
            }));
            
            //SendClearGrowthyLog();
        }
    }

    //void SendClearGrowthyLog()
    //{
    //    //그로씨 넣기
    //        List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM> itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();

    //        for (int i = 4; i < 7; i++)
    //        {
    //            if (GameItemManager.useCount[i] > 0)
    //            {
    //                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
    //                {
    //                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
    //                    L_IID = ((GameItemType)i + 1).ToString(),
    //                    L_CNT = GameItemManager.useCount[i]
    //                };
    //                itemList.Add(readyItem);
    //            }
    //        }
    //        if (GameManager.instance.useContinueCount > 0)
    //        {
    //            var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
    //            {
    //                L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
    //                L_IID = "Continue",
    //                L_CNT = GameManager.instance.useContinueCount
    //            };
    //            itemList.Add(readyItem);
    //        }
    //        var docItem = JsonConvert.SerializeObject(itemList);


    //        var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
    //            (
    //            ManagerData._instance.userData.userID,
    //            (ManagerData._instance.userData.stage - 1).ToString(),
    //            GameManager.instance.GrowthyAfterStage.ToString(),
    //            Global.GameInstance.GetGrowthyStageIndex(),
    //            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE,
    //            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
    //            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL,
    //            0,
    //            ManagerBlock.instance.coins,
    //            (long)(Time.time - GameManager.instance.playTime),
    //            GameManager.instance.firstPlay,//최초플레이
    //            GameManager.instance.useContinueCount > 0,
    //            0,   //남은턴 다시계산
    //            docItem,
    //            GameManager.instance.firstClear ? 1 : 0,
    //            GameManager.instance.clearMission > 0,
    //            GameManager.instance.allStageClearReward > 0,
    //            GameManager.instance.stageReview,
    //            Global.chapterIndex,
    //            null,
    //            "N", 
    //            boostLevel: "0"
    //            );


    //        //사용동물
    //        List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL> animalList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL>();
    //        for (int i = 0; i < 3; i++)
    //        {
    //            var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);

    //                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL
    //                {
    //                    L_CID = animalData.idx,
    //                    L_LEV = animalData.level,
    //                    L_CNT = animalData.overlap
    //                };
    //                animalList.Add(readyItem);
    //        }
    //        var docAnimal = JsonConvert.SerializeObject(animalList);
    //        playEnd.L_CHAR = docAnimal;

    //        var doc = JsonConvert.SerializeObject(playEnd);
    //        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

    //        if (GameManager.instance.firstClear)
    //        {
    //            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
    //                (
    //                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
    //                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ADVENTURE,
    //                    string.Format("CHAP_{0}_STAGE_{1}",Global.chapterIndex.ToString(), Global.stageIndex.ToString()),
    //                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
    //                );

    //            var chapProg = ManagerAdventure.User.GetChapterProgress(Global.chapterIndex);
    //            if( chapProg == null )
    //            {
    //                achieve.L_NUM1 = 1;
    //            }
    //            else
    //            {
    //                var stageProg = chapProg.GetStageProgress(Global.stageIndex);
    //                achieve.L_NUM1 = stageProg == null ? 1 : stageProg.playCount + 1;
    //            }
                
    //            var d = JsonConvert.SerializeObject(achieve);
    //            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    //        }
    //}

    IEnumerator CoEvent(float tempTime, UnityAction tempAction)
    {
        float timer = 0;

        while (timer < tempTime)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        tempAction();
        yield return null;
    }

    private bool isChallenge(int chapIdx, int stageIdx)
    {
        int key = chapIdx * 100000 + stageIdx;

        var stageList = ManagerAdventure.Stage.GetStageOrderList(true);

        if (stageList[key] != null && stageList[key].stageType == 1)
            return true;
        else
            return false;
    }
}
