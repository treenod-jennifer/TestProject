using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupPlusInfo : UIPopupBase
{
    public UIPanel scrollPanel;
    public GameObject bannerRoot;
    public GameObject _objItemPlusInfo;

    private List<GameObject> itemList = new List<GameObject>();
    private float itemSpace = 290f;

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(List<ServerImageLink> links)
    {
        float yPos = 0f;
        for (int i = 0; i < links.Count; i++)
        {
            yPos = (itemSpace * i * -1);
            UIItemPlusInfo item = NGUITools.AddChild(bannerRoot, _objItemPlusInfo).GetComponent<UIItemPlusInfo>();
            item.SettingPluseInfoItem(links[i].image, links[i].link);
            item.transform.localPosition = new Vector3(0f, yPos, 0f);
        }
    }
}
