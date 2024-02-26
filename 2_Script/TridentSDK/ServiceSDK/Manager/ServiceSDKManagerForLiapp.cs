using Liapp;
using UnityEngine;

namespace ServiceSDK
{
    public partial class ServiceSDKManager
    {
        private bool bIsStarted = false;
        private int nRet = LiappforUnityImpl.LIAPP_EXCEPTION;

        private string strLiappMessage;
        private int _intLA1;

        string user_key_from_server = "user_key_from_server";
        LiappforUnityImpl _LiappAgent;

        public void StartLiapp()
        {
            _LiappAgent = LiappforUnityImpl.Instance;

            if (bIsStarted == false)
            {
                nRet = _LiappAgent.LA1();
            }
            else
            {
                nRet = _LiappAgent.LA2();
            }

            if (LiappforUnityImpl.LIAPP_SUCCESS == nRet)
            {
                bIsStarted = true;
            }
            else if (LiappforUnityImpl.LIAPP_EXCEPTION == nRet)
            {
                if (UseLiapp())
                {
                    ServiceSDK.ServiceSDKManager.instance.ErrorLog("[LiappCheck] LIAPP_EXCEPTION");
                    this.ShowRootingDetectingPopup();
                }
            }
            else
            {
                strLiappMessage = _LiappAgent.GetMessage();
                ServiceSDK.ServiceSDKManager.instance.ErrorLog("[LiappCheck] LIAPP_DETECTED : " + strLiappMessage);

                if (LiappforUnityImpl.LIAPP_DETECTED == nRet)
                {
                    // Line에서 앱 강제종료, 추가 처리 불필요
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_ROOTING == nRet)
                {
                    this.ShowRootingDetectingPopup();
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_VM == nRet)
                {
                    this.ShowRootingDetectingPopup();
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_HACKING_TOOL == nRet)
                {
                    this.ShowCheatDetectingPopup();
                }
            }
        }

        public void ReStartLiapp()
        {
            if (bIsStarted == false)
            {
                nRet = _LiappAgent.LA1();
            }
            else
            {
                nRet = _LiappAgent.LA2();
            }

            if (LiappforUnityImpl.LIAPP_SUCCESS == nRet)
            {
                bIsStarted = true;
            }
            else if (LiappforUnityImpl.LIAPP_EXCEPTION == nRet)
            {
                if (UseLiapp())
                {
                    ServiceSDK.ServiceSDKManager.instance.ErrorLog("[LiappCheck] LIAPP_EXCEPTION");
                    this.ShowRootingDetectingPopup();
                }
            }
            else
            {
                strLiappMessage = _LiappAgent.GetMessage();
                ServiceSDK.ServiceSDKManager.instance.ErrorLog("[LiappCheck] LIAPP_DETECTED : " + strLiappMessage);

                if (LiappforUnityImpl.LIAPP_DETECTED == nRet)
                {
                    // Line에서 앱 강제종료, 추가 처리 불필요
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_ROOTING == nRet)
                {
                    this.ShowRootingDetectingPopup();
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_VM == nRet)
                {
                    this.ShowRootingDetectingPopup();
                }
                else if (LiappforUnityImpl.LIAPP_DETECTED_HACKING_TOOL == nRet)
                {
                    this.ShowCheatDetectingPopup();
                }
            }
        }

        /// <summary>
        /// 접속 성공 시 프로필 초기화 될 때까지 대기 후 Liapp 서버에 userKey 전송
        /// </summary>
        public void SendUserIDinLiapp()
        {
            if (bIsStarted)
            {
                string user_id = SDKGameProfileManager._instance.GetMyProfile()._userKey;
                _LiappAgent.SUID(user_id);
            }
        }

        /// <summary>
        /// 치팅 감지시 팝업 생성
        /// </summary>
        private void ShowCheatDetectingPopup()
        {
            //TO DO: 로컬파일 추가되면 수정
            string title = Global._instance.GetString("p_t_4"); //알림 타이틀
            string message = Global._instance.GetString("n_er_12");//정상적인 게임 이용을 방해하는 툴이 탐지되었습니다. 해당 툴을 삭제하지 않으면 본 서비스를 이용하실 수 없습니다.不正行為を行うためのツールが検出されました。該当ツールを削除するまで本サービスは利用できません。Iniquitous tool(s) has been found. You cannot access the service unless you delete it.

            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(title, message, false, () =>
            {
                Global.ReBoot();
            });
            popup.SetButtonText(1, Global._instance.GetString("btn_1"));

        }

        /// <summary>
        /// 루팅 감지시 생성
        /// </summary>
        private void ShowRootingDetectingPopup()
        {
            //TO DO: 로컬파일 추가되면 수정
            string title = Global._instance.GetString("btn_1"); //알림 타이틀
            //당사 서비스에 대한 부정행위에 사용되는 툴이 단말기 내에 존재할 경우, 본 서비스를 이용하실 수 없습니다. 또한 해당 툴이 단말기 내에 존재할 경우, 
            //툴에 관한 정보를 취득합니다.端末内に当社サービスに対する不正行為を行うためのツールがある場合、本サービスをご利用いただけません。また、該当ツールが端末内にある場合、該当ツールのアプリ情報を取得いたします。
            //Devices with iniquitous tool(s) are denied access to this service.\nWhen such tool(s) are detected, will you permit us to collect App data of the tool(s)?
            string message = Global._instance.GetString("n_er_13");
            string positiveButtonText = Global._instance.GetString("btn_31"); //허가 許可する Permit
            string negativeButtonText = Global._instance.GetString("btn_30"); //거절 許可しない Refuse

            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.FunctionSetting(2, "OnRefuseRooting", gameObject);
            popup.SetButtonText(1, positiveButtonText);
            popup.SetButtonText(2, negativeButtonText);
            popup.InitSystemPopUp(title, message, true);
        }


        /// <summary>
        /// 루팅 감지시 팝업이 떴을때 거절 버튼을 누르면 콜백
        /// </summary>
        private void OnRefuseRooting()
        {
            Global.ReBoot();
        }

        /// <summary>
        /// 개발 환경인지 아닌지 체크
        /// </summary>
        private bool UseLiapp()
        {
#if LIAPP_FOR_DISTRIBUTE
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
        {
            return false;
        }
        else
        {
            return true;
        }
#else
            return false;
#endif
        }
    }
}