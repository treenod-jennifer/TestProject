using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionUIActive : ActionBase
{
    public bool _active = true;

    public override void DoAction()
    {
        // 원래는 상위 3단 위에 (parent.parent.parent) 에 메시지를 전하는 건데
        // OnActionUIActive 함수를 갖고있는 클래스가 모두 AreaBase 계열이라 걍 AreaBase 찾는함수 써서 작동시켜도 되겠다 싶었음

        AreaBase ab = AreaBase.ScanNearestAreaBase(this.transform);
        if (ab != null)
            ab.SendMessage("OnActionUIActive", _active);
    }

}
