using System;
using ServiceSDK;
using Trident;
using UnityEngine;

public class TriggerCustomCallBackHelper
{
    public Action<long, string>  errorMessageCallBack;
    public Action<int, int, int> statusCallback;
    
    // NewsIndex, Interstitial 호출 시 사용.
    public void PromotionTriggerChannelCallBack(string triggerId, Action<long, string> onComplete)
    {
        if (!Application.isEditor)
        {
            ServiceSDKManager.instance.TriggerChannel(triggerId, onComplete);
        }
    }

    public void PromotionTriggerChannelCallbackReset()
    {
        ServiceSDKManager.instance.TriggerChannelCallbackReset();
    }
    
    // 게시글이 있는지 조회를 위해 사용하는 콜백.
    public void GetPromotionStatus(string triggerId)
    {
        if (NetworkSettings.Instance.IsRealDevice() == true)
        {
            ServiceSDKManager.instance.GetPromotionStatus(triggerId, (isSuccess, eventCode, message, list) =>
            {
                if (isSuccess)
                {
                    onSuccess(list);
                }
                else
                {
                    onError(eventCode, message);
                }
            });
        }
    }

    // GetPromotionStatus 호출 성공 콜백
    // 전체 게시글 갯수 조회 후 statusCallback  호출
    private void onSuccess(PromotionStatusList promotionStatusList)
    {
        if (statusCallback == null)
            return;
        
        int contentsTotal  = 0;
        int contentsNew    = 0;
        int contentsUnread = 0;
                    
        foreach (var item in promotionStatusList.getNewsIndexes())
        {
            contentsTotal  += item.getContentCountOfTotal();
            contentsNew    += item.getContentCountOfNew();
            contentsUnread += item.getContentCountOfUnreadNew();
        }
        
        statusCallback(contentsTotal, contentsNew, contentsUnread);
    }

    // GetPromotionStatus 호출 에러 콜백
    private void onError(long errorCode, string errorMessage)
    {
        if (errorMessageCallBack != null)
        {
            errorMessageCallBack(errorCode, errorMessage);
        }
    }
}