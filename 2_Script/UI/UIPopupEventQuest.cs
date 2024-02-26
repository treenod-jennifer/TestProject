using System.Collections.Generic;
using DG.Tweening;
using SideIcon;
using UnityEngine;
using PokoAddressable;

public class UIPopupEventQuest : UIPopupBase
{
    public const int QUEST_SPACE_SIZE = 150;
    
    public static UIPopupEventQuest _instance = null;
    
    private List<QuestGameData> dataList = new List<QuestGameData>();
    
    [SerializeField] private UIUrlTexture titleTexture;
    [SerializeField] private List<UIItemDiaryQuest> diaryItemList = new List<UIItemDiaryQuest>();

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
            _instance = null;
    }

    public void InitPopup()
    {
        dataList = new List<QuestGameData>();
        foreach (var data in ManagerEventQuest.eventQuestList)
        {
            if (data.state != QuestState.Finished && data.valueTime1 >= Global.GetTime())
                dataList.Add(data);
        }
        
        gameObject.AddressableAssetLoad<Texture2D>("local_ui/event_quest_title_BG", (texture) =>
        {
            titleTexture.SettingTextureScale(738, 383);
            titleTexture.mainTexture = texture;
        });

        for (int i = 0; i < dataList.Count; i++)
        {
            if (i < diaryItemList.Count)
            {
                diaryItemList[i].gameObject.SetActive(true);
                diaryItemList[i].InitBtnQuest(dataList[i]);
            }
        }
    }

    #region 연출 관련 (UIDiaryMission 연출과 동일)

    public void DoBtnQuest(int in_index, float _fBtnRemoveTime = 0.3f)
    {
        int deleteIndex = -1;

        for (int i = 0; i < diaryItemList.Count; i++)
        {
            UIItemDiaryQuest btn = diaryItemList[i];
            if (btn.qData.index == in_index)
            {
                deleteIndex = i;
                if (_fBtnRemoveTime > 0)
                {
                    btn.transform.DOScaleX(0f, _fBtnRemoveTime);
                    DOTween.ToAlpha(() => btn.mainSprite.color, x => btn.mainSprite.color = x, 0f, _fBtnRemoveTime);
                }

                StartCoroutine(CoAction(_fBtnRemoveTime, () => { btn.gameObject.SetActive(false); }));
                break;
            }
        }

        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].index == in_index)
            {
                dataList.RemoveAt(i);
                break;
            }
        }

        if (deleteIndex > -1)
        {
            StartCoroutine(CoAction(_fBtnRemoveTime, () =>
            {
                for (int i = deleteIndex; i < diaryItemList.Count; i++)
                    diaryItemList[i].transform
                        .DOLocalMoveY(diaryItemList[i].transform.localPosition.y + QUEST_SPACE_SIZE, 0.2f, true)
                        .SetEase(Ease.Linear);
            }));
            StartCoroutine(CoAction(0.3f, ClosePopupPhase));
        }
        else
            ClosePopupPhase();
    }

    #endregion

    private void ClosePopupPhase()
    {
        bCanTouch = true;
        
        IconEventQuest uiIcon = null;
        foreach (var icon in ManagerUI._instance.ScrollbarRight.icons)
            if (icon is IconEventQuest)
                uiIcon = (icon as IconEventQuest);
        
        if (dataList.Count < 1)
        {
            ClosePopUp();
            uiIcon?.SetText();
        }
        uiIcon?.SetNewIcon(true);
    }
}
