using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemGlobalString : MonoBehaviour {
    [SerializeField] private UILabel stringLabel;
    [SerializeField] private string stringKey;

    private Coroutine coroutine;

    private bool isLoaded = false;

    private const float TIME_OUT = 5.0f;

    public string Text
    {
        get
        {
            return stringKey;
        }
    }

    private void OnEnable()
    {
        if (!isLoaded && IsInitialized())
        {
            if (coroutine != null) StopCoroutine(coroutine);

            coroutine = StartCoroutine(LoadText());
        }
    }

    private IEnumerator LoadText ()
    {
        if (!Global._instance.HasString(stringKey))
        {
            stringLabel.text = "Loading...";

            float totalTime = 0.0f;

            yield return new WaitUntil(() =>
            {
                totalTime += Global.deltaTime;
                return Global._instance.HasString(stringKey) || totalTime > TIME_OUT;
            });
        }

        stringLabel.text = Global._instance.GetString(stringKey);
        coroutine = null;
        isLoaded = true;
    }

    private bool IsInitialized()
    {
        if(Global._pendingReboot)
        {
            return false;
        }
        else
        {
            return stringLabel != null && !string.IsNullOrEmpty(stringKey);
        }
    }
}
