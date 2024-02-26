using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class ProtocolUser_scene
{
    // 0    받은 상태
    // 1    완료 했지만 시간이 있어 기다리는 상태
    // 10   정말 완료
    
    public int state = 0;
    public long clearTime = 0;
}

[System.Serializable]
public class ProtocolUser_user
{
    public int coin = 0;
    public int jewel = 0;
    public int clover = 0;
    public int star = 0;

    public int chapter = 0;
    // 진행중인 챕터의 미션 정보 , 이미 완료한 챕터는 필요없음
    public List<ProtocolUser_scene> mission = null;
}


[System.Serializable]
public class ProtocolGame_Scene
{
    public int sceneIndex = 0;
    public string sceneName = "";
    public int needStar = 1;
    public int waitTime = 0;
}
[System.Serializable]
public class ProtocolGame_Chapter
{
    public int chapterIndex = 0;
    public List<ProtocolUser_scene> sceneList = null;
}
