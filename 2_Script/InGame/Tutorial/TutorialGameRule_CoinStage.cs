using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGameRule_CoinStage : TutorialBase
{
    private Vector2 blindBoxSize_targetUI = new Vector2(330f, 240f);
    private Vector2 blindBoxSize_feverUI = new Vector2(380f, 230f);

    IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region 첫번째 연출 시작 (코인스테이지는 시간제한)
        {
            //블라인드 생성.
            blind.transform.position = GameUIManager.instance.MoveCountBGRoot.transform.position;
            blind.transform.localPosition += new Vector3(0f, 15f, 0f);
            blind._panel.depth = 10;
            blind.SetDepth(0);
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);
            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

            //파랑새 생성.
            birdType = BirdPositionType.BottomRight;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            yield return new WaitForSeconds(0.3f);
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //블라인드 세팅.
            blind._textureCenter.border = Vector4.one * 50;
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            blind.SetSize((int)blindBoxSize_targetUI.x, (int)blindBoxSize_targetUI.y);

            //파랑새 모션 변경.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m2_1"), 0.3f);
            yield return new WaitForSeconds(0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //블라인드 세팅.
            blind._textureCenter.type = UIBasicSprite.Type.Simple;
            blind.SetSize(0, 0);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 첫번째 연출 끝 (코인스테이지는 시간제한)

        #region 두번째 연출 시작 (게이지를 채우면 피버모드)
        {
            //파랑새 설정.
            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
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

            //블라인드 세팅.
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            blind.transform.position = GameUIManager.instance.targetBG.transform.position;
            blind.SetSize((int)blindBoxSize_feverUI.x, (int)blindBoxSize_feverUI.y);

            //파랑새 모션 변경.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m2_2"), 0.3f);
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
        #endregion 두번째 연출 끝 (게이지를 채우면 피버모드)

        #region 세번째 연출 시작(피버모드 설명)
        {
            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m2_3"), 0.3f);

            yield return new WaitForSeconds(0.3f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            //블라인드 세팅.
            blind._textureCenter.type = UIBasicSprite.Type.Simple;
            blind.SetSize(0, 0);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion 세번째 연출 끝(피버모드 설명)

        #region 네번째 연출 시작(골드쿠니 춤추게 하자)
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
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m2_4"), 0.3f);

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
        #endregion 네번째 연출 끝(골드쿠니 춤추게 하자)

        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            blind.SetAlpha(0);
            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            yield return new WaitForSeconds(0.3f);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion 마지막 연출 끝
    }
}
