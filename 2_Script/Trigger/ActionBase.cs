using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBase : MonoBehaviour
{   
    public float _delay = 0f;

    [System.NonSerialized]
    public TypeTriggerState _stateType = TypeTriggerState.None;
    [System.NonSerialized]
    public bool bAction = false;
    [System.NonSerialized]
    public bool bActionFinish = false;

    [System.NonSerialized]
    public float timer = 0f;
    [System.NonSerialized]
    public bool bWaitActionFinishOnOff = false;

    [System.NonSerialized]
    public static Dictionary<TypeCharacterType, int> characterRefCount = new Dictionary<TypeCharacterType, int>();

    public static bool TryLockCharacter(TypeCharacterType charType)
    {
        if (ActionBase.characterRefCount.ContainsKey(charType))
        {
            if (ActionBase.characterRefCount[charType] == 0)
            {
                ActionBase.characterRefCount[charType] += 1;
                return true;
            }
            else
                return false;
        }
        else
        {
            ActionBase.characterRefCount[charType] = 1;
            return true;
        }
    }

    public static bool TryUnlockCharacter(TypeCharacterType charType)
    {
        if (ActionBase.characterRefCount.ContainsKey(charType))
        {
            ActionBase.characterRefCount[charType] -= 1;

            if (ActionBase.characterRefCount[charType] == 0)
                ActionBase.characterRefCount.Remove(charType);

            return true;
        }

        return false;
    }


    public virtual bool ActionStartPrecheck() { return true; }

    public virtual void DoAction() { }

    public virtual void AbortAction() { }

    static public AreaBase ScanNearestAreaBase(Transform t)
    {
        if (t.parent == null)
            return null;

        AreaBase areaBase = t.GetComponentInParent<AreaBase>();
        if (areaBase == null)
        {
            return ScanNearestAreaBase(t.parent);
        }
        else return areaBase;
    }

}
