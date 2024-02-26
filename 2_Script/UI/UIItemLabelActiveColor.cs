using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLabelActiveColor : MonoBehaviour
{
    public enum ColorType
    {
        NORMAL,
        SHADOW,
    }

    [SerializeField] private Color[] activeColor;
    [SerializeField] private Color[] activeShadowColor;

    [SerializeField] private UILabel label;

    public Color SetLabel
    {
        get
        {
            return label.color;
        }
        set
        {
            label.color = value;
        }
    }
    public Color SetShadowLabel
    {
        get
        {
            return label.effectColor;
        }
        set
        {
            label.effectColor = value;
        }
    }

    public Color SetActiveColor(bool value, ColorType colorType)
    {
        Color color = new Color();

        if (colorType == ColorType.NORMAL)
            color = activeColor[value ? 0 : 1];
        else
            color = activeShadowColor[value ? 0 : 1];

        return color;
    }

}
