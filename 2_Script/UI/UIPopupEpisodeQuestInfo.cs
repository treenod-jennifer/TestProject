using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupEpisodeQuestInfo : UIPopupBase
{
    public UILabel[] title;
    public UILabel[] subTitle;

    public UISprite questTargetSprite;

    public UILabel leftDesc;
    public UILabel rightDesc;
    public UILabel desc;

    [SerializeField] private GenericReward reward;

    public StageUIData item = null;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
    }

    public void Start()
    {
        string targetName = "";
        if (item.questData.type == QuestType.chapter_Duck)
        {
            questTargetSprite.spriteName = "Mission_DUCK_2";
            questTargetSprite.MakePixelPerfect();

            targetName = Global._instance.GetString("item_ep_1");
        }
        else if (item.questData.type == QuestType.chapter_Candy)
        {
            questTargetSprite.spriteName = "Mission_CANDY_2";
            questTargetSprite.MakePixelPerfect();

            targetName = Global._instance.GetString("item_ep_2");
        }

        //보상 이미지 셋팅
        reward.SetReward(ServerContents.ChapterMissions[item.questData.chapter].rewards[0]);

        title.SetText(Global._instance.GetString("ep_m_1").Replace("[1]", targetName));
        subTitle.SetText(Global._instance.GetString("ep_m_2").Replace("[1]", targetName));
        leftDesc.text = Global._instance.GetString("ep_m_3").Replace("[1]", targetName);
        rightDesc.text = Global._instance.GetString("ep_m_4");
        desc.text = Global._instance.GetString("ep_m_5").Replace("[1]", targetName).Replace("[n]", item.nChapterNumber.ToString());

    }

    void OnClickBtnGo()
    {

        this._callbackClose = () => { ManagerUI._instance.ClosePopupAndOpenPopupMission(); };
        OnClickBtnClose();

    }
}

