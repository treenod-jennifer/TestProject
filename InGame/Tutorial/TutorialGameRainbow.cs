using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGameRainbow : TutorialBase
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
    //    {
    //        tutorialRoot.manualWidth = 750;
    //    }


    //    #region 첫번째 연출 시작 (화살 폭탄 만드는 법)
    //    {
    //        targetPos = (ManagerBlock.boards[5, 6].Block.transform.position + ManagerBlock.boards[6, 6].Block.transform.position) *0.5f;

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

    //        //파랑새 등장 설명해준다고 얘기시작
    //        if (birdType != BirdPositionType.BottomLeft)
    //        {
    //            birdType = BirdPositionType.BottomLeft;
    //            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m4_1"), 0.3f);        //이번에는 레인보우 폭탄에 대해 알려줄게마유.

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        yield return null;

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //    }
    //    #endregion 첫번째 연출 끝 (화살 폭탄 만드는 법)

    //    #region 두번째 연출시작 (블록을 5개 이상 제거하면 화살 폭탄)
    //    {
    //        //파랑새 위로 가고 폭탄생성
    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.TopLeft)
    //        {
    //            birdType = BirdPositionType.TopLeft;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
    //        }
    //        timer = 0;
    //        while (timer < 0.6f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

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

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //블럭이미지복사
    //        List<NormalBlock> listBlock = new List<NormalBlock>();
    //        for (int i = 5; i < 7; i++)
    //            for (int j = 4; j < 9; j++)
    //            {
    //                listBlock.Add(ManagerBlock.boards[i, j].Block as NormalBlock);
    //            }

    //        List<GameObject> tempObject = new List<GameObject>();

    //        foreach (var tempBlock in listBlock)
    //        {
    //            NormalBlock blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<NormalBlock>();
    //            blockA._transform.position = tempBlock._transform.position;
    //            tempObject.Add(blockA.gameObject);

    //            if (tempBlock.rightLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);

    //                //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
    //                //LinkerB.transform.position = tempBlock.rightLinker.linkerSpriteB_FR.transform.position;
    //                //tempObject.Add(LinkerB.gameObject);
    //            }

    //            if (tempBlock.DownLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);

    //                //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
    //                //LinkerB.transform.position = tempBlock.DownLinker.linkerSpriteB_FR.transform.position;
    //                //tempObject.Add(LinkerB.gameObject);
    //            }
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m4_2"), 0.3f);        //레인보우는 블럭이 10개 이상 연결디었을 때 만들 수 있다마유.


    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        yield return null;

    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m4_3"), 0.3f);        //이것도 터치한 곳에 레인보우 폭탄이 만들어져마유.

    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove(targetPos, 0.3f);
    //        blind.SetSizeCollider(78 * 2, 78 * 5);

    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        bool bPush = false;
    //        float fTimer = 0.0f;
    //        while (ManagerBlock.instance.state != BlockManagrState.MOVE)
    //        {
    //            //기다리는 동안 손가락 이미지 전환.
    //            fTimer += Global.deltaTime * 1.0f;
    //            if (fTimer >= 0.15f)
    //            {
    //                if (bPush == true)
    //                {
    //                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
    //                    bPush = false;
    //                }
    //                else
    //                {
    //                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
    //                    bPush = true;
    //                }
    //                fTimer = 0.0f;
    //            }
    //            yield return null;
    //        }

    //        blind.SetAlpha(0);
    //        //복사이미지제거
    //        foreach (var tempObj in tempObject) Destroy(tempObj);
    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝 

    //    #region 세번째 연출 시작 (자, 한 번 직접 해보자!)
    //    {
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        //손가락 제거.
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

    //        //블라인드 설정.
    //        blind.SetSize(0, 0);
    //        blind.SetSizeCollider(0, 0);

    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }
    //        yield return null;
    //    }
    //    #endregion 세번째 연출 끝 (자, 한 번 직접 해보자!)

    //    #region 네번째 연출 시작 (자, 한 번 직접 해보자!)
    //    {
    //        //블럭이 멈출때까지 기다림
    //        while (ManagerBlock.instance.state != BlockManagrState.WAIT)
    //        {
    //            yield return null;
    //        }

    //        //선택한 블럭 찾기
    //        BlockBase tempSelectBlock = null;

    //        for (int i = 5; i < 7; i++)
    //            {
    //                if (ManagerBlock.boards[i, 8].Block.bombType != BlockBombType.NONE)
    //                {
    //                    tempSelectBlock = ManagerBlock.boards[i, 8].Block;
    //                }
    //            }


    //        targetPos = tempSelectBlock._transform.position;

    //        //목표화면으로 이동
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
    //        blind.SetDepth(-1);
    //        //blind.SetSize(360, 720);
    //        blind.transform.position = targetPos;

    //        //타겟복사하기
    //        GameObject targetObject = NGUITools.AddChild(blind._panel.gameObject, tempSelectBlock.gameObject);
    //        targetObject.transform.position = tempSelectBlock._transform.position;

    //        //파랑새 설정.
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove(targetPos, 0.3f);

    //        //사운드 출력.
    //        //ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(BirdPositionType.TopLeft, Global._instance.GetString("t_g_m4_4"), 0.3f);    //어떤 효과가 있을지 궁금해마유?\n지금 바로 사용해봐.

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        blind.SetSizeCollider(78, 78);

    //        bool bPush = false;
    //        float fTimer = 0.0f;
    //        while (ManagerBlock.instance.state != BlockManagrState.MOVE)
    //        {
    //            //기다리는 동안 손가락 이미지 전환.
    //            fTimer += Global.deltaTime * 1.0f;
    //            if (fTimer >= 0.15f)
    //            {
    //                if (bPush == true)
    //                {
    //                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
    //                    bPush = false;
    //                }
    //                else
    //                {
    //                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
    //                    bPush = true;
    //                }
    //                fTimer = 0.0f;
    //            }
    //            yield return null;
    //        }
    //        Destroy(targetObject);
    //        yield return null;
    //    }
    //    #endregion 네번째 연출 끝 (자, 한 번 직접 해보자!)

    //    #region 다섯번째 연출 시작 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)
    //    {
    //        blind.SetSizeCollider(0, 0);

    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.BottomLeft)
    //        {
    //            birdType = BirdPositionType.BottomLeft;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
    //        }

    //        //손가락 제거.
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
    //        blind.SetAlpha(0);

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }

    //        // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
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
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        yield return null;
    //    }
    //    #endregion 다섯번째 연출 끝 (파랑새 굿, 오브젝트 제거 연출, 뎁스 조절)

    //    #region 여섯번째 연출 시작 (이제 게임을 해보자)   
    //    {
    //        while (ManagerBlock.instance.state != BlockManagrState.WAIT)
    //        {
    //            yield return null;
    //        }
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m4_5"), 0.3f);        //이제 중요한 폭탄은 다 배웠어.\n이제 실전에서 잘 써봐마유~

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        yield return null;
    //    }
    //    #endregion  여섯번째 연출 끝 (이제 게임을 해보자)   

    //    #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    //    {
    //        blind.transform.parent = ManagerUI._instance.transform;
    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

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
    //    #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    //}
    #endregion
}
