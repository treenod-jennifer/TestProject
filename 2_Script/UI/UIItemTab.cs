using UnityEngine;
using UnityEngine.Events;

public class UIItemTab : MonoBehaviour
{
    [System.Serializable]
    private class UnityEvent_Bool : UnityEvent<bool> { }

    [SerializeField] private UIItemTab[] group;

    [SerializeField] private UIItemTabAnimation tabAnimation;
    [SerializeField] private UnityEvent OnEvent;
    [SerializeField] private UnityEvent OffEvent;
    [SerializeField] private UnityEvent_Bool ActiveEvent;

    private bool? isOn = null;
    private bool isActive = true;

    public void SetActive(bool isActive)
    {
        if(this.isActive != isActive)
        {
            this.isActive = isActive;
            tabAnimation?.ActiveAnimation(this.isActive);
            ActiveEvent?.Invoke(this.isActive);
        }
    }

    public void On()
    {
        if (!isActive)
        {
            return;
        }

        if (isOn != null && isOn.Value)
        {
            return;
        }

        tabAnimation?.OnAnimation();
        OnEvent?.Invoke();
        isOn = true;

        foreach (var tab in group)
        {
            tab.Off();
        }
    }

    private void Off()
    {
        if (!isActive)
        {
            return;
        }

        if (isOn != null && !isOn.Value)
        {
            return;
        }

        tabAnimation?.OffAnimation();
        OffEvent?.Invoke();
        isOn = false;
    }
}