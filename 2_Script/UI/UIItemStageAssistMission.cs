using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemStageAssistMission : MonoBehaviour
{
    [SerializeField] private GenericReward reward;

    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel labelMaxCount;
    [SerializeField] private UILabel labelCurrentCount;

    [SerializeField] private UISprite sprMission;

    [SerializeField] private GameObject objAddBubble;

    private CdnStageAssistMission stageAssistMission = new CdnStageAssistMission();

    private enum MissionType
    {
        BLOCK_PINK = 1,
        BLOCK_BROWN = 2,
        BLOCK_YELLOW = 3,
        BLOCK_ORANGE =4 ,
        BLOCK_BLUE = 5,
        BOMB_LINE = 11,
        BOMB_CIRCLE = 12,
        BOMB_RANBOW = 13,
        BOMB_COMPOSE = 14,
    }

    /// <summary>
    /// ProgressBar Data 세팅, 연출이 필요한 경우 true
    /// </summary>
    /// <param name="isAction"></param>
    public void SetStageAssist(bool isAction = false)
    {
        int index = ServerRepos.UserStageAssistMissionEvent.missionIndex;

        if (ManagerStageAssistMissionEvent.IsStageAssistMissionClear())
            stageAssistMission = ServerContents.StageAssistMissionEventDetails[index - 1];
        else
            stageAssistMission = ServerContents.StageAssistMissionEventDetails[index];

        InitStageAssistMissionData(stageAssistMission);

        if (isAction)
            StartCoroutine(StartAction());
    }

    public void InitStageAssistMissionData(CdnStageAssistMission stageAssistMission)
    {
        this.stageAssistMission = stageAssistMission;

        reward.SetReward(stageAssistMission.reward);

        float maxCount = stageAssistMission.targetCount;
        float currentCount = ManagerStageAssistMissionEvent.currentMissionCount;

        progressBar.value = currentCount / maxCount;

        labelMaxCount.text = "/" + ((int)maxCount).ToString();
        labelCurrentCount.text = ((int)currentCount).ToString();

        sprMission.spriteName = $"{(MissionType)stageAssistMission.missionType}";
    }

    #region 컨티뉴창 연출

    public IEnumerator StartAction()
    {
        float maxCount = stageAssistMission.targetCount;
        float currentCount = ManagerStageAssistMissionEvent.currentMissionCount;
        float addCount = ManagerStageAssistMissionEvent.GetStageAssistMissionCount(ServerContents.StageAssistMissionEventDetails[ServerRepos.UserStageAssistMissionEvent.missionIndex].missionType);

        if(addCount + currentCount > maxCount)
        {
            addCount = maxCount;
        }
        else
        {
            addCount += currentCount;
        }

        if (objAddBubble != null)
        {
            if(addCount - currentCount > 0)
            {
                objAddBubble.SetActive(true);
                objAddBubble.GetComponentInChildren<UILabel>().text = $"+{addCount - currentCount}";
            }
            else
            {
                objAddBubble.SetActive(false);
            }
        }

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(CoProgressBarAction(addCount / maxCount, currentCount / maxCount));

        StartCoroutine(CoCountAction(addCount, currentCount));
    }

    private IEnumerator CoProgressBarAction(float target, float current)
    {
        float duration = 0.5f;
        float offset = (target - current) / duration;

        while (current < target)
        {
            current += offset * Time.deltaTime;
            progressBar.value = current;

            yield return null;
        }

        current = target;
        progressBar.value = current;
    }

    private IEnumerator CoCountAction(float target, float current)
    {
        float duration = 0.5f;
        float offset = (target - current) / duration;

        while (current < target)
        {
            current += offset * Time.deltaTime;
            labelCurrentCount.text = ((int)current).ToString();

            yield return null;
        }

        current = target;

        labelCurrentCount.text = ((int)current).ToString();
    }

    #endregion

    #region 클리어창 연출
    public IEnumerator StartAction_v2()
    {
        yield return StartCoroutine(CoStageAssistMissionAction());
    }

    private IEnumerator CoStageAssistMissionAction()
    {
        float maxCount = stageAssistMission.targetCount;

        float target = ManagerStageAssistMissionEvent.GetStageAssistMissionCount(ServerContents.StageAssistMissionEventDetails[ManagerStageAssistMissionEvent.currentMissionIndex].missionType);
        float current = ManagerStageAssistMissionEvent.currentMissionCount;
        float currentProgress = current / maxCount;

        if (target + current > maxCount)
        {
            target = maxCount;
        }
        else
        {
            target += current;
        }

        float duration = 0.5f;
        float offsetProgress = ((target - current) / maxCount) / duration;
        float offsetCount = (target - current) / duration;

        while (current < target)
        {
            current += offsetCount * Time.deltaTime;
            currentProgress += offsetProgress * Time.deltaTime;

            progressBar.value = currentProgress;
            labelCurrentCount.text = ((int)current).ToString();

            yield return null;
        }

        current = target;
        currentProgress = target / maxCount;

        labelCurrentCount.text = ((int)current).ToString();
        progressBar.value = currentProgress;

        ManagerStageAssistMissionEvent.currentMissionCount = ServerRepos.UserStageAssistMissionEvent.progress;
        ManagerStageAssistMissionEvent.currentMissionIndex = ServerRepos.UserStageAssistMissionEvent.missionIndex;
    }
    #endregion

    public void OnClickOpenStageAssistMission()
    {
        if (ManagerStageAssistMissionEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopUpStageAssistMissionEvent>();
        }
        else
        {
            string text = "";

            if (Global.LeftTime(ServerContents.StageAssistMissionEvent.endTs) <= 0)
            {
                text = Global._instance.GetString("n_ev_13");
            }
            else
            {
                text = Global._instance.GetString("n_ev_19");
            }

            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
            systemPopup.SortOrderSetting();
        }
    }
}