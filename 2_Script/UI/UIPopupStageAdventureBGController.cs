using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureBGController : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.0f;

    private List<Transform> bgItems;
    [SerializeField] private int bgItemWidth;
    [SerializeField] private int screenWidth;

    private void Start()
    {
        bgItems = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
            bgItems.Add(transform.GetChild(i));
    }

    private void LateUpdate()
    {
        Vector3 movePosition = new Vector3(target.localPosition.x * speed, transform.localPosition.y, transform.localPosition.z);

        float distance = transform.localPosition.x - movePosition.x;
        if (Mathf.Abs(distance) <= Mathf.Epsilon)
            return;

        transform.localPosition = movePosition;

        #region outside chek
        bool leftMoving = distance > 0.0f;
        Transform targetItem = leftMoving ? bgItems[0] : bgItems[bgItems.Count - 1];
        bool outsideChecker = leftMoving ? LeftOutCheck(targetItem) : RightOutCheck(targetItem);

        if (outsideChecker)
        {
            targetItem.localPosition = 
                leftMoving ? 
                bgItems[bgItems.Count - 1].localPosition + Vector3.right * bgItemWidth : 
                bgItems[0].localPosition + Vector3.left * bgItemWidth;

            bgItems.Remove(targetItem);

            if (leftMoving)
                bgItems.Add(targetItem);
            else
                bgItems.Insert(0, targetItem);
        }
        #endregion
    }

    private bool LeftOutCheck(Transform bgItem)
    {
        float positionX = bgItem.localPosition.x + transform.localPosition.x;

        float halfScreenWidth = screenWidth * 0.5f;
        float halfBGItemWidth = bgItemWidth * 0.5f;

        return (halfScreenWidth + halfBGItemWidth) * -1.0f > positionX;
    }

    private bool RightOutCheck(Transform bgItem)
    {
        float positionX = bgItem.localPosition.x + transform.localPosition.x;

        float halfScreenWidth = screenWidth * 0.5f;
        float halfBGItemWidth = bgItemWidth * 0.5f;

        return (halfScreenWidth + halfBGItemWidth) < positionX;
    }
}
