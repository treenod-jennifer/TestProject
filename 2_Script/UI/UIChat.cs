using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[System.Serializable]
public enum CHAT_SIDE
{
    LEFT,
    RIGHT
}


public class UIChat : MonoBehaviour
{
    public static UIChat _instance = null;

    public Method.FunctionVoid _callbackEnd = null;

    #region 대화창 프리팹.
    public GameObject _objDiaryChat;
    public GameObject _objDialogMission;

    //이모티콘 대화상자.
    public GameObject _objChatEmoticon;
    #endregion

    public ChatGameData _chatGameData;

    public UIWidget anchorRight;
    public UIWidget anchorRight_Dialog;
    public UIWidget anchorLeft_Dialog;

    //배경 관련.
    public UISprite letterBox_right;
    public UISprite letterBox_left;
    public UITexture background_center;
    public UITexture background_left;
    public UITexture background_right;

    //전화 화면 관련.
    public UITexture phoneIcon;
    public UITexture phoneCircle;
    public UITexture phoneLine;

    //채팅 중 Dialog가 뜨는 창.
    public Transform chatLayer;
    public Transform btnSkip;

    public AnimationCurve telePhoneBackAnimation;
    
    //이모티콘.
    private UIChatEmoticon emoticon_0 = null;
    private UIChatEmoticon emoticon_1 = null;
    
    private List<ChatData> _listChatData = new List<ChatData>();
    private ChatData chatData = null;
    private int currentChatDataIndex = 0;
    private float chatDelayTime = 0f;

    private bool bshow = false;
    private bool bEndChatUIAct = false;

    private BoxCollider colliderbtnClose = null;

    private List<bool> _listSpritePos = new List<bool>();

    private List<UIDialog> _ListDialogs = new List<UIDialog>();
    private int DIALOG_SPACE_SIZE = 140; //  대화창 간의 간격
    private float addDialogTime = 0.0f;
    private int missionSpriteCnt = 0;
    private const float CHAT_CAMERA_OFFSET = 6;

    #region 채팅 창 사운드.
    private const int DEAFAULT_DIALOG_SOUND_PACK = 1000;
    private const string DEAFAULT_DIALOG_SOUND_CLIP = "tutorial02";
    private const string CREATE_MISSION_SOUND_CLIP = "CreatMission";
    #endregion

    [SerializeField]
    private UIPanel scrollPanel = null;
    [SerializeField]
    public GameObject scrollBar = null;

    //전화 대화인지.
    private bool bCall = false;

    //터치 조작.
    private bool bCanTouch = true;

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        _instance = this;
        letterBox_right.enabled = false;
        letterBox_left.enabled = false;
        background_center.enabled = false;
        background_left.enabled = false;
        background_right.enabled = false;
        phoneIcon.gameObject.SetActive(false);
        phoneCircle.gameObject.SetActive(false);
        phoneLine.gameObject.SetActive(false);
    }
    void Start()
    {
        SetChatAnchor();
        ShowChatUI(true, _chatGameData);
    }

    public void TouchOn()
    {
        bCanTouch = true;
    }

    public void ShowChatUI(bool bShow, ChatGameData chatGameData)
    {
        bCanTouch = false;
        float _startTime = 0.0f;

        //대화창 띄울 때 현재 대화저장, 배경 표시여부 저장
        if (bShow == true)
        {
            _listChatData = null;
            if (chatGameData != null)
            {
                _listChatData = chatGameData._listChatData;
                chatDelayTime = chatGameData.chatDelayTime;
            }
            bEndChatUIAct = false;
            currentChatDataIndex = 0;

            //챗 데이터 받아오기.
            SetChatData(currentChatDataIndex);
        }

        //대화창이 열릴 때는 delayTime 만큼 기다림.
        if (bShow == true)
            _startTime += chatDelayTime;

        _startTime += 0.3f;
        //아래에서 대화박스 올라오는 연출.
        StartCoroutine(CoShowChatLayer(bShow, _startTime, 0.1f));
        _startTime += 0.3f;

        // 전화창 나타나는 연출(전화 안하면 대화창 안 나타남).
        StartCoroutine(SetFirstCallBackGround(_startTime));
        if (chatData.chatBack_0 != "" || chatData.chatBack_1 != "")
        {
            _startTime += 0.4f;
        }

        //캐릭터 나타나는 연출.
        StartCoroutine(CoShowChatCharacter(bShow, _startTime));
        //스킵버튼 나타나는 연출.
//        StartCoroutine(CoShowBtnSkip(bShow, _startTime, 0.2f, 0.5f));

        if (bShow == true)
        {
            _startTime += 0.5f;
            StartCoroutine(CoAction(_startTime, () =>
            {
                bEndChatUIAct = true;
                TouchOn();
                if (_listChatData != null && chatData != null)
                {
                    AddDialog(chatData.boxType, _chatGameData.GetString(chatData.stringKey), chatData.bChatColor);
                    StartCoroutine(SetEmoticon(chatData));
                }
            }));
        }
        bshow = bShow;
        if (!bShow)
        {
            colliderbtnClose.enabled = false;
        }
    }

    void SetChatAnchor()
    {
        anchorRight.leftAnchor.target = ManagerUI._instance._transform;
        anchorRight.rightAnchor.target = ManagerUI._instance._transform;
        anchorRight.leftAnchor.absolute = 0;
        anchorRight.rightAnchor.absolute = 0;

        anchorLeft_Dialog.leftAnchor.target = ManagerUI._instance._transform;
        anchorLeft_Dialog.rightAnchor.target = ManagerUI._instance._transform;
        anchorLeft_Dialog.leftAnchor.absolute = 0;
        anchorLeft_Dialog.rightAnchor.absolute = 0;

        anchorRight_Dialog.leftAnchor.target = ManagerUI._instance._transform;
        anchorRight_Dialog.rightAnchor.target = ManagerUI._instance._transform;
        anchorRight_Dialog.leftAnchor.absolute = 0;
        anchorRight_Dialog.rightAnchor.absolute = 0;

        anchorRight.ResetAndUpdateAnchors();
        anchorRight_Dialog.ResetAndUpdateAnchors();
        anchorLeft_Dialog.ResetAndUpdateAnchors();
    }

    void ShowChatUI(bool bShow)
    {
        if (bShow == true)
        {
            chatLayer.transform.localScale = new Vector3(1f, 0f, 1f);
            chatLayer.DOScaleY(1, 0.3f);
        }
        else
        {
            chatLayer.transform.localScale = Vector3.one;
            chatLayer.DOScaleY(0, 0.3f);
        }
    }

    private void SetChatData(int nIndex)
    {
        if (_listChatData == null || _listChatData.Count <= nIndex)
            return;
        
        chatData = _listChatData[nIndex];

        int nMissionCount = chatData.addMission.Count;

        //현재 타입이 미션이라면 이미 미션 수 받아온 것이므로 검사할 필요없음..
        if (chatData.boxType != TypeChatBoxType.Mission)
        {
            missionSpriteCnt = nMissionCount;
        }
        currentChatDataIndex++;
    }

    private IEnumerator CoShowChatLayer(bool bShow, float _startDelay = 0.0f, float _mainDelay = 0.0f)
    {
        yield return new WaitForSeconds(_startDelay);
        float fLayerYScale = 1f;
        if (bShow == true)
        {
            scrollBar.SetActive(true);
        }
        else
        {
            scrollBar.SetActive(false);
            fLayerYScale = 0f;
        }
        chatLayer.DOScaleY(fLayerYScale, _mainDelay);
    }

    private IEnumerator CoShowChatCharacter(bool bShow, float _startDelay = 0.0f)
    {
        yield return new WaitForSeconds(_startDelay);

        if (bShow)
        {
            LAppLive2DManager._instance.SetCharacterMotion(chatData);
        }
        else
        {
            LAppLive2DManager._instance.HideCharacter();
        }
    }

    private IEnumerator CoShowBtnSkip(bool bShow, float _startDelay = 0.0f, float _mainDelay = 0.0f, float _endDelay = 0.0f)
    {
        yield return new WaitForSeconds(_startDelay);
        Vector3 vecBtnSkipScale = Vector3.one;
        if (bShow == false)
            vecBtnSkipScale = Vector3.zero;
        btnSkip.DOScale(vecBtnSkipScale, _mainDelay);
        yield return new WaitForSeconds(_endDelay);
    }

    private IEnumerator CoRemoveAllDialog(float _startDelay = 0.0f)
    {
        yield return new WaitForSeconds(_startDelay);
        int nCount = _ListDialogs.Count;
        for (int i = nCount - 1; i >= 0; i--)
        {
            _ListDialogs[i].RemoveDialog(true);
        }
        _ListDialogs.Clear();
    }

    public void CloseBtn()
    {
        if (chatData == null || _listChatData == null || bCanTouch == false)
            return;

        if (chatData.waitConditionData != null)
            if (!chatData.waitConditionData._end)
                return;

        if (bEndChatUIAct == true)
        {
            bEndChatUIAct = false;
            //마지막 대사가 아닐 경우.
            if (currentChatDataIndex < _listChatData.Count)
            {
                bool continueChat = true;

                if (_listChatData[currentChatDataIndex].continueTargetMissionClear != null)
                {
                    for (int i = 0; i < _listChatData[currentChatDataIndex].continueTargetMissionClear.Count; i++)
                    {
                        if (ManagerData._instance._missionData[_listChatData[currentChatDataIndex].continueTargetMissionClear[i]].state != TypeMissionState.Clear)
                            continueChat = false;
                    }
                }

                if (continueChat)
                {
                    //챗 데이터 들고옴.
                    SetChatData(currentChatDataIndex);

                    bool bCharChange = false;
                    float changeTime = 0.0f;

                    // 캐릭터가 자리바꾸는 지 확인.(ex : 이름 말해줘)
                    if (chatData.bCharPositionChange == true)
                    {
                        bCharChange = true;
                        StartCoroutine(LAppLive2DManager._instance.ChangeCharPosition(chatData));
                        changeTime = 1.0f;
                    }
                    else
                    {
                        //라이브 투디 연결 - 캐릭터 전환이 있으면 전환 시간 추가.
                        bCharChange = LAppLive2DManager._instance.SetCharacterMotion(chatData);
                        if (bCharChange == true)
                        {
                            changeTime = 0.7f;
                        }
                    }

                    //이모티콘 재생.
                    StartCoroutine(SetEmoticon(chatData));

                    //캐릭터 전환 효과가 있으면 changeTime 후 터치 전환.
                    if (bCharChange == true)
                    {
                        bCanTouch = false;
                        StartCoroutine(CoAction(changeTime, TouchOn));
                    }

                    if (chatData.conditionData != null)
                        chatData.conditionData.gameObject.SetActive(true);

                    //캐릭터 전환 효과 등 연출 후 대화창 생성.
                    StartCoroutine(CoAction(changeTime, () =>
                    {
                        if (chatData.userInputBoxFunction != "")
                        {
                            bEndChatUIAct = true;
                            bCanTouch = false;
                            ManagerUI._instance.SendMessage(chatData.userInputBoxFunction, true);
                        }
                        else
                        {
                            AddDialog(chatData.boxType, _chatGameData.GetString(chatData.stringKey), chatData.bChatColor);
                        }
                    }));
                    return;
                }
            }
            //현재 대사가 마지막 대사이면 종료 연출.
            bCanTouch = false;

            //콜백호출.
            if (chatData != null && _callbackEnd != null)
                _callbackEnd();

            //_colliderbtnClose.enabled = false;
            float _startTime = 0.0f;

            //이모티콘 있으면 이모티콘 제거.
            StartCoroutine(SetEmoticon());
            
            //캐릭터 제거.
            StartCoroutine(CoShowChatCharacter(false, _startTime));
            _startTime += 0.7f;

            //전화 화면 남아있으면 전화화면 제거.
            if (bCall == true)
            {
                StartCoroutine(CoAction(_startTime, () =>
                {
                    bCall = false;
                    //전화 아이콘 제거.
                    StartCoroutine(CoPhoneIconAction());
                    //좌측 우측 배경 제거.
                    StartCoroutine(CoAction(0.2f, () =>
                    {
                        CallBackGroundAction(false, false, null);
                        CallBackGroundAction(false, true, null);
                    }));
                }));
                _startTime += (0.4f + 0.2f);
            }

            StartCoroutine(CoRemoveAllDialog(_startTime));
            //StartCoroutine(CoShowBtnSkip(false, _startTime, 0.2f, 0.2f));
            StartCoroutine(CoShowChatLayer(false, _startTime, 0.2f));
            _startTime += 0.2f;
            //배경제거, chatUI 삭제.
            StartCoroutine(CoAction(_startTime, () =>
            {
                StartCoroutine(ShowBackGround(false));
                bEndChatUIAct = true;
                Destroy(gameObject);
            }));
        }
    }

    private IEnumerator ShowBackGround(bool bShowback)
    {
        yield return new WaitForSeconds(chatDelayTime);
        letterBox_right.enabled = bShowback;
        letterBox_left.enabled = bShowback;
        background_center.enabled = bShowback;
        background_left.enabled = bShowback;
        background_right.enabled = bShowback;
    }

    void AudioPlay(TypeCharacterType in_ch)
    {
        AudioClip a = ManagerCharacter._instance.GetChatSound(in_ch);
        if( a != null)
        {
            ManagerSound.AudioPlay(a);
        }
    }

    void BackAudioPlay(AudioLobby audioLobby)
    {
        ManagerSound.AudioPlay(audioLobby);
    }

    void AddDialog(TypeChatBoxType boxType, string strChat, bool _bColor)
    {
        addDialogTime = 0.0f;
        int nDialogCount = _ListDialogs.Count;
        bEndChatUIAct = false;
        
        //대화 삭제하는 연출 있으면 삭제.
        if (chatData.bDeleteChat == true)
        {
            if (nDialogCount > 0)
                addDialogTime += 0.3f;
            for (int i = 0; i < nDialogCount; i++)
            {
                UIDialog _dialog = _ListDialogs[i];
                _dialog.RemoveDialog();
            }
            _ListDialogs.Clear();
            chatData.bDeleteChat = false;
        }
        if (addDialogTime == -1)
            return;

        //좌측 우측 배경 확인.
        addDialogTime += SetCallBackGround();
        /*
        //연출(페이드,사진,카메라) 이후 대화창 생성.
        StartCoroutine(CoAction(addDialogTime, () =>
        {
            if (strChat != "")
            {
                if (boxType == TypeChatBoxType.NormalLeft || boxType == TypeChatBoxType.NormalRight)
                {
                    UIItemDialogChat dialog = null;

                    if (chatData.boxType == TypeChatBoxType.NormalLeft)
                    {
                        dialog = NGUITools.AddChild(anchorLeft_Dialog.gameObject, _objDiaryChat).GetComponent<UIItemDialogChat>();
                        AudioPlay(chatData.character_0);
                    }

                    else if (chatData.boxType == TypeChatBoxType.NormalRight)
                    {
                        dialog = NGUITools.AddChild(anchorRight_Dialog.gameObject, _objDiaryChat).GetComponent<UIItemDialogChat>();
                        AudioPlay(chatData.character_1);
                    }

                    dialog.InitDialogBubble(boxType, strChat, _bColor);
                    _ListDialogs.Add(dialog);

                    DIALOG_SPACE_SIZE = dialog._sprDialogBox.height + 10;
                }

                else if (boxType == TypeChatBoxType.Mission && missionSpriteCnt > 0)
                {
                    int nIndex = chatData.addMission.Count - missionSpriteCnt;

                    ManagerSound.AudioPlay(AudioLobby.CreateMission);
                    Vector3 pos = new Vector3(0f, 175f, 0f);
                    UIItemDialogMission missionUI = NGUITools.AddChild(scrollPanel.gameObject, _objDialogMission).GetComponent<UIItemDialogMission>();
                    missionUI.transform.localPosition = pos;

                    //미션 데이터 가져오기.
                    MissionData missionData = ManagerData._instance._missionData[chatData.addMission[nIndex]._missionIndex];
                    string key = "m" + missionData.index;
                    missionUI.InitMissionSprite(ManagerArea._instance.GetAreaString(missionData.sceneArea, key), missionData.index);
                    _ListDialogs.Add(missionUI);

                    missionSpriteCnt -= 1;
                    DIALOG_SPACE_SIZE = missionUI._sprDialogBox.height + 10;
                    //대화 중 미션이 있으면 월드에 미션 아이콘 추가
                    ManagerLobby._instance.OpenPopupLobbyMission(chatData.addMission[nIndex]._missionIndex, chatData.addMission[nIndex]._targetTranform.position + chatData.addMission[nIndex]._offset);
                }
                else if(boxType == TypeChatBoxType.NormalMiddle)
                {
                    Vector3 pos = new Vector3(0f, 280f, 0f);

                    UIItemDialogSound dialog = null;
                    dialog = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemDialogSound", scrollPanel.gameObject).GetComponent<UIItemDialogSound>();
                    dialog.transform.localPosition = pos;
                    dialog.InitDialogBubble(strChat);
                    _ListDialogs.Add(dialog);
                    DIALOG_SPACE_SIZE = dialog._sprDialogBox.height + 10;
                }

                nDialogCount = _ListDialogs.Count;
                for (int i = 0; i < nDialogCount - 1; i++)
                {
                    UIDialog dialog = _ListDialogs[i];
                    dialog.MoveDialog(DIALOG_SPACE_SIZE);
                }

                //입력된 텍스쳐 이름에 따라 배경설정.
                SetBackGround();

                scrollPanel.transform.localPosition = Vector3.up * 0;
                scrollPanel.clipOffset = new Vector2(0, 0);
            }

            //미션이 하나라도 있으면 타입을 미션으로 변경 해줌(미션 챗 생성해주기 위해).
            if (missionSpriteCnt > 0)
            {
                chatData.boxType = TypeChatBoxType.Mission;
                currentChatDataIndex--;
            }
        }));
        */
        addDialogTime += 0.3f;
        StartCoroutine(CoAction(addDialogTime, () => { bEndChatUIAct = true; }));
    }

    private IEnumerator SetEmoticon(ChatData chatData = null)
    {
        #region 왼쪽 캐릭터 이모티콘.
        //이모티콘이 타입이 none 이 아니라면.
        if (chatData != null && chatData.emoticon_0 != TypeChatEmoticon.none)
        {
            //현재 이모티콘 아이템이 있는지.
            if (emoticon_0 != null)
            {
                //지금 이모티콘과 다른 이모티콘이라면 전 이모티콘 삭제, 현재 이모티콘 생성.
                if (chatData.emoticon_0 != emoticon_0.GetEmoticonType())
                {
                    StartCoroutine(emoticon_0.DestroyEmoticon());
                    yield return new WaitForSeconds(0.3f);
                    emoticon_0 = NGUITools.AddChild(gameObject, _objChatEmoticon).GetComponent<UIChatEmoticon>();
                    emoticon_0.MakeEmoticon(chatData.emoticon_0, CHAT_SIDE.LEFT, chatData.character_0);
                }
            }
            else
            {
                emoticon_0 = NGUITools.AddChild(gameObject, _objChatEmoticon).GetComponent<UIChatEmoticon>();
                emoticon_0.MakeEmoticon(chatData.emoticon_0, CHAT_SIDE.LEFT, chatData.character_0);
            }
            
        }
        //이모티콘이 타입이 none이거나 chatData 가 null 이라면.
        else
        { 
            //현재 이모티콘이 있다면, 아이템 삭제.
            if (emoticon_0 != null)
            {
                StartCoroutine(emoticon_0.DestroyEmoticon());
                emoticon_0 = null;
            }
        }
        #endregion 왼쪽 캐릭터 이모티콘.
        
        #region 오른쪽 캐릭터 이모티콘.
        //이모티콘이 타입이 none 이 아니라면.
        if (chatData != null && chatData.emoticon_1 != TypeChatEmoticon.none)
        {
            //현재 이모티콘 아이템이 있는지.
            if (emoticon_1 != null)
            {
                //지금 이모티콘과 다른 이모티콘이라면 전 이모티콘 삭제, 현재 이모티콘 생성.
                if (chatData.emoticon_1 != emoticon_1.GetEmoticonType())
                {
                    StartCoroutine(emoticon_1.DestroyEmoticon());
                    yield return new WaitForSeconds(0.3f);
                    emoticon_1 = NGUITools.AddChild(gameObject, _objChatEmoticon).GetComponent<UIChatEmoticon>();
                    emoticon_1.MakeEmoticon(chatData.emoticon_1, CHAT_SIDE.RIGHT, chatData.character_1);
                }
            }
            else
            {
                emoticon_1 = NGUITools.AddChild(gameObject, _objChatEmoticon).GetComponent<UIChatEmoticon>();
                emoticon_1.MakeEmoticon(chatData.emoticon_1, CHAT_SIDE.RIGHT, chatData.character_1);
            }
        }
        //이모티콘이 타입이 none이거나 chatData 가 null 이라면.
        else
        {
            //현재 이모티콘이 있다면, 아이템 삭제.
            if (emoticon_1 != null)
            {
                StartCoroutine(emoticon_1.DestroyEmoticon());
                emoticon_1 = null;
            }
        }
        #endregion 오른쪽 캐릭터 이모티콘.
    }

    private void SetBackGround()
    {
        Texture textureCenter = null;

        //텍스쳐 읽어오기.
        if (chatData.chatBackCenter != "")
            textureCenter = Resources.Load("Chat/" + chatData.chatBackCenter) as Texture;

        #region 중앙 배경 연출.
        //현재 적용된 배경 텍스쳐와 데이터의 텍스쳐가 다를 경우 실행됨.
        if (background_center.mainTexture != textureCenter)
        {
            if (chatData.chatBackCenter == "")
            {
                background_center.color = new Color(background_center.color.r, background_center.color.g, background_center.color.b, 1f);
                DOTween.ToAlpha(() => background_center.color, x => background_center.color = x, 0f, 0.3f)
                .OnComplete(() => { background_center.enabled = false; background_center.mainTexture = null; });

                //레터박스 알파.
                letterBoxAlpha(false, true);
                letterBoxAlpha(false, false);
            }
            else
            {
                background_center.enabled = true;
                background_center.mainTexture = textureCenter;
                background_center.color = new Color(background_center.color.r, background_center.color.g, background_center.color.b, 0f);
                DOTween.ToAlpha(() => background_center.color, x => background_center.color = x, 1f, 0.3f);

                //레터박스 알파.
                letterBoxAlpha(true, true);
                letterBoxAlpha(true, false);
            }
        }
        #endregion
    }

    //대화 창 처음 열릴 때, 전화 화면 호출.
    private IEnumerator SetFirstCallBackGround(float startTime)
    {
        yield return new WaitForSeconds(startTime);

        Texture textureLeft = null;
        Texture textureRight = null;

        if (chatData.chatBack_0 != "")
            textureLeft = Resources.Load("Chat/" + chatData.chatBack_0) as Texture;
        if (chatData.chatBack_1 != "")
            textureRight = Resources.Load("Chat/" + chatData.chatBack_1) as Texture;

        bool bSoundOn = false;
        
        if (chatData.chatBack_0 != "")
        {
            CallBackGroundAction(true, false, textureLeft);
            bSoundOn = true;
            bCall = true;
        }
        
        if (chatData.chatBack_1 != "")
        {
            CallBackGroundAction(true, true, textureRight);
            bSoundOn = true;
            bCall = true;
        }

        //소리 중복으로 재생되지 않도록.
        if (bSoundOn == true)
        {
            StartCoroutine(CoPhoneIconAction());
            //소리 중복으로 재생되지 않도록.
            if (bSoundOn == true)
            {
                ManagerSound.AudioPlay(AudioLobby.Call_Start);
            }
        }
    }

    //대화 도중 전화 배경 열리는지 검사 & 연출.
    private float SetCallBackGround()
    {
        Texture textureLeft = null;
        Texture textureRight = null;

        if (chatData.chatBack_0 != "")
            textureLeft = Resources.Load("Chat/" + chatData.chatBack_0) as Texture;
        if (chatData.chatBack_1 != "")
            textureRight = Resources.Load("Chat/" + chatData.chatBack_1) as Texture;

        bool bSoundOn = false;

        #region 좌측 배경 연출.
        //현재 적용된 배경 텍스쳐와 데이터의 텍스쳐가 다를 경우 실행됨.
        if (background_left.mainTexture != textureLeft)
        {
            if (chatData.chatBack_0 == "")
            {
                CallBackGroundAction(false, false, null);
            }
            else
            {
                CallBackGroundAction(true, false, textureLeft);
            }
            bSoundOn =true;
        }
        #endregion

        #region 우측 배경 연출.
        //현재 적용된 배경 텍스쳐와 데이터의 텍스쳐가 다를 경우 실행됨.
        if (background_right.mainTexture != textureRight)
        {
            if (chatData.chatBack_1 == "")
            {
                CallBackGroundAction(false, true, null);
            }
            else
            {
                CallBackGroundAction(true, true, textureRight);
            }
            bSoundOn = true;
        }
        #endregion

        //전화 화면 꺼질 때.
        if (bCall == true && chatData.chatBack_0 == "" && chatData.chatBack_1 == "")
        {
            bCall = false;
            StartCoroutine(CoPhoneIconAction());
            //소리 중복으로 재생되지 않도록.
            if (bSoundOn == true)
            {
                ManagerSound.AudioPlay(AudioLobby.Call_End);
            }
            return 0.4f;
        }

        //전화 화면 처음 켜질 때.
        else if (bCall == false && (chatData.chatBack_0 != "" || chatData.chatBack_1 != ""))
        {
            bCall = true;
            StartCoroutine(CoPhoneIconAction());
            //소리 중복으로 재생되지 않도록.
            if (bSoundOn == true)
            {
                ManagerSound.AudioPlay(AudioLobby.Call_Start);
            }
            return 0.4f;
        }
        return 0f;
    }

    void CallBackGroundAction(bool bShow, bool bRight, Texture texture = null)
    {
        //배경 사라질 때.
        if (bShow == false)
        {
            bool bSoundOn = false;

            if (bRight == false)
            {
                background_left.transform.localPosition = Vector3.one;
                background_left.transform.DOLocalMoveX(-200f, 0.3f).SetEase(telePhoneBackAnimation);
                background_left.color = new Color(background_left.color.r, background_left.color.g, background_left.color.b, 1f);
                DOTween.ToAlpha(() => background_left.color, x => background_left.color = x, 0f, 0.3f)
                .OnComplete(() => { background_left.enabled = false; background_left.mainTexture = null; });

                //레터박스 알파.
                letterBoxAlpha(false, bRight);
                bSoundOn = true;
            }
            else
            {
                background_right.transform.localPosition = Vector3.one;
                background_right.transform.DOLocalMoveX(200f, 0.3f).SetEase(telePhoneBackAnimation);
                background_right.color = new Color(background_right.color.r, background_right.color.g, background_right.color.b, 1f);
                DOTween.ToAlpha(() => background_right.color, x => background_right.color = x, 0f, 0.3f)
                .OnComplete(() => { background_right.enabled = false; background_right.mainTexture = null; });

                //레터박스 알파.
                letterBoxAlpha(false, bRight);
                bSoundOn = true;
            }
            if (bSoundOn == true)
            {
                ManagerSound.AudioPlay(AudioLobby.Call_End);
            }
        }

        //배경 생길 때
        else
        {
            if (bRight == false)
            {
                background_left.enabled = true;
                background_left.mainTexture = texture;
                background_left.transform.localPosition = new Vector3(-200f, 0f, 0f);
                background_left.transform.DOLocalMoveX(1f, 0.3f).SetEase(telePhoneBackAnimation);
                background_left.color = new Color(background_left.color.r, background_left.color.g, background_left.color.b, 0f);
                DOTween.ToAlpha(() => background_left.color, x => background_left.color = x, 1f, 0.3f);
                
                //레터박스 알파.
                letterBoxAlpha(true, bRight);
            }
            else
            {
                background_right.enabled = true;
                background_right.mainTexture = texture;
                background_right.transform.localPosition = new Vector3(200f, 0f, 0f);
                background_right.transform.DOLocalMoveX(1f, 0.3f).SetEase(telePhoneBackAnimation);
                background_right.color = new Color(background_right.color.r, background_right.color.g, background_right.color.b, 0f);
                DOTween.ToAlpha(() => background_right.color, x => background_right.color = x, 1f, 0.3f);

                //레터박스 알파.
                letterBoxAlpha(true, bRight);
            }
        }
    }

    //레터박스 알파.
    void letterBoxAlpha(bool bShow, bool bright)
    {
        if (bShow == false)
        {
            if (bright == true)
                DOTween.ToAlpha(() => letterBox_right.color, x => letterBox_right.color = x, 0f, 0.3f).OnComplete(() => { letterBox_right.enabled = false; });
            else
                DOTween.ToAlpha(() => letterBox_left.color, x => letterBox_left.color = x, 0f, 0.3f).OnComplete(() => { letterBox_left.enabled = false; });
        }
        else
        {  //레터박스 알파.
            if (bright == true)
            {
                letterBox_right.enabled = true;
                DOTween.ToAlpha(() => letterBox_right.color, x => letterBox_right.color = x, 1f, 0.3f);
            }
            else
            {
                letterBox_left.enabled = true;
                DOTween.ToAlpha(() => letterBox_left.color, x => letterBox_left.color = x, 1f, 0.3f);
            }
        }
    }

    IEnumerator CoPhoneIconAction()
    {
        if (bCall == true)
        {
            phoneIcon.transform.localScale = Vector3.zero;
            phoneCircle.transform.localScale = Vector3.zero;
            phoneLine.transform.localScale = new Vector3(1f, 0f, 1f);

            phoneIcon.gameObject.SetActive(true);
            phoneCircle.gameObject.SetActive(true);
            phoneLine.gameObject.SetActive(true);

            phoneIcon.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            phoneCircle.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);

            phoneLine.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(PhoneCircleRotate());
        }

        else
        {
            phoneLine.transform.localScale = Vector3.one;
            phoneIcon.transform.localScale = Vector3.one;
            phoneCircle.transform.localScale = Vector3.one;

            phoneLine.transform.DOScale(new Vector3(1f, 0f, 1f), 0.2f).SetEase(Ease.InBack);
            phoneIcon.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            phoneCircle.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);

            phoneIcon.gameObject.SetActive(false);
            phoneCircle.gameObject.SetActive(false);
            phoneLine.gameObject.SetActive(false);
        }
    }

    IEnumerator PhoneCircleRotate()
    {
        while (true)
        {
            if (phoneCircle.gameObject.activeInHierarchy == false || bCall == false)
                break;
            phoneCircle.transform.Rotate(new Vector3(0f, 0f, -1f) * 30f * Global.deltaTime);
            yield return null;
        }
    }

    private IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
