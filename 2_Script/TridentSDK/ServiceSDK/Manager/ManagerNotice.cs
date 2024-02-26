using System;
using System.Collections;
using Trident;
using UnityEngine;

namespace ServiceSDK
{
    public class ManagerNotice : MonoSingleton<ManagerNotice>
    {
        public  bool                IsWhitelistUser { get; private set; }
        public  bool                IsForceUpdate   { get; private set; }
        public  bool                IsMaintenance   { get; private set; }
        private NotificationPayload Notification    { get; set; }

        private bool   isInProcess;
        private string pageEventUrl;

        private Action        onCompleteShowNotice;
        private Action<Error> onFailShowNotice;

        public static void OnReboot()
        {
            if (instance != null)
            {
                instance.Notification    = null;
                
                instance.IsWhitelistUser = false;
                instance.IsForceUpdate   = false;
                instance.IsMaintenance   = false;

                instance.isInProcess = false;
                instance.pageEventUrl = "";

                instance.onCompleteShowNotice = null;
                instance.onFailShowNotice     = null;
            }
        }

        /// <summary>
        /// 강제 업데이트, 점검, 임의 업데이트 Notice를 받아와 출력, ProcessManager 스크립트에서 사용중인 함수
        /// </summary>
        public void ShowMajorNoticeForLogin(Action onComplete = null, Action<Error> onFail = null)
        {
            Init();
            
            if (onComplete != null)
            {
                onCompleteShowNotice = onComplete;
            }
            
            if (onFail != null)
            {
                onFailShowNotice = onFail;
            }

            isInProcess = true;

            var noticeType = new ArrayList()
            {
                LCNoticeServiceType.LCNoticeForceUpdate,
                LCNoticeServiceType.LCNoticeMaintenance,
                LCNoticeServiceType.LCNoticeUpdate,
            };

            ShowNotice(noticeType);
        }
        
        /// <summary>
        /// 강제 업데이트, 점검 Notice를 받아와 출력
        /// </summary>
        public void ShowMajorNotice(Action onComplete = null, Action<Error> onFail = null)
        {
            Init();
            
            if (onComplete != null)
            {
                onCompleteShowNotice = onComplete;
            }
            
            if (onFail != null)
            {
                onFailShowNotice = onFail;
            }

            isInProcess = true;

            var noticeType = new ArrayList()
            {
                LCNoticeServiceType.LCNoticeForceUpdate,
                LCNoticeServiceType.LCNoticeMaintenance
            };

            ShowNotice(noticeType);
        }
        
        /// <summary>
        /// 그 외 Notice를 받아와 출력
        /// </summary>
        public void ShowMinorNotice(Action onComplete)
        {
            Init();
            
            if (onComplete != null)
            {
                onCompleteShowNotice = onComplete;
            }

            isInProcess = true;

            var noticeType = new ArrayList()
            {
                LCNoticeServiceType.LCNoticePage,
                LCNoticeServiceType.LCNoticeSystem,
                LCNoticeServiceType.LCNoticeUnknown,
            };
        
            ShowNotice(noticeType);
        }
        
        /// <summary>
        /// 백그라운드에서 포그라운드 전환시에 major 노티스 출력 확인
        /// </summary>
        public void ShowNoticeOnAppleCationPause(Action onComplete = null, Action<Error> onFail = null)
        {
#if UNITY_EDITOR
            return;
#endif
            if (ServiceSDKManager.instance == null || isInProcess) return;

            isInProcess          = true;
            
            ShowMajorNotice(onComplete, onFail);
        }

        /// <summary>
        /// 노티스 매니저 초기화
        /// </summary>
        private void Init()
        {
            isInProcess          = false;
            onCompleteShowNotice = null;
            onFailShowNotice     = null;
            pageEventUrl         = "";
        }
        
        /// <summary>
        /// 노티스 출력
        /// </summary>
        /// <param name="noticeType">출력이 필요한 노티스 타입 리스트</param>
        private void ShowNotice(ArrayList noticeType)
        {
            ServiceSDKManager.instance.GetNotice(noticeType, (isSuccess, payload, error) =>
            {
                if (isSuccess)
                {
                    SetInfoFromNotice(payload);
                    StartCoroutine("CoShowNotice");
                }
                else
                {
                    if (onFailShowNotice != null)
                    {
                        onFailShowNotice.Invoke(error);
                        onFailShowNotice = null;
                    }
                    
                    isInProcess = false;
                }
            });
        }

        /// <summary>
        /// 노티스 정보 세팅
        /// </summary>
        /// <param name="payload"></param>
        private void SetInfoFromNotice(NotificationPayload payload)
        {
            Global._marketAppLinkAddr = payload.getAppInfo().getMarketAppLink();
            Notification              = payload;
            IsForceUpdate             = payload.getIsForceUpdate();
            IsMaintenance             = payload.getIsMaintenance();
            IsWhitelistUser           = ServiceSDKManager.instance.IsWhiteListUser();
        }

        /// <summary>
        /// GetNotice호출로 받아온 Notice 타입에 맞게 출력
        /// </summary>
        private IEnumerator CoShowNotice()
        {
            bool isShowNotice = false;
            
            // 강제 업데이트
            if (Notification.getIsForceUpdate())
            {
                foreach (var notificationInfo in Notification.getNotificationInfoList())
                {
                    if (notificationInfo.getType() == NotificationInfo.Type.TypeForceupdate)
                    {
                        isShowNotice = true;
                        ShowForcesUpdateNotice();
                        yield return new WaitUntil(() => UIPopupSystem._instance == null);
                    }
                }
            }
            // 점검 공지
            else if (Notification.getIsMaintenance())
            {
                foreach (var notificationInfo in Notification.getNotificationInfoList())
                {
                    if (notificationInfo.getType() == NotificationInfo.Type.TypeMaintenance)
                    {
                        isShowNotice = true;
                        ShowMaintenanceNotice(() =>
                        {
                            isShowNotice = false;
                        });
                        yield return new WaitUntil(() => UIPopupSystem._instance == null);
                    }
                }
            }
            else
            {
                // 임의 업데이트
                foreach (var notificationInfo in Notification.getNotificationInfoList())
                {
                    if (notificationInfo.getType() == NotificationInfo.Type.TypeUpdate)
                    {
                        isShowNotice = true;
                        ShowUpdateNotice(notificationInfo, () =>
                        {
                            isShowNotice = false;
                        });
                        yield return new WaitUntil(() => UIPopupSystem._instance == null);
                        break;
                    }
                }

                foreach (var notificationInfo in Notification.getNotificationInfoList())
                {
                    switch (notificationInfo.getType())
                    {
                        // 일반 공지
                        case NotificationInfo.Type.TypeSystem:
                        {
                            pageEventUrl = notificationInfo.getLink();
                            if (!string.IsNullOrEmpty(pageEventUrl))
                            {
                                ShowLinkNotice(notificationInfo);
                            }
                            else
                            {
                                ShowBasicNotice(notificationInfo);
                            }

                            break;
                        }
                        // 배너 타입 공지
                        case NotificationInfo.Type.TypeBanner:
                        // 알 수 없는 공지
                        case NotificationInfo.Type.TypeUnknown:
                        {
                            ShowBasicNotice(notificationInfo);
                            break;
                        }
                    }

                    yield return new WaitUntil(() => UIPopupSystem._instance == null);
                }
            }

                  
            if (isShowNotice == true)
            {
                onCompleteShowNotice = null;
            }
            else
            {
                if (onCompleteShowNotice != null)
                {
                    onCompleteShowNotice.Invoke();
                    onCompleteShowNotice = null;
                }
            }
            
            isInProcess = false;
        }

        /// <summary>
        /// 강제 업데이트 노티스 팝업 출력 (항상 출력, 읽음 처리 X)
        /// 강제 업데이트시에는 마켓 이동후 다시 돌아오는 경우 팝업이 종료되어 게임진입이 가능하지 않도록 팝업이 종료되지 않도록 처리
        /// </summary>
        private void ShowForcesUpdateNotice()
        {
            string title, message, buttonTitle;
            if (!Notification.getIsMaintenance())
            {
                // 강제업데이트만 걸린 경우
                // 새로운 업데이트가 있습니다. 업데이트를 해야 진행할수 있습니다.
                title       = Global._instance.GetString("p_t_4");
                message     = Global._instance.GetString("n_up_1");
                buttonTitle = Global._instance.GetString("btn_63");
            }
            else
            {
                // 메인터넌스 - 강제업데이트 동시에 걸린 경우
                // 새로운 업데이트가 있습니다. 업데이트를 해야 진행할수 있습니다.
                title       = Global._instance.GetString("p_t_4");
                message     = Global._instance.GetString("n_up_2");
                buttonTitle = Global._instance.GetString("btn_63");
            }

            UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupNotice.InitSystemPopUp(title, message, false, null);
            popupNotice.FunctionSetting(1, "GoToAppMarketAndReboot", this.gameObject);
            popupNotice.FunctionSetting(3, "GoToAppMarketAndReboot", this.gameObject);
            popupNotice.SetButtonText(1, buttonTitle);
            popupNotice.HideCloseButton();
        }

        /// <summary>
        /// 점검 노티스 팝업 출력 (항상 출력, 읽음 처리 X)
        /// 화이트 리스트인 경우에는 정상적으로 게임 진입이 가능, 화이트리스트는 IP주소를 기준으로 판단되며 라인 디렉터측으로 설정 요청 필요.
        /// </summary>
        private void ShowMaintenanceNotice(Action onClickWitheUser)
        {
            // 화이트유저라면
            if (ServiceSDKManager.instance.IsWhiteListUser())
            {
                // 점검중입니다. 당신은 withe user입니다. 계속 진행하시겠습니까?
                string title        = Global._instance.GetString("p_t_4");
                string message      = Global._instance.GetString("n_up_4");
                string buttonTitle  = Global._instance.GetString("btn_18");
                string buttonTitle2 = "WhiteList";

                UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupNotice.InitSystemPopUp(title, message, true, null);
                popupNotice.FunctionSetting(1, "RebootApplication", this.gameObject);
                popupNotice.SetCallbackSetting(2, onClickWitheUser, this.gameObject);
                popupNotice.HideCloseButton();
                popupNotice.SetButtonText(1, buttonTitle);
                popupNotice.SetButtonText(2, buttonTitle2);
            }
            else
            {
                // 점검중입니다
                string title   = Global._instance.GetString("p_t_4");
                string message = Global._instance.GetString("n_up_4");

                UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupNotice.InitSystemPopUp(title, message, false, RebootApplication);
            }
        }

        /// <summary>
        /// 임의 업데이트 노티스 팝업 (TODO : 포코포코는 1회만 출력하고 있음)
        /// </summary>
        /// <param name="notificationInfo"></param>
        private void ShowUpdateNotice(NotificationInfo notificationInfo, Action onClickNext)
        {
            // 새로운 업데이트가 있습니다. 마켓으로 이동하시겠습니까?
            string title        = Global._instance.GetString("p_t_4");
            string message      = Global._instance.GetString("n_up_3");
            string buttonTitle  = Global._instance.GetString("btn_63");
            string buttonTitle2 = Global._instance.GetString("btn_62");

            UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupNotice.InitSystemPopUp(title, message, true, null);
            popupNotice.FunctionSetting(1, "GoToAppMarketAndReboot", this.gameObject);
            popupNotice.SetCallbackSetting(2, onClickNext, this.gameObject);
            popupNotice.HideCloseButton();
            popupNotice.SetButtonText(1, buttonTitle);
            popupNotice.SetButtonText(2, buttonTitle2);
        }

        /// <summary>
        /// 기본 노티스 팝업 출력 (내용 출력, 확인 버튼)
        /// </summary>
        /// <param name="notificationInfo"></param>
        private void ShowBasicNotice(NotificationInfo notificationInfo)
        {
            string title   = Global._instance.GetString("p_t_4");
            string message = notificationInfo.getContent();

            UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupNotice.InitSystemPopUp(title, message, false, null);
            popupNotice.HideCloseButton();
            popupNotice.SetButtonText(1, Global._instance.GetString("btn_1"));

            ServiceSDKManager.instance.MarkNotificationRead(notificationInfo.getNoticeId());
        }

        /// <summary>
        /// Link 노티스 팝업 출력 (ok버튼 클릭시 링크로 전이)
        /// NotificationInfo.Type.TypeSystem인 경우에 getContentUrl 값 존재여부로 Link 팝업 or 기본 노티스 팝업 출력 여부를 결정
        /// </summary>
        /// <param name="notificationInfo"></param>
        private void ShowLinkNotice(NotificationInfo notificationInfo)
        {
            string title   = Global._instance.GetString("p_t_4");
            string message = notificationInfo.getContent();

            UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupNotice.InitSystemPopUp(title, message, false, null);
            popupNotice.FunctionSetting(1, "OpenLinkUrl", this.gameObject);
            popupNotice.HideCloseButton();
            popupNotice.SetButtonText(1, Global._instance.GetString("btn_1"));

            ServiceSDKManager.instance.MarkNotificationRead(notificationInfo.getNoticeId());
        }

        private void OpenLinkUrl()
        {
            if (!string.IsNullOrEmpty(pageEventUrl))
            {
                Application.OpenURL(pageEventUrl);
            }
        }

        private void RebootApplication()
        {
            Global.ReBoot();
        }

        private void GoToAppMarketAndReboot()
        {
            Application.OpenURL(ServiceSDK.ServiceSDKManager.instance.GetMarketAppLink());
            Global.ReBoot();
        }
    }
}
