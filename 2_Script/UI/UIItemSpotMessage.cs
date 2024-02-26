using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemSpotMessage : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private GameObject potoRoot;
    [SerializeField] private UITexture photo;
    [SerializeField] private UIItemProfile profileItem;

    [Header("Etc")]
    [SerializeField] private UILabel messageText;
    [SerializeField] private UILabel leftTime;
    [SerializeField] private GenericReward genericReward;

    private MessageData item;
    private UserFriend uData = null;

    private bool bTimeStart = false;
    private Coroutine lastRoutine = null;

    public void UpdateData(MessageData mData)
    {
        item = mData;
        if (item == null || gameObject.activeInHierarchy == false)
            return;

        InitData();

        Reward reward = new Reward();

        reward.type = (int)item.type;
        reward.value = item.value;

        //보상 이미지.
        genericReward.SetReward(reward);

        //메시지 내용 세팅.
        if (item.textKey > -1)
        {
            string key = string.Format("msg_s_{0}", item.textKey);
            messageText.text = Global._instance.GetString(key).Replace("[1]", RewardHelper.GetRewardName(item.type, item.value));
            messageText.text = messageText.text.Replace("[n]", item.value.ToString());
        }
        else if (item.text != "")
        {
            messageText.text = item.text;
        }
        else
        {
            messageText.text = "text null";
        }

        //메세지 사진 세팅.
        SettingProfileImage();

        if (item.ts == 0)
        {
            leftTime.gameObject.SetActive(false);
        }
        else
        {
            leftTime.gameObject.SetActive(true);
            bTimeStart = true;
            leftTime.text = Global.GetTimeText_DDHHMM(item.ts);
            StartCoroutine(CoMessageTimer());
        }
    }

    void InitData()
    {
        bTimeStart = false;
        uData = null;
        if (lastRoutine != null)
        {
            StopCoroutine(lastRoutine);
        }
        photo.enabled = false;
        lastRoutine = null;
    }

    void SettingProfileImage()
    {
        if (item.IsSystemMessage())
        {
            potoRoot.SetActive(true);
            profileItem.SetDefaultProfile("profile_1");
            photo.enabled = true;
        }
        else
        {
            potoRoot.SetActive(true);
            photo.enabled = true;
            if (uData != null)
            {
                //프로필 아이템 추가
                profileItem.SetProfile(uData);
            }
            else
            {
                profileItem.SetDefaultProfile("profile_2");
            }
        }
    }

    public IEnumerator CoMessageTimer()
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false || bTimeStart == false)
                break;
            long lTime = Global.LeftTime(item.ts);
            leftTime.text = GetTimeText_DDHHMM(item.ts);

            if (lTime <= 0)
            {
                leftTime.effectColor = new Color32(0x7a, 0x7a, 0x7a, 0xFF);
                break;
            }
            else
            {
                leftTime.effectColor = new Color32(0x8C, 0xAA, 0xBD, 0xFF);
            }
            yield return null;
        }
    }

    string GetTimeText_DDHHMM(long in_time, bool bLeftTime = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = Global.LeftTime(t);
        }
        if (t < 0)
            t = 0;

        string text = "";

        int day = (int)(t / (3600 * 24));
        if (day > 0)
        {
            text += day + "Day ";
        }

        text += string.Format("{0:D2}", (int)((t / 3600) % 24)) + ": ";//"時間 ";
        text += string.Format("{0:D2}", (int)((t / 60) % 60)) + ": ";//"分 ";
        text += string.Format("{0:D2}", (int)(t % 60));//+ "秒";
        return text;
    }
}
