using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerGlobal : MonoBehaviour
{

    public static ManagerGlobal _instance = null;


    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;
    }
}
