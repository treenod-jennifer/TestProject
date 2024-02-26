using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialDynamite : TutorialBase
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

        #region 연출 시작
        {
            targetPos = ManagerBlock.boards[5, 8].Block.transform.position;

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

            //파랑새 등장 설명해준다고 얘기시작
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m13_1"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
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

        #region 내려와서 1단계설명
        {
            targetPos = ManagerBlock.boards[5, 8].Block.transform.position;

            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);
            blind.SetSize(0, 0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            bool bTurn = false;
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);


            List<BlockBase> listBlock = new List<BlockBase>();
            listBlock.Add(ManagerBlock.boards[5, 8].Block as BlockBase);
            List<GameObject> tempObject = new List<GameObject>();

            foreach (var tempBlock in listBlock)
            {
                BlockBase blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<BlockBase>();
                blockA._transform.position = tempBlock._transform.position;
                tempObject.Add(blockA.gameObject);
            }

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m13_2"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
            blind.SetSizeCollider(0, 0);
            foreach (var tempObj in tempObject) Destroy(tempObj);       //오브젝트제거

            yield return null;
        }
        #endregion

        #region 2단계 선택
        {
            targetPos = (ManagerBlock.boards[4, 8].Block.transform.position + ManagerBlock.boards[5, 8].Block.transform.position) * 0.5f;

            //블라인드 설정.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);


            //블럭이미지복사
            List<BlockBase> listBlock = new List<BlockBase>();
            listBlock.Add(ManagerBlock.boards[4, 8].Block as BlockBase);
            listBlock.Add(ManagerBlock.boards[5, 8].Block as BlockBase);

            List<GameObject> tempObject = new List<GameObject>();

            foreach (var tempBlock in listBlock)
            {
                BlockBase blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<BlockBase>();
                blockA._transform.position = tempBlock._transform.position;
                tempObject.Add(blockA.gameObject);

                if (tempBlock.rightLinker != null)
                {
                    UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
                    LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
                    tempObject.Add(LinkerA.gameObject);
                }
            }

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m13_3"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            blind.SetSizeCollider(78 * 2, 78);

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
            StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
            foreach (var tempObj in tempObject) Destroy(tempObj);       //오브젝트제거

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            yield return null;
        }
        #endregion

        #region 2단계 단계 내리고 다시 설명
        {
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);


            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m13_4"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
            blind.SetSizeCollider(0, 0);

            yield return null;
        }
        #endregion


        #region 다시 설명
        {
            blind.transform.localPosition += Vector3.zero;
            blind._panel.depth = 10;
            blind.SetSizeCollider(0, 0);
            blind.SetDepth(0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            timer = 0;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);


            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m13_5"), 0.3f);      //더 강한 효과를 보고 싶다면            폭탄을 연결해서 사용해 봐.
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //파랑새 애니메이션 & 이동
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            StartCoroutine(textBox.DestroyTextBox(0.3f));               //말풍선 제거.
            blind.SetSizeCollider(0, 0);

            yield return null;
        }
        #endregion







        #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
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

