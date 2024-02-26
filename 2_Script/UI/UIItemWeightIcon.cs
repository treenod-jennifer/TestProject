using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWeightIcon : MonoBehaviour
{
    public enum WeightType
    {
        Attribute_Scissors  = 1,
        Attribute_Rock      = 2,
        Attribute_Paper     = 3,
        EventBonus
    }
    public struct WeightInfo
    {
        public WeightType weightType;
        public int weight;
    }

    [Header("Link Object")]
    [SerializeField] private GameObject root;
    [SerializeField] private UILabel weightLabel;

    [SerializeField] private List<UIItemWeightInfo> weightList;
    [SerializeField] private UISprite expandedPanelBG;
    [SerializeField] private UIGrid gridRoot;

    [SerializeField] private UIPanel mainPanel;
    [SerializeField] private UIPanel expandedPanel;

    [SerializeField] private GameObject expandedOn;
    [SerializeField] private GameObject expandedOff;

    [Header("Animation Controller")]
    [SerializeField] private AnimationCurve iconAniController;
    [SerializeField] private AnimationCurve iconAniPosYController;

    private void Start()
    {
        UIPanel top = transform.GetComponentInParent<UIPanel>();

        mainPanel.depth = top.depth + 2;
        expandedPanel.depth = top.depth + 1;
    }

    public void InitWeigth()
    {
        foreach (var item in weightList)
        {
            item.OffWeightInfo();
        }
    }

    public void AddWeigth(WeightInfo weightInfo)
    {
        UIItemWeightInfo target = null;

        foreach (var item in weightList)
        {
            if (!item.gameObject.activeSelf)
            {
                target = item;
                break;
            }
        }

        if (target == null)
        {
            return;
        }

        target.SetWeightInfo(weightInfo);

        gridRoot.enabled = true;
    }

    public void OnIcon()
    {
        StopAllCoroutines();
        StartCoroutine(IconAni_On());
    }

    private IEnumerator IconAni_On()
    {
        yield return new WaitForSeconds(0.3f);

        root.SetActive(true);

        StartCoroutine(CoWeightOpenAni());

        float totalTime = 0.0f;
        float endTime = iconAniController.keys[iconAniController.length - 1].time;

        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            float posY = 56 + iconAniPosYController.Evaluate(totalTime);

            transform.localScale = Vector3.one * iconAniController.Evaluate(totalTime);
            transform.localPosition = new Vector3(transform.localPosition.x, posY, 0);

            if (totalTime >= endTime)
                break;

            yield return null;
        }
    }

    const int CLOSE_HEIGHT = -160;
    const int OFFSET_HEIGHT = 10;

    private bool isOpen = false;
    private IEnumerator CoWeightOpenAni()
    {
        isOpen = true;

        const float RUN_TIME = 0.1f;
        float totalTime = 0.0f;
        int itemCount = 0;

        WeightUtility.SetLabel(0, weightLabel);

        foreach (var item in weightList)
        {
            if (item.gameObject.activeSelf)
            {
                itemCount++;
            }
        }

        float openHeight = CLOSE_HEIGHT + (gridRoot.cellHeight * itemCount) + (itemCount == 0 ? 0 : OFFSET_HEIGHT);

        while (true)
        {
            totalTime += Global.deltaTimeLobby;

            Vector3 pos = expandedPanelBG.transform.localPosition;
            pos.y = Mathf.Lerp(CLOSE_HEIGHT, openHeight, totalTime / RUN_TIME);
            expandedPanelBG.transform.localPosition = pos;

            if (pos.y >= openHeight)
            {
                break;
            }

            yield return null;
        }

        expandedOn.SetActive(false);
        expandedOff.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        yield return CoWeightCloseAni();

        isOpen = false;
    }

    private IEnumerator CoWeightCloseAni()
    {
        const float RUN_TIME = 0.1f;
        int itemCount = 0;
        int weight = 0;

        foreach (var item in weightList)
        {
            if (item.IsActive())
            {
                itemCount++;
            }
        }

        for (int i = 1; i <= itemCount; i++)
        {
            float totalTime = 0.0f;

            float currentHeight = expandedPanelBG.transform.localPosition.y;
            float closeHeight = currentHeight - gridRoot.cellHeight - (i == itemCount ? OFFSET_HEIGHT : 0);
            int nextWeight = weight + weightList[itemCount - i].GetWeight();

            while (true)
            {
                totalTime += Global.deltaTimeLobby;
                float time = totalTime / RUN_TIME;

                Vector3 pos = expandedPanelBG.transform.localPosition;
                pos.y = Mathf.Lerp(currentHeight, closeHeight, time);
                expandedPanelBG.transform.localPosition = pos;

                int tempWeight = Mathf.RoundToInt(Mathf.Lerp(weight, nextWeight, time));
                WeightUtility.SetLabel(tempWeight, weightLabel);

                if (pos.y <= closeHeight)
                {
                    weight = tempWeight;
                    break;
                }

                yield return null;
            }

            //yield return new WaitForSeconds(0.1f);
        }

        expandedOn.SetActive(true);
        expandedOff.SetActive(false);
    }

    private void WeightClose()
    {
        Vector3 pos = expandedPanelBG.transform.localPosition;
        pos.y = CLOSE_HEIGHT;
        expandedPanelBG.transform.localPosition = pos;
    }

    public void OnClickIcon()
    {
        if (!isOpen)
        {
            StartCoroutine(CoWeightOpenAni());
        }
    }
}

public static class WeightUtility
{
    private static Color textColorPositive      = new Color(99.0f/255.0f, 255.0f/255.0f, 60.0f/255.0f);
    private static Color effectColorPositive    = new Color(25.0f/255.0f, 103.0f/255.0f, 0.0f/255.0f);

    private static Color textColorNegative      = new Color(255.0f/255.0f, 99.0f/255.0f, 99.0f/255.0f);
    private static Color effectColorNegative    = new Color(156.0f/255.0f, 0.0f/255.0f, 0.0f/255.0f);

    private static Color textColorZero          = new Color(255.0f/255.0f, 255.0f/255.0f, 255.0f/255.0f);
    private static Color effectColorZero        = new Color(27.0f/255.0f, 73.0f/255.0f, 114.0f/255.0f);

    public static void SetLabel(int weight, UILabel target)
    {
        if (weight > 0)
        {
            target.color = textColorPositive;
            target.effectColor = effectColorPositive;
        }
        else if (weight < 0)
        {
            target.color = textColorNegative;
            target.effectColor = effectColorNegative;
        }
        else
        {
            target.color = textColorZero;
            target.effectColor = effectColorZero;
        }

        target.text = GetText(weight);
    }

    public static string GetText(int weight)
    {
        string text = string.Empty;

        if (weight > 0)
        {
            text = "+" + weight + "%";
        }
        else if (weight <= 0)
        {
            text = weight + "%";
        }

        return text;
    }
}
