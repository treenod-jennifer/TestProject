using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public static void SetActive(this GameObject obj, bool state, bool recursive)
    {
        obj.SetActive(state);
        if (recursive )
        {
            foreach (Transform child in obj.transform)
            {
                SetActive(child.gameObject, state, true);
            }
        }
    }

    public static void SetEnable(this Collider[] objList, bool state)
    {
        foreach (var child in objList)
        {
            child.enabled = state;
        }
    }
}
