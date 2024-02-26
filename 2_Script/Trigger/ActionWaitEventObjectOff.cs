using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionWaitEventObjectOff : ActionBase
{

    public int _targetIndex = 0;
    public string _skinName = "";
    public string _animationName = "";
    public bool _immediately = false;

    public override void DoAction()
    {
        if (ManagerArea._instance != null)
        {
            ObjectEvent targetObj = null;
            ManagerArea._instance._eventWaitObject.TryGetValue(_targetIndex,out targetObj);



            if (targetObj != null)
            {
                if (_immediately)
                    targetObj.gameObject.SetActive(false);
                else
                {
                    targetObj.PlayAnimation(_animationName, false, _skinName, 
                        () => { targetObj.gameObject.SetActive(false); });
                }
                    
            }
        }
    }
}
