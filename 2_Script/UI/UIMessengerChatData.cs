using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//대화 하나하나의 정보
//(ex. 보니 : 안녕).
public class MessengerChatInfo
{
    [TextArea]
    public string _strChat;
    //사진 관련
    public string _strStampName = null;
    //대화 시작 부터 n분 후(라고 보일지) 표시될 시간 설정.
    public int _nMinute;
    //말하는 캐릭터 설정.
    public string strCharacter;
}

[System.Serializable]
//한 미션당 대화.
//(ex. 보니 : 안녕 / 코코 : 안녕)
public class MessengerChatData
{
    //대화 내용을 들고있는 리스트(말 하나하나).
    public List<MessengerChatInfo> _listChatInfos;
    //메신저가 등록된 시간.
    public System.DateTime _nStartTime = new System.DateTime();
}

[System.Serializable]
//하나의 방의 메신저 대화.
//(ex. <코코 그룹>  10월 5일 보니 : 안녕 / 10월 6일  코코 : 안녕)
public class MessengerRoomData
{
    //대화들을 들고 있는 리스트(미션 단위의 대화).
    public List<MessengerChatData> _listMessengerDatas;
    //대화 방 이름.
    public string _strGroupName;
    //마지막으로 확인한 인덱스.
    //[HideInInspector]
    public int _nLastIndex = -1;
    //현재 열린 메신저 인덱스(Chat에서 등록해준 인덱스).
    //[HideInInspector]
    public int _nOpenIndex = -1;
}

public class UIMessengerChatData : MonoBehaviour
{
    public List<MessengerRoomData> _listMessengerRoomData;
}