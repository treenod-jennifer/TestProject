using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupTurnRelay_IngamePause_Warning : UIPopupBase
{
    //총 획득량(현재까지 모은 포인트)
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPoint_Current;

    //다음 획득 가능한 포인트
    [SerializeField] private UILabel[] labelTitle_Next;
    [SerializeField] private UIItemTurnRelay_EventPointCount eventPoint_Next;
    [SerializeField] private GameObject objLuckyStage;
    [SerializeField] private UILabel labelLuckyRatioText;

    //웨이브 표시
    [SerializeField] private UILabel labelWave;

    //게임 중지 시, 실행될 액션
    private System.Action stopAction = null;

    public void InitPopup(System.Action stopCallBack, System.Action closeCallBack)
    {   
        InitPopupText();
        InitLuckyStage();
        stopAction = stopCallBack;
        this._callbackClose += () => closeCallBack.Invoke();
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

        //다음 웨이브 텍스트
        string titleNextText = Global._instance.GetString("p_sc_7").Replace("[n]", ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString());
        for (int i = 0; i < labelTitle_Next.Length; i++)
            labelTitle_Next[i].text = titleNextText;
    }

    private void InitLuckyStage()
    {
        bool isLuckyStage = ManagerTurnRelay.turnRelayIngame.IsLuckyWave();
        objLuckyStage.SetActive(isLuckyStage);

        if(isLuckyStage == true)
        {
            labelLuckyRatioText.text = string.Format("X{0}", ManagerTurnRelay.turnRelayIngame.LuckRatio);
        }
    }

    private void OnClickBtnStop()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        stopAction?.Invoke();
    }
}
