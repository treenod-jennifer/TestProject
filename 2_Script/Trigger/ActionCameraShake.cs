using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraShake : ActionBase
{
    public float _duration = 1f;
 //   public Vector2 _offset;
 //   public float _zoom = 0f;


    public float _amplitudeX = 0.5f;
    public float _frequencyX = 4f;
    float _phaseX = 0f;
    public float _amplitudeY = 0.5f;
    public float _frequencyY = 4f;
    float _phaseY = 0f;
    Vector3 _startPos = Vector3.zero;


    public override void DoAction()
    {

        //Debug.Log("ActionCameraShake _ DoAction");
        if (_duration > 0f)
            StartCoroutine(CoCameraShake());



    }
    IEnumerator CoCameraShake()
    {
        _phaseX = Random.value;
        _phaseY = Random.value;

        Vector3 startPos = CameraController._instance._transform.position;

        float timer = 0f;
        while (timer<=1f)
        {
            timer += Global.deltaTimeLobby;
            float factor = timer * _duration;
            Vector3 value = Vector3.zero;

            if (factor <= _duration)
            {
                float result = Mathf.Sin(2.0f * 3.14159f * factor * _frequencyX + _phaseX) * _amplitudeX;
                value.x = result * (_duration - factor) / _duration;
            }
            if (factor <= _duration)
            {
                float result = Mathf.Sin(2.0f * 3.14159f * factor * _frequencyY + _phaseY) * _amplitudeY;
                value.y = result * (_duration - factor) / _duration;
            }

            CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(startPos.x + value.x, startPos.z + value.y));
            yield return null;
        }
        CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(startPos.x, startPos.z));
        yield return null;
    }
}
