using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdvtureRewardBox : UIItemRewardBox
{
    protected override string CloseBoxName
    {
        get
        {
            return $"icon_treasureBox_{((TreasureType)AdventureManager.instance.TreasureType).ToString()}_01";
        }
    }
    protected override string OpenBoxName
    {
        get
        {
            return $"icon_treasureBox_{((TreasureType)AdventureManager.instance.TreasureType).ToString()}_02";
        }
    }
    protected override Transform BoxRoot
    {
        get
        {
            return box.transform;
        }
    }
}
