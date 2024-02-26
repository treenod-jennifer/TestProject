using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco : MonoBehaviour
{
    static event System.Action<bool> BtnActiveEvent;

    [Header("ObjectLink")]
    [SerializeField] private UISprite sprBg;
    [SerializeField] private GameObject objSelectArrow;

    [HideInInspector] public int landColorIndex = 0;

    public DecoItemData decoItem;

    private void Start()
    {
        BtnActiveEvent += ActiveButton;
    }

    virtual public void UpdataData(DecoItemData cellData) { }

    public void SetDecoItemSpriteBg()
    {
        sprBg.spriteName = $"stage_box_{GetItemBg()}";
    }

    public void OnClickDecoItem()
    {
        if (UIDiaryDeco._instance != null && UIDiaryDeco._instance.GetScrollView().GetComponent<SpringPanel>() != null)
        {
            if (UIDiaryDeco._instance.GetScrollView().GetComponent<SpringPanel>().enabled)
                return;
        }

        bool IsOtherBtnClick = this != UIDiaryDeco.selectDecoItem && UIDiaryDeco.selectDecoItem != null;

        switch (UIDiaryDeco.state)
        {
            case UIDiaryDeco.SubScrollState.Idle:
                {
                    //서브 스크롤이 활성화 되지않은 상태에서 데코 아이템을 클릭했을 때
                    ActiveButton();
                    UIDiaryDeco.state = UIDiaryDeco.SubScrollState.Expansion;
                }
                break;
            case UIDiaryDeco.SubScrollState.Expansion:
                {
                    if (IsOtherBtnClick)
                    {
                        //서브 스크롤이 활성화가 되어 있을 때 다른 버튼을 클릭 할 때
                        BtnActiveEvent?.Invoke(false);
                    }
                    else
                    {
                        //서브 스크롤이 활성화가 되어 있을 때 클릭한 버튼을 다시 한번 클릭 할 때
                        ActiveButton(false);
                        UIDiaryDeco.state = UIDiaryDeco.SubScrollState.Idle;
                    }
                }
                break;
        }

        UIDiaryDeco._instance.OnClickEvent(this, IsOtherBtnClick);
    }

    public string GetItemBg()
    {
        return landColorIndex % 2 == 0 ? "02" : "03";
    }

    virtual protected void ActiveButton(bool IsActive = true)
    {
        this.gameObject.SetActive(true);

        sprBg.spriteName = $"stage_box_{(IsActive ? "01" : GetItemBg())}";
        objSelectArrow.SetActive(IsActive);
    }
    
    public void pActiveButton(bool IsActive)
    {
        ActiveButton(IsActive);
    }

    private void OnDestroy()
    {
        BtnActiveEvent = null;
    }
}