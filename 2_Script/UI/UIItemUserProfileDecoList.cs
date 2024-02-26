using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfileDecoList : MonoBehaviour
{
    public List<UIItemUserProfileDeco> decoItemList = new List<UIItemUserProfileDeco>();

    public void SettingItemDecoList(List<ServerUserHousingItem> decoList)
    {
        //데코 아이템 이미지 세팅.
        int nCount = decoList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (decoItemList[i] == null)
                break;
            string fileName = string.Format("{0}_{1}", decoList[i].index, decoList[i].modelIndex);
            decoItemList[i].SetDecoItemImage(fileName);
        }
    }
}