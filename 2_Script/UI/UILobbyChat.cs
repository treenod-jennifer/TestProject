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

public class UILobbyChat : MonoBehaviour
{
    public UISprite ChatBubbleTail;
    public UISprite ChatBubbleBox;
    public UILabel LobbyChat;
    public eTailDirection eDir;
    public AnimationCurve lobbyChatAnimationCurve;
    
    private bool showOutScreen = false;
    private Vector3 nguiPos = Vector3.zero;
    
    [System.NonSerialized]
    public Transform targetObject = null;
    public float heightOffset = 0.0f;

    [System.NonSerialized]
    public float _dulation = 1f;
    private const float Animation_Speed_Ratio = 5;


    static public List<UILobbyChat> _chatList = new List<UILobbyChat>();
    static public void RemoveAll()
    {
        for (int i = 0; i < _chatList.Count; i++)
        {
            _chatList[i].targetObject = null;
            DestroyImmediate(_chatList[i].gameObject);
        }
    }
    static public UILobbyChat MakeLobbyChat(Transform in_obj, string in_strChat,float in_dulation = 1f, bool in_show = false)
    {
        if (SceneLoading._sceneLoading)
            return null;

        ManagerSound.AudioPlay(AudioLobby.Button_02);
        UILobbyChat lobbyChat = NGUITools.AddChild(ManagerUI._instance.gameObject, ManagerUI._instance._objLobbyChat).GetComponent<UILobbyChat>();
        lobbyChat.targetObject = in_obj;
        lobbyChat.LobbyChat.text = in_strChat;
        lobbyChat.LobbyChat.text = in_strChat.Replace("[0]", ManagerData._instance.userData.name);
        lobbyChat._dulation = in_dulation;
        lobbyChat.showOutScreen = in_show;
        return lobbyChat;
    }
    void Awake()
    {
        _chatList.Add(this);
    }

    private void OnDestroy()
    {
        _chatList.Remove(this);
    }
    IEnumerator Start()
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

            animationTimer += Global.deltaTime * Animation_Speed_Ratio;
            yield return null;
        }
        ChatBubbleTail.alpha = 1f;
        ChatBubbleBox.alpha = 1f;
        LobbyChat.alpha = 1f;

        transform.localScale = Vector3.one;

        float elapsedTime = 0f;
        while( elapsedTime < _dulation)
        {
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(CoHide());
    }
    // 기준위치 설정 / 대화설정 /화면 영역 벗어났을 때 화면 끝에서 보이게 할지 설정.
    // (true - 보임, false - 영역밖으로 나감).
    // tr은 기준이 되는 오브젝트의 position을 넣어야 함(월드 좌표계).

    public IEnumerator ShowLobbyChat(Transform obj, string strChat, bool show = false)
    {
        LobbyChat.text = strChat;
        targetObject = obj;
        showOutScreen = show;
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

            animationTimer += Global.deltaTime * Animation_Speed_Ratio;
            yield return null;
        }
        ChatBubbleTail.alpha = 1f;
        ChatBubbleBox.alpha = 1f;
        LobbyChat.alpha = 1f;

        transform.localScale = Vector3.one;

        float elapsedTime = 0f;
        while (elapsedTime < 1.0f)
        {
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(CoHide());
    }

    private IEnumerator CoHide()
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

            animationTimer -= Global.deltaTime * 8f;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (targetObject == null)
            return;

        Vector3 posVec = targetObject.position;
        posVec.y += heightOffset;

        nguiPos = PokoMath.ChangeTouchPosNGUI(Camera.main.WorldToScreenPoint(posVec));
        transform.localPosition = nguiPos + Vector3.up * GetUIOffSet();
        if (showOutScreen == true)
        {
            LobbyChatMoveToScreen(ManagerUI._instance.uiScreenSize);
            TailMoveToScreen(ManagerUI._instance.uiScreenSize);
        }
    }

    //입력된 글자 수와 라인 수를 읽어 말풍선 크기 세팅.
    private void SetBoxSize()
    {
        int _nLineCharCount = (int)(LobbyChat.printedSize.x / LobbyChat.fontSize);
        int _nLineCount = (int)(LobbyChat.printedSize.y / LobbyChat.fontSize);

        //Heigth의 25는 폰트 사이즈.
        float boxLength = 60 + (_nLineCharCount * 25);
        float boxHeight = 40 + (_nLineCount * 25);

        ChatBubbleBox.width = (int)boxLength;
        ChatBubbleBox.height = (int)boxHeight;
    }

    private float GetUIOffSet()
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
}
