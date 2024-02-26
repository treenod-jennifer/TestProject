using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;

public class UIPopupClear_WorldRanking : UIPopupBase
{
    //결과 UI 루트
    [SerializeField] private UISprite sprResultUIRoot;

    //클리어 보상 텍스트
    [SerializeField] private UILabel[] labelTitle_ClearBonus;
    [SerializeField] private UILabel[] labelCoin;

    //스파인 위에 배치될 UI
    [SerializeField] private UIPanel panel_upperSpine;
    [SerializeField] private UILabel[] labelGetCount_center_WR;

    //좌측 상단에 표시될 UI
    [SerializeField] private GameObject totalCountRoot;
    [SerializeField] private UILabel labelAllCount_up_WR;
    [SerializeField] private UILabel labelGetCount_up_WR;
    [SerializeField] private UISprite spriteCollectItem_up_WR;

    //일반 UI
    [SerializeField] private UILabel labelStage;
    [SerializeField] private UILabel[] labelTitleCollect_WR;
    [SerializeField] private UILabel labelGetCount_down_WR;
    [SerializeField] private UILabel labelRatio_WR;
    [SerializeField] private UISprite spriteCollectItem_down_WR;

    //코인 배수 이벤트
    [SerializeField] private GameObject coinRatioEventRoot;
    [SerializeField] private UILabel coinEventLabel;

    //결과 스파인
    [SerializeField] private SkeletonAnimation spineFlower;

    //결과창 버튼
    [SerializeField] private GameObject objBtnClose;
    [SerializeField] private UILabel[] btnCloseText;

    //라이브 2d
    [SerializeField] private GameObject live2DAnchor;
    private LAppModelProxy live2DCharacter;

    //CoinStash 이벤트
    [SerializeField] private GameObject CoinStashEventRoot;

    //DiaStash 이벤트
    [SerializeField] private GameObject DiaStashEventRoot;
    
    //이벤트 오브젝트 Table
    [SerializeField] private UITable tableEventObject;

    private int clearLayer = 1;
    private bool isSkip = false;
    private bool isClose = false;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        panel_upperSpine.depth = depth + 1;
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
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup()
    {
        // TopUI보이게 설정.
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

        //코인 배수 이벤트 UI 설정
        if (Global.coinEvent != 0)
        {
            coinRatioEventRoot.SetActive(true);
            coinEventLabel.text = "x" + Global.coinEvent.ToString();
        }
        else
        {
            coinRatioEventRoot.SetActive(false);
        }

        //스테이지 UI
        labelStage.text = $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";

        //아이템 카운트
        int prevCount = ManagerWorldRanking.userData.Score;
        int getCount = ManagerBlock.instance.worldRankingItemCount * ManagerBlock.instance.flowrCount;

        //좌측 상단에 배치된 UI
        totalCountRoot.SetActive(false);
        labelAllCount_up_WR.text = string.Format("{0:n0}", prevCount);
        labelGetCount_up_WR.text = string.Format("(+ {0:n0})", getCount);
        spriteCollectItem_up_WR.atlas = ManagerWorldRanking.resourceData?.worldRankingPack?.IngameAtlas ?? null;
        spriteCollectItem_up_WR.MakePixelPerfect();

        //스파인 위에 배치된 UI
        string text = string.Format("{0:n0}", getCount);
        for (int i = 0; i < labelGetCount_center_WR.Length; i++)
            labelGetCount_center_WR[i].text = text;

        //하단 결과 UI
        string collectText = Global._instance.GetString(string.Format("wrk_col_{0}", ManagerWorldRanking.contentsData.ResourceIndex));
        string title_item = Global._instance.GetString("p_sc_4").Replace("[1]", collectText);

        for (int i = 0; i < labelTitleCollect_WR.Length; i++)
            labelTitleCollect_WR[i].text = title_item;

        labelGetCount_down_WR.text = string.Format("{0}", ManagerBlock.instance.worldRankingItemCount);
        labelRatio_WR.text = string.Format("X{0}", ManagerBlock.instance.flowrCount);

        spriteCollectItem_down_WR.atlas = ManagerWorldRanking.resourceData?.worldRankingPack?.IngameAtlas ?? null;
        spriteCollectItem_down_WR.MakePixelPerfect();
        spriteCollectItem_down_WR.width = (int)(spriteCollectItem_down_WR.width * 0.8f);
        spriteCollectItem_down_WR.height = (int)(spriteCollectItem_down_WR.height * 0.8f);

        //코인 UI
        for (int i = 0; i < labelTitle_ClearBonus.Length; i++)
            labelTitle_ClearBonus[i].text = Global._instance.GetString("p_sc_3");

        int coinCount = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;// ManagerBlock.instance.coins;//(int)GameData.Asset.AllCoin - Global.coin;
        for (int i = 0; i < labelCoin.Length; i++)
        {
            labelCoin[i].text = coinCount.ToString();
            labelCoin[i].MakePixelPerfect();
        }
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

    IEnumerator CoActionClear()
    {  
        //결과 스파인 연출
        yield return CoAction_ResultSpine();

        //결과창 UI 등장 연출
        yield return CoAction_ShowResultUI();

        //캐릭터 등장 연출
        Action_AppearCharacter();

        //현재 스테이지에서 획득한 월드랭킹 아이템 정산 연출
        StartCoroutine(CoAction_WorldRankResult());

        yield return new WaitForSeconds(0.5f);

        ManagerSound.AudioPlay(AudioInGame.CONTINUE);

        //상단 재화 UI 업데이트 연출
        yield return CoAction_UpdateTopUI();

        //이벤트 관련
        SetCoinStashEvent();
        SetDiaStashEvent();

        yield return new WaitForEndOfFrame();

        //그로씨
        Global.GameInstance.SendClearRewardGrowthyLog();
        Global.GameInstance.SendClearGrowthyLog();

        //보상 팝업
        yield return Global.GameInstance.CoOpenClearRewardPopup();
     
        //버튼 입력 활성화 처리
        objBtnClose.SetActive(true);
        this.bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;

        //CoinStash 이벤트 연출 스킵 상관없이 시작
        if (CoinStashEventRoot.activeInHierarchy)
            CoinStashEventRoot.GetComponent<UIItemCoinStashEvent>()?.StartDirecting();
        
        //DiaStash 이벤트 연출 스킵 상관없이 시작
        if(DiaStashEventRoot.activeInHierarchy)
            DiaStashEventRoot.GetComponent<UIItemDiaStashEvent>()?.StartDirecting();

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

    private IEnumerator CoAction_WorldRankResult()
    {
        totalCountRoot.SetActive(true);
        int getCount = ManagerBlock.instance.worldRankingItemCount * ManagerBlock.instance.flowrCount;

        if (isSkip == true)
        {
            int allCount = ManagerWorldRanking.userData.Score + getCount;
            labelAllCount_up_WR.text = string.Format("{0:n0}", allCount);
            labelGetCount_up_WR.gameObject.SetActive(false);
            yield break;
        }

        //월드랭킹 아이템 하나당 연출 시간
        float actionTime = getCount * 0.05f;
        if (actionTime > 1.0f)
            actionTime = 1.0f;

        DOTween.To(ActionWorldRankingResult, 1, getCount, actionTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(actionTime);

        labelGetCount_up_WR.gameObject.SetActive(false);
    }

    private void ActionWorldRankingResult(float addCount)
    {
        int getCount = ManagerBlock.instance.worldRankingItemCount * ManagerBlock.instance.flowrCount;
        ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
        labelAllCount_up_WR.text = string.Format("{0:n0}", ManagerWorldRanking.userData.Score + (int)addCount);
        labelGetCount_up_WR.text = string.Format("(+ {0:n0})", getCount - (int)addCount);
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
