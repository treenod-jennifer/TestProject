using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UIPopupAdventureSummonAction : UIPopupBase, ISummonHost
{
    public static UIPopupAdventureSummonAction _instance = null;

    [SerializeField] SkeletonAnimation spineSummonObject;
    [SerializeField] BoneFollower spineFollower;
    [SerializeField] UIPanel summonStartParticlePanel;
    [SerializeField] UIPanel[] decoPanels;

    [SerializeField] GameObject touchObject;

    private Coroutine swipeCoroutine = null;
    private string skinName = "1";

    [Header("Linked Object")]
    [SerializeField]
    private UIItemAdventureAnimalInfo animalInfo;


    [SerializeField]
    UIPanel animalPanel;
    [SerializeField]
    GameObject animalBoardRoot;
    [SerializeField]
    GameObject animalObjRoot;
    [SerializeField]
    GameObject animalTextureObj;
    [SerializeField]
    Animation animalAnimation;
    [SerializeField]
    GameObject animalOverlapCountRoot;
    Coroutine sceneCoroutine;

    [SerializeField]
    private Transform lightEffect;

    [SerializeField]
    GameObject newBaloonObj;
    [SerializeField]
    GameObject bonusBaloonObj;
    [SerializeField]
    GenericReward bonusReward;
    [SerializeField]
    UILabel labelBonusReward;

    [SerializeField]
    GameObject scriptObjRoot;
    [SerializeField]
    UILabel labelAnimalScript;

    [SerializeField]
    UISprite whiteOutSprite;

    [SerializeField]
    GameObject skipBtnObj;

    [SerializeField]
    GameObject[] gradeCommentObj = new GameObject[6];

    [SerializeField] UIItemSummon summonBtn;

    [SerializeField] GameObject confirmBtnRoot;

    [SerializeField] UIItemLanpageButton lanpageButton;
    [SerializeField] protected GameObject progressInfoRoot;

    [SerializeField] SortingLayerController maxOverlabEffect_01;
    [SerializeField] SortingLayerController maxOverlabEffect_02;

    // 일본어 환경 / 대만어 환경에 따라 위치 / 활성화 여부가 달라지는 오브젝트들
    [SerializeField] private GameObject buyInfoLabel;
    [SerializeField] private Transform transform_progressInfo;
    [SerializeField] private Transform transform_lanPage;
    [SerializeField] private Transform transform_labelName;
    [SerializeField] private Transform transform_ability;
    [SerializeField] private Transform transform_summmonButton;
    [SerializeField] private Transform transform_stars;
    
    CdnAdventureGachaProduct gachaData;
    ManagerAdventure.AnimalInstance aData = null;

    int overlapCount = 0;

    float animalCenterOffset = 100f;
    float animalRightOffset = 0f;

    bool summonButtonAllowed = false;
    bool newExist = false;
    bool bonusExist = false;

    bool textureLoadCompleted = false;

    public enum SummonType
    {
        NORMAL,
        PREMIUM,
        TICKET
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
            _instance = null;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 스파인 레이어만 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;

        summonStartParticlePanel.useSortingOrder = true;
        summonStartParticlePanel.sortingOrder = layer + 1;
        spineSummonObject.GetComponent<MeshRenderer>().sortingOrder = layer + 2;
        animalPanel.useSortingOrder = true;
        animalPanel.sortingOrder = layer + 3;

        foreach (var decoPanel in decoPanels)
        {
            decoPanel.useSortingOrder = true;
            decoPanel.sortingOrder = layer + 4;
        }

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        foreach (var listener in ManagerAdventure.User.animalListeners)
        {
            listener.OnAnimalChanged(aData.idx);
        }

        base.ClosePopUp(_mainTime, callback);
    }

    // isAnimalStated : 동료 보상, 중첩권 등으로 확정된 동료를 받을 경우 true, 가챠 티켓 등으로 랜덤한 동료를 받을 경우 false
    public virtual void InitPopup(ManagerAdventure.AnimalInstance orgAnimal, ManagerAdventure.AnimalInstance summoned, SummonType st, List<Reward> bonusRewards, CdnAdventureGachaProduct gachaData, Method.FunctionVoid callBack = null)
    {
        if (st == SummonType.TICKET && orgAnimal == null)
        {
            aData = summoned;
        }
        else
        {
            aData = orgAnimal;
        }

        overlapCount = summoned.overlap;

        SetSummonSpine();
        if (callBack != null)
            _callbackEnd += callBack;


        if (gachaData != null)
            summonBtn.InitData(this, gachaData);

        bool allowNextSummon = gachaData != null && gachaData.price != 0 && !summonBtn.premiumClose;

        this.summonBtn.gameObject.SetActive(allowNextSummon);
        this.confirmBtnRoot.SetActive(allowNextSummon == false);

        if (allowNextSummon)
        {
            lanpageButton.On(ManagerAdventure.GetGachaLanpageKey(gachaData), gachaData.asset_type == 3);
        }
        else
        {
            lanpageButton.Off();
        }

        this.progressInfoRoot.SetActive(allowNextSummon == false);
        
        bonusBaloonObj.SetActive(false);

        if ( bonusRewards != null && bonusRewards.Count > 0)
        {
            bonusReward.SetReward(bonusRewards[0]);
            labelBonusReward.text = "+" + bonusRewards[0].value.ToString();
            bonusExist = true;
        }

        InitAnimalInfo(summoned);
        SetPositionByCountry(allowNextSummon);

        if (aData.lobbyCharIdx != -1)
            SetLobbyInfoButton();

        ManagerSound._instance.PauseBGM();
    }

    public void SummonFailCallback(int errCode)
    {
        switch( errCode )
        {
            case 200:
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_29"), false, null);
                    popup.SortOrderSetting();
                }
                break;
            case 201:
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_33"), false, Global.ReBoot);
                    popup.SortOrderSetting();
                }
                break;
        }
    }

    public void Summon(CdnAdventureGachaProduct data)
    {
        if (bCanTouch == false || summonButtonAllowed == false)
            return;

        this._callbackEnd = () => {
            ManagerUI._instance.Summon(data, SummonFailCallback);

            if (UIPopupStageAdventureAnimal._instance != null)
            {
                UIPopupStageAdventureAnimal._instance.disableAnimalUpdateUntilFocus = true;
            }
        };
        OnClickBtnClose();
    }

    private int GetRandIndex(List<int> summonRate)
    {
        int rateNumber = 0;
        List<int> summonNum = new List<int>();
        for (int i = 0; i < summonRate.Count; i++)
        {
            rateNumber += summonRate[i];
            summonNum.Add(rateNumber);
        }
   
        int rand = Random.Range(1, (rateNumber + 1));
        for (int i = 0; i < summonNum.Count; i++)
        {
            if (rand <= summonNum[i])
                return i;
        }
        return 0;
    }

    private void SetSummonSpine()
    {        
        spineSummonObject.gameObject.SetActive(true);
        spineSummonObject.state.SetAnimation(0, "1_appear", false);
        spineSummonObject.state.End += delegate {
            bStartCheckSwipe = true;
        };
        spineSummonObject.state.AddAnimation(0, "1_idle", true, 0f);
        spineSummonObject.Update(0f);

        ManagerSound.AudioPlay(AudioLobby.phonebox_appear);
    }

    Vector2 firstPressPos = Vector2.zero;
    Vector2 lastPosition = Vector2.zero;
    Vector2 currentSwipe;
    float minSwipeLength = 200f;
    bool bAction = false;
    bool bStartCheckSwipe = false;

    private void Update()
    {
        //처음 등장하는 애니메이션 이후에 실행되도록.
        if (bAction == true || bStartCheckSwipe == false)
            return;

        bool bSwipe = false;
#if UNITY_EDITOR
        bSwipe = CheckEditorSwipe();
#else
        bSwipe = CheckDeviceSwipe();
#endif

        //뽑기 연출 실행.
        if (bSwipe == true)
        {
            bAction = true;
            sceneCoroutine = StartCoroutine(SummonRoutine());
        }
    }

    private bool CheckDeviceSwipe()
    {
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }

            if (t.phase == TouchPhase.Ended)
            {
                lastPosition = new Vector2(t.position.x, t.position.y);
                currentSwipe = new Vector3(lastPosition.x - firstPressPos.x, lastPosition.y - firstPressPos.y);

                if (currentSwipe.magnitude < minSwipeLength)
                {
                    return false;
                }

                return true;
            }
        }
        return false;
    }

    private bool CheckEditorSwipe()
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            firstPressPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) == true)
        {
            lastPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            currentSwipe = new Vector3(lastPosition.x - firstPressPos.x, lastPosition.y - firstPressPos.y);

            if (currentSwipe.magnitude < minSwipeLength)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    private void StartPhase2()
    {
        bCanTouch = false;
        
        string aniName = "2_appear";
        

        summonRoutineWait = true;
        spineSummonObject.state.ClearTracks();
        spineSummonObject.state.SetAnimation(0, aniName, false);
        spineSummonObject.state.Complete += delegate {
            spineSummonObject.gameObject.SetActive(false);
            spineFollower.enabled = false;
            //StartCoroutine(SpineDeltaModifier());
            skipBtnObj.SetActive(false);
            //if (coSync != null)
            //    StopCoroutine(coSync);
        };
        //ManagerSound.AudioPlay(AudioLobby.phonebox_whirl_2);
        this.summonStartParticlePanel.gameObject.SetActive(true);

        coSync = StartCoroutine(SpineSync());
        animalPanel.gameObject.SetActive(true);
        animalObjRoot.SetActive(true);
        animalAnimation.Play("UIPopupAdventureSummonAction_AnimalAnim", PlayMode.StopAll);
    }


    Coroutine coSync = null;
    bool syncAdj = false;
    Vector3 syncOffset = new Vector3();
    IEnumerator SpineSync()
    {
        float timer = 0f;
        var target = new Vector3(0f, this.animalCenterOffset-100f, 0f);
        while (true)
        {
            animalObjRoot.transform.localPosition = spineFollower.transform.localPosition;
            animalObjRoot.transform.localScale = spineFollower.transform.localScale;
            animalObjRoot.transform.localRotation = spineFollower.transform.localRotation;

            if( syncAdj )
            {
                timer += Time.deltaTime;
                animalObjRoot.transform.localPosition = Vector3.Lerp(animalObjRoot.transform.localPosition, target, timer > 1.0f ? 1.0f : timer);
            }

            yield return null;
        }

        Debug.Log("SpineSync End");
    }

    private void OpenGradeComment()
    {
        for (int i = 0; i < this.gradeCommentObj.Length; ++i)
        {
            if (gradeCommentObj[i] == null)
                continue;
            if(i == aData.grade)
            {
                gradeCommentObj[i].SetActive(true);
                return;
            }
        }
    }

    private void CloseGradeComment()
    {
        for (int i = 0; i < this.gradeCommentObj.Length; ++i)
        {
            if (gradeCommentObj[i] == null)
                continue;

            gradeCommentObj[i].SetActive(false);
        }

        lightEffect.gameObject.SetActive(true);
    }

    private void StopBoneFollow()
    {
        spineFollower.enabled = false;
        spineFollower.gameObject.transform.parent = this.animalBoardRoot.transform;
    }

    bool summonRoutineWait = false;
    private IEnumerator SummonRoutine()
    {
        NetworkLoading.MakeNetworkLoading(1);
        while(textureLoadCompleted == false)
        {
            yield return new WaitForSeconds(0.1f);
        }
        NetworkLoading.EndNetworkLoading();

        StartPhase2();
        this.touchObject.SetActive(false);
        skipBtnObj.SetActive(true);

        bool animalAppeared = false;
        while(summonRoutineWait)
        {
            lightEffect.position = animalObjRoot.transform.position;
            if (animalAnimation.isPlaying == false)
                break;
            yield return new WaitForSeconds(0.01f);
        }
        skipBtnObj.SetActive(false);

        scriptObjRoot.SetActive(true);
        yield return new WaitForSeconds(0.3f);

        newBaloonObj.SetActive(newExist);
        bonusBaloonObj.SetActive(bonusExist);

        summonButtonAllowed = true;

        if (aData.lobbyCharIdx != -1 && overlapCount == 1)
        {
            var popup = ManagerUI._instance.OpenPopup<UIPopupLobbyGiftInfo>(null, OnTouch);
            popup.Init(aData.idx);
        }
        else
            bCanTouch = true;


        //this.scriptObjRoot.transform.parent = this.animalPanel.transform;

        this.bShowTopUI = true;
        ManagerUI._instance.bTouchTopUI = true;
        ManagerUI._instance.TopUIDepthAndSortLayer();
    }

    public void InitAnimalInfo(ManagerAdventure.AnimalInstance summoned)
    {
        animalInfo.fullLoadedEvent += (texture) =>
        {
            textureLoadCompleted = true;
            this.animalCenterOffset = texture.GetCenter();
            this.animalRightOffset = ((texture.GetRight() - 128) * 2) + 15;
            this.animalRightOffset = Mathf.Max(this.animalRightOffset, 123.6f);
            animalOverlapCountRoot.transform.localPosition = new Vector3(animalRightOffset, 86.7f, 0);

            float lightScale = animalCenterOffset > 64.0f ? animalCenterOffset / 64.0f : 1.0f;
            lightEffect.localScale = Vector3.one * lightScale;

            spineFollower.transform.localPosition = new Vector3(0f, animalCenterOffset, 0f);
            SetAnimalTexturePosition(animalCenterOffset);
        };

        newBaloonObj.SetActive(false);

        if (aData == null || overlapCount == 1)
        {
            NewMarkUtility.newAnimalListAdd(summoned.idx);

            newExist = true;
            animalInfo.SetAnimalSelect(summoned);
        }
        else
        {
            newExist = false;
            animalInfo.SetAnimalSelect(aData);
            changeData = summoned;
        }
    }

    private bool isInitTexturePosition = false;
    private void SetAnimalTexturePosition(float animalCenterOffset)
    {
        if (isInitTexturePosition) return;

        animalTextureObj.transform.localPosition = new Vector3(0f, -1 * animalCenterOffset, 0f);
        isInitTexturePosition = true;
    }

    // 일본어 / 대만어 환경에 따라 오브젝트 위치 및 활성화 여부 세팅
    private void SetPositionByCountry(bool allowNextSummon)
    {
        if (LanguageUtility.IsShowBuyInfo && allowNextSummon)
        {
            buyInfoLabel.SetActive(true);
            transform_progressInfo.localPosition = new Vector2(transform_progressInfo.localPosition.x, -526f);
            transform_labelName.localPosition = new Vector2(transform_labelName.localPosition.x, -92f);
            transform_ability.localPosition = new Vector2(transform_ability.localPosition.x, -158f);
            transform_summmonButton.localPosition = new Vector2(transform_summmonButton.localPosition.x, 48f);
            transform_stars.localPosition = new Vector2(transform_stars.localPosition.x, -112f);
            transform_lanPage.localPosition = new Vector2(transform_lanPage.localPosition.x, -530f);
        }
    }

    [SerializeField] private UISprite skillTextBack;
    [SerializeField] private UILabel skillText;
    [SerializeField] private GameObject lobbyInfoButton;
    private void SetLobbyInfoButton()
    {
        skillTextBack.width = 300;
        skillText.width = 240;
        lobbyInfoButton.SetActive(true);

        var homeIcon = lobbyInfoButton.GetComponentInChildren<UISprite>();
        
        //스페셜 로비 배치 캐릭터 아이콘 변경
        if (ManagerCharacter._instance.IsSpecialLobbyChar(aData.idx))
        {
            homeIcon.spriteName = "icon_special_lobby";
            //기존의 아이콘은 크기를 조정하여 사용하기에 스페셜 로비 아이콘만 이미지 변경
            homeIcon.MakePixelPerfect();
        }
        else
            homeIcon.spriteName = "icon_home";
    }

    private ManagerAdventure.AnimalInstance changeData = null;
    public void ChangeAnimalState()
    {
        if(changeData != null)
        {
            animalInfo.ChangeAnimal_Ani(changeData);

            if (ManagerAdventure.ManagerAnimalInfo.GetMaxOverlap(changeData.idx) == changeData.overlap)
            {
                StartCoroutine(MaxOverlabAni(animalInfo));
            }
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }
    }
    
    public void OnClickBtnSkip()
    {
        skipBtnObj.SetActive(false);
        spineSummonObject.state.ClearTracks();
        spineSummonObject.gameObject.SetActive(false);
        spineFollower.enabled = false;

        StopCoroutine(this.sceneCoroutine);
        StartCoroutine(SkipRoutine());

    }

    public override void OnClickBtnBack()
    {
        if (skipBtnObj.activeSelf)
            OnClickBtnSkip();
        else if (mainSprite.gameObject.activeSelf)
            OnClickBtnClose();
    }

    public event System.Action<int> closeEvent;
    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        ManagerSound._instance.UnPauseBGM();

        if (closeEvent != null)
            closeEvent(aData.idx);

        // 원래 이렇게 하는 거 아닌데, 애니메이션을 먼저 다 잡아버리는 바람에 팝업 구조 새로잡고 애니 다 새로 잡는거 도저히 못하겠어서
        // 일단 닫을때만이라도 이렇게 처리...
        spineSummonObject.transform.parent = this.mainSprite.transform;
        animalBoardRoot.transform.parent = this.mainSprite.transform;
        ManagerUI._instance.ClosePopUpUI();
    }

    bool resultSoundPlayed = false;
    IEnumerator SkipRoutine()
    {
        if (coSync != null)
            StopCoroutine(coSync);

        animalTextureObj.SetActive(false);
        animalAnimation.Stop();
        animalAnimation.Play("UIPopupAdventureSummonAction_Skip", PlayMode.StopAll);
        //StartCoroutine(SpineDeltaModifier());

        this.spineSummonObject.gameObject.SetActive(false);
        summonStartParticlePanel.gameObject.SetActive(false);
        this.spineFollower.enabled = false;
        
        spineFollower.transform.localPosition = new Vector3(0f, animalCenterOffset - 100f, 0f);
        animalTextureObj.transform.localPosition = new Vector3(0f, -1 * animalCenterOffset, 0f);
        animalObjRoot.transform.localPosition = new Vector3(0f, this.animalCenterOffset - 100f, 0f);
        CloseGradeComment();

        PlaySound(AudioLobby.phonebox_result);

        lightEffect.gameObject.SetActive(true);
        while (summonRoutineWait)
        {
            lightEffect.position = animalObjRoot.transform.position;
            if (animalAnimation.isPlaying == false)
                break;
            yield return new WaitForSeconds(0.01f);
        }
        scriptObjRoot.SetActive(true);
        yield return new WaitForSeconds(0.3f);

        newBaloonObj.SetActive(newExist);
        bonusBaloonObj.SetActive(bonusExist);

        if (aData.lobbyCharIdx != -1 && overlapCount == 1)
        {
            var popup = ManagerUI._instance.OpenPopup<UIPopupLobbyGiftInfo>(null, OnTouch);
            popup.Init(aData.idx);
        }
        else
            bCanTouch = true;

        summonButtonAllowed = true;


        this.bShowTopUI = true;
        ManagerUI._instance.bTouchTopUI = true;
        ManagerUI._instance.TopUIDepthAndSortLayer();
    }

    void OnSkip_ResetTransform()
    {
        spineFollower.transform.localScale = Vector3.one;
        spineFollower.transform.rotation = new Quaternion();
        spineFollower.transform.localPosition = new Vector3(0f, animalCenterOffset - 100f, 0f);
        animalTextureObj.transform.localPosition = new Vector3(0f, -1 * animalCenterOffset, 0f);
        animalTextureObj.SetActive(true);
        animalObjRoot.transform.rotation = new Quaternion();
        animalObjRoot.transform.localScale = Vector3.one;
    }

    void StartPosSync()
    {
        syncAdj = true;
        //StartCoroutine(SpineDeltaModifier());
    }

    void OnClickProbabilityInfo()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_normal_gacha", Global._instance.GetString("p_frd_l_20"));
    }

    void PlaySound(AudioLobby idx)
    {
        if (idx == AudioLobby.phonebox_result)
        {
            if (resultSoundPlayed)
                return;
            resultSoundPlayed = true;

            switch (aData.grade)
            {
                case 3:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_result_3star);
                    break;
                case 4:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_result_4star);
                    break;
                case 5:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_result_5star);
                    break;
                default :
                    ManagerSound.AudioPlay(AudioLobby.phonebox_result);
                    break;
            }
        }
        else if(idx == AudioLobby.phonebox_starsound2)
        {
            switch (aData.grade)
            {
                case 3:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_starsound3);
                    break;
                case 4:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_starsound4);
                    break;
                case 5:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_starsound5);
                    break;
                default:
                    ManagerSound.AudioPlay(AudioLobby.phonebox_starsound2);
                    break;
            }
        }
        else
        {
            ManagerSound.AudioPlay(idx);
        }
    }

    void OnTouch()
    {
        bCanTouch = true;
    }

    public void OnClickLobbyInfo()
    {
        var popup = ManagerUI._instance.OpenPopup<UIPopupLobbyGiftInfo>();
        popup.Init(aData.idx);
    }

    [Header("Animation Control")]
    [SerializeField] private AnimationCurve textureSizeAni;
    private IEnumerator MaxOverlabAni(UIItemAdventureAnimalInfo animalInfo)
    {
        //입구오브젝트가 사라진다면 제거해도 되는 코드 입니다.
        //로비에 배치되어 있지 않지만 입구오브젝트에 등장하는 캐릭터일 때 외형을 변경해주는 코드
        if(ManagerCharacter._instance.IsSpecialLobbyChar(animalInfo.AnimalIdx))
            StartCoroutine(ManagerAIAnimal.instance.CoSelectLobbyAnimalCostumChange(animalInfo.AnimalIdx));
        
        yield return new WaitForSeconds(1.0f);

        var animalWidget = animalTextureObj.GetComponent<UIWidget>();

        float endTime = textureSizeAni.keys[textureSizeAni.length - 1].time;

        float totalTime = 0.0f;

        maxOverlabEffect_01.SetParticleSortingLayer(animalWidget.panel.sortingOrder + 1);
        maxOverlabEffect_01.gameObject.SetActive(true);

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimeLobby;

            animalTextureObj.transform.localScale = Vector3.one * textureSizeAni.Evaluate(totalTime);

            animalWidget.color = Color.Lerp(Color.black, Color.white, Mathf.Lerp(0.0f, 1.0f, (totalTime * 1.3f) / endTime));

            yield return null;
        }

        animalWidget.color = Color.black;

        animalInfo.LookID = 1;

        yield return new WaitForSeconds(0.5f);

        maxOverlabEffect_02.SetParticleSortingLayer(animalWidget.panel.sortingOrder - 1);
        maxOverlabEffect_02.gameObject.SetActive(true);
    }
}
