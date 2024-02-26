using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScore : MonoBehaviour {

    public UILabel _textScore;
    public float delay = 0;

    Transform _transform;
    float timer = 0f;
    Vector3 startPos;

    public void initScore(Vector3 startPos, string score, float tempDelay = 0)
    {
        _transform.position = startPos;
        _textScore.text = score;
        delay = tempDelay;
        _transform.localPosition += Vector3.up * 37;
       
        if (delay > 0f) _textScore.alpha = 0;
    }

    void Awake()
    {
        _transform = transform;
    }

    float waitTimer = 0f;
    float ComboTimer = 0f;

    void Update()
    {
        if(delay > 0 && delay > waitTimer)
        {
            waitTimer += Global.deltaTimePuzzle;
            return;
        }
        _textScore.alpha = 1;

        if(GameManager.gameMode == GameMode.ADVENTURE)
            _transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 40f;
        else
        _transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 100f;
        
        if (timer < 0.3f)
            _transform.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.PI * (timer + 0.1f) / 0.4f) * 0.6f);
        else
            _transform.localScale = Vector3.one;

        if (timer >= 0.7f)
            _textScore.color = new Color(1f, 1f, 1f, 1f - (timer - 0.7f) / 0.3f);

        timer += Global.deltaTimePuzzle * 1.2f;
        
        if (timer >= 1f)
            Destroy(gameObject);
    }
}
