using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Protocol;
using System.Globalization;

public class UIStampItemButtonText : UIStampItemButton
{
    public enum EditWorkState
    {
        Complete = 0,
        Empty,
        ProhibtedWord,
    }
    public const int ALLOW_WORD_BYTE = 1000;

    private bool isStartFirstTextEdit = true;

    public UIInput inputKeyBoard;
    public TweenPosition tweenPos;
    public bool isEditStart = false;
    public bool isEditComplete = false;
    public UILabel actionObj;
    public string originValue;
    private bool isEnteredLine = false;
    private int textLimitLength = 20;

    //--------------------------------------------------------------------------- 
    private IEnumerator StartWriteString ()
    {
        yield return new WaitForEndOfFrame();

        int length = this.inputKeyBoard.value.Length;
        this.originValue = this.inputKeyBoard.value;

        if ( length > textLimitLength)
        {
            StringInfo si = new StringInfo(this.inputKeyBoard.value);
            this.inputKeyBoard.value = si.SubstringByTextElements(0, si.LengthInTextElements - 1);
            
            this.originValue = this.inputKeyBoard.value;
        }

        while ( this.isEditComplete == false )
        {
            if ( this.isEditComplete )
            {
                this.isEnteredLine = false;
                break;
            }

            string value = this.inputKeyBoard.value;
            // 입력이 이루어 졌다면
            if ( value.Length != 0 && originValue != value
                && value.Length <= textLimitLength)
            {
                var clippedString = Global.ClipString(value);
                if (clippedString.Equals(value))
                {
                    try
                    {
                        actionObj.text = this.ProcDecisionWriteTextEvent("_");
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.Message);
                        actionObj.text = originValue;
                    }
                }
                else
                {
                    actionObj.text = originValue;
                }
            }
            else if ( value.Length > textLimitLength && originValue != value )
            {
                if ( UIPopupSystem._instance == null )
                {
                    curSelectItemButton?.DoDeselectButtonData();
                    curSelectItemButton = null;

                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                    string text = Global._instance.GetString("n_st_2");
                    text = text.Replace("[n]", textLimitLength.ToString());
                    popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), text, false );
                    popup.FunctionSetting( 1, "StartSubmitEvent", this.gameObject, true );
                }
                value = this.originValue;
            }
            else if ( value.Length == 0 && originValue != value )
            {
                actionObj.text = value;
                this.originValue = value;
            }

            this.inputKeyBoard.value = value;

            yield return null;
        }
       yield break;
    }

     private string ProcDecisionWriteTextEvent ( string delimiters )
    {
        string decisionStr = this.inputKeyBoard.value;
        StringInfo stringInfo = new StringInfo(decisionStr);

        if ( decisionStr.IndexOf (delimiters) != -1 || decisionStr.IndexOf (delimiters) > 10 ) 
        {
			this.originValue = "";
            string[] decisionStrs = decisionStr.Split( delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < decisionStrs.Length; i++ )
            {
                this.isEnteredLine = true;
                this.originValue = ( i != decisionStrs.Length - 1 ) ? originValue + decisionStrs[i] + "\n" :
                                                                      originValue + decisionStrs[i];
            }
            return this.originValue;
        }

        else if (stringInfo.LengthInTextElements > 10 )
        {
			// 만약 원래 데이터가 들어 있다면 
            this.isEnteredLine = true;
            if ( decisionStr.IndexOf( "\n" ) == -1 )
            {
                string subStringHead = stringInfo.SubstringByTextElements(0, 10);
                string subStringTail = stringInfo.SubstringByTextElements(10, stringInfo.LengthInTextElements - 10);
                this.originValue = $"{subStringHead}\n{subStringTail}";
            }
            else
            {
                this.originValue = decisionStr;
            }
			//Debug.Log (string.Format ("originValue : {0}, keyBoardValue : {1}", this.originValue, inputKeyBoard.value));
            return this.originValue;
        }

        if ( decisionStr.IndexOf (delimiters) == -1 ) 
        {
            if (isEnteredLine == true && decisionStr.Length < 10 ) 
            {
                isEnteredLine = false;
            }
        } 
        else if ( decisionStr.Length < 10 )
        {
            if (isEnteredLine == true && decisionStr.IndexOf (delimiters) == -1 ) 
            {
                isEnteredLine = false;
            }
        }

        originValue = decisionStr;
        return originValue;
    }



    public void StartSubmitEvent ()
    {
        this.IsAllowTheForbiddenWord( ( bool isResult ) =>
        {
            if( isResult )
            {
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                if(curSelectItemButton != null)
                { 
                    curSelectItemButton.DoDeselectButtonData();
                    curSelectItemButton = null;
                }

                popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false );
                popup.FunctionSetting(1, "StartButtonActionEvent", this.gameObject );
            }
        });

    }

    /// <summary>
    /// 글자 단어수가 한계를 넘지 않는지 검사함
    /// </summary>
    /// <returns></returns>
    private bool IsAllowWordByte ()
    {
        byte[] byteArr = System.Text.UTF8Encoding.Default.GetBytes( this.actionObj.text );

        int length = byteArr.Length;
        if ( length <= ALLOW_WORD_BYTE )
        {
            return true;
        }
        else
            return false;
    }

    private void StartTweenEvent ()
    {
        this.tweenPos.enabled = true;

        if ( this.isStartFirstTextEdit )
        {
            this.tweenPos.ResetToBeginning();
            this.isStartFirstTextEdit = false;
        }
        else
        {
            this.tweenPos.enabled = true;
            Vector3 tempPos = this.tweenPos.from;
            this.tweenPos.from = this.tweenPos.to;
            this.tweenPos.to = tempPos;
            this.tweenPos.ResetToBeginning();
        }
    }

    /// <summary>
    /// 금지할 언어가 있는지 검사함
    /// </summary>
    /// <returns></returns>
    private void IsAllowTheForbiddenWord ( Action<bool> callbackHandler )
    {
        bool isResult = true;
        int stampIndex = UIPopupSendItemToSocial._instance.StampIndex;

        ServerAPI.FilterStampNGWord(stampIndex, this.originValue, (Protocol.NGWordResp ngWordResp)
        =>
        {
            if ( ngWordResp.changed )
            {
                this.originValue = ngWordResp.text;
                this.originValue = this.originValue.Replace( "*", " " );
                this.inputKeyBoard.value = this.originValue;
                this.actionObj.text = this.originValue;
                isResult = true;
            }
            else
            {
                isResult = false;
            }

            string clippedString = Global.ClipString(this.originValue);
            if (clippedString != this.originValue)
            {
                isResult = true;

                this.originValue = clippedString;
                this.inputKeyBoard.value = this.originValue;
                this.actionObj.text = this.originValue;
            }


            callbackHandler( isResult );
        } );
    }
    //---------------------------------------------------------------------------
    protected override void InitEventData ()
    {
        this.selectTexture.gameObject.SetActive( false );
    }

    public override void StartButtonActionEvent ()
    {
        UIStampItemButtonContainer.instance.SetDefaultBtnData();

        this.inputKeyBoard.isSelected = true;
        this.isEditComplete = false;
        this.isEditStart = true;
        this.isEnteredLine = false;     
        this.StartCoroutine( this.StartWriteString() );

#if UNITY_EDITOR
        this.StartTweenEvent();
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR     
        this.StartTweenEvent();
#endif
    }

    protected override void DestroyButtonActionEvent ()
    {
        // 추후 작성
        this.inputKeyBoard.isSelected = false;
        this.isEditComplete = true;
        this.isEditStart = false;
#if UNITY_EDITOR
        this.StartTweenEvent();
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR     
        this.StartTweenEvent();
#endif
    }

    //---------------------------------------------------------------------------
    public string GetItemInputText ()
    {
        if( this.originValue == "" )
        {
            if(this.actionObj.text == "")
            {
                return this.originValue;
            }
            else
            {
                return this.actionObj.text;
            }
        }
        else
        {
            return this.originValue;
        }
    }

    public void GetStringEditState ( Action<EditWorkState, string> callbackHandler )
    {
        EditWorkState state = EditWorkState.Complete;

        state = EditWorkState.Complete;
        DelegateHelper.SafeCall( callbackHandler, state, this.actionObj.text );
    }

    public void InitializeData (GameObject actionObj, Stamp data)
    {
        this.actionObj = actionObj.GetComponent<UILabel>();
        this.actionObj.text = data.Text;
        this.inputKeyBoard.value = data.Text;
        this.actionObj.transform.localEulerAngles = data.textLocalRotation;
        this.actionObj.GetComponent<UIWidget>().width = data.textWidgetWidth;
        this.actionObj.GetComponent<UIWidget>().height = data.textWidgetHeight;
        this.actionObj.pivot = data.textPivot;
        this.actionObj.transform.localPosition = data.textLocalPosition;
        this.actionObj.aspectRatio = data.textAspectRatio;
        this.actionObj.alignment = data.textAlignment;
        this.actionObj.overflowMethod = data.textOverflow;
        this.actionObj.effectStyle = UILabel.Effect.Shadow;
        this.actionObj.effectColor = new Color( this.actionObj.color.r - ( 150 / 255f ), this.actionObj.color.g - ( 150 / 255f ), this.actionObj.color.b - ( 150 / 255f ), 57f / 255f );
        this.actionObj.effectDistance = new Vector2( 1.34f, 1.22f );
        this.actionObj.fontSize = data.textSize;

        //글자 수 제한값 받아오기.
        this.textLimitLength = data.textLimit;
    }
}
