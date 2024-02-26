using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGetMaterial : TutorialBase
{
    IEnumerator Start()
    {

        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;
        ObjectMaterial._materialList[0].SetClickable(false);

        #region 첫번째 연출 시작 (미션 클리어를 알려줄게)
        {
            //블라인드 설정1.
            blind.transform.position = Vector3.zero;
            blind._panel.depth = 10;
            blind.SetDepth(0);
            blind.SetSizeCollider(0, 0);

            yield return null;
            yield return null;
            //화면 이동.
            CameraController._instance.MoveToPosition(ObjectMaterial._materialList[0].transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            //위치 구하기.
            Camera guiCam = NGUITools.FindCameraForLayer(blind.gameObject.layer);
            Vector3 targetPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(ObjectMaterial._materialList[0].transform.position));
            targetPos.z = 0f;

            //블라인드 설정2.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.up * (GetUIOffSet());
            blind.SetSize(250, 250);

            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            //파랑새 생성.
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m4_1"), 0.3f);
            //손가락 생성.
            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            yield return new WaitForSeconds(0.5f);
            
            //블라인드 터치 영역 설정.
            blind.SetSizeCollider(150, 150);

            bool bPush = false;
            float fTimer = 0.0f;

            int materialCount = ObjectMaterial._materialList.Count;
            ObjectMaterial._materialList[0].SetClickable(true);
            while (ObjectMaterial._materialList.Count == materialCount)
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

            //블라인드 설정.
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);

            yield return null;
        }
        #endregion 첫번째 연출 끝 (미션 클리어를 알려줄게)

        #region 두번째 연출시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
        {

            //파랑새 애니메이션 & 이동.
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            while (true)
            {
                if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
                     ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    break;
                }
                yield return null;
            }
            yield return null;

            // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");
                while (true)
                {
                    if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_idle_turn") &&
                         ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                    {
                        break;
                    }
                    yield return null;
                }
                yield return null;
                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion 두번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

        #region 세번째 연출 시작 (요긴하게 쓰일거야)
        {
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            string text = Global._instance.GetString("t_l_m4_2");
            text = text.Replace("[0]", ManagerData._instance.userData.name);
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, text, 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;
        }
        #endregion 세번째 연출 끝 (요긴하게 쓰일거야)

        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
        {
            blind.transform.parent = ManagerUI._instance.transform;
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));

            //파랑새 나가는 연출.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");
            while (true)
            {
                if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_idle_turn") &&
                     ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    break;
                }
                yield return null;
            }
            yield return null;
            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.4f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
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
