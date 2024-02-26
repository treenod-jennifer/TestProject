
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIWakeupEventBubble : MonoBehaviour
{   
    [SerializeField]
    private GameObject[] activeObj;
    [SerializeField]
    private UISprite spriteBubble;
    [SerializeField]
    private UILabel wakeupText;

    private Coroutine actionRoutine = null;

    public void Start()
    {
        wakeupText.text = Global._instance.GetString("p_wu_2");
    }

    public void OnEnable()
    {
        for (int i = 0; i < activeObj.Length; i++)
            activeObj[i].SetActive(false);
        actionRoutine = StartCoroutine(CoAction());
    }

    private void OnDisable()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }
    }

    private IEnumerator CoAction()
    {
        int index = 0;
        while (true)
        {
            activeObj[index].gameObject.SetActive(true);
            if (index == 0)
            {   
                spriteBubble.gameObject.SetActive(true);
                DOTween.ToAlpha(() => spriteBubble.color, color => spriteBubble.color = color, 1f, 0.2f);
                spriteBubble.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(1.5f);

            if (index >= activeObj.Length - 1)
            {
                spriteBubble.transform.DOScale(new Vector3(0f, 1f, 1f), 0.3f).SetEase(Ease.InBack);
                yield return new WaitForSeconds(0.1f);
                DOTween.ToAlpha(() => spriteBubble.color, color => spriteBubble.color = color, 0f, 0.2f);
                yield return new WaitForSeconds(0.2f);
                spriteBubble.gameObject.SetActive(false);
                activeObj[activeObj.Length - 1].gameObject.SetActive(false);
                yield return new WaitForSeconds(1.5f);
                index = 0;
            }
            else
            {
                activeObj[index].gameObject.SetActive(false);
                index++;
            }
        }
    }
}
