using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemEpisode : MonoBehaviour
{
    [SerializeField] private UILabel epLabel;
    [SerializeField] private UISprite epFlowerAchievement;
    [SerializeField] private UIProgressBar epFlowerProgressBar;
    [SerializeField] private UILabel epFlowerProgressStep;
    [SerializeField] private GameObject epFlowerComplete;
    [SerializeField] private GameObject epFlowerProgressRoot;
    [SerializeField] private UISprite epQuestIcon;

    [SerializeField] private GameObject questGageRoot;
    [SerializeField] private GameObject lockStageRoot;
  

    private UIItemEpScrollView.EpisodeUIData item = null;

    public void UpdateData(UIItemEpScrollView.EpisodeUIData CellData)
    {
        if (CellData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        item = CellData;
        int stageCount = ManagerData._instance.chapterData[item.episodeCnt - 1]._stageCount;

        //에피소드 넘버
        epLabel.text = string.Format("{0}", item.episodeCnt);

        //꽃 이미지와 갯수 표현
        if(item.chapterState == ChapterState.Chapter_Lock)
        {
            questGageRoot.SetActive(false);
            lockStageRoot.SetActive(true);
        }
        else
        {
            questGageRoot.SetActive(true);
            lockStageRoot.SetActive(false);
            epFlowerAchievement.spriteName = SetFlowerSprite(item.waitFlower ? item.chapterState -1 : item.chapterState);
        }
            

        InitProgress(item.waitFlower ? stageCount : item.flowerCnt, stageCount);

        string iconName = "";
        iconName = GetMissionIconName();


        //에피소드 퀘스트 여부
        //item.questData != null 대신 사용할 퀘스트 보유 여부 변경 필요
        if (item.questData != null && item.chapterState != ChapterState.Chapter_Lock)
        {
            if (item.questProg)
                iconName += "2";
            else
                iconName += "1";

            epQuestIcon.spriteName = iconName;
            epQuestIcon.MakePixelPerfect();
            epQuestIcon.width = (int)(epQuestIcon.width * 0.5f);
            epQuestIcon.height = (int)(epQuestIcon.height * 0.5f);
            epQuestIcon.gameObject.SetActive(true);
        }
        else
        {
            epQuestIcon.gameObject.SetActive(false);
        }

    }

    //해당 에피소드 번호로 스크롤 이동
    private void IsMoveEpisode()
    {
        UIPopupStage._instance.SetScroll(ManagerData._instance.chapterData[item.episodeCnt - 1]._stageIndex);
        UIItemEpScrollView._instance.DropDown();
    }

    private void InitProgress(int flowerCount, int stageCnt)
    {
        //프로그레스 바 설정.
        float progressOffset = 100f / stageCnt;
        epFlowerProgressBar.value = (flowerCount * progressOffset) * 0.01f;
        //프로그레스 카운트 설정.
        if (item.chapterState == ChapterState.Chapter_Flower_Complete)
        {
            epFlowerProgressRoot.SetActive(false);
            epFlowerComplete.SetActive(true);
        }
        else
        {
            epFlowerProgressRoot.SetActive(true);
            epFlowerComplete.SetActive(false);
            epFlowerProgressStep.text = string.Format("{0}/{1}", flowerCount, stageCnt);
        }
    }

    //플라워 스프라이트 설정
    private string SetFlowerSprite(ChapterState chapterState)
    {
        if(chapterState == ChapterState.Chapter_Flower_White)
        {
            return "icon_flower_stroke_green";
        }
        else if(chapterState == ChapterState.Chapter_Flower_Blue)
        {
            return "icon_blueflower_stroke_green";
        }
        else
        {
            return "icon_redflower_stroke_green";
        }
    }

    string GetMissionIconName()
    {
        string iconName = "";
        //아이템 타입에 따라 이미지 이름 설정.
        //퀘스트 데이터로 비교하는 구분 변경 필요.
        if(item.questData == null)
        {
            return null;
        }
        else if (item.questData.type == QuestType.chapter_Duck)
        {
            iconName = "Mission_DUCK_";
        }
        else if (item.questData.type == QuestType.chapter_Candy)
        {
            iconName = "Mission_CANDY_";
        }
        else
        {
            return null;
        }
        return iconName;
    }
}
    