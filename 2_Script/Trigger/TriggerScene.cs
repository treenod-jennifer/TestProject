using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerScene : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Comment", order = 1000)]
    [MultiLineString(100.0f, order = 1000)]
    public string comment = "";
#endif
    [System.NonSerialized]
    public int _sceneIndex = 0;
    [System.NonSerialized]
    public int _missionIndex = 0;

    public bool isSkippableScene = true;

    public TriggerStateWait _triggerWait = null;
    public TriggerStateWakeUp _triggerWakeUp = null;
    public TriggerStateActive _triggerActive = null;
    public TriggerStateFinish _triggerFinish = null;

}
