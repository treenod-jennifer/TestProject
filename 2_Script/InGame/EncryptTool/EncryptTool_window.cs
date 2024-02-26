using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncryptTool_window : MonoBehaviour 
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 40, 200, 200), "맵변환"))
        {
            StageHelper.StageEncrypt("Stage/");
        }        
    }
}
