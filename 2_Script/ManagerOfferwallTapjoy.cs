using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TapjoyUnity;

public class ManagerOfferwallTapjoy : MonoBehaviour, IEventBase
{
    public static ManagerOfferwallTapjoy instance = null;

    #region const
    private const string ANDROID_OFFERWALL_PLACEMENT_NAME = "PokopangTown_Android";
    private const string IOS_OFFERWALL_PLACEMENT_NAME     = "Pokopangtown_iOS";
    private const string ANDROID_SDK_KEY                  = "YGj0jeymR_GRITJi-XDTfgECiRML1561O7NABV5ybIIetL-N47ikfS6LYsiX";
    private const string IOS_SDK_KEY                      = "q_liA6iIRRqko2UFEcP6RwEBRmAzdvbmPDNbvvSfCIUMEj_9esH1RXH1Lk34";
    private const string USER_ID_KEY_NAME                 = "TJC_OPTION_USER_ID";
    #endregion

    #region private
    private TJPlacement _placement;
    #endregion

    private Action _closeCallback = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Init()
    {
        if (!CheckStartable())
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerOfferwallTapjoy>();

#if !UNITY_EDITOR
        // 탭조이 서버 연결
        ManagerOfferwallTapjoy.instance.Connect();
        
        // 리얼 환경 이 외에는 디버그 모드 설정
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
        {
            Tapjoy.SetDebugEnabled(true);
        }
#endif
        ManagerEvent.instance.RegisterEvent(instance);
    }

    private static bool CheckStartable()
    {
        if (ServerRepos.LoginCdn.tapJoyOfferWall <= 0)
        {
            return false;
        }

        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
        {
            return false;
        }

        return true;
    }

    GameEventType IEventBase.GetEventType() => GameEventType.OFFERWALL_TAPJOY;

    public void OnReboot()
    {
        if (instance == null)
        {
            return;
        }

        InitHandler();

        Destroy(instance);
    }

    /// <summary>
    /// 탭조이 서버 연결 함수 입니다.
    /// </summary>
    public void Connect()
    {
        Debug.Log("++++ Offerwall Connect");

        Tapjoy.OnConnectSuccess += HandleConnectSuccess;
        Tapjoy.OnConnectFailure += HandleConnectFailure;

        var uid          = SDKGameProfileManager._instance.GetMyProfile()._userKey;
        var crypt        = ServerSecure.CryptJsonStringWithCustomKey(uid, ServerContents.OfferwallSalt, true);
        var connectFlags = new Dictionary<string, object> { { USER_ID_KEY_NAME, crypt } };

        Tapjoy.IsConnected = false;

#if UNITY_ANDROID
        Tapjoy.Connect(ANDROID_SDK_KEY, connectFlags);
#elif UNITY_IOS
        Tapjoy.Connect( IOS_SDK_KEY, connectFlags );
#endif
    }

    /// <summary>
    /// 초기화 함수 입니다.
    /// ConnectSuccess 콜백에서 호출
    /// </summary>
    public void InitTapJoy()
    {
#if UNITY_ANDROID
        _placement = TJPlacement.CreatePlacement(ANDROID_OFFERWALL_PLACEMENT_NAME);
#elif UNITY_IOS
        _placement = TJPlacement.CreatePlacement( IOS_OFFERWALL_PLACEMENT_NAME );
#endif
        TJPlacement.OnRequestSuccess += HandlePlacementRequestSuccess;
        TJPlacement.OnRequestFailure += HandlePlacementRequestFailure;
        TJPlacement.OnContentReady   += HandlePlacementContentReady;
        TJPlacement.OnContentShow    += HandlePlacementContentShow;
        TJPlacement.OnContentDismiss += HandlePlacementContentDismiss;
        TJPlacement.OnClick          += HandlePlacementClick;

        Request();
    }

    private void CloseCallBack()
    {
        _closeCallback?.Invoke();
        _closeCallback = null;
    }

    public void ShowOfferWall_Tapjoy(Action callback = null)
    {
        _closeCallback = callback;

#if !UNITY_EDITOR
        // 오퍼월 광고 요청
        ShowAd();
        UIPopupMailBox._instance.SetRefreshButtonState(true);
        CloseCallBack();
#else
        UIPopupMailBox._instance.SetRefreshButtonState(true);
        CloseCallBack();
#endif

        // 배너 선택으로 그로시 로그 전송
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

    /// <summary>
    /// 광고 요청 리퀘스트입니다.
    /// </summary>
    private void Request()
    {
        Debug.Log("++++ Offerwall Request");

        if (Tapjoy.IsConnected)
        {
            if (_placement != null)
            {
                _placement.RequestContent();
            }
        }
        else
        {
            Debug.LogWarning("Tapjoy SDK must be connected before you can request content.");
        }
    }

    /// <summary>
    /// 광고보기(오퍼월페이지 출력) 메서드 입니다.
    /// </summary>
    private void ShowAd()
    {
        Debug.Log("++++ Offerwall ShowAd");

        if (_placement == null)
        {
            Debug.Log("++++ placement is null");
            return;
        }

        if (_placement.IsContentReady())
        {
            _placement.ShowContent();
        }
        else
        {
            Debug.Log("표시 할 콘텐츠가 없거나 아직 다운로드되지 않은 상황을 처리합니다");
        }
    }

    public static bool IsOfferWallReady() => Tapjoy.IsConnected;

    public void ReConnectOfferWall(System.Action callback = null)
    {
        if (Tapjoy.IsConnected)
        {
            return;
        }

        Tapjoy.Connect();

        callback?.Invoke();
    }

    private void InitHandler()
    {
        Tapjoy.OnConnectSuccess      -= HandleConnectSuccess;
        Tapjoy.OnConnectFailure      -= HandleConnectFailure;
        TJPlacement.OnRequestSuccess -= HandlePlacementRequestSuccess;
        TJPlacement.OnRequestFailure -= HandlePlacementRequestFailure;
        TJPlacement.OnContentReady   -= HandlePlacementContentReady;
        TJPlacement.OnContentShow    -= HandlePlacementContentShow;
        TJPlacement.OnContentDismiss -= HandlePlacementContentDismiss;
        TJPlacement.OnClick          -= HandlePlacementClick;
    }

    #region callback
    /// <summary>
    /// 광고 요청 성공 콜백입니다.
    /// </summary>
    /// <param name="placement"></param>
    public void HandlePlacementRequestSuccess(TJPlacement placement) => Debug.Log("HandlePlacementRequestSuccess");

    /// <summary>
    /// 광고 요청 실패 콜백입니다.
    /// </summary>
    /// <param name="placement"></param>
    /// <param name="error"></param>
    public void HandlePlacementRequestFailure(TJPlacement placement, string error) => Debug.Log("HandlePlacementRequestFailure");

    public void HandlePlacementContentReady(TJPlacement placement) => Debug.Log("HandlePlacementContentReady");

    /// <summary>
    /// 광고 출력 콜백입니다.
    /// </summary>
    /// <param name="placement"></param>
    public void HandlePlacementContentShow(TJPlacement placement) => Debug.Log("HandlePlacementContentShow");

    /// <summary>
    /// 광고 종료 콜백입니다.
    /// </summary>
    /// <param name="placement"></param>
    public void HandlePlacementContentDismiss(TJPlacement placement)
    {
        Debug.Log("HandlePlacementContentDismiss");

        Request();
    }

    /// <summary>
    /// 클릭 콜백입니다.
    /// </summary>
    /// <param name="placement"></param>
    public void HandlePlacementClick(TJPlacement placement) => Debug.Log("HandlePlacementClick");

    /// <summary>
    /// 탭조이 서버 연결 실패 콜백입니다.
    /// </summary>
    public void HandleConnectFailure() => Debug.Log($"Connect Failure");

    /// <summary>
    /// 탭조이 서버 연결 콜백입니다.
    /// </summary>
    public void HandleConnectSuccess()
    {
        Debug.Log("Connect Success");

#if !UNITY_EDITOR
        InitTapJoy();
#endif
    }
    #endregion

    #region 미사용 함수
    void IEventBase.OnLobbyStart()
    {
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    bool IEventBase.OnTutorialCheck() => false;

    bool IEventBase.OnLobbySceneCheck() => false;

    IEnumerator IEventBase.OnPostLobbyEnter() => null;

    IEnumerator IEventBase.OnLobbyObjectLoadPhase() => null;

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland() => null;

    IEnumerator IEventBase.OnTutorialPhase() => null;

    IEnumerator IEventBase.OnLobbyScenePhase() => null;

    IEnumerator IEventBase.OnRewardPhase() => null;

    void IEventBase.OnIconPhase()
    {
    }
    #endregion
}