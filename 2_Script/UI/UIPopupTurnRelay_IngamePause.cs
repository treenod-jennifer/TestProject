using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIPopupTurnRelay_IngamePause : UIPopupBase
{
    [SerializeField] private UILabel labelStageText;

    private void Start()
    {
        ManagerSound._instance.PauseBGM();

        //스테이지 표시 텍스트 설정
        labelStageText.text = Global.GameInstance.GetStageText_IngamePopup();
    }

    private void OnClickBtnMenu()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        ManagerUI._instance.OpenPopupTurnRelay_IngamePause_Warning(FailProcess, () => bCanTouch = true);
    }

    void FailProcess()
    {
        ManagerSound._instance.StopBGM();

        #region 그로씨
        #endregion

        #region 실패 처리
        var usedItems = new List<int>();

        //사용한 레디 아이템 정보
        usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);

        //사용한 인게임 아이템 정보
        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameItemManager.useCount[7]);
        
        usedItems.Add(0); //11
        usedItems.Add(0); //12
        usedItems.Add(0); //13
        usedItems.Add(0); //14

        //사용한 더블 레디 아이템 정보
        usedItems.Add(UIPopupReady.readyItemUseCount[6].Value); //15
        usedItems.Add(UIPopupReady.readyItemUseCount[7].Value); //16

        var remainTarget = ManagerBlock.instance.GetListRemainTarget();
        int tempGameMode = (int)Global.GameType;

        var arg = new GameFailReq()
        {
            type = tempGameMode,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),

            gameCoin = 0,
            gameScore = 0,
            Remains = remainTarget,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = GameManager.instance.LevelAdjusted > 0 ? 1 : 0,
            items = usedItems
        };

        ServerAPI.TurnRelayGameCancel(arg, recvGameFail);
        #endregion
    }

    void recvGameFail(TurnRelayResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.GameInstance.OnRecvGameFail();
            SceneLoading.MakeSceneLoading("Lobby");
        }
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        if (GameManager.instance.state == GameState.PLAY)
            ManagerSound._instance.PlayBGM();
        base.ClosePopUp(_mainTime, callback);
    }
}
