using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemDiaryComplete : MonoBehaviour
{
    public UISprite     completeBoxSprite;
    public UILabel      completeGroupName;

    //더보기 버튼 관련.
    public GameObject   moreButton;
    public UILabel      moreText;

    private const float PROGRESS_START_XPOS = -230f;
    private const float PROGRESS_START_YPOS = -68f;
    private const float PROGRESS_SPACE_XSIZE = 230f;
    private const float PROGRESS_SPACE_YSIZE = -137f;

    [HideInInspector]
    public int housingIndex;

    [HideInInspector]
    public List<UIItemDiaryHousing_C> itemHousingCList = new List<UIItemDiaryHousing_C>();

    private int pIndex = 0;
    private List<PlusHousingModelData> housingData;
    private bool bOpen = false;

    public float InitItemComplete(List<PlusHousingModelData> data, int index)
    {
        float nextPosition = 0f;
        pIndex = index;
        housingData = new List<PlusHousingModelData>();
        for (int i = 0; i < data.Count; i++)
        {
            housingData.Add(data[i]);
        }
        //해당 카테고리 명 설정.
        housingIndex = data[0].housingIndex;
        //해당 카테고리 명 설정.
        string fileName = string.Format("h_{0}", housingIndex);
        completeGroupName.text = Global._instance.GetString(fileName);

        //목록 중 첫 줄 까지만 미리 생성.
        itemHousingCList.Clear();
        Vector2 completePos = new Vector2(PROGRESS_START_XPOS, PROGRESS_START_YPOS);
        for (int i = 0; i < housingData.Count; i++)
        {
            if (i > 3)
                break;
            UIItemDiaryHousing_C itemHousingC = NGUITools.AddChild(gameObject, UIDiaryStorage._instance._objItemDiaryHousingC).GetComponent<UIItemDiaryHousing_C>();
            itemHousingC.InitItemHousing_C(housingData[i]);
            itemHousingC.transform.localPosition = completePos;

            itemHousingCList.Add(itemHousingC);
            completePos.x += PROGRESS_SPACE_XSIZE;
        }

        //현재 카테고리에 아이템이 3개가 넘지 않는다면, more버튼 생성안함.
        if (housingData.Count <= 3)
        {
            moreButton.SetActive(false);
            //카테고리 상자 크기 설정.
            completeBoxSprite.height = 147;
        }
        //more버튼이 활성화 된 상태라면, get 버튼 안눌러지도록 설정.
        else
        {
            //itemHousingC.ActiveColliderGetButton(false);
            //카테고리 상자 크기 설정.
            completeBoxSprite.height = 175;
            nextPosition += 5;
        }
        nextPosition += completeBoxSprite.height;

        return nextPosition;
    }

    public float OnClickBtnMore()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return 0.0f;

        //데이터 수가 3개 이하일 경우, 반환.
        if (housingData.Count <= 3)
            return 0.0f;
        
        float moveTime = 0.2f;
        int nCount = housingData.Count;

        //다음 목록들의 위치는 target위치에서 한칸(기존에 있던 한칸) 뺀 간격 + 여분 공간.
        float nextPosY = 132f * ((nCount - 1) / 3);
        //현재 카테고리가 닫혀있는 상태라면(목록 열 때).
        if (bOpen == false)
        {
            //현재 카테고리의 데코 목록 생성.
            StartCoroutine(CoMakeComplete());

            //카테고리 상자 크기 설정.
            int target = 132 + (132 * ((nCount - 1) / 3));
            DOTween.To(() => completeBoxSprite.height, x => completeBoxSprite.height = x, target + 35, moveTime);
            
            //현재 카테고리 아래의 목록들 밑으로 이동.
            UIDiaryStorage._instance.OpenComplete(pIndex + 1, nextPosY, true);

            //더보기 버튼 설정.
            moreText.text = Global._instance.GetString("p_d_d_3");
            float morePosY = moreButton.transform.localPosition.y - target + 132f;
            moreButton.transform.DOLocalMoveY(morePosY, moveTime);

            bOpen = true;
        }
        //현재 카테고리가 열려있는 상태라면(목록 닫을 때).
        else
        {
            //현재 카테고리의 데코 목록 맨 위에 한줄빼고 삭제.
            for (int i = (itemHousingCList.Count - 1); i > 3; i--)
            {
                GameObject destroyObj = itemHousingCList[i].gameObject;
                itemHousingCList.Remove(itemHousingCList[i]);
                Destroy(destroyObj);
            }
            //현재 카테고리 아래의 목록들 위로 이동.
            int target = 175;
            DOTween.To(() => completeBoxSprite.height, x => completeBoxSprite.height = x, target, moveTime);
            UIDiaryStorage._instance.OpenComplete(pIndex + 1, nextPosY, false);

            //더보기 버튼 설정.
            moreText.text = Global._instance.GetString("p_d_d_4");
            float morePosY = moreButton.transform.localPosition.y + (132f * ((nCount - 1) / 3));
            moreButton.transform.DOLocalMoveY(morePosY, moveTime);
            bOpen = false;
        }
        return moveTime;
    }

    private IEnumerator CoMakeComplete()
    {
        yield return new WaitForSeconds(0.1f);

        //데이터 수가 3개 이하일 경우, 반환.
        //more 버튼 생성될때만 눌러져서 이런 경우 없지만, 혹시 생길경우 있을까봐 추가.
        if (housingData.Count <= 3)
            yield break;

        Vector2 completePos = new Vector2(PROGRESS_START_XPOS, PROGRESS_START_YPOS);
        for (int i = 3; i < housingData.Count; i++)
        {
            UIItemDiaryHousing_C itemHousingC = NGUITools.AddChild(gameObject, UIDiaryStorage._instance._objItemDiaryHousingC).GetComponent<UIItemDiaryHousing_C>();
            itemHousingC.InitItemHousing_C(housingData[i]);
            if (i >= 3 && (i % 3 == 0))
            {
                completePos.x = PROGRESS_START_XPOS;
                completePos.y += PROGRESS_SPACE_YSIZE;
            }
            itemHousingC.transform.localPosition = completePos;

            //알파.
            itemHousingC.mainSprite.alpha = 0;
            DOTween.ToAlpha(() => itemHousingC.mainSprite.color, x => itemHousingC.mainSprite.color = x, 1, 0.2f);
            itemHousingCList.Add(itemHousingC);
            completePos.x += PROGRESS_SPACE_XSIZE;
        }
    }
}
