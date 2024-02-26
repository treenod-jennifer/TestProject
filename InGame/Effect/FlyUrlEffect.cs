using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FlyUrlEffect : MonoBehaviour
{
    public UIUrlTexture _UISprite;

    [System.NonSerialized]
    public Vector3 startPos;

    [System.NonSerialized]
    public Vector3 endPos;

    [System.NonSerialized]
    public float _delay = 0f;

   // float _timer = 0f;

    private Action onCallbackEvent;

    public void initEffect(Vector3 tempStartPos, Vector3 tempEndPos, string spriteName,  Action onCallbackEvent = null)
    {
        this.onCallbackEvent = onCallbackEvent;

        startPos = tempStartPos;
        endPos = tempEndPos;

        transform.position = tempStartPos;

        _UISprite.LoadCDN(Global.gameImageDirectory, "IconEvent/", spriteName);
    }

    void Update ()
    {
        _delay -= Global.deltaTimePuzzle;
        if (_delay > 0f)
            return;

        //_timer += Global.deltaTimePuzzle; * 1.7f;
        if (transform.position == endPos)// _timer > 1f)
        {
            //_timer = 0f;
            InGameEffectMaker.instance.RemoveEffect(gameObject);
            if (onCallbackEvent != null)
            {
                this.onCallbackEvent();
            }
        }
        //transform.position = Vector3.Lerp(startPos, endPos, 1f - Mathf.Cos(_timer * ManagerBlock.PI90));

        transform.position = Vector3.MoveTowards(transform.position, endPos, Global.deltaTimePuzzle * 2f);// * BLOCK_MOVE_SPEED_RATIO);
    }
}
