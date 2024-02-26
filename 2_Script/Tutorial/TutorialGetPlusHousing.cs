﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGetPlusHousing : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        Vector3 targetPos = Vector3.zero;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;
        
        #region 첫번째 연출 시작 (벤치를 바꿔볼까)
        {
            //블라인드 설정.
            blind._panel.depth = 10;
            blind.SetDepth(0);
            blind.SetSizeCollider(0, 0);

            //잠시 뒤 설정.
            yield return new WaitForSeconds(1.0f);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            //DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            ////화면 이동.
            //CameraController._instance.MoveToPosition(ManagerLobby._instance._objectHousing[1].transform.position, 0.5f);
            //yield return new WaitForSeconds(0.5f);

            //Camera guiCam = NGUITools.FindCameraForLayer(blind.gameObject.layer);
            //targetPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(ManagerLobby._instance._objectHousing[1].transform.position));
            //targetPos.z = 0f;
            //blind.transform.position = targetPos;
            //blind.SetSize(500, 500);


            //파랑새 생성.
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m5_1"), 0.3f);

            yield return new WaitForSeconds(0.3f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 첫번째 연출 끝 (벤치를 바꿔볼까)

        #region 두번째 연출 시작 (다이어리 클릭)
        {
            //파랑새 설정.
            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
            }
            yield return new WaitForSeconds(0.6f);

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m5_2"), 0.3f);

            //타겟 위치 설정.
            GameObject diaryButton = ManagerUI._instance.buttonDiary.gameObject;
            targetPos = diaryButton.transform.position + (Vector3.up * 0.03f);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            finger.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            yield return new WaitForSeconds(0.3f);

            GameObject diaryObj = diaryButton.GetComponentInChildren<UISprite>().gameObject;
            
            //타겟 버튼 설정.
            diaryObj.SetActive(false);
            diaryButton.transform.parent = blind.transform;
            diaryObj.SetActive(true);

            bool bPush = false;
            float fTimer = 0.0f;
            while (UIPopupDiary._instance == null || UIDiaryMission._instance == null)
            {
                //기다리는 동안 손가락 이미지 전환.
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

            diaryObj.SetActive(false);
            diaryButton.transform.parent = ManagerUI._instance.anchorBottomLeft.transform;
            diaryObj.SetActive(true);
            ManagerUI._instance.SettingDiaryButton();

            yield return null;
        }
        #endregion 두번째 연출 끝 (로비 미션 아이콘 클릭)

        #region 세번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //타겟이 되는 오브젝트들 뎁스값 조절(잠시동안 topUI보다 위로 올려줌).
            UIPopupDiary._instance.uiPanel.depth = UIPopupDiary._instance.uiPanel.depth + 5;
            UIDiaryMission._instance.uiPanel.depth = UIDiaryMission._instance.uiPanel.depth + 6;

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_L");

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 2.0f);
            }

            //블라인드 설정.
            blind._panel.depth = UIDiaryMission._instance.uiPanel.depth + 1;
            blind.transform.parent = UIPopupDiary._instance.transform;
            DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0f, 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_L");

            // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion 세번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 네번째 연출 시작 (다이어리 창에서 창고 탭 누르기)
        {
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m5_3"), 0.3f);

            //타겟 위치 설정.
            GameObject storageTap = UIPopupDiary._instance.arrayDiaryTap[1].gameObject;
            targetPos = storageTap.transform.position;

            //블라인드 설정.
            blind.transform.position = storageTap.transform.position;
            blind.SetSizeCollider(100, 100);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            finger.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            yield return new WaitForSeconds(0.3f);

            bool bPush = false;
            float fTimer = 0.0f;

            while (UIPopupDiary._instance == null || UIDiaryDeco._instance == null /*UIDiaryStorage._instance == null*/)
            {
                //기다리는 동안 손가락 이미지 전환.
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
            yield return null;
        }
        #endregion 네번째 연출 끝 (다이어리 창에서 창고 탭 누르기)
        
        #region 다섯번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //타겟이 되는 오브젝트들 뎁스값 조절(잠시동안 topUI보다 위로 올려줌).
            //UIDiaryStorage._instance.scrollView.panel.depth = UIPopupDiary._instance.uiPanel.depth + 1;

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            //블라인드 설정.
            blind._panel.depth = UIDiaryDeco._instance.FrontPanelDepth + 1;
            blind.transform.parent = UIPopupDiary._instance.transform;

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion 다섯번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 여섯번째 연출 시작 (만들 수 있는 데코 확인 가능)
        {
            //2번째 탭 오픈.
            //UIDiaryStorage._instance.OpenProgressTap(2);
            UIDiaryDeco._instance.OpenProductionDecoItemBtn(2);

            //파랑새 설정.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m5_4"), 0.3f);

            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            yield return null;
        }
        #endregion 여섯번째 연출 끝 (만들 수 있는 데코 확인 가능)

        #region 일곱번째 연출 시작 (버튼 누르기)
        {
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m5_5"), 0.3f);

            //타겟 위치 설정.
            //UIItemDiaryHousing_P benchItem = UIDiaryStorage._instance.GetBenchItem(2, 2);
            UIItemDeco_Sub_Production benchItem = UIDiaryDeco._instance.GetDecoItme(2, 2);
            GameObject getButton = benchItem.btnGet.gameObject;
            targetPos = getButton.transform.position;

            //타겟 버튼 설정.
            getButton.gameObject.SetActive(false);
            getButton.transform.parent = blind.transform;
            getButton.gameObject.SetActive(true);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            finger.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            yield return new WaitForSeconds(0.3f);

            bool bPush = false;
            float fTimer = 0.0f;

            while (benchItem.bClearButton == false)
            {
                //기다리는 동안 손가락 이미지 전환.
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
            getButton.gameObject.SetActive(false);
            getButton.transform.parent = benchItem.GetComponentInChildren<TweenScale>().transform;
            getButton.gameObject.SetActive(true);
            yield return null;
        }
        #endregion 여섯번째 연출 끝 (버튼 누르기)

        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
        {
            blind.transform.parent = ManagerUI._instance.transform;
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //파랑새 나가는 연출.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle");
            yield return new WaitForSeconds(0.3f);
            
            //오브젝트 제거.
            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    }
}
