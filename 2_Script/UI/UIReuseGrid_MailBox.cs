using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_MailBox : UIReuseGridBase
{
    protected override void Awake()
    {
        if (UIPopupMailBox._instance == null)
            Debug.LogError("mailBoxPopUp Script is null");
        UIPopupMailBox._instance.callbackDataComplete += InitReuseGrid;
        base.Awake();
    }

    public void InitReuseGrid()
    {
        minIndex = (ManagerData._instance._messageData.Count - 1) * -1;
      
        UIPopupMailBox._instance.StartCoroutine(SetScroll());
        
        return;
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
        UIItemMessage itemMessage = go.gameObject.GetComponent<UIItemMessage>();
        itemMessage.UpdateData(UIPopupMailBox._instance.GetMessageData(realIndex * -1));
    }
}
