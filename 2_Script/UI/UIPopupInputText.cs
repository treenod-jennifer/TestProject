using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIPopupInputText : UIPopupBase 
{
    public static UIPopupInputText _instance = null;

    public static string[] wordForbidden = new string[2] { "Fuck", "바보" };
    public const int ALLOW_WORD_BYTE = 25; 

    public Action<string> _callbackBtnHandler; 
    public UIInput inputTextObject;
    public UILabel labelText; 

    public bool isEditComplete = false;

    // Use this for initialization
	void Awake ()
    {
        _instance = this;
    }

    IEnumerator StartEditString ()
    {
        yield return new WaitForEndOfFrame();

        while ( true )
        {
            if( this.isEditComplete )
            {
                break;   
            }

            this.labelText.text = this.inputTextObject.label.text;
            this._callbackBtnHandler( this.labelText.text );

            yield return null;
        }

        yield break;
    }

    public void InitPopUp ( Action<string> _callbackBtnHandler )
    {
        this.isEditComplete = false;
        
        this.labelText.text = "Please enter here";
        this._callbackBtnHandler = _callbackBtnHandler;

        // 실시간 스트링 수정되도록 수정작업
        this.StartCoroutine( this.StartEditString() );
    }

    /// <summary>
    /// 버튼 클릭시 핸들러
    /// </summary>
    public void OnClickBtnOk ()
    {
        if ( this.IsAllowTheForbiddenWord() == false )        // 금지된 단어가 있다면
        {
            ManagerUI._instance.OpenPopupConfirm( "There is a prohibted word.", EventCallbackHandler );
            return;
        }
        else if ( this.IsAllowWordByte() == false )          // 글자 허용범위를 초과한다면
        {
            ManagerUI._instance.OpenPopupConfirm( "Enter within 25 characters.", EventCallbackHandler );
            return;
        }

        this._callbackBtnHandler( this.labelText.text );
        this.isEditComplete = true;
        ManagerUI._instance.ClosePopUpUI();
    }

    /// <summary>
    /// 글자 단어수가 한계를 넘지 않는지 검사함
    /// </summary>
    /// <returns></returns>
    private bool IsAllowWordByte ()
    { 
        byte[] byteArr = System.Text.UTF8Encoding.Default.GetBytes( this.labelText.text );

        int length = byteArr.Length;
        if ( length <= ALLOW_WORD_BYTE )
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 금지할 언어가 있는지 검사함
    /// </summary>
    /// <returns></returns>
    private bool IsAllowTheForbiddenWord ()
    {
        string[] forbiddenLine = this.labelText.text.Split( wordForbidden, StringSplitOptions.RemoveEmptyEntries );
        string compareLine = "";
        for (int i = 0; i < forbiddenLine.Length; i++)
        {
            compareLine += forbiddenLine[i];
        }

        compareLine = compareLine.TrimStart(); compareLine = compareLine.TrimEnd();

        if(this.labelText.text.Length.Equals(compareLine.Length) == false )
        {
            return false;
        }

        return true;
    }

    // 확인 팝업 부를시 이벤트 처리할 것이 있다면 처리해줌
    private void EventCallbackHandler ()
    {

    }
}
