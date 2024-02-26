using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDialogMonologue : UIDialog
{
    public void InitDialogBubble(string text)
    {
        _labelChat.text = text;

        float boxHeight = _labelChat.printedSize.y + 70.0f;
        float boxWidth = _labelChat.printedSize.x + 100.0f;

        _sprDialogBox.height = (int)boxHeight;
        _sprDialogBox.width = (int)boxWidth;

        StartCoroutine(CoShow());
    }

    private IEnumerator CoShow()
    {
        float ElapsedTime = 0.0f;
        float TotalTime = 0.2f;
        while (ElapsedTime < TotalTime)
        {
            ElapsedTime += Time.deltaTime;
            float scaleX = Mathf.Lerp(0, 1, ElapsedTime / TotalTime);
            _sprDialogBox.cachedTransform.localScale = new Vector3(scaleX, 1, 1);
            yield return null;
        }
    }
}
