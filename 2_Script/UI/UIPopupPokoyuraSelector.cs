using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupPokoyuraSelector : UIPopupBase {
    public static UIPopupPokoyuraSelector _instance;
    static public List<int> prevPokoyuraList = new List<int>();
    private List<int> pokoyuraList = new List<int>();
    public UIScrollView pokoyuraScroll;
    [SerializeField]
    UILabel labelTitle;
    [SerializeField]
    UILabel labelEmpty;
    [SerializeField]
    UILabel labelCount;

    [SerializeField] GameObject _objListItemLine = null;

    List<int> orgSetting = null;
    bool exposeSimple = false;

    private void Awake()
    {
        _instance = this;
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        pokoyuraScroll.panel.depth = uiPanel.depth + 2;
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        base.ClosePopUp(_mainTime, callback);

        if (_instance == this)
            _instance = null;
    }
    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            pokoyuraScroll.panel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            pokoyuraScroll.panel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void Init(bool exposeSimple)
    {
        this.exposeSimple = exposeSimple;
        LoadPrevPokoyura();

        pokoyuraList = new List<int>();

        List<int> newPokoYura       = new List<int>();
        List<int> installedPokoYura = new List<int>();
        
        for (int i = 0; i < ManagerData._instance._pokoyuraData.Count; ++i)
        {
            if (prevPokoyuraList.Exists(x => x == ManagerData._instance._pokoyuraData[i].index) == false)
            {
                newPokoYura.Add(ManagerData._instance._pokoyuraData[i].index);
            }
        }

        for (int i = 0; i < ManagerData._instance._pokoyuraData.Count; ++i)
        {
            if (prevPokoyuraList.Exists(x => x == ManagerData._instance._pokoyuraData[i].index))
            {
                installedPokoYura.Add(ManagerData._instance._pokoyuraData[i].index);
            }
        }

        if (Pokoyura.pokoyuraDeployCustom == null)
        {
            int spawnPokoYuraCount = ManagerLobby._instance._spawnPokogoroPosition.Length;
            
            Pokoyura.pokoyuraDeployCustom = new List<int>();
            foreach (var pokoyura in installedPokoYura)
            {
                if (spawnPokoYuraCount <= 0) break;

                spawnPokoYuraCount--;
                Pokoyura.pokoyuraDeployCustom.Add(pokoyura);
            }
            
            foreach (var pokoyura in newPokoYura)
            {
                if (spawnPokoYuraCount <= 0) break;

                spawnPokoYuraCount--;
                Pokoyura.pokoyuraDeployCustom.Add(pokoyura);
            }
        }
        else
        {
            for (int i = 0; i < ManagerLobby._instance._spawnPokogoroPosition.Length; ++i)
            {
                if( i >= Pokoyura.pokoyuraDeployCustom.Count)
                {
                    Pokoyura.pokoyuraDeployCustom.Add(0);
                }
            }
            orgSetting = new List<int>(Pokoyura.pokoyuraDeployCustom);
        }

        pokoyuraList.AddRange(newPokoYura);
        pokoyuraList.AddRange(installedPokoYura);
        
        this.labelTitle.text = Global._instance.GetString("p_yura_1");
        labelTitle.gameObject.SetActive(pokoyuraList.Count != 0);

        this.labelEmpty.text = Global._instance.GetString("p_yura_2");
        labelEmpty.gameObject.SetActive(pokoyuraList.Count == 0);

        MakePokoYuraList();
        pokoyuraScroll.ResetPosition();
        SavePrevPokoyura();

        StartCoroutine(CoUpdateSelectedCount());
    }
	
	// Update is called once per frame
	void Update () {
        if (ManagerUI._instance._popupList.Count > 1)
            ManagerUI._instance.ClosePopUpUI(this);
	}

    IEnumerator CoUpdateSelectedCount()
    {
        int count = 0;
        int maxCount = ManagerLobby._instance._spawnPokogoroPosition.Length;
        labelCount.text = string.Format("{0} / {1}", count, maxCount);
        while (true)
        {
            maxCount = ManagerLobby._instance._spawnPokogoroPosition.Length;
            int newcount = CountSelectedPokoyura();

            if( newcount != count )
            {
                count = newcount;
                labelCount.text = string.Format("{0} / {1}", count, maxCount);
            }
            
            yield return new WaitForSeconds(0.1f);

        }
    }

    int CountSelectedPokoyura()
    {
        int count = 0;
        int maxCount = ManagerLobby._instance._spawnPokogoroPosition.Length;
        if (Pokoyura.pokoyuraDeployCustom != null)
        {
            for (int i = 0; i < Pokoyura.pokoyuraDeployCustom.Count; ++i)
            {
                if (Pokoyura.pokoyuraDeployCustom[i] != 0)
                    count++;
            }
        }
        return count;
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        Pokoyura.pokoyuraDeployCustom = orgSetting;
        ManagerLobby._instance.ReMakePokoyura();

        bCanTouch = false;
        ManagerUI._instance.ClosePopUpUI();
    }

    void OnClickSubmit()
    {
        if (bCanTouch == false)
            return;

        Pokoyura.SavePokoyuraDeploy();
        ManagerUI._instance.ClosePopUpUI();

        if( CountSelectedPokoyura() != 0)
        {
            if (exposeSimple)
            {
                ManagerLobby._instance.ExposePokoyuraTree();
                //c._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);
            }
            else
            {
                ManagerLobby._instance.ResetTriggerWakeUp("Extend_pokoura");
                ManagerLobby._instance.PlayTriggerWakeUp("Extend_pokoura");
            }
        }
    }

    private void MakePokoYuraList()
    {
        const int yuraPerLine = 5;
        int pokoyuraLineCount = pokoyuraList.Count / yuraPerLine + (pokoyuraList.Count % yuraPerLine > 0 ? 1 : 0);

        int nCount = pokoyuraList.Count;
        for (int i = 0; i < pokoyuraLineCount; i++)
        {
            //현재 생성할 데코 아이템 데이터 리스트 생성.
            List<int> pList = new List<int>();
            for (int j = (i * yuraPerLine); j < (i * yuraPerLine) + yuraPerLine; j++)
            {
                //리스트 끝까지 왔으면 종료.
                if (j >= nCount)
                    break;
                pList.Add(pokoyuraList[j]);
            }
            //리스트 오브젝트 생성 & 값 세팅.
            UIItemPokoYuraSelectorList itemPokoYuraList =
              NGUITools.AddChild(pokoyuraScroll.gameObject, _objListItemLine).GetComponent<UIItemPokoYuraSelectorList>();
            itemPokoYuraList.transform.localPosition = new Vector3(0, -95f * i, 0f);
            
            itemPokoYuraList.SettingPokoYuraList(pList, pokoyuraScroll);
        }
    }

    public class PokoyuraHaveJsonData
    {
        public List<int> PokoyuraList = new List<int>();
    }

    public void LoadPrevPokoyura()
    {
        if (PlayerPrefs.HasKey("PokoyuraLastHave") == false)
            return;

        string saved = PlayerPrefs.GetString("PokoyuraLastHave");

        try
        {
            PokoyuraHaveJsonData loaded = JsonUtility.FromJson<PokoyuraHaveJsonData>(saved);
            prevPokoyuraList = loaded.PokoyuraList;
        }
        catch (System.Exception e)
        {

        }

    }

    public void SavePrevPokoyura()
    {
        if (pokoyuraList == null || pokoyuraList.Count == 0)
        {
            PlayerPrefs.DeleteKey("PokoyuraLastHave");

            return;
        }


        try
        {
            PokoyuraHaveJsonData saveData = new PokoyuraHaveJsonData() { PokoyuraList = pokoyuraList };
            string data = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("PokoyuraLastHave", data);
        }
        catch (System.Exception e)
        {

        }

    }
}
