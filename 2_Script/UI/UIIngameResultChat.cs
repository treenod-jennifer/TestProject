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

    //에디터에서 테스트 하기 위한 더미 데이터.
  
    //리뷰 텍스트 위치.
    private float yPos = 95f;

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
        if( UIPopupPanelEmotionIcon._instance == null )
        {
            // 인스턴스가 null이거나 bCanTouch가 true라면
            if ( UIPopupStageReview._instance == null && 
                 UIPopupClear._instance.bCanTouch )
            {
                UIPopupClear._instance.bCanTouch = false;
                ManagerUI._instance.OpenPopupStageReview();
            }
        }
    }

  

    private void InitChatData ( int length )
    {


        return;
    }

    private void MakeDummyData()
    {
  
    }
    
    public void InitChat (int _panelDepth)
    {
        SetChatData();
        reviewBox.transform.localPosition = new Vector3( 0f, -200f, 0f );
        uiPanel.depth = _panelDepth;
        scrollView.panel.depth = _panelDepth + 1;
        StartCoroutine( ShowRivewBox() );
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
        //this.reviewBoardProcess.OnClickBtnUpdateEmotion();
     //   SDKReviewBoardManager._instance.AddReactionOfArticle( "Stage", string.Format( "Stage_{0:D3}", ( UIPopupReady.stageIndex ) ), EmotionID.dislike.ToString(), () => { } );
    }

    public void OnClickBtnDisLike ()
    {
        //this.reviewBoardProcess.OnClickBtnUpdateEmotion();
        //SDKReviewBoardManager._instance.DeleteReactionOfArticle( "Stage", string.Format( "Stage_{0:D3}", ( UIPopupReady.stageIndex ) ), EmotionID.dislike.ToString(), () => { } );
    }
}
