using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDialogSound : UIDialog
{
    public GameObject audioSprite;

    public void InitDialogBubble(string strChat)
    {
        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        _labelChat.text = strChat.Replace("[0]", myProfile.GameName);

        int _nLineCharCount = (int)(_labelChat.printedSize.x / _labelChat.fontSize);
        //30 폰트 사이즈.
        float boxLength = 120 + (_nLineCharCount * 30);
        _sprDialogBox.width = (int)boxLength;
        SettingSoundIconPosition(_nLineCharCount);
        StartCoroutine(CoShow());
        StartCoroutine(CoIconAction());
    }

    private void SettingSoundIconPosition(int lineCount)
    {
        float xPos = -18f - (15f * lineCount);
        audioSprite.transform.localPosition = new Vector3(xPos, audioSprite.transform.localPosition.y, 0);
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

    private IEnumerator CoIconAction()
    {
        while (gameObject.activeInHierarchy == true)
        {
            yield return new WaitForSeconds(0.5f);
            audioSprite.SetActive(!audioSprite.activeInHierarchy);
        }
    }
}
