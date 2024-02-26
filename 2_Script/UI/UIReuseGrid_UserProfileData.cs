using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_UserProfileData : UIReuseGridBase
{
    protected override void Awake()
    {
        onInitializeItem += OnInitializeItem;
    }

    private void InitReuseGrid()
    {
        minIndex = (UIPopUPUserProfileSelection._instance.GetUserProfileDataCount() - 1) * -1;

        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        base.OnInitializeItem(go, wrapIndex, realIndex);

        if(go.activeSelf)
        {
            UIItemUserProfile_AnimalDataArray profileCell = go.gameObject.GetComponent<UIItemUserProfile_AnimalDataArray>();
            profileCell.UpdataData(UIPopUPUserProfileSelection._instance.GetUserProfileData(realIndex * -1));
        }
    }

    public void ScrollReset()
    {
        InitReuseGrid();
    }
}
