using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialAttribute_Adventure : TutorialBase {

    private IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region #1 준비 팝업에서 속성 튜토리얼

        //블라인드 생성
        blind._panel.depth = 30;
        blind.SetDepth(0);
        blind.SetSize(450, 450);
        blind.SetSizeCollider(0, 0);
        blind.transform.localPosition = new Vector3(290.0f, 380.0f, 0.0f);
        blind.transform.parent = ManagerUI._instance.transform;
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //마유지 생성
        if (birdType != BirdPositionType.BottomLeft)
        {
            birdType = BirdPositionType.BottomLeft;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
        }
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m9_1"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        #endregion

        #region #2 준비 팝업에서 속성 튜토리얼
        
        //블라인드 설정
        blind.SetSize(820, 180);
        UITexture blindCenter = blind._textureCenter.GetComponent<UITexture>();
        blindCenter.type = UIBasicSprite.Type.Sliced;
        blindCenter.border = Vector4.one * 50;
        blind.transform.localPosition = Vector3.zero;

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bool bTurn = false;
        if (birdType != BirdPositionType.TopRight)
        {
            birdType = BirdPositionType.TopRight;
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
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m9_2"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        
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

        //블라인드 비활성화
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        #endregion

        #region #3 튜토리얼 종료

        Destroy(blind.gameObject);
        Destroy(ManagerTutorial._instance.gameObject);

        #endregion

    }
}
