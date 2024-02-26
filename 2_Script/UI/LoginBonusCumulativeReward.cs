using UnityEngine;

/// <summary>
/// 로그인 이벤트 개선 누적 보상 아이템
/// </summary>
public class LoginBonusCumulativeReward : MonoBehaviour
{
    // 누적 보상 UI 타입(보드, 말풍선)
    public enum CumulativeRewardType
    {
        BOARD,
        BUBBLE,
    }

    [SerializeField] private GenericReward genericReward;

    [SerializeField] private UISprite[] sprites;

    [SerializeField] private UISprite stepSprite;
    [SerializeField] private UILabel stepLabel;

    [SerializeField] private GameObject rewardLabelObj;
    [SerializeField] private GameObject effectObj;
    [SerializeField] private GameObject checkObj;
    [SerializeField] private GameObject getButtonObj;

    public Transform boniPosition;
    public CumulativeRewardType rewardType;

    private int index;                    // 보상 인덱스.
    private int targetCount;              // 보상 획득 가능한 누적일 수.
    private bool canGetReward = false;    // 보상 획득 가능한 상태인지.
    private bool adAvailable = false;     // 광고 시청 가능한지.
    private Reward reward;                // 보상 정보.

    /// <summary>
    /// 누적 보상 아이템 셋팅.
    /// </summary>
    /// <param name="index">보상 인덱스(1~4)</param>
    /// <param name="targetCount">보상 획득 가능 누적 접속일 수</param>
    /// <param name="currentCount">유저의 현재 누적 접속일 수</param>
    /// <param name="getReward">보상을 획득한 상태인지 여부</param>
    /// <param name="adAvailable">광고 시청이 가능한 보상인지 여부</param>
    /// <param name="reward">보상 정보</param>
    public void InitData(int index, int targetCount, int currentCount, bool getReward, bool adAvailable, Reward reward)
    {
        SetAtlas();

        this.index = index;
        this.adAvailable = adAvailable && targetCount == currentCount;
        this.targetCount = targetCount;
        this.reward = reward;
        canGetReward = !getReward && targetCount <= currentCount;

        stepLabel.text = targetCount.ToString();
        genericReward.SetReward(reward);
        getButtonObj.SetActive(canGetReward);
        
        if (getReward)
        {
            SettingRewardGetUI();
        }
    }

    public void SetEffect(bool enable)
    {
        if (effectObj != null)
        {
            effectObj.SetActive(enable);
        }
    }

    private void SetAtlas()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].atlas = ManagerLoginBonusRenewal.Instance.atlas;
        }
    }

    // 보상 획득 상태로 UI 셋팅.
    private void SettingRewardGetUI()
    {
        genericReward.SetColor(new Color32(160, 160, 160, 255));
        if (rewardType == CumulativeRewardType.BOARD && stepSprite != null)
        {
            stepSprite.color = new Color32(160, 160, 160, 255);
        }

        rewardLabelObj.SetActive(false);

        if (checkObj != null)
        {
            checkObj.SetActive(true);
        }
    }

    // Get 버튼 클릭.
    public void OnClickBtnGet()
    {
        if (!canGetReward) return;
        
        if (adAvailable && 
            ServerContents.AdInfos != null && ServerContents.AdInfos.ContainsKey((int)AdManager.AdType.AD_20))
        {
            ShowAdPopup();
        }
        else
        {
            RequestBasicReward();
        }
    }

    // 누적 보상 2배 획득 광고 팝업 출력.
    private void ShowAdPopup()
    {
        ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupAdditionalRewardAD>((popup) =>
            popup.InitData(AdManager.AdType.AD_20, "lb_bonus_1", "lb_bonus_2", reward, 
                RequestBasicReward, RequestAdReward, index));
    }

    // 일반 보상 획득.
    private void RequestBasicReward()
    {
        UIPopupLoginBonusRenewal.instance.RequestGetReward(UIPopupLoginBonusRenewal.LoginBonusRewardType.CUMULATIVE,
            index, OnGetRewardComplete, targetCount);
    }

    // 광고 시청 보상 획득.
    private void RequestAdReward()
    {
        UIPopupLoginBonusRenewal.instance.RequestGetAdReward(index, targetCount, OnGetRewardComplete);
    }

    // 보상 획득 완료 후 UI 갱신.
    private void OnGetRewardComplete()
    {
        canGetReward = false;
        SettingRewardGetUI();
        getButtonObj.SetActive(false);
    }
}