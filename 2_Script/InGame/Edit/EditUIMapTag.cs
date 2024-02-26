using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditUIMapTag : MonoBehaviour
{
    public enum MapTagState
    {
        used = 0,
        live,
        A,
        B,
        C,
        hard,
    }

    private List<MapTagState> listCurrentMapTag = new List<MapTagState>();
    private List<bool> listTagToggleData = new List<bool>();

    /// <summary>
    /// 읽어온 데이터로 태그 정보 초기화(맵 로드 시 사용)
    /// </summary>
    public void InitTaggingData_ByLoadData()
    {
        listCurrentMapTag.Clear();

        string[] tags = InGameFileHelper2.infoResult.data.tags;
        if (tags != null)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                //받아온 string 데이터를 태그 데이터로 변경
                listCurrentMapTag.Add((MapTagState)System.Enum.Parse(typeof(MapTagState), tags[i]));
            }
        }
        InitTagToggleData();
    }

    /// <summary>
    /// 태그 없는 상태로 태그 정보 초기화(맵 초기화 시 사용)
    /// </summary>
    public void InitTaggingData_Clear()
    {
        listCurrentMapTag.Clear();
        InitTagToggleData();
    }

    public void InitTagToggleData()
    {
        listTagToggleData.Clear();
        int stateCount = System.Enum.GetValues(typeof(MapTagState)).Length;
        for (int i = 0; i < stateCount; i++)
        {
            listTagToggleData.Add(CheckTagState((MapTagState)i));
        }
    }

    private bool CheckTagState(MapTagState tState)
    {
        return (listCurrentMapTag.FindIndex(x => x == tState) != -1);
    }

    public void MapTagUI()
    {
        float xPos = 150f;
        float yPos = -3f;

        //Used(U)
        SetTagToggleUI(xPos, yPos, "(U)", MapTagState.used);
        
        //Live(L)
        xPos += 50f;
        SetTagToggleUI(xPos, yPos, "(L)", MapTagState.live);
        
        //A
        xPos += 50f;
        SetTagToggleUI(xPos, yPos, "(A)", MapTagState.A);
        
        //B
        xPos += 50f;
        SetTagToggleUI(xPos, yPos, "(B)", MapTagState.B);
        
        //C
        xPos += 50f;
        SetTagToggleUI(xPos, yPos, "(C)", MapTagState.C);
    }

    private void SetTagToggleUI(float xPos, float yPos, string toggleName, MapTagState tState)
    {
        int stateIdx = (int)tState;
        bool tagUsed = GUI.Toggle(new Rect(xPos, yPos, 40, 20), listTagToggleData[stateIdx], toggleName);

        //토글 상태가 갱신되었을 때의 처리
        if (tagUsed != listTagToggleData[stateIdx])
        {
            listTagToggleData[stateIdx] = tagUsed;

            if (tagUsed == true)
            {
                if (listCurrentMapTag.FindIndex(x => x == tState) == -1)
                    listCurrentMapTag.Add(tState);
            }
            else
            {
                if (listCurrentMapTag.FindIndex(x => x == tState) != -1)
                    listCurrentMapTag.Remove(tState);
            }
        }
    }

    /// <summary>
    /// (H) 선택 시 맵에 어려움 태그 추가/제거
    /// </summary>
    public void SetHardTag(bool isHard)
    {
        // (H) 선택 시 listCurrentMapTag에 hard가 없다면 hard 태그 추가
        if (isHard)
            if (listCurrentMapTag.FindIndex(x => x == MapTagState.hard) == -1)
                listCurrentMapTag.Add(MapTagState.hard); 
        // (H) 해제 시 listCurrentMapTag에 hard가 있다면 hard 태그 제거
        else
            if (listCurrentMapTag.FindIndex(x => x == MapTagState.hard) != -1)
                listCurrentMapTag.Remove(MapTagState.hard);
    }

    /// <summary>
    /// 현재 맵에 설정된 태그 데이터를 string 배열형태로 가져옴(태그 저장 시 사용)
    /// </summary>
    /// <returns></returns>
    public string[] GetArrayStringTagData()
    {
        int currentTagCount = listCurrentMapTag.Count;
        string[] tags = new string[currentTagCount];
        for (int i = 0; i < currentTagCount; i++)
        {
            tags[i] = listCurrentMapTag[i].ToString();
        }
        return tags;
    }
}
