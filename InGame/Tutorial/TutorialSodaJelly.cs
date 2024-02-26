using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialSodaJelly : TutorialBase
{
    private List<BlockSodaJelly> listSodaJelly = new List<BlockSodaJelly>();
    private List<Sequence> listTweenSequence = new List<Sequence>();

    IEnumerator Start()
    {
        float timer = 0;
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
            tutorialRoot.manualWidth = 750;

        //맵에 있는 소다젤리 구하기.
        GetListSodaJelly();

        #region 첫번째 연출 시작(소다 젤리 모아보자)
        {
            //블라인드 설정.
            blind.transform.position = Vector3.zero;
            blind.transform.localPosition += Vector3.zero;
            blind.SetSizeCollider(0, 0);
            blind.SetActiveDefulatBlind(false, 10);

            //커스텀 블라인드 생성.
            blind.MakeCustomBlindTexture(2000, 3000);
            blind.customBlind.SetAlphaCustomBlind(0f);

            timer = 0;
            while (timer < 0.5f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            //파랑새 등장
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m21_1"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 두번째 연출 시작(젤리틀에 젤리를 만들어보자)
        {
            //파랑새 회전
            bool bTurn = false;
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
                yield return new WaitForSeconds(0.3f);
            }

            if (bTurn == true)
            {
                ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

                ManagerTutorial._instance.BlueBirdTurn();
            }

            ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

            //커스텀 블라인드 설정.
            blind.customBlind.FillColorAtCustomBlindTexture(GetCustomBlindAlphaData(), Color.clear);
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.5f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m21_2"), 0.3f);

            //소다젤리 오브젝트 움직이기
            RotateJellySoda();

            yield return new WaitForSeconds(0.3f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            //소다젤리 움직임 멈추기.
            StopRotationSequence();
            StopRotateSodaJelly();

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 세번째 연출 시작(틀 주변의 블록을 제거)
        {
            //소다젤리 다 어둡게 처리.
            List<CustomBlindData> listJellyData = new List<CustomBlindData>();
            for (int i = 0; i < listSodaJelly.Count; i++)
            {
                listJellyData.Add(new CustomBlindData(listSodaJelly[i].transform.localPosition, 95, 95));
            }

            //커스텀 블라인드 설정.
            blind.customBlind.FillColorAtCustomBlindTexture(listJellyData, Color.black);
            blind.customBlind.FillColorAtCustomBlindTexture(GetCustomBlindDataByBlockPos(6, 9, 4, 8, 82, 85), Color.clear);

            //소다젤리 오브젝트 움직이기
            BlockBase tempSodaBlock = PosHelper.GetBlock(6, 5);
            RotateObj(tempSodaBlock.mainSprite.gameObject);

            //블록찾기
            BlockBase tempBlock = PosHelper.GetBlock(7, 5);
            float posX = tempBlock.transform.localPosition.x;
            float posY = tempBlock.transform.localPosition.y;

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = new Vector3(posX, posY, 0f);
            finger.transform.DOLocalMove(targetPos, 0.3f);

            //파랑새 애니메이션, 사운드 출력
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m21_3"), 0.3f);

            yield return new WaitForSeconds(0.5f);
            blind.customBlind.SetTouchData(GetCustomBlindDataByBlockPos(7, 8, 5, 5, 82, 85), () => Pick.instance.OnPressAtTutorial());

            bool bPush = false;
            float fTimer = 0.0f;
            while (ManagerBlock.instance.state != BlockManagrState.MOVE)
            {
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
            //블라인드 터치영역 초기화 & 알파.
            blind.customBlind.ResetTouchData();
            blind.customBlind.SetAlphaCustomBlind(0);

            //소다젤리 오브젝트 움직이기
            StopRotationSequence();
            ResetRotationObj(tempSodaBlock.mainSprite.gameObject);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.5f);
        }
        #endregion

        #region 네번째 연출 시작(아직 하나가 남았네)
        {
            //블럭이 멈출때까지 기다림
            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }

            //커스텀 블라인드 설정.
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.5f);
            
            //소다젤리 오브젝트 움직이기
            BlockBase tempSodaBlock = PosHelper.GetBlock(9, 7);
            RotateObj(tempSodaBlock.mainSprite.gameObject);

            //블록찾기
            BlockBase tempBlock = PosHelper.GetBlock(8, 7);
            float posX = tempBlock.transform.localPosition.x;
            float posY = tempBlock.transform.localPosition.y;

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = new Vector3(posX, posY, 0f);
            finger.transform.DOLocalMove(targetPos, 0.3f);

            //파랑새 애니메이션, 사운드 출력
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m21_4"), 0.3f);

            yield return new WaitForSeconds(0.5f);
            blind.customBlind.SetTouchData(GetCustomBlindDataByBlockPos(7, 8, 7, 7, 82, 85), () => Pick.instance.OnPressAtTutorial());

            bool bPush = false;
            float fTimer = 0.0f;
            while (ManagerBlock.instance.state != BlockManagrState.MOVE)
            {
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

            //블라인드 터치영역 초기화 & 알파.
            blind.customBlind.ResetTouchData();
            blind.customBlind.SetAlphaCustomBlind(0);

            //소다젤리 오브젝트 움직이기
            StopRotationSequence();
            ResetRotationObj(tempSodaBlock.mainSprite.gameObject);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //파랑새 애니메이션 & 사운드 출력.
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
                bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
            }

            if (bTurn == true)
            {
                yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

                ManagerTutorial._instance.BlueBirdTurn();
            }
        }
        #endregion

        #region 다섯번째 연출 시작(폭탄 효과로도 젤리를 만들 수도 있음)
        {
            //파랑새 애니메이션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            //블럭이 멈출때까지 기다림
            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);

            //사운드 출력
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m21_5"), 0.3f);
            yield return new WaitForSeconds(0.5f);

            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 마지막 연출 시작
        {
            ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

            ManagerTutorial._instance.BlueBirdTurn();

            //파랑새 돌고 난 후 나감.
            StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
            yield return new WaitForSeconds(0.3f);

            //GameUIManager.instance.adventureDarkBGBlock.SetActive(false);
            GameUIManager.instance.ShowAdventureDarkBGBlock(false);

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion
    }

    private void GetListSodaJelly()
    {
        if(listSodaJelly.Count > 0)
            listSodaJelly.Clear();

        //블록찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.SODAJELLY)
            {
                BlockSodaJelly sodaJelly = tempBlock.Block as BlockSodaJelly;
                if (sodaJelly != null)
                    listSodaJelly.Add(sodaJelly);
            }
        }
    }

    private void RotateJellySoda()
    {
        listTweenSequence.Clear();
        for (int i = 0; i < listSodaJelly.Count; i++)
        {
            if (listSodaJelly[i].lifeCount <= 1)
                continue;
            RotateObj(listSodaJelly[i].mainSprite.gameObject);
        }
    }

    private void RotateObj(GameObject obj)
    {
        obj.transform.rotation = Quaternion.Euler(0f, 0f, 15f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(obj.transform.DORotate(new Vector3(0f, 0f, -15f), 0.5f));
        sequence.SetLoops(-1, LoopType.Yoyo);
        listTweenSequence.Add(sequence);
    }

    private void StopRotationSequence()
    {
        for (int i = 0; i < listTweenSequence.Count; i++)
        {
            listTweenSequence[i].Kill();
        }
        listTweenSequence.Clear();
    }

    private void StopRotateSodaJelly()
    {
        for (int i = 0; i < listSodaJelly.Count; i++)
        {
            if (listSodaJelly[i].lifeCount <= 1)
                continue;
            ResetRotationObj(listSodaJelly[i].mainSprite.gameObject);
        }
    }

    private void ResetRotationObj(GameObject obj)
    {
        obj.transform.rotation = Quaternion.identity;
    }

    private List<CustomBlindData> GetCustomBlindAlphaData()
    {
        List<CustomBlindData> listAlphaData = new List<CustomBlindData>();
        for (int i = 0; i < listSodaJelly.Count; i++)
        {
            listAlphaData.Add(new CustomBlindData(listSodaJelly[i].transform.localPosition + new Vector3(2f, 2f, 0f), 85, 85));
        }
        return listAlphaData;
    }

    //블록 좌표를 사용해 특정 영역의 커스텀 블라인드 사이즈를 가져옴
    private List<CustomBlindData> GetCustomBlindDataByBlockPos(int x1, int x2, int y1, int y2, int sizeX, int sizeY)
    {   
        BlockBase tempBlock_1 = PosHelper.GetBlock(x1, y1);
        BlockBase tempBlock_2 = PosHelper.GetBlock(x2, y2);

        if (tempBlock_1 == null || tempBlock_2 == null)
            return null;

        float centerPos_X = (tempBlock_1.transform.localPosition.x + tempBlock_2.transform.localPosition.x) * 0.5f;
        float centerPos_Y = (tempBlock_1.transform.localPosition.y + tempBlock_2.transform.localPosition.y) * 0.5f;
        Vector3 centerPos = new Vector3(centerPos_X, centerPos_Y, 0f);
        int touchSize_X = (x1 > x2) ? (x1 - x2 + 1) * sizeX : (x2 - x1 + 1) * sizeX;
        int touchSize_Y = (y1 > y2) ? (y1 - y2 + 1) * sizeY : (y2 - y1 + 1) * sizeY;

        List<CustomBlindData> listData = new List<CustomBlindData>();
        listData.Add(new CustomBlindData(centerPos, touchSize_X, touchSize_Y));
        return listData;
    }
}
