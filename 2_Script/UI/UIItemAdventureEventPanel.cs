using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureEventPanel : UIItemAdventureTopPanel
{
    public static UIItemAdventureEventPanel _instance;

    [Header("EventPanel Object")]
    [SerializeField] private UIScrollView mainScroll;
    [SerializeField] private UIItemAdventureEventStep[] steps;
    [SerializeField] private Transform animalIcon;
    [SerializeField] private Transform bossIcon;
    [SerializeField] private GameObject whiteFlag;
    [SerializeField] private Animation bossAniController;
    [SerializeField] private UIButtonAdventureEventShortcuts heroShortcutButton;
    [SerializeField] private UIButtonAdventureEventShortcuts bossShortcutButton;

    [Header("Animation Curve")]
    [SerializeField] private AnimationCurve animalMoveCurve;

    private void Awake()
    {
        _instance = this;
    }

    public void InitEventPanel(int stageCount, int selectIndex)
    {
        SetStep(stageCount);
        SetAnimalIcon(selectIndex);
        SetBossIcon(stageCount);
        SetShortcutButtons(stageCount);

        SetScrollPos((int)animalIcon.localPosition.x);

        if (ManagerAdventure.EventData.IsAdvEventStageFirstCleard())
        {
            StartCoroutine(CoClearAni());
        }
    }

    private Transform previousStep = null;
    private void SetStep(int count)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            if (i < count)
            {
                steps[i].InitStep(i + 1, previousStep);
                steps[i].gameObject.SetActive(true);

                previousStep = steps[i].transform;
            }
            else
            {
                steps[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetAnimalIcon(int stageIndex)
    {
        if(!steps[stageIndex - 1].StepActive)
        {
            stageIndex = 1;
            ManagerAdventure.EventData.SetLastPlayStage(1);
        }

        steps[stageIndex - 1].SelectStep();
        animalIcon.position = steps[stageIndex - 1].transform.position;
    }

    private const int BOSS_SPACING = 150;
    private void SetBossIcon(int stageCount)
    {
        Vector3 bossPos = bossIcon.localPosition;
        bossPos.x = steps[stageCount - 1].transform.localPosition.x + BOSS_SPACING;
        bossIcon.localPosition = bossPos;

        var clearState = ManagerAdventure.EventData.GetClearState(stageCount);

        switch (clearState)
        {
            case ManagerAdventure.EventData.ClearState.Clear:
                bossAniController.gameObject.SetActive(false);
                whiteFlag.SetActive(true);
                break;
            case ManagerAdventure.EventData.ClearState.NonClear:
            case ManagerAdventure.EventData.ClearState.FirstClear:
                bossAniController.gameObject.SetActive(true);
                whiteFlag.SetActive(false);
                break;
            default:
                break;
        }
    }

    protected override void OnVsTextureLoaded(int idx)
    {
        if (idx == -1)
        {
            if (boss.bossTextures != null)
            {
                boss.bossTextures.MakePixelPerfect();
                boss.bossTextures.width = (int)(boss.bossTextures.width * 0.8f);
                boss.bossTextures.height = (int)(boss.bossTextures.height * 0.8f);

                var effect = boss.bossTextures.gameObject.AddComponent<AuraEffect_RenderTexture>();
                effect.Make
                (
                    ObjectMaker.Instance.GetPrefab("AuraEffect_01"),
                    new Color(219.0f / 255.0f, 0.0f / 255.0f, 126.0f / 255.0f, 150.0f / 255.0f)
                );
            }
        }
        else
        {
            if (animals[idx].animalTextures != null)
            {
                animals[idx].animalTextures.MakePixelPerfect();
                animals[idx].animalTextures.width = (int)(animals[idx].animalTextures.width * 0.64f);
                animals[idx].animalTextures.height = (int)(animals[idx].animalTextures.height * 0.64f);
            }
        }
    }

    private void SetShortcutButtons(int stageCount)
    {
        heroShortcutButton.Init(animalIcon.transform);
        bossShortcutButton.Init(steps[stageCount - 1].transform);
    }

    protected override void SetBossAttribute(int value)
    {
        base.SetBossAttribute(value);
        bossShortcutButton.Attribute = value;
    }

    public override void SetWeight()
    {
        base.SetWeight();

        foreach (var animal in animals)
        {
            animal.weightIcon.AddWeigth(
                new UIItemWeightIcon.WeightInfo()
                {
                    weightType = UIItemWeightIcon.WeightType.EventBonus,
                    weight = ManagerAdventure.EventData.GetAdvEventBonus(animal.animalIndex)
                }
            );

            //animal.weightIcon.AddWeigth(
            //    new UIItemWeightIcon.WeightInfo()
            //    {
            //        weightType = UIItemWeightIcon.WeightType.EventBonus,
            //        weight = -15
            //    }
            //);

            //animal.weightIcon.AddWeigth(
            //    new UIItemWeightIcon.WeightInfo()
            //    {
            //        weightType = UIItemWeightIcon.WeightType.EventBonus,
            //        weight = 12
            //    }
            //);

            animal.weightIcon.OnIcon();
        }
    }

    public void SetScrollPos(int position_x)
    {
        const int MIN_RANGE = 70;
        const int MAX_RANGE = -80;
        
        int stepLength = (ManagerAdventure.EventData.GetAdvEventStageCount() - 1) * UIItemAdventureEventStep.HORIZONTAL_SPACING;

        int minX = MIN_RANGE;
        int maxX = stepLength + MAX_RANGE;

        SpringPanel.Begin(mainScroll.gameObject, Vector3.left * Mathf.Clamp(position_x, minX, maxX), 8.0f);
    }

    public event System.Action<int> MoveEndEvent;

    public void MoveAnimalIcon(int stageIndex)
    {
        StartCoroutine(CoMoveAnimalIcon(stageIndex));
    }

    private IEnumerator CoMoveAnimalIcon(int stageIndex)
    {
        mainScroll.enabled = false;

        const float playTime = 0.5f;

        Vector2 startPos = animalIcon.position;
        Vector2 endPos = steps[stageIndex - 1].transform.position;
        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimeLobby;

            float currentTime = totalTime / playTime;
            animalIcon.transform.position = Vector2.Lerp(startPos, endPos, animalMoveCurve.Evaluate(currentTime));

            if (currentTime >= 1.0f)
                break;

            yield return null;
        }

        SetScrollPos(Mathf.RoundToInt(animalIcon.transform.localPosition.x));

        steps[stageIndex - 1].SelectStep();

        mainScroll.enabled = true;

        MoveEndEvent?.Invoke(stageIndex);
    }

    private IEnumerator CoClearAni()
    {
        ClickBlocker.Make(Mathf.Infinity);

        int selectIndex = ManagerAdventure.EventData.GetLastPlayStage();
        int nextIndex = Mathf.Clamp(selectIndex + 1, 0, ManagerAdventure.EventData.GetAdvEventStageCount());

        if (selectIndex == nextIndex)
        {
            yield return CoAllClearAni();
        }
        else
        {
            
            var selectStep = steps[selectIndex - 1];
            var nextStep = steps[nextIndex - 1];

            yield return nextStep.CoOpenAni_1();

            yield return new WaitForSeconds(1.0f);

            yield return CoMoveAnimalIcon(nextIndex);

            yield return selectStep.CoClearAni();

            yield return nextStep.CoOpenAni_2();

        }

        ManagerAdventure.EventData.SetLastPlayStage(nextIndex);

        ClickBlocker.Make(0.0f);
    }

    private IEnumerator CoAllClearAni()
    {
        int selectIndex = ManagerAdventure.EventData.GetLastPlayStage();
        var selectStep = steps[selectIndex - 1];

        yield return new WaitForSeconds(1.0f);

        ManagerSound.AudioPlay(AudioInGame.ENEMY_DISAPPEAR_1);
        bossAniController.Play("Enemy_Dead_UI");

        yield return new WaitUntil(() => { return Mathf.Approximately(boss.bossTextures.alpha, 0.0f); });

        bossAniController.gameObject.SetActive(false);
        whiteFlag.SetActive(true);

        yield return selectStep.CoClearAni();

        yield return CoCallServer();
    }

    private IEnumerator CoCallServer()
    {
        if (ServerRepos.UserEventAdventure.state != 1) yield break;

        bool complete = false;

        var arg = new Protocol.AdventureGetChapterClearRewardReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex
        };

        Protocol.AppliedRewardSet rewardSet = null;
        ServerAPI.GetEventAdventureClearReward
        (
            arg,
            (clearRewardResp) => {
                complete = true;

                //보상 적용
                rewardSet = clearRewardResp.clearReward;

                foreach (var reward in clearRewardResp.rewards)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        reward.type,
                        reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PLAY,
                        null
                    );
                }
            }
        );

        yield return new WaitUntil(() => complete);

        ClickBlocker.Make(0.0f);

        OpenPopupGetReward(rewardSet);
    }

    private void UpdateUI()
    {
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.flower = (int)GameData.User.flower;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();
    }

    private void OpenPopupGetReward(Protocol.AppliedRewardSet rewardSet)
    {
        //보상 획득한 상태라면, 획득한 보상들 한번에 출력
        if (rewardSet != null)
        {
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (Global._instance.GetString("n_ev_1"),
                () => { UpdateUI(); },
                rewardSet);
        }
    }
}
