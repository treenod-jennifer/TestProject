using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSpecialEvent : UIPopupBase
{
    public UIProgressBar progress;
    public UIUrlTexture backImage;
    public UIUrlTexture ItemCountIcon;
    public UILabel[]    itemCountText;
    public UILabel[]    allCountText;
    public UILabel      eventTime;
    public GameObject   loadText;
    public GameObject   progressBox;

    //보상.
    public GameObject[] targetObject;
    public UILabel[]    targetCount;

    //public UIItemSpecialEventBubble[] rewardBubble;

    private Method.FunctionVoid callBackEnd = null;

    private int contentIndex = -1;
    private int userIndex = -1;
    private int allCount = 0;
    private int progressCount = 0;

    private long time = 0;

    public void InitPopUp(int index, bool notifyGetReward, Method.FunctionVoid callBack = null)
    {
        SettingLoad();

        contentIndex = index;
        //배경 이미지 로딩.
        string name = string.Format("sEventPopup_{0}", contentIndex);
        backImage.SettingTextureScale(706, 982);
        backImage.SettingCallBack(SettingLoadComplete);
        backImage.Load(Global.gameImageDirectory, "IconEvent/", name);

        //아이템 이미지 로딩.
        string iconName = string.Format("sEventBlock_{0}", contentIndex);
        ItemCountIcon.SettingCallBack(SettingIcon);
        ItemCountIcon.Load(Global.gameImageDirectory, "IconEvent/", iconName);
        
        //시간 세팅.
        SettingTime();

        //카운트 세팅.
        SettingCount();

        //목표 세팅.
        SettingTargetCount();
        SettingTargetBar();

        //프로그레스 바 세팅.
        SettingProgressBar();

        //보상 세팅.
        SettingReward();

        if (notifyGetReward)
        {   // 여기서 직접 팝업띄우면 팝업순서가 꼬이니까 다음프레임에 뜨도록 처리
            StartCoroutine(CoOpenPopupReceivedReward());
        }

        if (callBack != null)
        {
            callBack();
        }
    }

    IEnumerator CoOpenPopupReceivedReward()
    {
        yield return null;

        string title = Global._systemLanguage == CurrLang.eJap ? "メッセージ" : "MESSAGE";
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_1"), false, null);
        popup.SortOrderSetting();
    }

    private void SettingTime()
    {/*
        time = ServerContents.SpecialEvent[contentIndex].endTs;
        StartCoroutine(CoLefttimeCountdown());*/
    }

    private void SettingCount()
    {/*
        for (int i = 0; i < ServerRepos.UserSpecilEvents.Count; i++)
        {
            if (ServerRepos.UserSpecilEvents[i].eventIndex == contentIndex)
            {
                userIndex = i;
                break;
            }
        }
        
        int sectionCount = ServerContents.SpecialEvent[contentIndex].sections.Count;
        allCount = ServerContents.SpecialEvent[contentIndex].sections[sectionCount - 1];

        //현재 유저 데이터에 해당하는 이벤트가 없으면 0으로 설정.
        if (userIndex == -1)
        {
            progressCount = 0;
        }
        else
        {
            progressCount = ServerRepos.UserSpecilEvents[userIndex].progress;
        }

        //혹시라도 모은 수가 전체 수보다 많다면, 전체수 만큼만 출력.
        if (progressCount > allCount)
            progressCount = allCount;

        //카운트 설정.
        for (int i = 0; i < itemCountText.Length; i++)
        {
            itemCountText[i].text = progressCount.ToString();
        }
        for (int j = 0; j < allCountText.Length; j++)
        {
            allCountText[j].text = string.Format("/{0}", allCount.ToString());
        }*/
    }

    private void SettingTargetCount()
    {/*
        //카운트 설정.
        for (int i = 0; i < targetCount.Length; i++)
        {
            targetCount[i].text = ServerContents.SpecialEvent[contentIndex].sections[i].ToString();
        }*/
    }

    private void SettingTargetBar()
    {/*
        List<int> sections = ServerContents.SpecialEvent[contentIndex].sections;
        
        //칸 간격.
        float step = 520f/ allCount;

        // -263f : 초기위치.
        //카운트 설정.
        for (int i = 0; i < targetObject.Length; i++)
        {   
            float xPos = -260f + (step * sections[i]) + 3.5f;
            targetObject[i].transform.localPosition = new Vector3(xPos, 0f, 0f);
        }*/
    }

    private void SettingProgressBar()
    {/*
        //유저 정보 없으면 0.
        if (userIndex == -1)
        {
            progress.value = 0;
        }
        else
        {
            //칸 간격.
            int progressCount = ServerRepos.UserSpecilEvents[userIndex].progress;
            float progressStep = (float)progressCount / allCount;
            progress.value = progressStep;
        }*/
    }

    private void SettingReward()
    {/*
        int allRewardCount = ServerContents.SpecialEvent[contentIndex].rewards.Count;
        for (int i = 0; i < allRewardCount; i++)
        {
            //보상 풍선 수가 현재 검사하고 있는 데이터보다 적으면 나감.
            if (rewardBubble.Length <= i)
                return;

            int bubbleRewardCount = ServerContents.SpecialEvent[contentIndex].rewards[i].Count;

            //제일 마지막 보상 말풍선인지 체크.
            bool bLastBubble = false;
            if (i == (allRewardCount - 1))
            {
                bLastBubble = true;
            }

            //보상 수와 마지막 보상인지 여부에 따라 말풍선 타입 설정.
            CollectEventRewardType bubbleType = CollectEventRewardType.normal;
            if (bubbleRewardCount == 1)
            {
                if (bLastBubble == true)
                {
                    bubbleType = CollectEventRewardType.big_normal;
                }
                else
                {
                    bubbleType = CollectEventRewardType.normal;
                }
            }
            else
            {
                if (bLastBubble == true)
                {
                    bubbleType = CollectEventRewardType.big_multi;
                }
                else
                {
                    bubbleType = CollectEventRewardType.multi;
                }
            }

            //보상 받았는지 확인.
            bool bCheck = false;

            //유저 정보가 있다면 현재 유저 section정보 받아와서 체크 표시.
            if (userIndex > -1)
            {
                int userSection = ServerRepos.UserSpecilEvents[userIndex].rewardSection;
                if (userSection >= (i + 1))
                    bCheck = true;
            }
            //말풍선 설정.
            rewardBubble[i].InitBubble(bubbleType, ServerContents.SpecialEvent[contentIndex].rewards[i], bCheck);
        }*/
    }

    private void SettingIcon()
    {
        ItemCountIcon.width = (int)(ItemCountIcon.width * 1.5f);
        ItemCountIcon.height = (int)(ItemCountIcon.height * 1.5f);
    }

    private void SettingLoad()
    {
        progressBox.SetActive(false);
        loadText.SetActive(true);
    }

    private void SettingLoadComplete()
    {
        progressBox.SetActive(true);
        loadText.SetActive(false);
    }

    IEnumerator CoLefttimeCountdown()
    {
        while (true)
        {
            eventTime.text = Global.GetTimeText_DDHHMM(time);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
