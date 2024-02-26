using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIPopupGimmickTutorial : UIPopupBase
{
    //버튼 최대 6개까지
    public const int MAX_TUTORIALITEMBUTTON = 6;
    //버튼 가로 (여백까지)
    public const int WIDTH_TUTORIALITEMBUTTON = 100;

    //타이틀 정보
    [SerializeField]
    private UILabel[] title;

    //튜토리얼 콘텐츠
    [SerializeField]
    private UIGimmickTutorial_Contents gimmickTutorialContents;

    //그리드 관련
    [SerializeField] public UISprite[] gridSprites;
    [SerializeField] UIGrid itemGrid;
    [SerializeField] public GimmickTutorialItem[] gimmickTutorialItemArray;

    //시작 번호
    private int gimmickTutorialCount = -1;

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        GameUIManager.instance.gimmickTutorialPopup = null;
        base.ClosePopUp();
    }

    public void InitPopup()
    {
        //아이템 그리드 설정
        InitGrid();

        //0번으로 팝업 내용 세팅
        UpdatePopup(gimmickTutorialItemArray[0].gimmickData);

        //0번 아이템버튼 클릭 상태로 만들기
        gimmickTutorialItemArray[0].SetButtonSprite(true);
    }

    private void UpdatePopup(GimmickTutorial_Data data)
    {
        //타이틀 설정
        foreach (var label in title)
            label.text = data.gimmickName;

        //튜토리얼 내용 설정
        gimmickTutorialContents.Init(data.contentsList);
    }

    private void InitGrid()
    {
        for (int i = 0; i < gimmickTutorialItemArray.Length; i++)
        {
            if (gimmickTutorialItemArray[i] != null) gimmickTutorialItemArray[i].gameObject.SetActive(false);
        }

        //데이터 세팅
        InitItemArray();

        //위치 조정
        if (gimmickTutorialCount > 0)
        {
            //그리드 크기
            foreach (var sprite in gridSprites)
            {
                sprite.width -= (MAX_TUTORIALITEMBUTTON - gimmickTutorialCount) * WIDTH_TUTORIALITEMBUTTON;
            }
            itemGrid.Reposition();
        }
    }

    private void InitItemArray()
    {
        List<GimmickTutorial_Data> _dataList = ManagerGimmickTutorial.instance.gimmickTutorialList;
        gimmickTutorialCount = _dataList.Count > MAX_TUTORIALITEMBUTTON ? MAX_TUTORIALITEMBUTTON : _dataList.Count;

        for (int i = 0; i < gimmickTutorialCount; i++)
        {
            gimmickTutorialItemArray[i].gameObject.SetActive(true);
            gimmickTutorialItemArray[i].Init(_dataList[i]);

            //아이템 버튼 동작 설정
            gimmickTutorialItemArray[i].clickEvent += (GimmickTutorial_Data data) =>
            {
                ResetButtonSpriteColor();
                UpdatePopup(data);
            };
        }
    }

    public void ResetButtonSpriteColor()
    {
        for (int i = 0; i < gimmickTutorialItemArray.Length; i++)
        {
            gimmickTutorialItemArray[i].SetButtonSprite(false);
        }
    }
}
