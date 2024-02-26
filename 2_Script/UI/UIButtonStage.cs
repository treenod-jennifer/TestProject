using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonStage : MonoBehaviour {
    public static UIButtonStage _instance;
    [SerializeField]
    private GameObject newEffect;
    [SerializeField]
    private AnimationCurve button_AniController;

    private void OnEnable()
    {
        StartCoroutine(Inti());
    }

    private IEnumerator Inti()
    {
        _instance = this;

        while (ManagerData._instance._state != DataLoadState.eComplete)
        {
            yield return null;
        }

        if (!PlayerPrefs.HasKey("LastEpisodeIndex"))
        {
            newEffect.SetActive(false);
            yield break;
        }

        if (PlayerPrefs.GetInt("LastEpisodeIndex") < ManagerData._instance.chapterData.Count)
            OnEffect();
        else
            OffEffect();
    }

    private void OnEffect()
    {
        newEffect.SetActive(true);
        StartCoroutine("CoEffect");
    }

    private IEnumerator CoEffect()
    {
        float timeCount = 0.0f;

        while (true)
        {
            timeCount += Time.unscaledDeltaTime;
            newEffect.transform.localScale = Vector3.one * button_AniController.Evaluate(timeCount);

            yield return null;
        }
    }

    private void OffEffect()
    {
        StopCoroutine("CoEffect");
        newEffect.transform.localScale = Vector3.one;
        newEffect.SetActive(false);
    }

    public void LastEpisodeCheck(int stageIndex = -1)
    {
        if (stageIndex == -1)
        {
            PlayerPrefs.SetInt("LastEpisodeIndex", ManagerData._instance.chapterData.Count);
            OffEffect();
        }
        else
        {
            if (PlayerPrefs.HasKey("LastEpisodeIndex") && getChapterIndex(stageIndex) > PlayerPrefs.GetInt("LastEpisodeIndex"))
            {
                PlayerPrefs.SetInt("LastEpisodeIndex", ManagerData._instance.chapterData.Count);
                OffEffect();
            }
        }
    }

    private int getChapterIndex(int stageIndex)
    {
        for (int i = 0; i < ManagerData._instance.chapterData.Count; i++)
        {
            if (ManagerData._instance.chapterData[i]._stageIndex <= stageIndex && ManagerData._instance.chapterData[i]._stageIndex + ManagerData._instance.chapterData[i]._stageCount > stageIndex)
                return i + 1;
        }
        return -1;
    }
}
