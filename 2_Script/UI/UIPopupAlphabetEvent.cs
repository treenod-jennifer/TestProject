using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;

public class UIPopupAlphabetEvent : UIPopupBase
{
    public static UIPopupAlphabetEvent _instance = null;

    private Color getRewardTitleColor = new Color(147f / 255f, 159f / 255f, 171f / 255f, 1f);
    private Color getRewardFrameColor = new Color(100f / 255f, 100f / 255f, 100f / 255f, 100f / 255f);

    [System.Serializable]
    private class NormalAlphabetUI
    {
        public GameObject alphabetRoot;
        public UISprite[] arrSpriteAlphabet;
        public UISprite sprBlind;
        public UILabel textBlind;

        [System.NonSerialized]
        public List<GameObject> listGetAlphabetEffect;
    }

    [System.Serializable]
    private class NormalRewardUI
    {
        public GenericReward reward;
        public UILabel textGroup;
        public UISprite spriteRewardTitle;
        public UISprite spriteRewardFrame;
        public GameObject objCheck;
    }

    [SerializeField]
    private UISprite[] arrSpritePopupBG;

    #region 상단을 구성하는 UI들
    [SerializeField]
    private GameObject uiTopRoot;

    [SerializeField]
    private UITexture textureTitle;

    [SerializeField]
    private UILabel textLeftTime;
    #endregion

    #region 하단을 구성하는 UI들
    [SerializeField]
    private GameObject uiBottomRoot;

    [SerializeField]
    private UILabel textInfo;

    [SerializeField]
    private UILabel textWarning;

    [SerializeField]
    private GameObject objBtnStart;

    [SerializeField]
    private UILabel[] textBtnStart;
    #endregion

    [SerializeField] 
    private UIItemLanpageButton lanpageButton_Normal;

    [SerializeField] 
    private UIItemLanpageButton lanpageButton_Special;

    #region 일반 알파벳
    [SerializeField]
    private GameObject normalRoot;
    [SerializeField]
    private UILabel[] textTitle_Normal;
    [SerializeField]
    private GameObject objGuideLine;
    [SerializeField]
    private GameObject objGuideArrow;
    [SerializeField]
    private NormalAlphabetUI[] arrAlphabetUI_Normal;
    [SerializeField]
    private NormalRewardUI[] arrRewardUI_Normal;
    #endregion

    #region 스페셜 알파벳
    [SerializeField]
    private GameObject specialRoot;
    [SerializeField]
    private UILabel[] textTitle_Special;
    [SerializeField]
    private GenericReward reward_Special;
    [SerializeField]
    private UITexture textureRewardLight;
    [SerializeField]
    private GameObject objCheck_Special;
    [SerializeField]
    private GameObject specialAlphabetTextureRoot;
    [SerializeField]
    private UIUrlTexture[] arrAlphabet_Special;
    [SerializeField]
    private GameObject getRewardEffect_Special;

    private List<GameObject> listGetAlphabetEffect_Special;
    #endregion

    #region 연출
    [SerializeField]
    private GameObject getAlphabetEffect;

    //보상받는 연출이 등장해야하는지.
    int currentGroup = 0;

    private bool isRewardAction_N = false;
    private bool isRewardAction_S = false;

    private List<int> listGainAlphbaetUIIdx_N = new List<int>();
    private List<int> listGainAlphbaetUIIdx_S = new List<int>();

    private bool isOpenActionEnd = false;
    #endregion

    #region 보상
    Protocol.AppliedRewardSet rewardSet = null;
    #endregion

    //이벤트 플레이가 가능한 상태인지 저장하는 변수.
    private bool isCanPlay = true;

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
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        StartCoroutine(CoAction(openTime, () =>
        {
            //연출 뜨고 있는 도중에, 터치가능해지지 않도록.
            if (isOpenActionEnd == true)
            {
                bCanTouch = true;
                ManagerUI._instance.bTouchTopUI = true;
            }
            if (_callbackOpen != null)
                _callbackOpen();

            ManagerUI._instance.FocusCheck();
        }));
    }

    public void InitPopup(bool displayStartButton, Protocol.AppliedRewardSet rewardSet)
    {
        if (displayStartButton == false || ManagerAlphabetEvent.instance.isUser_eventComplete == true)
            isCanPlay = false;

        //보상 적용
        this.rewardSet = rewardSet;

        InitActionData();
        InitEventTimer();
        InitTextureAndText();
        InitBtnStart();
        InitPopupSize();
        InitNormalAlphabetRoot();
        InitSpecialAlphabetRoot();

        //연출 출력
        StartCoroutine(CoPopupAction());
    }

    #region 팝업 초기화 처리 함수
    private void InitActionData()
    {
        //보상 키 제거.
        if (PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_N) == true)
        {
            isRewardAction_N = true;
            PlayerPrefs.DeleteKey(ManagerAlphabetEvent.prefsKey_N);
        }

        if (PlayerPrefs.HasKey(ManagerAlphabetEvent.prefsKey_S) == true)
        {
            isRewardAction_S = true;
            PlayerPrefs.DeleteKey(ManagerAlphabetEvent.prefsKey_S);
        }
    }

    private void InitEventTimer()
    {
        long endTs = ManagerAlphabetEvent.instance.endTs;

        textLeftTime.text = Global.GetTimeText_MMDDHHMM_Plus1(endTs);
    }

    private void InitTextureAndText()
    {
        this.gameObject.AddressableAssetLoad<Texture>("local_ui/alphabet_title",(x) => textureTitle.mainTexture = x);
        textInfo.text = Global._instance.GetString("p_abc_7");
        textWarning.text = Global._instance.GetString("p_abc_8");
    }

    private void InitBtnStart() 
    {
        objBtnStart.SetActive(isCanPlay);

        if (isCanPlay == false)
        {
            mainSprite.transform.localPosition -= new Vector3(0f, 30f, 0f);
        }
    }

    private void InitPopupSize()
    {
        //스페셜 블럭이 있는지에 따라 팝업 크기 결정.
        if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == false)
        {
            specialRoot.gameObject.SetActive(false);
            uiTopRoot.transform.localPosition += new Vector3(0f, -155f, 0f);
            uiBottomRoot.transform.localPosition += new Vector3(0f, 133f, 0f);

            for (int i = 0; i < arrSpritePopupBG.Length; i++)
            {
                arrSpritePopupBG[i].height -= 270;
            }
        }
    }

    private void InitNormalAlphabetRoot()
    {
        InitTitle_Normal();
        InitAlphabet_Normal();
        InitReward_Normal();
        InitLanPageButton_Normal();
    }

    private void InitTitle_Normal()
    {
        string titleText = Global._instance.GetString("p_abc_1");
        for (int i = 0; i < textTitle_Normal.Length; i++)
            textTitle_Normal[i].text = titleText;
    }

    private void InitAlphabet_Normal()
    {
        currentGroup = ManagerAlphabetEvent.instance.currentGroup;
        if (isRewardAction_N == true)
        {
            currentGroup -= 1;

            //이펙트 생성
            NormalAlphabetUI normalAlphabetUI = arrAlphabetUI_Normal[currentGroup - 1];
            normalAlphabetUI.listGetAlphabetEffect = new List<GameObject>();
            for (int i = 0; i < normalAlphabetUI.arrSpriteAlphabet.Length; i++)
            {
                GameObject effect = NGUITools.AddChild(normalAlphabetUI.arrSpriteAlphabet[i].gameObject, getAlphabetEffect);
                effect.transform.localPosition = Vector3.zero;
                effect.SetActive(false);
                normalAlphabetUI.listGetAlphabetEffect.Add(effect);
            }
        }

        foreach (var alphabetGroup in ManagerAlphabetEvent.instance.dicAlphabetIndex_Normal)
        {
            int checkGruopIdx = alphabetGroup.Key - 1;
            UISprite[] uISprites = arrAlphabetUI_Normal[checkGruopIdx].arrSpriteAlphabet;
            for (int i = 0; i < uISprites.Length; i++)
            {
                if (i >= alphabetGroup.Value.Count)
                {
                    uISprites[i].gameObject.SetActive(false);
                }
                else
                {
                    bool isGainAlphabet = ManagerAlphabetEvent.instance.CheckCollectAlphabet_Normal_ByListIndex(alphabetGroup.Key, i);
                    uISprites[i].spriteName = ManagerAlphabetEvent.instance.GetAlphabetSpriteName_N(alphabetGroup.Value[i], isGainAlphabet);
                    if (isGainAlphabet == true && alphabetGroup.Key == currentGroup)
                        listGainAlphbaetUIIdx_N.Add(i);

                    //유저 그룹 상태 비교해서, 그룹이 활성화되지 않은 상태면 알파벳에 알파값 적용해줌.
                    if (alphabetGroup.Key > currentGroup)
                        uISprites[i].color = new Color(1f, 1f, 1f, 0.25f);
                }
            }

            //설정된 알파벳 수에 따라 위치 설정
            Vector3 pos = arrAlphabetUI_Normal[checkGruopIdx].alphabetRoot.transform.localPosition;
            arrAlphabetUI_Normal[checkGruopIdx].alphabetRoot.transform.localPosition
                = new Vector3(315f - (alphabetGroup.Value.Count * 30f), pos.y, pos.z);

            //유저 그룹 상태에 따라 블라인드 설정
            if (alphabetGroup.Key > currentGroup)
            {
                arrAlphabetUI_Normal[checkGruopIdx].textBlind.gameObject.SetActive(true);
                arrAlphabetUI_Normal[checkGruopIdx].textBlind.text
                    = Global._instance.GetString("p_abc_3").Replace("[n]", (alphabetGroup.Key - 1).ToString());
            }
            else
            {
                arrAlphabetUI_Normal[checkGruopIdx].textBlind.gameObject.SetActive(false);
            }
        }

        //유저 그룹 진행 상태에 따라 가이드 라인 위치 설정
        if (currentGroup <= ManagerAlphabetEvent.instance.dicAlphabetIndex_Normal.Count)
        {
            Vector3 guidPos = objGuideLine.transform.localPosition;
            objGuideLine.transform.localPosition = new Vector3(guidPos.x, 140f - (70 * currentGroup), guidPos.z);
            objGuideLine.SetActive(true);
        }
        else
        {
            objGuideLine.SetActive(false);
        }
    }

    private void InitReward_Normal()
    {
        string groupText = Global._instance.GetString("p_abc_4");
        for (int i = 0; i < arrRewardUI_Normal.Length; i++)
        {
            NormalRewardUI normalRewardUI = arrRewardUI_Normal[i];
            normalRewardUI.textGroup.text = groupText.Replace("[n]", (i + 1).ToString());
            normalRewardUI.reward.SetReward(ServerContents.AlphabetEvent.reward[i][0]);

            //보상 창 설정
            if (isRewardAction_N == true && i == (currentGroup - 1))
            {
                normalRewardUI.objCheck.SetActive(false);
            }
            else
            {   //유저 보상 검사해서 UI 설정.
                if (ManagerAlphabetEvent.instance.isAchieveReward(i) == true)
                {
                    normalRewardUI.objCheck.SetActive(true);
                    normalRewardUI.spriteRewardTitle.color = getRewardTitleColor;
                    normalRewardUI.spriteRewardFrame.color = getRewardFrameColor;
                }
                else
                {
                    arrRewardUI_Normal[i].objCheck.SetActive(false);
                }
            }   
        }
    }

    private void InitLanPageButton_Normal()
    {
        lanpageButton_Normal.On("LGPKV_al_collect", Global._instance.GetString("p_abc_5"));
    }

    private void InitSpecialAlphabetRoot()
    {
        if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == false)
            return;

        InitTitle_Special();
        InitAlphabet_Special();
        InitReward_Special();
        InitLanPageButton_Special();
    }

    private void InitTitle_Special()
    {
        string titleText = Global._instance.GetString("p_abc_2");
        for (int i = 0; i < textTitle_Special.Length; i++)
            textTitle_Special[i].text = titleText;
    }

    private void InitAlphabet_Special()
    {
        //이펙트 생성
        if (isRewardAction_S == true)
        {
            listGetAlphabetEffect_Special = new List<GameObject>();
            for (int i = 0; i < arrAlphabet_Special.Length; i++)
            {
                GameObject effect = NGUITools.AddChild(arrAlphabet_Special[i].gameObject, getAlphabetEffect);
                effect.transform.localPosition = Vector3.zero;
                effect.SetActive(false);
                listGetAlphabetEffect_Special.Add(effect);
            }
        }

        int specialCnt = ManagerAlphabetEvent.instance.listAlphabetIndex_Special.Count;
        for (int i = 0; i < arrAlphabet_Special.Length; i++)
        {
            if (i >= specialCnt)
            {
                arrAlphabet_Special[i].gameObject.SetActive(false);
            }
            else
            {
                int alphabetIdx = ManagerAlphabetEvent.instance.listAlphabetIndex_Special[i];
                string textureName = ManagerAlphabetEvent.instance.GetAlphabetSpriteName_S(alphabetIdx);
                arrAlphabet_Special[i].LoadCDN(Global.gameImageDirectory, "IconEvent/", textureName);

                //알파벳 획득 상태 검사해서, 알파벳 이미지 설정.
                bool isGainAlphabet = ManagerAlphabetEvent.instance.CheckCollectAlphabet_Special_ByListIndex(i);
                if (isGainAlphabet == false)
                    arrAlphabet_Special[i].color = new Color(0f, 0f, 0f, 0.25f);
                else
                    listGainAlphbaetUIIdx_S.Add(i);
            }
        }

        //설정된 알파벳 수에 따라 위치 설정
        Vector3 pos = specialAlphabetTextureRoot.transform.localPosition;
        specialAlphabetTextureRoot.transform.localPosition = new Vector3(315f - (specialCnt * 30f), pos.y, pos.z);
    }

    private void InitReward_Special()
    {
        //빛 이미지.
        textureRewardLight.mainTexture = Box.LoadResource<Texture2D>("UI/light");
        if (isRewardAction_S == true)
            textureRewardLight.gameObject.SetActive(false);

        //보상 설정.
        reward_Special.SetReward(ServerContents.AlphabetEvent.reward[3][0]);

        //유저 스페셜 블럭 진행 상태에따라 보상 받았는지 상태 설정, 연출이 나와야하는 타이밍이면 체크표시 비활성화.
        if (isRewardAction_S == true)
            objCheck_Special.SetActive(false);
        else
            objCheck_Special.SetActive(ManagerAlphabetEvent.instance.isAchieveReward(-1));
    }

    private void InitLanPageButton_Special()
    {
        lanpageButton_Special.On("LGPKV_sp_collect", Global._instance.GetString("p_abc_6"));
    }
    #endregion

    #region 팝업 연출
    private IEnumerator CoPopupAction()
    {
        //보상 연출 없으면 터치 가능하도록 설정
        if (isRewardAction_N == false && isRewardAction_S == false)
        {
            isOpenActionEnd = true;
            bCanTouch = true;
        }

        yield return new WaitForSeconds(0.3f);

        //획득한 알파벳 보여주는 연출
        yield return CoAction_ShowGainAlphabet_N();

        //보상 연출
        yield return CoAction_GetReward_N();

        //스페셜 블럭이 있는 경우에만 연출.
        if (ManagerAlphabetEvent.instance.IsExistSpecialBlock() == true)
        {
            //획득한 알파벳 보여주는 연출
            yield return CoAction_ShowGainAlphabet_S();

            //보상 연출
            yield return CoAction_GetReward_S();
        }

        //보상 획득한 상태라면, 획득한 보상들 한번에 출력
        if (rewardSet != null)
        {
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (Global._instance.GetString("n_ev_1"),
                () => { isGetReward = true; },
                rewardSet);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }

        //연출 끝난 후, 터치 가능하도록 해줌.
        isOpenActionEnd = true;
        bCanTouch = true;
    }

    private IEnumerator CoAction_ShowGainAlphabet_N()
    {
        if (isRewardAction_N == false && ManagerAlphabetEvent.instance.isUser_normalComplete == true)
            yield break;

        UISprite[] uISprites = arrAlphabetUI_Normal[currentGroup - 1].arrSpriteAlphabet;
        for (int i = 0; i < listGainAlphbaetUIIdx_N.Count; i++)
        {
            int idx = listGainAlphbaetUIIdx_N[i];
            StartCoroutine(CoAction_GaimAlphabet_N(i, uISprites[idx]));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CoAction_GaimAlphabet_N(int index, UISprite uISprite)
    {
        if (isRewardAction_N == true && arrAlphabetUI_Normal[currentGroup - 1].listGetAlphabetEffect[index] != null)
        {
            arrAlphabetUI_Normal[currentGroup - 1].listGetAlphabetEffect[index].SetActive(true);
            yield return new WaitForSeconds(0.13f);
        }

        uISprite.transform.DOLocalJump(uISprite.transform.localPosition, 7.0f, 1, 0.15f);
        uISprite.transform.DOScale(Vector3.one * 1.2f, 0.05f);

        yield return new WaitForSeconds(0.05f);
        uISprite.transform.DOScale(Vector3.one, 0.05f);
        yield return new WaitForSeconds(0.05f);
    }

    private IEnumerator CoAction_ShowGainAlphabet_S()
    {
        //스페셜 블럭을 모두 획득한 상황에서 보상 연출도 등장하지 않는다면 알파벳 움직이는 연출 하지 않음.
        if (isRewardAction_S == false && ManagerAlphabetEvent.instance.isUser_specialComplete == true)
            yield break;

        for (int i = 0; i < listGainAlphbaetUIIdx_S.Count; i++)
        {
            StartCoroutine(CoAction_GaimAlphabet_S(i));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CoAction_GaimAlphabet_S(int index)
    {
        if (isRewardAction_S == true && listGetAlphabetEffect_Special[index] != null)
        {
            listGetAlphabetEffect_Special[index].SetActive(true);
            yield return new WaitForSeconds(0.15f);
        }

        int idx = listGainAlphbaetUIIdx_S[index];
        arrAlphabet_Special[idx].transform.DOLocalJump(arrAlphabet_Special[idx].transform.localPosition, 5.0f, 1, 0.2f);
        arrAlphabet_Special[idx].transform.DOScale(Vector3.one * 1.2f, 0.05f);

        yield return new WaitForSeconds(0.05f);
        arrAlphabet_Special[idx].transform.DOScale(Vector3.one, 0.05f);
        yield return new WaitForSeconds(0.05f);
    }

    private IEnumerator CoAction_GetReward_N()
    {
        if (isRewardAction_N == false)
            yield break;

        currentGroup = ManagerAlphabetEvent.instance.currentGroup;

        //다음 그룹의 알파벳이 있으면, 가이드 이동 연출
        if (currentGroup <= arrAlphabetUI_Normal.Length)
        {
            UISprite[] arrAlphabets = arrAlphabetUI_Normal[currentGroup - 1].arrSpriteAlphabet;
            for (int i = 0; i < arrAlphabets.Length; i++)
            {
                arrAlphabets[i].color = new Color(1f, 1f, 1f, 1f);
            }

            UILabel textBlind = arrAlphabetUI_Normal[currentGroup - 1].textBlind;
            DOTween.ToAlpha(() => textBlind.color, x => textBlind.color = x, 0, 0.3f);
            yield return new WaitForSeconds(0.3f);

            objGuideLine.transform.DOLocalMoveY(140f - (70 * currentGroup), 0.3f);
            yield return new WaitForSeconds(0.3f);
        }

        //보상 UI 갱신
        //이전 그룹의 보상 정보를 확인해, 보상을 받은 상태라면 UI 갱신.
        int prevGroup = currentGroup - 1;
        NormalRewardUI normalRewardUI = arrRewardUI_Normal[prevGroup - 1];
        if (ManagerAlphabetEvent.instance.isAchieveReward(prevGroup - 1) == true)
        {
            normalRewardUI.spriteRewardTitle.color = getRewardTitleColor;
            normalRewardUI.spriteRewardFrame.color = getRewardFrameColor;

            normalRewardUI.objCheck.SetActive(true);
            normalRewardUI.objCheck.transform.localScale = Vector3.zero;
            normalRewardUI.objCheck.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator CoAction_GetReward_S()
    {
        if (isRewardAction_S == false)
            yield break;

        getRewardEffect_Special.SetActive(true);

        textureRewardLight.transform.localScale = Vector3.zero;
        textureRewardLight.gameObject.SetActive(true);
        textureRewardLight.transform.DOScale(Vector3.one, 0.3f);
        yield return new WaitForSeconds(0.3f);

        objCheck_Special.transform.localScale = Vector3.zero;
        objCheck_Special.SetActive(true);
        objCheck_Special.transform.DOScale(Vector3.one, 0.3f);
        yield return new WaitForSeconds(0.3f);
    }
    #endregion

    void OnClickStartButton()
    {
        if (bCanTouch == false)
            return;

        //이벤트 시간이 종료된 경우, 안내 팝업 띄우기
        if (ManagerAlphabetEvent.instance.CheckEventEnd() == true)
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_41"), false);
            systemPopup.SortOrderSetting();
        }
        else
        {
            StartCoroutine(CoStartButton());
        }
    }

    IEnumerator CoStartButton()
    {
        ManagerUI._instance.ClosePopUpUI();
        yield return new WaitForSeconds(0.2f);

        //유저의 스테이지 진행도에 따라 스테이지 리스트 팝업/스테이지 준비팝업 선택해서 출력.
        if (ManagerAlphabetEvent.instance.canPlayStageIndex == 0)
        {
            ManagerUI._instance.OpenPopupStage();
        }
        else
        {
            ManagerUI._instance.OpenPopupReadyLastStage(false);
        }
    }
}
