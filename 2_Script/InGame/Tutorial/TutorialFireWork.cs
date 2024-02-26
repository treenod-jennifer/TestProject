using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialFireWork : TutorialBase
{
    private IEnumerator Start()
    {
        #region Step Start

        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BirdPositionType birdType = BirdPositionType.none;
        BlockBase target = null;

        #endregion

        #region Step1

        //블라인드 생성
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        blind._panel.depth = 30;
        blind.SetDepth(0);
        blind.SetSize(0, 0);
        blind.SetSizeCollider(0, 0);
        blind.transform.localPosition = Vector3.zero;
        blind.transform.parent = ManagerUI._instance.transform;

        //마유지 생성
        if (birdType != BirdPositionType.BottomLeft)
        {
            birdType = BirdPositionType.BottomLeft;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
        }
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_1"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

        //입력을 받을때 까지 대기
        yield return new WaitUntil(() => Input.anyKeyDown);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        #endregion;

        #region Step2

        //블라인드 생성
        blind.SetSize(400, 550);
        blind.SetSizeCollider(0, 0);
        UITexture blindCenter = blind._textureCenter.GetComponent<UITexture>();
        blindCenter.type = UIBasicSprite.Type.Sliced;
        blindCenter.border = Vector4.one * 50;
        blind.transform.localPosition = Vector3.up * 48;
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
        yield return new WaitForSeconds(0.2f);

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bool bTurn = false;
        if (birdType != BirdPositionType.BottomRight)
        {
            birdType = BirdPositionType.BottomRight;
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
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_2"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //입력을 받을때 까지 대기
        yield return new WaitUntil(() => Input.anyKeyDown);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //블라인드 제거
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region Step3

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        finger.color = new Color(finger.color.r, finger.color.g, finger.color.b, 0.0f);
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        target = PosHelper.GetBlockScreen(4, 4);
        finger.transform.DOMove(target.transform.position, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_3"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //pangBlock들만 입력이 가능하도록 세팅
        blind.transform.position = target.transform.position;
        blind.SetSizeCollider(78, 78);

        //블럭이 터질때 까지 대기(기다리는 동안 손가락 이미지 전환)
        bool bPush = false;
        float fTimer = 0.0f;

        while(target != null)
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

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
        blind.SetSizeCollider(0, 0);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region Step4

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bTurn = false;
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
        yield return new WaitForSeconds(1.0f);

        //블라인드 생성
        target = PosHelper.GetBlockScreen(5, 7);
        blind.transform.position = target.transform.position;
        blind.SetSize(400, 170);
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_4"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        finger.color = new Color(finger.color.r, finger.color.g, finger.color.b, 0.0f);
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(target.transform.position, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //입력을 받을때 까지 대기
        yield return new WaitUntil(() => Input.anyKeyDown);

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //블라인드 제거
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region Step5

        target = PosHelper.GetBlockScreen(6, 7);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_5"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        finger.color = new Color(finger.color.r, finger.color.g, finger.color.b, 0.0f);
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(target.transform.position, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //target 입력이 가능하도록 세팅
        blind.transform.position = target.transform.position;
        blind.SetSizeCollider(78, 78);

        //블럭이 터질때 까지 대기(기다리는 동안 손가락 이미지 전환)
        bPush = false;
        fTimer = 0.0f;

        while (target != null)
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

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
        blind.SetSizeCollider(0, 0);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region Step6

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m19_6"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //입력을 받을때 까지 대기
        yield return new WaitUntil(() => Input.anyKeyDown);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region Step End

        //파랑새 나가는 연출.
        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

        ManagerTutorial._instance.BlueBirdTurn();

        //파랑새 돌고 난 후 나감.
        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        yield return new WaitForSeconds(0.3f);

        //튜토리얼 종료
        Destroy(blind.gameObject);
        Destroy(ManagerTutorial._instance.gameObject);

        #endregion
    }
}
