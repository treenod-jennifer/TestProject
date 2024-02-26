using UnityEngine;
using com.adjust.sdk;

namespace ServiceSDK
{
    public class AdjustManager : MonoBehaviour
    {
        private Adjust adjustObj = null;

        private static AdjustManager _instance = null;
        public static AdjustManager instance
        {
            get
            {
                if(_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "AdjustManager";
                    _instance = obj.AddComponent<AdjustManager>();

                    DontDestroyOnLoad(obj);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Adjust 초기화
        /// </summary>
        private void Awake()
        {
            if (IsEditor())
                return;

            var ori = Resources.Load("Adjust") as GameObject;
            adjustObj = Instantiate(ori, transform).GetComponent<Adjust>();
            adjustObj.transform.parent = transform;
            adjustObj.transform.name = "Adjust";

            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.RELEASE)
                adjustObj.environment = AdjustEnvironment.Production;
            else
                adjustObj.environment = AdjustEnvironment.Sandbox;

            AdjustConfig adjustConfig = new AdjustConfig(adjustObj.appToken, adjustObj.environment, (adjustObj.logLevel == AdjustLogLevel.Suppress));
            adjustConfig.setLogLevel(adjustObj.logLevel);
            adjustConfig.setSendInBackground(adjustObj.sendInBackground);
            adjustConfig.setEventBufferingEnabled(adjustObj.eventBuffering);
            adjustConfig.setLaunchDeferredDeeplink(adjustObj.launchDeferredDeeplink);

            adjustConfig.setEventSuccessDelegate((eventSuccessData) => { Debug.Log("Adjust EventSuccessDelegate : " + eventSuccessData.Message); });
            adjustConfig.setEventFailureDelegate((eventFailData) => { Debug.Log("Adjust EventFailureDelegate : " + eventFailData.Message); });

            #if UNITY_IOS
            adjustConfig.setAppSecret(1, 2016761221, 682226299, 1404530740, 1137743886);
            #elif UNITY_ANDROID
            adjustConfig.setAppSecret(2, 562283225, 913493117, 1844245297, 776189792);
            #endif

            Adjust.start(adjustConfig);

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust Init : " + adjustObj.appToken + " / " + adjustObj.environment);
        }

        private bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        #region event

        /// <summary>
        /// 앱을 설치하고 라인 아이디로 최초 로그인시
        /// </summary>
        public void OnRegistration_Line()
        {
            AdjustEvent adjustEvent = new AdjustEvent(EventToken.registration_line);
            Adjust.trackEvent(adjustEvent);

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust OnRegistration");
        }

        /// <summary>
        /// 앱을 설치하고 라인 게스트로 최초 로그인시
        /// </summary>
        public void OnRegistration_Guest()
        {
            if (PlayerPrefs.HasKey("OnRegistration_Guest"))
                return;

            PlayerPrefs.SetInt("OnRegistration_Guest", 0);
            AdjustEvent adjustEvent = new AdjustEvent(EventToken.registration_guest);
            Adjust.trackEvent(adjustEvent);

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust OnRegistration");
        }

        /// <summary>
        /// in app 결재시 
        /// </summary>
        /// <param name="revenue">가격</param>
        /// <param name="currency">화폐</param>
        public void OnPurchase(string revenue, string currency)
        {
            AdjustEvent adjustEvent = new AdjustEvent(EventToken.purchase);
            adjustEvent.setRevenue(double.Parse(revenue), currency);
            Adjust.trackEvent(adjustEvent);

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust OnPurchase");
        }

        /// <summary>
        /// 스테이지 클리어시 
        /// </summary>
        /// <param name="stage">클리어한 스테이지</param>
        public void OnStageClear(int stage)
        {
            string eventToken;

            switch (stage)
            {
                case 1:     eventToken = EventToken.stage1_clear; break;
                case 5:     eventToken = EventToken.stage5_clear; break;
                case 10:    eventToken = EventToken.stage10_clear; break;
                case 15:    eventToken = EventToken.stage15_clear; break;
                case 20:    eventToken = EventToken.stage20_clear; break; 
                case 30:    eventToken = EventToken.stage30_clear; break;
                case 100:   eventToken = EventToken.stage100_clear; break;
                case 150:   eventToken = EventToken.stage150_clear; break;
                case 200:   eventToken = EventToken.stage200_clear; break;
                case 300:   eventToken = EventToken.stage300_clear; break;
                case 350:   eventToken = EventToken.stage350_clear; break;
                case 400:   eventToken = EventToken.stage400_clear; break;
                default:    return;
            }

            AdjustEvent adjustEvent = new AdjustEvent(eventToken);
            Adjust.trackEvent(adjustEvent);
            
            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust OnStageClear : " + eventToken);
        }

        /// <summary>
        /// 친구 초대시 
        /// </summary>
        /// <param name="inviteCount">초대 횟수</param>
        public void OnInvite(int inviteCount)
        {
            if (inviteCount != 5)
            {
                return;
            }

            AdjustEvent adjustEvent = new AdjustEvent(EventToken.invite_5);
            Adjust.trackEvent(adjustEvent);

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Adjust OnInvite");
        }

        /// <summary>
        /// 테스트용 이벤트 입니다.
        /// </summary>
        public void OnTest()
        {
            AdjustEvent adjustEvent = new AdjustEvent("n3myvq");
            Adjust.trackEvent(adjustEvent);
        }

        private static class EventToken
        {
            public const string invite_5 = "qikmmu";
            public const string purchase = "inxbp2";
            public const string registration_guest = "4qgw62";
            public const string registration_line = "3qdkno";
            public const string stage1_clear = "bqsvgv";
            public const string stage5_clear = "85nmf5";
            public const string stage10_clear = "5waod3";
            public const string stage15_clear = "it3flt";
            public const string stage20_clear = "x1qg9p";
            public const string stage30_clear = "bavqsb";
            public const string stage100_clear = "s4rj4q";
            public const string stage150_clear = "cqxzmi";
            public const string stage200_clear = "qalzch";
            public const string stage300_clear = "mxjkdo";
            public const string stage350_clear = "d57hy2";
            public const string stage400_clear = "941cb1";
        }

        #endregion
    }
}