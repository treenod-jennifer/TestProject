using UnityEngine;

/// <summary>
/// 게스트 계정 마이그레이션 - ID 생성 완료
/// </summary>
public class UIPopupGuestMigrationRegisterComplete : UIPopupGuestMigrationBase
{
    public static UIPopupGuestMigrationRegisterComplete instance = null;

    [SerializeField] private UILabel[] copyButtonLabel;
    
    private void Awake()
    {
        instance = this;
    }

    private new void OnDestroy()
    {
        instance = null;
        base.OnDestroy();
    }

    public void InitPopup(string id, string password)
    {
        SetTitleLabel();
        SetSubTitleLabel("guset_data_5");
        
        SetPrecautionsLabel("guset_data_8");
        
        SetIdHeadLabel();
        SetPasswordHeadLabel();
        SetIdLabel(id);
        SetPasswordLabel(password);

        SetCopyButtonLabel();
        SetCloseButtonLabel();
    }

    private void SetCopyButtonLabel()
    {
        if (copyButtonLabel != null && copyButtonLabel.Length > 0)
        {
            for (int i = 0; i < copyButtonLabel.Length; i++)
            {
                copyButtonLabel[i].text = Global._instance.GetString("btn_94");
            }
        }
    }
    
    private void OnClickBtnPasswordCopy()
    {
        if (bCanTouch == false)
        {
            return;
        }
        
        if (!string.IsNullOrEmpty(GetPasswordLabel()))
        {
            GUIUtility.systemCopyBuffer = GetPasswordLabel();

            UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_guset_data_5"), false);
            popupWarning.HideCloseButton();
        }
    }
}