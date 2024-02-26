using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemNewDay : MonoBehaviour
{
    [SerializeField] private UITexture texture_1;
    [SerializeField] private UITexture texture_2;
    [SerializeField] private UILabel script;

    [Tooltip("Angel Curve")]
    [SerializeField] private AnimationCurve animationCurve_1;
    [Tooltip("Scale Curve")]
    [SerializeField] private AnimationCurve animationCurve_2;
    [Tooltip("Alpha Curve")]
    [SerializeField] private AnimationCurve animationCurve_3;

    private Texture2D tempTexture1 = null;
    private Texture2D tempTexture2 = null;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    private void OnDestroy()
    {
        if(tempTexture1 != null)
        {
            Destroy(tempTexture1);
        }

        if(tempTexture2 != null)
        {
            Destroy(tempTexture2);
        }
    }

    public static UIItemNewDay Make(int day, Transform parent = null)
    {
        var origin = Resources.Load("UIPrefab/UIItemNewDay");

        GameObject tipObject;

        if (parent != null)
        {
            tipObject = Instantiate(origin, parent) as GameObject;
        }
        else
        {
            tipObject = Instantiate(origin) as GameObject;
        }

        UIItemNewDay newDay = tipObject.GetComponent<UIItemNewDay>();

        newDay.Box.LoadCDN
        (
            "CachedResource",
            $"AreaIllustration_{day - 1}.png",
            (Texture2D texture) =>
            {
                newDay.texture_2.alpha = 0.0f;
                newDay.tempTexture2 = new Texture2D(texture.width, texture.height, texture.format, false);
                Texture2DCopy(texture, newDay.tempTexture2);
                newDay.texture_2.mainTexture = newDay.tempTexture2;
            }
        );

        if (day > 2)
        {
            newDay.Box.LoadCDN
            (
                "CachedResource",
                $"AreaIllustration_{day - 2}.png",
                (Texture2D texture) =>
                {
                    newDay.tempTexture1 = new Texture2D(texture.width, texture.height, texture.format, false);
                    Texture2DCopy(texture, newDay.tempTexture1);
                    newDay.texture_1.mainTexture = newDay.tempTexture1;
                }
            );
        }
        else
        {
            newDay.texture_1.gameObject.SetActive(false);
        }

        newDay.script.text = Global._instance.GetString($"newday_tip_{day - 1}");

        return newDay;
    }

    public IEnumerator ImageIn()
    {
        float endTime_1 = animationCurve_1.keys[animationCurve_1.length - 1].time;
        float endTime_2 = animationCurve_2.keys[animationCurve_2.length - 1].time;
        float endTime_3 = animationCurve_3.keys[animationCurve_3.length - 1].time;

        float maxEndTime = Mathf.Max(endTime_1, endTime_2, endTime_3);

        float totalTime = 0.0f;

        Vector3 endScale = texture_2.transform.localScale;
        Vector3 startScale = Vector3.zero;

        float endAngle = texture_2.transform.localEulerAngles.z;
        float startAngle = 180.0f;

        while (totalTime < maxEndTime)
        {
            totalTime += Global.deltaTime;

            texture_2.transform.localEulerAngles = Vector3.forward * animationCurve_1.Evaluate(totalTime);
            texture_2.transform.localScale = Vector3.one * animationCurve_2.Evaluate(totalTime);
            texture_2.alpha = animationCurve_3.Evaluate(totalTime);

            yield return null;
        }
    }

    public IEnumerator ImageOut()
    {
        yield return new WaitForSeconds(1.0f);
    }

    private static void Texture2DCopy(Texture2D source, Texture2D destination)
    {
        Rect sourceRect = new Rect(0, 0, source.width, source.height);

        // if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
        {
            Color[] pixelBuffer = source.GetPixels((int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height);
            destination.SetPixels(pixelBuffer);
            destination.Apply();
        }
        // else
        // {
        //     Graphics.CopyTexture(source, 0, 0, (int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height, destination, 0, 0, 0, 0);
        // }
    }
}
