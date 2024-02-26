using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionNaviEdit : ActionBase
{
    public bool _active = true;
    public List<Collider> _collider = new List<Collider>();

    public override void DoAction()
    {
        //base.DoAction();

        for (int i = 0; i < _collider.Count; i++)
        {
            GraphUpdateObject guo = new GraphUpdateObject(_collider[i].bounds);
            guo.modifyTag = true;
            guo.setTag = !_active ? 2 : 1;
            guo.updatePhysics = false;
            AstarPath.active.UpdateGraphs(guo);
        }
        //Debug.Log("ActionNaviEdit _ DoAction");

    }

}
