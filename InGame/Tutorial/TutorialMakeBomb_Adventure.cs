using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialMakeBomb_Adventure : TutorialBase
{   
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        List<GameObject> tempObject = new List<GameObject>();

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }

        #region 첫번째 연출 시작 (주황색 블럭을 제거)
        {
            //블라인드 설정.
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.3f);

            //파랑새 생성.
            birdType = BirdPositionType.TopRight;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //주황색 블록찾기
            foreach (var tempBoard in ManagerBlock.boards)
            {
                if (tempBoard == null || tempBoard.Block == null)
                    continue;

                BlockBase tempBlock = tempBoard.Block;
                if (tempBlock.type == BlockType.NORMAL && tempBlock.colorType == BlockColorType.D)
                {
                    GameObject blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
                    blockA.transform.position = tempBlock.transform.position;
                    tempObject.Add(blockA);

                    if (tempBlock.rightLinker != null)
                    {
                        GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject);
                        LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
                        tempObject.Add(LinkerA);
                    }

                    if (tempBlock.DownLinker != null)
                    {
                        GameObject LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject);
                        LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
                        tempObject.Add(LinkerA);
                    }
                }
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m5_1"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            foreach (var temp in tempObject) Destroy(temp);
            yield return new WaitForSeconds(0.5f);

        }
        #endregion 첫번째 연출 끝 (주황색 블럭을 제거)

        #region 두번째 연출 시작 (보너스 폭탄이 만들어져)
        {
            //AdventureManager.instance.GetAdventureGaige(100);
            tempObject.Clear();

            int depth = 5;
            foreach (Transform child in GameUIManager.instance.advantureGaigeSlider.transform)
            {
                GameObject obj = NGUITools.AddChild(blind._panel.gameObject, child.gameObject);
                obj.transform.position = child.position;
                if (obj.GetComponent<UISprite>() != null)
                    obj.GetComponent<UISprite>().depth = depth;
                tempObject.Add(obj);
                depth += 1;
            }

            GameObject bomb = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.advantureGaigeBomb.gameObject);
            bomb.GetComponent<UISprite>().color = new Color(1f, 1f, 1f);
            bomb.GetComponent<UISprite>().depth = 15;
            bomb.transform.position = GameUIManager.instance.advantureGaigeBomb.gameObject.transform.position;
            tempObject.Add(bomb);

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m5_2"), 0.3f);

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
        #endregion 두번째 연출 끝 (보너스 폭탄이 만들어져)
        
        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            //블라인드 알파.
            DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.2f);
            yield return new WaitForSeconds(0.3f);

            foreach (var temp in tempObject) Destroy(temp);
            //AdventureManager.instance.MakeAdventureBomb();
            //날아가는동안 기다리기
            float waitTimer = 0f;
            while (waitTimer < 1f)
            {
                waitTimer += Global.deltaTimePuzzle * 1.5f;
                yield return null;
            }

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }
}
