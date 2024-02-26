using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialPotion_Adventure : TutorialBase
{
    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }

        #region 첫번째 연출 시작 (생명력을 회복시켜주는 특별한 물약)
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
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");
            yield return new WaitForSeconds(0.3f);

            //회복 포션
            Board tempBoard = PosHelper.GetBoard(3, 7);
            BlockBase tempBlock = tempBoard.Block;
            GameObject blockHealPotion = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
            blockHealPotion.transform.position = tempBlock.transform.position;

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m4_1"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            Destroy(blockHealPotion);
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 첫번째 연출 끝 (생명력을 회복시켜주는 특별한 물약)

        #region 두번째 연출 시작 (스킬 게이지를 충전시켜주는 물약)
        {
            bool bTurn = false;
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
                yield return new WaitForSeconds(0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //스킬포션.
            Board tempBoard = PosHelper.GetBoard(7, 7);
            BlockBase tempBlock = tempBoard.Block;
            GameObject blockSkillPotion = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
            blockSkillPotion.transform.position = tempBlock.transform.position;

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m4_2"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            Destroy(blockSkillPotion);
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 두번째 연출 끝 (스킬 게이지를 충전시켜주는 물약)

        #region 세번째 연출 시작 (물약 주변의 블럭을 제거)
        {
            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
                yield return new WaitForSeconds(0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //블럭들 생성
            List<GameObject> tempBlockList = new List<GameObject>();

            //힐 물약 주변 오브젝트들 생성.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Board tempBoard = PosHelper.GetBoard((2 + i), (6 + j));
                    if (tempBoard == null || tempBoard.Block == null)
                        break;
                    BlockBase tempBlock = tempBoard.Block;

                    GameObject block = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
                    block.transform.position = tempBlock.transform.position;
                    tempBlockList.Add(block);
                }
            }

            //스킬 물약 주변 오브젝트들 생성.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Board tempBoard = PosHelper.GetBoard((6 + i), (6 + j));
                    BlockBase tempBlock = tempBoard.Block;
                    GameObject block = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject);
                    block.transform.position = tempBlock.transform.position;
                    tempBlockList.Add(block);
                }
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m4_3"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            foreach (var temp in tempBlockList) Destroy(temp);
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 세번째 연출 끝 (물약 주변의 블럭을 제거)

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
