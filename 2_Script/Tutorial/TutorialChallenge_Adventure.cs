using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialChallenge_Adventure : TutorialBase 
{
    private IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region #1 챕터1 선택

        //블라인드 생성
        blind._panel.depth = 30;
        blind.SetDepth(-1);
        blind.SetSize(0, 0);
        blind.SetSizeCollider(0, 0);
        blind.transform.localPosition = Vector3.up * 190;
        blind.transform.parent = ManagerUI._instance.transform;
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //마유지 생성
        if (birdType != BirdPositionType.BottomRight)
        {
            birdType = BirdPositionType.BottomRight;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
        }
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m6_1"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
        yield return new WaitForSeconds(2.0f);

        //챕터 스크롤 이동
        UIPopupStageAdventure._instance.scroll_ChapterList.ScrollItemMove(0);
        yield return new WaitForSeconds(0.5f);
        blind.SetSizeCollider(100, 100);

        //블라인드 세팅
        StartCoroutine(BlindAni(Vector2.zero, Vector2.one * 200, 0.5f));

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(blind.transform.position + (Vector3.down * 0.05f), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //챕터1 클릭할 때까지 대기(기다리는 동안 손가락 이미지 전환)
        bool bPush = false;
        float fTimer = 0.0f;

        while (UIPopupStageAdventure.selectedChapter != 1)
        {
            fTimer += Global.deltaTime * 1.0f;
            if (fTimer >= 0.15f)
            {
                if (bPush == true)
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = false;
                }
                else
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = true;
                }
                fTimer = 0.0f;
            }
            yield return null;
        }

        //블라인드 세팅
        StartCoroutine(BlindAni(Vector2.one * 200, Vector2.zero, 0.5f));
        blind.SetSizeCollider(0, 0);

        //스테이지 스크롤 이동
        int itemCount = UIPopupStageAdventure._instance.stageDataList.Count;
        UIPopupStageAdventure._instance.scroll_StageList.ScrollItemMove(itemCount - 1);

        //블라인드 세팅
        blind._textureCenter.border = Vector4.one * 50;
        blind._textureCenter.type = UIBasicSprite.Type.Sliced;
        blind.transform.localPosition = Vector2.down * 375;
        StartCoroutine(BlindAni(Vector2.zero, new Vector2(800, 350), 0.5f));

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bool bTurn = false;
        if (birdType != BirdPositionType.TopLeft)
        {
            birdType = BirdPositionType.TopLeft;
            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
        }

        yield return new WaitForSeconds(1.0f);

        // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
        if (bTurn == true)
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();
        }

        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m6_2"), 0.3f);
        yield return new WaitForSeconds(1.0f);

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m6_3"), 0.3f);
        yield return new WaitForSeconds(1.0f);

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);
        
        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m6_4"), 0.3f);
        yield return new WaitForSeconds(1.0f);

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //파랑새 나가는 연출.
        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

        ManagerTutorial._instance.BlueBirdTurn();

        //파랑새 돌고 난 후 나감.
        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        yield return new WaitForSeconds(0.3f);

        //블라인드 제거
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.6f);

        //튜토리얼 종료
        Destroy(blind.gameObject);
        blind = null;
        Destroy(ManagerTutorial._instance.gameObject);

        #endregion
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(blind != null)
            Destroy(blind.gameObject);
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
