using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol;
using Spine.Unity;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class AntiqueItemComparer : IComparer<HousingList>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(HousingList a, HousingList b)
    {
        if (a.idx < b.idx)
            return -1;
        else if (a.idx > b.idx)
            return 1;
        else
            return 0;
    }
}

public class UIPopupAntiqueStore : UIPopupBase
{
    public static UIPopupAntiqueStore _instance = null; 
    
    [Header("TopUI")] 
    [SerializeField] private UISprite progressBar;
    [SerializeField] private List<GameObject> objBonusRewards;
    [SerializeField] private List<GameObject> objRewardComplete;
    [SerializeField] private List<TweenScale> listTweenScales;
    [SerializeField] private UIPanel panelDecoInfo;

    [SerializeField] private GameObject objSpineRoot;
    
    [SerializeField] private GenericReward tierHousingItem;
    [SerializeField] private UITexture texHousingItemShadow;

    [SerializeField] private GameObject TopUIRoot;

    [Header("ScrollUI")] 
    [SerializeField] private UILabel labelTokenCount;
    [SerializeField] private UILabel labelMaxTokenCount;
    [SerializeField] private UILabel labelEndTs;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private UIPanel panelItemListShadow;
    [SerializeField] private UIReuseGrid_AntiqueStore scroll;

    [Header("BottomUI")] 
    [SerializeField] private GameObject objGameStart;
    [SerializeField] private UIItemLanpageButton lanPageBtn;

    [HideInInspector] public List<HousingList> _listAntiqueItemData = new List<HousingList>();

    private const int antiqueItemArraySize = 3;

    private AntiqueItemComparer antiqueItemComparer = new AntiqueItemComparer();
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprAntiqueStoreList;

    private void Awake()
    {
        _instance = this;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollView.depth = depth + 1;
        panelItemListShadow.depth = depth + 2;
        panelDecoInfo.depth = depth + 3;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        
        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
        
        panelItemListShadow.useSortingOrder = true;
        panelDecoInfo.useSortingOrder = true;
        
        panelItemListShadow.sortingOrder = layer + 2;
        _spineGlass.gameObject.GetComponent<MeshRenderer>().sortingOrder = layer + 3;
        _spineTwinkle.gameObject.GetComponent<MeshRenderer>().sortingOrder = layer + 4;
        panelDecoInfo.sortingOrder = layer + 5;
        
        base.SettingSortOrder(layer);
    }

    private void Start()
    {
        tierHousingItem.scale = 3f;
        tierHousingItem.rewardIcon_T.SuccessEvent +=
            () => {
                texHousingItemShadow.mainTexture = tierHousingItem.rewardIcon_T.mainTexture;
            };
        
        tierHousingItem.SetReward(ManagerAntiqueStore.instance.GetDecoInfoHousingIndex());
        
        //번들 Atlas 세팅
        for (int i = 0; i < sprAntiqueStoreList.Count; i++)
        {
            sprAntiqueStoreList[i].atlas =
                ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.AtlasUI;
        }

        //스크롤 그림자 생성
        StartCoroutine(CoCheckScrollShadowActive());
        
        //프로그래스 바 보상 위치 설정
        for (int i = 0; i < objBonusRewards.Count; i++)
        {
            objBonusRewards[i].transform.localPosition
                = new Vector3(progressBar.localSize.x * ServerContents.AntiqueStore.accumulateCount[i] / ServerContents.AntiqueStore.housingList.Count, 0f, 0f);
        }
        
        //누적 보상 프로그래스바 갱신
        progressBar.fillAmount = (float)ServerRepos.UserAntiqueStore.buyCount / ServerContents.AntiqueStore.housingList.Count;
        
        //Lan 페이지 설정
        lanPageBtn.On($"LGPKV_event_antique", Global._instance.GetString("p_antique_3"));
        
        
        //EndTs 설정
        EndTsTimer.Run(labelEndTs, ServerContents.AntiqueStore.endTs);
    }

    public void InitData()
    {
        //누적 보상 관련 초기화
        InitReward();
        
        //하우징 아이템 데이터 세팅
        SetAntiqueItemData();

        //스파인 세팅
        SetSpineObject();
        
        //재화 세팅
        InitTokenCount();
        
        //튜토리얼 진행
        StartCoroutine(CoPlayAntiqueStoreTutorial());

        //인게임에서 스타트 버튼이 보이지 않도록 설정
        objGameStart.SetActive(SceneManager.GetActiveScene().name == "Lobby");
    }

    private void InitTokenCount()
    {
        labelTokenCount.text = $"{ServerRepos.UserAntiqueStore.assetAmount}";
        labelMaxTokenCount.text = $"/{ServerContents.AntiqueStore.assetMaxCount}";
    }

    private void InitReward()
    {
        for (int i = 0; i < ServerRepos.UserAntiqueStore.accumulateState.Count; i++)
        {
            if (ManagerAntiqueStore.instance.IsBuyBonus(i))
            {
                if (ServerRepos.UserAntiqueStore.accumulateState[i] == 0)
                {
                    objRewardComplete[i].SetActive(false);
                    listTweenScales[i].enabled = true;
                }
                else
                {
                    objRewardComplete[i].SetActive(true);
                    listTweenScales[i].enabled = false;
                }
            }
            else
            {
                objRewardComplete[i].SetActive(false);
                listTweenScales[i].enabled = false;
            }
        }
        
        //누적 보상 프로그래스바 갱신
        progressBar.fillAmount = (float)ServerRepos.UserAntiqueStore.buyCount / ServerContents.AntiqueStore.housingList.Count;
    }

    private SkeletonAnimation _spineGlass;
    private SkeletonAnimation _spineTwinkle;
    
    private void SetSpineObject()
    {
        _spineGlass = Instantiate(ManagerAntiqueStore.instance.GetSpine_Glass(), objSpineRoot.transform).GetComponent<SkeletonAnimation>();
        _spineGlass.transform.localScale = Vector3.one * 100f;

        if(ServerContents.AntiqueStore.specialEffectState == 1)
        {
            _spineTwinkle = Instantiate(ManagerAntiqueStore.instance.GetSpine_Twinkle(), objSpineRoot.transform)
                .GetComponent<SkeletonAnimation>();
            _spineTwinkle.transform.localScale = Vector3.one * 100f;
        }
    }
    

    /// <summary>
    /// 상품 리스트 세팅
    /// 규칙 : 1순위 구매를 안한 하우징 우선 정렬, 2순위 : idx 순으로 정렬
    /// </summary>
    private void SetAntiqueItemData()
    {
        List<HousingList> listHousing = new List<HousingList>();
        List<HousingList> listBuyHousing = new List<HousingList>();
        
        for (int i = 0; i < ServerContents.AntiqueStore.housingList.Count; i++)
        {
            if (ServerRepos.UserAntiqueStore.buyState[i] == 0)
            {
                listHousing.Add(ServerContents.AntiqueStore.housingList[i]);
            }
            else
            {
                listBuyHousing.Add(ServerContents.AntiqueStore.housingList[i]);
            }
        }
        
        //idx로 정렬
        listHousing.Sort(antiqueItemComparer);
        listBuyHousing.Sort(antiqueItemComparer);
        
        _listAntiqueItemData.AddRange(listHousing);
        _listAntiqueItemData.AddRange(listBuyHousing);
    }

    public HousingList[] GetAntiqueItemData(int index)
    {
        int firstAntiqueItemIndex = index * antiqueItemArraySize;

        HousingList[] _arrayAntiqueItemData = new HousingList[3];

        for (int i = 0; i < antiqueItemArraySize; i++)
        {
            if(firstAntiqueItemIndex + i >= _listAntiqueItemData.Count) break;

            _arrayAntiqueItemData[i] = _listAntiqueItemData[firstAntiqueItemIndex + i];
        }

        return _arrayAntiqueItemData;
    }

    public int GetAntiqueItemCount()
    {
        int _nCount = Mathf.CeilToInt((float)_listAntiqueItemData.Count / antiqueItemArraySize);

        return _nCount;
    }

    public void PostBuyAntiqueItem()
    {
        if(_listAntiqueItemData.Count > 0)
            _listAntiqueItemData.Clear();
        
        //누적 보상 관련 초기화
        InitReward();
        
        //재화 세팅
        InitTokenCount();
        
        //구매 후 아이템 리스트 세팅
        SetAntiqueItemData();
        
        scroll.ScrollReset();
    }

    private IEnumerator CoCheckScrollShadowActive()
    {
        var scrollMaxValue = (scroll.itemSize * GetAntiqueItemCount()) - scrollView.height - (scrollView.clipSoftness.y * 2);
        var sprScrollShadow = panelItemListShadow.GetComponentInChildren<UISprite>();
        
        while (true)
        {
            DOTween.ToAlpha(() => sprScrollShadow.color, x => sprScrollShadow.color = x, scrollMaxValue < scrollView.transform.localPosition.y ? 0f : 1f, 0.15f)
                .SetEase(ManagerUI._instance.popupAlphaAnimation);
            
            yield return null;
        }
    }

    private bool IsStageAllClear()
    {
        bool isStageAllClear = false;

        int stageCount = 1;
        foreach (var chapterData in ManagerData._instance.chapterData)
        {
            stageCount += chapterData._stageCount;
            if (stageCount > ServerRepos.User.stage)
                break;
        }

        if (GameData.User.stage >= stageCount)
            isStageAllClear = true;
        
        return isStageAllClear;
    }

    #region OnClick
    
    private void OnClickDecoInfo()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
           ManagerUI._instance.OpenPopup<UIPopupDecoInformation>(); 
        });
    }

    private void OnClickStartBtn()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
            if (IsStageAllClear())
            {
                if (ManagerEndContentsEvent.CheckStartable())
                {
                    ManagerUI._instance.OpenPopupReadyEndContents();
                }
                else
                {
                    EventLobbyObjectOption.NotEventPopup();
                }
            }
            else
            {
                ManagerUI._instance.OpenPopupReadyLastStage(false);
            }
        });
    }

    private void OnClickTokenInfo()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
            ManagerUI._instance.OpenPopup<UIPopupAntiqueStoreInfo>();
        });
    }

    private int bonusIndex = 0;
    private void OnClickBonus_0()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;

        //보너스 아이템을 획득 가능한 상태인지 확인
        if (ManagerAntiqueStore.instance.IsBuyBonus(0) == false) return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
            bonusIndex = 1;
            
            ServerAPI.AntiqueStoreGetBonus(0, RecvGetBonus);
        });
    }

    private void OnClickBonus_1()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;
        
        //보너스 아이템을 획득 가능한 상태인지 확인
        if (ManagerAntiqueStore.instance.IsBuyBonus(1) == false) return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
            bonusIndex = 2;
            
            ServerAPI.AntiqueStoreGetBonus(1, RecvGetBonus);
        });
    }
    private void OnClickBonus_2()
    {
        if (bCanTouch == false || isPlayTutorial)
            return;
        
        //보너스 아이템을 획득 가능한 상태인지 확인
        if (ManagerAntiqueStore.instance.IsBuyBonus(2) == false) return;
        
        ManagerAntiqueStore.instance.PostCheckAntiqueStorePopupOpen(() =>
        {
            bonusIndex = 3;
            
            ServerAPI.AntiqueStoreGetBonus(2, RecvGetBonus);
        });
    }

    #endregion

    #region RecvFuction
    
    private void RecvGetBonus(AntiqueStoreGetBonus resp)
    {
        if (resp.IsSuccess)
        {
            //그로시
            {
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.ANTIQUE_STORE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ANTIQUE_STORE_GET_BONUS,
                    $"{ServerContents.AntiqueStore.eventIndex}_{bonusIndex}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
        
                //그로시 로그 디버깅
                var achievementDoc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", achievementDoc);

                if(resp.reward.directApplied != null)
                {
                    foreach (var reward in resp.reward.directApplied.Values)
                    {
                        // 구매한 아이템 기록
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            reward.type,
                            reward.valueDelta,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ANTIQUE_STORE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ANTIQUE_STORE,
                            $"{ServerContents.AntiqueStore.eventIndex}_ANTIQUE_STORE"
                        );
                    }
                }
                else
                {
                    foreach (var reward in resp.reward.mailReceived)
                    {
                        // 구매한 아이템 기록
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            reward.type,
                            reward.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ANTIQUE_STORE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ANTIQUE_STORE,
                            $"{ServerContents.AntiqueStore.eventIndex}_ANTIQUE_STORE"
                        );
                    }
                }
            }
            
            //통합 보상창
            ManagerUI._instance.OpenPopupGetRewardAlarm (Global._instance.GetString("n_s_46"), null, resp.reward);

            //획득한 상품 ui에 갱신
            ManagerUI._instance.SyncTopUIAssets();
            
            //우편함으로 보상이 획득 될 때 우편함 갱신
            ManagerUI._instance.UpdateUI();
            
            //Reward 갱신
            InitReward();
        }
        
        bCanTouch = true;
    }
    
    #endregion

    #region Tutorial

    public GameObject GetAntiqueStoreItemScroll()
    {
        return scrollView.gameObject;
    }

    public GameObject GetAntiqueStoreTopUI()
    {
        return TopUIRoot;
    }

    public GameObject GetSpineObjectRoot()
    {
        return objSpineRoot;
    }

    private bool isPlayTutorial = false;

    public IEnumerator CoPlayAntiqueStoreTutorial()
    {
        if (ManagerAntiqueStore.instance.CheckTutorial() == false) yield break;
        
        isPlayTutorial = true;
        
        ManagerTutorial.PlayTutorial(TutorialType.TutorialAntiqueStoreEvent_EventOpen);

        if(Global._optionTutorialOn)
            yield return new WaitWhile(() => ManagerTutorial._instance._playing);
        
        isPlayTutorial = false;
    }
    
    #endregion
}
