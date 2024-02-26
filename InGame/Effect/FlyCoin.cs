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
    
    public bool AdventureBossCoin = false;

    void Awake()
    {
        _transform = transform;

        //랜덤넣기 위로 갈지 밑으로 갈지 얼마나 갈지 
    }

    //탐험모드보스용
    public float speedRatio = -1f;
    float velocity = 0.12f;
    public int dirBoss = 1;
    public float distBoss = 1;

    void Update()
    {
        if (AdventureBossCoin)
        {
            speedRatio += velocity;
            transform.position = Vector3.MoveTowards(transform.position, endPos, speedRatio * Global.deltaTimePuzzle);

            if (speedRatio < 0f)
                transform.position += dirBoss * distBoss * Vector3.up * 0.5f * speedRatio * Global.deltaTimePuzzle;

            if (Vector3.Distance(endPos, transform.position) < 0.05f)
            {
                ManagerSound.AudioPlay(AudioInGame.Get_Jewelry);

                if (isAddCoin == true)
                    ManagerBlock.instance.AddCoin(1);

                InGameEffectMaker.instance.RemoveEffect(gameObject);
            }
        }
        else
        {
            _delay -= Global.deltaTimePuzzle;
            if (_delay > 0f)
                return;

            if (Global.GameType != GameType.ADVENTURE && Global.GameType != GameType.ADVENTURE_EVENT)
                _timer += Global.deltaTimePuzzle * 1.7f;
            else
                _timer += Global.deltaTimePuzzle * 4f;

            if (_timer > 1f)
            {
                if (isAddCoin == true)
                {
                    //턴추가
                    if (GameManager.gameMode == GameMode.ADVENTURE)
                    {
                        if (ManagerBlock.instance.coins < 100)
                            ManagerBlock.instance.AddCoin(1);
                    }
                    else
                    {
                        ManagerBlock.instance.AddCoin(count);
                    }
                }

                _timer = 0f;
                InGameEffectMaker.instance.RemoveEffect(gameObject);
            }
            _transform.position = Vector3.Lerp(startPos, endPos, 1f - Mathf.Cos(_timer * ManagerBlock.PI90));
        }
    }
}
