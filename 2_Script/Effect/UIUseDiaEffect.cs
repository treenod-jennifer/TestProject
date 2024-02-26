using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIUseDiaEffect : MonoBehaviour
{
    public Transform _Transform;
    public UISprite cloverHead;
    public UISprite cloverTail;

    public Vector3 startPos;
    public Vector3 EndPos;

    float effect_Timer = 0;

    public AnimationCurve curveSpeed;
    public AnimationCurve curveWidth;
    public AnimationCurve curveTemp;
    public AnimationCurve curveTemp2;

    public GameObject targetObj = null;

    private float scaleWidth = 0;
    public float width = 10f;

    public float startScale = 1.0f;
    public float endScale = 1.0f;

    public void Init(Vector3 tempStart, Vector3 tempEnd)
    {
        ManagerUI._instance.bTouchTopUI = false;
        startPos = tempStart;
        EndPos = tempEnd;

        scaleWidth = EndPos.x - startPos.x;
    }

    void Update()
    {
        if (effect_Timer > 1)
        {
            targetObj.SendMessage("ShowUseDia", SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject);
        }

        effect_Timer += Global.deltaTimeLobby * 1.2f;
        float ratio = curveSpeed.Evaluate(effect_Timer);
        float ratio2 = curveWidth.Evaluate(effect_Timer);
        float ratio3 = curveTemp.Evaluate(effect_Timer);
        float ratio4 = curveTemp2.Evaluate(effect_Timer);

        float tempheight = Mathf.Lerp(startPos.y, EndPos.y, ratio);

        _Transform.localScale = Vector3.one * (1 + ratio2 * 0.5f);
        _Transform.localEulerAngles = new Vector3(0, 0, 180 * (1 - Mathf.Sin(ManagerBlock.PI90 * ratio)));
        _Transform.position = new Vector3(startPos.x + scaleWidth * ratio2, tempheight, 0);
        _Transform.localPosition = _Transform.localPosition + new Vector3(ratio2 * width * ratio4, 0f, 0f);
        _Transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, ratio);

        //float tempheight = Mathf.Lerp(startPos.y, EndPos.y, ratio3);
        //float widthOffset = Mathf.Sin(ratio2 * Mathf.PI);

        //_Transform.localScale = Vector3.one * (1 + ratio2 * 0.5f);
        //_Transform.localEulerAngles = new Vector3(0, 0, 180 * (1 - Mathf.Sin(ManagerBlock.PI90 * ratio)));
        //_Transform.position = new Vector3(startPos.x + scaleWidth * ratio2, tempheight, 0);
        //_Transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, ratio);
    }

}
