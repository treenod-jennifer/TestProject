using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class UIPopupGenericPage : UIPopupBase
{
    bool refresh = false;

    bool isLocalization = false;

    [SerializeField]
    private UILabel title;

    [SerializeField]
    private UIPanel scrollPanel;
    [SerializeField]
    private GameObject bannerRoot;

    [SerializeField]
    private GameObject contentsBasis;

    [SerializeField] GameObject scrollBarObj;
    [SerializeField] UIScrollBar scrollBar;

    [SerializeField]
    private UIPokoButton btnClose;
    [SerializeField]
    private UIPokoButton btnBack;

    [SerializeField]
    private GameObject refButton;

    [SerializeField]
    private GameObject refGauge;

    [SerializeField]
    private UIScrollView draggablePanel;

    [SerializeField]
    UIFont[] font;

    enum ListObjType
    {
        IMAGE,
        TEXT,
        BLANK,
        BUTTON,
        QUESTGAUGE,
    }

    struct ListObject
    {
        public ListObjType type;
        public GameObject gameObject;
    }

    List<ListObject> objectList = new List<ListObject>();
    List<string> currentPageLink = new List<string>();
    Page currentPage = null;
    Stack<string> pageStack = new Stack<string>();


    class ListData
    {
        public ListObjType type;
        public int depth = 1;
    }

    interface TextureLoadable
    {
        string GetLoadImageFilename();
        void SetTexture(Texture tex);
    }
    class ImageData : ListData, TextureLoadable
    {
        public string imgFilename;
        public Texture texture;
        public Vector2? customSize;
        public List<LinkData> links = new List<LinkData>();

        public void SetTexture(Texture t) { texture = t;}
        public string GetLoadImageFilename() { return imgFilename;}
    }


    class FontData
    {
        public int fontIndex;   // 빌드상에 미리 등록되어있는 폰트 중에서 어느 걸 쓸지 고르는 번호

        public int fontSize = 20;
        public Color color = Color.white;
        public Color outLineColor = Color.black;
        public bool bold = false;
        public Vector2 spacing = new Vector2();
        internal bool haveOutline = true;
    }

    class ButtonData : ListData
    {
        public string spriteName_BG = "";
        public string buttonString = "";
        public Vector2 buttonSize = new Vector2(100, 50);

        public FontData fontData = new FontData();
        public LinkData link = new LinkData();
    }

    class QuestGaugeData : ListData, TextureLoadable
    {
        public int questIdx;
        public bool isEventQuest;
        
        public string CompleteImgBtn;
        public string bgColor;
        public string gaugeColor;
        public int gaugeWidth;
        public int gaugeHeight;
        public string fontColor;
        public string fontStrokeColor;
        public int fontSize;

        public Vector2 imgOffset;

        public bool loadSuccess;
        public int progress;
        public int targetCount;
        public int state;
        public string info;

        public Texture texture;
        public void SetTexture(Texture t) { texture = t; }
        public string GetLoadImageFilename() { return CompleteImgBtn; }
    }

    enum LinkType
    {
        LT_PAGE,        // 다른 페이지로 연결하는 링크
        LT_URL_LINK,    // OpenURL
        LT_COUPON_URL_LINK,
        LT_GAME_DEEP_LINK,
    }

    class LinkData
    {
        public LinkType linkType = LinkType.LT_PAGE;
        public Vector3 offset;
        public Vector3 size;
        public string text;
        public string linkName;
    }
    class BlankData : ListData
    {
        public int pixel;
    }


    class TextData : ListData
    {
        public FontData fontData;
        public string text;
        public UIWidget.Pivot pivot = UIWidget.Pivot.Top;
    }

    class Page
    {
        public bool isTopPage = false;
        public string title = "";
        public string pageName;
        public List<ListData> dataList = new List<ListData>();
    }

    Dictionary<string, Page> loadedPages = new Dictionary<string, Page>();

    string homePath = "";

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp(string homePath)
    {
        SetPageMode(0);
        this.homePath = homePath;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(VersionCheck());
        yield return StartCoroutine(LoadPage("index"));
        yield return StartCoroutine(OpenPage("index"));

        //this.draggablePanel.Scroll(-0.5f);
    }

    public void SaveData(byte[] byteData, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, homePath);
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
            dirInfo.Create();

        string targetPath = Application.persistentDataPath + "/" + homePath + "/" + fileName;

        FileStream fs = new FileStream(targetPath, FileMode.Create);
        fs.Seek(0, SeekOrigin.Begin);
        fs.Write(byteData, 0, byteData.Length);
        fs.Close();
    }

    IEnumerator VersionCheck()
    {
        string loadedXmlText = "";
        bool loaded = false;

        string localPath = @"file:///" + Application.persistentDataPath + "/" + homePath + "/index.xml";
        using( var www = UnityWebRequest.Get(localPath) )
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                if (www.downloadHandler.text.Length > 0)
                {
                    loadedXmlText = www.downloadHandler.text;
                    loaded = true;
                }
            }

        }
        
        

        if (loaded)
        {
            string url = Global._cdnAddress + homePath + "/index.xml";
            
            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();
                if (!www.IsError() && www.downloadHandler != null)
                {
                    if (www.downloadHandler.text.Length > 0)
                    {
                        if (www.downloadHandler.text.Equals(loadedXmlText))
                        {
                            refresh = false;
                            yield break;
                        }

                    }
                    else yield break;
                }
                else yield break;
            }
        }
        else
        {
            refresh = true;
            yield break;    // 어짜피 로컬에 받아놓은 게 없으니 모조리 새로받으면 된다
        }
        refresh = true;
    }

   
    IEnumerator LoadPage(string pageFilename, string parentPage = "")
    {
        if (this.loadedPages.ContainsKey(pageFilename))
            yield break;

        string localPath = Global.FileUri + Application.persistentDataPath + "/" + homePath + "/" + pageFilename + ".xml";
        //string loadedXmlText = "";
        //bool loaded = false;

        string loadedWWWText = "";
        if (!refresh)
        {
            using( UnityWebRequest www = UnityWebRequest.Get(localPath) )
            {
                yield return www.SendWebRequest();
                if (!www.IsError() && www.downloadHandler != null)
                {
                    if (www.downloadHandler.text.Length > 0)
                    {
                        loadedWWWText = www.downloadHandler.text;
                    }
                }

            }
            
        }

        if (string.IsNullOrEmpty(loadedWWWText))
        {
            string url = Global._cdnAddress + homePath + "/" + pageFilename + ".xml";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();
                if (!www.IsError() && www.downloadHandler != null)
                {
                    if (www.downloadHandler.text.Length > 0)
                    {
                        loadedWWWText = www.downloadHandler.text;
                        SaveData(www.downloadHandler.data, pageFilename + ".xml");
                    }
                    else yield break;
                }
                else
                    yield break;
            }
                
        }

        System.IO.StringReader stringReader = new System.IO.StringReader(loadedWWWText);
        stringReader.Read(); // skip BOM

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(stringReader.ReadToEnd());


        var page = new Page();
        page.isTopPage = parentPage.CompareTo("") == 0;
        page.pageName = pageFilename;

        XmlNode rootNode = doc.ChildNodes[1];

        
        if (rootNode.Attributes["localization"] != null && rootNode.Attributes["localization"].Value.ToLower() == "yes")
        {
            isLocalization = true;
        }
        else
        {
            isLocalization = false;
        }


        if (rootNode.Attributes["textFile"] != null)
        {
            yield return LoadTextFile(rootNode.Attributes["textFile"].Value);
        }


        if (rootNode.Attributes["title"] != null)
        {
            page.title = Global._instance.GetString(rootNode.Attributes["title"].Value);
        }   
        
        for (int i = 0; i < rootNode.ChildNodes.Count; ++i)
        {
            XmlNode node = rootNode.ChildNodes[i];
            bool linefeed = xmlhelper.GetBool(node, "linefeed", true);
            switch (node.Name.ToLower())
            {
                case "img":
                    {
                        ImageData img = new ImageData();
                        img.type = ListObjType.IMAGE;
                        img.imgFilename = node.Attributes["src"].Value;
                        if (node.Attributes["size"] != null)
                            img.customSize = xmlhelper.GetVector2(node, "size");
                        
                        img.depth = xmlhelper.GetInt(node, "depth");

                        yield return StartCoroutine(LoadTexture(img));

                        if (node.ChildNodes.Count > 0)
                        {
                            for (int j = 0; j < node.ChildNodes.Count; ++j)
                            {
                                XmlNode linkNode = node.ChildNodes[j];
                                if (linkNode.Name != "link")
                                    continue;

                                LinkData linkData = new LinkData();
                                ReadLinkData(linkNode, ref linkData);

                                if( linkData.linkType == LinkType.LT_PAGE)
                                    yield return StartCoroutine(LoadPage(linkData.linkName, pageFilename));

                                img.links.Add(linkData);
                            }
                        }

                        page.dataList.Add(img);
                    }
                    break;
                case "text":
                    {
                        TextData data = new TextData();
                        data.type = ListObjType.TEXT;

                        if( node.Attributes["text"] != null )
                        {
                            data.text = Global._instance.GetString(xmlhelper.GetString(node, "text", ""));
                        }
                        
                        if (node.Attributes["pivot"] != null)
                        {
                            data.pivot = (UIWidget.Pivot)Enum.Parse(typeof(UIWidget.Pivot), node.Attributes["pivot"].Value, true);
                        }
                        data.depth = xmlhelper.GetInt(node, "depth");


                        ReadFontData(node, ref data.fontData);

                        page.dataList.Add(data);
                    }
                    break;
                case "blank":
                    {
                        BlankData data = new BlankData()
                        {
                            type = ListObjType.BLANK,
                            pixel = Convert.ToInt32(node.Attributes["size"].Value)
                        };
                        page.dataList.Add(data);
                    }
                    break;
                case "button":
                    {
                        ButtonData data = new ButtonData()
                        {
                            type = ListObjType.BUTTON,
                        };
                        if( node.Attributes["text"] == null)
                            data.buttonString = Global._instance.GetString(xmlhelper.GetString(node, "textkey"));
                        else
                            data.buttonString = xmlhelper.GetString(node, "text");
                        data.buttonSize = xmlhelper.GetVector2(node, "size");

                        data.depth = xmlhelper.GetInt(node, "depth");

                        ReadFontData(node, ref data.fontData);
                        ReadLinkData(node, ref data.link);

                        page.dataList.Add(data);
                    }
                    break;
                case "questgauge":
                    {
                        QuestGaugeData data = new QuestGaugeData()
                        {
                            type = ListObjType.QUESTGAUGE,
                        };
                        data.questIdx = xmlhelper.GetInt(node, "questIdx");
                        data.isEventQuest = xmlhelper.GetBool(node, "isEventQuest");
                        data.CompleteImgBtn = xmlhelper.GetString(node, "completeImg");
                        data.gaugeColor = xmlhelper.GetString(node, "gaugeColor");
                        data.bgColor = xmlhelper.GetString(node, "bgColor");
                        data.gaugeWidth = xmlhelper.GetInt(node, "gaugeWidth");
                        data.gaugeHeight = xmlhelper.GetInt(node, "gaugeHeight");

                        data.depth = xmlhelper.GetInt(node, "depth");

                        data.fontStrokeColor = xmlhelper.GetString(node, "fontStrokeColor");
                        data.fontColor = xmlhelper.GetString(node, "fontColor");
                        data.fontSize = xmlhelper.GetInt(node, "fontSize");

                        if (data.isEventQuest)
                            yield return LoadEventQuestState(data);
                        else
                            yield return LoadQuestState( data);
                        yield return LoadTexture(data);
                        if (data.loadSuccess)
                            page.dataList.Add(data);
                        else
                        {

                        }

                    }
                    break;
            }

        }
        this.loadedPages.Add(pageFilename, page);

        yield break;
    }

    IEnumerator LoadQuestState(QuestGaugeData data)
    {
        bool waitUntil = false;
        ServerAPI.GetQuestState(new List<int>() { data.questIdx },
            (r) =>
            {
                if (r.IsSuccess)
                {
                    if (r.userQuests.Count > 0)
                    {
                        data.loadSuccess = true;
                        data.progress = r.userQuests[0].prog1;
                        data.targetCount = r.userQuests[0].targetCount;
                        data.info = r.userQuests[0].info;
                        data.state = r.userQuests[0].state;
                    }
                }
                waitUntil = true;
            });

        yield return new WaitUntil( () => {return waitUntil == true; } );
    }

    IEnumerator LoadEventQuestState(QuestGaugeData data)
    {
        bool waitUntil = false;
        ServerAPI.GetEventQuestState(new List<int>() { data.questIdx },
            (r) =>
            {
                if (r.IsSuccess)
                {
                    if (r.userQuests.Count > 0)
                    {
                        data.loadSuccess = true;
                        data.progress = r.userQuests[0].prog1;
                        data.targetCount = r.userQuests[0].targetCount;
                        data.info = r.userQuests[0].info;
                        data.state = r.userQuests[0].state;
                    }
                }
                waitUntil = true;
            });

        yield return new WaitUntil(() => { return waitUntil == true; });
    }

    void ReadFontData(XmlNode node, ref FontData fontData)
    {
        fontData = new FontData();

        fontData.fontIndex = 0;
        fontData.fontSize = xmlhelper.GetInt(node, "fontsize", 20);
        fontData.bold = xmlhelper.GetBool(node, "bold", false);
        fontData.color = xmlhelper.GetColor(node, "color");

        if (node.Attributes["outlineColor"] != null)
        {
            fontData.haveOutline = true;
            fontData.outLineColor = xmlhelper.GetColor(node, "outlineColor");
        }
        else fontData.haveOutline = false;
        fontData.spacing = xmlhelper.GetVector2(node, "spacing");
    }

    void ReadLinkData(XmlNode node, ref LinkData linkData)
    {
        linkData.linkName = xmlhelper.GetString(node, "ref");

        linkData.offset = xmlhelper.GetVector2(node, "offset");
        linkData.size = xmlhelper.GetVector2(node, "size");

        linkData.text = xmlhelper.GetString(node, "text");
        linkData.linkType = (LinkType)xmlhelper.GetInt(node, "linktype");

    }

    IEnumerator LoadTexture(TextureLoadable imgData)
    {
        string langPostfix = isLocalization ? $"_{LanguageUtility.SystemCountryCode}" : string.Empty;
        string localurl = Global.FileUri + Application.persistentDataPath + "/" + homePath + "/" + imgData.GetLoadImageFilename() + langPostfix + ".png";

        bool loadOK = false;
        if (!refresh)
        {
            using(var www = UnityWebRequestTexture.GetTexture(localurl))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();
                if (!www.IsError() && www.downloadHandler!= null)
                {
                    var t = www.downloadHandler as DownloadHandlerTexture;
                    if (t.data.Length > 0)
                    {
                        imgData.SetTexture(t.texture);
                        loadOK = true;
                    }
                }

            }
            
            
        }

        if (!loadOK)
        {
            string url = Global._cdnAddress + homePath + "/" + imgData.GetLoadImageFilename() + langPostfix + ".png";

            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();

                if (!www.IsError() && www.downloadHandler != null)
                {
                    var t = www.downloadHandler as DownloadHandlerTexture;
                    if (t.data.Length > 0)
                    {
                        imgData.SetTexture(t.texture);
                        loadOK = true;
                        SaveData(t.data, imgData.GetLoadImageFilename() + langPostfix + ".png");
                    }
                }
            }
        }

        yield break;
    }

    IEnumerator LoadTextFile(string textFileName)
    {
        string langPostfix = isLocalization ? $"_{LanguageUtility.SystemCountryCode}" : string.Empty;
        string textFullName = textFileName + langPostfix + ".json";
        string localurl = Global.FileUri + Application.persistentDataPath + "/" + homePath + "/" + textFullName;

        bool loadOK = false;
        if (!refresh)
        {
            using (var www = UnityWebRequest.Get(localurl))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();
                if (!www.IsError() && www.downloadHandler != null)
                {
                    var t = www.downloadHandler;
                    if (t.data.Length > 0)
                    {
                        StringHelper.LoadStringFromJson(t.text, ref Global._instance._stringData);
                        loadOK = true;
                    }
                }
            }
        }

        if (!loadOK)
        {
            string url = Global._cdnAddress + homePath + "/" + textFullName;

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                www.SetRequestHeader("Pragma", "no-cache");
                yield return www.SendWebRequest();

                if (!www.IsError() && www.downloadHandler != null)
                {
                    var t = www.downloadHandler;
                    if (t.data.Length > 0)
                    {
                        StringHelper.LoadStringFromJson(t.text, ref Global._instance._stringData);
                        loadOK = true;
                        SaveData(t.data, textFullName);
                    }
                }
            }
        }

        yield break;
    }

    IEnumerator OpenPage(string pageFilename, string callerPage = "")
    {
        ClearPage();

        currentPage = loadedPages[pageFilename];

        if (loadedPages[pageFilename].isTopPage)
            SetIndexPage();
        else
            SetContextPage();

        if (!callerPage.Equals(""))
        {
            pageStack.Push(callerPage);
        }

        if (currentPage.isTopPage)
        {
            pageStack.Clear();
        }

        if (this.pageStack.Count == 0)
        {
            SetIndexPage();
        }
        else SetContextPage();


        var titleLabels = this.title.gameObject.GetComponentsInChildren<UILabel>();
        string titleText = currentPage.title;
        for(int i = 0; i < titleLabels.Length; ++i)
        {
            titleLabels[i].text = titleText;
        }

        var dataList = this.loadedPages[pageFilename].dataList;
        for (int i = 0; i < dataList.Count; ++i)
        {
            switch (dataList[i].type)
            {
                case ListObjType.IMAGE:
                    {
                        var data = dataList[i] as ImageData;
                        if (data.texture == null)
                        {
                            
                            AddText("404: Texture Not Found", new FontData() { color = Color.red });
                            break;
                        }

                        GameObject imgObj = AddImage(data.texture, data.customSize, data.depth);
                        for (int j = 0; j < data.links.Count; ++j)
                        {
                            LinkData link = data.links[j];
                            AddLink(imgObj, link);
                        }
                    }
                    break;
                case ListObjType.TEXT:
                    {
                        var data = dataList[i] as TextData;
                        AddText(data.text, data.fontData, data.pivot, data.depth);
                    }
                    break;

                case ListObjType.BLANK:
                    {
                        var data = dataList[i] as BlankData;
                        AddBlank(data.pixel);
                    }
                    break;
                case ListObjType.BUTTON:
                    {
                        var data = dataList[i] as ButtonData;
                        AddButton(data);

                    }
                    break;
                case ListObjType.QUESTGAUGE:
                    {
                        var data = dataList[i] as QuestGaugeData;
                        AddQuestGauge(data);

                    }
                    break;

            }

        }
        StartCoroutine(ArrangePage());

        bCanTouch = true;
        yield break;
    }

    void ClearPage()
    {
        for (int i = 0; i < this.objectList.Count; ++i)
        {
            Destroy(objectList[i].gameObject);
        }
        objectList.Clear();
        currentPageLink.Clear();
    }

    IEnumerator ArrangePage()
    {
        yield return null;
        yield return null;

        int offset = 0;
        for (int i = 0; i < this.objectList.Count; ++i)
        {
            switch (this.objectList[i].type)
            {
                case ListObjType.IMAGE:
                    {
                        UITexture uiTex = objectList[i].gameObject.transform.Find("Image").GetComponent<UITexture>();
                        objectList[i].gameObject.transform.localPosition = new Vector3(0, offset, -5);

                        offset -= uiTex.height;
                    }
                    break;
                case ListObjType.TEXT:
                    {
                        UILabel uiFont = objectList[i].gameObject.GetComponent<UILabel>();
                        float xAnchorPos = 0.0f;
                        objectList[i].gameObject.transform.localPosition = new Vector3(xAnchorPos, offset, -5);

                        uiFont.height = (int)(uiFont.printedSize.y + 0.5f);

                        offset -= (int)(uiFont.printedSize.y + 0.5f);
                    }
                    break;

                case ListObjType.BLANK:
                    {
                        offset -= (int)(objectList[i].gameObject.transform.localScale.y);
                    }
                    break;
                case ListObjType.BUTTON:
                    {
                        objectList[i].gameObject.transform.localPosition = new Vector3(0, offset, -5);

                        UIPokoButton pkoButton = objectList[i].gameObject.GetComponent<UIPokoButton>();
                        BoxCollider col = objectList[i].gameObject.GetComponent<BoxCollider>();
                        offset -= (int)(col.size.y);
                    }
                    break;
                case ListObjType.QUESTGAUGE:
                    {
                        objectList[i].gameObject.transform.localPosition = new Vector3(0, offset, -5);
                        var uiTex = objectList[i].gameObject.GetComponent<UISprite>();
                        
                        offset -= (int)(uiTex.height);

                    }
                    break;

            }
        }

        this.draggablePanel.ResetPosition();
        this.draggablePanel.RestrictWithinBounds(false);
    }

    void AddLink(GameObject attachTarget, LinkData linkData)
    {
        GameObject newObj = NGUITools.AddChild(attachTarget);
        GameObject btnSpriteObj  = NGUITools.AddChild(newObj);
        btnSpriteObj.name = "btnSprite";

        newObj.name = "Link_" + linkData.linkName;

        var btnScript = newObj.AddComponent<UIPokoButton>();
        var btnCollider = newObj.AddComponent<BoxCollider>();

        btnScript._messageTarget = this.gameObject;
        btnScript.functionName = "OnClick_Link";
        btnScript.appendedObject = linkData;
        btnScript._bIsCanSkip = false;
        currentPageLink.Add(linkData.linkName);

        newObj.transform.localPosition = new Vector3(linkData.offset.x, linkData.offset.y, 0);
        btnCollider.size = linkData.size;

    }

    GameObject AddImage(Texture texture, Vector2? size, int depth)
    {
        GameObject newObj = NGUITools.AddChild(draggablePanel.gameObject);
        GameObject bgObj = NGUITools.AddChild(newObj);
        bgObj.name = "Image";

        UITexture newTexture = bgObj.AddComponent<UITexture>();
        newTexture.mainTexture = texture;
        newTexture.pivot = UIWidget.Pivot.Top;
        newTexture.shader = Shader.Find("Unlit/Transparent Colored (SoftClip)");
        newTexture.width = texture.width;
        newTexture.height= texture.height;
        newTexture.cachedTransform.localPosition = new Vector3(0, 0, -5);
        newTexture.depth = depth;

        if(size.HasValue)
        {
            newTexture.width = (int)size.Value.x;
            newTexture.height = (int)size.Value.y;
        }

        objectList.Add(new ListObject() { type = ListObjType.IMAGE, gameObject = newObj });
        return newObj;
    }

    GameObject AddText(string text, FontData fontData, UIWidget.Pivot pivot = UIWidget.Pivot.Top, int depth = 0)
    {
        GameObject newObj = NGUITools.AddChild(draggablePanel.gameObject);
        UILabel label = newObj.AddComponent<UILabel>();
        ApplyFontToLabel(ref label, fontData);
        label.width = 680;
        label.height = 2400;
        label.depth = depth;
        label.multiLine = true;
        label.cachedTransform.localPosition = new Vector3(0, 0, -5);
        label.text = text;

        objectList.Add(new ListObject() { type = ListObjType.TEXT, gameObject = newObj });
        return newObj;
    }

    void ApplyFontToLabel(ref UILabel label, FontData fontData)
    {
        label.bitmapFont = this.font[fontData.fontIndex];
        label.effectStyle = fontData.haveOutline ? UILabel.Effect.Outline8 : UILabel.Effect.None;
        label.effectDistance = new Vector2(2, 1);
        label.effectColor = fontData.outLineColor;
        label.fontSize = fontData.fontSize;
        label.color = fontData.color;
        label.spacingX = (int)fontData.spacing.x;
        label.spacingY = (int)fontData.spacing.y;
    }

    void AddBlank(int pixel)
    {
        GameObject newObj = NGUITools.AddChild(draggablePanel.gameObject);
        newObj.transform.localScale = new Vector3(0, pixel);

        objectList.Add(new ListObject() { type = ListObjType.BLANK, gameObject = newObj });
    }

    void AddButton(ButtonData data)
    {
        GameObject newObj = NGUITools.AddChild(draggablePanel.gameObject, this.refButton);
        UIPokoButton pokoBtn = newObj.GetComponent<UIPokoButton>();
        BoxCollider col = newObj.GetComponent<BoxCollider>();
        col.size = data.buttonSize;
        for( int i = 0; i < pokoBtn._colorSprites.Length; ++i)
        {
            pokoBtn._colorSprites[i].width = (int)(data.buttonSize.x + 0.5f);
            pokoBtn._colorSprites[i].height = (int)(data.buttonSize.y + 0.5f);
        }

        pokoBtn.functionName = "OnClick_Link";
        pokoBtn.appendedObject = data.link;
        //pokoBtn.SetLabel(Global._instance.GetString( data.buttonString), (x) => { this.ApplyFontToLabel(ref x, data.fontData); });
        pokoBtn.SetLabel(data.buttonString);

        objectList.Add(new ListObject() { type = ListObjType.BUTTON, gameObject = newObj });
    }

    void AddQuestGauge(QuestGaugeData data)
    {
        int depth = data.depth;
        GameObject newObj = NGUITools.AddChild(draggablePanel.gameObject, this.refGauge);
        UIProgressBar progBar = newObj.GetComponentInChildren<UIProgressBar>();
        var progText = newObj.GetComponentInChildren<UILabel>();

        Color fontColor, strokeColor;
        if (ColorUtility.TryParseHtmlString(data.fontColor, out fontColor))
            progText.color = fontColor;
        if (ColorUtility.TryParseHtmlString(data.fontStrokeColor, out strokeColor))
        {
            progText.effectColor = strokeColor;
            progText.effectStyle = UILabel.Effect.Outline8;
            progText.effectDistance = new Vector2(2f,2f);
        }
        progText.depth = depth + 2;

        progBar.value = data.progress/(float)data.targetCount;
        progText.text = $"{data.progress}/{data.targetCount}";
        Color bgColor, gaugeColor;
        if( ColorUtility.TryParseHtmlString(data.bgColor, out bgColor) )
            progBar.backgroundWidget.color = bgColor;
        if (ColorUtility.TryParseHtmlString(data.gaugeColor, out gaugeColor))
            progBar.foregroundWidget.color = gaugeColor;

        progBar.backgroundWidget.width = data.gaugeWidth;
        progBar.backgroundWidget.height = data.gaugeHeight;
        progBar.backgroundWidget.depth = depth;

        progBar.foregroundWidget.width = data.gaugeWidth;
        progBar.foregroundWidget.height = data.gaugeHeight;
        progBar.foregroundWidget.depth = depth + 1;

        bool completed = data.targetCount <= data.progress;

        var compTex = newObj.transform.Find("CompleteTexture")?.GetComponent<UITexture>();
        if (compTex != null && completed && data.texture != null)
        {
            compTex.gameObject.SetActive(true);
            compTex.mainTexture = data.texture;
            compTex.MakePixelPerfect();
            compTex.depth = depth + 4;
        }

        objectList.Add(new ListObject() { type = ListObjType.QUESTGAUGE, gameObject = newObj });
    }

    void OnClick_Link()
    {
        if (this.bCanTouch == false)
            return;
        bCanTouch = false;

        LinkData linkData = UIPokoButton._current.appendedObject as LinkData;
        if (linkData != null)
        {
            switch( linkData.linkType )
            {
                case LinkType.LT_PAGE:
                    {
                        StartCoroutine(OpenPage(linkData.linkName, this.currentPage.pageName));

                    }
                    break;

                case LinkType.LT_URL_LINK:
                    {
                        Application.OpenURL(linkData.linkName);
                        bCanTouch = true;
                    }
                    break;

                case LinkType.LT_COUPON_URL_LINK:
                    {
                        StartCoroutine(RequestCoupon());
                    }
                    break;
            }
        }
    }
    
    void OnBtnBack()
    {
        if (this.bCanTouch == false)
            return;
        bCanTouch = false;

        string pageName = this.pageStack.Pop();

        StartCoroutine(OpenPage(pageName));
    }

    IEnumerator RequestCoupon()
    {
        Debug.Log("RequestCoupon");

        bool retArrived = false;
        ServerAPI.GetCoupon((Protocol.GetCouponResp resp) =>
       {
           retArrived = true;
           if (resp.IsSuccess)
           {
               if (resp.coupon == null)
               {
                   UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                   popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_16"), false);
               }
               else
               {
                   Application.OpenURL(resp.coupon.coupon);
               }
           }
       });

        while (!retArrived)
        {
            yield return new WaitForSeconds(0.1f);

        }

        if( UIPokoButton._current )
        {
            UIPokoButton._current.gameObject.SetActive(false);
        }
        

        //yield return new WaitForSeconds(5f);
        bCanTouch = true;



        yield break;
    }

    void SetIndexPage()
    {
        btnBack.gameObject.SetActive(false);
    }

    void SetContextPage()
    {
        btnBack.gameObject.SetActive(true);
    }

    void SetPageMode(int mode)
    {
        if( mode == 0 )
        {
            mainSprite.spriteName = "popup_back_06";
            mainSprite.width = 755;
            mainSprite.height = 1016;
            mainSprite.transform.localPosition = new Vector3(0, 0, 0);
            
            contentsBasis.GetComponent<UISprite>().enabled = false;
            contentsBasis.transform.localPosition = new Vector3(0, 0, 0);

            btnClose.transform.localPosition = new Vector3(330f, 460f, 0f);
            btnBack.transform.localPosition = new Vector3(0, -450f, 0f);

            this.scrollPanel.SetRect(0, 0, 700, 980);
            this.scrollPanel.gameObject.transform.localPosition = new Vector3(-5f, 0, 0);
            //scrollBarObj.SetActive(false);
            scrollBarObj.transform.localPosition = new Vector3(355f, -26f, 0f);
            if( scrollBar.backgroundWidget != null )
            {
                scrollBar.backgroundWidget.height = 900;
            }
            if( scrollBar.foregroundWidget != null)
            {
                scrollBar.foregroundWidget.height = 900;
                var sbarPos = new Vector3();
                sbarPos = scrollBar.foregroundWidget.gameObject.transform.localPosition;
                sbarPos.y = scrollBar.foregroundWidget.height * 0.5f;
                scrollBar.foregroundWidget.gameObject.transform.localPosition = sbarPos;
            }
            
            
            title.gameObject.SetActive(false);
        }
        //else
        //{
        //    mainSprite.spriteName = "popup_bg_01";
        //    mainSprite.width = 790;
        //    mainSprite.height = 1200;
        //    mainSprite.transform.position = new Vector3(0, -40, 0);
        //    contentsBasis.GetComponent<UISprite>().enabled = true;
        //    contentsBasis.transform.position = new Vector3(0, -40, 0);
        //    this.scrollPanel.SetRect(0, 0, 704, 1026);
        //    scrollBarObj.SetActive(true);

        //}

    }

    //public static Texture2D TextureFromWWW(WWW www)
    //{
    //    if (www.text.Length == 0)
    //        return null;

    //    Texture2D tmp2dTex = new Texture2D(www.texture.width, www.texture.height, TextureFormat.ARGB32, false);
    //    www.LoadImageIntoTexture(tmp2dTex);
    //    return tmp2dTex;
    //}
}
