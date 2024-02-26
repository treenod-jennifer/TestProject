using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupCapsuleGacha : UIPopupBase
{
    public static UIPopupCapsuleGacha _instance = null;

    [Header("LimitedItemLink")]
    [SerializeField] private UILabel labelLimitedCount;
    [SerializeField] private UILabel labelLimitedMaxCount;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private float scrollSpeed = 10;

    [Header("CapsuleLink")]
    [SerializeField] private UILabel labelCapsuleEventTime;
    [SerializeField] private UILabel labelTakenAmount;
    [SerializeField] private UIUrlTexture texEventImage;
    [SerializeField] private UILabel labelEventLimitedItemSet;

    [Header("GachaLink")]
    [SerializeField] private GameObject objNomalGacha;
    [SerializeField] public GameObject objFreeGacha;
    [SerializeField] private UIItemLanpageButton lanpageButton;

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        //한정상품이 5개 이상일 때만 스크롤 기능 사용.
        if(ManagerCapsuleGachaEvent.GetLimitItemRatio().Count > 4)
            StartCoroutine(CoLimetedItemMove());

        StartCoroutine(CoCapsuleTime());

        texEventImage.SettingTextureScale(390, 360);
        texEventImage.LoadCDN(Global.gameImageDirectory, "IconCapsule/", $"c_limited_{ManagerCapsuleGachaEvent.GetResourceIndex()}");

        //Lan페이지 세팅
        lanpageButton.On("LGPKV_gacha_capsule", Global._instance.GetString("p_ct_9"));

        InitLimitedItem();
        InitCapsule();
        InitGachaBuuton();
    }

    protected override void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollView.depth = depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    void InitGachaBuuton()
    {
        if(ManagerCapsuleGachaEvent.IsFreeGacha())
        {
            objNomalGacha.SetActive(false);
            objFreeGacha.SetActive(true);
        }
        else
        {
            objNomalGacha.SetActive(true);
            objFreeGacha.SetActive(false);
        }

    }

    void InitLimitedItem()
    {
        var listCapsuleReward = ManagerCapsuleGachaEvent.GetLimitItemRatio();

        scrollView.GetComponentInChildren<UIReuseGrid_Move>().InItGrid(listCapsuleReward.Count, (go, index) =>
        {
            go.GetComponent<UIItemCapsuleLimitedItem>().UpdateData(listCapsuleReward[index]);
        });

        labelLimitedCount.text = ManagerCapsuleGachaEvent.GetGainLimitedItem().ToString();
        labelLimitedMaxCount.text = $"/{listCapsuleReward.Count.ToString()}";
    }

    void InitCapsule()
    {
        if (ServerRepos.UserTokenAssets != null && ServerRepos.UserTokenAssets.TryGetValue(2, out var value))
        {
            labelTakenAmount.text = value.amount.ToString();
        }

        string capsuleEventName = Global._instance.GetString($"ct_{ManagerCapsuleGachaEvent.GetCapsuleEventIndex()}");

        if(capsuleEventName.Contains($"ct_{ManagerCapsuleGachaEvent.GetCapsuleEventIndex()}"))
        {
            capsuleEventName = Global._instance.GetString($"ct_{0}");
        }

        labelEventLimitedItemSet.text = capsuleEventName;
    }

    private IEnumerator CoCapsuleTime()
    {
        while (Global.LeftTime(ServerContents.CapsuleGacha.endTs) > 0)
        {
            labelCapsuleEventTime.text = Global.GetTimeText_DDHHMM(ServerContents.CapsuleGacha.endTs);
            yield return new WaitForSeconds(1.0f);
        }

        labelCapsuleEventTime.text = "00:00:00";
    }

    public void CapsuleGachaSinglePlay()
    {
        if (ManagerCapsuleGachaEvent.IsNoEnoughCapsuleToyToken(1))
        {
            if (ManagerCapsuleGachaEvent.IsFreeGacha() == false)
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                popupSystem.SortOrderSetting();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_59"), false, null);

                return;
            }
        }

        int prevLimitedItemCount = ManagerCapsuleGachaEvent.GetGainLimitedItem();
        ServerAPI.CapsuleGachaSingleReward((resp) =>
        {
            if (resp.IsSuccess)
            {
                if (resp.reward == null)
                {
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                    popupSystem.SortOrderSetting();
                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_59"), false, null);

                    return;
                }

                //재화 업데이트.
                Global.clover = (int)(GameData.User.AllClover);
                Global.coin = (int)(GameData.User.AllCoin);
                Global.jewel = (int)(GameData.User.AllJewel);
                Global.wing = (int)GameData.Asset.AllWing;
                Global.exp = (int)GameData.User.expBall;

                if (ManagerUI._instance != null)
                    ManagerUI._instance.UpdateUI();

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)RewardType.capsuleGachaToken,
                        -1 * ServerContents.CapsuleGacha.price,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_USE_CAPSULE_GACHA,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                        $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_S"
                        );

                for (int i = 0; i < resp.reward.Count; ++i)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        resp.reward[i].type,
                        resp.reward[i].value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_CAPSULE_GACHA,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_CAPSULE_GACHA,
                        $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_S"
                        );
                }
                ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();


                Debug.Log("CapsuleGachaSingleReward Success");

                var currLimitedItemCount = ManagerCapsuleGachaEvent.GetGainLimitedItem();
                if( prevLimitedItemCount < currLimitedItemCount && currLimitedItemCount == ManagerCapsuleGachaEvent.GetLimitItemRatio().Count)
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                       (
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.CAPSULE_GACHA,
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CAPSULE_GACHA,
                           $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_S",
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                       );
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                }
                

                InitLimitedItem();
                InitCapsule();
                InitGachaBuuton();

                ManagerUI._instance.OpenPopup<UIPopupCapsuleGachaAction>((popup) => popup.InitPopup(resp.reward));

            }
            else
            {
                Debug.Log("CapsuleGachaSingleReward Fail");
            }
        });
    }

    public void CapsuleGachaMultiplePlay()
    {
        if (ManagerCapsuleGachaEvent.IsNoEnoughCapsuleToyToken(10))
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_59"), false, null);

            return;
        }

        int prevLimitedItemCount = ManagerCapsuleGachaEvent.GetGainLimitedItem();
        ServerAPI.CapsuleGachaMultipleReward((resp) =>
        {
            if (resp.IsSuccess)
            {
                if(resp.reward == null)
                {
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                    popupSystem.SortOrderSetting();
                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_59"), false, null);

                    return;
                }

                //재화 업데이트.
                Global.clover = (int)(GameData.User.AllClover);
                Global.coin = (int)(GameData.User.AllCoin);
                Global.jewel = (int)(GameData.User.AllJewel);
                Global.wing = (int)GameData.Asset.AllWing;
                Global.exp = (int)GameData.User.expBall;

                if (ManagerUI._instance != null)
                    ManagerUI._instance.UpdateUI();

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)RewardType.capsuleGachaToken,
                        -1 * ServerContents.CapsuleGacha.price * 10,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_USE_CAPSULE_GACHA,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                        $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_M"
                        );

                for (int i = 0; i < resp.reward.Count; ++i)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        resp.reward[i].type,
                        resp.reward[i].value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_CAPSULE_GACHA,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_CAPSULE_GACHA,
                        $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_M"
                        );
                }
                ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();

                var currLimitedItemCount = ManagerCapsuleGachaEvent.GetGainLimitedItem();
                if (prevLimitedItemCount < currLimitedItemCount && currLimitedItemCount == ManagerCapsuleGachaEvent.GetLimitItemRatio().Count)
                {
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                       (
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.CAPSULE_GACHA,
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CAPSULE_GACHA,
                           $"CAPSULE_{ServerRepos.LoginCdn.capsuleGachaVer}_M",
                           ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                       );
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                }

                Debug.Log("CapsuleGachaSingleReward Success");

                InitLimitedItem();
                InitCapsule();
                InitGachaBuuton();

                ManagerUI._instance.OpenPopup<UIPopupCapsuleGachaAction>((popup) => popup.InitPopup(resp.reward));
            }
            else
            {
                Debug.Log("CapsuleGachaSingleReward Fail");
            }
        });
    }

    private string RewardListToText(List<GradeReward> rewards)
    {
        string itemList = string.Empty;

        if(rewards == null || rewards.Count == 0) 
        {
            itemList = "no reward";
        }
        else
        {
            itemList += "\nGet Item List\n";

            foreach(var item in rewards)
            {
                itemList += $"grade : {item.grade} / type : {item.type} / value : {item.value}\n";
            }
        }
        
        return itemList;
    }

    IEnumerator CoLimetedItemMove()
    {
        while (true)
        {
            scrollView.transform.localPosition += -Vector3.right * Time.deltaTime * scrollSpeed;
            scrollView.clipOffset += Vector2.right * Time.deltaTime * scrollSpeed;

            yield return null;
        }
    }
}
