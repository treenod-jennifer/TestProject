using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionObjectActive : ActionBase
{
    public List<ObjectBase> _listObject = new List<ObjectBase>();
    public bool _active = true;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionObjectActive _ DoAction");

        for (int i = 0; i < _listObject.Count; i++)
            _listObject[i].gameObject.SetActive(_active);
    }

}
