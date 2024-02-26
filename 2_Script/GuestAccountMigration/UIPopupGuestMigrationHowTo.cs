/// <summary>
/// 게스트 계정 마이그레이션 - 데이터 이전 안내 팝업
/// </summary>
public class UIPopupGuestMigrationHowTo : UIPopupGuestMigrationBase
{
    public static UIPopupGuestMigrationHowTo instance = null;

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
        SetSubTitleLabel("guset_data_1");
        
        SetDescriptionLabel("guset_data_2");
        
        SetRegisterButtonLabel();
        SetStartButtonLabel();
        SetCloseButtonLabel();
    }

    private void OnClickBtnOpenMigrationRegister()
    {
        if (bCanTouch == false)
        {
            return;
        }

        if (UIPopupGuestMigrationRegister.instance == null)
        {
            OnClickBtnClose();
            
            UIPopupGuestMigrationRegister popup = ManagerUI._instance.OpenPopup<UIPopupGuestMigrationRegister>();
            popup.InitPopup();
        }
    }

    private void OnClickBtnOpenStartMigration()
    {
        if (bCanTouch == false)
        {
            return;
        }

        if (UIPopupGuestMigrationStart.instance == null)
        {
            OnClickBtnClose();
            
            UIPopupGuestMigrationStart popup = ManagerUI._instance.OpenPopup<UIPopupGuestMigrationStart>();
            popup.InitPopup();
        }
    }
}