using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageReview : UIPopupBase
{
    public enum ReviewOrderState   
    {
        none, // 최신순
        like, // 좋아요 순
    }
    static public UIPopupStageReview _instance = null;
    
    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;
    //more 버튼 눌렀을 때 함수.
    public Method.FunctionVoid moreSetting = null;
    //데이터 새로 갱신할 때 함수.
    public Method.FunctionVoid callbackDataSetting = null;

    public UIInput inputKeyBoard;
    public UILabel reviewInfoText;
    public UILabel[] _labelStage;
    public UILabel[] buttonText;
    public UISprite[] buttonImage;
    
    public GameObject scrollRoot;
    public GameObject moreButton;
    public GameObject reviewTopUI;

    public bool isEditComplete = false;

    private bool bRecentely = true;
    private ReviewOrderState isCurrentState = ReviewOrderState.none;

    //리뷰 데이터.
    private List<SDKBoardArticleCommentInfo> listReviewDatas = new List<SDKBoardArticleCommentInfo>();

    //------------------------------------------------------
    //public UILabel stageGrade;
    public UICommentUpdater uiCommentUpdater;
    [HideInInspector]
    public UIReviewBaordItemContainer itemContainer;

    private UIReviewScroll reviewScroll = null;

    private int sortOrder = 0;

    private bool isClicked = false;

    private string idCurPlayStageArticle;
    //------------------------------------------------------
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            //UIPopupClear._instance.bCanTouch = true;
            UIPopupStageReview._instance.bCanTouch = false;

            if ( PlayerPrefs.HasKey( "FirstOpenStageReview" ) == false )
            {
                this.isClicked = false;
            }
        }
    }

    void Start ()
    {
        string text = $"{Global._instance.GetString("p_sr_1")} {GameManager.instance.CurrentStage}";

        _labelStage[0].text = text;
        _labelStage[1].text = text;

        _labelStage[0].MakePixelPerfect();
        _labelStage[1].MakePixelPerfect();

        Invoke ( "OpenPopupReviewNotice", 1f );
    }

    public void SettingStageArticle(string article, Method.FunctionVoid reviewChatFunc)
    {
        idCurPlayStageArticle = article;
        callbackDataSetting += reviewChatFunc;
        DestroyOrMakeReviewScroll(false);

#if UNITY_EDITOR
        Debug.Log(JsonFx.Json.JsonWriter.Serialize(this.listReviewDatas));
        // 코맨트 데이터 초기화
        if (this.listReviewDatas.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                this.listReviewDatas.Add(new SDKBoardArticleCommentInfo(i.ToString(), string.Format("{0}_Comment", i), i));
            }
        }

        itemContainer.InitData(this.listReviewDatas.ToArray(), "Stage", idCurPlayStageArticle, () => { });
        // 업데이트시 초기화
        this.uiCommentUpdater.InitData((string comment, System.Action NGWordReviewHandler) =>
        {
            this.listReviewDatas.Add(new SDKBoardArticleCommentInfo(this.listReviewDatas.Count.ToString(), comment, this.listReviewDatas.Count));
            this.OnClickBtnRecently();
        });


#elif !UNITY_EDITOR
        this.isCurrentState = ReviewOrderState.none;
        this.uiCommentUpdater.InitData( (string comment, System.Action NGWordReviewHandler) =>
        {
            UIPopupStageReview._instance.bCanTouch = false;
            ManagerReviewBoardProcess.instance.UpdateComment( idCurPlayStageArticle, comment, this.UpdateCommentCompleteHandler,
                                                                                                                                        this.UpdateCommentAlreadyEnrollReviewHandler,
                                                                                                                                        NGWordReviewHandler );

        } );

        ManagerReviewBoardProcess.instance.InitUpdateCommentData( idCurPlayStageArticle, "", 5, this.isCurrentState.ToString(), "0", itemContainer.InitData, 
        () => {
                 if ( PlayerPrefs.HasKey( "FirstOpenStageReview" ))
                 {
                    UIPopupStageReview._instance.bCanTouch = true;
                 }
        } );
#endif
    }

    //------------------------------------------------------------------------------------------------
    public void OpenPopupReviewNotice ()
    {
        if ( PlayerPrefs.HasKey( "FirstOpenStageReview" ) == false )
        {
            UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
            popupWarning.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_rv_1" ), false );
            PlayerPrefs.SetString( "FirstOpenStageReview", "true" );
        }

        UIPopupStageReview._instance.bCanTouch = true;
        this.isClicked = true;
    }

    // Use this for initialization
    public void OnClickBtnRecently ()
    {
        if ( UIPopupStageReview._instance.bCanTouch == false )
            return;

        UIPopupStageReview._instance.bCanTouch = false;
        this.isCurrentState = ReviewOrderState.none;

#if UNITY_EDITOR
        //리뷰 삭제 후 생성.
        DestroyOrMakeReviewScroll(true);
        DestroyOrMakeReviewScroll(false);
        // sort
        this.listReviewDatas.Sort( (SDKBoardArticleCommentInfo x, SDKBoardArticleCommentInfo y) =>
        {
            return ( x.createdAt.CompareTo( y.createdAt ) ) * -1;
        } );

        // sortDataSetting
        itemContainer.InitData( this.listReviewDatas.ToArray(), "Stage", idCurPlayStageArticle, () => { UIPopupStageReview._instance.bCanTouch = true; } );

#elif !UNITY_EDITOR
        ManagerReviewBoardProcess.instance.InitUpdateCommentData(idCurPlayStageArticle, "", 5, this.isCurrentState.ToString(), "0", itemContainer.InitData ,
        () => {
          UIPopupStageReview._instance.bCanTouch = true;
        });
#endif
        if (bRecentely == true)
            return;
        ButtonSetting(true); 
    }

    protected override void OnClickBtnClose ()
    {
        if ( UIPopupStageReview._instance.bCanTouch == false || this.isClicked == false )
            return;

        UIPopupStageReview._instance.bCanTouch = false;
        ManagerUI._instance.ClosePopUpUI();
    }

    public void OnClickBtnRecommend ()
    {
        if ( UIPopupStageReview._instance.bCanTouch == false )
            return;

        UIPopupStageReview._instance.bCanTouch = false;
        this.isCurrentState = ReviewOrderState.like;

#if UNITY_EDITOR
        //리뷰 삭제 후 생성.
        DestroyOrMakeReviewScroll(true);
        DestroyOrMakeReviewScroll(false);
        // sort
        this.listReviewDatas.Sort( (SDKBoardArticleCommentInfo x, SDKBoardArticleCommentInfo y) =>
        {
            return ( x.likeCount.CompareTo( y.likeCount ) ) * -1;
        } );

#elif !UNITY_EDITOR      
        ManagerReviewBoardProcess.instance.InitUpdateCommentData(idCurPlayStageArticle, "", 5, this.isCurrentState.ToString(), "0", itemContainer.InitData ,
          () => {
          UIPopupStageReview._instance.bCanTouch = true;
        });

#endif
        if (bRecentely == false)
            return; 
        ButtonSetting(false);
    }
    
    private void UpdateCommentCompleteHandler()
    {
        ManagerReviewBoardProcess.instance.InitUpdateCommentData(idCurPlayStageArticle, "", 5, this.isCurrentState.ToString(), "0", itemContainer.InitData, 
              () => {
          UIPopupStageReview._instance.bCanTouch = true;
      });
    }

    private void UpdateCommentAlreadyEnrollReviewHandler ()
    {
        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
        popupWarning.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_rv_4" ), true, null );
        popupWarning.SetButtonText( 1, Global._instance.GetString( "btn_2" ) );
        popupWarning.SetButtonText( 2, Global._instance.GetString( "btn_25" ) );
        popupWarning.FunctionSetting( 1, "ClosePopupHandler", this.gameObject, true );
        popupWarning.FunctionSetting( 2, "RemoveSelfCommentData", this.gameObject, true );
        popupWarning.HideCloseButton();
    }

    private void RemoveSelfCommentData ()
    {
        // 자신의 코맨트 ID 가져옴
        SDKReviewBoardManager._instance.GetSpecificCommentOfArticle( "Stage", idCurPlayStageArticle,
            (SDKBoardArticleCommentInfo[] data) =>
            {
                if ( data.Length > 0 )
                {
                    // callback 성공시 자신의 코맨트 ID 지움
                    SDKReviewBoardManager._instance.DeleteMyComment( "Stage", idCurPlayStageArticle, data[0].id, ( bool isError ) =>
                    {
                        this.uiCommentUpdater.UpdateCurrentComment();
                    } );
                }
            } );
    }


    //------------------------------------------------------------------------------------------------
    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            sortOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void OnClickBtnMore ()
    {
#if UNITY_EDITOR
#elif !UNITY_EDITOR
        this.bCanTouch = false;

        string offset = ( this.isCurrentState.Equals( ReviewOrderState.like ) ) ? this.listReviewDatas.Count.ToString() : "0";
        int limit =  5;//( this.isCurrentState.Equals( ReviewOrderState.like ) ) ? ( this.listReviewDatas.Count + 4 ) : 5;

        string commentId = ( this.listReviewDatas.Count == 0 ) ? "" : this.listReviewDatas[this.listReviewDatas.Count - 1].id;
        Debug.Log( "----------- OnClickBtnMore [ CommentId ] : " + commentId );
        ManagerReviewBoardProcess.instance.InitUpdateCommentData( idCurPlayStageArticle, commentId, limit, this.isCurrentState.ToString(), offset, itemContainer.InitAddData,
          () => {
          UIPopupStageReview._instance.bCanTouch = true;
        });
#endif

    }

    private void OnClickTextBox()
    {
        inputKeyBoard.isSelected = true;
        StartCoroutine(StartWriteString());
    }

    private IEnumerator StartWriteString()
    {
        yield return new WaitForEndOfFrame();

        while (this.isEditComplete == false)
        {
            if (this.isEditComplete)
            {
                break;
            }
            yield return null;
        }
        yield break;
    }

    private void OnClickBtnWrite()
    {
        if (inputKeyBoard.value.Length > 0)
        {
            inputKeyBoard.isSelected = false;
            isEditComplete = true;
        }
    }

    public SDKBoardArticleCommentInfo GetStageReviewData(int index)
    {
        if (listReviewDatas.Count <= index || listReviewDatas[index] == null)
            return null;

        return listReviewDatas[index];
    }

    public int GetStageReviewCount()
    {
        return (listReviewDatas.Count - 1);
    }

    public string GetReviewArticle()
    {
        return idCurPlayStageArticle;
    }

    public void SetStageReviewData ( SDKBoardArticleCommentInfo[] data )
    {   
        this.listReviewDatas.Clear();
        for(int i = 0; i < data.Length; i++)
        {
            this.listReviewDatas.Add(data[i]);
        }
        if (callbackDataComplete != null && listReviewDatas.Count > 0)
        {
            callbackDataComplete();
        }
    }

    public void SettingPreviewChatData()
    {
        if (callbackDataSetting != null)
        {
            callbackDataSetting();
        }
    }

    public void SetAddStageReviewData ( SDKBoardArticleCommentInfo[] data )
    {
        for ( int i = 0; i < data.Length; i++ )
        {
            this.listReviewDatas.Add( data[i] );
        }

        if (moreSetting != null && listReviewDatas.Count > 0)
        {
            moreSetting();
        }
    }

    public void SetStageReviewData ( SDKBoardArticleCommentInfo data, int index )
    {
        this.listReviewDatas[index] = data;
    }

    public int GetReviewListLastIndex()
    {
        return listReviewDatas.Count - 1;
    }

    private void ButtonSetting(bool brecen)
    {
        bRecentely = brecen;
        int active = 0;
        int disactive = 0;
        if (bRecentely == true)
        {
            active = 1;
        }
        else
        {
            disactive = 1;
        }

        buttonImage[active].spriteName = "recommend_box";
        buttonImage[disactive].spriteName = "diary_todo_title_box";
        buttonText[active].color = new Color(120f / 255f, 145f / 255f, 161f / 255f);
        buttonText[disactive].color = new Color(42f / 255f, 93f / 255f, 139f / 255f);
    }

    public void DestroyOrMakeReviewScroll(bool bDestroy)
    {
        if (bDestroy == true)
        {
            moreButton.transform.parent = scrollRoot.transform;
            moreButton.transform.localPosition = new Vector3(0, -400, 0);
            moreButton.SetActive(false);
            if (reviewScroll != null)
                Destroy(reviewScroll.gameObject);
            if (itemContainer != null)
                Destroy(itemContainer.gameObject);
            reviewScroll = null;
            itemContainer = null;
            
            callbackDataComplete = null;
            moreSetting = null;
        }
        else
        {
            reviewScroll = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIReviewScroll", scrollRoot).GetComponent<UIReviewScroll>();
            itemContainer = reviewScroll.itemContainer;
            reviewScroll.SettingDepth(uiPanel.depth + 1);
            reviewScroll.SettingSortOrder(sortOrder);
        }
    }

    public void SettingDoNotUseWrite()
    {
        reviewTopUI.transform.localPosition = new Vector3(0f, -40f, 0f);
        uiCommentUpdater.gameObject.SetActive(false);
        reviewInfoText.transform.localPosition = new Vector3(reviewInfoText.transform.localPosition.x, reviewInfoText.transform.localPosition.y + 40f, 0f);
        mainSprite.height = 1120;
    }
}
