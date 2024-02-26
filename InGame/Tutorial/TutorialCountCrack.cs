using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialCountCrack : TutorialBase
{
    private Coroutine targetCoroutine = null;
    private Sequence sequence = null;

    IEnumerator Start()
    {
        List<CustomBlindData> listSelectBlind_1 = new List<CustomBlindData>();
        List<CustomBlindData> listSelectBlind_2 = new List<CustomBlindData>();
        CustomBlindData targetBlindData 
            = new CustomBlindData(GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(2f, 25f, 0f), 425, 170);

        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
            tutorialRoot.manualWidth = 750;

        #region 첫번째 연출 시작(목표를 순서대로 제거)
        {
            //블라인드 설정.
            blind.transform.position = Vector3.zero;
            blind.transform.localPosition += Vector3.zero;
            blind.SetSizeCollider(0, 0);
            blind.SetActiveDefulatBlind(false, 10);

            //커스텀 블라인드 생성.
            blind.MakeCustomBlindTexture(2000, 3000);
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            //파랑새 등장
            if (birdType != BirdPositionType.BottomLeft)
            {
                birdType = BirdPositionType.BottomLeft;
                ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
            }
            yield return new WaitForSeconds(0.3f);

            //커스텀 블라인드 설정.
            listSelectBlind_1 = new List<CustomBlindData>(GetCustomBlindDataByBlockPos(7, 7, 2, 4, 82, 85));
            listSelectBlind_2 = new List<CustomBlindData>(GetCustomBlindDataByBlockPos(9, 9, 2, 4, 82, 85));

            blind.customBlind.FillColorAtCustomBlindTexture(targetBlindData, Color.clear);
            blind.customBlind.FillColorAtCustomBlindTexture(listSelectBlind_1, Color.clear);
            blind.customBlind.FillColorAtCustomBlindTexture(listSelectBlind_2, Color.clear);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m22_1"), 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            ShowStageTarget();
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            yield return new WaitForSeconds(0.5f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            StopShowStageTarget();
            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 두번째 연출 시작(첫번째 석판을 제거해봐)
        {
            //블록찾기
            BlockBase tempBlock = PosHelper.GetBlock(7, 4);
            float posX = tempBlock.transform.localPosition.x;
            float posY = tempBlock.transform.localPosition.y;

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = new Vector3(posX, posY, 0f);
            finger.transform.DOLocalMove(targetPos, 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m22_2"), 0.3f);

            //커스텀 블라인드 설정.
            blind.customBlind.FillColorAtCustomBlindTexture(targetBlindData, Color.black);
            targetBlindData = new CustomBlindData(GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(-125f, 25f, 0f), 150, 170);
            blind.customBlind.FillColorAtCustomBlindTexture(targetBlindData, Color.clear);

            yield return new WaitForSeconds(0.5f);
            blind.customBlind.SetTouchData(GetCustomBlindDataByBlockPos(7, 4, 82, 85), () => Pick.instance.OnPressAtTutorial());

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

            //파랑새 모션
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //블라인드 터치영역 초기화 & 알파.
            blind.customBlind.ResetTouchData();
            blind.customBlind.SetAlphaCustomBlind(0);
            blind.customBlind.FillColorAtCustomBlindTexture(listSelectBlind_1, Color.black);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.5f);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }
        }
        #endregion

        #region 세번째 연출 시작(남아있는 첫번째 석판을 제거)
        {
            //블록찾기
            BlockBase tempBlock = PosHelper.GetBlock(9, 4);
            float posX = tempBlock.transform.localPosition.x;
            float posY = tempBlock.transform.localPosition.y;

            //손가락 생성.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            Vector3 targetPos = new Vector3(posX, posY, 0f);
            finger.transform.DOLocalMove(targetPos, 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m22_3"), 0.3f);

            //파랑새 모션
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            //커스텀 블라인드 설정.
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.3f);

            yield return new WaitForSeconds(0.5f);
            blind.customBlind.SetTouchData(GetCustomBlindDataByBlockPos(9, 4, 82, 85), () => Pick.instance.OnPressAtTutorial());

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

            //파랑새 모션
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            //블라인드 터치영역 초기화 & 알파.
            blind.customBlind.ResetTouchData();
            blind.customBlind.FillColorAtCustomBlindTexture(targetBlindData, Color.black);
            blind.customBlind.FillColorAtCustomBlindTexture(listSelectBlind_2, Color.black);
            blind.customBlind.SetAlphaCustomBlind(0);

            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
            yield return new WaitForSeconds(0.5f);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }
        }
        #endregion

        #region 네번째 연출 시작(두번째 석판)
        {
            //파랑새 회전
            bool bTurn = false;
            if (birdType != BirdPositionType.BottomRight)
            {
                birdType = BirdPositionType.BottomRight;
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

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m22_4"), 0.3f);

            //커스텀 블라인드 설정.
            List<CustomBlindData> listBlindData = GetCustomBlindDataByBlockPos(7, 9, 6, 10, 82, 85);
            targetBlindData
            = new CustomBlindData(GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(-90f, 25f, 0f), 200, 170);
            listBlindData.Add(targetBlindData);
            blind.customBlind.FillColorAtCustomBlindTexture(listBlindData, Color.clear);
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.3f);

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

            Destroy(blind.gameObject);
            Destroy(ManagerTutorial._instance.gameObject);
        }
        #endregion
        yield return null;
    }

    private void ShowStageTarget()
    {
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            GameUIManager.instance.listGameTarget[i]._transform.localScale = Vector3.one * 0.9f;
        }
        targetCoroutine = StartCoroutine(CoShowStageTarget());
    }

    private IEnumerator CoShowStageTarget()
    {
        int count = GameUIManager.instance.listGameTarget.Count;
        int currentIndex = 0;
        while (true)
        {
            sequence = DOTween.Sequence();
            sequence.Append(GameUIManager.instance.listGameTarget[currentIndex]._transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.InCubic));
            yield return new WaitForSeconds(0.5f);
            sequence.Append(GameUIManager.instance.listGameTarget[currentIndex]._transform.DOScale(Vector3.one * 0.9f, 0.2f).SetEase(Ease.InCubic));

            currentIndex = (currentIndex + 1 >= count) ? 0 : currentIndex + 1;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void StopShowStageTarget()
    {
        StopCoroutine(targetCoroutine);
        targetCoroutine = null;
        sequence.Kill();
        sequence = null;
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            GameUIManager.instance.listGameTarget[i]._transform.localScale = Vector3.one;
        }
    }

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

    private List<CustomBlindData> GetCustomBlindDataByBlockPos(int x1, int y1, int sizeX, int sizeY)
    {
        BlockBase tempBlock_1 = PosHelper.GetBlock(x1, y1);
        if (tempBlock_1 == null)
            return null;

        List<CustomBlindData> listData = new List<CustomBlindData>();
        Vector3 centerPos = new Vector3(tempBlock_1.transform.localPosition.x, tempBlock_1.transform.localPosition.y, 0f);
        listData.Add(new CustomBlindData(centerPos, sizeX, sizeY));
        return listData;
    }
}
