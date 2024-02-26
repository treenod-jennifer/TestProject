using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfilePokoYuraList : MonoBehaviour
{
    public List<UIItemUserProfilePokoYura> pokoYuraList = new List<UIItemUserProfilePokoYura>();

    public void SettingPokoYuraList(List<int> pokoYuraList, int selectIndex)
    {
        //데코 아이템 이미지 세팅.
        int nCount = pokoYuraList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (this.pokoYuraList[i] == null)
                break;
            this.pokoYuraList[i].SetPokoYuraItemImgae(pokoYuraList[i], selectIndex);
        }
    }
}
