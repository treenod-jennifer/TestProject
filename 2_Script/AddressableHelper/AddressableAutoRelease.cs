using System;
using UnityEngine;
using UnityEngine.Events;

public class AddressableAutoRelease : MonoBehaviour
{
    [NonSerialized] public UnityEvent OnRelease = new UnityEvent();

    private void OnDestroy()
    {
        OnRelease.Invoke();
        OnRelease = null;
    }
}
