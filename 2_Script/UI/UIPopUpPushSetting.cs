using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpPushSetting : UIPopupBase
{
    [SerializeField] private GameObject pushCheck;
    [SerializeField] private GameObject nightTimeCheck;
    [SerializeField] private GameObject fullCloverCheck;
    [SerializeField] private GameObject finishMissionCheck;
    [SerializeField] private GameObject giftBoxCheck;
    [SerializeField] private GameObject eventInfoCheck;
    [SerializeField] private GameObject friendGiftCheck;

    public UILabel[] title;

    [SerializeField] private UILabel pushText;
    [SerializeField] private UILabel nightTimeText;
    [SerializeField] private UILabel fullCloverText;
    [SerializeField] private UILabel finishMissionText;
    [SerializeField] private UILabel giftBoxText;
    [SerializeField] private UILabel eventInfoText;
    [SerializeField] private UILabel friendGiftText;

    private Color defaultColor;
    private Color hideColor;

    // -----------------------------------------------------------------------------------
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        //폰트 색 설정.
        defaultColor = new Color(58f / 255f, 94f / 255f, 145f / 255f, 1f);
        hideColor = new Color(135f / 255f, 135f / 255f, 135f / 255f, 1f);

        pushCheck.gameObject.SetActive(GetCheckState("_optionLocalPush"));
        nightTimeCheck.gameObject.SetActive(GetCheckState("_optionNotPushNightTime"));
        fullCloverCheck.gameObject.SetActive(GetCheckState("_optionPushFullClover"));
        finishMissionCheck.gameObject.SetActive(GetCheckState("_optionPushMissionComplete"));
        giftBoxCheck.gameObject.SetActive(GetCheckState("_optionPushGiftBox"));
        eventInfoCheck.gameObject.SetActive(GetCheckState("_optionPushEventInfo"));
        friendGiftCheck.gameObject.SetActive(GetCheckState("_optionPushFriendGift"));

        string titleText = Global._instance.GetString("p_ps_1");
        title[0].text = titleText;
        title[1].text = titleText;

        pushText.text = Global._instance.GetString("p_ps_2");
        nightTimeText.text = Global._instance.GetString("p_ps_3");
        fullCloverText.text = Global._instance.GetString("p_ps_4");
        finishMissionText.text = Global._instance.GetString("p_ps_5");
        giftBoxText.text = Global._instance.GetString("p_ps_6");
        eventInfoText.text = Global._instance.GetString("p_ps_7");
        friendGiftText.text = Global._instance.GetString("p_ps_8");

        ColorUpdate(GetCheckState("_optionLocalPush"));
    }

    bool GetCheckState(string key)
    {
        return PlayerPrefs.GetInt(key) == 0;
    }

    // -----------------------------------------------------------------------------------
    void OnClickPushCheck()
    {
        bool isCheck = this.OnClickEventSpriteCheck( this.pushCheck );
        PlayerPrefs.SetInt( "_optionLocalPush", ( isCheck ) ? 0 : 1 );
        UpdateAllPush(isCheck);
        ColorUpdate(isCheck);
    }

    void OnClickNightTimeCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck( this.nightTimeCheck );
        //Global._optionNotPushNightTime = isCheck;
        PlayerPrefs.SetInt( "_optionNotPushNightTime", ( isCheck ) ? 0 : 1 );
    }

    void OnClickFullCloverCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck( this.fullCloverCheck );
        //Global._optionPushFullClover = isCheck;
        PlayerPrefs.SetInt( "_optionPushFullClover", ( isCheck ) ? 0 : 1 );
    }

    void OnClickFinishMissionCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck( this.finishMissionCheck );
        //Global._optionPushMissionComplete = isCheck;
        PlayerPrefs.SetInt( "_optionPushMissionComplete", ( isCheck ) ? 0 : 1 );
    }

    void OnClickGiftBoxCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck( this.giftBoxCheck );
        //Global._optionPushGiftBox = isCheck;
        PlayerPrefs.SetInt( "_optionPushGiftBox", ( isCheck ) ? 0 : 1 );
    }

    void OnClickEventInfoCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck ( this.eventInfoCheck );
        //Global._optionPushEventInfo = isCheck;
        PlayerPrefs.SetInt( "_optionPushEventInfo", ( isCheck ) ? 0 : 1 );
    }

    void OnClickFriendGiftCheck()
    {
        if (pushCheck.activeInHierarchy == false)
            return;
        bool isCheck = this.OnClickEventSpriteCheck(this.friendGiftCheck);
        PlayerPrefs.SetInt("_optionPushFriendGift", (isCheck) ? 0 : 1);
    }

    // -----------------------------------------------------------------------------------
    private bool OnClickEventSpriteCheck ( GameObject spriteObj )
    {
        if ( spriteObj.activeInHierarchy == true )
        {
            spriteObj.SetActive( false );
            return false;
        }
        else
        {
            spriteObj.SetActive( true );
            return true;
        }
    }

    private void UpdateAllPush(bool bCanPush)
    {
        //버튼들 상태 변경.
        nightTimeCheck.SetActive(bCanPush);
        fullCloverCheck.SetActive(bCanPush);
        finishMissionCheck.SetActive(bCanPush);
        giftBoxCheck.SetActive(bCanPush);
        eventInfoCheck.SetActive(bCanPush);
        friendGiftCheck.SetActive(bCanPush);
        PlayerPrefs.SetInt("_optionNotPushNightTime", (bCanPush) ? 0 : 1);
        PlayerPrefs.SetInt("_optionPushFullClover", (bCanPush) ? 0 : 1);
        PlayerPrefs.SetInt("_optionPushMissionComplete", (bCanPush) ? 0 : 1);
        PlayerPrefs.SetInt("_optionPushGiftBox", (bCanPush) ? 0 : 1);
        PlayerPrefs.SetInt("_optionPushEventInfo", (bCanPush) ? 0 : 1);
        PlayerPrefs.SetInt("_optionPushFriendGift", (bCanPush) ? 0 : 1);
    }

    private void ColorUpdate(bool bCanPush)
    {
        //푸쉬 가능할 때.
        if (bCanPush == true)
        {
            nightTimeText.color = defaultColor;
            fullCloverText.color = defaultColor;
            finishMissionText.color = defaultColor;
            giftBoxText.color = defaultColor;
            eventInfoText.color = defaultColor;
            friendGiftText.color = defaultColor;
        }
        else
        {
            nightTimeText.color = hideColor;
            fullCloverText.color = hideColor;
            finishMissionText.color = hideColor;
            giftBoxText.color = hideColor;
            eventInfoText.color = hideColor;
            friendGiftText.color = hideColor;
        }
    }
}
