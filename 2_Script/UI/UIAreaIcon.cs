using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class UIAreaIcon : MonoBehaviour
{
    public GameObject backRoot;
    public GameObject alarmRoot;
    public UISprite mainSprite;
    public UISprite icon;

    private Vector3 nguiPos;
    private Vector3 targetPos;

    private bool bHouseIcon = false;
    private bool bUseArrow = true;

    public void SetIcon(bool bHouse, Vector3 pos, bool useArrow = true)
    {
        bHouseIcon = bHouse;
        bUseArrow = useArrow;
        MakeAreaIconAction();
        if (bHouseIcon == true)
        {
            icon.spriteName = "guide_icon_001";
        }
        else
        {
            icon.spriteName = "guide_icon_002";
        }
        icon.MakePixelPerfect();
        targetPos = pos;
    }

    public IEnumerator DestroyAreaIconAction()
    {
        transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, 0.08f);
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    public void UpdateTargetPos(Vector3 pos)
    {
        targetPos = pos;
    }


    private void LateUpdate()
    {   
        nguiPos = PokoMath.ChangeTouchPosNGUI(Camera.main.WorldToScreenPoint(targetPos));
        if( backRoot.activeSelf != bUseArrow )
        {
            backRoot.SetActive(bUseArrow);
        }
        if( backRoot.activeSelf != false )
            backRoot.transform.localRotation = Quaternion.Euler(GetAngle());
        
        icon.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 5f) * 8f);
        CheckAlarm();
        if (alarmRoot.activeInHierarchy == true)
        {
            alarmRoot.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 10f) * 8f);
        }
    }

    private void MakeAreaIconAction()
    {
        transform.localScale = Vector3.one * 0.2f;
        transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
    }

    private float GetUIOffSet()
    {
        float ratio = 120 - (Camera.main.fieldOfView - 10.4f) * 80 / 12f;

        return ratio;
    }

    private void OnClickBtnAreaIcon()
    {
        if( bHouseIcon && ManagerLobby.landIndex != 0)
        {
            ManagerLobby._instance.MoveLand(0);
        }
        else
        {
            if( !bHouseIcon )
            {
                var activeMissions = ServerRepos.OpenMission.Where(x => { return x.state == (int)TypeMissionState.Active; });
                var areaToLandMap = ServerContents.Day.CreateAreaToLandMap();
                int missionId = activeMissions.Max(x => x.idx);
                int landIndex = areaToLandMap[ServerContents.Missions[missionId].sceneArea];
                if (ManagerLobby.landIndex != landIndex)
                {
                    ManagerLobby._instance.MoveLand(landIndex);
                    return;
                }
            }

            CameraController._instance._rigidSkipTimer = 0.5f;
            CameraController._instance.MoveToPosition(targetPos, 0.5f);
        }
    }

    private void CheckAlarm()
    {        
        if (bHouseIcon == true)
        {
            bool bShow = true;
            if ( ManagerLobby.landIndex == 0)
            {
                bUseArrow = true;
                bShow = ObjectMaterial._materialList.Count > 0;
                if (bShow == false && ObjectGiftbox._giftboxList.Count > 0)
                {
                    for (int i = 0; i < ObjectGiftbox._giftboxList.Count; i++)
                    {
                        bShow = ObjectGiftbox._giftboxList[i].GetCanOpenBox();
                        if (bShow == true)
                            break;
                    }
                }
            }
            else
            {
                bUseArrow = false;
            }
            
            if(alarmRoot.activeInHierarchy != bShow)
                alarmRoot.SetActive(ManagerLobby.landIndex == 0 ? bShow : false);
        }
    }

    #region 화면 범위 관련.
    private Vector3 GetAngle()
    {
        if(ManagerUI._instance == null) return Vector3.one;
        
        Vector3 v = nguiPos - (ManagerUI._instance.anchorBottomLeft.gameObject.transform.localPosition + transform.localPosition);
        float z = -90.0f + (Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        return new Vector3(0f, 0f, z);
    }
   
    //화면 범위 넘어갔을 때 말풍선 위치 설정.
    private void AreaIconToScreen(Vector2 uiScreen)
    {
        float _foffsetX = 60f;
        float _foffsetY = 60f;

        if (transform.localPosition.y < (uiScreen.y - (_foffsetY - 10)) * -1)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (uiScreen.y - (_foffsetY - 10)) * -1, transform.localPosition.z);
        }
        else if (transform.localPosition.y > (uiScreen.y - _foffsetY))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (uiScreen.y - _foffsetY), transform.localPosition.z);
        }
        if (transform.localPosition.x < (uiScreen.x - _foffsetX) * -1)
        {
            transform.localPosition = new Vector3((uiScreen.x - _foffsetX) * -1f, transform.localPosition.y, transform.localPosition.z);
        }
        else if (transform.localPosition.x > (uiScreen.x - _foffsetX))
        {
            transform.localPosition = new Vector3((uiScreen.x - _foffsetX), transform.localPosition.y, transform.localPosition.z);
        }
    }
    #endregion
}
