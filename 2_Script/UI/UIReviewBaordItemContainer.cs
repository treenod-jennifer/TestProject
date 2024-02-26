using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIReviewBaordItemContainer : MonoBehaviour 
{
    //---------------------------------------------------------------------------
    public List<UIItemStageReview> containerItemList = new List<UIItemStageReview>();

    //---------------------------------------------------------------------------
    public GameObject objItem;

    //---------------------------------------------------------------------------
    public void InitData ( SDKBoardArticleCommentInfo[] commentDataList, string idBoard, string idArticle, Action callbackHandler )
    {
        //리뷰 삭제 후 생성.
        UIPopupStageReview._instance.DestroyOrMakeReviewScroll(true);
        UIPopupStageReview._instance.DestroyOrMakeReviewScroll(false);
        UIPopupStageReview._instance.SetStageReviewData( commentDataList );
        ManagerReviewBoardProcess.instance.SetReviewBoardCommetData( commentDataList );

        //데이터 갱신 후 리뷰 미리보기 데이터 갱신.
        UIPopupStageReview._instance.SettingPreviewChatData();
        UIPopupStageReview._instance.bCanTouch = true;
    }

    public void InitData ( SDKBoardArticleCommentInfo commentData, int index )
    {
        UIPopupStageReview._instance.SetStageReviewData( commentData, index );

        UIPopupStageReview._instance.bCanTouch = true;
    }

    public void InitAddData ( SDKBoardArticleCommentInfo[] commentDataList, string idBoard, string idArticle, Action callbackHandler )
    {
        UIPopupStageReview._instance.SetAddStageReviewData( commentDataList );
        UIPopupStageReview._instance.bCanTouch = true;
    }

    public List<UIItemStageReview> GetItemList ()
    {
        return this.containerItemList;
    }

    public int GetItemCount ()
    {
        return this.containerItemList.Count;
    }
}
