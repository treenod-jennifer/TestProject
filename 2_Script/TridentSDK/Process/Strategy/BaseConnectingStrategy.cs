using System.Collections;
using UnityEngine;

public abstract class BaseConnectingStrategy
{
	protected SDKGameProfileManager ProfileManager => SDKGameProfileManager._instance;
	
	/// <summary>
	/// ProfileManager 생성
	/// </summary>
	public IEnumerator CoInitializeProfileManager()
	{
		if (SDKGameProfileManager._instance != null)
		{
			SDKGameProfileManager.DestroySelf();
		}
		yield return new WaitUntil(() => SDKGameProfileManager._instance == null);
        
		SDKGameProfileManager.Initialized();
		
		yield return new WaitUntil(() => SDKGameProfileManager._instance != null);
	}
	
	/// <summary>
	/// 트라이던트 SDK 빌링 초기화
	/// </summary>
	public virtual void InitializeBilling() { }
	/// <summary>
	/// 그로시 초기화 및 시작
	/// </summary>
	public virtual void SetGrowthyIdAndTrackEvent() { }
	/// <summary>
	/// 트라이던트 SDK 프로모션 초기화
	/// </summary>
	public virtual void InitializePromotion() { }
	/// <summary>
	/// push 알림 토큰 등록
	/// </summary>
	public virtual void RegisterDeviceTokenForPushService() { }
	/// <summary>
	/// 앱 위변조 방지 (Liapp)
	/// </summary>
	public virtual IEnumerator CoAntiTamperingInLiapp() { yield break; }
	/// <summary>
	/// 노티스 출력 (강제 업데이트, 임의 업데이트, 점검을 제외한 공지 내용)
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator CoShowMinorNotice() { yield break; }
	/// <summary>
	/// 유저 정보 설정 (DisplayName, PictureURL)
	/// </summary>
	public abstract IEnumerator CoSetUserProfile();
	/// <summary>
	/// 게임을 플레이하는 친구 정보 설정 (UserKey, DisplayName, PictureURL)
	/// </summary>
	public virtual IEnumerator CoSetGameFriends() { yield break; }
	/// <summary>
	/// 게임을 플레이하지 않는 친구 정보 설정 (UserKey, DisplayName, PictureURL)
	/// </summary>
	public virtual IEnumerator CoSetNonGameFriends() { yield break; }
	/// <summary>
	/// 프로필 정보 설정(유저 + 친구)
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator CoSetAllProfile() { yield break; }
	/// <summary>
	/// 상품 정보 취득
	/// </summary>
	public virtual IEnumerator CoGetPurchasesList() { yield break; }
	/// <summary>
	/// 게스트 로그인 데이터 이전
	/// </summary>
	public virtual IEnumerator CoGuestMigration() { yield break; }
}