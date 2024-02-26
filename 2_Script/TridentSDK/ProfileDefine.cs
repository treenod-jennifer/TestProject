using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Profile_PION
{
    public string userKey;              // userKey

    public System.Int64 updatedTime;    // 업데이트된 시간
    public System.Int64 createdTime;    // 만들어진 시간

    public Profile_PIONCustom profile = new Profile_PIONCustom();
}

public enum ProfileOpts
{
    LineMessageBlocked = 0,
    LineThumbnailUsed
}

[System.Serializable]
public class Profile_PIONCustom
{
    public int stage;
    public int[] opts;    // Line Message Options
    public string name;
    public int rank;
    public int missionCnt;
    public int flower;
    public int toy; // 2017-11-15 추가
    public int rankEventPoint;
    public long lastLoginTs;    // 2019-07-01 추가
    public string alterPicture; // 2020-01-28 추가
    public int pokoFriendCount; // 2020-04-29 추가

    // 라인 메세지 블록 유무
    public bool isLineMessageBlocked()
    {
        if (opts == null)
            return false;
        if (opts.Length > 0)
            return opts[(int)ProfileOpts.LineMessageBlocked] > 0;

        return false;
    }

    public bool isLineTumbnailUsed()
    {
        if (opts == null)
            return false;
        if (opts.Length > 1)
            return opts[(int)ProfileOpts.LineThumbnailUsed] > 0;

        return false;
    }
    
    public Profile_PIONCustom()
    {
        this.name = "user";
        this.stage = this.rank = 0;
        this.rank = 0;
        this.missionCnt = 0;
        this.flower = 0;
        this.rankEventPoint = 0;
        this.lastLoginTs = 0;
        this.pokoFriendCount = 0;
    }

    public void SetSDKGameProfileRequestData(UserBase data)
    {
        //data.stage = this.stage;
        //data.name = this.name;
        //data.mission = this.missionCnt;
        //data.rankEventPoint = this.rankEventPoint;
        //data.lastLoginTs = this.lastLoginTs;
        //data.alterPicture = this.alterPicture;
        Debug.LogFormat("SetSDKGameProfileRequestData {0} LastLoginTS{1}", data.GameName, data.lastLoginTs);
    }

    public void Log()
    {
        string logStr = string.Format("[SDKGameProfileServerData] Name: {0}, Flower {1}, MissionCnt {2}, RankEventPoint {3}, PhotoUse {4} ", name, flower, missionCnt, rankEventPoint, isLineTumbnailUsed());
        Debug.Log(logStr);
    }
}

[System.Serializable]
public class SDKGameProfileRequestJsonData
{
    public Profile_PIONCustom profile;

    public SDKGameProfileRequestJsonData()
    {
        profile = new Profile_PIONCustom();
    }

    public SDKGameProfileRequestJsonData(Profile_PIONCustom data)
    {
        profile = data;
    }
}

[System.Serializable]
public class SDKGameProfileResPonseData
{
    public System.Int64 createdTime; // 만들어진 시간

    public Profile_PIONCustom profile = new Profile_PIONCustom();

    public string       status;
    public System.Int64 updatedTime; // 업데이트된 시간
    public string       userKey;     // userKey
}