using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureInfo : MonoBehaviour
{
    public static float ShowTime { get; private set; } = 1.5f;
    [SerializeField] private GameObject infoRoot;

    public void ShowInfo()
    {
        if (!gameObject.activeInHierarchy) return;

        if (!infoRoot.activeSelf)
            StartCoroutine("ShowInfoAni");
        else
        {
            StopCoroutine("ShowInfoAni");
            StartCoroutine("ShowInfoAni");
        }
    }

    private IEnumerator ShowInfoAni()
    {
        ActiveOn();

        yield return new WaitForSeconds(ShowTime);

        ActiveOff();
    }

    private void ActiveOn()
    {
        infoRoot.SetActive(true);
    }

    private void ActiveOff()
    {
        infoRoot.SetActive(false);
    }
}
