using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json;

public class TutorialReadyItem : TutorialBase
{

    private void OnDisable()
    {
        int[] readyIndex = new int[4] { 0, 3, 4, 5 };

        for (int i = 0; i < readyIndex.Length; i++)
        {
            int itemCount = ServerRepos.UserItem.ReadyItem(readyIndex[i]);
            if (readyIndex[i] == 0)
                itemCount++;

            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    ((RewardType)((int)RewardType.readyItem1 + readyIndex[i])).ToString(),
                    "ReadyItem" + ((READY_ITEM_TYPE)readyIndex[i]).ToString(),
                    itemCount,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TUTORIAL,
                    "TutorialReadyItem"
                );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
        }
    }

    public GameObject GetReadyItemRoot()
    {
        return UIPopupReady._instance.objSelectItemRoot;
    }

    public GameObject GetObjReadyItem_AddTurn()
    {
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            if (UIPopupReady._instance.listReadyItem[i].type == READY_ITEM_TYPE.ADD_TURN)
                return UIPopupReady._instance.listReadyItem[i].gameObject;
        }
        return null;
    }

    public List<GameObject> GetListObjReadyItem_AddTurn()
    {
        List<GameObject> listObj = new List<GameObject>();
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            if (UIPopupReady._instance.listReadyItem[i].type == READY_ITEM_TYPE.ADD_TURN)
                listObj.Add(UIPopupReady._instance.listReadyItem[i].gameObject);
        }
        return listObj;
    }

    public void FlashReadyItem_JewelType()
    {
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            ReadyItem readyItem = UIPopupReady._instance.listReadyItem[i];
            if (readyItem.type == READY_ITEM_TYPE.LINE_BOMB || readyItem.type == READY_ITEM_TYPE.CIRCLE_BOMB
                || readyItem.type == READY_ITEM_TYPE.RAINBOW_BOMB)
            {
                StartCoroutine(UIPopupReady._instance.listReadyItem[i].CoFlashItem_Alpha(2, 1));
            }
        }
    }

    public GameObject GetObjStartButton()
    {
        return UIPopupReady._instance.GetButton();
    }

    public List<GameObject> GetListObjStartButton()
    {
        List<GameObject> listObj = new List<GameObject>() { UIPopupReady._instance.GetButton() };
        return listObj;
    }

    public bool IsSelectReadyItem()
    {
        for (int i = 0; i < UIPopupReady._instance.listReadyItem.Count; i++)
        {
            ReadyItem readyItem = UIPopupReady._instance.listReadyItem[i];
            if (readyItem.type == READY_ITEM_TYPE.ADD_TURN)
                return readyItem.IsSelectItem();
        }
        return false;
    }

    public bool IsClickStartButton()
    {
        return UIPopupReady._instance.bStartGame;
    }

    #region 예전 튜토리얼 코드
    //IEnumerator Start()
    //{        
    //    TextboxTutorial textBox = null;
    //    UITexture finger = ManagerTutorial._instance._spriteFinger;
    //    BlindTutorial blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
    //    Vector3 targetPos = UIPopupReady._instance.listReadyItem[0].transform.position;

    //    BirdPositionType birdType = BirdPositionType.none;


    //    #region 첫번째 연출 시작 (잠긴 아이템이 열렸어)
    //    {
    //        //블라인드 설정.
    //        blind.transform.position = targetPos;
    //        blind._panel.depth = UIPopupReady._instance.uiPanel.depth + 3;
    //        blind.transform.parent = UIPopupReady._instance.transform;
    //        blind.SetSize(0, 0);
    //        blind.SetSizeCollider(0, 0);

    //        //콜라이더만 깔아놓고 일정 시간 후에 튜토리얼 시작.
    //        yield return new WaitForSeconds(1.0f);

    //        //파랑새 생성.
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
    //        }
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");

    //        yield return new WaitForSeconds(0.3f);
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);
    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m7_1"), 0.3f);
    //        yield return new WaitForSeconds(0.5f);

    //        while (Input.anyKeyDown == false)
    //        {
    //            yield return null;
    //        }

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.2f));
    //        yield return null;
    //    }
    //    #endregion 첫번째 연출 끝 (잠긴 아이템이 열렸어)

    //    #region 두번째 연출 시작 (아이템을 준비해봐)
    //    {
    //        bool bTurn = false;

    //        //파랑새 생성.
    //        if (birdType != BirdPositionType.TopRight)
    //        {
    //            birdType = BirdPositionType.TopRight;
    //            bTurn = ManagerTutorial._instance.SettingBlueBird(birdType, 0.6f);
    //        }
    //        yield return new WaitForSeconds(0.6f);

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
    //        //ManagerTutorial._instance.SetBirdAnimation("T_idle_Do");

    //        //사운드 출력.
    //        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

    //        //말풍선 생성.
    //        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_l_m7_2"), 0.3f);

    //        //손가락 생성.
    //        finger.color = new Color(1f, 1f, 1f, 0f);
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
    //        finger.transform.DOMove(targetPos, 0.3f);

    //        yield return new WaitForSeconds(0.3f);

    //        bool bPush = false;

    //        //터치 영역 설정.
    //        blind.SetSizeCollider(250, 250);

    //        float fTimer = 0.0f;
    //        while (UIPopupReady.readyItemSelectCount[0] == 0)
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
    //        yield return null;
    //    }
    //    #endregion 두번째 연출 끝 (아이템을 준비해봐)

    //    #region 마지막 연출 시작 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    //    {
    //        //블라인드 설정.
    //        blind.SetSizeCollider(0, 0);
    //        blind.transform.parent = ManagerUI._instance.transform;

    //        //말풍선 제거.
    //        StartCoroutine(textBox.DestroyTextBox(0.2f));
    //        //손가락 제거.
    //        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

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
    //        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.4f));
    //        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop");
    //        yield return new WaitForSeconds(0.5f);

    //        Destroy(blind.gameObject);
    //        Destroy(ManagerTutorial._instance.gameObject);
    //    }
    //    #endregion 마지막 연출 끝 (파랑새 나가기, 튜토리얼 오브젝트 삭제)
    //}
    #endregion
}
