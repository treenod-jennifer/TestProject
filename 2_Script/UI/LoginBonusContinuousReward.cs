using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로그인 이벤트 개선 연속 보상 아이템
/// </summary>
public class LoginBonusContinuousReward : MonoBehaviour
{
    [SerializeField] private List<GenericReward> genericReward;
    [SerializeField] private UIGrid grid;

    [SerializeField] private UISprite[] sprites;

    [SerializeField] private UISprite titleSprite;
    [SerializeField] private UISprite frameSprite;
    [SerializeField] private UISprite lineSprite;
    [SerializeField] private UISprite completeSprite;
    [SerializeField] private UISprite lockSprite;
    [SerializeField] private UILabel stepLabel;
    [SerializeField] private GameObject todayFrame;
    [SerializeField] private GameObject rewardRoot;
    [SerializeField] private GameObject getButton;

    private int index = 0;                // 보상 인덱스.
    private bool canGetReward = false;    // 보상 획득 가능한 상태인지.
    private bool getReward = false;       // 보상을 이미 획득한 상태인지.

    /// <summary>
    /// 연속 보상 아이템 셋팅.
    /// </summary>
    /// <param name="index">보상 인덱스(1~7)</param>
    /// <param name="today">유저의 현재 연속 접속일 수</param>
    /// <param name="mostContinuousCount">유저의 최대 연속 접속일 수</param>
    /// <param name="getReward">보상을 획득한 상태인지 여부</param>
    /// <param name="rewards">보상 정보</param>
    public void InitData(int index, int today, int mostContinuousCount, bool getReward, List<Reward> rewards)
    {
        SetAtlas();

        this.index = index;
        this.getReward = getReward;
        canGetReward = !getReward && index <= mostContinuousCount;

        // 4, 7번 보상의 경우 보상 갯수가 조절될 수 있어 예외 처리. 
        for (int i = 0; i < genericReward.Count; i++)
        {
            if (i < rewards.Count)
            {
                genericReward[i].SetReward(rewards[i]);
            }
            else
            {
                genericReward[i].gameObject.SetActive(false);
            }
        }
        if (grid != null)
        {
            grid.enabled = true;
        }

        todayFrame.SetActive(today == index);
        stepLabel.text = Global._instance.GetString("p_lb_5").Replace("[n]", index.ToString());

        UpdateUI();
    }

    private void SetAtlas()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].atlas = ManagerLoginBonusRenewal.Instance.atlas;
        }
    }

    private void UpdateUI()
    {
        if (getReward)
        {
            completeSprite.gameObject.SetActive(true);
            rewardRoot.SetActive(false);
        }
        else
        {
            completeSprite.gameObject.SetActive(false);
            rewardRoot.SetActive(true);

            getButton.SetActive(canGetReward);
            lockSprite.gameObject.SetActive(!canGetReward);
        }

        UpdateColor();
    }

    // 보상 상태에 따라 컬러값 변경(마지막 보상 제외).
    private void UpdateColor()
    {
        if (index < 7)
        {
            if (getReward)
            {
                titleSprite.color = new Color32(122, 185, 225, 255);
                frameSprite.color = new Color32(211, 225, 234, 255);
                lineSprite.color = new Color32(179, 212, 233, 255);
            }
            else
            {
                titleSprite.color = new Color32(122, 185, 225, 255);
                frameSprite.color = new Color32(178, 217, 242, 255);
                lineSprite.color = new Color32(122, 185, 225, 255);
            }
        }
    }

    // Get 버튼 클릭.
    public void OnClickBtnGet()
    {
        if (!canGetReward) return;

        UIPopupLoginBonusRenewal.instance.RequestGetReward(UIPopupLoginBonusRenewal.LoginBonusRewardType.CONTINUOUS,
            index, OnGetRewardComplete);
    }

    // 보상 획득 완료 후 UI 갱신.
    private void OnGetRewardComplete()
    {
        canGetReward = false;
        getReward = true;
        UpdateUI();
    }
}