#define DEV_CAM

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CameraController : MonoBehaviour
{


#if DEV_CAM
    private const string MOUSE_SCROLL_WHEEL = "Mouse ScrollWheel";

    // panning
    public static Plane planeUp = new Plane(Vector3.up, 0f);
    public static Vector3 cameraRight = Vector3.zero;

    public static CameraController _instance;
    public Camera moveCamera = null;
    public float dragForceCoefficient = 0.4f;
    public Rigidbody _cameraTarget;
    [System.NonSerialized]
    public Transform _transform;
    // zoom
    private int NearZoom = 10;
    private int FarZoom = 25;

    private Vector3 touchBegan = Vector3.zero;
    private Vector3 position;

    float screenZoomEdit = 1f;
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _transform = transform;
        }
    }
    void Start()
    {
        zoomDistance = GetFieldOfView();
        cameraRight = _transform.right;

        screenZoomEdit = (720f / 1280f) / ((float)Screen.width / (float)Screen.height);

        _cameraTarget.maxDepenetrationVelocity = 100f;
    }

    public Vector3 GetWorldPosFromScreen(Vector3 in_screenPos,float in_scale = 1f)
    {
        Ray ray = moveCamera.ScreenPointToRay(in_screenPos);
        float len = 0f;
        planeUp.Raycast(ray, out len);
        return ray.GetPoint(len * in_scale);
    }
    public Vector3 WorldToScreen(Vector3 in_worldPos)
    {
        return moveCamera.WorldToScreenPoint(in_worldPos);
    }
    public Vector3 GetCenterWorldPos()
    {
        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        return GetWorldPosFromScreen(center);
    }
    
    public void SetCameraPosition(Vector3 in_position)
    {
        Ray ray = moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = _transform.position - startPos;
        Vector3 pos = offset + in_position;
        MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));

        _cameraTarget.transform.position = _transform.position - offset;
        _cameraTarget.velocity = Vector3.zero;
    }
    public void MoveToPosition(Vector3 in_wordPos, float in_duration = 0f)
    {
        Ray ray = moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = _transform.position - startPos;
        Vector3 pos = offset + in_wordPos;
        pos.y = _transform.position.y;

        _rigidSkipTimer = in_duration;
        _cameraTarget.velocity = Vector3.zero;

        DOTween.To(() => _transform.position, x => _transform.position = x, pos, in_duration).SetEase(Ease.InOutSine);
    }
    public void MoveCameraInstantlyToPosition(Vector2 in_pos)
    {
        Vector3 pos = _transform.position;
        _transform.position = new Vector3(in_pos.x, pos.y, in_pos.y);
    }

    private bool IsUiTouched()
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // UI터치 할때는 카메라 컨트롤 막기
        bool touchUI = false;

        if (ManagerUI._instance != null)
        {
            if (UICamera.selectedObject != null && UICamera.selectedObject != ManagerUI._instance.gameObject)
            {
                touchUI = true;
            }
            if (ManagerUI._instance.GetPopupCount() > 0)
            {
                touchUI = true;
            }
        }
        return touchUI;
    }
    [System.NonSerialized]
    public float _rigidSkipTimer = 0f;

    bool startInput = false;
    void Update()
    {
        _rigidSkipTimer -= Global.deltaTime;

        if (ManagerLobby._instance._state != TypeLobbyState.Wait)
        {
            _cameraTarget.velocity = Vector3.zero;
            startInput = false;
            return;
        }

        // 투토리얼시 무효화
        if (ManagerTutorial._instance != null)
            if (ManagerTutorial._instance._current != null)
            {
                _cameraTarget.velocity = Vector3.zero;
                startInput = false;
                return;
            }


        if (ManagerLobby._instance._state != TypeLobbyState.Wait || IsUiTouched())
        {
            _cameraTarget.velocity = Vector3.zero;
            startInput = false;
            return;
        }
        UpdateCameraPosition();


    }


    private Vector3 moveCurrentPosition = Vector3.zero;
    private Vector3 saveVelocity = Vector3.zero;
    private Vector3 saveCurrentVelocity = Vector3.zero;
    private List<Touch> touchList = new List<Touch>();

    private float bounceSize = 8f;
    private float zoomDistance;
    private float zoomCurrentDistance;
    private float touchAllLen;
    private bool _touching;


    Vector3 aaaaa = Vector3.zero;

    private void UpdateCameraPosition()
    {
        position = GetCenterWorldPos();

        _touching = false;

        float moveSmoothTime = 0.05f;
        float moveMaxSpeed = 200f;
        Vector3 move = Vector3.zero;

#if UNITY_EDITOR
        move = UpdateCamera();
#else
        move = UpdateCameraMobileDevice();

        moveSmoothTime = 0.02f;
        moveMaxSpeed = 1000f;
#endif
        
        Vector3 endPos = Vector3.SmoothDamp(position, position + move, ref moveCurrentPosition, moveSmoothTime, moveMaxSpeed, Global.deltaTimeNoScale);
        Vector3 deltaPosition = GetMoveDeltaPosition(position, moveMaxSpeed, endPos);
        //Vector3 deltaPosition = Vector3.zero;

        {
            //_cameraTarget.transform.position = position;

            
            //_cameraTarget.
            if (_rigidSkipTimer <= 0f)
            {
                if (_cameraTarget.isKinematic == true)
                    _cameraTarget.isKinematic = false;



                _cameraTarget.velocity = Vector3.ClampMagnitude((position - _cameraTarget.transform.position) / Global.deltaTime,700f);
                deltaPosition += Vector3.SmoothDamp(Vector3.zero, _cameraTarget.transform.position - position, ref aaaaa, 0.1f, 200f, Global.deltaTimeNoScale);
            }
            else
            {
                _cameraTarget.velocity = Vector3.zero;
                if (_cameraTarget.isKinematic == false)
                    _cameraTarget.isKinematic = true;

                {
                    Ray ray = moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
                    float len = 0f;
                    CameraController.planeUp.Raycast(ray, out len);
                    Vector3 startPos = ray.GetPoint(len);
                    Vector3 offset = _transform.position - startPos;
                    Vector3 pos = offset + _transform.position;

                    _cameraTarget.transform.position = _transform.position - offset;
                    _cameraTarget.velocity = Vector3.zero;
                    
                }
            }

        }
        _transform.position += deltaPosition;// deltaPosition;


    }
    Collider[] overlapCapsuleResult = new Collider[10];
    private Vector3 GetMoveDeltaPosition(Vector3 position, float moveMaxSpeed, Vector3 endPos)
    {
        Vector3 deltaPosition = endPos - position;

        if (!_touching)
        {
            saveVelocity = Vector3.SmoothDamp(saveVelocity, Vector3.zero, ref saveCurrentVelocity, 0.25f, moveMaxSpeed, Global.deltaTimeNoScale);
            deltaPosition = saveVelocity;
        }
        float bSize = bounceSize + (zoomDistance - 10f) * 1.0f;
        //Collider[] result = Physics.OverlapCapsule(position, endPos, bSize, Global.cameraBounceMask);
        //Collider[] result= new;// = Physics.OverlapCapsule(position, endPos, bSize, Global.cameraBounceMask);

        

        if (deltaPosition.sqrMagnitude > 0.0001f)
        {
       /*     Debug.Log(deltaPosition.sqrMagnitude + "  " + bSize);
            int resultCount = Physics.OverlapCapsuleNonAlloc(position, endPos, bSize, overlapCapsuleResult, Global.cameraBounceMask);
            Debug.Log("resultCount  " + resultCount);
            if (resultCount > 0)
            {
                int collsionCount = resultCount;
                bool outOfBorder = collsionCount > 0;
                Vector3 force = Vector3.zero;

                if (outOfBorder && _touching)
                {
                    CalcDragForcedDeltaPosition(position, endPos, ref deltaPosition, overlapCapsuleResult, collsionCount, ref force);
                }
                else
                {
                    CalcForcedDealtaPosition(position, ref deltaPosition, bSize, overlapCapsuleResult, collsionCount, ref force);
                }
                if (!_touching)
                {
                    saveVelocity += force * Global.deltaTimeNoScale * 0.1f;
                }
            }*/
        }
        

        



        
        if (_touching)
        {
            saveVelocity = Vector3.SmoothDamp(saveVelocity, deltaPosition, ref saveCurrentVelocity, 0.1f);
        }
        float speed = saveVelocity.magnitude;
        saveVelocity = saveVelocity.normalized * Mathf.Min(5.0f, speed);
        deltaPosition.y = 0f;

        return deltaPosition;
    }

    #region 경계 조작시의 힘계산
    private void CalcDragForcedDeltaPosition(Vector3 position, Vector3 endPos, ref Vector3 deltaPosition, Collider[] result, int collsionCount, ref Vector3 force)
    {
        float sumOfMagnitude = 0.0f;

        for (int i = 0; i < collsionCount; i++)
        {
            Vector3 f = CalcDragForce(position, deltaPosition, result[i]);
            float magnitude = f.magnitude;

            if (magnitude < 0.0001f)
            {
                sumOfMagnitude = 0.0f;
                break;
            }
            sumOfMagnitude += magnitude;
            force += f;
        }
        Vector3 direction = Vector3.Normalize(endPos - position);
        deltaPosition = direction * sumOfMagnitude * dragForceCoefficient * Global.deltaTimeNoScale * (0.1f / collsionCount);
    }

    // min value 미만 0.
    private float GetValueBetween(float min, float max, float v)
    {
        float result = Mathf.Clamp(v, 0, max);
        result = result < min ? 0.0f : result;
        return result;
    }

    private Vector3 CalcDragForce(Vector3 position, Vector3 velocity, Collider collider)
    {
        Vector3 collisionPoint = position - collider.ClosestPointOnBounds(position);
        Vector3 collisionNearPoint = position - collider.ClosestPoint(position);
        
        float toBound = GetValueBetween(1.5f, 8.0f, Vector3.Distance(collisionNearPoint, collisionPoint));
        float length = GetValueBetween(1.5f, 8.0f, collisionNearPoint.magnitude);
        
        Vector3 deltaForce = -collisionNearPoint.normalized * toBound * length * velocity.magnitude;

        return deltaForce;
    }
    #endregion

    #region 일반 조작시의 힘계산
    private void CalcForcedDealtaPosition(Vector3 position, ref Vector3 deltaPosition, float bSize, Collider[] result, int collsionCount, ref Vector3 force)
    {
        for (int i = 0; i < collsionCount; i++)
        {
            Vector3 deltaForce = CalcDeltaForce(position, bSize, result[i]);
            force += deltaForce;
        }
        if (collsionCount > 0)
        {
            deltaPosition += force * Global.deltaTimeNoScale * (1.0f / collsionCount);
        }
    }
    
    private Vector3 CalcDeltaForce(Vector3 position, float bSize, Collider collider)
    {
        Vector3 collisionNearPoint = position - collider.ClosestPoint(position);
        float length = collisionNearPoint.magnitude;
        float strength = Mathf.Abs(bSize - length);

        Vector3 deltaForce = collisionNearPoint.normalized * strength * 40f;

        return deltaForce;
    }
    #endregion

    private void OnTouch()
    {
        Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);

        float len = 0f;
        planeUp.Raycast(ray, out len);
        touchBegan = ray.GetPoint(len);
    }

    private Vector3 OnPressed()
    {
        Vector3 move = Vector3.zero;
        Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);

        float len = 0f;
        planeUp.Raycast(ray, out len);

        Vector3 targetPos = ray.GetPoint(len);
        move = touchBegan - targetPos;
        _touching = true;

        return move;
    }
    public void SetFieldOfView(float in_field)
    {
        moveCamera.fieldOfView = in_field * screenZoomEdit;
    }
    public float GetFieldOfView()
    {
        return moveCamera.fieldOfView / screenZoomEdit;
    }

    private void UpdateCameraZoom()
    {
        float axis = Input.GetAxis(MOUSE_SCROLL_WHEEL);
        zoomDistance -= axis * 10f;
        zoomDistance = Mathf.Clamp(zoomDistance, NearZoom, FarZoom);
       //moveCamera.fieldOfView = Mathf.SmoothDamp(moveCamera.fieldOfView, zoomDistance, ref zoomCurrentDistance, 0.1f);

        SetFieldOfView(Mathf.SmoothDamp(GetFieldOfView(), zoomDistance, ref zoomCurrentDistance, 0.1f));
    }

    private Vector3 UpdateCamera()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            startInput = true;
            OnTouch();
        }
        if (Input.GetMouseButton(0) && startInput)
        {
            move = OnPressed();
        }
        UpdateCameraZoom();
        return move;
    }

    private Vector3 UpdateCameraMobileDevice()
    {
        Vector3 move = Vector3.zero;

        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch getTouch = Input.GetTouch(i);

            if (getTouch.phase == TouchPhase.Began)
            {
                /*     Ray ray = moveCamera.ScreenPointToRay(getTouch.position);
                     float len = 0f;
                     planeUp.Raycast(ray, out len);
                     touchBegan = ray.GetPoint(len);*/
                startInput = true;
                touchList.Add(getTouch);

                Vector2 begingP = Vector2.zero;
                for (int t = 0; t < touchList.Count; t++)
                    begingP += touchList[t].position;
                begingP = begingP / (float)touchList.Count;

                Ray ray = moveCamera.ScreenPointToRay(begingP);
                float len = 0f;
                planeUp.Raycast(ray, out len);
                touchBegan = ray.GetPoint(len);
            }

            if ((getTouch.phase == TouchPhase.Moved || getTouch.phase == TouchPhase.Stationary) && startInput)
            {

                for (int t = 0; t < touchList.Count; t++)
                {
                    if (touchList[t].fingerId == getTouch.fingerId)
                    {
                        touchList[t] = getTouch;
                        _touching = true;
                        break;
                    }
                }

            }
           /* else if (getTouch.phase != TouchPhase.Began && getTouch.phase == TouchPhase.Canceled && getTouch.phase == TouchPhase.Ended)
            {
                if(touchList.Count>0)
                    touchList.Clear();
            }*/

            if ((getTouch.phase == TouchPhase.Canceled || getTouch.phase == TouchPhase.Ended) && startInput)
            {
                for (int t = 0; t < touchList.Count; t++)
                {
                    if (touchList[t].fingerId == getTouch.fingerId)
                    {
                        touchList.RemoveAt(t);
                        break;
                    }
                }

                if (touchList.Count > 0)
                {
                    Vector2 begingP = Vector2.zero;
                    for (int t = 0; t < touchList.Count; t++)
                    {
                        begingP += touchList[t].position;
                    }
                    begingP = begingP / (float)touchList.Count;

                    Ray ray = moveCamera.ScreenPointToRay(begingP);
                    float len = 0f;
                    planeUp.Raycast(ray, out len);
                    touchBegan = ray.GetPoint(len);
                }
            }
        }
        if (Input.touchCount == 0)
        {
            touchList.Clear();
        }
        if (touchList.Count > 0)
        {
            Vector2 moveP = Vector2.zero;
            for (int t = 0; t < touchList.Count; t++)
            {
                moveP += touchList[t].position;
            }
            moveP = moveP / (float)touchList.Count;

            Ray ray = moveCamera.ScreenPointToRay(moveP);
            float len = 0f;
            planeUp.Raycast(ray, out len);

            Vector3 targetPos = ray.GetPoint(len);
            move = touchBegan - targetPos;



           if (move.magnitude > 20f)
           {
               move = Vector3.zero;
               touchList.Clear();
              // Debug.Log(move.magnitude + "   " + Input.touchCount);
              // Debug.Log(touchBegan + " __  " + targetPos);
           }
        }

        if (touchList.Count > 1)
        {
            float lenAll = 0f;
            for (int t = 0; t < touchList.Count - 1; t++)
            {
                lenAll += (touchList[t].position - touchList[t + 1].position).magnitude;
            }
            if (Math.Abs(touchAllLen) > 0.000001f)
            {
                zoomDistance += (touchAllLen - lenAll) * 0.03f;
                zoomDistance = Mathf.Clamp(zoomDistance, NearZoom, FarZoom);
            }
            touchAllLen = lenAll;
        }
        else
        {
            touchAllLen = 0f;
        }
        


        //moveCamera.fieldOfView = Mathf.SmoothDamp(moveCamera.fieldOfView, zoomDistance, ref zoomCurrentDistance, 0.1f);
        SetFieldOfView(Mathf.SmoothDamp(GetFieldOfView(), zoomDistance, ref zoomCurrentDistance, 0.1f));
        //  Input.touchCount == 1 && nowSpeed <= 0f && bIsTwoTouch == false)
        /*  if (Input.touchCount == 1)
          {
              if (Input.GetTouch(0).phase == TouchPhase.Began)
              {
                  Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                  float len = 0f;
                  planeUp.Raycast(ray, out len);
                  touchBegan = ray.GetPoint(len);
              }

              if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
              {
                  Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                  float len = 0f;
                  planeUp.Raycast(ray, out len);
                  targetPos = ray.GetPoint(len);
                  move = touchBegan - targetPos;

                  _touching = true;
              }
          }*/

        return move;
    }

    void OnDrawGizmosSelected()
    {
        float bSize = bounceSize + (zoomDistance - 10f) * 1.3f;

        Vector3 position = GetCenterWorldPos();
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, bSize / 2);
    }
#else

    public static CameraController _instance = null;

    public Camera moveCamera = null;
    [System.NonSerialized]
    public Transform _transform;
    // zoom
    int NearZoom = 10;
    int FarZoom = 25;

    Vector3 touchBegan = Vector3.zero;
    Vector3 targetPos = Vector3.zero;
    // panning
    static public Plane planeUp = new Plane(Vector3.up, 0f);
    static public Vector3 cameraRight = Vector3.zero;


    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _transform = transform;
        }
    }
    void Start()
    {
        zoomDistance = moveCamera.fieldOfView;
        cameraRight = _transform.right;
    }
    public Vector3 GetWorldPosFromScreen(Vector3 in_screenPos)
    {
        Ray ray = moveCamera.ScreenPointToRay(in_screenPos);
        float len = 0f;
        planeUp.Raycast(ray, out len);
        return ray.GetPoint(len);
    }
    public Vector3 WorldToScreen(Vector3 in_worldPos)
    {
        return moveCamera.WorldToScreenPoint(in_worldPos);
    }
    public Vector3 GetCenterWorldPos()
    {
        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        return GetWorldPosFromScreen(center);
    }
    public void SetCameraPosition(Vector3 in_position)
    {
        Ray ray = moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = _transform.position - startPos;
        Vector3 pos = offset + in_position;
        MoveCameraInstantlyToPosition(new Vector2(pos.x, pos.z));
    }
    public void MoveToPosition(Vector3 in_wordPos, float in_duration = 0f)
    {
        Ray ray = moveCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) * 0.5f);
        float len = 0f;
        CameraController.planeUp.Raycast(ray, out len);
        Vector3 startPos = ray.GetPoint(len);
        Vector3 offset = _transform.position - startPos;
        Vector3 pos = offset + in_wordPos;
        pos.y = _transform.position.y;

        DOTween.To(() => _transform.position, x => _transform.position = x, pos, in_duration).SetEase(Ease.InOutSine);
    }
    public void MoveCameraInstantlyToPosition(Vector2 in_pos)
    {
        Vector3 pos = _transform.position;
        _transform.position = new Vector3(in_pos.x, pos.y, in_pos.y);
    }
    void Update()
    {
        if (ManagerLobby._instance._state != TypeLobbyState.Wait)
            return;

        // 투토리얼시 무효화
        if (ManagerTutorial._instance != null)
            if (ManagerTutorial._instance._current != null)
                return;
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // UI터치 할때는 카메라 컨트롤 막기
        bool touchUI = false;


        if (ManagerUI._instance != null)
        {

            if (UICamera.selectedObject != null)
                if (UICamera.selectedObject != ManagerUI._instance.gameObject)
                    touchUI = true;

            if (ManagerUI._instance.GetPopupCount() > 0)
                touchUI = true;
        }

        if (touchUI)
            return;

        CameraUpdate();
    }


    float bounceSize = 8f;
    Vector3 bounceVelocity = Vector3.zero;
    Vector3 bounceCurrentVelocity = Vector3.zero;
    Vector3 moveTargetPosition = Vector3.zero;
    Vector3 moveCurrentPosition = Vector3.zero;
    Vector3 saveVelocity = Vector3.zero;
    Vector3 saveCurrentVelocity = Vector3.zero;
    List<Touch> touchList = new List<Touch>();
    bool _touching = false;

    float zoomDistance = 0f;
    float zoomCurrentDistance = 0f;
    float touchAllLen = 0f;

    void CameraUpdate()
    {
        Vector3 position = GetCenterWorldPos();
        Vector3 move = Vector3.zero;
        _touching = false;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {

            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch getTouch = Input.GetTouch(i);

                if (getTouch.phase == TouchPhase.Began)
                {
                    /*     Ray ray = moveCamera.ScreenPointToRay(getTouch.position);
                         float len = 0f;
                         planeUp.Raycast(ray, out len);
                         touchBegan = ray.GetPoint(len);*/

                    touchList.Add(getTouch);

                    Vector2 begingP = Vector2.zero;
                    for (int t = 0; t < touchList.Count; t++)
                        begingP += touchList[t].position;
                    begingP = begingP / (float)touchList.Count;

                    Ray ray = moveCamera.ScreenPointToRay(begingP);
                    float len = 0f;
                    planeUp.Raycast(ray, out len);
                    touchBegan = ray.GetPoint(len);
                }

                if (getTouch.phase == TouchPhase.Moved || getTouch.phase == TouchPhase.Stationary)
                {

                    for (int t = 0; t < touchList.Count; t++)
                    {
                        if (touchList[t].fingerId == getTouch.fingerId)
                        {
                            touchList[t] = getTouch;
                            break;
                        }
                    }
                    _touching = true;
                }

                if (getTouch.phase == TouchPhase.Canceled || getTouch.phase == TouchPhase.Ended)
                {
                    for (int t = 0; t < touchList.Count; t++)
                    {
                        if (touchList[t].fingerId == getTouch.fingerId)
                        {
                            touchList.RemoveAt(t);
                            break;
                        }
                    }

                    if (touchList.Count > 0)
                    {
                        Vector2 begingP = Vector2.zero;
                        for (int t = 0; t < touchList.Count; t++)
                            begingP += touchList[t].position;
                        begingP = begingP / (float)touchList.Count;

                        Ray ray = moveCamera.ScreenPointToRay(begingP);
                        float len = 0f;
                        planeUp.Raycast(ray, out len);
                        touchBegan = ray.GetPoint(len);
                    }
                }
            }
            if (Input.touchCount == 0)
                touchList.Clear();
            if (touchList.Count > 0)
            {
                Vector2 moveP = Vector2.zero;
                for (int t = 0; t < touchList.Count; t++)
                    moveP += touchList[t].position;
                moveP = moveP / (float)touchList.Count;

                Ray ray = moveCamera.ScreenPointToRay(moveP);
                float len = 0f;
                planeUp.Raycast(ray, out len);
                targetPos = ray.GetPoint(len);
                move = touchBegan - targetPos;
            }

            if (touchList.Count > 1)
            {
                float lenAll = 0f;
                for (int t = 0; t < touchList.Count - 1; t++)
                {
                    lenAll += (touchList[t].position - touchList[t + 1].position).magnitude;
                }
                if (touchAllLen != 0f)
                {
                    zoomDistance += (touchAllLen - lenAll) * 0.03f;
                    zoomDistance = Mathf.Clamp(zoomDistance, NearZoom, FarZoom);

                }
                touchAllLen = lenAll;
            }
            else
            {
                touchAllLen = 0f;
            }
            moveCamera.fieldOfView = Mathf.SmoothDamp(moveCamera.fieldOfView, zoomDistance, ref zoomCurrentDistance, 0.1f);
            //  Input.touchCount == 1 && nowSpeed <= 0f && bIsTwoTouch == false)
            /*  if (Input.touchCount == 1)
              {
                  if (Input.GetTouch(0).phase == TouchPhase.Began)
                  {
                      Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                      float len = 0f;
                      planeUp.Raycast(ray, out len);
                      touchBegan = ray.GetPoint(len);
                  }

                  if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                  {
                      Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                      float len = 0f;
                      planeUp.Raycast(ray, out len);
                      targetPos = ray.GetPoint(len);
                      move = touchBegan - targetPos;

                      _touching = true;
                  }
              }*/
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                float len = 0f;
                planeUp.Raycast(ray, out len);
                touchBegan = ray.GetPoint(len);
            }


            if (Input.GetMouseButton(0))
            {
                Ray ray = moveCamera.ScreenPointToRay(Input.mousePosition);
                float len = 0f;
                planeUp.Raycast(ray, out len);
                targetPos = ray.GetPoint(len);
                move = touchBegan - targetPos;

                _touching = true;
            }


            {
                float axis = Input.GetAxis("Mouse ScrollWheel");
                zoomDistance -= axis * 10f;
                zoomDistance = Mathf.Clamp(zoomDistance, NearZoom, FarZoom);
                moveCamera.fieldOfView = Mathf.SmoothDamp(moveCamera.fieldOfView, zoomDistance, ref zoomCurrentDistance, 0.1f);
            }
        }
        float moveSmoothTime = 0.05f;
        float moveMaxSpeed = 200f;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            moveSmoothTime = 0.02f;
            moveMaxSpeed = 1000f;
        }


        Vector3 endPos = Vector3.SmoothDamp(position, position + move, ref moveCurrentPosition, moveSmoothTime, moveMaxSpeed, Global.deltaTimeNoScale);
        Vector3 velocityEnd = endPos - position;
        if (!_touching)
        {
            saveVelocity = Vector3.SmoothDamp(saveVelocity, Vector3.zero, ref saveCurrentVelocity, 0.25f, moveMaxSpeed, Global.deltaTimeNoScale);
            velocityEnd = saveVelocity;
        }

        float bSize = bounceSize + (zoomDistance - 10f) * 1f;

        Collider[] result = Physics.OverlapCapsule(position, endPos, bSize, Global.cameraBounceMask);
        if (result != null)
        {
            Vector3 force = Vector3.zero;
            for (int i = 0; i < result.Length; i++)
            {
                Vector3 v = position - result[i].ClosestPoint(position);
                float len = v.magnitude;
                force += v.normalized * Mathf.Abs(bSize - len) * 40f;
            }
            velocityEnd += force * Global.deltaTimeNoScale;

            if (!_touching)
                saveVelocity += force * Global.deltaTimeNoScale * 0.1f;
        }




        if (_touching)
            saveVelocity = Vector3.SmoothDamp(saveVelocity, velocityEnd, ref saveCurrentVelocity, 0.1f);


        //bounceVelocity.y = 0f;



        velocityEnd.y = 0f;
        _transform.position += velocityEnd;

    }
    void OnDrawGizmosSelected()
    {
        float bSize = bounceSize + (zoomDistance - 10f) * 1.3f;

        Vector3 position = GetCenterWorldPos();
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, bSize / 2);
    }
#endif
}
