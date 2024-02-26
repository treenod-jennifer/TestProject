using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupFlowerReward : UIPopupBase
{
    Dictionary<int, FlowerRewardUIData> dicFlowerCount_Blue = new Dictionary<int, FlowerRewardUIData>();

    public UIReuseGrid_FlowerReward reuserGrid;
    public UIPanel scrollPanel;
    public UILabel[] title;
    public UILabel lodingText;

    [HideInInspector] public int _flowerType;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            scrollPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            scrollPanel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitFlowerCount(int topIndex)
    {   
        InitPopupText();

        List<ChapterData> listChapterData = new List<ChapterData>();
        List<StageData> listStageData = new List<StageData>();
        listChapterData = ManagerData._instance.chapterData;
        listStageData = ManagerData._instance._stageData;

        dicFlowerCount_Blue.Clear();

        if (listChapterData.Count <= 0 || listStageData.Count <= 0)
            return;
        
        int chapterIdx = 0;
        int flowerCount = 0;
        int chapterLastIndex = listChapterData[chapterIdx]._stageIndex + listChapterData[chapterIdx]._stageCount - 2;
        for (int i = 0; i < listStageData.Count; i++)
        {
            if (listStageData[i]._flowerLevel > _flowerType - 1)
            {
                flowerCount++;
            }
            if (i == chapterLastIndex)
            {
                Reward reward = null;

                switch(_flowerType)
                {
                    case 3:
                        reward = listChapterData[chapterIdx].allWhiteFlowerReward[0];
                        break;
                    case 4:
                        reward = listChapterData[chapterIdx].allBlueFlowerReward[0];
                        break;
                    case 5:
                        reward = listChapterData[chapterIdx].allPokoFlowerReward[0];
                        break;
                }

                int value = reward.value;
                int type = reward.type;
                int getState = 0;
                if (chapterIdx <= ServerRepos.UserChapters.Count - 1)
                {
                    getState = ServerRepos.UserChapters[chapterIdx].isGetBlueFlowerReward;
                }

                //보상타입은 더미.
                FlowerRewardUIData data 
                    = new FlowerRewardUIData(chapterIdx, listChapterData[chapterIdx]._stageCount, flowerCount, (RewardType)type, value, getState, _flowerType);
                
                dicFlowerCount_Blue.Add(chapterIdx, data);
                chapterIdx++;
                if (chapterIdx >= listChapterData.Count)
                    break;
                flowerCount = 0;
                chapterLastIndex = listChapterData[chapterIdx]._stageIndex + listChapterData[chapterIdx]._stageCount - 2;
            }
        }
        lodingText.gameObject.SetActive(false);
        reuserGrid.InitReuseGrid((listChapterData.Count - 1), topIndex, dicFlowerCount_Blue);
    }

    private void InitPopupText()
    {
        string titleText = Global._instance.GetString($"p_f_r_1");
        titleText = titleText.Replace("[1]", Global._instance.GetString($"item_flw_{_flowerType - 2}"));
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }
    }
}
