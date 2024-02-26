using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Rainbow_Light : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;
    public UISprite _sprite;
    [System.NonSerialized]
    public bool _rainbow = false;
    [System.NonSerialized]
    public float maxTime;

    float speed = 10f;

    void Awake()
    {
        _transform = transform;
        maxTime = 3.0f;
        _sprite.depth = (int)GimmickDepth.FX_EFFECTBASE;
    }

    IEnumerator Start()
    {
        float timer = 0f;
        if (_rainbow)
        {
            while (true)
            {
                timer += Global.deltaTimePuzzle * speed;

                _sprite.spriteName = "rainbowPang" + (1 + (int)timer % 2);
                _sprite.MakePixelPerfect();

                if (timer > maxTime)
                    break;

                yield return null;
            }
            timer = 3f;
            while (true)
            {
                timer += Global.deltaTimePuzzle * speed;

                int frame = Mathf.FloorToInt(timer);
                if (frame > 5)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _sprite.spriteName = "rainbowPang" + frame;
                    _sprite.MakePixelPerfect();
                }
                yield return null;
            }
        }
        else
        {
            while (true)
            {
                timer += Global.deltaTimePuzzle * 10f;
                if (timer > 5f)
                {
                    int frame = Mathf.FloorToInt(timer) - 4;
                    if (frame >= 5)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        _sprite.color = Color.white;
                        _sprite.spriteName = "blockRainbowPang" + frame;
                        _sprite.MakePixelPerfect();
                    }
                }
                else
                {
                    if (Random.value > 0.2f)
                        _sprite.color = new Color(1f, 1f, 1f, Mathf.Clamp01(timer * 5f));
                    else
                        _sprite.color = new Color(1f, 1f, 1f, Mathf.Clamp01(timer * 5f) * 0.4f);

                }
                yield return null;
            }
        }
    }
}
