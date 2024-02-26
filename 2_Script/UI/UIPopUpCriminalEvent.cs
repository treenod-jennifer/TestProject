using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Protocol;
using Spine.Unity;
using UnityEngine.SceneManagement;
using ServiceSDK;

public class UIPopUpCriminalEvent : UIPopupBase
{
    public static UIPopUpCriminalEvent instance = null;

    [SerializeField] private UIGrid grid_itemList;
    [SerializeField] private UIGrid grid_rewardList;
    [SerializeField] private UIScrollView scrollView;
    [SerializeField] private UILabel label_endTsCount;
    [SerializeField] private UILabel progressCount_now;
    [SerializeField] private UILabel progressCount_all;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private GameObject criminalItem;
    [SerializeField] private GameObject rewardItem;
    [SerializeField] private GameObject allGetBtn;

    [SerializeField] private SkeletonAnimation spineWanted;
    [SerializeField] private TextureAttacher texAttacherCurrent;
    [SerializeField] private TextureAttacher texAttacherAfter;

    [SerializeField] private UIPanel panelCocoRoot;
    [SerializeField] private GameObject objCocoBubble;
    [SerializeField] private UILabel labelCocoBubble;

    [SerializeField] private GameObject objGameStart;
    [SerializeField] private GameObject objPlayBtn;
    [SerializeField] private GameObject objCompleteBtn;

    [SerializeField] private GameObject clearObj;
    [SerializeField] private GameObject clearObjRoot;
    
    private List<UIItemCriminalEvent> itemList = new List<UIItemCriminalEvent>();
    private List<GenericReward> rewardList = new List<GenericReward>();
    private List<StageData> stageDataList = new List<StageData>();

    [SerializeField] private UIItemLanpageButton lanPageBtn;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprCriminalList;

    public class StageData
    {
        public bool isGetReward;
        public int dataIndex;
        public int stageIndex;
        public Reward reward;
    }

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

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        for (var i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas = null;
        }
        
        if (instance == this)
            instance = null;
        base.OnDestroy();
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(ManagerCriminalEvent.instance.LoadCriminalEventResource());

        //스테이지 리스트 설정
        SetStageList();
        
        //Lan 페이지 설정
        lanPageBtn.On($"LGPKV_event_criminal", Global._instance.GetString("p_criminal_5"));
        
        //번들 Atlas 세팅
        for (int i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas =
                ManagerCriminalEvent.instance.criminalEventPack.AtlasUI;
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        panelCocoRoot.depth = depth + 1;
        scrollView.GetComponent<UIPanel>().depth = depth + 2;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;

        scrollView.GetComponent<UIPanel>().useSortingOrder = true;
        panelCocoRoot.useSortingOrder = true;
        spineWanted.GetComponent<MeshRenderer>().sortingOrder = layer + 1;
        panelCocoRoot.sortingOrder = layer + 2;
        scrollView.GetComponent<UIPanel>().sortingOrder = layer + 3;
    }


    #region UI 세팅

    public void InitPopup(bool isClear = false)
    {
        EndTsTimer.Run(label_endTsCount, ManagerCriminalEvent.instance.endTs);

        SetProgress();

        AnimalAction(isClear);

        //인게임에서 스타트 버튼이 보이지 않도록 설정
        objGameStart.SetActive(SceneManager.GetActiveScene().name == "Lobby");

        objPlayBtn.SetActive(ManagerCriminalEvent.instance.IsAllStageClear() == false);
        objCompleteBtn.SetActive(ManagerCriminalEvent.instance.IsAllStageClear());
    }

    private void SetSpineTextureAttacher(string fileName, System.Action<Texture2D> complete)
    {
        complete += texture2D => texAttacherCurrent.SetTextureAttacher(texture2D);

        Box.LoadCDN<Texture2D>(Global.adventureDirectory, "Animal/", fileName, complete);
    }

    private void AnimalAction(bool isClear)
    {
        string idleName;

        if (ManagerCriminalEvent.instance.stageClear.All(t => t != 0))
        {
            idleName = "clear_idle";
        }
        else
        {
            SetSpineTextureAttacher(
                $"m_{ManagerCriminalEvent.instance.animalList[ManagerCriminalEvent.instance.GetEventStep()]:D4}",
                texture2D => texAttacherCurrent.SetTextureAttacher(texture2D));

            idleName = "0_idle";
        }

        spineWanted.AnimationState.SetAnimation(0, idleName, true);
        spineWanted.AnimationState.Update(0f);

        // 연출이 팝업창 오픈시 바로 실행되는 것을 방지.
        if (ManagerCriminalEvent.instance.IsClearStage() && isClear) // 스테이지를 클리어 연출이 필요할 때 한번 호출.
            _callbackOpen += () => StartCoroutine(CoAnimalAction());
        
        
        if(ManagerCriminalEvent.instance.IsGetAllReward()) //모든 보상 수령 시 코코의 말풍선 제거
        {
            CocoBubbleDisabled();
        }
        else
        {
            SetCocoBubble(ManagerCriminalEvent.instance.IsAllStageClear() ? 3 : 2);
        }
    }

    private IEnumerator CoAnimalAction()
    {
        if (PlayerPrefs.GetInt(ManagerCriminalEvent.CRIMINAL_CLEAR_KEY) == Global.stageIndex)
            yield break;
        PlayerPrefs.SetInt(ManagerCriminalEvent.CRIMINAL_CLEAR_KEY, Global.stageIndex);

        bool isTextureSuccess = false;

        if (ManagerCriminalEvent.instance.IsAllStageClear() == false)
        {
            Box.LoadCDN<Texture2D>(Global.adventureDirectory, "Animal/",
                $"m_{ManagerCriminalEvent.instance.animalList[ManagerCriminalEvent.instance.GetEventStep() + 1]:D4}",
                (texture) =>
                {
                    isTextureSuccess = true;

                    texAttacherAfter.SetTextureAttacher(texture,
                        $"m_{ManagerCriminalEvent.instance.animalList[ManagerCriminalEvent.instance.GetEventStep() + 1]:D4}");
                });
        }
        else isTextureSuccess = true;

        yield return new WaitUntil(() => isTextureSuccess);

        if (ManagerCriminalEvent.instance.IsAllStageClear())
            spineWanted.AnimationState.SetAnimation(0, "clear_appear", false);
        else
            spineWanted.AnimationState.SetAnimation(0, "1_appear", false);

        spineWanted.AnimationState.Event += OnEventSpine;
        spineWanted.AnimationState.Complete += OnCompletedSpine;
    }

    private void SetCocoBubble(int index)
    {
        labelCocoBubble.text = Global._instance.GetString($"p_criminal_{index}");

        DOTween.Sequence().Join(DOTween.ToAlpha(() => objCocoBubble.GetComponentInChildren<UISprite>().color,
            x => objCocoBubble.GetComponentInChildren<UISprite>().color = x, 1f, 0.3f));
    }

    private void OnEventSpine(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (trackEntry.Animation.Name == "1_appear" || trackEntry.Animation.Name == "clear_appear")
        {
            switch (e.Data.Name)
            {
                case "sound_stamp":
                    ManagerSound.AudioPlay(AudioLobby.event_criminal_stamp);
                    break;
                case "sound_paper":
                    ManagerSound.AudioPlay(AudioLobby.event_criminal_paper);
                    break;
                case "sound_prison":
                    ManagerSound.AudioPlay(AudioLobby.event_criminal_prison);
                    break;
            }
        }
    }

    private void OnCompletedSpine(Spine.TrackEntry trackEntry)
    {
        int bubbleIndex = 2;

        switch (trackEntry.Animation.Name)
        {
            case "1_appear":
            {
                bubbleIndex = 3;

                spineWanted.AnimationState.AddAnimation(0, "1_idle", true, 0f);
                spineWanted.AnimationState.Update(0f);

                StartCoroutine(CoClearStage());
                SetCocoBubble(bubbleIndex);
            }
                break;
            case "clear_appear":
            {
                bubbleIndex = 4;

                spineWanted.AnimationState.AddAnimation(0, "clear_idle", true, 0f);
                spineWanted.AnimationState.Update(0f);

                StartCoroutine(CoClearStage());
                SetCocoBubble(bubbleIndex);
            }
                break;
        }
    }

    /// <summary>
    /// 팝업 상단 항목 설정 (진행도, 최종 보상 및 수령 여부)
    /// </summary>
    private void SetProgress()
    {
        int stageProgress = ManagerCriminalEvent.instance.GetEventStep();
        int stageCount = ManagerCriminalEvent.instance.stages.Count;

        // Progress Bar 세팅
        progressBar.value = (float)stageProgress / stageCount;
        progressCount_now.text = stageProgress.ToString();
        progressCount_all.text = "/" + stageCount;

        // 전체 보상 아이템 설정
        if (rewardList.Count == 0)
        {
            for (int i = 0; i < ManagerCriminalEvent.instance.finalRewardList.Count; i++)
            {
                var reward = NGUITools.AddChild(grid_rewardList.transform, rewardItem).GetComponent<GenericReward>();
                reward.SetReward(ManagerCriminalEvent.instance.finalRewardList[i]);
                rewardList.Add(reward);
                
                if (ManagerCriminalEvent.instance.isAllClearReward)
                {
                    reward.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f));
                    SetRewardTextColor(reward);
                    reward.EnableInfoBtn(false);
                    reward.btnTweenHolder.transform.localScale = Vector3.one;
                }
            }
        }

        clearObjRoot.SetActive(ManagerCriminalEvent.instance.isAllClearReward);
        
        allGetBtn.SetActive(stageProgress >= stageCount && !ManagerCriminalEvent.instance.isAllClearReward);
    }

    /// <summary>
    /// 스테이지 스크롤 리스트 설정
    /// </summary>
    private void SetStageList()
    {
        // 스테이지 데이터 세팅
        for (int i = 0; i < ManagerCriminalEvent.instance.stages.Count; i++)
        {
            stageDataList.Add(new StageData()
            {
                isGetReward = ManagerCriminalEvent.instance.stageReward[i] == 1,
                dataIndex = i,
                stageIndex = ManagerCriminalEvent.instance.stages[i],
                reward = ManagerCriminalEvent.instance.rewardList[i],
            });
        }

        // 스테이지 아이템에 데이터 기입
        for (int i = 0; i < stageDataList.Count; i++)
        {
            UIItemCriminalEvent item = NGUITools.AddChild(grid_itemList.transform, criminalItem)
                .GetComponent<UIItemCriminalEvent>();
            item.InitData(stageDataList[i]);
            itemList.Add(item);
        }

        //스크롤 자동 이동
        scrollView.ResetPosition();
        grid_itemList.Reposition();
        scrollView.UpdateScrollbars();

        int progress = ManagerCriminalEvent.instance.GetEventStep_ServerData();
        float positionIndex = (progress - 2 < 0 ? 0 : progress - 1.5f);

        scrollView.verticalScrollBar.value =
            ((float)grid_itemList.cellHeight * positionIndex) /
            ((float)grid_itemList.cellHeight * stageDataList.Count -
             scrollView.GetComponent<UIPanel>().height);
    }

    /// <summary>
    /// 코코의 말풍선 제거
    /// </summary>

    public void CocoBubbleDisabled()
    {
        objCocoBubble.gameObject.SetActive(false);
    }
    
    #endregion

    #region 버튼 이벤트

    private void OnClickGameStartButton()
    {
        if (!bCanTouch)
            return;
        StartCoroutine(CoStartButton());
    }

    IEnumerator CoStartButton()
    {
        if (ManagerCriminalEvent.CheckStartable() == false)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
            
            yield break;
        }
        
        ManagerUI._instance.ClosePopUpUI();

        if (ManagerCriminalEvent.instance.IsAllStageClear()) yield break;

        yield return new WaitForSeconds(0.2f);

        if (ServerRepos.User.stage > ManagerCriminalEvent.instance.GetTargetStage())
        {
            Global.SetGameType_NormalGame(ManagerCriminalEvent.instance.GetTargetStage());
            ManagerUI._instance.OpenPopupReadyStageCallBack();
        }
        else
        {
            ManagerUI._instance.OpenPopupReadyLastStage(false);
        }
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerCriminalEvent.instance.SyncFromServerUserData();
        base.ClosePopUp();
    }

    private void OnClickEventInfoButton()
    {
        if (!bCanTouch)
            return;

        ManagerUI._instance.OpenPopup<UIPopUpCriminalEventInfo>();
    }

    private void OnClickAllGetButton()
    {
        if (!bCanTouch)
            return;

        bCanTouch = false;
        
        NetworkLoading.MakeNetworkLoading(1f);
        
        // Server API 호출
        ServerAPI.CriminalEventGetAllClearReward((resp) =>
        {
            NetworkLoading.EndNetworkLoading();
            
            if (resp.IsSuccess)
            {
                //그로시
                {
                    if(resp.reward.directApplied != null)
                    {
                        foreach (var reward in resp.reward.directApplied)
                        {
                            // 구매한 아이템 기록
                            GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)reward.Key,
                                reward.Value.valueDelta,
                                GrowthyCustomLog_Money.Code_L_MRSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                GrowthyCustomLog_ITEM.Code_L_RSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                $"{ServerContents.CriminalEvent.vsn}_CRIMINAL_EVENT"
                            );
                        }
                    }

                    if(resp.reward.mailReceived != null)
                    {
                        foreach (var reward in resp.reward.mailReceived)
                        {
                            // 구매한 아이템 기록
                            GrowthyCusmtomLogHelper.SendGrowthyLog(
                                reward.type,
                                reward.value,
                                GrowthyCustomLog_Money.Code_L_MRSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                GrowthyCustomLog_ITEM.Code_L_RSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                $"{ServerContents.CriminalEvent.vsn}_CRIMINAL_EVENT"
                            );
                        }
                    }
                }
                StartCoroutine(CoAllClearRewardAction(resp.reward));
            }
        });
    }

    IEnumerator CoAllClearRewardAction(AppliedRewardSet reward)
    {
        bool isAnim = true;
        UISprite spriteClear = clearObj.GetComponent<UISprite>();
        UISprite spriteReceive = allGetBtn.GetComponentInChildren<UISprite>();

        var rewards = grid_rewardList.GetComponentsInChildren<GenericReward>();
        
        DOTween.Sequence().OnStart(() =>
            {
                UIPopUpCriminalEvent.instance.bCanTouch = false;
                clearObj.SetActive(true);
                clearObj.transform.localScale = Vector3.one * 2f;
                spriteClear.alpha = 0f;
                
                clearObjRoot.SetActive(true);
             
                if(ManagerCriminalEvent.instance.IsGetAllReward())
                    CocoBubbleDisabled();
                
                foreach (var reward in rewards)
                {
                    reward.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f));
                    SetRewardTextColor(reward);
                    reward.EnableInfoBtn(false);
                    reward.btnTweenHolder.transform.localScale = Vector3.one;
                }
            })
            .OnComplete(() =>
            {
                UIPopUpCriminalEvent.instance.bCanTouch = true;
                allGetBtn.SetActive(false);
                isAnim = false;
            })
            .Append(DOTween.ToAlpha(() => spriteReceive.color, x => spriteReceive.color = x, 0f, 0.3f))
            .Append(clearObj.transform.DOShakeRotation(0.5f, 10f, 10, 180f, false))
            .Join(clearObj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce))
            .Join(DOTween.ToAlpha(() => spriteClear.color, x => spriteClear.color = x, 1f, 0.5f));

        yield return new WaitUntil(() => !isAnim);

        ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, reward);
        ManagerCriminalEvent.instance.SyncFromServerUserData();
        
        //획득한 상품 ui에 갱신
        ManagerUI._instance.SyncTopUIAssets();
        
        //우편함으로 보상이 획득 될 때 우편함 갱신
        ManagerUI._instance.UpdateUI();

        bCanTouch = true;
    }
    
    private void SetRewardTextColor(GenericReward reward)
    {
        Color textColor = reward.rewardCount[0].color;
        Color effectColor = reward.rewardCount[0].effectColor;


        reward.SetTextColor(new Color(textColor.r * 0.4f, textColor.g * 0.4f, textColor.b * 0.4f));
        reward.SetEffectTextColor(new Color(effectColor.r * 0.4f, effectColor.g * 0.4f, effectColor.b * 0.4f));
    }

    #endregion

    /// <summary>
    /// 스테이지 클리어 후 재생 연출
    /// </summary>
    private IEnumerator CoClearStage()
    {
        int progress = ManagerCriminalEvent.instance.GetEventStep();

        // 첫 번째, 두 번째 스테이지 연출 관련 세팅 (목표 표시 제거, 스크롤 초기화, 동물 변경)
        itemList[progress].PostClearAnim(true, true);
        if (progress + 1 < itemList.Count)
            itemList[progress + 1].PostClearAnim(true, false);

        scrollView.verticalScrollBar.value =
            ((float)grid_itemList.cellHeight * (progress - 2 < 0 ? 0 : progress - 1.5f)) /
            ((float)grid_itemList.cellHeight * stageDataList.Count -
             scrollView.GetComponent<UIPanel>().height);

        // 첫 번째 스테이지 클리어 연출
        yield return new WaitForSeconds(openTime);
        bCanTouch = false;
        yield return StartCoroutine(itemList[progress].CoClearStageAnim());

        if (progress + 1 < itemList.Count)
        {
            int stageProgress = ManagerCriminalEvent.instance.GetEventStep();
            float positionIndex = (stageProgress - 2 < 0 ? 0 : stageProgress - 1.5f);
            float scrollValue = ((float)grid_itemList.cellHeight * positionIndex) /
                                ((float)grid_itemList.cellHeight * stageDataList.Count -
                                 scrollView.GetComponent<UIPanel>().height);

            // 스크롤 이동 연출
            while (scrollView.verticalScrollBar.value < scrollValue && scrollView.verticalScrollBar.value < 1)
            {
                scrollView.verticalScrollBar.value += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime * 0.5f);
            }

            // 두 번째 스테이지 목표 연출
            yield return StartCoroutine(itemList[progress + 1].CoSetStageAnim());
        }

        bCanTouch = true;
        ManagerCriminalEvent.instance.SyncFromServerUserData();
        SetProgress();
    }
}