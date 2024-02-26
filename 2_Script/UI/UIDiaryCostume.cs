using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CostumeComparer : IComparer<CdnCostume>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(CdnCostume a, CdnCostume b)
    {
        //캐릭터 id 검사.
        if (a.char_id < b.char_id)
            return -1;
        else if (a.char_id > b.char_id)
            return 1;
        else
        {
            if (UIDiaryController._instance != null)
            {
                bool uKey_A = UIDiaryController._instance.IsEuquipCostume(a);
                bool uKey_B = UIDiaryController._instance.IsEuquipCostume(b);

                //현재 장착 중인 코스튬 검사
                if (uKey_A || uKey_B)
                {
                    if (uKey_A)
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    bool nKey_A = UIDiaryController._instance.IsNewCostume(a);
                    bool nKey_B = UIDiaryController._instance.IsNewCostume(b);
                    
                    if (nKey_A && nKey_B == false)
                        return -1;
                    else if (nKey_A == false && nKey_B)
                        return 1;
                    else
                    {
                        //코스튬 id 검사.
                        if (a.costume_id < b.costume_id)
                            return 1;
                        else if (a.costume_id > b.costume_id)
                            return -1;
                        else
                            return 0;
                    }
                }
            }
            else
            {
                //코스튬 id 검사.
                if (a.costume_id < b.costume_id)
                    return 1;
                else if (a.costume_id > b.costume_id)
                    return -1;
                else
                    return 0;
            }
        }
    }
}

public class UIDiaryCostume : MonoBehaviour
{
    public static UIDiaryCostume _instance = null;

    public UIScrollView scrollView;
    public GameObject categoryRoot;

    private const float COSTUME_START_XPOS = 0f;
    private const float COSTUME_START_YPOS = 420f;
    private const float COSTUME_SPACE_XSIZE = 220f;
    private const float COSTUME_SPACE_YSIZE = -260f;

    //카테고리의 캐릭터 설명있는 박스 사이즈.
    private const float CATEGORI_INFO_YSIZE = 140f;

    private int selectIndex = 0;

    [HideInInspector]
    public List<UIItemDiaryCostumeCategory> itemCategoryList = new List<UIItemDiaryCostumeCategory>();

    //코스튬 데이터 리스트.
    private List<CdnCostume> costumeDataList = new List<CdnCostume>();

    CostumeComparer costumeComparer = new CostumeComparer();

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        InitCostumeItem();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public IEnumerator OpenCategoryTap(int cIndex)
    {
        while (UIPopupDiary._instance.bCanTouch == false)
        {
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;

        for (int i = 0; i < itemCategoryList.Count; i++)
        {
            if (itemCategoryList[i].CheckCharacterIdx(cIndex) == true)
            {
                itemCategoryList[i].OnClickBtnMore();
            }
        }
    }

    private void InitCostumeItem()
    {
        itemCategoryList.Clear();

        //데이터 가져오고 순서대로 정렬해줌.
        GetCostumeData();

        List<CdnCostume> listCategory = new List<CdnCostume>();
        int charId = -1;
        float yPos = COSTUME_START_YPOS;

        //첫번째 ID 가져오기.
        if (costumeDataList.Count > 0)
        {
            charId = costumeDataList[0].char_id;
        }

        //리스트 정보를 바탕으로 카테고리 생성.
        for (int i = 0; i < costumeDataList.Count; i++)
        {  
            //이전 데이터가 있고, 이전 데이터의 캐릭터 인덱스와 현재 검사하는 데이터의 캐릭터 인덱스가 다를 경우,
            //현재까지 리스트에 담아둔 정보로 카테고리 생성 후 다음 카테고의 캐릭터 인덱스를 저장.
            if (charId != costumeDataList[i].char_id)
            {
                yPos = MakeItemDiaryCostume(listCategory, yPos);
                charId = costumeDataList[i].char_id;

                //리스트 데이터 다 지우고 현재 데이터 추가.
                listCategory.Clear();
            }
            listCategory.Add(costumeDataList[i]);

            //현재 데이터가 맨 마지막 데이터일 경우, 현재까지 저장된 리스트 정보를 가지고 카테고리 생성.
            if (i == (costumeDataList.Count - 1))
            {
                MakeItemDiaryCostume(listCategory, yPos);
                listCategory.Clear();
            }
        }
        //스크롤 위치 맨 위로 올림.
        SpringPanel.Begin(scrollView.gameObject, Vector3.zero, 8);

        //newIcon 저장된 playerPrefs 갱신.
        UIDiaryController._instance.SaveCostumeNewIconPlayerPrefs();

        StartCoroutine(OpenCategoryTap(0));
    }

    private void GetCostumeData()
    {
        costumeDataList.Clear();
        costumeDataList = UIDiaryController._instance.GetCostumeData();
        costumeDataList.Sort(costumeComparer);
    }

    private float MakeItemDiaryCostume(List<CdnCostume> listCostume, float yPos)
    {
        float xPos = COSTUME_START_XPOS;
        int itemCount = listCostume.Count;

        //newIconList받아오기.
        List<int> newIconList = new List<int>();
        newIconList = CheckCostumeNewIcon(listCostume[0].char_id);

        UIItemDiaryCostumeCategory itemCostume
            = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemDiaryCostumeCategory", categoryRoot).GetComponent<UIItemDiaryCostumeCategory>();
        itemCostume.transform.localPosition = new Vector3(xPos, yPos, 0);

        // 현재 코스튬 아이템 정보 세팅( 데이터 전달, 현재 착용중인 코스튬인지 검사 ).
        itemCostume.InitCostumeCategory(listCostume, newIconList, OnClickMoreButtonHandler);

        //생성한 아이템 리스트에 추가.
        itemCategoryList.Add(itemCostume);

        //다음 계산해서 위치 반환(아이템 수, 이전 yPos, 한줄만 생성할 지 여부(맨 처음 창 열릴때는 무조건 1줄만 보임)).
        return GetNextYPos(itemCount, yPos, true);
    }

    private List<int> CheckCostumeNewIcon(int housingIdx)
    {
        List<int> newIconList = new List<int>();
        newIconList = UIDiaryController._instance.GetCostumeNewIconPlayerPrefs(housingIdx);
        return newIconList;
    }

    private float GetNextYPos(int itemCnt, float yPos, bool bOneLine)
    {
        int lineCount = 1;
        //more 버튼이 들어갈 공간.
        float yOffset = 0f;
        if (itemCnt > 3)
        {
            //한 줄만 생성하지 않는다면, 실제 라인 수 구함.
            if (bOneLine == false)
                lineCount += (itemCnt / 3);
            yOffset = 10;
        }

        //현재 위치 - 카테고리 정보박스 사이즈 + 카테고리 내 아이템들 줄 간격(간격은 - 간격).
        return yPos - CATEGORI_INFO_YSIZE + (COSTUME_SPACE_YSIZE * lineCount) - yOffset;
    }

    private void OnClickMoreButtonHandler(int index, float yOffset)
    {   
        MoveCategory(index, yOffset);
    }

    private void MoveCategory(int index, float yOffset)
    {
        for (int i = index; i < itemCategoryList.Count; i++)
        {
            float yPos = itemCategoryList[i].transform.localPosition.y + yOffset;
            itemCategoryList[i].transform.DOLocalMoveY(yPos, 0.2f);
        }
    }
}
