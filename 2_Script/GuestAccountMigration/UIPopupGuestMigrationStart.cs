using Protocol;
using ServiceSDK;
using Trident;
using UnityEngine;

/// <summary>
/// 게스트 계정 마이그레이션 - 데이터 이전 시작
/// </summary>
public class UIPopupGuestMigrationStart : UIPopupGuestMigrationBase
{
    public static UIPopupGuestMigrationStart instance = null;

    private System.Action onClosePopup;

    private void Awake()
    {
        instance = this;
    }

    private new void OnDestroy()
    {
        instance = null;
        base.OnDestroy();
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
        {
            return;
        }
        bCanTouch = false;
        
        if (onClosePopup != null)
        {
            onClosePopup.Invoke();
            onClosePopup = null;
        }

        ManagerUI._instance.ClosePopUpUI();
    }

    public void InitPopup(System.Action onClose = null)
    {
        SetTitleLabel();
        SetSubTitleLabel("guset_data_9");
        
        SetDescriptionLabel("guset_data_10");
        SetPrecautionsLabel("guset_data_12");
        
        SetIdHeadLabel();
        SetPasswordHeadLabel();
        
        SetStartButtonLabel();
        SetCloseButtonLabel();

        if (onClose != null)
        {
            onClosePopup = onClose;
        }
    }

    private void OnClickBtnStartMigration()
    {
        if (bCanTouch == false)
        {
            return;
        }
        bCanTouch = false;
        
        string id       = GetIdLabel().Replace(" ", "");
        string password = GetPasswordLabel().Replace(" ", "");
        
        // 공백 제거 후 반영.
        SetIdInputField(id); 
        SetPasswordInputField(password);
        
        ServerAPI.RequestGuestAccountMigration(id, password, OnResponseMigration);
    }

    /// <summary>
    /// 데이터 이전 서버 통신 완료 > 이전 완료 및 재로그인(로그아웃 처리와 동일하게) or 이전 실패 팝업 출력
    /// </summary>
    private void OnResponseMigration(BaseResp resp)
    {
        bCanTouch = true;
        
        if (resp.IsSuccess)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_guset_data_3"), false);
            popupSystem.FunctionSetting(1, "CompleteMigrationAndReboot", gameObject);
            popupSystem.FunctionSetting(4, "CompleteMigrationAndReboot", gameObject);
            popupSystem.HideCloseButton();
            popupSystem.useBlockClick = true;
        }
        else if (resp.Error == (int)ServerError.GuestMigrationError)
        {
            if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
            {
                Debug.Log("[GuestMigrationStartFail] " + resp.error);
            }

            string message = Global._instance.GetString("p_guset_data_4") + "\n" + GetErrorCodeStringKey(resp.error);

            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
            popupSystem.HideCloseButton();
            popupSystem.useBlockClick = true;
        }
    }

    /// <summary>
    /// 토큰 값 갱신 후 재로그인
    /// </summary>
    private void CompleteMigrationAndReboot()
    {
        bCanTouch = false;
        ServiceSDKManager.instance.Refresh(OnRefreshComplete);
    }

    private void OnRefreshComplete(bool arg1, Error arg2)
    {
        Global.isChangeUserData = true;
        Global.ReBoot();
    }
}