using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialDecoIce : TutorialBase
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

        #region 연출 시작
        {            
            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //블록찾기
            foreach (var tempBlock in ManagerBlock.boards)
            {
                if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.blockDeco != null
                    && tempBlock.indexX > 3 && tempBlock.indexX < 10
                    && tempBlock.indexY > 3 && tempBlock.indexY < 10) //위치조절
                {
                    if (tempBlock.Block.blockDeco is DecoIce)
                    {
                        targetPos = tempBlock.Block.transform.position;

                        BlockBase blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.Block.gameObject).GetComponent<BlockBase>();
                        blockA._transform.position = tempBlock.Block.transform.position;
                        tempObject.Add(blockA.gameObject);
                        break;
                    }
                }
            }

            //파랑새 등장 설명해준다고 얘기시작
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m15_1"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.


            yield return null;
        }
        #endregion

        #region 두번째 연출시작
        {
            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m15_2"), 0.3f);        //그 다음엔 블록을 터치해봐마유.


            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //blind.SetAlpha(0);
            blind.SetSizeCollider(0, 0);
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            yield return null;
        }
        #endregion 두번째 연출 끝


        #region 두번째 연출시작
        {
            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m15_3"), 0.3f);        //그 다음엔 블록을 터치해봐마유.

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //blind.SetAlpha(0);
            blind.SetSizeCollider(0, 0);
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            foreach (var temp in tempObject) Destroy(temp);

            yield return null;
        }
        #endregion 두번째 연출 끝


        #region 마지막 연출 시작
        {
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

            //블라인드 알파.
            DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
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
