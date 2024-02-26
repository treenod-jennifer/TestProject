using System;

public class LoginStrategyInEditor : BaseLoginStrategy
{
	public LoginStrategyInEditor(ProcessManager processManager) : base(processManager)
	{
	}
	
	public override bool IsDuplicateLogin => true;

	public override void CheckAuthentication()
	{
		IsValidAccessToken = true;
	}
	
	public override void RefreshToken(Action onRefreshTokenError = null)
	{
		IsSignedIn = true;
	}
}