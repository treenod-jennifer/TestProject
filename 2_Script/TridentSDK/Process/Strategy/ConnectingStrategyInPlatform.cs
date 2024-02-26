using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

public class ConnectingStrategyInPlatform : BaseConnectingStrategy
{
	private bool isCompleteGuestMigration = false;
	
	public override void SetGrowthyIdAndTrackEvent()
	{
		ServiceSDKManager.instance.SetupGrowthyService();
	}

	public override void InitializeBilling()
	{
		ServiceSDKManager.instance.BillingServiceInitialize();
	}
	
	public override void InitializePromotion()
	{
		ManagerData._instance.InitializePromotionSDK();
	}
	
	public override void RegisterDeviceTokenForPushService()
	{
		ServiceSDK.ServiceSDKManager.instance.RegisterDeviceTokenForPush();
	}
	
	public override IEnumerator CoSetUserProfile()
	{
		if (ServiceSDKManager.instance.IsGuestLogin())
		{
			ProfileManager.SetGuestUserProfile();
			yield break;
		}
		
		bool isComplete = false;
		ProfileManager.LoadMyProfile(() => { isComplete = true; });
		
		yield return new WaitUntil(() => isComplete);
	}

	public override IEnumerator CoSetGameFriends()
	{
		bool isComplete = false;
		yield return ProfileManager.LoadPlayingLineFriendList(() =>
		{
			isComplete     = true;
		});
		
		yield return new WaitUntil(() => isComplete);
		ManagerData._instance._state = DataLoadState.eLineFriendsData;
		
		yield return ProfileManager.InitGameFriendData();
	}

	public override IEnumerator CoSetNonGameFriends()
	{
		bool isComplete = false;
		yield return ProfileManager.LoadNonPlayLineFriendList(() =>
		{
			isComplete  = true;
		});
		
		yield return new WaitUntil(() => isComplete);
		ManagerData._instance._state = DataLoadState.eLineInviteFriendsData;
	}
	
	public override IEnumerator CoSetAllProfile()
	{
		var  userKeys   = ProfileManager.GetPlayingLineFriendKeys();
		userKeys.Add(ProfileManager.GetMyProfile()._userKey);
		
		bool isComplete = false;
		yield return ProfileManager.GetAllProfileList((profileData) =>
		{
			isComplete = true;
			OnGetGameProfileDataHandler(profileData);
		}, userKeys.ToArray());
		
		yield return new WaitUntil(() => isComplete);
	}

	private void OnGetGameProfileDataHandler(List<Profile_PION> profileData)
	{
		for (int i = 0; i < profileData.Count; i++)
		{
			ProfileManager.AddUsersProfile(profileData[i].userKey, profileData[i]);
		}
	}
	
	public override IEnumerator CoAntiTamperingInLiapp()
	{
		if (NetworkSettings.Instance.enableLiAppSecureTool) 
		{
			ServiceSDKManager.instance.StartLiapp();
			ServiceSDKManager.instance.SendUserIDinLiapp();
			
			yield return new WaitUntil(() => UIPopupSystem._instance == null);
		}
		else 
		{
			Debug.LogWarning("Diabled Line Liapp Tool (only debugging)...");
			yield return null;
		}
	}
	
	public override IEnumerator CoShowMinorNotice() 
	{
		bool isComplete = false;
        
		ManagerNotice.instance.ShowMinorNotice(() =>
		{
			isComplete = true;
		});
        
		yield return new WaitUntil(() => isComplete);
	}
	
	public override IEnumerator CoGetPurchasesList()
	{
		bool isComplete = false;
		
		ServiceSDKManager.instance.GetProductInfo((b) =>
		{
			isComplete = true;
		});
		
		yield return new WaitUntil(() => isComplete);
	}
	
	public override IEnumerator CoGuestMigration()
	{
		// iOS 디바이스에서 게스트 로그인 최초 접속일 때, 게스트 로그인 데이터 이전 플로우 진행.
#if UNITY_IOS
		if (Global.join == true && ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
		{
			isCompleteGuestMigration = false;
			ConfirmToGuestMigrationPopup();
			yield return new WaitUntil(() => isCompleteGuestMigration);
		}
		else
		{
			yield return null;
		}
#else
		yield return null;
#endif
	}

	private void ConfirmToGuestMigrationPopup()
	{
		// 유저한테 게스트 계정을 이전할지 의사요청.
		UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
		popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_guset_data_2"), true);
		popupSystem.SetButtonText(2, Global._instance.GetString("btn_1"));
		popupSystem.SetButtonText(1, Global._instance.GetString("btn_2"));
		popupSystem.SetCallbackSetting(2, OnConfirmToGuestMigrationYes, true);
		popupSystem.SetCallbackSetting(1, OnConfirmToGuestMigrationClose);
		popupSystem.SetCallbackSetting(4, OnConfirmToGuestMigrationClose);
		popupSystem.HideCloseButton();
		popupSystem.useBlockClick = true;
	}
    
	private void OnConfirmToGuestMigrationYes()
	{
		if (UIPopupGuestMigrationStart.instance == null)
		{
			UIPopupGuestMigrationStart popup = ManagerUI._instance.OpenPopup<UIPopupGuestMigrationStart>();
			popup.InitPopup(OnConfirmToGuestMigrationClose);
		}
	}
    
	private void OnConfirmToGuestMigrationClose()
	{
		isCompleteGuestMigration = true;
	}
}
