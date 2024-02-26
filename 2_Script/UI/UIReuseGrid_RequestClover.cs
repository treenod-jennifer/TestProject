using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_RequestClover : UIReuseGridBase
{
    protected override void Awake()
    {
        UIPopupRequestClover._instance.callbackDataComplete += InitReuseGrid;
        base.Awake();
        UIPopupRequestClover._instance.grid = this;

        onInitializeItem += OnInitializeItem;
    }

    public void InitReuseGrid()
    {
        minIndex = (UIPopupRequestClover._instance.GetUserFriendsCount()) * -1 / 2;
        return;
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);

        int itemIndex = Mathf.Abs(realIndex) * 2;
        UIItemRequestClover[] requestCells = go.transform.GetComponentsInChildren<UIItemRequestClover>(true);

        for (int i = 0; i < requestCells.Length; i++)
        {
            if (itemIndex + i <= UIPopupRequestClover._instance.GetUserFriendsCount())
            {
                requestCells[i].UpdateData(UIPopupRequestClover._instance.GetFriendData(itemIndex + i));
                requestCells[i].gameObject.SetActive(true);
            }
            else
                requestCells[i].gameObject.SetActive(false);
        }
    }

    public void OnCloverSendComplete()
    {
        for (int i = 0; i < mChildren.Count; i++)
        {
            var items = mChildren[i].GetComponentsInChildren<UIItemRequestClover>();

            foreach (var item in items)
                item.SettingRequestButton();
        }
    }
}
