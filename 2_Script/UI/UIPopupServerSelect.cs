using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class UIPopupServerSelect : UIPopupBase
{
    private class DCMap
    {
        [Newtonsoft.Json.JsonProperty("DCList")]
        public Dictionary<string, string> dcList = new Dictionary<string, string>();
    }
    
    public class ServerUrl
    {
        public int idx;
        public string desc;
        public string cdnUrl;
        public string gameUrl;
    }
    
    private const string MENU_FORCELOADBUNDLE_PATH = "Tools/ForceLoadBundle";
    
    public static int targetUID = -1;
    public static int dcServerNumber;
    
    
    // Url List
    List<ServerUrl> serverUrlList;
    
    // Game Server
    [SerializeField] UIPopupList serverList;
    [SerializeField] UILabel lbl_gameServer; 
    Dictionary<string, string> gameServerMap = new Dictionary<string, string>();
    
    // CDN Server
    [SerializeField] UIPopupList cdnList;
    [SerializeField] UILabel lbl_cdnServer;
    Dictionary<string, string> cdnServerMap = new Dictionary<string, string>();
    
    // DC Server
    [SerializeField] UIPopupList_DCSelect dcServerList;
    Dictionary<string, string> dcServerMap = new Dictionary<string, string>();
    
    // UID
    [SerializeField] GameObject rootUID;
    [SerializeField] UIInput inputKeyBoard;
    
    //Tutorial
    [SerializeField] GameObject tutorial_check;
    
    [SerializeField] UILabel loadingStatusLabel;
    string selectServerName;
    string selectServerUrl;

    private bool isDataLoadComplete;
    private bool isCanClick = true;
    
    IEnumerator Start ()
    {
        tutorial_check.gameObject.SetActive(!Global._optionTutorialOn);
        
#if !UNITY_EDITOR
        rootUID.SetActive(false);
#endif
        loadingStatusLabel.text = "URL 리스트 로드중...";
        if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LevelTeamServer)
        {
            yield return CoLoadLevelUrl();
        }
        else
        {
            yield return CoLoadUrl();
        }
        
        loadingStatusLabel.text = "URL 리스트 로드 완료";
        
        LoadEditorData();
        isDataLoadComplete = true;
    }

    IEnumerator CoLoadUrl()
    {
        string path = "https://lgpkv-launcher.lt.treenod.com/";
        using( var www = UnityWebRequest.Get(path) )
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                if (www.downloadHandler.text.Length > 0)
                {
                    var datas = JsonConvert.DeserializeObject<List<ServerUrl>>(www.downloadHandler.text);
                    if (datas != null && datas.Count > 0)
                    {
                        serverUrlList= datas.OrderBy(x => x.idx).ToList();
                        
                        foreach (var data in serverUrlList)
                        {
                            if (!string.IsNullOrEmpty(data.gameUrl))
                            {
                                serverList.AddItem(data.desc);
                                gameServerMap.Add(data.desc, data.gameUrl);
                            }

                            if (!string.IsNullOrEmpty(data.cdnUrl))
                            {
                                cdnList.AddItem(data.desc);
                                cdnServerMap.Add(data.desc, data.cdnUrl);
                            }
                        }

                        yield break;
                    }
                }
            }
            
            yield break;
        }
    }

    IEnumerator CoLoadLevelUrl()
    {
        serverList.Clear();
        cdnList.Clear();

        while (true)
        {
            string path = "https://lgpkv-launcher.lt.treenod.com/6";
            using (var www = UnityWebRequest.Get(path))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");

                yield return www.SendWebRequest();

                if (!www.IsError() && www.downloadHandler != null)
                {
                    if (www.downloadHandler.text.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<ServerUrl>(www.downloadHandler.text);
                        if (data != null)
                        {
                            if (!string.IsNullOrEmpty(data.gameUrl))
                            {
                                serverList.AddItem("LevelTeam Server");
                                gameServerMap.Add("LevelTeam Server", data.gameUrl);
                            }

                            if (!string.IsNullOrEmpty(data.cdnUrl))
                            {
                                cdnList.AddItem("LevelTeam Server");
                                cdnServerMap.Add("LevelTeam Server", data.cdnUrl);
                            }
                            break;
                        }
                    }
                }
            }
            loadingStatusLabel.text = "URL 리스트 로드 실패 재시도...";
        }
        yield break;
    }

    // DC Load
    IEnumerator CoServerDCSelect(string selectServerUrl, string selectServerName)
    {
        loadingStatusLabel.text = $"{selectServerName} DC 리스트 로드중...";
        
        string uri = selectServerUrl + "/auth/server_dc_select";
        
        var form = new Dictionary<string, string>
        {
            {"url", selectServerUrl},
            {"serverName", selectServerName}
        };
        
        using (UnityWebRequest request = UnityWebRequest.Post(new System.Uri(uri), form))
        {
            NetworkSettings.PreprocessCert(request);

            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.chunkedTransfer = false;
            request.useHttpContinue = false;

            request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            request.SetRequestHeader("Pragma", "no-cache");

            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                dcServerList.Clear();
                dcServerList.Set("0.0.0");
                
                loadingStatusLabel.text = $"{selectServerName} DC 리스트 로드 실패";
                isCanClick = true;
                yield break;
            }
            else
            {
                string dcResult = request.downloadHandler.text;

                if (dcResult == "404 page not found" || string.IsNullOrEmpty(dcResult))
                {
                    dcServerList.Clear();
                    dcServerList.Set("0.0.0");
                    
                    loadingStatusLabel.text = $"{selectServerName} DC 리스트 로드 실패";
                    isCanClick = true;
                    yield break;
                }

                var DCList = JsonConvert.DeserializeObject<DCMap>(dcResult); //dc리스트 값 저장.
                
                dcServerList.transform.parent.gameObject.SetActive(DCList.dcList.Count > 0);

                if (DCList.dcList.Count > 0)
                {
                    dcServerMap.Clear();
                    dcServerList.Clear();

                    foreach (var item in DCList.dcList)
                    {
                        dcServerMap.Add(item.Key, item.Value);
                        dcServerList.AddItem(item.Key);
                    }
                }

                string dcServerName = PlayerPrefs.GetString("DcServerName");
                if (dcServerList.items.Contains(NetworkSettings.Instance.gameAppVersion))
                    dcServerList.Set(NetworkSettings.Instance.gameAppVersion);
                else if (!string.IsNullOrEmpty(dcServerName) && dcServerList.items.Contains(dcServerName))
                    dcServerList.Set(dcServerName);
                else
                    dcServerList.Set("0.0.0");
            }
        }
        loadingStatusLabel.text = $"{selectServerName} DC 리스트 로드 완료";
        isCanClick = true;
    }
    
    public void OnServerPopupListChanged()
    {
        if (UIPopupList.current != null)
        {
            string text = UIPopupList.current.value;
            ServerPopupListChangeHandler(text);
        }
    }
    
    private void ServerPopupListChangeHandler(string text)
    {
        if (isCanClick && gameServerMap.TryGetValue(text, out var addr))
        {
            serverList.Set(text);
            lbl_gameServer.text = addr;
            selectServerName = text;

            if (NetworkSettings.Instance.serverTarget != NetworkSettings.ServerTargets.LevelTeamServer)
            {
                isCanClick = false;
                
                // 선택한 서버에 존재하는 DC 리스트를 불러옴.
                selectServerUrl = lbl_gameServer.text;
                StartCoroutine(CoServerDCSelect(selectServerUrl, selectServerName));
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"선택한 {text} Game Server 데이터가 없습니다.");
#endif
        }
    }

    public void OnCDNPopupListChanged()
    {
        if (UIPopupList.current != null)
        {
            string text = UIPopupList.current.value;
            CDNPopupListChangeHandler(text);
        }
    }

    private void CDNPopupListChangeHandler(string text)
    {
        if (cdnServerMap.TryGetValue(text, out var addr))
        {
            cdnList.Set(text);
            lbl_cdnServer.text = addr;
        }
        else
        {
            
#if UNITY_EDITOR
            Debug.Log($"선택한 {text} CDN Server 데이터가 없습니다.");
#endif
        }
    }

    private void OnClickBtnTutorialSkip()
    {
        Global._optionTutorialOn = !Global._optionTutorialOn;
        tutorial_check.SetActive(!Global._optionTutorialOn);
        PlayerPrefs.SetInt("_optionTutorialOn", (Global._optionTutorialOn == true) ? 1 : 0);
    }

    private void OnClickResetUID()
    {
        inputKeyBoard.value = "-1";
    }
    
    private void OnClickTextBox()
    {
        inputKeyBoard.isSelected = true;
    }
    
    protected void OnClickBtnConnectCustomDC()
    {
        if (!isDataLoadComplete) return;
        
        if(dcServerMap.TryGetValue(dcServerList.value, out string value))
        {
            dcServerNumber = int.Parse(value);
        }

        SetServerData();
        SaveEditorData();
        
        base.OnClickBtnClose();
    }

    protected void OnClickBtnConnectStandard()
    {
        if (!isDataLoadComplete) return; 
        
        dcServerNumber = 0;
        SetServerData();
        SaveEditorData();
        
        base.OnClickBtnClose();
    }
    
    #region DirectServerSettingButton

    private void OnClickRnD()
    {
        if (!isCanClick || !isDataLoadComplete) return;
        
        ServerPopupListChangeHandler("R&D Server");
        CDNPopupListChangeHandler("R&D Server");
    }
    private void OnClickDev()
    {
        if (!isCanClick || !isDataLoadComplete) return;
        
        ServerPopupListChangeHandler("Sandbox Server");
        CDNPopupListChangeHandler("Sandbox Server");
    }
    private void OnClickDev2()
    {
        if (!isCanClick || !isDataLoadComplete) return;
        
        ServerPopupListChangeHandler("Dev2 Server");
        CDNPopupListChangeHandler("Dev2 Server");
    }
    private void OnClickBuildQA()
    {
        if (!isCanClick || !isDataLoadComplete) return;
        
        ServerPopupListChangeHandler("Build QA Server");
        CDNPopupListChangeHandler("Build QA Server");
    }
    private void OnClickPreQA()
    {
        if (!isCanClick || !isDataLoadComplete) return;
        
        ServerPopupListChangeHandler("Pre QA Server");
        CDNPopupListChangeHandler("Pre QA Server");
    }
    #endregion
    
    private void LoadEditorData()
    {
        string serverName = PlayerPrefs.GetString("ServerName");
        string cdnName = PlayerPrefs.GetString("CDNName");
        string targetUid = PlayerPrefs.GetString("TargetUID");

        if (serverList.items.Count > 0)
        {
            if (!string.IsNullOrEmpty(serverName) && serverList.items.Contains(serverName))
            {
                serverList.Set(serverName);
            }
            else
            {
                serverList.Set(serverList.items[0]);
            }
        }


        if (cdnList.items.Count > 0)
        {
            if (!string.IsNullOrEmpty(cdnName) && cdnList.items.Contains(cdnName))
            {
                cdnList.Set(cdnName);
            }
            else
            {
                cdnList.Set(cdnList.items[0]);
            }
        }

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(targetUid))
        {
            inputKeyBoard.Set(targetUid);
            targetUID = int.Parse(targetUid);
        }
#endif
    }
    
    private void SaveEditorData()
    {
        PlayerPrefs.SetString("ServerName", serverList.value);
        PlayerPrefs.SetString("CDNName", cdnList.value);
        PlayerPrefs.SetString("DcServerName", dcServerList.value);
        PlayerPrefs.SetString("TargetUID", targetUID.ToString());
    }

    private void SetServerData()
    {
        Global._cdnAddress = lbl_cdnServer.text;
        Global._gameServerAddress = lbl_gameServer.text;
        
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(inputKeyBoard.value))
        {
            int.TryParse(inputKeyBoard.value, out targetUID);
        }
#endif
    }

    public static bool GetForceLoadBundleActive()
    {
#if UNITY_EDITOR
        string ForceLoadBundleData = UnityEditor.EditorUserSettings.GetConfigValue("ForceLoadBundle");

        bool check = ForceLoadBundleData == null ? false : bool.Parse(ForceLoadBundleData);

        return check;
#else
        return true;
#endif
    }

    #region Editor
    
#if UNITY_EDITOR
    [UnityEditor.MenuItem(MENU_FORCELOADBUNDLE_PATH, priority = 2001)]
    private static void ForceLoadBundleCheck()
    {
        bool check = !GetForceLoadBundleActive();
        UnityEditor.EditorUserSettings.SetConfigValue("ForceLoadBundle", check.ToString());
    }

    [UnityEditor.MenuItem(MENU_FORCELOADBUNDLE_PATH, true)]
    private static bool ForceLoadBundleToggle()
    {
        bool check = GetForceLoadBundleActive();
        UnityEditor.Menu.SetChecked(MENU_FORCELOADBUNDLE_PATH, check);

        return true;
    }
    
#endif
    #endregion
}
