using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopupCompletePackage : UIPopupBase
{
    public static UIPopupCompletePackage instance = null;

    [SerializeField] UIPanel scrollPanel;
    [SerializeField] GameObject bannerRoot;

    [SerializeField] GameObject contentsBasis;

    [SerializeField] GameObject scrollBarObj;
    [SerializeField] UIScrollBar scrollBar;

    [SerializeField] UIPokoButton btnClose;

    [SerializeField] UIPokoButton btnGet_Off;
    [SerializeField] UIPokoButton btnGet;
    [SerializeField] UIGrid completeRewardsGrid;
    [SerializeField] List<GenericReward> completeRewards;
    [SerializeField] List<GameObject> interRewardPlus;

    [SerializeField] GameObject refPackage;

    [SerializeField] UIScrollView draggablePanel;

    [SerializeField] UIItemCompletePackageGage packageGage;

    CdnCompletePackage compPkgData;

    List<int> needPackageList = new List<int>();
    List<UIItemPackage> objectList = new List<UIItemPackage>();

    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    [SerializeField] private UILabel labelTimer;
    
    public bool bAutoOpen = false;

    private string TermsOfUse
    {
        get
        {
            string value = Global._instance.GetString("p_dia_4");

            if (string.IsNullOrEmpty(value))
                objTermsOfUse.SetActive(false);
            else
                objTermsOfUse.SetActive(true);

            return value;
        }
    }

    private string Precautions
    {
        get
        {
            string value = Global._instance.GetString("p_dia_3");

            if (string.IsNullOrEmpty(value))
                objPrecautions.SetActive(false);
            else
                objPrecautions.SetActive(true);

            return value;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        base.OnDestroy();
    }

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(CdnCompletePackage data)
    {
        this.compPkgData = data;
    }

    private IEnumerator Start()
    {
        this.btnGet.SetLabel(Global._instance.GetString("btn_10"));
        this.btnGet_Off.SetLabel(Global._instance.GetString("btn_10"));

        labelTimer.gameObject.SetActive(true);
        labelTimer.text = Global.GetTimeText_MMDDHHMM_Plus1(compPkgData.expired_at);
            
        scrollPanel.clipOffset = new Vector2(0f, -19f);
        scrollPanel.SetRect(2f,0f,670f, 777f);
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        for (int i = 0; i < this.completeRewards.Count; ++i)
        {
            completeRewards[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < this.interRewardPlus.Count; ++i)
        {
            interRewardPlus[i].SetActive(false);
        }
        
        for (int i = 0; i < compPkgData.rewards.Count && i < this.completeRewards.Count; ++i)
        {
            completeRewards[i].gameObject.SetActive(true);
            completeRewards[i].SetReward(compPkgData.rewards[i]);
        }

        for (int i = 0; i < compPkgData.rewards.Count - 1 && i < this.interRewardPlus.Count; ++i)
        {
            interRewardPlus[i].SetActive(true);
        }

        this.completeRewardsGrid.cellWidth = 380 / (compPkgData.rewards.Count + 2);

        int packageCount = 0;
        for (int i = 0; i < compPkgData.packages.Count; ++i)
        {
            if (ServerContents.Packages.ContainsKey(compPkgData.packages[i]) == false)
                continue;
            var packageData = ServerContents.Packages[compPkgData.packages[i]];
            var packageEntry = AddPackage(packageData);

            objectList.Add(packageEntry);

            while( packageEntry.textureLoadCompleted == false)
            {
                yield return null;
            }
            packageEntry.gameObject.SetActive(true);
            packageCount++;
        }

        packageGage.InitGage(packageCount, GetBuyItemCount());

        GetButtonValidationCheck();

        yield return ArrangePage();

        

        yield break;
    }

    void ClearPage()
    {
        for (int i = 0; i < this.objectList.Count; ++i)
        {
            Destroy(objectList[i].gameObject);
        }
        objectList.Clear();
    }

    IEnumerator ArrangePage()
    {
        yield return null;

        #region Item 위치 정렬
        int offset = 0;
        for (int i = 0; i < this.objectList.Count; ++i)
        {
            objectList[i].gameObject.transform.localPosition = new Vector3(0, offset);

            if (i + 1 < objectList.Count)
            {
                offset -= objectList[i + 1].texture.height;
                offset -= 30;
            }
        }
        #endregion

        #region 미구매 아이템 위치로 스크롤 세팅
        int notBuyItemIdx = 0;
        for (int i = 0; i < objectList.Count; i++)
        {
            if (!objectList[i].isBuy())
            {
                notBuyItemIdx = i;
                break;
            }
        }

        int scrollHeight = 0;
        int notBuyItemHeight = 0;
        for (int i = 0; i < objectList.Count; ++i)
        {
            if(i == notBuyItemIdx)
                notBuyItemHeight = scrollHeight;

            scrollHeight += objectList[i].texture.height;
            if(i + 1 < objectList.Count)
                scrollHeight += 30;
        }

        draggablePanel.UpdateScrollbars();
        draggablePanel.verticalScrollBar.value = notBuyItemHeight / (scrollHeight - scrollPanel.height + scrollPanel.clipSoftness.y * 2.0f);
        #endregion
    }

    UIItemPackage AddPackage(CdnShopPackage data)
    {
        var newPackage = Instantiate(refPackage);
        //newPackage.SetActive(true);
        newPackage.transform.parent = this.bannerRoot.gameObject.transform;
        newPackage.transform.localScale = Vector3.one;
        newPackage.transform.localPosition = Vector3.zero;

        UIItemPackage package = newPackage.GetComponent<UIItemPackage>();
        package.Init(data, autoOpen: bAutoOpen);
        return package;
    }

    void OnBtnGet_Off()
    {

    }

    private void OnBtnGet()
    {
        if (this.bCanTouch == false)
        {
            return;
        }

        bCanTouch = false;

        if (ServerRepos.UserCompletePackages.Exists(x => x.idx == compPkgData.idx))
        {
            btnGet.gameObject.SetActive(false);
            btnGet_Off.gameObject.SetActive(true);

            bCanTouch = true;
            return;
        }

        ServerAPI.GetCompletePackageReward(this.compPkgData.idx, 
            (resp) => 
            {
                bCanTouch = true;
                if ( resp.IsSuccess)
                {
                    //그로시
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.COMPLETE_PACK,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.COMPLETE_PACK_GET_REWARD,
                            compPkgData.userSegment == 0 ? "PU" : "NPU",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }
                    
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_35"), false, () => { bCanTouch = true; });
                }
                GetButtonValidationCheck();
                ManagerUI._instance.UpdateUI();
            }
            );

    }

    void GetButtonValidationCheck()
    {
        for(int i = 0; i < objectList.Count; ++i)
        {
            objectList[i].CheckValidationBuyButton();
        }

        packageGage.SetGage(GetBuyItemCount());

        // 이미 보상을 받았으면 비활성화
        if( ServerRepos.UserCompletePackages.Exists(x => x.idx == compPkgData.idx ) )
        {
            btnGet.gameObject.SetActive(false);
            btnGet_Off.gameObject.SetActive(true);

            return;
        }

        for (int i = 0; i < this.compPkgData.packages.Count; ++i)
        {
            var userPkg = ServerRepos.UserShopPackages.Find(x => x.idx == compPkgData.packages[i]);
            if( userPkg == null)
            {
                btnGet.gameObject.SetActive(false);
                btnGet_Off.gameObject.SetActive(true);

                return;
            }
        }

        btnGet.gameObject.SetActive(true);
        btnGet_Off.gameObject.SetActive(false);
    }

    protected override void OnFocus(bool focus)
    {
        if( focus )
        {
            Debug.Log("UIPopupCompletePackage.OnFocus");

            GetButtonValidationCheck();
        }
    }

    private int GetBuyItemCount()
    {
        int buyCount = 0;
        foreach(var buyItem in ServerRepos.UserShopPackages)
        {
            if (compPkgData.packages.Exists(x => x == buyItem.idx))
                buyCount++;
        }

        return buyCount;
    }

    protected override void OnClickBtnClose()
    {
        //그로씨
        if (ManagerLobby._instance.IsLobbyComplete == false)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                    null,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FAIL
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        base.OnClickBtnClose();
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
}
