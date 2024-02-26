using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkLoading : MonoBehaviour
{
    public UITexture _sprite;
    public UISprite _spriteBlind;
    public UILabel _textLoading;
    public UIPanel _mainPanel;

    [System.NonSerialized]
    public float _delay = 0f;

    
    //float defaultAlpha = 155f / 256f;
    float alpha = 0f;

    void Awake()
    {
        _tempInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    static public NetworkLoading _tempInstance = null;
    static public NetworkLoading MakeNetworkLoading(float in_delay = 1f)
    {
        if (_tempInstance != null)
            return null;

        NetworkLoading obj = Instantiate(Global._instance._objNetworkLoading).gameObject.GetComponent<NetworkLoading>();
        obj._delay = in_delay;
        return obj;
    }
    static public void EndNetworkLoading()
    {
        if (_tempInstance != null)
            _tempInstance.StartCoroutine(_tempInstance.CoEnd());
        _tempInstance = null;
    }
	// Use this for initialization
    IEnumerator Start()
    {
        _mainPanel.alpha = 0.002f;
        yield return new WaitForSeconds(_delay);
        StartCoroutine(CoAnimation());

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimeNoScale * 8f;
            alpha = timer;

            _mainPanel.alpha = alpha;
            yield return null;
        }
        _mainPanel.alpha = 1f;
        yield return null;
    }
    IEnumerator CoEnd()
    {
        while (alpha >= 0f)
        {
            alpha -= Global.deltaTimeNoScale * 8f;
            _mainPanel.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }
        alpha = 0f;
        _mainPanel.alpha = alpha;
        Destroy(gameObject);
        yield return null;
    }
    IEnumerator CoAnimation()
    {
        float frame = 0f;
        float textFrame = 0f;

        while (true)
        {
            frame += Global.deltaTimeNoScale * 6f;
            textFrame += Global.deltaTimeNoScale * 13f;


            if (frame<1f)
                _sprite.uvRect = new Rect(0.5f, 0f, 0.5f, 0.5f);
            else if (frame < 2f)
                _sprite.uvRect = new Rect(0f, 0f, 0.5f, 0.5f);
            else if (frame < 3f)
                _sprite.uvRect = new Rect(0f, 0.5f, 0.5f, 0.5f);
            else
                _sprite.uvRect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            if (frame >= 4f)
                frame = 0f;

            _textLoading.spacingX = (int)Mathf.PingPong(textFrame, 3);

            yield return null;
        }
    }
}
