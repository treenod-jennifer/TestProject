using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialPowerHammer : TutorialBase
{
    public GameObject GeObj_PowerHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.THREE_HAMMER)
                return GameUIManager.instance.listGameItem[i].gameObject;
        }
        return null;
    }

    public List<GameObject> GeObjList_IngameItem_PowerHammer()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.THREE_HAMMER)
                listObj.Add(GameUIManager.instance.listGameItem[i].gameObject);
        }
        return listObj;
    }

    public List<BlockBase> GeBlockList_NormaBlock()
    {
        List<BlockBase> listBlock = new List<BlockBase>();

        //일반 블럭 찾기
        foreach (var tempBlock in ManagerBlock.boards)
        {
            if (tempBlock == null || tempBlock.HasDecoCoverBlock() == true)
                continue;

            BlockBase block = tempBlock.Block;
            if (block != null && block.IsNormalBlock() && block.type == BlockType.NORMAL)
            {
                listBlock.Add(block);
            }
        }
        return listBlock;
    }

    public void Touch_PowerHammer()
    {
        for (int i = 0; i < GameUIManager.instance.listGameItem.Count; i++)
        {
            if (GameUIManager.instance.listGameItem[i].type == GameItemType.THREE_HAMMER)
                Touch_UIPokoButton(GameUIManager.instance.listGameItem[i].gameObject);
        }
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

    #region 예전 튜토리얼 코드
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
    //            if (item.type == GameItemType.THREE_HAMMER)
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
    //        if (birdType != BirdPositionType.TopLeft)
    //        {
    //            birdType = BirdPositionType.TopLeft;
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
    //            if (item.type == GameItemType.THREE_HAMMER)
    //            {
    //                HammerObj = NGUITools.AddChild(blind._panel.gameObject, item.gameObject);
    //                HammerObj.transform.position = item.gameObject.transform.position;
    //                BoxCollider collider = HammerObj.GetComponent<BoxCollider>();
    //                Destroy(collider);
    //                break;
    //            }
    //        }

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m14_1"), 0.3f);      


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

    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        //blind.SetAlpha(0);
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

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m14_2"), 0.3f);        //그 다음엔 블록을 터치해봐마유.


    //        timer = 0;
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        //blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝


    //    #region 두번째 연출시작
    //    {
    //        timer = 0;
    //        while (timer < 1f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_happy);
    //        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m14_3"), 0.3f);        //그 다음엔 블록을 터치해봐마유.

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
    //        while (timer < 0.3f)
    //        {
    //            timer += Global.deltaTimePuzzle;
    //            yield return null;
    //        }
    //        */


    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        //blind.SetAlpha(0);
    //        blind.SetSizeCollider(0, 0);
    //        StartCoroutine(textBox.DestroyTextBox(0.3f));

    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝


    //    /*
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
    //        if (birdType != BirdPositionType.TopLeft)
    //        {
    //            birdType = BirdPositionType.TopLeft;
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


    //    #region 다섯번째 연출 시작
    //    {

    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);//사운드 출력.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_g_m14_3"), 0.3f);        //엄청 유용하지마유?            앞으로 해머를 써서 쉽게 플레이해보자!

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
    //    #endregion  여섯번째 연출 끝

    //     */


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
