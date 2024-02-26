using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIItemTurnRelay_CooperationReward : MonoBehaviour
{
    //이미지
    [SerializeField] private UISprite spriteRewardBG;

    //보상
    [SerializeField] private GenericReward genericReward;

    //체크 아이콘
    [SerializeField] private GameObject objCheck;

    //보상 버튼
    [SerializeField] private GameObject objBtnReward;

    //미션 데이터
    private ManagerTurnRelay.CoopMissionData coopMissionData = null;

    private bool isCanTouch = true;

    public void InitCoopMissionReward_Default()
    {
        InitRewardBG(ManagerTurnRelay.CoopMissionState.INCOMPLETED);
        RefreshGetButton(ManagerTurnRelay.CoopMissionState.INCOMPLETED);
        genericReward.gameObject.SetActive(false);
    }

    public void InitCoopMissionReward(int index)
    {
        coopMissionData = ManagerTurnRelay.turnRelayCoop.GetCoopMission(index);
        if (coopMissionData == null)
            return;

        InitRewardBG(coopMissionData.state);
        RefreshGetButton(coopMissionData.state);
        InitRewardCount();
    }

    private void InitRewardBG(ManagerTurnRelay.CoopMissionState state)
    {
        //상태에 따라 배경 설정
        switch (state)
        {
            case ManagerTurnRelay.CoopMissionState.INVALID_STATE:
            case ManagerTurnRelay.CoopMissionState.INCOMPLETED:
                spriteRewardBG.spriteName = "turnRelay_Team_Mission_Off";
                break;
            case ManagerTurnRelay.CoopMissionState.RUNNING:
            case ManagerTurnRelay.CoopMissionState.COMPLETED:
            case ManagerTurnRelay.CoopMissionState.REWARD_FINISHED:
                spriteRewardBG.spriteName = "turnRelay_Team_Mission_On";
                break;
        }
    }

    private void InitRewardCount()
    {
        genericReward.gameObject.SetActive(true);
        genericReward.SetReward(coopMissionData.rewardData);
    }

    private void RefreshGetButton(ManagerTurnRelay.CoopMissionState state)
    {
        //상태에 따라 버튼 설정
        switch (state)
        {
            case ManagerTurnRelay.CoopMissionState.INVALID_STATE:
            case ManagerTurnRelay.CoopMissionState.INCOMPLETED:
            case ManagerTurnRelay.CoopMissionState.RUNNING:
                objBtnReward.SetActive(false);
                objCheck.SetActive(false);
                break;
            case ManagerTurnRelay.CoopMissionState.COMPLETED:
                objBtnReward.SetActive(true);
                objCheck.SetActive(false);
                break;
            case ManagerTurnRelay.CoopMissionState.REWARD_FINISHED:
                objBtnReward.SetActive(false);
                objCheck.SetActive(true);
                break;
        }
    }

    private void OnClickBtnGetReward()
    {
        if (isCanTouch == false)
            return;
        isCanTouch = false;
        ServerAPI.TurnRelayReqCoopAchieveReward(coopMissionData.index, RecvGetReward);
    }

    private void RecvGetReward(TurnRelayReqRewardResp resp)
    {
        if (resp.IsSuccess && resp.result)
        {
            //재화 UI 갱신
            ManagerUI._instance.SyncTopUIAssets();
            ManagerUI._instance.UpdateUI();

            //데이터 갱신
            ManagerTurnRelay.turnRelayCoop.SyncFromServerUserData();
            coopMissionData.state = ManagerTurnRelay.turnRelayCoop.GetCoopMission(coopMissionData.index).state;
            
            //UI 상태 갱신
            RefreshGetButton(coopMissionData.state);
            OpenPopupRewardGet(resp.clearReward);

            //콜백 호출
            UIPopupTurnRelay_Cooperation._instance.getRewardAction?.Invoke();

            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                       (int)coopMissionData.rewardData.type,
                       coopMissionData.rewardData.value,
                       ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TURN_RELAY_COOP_ACHIEVE_REWARD,
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TURN_RELAY_COOP_ACHIEVE_REWARD,
                       $"TURNRELAY_{ServerContents.TurnRelayEvent.eventIndex}_COA_IDX_{coopMissionData.index}"
                       );
        }
        isCanTouch = true;
    }

    private void OpenPopupRewardGet(AppliedRewardSet rewardSet)
    {
        //보상 팝업 띄우기
        if (rewardSet != null)
        {
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (Global._instance.GetString("n_m_1"),
                null,
                rewardSet);
        }
    }
}
