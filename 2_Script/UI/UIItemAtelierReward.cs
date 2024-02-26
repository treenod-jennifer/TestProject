using System.Collections.Generic;
using UnityEngine;

public class UIItemAtelierReward : MonoBehaviour
{
    [SerializeField] private GenericReward _genericReward;

    //번들 처리 sprite
    [SerializeField] private List<UISprite> _sprAtelierList;
    [SerializeField] private UISprite       _spriteLight;

    private void OnDestroy()
    {
        foreach (var spr in _sprAtelierList)
            spr.atlas = null;
    }

    public void Init()
    {
        //번들 Atlas 세팅
        foreach (var sprite in _sprAtelierList)
        {
            sprite.atlas = ManagerAtelier.instance._atelierPack.AtlasUI;
        }
    }

    public void SetReward(Reward reward) => _genericReward.SetReward(reward);

    public void SetReceive(bool bReceived)
    {
        if (bReceived)
        {
            var color = new Color(0.6f, 0.6f, 0.6f);
            _genericReward.SetColor(color);
            _genericReward.SetTextColor(color);
            var colorAlpha = new Color(1, 1, 1, 0.5f);
            _spriteLight.color = colorAlpha;
        }
        else
        {
            _genericReward.SetColor(Color.white);
        }

        _genericReward.EnableCheck(bReceived);
    }
}