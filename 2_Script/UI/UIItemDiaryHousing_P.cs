using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemDiaryHousing_P : MonoBehaviour
{
    public GameObject   buttonGet;
    public UISprite     buttonSprite;
    public BoxCollider  buttonCollider;
    public UILabel      buttonGetText;

    public UISprite     mainSprite;
    public UISprite     activeButton;
    public UIUrlTexture itemTexture;
    public UIUrlTexture itemTextureShadow;
    public UILabel      itemName;
    public GameObject   newIcon;

    //이벤트 관련.
    public UISprite     eventBack;
    public GameObject   eventRibbon;
    public UILabel      eventTime;

    public UIItemDiaryMaterial[] diaryMaterials;

    [HideInInspector]
    public PlusHousingModelData plusData;

    [HideInInspector]
    public bool bClearButton = false;

    public void InitItemHousing_P(PlusHousingModelData data, bool bNewIcon)
    {
        plusData = data;
        string fileName = string.Format("{0}_{1}", plusData.housingIndex, plusData.modelIndex);
        itemTexture.SuccessEvent += SettingShadowDeco;
        itemTexture.SettingTextureScale(80, 80);
        itemTexture.LoadCDN(Global.gameImageDirectory, "IconHousing/", fileName);

        int matCount = 0;
        int needCount = 0;
        for (int i = 0; i < diaryMaterials.Length; i++)
        {
            if (i + 1 > plusData.material.Count)
            {
                diaryMaterials[i].gameObject.SetActive(false);
                continue;
            }
            needCount += plusData.material[i]._count;
            matCount += diaryMaterials[i].InitDiaryMaterial(plusData.material[i]);
        }
        fileName = string.Format("h_{0}_{1}", plusData.housingIndex, plusData.modelIndex);
        itemName.text = Global._instance.GetString(fileName);

        bool bCanGetDeco = false;
        //재료 수 다 채웠을 때는 버튼활성화.
        if (matCount == needCount)
        {
            bCanGetDeco = true;
            buttonGet.SetActive(true);
            StartCoroutine(CoHousingActive());
        }

        //현재 데코가 이벤트형일 경우.
        if (plusData.expire_ts > 0)
        {
            SettingEventDeco(bCanGetDeco);
        }
        //현재 데코가 새로 생긴 데코일 경우.
        else if (bNewIcon == true)
        {
            newIcon.SetActive(true);
            StartCoroutine(CoMoveIcon(newIcon));
        }

        buttonGetText.text = Global._instance.GetString("btn_28");
    }

    public void OpenHouising_P()
    {
        UIDiaryStorage._instance.bGetDeco = true;
    }

    private void SettingShadowDeco()
    {
        if (itemTexture == null)
            return;
        itemTextureShadow.mainTexture = itemTexture.mainTexture;
        itemTextureShadow.width = 80;
        itemTextureShadow.height = 80;
    }

    private IEnumerator CoHousingActive()
    {
        float color = 70f / 255f;
        activeButton.enabled = true;
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            //터치 불가 상태일 때는 멈춤.
            if (UIDiaryStorage._instance.bGetDeco == true)
                break;

            float ratio = (0.2f + Mathf.Cos(Time.time * 10f) * 0.1f);
            activeButton.color = new Color(color, color, color, ratio);
            yield return null;
        }
        yield return null;
        activeButton.enabled = false;
    }

    private void SettingEventDeco(bool bCanGetDeco)
    {
        eventBack.gameObject.SetActive(true);
        eventRibbon.SetActive(true);
        eventTime.gameObject.SetActive(true);

        //글자 색 변경.
        itemName.color = new Color(235f / 255f, 99f / 255f, 41f / 255f);

        //획득 불가능 한 상태일 경우, get 버튼 비활성화.
        if (bCanGetDeco == false)
        {
            buttonGet.SetActive(true);
            buttonCollider.enabled = false;
            buttonSprite.spriteName = "arrow_button_gray";
            float color = 90f / 255f;
            buttonGetText.effectColor = new Color(color, color, color);
        }

        //코루틴(배경, 시간, 리본).
        StartCoroutine(CoBackChange());
        StartCoroutine(CoEventTimer());
        StartCoroutine(CoMoveIcon(eventRibbon));
    }

    private IEnumerator CoBackChange()
    {
        bool bBack1 = false;
        float timer = 0.0f;
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            //배경 뒤 반짝이 전환.
            timer += Global.deltaTime * 1.0f;
            if (timer >= 0.15f)
            {
                if (bBack1 == true)
                {
                    eventBack.spriteName = "event_deco_bg_02";
                    bBack1 = false;
                }
                else
                {
                    eventBack.spriteName = "event_deco_bg_01";
                    bBack1 = true;
                }
                timer = 0.0f;
            }
            yield return null;
        }
        yield return null;
    }

    private IEnumerator CoEventTimer()
    {
        while (gameObject.activeInHierarchy == true)
        {
            eventTime.text = Global.GetTimeText_DDHHMM(plusData.expire_ts);
            if (Global.LeftTime(plusData.expire_ts) <= 0)
            {
                eventTime.text = "00:00:00";
                break;
            }
            yield return null;
        }
    }

    private IEnumerator CoMoveIcon(GameObject obj)
    {
        Vector3 initPos = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z);
        
        while (gameObject.activeInHierarchy == true)
        {
            obj.transform.localPosition
                = new Vector3(initPos.x, initPos.y + Mathf.Abs(Mathf.Cos(Time.time * 4f) * 3f), initPos.z);
            yield return null;
        }
        yield return null;
    }

    void OnClickBtnGet()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return;

        //이벤트 타입일 경우, 해당 하우징이 열려있는지 검사.
        if (plusData.expire_ts > 0)
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
        
        ServerAPI.MaterialCombine(plusData.housingIndex, plusData.modelIndex, recvCombineMaterial);
    }

    void HousingCantOpenPopup()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_16"), false);
        popupSystem.SetResourceImage("Message/tired");
    }

    void recvCombineMaterial(CombineMaterialResp resp)
    {
        if (resp.IsSuccess)
        {
            {
                ManagerHousing.OnBuyNewHousing(plusData.housingIndex);
            }
            UIDiaryStorage._instance.bGetDeco = true;

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.Deco_Get);
            PlusHousingModelData.SetUserData();
            MaterialData.SetUserData();
            HousingUserData.SetUserData();
            //옆으로 사라지는연출.
            UIDiaryStorage._instance.GetItemProgress(plusData.housingIndex, plusData.modelIndex, 0.3f);
            UIPopupDiary._instance._callbackEnd += OpenHousing;
            bClearButton = true;
            
            //이벤트 아이템이라면.
            if (plusData.expire_ts > 0)
            {
                UIDiaryStorage._instance.PurchaseEventItem();
            }
            UIDiaryController._instance.UpdateProgressHousingData();

            //그로씨
            string fileName = string.Format("h_{0}_{1}", plusData.housingIndex, plusData.modelIndex);
            string ItemName = string.Format("DECO-{0}-{1}", plusData.housingIndex, plusData.modelIndex);

            string housingName = Global._instance.GetString(fileName);

            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.SYNTHESIS,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                          ItemName,//plusData.housingIndex.ToString(),
                          fileName,
                          1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SYNTHESIS_RESULT
                      );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);


            for (int i = 0; i < plusData.material.Count; i++)
            {
                if (plusData.material[i] == null) break;

                //재료
                var useMaterialA = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                               "MATERIAL_" + plusData.material[i]._index.ToString(),
                              "material",
                              -plusData.material[i]._count,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_SYNTHESIS_MATERIAL
                          );
                var docMaterial = JsonConvert.SerializeObject(useMaterialA);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docMaterial);
            }
        }
        else
        {
            //통신 실패했으면 터치 다시 열기.
            UIPopupDiary._instance.bCanTouch = true;
        }
    }

    void OpenHousing()
    {
        var focusPos = ManagerHousing.GetHousingFocusPosition(plusData.housingIndex);
        LobbyEntryFocus.ResetFocusCandidates();
        ManagerUI._instance.OpenPopupHousing(plusData.housingIndex, plusData.modelIndex, true, focusPos, UIDiaryController._instance.DiaryActionComplete);
    }
}
