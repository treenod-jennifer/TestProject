using UnityEngine;

public abstract class UIItemTabAnimation : MonoBehaviour
{
    public abstract void OnAnimation();

    public abstract void OffAnimation();

    public virtual void ActiveAnimation(bool isActive) { }
}
