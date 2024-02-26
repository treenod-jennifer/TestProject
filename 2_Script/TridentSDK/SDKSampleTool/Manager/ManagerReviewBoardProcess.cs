using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ReviewBoard
{
    public static string GetArticleID(int stageIdx)
    {
        string articleSuffix = LanguageUtility.SystemLanguage == SystemLanguage.ChineseTraditional ? "tw_" : "";

        if (stageIdx < 1000)
        {
            return string.Format("Stage_{0}{1:D3}", articleSuffix, stageIdx);
        }
        else
        {
            int offset = (int)(stageIdx / 1000);
            char head = (char)('A' + offset - 1);
            int left = stageIdx % 1000;
            return "Stage_" + articleSuffix + head + string.Format("{0:D3}", left);
        }
    }
}

public class ManagerReviewBoardProcess : MonoSingletonOnlyScene<ManagerReviewBoardProcess> 
{
    //데이터 다 받아진 후 실행될 함수.
    public System.Action<int, bool> dataCompleteHandler;

    //리뷰 데이터.
    private Dictionary<int, SDKBoardArticleCommentInfo> dicReviewDatas = new Dictionary<int, SDKBoardArticleCommentInfo>();

    private int curDataIndex = 0;

    [HideInInspector]
    public SDKReviewBoard board;
    private bool isDicDataSettingComplete = false;

    //------------------------------------------------------------------------------------------------
    public string idCurPlayStageArticle
    {
        get 
        {
            return ReviewBoard.GetArticleID(GameManager.instance.CurrentStage);
        }
    }

    //------------------------------------------------------

    IEnumerator Start ()
    {
        if( Global.SkipStageComment() == false )
        {
            Extension.PokoLog.Log("============ Global.eventIndex" + Global.eventIndex);
            if ( Global.GameInstance.GetProp(GameTypeProp.STAGE_REVIEW_ENABLED) == false )
                yield break;

            this.curDataIndex = 0;
            this.isDicDataSettingComplete = false;

            // ReviewBoardData 초기화_articleData 초기화
            //GameObject objReviewBoard = new GameObject();
            //this.sdkReviewBoardSample = objReviewBoard.AddComponent<SDKProcReviewBoardEvent>();
            //this.sdkReviewBoardSample.name = "SDKReviewBoardSample";
            //this.sdkReviewBoardSample.InitData();
            
            Extension.PokoLog.Log( "============= SDkReviewBoardSample 호출 =============" );
            GameObject            objReviewBoard = new GameObject();
            objReviewBoard.AddComponent<SDKReviewBoardManager>();
            
            // ariticlaData 받아옴
            this.board = new SDKReviewBoard();

            while ( true )
            {
                if ( this.board.isDataSetting )
                {
                    break;
                }
                yield return null;
            }

            this.InitData( "none" );

            while ( true )
            {
                if ( this.isDicDataSettingComplete )
                {
                    break;
                }
                yield return null;
            }
            yield break;
        }   
    }

    //------------------------------------------------------------------------------------------------
    public void SetReviewBoardCommetData ( SDKBoardArticleCommentInfo[] data )
    {
        int length = data.Length;
        this.dicReviewDatas.Clear();
        for ( int i = 0; i < length; i++ )
        {
            this.dicReviewDatas.Add( i, data[i] );
        }
    }


    //------------------------------------------------------------------------------------------------
    // 아이템 데이터를 가져와서 정렬함
    public void InitData ( string order )
    {
        string articleId = ReviewBoard.GetArticleID(GameManager.instance.CurrentStage);
        int commentCount = this.board.GetArticleData( articleId ).infoReview.commentCount;
        if( commentCount != 0 )
        {
            if( commentCount >= 5 )     // 만약 가져온 comment데이터가 5보다 크거나 같으면 
            {
               commentCount = 5;        // 해당 카운트는 5
            }
#if UNITY_EDITOR
            List<SDKBoardArticleCommentInfo> dataList = this.dicReviewDatas.Values.ToList();
            dataList.Sort( (SDKBoardArticleCommentInfo x, SDKBoardArticleCommentInfo y) =>
            {
                return ( order.Equals( "none" ) ) ? x.createdAt.CompareTo( y.likeCount ) : x.likeCount.CompareTo( y.likeCount );
            } );
               SDKReviewBoardManager._instance.GetCommentsOfArticle( this.board.GetBoardID(),"", articleId, commentCount, false, order, "0",
               (SDKBoardArticleCommentInfo[] resultList) =>
               {
                   this.SetReviewBoardCommetData( resultList );
                   this.isDicDataSettingComplete = true;
               },
               () =>
               {
                   this.isDicDataSettingComplete = true;
                   UIPopupStageReview._instance.bCanTouch = true;
               } );

#elif !UNITY_EDITOR
              SDKReviewBoardManager._instance.GetCommentsOfArticle( this.board.GetBoardID(), "",  articleId, commentCount, false, order, "0",
              (SDKBoardArticleCommentInfo[] resultList) =>
              {
                  this.SetReviewBoardCommetData( resultList );
                  this.isDicDataSettingComplete = true;
                Debug.Log(this.isDicDataSettingComplete);
              },
              () =>
              {
                  this.isDicDataSettingComplete = true;
                  UIPopupStageReview._instance.bCanTouch = true;
                Debug.Log(this.isDicDataSettingComplete);
              } );
#endif
        }
        else
        {
            this.isDicDataSettingComplete = true;
        }
    }
    
    public void InitUpdateCommentData ( string idArticle, string commentId, int limited, string order, string offset, 
        System.Action<SDKBoardArticleCommentInfo[], string, string, System.Action> callbackSucessHandler, System.Action callbackFailedHandler )
    {
        SDKReviewBoardManager._instance.GetCommentsOfArticle( this.board.GetBoardID(), commentId, idArticle, limited, false, order, offset,
                                                               ( SDKBoardArticleCommentInfo[] resultList ) =>
                                                               {
                                                                   if (resultList.Length != 0)
                                                                   {
                                                                       callbackSucessHandler(resultList, this.board.GetBoardID()
                                                                                                           , idArticle.ToString(),
                                                                                                           () => { });
                                                                   }
                                                                   else
                                                                   {
                                                                        UIPopupStageReview._instance.moreButton.SetActive(true);
                                                                        UIPopupStageReview._instance.bCanTouch = true;
                                                                   }}, callbackFailedHandler);
    }

    /// <summary>
    /// 리뷰를 등록할 때 호출되는 함수.
    /// 리뷰가 등록될 때, UIPopupStageReview.cs 의 SettingStageArticle 이 호출되면서 해당 함수가 호출됨
    /// </summary>
    public void UpdateComment ( string idArticle, string comment, System.Action callbackCompleteHandler, System.Action callbackAlreadyEnrollReviewHandler, System.Action callbakNGWordReviewHandler )
    {
        SDKReviewBoardManager._instance.AddCommentOfArticle( this.board.GetBoardID(),idArticle, comment, callbackCompleteHandler, callbackAlreadyEnrollReviewHandler, callbakNGWordReviewHandler );
    }

    public SDKBoardArticleCommentInfo GetReviewBoardCommetData ( int index )
    {
        if( this.dicReviewDatas.ContainsKey( index ) )
        {
            this.curDataIndex = index;
            return this.dicReviewDatas[index];
        }

        Extension.PokoLog.Log("Is out of range of the index, give it a value of null.");
        return null;
    }
 
    public SDKBoardArticleCommentInfo[] GetReviewBoardAllCommetData ()
    {
        return this.dicReviewDatas.Values.ToArray();
    }

    public void OnClickBtnUpdateEmotion ()
    {
        string boardId = this.board.GetBoardID();
        string articleId = this.idCurPlayStageArticle;

        SDKReviewBoardManager._instance.AddReactionOfArticle( boardId, articleId, "like", ()=>{} );
    }

    public IEnumerator CompleteLoadReviewData(int _panelDepth, bool bWrite)
    {
        while (true)
        {
            if (this.isDicDataSettingComplete)
            {
                if (dataCompleteHandler != null)
                    dataCompleteHandler(_panelDepth, bWrite);
                break;
            }
            yield return null;
        }
        yield return null;
    }
}
