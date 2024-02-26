using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_Sub_Install : MonoBehaviour
{
    [Header("ObjectLink")]
    [SerializeField] private UIUrlTexture texDeco;
    [SerializeField] private UIUrlTexture texDecoShadow;
    [SerializeField] private UISprite btnCheck;
    [SerializeField] private UILabel labelDeco;

    [Header("bCanDecoLink")]
    [SerializeField] private GameObject objBuyRoot;
    [SerializeField] private UILabel labelBuyCount;

    private PlusHousingModelData decoItem;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void OnLoadComplete(Texture2D r)
    {
        texDeco.mainTexture = r;
        texDeco.width = 80;
        texDeco.height = 80;

        texDecoShadow.mainTexture = r;
        texDecoShadow.width = 80;
        texDecoShadow.height = 80;
    }

    void SetDecoItem()
    {
        labelDeco.text = Global._instance.GetString($"h_{decoItem.housingIndex}_{decoItem.modelIndex}");
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconHousing", $"{decoItem.housingIndex}_{decoItem.modelIndex}", OnLoadComplete, true);
    }

    public void UpdateData(PlusHousingModelData cellData)
    {
        decoItem = cellData;

        //아이템 세팅
        SetDecoItem();

        int selectedModelIdx = ManagerHousing.GetSelectedHousingModelIdx(decoItem.housingIndex);

        if (selectedModelIdx == decoItem.modelIndex)
            btnCheck.spriteName = "ready_button_02_on";
        else
            btnCheck.spriteName = "ready_button_02_off";

        if(decoItem.active == 0)
        {
            objBuyRoot.SetActive(true);
            btnCheck.gameObject.SetActive(false);
            labelBuyCount.text = $"{decoItem.costCoin}";

            if (decoItem.costCoin == 0)
                objBuyRoot.SetActive(false);
        }
        else
        {
            objBuyRoot.SetActive(false);
            btnCheck.gameObject.SetActive(true);
        }

    }

    void OnClickBtnHousing()
    {

        if (decoItem.openMission != 0)
        {
            int missionIndex = decoItem.openMission;
            if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
            {
                HousingCantOpenPopup();
                return;
            }
        }

        //콜백으로 창 닫히고 하우징 열기.
        if (ManagerLobby.landIndex != ManagerHousing.GetHousingLandIndex(decoItem.housingIndex))
        {
            ManagerLobby._instance.MoveLand(ManagerHousing.GetHousingLandIndex(decoItem.housingIndex), () => OpenHousing());
        }
        else
        {
            UIPopupDiary._instance._callbackEnd += OpenHousing;
        }
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
        var focusPos = ManagerHousing.GetHousingFocusPosition(decoItem.housingIndex);
        LobbyEntryFocus.ResetFocusCandidates();

        if(decoItem.active == 0)
        {
            ManagerUI._instance.OpenPopupHousing(decoItem.housingIndex, -1, false, focusPos, UIDiaryController._instance.DiaryActionComplete);
            UIPopupHousing._instance._callbackOpen += () => UIPopupHousing._instance.PurchaseHousingItem(decoItem.housingIndex, decoItem.modelIndex);
        }
        else
        {
            ManagerUI._instance.OpenPopupHousing(decoItem.housingIndex, decoItem.modelIndex, false, focusPos, UIDiaryController._instance.DiaryActionComplete);
        }
    }
}
