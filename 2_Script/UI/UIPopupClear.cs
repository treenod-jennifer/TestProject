using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

using Newtonsoft.Json;
using DG.Tweening;

public class UIPopupClear : UIPopupBase
{
    public static UIPopupClear _instance = null;

    public UILabel _labelStage;
    public UILabel[] _labelScore;
    public UILabel[] _labelCoin;
    public UILabel[] btnCloseText;
    public UILabel[] btnRetryText;

    public UISprite chapterMissionSprite;

    public GameObject doubleIcon;
    public GameObject HeartGameObject;
    public GameObject heartEffectObj;
    public GameObject mainObj;
    public GameObject btnClose;
    public GameObject btnRetry;
    public GameObject live2DAnchor;
    
    public SkeletonAnimation spineFlower;
    
    public UIContainerEmotionIcon emotionIconContainer;
    public UIIngameResultChat resultChat;


    //스페셜이벤트
    public GameObject specialEventRoot;
    public UILabel specialEventLabel;
    public UILabel specialEventLabel1;
    public UIUrlTexture specialEventTexture;

    //재료모으기이벤트
    public GameObject materialEventRoot;
    public UILabel materialEventLabel;
    public UILabel materialEventLabel1;
    public UIUrlTexture materialEventTexture;

    //코인이벤트
    public GameObject coinEventRoot;
    public UILabel coinEventLabel;

    //점수업
    public GameObject scoreUpRoot;

    [System.NonSerialized]
   // static public GameClearResp _clearResp = null;

    private LAppModelProxy boniLive2D;
    private bool getHeart = false;
    private int clearLayer = 1;

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
        resultChat.uiPanel.useSortingOrder = true;
        resultChat.scrollView.panel.useSortingOrder = true;
        resultChat.uiPanel.sortingOrder = clearLayer + 1;
        resultChat.scrollView.panel.sortingOrder = clearLayer + 2;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(int _score)
    {
        // TopUI보이게 설정.
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        resultChat.SetCollider(false);

        //점수체크
        /*
        if (UIPopupReady.readyItemUseCount[1].Value == 1 && Global.eventIndex == 0)
        {
            _score = (int)(_score * 1.1f);
        }
        */
        if (Global.eventIndex == 0)
        {
            HeartGameObject.SetActive(GameManager.instance.firstClear);
        }
       

        for (int i = 0; i < _labelScore.Length; i++)
        {
            _labelScore[i].text = string.Format("{0}", _score);
            _labelScore[i].MakePixelPerfect();
        }

        int coinCount = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;// ManagerBlock.instance.coins;//(int)GameData.Asset.AllCoin - Global.coin;
        for (int i = 0; i < _labelCoin.Length; i++)
        {
            _labelCoin[i].text = string.Format("{0}", coinCount);
            _labelCoin[i].MakePixelPerfect();
        }

        if (Global._systemLanguage == CurrLang.eJap)
        {
            _labelStage.text = string.Format("ステージ {0}", UIPopupReady.stageIndex);
        }
        else
        {
            _labelStage.text = string.Format("STAGE {0}", UIPopupReady.stageIndex);
        }
        _labelStage.MakePixelPerfect();

        /*if(보상 2배 이벤트 한다면)
        {
            doubleIcon.SetActive(true);
            Vector3 pos = HeartGameObject.transform.localPosition;
            HeartGameObject.transform.localPosition = new Vector3(pos.x + 23f, pos.y, pos.z);
        }
        else*/
        {
            doubleIcon.SetActive(false);
        }

        //챕터미션
        if (Global.eventIndex <= 0)
            SetStageMission();



        mainObj.transform.localScale = Vector3.zero;

        //코인체크

        StartCoroutine(showClearEvent());

        //하트 증가.
        //GlobalGameManager.instance._nCurrentHeart++;
        //GlobalGameManager.instance.SaveHeart();

        //_resultChatUI = NGUITools.AddChild(ManagerGameUI.instanse._objChatRoot, _objChatRoot).GetComponent<ResultChatUI>();
        //_resultChatUI._trResultChat.localScale = Vector3.one;
        //_resultChatUI.InitChat("ingame_clear_boni", (_uiPanel.depth + _nPanelCount));

        /*
         * //획득한 별에 따라 스파인 설정.
        spineFlower.gameObject.GetComponent<MeshRenderer>().sortingOrder = _uiPanel.depth;
        int startCount = Global._star.Value;
        if (startCount == 1)
        {
            spineFlower.state.SetAnimation(0, "001", false);
            spineFlower.state.AddAnimation(0, "002", true, 0f);
        }
        else if (startCount == 2)
        {
            spineFlower.state.SetAnimation(0, "003", false);
            spineFlower.state.AddAnimation(0, "004", true, 0f);
        }
        else if (startCount == 3)
        {
            spineFlower.state.SetAnimation(0, "005", false);
            spineFlower.state.AddAnimation(0, "006", true, 0f);
        }
        else if (startCount == 4)
        {
            spineFlower.state.SetAnimation(0, "007", false);
            spineFlower.state.AddAnimation(0, "008", true, 0f);
        }
        else if (startCount == 5)
        {
            spineFlower.state.SetAnimation(0, "009", false);
            spineFlower.state.AddAnimation(0, "010", true, 0f);
        }
        spineFlower.state.TimeScale = 1f;
        */

        /*
         * //사운드 설정, 이펙트 연출.
        int sountS = Global._star.Value;
        sountS = Mathf.Clamp(sountS, 0, 3);
        for (int i = 0; i < sountS; i++)
        {
            yield return new WaitForSeconds(0.3f);
            Global.AudioPlay(Global._instanse._audioResultStar[i], AudioPriority.eSystem);
            if (i == 0)
                yield return new WaitForSeconds(0.2f);
            else if (i == 1 && sountS == 3)
                yield return new WaitForSeconds(0.5f);
            if (i == 2)
                Global.AudioPlay(Global._instanse._audioResultStar[3], AudioPriority.eSystem);
        }
        GlobalGameManager.instance._nInGameResult = 1;

        FlyHeart flyHeart = NGUITools.AddChild(ManagerMenu.instance._btnJewelShop.gameObject, flyHeartObj).GetComponent<FlyHeart>();
        flyHeart.type = GrowthyType.HEART;
        flyHeart._delay = 0.05f;
        flyHeart._startStart = heartUIObj.transform.position;
        flyHeart.transform.position = heartUIObj.transform.position;
        flyHeart._startEnd = ManagerMenu.instance._btnJewelShop.transform.position;*/
    }

    void SetStageMission()
    {
        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == GameManager.instance.currentChapter(UIPopupReady.stageIndex) + 1)
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
                    else if (ManagerData._instance._stageData[UIPopupReady.stageIndex - 1]._missionClear == 0)
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
                    else if (ManagerData._instance._stageData[UIPopupReady.stageIndex - 1]._missionClear == 0)
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

    public bool showResultChat = false;
    public void InitResultChat()
    {
     /*   if( GameData.LoginCdn.skipStageComment == 0 )
        {
            resultChat.InitChat(uiPanel.depth + 1);
            // 이모션 아이콘 수정
            this.emotionIconContainer.gameObject.SetActive(true);
            //this.emotionIconContainer.InitData(ManagerReviewBoardProcess.instance.idCurPlayStageArticle);
            showResultChat = false;
        }*/
    }

    //검정-> 보니파랑새등장-> 꽃등장 -> 클릭-> 보니퇴장 -> 아웃
    IEnumerator showClearEvent()
    {
        float timer = 0;

        //ManagerBlock.instance.flowrCount = Random.Range(1, 4);
        // yield return null;

        //별점수확인
        string clearStartName = "clear_" + ManagerBlock.instance.flowrCount + "_start";
        string clearLoopName = "clear_" + ManagerBlock.instance.flowrCount + "_loop";

        if (Global.eventIndex > 0)
        {
            clearStartName = "clear_5_start";
            clearLoopName = "clear_5_loop";
        }

        //별사운드 추가
        spineFlower.gameObject.SetActive(true);
        spineFlower.state.SetAnimation(0, clearStartName, false);
        spineFlower.state.AddAnimation(0, clearLoopName, true, 0f);



        timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
        ManagerSound.AudioPlay(AudioInGame.RESULT_STAR1);

        if(ManagerBlock.instance.flowrCount > 1)
        {
            timer = 0f;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);
        }

        if (ManagerBlock.instance.flowrCount > 2)
        {
            timer = 0f;
            while (timer < 0.7f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
        }
        if (ManagerBlock.instance.flowrCount > 3)
        {
            timer = 0f;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR4);
        }


        timer = 0f;
        while (timer < 0.6f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        timer = 0;
        /*
        while (timer < 1)
        {
            spineFlower.gameObject.transform.localPosition = Vector3.Lerp(new Vector3(0, 56, 0), new Vector3(0, 256, 0), timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        spineFlower.gameObject.transform.localPosition = new Vector3(0, 256, 0);

        //꽃위로 올리기
        */
        timer = 0;
        mainObj.SetActive(true);

        while (timer < 1)
        {
            mainObj.transform.localScale = Vector3.one*timer;
            spineFlower.gameObject.transform.localPosition = Vector3.Lerp(new Vector3(0, 56, 0), new Vector3(0, 256, 0), timer);
            timer += Global.deltaTimePuzzle * 6f;
            yield return null;
        }
        mainObj.transform.localScale = Vector3.one;
        spineFlower.gameObject.transform.localPosition = new Vector3(0, 256, 0);
        SetCharacter();
        SetSpecialEvent();
        SetCoinEvent();
        SetMaterialEvent();

        if (Global.eventIndex == 0 && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            scoreUpRoot.SetActive(true);
        }


        timer = 0;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        if (GameManager.instance.firstClear && Global.eventIndex == 0)
        {
            //ManagerSound.AudioPlay(AudioInGame.APPLE);
            UseHeartEffect obj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, ManagerUI._instance._ObjHeartEffect).GetComponent<UseHeartEffect>();  //ManagerObjectPool.Spawn("FlyUseHeart")
        //    obj.Init(HeartGameObject.transform.position, ManagerUI._instance._StarSprite.transform.position, null, true);
            //GameManager.instance.firstClear = false;
        }


        ManagerSound.AudioPlay(AudioInGame.CONTINUE);
        timer = 0f;
        while (timer < 0.7f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        int coinCount = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins; //ManagerBlock.instance.coins;// (int)GameData.Asset.AllCoin - Global.coin;
        int totalCoinCount = Global.coin + coinCount;// ManagerBlock.instance.coins;
        
        if (UIPopupReady.eventGroupClear && Global.eventIndex > 0)
        {
            int rewardClover = GetEventRewardCloverCount();
         //   Global.clover = (int)(GameData.Asset.AllClover) - rewardClover;
        }
        else
        {
          //  Global.clover = (int)(GameData.Asset.AllClover);
        }

        ManagerUI._instance.UpdateUI();
        int addCount = coinCount / 10;
        if (addCount <= 0)
            addCount = 1;


        //for (int j = 0; j < coinCount; j++)
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


        //Global.coin = (int)GameData.Asset.AllCoin;

        /*
        if (_clearResp != null)
        {
            if (_clearResp.levelUp)
            {
                if (_clearResp.HasToy)
                    ManagerLobby._activeGetPokoyura = _clearResp.GetFirstToy().index;
                yield return new WaitForSeconds(0.5f);
                if (_clearResp.HasToy)
                    ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), _clearResp.GetFirstToy().index);
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
        }
        */
        if (GameManager.instance.allStageClearReward > 0)
        {
            yield return new WaitForSeconds(0.5f);

            string msgText = Global._instance.GetString("n_s_8");
            if(GameManager.instance.allStageClearReward >1) msgText = Global._instance.GetString("n_s_9");
            Texture2D texture = Resources.Load("UI/invite_icon") as Texture2D;

            UIPopupSystem popupConfirm = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            //popupConfirm.gameObject.transform.parent = ManagerUI._instance.topCenterPanel.transform;
            UIPanel systemPanel = popupConfirm.gameObject.GetComponent<UIPanel>();
            popupConfirm.InitSystemPopUp(Global._instance.GetString("p_t_4"), msgText, false, texture, () =>
            {
                StartCoroutine(CoAllStageClearReward());
                ManagerSound.AudioPlayMany(AudioInGame.GET_HEART);
            });

            ManagerSound.AudioPlay(AudioInGame.PRAISE0);

            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                if (UIPopupSystem._instance == null)
                    break;
                yield return null;
            }

        }

        SettingButton();
        this.bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;
        showResultChat = true;
        yield return null;
    }

    void SetMaterialEvent()
    {/*
        if (Global.eventIndex > 0 && ServerContents.GetEventChapter(Global.eventIndex).active == 1 && ServerContents.GetEventChapter(Global.eventIndex).type == (int)EVENT_CHAPTER_TYPE.COLLECT)
        {
            if(ManagerBlock.instance.materialCollectEvent > 0)
            {
                materialEventRoot.SetActive(true);
                materialEventLabel.text = ManagerBlock.instance.materialCollectEvent.ToString();
                materialEventLabel1.text = ManagerBlock.instance.materialCollectEvent.ToString();
                materialEventTexture.SettingTextureScale(60, 60);
                materialEventTexture.Load(Global.gameImageDirectory, "IconEvent/", "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType);
            }
        }
        else
        {
            materialEventRoot.SetActive(false);
        }*/
    }

    void SetSpecialEvent()
    {
        if (Global.eventIndex > 0) return;
        /*
        if(Global.specialEventIndex > 0)
        {
            if (Global.eventIndex == 0 && Global.specialEventIndex > 0)
            {
                if(ManagerBlock.instance.getSpecialEventBlock > 0)
                {
                    specialEventRoot.SetActive(true);
                    specialEventLabel.text = ManagerBlock.instance.getSpecialEventBlock.ToString();
                    specialEventLabel1.text = ManagerBlock.instance.getSpecialEventBlock.ToString();
                    specialEventTexture.SettingTextureScale(60, 60);
                    specialEventTexture.Load(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
                    specialEventTexture.gameObject.transform.localPosition = new Vector3(-50, -20, 0);
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

                        if(getCount < maxGetCount)
                        {
                            specialEventRoot.SetActive(true);
                            specialEventLabel.text = ManagerBlock.instance.getSpecialEventBlock.ToString();
                            specialEventLabel1.text = ManagerBlock.instance.getSpecialEventBlock.ToString();
                            specialEventTexture.SettingTextureScale(60, 60);
                            specialEventTexture.Load(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
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
        }
        else
        {
            specialEventRoot.SetActive(false);
        }*/
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

    void SettingButton()
    {

        bool onRetryBtn = true;
        /*
        if (_clearResp != null)
            if (_clearResp.levelUp)
                onRetryBtn = false;
                */
                
        if (GameManager.instance.firstClear == false && Global.eventIndex == 0 && onRetryBtn)
        {
            string retryText = Global._instance.GetString("btn_6");
            btnRetryText[0].text = retryText;
            btnRetryText[1].text = retryText;
            btnRetry.SetActive(true);
            btnClose.transform.localPosition = new Vector3(-130f, -163f, 0f);
        }
        else
        {
           // GameManager.instance.firstClear = false;
        }

        string closeText = Global._instance.GetString("btn_1");
        btnCloseText[0].text = closeText;
        btnCloseText[1].text = closeText;
        btnClose.SetActive(true);
    }


    int GetEventRewardCloverCount()
    {
        int eventGroup = ManagerData._instance._eventChapterData[Global.eventIndex]._groupState;

        /*
        CdnEventChapter eventChapter = ServerContents.GetEventChapter(Global.eventIndex);//ServerContents.EventChapters[i + 1];
        int rCnt = eventChapter.rewards[eventGroup - 1].Count;
        for (int j = 0; j < rCnt; j++)
        {
            if (eventChapter.rewards[eventGroup - 1][j].type == 1)
            {
                return eventChapter.rewards[eventGroup - 1][j].value;
            }
        }
        */
        /*
        for (int i = 0; i < ServerContents.EventChapters.Count; i++)
        {
            if (ServerContents.EventChapters[i + 1].index == Global.eventIndex)
            {
                CdnEventChapter eventChapter = ServerContents.GetEventChapter(Global.eventIndex);//ServerContents.EventChapters[i + 1];
                int rCnt = eventChapter.rewards[eventGroup - 1].Count;
                for (int j = 0; j < rCnt; j++)
                {
                    if (eventChapter.rewards[eventGroup - 1][j].type == 1)
                    {
                        return eventChapter.rewards[eventGroup - 1][j].value;
                    }
                }
            }
        }*/
        return 0;
    }

    IEnumerator CoAllStageClearReward()
    {/*
        int coinCount = ServerRepos.LoginCdn.AllClearRewards[GameManager.instance.allStageClearReward - 1];
        int totalCoinCount = Global.coin + coinCount;
        Global.clover = (int)(GameData.Asset.AllClover);
        ManagerUI._instance.UpdateUI();
        
        int addCount = coinCount / 10;
        if (addCount <= 0)
            addCount = 1;
            
        float timer = 0;

        //ManagerSound.AudioPlay(AudioInGame.PRAISE1);

        while (true)
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
        }*/
        yield return null;
    }

    /*
    public override void ClosePopUp(float _startTime = 0.3f)
    {
        StartCoroutine(CoAction(_startTime - 0.01f, () =>
        {
            //팝업 전부 없애고 로비로 이동.

            GlobalGameManager.instance._bOnPopUpUI = false;
            GlobalGameManager.instance.SetGameMode(eGameMode.None);
            GlobalGameManager.instance.LoadScene(eSceneNameType.Lobby);
        }));
    }*/

    void SetCharacter()
    {
        boniLive2D = NGUITools.AddChild(live2DAnchor, ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni].obj).GetComponent<LAppModelProxy>();
        boniLive2D.setAnimation(false, "Clear_appear");    //모션.
        boniLive2D.SetScale(450f);
        boniLive2D.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        boniLive2D._CubismRender.SortingOrder = clearLayer;
    }

    protected override void OnClickBtnClose()
    {
        if ( UIPopupPanelEmotionIcon._instance == null )
        {
            if ( this.bCanTouch == false )
                return;
            this.bCanTouch = false;

            ManagerSound._instance.StopBGM();
        //    ManagerSound.AudioPlay(AudioInGame.STAGE_CLEAR_BUTTON);
        /*
            StartCoroutine(CoEvent(0.5f, () =>
            {
                ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
                SceneLoading.MakeSceneLoading("Lobby");

                  Global.day = (int)GameData.User.day;
                  Global.clover = (int)(GameData.Asset.AllClover);
                  Global.coin = (int)(GameData.Asset.AllCoin);
                  Global.jewel = (int)(GameData.Asset.AllJewel);
                  ManagerData._instance.userData.stage = ( int ) ServerRepos.User.stage;
            }));
            */
        }
    }

    void OnClickBtnRetry()
    {
        if ( UIPopupPanelEmotionIcon._instance == null )
        {
            if ( this.bCanTouch == false )
                return;
            this.bCanTouch = false;

//            Global.stageIndex = UIPopupReady.stageIndex;
            ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch);
        }
    }

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



}
