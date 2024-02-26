using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialBlockBlack : TutorialBase
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


    //    #region 연출 시작 
    //    {
    //        targetPos = (ManagerBlock.boards[3, 5].Block.transform.position + ManagerBlock.boards[4, 5].Block.transform.position) * 0.5f;

    //        //블라인드 설정.
    //        blind.transform.position = targetPos;
    //        blind.transform.localPosition += Vector3.zero;
    //        blind._panel.depth = 10;
    //        blind.SetSizeCollider(0, 0);
    //        blind.SetDepth(0);
    //        blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //파랑새 등장 설명해준다고 얘기시작
    //        if (birdType != BirdPositionType.TopLeft)
    //        {
    //            birdType = BirdPositionType.TopLeft;
    //            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
    //        }

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");


    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m8_1"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        //파랑새 애니메이션 & 이동
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.

    //        yield return null;
    //    }
    //    #endregion

    //    #region 첫번째
    //    {
    //        targetPos = (ManagerBlock.boards[3, 5].Block.transform.position);

    //        //블라인드 설정.
    //        blind.transform.position = targetPos;
    //        blind.transform.localPosition += Vector3.zero;
    //        blind._panel.depth = 10;
    //        blind.SetSizeCollider(0, 0);
    //        blind.SetDepth(0);
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);


    //        bool bTurn = false;

    //        //파랑새 생성.
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
    //        }
    //        yield return new WaitForSeconds(0.6f);

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
    //        List<BlockBase> listBlock = new List<BlockBase>();
    //        listBlock.Add(ManagerBlock.boards[3, 5].Block as BlockBase);
    //        List<GameObject> tempObject = new List<GameObject>();
    //        foreach (var tempBlock in listBlock)
    //        {
    //            BlockBase blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<BlockBase>();
    //            blockA._transform.position = tempBlock._transform.position;
    //            tempObject.Add(blockA.gameObject);

    //            if (tempBlock.rightLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);
    //            }

    //            if (tempBlock.DownLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);
    //            }
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m8_2"), 0.3f);
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
    //        foreach (var tempObj in tempObject) Destroy(tempObj);       //오브젝트제거

    //        yield return null;
    //    }
    //    #endregion  여섯번째 연출 끝 (이제 게임을 해보자)   



    //    #region 첫번째 연출 시작 
    //    {
    //        targetPos = (ManagerBlock.boards[8, 5].Block.transform.position + ManagerBlock.boards[7, 5].Block.transform.position) * 0.5f;

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
    //        bool bTurn = false;
    //        //파랑새 등장 설명해준다고 얘기시작
    //        if (birdType != BirdPositionType.TopLeft)
    //        {
    //            birdType = BirdPositionType.TopLeft;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
    //        }
            
    //        //yield return new WaitForSeconds(0.6f);

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
    //        List<BlockBase> listBlock = new List<BlockBase>();
    //        listBlock.Add(ManagerBlock.boards[8, 5].Block as BlockBase);
    //        listBlock.Add(ManagerBlock.boards[7, 5].Block as BlockBase);

    //        List<GameObject> tempObject = new List<GameObject>();

    //        foreach (var tempBlock in listBlock)
    //        {
    //            BlockBase blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<BlockBase>();
    //            blockA._transform.position = tempBlock._transform.position;
    //            tempObject.Add(blockA.gameObject);

    //            if (tempBlock.rightLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSprite.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.rightLinker.linkerSprite.transform.position;
    //                tempObject.Add(LinkerA.gameObject);
    //            }

    //            if (tempBlock.DownLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSprite.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.DownLinker.linkerSprite.transform.position;
    //                tempObject.Add(LinkerA.gameObject);
    //            }
    //        }
    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m8_3"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
    //        foreach (var tempObj in tempObject) Destroy(tempObj);       //오브젝트제거

    //        //파랑새 애니메이션 & 이동
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

    //        yield return null;
    //    }
    //    #endregion

    //    #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    //    {
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
    //        //DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
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
