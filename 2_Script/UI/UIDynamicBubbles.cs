using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDynamicBubbles : MonoBehaviour, UILabelBubble.SpeechBubble
{
    [Header("Linked Object")]
    [SerializeField] private UILabel label;
    [SerializeField] private UISprite speechBubbles;
    [SerializeField] private GameObject tail;

    [Header("Effect")]
    [SerializeField] private int intervalHeight = 20;
    [SerializeField][Tooltip("'0' is unlimited")]
    private int limitHeight = 0;

    public void SetBubble(string text)
    {
        label.text = text;

        Vector2 labelSize = label.printedSize;

        speechBubbles.width = Mathf.RoundToInt(labelSize.x + 40);
        speechBubbles.height = Mathf.RoundToInt(labelSize.y + 35);
    }

    public void SetBubble(string text, UIItemAdventureAnimalInfo target)
    {
        SetBubble(text);
        target.fullLoadedEvent += (texture) => 
        {
            transform.localPosition = Vector3.up *
            Mathf.Clamp
            (
                texture.GetHeight() + intervalHeight,
                0.0f,
                (limitHeight != 0) ? limitHeight - speechBubbles.height : Mathf.Infinity
            );
        };
    }

    public void BubbleTail_Off()
    {
        SetBubble(label.text);

        speechBubbles.color = new Color(199.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f);
        tail.SetActive(false);
    }
}
