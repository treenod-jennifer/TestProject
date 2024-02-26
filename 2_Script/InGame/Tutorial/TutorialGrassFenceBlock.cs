using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGrassFenceBlock : TutorialBase
{


    #region 예전 튜토리얼 코드
    //IEnumerator Start()
    //{
    //    float timer = 0;
    //    TextboxTutorial textBox = null;
    //    UITexture finger = ManagerTutorial._instance._spriteFinger;
    //    Vector3 targetPos = Vector3.zero;
    //    BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
    //    BirdPositionType birdType = BirdPositionType.none;

    //    UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
    //    if (tutorialRoot != null)
    //        tutorialRoot.manualWidth = 750;

    //    List<GameObject> tempObject = new List<GameObject>();

    //    #region 연출 시작
    //    {
    //        //블라인드 설정.
    //        blind.transform.position = targetPos;
    //        blind.transform.localPosition += Vector3.zero;
    //        blind._panel.depth = 10;
    //        blind.SetSizeCollider(0, 0);
    //        blind.SetDepth(0);
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //블록찾기
    //        foreach (var tempBoard in ManagerBlock.boards)
    //        {
    //            if (tempBoard == null || tempBoard.Block == null)
    //                continue;

    //            BlockBase tempBlock = tempBoard.Block;
    //            if (BlockMatchManager.instance.checkBlockToyBlastMatch(tempBlock))
    //            {
    //                bool bHasGrassBlock = false;
    //                for (int i = 0; i < tempBoard.DecoOnBoard.Count; i++)
    //                {
    //                    if (tempBoard.DecoOnBoard[i] is GrassFenceBlock)
    //                    {
    //                        GameObject fenceBlock = NGUITools.AddChild(blind._panel.gameObject, tempBoard.DecoOnBoard[i].gameObject);
    //                        fenceBlock.transform.position = tempBoard.DecoOnBoard[i].transform.position;
    //                        tempObject.Add(fenceBlock.gameObject);
    //                        bHasGrassBlock = true;
    //                    }  
    //                }

    //                //식물 방해블럭 없으면 나감.
    //                if (bHasGrassBlock == false)
    //                    continue;

    //                GameObject blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
    //                blockA.transform.position = tempBlock.transform.position;
    //                tempObject.Add(blockA);

    //                if (tempBlock.rightLinker != null)
    //                {
    //                    GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject);
    //                    LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
    //                    tempObject.Add(LinkerA);
    //                }

    //                if (tempBlock.DownLinker != null)
    //                {
    //                    GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject);
    //                    LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
    //                    tempObject.Add(LinkerA);
    //                }
    //            }
    //        }

    //        //파랑새 등장 설명해준다고 얘기시작
    //        birdType = BirdPositionType.BottomLeft;
    //        ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m17_1"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //    }
    //    #endregion

    //    #region 두번째 연출시작
    //    {
    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //        yield return new WaitForSeconds(0.3f);

    //        //파랑새 등장 설명해준다고 얘기시작
    //        birdType = BirdPositionType.BottomRight;
    //        bool bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);

    //        if (bTurn == true)
    //        {
    //            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");
    //            while (true)
    //            {
    //                if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_idle_turn") &&
    //                     ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //                {
    //                    break;
    //                }
    //                yield return null;
    //            }
    //            yield return null;
    //            ManagerTutorial._instance.BlueBirdTurn();
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m17_2"), 0.3f);        //그 다음엔 블록을 터치해봐마유.
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //    }
    //    #endregion 두번째 연출 끝

    //    #region 마지막 연출 시작
    //    {
    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //        foreach (var temp in tempObject) Destroy(temp);

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");
    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_idle_turn") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }
    //        yield return null;
    //        ManagerTutorial._instance.BlueBirdTurn();

    //        //파랑새 돌고 난 후 나감.
    //        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //블라인드 알파.
    //        DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
    //        //오브젝트 제거.
    //        timer = 0;
    //        while (timer < 0.4f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        Destroy(blind.gameObject);
    //        Destroy(ManagerTutorial._instance.gameObject);
    //    }
    //    #endregion 마지막 연출 끝
    //}
    #endregion
}
