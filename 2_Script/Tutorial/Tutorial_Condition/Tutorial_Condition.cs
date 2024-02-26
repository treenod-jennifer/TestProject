using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tutorial_Condition : MonoBehaviour
{
    public virtual IEnumerator StartCondition(System.Action endAction = null)
    {
        yield return null;
        endAction.Invoke();
    }
}