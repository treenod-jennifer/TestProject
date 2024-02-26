using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIHousingGauge : MonoBehaviour
{
    [SerializeField] private Transform uiRoot;
    [SerializeField] private UISprite progressBox;
    [SerializeField] private UISprite progressBar;

    private bool isShowGuage = false;

    private Tween tween = null;

    public void InitHousingGauge(float timer)
    {
        progressBar.fillAmount = 0;
        transform.position = ManagerUI._instance._camera.ScreenToWorldPoint(Global._touchPos);
        transform.localPosition += Vector3.up * 50f;
        tween = DOTween.To(() => progressBar.fillAmount, x => progressBar.fillAmount = x, 1f, timer);
        isShowGuage = true;
        StartCoroutine(CoSetPositionGauge());
    }

    public void HideHousingGauge()
    {
        tween?.Kill();
        Destroy(this.gameObject);
    }

    private IEnumerator CoSetPositionGauge()
    {
        float _foffsetX = (progressBox.width * 0.5f);
        float _foffsetY = (progressBox.height * 0.5f);

        while (isShowGuage == true)
        {
            transform.position = ManagerUI._instance._camera.ScreenToWorldPoint(Global._touchPos);
            transform.localPosition += Vector3.up * 50f;
            HousingGaugeMoveToScreen(ManagerUI._instance.uiScreenSize, _foffsetX, _foffsetY);
            yield return null;
        }
    }

    #region 화면 범위 관련.

    //화면 범위 넘어갔을 때 게이지 위치 설정.
    private void HousingGaugeMoveToScreen(Vector2 uiScreen, float _foffsetX, float _foffsetY)
    {
        if (transform.localPosition.y < (uiScreen.y) * -1)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (uiScreen.y) * -1, transform.localPosition.z);
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
