using Newtonsoft.Json;
using UnityEngine;
using ServiceSDK;

public class UIItemAntiqueStoreItem : MonoBehaviour
{
    [SerializeField] private GenericReward reward;
    [SerializeField] private UITexture texRewardComplete;
    [SerializeField] private GameObject objCompleteState;
    [SerializeField] private UITexture texRewardShadow;
    [SerializeField] private UILabel[] labelDecoPrice;
    [SerializeField] private GameObject objHit;
    [SerializeField] private UISprite sprTier;

    private HousingList _housingList = null;
    
    public void UpdateData(HousingList housingItem)
    {
        _housingList = housingItem;
        
        gameObject.SetActive(_housingList != null);
        if(_housingList == null) return;

        Reward _reward = new Reward();
        _reward.type = (int)RewardType.housing;
        _reward.value =  ManagerAntiqueStore.instance.GetHousingValue(_housingList.housingIndex, _housingList.modelIndex);
        
        reward.scale = 2.15f;
        reward.rewardIcon_T.SuccessEvent +=
            () => {
                texRewardShadow.mainTexture = reward.rewardIcon_T.mainTexture;
                texRewardComplete.mainTexture = reward.rewardIcon_T.mainTexture;
            };
        
        reward.SetReward(_reward);

        objCompleteState.SetActive(ManagerAntiqueStore.instance.IsBuyHousing(_housingList.idx));
        
        labelDecoPrice.SetText(_housingList.price.ToString());
        objHit.SetActive(_housingList.hotLabel == 1);

        sprTier.spriteName = $"AntiqueStore_e_Price{_housingList.tier}";
    }
    
    private void OnClickBuyAntiqueItem()
    {
        //해당 상품이 구매가 완료가 되었는지 검사.
        if (ManagerAntiqueStore.instance.IsBuyHousing(_housingList.idx)) return;
        
        //구매 팝업
        ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
        {
            popup.SortOrderSetting();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_21").Replace("[n]", $"{_housingList.price}"), false);
            popup.SetImage(reward.rewardIcon_T.mainTexture as Texture2D);
            popup.SetImageSize(128, 128);
            popup.FunctionSetting(1, "GetAntiqueStoreReward", gameObject, true);
        });
    }
    
    private void GetAntiqueStoreReward()
    {
        //교환 재화가 부족할 때
        if (_housingList.price > ServerRepos.UserAntiqueStore.assetAmount)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_22"), false);
                popup.SetImage(reward.rewardIcon_T.mainTexture as Texture2D);
                popup.SetImageSize(128, 128);
            });
        }
        else
        {
            ServerAPI.AntiqueStoreGetReward(_housingList.idx, resp =>
            {
                if (resp.IsSuccess)
                {
                    //그로시
                    {
                        var growthyMoney = new GrowthyCustomLog_Money
                        (
                            GrowthyCustomLog_Money.Code_L_TAG.AS,
                            GrowthyCustomLog_Money.Code_L_MRSN.U_ANTIQUE_STORE,
                            0,
                            -_housingList.price,
                            0,
                            ServerRepos.UserAntiqueStore.assetAmount,
                            $"{ServerContents.AntiqueStore.eventIndex}_ANTIQUE_STORE"
                        );
                        var docMoney = JsonConvert.SerializeObject(growthyMoney);
                        ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                        
                        // 구매한 아이템 기록
                        GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.housing,
                            ManagerAntiqueStore.instance.GetHousingValue(_housingList.housingIndex, _housingList.modelIndex),
                            GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                            GrowthyCustomLog_ITEM.Code_L_RSN.G_ANTIQUE_STORE,
                            $"{ServerContents.AntiqueStore.eventIndex}_ANTIQUE_STORE"
                        );
                    }
                    
                    UIPopupAntiqueStore._instance.PostBuyAntiqueItem();
                    ManagerAntiqueStore.instance.SyncUserToken();
                    ManagerUI._instance.UpdateUI();
                    
                    //통합 보상창
                    ManagerUI._instance.OpenPopupGetRewardAlarm (Global._instance.GetString("n_wrk_12"), null, resp.reward);
                }
            });
        }
        
        
    }
}