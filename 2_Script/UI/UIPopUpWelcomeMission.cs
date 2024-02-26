using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

public class UIPopUpWelcomeMission : UIPopupBase
{
    [Header("VipLink")]
    [SerializeField] private UILabel labelWecomeMissionEndTs;
    [SerializeField] private UILabel[] labelVipPassPrice;
    [SerializeField] private GameObject objVipPassBuyBtn;
    [SerializeField] private GameObject objVipPassCheck;
    [SerializeField] private GameObject objVipPassBlock;

    [Header("MissionLink")]
    [SerializeField] private UIGrid gridMissionItem;
    [SerializeField] private GameObject welcomeMissionItem;

    [Header("PanelLink")]
    [SerializeField] private UIPanel panelScroll;
    [SerializeField] private UIPanel panelBlock;

    [Header("LanPage")]
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;

    private PackageBuyRoutine buyRoutine = null;

    public bool bAutoOpen = false;

    private bool VipPassBuy
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
    
    private int WelcomePassIndex
    {
        get { return ServerContents.WelcomeMission.startPassPackageIdx; }
    }

    private void Start()
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

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        panelScroll.depth = depth + 1;
        panelBlock.depth = depth + 2;
    }

    private void InitData()
    {
        if(ManagerWelcomeEvent.isBuyVipPass && ManagerWelcomeEvent.IsBuyVipPass() == false)
        {
            //패스 구매 설정
            objVipPassBuyBtn.SetActive(false);
            objVipPassCheck.SetActive(true);
        }
        else
        {
            VipPassBuy = ManagerWelcomeEvent.IsBuyVipPass();

            labelVipPassPrice.SetText(ServiceSDKManager.instance.GetPassPrice_String(WelcomePassIndex));
        }

        for (int i = 0; i < ManagerWelcomeEvent.GetWelcomeMissionData().Count; i++)
        {
            UIItemWelcomeMission itemRoot = NGUITools.AddChild(gridMissionItem.transform, welcomeMissionItem).GetComponent<UIItemWelcomeMission>();

            int type = ManagerWelcomeEvent.GetWelcomeMissionData()[i].missionType;
            int value = ManagerWelcomeEvent.GetWelcomeMissionData()[i].missionValue;

            itemRoot.UpdateData(ManagerWelcomeEvent.GetWelcomeMissionData()[i], i + 1,
                type, value, ManagerWelcomeEvent.IsBuyVipPass());
        }

        gridMissionItem.Reposition();
        
        if (ServerContents.Packages.ContainsKey(WelcomePassIndex) )
        {
            var packageData = ServerContents.Packages[WelcomePassIndex];

            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.init(this, packageData, autoOpen: bAutoOpen);
        }
    }

    private void SetWelcomeEventTime()
    {
        EndTsTimer.Run(target: labelWecomeMissionEndTs, endTs: ManagerWelcomeEvent.welcomeEndTs());
    }

    private void OnClickBuyVipPass()
    {
        ManagerUI._instance.OpenPopup<UIPopUpWelcomeMissionBuyInfo>
        ((popup) =>
            popup.InitData(ServiceSDKManager.instance.GetPassPrice_String(WelcomePassIndex), "wm", () => StartBuy())
        );
    }

    private void RebootSetting()
    {
        ManagerWelcomeEvent.isBuyVipPass = true;

        //패스 구매 설정
        objVipPassBuyBtn.SetActive(false);
        objVipPassCheck.SetActive(true);

        SetRebootPopup();
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

    private void SetRebootPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_61"), true);
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
}
