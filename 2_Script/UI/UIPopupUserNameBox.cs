using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupUserNameBox : UIPopupBase
{
    public UIInput inputKeyBoard;
    public UILabel nameBox;

    public UIPokoButton btnOK;

    public bool isEditComplete = false;
    private bool isClickableBtnOk = false;

    public override void OnClickBtnBack()
    {
        return;
    }


    public void InitPopUp(bool bChat)
    {
        if (bChat == false)
            return;

        //this.transform.localPosition += (Vector3.up * 200);
        if (UIChat._instance != null)
        {
            //유저입력 후, touch 켜주기.
            _callbackEnd += UIChat._instance.TouchOn;
            _callbackEnd += UIChat._instance.CloseBtn;
        }

        this.isClickableBtnOk = false;
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
            
            // 입력이 이루어 졌다면
      /*      if (this.inputKeyBoard.value.Length != 0)
            {
                nameBox.text = this.inputKeyBoard.value;
            }*/
            yield return null;
        }
        yield break;
    }

    void OnClickTextBox()
    {
        this.isClickableBtnOk = false;

        inputKeyBoard.isSelected = true;
        StartCoroutine(StartWriteString());
    }

    void OnClickSaveNickName()
    {
        if( this.isClickableBtnOk )
        {
            if ( inputKeyBoard.value.Length > 0 )
            {
                //그로씨 로그 전송
                //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent( ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_S );

                inputKeyBoard.isSelected = false;
                isEditComplete = true;
             //   ServerAPI.SaveNickname( Global.ClipString( nameBox.text ), recvLogin );
            };
        }
        else    // 버튼 클릭이 가능한 상태가 아니라면 단말기마다의 차이점으로 인해 우선 클릭을 막음 
        {
            // 만약 안드로이드가 아니라면
            if( Application.platform != RuntimePlatform.Android )
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


    public void OnClickSubmitTextAndSaveNickName ()
    {
        //스페이스 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck( this.inputKeyBoard.value );
        if ( inputKeyBoard.value == "" )
        {
            this.isClickableBtnOk = true;
            return;
        }

        //특수기호 '[' , ']' 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck( this.inputKeyBoard.value );
        if ( inputKeyBoard.value == "" )
        {
            this.isClickableBtnOk = true;
            return;
        }
        /*
        //NG워드 검사.
        this.IsAllowTheForbiddenWord( (bool isResult) =>
        {
            if ( isResult )
            {
                this.isClickableBtnOk = true;
                inputKeyBoard.isSelected = false;
                isEditComplete = true;
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                popup.FunctionSetting( 1, "OnClickTextBox", this.gameObject );
            }
            else
            {
                bool isClipStrError = false;
                // NG워드 이모지 검사
                bool isError = Global.IsFilterClipString( () =>
                {
                    string isClipStr = Global.ClipString( nameBox.text );

                    if ( isClipStr.Equals( nameBox.text ) == false )
                    {
                        nameBox.text = isClipStr;
                        this.inputKeyBoard.text = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isEditComplete = true;
                        isClipStrError = true;
                    }

                    string filterStr = JsonFx.Json.JsonWriter.Serialize( isClipStr );
                    filterStr = JsonFx.Json.JsonReader.Deserialize<string>( filterStr );
                } );

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if ( isError || isClipStrError )
                {
                    nameBox.text = Global.ClipString( nameBox.text );
                    this.inputKeyBoard.text = nameBox.text;
                    inputKeyBoard.isSelected = false;
                    isEditComplete = true;
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                    popup.FunctionSetting( 1, "OnClickTextBox", this.gameObject );
                    this.isClickableBtnOk = true;
                }
                else
                {
                    nameBox.text = Global.ClipString( nameBox.text );
                    this.inputKeyBoard.text = nameBox.text;

                    this.isClickableBtnOk = true;
                    // 입력완료 저장
                    this.OnClickSaveNickName();
                }
            }
        } );*/
    }

    public void OnClickSubmitText ()
    { 
        //스페이스 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameLengthCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this.isClickableBtnOk = true;
            return;
        }

        //특수기호 '[' , ']' 검사.
        inputKeyBoard.value = ManagerUI._instance.UserNameCharacterCheck(this.inputKeyBoard.value);
        if (inputKeyBoard.value == "")
        {
            this.isClickableBtnOk = true;
            return;
        }
        /*
        //NG워드 검사.
        this.IsAllowTheForbiddenWord( (bool isResult) =>
        {
            if ( isResult )
            {
                this.isClickableBtnOk = true;
                inputKeyBoard.isSelected = false;
                isEditComplete = true;
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                popup.FunctionSetting( 1, "OnClickTextBox", this.gameObject );
            }
            else
            {
                bool isClipStrError = false;
                // NG워드 이모지 검사
                bool isError = Global.IsFilterClipString( () =>
                {
                    string isClipStr = Global.ClipString( nameBox.text );

                    if ( isClipStr.Equals( nameBox.text ) == false )
                    {
                        nameBox.text = isClipStr;
                        this.inputKeyBoard.text = isClipStr;
                        inputKeyBoard.isSelected = false;
                        isEditComplete = true;
                        isClipStrError = true;
                    }

                    string filterStr = JsonFx.Json.JsonWriter.Serialize( isClipStr );
                    filterStr = JsonFx.Json.JsonReader.Deserialize<string>( filterStr );
                } );

                // isError이거나 isClipStrError일 경우 에러 팝업 표시
                if ( isError || isClipStrError )
                {
                    nameBox.text = Global.ClipString( nameBox.text );
                    this.inputKeyBoard.text = nameBox.text;
                    inputKeyBoard.isSelected = false;
                    isEditComplete = true;
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                    popup.FunctionSetting( 1, "OnClickTextBox", this.gameObject );
                }
                else
                {
                    nameBox.text = Global.ClipString( nameBox.text );
                    this.inputKeyBoard.text = nameBox.text;
                }

                this.isClickableBtnOk = true;
            }
        });*/
    }
    /*
    void recvLogin(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //그로씨 로그 전송
           // ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_E);

            ManagerData._instance.userData.name = ServerRepos.Nickname;
            ManagerUI._instance.ClosePopUpUI();
        }
    }


    /// <summary>
    /// 금지할 언어가 있는지 검사함
    /// </summary>
    /// <returns></returns>
    private void IsAllowTheForbiddenWord ( System.Action<bool> callbackHandler)
    {
        bool isResult = true;
        ServerAPI.FilterNGWord( this.inputKeyBoard.value, (Protocol.NGWordResp ngWordResp)
        =>
        {
            if ( ngWordResp.changed )
            {
                this.inputKeyBoard.value = ngWordResp.text.Replace( "*", " " );
                isResult = true;
            }
            else
            {
                isResult = false;
            }

            callbackHandler( isResult );
        } );
    }*/
}
