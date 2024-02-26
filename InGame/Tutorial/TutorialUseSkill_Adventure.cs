using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialUseSkill_Adventure : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        List<GameObject> tempObject = new List<GameObject>();

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }

        #region 첫번째 연출 시작 (스킬쓰는 법 알려줄게)
        {
            //블라인드 설정.
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
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
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m2_1"), 0.3f);    //"이제부터 플레이 방법을 알려줄게! 간단해!"
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
        #endregion 첫번째 연출 끝 (스킬쓰는 법 알려줄게)

        #region 두번째 연출 시작 (블럭 제거하면 채워져)
        {
            DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.2f);
            GameUIManager.instance.adventureEffectBG.gameObject.SetActive(true);
            GameUIManager.instance.ShowAdventureDarkBGBlock(true);           

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
                yield return new WaitForSeconds(0.6f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m2_2"), 0.3f);

            yield return new WaitForSeconds(0.2f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
        }
        #endregion 두번째 연출 끝 (블럭 제거하면 채워져)

        #region 세번째 연출 시작(스킬 사용해)
        {   
            //스킬 사용가능 상태로 만듦.
            AdventureManager.instance.ChargeAllSkillPoint();

            //손가락 생성.
            Vector3 targetPos = Vector3.zero;
            for (int i = 0; i < AdventureManager.instance.AnimalLIst.Count; i++)
            {
                if (AdventureManager.instance.AnimalLIst[i].skillItemObj != null)
                {
                    targetPos = AdventureManager.instance.AnimalLIst[i].skillItemObj.transform.position;
                    break;
                }
            }
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);

            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m2_3"), 0.3f);

            int index = 0;
            if (AdventureManager.instance.skillItemList.Count > 1)
                index = AdventureManager.instance.skillItemList.Count / 2;

            blind.transform.position = AdventureManager.instance.skillItemList[index].transform.position;
            blind.SetSizeCollider(108 * 3, 108);

            yield return new WaitForSeconds(0.2f);

            bool bUseSkill = false;
            bool bPush = false;
            float fTimer = 0.0f;

            while (bUseSkill == false)
            {
                for (int i = 0; i < AdventureManager.instance.AnimalLIst.Count; i++)
                {
                    if (AdventureManager.instance.AnimalLIst[i].GetState() == ANIMAL_STATE.SKILL)
                        bUseSkill = true;
                }

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

            blind.SetSizeCollider(0, 0);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            GameUIManager.instance.adventureEffectBG.gameObject.SetActive(false);
            //GameUIManager.instance.adventureDarkBGBlock.SetActive(false);
            GameUIManager.instance.ShowAdventureDarkBGBlock(false);
        }
        #endregion 세번째 연출 끝(스킬 사용해)

        #region 네번째 연출 시작(파랑새 굿)
        {
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");
        }
        #endregion 네번째 연출 끝(파랑새 굿)

        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            yield return new WaitForSeconds(0.3f);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }
}