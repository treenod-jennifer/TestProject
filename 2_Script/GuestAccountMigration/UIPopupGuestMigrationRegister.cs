using Protocol;
using UnityEngine;

/// <summary>
/// 게스트 계정 마이그레이션 - ID 생성
/// </summary>
public class UIPopupGuestMigrationRegister : UIPopupGuestMigrationBase
{
    public static UIPopupGuestMigrationRegister instance = null;

    private void Awake()
    {
        instance = this;
    }

    private new void OnDestroy()
    {
        instance = null;
        base.OnDestroy();
    }

    public void InitPopup()
    {
        SetTitleLabel();
        SetSubTitleLabel("guset_data_3");
        
        SetDescriptionLabel("guset_data_4");
        SetPrecautionsLabel("guset_data_11");
        
        SetIdHeadLabel();
        
        SetRegisterButtonLabel();
        SetCloseButtonLabel();
    }
    
    private void OnClickBtnMigrationRegister()
    {
        if (bCanTouch == false)
        {
            return;
        }

        bCanTouch = false;
        ServerAPI.RegisterGuestAccountMigrationId(GetIdLabel(), OnResponseMigrationRegister);
    }
    
    /// <summary>
    /// ID 등록 서버 통신 완료 > 임시 ID, PW 확인 팝업 출력 or ID 생성 실패 팝업 출력
    /// </summary>
    private void OnResponseMigrationRegister(RegisterGuestAccountMigrationIdResp resp)
    {
        bCanTouch = true;
        
        if (resp.IsSuccess)
        {
            if (UIPopupGuestMigrationRegisterComplete.instance == null)
            {
                OnClickBtnClose();
                
                UIPopupGuestMigrationRegisterComplete popup = ManagerUI._instance.OpenPopup<UIPopupGuestMigrationRegisterComplete>();
                popup.InitPopup(GetIdLabel(), resp.password);
            }
        }
        else if (resp.Error == (int)ServerError.GuestMigrationError)
        {
            if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
            {
                Debug.Log("[GuestMigrationRegisterFail] " + resp.error);
            }

            string message = Global._instance.GetString("p_guset_data_1")  + "\n" + GetErrorCodeStringKey(resp.error);

            UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
            popupWarning.HideCloseButton();
        }
    }
}