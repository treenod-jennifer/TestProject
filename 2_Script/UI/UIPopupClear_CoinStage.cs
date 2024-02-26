using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;

public class UIPopupClear_CoinStage : UIPopupBase
{
    //결과 UI 루트
    [SerializeField] private UISprite sprResultUIRoot;

    //상단 UI
    [SerializeField] private GameObject bestCoinRoot;
    [SerializeField] private UILabel bestCoinTitle;
    [SerializeField] private UILabel labelBestCoin;

    //결과창 UI
    [SerializeField] private UILabel labelStage;
    [SerializeField] private UILabel[] labelTitle_DefaultCoin;
    [SerializeField] private UILabel[] labelTitle_feverCoin;
    [SerializeField] private UILabel[] labelTitle_allCoin;
    [SerializeField] private UILabel labelDefaultCoin;
    [SerializeField] private UILabel labelFeverCoin;
    [SerializeField] private UILabel labelAllCoin;

    //코인 배수 이벤트
    [SerializeField] private GameObject coinRatioEventRoot;
    [SerializeField] private UILabel coinEventLabel;

    //결과 스파인
    [SerializeField] private SkeletonAnimation spineFlower;

    //결과창 버튼
    [SerializeField] private GameObject objBtnClose;
    [SerializeField] private UILabel[] btnCloseText;

    //이펙트
    [SerializeField] private GameObject objEffectBlockCollect;
    [SerializeField] private GameObject objEffectBestScore;

    //라이브 2d
    [SerializeField] private GameObject live2DAnchor;
    private LAppModelProxy live2DCharacter;

    //CoinStash 이벤트
    [SerializeField] private GameObject CoinStashEventRoot;

    //이벤트 오브젝트 Table
    [SerializeField] private UITable tableEventObject;

    private int clearLayer = 1;
    private bool isSkip = false;
    private bool isClose = false;

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
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup()
    {
        //TopUI보이게 설정.
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);

        //스킵상태 저장
        isSkip = GameManager.instance.SkipIngameClear;
        isClose = false;

        //결과창 UI 초기화
        InitResultUIRoot();

        //결과 스파인 초기화
        InitSpineFlower();

        //버튼 설정
        InitButton();

        //결과 팝업 연출
        StartCoroutine(CoActionClear());
    }

    #region 초기화 함수
    private void InitResultUIRoot()
    {
        //결과 UI 비활성화
        sprResultUIRoot.gameObject.SetActive(false);
        sprResultUIRoot.transform.localScale = Vector3.zero;
        sprResultUIRoot.alpha = 0;

        //코인 배수 이벤트 비활성
        coinRatioEventRoot.SetActive(false);
        if (Global.coinEvent != 0)
            coinEventLabel.text = "x" + Global.coinEvent.ToString();

        //스테이지 텍스트
        labelStage.text = Global._instance.GetString("p_cs_sc_1").Replace("[n]", Global.stageIndex.ToString());

        //베스트 코인
        bestCoinRoot.SetActive(false);
        bestCoinTitle.text = Global._instance.GetString("p_cs_sc_5");
        labelBestCoin.text = ManagerCoinBonusStage.instance.GetStoredBestScore().ToString();

        //디폴트 코인
        string defaultTitleText = Global._instance.GetString("p_cs_sc_2");
        for (int i = 0; i < labelTitle_DefaultCoin.Length; i++)
            labelTitle_DefaultCoin[i].text = defaultTitleText;
        labelDefaultCoin.text = (ManagerBlock.instance.coins - ManagerBlock.instance.feverCoins).ToString();

        //피버시간 코인
        string feverTitleText = Global._instance.GetString("p_cs_sc_3");
        for (int i = 0; i < labelTitle_feverCoin.Length; i++)
            labelTitle_feverCoin[i].text = feverTitleText;
        labelFeverCoin.text = "0";

        //획득한 코인 합산
        string allTitleText = Global._instance.GetString("p_cs_sc_4");
        for (int i = 0; i < labelTitle_allCoin.Length; i++)
            labelTitle_allCoin[i].text = allTitleText;
        labelAllCoin.text = ManagerBlock.instance.coins.ToString();
        labelAllCoin.alpha = 0;
    }

    private void InitSpineFlower()
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

    private void InitButton()
    {
        string closeText = Global._instance.GetString("btn_1");
        for (int i = 0; i < btnCloseText.Length; i++)
            btnCloseText[i].text = closeText;
        objBtnClose.SetActive(false);
    }
    #endregion

    private IEnumerator CoActionClear()
    {
        //결과 스파인 연출
        yield return CoAction_ResultSpine();

        //결과창 UI 등장 연출
        yield return CoAction_ShowResultUI();

        //캐릭터 등장 연출
        Action_AppearCharacter();

        //현재 스테이지에서 획득한 코인 정산 연출
        yield return CoAction_CoinStageResult();

        yield return new WaitForSeconds(0.5f);

        //코인 배수 이벤트 진행 중일 때, 코인 배율 연출
        if (Global.coinEvent != 0)
            yield return CoAction_CoinEventAtCoinStage();

        //베스트 코인이 변경되었을 때 연출.
        if (ManagerCoinBonusStage.instance.GetCurrentBestScore() > ManagerCoinBonusStage.instance.GetStoredBestScore())
            ActionBestCoin();

        ManagerSound.AudioPlay(AudioInGame.CONTINUE);

        //상단 재화 UI 업데이트 연출
        yield return CoAction_UpdateTopUI();

        //이벤트 관련
        SetCoinStashEvent();

        yield return new WaitForEndOfFrame();

        //그로씨
        Global.GameInstance.SendClearGrowthyLog();

        //버튼 입력 활성화 처리
        objBtnClose.SetActive(true);
        this.bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;

        //CoinStash 이벤트 연출 스킵 상관없이 시작
        if (CoinStashEventRoot.activeInHierarchy)
            CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>()?.StartDirecting();

        //table 정렬
        tableEventObject.Reposition();
    }

    #region 연출 함수
    private IEnumerator CoAction_ResultSpine()
    {
        //게임 타입에 따라 스파인 이름 설정.
        var spineNames = Global.GameInstance.GetSpineAniNames_GameClearPopup(ManagerBlock.instance.flowrCount);
        string clearStartName = spineNames.Item1;
        string clearLoopName = spineNames.Item2;

        //스파인 애니메이션 설정
        spineFlower.gameObject.SetActive(true);
        if (isSkip == true)
        {
            spineFlower.state.SetAnimation(0, clearLoopName, true);
        }
        else
        {
            spineFlower.state.SetAnimation(0, clearStartName, false);
            spineFlower.state.AddAnimation(0, clearLoopName, true, 0f);
        }

        yield return new WaitForSeconds(0.2f);

        //게임 타입에 따른 결과 꽃 사운드.
        yield return Global.GameInstance.PlaySound_ResultStar(isSkip, 0);
    }

    void Action_AppearCharacter()
    {
        live2DCharacter = LAppModelProxy.MakeLive2D(live2DAnchor, TypeCharacterType.Boni);
        live2DCharacter.SetRenderer(false);
        live2DCharacter.SetAnimation("Clear_appear", "Clear_loop");
        StartCoroutine(live2DCharacter.CoSetRenderer(true));
        live2DCharacter.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        live2DCharacter.SetScale(450f);
        live2DCharacter.SetSortOrder(clearLayer);
    }

    private IEnumerator CoAction_ShowResultUI()
    {
        sprResultUIRoot.gameObject.SetActive(true);

        Vector3 flowerTarget = new Vector3(0, 256, 0);

        if (isSkip == false)
        {
            float actionTime = 0.3f;
            sprResultUIRoot.transform.DOScale(Vector3.one, actionTime);
            DOTween.ToAlpha(() => sprResultUIRoot.color, x => sprResultUIRoot.color = x, 1f, actionTime);
            spineFlower.gameObject.transform.DOLocalMoveY(flowerTarget.y, actionTime);
            yield return new WaitForSeconds(actionTime);
        }
        sprResultUIRoot.gameObject.transform.localScale = Vector3.one;
        sprResultUIRoot.alpha = 1;
        spineFlower.gameObject.transform.localPosition = flowerTarget;
    }

    private IEnumerator CoAction_CoinStageResult()
    {
        bestCoinRoot.SetActive(true);

        if (isSkip == true)
        {   
            labelFeverCoin.text = ManagerBlock.instance.feverCoins.ToString();
            labelAllCoin.alpha = 1;
            yield break;
        }

        labelAllCoin.transform.localScale = Vector3.zero;

        int allCointCount = ManagerBlock.instance.coins - ManagerBlock.instance.feverCoins;
        int addCoin = 0;
        int addBonusCount = 1;

        addBonusCount = (int)(allCointCount / 15f);
        if (addBonusCount <= 1)
            addBonusCount = 2;

        while (true)
        {
            addCoin += addBonusCount;
            labelDefaultCoin.text = addCoin.ToString();

            if (addCoin >= allCointCount)
            {
                labelDefaultCoin.text = allCointCount.ToString();
                break;
            }

            labelDefaultCoin.gameObject.transform.localScale = Vector3.one * 1.1f;
            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
            yield return new WaitForSeconds(0.01f);
            
            labelDefaultCoin.gameObject.transform.localScale = Vector3.one * 0.98f;
            yield return new WaitForSeconds(0.01f);
        }
        labelDefaultCoin.gameObject.transform.localScale = Vector3.one;

        allCointCount = ManagerBlock.instance.feverCoins;
        addCoin = 0;
        addBonusCount = 1;

        addBonusCount = (int)(allCointCount / 15f);
        if (addBonusCount <= 1)
            addBonusCount = 2;

        while (true)
        {
            addCoin += addBonusCount;
            labelFeverCoin.text = addCoin.ToString();

            if (addCoin >= allCointCount)
            {
                labelFeverCoin.text = allCointCount.ToString();
                break;
            }

            labelFeverCoin.gameObject.transform.localScale = Vector3.one * 1.1f;
            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
            yield return new WaitForSeconds(0.01f);

            labelFeverCoin.gameObject.transform.localScale = Vector3.one * 0.98f;
            yield return new WaitForSeconds(0.01f);
        }
        labelFeverCoin.gameObject.transform.localScale = Vector3.one;
        ManagerSound.AudioPlayMany(AudioInGame.GET_CANDY);

        DOTween.ToAlpha(() => labelAllCoin.color, x => labelAllCoin.color = x, 1f, 0.2f);
        labelAllCoin.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator CoAction_CoinEventAtCoinStage()
    {
        coinRatioEventRoot.SetActive(true);
        Color fontColor = new Color(1f, 254f / 255f, 72f / 255f);
        Color effectColor = new Color(180f / 255f, 74f / 255f, 0f);
        int allCoin = (ManagerBlock.instance.coins * Global.coinEvent);

        if (isSkip == false)
        {
            int coinCount = ManagerBlock.instance.coins;
            int bonusCount = (Global.coinEvent - 1) * coinCount;

            int addBonusCount = (int)((float)bonusCount / 15f);
            if (addBonusCount <= 1)
                addBonusCount = 2;

            while (true)
            {
                coinCount += addBonusCount;
                labelAllCoin.text = coinCount.ToString();

                if (coinCount >= allCoin)
                {
                    labelAllCoin.text = allCoin.ToString();
                    break;
                }
                labelAllCoin.gameObject.transform.localScale = Vector3.one * 1.1f;
                ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
                yield return new WaitForSeconds(0.01f);

                labelAllCoin.gameObject.transform.localScale = Vector3.one * 0.98f;
                yield return new WaitForSeconds(0.01f);
            }
            labelAllCoin.gameObject.transform.localScale = Vector3.one;
        }
        labelAllCoin.text = allCoin.ToString();
        labelAllCoin.color = fontColor;
        labelAllCoin.effectStyle = UILabel.Effect.Outline8;
        labelAllCoin.effectColor = effectColor;

        GameObject effectObj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, objEffectBlockCollect);
        effectObj.transform.position = labelAllCoin.transform.position;
    }

    private void ActionBestCoin()
    {
        labelBestCoin.text = ManagerCoinBonusStage.instance.GetCurrentBestScore().ToString();
        labelBestCoin.effectColor = new Color(204f / 255f, 56f / 255f, 5f / 255f, 1f);
        labelBestCoin.color = new Color(255f / 255f, 251f / 255f, 1f / 255f, 1f);
        NGUITools.AddChild(this.gameObject, objEffectBestScore);
    }

    private IEnumerator CoAction_UpdateTopUI()
    {
        //코인 배수 이벤트 설정
        int coinCount = (Global.coinEvent > 0) ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;
        int totalCoinCount = Global.coin + coinCount;

        //상단 UI 코인 획득 연출
        if (isSkip == false)
        {
            yield return new WaitForSeconds(0.7f);

            ManagerUI._instance.UpdateUI();

            int addCount = coinCount / 10;
            if (addCount <= 0)
                addCount = 1;

            while (Global.coin < totalCoinCount)
            {
                Global.coin += addCount;
                ManagerUI._instance.UpdateUI();
                yield return new WaitForSeconds(0.03f);
            }
        }

        Global.coin = totalCoinCount;
        ManagerUI._instance.UpdateUI();
    }
    #endregion

    void SetCoinStashEvent()
    {
        if (ManagerCoinStashEvent.CheckStartable())
        {
            CoinStashEventRoot.SetActive(true);
            CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>()?.InitData();
        }
        else
        {
            CoinStashEventRoot.SetActive(false);
        }
    }

    protected override void OnClickBtnClose()
    {
        if (this.bCanTouch == false)
            return;

        if (!isClose)
        {
            isClose = true;
            this.bCanTouch = false;
            ManagerSound._instance.StopBGM();
            StartCoroutine(CoClickBtnClose());
        }
    }

    private IEnumerator CoClickBtnClose()
    {
        yield return new WaitForSeconds(0.5f);

        ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
        SceneLoading.MakeSceneLoading("Lobby");

        Global.day = (int)GameData.User.day;
        Global.clover = (int)(GameData.Asset.AllClover);
        Global.coin = (int)(GameData.Asset.AllCoin);
        Global.jewel = (int)(GameData.Asset.AllJewel);
        Global.wing = (int)(GameData.Asset.AllWing);
        Global.exp = (int)GameData.User.expBall;
    }
}
