using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum ObjectDoingType
{
    Shake,
    Noize,
}

public class ActionObjectDoing : ActionBase
{
    public ObjectDoingType _type = ObjectDoingType.Shake;
    public List<ObjectBase> _target = new List<ObjectBase>();
    public bool _active = true;
    public int[] _enableMission = null;
    public float _scale = 1f;
    public float _speed = 1f;


    public override void DoAction()
    {
        if (_enableMission != null)
        {
            if (_enableMission.Length > 0)
            {
                bool playAction = false;
                for (int i = 0; i < _enableMission.Length; i++)
                {
                    TypeMissionState state = ManagerData._instance._missionData[_enableMission[i]].state;

                    if (ManagerData._instance._missionData[_enableMission[i]].state == TypeMissionState.Active)
                    {
                        playAction = true;
                        break;
                    }
                }

                if (!playAction)
                    return;
            }
        }


        for (int i = 0; i < _target.Count; i++)
        {
            if (_type == ObjectDoingType.Shake)
                _target[i].DoActionShake(_active, _speed, _scale);
        }
    }

}
