using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Stage : UIReuseGridBase
{
    UIPopupStage popupStage;

    protected override void Awake()
    {
        popupStage = GetComponentInParent<UIPopupStage>();
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        minIndex = (popupStage.GetBtnStageCount() - 1) * -1;
        StartCoroutine(SetScroll());
        return;
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();

        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }

        if (scrollMode == ScrollMode.None)
        {
            ScrollItemMove(popupStage.GetCurrentIndex(), true);
        }
        else if(scrollMode == ScrollMode.FlowerFind)
        {
            ScrollItemMove(GetMinFlowerStageIndex(3));
        }
        else if (scrollMode == ScrollMode.BlueFlowerFind)
        {
            ScrollItemMove(GetMinFlowerStageIndex(4));
        }
        else if (scrollMode == ScrollMode.RedFlowerFind)
        {
            ScrollItemMove(GetMinFlowerStageIndex(5));
        }
        else if(scrollMode == ScrollMode.CustomFind)
        {
            ScrollItemMove(customModeIndex);
        }
    }

    public enum ScrollMode
    {
        None,           //클리어한 가장 마지막 스테이지로 자동스크롤
        FlowerFind,     //흰꽃을 얻지 못한 스테이지중 가장 낮은 스테이지를 찾아 자동스크롤
        BlueFlowerFind, //하늘꽃을 얻지 못한 스테이지중 가장 낮은 스테이지를 찾아 자동스크롤
        RedFlowerFind,
        CustomFind      //customModeIndex(스테이지 번호)로 자동스크롤
    }

    private static ScrollMode scrollMode = ScrollMode.None;
    public static ScrollMode StageScrollMode
    {
        set
        {
            scrollMode = value;
        }
    }

    private static int customModeIndex = 0;
    public static int CustomModeIndex
    {
        set
        {
            customModeIndex = value;
        }
    }

    public void ScrollItemMove(int index, bool isItemIndex = false)
    {
        const int heightOffset = 2;

        float maxHeight = popupStage.GetBtnStageCount() * itemSize - mScroll.panel.GetViewSize().y + 2.0f;
        int itemIndex = isItemIndex ? index : index + GetChapterIndex(index); // UIItemStageButton의 위치 = 스테이지 + 에피소드 표시 아이템 갯수
        int arrayIndex = popupStage.GetBtnStageCount() - Mathf.CeilToInt((float)itemIndex / UIPopupStage.stageArraySize) - heightOffset;
        float value = (float)arrayIndex * itemSize / maxHeight;

        StartCoroutine(ReuseScrollMove(mScroll.verticalScrollBar, Vector2.up * value));
    }

    private int GetChapterIndex(int stageIndex)
    {
        for (int i=0; i<ManagerData._instance.chapterData.Count; i++)
        {
            if (ManagerData._instance.chapterData[i]._stageIndex <= stageIndex && 
                ManagerData._instance.chapterData[i]._stageIndex + ManagerData._instance.chapterData[i]._stageCount > stageIndex)
            {
                return i + 1;
            }
        }
        return -1;
    }

    private int GetMinFlowerStageIndex(int minFlower = 3)
    {
        int i;

        for (i = 0; i < ManagerData._instance._stageData.Count; i++)
        {
            if (ManagerData._instance._stageData[i]._flowerLevel < minFlower)
            {
                return i + 1;
            }
        }

        return i;
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }
        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemStageArray cellStageArray = go.gameObject.GetComponent<UIItemStageArray>();
        cellStageArray.UpdateData(popupStage.GetStageData(realIndex * -1));
    }
}
