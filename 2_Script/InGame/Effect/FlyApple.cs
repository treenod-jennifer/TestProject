using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyApple : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;

    public UISprite _UISprite;

    [System.NonSerialized]
    public Vector3 startPos;


    [System.NonSerialized]
    public Vector3 endPos;

    public int count = 1;

    [System.NonSerialized]
    public float _delay = 0f;

    float _timer = 0f;

    void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        _delay -= Global.deltaTimePuzzle;
        if (_delay > 0f)
            return;

        _timer += Global.deltaTimePuzzle * 1.7f;
        if (_timer > 1f)
        {

                //턴추가
                GameManager.instance.moveCount++;
                GameUIManager.instance.RefreshMove();
                _timer = 0f;
                InGameEffectMaker.instance.RemoveEffect(gameObject);

        }
        _transform.position = Vector3.Lerp(startPos, endPos, 1f - Mathf.Cos(_timer * ManagerBlock.PI90));
    }
}
