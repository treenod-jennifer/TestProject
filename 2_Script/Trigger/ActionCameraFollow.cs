using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraFollow : ActionBase
{
    public float smoothTime = 0.5f;
    public float _stopWaitStart = 1f;
    public float _stopWaitTime = 0.2f;

    public Vector3 _startOffset = Vector3.zero;
    public float _startZoom = 0f;

    public TypeCharacterType _type = TypeCharacterType.Boni;
    public Vector2 _offset;
    public float _zoom = 0f;

    Character character;

    public override void DoAction()
    {
        character = ManagerLobby._instance.GetCharacter(_type);
        StartCoroutine(CoCameraMove());

        //Debug.Log("ActionCameraFollow _ DoAction");

    }
    IEnumerator CoCameraMove()
    {
        {
            if (_startZoom > 0f)
                CameraController._instance.SetFieldOfView(_startZoom);
                //CameraController._instance.moveCamera.fieldOfView = _startZoom;

            if (_startOffset != Vector3.zero)
            {
                Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
                float len = 0f;
                CameraController.planeUp.Raycast(ray, out len);
                Vector3 startPos = ray.GetPoint(len);
                Vector3 offset = CameraController._instance._transform.position - startPos;
                Vector3 pos = offset + _startOffset + character._transform.position;
                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
            }

        }
        {
            Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
            float len = 0f;
            CameraController.planeUp.Raycast(ray, out len);
            Vector3 startPos = ray.GetPoint(len);
            Vector3 offset = CameraController._instance._transform.position - startPos;

            float timer = 0f;
            float timerLeave = 0f;
            //float startZoom = CameraController._instance.moveCamera.fieldOfView;
            float startZoom = CameraController._instance.GetFieldOfView();

            Vector3 up = Vector3.Cross(CameraController._instance._transform.right, Vector3.up);
            Vector3 offsetPosition = CameraController._instance._transform.right * _offset.x + up * _offset.y;

            Vector3 currentVelocity = Vector3.zero;
            float zcurrentVelocity = 0f;
            Vector3 beforePos = Vector3.zero;

            while (true)
            {
                Vector3 targetPos = character._transform.position + offset + offsetPosition;

                targetPos = Vector3.SmoothDamp(CameraController._instance._transform.position, targetPos, ref currentVelocity, smoothTime, 10000f, Global.deltaTimeLobby);
                if (_zoom > 0f)
                    CameraController._instance.SetFieldOfView(Mathf.SmoothDamp(CameraController._instance.GetFieldOfView(), _zoom, ref zcurrentVelocity, smoothTime, 10000f, Global.deltaTimeLobby));
                    //CameraController._instance.moveCamera.fieldOfView = Mathf.SmoothDamp(CameraController._instance.moveCamera.fieldOfView, _zoom, ref zcurrentVelocity, smoothTime, 10000f, Global.deltaTimeLobby);

                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(targetPos.x, targetPos.z));


                timer += Global.deltaTimeLobby;
                if (timer > _stopWaitStart)
                {
                    if (beforePos == character._transform.position)
                    {
                        timerLeave += Global.deltaTimeLobby;
                        if (timerLeave > _stopWaitTime)
                            break;
                    }
                    else
                        timerLeave = 0f;

                    beforePos = character._transform.position;
                }
                yield return null;
            }
        }


        yield return null;
    }
}
