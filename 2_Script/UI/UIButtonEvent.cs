using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class UIButtonEvent : UIButtonEventBase
{
    public override void SetButtonEvent(int eventIndex)
    {
        base.SetButtonEvent(eventIndex);
        //이벤트 이미지 설정.
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconEvent/", "ev_" + index, this);
    }

    protected override void OnClickBtnEvent()
    {
        base.OnClickBtnEvent();

        if (UIPopupReady._instance != null)
            return;
        /*
        Global.stageIndex = ServerRepos.EventChapters[index].stage;

        
        if (ServerContents.EventChapters[index].type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            if (ServerRepos.EventChapters[index].groupState > ServerContents.EventChapters[index].counts.Count)
            {
                if (UIPopupSystem._instance == null)
                {
                    string title = Global._systemLanguage == CurrLang.eJap ? "メッセージ" : "MESSAGE";
                    var image = Resources.Load("Message/happy1") as Texture2D;
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_4"), false, image);
                    popup.SortOrderSetting();
                }
                return;
            }
        }

        this.bCanTouch = false;

        Global.eventIndex = index;

        CdnEventChapter cdnData = ServerContents.EventChapters[Global.eventIndex];
        if (Global.stageIndex > cdnData.counts[cdnData.counts.Count - 1])
            Global.stageIndex = cdnData.counts[cdnData.counts.Count - 1];

        ManagerUI._instance.OpenPopupReadyStageEvent(CanTouch);*/
    }
}
