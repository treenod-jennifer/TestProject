using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorAreaAnimal_ViewCharacterData : EditorAreaAnimal_ViewBase
{
    [System.Serializable]
    public class Editor_AreaCharacterData
    {
        public bool isFoldData = false;
        public CharacterAndCostumeData charData = new CharacterAndCostumeData();
        public string roamingAreaIndex = "";
        public int openMission = 0;
    }

    [SerializeField]
    private List<Editor_AreaCharacterData> listData = new List<Editor_AreaCharacterData>();

    public override void InitView()
    {
        viewSize.x = 300;
        titleName = "캐릭터 데이터";
        dataBoxHeight_normal = 202;
        dataBoxHeight_fold = 40;
    }

    protected override void CalcurateHeight_View()
    {
        viewSize.y = (listData.Count == 0) ? titleHeight + 5 : titleHeight + 5;
        for (int i = 0; i < listData.Count; i++)
        {
            if (listData[i].isFoldData == false)
                viewSize.y += dataBoxHeight_normal;
            else
                viewSize.y += dataBoxHeight_fold;
        }
    }

    protected override void DrawDataView()
    {
        for (int i = 0; i < listData.Count; i++)
        {
            viewPos = Draw_CharacterData(i);
        }
    }

    private Vector2 Draw_CharacterData(int index)
    {
        Editor_AreaCharacterData data = listData[index];
        if (data == null)
            return viewPos;

        if (index % 2 == 1)
        {
            int dataBoxHeight = (data.isFoldData == true) ? dataBoxHeight_fold : dataBoxHeight_normal;
            GUI.Box(new Rect(viewPos.x, viewPos.y, viewSize.x, dataBoxHeight), "");
        }
        viewPos.y += 10;

        if (data.charData == null)
            data.charData = new CharacterAndCostumeData();

        #region 리스트 타이틀(접기/펼치기)
        GUIStyle style = new GUIStyle();
        if (GUI.Button(new Rect(viewPos.x, viewPos.y, viewSize.x - 30, 22), "", style))
        {
            data.isFoldData = !data.isFoldData;
        }
        string buttonText = string.Format("{0} 캐릭터 : {1}", (data.isFoldData == true ? "▶" : "▼"), data.charData.charType);
        EditorGUI.LabelField(new Rect(viewPos.x + 5, viewPos.y, viewSize.x, 22), buttonText);
        #endregion

        #region 삭제 버튼
        if (GUI.Button(new Rect(viewPos.x + viewSize.x - 27, viewPos.y, 22, 22), "X"))
        {
            listRemoveDataIndex.Add(index);
        }
        #endregion
        viewPos.y += 30f;

        //접힌 상태라면 그리는 처리 안해줌.
        if (data.isFoldData == true)
            return viewPos;

        #region 데이터 입력란
        //캐릭터 타입 입력란
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y, 60, 22), "캐릭터");
        System.Enum enumCharType = EditorGUI.EnumPopup(new Rect(viewPos.x + 50, viewPos.y, 100, 25), data.charData.charType);
        data.charData.charType = (TypeCharacterType)System.Convert.ChangeType(enumCharType, data.charData.charType.GetType());
        viewPos.y += 27f;

        //코스튬 인덱스 입력란
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y, 60, 22), "코스튬");
        data.charData.costumeIdx = EditorGUI.IntField(new Rect(viewPos.x + 50, viewPos.y, 50, 22), "", data.charData.costumeIdx);
        viewPos.y += 27f;

        //오픈 미션 데이터
        GUIStyle infoStyle = new GUIStyle();
        style.wordWrap = true;
        string infoText_mission = "<color=grey>[해당 번호의 미션이 완료되어야 캐릭터가 등장합니다.]</color>";
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y + 3, 200, 22), infoText_mission, infoStyle);

        viewPos.y += 23f;
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y + 3, 80, 22), "오픈 미션");
        data.openMission = EditorGUI.IntField(new Rect(viewPos.x + 80, viewPos.y, 50, 22), data.openMission);
        viewPos.y += 27f;

        //로밍 에리어 인덱스 데이터
        string infoText_roaming = "<color=grey>[사용할 데이터의 인덱스를 입력(ex. 0,2,3)]</color>";
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y + 3, 200, 22), infoText_roaming, infoStyle);
        viewPos.y += 23f;

        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y + 3, 120, 22), "사용할 로밍 에리어");
        data.roamingAreaIndex = EditorGUI.TextField(new Rect(viewPos.x + 120, viewPos.y, 100, 22), data.roamingAreaIndex);
        #endregion

        viewPos.y += 35f;
        return viewPos;
    }

    protected override void RemoveData()
    {
        for (int i = (listData.Count - 1); i >= 0; i--)
        {
            if (listRemoveDataIndex.FindIndex(x => x == i) != -1)
            {
                listData.Remove(listData[i]);
            }
        }
    }

    protected override void AddData()
    {
        Editor_AreaCharacterData data = new Editor_AreaCharacterData();
        listData.Add(data);
    }

    #region 저장
    public void SaveData(int areaIndex, List<ManagerAreaAnimal.AreaAnimalData_InputData> listAreaAnimalData)
    {
        //기존 리스트에 해당 에리어 인덱스 데이터들 제거
        for (int i = (listAreaAnimalData.Count - 1); i >= 0; i--)
        {
            for (int j = (listAreaAnimalData[i].listRomingAreaIndexData.Count - 1); j >= 0; j--)
            {
                ManagerAreaAnimal.RoamingAreaIndexData checkData = listAreaAnimalData[i].listRomingAreaIndexData[j];
                if (checkData.areaIndex == areaIndex)
                {
                    listAreaAnimalData[i].listRomingAreaIndexData.Remove(checkData);
                }
            }

            if (listAreaAnimalData[i].listRomingAreaIndexData.Count == 0)
                listAreaAnimalData.Remove(listAreaAnimalData[i]);
        }

        //추가할 데이터가 없으면 반환.
        if (listData.Count == 0)
            return;

        //데이터 저장.
        List<int> listRoamingAreaIdx = new List<int>();
        for (int i = 0; i < listData.Count; i++)
        {
            listRoamingAreaIdx.Clear();

            //입력한 값으로, 인덱스 분리해냄.
            string[] arrayIndexText = listData[i].roamingAreaIndex.Split(',');
            for (int j = 0; j < arrayIndexText.Length; j++)
            {
                System.Int32.TryParse(arrayIndexText[j], out int numValue);
                listRoamingAreaIdx.Add(numValue);
            }

            //로밍 에리어 데이터 생성.
            ManagerAreaAnimal.RoamingAreaIndexData roamingAreaIndexData = new ManagerAreaAnimal.RoamingAreaIndexData
            {
                areaIndex = areaIndex,
                listRoamingAreaIdx = new List<int>(listRoamingAreaIdx),
            };

            //캐릭터 찾아서 데이터 넣어주기
            int findIndex = listAreaAnimalData.FindIndex(x => x.charData.charType == listData[i].charData.charType);
            if (findIndex > -1)
            {
                listAreaAnimalData[findIndex].charData.costumeIdx = listData[i].charData.costumeIdx;
                listAreaAnimalData[findIndex].listRomingAreaIndexData.Add(roamingAreaIndexData);
                listAreaAnimalData[findIndex].openMission = listData[i].openMission;
            }
            else
            {
                CharacterAndCostumeData charData = new CharacterAndCostumeData()
                {
                    charType = listData[i].charData.charType,
                    costumeIdx = listData[i].charData.costumeIdx,
                };

                ManagerAreaAnimal.AreaAnimalData_InputData addData = new ManagerAreaAnimal.AreaAnimalData_InputData()
                {
                    charData = charData,
                    listRomingAreaIndexData = new List<ManagerAreaAnimal.RoamingAreaIndexData>(),
                    openMission = listData[i].openMission,
                };
                addData.listRomingAreaIndexData.Add(roamingAreaIndexData);
                listAreaAnimalData.Add(addData);
            }
        }
    }
    #endregion

    #region 로드
    public void LoadData(int areaIndex, List<ManagerAreaAnimal.AreaAnimalData_InputData> listAreaAnimalData)
    {
        listData.Clear();
        if (listAreaAnimalData.Count == 0 || listAreaAnimalData == null)
            return;

        for (int i = 0; i < listAreaAnimalData.Count; i++)
        {
            int findIndex = listAreaAnimalData[i].listRomingAreaIndexData.FindIndex(x => x.areaIndex == areaIndex);
            if (findIndex == -1)
                continue;

            CharacterAndCostumeData charData = new CharacterAndCostumeData
            {
                charType = listAreaAnimalData[i].charData.charType,
                costumeIdx = listAreaAnimalData[i].charData.costumeIdx,
            };

            ManagerAreaAnimal.RoamingAreaIndexData roaminaAreaData = listAreaAnimalData[i].listRomingAreaIndexData[findIndex];
            string areaIndexText = "";
            for (int j = 0; j < roaminaAreaData.listRoamingAreaIdx.Count; j++)
            {
                if (j > 0)
                    areaIndexText += ",";
                areaIndexText += roaminaAreaData.listRoamingAreaIdx[j];
            }

            Editor_AreaCharacterData addData = new Editor_AreaCharacterData()
            {
                charData = charData,
                roamingAreaIndex = areaIndexText,
                openMission = listAreaAnimalData[i].openMission,
            };

            listData.Add(addData);
        }
    }
    #endregion
}
