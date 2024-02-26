using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorAreaAnimal_ViewBase : ScriptableObject
{
    //뷰 크기
    protected Vector2Int viewSize = new Vector2Int(250, 0);

    //타이틀 박스 크기
    protected int titleHeight = 22;

    //데이터 하나 당 박스 크기
    protected int dataBoxHeight_normal = 127;
    protected int dataBoxHeight_fold = 35;

    //타이틀 명
    protected string titleName = "";

    //제거할 데이터
    protected List<int> listRemoveDataIndex = new List<int>();

    protected Vector2 viewPos;

    /// <summary>
    /// 뷰 초기화(이름설정, 박스 크기 설정 등..)
    /// </summary>
    public virtual void InitView() { }

    /// <summary>
    /// 뷰 그리기
    /// </summary>
    /// <param name="startPos"></param>
    public virtual void Draw(Vector2 startPos)
    {
        listRemoveDataIndex.Clear();

        viewPos = new Vector2(startPos.x, startPos.y);
        CalcurateHeight_View();

        //뷰를 감쌀 박스
        GUI.Box(new Rect(startPos.x, startPos.y, viewSize.x, viewSize.y), "");

        //타이틀
        GUI.Box(new Rect(startPos.x, startPos.y, viewSize.x, titleHeight), "");
        EditorGUI.LabelField(new Rect(viewPos.x + 10, viewPos.y + 3, viewSize.x, titleHeight), titleName);
        viewPos.y += 25f;

        //데이터 뷰 그려주기
        DrawDataView();

        //제거해야 할 데이터 지워주기
        RemoveData();

        //리스트 추가 버튼
        if (IsUseAddDataButton() == true)
        {
            if(GUI.Button(new Rect(viewPos.x, viewPos.y, viewSize.x, 22), "데이터 추가"))
                AddData();
        }
    }

    /// <summary>
    /// 현재 표시될 뷰의 크기를 계산
    /// </summary>
    protected virtual void CalcurateHeight_View() { }

    /// <summary>
    /// 데이터 리스트를 그려주는 함수
    /// </summary>
    protected virtual void DrawDataView() { }

    /// <summary>
    /// 지워야 할 데이터를 제거시켜주는 함수
    /// </summary>
    protected virtual void RemoveData() { }

    /// <summary>
    /// AddListData
    /// </summary>
    protected virtual void AddData() { }

    public Vector2Int GetViewSize() { return viewSize; }

    protected virtual bool IsUseAddDataButton()
    {
        return true;
    }
}
