using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemEndContents_PriseItemArray : MonoBehaviour
{
    [SerializeField] private UIItemEndContents_PriseItem[] _arrayPriseItem;

    public void UpdateData(CdnEndContentsShop[] arrayPriseItemData)
    {
        for(int i = 0; i < _arrayPriseItem.Length; i++)
        {
            if (gameObject.activeInHierarchy == false)
                return;

            _arrayPriseItem[i].UpdateData(arrayPriseItemData[i]);
        }
    }

}
