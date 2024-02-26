using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_StageAdventure_S : UIReuseGridBase
{
    private int listSize;

    public UISprite scroll_Root;
    public UIPanel scroll_Panel;
    public BoxCollider scroll_Collider;

    protected override void Awake()
    {
        base.Awake();
        onInitializeItem += OnInitializeItem;
    }

    public void InitReuseGrid()
    {
        listSize = UIPopupStageAdventure._instance.stageDataList.Count;
        minIndex = (listSize - 1) * -1;
    }

    /// <summary>
    /// 스크롤의 초기 위치값 설정
    /// </summary>
    /// <param name="iitemIndex">기준 아이템 번호</param>
    /// <param name="itemOffset">기준 아이템 위로 표시할 아이템 갯수</param>    
    public void ScrollItemMove(int iitemIndex, int itemOffset = 0)
    {
        StartCoroutine(CoScrollItemMove(iitemIndex, itemOffset));
    }

    private IEnumerator CoScrollItemMove(int iitemIndex, int itemOffset = 0)
    {
        iitemIndex = iitemIndex - itemOffset;
        int item_count = listSize;

        float pos = itemSize * iitemIndex;
        float viewHeight = transform.GetComponentInParent<UIPanel>().GetViewSize().y;
        float start_pivot_vertical = ((transform.position.y - (itemSize * 0.5f)) - (transform.parent.transform.position.y - (viewHeight * 0.5f)));
        float maxHeight = (item_count * itemSize) - viewHeight;

        pos = Mathf.Clamp(pos, 0.0f, maxHeight >= 0.0f ? maxHeight : 0.0f) + start_pivot_vertical;

        while (mPanel == null)
            yield return null;

        SpringPanel.Begin(mPanel.gameObject, Vector3.up * pos, 8.0f);
    }

    /// <summary>
    /// 화면 밖으로 스크롤을 이동
    /// </summary>
    public void ScrollItemMoveOutSide()
    {
        StartCoroutine(CoScrollItemMoveOutSide());
    }

    private IEnumerator CoScrollItemMoveOutSide()
    {
        int item_count = listSize;

        float pos = itemSize * (item_count);
        float viewHeight = transform.GetComponentInParent<UIPanel>().GetViewSize().y;
        float start_pivot_vertical = ((transform.position.y - (itemSize * 0.5f)) - (transform.parent.transform.position.y - (viewHeight * 0.5f)));

        pos = pos + start_pivot_vertical;

        while (mPanel == null)
            yield return null;

        SpringPanel.Begin(mPanel.gameObject, Vector3.up * pos, 8.0f);
    }

    /// <summary>
    /// 스크롤뷰의 높이를 변경
    /// </summary>
    /// <param name="height">변경할 높이값</param>
    public void SetHeight(int height)
    {
        int moveDistance = scroll_Root.height - height;
        scroll_Root.height = height;
        scroll_Root.transform.localPosition += Vector3.up * (moveDistance * 0.5f);
        scroll_Panel.baseClipRegion = new Vector4(scroll_Panel.baseClipRegion.x, scroll_Panel.baseClipRegion.y, scroll_Panel.baseClipRegion.z, height);

        scroll_Collider.size = new Vector3(scroll_Collider.size.x, height, scroll_Collider.size.z);
    }

    /// <summary>
    /// 스크롤뷰의 높이를 변경한다.(애니메이션 효과 추가)
    /// </summary>
    /// <param name="height">변경할 높이값</param>
    //public void ScrollHeightExtend(int height)
    //{
    //    StartCoroutine(CoScrollHeightExtend(height));
    //}

    //private IEnumerator CoScrollHeightExtend(int extandHeight)
    //{
    //    float timeCount = 0.0f;
    //    int startHeight = scroll_Root.height;

    //    while (timeCount <= 1.0f)
    //    {
    //        SetHeight((int)Mathf.Lerp(startHeight, extandHeight, m_AniCurve.Evaluate(timeCount)));
    //        timeCount += Time.unscaledDeltaTime * 5.0f;
    //        yield return null;
    //    }
    //}

    //재사용을 위해 위치이동을 하는 Item이 있을때 위치이동을 한 Item의 갯수 만큼 호출
    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);

        UIItemStageAdventure_S StageAdvent_S_Cell = go.gameObject.GetComponent<UIItemStageAdventure_S>();
        StageAdvent_S_Cell.SettingItem(UIPopupStageAdventure._instance.stageDataList[Mathf.Abs(realIndex)]);
    }
    
    /// <summary>
    /// 데이터가 변경된 후 스크롤를 리셋
    /// </summary>
    public void ScrollReset()
    {
        InitReuseGrid();
        
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            if (listSize > i)
            {
                t.gameObject.SetActive(true);
                UIItemStageAdventure_S StageAdvent_S_Cell = t.gameObject.GetComponent<UIItemStageAdventure_S>();
                StageAdvent_S_Cell.SettingItem(UIPopupStageAdventure._instance.stageDataList[i]);
            }
            else
            {
                t.gameObject.SetActive(false);
            }

            t.localPosition = new Vector3(t.localPosition.x, i * itemSize * -1.0f, t.localPosition.z);
        }

        var scroll = GetComponentInParent<UIScrollView>();
        if (scroll != null)
            scroll.ResetPosition();

        ScrollItemMove(0);
    }

    public UIItemStageAdventure_S GetStageItem(int stageIdx)
    {
        UIItemStageAdventure_S[] stageList = transform.GetComponentsInChildren<UIItemStageAdventure_S>();
        for (int i = 0; i < stageList.Length; ++i)
        {
            if (stageList[i].GetStageNumber() == stageIdx)
            {
                return stageList[i];
            }

        }
        return null;
    }

    public void TestS()
    {
        int stageCount = Random.Range(1, 10);

        UIPopupStageAdventure.RefreshStageList();

        ScrollReset();
    }
}
