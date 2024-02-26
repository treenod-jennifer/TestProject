using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraMove : ActionBase
{
    public bool _conditionWaitOn = false;
    public AnimationCurve _curve = new AnimationCurve();
    public float _duration = 1f;
    public Transform _startPosition;
    public Transform _endPosition;
    public Vector2 _offset;
    public float _zoom = 0f;
    public TypeCharacterType _type = TypeCharacterType.None;
    Character character = null;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        character = ManagerLobby._instance.GetCharacter(_type);


        if (_startPosition != null)
        {
            Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
            float len = 0f;
            CameraController.planeUp.Raycast(ray, out len);
            Vector3 startPos = ray.GetPoint(len);
            Vector3 offset = CameraController._instance._transform.position - startPos;
            Vector3 pos = offset + _startPosition.position;
            CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));

            
        }

        if (_duration > 0f)
        {
            if (_endPosition != null || _zoom > 0f || character != null)
                StartCoroutine(CoCameraMove());
        }
        else
        {
            if (_zoom > 0f)
                CameraController._instance.SetFieldOfView(_zoom);
                //CameraController._instance.moveCamera.fieldOfView = _zoom;
        }


        //Debug.Log("ActionCameraMove _ DoAction");

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


        Vector3 up = Vector3.Cross(CameraController._instance._transform.right, Vector3.up);
        Vector3 offsetPosition = CameraController._instance._transform.right * _offset.x + up * _offset.y;

        while (true)
        {
            float ratio = _curve.Evaluate(timer / _duration);



            if (character != null)
            {
                pos = offset + Vector3.Lerp(startPos, character._transform.position + offsetPosition, ratio);
                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
            }
            else if (_endPosition != null)
            {
                pos = offset + Vector3.Lerp(startPos, _endPosition.position + offsetPosition, ratio);
                CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
            }

            if (_zoom > 0f)
            {
                CameraController._instance.SetFieldOfView(Mathf.Lerp(startZoom, _zoom, ratio));
                //CameraController._instance.moveCamera.fieldOfView = Mathf.Lerp(startZoom, _zoom, ratio);
            }


            timer += Global.deltaTimeLobby;
            if (timer > _duration)
            {
                if (character != null)
                {
                    pos = offset + character._transform.position + offsetPosition;
                    CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
                }
                else if (_endPosition != null)
                {
                    pos = offset + _endPosition.position + offsetPosition;
                    CameraController._instance.MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
                }
                bActionFinish = true;
                break;
            }
            yield return null;
        }

       // Vector3 startPos = CameraController._instance._transform.position;

        yield return null;
    }
    void OnDrawGizmosSelected()
    {
        if (_startPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_startPosition.position, 0.5f);
        }
        if (_endPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_endPosition.position, 0.5f);

            if (_startPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_startPosition.position, _endPosition.position);
            }
        }
    }
}
