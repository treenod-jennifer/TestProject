using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextMeshCapturer : MonoBehaviour
{
    [SerializeField] private Camera captureCamera;

    private const string GAMEOBJECT_PATH = "Utility/TextMeshCapturer";

    private static TextMeshCapturer instance = null;
    public static TextMeshCapturer Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = Instantiate(Resources.Load(GAMEOBJECT_PATH)) as GameObject;
                instance = obj.GetComponent<TextMeshCapturer>();

                obj.hideFlags = HideFlags.HideInHierarchy;
            }

            return instance;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Capturer(TMP_Text text, UITexture texture, bool autoTextDelete = true)
    {
        captureCamera.enabled = true;

        //텍스트 메시 후처리 업데이트
        var postProcess = text.GetComponentsInChildren<TextMeshPostProcessing>();
        if(postProcess != null)
        {
            foreach (var item in postProcess)
            {
                item.UpdateText();
            }
        }

        //카메라 세팅 (위치)
        Vector3 pos = captureCamera.transform.position;
        pos.x = texture.transform.position.x;
        pos.y = texture.transform.position.y;
        captureCamera.transform.position = pos;

        //카메라 세팅 (크기)
        float screenHeight = UIRoot.GetPixelSizeAdjustment(texture.gameObject) * Screen.height;
        captureCamera.orthographicSize = texture.height / screenHeight;

        //텍스쳐 세팅
        RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 24, RenderTextureFormat.ARGB32);
        renderTexture.useMipMap = false;
        //renderTexture.filterMode = FilterMode.Point;
        //renderTexture.antiAliasing = 8;

        //캡쳐 및 UITexture 세팅
        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();
        texture.shader = Shader.Find($"{texture.shader.name} CaptureOnly");
        texture.mainTexture = CreateTexture2D(renderTexture);
        
        //사용후 리소스 해제
        captureCamera.targetTexture = null;
        renderTexture.Release();
        Destroy(renderTexture);
        AutoDestroyer.SetDestroy(texture.gameObject, texture.mainTexture);

        //원본 자동 삭제
        //Dynamic 형태의 폰트는 폰트 아틀라스까지 초기화 해준다
        if (autoTextDelete)
        {
            FontAtlasClear(text);
            Destroy(text.gameObject);
        }

        captureCamera.enabled = false;



        Texture2D CreateTexture2D(RenderTexture rTexture)
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rTexture;

            Texture2D texture2D = new Texture2D (rTexture.width, rTexture.height, TextureFormat.RGBA32, false);
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentActiveRT;

            return texture2D;
        }
    }

    #region 폰트를 사용하고 아틀라스를 초기화 처리 하는 부분 (초기화는 캡쳐를 하고 다음 프레임에서 처리한다. - 이전 프레임에서 사용한 폰트를 종합하여 한번에 처리)

    private void FontAtlasClear(TMP_Text text)
    {
        fontAssets.Add(text.font);

        var subAsset = text.GetComponentsInChildren<TMP_SubMesh>();

        if(subAsset != null)
        {
            foreach(var asset in subAsset)
            {
                fontAssets.Add(asset.fontAsset);
            }
        }

        if(clearCoroutine == null)
        {
            clearCoroutine = StartCoroutine(FontAssetDataClear());
        }
    }

    private Coroutine clearCoroutine = null;

    private HashSet<TMP_FontAsset> fontAssets = new HashSet<TMP_FontAsset>();

    private IEnumerator FontAssetDataClear()
    {
        yield return null;

        foreach(var font in fontAssets)
        {
            if (font.atlasPopulationMode == AtlasPopulationMode.Dynamic)
            {
                font.ClearFontAssetData();
            }
        }

        fontAssets.Clear();
        clearCoroutine = null;
    }
    
    #endregion
}
