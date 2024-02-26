using UnityEngine;

public class UIReuseGrid_AntiqueStore : UIReuseGridBase
{
    protected override void Awake()
    {
        onInitializeItem += OnInitializeItem;
    }
    
    protected override void Start()
    {
        base.Start();
        InitReuseGrid();
    }
    
    public void InitReuseGrid()
    {
        minIndex = (UIPopupAntiqueStore._instance.GetAntiqueItemCount() - 1) * -1;
        SetContent();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }

        base.OnInitializeItem(go, wrapIndex, realIndex);
        UIItemAntiqueStoreItemArray arrayAntiqueStore = go.gameObject.GetComponent<UIItemAntiqueStoreItemArray>();
        arrayAntiqueStore.UpdateData(UIPopupAntiqueStore._instance.GetAntiqueItemData(realIndex * -1));
    }

    public void ScrollReset()
    {
        InitReuseGrid();
    }
}
