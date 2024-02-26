using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMissionStampEvent : UIPopupBase
{
    public UIUrlTexture texture;
    public GameObject   completeRoot;
    public GameObject   loadText;
    public UISprite     buttonImage;
    public UILabel      lefttimeText;
    public UILabel[]    getText;
    public UIPokoButton getButton;

    //프로그레스 관련.
    public UIProgressBar progress_1;
    public UIProgressBar progress_2;
    public UILabel progressText_1;
    public UILabel progressText_2;

    public int eventIndex = 0;

    Coroutine countdownCoroutine = null;

    public class ProgressSet
    {
        public UIProgressBar progress;
        public UILabel label;
    }

    List<ProgressSet> progresses = new List<ProgressSet>();

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        SettingLoad();

        progresses = new List<ProgressSet>() {
            new ProgressSet() { progress = progress_1, label = progressText_1 },
            new ProgressSet() { progress = progress_2, label = progressText_2 }
        };

        RequestEventDetail();
        //이미지 로딩.
        texture.SuccessEvent += SettingLoadComplete;
        texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "line_stamp_" + eventIndex.ToString() + "_p");
    }

    private void RequestEventDetail()
    {
        ServerAPI.StickerInfos(ServerRepos.UserEventStickers[0].eventIndex, OnReceiveEventDetail);
    }

    private void OnReceiveEventDetail(Protocol.BaseResp resp)
    {
        if (resp.IsSuccess)
        {
            int eventIndex = ServerRepos.UserEventStickers[0].eventIndex;
            var cdnInfo = ServerContents.EventStickers[eventIndex];

            var questDetails = ServerRepos.UserEventQuests.FindAll(x => x.eventIndex == eventIndex);

            for ( int i = 0; i < questDetails.Count && i < 2; ++i)
            {
                var evDetail = questDetails[i];
                SettingProgress(i, evDetail.prog1, evDetail.targetCount);
            }
            this.SetLefttimeText();

            SettingButton(CheckEnableGetButton());
        }
    }

    private void SettingLoad()
    {
        completeRoot.SetActive(false);
        loadText.SetActive(true);
    }

    private void SettingLoadComplete()
    {
        completeRoot.SetActive(true);
        loadText.SetActive(false);
        //getText.text = Global._instance.GetSting("");

        if(countdownCoroutine == null)
            countdownCoroutine = StartCoroutine( CoLefttimeCountdown() );
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }   
    }

    public void SettingProgress(int idx, int count, int allCount)
    {
        //프로그레스 바 설정.
        float progressOffset = 100f / allCount;
        progresses[idx].progress.value = (count * progressOffset) * 0.01f;
        progresses[idx].label.text = string.Format("{0}/{1}", count, allCount);
        if (count >= allCount)
        {
            progresses[idx].label.color = new Color(1f, 84f/255f, 0f);
        }
    }

    private void SettingButton(bool bEnable)
    {
        if (bEnable == false)
        {
            float color = 86f / 255f;
            buttonImage.spriteName = "button_play03";
            getText[0].effectColor = new Color(color, color, color);
            getText[1].effectColor = new Color(color, color, color);
        }
    }

    private bool CheckEnableGetButton()
    {
        return ServerRepos.UserEventStickers[0].state != 0;
    }


    private void OnClickBtnGet()
    {
        if (!CheckEnableGetButton())
            return;

        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            ManagerUI._instance.GuestLoginSignInCheck();
        }
        else
        {
            int eventIndex = ServerRepos.UserEventStickers[0].eventIndex;

            ServerAPI.StickerGetReward(eventIndex, OnStickerGetApproved);
        }
    }

    private void OnStickerGetApproved(Protocol.StickerGetRewardResp resp)
    {
        if (resp.IsSuccess)
        {
            int eventIndex = ServerRepos.UserEventStickers[0].eventIndex;
            var cdnInfo = ServerContents.EventStickers[eventIndex];

            string stickerURL = @"http://line.me/R/shop/detail/" + cdnInfo.packageId.ToString();
            Application.OpenURL(stickerURL);

            //그로씨 스템프받음

        }
        else
        {
        }
        
    }

    IEnumerator CoLefttimeCountdown()
    {
        while(true)
        {
            SetLefttimeText();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SetLefttimeText()
    {
        int eventIndex = ServerRepos.UserEventStickers[0].eventIndex;
        var cdnInfo = ServerContents.EventStickers[eventIndex];

        var t = Global.LeftTime(cdnInfo.endTs);
        if (t < 0)
            t = 0;

        //bool jp = Global._systemLanguage == CurrLang.eJap;

        string text = Global._instance.GetString("ig_2");
        int day = (int)(t / (3600 * 24));
        if (day > 0)
        {
            text += $"{day}{Global._instance.GetString("time_1")} ";
        }

        text += $"{string.Format("{0:D2}", (int)((t / 3600) % 24))}{Global._instance.GetString("time_2")} ";
        text += $"{string.Format("{0:D2}", (int)((t / 60) % 60))}{Global._instance.GetString("time_3")} ";
        text += $"{string.Format("{0:D2}", (int)(t % 60))}{Global._instance.GetString("time_4")}";

        this.lefttimeText.text = text;
    }
}
