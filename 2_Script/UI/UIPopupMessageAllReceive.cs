using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMessageAllReceive : UIPopupBase
{
    [SerializeField] private UIScrollView scrollView;
    [SerializeField] private UIItemMessageItem messageItem;


    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        scrollView.panel.depth = depth + 1;
    }

    public void InitData(List<MessageData> messageDatas)
    {
        for (int i = 0; i < messageDatas.Count; i++)
        {
            UIItemMessageItem messageItem = Instantiate(this.messageItem, scrollView.GetComponentInChildren<UIGrid>().transform);
            messageItem.cellData(messageDatas[i]);
        }

        scrollView.GetComponentInChildren<UIGrid>().Reposition();
    }
}