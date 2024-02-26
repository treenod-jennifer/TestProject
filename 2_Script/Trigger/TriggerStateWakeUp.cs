using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TriggerStateWakeUp : TriggerState
{
    [Header("CameraPosition")] 
    [SerializeField] public Transform posCamera;
    [SerializeField] public Transform posPokotaSpawn;
    [SerializeField] public Transform posNpcSpawn;
    [SerializeField] public Transform posEffect;
    

    void Awake()
    {
        _type = TypeTriggerState.WakeUp;
        for (int i = 0; i < _conditions.Count; i++)
            _conditions[i]._stateType = _type;
    }
}
