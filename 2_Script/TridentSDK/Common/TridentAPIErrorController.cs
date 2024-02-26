public static class TridentAPIErrorController
{
	private enum HTTPResponseCode
	{
		NetworkError = 0,
		Success      = 200,
	}

	public static void OpenErrorPopup(int code, string message)
	{
		if (code == (int)HTTPResponseCode.NetworkError)
		{
			UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
			popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false);
			popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
		}
		else
		{
			string        textWarning  = Global._instance.GetSystemErrorString(message, "");
			UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
			popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), textWarning, false, null, isError: true);
			popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
		}
	}

	public static void OpenErrorPopupAndReboot(int code, string message)
	{
		if (code == (int)HTTPResponseCode.NetworkError)
		{
			UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
			popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, Global.ReBoot);
			popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
		}
		else
		{
			string        textWarning  = Global._instance.GetSystemErrorString(message, "");
			UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
			popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), textWarning, false, Global.ReBoot, isError: true);
			popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
		}
	}
}