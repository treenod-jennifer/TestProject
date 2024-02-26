using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_DiaryGuest : UIReuseGridBase
{
    public IEnumerator InitReuseGrid()
    {
        minIndex = (UIDiaryGuest._instance.GetGuestCount()) * -1;
        yield return base.SetScroll();
        onInitializeItem += OnInitializeItem;
        UpdateItem();

        var scrollView = this.GetComponentInParent<UIScrollView>();
        if (scrollView != null)
        {
            scrollView.ResetPosition();
        }
    }

    public void UpdateItem()
    {
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
        UIItemInstallableGuestList itemGuestList = go.gameObject.GetComponent<UIItemInstallableGuestList>();
        itemGuestList.UpdateGuestList(UIDiaryGuest._instance.GetListGuestIndex(realIndex * -1), UIDiaryGuest._instance.IsCanChangeCharacter());
    }
}
