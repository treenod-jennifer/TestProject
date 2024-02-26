using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILabelBubble : UILabel
{
    public interface SpeechBubble
    {
        void BubbleTail_Off();
    }

    [SerializeField] private Object receiver;
    private SpeechBubble speechBubble;

    protected override void Awake()
    {
        base.Awake();
        speechBubble = receiver as SpeechBubble;

        Detector_Tail();
    }

    public override void MarkAsChanged()
    {
        Detector_Tail();
        base.MarkAsChanged();
    }

    private const string TAIL_CHECK = "[NO_TAIL]";

    private void Detector_Tail()
    {
        if (speechBubble == null) return;

        if (text.Contains(TAIL_CHECK))
        {
            text = text.Replace(TAIL_CHECK, "");
            speechBubble.BubbleTail_Off();
        }
    }
}
