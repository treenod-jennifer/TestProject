using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionObjectAnimation : ActionBase
{
    public List<ObjectEvent> _listObject = new List<ObjectEvent>();
    public string _skinName = "";
    public string _animationName = "";
    public bool _loop = false;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionObjectAnimation _ DoAction");


        for (int i = 0; i < _listObject.Count; i++)
            _listObject[i].PlayAnimation(_animationName,_loop,_skinName);
    }

}
