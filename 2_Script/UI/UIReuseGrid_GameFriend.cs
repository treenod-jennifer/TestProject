using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_GameFriend : UIReuseGridBase
{
    [SerializeField] private int ItemCount = 2;

    private List<Profile_PION> users = null;

    protected override void Awake()
    {
        base.Awake();
        onInitializeItem += OnInitializeItem;
    }

    public void InitReuseGrid(List<Profile_PION> users, bool scrollPosReset = true)
    {
        if (users == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        this.users = users;

        int count = Mathf.CeilToInt((float)this.users.Count / ItemCount);
        minIndex = count * -1;

        if(scrollPosReset && mScroll != null)
        {
            Vector3 pos = mScroll.transform.localPosition;
            pos.y = -40;
            mScroll.transform.localPosition = pos;

            mPanel.clipOffset = Vector2.zero;
        }

        SetContent();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (users == null) return;

        if (realIndex < minIndex) return;

        var friends = go.GetComponentsInChildren<UIItemGameFriend>();
        int index = realIndex * -1 * ItemCount;
        int ActiveCount = 0;

        foreach (var friend in friends)
        {
            Profile_PION user = index < users.Count && index >= 0 ? users[index] : null;

            friend.UpdateData(user);

            index++;

            if (user != null) ActiveCount++;
        }

        go.SetActive(ActiveCount > 0);
    }
}
