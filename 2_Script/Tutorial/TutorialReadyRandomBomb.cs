using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialReadyRandomBomb : TutorialBase
{
    public GameObject GetObjReadyItem_RandomBomb()
    {
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            if (UIPopupReady._instance.listReadyItem[i].type == READY_ITEM_TYPE.RANDOM_BOMB)
                return UIPopupReady._instance.listReadyItem[i].gameObject;
        }
        return null;
    }
    
    public List<GameObject> GetListObjReadyItem_RandomBomb()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            if (UIPopupReady._instance.listReadyItem[i].type == READY_ITEM_TYPE.RANDOM_BOMB)
                listObj.Add(UIPopupReady._instance.listReadyItem[i].gameObject);
        }
        return listObj;
    }
    
    public bool IsSelectReadyItem()
    {
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            ReadyItem readyItem = UIPopupReady._instance.listReadyItem[i];
            if (readyItem.type == READY_ITEM_TYPE.RANDOM_BOMB)
                return readyItem.IsSelectItem();
        }
        return false;
    }
    
    public GameObject GetObjStartButton()
    {
        return UIPopupReady._instance.GetButton();
    }
    
    public bool IsClickStartButton()
    {
        return UIPopupReady._instance.bStartGame;
    }
    
    public List<GameObject> GetListObjStartButton()
    {
        List<GameObject> listObj = new List<GameObject>() { UIPopupReady._instance.GetButton() };
        return listObj;
    }
}
