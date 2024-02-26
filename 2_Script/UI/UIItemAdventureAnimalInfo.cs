using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIItemAdventureAnimalInfo : MonoBehaviour {
    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalSlotMark slotMark;
    [SerializeField] private GameObject[] stars = new GameObject[5];
    [SerializeField] private UIUrlTexture animalFullShot;
    [SerializeField] private UIUrlTexture animalPicture;
    [SerializeField] private GameObject animalInfoButton;
    [SerializeField] private UISprite animalMark;
    [SerializeField] private UILabel animalCount;
    [SerializeField] private GameObject animalCountMax;
    [SerializeField] private bool overlapLabelIncludeX = false; // animalCount 레이블에 x를 붙여줄지
    [SerializeField] private bool sameTimeShow_Label_ICon = false; // 중첩 MAX일때 아이콘과 레이블을 동시에 보여줄지
    [SerializeField] private UILabel[] level;
    [SerializeField] private bool levelLabelIncludeLv = false; // level 레이블에 Lv.를 붙여줄지
    [SerializeField] private GameObject expGage_MAX_Effect;
    [SerializeField] private UIProgressBar expGage;
    [SerializeField] private UILabel expCurrent;
    [SerializeField] private UILabel expNext;
    [SerializeField] private UILabel attack;
    [SerializeField] private UILabel life;

    [SerializeField] private UISprite skillIcon;
    [SerializeField] private UILabel skill;

    [SerializeField] private UILabel maxLevel;
    [SerializeField] private UILabel maxAttack;
    [SerializeField] private UILabel maxLife;

    [SerializeField] private GameObject rootOn;
    [SerializeField] private UIItemAdventureAnimalOff rootOff;

    [SerializeField] private UILabel animalName;
    [SerializeField] private UIDynamicBubbles animalScript_1;
    [SerializeField] private UIDynamicBubbles animalScript_2;

    [SerializeField] private GameObject eventMark;
    [SerializeField] private bool autoLookColor = true;

    public bool IsInit
    {
        get { return aData != null; }
    }

    public int AnimalIdx
    {
        get { return aData.idx; }
    }

    private int Stars
    {
        set
        {
            if(stars[0] != null)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (i < value)
                        stars[i].SetActive(true);
                    else
                        stars[i].SetActive(false);
                }
            }
        }
    }

    public bool AnimalActive
    {
        private set
        {
            if(rootOn != null && rootOff != null)
            {
                rootOn.SetActive(value);
                rootOff.gameObject.SetActive(!value);
            }
        }
        get
        {
            if (rootOn != null && rootOff != null)
                return rootOn.activeSelf;
            else
                return true;
        }
    }

    public bool AnimalInfoButton
    {
        set
        {
            if(animalInfoButton != null)
            {
                animalInfoButton.SetActive(value);
            }
        }
    }

    private int AnimalMark
    {
        set
        {
            if(animalMark != null)
            {
                switch (value)
                {
                    case 0:
                        animalMark.gameObject.SetActive(false);
                        break;
                    case 1:
                        animalMark.gameObject.SetActive(true);
                        animalMark.spriteName = "animal_attr_1";
                        break;
                    case 2:
                        animalMark.gameObject.SetActive(true);
                        animalMark.spriteName = "animal_attr_2";
                        break;
                    case 3:
                        animalMark.gameObject.SetActive(true);
                        animalMark.spriteName = "animal_attr_3";
                        break;
                }
            }
        }
    }

    private int AnimalCount
    {
        set
        {
            if (animalCount != null && animalCountMax != null)
            {
                if(value == 0)
                {
                    SetOverlapZero();
                }
                else if(value == ManagerAdventure.ManagerAnimalInfo.GetMaxOverlap(AnimalIdx))
                {
                    animalCount.transform.parent.gameObject.SetActive(sameTimeShow_Label_ICon);
                    animalCountMax.SetActive(true);
                    animalCount.text = this.overlapLabelIncludeX ? "x" + value.ToString() : value.ToString();
                }
                else
                {
                    animalCount.transform.parent.gameObject.SetActive(true);
                    animalCountMax.SetActive(false);
                    animalCount.text = this.overlapLabelIncludeX ? "x" + value.ToString() : value.ToString();
                }
            }
        }
    }

    private int Level
    {
        set
        {
            if(level != null)
            {
                for(int i=0; i<level.Length; i++)
                {
                    if (level[i] == null)
                        break;

                    level[i].text = levelLabelIncludeLv ? ("Lv." + value.ToString()) : value.ToString();
                }
            }
        }
    }

    private float ExpGage
    {
        set
        {
            if(expGage != null)
            {
                expGage.value = value % 1.0f;

                //최대 레벨일 때 경험치바 처리
                if (expGage_MAX_Effect != null)
                {
                    bool isMaxLevel = ManagerAdventure.ManagerAnimalInfo.GetMaxLevel(aData.idx) == aData.level;
                    bool isGageFull = Mathf.Approximately(expGage.value, 0.0f);

                    expGage_MAX_Effect.SetActive(isGageFull && isMaxLevel);
                }
                    
            }
        }
    }

    private int ExpCurrent
    {
        set
        {
            if (expCurrent)
            {
                expCurrent.text = value.ToString();
            }
        }
    }

    private int ExpNext
    {
        set
        {
            if(expNext != null)
            {
                expNext.text = value.ToString();
            }
        }
    }

    private int Attack
    {
        set
        {
            if(attack != null)
            {
                attack.text = value.ToString();
            }
        }
    }

    private int Life
    {
        set
        {
            if(life != null)
            {
                life.text = value.ToString();
            }
        }
    }

    private int SkillIcon
    {
        set
        {
            if(skillIcon != null)
            {
                if(value == 0)
                {
                    skillIcon.transform.parent.gameObject.SetActive(false);

                    if (skill != null)
                    {
                        Color skillColor = skill.color;
                        skillColor.a = 0.3f;
                        skill.color = skillColor;
                    }
                }
                else
                {
                    skillIcon.transform.parent.gameObject.SetActive(true);
                    skillIcon.spriteName = "skill_icon_" + value + "_1";
                    skillIcon.MakePixelPerfect();
                    skillIcon.transform.localScale = Vector3.one * 0.66f;

                    if (skill != null)
                    {
                        Color skillColor = skill.color;
                        skillColor.a = 1.0f;
                        skill.color = skillColor;
                    }
                }
            }
        }
    }

    private string Skill
    {
        set
        {
            if (skill != null)
            {
                skill.text = value;
            }
        }
    }

    private string AnimalName
    {
        set
        {
            if (animalName != null)
            {
                animalName.text = value;
            }
        }
    }

    private string AnimalScript1
    {
        set
        {
            if (animalScript_1 != null)
            {
                if(aData.gettime == 1)
                {
                    animalScript_1.SetBubble(value, this);
                }
                else
                {
                    animalScript_1.SetBubble(ManagerAdventure.instance.GetString("a_c_1"), this);
                    animalScript_1.BubbleTail_Off();
                }
                
            }
        }
    }
    private Vector3 AnimalScript1Pos
    {
        set
        {
            if (animalScript_1 != null)
            {
                animalScript_1.transform.localPosition = value;
            }
        }
    }
    private string AnimalScript2
    {
        set
        {
            if (animalScript_2 != null)
            {
                animalScript_2.SetBubble(value, this);
            }
        }
    }
    private Vector3 AnimalScript2Pos
    {
        set
        {
            if (animalScript_2 != null)
            {
                animalScript_2.transform.localPosition = value;
            }
        }
    }

    private int MaxLevel
    {
        set
        {
            if(maxLevel != null)
            {
                maxLevel.text = value.ToString();
            }
        }

        get
        {
            if (maxLevel != null)
            {
                return int.Parse(maxLevel.text);
            }
            else
            {
                return 0;
            }
        }
    }

    private int MaxAttack
    {
        set
        {
            if(maxAttack != null)
            {
                maxAttack.text = value.ToString();
            }
        }
    }

    private int MaxLife
    {
        set
        {
            if(maxLife != null)
            {
                maxLife.text = value.ToString();
            }
        }
    }

    private bool EventMark
    {
        set
        {
            if(eventMark != null)
            {
                eventMark.SetActive(value);
            }
        }
    }

    private int lookID = -1;
    public int LookID
    {
        get
        {
            if(lookID == -1)
            {
                lookID = aData.lookId;
            }

            return lookID;
        }
        set
        {
            if (lookID != value)
            {
                lookID = value;
                TextureLoad();
            }
        }
    }

    private ManagerAdventure.AnimalInstance aData;

    public event System.Action<ManagerAdventure.AnimalInstance> setAnimalCallBack;

    public event System.Action changeStartEvent;
    public event System.Action changeEndEvent;
    private int changeAniCount = 0;
    private int ChangeCount
    {
        set
        {
            if(changeAniCount == 0 && value == 1)
            {
                if (changeStartEvent != null)
                    changeStartEvent();
            }
            else if(changeAniCount == 1 && value == 0)
            {
                changeAniTrigger = false;

                if (changeEndEvent != null)
                    changeEndEvent();
            }

            changeAniCount = value;
        }
        get
        {
            return changeAniCount;
        }
    }
    private bool changeAniTrigger = false;
    public bool isChangeAniRun
    {
        get
        {
            return changeAniTrigger;
        }
    }

    public void SetAnimalSelect(ManagerAdventure.AnimalInstance aData)
    {
        if (changeAniTrigger || isWaiting)   //Animation Running
            return;

        this.aData = aData;
        AnimalName = ManagerAdventure.instance.GetString(string.Format("n_a{0}", aData.idx));

        if (aData.overlap != 0)
        {
            var aBase = ManagerAdventure.Animal.GetAnimal(aData.idx);

            AnimalActive = true;
            Stars = aData.grade;
            
            AnimalScript1 = ManagerAdventure.instance.GetString(string.Format("a{0}_c_1", aData.idx));
            string script2 = aData.overlap == 1 ? string.Format("a{0}_d_1", aData.idx) : string.Format("a{0}_d_2", aData.idx);
            AnimalScript2 = ManagerAdventure.instance.GetString(script2);

            AnimalMark = aBase.attr;
            AnimalCount = aData.overlap;
            Level = aData.level;

            int currentExp = aData.totalExp - GetExpInfo(aData.grade, aData.level).startExp;
            int nextExp = GetNextExp(aData.grade, aData.level);
            ExpCurrent = currentExp;
            ExpNext = nextExp;
            ExpGage = nextExp != 0 ? (float)currentExp / nextExp : 0.0f;

            Attack = aData.atk;
            Life = aData.hp;
            SkillIcon = aData.skill % 100;
            string skillDesc = ManagerAdventure.instance.GetString(string.Format("s_d_{0}", aData.skill % 100));
            skillDesc = skillDesc.Replace("[n]", aData.skillGrade.ToString());
            Skill = skillDesc;

            MaxLevel = ManagerAdventure.ManagerAnimalInfo.GetMaxLevel(aData.idx);
            MaxAttack = ManagerAdventure.UserData.CalcAtk(aData.idx, aData.grade, MaxLevel, aData.overlap);
            MaxLife = ManagerAdventure.UserData.CalcHp(aData.idx, aData.grade, MaxLevel, aData.overlap);

            EventMark = ManagerAdventure.EventData.IsAdvEventBonusAnimal(aData.idx);

            LookID = aData.lookId;

            TextureLoad(aData);
        }
        else
        {
            AnimalActive = false;
            if(rootOff != null)
                rootOff.SetAnimalSelect(aData);
        }

        if (setAnimalCallBack != null)
            setAnimalCallBack(aData);
    }

    private void TextureLoad()
    {
        if(aData != null)
        {
            TextureLoad(aData);
        }
    }

    private void TextureLoad(ManagerAdventure.AnimalInstance aData)
    {
        if (animalPicture != null)
            animalPicture.LoadCDN(Global.adventureDirectory, "Animal/", ManagerAdventure.GetAnimalProfileFilename(aData.idx, LookID));

        if (animalFullShot != null)
        {
            animalFullShot.SuccessEvent += FullLoaded;
            string fileName = ManagerAdventure.GetAnimalTextureFilename(aData.idx, LookID);
            animalFullShot.LoadCDN(Global.adventureDirectory, "Animal/", fileName);
            animalFullShot.MakePixelPerfect();

            if (autoLookColor)
            {
                animalFullShot.color = LookCheck(LookID) ? Color.white : new Color(0.0f, 0.0f, 0.0f, 190.0f / 255.0f);
            }
        }
    }

    private bool LookCheck(int lookID)
    {
        switch (lookID)
        {
            case 0:
                return true;
            case 1:
                return aData.overlap == ManagerAdventure.ManagerAnimalInfo.GetMaxOverlap(aData.idx);
            default:
                return false;
        }
    }

    public event System.Action<Texture> fullLoadedEvent;
    public bool isFullShotLoaded {
        get
        {
            return animalFullShot.mainTexture != null;
        }
    }
    public GameObject FullshotObject
    {
        get
        {
            return animalFullShot.gameObject;
        }
    }
    private void FullLoaded()
    {
        if (fullLoadedEvent != null)
            fullLoadedEvent(animalFullShot.mainTexture);
    }

    public void ChangeAnimal_Ani(ManagerAdventure.AnimalInstance aData)
    {
        if (changeAniTrigger)   //Animation Running
            return;
        
        if (aData.overlap != this.aData.overlap)    //중첩 추가
        {
            changeAniTrigger = true;
            StartCoroutine(OverlapAni(aData));
        }

        if (aData.totalExp != this.aData.totalExp || aData.level != this.aData.level)    //레벨업시 또는 경험치만 얻을때
        {
            changeAniTrigger = true;
            StartCoroutine(GageChangeAni(aData.totalExp - this.aData.totalExp));
        }

        this.aData = aData;
    }

    private IEnumerator OverlapAni(ManagerAdventure.AnimalInstance aData)
    {
        string script2 = string.Format("a{0}_d_2", aData.idx);
        AnimalScript2 = ManagerAdventure.instance.GetString(script2);

        yield return new WaitForSeconds(0.5f);

        AnimalCount = aData.overlap;
        StartCoroutine(LabelChangeAni(attack, aData.atk));
        StartCoroutine(LabelChangeAni(life, aData.hp));
    }

    public void OpenPopupAnimalInfo()
    {
        if (!ManagerUI._instance._popupList[ManagerUI._instance._popupList.Count - 1].bCanTouch)
            return;

        ManagerUI._instance._popupList[ManagerUI._instance._popupList.Count - 1].bCanTouch = false;

        ManagerUI._instance.OpenPopup<UIPopupStageAdventureAnimalInfo>((popup) => popup.InitAnimalInfo(aData));
    }

    [Header("AnimationController")]
    private const float LABEL_CHANGE_TIME = 0.5f;
    [SerializeField] private AnimationCurve labelHeightAniController;
    [SerializeField] private AnimationCurve labelSizeAniController;
    
    private IEnumerator LabelChangeAni(UILabel target, int changeValue)
    {
        if (!TrySetRunAni(target))
            yield break;

        int startValue = int.Parse(target.text);
        int endValue = changeValue;
        
        if (startValue == endValue || !target.gameObject.activeSelf)
        {
            SetStopAni(target);
            yield break;
        }

        ChangeCount++;

        float startHeight = target.transform.localPosition.y;

        float showTimer = 0f;

        while (showTimer < 1f)
        {
            float posY = labelHeightAniController.Evaluate(showTimer);
            float scaleRatio = labelSizeAniController.Evaluate(showTimer);

            target.transform.localScale = Vector3.one * (scaleRatio * scaleRatio);
            target.transform.localPosition = new Vector3
            (
                target.transform.localPosition.x,
                startHeight + posY * 10,
                target.transform.localPosition.z
            );

            int tempValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, showTimer));
            target.text = tempValue.ToString();

            showTimer += Global.deltaTimePuzzle * 3f;
            yield return null;
        }

        Vector3 endPos = target.transform.localPosition;
        endPos.y = startHeight;
        target.transform.localPosition = endPos;
        target.text = changeValue.ToString();

        UIItemAdventureDamageMeter meter = target.GetComponentInChildren<UIItemAdventureDamageMeter>();
        if (meter != null)
        {
            meter.Init(40);
            int addValue = endValue - startValue;
            meter.Normal(addValue);
        }

        SetStopAni(target);
        ChangeCount--;
    }

    private List<UILabel> runList = new List<UILabel>();
    private bool TrySetRunAni(UILabel runObj)
    {
        bool isContains = runList.Contains(runObj);
        if (!isContains)
        {
            runList.Add(runObj);
            return true;
        }

        return false;
    }
    private void SetStopAni(UILabel runObj)
    {
        runList.Remove(runObj);
    }

    public event System.Func<UIItemAdventureAnimalInfo, IEnumerator> levelUpStartEvent;
    private const float GAGE_CHANGE_TIME = 0.4f;
    private IEnumerator GageChangeAni(int addExp)
    {
        if (addExp == 0 || !expGage.gameObject.activeSelf) yield break;

        int startLevel = aData.level;
        int currentLevel = startLevel;
        int startTotalExp = aData.totalExp;
        int endTotalExp = startTotalExp + addExp;

        ChangeCount++;
        float totalTime = 0.0f;

        ManagerSound.AudioPlay(AudioLobby.animal_levelup_gage);

        while (totalTime < GAGE_CHANGE_TIME)
        {
            totalTime += Global.deltaTimeNoScale;
            
            int currentTotalExp = Mathf.RoundToInt(Mathf.Lerp(startTotalExp, endTotalExp, totalTime / GAGE_CHANGE_TIME));
            currentLevel = GetLevel(startLevel, startTotalExp, currentTotalExp - startTotalExp);

            int currentExp = currentTotalExp - GetExpInfo(aData.grade, currentLevel).startExp;
            int nextExp = GetNextExp(aData.grade, currentLevel);

            ExpCurrent = currentExp;
            ExpNext = nextExp;
            ExpGage = nextExp != 0 ? (float)currentExp / nextExp : 0.0f;

            yield return null;
        }

        //레벨업 이벤트 발생
        if (startLevel < currentLevel && levelUpStartEvent != null)
        {
            DestrotExpGageEffect();

            //yield return levelUpStartEvent(this);
            StartCoroutine(levelUpStartEvent(this));
            ManagerSound.AudioPlay(AudioLobby.animal_levelup);

            Level = currentLevel;
            ExpNext = GetNextExp(aData.grade, currentLevel);

            StartCoroutine(LabelChangeAni(attack, aData.atk));
            StartCoroutine(LabelChangeAni(life, aData.hp));
        }

        ChangeCount--;
    }

    public void LevelUpWait()
    {
        StartCoroutine(GageWaitAni());
    }
    [HideInInspector] public bool isLevelUpLoaded = false;
    [SerializeField] private GameObject expGageEffect;
    private bool isWaiting = false;
    private const float GAGE_WAIT_TIME = 0.2f;//one cycle time
    private const int GAGE_WAIT_COUNT = 3;
    private IEnumerator GageWaitAni()
    {
        ChangeCount++;

        expGageEffect.SetActive(true);

        MakeExpGageEffect();

        isWaiting = true;
        isLevelUpLoaded = false;

        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            expGage.transform.localScale = Vector3.one * ((Mathf.Sin(totalTime * (Mathf.PI * 2.0f) / GAGE_WAIT_TIME) * 0.1f) + 1.0f);

            if (totalTime >= GAGE_WAIT_TIME * GAGE_WAIT_COUNT)
            {
                expGage.transform.localScale = Vector3.one;
                break;
            }
                
            yield return null;
        }

        if (isLevelUpLoaded)
            ChangeAnimal_Ani(ManagerAdventure.User.GetAnimalInstance(aData.idx));
        else
        {
            NetworkLoading.MakeNetworkLoading(0.0f);

            while (!isLevelUpLoaded)
                yield return new WaitForSeconds(0.1f);

            NetworkLoading.EndNetworkLoading();
            ChangeAnimal_Ani(ManagerAdventure.User.GetAnimalInstance(aData.idx));
        }

        isWaiting = false;

        expGageEffect.SetActive(false);

        ChangeCount--;
    }

    private AnimalExpInfo GetExpInfo(int grade, int currentLevel)
    {
        var table = ManagerAdventure.ManagerAnimalInfo.GetExpTable(grade);

        return table[currentLevel];
    }

    private int GetNextExp(int grade, int currentLevel)
    {
        var expInfo = GetExpInfo(grade, currentLevel);

        return expInfo.endExp - expInfo.startExp;
    }

    private int GetLevel(int currentLevel, int currentTotalExp, int addExp)
    {
        var expTable = ManagerAdventure.ManagerAnimalInfo.GetExpTable(aData.grade);

        while (expTable.ContainsKey(currentLevel) && currentTotalExp + addExp >= expTable[currentLevel].endExp)
        {
            currentLevel++;
        }

        return Mathf.Min(currentLevel, ManagerAdventure.ManagerAnimalInfo.GetMaxLevel(aData.idx));
    }

    [SerializeField] private GameObject levelUpPar_Effect;
    private GameObject effect;
    private void MakeExpGageEffect()
    {
        if (levelUpPar_Effect == null)
            return;

        effect = NGUITools.AddChild(expGage.gameObject, levelUpPar_Effect);
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localScale = levelUpPar_Effect.transform.localScale;
    }

    private void DestrotExpGageEffect()
    {
        if (levelUpPar_Effect == null || effect == null)
            return;

        Destroy(effect);
        effect = null;
    }

    public void CheckLookID(System.Action<bool> completeEvent = null)
    {
        if (LookCheck(LookID) && LookID != aData.lookId)
        {
            ManagerAdventure.User.AnimalLookChange(aData.idx, LookID, completeEvent);
        }
        else
        {
            completeEvent?.Invoke(false);
        }
    }

    public void SetOverlapZero()
    {
        animalCount.transform.parent.gameObject.SetActive(false);
        animalCountMax.SetActive(false);
    }
}


