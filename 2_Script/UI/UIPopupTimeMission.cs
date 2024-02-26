using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupTimeMission : UIPopupBase
{
    public UILabel[] missionName;
    public UILabel missionTime;
    public UILabel missionCoin;

    MissionData missionData;

    public void InitPopUp(MissionData mData)
    {
        missionData = mData;
        string key = "m" + mData.index;
        missionName[0].text = ManagerArea._instance.GetAreaString(mData.sceneArea, key);
        missionName[1].text = ManagerArea._instance.GetAreaString(mData.sceneArea, key);
        missionCoin.text = mData.waitCoin.ToString();
        StartCoroutine(CoMissionTimer());
    }

    public IEnumerator CoMissionTimer()
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;
            long leftTime = Global.LeftTime(missionData.clearTime);
            if (leftTime >= 60)
            {
                missionTime.text = Global.GetTimeText_HHMM(missionData.clearTime);
            }
            else
            {
                missionTime.text = Global.GetTimeText_SS(missionData.clearTime);
            }

            if ((leftTime) <= 0)
            {
                break;
            }
            yield return null;
        }

        if (gameObject.activeInHierarchy == true)
        {
            bCanTouch = false;
            missionTime.text = "00:00";
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnSpeedUp()
    {
        if (bCanTouch == false)
            return;

        if (missionData.waitCoin <= Global.coin)
        {
            bCanTouch = false;
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            missionData.clearTime = Global.GetTime();
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
        }
        bCanTouch = true;
    }
}
