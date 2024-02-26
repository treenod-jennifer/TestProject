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
    
    public UIInput inputKeyBoard;
    public UILabel[] _labelStage;
    public UILabel[] buttonText;
    public UISprite[] buttonImage;

    public GameObject scrollRoot;
    public GameObject moreButton;

    public bool isEditComplete = false;

    private bool bRecentely = true;
    private ReviewOrderState isCurrentState = ReviewOrderState.none;

    //리뷰 데이터.

    //------------------------------------------------------
    //public UILabel stageGrade;
    [HideInInspector]
    public UIReviewBaordItemContainer itemContainer;

    private UIReviewScroll reviewScroll = null;

    private int sortOrder = 0;

    private bool isClicked = false;
    //------------------------------------------------------
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            UIPopupClear._instance.bCanTouch = true;
            UIPopupStageReview._instance.bCanTouch = false;

            if ( PlayerPrefs.HasKey( "FirstOpenStageReview" ) == false )
            {
                this.isClicked = false;
            }
        }
    }

    void Start ()
    {
        DestoryOrMakeReviewScroll(false);


        if (Global._systemLanguage == CurrLang.eJap)
        {
            _labelStage[0].text = string.Format("ステージ {0}", UIPopupReady.stageIndex);
            _labelStage[1].text = string.Format("ステージ {0}", UIPopupReady.stageIndex);
        }
        else
        {
            _labelStage[0].text = string.Format("STAGE {0}", UIPopupReady.stageIndex);
            _labelStage[1].text = string.Format("STAGE {0}", UIPopupReady.stageIndex);
        }
        _labelStage[0].MakePixelPerfect();
        _labelStage[1].MakePixelPerfect();

        Invoke ( "OpenPopupReviewNotice", 1f );
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


        if (bRecentely == false)
            return; 
        ButtonSetting(false);
    }

    public void OnClickBtnSetStageGrade ()
    {
        if ( UIPopupStageReview._instance.bCanTouch == false )
            return;

        UIPopupStageReview._instance.bCanTouch = false;
        //ManagerReviewBoardProcess.instance.UpdateStageGrade( ManagerReviewBoardProcess.instance.idCurPlayStageArticle );
    }

    private void UpdateCommentCompleteHandler ( string comment )
    {

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

    public void DestoryOrMakeReviewScroll(bool bDestroy)
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
            reviewScroll = NGUITools.AddChild(scrollRoot, ManagerUI._instance._objReviewScroll).GetComponent<UIReviewScroll>();
            itemContainer = reviewScroll.itemContainer;
            reviewScroll.SettingDepth(uiPanel.depth + 1);
            reviewScroll.SettingSortOrder(sortOrder);
        }
    }
}
