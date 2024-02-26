using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Condition_PopupState : Tutorial_Condition
{
    public string popupName = "";
    public bool isCheckPopupOpen = true;

    public void Init(string name, bool isCheckOpen)
    {
        this.popupName = name;
        this.isCheckPopupOpen = isCheckOpen;
    }

    public override IEnumerator StartCondition(System.Action endAction)
    {
        if (isCheckPopupOpen == true)
            yield return new WaitUntil(() => ManagerUI._instance._popupList.FindIndex(x => x.name == popupName) != -1);
        else
            yield return new WaitUntil(() => ManagerUI._instance._popupList.FindIndex(x => x.name == popupName) == -1);
        endAction.Invoke();
    }
}
