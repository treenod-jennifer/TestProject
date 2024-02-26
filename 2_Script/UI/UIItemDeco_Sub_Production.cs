using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemDeco_Sub_Production : MonoBehaviour
{
    public PlusHousingModelData decoItem;

    [Header("ObjectLink")]
    [SerializeField] private UIUrlTexture texDeco;
    [SerializeField] private UIUrlTexture texDecoShadow;
    [SerializeField] private UILabel      labelDeco;
    [SerializeField] private GameObject   newIcon;

    [Header("GetButtonLink")]
    [SerializeField] public  GameObject  btnGet;
    [SerializeField] private UILabel     labelGetButton;
    [SerializeField] private UISprite    sprGetButton;
    [SerializeField] private BoxCollider colGetButton;
    [SerializeField] private UISprite    activeGetButton;

    [Header("EventLink")]
    [SerializeField] private UISprite   eventBack;
    [SerializeField] private GameObject eventRibbon;
    [SerializeField] private UILabel    eventTime;

    [Header("MaterialsArray")]
    [SerializeField] private UIItemDiaryMaterial[] decoItemMaterials;

    [HideInInspector] public bool bClearButton = false;

    private Coroutine BackChange = null;

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

    void InitDecoItem()
    {
        newIcon.SetActive(false);
        labelDeco.color = new Color32(58, 94, 145, 255);

        btnGet.SetActive(false);
        sprGetButton.spriteName = "arrow_button_red";
        activeGetButton.enabled = false;

        eventBack.gameObject.SetActive(false);
        eventRibbon.SetActive(false);
        eventTime.gameObject.SetActive(false);

        colGetButton.enabled = true;
    }

    public void UpdateData(PlusHousingModelData cellData)
    {
        decoItem = cellData;

        //아이템 상태 초기화
        InitDecoItem();

        //아이템 이미지, 이름 세팅
        SetDecoItem();

        //아이템 재료 세팅
        int matCount = 0;
        int needCount = 0;

        for (int i = 0; i < decoItemMaterials.Length; i++)
        {
            decoItemMaterials[i].gameObject.SetActive(true);

            if (decoItem.material.Count < i + 1)
            {
                decoItemMaterials[i].gameObject.SetActive(false);
                continue;
            }
            needCount += decoItem.material[i]._count;
            matCount += decoItemMaterials[i].InitDiaryMaterial(decoItem.material[i]);
        }

        //재료 수 다 채웠을 때
        if(needCount == matCount)
        {
            btnGet.SetActive(true);
            StartCoroutine(CoHousingActive());
        }

        //현재 데코가 이벤트
        if (decoItem.expire_ts > 0)
        {
            SetEventDeco(needCount == matCount);
        }
        //현재 데코가 새로 생긴 데코
        else if (UIDiaryController._instance.CheckNewIconHousing(decoItem.housingIndex, decoItem.modelIndex))
        {
            newIcon.SetActive(true);
            StartCoroutine(CoMoveIcon(newIcon));
        }

        labelGetButton.text = Global._instance.GetString("btn_28");
    }

    private void SetEventDeco(bool bCanGetDeco)
    {
        //이벤트 오브젝트 활성화
        eventBack.gameObject.SetActive(true);
        eventRibbon.SetActive(true);
        eventTime.gameObject.SetActive(true);

        labelDeco.color = new Color32(235, 99, 41, 255);

        //현재 데코가 이벤트 데코지만 획득을 하지 못할 때.
        if(bCanGetDeco == false)
        {
            btnGet.SetActive(true);
            colGetButton.enabled = false;
            sprGetButton.spriteName = "arrow_button_gray";
            labelGetButton.effectColor = new Color32(90, 90, 90, 255);
        }

        //코루틴(배경, 시간, 리본).
        //해당 오브젝트에서 데이터만 변경이 될 때 계속해서 코루틴을 부르는 경우가 생김.
        if (BackChange == null)
            BackChange = StartCoroutine(CoBackChange());
        StartCoroutine(CoEventTimer());
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
            eventTime.text = Global.GetTimeText_DDHHMM(decoItem.expire_ts);
            if (Global.LeftTime(decoItem.expire_ts) <= 0)
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

    private IEnumerator CoHousingActive()
    {
        float color = 70f / 255f;
        activeGetButton.enabled = true;
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            float ratio = (0.2f + Mathf.Cos(Time.time * 10f) * 0.1f);
            activeGetButton.color = new Color(color, color, color, ratio);
            yield return null;
        }
        yield return null;
        activeGetButton.enabled = false;
    }

    void OnClickBtnGet()
    {
        //이벤트 타입일 경우, 해당 하우징이 열려있는지 검사.
        if (decoItem.expire_ts > 0)
        {
            int missionIndex = decoItem.openMission;
            if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
            {
                HousingCantOpenPopup();
                return;
            }
        }

        ServerAPI.MaterialCombine(decoItem.housingIndex, decoItem.modelIndex, recvCombineMaterial);
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
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.Deco_Get);
            PlusHousingModelData.SetUserData();
            MaterialData.SetUserData();
            HousingUserData.SetUserData();

            this.GetComponentInChildren<TweenScale>().enabled = true;
            
            bClearButton = true;

            //이벤트 아이템이라면.
            UIPopupDiary._instance.SettingEventTap(1, ManagerHousing.GetUnfinishedEventItemCount() > 0);

            UIDiaryController._instance.UpdateProgressHousingData();

            //그로씨
            string fileName = string.Format("h_{0}_{1}", decoItem.housingIndex, decoItem.modelIndex);
            string ItemName = string.Format("DECO-{0}-{1}", decoItem.housingIndex, decoItem.modelIndex);

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


            for (int i = 0; i < decoItem.material.Count; i++)
            {
                if (decoItem.material[i] == null) break;

                //재료
                var useMaterialA = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                               "MATERIAL_" + decoItem.material[i]._index.ToString(),
                              "material",
                              -decoItem.material[i]._count,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_SYNTHESIS_MATERIAL
                          );
                var docMaterial = JsonConvert.SerializeObject(useMaterialA);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docMaterial);
            }

            //랜드가 다를 경우 처리
            if (ManagerLobby.landIndex != ManagerHousing.GetHousingLandIndex(decoItem.housingIndex))
            {
                ManagerLobby._instance.MoveLand(ManagerHousing.GetHousingLandIndex(decoItem.housingIndex), () => OpenHousing() );
            }
            else
            {
                UIPopupDiary._instance._callbackEnd += OpenHousing;
                UIDiaryController._instance.UpdateQuestData(true);
            }
        }
        else
        {
            //통신 실패했으면 터치 다시 열기.
            UIPopupDiary._instance.bCanTouch = true;
        }
    }

    void SetDecoItem()
    {
        labelDeco.text = Global._instance.GetString($"h_{decoItem.housingIndex}_{decoItem.modelIndex}");
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconHousing", $"{decoItem.housingIndex}_{decoItem.modelIndex}", OnLoadComplete, true);
    }

    void OpenHousing()
    {
        {
            ManagerHousing.OnBuyNewHousing(decoItem.housingIndex);
        }

        var focusPos = ManagerHousing.GetHousingFocusPosition(decoItem.housingIndex);
        LobbyEntryFocus.ResetFocusCandidates();
        ManagerUI._instance.OpenPopupHousing(decoItem.housingIndex, decoItem.modelIndex, true, focusPos, UIDiaryController._instance.DiaryActionComplete);
    }

    public void PostProductionDeco()
    {
        Destroy(UIDiaryDeco._instance.gameObject);
        UIPopupDiary._instance.ClosePopUp();
    }
}
