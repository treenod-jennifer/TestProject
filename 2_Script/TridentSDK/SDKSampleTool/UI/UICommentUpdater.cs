using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 리뷰보드 AddComment 이벤트 클래스
/// </summary>
public class UICommentUpdater : MonoBehaviour 
{
    //---------------------------------------------------------------------------
    private Action<string, System.Action> initCallbackHandler;

    //---------------------------------------------------------------------------
    public UIPokoInput lblComment;

    public string backupString = "";
    //---------------------------------------------------------------------------
    /// <summary>
    /// 데이터 초기화
    /// </summary>
    /// <param name="initCallbackHandler"></param>
    public void InitData ( Action<string, System.Action> initCallbackHandler )
    {
        Extension.PokoLog.Log("=============  InitaCallback 호출 =============");
        this.initCallbackHandler = initCallbackHandler;
        lblComment.onCanceled.Add(new EventDelegate(OnCancelBtnHandler));
    }

    public void OnSubmitBtnHandler ()
    {
        bool isClipStrError = false;
        // NG워드 이모지 검사
        bool isError = Global.IsFilterClipString( () =>
        {
            string isClipStr = Global.ClipString( lblComment.value );

            if ( isClipStr.Equals( lblComment.value) == false )
            {
                lblComment.value = isClipStr;
                lblComment.isSelected = false;
                isClipStrError = true;
            }

            string filterStr = JsonFx.Json.JsonWriter.Serialize( isClipStr );
            filterStr = JsonFx.Json.JsonReader.Deserialize<string>( filterStr );
        } );

        // isError이거나 isClipStrError일 경우 에러 팝업 표시
        if ( isError || isClipStrError )
        {
            lblComment.value = Global.ClipString( lblComment.value);
            lblComment.isSelected = false;
            this.UpdateCommentNGWordReviewHandler();
        }
        else
        {
            lblComment.value = Global.ClipString( lblComment.value);
        }
    }

    public void OnCancelBtnHandler ()
    {
        lblComment.isSelected = false;
        lblComment.value = string.Empty;
    }

    /// <summary>
    ///  버튼 클릭시 핸들러
    /// </summary>
    public void OnClickBtnOK ()
    {
        if ( UIPopupSystem._instance != null || UIPopupStageReview._instance.bCanTouch == false )
        {
            return;
        }
        else
        {
            UIPopupStageReview._instance.bCanTouch = false;

            int length = this.lblComment.label.text.Length;

            if ( length > 50 )
            {
                if ( UIPopupSystem._instance == null )
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_rv_2" ), false );
                    popup.SortOrderSetting();
                    Extension.PokoLog.Log("before : " + this.lblComment.label.text.Length);

                    var si = new System.Globalization.StringInfo(this.lblComment.label.text);                    
                    this.lblComment.label.text = si.SubstringByTextElements(0, si.LengthInTextElements - 1);
                    Extension.PokoLog.Log("current : " + this.lblComment.label.text.Length);
                }
            }
            else
            {
                DelegateHelper.SafeCall( this.initCallbackHandler, Global.ClipString( this.lblComment.label.text ), this.UpdateCommentNGWordReviewHandler );
                this.backupString = this.lblComment.label.text;
                this.lblComment.label.text = "";
            }

            UIPopupStageReview._instance.bCanTouch = true;
        }   
    }

    public void UpdateCurrentComment ()
    {
        this.lblComment.label.text = this.backupString;
        this.OnClickBtnOK();
    }

    private void UpdateCommentNGWordReviewHandler ()
    {
        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
        popupWarning.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_7" ), false, null );
        popupWarning.SetButtonText( 1, Global._instance.GetString( "btn_1" ) );
        popupWarning.FunctionSetting( 1, "EnterRetryInputText", this.gameObject, true );
        popupWarning.HideCloseButton();
    }

    private void EnterRetryInputText ()
    {
        this.lblComment.isSelected = true;
    }
}
