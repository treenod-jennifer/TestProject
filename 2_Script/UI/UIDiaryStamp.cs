using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using JsonFx.Json;
using System.Linq;

[System.Serializable]
public class StampJsonData
{
    public string text = "";                  // fileEditText
    public Vector3 textLocalPosition;          // textEditPosition
    public Vector3 textLocalRotation;          // textEditRotation
    public int textSize = 28;                   // textSize
    public Color fontColor = Color.white;                  // textFontColor
    public int textWidgetWidth = 256;                // textWidgetWidth;
    public int textWidgetHeight = 32;               // textWidgetHeight;

    public StampJsonData ( Stamp _stamp )
   {
        this.text = _stamp.TextOrKey;                  // fileEditText
        this.textLocalPosition = _stamp.textLocalPosition;          // textEditPosition
        this.textLocalRotation = _stamp.textLocalRotation;          // textEditRotation
        this.textSize = _stamp.textSize;                   // textSize
        this.fontColor = _stamp.fontColor;                  // textFontColor
        this.textWidgetWidth = _stamp.textWidgetWidth;                // textWidgetWidth;
        this.textWidgetHeight = _stamp.textWidgetHeight;               // textWidgetHeight;
   }

    public void ConvertJsonData ( StampJsonData jsonData, Stamp stampData )
    {
        stampData.TextOrKey = jsonData.text;                  // fileEditText
        stampData.textLocalPosition = jsonData.textLocalPosition;          // textEditPosition
        stampData.textLocalRotation = jsonData.textLocalRotation;          // textEditRotation
        stampData.textSize = jsonData.textSize;                   // textSize
        stampData.fontColor = jsonData.fontColor;                  // textFontColor
        stampData.textWidgetWidth = jsonData.textWidgetWidth;                // textWidgetWidth;
        stampData.textWidgetHeight = jsonData.textWidgetHeight;               // textWidgetHeight;
    }
}

public class StampIndexJsonData
{
    public List<int> stampIdxList = new List<int>();

    public StampIndexJsonData(List<int> index)
    {
        this.stampIdxList = index;
    }

    public void ConvertJsonData(StampIndexJsonData jsonData, List<int> indexList)
    {
        for (int i = 0; i < jsonData.stampIdxList.Count; i++)
        {
            indexList.Add(jsonData.stampIdxList[i]);
        }
    }
}

public class UIDiaryStamp : UIDiaryBase
{
    public static UIDiaryStamp _instance = null;
    public UIPanel uiPanel;
    public UILabel emptyText;
    public UIItemStampEdit _objStamp;
    public GameObject _objScreenCaptrue;
    List<UIItemStampEdit> stampItemList = new List<UIItemStampEdit>();
    public SortedDictionary<int, Stamp> _stamp = new SortedDictionary<int, Stamp>();
    
    public static Dictionary<string, GameObject> _assetBank = new Dictionary<string, GameObject>();

    void Awake()
    {
        _instance = this;
    }

    IEnumerator Start ()
    {
        Stamp stamp = null;
        GameObject bundleObject = null;

        if (_stamp.Count == 0)
        {
            for (int i = 0; i < ServerRepos.UserStamps.Count; i++)
            {
                ServerUserStamp item = ServerRepos.UserStamps[i];
                if (item.index <= 0)
                    continue;

                string bundleName = "s_" + item.index;
                string name = "s_" + item.index + (item.type == 1 ? string.Format("_{0}", item.grade) : "");    // 번들 내의 프리팹 네임

                if (Global.LoadFromInternal)
                {
#if UNITY_EDITOR
                    string path = "Assets/5_OutResource/stamps/stamp_" + item.index + "/" + name + ".prefab";
                    bundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    bundleObject = Instantiate(bundleObject);
                    stamp = bundleObject.GetComponent<Stamp>();
#endif
                }
                else
                {
                    if (!_assetBank.ContainsKey(name))
                    {
                        //   string url = ManagerAssetBundle.GetInstance().GetVariantPackageUri(name, "");
                        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
                        while (e.MoveNext())
                            yield return e.Current;
                        if (e.Current != null)
                        {
                            AssetBundle assetBundle = e.Current as AssetBundle;

                            if (assetBundle != null)
                            {
                                bundleObject = assetBundle.LoadAsset<GameObject>(name);
                                stamp = Instantiate(bundleObject).GetComponent<Stamp>();
                                _assetBank.Add(name, bundleObject);
                            }
                        }
                    }
                    else
                    {
                        bundleObject = _assetBank[name];
                        stamp = Instantiate(bundleObject).GetComponent<Stamp>();
                    }
                }

                if (stamp != null)
                {
                    //스탬프 시간제한이 없거나, 시작시간 후이고 아직 끝나는 시간이 아닐 때 리스트에 데이터 추가.
                    if ((stamp.startTime == 0 && stamp.endTime == 0) ||
                        ((stamp.startTime > 0 && stamp.endTime > 0) && (stamp.startTime < Global.GetTime() && stamp.endTime > Global.GetTime())))
                    {
                        _stamp.Add(item.index, stamp);
                    }
                }

            }
        }

        // 데이터 읽어옴
        this.JsonRead();

        //playerPrefs에 저장된 스탬프 인덱스 정보를 읽어옴.
        List<int> stampIndexJsonList = new List<int>();
        this.ReadStampPlayerPrefs(stampIndexJsonList);
        bool bDataChange = _stamp.Count != stampIndexJsonList.Count;

        if (_stamp.Count > 0)
        {
            emptyText.gameObject.SetActive(false);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_3");
        }

        int stampCount = 0;
        List<int> stampIndexList = new List<int>();
        foreach (var item in _stamp)
        {
            UIItemStampEdit stampItem = NGUITools.AddChild(uiPanel.gameObject, _objStamp.gameObject).GetComponent<UIItemStampEdit>();
            bool bNew = CheckStampPlayerPrefs(stampIndexJsonList, item.Key);
            stampItem.InitData(item.Value, item.Key, stampCount, bNew, this.ReloadData);
            stampItemList.Add(stampItem);
            stampCount++;
            //스탬프 데이터 변경된 값이 있으면 저장.
            if (bDataChange == true)
            {
                stampIndexList.Add(item.Key);
            }
        }
        SetStampPosition();
        //새로 갱신된 스탬프 정보가 있으면 playerPrefs에 값 저장.
        if(bDataChange == true)
        {
            SaveStampPlayerPrefs(stampIndexList);
        }
    }

    public GameObject GetBtnEditButton()
    {
        if (stampItemList.Count <= 0)
            return null;
        if (stampItemList[0] == null)
            return null;
        if(stampItemList[0].GetBtnEdit() == null)
            return null;
        return stampItemList[0].GetBtnEdit();
    }

    private void ReadStampPlayerPrefs(List<int> jsonData)
    {
        if (PlayerPrefs.HasKey("Stamp"))
        {
            string jsonStr = PlayerPrefs.GetString("Stamp");
            StampIndexJsonData readData = JsonUtility.FromJson<StampIndexJsonData>(jsonStr);
            readData.ConvertJsonData(readData, jsonData);
        }
    }

    private bool CheckStampPlayerPrefs(List<int> stampIdxList, int stampIdx)
    {
        for (int i = 0; i < stampIdxList.Count; i++)
        {
            if (stampIdxList[i] == stampIdx)
                return false;
        }
        return true;
    }

    private void SaveStampPlayerPrefs(List<int> stampIdxList)
    {
        StampIndexJsonData saveData = new StampIndexJsonData(stampIdxList);
        string jsonStr = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("Stamp", jsonStr);
    }

    void SetStampPosition()
    {
        float xPos = -183f;
        float yPos = 325f;

        for (int i = 0; i < stampItemList.Count; i++)
        {
            if (i == 0 || (i > 1 && i % 2 == 0))
            {
                xPos = -183f;
            }
            else
            {
                xPos = 138f;
            }
            stampItemList[i].transform.localPosition = new Vector3(xPos, yPos, 0);

            if(i > 0 && (i == 1 || (i % 2 == 1)))
                yPos -= 300f;
        }
    }

    private void JsonRead ()
    {
        foreach (var item in _stamp)
        {
            string key = string.Format("stamp_{0}", item.Key);

            if ( PlayerPrefs.HasKey( key ) )
            {
                string jsonStr = PlayerPrefs.GetString( key );

                StampJsonData jsonData = JsonUtility.FromJson<StampJsonData>( jsonStr );
                jsonData.ConvertJsonData( jsonData, item.Value );
            }
        }
    }

    private void JsonRead(string key, int dataIndex, int in_listIndex)
    {
        if ( PlayerPrefs.HasKey( key ) )
        {
            string jsonStr = PlayerPrefs.GetString( key );

            StampJsonData jsonData = JsonUtility.FromJson<StampJsonData>( jsonStr );
            jsonData.ConvertJsonData( jsonData, this._stamp[dataIndex] );
            this.stampItemList[in_listIndex].itemStamp.InitData(this._stamp[dataIndex]);
        }
    }


    private void ReloadData ( string key, int dataIndex ,int in_listIndex )
    {
        this.JsonRead(key, dataIndex, in_listIndex);
    }
}
