using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialCarpet : TutorialBase
{
    IEnumerator Start()
    {
        float timer = 0;
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        Vector3 targetPos = Vector3.zero;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
            tutorialRoot.manualWidth = 750;

        List<GameObject> tempObject = new List<GameObject>();

        #region 연출 시작 (블럭 밑에 양털이 깔려있네)
        {
            targetPos = (ManagerBlock.boards[3, 5].Block.transform.position + ManagerBlock.boards[4, 5].Block.transform.position) * 0.5f;

            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //파랑새 등장 설명해준다고 얘기시작
            birdType = BirdPositionType.BottomLeft;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m9_1"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.3f);
        }
        #endregion

        #region 두번째 연출시작 (양털 위에 있는 블럭을 터치하면, 그 블럭과 연결된 다른 블럭의 밑에도 양털이 깔려)
        {   
            //블라인드
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            //블록찾기   
            List<BlockBase> listMatchBlock = new List<BlockBase>();
            for (int i = 0; i < 2; i++)
            {
                int x = 3 + i;
                if (ManagerBlock.boards[x, 5].Block != null)
                    listMatchBlock.Add(ManagerBlock.boards[x, 5].Block);
            }
            for (int i = 0; i < 4; i++)
            {
                int x = 6 + i;
                if (ManagerBlock.boards[x, 5].Block != null)
                    listMatchBlock.Add(ManagerBlock.boards[x, 7].Block);
            }

            //오브젝트 추가.
            for (int i = 0; i < listMatchBlock.Count; i++)
            {
                Board tempBoard = ManagerBlock.boards[listMatchBlock[i].indexX, listMatchBlock[i].indexY];
                if (tempBoard == null)
                    continue;

                //카펫 오브젝트 찾기.
                for (int j = 0; j < tempBoard.DecoOnBoard.Count; j++)
                {
                    if (tempBoard.DecoOnBoard[j] is Carpet)
                    {
                        GameObject carpet = NGUITools.AddChild(blind._panel.gameObject, tempBoard.DecoOnBoard[j].gameObject);
                        carpet.transform.position = tempBoard.DecoOnBoard[j].transform.position;
                        tempObject.Add(carpet.gameObject);
                    }
                }

                //현재 검사하는 블럭 오브젝트 추가.
                GameObject blockA = NGUITools.AddChild(blind._panel.gameObject, listMatchBlock[i].gameObject);
                blockA.transform.position = listMatchBlock[i].transform.position;
                tempObject.Add(blockA);

                if (listMatchBlock[i].rightLinker != null)
                {
                    GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, listMatchBlock[i].rightLinker.linkerSpriteFR.gameObject);
                    LinkerA.transform.position = listMatchBlock[i].rightLinker.linkerSpriteFR.transform.position;
                    tempObject.Add(LinkerA);
                }

                if (listMatchBlock[i].DownLinker != null)
                {
                    GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, listMatchBlock[i].DownLinker.linkerSpriteFR.gameObject);
                    LinkerA.transform.position = listMatchBlock[i].DownLinker.linkerSpriteFR.transform.position;
                    tempObject.Add(LinkerA);
                }
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m9_2"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
        }
        #endregion 두번째 연출 끝

        #region 세번째 연출시작 (직접 양털 위의 블럭을 터치해보자)
        {
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.3f);

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

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //블라인드 알파.
            DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0f, 0.3f);
            blind.SetSizeCollider(76 * 2, 76 * 1);
            blind.SetSize(0, 0);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m9_3"), 0.3f);
            yield return new WaitForSeconds(0.2f);
            
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //손가락 생성.
            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            yield return new WaitForSeconds(0.3f);


            bool bPush = false;
            float fTimer = 0.0f;
            while (ManagerBlock.instance.state != BlockManagrState.MOVE)
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
        }
        #endregion 세번째 연출 끝

        #region 네번쨰 연출 시작 (양털 위의 폭탄을 터트리면 폭탄 범위만큼 양털이 깔리니까 잘 활용해봐)
        {
            //블라인드
            blind.SetSizeCollider(0, 0);

            //더미 블록 이미지 제거.
            foreach (var temp in tempObject) Destroy(temp);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_L");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_L");

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m9_4"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
        }
        #endregion 네번쨰 연출 끝

        #region 마지막 연출 시작
        {
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);

            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            //오브젝트 제거.
            timer = 0;
            while (timer < 0.4f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }
}
