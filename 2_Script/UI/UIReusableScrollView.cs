using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일반적인 재사용 스크롤을 위해서는 사용할 필요 없음
/// 재사용 스크롤 + 스크롤바를 구현해야 하는 경우 사용
/// </summary>
public class UIReusableScrollView : UIScrollView
{
    private UIReuseGridBase GridBase;
    private UIReuseGridBase gridBase
    {
        get
        {
            if(GridBase == null)
            {
                GridBase = transform.GetComponentInChildren<UIReuseGridBase>();
            }

            return GridBase;
        }
    }

    /// <summary>
    /// 일반적인 상황에서는 0으로 세팅
    /// 스크롤 아이템 이외의 특별한 오브젝트가 스크롤 내부에 들어가는 경우 그 오브젝트의 크기를 세팅하여 사용
    /// </summary>
    [Tooltip("스크롤 아이템 이외의 특별한 오브젝트가 스크롤 내부에 들어가는 경우 그 오브젝트의 크기를 세팅하여 사용")]
    [SerializeField] private float startPositionOffset = 0.0f;

    /// <summary>
    /// 스크롤을 처음으로 위치 했을때 스크롤뷰가 붙어 있는 오브젝트의 로컬위치값
    /// </summary>
    [Tooltip("스크롤을 처음으로 위치 했을때 스크롤뷰컴포넌트가 붙어 있는 오브젝트(현재 오브젝트)의 로컬위치값")]
    [SerializeField] private float startPosition = 0.0f;

    public override void UpdateScrollbars(bool recalculateBounds)
    {
        if (mPanel == null) return;

        if (horizontalScrollBar != null || verticalScrollBar != null)
        {
            if (recalculateBounds)
            {
                mCalculatedBounds = false;
                mShouldMove = shouldMove;
            }

            if (verticalScrollBar != null)
            {
                Vector4 clip = mPanel.finalClipRegion;
                int intViewSize = Mathf.RoundToInt(clip.w);
                if ((intViewSize & 1) != 0) intViewSize -= 1;
                float halfViewSize = intViewSize * 0.5f;
                halfViewSize = Mathf.Round(halfViewSize);

                if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
                    halfViewSize -= mPanel.clipSoftness.y;

                float viewSize = halfViewSize * 2f;
                float contentSize = gridBase.itemSize * (Mathf.Abs(gridBase.minIndex) + 1) + startPositionOffset;
                float contentMax = gridBase.transform.parent.localPosition.y - startPosition;
                float contentMin = contentSize - viewSize - contentMax;

                UpdateScrollbars(verticalScrollBar, contentMin, contentMax, contentSize, viewSize, true);
            }

            if (horizontalScrollBar != null)
            {
                Vector4 clip = mPanel.finalClipRegion;
                int intViewSize = Mathf.RoundToInt(clip.z);
                if ((intViewSize & 1) != 0) intViewSize -= 1;
                float halfViewSize = intViewSize * 0.5f;
                halfViewSize = Mathf.Round(halfViewSize);

                if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
                    halfViewSize -= mPanel.clipSoftness.x;

                float viewSize = halfViewSize * 2f;
                float contentSize = gridBase.itemSize * (Mathf.Abs(gridBase.maxIndex)) + startPositionOffset;
                float contentMax = (gridBase.transform.parent.localPosition.x - startPosition) * -1.0f;
                float contentMin = contentSize - viewSize - contentMax;

                UpdateScrollbars(horizontalScrollBar, contentMin, contentMax, contentSize, viewSize, true);
            }
        }
        else if (recalculateBounds)
        {
            mCalculatedBounds = false;
        }
    }

    public override void SetDragAmount(float x, float y, bool updateScrollbars)
    {
        if (mPanel == null) mPanel = GetComponent<UIPanel>();

        DisableSpring();

        if (!updateScrollbars)
        {
            if (canMoveVertically)
            {
                Vector4 clip = mPanel.finalClipRegion;
                int intViewSize = Mathf.RoundToInt(clip.w);
                if ((intViewSize & 1) != 0) intViewSize -= 1;
                float halfViewSize = intViewSize * 0.5f;
                halfViewSize = Mathf.Round(halfViewSize);

                float endPosition = gridBase.itemSize * (Mathf.Abs(gridBase.minIndex) + 1) + startPositionOffset - halfViewSize * 2.0f + startPosition;

                Vector3 pos = mTrans.localPosition;
                pos.y = Mathf.Lerp(startPosition, endPosition, y);
                mTrans.localPosition = pos;

                Vector4 cr = mPanel.baseClipRegion;
                mPanel.clipOffset = new Vector2(0.0f, mTrans.localPosition.y * -1.0f);

                gridBase.SetContent();
                gridBase.WrapContent();
            }

            if (canMoveHorizontally)
            {
                Vector4 clip = mPanel.finalClipRegion;
                int intViewSize = Mathf.RoundToInt(clip.z);
                if ((intViewSize & 1) != 0) intViewSize -= 1;
                float halfViewSize = intViewSize * 0.5f;
                halfViewSize = Mathf.Round(halfViewSize);

                float endPosition = (gridBase.itemSize * Mathf.Abs(gridBase.maxIndex) + startPositionOffset - (halfViewSize * 2.0f + startPosition)) * -1.0f;

                Vector3 pos = mTrans.localPosition;
                pos.x = Mathf.Lerp(startPosition, endPosition, x);
                mTrans.localPosition = pos;

                Vector4 cr = mPanel.baseClipRegion;
                mPanel.clipOffset = new Vector2(mTrans.localPosition.x * -1.0f, 0.0f);

                gridBase.SetContent();
                gridBase.WrapContent();
            }
        }

        if (updateScrollbars) UpdateScrollbars(mDragID == -10);
    }
}
