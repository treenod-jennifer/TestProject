using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIPopupTurnRelay_ExistData : UIPopupBase
{
    //총 획득량(현재까지 모은 포인트)
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPoint_Current;

    //다음 획득 가능한 포인트
    [SerializeField] private UILabel[] labelTitle_Next;
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPoint_Next;
    [SerializeField] private GameObject objLuckyStage;
    [SerializeField] private UILabel labelLuckyRatioText;

    //남은턴
    [SerializeField] private UILabel labelRemainTurn;

    //웨이브 표시
    [SerializeField] private UILabel labelWave;

    //통신 관련
    private bool networkEnd = false;

    public void InitPopup()
    {
        //팝업이 출력되면 턴 릴레이 상태로 게임 타입 변경시켜줌.
        Global.SetGameType_TurnRelay(ManagerTurnRelay.instance.EventIndex, ManagerTurnRelay.turnRelayIngame.CurrentWave);

        InitPopupText();
        InitLuckyStage();
    }

    private void InitPopupText()
    {
        //현재 웨이브 표시
        labelWave.text = string.Format("{0} {1}/{2}", Global._instance.GetString("p_sc_5"),
           ManagerTurnRelay.turnRelayIngame.CurrentWave, ManagerTurnRelay.instance.MaxWaveCount);

        //이벤트 포인트 설정
        eventPoint_Current.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.IngameEventPoint);
        eventPoint_Next.InitEventPointUI(ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1, 
            ManagerTurnRelay.turnRelayIngame.IsLuckyWave());
        labelRemainTurn.text = string.Format("+{0}", ManagerTurnRelay.turnRelayIngame.RemainTurn);

        //다음 웨이브 텍스트
        string titleNextText = Global._instance.GetString("p_sc_7").Replace("[n]", ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString());
        for (int i = 0; i < labelTitle_Next.Length; i++)
            labelTitle_Next[i].text = titleNextText;
    }

    private void InitLuckyStage()
    {
        bool isLuckyStage = ManagerTurnRelay.turnRelayIngame.IsLuckyWave();
        objLuckyStage.SetActive(isLuckyStage);

        if (isLuckyStage == true)
        {
            labelLuckyRatioText.text = string.Format("X{0}", ManagerTurnRelay.turnRelayIngame.LuckRatio);
        }
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
        // 만든 블럭 횟수 
        var usedItems = new List<int>();
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(0));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(1));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(2));
        usedItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalMakeBombCount(3));

        // 0~5 레디아이템, 6~10 인게임아이템, 15~16 더블레디아이템
        var paidItems = new List<int>();
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(0));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(1));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(2));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(3));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(4));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(5));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(0));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(1));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(2));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(3));
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetTotalUseIngameItemCount(4));
        paidItems.Add(0); //11
        paidItems.Add(0); //12
        paidItems.Add(0); //13
        paidItems.Add(0); //14
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(6)); //15
        paidItems.Add(ManagerTurnRelay.turnRelayIngame.GetUseReadyItemCount(7)); //16

        networkEnd = false;

        int remainTurn = ManagerTurnRelay.turnRelayIngame.RemainTurn;

        //총 점수는, 현재까지 얻은 점수 + 남은 턴을 합친 점수
        int totalScore = ManagerTurnRelay.turnRelayIngame.IngameEventPoint + remainTurn;

        var arg = new TurnRelayGameClearReq()
        {
            chapter = 0,
            stage = 1,
            eventIdx = ManagerTurnRelay.instance.EventIndex,
            type = (int)GameType.TURN_RELAY,
            playSec = (int)ManagerTurnRelay.turnRelayIngame.TotalPlayTime,
            gameScore = totalScore,
            gameTurn = remainTurn,
            seed = 0,
            easyMode = 0,
            items = usedItems,
            gameKey = ServerRepos.lastGameKey,
            bonusItemsSelected = ManagerTurnRelay.turnRelayIngame.listTotalBonusItem_Select,
            paidItems = paidItems,
            stageKey = Global.GameInstance.GetStageKey(),
        };

        ServerAPI.TurnRelayGameClear(arg, recvTurnRelayInGameClear);

        // 네트워크 통신이 완료될때까지 기다림
        yield return new WaitUntil(() => networkEnd);

        //팝업 닫으면서 결과 팝업 출력
        _callbackClose += () => OpenPopupGetReward();

        ClosePopUp();
    }

    void recvTurnRelayInGameClear(TurnRelayGameClearResp resp)
    {
        if (resp.IsSuccess)
        {
            networkEnd = true;

            if(ManagerDiaStash.instance != null && ManagerDiaStash.CheckStartable())
            {
                //다이아 주머니 적립 그로시를 남길지 조건 검사 (스테이지 클리어 여부 확인)
                ManagerDiaStash.instance.SetStageApplyDiaStash(ManagerDiaStash.CheckStartable());
                //클리어시 유저의 다이아 갯수를 갱신 해주는 코드
                //로비화면에서 클리어를 했기에 해당 부분은 따로 추가
                ManagerDiaStash.instance.SyncUserDiaCount();
            }

            Global.GameInstance.OnRecvGameClear(null, resp);
            Global.GameInstance.SendClearGrowthyLog();
            Global.GameInstance.SendClearRewardGrowthyLog(resp);
        }
    }

    private void OpenPopupGetReward()
    {
        string title = Global._instance.GetString("p_t_4");
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_1"), false);
        popup.SortOrderSetting();
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
            //게임 시작 설정
            ManagerLobby._stageStart = true;
            
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
}
