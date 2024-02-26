using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemRequestSmall : MonoBehaviour
{
    public int index = 0;

    public UILabel userName;

    [SerializeField] private UIItemProfile profileItem;

    UserBase item = new UserBase();

    public void SettingItem(UserBase cellData)
    {
        item = cellData;

        //프로필 아이템 추가
        profileItem.SetProfile(item);

        userName.text = Global.ClipString(item.DefaultName, 10);
    }

    void OnClickBtnItem()
    {
        if (UIPopupRequestCloverSmall._instance == null)
            return;
        UIPopupRequestCloverSmall._instance.OnClickBtnItem(index);
    }
}
