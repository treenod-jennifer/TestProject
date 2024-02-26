using System;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using ServiceSDK;
using Trident;
using UnityEngine;

public class LoginStrategyInPlatform : BaseLoginStrategy
{
	// PlayerPrefs 에 저장되는 GuestData 중 int형의 value값을 저장하는 키 리스트
	private readonly List<string> listGuestDataKey_int = new List<string>()
	{
		"com.linecorp.trident.kcs.ismigrated",
		"trident.growthy.userState"
	};

	// PlayerPrefs 에 저장되는 GuestData 중 string형의 value값을 저장하는 키 리스트
	private readonly List<string> listGuestDataKey_string = new List<string>()
	{
		"com.linecorp.trident.kcs.ver",
		"com.linecorp.trident.uuid",
		"com.linecorp.trident.accesstoken",
		"com.linecorp.trident.accesstoken.expiretime",
		"com.linecorp.trident.providerid",
		"com.linecorp.trident.userkey",
		"guest_auth_marker",
		"guest_reset_tm",
		"com.linecorp.trident.push.signature1",
		"com.linecorp.trident.push.signature2"
	};
	
	private bool   isAcceptedUserAgreement;
	private Error  signInError;
	
	public override bool IsShowStartButton  => SceneTitle._showStartButton && ServiceSDKManager.isRebootSignIn == false;
	public override bool IsDuplicateLogin => ServiceSDKManager.isRebootSignIn == false || ServiceSDKManager.isDuplicateLogin;
	
	public LoginStrategyInPlatform(ProcessManager processManager) : base(processManager)
	{
	}

	public override void SetCallbackAndHandler()
	{
		// 인증 결과를 받아올 콜백 설정 (콜백에서 UserKey 설정)
		ServiceSDKManager.instance.SetCredentialsChangedCallbackForAuth(SetInfoAfterAuth);
		// Guest 사용자의 초기화를 실행하기 위한 콜백 설정 (라인측에서 게스트 유저 리셋 없이 진행해달라고 요청)
		ServiceSDKManager.instance.SetAuthResetGuestUserHandlerForAuth(false);
		//signIn API를 사용하여 인증 정보를 취득할 때, 이전에 한 번이라도 Guest 인증이 아닌 다른 인증 타입으로 사용자 인증 정보를 연동한 적이 있는 사용자일 경우 Trident SDK에서 호출되는 콜백
		ServiceSDKManager.instance.SetAuthUserDataMigrationOptionHandlerForAuth(OpenPopupConfirmToUserDataMigration);
	}

	private void SetInfoAfterAuth(bool isSignedIn, string userKey, AuthProvider providerId)
	{
		// 원래라면 이 콜백이 들어왔을때 서버에서 authorization을 하지만 포코퍼즐은 게임로그인때 authorization함
	}

	public override void CheckAuthentication()
	{
		string token = ServiceSDKManager.instance.GetAccessToken();
		IsValidAccessToken = string.IsNullOrEmpty(token) == false;
	}

	public override void RefreshToken(Action onRefreshTokenError = null)
	{
		ServiceSDKManager.instance.Refresh((isSuccess, error) =>
		{
			IsSigningIn = false;
			if (isSuccess)
			{
				IsSignedIn = true;
			}
			else
			{
				onRefreshTokenError?.Invoke();
			}
		});
	}
	
	/// <summary>
	/// 최초 로그인 시도 (이용 약관 출력 후 로그인)
	/// </summary>
	public override IEnumerator CoLogin()
	{
		IsSigningIn = true;

		// 이용 약관 출력
		yield return processManager.StartCoroutine(CoCheckUserAgreement());

		// 사용자 이용약관에 동의하지 않은 경우 중단 (로그인 버튼 입력을 통해 재진행 가능)
		if (!isAcceptedUserAgreement)
		{
			IsSigningIn = false;
			
			// 기존에는 이용약관에 동의하지 않을 경우 에러 코드를 리턴했지만 V3에는 에러코드를 리턴하지 않기 때문에 임의로 V2에서 사용하던 에러 코드를 사용해서 팝업 출력 
			OpenPopupWarning(-61954, "");
			yield break;
		}

		yield return processManager.StartCoroutine(CoSignIn());
	}

	/// <summary>
	/// 게스트 or 라인 로그인 선택 팝업 출력 (게스트 로그인 버튼 클릭 시)
	/// </summary>
	public override void ShowGuestLoginMessage()
	{
		UIPopupSystem popupGuide = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
		popupGuide.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_lg_1"), true,
			null);
		popupGuide.SetButtonText(1, Global._instance.GetString("btn_60"));
		popupGuide.SetButtonText(2, Global._instance.GetString("btn_61"));
		popupGuide.SetCallbackSetting(1, OpenPopupGuestDataGuide, true);
		popupGuide.SetCallbackSetting(2, LineLogin, true);
		popupGuide.HideCloseButton();
		popupGuide.useBlockClick = true;
	}

	public override void CheckPidAndDeletePlayerPrefs()
	{
		if (ServiceSDKManager.instance.GetProvideType() == AuthProvider.AuthProviderLINE)
		{
			if (Global.isChangeUserData)
			{
				DeletePlayerPrefsWithOutGuestData();
			}

			Global.isChangeUserData = false;
			CheckLastLoginPid();
		}
		else
		{
			CheckLastLoginPid();
			AdjustManager.instance.OnRegistration_Guest();
		}
	}

	/// <summary>
	/// 이용 약관 출력
	/// </summary>
	private IEnumerator CoCheckUserAgreement()
	{
		bool isComplete = false;

		ServiceSDKManager.instance.CheckUserAgreement((isAccepted) =>
		{
			isComplete              = true;
			isAcceptedUserAgreement = isAccepted;
		});

		yield return new WaitUntil(() => isComplete);
	}

	private IEnumerator CoSignIn()
	{
		bool isComplete = false;
		signInError = null;

		ServiceSDKManager.instance.SignIn(AuthProvider, (isSuccess, error) =>
		{
			isComplete = true;

			if (!isSuccess)
			{
				signInError = error;
				IsSigningIn = false;
			}
			else
			{
				IsSignedIn = true;
			}
		});

		yield return new WaitUntil(() => isComplete);

		if (signInError != null && signInError.hasError())
		{
			yield return processManager.StartCoroutine(CoHandlingSignInError());
		}
	}

	private IEnumerator CoHandlingSignInError()
	{
		string errorCode16 = System.Convert.ToString(signInError.getCode() * -1, 16);
		if (errorCode16.Equals("f221"))
		{
			Extension.PokoLog.Log("===============AuthManager SignIn Error f221, signOut");
			yield return processManager.StartCoroutine(CoSignOut());
		}
		else
		{
			OpenPopupWarning(signInError.getCode(), signInError.getMessage());
		}
	}

	private IEnumerator CoSignOut()
	{
		bool isComplete = false;

		ServiceSDKManager.instance.SignOut((isSuccess, error) =>
		{
			isComplete = true;

			if (!isSuccess)
			{
				OpenPopupWarning(error.getCode(), error.getMessage());
			}
		});

		yield return new WaitUntil(() => isComplete);
	}

	/// <summary>
	/// 게스트 로그인 진행시 데이터가 복구되지 않을 수 있다는 경고 팝업 출력
	/// </summary>
	private void OpenPopupGuestDataGuide()
	{
		UIPopupSystem popupGuide = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
		popupGuide.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_lg_2"), true,
			null);
		popupGuide.SetButtonText(1, Global._instance.GetString("btn_2"));
		popupGuide.SetButtonText(2, Global._instance.GetString("btn_1"));
		popupGuide.SetCallbackSetting(2, GuestLogin, true);
		popupGuide.HideCloseButton();
		popupGuide.useBlockClick = true;
	}

	/// <summary>
	/// 게스트 로그인에서 라인로그인으로 전환시 유저한테 이전계정을 연동할지 새로할지 의사요청
	/// </summary>
	private void OpenPopupConfirmToUserDataMigration()
	{
		// 유저한테 이전계정을 연동할지 새로할지 의사요청
		UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
		popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_12"), true);
		popupWarning.SetButtonText(1, Global._instance.GetString("btn_23"));
		popupWarning.SetButtonText(2, Global._instance.GetString("btn_24"));
		popupWarning.SetCallbackSetting(1, OnConfirmToUserDataMigrationYes);
		popupWarning.SetCallbackSetting(2, OnConfirmToUserDataMigrationNo);
		popupWarning.SetCallbackSetting(3, OnConfirmToUserDataMigrationCancel);
		popupWarning.SetCallbackSetting(4, OnConfirmToUserDataMigrationCancel);
		popupWarning.useBlockClick = true;
	}

	private void OpenPopupWarning(int errorCode, string text)
	{
		string textWarning = text;
		string okBtnText   = Global._instance.GetString("btn_1");
		bool   closeBtn    = true;

		Global._instance.GetSystemErrorString(errorCode, text, ref textWarning, ref okBtnText, ref closeBtn);

		UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
		popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), textWarning, false, null, isError: true);
		popupWarning.SetButtonText(1, okBtnText);
		if (closeBtn == false)
		{
			popupWarning.HideCloseButton();
		}
	}

	private void OnConfirmToUserDataMigrationYes()
	{
		ServiceSDKManager.instance.ConfirmToUserDataMigration(AuthUserDataMigrationOption.AuthMigrateCurrentUserData);
	}

	private void OnConfirmToUserDataMigrationNo()
	{
		Global.isChangeUserData = true;
		ServiceSDKManager.instance.ConfirmToUserDataMigration(AuthUserDataMigrationOption.AuthLoadExistingUserData);
	}

	private void OnConfirmToUserDataMigrationCancel()
	{
		ServiceSDKManager.instance.ConfirmToUserDataMigration(AuthUserDataMigrationOption.AuthMigrationUserCancel);
	}

	private void LineLogin()
	{
		AuthProvider = AuthProvider.AuthProviderLINE;
		processManager.StartCoroutine(CoLogin());
	}

	private void GuestLogin()
	{
		AuthProvider = AuthProvider.AuthProviderGuest;
		processManager.StartCoroutine(CoLogin());
	}

	/// <summary>
	/// 이전 로그인한 정보와 현재 로그인한 정보가 다를 경우, PlayerPrefs에 있는 정보 초기화시켜줌.
	/// 현재 로그인한 pid 저장
	/// </summary>
	private void CheckLastLoginPid()
	{
		if (Global.lastLogin_pid != "" &&
		    Global.lastLogin_pid.Equals(ServiceSDKManager.instance.GetUserKey()) == false)
		{
			DeletePlayerPrefsWithOutGuestData();
		}

		Global.lastLogin_pid = ServiceSDKManager.instance.GetUserKey();
	}

	/// <summary>
	/// 게스트 데이터 외의 PlayerPrefs에 있는 데이터를 제거하는 함수
	/// </summary>
	private void DeletePlayerPrefsWithOutGuestData()
	{
		List<int?>   listData_int    = new List<int?>();
		List<string> listData_string = new List<string>();

		//현재 PlayerPrefs에 저장된 게스트 관련 데이터값 백업
		for (int i = 0; i < listGuestDataKey_int.Count; i++)
			listData_int?.Add(Global.GetIntByPlayerPrefs(listGuestDataKey_int[i]));
		for (int i = 0; i < listGuestDataKey_string.Count; i++)
			listData_string?.Add(Global.GetStringByPlayerPrefs(listGuestDataKey_string[i]));

		//PlayerPrefs에 저장되어 있던 모든 데이터 삭제
		PlayerPrefs.DeleteAll();

		//백업해둔 게스트 관련 데이터 다시 추가
		for (int i = 0; i < listGuestDataKey_int.Count; i++)
		{
			if (listData_int[i].HasValue == true)
				PlayerPrefs.SetInt(listGuestDataKey_int[i], listData_int[i].Value);
		}

		for (int i = 0; i < listGuestDataKey_string.Count; i++)
		{
			if (string.IsNullOrEmpty(listData_string[i]) == false)
				PlayerPrefs.SetString(listGuestDataKey_string[i], listData_string[i]);
		}
	}
}