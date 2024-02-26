using System.Collections;
using UnityEngine;

public class NoticeHelper
{
    private static bool OnNewsIndex => ServerRepos.LoginCdn.isNewsIndex != 0;

    public static IEnumerator CoShowNotice()
    {
        if (OnNewsIndex)
        {
            yield return CoShowNewsIndexAndInterstitial();
        }
    }

    private static IEnumerator CoShowNewsIndexAndInterstitial()
    {
#if! UNITY_EDITOR
        string selectLanguage = GetCountryCode();

        TriggerCustomCallBackHelper helper = new TriggerCustomCallBackHelper();

        // Interstitial 출력
        bool   isShowInterstitialsEnd  = false;
        string interstitialPlacementId = "Interstitials_" + selectLanguage;

        helper.PromotionTriggerChannelCallbackReset();
        helper.PromotionTriggerChannelCallBack(interstitialPlacementId, (code, message) =>
        {
            //1: 표시할 웹 뷰가 없음, 1002: WebView 표시 해제 성공, 1001: WebView 표시 성공
            if (code != 1001)
            {
                isShowInterstitialsEnd = true;
            }
        });

        yield return new WaitUntil(() => isShowInterstitialsEnd);

        // NewsIndex 출력 여부
        bool isNewsIndexOn       = false;
        bool isNewsIndexCountEnd = false;
        helper.statusCallback = (int contentsTotal, int contentsNew, int contentsUnread) =>
        {
            //total이 하나라도 있으면 newsIndex 띄우도록
            isNewsIndexOn         = contentsTotal > 0;
            isNewsIndexCountEnd   = true;
            helper.statusCallback = null;
        };
        helper.errorMessageCallBack = (long errorCode, string errorMessage) =>
        {
            //에러가 콜백되어도 정상적으로 게임 진입이 되도록.
            isNewsIndexOn               = false;
            isNewsIndexCountEnd         = true;
            helper.errorMessageCallBack = null;
        };

        string newsIndexPlacementId = "NewsIndexes_" + selectLanguage;
        helper.GetPromotionStatus(newsIndexPlacementId);

        yield return new WaitUntil(() => isNewsIndexCountEnd);
        helper.statusCallback       = null;
        helper.errorMessageCallBack = null;

        // NewsIndex 출력
        if (isNewsIndexOn)
        {
            bool isShowNewsIndexEnd = false;

            helper.PromotionTriggerChannelCallbackReset();
            helper.PromotionTriggerChannelCallBack(newsIndexPlacementId, (long code, string message) =>
            {
                //1: 표시할 웹 뷰가 없음, 1002: WebView 표시 해제 성공, 1001: WebView 표시 성공
                if (code != 1001)
                {
                    isShowNewsIndexEnd = true;
                }
            });

            yield return new WaitUntil(() => isShowNewsIndexEnd);
        }
#else
        yield return null;
#endif
    }
    
    private static string GetCountryCode()
    {
        string country = LanguageUtility.SystemCountryCode;
        if (string.Equals(country, "kr"))
        {
            return "jp";
        }
        
        return country;
    }
}
