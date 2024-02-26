using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#region TextureData
[System.Serializable]
public enum TextureLoadState
{
    Loading,
    LoadCompleted,
}

[System.Serializable]
public class TextureData
{
    public TextureLoadState loadState = TextureLoadState.Loading;
    public int boardIndex = 0;
}

[System.Serializable]
public class TextureKeyAndData
{
    public int key = 0;
    public TextureData textData = new TextureData();
}
#endregion

#region TextureBoardData
[System.Serializable]
public class BoardData
{
    public int boardIndex = 0;
    public bool bEmpty = true;
}
#endregion

[System.Serializable]
public class TextureHashData
{
    public int atlasIndex = 0;
    public string hash = "";
}

[System.Serializable]
public class BoardAndTextureData
{
    public int atlasCount = 0;
    public List<TextureKeyAndData> listTextureDatas = new List<TextureKeyAndData>();
    public List<BoardData> listBoardDatas = new List<BoardData>();
    public List<TextureHashData> listHashDatas = new List<TextureHashData>();
}

[System.Serializable]
public class CategoryAndTexutreData
{
    public int category = 0;
    public BoardAndTextureData textureData = new BoardAndTextureData();
}

public enum TextureCategory_T
{
    None = 0,
    Material,
}

public class TextureJsonData
{ 
    public List<CategoryAndTexutreData> listCategoryAndTextureData = new List<CategoryAndTexutreData>();
  
    public TextureJsonData(List<CategoryAndTexutreData> datas)
    {
        this.listCategoryAndTextureData = datas;
    }
}

public class ManagerCdnTextureAtlas : MonoBehaviour
{
    public static string filekey = "myHa766b14dLs3w6UZvaZHTG4VAn115G";
    public static ManagerCdnTextureAtlas _instance = null;

    //생성된 아틀라스 저장할 경로.
    static public string atlasPath = "/ImageCache/";
    static public string atlasPathAdd = "/ImageCache/";

    //각 이미지 카테고리 별, 텍스쳐 정보와 보드 정보를 저장할 딕셔너리.
    private Dictionary<int, BoardAndTextureData> dicBoardAndTextureData = new Dictionary<int, BoardAndTextureData>();
    //카테고리 별 이미지를 저장할 딕셔너리.
    private Dictionary<int, List<Texture2D>> dicTextureAtlas = new Dictionary<int, List<Texture2D>>();

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private IEnumerator Start()
    {
        //처음 시작할 때, 로컬에 있는 아틀라스 파일, 텍스쳐정보 읽어오기.
        //후작업 : 파일이 하나라도 지워진 경우, 데이터 초기화시키기(빈 텍스쳐 출력되는 거 막기위해).
        atlasPath = Application.persistentDataPath + atlasPathAdd;

        //텍스쳐 정보, 보드정보 읽어오기.
        yield return StartCoroutine(CoLoadTextureAndBoardDataFromJson());

        int matFileCount = 0;
        //위에서 가져온 정보를 바탕으로 텍스쳐 아틀라스 생성.
        if (dicBoardAndTextureData.ContainsKey((int)TextureCategory_T.Material))
        {
            matFileCount = dicBoardAndTextureData[(int)TextureCategory_T.Material].atlasCount;
        }
        StartCoroutine(CoLoadTextureAtlas(TextureCategory_T.Material, matFileCount));
    }

    private IEnumerator CoLoadTextureAndBoardDataFromJson()
    {
        string filePath = Global.FileUri + ManagerCdnTextureAtlas.atlasPath + "TextureData.json";
        using(UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();

            if (www.IsError() || www.downloadHandler == null)
            {
                Debug.LogWarning("LoadFromFile : " + filePath + www.error);
            }
            else
            {
                string text = StageHelper.Decrypt256(www.downloadHandler.text, "iGki2W12fM93h8UA");
                TextureJsonData stringData = JsonUtility.FromJson<TextureJsonData>(text);
                for (int i = 0; i < stringData.listCategoryAndTextureData.Count; i++)
                {
                    CategoryAndTexutreData data = stringData.listCategoryAndTextureData[i];
                    dicBoardAndTextureData.Add(data.category, data.textureData);
                }
            }
        }
    }

    private IEnumerator CoLoadTextureAtlas(TextureCategory_T category, int fileCount)
    {
        List<Texture2D> listTexture = new List<Texture2D>();
        bool bLoadComplete = true;

        //현재 텍스쳐 파일 수만큼 받아오기.
        for(int i =0; i<fileCount;i++)
        {
            string fileName = System.Enum.GetName(typeof(TextureCategory_T), category);
            fileName = fileName + "_" + i.ToString() + ".png";
            string filePath = Global.FileUri + ManagerCdnTextureAtlas.atlasPath + fileName;
            using( UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath) )
            {
                yield return www.SendWebRequest();
                if (www.IsError() || www.downloadHandler == null)
                {
                    Debug.LogWarning("LoadFromFile : " + filePath + www.error);
                }
                else
                {
                    //해시검사.
                    string path = ManagerCdnTextureAtlas.atlasPath + fileName;
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
                    string hash = checkMD5(fileInfo);
                    var t = www.downloadHandler as DownloadHandlerTexture;

                    //현재 데이터에 지금 받아오는 텍스쳐 파일 정보가 저장되어있다면 해시검사.
                    //해시가 다른 파일이 하나라도 있으면 모든 데이터 초기화시켜줌.
                    if (!dicBoardAndTextureData.ContainsKey((int)category) ||
                        dicBoardAndTextureData[(int)category].listHashDatas[i].hash != hash)
                    {
                        bLoadComplete = false;
                        break;
                    }

                    Texture2D texture = t.texture;
                    listTexture.Add(texture);
                }
            }
        }
        if (fileCount != listTexture.Count)
            bLoadComplete = false;

        //데이터 로드 모두 완료된 후, 데이터 저장 / 데이터 로드 실패하면 데이터 초기화 시켜줌.
        if (bLoadComplete == true)
        {
            //저장된 텍스쳐 리스트를 카테고리 별로 딕셔너리에 저장.
            int key = (int)category;
            if (dicTextureAtlas.ContainsKey(key))
                dicTextureAtlas.Remove(key);
            dicTextureAtlas.Add(key, listTexture);

            //현재 카테고리의 텍스쳐 수 저장.
            if (dicBoardAndTextureData.ContainsKey(key))
            {
                dicBoardAndTextureData[key].atlasCount = listTexture.Count;
            }
        }
        else
        {
            dicTextureAtlas.Clear();
            dicBoardAndTextureData.Clear();
        }
    }

    #region 외부에서 참조.
    public bool CheckTextureData(int category, int texIdx)
    {   //현재 이 텍스쳐 데이터가 저장되어있는지 검사하는 함수.
        if (dicBoardAndTextureData.ContainsKey(category))
        {
            BoardAndTextureData textureData = new BoardAndTextureData();
            if (dicBoardAndTextureData.TryGetValue(category, out textureData))
            {
                int index = textureData.listTextureDatas.FindIndex(x => x.key == texIdx);
                if (index > -1 && textureData.listTextureDatas[index].textData.loadState == TextureLoadState.LoadCompleted)
                    return true;
            }
        }
        return false;
    }

    public void InitTextureDataLoadState(int category, int texIdx)
    {
        if (dicBoardAndTextureData.ContainsKey(category) == false)
            return;
   
        BoardAndTextureData textureData = new BoardAndTextureData();
        if (dicBoardAndTextureData.TryGetValue(category, out textureData))
        {
            int index = textureData.listTextureDatas.FindIndex(x => x.key == texIdx);
            //현재 텍스쳐 정보가 있는 경우에 로드 상태만 변경시켜줌.
            if (index > -1)
            {
                textureData.listTextureDatas[index].textData.loadState = TextureLoadState.Loading;
            }
        }
    }

    public TextureKeyAndData GetTextureKeyAndData(int category, int texIdx)
    {
        BoardAndTextureData textureData = new BoardAndTextureData();
        TextureKeyAndData data = new TextureKeyAndData();
        if (dicBoardAndTextureData.TryGetValue(category, out textureData))
        {
            data = textureData.listTextureDatas.Find(x => x.key == texIdx);
        }
        return data;
    }

    public Texture2D GetTexture(int category, TextureKeyAndData data, int texWidth, int texHeight, int atlasWidth, int atlasHeight)
    {
        if (dicBoardAndTextureData.ContainsKey(category) == false)
            return null;

        int atlasIndex = GetAtlasIndex(texWidth, texHeight, atlasWidth, atlasHeight, data.textData.boardIndex);
        List<Texture2D> listTexture = new List<Texture2D>();
        dicTextureAtlas.TryGetValue(category, out listTexture);
        return listTexture[atlasIndex];
    }

    public Rect GetUVRect(TextureKeyAndData data, int texWidth, int texHeight, int atlasWidth, int atlasHeight)
    {
        Rect uvRect = new Rect(0, 0, 0, 0);

        //현재 보드인덱스를 가지고 있는 아틀라스 인덱스를 가져옴.
        int atlasIndex = GetAtlasIndex(texWidth, texHeight, atlasWidth, atlasHeight, data.textData.boardIndex);
        int currentBoardIdx = data.textData.boardIndex;
        //현재 보드에서의 인덱스 얼마인지 구함.
        int cellCount = GetCellCount(texWidth, texHeight, atlasWidth, atlasHeight);
        if (currentBoardIdx >= cellCount)
        {
            currentBoardIdx -= (cellCount * atlasIndex);
        }

        int wCount = atlasWidth / texWidth;
        int yCount = atlasHeight / texHeight;

        int attachX = currentBoardIdx;
        int attachY = 0;
        if (currentBoardIdx >= wCount)
        {
            attachX = currentBoardIdx % wCount;
            attachY = currentBoardIdx / yCount;
        }
        attachX *= texWidth;
        attachY *= texHeight;

        uvRect = new Rect(
         (attachX / (float)atlasWidth),
         (attachY / (float)atlasHeight),
         (texWidth / (float)atlasWidth),
         (texHeight / (float)atlasHeight)
        );

        return uvRect;
    }

    public void MakeAtlasAndTextureData(TextureCategory_T category, int texIdx, Texture2D tex, int texW, int texH, int atW, int atH)
    {   //가지고 온 텍스쳐를 아틀라스에 합치고, 해당 텍스쳐의 정보를 넣는 함수.

        ////텍스쳐 세부 설정들 해줌.
        //RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        //Graphics.Blit(tex, tmp);
        //RenderTexture previous = RenderTexture.active;
        //RenderTexture.active = tmp;
        //Texture2D myTexture2D = new Texture2D(tex.width, tex.height);
        //myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        //myTexture2D.Apply();
        //RenderTexture.active = previous;
        //RenderTexture.ReleaseTemporary(tmp);

        //텍스쳐 보드 인덱스 가져와서 텍스쳐 합쳐주는 작업.
        int boardIndex = GetBoardIndex((int)category, texIdx, texW, texH, atW, atH);
        CombineTexture(category, texIdx, tex, texW, texH, boardIndex);
    }
    #endregion

    #region hash 검사 코드
    private string checkMD5(System.IO.FileInfo fileInfo)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = fileInfo.OpenRead())
            //using (var stream = System.IO.File.OpenRead(fileInfo.ToString()))
            {
                return ByteArrayToHexString(md5.ComputeHash(stream));
            }
        }
    }

    private string ByteArrayToHexString(byte[] bytes)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; ++i)
            builder.Append(bytes[i].ToString("X2"));

        return builder.ToString();
    }
    #endregion

    #region 텍스쳐의 보드 인덱스 가져오는 코드.
    private int GetBoardIndex(int category, int texIdx, int texWidth, int texHeight, int atW, int atH)
    {   //현재 텍스쳐의 보드 인덱스를 가져오는 함수.
        if(dicBoardAndTextureData.ContainsKey(category))
        {
            BoardAndTextureData textureData = new BoardAndTextureData();
            if (dicBoardAndTextureData.TryGetValue(category, out textureData))
            {
                int index = textureData.listTextureDatas.FindIndex(x => x.key == texIdx);
                //현재 텍스쳐 정보가 있는 경우에만 보드 인덱스 반환.
                if (index > -1)
                {
                    return textureData.listTextureDatas[index].textData.boardIndex;
                }
            }
        }
        //보드 인덱스를 가져오지 못했다면, 보드데이터에서 비어있는 보드 인덱스 들고와서 반환.
        return GetEmptyBoardIndex(category, texWidth, texHeight, atW, atH);
    }

    private int GetEmptyBoardIndex(int category, int texWidth, int texHeight, int atW, int atH)
    {   //현재 카테고리 보드 데이터에서 비어있는 데이터를 가져옴.

        List<Texture2D> listTexture = new List<Texture2D>();

        if (dicBoardAndTextureData.ContainsKey(category))
        {
            BoardAndTextureData textureData = new BoardAndTextureData();
            if (dicBoardAndTextureData.TryGetValue(category, out textureData))
            {
                //보드 데이터를 검사해 비어있는 보드 데이터가 있는 지 검사.
                for (int i = 0; i < textureData.listBoardDatas.Count; i++)
                {
                    if (textureData.listBoardDatas[i].bEmpty == true)
                    {
                        textureData.listBoardDatas[i].bEmpty = false;
                        return textureData.listBoardDatas[i].boardIndex;
                    }
                }
                //빈 보드가 없다면 새로운 보드데이터 생성 & 텍스쳐 생성.
                if (dicTextureAtlas.ContainsKey(category))
                {
                    dicTextureAtlas.TryGetValue(category, out listTexture);
                }
                listTexture.Add(MakeEmptyTexture(atW, atH));
                //현재 카테고리의 텍스쳐 수 저장.
                dicBoardAndTextureData[(int)category].atlasCount = listTexture.Count;

                //새로운 텍스쳐 아틀라스 딕셔너리에 추가.
                if (dicTextureAtlas.ContainsKey(category))
                    dicTextureAtlas.Remove(category);
                dicTextureAtlas.Add(category, listTexture);

                int preBoardIndex = dicBoardAndTextureData[category].listBoardDatas.Count;
                MakeTextureBoardData(true, category, texWidth, texHeight, atW, atH);
                dicBoardAndTextureData[category].listBoardDatas[preBoardIndex].bEmpty = false;
                //새로운 보드 만들기 전까지의 카운트 반환.
                return preBoardIndex;
            }
        }
        //현재 카테고리에 보드 데이터 없다면 보드데이터 새로 생성 & 빈 텍스쳐 아틀라스 생성.
        MakeTextureBoardData(false, category, texWidth, texHeight, atW, atH);
        dicBoardAndTextureData[category].listBoardDatas[0].bEmpty = false;

        listTexture.Add(MakeEmptyTexture(atW, atH));
        dicBoardAndTextureData[(int)category].atlasCount = 1;

        //새로운 텍스쳐 아틀라스 딕셔너리에 추가.
        if (dicTextureAtlas.ContainsKey(category))
            dicTextureAtlas.Remove(category);
        dicTextureAtlas.Add(category, listTexture);
        return 0;
    }

    private void MakeTextureBoardData(bool bDictionary, int category, int textureWidth, int textureHeight, int atW, int atH)
    {   //보다 데이터 생성해주는 함수.
        //현재 보드에 들어갈 수 있는 최대 텍스쳐 갯수.
        int cellCount = GetCellCount(textureWidth, textureHeight, atW, atH);

        //현재 텍스쳐 정보가 든 딕셔너리 존재하냐에 따라 설정.
        if (bDictionary == true)
        {
            int preBoardIndex = dicBoardAndTextureData[category].listBoardDatas.Count;
            for (int i = 0; i < cellCount; i++)
            {
                BoardData board = new BoardData();
                board.boardIndex = i + preBoardIndex;;
                dicBoardAndTextureData[category].listBoardDatas.Add(board);
            }
        }
        else
        {
            BoardAndTextureData textureData = new BoardAndTextureData();
            for (int i = 0; i < cellCount; i++)
            {
                BoardData board = new BoardData();
                board.boardIndex = i;
                textureData.listBoardDatas.Add(board);
            }
            dicBoardAndTextureData.Add(category, textureData);
        }
    }
    #endregion

    private Texture2D MakeEmptyTexture(int atlasWidth, int atlasHeight)
    {   //빈 아틀라스 텍스쳐 생성.
        Texture2D atlasTexture = new Texture2D(atlasWidth, atlasHeight);
        Color fillColor = new Color(1f, 1f, 1f, 0.0f);
        var fillColorArray = atlasTexture.GetPixels();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }
        atlasTexture.SetPixels(fillColorArray);
        atlasTexture.Apply();
        return atlasTexture;
    }

    private int GetCellCount(int texW, int texY, int atW, int atH)
    {   //한 아틀라스안에 들어갈 수 있는 텍스쳐 갯수 반환하는 함수.
        int cellCount = 0;
        int listCount_X = atW / texW;
        int listCount_Y = atH / texY;
        cellCount = listCount_X * listCount_Y;
        return cellCount;
    }
    
    private void CombineTexture(TextureCategory_T category, int texIdx, Texture2D tex, int texW, int texH, int boardIdx)
    {   //텍스쳐 기존 아틀라스에 합쳐주는 함수.

        List<Texture2D> listAtlasTexture = null;
        dicTextureAtlas.TryGetValue((int)category, out listAtlasTexture);
        
        //현재 보드인덱스를 가지고 있는 아틀라스 인덱스를 가져옴.
        int atlasIndex = GetAtlasIndex(texW, texH, listAtlasTexture[0].width,listAtlasTexture[0].height, boardIdx);

        Texture2D atlasTexture = listAtlasTexture[atlasIndex];
        int wCount = atlasTexture.width / texW;
        int yCount = atlasTexture.height / texH;

        //현재 보드에서의 인덱스 얼마인지 구함.
        int cellCount = GetCellCount(texW, texH, atlasTexture.width, atlasTexture.height);
        int currentBoardIdx = boardIdx;
        if (currentBoardIdx >= cellCount)
        {
            currentBoardIdx -= (cellCount * atlasIndex);
        }

        int attachX = currentBoardIdx;
        int attachY = 0;
        if (currentBoardIdx >= wCount)
        {
            attachX = currentBoardIdx % wCount;
            attachY = currentBoardIdx / yCount;
        }
        attachX *= texW;
        attachY *= texH;

        for (int x = 0; x < texW; x++)
        {
            for (int y = 0; y < texH; y++)
            {
                var baseX = 0;
                var baseY = 0;
                baseX = attachX + x;
                baseY = attachY + y;
                
                Color texColor = tex.GetPixel(x, y);

                atlasTexture.SetPixel(baseX, baseY, texColor);
            }
        }
        atlasTexture.Apply();

        //텍스쳐 데이터 상태를 LoadCompleted 로 교체.
        AddTextureData((int)category, texIdx, boardIdx);

        if (!System.IO.Directory.Exists(atlasPath))
            System.IO.Directory.CreateDirectory(atlasPath);

        //이미지 저장.
        string fileName = System.Enum.GetName(typeof(TextureCategory_T), category);
        fileName = fileName + "_" + atlasIndex.ToString() + ".png";
        string path = atlasPath + fileName;
        SaveTextureAsPNG(atlasTexture, path);

        //저장한 이미지 해시 들고와서 딕셔너리에 추가.
        System.IO.FileInfo atlasFileInfo = new System.IO.FileInfo(path);
        TextureHashData hashData = new TextureHashData();
        hashData.atlasIndex = atlasIndex;
        hashData.hash = checkMD5(atlasFileInfo);

        //현재 아틀라스의 해시값을 가지고 있는지 검사.
        int index = dicBoardAndTextureData[(int)category].listHashDatas.FindIndex(x => x.atlasIndex == atlasIndex);
        if (index > -1)
        {
            dicBoardAndTextureData[(int)category].listHashDatas[index].hash = hashData.hash;
        }
        else
        {
            dicBoardAndTextureData[(int)category].listHashDatas.Add(hashData);
        }

        //데이터 json 파일로 저장.
        string jsonPath = atlasPath + "TextureData.json";
        SaveMaterialTextureDataToJson(dicBoardAndTextureData, jsonPath);
    }

    private int GetAtlasIndex(int textureWidth, int textureHeight, int atW, int atH, int boardIdx)
    {  //한 아틀라스 당 들어있는 보드 수 구함.

        int cellCount = GetCellCount(textureWidth, textureHeight, atW, atH);
        //현재 이미지가 들어갈 아틀라스 인덱스를 구함.
        int atlasIndex = 0;
        if (boardIdx >= cellCount)
            atlasIndex = (int)boardIdx / cellCount;
        return atlasIndex;
    }

    private void AddTextureData(int category, int texIdx, int boardIdx)
    {   //텍스쳐 데이터 갱신/추가 해주는 함수.
        if (dicBoardAndTextureData.ContainsKey(category))
        {
            BoardAndTextureData textureData = new BoardAndTextureData();
            if (dicBoardAndTextureData.TryGetValue(category, out textureData))
            {
                int index = textureData.listTextureDatas.FindIndex(x => x.key == texIdx);
                //현재 텍스쳐 정보가 있는 경우에는 보드 인덱스와 로드 상태만 변경시켜줌.
                if (index > -1)
                {   
                    textureData.listTextureDatas[index].textData.boardIndex = boardIdx;
                    textureData.listTextureDatas[index].textData.loadState = TextureLoadState.LoadCompleted;
                }
                else
                {
                    TextureKeyAndData data = new TextureKeyAndData();
                    data.key = texIdx;
                    data.textData.boardIndex = boardIdx;
                    data.textData.loadState = TextureLoadState.LoadCompleted;
                    textureData.listTextureDatas.Add(data);
                }
            }
        }
    }

    private void SaveTextureAsPNG(Texture2D texture, string path)
    {   
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, _bytes);
    }

    private void SaveMaterialTextureDataToJson(Dictionary<int, BoardAndTextureData> dicTextureData, string _fullPath)
    {
        List<CategoryAndTexutreData> listDatas = new List<CategoryAndTexutreData>();
        var enumerator = dicTextureData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            CategoryAndTexutreData data = new CategoryAndTexutreData();
            data.category = enumerator.Current.Key;
            data.textureData = enumerator.Current.Value;
            listDatas.Add(data);
        }
        TextureJsonData saveData = new TextureJsonData(listDatas);
        string jsonStr = JsonUtility.ToJson(saveData);
        jsonStr = StageHelper.AESEncrypt256(jsonStr, "iGki2W12fM93h8UA");

        System.IO.FileStream fs = new System.IO.FileStream(_fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        System.IO.StreamWriter outStream = new System.IO.StreamWriter(fs, System.Text.Encoding.ASCII);
        outStream.Write(jsonStr);
        outStream.Flush();
        outStream.Close();
        fs.Close();
    }
}
