using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindMaker : MonoBehaviour
{
    private static BlindMaker _instance;
    private static BlindMaker instance
    {
        get
        {
            if (_instance == null)
                _instance = Instantiate(Global._instance._objBlindMaker).GetComponent<BlindMaker>();

            return _instance;
        }
    }

    [SerializeField] private UISprite blindSprite;
    
    public static void Make(float disabledTime = 0.0f)
    {
        Debug.Log("test");

        instance.StartCoroutine(instance.BlindOnAni());

        if (disabledTime > 0.0f)
            instance.StartCoroutine(instance.BlindOffAni(disabledTime));
    }

    public static void Destroy()
    {
        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.BlindOffAni());
    }

    [SerializeField] private float conversionTime = 0.25f;
    private IEnumerator BlindOnAni()
    {
        blindSprite.gameObject.SetActive(true);

        float totalTime = 0.0f;

        while (totalTime < conversionTime)
        {
            totalTime += Global.deltaTimeLobby;

            Color blindColor = blindSprite.color;
            blindColor.a = Mathf.Lerp(0.0f, 1.0f, totalTime / conversionTime);
            blindSprite.color = blindColor;

            yield return null;
        }

        yield break;
    }

    private IEnumerator BlindOffAni(float disabledTime = 0.0f)
    {
        yield return new WaitForSeconds(disabledTime);

        float totalTime = 0.0f;

        while (totalTime < conversionTime)
        {
            totalTime += Global.deltaTimeLobby;

            Color blindColor = blindSprite.color;
            blindColor.a = Mathf.Lerp(1.0f, 0.0f, totalTime / conversionTime);
            blindSprite.color = blindColor;

            yield return null;
        }

        Destroy(instance.gameObject);

        yield break;
    }
}
