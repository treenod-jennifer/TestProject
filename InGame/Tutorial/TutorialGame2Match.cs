using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGame2Match : TutorialBase
{
    float timer = 0;

    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        Vector3 targetPos = Vector3.zero;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
        {
            tutorialRoot.manualWidth = 750;
        }


        #region 첫번째 연출 시작 (게임 방법 설명)
        {
            targetPos = (ManagerBlock.boards[5, 6].Block.transform.position + ManagerBlock.boards[5, 4].Block.transform.position) * 0.5f;

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

            //파랑새 생성.
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_1"), 0.3f);    //"이제부터 플레이 방법을 알려줄게! 간단해!"

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            timer = 0;
            while (timer < 0.2f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        #endregion 첫번째 연출 끝 (게임 방법 설명)


        #region 블럭깜빡깜빡
        {
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.3f, 0.5f);

            List<UISprite> ListspriteDummy = new List<UISprite>();

            //깜빡깜빡
            for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
            {
                for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                {
                    BlockBase tempBlock = PosHelper.GetBlockScreen(j, i);
                    if(tempBlock != null && BlockMatchManager.instance.checkBlockToyBlastMatch(tempBlock))
                    {
                        UISprite blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.mainSprite.gameObject).GetComponent<UISprite>();
                        blockA.transform.position = tempBlock.mainSprite.transform.position;
                        //blockA.transform.localScale = Vector3.one * 1.06f;
                        ListspriteDummy.Add(blockA);

                        if(tempBlock.rightLinker != null)
                        {
                            UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
                            LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
                            //LinkerA.transform.localScale = Vector3.one * 1.06f;
                            ListspriteDummy.Add(LinkerA);

                            //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
                            //LinkerB.transform.position = tempBlock.rightLinker.linkerSpriteB_FR.transform.position;
                            //LinkerB.transform.localScale = Vector3.one * 1.06f;
                            //ListspriteDummy.Add(LinkerB);
                        }

                        if (tempBlock.DownLinker != null)
                        {
                            UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
                            LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
                           // LinkerA.transform.localScale = Vector3.one * 1.06f;
                            ListspriteDummy.Add(LinkerA);

                            //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
                            //LinkerB.transform.position = tempBlock.DownLinker.linkerSpriteB_FR.transform.position;
                            //LinkerB.transform.localScale = Vector3.one * 1.06f;
                            //ListspriteDummy.Add(LinkerB);
                        }
                    }
                }
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_2"), 0.3f);//자, 여기 연결된 블럭이 보이지마유?

            float blockRatio = 10f;

            while (Input.anyKeyDown == false)
            {
                foreach (var tempObj in ListspriteDummy)
                {
                    tempObj.color = new Color(0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 1); 
                }
                yield return null;
            }
            yield return null;
                        
           ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);    //사운드 출력.
            StartCoroutine(textBox.DestroyTextBox(0.3f));       //말풍선 제거.


            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;

                foreach (var tempObj in ListspriteDummy)
                {
                    tempObj.color = new Color(0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 1);
                }
                yield return null;
            }

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_3"), 0.3f);//이렇게 2개 이상 연결된 블록을\n터치하면 블럭을 없앨 수 있어.

            while (Input.anyKeyDown == false)
            {
                foreach (var tempObj in ListspriteDummy)
                {
                    tempObj.color = new Color(0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 1);
                }
                yield return null;
            }
            yield return null;

            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);    //사운드 출력.
            StartCoroutine(textBox.DestroyTextBox(0.3f));       //말풍선 제거.

            
            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;

                foreach (var tempObj in ListspriteDummy)
                {
                    tempObj.color = new Color(0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 0.75f + Mathf.Sin(Time.time * blockRatio) * 0.25f, 1);
                }
                yield return null;
            }
            foreach (var tempObj in ListspriteDummy)
            {
                tempObj.color =Color.white;
            }


            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_4"), 0.3f);//블럭을 한 번 터치해볼까마유?

            //손가락 생성.
            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(ManagerBlock.boards[5, 5].Block.transform.position, 0.3f);
            blind.SetSizeCollider(76*7, 76 * 7);
            blind.SetSize(0, 0);
            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

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

            blind.SetAlpha(0);
            blind.SetSizeCollider(0, 0);
            blind.SetSize(0, 0);
            yield return null;

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);            //사운드 출력.
            StartCoroutine(textBox.DestroyTextBox(0.3f));            //말풍선 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);            //손가락 제거.

            foreach (var tempObj in ListspriteDummy)
            {
                Destroy(tempObj.gameObject);
            }
          
            StartCoroutine(textBox.DestroyTextBox(0.3f));
        }
        #endregion

        #region 두번째 연출 시작 (2개 이상 연결된 블록을 탭)
        {
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

            //ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_5"), 0.3f);    //잘했어! 이렇게 블럭을 없애면 스코어가 올라가마유!


            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            StartCoroutine(textBox.DestroyTextBox(0.3f));

        }  
        #endregion 두번째 연출 끝 (2개 이상 연결된 블록을 탭)


        #region 네번째 연출시작 (스테이지 목표는 이쪽)
        {
            //블럭이 멈출때까지 기다림
            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }

            targetPos = GameUIManager.instance.Target_Root.transform.position;

            //목표화면으로 이동
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            blind.SetDepth(-1);
            blind.transform.position = targetPos;

            //타겟복사하기
            GameObject targetObject = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.Target_Root);

            //파랑새 설정.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

            //사운드 출력.
            //ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_6"), 0.3f);    //스테이지 목표는 이쪽!\n목표를 다 달성하면 스테이지 클리어다마유!

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            Destroy(targetObject);
            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        #endregion 네번째 연출 끝 (스테이지 목표는 이쪽)




        #region 다섯번째 연출 시작 (사과를 잘 봐)
        {
            targetPos = GameUIManager.instance.turnUi.transform.position;
            blind.transform.position = targetPos;

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
            }
            timer = 0;
            while (timer < 0.6f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

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
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

            GameObject turnObject = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.turnUi);
            turnObject.transform.localScale = Vector3.one * 0.885f;

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_7"), 0.3f);    //사과가 떨어지면 블럭을 제거할 수 없으니 잘 확인해마유.
            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;
            Destroy(turnObject);
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        #endregion 다섯번째 연출 끝 (사과를 잘 봐)

        #region 점수보기
        {

            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }
            timer = 0;
            while (timer < 0.6f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            targetPos = GameUIManager.instance.scoreRoot.transform.position;
            blind.transform.position = targetPos;
            ///점수보이기
            ///
            GameObject scoreObject = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.scoreRoot);
            //scoreObject.transform.localScale = Vector3.one * 1.06f;

            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_8"), 0.3f); //높은 스코어를 받으면 꽃이 피어.\n꽃을 활짝 피워보자마유!
            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            Destroy(scoreObject);
            //blind.SetAlpha(0);
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

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
        }
        # endregion


        #region 여섯번째 연출 시작 (이제 게임을 해보자)
        {
            //파랑새 설정.
            ManagerTutorial._instance.SetBirdAnimation("T_targetUp_idle");

            //사운드 출력.
           // ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m1_9"), 0.3f);        //그럼 이제 게임을 시작해볼까?\n화이팅이다마유!

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;
        }
        #endregion  여섯번째 연출 끝 (이제 게임을 해보자)

        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
        {
            blind.transform.parent = ManagerUI._instance.transform;
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            blind.SetAlpha(0);
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
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

            timer = 0;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //블라인드 알파.
            //DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
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
        #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    }
}
