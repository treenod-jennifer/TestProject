using UnityEngine;

namespace ServiceSDK
{
    public partial class ServiceSDKManager
    {
        public void DebugLog(string log, string stackTrace = "")
        {
            SendLog(
                logType:    Trident.NeloServiceLogType.NeloServiceLogDebug,
                message:    log,
                location:   stackTrace
            );
        }

        public void ErrorLog(string log, string errorCode = "", string stackTrace = "")
        {
            SendLog(
                logType:    Trident.NeloServiceLogType.NeloServiceLogError,
                errorCode:  errorCode,
                message:    log,
                location:   stackTrace
            );
        }

        


        private Trident.NeloService neloInstance;
        private Trident.NeloService nelo
        {
            get
            {
                if (neloInstance == null)
                {
                    neloInstance = Trident.ServiceManager.getInstance().getService<Trident.NeloService>();
                }

                return neloInstance;
            }
        }

        private void SendLog (Trident.NeloServiceLogType logType, string stabilityValue = "", string errorCode = "", string message = "", string location = "")
        {
            message = (message == null) ? "no message" : message;
            string stability = GetStabilityValue(logType);
            string stackTrace = location == "" ? StackTraceUtility.ExtractStackTrace() : location;
            string serviceCode = ServiceCode.BuildServiceCode();

            message += "\nService Code : " + serviceCode;
            message += "\nBuilded Time : " + NetworkSettings.Instance.buildedTimeString;

#if UNITY_EDITOR || UNUSED_LINE_SDK
            Debug.Log(
                "Send Nelo Message" + "\n" +
                "logType : " + logType + "\n" +
                "stabilityValue : " + stability + "\n" +
                "errorCode : " + errorCode + "\n" +
                "message : " + message + "\n" +
                "location : " + stackTrace + "\n"
            );
#else
            nelo.sendNeloLog(logType, stability, errorCode, message, stackTrace);
#endif
        }

        private string GetStabilityValue(Trident.NeloServiceLogType logType)
        {
            string stabilityValue = logType.ToString();
            stabilityValue = stabilityValue.Replace("NeloServiceLog", "");

            return stabilityValue;
        }




        #region 예외 발생 이벤트 등록
        [RuntimeInitializeOnLoadMethod]
        private static void ExceptionEventSetting()
        {
            Application.logMessageReceived += instance.Application_logMessageReceived;
            System.AppDomain.CurrentDomain.UnhandledException += instance.CurrentDomain_UnhandledException;
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
            {
                ErrorLog(log: "LogType : " + type.ToString() + "\n" + condition, stackTrace: stackTrace);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorLog(sender.ToString() + "\n" + e.ToString());
        }
        #endregion
    }
}