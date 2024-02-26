using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIItemDiaryHousing_C : MonoBehaviour
{
    public UIUrlTexture itemTexture;
    public UIUrlTexture itemTextureShadow;
    public UISprite     btnCheck;
    public UISprite     mainSprite;
    public UILabel      itemName;

    [HideInInspector]
    public PlusHousingModelData plusData;

    public void InitItemHousing_C(PlusHousingModelData data)
    {
        plusData = data;

        string fileName = data.housingIndex + "_" + data.modelIndex;
        itemTexture.SuccessEvent += SettingShadowDeco;
        itemTexture.SettingTextureScale(80, 80);
        itemTexture.LoadCDN(Global.gameImageDirectory, "IconHousing/", fileName);

        fileName = string.Format("h_{0}_{1}", data.housingIndex, data.modelIndex);
        itemName.text = Global._instance.GetString(fileName);

        int selectedModelIdx = ManagerHousing.GetSelectedHousingModelIdx(plusData.housingIndex);

        if(selectedModelIdx == plusData.modelIndex)
            btnCheck.spriteName = "ready_button_02_on";
        else
            btnCheck.spriteName = "ready_button_02_off";
    }

    private void SettingShadowDeco()
    {
        if (itemTexture == null)
            return;
        itemTextureShadow.mainTexture = itemTexture.mainTexture;
        itemTextureShadow.width = 80;
        itemTextureShadow.height = 80;
    }

    void OnClickBtnHousing()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        
        if( plusData.openMission != 0 )
        {
            int missionIndex = plusData.openMission;
            if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
            {
                HousingCantOpenPopup();
                return;
            }
        }
        //터치막음.
        UIPopupDiary._instance.bCanTouch = false;

        //콜백으로 창 닫히고 하우징 열기.
        UIPopupDiary._instance._callbackEnd += OpenHousing;
        ManagerUI._instance.ClosePopUpUI();
    }

    void HousingCantOpenPopup()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_16"), false);
        popupSystem.SetResourceImage("Message/tired");
    }

    void OpenHousing()
    {
        var focusPos = ManagerHousing.GetHousingFocusPosition(plusData.housingIndex);
        LobbyEntryFocus.ResetFocusCandidates();
        ManagerUI._instance.OpenPopupHousing(plusData.housingIndex, plusData.modelIndex, false, focusPos, UIDiaryController._instance.DiaryActionComplete);
    }
}
