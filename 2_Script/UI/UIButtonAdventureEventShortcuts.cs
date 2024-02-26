using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonAdventureEventShortcuts : MonoBehaviour
{
    [SerializeField] private GameObject bubbleLeft;
    [SerializeField] private GameObject bubbleRight;

    [SerializeField] private UISprite attributeIcon;
    [SerializeField] private UIPanel scrollRoot;
    [SerializeField] private GameObject root;
    [SerializeField] private int targetOffsetX = 0;

    private Transform target;
    private int visibleRange;
    public int Attribute
    {
        set
        {
            if (attributeIcon != null)
            {
                attributeIcon.spriteName = "animal_attr_" + value.ToString();
            }
        }
    }

    private void Awake()
    {
        visibleRange = Mathf.RoundToInt(scrollRoot.GetViewSize().x * 0.5f);
    }

    public void Init(Transform target)
    {
        this.target = target;
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null)
        {
            return;
        }

        int posX = Mathf.RoundToInt(scrollRoot.transform.localPosition.x);
        int targetPosX = Mathf.RoundToInt(target.transform.localPosition.x + targetOffsetX) * -1;

        if (posX > targetPosX - visibleRange && posX < targetPosX + visibleRange)
        {
            Active(false);
        }
        else
        {
            Active(true);
        }
    }

    public void OnClickShortcutButton()
    {
        UIItemAdventureEventPanel._instance.SetScrollPos(Mathf.RoundToInt(target.transform.localPosition.x));
    }
    
    private void Active(bool trigger)
    {
        root.SetActive(trigger);

        if (!trigger) return;

        bool isLeft = target.position.x < 0.0f;

        bubbleLeft.SetActive(isLeft);
        bubbleRight.SetActive(!isLeft);

        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Abs(pos.x) * (isLeft ? -1.0f : 1.0f);
        transform.localPosition = pos;
    }
}
