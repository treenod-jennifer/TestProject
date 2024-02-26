using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIUseCloverEffect : MonoBehaviour
{
    public Transform _Transform;
    public UISprite cloverHead;
    public UISprite cloverTail;

    public Vector3 startPos;
    public Vector3 EndPos;

    float effect_Timer = 0;

    public AnimationCurve curveSpeed;
    public AnimationCurve curveWidth;

    public GameObject targetObj = null;

    private float scaleWidth = 0;

    public void Init(Vector3 tempStart, Vector3 tempEnd, bool isFree = false, bool isWing = false)
    {
        ManagerUI._instance.bTouchTopUI = false;
        startPos = tempStart;
        EndPos = tempEnd;

        scaleWidth = EndPos.x - startPos.x;

        if (isFree == true)
        {
            if(isWing)
            {
                cloverHead.spriteName = "adven_wing_icon_infinity";
            }
            else
            {
                cloverHead.spriteName = "Clover_Big04";
                cloverTail.spriteName = "Clover_Big05";
            }
        }
    }

    void Update()
    {
        if (effect_Timer > 1)
        {
            targetObj.SendMessage("ShowUseClover");

            Destroy(gameObject);
        }

        effect_Timer += Global.deltaTimeLobby*0.85f;
        float ratio = curveSpeed.Evaluate(effect_Timer);
        float ratio2 = curveWidth.Evaluate(effect_Timer);

        float tempheight = Mathf.Lerp(startPos.y, EndPos.y, ratio);

        _Transform.localScale = Vector3.one * (1 + ratio2 * 0.5f);
        _Transform.localEulerAngles = new Vector3(0, 0, 180 * (1 - Mathf.Sin(ManagerBlock.PI90 * ratio)));
        _Transform.position = new Vector3(startPos.x + scaleWidth * ratio2, tempheight, 0);
    }

}
