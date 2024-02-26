using System;
using Protocol;
using UnityEngine;


public enum PionProfileContentType
{
    WORLD_RANKING,
    TURN_RELAY,
    GROUP_RANKING,
}

/// <summary>
/// 프로필 설정 팝업 (월드랭킹, 금붕어 잡기)
/// </summary>
public class UIPopupPionProfile : UIPopupBase
{
    public static UIPopupPionProfile instance = null;
    
    [SerializeField] private UILabel[]     titleLabel;
    [SerializeField] private UILabel       descriptionLabel;
    [SerializeField] private UILabel       nameBoxLabel;
    [SerializeField] private UIInput       inputKeyBoard;
    [SerializeField] private UIItemProfile profileItem;
    
    [SerializeField] private GameObject _groupRankingUI;

    private bool     _isClickableBtnOk = true;
    private UserBase _userData         = new UserBase();
    private Action   _onSaveNicknameComplete;
    private Action   _onCloseButtonClick;
    private string   _userNameBackup;
    
    private PionProfileContentType _eventType;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        instance = null;
    }

    public void Init(PionProfileContentType type, Action onComplete = null, Action onCloseBtnClick = null, string title = "p_pionp_1", string desc = "p_pionp_4")
    {
        _eventType              = type;
        _onSaveNicknameComplete = onComplete;
        _onCloseButtonClick     = onCloseBtnClick;

        titleLabel.SetText(Global._instance.GetString(title));
        descriptionLabel.text = Global._instance.GetString(desc);

        SetProfile();

        profileItem.SetProfileOnlyAlterPicture(_userData);
        InitUserName(_userData.GameName);

        _groupRankingUI.SetActive(type == PionProfileContentType.GROUP_RANKING);
    }

    private void SetProfile()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        _userData = myProfile;
    }

    private void InitUserName(string gameName)
    {
        if (string.IsNullOrEmpty(gameName) || gameName.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            _userNameBackup    = "";
            nameBoxLabel.text = Global._instance.GetString("p_pionp_3");
        }
        else
        {
            _userNameBackup      = gameName;
            nameBoxLabel.text   = gameName;
            inputKeyBoard.value = gameName;
        }
    }

    private void SetUserName(string gameName)
    {
        if (string.IsNullOrEmpty(gameName))
        {
            inputKeyBoard.value = "";
            nameBoxLabel.text   = Global._instance.GetString("p_pionp_3");
        }
        else
        {
            nameBoxLabel.text   = gameName;
            inputKeyBoard.value = gameName;
        }
    }

    private void OnClickProfileSelectionButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        bCanTouch = false;
        if (ManagerAdventure.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopUPUserProfileSelection>((popup) => popup.InitData(_userData),
                () => { bCanTouch = true; });
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopUPUserProfileSelection>((popup) => popup.InitDataNotOpenAdventure(),
                () => { bCanTouch = true; });
        }
    }

    private void OnClickOKButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        if (_isClickableBtnOk)
        {
            // 스페이스 검사 + 길이 검사.
            inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck(inputKeyBoard.value);
            if (inputKeyBoard.value == "")
            {
                SetUserName(_userNameBackup);
                return;
            }

            // 특수기호 '[' , ']' 검사.
            inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck(inputKeyBoard.value);
            if (inputKeyBoard.value == "")
            {
                SetUserName(_userNameBackup);
                return;
            }
            
            inputKeyBoard.isSelected = false;
            bCanTouch = false;
            
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_S);
            RequestSaveNickname();
        }
        else
        {
            // 만약 안드로이드가 아니라면
            if (Application.platform != RuntimePlatform.Android)
            {
                // 입력 완료 처리
                SubmitInputFieldAndSave(true);
            }
            else
            {
                // 아무처리도 해주지 않음
                return;
            }
        }
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        _onCloseButtonClick?.Invoke();
    }

    #region InputField
    public void OnClickInputField()
    {
        _isClickableBtnOk         = false;
        inputKeyBoard.isSelected = true;
    }

    public void OnSubmitInputField() => SubmitInputFieldAndSave(false);

    private void SubmitInputFieldAndSave(bool save)
    {
        if (save == false)
        {
            _isClickableBtnOk = false;
        }

        // 스페이스 검사 + 길이 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck(inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            SetUserName(_userNameBackup);
            _isClickableBtnOk = true;
            return;
        }

        // 특수기호 '[' , ']' 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck(inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            SetUserName(_userNameBackup);
            _isClickableBtnOk = true;
            return;
        }
        
        RequestNgWordCheck(save);
    }
    #endregion

    private void RequestNgWordCheck(bool save)
    {
        var isResult = true;
        ServerAPI.FilterNGWord(inputKeyBoard.value, OnResponse);

        void OnResponse(NGWordResp ngWordResp)
        {
            if (ngWordResp.changed)
            {
                inputKeyBoard.value = ngWordResp.text.Replace("*", " ");
                isResult            = true;
            }
            else
            {
                isResult = false;
            }

            if (isResult)
            {
                var empty = inputKeyBoard.value.Trim();
                if (string.IsNullOrEmpty(empty))
                {
                    SetUserName(_userNameBackup);
                }

                _isClickableBtnOk         = true;
                inputKeyBoard.isSelected = false;

                var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                popup.FunctionSetting(1, "OnClickInputField", this.gameObject);
            }
            else
            {
                var isClipStrError = false;
                // NG워드 이모지 검사
                var isError = Global.IsFilterClipString(() =>
                {
                    var isClipStr = Global.ClipString(nameBoxLabel.text);
                    if (isClipStr.Equals(nameBoxLabel.text) == false)
                    {
                        nameBoxLabel.text        = isClipStr;
                        inputKeyBoard.value      = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isClipStrError           = true;
                    }
                });

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if (isError || isClipStrError)
                {
                    nameBoxLabel.text        = Global.ClipString(nameBoxLabel.text);
                    inputKeyBoard.value      = nameBoxLabel.text;
                    inputKeyBoard.isSelected = false;

                    var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                    popup.FunctionSetting(1, "OnClickInputField", this.gameObject);
                    _isClickableBtnOk = true;
                }
                else
                {
                    nameBoxLabel.text   = Global.ClipString(nameBoxLabel.text);
                    inputKeyBoard.value = nameBoxLabel.text;

                    if (save)
                    {
                        _isClickableBtnOk = true;
                        OnClickOKButton(); // 입력완료 저장
                    }
                }

                if (save == false)
                {
                    _isClickableBtnOk = true;
                }
            }
        }
    }

    private void RequestSaveNickname()
    {
        ServerAPI.SaveNickname(Global.ClipString(inputKeyBoard.value), OnComplete);
        
        void OnComplete(BaseResp resp)
        {
            if (resp.IsSuccess)
            {
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_E);
                var myProfile = SDKGameProfileManager._instance.GetMyProfile();
                myProfile.SetName(ServerRepos.Nickname);

                UpdateProfilePopupSetting();
            }
            else
            {
                bCanTouch = true;
            }
        }
    }

    private void UpdateProfilePopupSetting()
    {
        ServerAPI.UpdateProfilePopupSetting(GetEventName(), OnComplete);
        
        void OnComplete(BaseResp resp)
        {
            if (resp.IsSuccess)
            {
                _callbackClose = () => { _onSaveNicknameComplete?.Invoke(); };
                ManagerUI._instance.ClosePopUpUI(instance);
            }
            else
            {
                bCanTouch = true;
            }
        }
    }

    private string GetEventName() => _eventType switch
    {
        PionProfileContentType.WORLD_RANKING => "world_rank",
        PionProfileContentType.TURN_RELAY    => "turn_relay",
        PionProfileContentType.GROUP_RANKING => "group_rank",
        _                                    => string.Empty
    };
}