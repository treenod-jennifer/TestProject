using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;

// 지원하는 텍스쳐의 형태

//              CDN    Web    R      S
//Texture2D     o      o      o      o
//apng          o                    o

public class UIUrlTexture : UITexture
{
    public AssetReference assetReference;
    private int textureWidth = 0;
    private int textureHeight = 0;

    private ResourceBox box;
    private ResourceBox Box 
    {
        get
        {
            if(box == null)
            {
                box = ResourceBox.Make(gameObject);
                box.IsUnLoad = false;
            }

            return box;
        }
    }

    public event Action SuccessEvent;
    public event Action FailEvent;

    public void LoadCDN(string localPath, string cdnPath, string fileName)
	{
        Box.LoadCDN(localPath, cdnPath, fileName, (APNGInfo apngInfo) => OnLoadComplete(apngInfo), true);
    }
    
    /// <summary>
    /// 로드 완료된 시점에 텍스쳐를 적용할건지에 대한 조건을 콜백으로 전달하여 사용하는 함수.
    /// </summary>
    public void LoadCDN(string localPath, string cdnPath, string fileName, Func<bool> conditionCallback)
    {
        Box.LoadCDN(localPath, cdnPath, fileName, (APNGInfo apngInfo) =>
        {
            if (conditionCallback())
            {
                OnLoadComplete(apngInfo);
            }
        }, true);
    }

    public void LoadWeb(string url)
    {
        Box.LoadWeb(url, (Texture2D texture) => OnLoadComplete(texture), true);
    }

    public void LoadResource(string path)
    {
        Box.LoadResource(path, (Texture2D texture) => OnLoadComplete(texture), true);
    }

    public void LoadStreaming(string path)
    {
        Box.LoadStreaming(path, (APNGInfo apngInfo) => OnLoadComplete(apngInfo), true);
    }

    public void SettingTextureScale(int scaleX, int scaleY)
    {
        textureWidth = scaleX;
        textureHeight = scaleY;
    }

    private void OnLoadComplete(APNGInfo apngInfo)
    {
        if (apngInfo == null)
        {
            FailEvent?.Invoke();

            SuccessEvent = null;
            FailEvent = null;
            return;
        }

        APNGPlayer.Play(this, apngInfo);

        //사이즈.
        if (textureWidth > 0 || textureHeight > 0)
        {
            width = textureWidth;
            height = textureHeight;
        }
        else
        {
            MakePixelPerfect();
        }

        SuccessEvent?.Invoke();

        SuccessEvent = null;
        FailEvent = null;
    }

    private void OnLoadComplete(Texture2D texture)
    {
        if (texture == null)
        {
            FailEvent?.Invoke();

            SuccessEvent = null;
            FailEvent = null;
            return;
        }

        APNGPlayer.Stop(this);

        mainTexture = texture;

        //사이즈.
        if (textureWidth > 0 || textureHeight > 0)
        {
            width = textureWidth;
            height = textureHeight;
        }
        else
        {
            MakePixelPerfect();
        }

        SuccessEvent?.Invoke();

        SuccessEvent = null;
        FailEvent = null;
    }
}