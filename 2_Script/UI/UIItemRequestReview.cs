using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemRequestReview : MonoBehaviour
{
    [SerializeField] private GameObject objCheck;

    private int index = 0;
    public int Index { get { return index; } }

    private System.Action<int> selectAction = null;
    
    public void InitItem(int itemIdx, System.Action<int> selectEvent)
    {
        index = itemIdx;
        selectAction = selectEvent;
        objCheck.SetActive(false);
    }

    public void UpdateItem(bool isSelect)
    {
        objCheck.SetActive(isSelect == true);
    }

    public void OnClickBtnItem()
    {
        selectAction.Invoke(index);
    }
}
