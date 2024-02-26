using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSmallBase : UIPopupBase
{
    public GameObject[] check;
    public UILabel[]    title;

    public GameObject activeButton;
    public GameObject checkAllObj;
    public UILabel message;

    protected List<UserData> friends = new List<UserData>();
    protected bool bCheckAll = false;

    public void InitPopUp(List<UserData> friendsList)
    {
        friends = new List<UserData>();

        int nCount = friendsList.Count;
        if (nCount == 6)
        {
            friends = friendsList;
        }
        else
        {
            int r1 = nCount / 6;
            int r2 = nCount % 6;
            int first = 0;
            int last = 1;
            for (int i = 0; i < 6; i++)
            {
                first = (r1 * i);
                last = r1 * (i + 1);
                if (i == 5 && r2 > 0)
                {
                    last += r2;
                }
                int rand = Random.Range(first, last);
                friends.Add(friendsList[rand]);
            }
        }
        SetItem();
    }

    public virtual void OnClickBtnItem(int index)
    {
        OnClickEventSpriteCheck(check[index]);
        UpdateCheckButton();
        UpdateActiveButton();
    }

    public bool OnClickEventSpriteCheck(GameObject spriteObj)
    {
        if (spriteObj.activeInHierarchy == true)
        {
            spriteObj.SetActive(false);
            return false;
        }
        else
        {
            spriteObj.SetActive(true);
            return true;
        }
    }

    public void UpdateCheckButton()
    {
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                checkAllObj.SetActive(false);
                return;
            }
        }
        checkAllObj.SetActive(true);
    }

    public void UpdateActiveButton()
    {
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == true)
            {
                activeButton.SetActive(false);
                return;
            }
        }
        activeButton.SetActive(true);
    }

    protected virtual void SetItem()
    {
        //Debug.Log("SetItem");
    }

    protected virtual void OnClickBtnCheckAll()
    {
        bool bActive = false;
        if (checkAllObj.activeInHierarchy == false)
            bActive = true;

        for (int i = 0; i < 6; i++)
        {
            check[i].SetActive(bActive);
        }
        checkAllObj.SetActive(bActive);

        //비활성화 버튼 삭제.
        if (bActive == true && activeButton.activeInHierarchy == true)
        {
            activeButton.SetActive(false);
        }
        else if(bActive == false && activeButton.activeInHierarchy == false)
        {
            activeButton.SetActive(true);
        }
    }
}
