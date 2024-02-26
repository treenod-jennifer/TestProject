using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Move : UIReuseGridBase
{
    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="itemCount">스크롤 뷰의 열 수</param>
    /// <param name="update">
    /// GameObject : 타겟
    /// int : 업데이트 되는 열의 인덱스(0부터 시작)
    /// </param>
    public void InItGrid(int itemCount, Action<GameObject, int> update)
    {
        onInitializeItem = (GameObject go, int wrapIndex, int realIndex) =>
        {
            go.SetActive(true);
            update?.Invoke(go, Mathf.Abs(realIndex % itemCount));
        };
    }
}