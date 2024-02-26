using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureDamageMeter : MonoBehaviour {

    [System.Serializable]
    private struct FontColor
    {
        public Color main;
        public Color outLine;
        public Color gradientTop;
        public Color gradientBottom;
    }

    [Header("Link Object")]
    [SerializeField] private List<UILabel> damageLabels;

    [Header("Damage Text Color")]
    [SerializeField] private FontColor[] damageColor;
    //[SerializeField] private Color damageColor_Main;
    //[SerializeField] private Color damageColor_OutLine;
    //[SerializeField] private Color damageColor_GradientTop;
    //[SerializeField] private Color damageColor_GradientBottom;

    [Header("Heal Text Color")]
    [SerializeField] private Color healColor_Main;
    [SerializeField] private Color healColor_OutLine;
    [SerializeField] private Color healColor_GradientTop;
    [SerializeField] private Color healColor_GradientBottom;

    [Header("Animation Controller")]
    [SerializeField] private AnimationCurve heightController;
    [SerializeField] private AnimationCurve sizeController;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float maxPercent = 0.5f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private int offsetHeight = 0;
    public void Init(int height)
    {
        this.height = height + offsetHeight;
    }

    public void Damage(int enemyMaxHP, int damage, bool isHeal = false)
    {
        if (damage == 0)
            return;

        StartCoroutine(DamageAni(enemyMaxHP, damage, isHeal));
    }


    private int height = 100;
    private IEnumerator DamageAni(int enemyMaxHP, int damage, bool isHeal)
    {
        UILabel target = GetTatget();

        target.alpha = 1.0f;
        target.transform.localScale = Vector3.one;
        target.gameObject.SetActive(true);

        #region HeightAnimation
        float startHeight = 0.0f;
        float endHeight = startHeight + height + (LabelActiveCount() - 1) * 30.0f;
        target.depth = activeCount + 1;

        float totalTime = 0.0f;
        float endTime = Mathf.Max
        (
            heightController.keys[heightController.length - 1].time,
            sizeController.keys[sizeController.length - 1].time
        );

        #region ColorSet
        if (isHeal)
        {
            target.color = healColor_Main;
            target.effectColor = healColor_OutLine;
            target.gradientTop = healColor_GradientTop;
            target.gradientBottom = healColor_GradientBottom;
        }
        else
        {
            StartCoroutine(ColorAni(target, endTime + waitTime));
        }
        #endregion

        float minSizePercent = 1.0f / sizeController.keys[sizeController.length - 1].value;

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            target.transform.localPosition = Vector3.up * (startHeight + heightController.Evaluate(totalTime) * endHeight);

            int damageValue = Mathf.RoundToInt(Mathf.Lerp(0.0f, damage, totalTime / endTime));

            target.transform.localScale = 
                Vector3.one * (sizeController.Evaluate(totalTime) *
                Mathf.Clamp(((float)damageValue / enemyMaxHP) / maxPercent, minSizePercent, 1.0f));

            target.text = CustomRound(damage, 3, damageValue).ToString();
            
            if (totalTime >= endTime)
                break;

            yield return null;
        }
        #endregion

        yield return new WaitForSeconds(waitTime);

        #region FadeOutAnimation
        totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            target.alpha = Mathf.Lerp(1.0f, 0.0f, totalTime / fadeTime);

            if (totalTime >= 1.0f)
                break;

            yield return null;
        }
        #endregion

        target.gameObject.SetActive(false);        

    }

    public void Normal(int count)
    {
        if (count == 0)
            return;

        StartCoroutine(NormalAni(count));
    }
    private IEnumerator NormalAni(int count)
    {
        UILabel target = GetTatget();

        target.alpha = 1.0f;
        target.transform.localScale = Vector3.one;
        target.gameObject.SetActive(true);

        target.color = healColor_Main;
        target.effectColor = healColor_OutLine;
        target.gradientTop = healColor_GradientTop;
        target.gradientBottom = healColor_GradientBottom;
        target.text = "+" + count.ToString();

        #region HeightAnimation
        float startHeight = 0.0f;
        float endHeight = startHeight + height + (LabelActiveCount() - 1) * 30.0f;
        target.depth = activeCount + 1;

        float totalTime = 0.0f;
        float endTime = Mathf.Max
        (
            heightController.keys[heightController.length - 1].time,
            sizeController.keys[sizeController.length - 1].time
        );

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            target.transform.localPosition = Vector3.up * (startHeight + heightController.Evaluate(totalTime) * endHeight);
            target.transform.localScale = Vector3.one * (sizeController.Evaluate(totalTime));

            if (totalTime >= endTime)
                break;

            yield return null;
        }
        #endregion

        yield return new WaitForSeconds(waitTime);

        #region FadeOutAnimation
        totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            target.alpha = Mathf.Lerp(1.0f, 0.0f, totalTime / fadeTime);

            if (totalTime >= 1.0f)
                break;

            yield return null;
        }
        #endregion

        target.gameObject.SetActive(false);
    }

    private IEnumerator ColorAni(UILabel target, float endTime)
    {
        if (damageColor.Length == 0)
            yield break;

        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTimePuzzle;

            int colorIndex = Mathf.RoundToInt(Mathf.Lerp(0.0f, damageColor.Length - 1, totalTime / endTime));

            target.color = damageColor[colorIndex].main;
            target.effectColor = damageColor[colorIndex].outLine;
            target.gradientTop = damageColor[colorIndex].gradientTop;
            target.gradientBottom = damageColor[colorIndex].gradientBottom;

            if (totalTime >= endTime)
                break;

            yield return null;
        }
    }

    private UILabel GetTatget()
    {
        for(int i=0; i<damageLabels.Count; i++)
        {
            if (!damageLabels[i].gameObject.activeSelf)
                return damageLabels[i];
        }

        var label = Instantiate(damageLabels[0]);
        label.transform.SetParent(damageLabels[0].transform.parent);

        damageLabels.Add(label);
        return label;
    }

    private int activeCount = -1;
    private int LabelActiveCount()
    {
        activeCount++;

        CancelInvoke("ResetActiveCount");
        Invoke("ResetActiveCount", 1.0f);

        return activeCount;
    }

    private void ResetActiveCount()
    {
        activeCount = -1;
    }

    private int CustomRound(int maxValue, int splitCount, int value)
    {
        int baseValue = maxValue / splitCount;
        float normalValue = Mathf.Ceil((float)value / baseValue) / splitCount;

        return Mathf.RoundToInt(Mathf.Lerp(0.0f, maxValue, normalValue));
    }
}
