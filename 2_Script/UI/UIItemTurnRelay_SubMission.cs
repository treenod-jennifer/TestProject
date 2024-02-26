using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemTurnRelay_SubMission : MonoBehaviour
{
    //미션 내용
    [SerializeField] private UILabel labelMissionText;

    //보상
    [SerializeField] private GenericReward genericReward;

    //보상 받기 버튼/체크/받기전 텍스트  
    [SerializeField] private GameObject objBtnReward;
    [SerializeField] private BoxCollider collderBtnReward;
    [SerializeField] private UISprite spriteBtnReward;
    [SerializeField] private UILabel[] labelBtnReward;
    [SerializeField] private GameObject objCheck;
    [SerializeField] private GameObject objLabelRewardText;

    //프로그레스바
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel progressStep;

    //데이터
    private ManagerTurnRelay.SubmissionData item = null;

    //콜백
    private System.Action getRewardAction = null;

    private bool isCanTouch = true;

    public void InitItem(System.Action getRewardAction)
    {
        this.getRewardAction = getRewardAction;
    }

    public void UpdateData(ManagerTurnRelay.SubmissionData subMissionData)
    {
        item = subMissionData;
        genericReward.SetReward(item.reward);
        InitText();
        InitProgress();
        RefreshGetButton();
    }

    private void InitText()
    {
        labelMissionText.text = Global._instance.GetString(string.Format("tr_s_{0}", (int)item.type))
            .Replace("[n]", item.targetCount.ToString());
    }

    private void InitProgress()
    {
        //프로그레스 바 설정.
        float progressOffset = 100f / item.targetCount;
        progressBar.value = (item.progress * progressOffset) * 0.01f;

        //프로그레스 카운트 설정.
        progressStep.text = string.Format("{0}/{1}", item.progress, item.targetCount);
    }

    private void RefreshGetButton()
    {
        switch (item.state)
        {
            case ManagerTurnRelay.SubmissionState.INVALID_STATE:
            case ManagerTurnRelay.SubmissionState.INCOMPLETED:
                {
                    objBtnReward.SetActive(false);
                    objCheck.SetActive(false);
                    objLabelRewardText.SetActive(true);
                }
                break;
            case ManagerTurnRelay.SubmissionState.COMPLETED:
                {
                    objBtnReward.SetActive(true);
                    objCheck.SetActive(false);
                    objLabelRewardText.gameObject.SetActive(false);
                }
                break;
            case ManagerTurnRelay.SubmissionState.REWARD_FINISHED:
                {
                    objBtnReward.SetActive(false);
                    objCheck.SetActive(true);
                    objLabelRewardText.gameObject.SetActive(false);
                }
                break;
        }
    }

    private void OnClickBtnGet()
    {
        if (isCanTouch == false)
            return;
        isCanTouch = false;
        ServerAPI.TurnRelayReqSubmissionReward(item.idx, RecvGetReward);
    }

    private void RecvGetReward(TurnRelayReqRewardResp resp)
    {
        if (resp.IsSuccess && resp.result)
        {
            //재화 UI 갱신
            ManagerUI._instance.SyncTopUIAssets();
            ManagerUI._instance.UpdateUI();

            //데이터 갱신
            ManagerTurnRelay.turnRelaySubMission.SyncFromServerUserData();
            item.state = ManagerTurnRelay.turnRelaySubMission.GetSubmissionState(item.idx);

            //UI 상태 갱신
            RefreshGetButton();
            OpenPopupRewardGet(resp.clearReward);

            //콜백 호출
            getRewardAction.Invoke();

            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                       (int)item.reward.type,
                       item.reward.value,
                       ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TURN_RELAY_SUBMISSION_REWARD,
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TURN_RELAY_SUBMISSION_REWARD,
                       $"TURNRELAY_{ServerContents.TurnRelayEvent.eventIndex}_SMID_{item.idx}"
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
