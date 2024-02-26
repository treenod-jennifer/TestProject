using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIButtonLobbyGuest : MonoBehaviour
{
    public static UIButtonLobbyGuest _instance = null;

    [SerializeField] private UILabel textLabelObject;
    [SerializeField] private UISprite sprNewObject;
    private float timer = 0f;

    private Coroutine timeRoutine = null;

    private void Awake()
    {
        _instance = this;
    }

    void OnDestroy()
    {
        _instance = null;
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(gameObject);
        }
    }

    public void Reset()
    {
        timer = 3f;
        if (timeRoutine != null)
        {
            StopCoroutine(timeRoutine);
            timeRoutine = null;
        }

        if (ManagerAIAnimal.HaveNewcomer() == true)
        {
            sprNewObject.gameObject.SetActive(true);
            StartCoroutine(CoButtonDestroy());
            return;
        }
        textLabelObject.gameObject.SetActive(true);

        timeRoutine = StartCoroutine(CoShowButton());
    }

    private IEnumerator CoButtonDestroy()
    {
        yield return new WaitUntil(() => ManagerLobby.landIndex != 0);

        GameObject.Destroy(gameObject);
    }

    private IEnumerator CoShowButton()
    {
        while (true)
        {
            timer -= Global.deltaTime;
            if (timer < 0.5f)
            {
                textLabelObject.color = new Color(textLabelObject.color.r, textLabelObject.color.g, textLabelObject.color.b, timer);
            }

            if (timer < 0f)
                break;

            yield return null;
        }
        GameObject.Destroy(gameObject);
    }

    public void DestroyImmetiately()
    {
        if (timeRoutine != null)
            StopCoroutine(timeRoutine);
        GameObject.Destroy(gameObject);
    }

    private void OnClickBtnLobbyGuest()
    {
        ManagerUI._instance.OpenPopupDiary(TypePopupDiary.eGuest);
    }
}
