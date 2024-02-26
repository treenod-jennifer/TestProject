using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialPea : TutorialBase
{
    private List<Sequence> listTweenSequence = new List<Sequence>();
    List<BlockPea> listActivePea = new List<BlockPea>();

    IEnumerator Start()
    {
        List<CustomBlindData> listCostomBlindData = new List<CustomBlindData>();

        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
            tutorialRoot.manualWidth = 750;

        #region 첫번째 연출 시작(완두콩을 모아보자)
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

            //블럭찾기
            listActivePea = new List<BlockPea>(GetListBlockPea(true));
            RotateBlockPea();

            //커스텀 블라인드 설정.
            List<CustomBlindData> listActivePeaBlindData = new List<CustomBlindData>(GetListBlockPeaCustomData(true));
            List<CustomBlindData> listInActivePeaBlindData = new List<CustomBlindData>(GetListBlockPeaCustomData(false));
            blind.customBlind.FillColorAtCustomBlindTexture(listActivePeaBlindData, Color.clear);
            blind.customBlind.FillColorAtCustomBlindTexture(listInActivePeaBlindData, new Color(0f, 0f, 0f, 0.5f));
            listCostomBlindData.AddRange(listActivePeaBlindData);
            listCostomBlindData.AddRange(listInActivePeaBlindData);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m23_1"), 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            //파랑새 모션.
            ManagerTutorial._instance.SetBirdAnimation("T_idle_Up", "T_targetUp_loop");

            yield return new WaitForSeconds(0.5f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }
            //완두콩 움직임 멈추기.
            StopRotationSequence();
            StopRotateBlockPea();

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            yield return new WaitForSeconds(0.2f);
        }
        #endregion

        #region 두번째 연출 시작(인접한 블럭을 제거)
        {
            //블록찾기
            BlockBase tempBlock = PosHelper.GetBlock(5, 4);
            float posX = tempBlock.transform.localPosition.x;
            float posY = tempBlock.transform.localPosition.y;

            //커스텀 블라인드 설정.
            blind.customBlind.FillColorAtCustomBlindTexture(listCostomBlindData, new Color(0f, 0f, 0f, 0.5f));
            List<CustomBlindData> listData = new List<CustomBlindData>(GetListCustomBlindDataByBlockPos(5, 7, 4, 4, 80, 87, offsetX: 1));
            blind.customBlind.FillColorAtCustomBlindTexture(listData, Color.clear);
            listCostomBlindData.AddRange(listData);

            //손가락 움직이기.
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            finger.transform.DOMove(tempBlock._transform.position, 0.2f);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m23_2"), 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

            yield return new WaitForSeconds(0.5f);
            blind.customBlind.SetTouchData(GetCustomBlindDataByBlockPos(5, 4, 80, 80), () => Pick.instance.OnPressAtTutorial());

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
            blind.customBlind.FillColorAtCustomBlindTexture(listCostomBlindData, Color.black);
            
            //손가락 제거.
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));

            //파랑새 모션
            ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
            ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

            yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_clear_S");

            ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

            while (ManagerBlock.instance.state != BlockManagrState.WAIT)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
        #endregion

        #region 세번째 연출 시작(매턴마다 온오프 전환)
        {
            DOTween.To((a) => blind.customBlind.SetAlphaCustomBlind(a), 0f, 0.8f, 0.5f);
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

            //커스텀 블라인드 설정
            blind.customBlind.FillColorAtCustomBlindTexture(GetListBlockPeaCustomData(true), Color.clear);
            blind.customBlind.FillColorAtCustomBlindTexture(GetListBlockPeaCustomData(false), Color.clear);

            //말풍선 생성.
            textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m23_3"), 0.3f);

            //사운드 출력.
            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

            yield return new WaitForSeconds(0.5f);
            while (Input.anyKeyDown == false)
            {
                yield return null;
            }

            //말풍선 제거.
            StartCoroutine(textBox.DestroyTextBox(0.3f));
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

    private List<BlockPea> GetListBlockPea(bool isGetActivePea)
    {
        List<BlockPea> listBlockPea = new List<BlockPea>();

        //블록찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.PEA)
            {
                BlockPea blockPea = tempBlock.Block as BlockPea;
                if (blockPea == null)
                    continue;
                if (isGetActivePea == blockPea.isActive)
                {
                    listBlockPea.Add(blockPea);
                }
            }
        }
        return listBlockPea;
    }

    private void RotateBlockPea()
    {
        listTweenSequence.Clear();
        for (int i = 0; i < listActivePea.Count; i++)
        {
            RotateObj(listActivePea[i].gameObject);
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

    private void StopRotateBlockPea()
    {
        for (int i = 0; i < listActivePea.Count; i++)
        {
            ResetRotationObj(listActivePea[i].gameObject);
        }
    }

    private void ResetRotationObj(GameObject obj)
    {
        obj.transform.rotation = Quaternion.identity;
    }

    private List<CustomBlindData> GetListCustomBlindDataByBlockPos(int x1, int x2, int y1, int y2, int sizeX, int sizeY, int offsetX = 0, int offsetY = 0)
    {
        BlockBase tempBlock_1 = PosHelper.GetBlock(x1, y1);
        BlockBase tempBlock_2 = PosHelper.GetBlock(x2, y2);

        if (tempBlock_1 == null || tempBlock_2 == null)
            return null;

        float centerPos_X = (tempBlock_1.transform.localPosition.x + tempBlock_2.transform.localPosition.x) * 0.5f;
        float centerPos_Y = (tempBlock_1.transform.localPosition.y + tempBlock_2.transform.localPosition.y) * 0.5f;
        Vector3 centerPos = new Vector3(centerPos_X + offsetX, centerPos_Y + offsetY, 0f);
        int touchSize_X = (x1 > x2) ? (x1 - x2 + 1) * sizeX : (x2 - x1 + 1) * sizeX;
        int touchSize_Y = (y1 > y2) ? (y1 - y2 + 1) * sizeY : (y2 - y1 + 1) * sizeY;

        List<CustomBlindData> listData = new List<CustomBlindData>();
        listData.Add(new CustomBlindData(centerPos, touchSize_X, touchSize_Y));
        return listData;
    }

    private List<CustomBlindData> GetCustomBlindDataByBlockPos(int x1, int y1, int sizeX, int sizeY, int offsetX = 0, int offsetY = 0)
    {
        BlockBase tempBlock_1 = PosHelper.GetBlock(x1, y1);
        if (tempBlock_1 == null)
            return null;

        List<CustomBlindData> listData = new List<CustomBlindData>();
        Vector3 centerPos = new Vector3(tempBlock_1.transform.localPosition.x + offsetX, tempBlock_1.transform.localPosition.y + offsetY, 0f);
        listData.Add(new CustomBlindData(centerPos, sizeX, sizeY));
        return listData;
    }

    private List<CustomBlindData> GetListBlockPeaCustomData(bool isGetActivePea)
    {
        List<CustomBlindData> listCostomBlindData = new List<CustomBlindData>();

        //블록찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.PEA)
            {
                BlockPea blockPea = tempBlock.Block as BlockPea;
                if (blockPea == null)
                    continue;
                if (isGetActivePea == blockPea.isActive)
                {
                    listCostomBlindData.AddRange(GetCustomBlindDataByBlockPos(tempBlock.Block.indexX, tempBlock.Block.indexY, 80, 83, offsetX: 2, offsetY: 3));
                }
            }
        }
        return listCostomBlindData;
    }
}
