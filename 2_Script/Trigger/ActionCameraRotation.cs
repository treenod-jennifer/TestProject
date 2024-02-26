using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraRotation : ActionBase
{
    public AnimationCurve _curve = new AnimationCurve();
    public float _duration = 1f;
    public float _height = 150.8f;
    public Vector3 _rotation = new Vector3(50f,-45f,0f);


    public override void DoAction()
    {

        if (_duration > 0f)
        {
            StartCoroutine(CoCameraMove());
        }
        else
        {
            Vector3 pos = CameraController._instance._transform.position;
            pos.y = _height;

            CameraController._instance._transform.position = pos;
            CameraController._instance._transform.rotation = Quaternion.Euler(_rotation);
        }
        //Debug.Log("ActionCameraRotation _ DoAction");
    }
    IEnumerator CoCameraMove()
    {
        float startHeight = CameraController._instance._transform.position.y;
        Quaternion startRotation = CameraController._instance._transform.rotation;
        float timer = 0f;

        while (true)
        {
            timer += Global.deltaTimeLobby;
            float ratio = _curve.Evaluate(timer / _duration);


            CameraController._instance._transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(_rotation), ratio);

            Vector3 pos = CameraController._instance._transform.position;
            pos.y = Mathf.Lerp(startHeight,_height, ratio);
            CameraController._instance._transform.position = pos;

            
            if (timer > _duration)
            {


                break;
            }
            yield return null;
        }


        yield return null;
    }
}
