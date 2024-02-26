using System.Collections;
using UnityEngine;
using ServiceSDK;

public class ProcessManager : MonoBehaviour
{
	private const string APP_ICON_VERSION = "AppIconVer";
	
	public void Process()
	{
		StartCoroutine(CoProcess());
	}

	/// <summary>
	/// DataLoadState에 따른 동작 수행 (INITIALIZING, LINE_LONGIN, CONNECTING)
	/// </summary>
	private IEnumerator CoProcess()
	{
		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_INIT_S);
		yield return StartCoroutine(CoTridentInitialize());
		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_INIT_E);

		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_NOTICE_S);
		yield return StartCoroutine(CoShowMajorNotice());
		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_NOTICE_E);

		ManagerData._instance._state = DataLoadState.eLineInitialize;

		// SANDBOX 환경에서 서버 선택창 출력 (로그인 시 토큰값 유효성 검사를 위해 미리 서버 정보 셋팅)
		yield return StartCoroutine(CoLaunchProcess());
		
		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_LOGIN_S);
		var loginProcess = new LoginProcess(this, GetLoginStrategy());
		yield return StartCoroutine(loginProcess.CoProcess());
		ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SDK_LOGIN_E);

		ManagerData._instance._state = DataLoadState.eLineLogin;
		
		var connectingProcess = new ConnectingProcess(this, GetConnectingStrategy());
		yield return StartCoroutine(connectingProcess.CoProcess());
		
#if UNITY_IOS
		//iOS 환경에서만 앱 변경 기능 동작
		yield return StartCoroutine(CoAppIconChange());
#endif
		
		// 의미 없는 코드로 보이는데 기존에 있던 코드라 유지
		if (ServiceSDKManager.instance.IsGuestLogin() == false)
		{
			ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.GET_RANKING_S);
			ManagerData._instance._state = DataLoadState.eLineRankingData;
			ServiceSDKManager.instance.InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE.GET_RANKING_E);
		}
		
		ManagerData._instance._state = DataLoadState.eGameData;
		
	}

	/// <summary>
	/// 트라이던트 SDK 생성 및 초기화 진행
	/// </summary>
	private IEnumerator CoTridentInitialize()
	{
#if UNITY_EDITOR
		yield return null;
#else
		bool isComplete = false;

		ServiceSDKManager.instance.Initialize((isSuccess, error) =>
		{
			if (isSuccess)
			{
				isComplete = true;
			}
			else
			{
				// 스트링 추가필요(예 : 게임기동에 실패했습니다 등)
				string title       = Global._instance.GetString("p_t_4");
				string buttonTitle = Global._instance.GetString("btn_1");
				string errorMessage = Global._instance.GetSystemNetworkErrorAndRebootString(error.getCode(), error.getMessage());

				UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
				popupNotice.InitSystemPopUp(title, errorMessage, false, this.RebootApplication);
				popupNotice.SetButtonText(1, buttonTitle);
			}
		});

		yield return new WaitUntil(() => isComplete);
#endif
	}

	private IEnumerator CoShowMajorNotice()
	{
#if UNITY_EDITOR
		yield return null;
#else
        bool isComplete = false;

		ManagerNotice.instance.ShowMajorNoticeForLogin(
			() => { isComplete = true; }, fail => OnCheckSystemStateFail(fail));

		yield return new WaitUntil(() => isComplete);
#endif
	}
	
	private void OnCheckSystemStateFail(Trident.Error error)
	{
		string title        = Global._instance.GetString("p_t_4");
		string buttonTitle  = Global._instance.GetString("btn_1");
		string errorMessage = Global._instance.GetSystemNetworkErrorAndRebootString(error.getCode(), error.getMessage());
		
		UIPopupSystem popupNotice = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
		popupNotice.InitSystemPopUp(title, errorMessage, false, RebootApplication);
		popupNotice.SetButtonText(1, buttonTitle);
		popupNotice.HideCloseButton();
	}

	/// <summary>
	/// LINE_LONGIN 전략 반환
	/// </summary>
	/// <returns>에디터, 플랫폼 동작 전략 반환</returns>
	private BaseLoginStrategy GetLoginStrategy()
	{
#if UNITY_EDITOR
		return new LoginStrategyInEditor(this);
#else
        return new LoginStrategyInPlatform(this);
#endif
	}
	
	/// <summary>
	/// CONNECTING 전략 반환
	/// </summary>
	/// <returns>에디터, 플랫폼 동작 전략 반환</returns>
	private BaseConnectingStrategy GetConnectingStrategy()
	{
#if UNITY_EDITOR
		return new ConnectingStrategyInEditor();
#else
        return new ConnectingStrategyInPlatform();
#endif
	}

	private void RebootApplication()
	{
		Global.ReBoot();
	}
	
	private IEnumerator CoLaunchProcess()
	{
		Global._onlineMode = true;
		GameData.Reset();
		
		if (NetworkSettings.Instance.serverTarget != NetworkSettings.ServerTargets.Pub_LiveServer &&
		    NetworkSettings.Instance.serverTarget != NetworkSettings.ServerTargets.Pub_QAServer)
		{
			var popup = ManagerUI._instance.OpenPopupServerSelect();
			while (popup)
			{
				yield return null;
			}
		}
		else
		{
			yield return null;
		}
	}
	
	/// <summary>
	/// iOS 환경에서만 앱 변경 기능 동작
	/// </summary>
	private IEnumerator CoAppIconChange()
	{
		if (AppIconChanger.iOS.SupportsAlternateIcons)
		{
			// 앱 변경 기능 ON/OFF
            if (ServerContents.AppIconVer > 0)
			{
				if (string.Equals(PlayerPrefs.GetString(APP_ICON_VERSION),ServerContents.AppIconVer.ToString()) == false)
				{
                    if (string.IsNullOrEmpty(AppIconChanger.iOS.AlternateIconName) == false)
                    {
                        AppIconChanger.iOS.SetAlternateIconName(null);
                    }
                    
					PlayerPrefs.SetString(APP_ICON_VERSION, ServerContents.AppIconVer.ToString());
					var popup = ManagerUI._instance.OpenPopup<UIPopupChangeAppIcon>();
					yield return new WaitWhile(() => popup != null);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(AppIconChanger.iOS.AlternateIconName) == false)
				{
					AppIconChanger.iOS.SetAlternateIconName(null);
					
					var popup = ManagerUI._instance.OpenPopup<UIPopupChangeAppIcon>();
					yield return new WaitWhile(() => popup != null);
				}
			}
		}
		
		yield return null;
	}
}