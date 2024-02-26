using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCameraEffect : MonoBehaviour {
    [System.Serializable]
    private class aniController
    {
        public AnimationCurve xCon;
        public AnimationCurve yCon;
    }

    public static ShakeCameraEffect _instance = null;

    [Header("ShakeAni")]
    [SerializeField] private aniController[] ani_Controller;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void ShakeCamera(int index)
    {
        StartCoroutine(Shake(index));
    }

    private IEnumerator Shake(int index)
    {
        float x;
        float y;
        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            x = ani_Controller[index].xCon.Evaluate(totalTime);
            y = ani_Controller[index].yCon.Evaluate(totalTime);

            transform.localPosition = new Vector3(x, y, 0.0f);

            if (totalTime > ani_Controller[index].xCon.keys[ani_Controller[index].xCon.length - 1].time &&
                totalTime > ani_Controller[index].yCon.keys[ani_Controller[index].yCon.length - 1].time)
                break;

            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }

    [Header("FlickerAni")]
    [SerializeField] private UITexture flicker_Blind;
    [SerializeField] private AnimationCurve flicker_Controller;

    public void FlickerCamera()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        float totalTime = 0.0f;
        flicker_Blind.gameObject.SetActive(true);

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            flicker_Blind.alpha = flicker_Controller.Evaluate(totalTime);

            if (totalTime > flicker_Controller.keys[flicker_Controller.length - 1].time)
                break;

            yield return null;
        }

        flicker_Blind.gameObject.SetActive(false);
    }
}
