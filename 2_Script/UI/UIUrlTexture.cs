using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class UIUrlTexture : UITexture, IImageRequestable
{
    #region Private
    private Texture texture;
    private string path = "";
    private string fileName = "";
	private LoadingContext _loadingContext;
    private int textureWidth = 0;
    private int textureHeight = 0;

    //이미지 전송 다 받은 후 콜백.
    private Method.FunctionVoid callBack = null;
    #endregion

    protected override void OnStart()
	{
		base.OnStart();

#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			mainTexture = texture;
			return;
		}
#endif
		texture = mainTexture;
	}

    public void LoadWithSprite(string localPath, string cdnPath, string fileName)
    {
        this.path = cdnPath;
        this.fileName = fileName;

        if (!string.IsNullOrEmpty(cdnPath) && !string.IsNullOrEmpty(fileName))
        {
            _loadingContext = UIImageLoader.Instance.LoadWithSprite(localPath, cdnPath, fileName, this);
        }
        else
        {
            OnLoadComplete(null);
        }
    }

    public void Load(string localPath, string cdnPath, string fileName)
	{
        this.path = cdnPath;
        this.fileName = fileName;

        if (!string.IsNullOrEmpty(cdnPath) && !string.IsNullOrEmpty(fileName))
        {
	        _loadingContext = UIImageLoader.Instance.Load(localPath, cdnPath, fileName, this);
        }
        else
        {
            OnLoadComplete(null);
        }
	}

    public void ForceLoad(string localPath, string cdnPath, string fileName )
    {
        this.path = cdnPath;
        this.fileName = fileName;

        if (!string.IsNullOrEmpty(cdnPath) && !string.IsNullOrEmpty(fileName))
        {
            _loadingContext = UIImageLoader.Instance.ForceLoad(localPath, cdnPath, fileName, this);
        }
        else
        {
            OnLoadComplete(null);
        }
    }

    public void SettingTextureScale(int scaleX, int scaleY)
    {
        textureWidth = scaleX;
        textureHeight = scaleY;
    }

    public void SettingCallBack(Method.FunctionVoid func)
    {
        callBack = func;
    }
    
    void OnDestroy()
    {
        if (!UIImageLoader.IsNull())
        {
	        UIImageLoader.Instance.CancelAndUnload(_loadingContext);
	        
	        mainTexture = texture;
        }
	}

    #region IImageRequestable
    public void OnLoadComplete(ImageRequestableResult r)
    {
        if (r == null)
            return;
        r.texture.wrapMode = TextureWrapMode.Clamp;
        mainTexture = r.texture;

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

        //콜백.
        if (callBack != null)
        {
            callBack();
        }

        if (r.spriteDesc != null)
        {
            SettingMove(r);
        }
    }

    public void OnLoadFailed() { }

    public int GetWidth()
	{
		return width;
	}

	public int GetHeight()
	{
		return height;
	}
    #endregion

    private void SettingMove(ImageRequestableResult r)
    {
        type = Type.Tiled;
        width = r.spriteDesc.size[0];
        height = r.spriteDesc.size[1];

        float x = 1.0f / r.spriteDesc.count;

        List<float> listIntervals = new List<float>();
        for (int i = 0; i < r.spriteDesc.intervals.Count; i++)
        {
            float interval = r.spriteDesc.intervals[i] * 0.001f;
            listIntervals.Add(interval);
        }
        StartCoroutine(CoActionSprite(r.spriteDesc.count, x, listIntervals));
    }

    private IEnumerator CoActionSprite(int imageCount, float xStep, List<float> inteval)
    {
        int index = 0;
        while (gameObject.activeInHierarchy == true)
        {
            uvRect = new Rect((index * xStep), 0, 1, 1);
            index++;
            if (index == imageCount)
                index = 0;
            yield return new WaitForSeconds(inteval[index]);
        }
        yield return null;
    }
}
