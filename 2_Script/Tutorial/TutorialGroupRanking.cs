using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGroupRanking : TutorialBase
{
    /// <summary>
    /// 그룹랭킹 섬 위치로 이동 및 줌 설정
    /// </summary>
    public void CameraMove() => StartCoroutine(CoCameraMove());
    private IEnumerator CoCameraMove()
    {
        ManagerUI._instance.InactiveLobbyUI();
        
        var areaTransform = GroupRankingAreaBase.instance.cameraPositionObject.transform;
        CameraController._instance.MoveToPosition(areaTransform.position, 0.25f);
        CameraController._instance.SetFieldOfView(30);
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// 그룹랭킹 아이콘 제외 블라인드 처리
    /// </summary>
    public void SetBlindPanel()
    {
        ManagerUI._instance.ActiveLobbyUI();
        
        // TODO : 만약 그룹 랭킹 아이콘이 스크롤 영역에 보이지 않을 경우 예외처리
        blind._panel.depth = 30;
        blind.SetDepth(-1);

        blind.SetSize(200, 200);
        blind.SetSizeCollider(140, 140);
        blind._panel.transform.position      =  ManagerGroupRanking.instance.icon.GameObject.transform.position;
        blind._panel.transform.localPosition += (Vector3.down * 70);
        blind._textureCenter.type            =  UIBasicSprite.Type.Sliced;
        
        blind.SetSizeCollider_ClickFunc(150, 150, OnClicked);
    }
    private void OnClicked() => ManagerGroupRanking.instance.OnEventIconClick();

    /// <summary>
    /// 손가락 표기될 로비 아이콘 GameObject 반환
    /// </summary>
    public List<GameObject> GetIconGameObject()
    {
        var listScrollObject = new List<GameObject> { ManagerGroupRanking.instance.icon.GameObject };
        return listScrollObject;
    }

    /// <summary>
    /// 그룹랭킹 관련 팝업이 출력된 상태인지 여부
    /// 그룹랭킹 메인 팝업 or 프로필 설정 팝업 or 시스템 팝업(이벤트 기간 종료 시 안내 팝업)
    /// </summary>
    public bool IsOpenRankingPopup()
    {
        if (UIPopupGroupRanking.instance != null || UIPopupPionProfile.instance != null || UIPopupSystem._instance != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    /// <summary>
    /// 튜토리얼 종료 시 만일을 대비한 처리
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        ManagerUI._instance.ActiveLobbyUI();
        CameraController._instance.SetFieldOfView(23);
    }
}