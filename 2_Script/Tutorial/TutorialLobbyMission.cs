using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialLobbyMission : TutorialBase
{
    IEnumerator Start ()
    {

        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        //위치 구하기.
        Camera guiCam = NGUITools.FindCameraForLayer(blind.gameObject.layer);
        Vector3 targetPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(ObjectMissionIcon._iconList[0].transform.position));
        targetPos.z = 0f;

        BirdPositionType birdType = BirdPositionType.none;

        #region 첫번째 연출 시작 (미션 클리어를 알려줄게)
        {
            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.up * (GetUIOffSet() + 20f);
            blind._panel.depth = 10;
            blind.SetDepth(0);
            blind.SetSizeCollider(0, 0);

            //콜라이더만 깔아놓고 일정 시간 후에 튜토리얼 시작.
            yield return new WaitForSeconds(1.0f);

            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            //파랑새 생성.
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m1_1"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            yield return null;
        }
        #endregion 첫번째 연출 끝 (미션 클리어를 알려줄게)

        #region 두번째 연출 시작 (로비 미션 아이콘 클릭)
        {
            //블라인드 설정.
            blind.SetSize(250, 250);

            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            bool bTurn = false;
            //파랑새 생성.
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
            }
            yield return new WaitForSeconds(0.6f);

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m1_2"), 0.3f);

            //손가락 생성.
            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            yield return new WaitForSeconds(0.3f);
            blind.SetSizeCollider(180, 180);

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
            yield return null;
        }
        #endregion 두번째 연출 끝 (로비 미션 아이콘 클릭)

        #region 세번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //타겟이 되는 오브젝트들 뎁스값 조절(잠시동안 topUI보다 위로 올려줌).
            UIPopupDiary._instance.uiPanel.depth = UIPopupDiary._instance.uiPanel.depth + 5;
            UIDiaryMission._instance.uiPanel.depth = UIDiaryMission._instance.uiPanel.depth + 6;

            UIPopupDiary._instance.btnOpenAtLobbyInRoot.SetActive(false);

            //파랑새 애니메이션 & 이동.
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            { 
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            //블라인드 설정.
            blind._panel.depth = UIDiaryMission._instance.uiPanel.depth + 1;
            blind.transform.parent = UIPopupDiary._instance.transform;
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);
            DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0f, 0.6f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));

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
        #endregion 세번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 네번째 연출 시작 (다이어리 창에서 미션 클리어 누르기)
        {
            //파랑새 애니메이션.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m1_3"), 0.3f);

            //타겟 위치 설정.
            GameObject missionButton = UIDiaryMission._instance.GetMissionButton();
            targetPos = UIDiaryMission._instance.GetMissionButtonPosition();

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            yield return new WaitForSeconds(0.3f);

            //타겟 버튼 설정.
            missionButton.SetActive(false);
            missionButton.transform.parent = blind.transform;
            missionButton.SetActive(true);

            bool bPush = false;
            float fTimer = 0.0f;
            
            while (UIPopUpMethod._instance == null && UIPopupDiary._instance != null)
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
            missionButton.SetActive(false);
            missionButton.transform.parent = UIDiaryMission._instance.GetMissionItem();
            missionButton.SetActive(true);
            yield return null;
        }
        #endregion 네번째 연출 끝 (다이어리 창에서 미션 클리어 누르기)

        #region 다섯번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //파랑새 애니메이션 & 이동.
            ManagerTutorial._instance.SetBirdAnimation("T_clear_L");

            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 2.0f);
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //블라인드 설정.
            blind._panel.depth = UIPopUpMethod._instance.uiPanel.depth + 1;
            blind.transform.parent = UIPopUpMethod._instance.transform;

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_L");

            // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion 다섯번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 여섯번째 연출 시작 (미션 완료에는 별이 필요해)
        {
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //타겟 위치 설정.
            GameObject button = UIPopUpMethod._instance.GetButton();
            targetPos = button.transform.position;

            //블라인드 알파값 설정.
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.5f, 0.6f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m1_4"), 0.3f);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            yield return new WaitForSeconds(0.3f);
            
            //타겟 버튼 설정.
            button.SetActive(false);
            button.transform.parent = blind.transform;
            button.SetActive(true);

            bool bPush = false;
            float fTimer = 0.0f;

            //UIPopUpMethod Ready팝업이 뜨면서 바로 닫히기 때문에 눌러졌는지 확인하는 부분에서 버튼 위치 변경해야함.
            while (UIPopUpMethod._instance.bClickBtnOK == false)
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

            //start 버튼 원래위치로 돌려줌.
            button.SetActive(false);
            button.transform.parent = UIPopUpMethod._instance.mainSprite.transform;
            button.SetActive(true);

            blind.transform.parent = ManagerUI._instance.transform;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            while (UIPopupReady._instance == null)
            {
                yield return null;
            }
            yield return null;
        }
        #endregion 여섯번째 연출 끝 (미션 완료에는 별이 필요해)

        #region 일곱번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {
            //파랑새 애니메이션 & 이동.
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            bool bTurn = false;
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                bTurn = (ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f));
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //블라인드 설정.
            blind._panel.depth = UIPopupReady._instance.clippingPanel.depth + 2;
            blind.transform.parent = UIPopupReady._instance.clippingPanel.transform;

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion 일곱번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 여덟번째 연출 시작 (스테이지 시작 버튼)
        {
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //타겟 위치 설정.
            GameObject button = UIPopupReady._instance.GetButton();
            targetPos = button.transform.position;

            //블라인드 알파값 설정.
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.5f, 0.6f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m1_5"), 0.3f);
            
            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            yield return new WaitForSeconds(0.3f);

            //타겟 버튼 설정.
            button.SetActive(false);
            button.transform.parent = blind.transform;
            button.SetActive(true);

            bool bPush = false;
            float fTimer = 0.0f;

            while (UIPopupReady._instance.bStartGame == false)
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
            button.SetActive(false);
            button.transform.parent = UIPopupReady._instance.clippingPanel.transform;
            button.SetActive(true);
            yield return null;
        }
        #endregion 여덟번째 연출 끝 (스테이지 시작 버튼)

        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
        {
            blind.transform.parent = ManagerUI._instance.transform;
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //파랑새 나가는 연출.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.4f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //블라인드 알파.
            DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
            //오브젝트 제거.
            yield return new WaitForSeconds(0.4f);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    }

    private float GetUIOffSet()
    {
        float ratio = Camera.main.fieldOfView;
        return ratio;
    }
}
