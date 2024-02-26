using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemInstallableGuest : MonoBehaviour
{
    public UIItemGuest itemGuest;
    public UISprite[] grades;
    public GameObject objNewIcon;

    private float alphaColor = 125f / 255f;
    private int characterIdx = 0;

    public void Start()
    {
        itemGuest.objBtn.SetActive(false);
        itemGuest.textBtn.text = Global._instance.GetString("btn_36");
    }

    public void UpdateItem(InstallabelGuestData guestData, bool isCanChangeCharacter)
    {
        this.characterIdx = guestData.characterIndex;
        CdnAdventureAnimal aData = ServerContents.AdvAnimals[characterIdx];
        int coustumeIdx = ManagerCharacter._instance.IsSpecialLobbyChar(characterIdx) ? ManagerAdventure.User.GetAnimalInstance(characterIdx).lookId : 0;
        string fileName = ManagerAdventure.GetAnimalProfileFilename(characterIdx, coustumeIdx);
        itemGuest.animalPicture.LoadCDN(Global.adventureDirectory, "Animal/", fileName);
        UpdateGradeIcon(aData.grade);
        UpdateReward(aData);
        objNewIcon.SetActive(guestData.isNewCharacter);
        itemGuest.objBtn.SetActive(isCanChangeCharacter);
    }

    private void UpdateGradeIcon(int grade)
    {
        for (int i = 0; i < grades.Length; i++)
        {
            if (i < grade)
                grades[i].spriteName = "grade_star";
            else
                grades[i].spriteName = "grade_star_off";
        }
    }

    private void UpdateReward(CdnAdventureAnimal aData)
    {
        bool isExistReward = aData.lobby_rewards.Count > 0 ? true : false;

        itemGuest.reward.gameObject.SetActive(isExistReward);
        itemGuest.giftTimeRoot.SetActive(isExistReward);
        
        //스페셜 로비 배치 캐릭터
        itemGuest.specialFrame.SetActive(ManagerCharacter._instance.IsSpecialLobbyChar(characterIdx));
        
        if (isExistReward == true)
        {
            UpdateRewardIcon(aData.lobby_rewards[0], aData.lobby_reward_cool_time);
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            itemGuest.gifeBg.color = new Color(1f, 1f, 1f, alphaColor);
        }
    }

    private void UpdateRewardIcon(Reward lobby_rewards, int coolTime)
    {
        //보상 설정
        itemGuest.reward.SetReward(lobby_rewards);
        //보상 시간
        itemGuest.giftTimeText.text = Global.GetTimeText_HHMM(coolTime, false);
    }

    private void OnClickBtnSelect()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        UIDiaryGuest._instance.OpenPopupLobbyGuestChange(characterIdx);
    }
}
