using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Tutorial_Action : MonoBehaviour
{
    public enum ActionType
    {
        DATA_REMOVE = 1,
        DATA_SET = 2,
        ACTION = 3
    }

    public virtual ActionType GetActionType()
    {
        return ActionType.ACTION;
    }

    public virtual void StartAction(System.Action endAction = null)
    {
        endAction.Invoke();
    }
}