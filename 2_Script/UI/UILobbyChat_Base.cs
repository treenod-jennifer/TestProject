using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eTailDirection
{
    none,
    right,
    left,
    up,
    down,
}

public class UILobbyChat_Base : MonoBehaviour, UILabelBubble.SpeechBubble
{
    public UISprite ChatBubbleTail;
    public UISprite ChatBubbleBox;
    public UILabel LobbyChat;
    public eTailDirection eDir;
    public AnimationCurve lobbyChatAnimationCurve;

    protected bool showOutScreen = false;
    protected Vector3 nguiPos = Vector3.zero;

    [System.NonSerialized]
    public Transform targetObject = null;
    public float heightOffset = 0.0f;

    [System.NonSerialized]
    public float _dulation = 1f;
    protected const float Animation_Speed_Ratio = 5;
    
    static public List<UILobbyChat_Base> _chatList = new List<UILobbyChat_Base>();
    static public void RemoveAll()
    {
        var tmpChatList = new List<UILobbyChat_Base>(_chatList);
        for (int i = 0; i < tmpChatList.Count; i++)
        {
            tmpChatList[i].targetObject = null;
            DestroyImmediate(tmpChatList[i].gameObject);
        }
        _chatList.Clear();
    }

    public virtual void SetDepthOffset(int offset)
    {
    }
    
    public static void ArrangeDepth()
    {
        for (int i = 0; i < _chatList.Count; i++)
        {
            _chatList[i].SetDepthOffset(i * 3);
        }
    }

    void Awake()
    {
        _chatList.Add(this);
        ArrangeDepth();
    }

    private void OnDestroy()
    {
        _chatList.Remove(this);
        ArrangeDepth();
    }

    protected virtual IEnumerator Start()
    {
        eDir = eTailDirection.none;
        SetBoxSize();

        float animationTimer = 0;
        float ratio;
        while (animationTimer < 1)
        {
            ratio = lobbyChatAnimationCurve.Evaluate(animationTimer);
            transform.localScale = Vector3.one * ratio;

            ChatBubbleTail.alpha = ratio;
            ChatBubbleBox.alpha = ratio;
            LobbyChat.alpha = ratio;

            animationTimer += Global.deltaTimeLobby * Animation_Speed_Ratio;
            yield return null;
        }
        ChatBubbleTail.alpha = 1f;
        ChatBubbleBox.alpha = 1f;
        LobbyChat.alpha = 1f;
        transform.localScale = Vector3.one;

        //일정시간 기다렸다가 말풍선 제거.
        StartCoroutine(CoShowTimer());
    }

    protected virtual IEnumerator CoShowTimer()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _dulation)
        {
            elapsedTime += Global.deltaTimeLobby;
            yield return null;
        }
        StartCoroutine(CoHide());
    }

    protected virtual IEnumerator CoHide()
    {
        float animationTimer = 1;
        float ratio;
        while (animationTimer > 0)
        {
            ratio = lobbyChatAnimationCurve.Evaluate(animationTimer);
            transform.localScale = Vector3.one * ratio;

            ChatBubbleTail.alpha = ratio;
            ChatBubbleBox.alpha = ratio;
            LobbyChat.alpha = ratio;

            animationTimer -= Global.deltaTimeLobby * 8f;
            yield return null;
        }
        Destroy(gameObject);
    }

    //입력된 글자 수와 라인 수를 읽어 말풍선 크기 세팅.
    protected virtual void SetBoxSize()
    {
        int _nLineCharCount = (int)(LobbyChat.printedSize.x / LobbyChat.fontSize);
        int _nLineCount = (int)(LobbyChat.printedSize.y / LobbyChat.fontSize);

        //Heigth의 25는 폰트 사이즈.
        float boxLength = 60 + (_nLineCharCount * 25);
        float boxHeight = 40 + (_nLineCount * 25);

        ChatBubbleBox.width = (int)boxLength;
        ChatBubbleBox.height = (int)boxHeight;
    }

    protected virtual void LateUpdate()
    {
        if (targetObject == null)
            return;

        Vector3 pos = NGUIMath.WorldToLocalPoint(targetObject.position + Vector3.up * (heightOffset + 5f), Camera.main, ManagerUI._instance._camera, transform);
        pos.z = 0.0f;
        transform.localPosition = pos;

        if (showOutScreen == true)
        {
            LobbyChatMoveToScreen(ManagerUI._instance.uiScreenSize);
            TailMoveToScreen(ManagerUI._instance.uiScreenSize);
        }
    }

    protected virtual float GetUIOffSet()
    {
        float ratio = 120 - (Camera.main.fieldOfView - 10.4f) * 80 / 12f;

        return ratio;
    }

    #region 화면 범위 관련.

    //화면 범위 넘어갔을 때 말풍선 위치 설정.
    private void LobbyChatMoveToScreen(Vector2 uiScreen)
    {
        float _foffsetX = (ChatBubbleBox.width * 0.5f) + 10f;
        float _foffsetY = ChatBubbleBox.height + 30f;

        if (transform.localPosition.y < (uiScreen.y - 135) * -1)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (uiScreen.y - 135) * -1, transform.localPosition.z);
        }
        else if (transform.localPosition.y > (uiScreen.y - _foffsetY - 135))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (uiScreen.y - _foffsetY - 135), transform.localPosition.z);
        }
        if (transform.localPosition.x < (uiScreen.x - _foffsetX) * -1)
        {
            transform.localPosition = new Vector3((uiScreen.x - _foffsetX) * -1f, transform.localPosition.y, transform.localPosition.z);
        }
        else if (transform.localPosition.x > (uiScreen.x - _foffsetX))
        {
            transform.localPosition = new Vector3((uiScreen.x - _foffsetX), transform.localPosition.y, transform.localPosition.z);
        }
    }

    //화면 범위 넘어갔을 때 말풍선 꼬리 모양/위치 설정.
    private void TailMoveToScreen(Vector2 uiScreen)
    {
        bool _bDir = false;
        if (nguiPos.y > (uiScreen.y - 143f))
        {
            _bDir = true;
            if (eDir != eTailDirection.up)
                SetTailPosition(eTailDirection.up);
        }
        else if (nguiPos.y < (uiScreen.y - 143f))
        {
            _bDir = true;
            if (eDir != eTailDirection.down)
                SetTailPosition(eTailDirection.down);
        }

        if (nguiPos.x < (uiScreen.x) * -1)
        {
            _bDir = true;
            if (eDir != eTailDirection.right)
                SetTailPosition(eTailDirection.right);
        }
        else if (nguiPos.x > uiScreen.x)
        {
            _bDir = true;
            if (eDir != eTailDirection.left)
                SetTailPosition(eTailDirection.left);
        }

        if (_bDir == false)
        {
            if (eDir != eTailDirection.none)
                SetTailPosition(eTailDirection.none);
        }
    }

    //말풍선 꼬리 위치 설정.
    private void SetTailPosition(eTailDirection _dir)
    {
        float x = 0f;
        float y = 0f;
        switch (_dir)
        {
            case eTailDirection.right:
                ChatBubbleTail.spriteName = "lobby_bubble_tail_02";
                ChatBubbleTail.flip = UIBasicSprite.Flip.Horizontally;
                x = (ChatBubbleBox.width * -0.5f) - 3f;
                y = (ChatBubbleBox.height * 0.5f) - 14f;
                break;
            case eTailDirection.left:
                ChatBubbleTail.spriteName = "lobby_bubble_tail_02";
                ChatBubbleTail.flip = UIBasicSprite.Flip.Nothing;
                x = (ChatBubbleBox.width * 0.5f) + 3f;
                y = (ChatBubbleBox.height * 0.5f) - 14f;
                break;
            case eTailDirection.up:
                ChatBubbleTail.spriteName = "lobby_bubble_tail_01";
                ChatBubbleTail.flip = UIBasicSprite.Flip.Nothing;
                y = ChatBubbleBox.height - 2f;
                break;
            case eTailDirection.down:
            case eTailDirection.none:
                ChatBubbleTail.spriteName = "lobby_bubble_tail_03";
                ChatBubbleTail.flip = UIBasicSprite.Flip.Nothing;
                y = -11f;
                break;
        }
        eDir = _dir;
        ChatBubbleTail.MakePixelPerfect();
        ChatBubbleTail.transform.localPosition = new Vector3(x, y, 0);
    }
    #endregion

    public void BubbleTail_Off()
    {
        SetBoxSize();

        ChatBubbleBox.color = new Color(199.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f);
        ChatBubbleTail.gameObject.SetActive(false);
    }
}
