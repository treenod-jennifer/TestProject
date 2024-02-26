using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 리뷰보드 Comment 아이템
/// </summary>
public class UIItemContainerComment : MonoBehaviour
{
    //---------------------------------------------------------------------------
    public UILabel lblNickName;
    public UILabel lblFlowerCount;
    public UILabel lblComment;
    public UILabel lblGradeCount;

    private string boardId;
    private string articleId;
    private string id;
    private string body;
    private bool isLiked;
    private int likes;
    private string nickName;
    private string flowerCount;

    private Dictionary<string, string> profile = new Dictionary<string, string>();
    private Action<bool> callbackHandler;

    private int isClickComplete = 0;

    //---------------------------------------------------------------------------
    /// <summary>
    /// 아이템 데이터 초기화
    /// </summary>
    public void InitData ( int gradeCount, string boardId, string articleId, SDKBoardArticleCommentInfo data, Action<bool> callbackHandler )
    {
        this.id          = data.id;
        this.boardId     = boardId;
        this.articleId   = articleId;
        this.body        = data.body;
        this.isLiked     = data.isLiked;
        this.likes       = System.Convert.ToInt32( data.likeCount );
        this.nickName    = data.profile.name;//(data.profile.name("displayName")) ? data.profile["displayName"] : "GuestID";
        this.flowerCount = data.profile.flower.ToString();//(data.profile.ContainsKey("Count")) ? data.profile["Count"] : "0";

        this.lblNickName.text = this.nickName;
        this.lblFlowerCount.text = string.Format("count: {0}", this.flowerCount);
        this.lblComment.text = this.body;
        this.lblGradeCount.text = gradeCount.ToString();
        this.callbackHandler = callbackHandler;
    }

    /// <summary>
    /// 해당 코맨트에 추천 버튼 클릭가능
    /// </summary>
    public void OnClickBtnGradeUp ()
    {
        if( this.isClickComplete == 0 )
        {
            this.isClickComplete = 1;
            SDKReviewBoardManager._instance.UpdateLikeComment( this.boardId, this.articleId, this.id, ( bool isError ) => 
                                                                                                       { 
                                                                                                           if( isError )
                                                                                                           {
                                                                                                               this.isClickComplete = 0;
                                                                                                           }
                                                                                                           else
                                                                                                           {
                                                                                                               this.isClickComplete = 1;
                                                                                                               DelegateHelper.SafeCall( this.callbackHandler, isError ); 
                                                                                                           }
                                                                                                       });
        }
        else
        {
            if( UIPopupSystem._instance == null )
            { 
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_rv_3" ), false, null );
                popupWarning.SortOrderSetting();
            }
        }
    }

    /// <summary>
    /// 코맨트 삭제
    /// </summary>
    public void OnClickBtnDelete ()
    {
        SDKReviewBoardManager._instance.DeleteMyComment( this.boardId, this.articleId, this.id,
                                                     ( bool isError ) =>
                                                     {
                                                         Extension.PokoLog.Log(" ============ DeleteComplete!!! ============ ");
                                                         DelegateHelper.SafeCall( this.callbackHandler, isError );
                                                     } );
    }
}
