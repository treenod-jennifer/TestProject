using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Review : UIReuseGridBase
{
    string idCurPlayStageArticle = "";

    protected override void Awake()
    {
        UIPopupStageReview._instance.callbackDataComplete += InitReuseGrid;
        UIPopupStageReview._instance.moreSetting += MoreSettingReuseGrid;
        base.Awake();
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopupStageReview._instance.GetStageReviewCount()) * -1;
        idCurPlayStageArticle = UIPopupStageReview._instance.GetReviewArticle();
        StartCoroutine(SetScroll());
        return;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();
        //팝업켜지는 시간동안 대기.
        yield return new WaitForSeconds(0.3f);
        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    //more 버튼 눌렀을 때 데이터 세팅.
    private void MoreSettingReuseGrid()
    {
        minIndex = (UIPopupStageReview._instance.GetStageReviewCount()) * -1;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }
        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemStageReview reviewCell = go.gameObject.GetComponent<UIItemStageReview>();
       
        reviewCell.InitData( "Stage", idCurPlayStageArticle, realIndex * -1, UIPopupStageReview._instance.GetStageReviewData( realIndex * -1 ), ( bool isError ) => 
        {
            // 코맨트 Count만 갱신
            if( isError == false )
            { 
                SDKBoardArticleCommentInfo data = UIPopupStageReview._instance.GetStageReviewData(realIndex * -1);
                data.likeCount = System.Convert.ToInt32(reviewCell.likeCount.text);
                UIPopupStageReview._instance.itemContainer.InitData( data, realIndex * -1 );
            }
        });
    }
}
