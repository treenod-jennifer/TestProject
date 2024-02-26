using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialReadyScoreUp : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        Vector3 targetPos = UIPopupReady._instance.listReadyItem[0].transform.position;

        BirdPositionType birdType = BirdPositionType.none;

        #region 첫번째 연출 시작 (잠긴 아이템이 열렸어)
        {
            //블라인드 설정.
            blind._panel.depth = UIPopupReady._instance.uiPanel.depth + 3;
            blind.transform.parent = UIPopupReady._instance.transform;
            blind.SetSizeCollider(0, 0);

            blind.SetSize(250, 250);
            blind.transform.position = UIPopupReady._instance.listReadyItem[1].transform.position;
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);


            //콜라이더만 깔아놓고 일정 시간 후에 튜토리얼 시작.
            yield return new WaitForSeconds(1.0f);

            //파랑새 생성.
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

            yield return new WaitForSeconds(0.3f);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m9_1"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.2f));
            yield return null;
        }
        #endregion 첫번째 연출 끝 (잠긴 아이템이 열렸어)

        #region 두번째 연출 시작 (아이템을 준비해봐)
        {
            bool bTurn = false;

            //파랑새 생성.
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
            }
            yield return new WaitForSeconds(0.6f);

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

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m9_2"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            yield return null;
        }
        #endregion 두번째 연출 끝 (아이템을 준비해봐)

        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
        {
            //블라인드 설정.
            blind.SetSizeCollider(0, 0);
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
            yield return new WaitForSeconds(0.5f);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    }
}
