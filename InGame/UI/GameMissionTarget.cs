using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMissionTarget : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;

    public UISprite targetSprite;
    public UILabel targetCount;
    public UILabel targetCountShadow;

    public UISprite checkSprtie;

    public UILabel ScoreCount;
    public UILabel ScoreCountShadow;


    [System.NonSerialized]
    public int index;
    public TARGET_TYPE targetType;
    public BlockColorType targetColor;
    public int count;

    int leftCount = -1;

    void Awake()
    {
        _transform = transform;
    }

    float _effectTimer = 1f;

    public void ShowChangeCount(int tempCount)
    {
        count = tempCount;

        if (Time.time - effectStartTime > 0.15f)
        {
            waitTimer = 0;

            if (showEffect == false)
            {
                showEffect = true;
                effectStartTime = Time.time;
                StartCoroutine(CoChangeCount());
            }
        }
    }

    private int CURVE_SPPED = 3;
    float waitTimer = 0;
    bool showEffect = false;

    float effectStartTime = 0;

    IEnumerator CoChangeCount()
    {
        waitTimer = 0;

        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * CURVE_SPPED;
            float ratio = ManagerBlock.instance._curveBlockPopUp.Evaluate(waitTimer);
            transform.localScale = Vector3.one * ratio;

            yield return null;
        }
        showEffect = false;
        yield return null;
    }

}
