using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Serialization;

public class GimmickTutorial_Contents
{
    public string spriteName;
    public string text;

    public GimmickTutorial_Contents(string _name, string _text)
    {
        spriteName = _name;
        text = _text;
    }
}

//기믹 튜토리얼 정보

public class GimmickTutorial_Type
{
    public BlockType blockType;
    public BoardDecoType decoType;

    public GimmickTutorial_Type()
    {
        blockType = BlockType.NONE;
        decoType = BoardDecoType.NONE;
    }
    public GimmickTutorial_Type(BlockType _blockType)
    {
        blockType = _blockType;
        decoType = BoardDecoType.NONE;
    }
    public GimmickTutorial_Type(BoardDecoType _decoType)
    {
        blockType = BlockType.NONE;
        decoType = _decoType;
    }
}

public class GimmickTutorial_Data
{
    public GimmickTutorial_Type gimmickType;
    public string gimmickName;
    public int contentsCount;
    public List<GimmickTutorial_Contents> contentsList = new List<GimmickTutorial_Contents>();

    public GimmickTutorial_Data()
    {
        gimmickType = new GimmickTutorial_Type();
        gimmickName = "";
        contentsCount = 0;
        contentsList = new List<GimmickTutorial_Contents>();
    }

    public GimmickTutorial_Data(GimmickTutorial_Type _gimmickType, string _name, int _count, List<GimmickTutorial_Contents> _contentsList)
    {
        gimmickType = _gimmickType;
        gimmickName = _name;
        contentsCount = _count;
        contentsList = _contentsList;
    }

    public GimmickTutorial_Data(GimmickTutorial_Data data)
    {
        gimmickType = data.gimmickType;
        gimmickName = data.gimmickName;
        contentsCount = data.contentsCount;
        contentsList.CopyAllTo(data.contentsList);
    }
}

[System.Serializable]
public class TutorialData_JsonData
{
    public string key = "";
    public int count = 0;
    public int priority = 0;
}

[System.Serializable]
public class TutorialList_JsonData
{
    public List<TutorialData_JsonData> TutorialDataList = new List<TutorialData_JsonData>();
}

public class ManagerGimmickTutorial : MonoBehaviour
{
    public static ManagerGimmickTutorial instance = null;

    public const string TUTORIALDATA_FILEPATH = "GimmickTutorial/tutorialData.json"; //다국어 X
    public const string TUTORIALTEXT_FILEPATH = "GimmickTutorial/tutorialText.json"; //다국어 처리 필요함
    public const string TUTORIALTEXT_NAME_FORMAT = "t_{0}_Name";
    public const string TUTORIALTEXT_KEY_FORMAT = "t_{0}_{1}";

    public List<GimmickTutorial_Data> gimmickTutorialList = new List<GimmickTutorial_Data>();

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }
    #region 초기화 관련

    public IEnumerator CoInitTutorialData()
    {
        //튜토리얼 데이터 CDN에서 로드
        yield return CoLoadTutorialDataFromJSON();

        //로드한 데이터의 여부에 따라 팁 버튼 활성/비활성
        GameUIManager.instance.ShowTipButton(IsExistGimmickTutorialData() == true);
    }

    public IEnumerator CoLoadTutorialDataFromJSON()
    {
        gimmickTutorialList.Clear();

        //파일 로딩
        TutorialList_JsonData dataList = new TutorialList_JsonData();
        yield return CoLoadDataList(TUTORIALDATA_FILEPATH, (localText) =>
        {
            dataList = JsonUtility.FromJson<TutorialList_JsonData>(localText);
        });
        Dictionary<string, string> textList = new Dictionary<string, string>();
        yield return CoLoadDataList(LanguageUtility.FileNameConversion(TUTORIALTEXT_FILEPATH), (localText) =>
        {
            StringHelper.LoadStringFromJson(localText, ref textList);
        });

        //읽어온 내용으로 데이터 세팅
        Dictionary<GimmickTutorial_Type, int> tutorialPriorityDic = new Dictionary<GimmickTutorial_Type, int>();
        List<GimmickTutorial_Type> tutorialTypeList = GetTutorialListByUseGimmickData();

        foreach (var temp in dataList.TutorialDataList)
        {
            GimmickTutorial_Data tempItem = LoadGimmickTutorialData(temp.key, temp.count, textList);
            if (tempItem != null)
            {
                GimmickTutorial_Type tutoType = ConvertTutorialTypeByString(temp.key);
                if (tutorialTypeList.Find(x => (x.blockType != BlockType.NONE ? x.blockType == tutoType.blockType : x.decoType == tutoType.decoType)) != null)
                {
                    if (tutorialPriorityDic.ContainsKey(tutoType) == false)
                        tutorialPriorityDic.Add(tutoType, temp.priority);

                    tempItem.gimmickType = tutoType;
                    gimmickTutorialList.Add(tempItem);
                }
            }
        }

        //우선순위로 정렬
        Sorting(ref gimmickTutorialList, tutorialPriorityDic);
    }
    public IEnumerator CoLoadDataList(string fileName, System.Action<string> complete)
    {
        string filePath = Global._cdnAddress + fileName;
        using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                string text = www.downloadHandler.text;
                complete?.Invoke(text);
            }
        }
    }

    List<GimmickTutorial_Type> GetTutorialListByUseGimmickData()
    {
        //현재 맵에서 사용되는 기믹 정보 가져와서 튜토리얼 리스트 생성
        UseGimmickData gimmickData = ManagerBlock.instance.GetUseGimmickData();
        List<GimmickTutorial_Type> list = new List<GimmickTutorial_Type>();

        //블럭 확인
        for (int i = 0; i < gimmickData.listUseBlockType.Count; i++)
        {
            GimmickTutorial_Type temp = new GimmickTutorial_Type((BlockType)gimmickData.listUseBlockType[i]);
            if (temp != null)
                list.Add(temp);
        }

        //데코 확인
        for (int i = 0; i < gimmickData.listUseDecoType.Count; i++)
        {
            GimmickTutorial_Type temp = new GimmickTutorial_Type((BoardDecoType)gimmickData.listUseDecoType[i]);
            if (temp != null)
                list.Add(temp);
        }

        //탑 데코 확인
        for (int i = 0; i < gimmickData.listUseTopDecoType.Count; i++)
        {
            GimmickTutorial_Type temp = new GimmickTutorial_Type((BoardDecoType)gimmickData.listUseTopDecoType[i]);
            if (temp != null)
                list.Add(temp);
        }

        ExceptGimmickTutorial(ref list);

        return list;
    }

    //기믹 튜토리얼 예외처리(ex. 흙보석)
    private void ExceptGimmickTutorial(ref List<GimmickTutorial_Type> list)
    {
        //이벤트 블럭 예외처리
        if (list.Find(x => (x.blockType == BlockType.BLOCK_EVENT)) != null
            && list.Find(x => (x.blockType == BlockType.PLANT)) != null)
        {
            //맨 이벤트 블록만 있는 경우, 식물 튜토리얼 제거
            bool showPlantTutorial = false;
            foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
            {
                if (blockInfo.type == (int)BlockType.BLOCK_EVENT
                    && blockInfo.count > 0 )
                {
                    showPlantTutorial = true;
                    break;
                }

                if(blockInfo.type == (int)BlockType.PLANT
                    || blockInfo.type == (int)BlockType.PLANT_APPLE
                    || blockInfo.type == (int)BlockType.PLANT_ICE_APPLE
                    || blockInfo.type == (int)BlockType.PLANT_KEY)
                {
                    showPlantTutorial = true;
                    break;
                }
            }

            if (showPlantTutorial == false)
            {
                int removeIndex = list.FindIndex(x => (x.blockType == BlockType.PLANT));
                list.RemoveAt(removeIndex);
            }
        }

        //흙이벤트블럭 예외처리
        if (list.Find(x => (x.blockType == BlockType.BLOCK_EVENT_GROUND)) != null
            && list.Find(x => (x.blockType == BlockType.GROUND)) == null)
        {
            //흙 기믹 튜토리얼 없으면 추가
            GimmickTutorial_Type temp = new GimmickTutorial_Type(BlockType.GROUND);
            if (temp != null)
                list.Add(temp);
        }

        //흙보석 예외처리
        if (list.Find(x => (x.blockType == BlockType.GROUND_JEWEL)) != null)
        {
            //흙 기믹튜토리얼 출력 판단
            if (list.Find(x => (x.blockType == BlockType.GROUND)) != null)
            {
                bool showGroundTutorial = false;

                //맵에 설치된 경우
                foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
                {
                    if (blockInfo.type == (int)BlockType.GROUND
                        && blockInfo.subType == (int)GROUND_TYPE.NORMAL)
                    {
                        showGroundTutorial = true;
                        break;
                    }

                    if (blockInfo.type == (int)BlockType.GROUND
                        && blockInfo.subType == (int)GROUND_TYPE.JEWEL
                        && blockInfo.count > 1)
                    {
                        showGroundTutorial = true;
                        break;
                    }
                }

                //출발 설정된 경우
                foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
                {
                    if (startInfo.probability == 0)
                        continue;

                    if (startInfo.type == (int)BlockType.GROUND)
                    {
                        showGroundTutorial = true;
                        break;
                    }
                }

                if (showGroundTutorial == false)
                {
                    int removeIndex = list.FindIndex(x => (x.blockType == BlockType.GROUND));
                    list.RemoveAt(removeIndex);
                }
            }
            else
            {
                //흙보석 출발설정 중 흙단계가 있는 경우 흙기믹 튜토리얼 출력
                foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
                {
                    if (startInfo.probability == 0)
                        continue;

                    if (startInfo.type == (int)BlockType.GROUND_JEWEL
                        && (startInfo.countProb[0] > 0
                        || startInfo.countProb[1] > 0))
                    {
                        GimmickTutorial_Type temp = new GimmickTutorial_Type(BlockType.GROUND);
                        if (temp != null)
                            list.Add(temp);

                        break;
                    }
                }
            }
        }
    }

    private GimmickTutorial_Data LoadGimmickTutorialData(string gimmickName, int count, Dictionary<string, string> fileText)
    {
        //기믹명
        string key_NameStr = string.Format(TUTORIALTEXT_NAME_FORMAT, gimmickName);
        if (fileText.ContainsKey(key_NameStr) == true)
        {
            GimmickTutorial_Data InitData = new GimmickTutorial_Data();
            InitData.gimmickName = fileText[key_NameStr];

            //튜토리얼 내용(설명)
            for (int i = 0; i < count; i++)
            {
                string key_TextStr = string.Format(TUTORIALTEXT_KEY_FORMAT, gimmickName, (i + 1).ToString());

                if (fileText.ContainsKey(key_TextStr) == true)
                    InitData.contentsList.Add(new GimmickTutorial_Contents(key_TextStr, fileText[key_TextStr]));
            }
            return InitData;
        }
        return null;
    }

    private GimmickTutorial_Type ConvertTutorialTypeByString(string _type)
    {
        //예외처리
        if (_type == "ICE")
        {
            //얼음 기믹은 BlockType, BoardDecoType 둘 다 존재할 수 있으므로, BoardDecoType으로 통일
            return new GimmickTutorial_Type(BoardDecoType.ICE);
        }

        GimmickTutorial_Type type = null;
        BlockType tempBlockType = BlockType.NONE;
        System.Enum.TryParse(_type, out tempBlockType);

        if (tempBlockType == BlockType.NONE)
        {
            BoardDecoType tempDecoType = BoardDecoType.NONE;

            System.Enum.TryParse(_type, out tempDecoType);

            if (tempDecoType != BoardDecoType.NONE)
                type = new GimmickTutorial_Type(tempDecoType);
        }
        else
            type = new GimmickTutorial_Type(tempBlockType);

        return type;
    }

    public void Sorting(ref List<GimmickTutorial_Data> listGimmickTutorialInfo, Dictionary<GimmickTutorial_Type, int> tutorialPriorityDic)
    {
        listGimmickTutorialInfo.Sort((a, b) =>
        {
            int returnValue = tutorialPriorityDic[a.gimmickType].CompareTo(tutorialPriorityDic[b.gimmickType]);

            if (returnValue == 0)
            {
                //우선순위가 같을 경우에 대한 처리//블럭타입, 데코타입 늦게 추가된 기믹 우선순위가 더 높음
                if (a.gimmickType.blockType != BlockType.NONE)
                {
                    returnValue = b.gimmickType.blockType.CompareTo(a.gimmickType.blockType);
                }
                else
                {
                    //블럭 관련 튜토리얼이 아닌 경우
                    if (a.gimmickType.decoType != BoardDecoType.NONE)
                    {
                        returnValue = b.gimmickType.decoType.CompareTo(a.gimmickType.decoType);
                    }
                }
            }

            return returnValue;
        });
    }
    #endregion

    public string GetGimmickNameByTutorialType(GimmickTutorial_Type tutoType)
    {
        string returnValue = "";

        if (tutoType.blockType == BlockType.NONE)
        {
            if (tutoType.decoType != BoardDecoType.NONE)
            {
                returnValue = System.Enum.GetName(typeof(BoardDecoType), tutoType.decoType);
            }
        }
        else
        {
            returnValue = System.Enum.GetName(typeof(BlockType), tutoType.blockType);
        }

        return returnValue;
    }

    /// <summary>
    /// 튜토리얼 데이터가 있는지 반환
    /// </summary>
    public bool IsExistGimmickTutorialData()
    {
        if (gimmickTutorialList == null || gimmickTutorialList.Count == 0)
            return false;
        else
            return true;
    }

}
