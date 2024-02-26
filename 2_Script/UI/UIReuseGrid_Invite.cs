using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Invite : UIReuseGridBase
{
    public IEnumerator InitReuseGrid()
    {
        minIndex = (UIPopupInvite._instance.GetInviteFriendsCount()) * -1;
        yield return SetScroll();
    }

    protected override IEnumerator SetScroll()
    {
        yield return base.SetScroll();
        onInitializeItem += OnInitializeItem;
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
        UIItemInvite inviteCell = go.gameObject.GetComponent<UIItemInvite>();
        inviteCell.UpdateData(UIPopupInvite._instance.GetInviteData(realIndex * -1));
    }
}
