using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using DG.Tweening;

public enum PopupType
{
    none,
    exit,
    shop,
    method,
    housing
}

public class UIPopupBase : MonoBehaviour
{
    //닫는 연출 나오는 도중 콜백.
    public Method.FunctionVoid _callbackClose = null;
    //창이 완전히 사라진 뒤 호출되는 콜백.
    public Method.FunctionVoid _callbackEnd = null;
    //창이 열린 후 호출되는 콜백.
    public Method.FunctionVoid _callbackOpen = null;

    //현재 팝업창의 패널 수.
    public UIPanel uiPanel;

    //현재 팝업창의 최상위에 있는 이미지.
    //이 이미지를 중심으로 팝업창 전체의 알파, 연출등을 관리함.
    public UISprite mainSprite;
    public UISprite blackSprite;

    //현재 패널 + 자식 패널 수(default값은 무조건 1부터).
    public int panelCount = 1;
    //sortOrder 사용하는 수(default값은 무조건 0부터).
    public int sortOrderCount = 0;

    //제일 위 재화UI 보이게 할지 설정.
    public bool bShowTopUI = false;

    [HideInInspector]
    public const float openTime = 0.3f;
    [HideInInspector]
    public bool bCanTouch = false;

    //재화창들 겹쳐서 꺼짐/켜짐 연출 나올 때, 다음 창 열릴 때 까지 터치 막아야 할 때 사용.
    [HideInInspector]
    public bool bShopBehind = false;

    protected PopupType popupType = PopupType.none;

    public virtual void OpenPopUp(int depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        StartCoroutine(CoPostOpenPopup());
    }

    //Base의 오픈팝업 연출은 사용하지 않지만 _callbackOpen()이 필요 할 경우가 생겨 접근성을 변경
    protected virtual IEnumerator CoPostOpenPopup()
    {
        yield return new WaitForSeconds(openTime);

        ManagerUI._instance.bTouchTopUI = true;

        if (_callbackOpen != null)
        {
            _callbackOpen();
            yield return new WaitForSeconds(openTime);
        }

        bCanTouch = true;

        ManagerUI._instance.FocusCheck();
    }

    public virtual void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public virtual void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
        if (mainSprite != null)
        {
            mainSprite.transform.DOScale(Vector3.zero, _mainTime).SetEase(Ease.InBack);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, _mainTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(_mainTime, () =>
          PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            ManagerUI._instance._popupList.Remove(this);
            Destroy(gameObject);
        }));
    }

    public virtual void OnClickBtnBack()
    {
        if (bCanTouch == false)
            return;
        OnClickBtnClose();
    }

    public PopupType GetPopupType()
    {
        return popupType;
    }

    protected void PopUpCloseAlpha()
    {
        DOTween.ToAlpha(() => blackSprite.color, x => blackSprite.color = x, 0f, 0.15f);
        if (_callbackClose != null)
        {
            _callbackClose();
        }
    }
    
    protected IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }

    protected virtual void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.ClosePopUpUI();
    }

    protected virtual void OnDestroy()
    {
        if (bShopBehind == false)
        {
            ManagerUI._instance.bTouchTopUI = true;
        }

        if (_callbackEnd != null)
            _callbackEnd();
    }

    bool haveFocus = false;
    public void FocusCheck()
    {
        if (ManagerUI._instance._popupList == null)
            return;

        var popupList = ManagerUI._instance._popupList;

        int idx = popupList.IndexOf(this);
        bool focus = popupList.Count == idx + 1;

        if( focus != haveFocus )
        {
            haveFocus = focus;
            OnFocus(focus);
        }
    }

    virtual protected void OnFocus(bool focus)
    {   
        // 포커스가 오고나갈 때 이 함수를 오버라이드하면 됩니다
        // 포커스가 올 때 true, 포커스가 꺼질 때 false가 들어오며
        // 팝업이 켜질때는 포커스 true가 오지만, 팝업이 꺼질때는 포커스가 불리지 않으니 주의

        //Debug.Log("OnFocus(" + this.GetType().ToString() + ") : " + focus.ToString());
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale.x * new Vector3(800, 1280, 0));
    }
#endif
}
