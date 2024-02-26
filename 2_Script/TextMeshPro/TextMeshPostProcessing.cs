using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TextMeshPostProcessing : MonoBehaviour
{
    public abstract void UpdateText();

#if UNITY_EDITOR
    private void Update()
    {
        UpdateText();
    }
#endif
}
