using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraEffect : MonoBehaviour {

  //  static public CameraEffect _lastCameraEffect = null;
    public MeshRenderer _meshRenderer;

    Material material = null;
    Color startColor = Color.white;
    Color endColor = Color.white;
    float durationColor = 0f;
    bool autoDestroy = false;

    void Awake()
    {
        material = _meshRenderer.material;
    }
	// Use this for initialization
	void Start () {
        
	}

    static public CameraEffect MakeScreenEffect(int in_Depth = 1)//, bool in_dontDestroyOnLoad = false)
    {
        CameraEffect obj = Instantiate(Global._instance._objScreenEffect).gameObject.GetComponent<CameraEffect>();
        obj.GetComponent<Camera>().depth = in_Depth;

     /*   if (in_dontDestroyOnLoad)
        {
            _lastCameraEffect = obj;
            DontDestroyOnLoad(obj.gameObject);
        }*/
        return obj;
    }

    // in_Depth 가 0보다 작으면 UI카메라 및에 생성      0보다 크면 전체 화면 덮는용,
    public void ApplyScreenEffect(Color in_startColor, Color in_endColor, float in_duration, bool in_autoDestroy = true, int in_Depth = 1)
    {
        durationColor = in_duration;
        startColor = in_startColor;
        endColor = in_endColor;
        autoDestroy = in_autoDestroy;
        material.SetColor("_MainColor", startColor);

        if (durationColor > 0f)
            StartCoroutine(CoChangeColor());
    }
    IEnumerator CoChangeColor()
    {
        float timer = 0f;

        while (timer < durationColor)
        {
            timer += Global.deltaTime;
            material.SetColor("_MainColor", Color.Lerp(startColor, endColor, timer / durationColor));
            yield return null;
        }

        material.SetColor("_MainColor", endColor);
        yield return null;

        if (autoDestroy)
            Destroy(gameObject);
    }
}
