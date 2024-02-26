using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class LoginDialogData
{
    public Transform objRoot;
    public UIWidget.Pivot dialogPivot;
    public NGUIText.Alignment aligment;
    public int maxWidth;
    public UITexture dialogBg;
}

public class UIPopupLoginBonus : UIPopupBase
{
    public static UIPopupLoginBonus _instance = null;

    public UIPanel sortingPanel;

    //question
    public GameObject questionRoot;
    public UISprite questionBtnRoot;
    public UILabel[] btnQuestionText_1;
    public UILabel[] btnQuestionText_2;
    public UILabel[] dialogText;
    public UILabel warningText;

    //answer
    public GameObject answerRoot;
    public GameObject answerBtnRoot;
    public UISprite answerBtnSprite;
    public UITexture[] textureLight;
    public UILabel[] btnAnswerText;
    public UILabel[] questionText;
    public UILabel answerText;
    public UILabel rewardText;
    public UIItemLoginReward rewardItem;

    //bonus
    public GameObject bonusBtnRoot;
    public UISprite bonusBtnSprite;
    public GameObject bonusRoot;
    public UILabel[] btnBonusText;
    public UILabel loginBonusTimeText;
    public UILabel loginBonusInfoText;
    public UILabel[] rewardNumberText;

    private UILoginBonus loginBonusObj;
    
    private int questionIndex = 0;
    private string[] selectQuestionText = new string[2];

    private CdnLoginEvent loginEventData;
    private int loginCnt = 0;
    private Reward reward = new Reward();

    private int loginLayer = 1;

    private void Awake()
    {
        _instance = this;
    }

    new void OnDestroy()
    {   
        base.OnDestroy();
        _instance = null;
    }

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        sortingPanel.depth = uiPanel.depth + 2;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        loginLayer = layer;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = loginLayer;
            loginLayer += 1;
        }

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup(GameObject loginObj)
    {   
        MakeLoginBonusObj(loginObj);
        if (loginBonusObj == null)
            return;

        bCanTouch = false;

        loginEventData = ServerContents.LoginEvent;
        loginCnt = ServerRepos.UserLoginEvent.loginEventCnt;
        reward = ServerRepos.UserLoginEvent.loginEventReward;

        InitDepthAndSortLayer();
        InitLoginBonus();
        InitQuestionObject();
        InitAnswerObject();
        InitBonusObject();
    }

    private void InitDepthAndSortLayer()
    {
        loginBonusObj.gameObject.SetActive(false);
        //스파인 sortLayer.
        loginBonusObj.spineCharacter.GetComponent<MeshRenderer>().sortingOrder = loginLayer;
        //로그인 보너스 패널들.
        loginBonusObj.topPanel.useSortingOrder = true;
        loginBonusObj.topPanel.sortingOrder = loginLayer + 1;
        loginBonusObj.topPanel.depth = uiPanel.depth + 1;
        //현재 팝업의 패널들(최상위).
        sortingPanel.useSortingOrder = true;
        sortingPanel.sortingOrder = loginLayer + 2;
        loginBonusObj.gameObject.SetActive(true);
    }

    private void MakeLoginBonusObj(GameObject go)
    {
        //로그인 보너스 오브젝트 생성 및 설정.
        Transform t = go.transform;
        t.parent = mainSprite.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        go.layer = gameObject.layer;
        loginBonusObj = go.GetComponent<UILoginBonus>();
    }

    private void SettingRootActive(GameObject[] root, bool bActive)
    {
        for (int i = 0; i < root.Length; i++)
        {
            root[i].SetActive(bActive);
        }
    }

    private void InitLoginBonus()
    {
        questionRoot.gameObject.SetActive(true);
        answerRoot.gameObject.SetActive(false);
        bonusRoot.gameObject.SetActive(false);

        SettingRootActive(loginBonusObj.questionRoot, true);
        SettingRootActive(loginBonusObj.answerRoot, false);
        SettingRootActive(loginBonusObj.bonusRoot, false);
    }

    private void InitQuestionObject()
    {
        selectQuestionText[0] = "1. " + Global._instance.GetString("p_lg_q_0");
        selectQuestionText[1] = "2. " + Global._instance.GetString("p_lg_q_1");
        warningText.text = Global._instance.GetString("p_lg_2");

        for (int i = 0; i < btnQuestionText_1.Length; i++)
            btnQuestionText_1[i].text = selectQuestionText[0];
        for (int i = 0; i < btnQuestionText_2.Length; i++)
            btnQuestionText_2[i].text = selectQuestionText[1];

        InitDialogChat();
        questionBtnRoot.gameObject.SetActive(false);
        loginBonusObj.spineCharacter.gameObject.SetActive(true);
        loginBonusObj.spineCharacter.state.SetAnimation(0, loginBonusObj.appearAniname, false);
        loginBonusObj.spineCharacter.state.AddAnimation(0, loginBonusObj.idleAniname, true, 0f);
        StartCoroutine(CoActionQuest());
        for (int i = 0; i < loginBonusObj.titleAlphaObj.Length; i++)
        {
            int speed = Random.Range(1, 5);
            StartCoroutine(CoAlphaTexture(loginBonusObj.titleAlphaObj[i], speed));
        }
    }

    private void InitDialogChat()
    {
        for (int i = 0; i < loginBonusObj.dialogData.Count; i++)
        {
            if (dialogText.Length <= i)
                break;
            LoginDialogData textData = loginBonusObj.dialogData[i];
            string textKey = string.Format("dialog_{0}", i);
            dialogText[i].overflowWidth = textData.maxWidth;
            dialogText[i].pivot = textData.dialogPivot;
            dialogText[i].alignment = textData.aligment;
            dialogText[i].transform.transform.parent = textData.objRoot;
            dialogText[i].transform.transform.localPosition = Vector3.zero;
            dialogText[i].text = Global._instance.GetString(textKey);
            dialogText[i].gameObject.SetActive(true);

            StartCoroutine(CoQuestionDialogResize(dialogText[i], textData.objRoot, textData.dialogBg));
        }
    }

    private IEnumerator CoQuestionDialogResize(UILabel label, Transform objRoot, UITexture dialogBg)
    {
        yield return null;

        if(dialogBg != null)
        {
            var labelSize = label.printedSize;
            dialogBg.width = (int)labelSize.x + 80;
            dialogBg.height = (int)labelSize.y + 80;
            
            var lPos = objRoot.localPosition;
            lPos.x = 50f;
            lPos.y = 40f + (int)(labelSize.y / 2f);
            objRoot.localPosition = lPos;
        }

        yield break;
    }

    private IEnumerator CoActionQuest()
    {
        loginBonusObj.title.color = new Color(loginBonusObj.title.color.r, loginBonusObj.title.color.g, loginBonusObj.title.color.b, 0f);
        loginBonusObj.mainStar.color = new Color(loginBonusObj.mainStar.color.r, loginBonusObj.mainStar.color.g, loginBonusObj.mainStar.color.b, 0f);
        questionBtnRoot.color = new Color(questionBtnRoot.color.r, questionBtnRoot.color.g, questionBtnRoot.color.b, 0f);
        loginBonusObj.title.transform.parent.localPosition -= new Vector3(0f, -300f, 0f);
        questionBtnRoot.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.3f);
        ManagerSound.AudioPlay(AudioLobby.move_down);
        DOTween.ToAlpha(() => loginBonusObj.mainStar.color, x => loginBonusObj.mainStar.color = x, 1f, 0.7f);
        DOTween.ToAlpha(() => loginBonusObj.title.color, x => loginBonusObj.title.color = x, 1f, 0.3f);
        loginBonusObj.title.transform.parent.DOLocalMoveY(567f, 0.4f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.2f);
        ManagerSound.AudioPlay(AudioLobby.m_bird_aham2);
        questionBtnRoot.gameObject.SetActive(true);
        DOTween.ToAlpha(() => questionBtnRoot.color, x => questionBtnRoot.color = x, 1f, 0.2f);
        questionBtnRoot.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.3f);
        bCanTouch = true;
    }

    private void InitAnswerObject()
    {
        for (int i = 0; i < btnAnswerText.Length; i++)
            btnAnswerText[i].text = Global._instance.GetString("p_lg_4");
        rewardText.text = Global._instance.GetString("p_lg_5");
        rewardItem.InitReward(reward, 30, 1.5f, 100, true);

        answerBtnRoot.SetActive(false);
        questionText[0].color = new Color(questionText[0].color.r, questionText[0].color.g, questionText[0].color.b, 0f);
        answerBtnSprite.color = new Color(answerBtnSprite.color.r, answerBtnSprite.color.g, answerBtnSprite.color.b, 0f);
        answerText.color = new Color(answerText.color.r, answerText.color.g, answerText.color.b, 0f);
        rewardText.color = new Color(rewardText.color.r, rewardText.color.g, rewardText.color.b, 0f);
        rewardText.transform.localScale = new Vector3(1.5f, 1.5f, 0f);
        loginBonusObj.answerBox.color = new Color(loginBonusObj.answerBox.color.r, loginBonusObj.answerBox.color.g, loginBonusObj.answerBox.color.b, 0f);
    }

    private void InitBonusObject()
    {
        for (int i = 0; i < btnBonusText.Length; i++)
            btnBonusText[i].text = Global._instance.GetString("p_lg_4");
        loginBonusInfoText.text = Global._instance.GetString("p_lg_3");

        InitBonusButton();
        InitLoginTime();
        InitLoginStep();
        InitRewardBubble();
    }

    private void InitBonusButton()
    {
        bonusBtnRoot.SetActive(false);
        bonusBtnSprite.color = new Color(bonusBtnSprite.color.r, bonusBtnSprite.color.g, bonusBtnSprite.color.b, 0f);
    }

    private void InitLoginTime()
    {
        System.DateTime startTime = ConvertFromUnixTimestamp(loginEventData.startTs);
        System.DateTime endTime = ConvertFromUnixTimestamp(loginEventData.endTs);
        System.DateTime alterEndTime = endTime.AddHours(-ServerRepos.LoginCdn.LoginOffset);
        alterEndTime = alterEndTime.AddMinutes(-ServerRepos.LoginCdn.LoginOffsetMin);
        alterEndTime = alterEndTime.AddSeconds(-1);

        string text = string.Format(GetTimeText(startTime) +"~"+ GetTimeText(alterEndTime));
        loginBonusTimeText.text = text;
    }

    private System.DateTime ConvertFromUnixTimestamp(double timestamp)
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        origin = origin.AddHours(9);
        return origin.AddSeconds(timestamp);
    }

    private string GetTimeText(System.DateTime date)
    {
        string text = string.Format("{0}.{1:00}.{2:00}", date.Year, date.Month, date.Day);
        
        return text;
    }

    private void InitLoginStep()
    {
        if (loginCnt > loginBonusObj.step.Count)
            loginCnt = loginBonusObj.step.Count;
        else if (loginCnt < 1)
            loginCnt = 1;

        //연출을 위해 1 빼줌.
        loginCnt -= 1;

        //출석한 날들 발판이미지 바꿔줌.
        for (int i = 0; i < loginCnt; i++)
        {
            loginBonusObj.step[i].mainTexture = loginBonusObj.stepOnTexture;
            loginBonusObj.step[i].width = (int)loginBonusObj.stepOnSize.x;
            loginBonusObj.step[i].height = (int)loginBonusObj.stepOnSize.y;
            if ( i > 0 && loginBonusObj.stepLine.Count > (i - 1))
            {
                loginBonusObj.stepLine[(i - 1)].mainTexture = loginBonusObj.stepLineOnTexture;
            }
            if (rewardNumberText.Length > i)
            {
                rewardNumberText[i].color = loginBonusObj.stepOnColor;
                rewardNumberText[i].fontSize = loginBonusObj.stepOnFontSize;
            }
        }
        
        //숫자 세팅.
        for (int i = 0; i < loginBonusObj.step.Count; i++)
        {
            if (rewardNumberText.Length <= i)
                break;
            rewardNumberText[i].gameObject.SetActive(true);
            rewardNumberText[i].transform.localPosition = loginBonusObj.step[i].transform.localPosition;
            rewardNumberText[i].transform.localPosition += loginBonusObj.offsetReward;
            rewardNumberText[i].text = (i + 1).ToString();
        }
        loginBonusObj.actionStep.alpha = 0;
        loginBonusObj.actionStep.transform.transform.parent = loginBonusObj.step[loginCnt].transform;
        loginBonusObj.actionStep.transform.localPosition = Vector3.zero;
    }

    private void InitRewardBubble()
    {
        for (int i = 0; i < loginEventData.rares.Count; i++)
        {
            if (loginCnt >= loginEventData.rares[i])
            {
                if (i < loginBonusObj.loginRewardBubble.Count)
                    loginBonusObj.loginRewardBubble[i].SetActive(false);
                continue;
            }

            if (i < loginBonusObj.loginRewardRoot.Count)
            {
                GameObject itemRoot = loginBonusObj.loginRewardRoot[i];
                UIItemLoginReward item = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemLoginReward", itemRoot).GetComponent<UIItemLoginReward>();
                int index = loginEventData.rares[i];
                item.InitReward(loginEventData.rewards[index], 25, 1, 50);
            }
        }
    }

    private void OnClickBtnQuestion_1()
    {
        questionIndex = 0;
        OnClickBtnQuestion();
    }

    private void OnClickBtnQuestion_2()
    {
        questionIndex = 1;
        OnClickBtnQuestion();
    }

    private void OnClickBtnQuestion()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        loginBonusObj.spineCharacter.state.ClearTracks();
        loginBonusObj.spineCharacter.state.SetAnimation(0, loginBonusObj.questionSelectAniname, false);
        for (int i=0;i<questionText.Length; i++)
        {
            questionText[i].text = selectQuestionText[questionIndex];
        }
        //랜덤으로 답변 구하기.
        SettingRandomAnswer();

        //연출.
        StartCoroutine(CoActionAnswer());
    }

    private void SettingRandomAnswer()
    {
        int textListCount = loginBonusObj.textListCount[questionIndex];
        int randIndex = Random.Range(0, textListCount);
        string key = string.Format("a_{0}_{1}", questionIndex, randIndex);
        answerText.text = Global._instance.GetString(key);
        SetBoxSize();
    }

    private void SetBoxSize()
    {
        int _nLineCount = (int)(answerText.printedSize.y / answerText.fontSize);

        int boxHeight = 250;

        if (_nLineCount == 2)
        {
            boxHeight = 325;
        }
        else if (_nLineCount > 2)
        {
            boxHeight = 450;
        }
        loginBonusObj.answerBox.height = boxHeight;
    }

    private IEnumerator CoActionAnswer()
    {
        if( loginBonusObj.questionRoot != null && loginBonusObj.questionRoot.Length > 0 )
            loginBonusObj.questionRoot[0].SetActive(false);

        if (loginBonusObj._objParticleMagicCrystal != null)
        {
            InstantiateUIObject(loginBonusObj._objParticleMagicCrystal, sortingPanel.gameObject, loginBonusObj.magicEffectRoot.localPosition, loginBonusObj.magicEffectSize);
        }
        ManagerSound.AudioPlay(AudioLobby.LetterBox2);

        yield return new WaitForSeconds(1.0f);
        ManagerSound.AudioPlay(AudioLobby.m_boni_laugh);

        yield return new WaitForSeconds(loginBonusObj.questionSelectDelay);
        ManagerSound.AudioPlay(AudioLobby.Mission_BGM_End2);
        loginBonusObj.spineCharacter.state.SetAnimation(0, loginBonusObj.resultAppearAniname, false);
        loginBonusObj.spineCharacter.state.AddAnimation(0, loginBonusObj.resultIdleAniname, true, 0f);

        questionRoot.gameObject.SetActive(false);
        answerRoot.gameObject.SetActive(true);
        SettingRootActive(loginBonusObj.questionRoot, false);
        SettingRootActive(loginBonusObj.answerRoot, true);

        loginBonusObj.blind.gameObject.SetActive(false);
        loginBonusObj.title.gameObject.SetActive(false);
        loginBonusObj.mainStar.gameObject.SetActive(false);
        
        loginBonusObj.spineCharacter.transform.localPosition = new Vector3(5f, -247f, 0f);

        DOTween.ToAlpha(() => loginBonusObj.blind.color, x => loginBonusObj.blind.color = x, 0f, 0.3f);

        yield return new WaitForSeconds(0.2f);
        DOTween.ToAlpha(() => questionText[0].color, x => questionText[0].color = x, 1f, 0.3f);
        loginBonusObj.spineCharacter.transform.DOScale(230f, 0.4f);
        loginBonusObj.spineCharacter.transform.DOLocalMove(new Vector3(5f, -305f, 0f), 0.4f, true);

        yield return new WaitForSeconds(1.5f);
        ManagerSound.AudioPlay(AudioLobby.m_boni_surprise);
        DOTween.ToAlpha(() => answerText.color, x => answerText.color = x, 1f, 0.7f);
        DOTween.ToAlpha(() => loginBonusObj.answerBox.color, x => loginBonusObj.answerBox.color = x, 1f, 0.7f);

        yield return new WaitForSeconds(1.5f);
        float boxPosY = loginBonusObj.answerBox.transform.localPosition.y - 100f;
        float answerTextPosY = answerText.transform.localPosition.y - 100f;
        loginBonusObj.answerBox.transform.DOLocalMoveY(boxPosY, 0.4f);
        answerText.transform.DOLocalMoveY(answerTextPosY, 0.4f);

        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => rewardText.color, x => rewardText.color = x, 1f, 0.1f).SetEase(Ease.OutQuint);
        rewardText.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuint);

        yield return new WaitForSeconds(0.1f);
        ManagerSound.AudioPlay(AudioLobby.Deco_Get);
        rewardItem.RewardItemAlpha(0.3f);
        for (int i = 0; i < textureLight.Length; i++)
            textureLight[i].gameObject.SetActive(true);
        DOTween.To(() => 0f, x => SettingRewardLightAlpha(x), 1f, 0.3f);

        yield return new WaitForSeconds(0.1f);
        answerBtnRoot.SetActive(true);
        DOTween.ToAlpha(() => answerBtnSprite.color, x => answerBtnSprite.color = x, 1f, 0.3f);

        yield return new WaitForSeconds(0.3f);
        bonusBtnRoot.SetActive(true);
        bCanTouch = true;
    }

    public GameObject InstantiateUIObject(GameObject prefab, GameObject parent, Vector3 pos, float size)
    {
        var go = Instantiate(prefab);
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one * size;
            t.localPosition = pos;
            go.layer = parent.layer;
        }
        return go;
    }

    public void SettingRewardLightAlpha(float in_alpha)
    {
        textureLight[0].alpha = in_alpha;
        textureLight[1].alpha = in_alpha;
    }

    private void OnClickBtnAnswerOK()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        loginBonusObj.spineCharacter.state.ClearTracks();
        loginBonusObj.spineCharacter.state.SetAnimation(0, loginBonusObj.rewardIdleAniname, true);

        answerRoot.gameObject.SetActive(false);
        bonusRoot.gameObject.SetActive(true);
        SettingRootActive(loginBonusObj.answerRoot, false);
        SettingRootActive(loginBonusObj.bonusRoot, true);

        loginBonusObj.title.gameObject.SetActive(true);
        loginBonusObj.title.transform.localPosition = new Vector3(0f, 55f, 0f);
        
        StartCoroutine(CoActionStep(loginCnt));
    }

    public override void OnClickBtnBack()
    {
        if (bCanTouch == false || !answerBtnRoot.activeSelf)
            return;
        OnClickBtnClose();
    }

    private void OnClickBtnLoginBonusOK()
    {
        OnClickBtnClose();
    }

    private IEnumerator CoActionStep(int day)
    {
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => loginBonusObj.actionStep.color, x => loginBonusObj.actionStep.color = x, 1f, 0.5f).SetEase(Ease.InQuart);
        loginBonusObj.actionStep.transform.DOScale(1f, 0.5f).SetEase(Ease.InQuart);

        yield return new WaitForSeconds(0.5f);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        loginBonusObj.actionStep.transform.DOShakePosition(0.2f, 5f, 20, 90f, false, false);
        if (rewardNumberText.Length > day)
        {
            rewardNumberText[day].color = loginBonusObj.stepOnColor;
            rewardNumberText[day].fontSize = loginBonusObj.stepOnFontSize;
            rewardNumberText[day].transform.localScale = Vector3.one * 0.5f;
            rewardNumberText[day].transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(0.3f);
        if (day > 0 && loginBonusObj.stepLine.Count > day - 1)
        {
            loginBonusObj.stepLine[day - 1].color = new Color(loginBonusObj.stepLine[day - 1].color.r, loginBonusObj.stepLine[day - 1].color.g, loginBonusObj.stepLine[day - 1].color.b, 0f);
            loginBonusObj.stepLine[day - 1].mainTexture = loginBonusObj.stepLineOnTexture;
            DOTween.ToAlpha(() => loginBonusObj.stepLine[day - 1].color, x => loginBonusObj.stepLine[day - 1].color = x, 1f, 0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        bonusBtnRoot.SetActive(true);
        DOTween.ToAlpha(() => bonusBtnSprite.color, x => bonusBtnSprite.color = x, 1f, 0.3f);

        yield return new WaitForSeconds(0.3f);
        bCanTouch = true;
    }

    private IEnumerator CoAlphaTexture(UITexture texture, int speed)
    {
        while (true)
        {
            if (texture.gameObject.activeInHierarchy == false)
                break;
            
            float ratio = (0.5f + Mathf.Cos(Time.time * speed) * 0.2f);
            texture.color = new Color(1f, 1f, 1f, ratio);
            yield return null;
        }
        yield return null;
    }
}
