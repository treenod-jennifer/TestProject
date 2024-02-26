using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAreaIcon : MonoBehaviour
{
    public static LobbyAreaIcon _instance = null;
    
    private Vector2 houseCameraPos = new Vector2(78.0f, -97.0f);
    private Vector2 workCameraPos;

    private UIAreaIcon houseIcon = null;
    private UIAreaIcon workIcon = null;

    private bool bSetPos = false;
    private bool bStartHouse = false;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void SetWorkPosition()
    {
        workCameraPos = new Vector2(CameraController._instance._transform.position.x, CameraController._instance._transform.position.z);
        //work 카메라 시작 영역이 보니집 카메라 영역과 비슷하다면 보니 집 영역으로 체크.
        if (Vector2.Distance(workCameraPos, houseCameraPos) <= 30.0f)
        {   
            bStartHouse = true;
        }
        bSetPos = true;
    }

    private void LateUpdate()
    {
        if (bSetPos == false)
            return;
        CalculatePosition();
    }

    private void CalculatePosition()
    {
        Vector2 cameraPos = new Vector2(CameraController._instance._transform.position.x, CameraController._instance._transform.position.z);
        float houseDistance = Vector2.Distance(houseCameraPos, cameraPos);
        float workDistance = Vector2.Distance(workCameraPos, cameraPos);
        
        if (houseDistance > 60)
        {
            if (houseIcon == null)
            {
                houseIcon = NGUITools.AddChild(ManagerUI._instance.anchorBottomLeft.gameObject, ManagerUI._instance._objAreaIcon).GetComponent<UIAreaIcon>();
                houseIcon.SetIcon(true, ManagerLobby._instance._homeCameraPosition);
                ManagerUI._instance.anchorBottomLeft.AddLobbyButton(houseIcon.gameObject);
            }
        }
        else
        {
            if (houseIcon != null)
            {
                StartCoroutine(houseIcon.DestroyAreaIconAction());
                ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(houseIcon.gameObject);
                houseIcon = null;
            }
        }
        
        if (bStartHouse == false)
        {
            if (workDistance > 60)
            {
                if (workIcon == null)
                {
                    workIcon = NGUITools.AddChild(ManagerUI._instance.anchorBottomLeft.gameObject, ManagerUI._instance._objAreaIcon).GetComponent<UIAreaIcon>();
                    workIcon.SetIcon(false, ManagerLobby._instance._workCameraPosition);
                    ManagerUI._instance.anchorBottomLeft.AddLobbyButton(workIcon.gameObject);
                }
            }
            else
            {
                if (workIcon != null)
                {
                    StartCoroutine(workIcon.DestroyAreaIconAction());
                    ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(workIcon.gameObject);
                    workIcon = null;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (houseIcon != null && ManagerUI._instance != null)
        {
            ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(houseIcon.gameObject);
            Destroy(houseIcon.gameObject);
            houseIcon = null;
        }
        if (workIcon != null && ManagerUI._instance != null)
        {
            ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(workIcon.gameObject);
            Destroy(workIcon.gameObject);
            workIcon = null;
        }
    }
}
