using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemPokoYuraSelectorList : MonoBehaviour
{
    public List<UIItemPokoYuraSelector> pokoYuraList = new List<UIItemPokoYuraSelector>();

    public void SettingPokoYuraList(List<int> pokoYuraList, UIScrollView scrollView)
    {
        //데코 아이템 이미지 세팅.
        int nCount = pokoYuraList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (this.pokoYuraList[i] == null)
                break;
            this.pokoYuraList[i].SetPokoYuraItemImgae(pokoYuraList[i]);
        }

        Transform tr = gameObject.transform;
        for (int i = 0; i < tr.childCount; ++i)
        {
            var dragScroll = tr.GetChild(i).gameObject.AddComponent<UIDragScrollView>();
            dragScroll.scrollView = scrollView;
        }
        
    }
}
