using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReuseGrid_StageAdventure_Animal : UIReuseGridBase
{
    private int listSize;
    protected override void Awake()
    {
        base.Awake();
        onInitializeItem += OnInitializeItem;
    }

    public void InitReuseGrid()
    {
        listSize = UIPopupStageAdventureAnimal._instance.characterList.Count;
        minIndex = ((listSize - 1) * -1) / 3;
    }

    public void ScrollItemMove(int iitemIndex, int itemOffset = 0)
    {
        StartCoroutine(CoScrollItemMove(iitemIndex, itemOffset));
    }

    private IEnumerator CoScrollItemMove(int iitemIndex, int itemOffset = 0)
    {
        iitemIndex = iitemIndex - itemOffset;
        int item_count = listSize;

        float pos = itemSize * iitemIndex;
        float viewHeight = transform.GetComponentInParent<UIPanel>().GetViewSize().y;
        float start_pivot_vertical = ((transform.position.y - (itemSize * 0.5f)) - (transform.parent.transform.position.y - (viewHeight * 0.5f)));
        float maxHeight = (item_count * itemSize) - viewHeight;

        pos = Mathf.Clamp(pos, 0.0f, maxHeight >= 0.0f ? maxHeight : 0.0f) + start_pivot_vertical;

        while (mPanel == null)
            yield return null;

        SpringPanel.Begin(mPanel.gameObject, Vector3.up * pos, 8.0f);
    }

    //재사용을 위해 위치이동을 하는 Item이 있을때 위치이동을 한 Item의 갯수 만큼 호출
    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        realIndex = Mathf.Abs(realIndex) * 3;

        UIItemAdventureAnimalInfo[] animalInfo = go.GetComponentsInChildren<UIItemAdventureAnimalInfo>(true);

        for(int i=0; i< animalInfo.Length; i++)
        {
            if (realIndex + i < listSize)
            {
                animalInfo[i].gameObject.SetActive(true);
                animalInfo[i].SetAnimalSelect(UIPopupStageAdventureAnimal._instance.characterList[realIndex + i]);
            }
            else
                animalInfo[i].gameObject.SetActive(false);
        }

        #region 탭 설명문 세팅
        var tabExplanation = go.GetComponentInChildren<UIItemAdventureAnimalTabExplanation>(true);

        if(realIndex == 0)
        {
            tabExplanation.SettingText();
            tabExplanation.gameObject.SetActive(true);
        }
        else
        {
            tabExplanation.gameObject.SetActive(false);
        }
        #endregion
    }


    public void ScrollReset(bool repositioning = false)
    {
        InitReuseGrid();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);

            #region 스크롤뷰 아이템 내부의 3개의 UIItemStageAdventure_AnimalSelect에 각각 값을 세팅한다.
            Transform scrollItem = transform.GetChild(i);
            UIItemAdventureAnimalInfo[] animalInfo = scrollItem.GetComponentsInChildren<UIItemAdventureAnimalInfo>(true);

            int realIndex_i = Mathf.RoundToInt(Mathf.Abs(transform.GetChild(i).transform.localPosition.y) / itemSize);
            for (int j = 0; j < animalInfo.Length; j++)
            {
                int realIndex_j = (realIndex_i * 3) + j;
                    
                if (listSize > realIndex_j)
                {
                    animalInfo[j].gameObject.SetActive(true);
                    animalInfo[j].SetAnimalSelect(UIPopupStageAdventureAnimal._instance.characterList[realIndex_j]);
                }
                else
                {
                    animalInfo[j].gameObject.SetActive(false);
                }
            }
            #endregion

            #region 탭 설명문 세팅
            var tabExplanation = scrollItem.GetComponentInChildren<UIItemAdventureAnimalTabExplanation>(true);

            if(realIndex_i == 0)
            {
                tabExplanation.SettingText();
                tabExplanation.gameObject.SetActive(true);
            }
            else
            {
                tabExplanation.gameObject.SetActive(false);
            }
            #endregion

            //items[i].localPosition = new Vector3(items[i].localPosition.x, i * itemSize * -1.0f, items[i].localPosition.z);
        }

        if (repositioning)
        {
            var scrollBar = transform.parent.GetComponent<UIReusableScrollView>().verticalScrollBar;
            scrollBar.value = 0;
            EventDelegate.Execute(scrollBar.onChange);
        }
    }
}
