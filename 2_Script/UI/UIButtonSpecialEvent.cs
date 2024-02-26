using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonSpecialEvent : UIButtonEventBase
{
    public UILabel collectCount;
    public UILabel allCount;

    public override void SetButtonEvent(int eventIndex)
    {
        base.SetButtonEvent(eventIndex);
        //이벤트 이미지 설정.
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconEvent/", "ev_s_" + index, this);
        SettingCount();
    }

    protected override void OnClickBtnEvent()
    {
        base.OnClickBtnEvent();

        this.bCanTouch = false;

        ManagerUI._instance.OpenPopupSpecialEvent(index, false, CanTouch);
    }

    private void SettingCount()
    {/*
        int eventIndex = -1;
        for (int i = 0; i < ServerRepos.UserSpecilEvents.Count; i++)
        {
            if (ServerRepos.UserSpecilEvents[i].eventIndex == index)
            {
                eventIndex = i;
                break;
            }
        }

        int sectionCount = ServerContents.SpecialEvent[index].sections.Count;
        int allItemCount = ServerContents.SpecialEvent[index].sections[sectionCount - 1];
        allCount.text = string.Format("/{0}", allItemCount.ToString());

        //현재 유저 데이터에 해당하는 이벤트가 없으면 카운트 0.
        if (eventIndex == -1)
        {
            collectCount.text = "0";
        }
        else
        {
            int progressCount = ServerRepos.UserSpecilEvents[eventIndex].progress;
            //혹시라도 모은 수가 전체 수보다 많다면, 전체수 만큼만 출력.
            if (progressCount > allItemCount)
                progressCount = allItemCount;

            //유저가 모은 카운트 설정.
            collectCount.text = progressCount.ToString();
        }*/
    }
}
