using UnityEngine;

public class UIItemSpaceTravelContinueItem : MonoBehaviour
{
    [SerializeField] private UILabel _labelRemainCount;
    [SerializeField] private UISprite _spriteRemainItem;
    [SerializeField] private UISprite _spriteBg;
    [SerializeField] private ManagerSpaceTravel.BonusItemType _type = ManagerSpaceTravel.BonusItemType.LINE_BOMB;

    #region UI컬러 관련
    private Color _activeFontColor = new Color(1f, 141f / 255f, 48f / 255f);
    private Color _activeEffectColor = new Color(192f / 255f, 56f / 255f, 3f / 255f);
    private Color _inActiveFontColorCode = new Color(141f / 255f, 141f / 255f, 141f / 255f);
    private Color _inActiveEffectColorCode = new Color(55f / 255f, 55f / 255f, 141f / 255f);
    private Color _inActiveSpriteColor = new Color(100f / 255f, 100f / 255f, 100f / 255f, 180f / 255f);
    #endregion

    public void InitItem()
    {
        var remainCount = ManagerSpaceTravel.instance.selectItemDic[_type];

        _labelRemainCount.text = remainCount.ToString();
        if (remainCount > 0)
        {
            _spriteBg.gameObject.SetActive(true);
            _spriteRemainItem.color = Color.white;
            _labelRemainCount.color = _activeFontColor;
            _labelRemainCount.effectColor = _activeEffectColor;
        }
        else
        {
            _spriteBg.gameObject.SetActive(false);
            _spriteRemainItem.color = _inActiveSpriteColor;
            _labelRemainCount.color = _inActiveFontColorCode;
            _labelRemainCount.effectColor = _inActiveEffectColorCode;
        }
    }
}
