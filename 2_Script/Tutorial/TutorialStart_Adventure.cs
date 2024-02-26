using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialStart_Adventure : TutorialBase {

    private BlindTutorial blind;

    private IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region #1 로비에서 튜토리얼

        //카메라 부스로 이동
        Transform phoneBox = AdventureEntry._instance._touchTarget.transform;
        CameraController._instance.MoveToPosition(phoneBox.position, 0.25f);
        yield return new WaitForSeconds(0.3f);

        //블라인드 생성
        blind._panel.depth = 30;
        blind.SetDepth(-1);
        blind.SetSize(450, 450);
        blind.SetSizeCollider(0, 0);
        blind.transform.position = Vector3.up * 0.15f;
        blind.transform.parent = ManagerUI._instance.transform;
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //마유지 생성
        if (birdType != BirdPositionType.TopRight)
        {
            birdType = BirdPositionType.TopRight;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
        }
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m7_1"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");
        yield return new WaitForSeconds(2.0f);

        //블라인드 세팅(전화박스 어둡게)
        StartCoroutine(BlindAni(Vector2.one * 450, Vector2.zero, 0.7f));
        yield return new WaitForSeconds(0.7f);

        //모험모드 버튼 활성화
        blind.transform.position = UIButtonAdventure._instance.transform.position;
        StartCoroutine(BlindAni(Vector2.zero, Vector2.one * 300, 0.7f));

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(UIButtonAdventure._instance.transform.position + (Vector3.down * 0.05f), 0.3f);
        yield return new WaitForSeconds(0.8f);

        //블라인드 터치영역 설정
        blind.SetSizeCollider(100, 100);

        //팝업이 열릴때 까지 대기(기다리는 동안 손가락 이미지 전환)
        bool bPush = false;
        float fTimer = 0.0f;

        while (UIPopupStageAdventure._instance == null)
        {
            fTimer += Global.deltaTime * 1.0f;
            if (fTimer >= 0.15f)
            {
                if (bPush == true)
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = false;
                }
                else
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = true;
                }
                fTimer = 0.0f;
            }
            yield return null;
        }

        //블라인드 설정
        blind.SetSize(0, 0);
        blind.SetSizeCollider(0, 0);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

        #endregion

        #region #2 스테이지 선책 팝업 튜토리얼

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bool bTurn = false;
        if (birdType != BirdPositionType.BottomLeft)
        {
            birdType = BirdPositionType.BottomLeft;
            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
        }

        yield return new WaitForSeconds(1.0f);

        // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
        if (bTurn == true)
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();
        }

        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m7_2"), 0.3f);

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        finger.color = new Color(finger.color.r, finger.color.g, finger.color.b, 0.0f);
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(Init_PlayButton().position, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //1-1플레이 버튼 활성화
        SetFront_PlayButton();

        //팝업이 열릴때 까지 대기(기다리는 동안 손가락 이미지 전환)
        bPush = false;
        fTimer = 0.0f;

        while (UIPopupStageAdventureReady._instance == null)
        {
            fTimer += Global.deltaTime * 1.0f;
            if (fTimer >= 0.15f)
            {
                if (bPush == true)
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = false;
                }
                else
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = true;
                }
                fTimer = 0.0f;
            }
            yield return null;
        }

        //1-1플레이 버튼 비활성화
        SetBack_PlayButton();

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

        #endregion
         
        #region #3 모험 준비 팝업 튜토리얼

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bTurn = false;
        if (birdType != BirdPositionType.TopRight)
        {
            birdType = BirdPositionType.TopRight;
            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
        }

        yield return new WaitForSeconds(1.0f);

        // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
        if (bTurn == true)
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            yield return null;

            ManagerTutorial._instance.BlueBirdTurn();
        }

        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");
        yield return new WaitForSeconds(0.5f);

        //스타트 버튼 활성화
        SetFront_StartButton();

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m7_3"), 0.3f);
        yield return new WaitForSeconds(3.0f);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //파랑새 나가는 연출.
        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");


        yield return null;

        ManagerTutorial._instance.BlueBirdTurn();

        //파랑새 돌고 난 후 나감.
        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        yield return new WaitForSeconds(0.3f);

        //블라인드 비활성화
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        //스타트 버튼 비활성화
        SetBack_StartButton();

        #endregion

        #region #4 튜토리얼 종료

        Destroy(blind.gameObject);
        blind = null;
        Destroy(ManagerTutorial._instance.gameObject);

        #endregion
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(blind != null)
            Destroy(blind.gameObject);
    }

    private Transform playButton;
    private Transform originParent_PlayButton;
    private Transform Init_PlayButton()
    {
        playButton = UIPopupStageAdventure._instance.scroll_StageList.GetStageItem(1).GetComponentInChildren<UIPokoButton>().transform;
        return playButton;
    }

    private void SetFront_PlayButton()
    {
        originParent_PlayButton = playButton.parent;
        playButton.SetParent(blind.transform);

        playButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
    }

    private void SetBack_PlayButton()
    {
        playButton.SetParent(originParent_PlayButton);
        originParent_PlayButton = null;

        playButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
    }

    private Transform originParent_StartButton;
    private void SetFront_StartButton()
    {
        originParent_StartButton = UIPopupStageAdventureReady._instance.startBtnTr.parent;
        UIPopupStageAdventureReady._instance.startBtnTr.SetParent(blind.transform);

        UIPopupStageAdventureReady._instance.startBtnTr.gameObject.SetActive(false);
        UIPopupStageAdventureReady._instance.startBtnTr.gameObject.SetActive(true);
    }

    private void SetBack_StartButton()
    {
        UIPopupStageAdventureReady._instance.startBtnTr.SetParent(originParent_StartButton);
        originParent_StartButton = null;

        UIPopupStageAdventureReady._instance.startBtnTr.gameObject.SetActive(false);
        UIPopupStageAdventureReady._instance.startBtnTr.gameObject.SetActive(true);
    }

    private IEnumerator BlindAni(Vector2 startSize, Vector2 endSize, float time)
    {
        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimeLobby;

            Vector2 size = Vector2.Lerp(startSize, endSize, totalTime / time);
            blind.SetSize(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));

            if (totalTime > time)
                yield break;

            yield return null;
        }
    }
}
