using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTilemap : ActionBase
{
    [SerializeField] private UnityEngine.Tilemaps.Tilemap tile;
    [SerializeField] private TextAsset hole;
    [SerializeField] private ManagerTilemap.DirectionOption fadeOutOption;
    [SerializeField] private float _duration = 1.0f;

    public override void DoAction()
    {
        ManagerTilemap.Instance.MakeTilemap(tile.name, tile);

        if (_stateType == TypeTriggerState.WakeUp)
        {
            ManagerTilemap.Instance.MakeHole(tile.name, hole.text, _duration, fadeOutOption);
        }
        else
        {
            ManagerTilemap.Instance.MakeHole(tile.name, hole.text);
        }
    }
}
