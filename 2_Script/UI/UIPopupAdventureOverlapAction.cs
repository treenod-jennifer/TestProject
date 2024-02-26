using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupAdventureOverlapAction : UIPopupAdventureSummonAction
{
    public override void InitPopup(ManagerAdventure.AnimalInstance orgAnimal, ManagerAdventure.AnimalInstance summoned, SummonType st, List<Reward> bonusRewards, CdnAdventureGachaProduct gachaData, Method.FunctionVoid callBack = null)
    {
        base.InitPopup(orgAnimal, summoned, st, bonusRewards, gachaData, callBack);

        this.progressInfoRoot.SetActive(false);
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();

        OverlapPopupListAllClose();
    }
    private void OverlapPopupListAllClose()
    {
        List<UIPopupBase> popupList = ManagerUI._instance._popupList;

        for (int i = popupList.Count - 1; i >= 0; --i)
        {
            if (popupList[i] is UIPopupMailBox != true)
            {
                popupList[i].ClosePopUp();
            }
            else
            {
                break;
            }
        }
    }
}
