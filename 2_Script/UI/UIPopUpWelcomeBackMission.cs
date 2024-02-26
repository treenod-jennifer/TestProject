using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

public class UIPopUpWelcomeBackMission : UIPopupBase
{
    [Header("VipLink")]
    [SerializeField] protected UILabel labelWecomeMissionEndTs;
    [SerializeField] protected UILabel[] labelVipPassPrice;
    [SerializeField] protected GameObject objVipPassBuyBtn;
    [SerializeField] protected GameObject objVipPassCheck;
    [SerializeField] protected GameObject objVipPassBlock;

    [Header("MissionLink")]
    [SerializeField] protected UIGrid gridMissionItem;
    [SerializeField] protected GameObject welcomeMissionItem;

    [Header("PanelLink")]
    [SerializeField] protected UIPanel panelScroll;
    [SerializeField] protected UIPanel panelBlock;

    [Header("LanPage")]
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;

    [Header("WerlcomBackMissionLink")]
    [SerializeField] private List<GameObject> titleRoot;
    [SerializeField] private List<GameObject> passRoot;
    [SerializeField] private UIPanel panelTitle;

    public bool bAutoOpen = false;
    
    #region


    protected PackageBuyRoutine buyRoutine = null;

    protected bool VipPassBuy
    {
        set
        {
            if (value)
            {
                objVipPassBuyBtn.SetActive(false);
                objVipPassCheck.SetActive(true);
                objVipPassBlock.SetActive(false);
            }
            else
            {
                objVipPassBuyBtn.SetActive(true);
                objVipPassCheck.SetActive(false);
                objVipPassBlock.SetActive(true);
            }
        }
    }

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


    private int WelcomeBackPassIndex
    {
        get { return ManagerWelcomeBackEvent.GetWelcomeBackMission().packageIdx; }
    }
    
    protected virtual void Start()
    {
        SetWelcomeEventTime();
        InitData();

        labelTermsOfUse.text = TermsOfUse;
    }

    override protected void OnDestroy()
    {
        if (buyRoutine != null)
            buyRoutine.PostPurchaseCompleteEvent -= RebootSetting;

        base.OnDestroy();
    }

    void InitData()
    {
        //WelcomeBackMission or LoginMore 설정
        for (int i = 0; i < titleRoot.Count; i++)
        {
            titleRoot[i].SetActive(false);
            passRoot[i].SetActive(false);
        }

        titleRoot[(int)ManagerWelcomeBackEvent._instance.GetContentsType() - 1].SetActive(true);
        passRoot[(int)ManagerWelcomeBackEvent._instance.GetContentsType() - 1].SetActive(true);

        //패스 구매 설정
        if (ManagerWelcomeBackEvent._instance.buyPassNeedReboot && ManagerWelcomeBackEvent.GetUserWelcomeBackMission().buyPass == 0)
        {
            objVipPassBuyBtn.SetActive(false);
            objVipPassCheck.SetActive(true);
        }
        else
        {
            VipPassBuy = ManagerWelcomeBackEvent.GetUserWelcomeBackMission().buyPass > 0;

            labelVipPassPrice.SetText(ServiceSDKManager.instance.GetPassPrice_String(WelcomeBackPassIndex));
        }

        //미션 데이터 세팅
        for (int i = 0; i < ManagerWelcomeBackEvent.GetWelcomeMissionData().Count; i++)
        {
            UIItemWelcomeBackMission itemRoot = NGUITools.AddChild(gridMissionItem.transform, welcomeMissionItem).GetComponent<UIItemWelcomeBackMission>();

            int type = ManagerWelcomeBackEvent.GetWelcomeMissionData()[i].missionType;
            int value = ManagerWelcomeBackEvent.GetWelcomeMissionData()[i].missionValue;

            itemRoot.UpdateData(ManagerWelcomeBackEvent.GetWelcomeMissionData()[i], i + 1,
                ManagerWelcomeBackEvent.IsMissionClear(type, value), ManagerWelcomeBackEvent.GetUserWelcomeBackMission().buyPass == 1);
        }

        //패키지 등록
        if (ServerContents.Packages.ContainsKey(WelcomeBackPassIndex))
        {
            var packageData = ServerContents.Packages[WelcomeBackPassIndex];

            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.init(this, packageData, autoOpen: bAutoOpen);
        }
    }

    void SetWelcomeEventTime()
    {
        EndTsTimer.Run(target: labelWecomeMissionEndTs, endTs: ManagerWelcomeBackEvent.GetUserWelcomeBackMission().endTs);
    }

    protected virtual void OnClickBuyVipPass()
    {
        ManagerUI._instance.OpenPopup<UIPopUpWelcomeMissionBuyInfo>
        ((popup) =>
            popup.InitData(ServiceSDKManager.instance.GetPassPrice_String(WelcomeBackPassIndex), $"{ManagerWelcomeBackEvent.GetResourceKey()}", () => StartBuy())
        );
    }

    public void StartBuy()
    {
#if UNITY_EDITOR
        RebootSetting();
#else
        if(buyRoutine.PostPurchaseCompleteEvent == null)
            buyRoutine.PostPurchaseCompleteEvent += RebootSetting;
        buyRoutine.OnClickButton();
#endif
    }

    void RebootSetting()
    {
        ManagerWelcomeBackEvent._instance.buyPassNeedReboot = true;

        //패스 구매 설정
        objVipPassBuyBtn.SetActive(false);
        objVipPassCheck.SetActive(true);

        SetRebootPopup($"n_s_{66 + (int)ManagerWelcomeBackEvent._instance.GetContentsType() - 1}");
    }

    void SetRebootPopup(string infoKey)
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(infoKey), true);
        popup.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
        popup.SetButtonText(1, Global._instance.GetString("btn_54"));
        popup.SetButtonText(2, Global._instance.GetString("btn_55"));
        popup.SortOrderSetting();
    }

    void OnApplicationPauseReboot()
    {
        if (UIPopupSystem._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    #endregion

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        panelScroll.depth = depth + 1;
        panelTitle.depth = depth + 2;
        panelBlock.depth = depth + 3;
    }
}