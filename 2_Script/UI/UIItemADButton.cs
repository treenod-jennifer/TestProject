using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemADButton : MonoBehaviour
{
    [SerializeField] private AdManager.AdType adType;

    [Tooltip("AD_3 타입인 경우 박스 데이터를 가져오기 위한 타켓")]
    [SerializeField] private UIPopupTimeGiftBox box;

    [Tooltip("AD_4 타입인 경우 미션 데이터를 가져오기 위한 타켓")]
    [SerializeField] private UIItemDiaryMission mission;

    [Tooltip("AD_6, 7 타입인 경우 이미지 위치 변경을 위한 타켓")]
    [SerializeField] private GameObject objTextureImage;

    //광고 없을 때, 버튼 제거 여부
    [SerializeField] private bool isAutoRemoveButton = true;

    private void Start()
    {
        if (isAutoRemoveButton == true)
            gameObject.SetActive(AdManager.ADCheck(adType));
    }

    public void SetADType(AdManager.AdType _type)
    {
        adType = _type;
    }

    public void OpenADView()
    {
        switch (adType)
        {
            case AdManager.AdType.None:
                break;
            case AdManager.AdType.AD_1:
                break;
            case AdManager.AdType.AD_2:
                break;
            case AdManager.AdType.AD_3:
                ManagerUI._instance.OpenPopup<UIPopupADView>
                (
                    (popup) =>
                    {
                        popup.SetReduceGiftboxTime
                        (
                            (int)box.BoxID,
                            box.BoxTime,
                            ServerContents.AdInfos[(int)AdManager.AdType.AD_3].useInterval
                        );
                    }
                );
                break;
            case AdManager.AdType.AD_4:
                ManagerUI._instance.OpenPopup<UIPopupADView>
                (
                    (popup) =>
                    {
                        popup.SetMissionTime
                        (
                            mission.mData.index, 
                            mission.mData.clearTime, 
                            ServerContents.AdInfos[(int)AdManager.AdType.AD_4].useInterval
                        );
                    }
                );
                break;
            case AdManager.AdType.AD_5:
                AdManager.ShowAD_TurnRelayRechargeAPByAd();
                break;
            case AdManager.AdType.AD_6:
                AdManager.ShowAD_RequestAdReward(adType, (isSuccess, reward) =>
                {
                    if (AdManager.ADCheck(adType) == false)
                    {
                        ADCheck();
                    }
                });
                break;
            case AdManager.AdType.AD_7:
            case AdManager.AdType.AD_15:
                ManagerUI._instance.OpenPopup<UIPopupADView>
                (
                    (popup) =>
                    {
                        popup.SetRequestAdReward(adType, ServerContents.AdInfos[(int)adType].rewards);
                        popup._callbackClose += () =>
                        {
                            if (AdManager.ADCheck(adType) == false)
                            {
                                ADCheck();
                            }
                        };
                    }
                );
                break;
            case AdManager.AdType.AD_8:
                ManagerUI._instance.OpenPopup<UIPopupADView>
                (
                    (popup) =>
                    { 
                        popup.SetMaterialAdReward(() => Destroy(ObjectMaterialbox._materialboxList[0].gameObject));
                    }
                );
                break;
            default:
                break;
        }
    }

    private void ADCheck()
    {
        switch (adType)
        {
            case AdManager.AdType.AD_6:
                UIItemShopClover._instance.ADCheck();
                break;
            case AdManager.AdType.AD_7:
                UIItemShopWing._instance.ADCheck();
                break;
            case AdManager.AdType.AD_15:
                UIItemShopDiamond._instance.ADCheck();
                break;
        }
    }
}
