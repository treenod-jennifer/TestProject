using System.Collections;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;

public class UIPopupTurnRelay_IngameClear : UIPopupBase
{
    //결과 UI 루트
    [SerializeField] private UISprite sprResultUIRoot;

    //웨이브 정보
    [SerializeField] private UILabel labelWave;

    //결과창 버튼
    [SerializeField] private GameObject objBtnOK;

    //남은 턴 정보
    [SerializeField] private UILabel labelRemainTurn;

    //현재 웨이브 포인트 정보
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPointCnt;
    [SerializeField] private GameObject objBubbleBonusPoint;
    [SerializeField] private UILabel labelBubbleBonusPoint;

    //이벤트 랭킹 정보
    [SerializeField] private GameObject objEventRank;
    [SerializeField] private UILabel labelEventRank;

    //이펙트
    [SerializeField] private UIItemFlyEffect itemFlyEffect;
    [SerializeField] private UIItemFlyEffect itemFlyEffect_Rank;
    [SerializeField] private GameObject objEffectBlockCollect;
    [SerializeField] private GameObject objEffectBlockCollect_Rank;
    [SerializeField] private Renderer[] rendererEffect;

    //결과 스파인
    [SerializeField] private SkeletonAnimation spineResult;

    //라이브 2d
    [SerializeField] private GameObject live2DAnchor;
    private LAppModelProxy live2DCharacter;
    
    //이벤트 관련
    //이벤트 아이콘
    [SerializeField] private GameObject DiaStashEventRoot;
    
    //이벤트 오브젝트 Table
    [SerializeField] private UITable tableEventObject;

    private int clearLayer = 1;

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        clearLayer = layer;

        //클리어창 전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d, spine만 레이어 올려줌.
        if (layer == 10)
        {
            spineResult.GetComponent<MeshRenderer>().sortingOrder = clearLayer;
            clearLayer += 1;
        }
        else
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            spineResult.GetComponent<MeshRenderer>().sortingOrder = clearLayer + 1;
            clearLayer += 2;
        }

        for (int i = 0; i < rendererEffect.Length; i++)
            rendererEffect[i].sortingOrder = clearLayer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup()
    {
        //TopUI보이게 설정.
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);

        //결과창 UI 초기화
        InitResultUIRoot();

        //결과 팝업 연출
        StartCoroutine(CoActionClear());
    }

    private void InitResultUIRoot()
    {
        //결과 UI 비활성화
        sprResultUIRoot.gameObject.SetActive(false);
        sprResultUIRoot.transform.localScale = Vector3.zero;
        sprResultUIRoot.alpha = 0;

        //웨이브 텍스트 설정
        labelWave.text = Global.GameInstance.GetStageText_IngamePopup();

        //남은 턴 텍스트 설정
        string remainTurnText = ManagerTurnRelay.turnRelayIngame.RemainTurn.ToString();
        labelRemainTurn.text = remainTurnText;
        labelBubbleBonusPoint.text = string.Format("+{0}", remainTurnText);

        //현재 포인트 텍스트 설정(남은 턴을 합친 점수 x)
        eventPointCnt.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.IngameEventPoint);
        
        //유예기간에 따라 랭크 표시
        objEventRank.SetActive(false);
        if (ManagerTurnRelay.turnRelayIngame.IsPlayEnd == false && ManagerTurnRelay.IsRankingEnabled())
        {
            objEventRank.transform.localScale = Vector3.zero;
            int prevRankPoint = ManagerTurnRelay.turnRelayCoop.myScore -
                (ManagerTurnRelay.turnRelayIngame.IngameEventPoint + ManagerTurnRelay.turnRelayIngame.RemainTurn);
            labelEventRank.text = prevRankPoint.ToString();
        }

        //연출에 필요한 오브젝트들 비활성화 처리
        itemFlyEffect.gameObject.SetActive(false);
        itemFlyEffect_Rank.gameObject.SetActive(false);
        objBubbleBonusPoint.SetActive(false);

        //버튼 설정
        objBtnOK.SetActive(false);
    }

    private IEnumerator CoActionClear()
    {
        //결과 스파인 연출
        yield return CoAction_ResultSpine();

        //결과창 UI 등장 연출
        yield return CoAction_ShowResultUI();

        //캐릭터 등장 연출
        Action_AppearCharacter();
        
        //이벤트 관련
        SetDiaStashEvent();

        //총 점수 연출
        yield return CoAction_AddTotalScore();

        ManagerSound.AudioPlay(AudioInGame.CONTINUE);

        //그로씨
        Global.GameInstance.SendClearGrowthyLog();
        Global.GameInstance.SendClearRewardGrowthyLog();

        //버튼 입력 활성화 처리
        objBtnOK.SetActive(true);
        this.bCanTouch = true;
        ManagerUI._instance.bTouchTopUI = true;
        
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
        spineResult.gameObject.SetActive(true);
        spineResult.state.SetAnimation(0, clearStartName, false);
        spineResult.state.AddAnimation(0, clearLoopName, true, 0f);

        yield return new WaitForSeconds(0.2f);

        //게임 타입에 따른 결과 꽃 사운드.
        yield return Global.GameInstance.PlaySound_ResultStar(false, 0);
    }

    private IEnumerator CoAction_ShowResultUI()
    {
        sprResultUIRoot.gameObject.SetActive(true);

        Vector3 flowerTarget = new Vector3(0f, 335f, 0f);

        float actionTime = 0.3f;
        sprResultUIRoot.transform.DOScale(Vector3.one, actionTime);
        DOTween.ToAlpha(() => sprResultUIRoot.color, x => sprResultUIRoot.color = x, 1f, actionTime);
        spineResult.gameObject.transform.DOLocalMoveY(flowerTarget.y, actionTime);
        yield return new WaitForSeconds(actionTime);

        sprResultUIRoot.gameObject.transform.localScale = Vector3.one;
        sprResultUIRoot.alpha = 1;
        spineResult.gameObject.transform.localPosition = flowerTarget;
    }

    void Action_AppearCharacter()
    {
        live2DCharacter = LAppModelProxy.MakeLive2D(live2DAnchor, TypeCharacterType.Boni);
        live2DCharacter.SetRenderer(false);
        live2DCharacter.SetSortOrder(clearLayer);
        live2DCharacter.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        live2DCharacter.SetScale(450f);
        live2DCharacter.SetAnimation("Clear_appear", "Clear_loop");
        StartCoroutine(live2DCharacter.CoSetRenderer(true));
    }

    private IEnumerator CoAction_AddTotalScore()
    {
        //턴 포인트 증가 연출
        yield return CoAction_AddTurn();

        //이벤트 랭크 포인트 증가 연출
        if (ManagerTurnRelay.turnRelayIngame.IsPlayEnd == false)
        {
            yield return CoAction_AddEventRankPoint();
        }
        else
        {
            yield return CoShowPlayEndPopup();
        }
    }

    private IEnumerator CoAction_AddTurn()
    {
        int remainTurn = ManagerTurnRelay.turnRelayIngame.RemainTurn;
        int totalPoint = ManagerTurnRelay.turnRelayIngame.IngameEventPoint + remainTurn;

        if (remainTurn > 0)
        {
            //턴에서 점수로 날아가는 이펙트 생성
            bool isEffectEnd = false;
            itemFlyEffect.gameObject.SetActive(true);
            itemFlyEffect.SetSpriteImage("icon_apple_stroke", labelRemainTurn.depth, 0.5f);
            itemFlyEffect.InitFlyEffect(eventPointCnt.GetTextTr().position, 0.5f, clearLayer, () => isEffectEnd = true);
            yield return new WaitUntil(() => isEffectEnd == true);

            objBubbleBonusPoint.SetActive(true);
            yield return new WaitForSeconds(0.1f);

            //남은턴이 일정 이상이 되면, 연출 시간을 줄이기 위해 증가폭을 다르게 설정
            int addCount = (remainTurn > 20) ? ((int)((float)remainTurn / 20f) + 1) : 1;
            int point = ManagerTurnRelay.turnRelayIngame.IngameEventPoint;

            //연출 시간
            float actionTime = 0.02f;

            //카운트 증가 연출
            while (true)
            {
                //사운드추가
                ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);

                //카운트 증가
                point += addCount;
                if (point > totalPoint)
                    point = totalPoint;

                //텍스트 변경
                eventPointCnt.SetEventPointText(point);

                //스케일 연출
                eventPointCnt.SetTextAction(actionTime);

                if (point >= totalPoint)
                    break;
                else
                    yield return new WaitForSeconds(actionTime);
            }
        }

        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        eventPointCnt.SetEventPointText(totalPoint);
        objEffectBlockCollect.SetActive(true);

        if (remainTurn > 0)
        {
            objBubbleBonusPoint.SetActive(true);
            labelBubbleBonusPoint.text = string.Format("+{0}", remainTurn);
        }
    }

    private IEnumerator CoAction_AddEventRankPoint()
    {
        //UI 이동 연출
        float targetPosY = eventPointCnt.transform.localPosition.y + 30f;
        eventPointCnt.transform.DOLocalMoveY(targetPosY, 0.2f);
        yield return new WaitForSeconds(0.1f);

        //랭크 UI 등장 연출
        ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
        objEventRank.SetActive(true);
        objEventRank.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.2f);

        bool isEffectEnd = false;
        itemFlyEffect_Rank.gameObject.SetActive(true);
        itemFlyEffect_Rank.SetSpriteImage("capsuleToy_Token", labelEventRank.depth, 0.5f);
        itemFlyEffect_Rank.InitFlyEffect(labelEventRank.transform.position, 0.3f, clearLayer, () => isEffectEnd = true);
        yield return new WaitUntil(() => isEffectEnd == true);

        //증가폭 설정
        int prevRankPoint = ManagerTurnRelay.turnRelayCoop.myScore -
                (ManagerTurnRelay.turnRelayIngame.IngameEventPoint + ManagerTurnRelay.turnRelayIngame.RemainTurn);

        int targetPoint = ManagerTurnRelay.turnRelayCoop.myScore;
        int diffPoint = targetPoint - prevRankPoint;

        //획득 포인트가 일정 이상이 되면, 연출 시간을 줄이기 위해 증가폭을 다르게 설정
        int addCount = (diffPoint > 20) ? ((int)((float)diffPoint / 20f) + 1) : 1;
        int point = prevRankPoint;

        //연출 시간
        float actionTime = 0.02f;

        //카운트 증가 연출
        while (true)
        {
            //사운드추가
            ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);

            //카운트 증가
            point += addCount;
            if (point > targetPoint)
                point = targetPoint;

            //텍스트 변경
            labelEventRank.text = point.ToString();
            labelEventRank.transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);

            if (point >= targetPoint)
                break;
            else
                yield return new WaitForSeconds(actionTime);
        }

        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        objEffectBlockCollect_Rank.SetActive(true);
        labelEventRank.text = ManagerTurnRelay.turnRelayCoop.myScore.ToString();
    }

    private IEnumerator CoShowPlayEndPopup()
    {
        //보상기간 종료 안내 팝업
        bool isClosePopup = false;
        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) =>
            {
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_14"), false);
            },
            endCallback: () => { isClosePopup = true; }
        );

        //팝업이 닫힐 때 까지 대기
        yield return new WaitUntil(() => isClosePopup == true);
    }
    #endregion

    protected override void OnClickBtnClose()
    {
        if (this.bCanTouch == false)
            return;

        this.bCanTouch = false;
        ManagerSound._instance.StopBGM();
        StartCoroutine(CoClickBtnClose());
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
}
