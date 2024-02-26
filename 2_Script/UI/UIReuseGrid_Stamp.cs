using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_Stamp : UIWrapContent
{
    UIDiaryStamp diaryStamp;

    void Awake()
    {
        onInitializeItem = null;
    }

    protected override void Start()
    {
        SortBasedOnScrollMovement();
        minIndex = ServerRepos.UserStamps.Count * -1;
        if (mScroll != null) mScroll.GetComponent<UIPanel>().onClipMove = OnMove;
        mFirstTime = false;
        StartCoroutine(SetScroll());
        return;
    }

    private IEnumerator SetScroll()
    {
        //스테이지 리스트 읽어오는 동안 기다림.
        while (mPanel == null || mChildren.Count == 0)
        {
            yield return null;
        }

        for (int i = 0; i < mChildren.Count; i++)
        {
            mChildren[i].gameObject.SetActive(false);
        }

        onInitializeItem += OnInitializeItem;
        for (int i = 0; i < mChildren.Count; i++)
        {
            UpdateItem(mChildren[i], i);
        }
    }

    private void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }
        if (go.activeInHierarchy == false)
            go.SetActive(true);

        UIItemStampArray itemStampArray = go.gameObject.GetComponent<UIItemStampArray>();
        itemStampArray.UpdateData(GetStampData(realIndex * -1));
    }

    public ServerUserStamp[] GetStampData(int _nIndex)
    {
        ServerUserStamp[] stampDataArray = new ServerUserStamp[2];
        int _firstIndex = (2 * _nIndex);

        //첫번째 인덱스부터 2개까지의 데이터 반환
        for (int i = 0; i < 2; i++)
        {
            //찾고자 하는 인덱스값이 리스트의 수보다 작을 떄만 데이터 세팅.
            if (_firstIndex + i >= ServerRepos.UserStamps.Count)
                break;

            stampDataArray[i] = ServerRepos.UserStamps[_firstIndex + i];
        }
        return stampDataArray;
    }
}
