using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIIngameResultChat : MonoBehaviour
{
    public UIPanel uiPanel;
    public UIScrollView scrollView;
    public UISprite reviewBox;
    public UILabel[] reviewText;
    public BoxCollider reviewCollider;
    public UIContainerEmotionIcon emotionIconContainer;
    public ManagerReviewBoardProcess reviewBoardProcess;

    //에디터에서 테스트 하기 위한 더미 데이터.
    private List<SDKBoardArticleCommentInfo> listDummyData = new List<SDKBoardArticleCommentInfo>();

    //리뷰 텍스트 위치.
    private float yPos = 95f;
    public int panelCount = -1;

    IEnumerator ShowRivewBox ()
    {
        yPos = 95f;
        for (int i = 0; i < 5; i++)
        {
            reviewText[i].transform.localScale = new Vector3(0f, 0f, 1f);
        }
        //원래는 서버에서 데이터 들고오는 시간만큼 기다려야함.
        yield return new WaitForSeconds( 0.3f );
        //Chat 리스트 5개까지만 생성.
        transform.localScale = Vector3.one;
        DOTween.ToAlpha(() => reviewBox.color, x => reviewBox.color = x, 1f, 0.1f);
        reviewBox.transform.DOLocalMoveY(3f, 0.3f).SetEase( Ease.OutBack );

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 5; i++)
        {
            if (reviewText[i].text != "")
            {
                reviewText[i].transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                reviewText[i].transform.localScale = Vector3.one;
            }
        }
        yield return new WaitForSeconds(0.4f);
        SetCollider(true);
    }

    private void OnClickBtnReviewChat()
    {
        if (ManagerUI._instance._popupList.Count <= 0 ||
            !ManagerUI._instance._popupList[ManagerUI._instance._popupList.Count - 1].bCanTouch)
        {
            return;
        }

        if (UIPopupPanelEmotionIcon._instance == null)
        {
            if (UIPopupStageReview._instance == null)
            {
                ManagerUI._instance.OpenPopupStageReview(reviewBoardProcess.idCurPlayStageArticle, SetChatData,
                    emotionIconContainer.gameObject.activeInHierarchy);
            }
        }
    }

    private void SetReviewText(SDKBoardArticleCommentInfo data = null, int index = 0)
    {
        if (data == null)
        {
            if (UIPopupClear._instance != null)
            {
                reviewText[0].text = Global._instance.GetString("p_irv_4");
            }
            else
            {
                reviewText[0].text = Global._instance.GetString("p_irv_6");
            }

            for (int i = 1; i < 5; i++)
            {
                reviewText[i].enabled = false;
            }
            return;
        }
        if (reviewText[index].enabled == false)
            reviewText[index].enabled = true;

        string text = string.Format("[ {0} ] : ", data.profile.name);//( data.profile.ContainsKey( "displayName" ) ) ? data.profile["displayName"] : "GuestID";
        text += data.body;
        reviewText[index].text = text;
        reviewText[index].transform.localPosition = new Vector3(-350f, yPos, 0f);
        yPos -= (reviewText[index].height + 13);
    }

    private void InitChatData ( int length )
    {
        yPos = 95f;
        for ( int i = 0; i < length; i++ )
        {   
            SDKBoardArticleCommentInfo item = ManagerReviewBoardProcess.instance.GetReviewBoardCommetData( i );

            if( item != null )
            {
                this.SetReviewText(item, i);
            }
            else
            {
                if (i == 0)
                {
                    this.SetReviewText();
                }
                break;
            }
        }

        return;
    }

    private void MakeDummyData()
    {
        {
            SDKBoardArticleCommentInfo comment = new SDKBoardArticleCommentInfo();
            comment.body = ".. .......................................................................................................................................";
            listDummyData.Add(comment);
        }

        {
            SDKBoardArticleCommentInfo comment = new SDKBoardArticleCommentInfo();
            comment.body = "I didn't use the item.\nBut I cleared this stage.\nI'm genius!!!";
            listDummyData.Add(comment);
        }

        {
            SDKBoardArticleCommentInfo comment = new SDKBoardArticleCommentInfo();
            comment.body = "If you want to clear this stage, Please clear the left side first.";
            listDummyData.Add(comment);
        }

        {
            for (int i = 0; i < 50; i++)
            {
                SDKBoardArticleCommentInfo comment = new SDKBoardArticleCommentInfo();
                comment.body = i + " 's Comment.";
                listDummyData.Add(comment);
            }
        }
        yPos = 95f;
        reviewText[0].text = "";
        if (listDummyData.Count > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                SetReviewText(listDummyData[i], i);
            }
        }
        else
        {
            SetReviewText();
        }
    }

    public void SettingChatData(int _panelDepth, bool bWrite)
    {
        reviewBoardProcess.dataCompleteHandler = InitChat;
        StartCoroutine(reviewBoardProcess.CompleteLoadReviewData(_panelDepth, bWrite));
    }
    
    public void InitChat (int _panelDepth, bool bWrite)
    {
        SetChatData();
        reviewBox.transform.localPosition = new Vector3( 0f, -200f, 0f );
        
        uiPanel.depth = _panelDepth;
        scrollView.panel.depth = _panelDepth + 1;
        if (panelCount != -1)
        {
            uiPanel.depth += _panelDepth;
            scrollView.panel.depth += _panelDepth;
        }
        
        StartCoroutine( ShowRivewBox() );
        if (bWrite == true)
        {
            SettingEmoticon();
        }
    }

    private void SettingEmoticon()
    {
        this.emotionIconContainer.gameObject.SetActive(true);
        this.emotionIconContainer.InitData(ManagerReviewBoardProcess.instance.idCurPlayStageArticle);
    }

    public void SetChatData()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            this.MakeDummyData();
        }
        else
        {
            this.InitChatData(5);
        }
    }

    public void SetCollider(bool bColl)
    {
        reviewCollider.enabled = bColl;
    }

    public void OnClickBtnHandler ()
    {
        SDKReviewBoardManager._instance.AddReactionOfArticle("Stage", ReviewBoard.GetArticleID(GameManager.instance.CurrentStage), EmotionID.dislike.ToString(), () => { });
    }

    public void OnClickBtnDisLike ()
    {
        SDKReviewBoardManager._instance.DeleteReactionOfArticle("Stage", ReviewBoard.GetArticleID(GameManager.instance.CurrentStage), () => { });
    }
}
