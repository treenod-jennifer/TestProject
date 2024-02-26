using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_StageAdventure_C : UIReuseGridBase
{
    private int listSize;

    public UISprite scroll_Root;
    public UIPanel scroll_Panel;
    public BoxCollider scroll_Collider;

    protected override void Awake()
    {
        base.Awake();

        onInitializeItem += OnInitializeItem;

        currentPosition = transform.parent;
        previousPosition_X = currentPosition.position.x;
    }

    public void InitReuseGrid()
    {
        listSize = UIPopupStageAdventure._instance.chapterDataList.Count;
        maxIndex = UIPopupStageAdventure._instance.chapterDataList.Count;
        return;
    }

    private bool isChanged = false;
    private bool _isScrolling = false;
    public bool isScrolling { get { return _isScrolling; } }
    private float previousPosition_X;
    private Transform currentPosition;
    public System.Action indexChangedBegin_Callback = null;
    public System.Action<int> indexChangedEnd_Callback = null;
    private void Update()
    {
        #region isScrolling 값 세팅
        if (Mathf.Approximately(previousPosition_X, currentPosition.position.x))
        {
            if (!Global._touching && !Global._touchEnd)
                _isScrolling = false;
        }
        else
        {
            previousPosition_X = currentPosition.position.x;
            _isScrolling = true;
        }
        #endregion

        if (!_isScrolling)
        {
            if (!isChanged)
            {
                isChanged = true;

                selectIndex = Mathf.RoundToInt(currentPosition.localPosition.x / itemSize) * -1;

                if (previousSelectIndex != selectIndex)
                    previousSelectIndex = selectIndex;

                if (indexChangedEnd_Callback != null)
                    indexChangedEnd_Callback(selectIndex);
            }
        }
        else
        {
            if (isChanged)
            {
                isChanged = false;

                if (indexChangedBegin_Callback != null)
                    indexChangedBegin_Callback();
            }
        }
    }

    private int previousSelectIndex;
    private int selectIndex;

    /// <summary>
    /// 스크롤의 초기 위치값 설정
    /// </summary>
    /// <param name="itemIndex">기준 아이템 번호</param>
    public void ScrollItemMove(int itemIndex)
    {
        StartCoroutine(CoScrollItemMove(itemIndex));
    }

    private IEnumerator CoScrollItemMove(int itemIndex)
    {
        int item_count = listSize;
        float pos = itemSize * itemIndex * -1.0f;

        pos = Mathf.Clamp(pos, itemSize * (item_count - 1) * -1.0f, 0.0f);

        while (mPanel == null)
            yield return null;

        SpringPanel.Begin(mPanel.gameObject, Vector3.right * pos, 8.0f);
    }

    //재사용을 위해 위치이동을 하는 Item이 있을때 위치이동을 한 Item의 갯수 만큼 호출
    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex >= maxIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);

        UIItemStageAdventure_C StageAdvent_C_Cell = go.gameObject.GetComponent<UIItemStageAdventure_C>();

        if (UIPopupStageAdventure._instance == null)
            return;

        StageAdvent_C_Cell.SettingItem(UIPopupStageAdventure._instance.chapterDataList[Mathf.Abs(realIndex)]);
        float itemHeight = Mathf.Cos(Mathf.Abs(realIndex) * graphCycle) * itemHeightRange + itemHeightOffset;
        StageAdvent_C_Cell.transform.localPosition = new Vector3(
            StageAdvent_C_Cell.transform.localPosition.x,
            //realIndex % 2 == 0 ? -height : height,
            itemHeight,
            StageAdvent_C_Cell.transform.localPosition.z
            );

        if (Mathf.Abs(realIndex) == 0)
            StageAdvent_C_Cell.WayOff();
        else
        {
            //StageAdvent_C_Cell.WayOn(itemSize, realIndex % 2 == 0 ? height * 2 : height * -2);

            float PreviousItemHeight = Mathf.Cos((Mathf.Abs(realIndex) - 1) * graphCycle) * itemHeightRange + itemHeightOffset;
            StageAdvent_C_Cell.WayOn(itemSize, PreviousItemHeight - itemHeight);
        }
    }

    private const int itemHeightOffset = 0;
    private const int itemHeightRange = 12;
    private const float graphCycle = 2.5f;


    public void SelectChapter(int chapterIdx)
    {
        UIItemStageAdventure_C[] chapterList = transform.GetComponentsInChildren<UIItemStageAdventure_C>();
        for(int i = 0; i < chapterList.Length; ++i)
        {
            if( chapterList[i].GetChapterNumber() == chapterIdx )
            {
                chapterList[i].SelectChapter();
                break;
            }

        }
    }

    public UIItemStageAdventure_C GetChapterItem(int chapterIdx)
    {
        UIItemStageAdventure_C[] chapterList = transform.GetComponentsInChildren<UIItemStageAdventure_C>();
        for (int i = 0; i < chapterList.Length; ++i)
        {
            if (chapterList[i].GetChapterNumber() == chapterIdx)
            {
                return chapterList[i];
            }

        }
        return null;
    }
}
