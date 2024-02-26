using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_BombN : MonoBehaviour {

    public UISprite _sprite1;
    public UISprite _sprite2;
    [System.NonSerialized]
    public float _addScale = 0.1f;
    // Use this for initialization
    IEnumerator Start()
    {
        Transform _transform = transform;

        _sprite2.color = new Color(1f, 1f, 1f, 0.01f);
        _sprite1.color = new Color(1f, 1f, 1f, 1f);

        float timer = 0f;
        while (true)
        {
            timer += Global.deltaTimePuzzle * 6f;
            if (timer > 1.2f)
                break;


            if (timer > 0.8f && timer < 1f)
                _sprite2.color = new Color(1f, 1f, 1f, 0.4f);
            else
                _sprite2.color = new Color(1f, 1f, 1f, 0.01f);

            if (timer > 1f)
                _sprite1.color = new Color(1f, 1f, 1f, (1.2f - timer) * 5f);
            else
                _sprite1.color = new Color(1f, 1f, 1f, timer);

            _transform.localScale = Vector3.one * (_addScale + timer * 2f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
