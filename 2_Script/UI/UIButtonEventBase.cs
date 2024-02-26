using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonEventBase : MonoBehaviour, IImageRequestable
{
    public UITexture buttonTexture;

    protected int index = -1;
    protected bool bCanTouch = true;

    public virtual void SetButtonEvent(int eventIndex)
    {
        index = eventIndex;
    }

    public virtual void OnLoadComplete(ImageRequestableResult r)
    {
        buttonTexture.mainTexture = r.texture;
        buttonTexture.MakePixelPerfect();
        buttonTexture.gameObject.SetActive(true);
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }

    public int GetIndex()
    {
        return index;
    }

    protected virtual void OnClickBtnEvent()
    {
        if (index == 0)
            return;
        //터치가능.
        if (this.bCanTouch == false)
            return;
    }

    protected void CanTouch()
    {
        this.bCanTouch = true;
    }
}
