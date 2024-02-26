using System.Collections;
using UnityEngine;

public class UIItemAdventureLifeBar : MonoBehaviour {
    [SerializeField] private UISlider mainSlider;
    [SerializeField] private UISlider subSlider;
    [SerializeField] private UILabel lifeLabel;

    private int maxLife;
    public bool isEnemy = false;

    public float Value
    {
        set
        {
            if(mainSlider.value > value)
            {
                Damage(value);
            }
            else if(mainSlider.value < value)
            {
                Heal(value);
            }
        }
        get
        {
            if (isHealRuning)
                return subSlider.value;
            else
                return mainSlider.value;
        }
    }

    public void Init(float value, int maxLife)
    {
        mainSlider.value = value;
        subSlider.value = value;

        this.maxLife = maxLife;
        lifeLabel.text = Mathf.RoundToInt(maxLife * value).ToString();
    }

    private const float ANIMATION_SPEED = 2.0f;

    private void Damage(float value)
    {
        mainSlider.value = value;

        if (!isDamageRuning)
            StartCoroutine(DamageAni());
    }

    private bool isDamageRuning = false;
    private IEnumerator DamageAni()
    {
        isDamageRuning = true;
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            subSlider.value = Mathf.MoveTowards
            (
                subSlider.value, 
                mainSlider.value, 
                Global.deltaTimePuzzle * ANIMATION_SPEED * (mainSlider.value == 0.0f ? 2.0f : 1.0f)
            );
            lifeLabel.text = Mathf.RoundToInt(subSlider.value * maxLife).ToString();

            if (subSlider.value == mainSlider.value)
                break;

            yield return null;
        }
        isDamageRuning = false;

        if (mainSlider.value == 0)
            yield return DeadAni();
    }

    private IEnumerator DeadAni()
    {
        yield return null;
        if (isEnemy) gameObject.SetActive(false);
    }

    private void Heal(float value)
    {
        subSlider.value = value;

        if (!isHealRuning)
            StartCoroutine(HealAni());
    }

    public event System.Action HealEndEvent;
    private bool isHealRuning = false;
    private IEnumerator HealAni()
    {
        isHealRuning = true;
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            mainSlider.value = Mathf.MoveTowards(mainSlider.value, subSlider.value, Global.deltaTimePuzzle * ANIMATION_SPEED);
            lifeLabel.text = Mathf.RoundToInt(mainSlider.value * maxLife).ToString();

            if (subSlider.value == mainSlider.value)
                break;

            yield return null;
        }

        if (HealEndEvent != null)
            HealEndEvent();

        HealEndEvent = null;
        isHealRuning = false;
    }
}
