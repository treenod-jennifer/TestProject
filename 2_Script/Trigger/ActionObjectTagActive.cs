using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionObjectTagActive : ActionBase
{
    [SerializeField]
    ObjectVisibleTagType tagType = ObjectVisibleTagType.NONE;

    [SerializeField]
    bool _active = true;

    public override void DoAction()
    {
        ManagerArea._instance.objVisibleTagMgr.SetActiveToObjects(tagType, _active);
    }
}
