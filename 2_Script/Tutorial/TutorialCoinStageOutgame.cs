using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TutorialCoinStageOutgame : TutorialBase
{
    Character character;
    bool trackingOn = false;
    UITexture finger = null;

    // Use this for initialization
    IEnumerator Start()
    {
        finger = ManagerTutorial._instance._spriteFinger;
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;


        character = ManagerLobby._instance.GetCharacter((TypeCharacterType)22);
        var ai = character.GetComponent<LobbyAnimalAI>();
        ai.aiHint = Instantiate(ai.aiHint);
        ai.aiHint.monologs.Clear();

        #region 첫번째 연출 시작 (미션 클리어를 알려줄게)
        {
            //블라인드 설정1.
            blind.transform.position = Vector3.zero;
            blind._panel.depth = 10;
            blind.SetDepth(0);
            blind.SetSizeCollider(0, 0);

            trackingOn = true;

            yield return null;
            yield return null;
            //화면 이동.
            CameraController._instance.MoveToPosition(character.transform.position);
            //위치 구하기.
            Camera guiCam = NGUITools.FindCameraForLayer(blind.gameObject.layer);
            Vector3 targetPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(character.transform.position));
            targetPos.z = 0f;

            //블라인드 설정2.
            blind.transform.position = targetPos;
            blind.transform.localPosition += Vector3.up * (GetUIOffSet());
            blind.SetSize(300, 300);

            StartCoroutine(BlindAni(new Vector2(0f, 0f), new Vector2(500f, 500f), 0.1f));

            DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            //파랑새 생성.
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_1"), 0.3f);           

            StartCoroutine(BlindAni(new Vector2(500f, 500f), new Vector2(1200f, 1200f), 0.5f));

            

            ManagerSound.AudioPlay(AudioLobby.Mission_Finish);

            character.MakeSpeech(Global._instance.GetString("gk_c_4"), AudioLobby.DEFAULT_AUDIO, true);

            yield return new WaitForSeconds(.5f);
            
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return textBox.DestroyTextBox(0.3f);            

            yield return new WaitForSeconds(1f);

            bool bTurn = false;
            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_2"), 0.3f);

            character.MakeSpeech(Global._instance.GetString("gk_c_2"), AudioLobby.DEFAULT_AUDIO, true);            

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return textBox.DestroyTextBox(0.3f);

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_3"), 0.3f);

            character.MakeSpeech(Global._instance.GetString("gk_c_1"), AudioLobby.DEFAULT_AUDIO, true, 2.0f, false);

            yield return new WaitForSeconds(1.5f);

            StartCoroutine(BlindAni(new Vector2(1200f, 1200f), new Vector2(500f, 500f), 0.5f));

            yield return new WaitForSeconds(0.5f);


            //손가락 생성.
            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(targetPos, 0.3f);
            yield return new WaitForSeconds(0.5f);

            //블라인드 터치 영역 설정.
            blind.SetSizeCollider_ClickFunc(150, 150, OnClickedAnimal);

            bool bPush = false;
            float fTimer = 0.0f;

            while (UIPopupReadyCoinStage.instance == null)
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

            //블라인드 설정.
            blind.SetSize(0, 0);
            blind.SetSizeCollider(0, 0);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            yield return textBox.DestroyTextBox(0.3f);

            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

             //블라인드 세팅.
            blind._textureCenter.border = Vector4.one * 50;
            blind._textureCenter.type = UIBasicSprite.Type.Sliced;
            blind.transform.localPosition = new Vector3(0f, 70f);
            yield return BlindAni(Vector2.zero, new Vector2(800f, 440f), 0.3f);

            yield return new WaitForSeconds(0.5f);

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_4"), 0.3f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            yield return textBox.DestroyTextBox(0.3f);

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_5"), 0.3f);

           

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            yield return null;

            yield return textBox.DestroyTextBox(0.3f);

            yield return new WaitForSeconds(0.5f);

            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

            if (birdType != BirdPositionType.TopRight)
            {
                birdType = BirdPositionType.TopRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }

            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_cs_m1_6"), 0.3f);

            blind.transform.localPosition = new Vector3(0f, -340);
            StartCoroutine(BlindAni(Vector2.zero, new Vector2(400f, 200f), 0.3f));

            finger.color = new Color(1f, 1f, 1f, 0f);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(UIPopupReadyCoinStage.instance.objStartButton.transform.position, 0.3f);
            yield return new WaitForSeconds(0.5f);

            blind.SetSizeCollider_ClickFunc(400, 200, OnStartClicked);

            float finalWaitTime = 0.0f;
            while (UIPopupReadyCoinStage.instance != null)
            {
                //기다리는 동안 손가락 이미지 전환.
                fTimer += Global.deltaTime * 1.0f;
                finalWaitTime += Global.deltaTime * 1.0f;
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
                if (finalWaitTime > 0.8f)
                    break;

                yield return null;
            }

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            yield return textBox.DestroyTextBox(0.2f);
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

            Destroy(blind.gameObject);
            blind = null;
            Destroy(ManagerTutorial._instance.gameObject);

            yield return new WaitForSeconds(0.1f);
        }
        #endregion 첫번째 연출 끝 (미션 클리어를 알려줄게)
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (blind != null)
            Destroy(blind.gameObject);

        if( finger != null )
        {
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.0f);
        }
        if ( textBox != null && textBox.gameObject != null)
        {
            textBox.DestroyTextBox(0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (this.trackingOn)
        {
            if (character != null && CameraController._instance != null)
                CameraController._instance.MoveToPosition(character.transform.position);
        }

    }

    private float GetUIOffSet()
    {
        float ratio = Camera.main.fieldOfView;
        return ratio;
    }

    void OnClickedAnimal()
    {
        ManagerCoinBonusStage.instance.OnNPCTap(null);
    }

     void OnStartClicked()
    {
        StartCoroutine(CoStartGame());
    }

    IEnumerator CoStartGame()
    {
        if( finger != null )
        {
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.1f);
        }

        yield return new WaitForSeconds(0.1f);

        if ( textBox != null && textBox.gameObject != null)
        {
            yield return textBox.DestroyTextBox(0.3f);
        }

        Destroy(blind.gameObject);
        blind = null;

        UIPopupReadyCoinStage.instance.OnClickGoInGame();

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
