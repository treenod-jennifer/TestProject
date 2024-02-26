using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopUpUserInfo : UIPopupBase
{
    public UILabel[] title;
    public UIInput inputKeyBoard;
    public UILabel nameBox;
    public UILabel jewelCount;
    public UILabel jewelFreeCount;
    public UILabel coinCount;
    public UILabel coinFreeCount;

    public bool isEditComplete = false;
    private bool isClickableBtnOk = false;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        InitPopUp();
    }

    public void InitPopUp()
    {
        //타이틀 세팅.
        string titletext = Global._instance.GetString("p_u_i_1");
        title[0].text = titletext;
        title[1].text = titletext;


        inputKeyBoard.defaultText = ManagerData._instance.userData.name;
        
        // 초기화
        this.isClickableBtnOk = false;
        //this.transform.localPosition += ( Vector3.up * 200 );
    }

    void OnClickTextBox()
    {
        // 텍스트 박스가 열릴시 클릭 불가능
        this.isClickableBtnOk = false;

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

    void OnClickSaveNickName()
    {
        // 만약 버튼을 클릭할 수 있는 상태라면
        if ( this.isClickableBtnOk )
        {
            if ( inputKeyBoard.value.Length > 0 )
            {
                //그로씨 로그 전송
                //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent( ServiceSDK.GROWTHY_INFLOW_VALUE.NICKNAME_S );

                inputKeyBoard.isSelected = false;
                isEditComplete = true;

               // ServerAPI.SaveNickname( Global.ClipString( nameBox.text ), recvLogin );
            }
        }
        else    // 버튼 클릭이 가능한 상태가 아니라면 단말기마다의 차이점으로 인해 우선 클릭을 막음 
        {
            // 만약 안드로이드가 아니라면
            if ( Application.platform != RuntimePlatform.Android )
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
        } );
    }

    public void OnClickSubmitText ()
    {
        this.isClickableBtnOk = false;

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

        //NG워드 검사.
        this.IsAllowTheForbiddenWord( (bool isResult) =>
        {
            if ( isResult )
            {
                inputKeyBoard.isSelected = false;
                isEditComplete = true;
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                popup.FunctionSetting( 1, "OnClickTextBox", this.gameObject );
                this.isClickableBtnOk = true;
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
        } );

    
    }

    /// <summary>
    /// 금지할 언어가 있는지 검사함
    /// </summary>
    /// <returns></returns>
    private void IsAllowTheForbiddenWord (System.Action<bool> callbackHandler)
    {

    }
}
