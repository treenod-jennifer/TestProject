using DG.Tweening;
using UnityEngine;

/// <summary>
/// 상점에서 사용되는 연출 타입
/// </summary>
public class UIItemTabAnimation_Type1 : UIItemTabAnimation
{
    private abstract class OnOffItem
    {
        public abstract void Enabel();

        public abstract void Disabel();
    }

    [System.Serializable]
    private class OnOffItem_Widget : OnOffItem
    {
        [SerializeField] private UIWidget item;

        [SerializeField] private Color enabelColor;
        [SerializeField] private Color disabelColor;

        public override void Enabel()
        {
            item.color = enabelColor;
        }

        public override void Disabel()
        {
            item.color = disabelColor;
        }
    }

    [System.Serializable]
    private class OnOffItem_Label : OnOffItem
    {
        [SerializeField] private UILabel item;

        [SerializeField] private Color enabelColor;
        [SerializeField] private Color disabelColor;

        [SerializeField] private Color enabelEffectColor;
        [SerializeField] private Color disabelEffectColor;

        public override void Enabel()
        {
            item.color = enabelColor;
            
            if(item.effectStyle != UILabel.Effect.None)
            {
                item.effectColor = enabelEffectColor;
            }
        }

        public override void Disabel()
        {
            item.color = disabelColor;

            if (item.effectStyle != UILabel.Effect.None)
            {
                item.effectColor = disabelEffectColor;
            }
        }
    }

    [Header("color setting")]
    [SerializeField] private OnOffItem_Widget[] onOffWidgets;
    [SerializeField] private OnOffItem_Label[] onOffLabels;

    [Header("etc")]
    [SerializeField] protected Transform root;
    [SerializeField] protected UISprite tabBG;
    [SerializeField] protected GameObject saleTag;
    [SerializeField] protected GameObject disableIcon;

    public override void OnAnimation()
    {
        if(onOffWidgets != null)
        {
            foreach(var item in onOffWidgets)
            {
                item.Enabel();
            }
        }

        if(onOffLabels != null)
        {
            foreach(var item in onOffLabels)
            {
                item.Enabel();
            }
        }

        tabBG.spriteName = "diary_tap_01";
        tabBG.height = 98;
        saleTag.SetActive(false);
        root.DOScale(Vector3.one, 0.1f);
    }

    public override void OffAnimation()
    {
        if (onOffWidgets != null)
        {
            foreach (var item in onOffWidgets)
            {
                item.Disabel();
            }
        }

        if (onOffLabels != null)
        {
            foreach (var item in onOffLabels)
            {
                item.Disabel();
            }
        }

        tabBG.spriteName = "diary_tap_02";
        tabBG.height = 95;
        saleTag.SetActive(true);
        root.DOScale(Vector3.one * 0.8f, 0.1f);
    }

    public override void ActiveAnimation(bool isActive)
    {
        if (isActive)
        {
            disableIcon.SetActive(false);
            root.gameObject.SetActive(true);
            saleTag.SetActive(true);
        }
        else
        {
            OffAnimation();
            disableIcon.SetActive(true);
            root.gameObject.SetActive(false);
            saleTag.SetActive(false);
        }
    }
}
