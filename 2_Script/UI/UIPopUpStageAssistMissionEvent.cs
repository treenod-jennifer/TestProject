using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;

public class UIPopUpStageAssistMissionEvent : UIPopupBase
{
    [SerializeField] private List<UILabel> labelTitle;
    [SerializeField] private UILabel labelEndTs;

    [SerializeField] private UIUrlTexture texMainImage;

    [SerializeField] private UIItemStageAssistMission itemStageAssistMission;

    [SerializeField] private UILabel labelLineValue;
    [SerializeField] private UILabel labelMessage;

    private void Start()
    {
        InitData();

        // 이벤트 남은 시간 세팅
        labelEndTs.text = Global.GetTimeText_MMDDHHMM_Plus1(ServerContents.StageAssistMissionEvent.endTs);
    }

    private void InitData()
    {
        itemStageAssistMission.SetStageAssist();
        labelMessage.text = ManagerStageAssistMissionEvent.Instance.GetMessage();

        for (int i = 0; i < labelTitle.Count; i++)
        {
            labelTitle[i].text = Global._instance.GetString($"p_sa_{(ManagerStageAssistMissionEvent.IsBlockMissionType() ? 1 : 2)}");
        }

        texMainImage.SettingTextureScale(715, 565);
        this.gameObject.AddressableAssetLoad<Texture>($"local_ui/stage_assist_mission_bg_0{(ManagerStageAssistMissionEvent.IsBlockMissionType() ? 1 : 2)}",(x) =>
        {
            texMainImage.mainTexture = x;
            texMainImage.width = 715;
            texMainImage.height = 565;
        });

        if(ServerContents.StageAssistMissionEventDetails.TryGetValue(ServerRepos.UserStageAssistMissionEvent.missionIndex, out var value))
        {
            labelLineValue.text = value.targetCount.ToString();
        }
    }

    public void OpenPopupReadyLastStage()
    {
        if (ManagerStageAssistMissionEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopupReadyLastStage();
        }
        else
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false);
            systemPopup.SortOrderSetting();
        }
    }
}
