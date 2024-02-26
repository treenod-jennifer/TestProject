using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGridGallery : UIReuseGridBase
{
    public        bool  _useScaleChange   = true;
    public        float _scaleChangeValue = 0.5f;
    private const int   VIEW_COUNT        = 5;
    private       bool  IsNotInfinity => maxIndex < VIEW_COUNT;

    private List<int>  _data;      //데이터
    private GameObject _centerObj; //현재 센터에 있는 오브젝트

    private UICenterOnChild _mCenterOnChild;

    public void Init(List<int> data, UICenterOnChild.OnCenterCallback onClickCenterCallback = null)
    {
        maxIndex        = data.Count;
        _mCenterOnChild = gameObject.GetComponentInChildren<UICenterOnChild>();

        this._data               =  data;
        _mCenterOnChild.onCenter += onClickCenterCallback;

        _mCenterOnChild.onCenter += (obj) => { _centerObj = obj; };

        StartCoroutine(SetScroll());
    }

    //스크롤 초기화
    protected override IEnumerator SetScroll()
    {
        if (gameObject.activeInHierarchy == false)
        {
            yield return null;
        }

        //리스트 읽어오는 동안 기다림.
        while (mPanel == null || mChildren.Count == 0)
        {
            yield return null;
        }

        onInitializeItem += OnInitializeItem;
        mFirstTime       =  true;
        WrapContent();
        mFirstTime = false;
    }

    //1, -1 방향으로 스크롤 이동
    public void MoveRelative(int direct)
    {
        var curCenterIndex = mChildren.FindIndex(x => x == _centerObj.transform);
        direct = (curCenterIndex + direct + mChildren.Count) % mChildren.Count;

        mChildren[direct].SendMessage("OnClick", gameObject, SendMessageOptions.DontRequireReceiver);
        _mCenterOnChild.onCenter.Invoke(_centerObj.gameObject);
    }

    protected override void UpdateItem(Transform item, int index)
    {
        if (onInitializeItem != null)
        {
            var realIndex = Mathf.RoundToInt(item.localPosition.x / itemSize);
            onInitializeItem(item.gameObject, index, realIndex);
        }
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (IsNotInfinity && realIndex >= maxIndex)
        {
            go.SetActive(false);
            return;
        }

        realIndex %= _data.Count;

        if (realIndex < 0)
        {
            realIndex += _data.Count;
        }

        var painting = go.GetComponent<UIItemGallery>();
        painting.UpdateData(_data[realIndex]);

        if (go.activeInHierarchy == false)
        {
            go.SetActive(true);
        }
    }

    //무한으로 돌아가도록 수정
    public override void WrapContent()
    {
        var extents = itemSize * mChildren.Count * 0.5f;
        var corners = mPanel.worldCorners;

        //패널 코너의 월드 좌표를 로컬 좌표로 변환
        for (var i = 0; i < 4; ++i)
        {
            var v = corners[i];
            v          = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        var center = Vector3.Lerp(corners[0], corners[2], 0.5f);
        var ext2   = extents * 2f;

        Transform closedCenterObj      = null;
        var       closedCenterDistance = float.MaxValue;

        for (int i = 0, imax = mChildren.Count; i < imax; ++i)
        {
            var t        = mChildren[i];
            var distance = t.localPosition.x - center.x;

            if (distance < -extents)
            {
                var pos = t.localPosition;
                pos.x += ext2;

                if (IsNotInfinity)
                {
                    var realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (maxIndex <= realIndex && minIndex >= realIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else
                    {
                        UpdateItem(t, i);
                    }
                }
                else
                {
                    t.localPosition = pos;
                    UpdateItem(t, i);
                }
            }
            else if (distance > extents)
            {
                var pos = t.localPosition;
                pos.x -= ext2;

                if (IsNotInfinity)
                {
                    var realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex <= realIndex && realIndex <= maxIndex)
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else
                    {
                        UpdateItem(t, i);
                    }
                }
                else
                {
                    t.localPosition = pos;
                    UpdateItem(t, i);
                }
            }
            else if (mFirstTime)
            {
                UpdateItem(t, i);
            }

            //센터에서 멀어지면 스케일이 줄어들고 그림자가 생김
            var absDistance = Mathf.Abs(t.localPosition.x - center.x);
            if (_useScaleChange)
            {
                var ratioValue = extents / (extents + (absDistance * _scaleChangeValue));
                var scale      = new Vector3(ratioValue, ratioValue);
                mChildren[i].localScale = scale;

                var shadow = (1 - ratioValue * ratioValue) / ratioValue;
                shadow = Mathf.Clamp(shadow, 0, 0.3f);
                UIPopupGallery.instance.SetPaintAlpha(i, shadow);
            }
        }

        mScroll.InvalidateBounds();
    }
}