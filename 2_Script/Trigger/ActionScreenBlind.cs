using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ActionScreenBlind : ActionBase
{
    public TextAsset    _BlindData;
    public int          _area       = 1;
    public float        _duration   = 1f;
    
    public override void DoAction()
    {
//        Debug.Log("ActionScreenBlind _ DoAction");

        if (_stateType == TypeTriggerState.WakeUp)
            ManagerLevelBlindStatic.instance.LoadTileChunkDataInfo(_BlindData, _area, _duration, true);
        else
            ManagerLevelBlindStatic.instance.SetBank(_area, _BlindData);
    }
}
