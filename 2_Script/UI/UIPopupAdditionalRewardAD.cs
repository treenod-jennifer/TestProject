using UnityEngine;

/// <summary>
/// 광고 시청 후 보상 2배 획득 가능 팝업 (X 버튼 클릭 시 기본 보상 획득)
/// </summary>
public class UIPopupAdditionalRewardAD : UIPopupBase
{
    [SerializeField] private UILabel[] labelsTitle;
    [SerializeField] private UILabel labelText;
    [SerializeField] private GenericReward reward;

    // 광고 시청하지 않고 일반 보상 획득.
    private System.Action getBasicRewardRequest = null;
    // 광고 시청 후 2배 보상 획득.
    private System.Action getAdRewardRequest = null;

    private AdManager.AdType _adType;

    private int growthyNum1 = 0;

    public void InitData(AdManager.AdType adType, string titleKey, string messageKey, Reward rewardData, System.Action getBasicRewardRequest,
        System.Action getAdRewardRequest, int num1 = 0)
    {
        _adType = adType;
        growthyNum1 = num1;
        this.getBasicRewardRequest = getBasicRewardRequest;
        this.getAdRewardRequest = getAdRewardRequest;
        
        labelsTitle.SetText(Global._instance.GetString(titleKey));
        labelText.text = Global._instance.GetString(messageKey);
        reward.SetReward(rewardData);
    }

    private void OnClickReward()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        
        ClosePopUp();
        getBasicRewardRequest?.Invoke();
    }

    public override void OnClickBtnBack()
    {
        OnClickReward();
    }
    
    private void OnClickADReward()
    {
        if (!bCanTouch)
            return;
        bCanTouch = false;
        
        switch (_adType)
        {
            case AdManager.AdType.AD_20:
            {
                AdManager.ShowAD_ReqAdLoginBonusRenewal(AdManager.AdType.AD_20, growthyNum1, (isSuccess) =>
                {
                    if (!isSuccess)
                    {
                        bCanTouch = true;
                        return;
                    }
                    else
                    {
                        getAdRewardRequest?.Invoke();
                        ClosePopUp();
                    }
                });
                break;
            }
        }
    }
}