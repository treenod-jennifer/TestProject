using UnityEngine;

public class UIItemSpaceTravelSelectItem : MonoBehaviour
{
    //체크박스 이미지
    [SerializeField] private UISprite _spriteCheck;
    [SerializeField] private UISprite _spriteItem;
    [SerializeField] private UILabel _addTurnLabel;
    [SerializeField] private UILabel _addTurnLabelS;

    //아이템 타입
    [SerializeField] private ManagerSpaceTravel.BonusItemType _itemType = ManagerSpaceTravel.BonusItemType.ADD_TURN;

    //아이템 선택 콜백
    private System.Action<ManagerSpaceTravel.BonusItemType> _itemSelectAction = null;

    private bool _isCanTouch = false;
    public void SetIsCanTouchItem(bool bCanTouch) => _isCanTouch = bCanTouch;

    public void InitItem(System.Action<ManagerSpaceTravel.BonusItemType> selectAction)
    {
        _itemSelectAction = selectAction;
        _spriteCheck.atlas = ManagerSpaceTravel.instance._spaceTravelPackIngame.Atlas;
        _spriteItem.atlas = ManagerSpaceTravel.instance._spaceTravelPackIngame.Atlas;
        _spriteCheck.spriteName = "ready_button_02_off";
        if (_itemType == ManagerSpaceTravel.BonusItemType.ADD_TURN && _addTurnLabel != null && _addTurnLabelS != null)
        {
            _addTurnLabel.text   = ManagerSpaceTravel.instance.addTurnCount.ToString();
            _addTurnLabelS.text = ManagerSpaceTravel.instance.addTurnCount.ToString();
        }
    }

    private void OnClickBtnItem()
    {
        //터치 불가능한 상태이거나, 아이템 선택 콜백이 등록되지 않은 상태에서는 선택 막음.
        if (_isCanTouch == false || _itemSelectAction == null)
        {
            return;
        }

        //체크박스 이미지 변경
        _spriteCheck.spriteName = "ready_button_02_on";

        //아이템 선택 콜백 호출
        _itemSelectAction?.Invoke(_itemType);
    }
}
