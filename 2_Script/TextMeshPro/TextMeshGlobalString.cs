using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshGlobalString : MonoBehaviour
{
    [Header("TMP Text Object Path")]
    [SerializeField] private string tmpPath;

    [Header("GameObject Link Field")]
    [SerializeField] private UITexture texture;

    [Header("Text Field")]
    [SerializeField] private bool isStringKey = true;

    [TextArea(3, 10)]
    [SerializeField] private string text;

    private TMP_Text tmpText;

    private Coroutine coroutine;

    private bool isLoaded = false;

    private const float TIME_OUT = 5.0f;

    private const string MULTLINE_SEPARATOR = "<ml>";

    private const string MOVE_X_SEPARATOR = "<movex";

    private const string MOVE_Y_SEPARATOR = "<movey";

    private const char END_SEPARATOR = '>';

    private int _captureCount = 0; 
    
    private void OnEnable()
    {
        Load();
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(tmpPath))
        {
            return;
        }

        if(tmpText == null)
        {
            var tmp = Resources.Load(tmpPath) as GameObject;
            tmpText = Instantiate(tmp, transform).GetComponent<TMP_Text>();
            tmpText.rectTransform.sizeDelta = new Vector2(texture.width - 20, texture.height - 30);
        }

        if (gameObject.activeInHierarchy && !isLoaded && IsInitialized())
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            // TMP_Text의 캡처가 제대로 동작하지 않는 경우가 있어 현재 동작중인 캡쳐가 있을 경우 위치 조정을 하여 TMP_Text의 캡처가 겹쳐서 나오는 것을 방지.
            _captureCount++;
            if (_captureCount > 1)
            {
                texture.transform.localPosition += Vector3.right * (2000f * _captureCount);
                tmpText.transform.localPosition =  texture.transform.localPosition;
            }
            
            coroutine = StartCoroutine(LoadText(_captureCount));
        }
    }

    private IEnumerator LoadText(int captureCount)
    {
        StartCoroutine(FixingLayer(tmpText.gameObject, "RenderTextureObject"));

        string text;

        if (isStringKey)
        {
            if (!Global._instance.HasString(this.text))
            {
                float totalTime = 0.0f;

                yield return new WaitUntil(() =>
                {
                    totalTime += Global.deltaTime;
                    return Global._instance.HasString(this.text) || totalTime > TIME_OUT;
                });
            }

            text = Global._instance.GetString(this.text);
        }
        else
        {
            text = this.text;
        }

        text = PositionConversion(text);
        string[] multiLineText = MultiLineConversion(text);

        var tmps = tmpText.GetComponentsInChildren<TMP_Text>();

        for(int i = 0; i < tmps.Length; i++)
        {
            int multiLineTextindex = Mathf.Clamp(i, 0, multiLineText.Length - 1);
            tmps[i].text = multiLineText[multiLineTextindex];
        }

#if UNITY_EDITOR
        //유니티 에디터에서 인스펙터 창에서 해당 컴포넌트를 활성화 하여 이미지를 캡처하는 경우
        //Screen.height의 값이 인스펙터 창의 높이값을 출력하여 캡쳐에 문제가 발생함
        yield return null;
#endif

        Transform parent = transform.parent;
        Vector3 position = transform.localPosition;
        Vector3 angles = transform.localEulerAngles;
        Vector3 scale = transform.localScale;

        transform.parent = ManagerUI._instance.transform;
        transform.localScale = Vector3.one;

        tmpText.gameObject.SetActive(true);
        TextMeshCapturer.Instance.Capturer(tmpText, texture);

        transform.parent = parent;
        transform.localPosition = position;
        transform.localEulerAngles = angles;
        transform.localScale = scale;

        coroutine = null;
        isLoaded = true;
        
        if (captureCount > 1)
        {
            texture.transform.localPosition += Vector3.left * (2000f * captureCount);
        }
        _captureCount--;
        
        yield break;
    }

    private bool IsInitialized()
    {
        return
            tmpText != null &&
            texture != null &&
            !string.IsNullOrEmpty(text);
    }

    private string[] MultiLineConversion(string text)
    {
        string[] separatingStrings = { MULTLINE_SEPARATOR };
        return text.Split(separatingStrings, System.StringSplitOptions.None);
    }

    private string PositionConversion(string text)
    {
        var valueX = GetMoveAndTextRemoved(MOVE_X_SEPARATOR, text);

        transform.localPosition += Vector3.right * valueX.move;

        var valueY = GetMoveAndTextRemoved(MOVE_Y_SEPARATOR, valueX.text);

        transform.localPosition += Vector3.up * valueY.move;

        return valueY.text;

        (int move, string text) GetMoveAndTextRemoved(string separator, string target)
        {
            int startIndex = target.IndexOf($"{separator}");

            if (startIndex == -1)
            {
                return (0, target);
            }

            for (int i = startIndex + separator.Length; i < target.Length; i++)
            {
                if (target[i] == END_SEPARATOR)
                {
                    string tag = target.Substring(startIndex, i - startIndex + 1);

                    var pair = tag.Split('=');

                    if(pair != null && pair.Length == 2)
                    {
                        pair[1] = pair[1].Remove(pair[1].Length - 1);
                        pair[1] = pair[1].Replace(" ", string.Empty);

                        if(int.TryParse(pair[1], out int moveValue))
                        {
                            return (moveValue, target.Replace(tag, string.Empty));
                        }
                    }

                    break;
                }
            }

            return (0, target);
        }
    }

    private IEnumerator FixingLayer(GameObject target, string layerName)
    {
        while (target != null)
        {
            target.layer = LayerMask.NameToLayer(layerName);
            yield return null;
        }
    }

    /// <summary>
    /// text 필드가 비어있는 상태에서 사용해야 합니다. text가 비어 있지 않으면 해당 함수가 동작하기 전에 Load 처리가 되기 때문에 동작하지 않습니다.
    /// </summary>
    public void SetStringKey(string key)
    {
        isStringKey = true;
        text = key;
        Load();
    }

    /// <summary>
    /// text 필드가 비어있는 상태에서 사용해야 합니다. text가 비어 있지 않으면 해당 함수가 동작하기 전에 Load 처리가 되기 때문에 동작하지 않습니다.
    /// </summary>
    public void SetText(string text)
    {
        isStringKey = false;
        this.text = text;
        Load();
    }

    /// <summary>
    /// tmp에서 path를 변경할 때 사용하는 함수입니다. 반드시 해당 함수를 부른 이후에 SetStringKey, SetText를 불러줘야 합니다.
    /// </summary>
    /// <param name="path"></param>
    public void SetPath(string path)
    {
        tmpPath = path;
    }
}