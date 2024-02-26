using UnityEngine;

public class UIPopupGuestMigrationBase : UIPopupBase
{
    [SerializeField] private UIInput idInputField;
    [SerializeField] private UIInput passwordInputField;
    
    [SerializeField] private UILabel idHeaderLabel;
    [SerializeField] private UILabel idLabel;
    [SerializeField] private UILabel passwordHeaderLabel;
    [SerializeField] private UILabel passwordLabel;

    [SerializeField] private UILabel[] titleLabels;
    [SerializeField] private UILabel   subTitleLabel;
    [SerializeField] private UILabel   descriptionLabel;
    [SerializeField] private UILabel   precautionsLabel;

    [SerializeField] private UILabel[] registerButtonLabel;
    [SerializeField] private UILabel[] startButtonLabel;
    [SerializeField] private UILabel[] closeButtonLabel;

    protected void SetTitleLabel()
    {
        if (titleLabels == null || titleLabels.Length <= 0) return;
        
        for (int i = 0; i < titleLabels.Length; i++)
        {
            if (titleLabels[i] != null)
            {
                titleLabels[i].text = Global._instance.GetString("t_guest_data_1");
            }
        }
    }
    
    protected void SetIdHeadLabel()
    {
        if (idHeaderLabel != null)
        {
            idHeaderLabel.text = Global._instance.GetString("guset_data_6");
        }
    }
    
    protected void SetPasswordHeadLabel()
    {
        if (passwordHeaderLabel != null)
        {
            passwordHeaderLabel.text = Global._instance.GetString("guset_data_7");
        }
    }
    
    protected void SetRegisterButtonLabel()
    {
        if (registerButtonLabel == null || registerButtonLabel.Length <= 0) return;
        
        for (int i = 0; i < registerButtonLabel.Length; i++)
        {
            if (registerButtonLabel[i] != null)
            {
                registerButtonLabel[i].text = Global._instance.GetString("btn_91");
            }
        }
    }
    
    protected void SetStartButtonLabel()
    {
        if (startButtonLabel == null || startButtonLabel.Length <= 0) return;
        
        for (int i = 0; i < startButtonLabel.Length; i++)
        {
            if (startButtonLabel[i] != null)
            {
                startButtonLabel[i].text = Global._instance.GetString("btn_92");
            }
        }
    }
    
    protected void SetCloseButtonLabel()
    {
        if (closeButtonLabel == null || closeButtonLabel.Length <= 0) return;
        
        for (int i = 0; i < closeButtonLabel.Length; i++)
        {
            if (closeButtonLabel[i] != null)
            {
                closeButtonLabel[i].text = Global._instance.GetString("btn_93");
            }
        }
    }
    
    protected void SetSubTitleLabel(string key)
    {
        if (subTitleLabel != null)
        {
            subTitleLabel.text = Global._instance.GetString(key);
        }
    }

    protected void SetDescriptionLabel(string key)
    {
        if (descriptionLabel != null)
        {
            descriptionLabel.text = Global._instance.GetString(key);
        }
    }
    
    protected void SetPrecautionsLabel(string key)
    {
        if (precautionsLabel != null)
        {
            precautionsLabel.text = Global._instance.GetString(key);
        }
    }
    
    protected void SetIdLabel(string id)
    {
        if (idLabel != null)
        {
            idLabel.text = id;
        }
    }
    
    protected void SetPasswordLabel(string id)
    {
        if (passwordLabel != null)
        {
            passwordLabel.text = id;
        }
    }
    
    protected string GetIdLabel()
    {
        return idLabel != null ? idLabel.text : "";
    }
    
    protected string GetPasswordLabel()
    {
        return passwordLabel != null ? passwordLabel.text : "";
    }
    
    protected void SetIdInputField(string id)
    {
        if (idInputField != null)
        {
            idInputField.value = id;
        }
    }
    
    protected void SetPasswordInputField(string password)
    {
        if (passwordInputField != null)
        {
            passwordInputField.value = password;
        }
    }

    /// <summary>
    /// 에러 코드별 스트링 리턴
    /// </summary>
    protected string GetErrorCodeStringKey(string error)
    {
        switch (error)
        {
            // 이미 등록된 ID가 있어 등록할 수 없음
            case "AUTH_400_0008":
                return Global._instance.GetString("er_gusst_data_1");
            // ID 공백 또는 문자 종류가 올바르지 않음
            case "AUTH_400_0009":
                return Global._instance.GetString("er_gusst_data_2");
            // ID와 비밀번호가 잘못되었거나 기한이 만료됨
            case "AUTH_400_0010":
                return Global._instance.GetString("er_gusst_data_3");
            // 마이그레이션 전과 후의 userKey가 동일함
            case "AUTH_400_0011":
                return Global._instance.GetString("er_gusst_data_6");
            // 게스트 계정이 아닌 다른 인증 방식으로 연결하려 함
            case "AUTH_400_0012":
                return Global._instance.GetString("er_gusst_data_4");
            // 비밀번호 입력 횟수 제한이 초과되어 ID와 비밀번호가 삭제됨
            case "AUTH_400_0013":
                return Global._instance.GetString("er_gusst_data_5");
            // 기타 오류가 발생했을 때
            default:
                return Global._instance.GetString("er_gusst_data_7");
        }
    }
}
