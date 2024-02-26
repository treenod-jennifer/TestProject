using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TypeTriggerState
{
    None,
    Wait,
    WakeUp,
    Active,
    Finish,

    BehaviorScene,
    BehaviorArea,
    BehaviorGlobal,
}

public class TriggerState : MonoBehaviour {

    [System.NonSerialized]
    public TypeTriggerState _type = TypeTriggerState.None;
    public List<TriggerCondition> _conditions = new List<TriggerCondition>();
    public List<TriggerCondition> _conditionsChating = new List<TriggerCondition>();
    public void Reset()
    {
        for (int i = 0; i < _conditions.Count; i++)
        {
            _conditions[i].Reset();
            _conditions[i].gameObject.SetActive(false);
            _conditions[i].enabled = true;
        }
    }
    public void StartCondition()
    {
        if (_conditions.Count>0)
            _conditions[0].gameObject.SetActive(true);
    }

    public virtual bool CanExecuteTapAction()
    {
        return true;
    }

    public virtual void OnTap(ObjectBase in_obj)
    {
        StartCondition();
    }
}
