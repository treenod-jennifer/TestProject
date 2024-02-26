using Protocol;
using ServiceSDK;
using UnityEngine;

public class InitializeCallbackImpl
{
    private string url = "";

    public InitializeCallbackImpl()
    {
        url = Application.absoluteURL;
        
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.deepLinkActivated += ApplicationOnDeepLinkActivated;
#endif
    }

    public void ResetUrl()
    {
        url = "";
    }
    
    private void ApplicationOnDeepLinkActivated(string deeplink)
    {
        if (ManagerData.promotionState == ManagerData.PromotionInitializeState.COMPLETE)
        {
            url = deeplink;
            SendDeeplink();
        }
    }
    
    public void InitializePromotionSDK()
    {
        if (ManagerData.promotionState == ManagerData.PromotionInitializeState.COMPLETE)
        {
            SendDeeplink();
            return;
        }
        else
        {
            ManagerData.promotionState = ManagerData.PromotionInitializeState.INITIALIZING;
            // Reward URL을 통해 앱이 실행되는 경우에는 initialize()를 호출하기 전에 sendTrackingDeeplink()를 호출
            // sendTrackingDeeplink()를 initialize() 전에 호출하면 initialize() 호출 시에 같이 전송
            SendDeeplink();
            ServiceSDKManager.instance.InitializePromotionSDK(onInitializeSuccess, onDeeplinkReceived);
        }
    }

    private void onInitializeSuccess(bool isSuccess)
    {
        if (isSuccess == false)
        {
            ManagerData.promotionState = ManagerData.PromotionInitializeState.NONE;
            return;
        }

        ManagerData.promotionState = ManagerData.PromotionInitializeState.COMPLETE;
    }

    public void SendDeeplink()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string deepLinkUrl = url;
        if (string.IsNullOrEmpty(deepLinkUrl) == false)
        {
            ServiceSDKManager.instance.SendDeeplink(deepLinkUrl);

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.LINK,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.REWARD_URL,
                deepLinkUrl,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }

#elif UNITY_IOS && !UNITY_EDITOR
        TreenodUnityBridge.instance.Initialize(deeplink =>
        {
            if (string.IsNullOrEmpty(deeplink) == false)
            {
                ServiceSDKManager.instance.SendDeeplink(deeplink);
                
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.LINK,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.REWARD_URL,
                    deeplink,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
                var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
            }
        });
#endif
    }

    private void onDeeplinkReceived()
    {
        if (ManagerData._instance != null &&
            ManagerData._instance._state >= DataLoadState.eUserLogin &&
            ManagerData.promotionState == ManagerData.PromotionInitializeState.COMPLETE)
        {
            RewardUrlTimeItemRequest();
        }
    }
    private void RewardUrlTimeItemRequest()
    {
        ServerAPI.RewardURL(RewardUrlTimeItemReceive);
    }

    private void RewardUrlTimeItemReceive(RewardUrlResp resp)
    {
        // 서버 통신 후 콜백.
        if (resp.IsSuccess)
        {
            // 즉시지급 되는 보상이 있을 경우에만 데이터 갱신 및 그로시 전송.
            if (resp.directTimeItems != null && resp.directTimeItems.Count > 0)
            {
                ServerRepos.SaveUserAsset(resp.userAsset);
                ServerRepos.SaveUserItemFreeTime(resp.userItemFreeTime);
                
                foreach (var reward in resp.directTimeItems)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)reward.type,
                        reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_REWARD_URL,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_REWARD_URL,
                        "RewardURL"
                    );  
                }
            }
            
            ServerRepos.MailCnt += resp.mailDeliveredItemCount;
            if (ManagerUI._instance != null)
            {
                ManagerUI._instance.UpdateUI();
            }
        }
    }
}