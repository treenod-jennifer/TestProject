using UnityEngine;
using System;
using System.Collections;
using Trident;

namespace ServiceSDK
{
    public partial class ServiceSDKManager : MonoSingletonManager<ServiceSDKManager>
    {
        public string existingUserKey = "";

        private TridentSDK           trident;
        private TridentConfiguration tridentConfig;
        private NotificationAppInfo  appInfo;

        private AuthManager.AuthUserDataMigrationHandlerCallbackDelegate userDataMigrationHandlerCallback;

        public static bool isRebootSignIn       = false;
        public static bool isDuplicateLogin     = false;

        
        public string GetPlatformVer()
        {
            using (TridentSDK tr = TridentSDK.getInstance() )
            {
                var deviceDetail = tr?.getDeviceDetails() ?? null;
                if (deviceDetail == null)
                {
                    return "";
                }

                return deviceDetail.getPlatformVersion();
            }
        }

        public string GetUUID()
        {
            using (TridentSDK tr = TridentSDK.getInstance())
            {
                return tr?.getUUID() ?? "";
            }
        }

        public string GetCarrierString()
        {
            using (TridentSDK tr = TridentSDK.getInstance())
            {
                string mcc = tr?.getTelephonyDetails()?.mcc() ?? "";
                string mnc = tr?.getTelephonyDetails()?.mnc() ?? "";
                return $"{mcc}/{mnc}";
            }
        }

        public string GetNetworkType()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return "0";
            }
            //Check if the device can reach the internet via a carrier data network
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                return "1";
            }
            //Check if the device can reach the internet via a LAN
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                return "2";
            }

            return "0";
        }

        public string GetDeviceName()
        {
            using (TridentSDK tr = TridentSDK.getInstance())
            {
#if UNITY_IOS
                return tr.getDeviceDetails().getDeviceIdentifier();
#elif UNITY_ANDROID
                return tr.getDeviceDetails().getModel();
#else 
                return "";
#endif
            }
        }
        
        /// <summary>
        /// 스토어 링크 가져오기 (Notice 초기 화 후 설정)
        /// </summary>
        public string GetMarketAppLink()
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            return string.Empty;
#endif

            return Global._marketAppLinkAddr;
        }

        public bool IsGuestLogin()
        {
            return GetProvideType() == AuthProvider.AuthProviderGuest;
        }

        #region Core

        /// <summary>
        /// 앱이 LINE GAME SDK의 기능을 사용하려면 사용자가 앱을 실행할 때 SDK를 초기화해야 한다. LINE GAME SDK가 성공적으로 초기화되어야 Trident API를 사용할 수 있다.
        /// error: 구현 오류, 등록 문제로 발생하는 내용으로 개발시에는 별도 처리 없이 에러 발생시 팝업으로 알려줄 수 있도록 한다.
        /// </summary>
        /// <param name="onComplete"></param>
        public void Initialize(Action<bool, Error> onComplete)
        {
            using (var trident = TridentSDK.getInstance())
            {
                if (trident != null)
                {
                    // 초기화가 중복으로 실행된다면 LINE GAME SDK는 LAN 정보를 취득할 수 없기 때문에, 초기화를 실행하기 전에 이미 초기화가 진행 중인지 확인을 해야 한다.
                    if (trident.isInitializing())
                    {
                        //Debug.Log("[Trident Initialize] already in progress");
                        return;
                    }
                    else if (trident.isInitialized())
                    {
                        //Debug.Log("[Trident Initialize] already done");
                        SetTridentLanuage();
                        if (onComplete != null)
                        {
                            onComplete.Invoke(true, null);
                        }
                    }
                    else
                    {
                        var configList = GetConfigList();
                        trident.initialize(configList, (isSuccess, error) =>
                        {
                            if (onComplete != null)
                            {
                                //Debug.Log($"[Trident Initialize] onComplete : isSuccess {isSuccess}");
                                onComplete.Invoke(isSuccess, error);
                            }
                        });
                    }
                }
            }
        }

        private BaseConfigList GetConfigList()
        {
            var configList = new BaseConfigList();
        
#if TRIDENT_SETUP_DONE // SDK를 재설정 하는 경우 GameInfo를 삭제해야하여 해당 처리 필요
            configList.Add(GetTridentConfiguration());
            configList.Add(GetPushConfiguration());
            configList.Add(GetNoticeConfiguration());
            configList.Add(GetNeloConfiguration());
            configList.Add(GetGuestAdapterConfiguration()); 
            configList.Add(GetAnalyticsConfiguration()); 
#endif

            return configList;
        }

#if TRIDENT_SETUP_DONE
        private TridentConfiguration GetTridentConfiguration()
        {
            var tridentConfig = new TridentConfiguration(GameInfo.ApplicationId);
            tridentConfig.setApplicationPhase(GameInfo.AppPhase)                        // Application Phase
                         .setDebugLevel(GameInfo.AppDebugLevel)                         // Debug level
                         .setAllowAndroidInternalStorage(GameInfo.AllowInternalStorage) // Allow use internal storage for keychain
                         .setUILanguage(GetLanguageData())                        // UI Language
                         .setNetworkTimeout(GameInfo.NetworkTimeoutSeconds);            // timeout for API requests

            return tridentConfig;
        }

        private PushConfiguration GetPushConfiguration()
        {
            var pushConfig = new PushConfiguration();
            pushConfig.setGcmProjectNumber(GameInfo.GcmProjectNumber);

            return pushConfig;
        }

        private LCNoticeConfiguration GetNoticeConfiguration()
        {
            var noticeConfig = new LCNoticeConfiguration();
            var validHosts   = new StringList(GameInfo.ValidHostsForLAN.Length);
            foreach (string eachHost in GameInfo.ValidHostsForLAN)
            {
                if (eachHost.Trim() == string.Empty)
                {
                    continue;
                }

                validHosts.Add(eachHost.Trim());
            }

            noticeConfig.setValidHostsForLAN(validHosts);
            noticeConfig.setTimeoutSeconds(GameInfo.NoticeTimeoutSeconds);

            return noticeConfig;
        }

        private NeloConfiguration GetNeloConfiguration()
        {
            var neloConfig = new NeloConfiguration();
            neloConfig.setProjectName(GameInfo.NeloProjectName);
            neloConfig.setServerDomain(GameInfo.NeloServerDomain);
            neloConfig.setServerPort(GameInfo.NeloServerPort);
            neloConfig.setStabilityKey(GameInfo.NeloStabilityKey);

            return neloConfig;
        }

        private GuestAdapterConfiguration GetGuestAdapterConfiguration()
        {
            var guestConf = new GuestAdapterConfiguration();
            guestConf.setResetTimeLimit(GameInfo.GuestResetTimeLimitInHours);

            return guestConf;
        }

        private AnalyticsConfiguration GetAnalyticsConfiguration()
        {
            AnalyticsConfiguration analyticsConf = new AnalyticsConfiguration();
            analyticsConf.setTrackAllUser(true)
                         .setLoggingOption(AnalyticsLoggingOption.AnalyticsLoggingOptionDefault);
            return analyticsConf;
        }
#endif

        public void SetTridentLanuage()
        {
            using (var trident = TridentSDK.getInstance())
            {
                trident.setUILanguage(GetLanguageData());
            }
        }

        /// <summary>
        /// 언어 설정 리턴
        /// </summary>
        private Trident.TridentLanguage GetLanguageData ()
        {
            SystemLanguage language = LanguageUtility.SystemLanguage;
            
            switch ( language )
            {
                case SystemLanguage.Japanese:
                    return Trident.TridentLanguage.TridentJapanese;
                case SystemLanguage.ChineseTraditional:
                    return Trident.TridentLanguage.TridentChineseTraditional;
            }

            return Trident.TridentLanguage.TridentJapanese;
        }

        public Phase GetPhase()
        {      
            using (var trident = TridentSDK.getInstance())
            {
                return trident.isInitialized() ? trident.getPhase() : Trident.Phase.SandboxPhase;
            }
        }
        
        /// <summary>
        /// LINE GAME SDK에서는 점검 중에도 일부 사용자가 게임을 실행할 수 있도록 화이트리스트 사용자 기능을 제공
        /// 게임 앱에서는 TridentSDK.initialize와 getNotice를 실행한 결과 가져온 NotificationPayload 정보를 이용하여 공지를 표시해야 한다. 이때 사용자가 화이트리스트 사용자인지 여부를 판단해서 점검 중이라도 화이트리스트 사용자가 게임을 실행할 수 있게 한다.
        /// 점검 일정에 맞춰 미리 Technical PM에게 AppId와 Global IP 등록을 신청한다. AppId와 Global IP 등록이 완료되지 않으면 화이트리스트 사용자로 간주되지 않아 점검 중 게임에 접근할 수 없으니 주의해야 한다.
        /// WhiteListUser가 아닌 일반 유저에게만 점검용 팝업을 띄우고 이 팝업을 탭하면 게임이 종료되도록 해서 결과적으로 게임을 실행할 수 없게 하거나 화이트리스트 사용자에게도 점검용 팝업을 띄워서 이 팝업을 탭한 다음 게임을 실행할 수 있게 하는 식으로 구현해도 무방하다.
        /// </summary>
        /// <returns></returns>
        public bool IsWhiteListUser()
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            return true;
#endif
        
            using (var trident = TridentSDK.getInstance())
            {
                return trident.isInitialized() && trident.isWhiteListUser();
            }
        }

        public string GetCdnServerUrl()
        {
            using (var trident = TridentSDK.getInstance())
            {
                return trident.isInitialized() ? trident.getCdnServerUrl() : string.Empty;
            }
        }
        
        public string GetCountryCode()
        {
            using (var trident = TridentSDK.getInstance())
            {
                if (trident != null)
                {
                    return trident.isInitialized() ? trident.getDeviceDetails().getCountryCode() : string.Empty;
                }
            }
            
            SystemLanguage language = Application.systemLanguage;

            switch (language)
            {
                case SystemLanguage.English:
                    return "US";
                case SystemLanguage.Korean:
                    return "KR";
                case SystemLanguage.Japanese:
                    return "JP";
                case SystemLanguage.Spanish:
                    return "ES";
                case SystemLanguage.Thai:
                    return "TH";
                case SystemLanguage.ChineseSimplified:
                    return "CN";
                case SystemLanguage.ChineseTraditional:
                    return "TW";
            }

            return "JP";
        }

        public string GetLanguageCode()
        {
            using (var trident = TridentSDK.getInstance())
            {
                if (trident != null)
                {
                    return trident.isInitialized() ? trident.getDeviceDetails().getLocaleLanguage() : string.Empty;
                }
            }
            
            SystemLanguage language = Application.systemLanguage;

            switch (language)
            {
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Thai:
                    return "th";
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.ChineseTraditional:
                    return "zh";
            }

            return "ja";
        }
        
        #endregion

        #region Authentication

        /// <summary>
        /// 인증 결과를 받는 콜백이다. 이 콜백을 설정하지 않으면 인증 정보를 받을 수 없으니 반드시 구현한다. 인증 정보가 변경되었을 때 호출되기 때문에 게임 앱에서 적절한 처리를 구현해야 한다.
        /// isSignedIn: true (로그인), false (로그아웃)
        /// userKey: LINE GAME SDK userKey (예: T008000000RL)
        /// providerId: 인증 프로바이더 유형 (라인, 게스트)
        /// </summary>
        public void SetCredentialsChangedCallbackForAuth(Action<bool, string, AuthProvider> onComplete)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                authManager.setCredentialsChangedCallback((isSignedIn, userKey, providerId) =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke(isSignedIn, userKey, providerId);
                    }
                });
            }
        }
        
        /// <summary>
        /// Guest 인증을 사용하는 게임 앱은 반드시 'AuthResetGuestUserHandler'를 등록하고 resetCallback 알림을 구현해야 한다.
        /// shouldResetGuestUser가 true인 경우 = 게스트 계정 초기화
        /// </summary>
        public void SetAuthResetGuestUserHandlerForAuth(bool shouldResetGuestUser)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                authManager.setAuthResetGuestUserHandler(callback =>
                {
                    callback(shouldResetGuestUser);
                });
            }
        }
        
        /// <summary>
        /// SignIn 시도 후 [사용자가 이미 인증 정보를 취득한(이미 userKey를 발급한 사용자) 경우 호출]되는 콜백 등록
        /// </summary>
        public void SetAuthUserDataMigrationOptionHandlerForAuth(Action onUserDataMigrationOption)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                authManager.setAuthUserDataMigrationOptionHandler((key, callback) =>
                {
                    // callback : confirmToUserDataMigrationAction 여기서 선택에 따라서 userDataMigrationHandlerCallback 호출
                    // ConfirmToUserDataMigration : 유저한테 이전계정을 연동할지 새로할지 의사요청
                    existingUserKey                  = key;
                    userDataMigrationHandlerCallback = callback;
                    
                    if (onUserDataMigrationOption != null)
                    {
                        onUserDataMigrationOption.Invoke();
                    }
                });
            }
        }

        public string GetAccessToken()
        {
            if (NetworkSettings.Instance.IsRealDevice() == false &&
                NetworkSettings.Instance.useDebugAuthMode        &&
                NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line)
            {
                return NetworkSettings.Instance.debugToken;
            }

            using (AuthManager authManager = AuthManager.getInstance())
            {
                if (authManager != null)
                {
                    return authManager.getAccessToken();
                }

                return string.Empty;
            }
        }
        
        /// <summary>
        /// 프로바이더 가져오기
        /// </summary>
        public AuthProvider GetProvideType()
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                if (authManager != null)
                {
                    return authManager.getProviderId();
                }

                return AuthProvider.AuthProviderNone;
            }
        }
        

        /// <summary>
        /// 유저의 유니크한 id 가져오기
        /// </summary>
        public string GetUserKey()
        {
            if (NetworkSettings.Instance.IsRealDevice() == false &&
                NetworkSettings.Instance.useDebugAuthMode        &&
                NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line)
            {
                return NetworkSettings.Instance.debugUserKey;
            }

            using (AuthManager authManager = AuthManager.getInstance())
            {
                return authManager.getUserKey();
            }
        }

        /// <summary>
        /// signIn()을 사용해서 인증을 진행한다. signIn()을 사용하는 경우는 아래의 3가지이다.
        /// 최초 인증: 게임 앱 설치 후 최초 인증 시(기기에 지난 번 인증 정보가 없는 경우)
        /// 재인증: refresh에서 AccessToken의 유효기간 만료 에러가 발생한 경우
        /// 인증 타입의 변경: 인증 성공 후에 다른 providerId에 연동한 경우
        /// </summary>
        /// <param name="authProvider"> 인증 타입 (게스트, 라인) </param>
        public void SignIn(AuthProvider authProvider, Action<bool, Error> onComplete)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                if (authManager == null)
                {
                    return;
                }
                
                if (authManager.isAuthorizing())
                {
                    return;
                }
                
                authManager.signIn(authProvider, (isSuccess, error) =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke(isSuccess, error);
                    }
                });
            }
        }
        
        /// <summary>
        /// 유효한 AccessToken을 소지하고 있는 경우에는 refresh를 사용하여 인증한다. 
        /// </summary>
        public void Refresh(Action<bool, Error> onComplete)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                authManager.refresh((isSuccess, error) =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke(isSuccess, error);
                    }
                });
            }
        }
        
        public void SignOut(Action<bool, Error> onComplete = null)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                if (authManager != null)
                {
                    // 인증이 완료된 상태라면 signOut 호출하지 않고 바로 onComplete 실행
                    if (!AuthManager.getInstance().isAuthorized())
                    {
                        if (onComplete != null)
                        {
                            onComplete.Invoke(true, null);
                        }
                        return;
                    }
                    
                    AuthManager.getInstance().signOut((bool isSuccess, Error error) =>
                    {
                        if (onComplete != null)
                        {
                            onComplete.Invoke(isSuccess, error);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 계정연동시 SDK로부터 콜백함수로 들어옴. 이때 유저에게 의사결정 확인후 SDK에게 전달하는 함수
        /// </summary>
        public void ConfirmToUserDataMigration(AuthUserDataMigrationOption option)
        {
            using (AuthManager authManager = AuthManager.getInstance())
            {
                if (option == AuthUserDataMigrationOption.AuthMigrateCurrentUserData)
                {
                    Protocol.LoginReq guestReq = new Protocol.LoginReq();
                    guestReq.token   = this.GetAccessToken();
                    guestReq.userKey = authManager.getUserKey();

                    Extension.PokoLog.Log("---------------------------- existingUserKey : " + existingUserKey);
                    Extension.PokoLog.Log("---------------------------- userKey : "         + guestReq.userKey);
                    Extension.PokoLog.Log("---------------------------- token : "           + guestReq.token);

                    ServerAPI.MoveAccount(this.existingUserKey, guestReq, (Protocol.BaseResp resp) =>
                    {
                        if (!resp.IsSuccess) // 만약 중간에 error가 난다면 해당 프로세스를 취소한다.
                        {
                            Extension.PokoLog.Log(" ------------------------------- MoveAccount FALSE ------------------------------- " + resp);
                            userDataMigrationHandlerCallback(AuthUserDataMigrationOption.AuthMigrationUserCancel);
                        }
                        else
                        {
                            Extension.PokoLog.Log(" ------------------------------- MoveAccount TRUE ------------------------------- ");
                            userDataMigrationHandlerCallback((AuthUserDataMigrationOption)option);
                        }
                    });
                }
                else
                {
                    Extension.PokoLog.Log(" ------------------------------- resp Default ------------------------------- ");
                    userDataMigrationHandlerCallback((AuthUserDataMigrationOption)option);
                }
            }
        }
        #endregion
        
        #region TermView

        /// <summary>
        /// 이용약관을 표시하고 사용자의 동의를 요청한다. 사용자 동의 결과를 응답한다.
        /// </summary>
        public void CheckUserAgreement(Action<bool> onComplete)
        {
            using (TermViewService termViewService = ServiceManager.getInstance().getService<TermViewService>())
            {
                if (termViewService != null)
                {
                    termViewService.checkUserAgreement((isAccepted) =>
                    {
                        if (onComplete != null)
                        {
                            onComplete.Invoke(isAccepted);
                        }
                    });
                }
            }
        }

        #endregion
        
        #region Push

        /// <summary>
        /// DeviceToken을 Trident Push 서버로 보낸다.
        /// </summary>
        public void RegisterDeviceTokenForPush()
        {
            if (!HasDeviceToken())
            {
                return;
            }

            PushService pushService = ServiceManager.getInstance().getService<PushService>();
            if (pushService != null)
            {
                pushService.registerDeviceToken((isSuccess, error) =>
                {
                    Extension.PokoLog.Log("PushService - registerDeviceToken callback: " + isSuccess);
                    if (!isSuccess)
                    {
                        Extension.PokoLog.Log("Error[" + error.getCode() + "]: " + error.getMessage());
                    }
                });
            }
        }
        
        private bool HasDeviceToken()
        {
            string deviceToken = GetDeviceToken();
            return !string.IsNullOrEmpty(deviceToken);
        }

        private string GetDeviceToken()
        {
            PushService pushService = ServiceManager.getInstance().getService<PushService>();
            if (pushService != null)
            {
                return pushService.getDeviceToken();
            }

            return null;
        }

        #endregion
        
        #region Notice
        
        public void GetNotice(ArrayList noticeType, Action<bool, NotificationPayload, Error> onComplete)
        {
            using (var noticeService = ServiceManager.getInstance().getService<LCNoticeService>())
            {
                var type = (LCNoticeServiceType[])noticeType.ToArray(typeof(LCNoticeServiceType));
            
                noticeService.getNotice(true, type, (isSuccess, payload, error) =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke(isSuccess, payload, error);
                    }
                });
            }
        }
        
        /// <summary>
        /// markNotificationRead를 이용하여 가져온 NotificationInfo에 포함되어 있는 공지 ID를 지정하고 공지를 읽음 처리한다.
        /// </summary>
        /// <param name="noticeId"></param>
        public void MarkNotificationRead(long noticeId)
        {
            using (var noticeService = ServiceManager.getInstance().getService<LCNoticeService>())
            {
                noticeService.markNotificationRead( noticeId );
            }
        }
        
        public void ShowBoard(LCNoticeServiceBoardCategory noticeBoardCategory, string noticeBoardDocumentId, string noticeBoardTitle)
        {
#if UNITY_EDITOR
            return;
#endif
            LCNoticeService noticeService = ServiceManager.getInstance().getService<LCNoticeService>();
            if (noticeService != null)
            {
                noticeService.showBoard(noticeBoardCategory, noticeBoardDocumentId, noticeBoardTitle);
            }
        }
        
        #endregion

        #region Promotion

        private string url = "";

        private PromotionService.PromotionDeeplinkCallbackDelegate deeplinkCallbackDelegate;
        
        public void InitializePromotionSDK(Action<bool> onInitializeSuccess, Action onDeeplinkReceived)
        {
            ManagerData.promotionState = ManagerData.PromotionInitializeState.INITIALIZING;
            
            // DeeplinkCallback 등록
            deeplinkCallbackDelegate = (bool isSuccess, long eventCode, string message) => 
            {
                Debug.Log(" *********** deeplinkCallbackDelegate ***********");
                if (isSuccess)
                {
                    onDeeplinkReceived.Invoke();
                }
            };
            
            // PromotionService 초기화
            PromotionService promotionService = ServiceManager.getInstance().getService<PromotionService>();
            promotionService.initialize((bool isSuccess, long eventCode, string message) =>
            {
                onInitializeSuccess(isSuccess);
                
            }, deeplinkCallbackDelegate);
        }

        /// <summary>
        /// sendTrackingDeeplink()를 initialize() 전에 호출하면 initialize() 호출 시에 같이 전송된다.
        /// </summary>
        public void SendDeeplink(string deepLinkUrl)
        {
            PromotionService promotionService = ServiceManager.getInstance().getService<PromotionService>();
            promotionService.sendTrackingDeeplink(deepLinkUrl);
        }

        public void TriggerChannel(string trigger, Action<long, string> onComplete)
        {
            PromotionService promotionService = ServiceManager.getInstance().getService<PromotionService>();
            promotionService.triggerChannel(trigger, (long eventCode, string message) =>
            {
                if (onComplete != null)
                {
                    onComplete.Invoke(eventCode, message);
                }
            });
        }
        
        public void TriggerChannelCallbackReset()
        {
            PromotionService promotionService = ServiceManager.getInstance().getService<PromotionService>();
            promotionService.triggerChammelCallbackReset();
        }

        public void GetPromotionStatus(string trigger, Action<bool, long, string, PromotionStatusList> onComplete)
        {
            PromotionService promotionService = ServiceManager.getInstance().getService<PromotionService>();
            promotionService.getPromotionStatus(trigger, (bool isSuccess, long eventCode, string message, PromotionStatusList list) =>
            {
                if (onComplete != null)
                {
                    onComplete.Invoke(isSuccess, eventCode, message, list);
                }
            });
        }

        #endregion
    }
}