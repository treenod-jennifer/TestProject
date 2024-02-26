using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMaterialExchange : UIPopupBase
{
    public static UIPopupMaterialExchange instance;

    private GameObject exchangeMaterial = null;
    private GameObject ExchangeMaterial
    {
        get
        {
            if(exchangeMaterial == null)
            {
                exchangeMaterial = Resources.Load("UIPrefab/UIItemExchangeMaterial") as GameObject;
            }

            return exchangeMaterial;
        }
    }

    [Header("Scroll Root")]
    [SerializeField] private UIGrid subGradeGridRoot;
    [SerializeField] private UIGrid upperGradeGridRoot;
    [SerializeField] private UIPanel subScrollPanel;
    [SerializeField] private UIPanel upperScrollPanel;
    [SerializeField] private UICenterOnChild subCenterControl;
    [SerializeField] private UICenterOnChild upperCenterControl;

    [Header("Exchange Count")]
    [SerializeField] private UILabel subGradeCount;
    [SerializeField] private UILabel upperGradeCount;
    [SerializeField] private UILabel subName;
    [SerializeField] private UILabel upperName;

    [Header("Tab")]
    [SerializeField] private GameObject normalToRareOn;
    [SerializeField] private GameObject normalToRareOff;
    [SerializeField] private GameObject rareToSuperOn;
    [SerializeField] private GameObject rareToSuperOff;
    [SerializeField] private UILabel tabExplanation;

    private List<ServerUserMaterial> normalMaterials = new List<ServerUserMaterial>();
    private List<ServerUserMaterial> rareMaterials = new List<ServerUserMaterial>();
    private List<ServerUserMaterial> superMaterials = new List<ServerUserMaterial>();

    private List<UIItemExchangeMaterial> subGradeMaterialItems = new List<UIItemExchangeMaterial>();
    private List<UIItemExchangeMaterial> upperGradeMaterialItems = new List<UIItemExchangeMaterial>();

    private UIItemExchangeMaterial selectsubGradeMaterial;
    private UIItemExchangeMaterial selectUpperGradeMaterial;

    private enum Mode
    {
        NormalToRare,
        RareToSuper
    }

    private Mode currentMode = Mode.NormalToRare;

    public int SubGradeCount
    {
        get
        {
            return int.Parse(subGradeCount.text.Trim('-'));
        }
        private set
        {
            if (selectsubGradeMaterial == null) return;

            subGradeCount.text = "-" + value.ToString();
        }
    }

    public int UpperGradeCount
    {
        get
        {
            return int.Parse(upperGradeCount.text.Trim('+'));
        }
        private set
        {
            if (selectsubGradeMaterial == null) return;

            int exchangeCount = GetSubCount(value);

            if (selectsubGradeMaterial.GetCount() >= exchangeCount)
            {
                upperGradeCount.text = "+" + value.ToString();
                SubGradeCount = exchangeCount;
                subGradeCount.color = Color.white;
            }
            else
            {
                upperGradeCount.text = "0";
                SubGradeCount = GetSubCount(1);
                subGradeCount.color = Color.red;
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }

    protected override void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }

        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        panelCount = panels.Length - 1;

        InitData();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();

        for(int i=0; i<panels.Length; i++)
        {
            panels[i].useSortingOrder = true;
            panels[i].sortingOrder = layer + i;
        }

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private void InitData()
    {
        normalMaterials.Clear();
        rareMaterials.Clear();
        superMaterials.Clear();

        foreach (var material in ServerContents.MaterialMeta)
        {
            if (IsMaterialTimeOver(material.Key)) continue;

            switch (material.Value.grade)
            {
                case 0:
                    normalMaterials.Add(new ServerUserMaterial() { index = material.Value.mat_id, count = GetCount(material.Value.mat_id) });
                    break;
                case 1:
                    rareMaterials.Add(new ServerUserMaterial() { index = material.Value.mat_id, count = GetCount(material.Value.mat_id) });
                    break;
                case 2:
                    superMaterials.Add(new ServerUserMaterial() { index = material.Value.mat_id, count = GetCount(material.Value.mat_id) });
                    break;
                default:
                    break;
            }
        }

        if(currentMode == Mode.NormalToRare)
        {
            SetMaterials(subGradeGridRoot, subGradeMaterialItems, normalMaterials, SelectNormalItem);
            SetMaterials(upperGradeGridRoot, upperGradeMaterialItems, rareMaterials, SelectUpperGradeItem);
        }
        else if(currentMode == Mode.RareToSuper)
        {
            SetMaterials(subGradeGridRoot, subGradeMaterialItems, rareMaterials, SelectNormalItem);
            SetMaterials(upperGradeGridRoot, upperGradeMaterialItems, superMaterials, SelectUpperGradeItem);
        }

        tabExplanation.text = Global._instance.GetString("p_bs_10").Replace("[n]", GetSubCount(1).ToString());
    }

    private bool IsMaterialTimeOver(int materialId)
    {
        int endTs = ServerContents.MaterialMeta[materialId].expireTs;

        if (endTs == 0)
        {
            return false;
        }
        else
        {
            return Global.LeftTime(endTs) <= 0;
        }
    }

    private int GetCount(int materialIndex)
    {
        int count = 0;

        foreach(var material in ServerRepos.UserMaterials)
        {
            if(material.index == materialIndex)
            {
                count = material.count;
                break;
            }
        }

        return count;
    }

    private void SetMaterials(UIGrid root, List<UIItemExchangeMaterial> targetList, List<ServerUserMaterial> materialsData, System.Action<UIItemExchangeMaterial> selectEvent)
    {
        if(targetList.Count > materialsData.Count)
        {
            int deleteCount = targetList.Count - materialsData.Count;

            for (int i=0; i<deleteCount; i++)
            {
                Destroy(targetList[targetList.Count - 1].gameObject);
                targetList.RemoveAt(targetList.Count - 1);
            }
        }
        else if(targetList.Count < materialsData.Count)
        {
            int insertCount = materialsData.Count - targetList.Count;

            for (int i=0; i<insertCount; i++)
            {
                var item = Instantiate(ExchangeMaterial, root.transform).GetComponent<UIItemExchangeMaterial>();

                item.CenterOnEvent += selectEvent;

                targetList.Add(item);
            }
        }
        
        for(int i=0; i<targetList.Count; i++)
        {
            targetList[i].InitMaterial(materialsData[i]);
        }

        root.enabled = true;
    }

    private void SelectNormalItem(UIItemExchangeMaterial item)
    {
        selectsubGradeMaterial = item;

        UpperGradeCount = 1;

        string materialName = Global._instance.GetString($"mt_{item.GetMaterial().index}");

        subName.text = materialName.Replace("\n", "");
    }

    private void SelectUpperGradeItem(UIItemExchangeMaterial item)
    {
        selectUpperGradeMaterial = item;

        string materialName = Global._instance.GetString($"mt_{item.GetMaterial().index}");

        upperName.text = materialName.Replace("\n", "");
    }

    public void MaterialExchange()
    {
        if (ServerContents.MaterialExchangeRate == null || ServerContents.MaterialExchangeRate.Count <= 0)
        {
            return;
        }
        
        if(selectsubGradeMaterial.GetCount() >= SubGradeCount)
        {
            if (IsMaterialTimeOver(selectsubGradeMaterial.GetMaterial().index) || 
                IsMaterialTimeOver(selectUpperGradeMaterial.GetMaterial().index))
            {
                ManagerUI._instance.OpenPopup<UIPopupSystem>
                (
                    (popup) =>
                    { 
                        var title = Global._instance.GetString("p_t_4");
                        var main = Global._instance.GetString("n_s_42");
                        popup.InitSystemPopUp(title, main, false);
                    }
                );

                StartCoroutine(RePaintAndReScroll());
            }
            else
            {
                ManagerUI._instance.OpenPopup<UIPopupMaterialExchangeConfirmation>
                (
                    (popup) =>
                    {
                        popup.InitPopup
                        (
                            selectsubGradeMaterial.GetMaterial(),
                            selectUpperGradeMaterial.GetMaterial(),
                            UpperGradeCount
                        );
                    }
                );
            }
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>
            (
                (popup) =>
                {
                    string title = Global._instance.GetString("p_t_4");
                    string main = Global._instance.GetString("n_s_40");
                    popup.InitSystemPopUp(title, main, false);
                }
            );
        }
    }
    

    public int GetSubCount(int upperCount)
    {
        if(currentMode == Mode.NormalToRare)
        {
            return ServerContents.MaterialExchangeRate[0] * upperCount;
        }
        if(currentMode == Mode.RareToSuper)
        {
            return ServerContents.MaterialExchangeRate[1] * upperCount;
        }

        return 0;
    }

    public void RePaint()
    {
        InitData();
        UpperGradeCount = 1;
    }

    private void ResetScroll()
    {
        var subPos = subScrollPanel.transform.localPosition;
        subPos.x = 0.0f;
        subScrollPanel.transform.localPosition = subPos;
        subScrollPanel.clipOffset = Vector2.zero;

        var upperPos = upperScrollPanel.transform.localPosition;
        upperPos.x = 0.0f;
        upperScrollPanel.transform.localPosition = upperPos;
        upperScrollPanel.clipOffset = Vector2.zero;

        subCenterControl.Recenter();
        upperCenterControl.Recenter();
    }

    public void ExchangeCountDecrease()
    {
        int count = UpperGradeCount - 1;

        if (count > 0)
        {
            UpperGradeCount = count;
            StartCoroutine(CountChangeAni(upperGradeCount.transform));
            StartCoroutine(CountChangeAni(subGradeCount.transform));
        }
    }

    public void ExchangeCountIncrease()
    {
        int count = UpperGradeCount + 1;
        int subCount = GetSubCount(count);

        if (selectsubGradeMaterial.GetCount() >= subCount)
        {
            UpperGradeCount = count;
            StartCoroutine(CountChangeAni(upperGradeCount.transform));
            StartCoroutine(CountChangeAni(subGradeCount.transform));
        }
    }

    private List<Transform> targetList = new List<Transform>();
    private IEnumerator CountChangeAni(Transform target)
    {
        if (targetList.Contains(target)) yield break;

        targetList.Add(target);

        const float animationTime = 0.2f;
        const float startScale = 1.0f;
        const float maxScale = 1.2f;

        float totalTime = 0.0f;

        while (totalTime <= animationTime)
        {
            totalTime += Global.deltaTimeLobby;

            float time = Mathf.Min(totalTime, animationTime) / animationTime;
            float sin = Mathf.Sin(time * Mathf.PI);
            float scale = Mathf.Lerp(startScale, maxScale, sin);

            target.localScale = Vector3.one * scale;

            yield return null;
        }

        targetList.Remove(target);
    }

    public void Tab_NormalToRare()
    {
        if (currentMode == Mode.NormalToRare) return;

        currentMode = Mode.NormalToRare;

        normalToRareOn.SetActive(true);
        normalToRareOff.SetActive(false);
        rareToSuperOn.SetActive(false);
        rareToSuperOff.SetActive(true);

        StartCoroutine(RePaintAndReScroll());
    }

    public void Tab_RareToSuper()
    {
        if (currentMode == Mode.RareToSuper) return;

        currentMode = Mode.RareToSuper;

        normalToRareOn.SetActive(false);
        normalToRareOff.SetActive(true);
        rareToSuperOn.SetActive(true);
        rareToSuperOff.SetActive(false);

        StartCoroutine(RePaintAndReScroll());
    }

    private IEnumerator RePaintAndReScroll()
    {
        RePaint();

        yield return null;

        ResetScroll();

        yield return null;

        SelectUpperGradeItem(selectUpperGradeMaterial);
        SelectNormalItem(selectsubGradeMaterial);
    }
}
