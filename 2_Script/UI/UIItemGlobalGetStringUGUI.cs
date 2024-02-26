using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemGlobalGetStringUGUI : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private string textKeyValue;

    private void Start()
    {
        text.text = Global._instance.GetString(textKeyValue);
    }
}
