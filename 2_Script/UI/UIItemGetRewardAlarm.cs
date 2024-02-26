using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemGetRewardAlarm : MonoBehaviour
{
    [SerializeField] private GenericReward genericReward;
    [SerializeField] private GameObject objImmediateRoot;
    [SerializeField] private GameObject objMailBoxRoot;
    [SerializeField] private UILabel labelTotalCount;

    #region 폰트 컬러
    private Color timeColor = new Color(1f, 0.914f, 0f);
    private Color defaultColor = new Color(1f, 1f, 1f);
    #endregion

    /// <summary>
    /// 메일로 지급되는 형태의 보상 UI 초기화
    /// </summary>
    public void Init(Reward reward)
    {
        genericReward.countIncludeX = false;
        genericReward.SetReward(reward);
        bool isTimeLimitType = IsTimeLimitType(reward.type);
        Color fontColor = (isTimeLimitType == false) ? defaultColor : timeColor;

        //보상 카운트 폰트 컬러 설정
        genericReward.SetTextColor(fontColor);

        //보상 합계 UI 설정
        objImmediateRoot.SetActive(false);
        objMailBoxRoot.SetActive(true);
    }

    /// <summary>
    /// 즉시 지급되는 형태의 보상 UI 초기화
    /// </summary>
    public void Init(RewardType rType, Protocol.AppliedReward valueData)
    {
        Reward reward = new Reward { type = (int)rType, value = (int)valueData.valueDelta };
        genericReward.countIncludeX = false;
        genericReward.SetReward(reward);
        bool isTimeLimitType = IsTimeLimitType(reward.type);
        Color fontColor = (isTimeLimitType == false) ? defaultColor : timeColor;

        //보상 카운트 폰트 컬러 설정
        genericReward.SetTextColor(fontColor);

        //보상 합계 UI 설정
        objImmediateRoot.SetActive(true);
        objMailBoxRoot.SetActive(false);

        //전체 카운트 설정
        labelTotalCount.color = fontColor;
        if (isTimeLimitType == true)
            EndTsTimer.Run(labelTotalCount, valueData.valueFinal);
        else
            labelTotalCount.text = valueData.valueFinal.ToString();
    }

    private bool IsTimeLimitType(int type)
    {
        RewardType rType = (RewardType)type;
        if (rType == RewardType.cloverFreeTime || rType == RewardType.wingFreetime || rType == RewardType.readyItem3_Time
           || rType == RewardType.readyItem4_Time || rType == RewardType.readyItem5_Time
           || rType == RewardType.readyItem6_Time || rType == RewardType.readyItemBomb_Time)
            return true;
        return false;
    }
}
