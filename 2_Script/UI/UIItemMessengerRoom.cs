using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemMessengerRoom : MonoBehaviour
{
    public UISprite _sprCharIcon;
    public UISprite _sprNewIcon;
    public UILabel _labelCharName;
    public UILabel _labelCharChat;
    public UILabel _labelLastTime;

    [HideInInspector]
    public MessengerCharData _charData = null;

    public void InitBtnMessenger(MessengerCharData _data)
    {
        _charData = _data;
        _sprCharIcon.spriteName = "messenger_ch_" + _charData.roomName;
        _sprCharIcon.MakePixelPerfect();
        _labelCharName.text = _charData.roomName;
        _labelCharChat.text = _charData.lastMessage;

        //마지막 글자 너무 길면 "....." 으로 대체;
        int _nLineCount = (int)(_labelCharChat.printedSize.y / _labelCharChat.fontSize);
        if (_nLineCount > 2)
            _labelCharChat.text = _labelCharChat.text.Replace(_labelCharChat.text.Substring(15),".....");

        _labelLastTime.text = _charData.lastTime;
        if (_charData.bNew == true)
            _sprNewIcon.enabled = true;
        else
            _sprNewIcon.enabled = false;
    }

    void OnClickBtnMessenger()
    {
        UIDiaryMessenger.instance.OnClickBtnMessage(true, _charData);
        if (_charData.bNew == true)
        {
            //실제 데이터를 변경시켜줘야 함(아니면 껐다키면 다시 new 버튼 나옴).
            _charData.bNew = false;
            _sprNewIcon.enabled = false;
        }
    }
}
