using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Protocol;
using Spine.Unity;
using UnityEngine;

public class UIPopupLuckyRoulette : UIPopupBase
{
    
    private enum SpineState_LuckyRoulette
    {
        IDLE,
        FRAME,
        APPEAR
    }
    
    public static UIPopupLuckyRoulette _instance = null; 
    
    [Header("TopUI")] 
    [SerializeField] private GameObject objSpineRoot;
    [SerializeField] private GameObject objAlphonsePos;
    [SerializeField] private GameObject objFramePos;
    private SkeletonAnimation spineAlphonse;
    private SkeletonAnimation spineFrame;
    
    [SerializeField] private UILabel labelCanGetRewardCount;
    [SerializeField] private UILabel labelRewardCount;
    [SerializeField] private GameObject[] bubbleLabel_Objs;
    [SerializeField] private GenericReward specialReward;

    [Header("MachineUI")] 
    [SerializeField] private UILabel labelEndTs;
    [SerializeField] private List<UIItemLuckyRoulette> listUIItemLuckyRoulette;
    [SerializeField] private UIPanel panelMachine;
    [SerializeField] private ParticleSystem rewardParticle;

    [Header("BottomUI")] 
    [SerializeField] private UIItemLanpageButton lanPageBtn;
    [SerializeField] private UILabel[] labelPrice;
    [SerializeField] private GameObject objBtnPurchase;
    [SerializeField] private GameObject objBtnPurchaseBlock;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprLuckyRouletteList;
    
    //데이터
    private bool isOverPurchase = false;
    private List<LuckyRouletteReward> rewardList;
    private List<RouletteProbability> rouletteProbabilities;

    private void Awake()
    {
        _instance = this;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            foreach (var spr in sprLuckyRouletteList)
                spr.atlas = null;
            
            Destroy(spineAlphonse);
            Destroy(spineFrame);
            spineAlphonse = null;
            spineFrame = null;
            ManagerLuckyRoulette.instance.luckyRoulettePack = null;
            
            _instance = null;
        }
        
        base.OnDestroy();
    }
    
    private void SetSpineObject()
    {
        GameObject spineObj = ManagerLuckyRoulette.instance.luckyRoulettePack.ObjSpine ? ManagerLuckyRoulette.instance.luckyRoulettePack.ObjSpine : null;

        if (spineObj == null)
            return;
        
        spineAlphonse = Instantiate(spineObj, objSpineRoot.transform).GetComponent<SkeletonAnimation>();
        spineFrame = Instantiate(spineObj, objSpineRoot.transform).GetComponent<SkeletonAnimation>();
        spineAlphonse.transform.localPosition = objAlphonsePos.transform.localPosition;
        spineFrame.transform.localPosition = objFramePos.transform.localPosition;
        
        //스파인 초기화
        SpineAction(spineAlphonse, SpineState_LuckyRoulette.IDLE);
        SpineAction(spineFrame, SpineState_LuckyRoulette.FRAME);
    }

    public override void SettingSortOrder(int layer)
    {
        //스파인 초기화
        SetSpineObject();

        if (layer < 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        spineAlphonse.GetComponent<MeshRenderer>().sortingOrder = layer + 1;
        spineFrame.GetComponent<MeshRenderer>().sortingOrder = layer + 2;
        panelMachine.useSortingOrder = true;
        panelMachine.sortingOrder = layer + 3;
        rewardParticle.GetComponent<Renderer>().sortingOrder = layer + 4;
        
        base.SettingSortOrder(layer);
    }

    private void Start()
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprLuckyRouletteList.Count; i++)
        {
            sprLuckyRouletteList[i].atlas =
                ManagerLuckyRoulette.instance.luckyRoulettePack.AtlasUI;
        }
        for (int i = 0; i < listUIItemLuckyRoulette.Count; i++)
        {
            listUIItemLuckyRoulette[i].Init();
        }
    }

    
    #region UI 세팅

    public void InitData()
    {
        //데이터 초기화
        rewardList = ManagerLuckyRoulette.instance.reward;
        for (int i = 0; i < rewardList.Count; i++)
        {
            Reward reward = new Reward { type = rewardList[i].type, value = rewardList[i].value };
            listUIItemLuckyRoulette[i].SetReward(reward);
        }

        //Lan 페이지 설정
        lanPageBtn.On($"LGPKV_event_Roulette", Global._instance.GetString("p_algacha_6"));
        
        //가격 설정
        labelPrice.SetText(ManagerLuckyRoulette.instance.price.ToString());
        
        //EndTs 설정
        EndTsTimer.Run(labelEndTs, ServerContents.LuckyRoulette.endTs);
        
        //스페셜 리워드 초기화
        specialReward.SetReward(ManagerLuckyRoulette.instance.completeReward);
        
        //리워드 초기화
        InitReward();
    }
    
    /// <summary>
    /// 리워드 UI 초기화
    /// </summary>
    private void InitReward()
    {
        ManagerLuckyRoulette.instance.SyncUserData();
        
        #region 슬롯 머신 초기화

        for (int i = 0; i < rewardList.Count; i++)
        {
            //리워드 획득 완료 
            if (ManagerLuckyRoulette.instance.rewardState.Contains(rewardList[i].id))
            {
                listUIItemLuckyRoulette[i].SetIcon(LuckyRouletteState.RECEIVED_BOX);
            }
            else
            {
                if (rewardList[i].hot_label == 0) //Hot 리워드 x
                {
                    listUIItemLuckyRoulette[i].SetIcon(LuckyRouletteState.NORMAL_BOX);
                }
                else //Hot 리워드 o
                {
                    listUIItemLuckyRoulette[i].SetIcon(LuckyRouletteState.SPECIAL_BOX);
                }
            }
        }

        #endregion

        #region 획득 현황 초기화
        
        labelRewardCount.text = $"{ManagerLuckyRoulette.instance.rewardState.Count()}/{ServerContents.LuckyRoulette.reward.Count}";

        SetBubbleLabel(ManagerLuckyRoulette.instance.completeRewardState);

        if (ManagerLuckyRoulette.instance.completeRewardState > 0)
        {
            objBtnPurchase.gameObject.SetActive(false);
            objBtnPurchaseBlock.gameObject.SetActive(true);
        }
        else
        {
            objBtnPurchase.gameObject.SetActive(true);
            objBtnPurchaseBlock.gameObject.SetActive(false);
        }
        
        #endregion
        
        //확률 초기화
        GetRouletteRatio();
    }

    /// <summary>
    /// 상단 버블 라벨 초기화
    /// 획득 현황 0, 획득 가능 1, 획득 완료 2
    /// </summary>
    private void SetBubbleLabel(int idx)
    {
        foreach (var obj in bubbleLabel_Objs)
        {
            obj.SetActive(false);
        }
        labelCanGetRewardCount.text = Global._instance.GetString("p_algacha_3")
            .Replace("[n]", (ManagerLuckyRoulette.instance.reward.Count - ManagerLuckyRoulette.instance.rewardState.Count).ToString());
        bubbleLabel_Objs[idx].SetActive(true);
        
        if(idx == 2)
            specialReward.SetColor(Color.gray);
    }
    
    private void SpineAction(SkeletonAnimation spine, SpineState_LuckyRoulette spineState)
    {
        if (spineState == SpineState_LuckyRoulette.APPEAR)
        {
            ManagerSound.AudioPlay(AudioLobby.lucky_roulette_spin);
        }

        string animName = "";
        if (spineState == SpineState_LuckyRoulette.IDLE)
            animName = "idle";
        else if (spineState == SpineState_LuckyRoulette.APPEAR)
            animName = "appear";
        else if (spineState == SpineState_LuckyRoulette.FRAME)
            animName = "frame";
        
        spine.AnimationState.SetAnimation(0, animName, true);
    }
    
    #endregion

    /// <summary>
    /// 일반 보상 획득 연출
    /// </summary>
    IEnumerator CoRandomSlotEffect(LuckyRouletteRewardResp resp)
    {
        bCanTouch = false;
        ManagerUI._instance.bTouchTopUI = false;
        //받을 수 있는 보상 리스트
        List<int> CanGetRewardList()
        {
            List<int> list = new List<int>();
            for (int i = 1; i <= ManagerLuckyRoulette.instance.reward.Count; i++)
            {
                if(ManagerLuckyRoulette.instance.rewardState.Contains(i))
                    continue;
                list.Add(i);
            }

            return list;
        }

        //id로부터 위치 인덱스 구함
        int GetPositionIdxFromId(int number)
        {
            return ManagerLuckyRoulette.instance.reward.Find(x => x.id == number).order_index - 1;
        }
        
        int recvIdx = GetPositionIdxFromId(resp.rewardedId);
        var activeRewards = CanGetRewardList();

        if(activeRewards.Count > 1)
        {
            float highlightTime = 0.13f;
            int count = 12;
            float nextTime = 0.12f;
            float waitTime = 0.2f;
            
            //하이라이트할 리스트 생성
            Queue<int> randomList = new Queue<int>();
            while (randomList.Count < count || randomList.Last() == recvIdx)
            {
                int random = Random.Range(0, activeRewards.Count);
                if (randomList.Count > 0 && randomList.Last() == random) //이전과 동일한 슬롯이 연속으로 연출하지 않도록 하기 위함
                    continue;
                randomList.Enqueue(GetPositionIdxFromId(activeRewards[random]));
            }

            //스파인 연출
            SpineAction(spineAlphonse, SpineState_LuckyRoulette.APPEAR);
            
            //하이라이트 연출
            var wait = new WaitForSeconds(nextTime);
            while (randomList.Count > 0)
            {
                var highLightIdx = randomList.Dequeue();
                StartCoroutine(listUIItemLuckyRoulette[highLightIdx].SetHighLight(highlightTime));
                yield return wait;
            }
            yield return new WaitForSeconds(waitTime);
        }
        
        // 받은 보상 연출
        rewardParticle.transform.position = listUIItemLuckyRoulette[recvIdx].transform.position;
        rewardParticle.Play();
        yield return listUIItemLuckyRoulette[recvIdx].SetHighLight(1);
        SpineAction(spineAlphonse, SpineState_LuckyRoulette.IDLE);
        
        yield return CoOpenRewardPopup(resp.reward);
    }

    IEnumerator CoOpenRewardPopup(AppliedRewardSet reward)
    {
        if (reward == null)
        {
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
            yield break;
        }
        //리워드 팝업
        ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), () =>
        {
            //UI 리셋
            InitReward();
            ManagerUI._instance.UpdateUI();
            ManagerUI._instance.SyncTopUIAssets();
        
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
        }, reward);
    }

    #region OnClick

    private void OnClickBuyBtn()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.bTouchTopUI = false;
        
        //모두 획득 시 무시
        if (ManagerLuckyRoulette.instance.completeRewardState != 0)
        {
            //모두 획득 팝업
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
            return;
        }
        
        //기간 종료 시 1회 구매 후 2회차부터 구매 불가
        if (ManagerLuckyRoulette.CheckStartable() == false)
        {
            if (isOverPurchase == true)
            {
                ManagerUI._instance.OpenPopupEventOver();
                bCanTouch = true;
                ManagerUI._instance.bTouchTopUI = true;
                return;
            }
        }

        //구매 확인 팝업
        var popup = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        string text = Global._instance.GetString("n_b_26").Replace("[n]", ManagerLuckyRoulette.instance.price.ToString());
        popup.InitSystemPopUp(
            Global._instance.GetString("p_t_4"),
            text,
            false,
            () =>
            {
                bCanTouch = true;
                ManagerUI._instance.bTouchTopUI = true;
            },
            price_type:PackagePriceType.Jewel,
            price_value:ManagerLuckyRoulette.instance.price.ToString());
        popup.SetCallbackSetting(1, Purchase, true);
        popup.SetResourceImage("Message/machine_icon");
        popup.ShowBuyInfo("buyinfo_algacha_1");
    }

    private void OnClickSpecialReward()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.bTouchTopUI = false;
        
        if (ManagerLuckyRoulette.instance.completeRewardState != 1)
        {
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
            return;
        }
        
        GetCompleteReward();
    }

    private void OnOpenPercentPopup()
    {
        if (bCanTouch == false)
            return;
        if (rouletteProbabilities == null)
            return;
        ManagerUI._instance.OpenPopup<UIPopupLuckyRouletteRatio>((popup) =>
        {
            popup.SetReward(rouletteProbabilities);
        });
    }
    
    #endregion

    #region RecvFuction

    private void Purchase()
    {
        if (ManagerLuckyRoulette.instance.price <= 0)
        {
            return;
        }
        
        //구매 가능 여부 확인
        if(Global.jewel < ManagerLuckyRoulette.instance.price)
        {
            //재화 부족 상점 오픈
            ManagerUI._instance.LackDiamondsPopUp();
            return;
        }
        
        var useJewel = Global._instance.UseJewel(ManagerLuckyRoulette.instance.price);
        ServerAPI.LuckyRouletteReward
        (
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    RewardCompleteCallback(resp, useJewel);
                }
            }
        );
    }

    //일반 보상 획득
    private void RewardCompleteCallback(LuckyRouletteRewardResp resp, Global.UseMoneyData useJewel)
    {
        //그로시
        {
            var useDiamond = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_LUCKY_ROULETTE,
                -useJewel.usePMoney,
                -useJewel.useFMoney,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                $"{ServerContents.LuckyRoulette.vsn}_LUCKY_ROULETTE"
            );
            var docDiamond = JsonConvert.SerializeObject(useDiamond);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDiamond);


            if (resp.reward.directApplied != null)
            {
                foreach (var reward in resp.reward.directApplied.Values)
                {
                    // 구매한 아이템 기록
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.type,
                        reward.valueDelta,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LUCKY_ROULETTE_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LUCKY_ROULETTE_REWARD,
                        $"{ServerContents.LuckyRoulette.vsn}_LUCKY_ROULETTE"
                    );
                }
            }

            if (resp.reward.mailReceived != null)
            {
                foreach (var reward in resp.reward.mailReceived)
                {
                    // 구매한 아이템 기록
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.type,
                        reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LUCKY_ROULETTE_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LUCKY_ROULETTE_REWARD,
                        $"{ServerContents.LuckyRoulette.vsn}_LUCKY_ROULETTE"
                    );
                }
            }
        }
        if (ManagerLuckyRoulette.CheckStartable() == false)
            isOverPurchase = true;

        StartCoroutine(CoRandomSlotEffect(resp));
    }

    //최종 보상 획득
    private void GetCompleteReward()
    {
        ServerAPI.LuckyRouletteSpecialReward
        (
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    //그로시
                    {
                        if (resp.reward.directApplied != null)
                        {
                            foreach (var reward in resp.reward.directApplied.Values)
                            {
                                // 구매한 아이템 기록
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                    reward.type,
                                    reward.valueDelta,
                                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LUCKY_ROULETTE_COMPLETE_REWARD,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LUCKY_ROULETTE_COMPLETE_REWARD,
                                    $"{ServerContents.LuckyRoulette.vsn}_LUCKY_ROULETTE"
                                );
                            }
                        }
                        
                        if (resp.reward.mailReceived != null)
                        {
                            foreach (var reward in resp.reward.mailReceived)
                            {
                                // 구매한 아이템 기록
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                    reward.type,
                                    reward.value,
                                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LUCKY_ROULETTE_COMPLETE_REWARD,
                                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LUCKY_ROULETTE_COMPLETE_REWARD,
                                    $"{ServerContents.LuckyRoulette.vsn}_LUCKY_ROULETTE"
                                );
                            }
                        }
                    }

                    StartCoroutine(CoOpenRewardPopup(resp.reward));
                }
            });
    }

    //확률 데이터 갱신
    private void GetRouletteRatio()
    {
        ServerAPI.LuckyRouletteRatio((resp) =>
        {
            if (resp.IsSuccess)
            {
                var probabilities = resp.RouletteProbabilities.OrderBy(x => x.order_index).ToList();
                rouletteProbabilities = probabilities;
            }
        });
    }

    #endregion
}
