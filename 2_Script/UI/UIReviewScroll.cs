using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReviewScroll : MonoBehaviour
{
    public UIPanel scrollPanel;
    public UIReviewBaordItemContainer itemContainer;

    public void SettingDepth(int depth)
    {
        scrollPanel.depth = depth;
    }

    public void SettingSortOrder(int layer)
    {
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer;
    }
}
