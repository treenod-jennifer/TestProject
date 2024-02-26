using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGame2Match_Adventure : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;
        
        GameObject animal = null;
        GameObject skillItem = null;

        List<GameObject> tempObject = new List<GameObject>();

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }

        #region 첫번째 연출 시작 (답답하니까 내가 설명할게.)
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
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m1_1"), 0.3f);
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
        #endregion 첫번째 연출 끝 (답답하니까 내가 설명할게.)

        #region 두번째 연출 시작 (제거할 블럭 유도)
        {
            BlockBase targetBlock = PosHelper.GetBoard(5, 6).Block;
            blind.transform.position = targetBlock.transform.position;

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

            //블록찾기
            for (int i = 0; i < 3; i++)
            {
                Board tempBoard = PosHelper.GetBoard((4 + i), 6);
                if (tempBoard == null || tempBoard.Block == null)
                    continue;
                BlockBase tempBlock = tempBoard.Block;

                GameObject blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
                blockA.transform.position = tempBlock.transform.position;
                tempObject.Add(blockA);

                if (tempBlock.rightLinker != null)
                {
                    GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject);
                    LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
                    tempObject.Add(LinkerA);
                }
            }
            
            //캐릭터 찾기
            for (int i = 0; i < AdventureManager.instance.AnimalLIst.Count; i++)
            {
                if (AdventureManager.instance.AnimalLIst[i].GetColor() == targetBlock.colorType)
                {
                    animal = AdventureManager.instance.AnimalLIst[i].gameObject;
                    skillItem = AdventureManager.instance.AnimalLIst[i].skillItemObj;
                    break;
                }
            }
            ChangeParent(animal, ManagerTutorial._instance.transform);
            ChangeParent(skillItem, ManagerTutorial._instance.transform);

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = targetBlock.transform.position;
            finger.transform.DOMove(targetPos, 0.3f);

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m1_2"), 0.3f);
            yield return null;

            blind.SetSizeCollider(78 * 3, 78);

            bool bPush = false;
            float fTimer = 0.0f;
            while (ManagerBlock.instance.state != BlockManagrState.MOVE)
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
        }
        #endregion 두번째 연출 끝 (제거할 블럭 유도)

        #region 세번째 연출 시작(파랑새 나감)
        {
            ChangeParent(animal, GameUIManager.instance.Advance_Root.transform);
            ChangeParent(skillItem, GameUIManager.instance.Advance_Root.transform);

            //공격 일시정지.
            AdventureManager.instance.PauseEnemyAttactEvent(true);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            foreach (var temp in tempObject) Destroy(temp);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //블라인드 알파.
            DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.2f);
            blind.SetSizeCollider(0, 0);

            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();
            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f, false));
            yield return new WaitForSeconds(0.3f);
        }
        #endregion 세번째 연출 끝(파랑새 나감)

        #region 네번째 연출 시작(적 턴이 되면 공격)
        {
            while (true)
            {
                if (AdventureManager.instance.AdventureState == ADVENTURE_STATE.ENEMY_ATT)
                    break;

                yield return null;
            }
            yield return null;

            //블라인드 설정.
            blind.transform.position = AdventureManager.instance.EnemyLIst[0].transform.position;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetSize(1200, 1000);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);


            birdType = BirdPositionType.BottomLeft;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m1_3"), 0.3f);

            yield return new WaitForSeconds(0.2f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            AdventureManager.instance.PauseEnemyAttactEvent(false);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
            blind.SetAlpha(0);
        }
        #endregion 네번쨰 연출 끝(적 턴이 되면 공격)

        #region 다섯번째 연출 시작(제거된 블럭의 수만큼 동료의 파워가 올라가)
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
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m1_4"), 0.3f);

            yield return new WaitForSeconds(0.6f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 다섯번째 연출 끝(제거된 블럭의 수만큼 동료의 파워가 올라가)


        #region 여섯번째 연출 시작(콤보쌓이면 적에게 더 강한 공격 가능)
        {
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m1_5"), 0.3f);

            yield return new WaitForSeconds(0.6f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 여섯번째 연출 끝(콤보쌓이면 적에게 더 강한 공격 가능)

        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            yield return new WaitForSeconds(0.3f);

            //GameUIManager.instance.adventureDarkBGBlock.SetActive(false);
            GameUIManager.instance.ShowAdventureDarkBGBlock(false);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }

    private void ChangeParent(GameObject obj, Transform parent)
    {
        if (obj == null)
            return;
        obj.SetActive(false);
        obj.transform.parent = parent;
        obj.SetActive(true);
    }
}
