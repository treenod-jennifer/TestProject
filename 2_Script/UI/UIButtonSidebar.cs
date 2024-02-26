using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonSidebar : MonoBehaviour
{
    public enum IconState
    {
        EVENT,
        PACKAGE,
    }

    [SerializeField] private UIUrlTexture texSidebarBtnIcon;
    [SerializeField] private GameObject newIcon;
    [SerializeField] private GameObject bubbleIcon;
    [SerializeField] private GameObject[] bubbleIconArray;
    [SerializeField] private GameObject[] objIcon;

    private Action onClickAction;
    private Coroutine bubbleCoroutine = null;

    private void Start()
    {
        texSidebarBtnIcon.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"icon_package_{ServerRepos.LoginCdn.limitedProductIconResource}");
    }

    private void OnEnable()
    {
        if (bubbleCoroutine == null)
            bubbleCoroutine = StartCoroutine(CoADIconChangeAction());
    }

    private void OnDisable()
    {
        if (bubbleCoroutine != null)
        {
            StopCoroutine(bubbleCoroutine);
            bubbleCoroutine = null;
        }
    }

    public void Init(IconState iconState, Action onClickAction)
    {
        objIcon[(int)iconState].SetActive(true);

        this.onClickAction = onClickAction;
    }

    public void OnButtonClick()
    {
        if (ManagerUI.IsLobbyButtonActive)
        {
            onClickAction?.Invoke();
        }
    }

    public void SetActiveNewIcon(bool isActive)
    {
        newIcon.SetActive(isActive);
    }
    public void SetActiveBubbleIcon(bool isActive)
    {
        bubbleIcon.SetActive(isActive);
    }

    public void DestroyButton()
    {
        var buttonList = gameObject.GetComponentInParent<UILobbyButtonListManager>();
        
        if(buttonList != null)
        {
            buttonList.DestroyLobbyButton(gameObject);
        }

        Destroy(gameObject);
    }

    IEnumerator CoADIconChangeAction()
    {
        bool changeValue = false;

        while (gameObject.activeInHierarchy)
        {
            bubbleIconArray[0].SetActive(changeValue);
            bubbleIconArray[1].SetActive(!changeValue);

            changeValue = !changeValue;

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }
}
