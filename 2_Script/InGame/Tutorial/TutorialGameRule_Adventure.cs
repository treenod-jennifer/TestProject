using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGameRule_Adventure : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }

        #region 첫번째 연출 시작 (보스가 나타났다)
        {
            //블라인드 설정.
            blind.transform.position = GetCenterPosition(true);
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetSize(1200, 1000);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            yield return new WaitForSeconds(0.3f);

            //파랑새 생성.
            birdType = BirdPositionType.BottomLeft;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m3_1"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 첫번째 연출 끝 (보스가 나타났다)

        #region 두번째 연출 시작 (보스를 쓰러뜨리면 우리의 승리)
        {
            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = Vector3.zero;
            for (int i = 0; i < AdventureManager.instance.EnemyLIst.Count; i++)
            {
                if (AdventureManager.instance.EnemyLIst[i].data.idx == ManagerBlock.instance.stageInfo.bossInfo.idx)
                {
                    targetPos = AdventureManager.instance.EnemyLIst[i].transform.position;
                    break;
                }
            }

            finger.transform.DOMove(targetPos, 0.3f);
            finger.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            yield return new WaitForSeconds(0.3f);

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m3_2"), 0.3f);

            yield return new WaitForSeconds(0.2f);
            while (Input.anyKeyDown == false)
            {
                finger.transform.localPosition += new Vector3(0f, Mathf.Sin(Time.time * 10), 0);
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
        }
        #endregion 두번째 연출 끝 (보스를 쓰러뜨리면 우리의 승리)

        #region 세번째 연출 시작(패배 주의해)
        {
            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
                yield return new WaitForSeconds(0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m3_3"), 0.3f);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            blind.transform.position = GetCenterPosition(false);

            yield return new WaitForSeconds(0.3f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
        }
        #endregion 세번째 연출 끝(패배 주의해)

        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();
            
            blind.SetAlpha(0);
            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            yield return new WaitForSeconds(0.3f);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }

    private Vector3 GetCenterPosition(bool bEnemy)
    {
        if (bEnemy)
        {
            //적 중앙 위치.
            List<Vector3> listEnemyPos = new List<Vector3>();
            for (int i = 0; i < AdventureManager.instance.EnemyLIst.Count; i++)
            {
                listEnemyPos.Add(AdventureManager.instance.EnemyLIst[i].transform.position);
            }
            return GetCenterPos(listEnemyPos);
        }
        else
        {
            //내 캐릭터 위치.
            List<Vector3> listAnimalPos = new List<Vector3>();
            for (int i = 0; i < AdventureManager.instance.AnimalLIst.Count; i++)
            {
                listAnimalPos.Add(AdventureManager.instance.AnimalLIst[i].transform.position);
            }
            return GetCenterPos(listAnimalPos);
        }
    }

    private Vector3 GetCenterPos(List<Vector3> listPos)
    {
        listPos.Sort((a, b) => a.x > b.x ? 1 : -1);

        if (listPos.Count > 1)
            return listPos[1];
        else
            return listPos[0];
    }
}
