using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerFont : MonoBehaviour {
    public static ManagerFont _instance = null;

    [SerializeField]
    private Font[] fonts;

    [SerializeField]
    private NGUIFont[] uiFonts;

    private void Awake()
    {
        _instance = this;
    }

    public Font GetFont(string fontName)
    {
        for (int i = 0; i < fonts.Length; i++)
        {
            if(fonts[i].name == fontName)
            {
                return fonts[i];
            }
        }
        return null;
    }

    public NGUIFont GetUIfont(string fontName)
    {
        for (int i = 0; i < uiFonts.Length; i++)
        {
            if (uiFonts[i].name == fontName)
            {
                return uiFonts[i];
            }
        }
        return null;
    }
}
