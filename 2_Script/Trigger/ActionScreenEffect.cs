using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionScreenEffect : ActionBase
{
    public Color _startColor = Color.white;
    public Color _endColor = Color.clear;
    public float _duration = 1f;
    public bool _autoDestroy = true;
    static CameraEffect _staticCamera = null;

    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionScreenEffect _ DoAction");

        

        if (_staticCamera != null && _autoDestroy)
        {
            _staticCamera.ApplyScreenEffect(_startColor, _endColor, _duration, _autoDestroy);
            _staticCamera = null;
        }
        else
        {
            _staticCamera = CameraEffect.MakeScreenEffect();
            _staticCamera.ApplyScreenEffect(_startColor, _endColor, _duration, _autoDestroy);
            if(_autoDestroy)
                _staticCamera = null;
        }
    }
}
