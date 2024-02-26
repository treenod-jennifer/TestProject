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
    public float spaceSize = 100f;  // 기본 로비버튼 사이즈

    //DOTween 재생관련.
    private Sequence mySequence;
    private List<GameObject> objList = new List<GameObject>();

    private void Start()
    {
        //objList = new List<GameObject>();
        mySequence = DOTween.Sequence();
    }

    public bool CheckButtonAdd<T>() where T : UIButtonEventBase
    {
        return CheckEventButtonAdd<T>(0, false);
    }

    public bool CheckEventButtonAdd<T>(int eventIdx) where T : UIButtonEventBase
    {
        return CheckEventButtonAdd<T>(eventIdx, true);
    }

    bool CheckEventButtonAdd<T>(int eventIdx, bool checkIdx) where T : UIButtonEventBase
    {
        if (objList == null)
            return false;
        //이벤트 버튼이 이미 존재하는지 확인.
        for (int i = 0; i < objList.Count; i++)
        {
            T eventIcon = objList[i].GetComponent<T>();
            if (eventIcon != null)
            {
                if (checkIdx)
                {
                    //같은 인덱스가 있으면 false 리턴.
                    if (eventIcon.GetIndex() == eventIdx)
                    {
                        return false;
                    }
                }
                else
                    return false;   // 인덱스체크 안하는 경우, 같은 종류가 있으면 무조건 추가불가
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

    public void AddLobbyButtonTop(GameObject obj)
    {
        objList.Insert(0, obj);
        obj.transform.localPosition = Vector3.right * startPosX;
        RefreshUI();
    }

    public void DestroyLobbyButton(GameObject obj)
    {
        objList.Remove(obj);
        RefreshUI();
    }

    public void DestroyLobbyButton<T>() where T : UIButtonEventBase
    {
        List<GameObject> newObjList = new List<GameObject>();
        int nCount = objList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (objList[i] != null)
            {
                var comp = objList[i].GetComponent<T>();
                if( comp != null )
                {
                    Destroy(objList[i]);
                    objList[i] = null;
                }
            }

            if (objList[i] != null)
                newObjList.Add(objList[i]);
        }
        objList = newObjList;

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

        objList.RemoveAll((obj) => obj == null);

        int nCount = objList.Count;
        float yPos = startPosY - (spaceSize * 0.5f) + spaceSize * startCount;
        for (int i = 0; i < nCount; i++)
        {
            float yPosOffset = spaceSize;
            var buttonObj = objList[i].GetComponent<UIButtonEventBase>();
            if ( buttonObj != null )
            {
                // 기본 로비버튼 사이즈보다 큰 버튼이 보이는 경우, 그 버튼은 그 크기 만큼 간격을 더 벌리는 처리가 되어있습니다
                // 버튼사이즈 변경은 GetButtonSize에서 리턴하는 값으로 넣어주면 됨
                float btnSize = spaceSize < 0 ? buttonObj.GetButtonSize() * -1 : buttonObj.GetButtonSize();

                yPosOffset = spaceSize < 0 ? Mathf.Min(spaceSize, btnSize) : Mathf.Max(spaceSize, btnSize);
            }

            float iconCenter = yPos + yPosOffset * 0.5f;
            yPos += yPosOffset;
            Vector3 pos = objList[i].transform.localPosition;

            if (UIPopupReady._instance != null)
            {
                objList[i].transform.localPosition = new Vector3(pos.x, iconCenter, pos.z);
            }
            else
            {
                SetPosition(objList[i], iconCenter);
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
