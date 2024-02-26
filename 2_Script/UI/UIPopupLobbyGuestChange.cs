using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupLobbyGuestChange : UIPopupBase
{
    #region 캐릭터
    public UIItemGuest itemGuest_1;
    public UISprite[] grades_1;

    public UIItemGuest itemGuest_2;
    public UISprite[] grades_2;
    #endregion

    #region 팝업 텍스트
    public UILabel[] titleText;
    public UILabel beforeText;
    public UILabel afterText;
    public UILabel messageText;
    public UILabel[] btnText_1;
    public UILabel[] btnText_2;
    #endregion

    private int beforeIndex = 0;
    private int afterIndex = 0;

    #region 콜백
    //캐릭터 변경 선택 콜백
    public System.Action<int, int> gusetChangeAction = null;
    #endregion

    private float alphaColor = 125f / 255f;

    public void InitPopup(int beforeIdx, int afterIdx, System.Action<int, int> changeAction)
    {
        gusetChangeAction = changeAction;
        InitPopupText();
        this.beforeIndex = beforeIdx;
        this.afterIndex = afterIdx;
        InitCharacter(itemGuest_1, grades_1, beforeIndex);
        InitCharacter(itemGuest_2, grades_2, afterIndex);
    }

    #region 텍스트 설정
    private void InitPopupText()
    {
        string titleString = Global._instance.GetString("p_frd_cg_1");
        for (int i = 0; i < titleText.Length; i++)
        {
            titleText[i].text = titleString;
        }
        beforeText.text = Global._instance.GetString("p_frd_cg_2");
        afterText.text = Global._instance.GetString("p_frd_cg_3");
        messageText.text = Global._instance.GetString("p_d_f_4");

        string btnString_1 = Global._instance.GetString("btn_1");
        string btnString_2 = Global._instance.GetString("btn_2");

        for (int i = 0; i < btnText_1.Length; i++)
        {
            btnText_1[i].text = btnString_1;
        }
        for (int i = 0; i < btnText_2.Length; i++)
        {
            btnText_2[i].text = btnString_2;
        }
    }
    #endregion

    #region 캐릭터 설정
    private void InitCharacter(UIItemGuest itemGuest, UISprite[] grades, int index)
    {
        CdnAdventureAnimal aData = ServerContents.AdvAnimals[index];
        int coustumeIdx = ManagerCharacter._instance.IsSpecialLobbyChar(index) ? ManagerAdventure.User.GetAnimalInstance(index).lookId : 0;
        string fileName = ManagerAdventure.GetAnimalProfileFilename(index, coustumeIdx);
        itemGuest.animalPicture.LoadCDN(Global.adventureDirectory, "Animal/", fileName);

        //등급
        for (int i = 0; i < grades.Length; i++)
        {
            if (i < aData.grade)
                grades[i].spriteName = "grade_star";
            else
                grades[i].spriteName = "grade_star_off";
        }
        
        //보상
        bool isExistReward = aData.lobby_rewards.Count > 0 ? true : false;
        itemGuest.reward.gameObject.SetActive(isExistReward);
        itemGuest.giftTimeRoot.SetActive(isExistReward);
        if (isExistReward == true)
        {
            itemGuest.reward.SetReward(aData.lobby_rewards[0]);
            itemGuest.giftTimeText.text = Global.GetTimeText_HHMM(aData.lobby_reward_cool_time, false);
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, alphaColor);
        }
        
        //스페셜 로비 배치 프레임
        itemGuest.specialFrame.SetActive(ManagerCharacter._instance.IsSpecialLobbyChar(index));
    }
    #endregion

    private void OnClickBtnChange()
    {
        if (gusetChangeAction != null)
            gusetChangeAction(beforeIndex, afterIndex);
        ManagerUI._instance.ClosePopUpUI();
    }
}
