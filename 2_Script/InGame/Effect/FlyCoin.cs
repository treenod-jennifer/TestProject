using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCoin : MonoBehaviour
{
    [System.NonSerialized]
    public Transform _transform;

    public UISprite _UISprite;

    [System.NonSerialized]
    public Vector3 startPos;

    [System.NonSerialized]
    public Vector3 endPos;

    //실제로 코인을 증가시킬건지.
    public bool isAddCoin = true;

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

        if (GameManager.gameMode == GameMode.ADVENTURE)
            _timer += Global.deltaTimePuzzle * 1.7f;
        else
            _timer += Global.deltaTimePuzzle * 4f;
        
        if (_timer > 1f)
        {
            if (isAddCoin == true)
            {
                ManagerBlock.instance.AddCoin(count);
            }

            _timer = 0f;
            InGameEffectMaker.instance.RemoveEffect(gameObject);
        }
        _transform.position = Vector3.Lerp(startPos, endPos, 1f - Mathf.Cos(_timer * ManagerBlock.PI90));
    }
}
