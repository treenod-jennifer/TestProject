using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity;
using DG.Tweening;
using UnityEditor;

public enum TARGET_TYPE
{
    CRACK,
    KEY,
    STATUE,
    SHOVEL,
    JEWEL,
    BOMB_COLLECT,//SCORE,
    COLORBLOCK,
    BLACK,  //사용하지 않음
    ICE,
    CARPET,
    FLOWER_POT,
    COUNT_CRACK1,
    COUNT_CRACK2,
    COUNT_CRACK3,
    COUNT_CRACK4,
    PEA,
    FLOWER_INK,
    SPACESHIP,
    BOMB_ALL,
    BOMB_LINE,
    BOMB_CIRCLE,
    BOMB_RAINBOW,
    HEART,
    BREAD_1,
    BREAD_2,
    BREAD_3,
}

public class GameUIManager : MonoSingletonOnlyScene<GameUIManager>
{
    readonly Vector3 GAME_ITEM_POS = new Vector3(-70, 75, 0);   //60

    //최상위 패널
    public GameObject TopUIRoot;

    //게임타겟
    public GameObject targetObj;
    public GameObject Target_Root;
    public List<GameMissionTarget> listGameTarget = new List<GameMissionTarget>();
    public List<GameObject> listUiObj = new List<GameObject>();

    public UISprite targetBG;
    public UISprite targetIcon;

    //턴
    public GameObject MoveCount_Root;
    public UILabel moveCountLabel;
    public UILabel moveCountLabelShadow;
    public GameObject moveCountAddTurnBubble;
    public UILabel moveCountAddTurnLabel;
    public AnimationCurve _curveAddApple = new AnimationCurve();
    public GameObject turnUi;
    public UISprite turnSpirte;
    public UISprite turnShadowSpirte;
    public UISprite turnIceSpirte;
    public UISprite turnLightSprite;

    public UILabel[] turnLabelList;

    //점수
    public GameObject scoreRoot;
    public UISlider scoreSlider;
    public UILabel scoreLabel;
    public GameObject gaugeRoot;

    //코인
    public GameObject coinRoot;
    public UILabel coinLabel;
    public ParticleSystem coinParticle;
    public UISprite coinSprite;


    //인게임아이템
    public GameObject gameItemObj;
    public List<GameItem> listGameItem = new List<GameItem>();
    public GameObject gameItemManagerObj;
    public GameObject gameItemRoot;

    //인게임루트
    public GameObject AnchorBottom;
    public GameObject groundAnchor;
    public GameObject UI_Root;
    public Transform groundMoveTransform;
    public GameObject Effect_Root;
    public GameObject gyroRoot;
    public GameObject gyroSpawn_Root;

    //땅파기모드
    public UIPanel block_Panel;
    public GameObject DigModeBG;
    public GameObject DigModeBG1;
    public GameObject DigModeBlockBG;

    //땅파기 게이지
    public GameObject DigGauge_Root;
    public GameObject digGradation;
    const float digGradStart = 50;
    const float digGradEnd = -56;

    //일시정지버튼
    public GameObject pauseButton;

    //챕터미션관련
    public UISprite CandySprite;

    //별표시
    int tempFlowerCount = 0;
    int flowerClearState = 0;
    public GameObject flowerGaugeRoot;
    public SkeletonAnimation[] flowerSpineObj;
    public GameObject[] flowerGauge;
    public List<UIWidget> iphoneXWidget = new List<UIWidget>();
    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    //스페셜이벤트
    public GameObject specialEventRoot;
    public UIUrlTexture specialEventObj;
    public UILabel specialEventLabel;

    //재료모으기이벤트
    public GameObject materialEventRoot;
    public UIUrlTexture materialEventObj;
    public UILabel materialEventLabel;

    int materialTurnCount = -1;
    bool isGetSpecialEvent = false;

    //재료정해지기전
    public GameObject materialRoutteRoot;
    public UILabel materialRoutteCountLabel;
    public UILabel[] materialRoutteRatioLabel;

    public UISprite[] materialBubbleBefore;
    public UISprite[] materialBubbleAfter;

    //점수업
    public GameObject scoreUpRoot;

    public GameObject InGameCamera;

    //랭킹모드
    public GameObject rankModeRoot;
    public UILabel[] rankScoreLabel;
    public UILabel[] targetLabel;

    //랭킹모드 게이지
    public GameObject myScoreObj;
    public UILabel myScoreLabel;

    public AnimationCurve rankPos;
    public AnimationCurve rankScl;

    public GameObject rankItemObj;
    public GameObject rank1stObj;

    //스테이지랭킹
    public GameObject stageRankRoot; //루트

    public UILabel stageRankTurnCountLabel;
    public UISprite[] stageRankBubbleBefore;
    public UISprite[] stageRankBubbleAfter;
    public UILabel[] stageRankBonusLabel;

    public GameObject stageRankScoreRoot;
    public UILabel[] stageRankScoreLabel;

    //빨간꽃모으기
    public GameObject RedFlowerRoot;
    //public UILabel RedFlowerLabel;
    public UISprite RedFlowerEcopi;
    public UISprite RedFlowerSprite;
    
    public bool IsRedFlowerGetEvent = false;
    public bool IsBlueFlowerGetEvent = false;
    public bool IsWhiteFlowwerGetEvent = false;

    public int RedFlowerGetCount = 0;
    public int RedFlowerTargetCount = 0;

    //모으고자 하는 아이템 스프라이트
    public UISprite collectItemSprite;

    //샌드박스메세지
    public UILabel sandboxLabel;

    int stageRankTurnCount = 5;

//    public List<UIRankInGameItem> listRankItem = new List<UIRankInGameItem>();

    public int rankMaxScore = 0;

    int rankTargetSocre = 0;
    bool isFirstRank = false;
    bool testRank = false;
    bool isMoveBird = false;
    float targetBirdPos = 0f;
    
    //모험모드
    public GameObject Advance_Root;
    public GameObject Advance_Gaige_Root;
    public GameObject Advanture_BG_Root;
    public GameObject Advanture_Block_BG;
    public GameObject Advanture_Grass;

    bool isChargeAdvantureGaige = false;
    public UISlider advantureGaigeSlider;
    public UISprite advantureGaigeBomb;
    public UILabel advantureWaveLabel;
    public SkeletonAnimation advantureBGSkeleton;
//    public UIItemBGController adventureBGSky;
    public UISprite adventureEffectBG;

    //모험모드 보상
    public GameObject adventure_RewardRoot;
    public UILabel adventureCoinLabel;
    public ParticleSystem coinAdventureParticle;

    public GameObject adventureBoxRoot;
    public UILabel adventureBoxLabel;
    public UILabel adventureComboLabel;

    //배경
    public GameObject ingameBGSprites;

    //에디터에서 사용
    public GameObject objScreenLine;
    public BoxCollider editPopupCollider;
    public GameObject[] hideObjectsAtSaveMap;

    //툴 명 출력
    public UILabel toolNameLabel;

    #region 턴 컬러
    private Color defaultEffectColor_turn = new Color(118f / 255f, 72f / 255f, 36f / 255f);
    private Color warningEffectColor_turn = new Color(88f / 255f, 4f / 255f, 0f);
    private Color warningColor_turn = new Color(239f / 255f, 54f / 255f, 86f / 255f);
    #endregion

    //목표 UI 카운트 갱신해주기 위한 리스트.
    private List<int> listTargetUICount = new List<int>();

    //자이로 오브젝트 리스트.    
    private List<IngameGyroObj> listGyroObj = new List<IngameGyroObj>();

    //자이로 연출 코루틴.
    private Coroutine gyroAddForceAction = null;

    //수집형 아이템 카운트 연출 코루틴.
    private Coroutine collectItemCountAction = null;

    void Awake()
    {
        foreach (var obj in Liststatue) Destroy(obj);
        Liststatue.Clear();
    }

    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            if ((float)Screen.height / (float)Screen.width > 2f || (float)Screen.width / (float)Screen.height > 2f)
            {
                for (int i = 0; i < iphoneXWidget.Count; i++)
                { iphoneXWidget[i].topAnchor.absolute = -100;
                    iphoneXWidget[i].UpdateAnchors();
                }

                for (int i = 0; i < iphoneXWidgetY.Count; i++)
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 85;
                    iphoneXWidgetY[i].UpdateAnchors();
                }
            }
        }

        if(EditManager.instance != null)
        {
            SetToolName();
        }
    }

    public void SetToolName()
    {
        toolNameLabel.text = Application.productName;
    }


    IEnumerator DoMoveBird()
    {
        while (true)
        {
            if (myScoreObj.transform.localPosition.x == targetBirdPos)
            {
                break;
            }
            myScoreObj.transform.localPosition = Vector3.MoveTowards(myScoreObj.transform.localPosition, new Vector3(targetBirdPos, 0 ,0), Global.deltaTimePuzzle * 450f);
            yield return null;
        }
        isMoveBird = false;
        yield return null;
    }

    public void SetScore_Rank(int tempScore)
    {
/*
        rankScoreLabel[0].text = string.Format("{0:n0}", tempScore);//tempScore.ToString();
        rankScoreLabel[1].text = string.Format("{0:n0}", tempScore);//tempScore.ToString();

        float pos = 0;

        int tempScoreA = ManagerBlock.instance.score;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            tempScoreA = (int)(tempScoreA * (1.1f + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
        }
        else
        {
            tempScoreA = (int)(tempScoreA * (1 + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
        }

        //게이지정렬
        if (isFirstRank == true && rankMaxScore < tempScoreA)
        {
            myScoreObj.transform.localPosition = new Vector3(300, 0, 0);
            pos = 600;
        }
        else
        {
            float posRatio = ((float)(tempScoreA)) / ((float)rankMaxScore);
            // myScoreObj.transform.localPosition = new Vector3(-300 + 600 * posRatio, 0, 0);
            pos = 600 * posRatio;

            if (isMoveBird == false)
            {
                isMoveBird = true;
                targetBirdPos = -300 + 600 * posRatio;
                StartCoroutine(DoMoveBird());
            }
        }


        myScoreLabel.text = string.Format("{0:n0}", tempScore);

        for (int i = 0; i < listRankItem.Count; i++)
        {
            if (listRankItem[i] != null)
            {
                listRankItem[i].SetItem(tempScore, pos);
            }
        }
*/
        return;
    }

    public void SetScore_Normal(int tempScore)
    {
        if (flowerClearState == 0)
            scoreSlider.value = (float)tempScore / (float)ManagerBlock.instance.stageInfo.score3;
        else if (flowerClearState > 1 && IsRedFlowerGetEvent)
            scoreSlider.value = (float)tempScore / ((((float)ManagerBlock.instance.stageInfo.score4))*1.1f);
        else if (flowerClearState > 0)
            scoreSlider.value = (float)tempScore / ((float)ManagerBlock.instance.stageInfo.score4);


        if (tempFlowerCount < 1 && tempScore > ManagerBlock.instance.stageInfo.score1)
        {
            tempFlowerCount = 1;
            flowerSpineObj[0].state.SetAnimation(0, "appear_1", false);
            flowerSpineObj[0].state.AddAnimation(0, "idle2", true, 0);
        }

        if (tempFlowerCount < 2 && tempScore > ManagerBlock.instance.stageInfo.score2)
        {
            tempFlowerCount = 2;

            flowerSpineObj[1].state.SetAnimation(0, "appear_1", false);
            flowerSpineObj[1].state.AddAnimation(0, "idle2", true, 0);

            flowerSpineObj[1].state.Complete += delegate
            {
                flowerSpineObj[0].state.SetAnimation(0, "idle2", true);
            };

        }
        if (tempFlowerCount < 3 && tempScore > ManagerBlock.instance.stageInfo.score3)
        {
            tempFlowerCount = 3;

            flowerSpineObj[2].state.SetAnimation(0, "appear_1", false);
            flowerSpineObj[2].state.AddAnimation(0, "idle2", true, 0);

            flowerSpineObj[2].state.Complete += delegate
            {
                flowerSpineObj[0].state.SetAnimation(0, "idle2", true);
                flowerSpineObj[1].state.SetAnimation(0, "idle2", true);
            };

            ////에코피이펙트넣기
            if (IsWhiteFlowwerGetEvent)
                StartCoroutine(DoShowEcoPiAnimation());
        }

       if (flowerClearState > 1)
        {
            if (IsRedFlowerGetEvent && tempFlowerCount < 5 && tempScore > (ManagerBlock.instance.stageInfo.score4 * 1.1f))
            {
                tempFlowerCount = 5;

                flowerSpineObj[4].state.SetAnimation(0, "appear_1", false);
                flowerSpineObj[4].state.AddAnimation(0, "idle2", true, 0);

                flowerSpineObj[4].state.Complete += delegate
                {
                    flowerSpineObj[3].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[2].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[0].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[1].state.SetAnimation(0, "idle2", true);
                };
                flowerSpineObj[4].transform.localPosition = new Vector3(235, 10, -5);

                //에코피이펙트넣기
                StartCoroutine(DoShowEcoPiAnimation());
            }
            else if (tempFlowerCount < 4 && tempScore > ManagerBlock.instance.stageInfo.score4)
            {
                tempFlowerCount = 4;

                flowerSpineObj[3].state.SetAnimation(0, "appear_1", false);
                flowerSpineObj[3].state.AddAnimation(0, "idle2", true, 0);

                flowerSpineObj[3].state.Complete += delegate
                {
                    flowerSpineObj[2].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[0].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[1].state.SetAnimation(0, "idle2", true);
                };
                flowerSpineObj[3].transform.localPosition += new Vector3(0, 0, -3);
            }
        }
        else if (flowerClearState > 0)
        {
            if (tempFlowerCount < 4 && tempScore > ManagerBlock.instance.stageInfo.score4)
            {
                tempFlowerCount = 4;

                flowerSpineObj[3].state.SetAnimation(0, "appear_1", false);
                flowerSpineObj[3].state.AddAnimation(0, "idle2", true, 0);

                flowerSpineObj[3].state.Complete += delegate
                {
                    flowerSpineObj[2].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[0].state.SetAnimation(0, "idle2", true);
                    flowerSpineObj[1].state.SetAnimation(0, "idle2", true);
                };
                flowerSpineObj[3].transform.localPosition = new Vector3(235, 10, -3);

                if (IsBlueFlowerGetEvent)                
                    StartCoroutine(DoShowEcoPiAnimation());                
            }
        }
    }

    IEnumerator DoShowEcoPiAnimation()
    {
        float waitTimer = 0f;
        bool changeLabel = false;
        yield return null;

        RedFlowerEcopi.spriteName = "ecopi_event_ingame2";
        RedFlowerEcopi.MakePixelPerfect();

        RedFlowerSprite.gameObject.SetActive(true);

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            float ratio = ManagerBlock.instance._curveBlockPopUp.Evaluate(waitTimer);
            RedFlowerSprite.transform.localScale = Vector3.one * ratio;

            if (changeLabel == false && waitTimer > 0.5f)
            {
                changeLabel = true;
                //RedFlowerLabel.text = (GetCount+1) + " / " + TargetCount;
                InGameEffectMaker.instance.MakeDuckEffect(RedFlowerSprite.transform.position);
                ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
            }
            yield return null;
        }

        RedFlowerRoot.transform.localScale = Vector3.one;
        yield return null;
    }

    public void SetScore(int tempScore)
    {
        scoreLabel.text = tempScore.ToString();
		SetScore_Normal(tempScore);
/*
        if (Global.GameType == GameType.RANK)
        {
            SetScore_Rank(tempScore);
        }
        else if( Global.GameType == GameType.NORMAL)
        {
            SetScore_Normal(tempScore);
        }
        else if (Global.GameType == GameType.EVENT)
            return;

        //점수업
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            scoreUpRoot.SetActive(true);
        }
*/
    }

    public void SetCoin(int tempCoin)
    {
        coinLabel.text = tempCoin.ToString();

        coinParticle.gameObject.SetActive(true);
        coinParticle.Play();

        //코인획득효과
        StartCoroutine(CoAddCoin());
    }

    float waitTimerCoin = 0f;
    IEnumerator CoAddCoin()
    {
        waitTimerCoin = 0;
        while (waitTimerCoin < 1f)
        {
            waitTimerCoin += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimerCoin);
            coinSprite.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f) * 1f);

            yield return null;
        }
    }

    //게임시작할때 UI정렬
    public void GameStart()
    {
        SetDigMode();
        SetFlower();
        SetUI();
        SetGameItem();
        SetChapterMission();
        SetCoin(ManagerBlock.instance.coins);
        SetSpecialEvent();
        //SetMaterialEvent();
        SetWorldRanking();
        SetEndContents();
        SetScore(0);
        //SetStageRanking();
        SetAdvanceMode();
        SetCoinStage();
        SetListGyroObject();
    }

    public void SetAdventureCoin(int tempCoin)
    {
        adventureCoinLabel.text = tempCoin.ToString();

        coinAdventureParticle.gameObject.SetActive(true);
        coinAdventureParticle.Play();
    }

    void SetAdvanceMode()
    {
        if (GameManager.gameMode != GameMode.ADVENTURE)
            return;

        Target_Root.SetActive(false);
        turnUi.SetActive(false);
        scoreRoot.SetActive(false);
        coinRoot.SetActive(false);

        Advance_Root.SetActive(true);
        Advanture_BG_Root.SetActive(true);
        Advance_Gaige_Root.SetActive(true);
        Advanture_Block_BG.SetActive(true);
        Advanture_Grass.SetActive(false);

        advantureGaigeSlider.value = 0;
        ingameBGSprites.SetActive(false);

        advantureWaveLabel.gameObject.SetActive(true);

        adventure_RewardRoot.SetActive(true);
        adventureCoinLabel.text = "0";
    }

    public void SetAdvantureWave(int waveCount, int totalWaveCount)
    {
        advantureWaveLabel.text = "Wave " + waveCount + "/" + totalWaveCount;
    }

    public void SetAdvantureBG(string aniNum, bool loop)
    {
        advantureBGSkeleton.state.ClearTracks();
        advantureBGSkeleton.state.SetAnimation(0, aniNum, loop);

        if(aniNum == "Boss_wave")
        {
            //adventureBGSky.ChangeBG(1);
        }
    }

    void SetSpecialEvent()
    {
/*
        if (Global.GameInstance.CanPlay_SpecialEvent() == false)
            return;

        if (Global.specialEventIndex > 0)
        {
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
                        isGetSpecialEvent = true;
                        specialEventRoot.SetActive(true);
                        specialEventLabel.text = "0";
                        specialEventObj.Load(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);

                        materialRoutteRoot.SetActive(true);
                        materialTurnCount = 5;
                    }
                }
            }
        }
*/
    }

/*
    public void RemoveSpecialEventCount()
    {
        if (Global.specialEventIndex == 0)
            return;

        if (Global.GameInstance.CanPlay_SpecialEvent() == false)
            return;

        if (materialTurnCount <= 0)
            return;

        if (isGetSpecialEvent == false)
            return;

        materialTurnCount--;
        materialRoutteCountLabel.text = materialTurnCount + "ターン後";
        StartCoroutine(DoBubbleAnim(materialRoutteRoot)); 

        if (materialTurnCount == 0)
        {
            StartCoroutine(DoSetRoutte()); 
        }
    }

    IEnumerator DoBubbleAnim(GameObject tempObj)
    {
        float waitTimer = 0f;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 2;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 4);

            tempObj.transform.localScale = Vector3.one * (1 + (0.3f * scaleRatio) * (1 - waitTimer));            
            yield return null;
        }
        tempObj.transform.localScale = Vector3.one;            
        yield return null;
    }

    [System.NonSerialized]
    public bool isCompleteSpecialEventSettings = false;
    IEnumerator DoSetRoutte()
    {
        float waitTimer = 0f;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        while (ManagerBlock.instance.state == BlockManagrState.MOVE)
        {
            yield return null;
        }

        //확률설정
        int RatioRandom = Random.Range(0, 1000);
        int materialRatio = 1;
        if (RatioRandom > 990)
            materialRatio = 10;
        else if (RatioRandom > 950)
            materialRatio = 5;
        else if (RatioRandom > 800)
            materialRatio = 3;
        else if (RatioRandom > 500)
            materialRatio = 2;

        //룰렛돌기
        int[] ratioList = new int[] { 1, 2, 3, 5, 10 };
        waitTimer = 0f;

        materialRoutteCountLabel.gameObject.SetActive(false);
        materialRoutteRatioLabel[0].gameObject.transform.localPosition = new Vector3(-2.8f, 4.3f, 0);

        //사운드재생 SPECIAL_EVENT_ROUTTE
        ManagerSound.AudioPlay(AudioInGame.SPECIAL_EVENT_ROUTTE);

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle;

            int count = (int)(waitTimer * 5f);
            count = count % ratioList.Length;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 10);

            foreach (var temp in materialRoutteRatioLabel)
            {
                temp.text = "x" + ratioList[count].ToString();
                temp.gameObject.transform.localScale = Vector3.one * (0.8f + 0.3f * scaleRatio);
            }

            materialRoutteRoot.transform.localScale = Vector3.one * (1f + 0.05f * scaleRatio);

            foreach (var temp in materialBubbleBefore)
                temp.alpha = 1 - waitTimer * 4;

            foreach (var temp in materialBubbleAfter)
            {
                if (waitTimer * 4 > 1f)
                    temp.color = Color.white;
                else 
                temp.color = new Color(1, 1, 1, waitTimer * 4); 
            }

            yield return null;
        }

        waitTimer = 0f;
        while (waitTimer < 0.1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //이펙트 사운드
        if (materialRatio > 1)
        {
            InGameEffectMaker.instance.MakeDuckEffect(materialRoutteRatioLabel[0].transform.position);
        }
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

        isCompleteSpecialEventSettings = true;
        ManagerBlock.instance.getSpecialEventRatio = materialRatio;
        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
        specialEventLabel.color = Color.yellow;

        waitTimer = 0f;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle* 2f;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 5) * (1 - waitTimer);//ManagerBlock.instance._curveBlockJump.Evaluate(waitTimer);

            foreach (var temp in materialRoutteRatioLabel)
            {
                temp.text = "x" + materialRatio.ToString();
                temp.gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 1f);
                temp.alpha = waitTimer * 2f;
            }
            
            materialBubbleAfter[0].gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 0.5f);

            yield return null;
        }

        foreach (var temp in materialRoutteRatioLabel)
            temp.text = "x" + materialRatio.ToString();

        waitTimer = 0f;
        while (true)
        {
            if (materialRoutteRoot == null)
                break;

            waitTimer += Global.deltaTimePuzzle;
            float ratio = Mathf.Sin(Mathf.PI * waitTimer);
            materialRoutteRoot.transform.localScale = new Vector3(1 + 0.08f * ratio, 1 - 0.08f * ratio, 0);
            yield return null;
        }


        yield return null;
    }
*/

    public void SetMaterialEvent()
    {
/*
        if (Global.GameInstance.CanPlay_CollectEvent() && ServerContents.GetEventChapter(Global.eventIndex).active == 1 && ServerContents.GetEventChapter(Global.eventIndex).type == (int)EVENT_CHAPTER_TYPE.COLLECT)    //일반 이벤트 스테이지는 아니고
        {
            string blockName = "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType;
            materialEventRoot.SetActive(true);
            materialEventLabel.text = "0";
            materialEventObj.Load(Global.gameImageDirectory, "IconEvent/", blockName);
        }
*/
    }

    //인게임 블럭 모으기 카운트 갱신
    public void RefreshMaterial()
    {
        StartCoroutine(CoChangeCount(materialEventObj.transform));
        materialEventLabel.text = ManagerBlock.instance.materialCollectEvent.ToString();
    }

    //껌딱지 모으기 카운트 갱신
    public void RefreshSpecialEvent()
    {
        StartCoroutine(CoChangeCount(specialEventObj.transform));
        specialEventLabel.text = (ManagerBlock.instance.getSpecialEventBlock * ManagerBlock.instance.getSpecialEventRatio).ToString();
    }

    //생성된 자이로 오브젝트들 삭제, 리스트 초기화.
    public void SetListGyroObject()
    {
        for (int i = 0; i < listGyroObj.Count; i++)
        {
            Destroy(listGyroObj[i].gameObject);
        }
        listGyroObj.Clear();
    }

    #region 월드랭킹 UI 설정
    private void SetWorldRanking()
    {
        if (Global.GameType == GameType.WORLD_RANK)
        {
            gyroRoot.SetActive(true);

            //월드랭킹 수집품 UI 설정
            if (collectItemCountAction != null)
            {
                StopCoroutine(collectItemCountAction);
                collectItemCountAction = null;
            }
            materialEventRoot.SetActive(true);
            /*if (ManagerWorldRanking.instance != null)
            {
                if (ManagerWorldRanking.resourceData?.worldRankingPack?.IngameAtlas != null)
                    collectItemSprite.atlas = ManagerWorldRanking.resourceData.worldRankingPack.IngameAtlas;
            }*/
            collectItemSprite.gameObject.SetActive(true);
            materialEventLabel.text = "0";
        }
    }

    public void CollectAction_WorldRankingItem(int itemCount)
    {
        //이전에 진행되던 연출 있으면 멈추기.
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemSprite.MakePixelPerfect();
        }

        //재료 카운트 표시 마지막으로 획득한 카운트까지 갱신해줌.
        materialEventLabel.text = ManagerBlock.instance.worldRankingItemCount.ToString();

        //연출에 필요한 카운트는 따로 저장.
        int actionCount = ManagerBlock.instance.worldRankingItemCount;
        if (itemCount > 8)
        {
            actionCount += (itemCount - 8);
            itemCount = 8;
        }
        
        //실제 카운트는 따로 올려주고 연출 진행.
        ManagerBlock.instance.worldRankingItemCount += itemCount;
        collectItemCountAction = StartCoroutine(CoCollectAction_WorldRanking(actionCount, itemCount));
    }

    private IEnumerator CoCollectAction_WorldRanking(int actionCount, int itemCnt)
    {
        for (int i = 0; i < itemCnt; i++)
        {
            actionCount++;
            materialEventLabel.text = actionCount.ToString();
            StartCoroutine(CoChangeCount(collectItemSprite.transform));

            //자이로 오브젝트 생성(최대 카운트 제한), 일정 수 이상되면 사라지도록 해줌.
            IngameGyroObj gyroObj = InGameEffectMaker.instance.MakeFlyWorldRankGyroItem();
            if (ManagerBlock.instance.worldRankingItemCount <= 30)
            {
                if (gyroObj != null)
                    listGyroObj.Add(gyroObj);
            }
            else
            {
                if (gyroObj != null)
                    StartCoroutine(gyroObj.CoAutoDestroy(0.5f));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    //에디터에서 사용
    public void SetWorldRanking_Editor(bool isWorldRank)
    {
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemCountAction = null;
        }
        materialEventRoot.SetActive(isWorldRank);
        collectItemSprite.gameObject.SetActive(isWorldRank);
        materialEventLabel.text = "0";
        gyroRoot.SetActive(isWorldRank);
    }
    #endregion

    IEnumerator CoChangeCount(Transform transObj)
    {
        float tmpeWaitTimer = 0;

        while (tmpeWaitTimer < 1f)
        {
            tmpeWaitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = ManagerBlock.instance._curveBlockPopUp.Evaluate(tmpeWaitTimer);
            transObj.localScale = Vector3.one * ratio;

            yield return null;
        }
        transObj.localScale = Vector3.one;

        yield return null;
    }


    void SetChapterMission()
    {
/*
        if (Global.GameType != GameType.NORMAL) return;

        if (EditManager.instance != null) return;

        foreach (var item in ManagerData._instance._questGameData)
        {
            if(item.Value.level == GameManager.instance.currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                    {
                        CandySprite.gameObject.SetActive(true);
                        CandySprite.spriteName = "Mission_DUCK_1";
                        CandySprite.MakePixelPerfect();
                    }
                    return;
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                    {
                        CandySprite.gameObject.SetActive(true);
                        CandySprite.spriteName = "Mission_CANDY_1";
                        CandySprite.MakePixelPerfect();
                    }
                    return;
                }
            }
        }
*/
        CandySprite.gameObject.SetActive(false);
    }

    public void GetChapterMissionSprite()
    {
        /*
        foreach (var item in ManagerData._instance._questGameData)
        {
            if (item.Value.level == GameManager.instance.currentChapter() + 1)
            {
                if (item.Value.type == QuestType.chapter_Duck)
                {
                    CandySprite.gameObject.SetActive(true);
                    CandySprite.spriteName = "Mission_DUCK_2";
                    CandySprite.MakePixelPerfect();
                }
                else if (item.Value.type == QuestType.chapter_Candy)
                {
                    CandySprite.gameObject.SetActive(true);
                    CandySprite.spriteName = "Mission_CANDY_2";
                    CandySprite.MakePixelPerfect();
                }
            }
        }
        */
        //획득효과 추가
        TweenPosition tweenPos = CandySprite.gameObject.GetComponent<TweenPosition>();
        tweenPos.enabled = true;
        //StartCoroutine(CoShowGetDuck());
    }

    /*
    IEnumerator CoShowGetDuck()
    {
        waitTimer = 0;

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            CandySprite.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f));

            yield return null;
        }
    }
    */


    void SetDigMode()
    {
        if(ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.NORMAL)
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_apple";
            turnShadowSpirte.spriteName = "ingame_item_apple";

            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach(var label in turnLabelList)
            {
                label.text = "残り";
                label.fontSize = 25;
                label.MakePixelPerfect();
            }
        }
        else if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.DIG)
        {
            //if (ManagerBlock.instance.stageInfo.digType ==  2)            
                turnUi.SetActive(true);
            DigGauge_Root.SetActive(true);

            turnSpirte.spriteName = "ingame_item_apple";
            turnShadowSpirte.spriteName = "ingame_item_apple";
            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
            {
                label.text = "残り";
                label.fontSize = 25;
                label.MakePixelPerfect();
            }
        }
        else if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.LAVA)
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_lava";
            turnShadowSpirte.spriteName = "ingame_item_lava";
            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
            {
                label.text = "上昇数";
                label.fontSize = 22;
                label.MakePixelPerfect();
            }   
        }
/*
        if (Global.GameType == GameType.RANK)
        {
            //rankModeRoot.SetActive(true);
            rankScoreLabel[0].gameObject.SetActive(true);
        }
        else
        {
            rankScoreLabel[0].gameObject.SetActive(false);
        }
*/
    }

    void SetGameItem()
    {
        foreach(var tempItem in listGameItem)
        {
            Destroy(tempItem.gameObject);
        }
        listGameItem.Clear();

/*
        if (EditManager.instance == null)
        {
            bool bSale = ServerRepos.LoginCdn.InGameItemSale;

            if (Global.GameType == GameType.ADVENTURE)
            {
                MakeGameItem(GameItemType.ADVENTURE_RAINBOW_BOMB, ServerRepos.UserItem.AdventureItem(2), ServerRepos.LoginCdn.InGameItems[7], bSale);
                MakeGameItem(GameItemType.SKILL_POINT_CHARGE, ServerRepos.UserItem.AdventureItem(1), ServerRepos.LoginCdn.InGameItems[6], bSale);
                MakeGameItem(GameItemType.HEAL_ONE_ANIMAL, ServerRepos.UserItem.AdventureItem(0), ServerRepos.LoginCdn.InGameItems[5], bSale);
            }
            else
            {
                MakeGameItem(GameItemType.THREE_HAMMER, ServerRepos.UserItem.InGameItem(2), ServerRepos.LoginCdn.InGameItems[2], bSale);
                MakeGameItem(GameItemType.CROSS_LINE, ServerRepos.UserItem.InGameItem(1), ServerRepos.LoginCdn.InGameItems[1], bSale);
                MakeGameItem(GameItemType.HAMMER, ServerRepos.UserItem.InGameItem(0), ServerRepos.LoginCdn.InGameItems[0], bSale);
            } 
        }
        else
*/
        {
            if (Global.GameType == GameType.ADVENTURE)
            {
                MakeGameItem(GameItemType.ADVENTURE_RAINBOW_BOMB, 3, 3);
                MakeGameItem(GameItemType.SKILL_POINT_CHARGE, 3, 3);
                MakeGameItem(GameItemType.HEAL_ONE_ANIMAL, 3, 3);
            }
            else
            {
                MakeGameItem(GameItemType.RAINBOW_BOMB_HAMMER, 3, 3);
                MakeGameItem(GameItemType.THREE_HAMMER, 3, 3);
                MakeGameItem(GameItemType.CROSS_LINE, 3, 3);
                MakeGameItem(GameItemType.HAMMER, 3, 3);
            }
        }
    }

    //무료아이템카운트 갱신
    public void RefreshInGameItem()
    {
        foreach (var tempItem in listGameItem)
        {
            if (tempItem.type == GameItemType.HAMMER) tempItem.RefreshCount(3);
            if (tempItem.type == GameItemType.CROSS_LINE) tempItem.RefreshCount(3);
            if (tempItem.type == GameItemType.THREE_HAMMER) tempItem.RefreshCount(3);
            if (tempItem.type == GameItemType.RAINBOW_BOMB_HAMMER) tempItem.RefreshCount(3);

            /*
            if (tempItem.type == GameItemType.ADVENTURE_RAINBOW_BOMB) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(2));
            if (tempItem.type == GameItemType.SKILL_POINT_CHARGE) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(1));
            if (tempItem.type == GameItemType.HEAL_ONE_ANIMAL) tempItem.RefreshCount(ServerRepos.UserItem.AdventureItem(0));
            */
        }
    }

    void MakeGameItem(GameItemType tempType, int count, int jewelCount, bool bSale = false)
    {
        GameItem gameItem = NGUITools.AddChild(gameItemRoot, gameItemObj).GetComponent<GameItem>();
        gameItem.Init(tempType, count, jewelCount, bSale);
        
        gameItem._transform.localScale = Vector3.one * 0.9f;
        gameItem._transform.localPosition = GAME_ITEM_POS + Vector3.left *95* listGameItem.Count;      //100
        //gameItem._transform.localScale = Vector3.one * 0.9f;

        listGameItem.Add(gameItem);
    }
    
    void SetFlower()
    {
/*
        if (Global.GameInstance.FlowerOn() == false)
        {
            gaugeRoot.SetActive(false);
            return;
        }
*/
        gaugeRoot.SetActive(true);
        tempFlowerCount = 0;

        if (EditManager.instance == null)
        {
/*
            flowerClearState = 0;
            if (GameManager.instance.currentChapter() <= ServerRepos.UserChapters.Count - 1) 
                flowerClearState = ServerRepos.UserChapters[GameManager.instance.currentChapter()].clearState;
*/
        }
        else
        {
            flowerClearState = 0;
        }

        //꽃 1,2 위치 잡기
        foreach (var flower in flowerSpineObj)
        {
            flower.state.SetAnimation(0, "idle", true);
        }

        //꽃피우기 이벤트시        //이벤트 중이고 
        //flowerClearState가 0보다 크고
        IsRedFlowerGetEvent = false;
/*
        if (ServerContents.BlossomEvents != null)
        {
            var userEventData = ServerRepos.UserBlossomEvents.Find(x => x.idx == ServerContents.BlossomEvents.idx);

            if( userEventData != null)
            {
                int prevStageFlower = ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel;
                
                UIPopupBlossomEvent.rewardReceivedNotified = userEventData.state == 1;  // 시작할때 이미 상태가 1이었으면 이미 보상 다 받은 상태...
                if (prevStageFlower < userEventData.flowerType &&
                    Global.LeftTime(ServerContents.BlossomEvents.start_ts) < 0 &&
                    Global.LeftTime(ServerContents.BlossomEvents.end_ts) > 0 && 
                    UIPopupBlossomEvent.rewardReceivedNotified == false)
                {
                    RedFlowerGetCount = userEventData.prog;
                    RedFlowerTargetCount = userEventData.targetCount;

                    RedFlowerRoot.SetActive(true);
                    RedFlowerEcopi.spriteName = "ecopi_event_ingame1";
                    RedFlowerEcopi.MakePixelPerfect();

                    if (GameManager.gameMode == GameMode.DIG)
                    {
                        turnUi.transform.localPosition = new Vector3(-235, 360, 0);
                        DigGauge_Root.transform.localPosition = new Vector3(-130f, 438.5f, 0);
                        RedFlowerRoot.transform.localPosition = new Vector3(-44f, 437.8f, 0);
                    }

                    //에코피이미지선택표시
                    //ServerRepos.UserBlossomEvents[ServerContents.BlossomEvents.idx].flowerType

                    if (userEventData.flowerType == 5 && flowerClearState > 1)
                    {
                        IsRedFlowerGetEvent = true;
                        RedFlowerSprite.spriteName = "stage_icon_level_05";
                        RedFlowerSprite.MakePixelPerfect();
                        RedFlowerSprite.cachedTransform.localScale = Vector3.one * 0.77f;
                    }
                    else if (userEventData.flowerType == 4 && flowerClearState > 0)
                    {
                        IsBlueFlowerGetEvent = true;
                        RedFlowerSprite.spriteName = "stage_icon_level_04";
                        RedFlowerSprite.MakePixelPerfect();
                        RedFlowerSprite.cachedTransform.localScale = Vector3.one * 0.77f;
                    }
                    else if (userEventData.flowerType == 3)
                    {
                        IsWhiteFlowwerGetEvent = true;
                        RedFlowerSprite.spriteName = "stage_icon_level_03";
                        RedFlowerSprite.MakePixelPerfect();
                        RedFlowerSprite.cachedTransform.localScale = Vector3.one * 0.77f;
                    }
                    RedFlowerSprite.gameObject.SetActive(false);
                }
            }
        }
*/
        //꽃점수체크
        if (IsRedFlowerGetEvent && flowerClearState > 1) //빨간꽃모으기일때
        {
            float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / (((float)ManagerBlock.instance.stageInfo.score4) * 1.1f);
            float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / (((float)ManagerBlock.instance.stageInfo.score4) * 1.1f);
            float ratio3 = (float)ManagerBlock.instance.stageInfo.score3 / (((float)ManagerBlock.instance.stageInfo.score4) * 1.1f);
            float ratio4 = (float)ManagerBlock.instance.stageInfo.score4 / (((float)ManagerBlock.instance.stageInfo.score4) * 1.1f);
            
            flowerSpineObj[0].transform.localPosition = new Vector3(235 * ratio1, 10, 0);
            flowerSpineObj[1].transform.localPosition = new Vector3(235 * ratio2, 10, 0);
            flowerSpineObj[2].transform.localPosition = new Vector3(235 * ratio3, 10, 0);
            flowerSpineObj[3].transform.localPosition = new Vector3(235 * ratio4, 10, 0);
            flowerSpineObj[4].transform.localPosition = new Vector3(235, 10, 5);

            flowerGauge[0].transform.localPosition = new Vector3(235 * ratio1, -2, 0);
            flowerGauge[1].transform.localPosition = new Vector3(235 * ratio2, -2, 0);
            flowerGauge[2].transform.localPosition = new Vector3(235 * ratio3, -2, 0);
            flowerGauge[3].transform.localPosition = new Vector3(235 * ratio4, -2, 0);
        }        
        else if (flowerClearState >0)
        {
            flowerSpineObj[4].gameObject.SetActive(false);
            flowerGauge[3].gameObject.SetActive(false);

            float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / (float)ManagerBlock.instance.stageInfo.score4;
            float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / (float)ManagerBlock.instance.stageInfo.score4;
            float ratio3 = (float)ManagerBlock.instance.stageInfo.score3 / (float)ManagerBlock.instance.stageInfo.score4;

            flowerSpineObj[0].transform.localPosition = new Vector3(235 * ratio1, 10, 0);
            flowerSpineObj[1].transform.localPosition = new Vector3(235 * ratio2, 10, 0);
            flowerSpineObj[2].transform.localPosition = new Vector3(235 * ratio3, 10, 0);
            flowerSpineObj[3].transform.localPosition = new Vector3(235, 10, 5);

            flowerGauge[0].transform.localPosition = new Vector3(235 * ratio1, -2, 0);
            flowerGauge[1].transform.localPosition = new Vector3(235 * ratio2, -2, 0);
            flowerGauge[2].transform.localPosition = new Vector3(235 * ratio3, -2, 0);
        }
        else
        {
            flowerSpineObj[4].gameObject.SetActive(false);
            flowerGauge[3].gameObject.SetActive(false);

            flowerSpineObj[3].gameObject.SetActive(false);
            flowerGauge[2].gameObject.SetActive(false);

            float ratio1 = (float)ManagerBlock.instance.stageInfo.score1 / (float)ManagerBlock.instance.stageInfo.score3;
            float ratio2 = (float)ManagerBlock.instance.stageInfo.score2 / (float)ManagerBlock.instance.stageInfo.score3;

            flowerSpineObj[0].transform.localPosition = new Vector3(235 * ratio1, 10, 0);
            flowerSpineObj[1].transform.localPosition = new Vector3(235 * ratio2, 10, 0);
            flowerSpineObj[2].transform.localPosition = new Vector3(235, 10, 0);

            flowerGauge[0].transform.localPosition = new Vector3(235 * ratio1, -2, 0);
            flowerGauge[1].transform.localPosition = new Vector3(235 * ratio2, -2, 0);
        }        
    }

    void SetUI()
    {
        if (listGameTarget.Count > 0)
        {
            foreach (GameMissionTarget item in listGameTarget)
            {
                Destroy(item.gameObject);
            }
            listGameTarget.Clear();
        }

        if (listUiObj.Count > 0)
        {
            foreach (GameObject item in listUiObj)
            {
                Destroy(item.gameObject);
            }
            listUiObj.Clear();
        }

        //목표 UI 설정
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;

            if (enumerator.Current.Value != null)
            {
                string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                    string.Format("StageTarget_{0}", targetType) : "StageTarget";

                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;

                    GameMissionTarget target = NGUITools.AddChild(Target_Root, targetObj).GetComponent<GameMissionTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    string collectCount = e.Current.Value.collectCount.ToString();
                    target.targetCount.text = collectCount;
                    target.targetCountShadow.text = collectCount;

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;

                    //이미지 크기 및 체크 설정
                    target.targetSprite.MakePixelPerfect();
                    target.targetSprite.cachedTransform.localScale = Vector3.one * 0.75f;

                    listGameTarget.Add(target);
                    listUiObj.Add(target.gameObject);
                }
            }
        }

        moveCountLabel.text = GameManager.instance.moveCount.ToString();
        moveCountLabelShadow.text = GameManager.instance.moveCount.ToString();

        float startPos = (1 - listGameTarget.Count) * 48;
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            listGameTarget[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }

        targetBG.width = 302;
        GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(152, 431, 0);

        if (listGameTarget.Count == 3) targetBG.width = 340;
        else if (listGameTarget.Count == 4) targetBG.width = 440;
        else if (listGameTarget.Count == 5)
        {
            targetBG.width = 530;
            GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(130, 431, 0);
        }
        else if (listGameTarget.Count > 5)
        {
            startPos = (1 - listGameTarget.Count) * 40;
            for (int i = 0; i < listGameTarget.Count; i++)
            {
                listGameTarget[i].transform.localPosition = new Vector3(startPos + 80 * i, 0, 0);
            }
            targetBG.width = 540;
            GameUIManager.instance.Target_Root.transform.localPosition = new Vector3(130, 431, 0);
        }

        InitTargetUICount();
    }

    //목표 UI 카운트 초기화.
    private void InitTargetUICount()
    {
        listTargetUICount.Clear();
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            listTargetUICount.Add(0);
        }
    }

    //목표 UI 카운트 변경시킬 때 사용하는 함수.
    public void RefreshTarget(TARGET_TYPE targetType, BlockColorType targetColor = BlockColorType.NONE, int addCount = 1)
    {
        /*if (Global.GameInstance.IsStageTargetHidden())
            return;

        if (GameManager.gameMode == GameMode.ADVENTURE || GameManager.gameMode == GameMode.COIN)
            return;*/

        ManagerBlock.CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(targetType, targetColor);
        if (collectTargetCount != null)
        {
            int findIndex = listGameTarget.FindIndex(x => x.targetType == targetType && x.targetColor == targetColor);
            if (findIndex == -1)
                return;

            listTargetUICount[findIndex] += addCount;
            int count = collectTargetCount.collectCount - listTargetUICount[findIndex];
            listGameTarget[findIndex].ShowChangeCount(count);
            showCount(findIndex, count);
        }
    }

    void showCount(int tempIndex, int tempCount)
    {
        listGameTarget[tempIndex].targetCount.text = tempCount.ToString();
        listGameTarget[tempIndex].targetCountShadow.text = tempCount.ToString();

        if (tempCount <= 0)
        {
            listGameTarget[tempIndex].targetCount.gameObject.SetActive(false);
            listGameTarget[tempIndex].targetCountShadow.gameObject.SetActive(false);
            listGameTarget[tempIndex].checkSprtie.gameObject.SetActive(true);
        }
    }

    public void RefreshTargetAll()
    {
        for (int i = 0; i < listGameTarget.Count; i++)
        {
            TARGET_TYPE targetType = listGameTarget[i].targetType;
            BlockColorType targetColor = listGameTarget[i].targetColor;

            ManagerBlock.CollectTargetCount collectTargetCount = ManagerBlock.instance.GetCollectTargetCountData(targetType, targetColor);
            if (collectTargetCount != null)
            {
                int count = (collectTargetCount.collectCount > collectTargetCount.pangCount) ?
                    (collectTargetCount.collectCount - collectTargetCount.pangCount) : 0;
                showCount(i, count);
            }
        }
    }

    public void RefreshMove()
    {
        ShowEffectAddApple();

        moveCountLabel.text = GameManager.instance.moveCount.ToString();
        moveCountLabelShadow.text = GameManager.instance.moveCount.ToString();

        if (IsWarningsRemainingTurns() == true)
            StartCoroutine(CoWarningsRemainingTurnsAction());
    }

    public void SetIceCount()
    {
        //turnIceSpirte.gameObject.SetActive(true);
        //turnIceSpirte.spriteName = "DecoIce2";
        InGameEffectMaker.instance.MakeIceShineEffect2(turnSpirte.gameObject.transform.position);
        InGameEffectMaker.instance.MakeIceShineEffect1(turnSpirte.gameObject.transform.position);

        GameManager.instance.moveCount = 0;
        RefreshMove();

        turnSpirte.spriteName = "ingame_item_lava2";
        turnSpirte.MakePixelPerfect();
        //turnShadowSpirte.spriteName = "ingame_item_lava";
    }

    public void DisCountIce(int iceCount)
    {
        if(iceCount > 1)
        {
            turnSpirte.spriteName = "ingame_item_lava2";
            turnSpirte.MakePixelPerfect();                
        }
        else if(iceCount == 0)
        {
            turnSpirte.spriteName = "ingame_item_lava";
            turnSpirte.MakePixelPerfect();
        }
        else
        {
            turnSpirte.spriteName = "ingame_item_lava1";
            turnSpirte.MakePixelPerfect();
        }

        //이펙트
        if (iceCount < 2) InGameEffectMaker.instance.MakeICeEffect(turnSpirte.gameObject.transform.position);
    }


    #region 턴 증가 연출 관련
    //말풍선과 함께 턴 증가 연출이 뜨는 연출
    public IEnumerator CoActionAddTurn_WithMakeBubble(int addTurnCount)
    {
        //말풍선 설정
        moveCountAddTurnBubble.SetActive(true);
        moveCountAddTurnLabel.text = string.Format("+{0}", addTurnCount);

        //빛 설정
        turnLightSprite.gameObject.SetActive(true);
        turnLightSprite.transform.localScale = Vector3.one * 0.1f;

        //턴 이미지 설정
        turnSpirte.transform.localScale = Vector3.one * 0.5f;

        float actionTime = ManagerBlock.instance.GetIngameTime(0.4f);

        //말풍선 커지는 연출
        moveCountAddTurnBubble.transform.localScale = Vector3.one * 0.5f;
        moveCountAddTurnBubble.transform.DOScale(1f, actionTime).SetEase(Ease.OutBack);

        //빛 회전 연출
        turnLightSprite.transform.DORotate(new Vector3(0f, 0f, 200f), actionTime * 2f);
        turnLightSprite.transform.DOScale(1f, actionTime);

        //턴 이미지 커지는 연출
        turnSpirte.transform.DOScale(1.2f, actionTime).SetEase(Ease.OutBack);

        //연출 종료까지 대기
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.2f);

        //빛 이미지 사라지는 연출
        turnLightSprite.transform.DOScale(0.1f, actionTime);

        //턴 이미지 원래 사이즈로 돌아가는 연출
        turnSpirte.transform.DOScale(1.0f, actionTime).SetEase(Ease.OutBack);

        //연출 종료까지 대기
        yield return new WaitForSeconds(actionTime);

        turnLightSprite.gameObject.SetActive(false);
        turnSpirte.transform.localScale = Vector3.one;

        //증가할 턴 카운트 컬러 변경
        Color originColor = moveCountLabel.color;
        moveCountLabel.color = new Color32(0xff, 0xe9, 0x32, 0xff);

        //턴 카운트 증가 연출
        yield return CpActionAddTurn(addTurnCount);

        //턴 카운트 설정
        GameManager.instance.moveCount += addTurnCount;

        string turnText = GameManager.instance.moveCount.ToString();
        moveCountLabel.text = turnText;
        moveCountLabelShadow.text = turnText;
        moveCountLabel.color = originColor;

        //말풍선 사라지는 연출
        actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        moveCountAddTurnBubble.transform.DOScale(0.1f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
        moveCountAddTurnBubble.SetActive(false);
    }

    //턴 숫자 증가 연출
    private IEnumerator CpActionAddTurn(int addTurn)
    {
        UILabel[] labels = new UILabel[] { moveCountLabel, moveCountLabelShadow };
        int startCount = GameManager.instance.moveCount;
        yield return CpActionAddTurn(labels, startCount, startCount + addTurn);
    }

    //숫자 증가 연출
    private IEnumerator CpActionAddTurn(UILabel[] labetText, int startCount, int totalCount, int maxActionCount = 5)
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.1f);

        //더할 량
        int addTurn = totalCount - startCount;

        //연출이 등장할 횟수
        int actionCnt = (addTurn > maxActionCount) ? maxActionCount : addTurn;

        //연출 한 번당 증가할 카운트
        int count = (addTurn > maxActionCount) ? (addTurn / maxActionCount) : 1;

        //턴 카운트 증가 연출
        for (int i = 0; i < actionCnt; i++)
        {
            int addCount = count;

            //맨 마지막 카운트는 나머지까지 전부 합산한 카운트.
            if (addTurn > maxActionCount && (i + 1) >= actionCnt)
                addCount += addTurn % maxActionCount;

            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
            startCount += addCount;
            string turnText = startCount.ToString();
            foreach (var item in labetText)
                item.text = turnText;

            labetText[0].transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);
            yield return new WaitForSeconds(actionTime);
        }
    }

    public void ShowEffectAddApple()
    {
        StartCoroutine(CoAddApple());
    }

    float waitTimer = 0f;
    const int CURVE_SPPED = 3;

    IEnumerator CoAddApple()
    {
        waitTimer = 0;

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            moveCountLabel.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f) * 0.8f);
            yield return null;
        }
    }

    IEnumerator CoAddAppleColor(float addTime = 0f)
    {
        moveCountLabel.color = new Color32(0xff, 0xe9, 0x32, 0xff);

        waitTimer = 0;

        while (waitTimer < (1f + addTime))
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = _curveAddApple.Evaluate(waitTimer);
            moveCountLabel.transform.localScale = Vector3.one * ((1 + (ratio - 1) * 0.5f) * 0.8f);

            yield return null;
        }

        moveCountLabel.color = new Color32(0xff, 0xff, 0xff, 0xff);
    }
    #endregion

    void OnClickBtnPause()
    {        
        if (SceneManager.GetActiveScene().name == "InGameTool") return;    
        if (GameItemManager.instance != null) return;
        if (GameManager.instance.state == GameState.GAMECLEAR || GameManager.instance.state == GameState.GAMEOVER) return;

        if (Input.touchCount > 0) return;


        if (Global.GameType == GameType.ADVENTURE)
        {
//            ManagerUI._instance.OpenPopupAdventurePause(AdventureManager.instance.waveCount.ToString(), null); 
        }
        else
            ManagerUI._instance.OpenPopupPause();


    }

    public void ShowFlower(bool enable)
    {
        //flowerGaugeRoot.SetActive(enable);
    }

    public void ShowPauseButton(bool isShow)
    {
        pauseButton.SetActive(isShow);
    }

    bool showing5Left = false;

    public void ShowLeft5Turn()
    {
        if (EditManager.instance != null) return;

        if (showing5Left) return;

        showing5Left = true;

        if (birdLive2D == null)
        {
            birdLive2D = NGUITools.AddChild(ManagerUI._instance.anchorCenter, ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.BlueBird].obj).GetComponent<LAppModelProxy>();
        }

        birdLive2D.gameObject.SetActive(true);

        ManagerSound.AudioPlay(AudioInGame.TURN5_LEFT);
        birdLive2D.SetVectorScale(new Vector3(-1f, 1f, 1f) * 200f);
        birdLive2D.transform.localPosition = new Vector3(-358f, 415f, 0f);
        birdLive2D._CubismRender.SortingOrder = 10;
        birdLive2D._CubismRender.Opacity = 1;
        birdLive2D.setAnimation(false, "count");    //모션.

        StartCoroutine(CoShowLeft5Turn());
    }


    public static LAppModelProxy birdLive2D = null;
    public List<GameObject> Liststatue = new List<GameObject>();//static



    IEnumerator CoShowLeft5Turn()
    {       
        float waitTimer = 0;
        while (waitTimer < 2.3f)
        {
            //if (birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("count") && birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) break;
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
         
        birdLive2D.gameObject.SetActive(false);
        showing5Left = false;
        yield return null;
    }

    public void SetDigGauge(float ratio)
    {
        float ratioY = Mathf.Lerp(digGradStart, digGradEnd, ratio);
        digGradation.transform.localPosition = new Vector3(13f, ratioY, 0);
    }

    public void ShakingCamera()
    {
        StartCoroutine(CoShakeCamera());
    }

    IEnumerator CoShakeCamera()
    {
        float factor = 1f;
        Vector3 value = Vector3.zero;

        while (factor > 0)
        {
            factor -= Global.deltaTimePuzzle;
            if (factor <= 1f)
            {
                float result = Mathf.Sin(2.0f * 3.14159f * factor * 7f) * 6f;
                value.x = result * (factor) / 1f;
                float result1 = Mathf.Sin(2.0f * 3.14159f * factor * 6f + 0.4f) * 5f;
                value.y = result1 * (factor) / 1f;
            }
            InGameCamera.transform.localPosition = value;
            yield return null;
        }        

        InGameCamera.transform.localPosition = Vector3.zero;
        yield return null;
    }

    void GetRankState()
    {
/*
        myScoreObj.transform.localPosition = new Vector3(-300, 0, 0);

#if UNITY_EDITOR         
        rankModeRoot.SetActive(true);
        rankMaxScore = 60000;
        
        UIRankInGameItem tempItem1 = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
        tempItem1.InitItem(9, false, null, 10000, false);
        listRankItem.Add(tempItem1);

        UIRankInGameItem tempItemBest1 = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
        tempItemBest1.InitItem(0, false, null, 3000, true);
        listRankItem.Add(tempItemBest1);
        return;
#endif

        if (ManagerEventRanking.GetMyRank() == null)    //처음진입시
        {
            rankModeRoot.SetActive(false);
            return;
        }
        else
        {
            rankModeRoot.SetActive(true);
        }

        int rankListCount = ManagerEventRanking.GetRankDataCount();
        int scoreGap = 0;
        ManagerEventRanking.EventRankData tempRankData = null;

        for (int i = 0; i < rankListCount; i++)
        {
            if (ManagerEventRanking.GetRankData(i).scoreValue > rankMaxScore)
            {
                rankMaxScore = (int)(ManagerEventRanking.GetRankData(i).scoreValue * 1.5f);
            }
        }

        //베스트랭킹
        UIRankInGameItem tempItemBest = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
        tempItemBest.InitItem(0, false, null, ManagerEventRanking.GetMyRank().scoreValue, true);
        listRankItem.Add(tempItemBest);


        //내 베스트 점수보다 높은 랭크 찾기
        for (int i = 0; i < rankListCount; i++)
        {
            if (ManagerEventRanking.GetRankData(i).scoreValue > ManagerEventRanking.GetMyRank().scoreValue)
            {
                if (scoreGap == 0 || scoreGap > (int)ManagerEventRanking.GetRankData(i).scoreValue - ManagerEventRanking.GetMyRank().scoreValue)
                {
                    scoreGap = (int)ManagerEventRanking.GetRankData(i).scoreValue - (int)ManagerEventRanking.GetMyRank().scoreValue;
                    tempRankData = ManagerEventRanking.GetRankData(i);
                }
            }
        }

        if (rankListCount != 0 && tempRankData != null)
        {
            UIRankInGameItem tempItem = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
            tempItem.InitItem(tempRankData.rank, tempRankData.photoUseAgreed, tempRankData.pictureURL, tempRankData.scoreValue, false);
            listRankItem.Add(tempItem);
            return;
        }
   
        //내가 랭킹이 1위일경우        
        isFirstRank = true;
        rank1stObj.SetActive(true);
*/
    }


    public void AddRanking() 
    {
/*
        if (isFirstRank)
            return;

        int tempScore = ManagerBlock.instance.score;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            tempScore = (int)(tempScore * (1.1f + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
        }
        else
        {
            tempScore = (int)(tempScore * (1 + ((float)ManagerBlock.instance.StageRankBonusRatio) * 0.01f));
        }

#if UNITY_EDITOR
        if (tempScore > 40000)
        {
            isFirstRank = true;
            rank1stObj.SetActive(true);
        }
        else
        {
            UIRankInGameItem tempItem1 = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
            tempItem1.InitItem(8, false, null, tempScore + 10000, false);
            listRankItem.Add(tempItem1);
            //새위치는?
        }
        return;
#endif

        int rankListCount = ManagerEventRanking.GetRankDataCount();
        ManagerEventRanking.EventRankData tempRankData = null;
        int scoreGap = 0;

        //내점수가 내 베스트보다 크다면
        for (int i = 0; i < rankListCount; i++)
        {
            if (ManagerEventRanking.GetRankData(i).scoreValue > tempScore)
            {
                if (scoreGap == 0 || scoreGap > (int)ManagerEventRanking.GetRankData(i).scoreValue - tempScore)
                {
                    scoreGap = (int)ManagerEventRanking.GetRankData(i).scoreValue - tempScore;
                    tempRankData = ManagerEventRanking.GetRankData(i);
                }
            }
        }

        if (rankListCount != 0 && tempRankData != null)
        {
            UIRankInGameItem tempItem = NGUITools.AddChild(rankModeRoot, rankItemObj).GetComponent<UIRankInGameItem>();
            tempItem.InitItem(tempRankData.rank, tempRankData.photoUseAgreed, tempRankData.pictureURL, tempRankData.scoreValue, false);
            listRankItem.Add(tempItem);
        }
        else if (rankListCount > 0 && tempRankData == null)
        {
            isFirstRank = true;
            rank1stObj.SetActive(true);
        }
*/
    }


    //스테이지랭킹 
    void SetStageRanking()  //룰렛설정 //베스트스코어설정
    {
/*
        if(GameManager.IsStageRank) //(ManagerStageRanking.NowOnRace())
        {
            stageRankRoot.SetActive(true);
            stageRankTurnCount = 5;

            int rankingStageIdx = ManagerStageRanking.GetEventInfo().CheckRankingStage(Global.stageIndex);
            if (rankingStageIdx != -1)
            {
                if (ManagerStageRanking.IsParticipated())
                {
                    var stageRankData = ServerRepos.StageRanks.Find(x => { return x.eventIdx == ServerContents.StageRank.eventIndex; });
                    if (stageRankData.scores != null)
                    {
                        stageRankScoreRoot.SetActive(true);
                        foreach (var temp in stageRankScoreLabel) temp.text = string.Format("{0:n0}", stageRankData.scores[rankingStageIdx]);

                        if (stageRankData.scores[rankingStageIdx] == 0)
                        {
                            stageRankScoreRoot.SetActive(false);
                        }
                    }
                    else
                    {
                        foreach (var temp in stageRankScoreLabel) temp.text = "0";
                        stageRankScoreRoot.SetActive(false);
                    }
                }
                else
                {
                    stageRankScoreRoot.SetActive(false);
                }
            }     
        }
*/
    }


    public void GetStageRankRoutte()
    {

        //if (ManagerStageRanking.NowOnRace() == false)return;
        if (GameManager.IsStageRank == false) return;

        if (stageRankTurnCount <= 0)
            return;
        
        stageRankTurnCount--;
        stageRankTurnCountLabel.text = stageRankTurnCount + "ターン後";
//        StartCoroutine(DoBubbleAnim(stageRankRoot)); 

        if (stageRankTurnCount == 0)
        {
            StartCoroutine(DoShowStageRankingRoutte());
        }
    }

    //룰렛돌아가는 
    IEnumerator DoShowStageRankingRoutte()
    {
/*
        float waitTimer = 0f;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }
        while (ManagerBlock.instance.state == BlockManagrState.MOVE)
        {
            yield return null;
        }

        //확률설정
        int RatioRandom = Random.Range(0, 1000);
        int materialRatio = 10;
        if (RatioRandom > 800)
            materialRatio = 50;
        else if (RatioRandom > 300)
            materialRatio = 20;

        //룰렛돌기
        int[] ratioList = new int[] { 10, 20, 50 };
        waitTimer = 0f;

        stageRankTurnCountLabel.gameObject.SetActive(false);
        stageRankBonusLabel[0].gameObject.transform.localPosition = new Vector3(-3.5f, -3.7f, 0);

        //사운드재생 SPECIAL_EVENT_ROUTTE
        ManagerSound.AudioPlay(AudioInGame.SPECIAL_EVENT_ROUTTE);

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle;

            int count = (int)(waitTimer * 5f);
            count = count % ratioList.Length;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 10);

            foreach (var temp in stageRankBonusLabel)
            {
                temp.text = "+" + ratioList[count].ToString() + "%";
                temp.gameObject.transform.localScale = Vector3.one * (0.8f + 0.3f * scaleRatio);
            }

            stageRankRoot.transform.localScale = Vector3.one * (1f + 0.05f * scaleRatio);

            foreach (var temp in stageRankBubbleBefore)
                temp.alpha = 1 - waitTimer * 4;

            foreach (var temp in stageRankBubbleAfter)
            {
                if (waitTimer * 4 > 1f)
                    temp.color = Color.white;
                else
                    temp.color = new Color(1, 1, 1, waitTimer * 4);
            }

            yield return null;
        }

        waitTimer = 0f;
        while (waitTimer < 0.1f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        InGameEffectMaker.instance.MakeDuckEffect(stageRankBonusLabel[0].transform.position);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        ManagerBlock.instance.StageRankBonusRatio = materialRatio;

        waitTimer = 0f;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 2f;

            float scaleRatio = Mathf.Sin(Mathf.PI * waitTimer * 5) * (1 - waitTimer);//ManagerBlock.instance._curveBlockJump.Evaluate(waitTimer);

            foreach (var temp in stageRankBonusLabel)
            {
                temp.text = "+" + materialRatio.ToString() + "%";
                temp.gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 1f);
                temp.alpha = waitTimer * 2f;
            }

            stageRankBubbleAfter[0].gameObject.transform.localScale = Vector3.one * (1 + scaleRatio * 0.5f);

            yield return null;
        }

        foreach (var temp in stageRankBonusLabel)
            temp.text = "+" + materialRatio.ToString() + "%";

        float scoreRatio = 1;
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
            scoreRatio = 1.1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;
        else
            scoreRatio = 1f + ManagerBlock.instance.StageRankBonusRatio * 0.01f;

        GameUIManager.instance.SetScore((int)(ManagerBlock.instance.score * scoreRatio));   //변경된 점수 추가        
        scoreLabel.color = Color.yellow;

        waitTimer = 0f;
        while (true)
        {
            if (stageRankRoot == null)
                break;

            waitTimer += Global.deltaTimePuzzle;
            float ratio = Mathf.Sin(Mathf.PI * waitTimer);
            stageRankRoot.transform.localScale = new Vector3(1 + 0.08f * ratio, 1 - 0.08f * ratio, 0);
            yield return null;
        }
        
*/
        yield return null;
    }

    public void SetAdventureGaige(float tempCount)
    {
        advantureGaigeSlider.value = tempCount;
        if (tempCount >= 1f && isChargeAdvantureGaige == false)
        {
            isChargeAdvantureGaige = true;
            StartCoroutine(DoAdvantureGaige());
        }
        else if (tempCount < 1f)
        {
            isChargeAdvantureGaige = false;
        }
    }

    IEnumerator DoAdvantureGaige()
    {
        //반짝반짝거리기
        while (isChargeAdvantureGaige)
        {
            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            advantureGaigeBomb.color = new Color(0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 1); //mainSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);            
            advantureGaigeBomb.cachedTransform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);
            yield return null;
        }
        advantureGaigeBomb.color = Color.white;
        advantureGaigeBomb.cachedTransform.localScale = Vector3.one;
        yield return null;
    }

    public void SetAdventureEffectBG(bool enable)
    {
        adventureEffectBG.gameObject.SetActive(enable);
    }
    #region 코인스테이지

    [SerializeField]
    List<GameObject> CoinStageUI = new List<GameObject>();  //코인스테이지용 UI

    [SerializeField]
    List<GameObject> NonCoinStageUI = new List<GameObject>();  //일반스테이지용 UI
  
    [SerializeField]
    List<GameObject> CoinStageFeverUI = new List<GameObject>();     //피버시 나타나는UI

    [SerializeField]
    List<GameObject> CoinStageFeverHideUI = new List<GameObject>(); //피버일때 숨기는 UI

    [SerializeField]
    List<UISprite> FeverColorChangeUI = new List<UISprite>(); //피버일때 숨기는 UI

    [SerializeField]
    List<GameObject> FeverMovingObjList = new List<GameObject>();   //움직이는 동전오브젝트
    [SerializeField]
    AnimationCurve feverCoinCurve;
    float coinMoveHeight = 20f;
    float kuniDrawScale = 1260f;
    float kuniDistance = 120f;

    public UISprite feverBlockColor;

    [SerializeField]
    UISprite feverBlockGaige;

    [SerializeField]
    GameObject KuinObj;
    [SerializeField]
    GameObject KuinRoot;
    List<GameObject> KuniList = new List<GameObject>();

    //미러볼
    [SerializeField]
    List<GameObject> mirrorBallRootList = new List<GameObject>();  //미러볼
    [SerializeField]
    List<UISprite> mirrorBallList = new List<UISprite>();  //미러볼
    [SerializeField]
    List<UISprite> mirrorBallEffectList = new List<UISprite>();  //미러볼이펙트

    void SetCoinStage()
    {
        if (GameManager.gameMode != GameMode.COIN)
        {
            foreach (var nonObj in NonCoinStageUI)
                nonObj.SetActive(true);

            foreach (var coinObj in CoinStageUI)
                coinObj.SetActive(false);   
            return;
        }
        
        //턴라벨수정
        {
            turnUi.SetActive(true);
            DigGauge_Root.SetActive(false);

            turnSpirte.spriteName = "ingame_item_clock";
            turnShadowSpirte.spriteName = "ingame_item_clock";

            turnSpirte.MakePixelPerfect();
            turnShadowSpirte.MakePixelPerfect();

            foreach (var label in turnLabelList)
                label.enabled = false;

            moveCountLabel.transform.localPosition = new Vector3(-9.6f, 7f, 0);
            turnSpirte.transform.localPosition = new Vector3(-50f, 122f, 0);
            
            //턴, 목표 위치 옮기기
            Target_Root.transform.localPosition = new Vector3(228, 431, 0); //0.8
            Target_Root.transform.localScale = Vector3.one * 0.8f;
            turnUi.transform.localPosition = new Vector3(-246, 360, 0); //0.83
            //turnUi.transform.localScale = Vector3.one * 0.83f;
        }        

        //점수, 아이템
        foreach (var nonObj in NonCoinStageUI)
            nonObj.SetActive(false);

        //피버게이지, 코인배경
        foreach (var coinObj in CoinStageUI)
            coinObj.SetActive(true);

        //피버타임 배경
        foreach (var feverObj in CoinStageFeverUI)
            feverObj.SetActive(false);

        feverBlockGaige.fillAmount = 0f;


        int kuniRatio = (int)(kuniDrawScale / kuniDistance);
        kuniDrawScale = (float)(kuniRatio * kuniDistance);

        foreach(var tmp in KuniList)
            Destroy(tmp);
        KuniList = new List<GameObject>();
        for (int i = 0; i < kuniRatio; i++)
            for (int j = 0; j < kuniRatio; j++)
            {
                UISprite kuni = NGUITools.AddChild(KuinRoot, KuinObj).GetComponent<UISprite>();
                kuni.depth = -3;

                if(j % 2 == 0)
                    kuni.gameObject.transform.localPosition = new Vector3(-kuniDistance * i, -kuniDistance * j, 0);
                else
                    kuni.gameObject.transform.localPosition = new Vector3(-kuniDistance * i + kuniDistance * 0.5f, -kuniDistance * j, 0);

                if ((i + j) % 2 == 0)
                {
                    kuni.spriteName = "bg_coin";
                    kuni.MakePixelPerfect();

                    TweenScale tween = kuni.gameObject.GetComponent<TweenScale>();
                    tween.from = Vector3.one * 1.2f;
                    tween.to = Vector3.one * 0.8f;
                }
                KuniList.Add(kuni.gameObject);
            }   
    }

    public void SetFeverMode(bool isOn)
    {
        if (!isOn)
        {
            foreach (var feverObj in CoinStageFeverUI)
                feverObj.SetActive(isOn);

            foreach (var feverObj in CoinStageFeverHideUI)
                feverObj.SetActive(!isOn);

            foreach (var tmp in FeverMovingObjList)
                tmp.transform.localPosition = Vector3.zero;            
        }
        else
        {
            if (mirrorBallOn == false)
            {
                mirrorBallOn = true;
                StartCoroutine(ShowFeverColorChagne());
            }
        }
    }

    bool mirrorBallOn = false;
    IEnumerator ShowFeverColorChagne()
    {
        float timer = 0f;
        foreach (var tmp in mirrorBallRootList)
            tmp.SetActive(true);

        StartCoroutine(CoMirrorballDown());
        yield return new WaitForSeconds(0.1f);

        foreach (var feverObj in CoinStageFeverUI)
            feverObj.SetActive(true);

        foreach (var feverObj in CoinStageFeverHideUI)
            feverObj.SetActive(true);

        while (ManagerBlock.instance.isFeverTime())
        {
            timer += Global.deltaTimePuzzle;
            float color = ((Mathf.Sin(timer * Mathf.PI * 6) + 1) * 0.5f) *0.25f + 0.75f;

            foreach (var tmp in FeverColorChangeUI)
                tmp.color = new Color(color, color, color, 1f);

            yield return null;
        }

        timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 4;
            float posY = Mathf.Lerp(740f, 1100f, timer);
            foreach (var tmp in mirrorBallRootList)
                tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x, posY);
            yield return null;
        }

        foreach (var tmp in mirrorBallRootList)
            tmp.SetActive(false);

        mirrorBallOn = false;
        yield return null;
    }

    private IEnumerator CoMirrorballDown()
    {
        yield return new WaitForSeconds(0.3f);
        float targetPosY = 670f;
        for (int i = 0; i < mirrorBallRootList.Count; i++)
        {
            mirrorBallRootList[i].transform.DOLocalMoveY(targetPosY, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void SetFeverGaige(float ratio, float color = 1f)
    {
        feverBlockGaige.fillAmount = ratio;
        feverBlockGaige.color = new Color(color, color, color, color);

        if (ManagerBlock.instance.isFeverTime())
        {
            foreach (var tmp in FeverMovingObjList)
            {
                float timeRatio = (ManagerBlock.instance.BlockTime * 2) % 1;
                float MoveRatio = feverCoinCurve.Evaluate(timeRatio);
                tmp.transform.localPosition = Vector3.up * coinMoveHeight * MoveRatio;
            }

            foreach(var tmp in KuniList)
            {
                tmp.transform.localPosition += (Vector3.down + Vector3.left)*2;

                if (tmp.transform.localPosition.x <= -kuniDrawScale)
                    tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x + kuniDrawScale, tmp.transform.localPosition.y);
                if (tmp.transform.localPosition.y <= -kuniDrawScale)
                    tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x, tmp.transform.localPosition.y + kuniDrawScale);               
            }

            int MirrorTimeRatio = (int)((ManagerBlock.instance.BlockTime * 6) % 4 + 1);
            foreach(var tmp in mirrorBallList)            
                tmp.spriteName = "Mirror_ball0" + MirrorTimeRatio;

            int MirrorTimeRatioE = (int)((ManagerBlock.instance.BlockTime * 3) % 3 + 1);
            foreach(var tmp in mirrorBallEffectList)
                tmp.spriteName = "Mirror_ball_light0" + MirrorTimeRatioE;            
        }
    }

    private bool IsWarningsRemainingTurns()
    {
        if (GameManager.gameMode != GameMode.COIN)
            return false;
        if (GameManager.instance.moveCount > 10)
            return false;
        else
            return true;
    }

    private IEnumerator CoWarningsRemainingTurnsAction()
    {
        moveCountLabel.color = warningColor_turn;
        moveCountLabel.effectColor = warningEffectColor_turn;
        yield return new WaitForSeconds(0.25f);
        moveCountLabel.color = Color.white;
        moveCountLabel.effectColor = defaultEffectColor_turn;
    }
    #endregion
    
    #region 엔드 컨텐츠
    
    public void SetEndContents_Editor()
    {
        foreach (var item in ManagerBlock.boards)
        {
            if (item.Block != null && item.Block.type == BlockType.ENDCONTENTS_ITEM)
            {
                if (collectItemCountAction != null)
                {
                    StopCoroutine(collectItemCountAction);
                    collectItemCountAction = null;
                }
                materialEventRoot.SetActive(true);
                collectItemSprite.atlas = item.Block.mainSprite.atlas;
                collectItemSprite.spriteName = "endContents2";
                collectItemSprite.MakePixelPerfect();
                collectItemSprite.gameObject.SetActive(true);
                materialEventLabel.text = "0";
                break;
            }
        }
    }

    public void SetEndContents()
    {
        if (Global.GameType == GameType.END_CONTENTS)
        {
            if (collectItemCountAction != null)
            {
                StopCoroutine(collectItemCountAction);
                collectItemCountAction = null;
            }
            materialEventRoot.SetActive(true);
            /* if (ManagerEndContentsEvent.instance != null)
            {
                if (ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas != null)
                {
                    collectItemSprite.atlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas;
                    collectItemSprite.spriteName = "endContents2";
                }
            } */
            collectItemSprite.gameObject.SetActive(true);
            materialEventLabel.text = "0";
        }
    }
    
    public void CollectAction_EndContentsItem(int itemCount)
    {
        //이전에 진행되던 연출 있으면 멈추기.
        if (collectItemCountAction != null)
        {
            StopCoroutine(collectItemCountAction);
            collectItemSprite.MakePixelPerfect();
        }

        //재료 카운트 표시 마지막으로 획득한 카운트까지 갱신해줌.
        materialEventLabel.text = ManagerBlock.instance.endContentsItemCount.ToString();

        //연출에 필요한 카운트는 따로 저장.
        int actionCount = ManagerBlock.instance.endContentsItemCount;
        if (itemCount > 8)
        {
            actionCount += (itemCount - 8);
            itemCount = 8;
        } 
        
        //실제 카운트는 따로 올려주고 연출 진행.
        ManagerBlock.instance.endContentsItemCount += itemCount;
        collectItemCountAction = StartCoroutine(CoCollectAction_EndContents(actionCount, itemCount));
    }

    private IEnumerator CoCollectAction_EndContents(int actionCount, int itemCnt)
    {
        for (int i = 0; i < itemCnt; i++)
        {
            actionCount++;
            materialEventLabel.text = actionCount.ToString();
            StartCoroutine(CoChangeCount(collectItemSprite.transform));
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    #region 인게임 툴
    public void SetActiveHideObjectsAtMapSave(bool isActive)
    {
        foreach (var item in hideObjectsAtSaveMap)
        {
            item.SetActive(isActive);
        }
    }
    #endregion

    public void DestroyIngameItemUI()
    {
        foreach(var tempItem in listGameItem)
        {
            Destroy(tempItem.gameObject);
        }
        listGameItem.Clear();
    }
}
