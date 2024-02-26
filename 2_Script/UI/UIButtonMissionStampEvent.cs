using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonMissionStampEvent : UIButtonEventBase
{
    bool nowLoading = false;
    bool missionState = false;
    public void SetButtonState(bool completed)
    {
        missionState = completed;
        nowLoading = true;
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconEvent/", "line_stamp_" + index.ToString() + (completed == false ? "_1" : "_2"), this);
    }


    protected override void OnClickBtnEvent()
    {
        base.OnClickBtnEvent();

        this.bCanTouch = false;

        ManagerUI._instance.OpenPopupMissionStampEvent(index);
    }

    override public void OnLoadComplete(ImageRequestableResult r)
    {
        base.OnLoadComplete(r);
        nowLoading = false;


    }

    public bool CheckCompleted()
    {
        if (nowLoading)
            return false;

        if (missionState == true)
            return true;

        //if (ServerRepos.UserEventStickers.Count > 0 && ServerRepos.UserEventStickers[0].state != 0)
        {
           // if(ServerContents.EventStickers.ContainsKey(ServerRepos.UserEventStickers[0].eventIndex) )
                return true;
        }
        return false;
    }
}
