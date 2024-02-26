using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_WorldRanking : UIReuseGridBase
{
    public bool IsReady { get; private set; } = false;
    private UIScrollView uIScrollView;

    protected override void Awake()
    {
        uIScrollView = GetComponentInParent<UIScrollView>();
        onInitializeItem += OnInitializeItem;
    }

    private void InitReuseGrid()
    {
        minIndex = (UIPopUpWorldRank._instance.GetWorldRankingDataCount() - 1) * -1;
        StartCoroutine(SetScroll());
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        base.OnInitializeItem(go, wrapIndex, realIndex);

        if (go.activeSelf)
        {
            UIItemWorldRank_UserInfo rankingCell = go.gameObject.GetComponent<UIItemWorldRank_UserInfo>();
            rankingCell.UpdateData(UIPopUpWorldRank._instance.GetWorldRankingData(realIndex * -1));
        }
    }

    public void ScrollReset()
    {
        uIScrollView?.ResetPosition();

        for (int i = 0; i < mChildren.Count; i++)
        {
            mChildren[i].localPosition = Vector3.down * i * itemSize;
        }

        InitReuseGrid();
    }

    public void ScrollReset(int myIndex, int offsetIndex = 0)
    {
        ScrollReset();

        myIndex = Mathf.Clamp(myIndex - offsetIndex, 1, ManagerWorldRanking.RANKING_PAGE_SIZE);
        
        StartCoroutine(WrapScroll(myIndex));
    }

    /// <summary>
    /// 스크롤의 동작을 온오프 시키는 기능
    /// </summary>
    /// <param name="isActive"></param>
    public void ActiveScroll(bool isActive)
    {
        uIScrollView.enabled = isActive;
    }

    /// <summary>
    /// 알파값을 0 또는 1로 세팅
    /// </summary>
    /// <param name="isHide"></param>
    public void HideScroll(bool isHide)
    {
        uIScrollView.panel.alpha = isHide ? 0.0f : 1.0f;
    }

    private IEnumerator WrapScroll(int myIndex)
    {
        yield return new WaitWhile(() => mPanel == null || mChildren.Count == 0);

        float startPos = mPanel.transform.localPosition.y;

        float height = itemSize * UIPopUpWorldRank._instance.GetWorldRankingDataCount() - (mPanel.GetViewSize().y - mPanel.clipSoftness.y * 2.0f);

        float maxHeight = startPos + Mathf.Max(0.0f, height);

        float moveHight = Mathf.Min(startPos + itemSize * (myIndex - 1), maxHeight);

        Vector3 pos = mPanel.transform.localPosition;
        pos.y = moveHight;
        mPanel.transform.localPosition = pos;

        Vector2 offset = mPanel.clipOffset;
        offset.y = moveHight * -1.0f;
        mPanel.clipOffset = offset;

        SetContent();

        IsReady = true;
    }

    public Transform GetItem(string userKey)
    {
        //test
        //var testData = mChildren.FindAll((item) => item.gameObject.activeSelf);
        //return testData[Random.Range(0, testData.Count)];
        ///

        return mChildren.Find
        (
            (item) =>
            {
                if (item.gameObject.activeSelf)
                {
                    var userInfo = item.GetComponent<UIItemWorldRank_UserInfo>();
                    return userInfo.CheckItem(userKey);
                }
                else
                {
                    return false;
                }
            }
        );
    }
}
