using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemDiaryCostumeCategory : MonoBehaviour
{
    public UISprite     categoryBox;
    public UILabel[]    characterName;
    public UILabel      characterInfo;
    public UITexture    characterImage;
    public GameObject   costumeRoot;

    //more 버튼 관련.
    public GameObject moreButton;
    public UILabel moreButtonText;

    //more버튼 눌렀을 때 실행 될 함수.
    public System.Action<int, float> moreButtonHandler;

    private const float CATEGORY_START_XPOS = -220f;
    private const float CATEGORY_START_YPOS = -10f;
    private const float CATEGORY_SPACE_XSIZE = 220f;
    private const float CATEGORY_SPACE_YSIZE = -250f;
    
    //현재 카테고리의 캐릭터 인덱스를 가지고 있는 변수.
    private int characterIndex = 0;
    //현재 선택된 아이템의 인덱스를 가지고 있는 변수.
    private int selectIndex = 0;

    //more 버튼 누른상태인지.
    private bool bOpen = false;

    private List<CdnCostume> listCostumeData = new List<CdnCostume>();
    private List<UIItemDiaryCostume> itemDiaryCostumeList = new List<UIItemDiaryCostume>();
    private List<int> listNewIcon = new List<int>();

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void OnLoadComplete(Texture2D r)
    {
        characterImage.mainTexture = r;
        characterImage.MakePixelPerfect();
    }

    public bool CheckCharacterIdx(int idx)
    {
        if (characterIndex == idx)
            return true;
        return false;
    }

    public void InitCostumeCategory(List<CdnCostume> listCostume, List<int> newIconList, System.Action<int, float> moreButtonClick)
    {
        moreButtonHandler = moreButtonClick;
        listNewIcon = newIconList;
        itemDiaryCostumeList.Clear();
        SettingSelectCostumeIndex();
        SettingCostumeData(listCostume);
        MakeItemDiaryCostume();
    }

    private void SettingSelectCostumeIndex()
    {
        ServerUserCostume costumeInfo = ServerRepos.UserCostumes.Find(x => x.is_equip == 1);
        if (costumeInfo != null)
        {
            selectIndex = costumeInfo.costume_id;
        }
        else
        {   //현재 코스튬 착용중인 상태가 없으면 기본형으로 설정.
            selectIndex = 0;
        }
    }

    private void SettingCostumeData(List<CdnCostume> listCostume)
    {
        for (int i = 0; i < listCostume.Count; i++)
        {
            listCostumeData.Add(listCostume[i]);
        }
    }

    private void MakeItemDiaryCostume()
    {
        int dataCnt = listCostumeData.Count;
        if (dataCnt > 0)
            characterIndex = listCostumeData[0].char_id;

        //코스튬 아이템들 생성.
        MakeCostumeItem();

        //카테고리 박스 사이즈 설정.
        categoryBox.height = SettingDefaultCategoryBoxHeight();

        //more버튼 세팅.
        MakeMoreButton(dataCnt);

        //카테고리 캐릭터, 이름, 캐릭터 설명 등록.
        SettingCharacterNameAndInfo();
        SettingCharacterImage();
    }

    private void MakeCostumeItem()
    {
        float xPos = CATEGORY_START_XPOS;
        float yPos = CATEGORY_START_YPOS;

        //카테고리 내 코스튬 아이콘 추가.
        for (int i = 0; i < listCostumeData.Count; i++)
        {
            bool bNewIcon = CheckNewIcon(listCostumeData[i].idx, listNewIcon);

            UIItemDiaryCostume itemCostume
                = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemDiaryCostume", costumeRoot).GetComponent<UIItemDiaryCostume>();
            itemCostume.transform.localPosition = new Vector3(xPos, yPos, 0);
            bool bSelect = selectIndex == listCostumeData[i].costume_id;
            //현재 코스튬 아이템 정보 세팅(현재 선택된 코스튬 인덱스, 코스튬 데이터, 코스튬 선택 후 핸들러 전달).
            itemCostume.InitItemDiaryCostume(bSelect, bNewIcon, listCostumeData[i], OnSelectCostumeHandler);
            xPos += CATEGORY_SPACE_XSIZE;
            //생성한 아이템 리스트에 추가.
            itemDiaryCostumeList.Add(itemCostume);

            //창 처음 열때는 아이템 3개까지만 생성.
            if (i >= 2)
                break;
        }
    }

    private bool CheckNewIcon(int modelIndex, List<int> newIconList)
    {
        if (newIconList == null)
            return false;
        for (int i = 0; i < newIconList.Count; i++)
        {
            if (newIconList[i] == modelIndex)
                return true;
        }
        return false;
    }

    private void MakeMoreButton(int itemCount)
    {
        if (itemCount <= 3)
        {
            moreButton.SetActive(false);
            return;
        }

        int lineCnt = (itemCount / 3) + 1;
        if (itemCount > 3)
        {
            float yPos = CATEGORY_START_YPOS + CATEGORY_SPACE_YSIZE - 30;
            moreButton.transform.localPosition = new Vector3(0, yPos, 0);
            moreButton.SetActive(true);
            //카테고리 상자 크기 설정.
            categoryBox.height += 10;
        }
    }

    private int SettingDefaultCategoryBoxHeight()
    {
         return ((int)CATEGORY_SPACE_YSIZE * -1) + 20;
    }

    private void SettingCharacterNameAndInfo()
    {
        string nameKey = string.Format("c_n_{0}", characterIndex);
        string infoKey = string.Format("c_i_{0}", characterIndex);

        // NOTE: 2019 만우절
        if (ServerRepos.LoginCdn.aprilFool != 0)
        {
            infoKey += "_190401";
        }

        string charName = Global._instance.GetString(nameKey);

        characterName[0].text = charName;
        characterName[1].text = charName;
        characterInfo.text = Global._instance.GetString(infoKey);
        SettingCharInfoPostion();
    }
    
    private void SettingCharInfoPostion()
    {
        int _nLineCharCount = (int)(characterName[0].printedSize.x / characterName[0].fontSize);
        float xPos = 75f + (_nLineCharCount * 30f);
        characterInfo.transform.localPosition = new Vector3(xPos, 0f, 0f);
    }

    private void SettingCharacterImage()
    {
        // 만우절
        if (ServerRepos.LoginCdn.aprilFool != 0)
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconAprilFool", $"char_{ServerRepos.LoginCdn.aprilFool}", OnLoadComplete, true);
        }
        else
        {
            characterImage.mainTexture = Box.LoadResource<Texture2D>($"UI/char_{characterIndex}");
            characterImage.MakePixelPerfect();
        }
    }

    private void OnSelectCostumeHandler(int index)
    {
        if (selectIndex != index)
            SelectCostume(index);
    }

    private void SelectCostume(int cIndex)
    {
        for (int i = 0; i < itemDiaryCostumeList.Count; i++)
        {
            //현재 선택된 아이템의 체크 버튼 해제.
            if (itemDiaryCostumeList[i].CheckSelectIndex(selectIndex) == true)
                break;
        }
        selectIndex = cIndex;
    }

    public void OnClickBtnMore()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
            return;

        int nCount = listCostumeData.Count;

        //more 버튼 뜨는 조건에 해당하지 않을 경우.
        if (nCount <= 3)
            return;
        int lineCount = 0;
        lineCount = ((nCount - 1) / 3) + 1;

        //다음 카테고리가 옮겨질 위치.
        float nextPosY = CATEGORY_SPACE_YSIZE * (lineCount - 1);

        //현재 카테고리가 닫혀있는 상태라면(목록 열 때).
        if (bOpen == false)
        {
            //현재 카테고리의 데코 목록 생성.
            StartCoroutine(CoMakeProgress());

            //카테고리 상자 크기 설정.
            int target = ((int)CATEGORY_SPACE_YSIZE * lineCount * -1) + 20;
            DOTween.To(() => categoryBox.height, x => categoryBox.height = x, (target + 10), 0.2f);

            //더보기 버튼 설정.
            moreButtonText.text = Global._instance.GetString("p_d_c_1");
            float morePosY = CATEGORY_START_YPOS + (CATEGORY_SPACE_YSIZE * lineCount) - 30;
            moreButton.transform.DOLocalMoveY(morePosY, 0.2f);
            
            //현재 카테고리 아래의 목록들 밑으로 이동.
            moreButtonHandler(characterIndex + 1, nextPosY);
            bOpen = true;
        }
        //현재 카테고리가 열려있는 상태라면(목록 닫을 때).
        else
        {
            //현재 카테고리의 데코 목록 삭제.
            for (int i = (itemDiaryCostumeList.Count - 1); i > 2; i--)
            {
                GameObject destroyObj = itemDiaryCostumeList[i].gameObject;
                itemDiaryCostumeList.Remove(itemDiaryCostumeList[i]);
                Destroy(destroyObj);
            }
            //현재 카테고리 아래의 목록들 위로 이동.
            int target = SettingDefaultCategoryBoxHeight();
            DOTween.To(() => categoryBox.height, x => categoryBox.height = x, target, 0.2f);

            //더보기 버튼 설정.
            moreButtonText.text = $"{Global._instance.GetString("p_d_c_2")}";
            float morePosY = CATEGORY_START_YPOS + CATEGORY_SPACE_YSIZE - 20;
            moreButton.transform.DOLocalMoveY(morePosY, 0.2f);

            //현재 카테고리 아래의 목록들 위로 이동.
            moreButtonHandler(characterIndex + 1, -nextPosY);

            bOpen = false;

            UIDiaryCostume._instance.scrollView.ResetPosition();
        }
    }

    private IEnumerator CoMakeProgress()
    {
        yield return new WaitForSeconds(0.1f);
        int nCount = listCostumeData.Count;

        float xPos = CATEGORY_START_XPOS;
        float yPos = CATEGORY_START_YPOS;

        if (listCostumeData.Count > 3)
        {
            for (int i = 3; i < nCount; i++)
            {
                bool bNewIcon = CheckNewIcon(listCostumeData[i].idx, listNewIcon);

                UIItemDiaryCostume itemCostume
                  = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemDiaryCostume", costumeRoot).GetComponent<UIItemDiaryCostume>();
                bool bSelect = selectIndex == listCostumeData[i].costume_id;
                //현재 코스튬 아이템 정보 세팅(데이터 전달, 코스튬 선택 후 핸들러 전달).
                itemCostume.InitItemDiaryCostume(bSelect, bNewIcon, listCostumeData[i], OnSelectCostumeHandler);

                // 한줄에 3개씩 정렬되므로, 
                // 3번째 칸 다음 아이템들은 xPos을 기존 위치로 고정시켜주고 yPos은 다음 칸 간격만큼 증가.
                if (i> 2 && i % 3 == 0)
                {
                    xPos = CATEGORY_START_XPOS;
                    yPos += CATEGORY_SPACE_YSIZE;
                }
                else
                    xPos += CATEGORY_SPACE_XSIZE;

                //위치 이동.
                itemCostume.transform.localPosition = new Vector3(xPos, yPos, 0);
                //알파.
                itemCostume.backImage.alpha = 0;
                DOTween.ToAlpha(() => itemCostume.backImage.color, x => itemCostume.backImage.color = x, 1, 0.2f);

                //생성한 아이템 리스트에 추가.
                itemDiaryCostumeList.Add(itemCostume);
            }
        }
    }
}
