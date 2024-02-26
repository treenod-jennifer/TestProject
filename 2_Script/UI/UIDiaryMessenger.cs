using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

//메신저 창의 그룹 아이템에서의 정보들.
public class MessengerCharData
{
    public int roomIndex;
    public string roomName;
    public string lastTime;
    public string lastMessage;
    public bool bNew;

    public DateTime dateTime;
}

public class DateComparer : IComparer<MessengerCharData>
{
    public int Compare(MessengerCharData a, MessengerCharData b)
    {
        DateTime _timeA = a.dateTime;
        DateTime _timeB = b.dateTime;
        //메신저 시간과 현재 시간 비교(0보다 크면 좌측 시간이 더 큼, 0이면 둘의 값이 동일, 0보다 작으면 우측 시간이 더 큼)
        //내림차순으로 정렬하기 위해 구해진 값 * -1 을 해줌.
        return (DateTime.Compare(_timeA, _timeB) * -1);
    }
}

public class UIDiaryMessenger : MonoBehaviour
{
    public static UIDiaryMessenger instance;

    //메신저 대화 더미 데이터.(임시).
    public UIMessengerChatData _objMessengerDummy;

    //메신저 대화 들고올 리스트.
    private List<MessengerRoomData> _listMessengerRooms = new List<MessengerRoomData>();

    //룸 정보 세팅된 프리팹.
    public GameObject _objMessengerRoom;

    public GameObject roomBox;
    public GameObject chatBox;
    public GameObject blackSprite;

    public GameObject roomScrollBar;
    public GameObject chatScrollBar;

    public GameObject RoomRoot;

    #region 무한 스크롤뷰 사용 안함.
    public GameObject _objChatItem;
    public GameObject _objCellRoot;
    public UIScrollView chatScrollView;
    #endregion

    public UIPanel messengerPanel;
    public UIPanel roomPanel;
    public UIPanel chatPanel;
    public UILabel roomName;
    //public ReuseGrid_MessengerChat _messengerChatGrid;

    List<MessengerCharData> _listCharData = new List<MessengerCharData>();
    List<UIItemMessengerRoom> _listBtnMessenger = new List<UIItemMessengerRoom>();

    DateComparer dateComparer = new DateComparer();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        blackSprite.SetActive(false);
        roomBox.transform.localPosition = new Vector3(0f, -30f, 0f);
        chatBox.transform.localPosition = new Vector3(683f, -30f, 0f);
        SetMessengerRoom();
        InitRoomBox();
    }

    public void RemoveAllBtnMessenger()
    {
        int nCount = _listBtnMessenger.Count;
        for (int i = 0; i < nCount; i++)
        {
            DestroyImmediate(_listBtnMessenger[i].gameObject);
        }
        _listBtnMessenger.Clear();
        ResetDiaryMessenger();
    }

    //그룹 정보 들고와서 세팅해주는 함수.
    public void SetMessengerRoom()
    {
        _listMessengerRooms = _objMessengerDummy._listMessengerRoomData;

        if (_listMessengerRooms.Count == 0)
            return;

        int nCount = _listMessengerRooms.Count;

        //메신저데이터(챗 묶음) 인덱스.
        int _mIndex = -1;
        //챗 인덱스.
        int _cIndex = -1;

        //그룹들 검사하면서 정보 가지고 옴.
        for (int i = 0; i < nCount; i++)
        {
            MessengerRoomData _messengerRoom = _listMessengerRooms[i];
            //현재 검사하고자 하는 그룹의 오픈된 메신저 인덱스가 -1이라면 정보를 받아오지 않음.
            if (_messengerRoom._nOpenIndex == -1)
            {
                continue;
            }
            //현재 검사하고자 하는 그룹정보 저장.
            MessengerCharData _charData = new MessengerCharData();
            _mIndex = _messengerRoom._nOpenIndex;
            _cIndex = _messengerRoom._listMessengerDatas[_mIndex]._listChatInfos.Count - 1;

            //해당 그룹 아이템에 정보 전달.
            _charData.roomIndex = i;
            _charData.roomName = _messengerRoom._strGroupName;
            _charData.lastMessage = _messengerRoom._listMessengerDatas[_mIndex]._listChatInfos[_cIndex]._strChat;

            #region 시간/날짜 표시 관련.
            _charData.dateTime = _messengerRoom._listMessengerDatas[_mIndex]._nStartTime;
            //메신저 시간과 현재 시간 비교(0보다 크면 좌측 시간이 더 큼, 0이면 둘의 값이 동일, 0보다 작으면 우측 시간이 더 큼)
            if (DateTime.Compare(_charData.dateTime.Date, DateTime.Now.Date) < 0)
            {
                _charData.lastTime = _charData.dateTime.ToString("M/dd");
            }
            else
            {
                _charData.lastTime = _charData.dateTime.ToString("tt HH:mm");
            }
            #endregion

            //현재 내가 보고자 하는 메신저의 인덱스가 마지막으로 확인한 인덱스보다 크다면 new 버튼 생성.
            if (_messengerRoom._nOpenIndex > _messengerRoom._nLastIndex)
            {
                _charData.bNew = true;
            }
            _listCharData.Add(_charData);
        }
    }

    //메신저 창 옮겨지는 버튼들 클릭 시 움직이는 연출해주는 함수.
    public void OnClickBtnMessage(bool bRoomBox, MessengerCharData _charData = null)
    {
        Transform _tr;
        float _xOffset = 683f;

        //룸 목록 창에서의 이동 시 설정.
        if (bRoomBox)
        {
            _tr = roomBox.transform;
            roomName.text = _charData.roomName;
            roomScrollBar.SetActive(false);
            chatScrollBar.SetActive(true);

            #region 무한 스크롤뷰 사용 안함.

            DateTime _time = new DateTime(1997, 1, 1);
            //현재 대화의 처음 위치(시간 같은게 위에 있을때 조금 밑에 위치하도록 하기 위해).
            float _yPosCell = 0f;
            //다음 대화가 생성될 위치.
            float nextPos = 0f;
            bool _bFirst = false;

            MessengerRoomData _messengerRoom = _listMessengerRooms[_charData.roomIndex];
            _messengerRoom._nLastIndex = _messengerRoom._nOpenIndex;

            //현재 그룹의 열린 메신저 목록만큼 아이템 생성.
            for (int i = 0; i < _messengerRoom._nOpenIndex + 1; i++)
            {
                _bFirst = false;
                if (i == 0)
                {
                    _bFirst = true;
                }

                MessengerChatData _chatData = _listMessengerRooms[_charData.roomIndex]._listMessengerDatas[i];
                #region 시간표시 계산.
                /*
                //메신저 시간 비교(0보다 크면 좌측 시간이 더 큼, 0이면 둘의 값이 동일, 0보다 작으면 우측 시간이 더 큼)
                //날짜별로 비교(시간은 고려안함).
                if (DateTime.Compare(_time.Date, _chatData._nStartTime.Date) < 0)
                {
                    //시간과 라인 띄운 후 위치 값 반환.
                    _yPosCell = itemChat.SetLineAndTime(_bFirst, _chatData._nStartTime);
                }*/
                #endregion
                //대화 내용 생성.
                for (int j = 0; j < _chatData._listChatInfos.Count; j++)
                {
                    UIItemMessengerChat itemChat = NGUITools.AddChild(_objCellRoot, _objChatItem).GetComponent<UIItemMessengerChat>();
                    _time = _chatData._nStartTime;

                    /*
                    bool bShowIcon = true;
                    //이전 대화가 있을 때 이전 대화의 캐릭터 검사.
                    if (j > 0 && _chatData._listChatInfos[j - 1] != null)
                    {
                        if (_chatData._listChatInfos[j].strCharacter == _chatData._listChatInfos[j - 1].strCharacter)
                        {
                            bShowIcon = false;
                        }
                    }*/
                    itemChat.UpdateData(_chatData._nStartTime, _chatData._listChatInfos[j]);
                    itemChat.transform.localPosition = new Vector3(itemChat.transform.localPosition.x, nextPos, itemChat.transform.localPosition.z);
                    nextPos = itemChat.GetNextPos();
                }
            }
            //Vector3 _pos = new Vector3(0, -nextPos, 0);
            SpringPanel.Begin(chatScrollView.gameObject, Vector3.zero, 8);
            //_scrollView.transform.localPosition = _pos;
            //_scrollView.panel.clipOffset = new Vector2(0, _yPosList);
        }
        #endregion

        //    //무한 스크롤 뷰 사용하려면 아래 코드 주석 제거(아이템 사이즈에 따른 코드 변경 필요).
        //    //InitMessengerChatScrollView(_charData._nGroupIndex);
        //}
        //대화 창에서의 이동 시 설정.
        else
        {
            roomScrollBar.SetActive(true);
            chatScrollBar.SetActive(false);
            _tr = chatBox.transform;
            _xOffset *= -1;
        }

        blackSprite.transform.parent = _tr;
        blackSprite.transform.localPosition = Vector3.zero;
        blackSprite.SetActive(true);
        StartCoroutine(CoSpriteBlackAlpha(0.2f));
        MoveMessenger(roomBox.transform, roomBox.transform.localPosition.x - _xOffset, 0.2f);
        MoveMessenger(chatBox.transform, chatBox.transform.localPosition.x - _xOffset, 0.2f);
        chatBox.SetActive(true);
    }

    //다른 탭 눌렀거나 할때 연출없이 초기화 시켜주는 함수.
    void ResetDiaryMessenger()
    {
        float _xOffset = -683f;
        roomBox.transform.localPosition
            = new Vector3(roomBox.transform.localPosition.x - _xOffset, roomBox.transform.localPosition.y, 0);
        chatBox.transform.localPosition
            = new Vector3(chatBox.transform.localPosition.x - _xOffset, roomBox.transform.localPosition.y, 0);
    }

    //친구 목록 생성.
    void InitRoomBox()
    {
        //시간순으로 정렬.
        _listCharData.Sort(dateComparer);
        for (int i = 0; i < _listCharData.Count; i++)
        {
            UIItemMessengerRoom _btnMessenger = NGUITools.AddChild(RoomRoot, _objMessengerRoom).GetComponent<UIItemMessengerRoom>();
            _btnMessenger.InitBtnMessenger(_listCharData[i]);
            _btnMessenger.transform.localPosition = new Vector3(-330f, 463f - (140f * i), 0f);
            _listBtnMessenger.Add(_btnMessenger);
        }
    }

    //채팅 창에서 back버튼 눌렀을 때 호출.
    void onClickBtnBack()
    {
        OnClickBtnMessage(false);
        DestroyChat();
    }

    void DestroyChat()
    {
        UIItemMessengerChat[] childList = _objCellRoot.GetComponentsInChildren<UIItemMessengerChat>();
        if (childList != null)
        {
            for (int i = 0; i < childList.Length; i++)
            {
                if (childList[i] != transform)
                {
                    Destroy(childList[i].gameObject);
                }
            }
        }
    }

    #region MessengerAction
    private void MoveMessenger(Transform _tr, float _xPos, float _mainDelay = 0.0f)
    {
        _tr.DOLocalMoveX(_xPos, _mainDelay);
    }

    private IEnumerator CoSpriteBlackAlpha(float _mainDelay = 0.0f)
    {
        UISprite _sprBlack = blackSprite.GetComponent<UISprite>();

        float ElapsedTime = 0f;
        float fTime = (ElapsedTime / _mainDelay);
        while ((0.15f - fTime) >= 0f)
        {
            fTime = (ElapsedTime / _mainDelay);
            _sprBlack.alpha = fTime;
            ElapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    #endregion
    
}
