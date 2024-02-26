using System;
using System.Collections;
using Trident;

public abstract class BaseLoginStrategy
{
	public bool         IsValidAccessToken { get;           protected set; }
	public bool         IsSigningIn        { get;           protected set; }
	public bool         IsSignedIn         { get;           protected set; }
	public AuthProvider AuthProvider       { protected get; set; }
	
	public virtual bool IsShowStartButton => false;
	public virtual bool IsDuplicateLogin  => false;
	
	protected ProcessManager processManager;

	protected BaseLoginStrategy(ProcessManager processManager)
	{
		this.processManager = processManager;
	}

	/// <summary>
	/// 유효한 인증정보가 있는지 확인
	/// </summary>
	public abstract void CheckAuthentication();

	/// <summary>
	/// AccessToken 재발급
	/// </summary>
	public virtual void RefreshToken(Action onRefreshTokenError = null)
	{
	}

	/// <summary>
	/// 콜백 및 핸들러 설정
	/// </summary>
	public virtual void SetCallbackAndHandler()
	{
	}

	/// <summary>
	/// 인증 타입에 따른 로그인 처리
	/// </summary>
	public virtual IEnumerator CoLogin()
	{
		yield break;
	}

	/// <summary>
	/// 게스트 유저 로그인 처리
	/// </summary>
	public virtual void ShowGuestLoginMessage()
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void CheckPidAndDeletePlayerPrefs()
	{
	}

}