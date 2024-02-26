using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class UIPopUpSpaceTravel : UIPopupBase
{
    public class SpaceTravelStage
    {
        public int                          stageIndex;
        public ManagerSpaceTravel.StageType stageType;
        public List<Reward>                 rewardList;
    }

    public static UIPopUpSpaceTravel instance = null;

    // 스파인 (팝업 오픈 후 생성 및 할당)
    [SerializeField] private UIPanel           _spaceShipPanel;
    private                  SkeletonAnimation _galaxyBgSpineObj  = null;
    private                  SkeletonAnimation _spaceshipSpineObj = null;

    // 보상 관련
    [SerializeField] private UIPanel       _finalRewardRootPanel;
    [SerializeField] private Transform     _finalRewardRoot;
    [SerializeField] private GenericReward _finalRewardItem;
    [SerializeField] private GameObject    _getButton;

    // 스테이지 관련
    [SerializeField] private UIItemSpaceTravelStage _stagePrefab;
    [SerializeField] private UIScrollView           _scrollView;
    [SerializeField] private UIPanel                _scrollViewOverPanel;
    [SerializeField] private Transform              _pokotaObject;

    // 게이지 관련
    [SerializeField] private UIProgressBar               _stageProgress;
    [SerializeField] private List<UILabel>               _stageProgressLabel;
    [SerializeField] private List<GameObject>            _stageProgressBorders;
    [SerializeField] private List<UIItemSpaceTravelStar> _starRewardList;

    // 기타 세팅 오브젝트
    [SerializeField] private List<UISprite>       _spriteList;
    [SerializeField] private UIItemLanpageButton  _lanpageButton;
    [SerializeField] private GameObject           _startButton;
    [SerializeField] private UILabel              _endTsLabel;
    [SerializeField] private List<Color>          _colorList;

    // Init 후 생성되는 스테이지 데이터, 스테이지 아이템
    private List<SpaceTravelStage>       _stageDataList = new List<SpaceTravelStage>();
    private List<UIItemSpaceTravelStage> _stageItemList = new List<UIItemSpaceTravelStage>();

    #region 베이스 함수
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void OnDestroy()
    {
        ManagerSpaceTravel.instance.UnLoadSpaceTravelResource(ManagerSpaceTravel.PrefabType.UI);
        foreach (var spr in _spriteList)
            spr.atlas = null;
        Destroy(_spaceshipSpineObj);
        _spaceshipSpineObj = null;
        Destroy(_galaxyBgSpineObj);
        _galaxyBgSpineObj = null;
        
        if (instance == this)
        {
            instance = null;
        }

        base.OnDestroy();
    }
    #endregion

    #region UI 뎁스 설정 및 오브젝트 생성 함수
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        // 스파인 오브젝트 생성
        InitSpineObject();

        // 스프라이트 아틀라스 할당
        foreach (var sprite in _spriteList)
        {
            sprite.atlas = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
        }

        // BGM 변경
        ManagerSound._instance.PlayBGM();
    }

    public override void ClosePopUp(float mainTime = openTime, Method.FunctionVoid callback = null)
    {
        DOTween.To(() => _spaceshipSpineObj.transform.localScale, x => _spaceshipSpineObj.transform.localScale = x, Vector3.zero, openTime);
        DOTween.To(() => _finalRewardRootPanel.transform.localScale, x => _finalRewardRootPanel.transform.localScale = x, Vector3.zero, openTime);
        base.ClosePopUp(mainTime, callback);

        // 통상 게임타입으로 변경 후 BGM 변경
        ManagerSound._instance.StopBGM();
        Global.SetGameType_NormalGame();
        ManagerSound._instance.PlayBGM();
    }

    public override void SettingSortOrder(int layer)
    {
        _scrollView.panel.depth    = uiPanel.depth + 1;
        _spaceShipPanel.depth      = uiPanel.depth + 2;
        _finalRewardRootPanel.depth     = uiPanel.depth + 3;
        _scrollViewOverPanel.depth = uiPanel.depth + 3;

        if (layer < 10)
        {
            return;
        }

        uiPanel.useSortingOrder                                     = true;
        uiPanel.sortingOrder                                        = layer;
        _galaxyBgSpineObj.GetComponent<MeshRenderer>().sortingOrder = layer + 1;

        _scrollView.panel.useSortingOrder = true;
        _scrollView.panel.sortingOrder    = layer + 2;

        _spaceShipPanel.useSortingOrder                              = true;
        _spaceShipPanel.sortingOrder                                 = layer + 3;
        _spaceshipSpineObj.GetComponent<MeshRenderer>().sortingOrder = layer + 4;

        _finalRewardRootPanel.useSortingOrder = true;
        _finalRewardRootPanel.sortingOrder    = layer + 5;

        _scrollViewOverPanel.useSortingOrder = true;
        _scrollViewOverPanel.sortingOrder    = layer + 5;
    }

    private void InitSpineObject()
    {
        // BG 스파인 오브젝트 생성
        if (_galaxyBgSpineObj == null)
        {
            _galaxyBgSpineObj                      = Instantiate(ManagerSpaceTravel.instance._spaceTravelPackUI.GalaxySpineObj, mainSprite.transform).GetComponent<SkeletonAnimation>();
            _galaxyBgSpineObj.transform.localScale = Vector3.one * 110f;
            _galaxyBgSpineObj.AnimationState.Complete += delegate
            {
                _galaxyBgSpineObj.loop          = true;
                _galaxyBgSpineObj.AnimationName = "idle";
            };
        }

        // BG에 TextureAttacher 적용
        var textureAttacherList = _galaxyBgSpineObj.GetComponents<TextureAttacher>().ToList();
        var textureList         = ManagerSpaceTravel.instance._spaceTravelPackUI.PlanetTextureList;

        if (textureList != null)
        {
            for (var i = 0; i < textureAttacherList.Count; i++)
            {
                if (textureList.Count > i && textureList[i] != null)
                {
                    textureAttacherList[i].SetTextureAttacher(textureList[i]);
                }
            }
        }

        // 우주선 스파인 오브젝트 생성
        if (_spaceshipSpineObj == null)
        {
            _spaceshipSpineObj                      = Instantiate(ManagerSpaceTravel.instance._spaceTravelPackUI.SpaceshipSpineObj, _spaceShipPanel.transform).GetComponent<SkeletonAnimation>();
            _spaceshipSpineObj.transform.localScale = Vector3.one * 100f;
        }
    }
    #endregion

    #region UI 세팅 함수
    public void InitPopup()
    {
        SetStageData();
        SetStageItem();
        SetProgressItem();
        _startButton.SetActive(ManagerSpaceTravel.instance.CurrentStage <= _stageDataList.Count);
        _endTsLabel.text = Global.GetTimeText_MMDDHHMM_Plus1(ManagerSpaceTravel.instance.EndTs);
        _lanpageButton.On("LGPKV_event_SpaceTravel", Global._instance.GetString("p_spt_2"));
        StartCoroutine(CoMovePokota(ManagerSpaceTravel.instance.CurrentStage));
    }

    /// <summary>
    /// 스테이지 데이터 생성 및 세팅
    /// </summary>
    private void SetStageData()
    {
        _stageDataList.Clear();
        for (var i = 0; i < ManagerSpaceTravel.instance.MaxStage; i++)
        {
            _stageDataList.Add(new SpaceTravelStage()
            {
                stageIndex = i,
                stageType  = ManagerSpaceTravel.instance.GetStageType(i),
                rewardList = null,
            });
            if ((i + 1) % 3 == 0)
            {
                _stageDataList[i].rewardList = ManagerSpaceTravel.instance.RewardList[i / 3];
            }
        }
    }

    /// <summary>
    /// 스테이지 프리팹 생성 및 세팅
    /// </summary>
    private void SetStageItem()
    {
        // 스테이지 프리팹 생성 후 지정된 위치값으로 설정
        var xList      = new List<int>() {240, 50, -140, -240, -50, 140, 240, 50, -140, -240, -50, 140};
        var yList      = new List<int>() {100, 100, 250, 250, 100, 250, 250, 100, 250, 250, 100, 250};
        var stageCount = _stageDataList.Count;
        xList[stageCount - 1] = 0;
        var y = 0;
        for (var i = 0; i < stageCount; i++)
        {
            y += yList[i];
            var stageItem = NGUITools.AddChild(_scrollView.gameObject, _stagePrefab.gameObject).GetComponent<UIItemSpaceTravelStage>();
            _stageItemList.Add(stageItem);
            stageItem.transform.localPosition = new Vector2(xList[i], y);
            stageItem.InitStage(_stageDataList[i], _colorList[i / 3]);
        }

        _spaceShipPanel.transform.localPosition = new Vector2(0, y + 300);
        if (_stageDataList[stageCount - 1].stageType == ManagerSpaceTravel.StageType.REWARDED)
        {
            _finalRewardRootPanel.gameObject.SetActive(false);
            _spaceshipSpineObj.loop          = true;
            _spaceshipSpineObj.AnimationName = "idle";
        }
        else
        {
            _finalRewardRootPanel.gameObject.SetActive(true);
            _getButton.SetActive(_stageDataList[stageCount - 1].stageType == ManagerSpaceTravel.StageType.CLEAR);
            _spaceshipSpineObj.AnimationName = "0_idle";
            foreach (var reward in _stageDataList[stageCount - 1].rewardList)
                NGUITools.AddChild(_finalRewardRoot, _finalRewardItem.gameObject).GetComponent<GenericReward>().SetReward(reward);
        }

        // 스테이지 프리팹 아래 선 각도 설정
        for (var i = 0; i < _stageItemList.Count; i++) _stageItemList[i].InitLine(i < stageCount - 1 ? _stageItemList[i + 1].transform : null);
    }

    /// <summary>
    /// 특정 index로 포코타 위치 설정
    /// </summary>
    private void SetPokotaPosition(int stageIndex)
    {
        // 포코타 캐릭터 위치 설정
        var stagePos = new Vector2();

        // 마지막 스테이지일 경우 포코타 활성화 여부 설정
        if (stageIndex >= _stageDataList.Count)
        {
            if (_stageDataList[_stageDataList.Count - 1].stageType == ManagerSpaceTravel.StageType.REWARDED)
            {
                _pokotaObject.gameObject.SetActive(false);
            }
            else
            {
                stagePos                                            = _stageItemList[_stageDataList.Count - 1].transform.localPosition;
                _pokotaObject.GetComponent<TweenPosition>().enabled = true;
                _pokotaObject.GetComponent<TweenPosition>().from    = new Vector2(stagePos.x + 80, stagePos.y - 60);
                _pokotaObject.GetComponent<TweenPosition>().to      = new Vector2(stagePos.x + 80, stagePos.y - 80);
            }
        }
        // 마지막 스테이지가 아닐 경우
        else
        {
            stagePos                                            = _stageItemList[stageIndex - 1].transform.localPosition;
            _pokotaObject.GetComponent<TweenPosition>().enabled = true;
            _pokotaObject.GetComponent<TweenPosition>().from    = new Vector2(stagePos.x + 80, stagePos.y - 60);
            _pokotaObject.GetComponent<TweenPosition>().to      = new Vector2(stagePos.x + 80, stagePos.y - 80);
        }
    }

    /// <summary>
    /// 하단 게이지 바 관련 세팅
    /// </summary>
    private void SetProgressItem()
    {
        _stageProgress.value = (float) (ManagerSpaceTravel.instance.CurrentStage - 1) / _stageDataList.Count;
        _stageProgressLabel[0].text = $"{ManagerSpaceTravel.instance.CurrentStage - 1}/{_stageDataList.Count}";
        _stageProgressLabel[1].text = $"{ManagerSpaceTravel.instance.CurrentStage - 1}/{_stageDataList.Count}";
        foreach (var item in _stageProgressBorders) item.SetActive(_stageProgress.value > 0);
        var rewardList = ManagerSpaceTravel.instance.RewardList;
        for (var i = 0; i < rewardList.Count; i++)
        {
            var posX = _stageProgress.GetComponent<UISprite>().width / rewardList.Count * (i + 1) + _stageProgress.transform.localPosition.x;
            _starRewardList[i].gameObject.SetActive(true);
            _starRewardList[i].transform.localPosition = new Vector2(posX, 0);
            _starRewardList[i].InitItem(_stageDataList[i * 3 + 2], _colorList[i]);
        }
    }
    #endregion

    #region 연출 함수
    /// <summary>
    /// 포코타 이동 연출
    /// </summary>
    private IEnumerator CoMovePokota(int index)
    {
        // 스크롤 값, 이전 로컬 데이터값 세팅
        var scrollValuesList = new List<List<double>>()
        {
            new List<double>() {1, 0.3f},
            new List<double>() {1, 0.65f, 0.25f},
            new List<double>() {1, 0.75f, 0.5f, 0.15f},
        };
        var scrollValueList = scrollValuesList[_stageItemList.Count / 3 - 2];
        var prevEventIndex  = GetIndex().Item1;
        var prevStageIndex  = GetIndex().Item2;

        // 이전 스테이지 관련 키가 없거나, 회차가 다르거나, 스테이지 차이가 없을 경우 연출 재생 X
        if (prevEventIndex < 0 || prevStageIndex < 0 || _stageItemList.Count < prevStageIndex ||
            prevEventIndex != ManagerSpaceTravel.instance.EventIndex ||
            ManagerSpaceTravel.instance.CurrentStage > _stageItemList.Count ||
            ManagerSpaceTravel.instance.CurrentStage == prevStageIndex)
        {
            SetPokotaPosition(ManagerSpaceTravel.instance.CurrentStage);
            SetIndex(ManagerSpaceTravel.instance.EventIndex, ManagerSpaceTravel.instance.CurrentStage);
            yield return null; // 1프레임 대기 후 스크롤 세팅 (오픈 중 스크롤 세팅되면 이후 스크롤 자체 LateUpdate에서 초기화 시켜버림)
            StartCoroutine(SetReward());    // 보상 새로고침 진행
            if (ManagerSpaceTravel.instance.CurrentStage > _stageItemList.Count) // 최종 스테이지 클리어 한 경우
            {
                // 최종 스테이지 최초 클리어 시 사운드 재생
                if (prevStageIndex < ManagerSpaceTravel.instance.CurrentStage)
                {
                    ManagerSound.AudioPlayMany(AudioInGame.PRAISE4);
                }

                yield return null;
                _scrollView.verticalScrollBar.value = 0;
                yield break;
            }
            else
            {
                _scrollView.verticalScrollBar.value = (float) scrollValueList[(ManagerSpaceTravel.instance.CurrentStage - 1) / 3];
            }
            yield break;
        }

        SetPokotaPosition(prevStageIndex);
        yield return null;
        _scrollView.verticalScrollBar.value = (float) scrollValueList[(prevStageIndex - 1) / 3];
        StartCoroutine(SetReward()); // 보상 새로고침 진행

        // 포코타 이동 관련 데이터 세팅
        var isSuccess   = index - prevStageIndex > 0;
        var pokotaTween = _pokotaObject.GetComponent<TweenPosition>();
        var stagePos    = _stageItemList[index - 1].transform.localPosition;
        stagePos = new Vector3(stagePos.x + 80, stagePos.y - 60, 0f);
        var posDiff = _pokotaObject.transform.localPosition - pokotaTween.from;

        // 세팅한 데이터 기반으로 액션
        if (isSuccess)
        {
            var isRewardStage = prevStageIndex % 3 == 0;
            ManagerSound.AudioPlayMany(isRewardStage ? AudioInGame.PRAISE0 : AudioInGame.PRAISE1);
            ManagerSound.AudioPlayMany(AudioInGame.PANG_CIRCLE_BOMB);
            _pokotaObject.transform.localScale = stagePos.x > _pokotaObject.transform.localPosition.x ? new Vector3(-1, 1, 1) : Vector3.one;
            yield return new WaitForSeconds(openTime);
            ManagerSound.AudioPlayMany(AudioInGame.GET_CANDY);
            _pokotaObject.GetComponent<UISprite>().spriteName = "c2";
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.event_mole_hit_fail);
            yield return new WaitForSeconds(openTime);
            _pokotaObject.GetComponent<UISprite>().spriteName = "c3";
        }
        pokotaTween.enabled = false;
        SetIndex(ManagerSpaceTravel.instance.EventIndex, ManagerSpaceTravel.instance.CurrentStage);
        
        // 포코타 이동 및 경우에 따라 스크롤 이동
        _pokotaObject.transform.DOLocalMove(stagePos + posDiff, 0.5f).onComplete += () =>
        {
            pokotaTween.enabled                               = true;
            pokotaTween.from                                  = stagePos;
            pokotaTween.to                                    = new Vector2(stagePos.x, stagePos.y - 20);
            bCanTouch                                         = true;
            _pokotaObject.GetComponent<UISprite>().spriteName = "c1";
            _pokotaObject.transform.localScale                = Vector3.one;
            if ((prevStageIndex - 1) / 3 != (index - 1) / 3)
            {
                StartCoroutine(CoScroll(index, scrollValueList));
            }
        };

        // PlayerPrefs 로컬 데이터 로드 함수
        (int, int) GetIndex()
        {
            if (!PlayerPrefs.HasKey(ManagerSpaceTravel.instance.prevClearIndexKey))
            {
                return (-1, -1);
            }

            var indexArray = JsonFx.Json.JsonReader.Deserialize<int[]>(PlayerPrefs.GetString(ManagerSpaceTravel.instance.prevClearIndexKey));
            return (indexArray[0], indexArray[1]);
        }

        // PlayerPrefs 로컬 데이터 저장 함수
        void SetIndex(int eventIndex, int stageIndex)
        {
            var indexArray = new[] {eventIndex, stageIndex};
            PlayerPrefs.SetString(ManagerSpaceTravel.instance.prevClearIndexKey, JsonFx.Json.JsonWriter.Serialize(indexArray));
        }
        
        // 보상 출력되지 않을 경우를 대비하여 연출 중 보상 1회 더 새로고침
        IEnumerator SetReward()
        {
            yield return new WaitForSeconds(openTime);
            
            foreach (var star in _starRewardList)
            {
                star.SetRewardRootActive(false);
            }
            foreach (var star in _starRewardList)
            {
                star.SetRewardRootActive(true);
            }
        }
    }

    /// <summary>
    /// 스크롤 이동 연출
    /// </summary>
    private IEnumerator CoScroll(int index, List<double> scrollValueList)
    {
        if (index > _stageItemList.Count)
        {
            index -= 1;
        }

        var scrollValue = scrollValueList[(index - 1) / 3];

        // 스크롤 이동 연출
        if (_scrollView.verticalScrollBar.value < scrollValue)
        {
            while (_scrollView.verticalScrollBar.value < scrollValue)
            {
                _scrollView.verticalScrollBar.value += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime * 0.2f);
            }
        }
        else
        {
            while (_scrollView.verticalScrollBar.value > scrollValue)
            {
                _scrollView.verticalScrollBar.value -= Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime * 0.2f);
            }
        }
    }
    #endregion

    #region 버튼 이벤트
    /// <summary>
    /// 최종 보상 버튼 클릭
    /// </summary>
    private void OnClickGetButton()
    {
        if (!bCanTouch)
        {
            return;
        }

        bCanTouch = false;
        var rewardIndex = ManagerSpaceTravel.instance.RewardList.Count - 1;
        ServerAPI.SpaceTravelGetReward(rewardIndex, (resp) =>
        {
            if (resp.IsSuccess)
            {
                ManagerSpaceTravel.instance.SyncFromServerUserData();
                ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), SetAnimationAppear, resp.reward);
                _finalRewardRootPanel.gameObject.SetActive(false);
                PostRewardItem();
                ManagerUI._instance.SyncTopUIAssets();
                ManagerUI._instance.UpdateUI();
                
                if (resp.reward != null)
                {
                    if (resp.reward.directApplied != null)
                    {
                        foreach (var reward in resp.reward.directApplied)
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)reward.Key, reward.Value.valueDelta,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SPACE_TRAVEL_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SPACE_TRAVEL_REWARD,
                            $"SPACE_TRAVEL_REWARD_{ManagerSpaceTravel.instance.EventIndex}_{rewardIndex + 1}"
                            );
                        }
                    }

                    if (resp.reward.mailReceived != null)
                    {
                        foreach (var reward in resp.reward.mailReceived)
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(reward.type, reward.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SPACE_TRAVEL_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SPACE_TRAVEL_REWARD, 
                            $"SPACE_TRAVEL_REWARD_{ManagerSpaceTravel.instance.EventIndex}_{rewardIndex + 1}"
                            );
                        }
                    }
                }
            }

            bCanTouch = true;
        });

        // 보상 팝업 클로즈 후 연출
        void SetAnimationAppear()
        {
            ManagerSound.AudioPlayMany(AudioInGame.TIME_CLEAR);
            _spaceshipSpineObj.AnimationName = "appear";
            _spaceshipSpineObj.AnimationState.Complete += delegate
            {
                _spaceshipSpineObj.loop          = true;
                _spaceshipSpineObj.AnimationName = "idle";
            };
            _pokotaObject.gameObject.SetActive(false);
            _stageItemList[_stageDataList.Count - 1].SetStageHighlight();
        }
    }

    /// <summary>
    /// 스타트 버튼 클릭
    /// </summary>
    private void OnClickStartButton()
    {
        if (!bCanTouch)
        {
            return;
        }

        if (!ManagerSpaceTravel.CheckStartable())
        {
            ManagerUI._instance.OpenPopupEventOver();
            return;
        }

        ManagerUI._instance.OpenPopupReadyStageCallBack();
    }

    /// <summary>
    /// 보상 수령 후 스테이지 데이터 재세팅 및 보상 프리팹 재세팅
    /// </summary>
    public void PostRewardItem()
    {
        SetStageData();
        var rewardList = ManagerSpaceTravel.instance.RewardList;
        for (var i = 0; i < rewardList.Count; i++)
        {
            var item = _starRewardList[i];
            if (item.isActiveAndEnabled)
            {
                item.StageData = _stageDataList[i * 3 + 2];
                item.SetRewardHide();
            }
        }
        
        //강제 노출 이벤트 리워드 갱신
        if (ManagerForceDisplayEvent.instance != null)
        {
            foreach (var getState in ManagerSpaceTravel.instance.RewardGetState)
            {
                if (getState == 0)
                {
                    return;
                }
            }
            
            // 보상을 전부 받았다면 보상을 전부 받았는지에 대한 데이터 갱신 > 로비 재진입 시 팝업 강제 오픈 리스트에서 제외하기 위함
            ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.SPACE_TRAVEL);
        }
    }
    #endregion
}