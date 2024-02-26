using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIReuseGridBase : UIWrapContent
{
    protected virtual void Awake()
    {
        onInitializeItem = null;
    }

    protected override void Start()
    {
        SortBasedOnScrollMovement();
        if (mScroll != null) mScroll.GetComponent<UIPanel>().onClipMove = OnMove;
        mFirstTime = false;
        return;
    }

    protected virtual IEnumerator SetScroll()
    {
        if (gameObject.activeInHierarchy == false)
            yield return null;

        //리스트 읽어오는 동안 기다림.
        while (mPanel == null || mChildren.Count == 0)
        {
            yield return null;
        }

        for (int i = 0; i < mChildren.Count; i++)
        {
            mChildren[i].gameObject.SetActive(false);
        }
        yield return null;
    }

    protected virtual void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }
        if (go.activeInHierarchy == false)
            go.SetActive(true);
    }

    //min 인덱스가 0일 때 무한으로 돌아가는 조건 제거.
    public override void WrapContent()
    {
        float extents = itemSize * mChildren.Count * 0.5f;
        Vector3[] corners = mPanel.worldCorners;

        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = corners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
        float ext2 = extents * 2f;

        if (mHorizontal)
        {
            float min = corners[0].x - itemSize;
            float max = corners[2].x + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.x - center.x;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x += ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex <= realIndex && realIndex <= maxIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex <= realIndex && realIndex <= maxIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.x - mTrans.localPosition.x;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }
        else
        {
            float min = corners[0].y - itemSize;
            float max = corners[2].y + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.y - center.y;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y += ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex <= realIndex && realIndex <= maxIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y -= ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex <= realIndex && realIndex <= maxIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.y - mTrans.localPosition.y;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }

        //mScroll.restrictWithinPanel = !allWithinRange;
        mScroll.InvalidateBounds();
    }

    /// <summary>
    /// 스크롤바를 이용해 한번에 큰 거리를 이동할 경우 item들의 위치를 다시 설정해준다.
    /// </summary>
    public void SetContent()
    {
        if (mPanel == null)
            CacheScrollView();

        if (mHorizontal)
        {
            Vector4 clip = mPanel.finalClipRegion;
            int intViewSize = Mathf.RoundToInt(clip.z);
            if ((intViewSize & 1) != 0) intViewSize -= 1;
            float halfViewSize = intViewSize * 0.5f;
            halfViewSize = Mathf.Round(halfViewSize);

            float startPosition = (halfViewSize - itemSize * 0.5f);

            float scrollViewHeight = transform.parent.localPosition.x;
            int count = (int)(scrollViewHeight / itemSize);
            int itemStartPosition = itemSize * count * -1;

            for (int i = 0; i < mChildren.Count; i++)
            {
                Vector3 pos = mChildren[i].transform.localPosition;
                pos.x = itemStartPosition - (itemSize * i);
                mChildren[i].transform.localPosition = pos;
                UpdateItem(mChildren[i], i);
            }
        }
        else
        {
            Vector4 clip = mPanel.finalClipRegion;
            int intViewSize = Mathf.RoundToInt(clip.w);
            if ((intViewSize & 1) != 0) intViewSize -= 1;
            float halfViewSize = intViewSize * 0.5f;
            halfViewSize = Mathf.Round(halfViewSize);

            float startPosition = (halfViewSize - itemSize * 0.5f);

            float scrollViewHeight = transform.parent.localPosition.y;
            int count = (int)(scrollViewHeight / itemSize);
            int itemStartPosition = itemSize * count * -1;

            for (int i = 0; i < mChildren.Count; i++)
            {
                Vector3 pos = mChildren[i].transform.localPosition;
                pos.y = itemStartPosition - (itemSize * i);
                mChildren[i].transform.localPosition = pos;
                UpdateItem(mChildren[i], i);
            }
        }
    }

    /// <summary>
    /// 스크롤바 이동 연출용(재사용 스크롤에서 SpringPanel을 사용하면 큰값을 이동할때 화면 갱신이 재대로 안되는 문제가 있다. 이 함수 사용시 문제 없이 사용 가능)
    /// </summary>
    /// <param name="target">해당 스크롤뷰에 있는 스크롤 바</param>
    /// <param name="position">위치 값 (0.0 ~ 1.0)</param>
    /// <param name="time">연출 시간</param>
    /// <returns></returns>
    public static IEnumerator ReuseScrollMove(UIProgressBar target, System.Func<float, float> animationCurve, Vector2 position, float time = 0.0f)
    {
        float totalTime = 0.0f;
        float startValue = target.value;
        float endValue = Mathf.Approximately(position.x, 0.0f) ? position.y : position.x;
        endValue = Mathf.Clamp01(endValue);

        if(Mathf.Approximately(time, 0.0f))
        {
            target.value = endValue;
            yield break;
        }

        while (totalTime < time)
        {
            totalTime += Global.deltaTime;
            float normalizedTime = Mathf.Clamp01(totalTime / time);
            float animationCurveTime = animationCurve(normalizedTime);
            target.value = Mathf.Lerp(startValue, endValue, animationCurveTime);
            yield return null;
        }
    }

    public static IEnumerator ReuseScrollMove(UIProgressBar target, Vector2 position, float time = 0.0f)
    {
        return ReuseScrollMove(target, (normalizedTime) => Mathf.Sin(Mathf.PI * 0.5f * normalizedTime), position, time);
    }

    public static IEnumerator ReuseScrollMove(UIProgressBar target, AnimationCurve animationCurve, Vector2 position)
    {
        float endTime = animationCurve.length == 0 ? 0.0f : animationCurve[animationCurve.length - 1].time;
        return ReuseScrollMove(target, (normalizedTime) => animationCurve.Evaluate(normalizedTime), position, endTime);
    }

    /// <summary>
    /// 스크롤 뷰 시작 위치 설정
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <param name="itemDataCount">리스트에 표기될 데이터 총 갯수</param>
    /// <param name="startDataIndex">시작 위치로 설정 할 데이터 인덱스</param>
    protected async UniTask AsyncInitReuseGrid(CancellationToken ct, int itemDataCount, int startDataIndex)
    {
        if (startDataIndex > 0)
        {
            await UniTask.WaitUntil(() => mPanel != null || mChildren.Count > 0, cancellationToken: ct);

            var startPos   = mPanel.transform.localPosition.y;
            var height     = itemSize * itemDataCount - (mPanel.GetViewSize().y - mPanel.clipSoftness.y * 2.0f);
            var maxHeight  = startPos             + Mathf.Max(0.0f, height);
            var moveHeight = Mathf.Min(startPos + itemSize * (startDataIndex - 1), maxHeight);
            var pos        = mPanel.transform.localPosition;

            pos.y                          = moveHeight;
            mPanel.transform.localPosition = pos;

            var offset = mPanel.clipOffset;
            offset.y          = moveHeight * -1.0f;
            mPanel.clipOffset = offset;
        }

        SetContent();
    }
}
