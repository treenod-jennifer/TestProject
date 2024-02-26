using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialInGameItem : TutorialBase
{
    public List<CustomBlindData> GetCustomBlindData_NormalBlock()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        //블록찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock != null && tempBlock.Block != null && tempBlock.Block.type == BlockType.NORMAL 
                && tempBlock.Block.bombType == BlockBombType.NONE)
            {
                CustomBlindData data = new CustomBlindData(tempBlock.Block.transform.localPosition, 85, 75);
                listCustomBlindData.Add(data);
            }
        }
        return listCustomBlindData;
    }

    public void Touch_InGameItemHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.HAMMER)
                Touch_UIPokoButton(GameUIManager.instance.listGameItem[i].gameObject);
        }
    }

    public GameObject GeObj_IngameItem_Hammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.HAMMER)
                return GameUIManager.instance.listGameItem[i].gameObject;
        }
        return null;
    }

    public List<GameObject> GeObjList_IngameItem_Hammer()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.HAMMER)
                listObj.Add(GameUIManager.instance.listGameItem[i].gameObject);
        }
        return listObj;
    }

    public bool IsOpenItemUI()
    {
        if (GameItemManager.instance == null)
            return false;
        else
            return true;
    }

    public bool IsUsedItem()
    {
        if (GameItemManager.instance == null || GameItemManager.instance.used == true)
            return true;
        else
            return false;
    }

    #region 이전 튜토리얼
    //IEnumerator Start()
    //{
    //    float timer = 0;
    //    TextboxTutorial textBox = null;
    //    UITexture finger = ManagerTutorial._instance._spriteFinger;
    //    Vector3 targetPos = Vector3.zero;
    //    BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
    //    BirdPositionType birdType = BirdPositionType.none;


    //    #region 첫번째 연출 시작
    //    {
    //        foreach (var item in GameUIManager.instance.listGameItem)
    //        {
    //            if (item.type == GameItemType.HAMMER)
    //            {
    //                targetPos = item.gameObject.transform.position;
    //                break;
    //            }
    //        }

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

    //        //망치
    //        GameObject HammerObj = null;
    //        foreach (var item in GameUIManager.instance.listGameItem)
    //        {
    //            if(item.type == GameItemType.HAMMER)
    //            {
    //                HammerObj = NGUITools.AddChild(blind._panel.gameObject, item.gameObject);
    //                HammerObj.transform.position = item.gameObject.transform.position;
    //                BoxCollider collider = HammerObj.GetComponent<BoxCollider>();
    //                Destroy(collider);
    //                break;
    //            }
    //        }

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m7_1"), 0.3f);       //새로운 아이템이 생겼어!         자, 해머를 사용해볼까마유?

    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove(HammerObj.transform.position, 0.3f);
    //        blind.SetSizeCollider(80, 80);

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }


    //        bool bPush = false;
    //        float fTimer = 0.0f;
    //        while (GameItemManager.instance == null)
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

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
    //        blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        Destroy(HammerObj);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //    }

    //    #endregion 첫번째 연출 끝

    //    #region 두번째 연출시작 
    //    {
    //        timer = 0;
    //        while (timer < 1f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        //콜라이더 재정렬
    //        blind.transform.position = ManagerBlock.boards[5, 7].Block._transform.position;


    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m7_2"), 0.3f);        //그 다음엔 블록을 터치해봐마유.

    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove((ManagerBlock.boards[5, 7].Block.transform.position*0.8f + 0.2f* ManagerBlock.boards[5, 8].Block.transform.position), 0.3f);
    //        blind.SetSizeCollider(78, 78);

    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        bool bPush = false;
    //        float fTimer = 0.0f;
    //        while (GameItemManager.instance != null && GameItemManager.instance.used == false)
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

    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);
    //        blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝

    //    #region 세번째 연출 시작 
    //    {

    //        while (GameItemManager.instance != null)
    //        {
    //            yield return null;
    //        }

    //        timer = 0;
    //        while (timer < 1f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }


    //        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

    //        bool bTurn = false;
    //        if (birdType != BirdPositionType.BottomRight)
    //        {
    //            birdType = BirdPositionType.BottomRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 1.0f);
    //        }
    //        yield return null;

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);//사운드 출력.
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");       
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


    //        /*
    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        */


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
    //    }
    //    #endregion 세번째 연출 끝 

    //    #region 네번째 연출 시작
    //    {

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Up");//파랑새 설정.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);//사운드 출력.

    //        GameObject turnObject = NGUITools.AddChild(blind._panel.gameObject, GameUIManager.instance.turnUi);
    //        turnObject.transform.position = GameUIManager.instance.turnUi.transform.position;


    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m7_3"), 0.3f);    //짠! 해머를 사용하면 턴을 소모하지 않고 블록과 석판을 제거할 수 있어.

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

    //        timer = 0;
    //        while (timer < 0.5f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        blind.SetAlpha(0);
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m7_4"), 0.3f);    

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

    //        StartCoroutine(textBox.DestroyTextBox(0.3f));
    //        yield return null;
    //    }
    //    #endregion 네번째 연출 끝 

    //    #region 다섯번째 연출 시작
    //    {

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);//사운드 출력.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType,  Global._instance.GetString("t_g_m7_5"), 0.3f);        //엄청 유용하지마유?            앞으로 해머를 써서 쉽게 플레이해보자!

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
    //       // DOTween.To((a) => blind.SetAlpha(a), 0.5f, 0f, 0.3f);
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
