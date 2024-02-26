using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialLava : TutorialBase
{
    public List<CustomBlindData> GetListCustomBlindData_LavaArea()
    {
        List<CustomBlindData> listData = new List<CustomBlindData>();
        //용암 영역 데이터 입력
        int x1 = 1;
        int x2 = 10;
        int y1 = 10;
        int y2 = 13;

        Vector3 startPos = PosHelper.GetPosByIndex(x1, y1);
        Vector3 endPos = PosHelper.GetPosByIndex(x2, y2);

        float centerPos_X = (startPos.x + endPos.x) * 0.5f;
        float centerPos_Y = (startPos.y + endPos.y) * 0.5f;
        Vector3 centerPos = new Vector3(centerPos_X, centerPos_Y, 0f);

        int touchSize_X = (x2 - x1 + 1) * 87;
        int touchSize_Y = (y2 - y1 + 1) * 87;

        listData.Add(new CustomBlindData(centerPos, touchSize_X, touchSize_Y));
        return listData;
    }

    public List<CustomBlindData> GetListCustomBlindData_LavaArea_UP()
    {
        List<CustomBlindData> listData = new List<CustomBlindData>();
        //용암 영역 데이터 입력
        int x1 = 5;
        int x2 = 5;
        int y1 = 8;
        int y2 = 9;

        Vector3 startPos = PosHelper.GetPosByIndex(x1, y1);
        Vector3 endPos = PosHelper.GetPosByIndex(x2, y2);

        float centerPos_X = (startPos.x + endPos.x) * 0.5f;
        float centerPos_Y = (startPos.y + endPos.y) * 0.5f;
        Vector3 centerPos = new Vector3(centerPos_X, centerPos_Y, 0f);

        int touchSize_X = (x2 - x1 + 1) * 100;
        int touchSize_Y = (y2 - y1 + 1) * 95;

        listData.Add(new CustomBlindData(centerPos, touchSize_X, touchSize_Y));
        return listData;
    }

    public List<DecoBase> GetListLava()
    {
        List<DecoBase> listLava = new List<DecoBase>();

        //용암 찾기
        for (int i = 0; i < 2; i++)
        {
            Board tempBoard = ManagerBlock.boards[5, 8 + i];
            if (tempBoard != null && tempBoard.DecoOnBoard != null)
            {
                if (tempBoard.lava != null)
                    listLava.Add(tempBoard.lava);
            }
        }
        return listLava;
    }

    #region 예전 튜토리얼 코드
    //IEnumerator Start()
    //{
    //    float timer = 0;
    //    TextboxTutorial textBox = null;
    //    UITexture finger = ManagerTutorial._instance._spriteFinger;
    //    Vector3 targetPos = Vector3.zero;
    //    BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
    //    BirdPositionType birdType = BirdPositionType.none;

    //    UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
    //    if (tutorialRoot != null)
    //    {
    //        tutorialRoot.manualWidth = 750;
    //    }


    //    #region 첫번째 연출 시작
    //    {
    //        targetPos = (ManagerBlock.boards[5, 8].Block.transform.position + ManagerBlock.boards[5, 9].Block.transform.position) * 0.5f;

    //        //블라인드 설정.
    //        blind.transform.position = targetPos;
    //        blind.transform.localPosition += Vector3.zero;
    //        blind._panel.depth = 10;
    //        blind.SetSizeCollider(0, 0);
    //        blind.SetDepth(0);
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
    //        timer = 0;

    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //파랑새 등장 설명해준다고 얘기시작
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //용암보이게
    //        List<GameObject> tempLavaObject = new List<GameObject>();
    //        foreach (var tempLava in ManagerBlock.instance.listLava)
    //        {
    //            GameObject lavaA = NGUITools.AddChild(blind._panel.gameObject, tempLava.gameObject);
    //            lavaA.transform.position = tempLava.transform.position;
    //            lavaA.transform.localScale = Vector3.one * 1.08f;
    //            tempLavaObject.Add(lavaA.gameObject);
    //        }

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m6_1"), 0.3f);        //앗, 블록 아래에 용암이 들끓고 있어마유!

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        yield return null;

    //        foreach (var tempObj in tempLavaObject) Destroy(tempObj);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //    }
    //    #endregion 첫번째 연출 끝

    //    #region 두번째 연출시작 
    //    {
    //        //파랑새 위로 가고 폭탄생성
    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
    //        }
    //        timer = 0;
    //        while (timer < 0.6f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

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

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //블럭이미지복사
    //        List<NormalBlock> listBlock = new List<NormalBlock>();
    //        listBlock.Add(ManagerBlock.boards[5, 8].Block as NormalBlock);
    //        listBlock.Add(ManagerBlock.boards[5, 9].Block as NormalBlock);

    //        List<GameObject> tempObject = new List<GameObject>();

    //        foreach (var tempBlock in listBlock)
    //        {
    //            NormalBlock blockA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.gameObject).GetComponent<NormalBlock>();
    //            blockA._transform.position = tempBlock._transform.position;
    //            tempObject.Add(blockA.gameObject);

    //            if (tempBlock.rightLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.rightLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);

    //                //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.rightLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
    //                //LinkerB.transform.position = tempBlock.rightLinker.linkerSpriteB_FR.transform.position;
    //                //tempObject.Add(LinkerB.gameObject);
    //            }

    //            if (tempBlock.DownLinker != null)
    //            {
    //                UISprite LinkerA = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteFR.gameObject).GetComponent<UISprite>();
    //                LinkerA.transform.position = tempBlock.DownLinker.linkerSpriteFR.transform.position;
    //                tempObject.Add(LinkerA.gameObject);

    //                //UISprite LinkerB = NGUITools.AddChild(blind._panel.gameObject, tempBlock.DownLinker.linkerSpriteB_FR.gameObject).GetComponent<UISprite>();
    //                //LinkerB.transform.position = tempBlock.DownLinker.linkerSpriteB_FR.transform.position;
    //                //tempObject.Add(LinkerB.gameObject);
    //            }
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m6_2"), 0.3f);        //우선 목표에 맞춰서 블록을 없애보자마유.
    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove(ManagerBlock.boards[5, 9].Block.transform.position, 0.3f);
    //        blind.SetSizeCollider(78 * 2, 78);

    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        bool bPush = false;
    //        float fTimer = 0.0f;
    //        while (ManagerBlock.instance.state != BlockManagrState.MOVE)
    //        {
    //            //기다리는 동안 손가락 이미지 전환.
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

    //        blind.SetAlpha(0);
    //        foreach (var tempObj in tempObject) Destroy(tempObj);
    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝

    //    #region 세번째 연출 시작 
    //    {
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);         //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));            
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);       //손가락 제거.

    //        //블라인드 설정.
    //        blind.SetSize(0, 0);
    //        blind.SetSizeCollider(0, 0);

    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        yield return null;
    //    }
    //    #endregion 세번째 연출 끝 

    //    #region 네번째 연출 시작
    //    {
    //        while (ManagerBlock.instance.state != BlockManagrState.WAIT)
    //        {
    //            yield return null;
    //        }

    //        timer = 0;
    //        while (timer < 0.8f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //목표화면으로 이동
    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);
    //        blind.SetDepth(-1);
    //        //blind.SetSize(360, 720);
    //        blind.transform.position = targetPos;

    //        //파랑새 설정.
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //용암보이게
    //        List<GameObject> tempLavaObject = new List<GameObject>();
    //        foreach (var tempLava in ManagerBlock.instance.listLava)
    //        {
    //            if(tempLava.inY == 9)
    //            {
    //                GameObject lavaA = NGUITools.AddChild(blind._panel.gameObject, tempLava.gameObject);
    //                lavaA.transform.position = tempLava.transform.position;
    //                lavaA.transform.localScale = Vector3.one * 1.08f;
    //                tempLavaObject.Add(lavaA.gameObject);
    //            }
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m6_3"), 0.3f);    //자, 한 턴이 지나니까 용암 2개가 올라왔지?

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        yield return null;
    //        foreach (var tempObj in tempLavaObject) Destroy(tempObj);
    //        yield return null;
    //    }
    //    #endregion 네번째 연출 끝 

    //    #region 다섯번째 연출 시작
    //    {
    //        blind.SetSizeCollider(0, 0);

    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.BottomRight)
    //        {
    //            birdType = BirdPositionType.BottomRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
    //        }

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }

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
    //        yield return null;
    //    }
    //    #endregion 다섯번째 연출 끝 

    //    #region 여섯번째 연출 시작
    //    {
    //        GameObject turnObject = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.turnUi);
    //        turnObject.transform.position = GameUIManager.instance.turnUi.transform.position;

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);//사운드 출력.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m6_4"), 0.3f);        //팻말에 적현 2는 바로 다음 턴에 올라올 용암의 개수야!

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }
    //        Destroy(turnObject);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //        yield return null;
    //    }
    //    #endregion  여섯번째 연출 끝




    //    #region 다섯번째 연출 시작
    //    {

    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
    //        }
    //        yield return null;

    //        while (true)
    //        {
    //            if (ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("T_clear_S") &&
    //                 ManagerTutorial._instance.birdLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    //            {
    //                break;
    //            }
    //            yield return null;
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        yield return null;

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
    //        yield return null;


    //        List<GameObject> tempLavaObject = new List<GameObject>();
    //        foreach (var tempLava in ManagerBlock.instance.listLava)
    //        {
    //            GameObject lavaA = NGUITools.AddChild(blind._panel.gameObject, tempLava.gameObject);
    //            lavaA.transform.position = tempLava.transform.position;
    //            lavaA.transform.localScale = Vector3.one * 1.08f;
    //            tempLavaObject.Add(lavaA.gameObject);
    //        }

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");
    //        yield return null;

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);//사운드 출력.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m6_5"), 0.3f);        //이 용암은 없앨 수 없어마유.    목표물이 다 덮히기 전에 서두르자!
    //                    timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        foreach (var tempObj in tempLavaObject) Destroy(tempObj);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //        yield return null;
    //    }
    //    #endregion  여섯번째 연출 끝



    //    #region 마지막 연출 시작
    //    {

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
    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //블라인드 알파.
    //        DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
    //        //오브젝트 제거.
    //        timer = 0;
    //        while (timer < 0.4f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        Destroy(blind.gameObject);
    //        Destroy(ManagerTutorial._instance.gameObject);
    //    }
    //    #endregion 마지막 연출 끝
    //}
    #endregion
}
