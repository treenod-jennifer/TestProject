using System.Collections;
using UnityEngine;
using Protocol;

public class UIPopUpUserInfo : UIPopupBase
{
    public UILabel[] title;
    public UIInput   inputKeyBoard;
    public UILabel   nameBox;
    public UILabel   jewelCount;
    public UILabel   jewelFreeCount;
    public UILabel   coinCount;
    public UILabel   coinFreeCount;
    public UILabel   serviceCode;

    public bool isEditComplete = false;
    
    private bool _isClickableBtnOk = false;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        InitPopUp();
    }

    public void InitPopUp()
    {
        //타이틀 세팅.
        var titleText = Global._instance.GetString("p_u_i_1");
        title[0].text = titleText;
        title[1].text = titleText;

        jewelCount.text     = ServerRepos.User.jewel.ToString();
        coinCount.text      = ServerRepos.User.coin.ToString();
        jewelFreeCount.text = ServerRepos.User.fjewel.ToString();
        coinFreeCount.text  = ServerRepos.User.fcoin.ToString();

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        inputKeyBoard.defaultText = myProfile.GameName;

        InitServiceCode();

        // 초기화
        this._isClickableBtnOk = false;
    }

    private void InitServiceCode()
    {
        var uidStr = string.Format("{0:X8}", ServerRepos.User.uid);
        var xorStr = "pokotown";
        var midStr = new char[8];
        for (var i = 0; i < uidStr.Length; ++i)
        {
            midStr[i] = System.Convert.ToChar(uidStr[i] ^ xorStr[i]);
        }

        var grayStr = new char[8];
        grayStr[0] = midStr[0];
        for (var i = 1; i < midStr.Length; ++i)
        {
            grayStr[i] = System.Convert.ToChar(midStr[i - 1] ^ midStr[i]);
        }


        var hexEncoded = "";
        for (var i = 0; i < grayStr.Length / 2; ++i)
        {
            hexEncoded += string.Format("{0:X2}{1:X2}", System.Convert.ToInt16(grayStr[i]), System.Convert.ToInt16(grayStr[i + (grayStr.Length / 2)]));
        }

        this.serviceCode.text = string.Format($"Service Code: {hexEncoded}.{ServiceCode.BuildServiceCode()} ");
    }


#if UNITY_EDITOR
    private string Decode(string s)
    {
        string src;
        string a = "";
        string b = "";
        for (int i = 0; i < s.Length; i = i + 4)
        {
            string x = "", y = "";
            x += s[i];
            x += s[i + 1];

            y += s[i + 2];
            y += s[i + 3];

            a += System.Convert.ToChar(System.Convert.ToUInt32(x, 16));
            b += System.Convert.ToChar(System.Convert.ToUInt32(y, 16));
        }

        src = a + b;

        char[] grayStr = new char[8];
        grayStr[0] = src[0];
        for (int i = 1; i < src.Length; ++i)
        {
            grayStr[i] = System.Convert.ToChar(grayStr[i - 1] ^ src[i]);
        }

        string xorStr    = "pokotown";
        char[] decodeStr = new char[8];
        for (int i = 0; i < grayStr.Length; ++i)
        {
            decodeStr[i] = System.Convert.ToChar(grayStr[i] ^ xorStr[i]);
        }

        string decodedString = new string(decodeStr);
        int    uid           = System.Convert.ToInt32(decodedString, 16);

        return new string(decodeStr);
    }
#endif

    private void OnClickServiceCode()
    {
        UniClipboard.SetText(this.serviceCode.text);

        var popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_37"), false, null);
    }

    private void OnClickTextBox()
    {
        // 텍스트 박스가 열릴시 클릭 불가능
        this._isClickableBtnOk = false;

        inputKeyBoard.isSelected = true;
        StartCoroutine(StartWriteString());
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

    private void OnClickSaveNickName()
    {
        // 만약 버튼을 클릭할 수 있는 상태라면
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
                        this.inputKeyBoard.value = isClipStr;
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
                    this._isClickableBtnOk = true;
                }
                else
                {
                    nameBox.text        = Global.ClipString(nameBox.text);
                    inputKeyBoard.value = nameBox.text;

                    this._isClickableBtnOk = true;
                    // 입력완료 저장
                    this.OnClickSaveNickName();
                }
            }
        });
    }

    public void OnClickSubmitText()
    {
        this._isClickableBtnOk = false;

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
                
                inputKeyBoard.isSelected = false;
                isEditComplete           = true;

                var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
                this._isClickableBtnOk = true;
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
                        this.inputKeyBoard.value = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isEditComplete           = true;
                        isClipStrError           = true;
                    }
                });

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if (isError || isClipStrError)
                {
                    nameBox.text             = Global.ClipString(nameBox.text);
                    this.inputKeyBoard.value = nameBox.text;
                    inputKeyBoard.isSelected = false;
                    isEditComplete           = true;

                    var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                    popup.FunctionSetting(1, "OnClickTextBox", this.gameObject);
                }
                else
                {
                    nameBox.text            = Global.ClipString(nameBox.text);
                    this.inputKeyBoard.value = nameBox.text;
                }

                this._isClickableBtnOk = true;
            }
        });
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