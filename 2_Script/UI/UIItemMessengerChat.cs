using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemMessengerChat : MonoBehaviour
{
    public UISprite charIcon;
    public UISprite charBubble;
    public UILabel chatText;
    public UILabel time;

    private const float iconXPos = 211f;
    private const float bubbleXPos = 208f;
    private const float chatXPos = -24f;

    private Vector2 timePos = Vector2.zero;

    public void UpdateData(System.DateTime startTime, MessengerChatInfo chatInfo, bool bShowIcon = true)
    {
        //if (bShowIcon == true)
            charIcon.spriteName = "messenger_ch_" + chatInfo.strCharacter;
        //else
        //    charIcon.enabled = false;

        chatText.text = chatInfo._strChat;
        time.text = startTime.AddMinutes(chatInfo._nMinute).ToString("HH:mm");

        GetBoxSize();
        timePos.x = charBubble.width + 8;
        timePos.y = (charBubble.height - 10) * -1;

        //현재 대화 하는 캐릭터가 보니인 경우 위치 세팅.
        if (chatInfo.strCharacter == "boni")
        {
            //if (bShowIcon == true)
            //{
                charIcon.transform.localPosition = new Vector3(iconXPos, charIcon.transform.localPosition.y, 0);
            //}
            charBubble.pivot = UIWidget.Pivot.TopRight;
            chatText.pivot = UIWidget.Pivot.TopRight;
            time.pivot = UIWidget.Pivot.BottomRight;

            charBubble.transform.localPosition = new Vector3(bubbleXPos, charBubble.transform.localPosition.y, 0);
            chatText.transform.localPosition = new Vector3(chatXPos, chatText.transform.localPosition.y, 0);
            time.transform.localPosition = new Vector3(-timePos.x, timePos.y, 0);

            chatText.alignment = NGUIText.Alignment.Left;
            charBubble.flip = UIBasicSprite.Flip.Horizontally;
        }

        else
        {
            time.transform.localPosition = new Vector3(timePos.x, timePos.y, 0);
        }
    }

    public float GetNextPos()
    {
        int height = charBubble.height;
        if (charBubble.height < charIcon.height)
            height = charIcon.height;
        return transform.localPosition.y - (height + 10);
    }

    void GetBoxSize()
    {
        int _nLineCharCount = (int)(chatText.printedSize.x / chatText.fontSize);
        int _nLineCount = (int)(chatText.printedSize.y / chatText.fontSize);

        float boxLength = 56 + (_nLineCharCount * 25);
        float boxHeight = 40 + (_nLineCount * 25);

        charBubble.width = (int)boxLength;
        charBubble.height = (int)boxHeight;
    }
}
