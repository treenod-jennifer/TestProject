using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;





public class ActionTransformAnimationObject : ActionBase
{
    public bool _conditionWaitOn = false;
    public ObjectBase _targetObject = null;
    public string _animationName = "";

    Animation animaitonData = null;
    public int _staticSource = 0;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //Debug.Log("ActionTransformAnimationObject _ DoAction");

        animaitonData = _targetObject.GetComponent<Animation>();

        if (animaitonData != null)
        {
            if (_staticSource > 0)
                animaitonData.clip = ManagerLobby._instance._staticAnimationObj[_staticSource - 1];
            StartCoroutine(CoDoAction());
        }
    }

    IEnumerator CoDoAction()
    {
        if (_animationName.Length > 0)
            animaitonData.Play(_animationName);
        else
            animaitonData.Play();
        Vector3 obScale = _targetObject._transformModel.localScale;

        while (true)
        {
            if (!animaitonData.isPlaying)
            {
                _targetObject._transformModel.localScale = obScale;
                bActionFinish = true;
                break;
            }
            yield return null;
        }
    }
}
