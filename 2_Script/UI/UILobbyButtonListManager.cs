using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UILobbyButtonListManager : MonoBehaviour
{
    //리스트에 원래부터 있는 ui수.
    public int  startCount = 1;
    //시작 위치 & 사이간격.
    public float startPosX = 0f;
    public float startPosY = 0f;
    public float spaceSize = 100f;

    //DOTween 재생관련.
    private Sequence mySequence;
    private List<GameObject> objList = new List<GameObject>();

    private void Start()
    {
        //objList = new List<GameObject>();
        mySequence = DOTween.Sequence();
    }

    public bool CheckBoxShopButton()
    {
        if (objList == null)
            return false;
        //이벤트 버튼이 이미 존재하는지 확인.
        for (int i = 0; i < objList.Count; i++)
        {
            var eventIcon = objList[i].GetComponent<UIButtonBoxShop>();
            if (eventIcon != null)
            {
                // 하나라도 있으면 false
                return false;
            }
        }
        return true;
    }

    public bool CheckEventButton(int index)
    {
        if (objList == null)
            return false;
        //이벤트 버튼이 이미 존재하는지 확인.
        for (int i = 0; i < objList.Count; i++)
        {
            UIButtonEvent eventIcon = objList[i].GetComponent<UIButtonEvent>();
            if (eventIcon != null)
            {
                //같은 인덱스가 있으면 false 리턴.
                if (eventIcon.GetIndex() == index)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CheckSpecialEventButton(int index)
    {
        if (objList == null)
            return false;
        //이벤트 버튼이 이미 존재하는지 확인.
        for (int i = 0; i < objList.Count; i++)
        {
            UIButtonSpecialEvent eventIcon = objList[i].GetComponent<UIButtonSpecialEvent>();
            if (eventIcon != null)
            {
                //같은 인덱스가 있으면 false 리턴.
                if (eventIcon.GetIndex() == index)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CheckMissionStampEventButton()
    {
        if (objList == null)
            return false;
        //이벤트 버튼이 이미 존재하는지 확인.
        for (int i = 0; i < objList.Count; i++)
        {
            var eventIcon = objList[i].GetComponent<UIButtonMissionStampEvent>();
            if (eventIcon != null)
            {
                // 하나라도 있으면 false
                return false;
            }
        }
        return true;
    }


    public void AddLobbyButton(GameObject obj)
    {
        objList.Add(obj);
        float yPos = startPosY + ((startCount + (objList.Count - 1)) * spaceSize);
        obj.transform.localPosition = new Vector3(startPosX, yPos, 0f);
        RefreshUI();
    }

    public void DestroyLobbyButton(GameObject obj)
    {
        int nCount = objList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if(objList[i].transform.position == obj.transform.position)
            {
                objList.Remove(objList[i]);
                break;
            }
        }
        RefreshUI();
    }

    public void DestroyAllButtons()
    {
        int nCount = objList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (objList[i] != null)
            {
                Destroy(objList[i]);
            }
        }
        objList.Clear();
        RefreshUI();
    }

    public void RefreshUI()
    {
        mySequence.Kill();
        int nCount = objList.Count;
        float yPos = 0f;
        for (int i = 0; i < nCount; i++)
        {
            yPos = startPosY + ((startCount + i) * spaceSize);
            //Debug.Log(objList[i].name);
            Vector3 pos = objList[i].transform.localPosition;
            if (objList[i].transform.localPosition.y != yPos)
            {
                if (UIPopupReady._instance != null)
                {
                    objList[i].transform.localPosition = new Vector3(pos.x, yPos, pos.z);
                }
                else
                {
                    SetPosition(objList[i], yPos);
                }
            }
        }
    }

    private void SetPosition(GameObject obj, float yPos)
    {
        mySequence.Append(obj.transform.DOLocalMoveY(yPos, 0.2f).SetEase(Ease.OutQuint));
    }

    public void LogObjListCount()
    {
        Debug.Log("objList.Count : " + objList.Count);
    }
}
