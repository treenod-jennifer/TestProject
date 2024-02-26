using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Rainbow : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;
    public UISprite _tail;

    [System.NonSerialized]
    public Vector3 _startPos;
    [System.NonSerialized]
    public Vector3 _endPos;

    void Awake()
    {
        _transform = transform;
        _tail.depth = (int)GimmickDepth.FX_EFFECT;
    }
    IEnumerator Start()
    {
        Vector3 dir = _endPos - _startPos;
        float len = dir.magnitude;
        dir = dir / len;


        _tail.cachedTransform.localRotation = Quaternion.LookRotation(dir, Vector3.forward) * Quaternion.Euler(90f, 0f, 0f);//Quaternion.Euler(0f, 0f, Vector3.Angle(Vector3.up, dir));
        _transform.localPosition = _endPos;
        _tail.height = (int)len;
        float timer = 0f;

        while (true)
        {
            timer += Global.deltaTimePuzzle;
            if (timer > 0.2f && _tail != null)
            {
                if (len <= 40f)
                {
                    Destroy(_tail.gameObject);
                    _tail = null;
                    continue;
                }
                len -= Global.deltaTimePuzzle * 3000f;
                _tail.height = (int)len;
            }

            if (timer > 0.5f)
                Destroy(gameObject);

            yield return null;
        }
    }
}
