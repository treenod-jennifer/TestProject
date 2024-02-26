using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_EpisodeScroll : UIReuseGridBase
{
    UIItemEpScrollView epScroll;

    private float startPos;

    protected override void Awake()
    {
        epScroll = GetComponentInParent<UIItemEpScrollView>();
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        minIndex = (epScroll._listChapterData.Count - 1) * -1;
        StartCoroutine(SetScroll());

        startPos = mPanel.transform.localPosition.y;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }

        ScrollItemMove(epScroll.currentEpisode);
    }

    public void ScrollItemMove(int chapterIdx)
    {
        StartCoroutine(WrapScroll(chapterIdx));
    }

    private IEnumerator WrapScroll(int myIndex)
    {
        yield return new WaitWhile(() => mPanel == null || mChildren.Count == 0);

        float height = itemSize  * epScroll.GetEpisodeDataCount() - (mPanel.GetViewSize().y - mPanel.clipSoftness.y * 2.0f);

        float maxHeight = startPos + Mathf.Max(0.0f, height);

        float moveHight = Mathf.Min(startPos + itemSize * (myIndex - 4 < 0 ? 0 : myIndex - 4), maxHeight);

        Vector3 pos = mPanel.transform.localPosition;
        pos.y = moveHight;
        mPanel.transform.localPosition = pos;

        Vector2 offset = mPanel.clipOffset;
        offset.y = moveHight * -1.0f;
        mPanel.clipOffset = offset;

        SetContent();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if(realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemEpisode cellEpisode = go.gameObject.GetComponent<UIItemEpisode>();
        cellEpisode.UpdateData(epScroll.GetEpisodeData(realIndex * -1));
    }

}
