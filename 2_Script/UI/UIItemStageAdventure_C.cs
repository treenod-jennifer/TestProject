using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StageAdventure_C_ItemData
{
    public string bossName;
    public int chapterNumber;
    public int bossResId;

    public StageAdventure_C_ItemData(string bossName, int chapterNumber, int bossResId)
    {
        this.bossName = bossName;
        this.chapterNumber = chapterNumber;
        this.bossResId = bossResId;
    }
}

public class UIItemStageAdventure_C : MonoBehaviour {
    public UIUrlTexture chapterBoss;
    public UILabel chapterNumber;
    [SerializeField] private Color openColor;
    [SerializeField] private Color closeColor;

    public GameObject chapterCursorRoot;

    public GameObject whiteFlowerRoot;
    public GameObject blueFlowerRoot;
    public GameObject blueFlowerParticle;
    public GameObject bushRoot;
    public GameObject[] bushObjects;

    [SerializeField] private Animation bossAniController;
    public TweenScale tweenFlowerBloom;
    public TweenColor tweenColorBossAppear;

    public GameObject diamondsRoot;
    public GameObject[] diamonds;
    public GameObject diamondEffect;
    public GameObject effectPanel;

    public GameObject numberPanelRoot;
    public GameObject comingSoonRoot;
    public GameObject comingSoonAnimal;

    private StageAdventure_C_ItemData sData;

    public void SettingItem(StageAdventure_C_ItemData data)
    {
        sData = data;

        if ( ManagerAdventure.Stage.GetChapter(data.chapterNumber) == null )
        {
            comingSoonRoot.SetActive(true);
            chapterCursorRoot.SetActive(false);
            chapterBoss.gameObject.SetActive(false);
            bushRoot.SetActive(false);
            whiteFlowerRoot.SetActive(false);
            blueFlowerRoot.SetActive(false);
            diamondsRoot.SetActive(false);

            chapterNumber.text = sData.chapterNumber.ToString();
            chapterNumber.color = closeColor;

            if ((ManagerAdventure.User.AllCleared() && !ManagerAdventure.User.IsNormalFirstCleared()) || isShowComingSoon)
            {
                comingSoonAnimal.SetActive(true);
            }
            else
            {
                comingSoonAnimal.SetActive(false);
                numberPanelRoot.transform.localScale = new Vector3(0f, 0f, 1f);
            }
                
            return;
        }

        comingSoonRoot.SetActive(false);

        if (ManagerAdventure.User.GetLastUnfinishedChapter() == 0)
        {
            bushRoot.SetActive(data.chapterNumber > ManagerAdventure.User.GetChapterCursor());
        }
        else
        {
            bushRoot.SetActive(data.chapterNumber > ManagerAdventure.User.GetLastUnfinishedChapter());
        }

            

        var chapterProg = ManagerAdventure.User.GetChapterProgress(sData.chapterNumber);
        if (chapterProg != null && chapterProg.state > 1)
        {
            chapterCursorRoot.transform.localPosition = Vector3.up * 90.0f;
            chapterCursorRoot.SetActive(UIPopupStageAdventure.selectedChapter == sData.chapterNumber);

            chapterBoss.gameObject.SetActive(false);
            this.diamondsRoot.SetActive(false);
            blueFlowerRoot.SetActive(chapterProg.state > 3);
            whiteFlowerRoot.SetActive(chapterProg.state < 3);

            chapterNumber.color = openColor;
        }
        else
        {
            this.diamondsRoot.SetActive(true);
            chapterBoss.gameObject.SetActive(true);
            chapterBoss.SuccessEvent += OnBossTextureLoaded;
            chapterBoss.LoadCDN(Global.adventureDirectory, "Animal/", string.Format("m_{0:D4}", data.bossResId));

            whiteFlowerRoot.SetActive(false);
            blueFlowerRoot.SetActive(false);

            var chapData = ManagerAdventure.Stage.GetChapter(sData.chapterNumber);
            if (chapData != null)
            {
                var clearRewards = chapData.chapterClearReward;
                if (clearRewards.Count > 0)
                {
                    int diaIdx = (clearRewards[0].value - 1) > 2 ? 2 : (clearRewards[0].value - 1);
                    for (int i = 0; i < diamonds.Length; ++i)
                        diamonds[i].SetActive(diaIdx == i);
                }
            }
            if( bushRoot.activeSelf )
            {
                diamondsRoot.SetActive(false);
            }
        }
        numberPanelRoot.transform.localScale = Vector3.one;
        chapterNumber.text = sData.chapterNumber.ToString();

        //chapterCursorRoot.SetActive(UIPopupStageAdventure.selectedChapter == sData.chapterNumber);

        //chapterNumber.transform.localScale = Vector3.one * (Global.chapterIndex == sData.chapterNumber ? 2.0f : 1.0f);
    }

    private void CursorSetting()
    {
        chapterCursorRoot.transform.position = new Vector3
        (
            chapterCursorRoot.transform.position.x,
            chapterBoss.transform.position.y + chapterBoss.mainTexture.GetHeight() * chapterBoss.transform.lossyScale.y, 
            chapterCursorRoot.transform.position.z
        );
        chapterCursorRoot.transform.localPosition = new Vector3
        (
            chapterCursorRoot.transform.localPosition.x,
            Mathf.Clamp(chapterCursorRoot.transform.localPosition.y + 130.0f, 0.0f, 230.0f),
            chapterCursorRoot.transform.localPosition.z
        );

        //Debug.Log(chapterCursorRoot.transform.localPosition);
    }

    void OnBossTextureLoaded()
    {
        CursorSetting();
        chapterCursorRoot.SetActive(UIPopupStageAdventure.selectedChapter == sData.chapterNumber);

        bool bossInactive = false;
        if(ManagerAdventure.User.GetLastUnfinishedChapter() == 0 )
        {
            if (sData.chapterNumber > ManagerAdventure.User.GetChapterCursor())
                bossInactive = true;
        }
        else
        {
            if (sData.chapterNumber > ManagerAdventure.User.GetLastUnfinishedChapter())
                bossInactive = true;
        }

        if( bossInactive )
        {
            chapterBoss.color = new Color(0f, 0f, 0f, 0.79f);

            bossAniController.Stop();
            chapterBoss.transform.localEulerAngles = Vector3.zero;
            chapterBoss.transform.localScale = Vector3.one;
            chapterNumber.color = closeColor;
        }
        else
        {
            chapterBoss.color = Color.white;

            bossAniController.Play("Enemy_Wait");
            chapterNumber.color = openColor;
        }
    }

    //포지션에 따라 챕터번호의 크기가 바뀌는 기능
    //private const float MOVE_SIZE = 0.2f;
    //private void Update()
    //{
    //    float size = Mathf.Clamp(transform.position.x, MOVE_SIZE * -1.0f, MOVE_SIZE);

    //    chapterNumber.transform.localScale = Vector3.one * Mathf.Lerp(2.0f, 1.0f, Mathf.Abs(size) / MOVE_SIZE);
    //    chapterNumber.transform.localPosition = new Vector3(100.0f * chapterNumber.transform.localScale.x * -0.5f, chapterNumber.transform.localPosition.y, chapterNumber.transform.localPosition.z);
    //}

    private void SetLargerNumber()
    {
        //StartCoroutine(CoSetLargerNumber());
        chapterCursorRoot.SetActive(UIPopupStageAdventure.selectedChapter == sData.chapterNumber);
    }

    private IEnumerator CoSetLargerNumber()
    {
        float time = 0.0f;

        while(time <= 1.0f)
        {
            time += Time.unscaledDeltaTime * 5.0f;
            chapterNumber.transform.localScale = Vector3.one * Mathf.Lerp(UIPopupStageAdventure.selectedChapter == sData.chapterNumber ? 1.0f : 2.0f, UIPopupStageAdventure.selectedChapter == sData.chapterNumber ? 2.0f : 1.0f, time);
            //chapterNumber.transform.localPosition = new Vector3(0, chapterNumber.transform.localPosition.y, chapterNumber.transform.localPosition.z);
            yield return null;
        }
    }

    public int GetChapterNumber()
    {
        return sData.chapterNumber;
    }

    //public static int selectIndex = -1;
    public void SelectChapter()
    {
        //선택된 번호가 같으면 캔슬 한다.
        if (UIPopupStageAdventure.selectedChapter == sData.chapterNumber)
            return;

        if (sData.chapterNumber > ManagerAdventure.User.GetChapterCursor())
            return;

        //이전에 선택된 챕터의 번호는 작아지게 한다.
        UIItemStageAdventure_C[] brothers = transform.parent.GetComponentsInChildren<UIItemStageAdventure_C>();
        for (int i = 0; i < brothers.Length; i++)
        {
            if (brothers[i].sData.chapterNumber == UIPopupStageAdventure.selectedChapter)
            {
                brothers[i].chapterCursorRoot.SetActive(false);
                break;
            }
        }

        //현제 선택된 챕터의 번호를 저장하고 크게 한다.
        //Global.SetGameType_Adventure(sData.chapterNumber, 0);
        UIPopupStageAdventure.selectedChapter = sData.chapterNumber;
        SetLargerNumber();

        #region 스크롤 새로고침
        UIPopupStageAdventure._instance.scroll_ChapterList.ScrollItemMove(sData.chapterNumber - 1);

        UIPopupStageAdventure.RefreshStageList();

        UIPopupStageAdventure._instance.scroll_StageList.ScrollReset();
        #endregion
    }

    [SerializeField] private GameObject way;

    public void WayOn(int distance, float height)
    {
        way.transform.localPosition = new Vector3(distance * -0.5f, height * 0.5f, way.transform.localPosition.z);
        way.transform.localEulerAngles = Vector3.forward * Mathf.Atan2(height, -distance) * Mathf.Rad2Deg;
        way.SetActive(true);
    }

    public void WayOff()
    {
        way.SetActive(false);
    }

    int sentDiaCount = 0;
    bool bossColorWait = false;

    public IEnumerator CoReplaceFlower()
    {
        whiteFlowerRoot.SetActive(false);
        
        blueFlowerRoot.SetActive(true);
        blueFlowerParticle.SetActive(true);
        var tweeners = blueFlowerRoot.GetComponents<UITweener>();
        for (int i = 0; i < tweeners.Length; ++i)
        {
            tweeners[i].enabled = true;
        }

        yield return new WaitForSeconds(0.2f);

        yield break;

    }
    public IEnumerator CoBossDown()
    {
        ManagerSound.AudioPlay(AudioInGame.ENEMY_DISAPPEAR_1);
        bossAniController.Play("Enemy_Dead_UI");
        bossColorWait = true;

        yield return new WaitForSeconds(1.2f);

        OnBossDisappear();

        chapterCursorRoot.transform.DOLocalMoveY(90.0f, 0.4f);

        sentDiaCount = 0;
        var chapData = ManagerAdventure.Stage.GetChapter(sData.chapterNumber);
        if (chapData != null)
        {
            var clearRewards = chapData.chapterClearReward;
            if( clearRewards.Count > 0)
            {
                for (int i = 0; i < clearRewards[0].value; ++i)
                {
                    yield return new WaitForSeconds(0.3f);

                    Transform startTarget = diamonds[clearRewards[0].value - 1].transform.GetChild(i);
                    startTarget.gameObject.SetActive(false);
                    FlyDiamond(startTarget);
                    sentDiaCount++;
                }
            }
        }

        while (bossColorWait == true && sentDiaCount > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }

    public void OnBossDisappear()
    {
        whiteFlowerRoot.SetActive(true);
        this.tweenFlowerBloom.enabled = true;
    }

    public void OnFlowerBloom()
    {
        bossColorWait = false;
    }

    public IEnumerator CoBossAppear()
    {
        bossColorWait = true;

        var chapData = ManagerAdventure.Stage.GetChapter(sData.chapterNumber);
        if (chapData != null)
        {
            var clearRewards = chapData.chapterClearReward;
            if (clearRewards.Count > 0)
            {
                diamondsRoot.SetActive(true);
                int diaIdx = (clearRewards[0].value - 1) > 2 ? 2 : (clearRewards[0].value - 1);
                for (int i = 0; i < diamonds.Length; ++i)
                    diamonds[i].SetActive(diaIdx == i);
            }
        }

        {
            var tweeners = chapterBoss.GetComponents<UITweener>();
            for (int i = 0; i < tweeners.Length; ++i)
            {
                tweeners[i].enabled = true;
            }
        }

        {
            var tweeners = bushRoot.GetComponentsInChildren<UITweener>();
            for (int i = 0; i < tweeners.Length; ++i)
            {
                tweeners[i].enabled = true;
            }
        }
        

        while (bossColorWait)
            yield return new WaitForSeconds(0.1f);

        chapterNumber.color = Color.white;

        bossAniController.Play("Enemy_Wait");

        yield break;

    }

    public void OnBossAppear()
    {
        bossColorWait = false;
        bushRoot.SetActive(false);
    }

    public IEnumerator CoFlowerBloom()
    {
        this.tweenFlowerBloom.enabled = true;

        yield break;
    }

    public IEnumerator CoBossHit_StageClear()
    {
        bossAniController.Play("Enemy_StageClear");
        ManagerSound.AudioPlay(AudioInGame.ENEMY_DISAPPEAR_2);
        yield return new WaitForSeconds(0.2f);

        yield break;
    }

    private bool isShowComingSoon = false;
    public IEnumerator OpenComingSoon()
    {
        var tweeners = this.numberPanelRoot.GetComponents<UITweener>();
        for(int i = 0; i < tweeners.Length; ++i)
        {
            tweeners[i].enabled = true;
        }
        ManagerSound.AudioPlay(AudioInGame.GET_STATUE);
        yield return new WaitForSeconds(0.25f);

        this.comingSoonAnimal.SetActive(true);
        ManagerSound.AudioPlay(AudioInGame.GET_STONE_STATUE);

        isShowComingSoon = true;

        yield break;
    }

    public void FlyDiamond(Transform startTarget)
    {
        ManagerSound.AudioPlay(AudioLobby.UseClover);

        UIUseDiaEffect cloverEffect = NGUITools.AddChild(effectPanel, diamondEffect).GetComponent<UIUseDiaEffect>();
        cloverEffect.targetObj = this.gameObject;
        cloverEffect.Init(startTarget.position, ManagerUI._instance._DiaSprite.transform.position);
        cloverEffect.width = 150f;
        cloverEffect.startScale = 0.6f;
        cloverEffect.endScale = 0.9f;
    }

    void ShowUseDia()
    {
        Global.jewel++;
        ManagerUI._instance.UpdateUI();

        sentDiaCount--;
    }
}
