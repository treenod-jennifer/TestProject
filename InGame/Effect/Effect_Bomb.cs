using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Bomb : MonoBehaviour 
{
    public UISprite _sprite;
    [System.NonSerialized]
    public float _addScale = 0.1f;
    // Use this for initialization
    IEnumerator Start()
    {
        Transform _transform = transform;
        float timer = 0f;
        while (true)
        {
            timer += Global.deltaTimePuzzle * 5f;
            if (timer > 1f)
                break;

            if (timer > 0.8f)
                _sprite.color = new Color(1f, 1f, 1f, (1f - timer) * 5f);

            _transform.localScale = Vector3.one * (_addScale + timer * 1.6f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
