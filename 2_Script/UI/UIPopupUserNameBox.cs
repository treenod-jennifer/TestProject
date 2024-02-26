using System.Collections;
using UnityEngine;
using Protocol;

public class UIPopupUserNameBox : UIPopupBase
{
    public UIInput      inputKeyBoard;
    public UILabel      nameBox;
    public UIPokoButton btnOK;

    public bool isEditComplete = false;

    private bool _isClickableBtnOk = false;

    public override void OnClickBtnBack()
    {
        return;
    }
    
    public void InitPopUp(bool bChat)
    {
        if (bChat == false)
        {
            return;
        }

        if (UIChat._instance != null)
        {
            //유저입력 후, touch 켜주기.
            _callbackEnd += UIChat._instance.TouchOn;
            _callbackEnd += UIChat._instance.CloseBtn;
        }

        this._isClickableBtnOk = false;
    }

    private IEnumerator StartWriteString()
    {
        yield return new WaitForEndOfFrame();

        while (this.isEditComplete == false)
        {
            if (this.isEditComplete)
            {
                break;
            }

            yield return null;
        }

        yield break;
    }

    private void OnClickTextBox()
    {
        this._isClickableBtnOk = false;

        inputKeyBoard.isSelected = true;
        StartCoroutine(StartWriteString());
    }

    private void OnClickSaveNickName()
    {
        if (this._isClickableBtnOk)
        {
            if (inputKeyBoard.value.Length > 0)
            {
                //그로씨 로그 전송
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_S);

                inputKeyBoard.isSelected = false;
                isEditComplete           = true;
                ServerAPI.SaveNickname(Global.ClipString(nameBox.text), ResponseSaveNickname);
            }

            ;
        }
        else // 버튼 클릭이 가능한 상태가 아니라면 단말기마다의 차이점으로 인해 우선 클릭을 막음 
        {
            // 만약 안드로이드가 아니라면
            if (Application.platform != RuntimePlatform.Android)
            {
                // 입력 완료 처리
                this.OnClickSubmitTextAndSaveNickName();
            }
            else
            {
                // 아무처리도 해주지 않음
                return;
            }
        }
    }


    public void OnClickSubmitTextAndSaveNickName()
    {
        //스페이스 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this._isClickableBtnOk = true;
            return;
        }

        //특수기호 '[' , ']' 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this._isClickableBtnOk = true;
            return;
        }

        //NG워드 검사.
        IsAllowTheForbiddenWord((bool isResult) =>
        {
            if (isResult)
            {
                var empty = inputKeyBoard.value.Trim();
                if (string.IsNullOrEmpty(empty))
                {
                    inputKeyBoard.value = "";
                }

                _isClickableBtnOk         = true;
                inputKeyBoard.isSelected = false;
                isEditComplete           = true;

                var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
            }
            else
            {
                var isClipStrError = false;
                // NG워드 이모지 검사
                var isError = Global.IsFilterClipString(() =>
                {
                    var isClipStr = Global.ClipString(nameBox.text);
                    if (isClipStr.Equals(nameBox.text) == false)
                    {
                        nameBox.text             = isClipStr;
                        inputKeyBoard.value      = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isEditComplete           = true;
                        isClipStrError           = true;
                    }
                });

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if (isError || isClipStrError)
                {
                    nameBox.text             = Global.ClipString(nameBox.text);
                    inputKeyBoard.value      = nameBox.text;
                    inputKeyBoard.isSelected = false;
                    isEditComplete           = true;

                    var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                    popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
                    _isClickableBtnOk = true;
                }
                else
                {
                    nameBox.text        = Global.ClipString(nameBox.text);
                    inputKeyBoard.value = nameBox.text;

                    _isClickableBtnOk = true;
                    // 입력완료 저장
                    OnClickSaveNickName();
                }
            }
        });
    }

    public void OnClickSubmitText()
    {
        //스페이스 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this._isClickableBtnOk = true;
            return;
        }

        //특수기호 '[' , ']' 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this._isClickableBtnOk = true;
            return;
        }

        //NG워드 검사.
        IsAllowTheForbiddenWord((bool isResult) =>
        {
            if (isResult)
            {
                var empty = inputKeyBoard.value.Trim();
                if (string.IsNullOrEmpty(empty))
                {
                    inputKeyBoard.value = "";
                }

                _isClickableBtnOk         = true;
                inputKeyBoard.isSelected = false;
                isEditComplete           = true;

                var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
            }
            else
            {
                var isClipStrError = false;
                // NG워드 이모지 검사
                var isError = Global.IsFilterClipString(() =>
                {
                    var isClipStr = Global.ClipString(nameBox.text);
                    if (isClipStr.Equals(nameBox.text) == false)
                    {
                        nameBox.text             = isClipStr;
                        inputKeyBoard.value      = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isEditComplete           = true;
                        isClipStrError           = true;
                    }
                });

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if (isError || isClipStrError)
                {
                    nameBox.text             = Global.ClipString(nameBox.text);
                    inputKeyBoard.value      = nameBox.text;
                    inputKeyBoard.isSelected = false;
                    isEditComplete           = true;

                    var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                    popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
                }
                else
                {
                    nameBox.text        = Global.ClipString(nameBox.text);
                    inputKeyBoard.value = nameBox.text;
                }

                this._isClickableBtnOk = true;
            }
        });
    }

    private void ResponseSaveNickname(BaseResp code)
    {
        if (code.IsSuccess)
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            //그로씨 로그 전송
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_E);

            myProfile.SetName(ServerRepos.Nickname);
            ManagerUI._instance.ClosePopUpUI();
        }
    }


    /// <summary>
    /// 금지할 언어가 있는지 검사함
    /// </summary>
    private void IsAllowTheForbiddenWord(System.Action<bool> callbackHandler)
    {
        var isResult = true;
        ServerAPI.FilterNGWord(this.inputKeyBoard.value, (Protocol.NGWordResp ngWordResp) =>
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

            callbackHandler(isResult);
        });
    }
}