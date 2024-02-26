using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 파랑새 위치 이동 및 애니메이션 설정
/// </summary>
public class Tutorial_Action_Bird_Setting : Tutorial_Action
{
    public BirdPositionType posType = BirdPositionType.none;
    //posType 이 none 이 아닐 경우에만 활성화.
    public BirdAnimationType aniType_Start = BirdAnimationType.NONE;
    public BirdAnimationType anitype_End = BirdAnimationType.NONE;

    private System.Action endAction = null;
    private bool isTurn = false;

    public Tutorial_Action_Bird_Setting(BirdPositionType positionType = BirdPositionType.none, BirdAnimationType animationType = BirdAnimationType.NONE)
    {
        this.posType = positionType;
        this.anitype_End = animationType;
    }

    public void Init(BirdPositionType positionType = BirdPositionType.none, BirdAnimationType animationType = BirdAnimationType.NONE)
    {
        this.posType = positionType;
        this.anitype_End = animationType;
    }

    public override void StartAction(System.Action endAction = null)
    {
        this.endAction = endAction;
        StartCoroutine(CoActionBird());
    }

    private IEnumerator CoActionBird()
    {
        yield return SetBirdAnimation_Start();
        yield return CoSetBirdPosition();
        yield return SetBirdAnimation_End();
        endAction.Invoke();
    }

    private IEnumerator CoSetBirdPosition()
    {
        if (posType == BirdPositionType.none)
            yield break;

        if (ManagerTutorial._instance.birdLive2D == null || ManagerTutorial._instance.birdType != posType)
        {
            isTurn = ManagerTutorial._instance.SettingBlueBird(posType, 0.3f);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator SetBirdAnimation_Start()
    {
        if (ManagerTutorial._instance.birdLive2D == null || aniType_Start == BirdAnimationType.NONE)
            yield break;

        yield return CoSetAnimation(aniType_Start);
    }

    private IEnumerator SetBirdAnimation_End()
    {
        if (isTurn == true)
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");
            yield return CoWaitAnimationEnd("T_idle_turn");
            ManagerTutorial._instance.BlueBirdTurn();
        }
        if (anitype_End == BirdAnimationType.NONE)
            yield break;

        yield return CoSetAnimation(anitype_End);
    }

    private IEnumerator CoSetAnimation(BirdAnimationType aniType)
    {
        bool isWaitAnimationEnd = false;
        bool isLoop = false;
        string isLoopAnimation = string.Empty;
        string aniName = "";
        switch (aniType)
        {
            case BirdAnimationType.Clear_L:
                aniName = "T_clear_L";
                isWaitAnimationEnd = true;
                ManagerSound.AudioPlay(AudioLobby.m_bird_happy);
                break;
            case BirdAnimationType.Clear_S:
                aniName = "T_clear_S";
                isWaitAnimationEnd = true;
                ManagerSound.AudioPlay(AudioLobby.m_bird_happy);
                break;
            case BirdAnimationType.IDLE_LOOP:
                aniName = "T_idle_loop";
                isLoop = true;
                break;
            case BirdAnimationType.IDLE_DOWN:
                aniName = "T_idle_Do";
                isLoopAnimation = "T_targetDo_loop";
                break;
            case BirdAnimationType.IDLE_UP:
                aniName = "T_idle_Up";
                isLoopAnimation = "T_targetUp_loop";
                break;
        }

        if(string.IsNullOrEmpty(isLoopAnimation))
            ManagerTutorial._instance.SetBirdAnimation(aniName, isLoop);
        else
            ManagerTutorial._instance.SetBirdAnimation(aniName, isLoopAnimation);

        //특정 애니메이션은 애니메이션이 끝날 때 까지 대기한 뒤, 다른 애니메이션으로 전환해줘야 함.
        if (isWaitAnimationEnd == true)
        {
            yield return CoWaitAnimationEnd(aniName);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        }
    }

    private IEnumerator CoWaitAnimationEnd(string aniName)
    {
        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation(aniName);
    }
}
