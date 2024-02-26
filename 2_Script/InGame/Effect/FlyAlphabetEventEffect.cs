using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FlyAlphabetEventEffect : MonoBehaviour
{
    public UISprite sprite;
    public UIUrlTexture urlTexture;

    [System.NonSerialized]
    public Vector3 endPos;

    [System.NonSerialized]
    public float _delay = 0f;

    private Action onCallbackEvent;

    public void InitEffect(Vector3 tempStartPos, Vector3 tempEndPos, string spriteName, bool isNormalBlock = true, Action onCallbackEvent = null)
    {
        this.onCallbackEvent = onCallbackEvent;

        endPos = tempEndPos;
        transform.position = tempStartPos;

        sprite.gameObject.SetActive(isNormalBlock);
        urlTexture.gameObject.SetActive(!isNormalBlock);

        if (isNormalBlock == true)
        {
            sprite.spriteName = spriteName;
            sprite.MakePixelPerfect();
        }
        else
        {
            urlTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", spriteName);
        }
    }

    void Update()
    {
        _delay -= Global.deltaTimePuzzle;
        if (_delay > 0f)
            return;
     
        if (transform.position == endPos)
        {
            InGameEffectMaker.instance.RemoveEffect(gameObject);
            if (onCallbackEvent != null)
            {
                this.onCallbackEvent();
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, endPos, Global.deltaTimePuzzle * 2f);
    }
}
