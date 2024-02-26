using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemBonusRewardBox : UIItemRewardBox
{
    protected override string CloseBoxName
    {
        get
        {
            return "icon_bonusBox_01";
        }
    }
    protected override string OpenBoxName
    {
        get
        {
            return "icon_bonusBox_02";
        }
    }
    protected override Transform BoxRoot
    {
        get
        {
            return box.transform.parent;
        }
    }
}
