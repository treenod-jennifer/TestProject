using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGetSticker : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        Vector3 targetPos = Vector3.zero;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region 첫번째 연출 시작 (스티커 버튼을 눌러봐~)
        {
            //블라인드 설정.
            blind._panel.depth = UIDiaryMission._instance.uiPanel.depth + 10;
            blind.transform.parent = UIPopupDiary._instance.transform;
            blind.SetDepth(0);
            blind.SetSizeCollider(0, 0);

            //잠시 뒤 설정.
            yield return new WaitForSeconds(0.2f);

            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.7f, 0.5f);

            //파랑새 생성.
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            yield return new WaitForSeconds(0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m8_1"), 0.3f);

            //타겟 위치 설정.
            GameObject stickerTap = UIPopupDiary._instance.arrayDiaryTap[2].gameObject;
            targetPos = stickerTap.transform.position;

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            finger.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.SetSize(250, 250);
            blind.SetSizeCollider(100, 100);

            yield return new WaitForSeconds(0.3f);

            bool bPush = false;
            float fTimer = 0.0f;

            while (UIPopupDiary._instance == null || UIDiaryStamp._instance == null)
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

            //블라인드 설정.
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);
        }
        #endregion 첫번째 연출 끝 (스티커 버튼을 눌러봐~)

        #region 두번째 연출 시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            //블라인드 설정.
            DOTween.To((a) => blind.SetAlpha(a), 0.7f, 0f, 0.3f);

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
        #endregion 두번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 세번째 연출시작 (스티커의 메시지를 바꿔봐)
        {
            GameObject editObj = null;
            while (true)
            {
                editObj = UIDiaryStamp._instance.GetBtnEditButton();
                if (editObj != null)
                {
                    //타겟 위치 설정.
                    targetPos = editObj.transform.position;
                    break;
                }
                yield return null;
            }
            yield return null;

            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m8_2"), 0.3f);

            //블라인드 설정.
            blind.transform.position = targetPos + (Vector3.up * 0.02f);
            blind.SetSizeCollider(80, 80);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            yield return new WaitForSeconds(0.3f);

            bool bPush = false;
            float fTimer = 0.0f;

            while (UIPopupSendItemToSocial._instance == null)
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

            //블라인드 설정.
            blind._panel.depth = UIPopupSendItemToSocial._instance.uiPanel.depth + 10;
            blind.transform.parent = UIPopupSendItemToSocial._instance.transform;
            blind.SetSizeCollider(0, 0);
        }
        #endregion 세번째 연출 끝 (스티커의 메시지를 바꿔봐)

        #region 네번째 연출 시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

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
        #endregion 네번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 다섯번째 연출시작 (아래의 버튼으로 메세지를 바꿀 수 있어)
        {
            //파랑새 설정.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m8_3"), 0.3f);

            yield return new WaitForSeconds(0.5f);

            bool bTouch = false;
            while (bTouch == false)
            {
                if (Input.anyKeyDown == true)
                {
                    bTouch = true;
                }
                yield return null;
            }
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            yield return null;
        }
        #endregion 다섯번째 연출 끝 (아래의 버튼으로 메세지를 바꿀 수 있어)

        #region 여섯번째 연출 시작 (다양하게 작성해보자)
        {
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m8_4"), 0.3f);

            yield return new WaitForSeconds(0.5f);

            bool bTouch = false;
            while (bTouch == false)
            {
                if (Input.anyKeyDown == true)
                {
                    bTouch = true;
                }
                yield return null;
            }
            yield return null;
        }
        #endregion 여섯번째 연출 끝 (다양하게 작성해보자)

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
