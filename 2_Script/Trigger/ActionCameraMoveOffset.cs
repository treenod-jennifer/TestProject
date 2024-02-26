using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraMoveOffset : ActionBase
{
    public AnimationCurve _curve = new AnimationCurve();
    public float _duration = 1f;
    public Vector2 _offset;
    public float _zoom = 0f;

    public override void DoAction()
    {


      /*  if (_startPosition != null)
        {
            Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
            float len = 0f;
            CameraController.planeUp.Raycast(ray, out len);
            Vector3 startPos = ray.GetPoint(len);
            Vector3 offset = CameraController._instance._transform.position - startPos;
            Vector3 pos = offset + _startPosition.position;
            CameraController._instance._proCamera.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));

            
        }

        if (_duration > 0f)
        {
            if (_endPosition != null || _zoom > 0f)
                StartCoroutine(CoCameraMove());
        }
        else
        {
            if (_zoom > 0f)
                CameraController._instance.moveCamera.fieldOfView = _zoom;
        }*/
              
        if (_duration > 0f)
            StartCoroutine(CoCameraMove());

        //Debug.Log("ActionCameraMoveOffset _ DoAction");

    }
    IEnumerator CoCameraMove()
    {
        Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = CameraController._instance._transform.position - startPos;

        float timer = 0f;
        Vector3 pos = Vector3.zero;
        //float startZoom = CameraController._instance.moveCamera.fieldOfView;
        float startZoom = CameraController._instance.GetFieldOfView();

        Vector3 up = Vector3.Cross(CameraController._instance._transform.right,Vector3.up);
        Vector3 endPosition = startPos + CameraController._instance._transform.right * _offset.x + up * _offset.y;

        while (true)
        {
            float ratio = _curve.Evaluate(timer / _duration);

            {
                pos = offset + Vector3.Lerp(startPos, endPosition, ratio);
                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
            }

            if (_zoom > 0f)
            {
                //CameraController._instance.moveCamera.fieldOfView = Mathf.Lerp(startZoom, _zoom, ratio);
                CameraController._instance.SetFieldOfView(Mathf.Lerp(startZoom, _zoom, ratio));
            }


            timer += Global.deltaTimeLobby;
            if (timer > _duration)
            {
                {
                    pos = offset + endPosition;
                    CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
                }
                break;
            }
            yield return null;
        }

       // Vector3 startPos = CameraController._instance._transform.position;

        yield return null;
    }
}
