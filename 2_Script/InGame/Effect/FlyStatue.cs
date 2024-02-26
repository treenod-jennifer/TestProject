using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyStatue : MonoBehaviour
{
    public static List<FlyStatue> flyStatueList = new List<FlyStatue>();

    public UISprite statueSprite;
    public UISprite shadowSprite;
    public Transform _transform;

    [System.NonSerialized]    public Vector3 startPos;
    [System.NonSerialized]    public float startRotatePos;
    [System.NonSerialized]    public Vector3 startScale = Vector3.one *0.4f;

    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveV;

    //[System.NonSerialized]
    public Vector3 endPos = new Vector3(150, 430,0);

    Vector3 midPos;
    Vector3 midRot;
    Vector3 midSca;

    public void StartFly()
    {
        //초기화
        endPos = new Vector3(150, 430, 0);

        shadowSprite = NGUITools.AddChild(gameObject, InGameEffectMaker.instance.flyStatueShadowObj).GetComponent<UISprite>();
        shadowSprite.gameObject.transform.localPosition = new Vector3(54, -46, 0);
        statueSprite.depth++;

        StartCoroutine(CoFlyStatue());
    }

    IEnumerator CoFlyStatue ()
    {
        //최종위치찾기
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.STATUE)
            {
                float tempStartPos = (1 - GameUIManager.instance.listGameTarget.Count) * 48;

                endPos += new Vector3(tempStartPos + 96 * i, 0, 0);
                break;                
            }
        }

        float timer = 0;
        float timerX;
        float timerY;

        int addX = startPos.x > 0 ? 1 : -1;
        int addY = startPos.y > 0 ? 1 : -1;

        int rotZ = startRotatePos ==  0 ? 1 : -1;
            
        if(startRotatePos == 270)
        {
            startRotatePos = -90;
        }

        Vector3 beforePos = Vector3.zero;

        while (timer < 0.8f)
        {
            if(timer > 0.4f)
                timer += Global.deltaTimePuzzle*1.2f;
            else 
            timer += Global.deltaTimePuzzle * 0.8f;

            timerX = curveX.Evaluate(timer);
            timerY = curveY.Evaluate(timer);

            _transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, timerY) + new Vector3(0, 100 * curveV.Evaluate(timer*1.25f), 0);            
            _transform.localScale = startScale * 0.5f + Vector3.one * 0.3f * timerX; 

            if (timer > 0.4f)
            {
                _transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 1f* rotZ), new Vector3(0, 0, 10f * rotZ), (timer * 1.25f - 0.5f)*2);
            }
            else
            {
                _transform.localEulerAngles = Vector3.Lerp(new Vector3(0,0,startRotatePos), new Vector3(0,0,1f* rotZ), (timer*1.25f)*2);
            }            

            if(flyStatueList.Contains(this) && timer > 0.7f)
            {
                flyStatueList.Remove(this);
            }
            yield return null;
        }

        midPos = _transform.localPosition;
        midRot = _transform.localEulerAngles;
        midSca = _transform.localScale;


        timer = 0;        
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 2;

            _transform.localPosition = Vector3.Lerp(midPos, endPos, timer) - new Vector3(startPos.x * Mathf.Sin(ManagerBlock.PI90*2 * timer)*0.3f, 0,0);
            _transform.localScale = Vector3.Lerp(midSca, Vector3.one*0.217f, timer);
 
            if (timer > 0.5f)
            {
             //   _transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -10f * rotZ), new Vector3(0, 0, 0f), (timer - 0.5f) * 2);
            }
            else
            {
                _transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 10f * rotZ), new Vector3(0, 0, -30f * rotZ), (timer) * 2);
            }
            yield return null;
        }


        
        ManagerSound.AudioPlay(AudioInGame.GET_STATUE);
        GameUIManager.instance.RefreshTarget(TARGET_TYPE.STATUE);
        FlyTarget.flyTargetCount--;


        GameUIManager.instance.Liststatue.Add(gameObject);
        //transform.parent = ManagerUI._instance.anchorCenter.transform;
        gameObject.SetActive(false);



        Destroy(shadowSprite.gameObject);

        yield return null;
    }


    void Update () {
		
	}
}
