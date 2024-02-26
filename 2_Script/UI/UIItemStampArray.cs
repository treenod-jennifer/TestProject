using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemStampArray : MonoBehaviour
{
    public UIItemStamp[] arrayItemStamp;
    ServerUserStamp stampData;

    public void UpdateData(ServerUserStamp[] arrayStampData)
    {
        //현재 아이템의 정보 받아와서 세팅.
        for (int i = 0; i < arrayItemStamp.Length; i++)
        {
            stampData = arrayStampData[i];
            if (gameObject.activeInHierarchy == false)
                return;

            arrayItemStamp[i].UpdateData(stampData);
        }
    }
}
