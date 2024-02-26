using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemDiaryProgress : MonoBehaviour
{
    public UISprite progressBoxSprite;
    public UILabel progressGroupName;

    //더보기 버튼 관련.
    public GameObject moreButton;
    public GameObject alarmIcon;
    public UILabel alarmCountText;
    public UILabel moreText;

    private const float PROGRESS_START_XPOS = 55f;
    private const float PROGRESS_START_YPOS = -35f;
    private const float PROGRESS_SPACE_SIZE = -132f;
    private const float COLLIDER_SPACE_SIZE = 72f;

    [HideInInspector]
    public int housingIndex;

    [HideInInspector]
    public List<UIItemDiaryHousing_P> itemHousingPList = new List<UIItemDiaryHousing_P>();

    private int pIndex = 0;
    private List<PlusHousingModelData> housingData;
    private bool bOpen = false;
    private bool bGetDeco = false;

    //미리 보이는 데코 수.
    private int lookDecoCount = 0;

    public int GetDataCount()
    {
        return housingData.Count;
    }

    public float InitItemProgress(List<PlusHousingModelData> data, List<int> newIconList, int index)
    {
        float nextPosition = 5f;
        pIndex = index;
        housingData = new List<PlusHousingModelData>();
        for (int i = 0; i < data.Count; i++)
        {
            housingData.Add(data[i]);
        }
        //해당 카테고리 명 설정.
        housingIndex = data[0].housingIndex;
        string fileName = string.Format("h_{0}", housingIndex);
        progressGroupName.text = Global._instance.GetString(fileName);

        //목록 중 이벤트 데코가 있으면 이벤트 데코 미리 생성.
        lookDecoCount = 0;
        itemHousingPList.Clear();

        float yPos = PROGRESS_START_YPOS;
        for (int i = 0; i < housingData.Count; i++)
        {
            bool bNewIcon = CheckNewIcon(housingData[i].modelIndex, newIconList);

            //현재 데코가 이벤트이거나, 새로 생긴 데코일 경우.
            if (housingData[i].expire_ts > 0 || bNewIcon == true)
            {
                UIItemDiaryHousing_P itemHousingP = NGUITools.AddChild(gameObject, UIDiaryStorage._instance._objItemDiaryHousingP).GetComponent<UIItemDiaryHousing_P>();
                itemHousingP.InitItemHousing_P(housingData[i], bNewIcon);
                yPos = PROGRESS_START_YPOS + (PROGRESS_SPACE_SIZE * i);
                itemHousingP.transform.localPosition = new Vector3(PROGRESS_START_XPOS, yPos, 0);
                itemHousingPList.Add(itemHousingP);
                lookDecoCount++;
            }
        }
        //이벤트 데코나 새로 생긴 데코가 없으면, 맨 위에 있는 데코 하나 생성.
        if (lookDecoCount == 0 && housingData.Count > 0)
        {
            UIItemDiaryHousing_P itemHousingP = NGUITools.AddChild(gameObject, UIDiaryStorage._instance._objItemDiaryHousingP).GetComponent<UIItemDiaryHousing_P>();
            itemHousingP.InitItemHousing_P(housingData[0], false);
            itemHousingP.transform.localPosition = new Vector3(PROGRESS_START_XPOS, yPos, 0);
            itemHousingPList.Add(itemHousingP);
            lookDecoCount = 1;
        }

        //현재 카테고리에 미리 생성된 아이템 외에 다른 아이템들이 없으면 more버튼 생성안함.
        if (housingData.Count - lookDecoCount <= 0)
        {
            moreButton.SetActive(false);
            //카테고리 상자 크기 설정.
            progressBoxSprite.height = (132 * lookDecoCount) + 10;
        }
        //more버튼이 활성화 된 상태라면, get 버튼 안눌러지도록 설정.
        else
        {
            //더보기 버튼 설정.
            moreText.text = $"{Global._instance.GetString("p_d_d_4")}({lookDecoCount}/{housingData.Count})";
            if (lookDecoCount > 1)
            {
                float morePosY = moreButton.transform.localPosition.y - (132f * (lookDecoCount - 1));
                moreButton.transform.localPosition = new Vector3(moreButton.transform.localPosition.x, morePosY, 0);
            }

            //카테고리 상자 크기 설정(more 버튼이 가려질 정도의 크기).
            progressBoxSprite.height = (132 * lookDecoCount) + 50;
            nextPosition += 5;
        }
        nextPosition += progressBoxSprite.height;
        return nextPosition;
    }

    public void MoveProgressItemPos(int _nDeleteIndex, int getCount = 0)
    {
        int target = progressBoxSprite.height + (int)PROGRESS_SPACE_SIZE;
        DOTween.To(() => progressBoxSprite.height, x => progressBoxSprite.height = x, target, 0.2f).SetEase(Ease.Linear);
        if (target <= 0)
        {
            DOTween.ToAlpha(() => progressGroupName.color, x => progressGroupName.color = x, 0f, 0.2f);
        }

        int nCount = itemHousingPList.Count;
        for (int i = _nDeleteIndex; i < nCount; i++)
        {
            itemHousingPList[i].transform.DOLocalMoveY(itemHousingPList[i].transform.localPosition.y - PROGRESS_SPACE_SIZE, 0.2f, true).SetEase(Ease.Linear);
        }

        //현재 카테고리에 아직 아이템이 남아있으면 moreButton 옮겨줌.
        if (housingData.Count - 1 > 0)
        {
            moreButton.transform.DOLocalMoveY(moreButton.transform.localPosition.y - PROGRESS_SPACE_SIZE, 0.2f).SetEase(Ease.Linear);

            //더보기 버튼 설정.
            int dataCnt = housingData.Count - 1;
            //현재 카테고리가 열린 상태가 아니면 text 갱신.
            if (nCount < dataCnt)
            {
                moreText.text = $"{Global._instance.GetString("p_d_d_4")}({nCount}/{dataCnt})";
            }

            //획득할 수 있는 데코 부분 표시.
            if (bGetDeco == true && bOpen == false)
            {
                if (getCount <= 0)
                    alarmIcon.SetActive(false);
                else
                    alarmCountText.text = getCount.ToString();
            }
        }
    }

    public void OnClickBtnMore()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return;

        int nCount = housingData.Count;

        //more 버튼 뜨는 조건에 해당하지 않을 경우.
        //(미리 생성된 아이템의 수를 제외한 데코가 없을 경우)
        if (nCount - lookDecoCount <= 0)
            return;

        //다음 카테고리가 옮겨질 위치.
        //다음 위치는 이미 생성된 아이템들을 뺀 위치.
        float nextPosY = 132 * (nCount - lookDecoCount);

        if (bOpen == false)
        {   //현재 카테고리가 닫혀있는 상태라면(목록 열 때).
            //현재 카테고리의 데코 목록 생성.
            StartCoroutine(CoMakeProgress());

            //카테고리 상자 크기 설정.
            int target = 132 * nCount;
            DOTween.To(() => progressBoxSprite.height, x => progressBoxSprite.height = x, (target + 35), 0.2f);

            //현재 카테고리 아래의 목록들 밑으로 이동.
            UIDiaryStorage._instance.OpenProgress(pIndex + 1, nextPosY, true);

            //더보기 버튼 설정.
            moreText.text = Global._instance.GetString("p_d_d_3");
            float morePosY = moreButton.transform.localPosition.y - (132f * (nCount - lookDecoCount));
            moreButton.transform.DOLocalMoveY(morePosY, 0.2f);

            bOpen = true;
        }
        else
        {   //현재 카테고리가 열려있는 상태라면(목록 닫을 때).
            //현재 카테고리의 데코 목록 삭제.
            for (int i = (itemHousingPList.Count - 1); i > (lookDecoCount - 1); i--)
            {
                GameObject destroyObj = itemHousingPList[i].gameObject;
                itemHousingPList.Remove(itemHousingPList[i]);
                Destroy(destroyObj);
            }
            //현재 카테고리 아래의 목록들 위로 이동.
            int target = (132 * lookDecoCount) + 50;
            DOTween.To(() => progressBoxSprite.height, x => progressBoxSprite.height = x, target, 0.2f);
            UIDiaryStorage._instance.OpenProgress(pIndex + 1, nextPosY, false);

            //더보기 버튼 설정.
            moreText.text = $"{Global._instance.GetString("p_d_d_4")}({lookDecoCount}/{nCount})";
            float morePosY = moreButton.transform.localPosition.y + (132f * (nCount - lookDecoCount));
            moreButton.transform.DOLocalMoveY(morePosY, 0.2f);

            bOpen = false;
        }
        GetDecoCountSetting();
    }

    public void InitCategoryAlarm(bool bAlarm, string count = "")
    {
        alarmIcon.SetActive(bAlarm);
        bGetDeco = bAlarm;
        if(bAlarm == true)
            alarmCountText.text = count;
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

    private IEnumerator CoMakeProgress()
    {
        yield return new WaitForSeconds(0.1f);
        int nCount = housingData.Count;
        for (int i = lookDecoCount; i < nCount; i++)
        {
            UIItemDiaryHousing_P itemHousingP = NGUITools.AddChild(gameObject, UIDiaryStorage._instance._objItemDiaryHousingP).GetComponent<UIItemDiaryHousing_P>();
            itemHousingP.InitItemHousing_P(housingData[i], false);
            float yPos = PROGRESS_START_YPOS + (PROGRESS_SPACE_SIZE * i);
            itemHousingP.transform.localPosition = new Vector3(PROGRESS_START_XPOS, yPos, 0);

            //알파.
            itemHousingP.mainSprite.alpha = 0;
            DOTween.ToAlpha(() => itemHousingP.mainSprite.color, x => itemHousingP.mainSprite.color = x, 1, 0.2f);

            itemHousingPList.Add(itemHousingP);
        }
    }

    private void GetDecoCountSetting()
    {
        if (bGetDeco == false)
            return;
        alarmIcon.SetActive(!bOpen);
    }
}
