using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIItemStageReview : MonoBehaviour
{
    public UILabel[] flowerCount;
    public UILabel  userName;
    public UILabel  reviewText;
    public UILabel  likeCount;

    public UISprite bestImg;
    public UISprite likeImg;

    public GameObject moreButton = null;

    // 해당 랭킹 아이템 데이터
    private string boardId;
    private string articleId;
    private string id;
    private string body;
    private bool isLiked;
    private int likes;
    private string nickName;
    private string flower;

    private Action<bool> callbackHandler;
    private int isClickComplete = 0;

    private SDKBoardArticleCommentInfo data;

    public SDKBoardArticleCommentInfo GetConvertCommnetInfo ()
    {
        SDKBoardArticleCommentInfo info = new SDKBoardArticleCommentInfo();
        info.id      = this.id;
        info.body    = this.body;
        info.isLiked = this.isLiked;
        info.likeCount   = this.likes;
        
        //========= default Value
        info.isBest = false;   
        info.isHidden = false;
        info.createdAt = 0;

        return info;
    } 


    // -----------------------------------------------------------------------------------
    /// <summary>
    /// 아이템 데이터 초기화
    /// </summary>
    public void InitData ( string boardId, string articleId, int index, SDKBoardArticleCommentInfo data, Action<bool> callbackHandler)
    {
        this.data = data;

        this.isClickComplete = 0;
        this.id = this.data.id;
        this.boardId = boardId;
        this.articleId = articleId;
        this.body = this.data.body;
        this.isLiked = this.data.isLiked;
        this.likes = System.Convert.ToInt32( this.data.likeCount );
        this.nickName = this.data.profile.name;//( data.profile.ContainsKey( "displayName" ) ) ? data.profile["displayName"] : "GuestID";
        this.flower = this.data.profile.flower.ToString();//( data.profile.ContainsKey( "Count" ) ) ? data.profile["Count"] : "0";

        this.userName.text = this.nickName;
        int length = this.flowerCount.Length;
        for(int i = 0; i < length; i++)
        {
            this.flowerCount[i].text = this.flower;
        }
        this.reviewText.text = this.body;
        this.likeCount.text = this.data.likeCount.ToString();
        this.callbackHandler = callbackHandler;

        //현재 데이터가 마지막 데이터라면 more 버튼.
        if (UIPopupStageReview._instance.GetReviewListLastIndex() == index)
        {
            if (moreButton == null)
            {
                moreButton = UIPopupStageReview._instance.moreButton;
                moreButton.SetActive(false);
                moreButton.transform.parent = transform;
                moreButton.transform.localPosition = new Vector3(0f, -160f, 0f);
            }
            moreButton.SetActive(true);
        }
        else
        {
            if (moreButton != null)
            {
                moreButton.SetActive(false);
                moreButton = null;
            }
        }
    }

    /// <summary>
    /// 해당 코맨트에 추천 버튼 클릭가능
    /// </summary>
    public void OnClickBtnGradeUp ()
    {
        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if ( this.data.userKey == myProfile._userKey)
        {
            if (UIPopupSystem._instance == null)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_rv_6"), false, null);
                popupWarning.SortOrderSetting();
            }
            return;
        }

        if ( this.isClickComplete == 0 )
        {
            this.isClickComplete = 1;
#if UNITY_EDITOR
            DelegateHelper.SafeCall( this.callbackHandler, true );
#elif !UNITY_EDITOR
            SDKReviewBoardManager._instance.UpdateLikeComment( this.boardId, this.articleId, this.id, ( bool isError ) => 
                                                                                                    { 
                                                                                                        if( isError )
                                                                                                        {
                                                                                                            this.isClickComplete = 0;
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            Debug.Log("isError : " + isError);
                                                                                                            this.isClickComplete = 1;
                                                                                                            DelegateHelper.SafeCall( this.callbackHandler, isError ); 
                                                                                                            int count = System.Convert.ToInt32( this.likeCount.text );
                                                                                                            this.data.likeCount = ( count += 1 );
                                                                                                            this.likeCount.text = this.data.likeCount.ToString();
                                                                                                        }
                                                                                                    });
#endif
        }
        else
        {
            if ( UIPopupSystem._instance == null )
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_rv_3" ), false, null );
                popupWarning.SortOrderSetting();
            }
        }
    }
}
