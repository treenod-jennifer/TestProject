using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialMoleCatch : TutorialBase
{
    private BlindTutorial blind;

    private Vector2 blindBoxSize_hole = new Vector2(700f, 500f);
    private Vector2 blindBoxSize_board = new Vector2(600f, 500f);

    private IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region 첫번째 연출(두더지를 선택하면 이벤트에 참여할 수 있어)
        {
            //블라인드 생성
            blind._panel.depth = 30;
            blind.SetDepth(-1);
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);
            blind.transform.localPosition = Vector3.up * 190;
            blind.transform.parent = ManagerUI._instance.transform;
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            //파랑새 생성.
            if (birdType != BirdPositionType.TopLeft)
            {
                birdType = BirdPositionType.TopLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            yield return new WaitForSeconds(0.3f);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //블라인드 세팅.
            blind.transform.parent = UIPopupMoleCatch._instance.GetMoleCatchTr();
            blind.transform.localPosition = UIPopupMoleCatch._instance.GetHoleTr();

            blind._textureCenter.border = Vector4.one * 50;
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            yield return BlindAni(Vector2.zero, blindBoxSize_hole, 0.3f);

            //파랑새 모션 변경.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_mc_m1_1"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            //블라인드 세팅.
            StartCoroutine(BlindAni(blindBoxSize_hole, Vector2.zero, 0.2f));
            yield return new WaitForSeconds(0.2f);
            blind._textureCenter.type = UIBasicSprite.Type.Simple;
        }
        #endregion

        #region 두번째 연출(번호판이 열리면서 보상이 나타나)
        {
            //파랑새 설정.
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //블라인드 세팅.
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            blind.transform.localPosition = UIPopupMoleCatch._instance.GetBoardTr();
            yield return BlindAni(Vector2.zero, blindBoxSize_board, 0.3f);

            //파랑새 모션 변경.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_mc_m1_2"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 세번째 연출(두더지를 잡고 숨겨진 보상을 획득하자)
        {
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_mc_m1_3"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //블라인드 세팅.
            StartCoroutine(BlindAni(blindBoxSize_board, Vector2.zero, 0.2f));
            yield return new WaitForSeconds(0.2f);
            blind._textureCenter.type = UIBasicSprite.Type.Simple;

            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 네번째 연출(두더지를 잡고 숨겨진 보상을 획득하자)
        {
            //파랑새 설정.
            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            yield return new WaitForSeconds(0.3f);

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_mc_m1_4"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 다섯번째 연출(파랑새 나가기, 두더지 강조)
        {
            //파랑새 나가는 연출.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            yield return new WaitForSeconds(0.3f);

            //블라인드 세팅.
            blind.transform.parent = UIPopupMoleCatch._instance.GetMoleCatchTr();
            blind.transform.localPosition = UIPopupMoleCatch._instance.GetHoleTr();
            
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            yield return BlindAni(Vector2.zero, blindBoxSize_hole, 0.3f);

            //손가락 연출.
            yield return FingersAction();
            //블라인드 세팅.
            DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
        #endregion

        #region 마지막 연출 (튜토리얼 오브젝트 삭제)
        {
            //오브젝트 제거.
            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
     
    }

    private IEnumerator FingersAction()
    {
        List<Transform> listMoleObj = UIPopupMoleCatch._instance.GetUpMoleList();
        List<UITexture> listFingers = new List<UITexture>();
        Vector3 fingerOffset = UIPopupMoleCatch._instance.GetHitOffset();

        int depth = 100;
        for (int i = 0; i < UIPopupMoleCatch._instance.GetStageListCount(); i++)
        {
            Vector3 targetPos = Vector3.zero;
            if (listMoleObj.Count <= i)
                continue;
            
            targetPos = listMoleObj[i].position;

            GameObject fingerObj = NGUITools.AddChild(this.gameObject, ManagerTutorial._instance._spriteFinger.gameObject);
            UITexture finger = fingerObj.GetComponent<UITexture>();
            if (finger != null)
            {
                listFingers.Add(finger);

                finger.depth = depth;
                finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
                finger.transform.position = targetPos;
                finger.transform.localPosition += fingerOffset;
                finger.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

                depth --;
            }
        }

        bool bPush = false;
        float fTimer = 0.0f;
        while (fTimer < 0.1f)
        {
            //기다리는 동안 손가락 이미지 전환.
            fTimer += Global.deltaTime * 1.0f;
            for (int i = 0; i < listFingers.Count; i++)
            {
                if (bPush == true)
                {
                    listFingers[i].mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = false;
                }
                else
                {
                    listFingers[i].mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = true;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;

        for (int i = 0; i < listFingers.Count; i++)
        {
            UITexture texture = listFingers[i];
            DOTween.ToAlpha(() => texture.color, x => texture.color = x, 0f, 0.2f);
        }
    }

    private IEnumerator BlindAni(Vector2 startSize, Vector2 endSize, float time)
    {
        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimeLobby;

            Vector2 size = Vector2.Lerp(startSize, endSize, totalTime / time);
            blind.SetSize(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));

            if (totalTime > time)
                yield break;

            yield return null;
        }
    }
}
