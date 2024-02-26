using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void SetWorkPosition(Vector3 workPos, bool reset = false)
    {
        if (reset)
        {
            bSetPos = false;
            bStartHouse = false;
        }

        workCameraPos = new Vector2(workPos.x, workPos.z);
        //work 카메라 시작 영역이 보니집 카메라 영역과 비슷하다면 보니 집 영역으로 체크.
        if (Vector2.Distance(workCameraPos, houseCameraPos) <= 30.0f)
        {   
            bStartHouse = true;
        }
        bSetPos = true;
    }

    (bool, int) CalcMissionPos()
    {
        if (ServerRepos.OpenMission.Count == 0)
        {
            return (false, 0);
        }

        var activeMissions = ServerRepos.OpenMission.Where(x => { return x.state == (int)TypeMissionState.Active; });
        if( activeMissions.Count() == 0 )
        {
            return (false, 0);
        }

        

        var areaToLandMap = ServerContents.Day.CreateAreaToLandMap();
        int missionId = activeMissions.Max(x => x.idx) ;

        if (!ServerContents.Missions.ContainsKey(missionId))
        {
            return (false, 0);
        }
        if( !areaToLandMap.ContainsKey(ServerContents.Missions[missionId].sceneArea))
        {
            return (false, 0);
        }

        int landIndex = areaToLandMap[ServerContents.Missions[missionId].sceneArea];

        var icon = ObjectMissionIcon.FindMissionIcon(missionId);
        if (icon == null || ManagerLobby.landIndex != landIndex)
        {
            return (false, landIndex);
        }

        SetWorkPosition(icon.gameObject.transform.position, true);
        workIcon?.UpdateTargetPos(icon.gameObject.transform.position);

        return (true, landIndex);

    }

    private void LateUpdate()
    {
        if (bSetPos == false)
            return;
        CalculatePosition();
    }

    private void CalculatePosition()
    {
        if (ManagerUI._instance == null || CameraController._instance == null )
            return;

        var (missionRet, landIndex) = CalcMissionPos();
        bool otherLandWork = (missionRet == false && landIndex != ManagerLobby.landIndex);



        Vector2 cameraPos = new Vector2(CameraController._instance._transform.position.x, CameraController._instance._transform.position.z);
        float houseDistance = Vector2.Distance(houseCameraPos, cameraPos);
        float workDistance = Vector2.Distance(workCameraPos, cameraPos);
        
        if (houseDistance > 60 || ManagerLobby.landIndex != 0)
        {
            if (houseIcon == null)
            {
                houseIcon = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIAreaIcon", ManagerUI._instance.anchorBottomLeft.gameObject).GetComponent<UIAreaIcon>();
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
        
        if (bStartHouse == false || otherLandWork)
        {
            if (workDistance > 60 || otherLandWork)
            {
                if (workIcon == null)
                {
                    bool bUseArrow = true;
                    if (otherLandWork)
                        bUseArrow = false;

                    workIcon = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIAreaIcon", ManagerUI._instance.anchorBottomLeft.gameObject).GetComponent<UIAreaIcon>();
                    workIcon.SetIcon(false, ManagerLobby._instance._workCameraPosition, bUseArrow);
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
