using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemStage : MonoBehaviour, UIItemFriendsManager.FriendsManagerObserver
{
    public UISprite _sprBtnBack;

    #region 스테이지 버튼.
    public GameObject   stageRoot;
    public UILabel      stageLevel;
    public UISprite     flower;
    public UISprite     arrow;
    public UISprite     stageQuestIcon;
    public UISprite     eventIcon;
    #endregion

    #region 챕터 UI.
    public GameObject   chapterRoot;
    public UILabel      chapterNumber;
    public UILabel      chapterNumberShadow;
    public UILabel      chapterQuestCount;
    public UISprite     chapterQuestBox;
    public UISprite     chapterQuestIcon;
    #endregion
    
    [SerializeField] private UIItemProfile[] profileItem;

    private StageUIData item = null;

    Color lineColorGrey = new Color32(169, 174, 177, 255);
    Color lineColorGreen = new Color32(100, 185, 118, 255);
    Color lineColorBlue = new Color32(16, 112, 133, 255);

    private void Awake()
    {
        UIPopupStage._instance.FriendManager.AddObserver(this);
    }

    private void OnDestroy()
    {
        UIPopupStage._instance.FriendManager.DeleteObserver(this);
    }

    public void UpdateData(StageUIData CellData)
    {
        FriendsOff();

        if (CellData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        item = CellData;
        
        //현재 유저가 연 스테이지에 따라 아이템 배경 이미지 변경.
        if (item.eBtnState != BtnStageState.BtnLock && item.eBtnState != BtnStageState.BtnComingSoon 
            && item.eBtnState != BtnStageState.BtnCheerUp)
        {
            //홀수 번째 챕터의 경우, 녹색.
            if (item.nChapterNumber <= 1 || item.nChapterNumber % 2 == 1)
            {
                _sprBtnBack.spriteName = "stage_box_01";
            }
            //짝수 번째 챕터의 경우, 파란색.
            else
            {
                _sprBtnBack.spriteName = "stage_box_02";
            }
        }
        else
        {
            _sprBtnBack.spriteName = "stage_box_lock";
        }

        if (item.eBtnState == BtnStageState.BtnComingSoon)
        {
            SetBtnComingSoon();
        }
        else
        {
            //스테이지 버튼인 상태라면 스테이지 버튼을 세팅.
            if (item.bChapterUI == false)
            {
                SetBtnStage();
                LoadFriends(item.nStageNumber);
            }
            //챕터 버튼인 상태라면 스테이지 버튼을 세팅.
            else
            {
                SetBtnChapter();
            }
        }
        
        SetArrowDirection(item.eArrowDir);
    }

    public void UpdateFriend()
    {
        if (item == null) return;

        if (item.eBtnState == BtnStageState.BtnComingSoon || item.bChapterUI) 
        {
            FriendsOff();
        }
        else
        {
            LoadFriends(item.nStageNumber);
        }
    }

    //스테이지 버튼 세팅.
    void SetBtnStage()
    {
        if (stageRoot.activeInHierarchy == false)
        {
            stageRoot.SetActive(true);
            chapterRoot.SetActive(false);
        }

        #region 글자 설정.
        stageLevel.text = string.Format("{0}", item.nStageNumber);

        //현재 스테이지가 잠겨있지 않을 경우 글자 설정.
        if (item.eBtnState != BtnStageState.BtnLock)
        {
            //홀수 번째 챕터의 경우, 녹색.
            if (item.nChapterNumber <= 1 || item.nChapterNumber % 2 ==1)
                stageLevel.color = new Color(59f / 255f, 103f / 255f, 16f / 255f);
            //짝수 번째 챕터의 경우, 파란색.
            else
                stageLevel.color = new Color(18f / 255f, 127f / 255f, 142f / 255f);
        }
        else
        {
            //잠긴 챕터는 회색.
            stageLevel.color = new Color(80f / 255f, 80f / 255f, 80f / 255f);
        }
        #endregion 글자 설정.

        #region 꽃 설정.
        //연출이 있을 경우, 연출.
        if (item.bAction == true)
        {
            UIPopupStage._instance.FlowerAction = () => StartCoroutine(CoStateChangeAction());
        }
        //연출 없는 경우 설정.
        else if (item.eBtnState == BtnStageState.none)
        {
            flower.enabled = false;
        }
        else
        {
            setFlowerSprite();
        }
        #endregion 꽃 설정.

        #region 퀘스트 설정.
        if (item.questData != null && item.eBtnState != BtnStageState.BtnLock)
        {
            string iconName = "";
            iconName = GetMissionIconName();

            //퀘스트 진행도에 따라 이미지 이름 추가 설정.
            if (item.questProg1 == 1)
            {
                iconName += "2";
            }
            else
            {
                iconName += "1";
            }
            stageQuestIcon.spriteName = iconName;
            stageQuestIcon.MakePixelPerfect();
            stageQuestIcon.width = (int)(stageQuestIcon.width * 0.6f);
            stageQuestIcon.height = (int)(stageQuestIcon.height * 0.6f);
            stageQuestIcon.gameObject.SetActive(true);
        }
        else
        {
            stageQuestIcon.gameObject.SetActive(false);
        }
        #endregion 퀘스트 설정.

        #region 이벤트 UI 설정
        
        //이벤트 아이콘 띄우는 설정
        SetEventIcon(this.item.nStageNumber);

        #endregion
    }

    string GetMissionIconName()
    {
        string iconName = "";
        //아이템 타입에 따라 이미지 이름 설정.
        if (item.questData.type == QuestType.chapter_Duck)
        {
            iconName = "Mission_DUCK_";
        }
        else if (item.questData.type == QuestType.chapter_Candy)
        {
            iconName = "Mission_CANDY_";
        }
        return iconName;
    }

    IEnumerator CoStateChangeAction()
    {   
        if (flower.enabled == false)
            flower.enabled = true;
        flower.transform.localPosition = new Vector3(0f, -10f, 0f);

        int stageState = 1;
        if (PlayerPrefs.HasKey("ActionFlowerState") == true)
        {
            stageState = PlayerPrefs.GetInt("ActionFlowerState");
            PlayerPrefs.DeleteKey("ActionFlowerState");
            item.bAction = false;
        }
        //이전 꽃 이미지로 세팅.
        string flowerName = string.Format("stage_icon_level_0{0}", stageState);
        flower.spriteName = flowerName;
        flower.MakePixelPerfect();

        //새로운 꽃 이미지로 세팅.
        yield return new WaitForSeconds(0.5f);
        flower.transform.DOScale(0f, 0.3f).SetEase(Ease.InOutBack);
        DOTween.ToAlpha(() => flower.color, x => flower.color = x, 0f, 0.25f);
        yield return new WaitForSeconds(0.3f);
        setFlowerSprite();
        ManagerSound.AudioPlay(AudioLobby.m_boni_wow);
        flower.transform.localScale = Vector3.zero;
        flower.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutBack);
        DOTween.ToAlpha(() => flower.color, x => flower.color = x, 1f, 0.1f);
    }

    void setFlowerSprite()
    {
        if (flower.enabled == false)
            flower.enabled = true;
        flower.transform.localPosition = new Vector3(0f, -10f, 0f);

        if (item.eBtnState == BtnStageState.BtnLock)
        {
            flower.spriteName = "stage_icon_level_lock";
        }
        else if (item.eBtnState == BtnStageState.BtnSeed)
        {
            flower.spriteName = "stage_icon_level_01";
        }
        else if (item.eBtnState == BtnStageState.BtnLeaf)
        {
            flower.spriteName = "stage_icon_level_02";
        }
        else if (item.eBtnState == BtnStageState.BtnFlower)
        {
            flower.spriteName = "stage_icon_level_03";
        }
        else if (item.eBtnState == BtnStageState.BtnBlueFlower)
        {
            flower.spriteName = "stage_icon_level_04";
        }
        else if (item.eBtnState == BtnStageState.BtnRedFlower)
        {
            flower.spriteName = "stage_icon_level_05";
        }
        flower.MakePixelPerfect();
    }

    void SetArrowDirection(BtnStageArrowDir _dir)
    {
        switch (_dir)
        {
            case BtnStageArrowDir.none:
                arrow.enabled = false;
                break;
            case BtnStageArrowDir.left:
                arrow.enabled = true;
                arrow.transform.localPosition = new Vector3(-87f, 5f, 0f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                arrow.transform.localScale = new Vector3(-1f, 1f, 1f);
                break;
            case BtnStageArrowDir.right:
                arrow.enabled = true;
                arrow.transform.localPosition = new Vector3(83.5f, 5f, 0f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                arrow.transform.localScale = Vector3.one;
                break;
            case BtnStageArrowDir.down:
                arrow.enabled = true;
                arrow.transform.localPosition = new Vector3(0f, -81.5f, 0f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                arrow.transform.localScale = Vector3.one;
                break;
            case BtnStageArrowDir.up:
                arrow.enabled = true;
                arrow.transform.localPosition = new Vector3(0f, 90.0f, 0f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                arrow.transform.localScale = Vector3.one;
                break;
            default:
                break;
        }

        //마지막 클리어 한 스테이지 정보를 받아와 화살표 이미지 세팅.
        if (item.eBtnState != BtnStageState.BtnLock && item.eBtnState != BtnStageState.none 
            && item.eBtnState != BtnStageState.BtnComingSoon && item.eBtnState != BtnStageState.BtnCheerUp)
        {
            //화살표 이미지 설정.
            if (item.nChapterNumber <= 1 || item.nChapterNumber % 2 == 1)
            {
                //arrow.spriteName = "stage_line_01";
                arrow.color = lineColorGreen;
            }
            else
            {
                //arrow.spriteName = "stage_line_02";
                arrow.color = lineColorBlue;
            }
        }
        else
        {
            //arrow.spriteName = "stage_line_00";
            arrow.color = lineColorGrey;
        }
    }

    //챕터 버튼 세팅.
    void SetBtnChapter()
    {
        if (chapterRoot.activeInHierarchy == false)
        {
            stageRoot.SetActive(false);
            chapterRoot.SetActive(true);
        }
        
        chapterNumber.text = string.Format("ep.{0}", item.nChapterNumber);
        chapterNumberShadow.text = string.Format("ep.{0}", item.nChapterNumber);

        //현재 챕터가 잠겨있지 않을 경우 글자 설정.
        if (item.eBtnState != BtnStageState.BtnLock && item.eBtnState != BtnStageState.BtnComingSoon
            && item.eBtnState != BtnStageState.BtnCheerUp)
        {
            //홀수 번째 챕터의 경우, 녹색.
            if (item.nChapterNumber <= 1 || item.nChapterNumber % 2 == 1)
                chapterNumber.effectColor = new Color(24f / 255f, 143f / 255f, 94f / 255f);
            //짝수 번째 챕터의 경우, 파란색.
            else
                chapterNumber.effectColor = new Color(24f / 255f, 119f / 255f, 143f / 255f);
        }
        else
        {
            //잠긴 챕터의 경우, 회색.
            chapterNumber.effectColor = new Color(100f / 255f, 100f / 255f, 100f / 255f);
        }

        #region 퀘스트 설정.
        if (item.questData != null && item.eBtnState != BtnStageState.BtnLock && item.eBtnState != BtnStageState.BtnComingSoon 
            && item.eBtnState != BtnStageState.BtnCheerUp)
        {
            chapterNumber.transform.localPosition = new Vector3(-3f, 38f, 0f);

            chapterQuestIcon.spriteName = GetSignIconName();
            chapterQuestIcon.MakePixelPerfect();
            chapterQuestIcon.width = (int)(chapterQuestIcon.width * 0.6f);
            chapterQuestIcon.height = (int)(chapterQuestIcon.height * 0.6f);

            chapterQuestCount.text = string.Format("{0}/{1}", item.questProg2, item.questData.targetCount);
            chapterQuestBox.gameObject.SetActive(true);
        }
        else
        {
            chapterNumber.transform.localPosition = new Vector3(-5f, 10f, 5f);
            chapterQuestBox.gameObject.SetActive(false);
        }
        #endregion 퀘스트 설정.

        #region 파랑새 설정.
        if (item.eBtnState == BtnStageState.BtnCheerUp)
        {
            chapterNumber.transform.localPosition = new Vector3(-3f, 38f, 0f);
        }
        #endregion

        // 챕터 아이콘에 이벤트 아이콘이 표시되지 않도록
        if (eventIcon.gameObject.activeSelf)
            eventIcon.gameObject.SetActive(false);
    }

    //커밍순 버튼 세팅.
    void SetBtnComingSoon()
    {
        stageRoot.SetActive(false);
        chapterRoot.SetActive(false);
    }

    /// <summary>
    /// 이벤트 아이콘 설정하는 함수
    /// </summary>
    private void SetEventIcon(int stageNum)
    {
        #region 알파벳 이벤트
        if (ManagerAlphabetEvent.instance != null)
        {
            if (ManagerAlphabetEvent.instance.canPlayStageIndex != 0 
                && ManagerAlphabetEvent.instance.IsCanPlayEvent_AtStage_RealTime(stageNum) == true)
            {
                eventIcon.gameObject.SetActive(true);
                return;
            }
        }

        eventIcon.gameObject.SetActive(false);
        #endregion
    }

    string GetSignIconName()
    {
        string iconName = "";
        //아이템 타입에 따라 이미지 이름 설정.
        if (item.questData.type == QuestType.chapter_Duck)
        {
            iconName = "Mission_DUCK_2";
        }
        else if (item.questData.type == QuestType.chapter_Candy)
        {
            iconName = "Mission_CANDY_2";
        }
        return iconName;
    }

    void OnClickStage()
    {
        if (item == null)
            return;

        //Chapter 표시있는 버튼이 아니고 잠겨있지 않았을 때 스테이지로 이동가능.
        if( item.eBtnState != BtnStageState.BtnLock )
        {
            if (item.bChapterUI == false )
            {
                //터치 불가면 동작안함.
                if (UIPopupStage._instance.bCanTouch == false)
                    return;
                UIPopupStage._instance.bCanTouch = false;

                Global.SetGameType_NormalGame(item.nStageNumber);

                ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch);
            }
            else if( item.questData != null )
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupEpisodeQuestInfo>();
                popup.item = item;

            }
        }
        
    }

    void OnTouch()
    {
        if (UIPopupStage._instance != null)
        {
            UIPopupStage._instance.bCanTouch = true;
        }
    }

    private void LoadFriends(int stageIndex)
    {
        var friends = UIPopupStage._instance.FriendManager.GetFriends(stageIndex);

        if(friends != null)
        {
            friends.Sort(new GameStarComparer());
        }

        SetFriends(friends);
    }

    private void SetFriends(List<UserFriend> friends)
    {
        for (int i = 0; i < profileItem.Length; i++)
        {
            if (friends != null && i < friends.Count)
            {
                profileItem[i].gameObject.SetActive(true);

                //프로필 아이템 추가
                profileItem[i].SetProfile(friends[i]);
            }
        }
    }

    private void FriendsOff()
    {
        for (int i = 0; i < profileItem.Length; i++)
        {
            profileItem[i].gameObject.SetActive(false);
        }
    }

    public void OpenFriendsInfo()
    {
        if (item == null) return;

        bool isActive = false;

        foreach (var texture in profileItem)
        {
            if (texture.gameObject.activeSelf)
            {
                isActive = true;
                break;
            }
        }

        if (!isActive) return;

        ManagerUI._instance.OpenPopup<UIPopupRankingOnStage>((popup) => popup.Init(item.nStageNumber));
    }
}
