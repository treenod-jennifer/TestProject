using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// 리뷰보드 구조체 데이터
/// </summary>
public struct SDKArticleReviewInfo
{
    public int viewCount;
    public int commentCount;
    public long updated;
    public long created;
    public List<string> images;
    public bool isHidden;

    public SDKArticleReviewInfo ( int viewCount, int commentCount, long updated, long created, List<string> images, bool isHidden )
    {
        this.viewCount = viewCount;
        this.commentCount = commentCount;
        this.updated = updated;
        this.created = created;
        this.images = images;
        this.isHidden = isHidden;
    }
}

/// <summary>
/// 리뷰보드 이벤트 테스트 Sample 클래스
/// </summary>
public class SDKProcReviewBoardEvent : MonoBehaviour
{
    //---------------------------------------------------------------------------
    [HideInInspector]
    public SDKReviewBoard board;

    //---------------------------------------------------------------------------
    /// <summary>
    /// 테스트 SDK 초기화
    /// </summary>
    public void InitData ()
    {
        if( Global.SkipStageComment() == false )
        {
            Extension.PokoLog.Log("============= SDkReviewBoardSample 호출 =============");
            GameObject            objReviewBoard = new GameObject();
            objReviewBoard.AddComponent<SDKReviewBoardManager>();
            
            // ariticlaData 받아옴
            this.board = new SDKReviewBoard();
        }
    }

    //---------------------------------------------------------------------------
    /// <summary>
    /// 내 코맨트를 삭제한다.
    /// </summary>
    public void DeleteComment ( string idArticle, string idComment, Action<bool> callbackHandler)
    {
        SDKReviewBoardManager._instance.DeleteMyComment ( this.board.GetBoardID(), idArticle, idComment, callbackHandler ); 
    }

    public bool isDataSettingComplete ()
    {
        return this.board.isDataSetting;
    }

    public int GetCurrentArticleCommentData ()
    {
        return this.board.GetArticleData ( ManagerReviewBoardProcess.instance.idCurPlayStageArticle ).infoReview.commentCount;
    }
}
