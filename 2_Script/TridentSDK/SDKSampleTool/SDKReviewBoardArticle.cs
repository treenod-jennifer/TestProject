using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SDK 리뷰보드 아이템 데이터
/// </summary>
public class SDKReviewBoardArticle
{
    //---------------------------------------------------------------------------
    public string idArticle;
    public string idBoard;
    public string title;
    public string userKey;
    public EmotionCountData emotionCounts = new EmotionCountData();
    public Profile_PIONCustom profile = new Profile_PIONCustom();

    public SDKArticleReviewInfo infoReview;

    //---------------------------------------------------------------------------
    public SDKReviewBoardArticle ()
    {
    }

    //---------------------------------------------------------------------------
    public SDKReviewBoardArticle ( SDKReviewBoardArticle data )
    {
        this.idArticle = data.idArticle;
        this.idBoard = data.idBoard;
        this.title = data.title;
        this.userKey = data.userKey;
        this.profile = data.profile;

        SDKArticleReviewInfo dataInfoReview = data.infoReview;
        this.infoReview = new SDKArticleReviewInfo( dataInfoReview.viewCount, dataInfoReview.commentCount,
            dataInfoReview.updated, dataInfoReview.created,
            dataInfoReview.images, dataInfoReview.isHidden );
    }
}