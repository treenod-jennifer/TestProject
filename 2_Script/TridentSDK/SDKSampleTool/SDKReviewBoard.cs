using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// SDK 리뷰보드 데이터 클래스
/// </summary>
public class SDKReviewBoard
{    
    //---------------------------------------------------------------------------
    private const int FIRST_SETTING_INDEX = 5;
    private string idBoard = "Stage";

    private Dictionary<string, SDKReviewBoardArticle> articleItems = new Dictionary<string, SDKReviewBoardArticle>();
    private SDKReviewBoardArticle articleItem;

    private bool _isDataSetting = false;
    public bool isDataSetting
    {
        get { return this._isDataSetting; }
        set {  this._isDataSetting = value; }
    }
    //---------------------------------------------------------------------------
    public SDKReviewBoard ()
    {
        if (Global.SkipStageComment() == false)
        {
            Extension.PokoLog.Log("InitData 들어옴");
            this._isDataSetting = false;

            if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
            {
                string idArticle = ReviewBoard.GetArticleID(GameManager.instance.CurrentStage - 1);
                SDKReviewBoardManager._instance.GetAllArticles(this.idBoard, idArticle, FIRST_SETTING_INDEX, GetBoardArticleDataHandler);
            }
            else
            {
                this.GetBoardDummyArticleData();
            }
        }
    }

    public void GetBoardDummyArticleData ()
    {
        this.articleItems.Clear();

        for(int i = 0; i < FIRST_SETTING_INDEX; i++)
        {
            string articleID = ReviewBoard.GetArticleID(GameManager.instance.CurrentStage - 1 + i);
            SDKReviewBoardArticle articleData = new SDKReviewBoardArticle();
            SDKBoardArticleInfo dataInfo = new SDKBoardArticleInfo(articleID, "Stage");
            this.idBoard = dataInfo.boardId;
            articleData.idBoard = dataInfo.boardId;
            articleData.idArticle = dataInfo.id;
            articleData.title = dataInfo.title;
            articleData.userKey = dataInfo.userKey;
            articleData.profile = dataInfo.profile;

            this.articleItems.Add( articleData.idArticle, articleData );
        }
        this._isDataSetting = true;
    }

    //---------------------------------------------------------------------------
    private void GetBoardArticleDataHandler (SDKBoardArticleInfo[] data)
    {
        this.idBoard = data[0].boardId;

        int length = data.Length;
        for ( int i = 0; i < length; i++ )
        {
            SDKReviewBoardArticle articleData = new SDKReviewBoardArticle();
            SDKBoardArticleInfo dataInfo = data[i];
            articleData.idBoard = dataInfo.boardId;
            articleData.idArticle = dataInfo.id;
            articleData.title = dataInfo.title;
            articleData.userKey = dataInfo.userKey;
            articleData.profile = dataInfo.profile;

            articleData.infoReview = new SDKArticleReviewInfo( dataInfo.viewCount, dataInfo.commentCount,
                                                               dataInfo.updatedAt, dataInfo.createdAt,
                                                               dataInfo.images, dataInfo.isHidden );

            if( this.articleItems.ContainsKey(articleData.idArticle) == false )
            { 
                this.articleItems.Add( articleData.idArticle, articleData );
            }
            else
            {
                this.articleItems[articleData.idArticle] = articleData;
            }
        }

        this._isDataSetting = true;
    }

    //---------------------------------------------------------------------------
    public SDKReviewBoardArticle GetArticleData ( string articleID )
    {
        return this.articleItems[articleID];
    }

    public string GetBoardID ()
    {
        return this.idBoard;
    }
}