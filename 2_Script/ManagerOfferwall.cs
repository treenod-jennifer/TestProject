using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gpm.WebView;

public class ManagerOfferwall : MonoBehaviour, IEventBase
{
    public static ManagerOfferwall instance = null;
    
    private Action closeCallback = null;

    private static string owUrl;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    
    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerOfferwall>();
        
        if (instance == null)
            return;
        
        ManagerEvent.instance.RegisterEvent(instance);
    }

    private static bool CheckStartable()
    {
        if (ServerContents.Offerwall == null || ServerContents.Offerwall.isOnEvent <= 0 || string.IsNullOrEmpty(ServerContents.Offerwall.salt))
            return false;
        
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
            return false;
        
        SetUrl();
        if(string.IsNullOrEmpty(owUrl))
            return false;
        
        return true;
    }

    private static void SetUrl()
    {
#if UNITY_IOS
        owUrl = ServerContents.Offerwall.owUrlIos;
#elif UNITY_ANDROID
        owUrl = ServerContents.Offerwall.owUrlAos;
#endif
        
        //유저 키 암호화
        var doc = SDKGameProfileManager._instance.GetMyProfile()._userKey;
        if (string.IsNullOrEmpty(owUrl) || string.IsNullOrEmpty(doc))
        {
            owUrl = null;
            return;
        }
        
        string str = ServerSecure.CryptJsonStringWithCustomKey(doc, ServerContents.Offerwall.salt, true);
        if (string.IsNullOrEmpty(str))
        {
            owUrl = null;
            return;
        }
        
        owUrl += "&suid=";
        owUrl += str;
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
        {
            owUrl = null;
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.OFFERWALL;
    }

    private void CloseCallBack()
    {
        closeCallback?.Invoke();
        closeCallback = null;
    }

    public void ShowOW(Action callback = null)
    {
        closeCallback = callback;

        if (string.IsNullOrEmpty(owUrl))
        {
            CloseCallBack();
            return;
        }

#if UNITY_EDITOR
        Application.OpenURL(owUrl);
        OnCloseCallback(null);
#else
        //웹뷰 출력
        GpmWebView.ShowUrl(owUrl, new GpmWebViewRequest.Configuration()
        {
            style = GpmWebViewStyle.FULLSCREEN,
            orientation = GpmOrientation.PORTRAIT,
            backgroundColor = "#FFFFFF",
            isNavigationBarVisible = true,
            navigationBarColor = "#faab9b",
            supportMultipleWindows = true,
            
#if UNITY_IOS
            contentMode = GpmWebViewContentMode.MOBILE
#endif
        }, null, OnCloseCallback, null, null);
        
#endif
        // 배너 선택으로 url 웹 브라우저 오픈 시 그로시 로그 전송
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.OFFERWALL,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.OFFERWALL_SELECT,
            null,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }

    private void OnCloseCallback(GpmWebViewError error)
    {
        Screen.orientation = ScreenOrientation.Portrait;
        
        if (error == null)
        {
            ServerAPI.Inbox(UIPopupMailBox._instance.OnRefreshReceived, 0, (int)UIPopupMailBox.tapType);
        }

        CloseCallBack();
    }

    #region 미사용 함수

    
    void IEventBase.OnLobbyStart()
    {
    }
    
    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }


    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield return null;
    }

    void IEventBase.OnIconPhase()
    {
        
    }

    #endregion
}
