using System.Collections;
using Protocol;
using ServiceSDK;
using UnityEngine;

public class ConnectingProcess
{
    private ProcessManager         processManager;
    private BaseConnectingStrategy strategy;
    
    public ConnectingProcess(ProcessManager processManager, BaseConnectingStrategy strategy)
    {
        this.processManager = processManager;
        this.strategy       = strategy;
    }

    public IEnumerator CoProcess()
    {
        if (processManager == null || strategy == null)
        {
            yield break;
        }

        strategy.RegisterDeviceTokenForPushService();
        
        yield return processManager.StartCoroutine(strategy.CoInitializeProfileManager());
        
        yield return processManager.StartCoroutine(strategy.CoAntiTamperingInLiapp());
        
        strategy.SetGrowthyIdAndTrackEvent();
        
        strategy.InitializeBilling();
        
        ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.SDK_PROFILE_S);
        yield return processManager.StartCoroutine(strategy.CoSetUserProfile());
        ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.SDK_PROFILE_E);
        
        ManagerData._instance._state = DataLoadState.eLineUserData;
        
        if (ServiceSDKManager.instance.IsGuestLogin() == false)
        {
            ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.SDK_GET_FRIENDS_S);
            yield return processManager.StartCoroutine(strategy.CoSetGameFriends());
            yield return processManager.StartCoroutine(strategy.CoSetNonGameFriends());
            ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.SDK_GET_FRIENDS_E);
        }
        
        yield return processManager.StartCoroutine(strategy.CoSetAllProfile());
        
        ManagerData._instance._state = DataLoadState.eLineProfileData;
        
        yield return processManager.StartCoroutine(strategy.CoShowMinorNotice());
        
        strategy.InitializePromotion();
        
        InitializeAdManager();

        ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.GAME_LOGIN_S);
        yield return processManager.StartCoroutine(CoRequestLogin());
        yield return processManager.StartCoroutine(strategy.CoGetPurchasesList());
        yield return processManager.StartCoroutine(strategy.CoGuestMigration());
        ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.GAME_LOGIN_E);
    }
    
    /// <summary>
    /// Login API 호출
    /// </summary>
    private IEnumerator CoRequestLogin()
    {
        var data = GetRequestLoginData();
        ServerAPI.Login(data, OnRequestLoginComplete, UIPopupServerSelect.dcServerNumber, UIPopupServerSelect.targetUID);
        
        yield return new WaitUntil(() =>ManagerData._instance != null && ManagerData._instance._state == DataLoadState.eUserLogin);
    }
    
    private LoginReq GetRequestLoginData()
    {
        var myProfile      = SDKGameProfileManager._instance.GetMyProfile();
        var tridentProfile = myProfile.GetTridentProfile();
        var data = new LoginReq()
        {
            authProvider = (int)ServiceSDKManager.instance.GetProvideType(),
            userKey      = myProfile._userKey, 
            token        = myProfile.token,
            providerKey  = (tridentProfile?.providerKey ?? "")
        };

        return data;
    }

    private void OnRequestLoginComplete(BaseResp code)
    {
        if (code.IsSuccess)
        {
            Global.join = ServerRepos.User.loginTs == 0;
            ManagerData._instance._state = DataLoadState.eUserLogin;
        }
        else
        {
            // TODO: FAILED DIALOG
        }
    }
    
    /// <summary>
    /// AppLovin 어댑터 초기화
    /// </summary>
    private void InitializeAdManager()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        AdManager.Initialize(myProfile._userKey);
    }
}
