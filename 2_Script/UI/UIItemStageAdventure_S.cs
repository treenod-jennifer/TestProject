using System.Collections;
using UnityEngine;

public class StageAdventure_S_ItemData
{
    public enum CheckBoxState
    {
        None,
        Check_1,
        Check_2
    }

    public enum RewardState
    {
        None,
        Check,
        Lock
    }


    public int stageType = 0;
    public int chapterNumber;
    public int stageNumber;
    public int mission = 0;

    public ManagerAdventure.StageReward rewards;

    public CheckBoxState stageCheck;
    public bool missionCleared;

    public StageAdventure_S_ItemData(int chapterNumber, int stageNumber, ManagerAdventure.StageReward reward, int stageType, int mission)
    {
        this.chapterNumber = chapterNumber;
        this.stageNumber = stageNumber;
        this.rewards = reward;
        this.stageType = stageType;
        this.mission = mission;
    }
}

public class UIItemStageAdventure_S : MonoBehaviour {
    private enum StageMode
    {
        Normal,
        Bonus,
    }
    private enum SubMissionType
    {
        NonMission,
        NonClear,
        Clear
    }

    private enum StageClearState
    {
        Locked,
        Opened,
        Cleared
    }
    [SerializeField] UISprite mainOpacitySprite;
    [SerializeField] UISprite backgroundSprite;
    [SerializeField] UISprite cutlineSprite;

    [SerializeField] UILabel[] stageNumber;
    [SerializeField] UILabel[] challengeStageNumber;
    [SerializeField] UILabel rewardLabel;
    [SerializeField] UILabel missionLabel;
    [SerializeField] UILabel[] hardStageLabel;
    [SerializeField] GenericReward[] rewards;

    [SerializeField] GameObject frame_Normal;
    [SerializeField] UISprite focusFrameSprite;

    [SerializeField] GameObject subMission_Non;
    [SerializeField] GameObject subMission_Clear;

    [SerializeField] UIPokoButton button;
    [SerializeField] GameObject button_Green;
    [SerializeField] GameObject button_Black;

    [SerializeField] UILabel[] buttonLables_Active;
    [SerializeField] UILabel[] buttonLables_Clear;
    [SerializeField] UILabel[] buttonLables_Inactive;

    [SerializeField] GameObject check_Locked;
    [SerializeField] GameObject check_Unchecked;
    [SerializeField] GameObject check_checked;

    [SerializeField] GameObject clearedLabel;
    [SerializeField] UITweener clearedLabelTween;

    [SerializeField] Animation anim;



    private StageAdventure_S_ItemData data;
    private StageMode stageMod
    {
        set
        {
            switch (value)
            {
                case StageMode.Normal:
                    {
                        for (int i = 0; i < stageNumber.Length; ++i)
                            stageNumber[i].gameObject.SetActive(true);
                        for (int i = 0; i < hardStageLabel.Length; ++i)
                            hardStageLabel[i].gameObject.SetActive(false);
                        var btnSpr = button_Green.GetComponent<UISprite>();
                        var btnSpr2 = button_Black.GetComponent<UISprite>();
                        if (btnSpr != null && btnSpr2 != null)
                        {
                            btnSpr.spriteName = "button_play";
                            btnSpr2.spriteName = "button_play";
                        }
                            
                        focusFrameSprite.spriteName = "popup_back_02";
                        focusFrameSprite.width = 690;
                        focusFrameSprite.height = 108;
                        this.backgroundSprite.spriteName = "popup_back_01";
                        this.cutlineSprite.spriteName = "popup_line";

                        buttonLables_Active.SetEffectColor(new Color32(0x28, 0x6b, 0x12, 0xFF));
                        buttonLables_Inactive.SetEffectColor(new Color32(0x2c, 0x5a, 0x1d, 0xFF));
                        buttonLables_Clear.SetEffectColor(new Color32(0x28, 0x71, 0xB0, 0xFF));
                    }
                    break;

                case StageMode.Bonus:
                    {
                        for (int i = 0; i < stageNumber.Length; ++i)
                            stageNumber[i].gameObject.SetActive(false);

                        for (int i = 0; i < challengeStageNumber.Length; ++i)
                            challengeStageNumber[i].gameObject.SetActive(true);

                        for (int i = 0; i < hardStageLabel.Length; ++i)
                            hardStageLabel[i].gameObject.SetActive(true);

                        var btnSpr = button_Green.GetComponent<UISprite>();
                        var btnSpr2 = button_Black.GetComponent<UISprite>();
                        if (btnSpr != null && btnSpr2 != null)
                        {
                            btnSpr.spriteName = "button_play04";
                            btnSpr2.spriteName = "button_play04";
                        }
                            
                        focusFrameSprite.spriteName = "popup_back_17";
                        focusFrameSprite.width = 700;
                        focusFrameSprite.height = 131;
                        this.cutlineSprite.spriteName = "popup_line02";
                        this.backgroundSprite.spriteName = "popup_back_18";

                        buttonLables_Active.SetEffectColor(new Color32(0x65, 0x2B, 0x7C, 0xFF));
                        buttonLables_Inactive.SetEffectColor(new Color32(0x65, 0x2B, 0x7C, 0xFF));
                        buttonLables_Clear.SetEffectColor(new Color32(0x65, 0x2B, 0x7C, 0xFF));
                    }
                    break;
            }
        }
    }
    private SubMissionType subMission
    {
        set
        {
            subMission_Non.transform.parent.gameObject.SetActive(value != SubMissionType.NonMission);
            if(value == SubMissionType.NonMission)
            {
                var orgPos = rewards[0].transform.localPosition;
                orgPos.x = -24f;
                if( rewards.Length > 0)
                    rewards[0].transform.localPosition = orgPos;
            }

            switch (value)
            {
                case SubMissionType.NonMission:
                    
                    break;
                case SubMissionType.NonClear:
                    subMission_Non.SetActive(true);
                    subMission_Clear.SetActive(false);
                    break;
                case SubMissionType.Clear:
                    subMission_Non.SetActive(false);
                    subMission_Clear.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private StageClearState ClearState
    {
        set
        {
            switch(value)
            {
                case StageClearState.Locked:
                    {
                        button.functionName = "";
                        button_Green.SetActive(false);
                        button_Black.SetActive(true);
                        clearedLabel.SetActive(false);

                        check_checked.SetActive(false);
                        check_Locked.SetActive(true);
                        check_Unchecked.SetActive(false);

                        stageNumber[0].color = new Color(0x7f / 255f, 0x86 / 255f, 0x8a / 255f);
                        challengeStageNumber[0].color = new Color(0x7f / 255f, 0x86 / 255f, 0x8a / 255f);
                        challengeStageNumber[0].effectStyle = UILabel.Effect.None;
                        hardStageLabel[0].color = new Color(0x7f / 255f, 0x86 / 255f, 0x8a / 255f);
                        hardStageLabel[0].effectStyle = UILabel.Effect.None;
                        hardStageLabel[1].gameObject.SetActive(false);

                        rewardLabel.color = new Color32(0x7f, 0x86, 0x8a, 0xff);
                        if (rewards.Length > 0)
                        {
                            rewards[0].SetColor(new Color(0f, 0f, 0f, 0.3f));
                            rewards[0].EnableInfoBtn(false);
                        }
                    }
                    break;
                case StageClearState.Opened:
                    {
                        button.functionName = "OnClickStart";
                        button_Green.SetActive(true);
                        button_Black.SetActive(false);
                        clearedLabel.SetActive(false);

                        check_checked.SetActive(false);
                        check_Locked.SetActive(false);
                        check_Unchecked.SetActive(true);

                        stageNumber[0].color = new Color(0x2A / 255f, 0x5d / 255f, 0x8b / 255f);
                        challengeStageNumber[0].color = new Color32(0xff, 0xff, 0xff, 0xff);
                        challengeStageNumber[0].effectStyle = UILabel.Effect.Outline8;
                        hardStageLabel[0].color = new Color32(0xff, 0xff, 0xff, 0xff);
                        hardStageLabel[0].effectStyle = UILabel.Effect.Outline8;
                        hardStageLabel[1].gameObject.SetActive(true);
                        rewardLabel.color = data.stageType == 1 ? new Color32(0x77, 0x5e, 0x87, 0xff) : new Color32(0x54, 0x7F, 0xA3 , 0xff); //54 7F A3FF

                        if (rewards.Length > 0)
                            rewards[0].SetColor(Color.white);
                    }
                    break;
                case StageClearState.Cleared:
                    {
                        button.functionName = "OnClickStart";
                        button_Green.SetActive(true);
                        button_Black.SetActive(false);
                        clearedLabel.SetActive(true);

                        check_checked.SetActive(true);
                        check_Locked.SetActive(false);
                        check_Unchecked.SetActive(false);

                        stageNumber[0].color = new Color(0x2A / 255f, 0x5d / 255f, 0x8b / 255f);
                        challengeStageNumber[0].color = new Color32(0x71, 0x4D, 0xA1, 0xFF); ;
                        challengeStageNumber[0].effectStyle = UILabel.Effect.None;
                        hardStageLabel[0].color = new Color32(0xff, 0xff, 0xff, 0xff);
                        hardStageLabel[0].effectStyle = UILabel.Effect.Outline8;
                        hardStageLabel[1].gameObject.SetActive(true);
                        rewardLabel.color = data.stageType == 1 ? new Color32(0x77, 0x5e, 0x87, 0xff) : new Color32(0x54, 0x7F, 0xA3, 0xff); //54 7F A3FF
                        if (rewards.Length > 0)
                        {
                            rewards[0].SetColor(Color.white);
                            rewards[0].EnableCheck(true);
                        }
                            


                    }
                    break;
            }
        }
    }

    

    public void SettingItem(StageAdventure_S_ItemData data)
    {
        this.data = data;

        string stageNumberString = string.Format("{0}-{1}", data.chapterNumber.ToString(), (data.stageNumber % 100).ToString());
        stageNumber.SetText(stageNumberString);

        if (data.mission == 0)
            subMission = SubMissionType.NonMission;
        else
        {
            subMission = data.missionCleared == true ? SubMissionType.Clear : SubMissionType.NonClear;
        }

        if (data.rewards.reward != null)
        {
            if (data.stageType == 1 && data.rewards.reward.type == (int)(RewardType.flower))
            {
                rewards[0].AddSpriteOverride("stage_icon_level_03", "stage_icon_level_04");
            }
            else
            {
                rewards[0].RemoveSpriteOverride("stage_icon_level_03");
            }

            rewards[0].SetReward(data.rewards.reward);
            rewards[0].gameObject.SetActive(true);
            rewards[0].EnableLock(false);
            rewards[0].EnableCheck(false);

            
        }
        else
        {
            rewards[0].gameObject.SetActive(false);
        }

        stageMod = (StageMode)data.stageType;

        var clearState = GetClearState(data.chapterNumber, data.stageNumber);
        if(UIPopupStageAdventure.nowStageClear)
        {
            if (data.chapterNumber == UIPopupStageAdventure.prevChapterCursor && data.stageNumber == UIPopupStageAdventure.prevStageCursor)
            {
                ClearState = StageClearState.Opened;
            }
            else if (data.chapterNumber == ManagerAdventure.User.GetChapterCursor() && ManagerAdventure.User.GetStageCursor() == data.stageNumber)
            {
                ClearState = StageClearState.Locked;
            }
            else if (data.stageType == 1 && data.chapterNumber == UIPopupStageAdventure.selectedChapter && data.stageNumber == UIPopupStageAdventure.stageCursor)
            {
                ClearState = StageClearState.Locked;
            }
            else
            {
                ClearState = clearState;
            }

            if ( data.stageType == 1 )
            {
                int lastUnfinishedChap = ManagerAdventure.User.GetLastUnfinishedChapter();
                if (lastUnfinishedChap != 0)
                {
                    this.mainOpacitySprite.color = new Color32(0xff, 0xff, 0xff, 0x01);
                    //this.frame_Normal.SetActive(false);
                }

                this.frame_Normal.SetActive(false);
            }
        }
        else
        {
            ClearState = clearState;
            if (data.stageType == 1)
            {
                this.frame_Normal.SetActive(clearState == StageClearState.Opened);
            }
            else
            {
                this.frame_Normal.SetActive(clearState == StageClearState.Opened && UIPopupStageAdventure.allClearedAndNothingToExpress == false);
            }
        }

        if (ManagerAdventure.Stage.IsLastStageInChapter(data.chapterNumber, data.stageNumber))
        {
            this.backgroundSprite.transform.localPosition = Vector3.zero;
            this.backgroundSprite.height = 120;
            this.cutlineSprite.gameObject.SetActive(false);
        }
        else
        {
            this.backgroundSprite.transform.localPosition = Vector3.up * -10;
            this.backgroundSprite.height = 140;
            this.cutlineSprite.gameObject.SetActive(true);
        }
    }

    public void OnClickCheck()
    {
        Debug.Log("OnClickCheck");
    }

    public void OnClickStart()
    {
        if (!UIPopupStageAdventure._instance.bCanTouch)
            return;

        UIPopupStageAdventure._instance.bCanTouch = false;

        Global.SetGameType_Adventure(data.chapterNumber, data.stageNumber);
        ManagerAdventure.User.SaveLastSelectedStage(data.chapterNumber, data.stageNumber);

        if (IsOpenReady)
            return;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);

        string stageName = Global.GameInstance.GetStageFilename();
        StageUtility.StageMapDataLoad(stageName, (mapData) => {
            IsOpenReady = false;
            NetworkLoading.EndNetworkLoading();

            if (mapData != null)
            {
                ManagerUI._instance.OpenPopupStageAdventureReady(mapData);
            }
            else
            {
                ErrorController.ShowNetworkErrorDialogAndRetry("", () => IsOpenReady = false);
            }
        });
    }

    bool IsOpenReady = false;

    private StageClearState GetClearState(int chapterNumber, int stageNumber)
    {
        if (CurrentCheck(chapterNumber, stageNumber))
            return StageClearState.Opened;
        else if (!ClearCheck(chapterNumber, stageNumber))
            return StageClearState.Locked;
        else
            return StageClearState.Cleared;
    }

    private bool ClearCheck(int chapterNumber, int stageNumber)
    {
        bool clearCheck = false;

        var chapterProgress = ManagerAdventure.User.GetChapterProgress(chapterNumber);

        if (chapterProgress != null)
        {
            var stageProgress = chapterProgress.GetStageProgress(stageNumber);
            if(stageProgress != null)
                clearCheck = stageProgress.clearLevel > 0;
        }

        return clearCheck;
    }

    private bool CurrentCheck(int chapterNumber, int stageNumber)
    {
        int previousChapter;
        int previousStage;
        GetPreviousStage(chapterNumber, stageNumber, out previousChapter, out previousStage);

        if (previousChapter == -1)
        {
            if (ClearCheck(chapterNumber, stageNumber))
                return false;
            else
                return true;
        }

        bool currentClear = ClearCheck(chapterNumber, stageNumber);
        bool previousClear = ClearCheck(previousChapter, previousStage);

        if (previousClear && !currentClear)
            return true;
        else
            return false;
    }

    private void GetPreviousStage(int chapterNumber, int stageNumber, out int previousChapterIndex, out int previousStageIndex)
    {
        var prevStage = ManagerAdventure.Stage.GetPrevStage(chapterNumber, stageNumber, this.data.stageType == 0);
        if( prevStage == null)
        {
            previousChapterIndex = -1;
            previousStageIndex = -1;
        }
        else
        {
            previousChapterIndex = prevStage.chapIdx;
            previousStageIndex = prevStage.stageIdx;
        }
        //if (stageNumber > 1)
        //{
        //    previousChapterIndex = chapterNumber;
        //    previousStageIndex = stageNumber - 1;
        //}
        //else if (chapterNumber > 1)
        //{
        //    previousChapterIndex = chapterNumber - 1;
        //    var chapData = ManagerAdventure.Stage.GetChapter(chapterNumber - 1);
        //    previousStageIndex = chapData.GetLastStageIdx();
        //}
        //else
        //{
        //    previousChapterIndex = -1;
        //    previousStageIndex = -1;
        //}
    }

    public int GetStageNumber()
    {
        return data.stageNumber;
    }

    public IEnumerator PlayClear()
    {
        Debug.Log(string.Format("PlayClear{0} - {1} ({2})", data.chapterNumber, data.stageNumber, data.stageType));
        anim.Stop();
        anim.Play("UIItemStageAdventure_Stage_Clear");

        while(anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }

        ClearState = StageClearState.Cleared;
    }

    public IEnumerator PlayOpen()
    {
        // 챌린지모드의 경우 맨 끝 챕터에서 오픈 아닌데 마지막 스테이지라서 잘못 오픈 처리되는 문제가 있음
        // 그래서 확인 한번 더하고 잘못열리는 경우 방지
        var realClearState = GetClearState(data.chapterNumber, data.stageNumber);
        if( realClearState != StageClearState.Opened)
        {
            ClearState = realClearState;
            yield break;
        }

        Debug.Log(string.Format("PlayOpen{0} - {1} ({2})", data.chapterNumber, data.stageNumber, data.stageType));
        anim.Stop();
        anim.Play("UIItemStageAdventure_Stage_Open");

        while (anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }
        ClearState = realClearState;
        yield break;
    }

    public IEnumerator PlayBonusStageOpen()
    {
        Debug.Log(string.Format("PlayBonusStageOpen{0} - {1} ({2})", data.chapterNumber, data.stageNumber, data.stageType));
        anim.Stop();
        anim.Play("UIItemStageAdventure_BonusStage_Open");

        while (anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }

    void PlaySound(AudioLobby audioId)
    {
        ManagerSound.AudioPlay(audioId);
    }

    void PlayIngameSound(AudioInGame audioId)
    {
        ManagerSound.AudioPlay(audioId);
    }
}
