using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyTreasureBlock : MonoBehaviour
{
    public UISprite mainSprite;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalTime = 0.0f;
    private float endTime = 0.0f;

    void Start()
    {
        startPos = transform.position;
        targetPos = GameUIManager.instance.treasureSprite.gameObject.transform.position;
        totalTime = 0.0f;
        endTime = Mathf.Max(moveX_Controller.keys[moveX_Controller.length-1].time, moveY_Controller.keys[moveY_Controller.length-1].time);
    }

    [SerializeField] private AnimationCurve moveX_Controller;
    [SerializeField] private AnimationCurve moveY_Controller;

    void Update()
    {
        totalTime += Global.deltaTimePuzzle;

        float x = moveX_Controller.Evaluate(totalTime);
        float y = moveY_Controller.Evaluate(totalTime);

        transform.position = new Vector3
        (
            Mathf.LerpUnclamped(startPos.x, targetPos.x, x),
            Mathf.LerpUnclamped(startPos.y, targetPos.y, y),
            transform.position.z
        );

        if(totalTime >= endTime)
        {
            GameUIManager.instance.SetTreasure(AdventureManager.instance.TreasureCnt);
            InGameEffectMaker.instance.RemoveEffect(gameObject);
            ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        }
        
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, Global.deltaTimePuzzle * 2.5f);

        //if (Vector3.Distance(targetPos, transform.position) < 0.05f)
        //{
        //    GameUIManager.instance.SetTreasure(AdventureManager.instance.TreasureCnt);
        //    InGameEffectMaker.instance.RemoveEffect(gameObject);
        //}
    }

    public void SetSprite(string spriteName)
    {
        mainSprite.spriteName = spriteName;
        mainSprite.MakePixelPerfect();
    }
}
