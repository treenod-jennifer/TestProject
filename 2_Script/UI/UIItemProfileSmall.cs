using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemProfileSmall : MonoBehaviour
{
    [SerializeField] private UILabel userName;

    [SerializeField] private UIItemProfile profileItem;

    private int index;

    private UserBase item = new UserBase();

    private System.Action<int> OnClickBtn;

    public virtual void SettingItem(UserBase cellData, int index, System.Action<int> callback)
    {
        item = cellData;
        this.index = index;

        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        //프로필 아이템 추가
        profileItem.SetProfile(item);

        userName.text = Global.ClipString(item.DefaultName, 10);

        OnClickBtn = callback;
    }

    void OnClickBtnItem()
    {
        if (OnClickBtn == null)
            return;
        OnClickBtn(index);
    }
}
