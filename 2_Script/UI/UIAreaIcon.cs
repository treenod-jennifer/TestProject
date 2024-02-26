using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIAreaIcon : MonoBehaviour
{
    public GameObject backRoot;
    public UISprite mainSprite;
    public UISprite icon;

    private Vector3 nguiPos;
    private Vector3 targetPos;

    public void SetIcon(bool bHouse, Vector3 pos)
    {
        MakeAreaIconAction();
        if (bHouse == true)
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

    private void LateUpdate()
    {   
        nguiPos = PokoMath.ChangeTouchPosNGUI(Camera.main.WorldToScreenPoint(targetPos));
        //transform.localPosition = nguiPos + Vector3.up * GetUIOffSet();
        //AreaIconToScreen(ManagerUI._instance.uiScreenSize);
        backRoot.transform.localRotation = Quaternion.Euler(GetAngle());

        icon.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 5f) * 8f);
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
        CameraController._instance._rigidSkipTimer = 0.5f;
        CameraController._instance.MoveToPosition(targetPos, 0.5f);
    }

    #region 화면 범위 관련.
    private Vector3 GetAngle()
    {
        Vector3 v = nguiPos - transform.position;
        float z = -90f + (Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
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
