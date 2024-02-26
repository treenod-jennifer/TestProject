using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDefaultTitle : MonoBehaviour
{
    [SerializeField] private UIUrlTexture title;

    private const string TITLE_PATH = "Title/title.png";

    private void Start () 
    {
        string path = LanguageUtility.FileNameConversion(TITLE_PATH);
        title.LoadStreaming(path);
    }
}
