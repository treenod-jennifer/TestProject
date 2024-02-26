using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using Protocol;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopupClear : UIPopupBase
{
    public static UIPopupClear _instance = null;

    public GameObject scoreTextLabel;
    public GameObject rewardTextLabel;

    public UILabel _labelStage;
    public UILabel[] _labelScore;
    public UILabel[] _labelCoin;
    public UILabel[] btnCloseText;
    public UILabel[] btnRetryText;

    public UISprite chapterMissionSprite;

    public GameObject doubleIcon;
    public GameObject HeartGameObject;
    public GameObject _coinGameObject;
    public GameObject heartEffectObj;
    public GameObject mainObj;
    public GameObject btnClose;
    public GameObject btnRetry;
    public GameObject live2DAnchor;

    public GameObject eventCloverObj;

    public SkeletonAnimation spineFlower;

    //스파인 위에 그려질 패널
    public UIPanel panel_upperSpine;

    public UIIngameResultChat resultChat;

    //스페셜이벤트
    public GameObject specialEventRoot;
    public UILabel specialEventLabel;
    public UILabel specialEventLabel1;
    public UIUrlTexture specialEventTexture;

    public GameObject specialEventBubbleRoot;
    public UILabel specialEventRatio;
    public UILabel specialEventRatioShadow;

    //재료모으기이벤트
    public GameObject materialEventRoot;
    public UILabel materialEventLabel;
    public UILabel materialEventLabel1;
    public UIUrlTexture materialEventTexture;

    public static bool materialEventGetReward = false;

    //알파벳 이벤트
    public GameObject alphabetEventRoot;
    public GameObject alphabetRoot_Normal;
    public UILabel alphabetCount_N_Label;
    public UIUrlTexture alphabetTexture_S;

    //코인이벤트
    public GameObject coinEventRoot;
    public UILabel coinEventLabel;

    //점수업
    public UISprite scoreUpSprite;

    public GameObject bestScoreEffectObj;
    public GameObject CoinRoot;

    //스테이지랭킹모드
    public GameObject StageRankRoot;
    public UILabel StageRankBestScoreLabel;
    public UILabel StageRankBestScoreTestLabel;
    public UILabel[] StageRankRatioLabel;
 
    //이펙트
    public GameObject EffectBlockCollect;

    //개편된 에코피 이벤트
    public GameObject PokoFlowerEventRoot;
    public UILabel PokoFlowerEventLabelProgress;
    public UILabel PokoFlowerEventLabelTarget;

    //CoinStash 이벤트
    public GameObject CoinStashEventRoot;

    //DiaStash 이벤트
    public GameObject DiaStashEventRoot;

    //통상스테이지 독려 이벤트
    public UIItemStageAssistMission stageAssistMission;
    
    //앤티크 스토어 이벤트
    public GameObject objAntiqueStoreEventRoot;
    
    //코코의 수사일지 이벤트
    public GameObject criminalEventRoot;

    //이벤트 오브젝트 Table
    public UITable tableEventObjectRight;
    public UITable tableEventObjectLeft;

    //[System.NonSerialized]
    //static public GameClearRespBase _clearResp = null;

    private LAppModelProxy boniLive2D;
    private bool getHeart = false;
    private bool getClover = false;
    private int clearLayer = 1;

    const float MAIN_OBJ_POS_Y = -84;
    int tempScore = 0;

    //표시할 버튼 상태
    private ProceedPlayType proceedPlayType = ProceedPlayType.NONE;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int depth)
    {
        //터치 관련 막음.
        this.bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        clearLayer = layer;
        //클리어창 전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d, spine만 레이어 올려줌.
        if (layer == 10)
        {
            spineFlower.GetComponent<MeshRenderer>().sortingOrder = clearLayer;
            clearLayer += 1;
        }
        else
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            spineFlower.GetComponent<MeshRenderer>().sortingOrder = clearLayer + 1;
            clearLayer += 2;
        }
        panel_upperSpine.useSortingOrder = true;
        panel_upperSpine.sortingOrder = clearLayer + 1;

        resultChat.uiPanel.useSortingOrder = true;
        resultChat.scrollView.panel.useSortingOrder = true;
        resultChat.uiPanel.sortingOrder = clearLayer + 2;
        resultChat.scrollView.panel.sortingOrder = clearLayer + 3;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(int _score)
    {
        //점수정산
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            _score = (int)(_score * 1.1f);
        }
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
        {
            _score = (int)(_score * 1.2f);
        }

        // TopUI보이게 설정.
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        resultChat.SetCollider(false);

        var firstClearRewardTypeFlag = Global.GameInstance.GetFirstClearRewardType();
        if (firstClearRewardTypeFlag == (int)GameType_Base.FirstClearRewardType.NONE)
        {
            HeartGameObject.SetActive(false);
            eventCloverObj.SetActive(false);
        }
        else if ((firstClearRewardTypeFlag & (int)GameType_Base.FirstClearRewardType.STAR) != 0)
        {
            HeartGameObject.SetActive(GameManager.instance.firstClear);
            getHeart = GameManager.instance.firstClear;
        }
        else if ((firstClearRewardTypeFlag & (int)GameType_Base.FirstClearRewardType.CLOVER) != 0)
        {
            eventCloverObj.SetActive(GameManager.instance.gainClover);
            getClover = GameManager.instance.gainClover;
        }

        for (int i = 0; i < _labelScore.Length; i++)
        {
            _labelScore[i].text = string.Format("{0:n0}", _score);
            _labelScore[i].MakePixelPerfect();
        }

        int coinCount = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;// ManagerBlock.instance.coins;//(int)GameData.Asset.AllCoin - Global.coin;
        for (int i = 0; i < _labelCoin.Length; i++)
        {
            _labelCoin[i].text = string.Format("{0}", coinCount);
            _labelCoin[i].MakePixelPerfect();
        }

        _labelStage.text = Global.GameInstance.GetStageText_ClearPopup();
        _labelStage.MakePixelPerfect();
        doubleIcon.SetActive(false);   
        tempScore = _score;

        //챕터미션
        SetStageMission();
        mainObj.transform.localPosition = new Vector3(0, MAIN_OBJ_POS_Y, 0);

        //일반 결과화면 보이지 않도록 설정.
        if (Global.GameInstance.IsHideDefaultResultUI() == true)
        {
            HideDefaultResultUI();
        }

        //게임 타입에 따라 꽃 스파인 설정.
        SetSpineFlower();

        mainObj.transform.localScale = Vector3.zero;
        StartCoroutine(showClearEvent(GameManager.instance.SkipIngameClear));
        GameManager.instance.SkipIngameClear = false;
    }

    private void HideDefaultResultUI()
    {
        HeartGameObject.SetActive(false);
        scoreTextLabel.SetActive(false);
        rewardTextLabel.SetActive(false);
    }

    /// <summary>
    /// 특정 이벤트에서 보상창 재설정이 필요한 경우 사용
    /// </summary>
    private void SetDefaultRewardUI()
    {
        if (Global.GameType == GameType.SPACE_TRAVEL)
        {
            coinEventRoot.SetActive(false);
            scoreTextLabel.SetActive(false);
            _coinGameObject.SetActive(false);
            rewardTextLabel.transform.localPosition = new Vector2(0, rewardTextLabel.transform.localPosition.y);
        }
    }

    private void SetSpineFlower()
    {
        GameObject spineObj = Global.GameInstance.GetSpine_GameClearPopup();
        if (spineObj != null)
        {
            var changeObj = Instantiate(spineObj, this.transform);
            changeObj.transform.localPosition = spineFlower.transform.localPosition;
            changeObj.transform.localScale = spineFlower.transform.localScale;
            SkeletonAnimation changeSpine = changeObj.GetComponent<SkeletonAnimation>();

            if (changeSpine != null)
            {
                changeSpine.GetComponent<MeshRenderer>().sortingOrder = spineFlower.GetComponent<MeshRenderer>().sortingOrder;
                spineFlower.gameObject.SetActive(false);
                spineFlower = changeSpine;
            }
            else
            {
                changeObj.SetActive(false);
            }
        }
    }

    void SetStageMission()
    {
        if (Global.GameType != GameType.NORMAL)
        {
            chapterMissionSprite.gameObject.SetActive(false);
            return;
        }

        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == GameManager.instance.currentChapter(GameManager.instance.CurrentStage) + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    if (GameManager.instance.getDuck == 1)
                    {
                        chapterMissionSprite.gameObject.SetActive(true);
                        chapterMissionSprite.spriteName = "Mission_DUCK_2";
                        chapterMissionSprite.MakePixelPerfect();

                        TweenPosition tweenPos = chapterMissionSprite.gameObject.GetComponent<TweenPosition>();
                        tweenPos.enabled = true;
                        return;
                    }
                    else if (ManagerData._instance._stageData[GameManager.instance.CurrentStage - 1]._missionClear == 0)
                    {
                        chapterMissionSprite.transform.localPosition = new Vector3(-284, 133, 0);
                        chapterMissionSprite.gameObject.SetActive(true);
                        chapterMissionSprite.spriteName = "Mission_DUCK_1";
                        chapterMissionSprite.MakePixelPerfect();
                        return;
                    }
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    if (GameManager.instance.getCandy == 1)
                    {
                        chapterMissionSprite.gameObject.SetActive(true);
                        chapterMissionSprite.spriteName = "Mission_CANDY_2";
                        chapterMissionSprite.MakePixelPerfect();

                        TweenPosition tweenPos = chapterMissionSprite.gameObject.GetComponent<TweenPosition>();
                        tweenPos.enabled = true;
                        return;
                    }
                    else if (ManagerData._instance._stageData[GameManager.instance.CurrentStage - 1]._missionClear == 0)
                    {
                        chapterMissionSprite.transform.localPosition = new Vector3(-284, 133, 0);
                        chapterMissionSprite.gameObject.SetActive(true);
                        chapterMissionSprite.spriteName = "Mission_CANDY_1";
                        chapterMissionSprite.MakePixelPerfect();
                        return;
                    }
                }
            }
        }
        chapterMissionSprite.gameObject.SetActive(false);
    }
    
    public void InitResultChat()
    {
        if (Global.GameType == GameType.NORMAL && GameManager.instance.CurrentStage >= 2 && Global.SkipStageComment() == false )
        {
            resultChat.InitChat(uiPanel.depth + 1, true);
        }
    }

    //검정-> 보니파랑새등장-> 꽃등장 -> 클릭-> 보니퇴장 -> 아웃
    IEnumerator showClearEvent(bool skip)
    {
        float timer = 0;

        //별점수확인
        string clearStartName = "clear_" + ManagerBlock.instance.flowrCount + "_start";
        string clearLoopName = "clear_" + ManagerBlock.instance.flowrCount + "_loop";

        if (Global.GameType == GameType.EVENT)
        {
            clearStartName = "clear_E_start";
            clearLoopName = "clear_E_loop";
        }
        else if ( Global.GameType == GameType.MOLE_CATCH )
        {
            clearStartName = "clear_E_start";
            clearLoopName = "clear_E_loop";
        }
        else if (Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            clearStartName = "clear_Coin_start";
            clearLoopName = "clear_Coin_loop";
        }
        else if (Global.GameType == GameType.BINGO_EVENT)
        {
            clearStartName = "bingo_start";
            clearLoopName = "bingo_loop";
        }
        else if (Global.GameType == GameType.TREASURE_HUNT)
        {
            clearStartName = "Coinevent_start";
            clearLoopName = "Coinevent_loop";
        }

        //게임 타입에 따라 스파인 이름 설정.
        var spineNames = Global.GameInstance.GetSpineAniNames_GameClearPopup(ManagerBlock.instance.flowrCount);
        if(spineNames.Item1 != "")
            clearStartName = spineNames.Item1;
        if (spineNames.Item2 != "")
            clearLoopName = spineNames.Item2;

        //별사운드 추가
        spineFlower.gameObject.SetActive(true);
        if (skip)
        {
            spineFlower.state.SetAnimation(0, clearLoopName, true);

            timer = 0f;
            while (timer < 0.2f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //게임 타입에 따른 결과 꽃 사운드.
            yield return Global.GameInstance.PlaySound_ResultStar(true, tempScore);

            timer = 0;
            mainObj.SetActive(true);
            spineFlower.gameObject.transform.localPosition = new Vector3(0, 191, 0); 
        }
        else
        {
            spineFlower.state.SetAnimation(0, clearStartName, false);
            spineFlower.state.AddAnimation(0, clearLoopName, true, 0f);

            //게임 타입에 따른 결과 꽃 사운드.
            yield return Global.GameInstance.PlaySound_ResultStar(false, tempScore);

            timer = 0;
            mainObj.SetActive(true);

            Vector3 flowerTarget = new Vector3(0, 191, 0);

            while (timer < 1)
            {
                mainObj.transform.localScale = Vector3.one*timer;
                spineFlower.gameObject.transform.localPosition = Vector3.Lerp(new Vector3(0, 56, 0), flowerTarget, timer);
                timer += Global.deltaTimePuzzle * 6f;
                yield return null;
            }

            spineFlower.gameObject.transform.localPosition = flowerTarget;
        }
        
        mainObj.transform.localScale = Vector3.one;
        
        SetCharacter();
        SetStageAssistMission();
        SetSpecialEvent();
        SetAlphabetEvent();
        SetCoinEvent();
        SetMaterialEvent();
        SetDefaultRewardUI();
        SetSpineLocalPosition();

        if ((Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
            || (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1))
        {
            scoreUpSprite.gameObject.SetActive(true);

            if (UIPopupReady.readyItemUseCount[7].Value == 1)
                scoreUpSprite.spriteName = "item_scoreUp20";
            else
                scoreUpSprite.spriteName = "item_scoreUp";

        }

        timer = 0;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        //블록모으기 연출넣기
        //곱하기 생성
        //숫자올리기
        if (Global.specialEventIndex > 0)
        {
            if (ManagerBlock.instance.getSpecialEventBlock > 0)
            {
                timer = 0f;
                while (timer < 0.3f && skip == false)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                
                specialEventBubbleRoot.SetActive(true);
                specialEventRatio.text = "x" + ManagerBlock.instance.getSpecialEventRatio;
                specialEventRatioShadow.text = "x" + ManagerBlock.instance.getSpecialEventRatio;

                //사운드추가
                ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

                timer = 0f;
                while (timer < 0.5f && skip == false)
                {                    
                    timer += Global.deltaTimePuzzle;
                    yield return null;                    
                }

                specialEventBubbleRoot.transform.localScale = Vector3.one;

                if (ManagerBlock.instance.getSpecialEventRatio > 1)
                {

                    if (skip)
                    {
                        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
                        specialEventLabel1.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();

                        specialEventTexture.gameObject.transform.localScale = Vector3.one;
                        specialEventLabel.gameObject.transform.localScale = Vector3.one;
                        specialEventLabel1.gameObject.transform.localScale = Vector3.one;

                        yield return null;

                        GameObject effectObj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, EffectBlockCollect);
                        effectObj.transform.position = specialEventTexture.transform.position;
                        yield return null;
                    }
                    else
                    {
                        int specialCount = ManagerBlock.instance.getSpecialEventBlock;
                        int bonusCount = (ManagerBlock.instance.getSpecialEventRatio - 1) * ManagerBlock.instance.getSpecialEventBlock;
                        int addBonusCount = 1;                    

                        if (bonusCount > 15) //보너스 갯수가 30개보다 크면 시간으로 
                        {
                            addBonusCount = (int)((float)bonusCount / 15f);
                            if (addBonusCount <= 1)
                                addBonusCount = 2;
                        }

                        while (true)
                        {
                            specialCount += addBonusCount;

                            specialEventLabel.text = specialCount.ToString();
                            specialEventLabel1.text = specialCount.ToString();

                            if (specialCount >= ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio)
                            {
                                specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
                                specialEventLabel1.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
                                break;
                            }

                            specialEventTexture.gameObject.transform.localScale = Vector3.one * (1.3f);
                            specialEventLabel.gameObject.transform.localScale = Vector3.one * (1.3f);
                            specialEventLabel1.gameObject.transform.localScale = Vector3.one * (1.3f);
                            //사운드추가
                            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);

                            yield return null;

                            timer = 0;
                            while (timer < 0.03f)
                            {
                                timer += Global.deltaTimePuzzle;
                                specialEventTexture.gameObject.transform.localScale = specialEventTexture.gameObject.transform.localScale * 0.95f;
                                specialEventLabel.gameObject.transform.localScale = specialEventTexture.gameObject.transform.localScale * 0.95f;
                                specialEventLabel1.gameObject.transform.localScale = specialEventTexture.gameObject.transform.localScale * 0.95f;
                                yield return null;
                            }
                            yield return null;
                        }                           

                        specialEventTexture.gameObject.transform.localScale = Vector3.one;
                        specialEventLabel.gameObject.transform.localScale = Vector3.one;
                        specialEventLabel1.gameObject.transform.localScale = Vector3.one;

                        yield return null;

                        GameObject effectObj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, EffectBlockCollect);
                        effectObj.transform.position = specialEventTexture.transform.position;
                        yield return null;
                        

                    }
                }
            }
        }
        
        if (GameManager.instance.firstClear && Global.GameType == GameType.NORMAL )
        {
            UseHeartEffect obj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, ManagerUI._instance._ObjHeartEffect).GetComponent<UseHeartEffect>();  //ManagerObjectPool.Spawn("FlyUseHeart")
            obj.Init(HeartGameObject.transform.position, ManagerUI._instance._StarSprite.transform.position, null, true);
        }

        ManagerSound.AudioPlay(AudioInGame.CONTINUE);

        timer = 0f;
        while (timer < 0.7f && skip == false)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        int coinCount = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;
        int totalCoinCount = Global.coin + coinCount;
        
        if (UIPopupReady.eventGroupClear && Global.GameType == GameType.EVENT)
        {
            int rewardClover = GetEventRewardCloverCount();
            Global.clover = (int)(GameData.Asset.AllClover) - rewardClover;
        }
        else
        {
            Global.clover = (int)(GameData.Asset.AllClover);
        }

        if (skip == false)
        {
            ManagerUI._instance.UpdateUI();
            int addCount = coinCount / 10;
            if (addCount <= 0)
                addCount = 1;

            while(true)
            {
                Global.coin += addCount;
                if (Global.coin >= totalCoinCount)
                {
                    Global.coin = totalCoinCount;
                    ManagerUI._instance.UpdateUI();
                    break;
                }
                ManagerUI._instance.UpdateUI();

                timer = 0;
                while (timer < 0.03f)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                yield return null;
            }
        }
        else
        {
            Global.coin = totalCoinCount;
            ManagerUI._instance.UpdateUI();
        }

        var clearResp = GameManager.instance.clearResp as GameClearResp;
        if (clearResp != null)
        {
            if (clearResp.newAnimal != null)
            {
                bool waitFlag = true;
                ManagerAdventure.OnInit((b)
                    =>
                {
                    ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                    {
                        animalIdx = clearResp.newAnimal.animalId,
                        exp = clearResp.newAnimal.exp,
                        gettime = 0,
                        grade = clearResp.newAnimal.grade,
                        level = clearResp.newAnimal.level,
                        overlap = clearResp.newAnimal.Overlap
                    };

                    ManagerAdventure.User.noticePostponedAnimal = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                    ManagerAdventure.User.SyncFromServer_Animal();
                    ManagerAIAnimal.Sync();
                    waitFlag = false;
                });

                while (waitFlag)
                    yield return null;

                if (Global.GameType != GameType.EVENT)
                {
                    UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, ManagerAdventure.User.noticePostponedAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                    ManagerAdventure.User.noticePostponedAnimal = null;

                    while (UIPopupAdventureSummonAction._instance != null)
                        yield return null;
                }
            }

            if (clearResp.levelUp)
            {
                if (clearResp.HasToy)
                    ManagerLobby._activeGetPokoyura = clearResp.GetFirstToy().index;
                yield return new WaitForSeconds(0.5f);
                if (clearResp.HasToy)
                    ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), clearResp.GetFirstToy().index);
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
            }
            
            if (clearResp.groupRankingEventEnd && ManagerGroupRanking.IsGroupRankingStagePlaying(false))
            {
                //그룹랭킹 플레이 중 참여기간이 종료될 경우 안내 팝업 출력
                yield return ShowGroupRankingPlayEndPopup();
            }
        }

        //그로씨
        Global.GameInstance.SendClearRewardGrowthyLog();
        Global.GameInstance.SendClearGrowthyLog();

        //보상 팝업
        yield return Global.GameInstance.CoOpenClearRewardPopup();

        //버튼 설정
        proceedPlayType = (ProceedPlayType)clearResp.ProceedPlayType;
        SettingButton();

        this.bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;
        InitResultChat();
        
        //터치 가능한 이벤트 아이콘 생성
        SetBlossomEventUI();
        SetCoinStashEvent();
        SetDiaStashEvent();
        SetAntiqueStoreEvent();
        SetCriminalEventUI();
        
        //table 정렬
        tableEventObjectRight.Reposition();
        tableEventObjectLeft.Reposition();

        yield return new WaitForSeconds(0.2f);

        //CoinStash 이벤트 연출 스킵 상관없이 시작
        if (CoinStashEventRoot.activeInHierarchy)
            CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>()?.StartDirecting();
        
        //DiaStash 이벤트 연출 스킵 상관없이 시작
        if(DiaStashEventRoot.activeInHierarchy)
            DiaStashEventRoot.GetComponent<UIItemDiaStashEvent>()?.StartDirecting();
        
        //AntiqueStore 이벤트 연출 스킵 상관 없이 시작
        if(objAntiqueStoreEventRoot.activeInHierarchy)
            objAntiqueStoreEventRoot.GetComponent<UIItemAntiqueStoreEvent>()?.StartDirection();
        
        //CriminalEvent 이벤트 연출 스킵 상관 없이 시작
        if(criminalEventRoot.activeInHierarchy)
            criminalEventRoot.GetComponent<UIItemCriminalEventIcon>()?.StartDirection();
    }

    //에코피 이벤트
    void SetBlossomEventUI()
    {
        if (GameManager.instance.isRunningEcopiEvent == false)
        {
            PokoFlowerEventRoot.SetActive(false);
            return;
        }
        var clearResp = GameManager.instance.clearResp as GameClearResp;
        if (clearResp != null && ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent_Ingame() == true) //신버전 에코피 이벤트
        {
            // 보상 최종수치에 도달 못했거나, 최종수치에 도달한 상태라도 이번에 보상을 받는 경우에만 표시
            if( ManagerPokoFlowerEvent.IsRewardExhausted() == false ||
                (clearResp.pokoFlower != null && clearResp.pokoFlower.get_reward == 1) )
            {
                //획득한 꽃 카운트
                int userFlowerCount = ManagerPokoFlowerEvent.currentPokoFlowerCount;

                //보상을 받을 수 있는 상태에서 꽃 카운트가 0 이면, 타겟 카운트까지 도달한 카운트로 표시해줌.
                if (userFlowerCount == 0 && clearResp != null && clearResp.pokoFlower != null && clearResp.pokoFlower.get_reward == 1)
                {
                    userFlowerCount = ManagerPokoFlowerEvent.targetPokoFlowerCount;
                }

                //획득한 꽃 UI 표시
                PokoFlowerEventLabelProgress.text = userFlowerCount.ToString();
                PokoFlowerEventLabelTarget.text = "/" + ManagerPokoFlowerEvent.targetPokoFlowerCount;
                PokoFlowerEventRoot.SetActive(true);
            }
        }
    }

    void SetMaterialEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_COLLECT_EVENT) && ServerContents.EventChapters.active == 1 && ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.COLLECT)
        {
            if(ManagerBlock.instance.materialCollectEvent > 0)
            {
                if (getHeart)
                {
                    materialEventRoot.transform.localPosition = new Vector3(303,0,0);
                }
                else if (getClover)
                {
                    materialEventRoot.transform.localPosition = new Vector3(223, 0, 0);
                }
                else
                {
                    materialEventRoot.transform.localPosition = new Vector3(133, 0, 0);
                }

                materialEventRoot.SetActive(true);
                materialEventLabel.text = ManagerBlock.instance.materialCollectEvent.ToString();
                materialEventLabel1.text = ManagerBlock.instance.materialCollectEvent.ToString();
                materialEventTexture.SettingTextureScale(60, 60);
                materialEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType);
            }
        }
        else
        {
            materialEventRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 위치 조정이 필요한 결과 스파인의 경우 이쪽 함수에서 세팅
    /// </summary>
    private void SetSpineLocalPosition()
    {
        if (Global.GameType == GameType.SPACE_TRAVEL)
        {
            spineFlower.transform.localPosition = new Vector3(0, 230f, 0);
        }
    }

    void SetStageAssistMission()
    {
        if (ManagerStageAssistMissionEvent.CheckStartable_InGame()
            && ServerContents.StageAssistMissionEventDetails.ContainsKey(ManagerStageAssistMissionEvent.currentMissionIndex))
        {
            stageAssistMission.InitStageAssistMissionData(ServerContents.StageAssistMissionEventDetails[ManagerStageAssistMissionEvent.currentMissionIndex]);

            stageAssistMission.gameObject.SetActive(true);
        }
        else
        {
            stageAssistMission.gameObject.SetActive(false);
        }
    }

    void SetSpecialEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_SPECIAL_EVENT) == false)
            return;

        if (Global.specialEventIndex > 0)
        {
            if (getHeart)
            {
                specialEventRoot.transform.localPosition = new Vector3(303, 0, 0);
            }
            else if (getClover)
            {
                specialEventRoot.transform.localPosition = new Vector3(223, 0, 0);
            }
            else
            {
                specialEventRoot.transform.localPosition = new Vector3(133, 0, 0);
            }


            if (ManagerBlock.instance.getSpecialEventBlock > 0)
            {
                specialEventRoot.SetActive(true);
                specialEventLabel.text = ManagerBlock.instance.getSpecialEventBlock.ToString();// (ManagerBlock.instance.getSpecialEventBlock + ManagerBlock.instance.getSpecialEventRatio).ToString();
                specialEventLabel1.text = ManagerBlock.instance.getSpecialEventBlock.ToString();//(ManagerBlock.instance.getSpecialEventBlock + ManagerBlock.instance.getSpecialEventRatio).ToString();
                specialEventTexture.SettingTextureScale(60, 60);
                specialEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
                specialEventTexture.gameObject.transform.localPosition = new Vector3(-50, -20, 0);
                return;
            }

            
            if (ManagerBlock.instance.getSpecialEventBlock == 0)
            {
                specialEventRoot.SetActive(false);
                return;
            }
            
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
                        specialEventRoot.SetActive(true);
                        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
                        specialEventLabel1.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
                        specialEventTexture.SettingTextureScale(60, 60);
                        specialEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
                        specialEventTexture.gameObject.transform.localPosition = new Vector3(-50, -20, 0);
                        return;
                    }
                    else
                    {
                        specialEventRoot.SetActive(false);
                        return;
                    }
                }
            }
        }
        else
        {
            specialEventRoot.SetActive(false);
        }
    }

    void SetAlphabetEvent()
    {
        if (Global.GameInstance.GetProp(GameTypeProp.CAN_PLAY_ALPHABET_EVENT) == false
          || ManagerAlphabetEvent.instance == null
          || ManagerAlphabetEvent.alphabetIngame.IsStage_ApplyAlphabetEvent == false)
        {
            alphabetEventRoot.SetActive(false);
            return;
        }

        alphabetEventRoot.SetActive(true);

        Vector3 eventRootPos = Vector3.zero;

        if (getHeart)
        {
            eventRootPos = new Vector3(300f, 0f, 0f);
        }
        else if (getClover)
        {
            eventRootPos = new Vector3(220f, 0f, 0f);
        }
        else
        {
            eventRootPos = new Vector3(130f, 0f, 0f);
        }

        //일반 알파벳 UI 표시
        int getCount_N = ManagerAlphabetEvent.alphabetIngame.GetAlphabetKindsCount_NewGain_N();
        if (getCount_N > 0)
        {
            alphabetRoot_Normal.SetActive(true);
            alphabetCount_N_Label.text = getCount_N.ToString();
        }
        else
        {   //새로 획득한 알파벳이 없다면 ui 표시하지 않음.
            alphabetRoot_Normal.SetActive(false);
            eventRootPos -= new Vector3(50f, 0f, 0f);
        }

        //스페셜 알파벳 UI 표시
        if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == true
            && ManagerAlphabetEvent.alphabetIngame.IsGainNewAlphabet_S() == true)
        {
            alphabetTexture_S.gameObject.SetActive(true);
            alphabetTexture_S.SettingTextureScale(60, 60);

            string textureName = ManagerAlphabetEvent.alphabetIngame.GetAppearAlphabetSpriteName_S();
            alphabetTexture_S.LoadCDN(Global.gameImageDirectory, "IconEvent/", textureName);
        }
        else
        {   //새로 획득한 알파벳이 없다면 ui 표시하지 않음.
            alphabetTexture_S.gameObject.SetActive(false);
        }

        alphabetEventRoot.transform.localPosition = eventRootPos;
    }

    void SetCoinEvent()
    {
        if (Global.coinEvent > 0)
        {
            coinEventRoot.SetActive(true);
            coinEventLabel.gameObject.SetActive(true);
            coinEventLabel.text = "x" + Global.coinEvent.ToString();
            StartCoroutine(CoMoveCoinEventObject());
        }
        else
        {
            coinEventRoot.SetActive(false);
            coinEventLabel.gameObject.SetActive(false);
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

    void SetCoinStashEvent()
    {
        // 우주 여행에서는 코인 스태시 적용 X
        if(ManagerCoinStashEvent.CheckStartable() && Global.GameType != GameType.SPACE_TRAVEL)
        {
            CoinStashEventRoot.SetActive(true);
            CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>()?.InitData();
        }
        else
        {
            CoinStashEventRoot.SetActive(false);
        }
    }

    void SetDiaStashEvent()
    {
        if (ManagerDiaStash.CheckStartable())
        {
            DiaStashEventRoot.SetActive(true);
            DiaStashEventRoot.GetComponent<UIItemDiaStashEvent>()?.InitData();
            if(ManagerDiaStash.instance != null)
            {
                ManagerDiaStash.instance.SetStageApplyDiaStash(true);
                
                //스테이지 클리어 횟수 변환
                ManagerDiaStash.instance.SyncPrevStageClearCount(GameManager.instance.clearResp.diaStash.stageClearCount);
                //유저의 보너스 다이아 업데이트
                ManagerDiaStash.instance.SyncPrevBonusDiaCount();
            }
        }
        else
        {
            DiaStashEventRoot.SetActive(false);
            if(ManagerDiaStash.instance != null)
                ManagerDiaStash.instance.SetStageApplyDiaStash(false);
        }
    }

    private void SetAntiqueStoreEvent()
    {
        if (Global.GameType == GameType.NORMAL && ManagerAntiqueStore.CheckStartable() && 
            ServerRepos.User.stage == Global.stageIndex && Global.GameInstance.GetProp(GameTypeProp.IS_EVENT) == false)
        {
            objAntiqueStoreEventRoot.SetActive(true);
            objAntiqueStoreEventRoot.GetComponent<UIItemAntiqueStoreEvent>()?.InitData();
        }
        else
        {
            objAntiqueStoreEventRoot.SetActive(false);

        }
    }
    
    private void SetCriminalEventUI()
    {
        if (Global.GameType == GameType.NORMAL && ManagerCriminalEvent.instance != null && ManagerCriminalEvent.CheckStartable())
        {
            criminalEventRoot.SetActive(true);
            criminalEventRoot.GetComponent<UIItemCriminalEventIcon>()?.InitData();
        }
        else
        {
            criminalEventRoot.SetActive(false);
        }
    }

    private IEnumerator ShowGroupRankingPlayEndPopup()
    {
        var isClosePopup = false;
        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) =>
            {
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_23"), false);
            },
            endCallback: () => { isClosePopup = true; }
        );

        yield return new WaitUntil(() => isClosePopup == true);
    }
    
    void SettingButton()
    {
        string subBtnText = "";
        switch (proceedPlayType)
        {
            case ProceedPlayType.RETRY: //재플레이
                subBtnText = Global._instance.GetString("btn_6");
                break;
            case ProceedPlayType.NEXT: // 다음 스테이지
                subBtnText = Global._instance.GetString("btn_33");
                break;
        }

        //추가적인 버튼을 사용하는 경우 설정
        bool isUseSubButton = (proceedPlayType == ProceedPlayType.RETRY || proceedPlayType == ProceedPlayType.NEXT);
        if (isUseSubButton == true)
        {
            btnRetry.SetActive(true);
            btnClose.transform.localPosition = new Vector3(-130f, -163f, 0f);
            btnRetryText[0].text = subBtnText;
            btnRetryText[1].text = subBtnText;
        }

        //확인버튼
        string closeBtnText = Global._instance.GetString("btn_1");
        btnCloseText[0].text = closeBtnText;
        btnCloseText[1].text = closeBtnText;
        btnClose.SetActive(true);
    }

    int GetEventRewardCloverCount()
    {
        int eventGroup = ManagerData._instance._eventChapterData._groupState;
        CdnEventChapter eventChapter = ServerContents.EventChapters;
        int rCnt = eventChapter.rewards[eventGroup - 1].Count;
        for (int j = 0; j < rCnt; j++)
        {
            if (eventChapter.rewards[eventGroup - 1][j].type == 1)
            {
                return eventChapter.rewards[eventGroup - 1][j].value;
            }
        }
        return 0;
    }

    void SetCharacter()
    {
        boniLive2D = LAppModelProxy.MakeLive2D(live2DAnchor, TypeCharacterType.Boni);
        boniLive2D.SetRenderer(false);
        boniLive2D.SetAnimation("Clear_appear", "Clear_loop");
        StartCoroutine(boniLive2D.CoSetRenderer(true));
        boniLive2D.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        boniLive2D.SetScale(450f);
        boniLive2D.SetSortOrder(clearLayer);
    }

    #region 확인 버튼 눌렀을 때 동작
    protected override void OnClickBtnClose()
    {
        if (this.bCanTouch == false)
            return;

        if (UIPopupPanelEmotionIcon._instance != null)
            return;

        this.bCanTouch = false;
        switch (proceedPlayType)
        {
            case ProceedPlayType.REBOOT:
                Reboot();
                break;
            default:
                GoToLobby();
                return;
        }
    }

    private void Reboot()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_65"), false);
        popupSystem._callbackEnd += () => Global.ReBoot();
    }

    private void GoToLobby()
    {
        ManagerSound._instance.StopBGM();
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
        }));
    }
    #endregion

    #region 서브 버튼 선택했을 때 동작
    void OnClickBtnRetry()
    {
        if (this.bCanTouch == false)
            return;

        if (UIPopupPanelEmotionIcon._instance != null)
            return;

        this.bCanTouch = false;
        switch (proceedPlayType)
        {
            case ProceedPlayType.RETRY:
                RetryCurrentStage();
                break;
            case ProceedPlayType.NEXT:
                PlayNextStage();
                break;
            default:
                this.bCanTouch = true;
                return;
        }
    }

    private void RetryCurrentStage()
    {
        Global.SetStageIndex(GameManager.instance.CurrentStage);
        bool isSingleRoundEvent = Global.isSingleRoundEvent;
        //다음 스테이지 진입 취소할 경우, isSingleRoundEvent를 이전 상태로 변경해줌.
        ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch, callBackCancel: () =>
        {
            Global.isSingleRoundEvent = isSingleRoundEvent;
        });
    }

    private void PlayNextStage()
    {
        int globalStageIdx = Global.stageIndex;
        int nextStageIdx = GameManager.instance.CurrentStage + 1;
        if (nextStageIdx <= ManagerData._instance.maxStageCount)
        {
            Global.SetStageIndex(nextStageIdx);
            bool isSingleRoundEvent = Global.isSingleRoundEvent;
            //다음 스테이지 진입 취소할 경우, Global.StageIndex, isSingleRoundEvent를 이전 상태로 변경해줌.
            ManagerUI._instance.OpenPopupReadyUsePrevTargetData(OnTouch,
                callBackCancel: () =>
                {
                    Global.SetStageIndex(globalStageIdx);
                    Global.isSingleRoundEvent = isSingleRoundEvent;
                });
        }
        else
        {
            this.bCanTouch = true;
        }
    }
    #endregion

    void OnTouch()
    {
        this.bCanTouch = true;
    }

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


    public void OnClickBtnOK ()
    {
        this.resultChat.OnClickBtnHandler();
    }

    public void OnClickBtnDisLike ()
    {
        this.resultChat.OnClickBtnDisLike();
    }

    public void OnClickPokoFlowerOpen()
    {
        ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ManagerPokoFlowerEvent.PokoFlowerEventIndex, true));
    }
}