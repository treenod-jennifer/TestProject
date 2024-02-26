using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialBigJewel : TutorialBase
{
    #region 기존 튜토리얼 코드
//    private IEnumerator Start()
//    {
//        TextboxTutorial textBox = null;
//        UITexture finger = ManagerTutorial._instance._spriteFinger;
//        BirdPositionType birdType = BirdPositionType.none;

//        //블라인드 생성
//        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
//        blind._panel.depth = 30;
//        blind.SetDepth(0);
//        blind.SetSize(0, 0);
//        blind.SetSizeCollider(0, 0);
//        blind.transform.localPosition = Vector3.zero;
//        blind.transform.parent = ManagerUI._instance.transform;

//        //마유지 생성
//        if (birdType != BirdPositionType.BottomLeft)
//        {
//            birdType = BirdPositionType.BottomLeft;
//            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
//        }
//        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

//        yield return new WaitForSeconds(0.5f);

//        //사운드 출력.
//        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

//        //말풍선 생성.
//        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m18_1"), 0.3f);
//        yield return new WaitForSeconds(0.3f);
//        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

//        //입력을 받을때 까지 대기
//        yield return new WaitUntil(() => Input.anyKeyDown);

//        //말풍선 제거.
//        StartCoroutine(textBox.DestroyTextBox(0.3f));
//        yield return new WaitForSeconds(0.2f);

//        //BigJewel과 인접한 터트릴 수 있는 블럭을 찾는다.
//        BlockBase bigJewel = null;
//        Transform pangBlock = null;
//        GetBigJewelAndPangBlock(ref bigJewel, ref pangBlock);

//#if UNITY_EDITOR
//        if (bigJewel == null)
//            Debug.LogError("BigJewel을 찾을 수 없습니다.");
//        if (pangBlock == null)
//            Debug.LogError("BigJewel에 인접한 퍼트릴 수 있는 NormalBlock을 찾을 수 없습니다.");
//#endif

//        //블라인드 생성
//        blind.SetSize(430, 430);
//        blind.SetSizeCollider(0, 0);
//        UITexture blindCenter = blind._textureCenter.GetComponent<UITexture>();
//        blindCenter.type = UIBasicSprite.Type.Sliced;
//        blindCenter.border = Vector4.one * 50;
//        blind.transform.position = bigJewel.transform.position;
//        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
//        yield return new WaitForSeconds(0.2f);

//        //사운드 출력.
//        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

//        //말풍선 생성.
//        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m18_2"), 0.3f);
//        yield return new WaitForSeconds(0.3f);

//        //입력을 받을때 까지 대기
//        yield return new WaitUntil(() => Input.anyKeyDown);

//        //말풍선 제거.
//        StartCoroutine(textBox.DestroyTextBox(0.3f));
//        yield return new WaitForSeconds(0.2f);

//        //블라인드 제거
//        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
//        yield return new WaitForSeconds(0.2f);

//        //파랑새 애니메이션 & 이동.
//        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

//        bool bTurn = false;
//        if (birdType != BirdPositionType.BottomRight)
//        {
//            birdType = BirdPositionType.BottomRight;
//            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
//        }
//        yield return new WaitForSeconds(1.0f);

//        // 파랑새가 돌아야 할 경우, 애니메이션 재생 & 파랑새 도는거 기다려야함.
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

//        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");
//        yield return new WaitForSeconds(0.5f);

//        //사운드 출력.
//        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

//        //말풍선 생성.
//        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m18_3"), 0.3f);
//        yield return new WaitForSeconds(0.3f);

//        //손가락 생성
//        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
//        finger.color = new Color(finger.color.r, finger.color.g, finger.color.b, 0.0f);
//        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
//        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
//        finger.transform.DOMove(pangBlock.position, 0.3f);
//        yield return new WaitForSeconds(0.3f);

//        //pangBlock들만 입력이 가능하도록 세팅
//        blind.transform.position = pangBlock.position;
//        blind.SetSizeCollider(78, 78);

//        //블럭이 터질때 까지 대기(기다리는 동안 손가락 이미지 전환)
//        bool bPush = false;
//        float fTimer = 0.0f;

//        while (pangBlock != null)
//        {
//            fTimer += Global.deltaTime * 1.0f;
//            if (fTimer >= 0.15f)
//            {
//                if (bPush == true)
//                {
//                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
//                    bPush = false;
//                }
//                else
//                {
//                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
//                    bPush = true;
//                }
//                fTimer = 0.0f;
//            }
//            yield return null;
//        }

//        //손가락 제거.
//        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
//        blind.SetSizeCollider(0, 0);

//        //말풍선 제거.
//        StartCoroutine(textBox.DestroyTextBox(0.3f));
//        yield return new WaitForSeconds(0.5f);

//        //사운드 출력.
//        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

//        //말풍선 생성.
//        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m18_4"), 0.3f);
//        yield return new WaitForSeconds(0.3f);

//        //입력을 받을때 까지 대기
//        yield return new WaitUntil(() => Input.anyKeyDown);

//        //말풍선 제거.
//        StartCoroutine(textBox.DestroyTextBox(0.3f));
//        yield return new WaitForSeconds(0.2f);

//        //파랑새 나가는 연출.
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
//        yield return new WaitForSeconds(0.3f);

//        //튜토리얼 종료
//        Destroy(blind.gameObject);
//        Destroy(ManagerTutorial._instance.gameObject);
//    }

//    private void GetBigJewelAndPangBlock(ref BlockBase bigJewel, ref Transform pangBlock)
//    {
//        //int xSize = ManagerBlock.boards.GetUpperBound(0);
//        //int ySize = ManagerBlock.boards.GetUpperBound(1);
//        //Debug.Log("xSize : " + xSize + " / ySize : " + ySize);

//        //////////////////////////////////////////////////////////

//        List<BlockBase> bigJewelList = new List<BlockBase>();

//        foreach (var board in ManagerBlock.boards)
//        {
//            if (board.Block == null)
//                continue;

//            if (board.Block.type != BlockType.BigJewel && board.Block.type != BlockType.ColorBigJewel)
//                continue;

//            if (!bigJewelList.Contains(board.Block))
//                bigJewelList.Add(board.Block);
//        }

//        foreach (var jewel in bigJewelList)
//        {
//            Transform pang = GetPangBlock(jewel);
//            if (pang != null)
//            {
//                bigJewel = jewel;
//                pangBlock = pang;
//                return;
//            }
//        }
//    }

//    private Transform GetPangBlock(BlockBase bigJewel)
//    {
//        List<NormalBlock> AdjacentBlockList = new List<NormalBlock>();

//        BlockBase left_1 = PosHelper.GetBlock(bigJewel.indexX - 1, bigJewel.indexY);
//        BlockBase left_2 = PosHelper.GetBlock(bigJewel.indexX - 1, bigJewel.indexY + 1);
//        BlockBase right_1 = PosHelper.GetBlock(bigJewel.indexX + 2, bigJewel.indexY);
//        BlockBase right_2 = PosHelper.GetBlock(bigJewel.indexX + 2, bigJewel.indexY + 1);
//        BlockBase up_1 = PosHelper.GetBlock(bigJewel.indexX, bigJewel.indexY - 1);
//        BlockBase up_2 = PosHelper.GetBlock(bigJewel.indexX + 1, bigJewel.indexY - 1);
//        BlockBase down_1 = PosHelper.GetBlock(bigJewel.indexX, bigJewel.indexY + 2);
//        BlockBase down_2 = PosHelper.GetBlock(bigJewel.indexX + 1, bigJewel.indexY + 2);

//        if (left_1 != null && left_1.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(left_1 as NormalBlock);
//        if (left_2 != null && left_2.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(left_2 as NormalBlock);
//        if (right_1 != null && right_1.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(right_1 as NormalBlock);
//        if (right_2 != null && right_2.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(right_2 as NormalBlock);
//        if (up_1 != null && up_1.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(up_1 as NormalBlock);
//        if (up_2 != null && up_2.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(up_2 as NormalBlock);
//        if (down_1 != null && down_1.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(down_1 as NormalBlock);
//        if (down_2 != null && down_2.type == BlockType.NORMAL)
//            AdjacentBlockList.Add(down_2 as NormalBlock);

//        var pangBlock = AdjacentBlockList.Find((NormalBlock block) =>
//        {
//            return block.upLinker != null ||
//                    block.DownLinker != null ||
//                    block.rightLinker != null ||
//                    block.leftLinker != null;
//        });

//        if (pangBlock == null)
//            return null;
//        else
//            return pangBlock.transform;

//        //Debug.Log(pangBlock.name, pangBlock);

//        //Debug.Log("left_1", left_1);
//        //Debug.Log("left_2", left_2);
//        //Debug.Log("right_1", right_1);
//        //Debug.Log("right_2", right_2);
//        //Debug.Log("up_1", up_1);
//        //Debug.Log("up_2", up_2);
//        //Debug.Log("down_1", down_1);
//        //Debug.Log("down_2", down_2);
//    }
    #endregion
}
