using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#region 로밍 데이터 계산
public class RoamingDataComparer : IComparer<RoamingAreaData>
{
    public int Compare(RoamingAreaData a, RoamingAreaData b)
    {
        if (a.areaIndex > b.areaIndex)
            return 1;
        else if (a.areaIndex < b.areaIndex)
            return -1;
        else
            return 0;
    }
}
#endregion

public class EditorAreaAnimal_ViewRoamingData : EditorAreaAnimal_ViewBase
{
    [System.Serializable]
    public class Editor_RoamingData
    {
        public bool isFoldData = false;
        public LobbyAnimalAI.Circle roamingData;
    }

    [SerializeField]
    private List<Editor_RoamingData> listData = new List<Editor_RoamingData>();

    RoamingDataComparer roamingDataComparer = new RoamingDataComparer();


    public override void InitView()
    {
        titleName = "로밍 에리어 데이터";
        dataBoxHeight_normal = 127;
        dataBoxHeight_fold = 35;
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
            viewPos = Draw_RoamingData(i);
        }
    }

    private Vector2 Draw_RoamingData(int index)
    {
        Editor_RoamingData data = listData[index];
        if (data == null)
            return viewPos;

        if (index % 2 == 1)
        {
            int dataBoxHeight = (data.isFoldData == true) ? dataBoxHeight_fold : dataBoxHeight_normal;
            GUI.Box(new Rect(viewPos.x, viewPos.y, viewSize.x, dataBoxHeight), "");
        }
        viewPos.y += 5;

        #region 리스트 타이틀(접기/펼치기)
        GUIStyle style = new GUIStyle();
        if (GUI.Button(new Rect(viewPos.x, viewPos.y, viewSize.x - 30, 22), "", style))
        {
            data.isFoldData = !data.isFoldData;
        }
        string buttonText = string.Format("{0} 데이터 인덱스 : {1}", (data.isFoldData == true ? "▶" : "▼"), index);
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
        if (data.roamingData == null)
            data.roamingData = new LobbyAnimalAI.Circle();

        //centerPos 입력란
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y, 100, 22), "Center (중심)");
        viewPos.y += 25f;
        data.roamingData.center = EditorGUI.Vector3Field(new Rect(viewPos.x + 10, viewPos.y, 200, 25), "", data.roamingData.center);
        viewPos.y += 27f;

        //radius 입력란
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y, 100, 22), "Radius (반지름)");
        data.roamingData.radius = EditorGUI.FloatField(new Rect(viewPos.x + 120, viewPos.y, 50, 22), "", data.roamingData.radius);
        #endregion

        viewPos.y += 40f;
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
        Editor_RoamingData data = new Editor_RoamingData();
        listData.Add(data);
    }

    #region 저장
    public void SaveData(int areaIndex, List<RoamingAreaData> listRoamingAreaData)
    {
        //기존 리스트에 해당 에리어 인덱스 데이터들 제거
        for (int i = (listRoamingAreaData.Count - 1); i >= 0; i--)
        {
            if (listRoamingAreaData[i].areaIndex == areaIndex)
            {
                listRoamingAreaData.Remove(listRoamingAreaData[i]);
            }
        }

        //추가할 데이터가 없으면 반환.
        if (listData.Count == 0)
            return;

        //저장할 데이터 생성
        List<LobbyAnimalAI.Circle> listAddData = new List<LobbyAnimalAI.Circle>();
        for (int i = 0; i < listData.Count; i++)
        {
            LobbyAnimalAI.Circle roamingData = new LobbyAnimalAI.Circle()
            {
                center = listData[i].roamingData.center,
                radius = listData[i].roamingData.radius,
            };
            listAddData.Add(roamingData);
        }

        //기존 리스트에 새로운 데이터를 넣어줌.
        RoamingAreaData addData = new RoamingAreaData()
        {
            areaIndex = areaIndex,
            listRoamingArea = new List<LobbyAnimalAI.Circle>(listAddData),
        };
        listRoamingAreaData.Add(addData);

        //데이터 저장한 뒤, 에리어 순으로 정렬
        listRoamingAreaData.Sort(roamingDataComparer);
    }
    #endregion

    #region 로드
    public void LoadData(int areaIndex, List<RoamingAreaData> listRoamingAreaData)
    {
        listData.Clear();
        if (listRoamingAreaData.Count == 0 || listRoamingAreaData == null)
            return;

        List<RoamingAreaData> listAddData = listRoamingAreaData.FindAll(x => x.areaIndex == areaIndex);
        for (int i = 0; i < listAddData.Count; i++)
        {
            for (int j = 0; j < listAddData[i].listRoamingArea.Count; j++)
            {
                Editor_RoamingData data = new Editor_RoamingData();
                data.roamingData = new LobbyAnimalAI.Circle()
                {
                    center = listAddData[i].listRoamingArea[j].center,
                    radius = listAddData[i].listRoamingArea[j].radius
                };
                listData.Add(data);
            }
        }
    }
    #endregion
}
