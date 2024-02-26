using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemTurnRelay_SelectItem : MonoBehaviour
{
    //체크박스 이미지
    [SerializeField] private UISprite spriteCheck;

    //아이템 타입
    [SerializeField] private ManagerTurnRelay.BONUSITEM_TYPE type = ManagerTurnRelay.BONUSITEM_TYPE.EVENTPOINT;

    //아이템 선택 콜백
    private System.Action<ManagerTurnRelay.BONUSITEM_TYPE> itemSelectAction = null;

    private bool isCanTouch = false;

    public void InitItem(System.Action<ManagerTurnRelay.BONUSITEM_TYPE> selectAction)
    {
        itemSelectAction = selectAction;
        spriteCheck.spriteName = "ready_button_02_off";
    }

    private void OnClickBtnItem()
    {
        //터치 불가능한 상태이거나, 아이템 선택 콜백이 등록되지 않은 상태에서는 선택 막음.
        if (isCanTouch == false || itemSelectAction == null)
            return;

        //체크박스 이미지 변경
        spriteCheck.spriteName = "ready_button_02_on";

        //아이템 선택 콜백 호출
        itemSelectAction?.Invoke(type);
    }

    public void SetIsCanTouchItem(bool bCanToucn)
    {
        this.isCanTouch = bCanToucn;
    }
}
