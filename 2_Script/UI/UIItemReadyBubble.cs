using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemReadyBubble : MonoBehaviour, UILabelBubble.SpeechBubble
{
    [SerializeField] private UIBasicSprite tailBubble;
    [SerializeField] private UIBasicSprite noTailBubble;
    [SerializeField] private UILabel label;
    private int LineHeight = 12;
    public string text
    {
        set
        {
            label.text = value;
            SetBubbleHeight();
        }
        get
        {
            return label.text;
        }
    }

    private bool ActiveTail
    {
        get
        {
            return tailBubble.gameObject.activeSelf;
        }
        set
        {
            tailBubble.gameObject.SetActive(value);
            noTailBubble.gameObject.SetActive(!value);
        }
    }
    private int defaultTailBubbleHeight = -1;
    private int DefaultTailBubbleHeight
    {
        get
        {
            if(defaultTailBubbleHeight == -1)
            {
                defaultTailBubbleHeight = tailBubble.height;
            }

            return defaultTailBubbleHeight;
        }
    }

    private int defaultNoTailBubbleHeight = -1;
    private int DefaultNoTailBubbleHeight
    {
        get
        {
            if (defaultNoTailBubbleHeight == -1)
            {
                defaultNoTailBubbleHeight = noTailBubble.height;
            }

            return defaultNoTailBubbleHeight;
        }
    }

    private void SetBubbleHeight()
    {
        int lineCount = (int)(label.printedSize.y / label.fontSize);

        if (ActiveTail)
        {
            tailBubble.height = DefaultTailBubbleHeight + (lineCount - 1) * LineHeight;
        }
        else
        {
            noTailBubble.height = DefaultNoTailBubbleHeight + (lineCount - 1) * LineHeight;
        }
    }

    public int GetTextLineCount()
    { 
        if (label != null)
            return (int)(label.printedSize.y / label.fontSize);
        return -1;
    }

    public void BubbleTail_Off()
    {
        ActiveTail = false;
        SetBubbleHeight();
    }
}
