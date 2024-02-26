using System.Collections;
using UnityEngine;
using Trident;

public class LoginProcess
{
	private ProcessManager    processManager;
	private BaseLoginStrategy strategy;

	public LoginProcess(ProcessManager processManager, BaseLoginStrategy strategy)
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

		strategy.SetCallbackAndHandler();

		strategy.CheckAuthentication();
		
		if (strategy.IsValidAccessToken)
		{
			if (strategy.IsShowStartButton)
			{
				SetLoginButton(SceneTitleBtnContainer.BtnContainerState.Start);
			}
			else
			{
				if (strategy.IsDuplicateLogin)
				{
					ServiceSDK.ServiceSDKManager.isDuplicateLogin = false;
					
					// 토큰 갱신에 실패 할 경우 라인&게스트 로그인 버튼 출력
					strategy.RefreshToken(() =>
					{
						SetLoginButton(SceneTitleBtnContainer.BtnContainerState.LineAndGuest);
					});
				}
				else
				{
					SetLoginButton(SceneTitleBtnContainer.BtnContainerState.LineAndGuest);
				}
			}
		}
		else
		{
			SetLoginButton(SceneTitleBtnContainer.BtnContainerState.LineAndGuest);
		}

		// 로그인 프로세스가 완료될 때 까지 대기 (로그인 버튼 생성의 경우 버튼 클릭을 통해 로그인 프로세스 실행)
		yield return new WaitUntil(() => strategy.IsSignedIn);

		strategy.CheckPidAndDeletePlayerPrefs();

		CompleteLoginProcess();
	}

	private void SetLoginButton(SceneTitleBtnContainer.BtnContainerState state)
	{
		if (SceneTitle.instance == null) return;
		
		switch (state)
		{
			case SceneTitleBtnContainer.BtnContainerState.None:
				SceneTitle.instance.SetLoginButton(SceneTitleBtnContainer.BtnContainerState.None, null);
				break;
			case SceneTitleBtnContainer.BtnContainerState.Start:
				SceneTitle.instance.SetLoginButton(SceneTitleBtnContainer.BtnContainerState.Start, OnClickStartButton);
				break;
			case SceneTitleBtnContainer.BtnContainerState.LineAndGuest:
				SceneTitle.instance.SetLoginButton(state, (bool isGuestLogin) =>
				{
					if (isGuestLogin)
					{
						OnClickGuestLoginButton();
					}
					else
					{
						OnClickLineLoginButton();
					}
				});
				break;
		}
	}

	private void OnClickStartButton(bool isGuest)
	{
		if (strategy.IsSigningIn) return;

		// 리프래시 토큰, Reboot 시에도 토큰 유효성 검사를 진행하도록
		strategy.RefreshToken(() =>
		{
			SetLoginButton(SceneTitleBtnContainer.BtnContainerState.LineAndGuest);
		});
	}

	private void OnClickLineLoginButton()
	{
		if (strategy.IsSigningIn) return;

		strategy.AuthProvider = AuthProvider.AuthProviderLINE;
		processManager.StartCoroutine(strategy.CoLogin());
	}

	private void OnClickGuestLoginButton()
	{
		if (strategy.IsSigningIn) return;

		// 기존이 게스트 로그인이라면 그냥 넘어감
		if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
		{
			strategy.RefreshToken();
			return;
		}

		strategy.AuthProvider = AuthProvider.AuthProviderGuest;
		strategy.ShowGuestLoginMessage();
	}

	private void CompleteLoginProcess()
	{
		SceneTitle._showStartButton                   = false;
		ServiceSDK.ServiceSDKManager.isRebootSignIn   = false;
		ServiceSDK.ServiceSDKManager.isDuplicateLogin = false;
		
		SetLoginButton(SceneTitleBtnContainer.BtnContainerState.None);
	}
}