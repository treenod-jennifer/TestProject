using System.Collections;
using UnityEngine;

public class UIItemDialogChat : UIDialog
{
    public UISprite _sprPhoto;

    public void InitDialogBubble(TypeChatBoxType boxType, string strChat, bool _bUser)
    {
        Vector3 dialogPos = new Vector3(10f, -10f, 0f);
        Vector3 labelPos = new Vector3(30f, -45f, 0f);

        _sprDialogBox.height = 110;
        _sprPhoto.gameObject.SetActive(false);

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
        _labelChat.text = strChat.Replace("[0]", myProfile.GameName);

        int _nLineCount = (int)(_labelChat.printedSize.y / _labelChat.fontSize);
        float _nLineCharCount = (_labelChat.printedSize.x / _labelChat.fontSize);

        //30 폰트 사이즈.
        float boxLength = 70 + (_nLineCharCount * 30);
        //30 폰트 사이즈 + 5 줄간격.
        float boxHeight = 75 + (_nLineCount * (30 + 5));

        _sprDialogBox.height = (int)boxHeight;
        _sprDialogBox.width = (int)boxLength;

        _sprDialogBox.spriteName = "chat_dialog_bubble_01";
        if (boxType == TypeChatBoxType.NormalRight)
        {
            _sprDialogBox.spriteName = "chat_dialog_bubble_02";
            _sprDialogBox.flip = UIBasicSprite.Flip.Horizontally;
            _sprDialogBox.pivot = UIWidget.Pivot.TopRight;
            _labelChat.pivot = UIWidget.Pivot.TopRight;
            dialogPos.x = (_sprDialogBox.width + 25f) * -1f;
            labelPos.x = -25f;
        }
        if (_bUser == true)
            _sprDialogBox.spriteName = "chat_dialog_bubble_03";

        transform.localPosition = dialogPos;
        _labelChat.transform.localPosition = labelPos;
        StartCoroutine(CoShow());
    }

    public IEnumerator CoShow()
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

    public void SetLabelAlignment(bool _bRemove = false, float fBoxSize = 1)
    {
        if (fBoxSize < 0)
        {
            _labelChat.transform.localScale = new Vector3(-1f, 1f, 1f);
            _labelChat.alignment = NGUIText.Alignment.Right;
        }

        if (_bRemove)
        {
            _labelChat.transform.localScale = new Vector3(1f, 1f, 1f);
            _labelChat.alignment = NGUIText.Alignment.Left;
        }
    }

    public void SetChatImg(string _strName)
    {
        _sprDialogBox.width = 494;
        _sprDialogBox.height = 200;
        _sprPhoto.spriteName = _strName;
        _sprPhoto.gameObject.SetActive(true);
    }
}
