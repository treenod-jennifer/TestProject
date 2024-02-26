using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_CopyDecoItem : MonoBehaviour
{
    [Header("UIItemDecoLink")]
    [SerializeField] private UIItemDeco[] copyDecoItem;


    public void SetDecoDataPosition(Vector3 pos, DecoItemData decoData)
    {
        copyDecoItem[(int)UIDiaryDeco.tapType].transform.position = pos;
        copyDecoItem[(int)UIDiaryDeco.tapType].UpdataData(decoData);

        //복사된 아이템의 버튼 상태를 활성화된 상태로 변경
        copyDecoItem[(int)UIDiaryDeco.tapType].pActiveButton(true);
    }

    private void OnClickFrontButton()
    {
        UIDiaryDeco.selectDecoItem.gameObject.SetActive(true);
        UIDiaryDeco.selectDecoItem.OnClickDecoItem();

        copyDecoItem[(int)UIDiaryDeco.tapType].gameObject.SetActive(false);
    }

    public void InitCopyDecoItem()
    {
        for (int i = 0; i < copyDecoItem.Length; i++)
        {
            copyDecoItem[i].gameObject.SetActive(false);
        }
    }
}