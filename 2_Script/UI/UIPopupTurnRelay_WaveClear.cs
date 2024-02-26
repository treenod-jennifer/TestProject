using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Protocol;
using DG.Tweening;

public class UIPopupTurnRelay_WaveClear : UIPopupBase
{
    [Header("**Upper Panel")]
    //웨이브 정보
    [SerializeField] private UILabel labelWave_Current;
    [SerializeField] private UILabel labelWave_All;

    //결과 스파인
    [SerializeField] private SkeletonAnimation spineResult;

    //스파인 위에 배치될 UI
    [SerializeField] private UIPanel panel_upperSpine;
    [SerializeField] private UILabel labelTotalCount;

    [Header("**Point")]
    //현재 웨이브 포인트 정보
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPointCnt_current;

    //다음 웨이브 포인트 정보
    [SerializeField] private UILabel[] labelTitle_Next;
    [SerializeField] private GameObject objLuckyStageIcon_next;
    [SerializeField] private UILabel labelLuckyRatioText;
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPointCnt_next;

    //이월된 턴 정보
    [SerializeField] private UILabel labelRemainTurn;

    //결과창 버튼
    [SerializeField] private GameObject objBtnGetReward;
    [SerializeField] private GameObject objBtnPlay;

    [Header("**Effect")]
    //이펙트
    [SerializeField] private UIItemFlyEffect itemFlyEffect;
    [SerializeField] private GameObject objEffectBlockCollect;
    [SerializeField] private Renderer[] rendererEffect;

    private int currentWave = 0;

    private int clearLayer = 1;
    private bool isSkip = false;

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
        panel_upperSpine.useSortingOrder = true;
        panel_upperSpine.sortingOrder = clearLayer + 1;

        for (int i = 0; i < rendererEffect.Length; i++)
            rendererEffect[i].sortingOrder = clearLayer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup()
    {   
        currentWave = ManagerTurnRelay.turnRelayIngame.CurrentWave;

        //스킵상태 저장
        isSkip = GameManager.instance.SkipIngameClear;
        GameManager.instance.SkipIngameClear = false;

        //웨이브 텍스트 설정
        labelWave_Current.text = string.Format("{0} {1}/", Global._instance.GetString("p_sc_5"), currentWave);
        labelWave_All.text = ManagerTurnRelay.instance.MaxWaveCount.ToString();

        int allPoint = ManagerTurnRelay.turnRelayIngame.IngameEventPoint;
        int currentWavePoint = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1;

        //현재 스테이지에서 획득한 포인트 텍스트 설정
        labelTotalCount.text = currentWavePoint.ToString();

        //전체 포인트 텍스트 설정
        eventPointCnt_current.InitEventPointUI(allPoint - currentWavePoint);

        //다음 포인트 텍스트 설정
        int nextWave = currentWave + 1;

        //다음 웨이브 텍스트
        string titleNextText = Global._instance.GetString("p_sc_7").Replace("[n]", nextWave.ToString());
        for (int i = 0; i < labelTitle_Next.Length; i++)
            labelTitle_Next[i].text = titleNextText;

        bool isLuckStage_Next = ManagerTurnRelay.turnRelayIngame.IsLuckyWave(nextWave);
        objLuckyStageIcon_next.SetActive(isLuckStage_Next);
        eventPointCnt_next.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave(nextWave).Item1, isLuckStage_Next);

        if (isLuckStage_Next == true)
        {
            labelLuckyRatioText.text = string.Format("X{0}", ManagerTurnRelay.turnRelayIngame.LuckRatio);
        }

        //이월된 턴 텍스트 설정
        labelRemainTurn.text = string.Format("+{0}", ManagerTurnRelay.turnRelayIngame.RemainTurn.ToString());

        //스파인 초기화
        InitResultSpine();

        //연출에 필요한 오브젝트들 비활성화 처리
        itemFlyEffect.gameObject.SetActive(false);

        //웨이브 결과 화면 연출
        StartCoroutine(CoActionResult());
    }

    private void InitResultSpine()
    {
        string clearStartName = string.Format("{0}_appear", ManagerTurnRelay.turnRelayIngame.CurrentWave);
        string clearLoopName = string.Format("{0}_idle", ManagerTurnRelay.turnRelayIngame.CurrentWave);

        if (isSkip == true)
        {
            spineResult.state.SetAnimation(0, clearLoopName, true);
        }
        else
        {
            spineResult.state.SetAnimation(0, clearStartName, false);
            spineResult.state.AddAnimation(0, clearLoopName, true, 0f);
        }
    }

    private IEnumerator CoActionResult()
    {
        if (isSkip == false)
        {
            yield return new WaitForSeconds(1.0f);
            ManagerSound.AudioPlay(AudioInGame.TURNRELAY_WAVECLEAR);

            //현재 포인트에서 전체 포인트로 날아가는 이펙트 생성
            bool isEffectEnd = false;
            itemFlyEffect.gameObject.SetActive(true);
            itemFlyEffect.InitFlyEffect(eventPointCnt_current.GetTextTr().position, 0.5f, clearLayer, () => isEffectEnd = true);
            yield return new WaitUntil(() => isEffectEnd == true);
        }
        eventPointCnt_current.SetTextAction(0.2f);
        eventPointCnt_current.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.IngameEventPoint);
        objEffectBlockCollect.SetActive(true);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
    }

    private void OnClickBtnGetReward()
    {
        if (this.bCanTouch == false)
            return;

        this.bCanTouch = false;

        StartCoroutine(CoGetReward());
    }

    private IEnumerator CoGetReward()
    {
        yield return GameManager.instance.CoStageClear_TurnRelay();

        //팝업 닫으면서 결과 팝업 출력
        _callbackClose += () => ManagerUI._instance.OpenPopupTurnRelay_IngameClear();
        ClosePopUp();
    }

    private void OnClickBtnPlay()
    {
        if (bCanTouch == false)
            return;

        bCanTouch = false;
        
        //스테이지 정보 로드 후, 게임 시작 처리
        ServerRepos.GameStartTs = Global.GetTime();
        StartCoroutine(ManagerUI._instance.CoLoadStageData_TurnRelayEvent(() => StartGame_TurnRelay()));
    }

    private void StartGame_TurnRelay()
    {
        ServerAPI.TurnRelayGameProceed(recvProceed);
    }

    private void recvProceed(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //레디 아이템 모두 해제
            int readyItemCount = Global._instance.GetReadyItemCount();
            for (int i = 0; i < readyItemCount; i++)
            {
                UIPopupReady.readyItemUseCount[i] = new EncValue();
                UIPopupReady.readyItemUseCount[i].Value = 0;
            }
            
            for (int i = 0; i < 6; i++)
            {
                UIPopupReady.readyItemAutoUse[i] = new EncValue();
                UIPopupReady.readyItemAutoUse[i].Value = 0;
            }

            //턴 릴레이 웨이브 데이터 초기화
            ManagerTurnRelay.turnRelayIngame.InitIngameUserData();

            //인게임 진입 처리
            ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
            ManagerSound._instance.StopBGM();
            SceneLoading.MakeSceneLoading("InGame");
            ManagerUI._instance.bTouchTopUI = true;
        }
    }

    private void RetryCancel()
    {
        Global.ReBoot();
    }

    //나가기 버튼 막아놓음
    protected override void OnClickBtnClose()
    {
        return;
    }
}
