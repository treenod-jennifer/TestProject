using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemSummon : MonoBehaviour
{
    [SerializeField] private ISummonHost hostPopup = null;
    [SerializeField] private GameObject summonBG_Normal;
    [SerializeField] private GameObject summonBG_Premium;
    [SerializeField] private UIUrlTexture summonBG_Custom;
    [SerializeField] private UISprite summonIcon;

    
    

    
    

    [Header("NormalGacha Object")]
    [SerializeField] private GameObject normalLabel;
    [SerializeField] private UILabel[] costLabel_Normal;
    [SerializeField] private GameObject costIcon_Normal;

    [Header("PremiumGacha Object")]
    [SerializeField] private GameObject premiumLabel;
    [SerializeField] private UILabel[] costLabel_Premium;
    [SerializeField] private GameObject costIcon_Premium;
    [SerializeField] private GameObject premiumEventObj;
    [SerializeField] private GameObject premiumCountObj;
    [SerializeField] private UILabel premiumCountLabel;
    [SerializeField] private UILabel premiumMaxCountLabel;
    [SerializeField] private GameObject premiumDisable;
    [SerializeField] private UILabel disableTimeLabel;

    [Header("MileageGacha Object")]
    [SerializeField] private GameObject mileageCountObj;
    [SerializeField] private UILabel mileageCountLabel;
    [SerializeField] private UILabel mileageMaxCountLabel;
    [SerializeField] private UIProgressBar mileageProgressBar;

    [Header("Event Object")]
    [SerializeField] private GameObject rateUpEventObj;
    [SerializeField] private GameObject discountEventObj;

    [Header("Effect Object")]
    [SerializeField] private GameObject freeEffectObj;

    CdnAdventureGachaProduct data;

    public bool premiumClose
    {
        get
        {
            return openTime == -1;
        }
    }

    public bool premiumTrigger
    {
        get
        {
            return premiumLabel.activeSelf;
        }
        private set
        {
            if (value)
            {
                costLabel_Normal[0].gameObject.SetActive(false);
                costLabel_Premium[0].gameObject.SetActive(true);

                summonBG_Normal.SetActive(false);
                summonBG_Premium.SetActive(true);
                summonIcon.gameObject.SetActive(false);

                normalLabel.SetActive(false);
                premiumLabel.SetActive(true);

                if (rateUpEventObj.activeSelf)
                    premiumEventObj.SetActive(false);
                else
                    premiumEventObj.SetActive(true);

                premiumCountObj.SetActive(true);
            }
            else
            {
                costLabel_Normal[0].gameObject.SetActive(true);
                costLabel_Premium[0].gameObject.SetActive(false);

                summonBG_Normal.SetActive(true);
                summonBG_Premium.SetActive(false);
                summonIcon.gameObject.SetActive(true);

                normalLabel.SetActive(true);
                premiumLabel.SetActive(false);
                premiumEventObj.SetActive(false);

                premiumCountObj.SetActive(false);
            }
        }
    }

    public bool mileageTrigger
    {
        get
        {
            return mileageCountObj.activeSelf;
        }
        private set
        {
            if (value)
            {
                mileageCountObj.SetActive(true);
            }
            else
            {
                mileageCountObj.SetActive(false);
            }
        }
    }

    private UILabel[] costLabel
    {
        get
        {
            return premiumTrigger ? costLabel_Premium : costLabel_Normal;
        }
    }

    private GameObject costIcon
    {
        get
        {
            return premiumTrigger ? costIcon_Premium : costIcon_Normal;
        }
    }

    private void OnEnable()
    {
        activeTimer = true;
    }

    private string _freeText = null;
    private string freeText
    {
        get
        {
            if (_freeText == null)
                _freeText = Global._instance.GetString("p_frd_l_15");

            return _freeText;
        }
    }

    private string _firstFreeText = null;
    private string firstFreeText
    {
        get
        {
            if (_firstFreeText == null)
                _firstFreeText = Global._instance.GetString("p_frd_l_11");

            return _firstFreeText;
        }
    }

    private void SetPrice(int price, bool isFirst = false)
    {
        if (isFirst && price <= 0)
        {
            string fontData = costLabel[0].bitmapFont.ToString();
            if (!fontData.Contains("NotoSansCJKjp-Black"))
            {
                NGUIFont font = ManagerFont._instance.GetUIfont("NotoSansCJKjp-Black");
                costLabel[0].bitmapFont = font;
                costLabel[1].bitmapFont = font;
            }

            costLabel.SetText(firstFreeText);

            Vector3 pos = costLabel[0].transform.localPosition;
            pos.x = 35;
            costLabel[0].transform.localPosition = pos;
            costIcon.SetActive(false);
            freeEffectObj.SetActive(isMileageAniRun ? false : true);
        }
        else if(!isFirst && price <= 0)
        {
            string fontData = costLabel[0].bitmapFont.ToString();
            if (!fontData.Contains("NotoSansCJKjp-Black"))
            {
                NGUIFont font = ManagerFont._instance.GetUIfont("NotoSansCJKjp-Black");
                costLabel[0].bitmapFont = font;
                costLabel[1].bitmapFont = font;
            }

            costLabel.SetText(freeText);

            Vector3 pos = costLabel[0].transform.localPosition;
            pos.x = 54;
            costLabel[0].transform.localPosition = pos;
            costIcon.SetActive(true);
            freeEffectObj.SetActive(isMileageAniRun ? false : true);
        }
        else
        {
            string fontData = costLabel[0].bitmapFont.ToString();
            if (!fontData.Contains("LILITAONE-REGULAR"))
            {
                NGUIFont font = ManagerFont._instance.GetUIfont("LILITAONE-REGULAR");
                costLabel[0].bitmapFont = font;
                costLabel[1].bitmapFont = font;
            }

            costLabel.SetText(price.ToString());

            Vector3 pos = costLabel[0].transform.localPosition;
            pos.x = 54;
            costLabel[0].transform.localPosition = pos;
            costIcon.SetActive(true);
            freeEffectObj.SetActive(false);
        }
    }
    private bool GetPriceFree()
    {
        return costLabel[0].text == freeText || costLabel[0].text == firstFreeText;
    }

    public void InitData(ISummonHost hostPopup, CdnAdventureGachaProduct data)
    {
        this.hostPopup = hostPopup;
        this.data = data;


        if (rateUpEventObj != null)
            rateUpEventObj.gameObject.SetActive(data.rate_up != 0);
        if (discountEventObj != null)
            discountEventObj.gameObject.SetActive(data.sale != 0);

        //가챠 데이터에는 가격이 설정되어 있지만, 이런 저런 이유료 무료가챠인 경우 무료라고 표기해 주기 위해 사용되는 값
        int price = data.price;

        mileageTrigger = false;

        if (data.asset_type == 2)
        {
            premiumTrigger = false;
        }
        else if (data.asset_type == 3)
        {
            premiumTrigger = true;

            if (ServerRepos.UserInfo.jewelGachaCount == 0)
                price = 0;

            List<ServerUserPremiumGacha> gachaInfos = ServerRepos.UserPremiumGachas.FindAll(
                (ServerUserPremiumGacha gachaInfo) => { return gachaInfo.product_id == data.product_id; }
            );

            int gachaCount = gachaInfos.Count == 0 ? 0 : gachaInfos[gachaInfos.Count - 1].gacha_count;
            long nextResetTime = gachaInfos.Count == 0 ? 0 : GetNextResetTime(gachaInfos[gachaInfos.Count - 1].gacha_ts);

            //초기화 시간을 넘긴 경우 0으로 표시한다.
            if (gachaCount > 0 && nextResetTime != 0 && Global.LeftTime(nextResetTime) < 0)
                gachaCount = 0;

            if (gachaCount == data.can_count)
            {
                long endTime = ServerContents.AdvGachaProducts[gachaInfos[gachaInfos.Count - 1].product_id].expired_at;

                if (endTime != 0 && nextResetTime > endTime)
                    nextResetTime = -1;

                openTime = nextResetTime;
                activeTimer = true;
            }

            SetPremiumCount(gachaCount, data.can_count);
        }

        //종료된 마일리지 가챠중 무료기회를 가졌었는지에 대한 체크
        bool isEndedGachaFree = CheckEndedGachaFree();
        if (isEndedGachaFree)
            price = 0;

        if (data.mileage_flag == 0 || isEndedGachaFree)
        {
            mileageTrigger = false;
        }
        else
        {
            mileageTrigger = true;
            List<ServerUserGachaMileage> mileageinfos = ServerRepos.UserGachaMileages.FindAll(
                (ServerUserGachaMileage mileageInfo) => { return mileageInfo.product_id == data.product_id; }
            );

            bool isFree = SetMileageGage(
                mileageinfos.Count > 0 ? mileageinfos[mileageinfos.Count - 1].gacha_count : 0,
                data.mileage_max,
                name == "SummonsButton"
            );

            if (isFree) price = 0;
        }

        if (data.collabo > 0)
        {
            string fileName = (premiumTrigger ? "bt_p_gacha_" : "bt_c_gacha_");

            summonBG_Custom.gameObject.SetActive(true);
            summonBG_Custom.LoadCDN(Global.adventureDirectory, "Animal/", fileName + data.collabo);
        }
        else summonBG_Custom.gameObject.SetActive(false);

        //첫회 무료라고 보여줘야 하는 경우
        bool isTutorialGacha = data.asset_type == 2 && data.price == 0 && data.expired_at == 0;
        bool isFirstPremiumGacha = data.asset_type == 3 && price == 0;

        SetPrice(price, isTutorialGacha || isFirstPremiumGacha);
    }

    private bool CheckEndedGachaFree()
    {
        if (data.asset_type != 2)
            return false;

        if (ServerRepos.UserGachaMileages == null)
            return false;

        if (ServerRepos.UserGachaMileages.Count == 0)
            return false;

        ServerUserGachaMileage lastMileagesGachaInfo = ServerRepos.UserGachaMileages[ServerRepos.UserGachaMileages.Count - 1];

        if (data.product_id == lastMileagesGachaInfo.product_id)
            return false;

        CdnAdventureGachaProduct lastMileagesGachaProduct;
        if (!ServerContents.AdvGachaProducts.TryGetValue(lastMileagesGachaInfo.product_id, out lastMileagesGachaProduct))
            return false;

        return CheckMileageFree(lastMileagesGachaInfo.gacha_count, lastMileagesGachaProduct.mileage_max);
    }

    public void OnClickSummon()
    {
        if (activeTimer)
            return;

        hostPopup.Summon(data);
    }


    private void SetPremiumCount(int count, int fullCount)
    {
        premiumCountLabel.text = (fullCount - count).ToString();
        premiumMaxCountLabel.text = "/" + fullCount.ToString();
    }
    private void ResetPremiumCount()
    {
        premiumCountLabel.text = premiumMaxCountLabel.text.Remove(0, 1);
    }

    private bool SetMileageGage(int count, int fullCount, bool isAnimation = false)
    {
        int mileageCount = count % (fullCount + 1);
        float progressBar = (float)mileageCount / fullCount;

        if (mileageCount != 0 && isAnimation)
        {
            UIPopupAdventureSummonAction._instance.StartCoroutine(SetMileageGageAni(mileageCount, fullCount));
        }
        else
        {
            mileageProgressBarValue = progressBar;
            mileageCountLabel.text = mileageCount.ToString();
            mileageMaxCountLabel.text = "/" + fullCount.ToString();
        }

        return Mathf.Approximately(progressBar, 1.0f);
    }

    private UITexture mileageProgressBarTexture;
    private float mileageProgressBarValue
    {
        set
        {
            if (mileageProgressBarTexture == null)
                mileageProgressBarTexture = mileageProgressBar.GetComponentInChildren<UITexture>(true);

            mileageProgressBar.value = value;
            
            Rect rect = mileageProgressBarTexture.uvRect;
            rect.x = 1.0f - mileageProgressBar.value;
            mileageProgressBarTexture.uvRect = rect;
        }
        get
        {
            return mileageProgressBar.value;
        }
    }

    private bool isMileageAniRun = false;
    private IEnumerator SetMileageGageAni(int count, int fullCount)
    {
        isMileageAniRun = true;

        const float gageBarAnimaionTime = 0.3f;

        float startProgressBar = (float)Mathf.Clamp(count - 1, 0, fullCount) / fullCount;
        float endProgressBar = (float)Mathf.Clamp(count, 0, fullCount) / fullCount;
        float totalTime = 0.0f;

        mileageProgressBarValue = startProgressBar;
        mileageCountLabel.text = (count - 1).ToString();
        mileageMaxCountLabel.text = "/" + fullCount.ToString();

        yield return new WaitUntil(
            () => { return gameObject.activeInHierarchy && transform.parent.parent.localScale == Vector3.one; }
        );

        yield return new WaitForSeconds(0.7f);

        while (totalTime < gageBarAnimaionTime)
        {
            totalTime += Global.deltaTimeLobby;

            float t = Mathf.Sin(Mathf.Clamp(totalTime / gageBarAnimaionTime, 0.0f, 1.0f) * (Mathf.PI * 0.5f));
            mileageProgressBarValue = Mathf.LerpUnclamped(startProgressBar, endProgressBar, t);

            yield return null;
        }


        const float countAnimaionTime = 0.3f;
        const float startScale = 1.0f;
        const float endScale = 1.5f;

        totalTime = 0.0f;

        mileageCountLabel.depth += 3;
        mileageCountLabel.text = count.ToString();

        while (totalTime < countAnimaionTime)
        {
            totalTime += Global.deltaTimeLobby;

            float t = Mathf.Sin(totalTime / countAnimaionTime * Mathf.PI);
            mileageCountLabel.transform.localScale = Mathf.Lerp(startScale, endScale, t) * Vector3.one;

            yield return null;
        }

        mileageCountLabel.depth -= 3;

        if (GetPriceFree())
            freeEffectObj.SetActive(true);

        isMileageAniRun = false;
        yield break;
    }

    private bool CheckMileageFree(int count, int fullCount)
    {
        int mileageCount = count % (fullCount + 1);
        return mileageCount == fullCount;
    }

    #region 프리미엄 가챠 비활성화 처리
    private bool activeTimer
    {
        set
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (value && !premiumDisable.activeSelf)
            {
                if(openTime == -1)
                    return;

                long timeRemaining = Global.LeftTime(openTime);

                if (timeRemaining > 0.0f)
                {
                    premiumDisable.SetActive(true);
                    StartCoroutine(StartTimeLabel(timeRemaining));
                }
            }
            else if (!value && premiumDisable.activeSelf)
            {
                StopAllCoroutines();
                premiumDisable.SetActive(false);
            }
        }
        get
        {
            return premiumDisable.activeSelf;
        }
    }
    private long openTime;
    private const float TIMER_TICK = 1.0f;
    private IEnumerator StartTimeLabel(long time)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);

        while (timeSpan.TotalSeconds >= 0)
        {
            disableTimeLabel.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            yield return new WaitForSecondsRealtime(TIMER_TICK);

            timeSpan -= System.TimeSpan.FromSeconds(TIMER_TICK);
        }

        activeTimer = false;
        ResetPremiumCount();
    }

    private long GetNextResetTime(long lastPremiumGachaTime)
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

        System.DateTime todayResetTime = origin;
        todayResetTime = todayResetTime.AddSeconds(Global.GetTime());
        todayResetTime = todayResetTime.Date;
        todayResetTime = todayResetTime.AddHours(ServerRepos.LoginCdn.LoginOffset);
        todayResetTime = todayResetTime.AddMinutes(ServerRepos.LoginCdn.LoginOffsetMin);

        System.DateTime lastGachaTime = origin;
        lastGachaTime = lastGachaTime.AddSeconds(lastPremiumGachaTime);

        System.DateTime resetTime = todayResetTime > lastGachaTime ? todayResetTime : todayResetTime.AddDays(1);
        resetTime = resetTime.AddSeconds(10);

        System.TimeSpan diff = resetTime - origin;
        double time = System.Math.Floor(diff.TotalSeconds);

        return (long)time;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!activeTimer)
            return;

        if (focus)
        {
            activeTimer = false;
            activeTimer = true;
        }
    }
    #endregion
}

public interface ISummonHost
{
    void Summon(CdnAdventureGachaProduct data);
}