using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonEventBase : MonoBehaviour
{
    public UIUrlTexture buttonTexture;

    protected int index = -1;
    protected bool bCanTouch = true;

    public virtual void SetButtonEvent(int eventIndex)
    {
        index = eventIndex;
    }

    public virtual void OnLoadComplete()
    {
        buttonTexture.MakePixelPerfect();
        buttonTexture.gameObject.SetActive(true);
    }

    public virtual void InitEventTime(long endTs)
    {
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

    virtual public float GetButtonSize()
    {
        // 버튼사이즈 : 절대값으로 넣어주면 UILobbListManager 에서 대충 부호 처리해서 씀
        return 120f;
    }
}
