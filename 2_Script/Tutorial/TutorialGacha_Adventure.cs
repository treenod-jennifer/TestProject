using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGacha_Adventure : TutorialBase
{
    private int lastGetAnimalIndex;

    private IEnumerator Start()
    {
        TextboxTutorial textBox = null;
        UITexture finger = ManagerTutorial._instance._spriteFinger;
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerUI._instance.transform);
        BirdPositionType birdType = BirdPositionType.none;

        #region #1 동물 리스트 팝업에서 튜토리얼
        
        //블라인드 생성
        blind._panel.depth = 30;
        blind.SetDepth(0);
        blind.SetSize(0, 0);
        blind.SetSizeCollider(0, 0);
        blind.transform.localPosition = Vector3.zero;
        blind.transform.parent = ManagerUI._instance.transform;
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //기본 동물들 블라인드 앞으로
        SetFront_Animals();

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
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m8_1"), 0.3f);
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

        //기본 동물들 블라인드 뒤로
        SetBack_Animals();

        //가챠 버튼 블라인드 앞으로
        SetFront_GachaButton();
        gachaButton.GetComponent<BoxCollider>().enabled = false;

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
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m8_2"), 0.3f);
        yield return new WaitForSeconds(0.3f);

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m8_3"), 0.3f);

        //손가락 생성
        finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
        finger.transform.position = ManagerTutorial._instance.birdLive2D.transform.position;
        finger.transform.DOMove(Vector3.down * 445 * finger.transform.lossyScale.x, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //가챠버튼 활성화(클릭 되도록)
        gachaButton.GetComponent<BoxCollider>().enabled = true;

        //팝업이 열릴때 까지 대기(기다리는 동안 손가락 이미지 전환)
        bool bPush = false;
        float fTimer = 0.0f;

        while (UIPopupAdventureSummonAction._instance == null)
        {
            fTimer += Global.deltaTime * 1.0f;
            if (fTimer >= 0.15f)
            {
                if (bPush == true)
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = false;
                }
                else
                {
                    finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = true;
                }
                fTimer = 0.0f;
            }
            yield return null;
        }

        //소환창 열림 튜토리얼 잠시 비활성화

        //가챠버튼 블라인드 뒤로
        SetBack_GachaButton();

        //손가락 제거.
        DOTween.ToAlpha(() => finger.color, x => finger.color = x, 0f, 0.2f);

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //파랑새 나가는 연출.
        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

        ManagerTutorial._instance.BlueBirdTurn();

        //파랑새 돌고 난 후 나감.
        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f, false));
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        yield return new WaitForSeconds(0.3f);
        birdType = BirdPositionType.none;

        //블라인드 비활성화
        DOTween.To((a) => blind.SetAlpha(a), 0.8f, 0.0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        blind.SetSizeCollider(5000, 5000);

        //가챠 팝업이 사라질때 까지 대기
        UIPopupAdventureSummonAction._instance.closeEvent += (int idx) => lastGetAnimalIndex = idx;
        while (UIPopupAdventureSummonAction._instance != null)
        {
            yield return null;
        }

        //블라인드 활성화
        blind.SetSizeCollider(0, 0);
        DOTween.To((a) => blind.SetAlpha(a), 0f, 0.8f, 0.5f);

        //새로 얻은 동물 블라인드 앞으로
        yield return SetFront_NewAnimal();

        //마유지 생성
        if (birdType != BirdPositionType.TopRight)
        {
            birdType = BirdPositionType.TopRight;
            ManagerTutorial._instance.SettingBlueBird(birdType, 0.3f);
        }
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);

        yield return new WaitForSeconds(0.5f);

        //사운드 출력.
        ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m8_4"), 0.3f);
        yield return new WaitForSeconds(0.3f);
        ManagerTutorial._instance.SetBirdAnimation("T_idle_Do", "T_targetDo_loop");

        //입력을 받을때 까지 대기
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //새로 얻은 동물 블라인드 뒤로
        SetBack_NewAnimal();

        //말풍선 제거.
        StartCoroutine(textBox.DestroyTextBox(0.3f));
        yield return new WaitForSeconds(0.2f);

        //파랑새 애니메이션 & 이동.
        ManagerTutorial._instance.SetBirdAnimation("T_clear_S");

        bTurn = false;
        if (birdType != BirdPositionType.TopLeft)
        {
            birdType = BirdPositionType.TopLeft;
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
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham);

        //말풍선 생성.
        textBox = ManagerTutorial._instance.MakeTextbox(birdType, Global._instance.GetString("t_ad_m8_5"), 0.3f);

        //가챠 버튼 블라인트 앞으로
        SetFront_GachaButton();
        gachaButton.GetComponent<BoxCollider>().enabled = false;

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

        //가챠 버튼 블라인트 뒤로
        gachaButton.GetComponent<BoxCollider>().enabled = true;
        SetBack_GachaButton();

        #endregion

        #region #2 튜토리얼 종료

        Destroy(blind.gameObject);
        Destroy(ManagerTutorial._instance.gameObject);

        #endregion
    }

    private GameObject animalsBox;
    private void SetFront_Animals()
    {
        var animals = UIPopupStageAdventureAnimal._instance.GetComponentsInChildren<UIItemAdventureAnimalInfo>();

        for(int i=0; i< animals.Length; i++)
        {
            if (animals[i].AnimalIdx == ManagerAdventure.User.GetAnimalIdxFromDeck(1, 0))
            {
                Transform originAnimalsBox = animals[i].transform.parent;

                animalsBox = Instantiate(originAnimalsBox.gameObject);
                animalsBox.transform.SetParent(blind.transform);
                animalsBox.transform.localScale = Vector3.one;
                animalsBox.transform.position = originAnimalsBox.transform.position;

                var boxs = animalsBox.GetComponentsInChildren<BoxCollider>();
                foreach(var box in boxs)
                {
                    box.enabled = false;
                }

                var tabExplanation = animalsBox.GetComponentInChildren<UIItemAdventureAnimalTabExplanation>();
                tabExplanation.gameObject.SetActive(false);

                return;
            }
        }
    }
    private void SetBack_Animals()
    {
        Destroy(animalsBox);
        animalsBox = null;
    }

    private GameObject gachaButton;
    private Transform originParent_gachaButton;
    private void SetFront_GachaButton()
    {
        gachaButton = UIPopupStageAdventureAnimal._instance.GetSummonsButton();
        originParent_gachaButton = gachaButton.transform.parent;

        gachaButton.transform.SetParent(blind.transform);
        gachaButton.SetActive(false);
        gachaButton.SetActive(true);
    }
    private void SetBack_GachaButton()
    {
        gachaButton.transform.SetParent(originParent_gachaButton);
        gachaButton.transform.SetSiblingIndex(0);
        gachaButton.SetActive(false);
        gachaButton.SetActive(true);

        originParent_gachaButton = null;
        gachaButton = null;
    }

    private GameObject newAnimal;
    private IEnumerator SetFront_NewAnimal()
    {
        var animalList = ManagerAdventure.User.GetAnimalList(false);
        for (int i = 2; i >= 0; --i)
        {
            var aId = ManagerAdventure.User.GetAnimalIdxFromDeck(1, i);
            animalList.RemoveAll(animal => animal.idx == aId);
        }

        for(int i = 0; i < animalList.Count; i++)
        {
            if(animalList[i].idx == lastGetAnimalIndex)
            {
                var scroll = UIPopupStageAdventureAnimal._instance.GetComponentInChildren<UIReuseGrid_StageAdventure_Animal>();
                int itemIndex = i / 3;
                if(itemIndex > 0)
                {
                    scroll.ScrollItemMove(itemIndex);
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            }
        }

        var animals = UIPopupStageAdventureAnimal._instance.GetComponentsInChildren<UIItemAdventureAnimalInfo>();

        for (int i = 0; i < animals.Length; i++)
        {
            if (animals[i].AnimalIdx == lastGetAnimalIndex)
            {
                newAnimal = Instantiate(animals[i].gameObject);
                newAnimal.GetComponent<BoxCollider>().enabled = false;
                newAnimal.transform.SetParent(blind.transform);
                newAnimal.transform.localScale = Vector3.one;
                newAnimal.transform.position = animals[i].transform.position;

                yield break;
            }
        }
    }

    private void SetBack_NewAnimal()
    {
        Destroy(newAnimal);
        newAnimal = null;
    }
}
