using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol;
using UnityEngine;

public class UIPopupTreasureHunt : UIPopupBase
{
    public static UIPopupTreasureHunt _instance = null;
    
    [SerializeField] private List<UIItemTreasureHunt> stageItemList;
    [SerializeField] private List<UIItemTreasureDecoReward> rewardItemList;
    
    [SerializeField] private UIItemLanpageButton lanpageButton;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel progressCount;
    [SerializeField] private Transform mapPos;
    
    // Sprite
    [SerializeField] private UISprite pointObj;
    [SerializeField] private UISprite pointShadow;
    [SerializeField] private UISprite shadowObj;
    [SerializeField] private UISprite titleSprite;
    [SerializeField] private UISprite bgSprite;
    [SerializeField] private UISprite closeBtnSprite;
    [SerializeField] private UISprite progressBgSprite;
    [SerializeField] private UISprite progressGaugeSprite;
    [SerializeField] private UISprite stageBgSprite;
    [SerializeField] private UISprite lanPageButtonSprite;
    [SerializeField] private UISprite mapIconSprite;
    [SerializeField] private UISprite backBg;
    [SerializeField] private UISprite blackBg;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    
    private void OnDestroy()
    {
        ManagerUI._instance.bTouchTopUI = true;
        _instance = null;
    }
    
    public void InitPopup()
    {
        SetSprite();
        
        foreach (var item in stageItemList)
            item.InitStage();
        foreach (var item in rewardItemList)
            item.InitItem();

        lanpageButton.On("LGPKV_coin_event_stage", Global._instance.GetString("p_ce_3"));
        progressBar.value = (float)ManagerTreasureHunt.instance.GetMapCount() / ManagerTreasureHunt.instance.stageStatusList.Count;
        progressCount.text = $"{ManagerTreasureHunt.instance.GetMapCount()}/{ManagerTreasureHunt.instance.stageStatusList.Count}";
        
        int posIndex = GetPointIndex();
        MovePoint(posIndex);

        if (ManagerTreasureHunt.instance.isGetMap)
        {
            StartCoroutine(CoFlyTreasureMap(Global.stageIndex));
            ManagerTreasureHunt.instance.isGetMap = false;
        }
    }

    private void SetSprite()
    {
        INGUIAtlas uiAtlas = ManagerTreasureHunt.instance.treasureHuntPack.UIAtlas;
        
        titleSprite.atlas = uiAtlas;
        bgSprite.atlas = uiAtlas;
        closeBtnSprite.atlas = uiAtlas;
        progressBgSprite.atlas = uiAtlas;
        progressGaugeSprite.atlas = uiAtlas;
        stageBgSprite.atlas = uiAtlas;
        lanPageButtonSprite.atlas = uiAtlas;
        mapIconSprite.atlas = uiAtlas;
        pointObj.atlas = uiAtlas;
        pointShadow.atlas = uiAtlas;
        backBg.atlas = uiAtlas;
        blackBg.atlas = uiAtlas;
    }

    public void MovePoint(int index)
    {
        UIItemTreasureHunt stageItem = null;
        foreach (var item in stageItemList)
        {
            if (item.GetStageIndex() == index)
                stageItem = item;
        }

        if (stageItem != null)
        {
            shadowObj.transform.SetParent(stageItem.pointPos);
            shadowObj.transform.localPosition = new Vector2(0, 0);
            pointObj.MakePixelPerfect();
            shadowObj.MakePixelPerfect();
            shadowObj.transform.localScale = new Vector2(1.3f, 1.3f);
        }
        
        PlayerPrefs.SetInt(ManagerTreasureHunt.TREASURE_POINT_POS_KEY, index);
    }

    private int GetPointIndex()
    {
        // 이전 포코타 이동 기록이 있을 경우 : 해당 PlayerPrefs값 스테이지로 이동 / 없을 경우 : 첫 번째 스테이지로 이동
        // 포코타 이동 기록이 있으나 현재 스테이지 클리어 상태가 PlayerPrefs 값보다 낮을 경우 (이벤트 인덱스 변경) : 첫 번째 스테이지로 이동
        int index = 0;

        if (PlayerPrefs.HasKey(ManagerTreasureHunt.TREASURE_POINT_POS_KEY))
        {
            index= PlayerPrefs.GetInt(ManagerTreasureHunt.TREASURE_POINT_POS_KEY);
            if (ManagerTreasureHunt.instance.GetStageProgress() < index)
                index = 0;
        }

        return index;
    }

    public void GetDecoReward(int decoIndex)
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        growthyRewardIndex = decoIndex + 1;
        ServerAPI.TreasureHuntGetReward(decoIndex, RecvGetReward);
    }

    private int growthyRewardIndex = -1;
    private void RecvGetReward(TreasureHuntGetRewardResp resp)
    {
        if (resp.IsSuccess)
        {
            ManagerUI._instance.OpenPopupGetRewardAlarm (Global._instance.GetString("n_s_46"), null, resp.reward);
            ManagerTreasureHunt.instance.SyncFromServerUserData();
            foreach (var item in rewardItemList)
                item.InitItem();
            
            // 보상 수령 관련 그로시 로그 전송
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.TREASURE_HUNT,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.TREASURE_HUNT_MAP_REWARD,
                growthyRewardIndex.ToString(),
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
            
            if (resp.reward != null && resp.reward.directApplied != null)
            {
                foreach (var reward in resp.reward.directApplied)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.Value.type, reward.Value.valueDelta,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TREASURE_HUNT_MAP_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TREASURE_HUNT_MAP_REWARD,
                        growthyRewardIndex.ToString()
                    );
                }
            }

            if (resp.reward != null && resp.reward.mailReceived != null)
            {
                foreach (var reward in resp.reward.mailReceived)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.type, reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TREASURE_HUNT_MAP_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TREASURE_HUNT_MAP_REWARD,
                        growthyRewardIndex.ToString()
                    );
                }
            }
            
            //강제 노출 이벤트 리워드 갱신
            if (ManagerForceDisplayEvent.instance != null)
            {
                var rewardState = resp.userTreasureHunt.decoRewardState.FindIndex(x => x == 0 || x == 1);
                if (rewardState == -1)
                {
                    ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.TREASURE_HUNT);
                }
            }
        }

        bCanTouch = true;
    }

    public IEnumerator CoFlyTreasureMap(int index)
    {
        bCanTouch = false;
        yield return new WaitForSeconds(0.5f);
        UISprite flyObj = NGUITools.AddChild(mainSprite.transform, new GameObject()).AddComponent<UISprite>();
        flyObj.transform.localPosition = stageItemList[index - 1].transform.localPosition;
        flyObj.atlas = ManagerTreasureHunt.instance.treasureHuntPack.UIAtlas;
        flyObj.spriteName = "icon";
        flyObj.MakePixelPerfect();
        flyObj.transform.localScale = Vector3.one * 0.85f;
        flyObj.depth = 100;
        float delay;
        if (index >= 0 && index <= 3)
            delay = 0.7f;
        else if (index >= 4 && index <= 7)
            delay = 0.5f;
        else
            delay = 0.3f;
        flyObj.transform.DOLocalMove(mapPos.localPosition, delay).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(delay);
        
        flyObj.transform.DOScale(Vector3.one * 1.2f, 0.2f);
        yield return new WaitForSeconds(0.1f);
        flyObj.transform.DOScale(Vector3.one * 0.72f, 0.1f);
        bCanTouch = true;
        yield return null;
    }
}
