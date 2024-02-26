using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemAdventureEventStep : MonoBehaviour
{
    public static int selectStepIndex { private set; get; }
    private int stepIndex;

    [SerializeField] private UISprite stepSprite;
    [SerializeField] private GameObject road;
    [SerializeField] private GenericReward reward;

    private bool _stepActive = false;
    public bool StepActive
    {
        private set
        {
            _stepActive = value;

            if (ManagerAdventure.EventData.IsAdvEventStageBossStage(stepIndex))
            {
                stepSprite.spriteName = _stepActive ? "adven_event_step_boss_on" : "adven_event_step_boss_off";
            }
            else
            {
                stepSprite.spriteName = _stepActive ? "adven_event_step_on" : "adven_event_step_off";
            }

            stepSprite.MakePixelPerfect();
        }
        get
        {
            return _stepActive;
        }
    }

    public void InitStep(int stepIndex, Transform previousStep = null)
    {
        this.stepIndex = stepIndex;

        var clearState = ManagerAdventure.EventData.GetClearState(stepIndex);

        SetTexture(clearState);
        SetStar(clearState);
        SetHeight();
        SetRoad(previousStep);
        SetReward(clearState);
    }
    
    private void SetTexture(ManagerAdventure.EventData.ClearState clearState)
    {
        bool isActive = false;

        switch (clearState)
        {
            case ManagerAdventure.EventData.ClearState.NonClear:
                isActive = IsLastOpenStep();
                break;
            case ManagerAdventure.EventData.ClearState.Clear:
                isActive = true;
                break;
            case ManagerAdventure.EventData.ClearState.FirstClear:
                isActive = true;
                break;
            default:
                break;
        }

        StepActive = isActive;
    }

    [SerializeField] private GameObject star;
    
    private void SetStar(ManagerAdventure.EventData.ClearState clearState)
    {
        switch (clearState)
        {
            case ManagerAdventure.EventData.ClearState.NonClear:
                star.SetActive(false);
                break;
            case ManagerAdventure.EventData.ClearState.Clear:
                star.SetActive(true);
                break;
            case ManagerAdventure.EventData.ClearState.FirstClear:
                break;
            default:
                break;
        }
    }

    public const int HORIZONTAL_SPACING = 180;
    private const int MAX_HEIGHT = 20;
    private void SetHeight()
    {
        Vector3 position = transform.localPosition;
        position.x = (stepIndex - 1) * HORIZONTAL_SPACING;
        position.y = Mathf.RoundToInt(Mathf.Sin(GetCycle(1) * stepIndex) * MAX_HEIGHT);
        transform.localPosition = position;
    }

    private float GetCycle(int eventIndex)
    {
        Random.InitState(eventIndex);
        return Random.Range(1.7f, 3.0f);
    }

    private void SetRoad(Transform previousStep)
    {
        if (previousStep == null)
        {
            road.SetActive(false);
        }
        else
        {
            Vector3 distance = transform.localPosition - previousStep.localPosition;
            road.transform.localPosition = distance * -0.5f;

            float angle = Mathf.Atan2(road.transform.localPosition.y, road.transform.localPosition.x) * Mathf.Rad2Deg;
            road.transform.localEulerAngles = Vector3.forward * angle;

            road.SetActive(true);
        }
    }

    private void SetReward(ManagerAdventure.EventData.ClearState clearState)
    {
        reward.gameObject.SetActive(false);

        if (clearState == ManagerAdventure.EventData.ClearState.NonClear)
        {
            Reward rewardData = null;
            foreach (var item in ManagerAdventure.EventData.GetAdvEventStageRewards(stepIndex))
            {
                rewardData = item;
                break;
            }

            if(rewardData != null)
            {
                //최초 클리어 보상이 날개이면서 갯수가 1개이면 보여주지 않는다.(2개 이상의 날개를 보상으로 줄 경우에만 보여준다.)
                if((RewardType)rewardData.type == RewardType.wing && rewardData.value == 1) return;

                reward.SetReward(rewardData);
                reward.gameObject.SetActive(true);
                return;
            }
        }
    }

    public void SelectStep()
    {
        selectStepIndex = stepIndex;
    }

    public void OnClickStep()
    {
        if(StepActive)
        {
            UIItemAdventureEventPanel._instance.MoveAnimalIcon(stepIndex);
        }
    }

    private bool IsLastOpenStep()
    {
        if (stepIndex > 1)
        {
            if (!ManagerAdventure.EventData.IsAdvEventStageCleared(stepIndex) &&
                ManagerAdventure.EventData.IsAdvEventStageCleared(stepIndex - 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (!ManagerAdventure.EventData.IsAdvEventStageCleared(stepIndex))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #region Animation
    public IEnumerator CoClearAni()
    {
        const float STAR_ALPHA_TIME = 0.5f;

        var startSprite = star.GetComponent<UITexture>();
        startSprite.alpha = 0.0f;
        star.SetActive(true);

        float totalTime = 0.0f;

        while (startSprite.alpha < 1.0f)
        {
            totalTime += Global.deltaTimeLobby;
            startSprite.alpha = Mathf.Lerp(0.0f, 1.0f, totalTime / STAR_ALPHA_TIME);

            yield return null;
        }

        star.transform.DOShakePosition(0.5f, 5f, 20, 90f, false, false);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
    }

    public IEnumerator CoOpenAni_1()
    {
        StepActive = false;
        yield return null;
    }

    public IEnumerator CoOpenAni_2()
    {
        yield return null;
        StepActive = true;
    }
    #endregion
}
